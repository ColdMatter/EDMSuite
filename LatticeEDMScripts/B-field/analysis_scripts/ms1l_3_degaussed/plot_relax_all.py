#!/usr/bin/env python3
"""Relaxation after a degauss cycle: three windings on one time axis.

Ten records, three degauss campaigns, one shield. t0 is the end of the
current taper, read off the degausser panel screenshots:

  N=16  22.4 A-turns   07-29 14:16   -> 5 records out to t0+25.6 h
  N=8   25.6 then 20.8 07-30 16:56   -> 3 records out to t0+90.7 h
  N=4   18 A-turns     08-03 14:50   -> 2 records at t0+16 and +48 min

Every statistic below is taken on the FIRST 900 s of each record. That
matters: the fitted slope of a red-noise record shrinks with window
length, so the one 1800 s run is truncated rather than compared as-is.

Two statistics are tracked because they answer different questions.
  - the 0.05 Hz ASD is the requirement-facing number (target 50 fT/rtHz);
  - the peak-to-peak excursion and the fitted drift of the 900 s record
    are dominated by content below ~0.005 Hz, i.e. the part of the
    spectrum the ASD window cannot resolve.

Neither is interpreted here as "the shield relaxing". Every record in
this set was preceded by its own Field Zero + Calibrate -- the recorded
factors differ from run to run -- so the sensor's own settling travels
with the shield's, and the 07-30 pair (both re-zeroed, 53 min apart,
no degauss between them) moves the wrong way. See the printed control.

Writes fig_relax_all.png next to this script.
"""

import datetime as dt
import json
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
WIN = int(900 * FS)
TARGET_FT = 50.0

# run-to-run scatter measured in the 08-01 audit: CV ~ 0.29 across
# nominally identical runs (chi-square p ~ 0.003 against Welch-only), so
# a ratio of two runs carries sigma_ln = 0.29*sqrt(2) = 0.41
RUN_CV = 0.29
SIGMA_LN = RUN_CV * np.sqrt(2.0)

T0 = {
    "N=16": dt.datetime(2026, 7, 29, 14, 16),
    "N=8": dt.datetime(2026, 7, 30, 16, 56),
    "N=4": dt.datetime(2026, 8, 3, 14, 50),
}
COLOR = {"N=16": "#C0504D", "N=8": "#2E5FA3", "N=4": "#4E7B52"}
DOSE = {"N=16": "22.4 A·t", "N=8": "20.8–25.6 A·t", "N=4": "18 A·t"}

RUNS = [
    ("N=16", "postdegauss0729_900s/postdg0729"),
    ("N=16", "relax0729_t80m_900s/relax_t80m"),
    ("N=16", "relax0729_t104m_900s/relax_t104m"),
    ("N=16", "relax0730_t25h_1800s/relax_t25h"),
    ("N=16", "relax0730_t26h_900s/relax_t26h"),
    ("N=8", "postdg0730_n8_900s/postdg_n8"),
    ("N=8", "postdg0730_n8_2nd_900s/postdg_n8_2nd"),
    ("N=8", "relax0803_t90h_900s/relax0803_t90h"),
    ("N=4", "postdg0803_n4_900s/postdg_n4"),
    ("N=4", "relax0803_n4_t48m_900s/relax_n4_t48m"),
]
# first and last record of the same lab session, per campaign
SESSION_PAIR = {"N=16": (0, 2), "N=8": (5, 6), "N=4": (8, 9)}
CONTROL = (3, 4)  # 07-30 afternoon, both re-zeroed, no degauss between


from bfield.analysis.spectra import spliced_asd  # noqa: E402 -- was an identical local copy


def measure(path):
    x = np.loadtxt(path)[:WIN]
    f, a = spliced_asd(x, FS)
    t = np.arange(len(x)) / FS / 60.0
    return dict(
        f=f, a=a,
        v005=a[np.argmin(np.abs(f - 0.05))] * 1e3,
        v01=a[np.argmin(np.abs(f - 0.01))] * 1e3,
        mid=np.median(a[(f >= 0.1) & (f <= 1)]) * 1e3,
        white=np.median(a[(f >= 30) & (f <= 45)]) * 1e3,
        drift=abs(np.polyfit(t, x, 1)[0]), ptp=np.ptp(x))


def style():
    plt.rcParams.update({
        "figure.dpi": 150, "font.size": 10.5, "axes.titleweight": "bold",
        "axes.spines.top": False, "axes.spines.right": False,
        "axes.edgecolor": "#7A7F85", "text.color": "#33373B",
        "legend.frameon": False,
    })


def load():
    rows = []
    for wind, stem in RUNS:
        base = data_join("result/ms1l_3_degaussed", stem)
        with open(base + ".meta.json") as fh:
            meta = json.load(fh)
        start = dt.datetime.fromisoformat(meta["start_iso"])
        rows.append(dict(
            wind=wind, stem=stem, name=os.path.basename(stem),
            t_min=(start - T0[wind]).total_seconds() / 60.0,
            cal=meta.get("cal_factors"),
            Z=measure(f"{base}_Z_pT.txt"), Y=measure(f"{base}_Y_pT.txt")))
    return rows


