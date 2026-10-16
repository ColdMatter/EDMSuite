#!/usr/bin/env python3
"""Shielding factor at DC, S_DC, from hand-reversed current dwells (B1).

The generator stops at 0.1 Hz and the reversal band runs a decade below
it, so the bottom of the science band has no AC source on this bench.
A steady current whose polarity the operator throws by hand does reach
it, and S is the same quantity: the transfer function is flat from DC up
to the eddy corner near 6 Hz, so the quasi-static factor IS the in-band
factor. This program reads what sf_sweep.py --dc recorded.

    B = [(m1 + m4) - (m2 + m3)] / 4        over one + - - + block
    S = k_geom * I / B,    k_geom = mu0 N sqrt(a^2+b^2) / (pi a b)

The ordering is the whole point. With four dwell means taken at + - - +
and a field drifting linearly at d per dwell,

    m1 = +B        m2 = -B + d        m3 = -B + 2d       m4 = +B + 3d

the combination above returns B exactly: the drift falls out rather than
being fitted out. That matters because the sensor's own drift here is
0.44 pT/min at its best, which is the size of the signal. A program that
fitted B(I) = B0 + m I across levels in acquisition order lived in this
folder until 2026-08-10; it discarded the offset but not the drift, and
it was removed when the plan was for the AC sweep to cover 0.05 Hz. The
sweep cannot: its generator stops at 0.1 Hz.

Everything the AC program says about the circularity and the gain
applies here unchanged, including the part that is easy to misread: the
two-coil argument is why a gain CAN be had, not a step this measurement
takes. The gain is the nominal 2.7 V/nT, cal_tone.py is not a
prerequisite, and it enters as a budgeted 3 percent behind k_geom's 5.
This program consumes a gain, it does not produce one. B_applied is the
free-space field the coil would make at the sensor position with no
shield present.

    python analysis/shield_sf/shield_sf_dc.py result/shield_sf/dc_<stamp>
    python analysis/shield_sf/shield_sf_dc.py --test
"""

from __future__ import annotations

import argparse
import glob
import json
import math
import os
import sys
from datetime import datetime

import numpy as np

MU0 = 4e-7 * math.pi
COIL_TURNS = 2              # the lab coil, measured 2026-07-10; the run's
COIL_W_M = 0.9              # own session json overrides all three
COIL_H_M = 1.2
PATTERN = ("+", "-", "-", "+")
SETTLE_S = 30.0             # dropped after each flip, before the mean
MIN_BLOCKS = 5              # below this the spread is not an interval
RAIL_LIMIT = 0.001
C_PT, C_INK, C_MUTE = "#2E6E8E", "#33373B", "#7A7F85"


class DCError(Exception):
    def __init__(self, msg, code=13):
        super().__init__(msg)
        self.code = code


def k_geom_nt_per_ma(n_turns, w_m, h_m):
    """Field at the centre of a rectangular loop, nT/mA."""
    a, b = w_m / 2.0, h_m / 2.0
    return MU0 * n_turns * math.sqrt(a * a + b * b) / (math.pi * a * b) * 1e6


# ------------------------------------------------------------------ i/o

def dwell_mean_pT(run_dir, dwell_dir, channel, settle_s):
    """Mean field of one dwell in pT, and the samples behind it.

    The settle window is dropped from the front: a flip is a step, and
    what the walls do in the first seconds after one is eddy physics,
    not the quasi-static factor this program is measuring.
    """
    d = os.path.join(run_dir, os.path.basename(dwell_dir))
    hits = glob.glob(os.path.join(d, "**", f"*_{channel.upper()}_pT.txt"),
                     recursive=True)
    if not hits:
        raise DCError(f"{os.path.basename(d)}: no {channel.upper()} record")
    meta = glob.glob(os.path.join(os.path.dirname(hits[0]), "*.meta.json"))
    if not meta:
        raise DCError(f"{os.path.basename(d)}: no meta beside the record")
    m = json.load(open(meta[0], encoding="utf-8"))
    rail = (m.get("rail_fraction") or {}).get(channel.lower(), 0.0) or 0.0
    if rail > RAIL_LIMIT:
        raise DCError(f"{os.path.basename(d)}: {channel} railed "
                      f"{rail:.1%} of the record", 30)
    fs = float(m["fs_hz"])
    x = np.loadtxt(hits[0])
    keep = x[int(settle_s * fs):]
    if keep.size < int(10.0 * fs):
        raise DCError(f"{os.path.basename(d)}: only {keep.size / fs:.0f} s "
                      f"left after a {settle_s:g} s settle")
    return float(np.mean(keep)), keep.size, fs


