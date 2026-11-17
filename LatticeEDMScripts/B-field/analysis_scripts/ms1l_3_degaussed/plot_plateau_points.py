#!/usr/bin/env python3
"""Every post-degauss plateau point, shown rather than summarised.

Claim C8 says the plateau is not remnant magnetisation, and its
uncertainty column says the run-to-run scatter (CV about 0.29) exceeds
what the estimator alone would give (0.18). That is an assertion in a
table. Ayres et al. (2022) put the individual shielding-factor
measurements in a histogram and attributed the excess spread to the
environment changing during the campaign; with ten points the
honest version of that is to draw all of them (REPORT_PLAN §18.11).

Each point carries the chi-squared interval of its own Welch estimate,
so the reader can see directly whether the points disagree by more than
their error bars. The central value is the mean with a bootstrap
interval, the robust pair Kara et al. (2012) use when a distribution is
not normal. Ten runs is enough for the interval to mean something,
which it would not have been at the five the claim ledger assumed.

Channels are checked against their run's recorded flags before use: a
railed channel is dropped with its reason printed, never plotted
quietly (see analysis/run_audit.py).

Run from the repo root:
    python analysis/ms1l_3_degaussed/plot_plateau_points.py
"""

import glob
import json
import os

import numpy as np
import matplotlib
matplotlib.use("Agg")
import matplotlib.pyplot as plt
from scipy import signal as sg
from scipy import stats

HERE = os.path.dirname(os.path.abspath(__file__))
import sys as _sys  # analysis_scripts/ on sys.path for _paths
_sys.path.insert(0, os.path.dirname(os.path.dirname(os.path.abspath(__file__))))
from _paths import DATA_ROOT, PROJECT_DIR, ANALYSIS_DIR, data, data_join, rel_data  # noqa: E402,F401
REPO = PROJECT_DIR
RUNS = data_join("result", "ms1l_3_degaussed")

F0 = 0.05              # the eEDM reversal band
SEG_S = 200.0          # the window convention (REPORT_PLAN §11.5)
WIN_S = 900.0          # every point is read over its first 900 s, so a
                       # 1800 s run is one point of the same kind
AXIS = "Z"
RAIL_LIMIT = 0.001
MIN_SECONDS = 600.0    # below this the 200 s window leaves too few
                       # segments for the interval to mean anything
TARGET_FT = 50.0       # first-generation requirement (Lim 2022; §19.2)
C_PT, C_INK, C_MUTE, C_BAND = "#2E6E8E", "#33373B", "#7A7F85", "#F5E9D0"