def print_table(rows):
    hdr = ("winding  t-t0       run                  cal z   |drift|   ptp"
           "    0.01Hz  0.05Hz  0.1-1Hz  white")
    print("all statistics on the first 900 s of every record; Z axis\n")
    print(hdr)
    print("-" * len(hdr))
    for r in rows:
        z = r["Z"]
        lab = f"{r['t_min']/60:.1f} h" if r["t_min"] > 180 \
            else f"{r['t_min']:.0f} min"
        print(f"{r['wind']:<8s} {lab:<10s} {r['name']:<20s} "
              f"{r['cal']['z']:<6} {z['drift']:7.2f} {z['ptp']:6.1f}  "
              f"{z['v01']:6.0f}  {z['v005']:6.0f}  {z['mid']:7.0f}  "
              f"{z['white']:5.1f}")


def print_pairs(rows):
    print("\npaired change within one lab session, first record -> last")
    print("(N=16 46->103 min, N=8 17->40 min, N=4 16->48 min)")
    for key, name in (("drift", "|drift|"), ("ptp", "ptp"),
                      ("v01", "0.01 Hz"), ("v005", "0.05 Hz"),
                      ("mid", "0.1-1 Hz")):
        parts, lr = [], []
        for wind, (i, j) in SESSION_PAIR.items():
            for ax in ("Z", "Y"):
                ratio = rows[j][ax][key] / rows[i][ax][key]
                parts.append(f"{wind} {ax} x{ratio:.2f}")
                lr.append(np.log(ratio))
        lr = np.array(lr)
        sem = lr.std(ddof=1) / np.sqrt(len(lr))
        print(f"  {name:<9s} " + " ".join(f"{p:<12s}" for p in parts)
              + f"| mean x{np.exp(lr.mean()):.2f} ({abs(lr.mean()/sem):.1f}"
              f" sigma of the spread, n={len(lr)})")
    print("\n  the six ratios are three campaigns x two axes, and the two")
    print("  axes of one record move together, so n=6 overstates; the Z-only")
    print("  test has 2 dof.")

    i, j = CONTROL
    print("\ncontrol -- 07-30 t0+24.7 h and t0+25.6 h, 53 min apart, no")
    print("degauss between them, and BOTH preceded by a Field Zero +")
    print(f"Calibrate (recorded z factors {rows[i]['cal']['z']} then "
          f"{rows[j]['cal']['z']}):")
    for ax in ("Z", "Y"):
        a, b = rows[i][ax], rows[j][ax]
        print(f"  {ax}: |drift| {a['drift']:5.2f} -> {b['drift']:5.2f} pT/min"
              f"   ptp {a['ptp']:5.0f} -> {b['ptp']:5.0f} pT"
              f"   0.05 Hz {a['v005']:5.0f} -> {b['v005']:5.0f}")
    print("  -> the drift and ptp statistics move the OTHER way here, by up")
    print("     to 17x, with nothing done to the shield. A within-session")
    print("     fall is therefore not on its own evidence of relaxation.")


def print_spread(rows):
    v = np.array([r[ax]["v005"] for r in rows for ax in ("Z", "Y")])
    w = np.array([r[ax]["white"] for r in rows for ax in ("Z", "Y")])
    print(f"\nall {len(v)} axis-records, 0.05 Hz: {v.min():.0f}-{v.max():.0f} "
          f"fT/rtHz, geometric mean {np.exp(np.mean(np.log(v))):.0f}, "
          f"CV(log) {np.std(np.log(v)):.2f}")
    print(f"  (measured run-to-run CV is {RUN_CV}, so the whole 16 min-to-"
          f"90 h span is inside the scatter of nominally identical runs)")
    print(f"white band 30-45 Hz: {w.min():.1f}-{w.max():.1f} fT/rtHz "
          f"across every record")


