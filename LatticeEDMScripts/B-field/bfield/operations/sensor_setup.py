"""Sensor operations: bring up, zero, calibrate, prove, heal, standby.

Moved from qzfm_serial.py (run_setup, heal, check_factors,
apply_sync_roles, wait_for_cell_lock, the --zero / --standby / --status
bodies). Every function takes the Context, the open sensors ({sensor
key "s1": QZFMSensor}, the first leading) and a SetupSettings, and fills
the session
record it is given -- the same "setup" / "heals" / "standby" blocks the
old qzfm_session.json had.
"""

from __future__ import annotations

import math
from dataclasses import dataclass, field

from bfield import clock
from bfield.constants import (CAL_FACTOR_MAX, CAL_FACTOR_MIN,
                              CAL_FACTOR_WARN, MOD_CARRIER_HZ)
from bfield.devices.qzfm import (CELL_RELOCK_S, LOCK_RECHECK_S,
                                 REZERO_TIMEOUT_S, STANDBY_VERIFY_WAIT_S,
                                 ZERO_CONSECUTIVE, frame_is_blank)
from bfield.errors import (ConfigError, SensorAbort, SensorError,
                           SessionStop, exit_meaning)
from bfield.operations.record import (RecordSettings, factors_to_flag,
                                      guard_record, guard_seconds)
from bfield.operations.sync_check import check_modulation_sync

SETUP_STEPS = ("connect", "auto start", "gain / axis mode", "field zero",
               "calibrate", "guard record")
SYNC_STEP = "sync check"
# how long to watch the first sensor's output before and after the
# others are zeroed
ZERO_SHIFT_READ_S = 2.0


@dataclass
class SetupSettings:
    axes: list = field(default_factory=lambda: ["z"])
    gain: str = "1x"
    force_start: bool = False
    require_factors: bool = False
    zero_shift_warn: float = 200.0
    sync: str | None = None        # None = master-slave with 2+ sensors
    master_head: str | None = None  # which slot head is master (None: first)
    lock_timeout: float = 600.0
    outdir: str | None = None      # where guard records go; None = no guard
    rs: RecordSettings = field(default_factory=RecordSettings)


def stage(ctx, heads, name, state, detail=None, **data):
    """One setup stage of one or more sensors, for indicators.
    state: running | passed | skipped | warn | failed."""
    for h in heads:
        ctx.status("stage", head=h, stage=name, state=state, detail=detail,
                   **data)


def factors_state(factors, expected):
    """passed, or warn when a factor is missing or above the health rule."""
    if not factors or any(ax not in factors for ax in expected):
        return "warn"
    if any(f > CAL_FACTOR_WARN for f in factors.values()):
        return "warn"
    return "passed"


def mode_from_axes(axes):
    return "dual" if len(axes) > 1 else axes[0]


def setup_steps(n_heads):
    """Two heads add reading the modulation clocks off the guard record;
    last, because it needs that record."""
    return SETUP_STEPS + ((SYNC_STEP,) if n_heads > 1 else ())


class StepTimer:
    """Numbered progress through the setup sequence; every step lands in
    session["setup"]["steps"]."""

    def __init__(self, ctx, steps):
        self.ctx = ctx
        self.names = list(steps)
        self.rows = []
        self.t = None

    def start(self, name, detail=""):
        self.t = clock.monotonic()
        n = self.names.index(name) + 1
        self.ctx.say("step", f"{n}/{len(self.names)} {name}"
                             + (f" -- {detail}" if detail else ""))
        self.ctx.progress((n - 1) / len(self.names), f"setup: {name}")

    def done(self, name, detail="", skipped=False, skip_reason=None):
        # a step never start()ed has no elapsed time; None, not 0.0,
        # so it does not read as "finished instantly"
        dt = None if self.t is None else clock.monotonic() - self.t
        self.rows.append({"step": name, "skipped": bool(skipped),
                          "skip_reason": skip_reason,
                          "elapsed_s": None if dt is None else round(dt, 1),
                          "detail": detail or None})
        n = self.names.index(name) + 1
        tail = "skipped" if skipped else f"{dt:.1f} s"
        self.ctx.say("step", f"{n}/{len(self.names)} {name}: {tail}"
                             + (f" -- {detail}" if detail else ""))

    def summary(self):
        self.ctx.say("step", "setup summary:")
        for r in self.rows:
            mark = "skip" if r["skipped"] else f"{r['elapsed_s']:6.1f}s"
            self.ctx.say("step", f"  {mark}  {r['step']}"
                                 + (f"  ({r['detail']})" if r["detail"]
                                    else ""))


