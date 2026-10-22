"""Record: one recorder child plus housekeeping, stopped safely.

Moved from qzfm_serial.py (child_command, record_run, guard_record, the
Poller threads, the map parser). The recorder runs as its own process
(bfield.acquisition.recorder); this side watches the heads while it
records and stops it -- stop file first, so it can write its meta.json
-- when a head goes bad, the operator aborts, or it overruns.

    rc, meta, pollers = record_run(ctx, lab, sensors, run_dir, rs,
                                   axes, factors, seconds=900)
"""

from __future__ import annotations

import contextlib
import json
import os
import subprocess
import tempfile
import threading
import traceback
from dataclasses import dataclass, field

from bfield import clock
from bfield.acquisition.child import (new_process_group_kwargs,
                                      recorder_argv, stop_child_ladder)
from bfield.channels import (label, parse_map, pins_of, restrict,
                             split_channel)
from bfield.constants import HOUSE_HEADER, HOUSE_LED_KEYS
from bfield.errors import (ConfigError, RecorderStuckError, SensorError,
                           Stopped, exit_meaning)
from bfield.io.files import read_meta

# max overrun before the parent stops the recorder: a child hung in a
# DAQ read holds the USB-6008 forever otherwise
CHILD_GRACE_S = 90.0
STOP_FILE_NAME = "STOP"
STOP_FILE_WAIT_S = 15.0
GUARD_SECONDS = 2.0            # pre-flight record
# two heads: the guard doubles as the sync check. 2 s = 0.5 Hz bins,
# too coarse for the 0.79 Hz split on this bench; 20 s = 0.05 Hz
GUARD_SECONDS_TWO_HEAD = 20.0
LIVE_TAIL_S = 1.0              # how often live samples go to the UI
CHILD_GRACE_REAL_S = 10.0      # the grace never shrinks below this in real
                               # time: at a high simulator speed the child's
                               # own interpreter start-up would eat it


def child_grace_s():
    """CHILD_GRACE_S in clock seconds, floored at CHILD_GRACE_REAL_S real."""
    return max(CHILD_GRACE_S, CHILD_GRACE_REAL_S * clock.speed())

# flags extra recorder args may not carry (value = what to use instead,
# "" = no substitute); --probe exits 0 having acquired nothing
QZFM_ARG_RESERVED = {
    "--outdir": "--outdir", "--stop-file": "", "--seconds": "--seconds",
    "--fs": "--fs", "--vrange": "--vrange", "--gain": "--gain",
    "--cal-v-per-nT": "--cal-v-per-nT", "--cal-factors": "",
    "--map": "--map", "--axes": "--axes", "--ids": "--device-name",
    "--sensor-id": "--device-name", "--probe": "", "--hardware-check": "",
    "--numbered": "", "--1head": "", "--2head": "", "--4head": "",
    # rail flags stay fenced: --no-rail-check would suppress the 30/31
    # exits the self-heal triggers on (regression guard)
    "--no-rail-check": "", "--rail-abort": "", "--rail-limit": "",
    "--lab-file": "", "--sim": "", "--speed": "", "--sensor-info": "",
}

@dataclass
class RecordSettings:
    """How the recorder child records (one set per session)."""
    map: str = "s1z:ai0,s1y:ai1,s2z:ai2,s2y:ai3"
    fs: float = 1200.0
    vrange: str = "2"
    gain: str = "1x"
    cal_v_per_nT: float | None = None
    notes: str | None = None
    poll_s: float = 2.0
    child_console: bool = False
    extra_args: list = field(default_factory=list)
    # which physical sensor is in which slot (bfield.sensors), stamped
    # into every meta.json as "sensor_info"
    sensor_info: list = field(default_factory=list)

    def check_extra_args(self):
        # appended last, so the child's argparse lets these win -- hence
        # the fence; matching relies on allow_abbrev=False
        for extra in self.extra_args or []:
            flag = str(extra).split("=", 1)[0].strip()
            alt = QZFM_ARG_RESERVED.get(flag)
            if flag in QZFM_ARG_RESERVED:
                raise ConfigError(
                    f"extra recorder arg {extra!r} is not passed on: {flag} "
                    "decides how this run is recorded"
                    + (f" -- use {alt} instead" if alt else
                       " -- not available from here"))


