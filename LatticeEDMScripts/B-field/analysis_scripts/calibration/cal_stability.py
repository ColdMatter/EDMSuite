#!/usr/bin/env python3
"""Track how each calibration constant moves over time (A4).

Reads the append-only ledger the other calibration tools write and
answers, per constant: how much does it move, on what timescale, is the
movement real or just the quoted measurement noise, and what value should
be used today. Per-event constants (zero_flip) get a distribution instead
of a trend. This is a pure ledger consumer -- it never writes the ledger,
never touches run data.
Writes cal_stability_summary.json and cal_stability_timeline.png.
Offline check: python cal_stability.py --test
"""

from __future__ import annotations

import argparse
import json
import os
import shutil
import sys
from dataclasses import dataclass, field, asdict
from datetime import datetime, timedelta

import numpy as np

CHI2_THRESH_DEFAULT = 1.5
WINDOW_DAYS_DEFAULT = 30
DRIFT_SIG = 3.0
SESSION_GAP_H = 6.0     # zero_flip dates further apart than this = new session
PER_EVENT_KINDS = ("zero_flip",)   # reset at every Field Zero: distribution,
                                   # not trend
GAIN_KINDS = ("gain_tone", "gain_50hz")

import sys as _sys  # analysis_scripts/ on sys.path for _paths
_sys.path.insert(0, os.path.dirname(os.path.dirname(os.path.abspath(__file__))))
from _paths import DATA_ROOT, PROJECT_DIR, ANALYSIS_DIR, data, data_join, rel_data  # noqa: E402,F401
# run refs in the ledger are relative to the data root
REPO_ROOT = DATA_ROOT
DEFAULT_LEDGER = data("calibration", "ledger.jsonl")

C_PT, C_BAND, C_DRIFT, C_MUTE, C_INK = \
    "#2E6E8E", "#E6E8EA", "#C0504D", "#7A7F85", "#33373B"


def require(ok, msg, code=13):
    if not ok:
        print(f"[error] {msg}", file=sys.stderr)
        raise SystemExit(code)


# ---------------------------------------------------------------- ledger

def read_ledger(path):
    """Tolerant read: malformed lines are skipped and counted, never fatal.
    A record needs date, kind, value and unit to mean anything."""
    require(os.path.exists(path), f"ledger not found: {path}")
    records, n_bad = [], 0
    with open(path, encoding="utf-8") as fh:
        for lineno, line in enumerate(fh, start=1):
            if not line.strip():
                continue
            try:
                rec = json.loads(line)
                rec["_epoch"] = datetime.fromisoformat(rec["date"]).timestamp()
                float(rec["value"])
                str(rec["unit"])
                str(rec["kind"])
            except (ValueError, KeyError, TypeError) as e:
                n_bad += 1
                print(f"[warn] ledger line {lineno} skipped: {e}")
                continue
            records.append(rec)
    require(records, f"ledger {path} holds no readable records")
    # corrections: a later record names the date of the entry it supersedes
    superseded_dates = {r["supersedes"] for r in records if r.get("supersedes")}
    n_sup = 0
    for r in records:
        r["_superseded"] = r["date"] in superseded_dates
        n_sup += r["_superseded"]
    return records, n_bad, n_sup


def series_key(rec):
    return (rec["kind"], rec["unit"], rec.get("axis"), rec.get("head"),
            rec.get("gain_mode"), rec.get("channel"))


@dataclass
class Series:
    key: tuple
    records: list = field(default_factory=list)   # kept sorted by date

    @property
    def active(self):
        # "pass": False means the tool that wrote the row printed
        # FAIL or INVALID beside it. Older rows have no field and
        # are kept -- they predate the flag, not the acceptance
        # (2026-08-19 analysis review, H3).
        return [r for r in self.records
                if not r["_superseded"] and r.get("pass") is not False]

    @property
    def failed(self):
        return [r for r in self.records if r.get("pass") is False]


def group_series(records):
    groups = {}
    for rec in sorted(records, key=lambda r: r["date"]):
        groups.setdefault(series_key(rec), Series(series_key(rec))) \
              .records.append(rec)
    return list(groups.values())