def check_factors(ctx, ss, setup, factors, raw, label="", expected=()):
    lead = f"{label}: " if label else ""
    if not factors:
        msg = "calibration factors could not be parsed from the serial reply"
        if ss.require_factors:
            raise ConfigError(f"{lead}{msg} (raw message: {raw!r})")
        setup["warnings"].append(msg)
        ctx.warn(f"{lead}{msg} -- raw message kept: {raw!r}")
        return
    # check per axis: the parser drops factors outside 0.1-3.0, so a
    # missing axis is often an unhealthy one (15.90 event, 2026-08-19)
    missing = [ax for ax in expected if ax not in factors]
    if missing:
        msg = (f"calibration incomplete: no factor for "
               f"{', '.join(missing)} of {', '.join(expected)} "
               f"(outside {CAL_FACTOR_MIN:g}-{CAL_FACTOR_MAX:g} is "
               f"dropped -- can be an unhealthy axis)")
        if ss.require_factors:
            raise ConfigError(f"{lead}{msg} (raw message: {raw!r})")
        setup["warnings"].append(msg)
        ctx.warn(f"{lead}{msg}")
    for ax, f in sorted(factors.items()):
        if f > CAL_FACTOR_WARN:
            note = (f"cal factor {ax}:{f:g} exceeds {CAL_FACTOR_WARN:g} "
                    "(QuSpin health rule) -- the sensor cell may need service")
            setup["warnings"].append(note)
            ctx.warn(f"{lead}{note}")


def zero_progress_for(ctx, label):
    """Heartbeat while a head zeroes, so a 40 s zero is never silence."""
    def zero_progress(worst, streak, stale, left):
        ctx.say("zero", "  " + label
                + (f"{worst:.1f} pT/s" if worst is not None
                   else "no reading yet")
                + f", streak {streak}/{ZERO_CONSECUTIVE}"
                + (f", {stale} repeated frames" if stale else "")
                + f", {left:.0f} s left")
    return zero_progress


def wait_for_cell_lock(ctx, sensors, st=None):
    """Hold until every head has its cell-temperature lock back.

    Bounded and fatal on timeout: the warn-only version let an unlocked
    head go straight into a record (2026-08-07).
    """
    deadline = clock.time() + CELL_RELOCK_S
    t0 = clock.time()
    pending = dict(sensors)
    waited, read_errors = {}, {}
    while pending and clock.time() < deadline:
        for head in list(pending):
            try:
                snap = pending[head].status()
                read_errors.pop(head, None)
                if snap["cellT_lock"]:
                    waited[head] = round(clock.time() - t0, 1)
                    del pending[head]
            except SensorError as e:
                # an unreadable head is NOT settled (a USB dropout used
                # to PASS this gate); keep polling until the deadline
                read_errors[head] = str(e)
        if pending:
            ctx.sleep(1.0)
    if pending:
        names = ", ".join((sensors[h].display if len(sensors) > 1
                           else "the sensor")
                          for h in pending)
        extra = ("; status reads failing: "
                 + "; ".join(f"{h}: {e}"
                             for h, e in sorted(read_errors.items()))
                 if read_errors else "")
        raise SensorAbort(
            f"{names}: cell temperature lock not back within "
            f"{CELL_RELOCK_S:g} s{extra} -- calibrated but not settled. "
            "Factors are in the session json; re-run the setup once LED2 "
            "is back")
    waited = {h: waited[h] for h in sensors}
    if any(v > 1.0 for v in waited.values()) and st is not None:
        ctx.say("setup", "waited for the cell temperature lock to come back: "
                + ", ".join(f"{sensors[h].display} {v:g} s"
                            for h, v in waited.items()))
    return waited


