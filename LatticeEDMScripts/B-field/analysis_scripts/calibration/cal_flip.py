#!/usr/bin/env python3
"""Separate the sensor zero offset from the real DC field by a 180-degree
flip (A3b).

A single reading is m = B_lab + zero, with zero head-fixed and nT-class.
Flipping the head reverses the lab field's projection and leaves zero in
place: B_lab = (m0 - m180)/2, zero_eff = (m0 + m180)/2. The one hard rule
of the measurement: Field Zero ONCE at orientation 0 and never again --
re-zeroing re-nulls the total field and destroys the arithmetic.
zero_eff contains the intrinsic zero plus the head's own compensation-coil
field; splitting those would need the unlogged GUI currents, so the sum is
reported as such. Analog runs only (the GUI stream carries a path offset).
Writes cal_flip_summary.json, cal_flip_timeseries.png and ledger lines.
Offline check: python cal_flip.py --test
"""

from __future__ import annotations

import argparse
import glob
import json
import os
import shutil
import sys
from dataclasses import dataclass
from datetime import datetime

import numpy as np
from scipy.optimize import curve_fit

GAIN_MEASURED = 2.624   # V/nT at 1x (sync5, 50 Hz two-readout method)
GAIN_TOL = 0.03         # relative, until the A1 tone calibration lands
SETTLE_S = 500.0        # wait after Field Zero / any head move
ZERO_LIMIT_PT = 50000.0     # compensation range +/-50 nT
B_LAB_LIMIT_PT = 10000.0    # a closed-shield residual beyond 10 nT is not real
SIM_SEED = 20260707

import sys as _sys  # analysis_scripts/ on sys.path for _paths
_sys.path.insert(0, os.path.dirname(os.path.dirname(os.path.abspath(__file__))))
from _paths import DATA_ROOT, PROJECT_DIR, ANALYSIS_DIR, data, data_join, rel_data  # noqa: E402,F401
# run refs in the ledger are relative to the data root
REPO_ROOT = DATA_ROOT
DEFAULT_LEDGER = data("calibration", "ledger.jsonl")

RUN_NAMES = ("A0", "B180", "A0'")
C_RUNS = ("#2E6E8E", "#C0504D", "#5A7D3A")
C_MUTE, C_INK = "#7A7F85", "#33373B"


def require(ok, msg, code=13):
    if not ok:
        print(f"[error] {msg}", file=sys.stderr)
        raise SystemExit(code)


# ---------------------------------------------------------------- loading

def find_meta(run_dir):
    metas = glob.glob(os.path.join(run_dir, "*.meta.json"))
    require(len(metas) == 1, f"{run_dir}: expected one meta.json, found {len(metas)}")
    with open(metas[0], encoding="utf-8") as fh:
        return json.load(fh), os.path.basename(metas[0])[:-len(".meta.json")]


def check_run(meta, path, force=False, allow_sim=False):
    """Acceptance gate, calibration variant: rails are never forceable --
    a railed flip run is worthless."""
    if not (meta.get("status") == "ok" and meta.get("exit_code") == 0):
        require(force, f"{path}: status={meta.get('status')} exit_code="
                       f"{meta.get('exit_code')}; --force to analyze anyway")
        print(f"[gate] {path}: overridden status={meta.get('status')}")
    nonfinite = meta.get("n_nonfinite") or {}
    require(all(v == 0 for v in nonfinite.values()),
            f"{path}: non-finite samples: {nonfinite}")
    for key in ("rail_fraction", "qzfm_rail_fraction"):
        rails = {k: v for k, v in (meta.get(key) or {}).items() if v > 0.001}
        require(not rails, f"{path}: {key} above 0.1%: {rails} -- the head "
                           "did not hold zero after the flip (never forceable)")
    if meta.get("source") == "simulate":
        require(allow_sim, f"{path}: run is simulated; pass --allow-sim if intended")


def pt_correction(meta, gain_true):
    # pT_file = V * 1000/g_used  ->  pT_true = pT_file * g_used/g_true
    return float(meta["cal_v_per_nT_used"]) / gain_true


def load_axis(run_dir, session, axis, meta, args):
    if args.raw:
        path = os.path.join(run_dir, f"{session}_raw.csv")
        require(os.path.exists(path), f"missing {path}")
        with open(path) as fh:
            fh.readline()
            names = fh.readline().strip().split(",")
        data = np.genfromtxt(path, delimiter=",", skip_header=2)
        col = f"V_{axis}"
        require(col in names, f"{path}: no column {col}")
        return data[:, names.index(col)] * 1000.0 / args.gain_v_per_nt, 1.0
    path = os.path.join(run_dir, f"{session}_{axis.upper()}_pT.txt")
    require(os.path.exists(path), f"missing {path}")
    factor = pt_correction(meta, args.gain_v_per_nt)
    return np.loadtxt(path, comments="#") * factor, factor


