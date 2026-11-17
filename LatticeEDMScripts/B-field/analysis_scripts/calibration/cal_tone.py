#!/usr/bin/env python3
"""Absolute gain (A1) and coil-constant (A2) calibration from a coil tone.

A sine current drives a coil inside the closed shield while the QZFM
analog output and the monitor-resistor voltage share one DAQ task. With a
trusted coil constant k the amplitude ratio gives the absolute gain
g = A_field/(k*I0); with a trusted gain it gives k instead -- the MS-1L
acceptance check. Amplitude sweeps add linearity, frequency sweeps add
flatness. Only raw volts enter the analysis: pT files embed a gain and
would make the measurement circular.
Writes cal_tone_summary.json, figures and ledger lines.
Offline check: python cal_tone.py --test
"""

from __future__ import annotations

import argparse
import csv
import glob
import json
import os
import shutil
import sys
from dataclasses import dataclass, asdict
from datetime import datetime

import numpy as np

GAIN_TABLE = {"0.33x": 0.9, "1x": 2.7, "3x": 8.1}  # V/nT, QuSpin nominal
GAIN_MEASURED = 2.624   # V/nT at 1x (relative to the factory calibration)
SETTLE_S = 500.0        # wait after Field Zero (tau~185 s transient)
QZFM_BW_HZ = 135.0      # sensor 3 dB bandwidth (spec class)
KNOWN_LINES_HZ = (50.0, 59.917, 100.0, 150.0, 203.18, 277.1, 294.0)
MIN_LINE_DIST_HZ = 0.7  # a tone this close to a known line is unusable
SIM_SEED = 20260707

import sys as _sys  # analysis_scripts/ on sys.path for _paths
_sys.path.insert(0, os.path.dirname(os.path.dirname(os.path.abspath(__file__))))
from _paths import DATA_ROOT, PROJECT_DIR, ANALYSIS_DIR, data, data_join, rel_data  # noqa: E402,F401
# run refs in the ledger are relative to the data root
REPO_ROOT = DATA_ROOT
DEFAULT_LEDGER = data("calibration", "ledger.jsonl")

C_F, C_M, C_ACC, C_MUTE, C_INK = "#2E6E8E", "#5A7D3A", "#C0504D", "#7A7F85", "#33373B"


def require(ok, msg, code=13):
    if not ok:
        print(f"[error] {msg}", file=sys.stderr)
        raise SystemExit(code)


# ------------------------------------------------------------ conversions

def gain_from_ratio(ratio, k_nt_per_ma, rmon_ohm):
    """g [V/nT] from the field/monitor amplitude ratio.
    I0 [mA] = A_mon/R * 1000, B0 [nT] = k*I0, g = A_field/B0."""
    return ratio / (k_nt_per_ma / rmon_ohm * 1000.0)


def coil_from_ratio(ratio, gain_v_per_nt, rmon_ohm):
    """k [nT/mA] from the ratio with a trusted gain:
    k = A_field/(g*I0) = ratio * R / (1000 * g)."""
    return ratio * rmon_ohm / (1000.0 * gain_v_per_nt)


# ---------------------------------------------------------------- loading

def find_meta(run_dir):
    metas = glob.glob(os.path.join(run_dir, "*.meta.json"))
    require(len(metas) == 1, f"{run_dir}: expected one meta.json, found {len(metas)}")
    with open(metas[0], encoding="utf-8") as fh:
        return json.load(fh), os.path.basename(metas[0])[:-len(".meta.json")]


from bfield.analysis.spectra import load_raw_csv  # noqa: E402 -- was an identical local copy


def check_run(meta, path, force=False, allow_sim=False):
    """Acceptance gate, calibration variant: a railed channel makes the
    amplitude wrong, so rails are refused even with --force."""
    if not (meta.get("status") == "ok" and meta.get("exit_code") == 0):
        require(force, f"{path}: status={meta.get('status')} exit_code="
                       f"{meta.get('exit_code')}; --force to analyze anyway")
        print(f"[gate] {path}: overridden status={meta.get('status')}")
    nonfinite = meta.get("n_nonfinite") or {}
    require(all(v == 0 for v in nonfinite.values()),
            f"{path}: non-finite samples: {nonfinite}")
    for key in ("rail_fraction", "qzfm_rail_fraction"):
        rails = {k: v for k, v in (meta.get(key) or {}).items() if v > 0.001}
        require(not rails, f"{path}: {key} above 0.1%: {rails} -- a clipped "
                           "calibration is worthless (never forceable)")
    if meta.get("source") == "simulate":
        require(allow_sim, f"{path}: run is simulated; pass --allow-sim if intended")
    fs, fse = meta.get("fs_hz"), meta.get("fs_effective_hz")
    if fs and fse and abs(fse / fs - 1.0) > 1e-3:
        print(f"[warn] {path}: fs_effective off nominal by "
              f"{abs(fse / fs - 1.0) * 100:.2f}%")


# ------------------------------------------------------------ line fitting

