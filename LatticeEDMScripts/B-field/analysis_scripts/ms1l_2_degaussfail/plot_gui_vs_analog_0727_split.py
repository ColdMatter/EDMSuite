#!/usr/bin/env python3
"""GUI (digital) vs analog (USB-6008) on the 2026-07-27 runs -- split figures.

Same alignment as plot_gui_vs_analog_0727.py (GUI clock, start lag, fixed
group delay). Leaves the combined figure alone; writes four separate
images next to this script:
  fig_gui_vs_analog_0727_timeseries.png
  fig_gui_vs_analog_0727_zoom.png
  fig_gui_vs_analog_0727_asd.png
  fig_gui_vs_analog_0727_ratio.png

Run from the repository root:
    .venv/bin/python analysis/ms1l_2_degaussfail/plot_gui_vs_analog_0727_split.py
"""

import os

import numpy as np
from scipy.signal import welch, coherence, butter, sosfiltfilt

import matplotlib
matplotlib.use("Agg")
import matplotlib.pyplot as plt

HERE = os.path.dirname(os.path.abspath(__file__))
import sys as _sys  # analysis_scripts/ on sys.path for _paths
_sys.path.insert(0, os.path.dirname(os.path.dirname(os.path.abspath(__file__))))
from _paths import DATA_ROOT, PROJECT_DIR, ANALYSIS_DIR, data, data_join, rel_data  # noqa: E402,F401
REPO = PROJECT_DIR

FS_A = 1200.0
FC = 90.0
LSB_PT = 512e-6 * 381.0975609756097

RUNS = [
    ("DG ON", "result/ms1l_2_degaussfail/dgnoise_on_60s/dgnoise_on_daq_Z_pT.txt",
     "result/ms1l_2_degaussfail/dgnoise_on_60s/gui_dgnoise_on_Formatted.txt",
     203.113, -4.465, 7.24e-3, "#2E6E8E"),
    ("DG OFF", "result/ms1l_2_degaussfail/dgnoise_off_60s/dgnoise_off_daq_Z_pT.txt",
     "result/ms1l_2_degaussfail/dgnoise_off_60s/gui_dgnoise_off_Formatted.txt",
     203.151, -2.983, 8.63e-3, "#C07030"),
]
BANDS = [(0.05, 0.2), (0.2, 0.5), (0.5, 1.5), (1.5, 4), (4, 12), (12, 35),
         (35, 70)]


def style():
    plt.rcParams.update({
        "figure.dpi": 150, "font.size": 11, "axes.titleweight": "bold",
        "axes.spines.top": False, "axes.spines.right": False,
        "axes.edgecolor": "#7A7F85", "text.color": "#33373B",
        "legend.frameon": False,
    })


def save(fig, name):
    out = os.path.join(HERE, name)
    fig.savefig(out, bbox_inches="tight")
    plt.close(fig)
    print("wrote", out)


def load_matched(pa, pg, fs_g, lag, tau):
    a = np.loadtxt(data_join(pa))
    g = np.loadtxt(data_join(pg), skiprows=1)
    ta = np.arange(len(a)) / FS_A
    a_lp = sosfiltfilt(butter(8, FC / (FS_A / 2), output="sos"), a)
    k = np.arange(len(g))
    u = k / fs_g
    keep = (u - lag + tau >= 1.0) & (u - lag + tau <= ta[-1] - 1.0)
    gk = g[keep]
    ak = np.interp(u[keep] - lag + tau, ta, a_lp)
    t = u[keep] - lag
    return t, gk, ak, fs_g


def spectra(gk, ak, fs_g):
    slope, off = np.polyfit(gk, ak, 1)
    resid = ak - (slope * gk + off)
    nps = int(10 * fs_g)
    f, pa_ = welch(ak, fs=fs_g, nperseg=nps, detrend="linear")
    _, pg_ = welch(gk, fs=fs_g, nperseg=nps, detrend="linear")
    _, cxy = coherence(gk, ak, fs=fs_g, nperseg=nps)
    _, pr_ = welch(resid, fs=fs_g, nperseg=nps, detrend="linear")
    return slope, off, resid, f, pa_, pg_, pr_, cxy


