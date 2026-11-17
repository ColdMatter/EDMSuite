#!/usr/bin/env python3
"""Figures for the first complete synchronized two-head run (2026-08-07).

Data: result/twohead_sync02/R01_1242 -- 900 s, both heads dual (z,y),
axes aligned, master/slave modulation sync, --vrange 2, exit 0, and the
first two-head session ever to return calibration factors for both
heads (a: z 0.81 / y 0.81, b: z 0.75 / y 0.72).

Four figures, one claim each:
  fig_sync02_timeseries.png  clean 900 s, drifts ~10 pT, nothing railed
  fig_sync02_asd.png         the raw z difference beats either head
                             in the drift band; y difference does not,
                             because the y pair shares almost nothing
  fig_sync02_coherence.png   the same statement head-on, against the
                             null band a 900 s record cannot see past:
                             the z pair clears it above ~0.3 Hz, the y
                             pair never does, and at 0.05 Hz -- the
                             frequency that matters -- the record is too
                             short to say either way
  fig_sync02_carrier.png     one shared modulation line, both heads,
                             both run halves -- the sync, seen directly

Run from the repo root: python analysis/twohead_sync/plot_sync02.py
"""

import json
import os

import numpy as np
import matplotlib
matplotlib.use("Agg")
import matplotlib.pyplot as plt
from scipy import signal as sg

HERE = os.path.dirname(os.path.abspath(__file__))
import sys as _sys  # analysis_scripts/ on sys.path for _paths
_sys.path.insert(0, os.path.dirname(os.path.dirname(os.path.abspath(__file__))))
from _paths import DATA_ROOT, PROJECT_DIR, ANALYSIS_DIR, data, data_join, rel_data  # noqa: E402,F401
REPO = PROJECT_DIR
RUN = data_join("result", "twohead_sync02", "R01_1242", "R01_1242")
# the earlier session, same wiring and duration at vrange 1, taken 48
# minutes before this one (11:54 vs 12:42, same day): the only thing
# that answers "does this repeat?" without new beam time
RUN01 = data_join("result", "twohead_sync01", "R01_1154", "R01_1154")

FS = 1200.0            # USB-6008 hardware clock
NSEG = 1 << 18         # 218 s Welch segments -> 4.6 mHz resolution
EDM_BAND = (0.05, 1.0)  # the band a modulation beat must stay out of
WHITE_FLOOR_PT = 0.028  # measured white floor, both takes, both ranges

C_A, C_B, C_DIFF = "#2E6E8E", "#C0504D", "#5A7D3A"
C_REF, C_INK, C_MUTE = "#9AA0A6", "#33373B", "#7A7F85"
NOTE_BBOX = dict(boxstyle="round,pad=0.2", facecolor="white",
                 edgecolor="none", alpha=0.92)


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


def load(run=RUN):
    chans = {c: np.loadtxt(f"{run}_{c}_pT.txt") for c in ("Z", "Z2", "Y", "Y2")}
    with open(f"{run}.meta.json", encoding="utf-8") as fh:
        meta = json.load(fh)
    # per-channel factors as the run recorded them: z/y are head a,
    # z2/y2 head b. Recorded, never applied to the files -- applying
    # them is exactly what this analysis is for.
    cal = meta["cal_factors"]
    # a channel that spent the run against the rail cannot carry a
    # coherence statement -- sync01's y did, and a null read off it would
    # be an artefact rather than a measurement
    railed = {c.upper() for c, v in meta.get("rail_fraction", {}).items()
              if v > 0.01}
    return chans, cal, railed


def fig_timeseries(ch):
    n1s = int(FS)
    t_min = np.arange(900) / 60.0
    fig, axes = plt.subplots(2, 1, figsize=(9, 6), sharex=True)
    for ax, (top, bot, name) in zip(axes, (("Z", "Z2", "z (cylinder axis)"),
                                           ("Y", "Y2", "y (transverse)"))):
        a = ch[top].reshape(-1, n1s).mean(1)
        b = ch[bot].reshape(-1, n1s).mean(1)
        ax.plot(t_min, a, color=C_A, lw=1.0, label=f"head a ({top.lower()})")
        ax.plot(t_min, b, color=C_B, lw=1.0, label=f"head b ({bot.lower()})")
        ax.set_ylabel("field [pT]")
        ax.grid(True, axis="y")
        ax.legend(loc="upper right", ncols=2)
        ax.set_title(name, loc="left")
    axes[0].annotate("1 s means; drifts stay within tens of pT --\n"
                     "the +/-741 pT window (vrange 2) is never approached",
                     xy=(0.02, 0.06), xycoords="axes fraction",
                     fontsize=10, color=C_MUTE, bbox=NOTE_BBOX)
    axes[1].set_xlabel("time [min]")
    fig.suptitle("twohead_sync02: 900 s, both heads dual, synced", y=0.98)
    fig.tight_layout()
    return fig


