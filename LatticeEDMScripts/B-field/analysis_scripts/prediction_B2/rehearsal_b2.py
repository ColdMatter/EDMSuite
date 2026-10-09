#!/usr/bin/env python3
"""Dress rehearsal of the B2 decision tree, before any MS-1L data exists.

PREDICTION.md section 4 claims the first long record inside the MS-1L
decides what the bench 1/f excess is: sensor-limited (~500-600 fT/rtHz at
0.05 Hz), real field or magnetization (~35-90), or mixed (~100-300). This
harness generates synthetic MS-1L records under each scenario -- nuisance
parameters (f_magn, shielding factors, sensor floor) drawn from the
documented ranges -- runs the same spectral analysis the real tools use,
applies the section-4 bands mechanically, and reports how often the tree
gets it right as a function of record length. It also tests whether the
low-frequency slope can separate E from M (the tree says one sensor
cannot; the slope is the one handle it has), and how the tree behaves
under realistic contamination (unsettled start, gain slip, a mid-record
field step).
Outputs rehearsal_summary.json and figures under rehearsal/.
Quick pass: --quick.  Self-checks: --test.
"""

from __future__ import annotations

import argparse
import json
import os
import shutil
import subprocess
import sys
from datetime import datetime

import numpy as np
from scipy.signal import welch
from scipy.optimize import curve_fit

# inputs fixed by PREDICTION.md (sections 1-3)
BENCH_005_FT = 613.0     # bench 0.05 Hz ASD, closed_long 06-30
BENCH_ALPHA = 0.93       # bench excess slope (sync5 model fit)
BENCH_WHITE_FT = 11.5    # bench white floor (GUI path)
SHIELD_WHITE_FT = 16.0   # MS-1L magnetic noise spec (white part)
SENSOR_FT = (7.0, 15.0)  # QZFM self-noise spec band
F_MAGN_HZ = (0.17, 1.5)  # magnetization knee sweep (wall thickness unknown)
S_BENCH = (3e3, 1e5)     # bench shield bracket (unmeasured)
S_MS1L = (3e5, 1e6)      # spec > 1e6, derated to 3e5

# the section-4 bands, mechanized. The gaps between them (95-100 and
# 300-400 in the prose) are closed here at 95 and 400; where a trial
# lands in a gap is itself a finding.
S_BAND_FT = 400.0        # above this at 0.05 Hz -> sensor-limited
EM_BAND_FT = 95.0        # below this -> field/magnetization
WHITE_LO, WHITE_HI = 14.5, 25.0   # white-floor branch: gain-slip / nominal / noisy
ALPHA_SPLIT = 0.7        # excess slope: magnetization 0.5 vs leak ~0.93

FS = 120.0               # rehearsal rate: keeps 3-10 Hz and the 50 Hz line
SETTLE_TAU_S = 185.0     # Z settling transient (sync5)
SETTLE_A_PT = 44.0
SIM_SEED = 20260708

HERE = os.path.dirname(os.path.abspath(__file__))
import sys as _sys  # analysis_scripts/ on sys.path for _paths
_sys.path.insert(0, os.path.dirname(os.path.dirname(os.path.abspath(__file__))))
from _paths import DATA_ROOT, PROJECT_DIR, ANALYSIS_DIR, data, data_join, rel_data  # noqa: E402,F401
REPO_ROOT = PROJECT_DIR
RUNS_COMPARE = os.path.join(ANALYSIS_DIR, "runs_compare",
                            "runs_compare.py")


def require(ok, msg, code=13):
    if not ok:
        print(f"[error] {msg}", file=sys.stderr)
        raise SystemExit(code)


# ------------------------------------------------------------- scenarios

def draw_params(rng):
    lu = lambda lo, hi: float(np.exp(rng.uniform(np.log(lo), np.log(hi))))
    return {"sensor_ft": float(rng.uniform(*SENSOR_FT)),
            "f_magn": lu(*F_MAGN_HZ),
            "s_bench": lu(*S_BENCH),
            "s_ms1l": lu(*S_MS1L)}


