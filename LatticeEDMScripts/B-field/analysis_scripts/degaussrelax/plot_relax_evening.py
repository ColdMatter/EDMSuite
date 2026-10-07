#!/usr/bin/env python3
"""One degauss_relax evening, read straight out of its own run directory.

    python analysis/degaussrelax/plot_relax_evening.py \
        result/degaussrelax/rx_20260817_153251

plot_relax_all.py is the canonical cross-campaign view and its run list
is curated by hand. This one takes a single evening as the orchestrator
left it -- point directories, session json, t0 and all -- so a series
can be read the morning after without editing any script.

Every statistic is the same as plot_relax_all.py's: the FIRST 900 s of
each record, a spliced Welch (200 s segments below 1 Hz, 20 s above,
linear detrend), Z and Y reported apart. Numbers therefore sit beside
that script's table without conversion.

What the printed table is for, in order of what it can carry:
  - the 0.05 Hz ASD is the requirement-facing number (target 50 fT/rtHz);
  - |drift| and peak-to-peak are dominated by content below ~0.005 Hz,
    which the ASD window cannot resolve, and that is where the
    after-effect has shown up in every campaign so far;
  - the white band (30-45 Hz) should NOT move within an evening. If it
    does, suspect the sensor or the wiring, not the shield.

Two things this cannot separate, and the table prints what it needs to
make them visible rather than hiding them:
  - every point preceded by its own Field Zero + Calibrate carries the
    sensor's settling along with the shield's, so the per-point cal
    factors are printed. A factor that steps mid-series means the
    sensor was re-zeroed there (a standby nap), and the points either
    side are not a clean pair;
  - a railed or flagged point is still printed, with its exit code, so
    a bad channel is visible rather than silently averaged.

Writes fig_<run>.png next to this script.
"""

import argparse
import glob
import json
import os
import sys

import numpy as np
from scipy.signal import welch

import matplotlib
matplotlib.use("Agg")
import matplotlib.pyplot as plt

HERE = os.path.dirname(os.path.abspath(__file__))
FS_DEFAULT = 1200.0
WIN_S = 900.0
TARGET_FT = 50.0
# run-to-run scatter measured in the 08-01 audit: CV ~ 0.29 across
# nominally identical runs, so a ratio of two runs carries this
SIGMA_LN = 0.29 * np.sqrt(2.0)


def load_series(path, n):
    """The first n samples of a per-channel pT file.

    np.loadtxt on a 1.08 M line file is slow enough to matter when a
    morning's reading is ten of them; splitting the text once and
    handing numpy a flat list is the same numbers in a fraction of the
    time. np.loadtxt drops the recorder's four '#' provenance lines for
    free, so dropping them here is not optional -- doing it by prefix
    rather than by count also survives a header that grows.
    """
    with open(path, encoding="utf-8") as fh:
        body = [ln for ln in fh.read().split("\n") if ln[:1] not in ("#", "")]
    return np.array(body[:n], dtype=np.float64)


import sys as _sys  # analysis_scripts/ on sys.path for _paths
_sys.path.insert(0, os.path.dirname(os.path.dirname(os.path.abspath(__file__))))
import _paths  # noqa: E402,F401  (puts B-field on sys.path)
from bfield.analysis.spectra import spliced_asd  # noqa: E402 -- was an identical local copy


def measure(x, fs):
    f, a = spliced_asd(x, fs)
    t = np.arange(len(x)) / fs / 60.0
    return dict(f=f, a=a, x=x,
                v005=a[np.argmin(abs(f - 0.05))] * 1e3,
                v01=a[np.argmin(abs(f - 0.01))] * 1e3,
                mid=np.median(a[(f >= 0.1) & (f <= 1)]) * 1e3,
                white=np.median(a[(f >= 30) & (f <= 45)]) * 1e3,
                drift=abs(np.polyfit(t, x, 1)[0]), ptp=float(np.ptp(x)))


def read_points(run_dir, axes):
    """Every point of the evening, in schedule order, with its meta."""
    doc_path = os.path.join(run_dir, "degauss_relax_session.json")
    entries = []
    if os.path.exists(doc_path):
        with open(doc_path, encoding="utf-8") as fh:
            entries = (json.load(fh).get("points") or [])
    if not entries:      # a run stopped before it wrote its verdicts
        entries = [{"dir_name": os.path.basename(d), "t_sched_min": None,
                    "exit_code": None}
                   for d in sorted(glob.glob(os.path.join(run_dir, "p[0-9]*")))
                   if os.path.isdir(d)]
    points = []
    for e in entries:
        pdir = os.path.join(run_dir, e["dir_name"])
        metas = sorted(glob.glob(os.path.join(pdir, "*", "*.meta.json")))
        if not metas:
            print(f"[skip] {e['dir_name']}: no recorder output")
            continue
        with open(metas[0], encoding="utf-8") as fh:
            meta = json.load(fh)
        base = metas[0][: -len(".meta.json")]
        fs = float(meta.get("fs_hz") or FS_DEFAULT)
        n = int(WIN_S * fs)
        rec = {"name": e["dir_name"], "t": e.get("t_actual_min")
               or e.get("t_sched_min"), "exit": e.get("exit_code"),
               "cal": meta.get("cal_factors") or {},
               "rail": meta.get("rail_fraction") or {}, "fs": fs}
        for ax in axes:
            f = f"{base}_{ax.upper()}_pT.txt"
            rec[ax] = measure(load_series(f, n), fs) if os.path.exists(f) \
                else None
        points.append(rec)
    return points


