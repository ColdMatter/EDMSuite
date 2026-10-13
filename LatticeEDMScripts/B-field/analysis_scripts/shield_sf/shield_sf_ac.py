#!/usr/bin/env python3
"""Shielding factor against frequency, S(f), from an AC coil sweep (B1).

A rectangular coil outside the shield is driven at one frequency per run
by hand; two QZFM heads inside read the leaked field on the same clocked
DAQ task, optionally with the coil current on a monitor resistor. For
each point,

    S(f) = B_applied(f) / B_inside(f) = k_geom * I(f) / B_inside(f)

and the sweep is the figure. This is the AC half. The other half is
shield_sf_dc.py, which reads the same coil driven at a steady current
whose polarity the operator reverses by hand: the generator stops at
0.1 Hz and the reversal band runs a decade below it, so the bottom of
the science band has no AC source on this bench. (A DC variant that
fitted stepped current levels in acquisition order lived here until
2026-08-10 and was removed when the plan was for this sweep to cover
0.05 Hz. It cannot -- the dial does not go there. The one that came
back orders its dwells + - - + so a linear drift cancels, which the
level fit never did.)

THE CIRCULARITY, AND WHERE IT BREAKS. The QZFM cannot operate outside
the shield -- it dies in the 50 uT of the earth's field -- so S cannot be
measured by carrying the sensor out and back. And the sensor cannot be
calibrated *through* the shield without already knowing S. In principle
the circle breaks in two steps that have to stay apart: a gain measured
against an INTERNAL coil, where S never enters, and then that gain used
with an EXTERNAL coil to measure S (here). Hence the rule that does
bind this program: **it must never be used to calibrate a sensor.** It
consumes a gain, it does not produce one.

WHAT THIS CAMPAIGN ACTUALLY DOES, WHICH IS NOT THAT. The gain is fixed
at the nominal 2.7 V/nT and cal_tone.py is NOT a prerequisite of a
sweep -- a standing decision (2026-07-30), not an omission, and the
reasons are worth keeping next to the code that would otherwise be
read as demanding one:

  - S = k I g / V, so g multiplies every point equally and cancels
    exactly in the SHAPE, S(f)/S(f_ref), which is most of the result.
  - In the absolute value the error budget is k_geom 5 percent >
    gain 3 percent > current 1 percent, so the gain is not the leading
    term, and the verdict it feeds is "is S above 1e6?" against models
    spread over 1.7e5 to 1.7e9. Three percent is not the digit in
    question.
  - 2.624 V/nT was measured, but on one head (AB3Q, 2026-07-08) and a
    two-head sweep needs one number both heads can be read with.

So the gain enters as a budgeted 3 percent, not as a step in the
procedure. Run cal_tone the day an error bar on the absolute S is
wanted; nothing before the sweep waits on it.

B_applied is the field the external coil would produce at the sensor
position WITH NO SHIELD PRESENT, k_geom * I. The shield does distort the
external field pattern, drawing flux into its walls -- and that
distortion is part of what "shielding factor" operationally means. The
free-space value is the definition, not an approximation to be corrected;
do not try to correct it.

WHAT IS ROBUST HERE AND WHAT IS NOT. k_geom is frequency-independent far
below the coil's self resonance, so it CANCELS in the shape S(f)/S(f0):
the frequency dependence -- the thing being measured -- needs only that
I(f) be known at each point, which is what the monitor resistor is for.
The absolute scale is a different matter: it carries the geometry
tolerance, the sensor gain, and the unbudgeted assumption that the
sensor sits near the coil centre. So the report gives both, and says
which is which.

TWO HEADS. Each head gives its own S and the two are printed side by
side. Their spread covers head-to-head gain mismatch and the real
position dependence of the leaked field inside the shield. It does NOT
cover anything common to both -- k_geom, the coil's placement, a gain
error shared by both heads -- so it is a consistency check, not a total
uncertainty, and the report keeps the two apart.

NOT DETECTED IS AN ANSWER. S rises steeply with frequency, so the
interior line disappears into the floor at the top of the sweep. Those
points become lower bounds at LIMIT_SIGMA and are drawn as upward
arrows (the truth lies above a floor); the bound is quoted at the
DETECTION threshold, so a point can never publish a bound above its
own point estimate.

Amplitudes are zero-to-peak throughout.

HOW A SWEEP IS TAKEN (2026-08-14: the acquisition wrapper sf point
tool was removed -- the generator is hand-set, so the recorder is run
directly). One qzfm_record run per frequency, every run with the SAME
fs / vrange / gain -- check_same_setup refuses a mixed sweep here at
analysis time -- and the hand-set values written into the run's notes
at the bench, e.g.

    python scripts\\qzfm_record.py --2head
        --map z:a:ai0,z2:b:ai1 --aux mon:ai2
        --fs 1200 --vrange 2 --gain 1x --cal-v-per-nT 2.7
        --seconds 1200 --notes "sf 0.1 Hz 500 mA rmon 1 ohm"

WHICH LINES, AND WHY fs IS PART OF THE ANSWER. The lines that matter
are physical: mains 50 / 100 / 150 Hz, the external 59.917 Hz comb, the
203.18 Hz GUI-clock leak (present whenever the QuSpin GUI is streaming),
and the cell modulation at 922.94 Hz with its 906.1 Hz sideband. Above
Nyquist they FOLD, so the list of frequencies to avoid is a property of
the sample rate, not of the bench:

    at fs = 1200: 277.1 and 293.9 (the carrier pair), everything else
                  where it already was -- nothing new lands under 100 Hz
    at fs =  200: the carrier pair lands at 77.1 and 93.9, and the
                  GUI-clock leak lands at 3.18 Hz -- inside the sweep

That last one is why this recipe records at 1200 even though the tone is
slow: at 200 Hz a point near 3.18 Hz sits on a folded line that no gate
here can tell from drive. A lower rate is allowed -- the analysis only
requires every run in a sweep to share it -- but recompute the folding
first, |f - round(f/fs)*fs| for each line above, and keep the sweep
clear of what comes back.

Bench rules that no tool enforces any more: each record needs >= 20
drive cycles (seconds >= 20/f); the frequency must stay clear of the
lines, or the drive and the line are inseparable; and the monitor must
stay inside the ADC window,

    I_0p x R_mon < 0.99 x vrange

which is the sizing rule for the resistor, not a per-point check:
--rmon-ohm is ONE value for the whole sweep, so the resistor is chosen
from the sweep's LARGEST current -- and that is the low-frequency end,
where the leaked line sits ~200x above the white band and needs an amp
of drive. This bench's amplifier is an HP 467A, whose ceiling is
0.5 A peak, and 0.5 A into 1 ohm reads 0.50 V on the +/-2 V window --
about 490 ADC codes of amplitude on this board's measured 1.02 mV step,
coarse in a single sample but far above what the lock-in needs over
600 s x 1200 Hz. The window would take up to 3.9 ohm at that current,
so a larger monitor resistor is allowed and doubles as ballast; what is
not allowed is sizing on the SMALL current, the trap being that 120 mA
into 100 ohm is a comfortable 12 V until the 0.5 A point rails it. The RECORDER tolerates a railed aux channel by
design (exit 0, its rail gate covers the field channels only), so the
mistake is invisible at the bench and only surfaces here -- and with
--rmon-ohm there is no way to rescue the point afterwards, because the
railed monitor IS the current measurement.

And the rule that costs nothing at the bench and cannot be repaired
afterwards: TAKE THE POINTS IN SHUFFLED FREQUENCY ORDER, and repeat the
first point last. An amp through the external coil puts a microtesla on
the walls, which magnetizes mu-metal, so S can drift over a session. In
frequency order that drift IS a smooth trend across the sweep, wearing
the shape this program exists to measure; shuffled, it becomes scatter
and the repeated point measures it. Degauss after the sweep. The
analysis warns when it sees a frequency-ordered sweep (ORDER_RHO_WARN),
but by then the session is over. Then the run directories are given
with the notes transcribed onto the command line:

    python shield_sf_ac.py runA runB --freqs 0.05,0.5 --rmon-ohm 1
    python shield_sf_ac.py runA runB --freqs 0.05,0.5 --i-ma 1000,1000
    python shield_sf_ac.py --test

Writes shield_sf_ac_summary.json and shield_sf_ac.png.
"""