# ---------------------------------------------------------------- analysis

@dataclass
class Verdict:
    key: dict
    n: int
    span_days: float
    wmean: float | None
    wmean_err: float | None
    unc_missing: bool
    chi2_dof: float | None
    sigma_ext: float | None
    drift_per_day: float | None
    drift_err: float | None
    drift_sig: float | None
    kind_stats: dict | None
    recommended: dict
    verdict_text: str


def weighted_mean(values, uncs):
    """(wmean, wmean_err, chi2_dof, sigma_ext, unc_missing). Records with
    no uncertainty are excluded from the weighting; if none carries one,
    fall back to an unweighted mean."""
    v = np.asarray(values, float)
    u = np.asarray([np.nan if x is None else x for x in uncs], float)
    ok = np.isfinite(u) & (u > 0)
    if not ok.any():
        m = float(v.mean())
        err = float(v.std(ddof=1) / np.sqrt(v.size)) if v.size > 1 else None
        return m, err, None, None, True
    v, u = v[ok], u[ok]
    w = 1.0 / u ** 2
    m = float(np.sum(w * v) / np.sum(w))
    err = float(1.0 / np.sqrt(np.sum(w)))
    if v.size < 2:
        return m, err, None, None, False
    chi2_dof = float(np.sum(w * (v - m) ** 2) / (v.size - 1))
    sigma_ext = float(np.sqrt(max(0.0, v.var(ddof=1) - np.mean(u ** 2))))
    return m, err, chi2_dof, sigma_ext, False


def weighted_drift(days, values, uncs):
    """Weighted LS of value vs time in days -> (slope, slope_err, sig).

    The slope error is inflated by sqrt(chi2/dof) when the points
    scatter by more than they claim, for the same reason
    weighted_mean inflates its own: (X^T W X)^-1 believes the quoted
    uncertainties, and a series whose real scatter is four times
    them reported 8.7 sigma of drift where the scaled figure is 4.1
    -- both above the 3 sigma threshold in that example, but the
    unscaled one crosses it in cases the scaled one does not
    (2026-08-19 analysis review, H4). Never scaled DOWN: a series
    that fits better than its error bars is not evidence for a
    tighter slope.
    """
    t = np.asarray(days, float)
    v = np.asarray(values, float)
    u = np.asarray([np.nan if x is None else x for x in uncs], float)
    u = np.where(np.isfinite(u) & (u > 0), u, np.nanmedian(u[u > 0])
                 if np.any(u > 0) else 1.0)
    w = 1.0 / u ** 2
    X = np.vstack([t, np.ones_like(t)]).T
    cov = np.linalg.inv(X.T @ (X * w[:, None]))
    beta = cov @ (X.T @ (w * v))
    slope = float(beta[0])
    err = float(np.sqrt(cov[0, 0]))
    dof = v.size - 2
    if dof > 0:
        chi2_dof = float(np.sum(w * (v - X @ beta) ** 2) / dof)
        if chi2_dof > 1.0:
            err *= np.sqrt(chi2_dof)
    return slope, err, float(abs(slope) / err) if err > 0 else None


def per_event_stats(epochs, values):
    v = np.asarray(values, float)
    med = float(np.median(v))
    rstd = float(1.4826 * np.median(np.abs(v - med)))
    out = {"median": med, "robust_std": rstd}
    # cluster into sessions on gaps > SESSION_GAP_H
    t = np.asarray(sorted(epochs), float)
    breaks = np.where(np.diff(t) > SESSION_GAP_H * 3600.0)[0]
    if breaks.size:
        sessions, start = [], 0
        order = np.argsort(epochs)
        vv = v[order]
        for b in (*breaks, t.size - 1):
            sessions.append(vv[start:b + 1])
            start = b + 1
        within = [s.std(ddof=1) for s in sessions if s.size > 1]
        med_by_session = [float(np.median(s)) for s in sessions]
        out["n_sessions"] = len(sessions)
        if within:
            out["within_session_std"] = float(np.mean(within))
        if len(med_by_session) > 1:
            out["across_session_std"] = float(np.std(med_by_session, ddof=1))
    return out