def line_fit(x, fs, f0, half=0.12, required=True):
    """Centroid frequency and amplitude of a spectral line near f0.
    Power centroid over f0 +/- half with the sideband median subtracted
    (robust to mains wander). Amplitude exact for a windowed sine by
    Parseval: A = sqrt(4*sum(p)/(n*sum(w^2)))."""
    x = x - x.mean()
    n = x.size
    w = np.hanning(n)
    p = np.abs(np.fft.rfft(x * w)) ** 2
    f = np.fft.rfftfreq(n, 1.0 / fs)
    m = (f >= f0 - half) & (f <= f0 + half)
    side = (((f >= f0 - 3 * half) & (f < f0 - half))
            | ((f > f0 + half) & (f <= f0 + 3 * half)))
    ps = np.clip(p[m] - np.median(p[side]), 0.0, None)
    if ps.sum() <= 0:
        require(not required, f"no line found near {f0} Hz")
        return f0, 0.0
    fc = float(np.sum(f[m] * ps) / np.sum(ps))
    amp = float(np.sqrt(4.0 * ps.sum() / (n * np.sum(w ** 2))))
    return fc, amp


def segmented_line_ratio(xa, xb, fs, f0, n_seg=8, min_cycles=20.0):
    """Amplitude ratio A(xa)/A(xb) at f0 with a segment-scatter error.
    The RATIO per segment is immune to drive-amplitude wander.
    n_used < 4 -> sem is None (single-shot, warn)."""
    n_seg = min(n_seg, int(xa.size / fs * f0 / min_cycles))
    if n_seg < 4:
        _, aa = line_fit(xa, fs, f0)
        _, ab = line_fit(xb, fs, f0)
        return aa / ab, None, 1
    L = xa.size // n_seg
    r = []
    for i in range(n_seg):
        s = slice(i * L, (i + 1) * L)
        _, aa = line_fit(xa[s], fs, f0, half=max(0.12, 3.0 * fs / L))
        _, ab = line_fit(xb[s], fs, f0, half=max(0.12, 3.0 * fs / L))
        r.append(aa / ab)
    r = np.asarray(r)
    return float(r.mean()), float(r.std(ddof=1) / np.sqrt(n_seg)), n_seg


# ---------------------------------------------------------------- ledger

def ledger_append(record, path):
    os.makedirs(os.path.dirname(path), exist_ok=True)
    record = {"date": datetime.now().isoformat(timespec="seconds"), **record}
    line = json.dumps(record, ensure_ascii=False)
    with open(path, "a", newline="\n") as fh:
        fh.write(line + "\n")
    print(f"[ledger] {line}")
    return record


# ---------------------------------------------------------------- rows

@dataclass
class ToneRow:
    run_dir: str
    f_drive: float
    rmon: float
    axis: str
    k: float | None       # gain mode when set
    g: float | None       # coil mode when set
    label: str


@dataclass
class ToneResult:
    row: ToneRow
    mode: str             # gain / coil
    fc_field: float
    fc_mon: float
    a_field_v: float
    a_mon_v: float
    i_ma: float
    b_applied_nt: float
    value: float
    stat: float
    syst: float
    hd2: float
    hd3: float
    hd2_mon: float
    offaxis_ratio: float | None
    n_seg: int
    gain_mode: str
    simulated: bool
    warnings: list


def check_f_drive(f0):
    require(0.3 <= f0 <= 100.0, f"--f-drive {f0} Hz outside 0.3-100 Hz")
    for h in sorted(set(KNOWN_LINES_HZ) | {50.0, 100.0, 150.0}):
        require(abs(f0 - h) > MIN_LINE_DIST_HZ,
                f"--f-drive {f0} Hz is within {MIN_LINE_DIST_HZ} Hz of the "
                f"known {h} Hz line; pick another frequency (7.3, 11.7, 17.3)")
    if f0 < 3.0:
        print(f"[warn] {f0} Hz is below the white band: this measures the "
              "closed-loop Field-Zero response, not the bare gain")


def row_mode(row):
    require((row.k is None) != (row.g is None),
            f"{row.run_dir}: exactly one of the coil constant (k) and the "
            f"gain (g) must be trusted; got k={row.k}, g={row.g}")
    return "gain" if row.k is not None else "coil"


