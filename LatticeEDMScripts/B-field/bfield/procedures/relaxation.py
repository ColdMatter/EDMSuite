"""Evening degauss-relaxation program: degauss the shield, then take
timed relaxation records with sensor standby between points.

Moved from IROP_research/scripts/degauss_relax.py. The old program ran
degauss.py and qzfm_serial.py as child processes; here the same steps
are in-process calls (DegaussSession, QZFMSession), each with its own
child Context so its ledger and log stay its own. Only the recorder is
still a separate process (inside QZFMSession.record()).

S1 operator confirms 19V OFF (unpowered head has no COM port). S2
degauss pass(es) (parallel: vendor recipe 8 A / 15 Hz; series: 3 Hz,
strong adds a 5 Hz pass first). S3 operator unplugs banana leads
(closed loop couples vibration), turns 19V ON. S3b short checkout
record (checkout_seconds 0 skips). S4 sensor setup. S5 one record at
t0 + 5, 20, 45, 80, 115 min (default); long idle gaps go to serial
standby (no_sleep keeps it on). S6 standby, then 19V OFF.

t0 = taper end of the LAST degauss pass (its degauss_session.json);
late points are recorded, not hidden. Each point re-runs Field Zero +
Calibrate so points are comparable; points after a fresh wake are
marked woke_from_standby (different thermal history).

Output (unchanged): <bucket>/rx_<stamp>/ with dg_passN/dg_<stamp>/,
setup/, checkout/, pN_t<M>m/Rnn_HHMM/, wake_pN/, standby_pN/,
standby_final/, degauss_relax_session.json, relax_summary.txt; the
operations log is <bucket>/relax_session.log (appended, as before).
Exit codes: 0 completed / 13 config / 20 aborted / 130 stop / 1 crash.
"""

from __future__ import annotations

import contextlib
import dataclasses
import glob
import json
import math
import os
import traceback
from dataclasses import dataclass
from datetime import datetime

from bfield import clock
from bfield.constants import (DG_RECIPE_AMP_A, DG_RECIPE_FREQ_HZ,
                              DG_TAPER_FALLBACK_S, dg_cycle_plan_s)
from bfield.errors import AbortRun, BFieldError, ConfigError, Stopped
from bfield.io.files import sanitize, write_json_atomic
from bfield.operations.degauss_cycle import (DegaussSession, DegaussSettings,
                                             validate_settings)
from bfield.procedures import qzfm_session
from bfield.procedures.qzfm_session import QZFMPlan, QZFMSession
from bfield.ui import Context

DEFAULT_AT_MIN = "5,20,45,80,115"   # spans the after-effect window
DEFAULT_SECONDS = 900.0   # published plateau points are all 900 s;
                          # shorter points cannot join that series
SEG_CONVENTION_S = 200.0  # the window the 0.05 Hz statistic averages
PLATEAU_MIN_S = 600.0     # floor for the 0.05 Hz statistic

# Sensor duty cycle: laser/heater hours are the lifetime, so idle gaps
# go to standby when the off stretch is at least MIN_OFF (cold setup ~3 min)
DEFAULT_SLEEP_GAP_MIN = 0.0         # 0 = every gap long enough to be worth it
DEFAULT_WAKE_LEAD_MIN = 5.0         # wake this early (cold cell + zero + cal)
MIN_OFF_MIN = 5.0                   # shorter than this, sleeping is pointless
DEFAULT_CHECKOUT_S = 20.0           # post-degauss health read length
ON_TIME_WARN_H = 4.0                # above this, say so before driving
DEFAULT_STRONG_AMP_A = 3.2          # series: 5 Hz pass ceiling on N=8 (07-30)
DEFAULT_SOAK_S = 15.0

# Per-winding degauss defaults. "strong" = the winding takes the
# 5 Hz + 3 Hz treatment.
DEFAULT_WINDING = "parallel"        # standing since 2026-08-13
WINDING_DEFAULTS = {
    # 300 s taper = vendor recipe; taper 60 is the retreat that has
    # never failed
    "parallel": {"amp": DG_RECIPE_AMP_A, "freq": DG_RECIPE_FREQ_HZ,
                 "taper": 300.0, "coil": "parallel4",
                 "strong": False},
    "series": {"amp": 2.6,          # routine 3 Hz acceptance on N=8
               "freq": 3.0,         # the finish is pinned to this
               "taper": 300.0, "coil": "N8", "strong": True},
}

LONG_POINT_S = 300.0                # above this a +/-1 V window is a risk
# Per-point setup before the DAQ opens, by head count; duty planning must
# count it. Measured upper ends: 1 head 44-45 s (08-17), 2 heads 109-130 s.
POINT_SETUP_MIN = {1: 0.8, 2: 2.2}
WAIT_TICK_S = 1.0
WAIT_SAY_S = 60.0


def point_setup_min(device_name):
    """Minutes a point spends on its own setup before the DAQ opens."""
    heads = len([d for d in str(device_name or "").split(",")
                 if d.strip()]) or 1
    return POINT_SETUP_MIN.get(heads, max(POINT_SETUP_MIN.values()))


def require(cond, msg):
    if not cond:
        raise ConfigError(msg)


# ------------------------------------------------------------- settings

@dataclass
class RelaxSettings:
    """The old degauss_relax flags, one field each."""
    dg_port: str | None = None           # None = config [degausser] port
    device_name: str | None = None       # None = config [qzfm.heads] a
    winding: str = DEFAULT_WINDING
    amp: float | None = None             # None = winding default
    freq: float | None = None
    strong: bool = False
    strong_amp: float = DEFAULT_STRONG_AMP_A
    soak: float = DEFAULT_SOAK_S
    taper: float | None = None
    coil: str | None = None
    at: str = DEFAULT_AT_MIN
    every: float | None = None
    span: float = 12.0
    checkout_seconds: float = DEFAULT_CHECKOUT_S
    seconds: float = DEFAULT_SECONDS
    sleep_between: float = DEFAULT_SLEEP_GAP_MIN
    no_sleep: bool = False
    wake_lead: float = DEFAULT_WAKE_LEAD_MIN
    axes: str = "z"
    map: str | None = None               # None = config [daq] two_head_map
    gain: str = "1x"
    fs: float = 1200.0
    vrange: str = "2"
    outdir: str | None = None            # parent; None = the next dated
                                         # run folder (or the relax bucket)
    name: str | None = None              # dated folder name; None =
                                         # Relaxation
    notes: str | None = None
    confirm: bool = False
    leads_stay: bool = False
    sensor_powered: bool = False
    allow_missing_factors: bool = False
    accept_flagged_degauss: bool = False
    accept_unverified_standby: bool = False
    # resolved by validate()
    at_min: list | None = None
    sleep_gap: float | None = None