# ---------------------------------------------------------------- stats

def block_stats(x, fs, block_s=60.0):
    """DC and drift with honest low-frequency errors. Returns
    (mean, sem, drift_per_min, drift_sem_per_min, n_blocks)."""
    n = int(block_s * fs)
    nb = x.size // n
    require(nb >= 3, f"record too short for block stats ({nb} blocks)")
    means = x[:nb * n].reshape(nb, n).mean(axis=1)
    tmin = (np.arange(nb) + 0.5) * block_s / 60.0
    mean = float(means.mean())
    sem = float(means.std(ddof=1) / np.sqrt(nb))
    A = np.vstack([tmin, np.ones(nb)]).T
    coef, res, *_ = np.linalg.lstsq(A, means, rcond=None)
    resid = means - A @ coef
    s2 = float(resid @ resid) / max(nb - 2, 1)
    cov00 = s2 * np.linalg.inv(A.T @ A)[0, 0]
    return mean, sem, float(coef[0]), float(np.sqrt(cov00)), nb


def settle_residual(x, fs, trim_s):
    """Amplitude of any settling exponential still alive at the analysis
    window start: fit A*exp(-t/tau)+c on 1 s means of the trimmed-away
    head of the record, extrapolate to t=trim. Zero when no transient."""
    n = int(fs)
    span = int(min(trim_s + 300.0, x.size / fs - 60.0))
    if span < 120:
        return 0.0
    y = x[:span * n].reshape(span, n).mean(axis=1)
    t = np.arange(span) + 0.5

    def model(tt, A, tau, c):
        return A * np.exp(-tt / tau) + c

    try:
        popt, _ = curve_fit(model, t, y, p0=[y[0] - y[-1], 185.0, y[-1]],
                            maxfev=20000)
    except Exception:
        return 0.0
    A, tau, c = popt
    resid = float(np.std(y - model(t, *popt)))
    if abs(A) < 5 * resid or not (10.0 < tau < 600.0):
        return 0.0
    return float(abs(A) * np.exp(-trim_s / tau))


# ---------------------------------------------------------------- runs

@dataclass
class RunMeans:
    name: str
    path: str
    mid_epoch_s: float
    trim_s: float
    correction: float
    mean_pt: dict
    sem_pt: dict
    drift_pt_min: dict
    settle_resid_pt: dict
    warnings: list


def load_run_means(name, run_dir, axes, args):
    meta, session = find_meta(run_dir)
    check_run(meta, run_dir, args.force, args.allow_sim)
    fs = float(meta["fs_hz"])
    warnings = []
    # "zero" alone matched every "field zero done" note, so the loud
    # warning fired on healthy runs and stopped being read. Look for
    # a re-zero, which is what invalidates a flip pair
    # (2026-08-19 analysis review, M3).
    notes = (meta.get("notes") or "").lower()
    rezeroed = any(w in notes for w in
                   ("re-zero", "rezero", "re zero", "zeroed again",
                    "second zero"))
    if rezeroed:
        warnings.append(f"{name}: the notes say the head was re-zeroed "
                        "between orientations -- THE RESULT IS INVALID")
    start = datetime.fromisoformat(meta["start_iso"]).timestamp()
    mid = start + float(meta["duration_s"]) / 2.0

    dur = float(meta["duration_s"])
    trim = SETTLE_S if dur >= 700.0 else 60.0
    if dur < 700.0:
        warnings.append(f"{name}: run is only {dur:.0f} s; trimmed just 60 s")

    mean, sem, drift, resid = {}, {}, {}, {}
    correction = None
    for axis in axes:
        x, factor = load_axis(run_dir, session, axis, meta, args)
        correction = factor
        n0 = int(trim * fs)
        m, s, d, _, _ = block_stats(x[n0:], fs)
        mean[axis], sem[axis], drift[axis] = m, s, d
        resid[axis] = settle_residual(x, fs, trim)
        if resid[axis] > s:
            warnings.append(f"{name}/{axis}: settling not complete "
                            f"(residual {resid[axis]:.1f} pT > sem {s:.1f})")
    return RunMeans(name=name, path=run_dir, mid_epoch_s=mid, trim_s=trim,
                    correction=correction, mean_pt=mean, sem_pt=sem,
                    drift_pt_min=drift, settle_resid_pt=resid,
                    warnings=warnings), meta, fs


