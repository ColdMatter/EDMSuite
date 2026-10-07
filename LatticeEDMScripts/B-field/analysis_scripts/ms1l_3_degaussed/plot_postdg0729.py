#!/usr/bin/env python3
"""The first sensor-side verdict on the MS-1L degauss (07-29 15:01).

Same head (AB3Q), same shield, 27 hours apart: before the 22.4 A-turn
degauss the Z output started 1.69 nT off and walked 168 pT/min; after it
the record starts at -14 pT and the drift settles to -1 pT/min inside
the run. The white band collapses from 89 to 23 fT/rtHz -- back to the
bench-shield level -- which is exactly what the f^-0.5 magnetization
attribution predicted a successful degauss would do. The 4-12 Hz band
barely moves, consistent with its identification as vibration.
Writes two figures next to this script:
  fig_postdg0729_field.png  -- absolute field
  fig_postdg0729_asd.png    -- noise spectrum
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

POST = "result/ms1l_3_degaussed/postdegauss0729_900s/postdg0729_Z_pT.txt"
POST_Y = "result/ms1l_3_degaussed/postdegauss0729_900s/postdg0729_Y_pT.txt"
PRE = "result/ms1l_2_degaussfail/swapref_oldhead_900s/swapref_old_Z_pT.txt"   # 07-28 12:13


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


def plot_field(pre, post, post_y):
    fig, ax = plt.subplots(figsize=(6.0, 4.2))
    t, b = blocks(pre, FS)
    ax.plot(t, b, color="#9AA0A6", lw=1.8, label="Z before")
    t, b = blocks(post, FS)
    ax.plot(t, b, color="#C0504D", lw=1.8, label="Z after")
    t, b = blocks(post_y, FS)
    ax.plot(t, b, color="#D89A96", lw=1.3, ls="--", label="Y after")
    ax.axhline(0, color="#7A7F85", lw=0.7)
    ax.annotate("−14 pT → −1 pT/min",
                xy=(11, 95), xytext=(3.5, 650), fontsize=10,
                color="#7A2E2C",
                arrowprops=dict(arrowstyle="->", color="#7A2E2C", lw=0.9))
    ax.set_xlabel("time (min)")
    ax.set_ylabel("field (pT)")
    ax.set_title("MS-1L Z (AB3Q): absolute field after degauss")
    ax.legend(loc="center right", fontsize=10)
    fig.tight_layout()
    out = os.path.join(HERE, "fig_postdg0729_field.png")
    fig.savefig(out, bbox_inches="tight")
    plt.close(fig)
    print("wrote", out)


def plot_asd(pre, post):
    fig, ax = plt.subplots(figsize=(6.0, 4.2))
    for lab, x, col, lw in (
            ("Z before", pre, "#9AA0A6", 1.6),
            ("Z after", post, "#C0504D", 1.6)):
        f, a = spliced_asd(x * 1e3, FS)
        ax.loglog(f, a, color=col, lw=lw, label=lab)
    ax.axhline(TARGET_FT, color="#33373B", ls="--", lw=1.0, alpha=0.75)
    ax.text(3e-2, TARGET_FT * 1.25, "50 fT target", fontsize=9, va="bottom")
    ax.axhspan(18, 22, color="#DDE8DD", zorder=0)
    ax.text(3e-2, 16.5, "bench floor", fontsize=8.5, color="#3A5A3A",
            va="top")
    # full-record values (canonical window); the settled second half of the
    # post run gives 5.8k -- quoted elsewhere only with the window stated
    ax.annotate("0.05 Hz:\n37k → 7.9k",
                xy=(0.05, 7855), xytext=(0.003, 180), fontsize=9.5,
                color="#7A2E2C", ha="left",
                arrowprops=dict(arrowstyle="->", color="#7A2E2C", lw=0.9))
    ax.set_xlim(2e-3, 100)
    ax.set_ylim(10, 2e5)
    ax.set_xlabel("frequency (Hz)")
    ax.set_ylabel("ASD (fT/√Hz)")
    ax.set_title("MS-1L Z (AB3Q): noise spectrum after degauss")
    ax.legend(loc="upper right", fontsize=10)
    fig.tight_layout()
    out = os.path.join(HERE, "fig_postdg0729_asd.png")
    fig.savefig(out, bbox_inches="tight")
    plt.close(fig)
    print("wrote", out)


def main():
    style()
    post = np.loadtxt(data_join(POST))
    post_y = np.loadtxt(data_join(POST_Y))
    pre = np.loadtxt(data_join(PRE))
    plot_field(pre, post, post_y)
    plot_asd(pre, post)


if __name__ == "__main__":
    main()
