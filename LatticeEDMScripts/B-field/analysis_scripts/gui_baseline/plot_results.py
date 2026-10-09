#!/usr/bin/env python3
"""Plot the headline magnetic-shield result: closed vs open.

Data are from the lab bench (provisional) shield, not the production
lattice-generation shield.

Lives in gui_baseline/ alongside the data. Produces two figures:
  - gui_baseline/fig_asd_closed_vs_open.png    (amplitude spectral density)
  - gui_baseline/fig_allan_closed_vs_open.png  (overlapping Allan deviation)

The short open/closed takes are subsumed by these two records and omitted.
QZFM digital output is ~203 Hz (nominal 200), values in pT.

Stops with [error] instead of plotting if the input data are missing,
non-finite, too short, or land outside the physically plausible range.

Run:  python analysis/gui_baseline/plot_results.py
"""
from __future__ import annotations
import os

import numpy as np
import matplotlib
matplotlib.use("Agg")
import matplotlib.pyplot as plt
from scipy.signal import welch

FS = 203.0
HERE = os.path.dirname(os.path.abspath(__file__))            # analysis/gui_baseline
import sys as _sys  # analysis_scripts/ on sys.path for _paths
_sys.path.insert(0, os.path.dirname(os.path.dirname(os.path.abspath(__file__))))
from _paths import DATA_ROOT, PROJECT_DIR, ANALYSIS_DIR, data, data_join, rel_data  # noqa: E402,F401
DATA = data("gui_baseline")
OPEN = os.path.join(DATA, "2026-06-29_open-shield", "open_run1_Z_pT.txt")
CLOSED = os.path.join(DATA, "2026-06-30_closed-shield", "closed_long_Z_pT.txt")
LATTICE_BUDGET = 0.05                    # 50 fT/rtHz, Lim LOI / Collings lattice budget
C_OPEN, C_CLOSED = "#C0504D", "#2E6E8E"  # warm = noisy, cool = quiet
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
    """Physics/data sanity gate: stop rather than plot a wrong number."""
    if not ok:
        raise SystemExit(f"[error] {msg}")


def load_pT(path):
    """One column of numbers from a QZFM text export, validated."""
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
                bad += 1  # e.g. the GUI "Mag#1: ..." header line
    require(rows, f"{path}: no numeric samples")
    require(bad <= 16, f"{path}: {bad} unparsable lines -- corrupt file?")
    x = np.asarray(rows, dtype=float)
    require(np.isfinite(x).all(), f"{path}: contains NaN/inf")
    return x


def welch_asd(x, fs, nperseg):
    """Amplitude spectral density by Welch, segments linearly detrended."""
    f, pxx = welch(x, fs=fs, nperseg=min(len(x), nperseg), detrend="linear")
    a = np.sqrt(pxx)
    require(np.isfinite(a).all(), "ASD came out non-finite -- corrupt input?")
    return f, a


def adev(y):
    y = np.asarray(y, float)
    require(y.size >= 8, "record too short for an Allan deviation")
    tau0 = 1.0 / FS
    theta = np.concatenate([[0.0], np.cumsum(y)]) * tau0
    taus, adevs = [], []
    m = 1
    while m <= (y.size - 1) // 2:
        d = theta[2 * m:] - 2 * theta[m:-m] + theta[:-2 * m]
        avar = np.sum(d ** 2) / (2.0 * (m * tau0) ** 2 * d.size)
        taus.append(m * tau0)
        adevs.append(np.sqrt(avar))
        m *= 2
    return np.asarray(taus), np.asarray(adevs)


def fig_asd(xo, xc):
    fo, ao = welch_asd(xo, FS, 2048)
    fc, ac = welch_asd(xc, FS, 16384)
    fig, ax = plt.subplots(figsize=(6.0, 4.2))
    ax.loglog(fo[1:], ao[1:], color=C_OPEN, lw=1.4, label="open")
    ax.loglog(fc[1:], ac[1:], color=C_CLOSED, lw=1.6, label="closed")
    ax.axhline(LATTICE_BUDGET, color="#5A7D3A", ls="--", lw=1.2,
               label="50 fT target")
    ax.set_xlim(0.01, 100)
    ax.set_ylim(0.005, 60)
    ax.set_xlabel("frequency (Hz)")
    ax.set_ylabel(r"ASD (pT/$\sqrt{\mathrm{Hz}}$)")
    ax.set_title("Bench shield: open vs closed")
    ax.legend(loc="upper left", fontsize=10)
    fig.tight_layout()
    out = os.path.join(HERE, "fig_asd_closed_vs_open.png")
    fig.savefig(out, bbox_inches="tight")
    plt.close(fig)
    print(f"[saved] {out}")


def fig_allan(xo, xc):
    to, ado = adev(xo)
    tc, adc = adev(xc)
    fig, ax = plt.subplots(figsize=(6.0, 4.2))
    ax.loglog(to, ado, color=C_OPEN, lw=1.4, marker="o", ms=3.5,
              label="open")
    ax.loglog(tc, adc, color=C_CLOSED, lw=1.6, marker="o", ms=3.5,
              label="closed")
    ax.set_xlabel(r"averaging time $\tau$ (s)")
    ax.set_ylabel(r"Allan deviation $\sigma_B(\tau)$ (pT)")
    ax.set_title("Bench shield: field stability vs averaging time")
    ax.legend(loc="upper right", fontsize=10)
    fig.tight_layout()
    out = os.path.join(HERE, "fig_allan_closed_vs_open.png")
    fig.savefig(out, bbox_inches="tight")
    plt.close(fig)
    print(f"[saved] {out}")


def main():
    style()
    xo, xc = load_pT(OPEN), load_pT(CLOSED)
    require(xo.size >= 2048,
            f"open record has {xo.size} samples; need >=2048 (~10 s at {FS:g} Hz)")
    require(xc.size >= 16384,
            f"closed record has {xc.size} samples; need >=16384 (~81 s at {FS:g} Hz)")
    # closed shield is sensor-limited around 12 fT/rtHz; far outside [1, 1000]
    # means the wrong file, the wrong FS, or a broken calibration
    f, a = welch_asd(xc, FS, 16384)
    med = float(np.median(a[(f >= 3) & (f <= 100)]))
    require(1e-3 <= med <= 1.0,
            f"closed-shield white floor {med * 1e3:.1f} fT/rtHz is outside "
            f"[1, 1000] -- wrong file or wrong FS?")
    fig_asd(xo, xc)
    fig_allan(xo, xc)


if __name__ == "__main__":
    main()
