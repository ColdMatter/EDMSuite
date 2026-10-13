#!/usr/bin/env python3
"""Every recorded run, judged on the quantities that should read zero.

The recorders already write the evidence -- rail fractions, non-finite
counts, pinned-block fractions, exit codes, ADC code inventories -- into
each run's meta.json. Nobody had ever lined them up. Doing so on
2026-08-11 flagged two runs that had been used in an analysis that same
afternoon, one of them with a channel against the rail for 77 % of the
record. Neither is visible from a single meta file; both are obvious
across the set (REPORT_PLAN §18.13, after Kara et al. 2012 Table 2).

Read it per channel, not per run. A railed y says nothing about z, and
that distinction is what decides whether an analysis survives.

    python analysis/run_audit.py            # everything under result/
    python analysis/run_audit.py result/ms1l_3_degaussed
    python analysis/run_audit.py --all      # list clean runs too

Exit 0 always: this reports, it does not judge. A flag is a prompt to
look, and several flagged runs are deliberate (failed_analog keeps the
railed records on purpose, and an operator may mark a run void).
"""

import argparse
import glob
import json
import os
import sys

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from _paths import DATA_ROOT  # noqa: E402

# a channel is worth mentioning above these; zero would flag dither noise
RAIL_FRAC = 0.001          # ADC against the input range
QZFM_RAIL_FRAC = 0.001     # sensor's own output rail
PINNED_FRAC = 0.001        # blocks dominated by one ADC code
MIN_CODES = 8              # fewer codes than this and the white-noise
                           # model for quantization stops holding


def channel_flags(meta):
    """Per-channel problems, as {channel: [reason, ...]}."""
    out = {}
    for key, limit, label in (("rail_fraction", RAIL_FRAC, "railed"),
                              ("qzfm_rail_fraction", QZFM_RAIL_FRAC,
                               "sensor-railed"),
                              ("pinned_block_fraction", PINNED_FRAC,
                               "pinned")):
        for ch, v in (meta.get(key) or {}).items():
            if v and v > limit:
                out.setdefault(ch, []).append(f"{label} {v:.3f}")
    for ch, v in (meta.get("n_nonfinite") or {}).items():
        if v:
            out.setdefault(ch, []).append(f"{v} non-finite")
    for ch, v in (meta.get("adc_unique_codes") or {}).items():
        if v is not None and v < MIN_CODES:
            out.setdefault(ch, []).append(f"only {v} ADC codes")
    return out


def run_flags(meta):
    """Whole-run problems, as a list of reasons."""
    out = []
    if meta.get("exit_code") not in (0, None):
        out.append(f"exit {meta['exit_code']}")
    if meta.get("status") not in ("ok", None):
        out.append(f"status {meta['status']}")
    for w in meta.get("warnings") or []:
        out.append(f"warning: {w}")
    return out


def pair_note(meta):
    """Block-mean correlations, printed with the length that governs them.

    Recorders write pairs[].blockmean_r into every multi-head run and it
    reads like a physical statement. It is not, at these durations: its
    effective sample count is set by the drift timescale, so a 900 s
    record holds four or five independent samples and the standard error
    lands near 0.7. A true +0.9 returns -0.26 without difficulty, which
    is exactly what happened in twohead_sync02 (REPORT_PLAN §22.4b).
    Never quoted alone -- hence printed next to the duration.
    """
    pairs = meta.get("pairs") or []
    if not pairs:
        return ""
    secs = meta.get("duration_s") or 0.0
    body = ", ".join(f"{p.get('axis')} {p.get('blockmean_r'):+.2f}"
                     for p in pairs if p.get("blockmean_r") is not None)
    return f"{body}  over {secs:.0f} s" if body else ""


def audit(roots, show_clean=False):
    metas = sorted(p for root in roots
                   for p in glob.glob(os.path.join(root, "**", "*.meta.json"),
                                      recursive=True))
    if not metas:
        print(f"no runs under {', '.join(roots)}")
        return
    flagged = clean = 0
    for path in metas:
        try:
            with open(path, encoding="utf-8") as fh:
                meta = json.load(fh)
        except (OSError, ValueError) as e:
            print(f"{path}: unreadable ({e})")
            flagged += 1
            continue
        name = path.removesuffix(".meta.json")
        pairs = pair_note(meta)
        if pairs:
            print(f"  pair {name}\n         blockmean_r: {pairs}")
        run, chan = run_flags(meta), channel_flags(meta)
        if not run and not chan:
            clean += 1
            if show_clean:
                print(f"  ok   {name}")
            continue
        flagged += 1
        print(f"\n  FLAG {name}")
        for reason in run:
            print(f"         run: {reason}")
        for ch in sorted(chan):
            print(f"         {ch}: {'; '.join(chan[ch])}")
    print(f"\n{len(metas)} runs scanned, {clean} clean, {flagged} flagged.")
    if flagged:
        print("A flag is per CHANNEL where one is named: a railed y leaves z "
              "usable, and that is usually the question worth asking.")


def main(argv=None):
    p = argparse.ArgumentParser(description=__doc__.splitlines()[0])
    p.add_argument("roots", nargs="*", default=[DATA_ROOT],
                   help="directories to walk (default: DATA_ROOT from config/lab.toml)")
    p.add_argument("--all", action="store_true",
                   help="list the clean runs as well as the flagged ones")
    args = p.parse_args(argv)
    audit(args.roots, show_clean=args.all)
    return 0


if __name__ == "__main__":
    raise SystemExit(main(sys.argv[1:]))
