"""Twinleaf DG degausser over tio-python (tldevice).

Moved from IROP_research/scripts/degauss.py (TioDg and the status
helpers). The DG runs the cycle itself -- sine drive, soak, linear taper
to exactly zero -- from its OWN stored settings; this class writes the
settings, reads them back, triggers, streams the waveform and aborts.
The fail-closed readback gate and the QC live one layer up
(bfield.operations.degauss_cycle).

Bench-verified on DG R4 N78 (2026-08-12 no-coil, 2026-08-13 with
winding). Access that cannot be verified degrades to a recorded warning.

Hardware facts: firmware clamps out-of-range writes silently (9 A ->
8.0, 2 Hz -> 3.0); the drive is current-regulated, so saturation shows
as |Z| collapse; stream columns are named by unit (degauss.A / .V /
.Ohm / status).
"""

from __future__ import annotations

import contextlib
import itertools
import re

from bfield import clock
from bfield.constants import dg_cycle_plan_s
from bfield.errors import DgConnectError, DgError

DG_FREQ_HARD_HZ = (3.0, 60.0)       # the DG firmware refuses < 3 Hz
DG_SOAK_HARD_S = (0.0, 120.0)
DG_TAPER_HARD_S = (5.0, 600.0)
DG_OUTPUT_MAX_A = 10.0              # output stage (vendor page); fallback,
                                    # connect() adopts the unit's cap (8.0 A)
DG_OUTPUT_MAX_V = 15.0              # output stage; DG refuses trigger
                                    # when amp > this / |Z|
STREAM_POLL_ROWS = 400              # per read_rows drain, ~1 s of stream
                                    # (~348 row/s measured); caps poll stall

# phase codes read off hardware; soak/taper match commanded durations,
# ramp is state-dependent (1.6 s at 2 A to 12.6 s at 8 A), -3 = unit fault
DG_PHASE_NAMES = {0: "idle", 4: "measuring", 5: "ramping", 6: "soaking",
                  7: "taper", 8: "cooldown", -3: "fault"}
DG_STATUS_FAULT = -3
DG_PHASE_CODES = {"measuring": 4, "ramping": 5, "soaking": 6,
                  "running": 7, "cooldown": 8, "idle": 0}
WAVEFORM_KEYS = ("current", "frequency", "soaktime", "duration")


def status_is_idle(st):
    """True when a status reading means the output is idle.

    None is not idle (no reading is no proof); -3 fault is not idle
    (output still falling during the ~14 s fault decay)."""
    if st is None:
        return False
    return str(st).strip().lower() in ("0", "idle", "done", "none")


def fmt_status(st):
    """"7 (taper)" for known codes, str(st) otherwise."""
    try:
        name = DG_PHASE_NAMES.get(int(st))
    except (TypeError, ValueError):
        name = None
    return f"{st} ({name})" if name else str(st)


def build_from_desc(desc):
    """Bracketed build stamp from dev.desc(), e.g. "2023-07-14/4d4c6f".

    dev.firmware is a namespace on this unit, not a callable, so parse
    desc instead of probing (bench D-9)."""
    m = re.search(r"\[([^\]]+)\]", str(desc or ""))
    return m.group(1) if m else None


def rpc_name(entry):
    """The RPC name whether tio hands back a string or a dict.

    tio 3.8 lists rpcs as dicts; stringifying them blanked the status
    degauss.* line (bench D-6). Dicts stay whole in the session json."""
    if isinstance(entry, dict):
        return str(entry.get("name", ""))
    return str(entry)


def real_tio_backend(port):
    """The vendor device on the bench: tio-python's tldevice.Device."""
    try:
        import tldevice
    except Exception as e:
        raise DgConnectError(
            f"tio-python unavailable ({e}); run `pip install tio` (lab PC)")
    return tldevice.Device(port)


