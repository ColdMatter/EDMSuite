#!/usr/bin/env python3
"""Predicted low-frequency noise budget inside the Twinleaf MS-1L.

The bench baseline (closed_long, 2026-06-30) shows a 1/f rise reaching
~500-600 fT/rtHz at 0.05 Hz, 10x the lattice budget. This script predicts
the 0.05 Hz ASD inside the MS-1L for three possible origins of that excess:
E = external field leaking through the bench shield, M = bench-shield
magnetization drift, S = sensor-intrinsic 1/f. The model is a quadrature
sum of the suppressed leak, the MS-1L's own magnetization noise (knee from
Lee & Romalis 2008, Eq. 9-10; wall thickness unpublished, so a band) and
the sensor noise. Full write-up and caveats: PREDICTION.md next to this.

Refuses to write outputs if inputs are missing or a number looks unphysical.
Run:  python analysis/prediction_B2/predict_ms1l_noise_budget.py
Outputs (same directory): fig_b2_ms1l_prediction.png, b2_summary.md, b2_summary.json
"""
from __future__ import annotations

import json
import os

import numpy as np
import matplotlib
matplotlib.use("Agg")
import matplotlib.pyplot as plt
from scipy.signal import welch

HERE = os.path.dirname(os.path.abspath(__file__))            # analysis/prediction_B2
import sys as _sys  # analysis_scripts/ on sys.path for _paths
_sys.path.insert(0, os.path.dirname(os.path.dirname(os.path.abspath(__file__))))
from _paths import DATA_ROOT, PROJECT_DIR, ANALYSIS_DIR, data, data_join, rel_data  # noqa: E402,F401
DATA = DATA_ROOT
BENCH_LONG = os.path.join(DATA, "gui_baseline", "2026-06-30_closed-shield", "closed_long_Z_pT.txt")
# INVALIDATED 2026-07-06 (result/README.md): that day was wired with
# AI- unconnected, so every analog number from it -- this 0.18 pT
# floor included -- carries the USB-6008 divider potential. The
# scenario bands do not use it; the grey curve and white_daq do,
# and are labelled as invalid rather than removed, because this
# figure is the pre-registration frozen on 2026-07-04 and its
# contents are evidence about what was predicted, not a current
# measurement (2026-08-19 analysis review, H6). Replace with a
# post-repair shorted floor when one exists.
DAQ_OFF = os.path.join(DATA, "failed_analog", "2026-07-01_closed-shield-3axis",
                       "closed_qzfmOFF-1V-noisefloor_Z_pT.txt")

FS_BENCH = 203.0        # GUI digital record, measured (nominal 200) [analysis/README.md]
FS_DAQ = 1000.0         # USB-6008 run [meta.json]

# --- constants of the budget (units: pT/sqrt(Hz) internally, pT everywhere) ---
MU0 = 4e-7 * np.pi
TARGET = 0.050          # lattice budget, 50 fT/rtHz [Lim LOI / Collings 2025]
F_EDM = 0.05            # reference low frequency (E-reversal band placeholder)
EDM_BAND = (0.01, 1.0)  # E/B reversal band that sets the eEDM sensitivity
EDM_FILL = "#ECE7DD"    # muted warm-neutral wash, kept understated

SENSOR_LO, SENSOR_TYP, SENSOR_HI = 0.007, 0.010, 0.015  # QZFM dual spec, 3-100 Hz
MS1L_WHITE = 0.016      # MS-1L shield noise spec [Twinleaf product page]
S_MS1L_NOM, S_MS1L_DERATED = 1e6, 3e5   # spec DC; derated for small-signal mu droop
S_BENCH_LO, S_BENCH_CENTRAL = 3e3, 1e5  # Collings axial / S_zz bracket (unverified)

MU_R, SIGMA, TAN_DELTA = 25e3, 1.6e6, 0.04  # mu-metal typicals [Lee&Romalis 2008]
T_WALL_CENTRAL, T_WALL_LO, T_WALL_HI = 1.0e-3, 1.5e-3, 0.5e-3  # m (unpublished)