def check_same_setup(metas, paths):
    ref = metas[0]
    for meta, path in zip(metas[1:], paths[1:]):
        for key in ("gain_mode", "vrange", "channel_map", "fs_hz"):
            require(meta.get(key) == ref.get(key),
                    f"{path}: {key}={meta.get(key)!r} differs from "
                    f"{paths[0]}'s {ref.get(key)!r} -- the flip arithmetic "
                    "needs an unchanged setup")


# ---------------------------------------------------------------- flip

@dataclass
class FlipResult:
    axis: str
    flipped: bool
    m0_pt: float
    m180_pt: float
    m0p_pt: float | None
    b_lab_pt: float | None
    b_lab_stat: float | None
    b_lab_syst: float | None
    zero_eff_pt: float | None
    zero_eff_stat: float | None
    drift_syst_pt: float | None
    shifted: bool | None      # non-flipped consistency check
    warnings: list


def combine_a_runs(a0, a0p, axis, b_mid):
    """Time-weighted A-mean whose effective time equals the B180 mid-time;
    this cancels a linear ambient drift exactly (the ABA point)."""
    t0, t1 = a0.mid_epoch_s, a0p.mid_epoch_s
    w = (t1 - b_mid) / (t1 - t0)
    m = w * a0.mean_pt[axis] + (1.0 - w) * a0p.mean_pt[axis]
    sem = float(np.hypot(w * a0.sem_pt[axis], (1.0 - w) * a0p.sem_pt[axis]))
    return m, sem


def analyze_axis(axis, flipped, runs, args):
    a0, b180 = runs[0], runs[1]
    a0p = runs[2] if len(runs) == 3 else None
    warnings = []
    gap_min = (b180.mid_epoch_s - a0.mid_epoch_s) / 60.0

    if a0p is not None:
        m0, sem0 = combine_a_runs(a0, a0p, axis, b180.mid_epoch_s)
        drift_syst = 0.5 * abs(a0.drift_pt_min[axis]
                               - a0p.drift_pt_min[axis]) * gap_min
    else:
        m0, sem0 = a0.mean_pt[axis], a0.sem_pt[axis]
        drift_syst = abs(a0.drift_pt_min[axis]) * gap_min / 2.0
        warnings.append(f"{axis}: two-run mode; drift systematic "
                        f"{drift_syst:.1f} pT is one-sided -- use ABA")
    m180, sem180 = b180.mean_pt[axis], b180.sem_pt[axis]

    if not flipped:
        shifted = abs(m0 - m180) > 3.0 * float(np.hypot(sem0, sem180))
        if shifted:
            warnings.append(f"{axis} (not flipped) moved by "
                            f"{m0 - m180:+.1f} pT between runs -- setup moved?")
        return FlipResult(axis=axis, flipped=False, m0_pt=m0, m180_pt=m180,
                          m0p_pt=a0p.mean_pt[axis] if a0p else None,
                          b_lab_pt=None, b_lab_stat=None, b_lab_syst=None,
                          zero_eff_pt=None, zero_eff_stat=None,
                          drift_syst_pt=None, shifted=shifted,
                          warnings=warnings)

    b_lab = (m0 - m180) / 2.0
    zero_eff = (m0 + m180) / 2.0
    stat = 0.5 * float(np.hypot(sem0, sem180))
    cell_syst = args.gradient_pt_per_m * args.cell_offset_mm / 1000.0
    gain_syst = GAIN_TOL * abs(b_lab)
    settle = max(r.settle_resid_pt[axis] for r in runs) / 2.0
    syst = float(np.sqrt(drift_syst ** 2 + cell_syst ** 2 + gain_syst ** 2
                         + settle ** 2))

    require(abs(zero_eff) < ZERO_LIMIT_PT,
            f"{axis}: zero_eff = {zero_eff / 1000.0:.1f} nT exceeds the "
            "compensation range -- re-zeroed between runs? wrong --flipped?")
    require(abs(b_lab) < B_LAB_LIMIT_PT,
            f"{axis}: B_lab = {b_lab / 1000.0:.2f} nT is not a closed-shield "
            "residual -- re-zeroed between runs? wrong --flipped?")

    return FlipResult(axis=axis, flipped=True, m0_pt=m0, m180_pt=m180,
                      m0p_pt=a0p.mean_pt[axis] if a0p else None,
                      b_lab_pt=b_lab, b_lab_stat=stat, b_lab_syst=syst,
                      zero_eff_pt=zero_eff, zero_eff_stat=stat,
                      drift_syst_pt=drift_syst, shifted=None,
                      warnings=warnings)