# --------------------------------------------------------- plan helpers

def parse_at(text):
    """Schedule minutes after t0: positive, strictly increasing."""
    out = []
    for part in str(text).split(","):
        part = part.strip()
        if not part:
            continue
        try:
            out.append(float(part))
        except ValueError:
            raise ConfigError(f"--at entry {part!r} is not a number")
    require(out, "--at is empty; give minutes like \"5,20,45,80,115\"")
    require(all(math.isfinite(t) and t > 0 for t in out),
            "--at minutes must be positive numbers")
    require(all(b > a for a, b in zip(out, out[1:])),
            "--at minutes must strictly increase")
    return out


def ladder_at(every_min, span_h):
    """A fixed-interval schedule: every_min, 2x, ... up to span_h.

    The default --at is uneven on purpose (chases the after-effect);
    an even ladder samples slow overnight drift instead.
    """
    require(every_min > 0 and math.isfinite(every_min),
            "--every has to be a positive number of minutes")
    require(span_h > 0 and math.isfinite(span_h),
            "--span has to be a positive number of hours")
    span_min = span_h * 60.0
    require(every_min <= span_min,
            f"--every {every_min:g} min does not fit in --span {span_h:g} h")
    n = int(span_min / every_min + 1e-9)
    return [every_min * k for k in range(1, n + 1)]


def fmt_minutes(minutes):
    """Format as hours at or above 60 min, else minutes."""
    return (f"{minutes / 60.0:.1f} h" if minutes >= 60.0
            else f"{minutes:.0f} min")


def point_dir_name(index, minutes):
    tag = f"{minutes:g}".replace(".", "p")
    return f"p{index}_t{tag}m"


def build_passes(s):
    """(freq_Hz, amp_A) per degauss pass; the last taper sets the final
    state. strong = series treatment: 5 Hz pass, then the 3 Hz finish."""
    if s.strong:
        return [(5.0, s.strong_amp), (3.0, s.amp)]
    return [(s.freq, s.amp)]


def plan_duty_cycle(at_min, seconds, sleep_gap_min, wake_lead_min,
                    setup_min=0.0):
    """Decide before anything is driven when the sensor sleeps.

    sleep_gap_min None = never sleep; 0 = sleep in every gap that leaves
    MIN_OFF min off after the wake lead. Per point: wake_at_min (None =
    already on), sleep_after, gap_after_min (negative = next point late,
    recorded not hidden).
    """
    rec_min = seconds / 60.0 + setup_min
    duty = [{"index": i + 1, "t_min": t, "wake_at_min": None,
             "sleep_after": False, "gap_after_min": None}
            for i, t in enumerate(at_min)]
    if not duty:
        return duty
    on = sleep_gap_min is not None
    lead = wake_lead_min if on else 0.0
    # point 1 is a cold start after 19V came back on; without duty
    # cycling the wake is immediate
    duty[0]["wake_at_min"] = max(0.0, at_min[0] - lead) if on else 0.0
    for i in range(len(duty) - 1):
        gap = at_min[i + 1] - (at_min[i] + rec_min)
        duty[i]["gap_after_min"] = gap
        if on and gap > sleep_gap_min and gap - lead >= MIN_OFF_MIN:
            duty[i]["sleep_after"] = True
            duty[i + 1]["wake_at_min"] = at_min[i + 1] - lead
    return duty


def duty_on_time_min(duty, at_min, seconds, setup_min=0.0):
    """Planned sensor on-time in minutes for the S4-S6 stretch."""
    if not duty:
        return 0.0
    rec_min = seconds / 60.0 + setup_min
    total = 0.0
    awake_from = duty[0]["wake_at_min"]
    for i, d in enumerate(duty):
        end = at_min[i] + rec_min
        if d["sleep_after"] or i == len(duty) - 1:
            total += max(0.0, end - awake_from)
            nxt = duty[i + 1]["wake_at_min"] if i + 1 < len(duty) else None
            awake_from = nxt if nxt is not None else end
    return total


CHECKLIST = (
    "QZFM 19V power switch OFF -- electronics dead, not serial standby",
    "endcaps closed, winding connected with banana plugs",
    "QuSpin GUI fully closed (it must stay closed after you turn 19V on)",
    "DG Control / DTLG closed (the DG COM port is exclusive)",
    "stay until the degauss ends -- you will be prompted to unplug the "
    "banana leads and turn the 19V switch ON; between records the sensor "
    "sleeps itself; at the end turn 19V OFF",
)


def on_time_line(at_min, seconds, sleep_gap_min, wake_lead_min,
                 setup_min=0.0):
    """The line the operator reads against the PI's constraint."""
    duty = plan_duty_cycle(at_min, seconds, sleep_gap_min, wake_lead_min,
                           setup_min)
    on_h = duty_on_time_min(duty, at_min, seconds, setup_min) / 60.0
    span_h = (at_min[-1] + seconds / 60.0) / 60.0
    naps = sum(1 for d in duty if d["sleep_after"])
    how = (f"{naps} standby nap(s) inside a {span_h:.1f} h span" if naps
           else "no naps -- the sensor stays on the whole time")
    return (f"sensor on-time ~{on_h:.1f} h: {how}; serial standby after "
            f"the last point, then turn 19V OFF (laser/heater hours)"), on_h


def checklist_lines(at_min, seconds, sleep_gap_min, wake_lead_min,
                    setup_min=0.0):
    line, on_h = on_time_line(at_min, seconds, sleep_gap_min,
                              wake_lead_min, setup_min)
    items = [*CHECKLIST, line]
    if on_h > ON_TIME_WARN_H:
        items.append(f"^ that exceeds {ON_TIME_WARN_H:g} h -- clear it with "
                     f"the PI, or lower --sleep-between / trim --at")
    return items


# ------------------------------------------------ what the lab resolves

def resolve(s, lab):
    """Fill the None placeholders from the winding and the lab config."""
    if s.dg_port is None:
        s.dg_port = lab.config.section("degausser").get("port")
    if s.device_name is None:
        # the lowest-numbered sensor in config [[sensors]]
        from bfield.sensors import load_sensors
        known = load_sensors(lab.config)
        s.device_name = known[0].usb_serial if known else None
    wd = WINDING_DEFAULTS[s.winding]
    if s.amp is None:
        s.amp = wd["amp"]
    if s.freq is None:
        s.freq = wd["freq"]
    if s.taper is None:
        s.taper = wd["taper"]
    if s.coil is None:
        s.coil = wd["coil"]
    return s