from __future__ import annotations

import argparse
import glob
import json
import math
import os
import sys
from dataclasses import dataclass, field
from datetime import datetime

import numpy as np

# V/nT at 1x gain. The nominal is what a sweep uses: 2.624 was measured
# on head AB3Q alone (50 Hz match against the GUI, 2026-07-08) and does
# not carry to another head, let alone to both at once -- and a two-head
# sweep needs one number both heads can be read with. The recorder's own
# default for 1x is the same 2.7, so the two ends of the chain agree
# without anyone remembering a flag. The tolerance is the one datum there
# is on the spread: the head that was measured sat 2.8% below nominal.
GAIN_NOMINAL = 2.7
GAIN_TOL = 0.03
MU0 = 4e-7 * math.pi

COIL_TURNS = 2              # the lab coil, measured 2026-07-10; only a
COIL_W_M = 0.9              # fallback -- a sweep carries its own geometry
COIL_H_M = 1.2
K_TOL = 0.05                # geometry tolerance, sensor near coil centre
I_TOL = 0.01                # accuracy of a DECLARED current
RMON_TOL = 0.02             # how well the monitor resistor is known.
                            # 0.1 percent needs a four-wire reading;
                            # this bench's part is 0.3 ohm, where 20
                            # mohm of DMM lead is already 6.7 percent,
                            # so the default assumes a lead-nulled
                            # two-wire measurement. --rmon-tol says
                            # what was actually done.

DETECT_SIGMA = 5.0          # amplitude significance needed to quote S
LIMIT_SIGMA = 3.0           # confidence of the lower bound otherwise
MIN_CYCLES = 20.0           # per point, and per segment when segmenting
MAX_SEGMENTS = 8
HANN_NEB = 1.5              # noise-equivalent penalty of the Hann taper
                            # relative to an unwindowed least-squares fit
SEARCH_FRAC = 0.05          # the declared frequency is a hand-set dial
SEARCH_FRAC_MON = 0.3       # ...and on the monitor the window stays
                            # wide, so a misread dial is FOUND rather
                            # than measured as noise at the number the
                            # operator typed. What comes back is then
                            # refused unless it is the drive: the mains
                            # also reaches a monitor resistor, and a
                            # declared 40 Hz once selected 50 Hz
                            # (2026-08-19 review, H7)
MAINS_HZ = 50.0             # this bench; its harmonics reach the
                            # monitor resistor through the amplifier
MAINS_HARMONICS = 8
LOBE_BINS = 3
NOISE_SPAN = 12             # sideband half-width, in lobe widths
SPREAD_WARN = 0.2           # head-to-head disagreement worth saying twice
ORDER_RHO_WARN = 0.8        # run-time vs frequency rank correlation: above
                            # this the sweep was taken in frequency order and
                            # a magnetization drift would read as S(f) shape
S_SANE = (1e0, 1e12)        # wide on purpose: this catches a unit or gain
                            # blunder, not physics. Published shields reach
                            # 1e8 above 1 Hz and a lower bound can sit
                            # higher still, so a tight cap here would
                            # refuse real data
K_GEOM_SANE = (1e-2, 1e3)   # nT/mA

C_SERIES = ("#2E6E8E", "#C0504D", "#5A7D3A", "#8064A2")
C_MUTE, C_INK = "#7A7F85", "#33373B"


def require(ok, msg, code=13):
    if not ok:
        print(f"[error] {msg}", file=sys.stderr)
        raise SystemExit(code)


def k_geom_nt_per_ma(n_turns, w_m, h_m):
    """Field at the centre of a rectangular loop, nT/mA.

    The square limit is the textbook value.
    """
    a, b = w_m / 2.0, h_m / 2.0
    k_si = MU0 * n_turns * math.sqrt(a * a + b * b) / (math.pi * a * b)
    return k_si * 1e6


# ------------------------------------------------------------------ i/o

def find_meta(run_dir):
    metas = glob.glob(os.path.join(run_dir, "*.meta.json"))
    require(len(metas) == 1,
            f"{run_dir}: expected one meta.json, found {len(metas)}")
    with open(metas[0], encoding="utf-8") as fh:
        return json.load(fh), os.path.basename(metas[0])[:-len(".meta.json")]


def load_raw_csv(path):
    # line 1: session banner, line 2: column names, then the samples
    with open(path) as fh:
        fh.readline()
        names = fh.readline().strip().split(",")
    data = np.genfromtxt(path, delimiter=",", skip_header=2)
    return {n.removeprefix("V_"): data[:, i]
            for i, n in enumerate(names) if n.startswith("V_")}


def check_run(meta, path, force=False, allow_sim=False, soft_rail=()):
    """soft_rail: channels whose railing is tolerated with a warning --
    the monitor when --i-ma supplies the current, because then its
    amplitude is never consumed (a clipped sine still anchors the line
    frequency; phase and rel_stat from it stay suspect). The recorder
    itself tolerates a railed aux by design, so this gate is the only
    place the distinction can be made."""
    if not (meta.get("status") == "ok" and meta.get("exit_code") == 0):
        require(force, f"{path}: status={meta.get('status')} exit_code="
                       f"{meta.get('exit_code')}; --force to analyze anyway")
        print(f"[gate] {path}: overridden status={meta.get('status')}")
    nonfinite = meta.get("n_nonfinite") or {}
    require(all(v == 0 for v in nonfinite.values()),
            f"{path}: non-finite samples: {nonfinite}")
    for key in ("rail_fraction", "qzfm_rail_fraction"):
        rails = {k: v for k, v in (meta.get(key) or {}).items() if v > 0.001}
        soft = {k: v for k, v in rails.items() if k in soft_rail}
        hard = {k: v for k, v in rails.items() if k not in soft_rail}
        require(not hard, f"{path}: {key} above 0.1%: {hard}")
        if soft:
            print(f"[gate] {path}: {key} above 0.1% on {soft} -- "
                  f"tolerated (--i-ma supplies the current), but the "
                  f"monitor's phase and its share of rel_stat are "
                  f"unreliable")
    fs_eff = meta.get("fs_effective_hz")
    if fs_eff and meta.get("fs_hz") \
            and abs(fs_eff / meta["fs_hz"] - 1.0) >= 1e-3:
        print(f"[gate] {path}: fs_effective_hz {fs_eff:g} is >=0.1% off "
              f"fs_hz {meta['fs_hz']:g} (host-observed throughput, not "
              f"the DAQ clock -- but worth a look)")
    if meta.get("source") == "simulate":
        require(allow_sim,
                f"{path}: run is simulated; pass --allow-sim if intended")


def check_same_setup(metas, paths):
    """Every point must share the settings the comparison rests on.

    The operator keeps fs / vrange / gain fixed across the sweep by
    hand; this is where that discipline is checked, because a
    mid-sweep vrange change compares two different noise floors.
    """
    ref = metas[0]
    for meta, path in zip(metas[1:], paths[1:]):
        for key in ("gain_mode", "vrange", "fs_hz", "channels"):
            require(meta.get(key) == ref.get(key),
                    f"{path}: {key}={meta.get(key)!r} differs from "
                    f"{paths[0]}'s {ref.get(key)!r}")


# ----------------------------------------------------------- extraction

def locate_line(x, fs, f0, search_frac=SEARCH_FRAC):
    """Frequency of the strongest line near f0, at full-record resolution.

    The declared frequency is a hand-set dial reading, so it locates the
    search band and nothing more; everything downstream uses the line
    that is actually there.
    """
    n = x.size
    w = np.hanning(n)
    p = np.abs(np.fft.rfft((x - x.mean()) * w)) ** 2
    f = np.fft.rfftfreq(n, 1.0 / fs)
    span = max(search_frac * f0, LOBE_BINS * fs / n)
    band = np.flatnonzero((f >= f0 - span) & (f <= f0 + span) & (f > 0.0))
    require(band.size > 0,
            f"no spectral bin within {span:.4g} Hz of {f0:g} Hz -- the "
            f"record is too short for this frequency")
    k = int(band[int(np.argmax(p[band]))])
    lo, hi = max(1, k - LOBE_BINS), min(f.size, k + LOBE_BINS + 1)
    lobe = p[lo:hi]
    total = float(lobe.sum())
    return float(np.sum(f[lo:hi] * lobe) / total) if total > 0 else float(f[k])