def print_table(points, axes):
    for ax in axes:
        if not any(p[ax] for p in points):
            continue
        print(f"\n== {ax.upper()} axis  (first {WIN_S:.0f} s; drift pT/min, "
              f"ptp pT, ASD fT/rtHz)")
        hdr = (f"{'point':<12}{'t-t0':>8}{'cal':>7}{'exit':>6}{'|drift|':>9}"
               f"{'ptp':>8}{'0.01Hz':>9}{'0.05Hz':>9}{'0.1-1Hz':>9}"
               f"{'white':>7}")
        print(hdr)
        print("-" * len(hdr))
        for p in points:
            s = p[ax]
            if not s:
                continue
            t = f"{p['t']:.0f} m" if p["t"] is not None else "?"
            cal = p["cal"].get(ax, "-")
            rail = p["rail"].get(ax, 0.0) or 0.0
            flag = "  RAILED" if rail > 0.001 else ""
            print(f"{p['name']:<12}{t:>8}{str(cal):>7}{str(p['exit']):>6}"
                  f"{s['drift']:9.2f}{s['ptp']:8.1f}{s['v01']:9.0f}"
                  f"{s['v005']:9.0f}{s['mid']:9.0f}{s['white']:7.1f}{flag}")

    cals = [str(p["cal"].get(axes[0], "")) for p in points]
    if len(set(cals)) > 1:
        print("\n[note] the calibration factor is not constant across this "
              "evening: the sensor was re-zeroed mid-series (a standby nap), "
              "so its own settling travels with the shield's. --no-sleep "
              "keeps one Field Zero for the whole schedule.")

    first = next((p for p in points if p[axes[0]]), None)
    last = next((p for p in reversed(points) if p[axes[0]]), None)
    if first is None or last is None or first is last:
        return
    print(f"\n== first point -> last, against the run-to-run scatter "
          f"(sigma_ln = {SIGMA_LN:.2f})")
    for key, name in (("drift", "|drift|"), ("ptp", "ptp"),
                      ("v01", "0.01 Hz"), ("v005", "0.05 Hz"),
                      ("mid", "0.1-1 Hz"), ("white", "white")):
        bits = []
        for ax in axes:
            if not (first[ax] and last[ax]):
                continue
            ratio = last[ax][key] / first[ax][key]
            bits.append(f"{ax.upper()} x{ratio:.2f} "
                        f"({np.log(ratio) / SIGMA_LN:+.1f} sigma)")
        print(f"  {name:<9} " + "   ".join(bits))
    print("  A ratio inside about +-2 sigma is not a change this archive "
          "can resolve.")


def make_figure(points, run_dir, axis):
    usable = [p for p in points if p[axis]]
    if not usable:
        return None
    plt.rcParams.update({"figure.dpi": 150, "font.size": 10.5,
                         "axes.titleweight": "bold",
                         "axes.spines.top": False,
                         "axes.spines.right": False,
                         "axes.edgecolor": "#7A7F85",
                         "text.color": "#33373B", "legend.frameon": False})
    fig, (a1, a2) = plt.subplots(1, 2, figsize=(12.5, 4.6))
    shades = plt.cm.viridis(np.linspace(0.15, 0.9, len(usable)))
    for p, c in zip(usable, shades):
        s = p[axis]
        lab = f"t0+{p['t']:.0f} min" if p["t"] is not None else p["name"]
        t = np.arange(len(s["x"])) / p["fs"] / 60.0
        a1.plot(t, s["x"] - s["x"].mean(), lw=0.5, color=c, label=lab)
        a2.loglog(s["f"], s["a"] * 1e3, lw=1.2, color=c, label=lab)
    a1.set_xlabel("time within the record (min)")
    a1.set_ylabel(f"{axis.upper()} field, mean removed (pT)")
    a1.set_title(f"{os.path.basename(run_dir)} -- {axis.upper()}")
    a1.legend(fontsize=8, ncol=2)
    a2.axhline(TARGET_FT, color="#7A7F85", lw=0.8, ls="--")
    a2.text(60, TARGET_FT * 1.12, f"{TARGET_FT:g} fT/$\\sqrt{{Hz}}$",
            fontsize=8.5, color="#7A7F85")
    a2.set_xlim(3e-3, 200)
    a2.set_xlabel("frequency (Hz)")
    a2.set_ylabel("ASD (fT/$\\sqrt{\\mathrm{Hz}}$)")
    a2.set_title(f"{axis.upper()}-axis ASD, first {WIN_S:.0f} s")
    a2.legend(fontsize=8)
    fig.tight_layout()
    out = os.path.join(HERE, f"fig_{os.path.basename(run_dir)}_{axis}.png")
    fig.savefig(out, bbox_inches="tight")
    return out


def main(argv=None):
    p = argparse.ArgumentParser(description=__doc__.splitlines()[0])
    p.add_argument("run_dir", help="a degauss_relax rx_<stamp> directory")
    p.add_argument("--axes", default="z,y", help="axes to read (default z,y)")
    args = p.parse_args(argv)
    run_dir = os.path.abspath(args.run_dir)
    if not os.path.isdir(run_dir):
        print(f"[error] no such run directory: {run_dir}", file=sys.stderr)
        return 13
    axes = [a.strip().lower() for a in args.axes.split(",") if a.strip()]

    summary = os.path.join(run_dir, "relax_summary.txt")
    if os.path.exists(summary):
        with open(summary, encoding="utf-8") as fh:
            print(fh.readline().rstrip())

    points = read_points(run_dir, axes)
    if not points:
        print(f"[error] no readable points under {run_dir}", file=sys.stderr)
        return 13
    print_table(points, axes)
    for ax in axes:
        out = make_figure(points, run_dir, ax)
        if out:
            print(f"[out] {out}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
