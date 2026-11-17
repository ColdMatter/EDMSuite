#!/usr/bin/env python3
"""Residual DC field and gradient map inside a closed shield (B2).

One asymmetry governs everything here. A single run's DC is field plus a
head-fixed zero of nT class, so a map of raw DCs is a map of nothing.
Gradients from a MOVED single head are clean -- the same zero cancels in
the position difference. Gradients from DIFFERENT heads (--4head)
do not cancel; they stay offset-contaminated unless both heads have
flip-calibrated zeros in the ledger. Every number this tool prints
carries an absolute / offset-contaminated flag.
Reference revisits detrend the ambient drift; without them, drift aliases
into fake gradients and the tool says by how much.
Writes map_residual_summary.json and figures.
Offline check: python map_residual.py --test
"""

from __future__ import annotations

import argparse
import csv
import glob
import json
import os
import shutil
import sys
from dataclasses import dataclass
from datetime import datetime

import numpy as np

GAIN_MEASURED = 2.624   # V/nT at 1x
SETTLE_S = 500.0
TARGET_GRAD = 60.0      # pT/m Lattice benchmark
# The delay that actually caps a TWO-HEAD gradient. The recorder's meta
# carries a CMRR figure of its own, but that one is the DAQ's
# within-record timing -- mux skew plus settling, a few hundred
# microseconds -- while a gradient built from two heads is limited by
# the delay BETWEEN the heads, measured at 1.4 ms for this pair
# (report/main.tex, Delta tau). Printing the meta number as "the
# ceiling" showed a 65 us instrument where the bench has 1.4 ms
# (2026-08-20 analysis review). Both are printed now, each named.
HEAD_SKEW_S = 1.4e-3


def cmrr_ceiling_db(f_hz, tau_s):
    """Timing-only ceiling on common-mode rejection, in dB.

    Two channels that see the same field a time tau apart cannot reject
    it better than -20 log10(2 pi f tau): the residual is the field's
    own slope over tau. Nothing about the sensors enters, so this is a
    ceiling and not a prediction -- the achieved rejection is below it.
    """
    return -20.0 * float(np.log10(2.0 * np.pi * f_hz * tau_s))
B_SANE_PT = 10000.0     # closed-shield absolute field beyond 10 nT is not real
G_SANE = 100000.0       # pT/m; 100 nT/m likewise
SIM_SEED = 20260707

REPO_ROOT = os.path.dirname(os.path.dirname(
    os.path.dirname(os.path.abspath(__file__))))

C_AX = {"z": "#2E6E8E", "y": "#5A7D3A", "x": "#C0504D"}
C_MUTE, C_INK, C_ACC = "#7A7F85", "#33373B", "#C0504D"


def require(ok, msg, code=13):
    if not ok:
        print(f"[error] {msg}", file=sys.stderr)
        raise SystemExit(code)


# ---------------------------------------------------------------- loading

def find_meta(run_dir):
    metas = glob.glob(os.path.join(run_dir, "*.meta.json"))
    require(len(metas) == 1, f"{run_dir}: expected one meta.json, found {len(metas)}")
    with open(metas[0], encoding="utf-8") as fh:
        return json.load(fh), os.path.basename(metas[0])[:-len(".meta.json")]


def check_run(meta, path, force=False, allow_sim=False):
    if not (meta.get("status") == "ok" and meta.get("exit_code") == 0):
        require(force, f"{path}: status={meta.get('status')} exit_code="
                       f"{meta.get('exit_code')}; --force to analyze anyway")
        print(f"[gate] {path}: overridden status={meta.get('status')}")
    nonfinite = meta.get("n_nonfinite") or {}
    require(all(v == 0 for v in nonfinite.values()),
            f"{path}: non-finite samples: {nonfinite}")
    for key in ("rail_fraction", "qzfm_rail_fraction"):
        rails = {k: v for k, v in (meta.get(key) or {}).items() if v > 0.001}
        require(not rails, f"{path}: {key} above 0.1%: {rails}")
    if meta.get("source") == "simulate":
        require(allow_sim, f"{path}: run is simulated; pass --allow-sim if intended")


def load_axis(run_dir, session, axis, meta, args):
    if args.raw:
        path = os.path.join(run_dir, f"{session}_raw.csv")
        with open(path) as fh:
            fh.readline()
            names = fh.readline().strip().split(",")
        data = np.genfromtxt(path, delimiter=",", skip_header=2)
        return data[:, names.index(f"V_{axis}")] * 1000.0 / args.gain_v_per_nt
    path = os.path.join(run_dir, f"{session}_{axis.upper()}_pT.txt")
    require(os.path.exists(path), f"missing {path}")
    factor = float(meta["cal_v_per_nT_used"]) / args.gain_v_per_nt
    return np.loadtxt(path, comments="#") * factor


def block_stats(x, fs, block_s=60.0):
    n = int(block_s * fs)
    nb = x.size // n
    require(nb >= 3, f"record too short for block stats ({nb} blocks)")
    means = x[:nb * n].reshape(nb, n).mean(axis=1)
    tmin = (np.arange(nb) + 0.5) * block_s / 60.0
    mean = float(means.mean())
    sem = float(means.std(ddof=1) / np.sqrt(nb))
    coef = np.polyfit(tmin, means, 1)
    return mean, sem, float(coef[0])


