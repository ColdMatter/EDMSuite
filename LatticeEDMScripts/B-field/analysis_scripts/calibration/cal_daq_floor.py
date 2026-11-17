#!/usr/bin/env python3
"""Characterize the bare USB-6008 chain from shorted-input runs.

With the QZFM disconnected and every differential pair shorted to AGND,
whatever remains is the DAQ itself. This measures the per-channel DC
offset, the voltage-noise floor, and the ADC code grid that the
quantization model rests on. Raw volts only: the pT files of a shorted
run carry a meaningless field conversion.
Writes cal_daq_floor_summary.json, cal_daq_floor.png and ledger lines.
Offline check: python cal_daq_floor.py --test
"""

from __future__ import annotations

import argparse
import glob
import json
import os
import shutil
import sys
from dataclasses import dataclass, asdict
from datetime import datetime

import numpy as np
from scipy.signal import welch
from scipy.optimize import curve_fit

GAIN_TABLE = {"0.33x": 0.9, "1x": 2.7, "3x": 8.1}  # V/nT, QuSpin nominal
GAIN_MEASURED = 2.624       # V/nT at 1x (sync5, 50 Hz two-readout method)
ADC_STEP_V = 512e-6         # measured code grid at +/-1 V (true FS +/-1.0486 V)
DAQ_FLOOR_SPEC = 20.4e-6    # V/rtHz: NI 0.5 mV rms system noise over 600 Hz
DAQ_FLOOR_QUANT = 6.0e-6    # V/rtHz: 512 uV / sqrt(12) / sqrt(600), quantization only
CELL_ALIAS_HZ = (277.1, 294.0)   # QZFM cell-modulation aliases at fs=1200
OFFSET_LIMIT_MV = 15.0      # post-repair reference offset was -14 mV
LINE_LIMIT = 10.0           # in-band line above this ratio fails the run
SIM_SEED = 20260707

import sys as _sys  # analysis_scripts/ on sys.path for _paths
_sys.path.insert(0, os.path.dirname(os.path.dirname(os.path.abspath(__file__))))
from _paths import DATA_ROOT, PROJECT_DIR, ANALYSIS_DIR, data, data_join, rel_data  # noqa: E402,F401
# run refs in the ledger are relative to the data root
REPO_ROOT = DATA_ROOT
DEFAULT_LEDGER = data("calibration", "ledger.jsonl")

C_LINES = ("#2E6E8E", "#5A7D3A", "#C0504D", "#8064A2")
C_INK, C_MUTE = "#33373B", "#7A7F85"


def require(ok, msg, code=13):
    if not ok:
        print(f"[error] {msg}", file=sys.stderr)
        raise SystemExit(code)


# ---------------------------------------------------------------- loading

def find_run_files(run_dir):
    metas = glob.glob(os.path.join(run_dir, "*.meta.json"))
    require(len(metas) == 1, f"{run_dir}: expected one meta.json, found {len(metas)}")
    session = os.path.basename(metas[0])[:-len(".meta.json")]
    raw = os.path.join(run_dir, f"{session}_raw.csv")
    require(os.path.exists(raw), f"{run_dir}: no {session}_raw.csv (need raw volts)")
    return metas[0], raw


from bfield.analysis.spectra import load_raw_csv  # noqa: E402 -- was an identical local copy


def phys_channel(meta, column):
    # 'z' -> 'ai0' via the channel/aux maps; fall back to the column name
    for key in ("channel_map", "aux_map", "channels"):
        phys = (meta.get(key) or {}).get(column)
        if phys:
            return phys.split("/")[-1]
    return column


def check_run(meta, force=False, allow_sim=False):
    """Acceptance gate, DAQ variant: flat channels and QZFM-rail entries are
    expected on a shorted input, so only run status, data health and the ADC
    rails are enforced."""
    if not (meta.get("status") == "ok" and meta.get("exit_code") == 0):
        require(force, f"run status={meta.get('status')} exit_code="
                       f"{meta.get('exit_code')}; --force to analyze anyway")
        print(f"[gate] overridden: status={meta.get('status')} "
              f"exit_code={meta.get('exit_code')}")
    nonfinite = meta.get("n_nonfinite") or {}
    require(all(v == 0 for v in nonfinite.values()),
            f"non-finite samples in run: {nonfinite}")
    rails = {k: v for k, v in (meta.get("rail_fraction") or {}).items() if v > 0.001}
    require(not rails, f"ADC rail fraction above 0.1%: {rails} "
                       "(calibration input must be clean, no --force here)")
    if meta.get("source") == "simulate":
        require(allow_sim, "run is simulated; pass --allow-sim if that is intended")
    fs, fse = meta.get("fs_hz"), meta.get("fs_effective_hz")
    if fs and fse and abs(fse / fs - 1.0) > 1e-3:
        print(f"[warn] fs_effective {fse:.4f} Hz is "
              f"{abs(fse / fs - 1.0) * 100:.2f}% off nominal {fs:g} Hz")