def plot(rows):
    style()
    fig, axes = plt.subplots(1, 3, figsize=(14.0, 4.5))

    # (a) the requirement-facing number vs time
    ax = axes[0]
    ax.axhspan(3e3, 8e3, color="#E8E2D4", alpha=0.6, zorder=0)
    ax.text(11, 3.35e3, "plateau band 3–8k", fontsize=9, color="#8A8064")
    for wind in ("N=16", "N=8", "N=4"):
        sel = [r for r in rows if r["wind"] == wind]
        t = [r["t_min"] / 60.0 for r in sel]
        ax.plot(t, [r["Z"]["v005"] for r in sel], "o-", color=COLOR[wind],
                lw=1.3, ms=6, label=f"{wind}  {DOSE[wind]}  (Z)")
        ax.plot(t, [r["Y"]["v005"] for r in sel], "s--", color=COLOR[wind],
                lw=1.0, ms=5, mfc="none", alpha=0.8)
    ax.errorbar([0.27], [900.0], yerr=[[900.0 * (1 - np.exp(-SIGMA_LN))],
                                       [900.0 * (np.exp(SIGMA_LN) - 1)]],
                fmt="none", ecolor="#33373B", capsize=3, lw=1.2)
    ax.text(0.33, 900.0, "run-to-run\n$\\sigma_{\\ln}$ = %.2f" % SIGMA_LN,
            fontsize=8.5, va="center")
    ax.axhline(TARGET_FT, color="#4E7B52", lw=1.0, ls=":")
    ax.text(0.24, TARGET_FT * 1.3, "lattice eEDM target 50 fT/√Hz",
            fontsize=9, color="#4E7B52")
    ax.set_xscale("log")
    ax.set_yscale("log")
    ax.set_xlim(0.2, 200)
    ax.set_ylim(30, 6e4)
    ax.set_xlabel("time since the taper ended (h)")
    ax.set_ylabel("ASD at 0.05 Hz (fT/$\\sqrt{\\mathrm{Hz}}$)")
    ax.set_title("(a)  0.05 Hz: flat, 16 min to 90 h")
    ax.legend(loc="upper left", fontsize=8.5,
              title="filled ● Z    open □ dashed  Y",
              title_fontsize=8.5, alignment="left")

    # (b) the sub-0.005 Hz content, where the records do move
    ax = axes[1]
    for wind in ("N=16", "N=8", "N=4"):
        sel = [r for r in rows if r["wind"] == wind]
        t = [r["t_min"] / 60.0 for r in sel]
        ax.plot(t, [r["Z"]["ptp"] for r in sel], "o-", color=COLOR[wind],
                lw=1.3, ms=6, label=f"{wind}  (Z)")
        ax.plot(t, [r["Y"]["ptp"] for r in sel], "s--", color=COLOR[wind],
                lw=1.0, ms=5, mfc="none", alpha=0.8)
    i, j = CONTROL
    ax.annotate("", xy=(rows[j]["t_min"] / 60.0, rows[j]["Z"]["ptp"]),
                xytext=(rows[i]["t_min"] / 60.0, rows[i]["Z"]["ptp"]),
                arrowprops=dict(arrowstyle="->", color="#33373B", lw=1.6))
    ax.annotate("control: no degauss,\n53 min apart — rises",
                xy=(24.0, 75.0), xytext=(1.6, 430.0), fontsize=8.5,
                color="#33373B", ha="left",
                arrowprops=dict(arrowstyle="-", color="#9AA0A6", lw=0.8))
    ax.set_xscale("log")
    ax.set_yscale("log")
    ax.set_xlim(0.2, 200)
    ax.set_ylim(18, 700)
    ax.set_xlabel("time since the taper ended (h)")
    ax.set_ylabel("peak-to-peak of the 900 s record (pT)")
    ax.set_title("(b)  sub-0.005 Hz content")
    ax.legend(loc="lower left", fontsize=8.5,
              title="filled ● Z    open □ dashed  Y",
              title_fontsize=8.5, alignment="left")

    # (c) the two N=4 spectra themselves
    ax = axes[2]
    sel = [r for r in rows if r["wind"] == "N=4"]
    for r, shade, lw in zip(sel, ("#9BBFA0", "#2E5B36"), (1.4, 1.8)):
        ax.loglog(r["Z"]["f"], r["Z"]["a"] * 1e3, color=shade, lw=lw,
                  label=f"Z, t0+{r['t_min']:.0f} min")
        ax.loglog(r["Y"]["f"], r["Y"]["a"] * 1e3, color=shade, lw=lw * 0.6,
                  ls="--", alpha=0.85, label=f"Y, t0+{r['t_min']:.0f} min")
    ax.axhspan(3e3, 8e3, color="#E8E2D4", alpha=0.6, zorder=0)
    ax.axhline(TARGET_FT, color="#4E7B52", lw=1.0, ls=":")
    ax.axvline(0.05, color="#B0B4B9", lw=0.8, ls=":", zorder=0)
    ax.set_xlim(5e-3, 100)
    ax.set_ylim(5, 3e5)
    ax.set_xlabel("frequency (Hz)")
    ax.set_ylabel("ASD (fT/$\\sqrt{\\mathrm{Hz}}$)")
    ax.set_title("(c)  N=4, 18 A·t: the two records")
    ax.legend(loc="upper right", fontsize=8.5, ncol=2, columnspacing=1.0)

    fig.suptitle("MS-1L relaxation after a degauss cycle — three windings, "
                 "ten records", fontweight="bold")
    fig.tight_layout()
    out = os.path.join(HERE, "fig_relax_all.png")
    fig.savefig(out)
    print(f"\nsaved {os.path.relpath(out, REPO)}")


def main():
    rows = load()
    print_table(rows)
    print_pairs(rows)
    print_spread(rows)
    plot(rows)


if __name__ == "__main__":
    main()