def qzfm_plan(s, lab, outdir, seconds=None, notes=None, mode="setup"):
    """The QZFMPlan one wake / point / standby step runs with."""
    return QZFMPlan.from_config(
        lab.config, device_name=s.device_name, axes=s.axes, gain=s.gain,
        map=s.map, fs=s.fs, vrange=s.vrange, outdir=outdir, notes=notes,
        seconds=seconds if seconds is not None else s.seconds,
        # a series point without calibration factors is a failure, not
        # data (the lenient default is for exploratory use only)
        require_factors=(mode in ("setup", "run")
                         and not s.allow_missing_factors))


def degauss_settings(s, freq, amp, outdir):
    return DegaussSettings(mode="run", port=s.dg_port, amp=amp, freq=freq,
                           soak=s.soak, taper=s.taper,
                           max_amp=float(DEFAULT_MAX_AMP), outdir=outdir,
                           coil=s.coil, confirm_checklist=True)


DEFAULT_MAX_AMP = DG_RECIPE_AMP_A


def plan_dict(obj):
    """A settings object as a json-able dict (the ledger's stand-in for
    the old child command line)."""
    return sanitize(dataclasses.asdict(obj))


# ------------------------------------------------------------ validation

def validate(ctx, lab, s, mode):
    """The old validate(): resolve defaults, refuse bad plans, warn."""
    require(mode in ("plan", "go"), "pick exactly one mode: --plan or --go")
    require(s.winding in WINDING_DEFAULTS,
            f"--winding must be one of {sorted(WINDING_DEFAULTS)}")
    resolve(s, lab)
    wd = WINDING_DEFAULTS[s.winding]
    if s.every is not None:
        require(s.at == DEFAULT_AT_MIN,
                "--every builds the schedule, so --at must be left alone; "
                "pass one or the other")
        s.at_min = ladder_at(s.every, s.span)
        s.at = ",".join(f"{t:g}" for t in s.at_min)
    else:
        s.at_min = parse_at(s.at)
    require(s.seconds > 0 and math.isfinite(s.seconds),
            "--seconds must be positive")
    require(s.checkout_seconds >= 0 and math.isfinite(s.checkout_seconds),
            "--checkout-seconds cannot be negative (0 skips the check)")
    # short points still measure field/drift/white band, but not the
    # 0.05 Hz statistic (200 s windows need >= 600 s)
    if s.seconds >= PLATEAU_MIN_S:
        ctx.say("points", f"{s.seconds:g} s per point: long enough for the "
                          f"0.05 Hz plateau statistic")
    else:
        ctx.warn(f"{s.seconds:g} s per point: too short for the 0.05 Hz "
                 f"plateau statistic ({SEG_CONVENTION_S:g} s windows need "
                 f"{PLATEAU_MIN_S:g} s+; published points are all 900 s). "
                 f"Field, drift and white band still measured; --seconds "
                 f"900 joins the series.")
    require(s.wake_lead >= 0 and math.isfinite(s.wake_lead),
            "--wake-lead cannot be negative")
    require(s.sleep_between >= 0 and math.isfinite(s.sleep_between),
            "--sleep-between cannot be negative")
    # one resolved knob from here on: None means "never sleep"
    s.sleep_gap = None if s.no_sleep else s.sleep_between
    # the window must hold the drift: a clipped point is lost, not weaker
    if str(s.vrange) == "1" and s.seconds >= LONG_POINT_S:
        ctx.warn(f"--vrange 1 with {s.seconds:g} s points: a 900 s record "
                 f"drifted ~200 pT on 2026-08-07 and clipped the +/-1 V "
                 f"window at t=199 s -- use --vrange 2 unless this shield "
                 f"is known quieter")
    for label, v in (("--amp", s.amp), ("--strong-amp", s.strong_amp),
                     ("--taper", s.taper), ("--soak", s.soak),
                     ("--freq", s.freq), ("--fs", s.fs)):
        require(v is not None and math.isfinite(v) and v > 0,
                f"{label} must be positive")
    require(not s.strong or wd["strong"],
            f"--strong has no meaning on the {s.winding} winding "
            f"(it is the series 5 Hz + 3 Hz treatment)")
    if s.winding == "series":
        require(s.freq == wd["freq"] or s.strong,
                f"--freq other than {wd['freq']:g} needs --strong (the "
                f"series final pass must be {wd['freq']:g} Hz -- it sets "
                f"the final state)")
    elif s.amp >= DG_RECIPE_AMP_A and s.taper > DG_TAPER_FALLBACK_S:
        ctx.say("degauss", f"{s.amp:g} A / {s.taper:g} s taper: the vendor "
                           f"recipe -- not deterministic (faulted 4/4 at "
                           f"~46 s on 08-13, completed twice on 08-14); a "
                           f"fault ends the evening before any point. "
                           f"--taper {DG_TAPER_FALLBACK_S:g} has never "
                           f"failed")
    require(s.dg_port, "--dg-port is required (COMx; or config "
                       "[degausser] port)")
    require(s.device_name, "no sensor: --sensors / --device-name, or a "
                           "[[sensors]] entry in the config")
    if s.confirm and s.sensor_powered:
        # contradictory flags: warned, not refused -- which one is true
        # is a bench fact
        ctx.warn("--confirm asserts the 19V switch is OFF for the degauss; "
                 "--sensor-powered skips the pause where it goes back ON. "
                 "Unless the switch is thrown between them, one is false: "
                 "the pulse runs over a powered head, or the wake finds no "
                 "COM port and the degauss is spent for no points")
    preflight(lab, s)


def preflight(lab, s):
    """Validate the wake, point and degauss plans now, before the degauss
    is spent (the old --validate-only children; nothing is opened)."""
    parent = s.outdir or lab.bucket("relax")
    for mode, what in (("setup", "the wake"), ("run", "a relaxation point"),
                       ("standby", "the standby")):
        p = qzfm_plan(s, lab, os.path.join(parent, "_preflight"),
                      mode=mode)
        try:
            qzfm_session.validate(p, mode)
        except ConfigError as e:
            raise ConfigError(f"the sensor session refuses {what}: {e} -- "
                              f"fix the flags now, before the degauss is "
                              f"spent")
    for freq, amp in build_passes(s):
        try:
            validate_settings(degauss_settings(s, freq, amp, parent))
        except ConfigError as e:
            raise ConfigError(f"the degauss pass {amp:g} A / {freq:g} Hz is "
                              f"refused: {e}")


# ------------------------------------------------------ reading records

def read_standby_darkness(step_dir):
    """(all_dark, detail) from a standby step's session json.

    Exit 0 with the laser-off readback unverified must not be read as
    confirmed dark. (None, reason) when the record is missing."""
    path = next((c for c in
                 (os.path.join(step_dir, "qzfm_standby_session.json"),
                  os.path.join(step_dir, "qzfm_session.json"))
                 if os.path.exists(c)), None)
    if path is None:
        return None, f"no standby session json under {step_dir}"
    try:
        with open(path, encoding="utf-8") as fh:
            doc = json.load(fh)
    except (OSError, ValueError) as e:
        return None, f"unreadable standby session ({e})"
    standby = doc.get("standby") or {}
    if not standby:
        return None, "no standby record in the session json"
    undark = sorted(h for h, rec in standby.items()
                    if not (rec or {}).get("dark_confirmed"))
    if undark:
        return False, ("darkness not confirmed for head(s) "
                       + ", ".join(undark))
    return True, "all heads confirmed dark"