# ---------------------------------------------------------------- analysis

from bfield.analysis.spectra import welch_asd  # noqa: E402 -- was an identical local copy


def band_median(f, asd, lo, hi):
    # no line exclusion here: on a shorted input a mains line is a finding,
    # not something to hide
    m = (f >= lo) & (f <= hi)
    require(m.any(), f"no spectral bins in {lo}-{hi} Hz")
    return float(np.median(asd[m]))


def band_floor_scatter(x, fs, lo, hi, win_s=90.0, hop_s=30.0):
    """Median block floor and its SEM. Overlapping windows are correlated,
    so the std is divided by the count of NON-overlapping windows."""
    n, h = int(win_s * fs), int(hop_s * fs)
    floors = []
    for i in range(0, x.size - n + 1, h):
        f, a = welch_asd(x[i:i + n], fs, 20.0 * fs)
        floors.append(band_median(f, a, lo, hi))
    floors = np.asarray(floors)
    n_eff = max(1.0, (x.size / fs) / win_s)
    return float(np.median(floors)), float(floors.std(ddof=1) / np.sqrt(n_eff))


def block_stats(x, fs, block_s=60.0):
    """DC and drift with honest low-frequency errors (never the naive std
    of the mean -- samples are correlated). Returns
    (mean, sem, drift_per_min, drift_sem_per_min, n_blocks)."""
    n = int(block_s * fs)
    nb = x.size // n
    require(nb >= 3, f"record too short for block stats ({nb} blocks)")
    means = x[:nb * n].reshape(nb, n).mean(axis=1)
    tmin = (np.arange(nb) + 0.5) * block_s / 60.0
    mean = float(means.mean())
    sem = float(means.std(ddof=1) / np.sqrt(nb))
    A = np.vstack([tmin, np.ones(nb)]).T
    coef, res, *_ = np.linalg.lstsq(A, means, rcond=None)
    resid = means - A @ coef
    s2 = float(resid @ resid) / max(nb - 2, 1)
    cov00 = s2 * np.linalg.inv(A.T @ A)[0, 0]
    return mean, sem, float(coef[0]), float(np.sqrt(cov00)), nb


def find_lines(x, fs, flo=1.0, fhi=600.0, ratio=5.0):
    """Spectral lines standing more than `ratio` above the local floor."""
    from scipy.ndimage import median_filter
    f, a = welch_asd(x, fs, 10.0 * fs)
    base = median_filter(a, size=201)
    idx = np.where((f >= flo) & (f <= fhi) & (a > ratio * base))[0]
    lines = []
    while idx.size:
        grp = idx[idx <= idx[0] + int(1.0 / (f[1] - f[0]))]
        j = grp[np.argmax(a[grp])]
        lines.append((float(f[j]), float(a[j]), float(a[j] / base[j])))
        idx = idx[idx > grp[-1]]
    lines.sort(key=lambda r: -r[2])
    return lines


def line_note(freq):
    if abs(freq - 50.0) < 0.5:
        return "mains pickup (ground loop -- on a shorted input this is a wiring problem)"
    if abs(freq - 59.917) < 0.5:
        return "external comb (unidentified, seen in sync5 too)"
    if abs(freq - 500.0) < 0.5:
        return "USB frame artifact"
    return ""


