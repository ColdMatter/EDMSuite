"""A QZFM session: status, setup, zero, one record, stream, standby.

Moved from qzfm_serial.py (the mode bodies, the session harness, the
command-line validation). One session = one qzfm_session.json ledger
(checkpointed atomically) + one operations log, written on every exit
path, ports closed on every exit path.

    plan = QZFMPlan.for_sensors(config, [1, 2], seconds=900)
    code = QZFMSession(ctx, lab, plan).record()

Sensors are keyed by number everywhere ("s1", "s2"; channels "s1z"...,
bfield.channels).

Every exit path leaves the sensor running except standby(), which
reboots it dark; wake it with setup().
"""

from __future__ import annotations

import dataclasses
import math
import os
import traceback
from dataclasses import dataclass, field

from bfield import clock
from bfield.channels import KEY_RX, key_number, split_channel
from bfield.constants import (COMMON_VERSION, DAQ_AGGREGATE_MAX, GAIN_TABLE,
                              RECORDABLE_AXES, VALID_VRANGE)
from bfield.errors import (BFieldError, ConfigError, SensorAbort,
                           SessionStop, Stopped, exit_meaning)
from bfield.heads import same_head
from bfield.io.files import (alloc_run_dir, rel_to, sanitize, unique_outdir,
                             write_json_atomic)
from bfield.operations import sensor_setup as ops
from bfield.operations.record import (RecordSettings, head_pins, parse_map,
                                      record_run)
from bfield.sensors import (Selection, default_map, display_of,
                            resolve_device_name)

DEVICE_NAME_EXAMPLE_TWO = "s1:A10QHMQMA,s2:A10QHJ1ZA"
# exit 0 with no readable meta.json: samples exist, provenance does not
NO_META_EXIT = 40
MODES = ("status", "setup", "zero", "run", "stream", "standby")


@dataclass
class QZFMPlan:
    """Everything a session needs; the old qzfm_serial flags."""
    device_name: str = ""            # s1:SERIAL,s2:SERIAL
    axes: str | None = None          # None = z for one sensor, z,y for more
    gain: str = "1x"
    fs: float = 1200.0
    vrange: str = "2"
    map: str = "s1z:ai0,s1y:ai1,s2z:ai2,s2y:ai3"
    cal_v_per_nT: float | None = None
    seconds: float = 900.0
    zero_cond: float = 100.0
    lock_timeout: float = 600.0
    poll_s: float = 2.0
    outdir: str | None = None
    # the run's name in the dated layout (<NNN>_<name>); None = automatic,
    # e.g. SingleSensorZ, TwoSensorsZY
    name: str | None = None
    notes: str | None = None
    extra_args: list = field(default_factory=list)
    require_factors: bool = False
    force_start: bool = False
    log: str | None = None
    child_console: bool = False
    sync: str | None = None
    zero_shift_warn: float = 200.0
    stream_head: str | None = None   # the sensor streamed ("s2"); None =
                                     # the first
    stream_seconds: float | None = None
    # the physical sensors (bfield.sensors.Selection.info()); empty when
    # serials were given directly -- the session then looks them up
    sensor_info: list = field(default_factory=list)
    # the modulation master with two or more sensors: its key ("s2");
    # None = the first
    master_head: str | None = None

    @classmethod
    def from_config(cls, config, **overrides):
        """Defaults from config/lab.toml, then the caller's overrides.
        Sensors given by serial (device_name) are looked up in the sensor
        list and keyed by their numbers, as for_sensors."""
        device = overrides.get("device_name")
        if device and not overrides.get("sensor_info"):
            numbers = resolve_device_name(config, device)
            kw = {k: v for k, v in overrides.items() if k != "device_name"}
            return cls.for_sensors(config, numbers, **kw)
        return cls._defaults(config, **overrides)

    @classmethod
    def _defaults(cls, config, **overrides):
        q = (config.section("qzfm").get("defaults") or {})
        d = config.section("daq")
        base = dict(gain=q.get("gain", "1x"),
                    zero_cond=q.get("zero_cond_pT_per_s", 100.0),
                    lock_timeout=q.get("lock_timeout_s", 600.0),
                    poll_s=q.get("poll_s", 2.0),
                    zero_shift_warn=q.get("zero_shift_warn_pT", 200.0),
                    fs=d.get("fs_hz", 1200.0),
                    vrange=f"{d.get('vrange_V', 2.0):g}",
                    map=default_map(config))
        base.update({k: v for k, v in overrides.items() if v is not None})
        return cls(**base)

    @classmethod
    def for_sensors(cls, config, numbers, master=None, **overrides):
        """A plan for sensors chosen by number (config [[sensors]]);
        master = the number of the modulation master (default: first);
        axes (an override) = what to record (default: Z for one sensor,
        Z and Y for two)."""
        sel = Selection(config, numbers, master=master,
                        axes=overrides.pop("axes", None))
        info = sel.info()
        explicit_map = overrides.get("map")
        if explicit_map:
            # a hand-written map: the records say what it records
            for rec in info:
                rec["inputs"] = head_pins(explicit_map, rec["sensor"])
                rec["channels"] = [n for n, k, _p in parse_map(explicit_map)
                                   if k == rec["sensor"]]
        kw = dict(device_name=sel.device_name, map=sel.map,
                  axes=sel.axes, sensor_info=info,
                  master_head=sel.master_head)
        # an explicit override (e.g. a hand-written map) still wins
        kw.update({k: v for k, v in overrides.items() if v is not None})
        return cls._defaults(config, **kw)

    def record_settings(self):
        return RecordSettings(map=self.map, fs=self.fs, vrange=self.vrange,
                              gain=self.gain, cal_v_per_nT=self.cal_v_per_nT,
                              notes=self.notes, poll_s=self.poll_s,
                              child_console=self.child_console,
                              extra_args=list(self.extra_args or []),
                              sensor_info=list(self.sensor_info or []))

    def setup_settings(self, heads):
        return ops.SetupSettings(
            axes=parse_axes(self.axes or default_axes(len(heads))),
            gain=self.gain, force_start=self.force_start,
            require_factors=self.require_factors,
            zero_shift_warn=self.zero_shift_warn, sync=self.sync,
            master_head=self.master_head,
            lock_timeout=self.lock_timeout, outdir=self.outdir,
            rs=self.record_settings())