def read_ledger_zeros(path):
    """Latest zero_flip per (head, axis) from the calibration ledger."""
    zeros = {}
    if not path or not os.path.exists(path):
        return zeros
    with open(path, encoding="utf-8") as fh:
        for line in fh:
            try:
                r = json.loads(line)
            except ValueError:
                continue
            if r.get("kind") == "zero_flip":
                zeros[(r.get("head"), r.get("axis"))] = float(r["value"])
    return zeros


# ---------------------------------------------------------------- survey

@dataclass
class PosRun:
    run_dir: str
    pos_mm: tuple
    axis_map: str
    orientation: int
    ref: bool
    label: str
    head: str
    mid_epoch_s: float
    mean_pt: dict
    sem_pt: dict
    drift_pt_min: dict


def parse_manifest(path):
    rows = []
    with open(path, newline="") as fh:
        lines = [ln for ln in fh if not ln.lstrip().startswith("#")]
    for i, rec in enumerate(csv.DictReader(lines), start=2):
        try:
            rows.append({
                "run_dir": rec["run_dir"].strip(),
                "pos_mm": (float(rec["x_mm"]), float(rec["y_mm"]),
                           float(rec["z_mm"])),
                "axis_map": (rec.get("axis_map") or "").strip(),
                "orientation": int(rec.get("orientation") or 0),
                "ref": (rec.get("ref") or "0").strip() == "1",
                "label": (rec.get("label") or "").strip()})
        except (KeyError, ValueError) as e:
            require(False, f"{path} row {i}: {e}")
    return rows


def load_survey(rows, args):
    posruns, metas = [], []
    for row in rows:
        meta, session = find_meta(row["run_dir"])
        check_run(meta, row["run_dir"], args.force, args.allow_sim)
        metas.append(meta)
        fs = float(meta["fs_hz"])
        dur = float(meta["duration_s"])
        trim = SETTLE_S if dur >= 700.0 else 60.0
        if dur < 700.0:
            print(f"[warn] {row['run_dir']}: only {dur:.0f} s; trimmed 60 s")
        mid = datetime.fromisoformat(meta["start_iso"]).timestamp() + dur / 2.0
        mean, sem, drift = {}, {}, {}
        for axis in meta["axes"]:
            x = load_axis(row["run_dir"], session, axis, meta, args)
            m, s, d = block_stats(x[int(trim * fs):], fs)
            mean[axis], sem[axis], drift[axis] = m, s, d
        posruns.append(PosRun(
            run_dir=row["run_dir"], pos_mm=row["pos_mm"],
            axis_map=row["axis_map"], orientation=row["orientation"],
            ref=row["ref"], label=row["label"],
            head=meta.get("sensor_id", "?"), mid_epoch_s=mid,
            mean_pt=mean, sem_pt=sem, drift_pt_min=drift))
    ref_meta = metas[0]
    for meta, row in zip(metas[1:], rows[1:]):
        for key in ("gain_mode", "vrange"):
            if meta.get(key) != ref_meta.get(key):
                print(f"[note] survey mixes {key}: {row['run_dir']}")
                require(args.force, f"mixed {key} across the survey; "
                                    "--force to map anyway")
    return posruns


def detrend_by_refs(posruns):
    """A line through the reference revisits, per axis, subtracted from
    every mean. The residual scatter of the refs after the line is the
    survey's systematic floor. (A line, not a spline: revisits are sparse
    and an exact interpolant would hide its own scatter.)"""
    refs = [p for p in posruns if p.ref]
    axes = sorted({a for p in posruns for a in p.mean_pt})
    if len(refs) < 2:
        return False, {}
    scatter = {}
    t0 = refs[0].mid_epoch_s
    for axis in axes:
        t = np.array([r.mid_epoch_s - t0 for r in refs]) / 60.0
        v = np.array([r.mean_pt[axis] for r in refs])
        coef = np.polyfit(t, v, 1)
        resid = v - np.polyval(coef, t)
        scatter[axis] = float(resid.std(ddof=1)) if len(refs) > 2 else 0.0
        for p in posruns:
            tm = (p.mid_epoch_s - t0) / 60.0
            p.mean_pt[axis] = p.mean_pt[axis] \
                - (np.polyval(coef, tm) - np.polyval(coef, 0.0))
    return True, scatter


# ---------------------------------------------------------------- fields

def flip_pairs(posruns, tol_mm):
    pairs = []
    zeros = [p for p in posruns if p.orientation == 0 and not p.ref]
    for p180 in [p for p in posruns if p.orientation == 180]:
        for p0 in zeros:
            if all(abs(a - b) <= tol_mm for a, b in
                   zip(p0.pos_mm, p180.pos_mm)):
                pairs.append((p0, p180))
                break
    return pairs


