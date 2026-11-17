#!/usr/bin/env python3
"""What is that line, and is it on both heads?

Bench triage for "a clean sine appeared in the trace". Point it at any
qzfm_record.py run directory (any topology); it lists the strongest narrow
lines per channel, and for a two-head run it says whether each line is
coherent between the heads -- which is the difference between a real
common field, a modulation beat, and one sensor misbehaving.

  python analysis/ms1l_3_degaussed/find_sine.py <run_dir>

Written 2026-08-04 for the master/slave sync check: after cabling the
modulation clock 3Q -> 3S, a clean sine turned up on 3Q that survived a
re-zero. The three candidate frequencies were pre-registered before
looking, so the printout labels them rather than letting the eye pick:

  0.05-1 Hz   the bad case -- straight into the eEDM band
  ~12 Hz      the beat predicted from the 1.32% Check Modulation split
              (445622 vs 439740; IF those numbers track the modulation
              frequency, which is unverified)
  ~50 Hz      mains, i.e. nothing to do with the modulation. The Mod
              cables also bond the two electronics modules together for
              the first time, so a new ground loop is on the table.
"""

import glob
import json
import os
import sys

import numpy as np
from scipy.signal import coherence, welch

BANDS = (("eEDM band -- STOP", 0.05, 1.0),
         ("modulation beat?", 8.0, 20.0),
         ("mains", 45.0, 55.0))


def load_run(run_dir):
    """(fs, {channel: series_pT}) from a recorder output directory."""
    metas = glob.glob(os.path.join(run_dir, "*.meta.json"))
    if not metas:
        raise SystemExit(f"no *.meta.json in {run_dir}")
    with open(metas[0]) as fh:
        meta = json.load(fh)
    stem = metas[0][: -len(".meta.json")]
    series = {}
    for path in sorted(glob.glob(stem + "_*_pT.txt")):
        name = os.path.basename(path)[len(os.path.basename(stem)) + 1: -7]
        series[name] = np.loadtxt(path)
    if not series:
        raise SystemExit(f"no *_pT.txt next to {metas[0]}")
    return float(meta.get("fs_hz", 1200.0)), meta, series


def asd(x, fs, seg_s=20.0):
    f, p = welch(x, fs=fs, nperseg=min(len(x), int(seg_s * fs)),
                 detrend="linear")
    return f, np.sqrt(p)


def top_lines(f, a, n=6, halfwidth=25):
    """Peaks measured against the local median, so the 1/f slope does not
    win every time. Returns (freq, amplitude, ratio-to-local-floor)."""
    out = []
    for i in range(2, len(a) - 2):
        lo, hi = max(0, i - halfwidth), min(len(a), i + halfwidth)
        floor = np.median(np.concatenate([a[lo:i - 1], a[i + 2:hi]]))
        if floor <= 0 or a[i] < a[i - 1] or a[i] < a[i + 1]:
            continue
        out.append((f[i], a[i], a[i] / floor))
    out.sort(key=lambda r: -r[2])
    kept = []
    for row in out:
        if all(abs(row[0] - k[0]) > 0.5 for k in kept):
            kept.append(row)
        if len(kept) == n:
            break
    return kept


def label(freq):
    for name, lo, hi in BANDS:
        if lo <= freq <= hi:
            return name
    return ""


