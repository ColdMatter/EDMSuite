#!/usr/bin/env python3
"""White-noise floor vs wall clock across the July 8 evening dual-single-dual
sandwich -- the repeat that tests the hysteresis seen at midday.

Three back-to-back 15-min DAQ runs: dual Z+Y (17:24), single-axis Z
(17:42, three minutes after the first dual ends), dual again (18:20).
Each run is cut into 60-s blocks and the 3-40 Hz median ASD of each
block is plotted at its absolute time, sensor-only (the 6.8 fT/rtHz
analog chain removed in quadrature). The midday A-B-A' run suggested the
floor was slow to fall after switching dual->single; here the single leg
starts three minutes after a dual and is asked whether it still lags.
Writes fig_mode_settling2_0708.png next to this script.
"""

import os
from datetime import datetime

import numpy as np
from scipy.signal import welch

import matplotlib
matplotlib.use("Agg")
import matplotlib.pyplot as plt

HERE = os.path.dirname(os.path.abspath(__file__))
import sys as _sys  # analysis_scripts/ on sys.path for _paths
_sys.path.insert(0, os.path.dirname(os.path.dirname(os.path.abspath(__file__))))
from _paths import DATA_ROOT, PROJECT_DIR, ANALYSIS_DIR, data, data_join, rel_data  # noqa: E402,F401
REPO = PROJECT_DIR

FS = 1200.0
BLOCK_S = 60.0
SKIP_S = 5.0
CHAIN_FT = 6.8
BASELINE_0630_FT = 11.7
SINGLE_REF = 15.0
DUAL_REF = 20.0

RUNS = (
    ("result/modetest2/modeB1_dual/modeB1daq_Z_pT.txt", "2026-07-08T17:24:31",
     "B dual", "#C0504D", "s"),
    ("result/modetest2/modeA_single/modeAdaq_Z_pT.txt", "2026-07-08T17:42:19",
     "A single", "#2E6E8E", "o"),
    ("result/modetest2/modeB2_dual/modeB2daq_Z_pT.txt", "2026-07-08T18:20:57",
     "B′ dual", "#C0504D", "^"),
)


def block_floors(x, fs):
    n = int(BLOCK_S * fs)
    x = x[int(SKIP_S * fs):]
    out_t, out_a = [], []
    for i in range(len(x) // n):
        seg = x[i * n:(i + 1) * n]
        f, p = welch(seg, fs=fs, nperseg=int(10 * fs), detrend="linear")
        a = np.sqrt(p) * 1e3
        raw = float(np.median(a[(f >= 3.0) & (f <= 40.0)]))
        sens = float(np.sqrt(max(0.0, raw**2 - CHAIN_FT**2)))
        out_t.append(SKIP_S + (i + 0.5) * BLOCK_S)
        out_a.append(sens)
    return np.array(out_t), np.array(out_a)


def main():
    plt.rcParams.update({
        "figure.dpi": 150, "font.size": 11, "axes.titleweight": "bold",
        "axes.spines.top": False, "axes.spines.right": False,
        "axes.edgecolor": "#7A7F85", "text.color": "#33373B",
        "legend.frameon": False,
    })
    fig, ax = plt.subplots(figsize=(6.5, 4.2))

    t0 = datetime.fromisoformat(RUNS[0][1])
    for rel, iso, label, color, marker in RUNS:
        x = np.loadtxt(data_join(rel), skiprows=1)
        start = (datetime.fromisoformat(iso) - t0).total_seconds()
        t, a = block_floors(x, FS)
        ax.plot((start + t) / 60.0, a, marker=marker, ms=5, lw=1.2,
                color=color, label=label)

    ax.axhline(DUAL_REF, color="#C0504D", ls="--", lw=0.9, alpha=0.6)
    ax.axhline(SINGLE_REF, color="#2E6E8E", ls="--", lw=0.9, alpha=0.6)
    ax.axhline(BASELINE_0630_FT, color="#9AA0A6", ls="--", lw=0.9, alpha=0.6)

    ax.set_xlabel("time from 17:24 (min)")
    ax.set_ylabel("3–40 Hz floor (fT/√Hz)")
    ax.set_title("Mode settling: dual–single–dual")
    ax.set_xlim(-2, 80)
    ax.set_ylim(10, 28)
    ax.legend(loc="upper right", fontsize=9)
    fig.tight_layout()
    out = os.path.join(HERE, "fig_mode_settling2_0708.png")
    fig.savefig(out, bbox_inches="tight")
    print(f"[out ] {out}")


if __name__ == "__main__":
    main()