def field_map(posruns, pairs, zeros_ledger, ref_scatter):
    fields = []
    flip_members = {id(p) for pair in pairs for p in pair}
    for p0, p180 in pairs:
        for axis in p0.mean_pt:
            b = (p0.mean_pt[axis] - p180.mean_pt[axis]) / 2.0
            err = 0.5 * float(np.hypot(p0.sem_pt[axis], p180.sem_pt[axis]))
            err = float(np.hypot(err, ref_scatter.get(axis, 0.0)))
            fields.append({"pos_mm": list(p0.pos_mm), "axis": axis,
                           "value_pt": b, "err_pt": err, "absolute": True,
                           "method": "flip"})
    for p in posruns:
        if id(p) in flip_members or p.orientation != 0:
            continue
        for axis in p.mean_pt:
            zero = zeros_ledger.get((p.head, axis))
            value = p.mean_pt[axis] - (zero or 0.0)
            err = float(np.hypot(p.sem_pt[axis], ref_scatter.get(axis, 0.0)))
            fields.append({"pos_mm": list(p.pos_mm), "axis": axis,
                           "value_pt": value, "err_pt": err,
                           "absolute": zero is not None,
                           "method": "ledger-zero" if zero is not None
                                     else "raw-mean"})
    for f in fields:
        if f["absolute"]:
            require(abs(f["value_pt"]) < B_SANE_PT,
                    f"absolute |B|={f['value_pt'] / 1000.0:.2f} nT at "
                    f"{f['pos_mm']} is not a closed-shield residual")
    return fields


# ---------------------------------------------------------------- gradients

COORD = {0: "x", 1: "y", 2: "z"}


def neighbor_gradients(posruns, args, drift_protected, ref_scatter):
    """Moved-head pair gradients: the head zero cancels between two
    positions of the SAME head, so these are clean. Positions in mm are
    converted to metres before any division."""
    grads = []
    pts = [p for p in posruns if p.orientation == 0 and not p.ref]
    for i, p1 in enumerate(pts):
        for p2 in pts[i + 1:]:
            diffs = [abs(a - b) for a, b in zip(p1.pos_mm, p2.pos_mm)]
            moving = [j for j, d in enumerate(diffs)
                      if d > 3.0 * args.pos_tol_mm]
            if len(moving) != 1:
                continue
            j = moving[0]
            d_m = (p2.pos_mm[j] - p1.pos_mm[j]) * 1e-3
            for axis in p1.mean_pt:
                g = (p2.mean_pt[axis] - p1.mean_pt[axis]) / d_m
                stat = float(np.hypot(p1.sem_pt[axis], p2.sem_pt[axis])
                             / abs(d_m))
                syst = float(np.hypot(
                    ref_scatter.get(axis, 0.0) * np.sqrt(2.0) / abs(d_m),
                    abs(g) * args.pos_tol_mm * 1e-3 * np.sqrt(2.0) / abs(d_m)))
                alias = None
                if not drift_protected:
                    dt_min = abs(p2.mid_epoch_s - p1.mid_epoch_s) / 60.0
                    rate = np.median([abs(p.drift_pt_min[axis]) for p in pts])
                    alias = float(rate * dt_min / abs(d_m))
                    syst = float(np.hypot(syst, alias))
                grads.append({
                    "component": f"dB{axis}/d{COORD[j]}",
                    "value_pt_per_m": g, "stat": stat, "syst": syst,
                    "method": "pair", "absolute": True,
                    "drift_protected": drift_protected,
                    "drift_alias_pt_per_m": alias,
                    "pass": bool(abs(g) + np.hypot(stat, syst) < TARGET_GRAD)
                            if abs(g) < G_SANE else None,
                    "between": [list(p1.pos_mm), list(p2.pos_mm)]})
    for g in grads:
        require(abs(g["value_pt_per_m"]) < G_SANE,
                f"{g['component']} = {g['value_pt_per_m'] / 1000.0:.1f} nT/m "
                f"between {g['between']} is not a closed-shield gradient")
    return grads


def linear_fit(posruns, args):
    """Per measured component: B_i(r) = B0_i + sum_j G_ij r_j, fitted only
    over the coordinates the survey actually varies (a 1D bore survey
    constrains one column; fitting the rest would be rank-deficient
    guesswork, so they are reported as 'not surveyed')."""
    pts = [p for p in posruns if p.orientation == 0]
    if len(pts) < 6:
        return None
    R = np.array([p.pos_mm for p in pts]) * 1e-3
    spread = R.std(axis=0)
    cols = [j for j in range(3) if spread[j] > 2.0 * args.pos_tol_mm * 1e-3]
    if not cols:
        return None
    out = {"G_ij_pt_per_m": {}, "G_err": {}, "not_surveyed":
           [COORD[j] for j in range(3) if j not in cols],
           "max_residual_pt": 0.0, "worst_pos_mm": None, "warnings": []}
    X = np.column_stack([np.ones(len(pts)), *[R[:, j] for j in cols]])
    for axis in pts[0].mean_pt:
        b = np.array([p.mean_pt[axis] for p in pts])
        coef, *_ = np.linalg.lstsq(X, b, rcond=None)
        resid = b - X @ coef
        dof = max(len(pts) - X.shape[1], 1)
        cov = float(resid @ resid) / dof * np.linalg.inv(X.T @ X)
        for m, j in enumerate(cols, start=1):
            out["G_ij_pt_per_m"][f"dB{axis}/d{COORD[j]}"] = float(coef[m])
            out["G_err"][f"dB{axis}/d{COORD[j]}"] = float(np.sqrt(cov[m, m]))
        rms = float(np.sqrt(np.mean(resid ** 2)))
        worst = int(np.argmax(np.abs(resid)))
        if abs(resid[worst]) > out["max_residual_pt"]:
            out["max_residual_pt"] = float(abs(resid[worst]))
            out["worst_pos_mm"] = list(pts[worst].pos_mm)
        others = np.delete(resid, worst)
        if others.size and abs(resid[worst]) > 3.0 * np.sqrt(
                np.mean(others ** 2)) and abs(resid[worst]) > 5.0:
            out["warnings"].append(
                f"local source near {list(pts[worst].pos_mm)} mm? "
                f"({axis} residual {resid[worst]:+.1f} pT stands alone)")
    return out


