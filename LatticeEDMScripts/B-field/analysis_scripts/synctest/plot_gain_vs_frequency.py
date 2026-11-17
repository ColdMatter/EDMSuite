#!/usr/bin/env python3
"""How well a single gain (2.624 V/nT) matches the analog record to the GUI
across the whole band, not just at the 50 Hz line where it was fixed.

The GUI is taken as truth (QuSpin's own calibration) and the analog pT
record, written with the nominal 2.7 V/nT, is rescaled to 2.624 by
x2.7/2.624 = x1.0290. The lower panel is the magnitude ratio
analog/GUI: where the two see the same signal it should sit at 1.0.
The middle panel is the coherence, which says where the ratio is a real
common-signal comparison and where it is just two independent noises.

Only the low-frequency drift band carries a real common field, so that
is the only honest test of the gain. The narrow lines (50 / 100 / 59.9
Hz) are shown but are unreliable: GUI timing jitter smears them and the
mains-family pickup is partly path-dependent (different in the analog
cabling than in the sensor field), so their ratio swings run to run.
Writes fig_gain_vs_frequency.png next to this script.
"""

import os

import numpy as np
from scipy.signal import welch, coherence, resample_poly

import matplotlib
matplotlib.use("Agg")
import matplotlib.pyplot as plt

HERE = os.path.dirname(os.path.abspath(__file__))
import sys as _sys  # analysis_scripts/ on sys.path for _paths
_sys.path.insert(0, os.path.dirname(os.path.dirname(os.path.abspath(__file__))))
from _paths import DATA_ROOT, PROJECT_DIR, ANALYSIS_DIR, data, data_join, rel_data  # noqa: E402,F401
REPO = PROJECT_DIR

FS_A = 1200.0
FS_G = 203.18
SETTLE_S = 500.0
GAIN_FILE = 2.7          # written into the analog pT files
GAIN_ADOPT = 2.624       # the 50 Hz value we want to test across the band
SCALE = GAIN_FILE / GAIN_ADOPT

DAQ = "result/synctest/sync5_30min/sync5daq_Z_pT.txt"
GUI = "result/synctest/sync5_30min/gui_sync5_Formatted.txt"


def asd(x, fs, seg_s):
    f, p = welch(x, fs=fs, nperseg=int(seg_s * fs), detrend="linear")
    return f, np.sqrt(p)


def main():
    a = np.loadtxt(data_join(DAQ), skiprows=1)[int(SETTLE_S * FS_A):] * SCALE
    g = np.loadtxt(data_join(GUI), skiprows=1)[int(SETTLE_S * FS_G):]

    fa, Aa = asd(a, FS_A, 100.0)
    fg, Ag = asd(g, FS_G, 100.0)
    Aa_g = np.interp(fg, fa, Aa)
    ratio = Aa_g / Ag

    a_ds = resample_poly(a, int(round(FS_G)), int(round(FS_A)))
    n = min(len(a_ds), len(g))
    fc, coh = coherence(a_ds[:n], g[:n], fs=FS_G, nperseg=int(50 * FS_G))

    def bandmed(lo, hi):
        m = (fg >= lo) & (fg <= hi)
        return float(np.median(ratio[m]))

    lf = bandmed(0.02, 0.5)

    plt.rcParams.update({
        "figure.dpi": 150, "font.size": 10.5, "axes.titleweight": "bold",
        "axes.spines.top": False, "axes.spines.right": False,
        "axes.edgecolor": "#7A7F85", "text.color": "#33373B",
        "legend.frameon": False,
    })
    fig, (ax0, axc, ax1) = plt.subplots(
        3, 1, figsize=(8.4, 8.0), sharex=True,
        gridspec_kw={"height_ratios": [2.2, 1.0, 1.8], "hspace": 0.12})

    ax0.loglog(fg[fg > 0], Ag[fg > 0] * 1e3, lw=0.8, color="#2E6E8E",
               label="GUI Z (truth)")
    ax0.loglog(fg[fg > 0], Aa_g[fg > 0] * 1e3, lw=0.8, color="#C0504D",
               label="analog Z, rescaled to 2.624 V/nT")
    ax0.set_ylabel("ASD [fT/rtHz]")
    ax0.legend(loc="lower left", fontsize=9)
    ax0.set_title("does one gain (2.624 V/nT) match analog to GUI across the band?")

    axc.semilogx(fc[fc > 0], coh[fc > 0], lw=0.9, color="#5F6368")
    axc.axhspan(0.5, 1.0, color="#DFF0D8", alpha=0.5, zorder=0)
    axc.set_ylabel("coherence")
    axc.set_ylim(0, 1)
    axc.annotate("real common signal\n(ratio is meaningful)", (0.03, 0.78),
                 fontsize=8, color="#3C763D")
    axc.annotate("independent noise -> ratio just compares two noise floors",
                 (0.6, 0.12), fontsize=8, color="#8A8A8A")

    ax1.semilogx(fg[fg > 0], ratio[fg > 0], lw=0.7, color="#7A7F85", alpha=0.7)
    ax1.axhline(1.0, color="#33373B", ls="--", lw=0.9)
    ax1.axvspan(0.02, 0.5, color="#E6E8EA", alpha=0.7, zorder=0)
    ax1.annotate(f"LF drift band: ratio {lf:.3f}  ({(lf - 1) * 100:+.1f}%)\n"
                 "the only honest gain test",
                 (0.024, 1.8), fontsize=8.5, color="#33373B")
    for f0, txt in [(50.0, "50 Hz"), (59.9, "59.9 comb"), (100.0, "100 Hz")]:
        ax1.axvline(f0, color="#C0504D", ls=":", lw=0.8, alpha=0.6)
        ax1.annotate(txt, (f0, 2.6), fontsize=7.5, color="#C0504D",
                     ha="center", rotation=90, va="top")
    ax1.set_ylim(0.3, 3.0)
    ax1.set_ylabel("ratio  analog / GUI")
    ax1.set_xlabel("frequency [Hz]")

    out = os.path.join(HERE, "fig_gain_vs_frequency.png")
    fig.savefig(out, bbox_inches="tight")
    print(f"[out ] {out}")
    print(f"[num ] LF 0.02-0.5 Hz ratio {lf:.3f} ({(lf - 1) * 100:+.1f}%)  "
          f"floor 3-40 Hz {bandmed(3, 40):.3f}")


if __name__ == "__main__":
    main()