def read_degauss_t0(pass_dir, accept_flagged=False):
    hits = sorted(glob.glob(os.path.join(pass_dir, "dg_*",
                                         "degauss_session.json")))
    if not hits:
        raise AbortRun(f"no degauss_session.json under {pass_dir} -- "
                       f"cannot anchor t0")
    with open(hits[-1], encoding="utf-8") as fh:
        doc = json.load(fh)
    cycles = doc.get("cycles") or []
    cyc = cycles[-1] if cycles else {}
    problems = []
    if doc.get("outcome") != "completed":
        problems.append(f"session outcome {doc.get('outcome')!r}")
    if cyc.get("outcome") != "ok":
        problems.append(f"final cycle outcome {cyc.get('outcome')!r}")
    t0_iso = doc.get("t0")
    if not t0_iso:
        t0_iso = cyc.get("end")
        problems.append("no t0 recorded (unverified pass; anchoring on "
                        "the cycle end time)")
    if not t0_iso:
        raise AbortRun(f"degauss session under {pass_dir} has neither t0 "
                       f"nor a cycle end time -- nothing to anchor the "
                       f"series on")
    if problems and not accept_flagged:
        raise AbortRun("degauss is not measurement-ready: "
                       + "; ".join(problems)
                       + " -- redo the degauss, or override with "
                       "--accept-flagged-degauss")
    return datetime.fromisoformat(t0_iso), hits[-1], problems


def _point_metas(step_dir):
    return [m for m in glob.glob(os.path.join(step_dir, "*", "*.meta.json"))
            if "_guard" not in m]


def daq_start_min(step_dir, t0):
    """Minutes from t0 to when the DAQ actually opened (None: no record)."""
    metas = _point_metas(step_dir)
    if len(metas) != 1:
        return None
    try:
        with open(metas[0], encoding="utf-8") as fh:
            start = json.load(fh).get("start_iso")
        return (datetime.fromisoformat(start) - t0).total_seconds() / 60.0
    except (OSError, ValueError, TypeError):
        return None


def read_run_health(step_dir):
    """(ok, detail) from the record a point left behind: railed or flat
    channels (the miswiring signature, invisible in the exit code)."""
    metas = _point_metas(step_dir)
    if len(metas) != 1:
        return None, (f"expected one record under {step_dir}, found "
                      f"{len(metas)}")
    try:
        with open(metas[0], encoding="utf-8") as fh:
            meta = json.load(fh)
    except (OSError, ValueError) as e:
        return None, f"unreadable meta ({e})"
    rails = {k: v for k, v in (meta.get("rail_fraction") or {}).items() if v}
    flat = {k: v for k, v in (meta.get("flat_ptp_V") or {}).items()
            if v is not None and v <= 0}
    bits = []
    if rails:
        bits.append("railed: " + ", ".join(f"{k} {v:.1%}"
                                           for k, v in sorted(rails.items())))
    if flat:
        bits.append("flat: " + ", ".join(sorted(flat)))
    if meta.get("exit_code") not in (0,):
        bits.append(f"exit {meta.get('exit_code')} ({meta.get('status')})")
    if bits:
        return False, "; ".join(bits)
    chans = ", ".join(sorted((meta.get("channel_map")
                              or meta.get("channels") or {})))
    return True, f"{chans} reading, nothing railed or flat"


# -------------------------------------------------------------- ledger

def write_session(outdir, session, ctx=None):
    """Atomic checkpoint; a failed write is a warning, not an abort."""
    if outdir is None:
        return
    try:
        write_json_atomic(os.path.join(outdir, "degauss_relax_session.json"),
                          sanitize(session))
    except OSError as e:
        if ctx is not None:
            ctx.warn(f"session json write failed: {e}")


def write_summary(outdir, session):
    if outdir is None:
        return
    with contextlib.suppress(OSError):
        path = os.path.join(outdir, "relax_summary.txt")
        with open(path, "w", newline="\n", encoding="utf-8") as fh:
            fh.write(f"degauss_relax {session['started']}  "
                     f"outcome: {session['outcome']}\n")
            fh.write(f"t0: {session['t0']}\n")
            dc = session["duty_cycle"]
            fh.write("sensor on-time: %.2f h actual, %.2f h planned "
                     "(%d nap(s))\n" % (dc["on_time_min_actual"] / 60.0,
                                        dc["on_time_min_planned"] / 60.0,
                                        dc["naps_planned"]))
            for st in session["steps"]:
                fh.write("step %-14s exit %-4s %s\n" % (
                    st["step"], st.get("exit_code"), st.get("outcome")))
            for p in session["points"]:
                fh.write("%-12s sched %6.4g min  actual %6.4g min  "
                         "exit %-4s %s\n" % (
                             p["dir_name"], p["t_sched_min"],
                             p["t_actual_min"] if p["t_actual_min"]
                             is not None else float("nan"),
                             p.get("exit_code"), p.get("outcome")))


# ------------------------------------------------------------ the program