def uniform_cube(fit, bound_pt):
    """Largest cube where the linear model keeps |B - B_center| under the
    bound, using only surveyed gradient components."""
    if not fit or not fit["G_ij_pt_per_m"]:
        return None
    per_axis = {}
    for comp, g in fit["G_ij_pt_per_m"].items():
        axis = comp[2]
        per_axis[axis] = per_axis.get(axis, 0.0) + abs(g)
    worst = max(per_axis.values())
    if worst <= 0:
        return None
    side_m = 2.0 * bound_pt / worst
    return {"side_mm": round(side_m * 1e3, 1), "bound_pt": bound_pt}


def multihead_gradients(run_dirs, zeros_ledger, args):
    grads = []
    for run_dir in run_dirs or []:
        meta, session = find_meta(run_dir)
        check_run(meta, run_dir, args.force, args.allow_sim)
        # the four-head recorder was its own program (qzfm_gradient.py)
        # until 2026-08-10 and is now qzfm_record.py --4head, which stamps
        # the old name in software_legacy. Same instrument, same files, so
        # both are accepted: gating on either name alone would make half
        # the runs unreadable -- the ones before the merge, or the ones
        # after it.
        wrote = {meta.get("software"), meta.get("software_legacy")}
        require("qzfm_gradient.py" in wrote,
                f"{run_dir}: not a four-head run (qzfm_record.py --4head, "
                f"or the older qzfm_gradient.py); software="
                f"{meta.get('software')!r}")
        fs = float(meta["fs_hz"])
        for pair in meta.get("gradients", []):
            a, b, L = pair["a"], pair["b"], float(pair["baseline_m"])
            path = os.path.join(
                run_dir, f"{session}_G_{a.upper()}_{b.upper()}_pT_per_m.txt")
            require(os.path.exists(path), f"missing {path}")
            g = np.loadtxt(path, comments="#")
            trim = SETTLE_S if g.size / fs >= 700.0 else 60.0
            mean, sem, _ = block_stats(g[int(trim * fs):], fs)
            za, zb = zeros_ledger.get((a, None)), zeros_ledger.get((b, None))
            absolute = za is not None and zb is not None
            if absolute:
                mean -= (za - zb) / L
            grads.append({
                "component": f"({a}-{b})/{L:g}m", "value_pt_per_m": mean,
                "stat": sem, "syst": 0.0, "method": "multi-head",
                "absolute": absolute,
                "cmrr_db_at_30hz": pair.get("expected_cmrr_db_at_30hz"),
                "cmrr_head_db_at_30hz": round(
                    cmrr_ceiling_db(30.0, HEAD_SKEW_S), 1),
                "pass": None if not absolute
                        else abs(mean) + sem < TARGET_GRAD})
    return grads


# ---------------------------------------------------------------- report

def report(fields, grads, mh_grads, fit, cube, ref_scatter, protected):
    if ref_scatter:
        parts = ", ".join(f"{a} {s:.1f} pT" for a, s in ref_scatter.items())
        print(f"[detrend] ref scatter after detrend: {parts} "
              "(survey syst floor)")
    elif not protected:
        print("[detrend] NO reference revisits: gradients are "
              "drift-unprotected (alias estimates attached)")
    if fields:
        print("[field ] map (absolute where flip- or ledger-pinned):")
        for f in fields:
            tag = "[absolute]" if f["absolute"] else \
                  "(offset-contaminated, head zero included)"
            print(f"   ({f['pos_mm'][0]:g},{f['pos_mm'][1]:g},"
                  f"{f['pos_mm'][2]:g}) mm  B{f['axis']} "
                  f"{f['value_pt']:+.1f} +/- {f['err_pt']:.1f} pT   {tag}")
    if grads:
        print("[grad  ] moved-head neighbor gradients "
              "[clean, head-zero cancels]:")
        for g in grads:
            unc = np.hypot(g["stat"], g["syst"])
            tag = "" if g["drift_protected"] else \
                  f"  DRIFT-UNPROTECTED (alias ~{g['drift_alias_pt_per_m']:.0f})"
            verdict = "PASS(<60)" if g["pass"] else "FAIL(>=60)"
            print(f"   {g['component']} = {g['value_pt_per_m']:+.0f} +/- "
                  f"{unc:.0f} pT/m   {verdict}{tag}")
    for g in mh_grads:
        tag = "[absolute]" if g["absolute"] else "(offset-contaminated)"
        print(f"[grad  ] multi-head {g['component']}: "
              f"{g['value_pt_per_m']:+.0f} +/- {g['stat']:.0f} pT/m {tag}  "
              f"CMRR ceiling {g['cmrr_head_db_at_30hz']:.1f} dB @30 Hz "
              f"(head-to-head {HEAD_SKEW_S * 1e3:g} ms; the recorder's "
              f"within-record figure is {g['cmrr_db_at_30hz']} dB)")
    if fit:
        comps = fit["G_ij_pt_per_m"]
        if comps:
            worst = max(comps, key=lambda k: abs(comps[k]))
            print(f"[fit   ] linear model: max |G| = {abs(comps[worst]):.0f} "
                  f"+/- {fit['G_err'][worst]:.0f} pT/m ({worst})  "
                  f"{'PASS' if abs(comps[worst]) < TARGET_GRAD else 'FAIL'}")
        print(f"   max residual {fit['max_residual_pt']:.1f} pT"
              + ("" if not fit["warnings"] else ""))
        for w in fit["warnings"]:
            print(f"   [warn] {w}")
        if fit["not_surveyed"]:
            print(f"   (directions not surveyed: "
                  f"{', '.join(fit['not_surveyed'])})")
    if cube:
        print(f"[uniform] |B-Bc| < {cube['bound_pt']:g} pT cube: side "
              f"{cube['side_mm']:g} mm (surveyed directions only)")