def analyze_series(s, args, now_epoch):
    recs = s.active
    kind, unit = s.key[0], s.key[1]
    key_dict = {"kind": kind, "unit": unit, "axis": s.key[2],
                "head": s.key[3], "gain_mode": s.key[4], "channel": s.key[5]}
    values = [float(r["value"]) for r in recs]
    uncs = [r.get("unc") for r in recs]
    epochs = [r["_epoch"] for r in recs]
    n = len(recs)
    span = (max(epochs) - min(epochs)) / 86400.0 if n > 1 else 0.0
    small = " (small sample)" if n < 5 else ""

    if n == 1:
        return Verdict(key=key_dict, n=1, span_days=0.0,
                       wmean=values[0], wmean_err=uncs[0], unc_missing=False,
                       chi2_dof=None, sigma_ext=None, drift_per_day=None,
                       drift_err=None, drift_sig=None, kind_stats=None,
                       recommended={"value": values[0], "err": uncs[0],
                                    "how": "single"},
                       verdict_text="no stability information yet -- "
                                    "never extrapolate")

    if kind in PER_EVENT_KINDS:
        stats = per_event_stats(epochs, values)
        return Verdict(key=key_dict, n=n, span_days=span,
                       wmean=None, wmean_err=None, unc_missing=False,
                       chi2_dof=None, sigma_ext=None, drift_per_day=None,
                       drift_err=None, drift_sig=None, kind_stats=stats,
                       recommended={"value": None, "err": stats["robust_std"],
                                    "how": "per_event"},
                       verdict_text="resets each Field Zero: session-local "
                                    f"only, do not carry a fixed value{small}")

    wmean, werr, chi2_dof, sigma_ext, unc_missing = weighted_mean(values, uncs)
    stable = chi2_dof is not None and chi2_dof <= args.chi2_thresh
    if chi2_dof is not None and not stable:
        werr = werr * np.sqrt(chi2_dof)   # inflate for the excess scatter
    days = [(e - epochs[0]) / 86400.0 for e in epochs]
    slope, serr, sig = weighted_drift(days, values, uncs)
    drifting = sig is not None and sig >= DRIFT_SIG

    # recommended value: the copy-paste number the other tools use today.
    # stable + a recent point -> weighted mean of the recent points;
    # anything suspicious (real drift, excess scatter, stale series) ->
    # the latest value with the excess scatter attached
    recent = [(v, u) for v, u, e in zip(values, uncs, epochs)
              if (now_epoch - e) / 86400.0 <= args.window_days]
    if stable and not drifting and recent:
        rv, re_, *_ = weighted_mean(*zip(*recent))
        recommended = {"value": rv, "err": re_, "how": "wmean_recent"}
    else:
        ext = sigma_ext or 0.0
        latest_unc = uncs[-1] or 0.0
        recommended = {"value": values[-1],
                       "err": float(np.hypot(latest_unc, ext)) or None,
                       "how": "latest+ext"}

    if unc_missing:
        text = f"no uncertainties in the ledger; unweighted mean only{small}"
    elif drifting:
        text = (f"significant drift ({sig:.1f} sigma); use the latest "
                f"value{small}")
    elif stable:
        text = f"stable, no significant drift{small}"
    else:
        text = (f"excess scatter (chi2/dof {chi2_dof:.1f}); real variation "
                f"beyond the quoted errors{small}")
    return Verdict(key=key_dict, n=n, span_days=span,
                   wmean=wmean, wmean_err=werr, unc_missing=unc_missing,
                   chi2_dof=chi2_dof, sigma_ext=sigma_ext,
                   drift_per_day=slope, drift_err=serr, drift_sig=sig,
                   kind_stats=None, recommended=recommended,
                   verdict_text=text)