class Relaxation:
    """One evening. `plan_text()` is the old --plan, `go()` the old --go."""

    def __init__(self, ctx, lab, settings):
        self.ctx = ctx
        self.lab = lab
        self.s = settings
        self.outdir = None
        self.session = None
        self.state = {}
        self.t_start = clock.monotonic()

    # -- small helpers --------------------------------------------------

    def warn(self, msg):
        self.ctx.warn(msg, (self.session or {}).setdefault("warnings", [])
                      if self.session is not None else None)

    def rel(self, path):
        """As the old ledger: relative to the data tree's parent."""
        try:
            return os.path.relpath(path, os.path.dirname(self.lab.data_root))
        except ValueError:
            return path

    def setup_min(self):
        return point_setup_min(self.s.device_name)

    def duty(self):
        s = self.s
        return plan_duty_cycle(s.at_min, s.seconds, s.sleep_gap,
                               s.wake_lead, self.setup_min())

    def step(self, name, fn, plan=None):
        """Run one step, record it, checkpoint the ledger after it."""
        rec = {"step": name, "started": clock.iso_now(), "exit_code": None,
               "outcome": None}
        if plan is not None:
            # stands in for the old child command line
            rec["plan"] = plan
        self.session["steps"].append(rec)
        try:
            rc = fn()
            rec["exit_code"] = rc
            rec["outcome"] = "ok" if rc == 0 else f"exit {rc}"
            return rc
        except BFieldError as e:
            rec["outcome"] = f"{type(e).__name__}: {e}"
            raise
        finally:
            rec["ended"] = clock.iso_now()
            write_session(self.outdir, self.session, self.ctx)

    def _sub_code(self, rc, desc):
        """A sub-session that swallowed a stop reports 130; pass it on."""
        if rc == 130:
            if self.ctx.stop_requested():
                raise Stopped(f"stopped by the operator during {desc}")
            raise KeyboardInterrupt
        return rc

    def run_qzfm(self, mode, step_dir, seconds=None, notes=None, ctx=None):
        """One sensor sub-session (setup / run / standby) -> exit code."""
        p = qzfm_plan(self.s, self.lab, step_dir, seconds=seconds,
                      notes=notes, mode=mode)
        self.ctx.say("child", f"sensor {mode} -> {self.rel(step_dir)}")
        sess = QZFMSession((ctx or self.ctx).child(), self.lab, p)
        rc = {"setup": sess.setup, "run": sess.record,
              "standby": sess.standby}[mode]()
        return self._sub_code(rc, f"sensor {mode}")

    def qzfm_step(self, name, mode, step_dir, seconds=None, notes=None,
                  ctx=None):
        p = plan_dict(qzfm_plan(self.s, self.lab, step_dir, seconds=seconds,
                                notes=notes, mode=mode))
        p["mode"] = mode
        return self.step(name, lambda: self.run_qzfm(
            mode, step_dir, seconds, notes, ctx), plan=p)

    def run_degauss_pass(self, freq, amp, pass_dir):
        ds = degauss_settings(self.s, freq, amp, pass_dir)
        self.ctx.say("child", f"degauss {amp:g} A at {freq:g} Hz -> "
                              f"{self.rel(pass_dir)}")
        rc = DegaussSession(self.ctx.child(), self.lab, ds).run()
        return self._sub_code(rc, "the degauss")

    # -- duty ledger ----------------------------------------------------

    def mark_awake(self):
        if self.state.get("awake_since") is None:
            # monotonic: a stepped wall clock once booked 17.71 h of
            # on-time for a 1.06 h run (2026-08-19)
            self.state["awake_since"] = clock.monotonic()

    def mark_asleep(self):
        """Bank the elapsed awake span; safe to call when asleep."""
        started = self.state.get("awake_since")
        if started is None:
            return
        dc = self.session["duty_cycle"]
        dc["on_time_min_actual"] = round(
            dc["on_time_min_actual"]
            + (clock.monotonic() - started) / 60.0, 3)
        self.state["awake_since"] = None
        alive_min = (clock.monotonic() - self.t_start) / 60.0
        if dc["on_time_min_actual"] > alive_min + 1.0:
            dc["on_time_min_raw"] = dc["on_time_min_actual"]
            dc["on_time_min_actual"] = round(alive_min, 3)
            self.warn(f"on-time ledger read {dc['on_time_min_raw']:.1f} min "
                      f"but the run has been alive {alive_min:.1f} min -- "
                      f"clamped to the run length; raw figure in "
                      f"on_time_min_raw")

    # -- the operator ---------------------------------------------------

    def confirm_checklist(self):
        s = self.s
        if s.confirm:
            return
        self.ctx.say("check", "--- before starting ---")
        for item in checklist_lines(s.at_min, s.seconds, s.sleep_gap,
                                    s.wake_lead, self.setup_min()):
            self.ctx.say("check", f"  - {item}")
        self.ctx.ask("19V switch is OFF, and the rest is confirmed?",
                     "turn the QZFM 19V switch OFF, then rerun --go -- "
                     "nothing driven")

    def after_degauss_pause(self):
        """Hand steps that cannot be scripted: open the winding loop and
        restore sensor power. After this, S4-S6 run unattended."""
        s = self.s
        bits = []
        if s.leads_stay:
            self.warn("banana leads stay plugged: the closed winding loop "
                      "couples vibration into the shield -- a known "
                      "systematic, recorded")
        else:
            bits.append("UNPLUG the banana leads (a closed winding loop "
                        "couples vibration into the shield)")
        if s.sensor_powered:
            self.warn("19V switch asserted already ON (--sensor-powered) -- "
                      "setup will start without waiting")
        else:
            bits.append("turn the QZFM 19V switch ON and wait until Windows "
                        "enumerates the COM port")
        if not bits:
            return
        try:
            self.ctx.pause("Degauss done. Before measurements: "
                           + " ".join(f"{i}. {b}"
                                      for i, b in enumerate(bits, 1)))
        except ConfigError:
            raise ConfigError(
                "no terminal for the post-degauss pause -- pass --leads-stay "
                "and --sensor-powered to skip it")

    def power_off_pause(self, shut_down=True):
        """Ask for the 19V switch OFF; returns what actually happened (a
        prompt nobody saw must not be filed as "operator 19V off")."""
        tail = ("(laser and heater are already in standby)" if shut_down
                else "(the standby was NOT confirmed -- check LED1 first)")
        msg = f"records finished: turn the QZFM 19V switch OFF {tail}"
        # a cleanup context: a GUI Abort must not skip this prompt
        cleanup = Context(self.ctx.ui, self.ctx.config)
        cleanup.events = self.ctx.events
        try:
            cleanup.pause(msg)
        except ConfigError:
            self.warn(msg + " -- no terminal, not waiting")
            return "not asked: no terminal"
        except EOFError:
            self.warn("no answer at the 19V prompt (stdin closed) -- the "
                      "switch may still be ON")
            return "not answered: stdin closed"
        return "operator 19V off"

    # -- waits ----------------------------------------------------------

    def wait_until(self, t0, minutes, what="next point"):
        """Interruptible wait until t0 + minutes (wall clock)."""
        target = t0.timestamp() + minutes * 60.0
        last_say = -1e18
        while True:
            self.ctx.check_stop()
            now = clock.time()
            if now >= target:
                return
            if now - last_say >= WAIT_SAY_S:
                self.ctx.say("wait", f"{what} at t0+{minutes:g} min "
                                     f"({(target - now) / 60.0:.1f} min to "
                                     f"go)")
                last_say = now
            self.ctx.sleep(min(WAIT_TICK_S, target - now))

    # -- --plan ---------------------------------------------------------

    def plan_text(self):
        s = self.s
        say = self.ctx.say
        passes = build_passes(s)
        plan_s = dg_cycle_plan_s(s.soak, s.taper)
        say("plan", "S1: operator y/N that the QZFM 19V switch is OFF "
                    "(no standby -- an unpowered head has no COM port)")
        say("plan", f"S2 degauss passes ({plan_s:.0f} s each):")
        for i, (freq, amp) in enumerate(passes, start=1):
            say("plan", f"  p{i}: degauss run {amp:g} A, {freq:g} Hz, soak "
                        f"{s.soak:g} s, taper {s.taper:g} s, coil {s.coil}, "
                        f"port {s.dg_port} -> <out>/dg_pass{i}")
        say("plan", "S3: unplug banana leads, turn 19V ON, confirm")
        say("plan", f"then wake: sensor setup of {s.device_name} (axes "
                    f"{s.axes}, gain {s.gain}, fs {s.fs:g}, vrange "
                    f"{s.vrange}"
                    + ("" if s.allow_missing_factors
                       else ", factors required") + ") -> <out>/setup")
        duty = self.duty()
        if s.checkout_seconds > 0:
            say("plan", f"then a {s.checkout_seconds:g} s checkout: bring "
                        f"up, read, judge the channels, back to standby "
                        f"(~4 min of on-time)")
        else:
            say("plan", "--checkout-seconds 0: nothing touches the sensor "
                        "until the wake before the first point")
        say("plan", f"points ({s.seconds:g} s each):")
        for i, minutes in enumerate(s.at_min, start=1):
            d = duty[i - 1]
            marks = []
            if d["wake_at_min"] is not None and i > 1:
                marks.append(f"wake t0+{d['wake_at_min']:g}")
            if d["sleep_after"]:
                marks.append(f"standby {d['gap_after_min'] - s.wake_lead:.0f}"
                             f" min")
            gap = d["gap_after_min"]
            if gap is not None and gap < 0:
                marks.append(f"NEXT POINT STARTS ~{-gap:.0f} min LATE")
            tail = f"   [{'; '.join(marks)}]" if marks else ""
            say("plan", f"  t0+{minutes:>6g} min -> "
                        f"{point_dir_name(i, minutes)}{tail}")
        total_min = (len(passes) * plan_s) / 60.0 + s.at_min[-1] \
            + s.seconds / 60.0
        line, on_h = on_time_line(s.at_min, s.seconds, s.sleep_gap,
                                  s.wake_lead, self.setup_min())
        say("plan", f"whole program ~{total_min:.0f} min from start; {line}")
        if on_h > ON_TIME_WARN_H:
            self.ctx.warn(f"planned on-time {on_h:.1f} h is above "
                          f"{ON_TIME_WARN_H:g} h -- clear it with the PI, or "
                          f"lower --sleep-between (now {s.sleep_gap!r} min) "
                          f"/ trim --at")
        if any(d["gap_after_min"] is not None and d["gap_after_min"] < 0
               for d in duty):
            self.ctx.warn(f"the schedule overruns itself: {s.seconds:g} s "
                          f"of record plus ~{self.setup_min():g} min of "
                          f"per-point Field Zero + Calibrate does not fit "
                          f"between points, so they start late (recorded, "
                          f"not hidden) -- widen --every, or shorten "
                          f"--seconds and leave the published 900 s series")
        return 0

    # -- --go -----------------------------------------------------------

    def new_session(self):
        s = self.s
        duty = self.duty()
        return {"program": "degauss_relax",
                "software": "bfield.procedures.relaxation",
                "simulated": self.lab.sim,
                "started": clock.iso_now(), "mode": "go",
                "winding": s.winding,
                "settings": dict(sorted(plan_dict(s).items())),
                "duty_cycle": {
                    "sleep_gap_min": s.sleep_gap,
                    "wake_lead_min": s.wake_lead,
                    "naps_planned": sum(1 for d in duty if d["sleep_after"]),
                    "on_time_min_planned": round(
                        duty_on_time_min(duty, s.at_min, s.seconds,
                                         self.setup_min()), 2),
                    "on_time_min_actual": 0.0,
                    "plan": duty},
                "passes": [], "steps": [], "t0": None, "points": [],
                "warnings": [], "outcome": "crashed", "end_reason": None,
                "traceback": None, "events": self.ctx.events}

    def checkout(self, t0, duty):
        """Post-degauss health check: wake, short record, judge, standby.
        Returns True if the sensor was left AWAKE."""
        s = self.s
        cdir = os.path.join(self.outdir, "checkout")
        # set before the call: an interrupted checkout once left this
        # False and cleanup skipped standby with the laser on (08-19, H3)
        self.state["wake_attempted"] = True
        self.ctx.say("check", f"bringing the sensor up for a "
                              f"{s.checkout_seconds:g} s health read "
                              f"(checks the evening can be left alone)")
        rc = self.qzfm_step("checkout", "run", cdir,
                            seconds=s.checkout_seconds,
                            notes="checkout: post-degauss health, not a "
                                  "relaxation point")
        ok, detail = read_run_health(cdir)
        self.session["checkout"] = {"dir": self.rel(cdir), "exit_code": rc,
                                    "healthy": ok, "detail": detail}
        if rc != 0:
            raise AbortRun(f"post-degauss checkout exited {rc} ({detail}) "
                           f"-- sensor unfit for the night; t0 is stamped, "
                           f"so a fixed setup can still run against this "
                           f"degauss")
        if ok is False:
            raise AbortRun(f"the checkout recorded, but {detail} -- every "
                           f"point tonight would fail the same way")
        if ok is None:
            self.warn(f"checkout: {detail} -- the read cannot be judged, "
                      f"continuing on its exit code alone")
        else:
            self.ctx.say("check", f"sensor up and zeroed; {detail}")
        self.mark_awake()

        # sleep only if the wait is long enough to be worth the wake later
        first_wake = duty[0]["wake_at_min"]
        now_min = (clock.time() - t0.timestamp()) / 60.0
        gap = (first_wake if first_wake is not None else s.at_min[0]) \
            - now_min
        awake = True
        if s.sleep_gap is not None and gap >= MIN_OFF_MIN:
            sdir = os.path.join(self.outdir, "standby_checkout")
            rc = self.qzfm_step("standby_checkout", "standby", sdir)
            dark, dark_detail = read_standby_darkness(sdir)
            if rc != 0 or dark is not True:
                why = (f"exited {rc}" if rc != 0
                       else f"exit 0 but {dark_detail}")
                self.warn(f"standby after the checkout {why} -- sensor "
                          f"state UNKNOWN, counted as ON until the first "
                          f"point")
            else:
                awake = False
                self.mark_asleep()
        elif s.sleep_gap is None:
            self.ctx.say("check", "--no-sleep: the sensor stays on until the "
                                  "last point")
        else:
            self.ctx.say("check", f"first point in {gap:.0f} min -- staying "
                                  f"awake (a sleep under {MIN_OFF_MIN:g} min "
                                  f"is pointless)")
        first_at = t0.timestamp() + s.at_min[0] * 60.0
        last_at = t0.timestamp() + s.at_min[-1] * 60.0 + s.seconds
        say = self.ctx.say
        say("clear", "-" * 62)
        say("clear", f"degauss accepted, t0 {self.session['t0']}. Sensor "
                     + ("in standby" if not awake else "awake")
                     + " and healthy.")
        say("clear", f"NOTHING NEEDS A PERSON UNTIL THE END. First point "
                     f"{datetime.fromtimestamp(first_at):%H:%M}, last one "
                     f"finishes about "
                     f"{datetime.fromtimestamp(last_at):%H:%M}.")
        say("clear", "Leave the 19V switch ON. At the end the program puts "
                     "the sensor into standby, then asks for the switch -- "
                     "unanswered is fine, the laser is already off.")
        say("clear", "-" * 62)
        return awake

    def go(self):
        s = self.s
        parent = s.outdir or self.lab.bucket("relax")
        outdir = os.path.join(parent, f"rx_{clock.strftime('%Y%m%d_%H%M%S')}")
        try:
            os.makedirs(outdir, exist_ok=False)
        except OSError as e:
            raise ConfigError(f"cannot create outdir {outdir}: {e}")
        self.outdir = outdir
        self.ctx.say("out", self.rel(outdir))

        session = self.session = self.new_session()
        session["passes"] = [{"freq": f, "amp": a}
                             for f, a in build_passes(s)]
        state = self.state = {"shut_down": False}
        outcome, code = "completed", 0
        try:
            # S1: 19V OFF is an operator fact, not a serial command
            session["sensor_power"] = {
                "during_degauss": "unpowered",
                "asserted_by": "--confirm" if s.confirm else "operator y/n",
            }
            rec = {"step": "sensor_unpowered", "started": clock.iso_now(),
                   "exit_code": None,
                   "outcome": "operator confirmed 19V OFF"}
            session["steps"].append(rec)
            rec["ended"] = clock.iso_now()
            write_session(outdir, session, self.ctx)

            # S2: degauss pass(es); t0 anchors on the LAST pass, the only
            # QC-gated one (a 5 Hz strong pass is a legitimate intermediate)
            last_pass_dir = None
            for i, (freq, amp) in enumerate(build_passes(s), start=1):
                pass_dir = os.path.join(outdir, f"dg_pass{i}")
                rc = self.step(
                    f"degauss_p{i}",
                    lambda f=freq, a=amp, d=pass_dir:
                        self.run_degauss_pass(f, a, d),
                    plan=plan_dict(degauss_settings(s, freq, amp, pass_dir)))
                if rc == 30 and s.accept_flagged_degauss:
                    self.warn(f"degauss pass {i} exited 30 (QC refused or "
                              f"unverified) -- accepted by "
                              f"--accept-flagged-degauss")
                elif rc != 0:
                    raise AbortRun(f"degauss pass {i} exited {rc} -- the "
                                   f"shield state is undefined, series "
                                   f"cancelled (redo the full cycle)")
                last_pass_dir = pass_dir
            t0, t0_src, qc_problems = read_degauss_t0(
                last_pass_dir, accept_flagged=s.accept_flagged_degauss)
            for p in qc_problems:
                self.warn(f"degauss accepted despite: {p} "
                          f"(--accept-flagged-degauss)")
            session["t0"] = t0.isoformat(timespec="seconds")
            session["t0_source"] = self.rel(t0_src)
            self.ctx.say("t0", f"{session['t0']} (from "
                               f"{os.path.basename(t0_src)})")
            write_session(outdir, session, self.ctx)

            # S3: open the winding loop and restore 19V
            self.after_degauss_pause()
            state["powered_on"] = True

            # S4+S5: wake, record, sleep
            duty = self.duty()
            awake = False
            checked_out = s.checkout_seconds > 0
            if checked_out:
                awake = self.checkout(t0, duty)
            for i, minutes in enumerate(s.at_min, start=1):
                d = duty[i - 1]
                slept_before = False
                if not awake:
                    slept_before = i > 1 or checked_out
                    if d["wake_at_min"] is not None:
                        # 0.0 is a real time (wake at t0), not "no wake"
                        self.wait_until(t0, d["wake_at_min"],
                                        f"wake for point {i}")
                    state["wake_attempted"] = True
                    sname = "setup" if i == 1 else f"wake_p{i}"
                    rc = self.qzfm_step(sname, "setup",
                                        os.path.join(outdir, sname))
                    if rc == 0:
                        awake = True
                        self.mark_awake()
                    elif i == 1 or rc in (13, 20, 40, 41):
                        # point 1 has no fallback; 13/20/40/41 mean every
                        # later point fails the same way
                        raise AbortRun(f"sensor setup exited {rc} -- no "
                                       f"measurements possible")
                    else:
                        # transient wake failure: the point repeats the
                        # setup itself; unknown sensor state counts as ON
                        self.mark_awake()
                        awake = True
                        self.warn(f"wake before point {i} exited {rc} -- "
                                  f"recording anyway, the point repeats the "
                                  f"setup")

                self.wait_until(t0, minutes)
                name = point_dir_name(i, minutes)
                pdir = os.path.join(outdir, name)
                actual_min = (clock.time() - t0.timestamp()) / 60.0
                note = (f"relax point {i}: t0+{minutes:g} min planned, "
                        f"launched t0+{actual_min:.2f} min (the ledger's "
                        f"t_actual_min is when the DAQ opened, about two "
                        f"minutes later); degauss "
                        f"{session['passes'][-1]['amp']:g} A"
                        + ("; woken from 19V off"
                           if i == 1 and not checked_out
                           else "; woken from standby" if slept_before
                           else ""))
                entry = {"index": i, "dir_name": name,
                         "dir": self.rel(pdir), "t_sched_min": minutes,
                         "t_actual_min": round(actual_min, 3),
                         "t_launch_min": round(actual_min, 3),
                         "t_setup_overhead_min": None,
                         "woke_from_unpowered": i == 1 and not checked_out,
                         "woke_from_standby": slept_before,
                         "exit_code": None, "outcome": None}
                session["points"].append(entry)
                rc = self.qzfm_step(f"point_{i}", "run", pdir,
                                    seconds=s.seconds, notes=note)
                entry["exit_code"] = rc
                # launch time != data time: the point spends ~2 min on
                # setup before the DAQ opens (08-19, H2)
                daq = daq_start_min(pdir, t0)
                if daq is not None:
                    entry["t_actual_min"] = round(daq, 3)
                    entry["t_setup_overhead_min"] = round(
                        daq - entry["t_launch_min"], 3)
                if rc == 0:
                    entry["outcome"] = "ok"
                elif rc in (30, 31):
                    entry["outcome"] = "flagged"
                    self.warn(f"point {i} flagged (exit {rc}) -- data kept, "
                              f"continuing")
                else:
                    entry["outcome"] = "aborted"
                    raise AbortRun(f"point {i} exited {rc} -- outside the "
                                   f"continuable set (0 ok, 30/31 flagged "
                                   f"data), stopping the series")
                write_session(outdir, session, self.ctx)

                if d["sleep_after"]:
                    sdir = os.path.join(outdir, f"standby_p{i}")
                    rc = self.qzfm_step(f"standby_p{i}", "standby", sdir)
                    awake = False
                    dark, dark_detail = read_standby_darkness(sdir)
                    if rc != 0 or dark is not True:
                        why = (f"exited {rc}" if rc != 0
                               else f"exit 0 but {dark_detail}")
                        self.warn(f"standby after point {i} {why} -- sensor "
                                  f"state UNKNOWN, still counted as ON "
                                  f"through the {d['gap_after_min']:.0f} min "
                                  f"gap")
                    else:
                        self.mark_asleep()
                        self.ctx.say("sleep", f"sensor in standby for "
                                     f"{d['gap_after_min'] - s.wake_lead:.0f}"
                                     f" min, waking for point {i + 1}")
            session["end_reason"] = f"{len(session['points'])} points done"
        except KeyboardInterrupt:
            outcome, code = "interrupted", 130
            session["end_reason"] = "Ctrl-C"
            self.ctx.error("interrupted.")
        except Stopped as e:
            outcome, code = "interrupted", 130
            session["end_reason"] = str(e)
            self.ctx.error(f"stopped: {e}")
        except BFieldError as e:
            outcome, code = f"aborted: {e}", e.code
            session["end_reason"] = str(e)
            self.ctx.error(f"ERROR[{e.code}] {type(e).__name__}: {e}")
        except Exception as e:
            outcome, code = f"crashed: {e}", 1
            session["end_reason"] = f"{type(e).__name__}: {e}"
            session["traceback"] = traceback.format_exc()
            raise
        finally:
            outcome, code = self._shutdown(outcome, code)
        if outcome != "completed":
            self.ctx.warn(f"program {outcome}")
        else:
            n_flag = sum(1 for p in session["points"] if p["outcome"] != "ok")
            self.ctx.say("done", f"{len(session['points'])} points, {n_flag} "
                         f"not-ok; sensor on-time "
                         f"{fmt_minutes(session['duty_cycle']['on_time_min_actual'])}"
                         f" and now in standby; results in "
                         f"{self.rel(outdir)}")
        return code

    def _shutdown(self, outcome, code):
        """S6: standby, then ask for the 19V switch. Any attempted wake
        gets a shutdown: a failed setup can still have powered the sensor.
        Runs under a fresh stop flag, so an Abort still shuts down."""
        s, session, state = self.s, self.session, self.state
        cleanup = Context(self.ctx.ui, self.ctx.config)
        if state.get("wake_attempted") and not state["shut_down"]:
            try:
                sdir = os.path.join(self.outdir, "standby_final")
                rc = self.qzfm_step("standby_final", "standby", sdir,
                                    ctx=cleanup)
                dark, dark_detail = read_standby_darkness(sdir)
                state["shut_down"] = (rc == 0 and dark is True)
                session["final_standby"] = {
                    "exit_code": rc, "dark_confirmed": dark,
                    "detail": dark_detail, "overridden": False}
                if not state["shut_down"] and rc == 0 \
                        and s.accept_unverified_standby:
                    state["shut_down"] = True
                    session["final_standby"]["overridden"] = True
                    self.warn(f"final standby exit 0 but {dark_detail} -- "
                              f"accepted on --accept-unverified-standby; "
                              f"still CHECK LED1 before the 19V switch")
                elif not state["shut_down"]:
                    why = (f"exited {rc}" if rc != 0
                           else f"exit 0 but {dark_detail}")
                    self.warn(f"final standby {why} -- the sensor may still "
                              f"be running, CHECK IT (LED1 must be off)")
                    if code == 0:
                        outcome, code = ("aborted: final standby "
                                         "unconfirmed", 20)
            except BaseException:
                self.warn("final standby did not complete -- the sensor may "
                          "still be running, CHECK IT")
                if code == 0:
                    outcome, code = "aborted: final standby unconfirmed", 20
        elif not state.get("wake_attempted"):
            session["steps"].append({"step": "standby_final",
                                     "exit_code": None,
                                     "outcome": "skipped (wake never "
                                                "attempted)"})
        if state.get("powered_on"):
            try:
                session.setdefault("sensor_power", {})["after_series"] = (
                    self.power_off_pause(shut_down=state["shut_down"]))
            except KeyboardInterrupt:
                self.warn("19V-off pause interrupted -- CHECK the power "
                          "switch")
                if code == 0:
                    outcome, code = "aborted: 19V still on", 20
        self.mark_asleep()
        session["duty_cycle"]["final_state"] = (
            "asleep" if state.get("shut_down")
            else "never-woken" if not state.get("wake_attempted")
            else "UNKNOWN (standby unconfirmed -- check the sensor)")
        session["outcome"] = outcome
        write_session(self.outdir, session, self.ctx)
        write_summary(self.outdir, session)
        return outcome, code


