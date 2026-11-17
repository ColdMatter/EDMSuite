#!/usr/bin/env python3
"""Meeting figure for the first 3-axis analog DAQ run (2026-07-01, pre-degauss).

Three separate figures (decorations stripped):
  fig_3axis_daq_dc.png         -- residual DC per axis
  fig_3axis_daq_staircase.png  -- mean-removed ADC staircase
  fig_3axis_daq_asd.png        -- ASD vs DAQ / quantization floors

Data: result/failed_analog/2026-07-01_closed-shield-3axis/ (+-10 V diagnostic,
5 s, and the QZFM-off +-1 V chain-floor run).

Stops with [error] instead of plotting if the input data are missing,
non-finite, mismatched, or land outside the physically plausible range.

Run:  .venv/bin/python analysis/failed_analog/plot_3axis_diagnostic.py
"""
from __future__ import annotations
import os

import numpy as np
import matplotlib
matplotlib.use("Agg")
import matplotlib.pyplot as plt
from scipy.signal import welch

FS = 1000.0
HERE = os.path.dirname(os.path.abspath(__file__))
import sys as _sys  # analysis_scripts/ on sys.path for _paths
_sys.path.insert(0, os.path.dirname(os.path.dirname(os.path.abspath(__file__))))
from _paths import DATA_ROOT, PROJECT_DIR, ANALYSIS_DIR, data, data_join, rel_data  # noqa: E402,F401
DIR = data("failed_analog", "2026-07-01_closed-shield-3axis")
ON = "closed_qzfmON-10V-residualDC"
OFF = "closed_qzfmOFF-1V-noisefloor"
C_Z, C_Y, C_X = "#2E6E8E", "#C0504D", "#5A7D3A"
C_INK, C_MUTE = "#33373B", "#7A7F85"


def style():
    plt.rcParams.update({
        "figure.dpi": 150,
        "font.size": 11,
        "axes.titleweight": "bold",
        "axes.spines.top": False, "axes.spines.right": False,
        "axes.edgecolor": C_MUTE, "text.color": C_INK,
        "legend.frameon": False,
    })


def require(ok, msg):
    if not ok:
        raise SystemExit(f"[error] {msg}")


def load_pT(path):
    require(os.path.exists(path),
            f"missing data file: {path} (copy the measurement into this folder first)")
    rows, bad = [], 0
    with open(path, "r") as fh:
        for line in fh:
            line = line.strip()
            if not line or line.startswith(("#", "%", "//")):
                continue
            try:
                rows.append(float(line.replace(",", " ").split()[0]))
            except ValueError:
                bad += 1
    require(rows, f"{path}: no numeric samples")
    require(bad <= 16, f"{path}: {bad} unparsable lines -- corrupt file?")
    x = np.asarray(rows, dtype=float)
    require(np.isfinite(x).all(), f"{path}: contains NaN/inf")
    return x


def load(prefix, axis):
    return load_pT(os.path.join(DIR, f"{prefix}_{axis}_pT.txt"))


def welch_asd(x, fs, nperseg):
    f, pxx = welch(x, fs=fs, nperseg=min(len(x), nperseg), detrend="linear")
    a = np.sqrt(pxx)
    require(np.isfinite(a).all(), "ASD came out non-finite -- corrupt input?")
    return f, a


def empirical_lsb(x):
    gaps = np.diff(np.unique(x))
    gaps = gaps[gaps > 0.05]
    require(gaps.size > 0,
            "cannot infer the ADC code width: the data never changes code")
    return float(gaps.min())


def save(fig, name):
    out = os.path.join(HERE, name)
    fig.savefig(out, bbox_inches="tight")
    plt.close(fig)
    print(f"[saved] {out}")


def plot_dc(t, z, y, x):
    fig, ax = plt.subplots(figsize=(6.0, 4.2))
    for v, c, name in ((z, C_Z, "Z"), (y, C_Y, "Y"), (x, C_X, "X")):
        ax.plot(t, v / 1e3, color=c, lw=1.4, label=f"{name}  {v.mean()/1e3:+.2f} nT")
    ax.set_xlim(0, t[-1])
    ax.set_xticks([0, 1, 2, 3, 4, 5])
    ax.set_xlabel("time (s)")
    ax.set_ylabel("B (nT)")
    ax.set_title("First 3-axis run: residual DC (pre-degauss)")
    ax.legend(loc="best", fontsize=10)
    fig.tight_layout()
    save(fig, "fig_3axis_daq_dc.png")


