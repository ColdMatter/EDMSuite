#!/usr/bin/env python3
"""White-noise floor vs wall clock across the July 8 A-B-A' mode sandwich.

Three back-to-back 15-min DAQ runs: single-axis Z (12:38), dual Z+Y
(12:57), single-axis Z again (13:14). Each run is cut into 60-s blocks
and the 3-40 Hz median ASD of each block is plotted at its absolute
time, on a sensor-only basis (the 6.8 fT/rtHz analog chain removed in
quadrature). If the floor were a pure function of the mode the two
single runs would sit on one level; a lag after each mode switch shows
up as a relaxation instead.
Writes fig_mode_settling_0708.png next to this script.
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
SKIP_S = 5.0              # acquisition-start transient only; settling is the signal here
CHAIN_FT = 6.8            # analog chain floor, removed in quadrature
BASELINE_0630_FT = 11.7   # GUI Z floor of the 06-30 closed-shield run (sensor-only)

RUNS = (
    ("result/modetest/modeA_single/modeAdaq_Z_pT.txt", "2026-07-08T12:38:11",
     "A  single-Z", "#2E6E8E", "o"),
    ("result/modetest/modeB_dual/modeBdaq_Z_pT.txt", "2026-07-08T12:57:13",
     "B  dual (Z shown)", "#C0504D", "s"),
    ("result/modetest/modeA2_single/modeA2daq_Z_pT.txt", "2026-07-08T13:14:36",
     "A' single-Z", "#2E6E8E", "^"),
)


def block_floors(x, fs):
    """(t_center_s, sensor-only floor fT/rtHz) for consecutive 60-s blocks."""
    n = int(BLOCK_S * fs)
    x = x[int(SKIP_S * fs):]
    out_t, out_a = [], []
    for i in range(len(x) // n):
        seg = x[i * n:(i + 1) * n]
        f, p = welch(seg, fs=fs, nperseg=int(10 * fs), detrend="linear")
        a = np.sqrt(p) * 1e3
        m = (f >= 3.0) & (f <= 40.0)
        raw = float(np.median(a[m]))
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
    fig, ax = plt.subplots(figsize=(8.6, 5.0))

    t0 = datetime.fromisoformat(RUNS[0][1])
    spans = []
    for rel, iso, label, color, marker in RUNS:
        x = np.loadtxt(data_join(rel), skiprows=1)
        start = (datetime.fromisoformat(iso) - t0).total_seconds()
        t, a = block_floors(x, FS)
        ax.plot((start + t) / 60.0, a, marker=marker, ms=5, lw=1.1,
                color=color, label=label)
        spans.append((start / 60.0, (start + len(x) / FS) / 60.0, label))

    for (_, prev_end, _), (nxt, _, label) in zip(spans, spans[1:]):
        ax.axvspan(prev_end, nxt, color="#E6E8EA", alpha=0.7, zorder=0)
        mode = "dual" if "dual" in label else "single"
        ax.annotate(f"switch to {mode}\n+ zero/calibrate",
                    ((prev_end + nxt) / 2, 23.6), ha="center", va="top",
                    fontsize=8, color="#5F6368")

    ax.axhline(BASELINE_0630_FT, color="#33373B", ls="--", lw=0.9)
    ax.annotate("2026-06-30 baseline 11.7 fT/rtHz", (0.5, BASELINE_0630_FT),
                fontsize=8.5, va="bottom")
    ax.set_xlabel("minutes from 12:38 (run A start)")
    ax.set_ylabel("3-40 Hz floor, sensor-only [fT/rtHz]")
    ax.set_title("mode sandwich, 60-s blocks: the floor lags the mode switch")
    ax.set_ylim(10, 24.5)
    ax.legend(fontsize=9, loc="lower right")
    out = os.path.join(HERE, "fig_mode_settling_0708.png")
    fig.savefig(out, bbox_inches="tight")
    print(f"[out ] {out}")


if __name__ == "__main__":
    main()