def detect_code_grid(v):
    """LSB step, implied full scale, and missing-code fraction, from the
    voltage values themselves. The step is the most common gap between
    distinct values; full scale assumes a 12-bit signed range (2048 codes
    per side). Missing codes are counted over the central 80% of the
    occupied range only -- the tails of a narrow signal are empty by
    statistics, not by converter fault."""
    vals = np.unique(v)
    require(vals.size >= 10, "too few distinct ADC codes to detect the grid")
    d = np.diff(vals)
    d = d[d < 5e-3]
    require(d.size > 0, "no code gaps below 5 mV; not a raw ADC record?")
    hist, edges = np.histogram(d, bins=np.arange(0.0, 5e-3 + 1e-6, 1e-6))
    j = int(np.argmax(hist))
    q = float(np.median(d[(d >= edges[j]) & (d < edges[j + 1])]))
    full_scale = 2048.0 * q
    codes = np.round(vals / q).astype(int)
    span = codes.max() - codes.min()
    lo = codes.min() + int(0.1 * span)
    hi = codes.max() - int(0.1 * span)
    grid = np.arange(lo, hi + 1)
    missing = float(1.0 - np.isin(grid, codes).mean()) if grid.size else 0.0
    return q, full_scale, missing


def fit_1f_corner(x, fs, lo=0.01, hi=40.0):
    """Fit asd(f) = sqrt(w^2 + (A/f^alpha)^2) in log space; return the
    frequency where the flicker term crosses the white floor, or None when
    the data show no 1/f inside the fit band."""
    f, a = welch_asd(x, fs, 200.0 * fs)
    m = (f >= lo) & (f <= hi) & (a > 0)
    if m.sum() < 20:
        return None
    logf, loga = np.log(f[m]), np.log(a[m])
    w0 = band_median(f, a, 5.0, hi)

    def logmodel(lf, w, A, alpha):
        ff = np.exp(lf)
        return 0.5 * np.log(w ** 2 + (A / ff ** alpha) ** 2)

    try:
        popt, _ = curve_fit(logmodel, logf, loga,
                            p0=[w0, a[m][0] * f[m][0], 0.8],
                            bounds=([1e-12, 1e-15, 0.05], [1.0, 1.0, 3.0]),
                            maxfev=40000)
    except Exception:
        return None
    w, A, alpha = popt
    if alpha < 0.1:
        return None
    corner = (A / w) ** (1.0 / alpha)
    return float(corner) if lo <= corner <= hi else None


@dataclass
class ChannelResult:
    name: str                 # DAQ channel, ai0..ai3
    column: str               # logical name in raw.csv (z, y, mon, ...)
    vrange: float
    offset_mv: float
    offset_sem_mv: float
    drift_uv_min: float
    q_uv: float
    full_scale_v: float
    missing_code_frac: float
    floor_inband_uv: float
    floor_inband_sem_uv: float
    floor_hi_uv: float
    floor_inband_ft: float
    floor_hi_ft: float
    corner_1f_hz: float | None
    worst_line: list | None   # [Hz, uV/rtHz, ratio, note]
    pass_offset: bool
    pass_floor: bool
    pass_lines: bool


def analyze_channel(name, column, v, fs, vrange, gain, band, hi_band):
    v = v[int(fs):]                      # drop the first 1 s (task startup)
    q, full_scale, missing = detect_code_grid(v)

    mean_v, sem_v, drift_v_min, _, _ = block_stats(v, fs)
    # pT = V * 1000 / g, so 1 mV = 1/g pT (0.381 pT at g = 2.624)

    f, a = welch_asd(v, fs, 20.0 * fs)
    floor_v = band_median(f, a, *band)
    floor_hi_v = band_median(f, a, *hi_band)
    _, floor_sem_v = band_floor_scatter(v, fs, *band)
    # V/rtHz over V/nT is nT/rtHz; nT -> fT is 1e6, and V -> uV is 1e6,
    # so fT-equiv = uV/rtHz / g  (20.4 uV / 2.624 -> 7.77 fT)
    uv = 1e6

    lines = find_lines(v, fs, flo=1.0, fhi=min(600.0, 0.99 * fs / 2))
    for freq, _, _ in lines:
        require(all(abs(freq - fa) > 0.5 for fa in CELL_ALIAS_HZ),
                f"{name}: cell-modulation alias at {freq:.1f} Hz; "
                "is the QZFM actually disconnected?")
    worst = None
    if lines:
        freq, amp, ratio = lines[0]
        worst = [freq, amp * uv, ratio, line_note(freq)]
    in_band_bad = [ln for ln in lines
                   if band[0] <= ln[0] <= band[1] and ln[2] > LINE_LIMIT]

    # the model constants are the +/-1 V values; both scale with the range
    quant_uv = DAQ_FLOOR_QUANT * uv * vrange
    spec_uv = DAQ_FLOOR_SPEC * uv * vrange

    return ChannelResult(
        name=name, column=column, vrange=vrange,
        offset_mv=mean_v * 1e3, offset_sem_mv=sem_v * 1e3,
        drift_uv_min=drift_v_min * uv,
        q_uv=q * uv, full_scale_v=full_scale, missing_code_frac=missing,
        floor_inband_uv=floor_v * uv, floor_inband_sem_uv=floor_sem_v * uv,
        floor_hi_uv=floor_hi_v * uv,
        floor_inband_ft=floor_v * uv / gain, floor_hi_ft=floor_hi_v * uv / gain,
        corner_1f_hz=fit_1f_corner(v, fs),
        worst_line=worst,
        pass_offset=abs(mean_v * 1e3) <= OFFSET_LIMIT_MV,
        pass_floor=(0.5 * quant_uv <= floor_v * uv <= 2.0 * spec_uv),
        pass_lines=not in_band_bad,
    ), (f, a * uv)