def analyze_row(row, args):
    mode = row_mode(row)
    check_f_drive(row.f_drive)
    require(row.rmon and row.rmon > 0, f"{row.run_dir}: monitor resistor missing")

    meta, session = find_meta(row.run_dir)
    check_run(meta, row.run_dir, args.force, args.allow_sim)
    fs = float(meta["fs_hz"])
    require(row.axis in (meta.get("channel_map") or {}),
            f"{row.run_dir}: axis {row.axis!r} not in the run's channel_map")
    require(args.mon_name in (meta.get("aux_map") or {}),
            f"{row.run_dir}: monitor {args.mon_name!r} not in the run's aux_map "
            "(record with --aux mon:ai3)")

    cols = load_raw_csv(os.path.join(row.run_dir, f"{session}_raw.csv"))
    require(row.axis in cols and args.mon_name in cols,
            f"{row.run_dir}: raw.csv lacks {row.axis} or {args.mon_name}")
    warnings = []
    dur = cols[row.axis].size / fs
    trim = SETTLE_S if dur >= 800.0 else 60.0
    if dur < 800.0:
        warnings.append("settling may contaminate (run < 800 s)")
    n0 = int(trim * fs)
    v_f, v_m = cols[row.axis][n0:], cols[args.mon_name][n0:]
    T = v_f.size / fs

    half = max(0.12, 3.0 / T)
    fc_f, a_f = line_fit(v_f, fs, row.f_drive, half)
    fc_m, a_m = line_fit(v_m, fs, row.f_drive, half)
    require(abs(fc_f - fc_m) <= 2 * half,
            f"{row.run_dir}: tone not found coherently (field {fc_f:.3f} vs "
            f"monitor {fc_m:.3f} Hz) -- wrong --f-drive, or dead monitor")
    require(abs(fc_m - row.f_drive) <= 0.5,
            f"{row.run_dir}: monitor tone at {fc_m:.3f} Hz, expected "
            f"{row.f_drive} Hz")

    ratio, sem, n_seg = segmented_line_ratio(v_f, v_m, fs, fc_m, n_seg=args.seg)
    i_ma = a_m / row.rmon * 1000.0
    if mode == "gain":
        value = gain_from_ratio(ratio, row.k, row.rmon)
        b_nt = row.k * i_ma
        tol = args.k_tol
    else:
        value = coil_from_ratio(ratio, row.g, row.rmon)
        b_nt = value * i_ma
        tol = args.gain_tol
    if sem is None:
        warnings.append("single-shot (record too short for segments); "
                        "no statistical error")
        stat = float("nan")
    else:
        stat = value * sem / ratio
    syst = value * float(np.hypot(args.rmon_tol, tol))

    # harmonics: field-channel distortion vs the drive's own distortion
    nyq = 0.95 * fs / 2.0
    hd2 = hd3 = hd2_mon = 0.0
    if 2 * fc_m < nyq:
        hd2 = line_fit(v_f, fs, 2 * fc_m, required=False)[1] / a_f
        hd2_mon = line_fit(v_m, fs, 2 * fc_m, required=False)[1] / a_m
    if 3 * fc_m < nyq:
        hd3 = line_fit(v_f, fs, 3 * fc_m, required=False)[1] / a_f
    if hd2 > 0.01 or hd3 > 0.01:
        src = ("driver distortion" if hd2_mon > 0.5 * hd2
               else "sensor/shield nonlinearity")
        warnings.append(f"harmonic distortion HD2 {hd2 * 100:.1f}% / HD3 "
                        f"{hd3 * 100:.1f}% -> {src}")

    offaxis = None
    others = [a for a in meta.get("axes", []) if a != row.axis and a in cols]
    if others:
        a_off = line_fit(cols[others[0]][n0:], fs, fc_m, required=False)[1]
        offaxis = a_off / a_f
        if offaxis > 0.05:
            warnings.append(f"off-axis tone {offaxis * 100:.1f}% on "
                            f"{others[0]} -> coil-axis misalignment "
                            "(fix geometry, do not average)")

    # sanity windows: a value outside these is a wiring/units mistake,
    # not a calibration result
    gm = meta.get("gain_mode", "1x")
    if mode == "gain":
        scale = GAIN_TABLE.get(gm, 2.7) / 2.7
        require(1.5 * scale <= value <= 4.0 * scale,
                f"{row.run_dir}: g = {value:.4g} V/nT outside the sane window "
                f"for {gm} -- check R_mon, k and the wiring")
    else:
        k_nom = args.k_nominal
        lo, hi = (k_nom / 3.0, 3.0 * k_nom) if k_nom else (1.0, 1000.0)
        require(lo <= value <= hi,
                f"{row.run_dir}: k = {value:.4g} nT/mA outside [{lo:.3g}, "
                f"{hi:.3g}] -- check R_mon, g and the wiring")

    return ToneResult(
        row=row, mode=mode, fc_field=fc_f, fc_mon=fc_m,
        a_field_v=a_f, a_mon_v=a_m, i_ma=i_ma, b_applied_nt=b_nt,
        value=value, stat=stat, syst=syst,
        hd2=hd2, hd3=hd3, hd2_mon=hd2_mon,
        offaxis_ratio=offaxis, n_seg=n_seg, gain_mode=gm,
        simulated=meta.get("source") == "simulate",
        warnings=warnings,
    ), (v_f, v_m, fs, fc_m, a_f, a_m)


# ---------------------------------------------------------------- sweeps

def fit_linearity(results):
    rows = [r for r in results if r.row.label.startswith("lin-")]
    if len(rows) < 3:
        return None
    fcs = [r.fc_mon for r in rows]
    if max(fcs) - min(fcs) > 0.1:
        print("[warn] linearity rows differ in frequency; fit skipped")
        return None
    b = np.array([r.b_applied_nt for r in rows])
    a = np.array([r.a_field_v for r in rows])
    sig = np.array([r.a_field_v * (r.stat / r.value) if np.isfinite(r.stat)
                    else a.mean() * 0.01 for r in rows])
    W = 1.0 / sig ** 2
    X = np.vstack([b, np.ones_like(b)]).T
    cov = np.linalg.inv(X.T @ (X * W[:, None]))
    slope, icpt = cov @ (X.T @ (W * a))
    fit = slope * b + icpt
    max_dev = float(np.max(np.abs(a - fit) / fit) * 100.0)
    # The quoted slope error comes from (X^T W X)^-1 alone, so it is
    # optimistic whenever the points scatter by more than they claim.
    # The PASS test below is residual-based and is unaffected; only
    # slope_err moves (2026-08-19 analysis review, M5).
    ok = max_dev < 0.5 and abs(icpt) < 2.0 * np.sqrt(cov[1, 1])
    return {"slope": float(slope), "slope_err": float(np.sqrt(cov[0, 0])),
            "intercept_v": float(icpt),
            "intercept_err_v": float(np.sqrt(cov[1, 1])),
            "max_dev_pct": max_dev, "n": len(rows), "pass": bool(ok)}


