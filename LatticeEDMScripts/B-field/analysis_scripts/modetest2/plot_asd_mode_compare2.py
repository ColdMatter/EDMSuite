#!/usr/bin/env python3
"""ASD of the July 8 evening dual-single-dual repeat against the 06-30 baseline.

All curves are the GUI digital record of Z, so the readout path is
identical and natively sensor-only. B and B' are dual Z+Y, A is
single-axis Z. Unlike the midday A-B-A' run, here the single leg sits
firmly at the single level with no lag, and the two dual legs bracket
it -- the mode sets the floor, cleanly and repeatably.
Writes fig_asd_mode_compare2_0708.png next to this script.
"""

import os

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

FS_GUI = 203.18
SETTLE_S = 500.0
TARGET_FT = 50.0

RUNS = (
    ("06-30 single",
     "result/gui_baseline/2026-06-30_closed-shield/closed_long_Z_pT.txt",
     "#9AA0A6"),
    ("B dual",
     "result/modetest2/modeB1_dual/gui_modeB1_Formatted.txt",
     "#C0504D"),
    ("A single",
     "result/modetest2/modeA_single/gui_modeA_Formatted.txt",
     "#2E6E8E"),
    ("B′ dual",
     "result/modetest2/modeB2_dual/gui_modeB2_Formatted.txt",
     "#E39A6B"),
)


from bfield.analysis.spectra import spliced_asd  # noqa: E402 -- was an identical local copy


from bfield.analysis.spectra import band_median  # noqa: E402 -- was an identical local copy


def main():
    plt.rcParams.update({
        "figure.dpi": 150, "font.size": 11, "axes.titleweight": "bold",
        "axes.spines.top": False, "axes.spines.right": False,
        "axes.edgecolor": "#7A7F85", "text.color": "#33373B",
        "legend.frameon": False,
    })
    fig, ax = plt.subplots(figsize=(6.0, 4.2))
    notes = [f"{'':<11s}{'0.05Hz':>7s}{'floor':>8s}"]
    for label, rel, color in RUNS:
        x = np.loadtxt(data_join(rel), skiprows=1)
        x = x[int(SETTLE_S * FS_GUI):]
        f, a = spliced_asd(x, FS_GUI)
        m = f > 0
        ax.loglog(f[m], a[m] * 1e3, lw=1.2, color=color, label=label)
        floor = band_median(f, a, 3.0, 40.0) * 1e3
        lf = band_median(f, a, 0.04, 0.0625) * 1e3
        notes.append(f"{label:<11s}{lf:>7.0f}{floor:>8.1f}")
    ax.axhline(TARGET_FT, color="#33373B", ls="--", lw=1.0, alpha=0.75)
    ax.text(3e-2, TARGET_FT * 1.25, "50 fT target", fontsize=9, va="bottom")
    ax.set_xlim(5e-3, 100)
    ax.set_ylim(10, 2e4)
    ax.set_xlabel("frequency (Hz)")
    ax.set_ylabel("ASD (fT/√Hz)")
    ax.set_title("GUI Z: dual–single–dual repeat")
    ax.legend(loc="upper right", fontsize=9)
    fig.tight_layout()
    out = os.path.join(HERE, "fig_asd_mode_compare2_0708.png")
    fig.savefig(out, bbox_inches="tight")
    print(f"[out ] {out}")
    for n in notes:
        print("[num ] " + n)


if __name__ == "__main__":
    main()