BENCH_WHITE = 0.0115    # measured bench-shield white floor, pT/rtHz (sensor-limited)


def require(ok, msg):
    """Physics/data sanity gate: stop rather than plot a wrong number."""
    if not ok:
        raise SystemExit(f"[error] {msg}")


def f_magn(t_wall, mu_r=MU_R, sigma=SIGMA, tan_delta=TAN_DELTA):
    """Magnetization-noise knee frequency, Lee & Romalis (2008) Eq. 10 [Hz]."""
    return 3.0 * tan_delta / (2.0 * np.pi * MU0 * mu_r * sigma * t_wall ** 2)


def shield_asd(f, fm):
    """MS-1L intrinsic noise: white eddy floor + 1/f magnetization [pT/rtHz]."""
    return MS1L_WHITE * np.sqrt(1.0 + fm / np.asarray(f))


def load_pT(path):
    """One column of numbers from a QZFM text export, validated."""
    require(os.path.exists(path),
            f"missing data file: {path} (copy the measurement into this folder first)")
    rows, bad = [], 0
    with open(path, "r") as fh:
        for line in fh:
            line = line.strip()
            if not line or line.startswith(("#", "%", "//")):
                continue
            try:
                rows.append(float(line.replace(",", " ").split()[0]))
            except ValueError:
                bad += 1  # e.g. the GUI "Mag#1: ..." header line
    require(rows, f"{path}: no numeric samples")
    require(bad <= 16, f"{path}: {bad} unparsable lines -- corrupt file?")
    x = np.asarray(rows, dtype=float)
    require(np.isfinite(x).all(), f"{path}: contains NaN/inf")
    return x


def welch_asd(x, fs, nperseg):
    """Amplitude spectral density by Welch, segments linearly detrended."""
    f, pxx = welch(x, fs=fs, nperseg=min(len(x), nperseg), detrend="linear")
    a = np.sqrt(pxx)
    require(np.isfinite(a).all(), "ASD came out non-finite -- corrupt input?")
    return f, a


def asd_two_res(x, fs, nper_low, nper_high, f_split):
    """Welch ASD merged from a long-segment (low f) and short-segment (high f) pass."""
    f_lo, a_lo = welch_asd(x, fs, nper_low)
    f_hi, a_hi = welch_asd(x, fs, nper_high)
    keep_lo = (f_lo > 0) & (f_lo < f_split)
    keep_hi = f_hi >= f_split
    f = np.concatenate([f_lo[keep_lo], f_hi[keep_hi]])
    a = np.concatenate([a_lo[keep_lo], a_hi[keep_hi]])
    return f, a


def at(f, a, f0):
    """ASD interpolated at f0 on log-log axes."""
    return float(np.exp(np.interp(np.log(f0), np.log(f), np.log(a))))


def quad(*terms):
    return float(np.sqrt(sum(t ** 2 for t in terms)))


