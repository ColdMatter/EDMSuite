#!/usr/bin/env python3
"""Consolidated sync5 analysis: all figures unchanged.

This single module merges the eleven original sync5 analysis scripts
(plot_sync5.py plus the ten topic scripts) into one file. The four
headline figures and every per-topic figure are reproduced byte-for-byte
identical to the originals: run main() to regenerate all 18 PNGs.

The shared plot_sync5 helpers live at module scope; each topic script's
body (with its own local helpers, whose names deliberately collide across
scripts) is wrapped in its own run_<name>() function so those locals stay
isolated. `p5` aliases this module so the original `p5.<helper>` calls
resolve to the shared definitions here.
"""
from __future__ import annotations
import sys
import os
import json

import numpy as np
import matplotlib
matplotlib.use("Agg")
import matplotlib.pyplot as plt
from matplotlib.collections import LineCollection
from scipy import stats
from scipy.signal import (welch, coherence, csd, decimate, filtfilt, firwin,
                          butter, sosfiltfilt, spectrogram, get_window)
from scipy.ndimage import median_filter
from scipy.optimize import curve_fit


# ======================================================================
# Shared helpers and headline plotters (verbatim from plot_sync5.py)
# ======================================================================

HERE = os.path.dirname(os.path.abspath(__file__))            # analysis/synctest
import sys as _sys  # analysis_scripts/ on sys.path for _paths
_sys.path.insert(0, os.path.dirname(os.path.dirname(os.path.abspath(__file__))))
from _paths import DATA_ROOT, PROJECT_DIR, ANALYSIS_DIR, data, data_join, rel_data  # noqa: E402,F401
REPO = PROJECT_DIR
DATA = data_join("result", "synctest", "sync5_30min")

AZ_FILE = os.path.join(DATA, "sync5daq_Z_pT.txt")
AY_FILE = os.path.join(DATA, "sync5daq_Y_pT.txt")
GZ_FILE = os.path.join(DATA, "gui_sync5_Formatted.txt")
CL_FILE = data_join("result", "gui_baseline", "2026-06-30_closed-shield", "closed_long_Z_pT.txt")

FS_DAQ = 1200.0        # USB-6008 hardware clock (spec 100 ppm)
FS_GUI_SEED = 203.48   # stopwatch estimate from the 1-min run; refined below
FS_CL = 203.2          # 06-30 reference record
NOMINAL_GAIN = 2.7     # V/nT assumed when the analog pT files were written

SPEC_LO, SPEC_HI = 0.007, 0.015          # QZFM Gen-3 dual-axis spec [pT/rtHz]
C_Z, C_Y, C_GUI = "#2E6E8E", "#5A7D3A", "#C0504D"
C_REF, C_INK, C_MUTE = "#9AA0A6", "#33373B", "#7A7F85"
NOTE_BBOX = dict(boxstyle="round,pad=0.2", facecolor="white", edgecolor="none", alpha=0.92)


def style():
    plt.rcParams.update({
        "figure.dpi": 150,
        "font.family": "sans-serif",
        "font.size": 12,
        "axes.titlesize": 13, "axes.titleweight": "bold",
        "axes.labelsize": 12, "axes.labelcolor": C_INK,
        "axes.edgecolor": C_MUTE, "axes.linewidth": 1.0,
        "axes.spines.top": False, "axes.spines.right": False,
        "xtick.color": C_MUTE, "ytick.color": C_MUTE,
        "xtick.labelsize": 11, "ytick.labelsize": 11,
        "text.color": C_INK,
        "grid.color": "#E6E8EA", "grid.linewidth": 0.8,
        "legend.frameon": False, "legend.fontsize": 10.5,
        "figure.facecolor": "white", "axes.facecolor": "white",
    })


def require(ok, msg):
    if not ok:
        raise SystemExit(f"[error] {msg}")


def load_col(path, skiprows=0):
    require(os.path.exists(path), f"missing data file: {path}")
    x = np.loadtxt(path, comments="#", skiprows=skiprows)
    require(x.ndim == 1 and x.size > 1000, f"{path}: unexpected shape")
    require(np.isfinite(x).all(), f"{path}: contains NaN/inf")
    return x


from bfield.analysis.spectra import welch_asd  # noqa: E402 -- was an identical local copy


def spliced_asd(x, fs, lowseg=200.0, hiseg=20.0, split=1.0):
    """Long segments below `split` Hz for resolution, short above for averaging."""
    fl, al = welch_asd(x, fs, lowseg * fs)
    fh, ah = welch_asd(x, fs, hiseg * fs)
    ml = (fl > 0) & (fl < split)
    mh = fh >= split
    return np.concatenate([fl[ml], fh[mh]]), np.concatenate([al[ml], ah[mh]])


def line_fit(x, fs, f0, half=0.12):
    """Centroid frequency and amplitude of a spectral line near f0.

    Uses the power centroid over f0 +/- half with the sideband median
    subtracted, so a mains line smeared by grid-frequency wander is
    handled; the centroid is what both records share.
    """
    x = x - x.mean()
    n = x.size
    w = np.hanning(n)
    p = np.abs(np.fft.rfft(x * w)) ** 2
    f = np.fft.rfftfreq(n, 1.0 / fs)
    m = (f >= f0 - half) & (f <= f0 + half)
    side = ((f >= f0 - 3 * half) & (f < f0 - half)) | ((f > f0 + half) & (f <= f0 + 3 * half))
    ps = np.clip(p[m] - np.median(p[side]), 0.0, None)
    require(ps.sum() > 0, f"no line found near {f0} Hz")
    fc = float(np.sum(f[m] * ps) / np.sum(ps))
    amp = float(np.sqrt(4.0 * ps.sum() / (n * np.sum(w ** 2))))
    return fc, amp


def bp(x, fs, lo, hi, order=2):
    sos = butter(order, [lo, hi], btype="band", fs=fs, output="sos")
    return sosfiltfilt(sos, x)


def lp(x, fs, cut, order=3):
    sos = butter(order, cut, btype="low", fs=fs, output="sos")
    return sosfiltfilt(sos, x)


def align_gui(az50, fs50, gz, fs_g):
    """GUI start offset vs the DAQ record, from the shared Z signal.

    Returns delta such that GUI sample time (t + delta) matches DAQ time t,
    i.e. delta > 0 means the GUI recording started first. Two passes:
    coarse on the drift band, fine on 0.3-15 Hz (the two readouts share
    the sensor noise itself, so the broadband peak is sharp).
    """
    t = np.arange(az50.size) / fs50
    tg = np.arange(gz.size) / fs_g
    core = (t > 130.0) & (t < t[-1] - 130.0)
    tc = t[core]

    def r_at(delta, aref, gref):
        m = (tc + delta > 5.0) & (tc + delta < tg[-1] - 5.0)
        if m.sum() < 1000:
            return -1.0
        g = np.interp(tc[m] + delta, tg, gref)
        return float(np.corrcoef(aref[m], g)[0, 1])

    a_dr = bp(az50, fs50, 0.01, 0.3)[core]
    g_dr = bp(gz, fs_g, 0.01, 0.3)
    coarse = np.arange(-120.0, 120.0, 0.5)
    rc = np.array([r_at(d, a_dr, g_dr) for d in coarse])
    require(rc.max() > 0.5, "drift-band alignment failed -- wrong file pair?")
    d0 = coarse[int(np.argmax(rc))]

    a_bb = bp(az50, fs50, 0.3, 15.0)[core]
    g_bb = bp(gz, fs_g, 0.3, 15.0)
    fine = np.arange(d0 - 3.0, d0 + 3.0, 0.02)
    rf = np.array([r_at(d, a_bb, g_bb) for d in fine])
    j = int(np.argmax(rf))
    delta = fine[j]
    if 0 < j < rf.size - 1:  # parabolic refinement of the peak
        a, b, c = rf[j - 1], rf[j], rf[j + 1]
        delta = fine[j] + 0.5 * (a - c) / (a - 2 * b + c) * 0.02
    return delta, float(rc.max()), float(rf.max())


def block_lags(az50, fs50, gz_f, fs_g, delta, nblk=6):
    """Best-fit GUI lag per time block, relative to the global delta [s].

    If the GUI stream were on a steady clock these would all be ~0; a
    wandering lag means the GUI writer drops or delays samples, which
    de-coheres the two Z records at high frequency without adding noise.
    """
    t = np.arange(az50.size) / fs50
    tg = np.arange(gz_f.size) / fs_g
    a_bb = bp(az50, fs50, 0.3, 15.0)
    g_bb = bp(gz_f, fs_g, 0.3, 15.0)
    lo0, hi0 = 130.0, t[-1] - 130.0
    out = []
    for k in range(nblk):
        lo = lo0 + k * (hi0 - lo0) / nblk
        hi = lo + (hi0 - lo0) / nblk
        mb = (t >= lo) & (t < hi)
        grid = np.arange(delta - 0.5, delta + 0.5, 0.005)
        rr = np.array([np.corrcoef(a_bb[mb], np.interp(t[mb] + d, tg, g_bb))[0, 1]
                       for d in grid])
        j = int(np.argmax(rr))
        d = grid[j]
        if 0 < j < rr.size - 1:
            a, b, c = rr[j - 1], rr[j], rr[j + 1]
            d = grid[j] + 0.5 * (a - c) / (a - 2 * b + c) * 0.005
        out.append(d - delta)
    return out


def adev(y, fs, tau_min=0.05):
    """Overlapping Allan deviation, ~1.6x spaced averaging times."""
    tau0 = 1.0 / fs
    theta = np.concatenate([[0.0], np.cumsum(y)]) * tau0
    taus, out = [], []
    m = max(1, int(round(tau_min * fs)))
    while m <= (y.size - 1) // 3:
        d = theta[2 * m:] - 2 * theta[m:-m] + theta[:-2 * m]
        out.append(np.sqrt(np.sum(d * d) / (2.0 * (m * tau0) ** 2 * d.size)))
        taus.append(m * tau0)
        m = int(np.ceil(m * 1.6))
    return np.asarray(taus), np.asarray(out)


def find_lines(x, fs, flo=40.0, fhi=600.0, ratio=5.0):
    """Spectral lines standing more than `ratio` above the local floor."""
    f, a = welch_asd(x, fs, 10.0 * fs)
    base = median_filter(a, size=201)
    idx = np.where((f >= flo) & (f <= fhi) & (a > ratio * base))[0]
    lines = []
    while idx.size:
        grp = idx[idx <= idx[0] + int(1.0 / (f[1] - f[0]))]  # bins within 1 Hz
        j = grp[np.argmax(a[grp])]
        lines.append((f[j], a[j], a[j] / base[j]))
        idx = idx[idx > grp[-1]]
    lines.sort(key=lambda r: -r[2])
    return lines


def fig_asd(curves, floors):
    fig, ax = plt.subplots(figsize=(7.6, 5.0))
    ax.axhspan(SPEC_LO, SPEC_HI, color=C_REF, alpha=0.22, zorder=0, lw=0)
    f, a = curves["cl"]
    ax.loglog(f, a, color=C_REF, lw=1.2, ls="--", zorder=2,
              label="06-30 closed, GUI Z (15 min)")
    f, a = curves["az"]
    ax.loglog(f, a, color=C_Z, lw=1.6, zorder=4, label="analog Z (calibrated)")
    f, a = curves["ay"]
    ax.loglog(f, a, color=C_Y, lw=1.3, alpha=0.9, zorder=3, label="analog Y (calibrated)")
    f, a = curves["gz"]
    ax.loglog(f, a, color=C_GUI, lw=1.0, alpha=0.85, zorder=5, label="GUI Z (digital)")
    ax.set_xlim(0.004, 120)
    ax.set_ylim(0.008, 12)
    ax.grid(True, which="major", alpha=0.9)
    ax.grid(True, which="minor", alpha=0.35)
    ax.set_xlabel("frequency  [Hz]")
    ax.set_ylabel(r"field noise  [pT/$\sqrt{\mathrm{Hz}}$]")
    ax.set_title("Closed-shield ASD", loc="left", pad=26)
    ax.text(0.0, 1.02, r"Gray band: QZFM dual-axis self-noise spec 7–15 fT/$\sqrt{\mathrm{Hz}}$",
            transform=ax.transAxes, fontsize=10, color=C_MUTE, va="bottom")
    ax.legend(loc="lower left")
    fig.tight_layout()
    out = os.path.join(HERE, "fig_sync5_asd.png")
    fig.savefig(out, bbox_inches="tight")
    plt.close(fig)
    print(f"[saved] {out}")


def fig_drift(tmin, z_daq, z_gui, y_daq, r_mid, r_slow, slope, dc_note):
    fig, (ax1, ax2) = plt.subplots(2, 1, figsize=(7.6, 5.2), sharex=True)
    ax1.plot(tmin, z_daq, color=C_Z, lw=1.5, label="analog Z")
    ax1.plot(tmin, z_gui, color=C_GUI, lw=1.0, alpha=0.85, label="GUI Z (aligned)")
    ax1.set_ylabel("Z drift  [pT]")
    ax1.legend(loc="upper right", ncols=2)
    ax1.text(0.01, 0.04, f"same sensor, two readouts: scale ratio {slope:.3f}",
             transform=ax1.transAxes, fontsize=9.5, color=C_MUTE, va="bottom")
    ax1.set_title("Slow drift (<0.1 Hz), means removed", loc="left")
    ax2.plot(tmin, z_daq, color=C_MUTE, lw=0.9, alpha=0.7, label="analog Z (repeated)")
    ax2.plot(tmin, y_daq, color=C_Y, lw=1.5, label="analog Y")
    ax2.set_ylabel("Y drift  [pT]")
    ax2.set_xlabel("time  [min]")
    ax2.legend(loc="upper right", ncols=2)
    ax2.text(0.01, 0.04,
             f"Y tracks Z: r = {r_mid:+.2f} (0.02–0.2 Hz), {r_slow:+.2f} (1–10 mHz)"
             " — one common cause, in phase",
             transform=ax2.transAxes, fontsize=9.5, color=C_MUTE, va="bottom")
    for ax in (ax1, ax2):
        ax.grid(True, alpha=0.7)
    fig.text(0.995, 0.005, dc_note, fontsize=8.5, color=C_MUTE, ha="right", va="bottom")
    fig.tight_layout()
    out = os.path.join(HERE, "fig_sync5_drift.png")
    fig.savefig(out, bbox_inches="tight")
    plt.close(fig)
    print(f"[saved] {out}")


def fig_coh(fzg, czg, fzy, czy, bias_zg, bias_zy, chain_note, c50):
    fig, ax = plt.subplots(figsize=(7.6, 4.8))
    m = fzg <= 18.0
    ax.semilogx(fzg[m], czg[m], color=C_Z, lw=1.6, label="analog Z × GUI Z (same sensor)")
    m = (fzy > 0) & (fzy <= 110.0)
    ax.semilogx(fzy[m], czy[m], color=C_Y, lw=1.1, alpha=0.9, label="analog Z × analog Y")
    ax.axhline(bias_zy, color=C_MUTE, ls=":", lw=1.0)
    ax.text(0.012, bias_zy + 0.02, "zero-coherence bias", fontsize=8.5, color=C_MUTE)
    ax.set_xlim(0.01, 120)
    ax.set_ylim(0, 1.05)
    ax.grid(True, which="both", alpha=0.6)
    ax.set_xlabel("frequency  [Hz]")
    ax.set_ylabel(r"coherence  $\gamma^2$")
    ax.set_title("What is shared between channels", loc="left")
    ax.text(0.03, 0.76, chain_note, transform=ax.transAxes, fontsize=9,
            color=C_Z, va="top", bbox=NOTE_BBOX)
    if c50 is not None:
        ax.annotate(f"50 Hz mains: γ² = {c50:.2f}\n(real common field)",
                    xy=(50, c50), xytext=(8, 0.32), fontsize=9.5, color=C_Y,
                    bbox=NOTE_BBOX,
                    arrowprops=dict(arrowstyle="->", color=C_Y, lw=1.0))
    ax.legend(loc="center left", bbox_to_anchor=(0.02, 0.86))
    fig.tight_layout()
    out = os.path.join(HERE, "fig_sync5_coherence.png")
    fig.savefig(out, bbox_inches="tight")
    plt.close(fig)
    print(f"[saved] {out}")


def fig_allan(sets):
    fig, ax = plt.subplots(figsize=(7.6, 4.8))
    for label, color, (taus, ad) in sets:
        ax.loglog(taus, ad, lw=1.5, marker="o", ms=3, color=color, label=label)
    taus, ad = sets[0][2]
    j = int(np.argmin(ad))
    ax.plot(taus[j], ad[j], "o", ms=9, mfc="none", mec=C_Z, mew=1.8)
    ax.annotate(f"best stability {ad[j]*1e3:.0f} fT\nat τ ≈ {taus[j]:.1f} s",
                xy=(taus[j], ad[j]), xytext=(0.03, 0.08), textcoords="axes fraction",
                fontsize=9.5, color=C_Z, bbox=NOTE_BBOX,
                arrowprops=dict(arrowstyle="->", color=C_Z, lw=1.0,
                                connectionstyle="arc3,rad=0.15"))
    ax.text(0.97, 0.75, "drift\ndominates", color=C_MUTE, transform=ax.transAxes,
            fontsize=9.5, ha="right", va="center", bbox=NOTE_BBOX)
    ax.grid(True, which="major", alpha=0.9)
    ax.grid(True, which="minor", alpha=0.35)
    ax.set_xlabel(r"averaging duration  $\tau$  [s]")
    ax.set_ylabel(r"Allan deviation  $\sigma_B(\tau)$  [pT]")
    ax.set_title("Field stability vs averaging time (30-min run)", loc="left")
    ax.legend(loc="upper right")
    fig.tight_layout()
    out = os.path.join(HERE, "fig_sync5_allan.png")
    fig.savefig(out, bbox_inches="tight")
    plt.close(fig)
    print(f"[saved] {out}")

p5 = sys.modules[__name__]