class TwinleafDG:
    """Thin adapter over tio-python.

    Warnings go into `warnings` (a list the session record owns) and to
    ctx.warn. `backend(port)` returns the vendor device.
    """

    name = "tio"

    def __init__(self, port, ctx, warnings, backend=None):
        self.port = port
        self.ctx = ctx
        self.warnings = warnings
        self.backend = backend or real_tio_backend
        self.dev = None
        self.columns = []
        self._t_trig = None
        self._plan_s = 0.0
        self._stream_warned = False
        self._status_dead = False
        self._stream_iter = None
        self._saw_active = False
        self.output_max_a = DG_OUTPUT_MAX_A

    def _warn(self, msg):
        self.ctx.warn(msg, self.warnings)

    def connect(self):
        try:
            self.dev = self.backend(self.port)
        except DgConnectError:
            raise
        except Exception as e:
            raise DgConnectError(
                f"could not open DG on {self.port!r}: {e} -- close DG "
                f"Control / other serial clients (the port is exclusive)")
        info = {"backend": "tio", "port": self.port}
        for attr in ("desc", "name", "serial"):
            with contextlib.suppress(Exception):
                v = getattr(self.dev.dev, attr)()
                info[attr] = str(v)
        build = build_from_desc(info.get("desc"))
        if build:
            info["build"] = build
        # adopt the unit's own cap: it silently clamped a 9 A write to
        # 8.0 on the bench (D-8); refuse up front, not at readback
        with contextlib.suppress(Exception):
            self.output_max_a = float(
                self._rpc("degauss.waveform.current.max"))
            info["current_max_a"] = self.output_max_a
        info["rpcs"] = self._rpc_dump()
        return info

    def _rpc_dump(self):
        for probe in ("_tio", "_routes", "rpcs"):
            with contextlib.suppress(Exception):
                node = getattr(self.dev, probe)
                names = getattr(node, "rpc_names", None) or getattr(
                    node, "rpcs", None)
                if names:
                    # entries may be dicts (tio 3.8): keep whole,
                    # sort by extracted name
                    return sorted(list(names), key=rpc_name)
        self._warn("RPC list not readable through this tio version; run the "
                   "L0 dump once (itio console)")
        return None

    def _rpc(self, dotted, *args):
        node = self.dev
        for part in dotted.split("."):
            node = getattr(node, part)
        return node(*args)

    def get_waveform(self):
        out = {}
        for key in WAVEFORM_KEYS:
            try:
                out[key] = float(self._rpc(f"degauss.waveform.{key}"))
            except Exception as e:
                self._warn(f"read-back of degauss.waveform.{key} failed ({e})")
                out[key] = None
        return out

    def set_waveform(self, wave):
        for key, value in wave.items():
            self._rpc(f"degauss.waveform.{key}", float(value))

    def trigger(self):
        wave = self.get_waveform()
        # no full readback -> no trigger: unknown durations collapse the
        # time plan to ~15 s = declared done mid-taper
        missing = [k for k in WAVEFORM_KEYS if wave.get(k) is None]
        if missing:
            raise DgError(
                "refusing to trigger without a full waveform readback "
                f"(missing: {', '.join(missing)}) -- completion timing and "
                "the unplug cue depend on it")
        self._plan_s = dg_cycle_plan_s(wave["soaktime"], wave["duration"])
        self._saw_active = False
        self._rpc("degauss.trigger")
        self._t_trig = clock.monotonic()

    def abort(self):
        # unacknowledged abort = output state unknown; never report it as sent
        try:
            self._rpc("degauss.abort")
            return True
        except Exception as e:
            self._warn(f"ABORT RPC FAILED ({e}); DG output state is UNKNOWN")
            return False

    def status(self):
        if self._status_dead:
            return None
        try:
            # stream source, not RPC. flush=False is load-bearing: the
            # default drains the shared queue, ~25% waveform loss
            v = self.dev.status(samples=1, flush=False)
            if isinstance(v, (list, tuple)):
                v = v[-1] if v else None
            if v is None:
                return None
            code = int(v)
            if not status_is_idle(code):
                self._saw_active = True
            return code
        except Exception as e:
            self._status_dead = True
            self._warn(f"status polling unavailable through this tio version "
                       f"({e}); falling back to the time plan")
            return None

    def read_rows(self, sink):
        """Append new stream rows into the caller-owned sink list.

        Caller-owned so rows drained before an interrupt survive (bench
        E-3). Drain bounded to STREAM_POLL_ROWS per call (D-5). The
        keep-alive write must raise; suppressed, the next read blocked
        forever."""
        if self._stream_warned:
            return
        try:
            if self._stream_iter is None:
                self._rpc("degauss.data.active", 1)
                for probe in ("data.iter", "data.stream_iter",
                              "stream_iter"):
                    with contextlib.suppress(Exception):
                        node = self.dev
                        for part in probe.split("."):
                            node = getattr(node, part)
                        self._stream_iter = node()
                        break
                if self._stream_iter is None:
                    raise RuntimeError("no known stream entry point")
                # one long-lived iterator: recreating with flush=True
                # discarded rows accumulated between calls (D-5)
                with contextlib.suppress(Exception):
                    self.columns = [
                        str(c) for c in self.dev.data.columnnames(False)]
            for row in itertools.islice(self._stream_iter,
                                        STREAM_POLL_ROWS):
                sink.append(tuple(row) if isinstance(row, (list, tuple))
                            else (row,))
            if sink and not self.columns:
                self.columns = [f"c{i}" for i in range(len(sink[0]))]
        except Exception as e:
            self._stream_warned = True
            self._warn(f"waveform stream capture unavailable through this "
                       f"tio version ({e}); the cycle still runs, QC will be "
                       f"absent")

    def done(self):
        st = self.status()
        elapsed_done = (self._t_trig is None
                        or clock.monotonic() - self._t_trig >= self._plan_s)
        if st is None:
            return elapsed_done
        if not status_is_idle(st):
            return False
        # idle counts only after the cycle was seen active: status can
        # still read 0 for a poll or two right after trigger
        return self._saw_active or elapsed_done

    def poll_sleep_s(self):
        return 0.5

    def close(self):
        with contextlib.suppress(Exception):
            self.dev.close()