# ---------------------------------------------------------------- ledger

def ledger_append(record, path):
    os.makedirs(os.path.dirname(path), exist_ok=True)
    record = {"date": datetime.now().isoformat(timespec="seconds"), **record}
    line = json.dumps(record, ensure_ascii=False)
    with open(path, "a", newline="\n") as fh:
        fh.write(line + "\n")
    print(f"[ledger] {line}")
    return record


# ---------------------------------------------------------------- report

def report_channel(r, gain):
    verdict = lambda ok: "PASS" if ok else "FAIL"
    print(f"[grid] {r.name}: q={r.q_uv:.1f} uV  FS=+/-{r.full_scale_v:.4f} V"
          f"  missing-codes {r.missing_code_frac * 100:.1f}%")
    print(f"[dc  ] {r.name}: offset {r.offset_mv:+.2f} mV "
          f"({r.offset_mv / gain:+.2f} pT-equiv @{gain:g})  "
          f"drift {r.drift_uv_min:+.2f} uV/min   {verdict(r.pass_offset)}")
    print(f"[asd ] {r.name}: in-band {r.floor_inband_uv:.1f} +/- "
          f"{r.floor_inband_sem_uv:.1f} uV/rtHz ({r.floor_inband_ft:.2f} fT-equiv)"
          f"   hi-band {r.floor_hi_uv:.1f} uV/rtHz")
    if r.corner_1f_hz is not None:
        print(f"[1/f ] {r.name}: white+flicker corner ~ {r.corner_1f_hz:.2f} Hz")
    else:
        print(f"[1/f ] {r.name}: no 1/f component detected")
    if r.worst_line:
        freq, amp, ratio, note = r.worst_line
        tail = f"  <- {note}" if note else ""
        print(f"[line] {r.name}: worst {freq:.1f} Hz  {amp:.0f} uV/rtHz "
              f"({ratio:.0f}x){tail}   {verdict(r.pass_lines)}(<{LINE_LIMIT:g}x in band)")
    else:
        print(f"[line] {r.name}: no lines above 5x the local floor   PASS")
    print(f"[ver ] {r.name}: floor vs models  quant "
          f"{DAQ_FLOOR_QUANT * 1e6 * r.vrange:.1f} / measured {r.floor_inband_uv:.1f}"
          f" / NI-spec {DAQ_FLOOR_SPEC * 1e6 * r.vrange:.1f} uV/rtHz"
          f"   {verdict(r.pass_floor)}")


def range_comparison(per_run):
    # channels grouped by name across runs; the offset should not move with
    # the range, q and the floor should scale with it
    rows = []
    names = sorted({r.name for chans, _ in per_run for r in chans})
    print("=== range comparison ===")
    for name in names:
        entries = [r for chans, _ in per_run for r in chans if r.name == name]
        offs = [r.offset_mv for r in entries]
        stable = max(offs) - min(offs) <= 5.0
        note = "stable" if stable else "MOVES ACROSS RANGES -- front-end problem"
        print(f"[cmp ] {name} offset " + " / ".join(f"{o:+.1f}" for o in offs)
              + f" mV ({note})   q " + " / ".join(f"{r.q_uv:.0f}" for r in entries)
              + " uV   floor " + " / ".join(f"{r.floor_inband_ft:.1f}" for r in entries)
              + " fT-equiv")
        rows.append({"name": name, "offset_mv": offs, "offset_stable": stable,
                     "q_uv": [r.q_uv for r in entries],
                     "vrange": [r.vrange for r in entries],
                     "floor_inband_ft": [r.floor_inband_ft for r in entries]})
    return rows