def lockin(x, fs, f_hz):
    """Complex zero-to-peak amplitude at exactly f_hz.

    A projection rather than an FFT bin, so the frequency does not have
    to land on the grid and every segment uses the same reference.
    """
    n = x.size
    t = np.arange(n) / fs
    w = np.hanning(n)
    ref = np.exp(-2j * math.pi * f_hz * t)
    return 2.0 * np.sum((x - x.mean()) * w * ref) / np.sum(w)


def band_asd(x, fs, f_hz):
    """One-sided ASD near f_hz from the sidebands, units of x per rtHz.

    Sidebands, not the line: this is the floor the line has to stand
    out of, so the line itself must be excluded from it.
    """
    n = x.size
    w = np.hanning(n)
    p = np.abs(np.fft.rfft((x - x.mean()) * w)) ** 2
    f = np.fft.rfftfreq(n, 1.0 / fs)
    k = int(np.argmin(np.abs(f - f_hz)))
    lo, hi = max(1, k - LOBE_BINS), min(f.size, k + LOBE_BINS + 1)
    guard = NOISE_SPAN * LOBE_BINS
    side = np.r_[np.arange(max(1, k - guard), lo),
                 np.arange(hi, min(f.size, k + guard + 1))]
    if side.size == 0:
        return float("nan")
    return math.sqrt(2.0 * float(np.median(p[side]))
                     / (fs * float(np.sum(w ** 2))))


def n_segments(n, fs, f_hz):
    return max(1, int(min(MAX_SEGMENTS, (n / fs) * f_hz / MIN_CYCLES)))


def segments(n, k):
    L = n // k
    return [slice(i * L, (i + 1) * L) for i in range(k)]


@dataclass
class Line:
    amp: float              # zero-to-peak, units of the input
    sigma: float            # 1 sigma on amp, same units
    asd: float              # local floor, units per rtHz
    phase_rad: float
    n_seg: int
    sigma_from: str


def measure_line(x, fs, f_hz):
    """Amplitude, uncertainty and phase of the line at f_hz.

    Two independent uncertainties and the LARGER wins. The segment
    scatter sees drive wander and everything else that varies within the
    record but underestimates badly when there are few segments; the
    sideband floor sees the noise but not the drive. Taking the max
    means a missing tone gets the large sigma it deserves rather than
    the tight scatter of a noise-only fit.
    """
    a = lockin(x, fs, f_hz)
    amp = float(abs(a))
    asd = band_asd(x, fs, f_hz)
    seconds = x.size / fs
    sigma_noise = asd * math.sqrt(HANN_NEB / seconds)
    k = n_segments(x.size, fs, f_hz)
    sigma_seg = 0.0
    if k >= 4:
        amps = np.array([abs(lockin(x[s], fs, f_hz)) for s in segments(x.size, k)])
        sigma_seg = float(amps.std(ddof=1) / math.sqrt(k))
    sigma = max(sigma_noise, sigma_seg)
    return Line(amp=amp, sigma=sigma, asd=asd, phase_rad=float(np.angle(a)),
                n_seg=k, sigma_from=("segments" if sigma_seg > sigma_noise
                                     else "floor"))


# -------------------------------------------------------------- a point

@dataclass
class Head:
    name: str
    amp_v: float
    sigma_v: float
    amp_pt: float
    sigma_pt: float
    asd_ft: float
    detected: bool
    s: float                # point estimate, or the bound when not detected
    rel_stat: float         # statistical only, and the only part that
                            # differs between the two heads
    phase_deg: float = float("nan")


@dataclass
class Point:
    path: str
    session: str
    f_declared: float
    f_hz: float
    seconds: float
    fs: float
    i_ma: float
    i_source: str
    i_rel: float
    started: str = ""       # meta start_iso: the sweep's running order,
                            # which the frequency order must not track
    heads: list = field(default_factory=list)
    warnings: list = field(default_factory=list)

    @property
    def detected(self):
        return all(h.detected for h in self.heads)


def analyse_point(run_dir, args, k_nt_ma, gains):
    meta, session = find_meta(run_dir)
    check_run(meta, run_dir, force=args.force, allow_sim=args.allow_sim,
              soft_rail=(() if args.rmon_ohm else (args.monitor,)))

    f_declared = args._freq_of.get(os.path.abspath(run_dir))
    require(f_declared is not None,
            f"{run_dir}: no drive frequency. Pass --freqs with one value "
            f"per run, in run order (the bench wrote it into the run's "
            f"notes)")

    cols = load_raw_csv(os.path.join(run_dir, f"{session}_raw.csv"))
    fs = float(meta["fs_hz"])
    field_names = [n for n in args.heads.split(",") if n.strip() in cols]
    require(len(field_names) >= 1,
            f"{run_dir}: none of --heads {args.heads!r} is in the run "
            f"(has {','.join(sorted(cols))})")

    seconds = len(next(iter(cols.values()))) / fs
    cycles = f_declared * seconds
    warnings = []
    # this program consumes a gain, it never produces one -- but it can
    # at least notice being handed the wrong one: record stamps the gain
    # it was told into every meta
    used = meta.get("cal_v_per_nT_used")
    if used and abs(used / args.gain_v_per_nt - 1.0) > 0.01:
        warnings.append(
            f"run was recorded with cal_v_per_nT {used:g} but the "
            f"analysis gain is {args.gain_v_per_nt:g} V/nT: every "
            f"absolute S scales by the ratio (the shape is immune) -- "
            f"pass --gain-v-per-nt to match the sweep if this is not "
            f"deliberate")
    if cycles < MIN_CYCLES:
        warnings.append(f"only {cycles:.0f} cycles of {f_declared:g} Hz in "
                        f"{seconds:.0f} s: the line is not separable from "
                        f"the drift under it")

    # locate the line once, on the channel most likely to carry it: the
    # monitor when there is one (it sees the drive, not the shielded
    # remnant), otherwise the loudest field channel
    mon_name = args.monitor if args.monitor in cols else None
    anchor = mon_name or max(field_names, key=lambda n: float(cols[n].std()))
    f_hz = locate_line(cols[anchor], fs, f_declared,
                       SEARCH_FRAC_MON if mon_name else SEARCH_FRAC)
    # Only compare the two when there IS a line. On a point whose drive
    # never made it inside, locate_line returns the largest noise bin in
    # the window, and comparing a dial reading against noise refuses a
    # perfectly good bound: the 2026-08-19 precheck (207 mA for 60 s, an
    # expected 23 fT against a 28 fT sigma) was stopped exactly that way.
    # With nothing found, the frequency that was DRIVEN is the right one
    # to quote the bound at.
    anchor_line = measure_line(cols[anchor], fs, f_hz)
    anchor_snr = anchor_line.amp / max(anchor_line.sigma, 1e-30)
    anchor_seen = anchor_snr >= DETECT_SIGMA
    if not anchor_seen:
        warnings.append(
            f"no line at {f_declared:g} Hz on {anchor}: the strongest "
            f"thing in the search window is {anchor_snr:.1f} sigma, which "
            f"is noise. The declared frequency is kept and this point can "
            f"only be a bound")
        f_hz = float(f_declared)
    tol = max(3.0 * fs / len(cols[anchor]), 0.01 * f_declared)
    off = abs(f_hz - f_declared)
    require(not anchor_seen or off <= tol,
            f"{os.path.basename(run_dir)}: the strongest line is at "
            f"{f_hz:.5g} Hz but {f_declared:g} Hz was declared. The "
            f"dial reading and the data disagree by more than "
            f"{tol:.3g} Hz, so one of them is wrong and S would be "
            f"computed at the wrong frequency. Give the frequency "
            f"the scope measured, or drop the point")
    # A drive close to the mains is a real experiment; a SELECTED
    # line that sits on the mains while the declared one does not is
    # the search having found the room instead of the coil.
    for n in range(1, MAINS_HARMONICS + 1) if anchor_seen else ():
        line_hz = n * MAINS_HZ
        if abs(f_hz - line_hz) <= tol < abs(f_declared - line_hz):
            require(False,
                    f"{os.path.basename(run_dir)}: the search picked "
                    f"{f_hz:.5g} Hz, which is mains harmonic {n} "
                    f"({line_hz:g} Hz), not the {f_declared:g} Hz "
                    f"that was declared. Drive further from the "
                    f"mains, or give the measured frequency")

    mon_line = measure_line(cols[mon_name], fs, f_hz) if mon_name else None
    i_ma, i_source, i_rel = point_current(args, mon_line, run_dir)

    heads = []
    for name in field_names:
        g = gains.get(name, gains["_default"])
        line = measure_line(cols[name], fs, f_hz)
        detected = line.amp >= DETECT_SIGMA * line.sigma
        # the bound is quoted at the DETECTION threshold, so it can never
        # sit above the point estimate the same data would have given
        amp_for_s = line.amp if detected else max(
            line.amp + LIMIT_SIGMA * line.sigma, DETECT_SIGMA * line.sigma)
        b_pt = amp_for_s / g * 1e3
        rel_stat = (line.sigma / line.amp) if line.amp > 0 else float("inf")
        if mon_line is not None and mon_line.amp > 0:
            rel_stat = math.hypot(rel_stat, mon_line.sigma / mon_line.amp)
        phase = float("nan")
        if mon_line is not None:
            phase = math.degrees(
                (line.phase_rad - mon_line.phase_rad + math.pi)
                % (2.0 * math.pi) - math.pi)
        heads.append(Head(
            name=name, amp_v=line.amp, sigma_v=line.sigma,
            amp_pt=line.amp / g * 1e3, sigma_pt=line.sigma / g * 1e3,
            asd_ft=line.asd / g * 1e6, detected=detected,
            s=(k_nt_ma * i_ma * 1e3 / b_pt) if (i_ma and b_pt > 0) else
              float("nan"),
            rel_stat=rel_stat if detected else float("nan"),
            phase_deg=phase))

    if mon_line is not None and not (mon_line.amp
                                     >= DETECT_SIGMA * mon_line.sigma):
        warnings.append("the drive tone is not on the current monitor: this "
                        "point has no known applied field, so its S is not "
                        "a measurement of anything")
    return Point(path=run_dir, session=session, f_declared=float(f_declared),
                 f_hz=f_hz, seconds=seconds, fs=fs, i_ma=i_ma,
                 i_source=i_source, i_rel=i_rel,
                 started=str(meta.get("start_iso") or ""), heads=heads,
                 warnings=warnings)


