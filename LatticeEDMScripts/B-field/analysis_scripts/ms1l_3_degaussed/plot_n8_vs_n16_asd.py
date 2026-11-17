#!/usr/bin/env python3
"""Does the winding turn count change the post-degauss spectrum?

Three windings, three doses, measured shortly after the taper ended:
  N=16, 1.4 A x 16 = 22.4 A-turns (3 Hz), t0+46 min (07-29 15:01)
  N=8, 5 Hz pass 3.2 A = 25.6 A-turns then 3 Hz finish 2.6 A = 20.8,
       t0+17 min and t0+40 min (07-30 17:12 / 17:36)
  N=4, 4.5 A x 4 = 18 A-turns (3 Hz), t0+16 min (08-03 15:06; t0 read
       off the 14:51 screenshot, taper end ~14:50)

The dose spread is only 18-25.6 A-turns, so this compares windings at a
nearly constant dose rather than mapping a dose-response curve.

Elapsed time is matched where it matters: N=4 at +16 min against N=8 at
+17 min is the like-for-like pair; N=16 at +46 min against N=8 at
+40 min is the other. Caveat carried in the caption: the N=8 and N=4
runs followed a sensor restart and a fresh Field Zero, so their DC
carries warm-up systematics; the ASD comparison above ~0.01 Hz is the
meaningful part.

Writes fig_n8_vs_n16_asd.png next to this script and prints the
0.05 Hz and white-band numbers with the run-to-run scatter attached.
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
TARGET_FT = 50.0

RUNS = [
    ("N=16  22.4 A·t   t0+46 min",
     "result/ms1l_3_degaussed/postdegauss0729_900s/postdg0729_Z_pT.txt",
     "#C0504D", 1.8, "-"),
    ("N=8   20.8–25.6 A·t   t0+40 min",
     "result/ms1l_3_degaussed/postdg0730_n8_2nd_900s/postdg_n8_2nd_Z_pT.txt",
     "#2E5FA3", 1.8, "-"),
    ("N=8   same, t0+17 min",
     "result/ms1l_3_degaussed/postdg0730_n8_900s/postdg_n8_Z_pT.txt",
     "#9DC3E6", 1.3, "--"),
    ("N=4   18 A·t   t0+16 min",
     "result/ms1l_3_degaussed/postdg0803_n4_900s/postdg_n4_Z_pT.txt",
     "#4E7B52", 1.8, "-"),
]

# run-to-run scatter measured in the 08-01 audit: CV ~ 0.29 across
# nominally identical runs (chi-square p ~ 0.003 against Welch-only),
# so a ratio of two runs carries sigma_ln ~ 0.29*sqrt(2) = 0.41
RUN_CV = 0.29


from bfield.analysis.spectra import spliced_asd  # noqa: E402 -- was an identical local copy


def band_value(f, a, f0):
    return a[np.argmin(np.abs(f - f0))]


def band_median(f, a, flo, fhi):
    m = (f >= flo) & (f <= fhi)
    return np.median(a[m])


def style():
    plt.rcParams.update({
        "figure.dpi": 150, "font.size": 11, "axes.titleweight": "bold",
        "axes.spines.top": False, "axes.spines.right": False,
        "axes.edgecolor": "#7A7F85", "text.color": "#33373B",
        "legend.frameon": False,
    })


def main():
    style()
    fig, ax = plt.subplots(figsize=(6.4, 4.4))
    print("run                                0.05 Hz     white 30-45 Hz")
    got = {}
    for label, rel, color, lw, ls in RUNS:
        x = np.loadtxt(data_join(rel))
        f, a = spliced_asd(x, FS)
        ax.loglog(f, a * 1e3, color=color, lw=lw, ls=ls, label=label)
        v005 = band_value(f, a, 0.05) * 1e3
        vwhite = band_median(f, a, 30.0, 45.0) * 1e3
        got[label] = v005
        print(f"{label:<34s} {v005:8.0f} fT   {vwhite:6.1f} fT/rtHz "
              f"(white 30-45 Hz, clear of the 12-25 Hz vibration forest)")

    # "no material difference" has to be stated against the known scatter,
    # not asserted from the eye
    sigma_ln = RUN_CV * np.sqrt(2.0)
    labels = [r[0] for r in RUNS]
    print(f"\n0.05 Hz pairwise ratios (sigma_ln = {sigma_ln:.2f} from the "
          f"measured run-to-run CV {RUN_CV})")
    for i in range(len(labels)):
        for j in range(i + 1, len(labels)):
            r = got[labels[i]] / got[labels[j]]
            n = abs(np.log(r)) / sigma_ln
            print(f"  {labels[i][:12]:<13s} / {labels[j][:12]:<13s} "
                  f"{r:5.2f}  -> {n:.1f} sigma")
    lo, hi = min(got.values()), max(got.values())
    print(f"\nspread across all four runs: {lo:.0f}-{hi:.0f} fT "
          f"(max/min {hi / lo:.2f} = {abs(np.log(hi / lo)) / sigma_ln:.1f} sigma)")
    ax.axhspan(3e3, 8e3, color="#E8E2D4", alpha=0.5, zorder=0)
    ax.text(5.6e-3, 3.4e3, "plateau band 3–8k", fontsize=9,
            color="#8A8064")
    ax.axhline(TARGET_FT, color="#4E7B52", lw=1.0, ls=":")
    ax.text(5.6e-3, TARGET_FT * 1.25, "lattice eEDM target 50 fT/√Hz",
            fontsize=9, color="#4E7B52")
    ax.axvline(0.05, color="#B0B4B9", lw=0.8, ls=":", zorder=0)
    ax.set_xlim(5e-3, 100)
    ax.set_xlabel("frequency (Hz)")
    ax.set_ylabel("ASD (fT/$\\sqrt{\\mathrm{Hz}}$)")
    ax.set_title("Post-degauss Z noise: windings N=16, N=8, N=4")
    ax.legend(loc="upper right", fontsize=9)
    fig.tight_layout()
    out = os.path.join(HERE, "fig_n8_vs_n16_asd.png")
    fig.savefig(out)
    print(f"saved {os.path.relpath(out, REPO)}")


if __name__ == "__main__":
    main()