def make_figure(path, spectra, gap_samples_uv, q_uv, gain, vrange):
    import matplotlib
    matplotlib.use("Agg")
    import matplotlib.pyplot as plt
    plt.rcParams.update({
        "figure.dpi": 150, "font.size": 11, "axes.titleweight": "bold",
        "axes.spines.top": False, "axes.spines.right": False,
        "axes.edgecolor": C_MUTE, "text.color": C_INK,
        "legend.frameon": False,
    })
    fig, (ax, axh) = plt.subplots(1, 2, figsize=(11.5, 4.2))
    fig.subplots_adjust(wspace=0.42)

    for (label, f, a_uv), color in zip(spectra, C_LINES * 4):
        m = f > 0
        ax.loglog(f[m], a_uv[m], color=color, lw=0.9, label=label)
    note_bbox = dict(boxstyle="round,pad=0.15", facecolor="white",
                     edgecolor="none", alpha=0.9)
    for level, name in ((DAQ_FLOOR_QUANT * 1e6 * vrange, "quantization"),
                        (DAQ_FLOOR_SPEC * 1e6 * vrange, "NI spec")):
        ax.axhline(level, color=C_MUTE, ls="--", lw=0.9)
        ax.annotate(f"{name} {level:.1f}", (ax.get_xlim()[1] * 0.5, level),
                    fontsize=9, color=C_MUTE, ha="right", va="bottom",
                    bbox=note_bbox)
    ax.set_xlabel("frequency [Hz]")
    ax.set_ylabel("ASD [uV/rtHz]")
    ax.set_title("shorted-input noise floor")
    sec = ax.secondary_yaxis("right", functions=(lambda u: u / gain,
                                                 lambda t: t * gain))
    sec.set_ylabel(f"fT-equiv at {gain:g} V/nT")
    ax.legend(loc="lower left", fontsize=9)

    axh.hist(gap_samples_uv, bins=np.arange(0, 2000, 8), color=C_LINES[0])
    axh.axvline(q_uv, color=C_LINES[2], ls="--", lw=1.0)
    axh.annotate(f"q = {q_uv:.0f} uV", (q_uv + 60, axh.get_ylim()[1] * 0.9),
                 color=C_LINES[2], fontsize=10, ha="left")
    axh.set_xlabel("gap between adjacent ADC values [uV]")
    axh.set_ylabel("count")
    axh.set_title("code grid")
    fig.savefig(path, bbox_inches="tight")
    plt.close(fig)


# ---------------------------------------------------------------- pipeline

