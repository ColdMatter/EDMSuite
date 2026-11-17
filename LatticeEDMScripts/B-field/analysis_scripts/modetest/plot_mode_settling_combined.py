#!/usr/bin/env python3
"""Both July-8 mode sandwiches overlaid from each sandwich's first run.

Midday (A–B–A′) is timed from its first A start; evening (B–A–B′) from
its first B start. Both therefore begin at t=0 on the same axes.
Writes the figure to modetest/ and modetest2/.
"""

import os
from datetime import datetime

import numpy as np
from scipy.signal import welch

import matplotlib
matplotlib.use("Agg")
import matplotlib.pyplot as plt
from matplotlib.lines import Line2D

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

# (path, iso start, legend, color, marker, sandwich)
RUNS = (
    ("result/modetest/modeA_single/modeAdaq_Z_pT.txt", "2026-07-08T12:38:11",
     "mid A single", "#2E6E8E", "o", "midday"),
    ("result/modetest/modeB_dual/modeBdaq_Z_pT.txt", "2026-07-08T12:57:13",
     "mid B dual", "#5B8FA8", "s", "midday"),
    ("result/modetest/modeA2_single/modeA2daq_Z_pT.txt", "2026-07-08T13:14:36",
     "mid A' single", "#1B4F72", "^", "midday"),
    ("result/modetest2/modeB1_dual/modeB1daq_Z_pT.txt", "2026-07-08T17:24:31",
     "eve B dual", "#C0504D", "s", "evening"),
    ("result/modetest2/modeA_single/modeAdaq_Z_pT.txt", "2026-07-08T17:42:19",
     "eve A single", "#E07A3D", "o", "evening"),
    ("result/modetest2/modeB2_dual/modeB2daq_Z_pT.txt", "2026-07-08T18:20:57",
     "eve B' dual", "#8B2E2A", "^", "evening"),
)

# origin = first run of each sandwich (midday: first A, evening: first B)
ORIGIN = {
    "midday": "2026-07-08T12:38:11",   # first A
    "evening": "2026-07-08T17:24:31",  # first B
}


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
    fig, ax = plt.subplots(figsize=(9.0, 5.6))
    origins = {k: datetime.fromisoformat(v) for k, v in ORIGIN.items()}

    for y, col in ((DUAL_REF, "#C0504D"),
                   (SINGLE_REF, "#2E6E8E"),
                   (BASELINE_0630_FT, "#33373B")):
        ax.axhline(y, color=col, ls="--", lw=0.9, alpha=0.45, zorder=0)

    for rel, iso, label, color, marker, sandwich in RUNS:
        x = np.loadtxt(data_join(rel), skiprows=1)
        start = (datetime.fromisoformat(iso) - origins[sandwich]).total_seconds()
        t, a = block_floors(x, FS)
        ax.plot((start + t) / 60.0, a, marker=marker, ms=5.5, lw=1.2,
                color=color, label=label, zorder=3)

    ax.set_xlabel("time [min]", labelpad=4)
    ax.set_ylabel("noise floor [fT/√Hz]")
    ax.set_title("Single vs dual floor: midday and evening sandwiches")
    ax.set_xlim(-2, 72)
    ax.set_ylim(10, 28)

    ref_handles = [
        Line2D([0], [0], color="#C0504D", ls="--", lw=1.2, label="dual ~20"),
        Line2D([0], [0], color="#2E6E8E", ls="--", lw=1.2, label="single ~15"),
        Line2D([0], [0], color="#33373B", ls="--", lw=1.2, label="06-30  11.7"),
    ]
    h, lab = ax.get_legend_handles_labels()
    ax.legend(h + ref_handles, lab + [r.get_label() for r in ref_handles],
              fontsize=8.5, loc="center left", bbox_to_anchor=(1.03, 0.5),
              frameon=False, borderaxespad=0.0, labelspacing=1.05,
              handletextpad=1.0, handlelength=2.0, markerscale=1.15)

    fig.tight_layout()
    fig.subplots_adjust(right=0.72)
    name = "fig_mode_settling_combined_0708.png"
    outs = [
        os.path.join(HERE, name),
        os.path.join(os.path.dirname(HERE), "modetest2", name),
    ]
    for out in outs:
        fig.savefig(out, bbox_inches="tight")
        print(f"[out ] {out}")


if __name__ == "__main__":
    main()
