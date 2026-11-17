#!/usr/bin/env python3
"""Split a two-head run into real field and per-head sensor noise.

Two QZFM heads with independent cells and lasers watch the same lab axis
on one DAQ clock (a qzfm_record.py --2head run). What they see
coherently is field
(or genuinely environmental common-mode); what remains incoherent is
per-head sensor noise. This settles the question every single-sensor
result leaves open -- with one stated caveat: a room-temperature
common-mode acting on both cells identically stays in the "field" bucket.
Writes cross_spectrum_summary.json and 3 figures.
Offline check: python cross_spectrum.py --test
"""

from __future__ import annotations

import argparse
import glob
import json
import os
import shutil
import sys
from dataclasses import dataclass, asdict
from datetime import datetime

import numpy as np
from scipy.signal import welch, csd, butter, sosfiltfilt
from scipy.optimize import curve_fit

GAIN_MEASURED = 2.624   # V/nT at 1x, head Z3D0-AB3Q (sync5)
SETTLE_S = 500.0        # wait after Field Zero (tau~185 s transient)
QZFM_SPEC_FT = (7.0, 15.0)   # single-axis self-noise spec band, fT/rtHz
FLOOR_SINGLE_FT = 21.0       # measured single-head dual-axis floor (sync5)
KNOWN_LINES_HZ = (50.0, 59.917, 100.0, 150.0, 203.18, 277.1, 294.0)
SIM_SEED = 20260707

REPO_ROOT = os.path.dirname(os.path.dirname(
    os.path.dirname(os.path.abspath(__file__))))

C_A, C_B, C_COM, C_MUTE, C_INK = "#2E6E8E", "#5A7D3A", "#C0504D", "#7A7F85", "#33373B"


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


import sys as _sys  # analysis_scripts/ on sys.path for _paths
_sys.path.insert(0, os.path.dirname(os.path.dirname(os.path.abspath(__file__))))
import _paths  # noqa: E402,F401  (puts B-field on sys.path)
from bfield.analysis.spectra import load_raw_csv  # noqa: E402 -- was an identical local copy


def check_run(meta, force=False, allow_sim=False):
    """Acceptance gate. Rails and non-finite samples are hard refusals --
    a railed or broken channel would fake incoherence and bias the split."""
    if not (meta.get("status") == "ok" and meta.get("exit_code") == 0):
        require(force, f"run status={meta.get('status')} exit_code="
                       f"{meta.get('exit_code')}; --force to analyze anyway")
        print(f"[gate] overridden: status={meta.get('status')}")
    nonfinite = meta.get("n_nonfinite") or {}
    require(all(v == 0 for v in nonfinite.values()),
            f"non-finite samples in run: {nonfinite}")
    for key in ("rail_fraction", "qzfm_rail_fraction"):
        rails = {k: v for k, v in (meta.get(key) or {}).items() if v > 0.001}
        require(not rails, f"{key} above 0.1%: {rails} (no --force here)")
    if meta.get("source") == "simulate":
        require(allow_sim, "run is simulated; pass --allow-sim if that is intended")
    fs, fse = meta.get("fs_hz"), meta.get("fs_effective_hz")
    if fs and fse and abs(fse / fs - 1.0) > 1e-3:
        print(f"[warn] fs_effective off nominal by {abs(fse / fs - 1) * 100:.2f}%")


# ---------------------------------------------------------------- spectra