# ---------------------------------------------------------- validation

def require(ok, msg):
    if not ok:
        raise ConfigError(msg)


def parse_heads(spec):
    """'s1:SERIAL,s2:SERIAL' -> {key: serial}, in that order."""
    text = (spec or "").strip()
    require(text != "", "the device name is required")
    heads = {}
    for item in text.split(","):
        key, sep, serial = (s.strip() for s in item.partition(":"))
        key = key.lower()
        require(sep and KEY_RX.fullmatch(key) and serial,
                f"bad device-name entry {item!r} (want sensor:SERIAL, e.g. "
                f"{DEVICE_NAME_EXAMPLE_TWO}; QZFMPlan.from_config turns plain "
                "serials into this)")
        require(key not in heads, f"sensor {key_number(key)} named twice")
        heads[key] = serial
    # one sensor named twice (FTDI serial plus its QuSpin name) fails as
    # a busy port on the second connect -- refuse it here instead
    items = list(heads.items())
    for i, (ka, a) in enumerate(items):
        for kb, b in items[i + 1:]:
            require(not same_head(a, b),
                    f"the device name names one sensor twice: {a!r} and "
                    f"{b!r} are the same head (an FTDI serial and its QuSpin "
                    "name count as one)")
    return heads


NUMBER_WORDS = {1: "Single", 2: "Two", 3: "Three", 4: "Four"}


def run_name(plan, mode, n_heads):
    """The automatic run name: SingleSensorZ, TwoSensorsZY, ..."""
    if mode == "stream":
        return "DigitalStream"
    axes = "".join(a.upper() for a in
                   parse_axes(plan.axes or default_axes(n_heads)))
    word = NUMBER_WORDS.get(n_heads, str(n_heads))
    return f"{word}Sensor{'s' if n_heads > 1 else ''}{axes}"


def default_axes(n_heads):
    """One sensor records Z; two or more record Z and Y each."""
    return "z,y" if n_heads > 1 else "z"
def parse_axes(spec):
    axes = [a.strip().lower() for a in str(spec).split(",") if a.strip()]
    if (not axes or any(a not in RECORDABLE_AXES for a in axes)
            or len(set(axes)) != len(axes)):
        raise ConfigError("axes: comma list from z,y with no repeats "
                          "(the Dual unit cannot record x)")
    return axes


