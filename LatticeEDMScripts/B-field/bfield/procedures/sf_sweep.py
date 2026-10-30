"""One S(f) shielding-factor sweep, conducted point by point at the bench.

Moved from IROP_research/scripts/sf_sweep.py with its plan, guards,
prompts, files and exit codes unchanged. The generator is set by hand,
so this cannot run unattended. Guards:

  order:      the drive magnetizes the walls, so frequencies are handed
              out shuffled (seed recorded) and one control point repeats
              first and last to measure session drift.
  monitor:    the resistor is sized once, from the LARGEST current; the
              sweep stops on the first point with a railed monitor.
  transcript: the analysis command is printed at the end from what was
              actually set, in run order.

Less current than asked (amplifier roll-off at low f) is not an error:
S uses the measured current, only the bound gets shallower.

Modes: plan (print the sweep, touch nothing), precheck (ONE short run
to prove the wiring), go (the sweep), dc (S at DC from hand-reversed
dwells, below the generator's 0.1 Hz floor).

    s = SweepSettings.from_config(lab.config, mode="go")
    code = SfSweep(ctx, lab, s).run()

What changed from the old program, deliberately:
  * the bring-up runs in-process (QZFMSession.setup) instead of as a
    qzfm_serial child; its files (setup/qzfm_session.json, _guard/) are
    the same. The whole-bring-up watchdog (--setup-timeout) is gone: each
    sensor step keeps its own deadline.
  * every point is still a BARE recorder child (no housekeeping), as
    before, with --stop-file added so a GUI Abort stops it cleanly.
  * the operator's typed replies go through ctx.ui.ask_text when the UI
    has it (a GUI), else the terminal. KeyboardInterrupt / Abort -> 20.

Exit codes: 0 done, 13 bad plan, 20 sweep stopped, 21 scope unreadable,
40 a run left no readable meta. Physics / recipe:
analysis_scripts/shield_sf/shield_sf_ac.py.
"""

from __future__ import annotations

import contextlib
import json
import math
import os
import random
import subprocess
import threading
from dataclasses import dataclass, field

from bfield import clock
from bfield.acquisition.child import (new_process_group_kwargs,
                                      recorder_argv, stop_child_ladder)
from bfield.channels import (key_number, label, parse_map,
                             shared_axis_pairs, split_channel)
from bfield.config import PROJECT_DIR
from bfield.daq_models import active as daq_model
from bfield.devices.scope import DEFAULT_SCOPE_CH, ScopeError
from bfield.errors import (AbortRun, BFieldError, ConfigError, Stopped)
from bfield.io.files import write_json_atomic
from bfield.operations.record import write_lab_file
from bfield.sensors import Selection, resolve_device_name
from bfield.ui import ConsoleUI

# 0.1 Hz is the generator's floor. 50 Hz and the 59.917 Hz comb sit on
# lines; 15/20 Hz sit on a bump that wanders 19-21 Hz in degaussed records.
DEFAULT_FREQS = "0.1,0.2,0.5,1,2,5,10,25,40,70"
# The repeated point measures session drift, so it has to detect; the
# lowest point only bounds, and two bounds measure nothing.
DEFAULT_CONTROL_HZ = 5.0
# 467A at gain x2, no ballast, is the clean ceiling; x5 clips (checked
# on the scope 2026-08-20). Stored so a clipped sweep is identifiable.
AMP_GAIN = 2
# Sized from the measurement, not the amplifier (467A ceiling 0.5 A peak):
# 0.1 Hz needs >= 300 mA to bound above 1e6 at 900 s; 5-40 Hz wants 362-476 mA.
DEFAULT_AMP_MA = 450.0
# MEASURED 2026-08-19 (Mydailymemo), not a catalogue value; re-measure
# when the wiring changes. S ~ 1/R_mon, so null DMM leads (REL) or go 4-wire.
DEFAULT_RMON_OHM = 0.3
# Provenance only: 2 turns, 0.9 x 1.2 m, measured 2026-07-10.
COIL_OHM = 0.19
# Free-space B/I at the coil centre, messages only: 20/9 nT/mA.
# Analysis recomputes it -- change both.
K_GEOM_NT_PER_MA = 20.0 / 9.0
# A DC offset does not average away: steady field on the walls for the
# whole record. Below this fraction the scope's own DC error dominates.
OFFSET_WARN_FRAC = 0.10
# 900 s is the campaign's canonical window, so a sweep point doubles as
# an ordinary noise record and can be read beside the archive.
DEFAULT_MIN_CYCLES = 90.0     # 900 s at the 0.1 Hz bottom
DEFAULT_MIN_SECONDS = 900.0
SECONDS_QUANTUM = 60.0        # round a long point up to whole minutes
# plausibility band for the drive; the 467A is rated 500 mA
I_SANE_MA = (10.0, 2000.0)
# Honest dial error is a few percent; misreads seen were 2.05x-14286x
# off (2026-08-20). 1.5 separates them; off-plan drives are typed, not read.
F_SCOPE_AGREE = 1.5
# The amplitude is NOT a knob (467A x2 is the clean ceiling, x5 clips):
# never tell the operator to raise it. Reach is bought with TIME.
AMP_BAND_MA = (350.0, 500.0)
AMP_SPEC_FLOOR_MA = 300.0     # 0.1 Hz bound = 1e6 exactly, at 900 s
# Drive-off record at each end of the sweep: the PAIR measures what the
# sweep did to the walls. 0 skips them.
DEFAULT_NULL_SECONDS = 900.0

# A cylinder has two shielding factors (axial / transverse), so the
# sweep runs twice with the coil turned. Z is the bore axis. The map is
# both sensors' Z and Y on their own inputs (config/sensors.toml), the
# same in both sweeps: turning the coil moves NO cable; the orientation
# picks the driven axis, i.e. the pair analysed (s1z,s2z axial).
ORIENTATION_AXIS = {"axial": "z", "transverse": "y"}
ORIENTATION_MAP = ORIENTATION_AXIS        # the orientations (old name)
ORIENTATION_COIL = {
    "axial":      "the coil's axis ALONG the bore (its plane across it)",
    "transverse": "the coil's axis ACROSS the bore (its plane containing it)",
}
DEFAULT_ORIENTATION = "axial"
DEFAULT_IDS = "s1:A10QHMQMA,s2:A10QHJ1ZA"
DEFAULT_CAL_V_PER_NT = 2.7    # nominal at 1x; the sweep reads both heads
# measured-vs-asked mismatch before the operator is told
CURRENT_TELL_FRAC = 0.15
CHILD_PAD_S = 180.0           # watchdog slack over a run's own length

# DC mode: ABBA (+ - - +): a linear drift cancels exactly in
# [(m1+m4)-(m2+m3)]/4. Sensor drift (~0.44 pT/min) is the signal's size.
DC_PATTERN = ("+", "-", "-", "+")
DEFAULT_DC_MA = 1000.0        # the DC supply's ceiling, not the 467A's
DEFAULT_DC_DWELL_S = 90.0
DEFAULT_DC_BLOCKS = 8

ANALYSIS_DIR = os.path.join(PROJECT_DIR, "analysis_scripts", "shield_sf")
MODES = ("plan", "precheck", "go", "dc")



def _q(path):
    """A path as it must appear in a copy-paste command: quoted when it
    has a space (the Box data folder does)."""
    path = str(path)
    return f'"{path}"' if (" " in path and not path.startswith('"')) else path

class NoMeta(BFieldError):
    code = 40


AbortSweep = AbortRun         # code 20


def require(cond, msg):
    if not cond:
        raise ConfigError(msg)