def load_run(run_dir, i_ma_override=None):
    """The dwell ledger, from the run's own session json where there is
    one. Without it the directory names still carry block, index and
    sign, but the current has to come from --i-ma."""
    sess = os.path.join(run_dir, "sf_dc_session.json")
    if os.path.exists(sess):
        doc = json.load(open(sess, encoding="utf-8"))
        k = (doc.get("settings", {}) or {}).get("k_geom_nT_per_mA")
        return doc.get("dwells", []), (float(k) if k else None), doc
    if i_ma_override is None:
        raise DCError(f"{run_dir}: no sf_dc_session.json, so the current "
                      f"is unknown -- pass --i-ma")
    dwells = []
    for d in sorted(glob.glob(os.path.join(run_dir, "b*d?_[pm]"))):
        name = os.path.basename(d)
        sign = "+" if name.endswith("_p") else "-"
        dwells.append({"block": int(name[1:3]), "index": int(name[4]),
                       "sign": sign, "dir": name,
                       "i_ma": i_ma_override if sign == "+"
                               else -i_ma_override})
    if not dwells:
        raise DCError(f"{run_dir}: no dwell directories")
    return dwells, None, {}


# ------------------------------------------------------------- estimator

def block_field_pT(means, signs):
    """[(m1 + m4) - (m2 + m3)] / 4 for a + - - + block."""
    if list(signs) != list(PATTERN):
        raise DCError(f"block order is {''.join(signs)}, not "
                      f"{''.join(PATTERN)} -- the drift cancellation is "
                      f"the ordering, so this block cannot be used", 31)
    return ((means[0] + means[3]) - (means[1] + means[2])) / 4.0


def analyse(run_dir, channels, settle_s, i_ma_override=None):
    dwells, k_sess, doc = load_run(run_dir, i_ma_override)
    k = k_sess or k_geom_nt_per_ma(COIL_TURNS, COIL_W_M, COIL_H_M)
    by_block = {}
    for d in dwells:
        by_block.setdefault(d["block"], []).append(d)
    out = {"run": os.path.basename(os.path.abspath(run_dir)),
           "k_geom_nT_per_mA": k, "settle_s": settle_s,
           "channels": {}, "skipped": []}
    for ch in channels:
        blocks = []
        for b in sorted(by_block):
            group = sorted(by_block[b], key=lambda d: d["index"])
            if len(group) != 4:
                out["skipped"].append(f"block {b}: {len(group)} dwell(s), "
                                      f"not 4")
                continue
            try:
                means = [dwell_mean_pT(run_dir, g["dir"], ch, settle_s)[0]
                         for g in group]
                B = block_field_pT(means, [g["sign"] for g in group])
            except DCError as e:
                out["skipped"].append(f"{ch}: {e}")
                continue
            I = float(np.mean([abs(g["i_ma"]) for g in group]))
            if B == 0.0:
                out["skipped"].append(f"{ch} block {b}: zero difference")
                continue
            blocks.append({"block": b, "B_in_pT": B, "i_ma": I,
                           "S": k * I * 1e3 / abs(B)})
        if not blocks:
            continue
        S = np.array([b["S"] for b in blocks], dtype=float)
        Bv = np.array([b["B_in_pT"] for b in blocks], dtype=float)
        sem = float(np.std(S, ddof=1) / math.sqrt(S.size)) if S.size > 1 \
            else float("nan")
        out["channels"][ch] = {
            "n_blocks": int(S.size),
            "S_mean": float(np.mean(S)), "S_sem": sem,
            "S_blocks": [round(v, 1) for v in S.tolist()],
            "B_in_pT_mean": float(np.mean(Bv)),
            "B_in_pT_sd": float(np.std(Bv, ddof=1)) if S.size > 1 else None,
            "i_ma_mean": float(np.mean([b["i_ma"] for b in blocks])),
            "usable": bool(S.size >= MIN_BLOCKS),
        }
    vals = [c["S_mean"] for c in out["channels"].values()]
    if len(vals) > 1:
        # geometric mean, as the AC program does: S is a ratio, and the
        # two heads disagree by their gain mismatch and by the real
        # position dependence of the leak inside the shield
        out["S_combined"] = float(math.exp(np.mean(np.log(vals))))
        out["head_spread"] = float(max(vals) / min(vals))
    elif vals:
        out["S_combined"] = float(vals[0])
        out["head_spread"] = None
    return out