# ------------------------------------------------------------ the map
# parse_map (bfield.channels) is re-exported here: the sync check and the
# session import it from this module


def head_pins(map_spec, head, axes=None):
    """{axis: aiN} one sensor ("s1") is wired to in the map."""
    return pins_of(map_spec, head, axes)


def channel_factor_flag(map_spec, factors):
    """--cal-factors for the recorder: s1z:0.66,s2z:0.62 from the
    sensor-keyed factors {s1: {z: 0.66, ...}, ...}."""
    out = []
    for name, key, _phys in parse_map(map_spec):
        axis = split_channel(name)[1]
        f = (factors.get(key) or {}).get(axis)
        if f is not None:
            out.append(f"{name}:{f:g}")
    return ",".join(out)


def factors_to_flag(factors, axes=None):
    """'z:0.66,y:0.63' for one sensor's factors, restricted to the
    recorded axes (the Dual unit's Calib message includes x)."""
    if not factors:
        return None
    keys = [a for a in (axes or ("z", "y", "x")) if a in factors]
    if not keys:
        return None
    return ",".join(f"{a}:{factors[a]:g}" for a in keys)


def factors_summary(factors):
    """Sensor-keyed factors for a summary table."""
    if not factors:
        return ""
    if not all(isinstance(v, dict) for v in factors.values()):
        return factors_to_flag(factors) or ""
    if len(factors) == 1:
        return factors_to_flag(next(iter(factors.values()))) or ""
    return "  ".join(f"{label(h)}[{factors_to_flag(f) or '-'}]"
                     for h, f in sorted(factors.items()))


# ------------------------------------------------------- the child

def stop_file_path(run_dir):
    return os.path.join(run_dir, STOP_FILE_NAME) if run_dir else None


def child_command(rs, axes, run_dir, seconds, factors, rail_abort,
                  sensors, lab_file):
    """The recorder invocation (--numbered: channels named by sensor).

    `factors` is keyed by sensor. Every sensor is stamped with the
    serial its link actually opened, never a default -- otherwise every
    meta.json claims the child's default head.
    """
    sensors = sensors or {}
    keys = list(sensors) or [k for _n, k, _p in parse_map(rs.map)][:1]
    chan_map = restrict(rs.map, keys)
    # only the axes the sensors are in (mode z: no y channel)
    chan_map = ",".join(
        f"{n}:{p}" for n, _k, p in parse_map(chan_map)
        if axes is None or split_channel(n)[1] in axes)
    cmd = [*recorder_argv(), "--numbered", "--map", chan_map]
    ids = ",".join(f"{k}:{s.serial_id}" for k, s in sensors.items()
                   if getattr(s, "serial_id", None))
    if ids:
        cmd += ["--ids", ids]
    cmd += ["--fs", f"{rs.fs:g}", "--vrange", str(rs.vrange),
            "--gain", rs.gain,
            "--seconds", f"{seconds:g}", f"--outdir={run_dir}"]
    flag = channel_factor_flag(chan_map, factors or {})
    # operator-asserted V/nT goes to every topology or none, so the
    # banner and the record cannot disagree
    if rs.cal_v_per_nT is not None:
        cmd += ["--cal-v-per-nT", f"{rs.cal_v_per_nT:g}"]
    if flag:
        cmd += ["--cal-factors", flag]
    cmd += ["--stop-file", stop_file_path(run_dir)]
    if rs.notes:
        cmd.append(f"--notes={rs.notes}")
    if rail_abort:
        cmd.append("--rail-abort")
    cmd += ["--lab-file", lab_file]
    info = [r for r in (rs.sensor_info or []) if r.get("sensor") in sensors] \
        if sensors else list(rs.sensor_info or [])
    if info:
        cmd += ["--sensor-info", json.dumps(info, separators=(",", ":"))]
    cmd += list(rs.extra_args or [])
    return cmd
def write_lab_file(lab):
    """The Lab the child must rebuild, in a temp file (not in the data)."""
    fd, path = tempfile.mkstemp(prefix="bfield_lab_", suffix=".json")
    with os.fdopen(fd, "w", encoding="utf-8") as fh:
        json.dump(lab.job_block(), fh)
    return path