def fit_flatness(results):
    rows = [r for r in results if r.row.label.startswith("flat-")]
    if len(rows) < 3:
        return None
    ref = min(rows, key=lambda r: abs(r.fc_mon - 7.3))
    pts = []
    for r in sorted(rows, key=lambda r: r.fc_mon):
        rel = r.value / ref.value
        err = rel * float(np.hypot(
            r.stat / r.value if np.isfinite(r.stat) else 0.0,
            ref.stat / ref.value if np.isfinite(ref.stat) else 0.0))
        pts.append([float(r.fc_mon), float(rel), float(err)])
    band = [p for p in pts if 3.0 <= p[0] <= 40.0]
    band_dev = max(abs(p[1] - 1.0) for p in band) * 100.0 if band else None
    ok = band_dev is not None and band_dev < 1.0

    def crossing(level):
        # log-linear interpolation of the first downward crossing
        for (f1, r1, _), (f2, r2, _) in zip(pts, pts[1:]):
            if r1 >= level > r2:
                lf = np.interp(level, [r2, r1], [np.log(f2), np.log(f1)])
                return float(np.exp(lf))
        return None

    return {"f_ref_hz": float(ref.fc_mon), "points": pts,
            "band_dev_pct": band_dev,
            "f_minus3pct_hz": crossing(0.97),
            "f_minus10pct_hz": crossing(0.90), "pass": bool(ok)}


# ---------------------------------------------------------------- report

def report_result(r):
    row = r.row
    print(f"[run] {row.run_dir}  axis={row.axis}  gain_mode={r.gain_mode}  "
          f"label={row.label or '-'}")
    print(f"[fit] tone {r.fc_field:.4f} Hz (field) / {r.fc_mon:.4f} Hz "
          f"(monitor)  n_seg={r.n_seg}")
    print(f"[amp] field {r.a_field_v:.5f} V   monitor {r.a_mon_v:.5f} V   "
          f"I={r.i_ma:.4g} mA   B={r.b_applied_nt:.4f} nT")
    unit = "V/nT" if r.mode == "gain" else "nT/mA"
    stat = f"{r.stat:.4f}" if np.isfinite(r.stat) else "n/a"
    print(f"[out] {'g' if r.mode == 'gain' else 'k'} = {r.value:.4f} {unit}"
          f"  +/- {stat} stat  +/- {r.syst:.4f} syst")
    off = f"   off-axis {r.offaxis_ratio * 100:.1f}%" if r.offaxis_ratio \
          is not None else ""
    print(f"[hd ] HD2 {r.hd2 * 100:.2f}%  HD3 {r.hd3 * 100:.2f}%  "
          f"(monitor HD2 {r.hd2_mon * 100:.2f}%){off}")
    for w in r.warnings:
        print(f"[warn] {w}")


# ---------------------------------------------------------------- figures