def plot_timeseries(runs):
    fig, ax = plt.subplots(figsize=(6.0, 4.2))
    for tag, t, gk, ak, col in runs:
        m = (t >= 20) & (t < 40)
        ax.plot(t[m], gk[m] - gk[m].mean(), color=col, lw=1.3,
                label=f"{tag} GUI")
        ax.plot(t[m], ak[m] - ak[m].mean(), color="#9AA0A6", lw=0.8,
                alpha=0.7, label="analog" if tag == "DG ON" else None)
    ax.set_xlabel("time (s)")
    ax.set_ylabel(r"$B_z$ − mean (pT)")
    ax.set_title("GUI vs analog: 20 s matched record")
    ax.legend(loc="best", fontsize=9)
    fig.tight_layout()
    save(fig, "fig_gui_vs_analog_0727_timeseries.png")


def plot_zoom(runs):
    fig, ax = plt.subplots(figsize=(6.0, 4.2))
    for tag, t, gk, ak, col in runs:
        m = (t >= 30) & (t < 32)
        ax.plot(t[m], gk[m] - gk[m].mean(), color=col, lw=1.4,
                label=f"{tag} GUI")
        ax.plot(t[m], ak[m] - ak[m].mean(), color=col, lw=1.0, ls="--",
                alpha=0.85, label=f"{tag} analog")
    ax.set_xlabel("time (s)")
    ax.set_ylabel(r"$B_z$ − mean (pT)")
    ax.set_title("GUI vs analog: 2 s zoom")
    ax.legend(loc="best", fontsize=9)
    fig.tight_layout()
    save(fig, "fig_gui_vs_analog_0727_zoom.png")


def plot_asd(specs):
    fig, ax = plt.subplots(figsize=(6.0, 4.2))
    for tag, f, pa_, pg_, pr_, col in specs:
        ax.loglog(f[1:], np.sqrt(pg_[1:]) * 1e3, color=col, lw=1.3,
                  label=f"{tag} GUI")
        ax.loglog(f[1:], np.sqrt(pa_[1:]) * 1e3, color=col, lw=1.1, ls="--",
                  label=f"{tag} analog")
        ax.loglog(f[1:], np.sqrt(pr_[1:]) * 1e3, color=col, lw=1.0, ls=":",
                  alpha=0.75, label=f"{tag} diff")
    ax.set_xlim(0.05, 100)
    ax.set_xlabel("frequency (Hz)")
    ax.set_ylabel(r"ASD (fT/$\sqrt{\mathrm{Hz}}$)")
    ax.set_title("GUI vs analog: noise spectrum")
    ax.legend(loc="upper right", fontsize=8)
    fig.tight_layout()
    save(fig, "fig_gui_vs_analog_0727_asd.png")


def plot_ratio(specs):
    fig, ax = plt.subplots(figsize=(6.0, 4.2))
    for tag, f, pa_, pg_, pr_, col in specs:
        ax.semilogx(f[1:], np.sqrt(pa_[1:] / pg_[1:]), color=col, lw=1.2,
                    label=tag)
    ax.axhline(1.0, color="#33373B", lw=0.8)
    ax.axhspan(0.95, 1.05, color="#9AA0A6", alpha=0.15)
    ax.set_xlim(0.05, 100)
    ax.set_ylim(0.5, 2.0)
    ax.set_xlabel("frequency (Hz)")
    ax.set_ylabel("analog / GUI")
    ax.set_title("GUI vs analog: amplitude ratio (±5 % band)")
    ax.legend(loc="upper right", fontsize=9)
    fig.tight_layout()
    save(fig, "fig_gui_vs_analog_0727_ratio.png")


def main():
    style()
    runs = []
    specs = []
    for tag, pa, pg, fs_g, lag, tau, col in RUNS:
        t, gk, ak, fs = load_matched(pa, pg, fs_g, lag, tau)
        slope, off, resid, f, pa_, pg_, pr_, cxy = spectra(gk, ak, fs)
        print(f"\n=== {tag} ===  {len(gk)} matched samples ({len(gk)/fs:.1f} s)")
        print(f"   scale  analog/GUI = {slope:.4f}   offset {off:+.2f} pT")
        print(f"   residual: {resid.std():.2f} pT rms "
              f"= {resid.std()/gk.std()*100:.1f} % of signal, "
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
        runs.append((tag, t, gk, ak, col))
        specs.append((tag, f, pa_, pg_, pr_, col))

    plot_timeseries(runs)
    plot_zoom(runs)
    plot_asd(specs)
    plot_ratio(specs)


if __name__ == "__main__":
    main()
