#!/usr/bin/env python3
"""Compare N runs of the same setup: noise floor, low-frequency ASD,
DC and drift, with honest uncertainties.

Standing questions this answers: is the white floor repeatable across
re-zeroed runs (B4a), what did a degauss change (B3), and any labelled
A/B comparison (single vs dual axis, gain modes, warm vs cold). It is
descriptive by design -- it quantifies differences, it does not explain
them. Accepts analog run dirs and GUI *_Formatted.txt records; when the
readout paths are mixed, every ratio is printed on an as-is AND a
sensor-only basis (the analog chain adds ~6.8 fT/rtHz in quadrature).
Offline check: python runs_compare.py --test
"""

from __future__ import annotations

import argparse
import csv
import glob
import json
import os
import shutil
import sys
from dataclasses import dataclass, asdict, field
from datetime import datetime

import numpy as np
from scipy.signal import welch
from scipy.optimize import curve_fit

GAIN_MEASURED = 2.624   # V/nT at 1x (sync5, 50 Hz two-readout method)
FS_GUI = 203.18         # GUI stream true rate (label says 200)
SETTLE_S = 500.0        # wait after Field Zero (tau~185 s Z transient)
CHAIN_FLOOR_FT = 6.8    # analog DAQ chain, fT/rtHz-equiv (quadrature
                        # inference from sync5; cal_daq_floor measures it)
KNOWN_LINES_HZ = (50.0, 59.917, 100.0, 150.0, 203.18, 277.1, 294.0)
LOWF_HZ = (0.01, 0.05, 0.1, 0.5, 1.0)
MIN_CYCLES = 10.0       # a low-f point needs this many cycles in the record
SIM_SEED = 20260707

REPO_ROOT = os.path.dirname(os.path.dirname(
    os.path.dirname(os.path.abspath(__file__))))

C_GROUPS = ("#2E6E8E", "#C0504D", "#5A7D3A", "#8064A2", "#B08C2E")
C_INK, C_MUTE = "#33373B", "#7A7F85"


def require(ok, msg, code=13):
    if not ok:
        print(f"[error] {msg}", file=sys.stderr)
        raise SystemExit(code)


# ---------------------------------------------------------------- spectra

import sys as _sys  # analysis_scripts/ on sys.path for _paths
_sys.path.insert(0, os.path.dirname(os.path.dirname(os.path.abspath(__file__))))
import _paths  # noqa: E402,F401  (puts B-field on sys.path)
from bfield.analysis.spectra import welch_asd  # noqa: E402 -- was an identical local copy