def popen_child(ctx, cmd, run_dir, rs):
    """Start the recorder, its console captured into child.log.

    CREATE_NEW_PROCESS_GROUP is what makes a graceful stop possible on
    Windows; plain terminate() never lets the child write meta.json.
    """
    kw = dict(new_process_group_kwargs())
    log_fh = None
    if not rs.child_console:
        # no encoding= on purpose: only the child writes through this
        # handle, its bytes land in the file as emitted
        log_fh = open(os.path.join(run_dir, "child.log"), "w", newline="\n")
        kw["stdout"] = log_fh
        kw["stderr"] = subprocess.STDOUT
    # the sensor list is in the session json; the log line stays short
    shown = [("<sensor list>" if i and str(cmd[i - 1]) == "--sensor-info"
              else str(c)) for i, c in enumerate(cmd)]
    ctx.say("run", "recorder: " + " ".join(shown))
    return subprocess.Popen([str(c) for c in cmd], **kw), log_fh


def stop_child(ctx, proc, stop_file=None):
    """Graceful stop first so the child can write its meta, then escalate.

    The stop-FILE rung leads: CTRL_BREAK_EVENT killed the recorder
    before its finally could write meta.json (measured 2026-08-04).
    Invariant: returning means the child is dead -- a survivor holds the
    DAQ, so "alive" raises, never returns.
    """
    how = stop_child_ladder(proc, stop_file=stop_file,
                            stop_file_wait_s=STOP_FILE_WAIT_S,
                            signal_wait_s=15.0, term_wait_s=5.0,
                            kill_wait_s=5.0, warn=ctx.warn)
    if how == "forced":
        ctx.warn("the recorder had to be force-killed -- its cleanup did "
                 "not run, so its meta.json may be missing")
    if how == "alive":
        raise RecorderStuckError(
            f"the recorder (pid {proc.pid}) survived the stop file, "
            f"CTRL_BREAK/SIGINT, terminate and kill -- it still holds "
            f"the DAQ device. End it by hand (Task Manager / kill) "
            f"before the next run")


# ------------------------------------------------------ housekeeping

def house_path(run_dir, sensors, head):
    """house_S1.csv: one per sensor, named by its number."""
    return os.path.join(run_dir, f"house_{label(head)}.csv")


class Poller(threading.Thread):
    """Compensation fields, cell temp and LEDs to house.csv during a
    record; every sample also feeds the abort criteria."""

    def __init__(self, sensor, path, poll_s, tracker):
        super().__init__(daemon=True)
        self.sensor = sensor
        self.path = path
        self.poll_s = poll_s
        self.tracker = tracker
        self.stop_flag = threading.Event()
        self.error = None
        self.error_tb = None
        self.abort_reason = None
        self.rows = 0
        self.last = None             # the latest frame, for a GUI

    def run(self):
        t0 = clock.monotonic()
        # sleep to a deadline: house() is a ~1.9 s round trip, so a
        # fixed nap made poll_s 2 sample every 3.9 s
        due = clock.monotonic()
        try:
            with open(self.path, "w", newline="\n", encoding="utf-8") as f:
                f.write(HOUSE_HEADER + "\n")
                while True:
                    due += self.poll_s
                    if clock.wait(self.stop_flag,
                                  max(0.0, due - clock.monotonic())):
                        return
                    if self.sensor.dead:
                        self.abort_reason = "serial link dead"
                        return
                    snap = self.sensor.house()
                    self.last = snap
                    leds = "".join("1" if snap[k] else "0"
                                   for k in HOUSE_LED_KEYS)
                    f.write(f"{clock.iso_now()},{clock.monotonic() - t0:.1f},"
                            f"{snap['B0_pT']:.1f},{snap['By_pT']:.1f},"
                            f"{snap['Bz_pT']:.1f},{snap['cellT_err']:g},"
                            f"{leds}\n")
                    f.flush()
                    self.rows += 1
                    if self.tracker.check(snap):
                        self.abort_reason = self.tracker.reason
                        return
        except Exception as e:  # re-raised by the caller after the record
            self.error = e
            self.error_tb = traceback.format_exc()
            # also stop the record: a failure here sets neither `dead`
            # nor an abort, and the poller would die in silence
            self.abort_reason = (f"housekeeping stopped ({type(e).__name__}: "
                                 f"{e}) -- the sensor is no longer watched")

    def stop(self):
        self.stop_flag.set()
        self.join(timeout=10.0)