def field_mean_pT(ctx, sensor, axis, seconds=ZERO_SHIFT_READ_S):
    """Mean of the head's own measured field, not its compensation.

    Compensation values freeze once Field Zero finishes (the analog
    head is open loop), so only the digital stream can show that a
    neighbour moved the field.
    """
    try:
        _, v = sensor.read_stream(seconds, axis)
    except Exception as e:
        ctx.warn(f"could not read the field stream ({type(e).__name__}: {e})")
        return None
    # the vendor package returns a bare float for a 1-sample read;
    # iterating it raised TypeError and killed a series (2026-08-19)
    if isinstance(v, (int, float)):
        v = [v]
    try:
        vals = [x for x in v
                if isinstance(x, (int, float)) and math.isfinite(x)]
    except TypeError:
        ctx.warn(f"the field stream came back as {type(v).__name__}, which "
                 f"is not samples -- skipping the zero-shift read")
        return None
    return sum(vals) / len(vals) if vals else None


def resolve_sync(ctx, ss, sensors):
    """Default: master-slave with two or more sensors, none with one
    (user decision after QuSpin support 2026-08-06)."""
    if ss.sync is not None:
        return ss.sync
    if len(sensors) > 1:
        ctx.say("sync", "several sensors default to master-slave (QuSpin "
                        "support 2026-08-06; free-running pairs silenced "
                        "calibrate on this bench 5/5, and 78/73 removed the "
                        "slave's sub-Hz sine 2026-10-15). sync = none "
                        "disables")
        return "master-slave"
    return "none"


def apply_sync_roles(ctx, sensors, spec, setup=None, master_head=None):
    """Set the modulation master/slave roles (commands 78/73).

    Free-running pairs silenced calibrate on this bench 5/5 (2026-08-05/
    06); single-head parsed 6/6. The LED |50/|51 flag is read back and
    recorded as evidence, not a gate (it contradicted the commanded roles
    once). `none` sends nothing; `independent` is for measuring the beat.
    """
    if spec == "none":
        return ""
    heads = list(sensors)
    if spec == "master-slave":
        if len(heads) < 2:
            raise ConfigError("master-slave needs two or more sensors (one "
                              "drives the modulation, the others follow)")
        master = master_head or heads[0]
        if master not in heads:
            raise ConfigError(f"master {master!r} is not one of the heads "
                              f"{heads}")
        roles = {h: h == master for h in heads}
    elif spec == "independent":
        roles = {h: True for h in heads}
    else:
        raise ConfigError(f"sync: unknown role plan {spec!r}")
    for head, is_master in roles.items():
        sensors[head].set_master(is_master)
    led = {}
    for head in roles:
        try:
            led[head] = bool(sensors[head].status().get("master"))
        except SensorError:
            led[head] = None
    # messages name the sensor ("sensor 1"); the ledger keeps the heads
    def short(h):
        return getattr(sensors[h], "display", h).split(" (")[0]

    def flag(v):
        return "?" if v is None else "master" if v else "not master"
    words = ", ".join(f"{short(h)} {'master' if m else 'slave'}"
                      for h, m in roles.items())
    if setup is not None:
        setup["sync_roles"] = {h: ("master" if m else "slave")
                               for h, m in roles.items()}
        setup["sync_roles_led"] = led
    ctx.say("setup", "modulation roles set: "
            + ", ".join(f"{getattr(sensors[h], 'display', h)} = "
                        f"{'master (command 78)' if m else 'slave (command 73)'}"
                        for h, m in roles.items())
            + " (QuSpin support 2026-08-06) -- LED master flag now reads "
            + ", ".join(f"{short(h)} {flag(led[h])}" for h in led))
    return f", sync {words}"