def factory_anchor(series_list):
    """gain_tone (absolute) over gain_50hz (factory-relative), matched by
    head+axis: the ratio's mean-minus-1 is the factory digital-calibration
    error, its drift is whether that anchor moves."""
    by = {}
    for s in series_list:
        kind, _, axis, head, *_ = s.key
        if kind in GAIN_KINDS and len(s.active) >= 2:
            by.setdefault((head, axis), {})[kind] = s
    out = []
    for (head, axis), kinds in by.items():
        if len(kinds) < 2:
            continue
        tone, rel = kinds["gain_tone"].active, kinds["gain_50hz"].active
        t_r = np.array([r["_epoch"] for r in rel])
        v_r = np.array([float(r["value"]) for r in rel])
        ratios, days = [], []
        for r in tone:
            interp = float(np.interp(r["_epoch"], t_r, v_r))
            ratios.append(float(r["value"]) / interp)
            days.append(r["_epoch"] / 86400.0)
        ratios = np.asarray(ratios)
        mean = float(ratios.mean())
        err = (float(ratios.std(ddof=1) / np.sqrt(ratios.size))
               if ratios.size > 1 else None)
        sig = None
        if ratios.size > 2:
            _, serr, sig = weighted_drift(
                np.asarray(days) - days[0], ratios, [err] * len(ratios))
        out.append({"head": head, "axis": axis, "ratio_mean": mean,
                    "ratio_err": err, "drift_sig": sig, "n": int(ratios.size)})
    return out


# ---------------------------------------------------------------- report

def key_label(k):
    parts = [k["kind"], k["unit"]]
    parts += [str(v) for v in (k["axis"], k["head"], k["gain_mode"],
                               k["channel"]) if v]
    return " | ".join(parts)


def report_verdict(v):
    print(f"\n[{key_label(v.key)}]  n={v.n} over {v.span_days:.1f} d")
    if v.kind_stats:
        ks = v.kind_stats
        extra = ""
        if "across_session_std" in ks:
            extra = (f"  (within-session {ks.get('within_session_std', 0):.0f}"
                     f" / across-session {ks['across_session_std']:.0f})")
        print(f"  per-event: median {ks['median']:.4g}  robust std "
              f"{ks['robust_std']:.3g}{extra}")
        print(f"  USE:  {v.verdict_text}")
        return
    if v.wmean is not None and v.chi2_dof is not None:
        print(f"  weighted mean {v.wmean:.5g} +/- {v.wmean_err:.2g}    "
              f"chi2/dof {v.chi2_dof:.1f} -> "
              f"{'stable' if v.chi2_dof <= 1.5 else 'excess scatter'}")
    if v.drift_per_day is not None and v.drift_sig is not None:
        week = ""
        if v.key["kind"] in GAIN_KINDS and v.wmean:
            week = f", {v.drift_per_day * 7.0 / v.wmean * 100:+.2f}%/week"
        signif = "significant" if v.drift_sig >= DRIFT_SIG else "not significant"
        print(f"  drift {v.drift_per_day:+.3g} {v.key['unit']}/d "
              f"({v.drift_sig:.1f} sigma{week}) -> {signif}")
    r = v.recommended
    if r["value"] is not None:
        err = f" +/- {r['err']:.2g}" if r["err"] else ""
        print(f"  USE:  {r['value']:.5g}{err}   ({r['how']})")
    print(f"  -> {v.verdict_text}")


# ---------------------------------------------------------------- figure