# ------------------------------------------------------------------ out

def figure(summary, path):
    import matplotlib
    matplotlib.use("Agg")
    import matplotlib.pyplot as plt

    fig, ax = plt.subplots(figsize=(7.0, 4.0))
    for ch, c in sorted(summary["channels"].items()):
        y = c["S_blocks"]
        x = list(range(1, len(y) + 1))
        ax.plot(x, y, "o-", label=f"{ch.upper()}  mean {c['S_mean']:.2e}")
        ax.axhline(c["S_mean"], color=C_MUTE, lw=0.8, ls="--")
    ax.set_xlabel(f"{''.join(PATTERN)} block")
    ax.set_ylabel("S at DC")
    ax.set_yscale("log")
    ax.set_title(f"{summary['run']}: S per block, "
                 f"{summary['settle_s']:g} s settle dropped")
    ax.grid(alpha=0.3)
    ax.legend(fontsize=8)
    fig.tight_layout()
    fig.savefig(path, dpi=150)
    plt.close(fig)


def report(summary):
    print(f"=== S at DC: {summary['run']}   "
          f"k_geom = {summary['k_geom_nT_per_mA']:.4g} nT/mA ===")
    print(f"{'ch':>4} {'blocks':>7} {'I mA':>8} {'B_in pT':>10} "
          f"{'S':>12} {'SEM':>11}")
    for ch, c in sorted(summary["channels"].items()):
        print(f"{ch.upper():>4} {c['n_blocks']:>7} {c['i_ma_mean']:>8.1f} "
              f"{c['B_in_pT_mean']:>10.3f} {c['S_mean']:>12.3e} "
              f"{c['S_sem']:>11.2e}"
              + ("" if c["usable"] else
                 f"   <{MIN_BLOCKS} blocks: no usable interval"))
    if summary.get("S_combined"):
        print(f"combined (geometric mean): {summary['S_combined']:.3e}"
              + (f"   heads differ by x{summary['head_spread']:.2f}"
                 if summary.get("head_spread") else ""))
    for s in summary["skipped"]:
        print(f"[skip] {s}")


def main(argv=None):
    p = argparse.ArgumentParser(
        description="S at DC from hand-reversed dwells (sf_sweep.py --dc)")
    p.add_argument("run_dir", nargs="?", help="a dc_<stamp> run directory")
    p.add_argument("--heads", default="z,z2",
                   help="interior channels to read (default z,z2)")
    p.add_argument("--settle", type=float, default=SETTLE_S,
                   help=f"seconds dropped after each flip (default "
                        f"{SETTLE_S:g})")
    p.add_argument("--i-ma", type=float, default=None,
                   help="drive in mA, when the run has no session json")
    p.add_argument("--outdir", default=None, help="where to write")
    p.add_argument("--test", action="store_true",
                   help="synthesise dwells with a known S and a drift, and "
                        "check both come back")
    args = p.parse_args(argv)
    if args.test:
        return selftest()
    if not args.run_dir:
        p.error("give a run directory, or --test")
    try:
        summary = analyse(args.run_dir, [c.strip().lower()
                                         for c in args.heads.split(",")
                                         if c.strip()],
                          args.settle, args.i_ma)
    except DCError as e:
        print(f"ERROR[{e.code}] {e}", file=sys.stderr)
        return e.code
    if not summary["channels"]:
        print("ERROR[30] no channel produced a block", file=sys.stderr)
        for s in summary["skipped"]:
            print(f"[skip] {s}", file=sys.stderr)
        return 30
    out = args.outdir or os.path.join(args.run_dir, "analysis")
    os.makedirs(out, exist_ok=True)
    summary["generated"] = datetime.now().isoformat(timespec="seconds")
    with open(os.path.join(out, "shield_sf_dc_summary.json"), "w",
              newline="\n", encoding="utf-8") as fh:
        json.dump(summary, fh, indent=1)
    figure(summary, os.path.join(out, "fig_shield_sf_dc.png"))
    report(summary)
    print(f"[out ] {out}")
    return 0