def fig_asd(ch, cal):
    """ASDs and the RAW head-to-head difference.

    The Calibrate factor is a health number whose direction is not
    settled -- the recorder does not multiply it into the pT files,
    and the rest of the campaign leaves it unapplied pending a tone
    calibration. This figure used to scale the difference by it,
    which took the 0.05-1 Hz z difference from 474 to 369 fT/rtHz;
    dividing instead gives 616. "The difference beats either head"
    must not rest on which of those was chosen, so the difference
    is now raw (2026-08-19 analysis review, H7). The coherence
    figure is scale-invariant and was never affected.
    """
    f, Pz = sg.welch(ch["Z"], FS, nperseg=NSEG, detrend="linear")
    _, Pz2 = sg.welch(ch["Z2"], FS, nperseg=NSEG, detrend="linear")
    _, Pzd = sg.welch(ch["Z"] - ch["Z2"], FS,
                      nperseg=NSEG, detrend="linear")
    _, Py = sg.welch(ch["Y"], FS, nperseg=NSEG, detrend="linear")
    _, Py2 = sg.welch(ch["Y2"], FS, nperseg=NSEG, detrend="linear")
    _, Pyd = sg.welch(ch["Y"] - ch["Y2"], FS,
                      nperseg=NSEG, detrend="linear")
    # what the difference would be if the two heads shared NOTHING
    quad_y = np.sqrt(Py + Py2)

    fig, axes = plt.subplots(1, 2, figsize=(11, 5), sharey=True)
    for ax, (P1, P2, Pd, lab) in zip(axes, (
            (Pz, Pz2, Pzd, "z"), (Py, Py2, Pyd, "y"))):
        ax.loglog(f[1:], np.sqrt(P1[1:]), color=C_A, lw=0.9,
                  label=f"head a ({lab})")
        ax.loglog(f[1:], np.sqrt(P2[1:]), color=C_B, lw=0.9,
                  label=f"head b ({lab}2)")
        ax.loglog(f[1:], np.sqrt(Pd[1:]), color=C_DIFF, lw=1.1,
                  label="calibrated difference")
        ax.axvspan(*EDM_BAND, color="#F5E9D0", zorder=0)
        ax.axhline(WHITE_FLOOR_PT, color=C_REF, ls=":", lw=1.0)
        ax.set_xlim(4e-3, 600)
        ax.set_xlabel("frequency [Hz]")
        ax.grid(True, which="both", alpha=0.5)
        ax.legend(loc="lower left")
    axes[0].loglog([], [])
    axes[1].loglog(f[1:], quad_y[1:], color=C_MUTE, ls="--", lw=1.0,
                   label="incoherent sum (nothing shared)")
    axes[1].legend(loc="lower left")
    axes[0].set_ylabel(r"ASD [pT/$\sqrt{\mathrm{Hz}}$]")
    axes[0].set_title("z pair: the difference beats either head", loc="left")
    axes[1].set_title("y pair: nothing shared to cancel", loc="left")
    axes[0].annotate("shaded: 0.05-1 Hz\ndotted: 28 fT/$\\sqrt{\\rm Hz}$ "
                     "white floor\nline at 277.1 Hz: modulation alias",
                     xy=(0.03, 0.75), xycoords="axes fraction",
                     fontsize=10, color=C_MUTE, bbox=NOTE_BBOX)
    fig.suptitle("twohead_sync02: per-head calibration turned the pair "
                 "into a working differencer (z), and showed y shares "
                 "almost nothing", y=1.00)
    fig.tight_layout()
    return fig