def main():
    bench = load_pT(BENCH_LONG)
    daq = load_pT(DAQ_OFF)
    require(bench.size >= 65536,
            f"bench record has {bench.size} samples; need >=65536 (~5 min at "
            f"{FS_BENCH:g} Hz) to resolve 0.05 Hz")
    f_b, a_b = asd_two_res(bench, FS_BENCH, 65536, 4096, 0.7)
    f_d, a_d = asd_two_res(daq, FS_DAQ, 16384, 4096, 0.25)

    # the excess subtraction below is only meaningful if BENCH_WHITE still
    # matches the data; a mismatch means the constant is stale
    med_bench = float(np.median(a_b[(f_b >= 3) & (f_b <= 10)]))
    require(0.5 * BENCH_WHITE <= med_bench <= 2.0 * BENCH_WHITE,
            f"measured bench floor {med_bench * 1e3:.1f} fT/rtHz disagrees with "
            f"BENCH_WHITE={BENCH_WHITE * 1e3:.1f} -- update the constant first")

    fm_c, fm_lo, fm_hi = (f_magn(T_WALL_CENTRAL), f_magn(T_WALL_LO), f_magn(T_WALL_HI))
    require(all(0.001 <= v <= 100 for v in (fm_c, fm_lo, fm_hi)),
            f"f_magn = {fm_lo:.3g}/{fm_c:.3g}/{fm_hi:.3g} Hz is outside "
            f"[0.001, 100] -- check the mu-metal parameters")

    # bench excess over its sensor-limited white floor (the thing that must be
    # explained); bins at/below the floor clip to zero excess by construction
    a_excess = np.sqrt(np.clip(a_b ** 2 - BENCH_WHITE ** 2, 0.0, None))

    f_ref = [0.02, 0.05, 0.1, 0.3, 1.0, 3.0, 10.0]
    rows, model = [], {}
    for f0 in f_ref:
        meas = at(f_b, a_b, f0)
        excess = float(np.sqrt(max(meas ** 2 - BENCH_WHITE ** 2, 0.0)))
        sh_c, sh_lo, sh_hi = (float(shield_asd(f0, fm)) for fm in (fm_c, fm_lo, fm_hi))
        # scenario E leak central / best / worst
        leak_c = excess * S_BENCH_CENTRAL / S_MS1L_NOM
        leak_best = excess * S_BENCH_LO / S_MS1L_NOM
        leak_worst = excess * S_BENCH_CENTRAL / S_MS1L_DERATED
        tot_E = quad(leak_c, sh_c, SENSOR_TYP)
        tot_E_lo = quad(leak_best, sh_lo, SENSOR_LO)
        tot_E_hi = quad(leak_worst, sh_hi, SENSOR_HI)
        tot_M = quad(sh_c, SENSOR_TYP)
        tot_M_lo, tot_M_hi = quad(sh_lo, SENSOR_LO), quad(sh_hi, SENSOR_HI)
        rows.append(dict(f=f0, meas=meas, excess=excess,
                         shield=(sh_lo, sh_c, sh_hi),
                         leak=(leak_best, leak_c, leak_worst),
                         E=(tot_E_lo, tot_E, tot_E_hi),
                         M=(tot_M_lo, tot_M, tot_M_hi),
                         # Scenario S is the bench spectrum unchanged:
                         # "the 1/f is the sensor, so it follows the
                         # head into any shield". It therefore leaves
                         # OUT the MS-1L's own wall noise, which is
                         # negligible at 0.05 Hz (613 -> 615 fT) and
                         # not at 1 Hz (42 -> 46) or 10 Hz (14 -> 21).
                         # Left as frozen on 2026-07-04 rather than
                         # improved after the fact -- the point of a
                         # pre-registration is that it cannot be
                         # (2026-08-19 analysis review, M9). Read the
                         # dotted S curve as optimistic above ~1 Hz.
                         S=meas))
    model["f_magn_Hz"] = dict(central=fm_c, lo=fm_lo, hi=fm_hi)

    # every headline number must be a positive finite ASD before anything
    # is written; a violation means the model inputs are off, not the data
    for r in rows:
        checks = [r["meas"], *r["shield"], *r["E"], *r["M"], r["S"]]
        require(all(np.isfinite(v) and v > 0 for v in checks),
                f"prediction at {r['f']:g} Hz is not a positive finite number")

    # mid-band white-floor acceptance numbers (3-10 Hz, below/at skin rolloff)
    white_gui = (quad(MS1L_WHITE, SENSOR_LO), quad(MS1L_WHITE, SENSOR_HI))
    daq_floor_1hz = at(f_d, a_d, 1.0)
    daq_floor_02hz = at(f_d, a_d, 0.2)  # lowest ~reliable bin of the 60 s record
    white_daq = (quad(MS1L_WHITE, SENSOR_LO, daq_floor_1hz),
                 quad(MS1L_WHITE, SENSOR_HI, daq_floor_1hz))

    # ---------------- figure ----------------
    plt.rcParams.update({
        "figure.dpi": 150, "font.size": 11, "axes.titleweight": "bold",
        "axes.spines.top": False, "axes.spines.right": False,
        "legend.frameon": False, "grid.color": "#E6E8EA",
    })
    fig, ax = plt.subplots(figsize=(8.2, 6.4))
    fgrid = np.logspace(np.log10(0.008), np.log10(100), 400)

    ax.loglog(f_b, a_b, color="#2E6E8E", lw=1.6, zorder=3,
              label="measured: bench shield closed (GUI, 15 min)")
    ax.loglog(f_d, a_d, color="#9AA0A6", lw=1.0, alpha=0.8, zorder=2,
              label="DAQ chain floor as frozen 07-04 "
                    "(QZFM off, ±1 V; INVALIDATED 07-06, AI- unwired)")

    sh_lo_c = shield_asd(fgrid, fm_lo)
    sh_hi_c = shield_asd(fgrid, fm_hi)
    ax.fill_between(fgrid, sh_lo_c, sh_hi_c, color="#C58A2D", alpha=0.25, lw=0,
                    zorder=1, label="MS-1L own noise (16 fT white + 1/f magn. band)")
    ax.loglog(fgrid, shield_asd(fgrid, fm_c), color="#C58A2D", lw=1.4, ls="--", zorder=2)

    ax.axhspan(SENSOR_LO, SENSOR_HI, color="#9AA0A6", alpha=0.20, lw=0,
               label="QZFM self-noise spec (7–15 fT/√Hz)")

    # scenario totals inside MS-1L (the NaN edges only blank the plotted line
    # outside the measured band; nothing downstream reads ex_grid)
    ex_grid = np.interp(fgrid, f_b, a_excess, left=np.nan, right=np.nan)
    totE = np.sqrt((ex_grid * S_BENCH_CENTRAL / S_MS1L_NOM) ** 2
                   + shield_asd(fgrid, fm_c) ** 2 + SENSOR_TYP ** 2)
    totM = np.sqrt(shield_asd(fgrid, fm_c) ** 2 + SENSOR_TYP ** 2)
    ax.loglog(fgrid, totE, color="#7A4E9E", lw=2.0, zorder=4,
              label="predicted in MS-1L — scenario E (external leak)")
    ax.loglog(fgrid, totM, color="#5A7D3A", lw=2.0, zorder=4,
              label="predicted in MS-1L — scenario M (bench magnetization)")
    ax.loglog(f_b[f_b < 3], a_b[f_b < 3], color="#C0504D", lw=2.0, ls=":", zorder=4,
              label="predicted in MS-1L — scenario S (sensor 1/f unchanged;\n"
                    "excludes MS-1L wall noise, so optimistic above ~1 Hz)")

    ax.axvspan(*EDM_BAND, color=EDM_FILL, alpha=0.45, zorder=0,
               label="measurement band (E/B reversal ~0.05 Hz)")
    ax.axhline(TARGET, color="#33373B", ls=(0, (5, 3)), lw=1.3, zorder=1)
    ax.text(20, TARGET * 1.12, "lattice budget 50 fT/√Hz", fontsize=9, color="#33373B")

    r05 = next(r for r in rows if r["f"] == F_EDM)
    for tot, color, dy in ((r05["E"][1], "#7A4E9E", 1.35), (r05["M"][1], "#5A7D3A", 0.62)):
        ax.plot(F_EDM, tot, "o", ms=6, color=color, zorder=5)
        ax.annotate(f"{tot * 1e3:.0f} fT/√Hz", xy=(F_EDM, tot),
                    xytext=(F_EDM * 1.25, tot * dy), fontsize=8.5, color=color)

    ax.set_xlim(0.008, 110)
    ax.set_ylim(0.002, 60)
    ax.grid(True, which="major", alpha=0.9)
    ax.grid(True, which="minor", alpha=0.35)
    ax.set_xlabel("frequency  [Hz]")
    ax.set_ylabel(r"magnetic field ASD  [pT/$\sqrt{\mathrm{Hz}}$]")
    ax.set_title("B2 — predicted noise budget inside the Twinleaf MS-1L", loc="left")
    ax.legend(loc="upper center", bbox_to_anchor=(0.5, -0.13), ncol=2, fontsize=8.6)
    fig.tight_layout()
    out_png = os.path.join(HERE, "fig_b2_ms1l_prediction.png")
    fig.savefig(out_png, bbox_inches="tight")
    plt.close(fig)
    print(f"[saved] {out_png}")

    # ---------------- tables ----------------
    def ft(v):  # pT/rtHz -> fT/rtHz string
        return f"{v * 1000:.3g}"

    lines = [
        "| f [Hz] | measured bench | scenario E in MS-1L | scenario M in MS-1L | scenario S in MS-1L |",
        "|---|---|---|---|---|",
    ]
    for r in rows:
        lines.append(
            f"| {r['f']:g} | {ft(r['meas'])} | {ft(r['E'][1])} ({ft(r['E'][0])}–{ft(r['E'][2])}) "
            f"| {ft(r['M'][1])} ({ft(r['M'][0])}–{ft(r['M'][2])}) | {ft(r['S'])} |")
    table = "\n".join(lines)

    summary = {
        "f_magn_Hz": model["f_magn_Hz"],
        "rows_fT_per_rtHz": [
            {k: (np.round(np.asarray(v) * 1e3, 2).tolist()
                 if k not in ("f",) else v) for k, v in r.items()} for r in rows
        ],
        "white_floor_fT": {
            "bench_measured": BENCH_WHITE * 1e3,
            "ms1l_gui_path_predicted": [round(w * 1e3, 1) for w in white_gui],
            "ms1l_daq_path_predicted": [round(w * 1e3, 1) for w in white_daq],
            "daq_floor_at_1Hz_measured": round(daq_floor_1hz * 1e3, 1),
            "daq_floor_at_0p2Hz_measured": round(daq_floor_02hz * 1e3, 1),
        },
        "assumptions": {
            "ms1l_white_fT": MS1L_WHITE * 1e3,
            "S_ms1l": [S_MS1L_DERATED, S_MS1L_NOM],
            "S_bench_bracket": [S_BENCH_LO, S_BENCH_CENTRAL],
            "sensor_spec_fT": [SENSOR_LO * 1e3, SENSOR_HI * 1e3],
            "wall_thickness_mm": [T_WALL_HI * 1e3, T_WALL_CENTRAL * 1e3, T_WALL_LO * 1e3],
            "mu_r": MU_R, "sigma_S_per_m": SIGMA, "tan_delta": TAN_DELTA,
        },
    }
    with open(os.path.join(HERE, "b2_summary.json"), "w") as fh:
        # allow_nan=False: a NaN that slipped past the gates must still fail
        # here rather than silently produce non-standard JSON
        json.dump(summary, fh, indent=2, ensure_ascii=False, allow_nan=False)
    with open(os.path.join(HERE, "b2_summary.md"), "w") as fh:
        fh.write("# B2 summary table (values in fT/sqrt(Hz))\n\n"
                 f"f_magn central {fm_c:.2f} Hz (band {fm_hi:.2f}"
                 f"–{fm_lo:.2f} Hz swept via wall thickness)\n\n" + table + "\n")
    print(f"[saved] b2_summary.json, b2_summary.md")

    print(f"\nf_magn: central {fm_c:.3f} Hz, band {fm_hi:.3f}–{fm_lo:.3f} Hz")
    print(table)
    print(f"\nwhite floor 3–10 Hz predicted in MS-1L: GUI path "
          f"{ft(white_gui[0])}–{ft(white_gui[1])} fT/rtHz, "
          f"DAQ path {ft(white_daq[0])}–{ft(white_daq[1])} fT/rtHz "
          f"(bench measured {ft(BENCH_WHITE)})")


if __name__ == "__main__":
    main()