class Pollers:
    """One housekeeping poller per head, reported as one. Either head
    going bad stops the record."""

    def __init__(self, sensors, run_dir, poll_s, trackers):
        self.items = {h: Poller(s, house_path(run_dir, sensors, h), poll_s,
                                trackers[h])
                      for h, s in sensors.items()}

    def start(self):
        for p in self.items.values():
            p.start()

    def stop(self):
        for p in self.items.values():
            p.stop()

    @property
    def abort_reason(self):
        for head, p in self.items.items():
            if p.abort_reason:
                return (p.abort_reason if len(self.items) == 1
                        else f"{getattr(p.sensor, 'display', head)}: "
                             f"{p.abort_reason}")
        return None

    @property
    def rows(self):
        return max((p.rows for p in self.items.values()), default=0)

    def first_error(self):
        for head, p in self.items.items():
            if p.error is not None:
                return head, p
        return None, None


class RawTail:
    """Follow the child's raw CSV and hand new samples to the UI."""

    def __init__(self, ctx, run_dir, fs):
        self.ctx = ctx
        self.path = os.path.join(
            run_dir, f"{os.path.basename(os.path.normpath(run_dir))}_raw.csv")
        self.fs = fs
        self.pos = 0
        self.names = None
        self.next_t = 0.0

    def poll(self, final=False):
        """New whole lines -> ctx.data. Rate-limited to LIVE_TAIL_S, except
        the final read after the recorder has exited (or its last second
        never reaches the UI)."""
        now = clock.monotonic()
        if now < self.next_t and not final:
            return
        self.next_t = now + LIVE_TAIL_S
        try:
            with open(self.path, encoding="utf-8") as fh:
                fh.seek(self.pos)
                chunk = fh.read()
        except OSError:
            return
        # only whole lines; a half-written one waits for the next poll
        cut = chunk.rfind("\n")
        if cut < 0:
            return
        self.pos += len(chunk[:cut + 1].encode("utf-8"))
        cols = {}
        for line in chunk[:cut].splitlines():
            if line.startswith("#"):
                continue
            if line.startswith("sample,"):
                self.names = line.split(",")[1:]
                continue
            if self.names is None:
                continue
            parts = line.split(",")[1:]
            for name, v in zip(self.names, parts):
                with contextlib.suppress(ValueError):
                    cols.setdefault(name, []).append(float(v))
        for name, vals in cols.items():
            self.ctx.data(name, vals, self.fs)