@dataclass
class SweepSettings:
    """The old sf_sweep flags (same names)."""
    mode: str = "plan"
    dc_ma: float = DEFAULT_DC_MA
    dc_dwell: float = DEFAULT_DC_DWELL_S
    dc_blocks: int = DEFAULT_DC_BLOCKS
    freqs: str = DEFAULT_FREQS
    control_hz: float = DEFAULT_CONTROL_HZ
    seed: int | None = None
    amp_ma: float = DEFAULT_AMP_MA
    rmon_ohm: float = DEFAULT_RMON_OHM
    min_cycles: float = DEFAULT_MIN_CYCLES
    min_seconds: float = DEFAULT_MIN_SECONDS
    precheck_seconds: float = 60.0
    null_seconds: float = DEFAULT_NULL_SECONDS
    orientation: str = DEFAULT_ORIENTATION
    map: str | None = None
    aux: str = ""
    scope: bool = True
    scope_resource: str | None = None
    scope_channel: str = DEFAULT_SCOPE_CH
    ids: str = DEFAULT_IDS
    fs: float = 1200.0
    vrange: float = 2.0
    gain: str = "1x"
    cal_v_per_nt: float = DEFAULT_CAL_V_PER_NT
    skip_setup: bool = False
    device: str | None = None
    outdir: str | None = None
    name: str | None = None       # dated run folder name; None = e.g.
                                  # SfSweepAxial, SfPrecheckAxial, SfDCAxial
    # filled in while running
    cal_factors: str | None = field(default=None, repr=False)
    n_points: int = field(default=0, repr=False)
    started: str = field(default="", repr=False)

    @classmethod
    def from_config(cls, config, **overrides):
        """Bench defaults from config/lab.toml, then the caller's values."""
        from bfield.sensors import load_sensors
        known = load_sensors(config)
        d = config.section("daq")
        sc = config.section("scope")
        base = {}
        if len(known) >= 2:
            # the two lowest-numbered sensors; --sensors picks others
            base["ids"] = ",".join(f"s{k.number}:{k.usb_serial}"
                                   for k in known[:2])
        if d.get("fs_hz"):
            base["fs"] = float(d["fs_hz"])
        if d.get("vrange_V"):
            base["vrange"] = float(d["vrange_V"])
        if d.get("device"):
            base["device"] = d["device"]
        if sc.get("resource"):
            base["scope_resource"] = sc["resource"]
        if sc.get("channel"):
            base["scope_channel"] = sc["channel"]
        base.update({k: v for k, v in overrides.items() if v is not None})
        return cls(**base)


# ------------------------------------------------------------ the sweep

class Point:
    """One frequency of the sweep, before and after it is recorded."""

    def __init__(self, index, f_hz, seconds, is_control):
        self.index = index
        self.f_hz = f_hz
        self.seconds = seconds
        self.is_control = is_control
        self.dirname = f"p{index:02d}_{hz_tag(f_hz)}"
        self.f_actual = f_hz
        self.i_actual_ma = None
        self.outdir = None
        self.result = None
        self.from_scope = False   # were f and I read, or asserted?
        self.dc_offset_ma = None  # what the generator was sitting on

    @property
    def cycles(self):
        return self.f_hz * self.seconds


def hz_tag(f):
    """0.05 -> 0p05Hz, 70 -> 70Hz (the index prefix keeps run order)."""
    return f"{f:g}".replace(".", "p") + "Hz"


def point_seconds(f_hz, min_cycles, min_seconds):
    """Seconds for min_cycles, floored at min_seconds, whole minutes."""
    need = min_cycles / f_hz
    if need <= min_seconds:
        return float(min_seconds)
    return float(math.ceil(need / SECONDS_QUANTUM) * SECONDS_QUANTUM)


def build_points(s, warn=None):
    freqs = []
    for tok in s.freqs.split(","):
        tok = tok.strip()
        if not tok:
            continue
        try:
            f = float(tok)
        except ValueError:
            raise ConfigError(f"--freqs {s.freqs!r}: {tok!r} is not a "
                              f"number")
        require(f > 0, f"--freqs: {f:g} Hz is not a frequency")
        require(f < s.fs / 2.0,
                f"--freqs: {f:g} Hz is above the Nyquist of --fs {s.fs:g}")
        freqs.append(f)
    require(freqs, "--freqs is empty")
    require(len(set(freqs)) == len(freqs),
            "--freqs repeats a frequency; the control point is added by "
            "--control-hz, not by listing it twice")
    require(s.control_hz in freqs,
            f"--control-hz {s.control_hz:g} is not in --freqs: the "
            f"repeated point has to be one of the sweep's own points")
    if s.control_hz == min(freqs) and warn is not None:
        warn(f"--control-hz {s.control_hz:g} is the bottom of the sweep, "
             f"the point most likely to come back as a bound -- two bounds "
             f"measure no drift. Pick a frequency that certainly detects.")
    middle = [f for f in freqs if f != s.control_hz]
    seed = s.seed if s.seed is not None else random.randrange(1 << 30)
    random.Random(seed).shuffle(middle)
    order = [s.control_hz] + middle + [s.control_hz]
    # ONE length for every point: per-point lengths made the points
    # different measurements (sigma, segment count, T all moved with f)
    secs = max(point_seconds(f, s.min_cycles, s.min_seconds) for f in order)
    points = [Point(i, f, secs, f == s.control_hz)
              for i, f in enumerate(order, start=1)]
    # the two control runs would otherwise collide on one directory name
    points[-1].dirname += "_again"
    return points, seed


def monitor_name(s):
    first = next((a.strip() for a in s.aux.split(",") if a.strip()), "")
    return first.partition(":")[0].lower()


def sweep_axes(s):
    """Lab axes the map covers, in the sensor session's spelling."""
    seen = []
    for name, _k, _p in parse_map(s.map):
        ax = split_channel(name)[1]
        if ax not in seen:
            seen.append(ax)
    return ",".join(a for a in ("z", "y") if a in seen)


def sweep_map(config, ids, orientation, with_monitor=False):
    """The two sensors' channel map: both axes on their own inputs; with a
    DAQ current monitor the second keeps only the driven axis (its other
    input carries the monitor, so its BNC moves between the sweeps)."""
    sel = Selection(config, resolve_device_name(config, ids), axes="z,y")
    if len(sel.sensors) != 2:
        raise ConfigError("the S(f) sweep reads exactly two sensors")
    driven = ORIENTATION_AXIS[orientation]
    entries = sel.channels()
    second = entries[-1][1]
    return ",".join(f"{n}:{p}" for n, k, ax, p in entries
                    if not (with_monitor and k == second and ax != driven))


def head_pair(s):
    """Interior pair to analyse: the driven axis on both sensors (s1z,s2z
    axial, s1y,s2y transverse; checked against the map so a transverse
    sweep is not analysed on the undriven axis)."""
    names = [n for n, _k, _p in parse_map(s.map)]
    pairs = shared_axis_pairs(names)
    driven = ORIENTATION_AXIS.get(s.orientation)
    on_axis = [p for p in pairs if split_channel(p[0])[1] == driven]
    if on_axis:
        return ",".join(on_axis[0])
    if len(pairs) == 1:
        return ",".join(pairs[0])
    return None


def notes_for(point, s):
    """Notes string in the form shield_sf_ac.py reads."""
    if monitor_name(s):
        return (f"sf {point.f_actual:g} Hz {point.i_actual_ma:g} mA "
                f"rmon {s.rmon_ohm:g} ohm")
    return (f"sf {point.f_actual:g} Hz {point.i_actual_ma:g} mA "
            f"from the scope over {s.rmon_ohm:g} ohm "
            f"(declared, not recorded)")


def judge(meta, point, s):
    """Verdict from the run's meta: the recorder already measured
    everything that decides a point."""
    mon = monitor_name(s)
    rails = meta.get("rail_fraction") or {}
    ptp = meta.get("flat_ptp_V") or {}
    out = {"exit_code": meta.get("exit_code"),
           "status": meta.get("status"),
           "has_monitor": bool(mon),
           "monitor_railed": bool(mon and rails.get(mon)),
           "monitor_rail_fraction": rails.get(mon) if mon else None,
           "monitor_ptp_V": ptp.get(mon) if mon else None,
           "field_rails": {k: v for k, v in rails.items()
                           if k != mon and v},
           "i_measured_ma": None}
    if not mon:
        return out
    v = ptp.get(mon)
    if v and s.rmon_ohm > 0:
        # 0-p from p-p; noise rides along: a bench check only
        out["i_measured_ma"] = (v / 2.0) / s.rmon_ohm * 1e3
    return out