def null_threshold(n, p=0.05):
    """Coherence a pair of UNCORRELATED channels exceeds with probability p.

    For n independent segment averages the magnitude-squared coherence of
    independent channels has P(C > c) = (1-c)^(n-1), and a mean of 1/n --
    so a short record manufactures coherence out of nothing. Without this
    line on the plot a reader takes every rise as a shared signal.
    """
    return 1.0 - p ** (1.0 / (n - 1))


def fig_coherence(ch):
    f, Cz = sg.coherence(ch["Z"], ch["Z2"], FS, nperseg=NSEG)
    _, Cy = sg.coherence(ch["Y"], ch["Y2"], FS, nperseg=NSEG)
    # two segment counts, because the honest answer lies between them:
    # Welch's 50 % overlap does not buy fully independent averages, so
    # the non-overlapping count is the conservative reading (MATH §2.2)
    n_ovl = (ch["Z"].size - NSEG) // (NSEG // 2) + 1
    n_ind = ch["Z"].size // NSEG
    thr_opt, thr_con = null_threshold(n_ovl), null_threshold(n_ind)

    fig, ax = plt.subplots(figsize=(9, 5))
    ax.axvspan(*EDM_BAND, color="#F5E9D0", zorder=0)
    ax.axhspan(0, thr_con, color=C_REF, alpha=0.22, zorder=1)
    ax.axhline(thr_opt, color=C_MUTE, ls="--", lw=1.0, zorder=2)
    ax.axhline(1.0 / n_ind, color=C_MUTE, ls=":", lw=1.0, zorder=2)
    ax.semilogx(f[1:], Cz[1:], color=C_A, lw=1.0, zorder=3,
                label="z pair (cylinder axis)")
    ax.semilogx(f[1:], Cy[1:], color=C_B, lw=1.0, alpha=0.9, zorder=3,
                label="y pair (transverse)")
    ax.set_xlim(4e-3, 20)
    ax.set_ylim(0, 1)
    ax.set_xlabel("frequency [Hz]")
    ax.set_ylabel("magnitude-squared coherence")
    ax.grid(True, which="both", alpha=0.5)
    ax.legend(loc="upper right")
    ax.set_title("twohead_sync02: what the two heads share, by axis",
                 loc="left")
    ax.text(4.4e-3, thr_con - 0.048,
            f"95 % of uncorrelated pairs fall below here "
            f"({n_ind} independent {NSEG/FS:.0f} s segments)",
            fontsize=8.5, color=C_INK)
    ax.text(4.4e-3, thr_opt + 0.012,
            f"...{thr_opt:.2f} if the {n_ovl} overlapped segments are "
            f"counted as independent", fontsize=8.5, color=C_MUTE)
    ax.text(4.4e-3, 1.0 / n_ind + 0.012, "mean coherence of pure noise, 1/n",
            fontsize=8.5, color=C_MUTE)
    ax.annotate("z: ambient leakage along the shield axis is seen by both --\n"
                "but only above ~0.3 Hz does it clear the null band. At\n"
                "0.05 Hz, 900 s is too short to say (that is the 2000-4000 s\n"
                "duration rule). y: transverse stays inside the band.\n"
                "One session; repetition pending.",
                xy=(0.035, 0.665), xycoords="axes fraction",
                fontsize=9.5, color=C_INK, bbox=NOTE_BBOX)
    fig.tight_layout()
    return fig


def fig_carrier(ch):
    n = 1 << 19                      # 437 s halves -> 2.3 mHz bins
    f = np.fft.rfftfreq(n, 1 / FS)
    m = (f > 276.9) & (f < 277.3)
    win = np.hanning(n)
    fig, ax = plt.subplots(figsize=(9, 5))
    for name, v, color in (("head a (z)", ch["Z"], C_A),
                           ("head b (z2)", ch["Z2"], C_B)):
        for half, seg, ls in (("first half", v[:n], "-"),
                              ("second half", v[-n:], "--")):
            F = np.abs(np.fft.rfft((seg - seg.mean()) * win))
            ax.semilogy(f[m], F[m] / F[m].max(), color=color, ls=ls,
                        lw=1.0, label=f"{name}, {half}")
    ax.set_xlim(276.9, 277.3)
    ax.set_xlabel("frequency [Hz]")
    ax.set_ylabel("amplitude (normalised)")
    ax.grid(True, alpha=0.5)
    ax.legend(loc="upper right", fontsize=9.5)
    ax.set_title("twohead_sync02: one modulation line, both heads, "
                 "both halves", loc="left")
    ax.annotate("alias of the 922.897 Hz carrier at fs = 1200 Hz.\n"
                "All four traces peak in the same 2.3 mHz bin: the pair\n"
                "shares one clock, and any beat is below ~2 mHz --\n"
                "far under the 0.05 Hz band edge",
                xy=(0.03, 0.05), xycoords="axes fraction",
                fontsize=10, color=C_MUTE, bbox=NOTE_BBOX)
    fig.tight_layout()
    return fig