def plot_staircase(t, y, x, lsb):
    fig, ax = plt.subplots(figsize=(6.0, 4.2))
    xr = x - x[0]
    yr = y - y[0]
    y_off = (xr.max() - yr.min()) + 2.5 * lsb
    ax.plot(t, xr, color=C_X, lw=1.0, drawstyle="steps-post",
            label=f"X  ({len(np.unique(x))} codes)")
    ax.plot(t, yr + y_off, color=C_Y, lw=1.0, drawstyle="steps-post",
            label=f"Y  ({len(np.unique(y))} codes, offset)")
    ax.set_xlim(0, t[-1])
    ax.set_xlabel("time (s)")
    ax.set_ylabel(r"$\Delta B$ (pT)")
    ax.set_title(f"First 3-axis run: ADC staircase (1 code = {lsb:.1f} pT)")
    ax.legend(loc="best", fontsize=10)
    fig.tight_layout()
    save(fig, "fig_3axis_daq_staircase.png")


def plot_asd(on_axes, off_z, lsb):
    fig, ax = plt.subplots(figsize=(6.0, 4.2))
    quant = lsb / np.sqrt(12.0) * np.sqrt(2.0 / FS)
    for v, c, name in ((on_axes[0], C_Z, "Z"), (on_axes[1], C_Y, "Y"),
                       (on_axes[2], C_X, "X")):
        f, a = welch_asd(v, FS, nperseg=min(len(v), 4096))
        ax.loglog(f[1:], a[1:], color=c, lw=1.2, label=name)
    fo, ao = welch_asd(off_z, FS, nperseg=min(len(off_z), 4096))
    ax.loglog(fo[1:], ao[1:], color=C_INK, lw=1.5, label="DAQ floor")
    ax.axhline(quant, color="#8B5E3C", ls="--", lw=1.2,
               label=f"quant. floor ({quant*1e3:.0f} fT/√Hz)")
    ax.set_xlim(0.2, 500)
    ax.set_ylim(2e-4, 40)
    ax.set_xlabel("frequency (Hz)")
    ax.set_ylabel(r"ASD (pT/$\sqrt{\mathrm{Hz}}$)")
    ax.set_title("First 3-axis run: noise spectrum (±10 V)")
    ax.legend(loc="upper left", fontsize=9)
    fig.tight_layout()
    save(fig, "fig_3axis_daq_asd.png")


def main():
    style()
    z, y, x = (load(ON, a) for a in "ZYX")
    off_z = load(OFF, "Z")
    require(len(z) == len(y) == len(x),
            f"axis records differ in length: Z={len(z)} Y={len(y)} X={len(x)}")
    require(len(off_z) >= 4096,
            f"chain-floor record has {len(off_z)} samples; need >=4096 (~4 s)")
    require(all(abs(v.mean()) < 5e3 for v in (z, y, x)),
            "DC outside the QZFM +/-5 nT dynamic range -- wrong file or calibration?")
    t = np.arange(len(z)) / FS
    lsb = empirical_lsb(x)

    plot_dc(t, z, y, x)
    plot_staircase(t, y, x, lsb)
    plot_asd((z, y, x), off_z, lsb)

    quant = lsb / np.sqrt(12.0) * np.sqrt(2.0 / FS)
    print(f"DC means [nT]: Z={z.mean()/1e3:+.3f}  Y={y.mean()/1e3:+.3f}  X={x.mean()/1e3:+.3f}")
    print(f"empirical LSB: {lsb:.2f} pT  (= {lsb*2.7/1e3*1e3:.2f} mV at 2.7 V/nT)")
    print(f"codes used in 5 s: Z={len(np.unique(z))}  Y={len(np.unique(y))}  X={len(np.unique(x))}")
    print(f"quantization ASD (theory, white): {quant*1e3:.1f} fT/rtHz")
    fo, ao = welch_asd(off_z, FS, nperseg=4096)
    m = (fo >= 3) & (fo <= 100)
    floor = float(np.median(ao[m]))
    require(1e-3 <= floor <= 1.0,
            f"chain floor {floor * 1e3:.1f} fT/rtHz is outside [1, 1000]")
    print(f"chain floor (QZFM off, 3-100 Hz median): {floor*1e3:.1f} fT/rtHz")


if __name__ == "__main__":
    main()