def make_figures(outdir, results, spectra, lin, flat):
    import matplotlib
    matplotlib.use("Agg")
    import matplotlib.pyplot as plt
    plt.rcParams.update({
        "figure.dpi": 150, "font.size": 11, "axes.titleweight": "bold",
        "axes.spines.top": False, "axes.spines.right": False,
        "axes.edgecolor": C_MUTE, "text.color": C_INK, "legend.frameon": False,
    })
    from scipy.signal import welch

    v_f, v_m, fs, fc, a_f, a_m = spectra
    fig, axes = plt.subplots(2, 1, figsize=(7.0, 6.0), sharex=True)
    for ax, v, amp, color, name in ((axes[0], v_f, a_f, C_F, "field"),
                                    (axes[1], v_m, a_m, C_M, "monitor")):
        f, p = welch(v, fs=fs, nperseg=min(len(v), int(20 * fs)),
                     detrend="linear")
        m = (f >= fc - 1.5) & (f <= fc + 1.5)
        ax.semilogy(f[m], np.sqrt(p[m]), color=color, lw=1.0)
        enbw = 1.5 * (f[1] - f[0])   # Hann, 20 s segments
        ax.axhline(amp / np.sqrt(2 * enbw), color=C_ACC, ls="--", lw=1.0)
        ax.annotate(f"A = {amp:.5f} V @ {fc:.3f} Hz", (0.02, 0.92),
                    xycoords="axes fraction", fontsize=9, color=C_ACC)
        ax.set_ylabel(f"{name} [V/rtHz]")
    axes[1].set_xlabel("frequency [Hz]")
    axes[0].set_title("tone (dashed: fitted amplitude, ASD-equivalent height)")
    fig.savefig(os.path.join(outdir, "cal_tone_spectrum.png"),
                bbox_inches="tight")
    plt.close(fig)

    if lin:
        rows = [r for r in results if r.row.label.startswith("lin-")]
        b = np.array([r.b_applied_nt for r in rows])
        a = np.array([r.a_field_v for r in rows])
        fit = lin["slope"] * b + lin["intercept_v"]
        fig, (ax, axr) = plt.subplots(2, 1, figsize=(6.5, 6.0), sharex=True,
                                      height_ratios=[3, 1])
        ax.plot(b, a, "o", color=C_F)
        ax.plot(b, fit, "-", color=C_MUTE, lw=1.0,
                label=f"slope {lin['slope']:.4f} V/nT")
        ax.set_ylabel("field amplitude [V]")
        ax.legend(fontsize=9)
        ax.set_title("linearity")
        axr.axhspan(-0.5, 0.5, color="#E6E8EA")
        axr.plot(b, (a - fit) / fit * 100.0, "o", color=C_F)
        axr.axhline(0, color=C_MUTE, lw=0.8)
        axr.set_xlabel("applied field [nT]")
        axr.set_ylabel("dev [%]")
        fig.savefig(os.path.join(outdir, "cal_tone_linearity.png"),
                    bbox_inches="tight")
        plt.close(fig)

    if flat:
        pts = np.array(flat["points"])
        fig, ax = plt.subplots(figsize=(6.8, 4.2))
        ax.errorbar(pts[:, 0], pts[:, 1], yerr=pts[:, 2], fmt="o-",
                    color=C_F, capsize=3, lw=0.9)
        ax.set_xscale("log")
        ax.axhspan(0.99, 1.01, xmin=0, color="#E6E8EA")
        ax.axvline(QZFM_BW_HZ, color=C_MUTE, ls=":", lw=1.0)
        ax.annotate(f"sensor BW {QZFM_BW_HZ:g} Hz", (QZFM_BW_HZ, 1.0),
                    rotation=90, fontsize=8, color=C_MUTE, va="center",
                    ha="right")
        ax.set_xlabel("frequency [Hz]")
        ax.set_ylabel(f"g(f) / g({flat['f_ref_hz']:.1f} Hz)")
        ax.set_title("in-band flatness")
        fig.savefig(os.path.join(outdir, "cal_tone_flatness.png"),
                    bbox_inches="tight")
        plt.close(fig)


# ---------------------------------------------------------------- pipeline

def parse_manifest(path):
    rows = []
    with open(path, newline="") as fh:
        lines = [ln for ln in fh if not ln.lstrip().startswith("#")]
    for i, rec in enumerate(csv.DictReader(lines), start=2):
        try:
            k = float(rec["k_nt_per_ma"]) if rec.get("k_nt_per_ma") else None
            g = float(rec["gain_v_per_nt"]) if rec.get("gain_v_per_nt") else None
            rows.append(ToneRow(
                run_dir=rec["run_dir"].strip(),
                f_drive=float(rec["f_drive_hz"]),
                rmon=float(rec["rmon_ohm"]),
                axis=(rec.get("axis") or "z").strip(),
                k=k, g=g, label=(rec.get("label") or "").strip()))
        except (KeyError, ValueError) as e:
            require(False, f"{path} row {i}: {e}")
    return rows


def build_rows(args):
    if args.manifest:
        require(not args.run_dirs, "--manifest and positional runs are "
                                   "mutually exclusive")
        return parse_manifest(args.manifest)
    require(args.run_dirs, "no run directories given (see --help)")
    require(args.f_drive is not None, "--f-drive is required without a manifest")
    require(args.rmon_ohm is not None, "--rmon-ohm is required without a manifest")
    return [ToneRow(run_dir=d, f_drive=args.f_drive, rmon=args.rmon_ohm,
                    axis=args.axis, k=args.k_nt_per_ma,
                    g=args.gain_v_per_nt, label="")
            for d in args.run_dirs]