# ---------------------------------------------------------------- figure

def make_figure(outdir, traces, runs, results, flipped_axes):
    import matplotlib
    matplotlib.use("Agg")
    import matplotlib.pyplot as plt
    plt.rcParams.update({
        "figure.dpi": 150, "font.size": 11, "axes.titleweight": "bold",
        "axes.spines.top": False, "axes.spines.right": False,
        "axes.edgecolor": C_MUTE, "text.color": C_INK, "legend.frameon": False,
    })
    others = sorted(set(traces[runs[0].name]) - set(flipped_axes))
    nrows = 1 + bool(others)
    fig, axes = plt.subplots(nrows, 1, figsize=(8.0, 3.6 * nrows),
                             squeeze=False, sharex=True)
    t0 = runs[0].mid_epoch_s

    ax = axes[0][0]
    for run, color in zip(runs, C_RUNS):
        for axis in flipped_axes:
            t, y = traces[run.name][axis]
            tm = (t - t0) / 60.0
            ax.plot(tm, y, lw=0.8, color=color)
            ax.hlines(run.mean_pt[axis], tm[0], tm[-1], color=color, ls="--",
                      lw=1.0)
        ax.annotate(run.name, ((run.mid_epoch_s - t0) / 60.0,
                               ax.get_ylim()[1]), fontsize=9, color=color,
                    ha="center", va="top")
    r = next(r for r in results if r.flipped)
    ax.set_title(f"flip sequence: B_lab({r.axis}) = (m0 - m180)/2 = "
                 f"{r.b_lab_pt:+.1f} pT, zero_eff = {r.zero_eff_pt:+.1f} pT")
    ax.set_ylabel("field [pT] (0.1 Hz low-pass)")

    if others:
        ax = axes[1][0]
        for run, color in zip(runs, C_RUNS):
            for axis in others:
                t, y = traces[run.name][axis]
                ax.plot((t - t0) / 60.0, y, lw=0.8, color=color)
        ax.set_ylabel("non-flipped axes [pT]")
        ax.set_title("consistency: these should not move")
    axes[-1][0].set_xlabel("time from A0 mid [min]")
    fig.savefig(os.path.join(outdir, "cal_flip_timeseries.png"),
                bbox_inches="tight")
    plt.close(fig)


def lowpass_trace(run_dir, session, axis, meta, args, fs, mid_epoch, trim_s):
    x, _ = load_axis(run_dir, session, axis, meta, args)
    n = int(fs)
    nb = x.size // n
    y = x[:nb * n].reshape(nb, n).mean(axis=1)
    from scipy.signal import butter, sosfiltfilt
    sos = butter(3, 0.1, btype="low", fs=1.0, output="sos")
    y = sosfiltfilt(sos, y)
    t = mid_epoch - x.size / fs / 2.0 + np.arange(nb) + 0.5
    keep = np.arange(nb) >= trim_s
    return t[keep], y[keep]


# ---------------------------------------------------------------- pipeline

