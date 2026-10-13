#!/usr/bin/env python3
"""A degauss_relax evening as trend lines, plus Allan deviation.

    python analysis/degaussrelax/plot_relax_trends.py \
        result/degaussrelax/rx_20260818_183900

plot_relax_evening.py draws every point's whole ASD on one pair of
axes, which for a twelve-point night is twelve curves that hide each
other. This reads the same records and asks the narrower question:
pick a few round frequencies, and plot each one against time.

Three figures, from one pass over the data (loading 48 records twice
is the only expensive thing here):

  _trend       ASD at 0.01 / 0.05 / 0.1 / 1 / 10 Hz vs time
  _allan       Allan deviation vs averaging time, per axis
  _allantrend  Allan deviation at tau = 1 / 10 / 100 s vs time

Why Allan deviation and not another ASD: it says which KIND of noise
each band is, by slope. tau^-1/2 is white, flat is flicker, tau^+1 is
a linear ramp. The Y records of 2026-08-18 fall 15-25 pT/min inside
every record, so their Allan curve should rise as tau^+1 and sit on
the dashed |D|tau/sqrt(2) reference drawn on each panel. That is the
check the ASD cannot make: a ramp and a random walk look similar in
an ASD and do not in an Allan plot.

Estimator notes, so the numbers can be compared with other scripts:
  - same spliced Welch as plot_relax_evening.py (200 s segments below
    1 Hz, 20 s above, linear detrend), first 900 s of each record;
  - each trend point is the MEDIAN over f/1.25 .. f*1.25, not one bin.
    That is NOT the campaign's 0.05 Hz: plot_relax_all.py,
    plot_relax_evening.py and plot_plateau_points.py all take the
    nearest single bin, and on postdg0729 the two conventions read
    7855 and 6300 fT/rtHz -- 20 percent apart. Read this table
    against itself, and use plot_relax_evening.py when the number
    has to sit beside a plateau point.
    At 10 Hz that is about 100 bins and the chi-square scatter mostly
    averages out; at 0.01 Hz the window holds a single bin and cannot,
    so the low-frequency lines are noisy by construction;
  - the round frequencies were chosen to miss the instrument's own
    lines (50 Hz and the 923 Hz modulation aliases);
  - Allan deviation is the overlapping estimator, computed on the
    record block-averaged to 100 Hz. For tau a whole multiple of 0.01 s
    the block averages are identical to the full-rate ones. tau stops
    at 180 s = T/5; the tau = 100 s column has about nine independent
    intervals behind it and should be read as an indication.
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
BAND = 1.25                       # trend point = median over f/1.25 .. f*1.25
FREQS = (0.01, 0.05, 0.1, 1.0, 10.0)
TAUS_ROUND = (1.0, 10.0, 100.0)
TAU_MAX_S = WIN_S / 5.0

# An ordinal blue ramp, light -> dark with rising frequency: these five
# series are ordered, not five identities, and an ordered ramp keeps the
# colour-blind pairs apart for free. Steps 250..700 of the reference
# sequential hue; 250 is the lightest allowed on a light surface.
RAMP5 = ("#86b6ef", "#5598e7", "#2a78d6", "#1c5cab", "#0d366b")
RAMP3 = ("#86b6ef", "#2a78d6", "#0d366b")
ACCENT = "#eb6834"                # the cold-start point, wherever it shows
INK, MUTE = "#33373B", "#7A7F85"

AXIS_TITLE = {"z": "Z (head a)", "y": "Y (head a)",
              "z2": "Z (head b)", "y2": "Y (head b)"}


def load_series(path, n):
    with open(path, encoding="utf-8") as fh:
        body = [ln for ln in fh.read().split("\n") if ln[:1] not in ("#", "")]
    return np.array(body[:n], dtype=np.float64)


import sys as _sys  # analysis_scripts/ on sys.path for _paths
_sys.path.insert(0, os.path.dirname(os.path.dirname(os.path.abspath(__file__))))
import _paths  # noqa: E402,F401  (puts B-field on sys.path)
from bfield.analysis.spectra import spliced_asd  # noqa: E402 -- was an identical local copy


def band_median(f, a, f0):
    """The ASD near f0 in fT/rtHz, median over a +/-25 percent window."""
    m = (f >= f0 / BAND) & (f <= f0 * BAND)
    if not m.any():                       # narrower than one bin
        m = np.zeros_like(f, dtype=bool)
        m[np.argmin(abs(f - f0))] = True
    return float(np.median(a[m])) * 1e3


def allan_dev(x, fs, taus):
    """Overlapping Allan deviation of x at each tau, same units as x.

    Block-averaging to 100 Hz first costs nothing for tau >= 0.01 s and
    makes the whole sweep of tau cheap: the block averages a tau-long
    window sees are the same numbers either way.
    """
    dec = max(1, int(round(fs / 100.0)))
    n = (len(x) // dec) * dec
    y = x[:n].reshape(-1, dec).mean(axis=1)
    fs2 = fs / dec
    c = np.concatenate([[0.0], np.cumsum(y)])
    out = []
    for tau in taus:
        m = int(round(tau * fs2))
        if m < 1 or 2 * m + 1 > len(y):
            out.append(np.nan)
            continue
        blocks = (c[m:] - c[:-m]) / m
        d = blocks[m:] - blocks[:-m]
        out.append(float(np.sqrt(0.5 * np.mean(d ** 2))))
    return np.array(out)


def measure(x, fs, taus):
    f, a = spliced_asd(x, fs)
    t = np.arange(len(x)) / fs
    return {"asd": {f0: band_median(f, a, f0) for f0 in FREQS},
            "tau": taus, "adev": allan_dev(x, fs, taus),
            "slope_pT_s": float(np.polyfit(t, x, 1)[0])}


def read_points(run_dir, axes, taus):
    with open(os.path.join(run_dir, "degauss_relax_session.json"),
              encoding="utf-8") as fh:
        sess = json.load(fh)
    points = []
    for e in sess.get("points") or []:
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
        rec = {"name": e["dir_name"], "cold": bool(e.get("woke_from_unpowered")),
               "t_h": (e.get("t_actual_min") or e.get("t_sched_min") or 0) / 60.0,
               "rail": meta.get("rail_fraction") or {}}
        for ax in axes:
            path = f"{base}_{ax.upper()}_pT.txt"
            rec[ax] = measure(load_series(path, n), fs, taus) \
                if os.path.exists(path) else None
        print(f"[read] {rec['name']}")
        points.append(rec)
    return points


def print_table(points, axes):
    for ax in axes:
        if not any(p[ax] for p in points):
            continue
        print(f"\n== {AXIS_TITLE.get(ax, ax.upper())}   "
              "ASD fT/rtHz at f (band median, NOT the campaign's "
              "nearest-bin 0.05 Hz), Allan dev pT at tau")
        hdr = (f"{'point':<11}{'t-t0':>6}|" +
               "".join(f"{f'{f0:g}Hz':>9}" for f0 in FREQS) + " |" +
               "".join(f"{f'{t:g}s':>8}" for t in TAUS_ROUND))
        print(hdr)
        print("-" * len(hdr))
        for p in points:
            s = p[ax]
            if not s:
                continue
            adev = dict(zip(s["tau"], s["adev"]))
            print(f"{p['name']:<11}{p['t_h']:5.1f}h|"
                  + "".join(f"{s['asd'][f0]:9.0f}" for f0 in FREQS) + " |"
                  + "".join(f"{adev[t]:8.1f}" for t in TAUS_ROUND)
                  + ("   cold start" if p["cold"] else ""))
        steady = [p for p in points if not p["cold"] and p[ax]]
        if len(steady) >= 3:
            print("  steady points (cold start left out), median and "
                  "max/min spread:")
            cells = []
            for f0 in FREQS:
                v = np.array([p[ax]["asd"][f0] for p in steady])
                cells.append(f"{np.median(v):9.0f}")
            for t in TAUS_ROUND:
                v = np.array([dict(zip(p[ax]["tau"], p[ax]["adev"]))[t]
                              for p in steady])
                cells.append(f"{np.median(v):8.1f}")
            print(f"{'  median':<11}{'':>6}|" + "".join(cells[:len(FREQS)])
                  + " |" + "".join(cells[len(FREQS):]))
            cells = []
            for f0 in FREQS:
                v = np.array([p[ax]["asd"][f0] for p in steady])
                cells.append(f"{v.max() / v.min():8.1f}x")
            for t in TAUS_ROUND:
                v = np.array([dict(zip(p[ax]["tau"], p[ax]["adev"]))[t]
                              for p in steady])
                cells.append(f"{v.max() / v.min():7.1f}x")
            print(f"{'  spread':<11}{'':>6}|" + "".join(cells[:len(FREQS)])
                  + " |" + "".join(cells[len(FREQS):]))
            tau = np.array(steady[0][ax]["tau"])
            w = (tau >= 10.0) & (tau <= TAU_MAX_S)
            sl = [np.polyfit(np.log10(tau[w]),
                             np.log10(np.array(p[ax]["adev"])[w]), 1)[0]
                  for p in steady]
            print(f"  Allan slope over tau = 10-{TAU_MAX_S:.0f} s: "
                  f"{np.median(sl):+.2f} (spread {np.min(sl):+.2f} to "
                  f"{np.max(sl):+.2f}).  +1 a ramp, +0.5 a random walk, "
                  "0 flicker, -0.5 white.")
            # where the curve bottoms out: averaging longer than this
            # buys nothing back, which is the number an operator wants
            best = [(tau[np.nanargmin(p[ax]["adev"])],
                     np.nanmin(p[ax]["adev"])) for p in steady]
            print(f"  best averaging time: tau = "
                  f"{np.median([b[0] for b in best]):.2f} s at "
                  f"{np.median([b[1] for b in best]):.2f} pT")


def style():
    plt.rcParams.update({"figure.dpi": 150, "font.size": 10,
                         "axes.titleweight": "bold", "axes.titlesize": 10.5,
                         "axes.spines.top": False, "axes.spines.right": False,
                         "axes.edgecolor": MUTE, "axes.labelcolor": INK,
                         "text.color": INK, "xtick.color": MUTE,
                         "ytick.color": MUTE, "legend.frameon": False,
                         "grid.color": "#E4E6E8", "grid.linewidth": 0.7})


def panels(axes, figsize):
    n = len(axes)
    rows = 2 if n > 2 else 1
    cols = int(np.ceil(n / rows))
    fig, axs = plt.subplots(rows, cols, figsize=figsize, sharex=True)
    return fig, np.atleast_1d(axs).ravel()[:n]


def label_right(ap, x, entries):
    """Name each line at its right end, pushed apart so they stay legible.

    Two series that end a decade apart need no help; two that end 10
    percent apart overwrite each other. Separating in pixels rather than
    in data units is what makes one rule work on every panel, so this
    runs after tight_layout has settled the axes size.
    """
    ent = sorted([e for e in entries if np.isfinite(e[0])], key=lambda e: e[0])
    if not ent:
        return
    inv = ap.transData.inverted()
    ys = [ap.transData.transform((x, e[0]))[1] for e in ent]
    for i in range(1, len(ys)):
        ys[i] = max(ys[i], ys[i - 1] + 13.0)
    top = ap.transData.transform((x, ap.get_ylim()[1]))[1]
    over = ys[-1] - (top - 6.0)
    if over > 0:
        ys = [y - over for y in ys]
    for (_, text, color), y in zip(ent, ys):
        ap.text(x, inv.transform((0, y))[1], f" {text}", color=color,
                fontsize=8.5, va="center", fontweight="bold", clip_on=False)


def fig_trend(points, axes, run_dir):
    fig, axs = panels(axes, (11.5, 6.6))
    t = np.array([p["t_h"] for p in points])
    tags = []
    for ap, ax in zip(axs, axes):
        tags.append([])
        for f0, c in zip(FREQS, RAMP5):
            v = np.array([p[ax]["asd"][f0] if p[ax] else np.nan
                          for p in points])
            ap.plot(t, v, "-o", color=c, lw=2, ms=5, mec="white", mew=0.8)
            tags[-1].append((v[-1], f"{f0:g} Hz", c))
        cold = [p["t_h"] for p in points if p["cold"]]
        for x in cold:
            ap.axvline(x, color=MUTE, lw=0.8, ls=":")
        ap.axhline(TARGET_FT, color=MUTE, lw=0.9, ls="--")
        ap.set_yscale("log")
        ap.set_xlim(0, t[-1] * 1.16)
        ap.grid(True, which="major", axis="y")
        ap.set_axisbelow(True)
        ap.set_title(AXIS_TITLE.get(ax, ax.upper()))
    axs[0].text(0.15, TARGET_FT * 1.2, f"{TARGET_FT:g} fT/$\\sqrt{{Hz}}$",
                fontsize=8, color=MUTE)
    for ap in axs[-2:]:
        ap.set_xlabel("time since degauss t0 (h)")
    for ap in axs[::2]:
        ap.set_ylabel("ASD (fT/$\\sqrt{\\mathrm{Hz}}$)")
    fig.suptitle(f"{os.path.basename(run_dir)}  --  ASD at five round "
                 "frequencies, point by point", fontweight="bold", y=0.995)
    if any(p["cold"] for p in points):
        fig.text(0.5, 0.955, "each point is one 900 s record; the dotted line "
                 "marks the cold start, whose sensor had just been powered up",
                 ha="center", fontsize=8.5, color=MUTE)
    fig.tight_layout(rect=(0, 0, 1, 0.945))
    for ap, ent in zip(axs, tags):
        label_right(ap, t[-1], ent)
    out = os.path.join(HERE, f"fig_{os.path.basename(run_dir)}_trend.png")
    fig.savefig(out, bbox_inches="tight")
    plt.close(fig)
    return out


def fig_allan(points, axes, run_dir):
    fig, axs = panels(axes, (11.5, 6.8))
    for ap, ax in zip(axs, axes):
        steady = [p for p in points if not p["cold"] and p[ax]]
        cold = [p for p in points if p["cold"] and p[ax]]
        if not steady:
            continue
        tau = steady[0][ax]["tau"]
        m = np.vstack([p[ax]["adev"] for p in steady])
        ap.fill_between(tau, np.nanmin(m, axis=0), np.nanmax(m, axis=0),
                        color=RAMP5[0], alpha=0.45, lw=0,
                        label=f"{len(steady)} steady points, min-max")
        ap.loglog(tau, np.nanmedian(m, axis=0), color=RAMP5[3], lw=2.2,
                  label="their median")
        for p in cold:
            ap.loglog(tau, p[ax]["adev"], color=ACCENT, lw=1.8, ls="-",
                      label=f"{p['name']} (cold start)")
        lo = min(np.nanmin(m), np.nanmin([p[ax]["adev"] for p in cold])
                  if cold else np.inf)
        hi = max(np.nanmax(m), np.nanmax([p[ax]["adev"] for p in cold])
                 if cold else -np.inf)
        d = np.median([abs(p[ax]["slope_pT_s"]) for p in steady])
        tr = np.array([t for t in tau if t >= 1.0])
        ap.loglog(tr, d * tr / np.sqrt(2), color=MUTE, lw=1.1, ls="--",
                  label=f"a {d * 60:.0f} pT/min ramp alone")
        ap.set_ylim(lo * 0.6, hi * 2.2)
        ap.grid(True, which="major")
        ap.set_axisbelow(True)
        ap.set_title(AXIS_TITLE.get(ax, ax.upper()))
        ap.legend(fontsize=8, loc="upper left")
    for ap in axs[-2:]:
        ap.set_xlabel("averaging time $\\tau$ (s)")
    for ap in axs[::2]:
        ap.set_ylabel("Allan deviation (pT)")
    fig.suptitle(f"{os.path.basename(run_dir)}  --  Allan deviation within "
                 "each 900 s record", fontweight="bold", y=0.99)
    fig.tight_layout(rect=(0, 0, 1, 0.97))
    out = os.path.join(HERE, f"fig_{os.path.basename(run_dir)}_allan.png")
    fig.savefig(out, bbox_inches="tight")
    plt.close(fig)
    return out


def fig_allan_trend(points, axes, run_dir):
    fig, axs = panels(axes, (11.5, 6.6))
    t = np.array([p["t_h"] for p in points])
    tags = []
    for ap, ax in zip(axs, axes):
        tags.append([])
        for tau0, c in zip(TAUS_ROUND, RAMP3):
            v = np.array([dict(zip(p[ax]["tau"], p[ax]["adev"]))[tau0]
                          if p[ax] else np.nan for p in points])
            ap.plot(t, v, "-o", color=c, lw=2, ms=5, mec="white", mew=0.8)
            tags[-1].append((v[-1], f"$\\tau$={tau0:g} s", c))
        for x in [p["t_h"] for p in points if p["cold"]]:
            ap.axvline(x, color=MUTE, lw=0.8, ls=":")
        ap.set_yscale("log")
        ap.set_xlim(0, t[-1] * 1.18)
        ap.grid(True, which="major", axis="y")
        ap.set_axisbelow(True)
        ap.set_title(AXIS_TITLE.get(ax, ax.upper()))
    for ap in axs[-2:]:
        ap.set_xlabel("time since degauss t0 (h)")
    for ap in axs[::2]:
        ap.set_ylabel("Allan deviation (pT)")
    fig.suptitle(f"{os.path.basename(run_dir)}  --  Allan deviation at three "
                 "round averaging times, point by point",
                 fontweight="bold", y=0.99)
    fig.tight_layout(rect=(0, 0, 1, 0.97))
    for ap, ent in zip(axs, tags):
        label_right(ap, t[-1], ent)
    out = os.path.join(HERE, f"fig_{os.path.basename(run_dir)}_allantrend.png")
    fig.savefig(out, bbox_inches="tight")
    plt.close(fig)
    return out


def main(argv=None):
    p = argparse.ArgumentParser(description=__doc__.splitlines()[0])
    p.add_argument("run_dir", help="a degauss_relax rx_<stamp> directory")
    p.add_argument("--axes", default="z,y,z2,y2", help="axes to read")
    args = p.parse_args(argv)
    run_dir = os.path.abspath(args.run_dir)
    if not os.path.isdir(run_dir):
        print(f"[error] no such run directory: {run_dir}", file=sys.stderr)
        return 13
    axes = [a.strip().lower() for a in args.axes.split(",") if a.strip()]

    taus = sorted(set(np.round(np.logspace(-2, np.log10(TAU_MAX_S), 25), 4))
                  | set(TAUS_ROUND))
    points = read_points(run_dir, axes, taus)
    if not points:
        print(f"[error] no readable points under {run_dir}", file=sys.stderr)
        return 13
    print_table(points, axes)
    style()
    for out in (fig_trend(points, axes, run_dir),
                fig_allan(points, axes, run_dir),
                fig_allan_trend(points, axes, run_dir)):
        print(f"[out] {out}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