def run_analysis(args):
    rows = build_rows(args)
    results, first_spectra = [], None
    for row in rows:
        res, spectra = analyze_row(row, args)
        results.append(res)
        if first_spectra is None:
            first_spectra = spectra
    modes = {r.mode for r in results}
    print(f"=== cal_tone: {' + '.join(sorted(modes))} mode, "
          f"{len(results)} run(s) ===")
    for r in results:
        report_result(r)

    lin = fit_linearity(results)
    if lin:
        print(f"=== sweep: linearity ({lin['n']} points) ===")
        print(f"[fit] slope {lin['slope']:.4f} +/- {lin['slope_err']:.4f} V/nT"
              f"   intercept {lin['intercept_v'] * 1e3:+.2f} +/- "
              f"{lin['intercept_err_v'] * 1e3:.2f} mV   max dev "
              f"{lin['max_dev_pct']:.2f}%   {'PASS' if lin['pass'] else 'FAIL'}")
    flat = fit_flatness(results)
    if flat:
        f3 = (f"   f(-3%) ~ {flat['f_minus3pct_hz']:.0f} Hz"
              if flat["f_minus3pct_hz"] else "")
        print(f"=== sweep: flatness ({len(flat['points'])} points, ref "
              f"{flat['f_ref_hz']:.1f} Hz) ===")
        print(f"[fit] 3-40 Hz within {flat['band_dev_pct']:.2f}%   "
              f"{'PASS' if flat['pass'] else 'FAIL'}{f3}")

    # ledger: one line per row; simulated runs never touch the shared ledger
    any_sim = any(r.simulated for r in results)
    skip_ledger = any_sim and \
        os.path.abspath(args.ledger) == os.path.abspath(DEFAULT_LEDGER)
    if skip_ledger:
        print("[note] simulated input: not writing to the shared ledger")
    # A sweep that FAILED linearity or flatness produced these same
    # numbers, and they used to enter the ledger unmarked -- where
    # cal_stability picks them up as recommended values. The row
    # carries its own verdict now (2026-08-19 analysis review, H3).
    sweep_ok = all(f["pass"] for f in (lin, flat) if f)
    for r in (() if skip_ledger else results):
        run_ref = os.path.abspath(r.row.run_dir)
        if run_ref.startswith(REPO_ROOT + os.sep):
            run_ref = os.path.relpath(run_ref, REPO_ROOT)
        unc = float(np.hypot(r.stat, r.syst)) if np.isfinite(r.stat) else None
        ledger_append({
            "kind": "gain_tone" if r.mode == "gain" else "coil_k",
            "value": round(r.value, 4),
            "unc": round(unc, 4) if unc else None,
            "unit": "V/nT" if r.mode == "gain" else "nT/mA",
            "tool": "cal_tone.py", "run": run_ref, "axis": r.row.axis,
            "gain_mode": r.gain_mode,
            "conditions": f"f={r.fc_mon:.2f} Hz, B0={r.b_applied_nt:.3f} nT, "
                          f"R_mon={r.row.rmon:g} ohm",
            "pass": bool(np.isfinite(r.stat)) and sweep_ok,
        }, args.ledger)

    ok = all(np.isfinite(r.stat) for r in results)
    for fit in (lin, flat):
        if fit:
            ok = ok and fit["pass"]

    os.makedirs(args.outdir, exist_ok=True)
    summary = {
        "program": "cal_tone.py",
        "generated": datetime.now().isoformat(timespec="seconds"),
        "inputs": {"runs": [r.run_dir for r in rows],
                   "args": {k: v for k, v in vars(args).items()
                            if k not in ("run_dirs", "test")}},
        "results": {
            "mode": sorted(modes)[0] if len(modes) == 1 else "mixed",
            "rows": [{**{k: v for k, v in asdict(r).items() if k != "row"},
                      "run": r.row.run_dir, "axis": r.row.axis,
                      "label": r.row.label} for r in results],
            "linearity": lin, "flatness": flat,
        },
        "warnings": sorted({w for r in results for w in r.warnings}),
        "pass": bool(ok),
    }
    path = os.path.join(args.outdir, "cal_tone_summary.json")
    with open(path, "w", newline="\n") as fh:
        json.dump(summary, fh, indent=1)
    print(f"[out ] {path}")

    if not args.no_figures and first_spectra:
        make_figures(args.outdir, results, first_spectra, lin, flat)
        print(f"[out ] figures in {args.outdir}")
    return summary


# ---------------------------------------------------------------- self-test

def synth_tone_run(tmp, session, fs=1200.0, seconds=300.0, f0=7.3,
                   g_true=2.62, k_true=69.0, rmon=9994.0, i0_ma=3.0e-3,
                   white_ft=21.0, mains_pt=0.14, mon_noise_v=50e-6,
                   hd2_field=0.0, hd2_drive=0.0, scale_field=1.0,
                   rail=0.0, rng=None):
    """Field channel = g*(k*I(t)) volts + sensor noise; monitor = I*R."""
    rng = rng if rng is not None else np.random.default_rng(SIM_SEED)
    n = int(fs * seconds)
    t = np.arange(n) / fs
    i = i0_ma * np.sin(2 * np.pi * f0 * t)
    if hd2_drive:
        i = i + hd2_drive * i0_ma * np.sin(2 * np.pi * 2 * f0 * t)
    b_nt = k_true * i
    noise_pt = rng.normal(0.0, white_ft * 1e-3 * np.sqrt(fs / 2.0), n) \
        + mains_pt * np.sin(2 * np.pi * 50.0 * t)
    v_f = scale_field * g_true * b_nt + g_true * noise_pt / 1000.0
    if hd2_field:
        v_f = v_f + hd2_field * g_true * k_true * i0_ma \
            * np.sin(2 * np.pi * 2 * f0 * t)
    v_m = i * 1e-3 * rmon + rng.normal(0.0, mon_noise_v, n)
    v_y = g_true * rng.normal(0.0, white_ft * 1e-3 * np.sqrt(fs / 2.0), n) / 1000.0

    run = os.path.join(tmp, session)
    os.makedirs(run)
    names = ["z", "y", "mon"]
    series = {"z": v_f, "y": v_y, "mon": v_m}
    header = (f"# QZFM raw voltages | session={session} "
              f"start=2026-07-08T09:00:00 fs={fs:g}\n"
              "sample," + ",".join(f"V_{c}" for c in names))
    table = np.column_stack([np.arange(n), *[series[c] for c in names]])
    np.savetxt(os.path.join(run, f"{session}_raw.csv"), table,
               fmt=["%d"] + ["%.6g"] * 3, delimiter=",",
               header=header, comments="")
    meta = {
        "start_iso": "2026-07-08T09:00:00", "session": session,
        "axes": ["z", "y"],
        "channel_map": {"z": "Dev2/ai0", "y": "Dev2/ai1"},
        "aux_map": {"mon": "Dev2/ai3"},
        "fs_hz": fs, "n_samples": n, "duration_s": n / fs,
        "clock_elapsed_s": n / fs, "fs_effective_hz": fs,
        "gain_mode": "1x", "cal_v_per_nT_used": 2.7,
        "cal_pT_per_V": 1000.0 / 2.7, "vrange": 1.0, "device": "Dev2",
        "sensor_id": "SIM",
        "rail_fraction": {"z": rail, "y": 0.0, "mon": 0.0},
        "qzfm_rail_fraction": {"z": 0.0, "y": 0.0, "mon": 0.0},
        "n_nonfinite": {"z": 0, "y": 0, "mon": 0},
        "flat_ptp_V": {"z": 0.5, "y": 0.02, "mon": 0.06},
        "mux_skew_note": "simulated", "source": "simulate",
        "software": "qzfm_record.py", "software_legacy": "qzfm_analog.py",
        "topology": "1head", "status": "ok", "exit_code": 0,
        "error": None, "warnings": [], "notes": "cal_tone --test",
    }
    with open(os.path.join(run, f"{session}.meta.json"), "w", newline="\n") as fh:
        json.dump(meta, fh, indent=1)
    return run