def main():
    if len(sys.argv) != 2:
        raise SystemExit(__doc__)
    fs, meta, series = load_run(sys.argv[1])
    print(f"{sys.argv[1]}  fs={fs:g} Hz  "
          f"{len(next(iter(series.values())))/fs:.0f} s  "
          f"channels {', '.join(sorted(series))}")
    rail = meta.get("rail_fraction") or {}
    if rail:
        print("rail_fraction: " + "  ".join(f"{k}={v:.3f}"
                                            for k, v in sorted(rail.items())))
        if any(v > 0.01 for v in rail.values()):
            print("  ⚠ a railed channel looks quiet -- fix the window first")

    spec = {}
    for name in sorted(series):
        f, a = asd(series[name], fs)
        spec[name] = (f, a)
        print(f"\n[{name}] strongest lines (amplitude, and how far above "
              f"the local floor)")
        for freq, amp, ratio in top_lines(f, a):
            tag = label(freq)
            print(f"   {freq:8.3f} Hz   {amp*1e3:9.0f} fT/rtHz   "
                  f"x{ratio:5.1f}   {tag}")

    # two heads: a line on both, in phase, is a common signal -- which is
    # exactly what the modulation beat would look like
    pairs = [(a, b) for a in series for b in series
             if b == a + "2"]
    for a_name, b_name in pairs:
        x, y = series[a_name], series[b_name]
        n = min(len(x), len(y))
        f, c = coherence(x[:n], y[:n], fs=fs,
                         nperseg=min(n, int(20.0 * fs)))
        print(f"\n[{a_name} vs {b_name}] coherence at the strongest lines")
        for freq, _, _ in top_lines(*spec[a_name]):
            i = int(np.argmin(np.abs(f - freq)))
            verdict = ("COMMON to both heads" if c[i] > 0.5
                       else "one head only")
            print(f"   {freq:8.3f} Hz   gamma^2 {c[i]:.2f}   {verdict}"
                  f"   {label(freq)}")
    if not pairs:
        print("\n(single-head run: record 4 channels to tell a common "
              "field from one sensor's own line)")
    carrier_report(fs, spec)


# One QZFM head puts BOTH of these into the analog output. Measured on
# 2026-07-13 (`ms1l_1_predegauss/predegauss_900s`), a single-head record
# taken 15 days before the second head existed, and both vanish with the
# sensor powered off (`ms1l_2_degaussfail/daqfloor_qzfmoff_1V_60s`), so
# they are sensor-side and one head is enough to make them. 906.0 is
# 16.9 Hz below the carrier -- a sideband, not a second oscillator.
KNOWN_SINGLE_HEAD = (922.9, 906.0)


def carrier_report(fs, spec, nominal=922.9):
    """Read the modulation carrier straight out of the record.

    The carrier leaks into the analog output and aliases: at fs=1200 the
    922.9 Hz modulation lands at 277.1 Hz, which is where it sits in every
    record we have. That turns the master/slave question into a direct
    measurement instead of an interpretation of the Check Modulation
    display -- but only for lines that are NOT already in the single-head
    signature above. A second oscillator shows up as a NEW line.
    """
    alias = abs(nominal - round(nominal / fs) * fs)
    lo, hi = alias - 30.0, alias + 30.0
    print(f"\n=== modulation carrier: {nominal:g} Hz aliases to "
          f"{alias:.1f} Hz at fs={fs:g} ===")
    found = {}
    for name, (f, a) in sorted(spec.items()):
        m = (f >= lo) & (f <= hi)
        if not m.any():
            continue
        rows = [r for r in top_lines(f[m], a[m], n=4) if r[2] > 3.0]
        found[name] = rows
        for freq, amp, ratio in rows:
            print(f"   [{name}] {freq:8.3f} Hz -> carrier "
                  f"{fs - freq:8.3f} Hz   {amp*1e3:7.0f} fT/rtHz  x{ratio:.1f}")
        if not rows:
            print(f"   [{name}] no carrier line above the floor")
    carriers = sorted({round(fs - r[0], 2)
                       for rows in found.values() for r in rows})
    known = [c for c in carriers
             if any(abs(c - k) < 1.0 for k in KNOWN_SINGLE_HEAD)]
    novel = [c for c in carriers if c not in known]
    print("\n   lines matching the known single-head signature "
          f"({', '.join(f'{k:g}' for k in KNOWN_SINGLE_HEAD)} Hz): "
          + (", ".join(f"{c:.2f}" for c in known) or "none"))
    if not novel:
        print("   no line beyond that signature -- nothing here says the "
              "heads run on different clocks")
        return
    print("   ★ NEW line(s): " + ", ".join(f"{c:.2f}" for c in novel))
    for c in novel:
        for k in KNOWN_SINGLE_HEAD + tuple(novel):
            gap = abs(c - k)
            if 0.0 < gap <= 40.0:
                flag = ("  ⚠ IN THE eEDM BAND" if 0.05 <= gap <= 1.0 else "")
                print(f"      {c:.2f} vs {k:.2f} -> beat {gap:.2f} Hz{flag}")
    print("   a new line near the carrier is what a second, unsynced "
          "oscillator looks like; confirm by powering one head off")


if __name__ == "__main__":
    main()
