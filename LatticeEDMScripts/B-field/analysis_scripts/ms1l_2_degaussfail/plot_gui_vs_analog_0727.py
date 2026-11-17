"""GUI (digital) vs analog (USB-6008) on the 2026-07-27 runs; makes the figure.

Run from the repository root:
    python analysis/ms1l_2_degaussfail/plot_gui_vs_analog_0727.py
Reads result/ms1l_2_degaussfail/dgnoise_{on,off}_60s/ and writes
analysis/ms1l_2_degaussfail/gui_vs_analog_0727.png.

Everything is put on the GUI's own 203 Hz time base and the analog (dense
at 1200 Hz) is interpolated onto it, so the resampling kernel costs <2 % at
80 Hz instead of the 22 % that resampling the GUI upward would cost.  Three
corrections are applied in order: the GUI clock rate, the record-start lag,
and the fixed analog-vs-digital group delay.  What remains is the answer.

The three constants per run in RUNS below are fitted, not assumed:
  fs_gui, lag  from a normalised cross-correlation of the two records on a
               common 20 Hz grid, high-passed with a centred boxcar; the lag
               is refined on a 1 ms grid and the clock rate comes from how
               that lag walks between an early and a late window.
  tau          from the slope of the cross-spectral phase over the bins with
               coherence > 0.9 in 0.5-60 Hz, after the first two are removed.
Re-fit them if the run set changes.  Leaving tau out is what made the
12-35 Hz regression slope collapse to 0.28 while the coherence stayed at
0.99 -- a phase shift read as a gain error.
"""
import os
import sys

import numpy as np
import matplotlib
matplotlib.use("Agg")
import matplotlib.pyplot as plt
from scipy.signal import welch, coherence, butter, sosfiltfilt

HERE = os.path.dirname(os.path.abspath(__file__))
sys.path.insert(0, os.path.dirname(HERE))  # analysis_scripts/ for _paths
from _paths import rel_data  # noqa: E402

FS_A = 1200.0
FC = 90.0            # anti-alias before landing on the 203 Hz grid
LSB_PT = 512e-6 * 381.0975609756097
RUNS = [
    ("DG unit ON, 15:17", "result/ms1l_2_degaussfail/dgnoise_on_60s/dgnoise_on_daq_Z_pT.txt",
     "result/ms1l_2_degaussfail/dgnoise_on_60s/gui_dgnoise_on_Formatted.txt",
     203.113, -4.465, 7.24e-3),
    ("DG unit OFF, 15:30", "result/ms1l_2_degaussfail/dgnoise_off_60s/dgnoise_off_daq_Z_pT.txt",
     "result/ms1l_2_degaussfail/dgnoise_off_60s/gui_dgnoise_off_Formatted.txt",
     203.151, -2.983, 8.63e-3),
]
BANDS = [(0.05, 0.2), (0.2, 0.5), (0.5, 1.5), (1.5, 4), (4, 12), (12, 35),
         (35, 70)]