def make_figure(outdir, series_list, verdicts):
    import matplotlib
    matplotlib.use("Agg")
    import matplotlib.pyplot as plt
    import matplotlib.dates as mdates
    plt.rcParams.update({
        "figure.dpi": 150, "font.size": 10, "axes.titleweight": "bold",
        "axes.spines.top": False, "axes.spines.right": False,
        "axes.edgecolor": C_MUTE, "text.color": C_INK, "legend.frameon": False,
    })
    kinds = sorted({s.key[0] for s in series_list})
    fig, axes = plt.subplots(len(kinds), 1, figsize=(8.0, 2.6 * len(kinds)),
                             squeeze=False)
    vmap = {tuple(v.key.values()): v for v in verdicts}
    for ax, kind in zip(axes[:, 0], kinds):
        active_vals = []
        for s in [s for s in series_list if s.key[0] == kind]:
            active_vals += [float(r["value"]) for r in s.active]
        for s in [s for s in series_list if s.key[0] == kind]:
            v = vmap.get(tuple({"kind": s.key[0], "unit": s.key[1],
                                "axis": s.key[2], "head": s.key[3],
                                "gain_mode": s.key[4],
                                "channel": s.key[5]}.values()))
            dates = [datetime.fromtimestamp(r["_epoch"]) for r in s.active]
            vals = [float(r["value"]) for r in s.active]
            uncs = [r.get("unc") or 0.0 for r in s.active]
            if kind in PER_EVENT_KINDS:
                ax.plot(dates, vals, "o", ms=4, color=C_PT, alpha=0.7)
            else:
                ax.errorbar(dates, vals, yerr=uncs, fmt="o-", ms=4, lw=0.8,
                            capsize=2, color=C_PT)
                if v and v.wmean is not None and v.wmean_err:
                    ax.axhspan(v.wmean - v.wmean_err, v.wmean + v.wmean_err,
                               color=C_BAND, zorder=0)
            if v and v.recommended["value"] is not None:
                ax.axhline(v.recommended["value"], color=C_DRIFT, ls=":",
                           lw=0.9)
            sup = [r for r in s.records if r["_superseded"]]
            if sup:
                ax.plot([datetime.fromtimestamp(r["_epoch"]) for r in sup],
                        [float(r["value"]) for r in sup], "x", ms=6,
                        color=C_MUTE)
        if active_vals:
            # scale to the live points; a wild superseded value may clip
            lo, hi = min(active_vals), max(active_vals)
            pad = 0.15 * (hi - lo) or abs(hi) * 0.01 or 1.0
            ax.set_ylim(lo - pad, hi + pad)
        ax.set_title(kind, fontsize=10, loc="left")
        ax.xaxis.set_major_formatter(mdates.DateFormatter("%m-%d"))
    fig.tight_layout()
    fig.savefig(os.path.join(outdir, "cal_stability_timeline.png"),
                bbox_inches="tight")
    plt.close(fig)


# ---------------------------------------------------------------- pipeline

def run_analysis(args):
    records, n_bad, n_sup = read_ledger(args.ledger)
    lo = datetime.fromisoformat(args.since).timestamp() if args.since else None
    hi = (datetime.fromisoformat(args.until).timestamp() + 86400.0
          if args.until else None)
    records = [r for r in records
               if (lo is None or r["_epoch"] >= lo)
               and (hi is None or r["_epoch"] < hi)]
    require(records, "no records left in the requested time window")
    if args.kind:
        records = [r for r in records if r["kind"] in args.kind]
        require(records, f"no records of kind {args.kind}")

    series_list = group_series(records)
    now_epoch = datetime.now().timestamp()
    verdicts = [analyze_series(s, args, now_epoch) for s in series_list
                if s.active]
    anchors = factory_anchor(series_list)

    print(f"=== cal_stability: {args.ledger} ({len(records)} records, "
          f"{n_bad} bad skipped, {n_sup} superseded) ===")
    for v in verdicts:
        report_verdict(v)
    for a in anchors:
        err = f" +/- {a['ratio_err']:.4f}" if a["ratio_err"] else ""
        print(f"\n[factory-anchor consistency] {a['head']}/{a['axis']}: "
              f"gain_tone/gain_50hz = {a['ratio_mean']:.4f}{err}")
        print(f"  -> factory digital calibration off by "
              f"{(a['ratio_mean'] - 1) * 100:+.2f}%"
              + (f" (drift {a['drift_sig']:.1f} sigma)" if a["drift_sig"]
                 else ""))

    os.makedirs(args.outdir, exist_ok=True)
    summary = {
        "program": "cal_stability.py",
        "generated": datetime.now().isoformat(timespec="seconds"),
        "inputs": {"ledger": args.ledger,
                   "args": {k: v for k, v in vars(args).items()
                            if k != "test"}},
        "results": {
            "n_records": len(records), "n_bad": n_bad, "n_superseded": n_sup,
            "series": [asdict(v) for v in verdicts],
            "factory_anchor": anchors,
        },
        "warnings": [],
        "pass": None,
    }
    path = os.path.join(args.outdir, "cal_stability_summary.json")
    with open(path, "w", newline="\n") as fh:
        json.dump(summary, fh, indent=1)
    print(f"\n[out ] {path}")

    if not args.no_figures:
        make_figure(args.outdir, series_list, verdicts)
        print(f"[out ] {os.path.join(args.outdir, 'cal_stability_timeline.png')}")
    return summary