def run(ctx, lab, settings, mode):
    """The old main(): validate, open the log, plan or go. Returns the
    exit code; BFieldErrors become their codes (13 config, 20 aborted)."""
    ctx.reset_log()       # so validate's warnings land in the events
    try:
        validate(ctx, lab, settings, mode)
        if mode == "go":
            if settings.outdir is None and lab.dated:
                # the dated run folder becomes the parent: the log and
                # rx_<stamp>/ go inside it
                try:
                    settings.outdir = lab.new_run_dir(
                        settings.name or "Relaxation")
                except (OSError, ValueError) as e:
                    raise ConfigError(f"cannot make the run folder: {e}")
            parent = settings.outdir or lab.bucket("relax")
            ctx.open_log(os.path.join(parent, "relax_session.log"))
        ctx.say("start", "bfield relaxation program, mode " + mode
                + (" [SIMULATED LAB]" if lab.sim else ""))
        rx = Relaxation(ctx, lab, settings)
        if mode == "plan":
            return rx.plan_text()
        rx.confirm_checklist()
        return rx.go()
    except KeyboardInterrupt:
        ctx.error("interrupted before driving.")
        return 130
    except Stopped:
        ctx.error("stopped before driving.")
        return 130
    except BFieldError as e:
        ctx.error(f"ERROR[{e.code}] {type(e).__name__}: {e}")
        return e.code
    except Exception as e:
        ctx.error(f"ERROR[1] {type(e).__name__}: {e}")
        ctx.write("trace", traceback.format_exc().rstrip())
        return 1
    finally:
        ctx.close_log()