def point_current(args, mon_line, run_dir):
    """Drive current in mA, where it came from, and its relative error."""
    if mon_line is not None and args.rmon_ohm:
        # A monitor lead that fell off gives amp 0, which used to
        # travel all the way to S = NaN in an exit-0 summary
        # (2026-08-19 review, C2).
        require(math.isfinite(mon_line.amp) and mon_line.amp > 0.0,
                f"{os.path.basename(run_dir)}: the current monitor "
                f"carries no tone (amplitude {mon_line.amp!r} V), so "
                f"this point has no measured applied field. Check "
                f"the monitor lead and the resistor; pass --i-ma if "
                f"the current is known from a meter instead")
        return (mon_line.amp / args.rmon_ohm * 1e3, "monitor",
                math.hypot(args.rmon_tol,
                           mon_line.sigma / max(mon_line.amp, 1e-30)))
    declared = args._i_of.get(os.path.abspath(run_dir))
    require(declared is not None,
            f"{run_dir}: no drive current. Give --rmon-ohm so the monitor "
            f"channel can supply it, or --i-ma per point (zero-to-peak; "
            f"the bench wrote the meter reading into the run's notes)")
    return float(declared), "declared", I_TOL


# ------------------------------------------------------------- the sweep

@dataclass
class Sweep:
    points: list
    k_nt_ma: float
    common_rel: float           # systematics shared by every head and point
    ref_index: int
    warnings: list = field(default_factory=list)


def combine(points, k_nt_ma, common_rel):
    """Per-point combined S and the head-to-head spread.

    The combination is a geometric mean because S is a ratio, and the
    uncertainty is the LARGER of propagated statistics and half the
    head-to-head spread: two heads that disagree by more than statistics
    allow have told us the statistics were not the whole story.
    """
    out = []
    for p in points:
        det = [h for h in p.heads if h.detected]
        if not det:
            out.append({"s": min((h.s for h in p.heads), default=float("nan")),
                        "rel": float("nan"), "spread": float("nan"),
                        "bound": True})
            continue
        logs = [math.log(h.s) for h in det]
        s = math.exp(sum(logs) / len(logs))
        rel_stat = math.sqrt(sum(h.rel_stat ** 2 for h in det)) / len(det)
        spread = (abs(det[0].s - det[1].s) / s) if len(det) == 2 \
            else float("nan")
        rel = max(rel_stat, 0.5 * spread if len(det) == 2 else 0.0)
        out.append({"s": s, "rel": rel, "rel_stat": rel_stat,
                    "spread": spread, "bound": False,
                    "rel_total": math.hypot(rel, common_rel)})
    return out


def order_correlation(points):
    """Spearman rho between the order the points were RECORDED and their
    frequency, or None when the run times are not all known.

    Driving the external coil at an amp puts a microtesla-scale field on
    the walls, which magnetizes mu-metal: S can then depend on the drive
    history. A sweep taken in frequency order turns that history into a
    smooth trend across the sweep -- exactly the shape S(f)/S(f0) claims
    to measure. Shuffling the order does not remove the magnetization,
    it makes it show up as scatter instead of as the answer.
    """
    if len(points) < 4 or not all(p.started for p in points):
        return None
    order = sorted(range(len(points)), key=lambda i: points[i].started)
    t_rank = [0] * len(points)
    for rank, i in enumerate(order):
        t_rank[i] = rank
    f_order = sorted(range(len(points)), key=lambda i: points[i].f_hz)
    f_rank = [0] * len(points)
    for rank, i in enumerate(f_order):
        f_rank[i] = rank
    n = len(points)
    d2 = sum((t_rank[i] - f_rank[i]) ** 2 for i in range(n))
    return 1.0 - 6.0 * d2 / (n * (n * n - 1))


def build_sweep(points, k_nt_ma, args):
    i_rel = max((p.i_rel for p in points), default=I_TOL)
    common_rel = math.sqrt(K_TOL ** 2 + GAIN_TOL ** 2 + i_rel ** 2)
    combined = combine(points, k_nt_ma, common_rel)
    for p, c in zip(points, combined):
        p.combined = c
    detected = [i for i, c in enumerate(combined) if not c["bound"]]
    ref = detected[0] if detected else 0
    warnings = []
    if not detected:
        warnings.append("no point in the sweep has a detected interior line: "
                        "every S here is a lower bound, and the shape cannot "
                        "be formed. More current or longer points")
    for p in points:
        det = [h.name for h in p.heads if h.detected]
        if det and len(det) < len(p.heads):
            p.warnings.append(
                f"only {','.join(det)} saw the line: one head measured it "
                f"and the other did not, so this point's S rests on a "
                f"single head and its head-to-head check is gone")
    for p, c in zip(points, combined):
        if not c["bound"] and c["spread"] > SPREAD_WARN:
            warnings.append(
                f"{p.session}: the two heads differ by "
                f"{c['spread'] * 100:.0f}%, more than measurement scatter. "
                f"That is position dependence inside the shield or a gain "
                f"mismatch between the heads, and it is now the uncertainty")
    rho = order_correlation(points)
    if rho is not None and abs(rho) >= ORDER_RHO_WARN:
        direction = "rising" if rho > 0 else "falling"
        warnings.append(
            f"the points were recorded in {direction} frequency order "
            f"(rank correlation {rho:+.2f} between run time and frequency): "
            f"any drift in the shield's magnetization over the session -- "
            f"and driving an amp through the external coil magnetizes it -- "
            f"is indistinguishable from frequency dependence here. Shuffle "
            f"the order, and repeat the first point last as a drift control")
    return Sweep(points=points, k_nt_ma=k_nt_ma, common_rel=common_rel,
                 ref_index=ref, warnings=warnings)