# ---------------------------------------------------------------- figures

def make_figures(outdir, posruns, fields, grads):
    import matplotlib
    matplotlib.use("Agg")
    import matplotlib.pyplot as plt
    plt.rcParams.update({
        "figure.dpi": 150, "font.size": 11, "axes.titleweight": "bold",
        "axes.spines.top": False, "axes.spines.right": False,
        "axes.edgecolor": C_MUTE, "text.color": C_INK, "legend.frameon": False,
    })
    pts = [p for p in posruns if p.orientation == 0]
    if pts:
        R = np.array([p.pos_mm for p in pts])
        j = int(np.argmax(R.std(axis=0)))
        fig, ax = plt.subplots(figsize=(7.0, 4.2))
        for axis in sorted(pts[0].mean_pt):
            x = [p.pos_mm[j] for p in pts]
            y = [p.mean_pt[axis] for p in pts]
            e = [p.sem_pt[axis] for p in pts]
            ax.errorbar(x, y, yerr=e, fmt="o", ms=5, capsize=3,
                        color=C_AX.get(axis, C_MUTE), label=f"B{axis}")
        for f in fields:
            if f["absolute"] and f["method"] == "flip":
                ax.plot(f["pos_mm"][j], f["value_pt"], "s", ms=9, mfc="none",
                        color=C_ACC)
        ax.set_xlabel(f"{COORD[j]} [mm]")
        ax.set_ylabel("detrended mean [pT]\n(head zero included)")
        ax.set_title("survey profile (squares: flip-pinned absolute)")
        ax.legend(fontsize=9)
        fig.savefig(os.path.join(outdir, "map_residual_profile.png"),
                    bbox_inches="tight")
        plt.close(fig)

    if grads:
        fig, ax = plt.subplots(figsize=(6.8, 4.0))
        names = [g["component"] for g in grads]
        vals = [g["value_pt_per_m"] for g in grads]
        errs = [float(np.hypot(g["stat"], g["syst"])) for g in grads]
        ax.axhspan(-TARGET_GRAD, TARGET_GRAD, color="#E6E8EA")
        ax.errorbar(range(len(grads)), vals, yerr=errs, fmt="o", ms=5,
                    capsize=3, color=C_AX["z"])
        ax.axhline(0, color=C_MUTE, lw=0.8)
        ax.set_xticks(range(len(grads)))
        ax.set_xticklabels(names, rotation=30, ha="right", fontsize=8)
        ax.set_ylabel("gradient [pT/m]")
        ax.set_title(f"gradients vs the +/-{TARGET_GRAD:g} pT/m target")
        fig.savefig(os.path.join(outdir, "map_residual_gradients.png"),
                    bbox_inches="tight")
        plt.close(fig)


# ---------------------------------------------------------------- pipeline