def asd_at(x, fs, f0, seg_s):
    """0.05 Hz ASD in fT/rtHz, and the segment count behind it."""
    n = int(seg_s * fs)
    f, P = sg.welch(x, fs, nperseg=n, noverlap=n // 2, window="hann",
                    detrend="linear")
    navg = 2 * (x.size // n) - 1
    return float(np.sqrt(P[int(np.argmin(abs(f - f0)))])) * 1000.0, navg


def chi2_band(value, navg):
    """95 % interval on an ASD from nu = 2*navg degrees of freedom."""
    nu = 2 * navg
    lo = value * np.sqrt(nu / stats.chi2.ppf(0.975, nu))
    hi = value * np.sqrt(nu / stats.chi2.ppf(0.025, nu))
    return lo, hi


# The ten runs the report's plateau statistic is computed from, and the
# same ten that plot_relax_all.py lists by hand. Naming them is not
# belt-and-braces: this program discovers its inputs by globbing the
# campaign folder, so a run added later that happens to pass every gate
# would join the average silently and move a published number (adding
# relax0803_n4_t94m alone carries the mean from 6508 to 6585).
# A run outside this set is skipped and SAID, so the decision to admit
# it stays a decision.
CANONICAL = {
    "postdegauss0729_900s", "relax0729_t80m_900s", "relax0729_t104m_900s",
    "relax0730_t25h_1800s", "relax0730_t26h_900s",
    "postdg0730_n8_900s", "postdg0730_n8_2nd_900s", "relax0803_t90h_900s",
    "postdg0803_n4_900s", "relax0803_n4_t48m_900s",
}


def collect():
    points, skipped = [], []
    for run in sorted(glob.glob(os.path.join(RUNS, "*"))):
        name = os.path.basename(run)
        if name.startswith("void") or "0817" in name:
            why = ("void / excluded (do not use in plateau)"
                   if name.startswith("void")
                   else "08-17 8A look -- compared in plot_postdg0817.py, "
                        "not folded into the n=10 plateau")
            skipped.append((name, why))
            continue
        metas = glob.glob(os.path.join(run, "*.meta.json"))
        chans = glob.glob(os.path.join(run, f"*_{AXIS}_pT.txt"))
        if not metas or not chans:
            continue
        with open(metas[0], encoding="utf-8") as fh:
            meta = json.load(fh)
        if not meta.get("cal_factors"):
            skipped.append((name, "no cal_factors (scale untrusted)"))
            continue
        rail = (meta.get("rail_fraction") or {}).get(AXIS.lower(), 0.0)
        if rail > RAIL_LIMIT:
            skipped.append((os.path.basename(run), f"{AXIS} railed {rail:.3f}"))
            continue
        seconds = meta.get("duration_s") or 0.0
        if seconds < MIN_SECONDS:
            skipped.append((os.path.basename(run),
                            f"{seconds:.0f} s is under the {MIN_SECONDS:.0f} s "
                            f"floor for a {SEG_S:.0f} s window"))
            continue
        if name not in CANONICAL:
            skipped.append((name, "usable, but not in the ten runs the "
                                  "report's plateau is computed from -- "
                                  "admit it deliberately or not at all"))
            continue
        fs = float(meta["fs_hz"])
        # The FIRST 900 s, the same window plot_relax_all.py takes.
        # Reading a 1800 s run whole gave it 17 averages against
        # everyone else's 7 -- a tighter error bar on a different
        # quantity, and 6875 where the canonical window says 6298
        # (2026-08-19 analysis review, H1).
        x = np.loadtxt(chans[0])[:int(WIN_S * fs)]
        value, navg = asd_at(x, fs, F0, SEG_S)
        points.append((os.path.basename(run), value, navg,
                       x.size / fs, meta.get("start_iso", "")[:10]))
    return points, skipped


def excess_scatter(vals, navgs):
    """Is the run-to-run spread larger than the estimator alone allows?

    A Welch ASD from nu = 2*navg degrees of freedom has a fractional
    spread of about sqrt(1/(2*nu)); the runs also differ physically. The
    ratio of observed to expected variance is a chi-squared statistic on
    n-1 degrees of freedom, which turns "the points look scattered" into
    a number that can be wrong.
    """
    cv_obs = vals.std(ddof=1) / vals.mean()
    cv_exp = float(np.mean([np.sqrt(1.0 / (2 * 2 * n)) for n in navgs]))
    stat = (cv_obs / cv_exp) ** 2 * (len(vals) - 1)
    p = float(stats.chi2.sf(stat, len(vals) - 1))
    return cv_obs, cv_exp, p


def bootstrap_trimmed(vals, n=5000, trim=0.05, seed=0):
    """Centre and its bootstrap interval.

    NOTE the trim is nominal: trim_mean drops int(n*trim) from each
    end, which for n=10 at 5 percent is zero. The number this
    returns is the arithmetic mean and is labelled as one. Ten runs
    are too few to trim -- 10 percent would drop one end point and
    move the centre by 5 fT, which is not outlier protection
    either (2026-08-19 analysis review, H2).
    """
    rng = np.random.default_rng(seed)
    draws = rng.choice(vals, size=(n, len(vals)), replace=True)
    means = np.array([stats.trim_mean(d, trim) for d in draws])
    return stats.trim_mean(vals, trim), np.percentile(means, [2.5, 97.5])


def main():
    points, skipped = collect()
    for name, why in skipped:
        print(f"skipped {name}: {why}")
    if len(points) < 2:
        print("not enough usable runs")
        return 1
    vals = np.array([p[1] for p in points])
    centre, (blo, bhi) = bootstrap_trimmed(vals)
    cv, cv_exp, pval = excess_scatter(vals, [p[2] for p in points])
    print(f"\n{len(vals)} runs, {AXIS} at {F0} Hz, {SEG_S:.0f} s segments")
    for name, v, navg, dur, day in points:
        lo, hi = chi2_band(v, navg)
        print(f"  {day}  {name:28s} {v:7.0f}  [{lo:6.0f},{hi:6.0f}]  "
              f"n_avg={navg}")
    print(f"  mean {centre:.0f}, bootstrap 95 % [{blo:.0f},{bhi:.0f}]")
    print(f"  CV observed {cv:.2f} against {cv_exp:.2f} expected from the "
          f"estimator alone: p = {pval:.3f}")

    plt.rcParams.update({"figure.dpi": 150, "font.size": 11,
                         "axes.spines.top": False, "axes.spines.right": False,
                         "text.color": C_INK, "axes.labelcolor": C_INK,
                         "legend.frameon": False})
    fig, ax = plt.subplots(figsize=(9, 4.6))
    y = np.arange(len(points))
    err = np.array([chi2_band(v, n) for _, v, n, _, _ in points]).T
    ax.axvspan(blo, bhi, color=C_BAND, zorder=0,
               label="bootstrap 95 % on the mean")
    ax.axvline(centre, color=C_MUTE, lw=1.0, ls="--")
    ax.errorbar(vals, y, xerr=[vals - err[0], err[1] - vals], fmt="o",
                color=C_PT, capsize=3, lw=1.2, zorder=3,
                label="run, with its own $\\chi^2$ 95 % interval")
    ax.axvline(TARGET_FT, color="#C0504D", lw=1.2)
    ax.text(TARGET_FT * 1.15, len(points) / 2.0 - 0.5,
            "first-generation\nrequirement\n(Lim 2022)",
            color="#C0504D", fontsize=9, va="center")
    ax.set_yticks(y)
    ax.set_yticklabels([f"{d}  {n}" for n, _, _, _, d in points], fontsize=8.5)
    ax.set_xscale("log")
    ax.set_xlim(TARGET_FT * 0.6, max(err[1]) * 1.6)
    ax.set_xlabel(f"{AXIS} ASD at {F0} Hz  [fT/$\\sqrt{{\\rm Hz}}$]")
    ax.grid(True, axis="x", which="both", alpha=0.4)
    ax.legend(loc="upper left", fontsize=9,
              bbox_to_anchor=(0.13, 0.99))
    ax.set_title("post-degauss plateau: every run, with its error bar",
                 loc="left", fontweight="bold")
    ax.annotate(f"CV {cv:.2f} across {len(points)} runs against {cv_exp:.2f} "
                f"from the estimator\nalone (p = {pval:.3f}): the runs differ "
                f"by more than Welch\nexplains. What that extra is remains "
                f"open -- these mix\nsettling times, and settling is one "
                f"candidate (claim C8).",
                xy=(0.13, 0.06), xycoords="axes fraction", fontsize=9,
                color=C_INK,
                bbox=dict(boxstyle="round,pad=0.25", facecolor="white",
                          edgecolor="none", alpha=0.92))
    fig.tight_layout()
    out = os.path.join(HERE, "fig_plateau_points.png")
    fig.savefig(out, bbox_inches="tight")
    print(f"wrote {os.path.relpath(out, REPO)}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