# ------------------------------------------------------------ reporting

def report(sweep, args):
    print(f"=== S(f) from {len(sweep.points)} points, "
          f"k_geom = {sweep.k_nt_ma:.4g} nT/mA ===")
    names = [h.name for h in sweep.points[0].heads] if sweep.points else []
    head_cols = "".join(f"{n + ' S':>12}" for n in names)
    print(f"{'f [Hz]':>9}{'I [mA]':>9}{'B_in [pT]':>11}"
          f"{'floor fT':>10}{head_cols}{'combined':>12}{'+-':>8}{'':>3}")
    for p in sweep.points:
        c = p.combined
        b_pt = sum(h.amp_pt for h in p.heads) / max(len(p.heads), 1)
        floor = sum(h.asd_ft for h in p.heads) / max(len(p.heads), 1)
        # ">" wherever the number is a bound, per head and combined --
        # without it a column of plain numbers reads as seven
        # measurements when some of them are floors
        cells = "".join(f"{'  ' if h.detected else ' >'}{h.s:10.3g}"
                        for h in p.heads)
        mark = " " if not c["bound"] else ">"
        rel = "" if c["bound"] else f"{c['rel'] * 100:7.1f}%"
        print(f"{p.f_hz:9.4g}{p.i_ma:9.4g}{b_pt:11.4g}{floor:10.4g}"
              f"{cells}{mark}{c['s']:11.3g}{rel:>8}")
    print()
    if sweep.points and not sweep.points[sweep.ref_index].combined["bound"]:
        ref = sweep.points[sweep.ref_index]
        print(f"[shape] normalised to {ref.f_hz:.4g} Hz. The shape carries "
              f"only the statistical error and the current measurement -- "
              f"k_geom and the sensor gain cancel in a ratio of two S "
              f"values, so they move the whole curve, never its form.")
    print(f"[syst ] common scale factor {sweep.common_rel * 100:.0f}% "
          f"(k_geom {K_TOL * 100:.0f}%, gain {GAIN_TOL * 100:.0f}%, "
          f"current) applies to every point equally and is NOT in the "
          f"per-point +- column")
    print("[syst ] the head-to-head spread covers gain mismatch and "
          "position dependence inside the shield; it cannot see anything "
          "both heads share")
    for w in sweep.warnings:
        print(f"[warn ] {w}")
    for p in sweep.points:
        for w in p.warnings:
            print(f"[warn ] {p.session}: {w}")


def summary_doc(sweep, args, inputs):
    ref = sweep.points[sweep.ref_index] if sweep.points else None
    s_ref = ref.combined["s"] if ref and not ref.combined["bound"] else None
    return {
        "program": "shield_sf_ac.py",
        "generated": datetime.now().isoformat(timespec="seconds"),
        "inputs": {"points": inputs, "k_nt_per_ma": sweep.k_nt_ma,
                   "args": {k: v for k, v in vars(args).items()
                            if not k.startswith("_")
                            and k not in ("run_dirs", "test")}},
        "amplitude_convention": "zero-to-peak",
        "systematics": {
            "common_relative": sweep.common_rel,
            "k_geom_relative": K_TOL, "gain_relative": GAIN_TOL,
            "note": "common terms scale every S together and cancel in the "
                    "S(f)/S(f_ref) shape",
        },
        "reference_hz": ref.f_hz if ref else None,
        "points": [{
            "session": p.session, "path": p.path,
            "f_declared_hz": p.f_declared, "f_hz": p.f_hz,
            "seconds": p.seconds, "i_ma": p.i_ma, "i_source": p.i_source,
            "S": p.combined["s"], "S_rel": p.combined.get("rel"),
            "S_rel_total": p.combined.get("rel_total"),
            "is_lower_bound": p.combined["bound"],
            "head_spread": p.combined.get("spread"),
            "shape": (None if (s_ref is None or p.combined["bound"])
                      else p.combined["s"] / s_ref),
            "heads": [{"name": h.name, "S": h.s, "detected": h.detected,
                       "amp_pT": h.amp_pt, "sigma_pT": h.sigma_pt,
                       "floor_fT_per_rtHz": h.asd_ft,
                       "rel_stat": h.rel_stat,
                       "phase_deg_vs_drive": h.phase_deg} for h in p.heads],
            "warnings": p.warnings,
        } for p in sweep.points],
        "warnings": sweep.warnings,
    }


def make_figure(outdir, sweep, args):
    import matplotlib
    matplotlib.use("Agg")
    import matplotlib.pyplot as plt
    plt.rcParams.update({
        "figure.dpi": 150, "font.size": 10, "axes.titleweight": "bold",
        "axes.spines.top": False, "axes.spines.right": False,
        "axes.edgecolor": C_MUTE, "text.color": C_INK, "legend.frameon": False,
    })
    has_phase = any(not math.isnan(h.phase_deg)
                    for p in sweep.points for h in p.heads)
    nrows = 3 if has_phase else 2
    fig, axes = plt.subplots(nrows, 1, figsize=(7.2, 3.2 * nrows),
                             sharex=True)
    ax = axes[0]
    names = [h.name for h in sweep.points[0].heads]
    for j, name in enumerate(names):
        color = C_SERIES[j % len(C_SERIES)]
        # two heads that agree land on top of each other, and a hidden
        # series reads as a missing one: give every head its own marker
        # and hollow all but the first
        marker = ("o", "s", "D", "v")[j % 4]
        face = color if j == 0 else "none"
        xs, ys, es = [], [], []
        bx, by = [], []
        for p in sweep.points:
            h = next((h for h in p.heads if h.name == name), None)
            if h is None or not math.isfinite(h.s):
                continue
            if h.detected:
                xs.append(p.f_hz)
                ys.append(h.s)
                es.append(h.s * (h.rel_stat if math.isfinite(h.rel_stat)
                                 else 0.0))
            else:
                bx.append(p.f_hz)
                by.append(h.s)
        ax.errorbar(xs, ys, yerr=es, fmt=marker, ms=6, capsize=3,
                    color=color, mfc=face, label=f"head {name}")
        if bx:
            # a bound: the value is a floor, so the arrow points up
            ax.errorbar(bx, by, yerr=[[0] * len(by), [v * 0.8 for v in by]],
                        fmt=marker, ms=6, color=color, mfc=face, alpha=0.7,
                        lolims=True, label=f"head {name}, lower bound")
    ax.set_xscale("log")
    ax.set_yscale("log")
    ax.set_ylabel("shielding factor S")
    ax.set_title("S(f) of the shield, external rectangular coil")
    ax.legend(fontsize=8, ncol=2)
    ax.grid(True, which="both", alpha=0.15)
    ref = sweep.points[sweep.ref_index]
    if not ref.combined["bound"]:
        rax = ax.twinx()
        rax.set_yscale("log")
        lo, hi = ax.get_ylim()
        rax.set_ylim(lo / ref.combined["s"], hi / ref.combined["s"])
        rax.set_ylabel(f"S / S({ref.f_hz:.3g} Hz)   [k-free]", fontsize=9)
        rax.spines["right"].set_visible(True)
        rax.spines["top"].set_visible(False)

    ax = axes[1]
    xs = [p.f_hz for p in sweep.points
          if len(p.heads) == 2 and p.detected]
    ratio = [p.heads[0].s / p.heads[1].s for p in sweep.points
             if len(p.heads) == 2 and p.detected]
    ax.axhline(1.0, color=C_MUTE, ls=":", lw=1.0)
    ax.plot(xs, ratio, "o-", ms=5, lw=1.0, color=C_SERIES[2])
    ax.set_xscale("log")
    ax.set_ylabel(f"S({names[0]}) / S({names[1] if len(names) > 1 else '-'})")
    ax.set_title("head-to-head consistency (gain mismatch + position, "
                 "not a total error)", fontsize=9)
    ax.grid(True, which="both", alpha=0.15)

    if has_phase:
        ax = axes[2]
        for j, name in enumerate(names):
            color = C_SERIES[j % len(C_SERIES)]
            xs = [p.f_hz for p in sweep.points
                  for h in p.heads if h.name == name and h.detected]
            ys = [h.phase_deg for p in sweep.points
                  for h in p.heads if h.name == name and h.detected]
            ax.plot(xs, ys, "o-", ms=5, lw=1.0, color=color, label=name)
        ax.axhline(0.0, color=C_MUTE, ls=":", lw=1.0)
        ax.set_xscale("log")
        ax.set_ylabel("phase of B_in vs drive [deg]")
        ax.set_title("diagnostic: eddy-current lag looks different from "
                     "non-magnetic pickup", fontsize=9)
        ax.grid(True, which="both", alpha=0.15)
    axes[-1].set_xlabel("drive frequency [Hz]")
    fig.tight_layout()
    path = os.path.join(outdir, "shield_sf_ac.png")
    fig.savefig(path, bbox_inches="tight")
    plt.close(fig)
    return path