fig, axes = plt.subplots(2, 2, figsize=(11, 7.5))
store = []
for idx, (tag, pa, pg, fs_g, lag, tau) in enumerate(RUNS):
    a = np.loadtxt(rel_data(pa))
    g = np.loadtxt(rel_data(pg), skiprows=1)
    ta = np.arange(len(a)) / FS_A
    # the analog carries signal to 600 Hz; without this the DG unit's 335 Hz
    # line folds to 71 Hz on the 203 Hz grid and lands in the residual
    a_lp = sosfiltfilt(butter(8, FC / (FS_A / 2), output="sos"), a)

    # GUI samples that fall inside the analog record, with the delay removed
    k = np.arange(len(g))
    u = k / fs_g
    keep = (u - lag + tau >= 1.0) & (u - lag + tau <= ta[-1] - 1.0)
    gk = g[keep]
    ak = np.interp(u[keep] - lag + tau, ta, a_lp)
    n = len(gk)

    slope, off = np.polyfit(gk, ak, 1)
    resid = ak - (slope * gk + off)
    nps = int(10 * fs_g)
    f, pa_ = welch(ak, fs=fs_g, nperseg=nps, detrend="linear")
    _, pg_ = welch(gk, fs=fs_g, nperseg=nps, detrend="linear")
    _, cxy = coherence(gk, ak, fs=fs_g, nperseg=nps)
    _, pr_ = welch(resid, fs=fs_g, nperseg=nps, detrend="linear")

    print(f"\n=== {tag} ===  {n} matched samples ({n/fs_g:.1f} s)")
    print(f"   scale  analog/GUI = {slope:.4f}   offset {off:+.2f} pT")
    print(f"   means  analog {ak.mean():+.2f}  GUI {gk.mean():+.2f} pT   "
          f"(difference {ak.mean()-gk.mean():+.2f} pT)")
    print(f"   sd     analog {ak.std():.2f}  GUI {gk.std():.2f} pT")
    print(f"   residual after scale+offset: {resid.std():.2f} pT rms "
          f"= {resid.std()/gk.std()*100:.1f} % of the signal, "
          f"{resid.std()/LSB_PT:.1f} DAQ LSB")
    print("   band Hz       ASDa      ASDg    ratio   coher   resid ASD")
    for lo, hi in BANDS:
        b = (f >= lo) & (f < hi)
        if not b.any():
            continue
        print(f"   {lo:5.2f}-{hi:<5.0f}{np.median(np.sqrt(pa_[b]))*1e3:9.0f}"
              f"{np.median(np.sqrt(pg_[b]))*1e3:10.0f}"
              f"{np.median(np.sqrt(pa_[b]/pg_[b])):9.3f}"
              f"{np.median(cxy[b]):8.3f}"
              f"{np.median(np.sqrt(pr_[b]))*1e3:12.0f}")
    store.append((tag, u[keep] - lag, gk, ak, f, pa_, pg_, pr_, cxy))

    c = f"C{idx}"
    ax = axes[0, 0]
    m = (u[keep] - lag >= 20) & (u[keep] - lag < 40)
    ax.plot(u[keep][m] - lag, gk[m] - gk[m].mean(), c, lw=1.1,
            label=f"{tag.split(',')[0]} GUI")
    ax.plot(u[keep][m] - lag, ak[m] - ak[m].mean(), "k", lw=0.7, alpha=0.7,
            label="analog" if idx == 0 else None)

    ax = axes[0, 1]
    m = (u[keep] - lag >= 30) & (u[keep] - lag < 32)
    ax.plot(u[keep][m] - lag, gk[m] - gk[m].mean(), c + "-", lw=1.4)
    ax.plot(u[keep][m] - lag, ak[m] - ak[m].mean(), "k--", lw=1.0)

    axes[1, 0].loglog(f[1:], np.sqrt(pg_[1:]) * 1e3, c, lw=1.0, label=f"{tag} GUI")
    axes[1, 0].loglog(f[1:], np.sqrt(pa_[1:]) * 1e3, c, lw=0.8, ls="--",
                      label=f"{tag} analog")
    axes[1, 0].loglog(f[1:], np.sqrt(pr_[1:]) * 1e3, c, lw=0.8, ls=":",
                      alpha=0.7, label=f"{tag} difference")
    axes[1, 1].semilogx(f[1:], np.sqrt(pa_[1:] / pg_[1:]), c, lw=0.9,
                        alpha=0.8, label=tag)

axes[0, 0].set(xlabel="time (s)", ylabel="B$_z$ - mean (pT)",
               title="20 s of the matched record")
axes[0, 0].legend(fontsize=7)
axes[0, 1].set(xlabel="time (s)", ylabel="B$_z$ - mean (pT)",
               title="2 s zoom (solid = GUI, dashed = analog)")
axes[1, 0].set(xlabel="frequency (Hz)", ylabel="ASD (fT/$\\sqrt{Hz}$)",
               title="both streams and their difference")
axes[1, 0].legend(fontsize=6)
axes[1, 0].grid(alpha=0.3, which="both")
axes[1, 1].axhline(1.0, color="k", lw=0.6)
axes[1, 1].axhspan(0.95, 1.05, color="k", alpha=0.08)
axes[1, 1].set(xlabel="frequency (Hz)", ylabel="analog / GUI",
               ylim=(0.5, 2.0), title="amplitude ratio (shaded = +-5 %)")
axes[1, 1].legend(fontsize=7)
axes[1, 1].grid(alpha=0.3, which="both")
fig.suptitle("QZFM digital stream vs USB-6008 analog record, B$_z$, "
             "2026-07-27 (clock, start lag and 8 ms delay removed)")
fig.tight_layout()
out = os.path.join(HERE, "gui_vs_analog_0727.png")
fig.savefig(out, dpi=140)
print(f"\nfigure -> {out}")