def cross_set(x1, x2, fs, seg_s):
    """One-clock Welch cross set: f, P11, P22, P12 (complex), gamma2,
    n_avg. Hann window, linear detrend, scipy's default 50% overlap."""
    nperseg = min(len(x1), int(seg_s * fs))
    kw = dict(fs=fs, nperseg=nperseg, detrend="linear")
    f, p11 = welch(x1, **kw)
    _, p22 = welch(x2, **kw)
    _, p12 = csd(x1, x2, **kw)
    gamma2 = np.abs(p12) ** 2 / np.clip(p11 * p22, 1e-300, None)
    n_avg = max(1, 2 * len(x1) // nperseg - 1)
    return f, p11, p22, p12, gamma2, n_avg


def coh_thresholds(n_avg):
    """(bias, z95) of magnitude-squared coherence under ZERO true
    coherence: bias = E[gamma2] = 1/n_avg; z95 from Goodman's null,
    P(g2 > z) = (1-z)^(n_avg-1). With 50%-overlapped averages z95 is
    ~10% optimistic; treat marginal detections accordingly."""
    bias = 1.0 / n_avg
    z95 = 1.0 - 0.05 ** (1.0 / max(n_avg - 1, 1))
    return bias, z95


def band_mask(f, lo, hi):
    m = (f >= lo) & (f <= hi)
    for fline in KNOWN_LINES_HZ:
        m &= np.abs(f - fline) > 0.5   # coherent lines would inflate "common"
    return m


def drift_series(x, fs):
    # 1 s block means, then a 0.1 Hz low-pass: the drift band, cheaply
    n = int(fs)
    nb = x.size // n
    y = x[:nb * n].reshape(nb, n).mean(axis=1)
    sos = butter(3, 0.1, btype="low", fs=1.0, output="sos")
    return sosfiltfilt(sos, y)


def settle_trim_s(x, fs):
    """Seconds to cut from the start of this channel. Baseline SETTLE_S;
    extended when a settling exponential is still visible (each head can
    carry its own Field-Zero transient). Fitted on 1 s means."""
    n = int(fs)
    span = int(min(900.0, x.size / fs / 2.0))
    y = x[:span * n].reshape(span, n).mean(axis=1)
    t = np.arange(span) + 0.5

    def model(tt, A, tau, c):
        return A * np.exp(-tt / tau) + c

    try:
        popt, _ = curve_fit(model, t, y, p0=[y[0] - y[-1], 185.0, y[-1]],
                            maxfev=20000)
    except Exception:
        return SETTLE_S
    A, tau, c = popt
    resid = float(np.std(y - model(t, *popt)))
    if abs(A) < 5 * resid or not (10.0 < tau < 600.0):
        return SETTLE_S
    t_ok = tau * np.log(abs(A) / max(resid, 1e-12))
    return float(np.clip(t_ok, SETTLE_S, 1200.0))


# ---------------------------------------------------------------- analysis

@dataclass
class PairAnalysis:
    a_name: str
    b_name: str
    axis: str
    blockmean_r_meta: float | None
    sign_flipped: bool
    drift: dict
    white: dict
    grad: dict | None
    verdict_drift: str
    verdict_white: str


def band_stats(f, p11, p22, p12, gamma2, n_avg, lo, hi):
    """Coherence, phase, amplitude ratio and the decomposition medians
    over one band (line bins excluded), all in fT/rtHz."""
    m = band_mask(f, lo, hi)
    require(m.any(), f"no spectral bins in {lo}-{hi} Hz")
    bias, z95 = coh_thresholds(n_avg)
    g2 = float(np.median(gamma2[m]))
    # per-bin coherence variance (Carter), tightened by the roughly
    # independent half of the band bins
    g2_err = float(np.sqrt(2.0 * g2 * (1.0 - g2) ** 2 / n_avg)
                   / np.sqrt(max(m.sum() / 2.0, 1.0)))
    phase = float(np.degrees(np.angle(np.sum(p12[m]))))
    ratio = float(np.median(np.sqrt(p22[m] / p11[m])))
    ap12 = np.abs(p12[m])
    common = np.sqrt(ap12)
    common_bc = np.sqrt(np.clip(ap12 - np.sqrt(bias) * np.sqrt(p11[m] * p22[m]),
                                0.0, None))
    ul = np.sqrt(np.sqrt(z95) * np.sqrt(p11[m] * p22[m]))
    a_res = np.sqrt(np.clip(p11[m] - ap12, 0.0, None))
    b_res = np.sqrt(np.clip(p22[m] - ap12, 0.0, None))
    to_ft = 1e3   # series are pT, so sqrt(PSD) is pT/rtHz
    return {"gamma2": g2, "gamma2_err": g2_err, "bias": bias, "z95": z95,
            "phase_deg": phase, "ratio": ratio,
            "common_ft": float(np.median(common)) * to_ft,
            "common_bc_ft": float(np.median(common_bc)) * to_ft,
            "common_ul_ft": float(np.median(ul)) * to_ft,
            "a_ft": float(np.median(a_res)) * to_ft,
            "b_ft": float(np.median(b_res)) * to_ft}


def drift_verdict(d):
    # registered decision order (MATH_METHODS): detection first, then the
    # uniform-field signatures phase ~ 0 and coupling ratio 1 +/- 0.05
    if d["gamma2"] <= d["z95"]:
        return (f"coherence {d['gamma2']:.2f} <= z95 {d['z95']:.2f} -> no "
                "detected common field: per-head noise (S) or short-"
                "correlation near-field -- discriminate with head-spacing "
                "and bench-reference runs")
    if abs(d["phase_deg"]) >= 10.0:
        return (f"detected but phase {d['phase_deg']:+.0f} deg -> "
                "non-uniform field (propagation or gradient structure): "
                "M-type evidence, not a uniform external leak")
    if abs(d["ratio"] - 1.0) > 0.05:
        return (f"detected, in phase, but coupling ratio {d['ratio']:.2f} "
                "outside 1.00 +/- 0.05 -> position-dependent coupling (M) "
                "or a residual gain systematic (check the tone budget)")
    return ("environmental field (or building-scale common-mode); "
            "sensor-internal causes are excluded by the independent heads "
            "-- except a room-temperature common-mode on both cells, "
            "which needs a temperature-log correlation to close")


def white_verdict(w):
    if w["gamma2"] > w["z95"]:
        w["detected"] = True
        return (f"common floor MEASURED: {w['common_bc_ft']:.1f} fT/rtHz "
                f"(bias-corrected; plain {w['common_ft']:.1f})")
    w["detected"] = False
    return (f"no common floor detected (gamma2 {w['gamma2']:.3f} within the "
            f"null z95 {w['z95']:.3f}); 95% upper limit "
            f"{w['common_ul_ft']:.1f} fT/rtHz")


def analyze_pair(a_name, b_name, axis, x1, x2, fs, blockmean_r, args):
    da, db = drift_series(x1, fs), drift_series(x2, fs)
    r = float(np.corrcoef(da, db)[0, 1])
    # orientation is pre-registered geometry, never inferred from the data:
    # a data-driven flip would erase a genuine 180-degree (anti-correlated)
    # field, which is exactly the near-field/gradient evidence the M/E
    # decision rides on
    sign_flipped = bool(args.invert_b)
    if sign_flipped:
        x2 = -x2
        print(f"[prep] {a_name} x {b_name}: head b inverted by pre-registered "
              "mounting geometry (--invert-b)")
    elif r < 0:
        print(f"[prep] {a_name} x {b_name}: drift correlation {r:+.2f} is "
              "negative -- NOT auto-corrected. If the mounting is known "
              "anti-parallel rerun with --invert-b; otherwise this is physics "
              "(anti-correlated field), kept for the phase diagnosis")

    # one segmentation for ALL quoted statistics, so n_avg is the same in
    # both bands; 200 s is the registered LF convention (MATH_METHODS)
    f, p11, p22, p12, gamma2, n_avg = cross_set(x1, x2, fs, args.seg_s)
    require(n_avg >= 3, f"only {n_avg} Welch segments at --seg-s "
            f"{args.seg_s:g}; record longer or lower --seg-s")
    if n_avg < 8:
        print(f"[warn] n_avg={n_avg} at --seg-s {args.seg_s:g}: coherence "
              "thresholds are weak; the registered design wants >= 14 "
              "(2000-4000 s of settled data)")
    drift = band_stats(f, p11, p22, p12, gamma2, n_avg, *args.drift_band)
    white = band_stats(f, p11, p22, p12, gamma2, n_avg, *args.white_band)
    # the registered headline is the 0.05 Hz point (gates G4); keep it
    # alongside the band medians
    i0 = int(np.argmin(np.abs(f - 0.05)))
    drift["asd005_a_ft"] = float(np.sqrt(p11[i0])) * 1e3
    drift["asd005_b_ft"] = float(np.sqrt(p22[i0])) * 1e3
    drift["asd005_f_hz"] = float(f[i0])

    grad = None
    if args.separation_m:
        L = args.separation_m
        g = (x1 - x2) / L
        fg, pg = welch(g, fs=fs, nperseg=min(len(g), int(20.0 * fs)),
                       detrend="linear")
        mg = band_mask(fg, *args.white_band)
        fw, pw1 = welch(x1, fs=fs, nperseg=min(len(x1), int(20.0 * fs)),
                        detrend="linear")
        _, pw2 = welch(x2, fs=fs, nperseg=min(len(x2), int(20.0 * fs)),
                       detrend="linear")
        mw = band_mask(fw, *args.white_band)
        grad = {"floor_pt_per_m": float(np.median(np.sqrt(pg[mg]))),
                "ideal_pt_per_m": float(np.median(
                    np.sqrt(pw1[mw] + pw2[mw])) / L),
                "dc_pt_per_m": float(np.mean(g)),
                "dc_note": "DC difference contains both sensor zero offsets; "
                           "a gradient claim needs cal_flip or map_residual"}

    pair = PairAnalysis(
        a_name=a_name, b_name=b_name, axis=axis,
        blockmean_r_meta=blockmean_r, sign_flipped=sign_flipped,
        drift=drift, white=white, grad=grad,
        verdict_drift=drift_verdict(drift), verdict_white=white_verdict(white),
    )
    spectra = {"f": f, "p11": p11, "p22": p22, "gamma2": gamma2,
               "common": np.sqrt(np.abs(p12)), "n_avg": n_avg}
    return pair, spectra


def cross_axis_table(cols_pt, pair_channels, fs, drift_band, seg_s):
    """Drift-band coherence of every non-pair channel against each pair
    channel -- secondary evidence: a real diagonal field drift shows
    across axes, per-head noise does not."""
    out = []
    others = [c for c in cols_pt if c not in pair_channels]
    for o in others:
        for p in pair_channels:
            f, _, _, _, g2, n_avg = cross_set(cols_pt[o], cols_pt[p], fs, seg_s)
            m = band_mask(f, *drift_band)
            out.append({"a": o, "b": p, "band": "drift",
                        "gamma2": float(np.median(g2[m]))})
    return out


# ---------------------------------------------------------------- figures

def make_figures(outdir, pair, spectra, args):
    import matplotlib
    matplotlib.use("Agg")
    import matplotlib.pyplot as plt
    plt.rcParams.update({
        "figure.dpi": 150, "font.size": 11, "axes.titleweight": "bold",
        "axes.spines.top": False, "axes.spines.right": False,
        "axes.edgecolor": C_MUTE, "text.color": C_INK, "legend.frameon": False,
    })
    f = spectra["f"]
    m = f > 0
    bias, z95 = coh_thresholds(spectra["n_avg"])

    fig, ax = plt.subplots(figsize=(7.5, 4.2))
    ax.semilogx(f[m], spectra["gamma2"][m], lw=0.8, color=C_A)
    ax.axhline(bias, color=C_MUTE, ls=":", lw=1.0)
    ax.axhline(z95, color=C_COM, ls="--", lw=1.0)
    ax.axhspan(0, z95, color="#E6E8EA", alpha=0.6, zorder=0)
    ax.annotate(f"z95 = {z95:.3f}", (f[m][2], z95), va="bottom", fontsize=9,
                color=C_COM)
    ax.annotate(f"bias = {bias:.3f}", (f[m][2], bias), va="bottom", fontsize=9,
                color=C_MUTE)
    ax.set_xlabel("frequency [Hz]")
    ax.set_ylabel("coherence $\\gamma^2$")
    ax.set_ylim(0, 1.02)
    ax.set_title(f"{pair.a_name} x {pair.b_name}: below the shading is "
                 "indistinguishable from zero")
    fig.savefig(os.path.join(outdir, "cross_coherence.png"), bbox_inches="tight")
    plt.close(fig)

    fig, ax = plt.subplots(figsize=(7.5, 4.5))
    for key, color, label in (("p11", C_A, f"head a ({pair.a_name})"),
                              ("p22", C_B, f"head b ({pair.b_name})")):
        ax.loglog(f[m], np.sqrt(spectra[key][m]) * 1e3, lw=0.8, color=color,
                  label=label)
    ax.loglog(f[m], spectra["common"][m] * 1e3, lw=1.0, color=C_COM,
              label="common (sqrt|P12|)")
    ax.set_xlabel("frequency [Hz]")
    ax.set_ylabel("ASD [fT/rtHz]")
    ax.set_title("two-head decomposition")
    ax.legend(fontsize=9)
    fig.savefig(os.path.join(outdir, "cross_decomposition.png"),
                bbox_inches="tight")
    plt.close(fig)

    if pair.grad:
        L = args.separation_m
        fig, ax = plt.subplots(figsize=(7.0, 4.2))
        ax.axhline(pair.grad["floor_pt_per_m"], color=C_A, lw=1.2,
                   label=f"difference floor {pair.grad['floor_pt_per_m']:.2f}")
        ax.axhline(pair.grad["ideal_pt_per_m"], color=C_MUTE, ls="--", lw=1.2,
                   label=f"ideal sqrt(P11+P22)/L {pair.grad['ideal_pt_per_m']:.2f}")
        ax.set_ylabel("gradient ASD [pT/m/rtHz]")
        ax.set_title(f"gradiometer view, L = {L * 1e3:.0f} mm")
        ax.set_xticks([])
        ax.legend(fontsize=9)
        fig.savefig(os.path.join(outdir, "cross_gradiometer.png"),
                    bbox_inches="tight")
        plt.close(fig)


# ---------------------------------------------------------------- pipeline

def run_analysis(args):
    require(len(args.run_dirs) == 1,
            "give exactly one two-head run: two separate run dirs cannot "
            "share a DAQ clock, and clock sharing is what the method rides on")
    run_dir = args.run_dirs[0]
    meta, session = find_meta(run_dir)
    check_run(meta, args.force, args.allow_sim)
    fs = float(meta["fs_hz"])

    if args.pair:
        a_name, b_name = (s.strip() for s in args.pair.split(","))
        pair_rows = [{"axis": a_name, "second": b_name, "blockmean_r": None}]
    else:
        # the two-head recorder was its own program (qzfm_3axis.py) until
        # 2026-08-10 and is now qzfm_record.py --2head, which stamps the
        # old name in software_legacy. Both are the same instrument and
        # the same meta shape, so both are accepted -- gating on the new
        # name alone would have made every run before the merge
        # unreadable, and on the old name alone, every run after it.
        wrote = {meta.get("software"), meta.get("software_legacy")}
        require("qzfm_3axis.py" in wrote,
                f"meta software={meta.get('software')!r}; this tool needs a "
                "two-head run (qzfm_record.py --2head, or the older "
                "qzfm_3axis.py) -- or --pair for a generic 2-column raw.csv")
        pair_rows = meta.get("pairs") or []
        require(pair_rows, "meta lists no shared pairs and no --pair given")

    raw = os.path.join(run_dir, f"{session}_raw.csv")
    require(os.path.exists(raw), f"missing {raw}")
    cols_v = load_raw_csv(raw)

    heads = meta.get("heads") or {}
    gain_b = args.gain_b if args.gain_b is not None else args.gain_a
    if args.gain_b is None:
        print(f"[warn] --gain-b not given, using head a's {args.gain_a:g} V/nT; "
              "decomposition ASDs carry the true gain difference (~%), "
              "coherence and the verdicts do not")

    def gain_of(ch, fallback):
        head = heads.get(ch)
        if head == "a":
            return args.gain_a
        if head == "b":
            return gain_b
        return fallback

    # per-channel Calibrate factors: recorded and surfaced, deliberately NOT
    # folded into the conversion. The factor is defined as a multiplier that
    # rescales the raw response to nominal, and whether the ANALOG tap is
    # pre- or post-Calibrate is an open hardware question (qzfm-sensor-specs
    # section 8) -- so the correct operation is "multiply" or "nothing", and
    # the choice is settled by the injected-tone check, not assumed here.
    # The base head gains are already per-head via --gain-a/--gain-b.
    cal = meta.get("cal_factors") or {}
    if cal:
        print("[note] Calibrate factors in meta (recorded, not applied): "
              + ", ".join(f"{k}:{v:g}" for k, v in sorted(cal.items()))
              + " -- bound the ratio gate by their head-to-head difference")
    else:
        print("[warn] no cal_factors in meta -- the amplitude-ratio budget "
              "cannot bound the per-head Calibrate systematic; record "
              "factors with --cal-factors at acquisition next time")

    head_ids = meta.get("head_ids") or {}
    sep = f"{args.separation_m * 1e3:.0f} mm" if args.separation_m else "unknown"
    print(f"=== cross_spectrum: {run_dir} (heads a={head_ids.get('a', '?')}, "
          f"b={head_ids.get('b', '?')}, separation {sep}) ===")
    if not args.separation_m:
        print("[note] no --separation-m: gradiometer view skipped; the "
              "field-vs-gradient reading of any phase offset is weaker")

    pairs, spectra_list = [], []
    trims = {}
    for row in pair_rows:
        a_name, b_name = row["axis"], row["second"]
        require(a_name in cols_v and b_name in cols_v,
                f"pair {a_name},{b_name} not found in {raw} "
                f"(columns: {sorted(cols_v)})")
        x1 = cols_v[a_name] * 1000.0 / gain_of(a_name, args.gain_a)
        x2 = cols_v[b_name] * 1000.0 / gain_of(b_name, gain_b)

        # trim at the later of the two heads' settle points
        ta, tb = settle_trim_s(x1, fs), settle_trim_s(x2, fs)
        trim = max(ta, tb)
        trim_ch = a_name if ta >= tb else b_name
        usable = x1.size / fs - trim
        require(usable >= 300.0, f"only {usable:.0f} s left after the "
                                 f"{trim:.0f} s settle trim -- record longer")
        n0 = int(trim * fs)
        trims[f"{a_name}x{b_name}"] = {"trim_s": round(trim, 1),
                                       "set_by": trim_ch}
        print(f"[prep] shared pair {a_name} x {b_name}   meta blockmean_r "
              f"{row.get('blockmean_r')}   settle trim {trim:.0f} s "
              f"(set by {trim_ch})")

        pair, spectra = analyze_pair(a_name, b_name, row["axis"],
                                     x1[n0:], x2[n0:], fs,
                                     row.get("blockmean_r"), args)
        pairs.append(pair)
        spectra_list.append(spectra)

        d, w = pair.drift, pair.white
        print(f"[drift {args.drift_band[0]:g}-{args.drift_band[1]:g} Hz] "
              f"gamma2 {d['gamma2']:.2f} (bias {d['bias']:.3f}, z95 "
              f"{d['z95']:.3f})  phase {d['phase_deg']:+.0f} deg  ratio "
              f"{d['ratio']:.2f}")
        print(f"   common {d['common_ft']:.0f} fT/rtHz, per-head a "
              f"{d['a_ft']:.0f} / b {d['b_ft']:.0f}")
        print(f"   -> VERDICT: {pair.verdict_drift}")
        print(f"[white {args.white_band[0]:g}-{args.white_band[1]:g} Hz]     "
              f"gamma2 {w['gamma2']:.3f} (bias {w['bias']:.3f}, z95 "
              f"{w['z95']:.3f})")
        print(f"   -> {pair.verdict_white}")
        print(f"   per-head floors a {w['a_ft']:.1f} / b {w['b_ft']:.1f} fT/rtHz "
              f"(spec {QZFM_SPEC_FT[0]:g}-{QZFM_SPEC_FT[1]:g}, single-head "
              f"measurement {FLOOR_SINGLE_FT:g})")
        if pair.grad:
            g = pair.grad
            ok = "CMRR ok" if g["floor_pt_per_m"] <= 1.25 * g["ideal_pt_per_m"] \
                 else "EXCESS common-mode leakage"
            print(f"[grad] difference floor {g['floor_pt_per_m']:.2f} pT/m/rtHz"
                  f"  vs ideal {g['ideal_pt_per_m']:.2f} ({ok})")

    pair_channels = sorted({p.a_name for p in pairs} | {p.b_name for p in pairs})
    cols_pt = {c: cols_v[c] * 1000.0 / gain_of(c, args.gain_a)
               for c in cols_v}
    xax = cross_axis_table(cols_pt, pair_channels, fs, args.drift_band,
                           args.seg_s)
    for row in xax:
        print(f"[xax ] {row['a']} x {row['b']} drift coherence "
              f"{row['gamma2']:.2f}")

    os.makedirs(args.outdir, exist_ok=True)
    summary = {
        "program": "cross_spectrum.py",
        "generated": datetime.now().isoformat(timespec="seconds"),
        "inputs": {"runs": [run_dir],
                   "args": {k: v for k, v in vars(args).items()
                            if k not in ("run_dirs", "test")}},
        "results": {
            "heads": head_ids, "separation_m": args.separation_m,
            "cal_factors_recorded": cal or None,
            "settle": trims,
            "pairs": [asdict(p) for p in pairs],
            "cross_axis": xax,
        },
        "warnings": [],
        "pass": None,
    }
    path = os.path.join(args.outdir, "cross_spectrum_summary.json")
    with open(path, "w", newline="\n") as fh:
        json.dump(summary, fh, indent=1)
    print(f"[out ] {path}")

    if not args.no_figures:
        make_figures(args.outdir, pairs[0], spectra_list[0], args)
        print(f"[out ] figures in {args.outdir}")
    return summary


# ---------------------------------------------------------------- self-test

def synth_two_heads(fs, seconds, common_white_ft=0.0, head_ft=(20.0, 20.0),
                    flick_ft=10.0, alpha=0.9, mains_pt=0.05,
                    b_sign=+1.0, b_transient=None, diff_pt=0.0, rng=None):
    """Two heads on one clock, in pT: shared flicker drift + shared white
    common floor + independent per-head white noise. b_transient is
    (A_pT, tau_s); diff_pt splits +/-diff/2 between the heads.
    flick_ft is kept small enough that the drift does not leak into the
    3-40 Hz white band and contaminate the common-floor checks."""
    rng = rng if rng is not None else np.random.default_rng(SIM_SEED)
    n = int(fs * seconds)
    t = np.arange(n) / fs
    common = rng.normal(0.0, common_white_ft * 1e-3 * np.sqrt(fs / 2.0), n) \
        if common_white_ft > 0 else np.zeros(n)
    wn = rng.normal(0.0, 1.0, n)
    F = np.fft.rfft(wn)
    f = np.fft.rfftfreq(n, 1.0 / fs)
    shape = np.zeros_like(f)
    shape[1:] = (flick_ft * 1e-3) / f[1:] ** alpha
    common += np.fft.irfft(F * shape * np.sqrt(fs / 2.0), n)
    common += mains_pt * np.sin(2 * np.pi * 50.0 * t)
    n1 = rng.normal(0.0, head_ft[0] * 1e-3 * np.sqrt(fs / 2.0), n)
    n2 = rng.normal(0.0, head_ft[1] * 1e-3 * np.sqrt(fs / 2.0), n)
    x1 = common + n1 + diff_pt / 2.0
    x2 = b_sign * (common + n2 - diff_pt / 2.0)
    if b_transient:
        A, tau = b_transient
        x2 = x2 + A * np.exp(-t / tau)
    return x1, x2, common


def write_3axis_run(tmp, session, cols_pt, fs, pairs_meta,
                    heads=None, gain=GAIN_MEASURED, legacy_meta=False):
    """Two-head run dir: raw.csv (volts) + meta.json.

    Writes the CURRENT meta shape by default (software=qzfm_record.py
    plus software_legacy), and the pre-merge shape with
    legacy_meta=True. Both exist in result/, so the acceptance gate has
    two branches and the tests have to walk both -- until 2026-08-10
    every fixture here wrote the old name, so the branch that reads
    post-merge runs was never executed by a test.
    """
    run = os.path.join(tmp, session)
    os.makedirs(run)
    names = list(cols_pt)
    n = len(next(iter(cols_pt.values())))
    header = (f"# QZFM raw voltages | session={session} "
              f"start=2026-07-08T09:00:00 fs={fs:g}\n"
              "sample," + ",".join(f"V_{c}" for c in names))
    table = np.column_stack([np.arange(n),
                             *[cols_pt[c] * gain / 1000.0 for c in names]])
    np.savetxt(os.path.join(run, f"{session}_raw.csv"), table,
               fmt=["%d"] + ["%.6g"] * len(names), delimiter=",",
               header=header, comments="")
    heads = heads or {c: ("b" if c.endswith("2") else "a") for c in names}
    meta = {
        "start_iso": "2026-07-08T09:00:00", "session": session,
        "channels": {c: f"Dev2/ai{i}" for i, c in enumerate(names)},
        "heads": heads, "head_ids": {"a": "Z3D0-AB3Q", "b": "SIMB01"},
        "pairs": pairs_meta,
        "fs_hz": fs, "n_samples": n, "duration_s": n / fs,
        "clock_elapsed_s": n / fs, "fs_effective_hz": fs,
        "gain_mode": "1x", "cal_v_per_nT_used": gain,
        "cal_pT_per_V": 1000.0 / gain, "vrange": 1.0, "device": "Dev2",
        "rail_fraction": {c: 0.0 for c in names},
        "qzfm_rail_fraction": {c: 0.0 for c in names},
        "n_nonfinite": {c: 0 for c in names},
        "flat_ptp_V": {c: 0.05 for c in names},
        "mux_skew_note": "simulated", "source": "simulate",
        "status": "ok", "exit_code": 0,
        **({"software": "qzfm_3axis.py"} if legacy_meta else
           {"software": "qzfm_record.py", "topology": "2head",
            "software_legacy": "qzfm_3axis.py"}),
        "error": None, "warnings": [], "notes": "cross_spectrum --test",
    }
    with open(os.path.join(run, f"{session}.meta.json"), "w", newline="\n") as fh:
        json.dump(meta, fh, indent=1)
    return run


PAIR_Y = [{"axis": "y", "second": "y2", "heads": ["a", "b"],
           "n_blocks": 100, "blockmean_r": 0.94}]


def targs(tmp, extra):
    # the synthetic records are 300 s, so the tests run the same machinery
    # at --seg-s 100; the production default stays at the registered 200 s
    argv = [*extra, "--allow-sim", "--no-figures", "--seg-s", "100",
            "--outdir", os.path.join(tmp, "out")]
    return build_parser().parse_args(argv)


def test_measured_common(ctx):
    rng = np.random.default_rng(SIM_SEED)
    x1, x2, common = synth_two_heads(300.0, 2400.0, common_white_ft=15.0,
                                     head_ft=(20.0, 20.0), rng=rng)
    z = common + rng.normal(0.0, 0.02, x1.size)   # head-a channel, same drift
    run = write_3axis_run(ctx["tmp"], "t1", {"y": x1, "y2": x2, "z": z}, 300.0,
                          PAIR_Y, heads={"y": "a", "y2": "b", "z": "a"})
    s = run_analysis(targs(ctx["tmp"], [run, "--gain-b", str(GAIN_MEASURED)]))
    p = s["results"]["pairs"][0]
    assert p["white"]["detected"], f"common floor not detected: {p['white']}"
    err = abs(p["white"]["common_ft"] - 15.0) / 15.0
    assert err <= 0.20, f"common floor off by {err * 100:.0f}%"
    assert "environmental field" in p["verdict_drift"], p["verdict_drift"]
    assert abs(p["drift"]["phase_deg"]) < 10 and 0.8 < p["drift"]["ratio"] < 1.25
    ctx["t1"] = s


def test_upper_limit(ctx):
    rng = np.random.default_rng(SIM_SEED + 1)
    x1, x2, _ = synth_two_heads(300.0, 2400.0, common_white_ft=0.0,
                                head_ft=(20.0, 20.0), rng=rng)
    run = write_3axis_run(ctx["tmp"], "t2", {"y": x1, "y2": x2}, 300.0, PAIR_Y)
    s = run_analysis(targs(ctx["tmp"], [run, "--gain-b", str(GAIN_MEASURED)]))
    w = s["results"]["pairs"][0]["white"]
    assert not w["detected"], f"false common-floor detection: {w}"
    assert 5.0 < w["common_ul_ft"] < 15.0, f"upper limit {w['common_ul_ft']}"


def test_sign(ctx):
    # anti-correlated data must NOT be silently "corrected": without the
    # pre-registered flag the 180-degree signature survives to the verdict;
    # with --invert-b (declared mounting) it reads as a uniform field
    rng = np.random.default_rng(SIM_SEED + 2)
    x1, x2, _ = synth_two_heads(300.0, 2400.0, common_white_ft=15.0,
                                head_ft=(20.0, 20.0), b_sign=-1.0, rng=rng)
    run = write_3axis_run(ctx["tmp"], "t3", {"y": x1, "y2": x2}, 300.0, PAIR_Y)
    s = run_analysis(targs(ctx["tmp"], [run, "--gain-b", str(GAIN_MEASURED)]))
    p = s["results"]["pairs"][0]
    assert not p["sign_flipped"], "sign must never flip without --invert-b"
    assert "environmental field" not in p["verdict_drift"], p["verdict_drift"]
    s = run_analysis(targs(ctx["tmp"], [run, "--gain-b", str(GAIN_MEASURED),
                                        "--invert-b"]))
    p = s["results"]["pairs"][0]
    assert p["sign_flipped"], "--invert-b not honoured"
    assert "environmental field" in p["verdict_drift"], p["verdict_drift"]


def test_settle(ctx):
    rng = np.random.default_rng(SIM_SEED + 3)
    x1, x2, _ = synth_two_heads(300.0, 2400.0, common_white_ft=15.0,
                                head_ft=(20.0, 20.0),
                                b_transient=(20.0, 150.0), rng=rng)
    run = write_3axis_run(ctx["tmp"], "t4", {"y": x1, "y2": x2}, 300.0, PAIR_Y)
    s = run_analysis(targs(ctx["tmp"], [run, "--gain-b", str(GAIN_MEASURED)]))
    trim = s["results"]["settle"]["yxy2"]
    assert trim["trim_s"] > SETTLE_S, f"transient did not extend the trim: {trim}"
    assert trim["set_by"] == "y2", trim


def test_gradiometer(ctx):
    rng = np.random.default_rng(SIM_SEED + 4)
    L = 0.08
    x1, x2, _ = synth_two_heads(300.0, 2400.0, common_white_ft=0.0,
                                head_ft=(20.0, 20.0),
                                diff_pt=30.0 * L, rng=rng)
    run = write_3axis_run(ctx["tmp"], "t5", {"y": x1, "y2": x2}, 300.0, PAIR_Y)
    s = run_analysis(targs(ctx["tmp"], [run, "--gain-b", str(GAIN_MEASURED),
                                        "--separation-m", str(L)]))
    g = s["results"]["pairs"][0]["grad"]
    assert abs(g["dc_pt_per_m"] - 30.0) / 30.0 <= 0.10, g
    assert abs(g["floor_pt_per_m"] - g["ideal_pt_per_m"]) \
        / g["ideal_pt_per_m"] <= 0.25, g


def test_refusals(ctx):
    run = os.path.join(ctx["tmp"], "t1")     # reuse the first synthetic run
    try:
        run_analysis(targs(ctx["tmp"], [run, run]))
    except SystemExit as e:
        assert e.code == 13, f"two run dirs exited {e.code}, want 13"
    else:
        raise AssertionError("two run dirs must be refused (one DAQ clock)")
    try:
        run_analysis(targs(ctx["tmp"], [run, "--pair", "y,nosuch"]))
    except SystemExit as e:
        assert e.code == 13, f"missing pair channel exited {e.code}, want 13"
    else:
        raise AssertionError("a pair channel missing from raw.csv must be refused")


def test_both_meta_generations_are_read(ctx):
    """A pre-merge run and a post-merge run of the same instrument must
    both be accepted. The gate reads software OR software_legacy; before
    2026-08-10 every fixture wrote the old name, so the post-merge
    branch shipped untested and a narrowed gate would have refused every
    new run without a single test going red."""
    rng = np.random.default_rng(4242)
    x1, x2, _ = synth_two_heads(300.0, 2400.0, common_white_ft=15.0,
                                head_ft=(20.0, 20.0), rng=rng)
    for name, legacy in (("gen_new", False), ("gen_old", True)):
        run = write_3axis_run(ctx["tmp"], name, {"y": x1, "y2": x2}, 300.0,
                              PAIR_Y, legacy_meta=legacy)
        with open(glob.glob(os.path.join(run, "*.meta.json"))[0],
                  encoding="utf-8") as fh:
            meta = json.load(fh)
        assert (meta["software"] == "qzfm_3axis.py") is legacy, meta
        s = run_analysis(targs(ctx["tmp"], [run]))
        assert s["results"]["pairs"], f"{name}: gate refused a valid run"


def run_self_test():
    import tempfile
    tmp = tempfile.mkdtemp(prefix="cross_spectrum_test_")
    print("=== cross_spectrum --test: synthetic checks (temp dir, removed after) ===")
    ctx = {"tmp": tmp}
    try:
        for test in (test_measured_common, test_upper_limit, test_sign,
                     test_settle, test_gradiometer, test_refusals,
                     test_both_meta_generations_are_read):
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
                   help="one qzfm_record.py --2head run (two heads, one clock)")
    p.add_argument("--pair", default=None, metavar="CH_A,CH_B",
                   help="explicit channel pair for a generic 2-column raw.csv")
    p.add_argument("--gain-a", type=float, default=GAIN_MEASURED,
                   help="head a gain in V/nT (default 2.624)")
    p.add_argument("--gain-b", type=float, default=None,
                   help="head b gain in V/nT (defaults to --gain-a, with a warning)")
    p.add_argument("--separation-m", type=float, default=None,
                   help="head spacing in m (enables the gradiometer view)")
    p.add_argument("--invert-b", action="store_true",
                   help="invert head b's sign from PRE-REGISTERED mounting "
                        "geometry (never inferred from the data)")
    p.add_argument("--seg-s", type=float, default=200.0,
                   help="Welch segment length in s for the quoted statistics "
                        "(200 s is the registered LF convention)")
    p.add_argument("--drift-band", nargs=2, type=float, default=[0.02, 0.2],
                   metavar=("LO", "HI"), help="common-drift band in Hz")
    p.add_argument("--white-band", nargs=2, type=float, default=[3.0, 40.0],
                   metavar=("LO", "HI"), help="white-floor band in Hz")
    p.add_argument("--outdir", default=os.path.dirname(os.path.abspath(__file__)),
                   help="where the summary and figures go")
    p.add_argument("--force", action="store_true",
                   help="analyze runs whose status is not ok")
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
    require(args.run_dirs, "no run directory given (see --help)")
    for name, g in (("--gain-a", args.gain_a), ("--gain-b", args.gain_b)):
        if g is not None:
            require(1.5 <= g <= 4.0, f"{name} {g} implausible at 1x")
    if args.separation_m is not None:
        require(args.separation_m > 0, "--separation-m must be positive")
    run_analysis(args)
    return 0


if __name__ == "__main__":
    sys.exit(main())