def run_analysis(args):
    require(len(args.runs) in (2, 3),
            f"need 2 or 3 runs (A0 B180 [A0']), got {len(args.runs)}")
    flipped_axes = [a.strip().lower() for a in args.flipped.split(",") if a.strip()]
    require(flipped_axes, "--flipped lists no axes")

    metas, sessions, fss = [], [], []
    for path in args.runs:
        meta, session = find_meta(path)
        metas.append(meta)
        sessions.append(session)
    check_same_setup(metas, args.runs)
    axes = list(metas[0]["axes"])
    missing = [a for a in flipped_axes if a not in axes]
    require(not missing, f"--flipped names {missing}, but the runs carry "
                         f"axes {axes}")

    runs = []
    for name, path in zip(RUN_NAMES, args.runs):
        rm, meta, fs = load_run_means(name, path, axes, args)
        runs.append(rm)
        fss.append(fs)
    order = [r.mid_epoch_s for r in runs]
    require(order == sorted(order),
            "run mid-times are not in A0, B180, A0' order -- check the "
            "positional arguments")
    gap_min = (runs[1].mid_epoch_s - runs[0].mid_epoch_s) / 60.0

    used_aba = len(runs) == 3
    head = metas[0].get("sensor_id", "?")
    print(f"=== cal_flip: position {args.position}, head {head}, "
          f"{'ABA' if used_aba else 'two-run'} ===")
    stamps = " | ".join(
        f"{r.name} {datetime.fromtimestamp(r.mid_epoch_s).strftime('%H:%M')}"
        for r in runs)
    print(f"[epoch] {stamps} (mid)   gap {gap_min:.1f} min")
    mode = "raw volts" if args.raw else "pT files"
    print(f"[path ] {mode}, epoch correction "
          + ", ".join(f"{r.name} x{r.correction:.4f}" for r in runs))
    for r in runs:
        for w in r.warnings:
            print(f"[warn] {w}")

    results = []
    for axis in axes:
        res = analyze_axis(axis, axis in flipped_axes, runs, args)
        results.append(res)
        for w in res.warnings:
            print(f"[warn] {w}")
        if res.flipped:
            print(f"[{axis} ] m0 {res.m0_pt:+.1f}  m180 {res.m180_pt:+.1f}"
                  + (f"  (ABA m0 weighted to B mid)" if used_aba else ""))
            print(f"     B_lab({axis}) = {res.b_lab_pt:+.1f} pT  +/- "
                  f"{res.b_lab_stat:.1f} stat  +/- {res.b_lab_syst:.1f} syst "
                  f"(drift {res.drift_syst_pt:.1f})")
            print(f"     zero_eff({axis}) = {res.zero_eff_pt:+.1f} pT +/- "
                  f"{res.zero_eff_stat:.1f} pT   [in the orientation-0 "
                  "head frame; includes the compensation-coil field]")
        else:
            print(f"[chk] {axis} (not flipped): m0 {res.m0_pt:+.1f} vs m180 "
                  f"{res.m180_pt:+.1f} pT"
                  + ("  SHIFTED (setup moved?)" if res.shifted else "  ok"))

    any_sim = any(m.get("source") == "simulate" for m in metas)
    skip_ledger = any_sim and \
        os.path.abspath(args.ledger) == os.path.abspath(DEFAULT_LEDGER)
    if skip_ledger:
        print("[note] simulated input: not writing to the shared ledger")
    run_ref = ",".join(os.path.relpath(os.path.abspath(p), REPO_ROOT)
                       if os.path.abspath(p).startswith(REPO_ROOT + os.sep)
                       else p for p in args.runs)
    all_warnings = [w for r in (*runs, *results) for w in r.warnings]
    valid = not any("INVALID" in w for w in all_warnings)
    for res in results:
        if skip_ledger or not res.flipped:
            continue
        base = {"tool": "cal_flip.py", "run": run_ref, "axis": res.axis,
                "head": head, "gain_mode": metas[0].get("gain_mode"),
                "conditions": f"position {args.position}, "
                              f"{'ABA' if used_aba else 'two-run'}, "
                              f"gap {gap_min:.1f} min"}
        # A pair the run said was INVALID produced these numbers too,
        # and they went in unmarked -- cal_stability then read them
        # as measurements (2026-08-19 analysis review, H3).
        ledger_append({"kind": "field_flip", "value": round(res.b_lab_pt, 2),
                       "unc": round(float(np.hypot(res.b_lab_stat,
                                                   res.b_lab_syst)), 2),
                       "unit": "pT", "pass": valid, **base}, args.ledger)
        ledger_append({"kind": "zero_flip", "value": round(res.zero_eff_pt, 1),
                       "unc": round(res.zero_eff_stat, 2), "unit": "pT",
                       "pass": valid, **base}, args.ledger)

    os.makedirs(args.outdir, exist_ok=True)
    summary = {
        "program": "cal_flip.py",
        "generated": datetime.now().isoformat(timespec="seconds"),
        "inputs": {"runs": list(args.runs),
                   "args": {k: v for k, v in vars(args).items()
                            if k not in ("runs", "test")}},
        "results": {
            "position": args.position, "head": head, "used_aba": used_aba,
            "gap_min": round(gap_min, 2),
            "runs": [{"name": r.name, "path": r.path,
                      "pt_correction": r.correction, "trim_s": r.trim_s,
                      "warnings": r.warnings} for r in runs],
            "axes": [{
                "axis": r.axis, "flipped": r.flipped,
                "m0_pt": r.m0_pt, "m180_pt": r.m180_pt, "m0p_pt": r.m0p_pt,
                "b_lab_pt": r.b_lab_pt, "b_lab_stat": r.b_lab_stat,
                "b_lab_syst": r.b_lab_syst,
                "zero_eff_pt": r.zero_eff_pt,
                "zero_eff_stat": r.zero_eff_stat,
                "drift_syst_pt": r.drift_syst_pt,
                "shifted": r.shifted, "warnings": r.warnings,
            } for r in results],
        },
        "warnings": [w for r in (*runs, *results) for w in r.warnings],
        "pass": None,
    }
    path = os.path.join(args.outdir, "cal_flip_summary.json")
    with open(path, "w", newline="\n") as fh:
        json.dump(summary, fh, indent=1)
    print(f"[out ] {path}")

    if not args.no_figures:
        traces = {}
        for r, meta, session, fs in zip(runs, metas, sessions, fss):
            traces[r.name] = {
                axis: lowpass_trace(r.path, session, axis, meta, args, fs,
                                    r.mid_epoch_s, r.trim_s)
                for axis in axes}
        make_figure(args.outdir, traces, runs, results, flipped_axes)
        print(f"[out ] {os.path.join(args.outdir, 'cal_flip_timeseries.png')}")
    return summary