def check_amp(i_ma):
    """Note when the drive lands outside the working band; never an error."""
    if i_ma <= 0:
        return (f"{i_ma:g} mA is no drive at all -- a flat trace on "
                f"the screen, not a small current")
    lo, hi = AMP_BAND_MA
    if lo <= i_ma <= hi:
        return None
    if i_ma > hi:
        return (f"{i_ma:.0f} mA is over the {hi:g} mA this chain gives "
                f"cleanly -- look at the waveform before you believe it: "
                f"a reading this high is more likely a clipped sine, "
                f"which breaks the cycle RMS")
    need_s = DEFAULT_MIN_SECONDS * (AMP_SPEC_FLOOR_MA / i_ma) ** 2
    return (f"{i_ma:.0f} mA. The point is correct (S divides by what "
            f"the scope read); nothing to change at the generator, but "
            f"it costs reach at the bottom: 0.1 Hz clears the 1e6 spec "
            f"at {AMP_SPEC_FLOOR_MA:g} mA in "
            f"{DEFAULT_MIN_SECONDS:g} s, and at {i_ma:.0f} mA the same "
            f"bound wants about {need_s:.0f} s "
            f"(--min-seconds {math.ceil(need_s / 60.0) * 60:.0f}). "
            f"From 2 Hz up nothing changes in kind")


def scope_gate(f, i, f_planned, fs):
    """Why this reading cannot be the drive, or None if it can be.

    Guard added after a sweep recorded 900 s under a misread
    "1428.57 Hz" (2026-08-20): misreads are the screen, not the drive.
    """
    if not (I_SANE_MA[0] <= i <= I_SANE_MA[1]):
        return (f"{i:g} mA is outside {I_SANE_MA[0]:g}-{I_SANE_MA[1]:g} "
                f"mA, which this chain cannot be producing")
    if f >= fs / 2.0:
        return (f"{f:g} Hz is above the Nyquist of the {fs:g} S/s "
                f"record -- no analysis could ever find that line")
    if not (f_planned / F_SCOPE_AGREE <= f <= f_planned * F_SCOPE_AGREE):
        return (f"{f:g} Hz against the planned {f_planned:g} Hz "
                f"({f / f_planned:.3g}x) -- the dial lands within a few "
                f"percent, never there")
    return None


def eng(x, unit):
    """1e-3 -> '1 m' (engineering prefix for a scope dial)."""
    for scale, pre in ((1.0, ""), (1e-3, "m"), (1e-6, "u")):
        if abs(x) >= scale or scale == 1e-6:
            return f"{x / scale:g} {pre}{unit}"


def snap_125(x):
    """Round up to the nearest 1-2-5 scope-dial step."""
    decade = 10.0 ** math.floor(math.log10(x))
    for m in (1.0, 2.0, 5.0):
        if m * decade >= x:
            return m * decade
    return 10.0 * decade


def bench_hint(f_hz, s):
    """Dial settings for this point (3311A = range button x dial 1-10).
    The scope must show >= 1 whole cycle or CRMs/FREQuency read 9.9E37."""
    rng = 10.0 ** math.floor(math.log10(f_hz))
    dial = f_hz / rng
    tb = snap_125(2.0 / (10.0 * f_hz))
    vpp = 2.0 * s.amp_ma * 1e-3 * s.rmon_ohm
    vdiv = snap_125(vpp / 5.0)
    lines = [f"    467A  : gain x{AMP_GAIN:g}, no ballast    (x5 clips; "
             f"so does turning the 3311A up from here)",
             f"    3311A : range {rng:g} x dial {dial:.1f}"
             f"        -> {s.amp_ma:g} mA = {vpp:.3f} Vp-p on "
             f"{s.rmon_ohm:g} ohm",
             f"    scope : {eng(tb, 's')}/div, {eng(vdiv, 'V')}/div"
             f"     ({10 * tb * f_hz:.1f} cycles on screen; under one and "
             f"CRMs fails)"]
    if abs(dial - 1.0) < 1e-9:
        lines.append("            (dial 1 reads poorly -- the range below "
                     "with dial 10 is the same frequency)")
    return lines


def dc_dwell_name(block, index, sign):
    return f"b{block:02d}d{index}_{'p' if sign == '+' else 'm'}"


# ------------------------------------------------------------ the program

