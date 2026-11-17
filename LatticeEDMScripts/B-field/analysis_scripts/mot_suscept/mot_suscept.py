#!/usr/bin/env python3
"""What a working MOT does to the field inside the closed shield (B5).

Compares MOT states (off / on / switching) at a fixed, never re-zeroed
sensor: the DC shift (a valid lab-frame difference -- the head zero
cancels), the added in-band noise, the switching comb with its aliases,
and the per-edge transient with the implied experiment dead time. A
MOT-on state that rails the sensor is not an error here: "shield
inadequate for MOT-on" is the headline result and exits 0. One caveat
travels with every line: the MOT also loads the lab electrically, so a
MOT-on line is not automatically magnetic coupling through the shield.
Writes mot_suscept_summary.json and figures.
Offline check: python mot_suscept.py --test
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
from scipy.signal import welch

GAIN_MEASURED = 2.624
SETTLE_S = 500.0
KNOWN_LINES_HZ = (50.0, 59.917, 100.0, 150.0, 203.18, 277.1, 294.0)
DC_SOFT_PT = 300.0      # beyond this the QZFM response is a few % nonlinear
RAIL_SAT = 0.05         # rail fraction that means "saturated state"
MAX_HARMONIC = 10
SIM_SEED = 20260707

REPO_ROOT = os.path.dirname(os.path.dirname(
    os.path.dirname(os.path.abspath(__file__))))

C_STATE = {"off": "#2E6E8E", "on": "#C0504D", "switching": "#5A7D3A"}
C_MUTE, C_INK = "#7A7F85", "#33373B"


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
    """Gate WITHOUT the rail refusal: a railed MOT-on state is the result
    'shield inadequate', not bad input."""
    if not (meta.get("status") == "ok" and meta.get("exit_code") == 0):
        # a saturation exit (30) with data on disk is expected here
        if meta.get("exit_code") not in (30,):
            require(force, f"{path}: status={meta.get('status')} exit_code="
                           f"{meta.get('exit_code')}; --force to analyze anyway")
            print(f"[gate] {path}: overridden status={meta.get('status')}")
    nonfinite = meta.get("n_nonfinite") or {}
    require(all(v == 0 for v in nonfinite.values()),
            f"{path}: non-finite samples: {nonfinite}")
    if meta.get("source") == "simulate":
        require(allow_sim, f"{path}: run is simulated; pass --allow-sim if intended")
    if "zero" in (meta.get("notes") or "").lower():
        print(f"[warn] {path}: notes mention 'zero' -- re-zeroing between "
              "states destroys the DC-shift measurement")


def is_saturated(meta):
    # keep the two counters separate: they share axis keys, and a dict
    # merge would let a clean qzfm counter mask a railed ADC one
    for key in ("rail_fraction", "qzfm_rail_fraction"):
        if any(v > RAIL_SAT for v in (meta.get(key) or {}).values()):
            return True
    return False


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


# ---------------------------------------------------------------- helpers

import sys as _sys  # analysis_scripts/ on sys.path for _paths
_sys.path.insert(0, os.path.dirname(os.path.dirname(os.path.abspath(__file__))))
import _paths  # noqa: E402,F401  (puts B-field on sys.path)
from bfield.analysis.spectra import welch_asd  # noqa: E402 -- was an identical local copy


def band_floor(x, fs, lo, hi, extra_lines=()):
    f, a = welch_asd(x, fs, 20.0 * fs)
    m = (f >= lo) & (f <= hi)
    for fline in (*KNOWN_LINES_HZ, *extra_lines):
        m &= np.abs(f - fline) > 0.5
    require(m.any(), f"no spectral bins left in {lo}-{hi} Hz")
    return float(np.median(a[m]))


def block_stats(x, fs, block_s=60.0):
    block_s = min(block_s, x.size / fs / 4.0)
    n = max(1, int(block_s * fs))
    nb = x.size // n
    require(nb >= 3, f"record too short for block stats ({nb} blocks)")
    means = x[:nb * n].reshape(nb, n).mean(axis=1)
    tmin = (np.arange(nb) + 0.5) * block_s / 60.0
    mean = float(means.mean())
    sem = float(means.std(ddof=1) / np.sqrt(nb))
    drift = float(np.polyfit(tmin, means, 1)[0])
    return mean, sem, drift


def line_fit(x, fs, f0, half=0.12):
    x = x - x.mean()
    n = x.size
    w = np.hanning(n)
    p = np.abs(np.fft.rfft(x * w)) ** 2
    f = np.fft.rfftfreq(n, 1.0 / fs)
    m = (f >= f0 - half) & (f <= f0 + half)
    side = (((f >= f0 - 3 * half) & (f < f0 - half))
            | ((f > f0 + half) & (f <= f0 + 3 * half)))
    if not m.any() or not side.any():
        return f0, 0.0, 0.0
    med = np.median(p[side])
    ps = np.clip(p[m] - med, 0.0, None)
    scale = float(np.sqrt(4.0 * max(med, 1e-300) * m.sum()
                          / (n * np.sum(w ** 2))))   # noise-only amplitude
    if ps.sum() <= 0:
        return f0, 0.0, scale
    fc = float(np.sum(f[m] * ps) / np.sum(ps))
    amp = float(np.sqrt(4.0 * ps.sum() / (n * np.sum(w ** 2))))
    return fc, amp, scale


def fold(f, fs):
    r = f % fs
    return min(r, fs - r)


# ---------------------------------------------------------------- states

@dataclass
class StateChar:
    state: str
    label: str
    axis: str
    saturated: bool
    floor_ft: float | None
    lowf_ft: dict
    dc_pt: float | None
    dc_sem_pt: float | None
    drift_pt_min: float | None


def characterize_state(state, label, axes_data, fs, f_sw, args, saturated):
    """Per-axis floor / low-f / DC for one steady state. Saturated states
    carry no spectra -- 'shield inadequate' IS the number."""
    chars = []
    for axis, x in axes_data.items():
        if saturated:
            chars.append(StateChar(state, label, axis, True, None, {},
                                   None, None, None))
            continue
        trim = 10.0 if state == "switching" else \
            (SETTLE_S if x.size / fs >= 700.0 else 60.0)
        x = x[int(trim * fs):]
        comb_lines = tuple(k * f_sw for k in range(1, MAX_HARMONIC + 1)) \
            if (state == "switching" and f_sw) else ()
        floor = band_floor(x, fs, *args.band, extra_lines=comb_lines) * 1e3
        lowf = {}
        f_sp, a_sp = welch_asd(x, fs, min(x.size, 200.0 * fs))
        for f0 in (0.1, 0.5, 1.0):
            if f0 < 10.0 / (x.size / fs):
                continue
            m = (f_sp >= f0 / 1.25) & (f_sp <= f0 * 1.25) & (f_sp > 0)
            if m.any():
                lowf[f"{f0:g}"] = float(np.median(a_sp[m])) * 1e3
        dc, sem, drift = block_stats(x, fs)
        chars.append(StateChar(state, label, axis, False, floor, lowf,
                               dc, sem, drift))
    return chars


def dc_shifts(chars):
    out = []
    off = {c.axis: c for c in chars if c.state == "off" and not c.saturated}
    on = {c.axis: c for c in chars if c.state == "on" and not c.saturated}
    for axis in sorted(set(off) & set(on)):
        shift = on[axis].dc_pt - off[axis].dc_pt
        err = float(np.hypot(on[axis].dc_sem_pt, off[axis].dc_sem_pt))
        entry = {"axis": axis, "shift_pt": shift, "err_pt": err}
        if abs(shift) > DC_SOFT_PT:
            entry["caveat"] = ("shifted operating point: the gain is a few % "
                               "soft here (quasi-linear +/-1 nT)")
        out.append(entry)
    return out


def added_noise(chars, band):
    out = []
    off = {c.axis: c for c in chars if c.state == "off" and not c.saturated}
    on = {c.axis: c for c in chars if c.state == "on" and not c.saturated}
    for axis in sorted(set(off) & set(on)):
        excess = float(np.sqrt(max(0.0, on[axis].floor_ft ** 2
                                   - off[axis].floor_ft ** 2)))
        out.append({"axis": axis, "band": list(band), "excess_ft": excess})
    return out


# ---------------------------------------------------------------- comb

def comb_census(x, fs, f_sw, band):
    """Amplitudes at k*f_sw up to MAX_HARMONIC, plus folded positions of
    higher harmonics above Nyquist that show up as lines."""
    nyq = fs / 2.0
    harmonics = []
    for k in range(1, MAX_HARMONIC + 1):
        fk = k * f_sw
        if fk >= 0.98 * nyq:
            break
        if any(abs(fk - h) < 0.7 for h in (*KNOWN_LINES_HZ, 100.0, 150.0)):
            harmonics.append({"k": k, "f": fk, "amp_ft": None,
                              "skipped": "too close to a known line"})
            continue
        fc, amp, noise = line_fit(x, fs, fk)
        harmonics.append({"k": k, "f": round(fc, 3),
                          "amp_ft": round(amp * 1e3, 1),
                          "detected": bool(amp > 5.0 * noise),
                          "in_band": bool(band[0] <= fk <= band[1]),
                          "folded": False})
    # high harmonics of the switching edge alias back below Nyquist;
    # annotate observed lines that sit at a folded k*f_sw position
    from scipy.ndimage import median_filter
    f, a = welch_asd(x, fs, 10.0 * fs)
    base = median_filter(a, size=201)
    strong = np.where((f > band[1]) & (a > 5.0 * base))[0]
    seen = set()
    for j in strong:
        fl = float(f[j])
        if any(abs(fl - h["f"]) < 1.0 for h in harmonics if h.get("f")):
            continue
        if any(abs(fl - h) < 1.0 for h in KNOWN_LINES_HZ):
            continue
        # a direct harmonic beyond the reported table is not "folded"
        k_direct = round(fl / f_sw)
        if k_direct >= 1 and k_direct * f_sw <= nyq \
                and abs(k_direct * f_sw - fl) < 0.5:
            continue
        for k in range(MAX_HARMONIC + 1, int(5 * fs / f_sw)):
            if k * f_sw > nyq and abs(fold(k * f_sw, fs) - fl) < 0.5:
                key = round(fl, 0)
                if key not in seen:
                    seen.add(key)
                    fc, amp, _ = line_fit(x, fs, fl)
                    harmonics.append({"k": k, "f": round(fc, 3),
                                      "amp_ft": round(amp * 1e3, 1),
                                      "detected": True, "in_band": False,
                                      "folded": True})
                break
    return harmonics


# ---------------------------------------------------------------- transient

def find_edges(aux, fs):
    th = 0.5 * (aux.max() + aux.min())
    up = np.where((aux[:-1] < th) & (aux[1:] >= th))[0] + 1
    down = np.where((aux[:-1] >= th) & (aux[1:] < th))[0] + 1
    return up, down


def edge_average(x, edges, n_pre, n_post):
    keep = edges[(edges > n_pre) & (edges < x.size - n_post)]
    if keep.size < 3:
        return None, 0
    snips = np.stack([x[e - n_pre:e + n_post] for e in keep])
    return snips.mean(axis=0), int(keep.size)


def transient_analysis(x, aux, fs, f_sw):
    """Synchronous edge averages: overshoot, t90 (within 10% of the new
    steady value) and a single-exponential tau. t90 * f_sw is the fraction
    of every switching period lost -- the experiment dead time."""
    from scipy.optimize import curve_fit
    T = 1.0 / f_sw
    up, down = find_edges(aux, fs)
    # the averaging window must end before the OPPOSITE edge arrives
    # (at 50% duty that is half a period, not 0.8 of one)
    all_edges = np.sort(np.concatenate([up, down]))
    gap = float(np.median(np.diff(all_edges))) if all_edges.size > 2 \
        else T * fs
    n_pre = int(min(0.2 * T * fs, 0.5 * gap))
    n_post = int(min(0.8 * T * fs, 0.9 * gap))
    out = []
    for name, edges in (("rising", up), ("falling", down)):
        avg, n = edge_average(x, edges, n_pre, n_post)
        if avg is None:
            continue
        t = (np.arange(avg.size) - n_pre) / fs
        steady = float(avg[-max(4, avg.size // 5):].mean())
        pre = float(avg[:int(0.75 * n_pre)].mean())
        step = steady - pre
        if abs(step) < 1e-9:
            continue
        dev = np.abs(avg - steady)
        overshoot = float(np.max(avg[n_pre:] - steady) if step > 0
                          else np.max(steady - avg[n_pre:]))
        outside = np.where(dev[n_pre:] > 0.1 * abs(step))[0]
        t90 = float(outside[-1] + 1) / fs if outside.size else 0.0
        tau = None
        post = avg[n_pre:] - steady
        k0 = int(np.argmax(np.abs(post)))
        decay = np.abs(post[k0:])
        tt = np.arange(decay.size) / fs
        try:
            popt, _ = curve_fit(lambda a, A, tau_: A * np.exp(-a / tau_),
                                tt, decay, p0=[decay[0], 0.05], maxfev=10000)
            if 1e-4 < popt[1] < T:
                tau = float(popt[1])
        except Exception:
            pass
        out.append({"edge": name, "n_edges": n, "overshoot_pt": overshoot,
                    "t90_s": round(t90, 4),
                    "tau_s": round(tau, 4) if tau else None,
                    "avg": (t.tolist(), avg.tolist())})
    return out


# ---------------------------------------------------------------- pipeline

def load_states(args):
    """(state, label, axes_data, fs, meta, saturated) per state, from the
    manifest or from one timeline run."""
    out = []
    if args.manifest:
        with open(args.manifest, newline="") as fh:
            lines = [ln for ln in fh if not ln.lstrip().startswith("#")]
        rows = list(csv.DictReader(lines))
        require(rows, f"{args.manifest}: empty manifest")
        metas = []
        for rec in rows:
            run_dir = rec["run_dir"].strip()
            state = rec["state"].strip().lower()
            require(state in ("off", "on", "switching"),
                    f"unknown state {state!r} (off/on/switching)")
            f_sw = float(rec["f_sw_hz"]) if rec.get("f_sw_hz") else args.f_sw
            meta, session = find_meta(run_dir)
            check_run(meta, run_dir, args.force, args.allow_sim)
            metas.append((meta, run_dir))
            saturated = is_saturated(meta)
            # a saturated state still needs its axis names in the report
            axes_data = {axis: None for axis in meta["axes"]}
            aux = None
            if not saturated:
                fs = float(meta["fs_hz"])
                for axis in meta["axes"]:
                    axes_data[axis] = load_axis(run_dir, session, axis,
                                                meta, args)
                if args.mot_name in (meta.get("aux_map") or {}):
                    path = os.path.join(run_dir, f"{session}_raw.csv")
                    with open(path) as fh:
                        fh.readline()
                        names = fh.readline().strip().split(",")
                    data = np.genfromtxt(path, delimiter=",", skip_header=2)
                    aux = data[:, names.index(f"V_{args.mot_name}")]
            out.append({"state": state, "label": rec.get("label", ""),
                        "axes": axes_data, "aux": aux,
                        "fs": float(meta["fs_hz"]), "f_sw": f_sw,
                        "saturated": saturated})
        ref = metas[0][0]
        for meta, path in metas[1:]:
            for key in ("gain_mode", "vrange"):
                if meta.get(key) != ref.get(key):
                    print(f"[note] states mix {key}: {path}")
                    require(args.force, f"mixed {key} across states; "
                                        "--force to compare anyway")
        return out

    require(args.runs and args.timeline,
            "give --manifest, or one RUN with --timeline")
    require(len(args.runs) == 1, "timeline mode takes exactly one run")
    run_dir = args.runs[0]
    meta, session = find_meta(run_dir)
    check_run(meta, run_dir, args.force, args.allow_sim)
    fs = float(meta["fs_hz"])
    full = {axis: load_axis(run_dir, session, axis, meta, args)
            for axis in meta["axes"]}
    with open(args.timeline, newline="") as fh:
        lines = [ln for ln in fh if not ln.lstrip().startswith("#")]
    for rec in csv.DictReader(lines):
        s0, s1 = float(rec["t_start_s"]), float(rec["t_end_s"])
        state = rec["state"].strip().lower()
        seg = {axis: x[int(s0 * fs):int(s1 * fs)] for axis, x in full.items()}
        out.append({"state": state, "label": f"{s0:g}-{s1:g}s",
                    "axes": seg, "aux": None, "fs": fs, "f_sw": args.f_sw,
                    "saturated": False})
    print("[note] hand timeline: state segmentation only -- edge averaging "
          "needs the synchronous aux channel and is refused")
    return out


def run_analysis(args):
    states = load_states(args)
    names = {s["state"] for s in states}
    require("off" in names, "no 'off' state: the comparison needs a baseline")
    require(names & {"on", "switching"},
            "need at least one of 'on' / 'switching' besides 'off'")

    chars, spectra = [], []
    comb, transients = None, []
    f_sw_used = None
    for s in states:
        if s["saturated"]:
            print(f"[state] {s['state']:<10} SATURATED (rails in meta) -> "
                  "shield inadequate for this state at this position")
            chars += characterize_state(s["state"], s["label"], s["axes"],
                                        s["fs"], s["f_sw"], args, True)
            continue
        cs = characterize_state(s["state"], s["label"], s["axes"], s["fs"],
                                s["f_sw"], args, False)
        chars += cs
        lead = cs[0]
        extra = f"   f_sw {s['f_sw']:g} Hz" if s["state"] == "switching" \
                and s["f_sw"] else ""
        print(f"[state] {s['state']:<10} floor {lead.floor_ft:.0f} fT/rtHz   "
              f"DC({lead.axis}) {lead.dc_pt:+.0f} pT{extra}")
        for axis, x in s["axes"].items():
            trim = 10.0 if s["state"] == "switching" else 60.0
            f, a = welch_asd(x[int(trim * s['fs']):], s["fs"], 20.0 * s["fs"])
            spectra.append((s["state"], axis, f, a * 1e3))
        if s["state"] == "switching" and s["f_sw"]:
            f_sw_used = s["f_sw"]
            axis0 = sorted(s["axes"])[0]
            x = s["axes"][axis0][int(10.0 * s["fs"]):]
            comb = comb_census(x, s["fs"], s["f_sw"], args.band)
            if s["aux"] is not None:
                transients = transient_analysis(s["axes"][axis0], s["aux"],
                                                s["fs"], s["f_sw"])

    shifts = dc_shifts(chars)
    noise = added_noise(chars, args.band)
    for d in shifts:
        cav = f"   ({d['caveat']})" if "caveat" in d else ""
        print(f"[dc   ] shift on-off: {d['axis']} {d['shift_pt']:+.0f} +/- "
              f"{d['err_pt']:.0f} pT{cav}")
    for n in noise:
        print(f"[noise] MOT adds {n['excess_ft']:.0f} fT/rtHz on {n['axis']} "
              f"in {n['band'][0]:g}-{n['band'][1]:g} Hz (quadrature)")
    if comb:
        det = [h for h in comb if h.get("detected")]
        direct = ", ".join(f"{h['k']}({h['amp_ft']:.0f})" for h in det
                           if not h["folded"])
        print(f"[comb ] f_sw {f_sw_used:g} Hz harmonics k(amp fT): {direct}")
        for h in det:
            if h["folded"]:
                print(f"        obs {h['f']:.1f} Hz = FOLDED harmonic "
                      f"k={h['k']} ({h['k'] * f_sw_used:g} Hz aliased)")
    for tr in transients:
        dead = tr["t90_s"] * f_sw_used
        tau = f" (tau {tr['tau_s'] * 1e3:.0f} ms)" if tr["tau_s"] else ""
        print(f"[trans] {tr['edge']} t90 {tr['t90_s'] * 1e3:.0f} ms{tau}, "
              f"{tr['n_edges']} edges; dead time ~{dead:.2f} of each period "
              f"(= t90*f_sw, f_sw {f_sw_used:g} Hz)")
    if any(c.saturated for c in chars):
        print("[sum ] HEADLINE: at least one MOT state saturates the sensor "
              "-- the shield is inadequate for that state at this position")
    print("[note] a MOT-state line is not automatically magnetic coupling: "
          "the MOT also loads the lab (supplies, chillers, mains)")

    os.makedirs(args.outdir, exist_ok=True)
    summary = {
        "program": "mot_suscept.py",
        "generated": datetime.now().isoformat(timespec="seconds"),
        "inputs": {"manifest": args.manifest, "runs": args.runs,
                   "timeline": args.timeline,
                   "args": {k: v for k, v in vars(args).items()
                            if k not in ("runs", "test")}},
        "results": {
            "states": [{k: v for k, v in c.__dict__.items()} for c in chars],
            "dc_shift": shifts, "added_noise": noise,
            "comb": {"f_sw": f_sw_used, "harmonics": comb} if comb else None,
            "transient": [{k: v for k, v in tr.items() if k != "avg"}
                          for tr in transients],
            "saturated_states": sorted({c.state for c in chars
                                        if c.saturated}),
        },
        "warnings": [],
        "pass": None,
    }
    path = os.path.join(args.outdir, "mot_suscept_summary.json")
    with open(path, "w", newline="\n") as fh:
        json.dump(summary, fh, indent=1)
    print(f"[out ] {path}")

    if not args.no_figures and spectra:
        make_figures(args.outdir, spectra, comb, transients, args.band,
                     f_sw_used)
        print(f"[out ] figures in {args.outdir}")
    return summary


# ---------------------------------------------------------------- figures

def make_figures(outdir, spectra, comb, transients, band, f_sw):
    import matplotlib
    matplotlib.use("Agg")
    import matplotlib.pyplot as plt
    plt.rcParams.update({
        "figure.dpi": 150, "font.size": 11, "axes.titleweight": "bold",
        "axes.spines.top": False, "axes.spines.right": False,
        "axes.edgecolor": C_MUTE, "text.color": C_INK, "legend.frameon": False,
    })
    fig, ax = plt.subplots(figsize=(7.5, 4.5))
    seen = set()
    for state, axis, f, a_ft in spectra:
        m = f > 0
        label = state if state not in seen else None
        seen.add(state)
        ax.loglog(f[m], a_ft[m], lw=0.8, color=C_STATE.get(state, C_MUTE),
                  label=label)
    ax.axvspan(band[0], band[1], color="#E6E8EA", alpha=0.5, zorder=0)
    if comb:
        for h in comb:
            if h.get("detected"):
                ax.axvline(h["f"], color=C_STATE["switching"], lw=0.5,
                           alpha=0.4)
    ax.set_xlabel("frequency [Hz]")
    ax.set_ylabel("ASD [fT/rtHz]")
    ax.set_title("MOT states")
    ax.legend(fontsize=9)
    fig.savefig(os.path.join(outdir, "mot_asd_states.png"),
                bbox_inches="tight")
    plt.close(fig)

    if transients:
        fig, ax = plt.subplots(figsize=(6.8, 4.0))
        for tr, color in zip(transients, ("#2E6E8E", "#C0504D")):
            t, avg = tr["avg"]
            ax.plot(np.array(t) * 1e3, avg, lw=1.0, color=color,
                    label=f"{tr['edge']} (t90 {tr['t90_s'] * 1e3:.0f} ms)")
            ax.axvline(tr["t90_s"] * 1e3, color=color, ls=":", lw=0.8)
        ax.axvline(0, color=C_MUTE, lw=0.8)
        ax.set_xlabel("time from edge [ms]")
        ax.set_ylabel("field [pT] (edge average)")
        ax.set_title("switching transient")
        ax.legend(fontsize=9)
        fig.savefig(os.path.join(outdir, "mot_transient.png"),
                    bbox_inches="tight")
        plt.close(fig)


# ---------------------------------------------------------------- self-test

def one_pole(s, fs, tau):
    a = 1.0 - np.exp(-1.0 / (fs * tau))
    y = np.empty_like(s)
    acc = s[0]
    for i, v in enumerate(s):
        acc += (v - acc) * a
        y[i] = acc
    return y


def synth_mot_run(tmp, session, state, fs=300.0, seconds=400.0,
                  dc_off=1726.0, shift=120.0, excess_ft=17.0, f_sw=5.0,
                  tau=0.040, folded_line_hz=None, rail=0.0, rng=None):
    rng = rng if rng is not None else np.random.default_rng(SIM_SEED)
    n = int(fs * seconds)
    t = np.arange(n) / fs
    x = dc_off + rng.normal(0.0, 21e-3 * np.sqrt(fs / 2.0), n)
    aux = None
    if state == "on":
        x = x + shift + rng.normal(0.0, excess_ft * 1e-3 * np.sqrt(fs / 2.0), n)
    if state == "switching":
        sq = ((t * f_sw) % 1.0 < 0.5).astype(float)
        x = x + shift * one_pole(sq, fs, tau)
        aux = sq + rng.normal(0.0, 0.01, n)
        if folded_line_hz:
            x = x + 0.15 * np.sin(2 * np.pi * folded_line_hz * t)
    run = os.path.join(tmp, session)
    os.makedirs(run)
    head = (f"# QZFM shield acquisition | axis=Z session={session}\n"
            f"# start=2026-07-08T10:00:00 fs={fs:g} Hz gain=1x "
            f"cal=370.4 pT/V vrange=1.0 V\n# device=Dev2 ch=Dev2/ai0 "
            f"sensor=SIM source=simulate\n# one value per line, unit=pT")
    np.savetxt(os.path.join(run, f"{session}_Z_pT.txt"),
               x * (GAIN_MEASURED / 2.7), fmt="%.3f", header=head,
               comments="")
    names = ["z"] + (["mot"] if aux is not None else [])
    table = [np.arange(n), x * 2.624 / 1000.0]
    if aux is not None:
        table.append(aux)
    np.savetxt(os.path.join(run, f"{session}_raw.csv"),
               np.column_stack(table), fmt=["%d"] + ["%.6g"] * (len(names)),
               delimiter=",", header=f"# QZFM raw voltages | session="
               f"{session} start=x fs={fs:g}\nsample," +
               ",".join(f"V_{c}" for c in names), comments="")
    meta = {
        "start_iso": "2026-07-08T10:00:00", "session": session,
        "axes": ["z"], "channel_map": {"z": "Dev2/ai0"},
        "aux_map": {"mot": "Dev2/ai1"} if aux is not None else {},
        "fs_hz": fs, "n_samples": n, "duration_s": n / fs,
        "clock_elapsed_s": n / fs, "fs_effective_hz": fs,
        "gain_mode": "1x", "cal_v_per_nT_used": 2.7,
        "cal_pT_per_V": 1000.0 / 2.7, "vrange": 1.0, "device": "Dev2",
        "sensor_id": "SIM",
        "rail_fraction": {"z": rail},
        "qzfm_rail_fraction": {"z": 0.0},
        "n_nonfinite": {"z": 0},
        "flat_ptp_V": {"z": 0.02},
        "mux_skew_note": "simulated", "source": "simulate",
        "software": "qzfm_record.py", "software_legacy": "qzfm_analog.py",
        "topology": "1head", "status": "ok", "exit_code": 0,
        "error": None, "warnings": [], "notes": "mot_suscept --test",
    }
    with open(os.path.join(run, f"{session}.meta.json"), "w", newline="\n") as fh:
        json.dump(meta, fh, indent=1)
    return run


def write_manifest(tmp, rows, name="mot.csv"):
    path = os.path.join(tmp, name)
    with open(path, "w", newline="\n") as fh:
        fh.write("run_dir,state,f_sw_hz,label\n")
        for row in rows:
            fh.write(row + "\n")
    return path


def targs(tmp, extra):
    return build_parser().parse_args(
        [*extra, "--allow-sim", "--no-figures",
         "--outdir", os.path.join(tmp, "out")])


def test_shift_and_noise(ctx):
    rng = np.random.default_rng(SIM_SEED)
    off = synth_mot_run(ctx["tmp"], "off", "off", rng=rng)
    on = synth_mot_run(ctx["tmp"], "on", "on", rng=rng)
    # the switching run needs fs 1200 for the fold annotation, and an
    # f_sw that does NOT divide fs (else every folded position collides
    # with the direct-harmonic grid): fold(170 * 5.3 Hz) = fold(901) =
    # 299 Hz, which is 2.2 Hz off any direct multiple of 5.3
    # tau 15 ms so the response settles inside each half period (94 ms);
    # a tau near the half period would leave no steady level to reach
    sw = synth_mot_run(ctx["tmp"], "sw", "switching", fs=1200.0,
                       seconds=120.0, f_sw=5.3, tau=0.015,
                       folded_line_hz=299.0, rng=rng)
    man = write_manifest(ctx["tmp"], [f"{off},off,,base", f"{on},on,,base",
                                      f"{sw},switching,5.3,base"])
    s = run_analysis(targs(ctx["tmp"], ["--manifest", man]))
    d = s["results"]["dc_shift"][0]
    assert abs(d["shift_pt"] - 120.0) <= 3 * max(d["err_pt"], 1.0), d
    n = s["results"]["added_noise"][0]
    assert abs(n["excess_ft"] - 17.0) / 17.0 <= 0.20, n
    ctx["full"] = s


def test_comb(ctx):
    comb = ctx["full"]["results"]["comb"]
    assert comb and comb["f_sw"] == 5.3
    by_k = {h["k"]: h for h in comb["harmonics"] if h.get("amp_ft") is not None}
    assert by_k[1]["detected"] and by_k[3]["detected"], by_k
    assert not by_k[2]["detected"], f"even harmonic detected: {by_k[2]}"
    # square-wave 1/k envelope times the one-pole edge response (tau 15 ms)
    h = lambda f: 1.0 / np.sqrt(1.0 + (2 * np.pi * f * 0.015) ** 2)
    expected = (1.0 / 3.0) * h(3 * 5.3) / h(5.3)
    ratio = by_k[3]["amp_ft"] / by_k[1]["amp_ft"]
    assert abs(ratio - expected) / expected <= 0.3, \
        f"k3/k1 = {ratio:.3f}, expected ~{expected:.3f}"
    # the sampled square itself carries genuine folded high harmonics
    # (e.g. k=235 -> 45.5 Hz), so assert on the injected one specifically
    folded = [h for h in comb["harmonics"] if h.get("folded")]
    inj = [h for h in folded if abs(h["f"] - 299.0) < 1.0]
    assert inj and inj[0]["k"] == 170, folded


def test_transient(ctx):
    trs = ctx["full"]["results"]["transient"]
    edges = {t["edge"] for t in trs}
    assert edges == {"rising", "falling"}, edges
    for tr in trs:
        assert abs(tr["t90_s"] - 2.303 * 0.015) / (2.303 * 0.015) <= 0.20, tr


def test_saturated(ctx):
    rng = np.random.default_rng(SIM_SEED + 1)
    off = synth_mot_run(ctx["tmp"], "off2", "off", rng=rng)
    on = synth_mot_run(ctx["tmp"], "on2", "on", rail=0.5, rng=rng)
    man = write_manifest(ctx["tmp"], [f"{off},off,,x", f"{on},on,,x"],
                         "sat.csv")
    s = run_analysis(targs(ctx["tmp"], ["--manifest", man]))
    assert s["results"]["saturated_states"] == ["on"], s["results"]
    sat = [c for c in s["results"]["states"] if c["state"] == "on"][0]
    assert sat["saturated"] and sat["floor_ft"] is None, sat


def test_refusals(ctx):
    rng = np.random.default_rng(SIM_SEED + 2)
    on = synth_mot_run(ctx["tmp"], "on3", "on", rng=rng)
    man = write_manifest(ctx["tmp"], [f"{on},on,,x"], "only_on.csv")
    try:
        run_analysis(targs(ctx["tmp"], ["--manifest", man]))
    except SystemExit as e:
        assert e.code == 13, f"only-on manifest exited {e.code}, want 13"
    else:
        raise AssertionError("a manifest without 'off' must be refused")

    # hand timeline: segmentation works, edge averaging is refused
    run = synth_mot_run(ctx["tmp"], "tl", "off", seconds=400.0,
                        rng=np.random.default_rng(SIM_SEED + 3))
    tl = os.path.join(ctx["tmp"], "tl.csv")
    with open(tl, "w", newline="\n") as fh:
        fh.write("t_start_s,t_end_s,state\n0,200,off\n200,400,on\n")
    s = run_analysis(targs(ctx["tmp"], [run, "--timeline", tl]))
    assert s["results"]["transient"] == [], "hand timeline must not " \
                                            "produce edge averages"


def run_self_test():
    import tempfile
    tmp = tempfile.mkdtemp(prefix="mot_suscept_test_")
    print("=== mot_suscept --test: synthetic checks (temp dir, removed after) ===")
    ctx = {"tmp": tmp}
    try:
        for test in (test_shift_and_noise, test_comb, test_transient,
                     test_saturated, test_refusals):
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
    p.add_argument("runs", nargs="*", metavar="RUN",
                   help="one timeline run (state runs use --manifest)")
    p.add_argument("--manifest", default=None,
                   help="state CSV: run_dir,state,f_sw_hz,label")
    p.add_argument("--timeline", default=None,
                   help="hand-logged CSV t_start_s,t_end_s,state "
                        "(segmentation only, never edge averaging)")
    p.add_argument("--f-sw", type=float, default=None,
                   help="switching frequency in Hz")
    p.add_argument("--mot-name", default="mot",
                   help="aux edge/monitor channel name")
    p.add_argument("--gain-v-per-nt", type=float, default=GAIN_MEASURED,
                   help="true gain for the epoch correction / --raw")
    p.add_argument("--raw", action="store_true",
                   help="analyze raw volts instead of pT files")
    p.add_argument("--band", nargs=2, type=float, default=[0.5, 40.0],
                   metavar=("LO", "HI"), help="science band for the verdicts")
    p.add_argument("--outdir", default=os.path.dirname(os.path.abspath(__file__)),
                   help="where the summary and figures go")
    p.add_argument("--force", action="store_true",
                   help="analyze runs whose status is not ok")
    p.add_argument("--allow-sim", action="store_true",
                   help="accept runs recorded with --simulate")
    p.add_argument("--no-figures", action="store_true",
                   help="skip figures (headless quick check)")
    p.add_argument("--test", action="store_true",
                   help="offline self-test on synthetic runs")
    return p


def main(argv=None):
    args = build_parser().parse_args(argv)
    if args.test:
        return run_self_test()
    require(1.5 <= args.gain_v_per_nt <= 4.0,
            f"--gain-v-per-nt {args.gain_v_per_nt} implausible at 1x")
    require(args.band[0] < args.band[1], f"--band {args.band} is not a band")
    run_analysis(args)
    return 0


if __name__ == "__main__":
    sys.exit(main())
