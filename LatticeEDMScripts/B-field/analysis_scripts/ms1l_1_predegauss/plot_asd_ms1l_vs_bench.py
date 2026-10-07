#!/usr/bin/env python3
"""MS-1L before degauss against the bench shield, DAQ records.

The July 13 pre-degauss run inside the MS-1L (900 s, dual Y+Z) against
the two bench references: the settled single-Z mode run (July 8) and the
30 min dual run with the lid closed (sync6). The MS-1L curves use the
full record on purpose -- the low-frequency wander IS the phenomenon
(wall-magnetization relaxation, axis-incoherent); the bench curves get
the standard 500 s settle trim. All curves are the DAQ pT record at
fs = 1200 Hz, so the readout path is identical.
Writes fig_asd_ms1l_vs_bench_0713.png next to this script.
"""

import os

import numpy as np
from scipy.signal import welch

import matplotlib
matplotlib.use("Agg")
import matplotlib.pyplot as plt
from matplotlib.lines import Line2D
from matplotlib.patches import Patch

HERE = os.path.dirname(os.path.abspath(__file__))
import sys as _sys  # analysis_scripts/ on sys.path for _paths
_sys.path.insert(0, os.path.dirname(os.path.dirname(os.path.abspath(__file__))))
from _paths import DATA_ROOT, PROJECT_DIR, ANALYSIS_DIR, data, data_join, rel_data  # noqa: E402,F401
REPO = PROJECT_DIR

FS = 1200.0
SETTLE_S = 500.0
TARGET_FT = 50.0
EDM_BAND = (0.01, 1.0)
EDM_FILL = "#ECE7DD"

# (legend, path, trim_s, color, ls, short key for table)
RUNS = (
    ("MS-1L Z (pre-degauss)",
     "result/ms1l_1_predegauss/predegauss_900s/predegaussdaq_Z_pT.txt", 0.0,
     "#C0504D", "-", "MS-1L Z"),
    ("MS-1L Y (pre-degauss)",
     "result/ms1l_1_predegauss/predegauss_900s/predegaussdaq_Y_pT.txt", 0.0,
     "#D89A96", "--", "MS-1L Y"),
    ("bench single-Z",
     "result/modetest/modeA_single/modeAdaq_Z_pT.txt", SETTLE_S,
     "#2E6E8E", "-", "bench single"),
    ("bench dual-Z",
     "result/synctest/sync6_30min/sync6daq_Z_pT.txt", SETTLE_S,
     "#9AA0A6", "-", "bench dual"),
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
    fig, ax = plt.subplots(figsize=(9.0, 5.2))

    ax.axvspan(*EDM_BAND, color=EDM_FILL, alpha=0.65, zorder=0)
    ax.axvspan(3.0, 40.0, color="#E6E8EA", alpha=0.45, zorder=0)
    ax.axhline(TARGET_FT, color="#33373B", ls="--", lw=0.9, alpha=0.7, zorder=1)

    rows = []
    for label, rel, trim_s, color, ls, key in RUNS:
        x = np.loadtxt(data_join(rel))
        x = x[int(trim_s * FS):]
        f, a = spliced_asd(x, FS)
        m = f > 0
        ax.loglog(f[m], a[m] * 1e3, lw=1.15, color=color, ls=ls,
                  label=label, zorder=3)
        floor = band_median(f, a, 3.0, 40.0) * 1e3
        lf = band_median(f, a, 0.04, 0.0625) * 1e3
        rows.append((key, lf, floor))

    ax.set_xlabel("frequency [Hz]")
    ax.set_ylabel(r"ASD [fT/$\sqrt{\mathrm{Hz}}$]")
    ax.set_title("MS-1L vs bench ASD")
    ax.set_xlim(8e-3, 6e2)
    ax.set_ylim(1.5e1, 3e5)

    # band numbers in legend labels (no floating table over the curves)
    data_labels = [
        f"MS-1L Z  (0.05Hz {rows[0][1]:.0f}, floor {rows[0][2]:.0f})",
        f"MS-1L Y  (0.05Hz {rows[1][1]:.0f}, floor {rows[1][2]:.0f})",
        f"bench single-Z  ({rows[2][1]:.0f} / {rows[2][2]:.1f})",
        f"bench dual-Z  ({rows[3][1]:.0f} / {rows[3][2]:.1f})",
    ]
    handles = [
        Line2D([0], [0], color="#C0504D", lw=1.2, label=data_labels[0]),
        Line2D([0], [0], color="#D89A96", lw=1.2, ls="--", label=data_labels[1]),
        Line2D([0], [0], color="#2E6E8E", lw=1.2, label=data_labels[2]),
        Line2D([0], [0], color="#9AA0A6", lw=1.2, label=data_labels[3]),
        Patch(facecolor=EDM_FILL, edgecolor="none", label="EDM band (~0.05 Hz)"),
        Patch(facecolor="#E6E8EA", edgecolor="none", label="white floor (3–40 Hz)"),
        Line2D([0], [0], color="#33373B", ls="--", lw=1.0, label="Lattice target 50"),
    ]
    ax.legend(handles=handles, fontsize=8.0, loc="center left",
              bbox_to_anchor=(1.02, 0.5), frameon=False, borderaxespad=0.0,
              labelspacing=0.75, handletextpad=0.7, handlelength=1.8)

    fig.tight_layout()
    fig.subplots_adjust(right=0.62)
    out = os.path.join(HERE, "fig_asd_ms1l_vs_bench_0713.png")
    fig.savefig(out, bbox_inches="tight")
    print(f"[out ] {out}")
    for k, lf, fl in rows:
        print(f"[num ] {k:<14s}  0.05Hz={lf:.0f}  floor={fl:.1f}")


if __name__ == "__main__":
    main()