class SfSweep:
    """One run of the sweep program in one mode."""

    def __init__(self, ctx, lab, s):
        self.ctx = ctx
        self.lab = lab
        self.s = s
        self.parent = s.outdir and os.path.dirname(s.outdir) or \
            lab.bucket("sf")

    # -- plumbing -------------------------------------------------------

    def say(self, tag, msg):
        self.ctx.say(tag, msg)

    def warn(self, msg):
        self.ctx.warn(msg)

    def rel(self, path):
        """Relative to data_root's parent ("result\\shield_sf\\...") while
        data_root is the old result tree, as the old ledgers."""
        try:
            return os.path.relpath(path, os.path.dirname(self.lab.data_root))
        except ValueError:
            return path

    def text(self, prompt):
        """A typed reply from the operator ("" = Enter)."""
        try:
            return self.ctx.ask_text(prompt)
        except EOFError:
            raise AbortSweep("stdin closed: a sweep needs an operator at "
                             "the generator")

    def _needs_operator(self):
        ui = self.ctx.ui
        if isinstance(ui, ConsoleUI) and not ui._interactive():
            raise ConfigError(
                "--go, --precheck and --dc need a terminal: the drive is "
                "set by hand between records")

    # -- validation -----------------------------------------------------

    def validate(self):
        s = self.s
        require(s.mode in MODES,
                "pass exactly one of --plan, --precheck, --go, --dc")
        require(s.orientation in ORIENTATION_AXIS,
                f"--orientation must be one of {sorted(ORIENTATION_AXIS)}")
        # the sensors by number: s1:SERIAL,s2:SERIAL whatever was typed
        s.ids = Selection(self.lab.config,
                          resolve_device_name(self.lab.config, s.ids),
                          axes="z,y").device_name
        mon_map = sweep_map(self.lab.config, s.ids, s.orientation,
                            with_monitor=True)
        if not s.map:
            s.map = (mon_map if monitor_name(s) else
                     sweep_map(self.lab.config, s.ids, s.orientation))
        parse_map(s.map)            # refuses a malformed hand-written map
        if s.mode == "dc":
            require(s.dc_ma > 0, "--dc-ma has to be positive")
            require(s.dc_dwell > 0 and math.isfinite(s.dc_dwell),
                    "--dc-dwell is a positive finite number of seconds")
            require(s.dc_blocks >= 1, "--dc-blocks is at least 1")
            if s.dc_blocks < 5:
                self.warn(f"{s.dc_blocks} block(s): the error bar is the "
                          f"spread ACROSS blocks -- that few gives no usable "
                          f"interval")
        require(s.amp_ma > 0, "--amp-ma has to be positive")
        require(s.rmon_ohm > 0, "--rmon-ohm has to be positive")
        require(s.null_seconds >= 0 and math.isfinite(s.null_seconds),
                "--null-seconds is zero (skip) or a positive finite "
                "number of seconds")
        require(math.isfinite(s.precheck_seconds) and s.precheck_seconds > 0,
                "--precheck-seconds must be positive and finite (zero or "
                "less means 'run until stopped')")
        require(s.min_cycles > 0 and s.min_seconds > 0,
                "--min-cycles and --min-seconds have to be positive")
        require(s.fs > 0 and s.vrange > 0, "--fs and --vrange are positive")
        if not monitor_name(s):
            # operator decision 2026-08-19: no DAQ monitor -- the scope
            # watches the drive, ai3 carries head b's second axis
            wiring = ", ".join(f"{label(n)}={p}"
                               for n, _k, p in parse_map(s.map))
            if s.scope:
                self.say("wiring", "no current monitor on the DAQ: every "
                                   "input is on a sensor, and the scope "
                                   "measures the drive over USB")
                self.say("wiring", f"turning the coil moves NO cable: "
                                   f"{wiring} in both sweeps, and the "
                                   "orientation only picks the pair to "
                                   f"analyse ({head_pair(s) or s.map})")
            else:
                self.warn("no current monitor on the DAQ (--aux is empty) "
                          "and --no-scope: the current is whatever you type, "
                          "and nothing later can check it (S goes as 1/I). "
                          "Drop --no-scope, or pass --aux mon:aiN at the "
                          "cost of the second sensor's other axis.")
        elif s.map == mon_map:
            second = parse_map(s.map)[-1]
            self.say("wiring", f"MOVE sensor {key_number(second[1])}'s BNC "
                               f"before this sweep: its one remaining "
                               f"channel must carry the {s.orientation} axis "
                               f"({label(second[0])} on {second[2]}). The "
                               "other sensor does not move.")
        n_ch = len([p for p in s.map.split(",") if p.strip()]) \
            + len([a for a in s.aux.split(",") if a.strip()])
        total = n_ch * s.fs
        daq = daq_model()
        require(total <= daq.aggregate_max,
                f"{n_ch} channels at {s.fs:g} Hz is {total:g} S/s, over the "
                f"{daq.title} aggregate limit of {daq.aggregate_max:g} -- drop a "
                f"channel or lower --fs (then re-check what folds into the "
                f"band)")
        v_mon = s.amp_ma * 1e-3 * s.rmon_ohm
        require(v_mon < 0.99 * s.vrange,
                f"{s.amp_ma:g} mA through {s.rmon_ohm:g} ohm is "
                f"{v_mon:.2f} V, outside the +/-{s.vrange:g} V window -- "
                f"a railed monitor makes the points unusable: use R < "
                f"{0.99 * s.vrange / (s.amp_ma * 1e-3):.2f} ohm")
        if s.mode != "plan" and not s.skip_setup:
            # the device-name/map/axes rules live in the sensor session;
            # ask it rather than re-deriving them here
            from bfield.procedures.qzfm_session import validate
            try:
                validate(self.setup_plan(os.path.join(self.parent,
                                                      "_preflight")),
                         "setup")
            except ConfigError as e:
                raise ConfigError(f"the sensor session refuses the bring-up:"
                                  f" {e}")
        if s.mode != "plan":
            self._needs_operator()

    # -- the bring-up ---------------------------------------------------

    def setup_plan(self, outdir):
        from bfield.procedures.qzfm_session import QZFMPlan
        s = self.s
        return QZFMPlan.from_config(
            self.lab.config, device_name=s.ids, map=s.map,
            axes=sweep_axes(s), gain=s.gain, fs=s.fs,
            vrange=f"{s.vrange:g}", cal_v_per_nT=s.cal_v_per_nt,
            outdir=outdir, require_factors=True)

    def run_setup(self, setup_dir):
        """Bring-up + Field Zero once; returns the recorder's
        --cal-factors from setup/qzfm_session.json."""
        from bfield.procedures.qzfm_session import QZFMSession
        plan = self.setup_plan(setup_dir)
        self.say("child", f"setup: sensor session --setup "
                          f"--device-name {plan.device_name} --map {plan.map}"
                          f" --axes {plan.axes} -> {self.rel(setup_dir)}")
        code = QZFMSession(self.ctx.child(), self.lab, plan).setup()
        if code != 0:
            raise AbortSweep(f"the bring-up exited {code} -- look in "
                             f"{self.rel(setup_dir)}")
        return self.read_cal_factors(setup_dir)

    def read_cal_factors(self, setup_dir):
        """Calibration factors from the setup json, as the recorder wants
        them: every record is traceable to its calibration."""
        path = os.path.join(setup_dir, "qzfm_session.json")
        try:
            with open(path, encoding="utf-8") as fh:
                heads = json.load(fh)["setup"]["heads"]
            parts = []
            for chan, key, _pin in parse_map(self.s.map):
                axis = split_channel(chan)[1]
                parts.append(f"{chan}:{heads[key]['cal_factors'][axis]:g}")
            return ",".join(parts)
        except (OSError, ValueError, KeyError, TypeError) as e:
            raise AbortSweep(
                f"the bring-up exited 0 but {path} would not give a "
                f"calibration factor for every mapped channel ({e!r})")

    # -- recording ------------------------------------------------------

    def record_args(self, point, outdir, seconds=None, notes=None):
        s = self.s
        cmd = ["--numbered", "--map", s.map, "--ids", s.ids,
               "--fs", f"{s.fs:g}", "--vrange", f"{s.vrange:g}",
               "--gain", s.gain, "--cal-v-per-nT", f"{s.cal_v_per_nt:g}",
               "--seconds", f"{seconds or point.seconds:g}",
               "--outdir", outdir]
        if s.cal_factors:
            cmd += ["--cal-factors", s.cal_factors]
        for spec in (a.strip() for a in s.aux.split(",")):
            if spec:
                cmd += ["--aux", spec]
        if s.device:
            cmd += ["--device", s.device]
        cmd += ["--notes", notes if notes is not None
                else notes_for(point, s)]
        return cmd

    def run_recorder(self, rec_args, timeout, desc):
        """One bare recorder child to completion; returns its exit code.

        Its console lines go to the operations log. A stop file is
        passed so an Abort lets it write its meta.json.
        """
        outdir = rec_args[rec_args.index("--outdir") + 1]
        stop_file = os.path.join(outdir, "STOP")
        lab_file = write_lab_file(self.lab)
        cmd = [*recorder_argv(), *rec_args, "--stop-file", stop_file,
               "--lab-file", lab_file]
        self.say("child", f"{desc}: " + " ".join(
            str(c) if " " not in str(c) else f'"{c}"' for c in rec_args))
        kw = dict(new_process_group_kwargs())
        kw.update(stdout=subprocess.PIPE, stderr=subprocess.STDOUT,
                  text=True, encoding="utf-8", errors="replace")
        proc = subprocess.Popen([str(c) for c in cmd], **kw)

        def pump():
            for line in proc.stdout:
                line = line.rstrip()
                if line:
                    self.ctx.say("rec", line)

        reader = threading.Thread(target=pump, daemon=True)
        reader.start()
        deadline = clock.monotonic() + timeout
        try:
            while True:
                try:
                    code = proc.wait(timeout=clock.real_seconds(0.5))
                    break
                except subprocess.TimeoutExpired:
                    pass
                if self.ctx.stop_requested():
                    stop_child_ladder(proc, stop_file=stop_file,
                                      warn=self.warn)
                    raise Stopped("stopped by the operator during a record")
                if clock.monotonic() > deadline:
                    how = stop_child_ladder(proc, stop_file=stop_file,
                                            warn=self.warn)
                    raise AbortSweep(
                        f"{desc}: watchdog fired after {timeout:.0f} s"
                        + ("" if how in ("dead", "stopped-file")
                           else " and the recorder had to be force-killed; "
                                "it may still hold the DAQ"))
        except KeyboardInterrupt:
            stop_child_ladder(proc, stop_file=stop_file, warn=self.warn)
            raise
        finally:
            reader.join(timeout=5.0)
            with contextlib.suppress(OSError):
                os.remove(lab_file)
        return code

    def find_meta(self, outdir):
        metas = ([f for f in sorted(os.listdir(outdir))
                  if f.endswith(".meta.json")]
                 if os.path.isdir(outdir) else [])
        if len(metas) != 1:
            raise NoMeta(f"{self.rel(outdir)}: expected one meta.json, found "
                         f"{len(metas)} -- nothing readable to judge this "
                         f"point (check the disk)")
        with open(os.path.join(outdir, metas[0]), encoding="utf-8") as fh:
            return json.load(fh)

    def report(self, point, verdict):
        s = self.s
        bits = [f"exit {verdict['exit_code']}"]
        im = verdict["i_measured_ma"]
        if im is not None:
            asked = point.i_actual_ma
            bits.append(f"monitor says {im:.0f} mA")
            if asked and abs(im / asked - 1.0) > CURRENT_TELL_FRAC:
                bits.append(f"against {asked:g} mA asked for")
        if verdict["field_rails"]:
            bits.append("railed: " + ", ".join(
                f"{k} {v:.1%}" for k, v in verdict["field_rails"].items()))
        self.say("point", f"{point.dirname}: " + "; ".join(bits))
        if verdict["has_monitor"] and (im is None or not (im > 0.0)):
            raise AbortSweep(
                f"{point.dirname}: the monitor measured no current "
                f"(peak-to-peak {verdict['monitor_ptp_V']!r} V on "
                f"{monitor_name(s)}); a fallen monitor lead still exits "
                f"0 here. Check the lead, the resistor and the amplifier "
                f"before recording anything else.")
        if verdict["monitor_railed"]:
            raise AbortSweep(
                f"{point.dirname}: the monitor railed "
                f"({verdict['monitor_rail_fraction']:.1%} of samples); the "
                f"point is lost and every larger one will rail too. Size R "
                f"from the sweep's LARGEST current: I_0p x R < 0.99 x "
                f"{s.vrange:g} V, i.e. R < "
                f"{0.99 * s.vrange / (s.amp_ma * 1e-3):.2f} ohm at "
                f"{s.amp_ma:g} mA. Change the resistor, pass it as "
                f"--rmon-ohm, and start a fresh sweep directory.")
        if im is not None and point.i_actual_ma \
                and abs(im / point.i_actual_ma - 1.0) > CURRENT_TELL_FRAC:
            self.warn(f"{point.dirname}: {point.i_actual_ma:g} mA was set but "
                      f"the monitor measured {im:.0f} mA -- at low f that is "
                      f"amplifier roll-off; S stays right, the bound gets "
                      f"shallower.")
        if verdict["exit_code"] not in (0, 30, 31):
            raise AbortSweep(
                f"{point.dirname}: the recorder exited "
                f"{verdict['exit_code']} ({verdict['status']}) -- 13 is the "
                f"command line, 11 the DAQ. Fix it and restart the sweep.")
        if verdict["exit_code"] in (30, 31):
            self.warn(f"{point.dirname}: recorder exit "
                      f"{verdict['exit_code']} -- the data is kept and the "
                      f"sweep continues, but read the rails before this "
                      f"point is used.")

    # -- the operator's side --------------------------------------------

    def open_scope(self, why):
        """Opened before anything is recorded: a missing pyvisa at point
        4 of 11 would cost the evening."""
        s = self.s
        if not s.scope:
            self.warn(f"--no-scope: {why} is whatever you type; nothing "
                      f"later can check it, and S goes as 1/I")
            return None
        try:
            scope = self.lab.scope(self.ctx, s.scope_resource,
                                   s.scope_channel)
        except ImportError as e:
            raise ScopeError(
                f"pyvisa is not installed ({e}) -- pip install -e .[lab], "
                f"or pass --no-scope and type the values") from None
        self.say("scope", f"{scope.idn} on {scope.channel} "
                          f"({scope.resource})")
        return scope

    def say_drive(self, f, i, dc, note, lead="scope"):
        self.say(lead, f"{f:.4g} Hz, {i:.0f} mA zero-to-peak, "
                       f"DC {dc:+.0f} mA" + (f"  [{note}]" if note else ""))
        if i > 0 and abs(dc) > OFFSET_WARN_FRAC * i:
            self.warn(f"DC offset {dc:+.0f} mA "
                      f"({abs(dc) / i:.0%} of the amplitude) = a steady "
                      f"{abs(dc) * K_GEOM_NT_PER_MA:.0f} nT on the walls for "
                      f"this point; S is unaffected (the lock-in reads the "
                      f"line). Turn the generator's DC offset down.")
        note_i = check_amp(i)
        if note_i:
            self.warn(note_i)

    def record_null(self, outdir, name, when):
        """One drive-off noise record. The before/after PAIR measures what
        the sweep did to the walls; one alone only says their state."""
        s = self.s
        self.say("null", f"{when}: turn the generator OUTPUT OFF (or pull the "
                         f"lead at the coil) for a drive-off record")
        reply = self.text(f"  [{name}] drive off? Enter to record "
                          f"{s.null_seconds:g} s, or 'skip': ")
        if reply.lower() == "skip":
            self.warn(f"{name} skipped -- without both nulls the sweep "
                      f"cannot say how much it magnetized the walls")
            return None
        d = os.path.join(outdir, name)
        self.run_recorder(self.record_args(None, d, seconds=s.null_seconds,
                                           notes=f"sf null {when}: drive off,"
                                                 f" {s.null_seconds:g} s"),
                          s.null_seconds + CHILD_PAD_S, name)
        return d

    def ask_setting(self, point, scope=None):
        """Ask for the generator to be set; return what was actually set.

        Enter reads the scope (or accepts the plan under --no-scope);
        anything typed replaces it, so 4.98 Hz is recorded as 4.98 Hz.
        """
        s = self.s
        vpp = 2.0 * s.amp_ma * 1e-3 * s.rmon_ohm
        prompt = (f"\n  point {point.index}/{s.n_points}"
                  f"{'  [control]' if point.is_control else ''}: "
                  f"{point.f_hz:g} Hz for {point.seconds:.0f} s "
                  f"({point.cycles:.0f} cycles)\n"
                  + "\n".join(bench_hint(point.f_hz, s)) + "\n"
                  + ("  Enter to READ THE SCOPE, or type "
                     if scope else "  Enter to accept, or type ")
                  + ("the actual 'f,I' "
                     "(Hz,mA -- or Vp-p with a 'v', e.g. 4.98,0.28v), "
                     "or 'skip', or 'stop': "))
        del vpp
        while True:
            reply = self.text(prompt)
            if reply.lower() in ("stop", "q", "quit"):
                raise AbortSweep("stopped by the operator")
            if reply.lower() == "skip":
                return None
            if reply.lower() == "go":
                self.say("ask", "'go' belongs to the DC-offset prompt; here, "
                                "Enter reads the scope")
                continue
            if not reply:
                if scope is None:
                    point.f_actual, point.i_actual_ma = point.f_hz, s.amp_ma
                    return point
                # read the instrument, not the plan: an assumed value is
                # the one number no later step can check
                try:
                    f, i, dc, note = scope.read_drive(s.rmon_ohm)
                except ScopeError as e:
                    self.warn(f"the scope could not be read: {e} -- set it "
                              f"up and press Enter again, or type the values")
                    continue
                self.say_drive(f, i, dc, note)
                bad = scope_gate(f, i, point.f_hz, s.fs)
                if bad:
                    self.warn(f"not recording under that: {bad} -- set the "
                              f"scope for THIS point (timebase from the plan "
                              f"above, a whole cycle on screen), then Enter "
                              f"to read again, or type 'f,I' to overrule")
                    continue
                if abs(dc) > OFFSET_WARN_FRAC * i:
                    # fix the offset NOW, before the record: adjusting it
                    # mid-record steps the walls into the data (2026-08-20)
                    r = self.text("  turn the DC offset down and press Enter "
                                  "to re-read, or type 'go' to record with "
                                  "the offset as it is: ")
                    if r.lower() != "go":
                        continue
                point.dc_offset_ma = dc
                point.f_actual, point.i_actual_ma = f, i
                point.from_scope = True
                return point
            parts = [p for p in reply.replace(" ", ",").split(",") if p]
            # glue a lone trailing unit back on ("0.30 v" -> "0.30v")
            while len(parts) > 1 and parts[-1].lower() in ("v", "vpp"):
                parts[-2:] = [parts[-2] + parts[-1]]
            try:
                f = float(parts[0])
                if len(parts) < 2:
                    i = s.amp_ma
                elif parts[1].lower().rstrip("pp").endswith("v"):
                    v = float(parts[1].lower().rstrip("pp").rstrip("v"))
                    i = v / 2.0 / s.rmon_ohm * 1e3
                    self.say("ask", f"{v:g} Vp-p / (2 x {s.rmon_ohm:g} ohm) "
                                    f"= {i:.0f} mA zero-to-peak")
                else:
                    i = float(parts[1])
            except (ValueError, IndexError):
                self.warn("want 'f,I' in Hz,mA -- e.g. 4.98,480 -- or the "
                          "scope reading, e.g. 4.98,0.28v")
                continue
            if f <= 0 or i <= 0:
                self.warn("both have to be positive")
                continue
            # far outside the bench's range = unit slip; a wrong current
            # is a wrong S with nothing later to catch it
            if not (I_SANE_MA[0] <= i <= I_SANE_MA[1]):
                self.warn(f"{i:g} mA is outside {I_SANE_MA[0]:g}-"
                          f"{I_SANE_MA[1]:g} mA -- milliamps, or volts "
                          f"peak-to-peak with a 'v'")
                continue
            point.f_actual, point.i_actual_ma = f, i
            return point

    # -- entry point ----------------------------------------------------

    def run(self):
        """The program: returns the exit code; the ledger is written on
        every exit path."""
        s = self.s
        s.started = clock.iso_now()
        try:
            self.validate()
            self.points, self.seed = build_points(s, self.warn)
            s.n_points = len(self.points)
            if s.mode == "plan":
                return self.do_plan()
            self.say("start", f"bfield sf_sweep, mode {s.mode}"
                              + (" [SIMULATED LAB]" if self.lab.sim else ""))
            if s.mode == "precheck":
                return self.do_precheck()
            if s.mode == "dc":
                return self.do_dc()
            return self.do_go()
        except BFieldError as e:
            self.ctx.error(f"ERROR[{e.code}] {type(e).__name__}: {e}")
            return 20 if isinstance(e, Stopped) else e.code
        except KeyboardInterrupt:
            self.ctx.error("stopped on Ctrl-C")
            return 20
        finally:
            self.ctx.close_log()

    # -- modes ----------------------------------------------------------

    def do_plan(self):
        s, points, seed = self.s, self.points, self.seed
        self.say("plan", f"{s.orientation} sweep -- "
                         f"{ORIENTATION_COIL[s.orientation]}, recording "
                         f"{head_pair(s) or s.map}")
        self.say("plan", f"{len(points)} points, control {s.control_hz:g} Hz "
                         f"first and last, shuffle seed {seed}")
        vpp = 2.0 * s.amp_ma * 1e-3 * s.rmon_ohm
        if monitor_name(s):
            self.say("plan", f"{s.amp_ma:g} mA into {s.rmon_ohm:g} ohm "
                             f"-> {vpp / 2:.2f} V zero-to-peak on "
                             f"{monitor_name(s)}, against a "
                             f"+/-{s.vrange:g} V window")
        else:
            self.say("plan", f"{s.amp_ma:g} mA into {s.rmon_ohm:g} ohm "
                             f"= {vpp:.3f} V peak-to-peak on the scope, with "
                             f"the 467A on gain x{AMP_GAIN:g}; the DAQ does "
                             f"not record it")
        total = sum(p.seconds for p in points)
        self.say("plan", f"  {'#':>3} {'f (Hz)':>8} {'s':>6} {'cycles':>8}"
                         f"  directory")
        for p in points:
            self.say("plan", f"  {p.index:>3} {p.f_hz:>8g} {p.seconds:>6.0f} "
                             f"{p.cycles:>8.0f}  {p.dirname}")
        nulls = 2 if s.null_seconds > 0 else 0
        span = total / 60 + len(points) + nulls * s.null_seconds / 60.0
        self.say("plan", f"recording {total / 60:.0f} min in {len(points)} "
                         f"points"
                 + (f", plus {nulls} drive-off records of "
                    f"{s.null_seconds:g} s" if nulls else "")
                 + f"; with the changeovers reckon {span:.0f} min "
                   f"({span / 60.0:.1f} h)")
        if s.skip_setup:
            self.say("plan", "--skip-setup: no bring-up; the heads are "
                             "assumed ready")
        else:
            plan = self.setup_plan("<outdir>/setup")
            self.say("plan", "first, once, the bring-up and Field Zero: "
                             f"sensor session setup --device-name "
                             f"{plan.device_name} --map {plan.map} --axes "
                             f"{plan.axes} --require-factors")
        demo = points[0]
        demo.f_actual, demo.i_actual_ma = demo.f_hz, s.amp_ma
        self.say("plan", "each point runs the recorder: " + " ".join(
            str(c) if " " not in str(c) else f'"{c}"'
            for c in self.record_args(demo, os.path.join("<outdir>",
                                                         demo.dirname))))
        self.say("plan", "afterwards: degauss (the sweep magnetized the "
                         "walls), then the analysis command this program "
                         "prints")
        return 0

    def _dated_outdir(self, kind):
        """The next dated run folder for this mode, or None with the
        old layout / an explicit --outdir."""
        s = self.s
        if s.outdir or not self.lab.dated:
            return None
        try:
            return self.lab.new_run_dir(
                s.name or f"Sf{kind}{s.orientation.capitalize()}")
        except (OSError, ValueError) as e:
            raise ConfigError(f"cannot make the run folder: {e}")

    def _open_outdir(self, prefix):
        s = self.s
        stamp = clock.strftime("%Y%m%d_%H%M%S")
        kind = {"precheck": "Precheck", "dc": "DC"}.get(prefix, prefix)
        outdir = s.outdir or self._dated_outdir(kind) or os.path.join(
            self.lab.bucket("sf"), f"{prefix}_{stamp}_{s.orientation}")
        self.ctx.open_log(os.path.join(outdir, "sf_sweep.log"))
        return outdir

    def precheck_drive(self, scope):
        """Read the drive, once the operator has had the chance to set it.
        With the generator still off, CRMs answers 9.9E37."""
        s = self.s
        while True:
            reply = self.text("  generator running, CH1 displayed, a whole "
                              "cycle on screen? Enter to read it, or 'skip': ")
            if reply.lower() == "skip":
                self.warn(f"no scope reading: the declared "
                          f"{s.control_hz:g} Hz and {s.amp_ma:g} mA go "
                          f"in unchecked")
                return None
            try:
                drive = scope.read_drive(s.rmon_ohm)
            except ScopeError as e:
                self.warn(f"the scope would not give a reading: {e}")
                continue
            f, i, dc, note = drive
            bad = scope_gate(f, i, s.control_hz, s.fs)
            if bad:
                self.say_drive(f, i, dc, note)
                self.warn(f"not using that reading: {bad} -- set the scope "
                          f"for {s.control_hz:g} Hz and try again")
                continue
            return drive

    def do_precheck(self):
        """One short run to prove the wiring before the sweep."""
        s = self.s
        # stamped directory: a fixed one made the second-ever precheck hit
        # the recorder's overwrite refusal (2026-08-20)
        outdir = self._open_outdir("precheck")
        p = Point(0, s.control_hz, s.precheck_seconds, True)
        p.f_actual, p.i_actual_ma = s.control_hz, s.amp_ma
        scope = self.open_scope("the precheck current")
        try:
            if not s.skip_setup:
                self.say("setup", "bringing the heads up and zeroing them "
                                  "first")
                s.cal_factors = self.run_setup(os.path.join(outdir, "setup"))
                self.say("setup", f"the record carries the calibration it "
                                  f"was taken under: --cal-factors "
                                  f"{s.cal_factors}")
            self.say("precheck", f"set the generator up, then let this run "
                                 f"{s.precheck_seconds:g} s:")
            for line in bench_hint(s.control_hz, s):
                self.say("hint", line)
            if scope is not None:
                drive = self.precheck_drive(scope)
                if drive is not None:
                    f, i, dc, note = drive
                    p.f_actual, p.i_actual_ma = f, i
                    p.from_scope = True
                    self.say_drive(f, i, dc, note)
            notes = (f"sf precheck {p.f_actual:g} Hz {p.i_actual_ma:g} mA "
                     f"rmon {s.rmon_ohm:g} ohm")
            code = self.run_recorder(
                self.record_args(p, outdir, seconds=s.precheck_seconds,
                                 notes=notes),
                s.precheck_seconds + CHILD_PAD_S, "precheck")
        finally:
            if scope is not None:
                scope.close()
        # judge only a record THIS run wrote
        if code not in (0, 30, 31):
            raise AbortSweep(
                f"the recorder exited {code}: this precheck has no record "
                f"of its own, so nothing in {self.rel(outdir)} is judged")
        verdict = judge(self.find_meta(outdir), p, s)
        self.report(p, verdict)
        if verdict["has_monitor"]:
            self.say("precheck", "monitor peak-to-peak "
                                 f"{verdict['monitor_ptp_V']!r} V; wanted "
                                 f"about "
                                 f"{2 * s.amp_ma * 1e-3 * s.rmon_ohm:.2f} V")
        else:
            self.say("precheck", "no DAQ monitor: check on the scope that the "
                                 f"{s.rmon_ohm:g} ohm resistor shows about "
                                 f"{2 * s.amp_ma * 1e-3 * s.rmon_ohm:.3f} "
                                 f"V peak-to-peak")
        current = (f"--monitor {monitor_name(s)} --rmon-ohm {s.rmon_ohm:g}"
                   if monitor_name(s) else f"--i-ma {p.i_actual_ma:g}")
        heads = head_pair(s)
        self.precheck_command = (
            f"python {_q(os.path.join(ANALYSIS_DIR, 'shield_sf_ac.py'))} "
            f"{_q(outdir)} --freqs {s.control_hz:g} {current} "
            f"--outdir {_q(os.path.join(outdir, 'analysis'))}"
            + (f" --heads {heads}" if heads else "")
            # simulated runs carry source "simulate", which the analysis
            # refuses unless told
            + (" --allow-sim" if self.lab.sim else ""))
        self.say("next", "if that looks right, put it through the analysis "
                         "once:")
        self.say("next", "  " + self.precheck_command)
        return 0

    def ask_polarity(self, sign, scope, first):
        """Get the switch thrown and the current that flowed. None = stop."""
        s = self.s
        want = "POSITIVE (+)" if sign == "+" else "NEGATIVE (-)"
        self.say("dc", f"set the drive {want}"
                       + (f", {s.dc_ma:g} mA" if first else " -- reverse it"))
        while True:
            reply = self.text(f"  switch thrown to {sign}? Enter to record "
                              f"{s.dc_dwell:g} s, 'stop' to end here: ")
            if reply.lower() == "stop":
                return None
            if scope is None:
                return s.dc_ma if sign == "+" else -s.dc_ma
            try:
                i_ma, note = scope.read_dc(s.rmon_ohm)
            except ScopeError as e:
                self.warn(f"the scope could not read the level ({e}) -- "
                          f"throw the switch again, or answer 'stop'")
                continue
            self.say("dc", f"monitor reads {i_ma:+.1f} mA"
                           + (f"  [{note}]" if note else ""))
            if (i_ma > 0) != (sign == "+"):
                self.warn(f"the monitor says {i_ma:+.1f} mA but this dwell "
                          f"is the {sign} one -- switch or leads reversed; "
                          f"fix it and press Enter again.")
                continue
            if abs(abs(i_ma) - s.dc_ma) > CURRENT_TELL_FRAC * s.dc_ma:
                self.warn(f"{abs(i_ma):.0f} mA is not the {s.dc_ma:g} mA "
                          f"asked for. Not an error -- the analysis divides "
                          f"by what flowed -- but check the supply is where "
                          f"you think.")
            return i_ma

    def dc_analysis_command(self, outdir):
        s = self.s
        return (f"python {_q(os.path.join(ANALYSIS_DIR, 'shield_sf_dc.py'))} "
                f"{_q(outdir)} --heads {head_pair(s) or 's1z,s2z'} "
                f"--settle {min(30.0, s.dc_dwell / 3.0):g} "
                f"--outdir {_q(os.path.join(outdir, 'analysis'))}")

    def dc_session_doc(self, outdir, dwells, nulls, outcome):
        s = self.s
        return {
            "program": "sf_sweep.py --dc",
            "software": "bfield.procedures.sf_sweep",
            "simulated": self.lab.sim,
            "started": s.started, "ended": clock.iso_now(),
            "outcome": outcome,
            "settings": {
                "orientation": s.orientation,
                "heads_analysed": head_pair(s),
                "pattern": "".join(DC_PATTERN),
                "dc_ma_asked": s.dc_ma, "dwell_s": s.dc_dwell,
                "blocks_asked": s.dc_blocks,
                "rmon_ohm": s.rmon_ohm, "coil_ohm": COIL_OHM,
                "amp_gain": AMP_GAIN, "k_geom_nT_per_mA": K_GEOM_NT_PER_MA,
                "map": s.map, "aux": s.aux, "ids": s.ids,
                "fs_hz": s.fs, "vrange": s.vrange, "gain": s.gain,
                "cal_v_per_nT": s.cal_v_per_nt,
            },
            "nulls": {k: (self.rel(v) if v else None)
                      for k, v in nulls.items()},
            "dwells": dwells,
            "analysis_command": self.dc_analysis_command(outdir),
            "events": self.ctx.events,
        }

    def do_dc(self):
        """S at DC, from dwells the operator reverses by hand."""
        s = self.s
        outdir = self._open_outdir("dc")
        session = os.path.join(outdir, "sf_dc_session.json")
        self.say("dc", f"{s.orientation}: {s.dc_blocks} blocks of "
                       f"{''.join(DC_PATTERN)} at {s.dc_dwell:g} s "
                       f"= {4 * s.dc_blocks * s.dc_dwell / 60.0:.0f} min of "
                       f"recording, plus the reversals between them")
        self.say("dc", f"{s.dc_ma:g} mA is "
                       f"{s.dc_ma * K_GEOM_NT_PER_MA:.0f} nT of applied field "
                       f"(free space, at the coil's centre)")
        scope = self.open_scope("the DC level")
        dwells, nulls, outcome = [], {}, "completed"
        try:
            if not s.skip_setup:
                self.say("setup", "bringing both heads up and zeroing them "
                                  "-- once, for the whole run")
                s.cal_factors = self.run_setup(os.path.join(outdir, "setup"))
                self.say("setup", f"every record carries the calibration it "
                                  f"was taken under: --cal-factors "
                                  f"{s.cal_factors}")
            if s.null_seconds > 0:
                nulls["before"] = self.record_null(outdir, "null_before",
                                                   "before the DC run")
            stop = False
            for block in range(1, s.dc_blocks + 1):
                for k, sign in enumerate(DC_PATTERN, start=1):
                    i_ma = self.ask_polarity(sign, scope,
                                             first=(block == 1 and k == 1))
                    if i_ma is None:
                        self.say("dc", "stopped by the operator")
                        stop = True
                        break
                    name = dc_dwell_name(block, k, sign)
                    d = os.path.join(outdir, name)
                    self.run_recorder(
                        self.record_args(None, d, seconds=s.dc_dwell,
                                         notes=f"sf DC {sign} {i_ma:+.1f} mA "
                                               f"block {block} dwell {k}"),
                        s.dc_dwell + CHILD_PAD_S, name)
                    dwells.append({"block": block, "index": k, "sign": sign,
                                   "i_ma": round(i_ma, 2),
                                   "dir": self.rel(d),
                                   "started": clock.iso_now(),
                                   "seconds": s.dc_dwell})
                    write_json_atomic(session, self.dc_session_doc(
                        outdir, dwells, nulls, "running"))
                if stop:
                    break
            if s.null_seconds > 0 and dwells:
                nulls["after"] = self.record_null(outdir, "null_after",
                                                  "after the DC run")
        except BFieldError as e:
            outcome = f"stopped: {e}"
            raise
        except KeyboardInterrupt:
            outcome = "stopped: Ctrl-C"
            raise
        except Exception as e:
            # a crash must label the session json "crashed", never
            # "completed" (one did, 2026-08-20)
            outcome = f"crashed: {e!r}"
            raise
        finally:
            if scope is not None:
                scope.close()
            with contextlib.suppress(Exception):
                os.makedirs(outdir, exist_ok=True)
                write_json_atomic(session, self.dc_session_doc(
                    outdir, dwells, nulls, outcome))
            whole = [b for b in range(1, s.dc_blocks + 1)
                     if sum(1 for d in dwells if d["block"] == b) == 4]
            if whole:
                self.say("next", "degauss now -- the run applied "
                                 f"{s.dc_ma * 1e-3 * K_GEOM_NT_PER_MA:.1f} uT"
                                 f" (free space, at the coil's centre) in "
                                 f"{len(dwells)} dwells")
                self.say("next", f"{len(whole)} complete block(s); analyse "
                                 f"with:")
                self.say("next", "  " + self.dc_analysis_command(outdir))
            elif dwells:
                self.warn(f"{len(dwells)} dwell(s) but no complete "
                          f"{''.join(DC_PATTERN)} block -- there is nothing "
                          f"the drift cancels out of")
        return 0

    def analysis_command(self, done):
        # --outdir per sweep: the analysis writes fixed filenames, and the
        # transverse run once replaced the axial summary (2026-08-19)
        s = self.s
        dirs = " ".join(_q(p.outdir) for p in done)
        freqs = ",".join(f"{p.f_actual:g}" for p in done)
        heads = head_pair(s)
        parent = os.path.dirname(os.path.abspath(done[0].outdir))
        out = os.path.join(parent, f"analysis_{s.orientation}")
        if monitor_name(s):
            current = (f"--monitor {monitor_name(s)} "
                       f"--rmon-ohm {s.rmon_ohm:g}")
        else:
            current = "--i-ma " + ",".join(f"{p.i_actual_ma:g}"
                                           for p in done)
        return (f"python {_q(os.path.join(ANALYSIS_DIR, 'shield_sf_ac.py'))} "
                f"{dirs} --freqs {freqs} {current} --outdir {_q(out)}"
                + (f" --heads {heads}" if heads else "")
                + (" --allow-sim" if self.lab.sim else ""))

    def session_doc(self, done, outcome, nulls=None):
        s, points = self.s, self.points
        return {
            "program": "sf_sweep.py",
            "software": "bfield.procedures.sf_sweep",
            "simulated": self.lab.sim,
            "started": s.started, "ended": clock.iso_now(),
            "outcome": outcome, "seed": self.seed,
            "settings": {
                "orientation": s.orientation,
                "setup_run": not s.skip_setup,
                "heads_analysed": head_pair(s),
                "freqs": s.freqs, "control_hz": s.control_hz,
                "amp_ma": s.amp_ma, "rmon_ohm": s.rmon_ohm,
                "coil_ohm": COIL_OHM, "amp_gain": AMP_GAIN,
                # scope-read f,I is different evidence from asserted f,I
                "from_scope": {p.dirname: bool(p.from_scope) for p in done},
                "dc_offset_ma": {p.dirname: p.dc_offset_ma for p in done},
                "map": s.map, "aux": s.aux, "ids": s.ids,
                "fs_hz": s.fs, "vrange": s.vrange, "gain": s.gain,
                "cal_v_per_nT": s.cal_v_per_nt,
                "min_cycles": s.min_cycles, "min_seconds": s.min_seconds,
            },
            "order": [p.f_hz for p in points],
            "points": [{
                "index": p.index,
                "dir": self.rel(p.outdir) if p.outdir else None,
                "f_planned_hz": p.f_hz, "f_set_hz": p.f_actual,
                "i_set_ma": p.i_actual_ma, "seconds": p.seconds,
                "is_control": p.is_control, "verdict": p.result,
            } for p in points],
            # sweep_nulls.py needs both nulls; "before" alone means the
            # magnetization control is missing
            "nulls": {k: (self.rel(v) if v else None)
                      for k, v in (nulls or {}).items()},
            "analysis_command": (self.analysis_command(done)
                                 if done else None),
            "events": self.ctx.events,
        }

    def do_go(self):
        s, points = self.s, self.points
        stamp = clock.strftime("%Y%m%d_%H%M%S")
        outdir = s.outdir or self._dated_outdir("Sweep") or os.path.join(
            self.lab.bucket("sf"), f"sw_{stamp}_{s.orientation}")
        require(not os.path.isdir(outdir) or not os.listdir(outdir),
                f"{self.rel(outdir)} already holds something -- a sweep wants "
                f"an empty parent, so give a fresh --outdir")
        os.makedirs(outdir, exist_ok=True)
        self.outdir = outdir
        self.ctx.open_log(os.path.join(outdir, "sf_sweep.log"))
        session = os.path.join(outdir, "sf_sweep_session.json")
        self.say("go", f"{s.orientation} sweep: "
                       f"{ORIENTATION_COIL[s.orientation]}")
        self.say("go", f"{len(points)} points into {self.rel(outdir)} "
                       f"(seed {self.seed})")
        scope = self.open_scope("each point's frequency and current")
        self.say("go", "the walls get magnetized by the drive: DEGAUSS AFTER "
                       "THIS")
        done, nulls, outcome = [], {}, "completed"
        self.done = done
        try:
            if s.skip_setup:
                self.warn("--skip-setup: the heads are taken to be running "
                          "and zeroed already -- nothing here has checked "
                          "that, and the records carry no calibration factors "
                          "from this run.")
            else:
                # Field Zero once per sweep, not per point: re-zeroing
                # moves the gain and would write itself into S(f)
                self.say("setup", "bringing both heads up and zeroing them "
                                  "-- once, for the whole sweep (cold start "
                                  "is about 3 min)")
                s.cal_factors = self.run_setup(os.path.join(outdir, "setup"))
                self.say("setup", f"every record carries the calibration it "
                                  f"was taken under: --cal-factors "
                                  f"{s.cal_factors}")
                self.say("setup", "zeroed. NOT re-done between points -- a "
                                  "mid-sweep re-zero splits the sweep into "
                                  "two measurements.")
            if s.null_seconds > 0:
                nulls["before"] = self.record_null(outdir, "null_before",
                                                   "before the sweep")
            for n, p in enumerate(points):
                self.ctx.progress(n / len(points), f"point {p.index}")
                if self.ask_setting(p, scope) is None:
                    self.say("point", f"{p.dirname}: skipped by the operator")
                    continue
                p.outdir = os.path.join(outdir, p.dirname)
                self.run_recorder(self.record_args(p, p.outdir),
                                  p.seconds + CHILD_PAD_S, p.dirname)
                p.result = judge(self.find_meta(p.outdir), p, s)
                self.report(p, p.result)
                done.append(p)
                write_json_atomic(session, self.session_doc(done, "running"))
            # null_after only on the success path: an aborted sweep drove
            # an unplanned field
            if s.null_seconds > 0:
                nulls["after"] = self.record_null(outdir, "null_after",
                                                  "after the sweep")
            self.ctx.progress(1.0, "sweep done")
        except BFieldError as e:
            outcome = f"stopped: {e}"
            raise
        except KeyboardInterrupt:
            outcome = "stopped: Ctrl-C"
            raise
        except Exception as e:
            outcome = f"crashed: {e!r}"
            raise
        finally:
            if scope is not None:
                scope.close()
            with contextlib.suppress(Exception):
                write_json_atomic(session, self.session_doc(done, outcome,
                                                            nulls))
            if done:
                self.say("next", "degauss now -- the sweep applied "
                                 f"{s.amp_ma * 1e-3 * K_GEOM_NT_PER_MA:.1f} uT"
                                 f" (free space, at the coil's centre) for "
                                 f"{len(done)} points")
                self.say("next", "then analyse, in run order, with what was "
                                 "actually set:")
                self.say("next", "  " + self.analysis_command(done))
        return 0
