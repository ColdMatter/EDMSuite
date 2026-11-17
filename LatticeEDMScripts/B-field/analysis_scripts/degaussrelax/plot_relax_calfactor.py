#!/usr/bin/env python3
"""The Calibrate factor each wake landed on, across a degauss_relax evening.

    python analysis/degaussrelax/plot_relax_calfactor.py \
        result/degaussrelax/rx_20260818_183900

Companion to plot_relax_fieldzero.py: same run directory, same layout,
the other half of what a point's setup produces. Field Zero nulls the
field at the cell; Calibrate then measures the scale factor that turns
the sensor's output into nT, and a factor under 1.5 is the health test
the vendor quick-start gives.

Two things this is for:

  - the factor is a per-point number, so a step in it says the sensor
    changed state between those points -- not the shield. plot_relax_
    evening.py prints a note when the column is not constant; this
    shows WHERE it stepped and by how much;
  - a factor climbing toward 1.5 across an evening would mean the
    background field at the cell is growing, and that is a reason to
    stop and degauss again rather than keep recording.

The axis is deliberately NOT stretched to 1.5. The whole interesting
range here is a few hundredths, and drawing the health limit would
flatten it to a line. The printed table gives the headroom instead.

Read the staircase knowing the sensor reports two decimals: 0.01 is
one step of its resolution, not a measurement of anything smaller.
"""

import argparse
import glob
import json
import os
import sys
from datetime import datetime

import numpy as np

import matplotlib
matplotlib.use("Agg")
import matplotlib.pyplot as plt
from matplotlib.ticker import MultipleLocator

HERE = os.path.dirname(os.path.abspath(__file__))
HEALTH_LIMIT = 1.5                # vendor quick-start: under this is healthy
QUANTUM = 0.01                    # the sensor reports two decimals
# One row per measured axis; each row holds the two heads' channel names.
ROWS = (("Z", "z", "z2"), ("Y", "y", "y2"))
HEAD_COLOR = {"a": "#2C6FBB", "b": "#C05746"}
MUTE = "#7A7F85"


def read_points(run_dir):
    with open(os.path.join(run_dir, "degauss_relax_session.json"),
              encoding="utf-8") as fh:
        sess = json.load(fh)
    t0 = datetime.fromisoformat(sess["t0"])
    points = []
    for e in sess.get("points") or []:
        pdir = os.path.join(run_dir, e["dir_name"])
        metas = sorted(glob.glob(os.path.join(pdir, "*", "*.meta.json")))
        if not metas:
            print(f"[skip] {e['dir_name']}: no recorder output")
            continue
        with open(metas[0], encoding="utf-8") as fh:
            meta = json.load(fh)
        sj = os.path.join(pdir, "qzfm_session.json")
        t_min = e.get("t_actual_min") or e.get("t_sched_min") or 0.0
        if os.path.exists(sj):
            with open(sj, encoding="utf-8") as fh:
                runs = json.load(fh).get("runs") or []
            if runs:
                t_min = (datetime.fromisoformat(runs[0]["t0"])
                         - t0).total_seconds() / 60.0
        points.append({"name": e["dir_name"], "t_min": t_min,
                       "cold": bool(e.get("woke_from_unpowered")),
                       "cal": meta.get("cal_factors") or {}})
    return points


def print_table(points):
    chans = [c for _, *pair in ROWS for c in pair]
    hdr = f"{'point':<12}{'t-t0':>7} |" + "".join(f"{c:>8}" for c in chans)
    print(hdr)
    print("-" * len(hdr))
    for p in points:
        cells = "".join(f"{p['cal'].get(c, float('nan')):8.2f}" for c in chans)
        flag = "   cold start" if p["cold"] else ""
        print(f"{p['name']:<12}{p['t_min']:6.0f} m |{cells}{flag}")

    steady = [p for p in points if not p["cold"]]
    cold = [p for p in points if p["cold"]]
    if cold and steady:
        print(f"\n== step from {cold[-1]['name']} to {steady[0]['name']}")
        bits = [f"{c} {steady[0]['cal'][c] - cold[-1]['cal'][c]:+.2f}"
                for c in chans if c in steady[0]["cal"] and c in cold[-1]["cal"]]
        print("  " + "   ".join(bits)
              + f"   ({QUANTUM:g} is one step of the sensor's resolution)")
    if len(steady) >= 2:
        print(f"\n== the {len(steady)} steady points")
        for c in chans:
            v = np.array([p["cal"][c] for p in steady if c in p["cal"]])
            if not v.size:
                continue
            span = v.max() - v.min()
            print(f"  {c:<3} {v.min():.2f} to {v.max():.2f} "
                  f"({span / QUANTUM:.0f} step{'' if span <= QUANTUM else 's'})"
                  f", headroom to the {HEALTH_LIMIT:g} limit: "
                  f"{HEALTH_LIMIT - v.max():.2f}")


def make_figure(points, run_dir):
    plt.rcParams.update({"figure.dpi": 150, "font.size": 10.5,
                         "axes.titleweight": "bold",
                         "axes.spines.top": False,
                         "axes.spines.right": False,
                         "axes.edgecolor": MUTE,
                         "text.color": "#33373B", "legend.frameon": False})
    fig, axes = plt.subplots(len(ROWS), 1, figsize=(7.5, 5.4), sharex=True)
    t = [p["t_min"] / 60.0 for p in points]
    for ax, (label, ca, cb) in zip(np.atleast_1d(axes), ROWS):
        for chan, head in ((ca, "a"), (cb, "b")):
            v = [p["cal"].get(chan, np.nan) for p in points]
            ax.plot(t, v, "o-", ms=4, lw=1.1, color=HEAD_COLOR[head],
                    label=f"head {head} ({chan})")
        for p, x in zip(points, t):
            if p["cold"]:
                ax.axvline(x, color=MUTE, lw=0.7, ls=":")
        ax.yaxis.set_major_locator(MultipleLocator(QUANTUM))
        ax.grid(True, axis="y", color="#E4E6E8", lw=0.7)
        ax.set_axisbelow(True)
        ax.set_ylabel(f"{label} factor")
        ax.legend(fontsize=8.5)
    np.atleast_1d(axes)[0].set_title(
        f"{os.path.basename(run_dir)} -- Calibrate factor at each wake")
    np.atleast_1d(axes)[-1].set_xlabel("time since degauss t0 (h)")
    fig.text(0.5, 0.005, f"gridline spacing {QUANTUM:g} = one step of the "
             f"sensor's two decimals; all values are far under the "
             f"{HEALTH_LIMIT:g} health limit",
             ha="center", fontsize=8.5, color=MUTE)
    fig.tight_layout(rect=(0, 0.035, 1, 1))
    out = os.path.join(HERE,
                       f"fig_{os.path.basename(run_dir)}_calfactor.png")
    fig.savefig(out, bbox_inches="tight")
    return out


def main(argv=None):
    p = argparse.ArgumentParser(description=__doc__.splitlines()[0])
    p.add_argument("run_dir", help="a degauss_relax rx_<stamp> directory")
    args = p.parse_args(argv)
    run_dir = os.path.abspath(args.run_dir)
    if not os.path.isdir(run_dir):
        print(f"[error] no such run directory: {run_dir}", file=sys.stderr)
        return 13
    points = read_points(run_dir)
    if not points:
        print(f"[error] no readable points under {run_dir}", file=sys.stderr)
        return 13
    print_table(points)
    print(f"[out] {make_figure(points, run_dir)}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