def n_averages(n_samples, nperseg):
    # scipy's default 50% overlap
    return max(1, 2 * n_samples // int(nperseg) - 1)


def spliced_asd(x, fs, lowseg=200.0, hiseg=20.0, split=1.0):
    """Long segments below `split` Hz for resolution, short above for
    averaging. lowseg/hiseg are segment lengths in seconds."""
    fl, al = welch_asd(x, fs, lowseg * fs)
    fh, ah = welch_asd(x, fs, hiseg * fs)
    ml = (fl > 0) & (fl < split)
    mh = fh >= split
    return np.concatenate([fl[ml], fh[mh]]), np.concatenate([al[ml], ah[mh]])


def band_floor(x, fs, lo=3.0, hi=40.0, seg_s=20.0):
    """Median ASD over [lo, hi] Hz with known lines excluded -- THE
    white-floor metric."""
    f, a = welch_asd(x, fs, seg_s * fs)
    m = (f >= lo) & (f <= hi)
    for fline in KNOWN_LINES_HZ:
        m &= np.abs(f - fline) > 0.5
    return float(np.median(a[m]))


def block_floor_scatter(x, fs, lo, hi, win_s=90.0, hop_s=30.0):
    """(median floor, SEM, floors, block centers in min). Overlapping
    windows are correlated, so the SEM divides by the non-overlapping
    window count. Records shorter than two windows fall back to a single
    whole-record estimate with a Welch-statistics error."""
    n, h = int(win_s * fs), int(hop_s * fs)
    if x.size < 2 * n:
        floor = band_floor(x, fs, lo, hi)
        k = n_averages(x.size, 20.0 * fs)
        return floor, floor / (2.0 * np.sqrt(k)), np.array([]), np.array([])
    floors, mids = [], []
    for i in range(0, x.size - n + 1, h):
        floors.append(band_floor(x[i:i + n], fs, lo, hi))
        mids.append((i + n / 2) / fs / 60.0)
    floors = np.asarray(floors)
    n_eff = max(1.0, (x.size / fs) / win_s)
    return (float(np.median(floors)),
            float(floors.std(ddof=1) / np.sqrt(n_eff)),
            floors, np.asarray(mids))


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


def asd_point(f, a, f0, n_avg):
    """Median ASD in a +/-25% window around f0, with an approximate
    Welch-statistics error (per-bin ASD scatter 1/(2 sqrt(k)), tightened
    by the roughly independent half of the bins in the window)."""
    m = (f >= f0 / 1.25) & (f <= f0 * 1.25) & (f > 0)
    if not m.any():
        return None, None
    val = float(np.median(a[m]))
    n_indep = max(1.0, m.sum() / 2.0)
    rel = 1.0 / (2.0 * np.sqrt(max(n_avg, 1))) / np.sqrt(n_indep)
    return val, val * rel


def fit_onef(x, fs):
    """White + flicker model asd(f) = sqrt(w^2 + (A/f^alpha)^2) fitted in
    log space over 0.005-40 Hz. Returns (w, A, alpha) or None."""
    f, a = spliced_asd(x, fs)
    m = (f >= 0.005) & (f <= 40.0) & (a > 0)
    for fline in KNOWN_LINES_HZ:
        m &= np.abs(f - fline) > 0.5
    if m.sum() < 20:
        return None
    logf, loga = np.log(f[m]), np.log(a[m])
    w0 = band_floor(x, fs)

    def logmodel(lf, w, A, alpha):
        ff = np.exp(lf)
        return 0.5 * np.log(w ** 2 + (A / ff ** alpha) ** 2)

    try:
        popt, _ = curve_fit(logmodel, logf, loga,
                            p0=[max(w0, 1e-6), max(a[m][0] * f[m][0], 1e-9), 0.9],
                            bounds=([1e-9, 1e-12, 0.1], [1e3, 1e4, 3.0]),
                            maxfev=40000)
    except Exception:
        return None
    return tuple(float(v) for v in popt)


# ---------------------------------------------------------------- loading

def find_meta(run_dir):
    metas = glob.glob(os.path.join(run_dir, "*.meta.json"))
    require(len(metas) == 1, f"{run_dir}: expected one meta.json, found {len(metas)}")
    with open(metas[0], encoding="utf-8") as fh:
        return json.load(fh), os.path.basename(metas[0])[:-len(".meta.json")]


def check_run(meta, path, force=False, allow_sim=False):
    if not (meta.get("status") == "ok" and meta.get("exit_code") == 0):
        require(force, f"{path}: status={meta.get('status')} exit_code="
                       f"{meta.get('exit_code')}; --force to analyze anyway")
        print(f"[gate] {path}: overridden status={meta.get('status')}")
    nonfinite = meta.get("n_nonfinite") or {}
    require(all(v == 0 for v in nonfinite.values()),
            f"{path}: non-finite samples: {nonfinite}")
    for key in ("rail_fraction", "qzfm_rail_fraction"):
        rails = {k: v for k, v in (meta.get(key) or {}).items() if v > 0.001}
        if rails:
            require(force, f"{path}: {key} above 0.1%: {rails}; --force to keep")
            print(f"[gate] {path}: overridden {key}={rails}")
    if meta.get("source") == "simulate":
        require(allow_sim, f"{path}: run is simulated; pass --allow-sim if intended")
    fs, fse = meta.get("fs_hz"), meta.get("fs_effective_hz")
    if fs and fse and abs(fse / fs - 1.0) > 1e-3:
        print(f"[warn] {path}: fs_effective off nominal by "
              f"{abs(fse / fs - 1.0) * 100:.2f}%")


def load_axis_pt(run_dir, session, axis, meta, gain_true):
    """Per-axis pT series with the per-run calibration-epoch correction:
    pT_true = pT_file * cal_used / gain_true (1.029 for pre-07-07 runs)."""
    path = os.path.join(run_dir, f"{session}_{axis.upper()}_pT.txt")
    require(os.path.exists(path), f"missing {path}")
    x = np.loadtxt(path, comments="#")
    factor = float(meta["cal_v_per_nT_used"]) / gain_true
    if abs(factor - 1.0) > 1e-4:
        print(f"[cal ] {os.path.basename(path)}: epoch correction x{factor:.4f}")
    return x * factor


def load_axis_raw(run_dir, session, axis, gain_true):
    path = os.path.join(run_dir, f"{session}_raw.csv")
    require(os.path.exists(path), f"missing {path}")
    with open(path) as fh:
        fh.readline()
        names = fh.readline().strip().split(",")
    data = np.genfromtxt(path, delimiter=",", skip_header=2)
    col = f"V_{axis}"
    require(col in names, f"{path}: no column {col}")
    return data[:, names.index(col)] * 1000.0 / gain_true


def load_gui(path):
    x = np.loadtxt(path, skiprows=1)
    require(x.ndim == 1 and x.size > 0, f"{path}: not a single-column GUI record")
    return x


# ------------------------------------------------------------ characterize

@dataclass
class RunChar:
    label: str
    group: str
    pair_id: str
    path: str
    source: str            # analog / gui
    axis: str
    floor_ft: float
    floor_err_ft: float
    floor_sensor_only_ft: float
    floor_trend_pct_min: float | None
    lowf_ft: dict          # {"0.05": value, ...}
    lowf_err_ft: dict
    dc_pt: float
    dc_sem_pt: float
    drift_pt_min: float
    onef: dict | None      # {"w":, "A":, "alpha":}
    gain_mode: str | None
    vrange: float | None
    fs: float
    dur_s: float
    start_iso: str | None
    short: bool
    spectrum: tuple = field(default=None, repr=False)   # (f, asd) for the figure


def characterize(entry, axis, x, fs, meta, args):
    dur = x.size / fs
    short = dur < 700.0
    trim = 60.0 if short else SETTLE_S
    x = x[int(trim * fs):]
    usable = x.size / fs
    require(usable >= 30.0, f"{entry['path']}: only {usable:.0f} s left after "
                            f"trimming {trim:.0f} s -- too short to characterize")
    lo, hi = args.band
    require(hi <= fs / 2, f"{entry['path']}: --band top {hi:g} Hz above "
                          f"Nyquist {fs / 2:.1f} Hz")

    floor, floor_err, floors, mids = block_floor_scatter(x, fs, lo, hi)
    trend = None
    if floors.size >= 4:
        slope = np.polyfit(mids, floors, 1)[0]
        trend = float(slope / np.median(floors) * 100.0)

    # sensor-only basis: remove the analog chain in quadrature; GUI records
    # never went through the DAQ, so they stay as-is
    ft = floor * 1e3
    if entry["source"] == "analog":
        sensor_only = float(np.sqrt(max(ft ** 2 - CHAIN_FLOOR_FT ** 2, 0.0)))
        if ft < CHAIN_FLOOR_FT:
            print(f"[warn] {entry['label']}: floor below the chain model; "
                  "sensor-only clipped to 0")
    else:
        sensor_only = ft

    f_sp, a_sp = spliced_asd(x, fs)
    lowf, lowf_err = {}, {}
    for f0 in LOWF_HZ:
        if f0 < MIN_CYCLES / usable:
            continue
        k = n_averages(x.size, (200.0 if f0 < 1.0 else 20.0) * fs)
        val, err = asd_point(f_sp, a_sp, f0, k)
        if val is not None:
            lowf[f"{f0:g}"] = val * 1e3      # fT/rtHz
            lowf_err[f"{f0:g}"] = err * 1e3

    # short records still get a DC/drift number: shrink the block so at
    # least four fit (the error stays honest, just coarser)
    dc, dc_sem, drift, _, _ = block_stats(x, fs, block_s=min(60.0, usable / 4.0))
    onef = fit_onef(x, fs)

    return RunChar(
        label=entry["label"], group=entry["group"], pair_id=entry["pair_id"],
        path=entry["path"], source=entry["source"], axis=axis,
        floor_ft=ft, floor_err_ft=floor_err * 1e3,
        floor_sensor_only_ft=sensor_only, floor_trend_pct_min=trend,
        lowf_ft=lowf, lowf_err_ft=lowf_err,
        dc_pt=dc, dc_sem_pt=dc_sem, drift_pt_min=drift,
        onef=None if onef is None else
             {"w": onef[0], "A": onef[1], "alpha": onef[2]},
        gain_mode=(meta or {}).get("gain_mode"),
        vrange=(meta or {}).get("vrange"),
        fs=fs, dur_s=dur, start_iso=(meta or {}).get("start_iso"),
        short=short, spectrum=(f_sp, a_sp),
    )


# ---------------------------------------------------------------- stats

def ordered_groups(chars):
    # keep the order in which groups first appear; the first group is the
    # comparison reference (denominator of every ratio)
    seen = []
    for c in chars:
        if c.group not in seen:
            seen.append(c.group)
    return seen


def group_stats(chars):
    out = []
    keys = [(g, a) for g in ordered_groups(chars)
            for a in sorted({c.axis for c in chars if c.group == g})]
    for group, axis in keys:
        sel = [c for c in chars if c.group == group and c.axis == axis]
        floors = np.array([c.floor_ft for c in sel])
        rel_std = (float(floors.std(ddof=1) / floors.mean())
                   if len(sel) >= 2 else None)
        out.append({"group": group, "axis": axis, "n": len(sel),
                    "floor_mean": float(floors.mean()),
                    "floor_rel_std": rel_std,
                    "small_sample": len(sel) < 5})
    return out


def pair_metrics(a, b):
    """Comparable metrics of a before/after pair: (name, v1, e1, v2, e2)."""
    rows = [("floor", a.floor_ft, a.floor_err_ft, b.floor_ft, b.floor_err_ft),
            ("dc_pt", a.dc_pt, a.dc_sem_pt, b.dc_pt, b.dc_sem_pt)]
    for f0 in sorted(set(a.lowf_ft) & set(b.lowf_ft), key=float):
        rows.append((f"lowf_{f0}", a.lowf_ft[f0], a.lowf_err_ft[f0],
                     b.lowf_ft[f0], b.lowf_err_ft[f0]))
    return rows


def pair_stats(chars):
    out = []
    ids = sorted({c.pair_id for c in chars if c.pair_id})
    for pid in ids:
        sel = sorted([c for c in chars if c.pair_id == pid],
                     key=lambda c: c.start_iso or c.label)
        for axis in sorted({c.axis for c in sel}):
            two = [c for c in sel if c.axis == axis]
            if len(two) != 2:
                print(f"[warn] pair {pid} axis {axis}: {len(two)} runs, need 2")
                continue
            before, after = two
            settling = None
            if before.start_iso and after.start_iso:
                t0 = datetime.fromisoformat(before.start_iso)
                t1 = datetime.fromisoformat(after.start_iso)
                gap_min = (t1 - t0).total_seconds() / 60.0 - before.dur_s / 60.0
                settling = gap_min < 30.0
            for name, v1, e1, v2, e2 in pair_metrics(before, after):
                err = np.hypot(e1, e2)
                z = float(abs(v2 - v1) / err) if err > 0 else None
                if z is None or z < 3.0:
                    verdict = "no significant change"
                else:
                    verdict = "reduced" if v2 < v1 else "increased"
                out.append({"pair_id": pid, "axis": axis, "metric": name,
                            "before": v1, "after": v2,
                            "ratio": float(v2 / v1) if v1 else None,
                            "z": z, "verdict": verdict,
                            "settling": settling})
    return out


def cross_group(chars, groups):
    out = []
    names = ordered_groups(chars)
    if len(names) != 2:
        return out
    ga, gb = names
    for axis in sorted({c.axis for c in chars}):
        a = [c for c in chars if c.group == ga and c.axis == axis]
        b = [c for c in chars if c.group == gb and c.axis == axis]
        if not a or not b:
            continue
        for basis, get in (("as-is", lambda c: c.floor_ft),
                           ("sensor-only", lambda c: c.floor_sensor_only_ft)):
            fa, fb = np.array([get(c) for c in a]), np.array([get(c) for c in b])
            ratio = float(fb.mean() / fa.mean())
            inferential = len(a) >= 3 and len(b) >= 3
            p = None
            if inferential:
                from scipy.stats import ttest_ind
                p = float(ttest_ind(fa, fb, equal_var=False).pvalue)
            # with tiny n, propagate the per-run errors instead
            ea = np.sqrt(np.sum([c.floor_err_ft ** 2 for c in a])) / len(a)
            eb = np.sqrt(np.sum([c.floor_err_ft ** 2 for c in b])) / len(b)
            z = float(abs(fb.mean() - fa.mean()) / np.hypot(ea, eb))
            out.append({"a": ga, "b": gb, "axis": axis, "basis": basis,
                        "floor_ratio": ratio, "z": z, "p_welch": p,
                        "inferential": inferential})
        if ({c.source for c in a} == {"gui"}) and ({c.source for c in b} == {"gui"}):
            out[-2]["note"] = "both groups GUI: as-is is already sensor-only"
    return out


def check_group_metadata(chars, force):
    # a floor comparison inside one group only means something at one
    # gain_mode and one vrange
    for group in sorted({c.group for c in chars}):
        sel = [c for c in chars if c.group == group and c.source == "analog"]
        for attr in ("gain_mode", "vrange"):
            vals = {getattr(c, attr) for c in sel} - {None}
            if len(vals) > 1:
                print(f"[note] group {group!r} mixes {attr} = {sorted(map(str, vals))}")
                require(force, f"mixed {attr} inside group {group!r}; "
                               "--force to compare anyway")
                print(f"[gate] overridden: mixed {attr} in group {group!r}")


# ---------------------------------------------------------------- report

def report_runs(chars):
    for c in chars:
        src = ("GUI record (no meta)" if c.source == "gui"
               else f"analog {c.gain_mode or '?'}")
        sens = ("" if c.source == "gui"
                else f" (sensor-only {c.floor_sensor_only_ft:.1f})")
        trend = ("" if c.floor_trend_pct_min is None
                 else f"  trend {c.floor_trend_pct_min:+.2f}%/min")
        tags = " [short]" if c.short else ""
        print(f"[run] {c.label} axis {c.axis.upper()}  {src}  floor "
              f"{c.floor_ft:.1f} +/- {c.floor_err_ft:.1f} fT/rtHz{sens}{trend}{tags}")
        if c.lowf_ft:
            pts = "  ".join(f"{k} Hz {v:.0f}" for k, v in
                            sorted(c.lowf_ft.items(), key=lambda i: float(i[0])))
            print(f"      low-f [fT/rtHz]: {pts}")
        print(f"      DC {c.dc_pt:+.1f} +/- {c.dc_sem_pt:.1f} pT  drift "
              f"{c.drift_pt_min:+.2f} pT/min" +
              ("" if not c.onef else
               f"  1/f fit w={c.onef['w'] * 1e3:.1f} fT A={c.onef['A'] * 1e3:.0f} "
               f"alpha={c.onef['alpha']:.2f}"))


def report_groups(groups):
    for g in groups:
        rel = ("n/a" if g["floor_rel_std"] is None
               else f"{g['floor_rel_std'] * 100:.1f}%")
        tag = "  (small sample)" if g["small_sample"] else ""
        print(f"[grp] {g['group']} axis {g['axis'].upper()}: n={g['n']}  "
              f"floor mean {g['floor_mean']:.1f} fT/rtHz  rel std {rel}{tag}")
        if g["n"] < 3:
            print("      -> descriptive only (need >=3 runs for a repeatability bar)")


def report_pairs(pairs):
    for p in pairs:
        tag = "  (settling?)" if p["settling"] else ""
        z = "n/a" if p["z"] is None else f"{p['z']:.1f}"
        print(f"[pair] {p['pair_id']} axis {p['axis'].upper()} {p['metric']}: "
              f"{p['before']:.4g} -> {p['after']:.4g}  ratio {p['ratio']:.2f}  "
              f"z={z}  {p['verdict']}{tag}")


def report_cross(cross):
    for c in cross:
        extra = "" if c["inferential"] else "  (descriptive only)"
        print(f"[cmp] {c['a']} vs {c['b']} axis {c['axis'].upper()}: floor ratio "
              f"{c['floor_ratio']:.2f} ({c['basis']})  z={c['z']:.1f}{extra}")


# ---------------------------------------------------------------- figures

def fig_style(plt):
    plt.rcParams.update({
        "figure.dpi": 150, "font.size": 11, "axes.titleweight": "bold",
        "axes.spines.top": False, "axes.spines.right": False,
        "axes.edgecolor": C_MUTE, "text.color": C_INK, "legend.frameon": False,
    })


def group_color(chars):
    order = sorted({c.group for c in chars})
    return {g: C_GROUPS[i % len(C_GROUPS)] for i, g in enumerate(order)}


def make_figures(outdir, chars, pairs, band):
    import matplotlib
    matplotlib.use("Agg")
    import matplotlib.pyplot as plt
    fig_style(plt)
    colors = group_color(chars)

    # ASD overlay -- never an averaged group ASD; averaging would hide the
    # run-to-run variance this tool exists to measure
    fig, ax = plt.subplots(figsize=(7.5, 4.5))
    for c in chars:
        f, a = c.spectrum
        m = f > 0
        ax.loglog(f[m], a[m] * 1e3, lw=0.8, color=colors[c.group],
                  label=f"{c.label} ({c.axis.upper()})")
    ax.axvspan(band[0], band[1], color="#E6E8EA", alpha=0.5, zorder=0)
    ax.set_xlabel("frequency [Hz]")
    ax.set_ylabel("ASD [fT/rtHz]")
    ax.set_title("run comparison")
    ax.legend(fontsize=9)
    fig.savefig(os.path.join(outdir, "runs_compare_asd.png"), bbox_inches="tight")
    plt.close(fig)

    # floors with error bars, grouped
    fig, ax = plt.subplots(figsize=(7.0, 4.0))
    for i, c in enumerate(chars):
        ax.errorbar(i, c.floor_ft, yerr=c.floor_err_ft, fmt="o",
                    color=colors[c.group], capsize=3)
        if c.source == "analog":
            ax.plot(i, c.floor_sensor_only_ft, "x", color=colors[c.group])
    for g, col in colors.items():
        sel = [c.floor_ft for c in chars if c.group == g]
        if len(sel) >= 2:
            ax.axhspan(np.mean(sel) - np.std(sel), np.mean(sel) + np.std(sel),
                       color=col, alpha=0.12, zorder=0)
    ax.set_xticks(range(len(chars)))
    ax.set_xticklabels([f"{c.label}\n{c.axis.upper()}" for c in chars], fontsize=8)
    ax.set_ylabel("floor [fT/rtHz]  (x = sensor-only)")
    ax.set_title("white-noise floor by run")
    fig.savefig(os.path.join(outdir, "runs_compare_floors.png"), bbox_inches="tight")
    plt.close(fig)

    if pairs:
        metrics = sorted({p["metric"] for p in pairs})
        fig, axes = plt.subplots(1, len(metrics),
                                 figsize=(2.6 * len(metrics) + 1, 3.6),
                                 squeeze=False)
        for ax, metric in zip(axes[0], metrics):
            for p in [q for q in pairs if q["metric"] == metric]:
                ax.plot([0, 1], [p["before"], p["after"]], "-o", lw=1.2,
                        label=f"{p['pair_id']} {p['axis'].upper()}")
            ax.set_xticks([0, 1])
            ax.set_xticklabels(["before", "after"])
            ax.set_title(metric, fontsize=10)
        axes[0][0].legend(fontsize=8)
        fig.savefig(os.path.join(outdir, "runs_compare_pairs.png"),
                    bbox_inches="tight")
        plt.close(fig)


# ---------------------------------------------------------------- pipeline

def collect_entries(args):
    entries = []
    if args.manifest:
        with open(args.manifest, newline="") as fh:
            for row in csv.DictReader(fh):
                entries.append({"path": row["run_dir"].strip(),
                                "label": (row.get("label") or "").strip()
                                         or os.path.basename(row["run_dir"].strip()),
                                "group": (row.get("group") or "").strip(),
                                "pair_id": (row.get("pair_id") or "").strip()})
    labels = args.labels or []
    require(not labels or len(labels) == len(args.runs),
            f"--labels has {len(labels)} entries for {len(args.runs)} runs")
    for i, run in enumerate(args.runs):
        label = labels[i] if labels else os.path.basename(run.rstrip("/"))
        entries.append({"path": run, "label": label, "group": "", "pair_id": ""})
    for path in args.gui or []:
        entries.append({"path": path,
                        "label": os.path.splitext(os.path.basename(path))[0],
                        "group": "", "pair_id": "", "source": "gui"})
    for e in entries:
        e["group"] = e["group"] or e["label"]
        e.setdefault("source",
                     "gui" if e["path"].endswith("_Formatted.txt") else "analog")
    require(entries, "no runs given (positional dirs, --gui or --manifest)")
    return entries


def run_analysis(args):
    entries = collect_entries(args)
    chars = []
    for entry in entries:
        if entry["source"] == "gui":
            print(f"[run ] {entry['path']}: GUI record, no meta -- "
                  f"gate skipped, fs={FS_GUI}, axis assumed z")
            x = load_gui(entry["path"])
            chars.append(characterize(entry, "z", x, FS_GUI, None, args))
            continue
        meta, session = find_meta(entry["path"])
        check_run(meta, entry["path"], args.force, args.allow_sim)
        axes = [a for a in meta["axes"] if not args.axes or a in args.axes]
        require(axes, f"{entry['path']}: no requested axis in {meta['axes']}")
        for axis in axes:
            if args.raw:
                x = load_axis_raw(entry["path"], session, axis, args.gain_v_per_nt)
            else:
                x = load_axis_pt(entry["path"], session, axis, meta,
                                 args.gain_v_per_nt)
            chars.append(characterize(entry, axis, x, float(meta["fs_hz"]),
                                      meta, args))

    check_group_metadata(chars, args.force)
    groups = group_stats(chars)
    pairs = pair_stats(chars)
    cross = cross_group(chars, groups)

    print(f"=== runs_compare: {len(chars)} run-axis records, "
          f"{len(groups)} group(s) ===")
    report_runs(chars)
    report_groups(groups)
    if pairs:
        report_pairs(pairs)
    if cross:
        report_cross(cross)
    sources = {c.source for c in chars}
    if len(sources) > 1:
        print("[note] mixed readout paths (GUI vs analog): "
              "the sensor-only basis is the comparable one")

    os.makedirs(args.outdir, exist_ok=True)
    summary = {
        "program": "runs_compare.py",
        "generated": datetime.now().isoformat(timespec="seconds"),
        "inputs": {"runs": [e["path"] for e in entries],
                   "args": {k: v for k, v in vars(args).items()
                            if k not in ("runs", "test")}},
        "results": {
            "runs": [{k: v for k, v in asdict(c).items() if k != "spectrum"}
                     for c in chars],
            "groups": groups, "pairs": pairs, "cross_group": cross,
        },
        "warnings": [],
        "pass": None,      # descriptive tool: no acceptance criteria
    }
    path = os.path.join(args.outdir, "runs_compare_summary.json")
    with open(path, "w", newline="\n") as fh:
        json.dump(summary, fh, indent=1)
    print(f"[out ] {path}")

    if not args.no_figures:
        make_figures(args.outdir, chars, pairs, args.band)
        print(f"[out ] figures in {args.outdir}")
    return summary


# ---------------------------------------------------------------- self-test

def synth_field_pt(fs, seconds, white_ft=21.0, flick_ft=0.0, alpha=0.9,
                   lines=((50.0, 0.14),), drift_pt_min=0.0, dc_pt=0.0,
                   rng=None):
    """Synthetic field in pT: white floor [fT/rtHz], flicker A/f^alpha
    (A in fT/rtHz at 1 Hz), lines [(Hz, pT amplitude)], linear drift."""
    rng = rng if rng is not None else np.random.default_rng(SIM_SEED)
    n = int(fs * seconds)
    t = np.arange(n) / fs
    x = rng.normal(0.0, white_ft * 1e-3 * np.sqrt(fs / 2.0), n)
    if flick_ft > 0:
        wn = rng.normal(0.0, 1.0, n)
        F = np.fft.rfft(wn)
        f = np.fft.rfftfreq(n, 1.0 / fs)
        shape = np.zeros_like(f)
        shape[1:] = (flick_ft * 1e-3) / f[1:] ** alpha
        x += np.fft.irfft(F * shape * np.sqrt(fs / 2.0), n)
    for fl, al in lines:
        x += al * np.sin(2 * np.pi * fl * t + rng.uniform(0, 2 * np.pi))
    return dc_pt + x + drift_pt_min * t / 60.0


def write_analog_run(tmp, session, axes_pt, fs, gain_mode="1x", vrange=1.0,
                     start_iso="2026-07-08T09:00:00"):
    run = os.path.join(tmp, session)
    os.makedirs(run)
    n = len(next(iter(axes_pt.values())))
    for axis, x in axes_pt.items():
        head = (f"# QZFM shield acquisition | axis={axis.upper()} session={session}\n"
                f"# start={start_iso} fs={fs:g} Hz gain={gain_mode} "
                f"cal={1000.0 / GAIN_MEASURED:.1f} pT/V vrange={vrange:g} V\n"
                f"# device=Dev2 ch=Dev2/ai0 sensor=SIM source=simulate\n"
                f"# one value per line, unit=pT")
        np.savetxt(os.path.join(run, f"{session}_{axis.upper()}_pT.txt"),
                   x, fmt="%.2f", header=head, comments="")
    meta = {
        "start_iso": start_iso, "session": session, "axes": list(axes_pt),
        "channel_map": {a: f"Dev2/ai{i}" for i, a in enumerate(axes_pt)},
        "aux_map": {}, "fs_hz": fs, "n_samples": n, "duration_s": n / fs,
        "clock_elapsed_s": n / fs, "fs_effective_hz": fs,
        "gain_mode": gain_mode, "cal_v_per_nT_used": GAIN_MEASURED,
        "cal_pT_per_V": 1000.0 / GAIN_MEASURED, "vrange": vrange,
        "device": "Dev2", "sensor_id": "SIM",
        "rail_fraction": {a: 0.0 for a in axes_pt},
        "qzfm_rail_fraction": {a: 0.0 for a in axes_pt},
        "n_nonfinite": {a: 0 for a in axes_pt},
        "flat_ptp_V": {a: 0.02 for a in axes_pt},
        "mux_skew_note": "simulated", "source": "simulate",
        "software": "qzfm_record.py", "software_legacy": "qzfm_analog.py",
        "topology": "1head", "status": "ok", "exit_code": 0,
        "error": None, "warnings": [], "notes": "runs_compare --test",
    }
    with open(os.path.join(run, f"{session}.meta.json"), "w", newline="\n") as fh:
        json.dump(meta, fh, indent=1)
    return run


def write_gui_record(tmp, name, x):
    path = os.path.join(tmp, f"{name}_Formatted.txt")
    np.savetxt(path, x, fmt="%.2f", header="QZFM GUI formatted output [pT]",
               comments="# ")
    return path


def targs(tmp, extra):
    argv = [*extra, "--allow-sim", "--no-figures",
            "--outdir", os.path.join(tmp, "out")]
    return build_parser().parse_args(argv)


def test_no_false_alarm(ctx):
    tmp = os.path.join(ctx["tmp"], "t1")
    os.makedirs(tmp)
    rng = np.random.default_rng(SIM_SEED)
    rows = []
    for i in range(3):
        x = synth_field_pt(300.0, 900.0, white_ft=21.0, flick_ft=30.0, rng=rng)
        run = write_analog_run(tmp, f"r{i}", {"z": x}, 300.0)
        rows.append((run, f"r{i}", "rep", ""))
    s = run_analysis(targs(tmp, ["--manifest", write_manifest(tmp, rows)]))
    g = s["results"]["groups"][0]
    assert g["n"] == 3, g
    rel_err = np.mean([r["floor_err_ft"] / r["floor_ft"]
                       for r in s["results"]["runs"]])
    assert g["floor_rel_std"] <= 3.0 * rel_err, \
        f"repeat scatter {g['floor_rel_std']:.3f} vs per-run err {rel_err:.3f}"


def test_real_difference(ctx):
    tmp = os.path.join(ctx["tmp"], "t2")
    os.makedirs(tmp)
    rng = np.random.default_rng(SIM_SEED + 1)
    rows = []
    for grp, ft in (("pre", 12.0), ("post", 21.0)):
        for i in range(2):
            x = synth_field_pt(300.0, 900.0, white_ft=ft, rng=rng)
            run = write_analog_run(tmp, f"{grp}{i}", {"z": x}, 300.0)
            rows.append((run, f"{grp}{i}", grp, ""))
    man = write_manifest(tmp, rows)
    s = run_analysis(targs(tmp, ["--manifest", man]))
    cmp_asis = [c for c in s["results"]["cross_group"] if c["basis"] == "as-is"][0]
    assert abs(cmp_asis["floor_ratio"] - 1.75) < 0.15, cmp_asis
    assert cmp_asis["z"] > 5.0, f"z={cmp_asis['z']:.1f}, want > 5"

    # same group, mixed gain_mode -> refused
    x = synth_field_pt(300.0, 900.0, white_ft=12.0, rng=rng)
    run3 = write_analog_run(tmp, "pre_mix", {"z": x}, 300.0, gain_mode="3x")
    man2 = write_manifest(tmp, rows + [(run3, "pre_mix", "pre", "")], "m2.csv")
    try:
        run_analysis(targs(tmp, ["--manifest", man2]))
    except SystemExit as e:
        assert e.code == 13, f"mixed gain_mode exited {e.code}, want 13"
    else:
        raise AssertionError("mixed gain_mode inside a group must be refused")


def test_pair(ctx):
    tmp = os.path.join(ctx["tmp"], "t3")
    os.makedirs(tmp)
    rng = np.random.default_rng(SIM_SEED + 2)
    x1 = synth_field_pt(300.0, 2100.0, white_ft=21.0, flick_ft=40.0, rng=rng)
    x2 = synth_field_pt(300.0, 2100.0, white_ft=21.0, flick_ft=20.0, rng=rng)
    r1 = write_analog_run(tmp, "pre", {"z": x1}, 300.0,
                          start_iso="2026-07-08T09:00:00")
    r2 = write_analog_run(tmp, "post", {"z": x2}, 300.0,
                          start_iso="2026-07-08T11:00:00")
    man = write_manifest(tmp, [(r1, "pre", "pre", "d1"), (r2, "post", "post", "d1")])
    s = run_analysis(targs(tmp, ["--manifest", man]))
    lows = [p for p in s["results"]["pairs"] if p["metric"] == "lowf_0.05"]
    assert lows, "no 0.05 Hz pair metric"
    p = lows[0]
    assert p["verdict"] == "reduced" and p["z"] > 3.0, p


def test_gui_parsing(ctx):
    tmp = os.path.join(ctx["tmp"], "t4")
    os.makedirs(tmp)
    rng = np.random.default_rng(SIM_SEED + 3)
    xg = synth_field_pt(FS_GUI, 900.0, white_ft=12.0, rng=rng)
    xa = synth_field_pt(300.0, 900.0, white_ft=12.0, rng=rng)
    gui = write_gui_record(tmp, "gui_base", xg)
    ana = write_analog_run(tmp, "ana", {"z": xa}, 300.0)
    s = run_analysis(targs(tmp, [gui, ana, "--labels", "gui", "ana"]))
    runs = {r["label"]: r for r in s["results"]["runs"]}
    fg, fa = runs["gui"]["floor_ft"], runs["ana"]["floor_ft"]
    assert abs(fg - fa) / fa <= 0.05, f"GUI {fg:.2f} vs analog {fa:.2f}"
    assert runs["gui"]["source"] == "gui"


def test_path_correction(ctx):
    tmp = os.path.join(ctx["tmp"], "t5")
    os.makedirs(tmp)
    rng = np.random.default_rng(SIM_SEED + 4)
    sensor = 12.0
    as_measured = float(np.hypot(sensor, CHAIN_FLOOR_FT))   # analog sees more
    xa = synth_field_pt(300.0, 900.0, white_ft=as_measured, rng=rng)
    xg = synth_field_pt(FS_GUI, 900.0, white_ft=sensor, rng=rng)
    ana = write_analog_run(tmp, "ana", {"z": xa}, 300.0)
    gui = write_gui_record(tmp, "gui_ref", xg)
    s = run_analysis(targs(tmp, [ana, gui, "--labels", "ana", "gui"]))
    runs = {r["label"]: r for r in s["results"]["runs"]}
    a, g = runs["ana"], runs["gui"]
    assert a["floor_ft"] > g["floor_ft"] * 1.05, "as-is floors should differ"
    assert abs(a["floor_sensor_only_ft"] - g["floor_ft"]) / g["floor_ft"] <= 0.05, \
        f"sensor-only {a['floor_sensor_only_ft']:.2f} vs GUI {g['floor_ft']:.2f}"


def test_short_run(ctx):
    tmp = os.path.join(ctx["tmp"], "t6")
    os.makedirs(tmp)
    rng = np.random.default_rng(SIM_SEED + 5)
    x = synth_field_pt(300.0, 120.0, white_ft=21.0, rng=rng)
    run = write_analog_run(tmp, "short", {"z": x}, 300.0)
    s = run_analysis(targs(tmp, [run, "--labels", "short"]))
    r = s["results"]["runs"][0]
    assert r["short"] is True
    assert np.isfinite(r["floor_ft"]) and r["floor_ft"] > 0
    assert "0.01" not in r["lowf_ft"], f"0.01 Hz must be absent: {r['lowf_ft']}"


def write_manifest(tmp, rows, name="manifest.csv"):
    path = os.path.join(tmp, name)
    with open(path, "w", newline="\n") as fh:
        fh.write("run_dir,label,group,pair_id\n")
        for run, label, group, pid in rows:
            fh.write(f"{run},{label},{group},{pid}\n")
    return path


def run_self_test():
    import tempfile
    tmp = tempfile.mkdtemp(prefix="runs_compare_test_")
    print("=== runs_compare --test: synthetic checks (temp dir, removed after) ===")
    ctx = {"tmp": tmp}
    try:
        for test in (test_no_false_alarm, test_real_difference, test_pair,
                     test_gui_parsing, test_path_correction, test_short_run):
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
                   help="run dirs or GUI *_Formatted.txt files")
    p.add_argument("--manifest", default=None,
                   help="CSV with run_dir,label,group,pair_id")
    p.add_argument("--gui", nargs="*", default=None, metavar="FILE",
                   help="GUI record file(s), any name (fs=203.18, no meta)")
    p.add_argument("--labels", nargs="*", default=None,
                   help="labels for the positional runs")
    p.add_argument("--gain-v-per-nt", type=float, default=GAIN_MEASURED,
                   help="true gain for the epoch correction (default 2.624)")
    p.add_argument("--raw", action="store_true",
                   help="analyze raw volts instead of the pT files")
    p.add_argument("--band", nargs=2, type=float, default=[3.0, 40.0],
                   metavar=("LO", "HI"), help="white-floor band in Hz")
    p.add_argument("--axes", nargs="*", default=None,
                   help="restrict to these axes (default all in each run)")
    p.add_argument("--outdir", default=os.path.dirname(os.path.abspath(__file__)),
                   help="where the summary and figures go")
    p.add_argument("--force", action="store_true",
                   help="override the acceptance gate and mixed-metadata refusals")
    p.add_argument("--allow-sim", action="store_true",
                   help="accept runs recorded with --simulate")
    p.add_argument("--no-figures", action="store_true",
                   help="skip figures (headless quick check)")
    p.add_argument("--test", action="store_true",
                   help="offline self-test on synthetic runs")
    return p


def main(argv=None):
    args = build_parser().parse_args(argv)
    if args.test:
        return run_self_test()
    run_analysis(args)
    return 0


if __name__ == "__main__":
    sys.exit(main())