def ledger_append(record, path):
    os.makedirs(os.path.dirname(path), exist_ok=True)
    record = {"date": datetime.now().isoformat(timespec="seconds"), **record}
    line = json.dumps(record, ensure_ascii=False)
    with open(path, "a", newline="\n") as fh:
        fh.write(line + "\n")
    print(f"[ledger] {line}")
    return record


# ---------------------------------------------------------------- self-test

def synth_field(fs, seconds, level_pt, t_start_s, drift_pt_min, white_ft,
                transient=None, rng=None):
    """One axis of one run: a constant level + ambient drift continuous in
    absolute time + white noise (+ an optional settling exponential)."""
    rng = rng if rng is not None else np.random.default_rng(SIM_SEED)
    n = int(fs * seconds)
    t = np.arange(n) / fs
    x = level_pt + drift_pt_min * (t_start_s + t) / 60.0 \
        + rng.normal(0.0, white_ft * 1e-3 * np.sqrt(fs / 2.0), n)
    if transient:
        A, tau = transient
        x = x + A * np.exp(-t / tau)
    return x


def write_flip_run(tmp, session, axes_pt, fs, start_iso,
                   gain_mode="1x", notes="flip sequence"):
    """pT files written as the acquisition would with the NOMINAL 2.7 gain
    (values scaled by 2.624/2.7), so the epoch correction is exercised."""
    run = os.path.join(tmp, session)
    os.makedirs(run)
    n = len(next(iter(axes_pt.values())))
    for axis, x in axes_pt.items():
        head = (f"# QZFM shield acquisition | axis={axis.upper()} "
                f"session={session}\n"
                f"# start={start_iso} fs={fs:g} Hz gain={gain_mode} "
                f"cal={1000.0 / 2.7:.1f} pT/V vrange=1.0 V\n"
                f"# device=Dev2 ch=Dev2/ai0 sensor=SIM source=simulate\n"
                f"# one value per line, unit=pT")
        np.savetxt(os.path.join(run, f"{session}_{axis.upper()}_pT.txt"),
                   x * (GAIN_MEASURED / 2.7), fmt="%.3f", header=head,
                   comments="")
    meta = {
        "start_iso": start_iso, "session": session, "axes": list(axes_pt),
        "channel_map": {a: f"Dev2/ai{i}" for i, a in enumerate(axes_pt)},
        "aux_map": {}, "fs_hz": fs, "n_samples": n, "duration_s": n / fs,
        "clock_elapsed_s": n / fs, "fs_effective_hz": fs,
        "gain_mode": gain_mode, "cal_v_per_nT_used": 2.7,
        "cal_pT_per_V": 1000.0 / 2.7, "vrange": 1.0, "device": "Dev2",
        "sensor_id": "SIM",
        "rail_fraction": {a: 0.0 for a in axes_pt},
        "qzfm_rail_fraction": {a: 0.0 for a in axes_pt},
        "n_nonfinite": {a: 0 for a in axes_pt},
        "flat_ptp_V": {a: 0.02 for a in axes_pt},
        "mux_skew_note": "simulated", "source": "simulate",
        "software": "qzfm_record.py", "software_legacy": "qzfm_analog.py",
        "topology": "1head", "status": "ok", "exit_code": 0,
        "error": None, "warnings": [], "notes": notes,
    }
    with open(os.path.join(run, f"{session}.meta.json"), "w", newline="\n") as fh:
        json.dump(meta, fh, indent=1)
    return run


TRUTH = {"b_lab": {"z": -350.0, "y": -40.0},
         "zero": {"z": 1800.0, "y": -1200.0}}