def run_analysis(args):
    gain = args.gain_v_per_nt
    per_run = []           # (channel results, run label) per run dir
    spectra, warnings = [], []
    gap_pool, q_first = [], None

    for run_dir in args.run_dirs:
        meta_path, raw_path = find_run_files(run_dir)
        with open(meta_path, encoding="utf-8") as fh:
            meta = json.load(fh)
        check_run(meta, args.force, args.allow_sim)
        fs = float(meta["fs_hz"])
        vrange = float(meta.get("vrange", 1.0))
        require(args.band[0] < args.band[1] <= fs / 2,
                f"--band {args.band} does not fit below Nyquist {fs / 2:g} Hz")
        hi_band = (args.hi_band[0], min(args.hi_band[1], 0.99 * fs / 2))

        print(f"=== cal_daq_floor: {run_dir} (vrange +/-{vrange:g} V) ===")
        cols = load_raw_csv(raw_path)
        chans = []
        for column, v in cols.items():
            name = phys_channel(meta, column)
            r, (f, a_uv) = analyze_channel(name, column, v, fs, vrange, gain,
                                           tuple(args.band), hi_band)
            report_channel(r, gain)
            chans.append(r)
            spectra.append((f"{name} ({run_dir.rstrip('/').split(os.sep)[-1]})",
                            f, a_uv))
            vals = np.unique(v)
            gap_pool.append(np.diff(vals)[np.diff(vals) < 5e-3] * 1e6)
            if q_first is None:
                q_first = r.q_uv
        per_run.append((chans, meta))

        run_ref = os.path.abspath(run_dir)
        if run_ref.startswith(REPO_ROOT + os.sep):
            run_ref = os.path.relpath(run_ref, REPO_ROOT)
        simulated = meta.get("source") == "simulate"
        if simulated and os.path.abspath(args.ledger) == os.path.abspath(DEFAULT_LEDGER):
            print("[note] simulated run: not writing to the shared ledger")
        else:
            for r in chans:
                # each row carries its own verdict: a FAILED offset
                # used to enter the ledger unmarked, where
                # cal_stability reads it as a usable value
                # (2026-08-19 analysis review, H3)
                base = {"run": run_ref,
                        "channel": r.name, "tool": "cal_daq_floor.py",
                        "gain_mode": args.gain_mode,
                        "conditions": f"shorted input, vrange {vrange:g} V"}
                ledger_append({"kind": "daq_offset", "value": round(r.offset_mv, 3),
                               "unc": round(r.offset_sem_mv, 4), "unit": "mV",
                               "pass": bool(r.pass_offset), **base},
                              args.ledger)
                ledger_append({"kind": "daq_floor", "value": round(r.floor_inband_ft, 3),
                               "unc": round(r.floor_inband_sem_uv / gain, 3),
                               "unit": "fT/rtHz",
                               "pass": bool(r.pass_floor and r.pass_lines),
                               **base}, args.ledger)

    comparison = range_comparison(per_run) if len(per_run) > 1 else []

    all_chans = [r for chans, _ in per_run for r in chans]
    overall = all(r.pass_offset and r.pass_floor and r.pass_lines
                  for r in all_chans)
    print(f"[sum ] {len(all_chans)} channel(s), "
          f"overall {'PASS' if overall else 'FAIL'}")

    os.makedirs(args.outdir, exist_ok=True)
    summary = {
        "program": "cal_daq_floor.py",
        "generated": datetime.now().isoformat(timespec="seconds"),
        "inputs": {"runs": list(args.run_dirs),
                   "args": {k: v for k, v in vars(args).items()
                            if k not in ("run_dirs", "test")}},
        "results": {
            "channels": [asdict(r) for r in all_chans],
            "models_uv_per_rthz": {"quantization": DAQ_FLOOR_QUANT * 1e6,
                                   "ni_spec": DAQ_FLOOR_SPEC * 1e6},
            "range_comparison": comparison,
        },
        "warnings": warnings,
        "pass": overall,
    }
    out_json = os.path.join(args.outdir, "cal_daq_floor_summary.json")
    with open(out_json, "w", newline="\n") as fh:
        json.dump(summary, fh, indent=1)
    print(f"[out ] {out_json}")

    if not args.no_figures and spectra:
        fig_path = os.path.join(args.outdir, "cal_daq_floor.png")
        vr = float(per_run[0][1].get("vrange", 1.0))
        make_figure(fig_path, spectra, np.concatenate(gap_pool), q_first,
                    gain, vr)
        print(f"[out ] {fig_path}")
    return summary


# ---------------------------------------------------------------- self-test

def quantize(x, q):
    return np.round(x / q) * q