def validate(plan, mode):
    """Refuse a bad plan before any port opens (the old --validate-only)."""
    require(mode in MODES, f"unknown mode {mode!r}")
    # no default on purpose: both heads' serials start "A10QH", and the
    # wrong pick silently stamps the wrong serial into every meta.json
    require(bool(plan.device_name and plan.device_name.strip()),
            "the device name is required, e.g. A10QHMQMA (the FTDI serial; "
            "a bare 'A10QH' matches both heads on this bench), or "
            f"{DEVICE_NAME_EXAMPLE_TWO} for two")
    heads = parse_heads(plan.device_name)
    if plan.axes is None:
        plan.axes = default_axes(len(heads))
    want = parse_axes(plan.axes)
    entries = parse_map(plan.map)
    mapped = {k for _n, k, _p in entries}
    require(mapped == set(heads),
            f"the map covers sensors {sorted(mapped)} but the device name "
            f"names {sorted(heads)} -- they have to agree, or the recorder "
            "stamps the wrong serial on a channel")
    if len(heads) == 1:
        head = next(iter(heads))
        named = head_pins(plan.map, head)
        missing = sorted(set(want) - set(named))
        require(not missing,
                f"the map gives sensor {key_number(head)} no channel for "
                f"{missing} -- the axes ask for {sorted(want)}, so every one "
                "of them needs an input")
    else:
        base = {split_channel(n)[1] for n, _k, _p in entries}
        extra = sorted(base - set(want))
        require(not extra,
                f"the map records {extra} but the axes only put the sensors "
                f"into {sorted(want)} mode -- those channels would not be "
                f"measurements. Add them to the axes, or drop them from "
                f"the map")
        require(plan.zero_shift_warn > 0, "zero_shift_warn must be positive")
    if mode == "stream":
        require(plan.stream_head is None or plan.stream_head in heads,
                f"stream sensor {plan.stream_head} is not one of the sensors "
                f"of this run ({', '.join(heads)})")
    try:
        v = float(plan.vrange)
    except ValueError:
        v = -1.0
    require(v in VALID_VRANGE, "vrange must be one of "
            + ", ".join(f"{r:g}" for r in VALID_VRANGE))
    require(plan.gain in GAIN_TABLE,
            f"gain must be one of {sorted(GAIN_TABLE)}")
    require(plan.fs >= 1.0, "fs must be at least 1 Hz")
    plan.record_settings().check_extra_args()
    if mode in ("run", "setup"):
        n_ch = sum(1 for n, _k, _p in entries if split_channel(n)[1] in want)
        require(plan.fs * n_ch <= DAQ_AGGREGATE_MAX,
                f"fs {plan.fs:g} Hz x {n_ch} channel(s) = "
                f"{plan.fs * n_ch:.0f} S/s, over the USB-6008 aggregate "
                f"limit of {DAQ_AGGREGATE_MAX} -- lower fs or record "
                f"fewer channels")
    require(plan.cal_v_per_nT is None or plan.cal_v_per_nT > 0,
            "cal_v_per_nT must be positive")
    require(plan.zero_cond > 0, "zero_cond must be positive")
    require(plan.lock_timeout > 0, "lock_timeout must be positive")
    require(plan.poll_s >= 0.2, "poll_s must be at least 0.2 s")
    # nan/inf survive every "> 0" test: seconds=inf once became a record
    # with no end
    for name, value in (("seconds", plan.seconds), ("fs", plan.fs),
                        ("zero_cond", plan.zero_cond),
                        ("lock_timeout", plan.lock_timeout),
                        ("poll_s", plan.poll_s),
                        ("cal_v_per_nT", plan.cal_v_per_nT),
                        ("stream_seconds", plan.stream_seconds)):
        require(value is None or math.isfinite(value),
                f"{name} must be a finite number (got {value!r})")
    if plan.sync == "master-slave":
        require(len(heads) >= 2,
                "master-slave needs two or more sensors (one drives the "
                "modulation, the others follow)")
    require(plan.sync in (None, "none", "master-slave", "independent"),
            f"sync: unknown role plan {plan.sync!r}")
    if mode == "run":
        require(plan.seconds > 0, "seconds must be positive")
    if mode == "stream":
        require((plan.stream_seconds or 0) > 0,
                "the stream duration must be positive")
    return heads