def targs(tmp, extra):
    argv = [*extra, "--allow-sim", "--no-figures",
            "--outdir", os.path.join(tmp, "out"),
            "--ledger", os.path.join(tmp, "ledger.jsonl")]
    return build_parser().parse_args(argv)


def test_gain_recovery(ctx):
    run = synth_tone_run(ctx["tmp"], "gain",
                         rng=np.random.default_rng(SIM_SEED))
    s = run_analysis(targs(ctx["tmp"], [run, "--f-drive", "7.3",
                                        "--rmon-ohm", "9994",
                                        "--k-nt-per-ma", "69"]))
    r = s["results"]["rows"][0]
    assert abs(r["value"] - 2.62) <= 3 * r["stat"], \
        f"g = {r['value']:.4f} +/- {r['stat']:.4f}, truth 2.62"
    assert r["stat"] <= 0.005 * r["value"], f"stat too large: {r['stat']}"
    ctx["gain_run"] = run


def test_coil_recovery(ctx):
    s = run_analysis(targs(ctx["tmp"], [ctx["gain_run"], "--f-drive", "7.3",
                                        "--rmon-ohm", "9994",
                                        "--gain-v-per-nt", "2.62"]))
    r = s["results"]["rows"][0]
    assert abs(r["value"] - 69.0) <= 3 * r["stat"], \
        f"k = {r['value']:.3f} +/- {r['stat']:.3f}, truth 69"


def test_hand_numbers(ctx):
    # section-10 numbers guard the 1000/mA/ohm factors
    ratio = 0.54320 / 0.03000
    g = gain_from_ratio(ratio, 69.0, 9994.0)
    b0 = 69.0 * (0.03000 / 9994.0 * 1000.0)
    assert abs(g - 0.54320 / b0) / g < 1e-6, g
    k = coil_from_ratio(ratio, 2.624, 9994.0)
    assert abs(k - 0.54320 / (2.624 * 0.03000 / 9994.0 * 1000.0)) / k < 1e-6, k


def linearity_manifest(ctx, name, compress_top):
    tmp = os.path.join(ctx["tmp"], name)
    os.makedirs(tmp)
    rng = np.random.default_rng(SIM_SEED + 4)
    rows = []
    for j, b0 in enumerate((0.05, 0.1, 0.2, 0.3)):
        scale = compress_top if (j == 3) else 1.0
        run = synth_tone_run(tmp, f"lin{j}", i0_ma=b0 / 69.0,
                             scale_field=scale, rng=rng)
        rows.append(f"{run},7.3,9994,z,69,,lin-{b0}nT")
    man = os.path.join(tmp, "lin.csv")
    with open(man, "w", newline="\n") as fh:
        fh.write("run_dir,f_drive_hz,rmon_ohm,axis,k_nt_per_ma,"
                 "gain_v_per_nt,label\n" + "\n".join(rows) + "\n")
    return run_analysis(targs(ctx["tmp"], ["--manifest", man]))


def test_linearity(ctx):
    s = linearity_manifest(ctx, "linA", compress_top=1.0)
    lin = s["results"]["linearity"]
    assert lin["pass"] and lin["max_dev_pct"] < 0.1, lin
    s = linearity_manifest(ctx, "linB", compress_top=0.99)
    lin = s["results"]["linearity"]
    assert not lin["pass"] and 0.7 <= lin["max_dev_pct"] <= 1.3, lin


