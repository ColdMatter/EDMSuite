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
import re
import subprocess
import tempfile
import threading
import traceback
from dataclasses import dataclass, field

from bfield import clock
from bfield.acquisition.child import (new_process_group_kwargs,
                                      recorder_argv, stop_child_ladder)
from bfield.constants import (DAQ_DIFF_AI, HEADS, HOUSE_HEADER,
                              HOUSE_LED_KEYS, MAP_AXIS_RX)
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
    # rail flags stay fenced: --no-rail-check would suppress the 30/31
    # exits the self-heal triggers on (regression guard)
    "--no-rail-check": "", "--rail-abort": "", "--rail-limit": "",
    "--lab-file": "", "--sim": "", "--speed": "", "--sensor-info": "",
}

# the recorder's default wiring; a one-head command naming exactly
# these pins omits --map, keeping the single-head argv unchanged
ANALOG_DEFAULT_PINS = {"z": "ai0", "y": "ai1"}


@dataclass
class RecordSettings:
    """How the recorder child records (one set per session)."""
    map: str = "z:a:ai0,y:a:ai1,z2:b:ai2,y2:b:ai3"
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

def parse_map(spec):
    """'z:a:ai0,y:a:ai1,...' -> ordered [(channel, head, phys)].

    Must refuse the same things the recorder does: a laxer parent sends
    maps the child rejects with exit 13, three minutes of bring-up later.
    """
    def require(ok, msg):
        if not ok:
            raise ConfigError(msg)

    entries = []
    for item in (spec or "").split(","):
        item = item.strip()
        require(bool(item),
                f"the map has an empty entry (stray comma?) in {spec!r}")
        parts = [p.strip() for p in item.split(":")]
        require(len(parts) == 3 and all(parts),
                f"bad map entry {item!r} (want axis:head:aiN, e.g. z:a:ai0)")
        name, head, phys = (p.lower() for p in parts)
        require(re.fullmatch(MAP_AXIS_RX, name),
                f"bad axis name {name!r} in the map (x, y, z, or x2/y2/z2 = "
                f"second reading on the other head)")
        require(head in HEADS,
                f"bad head {head!r} in the map (two heads, a and b)")
        require(re.fullmatch(DAQ_DIFF_AI, phys),
                f"bad channel {phys!r} in the map; the USB-6008 has four "
                f"differential inputs, ai0..ai3")
        entries.append((name, head, phys))
    require(bool(entries), "the map is empty")
    names = [e[0] for e in entries]
    require(len(set(names)) == len(names), "the map repeats a channel name")
    pins = [e[2] for e in entries]
    dupes = sorted({p for p in pins if pins.count(p) > 1})
    require(not dupes,
            f"the map assigns {', '.join(dupes)} to more than one channel; "
            f"the USB-6008 has four differential inputs, ai0..ai3")
    for head in HEADS:
        n = sum(1 for e in entries if e[1] == head)
        require(n <= 2,
                f"the map gives head {head} {n} channels; a dual-axis QZFM "
                f"has only two analog outputs")
    for name, _h, _p in entries:
        if name.endswith("2"):
            require(name[:-1] in names,
                    f"the map has {name!r} without {name[:-1]!r} -- '2' "
                    f"means a second reading of that axis, record the first")
    return entries


def head_pins(map_spec, head, axes=None):
    """{axis: aiN} one head is wired to, the '2' suffix stripped."""
    out = {}
    for name, h, phys in parse_map(map_spec):
        if h == head:
            out[name.rstrip("0123456789") or name] = phys
    if axes is None:
        return out
    return {ax: out[ax] for ax in axes if ax in out}


def channel_factor_flag(map_spec, factors):
    """--cal-factors for the 4-channel child (z/y/z2/y2 names)."""
    out = []
    for name, head, _phys in parse_map(map_spec):
        axis = name.rstrip("0123456789") or name
        f = (factors.get(head) or {}).get(axis)
        if f is not None:
            out.append(f"{name}:{f:g}")
    return ",".join(out)


def factors_to_flag(factors, axes=None):
    """--cal-factors value, restricted to the recorded axes (the Dual
    unit's Calib message includes x, which the recorder refuses)."""
    if not factors:
        return None
    keys = [a for a in (axes or ("z", "y", "x")) if a in factors]
    if not keys:
        return None
    return ",".join(f"{a}:{factors[a]:g}" for a in keys)


def factors_summary(factors):
    """Head-keyed factors for a summary table."""
    if not factors:
        return ""
    if not all(isinstance(v, dict) for v in factors.values()):
        return factors_to_flag(factors) or ""
    if len(factors) == 1:
        return factors_to_flag(next(iter(factors.values()))) or ""
    return "  ".join(f"{h}[{factors_to_flag(f) or '-'}]"
                     for h, f in sorted(factors.items()))


# ------------------------------------------------------- the child

def stop_file_path(run_dir):
    return os.path.join(run_dir, STOP_FILE_NAME) if run_dir else None


def child_command(rs, axes, run_dir, seconds, factors, rail_abort,
                  sensors, lab_file):
    """The recorder invocation: --1head or --2head.

    `factors` is keyed by head. Both shapes are stamped with the serial
    the link actually opened, never a default -- otherwise every
    meta.json claims the child's default head.
    """
    sensors = sensors or {}
    if len(sensors) > 1:
        cmd = [*recorder_argv(), "--2head",
               "--map", rs.map,
               "--ids", ",".join(f"{h}:{s.serial_id}"
                                 for h, s in sensors.items()),
               "--fs", f"{rs.fs:g}", "--vrange", str(rs.vrange),
               "--gain", rs.gain,
               "--seconds", f"{seconds:g}", f"--outdir={run_dir}"]
        flag = channel_factor_flag(rs.map, factors)
    else:
        head = next(iter(sensors)) if sensors else "a"
        sensor = sensors.get(head)
        cmd = [*recorder_argv(), "--1head",
               "--axes", ",".join(axes),
               "--fs", f"{rs.fs:g}", "--vrange", str(rs.vrange),
               "--gain", rs.gain,
               "--seconds", f"{seconds:g}", f"--outdir={run_dir}"]
        # pins for the head actually opened: head b alone would
        # otherwise get the recorder's default (a's pins) under b's serial
        pins = head_pins(rs.map, head, axes)
        if any(ANALOG_DEFAULT_PINS.get(ax) != p for ax, p in pins.items()):
            cmd += ["--map", ",".join(f"{ax}:{p}" for ax, p in pins.items())]
        if sensor is not None and sensor.serial_id:
            cmd += ["--sensor-id", sensor.serial_id]
        flag = factors_to_flag((factors or {}).get(head) or {}, axes)
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
    info = [r for r in (rs.sensor_info or []) if r.get("head") in sensors] \
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
    """house.csv for one head, house_a/house_b.csv for two."""
    if len(sensors) > 1:
        return os.path.join(run_dir, f"house_{head}.csv")
    return os.path.join(run_dir, "house.csv")


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
    """Two heads spend longer: the guard is also the sync check, which
    needs bins fine enough to split two carriers."""
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