# -------------------------------------------------------------- session

def session_json_name(mode):
    """zero/standby may be handed a finished session's outdir; writing
    qzfm_session.json there replaced a night's ledger once (2026-08-19),
    so they get their own name."""
    return ("qzfm_session.json" if mode in ("run", "setup", "stream")
            else f"qzfm_{mode}_session.json")


class QZFMSession:
    """One session of one mode, with its ledger and its log."""

    def __init__(self, ctx, lab, plan):
        self.ctx = ctx
        self.lab = lab
        self.plan = plan
        self.sensors = {}
        self.session = None

    # -- harness --------------------------------------------------------

    def _resolve_outdir(self, mode, n_heads=1):
        """--run/--stream default to the next dated run folder
        (<data_root>/Oct 2026/15/001_SingleSensorZ), or with the old
        layout to <data_root>/newdata, newdata1, ...; an explicit path
        already holding a session is numbered newdata1-style;
        zero/standby/status keep the given path."""
        recording = mode in ("run", "setup", "stream")
        requested = self.plan.outdir
        if not requested:
            if mode in ("run", "stream") and self.lab.dated:
                name = self.plan.name or run_name(self.plan, mode, n_heads)
                try:
                    chosen = self.lab.new_run_dir(name)
                except (ValueError, OSError) as e:
                    raise ConfigError(f"cannot make the run folder: {e}")
                self.ctx.say("out", chosen)
                return chosen
            if mode in ("run", "stream"):
                chosen = unique_outdir(self.lab.bucket("record"))
                self.ctx.say("out", chosen)
                return chosen
            return None
        if not recording:
            return requested
        chosen = unique_outdir(requested)
        if os.path.normcase(os.path.abspath(chosen)) != \
                os.path.normcase(os.path.abspath(requested)):
            self.ctx.say("out", f"{requested} already has a session -- "
                                f"using {chosen}")
        return chosen

    def _announce(self, heads):
        """Which physical sensor is in which slot: named in every message,
        recorded in the ledger and every meta.json, shown on the UI's
        indicator cards (which then get every status frame)."""
        info = self.plan.sensor_info or [
            {"sensor": h, "number": key_number(h), "label": None,
             "usb_serial": serial, "location": "",
             "inputs": head_pins(self.plan.map, h), "modulation": None}
            for h, serial in heads.items()]
        info = [r for r in info if r.get("sensor") in self.sensors]
        self.plan.sensor_info = info
        self.ctx.status("sensors", sensors=info)
        for sensor in self.sensors.values():
            sensor.on_frame = (lambda s, frame, ctx=self.ctx:
                               ctx.status("frame", head=s.head, frame=frame))
        for rec in info:
            self.sensors[rec["sensor"]].display = display_of(rec)
            pins = rec.get("inputs") or {}
            self.ctx.say("sensors", display_of(rec) + " -> DAQ "
                         + ", ".join(f"{p} ({a.upper()})"
                                     for a, p in pins.items()))
        self.lab.plug(info)
        return info

    def _open(self, mode, only_head=None):
        heads = validate(self.plan, mode)
        self.plan.outdir = self._resolve_outdir(mode, len(heads))
        self.ctx.reset_log()
        log_path = self.plan.log or (
            os.path.join(self.plan.outdir, "qzfm_session.log")
            if self.plan.outdir else None)
        if log_path:
            self.ctx.open_log(log_path)
        self.ctx.say("start", f"bfield qzfm session, mode {mode}"
                              + (" [SIMULATED LAB]" if self.lab.sim else ""))
        self.sensors = {}
        for head, name in heads.items():
            self.sensors[head] = self.lab.sensor(
                name, self.ctx, head=head, zero_cond=self.plan.zero_cond,
                lock_timeout=self.plan.lock_timeout)
        if only_head is not None:
            # only that head is opened; the other stays untouched
            self.sensors = {only_head: self.sensors[only_head]}
        info = self._announce(heads)
        self.session = {
            "program": "qzfm_serial", "software": "bfield.procedures."
                                                  "qzfm_session",
            "generated": clock.iso_now(), "mode": mode,
            "common_version": COMMON_VERSION,
            "simulated": self.lab.sim,
            "settings": dict(sorted(dataclasses.asdict(self.plan).items())),
            "sensors": info,
            "setup": None, "heals": [], "runs": [], "n_guards": 0,
            "end_reason": None, "outcome": None, "traceback": None,
            "events": self.ctx.events}
        return heads

    def checkpoint(self):
        """Write the ledger now, mid-run (atomic)."""
        if self.plan.outdir:
            self._save()

    def _save(self):
        try:
            os.makedirs(self.plan.outdir, exist_ok=True)
            write_json_atomic(
                os.path.join(self.plan.outdir,
                             session_json_name(self.session.get("mode"))),
                sanitize(self.session))
        except OSError as e:
            self.ctx.warn(f"could not write the session record to "
                          f"{self.plan.outdir} ({type(e).__name__}: {e}) -- "
                          f"the data is on disk but nothing describes the "
                          f"session; check for a full or read-only disk")

    def _close_sensors(self):
        """Close every head even when an earlier one refuses."""
        for head, sensor in self.sensors.items():
            try:
                sensor.close()
            except Exception as e:
                self.ctx.warn(f"{sensor.display}: closing the port failed "
                              f"({type(e).__name__}: {e}) -- if the next "
                              "run reports the port busy, this is why")

    def _run(self, mode, body, only_head=None):
        """Shared harness: session json in a finally, outcome labelling,
        ports closed on every exit. Returns the exit code."""
        self._open(mode, only_head)
        session = self.session
        outcome, code = "completed", 0
        try:
            code = body()
            if code is None:
                raise BFieldError(
                    "internal: the mode body returned None instead of an "
                    "exit code -- refusing to report silence as success")
            if code:
                outcome = (f"exit {code}: {session['end_reason']}"
                           if session.get("end_reason")
                           else f"recorder exit {code} "
                                f"({exit_meaning(code)})")
        except KeyboardInterrupt:
            outcome, code = "interrupted", 130
        except Stopped:
            outcome, code = "interrupted: stopped by the operator", 130
        except BFieldError as e:
            label = ("aborted" if isinstance(e, (SensorAbort, SessionStop))
                     else "failed")
            outcome, code = f"{label}: {type(e).__name__}: {e}", e.code
        except Exception as e:
            outcome, code = f"crashed: {type(e).__name__}: {e}", 1
            session["traceback"] = traceback.format_exc()
            raise
        finally:
            session["outcome"] = outcome
            if outcome != "completed" or code:
                self.ctx.write("fatal" if code else "warn",
                               (f"ERROR[{code}] " if code else "")
                               + f"session {outcome}",
                               "error" if code else "warn")
            if session.get("traceback"):
                self.ctx.write("trace", session["traceback"].rstrip())
            if self.plan.outdir:
                self._save()
            self._close_sensors()
            self.ctx.close_log()
        return code

    # -- modes ----------------------------------------------------------

    def status(self):
        """Read-only: one frame per head -> list of (head, sensor, frame,
        blank). Opens no ledger."""
        heads = validate(self.plan, "status")
        self.sensors = {h: self.lab.sensor(n, self.ctx, head=h)
                        for h, n in heads.items()}
        self._announce(heads)
        try:
            return ops.status_frames(self.sensors)
        finally:
            self._close_sensors()

    def setup(self):
        def body():
            ss = self.plan.setup_settings(self.sensors)
            factors = ops.run_setup(self.ctx, self.lab, self.sensors, ss,
                                    self.session, self.checkpoint)
            two = len(self.sensors) > 1
            missing = [h for h, f in factors.items() if not f]
            if missing:
                names = (", ".join(self.sensors[h].display
                                   for h in missing) if two
                         else "the sensor")
                self.ctx.warn(("both sensors zeroed" if two
                               else "sensor zeroed")
                              + f", but NO calibration factors for {names} "
                                "(see raw_calib_message in the session "
                                "json). Recording still works; "
                                "require_factors aborts instead")
            else:
                self.ctx.say("setup", ("both sensors ready" if two
                                       else "sensor ready")
                             + " -- zeroed, calibrated, factors recorded")
            return 0
        return self._run("setup", body)

    def zero(self):
        def body():
            ss = self.plan.setup_settings(self.sensors)
            ops.zero_and_calibrate(self.ctx, self.sensors, ss, self.session)
            return 0
        return self._run("zero", body)

    def standby(self):
        def body():
            ops.standby(self.ctx, self.sensors, self.session)
            return 0
        return self._run("standby", body)

    def record(self):
        """Setup, then one record (the old --run)."""
        def body():
            from bfield.devices.qzfm import HealthTracker
            ss = self.plan.setup_settings(self.sensors)
            factors = ops.run_setup(self.ctx, self.lab, self.sensors, ss,
                                    self.session, self.checkpoint)
            # ledger exists before the record starts: a power cut mid-run
            # must not cost the setup record
            self.checkpoint()
            trackers = {h: HealthTracker() for h in self.sensors}
            run_dir = alloc_run_dir(self.plan.outdir, 1)
            t0 = clock.iso_now()
            self.ctx.say("run", f"recording {self.plan.seconds:g} s -> "
                                f"{run_dir}")
            # no rail-abort here: a single run keeps full-length
            # behaviour, and the child's own exit code passes through
            rc, meta, pollers = record_run(
                self.ctx, self.lab, self.sensors, run_dir, ss.rs, ss.axes,
                factors, self.plan.seconds, rail_abort=False,
                trackers=trackers)
            entry = make_run_entry(1, run_dir, rc, meta, factors, t0,
                                   pollers.rows, self.lab.data_root)
            if pollers.abort_reason:
                entry["outcome"] = "aborted"
                self.session["runs"].append(entry)
                raise SensorAbort(pollers.abort_reason)
            # exit 0 with no meta must not read as success
            if rc == 0 and (meta or {}).get("status") != "ok":
                entry["outcome"] = "ok_no_meta"
                entry["exit_code"] = NO_META_EXIT
                entry["child_exit_code"] = rc
                rc = NO_META_EXIT
                self.ctx.warn(f"the recorder exited 0 but wrote no readable "
                              f"meta.json in {run_dir} -- samples with no "
                              f"provenance. Exiting {NO_META_EXIT} so this "
                              "is not filed as a good run")
                self.session["end_reason"] = ("the recorder finished and "
                                              "left no readable meta.json")
            else:
                entry["outcome"] = ("ok" if rc == 0 else "flagged"
                                    if rc in (30, 31) else "failed")
            self.session["runs"].append(entry)
            if rc:
                self.ctx.warn(f"recorder exited {rc} -- data kept in "
                              f"{run_dir}")
            return rc
        return self._run("run", body)

    def stream(self):
        """The serial digital stream (200 S/s) of one head to csv."""
        heads = parse_heads(self.plan.device_name)
        keep = self.plan.stream_head if self.plan.stream_head in heads \
            else next(iter(heads))
        self.plan.stream_head = keep

        def body():
            sensor = self.sensors[keep]
            axes = parse_axes(self.plan.axes)
            sensor.connect()
            t, v = sensor.read_stream(self.plan.stream_seconds, axes[0])
            os.makedirs(self.plan.outdir, exist_ok=True)
            path = os.path.join(
                self.plan.outdir,
                f"stream_{clock.strftime('%H%M%S')}_{axes[0]}.csv")
            with open(path, "w", newline="\n", encoding="utf-8") as f:
                f.write("t_s,pT\n")
                for ti, vi in zip(t, v):
                    f.write(f"{ti:.6g},{vi:.6g}\n")
            self.ctx.say("stream", f"{len(t)} samples at 200 S/s from "
                                   f"{sensor.display} -> {path}")
            self.session["runs"].append(
                {"index": 1,
                 "dir": rel_to(path, os.path.dirname(self.lab.data_root)),
                 "outcome": "ok", "exit_code": 0})
            return 0

        return self._run("stream", body, only_head=keep)


def make_run_entry(index, run_dir, rc, meta, factors, t0, house_rows,
                   data_root):
    m = meta or {}
    return {"index": index,
            # relative to data_root's parent: "result/newdata1/R01_1527"
            # while data_root is the old result tree, as the old ledgers
            "dir": (rel_to(run_dir, os.path.dirname(data_root))
                    if run_dir else None),
            "t0": t0, "t_end": clock.iso_now(), "outcome": None,
            "exit_code": rc, "meta_status": m.get("status"),
            "cal_factors": dict(factors) if factors else {},
            "house_rows": house_rows}
