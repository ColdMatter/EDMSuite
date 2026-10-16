#!/usr/bin/env python3
"""The interior DC field across a degauss_relax evening, from Field Zero.

    python analysis/degaussrelax/plot_relax_fieldzero.py \
        result/degaussrelax/rx_20260818_183900

Every wake runs its own Field Zero, and the compensation values it
lands on (B0/By/Bz in each record's house CSV, held for the whole
record) are the interior field at the cell at that moment, sign
apart. Read across the points they are an hourly DC trajectory that
the pT records cannot carry, because each record is zeroed at start.

The one comparison this table exists for: a within-record drift that
is the shield genuinely moving must accumulate here point to point
(-20 pT/min in a record is -1200 pT between wakes). One that restarts
at every wake and leaves these values flat is a post-wake transient,
whatever its mechanism, and does not belong to the shield.

Reads nothing but the run directory. Writes fig_<run>_fieldzero.png
next to this script and prints the table. A point whose compensation
moved mid-record (Field Zero re-ran) is flagged NOT-HELD; its row is
the first sample.
"""

import argparse
import csv
import glob
import json
import os
import sys
from datetime import datetime

import numpy as np

import matplotlib
matplotlib.use("Agg")
import matplotlib.pyplot as plt

HERE = os.path.dirname(os.path.abspath(__file__))
COMPS = ("B0", "By", "Bz")


def read_points(run_dir):
    doc = os.path.join(run_dir, "degauss_relax_session.json")
    with open(doc, encoding="utf-8") as fh:
        sess = json.load(fh)
    t0 = datetime.fromisoformat(sess["t0"])
    points = []
    for e in sess.get("points") or []:
        pdir = os.path.join(run_dir, e["dir_name"])
        sj = os.path.join(pdir, "qzfm_session.json")
        if not os.path.exists(sj):
            print(f"[skip] {e['dir_name']}: no qzfm_session.json")
            continue
        with open(sj, encoding="utf-8") as fh:
            runs = json.load(fh).get("runs") or []
        if not runs:
            print(f"[skip] {e['dir_name']}: no runs")
            continue
        rec = {"name": e["dir_name"], "cold": e.get("woke_from_unpowered"),
               "t_min": (datetime.fromisoformat(runs[0]["t0"]) - t0)
               .total_seconds() / 60.0}
        for head in "ab":
            # sorted: two records under one point would otherwise be
            # chosen by filesystem order (2026-08-19 review, M4)
            hits = sorted(glob.glob(
                os.path.join(pdir, "R*", f"house_{head}.csv")))
            if not hits:
                continue
            with open(hits[0], encoding="utf-8") as fh:
                rows = list(csv.DictReader(fh))
            for c in COMPS:
                vals = [float(r[f"{c}_pT"]) for r in rows]
                rec[f"{head}_{c}"] = vals[0]
                if max(vals) - min(vals) > 1e-9:
                    rec["not_held"] = True
        points.append(rec)
    return points


def print_table(points):
    hdr = (f"{'point':<12}{'t-t0':>7} |{'a:B0':>9}{'a:By':>9}{'a:Bz':>9} |"
           f"{'b:B0':>9}{'b:By':>9}{'b:Bz':>9}   (pT)")
    print(hdr)
    print("-" * len(hdr))
    for p in points:
        cells = "".join(f"{p.get(h + '_' + c, float('nan')):9.0f}"
                        for h in "ab" for c in COMPS)
        flag = "   NOT-HELD" if p.get("not_held") else \
               "   cold start" if p.get("cold") else ""
        print(f"{p['name']:<12}{p['t_min']:6.0f} m |{cells[:27]} |"
              f"{cells[27:]}{flag}")
    # steady points only: the cold-started first point carries the
    # sensor's own warm-up in B0 (light shift) and its cal step
    steady = [p for p in points if not p.get("cold")]
    if len(steady) >= 2:
        span_h = (steady[-1]["t_min"] - steady[0]["t_min"]) / 60.0
        print(f"\n== {steady[0]['name']} -> {steady[-1]['name']} "
              f"({span_h:.1f} h, cold-start point left out)")
        for h in "ab":
            bits = []
            for c in COMPS:
                v = [p.get(f"{h}_{c}") for p in steady]
                if None in v:
                    continue
                bits.append(f"{c} {v[-1] - v[0]:+.0f} pT "
                            f"({(v[-1] - v[0]) / span_h:+.0f}/h)")
            if bits:
                print(f"  head {h}:  " + "   ".join(bits))


def make_figure(points, run_dir):
    plt.rcParams.update({"figure.dpi": 150, "font.size": 10.5,
                         "axes.titleweight": "bold",
                         "axes.spines.top": False,
                         "axes.spines.right": False,
                         "axes.edgecolor": "#7A7F85",
                         "text.color": "#33373B", "legend.frameon": False})
    fig, axes = plt.subplots(3, 1, figsize=(7.5, 7.2), sharex=True)
    t = [p["t_min"] / 60.0 for p in points]
    for ax, c in zip(axes, COMPS):
        for h, color in (("a", "#2C6FBB"), ("b", "#C05746")):
            v = [p.get(f"{h}_{c}", np.nan) for p in points]
            ax.plot(t, v, "o-", ms=4, lw=1.1, color=color, label=f"head {h}")
        for p, x in zip(points, t):
            if p.get("cold"):
                ax.axvline(x, color="#7A7F85", lw=0.7, ls=":")
        ax.set_ylabel(f"{c} (pT)")
        ax.legend(fontsize=8.5)
    axes[0].set_title(f"{os.path.basename(run_dir)} -- Field Zero "
                      "compensation at each wake")
    axes[-1].set_xlabel("time since degauss t0 (h)")
    fig.tight_layout()
    out = os.path.join(HERE,
                       f"fig_{os.path.basename(run_dir)}_fieldzero.png")
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