def refresh_setup_factors(session, entry, lead_head, names=None):
    """Point session["setup"] at what the sensor says now, so a morning
    reader does not quote pre-fault numbers for post-fault runs."""
    setup = session.get("setup")
    if not setup:
        return          # a heal before the setup finished
    for head, per in entry["heads"].items():
        target = (setup.get("heads") or {}).get(head)
        if target is not None:
            target.update({k: per[k] for k in ("zero_residual_pT_per_s",
                                               "cal_factors",
                                               "raw_calib_message")})
    lead = entry["heads"].get(lead_head)
    if lead is not None:
        setup["cal_factors"] = lead["cal_factors"]
        setup["zero_residual_pT_per_s"] = lead["zero_residual_pT_per_s"]
        setup["raw_calib_message"] = lead["raw_calib_message"]
    two = len(entry["heads"]) > 1
    for head, per in entry["heads"].items():
        for w in per["warnings"]:
            who = (names or {}).get(head, f"head {head}")
            tagged = f"{who}: {w}" if two else w
            if tagged not in setup.setdefault("warnings", []):
                setup["warnings"].append(tagged)


def heal(ctx, sensors, ss, session, why, checkpoint=None):
    """Re-zero and re-calibrate every head (they share the shield, so
    the disturbance is rarely local). Returns {head: factors}."""
    ctx.say("heal", f"re-zero + re-calibrate ({why})")
    two = len(sensors) > 1
    factors = {}
    entry = {"time": clock.iso_now(), "why": why, "heads": {}, "warnings": []}
    for head, sensor in sensors.items():
        stage(ctx, [head], "field zero", "running", f"re-zero ({why})")
        residual = sensor.zero(timeout=min(ss.lock_timeout, REZERO_TIMEOUT_S))
        stage(ctx, [head], "field zero", "passed",
              f"{residual:g} pT/s worst axis" if residual is not None
              else "settled", residual=residual)
        stage(ctx, [head], "calibrate", "running", "re-calibrate")
        f, raw, cal_state = sensor.calibrate_factors()
        stage(ctx, [head], "calibrate", factors_state(f, ss.axes),
              factors_to_flag(f) or raw, factors=f)
        factors[head] = f
        per = {"zero_residual_pT_per_s": residual, "cal_factors": f,
               "raw_calib_message": raw, "calib_state_at_send": cal_state,
               "warnings": []}
        entry["heads"][head] = per
        check_factors(ctx, ss, per, f, raw,
                      label=(f"heal ({sensor.display})" if two
                             else "heal"),
                      expected=ss.axes)
        entry["warnings"].extend(per["warnings"])
    wait_for_cell_lock(ctx, sensors)
    lead_head = next(iter(sensors))
    lead = entry["heads"][lead_head]
    entry.update({k: lead[k] for k in ("zero_residual_pT_per_s",
                                       "cal_factors", "raw_calib_message")})
    session["heals"].append(entry)
    refresh_setup_factors(session, entry, lead_head,
                          {h: s.display for h, s in sensors.items()})
    if checkpoint is not None:
        checkpoint()
    return factors