# ---------------------------------------------------------------- self-test

def write_ledger(path, rows):
    with open(path, "w", newline="\n") as fh:
        for row in rows:
            fh.write((row if isinstance(row, str) else json.dumps(row)) + "\n")


def gain_rows(dates, values, unc, kind="gain_tone", head="H1", axis="z"):
    return [{"date": d, "kind": kind, "value": v, "unc": unc, "unit": "V/nT",
             "tool": "test", "axis": axis, "head": head, "gain_mode": "1x"}
            for d, v in zip(dates, values)]


def iso(days_ago, hour=12):
    d = datetime.now() - timedelta(days=days_ago)
    return d.replace(hour=hour, minute=0, second=0,
                     microsecond=0).isoformat(timespec="seconds")


def targs(tmp, ledger, extra=()):
    return build_parser().parse_args(
        ["--ledger", ledger, "--outdir", os.path.join(tmp, "out"),
         "--no-figures", *extra])


def test_drift_detected(ctx):
    rng = np.random.default_rng(20260707)
    days = [35, 28, 21, 14, 7, 0]
    slope = 2.62 * 0.001 / 7.0            # 0.1 %/week in V/nT per day
    vals = [2.62 + slope * (35 - d) + rng.normal(0, 0.001) for d in days]
    path = os.path.join(ctx["tmp"], "drift.jsonl")
    write_ledger(path, gain_rows([iso(d) for d in days], vals, 0.001))
    s = run_analysis(targs(ctx["tmp"], path))
    v = s["results"]["series"][0]
    assert v["drift_sig"] >= 3.0, f"drift not detected: {v['drift_sig']}"
    assert abs(v["drift_per_day"] - slope) <= 3 * v["drift_err"], v
    assert v["recommended"]["how"] == "latest+ext", v["recommended"]


def test_stable(ctx):
    rng = np.random.default_rng(20260708)
    days = [35, 28, 21, 14, 7, 0]
    vals = [2.62 + rng.normal(0, 0.002) for _ in days]
    path = os.path.join(ctx["tmp"], "stable.jsonl")
    write_ledger(path, gain_rows([iso(d) for d in days], vals, 0.002))
    s = run_analysis(targs(ctx["tmp"], path))
    v = s["results"]["series"][0]
    assert v["chi2_dof"] <= 1.5, f"chi2/dof {v['chi2_dof']}"
    assert "stable" in v["verdict_text"], v["verdict_text"]
    assert abs(v["wmean"] - 2.62) <= 2 * v["wmean_err"], v
    assert v["recommended"]["how"] == "wmean_recent", v["recommended"]


def test_per_event(ctx):
    rng = np.random.default_rng(20260709)
    rows = []
    for session, day in enumerate((10, 5, 1)):
        for k in range(2):
            rows.append({"date": iso(day, hour=9 + 8 * k),
                         "kind": "zero_flip",
                         "value": float(rng.normal(-2100.0, 300.0)),
                         "unc": 2.0, "unit": "pT", "tool": "test",
                         "axis": "z", "head": "H1"})
    path = os.path.join(ctx["tmp"], "event.jsonl")
    write_ledger(path, rows)
    s = run_analysis(targs(ctx["tmp"], path))
    v = s["results"]["series"][0]
    assert v["kind_stats"], "zero_flip did not use the distribution path"
    assert 100.0 <= v["kind_stats"]["robust_std"] <= 500.0, v["kind_stats"]
    assert v["drift_per_day"] is None, "per-event kind must not get a trend"


