#!/usr/bin/env python3
"""Does the 0.05 Hz plateau survive four days? (08-03 vs the N=16 tail)

The shield has not been degaussed since the N=8 passes on 07-30 (5 Hz
3.2 A = 25.6 A-turns, then the 3 Hz finish 2.6 A = 20.8). Today's run
therefore measures the same shield state at t0+90 h. The natural
comparison is the last run of the N=16 series -- 07-30 15:53, t0+25.6 h
after the 07-29 degauss -- which is the deepest point of that series and
was taken before the N=8 winding touched the shield.

Both runs are 900 s, +/-1 V, sensor Z3D0-AB3Q, rail 0%, so the pair is
directly comparable under the canonical window (first 900 s, 200 s Welch
segments, linear detrend, spliced at 1 Hz).

Two systematics travel with the comparison and are printed, not hidden:
  - the recorded Calibrate factors differ (0.80/0.80 today vs 0.83/0.82),
    a 2-4% gain-scale question that stays UNAPPLIED until the injected
    tone check settles which direction the factor goes;
  - today's record follows a sensor restart, so its DC carries warm-up.

Writes fig_plateau_4days.png next to this script.
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

FS = 1200.0
WIN_S = 900.0          # the canonical window; a longer run is cut to it
TARGET_FT = 50.0

RUNS = [
    ("08-03  N=8 state, t0+90 h", "result/ms1l_3_degaussed/relax0803_t90h_900s/relax0803_t90h",
     "#2E5FA3", 1.9, "-"),
    ("07-30  N=16 state, t0+25.6 h",
     "result/ms1l_3_degaussed/relax0730_t26h_900s/relax_t26h",
     "#C0504D", 1.9, "-"),
    ("07-30  N=16 state, t0+24.8 h (1800 s)",
     "result/ms1l_3_degaussed/relax0730_t25h_1800s/relax_t25h",
     "#E0A0A0", 1.2, "--"),
]

# run-to-run scatter established in the 08-01 audit: CV ~ 0.29 across
# nominally identical runs, well above the Welch-only 0.177 (chi-square
# p ~ 0.003), i.e. real day-to-day variation. A ratio of two runs
# therefore carries sigma_ln ~ 0.29*sqrt(2).
RUN_CV = 0.29


from bfield.analysis.spectra import spliced_asd  # noqa: E402 -- was an identical local copy


def band_value(f, a, f0):
    return a[np.argmin(np.abs(f - f0))]


def band_median(f, a, flo, fhi):
    m = (f >= flo) & (f <= fhi)
    return np.median(a[m])


def drift_pT_per_min(x, fs):
    """Slope of a straight line through the record."""
    t = np.arange(len(x)) / fs / 60.0
    return np.polyfit(t, x, 1)[0]


def style():
    plt.rcParams.update({
        "figure.dpi": 150, "font.size": 11, "axes.titleweight": "bold",
        "axes.spines.top": False, "axes.spines.right": False,
        "axes.edgecolor": "#7A7F85", "text.color": "#33373B",
        "legend.frameon": False,
    })


def main():
    style()
    fig, axes = plt.subplots(1, 2, figsize=(11.0, 4.4))
    print("ASD in fT/rtHz (band medians), drift in pT/min")
    # bands, not just the headline point: a uniform change and a change of
    # spectral shape mean different physics, and only the band table tells
    # them apart
    BANDS = ((0.01, 0.03), (0.03, 0.1), (0.1, 1.0), (1.0, 10.0),
             (30.0, 45.0))
    v005 = {}
    for ax, axis in zip(axes, ("Z", "Y")):
        print("\n=== %s axis      0.05 Hz  drift    " % axis
              + "  ".join("%9s" % ("%g-%g Hz" % b) for b in BANDS))
        for label, stem, color, lw, ls in RUNS:
            path = data_join(f"{stem}_{axis}_pT.txt")
            # first 900 s: a 1800 s run read whole is a different
            # statistic with a narrower error bar, not a longer
            # look at the same one (2026-08-19 review, H1)
            x = np.loadtxt(path)[:int(WIN_S * FS)]
            f, a = spliced_asd(x, FS)
            ax.loglog(f, a * 1e3, color=color, lw=lw, ls=ls, label=label)
            v005[(axis, label)] = band_value(f, a, 0.05) * 1e3
            print("%-38s %6.0f %7.1f    " % (label, v005[(axis, label)],
                                             drift_pT_per_min(x, FS))
                  + "  ".join("%9.1f" % (band_median(f, a, *b) * 1e3)
                              for b in BANDS))
        ax.axhspan(3e3, 8e3, color="#E8E2D4", alpha=0.5, zorder=0)
        ax.axhline(TARGET_FT, color="#4E7B52", lw=1.0, ls=":")
        ax.axvline(0.05, color="#B0B4B9", lw=0.8, ls=":", zorder=0)
        ax.set_xlim(5e-3, 100)
        ax.set_ylim(5, 3e5)
        ax.set_xlabel("frequency (Hz)")
        ax.set_title(f"{axis} axis")
    axes[0].set_ylabel("ASD (fT/$\\sqrt{\\mathrm{Hz}}$)")
    axes[0].text(5.6e-3, 3.4e3, "plateau band 3–8k", fontsize=9,
                 color="#8A8064")
    axes[0].text(5.6e-3, TARGET_FT * 1.3, "target 50 fT/√Hz", fontsize=9,
                 color="#4E7B52")
    axes[1].legend(loc="upper right", fontsize=8)

    # is today lower, or is this just the known run-to-run scatter?
    sigma_ln = RUN_CV * np.sqrt(2.0)
    print("\n0.05 Hz ratio, today / N=16 tail  (sigma_ln = %.2f from the "
          "measured run-to-run CV %.2f)" % (sigma_ln, RUN_CV))
    today, tail = RUNS[0][0], RUNS[1][0]
    for axis in ("Z", "Y"):
        r = v005[(axis, today)] / v005[(axis, tail)]
        n_sig = abs(np.log(r)) / sigma_ln
        verdict = "significant" if n_sig >= 2.0 else "NOT significant"
        print("  %s: %.2f  -> %.1f sigma, %s" % (axis, r, n_sig, verdict))
    fig.suptitle("Does the plateau hold for four days without a degauss?",
                 fontweight="bold")
    fig.tight_layout()
    out = os.path.join(HERE, "fig_plateau_4days.png")
    fig.savefig(out)
    print(f"saved {os.path.relpath(out, REPO)}")


if __name__ == "__main__":
    main()