def run_analysis(args):
    require(args.manifest or args.gradient_runs,
            "give --manifest (moved-head survey) and/or --gradient-runs")
    zeros = read_ledger_zeros(args.ledger)

    posruns, fields, grads, fit, cube = [], [], [], None, None
    ref_scatter, protected = {}, False
    if args.manifest:
        rows = parse_manifest(args.manifest)
        posruns = load_survey(rows, args)
        protected, ref_scatter = detrend_by_refs(posruns)
        pairs = flip_pairs(posruns, args.pos_tol_mm)
        fields = field_map(posruns, pairs, zeros, ref_scatter)
        grads = neighbor_gradients(posruns, args, protected, ref_scatter)
        fit = linear_fit(posruns, args)
        cube = uniform_cube(fit, args.uniformity_pt)

    mh_grads = multihead_gradients(args.gradient_runs, zeros, args)

    n_ref = sum(1 for p in posruns if p.ref)
    print(f"=== map_residual: {len(posruns)} survey position(s), "
          f"{n_ref} ref revisit(s), {len(mh_grads)} multi-head pair(s) ===")
    report(fields, grads, mh_grads, fit, cube, ref_scatter, protected)

    all_grads = grads + mh_grads
    clean = [g for g in all_grads if g["absolute"]]
    verdict = None
    if clean:
        # The component with the largest |g| is not the one most
        # likely to fail: a 50 +/- 5 pT/m passes a 60 pT/m spec while
        # a 30 +/- 40 alongside it does not, and taking the verdict
        # from the largest point estimate called that overall PASS
        # (2026-08-19 analysis review, H5).
        worst = max(clean, key=lambda g: abs(g["value_pt_per_m"]))
        failed = [g["component"] for g in clean if not g["pass"]]
        verdict = {"max_grad_pt_per_m": abs(worst["value_pt_per_m"]),
                   "component": worst["component"],
                   "failed_components": failed,
                   "pass": not failed}
        if failed and worst["pass"]:
            print("[warn] overall FAIL on "
                  + ", ".join(failed)
                  + f", although the largest gradient "
                    f"({worst['component']}) passes")

    os.makedirs(args.outdir, exist_ok=True)
    summary = {
        "program": "map_residual.py",
        "generated": datetime.now().isoformat(timespec="seconds"),
        "inputs": {"manifest": args.manifest,
                   "gradient_runs": args.gradient_runs,
                   "args": {k: v for k, v in vars(args).items()
                            if k != "test"}},
        "results": {
            "survey": {"n_positions": len(posruns), "n_refs": n_ref,
                       "ref_scatter_pt": ref_scatter,
                       "drift_protected": protected},
            "fields": fields, "gradients": all_grads,
            "linear_fit": fit, "uniform_cube": cube, "verdict": verdict,
        },
        "warnings": (fit or {}).get("warnings", []),
        "pass": None if verdict is None else verdict["pass"],
    }
    path = os.path.join(args.outdir, "map_residual_summary.json")
    with open(path, "w", newline="\n") as fh:
        json.dump(summary, fh, indent=1)
    print(f"[out ] {path}")

    if not args.no_figures and posruns:
        make_figures(args.outdir, posruns, fields, grads)
        print(f"[out ] figures in {args.outdir}")
    return summary


# ---------------------------------------------------------------- self-test

TRUE_G = 40.0          # pT/m along z
TRUE_B0 = -74.0        # pT at the origin
HEAD_ZERO = 1800.0     # pT, constant head-fixed zero


def synth_survey(tmp, sub, rng, with_refs=True, step_at=None, step_pt=0.0,
                 gap_min=10.0, drift=0.5):
    """1D bore survey: B(z) = B0 + G*z on the z axis, y flat; the head
    zero rides on every mean; a flip pair at the center; ref revisits at
    the origin every third run when with_refs."""
    base = os.path.join(tmp, sub)
    os.makedirs(base)
    fs, dur = 300.0, 400.0
    z_grid = (-100.0, -50.0, 0.0, 50.0, 100.0)
    t_start = datetime.fromisoformat("2026-07-08T10:00:00").timestamp()
    rows, k = [], 0

    def one_run(name, z_mm, orientation, ref):
        nonlocal k
        t0 = k * gap_min * 60.0
        k += 1
        sign = -1.0 if orientation == 180 else 1.0
        field_z = TRUE_B0 + TRUE_G * z_mm * 1e-3 \
            + (step_pt if step_at is not None and z_mm == step_at else 0.0)
        n = int(fs * dur)
        t = np.arange(n) / fs
        levels = {"z": HEAD_ZERO + sign * field_z, "y": -1200.0 - sign * 40.0}
        axes = {}
        for axis, level in levels.items():
            axes[axis] = level + drift * (t0 + t) / 60.0 \
                + rng.normal(0.0, 21e-3 * np.sqrt(fs / 2.0), n)
        run = os.path.join(base, name)
        os.makedirs(run)
        start = datetime.fromtimestamp(t_start + t0)
        for axis, x in axes.items():
            head = (f"# QZFM shield acquisition | axis={axis.upper()} "
                    f"session={name}\n# start={start.isoformat()} fs={fs:g} "
                    f"Hz gain=1x cal=370.4 pT/V vrange=1.0 V\n"
                    f"# device=Dev2 ch=Dev2/ai0 sensor=SIM source=simulate\n"
                    f"# one value per line, unit=pT")
            np.savetxt(os.path.join(run, f"{name}_{axis.upper()}_pT.txt"),
                       x * (GAIN_MEASURED / 2.7), fmt="%.3f", header=head,
                       comments="")
        meta = {
            "start_iso": start.isoformat(timespec="seconds"), "session": name,
            "axes": ["z", "y"],
            "channel_map": {"z": "Dev2/ai0", "y": "Dev2/ai1"}, "aux_map": {},
            "fs_hz": fs, "n_samples": n, "duration_s": dur,
            "clock_elapsed_s": dur, "fs_effective_hz": fs,
            "gain_mode": "1x", "cal_v_per_nT_used": 2.7,
            "cal_pT_per_V": 1000.0 / 2.7, "vrange": 1.0, "device": "Dev2",
            "sensor_id": "SIM",
            "rail_fraction": {"z": 0.0, "y": 0.0},
            "qzfm_rail_fraction": {"z": 0.0, "y": 0.0},
            "n_nonfinite": {"z": 0, "y": 0},
            "flat_ptp_V": {"z": 0.02, "y": 0.02},
            "mux_skew_note": "simulated", "source": "simulate",
            "software": "qzfm_record.py", "topology": "1head",
            "software_legacy": "qzfm_analog.py",
            "status": "ok", "exit_code": 0,
            "error": None, "warnings": [], "notes": "map_residual --test",
        }
        with open(os.path.join(run, f"{name}.meta.json"), "w",
                  newline="\n") as fh:
            json.dump(meta, fh, indent=1)
        rows.append(f"{run},0,0,{z_mm:g},z:+bore;y:+vert,{orientation},"
                    f"{1 if ref else 0},{name}")

    for i, z in enumerate(z_grid):
        one_run(f"p{i}", z, 0, ref=False)
        if with_refs and i % 2 == 1:
            one_run(f"ref{i}", 0.0, 0, ref=True)
    one_run("flip180", 0.0, 180, ref=False)
    if with_refs:
        one_run("ref_end", 0.0, 0, ref=True)

    man = os.path.join(base, "survey.csv")
    with open(man, "w", newline="\n") as fh:
        fh.write("run_dir,x_mm,y_mm,z_mm,axis_map,orientation,ref,label\n")
        fh.write("\n".join(rows) + "\n")
    return man