# ------------------------------------------------------------- the runs

def gather_inputs(args):
    """-> run directories, one per sweep point."""
    require(args.run_dirs, "give the recorded run directories, one per "
                           "drive frequency")
    for d in args.run_dirs:
        require(os.path.isdir(d), f"{d}: not a directory")
    return list(args.run_dirs)


def parse_per_point(spec, dirs, what):
    if spec is None:
        return {}
    vals = [t for t in spec.split(",") if t.strip()]
    require(len(vals) == len(dirs),
            f"--{what} has {len(vals)} values for {len(dirs)} runs")
    try:
        return {os.path.abspath(d): float(v) for d, v in zip(dirs, vals)}
    except ValueError:
        require(False, f"--{what} {spec!r} is not a list of numbers")


def run_analysis(args):
    dirs = gather_inputs(args)
    args._freq_of = parse_per_point(args.freqs, dirs, "freqs")
    args._i_of = parse_per_point(args.i_ma, dirs, "i-ma")

    k_nt_ma = (args.k_ext if args.k_ext else
               k_geom_nt_per_ma(args.n_turns, args.coil_w, args.coil_h))
    require(K_GEOM_SANE[0] <= k_nt_ma <= K_GEOM_SANE[1],
            f"k_geom = {k_nt_ma:g} nT/mA is outside {K_GEOM_SANE}")
    gains = {"_default": args.gain_v_per_nt}

    metas = []
    points = []
    for d in dirs:
        meta, _ = find_meta(d)
        metas.append(meta)
        points.append(analyse_point(d, args, k_nt_ma, gains))
    check_same_setup(metas, dirs)
    points.sort(key=lambda p: p.f_hz)
    for p in points:
        finite_s = [h.s for h in p.heads if math.isfinite(h.s)]
        require(finite_s,
                f"{p.session}: no head produced a finite S. That is "
                f"a dead current monitor or a dead field channel, "
                f"not a shielding factor")
        require(S_SANE[0] <= max(finite_s) <= S_SANE[1],
                f"{p.session}: S outside {S_SANE} -- check the current, the "
                f"gain and the coil geometry before believing this")

    sweep_res = build_sweep(points, k_nt_ma, args)
    report(sweep_res, args)

    os.makedirs(args.outdir, exist_ok=True)
    doc = summary_doc(sweep_res, args, dirs)
    path = os.path.join(args.outdir, "shield_sf_ac_summary.json")
    with open(path, "w", newline="\n") as fh:
        json.dump(doc, fh, indent=1)
    print(f"[out  ] {path}")
    if not args.no_figures:
        print(f"[out  ] {make_figure(args.outdir, sweep_res, args)}")
    return 0


# ----------------------------------------------------------- self-test

K_TEST = 250.0          # nT/mA, a coil nothing else in the suite uses
G_TEST = 2.62           # V/nT
RMON_TEST = 100.0       # ohm
FS_TEST = 200.0
SEED = 20260810
# the measured white band of this bench (22-28 fT/rtHz, 2026-07/08). The
# tests were written once with a floor 30x quieter than that, and every
# "this line is buried" case came back detected -- a synthetic floor has
# to be the real one or the limit branch is never exercised.
FLOOR_FT_TEST = 25.0
NOISE_V_TEST = FLOOR_FT_TEST * 1e-6 * G_TEST * math.sqrt(FS_TEST / 2.0)


def write_fake_point(root, session, *, f_hz, s_true, i_ma, seconds=120.0,
                     noise_v=NOISE_V_TEST, heads=("z", "z2"),
                     head_scale=(1.0, 1.0),
                     with_monitor=True, f_actual=None, status="ok",
                     vrange=2.0, phase_deg=0.0, rng=None, hour=0):
    """One synthetic sweep point with a known S, written like a real run."""
    rng = rng or np.random.default_rng(SEED)
    d = os.path.join(root, session)
    os.makedirs(d, exist_ok=True)
    n = int(seconds * FS_TEST)
    t = np.arange(n) / FS_TEST
    f_true = f_hz if f_actual is None else f_actual

    b_in_nt = K_TEST * i_ma / s_true                 # nT amplitude inside
    cols = {}
    for name, scale in zip(heads, head_scale):
        v = (b_in_nt * scale * G_TEST
             * np.cos(2 * math.pi * f_true * t
                      + math.radians(phase_deg)))
        cols[name] = v + rng.normal(0.0, noise_v, n)
    if with_monitor:
        # amplitude in volts across the monitor resistor, from I in mA
        cols["mon"] = (i_ma * 1e-3 * RMON_TEST * np.cos(2 * math.pi
                                                        * f_true * t)
                       + rng.normal(0.0, 1e-6, n))

    names = list(cols)
    with open(os.path.join(d, f"{session}_raw.csv"), "w",
              newline="\n") as fh:
        fh.write(f"# QZFM raw voltages | session={session} "
                 f"start=2026-08-10T00:00:00 fs={FS_TEST}\n")
        fh.write("sample," + ",".join(f"V_{k}" for k in names) + "\n")
        for i in range(n):
            fh.write(f"{i}," + ",".join(f"{cols[k][i]:.9g}"
                                        for k in names) + "\n")
    meta = {
        "session": session, "software": "qzfm_record.py", "topology": "2head",
        "start_iso": f"2026-08-10T{hour:02d}:00:00",
        "status": status, "exit_code": 0 if status == "ok" else 30,
        "fs_hz": FS_TEST, "n_samples": n, "vrange": vrange, "gain_mode": "1x",
        "cal_v_per_nT_used": G_TEST, "source": "test",
        "channels": {k: f"Dev/ai{i}" for i, k in enumerate(heads)},
        "n_nonfinite": {k: 0 for k in names},
        "rail_fraction": {k: 0.0 for k in names},
        "qzfm_rail_fraction": {k: 0.0 for k in heads},
    }
    with open(os.path.join(d, f"{session}.meta.json"), "w",
              newline="\n") as fh:
        json.dump(meta, fh, indent=1)
    return d


def write_fake_sweep(root, spec, order=None, **kw):
    """Point dirs named p001_<freq>Hz -- the declared frequency lives in
    the name, the way the bench writes it into the run's notes.

    order: the hour each point was recorded, one per spec entry, when a
    test needs the running order to differ from the frequency order.
    """
    os.makedirs(root, exist_ok=True)
    rng = np.random.default_rng(SEED)
    for i, (f_hz, s_true, i_ma) in enumerate(spec, start=1):
        write_fake_point(root, f"p{i:03d}_{f_hz:g}Hz".replace(".", "p"),
                         f_hz=f_hz, s_true=s_true, i_ma=i_ma, rng=rng,
                         hour=(order[i - 1] if order else i), **kw)
    return root


def sweep_argv(root, extra=()):
    """The command an analyst would type for a fixture sweep: run dirs
    plus --freqs transcribed from the names (i.e. from the notes)."""
    dirs = sorted(d for d in glob.glob(os.path.join(root, "p[0-9]*"))
                  if os.path.isdir(d))
    freqs = ",".join(
        f"{float(os.path.basename(d).split('_')[1][:-2].replace('p', '.')):g}"
        for d in dirs)
    return [*dirs, "--freqs", freqs,
            "--k-ext", f"{K_TEST:g}", "--gain-v-per-nt", f"{G_TEST:g}",
            "--rmon-ohm", f"{RMON_TEST:g}", "--outdir", root, *extra]