def test_attribution(ctx):
    run = synth_tone_run(ctx["tmp"], "hdfield", hd2_field=0.02,
                         rng=np.random.default_rng(SIM_SEED + 5))
    s = run_analysis(targs(ctx["tmp"], [run, "--f-drive", "7.3",
                                        "--rmon-ohm", "9994",
                                        "--k-nt-per-ma", "69"]))
    w = " ".join(s["results"]["rows"][0]["warnings"])
    assert "sensor/shield nonlinearity" in w, w
    run = synth_tone_run(ctx["tmp"], "hddrive", hd2_drive=0.02,
                         rng=np.random.default_rng(SIM_SEED + 6))
    s = run_analysis(targs(ctx["tmp"], [run, "--f-drive", "7.3",
                                        "--rmon-ohm", "9994",
                                        "--k-nt-per-ma", "69"]))
    w = " ".join(s["results"]["rows"][0]["warnings"])
    assert "driver" in w, w


def test_refusals(ctx):
    run = ctx["gain_run"]

    def expect13(extra):
        try:
            run_analysis(targs(ctx["tmp"], extra))
        except SystemExit as e:
            assert e.code == 13, f"{extra}: exited {e.code}, want 13"
        else:
            raise AssertionError(f"{extra}: must be refused (13)")

    expect13([run, "--f-drive", "50.2", "--rmon-ohm", "9994",
              "--k-nt-per-ma", "69"])
    railed = synth_tone_run(ctx["tmp"], "railed", rail=0.02,
                            rng=np.random.default_rng(SIM_SEED + 7))
    expect13([railed, "--f-drive", "7.3", "--rmon-ohm", "9994",
              "--k-nt-per-ma", "69", "--force"])
    expect13([run, "--f-drive", "7.3", "--rmon-ohm", "9994",
              "--k-nt-per-ma", "69", "--mon-name", "nomon"])
    expect13([run, "--f-drive", "7.3", "--rmon-ohm", "9994",
              "--k-nt-per-ma", "69", "--gain-v-per-nt", "2.62"])


def test_ledger(ctx):
    with open(os.path.join(ctx["tmp"], "ledger.jsonl")) as fh:
        records = [json.loads(line) for line in fh]
    kinds = {r["kind"] for r in records}
    assert kinds <= {"gain_tone", "coil_k"}, kinds
    assert any(r["kind"] == "gain_tone" and r["unit"] == "V/nT"
               for r in records)
    assert any(r["kind"] == "coil_k" and r["unit"] == "nT/mA"
               for r in records)


def run_self_test():
    import tempfile
    tmp = tempfile.mkdtemp(prefix="cal_tone_test_")
    print("=== cal_tone --test: synthetic checks (temp dir, removed after) ===")
    ctx = {"tmp": tmp}
    try:
        for test in (test_gain_recovery, test_coil_recovery, test_hand_numbers,
                     test_linearity, test_attribution, test_refusals,
                     test_ledger):
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
    p.add_argument("run_dirs", nargs="*", metavar="RUN_DIR",
                   help="tone run dir(s) recorded with --aux mon:aiN")
    p.add_argument("--manifest", default=None,
                   help="sweep CSV: run_dir,f_drive_hz,rmon_ohm,axis,"
                        "k_nt_per_ma,gain_v_per_nt,label")
    p.add_argument("--f-drive", type=float, default=None,
                   help="drive frequency in Hz (>= 0.7 Hz from known lines; "
                        "below 3 Hz measures the closed-loop response)")
    p.add_argument("--rmon-ohm", type=float, default=None,
                   help="monitor resistor in ohm (DMM value)")
    p.add_argument("--rmon-tol", type=float, default=0.001,
                   help="relative systematic of R_mon (default 0.1%%)")
    p.add_argument("--k-nt-per-ma", type=float, default=None,
                   help="gain mode: trusted coil constant in nT/mA")
    p.add_argument("--k-tol", type=float, default=0.05,
                   help="relative systematic of k (default 5%% MS-1L nominal)")
    p.add_argument("--k-nominal", type=float, default=None,
                   help="coil mode: nominal k for the sanity window")
    p.add_argument("--gain-v-per-nt", type=float, default=None,
                   help="coil mode: trusted gain in V/nT")
    p.add_argument("--gain-tol", type=float, default=0.03,
                   help="relative systematic of the trusted gain (default 3%%)")
    p.add_argument("--axis", default="z", choices=("z", "y"),
                   help="field channel carrying the tone")
    p.add_argument("--mon-name", default="mon",
                   help="monitor aux-channel name (default mon)")
    p.add_argument("--seg", type=int, default=8,
                   help="target segment count for the scatter error")
    p.add_argument("--outdir", default=os.path.dirname(os.path.abspath(__file__)),
                   help="where the summary and figures go")
    p.add_argument("--ledger", default=DEFAULT_LEDGER,
                   help="calibration ledger path")
    p.add_argument("--force", action="store_true",
                   help="analyze runs whose status is not ok (rails: never)")
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
    for name, val, lo, hi in (("--rmon-tol", args.rmon_tol, 0.0, 0.1),
                              ("--k-tol", args.k_tol, 0.0, 0.5),
                              ("--gain-tol", args.gain_tol, 0.0, 0.2)):
        require(lo <= val < hi, f"{name} {val} outside [{lo}, {hi})")
    require(args.seg >= 4, "--seg must be at least 4")
    if args.gain_v_per_nt is not None:
        require(1.5 <= args.gain_v_per_nt <= 4.0,
                f"--gain-v-per-nt {args.gain_v_per_nt} implausible at 1x")
    run_analysis(args)
    return 0


if __name__ == "__main__":
    sys.exit(main())