def run_setup(ctx, lab, sensors, ss, session, checkpoint=None):
    """Bring every head to zeroed, calibrated, recording proved.

    Returns the calibration factors, shaped {head: {axis: float}}.
    """
    two = len(sensors) > 1
    lead_head = next(iter(sensors))
    axes = ss.axes
    st = StepTimer(ctx, setup_steps(len(sensors)))
    setup = {"warm_start": None, "mode": None, "steps": st.rows,
             "zero_residual_pT_per_s": None, "cal_factors": {},
             "raw_calib_message": "", "warnings": [], "guard_exit": None,
             "sensor_id": None, "port": None,
             "heads": {h: {"sensor_id": None, "port": None,
                           "warm_start": None,
                           "zero_residual_pT_per_s": None,
                           "zero_shift_pT": None, "cal_factors": {},
                           "raw_calib_message": "", "warnings": []}
                       for h in sensors},
             "sync": None}
    session["setup"] = setup

    def note(head, msg):
        setup["heads"][head]["warnings"].append(msg)
        who = sensors[head].display
        setup["warnings"].append(msg if not two else f"{who}: {msg}")
        ctx.warn(msg if not two else f"{who}: {msg}")

    def tag(head):
        return f"{sensors[head].display}: " if two else ""

    st.start("connect", "port matching "
             + ", ".join(repr(s.device_name) for s in sensors.values()))
    for head, sensor in sensors.items():
        ctx.check_stop()
        stage(ctx, [head], "connect", "running")
        sensor.connect()
        stage(ctx, [head], "connect", "passed",
              f"{sensor.serial_id}" + (f" on {sensor.port}"
                                       if sensor.port else ""))
        setup["heads"][head]["sensor_id"] = sensor.serial_id
        setup["heads"][head]["port"] = sensor.port
    setup["sensor_id"] = sensors[lead_head].serial_id
    setup["port"] = sensors[lead_head].port

    def ident(s):
        return s.serial_id + (f" on {s.port}" if s.port else "")

    st.done("connect", ", ".join(f"{s.display} = {ident(s)}"
                                 for s in sensors.values()))

    st.start("auto start")
    details = []
    for head, sensor in sensors.items():
        ctx.check_stop()
        stage(ctx, [head], "auto start", "running",
              "cold start: 95-120 s is normal")
        info = sensor.bringup(force=ss.force_start)
        warm = bool(info.get("warm_start"))
        stage(ctx, [head], "auto start", "skipped" if warm else "passed",
              info.get("skip_reason") or "laser + cell locked")
        setup["heads"][head]["warm_start"] = warm
        setup["heads"][head]["lock_wait_s"] = info.get("lock_wait_s")
        detail = info.get("skip_reason") or "laser + cell locked"
        details.append(f"{tag(head)}{'skipped -- ' if warm and two else ''}"
                       f"{detail}")
    setup["warm_start"] = setup["heads"][lead_head]["warm_start"]
    all_warm = all(setup["heads"][h]["warm_start"] for h in sensors)
    st.done("auto start", "; ".join(details), skipped=all_warm,
            skip_reason=details[0] if all_warm else None)

    mode = mode_from_axes(axes)
    setup["mode"] = mode
    st.start("gain / axis mode")
    stage(ctx, sensors, "gain / mode", "running")
    for sensor in sensors.values():
        sensor.set_gain(ss.gain)
        sensor.set_mode(mode)  # resets zero/cal flags: always before zeroing
    # clock roles before zero/calibrate: both measure through the modulation
    sync_detail = apply_sync_roles(ctx, sensors, resolve_sync(ctx, ss, sensors),
                                   setup, master_head=ss.master_head)
    st.done("gain / axis mode", f"gain {ss.gain}, mode {mode}" + sync_detail)
    for head in sensors:
        role = (setup.get("sync_roles") or {}).get(head)
        stage(ctx, [head], "gain / mode", "passed",
              f"gain {ss.gain}, {mode}" + (f", modulation {role}"
                                           if role else ""), role=role)

    st.start("field zero",
             f"settle < {sensors[lead_head].zero_cond:g} pT/s "
             f"x{ZERO_CONSECUTIVE}, give up after {ss.lock_timeout:g} s"
             + (", in sensor order, then re-read the first" if two
                else ""))
    zero_bits, lead_before = [], None
    for head, sensor in sensors.items():
        ctx.check_stop()
        stage(ctx, [head], "field zero", "running",
              f"settle < {sensor.zero_cond:g} pT/s")
        residual = sensor.zero(timeout=ss.lock_timeout,
                               progress=zero_progress_for(ctx, tag(head)))
        setup["heads"][head]["zero_residual_pT_per_s"] = residual
        stage(ctx, [head], "field zero",
              "failed" if sensor.zero_off_failed else "passed",
              f"{residual:g} pT/s worst axis" if residual is not None
              else "settled", residual=residual)
        if sensor.zero_off_failed:
            # a head still zeroing makes the next calibrate measure the
            # compensation loop, not the head -- fail the setup
            raise SensorError(
                f"{tag(head)}field zero off failed "
                f"({sensor.zero_off_failed}) -- head may still be zeroing, "
                "cal factors would be suspect. Fix the link, re-run setup")
        zero_bits.append(f"{tag(head)}" + (f"{residual:g} pT/s worst axis"
                                           if residual is not None
                                           else "settled"))
        if two and head == lead_head:
            # the first sensor is zeroed and frozen; anything that moves
            # its reading from here on is the others' coils, not its own
            lead_before = field_mean_pT(ctx, sensor, axes[0])
    setup["zero_residual_pT_per_s"] = \
        setup["heads"][lead_head]["zero_residual_pT_per_s"]
    if two and lead_before is not None:
        after = field_mean_pT(ctx, sensors[lead_head], axes[0])
        if after is not None:
            shift = after - lead_before
            setup["heads"][lead_head]["zero_shift_pT"] = round(shift, 1)
            zero_bits.append(f"zeroing the other sensor(s) moved "
                             f"{sensors[lead_head].display} by {shift:+.1f} pT")
            if abs(shift) > ss.zero_shift_warn:
                note(lead_head,
                     f"zeroing the other sensor(s) moved this one by "
                     f"{shift:+.1f} pT (over zero_shift_warn "
                     f"{ss.zero_shift_warn:g}) -- move the heads further "
                     "apart, or accept the offset knowingly")
    st.done("field zero", "; ".join(zero_bits))

    st.start("calibrate")
    factors, cal_bits = {}, []
    for head, sensor in sensors.items():
        ctx.check_stop()
        stage(ctx, [head], "calibrate", "running",
              "waiting for the Calib reply")
        f, raw, cal_state = sensor.calibrate_factors()
        stage(ctx, [head], "calibrate", factors_state(f, axes),
              factors_to_flag(f) or raw, factors=f)
        factors[head] = f
        setup["heads"][head]["cal_factors"] = f
        setup["heads"][head]["raw_calib_message"] = raw
        setup["heads"][head]["calib_state_at_send"] = cal_state
        check_factors(ctx, ss, setup["heads"][head], f, raw,
                      label=tag(head).rstrip(": "), expected=axes)
        cal_bits.append(tag(head) + (factors_to_flag(f, axes)
                                     or "(none parsed)"))
    setup["cal_factors"] = factors[lead_head]
    setup["raw_calib_message"] = setup["heads"][lead_head]["raw_calib_message"]
    for head in sensors:
        for msg in setup["heads"][head]["warnings"]:
            tagged = (f"{sensors[head].display}: " + msg) if two else msg
            if tagged not in setup["warnings"]:
                setup["warnings"].append(tagged)
    st.done("calibrate", "; ".join(cal_bits))

    # LED2 drops ~16 s in the tail of a setup; the relock wait runs
    # before the guard record so the guard is not racing it
    wait_for_cell_lock(ctx, sensors, st)

    gsec = guard_seconds(sensors)
    st.start("guard record", f"{gsec:g} s pre-flight"
             + (" (also the sync check)" if two else ""))
    gdir = None
    stage(ctx, sensors, "guard record", "running", f"{gsec:g} s pre-flight")
    if ss.outdir:
        for attempt in (0, 1):
            ctx.check_stop()
            session["n_guards"] += 1
            rc, _, gdir = guard_record(ctx, lab, sensors, ss.rs, axes,
                                       factors, ss.outdir,
                                       session["n_guards"])
            if rc == 0:
                break
            if rc in (30, 31) and attempt == 0:
                ctx.say("guard", f"guard record flagged (exit {rc}: "
                                 f"{exit_meaning(rc)}) -- one heal before "
                                 "giving up")
                factors = heal(ctx, sensors, ss, session, f"guard exit {rc}",
                               checkpoint)
                continue
            st.done("guard record", f"exit {rc} ({exit_meaning(rc)})")
            stage(ctx, sensors, "guard record", "failed",
                  f"exit {rc} ({exit_meaning(rc)})")
            raise SessionStop(
                f"guard record failed (exit {rc}: {exit_meaning(rc)}) "
                "-- not ready to record")
        setup["guard_exit"] = rc
        st.done("guard record", "ok")
        stage(ctx, sensors, "guard record", "passed", "exit 0")
    else:
        st.done("guard record", "no outdir given", skipped=True,
                skip_reason="no outdir given")
        stage(ctx, sensors, "guard record", "skipped", "no outdir given")

    if two:
        st.start(SYNC_STEP, f"modulation carrier near {MOD_CARRIER_HZ:g} Hz")
        stage(ctx, sensors, "sync check", "running")
        if gdir is None:
            msg = ("several sensors set up without an outdir: no guard "
                   "record, no sync check -- shared clock unverified. Re-run "
                   "setup with an outdir before recording")
            setup["warnings"].append(msg)
            ctx.warn(msg)
            st.done(SYNC_STEP, "no guard record to read", skipped=True,
                    skip_reason="no outdir given")
            stage(ctx, sensors, "sync check", "skipped", "no guard record")
        else:
            sync = check_modulation_sync(
                ctx, gdir, ss.rs.fs, ss.rs.map,
                names={h: s.display for h, s in sensors.items()})
            setup["sync"] = sync
            if sync["verdict"] == "not synced":
                # warning, not refusal: an unsynced pair is still worth
                # recording (that is how the beat gets measured)
                ctx.warn(f"modulation NOT synced -- {sync['why']}"
                         + ("; the beat lands in the eEDM band (shared fake "
                            "signal there)" if sync["in_edm_band"] else "")
                         + ". Not a stop: setup still reports ready and "
                           "recording runs")
            st.done(SYNC_STEP, f"{sync['verdict']} -- {sync['why']}")
            stage(ctx, sensors, "sync check",
                  "warn" if sync["verdict"] == "not synced" else "passed",
                  f"{sync['verdict']} -- {sync['why']}")

    st.summary()
    ctx.progress(1.0, "setup done")
    return factors


