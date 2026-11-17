#!/usr/bin/env python3
"""Day-over-day relaxation series inside the MS-1L (before degauss).

One 900 s dual run per lab day while the transported walls relax. Adds
one row to DAYS per run and compares the 0.05 Hz band level, the white
floor and the DC drift, per axis, on the usable (unclipped) stretch.
The registered model is the magnetic after-effect log law: metrics are
reported against ln(days since arrival 2026-07-11).
The Y axis creeps one way while un-degaussed and eventually pins at the
DAQ +/-1 V window; clipped samples are cut automatically, so Y numbers
may rest on a short early stretch (the table prints the seconds used).
Writes fig_relax_series.png next to this script.
"""

import math
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

FS = 1200.0
CLIP_PT = 395.0           # DAQ +/-1 V is ~+/-400 pT at 2.624 V/nT
CLIP_HOLD_S = 1.0
CLIP_BACKOFF_S = 5.0
LF_BAND = (0.04, 0.0625)  # the 0.05 Hz report band
FLOOR_BAND = (3.0, 40.0)
EDM_BAND = (0.01, 1.0)
EDM_FILL = "#ECE7DD"
TARGET_FT = 50.0

# label, days since arrival (2026-07-11), file prefix, color
DAYS = (
    ("07-13 (day 2)", 2.0, "result/ms1l_1_predegauss/predegauss_900s/predegaussdaq",
     "#9AA0A6"),
    ("07-15 (day 4)", 4.0, "result/ms1l_1_predegauss/relax0715_900s/relax0715daq",
     "#C0504D"),
)
AXES = (("Z", "-"), ("Y", "--"))


def usable_stretch(x):
    """Samples before the first sustained excursion into the DAQ clip."""
    hold = int(CLIP_HOLD_S * FS)
    clipped = np.abs(x) >= CLIP_PT
    run = np.convolve(clipped.astype(int), np.ones(hold, int), "valid")
    hits = np.flatnonzero(run >= hold)
    if len(hits) == 0:
        return x
    cut = max(int(hits[0] - CLIP_BACKOFF_S * FS), 0)
    return x[:cut]


from bfield.analysis.spectra import spliced_asd_capped as spliced_asd  # noqa: E402 -- was an identical local copy


from bfield.analysis.spectra import band_median_or_nan as band_median  # noqa: E402 -- was an identical local copy


def drift_pT_per_min(x):
    t = np.arange(len(x)) / FS
    return float(np.polyfit(t, x, 1)[0] * 60.0)


def main():
    plt.rcParams.update({
        "figure.dpi": 150, "font.size": 11, "axes.titleweight": "bold",
        "axes.spines.top": False, "axes.spines.right": False,
        "axes.edgecolor": "#7A7F85", "text.color": "#33373B",
        "legend.frameon": False,
    })
    fig, ax = plt.subplots(figsize=(8.0, 5.2))
    rows = [f"{'':<22s}{'used':>6s}{'0.05Hz':>9s}{'floor':>7s}{'drift':>8s}"]
    for label, days, prefix, color in DAYS:
        for axname, ls in AXES:
            x = np.loadtxt(data_join(f"{prefix}_{axname}_pT.txt"))
            u = usable_stretch(x)
            if len(u) < 120 * FS:
                rows.append(f"{label + ' ' + axname:<22s}{'<120 s: skipped'}")
                continue
            f, a = spliced_asd(u, FS)
            lf = band_median(f, a, *LF_BAND) * 1e3
            floor = band_median(f, a, *FLOOR_BAND) * 1e3
            drift = drift_pT_per_min(u)
            rows.append(f"{label + ' ' + axname:<22s}{len(u) / FS:>5.0f}s"
                        f"{lf:>9.0f}{floor:>7.1f}{drift:>+8.1f}")
            ax.loglog(f[f > 0], a[f > 0] * 1e3, lw=0.9, color=color, ls=ls,
                      label=f"{label} {axname} ({len(u) / FS:.0f} s)")
    ax.axvspan(*EDM_BAND, color=EDM_FILL, alpha=0.7, zorder=0,
               label="measurement band (~0.05 Hz)")
    ax.axhline(TARGET_FT, color="#33373B", ls="--", lw=0.9)
    ax.annotate("Lattice target 50 fT/rtHz", (6.0, TARGET_FT * 1.08),
                fontsize=8.5, va="bottom", ha="left")
    ax.annotate("usable / 0.05 Hz [fT/rtHz] / floor / drift [pT/min]\n"
                + "\n".join(rows), xy=(1.03, 1.0), xycoords="axes fraction",
                ha="left", va="top", fontsize=8, family="monospace",
                color="#33373B")
    ax.set_xlabel("frequency [Hz]")
    ax.set_ylabel("ASD [fT/rtHz]")
    ax.set_title("MS-1L relaxation series, before degauss (DAQ record)")
    ax.legend(fontsize=8.5, loc="upper center", bbox_to_anchor=(0.5, -0.13),
              ncol=2, frameon=False)
    out = os.path.join(HERE, "fig_relax_series.png")
    fig.savefig(out, bbox_inches="tight")
    print(f"[out ] {out}")
    for r in rows:
        print("[num ] " + r)
    # first log-law slope estimate once two or more days exist (per axis,
    # 0.05 Hz level vs ln(days)); statistically meaningful from day 3 on
    if len(DAYS) >= 2:
        print("[note] log-law check: equal improvement per decade of time "
              "predicts d4->d8 change equal to d2->d4 change")


if __name__ == "__main__":
    main()