def record_run(ctx, lab, sensors, run_dir, rs, axes, factors, seconds,
               rail_abort=False, trackers=None):
    """One record with housekeeping. Returns (rc, meta, pollers)."""
    from bfield.devices.qzfm import HealthTracker
    trackers = trackers or {h: HealthTracker() for h in sensors}
    os.makedirs(run_dir, exist_ok=True)
    lab_file = write_lab_file(lab)
    cmd = child_command(rs, axes, run_dir, seconds, factors, rail_abort,
                        sensors, lab_file)
    pollers = Pollers(sensors, run_dir, rs.poll_s, trackers)
    tail = (RawTail(ctx, run_dir, rs.fs)
            if getattr(ctx.ui, "wants_live_data", False) else None)
    proc, log_fh = popen_child(ctx, cmd, run_dir, rs)
    started = clock.monotonic()
    grace = child_grace_s()
    deadline = started + seconds + grace if seconds > 0 else None
    pollers.start()
    stopped = None
    try:
        while True:
            try:
                rc = proc.wait(timeout=clock.real_seconds(0.5))
                break
            except subprocess.TimeoutExpired:
                pass
            if tail is not None:
                tail.poll()
            if seconds > 0:
                ctx.progress(min((clock.monotonic() - started) / seconds,
                                 1.0), "recording")
            dead = [h for h, s in sensors.items() if s.dead]
            if ctx.stop_requested():
                ctx.say("run", "stopping the recorder: stopped by the "
                               "operator")
                stop_child(ctx, proc, stop_file_path(run_dir))
                rc = proc.poll()
                stopped = Stopped("stopped by the operator during a record")
                break
            if pollers.abort_reason or dead:
                ctx.say("run", "stopping the recorder: "
                        + (pollers.abort_reason
                           or f"serial link dead ({', '.join(dead)})"))
                stop_child(ctx, proc, stop_file_path(run_dir))
                rc = proc.poll()
                break
            if deadline is not None and clock.monotonic() > deadline:
                # child overran its deadline; nothing else in this loop
                # can end it, and it holds the DAQ until something does
                ctx.say("run", f"the recorder is {grace:g} s past "
                               f"its {seconds:g} s duration and has not "
                               "exited -- stopping it")
                stop_child(ctx, proc, stop_file_path(run_dir))
                rc = proc.poll()
                break
    except KeyboardInterrupt:
        # give the child time to close its own meta before escalating
        try:
            proc.wait(timeout=15.0)
        except subprocess.TimeoutExpired:
            stop_child(ctx, proc, stop_file_path(run_dir))
        raise
    except BaseException:
        # a poller failure, a bug here, anything: the child must not
        # outlive this function holding the USB-6008
        with contextlib.suppress(Exception):
            stop_child(ctx, proc, stop_file_path(run_dir))
        raise
    finally:
        pollers.stop()
        if log_fh is not None:
            with contextlib.suppress(Exception):
                log_fh.close()
        with contextlib.suppress(OSError):
            os.remove(lab_file)
    if tail is not None:
        tail.poll(final=True)
    if rc is None:
        # unreachable under stop_child's contract, but a leaked None
        # used to be laundered into exit 0 (regression guard)
        raise RecorderStuckError(
            "the recorder's exit code is unknown (the process may still "
            "be running) -- refusing to report this run as a success")
    ctx.say("run", f"recorder exit {rc} ({exit_meaning(rc)}) after "
                   f"{clock.monotonic() - started:.0f} s")
    # for a UI: where the record is (the Analysis tab's "Last record")
    ctx.status("record", run_dir=run_dir, rc=rc)
    if stopped is not None:
        raise stopped
    head, bad = pollers.first_error()
    if bad is not None:
        raise SensorError(
            f"housekeeping failed during the run "
            f"({getattr(bad.sensor, 'display', head)}): "
            f"{type(bad.error).__name__}: {bad.error}"
            + (f"\n{bad.error_tb}" if bad.error_tb else ""))
    return rc, read_meta(run_dir), pollers


def guard_seconds(sensors):
    """Two or more sensors spend longer: the guard is also the sync
    check, which needs bins fine enough to split two carriers."""
    return GUARD_SECONDS_TWO_HEAD if len(sensors) > 1 else GUARD_SECONDS


def guard_record(ctx, lab, sensors, rs, axes, factors, outdir, number):
    """The pre-flight gate: a short guarded record whose exit code and
    meta are machine-readable (unlike a probe, which always exits 0)."""
    gdir = os.path.join(outdir, "_guard", f"g{number:03d}")
    os.makedirs(gdir, exist_ok=True)
    gsec = guard_seconds(sensors)
    lab_file = write_lab_file(lab)
    cmd = child_command(rs, axes, gdir, gsec, factors, rail_abort=True,
                        sensors=sensors, lab_file=lab_file)
    proc, log_fh = popen_child(ctx, cmd, gdir, rs)
    try:
        # bounded like record_run: an unbounded wait() here hangs the
        # whole setup on a child that never returns, with the DAQ held
        try:
            rc = proc.wait(timeout=clock.real_seconds(gsec + child_grace_s()))
        except subprocess.TimeoutExpired:
            ctx.say("guard", f"the guard record is {child_grace_s():g} s past "
                             f"its {gsec:g} s and has not exited -- "
                             "stopping it")
            stop_child(ctx, proc, stop_file_path(gdir))
            rc = proc.poll()
    except BaseException:
        # own process group: console Ctrl-C never reaches the child on
        # Windows. BaseException so no abnormal exit orphans it
        try:
            proc.wait(timeout=15.0)
        except subprocess.TimeoutExpired:
            stop_child(ctx, proc, stop_file_path(gdir))
        raise
    finally:
        if log_fh is not None:
            with contextlib.suppress(Exception):
                log_fh.close()
        with contextlib.suppress(OSError):
            os.remove(lab_file)
    return rc, read_meta(gdir), gdir