def write_sim_run(tmp, session, chans, fs=1200.0, vrange=1.0):
    """Write a run dir in the exact acquisition format (raw.csv + meta.json)."""
    run = os.path.join(tmp, session)
    os.makedirs(run)
    names = list(chans)
    n = len(next(iter(chans.values())))
    header = f"# QZFM raw voltages | session={session} start=2026-07-08T09:00:00 fs={fs:g}\n"
    header += "sample," + ",".join(f"V_{c}" for c in names)
    table = np.column_stack([np.arange(n), *[chans[c] for c in names]])
    np.savetxt(os.path.join(run, f"{session}_raw.csv"), table,
               fmt=["%d"] + ["%.6g"] * len(names), delimiter=",",
               header=header, comments="")
    channel_map = {c: f"Dev2/ai{i}" for i, c in enumerate(names)}
    meta = {
        "start_iso": "2026-07-08T09:00:00", "session": session,
        "axes": names[:2], "channel_map": channel_map,
        "aux_map": {c: channel_map[c] for c in names[2:]},
        "fs_hz": fs, "n_samples": n, "duration_s": n / fs,
        "clock_elapsed_s": n / fs, "fs_effective_hz": fs,
        "gain_mode": "1x", "cal_v_per_nT_used": 2.7,
        "cal_pT_per_V": 1000.0 / 2.7, "vrange": vrange, "device": "Dev2",
        "sensor_id": "none (shorted input)",
        "rail_fraction": {c: 0.0 for c in names},
        "qzfm_rail_fraction": {c: 0.0 for c in names},
        "n_nonfinite": {c: 0 for c in names},
        "flat_ptp_V": {c: 0.004 for c in names},
        "mux_skew_note": "USB-6008 multiplexed; <135 Hz negligible",
        "source": "simulate", "software": "qzfm_record.py",
        "topology": "1head", "software_legacy": "qzfm_analog.py",
        "status": "ok", "exit_code": 0, "error": None, "warnings": [],
        "notes": "synthetic shorted run for cal_daq_floor --test",
    }
    with open(os.path.join(run, f"{session}.meta.json"), "w", newline="\n") as fh:
        json.dump(meta, fh, indent=1)
    return run


def sim_short(n, fs, rng, offset_v=-14e-3, drift_uv_min=0.1, noise_v=0.5e-3):
    t_min = np.arange(n) / fs / 60.0
    x = offset_v + drift_uv_min * 1e-6 * t_min + rng.normal(0.0, noise_v, n)
    return x


def test_args(tmp, run, **over):
    argv = [run, "--allow-sim", "--no-figures",
            "--outdir", os.path.join(tmp, "out"),
            "--ledger", os.path.join(tmp, "ledger.jsonl")]
    for k, v in over.items():
        argv += [f"--{k}", str(v)]
    return build_parser().parse_args(argv)


def test_offset(ctx):
    r = ctx["base"]["results"]["channels"][0]
    err_uv = abs(r["offset_mv"] - (-14.0)) * 1e3
    assert err_uv <= 2.0, f"offset off by {err_uv:.2f} uV (want <= 2)"


def test_grid(ctx):
    r = ctx["base"]["results"]["channels"][0]
    assert abs(r["q_uv"] - 512.0) <= 1.0, f"q = {r['q_uv']:.2f} uV, want 512"
    assert abs(r["full_scale_v"] - 1.0486) <= 1e-3, \
        f"FS = {r['full_scale_v']:.4f} V, want 1.0486"


def test_floor(ctx):
    # reference floor measured on the unquantized noise, not the formula:
    # the generator's realized floor is what the program should recover
    f, a = welch_asd(ctx["unquantized"], 1200.0, 20.0 * 1200.0)
    ref_uv = band_median(f, a, 3.0, 40.0) * 1e6
    r = ctx["base"]["results"]["channels"][0]
    rel = abs(r["floor_inband_uv"] - ref_uv) / ref_uv
    assert rel <= 0.15, f"floor {r['floor_inband_uv']:.1f} vs ref {ref_uv:.1f} uV"


def test_ledger(ctx):
    with open(ctx["ledger"]) as fh:
        records = [json.loads(line) for line in fh]
    kinds = sorted(r["kind"] for r in records)
    assert kinds == ["daq_floor"] * 4 + ["daq_offset"] * 4, f"ledger kinds: {kinds}"
    for r in records:
        for key in ("date", "kind", "value", "unc", "unit", "channel", "tool"):
            assert key in r, f"ledger record missing {key}: {r}"


def test_mains_line(ctx):
    rng = np.random.default_rng(SIM_SEED + 1)
    fs, n = 1200.0, int(240 * 1200)
    t = np.arange(n) / fs
    v = quantize(sim_short(n, fs, rng) + 0.2e-3 * np.sin(2 * np.pi * 50.0 * t),
                 ADC_STEP_V)
    run = write_sim_run(ctx["tmp"], "mains", {"z": v})
    summary = run_analysis(test_args(ctx["tmp"], run))
    worst = summary["results"]["channels"][0]["worst_line"]
    assert worst and abs(worst[0] - 50.0) <= 0.5, f"worst line {worst}"
    assert "mains" in worst[3], f"line note: {worst[3]!r}"