def targs(tmp, extra):
    return build_parser().parse_args(
        [*extra, "--allow-sim", "--no-figures",
         "--outdir", os.path.join(tmp, "out")])


def zgrads(s):
    return [g for g in s["results"]["gradients"]
            if g["component"] == "dBz/dz"]


def test_gradient(ctx):
    man = synth_survey(ctx["tmp"], "t1", np.random.default_rng(SIM_SEED))
    s = run_analysis(targs(ctx["tmp"], ["--manifest", man]))
    for g in zgrads(s):
        unc = np.hypot(g["stat"], g["syst"])
        assert abs(g["value_pt_per_m"] - TRUE_G) <= 3 * unc, g
        assert g["pass"] is True and g["absolute"] is True, g
    ctx["survey"] = (man, s)


def test_absolute_field(ctx):
    man, s = ctx["survey"]
    flips = [f for f in s["results"]["fields"]
             if f["method"] == "flip" and f["axis"] == "z"]
    assert flips, "no flip-pinned field"
    f = flips[0]
    assert abs(f["value_pt"] - TRUE_B0) <= 3 * max(f["err_pt"], 1.0), f
    raw = [f for f in s["results"]["fields"] if f["method"] == "raw-mean"]
    assert raw and all(not f["absolute"] for f in raw), \
        "non-flip positions must be offset-contaminated without a ledger"

    ledger = os.path.join(ctx["tmp"], "zeros.jsonl")
    with open(ledger, "w", newline="\n") as fh:
        for axis, zero in (("z", HEAD_ZERO), ("y", -1200.0)):
            fh.write(json.dumps({"date": "2026-07-08T09:00:00",
                                 "kind": "zero_flip", "value": zero,
                                 "unc": 2.0, "unit": "pT", "tool": "test",
                                 "axis": axis, "head": "SIM"}) + "\n")
    s2 = run_analysis(targs(ctx["tmp"], ["--manifest", man,
                                         "--ledger", ledger]))
    pinned = [f for f in s2["results"]["fields"]
              if f["method"] == "ledger-zero" and f["axis"] == "z"]
    assert pinned and all(f["absolute"] for f in pinned), \
        "ledger zeros must promote raw means to absolute"


def test_drift_unprotected(ctx):
    man = synth_survey(ctx["tmp"], "t3", np.random.default_rng(SIM_SEED + 1),
                       with_refs=False)
    s = run_analysis(targs(ctx["tmp"], ["--manifest", man]))
    assert s["results"]["survey"]["drift_protected"] is False
    for g in zgrads(s):
        assert g["drift_protected"] is False
        alias = g["drift_alias_pt_per_m"]
        actual = abs(g["value_pt_per_m"] - TRUE_G)
        assert alias and actual <= 2.0 * alias + 3 * g["stat"], \
            f"alias estimate {alias:.0f} vs actual bias {actual:.0f}"


def test_local_source(ctx):
    man = synth_survey(ctx["tmp"], "t4", np.random.default_rng(SIM_SEED + 2),
                       step_at=50.0, step_pt=2000.0)
    try:
        s = run_analysis(targs(ctx["tmp"], ["--manifest", man]))
    except SystemExit as e:
        raise AssertionError(f"survey with a local source refused ({e.code}); "
                             "should warn instead")
    warns = " ".join(s["warnings"])
    assert "local source" in warns, f"no local-source warning: {warns!r}"