def synth_sequence(tmp, sub, rng, zero_z=None, x_shift=0.0, b180_kw=None,
                   b180_gain_mode="1x", notes="flip sequence"):
    """A0 / B180 / A0' at 16-minute spacing, drift +0.5 pT/min throughout."""
    base = os.path.join(tmp, sub)
    os.makedirs(base)
    fs, dur, gap = 300.0, 700.0, 16 * 60.0
    zero = dict(TRUTH["zero"])
    if zero_z is not None:
        zero["z"] = zero_z
    runs = []
    for i, (name, sign) in enumerate((("a0", +1), ("b180", -1), ("a0p", +1))):
        t0 = i * gap
        axes = {}
        for axis in ("z", "y"):
            level = zero[axis] + sign * TRUTH["b_lab"][axis]
            kw = dict(b180_kw or {}) if name == "b180" else {}
            axes[axis] = synth_field(fs, dur, level, t0, 0.5, 21.0,
                                     rng=rng, **kw)
        if x_shift:
            level = 500.0 + (x_shift if name != "a0" else 0.0)
            axes["x"] = synth_field(fs, dur, level, t0, 0.5, 21.0, rng=rng)
        start = datetime.fromtimestamp(
            datetime.fromisoformat("2026-07-08T10:00:00").timestamp() + t0)
        runs.append(write_flip_run(
            base, name, axes, fs, start.isoformat(timespec="seconds"),
            gain_mode=(b180_gain_mode if name == "b180" else "1x"),
            notes=notes))
    return runs


def targs(tmp, runs, extra=()):
    argv = [*runs, "--flipped", "z,y", "--allow-sim", "--no-figures",
            "--outdir", os.path.join(tmp, "out"),
            "--ledger", os.path.join(tmp, "ledger.jsonl"), *extra]
    return build_parser().parse_args(argv)


def test_aba_recovery(ctx):
    runs = synth_sequence(ctx["tmp"], "t1", np.random.default_rng(SIM_SEED))
    s = run_analysis(targs(ctx["tmp"], runs))
    z = [a for a in s["results"]["axes"] if a["axis"] == "z"][0]
    tol = 3.0 * (z["b_lab_stat"] + z["b_lab_syst"])
    assert abs(z["b_lab_pt"] - TRUTH["b_lab"]["z"]) <= tol, \
        f"ABA: {z['b_lab_pt']:.1f} vs {TRUTH['b_lab']['z']} (tol {tol:.1f})"
    ctx["aba"] = s

    s2 = run_analysis(targs(ctx["tmp"], runs[:2]))
    z2 = [a for a in s2["results"]["axes"] if a["axis"] == "z"][0]
    assert 2.0 <= z2["drift_syst_pt"] <= 6.0, \
        f"two-run drift systematic {z2['drift_syst_pt']:.2f}, expected ~4 pT"


def test_epoch_correction(ctx):
    for r in ctx["aba"]["results"]["runs"]:
        assert abs(r["pt_correction"] - 2.7 / GAIN_MEASURED) < 1e-6, r


def test_consistency(ctx):
    runs = synth_sequence(ctx["tmp"], "t3",
                          np.random.default_rng(SIM_SEED + 1), x_shift=30.0)
    s = run_analysis(targs(ctx["tmp"], runs))
    x = [a for a in s["results"]["axes"] if a["axis"] == "x"][0]
    assert x["shifted"] is True, "30 pT setup shift on x not flagged"


def test_refusals(ctx):
    def expect13(runs, extra=()):
        try:
            run_analysis(targs(ctx["tmp"], runs, extra))
        except SystemExit as e:
            assert e.code == 13, f"exited {e.code}, want 13"
        else:
            raise AssertionError("must be refused (13)")

    rng = np.random.default_rng(SIM_SEED + 2)
    expect13(synth_sequence(ctx["tmp"], "t4a", rng, b180_gain_mode="3x"))
    runs = synth_sequence(ctx["tmp"], "t4b", rng)
    try:
        argv = [*runs, "--flipped", "z,x", "--allow-sim", "--no-figures",
                "--outdir", os.path.join(ctx["tmp"], "out"),
                "--ledger", os.path.join(ctx["tmp"], "ledger.jsonl")]
        run_analysis(build_parser().parse_args(argv))
    except SystemExit as e:
        assert e.code == 13, f"absent flipped axis exited {e.code}, want 13"
    else:
        raise AssertionError("--flipped with an absent axis must be refused")
    expect13(synth_sequence(ctx["tmp"], "t4c", rng, zero_z=80000.0))