def test_alias_refusal(ctx):
    rng = np.random.default_rng(SIM_SEED + 2)
    fs, n = 1200.0, int(240 * 1200)
    t = np.arange(n) / fs
    v = quantize(sim_short(n, fs, rng) + 0.2e-3 * np.sin(2 * np.pi * 277.1 * t),
                 ADC_STEP_V)
    run = write_sim_run(ctx["tmp"], "alias", {"z": v})
    try:
        run_analysis(test_args(ctx["tmp"], run))
    except SystemExit as e:
        assert e.code == 13, f"alias run exited {e.code}, want 13"
    else:
        raise AssertionError("a cell-modulation alias must be refused (exit 13)")


def test_acceptance(ctx):
    rng = np.random.default_rng(SIM_SEED + 3)
    fs, n = 1200.0, int(240 * 1200)
    v = quantize(sim_short(n, fs, rng, offset_v=+30e-3), ADC_STEP_V)
    run = write_sim_run(ctx["tmp"], "bigoffset", {"z": v})
    summary = run_analysis(test_args(ctx["tmp"], run))
    r = summary["results"]["channels"][0]
    assert not r["pass_offset"], "a +30 mV offset must fail the offset check"
    assert summary["pass"] is False, "summary pass must be false"


def run_self_test():
    import tempfile
    tmp = tempfile.mkdtemp(prefix="cal_daq_floor_test_")
    print("=== cal_daq_floor --test: synthetic checks (temp dir, removed after) ===")
    try:
        rng = np.random.default_rng(SIM_SEED)
        fs, n = 1200.0, int(900 * 1200)
        clean = sim_short(n, fs, rng)
        chans = {c: quantize(sim_short(n, fs, rng), ADC_STEP_V)
                 for c in ("y", "m2", "m3")}
        chans = {"z": quantize(clean, ADC_STEP_V), **chans}
        run = write_sim_run(tmp, "base", chans)
        ctx = {"tmp": tmp, "unquantized": clean[int(fs):],
               "ledger": os.path.join(tmp, "ledger.jsonl")}
        ctx["base"] = run_analysis(test_args(tmp, run))
        for test in (test_offset, test_grid, test_floor, test_ledger,
                     test_mains_line, test_alias_refusal, test_acceptance):
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
    p.add_argument("run_dirs", nargs="*", metavar="RUN_DIR",
                   help="shorted-input run dir(s), one per vrange")
    p.add_argument("--gain-mode", default="1x", choices=sorted(GAIN_TABLE),
                   help="QZFM gain the fT-equiv numbers assume (default 1x)")
    p.add_argument("--gain-v-per-nt", type=float, default=GAIN_MEASURED,
                   help="V/nT used for the fT-equiv conversion (default 2.624)")
    p.add_argument("--band", nargs=2, type=float, default=[3.0, 40.0],
                   metavar=("LO", "HI"), help="in-band floor window in Hz")
    p.add_argument("--hi-band", nargs=2, type=float, default=[100.0, 500.0],
                   metavar=("LO", "HI"), help="wideband floor window in Hz")
    p.add_argument("--outdir", default=os.path.dirname(os.path.abspath(__file__)),
                   help="where the summary and figure go")
    p.add_argument("--ledger", default=DEFAULT_LEDGER,
                   help="calibration ledger path")
    p.add_argument("--force", action="store_true",
                   help="analyze runs whose status is not ok")
    p.add_argument("--allow-sim", action="store_true",
                   help="accept runs recorded with --simulate")
    p.add_argument("--no-figures", action="store_true",
                   help="skip the figure (headless quick check)")
    p.add_argument("--test", action="store_true",
                   help="offline self-test on synthetic runs")
    return p


def main(argv=None):
    args = build_parser().parse_args(argv)
    if args.test:
        return run_self_test()
    require(args.run_dirs, "no run directories given (see --help)")
    scale = GAIN_TABLE[args.gain_mode] / GAIN_TABLE["1x"]
    require(1.5 * scale <= args.gain_v_per_nt <= 4.0 * scale,
            f"--gain-v-per-nt {args.gain_v_per_nt} implausible for {args.gain_mode}")
    run_analysis(args)
    return 0


if __name__ == "__main__":
    sys.exit(main())