def analysed(root, extra=()):
    """Run the analysis on a fixture sweep and hand back its summary."""
    p = build_parser()
    args = p.parse_args(sweep_argv(root, ["--no-figures", *extra]))
    args._freq_of = {}
    args._i_of = {}
    rc = run_analysis(args)
    assert rc == 0, rc
    with open(os.path.join(root, "shield_sf_ac_summary.json"),
              encoding="utf-8") as fh:
        return json.load(fh)


def run_self_test():
    if not __debug__:
        print("[test] refusing to run under python -O: asserts are "
              "stripped, a pass would be vacuous")
        return 1
    import shutil
    import tempfile
    failures = []
    tmp = tempfile.mkdtemp(prefix="shield_sf_ac_test_")

    def check(name, fn):
        try:
            fn()
            print(f"[test] PASS: {name}")
        except AssertionError as e:
            failures.append(name)
            print(f"[test] FAIL: {name}: {e}")
        except SystemExit as e:
            failures.append(name)
            print(f"[test] FAIL: {name}: exited {e.code}")

    def test_k_geom_arithmetic():
        # square limit against the textbook value, and the lab coil
        k = k_geom_nt_per_ma(1, 0.5, 0.5)
        want = MU0 * 1 * math.sqrt(2 * 0.25 ** 2) / (math.pi * 0.25 ** 2) * 1e6
        assert abs(k - want) < 1e-9, (k, want)
        assert abs(k_geom_nt_per_ma(COIL_TURNS, COIL_W_M, COIL_H_M)
                   - 2.2222) < 1e-3

    def test_lockin_is_zero_to_peak():
        n, fs, f0 = 4000, 200.0, 3.0
        t = np.arange(n) / fs
        x = 0.7 * np.cos(2 * math.pi * f0 * t + 0.4)
        a = lockin(x, fs, f0)
        assert abs(abs(a) - 0.7) < 1e-3, abs(a)
        assert abs(np.angle(a) - 0.4) < 1e-3, np.angle(a)
        # a peak-to-peak convention would be out by exactly two
        assert abs(abs(a) - 1.4) > 0.5

    def test_recovers_known_s():
        root = write_fake_sweep(os.path.join(tmp, "known"),
                                [(0.5, 3.0e4, 100.0), (2.0, 1.0e5, 100.0),
                                 (8.0, 3.0e5, 100.0)])
        doc = analysed(root)
        for want, pt in zip((3.0e4, 1.0e5, 3.0e5), doc["points"]):
            assert not pt["is_lower_bound"], pt
            assert abs(pt["S"] / want - 1.0) < 0.05, (pt["S"], want)
            for h in pt["heads"]:
                assert h["detected"] and abs(h["S"] / want - 1.0) < 0.08, h

    def test_shape_is_free_of_the_coil_constant():
        """The whole point of the normalisation: doubling k doubles every
        S and leaves S(f)/S(f_ref) untouched."""
        root = write_fake_sweep(os.path.join(tmp, "shape"),
                                [(0.5, 3.0e4, 100.0), (2.0, 1.0e5, 100.0),
                                 (8.0, 3.0e5, 100.0)])
        a = analysed(root)
        b = analysed(root, extra=["--k-ext", f"{2 * K_TEST:g}"])
        for pa, pb in zip(a["points"], b["points"]):
            assert abs(pb["S"] / pa["S"] - 2.0) < 1e-6, (pa["S"], pb["S"])
            assert abs(pb["shape"] - pa["shape"]) < 1e-6, (pa, pb)

    def test_buried_line_is_a_bound_not_a_number():
        # the second point is driven 100x weaker into a 1000x better
        # shield, so its interior amplitude (2.5 fT) is under the floor
        root = write_fake_sweep(os.path.join(tmp, "buried"),
                                [(2.0, 1.0e5, 100.0), (8.0, 1.0e8, 1.0)])
        doc = analysed(root)
        lo, hi = doc["points"]
        assert not lo["is_lower_bound"] and hi["is_lower_bound"], doc
        # the bound may not sit above what the same data would estimate
        assert hi["S"] > 0 and math.isfinite(hi["S"])
        for h in hi["heads"]:
            assert not h["detected"], h

    def test_two_heads_disagreeing_becomes_the_uncertainty():
        """A 30% gain mismatch between the heads must not be averaged
        away into a tight error bar."""
        root = write_fake_sweep(os.path.join(tmp, "spread"),
                                [(2.0, 1.0e5, 100.0)], head_scale=(1.0, 1.3))
        doc = analysed(root)
        pt = doc["points"][0]
        assert pt["head_spread"] > 0.2, pt
        assert pt["S_rel"] >= 0.5 * pt["head_spread"] - 1e-9, pt
        assert any("differ by" in w for w in doc["warnings"]), doc["warnings"]

    def test_monitor_and_declared_current_agree():
        root = write_fake_sweep(os.path.join(tmp, "cur"),
                                [(2.0, 1.0e5, 100.0)])
        mon = analysed(root)["points"][0]
        assert mon["i_source"] == "monitor", mon
        # without the monitor the analyst types the meter reading from
        # the run's notes as --i-ma
        loose = analysed(root, extra=["--monitor", "nosuchchannel",
                                      "--i-ma", "100"])
        dec = loose["points"][0]
        assert dec["i_source"] == "declared", dec
        assert abs(dec["S"] / mon["S"] - 1.0) < 0.02, (dec["S"], mon["S"])

    def test_dial_and_data_disagreeing_is_refused():
        """S at the wrong frequency is not a measurement, so a dial
        that disagrees with the data stops the analysis."""
        root = write_fake_sweep(os.path.join(tmp, "dial"),
                                [(2.0, 1.0e5, 100.0)])
        # the same data, relabelled: the generator was really at 2.4 Hz
        shutil.rmtree(root)
        write_fake_sweep(os.path.join(tmp, "dial"), [(2.0, 1.0e5, 100.0)],
                         f_actual=2.4)
        try:
            analysed(root)
        except SystemExit as e:
            assert e.code == 13, e.code
        else:
            raise AssertionError("2.4 Hz data was analysed as 2.4 Hz "
                                 "while 2.0 Hz was declared")
        # declared truthfully, the same samples analyse
        shutil.rmtree(root)
        write_fake_sweep(os.path.join(tmp, "dial"),
                         [(2.4, 1.0e5, 100.0)], f_actual=2.4)
        assert abs(analysed(root)["points"][0]["f_hz"] - 2.4) < 0.05

    def test_the_mains_is_not_mistaken_for_the_drive():
        """A 40 Hz declaration must not settle on the 50 Hz line the
        wide monitor window can reach."""
        root = write_fake_sweep(os.path.join(tmp, "mains"),
                                [(40.0, 1.0e5, 100.0)], f_actual=50.0)
        try:
            analysed(root)
        except SystemExit as e:
            assert e.code == 13, e.code
        else:
            raise AssertionError("the mains was analysed as the drive")

    def test_frequency_ordered_sweep_is_flagged():
        """A sweep taken low-to-high cannot separate a magnetization
        drift from S(f); shuffled, the same points can."""
        spec = [(0.5, 1.0e5, 100.0), (1.0, 1.0e5, 100.0),
                (2.0, 1.0e5, 100.0), (4.0, 1.0e5, 100.0),
                (8.0, 1.0e5, 100.0)]
        flag = "recorded in"
        ordered = analysed(write_fake_sweep(os.path.join(tmp, "inorder"),
                                            spec))
        assert any(flag in w for w in ordered["warnings"]), ordered["warnings"]
        # same five frequencies, recorded shuffled
        shuffled = analysed(write_fake_sweep(os.path.join(tmp, "shuffled"),
                                             spec, order=[3, 5, 1, 4, 2]))
        assert not any(flag in w for w in shuffled["warnings"]), \
            shuffled["warnings"]

    def test_gates_refuse_bad_input():
        root = os.path.join(tmp, "gates")
        write_fake_sweep(root, [(2.0, 1.0e5, 100.0)])
        write_fake_point(root, "p002_2Hz_bad", f_hz=2.0, s_true=1e5,
                         i_ma=100.0, status="saturated")
        try:
            analysed(root)
        except SystemExit as e:
            assert e.code == 13, e.code
        else:
            raise AssertionError("a saturated run was analysed")
        # ... and the same run passes with --force
        analysed(root, extra=["--force"])

        mixed = os.path.join(tmp, "mixed")
        write_fake_sweep(mixed, [(2.0, 1.0e5, 100.0)])
        write_fake_point(mixed, "p002_4Hz", f_hz=4.0, s_true=1e5, i_ma=100.0,
                         vrange=10.0)
        try:
            analysed(mixed)
        except SystemExit as e:
            assert e.code == 13, e.code
        else:
            raise AssertionError("points with different vrange were compared")

    def test_figure_is_written():
        root = write_fake_sweep(os.path.join(tmp, "fig"),
                                [(0.5, 3.0e4, 100.0), (2.0, 1.0e5, 100.0),
                                 (8.0, 1.0e8, 1.0)])
        p = build_parser().parse_args(sweep_argv(root))
        p._freq_of = p._i_of = {}
        assert run_analysis(p) == 0
        png = os.path.join(root, "shield_sf_ac.png")
        assert os.path.exists(png) and os.path.getsize(png) > 5000

    print("=== shield_sf_ac --test: synthetic checks "
          "(temp dir, removed after) ===")
    try:
        for name, fn in (
                ("test_k_geom_arithmetic", test_k_geom_arithmetic),
                ("test_lockin_is_zero_to_peak", test_lockin_is_zero_to_peak),
                ("test_recovers_known_s", test_recovers_known_s),
                ("test_shape_is_free_of_the_coil_constant",
                 test_shape_is_free_of_the_coil_constant),
                ("test_buried_line_is_a_bound_not_a_number",
                 test_buried_line_is_a_bound_not_a_number),
                ("test_two_heads_disagreeing_becomes_the_uncertainty",
                 test_two_heads_disagreeing_becomes_the_uncertainty),
                ("test_monitor_and_declared_current_agree",
                 test_monitor_and_declared_current_agree),
                ("test_dial_and_data_disagreeing_is_refused",
                 test_dial_and_data_disagreeing_is_refused),
                ("test_the_mains_is_not_mistaken_for_the_drive",
                 test_the_mains_is_not_mistaken_for_the_drive),
                ("test_frequency_ordered_sweep_is_flagged",
                 test_frequency_ordered_sweep_is_flagged),
                ("test_gates_refuse_bad_input", test_gates_refuse_bad_input),
                ("test_figure_is_written", test_figure_is_written)):
            check(name, fn)
    finally:
        shutil.rmtree(tmp, ignore_errors=True)
    if failures:
        print(f"[test] {len(failures)} FAILED: {', '.join(failures)}")
        return 1
    print("[test] all passed")
    return 0