def test_settling(ctx):
    rng = np.random.default_rng(SIM_SEED + 3)
    # a 600 s B180 (short trim) with a live 44 pT / 185 s transient
    base = os.path.join(ctx["tmp"], "t5")
    os.makedirs(base)
    fs, gap = 300.0, 16 * 60.0
    runs = []
    for i, (name, sign, dur, tr) in enumerate(
            (("a0", +1, 700.0, None), ("b180", -1, 600.0, (44.0, 185.0)),
             ("a0p", +1, 700.0, None))):
        axes = {}
        for axis in ("z", "y"):
            level = TRUTH["zero"][axis] + sign * TRUTH["b_lab"][axis]
            axes[axis] = synth_field(fs, dur, level, i * gap, 0.5, 21.0,
                                     transient=tr, rng=rng)
        start = datetime.fromtimestamp(
            datetime.fromisoformat("2026-07-08T12:00:00").timestamp() + i * gap)
        runs.append(write_flip_run(base, name, axes, fs,
                                   start.isoformat(timespec="seconds")))
    s = run_analysis(targs(ctx["tmp"], runs))
    warns = " ".join(s["warnings"])
    assert "settling not complete" in warns, warns
    z = [a for a in s["results"]["axes"] if a["axis"] == "z"][0]
    assert z["b_lab_syst"] > 10.0, \
        f"settling residual not in the systematic: {z['b_lab_syst']:.1f}"


def test_rezero_guard(ctx):
    runs = synth_sequence(ctx["tmp"], "t6",
                          np.random.default_rng(SIM_SEED + 4),
                          notes="re-zeroed head before this run")
    s = run_analysis(targs(ctx["tmp"], runs))
    warns = " ".join(s["warnings"])
    assert "INVALID" in warns, "re-zero note not flagged loudly"


def run_self_test():
    import tempfile
    tmp = tempfile.mkdtemp(prefix="cal_flip_test_")
    print("=== cal_flip --test: synthetic checks (temp dir, removed after) ===")
    ctx = {"tmp": tmp}
    try:
        for test in (test_aba_recovery, test_epoch_correction,
                     test_consistency, test_refusals, test_settling,
                     test_rezero_guard):
            test(ctx)
            print(f"[test] PASS: {test.__name__}")
        return 0
    except AssertionError as e:
        print(f"[test] FAIL: {e}", file=sys.stderr)
        return 1
    finally:
        shutil.rmtree(tmp, ignore_errors=True)


# ---------------------------------------------------------------- CLI

def build_parser():
    p = argparse.ArgumentParser(
        description=__doc__,
        formatter_class=argparse.RawDescriptionHelpFormatter)
    p.add_argument("runs", nargs="*", metavar="RUN",
                   help="run dirs in order: A0 B180 [A0']")
    p.add_argument("--flipped", default=None,
                   help="comma list of axes whose lab projection reversed "
                        "(the tool does not guess the geometry)")
    p.add_argument("--position", default="?",
                   help="position label for the report and ledger")
    p.add_argument("--raw", action="store_true",
                   help="analyze raw volts instead of the pT files")
    p.add_argument("--gain-v-per-nt", type=float, default=GAIN_MEASURED,
                   help="true gain for --raw and the epoch correction")
    p.add_argument("--gradient-pt-per-m", type=float, default=60.0,
                   help="|G|max for the cell-displacement systematic")
    p.add_argument("--cell-offset-mm", type=float, default=0.0,
                   help="cell displacement if the rotation axis missed it")
    p.add_argument("--outdir", default=os.path.dirname(os.path.abspath(__file__)),
                   help="where the summary and figure go")
    p.add_argument("--ledger", default=DEFAULT_LEDGER,
                   help="calibration ledger path")
    p.add_argument("--force", action="store_true",
                   help="analyze runs whose status is not ok (rails: never)")
    p.add_argument("--allow-sim", action="store_true",
                   help="accept runs recorded with --simulate")
    p.add_argument("--no-figures", action="store_true",
                   help="skip the figure (headless quick check)")
    p.add_argument("--test", action="store_true",
                   help="offline self-test on synthetic runs")
    return p


def main(argv=None):
    args = build_parser().parse_args(argv)
    if args.test:
        return run_self_test()
    require(args.runs, "no run directories given (see --help)")
    require(args.flipped, "--flipped is required (the tool does not guess "
                          "which axes reversed)")
    require(1.5 <= args.gain_v_per_nt <= 4.0,
            f"--gain-v-per-nt {args.gain_v_per_nt} implausible at 1x")
    require(args.gradient_pt_per_m > 0, "--gradient-pt-per-m must be positive")
    require(args.cell_offset_mm >= 0, "--cell-offset-mm cannot be negative")
    run_analysis(args)
    return 0


if __name__ == "__main__":
    sys.exit(main())