def test_robustness(ctx):
    good = gain_rows([iso(10), iso(5)], [2.62, 2.62], 0.002)
    superseded = {**gain_rows([iso(20)], [9.99], 0.002)[0]}
    fix = {**gain_rows([iso(19)], [2.62], 0.002)[0],
           "supersedes": superseded["date"]}
    path = os.path.join(ctx["tmp"], "robust.jsonl")
    write_ledger(path, [superseded, fix, *good, "{not json"])
    s = run_analysis(targs(ctx["tmp"], path))
    r = s["results"]
    assert r["n_bad"] == 1 and r["n_superseded"] == 1, r
    v = r["series"][0]
    assert abs(v["wmean"] - 2.62) < 0.01, \
        f"superseded 9.99 leaked into the mean: {v['wmean']}"


def test_factory_anchor(ctx):
    rows = gain_rows([iso(20), iso(10)], [2.618, 2.618], 0.002) \
         + gain_rows([iso(21), iso(9)], [2.624, 2.624], 0.001,
                     kind="gain_50hz")
    path = os.path.join(ctx["tmp"], "anchor.jsonl")
    write_ledger(path, rows)
    s = run_analysis(targs(ctx["tmp"], path))
    a = s["results"]["factory_anchor"]
    assert a, "factory-anchor block missing"
    assert abs(a[0]["ratio_mean"] - 2.618 / 2.624) < 1e-3, a[0]


def test_recommended_stale(ctx):
    rng = np.random.default_rng(20260710)
    days = [200, 150, 100, 80, 60, 50]      # all far outside the window
    vals = [2.62 + rng.normal(0, 0.002) for _ in days]
    path = os.path.join(ctx["tmp"], "stale.jsonl")
    write_ledger(path, gain_rows([iso(d) for d in days], vals, 0.002))
    s = run_analysis(targs(ctx["tmp"], path))
    v = s["results"]["series"][0]
    assert v["recommended"]["how"] == "latest+ext", v["recommended"]


def run_self_test():
    import tempfile
    tmp = tempfile.mkdtemp(prefix="cal_stability_test_")
    print("=== cal_stability --test: synthetic ledgers (temp dir, removed after) ===")
    ctx = {"tmp": tmp}
    try:
        for test in (test_drift_detected, test_stable, test_per_event,
                     test_robustness, test_factory_anchor,
                     test_recommended_stale):
            test(ctx)
            print(f"[test] PASS: {test.__name__}")
        return 0
    except AssertionError as e:
        print(f"[test] FAIL: {e}", file=sys.stderr)
        return 1
    finally:
        shutil.rmtree(tmp, ignore_errors=True)


# ---------------------------------------------------------------- CLI

def build_parser():
    p = argparse.ArgumentParser(
        description=__doc__,
        formatter_class=argparse.RawDescriptionHelpFormatter)
    p.add_argument("--ledger", default=DEFAULT_LEDGER,
                   help="input ledger (read-only)")
    p.add_argument("--kind", action="append", default=None,
                   help="restrict to these kinds (repeatable)")
    p.add_argument("--since", default=None, help="ISO date, inclusive")
    p.add_argument("--until", default=None, help="ISO date, inclusive")
    p.add_argument("--window-days", type=int, default=WINDOW_DAYS_DEFAULT,
                   help="'recent' window for the recommended value")
    p.add_argument("--chi2-thresh", type=float, default=CHI2_THRESH_DEFAULT,
                   help="chi2/dof above this = real variation")
    p.add_argument("--outdir", default=os.path.dirname(os.path.abspath(__file__)),
                   help="where the summary and figure go")
    p.add_argument("--no-figures", action="store_true",
                   help="skip the figure (headless quick check)")
    p.add_argument("--test", action="store_true",
                   help="offline self-test on synthetic ledgers")
    return p


def main(argv=None):
    args = build_parser().parse_args(argv)
    if args.test:
        return run_self_test()
    require(args.window_days > 0, "--window-days must be positive")
    require(args.chi2_thresh > 0, "--chi2-thresh must be positive")
    run_analysis(args)
    return 0


if __name__ == "__main__":
    sys.exit(main())