def zero_and_calibrate(ctx, sensors, ss, session):
    """Re-zero + re-calibrate warm heads (the old --zero mode)."""
    two = len(sensors) > 1
    lead_head = next(iter(sensors))
    axes = ss.axes
    setup = {"zero_residual_pT_per_s": None, "cal_factors": {},
             "raw_calib_message": "", "warnings": [], "heads": {}}
    session["setup"] = setup
    for head, sensor in sensors.items():
        sensor.connect()
        # one read is not enough to call a head unlocked (a re-zero right
        # after setup was refused once); a cold head still fails
        s = sensor.status()
        t_wait = clock.time() + LOCK_RECHECK_S
        while not (s["laser_lock"] and s["cellT_lock"]) \
                and clock.time() < t_wait:
            ctx.sleep(1.0)
            s = sensor.status()
        if not (s["laser_lock"] and s["cellT_lock"]):
            raise ConfigError(
                (f"{sensor.display}: " if two else "")
                + "sensor is not locked -- run the setup first "
                  f"(laser lock {s['laser_lock']}, cell temperature "
                  f"lock {s['cellT_lock']}, still so after "
                  f"{LOCK_RECHECK_S:g} s)")
    # zeroing measures through the modulation, and roles may not survive
    # an earlier setup -- send them again
    apply_sync_roles(ctx, sensors, resolve_sync(ctx, ss, sensors), setup,
                     master_head=ss.master_head)
    lead_before = None
    for head, sensor in sensors.items():
        stage(ctx, [head], "field zero", "running",
              f"settle < {sensor.zero_cond:g} pT/s")
        residual = sensor.zero(
            timeout=ss.lock_timeout,
            progress=zero_progress_for(
                ctx, f"{sensor.display}: " if two else ""))
        ctx.say("zero", (f"{sensor.display} " if two else "")
                + (f"{residual:g} pT/s worst axis"
                   if residual is not None else "settled"))
        if sensor.zero_off_failed:
            raise SensorError(
                (f"{sensor.display}: " if two else "")
                + f"field zero off failed ({sensor.zero_off_failed}) "
                "-- the calibration would be suspect; fix the link "
                "and re-zero")
        stage(ctx, [head], "field zero", "passed",
              f"{residual:g} pT/s worst axis" if residual is not None
              else "settled", residual=residual)
        stage(ctx, [head], "calibrate", "running",
              "waiting for the Calib reply")
        factors, raw, cal_state = sensor.calibrate_factors()
        stage(ctx, [head], "calibrate", factors_state(factors, axes),
              factors_to_flag(factors) or raw, factors=factors)
        per = {"zero_residual_pT_per_s": residual,
               "cal_factors": factors, "raw_calib_message": raw,
               "calib_state_at_send": cal_state,
               "zero_shift_pT": None, "warnings": []}
        setup["heads"][head] = per
        check_factors(ctx, ss, per, factors, raw,
                      label=sensor.display if two else "", expected=axes)
        setup["warnings"].extend(per["warnings"])
        ctx.say("cal", (f"{sensor.display} " if two else "") + "factors: "
                + (factors_to_flag(factors) or "(none parsed)"))
        if two and head == lead_head:
            lead_before = field_mean_pT(ctx, sensor, axes[0])
    if two and lead_before is not None:
        after = field_mean_pT(ctx, sensors[lead_head], axes[0])
        if after is not None:
            shift = after - lead_before
            setup["heads"][lead_head]["zero_shift_pT"] = round(shift, 1)
            ctx.say("zero", f"zeroing the other sensor(s) moved "
                            f"{sensors[lead_head].display} by {shift:+.1f} pT")
            if abs(shift) > ss.zero_shift_warn:
                msg = (f"zeroing the other sensor(s) moved "
                       f"{sensors[lead_head].display} "
                       f"by {shift:+.1f} pT (over zero_shift_warn "
                       f"{ss.zero_shift_warn:g}) -- move the heads "
                       f"further apart, or accept the offset knowingly")
                setup["heads"][lead_head]["warnings"].append(msg)
                setup["warnings"].append(msg)
                ctx.warn(msg)
    wait_for_cell_lock(ctx, sensors)
    lead = setup["heads"][lead_head]
    setup["zero_residual_pT_per_s"] = lead["zero_residual_pT_per_s"]
    setup["cal_factors"] = lead["cal_factors"]
    setup["raw_calib_message"] = lead["raw_calib_message"]
    return {h: setup["heads"][h]["cal_factors"] for h in sensors}


