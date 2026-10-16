#!/usr/bin/env python3
"""The 07-28 16:12 "wide window" run: what +-10 V bought and what it did not.

The 15:43 run lost Z to the ADC rail at +-1 V, so the next attempt widened
the DAQ window all the way to +-10 V. Z came back inside the window and the
tool reported status "ok", exit 0 -- and the Z record is still worthless.
The limit that bit is the QZFM's OWN analog output, which saturates near
+4.27 / -4.32 V (not the +-5 V we assumed), so no `--vrange` can reach past
it. Over the full 1800 s the Z channel sits on three ADC codes, 99.7 % of
them the same one, and its 60 s block means do not move by 0.1 pT -- while
the GUI, reading the same axis of the same head over the same wall clock,
walks 529 pT.

The analog Y record is good (78 codes, 88 pT of drift, no rail), but at
+-10 V one code is 5.11 mV = 1.89 pT and the chain floor climbs to roughly
26 fT/rtHz. The saturated Z ASD is flat ~4 fT/rtHz at every frequency.

Between 8 and 38 Hz the MS-1L records carry a forest of lines that the
QZFM-off chain floor does not have, so it is field (or sensor response to
motion), not readout. A broad ~1.5 Hz-wide bump at 29.46 Hz agrees to 1 %
across two runs 29 min apart.

Writes four figures next to this script:
  fig_widewindow_0728_field.png
  fig_widewindow_0728_codes.png
  fig_widewindow_0728_asd.png
  fig_widewindow_0728_vibration.png
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

FS_A = 1200.0
FS_G = 203.151
PT_PER_V = 370.3703703703703
LSB_10V = 5.111e-3
TARGET_FT = 50.0

RUN = "result/ms1l_2_degaussfail/widewindow_10V_1800s"
GUI = f"{RUN}/gui_widewindow_Formatted.txt"

DAY = (
    ("16:12 Y ±10 V", None, FS_A, "#4F8A4A", 1.3, "-"),
    ("15:43 Y ±1 V",
     "result/ms1l_2_degaussfail/newhead_ab3s_900s/newhead_ab3s_Y_pT.txt", FS_A,
     "#D89A96", 1.2, "-"),
    ("12:13 Z ±5 V",
     "result/ms1l_2_degaussfail/swapref_oldhead_900s/swapref_old_Z_pT.txt", FS_A,
     "#9AA0A6", 1.0, "-"),
    ("07-13 Z ±1 V",
     "result/ms1l_1_predegauss/predegauss_900s/predegaussdaq_Z_pT.txt", FS_A,
     "#33373B", 1.0, "--"),
    ("QZFM OFF",
     "result/ms1l_2_degaussfail/daqfloor_qzfmoff_2V_60s/daqfloor_2V_Y_pT.txt", FS_A,
     "#B07A2E", 1.2, "-"),
)


def load_gui(rel):
    return np.loadtxt(data_join(rel), delimiter="\t",
                      skiprows=1, usecols=(1,))


from bfield.analysis.spectra import spliced_asd  # noqa: E402 -- was an identical local copy


from bfield.analysis.spectra import band_median  # noqa: E402 -- was an identical local copy


def blocks(x, fs, seg_s):
    k = int(seg_s * fs)
    n = len(x) // k
    return (np.arange(n) + 0.5) * seg_s, x[: n * k].reshape(n, k).mean(axis=1)


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


def plot_field(gui, y_pt, z_pt):
    fig, ax = plt.subplots(figsize=(6.0, 4.2))
    tg, bg = blocks(gui, FS_G, 30.0)
    ty, by = blocks(y_pt, FS_A, 30.0)
    tz, bz = blocks(z_pt, FS_A, 30.0)
    ax.plot(tg / 60.0, bg, color="#2E6E8E", lw=1.5, label="GUI Z")
    ax.plot(tz / 60.0, bz, color="#C0504D", lw=2.0, label="analog Z")
    ax.plot(ty / 60.0, by, color="#4F8A4A", lw=1.4, label="analog Y")
    ax.set_xlabel("time (min)")
    ax.set_ylabel("field (pT)")
    ax.set_title("MS-1L AB3S ±10 V: absolute field")
    ax.legend(loc="center right", fontsize=10)
    fig.tight_layout()
    save(fig, "fig_widewindow_0728_field.png")


def plot_codes(vy, vz):
    fig, ax = plt.subplots(figsize=(6.0, 4.2))
    for lab, v, col in (("Y", vy, "#4F8A4A"), ("Z", vz, "#C0504D")):
        u, c = np.unique(v, return_counts=True)
        ax.bar(u, c / len(v) * 100.0, width=LSB_10V, color=col,
               edgecolor="none", label=f"{lab}: {len(u)} codes")
    ax.set_yscale("log")
    ax.set_ylim(1e-3, 2000)
    ax.set_xlim(-5.6, 1.0)
    ax.set_xlabel("ADC code (V)")
    ax.set_ylabel("share of samples (%)")
    ax.set_title("MS-1L AB3S ±10 V: ADC code occupancy")
    ax.legend(loc="upper left", fontsize=10)
    fig.tight_layout()
    save(fig, "fig_widewindow_0728_codes.png")


def plot_asd(gui, y_pt, z_pt):
    fig, ax = plt.subplots(figsize=(6.0, 4.2))
    for lab, x, fs, col, lw in (
            ("analog Y", y_pt * 1e3, FS_A, "#4F8A4A", 1.4),
            ("GUI Z", gui * 1e3, FS_G, "#2E6E8E", 1.4),
            ("analog Z (sat.)", z_pt * 1e3, FS_A, "#C0504D", 1.6)):
        f, a = spliced_asd(x, fs)
        ax.loglog(f, a, color=col, lw=lw, label=lab)
    ax.axhline(TARGET_FT, color="#33373B", ls="--", lw=1.0, alpha=0.75)
    ax.text(3e-2, TARGET_FT * 1.25, "50 fT target", fontsize=9, va="bottom")
    ax.set_xlim(2e-3, 100)
    ax.set_ylim(1.0, 2e5)
    ax.set_xlabel("frequency (Hz)")
    ax.set_ylabel("ASD (fT/√Hz)")
    ax.set_title("MS-1L AB3S ±10 V: noise spectrum")
    ax.legend(loc="upper right", fontsize=10)
    fig.tight_layout()
    save(fig, "fig_widewindow_0728_asd.png")


def channel_ok(rel):
    """Refuse a channel its own run flagged (REPORT_PLAN §6, analysis/run_audit.py).

    This comparison draws on newhead_ab3s, whose z sat against the rail
    for 95 % of the record. It happens to use y, which is clean -- but
    nothing here was checking, so that was luck. A railed channel is
    dropped loudly rather than plotted.
    """
    import glob
    import json
    run = os.path.dirname(data_join(rel))
    metas = glob.glob(os.path.join(run, "*.meta.json"))
    if not metas:
        return True                     # nothing recorded; caller's problem
    with open(metas[0], encoding="utf-8") as fh:
        meta = json.load(fh)
    ch = os.path.basename(rel).rsplit("_", 2)[-2].lower()
    rail = (meta.get("rail_fraction") or {}).get(ch, 0.0)
    if rail > 0.001:
        print(f"[skip] {rel}: {ch} railed for {rail:.1%} of the run")
        return False
    return True


def plot_vibration(y_pt):
    fig, ax = plt.subplots(figsize=(6.0, 4.2))
    for lab, rel, fs, col, lw, ls in DAY:
        if rel is not None and not channel_ok(rel):
            continue
        x = y_pt * 1e3 if rel is None else np.loadtxt(
            data_join(rel)) * 1e3
        f, p = welch(x, fs=fs, nperseg=min(len(x), int(50.0 * fs)),
                     detrend="linear")
        a = np.sqrt(p)
        m = (f >= 8) & (f <= 40)
        ax.semilogy(f[m], a[m], color=col, lw=lw, ls=ls, label=lab)
    ax.set_ylim(3, 1.2e4)
    ax.set_xlabel("frequency (Hz)")
    ax.set_ylabel("ASD (fT/√Hz)")
    ax.set_title("MS-1L: 8–40 Hz band (vibration)")
    ax.legend(loc="upper right", fontsize=9)
    fig.tight_layout()
    save(fig, "fig_widewindow_0728_vibration.png")


def main():
    style()
    raw = np.loadtxt(data_join(RUN, "widewindow_raw.csv"),
                     delimiter=",", skiprows=2)
    vy, vz = raw[:, 1], raw[:, 2]
    y_pt, z_pt = vy * PT_PER_V, vz * PT_PER_V
    gui = load_gui(GUI)

    plot_field(gui, y_pt, z_pt)
    plot_codes(vy, vz)
    plot_asd(gui, y_pt, z_pt)
    plot_vibration(y_pt)

    print("\nband medians (fT/rtHz)")
    print(f"{'record':28s}{'0.05 Hz':>10s}{'0.01-1':>9s}{'4-12':>8s}"
          f"{'40-80':>8s}{'80-300':>8s}")
    for lab, x, fs in (("analog Y  +-10 V", y_pt * 1e3, FS_A),
                       ("analog Z  SATURATED", z_pt * 1e3, FS_A),
                       ("GUI Z  3062 s", gui * 1e3, FS_G)):
        f, a = spliced_asd(x, fs, lowseg=400.0 if fs < 500 else 200.0)
        i = int(np.argmin(np.abs(f - 0.05)))
        hi = band_median(f, a, 80, min(300, fs / 2 - 5))
        print(f"{lab:28s}{a[i]:10.0f}{band_median(f,a,0.01,1.0):9.0f}"
              f"{band_median(f,a,4,12):8.1f}{band_median(f,a,40,80):8.1f}"
              f"{hi:8.1f}")


if __name__ == "__main__":
    main()