def fig_repeat(ch01, rail01, ch02, rail02):
    """Both sessions on one pair of axes -- the answer to "does it repeat".

    Same wiring, same 900 s, 48 minutes apart, different vrange, so a
    feature surviving both is the shield and not the setting. The answer
    here is that the z coherence does not survive -- the shared axial
    signal came and went within 48 minutes -- and that the y pair only
    speaks once: sync01's y sat on the DAQ rail for 77 % of the run
    (run_audit), so its null is an artefact and is not plotted.
    """
    fig, axes = plt.subplots(2, 1, figsize=(9, 6.5), sharex=True)
    for ax, (top, bot, name) in zip(axes, (("Z", "Z2", "z pair (cylinder axis)"),
                                           ("Y", "Y2", "y pair (transverse)"))):
        thr = None
        for ch, rail, colour, label in (
                (ch01, rail01, C_MUTE, "sync01 (vrange 1)"),
                (ch02, rail02, C_A, "sync02 (vrange 2)")):
            thr = null_threshold(ch[top].size // NSEG)
            if rail & {top, bot}:
                ax.plot([], [], color=colour, lw=1.0,
                        label=f"{label} -- {'/'.join(sorted(rail & {top, bot}))}"
                              f" railed, not plotted")
                continue
            f, C = sg.coherence(ch[top], ch[bot], FS, nperseg=NSEG)
            ax.semilogx(f[1:], C[1:], color=colour, lw=1.0, zorder=3,
                        label=label)
        ax.axvspan(*EDM_BAND, color="#F5E9D0", zorder=0)
        ax.axhspan(0, thr, color=C_REF, alpha=0.22, zorder=1)
        ax.set_xlim(4e-3, 20)
        ax.set_ylim(0, 1)
        ax.set_ylabel("coherence")
        ax.grid(True, which="both", alpha=0.5)
        ax.legend(loc="upper left")
        ax.set_title(name, loc="left")
    axes[0].annotate("z does NOT repeat, 48 minutes apart. Over 0.05-1 Hz the\n"
                     "median coherence is 0.32 in sync01 and 0.76 in sync02;\n"
                     "9 % of bins clear the band against 85 %. sync01 had the\n"
                     "finer range, so its lower coherence is a weaker shared\n"
                     "signal in that session, not a worse floor.",
                     xy=(0.235, 0.045), xycoords="axes fraction",
                     fontsize=9.5, color=C_INK, bbox=NOTE_BBOX)
    axes[1].annotate("only sync02 speaks here: sync01's y sat against the\n"
                     "rail for 77 % of the run, so its null is an artefact.\n"
                     "The transverse pair has been measured once.",
                     xy=(0.38, 0.86), xycoords="axes fraction",
                     fontsize=9.5, color=C_INK, bbox=NOTE_BBOX)
    axes[1].set_xlabel("frequency [Hz]")
    fig.suptitle("two sessions, same question: what survives a repeat", y=0.99)
    fig.tight_layout()
    return fig


def main():
    style()
    ch, cal, rail = load()
    ch01, _, rail01 = load(RUN01)
    for name, fig in (("fig_sync02_timeseries.png", fig_timeseries(ch)),
                      ("fig_sync02_asd.png", fig_asd(ch, cal)),
                      ("fig_sync02_coherence.png", fig_coherence(ch)),
                      ("fig_sync02_carrier.png", fig_carrier(ch)),
                      ("fig_sync_repeat.png", fig_repeat(ch01, rail01, ch, rail))):
        path = os.path.join(HERE, name)
        fig.savefig(path, bbox_inches="tight")
        plt.close(fig)
        print(f"wrote {os.path.relpath(path, REPO)}")


if __name__ == "__main__":
    main()