def asd_model_ft(f, scenario, p):
    """True ASD inside the MS-1L for one scenario, PREDICTION.md section 2.
    B_shield^2 = 16^2 (1 + f_magn/f): white eddy part + 1/f magnetization."""
    white2 = p["sensor_ft"] ** 2 + SHIELD_WHITE_FT ** 2
    mag2 = SHIELD_WHITE_FT ** 2 * (p["f_magn"] / f)
    total2 = white2 + mag2
    bench_excess = BENCH_005_FT * (0.05 / f) ** BENCH_ALPHA
    if scenario == "E":                      # bench excess was leakage
        total2 = total2 + (bench_excess * p["s_bench"] / p["s_ms1l"]) ** 2
    elif scenario == "S":                    # the sensor carries its 1/f along
        total2 = total2 + bench_excess ** 2
    return np.sqrt(total2)


def synth_record(scenario, p, seconds, rng, dc_pt=-70.0, mains_pt=0.05,
                 transient=False, step_pt=0.0):
    """Time series [pT] with the scenario's spectrum, plus the standard
    contamination: DC offset, a weak 50 Hz line, optionally the settling
    transient and a mid-record field step."""
    n = int(FS * seconds)
    wn = rng.normal(0.0, 1.0, n)
    F = np.fft.rfft(wn)
    f = np.fft.rfftfreq(n, 1.0 / FS)
    shape = np.zeros_like(f)
    shape[1:] = asd_model_ft(f[1:], scenario, p) * 1e-3   # fT -> pT
    x = np.fft.irfft(F * shape * np.sqrt(FS / 2.0), n)
    t = np.arange(n) / FS
    x = x + dc_pt + mains_pt * np.sin(2 * np.pi * 50.0 * t)
    if transient:
        x = x + SETTLE_A_PT * np.exp(-t / SETTLE_TAU_S)
    if step_pt:
        x[n // 2:] += step_pt
    return x


# ------------------------------------------------------------- measure

from bfield.analysis.spectra import welch_asd  # noqa: E402 -- was an identical local copy


def asd_at(f, a, f0):
    m = (f >= f0 / 1.25) & (f <= f0 * 1.25) & (f > 0)
    return float(np.median(a[m])) if m.any() else float("nan")


def fit_excess_alpha(f, a, white_pt):
    """Slope of the low-frequency excess: fit sqrt(w^2+(A/f^alpha)^2) in
    log space over 0.01-2 Hz. Returns alpha or None."""
    m = (f >= 0.01) & (f <= 2.0) & (a > 0)
    if m.sum() < 15:
        return None

    def logmodel(lf, w, A, alpha):
        ff = np.exp(lf)
        return 0.5 * np.log(w ** 2 + (A / ff ** alpha) ** 2)

    try:
        popt, _ = curve_fit(
            logmodel, np.log(f[m]), np.log(a[m]),
            p0=[white_pt, a[m][0] * f[m][0], 0.7],
            bounds=([1e-6, 1e-9, 0.1], [1.0, 100.0, 2.0]), maxfev=40000)
    except Exception:
        return None
    return float(popt[2])


def measure(x, fs=FS):
    """The measurement the real chain makes: 0.05 Hz level from >=400 s
    Welch segments (PREDICTION.md section 6), white floor from 3-10 Hz,
    excess slope from a white+1/f fit."""
    f_lo, a_lo = welch_asd(x, fs, 400.0 * fs)
    level_ft = asd_at(f_lo, a_lo, 0.05) * 1e3
    f_hi, a_hi = welch_asd(x, fs, 20.0 * fs)
    mw = (f_hi >= 3.0) & (f_hi <= 10.0)
    white_ft = float(np.median(a_hi[mw])) * 1e3
    ml = f_lo < 1.0
    mh = f_hi >= 1.0
    f_sp = np.concatenate([f_lo[ml], f_hi[mh]])
    a_sp = np.concatenate([a_lo[ml], a_hi[mh]])
    alpha = fit_excess_alpha(f_sp, a_sp, white_ft * 1e-3)
    return {"level_005_ft": level_ft, "white_ft": white_ft, "alpha": alpha}


def classify(level_ft):
    if level_ft >= S_BAND_FT:
        return "S"
    if level_ft <= EM_BAND_FT:
        return "EM"
    return "mixed"


def white_branch(white_ft):
    if white_ft < WHITE_LO:
        return "gain-slip suspect (or quieter than spec)"
    if white_ft > WHITE_HI:
        return "excess white noise (ground loop / degauss?)"
    return "nominal (shield self-noise seen)"


EXPECTED = {"E": "EM", "M": "EM", "S": "S"}


# ------------------------------------------------------------- campaigns

def run_grid(durations_h, trials, rng):
    """The main Monte Carlo: scenario x duration x trials."""
    grid = []
    for scenario in ("E", "M", "S"):
        for hours in durations_h:
            rows = []
            for k in range(trials):
                p = draw_params(rng)
                x = synth_record(scenario, p, hours * 3600.0, rng)
                m = measure(x)
                m["class"] = classify(m["level_005_ft"])
                m["correct"] = m["class"] == EXPECTED[scenario]
                m["params"] = p
                rows.append(m)
            acc = float(np.mean([r["correct"] for r in rows]))
            grid.append({"scenario": scenario, "hours": hours,
                         "trials": trials, "accuracy": acc,
                         "levels_ft": [round(r["level_005_ft"], 1)
                                       for r in rows],
                         "classes": [r["class"] for r in rows],
                         "alphas": [None if r["alpha"] is None
                                    else round(r["alpha"], 3) for r in rows]})
            print(f"[grid] {scenario}  {hours:>4.1f} h  accuracy "
                  f"{acc * 100:3.0f}%   median level "
                  f"{np.median([r['level_005_ft'] for r in rows]):6.0f} fT/rtHz")
    return grid


def alpha_separation(grid, hours):
    """Can the excess slope tell E from M at this duration? M is exactly
    f^-0.5 (magnetization law); E carries the bench f^-0.93 leak."""
    out = {}
    for scen in ("E", "M"):
        cell = next(g for g in grid if g["scenario"] == scen
                    and g["hours"] == hours)
        alphas = [a for a in cell["alphas"] if a is not None]
        if not alphas:
            return None
        vote = [("E" if a > ALPHA_SPLIT else "M") for a in alphas]
        out[scen] = {"median_alpha": float(np.median(alphas)),
                     "called_E": vote.count("E"), "n": len(vote)}
    return out


def adversarial_suite(rng, hours=2.0, trials=8):
    """Contamination the tree will actually meet in the lab."""
    cases = []

    def run_case(name, scenario, expect, **kw):
        rows = []
        # popped ONCE, outside the loop: popping inside consumed the
        # flag on trial 1 and ran the other seven as plain M, so the
        # saved summary said "nominal" where REHEARSAL.md claims the
        # gain slip was caught (2026-08-19 analysis review, H9)
        quiet_sensor = kw.pop("quiet_sensor", False)
        for _ in range(trials):
            p = draw_params(rng)
            if quiet_sensor:
                p["sensor_ft"] = 3.0        # a too-quiet white floor,
                p_shield = 8.0              # the gain-slip look-alike
                x = synth_record(scenario, p, hours * 3600.0, rng, **kw)
                x = x * (p_shield / SHIELD_WHITE_FT)
            else:
                x = synth_record(scenario, p, hours * 3600.0, rng, **kw)
            m = measure(x)
            m["class"] = classify(m["level_005_ft"])
            rows.append(m)
        med = float(np.median([r["level_005_ft"] for r in rows]))
        classes = [r["class"] for r in rows]
        n_expected = classes.count(expect)
        cases.append({"case": name, "scenario": scenario,
                      "expected_class": expect,
                      "held": f"{n_expected}/{trials}",
                      "median_level_ft": round(med, 1),
                      "median_white_ft": round(float(np.median(
                          [r["white_ft"] for r in rows])), 1),
                      "white_branch": white_branch(float(np.median(
                          [r["white_ft"] for r in rows])))})
        print(f"[adv ] {name}: {n_expected}/{trials} still {expect},"
              f" median 0.05 Hz {med:.0f} fT/rtHz")

    # an unsettled start is the likeliest real-world mistake
    run_case("M + settling transient not trimmed", "M", "EM", transient=True)
    # a mid-record 20 pT step (door, HVAC, degauss elsewhere)
    run_case("M + 20 pT mid-record step", "M", "EM", step_pt=20.0)
    # the section-4 'white floor ~12' branch: quieter-than-spec look-alike
    run_case("M + quiet-shield/gain-slip look-alike", "M", "EM",
             quiet_sensor=True)
    return cases


# ------------------------------------------------------------- e2e demo

def write_pt_run(base, session, x, start_iso):
    run = os.path.join(base, session)
    os.makedirs(run)
    head = (f"# QZFM shield acquisition | axis=Z session={session}\n"
            f"# start={start_iso} fs={FS:g} Hz gain=1x cal=381.1 pT/V "
            f"vrange=1.0 V\n"
            f"# device=Dev2 ch=Dev2/ai0 sensor=SIM source=simulate\n"
            f"# one value per line, unit=pT")
    np.savetxt(os.path.join(run, f"{session}_Z_pT.txt"), x, fmt="%.3f",
               header=head, comments="")
    meta = {
        "start_iso": start_iso, "session": session, "axes": ["z"],
        "channel_map": {"z": "Dev2/ai0"}, "aux_map": {},
        "fs_hz": FS, "n_samples": x.size, "duration_s": x.size / FS,
        "clock_elapsed_s": x.size / FS, "fs_effective_hz": FS,
        "gain_mode": "1x", "cal_v_per_nT_used": 2.624,
        "cal_pT_per_V": 1000.0 / 2.624, "vrange": 1.0, "device": "Dev2",
        "sensor_id": "SIM", "rail_fraction": {"z": 0.0},
        "qzfm_rail_fraction": {"z": 0.0}, "n_nonfinite": {"z": 0},
        "flat_ptp_V": {"z": 0.05}, "mux_skew_note": "simulated",
        "source": "simulate", "software": "qzfm_record.py",
        "status": "ok", "exit_code": 0, "error": None, "warnings": [],
        "notes": "B2 rehearsal synthetic MS-1L record",
    }
    with open(os.path.join(run, f"{session}.meta.json"), "w",
              newline="\n") as fh:
        json.dump(meta, fh, indent=1)
    return run


def end_to_end_demo(outdir, rng):
    """One 2 h record per scenario through the real runs_compare.py, files
    and gate included -- proof the production chain digests MS-1L data."""
    require(os.path.exists(RUNS_COMPARE), f"missing {RUNS_COMPARE}")
    base = os.path.join(outdir, "e2e_runs")
    shutil.rmtree(base, ignore_errors=True)
    os.makedirs(base)
    runs = []
    for i, scenario in enumerate(("E", "M", "S")):
        p = draw_params(rng)
        # the record starts SETTLE_S after Field Zero, as the recipe says,
        # so the transient tail is what runs_compare's own trim handles
        x = synth_record(scenario, p, 2 * 3600.0 + 700.0, rng, transient=True)
        runs.append(write_pt_run(base, f"ms1l_{scenario}", x,
                                 f"2026-07-09T{9 + 3 * i:02d}:00:00"))
    cmd = [sys.executable, RUNS_COMPARE, *runs,
           "--labels", "E-like", "M-like", "S-like",
           "--allow-sim", "--band", "3", "10",
           "--outdir", os.path.join(outdir, "e2e_out")]
    print("[e2e ] " + " ".join(os.path.relpath(c) if os.path.exists(c) else c
                               for c in cmd))
    proc = subprocess.run(cmd, capture_output=True, text=True)
    tail = [ln for ln in proc.stdout.splitlines()
            if ln.startswith(("[run]", "      low-f"))]
    print("\n".join(tail))
    require(proc.returncode == 0,
            f"runs_compare failed on rehearsal data:\n{proc.stderr[-2000:]}",
            code=1)
    with open(os.path.join(outdir, "e2e_out",
                           "runs_compare_summary.json")) as fh:
        summary = json.load(fh)
    out = []
    for r in summary["results"]["runs"]:
        lvl = r["lowf_ft"].get("0.05")
        out.append({"label": r["label"], "floor_ft": r["floor_ft"],
                    "level_005_ft": lvl,
                    "class": classify(lvl) if lvl else None})
    return out


# ------------------------------------------------------------- figures

C_E, C_M, C_S, C_MUTE, C_INK = ("#2E6E8E", "#5A7D3A", "#C0504D",
                                "#7A7F85", "#33373B")
COLORS = {"E": C_E, "M": C_M, "S": C_S}


def make_figures(outdir, grid, rng):
    import matplotlib
    matplotlib.use("Agg")
    import matplotlib.pyplot as plt
    plt.rcParams.update({
        "figure.dpi": 150, "font.size": 11, "axes.titleweight": "bold",
        "axes.spines.top": False, "axes.spines.right": False,
        "axes.edgecolor": C_MUTE, "text.color": C_INK,
        "legend.frameon": False,
    })

    # example spectra with the decision bands
    fig, ax = plt.subplots(figsize=(7.5, 4.8))
    for scenario in ("E", "M", "S"):
        p = draw_params(rng)
        x = synth_record(scenario, p, 2 * 3600.0, rng)
        f, a = welch_asd(x, FS, 400.0 * FS)
        m = (f > 0) & (f < 10)
        ax.loglog(f[m], a[m] * 1e3, lw=0.9, color=COLORS[scenario],
                  label=f"{scenario} (2 h example)")
    ax.axhspan(S_BAND_FT, 2500, color=C_S, alpha=0.07)
    ax.axhspan(1, EM_BAND_FT, color=C_M, alpha=0.07)
    for level, txt in ((S_BAND_FT, "S band"), (EM_BAND_FT, "E/M band")):
        ax.axhline(level, color=C_MUTE, ls="--", lw=0.8)
        ax.annotate(txt, (4, level), fontsize=8, color=C_MUTE, va="bottom")
    ax.axvline(0.05, color=C_MUTE, ls=":", lw=0.8)
    ax.set_xlabel("frequency [Hz]")
    ax.set_ylabel("ASD [fT/rtHz]")
    ax.set_title("rehearsal spectra vs the section-4 bands at 0.05 Hz")
    ax.legend(fontsize=9)
    fig.savefig(os.path.join(outdir, "fig_rehearsal_asd.png"),
                bbox_inches="tight")
    plt.close(fig)

    # classification accuracy vs record length
    fig, ax = plt.subplots(figsize=(6.5, 4.2))
    hours = sorted({g["hours"] for g in grid})
    for scenario in ("E", "M", "S"):
        acc = [next(g["accuracy"] for g in grid
                    if g["scenario"] == scenario and g["hours"] == h)
               for h in hours]
        ax.plot(hours, [a * 100 for a in acc], "o-", color=COLORS[scenario],
                label=scenario)
    ax.axhline(90, color=C_MUTE, ls="--", lw=0.8)
    ax.set_xlabel("record length [h]")
    ax.set_ylabel("correct classification [%]")
    ax.set_ylim(0, 105)
    ax.set_title("decision-tree power vs record length")
    ax.legend(fontsize=9, title="true scenario")
    fig.savefig(os.path.join(outdir, "fig_rehearsal_power.png"),
                bbox_inches="tight")
    plt.close(fig)

    # E vs M via the excess slope
    fig, ax = plt.subplots(figsize=(6.5, 4.2))
    h_show = [h for h in hours if h >= 2.0] or hours[-1:]
    for scenario in ("E", "M"):
        for h in h_show:
            cell = next(g for g in grid if g["scenario"] == scenario
                        and g["hours"] == h)
            alphas = [a for a in cell["alphas"] if a is not None]
            jitter = (np.random.default_rng(1).uniform(-0.08, 0.08,
                                                       len(alphas)))
            ax.plot(np.full(len(alphas), h) + jitter, alphas, "o", ms=4,
                    alpha=0.6, color=COLORS[scenario])
    ax.axhline(ALPHA_SPLIT, color=C_MUTE, ls="--", lw=0.8)
    ax.annotate(f"split {ALPHA_SPLIT}", (h_show[0], ALPHA_SPLIT), fontsize=8,
                color=C_MUTE, va="bottom")
    ax.set_xlabel("record length [h]")
    ax.set_ylabel("fitted excess slope alpha")
    ax.set_title("can the slope tell E (0.93) from M (0.5)?")
    fig.savefig(os.path.join(outdir, "fig_rehearsal_alpha.png"),
                bbox_inches="tight")
    plt.close(fig)


# ------------------------------------------------------------- self-test

def test_generator(_):
    rng = np.random.default_rng(SIM_SEED)
    p = {"sensor_ft": 10.0, "f_magn": 0.4, "s_bench": 1e4, "s_ms1l": 5e5}
    x = synth_record("M", p, 2 * 3600.0, rng, dc_pt=0.0, mains_pt=0.0)
    f, a = welch_asd(x, FS, 400.0 * FS)
    got = asd_at(f, a, 0.05) * 1e3
    want = float(asd_model_ft(np.array([0.05]), "M", p)[0])
    assert abs(got - want) / want < 0.25, f"0.05 Hz: {got:.1f} vs {want:.1f}"
    f2, a2 = welch_asd(x, FS, 20.0 * FS)
    mw = (f2 >= 3) & (f2 <= 10)
    got_w = float(np.median(a2[mw])) * 1e3
    want_w = float(asd_model_ft(np.array([6.0]), "M", p)[0])
    assert abs(got_w - want_w) / want_w < 0.10, \
        f"white: {got_w:.1f} vs {want_w:.1f}"


def test_classifier(_):
    assert classify(613.0) == "S"
    assert classify(48.0) == "EM"
    assert classify(200.0) == "mixed"
    assert "nominal" in white_branch(19.0)
    assert "gain-slip" in white_branch(12.0)
    assert "excess" in white_branch(30.0)


def test_alpha_fit(_):
    rng = np.random.default_rng(SIM_SEED + 1)
    p = {"sensor_ft": 10.0, "f_magn": 0.4, "s_bench": 1e4, "s_ms1l": 5e5}
    x = synth_record("S", p, 2 * 3600.0, rng, dc_pt=0.0, mains_pt=0.0)
    m = measure(x)
    assert m["alpha"] is not None and 0.75 <= m["alpha"] <= 1.15, \
        f"S-scenario slope {m['alpha']} should be near the bench 0.93"


def run_self_test():
    print("=== rehearsal_b2 --test: generator and classifier checks ===")
    try:
        for test in (test_generator, test_classifier, test_alpha_fit):
            test(None)
            print(f"[test] PASS: {test.__name__}")
        return 0
    except AssertionError as e:
        print(f"[test] FAIL: {e}", file=sys.stderr)
        return 1


# ------------------------------------------------------------- main

def main(argv=None):
    p = argparse.ArgumentParser(
        description=__doc__,
        formatter_class=argparse.RawDescriptionHelpFormatter)
    p.add_argument("--durations", nargs="*", type=float,
                   default=[0.5, 1.0, 2.0, 4.0], help="record lengths in h")
    p.add_argument("--trials", type=int, default=24,
                   help="Monte Carlo trials per scenario per duration")
    p.add_argument("--quick", action="store_true",
                   help="small smoke pass (6 trials, 0.5 and 2 h)")
    p.add_argument("--skip-demo", action="store_true",
                   help="skip the end-to-end runs_compare demo")
    p.add_argument("--outdir", default=os.path.join(HERE, "rehearsal"),
                   help="where results and figures go")
    p.add_argument("--test", action="store_true",
                   help="generator and classifier self-checks")
    args = p.parse_args(argv)
    if args.test:
        return run_self_test()
    require(args.trials >= 4, "--trials must be at least 4")
    if args.quick:
        args.trials, args.durations = 6, [0.5, 2.0]

    os.makedirs(args.outdir, exist_ok=True)
    rng = np.random.default_rng(SIM_SEED)
    print(f"=== B2 rehearsal: {args.trials} trials x {args.durations} h x "
          "scenarios E/M/S ===")
    grid = run_grid(args.durations, args.trials, rng)
    alpha = {f"{h:g}h": alpha_separation(grid, h)
             for h in args.durations if h >= 1.0}
    adv = adversarial_suite(rng)
    e2e = None if args.skip_demo else end_to_end_demo(args.outdir, rng)

    summary = {
        "program": "rehearsal_b2.py",
        "generated": datetime.now().isoformat(timespec="seconds"),
        "inputs": {"args": {k: v for k, v in vars(args).items()
                            if k != "test"},
                   "bands": {"S": S_BAND_FT, "EM": EM_BAND_FT,
                             "white": [WHITE_LO, WHITE_HI],
                             "alpha_split": ALPHA_SPLIT}},
        "results": {"grid": grid, "alpha_separation": alpha,
                    "adversarial": adv, "end_to_end": e2e},
        "warnings": [],
        "pass": None,
    }
    path = os.path.join(args.outdir, "rehearsal_summary.json")
    with open(path, "w", newline="\n") as fh:
        json.dump(summary, fh, indent=1)
    print(f"[out ] {path}")
    make_figures(args.outdir, grid, rng)
    print(f"[out ] figures in {args.outdir}")
    return 0


if __name__ == "__main__":
    sys.exit(main())