def standby(ctx, sensors, session):
    """Reboot every head dark (laser + heater off) and verify it.

    "command sent" and "dark confirmed" are different facts: quiet =
    unverified (check LED1 by eye), laser still ON = did not take.
    Returns the per-head state dict.
    """
    session["standby"] = {}
    still_on = []
    for head, sensor in sensors.items():
        stage(ctx, [head], "standby", "running")
        sensor.connect()
        sensor.standby()
        state = {"command_sent": True, "dark_confirmed": False,
                 "readback": None, "blank_frame": False}
        session["standby"][head] = state
        ctx.sleep(STANDBY_VERIFY_WAIT_S)
        try:
            snap = sensor.status()
        except SensorError as e:
            state["readback"] = f"no response ({e})"
            continue
        state["readback"] = {k: bool(snap.get(k)) for k in
                             ("laser_on", "laser_lock", "cellT_lock")}
        if snap.get("laser_on"):
            still_on.append(head)
            stage(ctx, [head], "standby", "failed", "laser still ON")
        elif frame_is_blank(snap):
            # a blank frame is indistinguishable from a dark sensor on
            # flags alone -- it confirms nothing (2026-08-19)
            state["blank_frame"] = True
            stage(ctx, [head], "standby", "warn",
                  "blank frame: confirm LED1 is off by eye")
        else:
            state["dark_confirmed"] = True
            stage(ctx, [head], "standby", "passed", "dark confirmed")
    two = len(sensors) > 1
    bits = []
    for head, state in session["standby"].items():
        bits.append((f"{sensors[head].display}: " if two else "")
                    + ("dark confirmed (laser off)"
                       if state["dark_confirmed"]
                       else "reboot sent, no readback -- confirm LED1 "
                            "is off by eye"))
    if still_on:
        names = ", ".join(sensors[h].display for h in still_on) if two \
            else "the sensor"
        raise SensorAbort(
            f"standby did NOT take: {names} still report(s) the laser "
            "ON -- not dark (a degauss window needs it dark). Check "
            "the unit and retry standby")
    ctx.say("standby", "; ".join(bits) + " -- wake later with a setup")
    return session["standby"]


def status_frames(sensors):
    """[(head, sensor, frame, blank)] for every head (read-only)."""
    out = []
    for head, sensor in sensors.items():
        sensor.connect()
    for head, sensor in sensors.items():
        s = sensor.status()
        out.append((head, sensor, s, frame_is_blank(s)))
    return out