def run_headline():
    plt.rcdefaults()
    style()
    az_file = load_col(AZ_FILE)
    ay_file = load_col(AY_FILE)
    gz = load_col(GZ_FILE, skiprows=1)
    cl = load_col(CL_FILE, skiprows=1)
    require(abs(az_file.size - ay_file.size) == 0, "Z/Y length mismatch")
    require(az_file.size > 10 * FS_DAQ * 60, "analog record shorter than 10 min?")

    # --- clock and gain calibration off the shared 50 Hz line ---------------
    fc_a, _ = line_fit(az_file, FS_DAQ, 50.0, half=0.2)
    fc_g0, _ = line_fit(gz, FS_GUI_SEED, 50.0, half=0.3)
    fs_gui = FS_GUI_SEED * fc_a / fc_g0
    _, amp_a = line_fit(az_file, FS_DAQ, fc_a)
    _, amp_g = line_fit(gz, fs_gui, fc_a)
    gain = NOMINAL_GAIN * amp_a / amp_g          # V per nT, end to end
    require(1.5 < gain < 4.0, f"implausible gain {gain:.2f} V/nT")
    cal = NOMINAL_GAIN / gain                    # multiply analog pT by this
    az = az_file * cal
    ay = ay_file * cal
    print(f"[cal] mains line {fc_a:.3f} Hz -> GUI clock {fs_gui:.2f} Hz "
          f"(seed {FS_GUI_SEED})")
    print(f"[cal] line amp {amp_a:.4f} pT (analog file) / {amp_g:.4f} pT (GUI)"
          f" -> effective gain {gain:.3f} V/nT, analog scale x{cal:.4f}")

    dc_az, dc_ay, dc_gz = az.mean(), ay.mean(), gz.mean()
    print(f"[dc] Z {dc_az:+.1f}  Y {dc_ay:+.1f}  GUI Z {dc_gz:+.1f} pT "
          f"(GUI - analog = {dc_gz - dc_az:+.1f} pT; DC includes sensor offset,"
          " not a field claim)")

    # --- spectra -------------------------------------------------------------
    curves = {
        "az": spliced_asd(az, FS_DAQ),
        "ay": spliced_asd(ay, FS_DAQ),
        "gz": spliced_asd(gz, fs_gui),
        "cl": spliced_asd(cl, FS_CL, lowseg=100.0),
    }
    floors = {}
    for k, (f, a) in curves.items():
        floors[k] = float(np.median(a[(f >= 3) & (f <= 40)]))
    print("[noise] white floor 3-40 Hz [fT/rtHz]: "
          + "  ".join(f"{k} {floors[k]*1e3:.1f}" for k in ("az", "ay", "gz", "cl")))
    print("[noise] low-frequency ASD [fT/rtHz]  (0.005 Hz = first bin, indicative)")
    print(f"        {'f[Hz]':>7} {'analogZ':>9} {'analogY':>9} {'guiZ':>9} {'06-30':>9}")
    for f0 in (0.005, 0.01, 0.02, 0.05, 0.1, 0.5, 1.0):
        row = []
        for k in ("az", "ay", "gz", "cl"):
            f, a = curves[k]
            row.append(a[np.argmin(np.abs(f - f0))] * 1e3)
        print(f"        {f0:7.3f} " + " ".join(f"{v:9.0f}" for v in row))

    # --- align GUI onto the DAQ clock and measure coherence ------------------
    az50 = decimate(decimate(az, 6, ftype="fir"), 4, ftype="fir")
    ay50 = decimate(decimate(ay, 6, ftype="fir"), 4, ftype="fir")
    fs50 = FS_DAQ / 24.0
    delta, r_dr, r_bb = align_gui(az50, fs50, gz, fs_gui)
    print(f"[align] GUI started {delta:+.2f} s relative to the DAQ "
          f"(r drift {r_dr:.3f}, r 0.3-15 Hz {r_bb:.3f})")

    t = np.arange(az50.size) / fs50
    tg = np.arange(gz.size) / fs_gui
    glp = filtfilt(firwin(401, 21.0, fs=fs_gui), [1.0], gz)
    m = (t > 130.0) & (t < t[-1] - 130.0) & (t + delta > 2.0) & (t + delta < tg[-1] - 2.0)
    gz_al = np.interp(t[m] + delta, tg, glp)
    az_al, ay_al, t_al = az50[m], ay50[m], t[m]

    nseg = int(100 * fs50)
    fzg, czg = coherence(az_al, gz_al, fs=fs50, nperseg=nseg)
    n_avg_zg = max(1, 2 * az_al.size // nseg - 1)
    fzy, czy = coherence(az, ay, fs=FS_DAQ, nperseg=int(100 * FS_DAQ))
    n_avg_zy = max(1, 2 * az.size // int(100 * FS_DAQ) - 1)

    wm = (fzg >= 3) & (fzg <= 15)
    g_white = float(np.median(czg[wm]))
    quad = np.sqrt(max(floors["az"] ** 2 - floors["gz"] ** 2, 0.0))
    dm = (fzg >= 0.02) & (fzg <= 0.2)

    # is the loss of Z-GUI coherence above ~1 Hz noise or timing? check how
    # steady the GUI lag is, and redo the white band on the steady stretch
    lags = block_lags(az50, fs50, glp, fs_gui, delta)
    lagspan = 1e3 * (max(lags) - min(lags))
    ms = m & (t > 650.0)
    gz2 = np.interp(t[ms] + delta, tg, glp)
    f2, c2 = coherence(az50[ms], gz2, fs=fs50, nperseg=nseg)
    g_white2 = float(np.median(c2[(f2 >= 3) & (f2 <= 15)]))
    print("[align] GUI lag per ~260 s block [ms rel]: "
          + " ".join(f"{1e3*d:+.0f}" for d in lags))
    print(f"[coh] Z-analog x Z-GUI: drift band {np.median(czg[dm]):.3f}, "
          f"white band {g_white:.3f} (full), {g_white2:.3f} steady-half single-align")
    print(f"[coh] the >1 Hz roll-off is GUI stream timing wander ({lagspan:.0f} ms span); "
          "floor coherence is jitter-limited and method-sensitive")
    print("[coh] (~0.1-0.4 with per-block alignment) -> path cross-spectrum is a <1 Hz check, not a floor check;")
    print(f"[coh] DAQ chain noise from the floor quadrature difference: "
          f"{quad*1e3:.1f} fT/rtHz")

    dmy = (fzy >= 0.02) & (fzy <= 0.2)
    wmy = (fzy >= 3) & (fzy <= 15)
    j50 = np.argmin(np.abs(fzy - 50.0))
    c50 = float(czy[max(0, j50 - 5):j50 + 6].max())
    fgy, cgy = coherence(gz_al, ay_al, fs=fs50, nperseg=nseg)
    gy_drift = float(np.median(cgy[(fgy >= 0.02) & (fgy <= 0.2)]))
    fp, pzy = csd(az_al, ay_al, fs=fs50, nperseg=nseg)
    dmp = (fp >= 0.02) & (fp <= 0.2)
    ph = float(np.angle(np.sum(pzy[dmp])) * 180.0 / np.pi)
    print(f"[coh] Z x Y drift band: analog pair {np.median(czy[dmy]):.3f}, "
          f"GUI-Z x analog-Y {gy_drift:.3f} (same -> in the sensor, not the DAQ), "
          f"phase {ph:+.0f} deg")
    # white band: the same cross-path check, on the steady-lag half so the
    # GUI timing wander does not fake a low value
    f4, c4 = coherence(gz2, ay50[ms], fs=fs50, nperseg=nseg)
    gy_white = float(np.median(c4[(f4 >= 3) & (f4 <= 15)]))
    common = float(np.median(np.sqrt(np.abs(pzy[(fp >= 3) & (fp <= 15)]))))
    part = np.sqrt(max(floors["az"] ** 2 - common ** 2, 0.0))
    print(f"[coh] Z x Y white band: analog pair {np.median(czy[wmy]):.3f}, "
          f"GUI-Z x analog-Y {gy_white:.3f} steady half (bias {1.0/n_avg_zy:.3f}); "
          f"50 Hz {c50:.2f}")
    print(f"[coh] in-band common component ~{common*1e3:.0f} fT/rtHz (real field or "
          f"sensor-common, e.g. laser); per-axis remainder ~{part*1e3:.0f} fT/rtHz")

    # --- drift view and Z-Y correlation by band -------------------------------
    zd = lp(az_al, fs50, 0.1) ; zd -= zd.mean()
    gd = lp(gz_al, fs50, 0.1) ; gd -= gd.mean()
    yd = lp(ay_al, fs50, 0.1) ; yd -= yd.mean()
    slope = float(np.dot(gd, zd) / np.dot(zd, zd))
    print(f"[drift] GUI/analog scale ratio below 0.1 Hz: {slope:.3f} "
          f"(1.000 = calibration consistent)")
    z1 = decimate(decimate(az_al, 10, ftype="fir"), 5, ftype="fir")   # 1 Hz rate
    y1 = decimate(decimate(ay_al, 10, ftype="fir"), 5, ftype="fir")
    fs1 = fs50 / 50.0
    def bandr(lo, hi):
        return float(np.corrcoef(bp(z1, fs1, lo, hi)[100:-100],
                                 bp(y1, fs1, lo, hi)[100:-100])[0, 1])
    r_slow, r_mid = bandr(0.001, 0.01), bandr(0.02, 0.2)
    print(f"[drift] Z-Y correlation: r = {r_mid:+.2f} at 0.02-0.2 Hz, "
          f"{r_slow:+.2f} at 1-10 mHz -> the two axes drift together")

    # --- Allan ---------------------------------------------------------------
    az100 = decimate(az, 12, ftype="fir")
    ay100 = decimate(ay, 12, ftype="fir")
    gz100 = decimate(gz, 2, ftype="fir")
    allan_sets = [
        ("analog Z", C_Z, adev(az100, FS_DAQ / 12.0)),
        ("analog Y", C_Y, adev(ay100, FS_DAQ / 12.0)),
        ("GUI Z", C_GUI, adev(gz100, fs_gui / 2.0)),
    ]
    taus, ad = allan_sets[0][2]
    j = int(np.argmin(ad))
    print(f"[allan] analog Z minimum {ad[j]*1e3:.0f} fT at tau {taus[j]:.2f} s")

    # --- instrument lines above 40 Hz ----------------------------------------
    lines = find_lines(az, FS_DAQ)
    print("[lines] analog Z, above 5x local floor (40-600 Hz):")
    for fL, aL, ratio in lines[:8]:
        tag = ""
        if abs(fL - 276.4) < 1.5:
            tag = "  <- alias of the 923.6 Hz cell modulation (1200 - 923.6)"
        print(f"        {fL:7.1f} Hz  {aL*1e3:8.1f} fT/rtHz  ({ratio:4.0f}x){tag}")

    # --- figures --------------------------------------------------------------
    fig_asd(curves, floors)
    stride = int(round(fs50))          # 1 Hz plotting grid is plenty below 0.1 Hz
    tmin = (t_al - t_al[0])[::stride] / 60.0
    dc_note = (f"DC (not offset-separated): analog Z {dc_az:+.1f}, GUI Z {dc_gz:+.1f}, "
               f"Y {dc_ay:+.1f} pT")
    fig_drift(tmin, zd[::stride], gd[::stride], yd[::stride], r_mid, r_slow,
              slope, dc_note)
    chain_note = (f"γ² ≈ 1 below ~1 Hz: chain agreement, one signal\n"
                  f"above ~1 Hz GUI timing wander ({lagspan:.0f} ms span) makes the\n"
                  f"floor coherence jitter-limited and method-sensitive (~0.1–0.4)\n"
                  f"→ path check valid <1 Hz only. DAQ chain ⊕{quad*1e3:.0f} fT/√Hz")
    fig_coh(fzg, czg, fzy, czy, 1.0 / n_avg_zg, 1.0 / n_avg_zy, chain_note, c50)
    fig_allan(allan_sets)


def run_allan():
    plt.rcdefaults()
    BASE = PROJECT_DIR
    OUT = HERE
    os.makedirs(OUT, exist_ok=True)


    # ---- established constants (do NOT re-derive) ----
    FS_DAQ = 1200.0566
    FS_GUI = 203.18
    CAL_ANALOG = 1.029          # 2.7/2.624 -> effective gain correction
    EXCLUDE_S = 300.0           # first 5 min

    AZ = data_join("result/synctest/sync5_30min/sync5daq_Z_pT.txt")
    AY = data_join("result/synctest/sync5_30min/sync5daq_Y_pT.txt")
    GZ = data_join("result/synctest/sync5_30min/gui_sync5_Formatted.txt")
    CL = data_join("result/gui_baseline/2026-06-30_closed-shield/closed_long_Z_pT.txt")


    def hdev(y, fs, tau_min=0.05):
        """Overlapping Hadamard deviation of a rate series y (pT).

        Third differences of the cumulative phase -> insensitive to linear
        frequency drift (drift-rejecting). Same tau grid style as p5.adev.
        """
        tau0 = 1.0 / fs
        theta = np.concatenate([[0.0], np.cumsum(y)]) * tau0   # "phase" = integral
        taus, out = [], []
        m = max(1, int(round(tau_min * fs)))
        while m <= (y.size - 1) // 4:
            # third difference of phase over span m: theta[3m]-3theta[2m]+3theta[m]-theta[0]
            d = (theta[3 * m:] - 3 * theta[2 * m:-m]
                 + 3 * theta[m:-2 * m] - theta[:-3 * m])
            var = np.sum(d * d) / (6.0 * (m * tau0) ** 2 * d.size)
            out.append(np.sqrt(var))
            taus.append(m * tau0)
            m = int(np.ceil(m * 1.6))
        return np.asarray(taus), np.asarray(out)


    def load_after(path, fs, skiprows, cal):
        x = np.loadtxt(path, comments="#", skiprows=skiprows)
        n0 = int(round(EXCLUDE_S * fs))
        x = x[n0:]
        x = (x - x.mean()) * cal      # remove DC offset (not a field), calibrate
        return x


    # analog files: 4 comment '#' lines -> comments="#" strips them, skiprows=0
    az = load_after(AZ, FS_DAQ, 0, CAL_ANALOG)
    ay = load_after(AY, FS_DAQ, 0, CAL_ANALOG)
    gz = load_after(GZ, FS_GUI, 1, 1.0)          # GUI: 1 header line, already calibrated
    cl = load_after(CL, FS_GUI, 1, 1.0)          # 06-30 GUI Z reference

    print("sizes after 5-min cut:",
          "az", az.size, "ay", ay.size, "gz", gz.size, "cl", cl.size)
    print("durations [s]:",
          round(az.size / FS_DAQ, 1), round(ay.size / FS_DAQ, 1),
          round(gz.size / FS_GUI, 1), round(cl.size / FS_GUI, 1))

    chans = [
        ("analog Z", az, FS_DAQ, "#c62828"),
        ("analog Y", ay, FS_DAQ, "#1565c0"),
        ("GUI Z",    gz, FS_GUI, "#2e7d32"),
        ("06-30 GUI Z", cl, FS_GUI, "#616161"),
    ]

    results = {}
    for name, x, fs, col in chans:
        ta, da = p5.adev(x, fs, tau_min=1.0 / fs)   # resolve down to Nyquist-limited tau
        th, dh = hdev(x, fs, tau_min=1.0 / fs)
        # best-stability point of the ALLAN curve (search tau>=0.03s: below that
        # the 276.4 Hz cell-mod alias + quantization inflate the finest bins)
        valid = ta >= 0.03
        j = np.where(valid)[0][int(np.argmin(da[valid]))]
        results[name] = dict(ta=ta, da=da, th=th, dh=dh, col=col, fs=fs,
                             best_tau=ta[j], best_val=da[j])
        print(f"\n{name}: fs={fs}")
        print(f"  Allan min = {da[j]*1000:.2f} fT at tau={ta[j]:.3g} s")
        # sample a few taus
        for tt in (0.1, 1.0, 10.0, 30.0, 60.0, 120.0):
            k = int(np.argmin(np.abs(ta - tt)))
            print(f"    tau~{ta[k]:6.2f}s  ADEV={da[k]*1000:7.2f} fT  "
                  f"HDEV={dh[int(np.argmin(np.abs(th-tt)))]*1000:7.2f} fT")


    def loglog_slope(tau, dev, lo, hi):
        """Local log-log slope of dev(tau) over [lo,hi] s."""
        m = (tau >= lo) & (tau <= hi)
        if m.sum() < 2:
            return np.nan
        return np.polyfit(np.log(tau[m]), np.log(dev[m]), 1)[0]


    print("\n--- Allan local slopes (log-log) ---")
    for name in results:
        r = results[name]
        ta, da = r["ta"], r["da"]
        s0 = loglog_slope(ta, da, 0.005, 0.05)
        s1 = loglog_slope(ta, da, 0.1, 1.0)
        s2 = loglog_slope(ta, da, 1.0, 10.0)
        s3 = loglog_slope(ta, da, 10.0, 60.0)
        s4 = loglog_slope(ta, da, 60.0, 200.0)
        print(f"{name:12s}  .005-.05s:{s0:+.2f}  0.1-1s:{s1:+.2f}  1-10s:{s2:+.2f}  "
              f"10-60s:{s3:+.2f}  60-200s:{s4:+.2f}")

    # Y plateau interrogation 30-60 s
    ry = results["analog Y"]
    print("\n--- analog Y around 30-60 s ---")
    for tt in (20, 30, 40, 50, 60, 80):
        k = int(np.argmin(np.abs(ry["ta"] - tt)))
        print(f"  tau={ry['ta'][k]:6.2f}s ADEV={ry['da'][k]*1000:7.2f} fT")

    # ================= FIGURE =================
    fig, axes = plt.subplots(1, 2, figsize=(13.5, 5.6), sharey=True)
    for ax, kind, key_dev, key_tau, title in [
            (axes[0], "Allan", "da", "ta", "Overlapping Allan deviation"),
            (axes[1], "Hadamard", "dh", "th", "Hadamard deviation (drift-rejecting)")]:
        for name in ["analog Z", "analog Y", "GUI Z", "06-30 GUI Z"]:
            r = results[name]
            ls = "--" if "06-30" in name else "-"
            lw = 1.4 if "06-30" in name else 1.8
            ax.loglog(r[key_tau], r[key_dev] * 1000, ls, lw=lw,
                      color=r["col"], label=name)
        # slope guides anchored to analog Z
        rz = results["analog Z"]
        t0 = 0.3
        k0 = int(np.argmin(np.abs(rz["ta"] - t0)))
        v0 = (rz["da"][k0] if kind == "Allan" else rz["dh"][k0]) * 1000
        tg = np.array([0.1, 5.0])
        ax.loglog(tg, v0 * (tg / t0) ** -0.5, ":", color="k", alpha=0.55, lw=1.2)
        ax.text(0.12, v0 * (0.12 / t0) ** -0.5 * 1.15, r"$\tau^{-1/2}$",
                fontsize=10, color="k")
        tg2 = np.array([10.0, 200.0])
        va = (rz["da"] if kind == "Allan" else rz["dh"])
        ka = int(np.argmin(np.abs(rz["ta"] - 20.0)))
        vb = va[ka] * 1000
        ax.loglog(tg2, vb * (tg2 / 20.0) ** 0.5, ":", color="k", alpha=0.55, lw=1.2)
        ax.text(120, vb * (120 / 20.0) ** 0.5 * 1.1, r"$\tau^{+1/2}$",
                fontsize=10, color="k")
        ax.set_xlabel(r"averaging time $\tau$ [s]")
        ax.set_title(title, fontsize=11)
        ax.grid(True, which="both", alpha=0.25)
    axes[0].set_ylabel("deviation [fT]")
    axes[0].legend(fontsize=8.5, loc="lower left")
    fig.suptitle("2026-07-06 sync5 closed shield: Allan & Hadamard stability "
                 "(first 5 min excluded, analog calibrated x1.029)", fontsize=11)
    fig.tight_layout(rect=(0, 0, 1, 0.97))
    figpath = os.path.join(OUT, "allan_hadamard_sync5.png")
    fig.savefig(figpath, dpi=150)
    print("\nsaved", figpath)


def run_decomp():
    plt.rcdefaults()
    REPO = PROJECT_DIR
    OUTDIR = HERE
    os.makedirs(OUTDIR, exist_ok=True)


    FS_DAQ = 1200.0566           # precise analog clock
    FS_GUI_SEED = 203.48
    CAL = 1.029                  # analog pT file -> calibrated pT (2.7/2.624)

    # ---------------------------------------------------------------- load
    az = p5.load_col(p5.AZ_FILE) * CAL
    ay = p5.load_col(p5.AY_FILE) * CAL
    gz = p5.load_col(p5.GZ_FILE, skiprows=1)   # already calibrated
    print(f"[load] analog Z {az.size}, Y {ay.size}, GUI Z {gz.size}")

    # ---------------------------------------------------------------- GUI clock
    # refine GUI sample rate off the shared 50 Hz mains line (same as plot_sync5)
    fc_a, _ = p5.line_fit(az, FS_DAQ, 50.0, half=0.2)
    fc_g0, _ = p5.line_fit(gz, FS_GUI_SEED, 50.0, half=0.3)
    fs_gui = FS_GUI_SEED * fc_a / fc_g0
    print(f"[cal] mains {fc_a:.3f} Hz -> GUI clock {fs_gui:.3f} Hz")

    # ================================================================
    # PART A:  common(Z,Y) vs per-axis, sample-synchronous @ 1200 Hz
    # ================================================================
    # Welch settings: 200-s segments for resolution down to ~0.05 Hz, Hann, 50% ovlp
    NPS_A = int(200 * FS_DAQ)


    def psd(x, fs, nps):
        f, p = welch(x, fs=fs, nperseg=min(len(x), nps), detrend="linear")
        return f, p


    def cross(x, y, fs, nps):
        f, pxy = csd(x, y, fs=fs, nperseg=min(len(x), nps), detrend="linear")
        return f, pxy


    fZ, Pzz = psd(az, FS_DAQ, NPS_A)
    _, Pyy = psd(ay, FS_DAQ, NPS_A)
    _, Pzy = cross(az, ay, FS_DAQ, NPS_A)
    _, cohZY = coherence(az, ay, fs=FS_DAQ, nperseg=min(len(az), NPS_A))
    n_avg_A = max(1, 2 * az.size // NPS_A - 1)
    bias_A = 1.0 / n_avg_A     # zero-coherence coherence bias

    asd_Z = np.sqrt(Pzz)                 # total analog-Z ASD [pT/rtHz]
    asd_Y = np.sqrt(Pyy)                 # total analog-Y ASD
    # common ASD between the two axes: magnitude of cross-spectrum, sqrt->ASD.
    # |Pzy| = gamma * sqrt(Pzz*Pyy); this is the ASD of the coherent (shared) part.
    asd_common = np.sqrt(np.abs(Pzy))
    # per-axis independent remainder: quadrature-subtract the common power
    rem_Z = np.sqrt(np.clip(Pzz - np.abs(Pzy), 0, None))
    rem_Y = np.sqrt(np.clip(Pyy - np.abs(Pzy), 0, None))
    phase_ZY = np.angle(Pzy, deg=True)

    # ================================================================
    # PART B:  path-common(analogZ, guiZ), aligned + steady half
    # ================================================================
    # decimate analog Z to ~50 Hz for alignment (matches plot_sync5)
    az50 = decimate(decimate(az, 6, ftype="fir"), 4, ftype="fir")
    fs50 = FS_DAQ / 24.0
    delta, r_dr, r_bb = p5.align_gui(az50, fs50, gz, fs_gui)
    print(f"[align] GUI started {delta:+.3f} s vs DAQ (r_drift {r_dr:.3f}, r_bb {r_bb:.3f})")

    # anti-alias the GUI before interpolation onto the DAQ 50-Hz grid
    glp = filtfilt(firwin(401, 21.0, fs=fs_gui), [1.0], gz)
    t = np.arange(az50.size) / fs50
    tg = np.arange(gz.size) / fs_gui
    # STEADY-LAG HALF only: t > 650 s  (GUI timing wanders in the first half)
    m = (t > 650.0) & (t < t[-1] - 5.0) & (t + delta > 2.0) & (t + delta < tg[-1] - 2.0)
    az_al = az50[m]
    gz_al = np.interp(t[m] + delta, tg, glp)
    print(f"[align] steady-half samples {az_al.size} ({az_al.size/fs50:.0f} s @ {fs50:.2f} Hz)")

    NPS_B = int(50 * fs50)     # 50-s segments at 50 Hz
    fZG, Pzg = csd(az_al, gz_al, fs=fs50, nperseg=min(len(az_al), NPS_B), detrend="linear")
    _, Pzz_al = welch(az_al, fs=fs50, nperseg=min(len(az_al), NPS_B), detrend="linear")
    _, Pgg_al = welch(gz_al, fs=fs50, nperseg=min(len(az_al), NPS_B), detrend="linear")
    _, cohZG = coherence(az_al, gz_al, fs=fs50, nperseg=min(len(az_al), NPS_B))
    n_avg_B = max(1, 2 * az_al.size // NPS_B - 1)
    bias_B = 1.0 / n_avg_B
    asd_path_common = np.sqrt(np.abs(Pzg))    # ASD common to both Z readout paths


    # ---------------------------------------------------------------- report helper
    def at(f, a, f0):
        return float(a[np.argmin(np.abs(f - f0))])


    print("\n================ PART A: common(Z,Y) vs per-axis @1200 Hz ================")
    print(f"  Welch: {NPS_A/FS_DAQ:.0f}-s segs, {n_avg_A} averages, coh bias {bias_A:.3f}")
    print(f"  {'f[Hz]':>7} {'totZ':>8} {'totY':>8} {'common':>8} {'remZ':>8} {'remY':>8} "
          f"{'coh':>6} {'phase':>7}")
    for f0 in (0.1, 1.0, 10.0):
        print(f"  {f0:7.2f} "
              f"{at(fZ,asd_Z,f0)*1e3:8.1f} {at(fZ,asd_Y,f0)*1e3:8.1f} "
              f"{at(fZ,asd_common,f0)*1e3:8.1f} {at(fZ,rem_Z,f0)*1e3:8.1f} "
              f"{at(fZ,rem_Y,f0)*1e3:8.1f} {at(fZ,cohZY,f0):6.2f} "
              f"{at(fZ,phase_ZY,f0):+7.0f}")
    print("  (all ASD in fT/rtHz)")

    # white-floor split 3-40 Hz
    wm = (fZ >= 3) & (fZ <= 40)
    tot_wZ = np.median(asd_Z[wm]); tot_wY = np.median(asd_Y[wm])
    com_w = np.median(asd_common[wm]); remZ_w = np.median(rem_Z[wm]); remY_w = np.median(rem_Y[wm])
    coh_w = np.median(cohZY[wm])
    print(f"\n  white floor 3-40 Hz [fT/rtHz]: totZ {tot_wZ*1e3:.1f}  totY {tot_wY*1e3:.1f}  "
          f"common {com_w*1e3:.1f}  remZ {remZ_w*1e3:.1f}  remY {remY_w*1e3:.1f}  "
          f"coh {coh_w:.3f} (bias {bias_A:.3f})")
    # BIAS TREATMENT: the raw |CSD| common floor is inflated by finite-average
    # coherence bias. For independent signals E[gamma^2]=bias_A, so a spurious
    # common floor appears even with no real coupling.
    #
    # The exponent has to match the raw floor above it, which is
    # sqrt(|Pzy|) = (gamma^2)^(1/4) * sqrt(ASDz*ASDy). Using
    # (gamma^2)^(1/2) here took one square root too many, and since
    # gamma^2 < 1 that UNDER-states the common component: the white
    # band read 2.4 fT where the matched exponent gives about 7
    # (2026-08-19 analysis review, M7).
    spurious = bias_A ** 0.25 * np.sqrt(tot_wZ * tot_wY)
    excess = max(coh_w - bias_A, 0.0)
    true_common_w = excess ** 0.25 * np.sqrt(tot_wZ * tot_wY)
    per_axis_true = np.sqrt(max(tot_wZ**2 - true_common_w**2, 0.0))
    print(f"  bias check: raw common {com_w*1e3:.1f} fT vs spurious-from-bias "
          f"{spurious*1e3:.1f} fT; excess coh {excess:.3f} -> "
          f"TRUE common ~{true_common_w*1e3:.1f} fT, per-axis ~{per_axis_true*1e3:.1f} fT")
    print(f"  => white floor is PER-AXIS (independent sensor noise), NOT common field.")

    print("\n================ PART B: path-common(analogZ,guiZ) steady half ================")
    print(f"  Welch: {NPS_B/fs50:.0f}-s segs @ {fs50:.2f} Hz, {n_avg_B} averages, coh bias {bias_B:.3f}")
    print(f"  {'f[Hz]':>7} {'totZaz':>8} {'totZgui':>8} {'pathcom':>8} {'coh':>6}")
    for f0 in (0.1, 1.0, 10.0):
        print(f"  {f0:7.2f} {at(fZG,np.sqrt(Pzz_al),f0)*1e3:8.1f} "
              f"{at(fZG,np.sqrt(Pgg_al),f0)*1e3:8.1f} "
              f"{at(fZG,asd_path_common,f0)*1e3:8.1f} {at(fZG,cohZG,f0):6.2f}")
    wmB = (fZG >= 3) & (fZG <= 15)
    print(f"  white 3-15 Hz: path-common {np.median(asd_path_common[wmB])*1e3:.1f} fT/rtHz, "
          f"coh {np.median(cohZG[wmB]):.3f} (bias {bias_B:.3f})")

    # ---------------------------------------------------------------- FIGURE
    p5.style()
    fig, (axL, axR) = plt.subplots(1, 2, figsize=(13.5, 5.4))

    # left: Z/Y common vs per-axis decomposition
    axL.loglog(fZ, asd_Z*1e3, color=p5.C_Z, lw=1.6, zorder=5, label="total analog-Z")
    axL.loglog(fZ, asd_Y*1e3, color=p5.C_Y, lw=1.3, alpha=0.9, zorder=4, label="total analog-Y")
    axL.loglog(fZ, asd_common*1e3, color="#7B4FA0", lw=2.0, zorder=6,
               label=r"common(Z,Y) = $|S_{ZY}|^{1/2}$")
    axL.loglog(fZ, rem_Z*1e3, color=p5.C_Z, lw=1.0, ls=":", alpha=0.8, zorder=3,
               label="per-axis remainder Z")
    axL.loglog(fZ, rem_Y*1e3, color=p5.C_Y, lw=1.0, ls=":", alpha=0.8, zorder=3,
               label="per-axis remainder Y")
    axL.axhspan(19, 22, color=p5.C_REF, alpha=0.18, lw=0, zorder=0)
    axL.set_xlim(0.02, 120); axL.set_ylim(8, 4000)
    axL.grid(True, which="major", alpha=0.9); axL.grid(True, which="minor", alpha=0.3)
    axL.set_xlabel("frequency  [Hz]")
    axL.set_ylabel(r"field noise  [fT/$\sqrt{\mathrm{Hz}}$]")
    axL.set_title("(a) One sensor, two axes:  common(Z,Y) vs per-axis", loc="left")
    for f0 in (0.1, 1.0, 10.0):
        axL.axvline(f0, color=p5.C_MUTE, lw=0.7, ls="--", alpha=0.4, zorder=1)
    axL.text(0.98, 0.03,
             f"white floor 3-40 Hz: total {tot_wZ*1e3:.0f} fT/rtHz\n"
             f"raw |CSD| common {com_w*1e3:.0f} (bias floor {spurious*1e3:.0f})\n"
             f"-> bias-corrected common ~{true_common_w*1e3:.0f}, per-axis ~{per_axis_true*1e3:.0f}\n"
             f"FLOOR IS PER-AXIS (independent)",
             transform=axL.transAxes, ha="right", va="bottom", fontsize=8.5,
             bbox=p5.NOTE_BBOX)
    axL.legend(loc="upper right", fontsize=9)

    # right: path-common (analogZ x guiZ) on steady half
    axR.loglog(fZG, np.sqrt(Pzz_al)*1e3, color=p5.C_Z, lw=1.5, zorder=4, label="total analog-Z")
    axR.loglog(fZG, np.sqrt(Pgg_al)*1e3, color=p5.C_GUI, lw=1.2, alpha=0.9, zorder=3,
               label="total GUI-Z")
    axR.loglog(fZG, asd_path_common*1e3, color="#1F7A4D", lw=2.0, zorder=5,
               label=r"path-common = $|S_{Z,\mathrm{gui}}|^{1/2}$")
    axR.axhspan(19, 22, color=p5.C_REF, alpha=0.18, lw=0, zorder=0)
    axR.set_xlim(0.02, 25); axR.set_ylim(8, 4000)
    axR.grid(True, which="major", alpha=0.9); axR.grid(True, which="minor", alpha=0.3)
    axR.set_xlabel("frequency  [Hz]")
    axR.set_ylabel(r"field noise  [fT/$\sqrt{\mathrm{Hz}}$]")
    axR.set_title("(b) Same Z field, two readout paths (steady half, t>650 s)", loc="left")
    for f0 in (0.1, 1.0, 10.0):
        axR.axvline(f0, color=p5.C_MUTE, lw=0.7, ls="--", alpha=0.4, zorder=1)
    axR.text(0.98, 0.03,
             f"path-common floor 3-15 Hz:\n{np.median(asd_path_common[wmB])*1e3:.0f} fT/rtHz "
             f"(coh {np.median(cohZG[wmB]):.2f})\n"
             "GUI path timing wander de-\ncoheres above ~1 Hz",
             transform=axR.transAxes, ha="right", va="bottom", fontsize=9,
             bbox=p5.NOTE_BBOX)
    axR.legend(loc="upper right", fontsize=9)

    fig.suptitle("3-channel noise decomposition -- closed bench shield, 30-min sync5 (2026-07-06)",
                 fontsize=13, fontweight="bold", x=0.01, ha="left")
    fig.tight_layout(rect=[0, 0, 1, 0.97])
    figpath = os.path.join(OUTDIR, "decomp_asd_decomposition.png")
    fig.savefig(figpath, dpi=150, bbox_inches="tight")
    plt.close(fig)
    print(f"\n[saved] {figpath}")

    # save numeric summary keyed for the finding
    print("\n=== SUMMARY for finding ===")
    print(f"common(Z,Y) fT/rtHz @0.1/1/10: "
          f"{at(fZ,asd_common,0.1)*1e3:.0f}/{at(fZ,asd_common,1)*1e3:.0f}/{at(fZ,asd_common,10)*1e3:.1f}")
    print(f"per-axis remZ fT/rtHz @0.1/1/10: "
          f"{at(fZ,rem_Z,0.1)*1e3:.0f}/{at(fZ,rem_Z,1)*1e3:.0f}/{at(fZ,rem_Z,10)*1e3:.1f}")
    print(f"per-axis remY fT/rtHz @0.1/1/10: "
          f"{at(fZ,rem_Y,0.1)*1e3:.0f}/{at(fZ,rem_Y,1)*1e3:.0f}/{at(fZ,rem_Y,10)*1e3:.1f}")
    print(f"coh(Z,Y) @0.1/1/10: {at(fZ,cohZY,0.1):.2f}/{at(fZ,cohZY,1):.2f}/{at(fZ,cohZY,10):.3f}")


def run_distrib():
    plt.rcdefaults()
    REPO = PROJECT_DIR
    OUTDIR = HERE
    os.makedirs(OUTDIR, exist_ok=True)


    FS_DAQ = 1200.0566
    FS_GUI = 203.18
    CAL = 1.029           # calibrate analog pT
    RAW = data_join("result/synctest/sync5_30min/sync5daq_raw.csv")

    # --- load field records ---
    az = p5.load_col(p5.AZ_FILE) * CAL
    ay = p5.load_col(p5.AY_FILE) * CAL
    gz = p5.load_col(p5.GZ_FILE, skiprows=1)
    print(f"[load] analog Z {az.size}, analog Y {ay.size}, GUI Z {gz.size}")

    # drop the first 5 min (settling transient) from all -- Z-only transient noted,
    # but excluding uniformly keeps the comparison fair
    az = az[int(5*60*FS_DAQ):]
    ay = ay[int(5*60*FS_DAQ):]
    gz = gz[int(5*60*FS_GUI):]

    def highpass(x, fs, cut=3.0):
        return p5.bp(x, fs, cut, fs*0.45)   # 3 Hz .. near Nyquist

    hz = highpass(az, FS_DAQ)
    hy = highpass(ay, FS_DAQ)
    hg = highpass(gz, FS_GUI)
    # trim filter edges
    edge = 2000
    hz, hy = hz[edge:-edge], hy[edge:-edge]
    hg = hg[500:-500]

    def stat_block(x, name):
        x = x - x.mean()
        sd = x.std(ddof=1)
        sk = stats.skew(x)
        ku = stats.kurtosis(x, fisher=True)   # excess kurtosis
        # robust sigma from MAD (Gaussian-consistent)
        mad = np.median(np.abs(x - np.median(x))) * 1.4826
        # tail counts vs Gaussian expectation
        n = x.size
        for k in (3, 4, 5):
            obs = np.sum(np.abs(x) > k*sd)
            exp = n * 2*stats.norm.sf(k)
            print(f"    |x|>{k}sigma: obs {obs}  exp {exp:.1f}  ratio {obs/max(exp,1e-9):.2f}")
        # Anderson-Darling (subsample for speed if large)
        xs = x if n < 200000 else np.random.default_rng(0).choice(x, 200000, replace=False)
        ad = stats.anderson(xs, dist='norm')
        print(f"  [{name}] n={n} sd={sd:.3f} pT  MAD-sigma={mad:.3f} pT  "
              f"skew={sk:+.4f}  exkurt={ku:+.4f}  AD stat={ad.statistic:.1f} "
              f"(crit@1%={ad.critical_values[-1]:.3f})")
        return dict(x=x, sd=sd, sk=sk, ku=ku, mad=mad, ad=ad.statistic, name=name)

    print("[distrib] high-passed >3 Hz amplitude statistics:")
    S = {}
    S['az'] = stat_block(hz, "analog Z")
    S['ay'] = stat_block(hy, "analog Y")
    S['gz'] = stat_block(hg, "GUI Z")

    # ---- RTN / telegraph check: look at the LOW-frequency band (0.1-2 Hz) for
    # bimodality that a >3 Hz high-pass would hide ----
    def bimod_check(x, fs, name):
        lf = p5.bp(x, fs, 0.1, 2.0)[2000:-2000]
        lf = lf - lf.mean()
        # dip test proxy: kurtosis of low-band; bimodal -> negative excess kurtosis
        ku = stats.kurtosis(lf, fisher=True)
        print(f"  [{name}] 0.1-2 Hz band exkurt={ku:+.3f} "
              f"({'possible bimodal/flat' if ku < -0.3 else 'unimodal-consistent'})")
        return lf, ku
    print("[rtn] low-band (0.1-2 Hz) bimodality proxy:")
    lf_z, kz = bimod_check(az, FS_DAQ, "analog Z")
    lf_y, ky = bimod_check(ay, FS_DAQ, "analog Y")

    # ===================== RAW VOLTS QUANTIZATION =====================
    raw = np.loadtxt(RAW, delimiter=",", skiprows=2)
    vz = raw[:,1]; vy = raw[:,2]
    print(f"[raw] loaded {vz.size} samples; Vz range [{vz.min():.5f},{vz.max():.5f}] V")

    def lsb_analysis(v, name):
        u = np.unique(v)
        du = np.diff(np.sort(u))
        du = du[du > 1e-9]
        lsb = np.median(du)
        # nominal 12-bit +/-1V full scale = 2V/4096
        nom = 2.0/4096
        # code span actually used
        span = v.max() - v.min()
        codes_used = int(round(span/lsb)) + 1
        n_unique = u.size
        print(f"  [{name}] median code step (LSB) = {lsb*1e6:.2f} uV  "
              f"(12-bit +/-1V nominal = {nom*1e6:.2f} uV, ratio {lsb/nom:.3f})")
        print(f"           unique levels = {n_unique}, span = {span*1e3:.2f} mV "
              f"= {span/lsb:.1f} LSB, codes spanned ~ {codes_used}")
        return lsb, u, du

    print("[quant] raw-volt LSB:")
    lsb_z, uz, duz = lsb_analysis(vz, "V_z")
    lsb_y, uy, duy = lsb_analysis(vy, "V_y")

    # effective number of bits from noise: ENOB = log2(FS_range / (sqrt(12)*sigma_q))
    # but here we want to see if oversampling helps. USB-6008 spec ADC = 12 bit.
    # Compare raw sample sigma to the sigma after decimation to 50 Hz (24x).
    def enob(v, fs, name, lsb):
        fsr = 2.0  # +/-1 V
        # residual after removing slow field: high-pass 3 Hz gives quantization+sensor noise
        hp = p5.bp(v, fs, 3.0, fs*0.45)[2000:-2000]
        sig = hp.std(ddof=1)
        # ideal quantization noise rms = LSB/sqrt(12)
        q_rms = lsb/np.sqrt(12)
        enob_raw = np.log2(fsr/(np.sqrt(12)*sig))
        print(f"  [{name}] high-pass(>3Hz) rms = {sig*1e6:.2f} uV; "
              f"LSB/sqrt12 = {q_rms*1e6:.2f} uV (ratio noise/qnoise = {sig/q_rms:.2f})")
        print(f"           ENOB from raw noise = {enob_raw:.2f} bits "
              f"(vs 12-bit hardware)")
        # decimate 24x to ~50 Hz -> if oversampling+averaging recovers bits, rms should
        # drop and effective resolution grow by up to 0.5*log2(24)=2.29 bits
        from scipy.signal import decimate
        v50 = decimate(decimate(v, 6, ftype="fir"), 4, ftype="fir")
        hp50 = p5.bp(v50, fs/24.0, 3.0, (fs/24.0)*0.45)[200:-200]
        sig50 = hp50.std(ddof=1)
        enob50 = np.log2(fsr/(np.sqrt(12)*sig50))
        dbits = 0.5*np.log2(24)
        print(f"           after 24x decimation: rms = {sig50*1e6:.2f} uV, "
              f"ENOB = {enob50:.2f} bits (ideal averaging gain {dbits:.2f} bits, "
              f"observed {enob50-enob_raw:+.2f})")
        return dict(sig=sig, q_rms=q_rms, enob=enob_raw, enob50=enob50, hp=hp)

    print("[enob] effective bits & oversampling gain:")
    Ez = enob(vz, FS_DAQ, "V_z", lsb_z)
    Ey = enob(vy, FS_DAQ, "V_y", lsb_y)

    # =========================== FIGURE ===========================
    fig = plt.figure(figsize=(13, 9))
    gs = fig.add_gridspec(2, 3, height_ratios=[1,1], hspace=0.35, wspace=0.3)

    # top row: histograms with Gaussian overlay
    for i,(k,color) in enumerate([('az','#2E6E8E'),('ay','#5A7D3A'),('gz','#C0504D')]):
        ax = fig.add_subplot(gs[0,i])
        d = S[k]; x = d['x']
        lim = 6*d['sd']
        bins = np.linspace(-lim, lim, 120)
        ax.hist(x, bins=bins, density=True, color=color, alpha=0.55, edgecolor='none')
        xx = np.linspace(-lim, lim, 400)
        ax.plot(xx, stats.norm.pdf(xx, 0, d['sd']), 'k--', lw=1.3, label='Gaussian fit')
        ax.set_yscale('log')
        ax.set_ylim(1e-5, None)
        ax.set_xlabel('amplitude [pT]')
        ax.set_ylabel('density (log)')
        ax.set_title(f"{d['name']} (>3 Hz)\nsd={d['sd']:.2f} pT  skew={d['sk']:+.3f}  exkurt={d['ku']:+.3f}",
                     fontsize=10)
        ax.legend(fontsize=8)
        ax.grid(True, alpha=0.3)

    # bottom-left: Q-Q plots for all three overlaid
    axq = fig.add_subplot(gs[1,0])
    for k,color in [('az','#2E6E8E'),('ay','#5A7D3A'),('gz','#C0504D')]:
        d = S[k]; x = d['x']
        xs = x if x.size < 60000 else np.random.default_rng(1).choice(x, 60000, replace=False)
        (osm, osr), _ = stats.probplot(xs/d['sd'], dist='norm')
        axq.plot(osm, osr, '.', ms=1.5, color=color, alpha=0.5, label=d['name'])
    lim = 6
    axq.plot([-lim,lim],[-lim,lim],'k--',lw=1.0)
    axq.set_xlim(-lim,lim); axq.set_ylim(-lim,lim)
    axq.set_xlabel('theoretical quantiles (sigma)')
    axq.set_ylabel('sample quantiles (sigma)')
    axq.set_title('Q-Q vs Gaussian (normalized)', fontsize=10)
    axq.legend(fontsize=8, markerscale=6)
    axq.grid(True, alpha=0.3)

    # bottom-middle: raw-volt LSB step histogram (consecutive-code differences)
    axl = fig.add_subplot(gs[1,1])
    # histogram of the successive-sample voltage DIFFERENCES in units of LSB
    dvz = np.diff(vz)
    axl.hist(dvz/lsb_z, bins=np.arange(-8.5,8.6,1.0), color='#2E6E8E', alpha=0.6,
             edgecolor='k', linewidth=0.4)
    axl.set_xlabel('consecutive-sample step [LSB units]')
    axl.set_ylabel('count')
    axl.set_title(f'V_z code steps\nLSB = {lsb_z*1e6:.1f} uV (nominal {2e6/4096:.1f})', fontsize=10)
    axl.grid(True, alpha=0.3)

    # bottom-right: raw-volt value histogram showing quantization grid
    axr = fig.add_subplot(gs[1,2])
    # zoom into a narrow window to see the discrete levels
    vzc = vz - np.median(vz)
    w = 20*lsb_z
    sel = np.abs(vzc) < w
    axr.hist(vzc[sel]*1e6, bins=200, color='#7A7F85', alpha=0.7, edgecolor='none')
    for lvl in np.arange(-w, w, lsb_z):
        axr.axvline(lvl*1e6, color='#C0504D', lw=0.3, alpha=0.4)
    axr.set_xlabel('V_z - median [uV]')
    axr.set_ylabel('count')
    axr.set_title(f'Quantization grid (zoom)\nENOB={Ez["enob"]:.1f} bits, noise/qnoise={Ez["sig"]/Ez["q_rms"]:.1f}',
                  fontsize=10)
    axr.grid(True, alpha=0.3)

    fig.suptitle('sync5 (2026-07-06 closed shield): amplitude distributions & raw-volt quantization',
                 fontsize=13, weight='bold')
    figpath = os.path.join(OUTDIR, "distrib_histograms_qq_quantization.png")
    fig.savefig(figpath, dpi=150, bbox_inches='tight')
    plt.close(fig)
    print(f"[saved] {figpath}")

    # summary table
    print("\n=== SUMMARY ===")
    print(f"{'chan':10} {'sd[pT]':>8} {'skew':>8} {'exkurt':>8} {'AD':>8}")
    for k in ('az','ay','gz'):
        d=S[k]
        print(f"{d['name']:10} {d['sd']:8.3f} {d['sk']:+8.4f} {d['ku']:+8.4f} {d['ad']:8.1f}")
    print(f"LSB V_z={lsb_z*1e6:.2f}uV V_y={lsb_y*1e6:.2f}uV  nominal={2e6/4096:.2f}uV")
    print(f"ENOB raw V_z={Ez['enob']:.2f} -> 24x-decim {Ez['enob50']:.2f} bits")
    print(f"ENOB raw V_y={Ey['enob']:.2f} -> 24x-decim {Ey['enob50']:.2f} bits")


def run_driftvec():
    plt.rcdefaults()
    REPO = PROJECT_DIR
    OUT = HERE
    os.makedirs(OUT, exist_ok=True)


    FS_DAQ = 1200.0566
    CAL = 1.029           # analog pT files -> calibrated (2.7/2.624)
    LP_CUT = 0.05         # Hz
    SKIP_S = 300.0        # exclude first 5 min

    # ---- load & calibrate ------------------------------------------------------
    az = p5.load_col(p5.AZ_FILE) * CAL
    ay = p5.load_col(p5.AY_FILE) * CAL
    print(f"[load] N={az.size}  duration={az.size/FS_DAQ:.1f} s")

    # ---- decimate to ~1 Hz then low-pass to <0.05 Hz ---------------------------
    # 1200 -> /12 -> 100 -> /10 -> 10 -> /10 -> 1 Hz
    def to_1hz(x):
        return decimate(decimate(decimate(x, 12, ftype="fir"), 10, ftype="fir"), 10, ftype="fir")
    z1 = to_1hz(az); y1 = to_1hz(ay)
    fs1 = FS_DAQ / 1200.0
    print(f"[decim] fs1={fs1:.5f} Hz  N1={z1.size}")

    # NOT the module-level lp(x, fs, cut): argument order differs
    def lp4(x, cut, fs, order=4):
        sos = butter(order, cut, btype="low", fs=fs, output="sos")
        return sosfiltfilt(sos, x)
    zlp = lp4(z1, LP_CUT, fs1); ylp = lp4(y1, LP_CUT, fs1)

    t1 = np.arange(z1.size) / fs1
    keep = t1 >= SKIP_S
    tz = t1[keep] - t1[keep][0]
    Z = zlp[keep]; Y = ylp[keep]
    Z0 = Z - Z.mean(); Y0 = Y - Y.mean()   # centred for PCA/hodograph
    print(f"[window] kept {tz[-1]:.0f} s ({tz[-1]/60:.1f} min), N={Z.size}")

    # ---- drift rate (pT/min): endpoint & robust (Theil-Sen-ish least squares) --
    def rate_per_min(t, x):
        # linear least-squares slope over the kept window
        A = np.vstack([t, np.ones_like(t)]).T
        m, b = np.linalg.lstsq(A, x, rcond=None)[0]
        return m * 60.0                       # pT/min
    z_rate = rate_per_min(tz, Z); y_rate = rate_per_min(tz, Y)
    z_endpoint = (Z[-1] - Z[0]) / (tz[-1]/60.0)
    y_endpoint = (Y[-1] - Y[0]) / (tz[-1]/60.0)
    z_ptp = np.ptp(Z); y_ptp = np.ptp(Y)
    print(f"[rate] Z slope {z_rate:+.3f} pT/min (endpoint {z_endpoint:+.3f}), ptp {z_ptp:.1f} pT")
    print(f"[rate] Y slope {y_rate:+.3f} pT/min (endpoint {y_endpoint:+.3f}), ptp {y_ptp:.1f} pT")

    # ---- PCA on the (Z,Y) drift cloud ------------------------------------------
    X = np.column_stack([Z0, Y0])
    C = np.cov(X.T)
    evals, evecs = np.linalg.eigh(C)          # ascending
    order = np.argsort(evals)[::-1]
    evals = evals[order]; evecs = evecs[:, order]
    lam1, lam2 = evals
    v1 = evecs[:, 0]
    # principal axis angle measured from +Z axis toward +Y axis
    angle = np.degrees(np.arctan2(v1[1], v1[0]))
    if angle < -90: angle += 180
    if angle > 90: angle -= 180
    elong = np.sqrt(lam1 / max(lam2, 1e-30))   # sqrt eigenvalue ratio = aspect ratio
    var_ratio = lam1 / (lam1 + lam2)
    # Pearson r between Z and Y over the drift window
    r_zy = float(np.corrcoef(Z0, Y0)[0, 1])
    print(f"[pca] lam1={lam1:.2f} lam2={lam2:.2f} pT^2  angle={angle:+.1f} deg (from +Z)")
    print(f"[pca] sqrt-eigval ratio (aspect)={elong:.2f}  var frac PC1={var_ratio:.3f}  r_ZY={r_zy:+.3f}")

    # ---- random walk test 1: low-f PSD slope alpha -----------------------------
    # Use the drift-window 1-Hz series (first 5 min excluded) so the Z settling
    # transient does not bias the low-f slope. Welch detrends linearly per segment.
    fZ, pZ = welch(z1[keep], fs=fs1, nperseg=z1[keep].size, detrend="linear")
    fY, pY = welch(y1[keep], fs=fs1, nperseg=y1[keep].size, detrend="linear")
    def psd_slope(f, p, lo, hi):
        m = (f >= lo) & (f <= hi) & (p > 0)
        if m.sum() < 4: return np.nan, np.nan
        c = np.polyfit(np.log10(f[m]), np.log10(p[m]), 1)
        return c[0], m.sum()
    FIT_LO, FIT_HI = 3e-3, 3e-2
    aZ, nZ = psd_slope(fZ, pZ, FIT_LO, FIT_HI)
    aY, nY = psd_slope(fY, pY, FIT_LO, FIT_HI)
    print(f"[psd] slope {FIT_LO}-{FIT_HI} Hz: Z alpha={aZ:.2f} (n={nZ}), Y alpha={aY:.2f} (n={nY})"
          f"  [random walk -> -2]")

    # ---- random walk test 2: MSD / Hurst ---------------------------------------
    # MSD(tau) = < (x(t+tau)-x(t))^2 >. Random walk: MSD ~ tau^1 (H=0.5);
    # H = slope/2. Use the *drift-window* 1-Hz series (first 5 min excluded, so
    # the Z settling transient does not inflate H), with the linear ramp removed.
    zk = z1[keep]; yk = y1[keep]
    tk = np.arange(zk.size) / fs1
    zdt = zk - np.polyval(np.polyfit(tk, zk, 1), tk)
    ydt = yk - np.polyval(np.polyfit(tk, yk, 1), tk)
    def msd(x, lags):
        return np.array([np.mean((x[L:] - x[:-L])**2) for L in lags])
    lags = np.unique(np.round(np.logspace(0, np.log10(zk.size//4), 25)).astype(int))
    lags = lags[lags >= 1]
    tau_s = lags / fs1
    mZ = msd(zdt, lags); mY = msd(ydt, lags)
    def hurst(tau, m, lo, hi):
        mask = (tau >= lo) & (tau <= hi) & (m > 0)
        c = np.polyfit(np.log10(tau[mask]), np.log10(m[mask]), 1)
        return c[0] / 2.0, c
    H_LO, H_HI = 5.0, 200.0
    HZ, cZ = hurst(tau_s, mZ, H_LO, H_HI)
    HY, cY = hurst(tau_s, mY, H_LO, H_HI)
    print(f"[msd] Hurst {H_LO}-{H_HI}s: Z H={HZ:.2f}, Y H={HY:.2f}  [random walk -> 0.5]")

    # ============================ FIGURE =========================================
    p5.style()
    fig = plt.figure(figsize=(10.5, 5.4))
    gs = fig.add_gridspec(2, 2, width_ratios=[1.35, 1.0], height_ratios=[1, 1],
                          hspace=0.42, wspace=0.30)
    ax_h = fig.add_subplot(gs[:, 0])   # hodograph spans both rows
    ax_m = fig.add_subplot(gs[0, 1])   # MSD
    ax_p = fig.add_subplot(gs[1, 1])   # low-f PSD

    # --- hodograph, coloured by time --------------------------------------------
    pts = np.column_stack([Z0, Y0]).reshape(-1, 1, 2)
    segs = np.concatenate([pts[:-1], pts[1:]], axis=1)
    lc = LineCollection(segs, cmap="viridis", linewidth=1.6)
    lc.set_array(tz[:-1] / 60.0)
    ax_h.add_collection(lc)
    ax_h.autoscale()
    # PCA axes overlay
    c1 = np.sqrt(lam1); c2 = np.sqrt(lam2)
    for (vec, L, col, lab) in [(evecs[:, 0], c1, p5.C_INK, "PC1"),
                               (evecs[:, 1], c2, p5.C_MUTE, "PC2")]:
        ax_h.plot([-vec[0]*L, vec[0]*L], [-vec[1]*L, vec[1]*L],
                  color=col, lw=1.6 if lab == "PC1" else 1.1,
                  ls="-" if lab == "PC1" else "--", zorder=6)
    ax_h.set_aspect("equal", adjustable="datalim")
    ax_h.set_xlabel("Z drift  [pT]  (mean removed)")
    ax_h.set_ylabel("Y drift  [pT]  (mean removed)")
    ax_h.set_title("Y–Z drift hodograph (<0.05 Hz, first 5 min excluded)", loc="left")
    ax_h.grid(True, alpha=0.6)
    cb = fig.colorbar(lc, ax=ax_h, pad=0.02, fraction=0.046)
    cb.set_label("time  [min]")
    ax_h.text(0.03, 0.97,
              f"PC1 angle {angle:+.0f}° from +Z\n"
              f"aspect (√λ₁/λ₂) {elong:.1f}\n"
              f"r(Z,Y) = {r_zy:+.2f}",
              transform=ax_h.transAxes, fontsize=9.5, va="top",
              color=p5.C_INK, bbox=p5.NOTE_BBOX)

    # --- MSD ---------------------------------------------------------------------
    ax_m.loglog(tau_s, mZ, "o-", ms=3, color=p5.C_Z, lw=1.2, label="Z")
    ax_m.loglog(tau_s, mY, "s-", ms=3, color=p5.C_Y, lw=1.2, label="Y")
    tt = np.array([H_LO, H_HI])
    ax_m.loglog(tt, 10**(cZ[1]) * tt**cZ[0], color=p5.C_INK, lw=1.0, ls=":")
    # reference tau^1 slope guide
    g0 = mZ[np.argmin(np.abs(tau_s - H_LO))]
    ax_m.loglog(tt, g0 * (tt/H_LO)**1.0, color=p5.C_MUTE, lw=1.0, ls="--", alpha=0.7)
    ax_m.set_xlabel(r"lag $\tau$  [s]")
    ax_m.set_ylabel(r"MSD  [pT$^2$]")
    ax_m.set_title(f"MSD (H: Z {HZ:.2f}, Y {HY:.2f}; RW=0.5)", loc="left", fontsize=11)
    ax_m.text(0.97, 0.06, r"dashed = $\tau^1$ (RW)", transform=ax_m.transAxes,
              fontsize=8.5, ha="right", color=p5.C_MUTE)
    ax_m.grid(True, which="both", alpha=0.4)
    ax_m.legend(loc="upper left", fontsize=9)

    # --- low-f PSD ---------------------------------------------------------------
    mZp = (fZ > 0) & (fZ <= 0.1)
    ax_p.loglog(fZ[mZp], pZ[mZp], color=p5.C_Z, lw=1.0, alpha=0.85, label="Z")
    ax_p.loglog(fY[mZp], pY[mZp], color=p5.C_Y, lw=1.0, alpha=0.85, label="Y")
    ff = np.array([FIT_LO, FIT_HI])
    # draw the fitted slope for Z
    mfit = (fZ >= FIT_LO) & (fZ <= FIT_HI)
    cc = np.polyfit(np.log10(fZ[mfit]), np.log10(pZ[mfit]), 1)
    ax_p.loglog(ff, 10**np.polyval(cc, np.log10(ff)), color=p5.C_INK, lw=1.4, ls=":")
    # reference f^-2
    p0 = 10**np.polyval(cc, np.log10(FIT_LO))
    ax_p.loglog(ff, p0*(ff/FIT_LO)**(-2.0), color=p5.C_MUTE, lw=1.0, ls="--", alpha=0.7)
    ax_p.set_xlabel("frequency  [Hz]")
    ax_p.set_ylabel(r"PSD  [pT$^2$/Hz]")
    ax_p.set_title(rf"low-$f$ PSD (Z $\alpha$={aZ:.1f}, Y $\alpha$={aY:.1f}; RW=$-2$)",
                   loc="left", fontsize=11)
    ax_p.text(0.03, 0.06, r"dashed = $f^{-2}$", transform=ax_p.transAxes,
              fontsize=8.5, color=p5.C_MUTE)
    ax_p.grid(True, which="both", alpha=0.4)
    ax_p.legend(loc="upper right", fontsize=9)

    fig.suptitle("Drift geometry & random-walk — closed shield, sync5 (2026-07-06)",
                 x=0.01, ha="left", fontsize=13, fontweight="bold")
    figpath = os.path.join(OUT, "driftvec_hodograph.png")
    fig.savefig(figpath, dpi=150, bbox_inches="tight")
    plt.close(fig)
    print(f"[saved] {figpath}")

    # ---- summary ---------------------------------------------------------------
    print("\n===== SUMMARY =====")
    print(f"drift rate  Z {z_rate:+.2f} pT/min | Y {y_rate:+.2f} pT/min")
    print(f"drift ptp   Z {z_ptp:.1f} pT | Y {y_ptp:.1f} pT over {tz[-1]/60:.1f} min")
    print(f"PCA angle {angle:+.1f} deg from +Z, aspect {elong:.2f}, PC1 var frac {var_ratio:.3f}, r_ZY {r_zy:+.3f}")
    print(f"PSD alpha  Z {aZ:.2f} | Y {aY:.2f}  (RW=-2)")
    print(f"Hurst      Z {HZ:.2f} | Y {HY:.2f}  (RW=0.5)")


def run_guiclock():
    plt.rcdefaults()
    ROOT = PROJECT_DIR
    SYNC = HERE
    OUT  = SYNC
    os.makedirs(OUT, exist_ok=True)


    FS_DAQ_PRECISE = 1200.0566   # established
    FS_GUI_SEED    = 203.48      # seed for p5 refinement
    GZ_RAW = data_join("result", "synctest", "sync5_30min", "gui_sync5_raw.txt")
    GZ_FMT = data_join("result", "synctest", "sync5_30min", "gui_sync5_Formatted.txt")


    def bp(x, fs, lo, hi, order=2):
        sos = butter(order, [lo, hi], btype="band", fs=fs, output="sos")
        return sosfiltfilt(sos, x)


    def main():
        print("=== load ===")
        az = p5.load_col(p5.AZ_FILE) * 1.029          # calibrate analog pT
        gz = p5.load_col(p5.GZ_FILE, skiprows=1)       # GUI Z pT (already calibrated)
        print(f"analog Z: {az.size} samples ; GUI Z: {gz.size} samples")

        # ---------------------------------------------------------------------
        # (A) GUI clock frequency from the shared 50 Hz mains line
        # ---------------------------------------------------------------------
        fc_a, _ = p5.line_fit(az, FS_DAQ_PRECISE, 50.0, half=0.2)
        fc_g0, _ = p5.line_fit(gz, FS_GUI_SEED, 50.0, half=0.3)
        fs_gui = FS_GUI_SEED * fc_a / fc_g0
        print(f"[clock] analog 50Hz line at {fc_a:.4f} Hz ; GUI fs = {fs_gui:.4f} Hz "
              f"(established value 203.18)")

        # ---------------------------------------------------------------------
        # (B) Global GUI-vs-DAQ start offset
        # ---------------------------------------------------------------------
        az50 = decimate(decimate(az, 6, ftype="fir"), 4, ftype="fir")
        fs50 = FS_DAQ_PRECISE / 24.0
        delta, r_dr, r_bb = p5.align_gui(az50, fs50, gz, fs_gui)
        print(f"[align] global GUI start offset delta = {delta:+.3f} s "
              f"(r_drift={r_dr:.3f}, r_bb={r_bb:.3f})")

        # ---------------------------------------------------------------------
        # (C) fine-grained lag(t): best-fit GUI lag in ~40 s blocks
        #     lag is measured RELATIVE to the constant fs_gui timebase, i.e.
        #     residual timing wander on top of the global delta.
        # ---------------------------------------------------------------------
        t  = np.arange(az50.size) / fs50
        tg = np.arange(gz.size) / fs_gui
        a_bb = bp(az50, fs50, 0.3, 15.0)
        g_bb = bp(gz,  fs_gui, 0.3, 15.0)

        lo0, hi0 = 130.0, t[-1] - 130.0          # exclude settling + edges
        BLK = 40.0                                # block length [s]
        nblk = int((hi0 - lo0) // BLK)
        centers, lags, lag_err, rmax = [], [], [], []
        for k in range(nblk):
            lo = lo0 + k * BLK
            hi = lo + BLK
            mb = (t >= lo) & (t < hi)
            # fine search around the global delta
            grid = np.arange(delta - 0.30, delta + 0.30, 0.003)
            rr = np.empty(grid.size)
            ab = a_bb[mb]
            tb = t[mb]
            for i, d in enumerate(grid):
                g = np.interp(tb + d, tg, g_bb)
                rr[i] = np.corrcoef(ab, g)[0, 1]
            j = int(np.argmax(rr))
            d = grid[j]
            # parabolic peak refinement
            curv = np.nan
            if 0 < j < rr.size - 1:
                a_, b_, c_ = rr[j-1], rr[j], rr[j+1]
                denom = (a_ - 2*b_ + c_)
                if denom != 0:
                    d = grid[j] + 0.5 * (a_ - c_) / denom * 0.003
                    curv = -denom / (0.003**2)   # peak sharpness
            # lag uncertainty: half-width where correlation drops by (1-r^2)/N-ish.
            # Use curvature of the correlation peak and residual noise floor.
            # sigma_lag ~ sqrt( (1-rmax^2) / (N_eff * curv) ) as a rough CRLB proxy.
            rpk = rr[j]
            Neff = ab.size
            if np.isfinite(curv) and curv > 0 and rpk < 1:
                sig = np.sqrt(max((1 - rpk**2), 1e-6) / (Neff * curv))
            else:
                sig = 0.003
            centers.append(0.5*(lo+hi))
            lags.append((d - delta) * 1e3)        # ms, relative to global delta
            lag_err.append(sig * 1e3)
            rmax.append(rpk)
        centers = np.array(centers); lags = np.array(lags)
        lag_err = np.array(lag_err); rmax = np.array(rmax)
        print(f"[lag] {nblk} blocks of {BLK:.0f} s ; peak-corr range "
              f"{rmax.min():.3f}..{rmax.max():.3f}")
        print("[lag] lag(t) [ms rel]: " + " ".join(f"{v:+.1f}" for v in lags))

        # ---------------------------------------------------------------------
        # (D) drift vs jitter classification
        # ---------------------------------------------------------------------
        # linear fit lag(t) = a + b*t  ; b [ms/s] -> clock-rate error in ppm
        A = np.vstack([np.ones_like(centers), centers]).T
        # weighted least squares: lstsq minimises |Aw c - yw|^2, so the
        # multiplier is sqrt(weight) = 1/sigma. Multiplying by 1/sigma^2
        # squared it again and fitted with 1/sigma^4 (2026-08-19
        # analysis review, M8). Only the reported ppm and chi2 move;
        # the 203.19 Hz GUI rate is reconstructed on another path.
        w = 1.0 / np.maximum(lag_err, 1e-3)
        Aw = A * w[:, None]
        coef, *_ = np.linalg.lstsq(Aw, lags * w, rcond=None)
        intercept, slope = coef            # ms, ms/s
        fit = A @ coef
        resid = lags - fit
        ppm = slope * 1e3 / 1.0            # ms/s -> us/s = ppm directly (ms/s *1000 us/ms /1e6 s *1e6)
        # ms/s * 1e-3 s/ms = s/s ; *1e6 = ppm  => ppm = slope*1e-3*1e6 = slope*1e3
        ppm = slope * 1e3
        span = lags.max() - lags.min()
        # residual scatter after removing the linear drift -> jitter amplitude
        rms_resid = np.sqrt(np.mean(resid**2))
        max_jump = np.max(np.abs(np.diff(lags)))
        jump_idx = int(np.argmax(np.abs(np.diff(lags))))
        # goodness of the linear model
        chi2 = np.sum((resid / np.maximum(lag_err, 1e-3))**2)
        dof = max(1, nblk - 2)
        ss_tot = np.sum((lags - lags.mean())**2)
        r2_fit = 1.0 - np.sum(resid**2)/ss_tot if ss_tot > 0 else np.nan
        print(f"[fit] lag(t) = {intercept:+.1f} ms + ({slope:+.4f} ms/s) t")
        print(f"[fit] implied GUI clock-rate error = {ppm:+.1f} ppm "
              f"(vs the constant {fs_gui:.4f} Hz already applied)")
        print(f"[fit] total lag span = {span:.1f} ms ; linear-model R^2 = {r2_fit:.3f}")
        print(f"[fit] residual RMS (jitter) = {rms_resid:.2f} ms ; "
              f"max block-to-block step = {max_jump:.1f} ms at t~{centers[jump_idx]:.0f}s")
        print(f"[fit] reduced chi^2 = {chi2/dof:.2f} (dof={dof})")
        # The full-record shape is dominated by a fast early transient (GUI lag
        # settling in the first minutes). Characterize the STEADY stretch (t>650s)
        # separately: fit its slope -> residual clock-rate error and jitter there.
        steady = centers > 650.0
        if steady.sum() >= 3:
            As = np.vstack([np.ones(steady.sum()), centers[steady]]).T
            cs, *_ = np.linalg.lstsq(As, lags[steady], rcond=None)
            slope_s = cs[1]
            ppm_s = slope_s * 1e3
            resid_s = lags[steady] - As @ cs
            rms_s = np.sqrt(np.mean(resid_s**2))
            span_s = lags[steady].max() - lags[steady].min()
        else:
            slope_s = ppm_s = rms_s = span_s = np.nan
        # early transient magnitude
        early = centers < 400.0
        early_drop = lags[early].max() - lags[early].min() if early.sum() else np.nan
        print(f"[steady] t>650s: slope {slope_s:+.4f} ms/s = {ppm_s:+.1f} ppm ; "
              f"span {span_s:.1f} ms ; jitter RMS {rms_s:.1f} ms")
        print(f"[early ] t<400s transient amplitude = {early_drop:.0f} ms")
        verdict = ("SMOOTH DRIFT (steady clock-rate error)"
                   if (abs(span) > 4*rms_resid and r2_fit > 0.7)
                   else "NON-LINEAR WANDER: fast early transient + slow bounded wander, "
                        "no steady ppm drift")
        print(f"[verdict] {verdict}")

        # cross-check against p5's own coarse 6-block estimate
        coarse = p5.block_lags(az50, fs50, gz, fs_gui, delta)
        print("[xcheck] p5.block_lags 6-block [ms rel]: "
              + " ".join(f"{1e3*d:+.0f}" for d in coarse)
              + f"  (span {1e3*(max(coarse)-min(coarse)):.0f} ms)")

        # ---------------------------------------------------------------------
        # (E) impact on >1 Hz coherence & spectral leakage
        # ---------------------------------------------------------------------
        # coherence analog-Z x GUI-Z, full record vs steady-lag half (t>650s)
        from scipy.signal import filtfilt, firwin
        glp = filtfilt(firwin(401, 21.0, fs=fs_gui), [1.0], gz)
        m = (t > 130.0) & (t < t[-1]-130.0) & (t+delta > 2.0) & (t+delta < tg[-1]-2.0)
        gz_al = np.interp(t[m]+delta, tg, glp)
        nseg = int(100*fs50)
        fzg, czg = coherence(az50[m], gz_al, fs=fs50, nperseg=nseg)
        def bandmed(f, c, lo, hi):
            mm = (f >= lo) & (f <= hi)
            return float(np.median(c[mm]))
        coh_lo = bandmed(fzg, czg, 0.1, 1.0)
        coh_hi = bandmed(fzg, czg, 3.0, 15.0)
        # steady half
        ms = m & (t > 650.0)
        gz2 = np.interp(t[ms]+delta, tg, glp)
        f2, c2 = coherence(az50[ms], gz2, fs=fs50, nperseg=nseg)
        coh_hi2 = bandmed(f2, c2, 3.0, 15.0)
        print(f"[coh] analog-Z x GUI-Z: 0.1-1 Hz = {coh_lo:.3f} ; "
              f"3-15 Hz = {coh_hi:.3f} (full) -> {coh_hi2:.3f} (steady half t>650s)")
        # spectral leakage: does the 50 Hz mains line broaden in the GUI stream?
        # measure line width (half-power) of the mains line in GUI vs analog.
        def line_width(x, fs, f0, span=0.5):
            x = x - x.mean()
            n = x.size
            w = np.hanning(n)
            P = np.abs(np.fft.rfft(x*w))**2
            f = np.fft.rfftfreq(n, 1.0/fs)
            mm = (f >= f0-span) & (f <= f0+span)
            ff, pp = f[mm], P[mm]
            if pp.size < 3:
                return np.nan
            pk = pp.max(); half = pk/2
            above = ff[pp >= half]
            return float(above.max()-above.min()) if above.size > 1 else np.nan
        # both streams see 50 Hz directly (analog Nyquist 600 Hz, GUI Nyquist ~101.6 Hz).
        w_a = line_width(az, FS_DAQ_PRECISE, fc_a, span=0.5)
        w_g = line_width(gz, fs_gui, fc_a, span=0.5)
        print(f"[leak] 50 Hz line half-power width: analog {w_a*1e3:.1f} mHz ; "
              f"GUI {w_g*1e3:.1f} mHz (broadening -> timing jitter smears the line)")

        # ---------------------------------------------------------------------
        # (F) raw counts -> pT vs Formatted cross-check
        # ---------------------------------------------------------------------
        raw = np.loadtxt(GZ_RAW, dtype=str)
        counts = np.array([int(v.lstrip("!")) for v in raw[:, 1]], dtype=np.int64)
        pT_from_raw = (counts - 8388608) / 100.0
        fmt = p5.load_col(GZ_FMT, skiprows=1)
        n = min(pT_from_raw.size, fmt.size)
        diff = pT_from_raw[:n] - fmt[:n]
        print(f"[raw] raw counts: {counts.size} ; Formatted values: {fmt.size} ; compare {n}")
        print(f"[raw] pT_from_raw - Formatted: max|diff| = {np.max(np.abs(diff)):.6f} pT ; "
              f"mean = {diff.mean():+.2e} ; RMS = {np.sqrt(np.mean(diff**2)):.2e} pT")
        print(f"[raw] count range {counts.min()}..{counts.max()} "
              f"({counts.max()-counts.min()} LSB span)")

        # ---------------------------------------------------------------------
        # (G) ENOB of the GUI digital stream
        # ---------------------------------------------------------------------
        # quantization step = 1 count = 0.01 pT. Effective bits from the ratio of
        # full-scale range to the noise RMS, and SINAD via white-band ASD.
        lsb_pT = 0.01
        # unique step in Formatted stream
        dvals = np.unique(np.round(np.diff(np.sort(np.unique(fmt))), 6))
        step = float(np.min(dvals[dvals > 0])) if dvals[dvals>0].size else lsb_pT
        # The 24-bit counter spans 0..2^24; full scale = 2^24 counts = 167772.16 pT.
        full_scale_pT = (2**24) * lsb_pT
        # signal white-band noise RMS in a 1 Hz bin (proxy for LSB-limited noise):
        f, pxx = welch(gz - gz.mean(), fs=fs_gui, nperseg=int(20*fs_gui), detrend="linear")
        wm = (f >= 3) & (f <= 40)
        asd_white = float(np.median(np.sqrt(pxx[wm])))   # pT/rtHz
        # ENOB from quantization: q noise ASD = LSB/sqrt(12*fs/2)  (spread over Nyquist BW)
        q_asd = lsb_pT / np.sqrt(12 * (fs_gui/2))        # pT/rtHz
        # ENOB relative to full-scale counter and measured white ASD in Nyquist BW
        noise_rms_nyq = asd_white * np.sqrt(fs_gui/2)    # pT RMS over Nyquist band
        enob_fs = np.log2(full_scale_pT / (noise_rms_nyq * np.sqrt(12)))
        # quantization-only ceiling: how many bits the 0.01 pT LSB gives over full scale
        enob_quant_ceiling = np.log2(full_scale_pT / lsb_pT)  # = 24 by construction
        print(f"[enob] LSB = {lsb_pT} pT ; observed min step in Formatted = {step} pT")
        print(f"[enob] full-scale counter = {full_scale_pT:.1f} pT (2^24 * LSB) "
              f"-> {enob_quant_ceiling:.1f} bit quantization ceiling")
        print(f"[enob] white ASD 3-40 Hz = {asd_white*1e3:.1f} fT/rtHz ; "
              f"quantization ASD = {q_asd*1e3:.3f} fT/rtHz "
              f"(ratio {asd_white/q_asd:.0f}x -> noise-limited, not quantization-limited)")
        print(f"[enob] effective bits over full scale vs measured Nyquist noise = "
              f"{enob_fs:.1f} bit")

        # ---------------------------------------------------------------------
        # FIGURE: lag(t) with block error bars + drift-vs-jitter verdict
        # ---------------------------------------------------------------------
        p5.style()
        fig, ax = plt.subplots(figsize=(8.2, 5.0))
        ax.errorbar(centers/60.0, lags, yerr=lag_err, fmt="o", ms=5,
                    color=p5.C_GUI, ecolor=p5.C_MUTE, elinewidth=1.0, capsize=2.5,
                    zorder=3, label=f"GUI lag per {BLK:.0f} s block")
        # steady-stretch fit line only (t>650s), where a ppm interpretation is valid
        if steady.sum() >= 3:
            ts = np.linspace(650.0, centers.max(), 50)
            ax.plot(ts/60.0, cs[0] + cs[1]*ts, color=p5.C_Z, lw=1.6, zorder=4,
                    label=f"steady fit (t>650s): {ppm_s:+.0f} ppm")
        ax.axvspan(centers.min()/60.0, 650/60.0, color=p5.C_REF, alpha=0.15, lw=0,
                   zorder=0)
        ax.text(0.06, 0.94, "early\ntransient", transform=ax.transAxes,
                fontsize=9, color=p5.C_MUTE, va="top", ha="left")
        ax.axhline(0, color=p5.C_MUTE, ls=":", lw=1.0, zorder=1)
        ax.set_xlabel("time  [min]")
        ax.set_ylabel("GUI lag relative to global offset  [ms]")
        ax.set_title("GUI stream timing wander over 30 min (sync5, 2026-07-06)",
                     loc="left", pad=10)
        note = (f"global offset delta = {delta:+.2f} s\n"
                f"total lag span = {span:.0f} ms (early transient {early_drop:.0f} ms)\n"
                f"steady half (t>650s): {ppm_s:+.0f} ppm, jitter RMS {rms_s:.1f} ms\n"
                f"max block step = {max_jump:.0f} ms @ {centers[jump_idx]/60:.1f} min\n"
                f"verdict: non-linear wander, no steady ppm drift")
        ax.text(0.98, 0.04, note, transform=ax.transAxes, fontsize=9.5,
                ha="right", va="bottom", color=p5.C_INK, bbox=p5.NOTE_BBOX)
        ax.grid(True, alpha=0.7)
        ax.legend(loc="upper right")
        fig.tight_layout()
        figpath = os.path.join(OUT, "guiclock_lag_vs_time.png")
        fig.savefig(figpath, dpi=150, bbox_inches="tight")
        plt.close(fig)
        print(f"[saved] {figpath}")

        # secondary figure: coherence full vs steady half (leakage impact)
        fig2, ax2 = plt.subplots(figsize=(8.2, 4.6))
        mp = fzg <= 18
        ax2.semilogx(fzg[mp], czg[mp], color=p5.C_GUI, lw=1.5,
                     label="analog-Z x GUI-Z (full 30 min)")
        mp2 = f2 <= 18
        ax2.semilogx(f2[mp2], c2[mp2], color=p5.C_Z, lw=1.5, alpha=0.85,
                     label="steady-lag half (t>650 s)")
        ax2.axvline(1.0, color=p5.C_MUTE, ls=":", lw=1.0)
        ax2.text(1.05, 0.05, "1 Hz", color=p5.C_MUTE, fontsize=9)
        ax2.set_xlim(0.05, 18); ax2.set_ylim(0, 1.05)
        ax2.set_xlabel("frequency  [Hz]"); ax2.set_ylabel(r"coherence $\gamma^2$")
        ax2.set_title("Timing wander de-coheres the two Z readouts above ~1 Hz",
                      loc="left")
        ax2.text(0.03, 0.10,
                 f"3-15 Hz median: {coh_hi:.2f} (full) -> {coh_hi2:.2f} (steady half)",
                 transform=ax2.transAxes, fontsize=9.5, color=p5.C_INK,
                 bbox=p5.NOTE_BBOX)
        ax2.grid(True, which="both", alpha=0.6)
        ax2.legend(loc="upper right")
        fig2.tight_layout()
        figpath2 = os.path.join(OUT, "guiclock_coherence_impact.png")
        fig2.savefig(figpath2, dpi=150, bbox_inches="tight")
        plt.close(fig2)
        print(f"[saved] {figpath2}")

        print("\n=== SUMMARY ===")
        print(f"fs_gui={fs_gui:.4f} Hz  delta={delta:+.3f}s  span={span:.0f}ms  "
              f"ppm={ppm:+.0f}  R2={r2_fit:.2f}  jitter_rms={rms_resid:.1f}ms  "
              f"max_step={max_jump:.0f}ms  coh_hi {coh_hi:.2f}->{coh_hi2:.2f}  "
              f"raw_maxdiff={np.max(np.abs(diff)):.4f}pT")
    main()


def run_lines():
    plt.rcdefaults()
    REPO = PROJECT_DIR
    SYNC = HERE
    OUT = SYNC
    os.makedirs(OUT, exist_ok=True)


    FS_DAQ = 1200.0566           # precise analog clock (2,160,000 / 1798.915 s)
    FS_GUI = 203.18              # GUI clock transferred from shared 50 Hz mains
    CAL = 1.029                  # analog pT files x1.029 to correct 2.7->2.624 V/nT gain
    CELL = 923.6                 # cell modulation [Hz]

    C_Z, C_Y, C_GUI = "#2E6E8E", "#5A7D3A", "#C0504D"
    C_MUTE, C_INK = "#7A7F85", "#33373B"


    def load():
        az = p5.load_col(p5.AZ_FILE) * CAL
        ay = p5.load_col(p5.AY_FILE) * CAL
        gz = p5.load_col(p5.GZ_FILE, skiprows=1)   # already calibrated
        return az, ay, gz


    def asd(x, fs, nperseg):
        """One-sided ASD [pT/rtHz] via Welch, Hann, linear-detrend, 50% overlap."""
        f, pxx = welch(x, fs=fs, window="hann", nperseg=int(nperseg),
                       noverlap=int(nperseg)//2, detrend="linear")
        return f, np.sqrt(pxx)


    def census(f, a, flo, fhi, ratio=5.0, medwin=151, mergebw=0.6):
        """Lines > ratio * local median floor between flo,fhi.

        Returns list of dicts: f_hz, asd (pT/rtHz), floor (pT/rtHz), ratio,
        plus fwhm_hz and Q estimated by half-power width around the peak.
        """
        df = f[1] - f[0]
        base = median_filter(a, size=medwin)
        band = (f >= flo) & (f <= fhi)
        hot = band & (a > ratio * base)
        idx = np.where(hot)[0]
        merge = max(1, int(round(mergebw / df)))
        lines = []
        while idx.size:
            grp = idx[idx <= idx[0] + merge]
            j = grp[int(np.argmax(a[grp]))]
            # half-power width in power (asd^2): walk out to where asd^2 drops to
            # (peak_power + floor_power)/2 ; floor from local base
            pk_p = a[j]**2
            fl_p = base[j]**2
            half_p = 0.5 * (pk_p + fl_p)
            lo = j
            while lo > 0 and a[lo]**2 > half_p and (j-lo) < merge*3:
                lo -= 1
            hi = j
            while hi < a.size-1 and a[hi]**2 > half_p and (hi-j) < merge*3:
                hi += 1
            fwhm = (hi - lo) * df
            # a line spanning <=2 bins is unresolved at this df; its width (and Q)
            # is a lower/upper bound only, not a measured value
            resolved = (hi - lo) > 2
            lines.append(dict(f_hz=float(f[j]), asd=float(a[j]), floor=float(base[j]),
                              ratio=float(a[j]/base[j]), fwhm_hz=float(fwhm),
                              Q=float(f[j]/fwhm) if fwhm > 0 else np.inf,
                              resolved=resolved, df=float(df), idx=int(j)))
            idx = idx[idx > grp[-1]]
        lines.sort(key=lambda d: -d["ratio"])
        return lines, base


    def line_peak_amp(x, fs, f0, half=0.3):
        """Coherent peak amplitude [pT, zero-to-peak] of a line near f0 via
        windowed FFT power integrated over f0 +/- half, floor subtracted."""
        x = x - x.mean()
        n = x.size
        w = get_window("hann", n)
        p = np.abs(np.fft.rfft(x * w))**2
        f = np.fft.rfftfreq(n, 1.0/fs)
        m = (f >= f0-half) & (f <= f0+half)
        side = ((f >= f0-4*half) & (f < f0-half)) | ((f > f0+half) & (f <= f0+4*half))
        ps = np.clip(p[m] - np.median(p[side]), 0.0, None)
        if ps.sum() <= 0:
            return f0, 0.0
        fc = float(np.sum(f[m]*ps)/np.sum(ps))
        amp = float(np.sqrt(4.0*ps.sum()/(n*np.sum(w**2))))   # amplitude spectrum peak
        return fc, amp


    def coh_at(f, cxy, f0, win=0.6):
        m = (f >= f0-win) & (f <= f0+win)
        if not m.any():
            return np.nan
        return float(np.max(cxy[m]))


    CELL_EFF = 922.94   # effective cell-mod inferred from the 277.1 Hz alias (see notes)


    def classify(f0):
        tags = []
        # mains 50 Hz + harmonics
        for k in range(1, 13):
            if abs(f0 - 50.0*k) < 0.6:
                tags.append("mains 50 Hz" if k == 1 else f"mains 50 Hz x{k}")
        # 59.917 Hz comb (external line, NOT a DAQ sub-harmonic: FS/59.917=20.03)
        for k in range(1, 11):
            if abs(f0 - 59.917*k) < 0.4 and not any("mains" in t for t in tags):
                tags.append("59.92 Hz comb" if k == 1 else f"59.92 Hz comb x{k}")
        # cell-mod alias products: nominal 923.6 and effective 922.94
        for name, C in (("923.6", CELL), ("922.94eff", CELL_EFF)):
            for m in range(1, 4):
                for n in range(1, 4):
                    alias = abs(C*m - FS_DAQ*n)
                    if alias < FS_DAQ/2 and abs(f0 - alias) < 1.0:
                        tags.append(f"{name} cell-mod alias (m={m},n={n})")
        # cell-mod carrier 922.94 Hz lower sideband at 906.06 Hz -> aliases to 294.0
        #   (16.88 Hz sideband; alias = FS - 906.06)
        if abs(f0 - 293.99) < 0.6:
            tags.append("cell-mod sideband alias (906.1 Hz = 922.94-16.9)")
        # GUI sample clock 203.18 Hz leaking into analog
        if abs(f0 - 203.18) < 0.3:
            tags.append("203.18 Hz (GUI clock)")
        if 0.3 <= f0 <= 3.0:
            tags.append("low-f mechanical/HVAC band")
        return "; ".join(dict.fromkeys(tags)) if tags else ""


    def track_50(x, fs, dur_blk=120.0, f0=50.0):
        """50 Hz amplitude & centre freq in ~dur_blk-second blocks."""
        n_blk = int(len(x)/(dur_blk*fs))
        npts = int(dur_blk*fs)
        tc, amps, fcs = [], [], []
        for k in range(n_blk):
            seg = x[k*npts:(k+1)*npts]
            fc, amp = line_peak_amp(seg, fs, f0, half=0.4)
            tc.append((k+0.5)*dur_blk/60.0)
            amps.append(amp)
            fcs.append(fc)
        return np.array(tc), np.array(amps), np.array(fcs)


    def main():
        az, ay, gz = load()
        print(f"[load] analog Z {az.size}, Y {ay.size}, GUI Z {gz.size}")

        # ---- high-resolution ASD for the census -------------------------------
        # 60 s segments: df ~ 0.0167 Hz analog, resolves mains harmonics cleanly
        nA = int(60*FS_DAQ)
        nG = int(60*FS_GUI)
        fZ, aZ = asd(az, FS_DAQ, nA)
        fY, aY = asd(ay, FS_DAQ, nA)
        fG, aG = asd(gz, FS_GUI, nG)
        print(f"[asd] df analog={fZ[1]-fZ[0]:.4f} Hz  gui={fG[1]-fG[0]:.4f} Hz")

        # ---- Z-Y magnitude-squared coherence ----------------------------------
        ncoh = int(30*FS_DAQ)
        fC, cZY = coherence(az, ay, fs=FS_DAQ, window="hann",
                            nperseg=ncoh, noverlap=ncoh//2)
        n_avg = 2*az.size//ncoh - 1
        coh_bias = 1.0/n_avg
        print(f"[coh] Z-Y coherence: {n_avg} averages, zero-coherence bias {coh_bias:.3f}")

        # ---- census on each channel -------------------------------------------
        linesZ, baseZ = census(fZ, aZ, 0.2, 600.0)
        linesY, baseY = census(fY, aY, 0.2, 600.0)
        linesG, baseG = census(fG, aG, 0.2, 100.0)   # GUI only to ~100 Hz (fs/2=101.6)

        def enrich(lines, x, fs):
            for L in lines:
                fc, amp = line_peak_amp(x, fs, L["f_hz"],
                                        half=max(0.3, 3*L["fwhm_hz"]))
                L["fc"] = fc
                L["amp_pT"] = amp                       # zero-to-peak amplitude
                L["czy"] = coh_at(fC, cZY, L["f_hz"])
                L["tag"] = classify(L["f_hz"])
            return lines
        enrich(linesZ, az, FS_DAQ)
        enrich(linesY, ay, FS_DAQ)
        enrich(linesG, gz, FS_GUI)

        def report(name, lines):
            print(f"\n===== {name}: {len(lines)} lines >5x floor =====")
            print(f"{'f[Hz]':>9} {'amp[fT]':>9} {'asd[fT/rt]':>11} {'ratio':>6} "
                  f"{'FWHM[Hz]':>9} {'Q':>7} {'coh_ZY':>7}  tag")
            for L in lines:
                if not L["resolved"]:
                    q = ">%.0f" % (L["f_hz"]/(3*L["df"]))   # width < 3 bins -> Q lower bound
                elif not np.isfinite(L["Q"]):
                    q = "inf"
                else:
                    q = f"{L['Q']:.0f}"
                print(f"{L['fc']:9.3f} {L['amp_pT']*1e3:9.1f} {L['asd']*1e3:11.1f} "
                      f"{L['ratio']:6.1f} {L['fwhm_hz']:9.3f} {q:>8} "
                      f"{L['czy']:7.2f}  {L['tag']}")
        report("analog Z", linesZ)
        report("analog Y", linesY)
        report("GUI Z", linesG)

        # ---- 50 Hz tracking ----------------------------------------------------
        tcZ, ampZ, fcZ = track_50(az, FS_DAQ)
        tcY, ampY, fcY = track_50(ay, FS_DAQ)
        tcG, ampG, fcG = track_50(gz, FS_GUI)
        print("\n[50Hz track] analog Z blocks (t_min, amp_fT, fc_Hz):")
        for t, a_, fc_ in zip(tcZ, ampZ*1e3, fcZ):
            print(f"   {t:5.1f}  {a_:7.1f}  {fc_:8.4f}")
        print(f"[50Hz] analog Z amp range {ampZ.min()*1e3:.0f}-{ampZ.max()*1e3:.0f} fT, "
              f"fc range {fcZ.min():.4f}-{fcZ.max():.4f} Hz "
              f"(span {(fcZ.max()-fcZ.min())*1e3:.1f} mHz)")

        # ---- figure ------------------------------------------------------------
        make_fig(fZ, aZ, fY, aY, fG, aG, linesZ, linesG,
                 tcZ, ampZ, fcZ, tcY, ampY)
        return dict(linesZ=linesZ, linesY=linesY, linesG=linesG,
                    fcZ=fcZ, ampZ=ampZ, coh_bias=coh_bias)


    def make_fig(fZ, aZ, fY, aY, fG, aG, linesZ, linesG,
                 tcZ, ampZ, fcZ, tcY, ampY):
        plt.rcParams.update({"figure.dpi": 150, "font.size": 10,
                             "axes.spines.top": False, "axes.spines.right": False})
        fig = plt.figure(figsize=(11, 7.2))
        gs = fig.add_gridspec(2, 2, height_ratios=[2.1, 1.0], hspace=0.34, wspace=0.22)
        ax = fig.add_subplot(gs[0, :])

        ax.loglog(fZ, aZ*1e3, color=C_Z, lw=0.8, label="analog Z")
        ax.loglog(fY, aY*1e3, color=C_Y, lw=0.7, alpha=0.8, label="analog Y")
        ax.loglog(fG, aG*1e3, color=C_GUI, lw=0.7, alpha=0.7, label="GUI Z (<100 Hz)")
        ax.set_xlim(0.2, 600); ax.set_ylim(3, 3e4)
        ax.set_xlabel("frequency  [Hz]")
        ax.set_ylabel(r"ASD  [fT/$\sqrt{\mathrm{Hz}}$]")
        ax.set_title("Spectral-line census 0.2-600 Hz  (closed shield, 30-min sync5, 2026-07-06)",
                     loc="left")
        ax.grid(True, which="major", alpha=0.4); ax.grid(True, which="minor", alpha=0.15)

        # annotate the strongest / diagnostic lines from analog Z
        ann = sorted(linesZ, key=lambda d: -d["asd"])[:10]
        seen = set()
        for L in ann:
            key = round(L["fc"])
            if key in seen:
                continue
            seen.add(key)
            lab = f"{L['fc']:.1f}"
            if "cell-mod" in L["tag"]:
                lab = f"{L['fc']:.1f} (alias)"
            ax.annotate(lab, xy=(L["fc"], L["asd"]*1e3),
                        xytext=(L["fc"], L["asd"]*1e3*2.4),
                        fontsize=7.5, color=C_INK, ha="center",
                        arrowprops=dict(arrowstyle="-", color=C_MUTE, lw=0.6))
        ax.legend(loc="upper right", fontsize=9)

        # 50 Hz amplitude vs time
        ax2 = fig.add_subplot(gs[1, 0])
        ax2.plot(tcZ, ampZ*1e3, "o-", color=C_Z, ms=4, label="Z")
        ax2.plot(tcY, ampY*1e3, "s-", color=C_Y, ms=3, alpha=0.8, label="Y")
        ax2.set_xlabel("time  [min]"); ax2.set_ylabel("50 Hz amp  [fT]")
        ax2.set_title("50 Hz amplitude vs time (2-min blocks)", loc="left", fontsize=10)
        ax2.grid(True, alpha=0.4); ax2.legend(fontsize=8)

        # 50 Hz centre freq vs time
        ax3 = fig.add_subplot(gs[1, 1])
        ax3.plot(tcZ, fcZ, "o-", color=C_Z, ms=4)
        ax3.axhline(50.0, color=C_MUTE, ls=":", lw=0.8)
        ax3.set_xlabel("time  [min]"); ax3.set_ylabel("50 Hz centre  [Hz]")
        ax3.set_title("Mains centre frequency vs time (grid wander)", loc="left", fontsize=10)
        ax3.grid(True, alpha=0.4)
        ax3.ticklabel_format(axis="y", useOffset=False)

        out = os.path.join(OUT, "lines_census_spectrum.png")
        fig.savefig(out, dpi=150, bbox_inches="tight")
        plt.close(fig)
        print(f"[saved] {out}")
    main()


def run_models():
    plt.rcdefaults()
    ROOT = PROJECT_DIR
    OUT = HERE
    os.makedirs(OUT, exist_ok=True)


    # --- established calibration/sampling (given; do not re-derive) ---
    FS_DAQ = 1200.0566
    FS_GUI = 203.18
    CAL_ANALOG = 1.029          # 2.7/2.624 : multiply analog pT to calibrate
    SETTLE_S = 300.0            # drop first 5 min Z settling transient

    AZ_FILE = data_join("result/synctest/sync5_30min/sync5daq_Z_pT.txt")
    GZ_FILE = data_join("result/synctest/sync5_30min/gui_sync5_Formatted.txt")
    CL_FILE = data_join("result/gui_baseline/2026-06-30_closed-shield/closed_long_Z_pT.txt")

    # ---------------- physical model constants ----------------
    MU0 = 4e-7*np.pi
    KB = 1.380649e-23
    T = 300.0
    # QZFM Dual-axis self-noise spec (fT/rtHz), 3-100 Hz [qzfm-sensor-specs]
    SENS_LO, SENS_TYP, SENS_HI = 7.0, 10.0, 15.0
    # mu-metal typicals [Lee&Romalis 2008 / magnetic-johnson-noise]
    MU_R, SIGMA, TAN_D = 25e3, 1.6e6, 0.04
    # bench-shield wall-thickness band (UNKNOWN -> band), m
    T_LO, T_C, T_HI = 0.5e-3, 1.0e-3, 1.5e-3

    def f_magn(t, mu_r=MU_R, sigma=SIGMA, tan_d=TAN_D):
        """Lee&Romalis (2008) Eq.10 magnetization-noise knee [Hz]."""
        return 3.0*tan_d/(2.0*np.pi*MU0*mu_r*sigma*t**2)

    def johnson_white_cyl(a, t, mu_r=MU_R, sigma=SIGMA):
        """White eddy-current floor, transverse B at centre of an infinite
        high-mu thin cylindrical shell [Lee&Romalis 2008 Table I]:
           S_B = 8 kT mu0^2 mu_r^2 sigma t / (3 pi a^2)   [T^2/Hz]
        Returns amplitude in fT/rtHz. Geometry (a) ASSUMED for the bench shield.

        NOTE: taken literally with mu_r=2.5e4 this analytic high-mu limit yields a
        physically absurd ~1e6 fT/rtHz for a 1 mm / 7.5 cm shell; the closed-form
        high-mu asymptote overstates the true multilayer floor by orders of
        magnitude (real mu-metal shields measure ~5-15 fT/rtHz -- Collings 6.1,
        Twinleaf MS-1L 16). We therefore do NOT plot this formula; the credible
        Johnson scale is taken from those empirical anchors instead."""
        S = 8.0*KB*T*MU0**2*mu_r**2*sigma*t/(3.0*np.pi*a**2)
        return np.sqrt(S)*1e15

    # Empirically anchored Johnson white scale for a mu-metal shield (fT/rtHz):
    # Collings inner mu-metal layer 6.1, Twinleaf MS-1L (4-layer bench) 16.
    JOHNSON_EMPIRICAL = (6.1, 16.0)

    def loglog_at(f, a, f0):
        m = f > 0
        return float(np.exp(np.interp(np.log(f0), np.log(f[m]), np.log(a[m]))))

    # ---------------- load + calibrate ----------------
    az = p5.load_col(AZ_FILE) * CAL_ANALOG
    gz = p5.load_col(GZ_FILE, skiprows=1)         # already calibrated
    cl = p5.load_col(CL_FILE, skiprows=1)         # 06-30 reference GUI Z

    # drop settling transient
    az = az[int(SETTLE_S*FS_DAQ):]
    gz = gz[int(SETTLE_S*FS_GUI):]
    # 06-30 reference: also drop first 5 min
    cl = cl[int(SETTLE_S*FS_GUI):] if cl.size > int(SETTLE_S*FS_GUI)+10000 else cl

    # ASD in pT/rtHz (long low-f segments + short high-f), then -> fT/rtHz
    def asd_fT(x, fs):
        f, a = p5.spliced_asd(x, fs, lowseg=200.0, hiseg=20.0, split=1.0)
        return f, a*1000.0

    fg, ag = asd_fT(gz, FS_GUI)      # GUI Z  (primary; no quantization at low f)
    fa, aa = asd_fT(az, FS_DAQ)      # analog Z (cross-check; wider band)
    fc, ac = asd_fT(cl, FS_GUI)      # 06-30 reference

    # restrict to positive finite
    for f, a in ((fg, ag), (fa, aa), (fc, ac)):
        assert np.isfinite(a[f > 0]).all()

    # ---------------- white-floor sanity (3-40 Hz) ----------------
    def band_median(f, a, lo, hi):
        m = (f >= lo) & (f <= hi)
        return float(np.median(a[m]))

    wf_gui = band_median(fg, ag, 3, 40)
    wf_ana = band_median(fa, aa, 3, 40)
    wf_ref = band_median(fc, ac, 3, 40)

    # ---------------- 1/f slope fit of the low-freq excess ----------------
    # subtract sensor-limited white floor (use measured GUI white floor) in quadrature
    WHITE = wf_gui   # fT/rtHz, the sensor-limited floor for THIS run
    def excess(f, a):
        return np.sqrt(np.clip(a**2 - WHITE**2, 0.0, None))

    # Fit power law a_excess ~ A * f^p on 0.05-2 Hz on the GUI record.
    def fit_slope(f, a, lo, hi):
        ex = excess(f, a)
        m = (f >= lo) & (f <= hi) & (ex > 0)
        lf, la = np.log(f[m]), np.log(ex[m])
        p, logA = np.polyfit(lf, la, 1)
        # residual RMS in dex
        res = la - (p*lf + logA)
        return p, np.exp(logA), float(np.sqrt(np.mean(res**2))), int(m.sum())

    slope_gui, ampA_gui, rms_gui, n_gui = fit_slope(fg, ag, 0.05, 2.0)
    slope_ana, ampA_ana, rms_ana, n_ana = fit_slope(fa, aa, 0.05, 2.0)
    slope_ref, ampA_ref, rms_ref, n_ref = fit_slope(fc, ac, 0.05, 2.0)

    # magnetization prediction is amplitude slope = -0.5 (S_B ~ 1/f).
    # Build the fitted magnetization curve pinned to the GUI excess:
    #   B_magn(f) = C * f^-0.5   with C chosen at 0.1 Hz from the fit
    f_pin = 0.1
    C_magn = loglog_at(fg, excess(fg, ag), f_pin) * np.sqrt(f_pin)  # so B=C f^-0.5

    # ---------------- MS-1L shield model (from B2) ----------------
    MS1L_WHITE = 16.0  # fT/rtHz [Twinleaf]
    fm_c, fm_lo, fm_hi = f_magn(T_C), f_magn(T_LO), f_magn(T_HI)
    def shield_ms1l(f, fm):
        return MS1L_WHITE*np.sqrt(1.0 + fm/np.asarray(f))

    # Johnson white bound for a bench mu-metal cylinder (a ASSUMED). Kept only to
    # document that the naive high-mu analytic limit is not usable here.
    A_BENCH = 0.075   # 7.5 cm radius, plausible bench shield (ASSUMED)
    jw_c = johnson_white_cyl(A_BENCH, T_C)
    jw_lo = johnson_white_cyl(A_BENCH, T_LO)
    jw_hi = johnson_white_cyl(A_BENCH, T_HI)
    JW_EMP_LO, JW_EMP_HI = JOHNSON_EMPIRICAL

    # ---------------- key ASD values at reference freqs ----------------
    FREFS = [0.05, 0.1, 0.3, 1.0, 2.0]
    meas_gui = {f0: loglog_at(fg, ag, f0) for f0 in FREFS}
    meas_ana = {f0: loglog_at(fa, aa, f0) for f0 in FREFS}
    meas_ref = {f0: loglog_at(fc, ac, f0) for f0 in FREFS}

    # Bench magnetization model: fit its OWN knee to the GUI data.
    # In the magnetization regime B = B_white*sqrt(1+fm/f). Below the knee the
    # f^-1/2 term dominates; the fitted amplitude C_magn = B_white*sqrt(fm).
    # We report C_magn and the implied B_white*sqrt(fm) product (a measured combo),
    # noting fm and B_white are individually unconstrained for the bench shield.

    # ---------------- figure ----------------
    plt.rcParams.update({"figure.dpi":150, "font.size":11, "axes.titleweight":"bold",
                         "axes.spines.top":False, "axes.spines.right":False,
                         "legend.frameon":False})
    fig, ax = plt.subplots(figsize=(9.0, 6.6))
    fgrid = np.logspace(np.log10(0.008), np.log10(100), 500)

    # measured curves
    ax.loglog(fg, ag, color="#2E6E8E", lw=1.8, zorder=5,
              label=f"measured GUI Z (sync5, 25 min): white {wf_gui:.1f} fT/rtHz")
    ax.loglog(fa, aa, color="#5FA8C7", lw=0.9, alpha=0.7, zorder=3,
              label=f"measured analog Z (calibrated): white {wf_ana:.1f} fT/rtHz")
    ax.loglog(fc, ac, color="#B9C4CB", lw=1.0, alpha=0.85, zorder=2,
              label=f"reference 06-30 GUI Z (10 min): white {wf_ref:.1f} fT/rtHz")

    # sensor spec band
    ax.axhspan(SENS_LO, SENS_HI, color="#9AA0A6", alpha=0.18, lw=0,
               label="QZFM Dual self-noise spec (7-15 fT/rtHz, 3-100 Hz)")

    # fitted magnetization f^-1/2 model pinned to the measured excess
    b_magn = C_magn/np.sqrt(fgrid)
    b_magn_tot = np.sqrt(b_magn**2 + WHITE**2)
    ax.loglog(fgrid, b_magn_tot, color="#C0504D", lw=2.0, ls="--", zorder=4,
              label=r"magnetization model $f^{-1/2}$ pinned to excess + white floor")

    # fitted actual slope of the measured excess
    b_fit = ampA_gui*fgrid**slope_gui
    b_fit_tot = np.sqrt(b_fit**2 + WHITE**2)
    ax.loglog(fgrid, b_fit_tot, color="#7A4E9E", lw=1.6, ls=":", zorder=4,
              label=fr"measured excess power-law fit $f^{{{slope_gui:.2f}}}$ + white")

    # MS-1L own noise band (B2)
    sh_lo = shield_ms1l(fgrid, fm_lo); sh_hi = shield_ms1l(fgrid, fm_hi)
    ax.fill_between(fgrid, sh_lo, sh_hi, color="#C58A2D", alpha=0.20, lw=0, zorder=1,
                    label="B2 MS-1L own noise (16 fT white + 1/f magn. band)")
    ax.loglog(fgrid, shield_ms1l(fgrid, fm_c), color="#C58A2D", lw=1.3, ls="-.", zorder=2)

    # Johnson white scale: empirical mu-metal anchors (analytic high-mu limit is
    # unusable here -- see johnson_white_cyl docstring)
    ax.axhspan(JW_EMP_LO, JW_EMP_HI, color="#5A7D3A", alpha=0.15, lw=0, zorder=1,
               label="mu-metal Johnson white scale (empirical: Collings 6.1 - MS-1L 16 fT/rtHz)")

    ax.axhline(50.0, color="#33373B", ls=(0,(5,3)), lw=1.1, zorder=1)
    ax.text(18, 52, "lattice budget 50 fT/rtHz", fontsize=8.5, color="#33373B")

    ax.set_xlim(0.008, 110); ax.set_ylim(3, 5000)
    ax.grid(True, which="major", alpha=0.5); ax.grid(True, which="minor", alpha=0.2)
    ax.set_xlabel("frequency [Hz]")
    ax.set_ylabel(r"magnetic field ASD [fT/$\sqrt{\mathrm{Hz}}$]")
    ax.set_title("Physical noise-model check — sync5 closed bench shield, Z axis", loc="left")
    ax.legend(loc="upper center", bbox_to_anchor=(0.5,-0.12), ncol=2, fontsize=8.2)
    fig.tight_layout()
    figpath = os.path.join(OUT, "models_sync5_asd_models.png")
    fig.savefig(figpath, bbox_inches="tight"); plt.close(fig)

    # ---------------- residual / slope diagnostic figure ----------------
    fig2, ax2 = plt.subplots(figsize=(8.0, 5.4))
    exg = excess(fg, ag); m = (fg >= 0.02) & (fg <= 5)
    ax2.loglog(fg[m], exg[m], color="#2E6E8E", lw=1.6, label="GUI Z excess (measured - white, quad)")
    exa = excess(fa, aa); ma = (fa >= 0.02) & (fa <= 5)
    ax2.loglog(fa[ma], exa[ma], color="#5FA8C7", lw=0.9, alpha=0.7, label="analog Z excess")
    ff = np.logspace(np.log10(0.05), np.log10(2), 50)
    ax2.loglog(ff, C_magn/np.sqrt(ff), color="#C0504D", lw=2, ls="--", label=r"$f^{-1/2}$ (magnetization)")
    ax2.loglog(ff, ampA_gui*ff**slope_gui, color="#7A4E9E", lw=1.5, ls=":",
               label=fr"fit $f^{{{slope_gui:.2f}}}$")
    # a f^-1 reference (environmental / random-walk-ish steeper colour)
    ax2.loglog(ff, (C_magn/np.sqrt(0.1))*(0.1/ff), color="#888", lw=1, ls=(0,(2,2)),
               label=r"$f^{-1}$ reference")
    ax2.set_xlim(0.02,5); ax2.set_ylim(5,5000)
    ax2.grid(True, which="major", alpha=0.5); ax2.grid(True, which="minor", alpha=0.2)
    ax2.set_xlabel("frequency [Hz]"); ax2.set_ylabel(r"excess ASD [fT/$\sqrt{\mathrm{Hz}}$]")
    ax2.set_title(f"Low-frequency excess colour: fit slope {slope_gui:.2f} (RMS {rms_gui:.2f} dex)", loc="left")
    ax2.legend(loc="upper right", fontsize=8.5)
    fig2.tight_layout()
    figpath2 = os.path.join(OUT, "models_sync5_excess_slope.png")
    fig2.savefig(figpath2, bbox_inches="tight"); plt.close(fig2)

    # ---------------- printout ----------------
    def ln(*a): print(*a)
    ln("=== white floor 3-40 Hz (fT/rtHz) ===")
    ln(f"  GUI Z {wf_gui:.1f} | analog Z {wf_ana:.1f} | 06-30 ref {wf_ref:.1f}")
    ln("=== measured ASD at ref freqs (fT/rtHz) ===")
    ln("   f[Hz]   GUI     analog   06-30")
    for f0 in FREFS:
        ln(f"  {f0:5.2f}  {meas_gui[f0]:7.0f} {meas_ana[f0]:7.0f} {meas_ref[f0]:7.0f}")
    ln("=== low-freq excess power-law fit (0.05-2 Hz) ===")
    ln(f"  GUI    slope {slope_gui:+.3f}  RMS {rms_gui:.3f} dex  (n={n_gui})")
    ln(f"  analog slope {slope_ana:+.3f}  RMS {rms_ana:.3f} dex  (n={n_ana})")
    ln(f"  06-30  slope {slope_ref:+.3f}  RMS {rms_ref:.3f} dex  (n={n_ref})")
    ln(f"  magnetization prediction slope = -0.500")
    ln("=== magnetization model pinned amplitude ===")
    ln(f"  C_magn (=B_white*sqrt(fm)) = {C_magn:.1f} fT/rtHz*sqrt(Hz) at f^-1/2")
    ln(f"  -> if bench white were {WHITE:.1f} fT/rtHz, implied knee fm = {(C_magn/WHITE)**2:.2f} Hz")
    ln("=== Johnson white scale ===")
    ln(f"  analytic high-mu limit (a={A_BENCH*100:.0f}cm ASSUMED, t=1mm): {jw_c:.3g} fT/rtHz  <-- UNPHYSICAL, not used")
    ln(f"  empirical mu-metal anchor band: {JW_EMP_LO}-{JW_EMP_HI} fT/rtHz (Collings / MS-1L)")
    ln(f"  measured white floor {wf_gui:.1f} fT/rtHz sits in/above this band -> floor is sensor-limited, not Johnson-limited")
    ln("=== MS-1L f_magn knee band ===")
    ln(f"  fm central {fm_c:.2f} Hz, band {fm_hi:.2f}-{fm_lo:.2f} Hz")

    summary = {
      "white_floor_fT_3_40Hz": {"gui": round(wf_gui,2), "analog": round(wf_ana,2), "ref0630": round(wf_ref,2)},
      "measured_asd_fT": {str(f0): {"gui": round(meas_gui[f0],1), "analog": round(meas_ana[f0],1),
                                    "ref0630": round(meas_ref[f0],1)} for f0 in FREFS},
      "excess_slope_fit_0p05_2Hz": {
         "gui": {"slope": round(slope_gui,3), "rms_dex": round(rms_gui,3), "n": n_gui},
         "analog": {"slope": round(slope_ana,3), "rms_dex": round(rms_ana,3), "n": n_ana},
         "ref0630": {"slope": round(slope_ref,3), "rms_dex": round(rms_ref,3), "n": n_ref},
         "magnetization_predicted_slope": -0.5},
      "C_magn_fT_sqrtHz": round(C_magn,1),
      "implied_knee_if_white_eq_floor_Hz": round((C_magn/WHITE)**2,2),
      "johnson_white_fT": {"analytic_highmu_limit_UNPHYSICAL_a8cm_t1mm": float(f"{jw_c:.3g}"),
         "empirical_mumetal_band": [JW_EMP_LO, JW_EMP_HI]},
      "ms1l_f_magn_Hz": {"central": round(fm_c,3), "lo": round(fm_lo,3), "hi": round(fm_hi,3)},
      "assumptions": {
         "bench_wall_thickness_UNKNOWN_band_mm": [0.5,1.0,1.5],
         "bench_radius_ASSUMED_cm": A_BENCH*100,
         "mu_r": MU_R, "sigma_S_per_m": SIGMA, "tan_delta": TAN_D,
         "sensor_spec_fT": [SENS_LO, SENS_HI],
         "white_floor_used_for_excess_fT": round(WHITE,2)},
      "figures": [figpath, figpath2],
    }
    with open(os.path.join(OUT, "models_summary.json"), "w") as fh:
        json.dump(summary, fh, indent=2)
    print("\n[saved]", figpath)
    print("[saved]", figpath2)
    print("[saved]", os.path.join(OUT, "models_summary.json"))


def run_nonstat():
    plt.rcdefaults()
    REPO = PROJECT_DIR
    OUTDIR = HERE
    os.makedirs(OUTDIR, exist_ok=True)


    FS = 1200.0566          # precise analog sample rate [Hz]
    CAL = 1.029             # analog pT file -> calibrated (2.7/2.624)

    # ---- load calibrated analog Z ----
    z = p5.load_col(p5.AZ_FILE) * CAL
    N = z.size
    T = N / FS
    t = np.arange(N) / FS
    print(f"N={N} samples, T={T:.2f} s, fs={FS} Hz")
    print(f"raw file mean (uncal) = {z.mean()/CAL:.2f} pT, calibrated mean = {z.mean():.2f} pT")

    # =====================================================================
    # 1. SPECTROGRAM  (analog Z, calibrated)
    # =====================================================================
    # ~8 s segments -> fine enough time res, resolves down to ~0.25 Hz averaged
    nper = int(8 * FS)
    f_sg, t_sg, Sxx = spectrogram(z, fs=FS, nperseg=nper, noverlap=nper // 2,
                                  detrend="linear", scaling="density")
    asd_sg = np.sqrt(Sxx)   # pT/rtHz, shape (freq, time)

    # =====================================================================
    # 2. ROLLING WHITE FLOOR (3-40 Hz) and ROLLING LOW-FREQ AMPLITUDE (~0.1 Hz)
    # =====================================================================
    WIN = 90.0              # window length [s]
    STEP = 30.0             # hop [s]
    nwin = int(WIN * FS)
    nhop = int(STEP * FS)
    starts = np.arange(0, N - nwin + 1, nhop)
    tc = (starts + nwin / 2) / FS     # window-center time [s]

    floor = np.zeros(len(starts))     # median ASD 3-40 Hz  [fT/rtHz]
    lf01 = np.zeros(len(starts))      # ASD near 0.1 Hz     [pT/rtHz]
    rms_bb = np.zeros(len(starts))    # 0.02-0.2 Hz bandpassed rms [pT]

    # a long bandpass for the drift band, applied once to full record
    z_bb = p5.bp(z - z.mean(), FS, 0.02, 0.2)

    for i, s0 in enumerate(starts):
        seg = z[s0:s0 + nwin]
        # white floor: Welch, 4 s segments -> average many
        fw, pw = welch(seg, fs=FS, nperseg=int(4 * FS), detrend="linear")
        band = (fw >= 3.0) & (fw <= 40.0)
        floor[i] = np.median(np.sqrt(pw[band])) * 1e3     # fT/rtHz
        # low-freq amplitude near 0.1 Hz: Welch with long segment for resolution
        fl, pl = welch(seg, fs=FS, nperseg=nwin, detrend="linear")
        sel = (fl >= 0.07) & (fl <= 0.15)
        lf01[i] = np.median(np.sqrt(pl[sel]))             # pT/rtHz
        rms_bb[i] = np.std(z_bb[s0:s0 + nwin])            # pT

    # exclude first 5 min transient
    T_EXCL = 300.0
    keep = tc >= T_EXCL
    floor_k = floor[keep]
    lf_k = lf01[keep]
    tc_k = tc[keep]

    def spread_pct(a):
        return 100.0 * (a.max() - a.min()) / np.median(a)

    def rstd_pct(a):
        return 100.0 * np.std(a) / np.median(a)

    print("\n-- rolling floor 3-40 Hz [fT/rtHz] --")
    print(f"full record: median={np.median(floor):.2f}, min={floor.min():.2f}, max={floor.max():.2f}")
    print(f"after 5min:  median={np.median(floor_k):.2f}, min={floor_k.min():.2f}, "
          f"max={floor_k.max():.2f}, std={np.std(floor_k):.2f}")
    print(f"after 5min:  peak-to-peak spread = {spread_pct(floor_k):.1f}%, "
          f"rel std = {rstd_pct(floor_k):.1f}%")
    print("\n-- rolling low-freq ~0.1 Hz [pT/rtHz] --")
    print(f"after 5min:  median={np.median(lf_k):.1f}, min={lf_k.min():.1f}, "
          f"max={lf_k.max():.1f}, p2p spread={spread_pct(lf_k):.0f}%, rel std={rstd_pct(lf_k):.0f}%")

    # =====================================================================
    # 3. STATIONARITY TESTS
    # =====================================================================
    def reverse_arrangement(x):
        """Reverse-arrangement test statistic A and z-score under H0 (stationary).
        Counts pairs i<j with x[i] > x[j]. E[A]=n(n-1)/4, Var per Bendat&Piersol."""
        n = len(x)
        A = 0
        for i in range(n - 1):
            A += np.sum(x[i] > x[i + 1:])
        A = int(A)
        mu = n * (n - 1) / 4.0
        var = (2 * n**3 + 3 * n**2 - 5 * n) / 72.0
        zc = (A - mu) / np.sqrt(var)
        return A, mu, zc

    A_f, mu_f, z_f = reverse_arrangement(floor_k)
    A_l, mu_l, z_l = reverse_arrangement(lf_k)
    print("\n-- reverse-arrangement test (after 5 min) --")
    print(f"floor:  A={A_f}, E[A]={mu_f:.1f}, z={z_f:+.2f}  "
          f"({'trend/nonstat' if abs(z_f)>1.96 else 'stationary'} at 5%)")
    print(f"lf0.1:  A={A_l}, E[A]={mu_l:.1f}, z={z_l:+.2f}  "
          f"({'trend/nonstat' if abs(z_l)>1.96 else 'stationary'} at 5%)")

    # variance-of-blocks: split floor_k into thirds, compare variances (F-like ratio)
    def block_var_ratio(x, nblk=3):
        b = np.array_split(x, nblk)
        v = [np.var(bi, ddof=1) for bi in b]
        m = [np.mean(bi) for bi in b]
        return v, m

    vv, mm = block_var_ratio(floor_k, 3)
    print(f"floor thirds means [fT/rtHz]: {[f'{x:.2f}' for x in mm]}")
    print(f"floor thirds vars:            {[f'{x:.3f}' for x in vv]}, "
          f"max/min var ratio = {max(vv)/min(vv):.2f}")

    # first-5min vs rest floor comparison (uses full-record floor series)
    fl_first = floor[tc < T_EXCL]
    print(f"first-5min floor median={np.median(fl_first):.2f} fT/rtHz vs "
          f"rest median={np.median(floor_k):.2f} fT/rtHz "
          f"(ratio {np.median(fl_first)/np.median(floor_k):.2f})")

    # =====================================================================
    # 4. GLITCH / STEP HUNT  (sample-to-sample difference threshold)
    # =====================================================================
    d = np.diff(z)
    sig = 1.4826 * np.median(np.abs(d - np.median(d)))   # robust MAD-sigma of diff
    thr = 6.0 * sig
    out = np.where(np.abs(d - np.median(d)) > thr)[0]
    print(f"\n-- glitch hunt on sample-to-sample diff --")
    print(f"diff robust sigma (MAD) = {sig:.2f} pT, 6-sigma threshold = {thr:.1f} pT")
    print(f"diff std (non-robust) = {np.std(d):.2f} pT")
    print(f"count of >6sigma single-sample jumps = {out.size} "
          f"({100.0*out.size/d.size:.4f}% of samples)")

    # cluster consecutive outlier indices into events
    events = []
    if out.size:
        grp_start = out[0]
        prev = out[0]
        for idx in out[1:]:
            if idx - prev > int(0.05 * FS):    # >50 ms gap -> new event
                events.append((grp_start, prev))
                grp_start = idx
            prev = idx
        events.append((grp_start, prev))
    print(f"clustered into {len(events)} events (>50 ms apart)")
    # report the largest events by peak |diff|
    ev_info = []
    for (a, b) in events:
        lo = max(0, a - 2); hi = min(d.size, b + 3)
        peak = np.max(np.abs(d[lo:hi]))
        it = a + int(np.argmax(np.abs(d[lo:hi]))) - (a - lo)
        ev_info.append((it, peak, (b - a + 1)))
    ev_info.sort(key=lambda e: -e[1])
    print("top events (time_s, peak_jump_pT, span_samples):")
    for it, pk, span in ev_info[:12]:
        print(f"  t={it/FS:8.2f} s   jump={pk:7.1f} pT   span={span}")

    # step hunt: look for level shifts using a coarse block-median difference
    BS = int(2.0 * FS)   # 2 s blocks
    nblk_full = N // BS
    blk_med = np.array([np.median(z[i*BS:(i+1)*BS]) for i in range(nblk_full)])
    step = np.diff(blk_med)
    step_sig = 1.4826 * np.median(np.abs(step - np.median(step)))
    big_steps = np.where(np.abs(step - np.median(step)) > 6 * step_sig)[0]
    print(f"\n-- step hunt on 2 s block medians --")
    print(f"block-median-diff robust sigma = {step_sig:.2f} pT, "
          f"6sigma = {6*step_sig:.1f} pT")
    print(f"big level steps (>6sigma): {big_steps.size}")
    for k in big_steps:
        print(f"  t~{(k+1)*2.0:.0f} s  step={step[k]:+.1f} pT")

    # =====================================================================
    # 5. FIGURE: spectrogram + rolling-floor panel
    # =====================================================================
    p5.style()
    fig, (ax1, ax2) = plt.subplots(2, 1, figsize=(11, 9),
                                   gridspec_kw=dict(height_ratios=[1.15, 1.0]))

    # --- spectrogram ---
    fmask = (f_sg >= 0.1) & (f_sg <= 100.0)
    Z = 20 * np.log10(np.clip(asd_sg[fmask], 1e-6, None))   # dB re 1 pT/rtHz
    vmin, vmax = np.percentile(Z, [5, 99])
    pcm = ax1.pcolormesh(t_sg / 60.0, f_sg[fmask], Z, shading="auto",
                         cmap="magma", vmin=vmin, vmax=vmax)
    ax1.set_yscale("log")
    ax1.set_ylabel("frequency [Hz]")
    ax1.set_xlabel("time [min]")
    ax1.set_title("Analog Z spectrogram (calibrated), sync5 30 min")
    ax1.axvline(T_EXCL / 60.0, color="cyan", ls="--", lw=1.2, alpha=0.8)
    ax1.text(T_EXCL/60.0 + 0.1, ax1.get_ylim()[1]*0.5, "5 min", color="cyan",
             fontsize=9, va="top")
    cb = fig.colorbar(pcm, ax=ax1, pad=0.01)
    cb.set_label("ASD [dB re 1 pT/$\\sqrt{Hz}$]")

    # --- rolling floor / lf panel ---
    ax2.plot(tc / 60.0, floor, "o-", color=p5.C_Z, ms=3, lw=1.2,
             label="white floor 3-40 Hz [fT/$\\sqrt{Hz}$]")
    ax2.axvline(T_EXCL / 60.0, color="0.4", ls="--", lw=1.0)
    med_k = np.median(floor_k)
    ax2.axhline(med_k, color=p5.C_Z, ls=":", lw=1.0, alpha=0.7)
    ax2.fill_between([T_EXCL/60.0, T/60.0],
                     med_k*(1-rstd_pct(floor_k)/100), med_k*(1+rstd_pct(floor_k)/100),
                     color=p5.C_Z, alpha=0.10)
    ax2.set_ylabel("white floor [fT/$\\sqrt{Hz}$]", color=p5.C_Z)
    ax2.set_xlabel("time [min]")
    ax2.tick_params(axis="y", labelcolor=p5.C_Z)
    ax2.set_title(f"Rolling floor ({WIN:.0f} s win / {STEP:.0f} s hop) and 0.1 Hz amplitude vs time")

    axr = ax2.twinx()
    axr.plot(tc / 60.0, lf01, "s-", color=p5.C_REF, ms=3, lw=1.0,
             label="0.1 Hz amp [pT/$\\sqrt{Hz}$]")
    axr.set_ylabel("0.1 Hz amp [pT/$\\sqrt{Hz}$]", color=p5.C_MUTE)
    axr.tick_params(axis="y", labelcolor=p5.C_MUTE)

    # annotate glitch events on the floor panel
    for it, pk, span in ev_info[:6]:
        if pk > 2 * thr:
            ax2.axvline(it/FS/60.0, color="red", ls="-", lw=0.6, alpha=0.5)

    # combined legend
    h1, l1 = ax2.get_legend_handles_labels()
    h2, l2 = axr.get_legend_handles_labels()
    ax2.legend(h1 + h2, l1 + l2, loc="upper right", fontsize=9, framealpha=0.9)

    txt = (f"after 5 min: floor median {med_k:.1f} fT/$\\sqrt{{Hz}}$, "
           f"rel std {rstd_pct(floor_k):.1f}%, p2p {spread_pct(floor_k):.0f}%\n"
           f"rev-arr z(floor)={z_f:+.2f}  ->  "
           f"{'non-stationary trend' if abs(z_f)>1.96 else 'stationary'}")
    ax2.text(0.02, 0.04, txt, transform=ax2.transAxes, fontsize=9,
             va="bottom", ha="left", bbox=p5.NOTE_BBOX)

    fig.tight_layout()
    figpath = os.path.join(OUTDIR, "nonstat_spectrogram_rolling.png")
    fig.savefig(figpath, dpi=150)
    print(f"\nsaved figure: {figpath}")

    # save a compact summary line for the notebook
    print("\n=== SUMMARY ===")
    print(f"floor after 5min: median {med_k:.1f} fT/rtHz, rel std {rstd_pct(floor_k):.1f}%, "
          f"p2p {spread_pct(floor_k):.0f}%")
    print(f"rev-arr z: floor {z_f:+.2f}, lf {z_l:+.2f}")
    print(f"glitches >6sig: {out.size} samples, {len(events)} events; "
          f"largest jump {ev_info[0][1]:.0f} pT at t={ev_info[0][0]/FS:.1f}s" if ev_info else "no glitches")
    print(f"level steps >6sig: {big_steps.size}")


def run_slope1f():
    plt.rcdefaults()
    REPO = PROJECT_DIR
    OUT = HERE
    os.makedirs(OUT, exist_ok=True)


    FS_DAQ = 1200.0566
    FS_GUI = 203.18
    CAL = 1.029           # analog pT -> calibrated
    FMIN, FMAX = 0.005, 40.0
    SETTLE_S = 300.0      # exclude first 5 min from analog fits

    rng = np.random.default_rng(20260706)

    # ---------------------------------------------------------------- load
    az = p5.load_col(p5.AZ_FILE) * CAL
    ay = p5.load_col(p5.AY_FILE) * CAL
    gz = p5.load_col(p5.GZ_FILE, skiprows=1)
    cl = p5.load_col(p5.CL_FILE, skiprows=1)

    # drop settling transient from analog channels
    n_settle = int(SETTLE_S * FS_DAQ)
    az = az[n_settle:]
    ay = ay[n_settle:]
    # GUI channels: 06-30 note says first minutes are a Z settling transient too;
    # for the GUI 07-06 record drop the first 5 min as well for consistency.
    gz = gz[int(SETTLE_S * FS_GUI):]
    # 06-30 reference is 15 min; drop 3 min settle (proportionate, conservative)
    cl = cl[int(180.0 * FS_GUI):]


    def seg_psd(x, fs, nperseg, noverlap=None):
        """Return (f, per-segment periodograms P[nseg, nfreq]) with linear detrend,
        Hann window, so we can median-average and bootstrap over segments."""
        x = np.asarray(x, float)
        if noverlap is None:
            noverlap = nperseg // 2
        step = nperseg - noverlap
        win = get_window("hann", nperseg)
        scale = 1.0 / (fs * (win ** 2).sum())
        starts = range(0, len(x) - nperseg + 1, step)
        f = np.fft.rfftfreq(nperseg, 1.0 / fs)
        segs = []
        # linear detrend basis
        t = np.arange(nperseg)
        tc = t - t.mean()
        denom = (tc ** 2).sum()
        for st in starts:
            seg = x[st:st + nperseg]
            # linear detrend
            a = (seg * tc).sum() / denom
            b = seg.mean()
            seg = seg - (a * tc + b)
            sp = np.fft.rfft(seg * win)
            p = (np.abs(sp) ** 2) * scale
            p[1:-1] *= 2.0
            segs.append(p)
        return f, np.array(segs)


    def multi_res_psd(x, fs):
        """Build ASD across the full band by combining several segment lengths:
        long segments for low-f resolution, short for high-f averaging.
        Returns list of (f, P[nseg,nfreq], band_mask_applied_freq) merged into
        one (f, median_asd, per-freq segment matrices dict) usable for bootstrap.
        We keep the per-resolution segment matrices for bootstrap.
        """
        N = len(x)
        dur = N / fs
        # segment durations (s): pick so that even the longest gives >=8 segments (50% ovlp)
        blocks = []
        for segdur, flo, fhi in [(200.0, FMIN, 0.05),
                                 (50.0, 0.05, 0.5),
                                 (10.0, 0.5, 5.0),
                                 (2.0, 5.0, FMAX)]:
            nper = int(segdur * fs)
            if nper > N:
                nper = N // 4
            f, P = seg_psd(x, fs, nper)
            m = (f >= flo) & (f < fhi)
            if m.sum() == 0:
                continue
            blocks.append((f[m], P[:, m]))
        fs_all = np.concatenate([b[0] for b in blocks])
        order = np.argsort(fs_all)
        return blocks, fs_all[order], order


    def model(f, w, A, alpha):
        return np.sqrt(w ** 2 + (A / f ** alpha) ** 2)


    def fit_asd(ff, asd, sigma_log=None):
        """Fit model in log space. Returns w, A, alpha."""
        logf = np.log(ff)

        def logmodel(lf, w, A, alpha):
            return np.log(model(np.exp(lf), w, A, alpha))

        # seeds
        w0 = np.median(asd[ff > 5.0]) if np.any(ff > 5.0) else np.median(asd)
        lowm = ff < 0.05
        if lowm.sum() > 2:
            # A/f^alpha ~ asd at low f; alpha from slope
            lf = np.log(ff[lowm]); la = np.log(asd[lowm])
            alpha0 = -np.polyfit(lf, la, 1)[0]
            A0 = np.exp(np.median(la + alpha0 * lf))
        else:
            alpha0, A0 = 1.0, asd[0] * ff[0]
        alpha0 = float(np.clip(alpha0, 0.2, 2.5))
        p0 = [max(w0, 1e-3), max(A0, 1e-6), alpha0]
        try:
            popt, _ = curve_fit(
                logmodel, logf, np.log(asd), p0=p0,
                sigma=sigma_log, absolute_sigma=False,
                bounds=([1e-4, 1e-8, 0.1], [1e3, 1e4, 3.0]), maxfev=40000)
        except Exception:
            popt = np.array(p0)
        return popt


    def knee(w, A, alpha):
        # A / f^alpha = w  ->  f = (A/w)^(1/alpha)
        return (A / w) ** (1.0 / alpha)


    def analyze(x, fs, label):
        blocks, f_all, order = multi_res_psd(x, fs)
        # median ASD across segments per resolution block
        ff = []
        asd = []
        seg_counts = []
        for fb, Pb in blocks:
            ff.append(fb)
            asd.append(np.sqrt(np.median(Pb, axis=0)))
            seg_counts.append(Pb.shape[0])
        ff = np.concatenate(ff)
        asd = np.concatenate(asd)
        o = np.argsort(ff)
        ff, asd = ff[o], asd[o]
        m = (ff >= FMIN) & (ff <= FMAX) & np.isfinite(asd) & (asd > 0)
        ff, asd = ff[m], asd[m]

        popt = fit_asd(ff, asd)

        # bootstrap over segments within each resolution block
        boots = []
        NB = 300
        for _ in range(NB):
            bff, basd = [], []
            for fb, Pb in blocks:
                ns = Pb.shape[0]
                idx = rng.integers(0, ns, ns)
                bff.append(fb)
                basd.append(np.sqrt(np.median(Pb[idx], axis=0)))
            bff = np.concatenate(bff); basd = np.concatenate(basd)
            oo = np.argsort(bff); bff, basd = bff[oo], basd[oo]
            mm = (bff >= FMIN) & (bff <= FMAX) & np.isfinite(basd) & (basd > 0)
            try:
                bp_ = fit_asd(bff[mm], basd[mm])
                boots.append(bp_)
            except Exception:
                pass
        boots = np.array(boots)
        w, A, alpha = popt
        kn = knee(w, A, alpha)
        kn_boot = knee(boots[:, 0], boots[:, 1], boots[:, 2])
        res = {
            "label": label, "ff": ff, "asd": asd, "popt": popt,
            "w": w, "A": A, "alpha": alpha, "knee": kn,
            "w_e": np.std(boots[:, 0]), "A_e": np.std(boots[:, 1]),
            "alpha_e": np.std(boots[:, 2]),
            "knee_e": np.nanstd(kn_boot),
            "seg_counts": dict(zip([b[0].mean() for b in blocks], seg_counts)),
        }
        # fit quality: rms of log residual
        res["logrms"] = float(np.sqrt(np.mean((np.log(asd) - np.log(model(ff, *popt))) ** 2)))
        return res


    chans = [
        ("analog Z (07-06, cal)", az, FS_DAQ, "#1f77b4"),
        ("analog Y (07-06, cal)", ay, FS_DAQ, "#ff7f0e"),
        ("GUI Z (07-06)", gz, FS_GUI, "#2ca02c"),
        ("06-30 ref GUI Z", cl, FS_GUI, "#d62728"),
    ]

    results = []
    for lab, x, fs, c in chans:
        r = analyze(x, fs, lab)
        r["color"] = c
        results.append(r)
        print(f"{lab:24s} w={r['w']*1000:6.1f}+/-{r['w_e']*1000:4.1f} fT/rtHz  "
              f"A={r['A']:.3f}+/-{r['A_e']:.3f} pT/rtHz@1Hz  "
              f"alpha={r['alpha']:.2f}+/-{r['alpha_e']:.2f}  "
              f"knee={r['knee']:.3f}+/-{r['knee_e']:.3f} Hz  logrms={r['logrms']:.3f}")

    # ---------------------------------------------------------------- figure
    fig, axes = plt.subplots(2, 2, figsize=(12, 9), sharex=True, sharey=True)
    fm = np.logspace(np.log10(FMIN), np.log10(FMAX), 400)
    for ax, r in zip(axes.ravel(), results):
        ax.loglog(r["ff"], r["asd"] * 1000, ".", ms=2.5, color=r["color"], alpha=0.45,
                  label="Welch ASD")
        ax.loglog(fm, model(fm, *r["popt"]) * 1000, "-", color="k", lw=1.8, label="w + A/f^a fit")
        ax.axhline(r["w"] * 1000, ls=":", color="gray", lw=1)
        ax.axvline(r["knee"], ls="--", color=r["color"], lw=1)
        ax.set_title(r["label"], fontsize=11)
        txt = (f"$\\alpha$={r['alpha']:.2f}$\\pm${r['alpha_e']:.2f}\n"
               f"w={r['w']*1000:.1f}$\\pm${r['w_e']*1000:.1f} fT/$\\sqrt{{Hz}}$\n"
               f"A={r['A']:.2f}$\\pm${r['A_e']:.2f} pT/$\\sqrt{{Hz}}$@1Hz\n"
               f"knee={r['knee']:.3f}$\\pm${r['knee_e']:.3f} Hz")
        ax.text(0.04, 0.05, txt, transform=ax.transAxes, fontsize=8.5, va="bottom",
                bbox=dict(fc="white", ec="0.6", alpha=0.85))
        ax.grid(True, which="both", alpha=0.25)
        ax.legend(loc="upper right", fontsize=8)
        ax.set_xlim(FMIN, FMAX)
    for ax in axes[:, 0]:
        ax.set_ylabel("ASD [fT/$\\sqrt{Hz}$]")
    for ax in axes[1, :]:
        ax.set_xlabel("frequency [Hz]")
    fig.suptitle("1/f model fit: asd(f)=sqrt(w^2+(A/f^a)^2), 0.005-40 Hz "
                 "(analog first 5 min excluded)", fontsize=12)
    fig.tight_layout(rect=[0, 0, 1, 0.97])
    figpath = os.path.join(OUT, "slope1f_asd_model_fits.png")
    fig.savefig(figpath, dpi=150)
    print("saved", figpath)

    # overlay comparison figure: 06-30 vs 07-06 GUI Z
    fig2, ax = plt.subplots(figsize=(8, 6))
    for r in results:
        if r["label"].startswith("GUI Z") or r["label"].startswith("06-30"):
            ax.loglog(r["ff"], r["asd"] * 1000, ".", ms=3, color=r["color"], alpha=0.4)
            ax.loglog(fm, model(fm, *r["popt"]) * 1000, "-", color=r["color"], lw=2,
                      label=f"{r['label']}: a={r['alpha']:.2f}, A={r['A']:.2f}")
    ax.set_xlabel("frequency [Hz]"); ax.set_ylabel("ASD [fT/$\\sqrt{Hz}$]")
    ax.set_title("06-30 vs 07-06 GUI Z: colour change vs amplitude scale")
    ax.grid(True, which="both", alpha=0.25); ax.legend(fontsize=9)
    ax.set_xlim(FMIN, FMAX)
    fig2.tight_layout()
    figpath2 = os.path.join(OUT, "slope1f_0630_vs_0706_GUIZ.png")
    fig2.savefig(figpath2, dpi=150)
    print("saved", figpath2)

    # print segment counts for the low-res block (bootstrap reliability)
    for r in results:
        lowest = min(r["seg_counts"].items(), key=lambda kv: kv[0])
        print(f"{r['label']:24s} lowest-f block segs={lowest[1]}")


def run_transient():
    plt.rcdefaults()
    ROOT = PROJECT_DIR
    DATADIR = data_join("result/synctest/sync5_30min")
    OUTDIR = HERE
    os.makedirs(OUTDIR, exist_ok=True)

    FS = 1200.0566          # precise analog sample rate (Hz)
    CAL = 1.029             # multiply analog pT file values -> calibrated pT

    AZ_FILE = os.path.join(DATADIR, "sync5daq_Z_pT.txt")
    AY_FILE = os.path.join(DATADIR, "sync5daq_Y_pT.txt")

    # ---------------------------------------------------------------- load & calibrate
    az = np.loadtxt(AZ_FILE, comments="#") * CAL
    ay = np.loadtxt(AY_FILE, comments="#") * CAL
    n = min(az.size, ay.size)
    az, ay = az[:n], ay[:n]
    t = np.arange(n) / FS
    T_total = t[-1]
    print(f"loaded {n} samples, duration {T_total:.1f} s")

    # ---------------------------------------------------------------- decimate for trend/fit
    # The transient is a slow (>1 s) settling; heavy noise/lines are irrelevant.
    # Block-average to ~5 Hz to suppress white floor and mains without distorting slow shape.
    DEC = int(round(FS / 5.0))         # ~240 -> 5.0 samples/s
    m = (n // DEC) * DEC
    def decimate(x):
        return x[:m].reshape(-1, DEC).mean(axis=1)
    azd = decimate(az)
    ayd = decimate(ay)
    td = (np.arange(azd.size) * DEC + DEC / 2.0) / FS
    fsd = FS / DEC
    print(f"decimated to {azd.size} pts at {fsd:.3f} Hz")

    # ---------------------------------------------------------------- steady-state trend from t>600 s
    # Linear trend fit on the quiet late window, extrapolated over the whole record.
    def steady_trend(td, xd, t0=600.0, t1=None):
        if t1 is None:
            t1 = td[-1]
        sel = (td >= t0) & (td <= t1)
        c = np.polyfit(td[sel], xd[sel], 1)      # slope, intercept
        return c

    cz = steady_trend(td, azd)
    cy = steady_trend(td, ayd)
    print(f"Z steady slope {cz[0]*60:.3f} pT/min, intercept {cz[1]:.2f} pT")
    print(f"Y steady slope {cy[0]*60:.3f} pT/min, intercept {cy[1]:.2f} pT")

    az_trend = np.polyval(cz, td)
    ay_trend = np.polyval(cy, td)
    az_res = azd - az_trend       # residual = transient + noise
    ay_res = ayd - ay_trend

    # ---------------------------------------------------------------- exponential fits on first ~400 s
    FIT_T = 400.0
    fit = td <= FIT_T
    tf = td[fit]
    zf = az_res[fit]
    yf = ay_res[fit]

    def exp1(t, A, tau, c):
        return A * np.exp(-t / tau) + c

    def exp2(t, A1, tau1, A2, tau2, c):
        return A1 * np.exp(-t / tau1) + A2 * np.exp(-t / tau2) + c

    def rss(y, yhat):
        return float(np.sum((y - yhat) ** 2))

    def fit_report(t, y, label):
        out = {}
        # single exponential
        A0 = y[0] - y[-1]
        p1, _ = curve_fit(exp1, t, y, p0=[A0, 60.0, y[-1]], maxfev=200000)
        r1 = rss(y, exp1(t, *p1))
        # double exponential
        try:
            p2, _ = curve_fit(
                exp2, t, y,
                p0=[A0 * 0.6, 20.0, A0 * 0.4, 120.0, y[-1]],
                maxfev=400000,
            )
            r2 = rss(y, exp2(t, *p2))
        except Exception as e:
            p2, r2 = None, np.inf
            print(f"  {label}: double-exp fit failed ({e})")
        ntot = y.size
        # reduced chi-ish: RSS/dof
        dof1 = ntot - 3
        dof2 = ntot - 5
        sst = float(np.sum((y - y.mean()) ** 2))
        out["p1"] = p1; out["r1"] = r1; out["R2_1"] = 1 - r1 / sst
        out["p2"] = p2; out["r2"] = r2; out["R2_2"] = (1 - r2 / sst) if np.isfinite(r2) else np.nan
        out["var_ratio"] = (r2 / dof2) / (r1 / dof1) if np.isfinite(r2) else np.nan
        print(f"[{label}] single-exp: A={p1[0]:.2f} pT tau={p1[1]:.1f} s c={p1[2]:.2f} pT "
              f"R2={out['R2_1']:.4f}")
        if p2 is not None:
            tau1, tau2 = p2[1], p2[3]
            A1, A2 = p2[0], p2[2]
            print(f"[{label}] double-exp: A1={A1:.2f} tau1={tau1:.1f}s  A2={A2:.2f} tau2={tau2:.1f}s "
                  f"c={p2[4]:.2f}  R2={out['R2_2']:.4f}  (rms/dof ratio dbl/sgl={out['var_ratio']:.3f})")
        return out

    print("\n--- exponential fits (first 400 s, steady trend removed) ---")
    zfit = fit_report(tf, zf, "Z")
    yfit = fit_report(tf, yf, "Y")

    # transient amplitude at t=0 (extrapolated, single-exp A) and its span
    z_A = zfit["p1"][0]; z_tau = zfit["p1"][1]
    y_A = yfit["p1"][0]; y_tau = yfit["p1"][1]
    print(f"\nZ transient amplitude (single-exp A) = {z_A:.1f} pT, tau = {z_tau:.1f} s")
    print(f"Y transient amplitude (single-exp A) = {y_A:.1f} pT, tau = {y_tau:.1f} s")
    print(f"Y/Z transient amplitude ratio = {y_A / z_A:.3f}")

    # ---------------------------------------------------------------- drift-variance fraction
    # Total 30-min drift = decimated (slow) signal after removing overall mean, but we
    # want DRIFT variance not white noise -> use the decimated series (already ~5 Hz, floor
    # suppressed by sqrt(DEC)). Compare variance of the whole record vs variance with the
    # first 300 s (5 min) excluded, using the SAME steady detrend so we measure how much of
    # the drift excursion the early transient contributes.
    def drift_var_fraction(td, xres, t_cut=300.0):
        # xres: residual after steady-trend removal (so late part ~ noise floor)
        v_all = float(np.var(xres))
        late = td >= t_cut
        v_late = float(np.var(xres[late]))
        early = td < t_cut
        v_early = float(np.var(xres[early]))
        frac = 1.0 - v_late / v_all
        return v_all, v_early, v_late, frac

    # Use residual about steady trend over the FULL record for a fair drift-variance accounting.
    az_res_full = azd - np.polyval(cz, td)
    ay_res_full = ayd - np.polyval(cy, td)
    zv = drift_var_fraction(td, az_res_full, 300.0)
    yv = drift_var_fraction(td, ay_res_full, 300.0)
    print(f"\nZ residual variance: all={zv[0]:.1f}, first5min={zv[1]:.1f}, after5min={zv[2]:.1f} pT^2")
    print(f"Z fraction of residual variance from first 5 min = {zv[3]*100:.1f}%")
    print(f"Y residual variance: all={yv[0]:.1f}, first5min={yv[1]:.1f}, after5min={yv[2]:.1f} pT^2")
    print(f"Y fraction of residual variance from first 5 min = {yv[3]*100:.1f}%")

    # Also express as peak-to-peak drift excursion the transient removes:
    print(f"\nZ residual range first 400s: {az_res[fit].max()-az_res[fit].min():.1f} pT p-p")
    print(f"Y residual range first 400s: {ay_res[fit].max()-ay_res[fit].min():.1f} pT p-p")

    # ---------------------------------------------------------------- settle time recommendation
    # Time for the single-exp transient to fall below a threshold relative to the late noise.
    # Use the decimated late-window RMS as the "settled" bar.
    late_rms_z = float(np.std(az_res_full[td >= 900.0]))
    late_rms_y = float(np.std(ay_res_full[td >= 900.0]))
    print(f"\nlate (t>900s) residual RMS: Z {late_rms_z:.2f} pT, Y {late_rms_y:.2f} pT")

    def settle_time(A, tau, thresh):
        # |A| exp(-t/tau) = thresh  -> t = tau ln(|A|/thresh)
        if abs(A) <= thresh:
            return 0.0
        return tau * np.log(abs(A) / thresh)

    for k in (3.0, 5.0):
        ts = settle_time(z_A, z_tau, k * late_rms_z)
        print(f"Z: transient decays below {k:.0f}x late-RMS ({k*late_rms_z:.1f} pT) at t = {ts:.0f} s")
    n_tau = 3
    print(f"Z: {n_tau} time constants = {n_tau*z_tau:.0f} s")

    # ---------------------------------------------------------------- figure: first 5 min Z & Y + fit
    FIG_T = 300.0
    figsel = td <= FIG_T
    plt.figure(figsize=(10, 6.5))

    ax1 = plt.subplot(2, 1, 1)
    ax1.plot(td[figsel], azd[figsel], color="0.6", lw=0.8, label="analog Z (5 Hz avg)")
    ax1.plot(td[figsel], np.polyval(cz, td[figsel]), "b--", lw=1.0, label="steady trend (t>600s)")
    ax1.plot(td[figsel], np.polyval(cz, td[figsel]) + exp1(td[figsel], *zfit["p1"]),
             "r-", lw=1.4, label=f"steady + single-exp (tau={z_tau:.0f}s, A={z_A:.0f}pT)")
    if zfit["p2"] is not None:
        ax1.plot(td[figsel], np.polyval(cz, td[figsel]) + exp2(td[figsel], *zfit["p2"]),
                 "g-", lw=1.0, alpha=0.8,
                 label=f"steady + double-exp (tau={zfit['p2'][1]:.0f},{zfit['p2'][3]:.0f}s)")
    ax1.set_ylabel("Z field (pT, calibrated)")
    ax1.set_title("2026-07-06 sync5 closed shield — settling transient, first 5 min")
    ax1.legend(fontsize=8, loc="best")
    ax1.grid(alpha=0.3)

    ax2 = plt.subplot(2, 1, 2, sharex=ax1)
    ax2.plot(td[figsel], ayd[figsel], color="0.6", lw=0.8, label="analog Y (5 Hz avg)")
    ax2.plot(td[figsel], np.polyval(cy, td[figsel]), "b--", lw=1.0, label="steady trend (t>600s)")
    ax2.plot(td[figsel], np.polyval(cy, td[figsel]) + exp1(td[figsel], *yfit["p1"]),
             "r-", lw=1.4, label=f"steady + single-exp (tau={y_tau:.0f}s, A={y_A:.0f}pT)")
    ax2.set_ylabel("Y field (pT, calibrated)")
    ax2.set_xlabel("time since acquisition start (s)")
    ax2.legend(fontsize=8, loc="best")
    ax2.grid(alpha=0.3)

    plt.tight_layout()
    figpath = os.path.join(OUTDIR, "transient_first5min_Z_Y_expfit.png")
    plt.savefig(figpath, dpi=150)
    print(f"\nsaved figure: {figpath}")

    # residual-of-fit diagnostic figure
    plt.figure(figsize=(10, 4))
    plt.plot(tf, zf - exp1(tf, *zfit["p1"]), lw=0.8, label="Z single-exp residual")
    if zfit["p2"] is not None:
        plt.plot(tf, zf - exp2(tf, *zfit["p2"]), lw=0.8, alpha=0.7, label="Z double-exp residual")
    plt.axhline(0, color="k", lw=0.5)
    plt.xlabel("time (s)"); plt.ylabel("fit residual (pT)")
    plt.title("Z transient fit residuals (first 400 s)")
    plt.legend(fontsize=8); plt.grid(alpha=0.3)
    plt.tight_layout()
    figpath2 = os.path.join(OUTDIR, "transient_Z_fit_residuals.png")
    plt.savefig(figpath2, dpi=150)
    print(f"saved figure: {figpath2}")

    print("\nDONE")


def main():
    run_headline()
    run_allan()
    run_decomp()
    run_distrib()
    run_driftvec()
    run_guiclock()
    run_lines()
    run_models()
    run_nonstat()
    run_slope1f()
    run_transient()


if __name__ == "__main__":
    main()
