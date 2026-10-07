#!/usr/bin/env python3
"""ASD of the three 30-min-class sessions on one axis and one readout.

06-30 (lid fully closed, before it was ever opened), 07-06 sync5 (outer
lid ajar by ~5 cm, found on 07-08), and 07-08 sync6 (lid re-closed,
calibration factor z,y ~0.8). All three curves are the GUI digital
record of Z, so the readout path is identical and the differences are
the sensor/shield state, not the chain.
Writes fig_asd_compare_0630_0706_0708.png next to this script.
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
TARGET_FT = 50.0          # Lattice benchmark at 0.05 Hz band
EDM_BAND = (0.01, 1.0)    # E/B reversal band that sets the eEDM sensitivity
EDM_FILL = "#ECE7DD"      # muted warm-neutral wash, kept understated

RUNS = (
    ("2026-06-30  lid closed (baseline)",
     "result/gui_baseline/2026-06-30_closed-shield/closed_long_Z_pT.txt",
     "#9AA0A6"),
    ("2026-07-06  lid ajar ~5 cm (sync5)",
     "result/synctest/sync5_30min/gui_sync5_Formatted.txt",
     "#C0504D"),
    ("2026-07-08  lid re-closed (sync6)",
     "result/synctest/sync6_30min/gui_sync6_Formatted.txt",
     "#2E6E8E"),
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
    fig, ax = plt.subplots(figsize=(8.0, 5.2))
    notes = [f"{'':<12s}{'0.05Hz':>7s}{'floor':>8s}"]
    for label, rel, color in RUNS:
        x = np.loadtxt(data_join(rel), skiprows=1)
        x = x[int(SETTLE_S * FS_GUI):]
        f, a = spliced_asd(x, FS_GUI)
        m = f > 0
        ax.loglog(f[m], a[m] * 1e3, lw=0.9, color=color, label=label)
        floor = band_median(f, a, 3.0, 40.0) * 1e3
        lf = band_median(f, a, 0.04, 0.0625) * 1e3
        notes.append(f"{label.split()[0]:<12s}{lf:>7.0f}{floor:>8.1f}")
    ax.axvspan(*EDM_BAND, color=EDM_FILL, alpha=0.7, zorder=0,
               label="measurement band (E/B reversal ~0.05 Hz)")
    ax.axvspan(3.0, 40.0, color="#E6E8EA", alpha=0.5, zorder=0,
               label="white-floor diagnostic (3-40 Hz)")
    ax.axhline(TARGET_FT, color="#33373B", ls="--", lw=0.9)
    ax.annotate("Lattice target 50 fT/rtHz", (6.0, TARGET_FT * 1.08),
                fontsize=8.5, va="bottom", ha="left")
    # numbers table sits outside the axes, to the right
    ax.annotate("band levels [fT/rtHz]\n" + "\n".join(notes), xy=(1.03, 1.0),
                xycoords="axes fraction", ha="left", va="top",
                fontsize=8.5, family="monospace", color="#33373B")
    ax.set_xlabel("frequency [Hz]")
    ax.set_ylabel("ASD [fT/rtHz]")
    ax.set_title("bench shield, GUI Z record: three sessions, one readout path")
    # legend below the axes so the plot area stays clean
    ax.legend(fontsize=8.5, loc="upper center", bbox_to_anchor=(0.5, -0.13),
              ncol=2, frameon=False)
    out = os.path.join(HERE, "fig_asd_compare_0630_0706_0708.png")
    fig.savefig(out, bbox_inches="tight")
    print(f"[out ] {out}")
    for n in notes:
        print("[num ] " + n)


if __name__ == "__main__":
    main()