def test_multihead(ctx, legacy_meta=False, tag="mh"):
    rng = np.random.default_rng(SIM_SEED + 3)
    run = os.path.join(ctx["tmp"], tag)
    os.makedirs(run)
    fs, dur, L = 300.0, 400.0, 0.1
    n = int(fs * dur)
    g_series = 25.0 + rng.normal(0.0, 0.5, n)
    head = ("# QZFM gradient | pair=S1-S2 baseline_m=0.1\n# start fs\n"
            "# device\n# one value per line, unit=pT/m")
    np.savetxt(os.path.join(run, f"{tag}_G_S1_S2_pT_per_m.txt"), g_series,
               fmt="%.4f", header=head, comments="")
    meta = {
        "start_iso": "2026-07-08T15:00:00", "session": "mh",
        "sensors": ["s1", "s2"],
        "gradients": [{"a": "s1", "b": "s2", "baseline_m": L,
                       "expected_cmrr_db_at_30hz": 38.2}],
        "fs_hz": fs, "n_samples": n, "duration_s": dur,
        "clock_elapsed_s": dur, "fs_effective_hz": fs,
        "gain_mode": "1x", "cal_v_per_nT_used": 2.624,
        "cal_pT_per_V": 1000.0 / 2.624, "vrange": 1.0, "device": "Dev2",
        "rail_fraction": {"s1": 0.0, "s2": 0.0},
        "qzfm_rail_fraction": {"s1": 0.0, "s2": 0.0},
        "n_nonfinite": {"s1": 0, "s2": 0},
        "flat_ptp_V": {"s1": 0.02, "s2": 0.02},
        "source": "simulate",
        **({"software": "qzfm_gradient.py"} if legacy_meta else
           {"software": "qzfm_record.py", "topology": "4head",
            "software_legacy": "qzfm_gradient.py"}),
        "status": "ok", "exit_code": 0, "error": None, "warnings": [],
        "notes": "map_residual --test",
    }
    with open(os.path.join(run, f"{tag}.meta.json"), "w", newline="\n") as fh:
        json.dump(meta, fh, indent=1)
    s = run_analysis(targs(ctx["tmp"], ["--gradient-runs", run]))
    g = s["results"]["gradients"][0]
    assert abs(g["value_pt_per_m"] - 25.0) <= 1.0, g
    assert g["absolute"] is False, "multi-head without ledger zeros must be " \
                                   "offset-contaminated"
    assert g["cmrr_db_at_30hz"] == 38.2, g
    # the head-to-head ceiling is computed here, not copied: 1.4 ms at
    # 30 Hz is 11.6 dB, and reading the meta's 38.2 as the bench's
    # ceiling was the misreading this key exists to stop
    assert abs(g["cmrr_head_db_at_30hz"] - 11.6) < 0.1, g
    assert abs(cmrr_ceiling_db(0.05, HEAD_SKEW_S) - 67.1) < 0.2, "0.05 Hz"


def test_multihead_legacy_meta(ctx):
    """The pre-merge four-head runs in result/ must stay readable. The
    gate takes software OR software_legacy; every fixture here wrote the
    old name until 2026-08-10, so it was the POST-merge branch that had
    no test -- this pair covers both."""
    test_multihead(ctx, legacy_meta=True, tag="mh_legacy")


def run_self_test():
    import tempfile
    tmp = tempfile.mkdtemp(prefix="map_residual_test_")
    print("=== map_residual --test: synthetic checks (temp dir, removed after) ===")
    ctx = {"tmp": tmp}
    try:
        for test in (test_gradient, test_absolute_field,
                     test_drift_unprotected, test_local_source,
                     test_multihead, test_multihead_legacy_meta):
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
    p.add_argument("--manifest", default=None,
                   help="survey CSV: run_dir,x_mm,y_mm,z_mm,axis_map,"
                        "orientation,ref,label")
    p.add_argument("--gradient-runs", nargs="*", default=None, metavar="DIR",
                   help="qzfm_record.py --4head run dir(s) (multi-head)")
    p.add_argument("--ledger", default=None,
                   help="calibration ledger; zero_flip entries promote "
                        "means to absolute")
    p.add_argument("--gain-v-per-nt", type=float, default=GAIN_MEASURED,
                   help="true gain for the epoch correction / --raw")
    p.add_argument("--raw", action="store_true",
                   help="analyze raw volts instead of pT files")
    p.add_argument("--pos-tol-mm", type=float, default=2.0,
                   help="survey position uncertainty")
    p.add_argument("--uniformity-pt", type=float, default=50.0,
                   help="bound for the uniform-cube search")
    p.add_argument("--outdir", default=os.path.dirname(os.path.abspath(__file__)),
                   help="where the summary and figures go")
    p.add_argument("--force", action="store_true",
                   help="analyze runs whose status is not ok (rails: never)")
    p.add_argument("--allow-sim", action="store_true",
                   help="accept runs recorded with --simulate")
    p.add_argument("--no-figures", action="store_true",
                   help="skip figures (headless quick check)")
    p.add_argument("--test", action="store_true",
                   help="offline self-test on synthetic surveys")
    return p


def main(argv=None):
    args = build_parser().parse_args(argv)
    if args.test:
        return run_self_test()
    require(1.5 <= args.gain_v_per_nt <= 4.0,
            f"--gain-v-per-nt {args.gain_v_per_nt} implausible at 1x")
    require(args.pos_tol_mm > 0, "--pos-tol-mm must be positive")
    require(args.uniformity_pt > 0, "--uniformity-pt must be positive")
    run_analysis(args)
    return 0


if __name__ == "__main__":
    sys.exit(main())