def build_parser():
    p = argparse.ArgumentParser(
        allow_abbrev=False,
        description=__doc__,
        formatter_class=argparse.RawDescriptionHelpFormatter)
    p.add_argument("run_dirs", nargs="*",
                   help="qzfm_record run directories, one per drive "
                        "frequency")
    p.add_argument("--freqs", default=None,
                   help="drive frequency per run in Hz, in run order "
                        "(transcribed from each run's notes)")
    p.add_argument("--i-ma", default=None,
                   help="drive current amplitude per run in mA, in run order")
    p.add_argument("--heads", default="z,z2",
                   help="interior channels to analyse (default z,z2)")
    p.add_argument("--monitor", default="mon",
                   help="aux channel carrying the coil current")
    p.add_argument("--rmon-ohm", type=float, default=None,
                   help="monitor resistor in ohms, as measured")
    p.add_argument("--rmon-tol", type=float, default=RMON_TOL,
                   help=f"fractional uncertainty on --rmon-ohm "
                        f"(default {RMON_TOL:g}, a lead-nulled "
                        f"two-wire DMM reading; use 0.001 for "
                        f"four-wire). S goes as 1/R, so this lands "
                        f"whole in the common scale factor")
    p.add_argument("--n-turns", type=int, default=COIL_TURNS)
    p.add_argument("--coil-w", type=float, default=COIL_W_M)
    p.add_argument("--coil-h", type=float, default=COIL_H_M)
    p.add_argument("--k-ext", type=float, default=None,
                   help="coil constant in nT/mA, instead of the geometry")
    p.add_argument("--gain-v-per-nt", type=float, default=GAIN_NOMINAL,
                   help=f"sensor gain in V/nT, applied to every head "
                        f"(default {GAIN_NOMINAL:g} = nominal at 1x, which "
                        f"is also what the recorder stamps; the absolute S "
                        f"scales with it and the shape does not)")
    p.add_argument("--outdir", default=".", help="where the outputs go")
    p.add_argument("--no-figures", action="store_true")
    p.add_argument("--force", action="store_true",
                   help="analyse runs whose status is not ok")
    p.add_argument("--allow-sim", action="store_true")
    p.add_argument("--test", action="store_true", help="offline self-tests")
    p.add_argument("--inject-check", nargs="+", metavar="CHANNEL.txt",
                   help="add known tones to real recorded channels and "
                        "report what the estimator gives back")
    return p


def inject_check(paths, freqs=(0.05, 0.3, 3.0), levels=(200.0, 20.0, 2.0, 0.5),
                 fs=1200.0, phi=0.7, clear=20.0, slack=1.3):
    """Does measure_line give back the amplitude that was put in?

    --test proves each function against data the test wrote, which cannot
    catch an error the functions share: a window normalisation, a
    zero-to-peak slip, a sign in the phase. Only a known tone added to a
    REAL recording, pushed through this estimator, can (REPORT_PLAN §18.4).

    The tolerance is declared before measuring, or the check proves
    nothing. The recording already holds something at f, and an injected
    line adds to it as a phasor, so the error to expect is bounded by
    A0/A with A0 what the estimator returns with nothing injected. An
    error at that bound is the recording; one above it is this code.
    """
    import numpy as np
    bad = []
    for path in paths:
        x = np.loadtxt(path)
        print(f"\n{path}: {x.size / fs:.0f} s, std {x.std():.3g}")
        t = np.arange(x.size) / fs
        for f in freqs:
            a0 = measure_line(x, fs, f).amp        # what is already there
            print(f"  f = {f:g} Hz, already present: {a0:.3f}")
            print(f"    {'A in':>8} {'A out':>9} {'err %':>8} {'bound %':>8} "
                  f"{'A/sigma':>9} {'phase err':>10}  detected")
            for amp in levels:
                line = measure_line(x + amp * np.sin(2*math.pi*f*t + phi), fs, f)
                # the projection is onto exp(-i w t), so sin(w t + phi)
                # lands at phi - pi/2
                err_deg = math.degrees(
                    (line.phase_rad - (phi - math.pi/2) + math.pi)
                    % (2*math.pi) - math.pi)
                err = 100.0 * (line.amp - amp) / amp
                bound = 100.0 * a0 / amp
                snr = line.amp / line.sigma
                det = line.amp >= DETECT_SIGMA * line.sigma
                print(f"    {amp:8.2f} {line.amp:9.3f} {err:8.2f} {bound:8.2f} "
                      f"{snr:9.1f} {err_deg:9.2f} deg  {det}")
                if snr >= clear and abs(err) > slack * bound:
                    bad.append(f"{path} {f} Hz A={amp}: {err:+.2f}% "
                               f"over the {bound:.2f}% phasor bound")
            if measure_line(x, fs, f).amp >= DETECT_SIGMA * measure_line(
                    x, fs, f).sigma:
                bad.append(f"{path} {f} Hz: a bare record was called a "
                           f"detection")
    if bad:
        print("\nFAILED:")
        for msg in bad:
            print(f"  {msg}")
        return 1
    print("\nevery level recovers within the phasor bound set by the "
          "recording itself, and no bare record is called a detection")
    return 0


def main(argv=None):
    argv = list(sys.argv[1:] if argv is None else argv)
    p = build_parser()
    args = p.parse_args(argv)
    args._freq_of = {}
    args._i_of = {}
    if args.test:
        return run_self_test()
    if args.inject_check:
        return inject_check(args.inject_check)
    if not args.run_dirs:
        p.print_help()
        return 13
    return run_analysis(args)


if __name__ == "__main__":
    raise SystemExit(main())
