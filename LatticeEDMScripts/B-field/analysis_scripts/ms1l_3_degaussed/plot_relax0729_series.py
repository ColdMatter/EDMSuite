#!/usr/bin/env python3
"""Absolute field and noise spectrum: before the degauss vs the 07-29 series.

Z axis of the same head (AB3Q) throughout.
Absolute field: the pre-degauss record walks 2.7 nT across the window;
the three post-degauss runs live inside +/-100 pT (inset).
ASD: within this one campaign the 0.05 Hz level reads 7.9k -> 5.1k ->
3.8k over the first two hours and 6.7k a day later, a plateau near
4-7k fT/rtHz. The white band stays at the bench-era level throughout,
and the day-after DC is fully settled (the 30 min record moves ~20 pT).
What remains at low frequency is fluctuation, not drift.

Note added 2026-08-03: the descent over the first two hours does NOT
reproduce across campaigns -- the N=8 series rose over the same window
and the N=4 series was flat in Z. Pooled over three campaigns the
0.05 Hz level is inside the measured run-to-run scatter from t0+16 min
to t0+90 h. See plot_relax_all.py, which is the canonical view; this
script stays as the within-campaign 07-29 record.
Writes three figures next to this script:
  fig_relax0729_field.png  -- absolute field
  fig_relax0729_asd.png    -- noise spectrum
  fig_relax0729_trend.png  -- 0.05 Hz level vs time since the cycle
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

# (label, path, colour, lw)
RUNS = (
    ("before",
     "result/ms1l_2_degaussfail/swapref_oldhead_900s/swapref_old_Z_pT.txt",
     "#9AA0A6", 1.8),
    ("+46 min",
     "result/ms1l_3_degaussed/postdegauss0729_900s/postdg0729_Z_pT.txt",
     "#E2A9A5", 1.3),
    ("+80 min",
     "result/ms1l_3_degaussed/relax0729_t80m_900s/relax_t80m_Z_pT.txt",
     "#CD7B76", 1.3),
    ("+104 min",
     "result/ms1l_3_degaussed/relax0729_t104m_900s/relax_t104m_Z_pT.txt",
     "#A33B36", 1.3),
    ("+24.8 h",
     "result/ms1l_3_degaussed/relax0730_t25h_1800s/relax_t25h_Z_pT.txt",
     "#5B1F1C", 1.4),
    ("+25.6 h",
     "result/ms1l_3_degaussed/relax0730_t26h_900s/relax_t26h_Z_pT.txt",
     "#2E0F0D", 1.1),
)

# the whole series, 0.05 Hz vs time since the cycle end (minutes, both axes)
# (minutes after the degauss, Y, Z) at 0.05 Hz in fT/rtHz. Frozen
# numbers, not recomputed from the series this script loads: the
# 900 s rows still match today's Welch to the digit, but the
# 1800 s row (1488) matched neither the full record (6875/5295)
# nor its first 900 s (6298/5899) and is now the latter, which is
# the canonical window (2026-08-19 analysis review, M2).
TREND = (
    (46, 4194, 7855), (80, 6124, 5069), (104, 2859, 3776),
    (1488, 5899, 6298), (1536, 5800, 8363),
)


from bfield.analysis.spectra import spliced_asd  # noqa: E402 -- was an identical local copy


def blocks(x, fs, seg_s=30.0):
    k = int(seg_s * fs)
    n = len(x) // k
    return (np.arange(n) + 0.5) * seg_s / 60.0, \
        x[: n * k].reshape(n, k).mean(axis=1)


def style():
    plt.rcParams.update({
        "figure.dpi": 150, "font.size": 11, "axes.titleweight": "bold",
        "axes.spines.top": False, "axes.spines.right": False,
        "axes.edgecolor": "#7A7F85", "text.color": "#33373B",
        "legend.frameon": False,
    })


def plot_field(data):
    fig, ax = plt.subplots(figsize=(6.0, 4.2))
    for lab, x, col, lw in data:
        t, b = blocks(x, FS)
        ax.plot(t, b, color=col, lw=lw, label=lab)
    ax.axhline(0, color="#7A7F85", lw=0.7)
    ax.set_xlabel("time (min)")
    ax.set_ylabel("field (pT)")
    ax.set_title("MS-1L Z (AB3Q): absolute field, relaxation series")
    ax.annotate("walks 2.7 nT", xy=(10, -1100), fontsize=10,
                color="#5A5F65", ha="center")
    ax.legend(loc="lower left", fontsize=9)

    axi = ax.inset_axes([0.48, 0.55, 0.48, 0.40])
    for lab, x, col, lw in data[1:]:
        t, b = blocks(x, FS)
        axi.plot(t, b, color=col, lw=lw)
    axi.axhline(0, color="#7A7F85", lw=0.5)
    axi.set_ylim(-110, 110)
    axi.set_xlim(0, 15)
    axi.set_title("after (zoom)", fontsize=9)
    axi.tick_params(labelsize=8)
    axi.set_xticks([0, 5, 10, 15])

    fig.tight_layout()
    out = os.path.join(HERE, "fig_relax0729_field.png")
    fig.savefig(out, bbox_inches="tight")
    plt.close(fig)
    print("wrote", out)


def plot_asd(data):
    fig, ax = plt.subplots(figsize=(6.0, 4.2))
    for lab, x, col, lw in data:
        f, a = spliced_asd(x * 1e3, FS)
        ax.loglog(f, a, color=col, lw=lw,
                  alpha=0.95 if lab == "before" else 0.85, label=lab)
    ax.axhline(TARGET_FT, color="#33373B", ls="--", lw=1.0, alpha=0.75)
    ax.text(3e-2, TARGET_FT * 1.25, "50 fT target", fontsize=9, va="bottom")
    ax.axhspan(18, 22, color="#DDE8DD", zorder=0)
    ax.text(3e-2, 16.5, "bench floor", fontsize=8.5, color="#3A5A3A",
            va="top")
    ax.annotate("0.05 Hz: 37k → 3.8k,\nthen a 4-7k plateau",
                xy=(0.05, 3800), xytext=(0.003, 180), fontsize=9.5,
                color="#7A2E2C", ha="left",
                arrowprops=dict(arrowstyle="->", color="#7A2E2C", lw=0.9))
    ax.set_xlim(2e-3, 100)
    ax.set_ylim(10, 2e5)
    ax.set_xlabel("frequency (Hz)")
    ax.set_ylabel("ASD (fT/√Hz)")
    ax.set_title("MS-1L Z (AB3Q): noise spectrum, relaxation series")
    ax.legend(loc="upper right", fontsize=9)
    fig.tight_layout()
    out = os.path.join(HERE, "fig_relax0729_asd.png")
    fig.savefig(out, bbox_inches="tight")
    plt.close(fig)
    print("wrote", out)


def plot_trend():
    fig, ax = plt.subplots(figsize=(6.0, 4.2))
    t = [row[0] / 60.0 for row in TREND]
    ax.axhspan(3000, 8500, color="#F3E6E5", zorder=0)
    ax.text(1.15, 3300, "plateau band 3-8k", fontsize=9, color="#7A2E2C")
    ax.loglog(t, [r[2] for r in TREND], "o-", color="#A33B36", lw=1.3,
              ms=6, label="Z")
    ax.loglog(t, [r[1] for r in TREND], "s--", color="#D89A96", lw=1.1,
              ms=5, label="Y")
    ax.axhline(37058, color="#9AA0A6", ls=":", lw=1.0)
    ax.text(0.8, 41000, "before the degauss (37k)", fontsize=9,
            color="#5A5F65")
    ax.axhline(TARGET_FT, color="#33373B", ls="--", lw=0.9)
    ax.text(0.8, 58, "50 fT target", fontsize=9)
    ax.set_xlabel("time since the cycle end (h)")
    ax.set_ylabel("ASD at 0.05 Hz (fT/" + chr(0x221a) + "Hz)")
    ax.set_ylim(30, 6e4)
    ax.set_title("MS-1L: 0.05 Hz level after the degauss")
    ax.legend(loc="center right", fontsize=9)
    fig.tight_layout()
    out = os.path.join(HERE, "fig_relax0729_trend.png")
    fig.savefig(out, bbox_inches="tight")
    plt.close(fig)
    print("wrote", out)


def main():
    style()
    data = [(lab, np.loadtxt(data_join(rel)), col, lw)
            for lab, rel, col, lw in RUNS]
    plot_field(data)
    plot_asd(data)
    plot_trend()


if __name__ == "__main__":
    main()