# ----------------------------------------------------------------- test

def _write_dwell(root, name, sign, S, k, i_ma, drift_pT, fs=1200.0,
                 seconds=90.0, seed=0):
    d = os.path.join(root, name)
    os.makedirs(d, exist_ok=True)
    rng = np.random.default_rng(seed)
    n = int(seconds * fs)
    B = k * i_ma * 1e3 / S               # pT the shield lets in
    level = (B if sign == "+" else -B) + drift_pT
    for ch in ("Z", "Z2"):
        x = level + rng.normal(0.0, 20.0, n)
        np.savetxt(os.path.join(d, f"{name}_{ch}_pT.txt"), x, fmt="%.4f")
    with open(os.path.join(d, f"{name}.meta.json"), "w", newline="\n",
              encoding="utf-8") as fh:
        json.dump({"fs_hz": fs, "n_samples": n,
                   "rail_fraction": {"z": 0.0, "z2": 0.0}}, fh)


def selftest():
    import shutil
    import tempfile

    root = tempfile.mkdtemp(prefix="sf_dc_test_")
    try:
        k = k_geom_nt_per_ma(COIL_TURNS, COIL_W_M, COIL_H_M)
        assert abs(k - 20.0 / 9.0) < 1e-9, k
        print(f"[test] PASS: k_geom = {k:.6f} nT/mA = 20/9")

        S_TRUE, I_MA, NB = 3.0e6, 1000.0, 8
        DRIFT_PER_DWELL = 90.0     # pT per dwell: far above the signal
        dwells, n = [], 0
        for b in range(1, NB + 1):
            for j, sign in enumerate(PATTERN, start=1):
                name = f"b{b:02d}d{j}_{'p' if sign == '+' else 'm'}"
                _write_dwell(root, name, sign, S_TRUE, k, I_MA,
                             drift_pT=n * DRIFT_PER_DWELL, seed=n)
                dwells.append({"block": b, "index": j, "sign": sign,
                               "dir": name,
                               "i_ma": I_MA if sign == "+" else -I_MA})
                n += 1
        with open(os.path.join(root, "sf_dc_session.json"), "w",
                  newline="\n", encoding="utf-8") as fh:
            json.dump({"settings": {"k_geom_nT_per_mA": k},
                       "dwells": dwells}, fh)

        s = analyse(root, ["z", "z2"], settle_s=10.0)
        got = s["channels"]["z"]["S_mean"]
        assert abs(got / S_TRUE - 1.0) < 0.05, (got, S_TRUE)
        print(f"[test] PASS: S recovered {got:.3e} against {S_TRUE:.3e}, "
              f"through a drift of {DRIFT_PER_DWELL * n:.0f} pT across the "
              f"run ({DRIFT_PER_DWELL / (k * I_MA * 1e3 / S_TRUE):.0f}x the "
              f"signal)")
        assert s["channels"]["z"]["n_blocks"] == NB
        assert s["channels"]["z"]["usable"] is True
        print(f"[test] PASS: {NB} blocks, interval usable")

        # the ordering IS the cancellation: a block in acquisition order
        # + - + - must be refused rather than quietly averaged
        try:
            block_field_pT([1.0, 2.0, 3.0, 4.0], ["+", "-", "+", "-"])
        except DCError as e:
            print(f"[test] PASS: wrong order refused ({str(e)[:44]}...)")
        else:
            raise AssertionError("a +-+- block was accepted")

        # and without a session json the current has to be supplied
        os.remove(os.path.join(root, "sf_dc_session.json"))
        try:
            analyse(root, ["z"], settle_s=10.0)
        except DCError:
            print("[test] PASS: no session json and no --i-ma is an error")
        else:
            raise AssertionError("a run with no current was analysed")
        s2 = analyse(root, ["z"], settle_s=10.0, i_ma_override=I_MA)
        assert abs(s2["channels"]["z"]["S_mean"] / S_TRUE - 1.0) < 0.05
        print("[test] PASS: --i-ma fallback reads the directory names")
        print("[test] all passed")
        return 0
    finally:
        shutil.rmtree(root, ignore_errors=True)


if __name__ == "__main__":
    sys.exit(main())
