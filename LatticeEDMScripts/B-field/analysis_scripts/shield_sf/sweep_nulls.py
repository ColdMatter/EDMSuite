#!/usr/bin/env python3
"""What a sweep's own field did to the walls, from its two drive-off records.

    python analysis/shield_sf/sweep_nulls.py result/shield_sf/sw_<stamp>

sf_sweep.py takes one record with the drive OFF before the sweep and one
after. They are ordinary 900 s noise records, so the question they answer
is the campaign's own question: did the 0.05 Hz plateau move?

Why the pair and not the second one alone: a single record says what
state the shield is in, and the archive already has a distribution to
compare that against. It does not say what THIS sweep cost, because the
shield's state also moves between days for reasons the sweep had nothing
to do with. The two records bracket the sweep and nothing else.

The test is against measured scatter, not against zero. Nominally
identical runs in this campaign scatter with CV 0.29 (the 08-01 audit),
so a ratio of two of them carries sigma_ln = 0.29 * sqrt(2) = 0.41. A
change inside about +-2 sigma is not one this bench can resolve, and
saying otherwise would be reading noise.

Statistics are the campaign's: the FIRST 900 s of each record, a spliced
Welch (200 s segments below 1 Hz, 20 s above, linear detrend), each axis
reported apart. They sit beside plot_relax_all.py's numbers without
conversion.
"""

import argparse
import glob
import json
import os
import sys

import numpy as np
from scipy.signal import welch

HERE = os.path.dirname(os.path.abspath(__file__))
WIN_S = 900.0
F0 = 0.05
# run-to-run scatter measured in the 08-01 audit: CV ~ 0.29 across
# nominally identical runs, so a ratio of two carries this
SIGMA_LN = 0.29 * np.sqrt(2.0)
RESOLVE_SIGMA = 2.0


def load(path, n):
    with open(path, encoding="utf-8") as fh:
        body = [ln for ln in fh.read().split("\n") if ln[:1] not in ("#", "")]
    return np.array(body[:n], dtype=np.float64)


import sys as _sys  # analysis_scripts/ on sys.path for _paths
_sys.path.insert(0, os.path.dirname(os.path.dirname(os.path.abspath(__file__))))
import _paths  # noqa: E402,F401  (puts B-field on sys.path)
from bfield.analysis.spectra import spliced_asd  # noqa: E402 -- was an identical local copy


def read_null(run_dir, axes):
    """(label -> stats) for one drive-off record."""
    metas = [m for m in glob.glob(os.path.join(run_dir, "*.meta.json"))
             if "_guard" not in m]
    if len(metas) != 1:
        return None, (f"expected one record under {run_dir}, "
                      f"found {len(metas)}")
    with open(metas[0], encoding="utf-8") as fh:
        meta = json.load(fh)
    if not (meta.get("status") == "ok" and meta.get("exit_code") == 0):
        return None, (f"{run_dir}: status={meta.get('status')} "
                      f"exit={meta.get('exit_code')}")
    base = metas[0][: -len(".meta.json")]
    fs = float(meta["fs_hz"])
    n = int(WIN_S * fs)
    out = {}
    for ax in axes:
        path = f"{base}_{ax.upper()}_pT.txt"
        if not os.path.exists(path):
            continue
        x = load(path, n)
        f, a = spliced_asd(x, fs)
        t = np.arange(len(x)) / fs / 60.0
        out[ax] = {"v005": a[np.argmin(abs(f - F0))] * 1e3,
                   "white": np.median(a[(f >= 30) & (f <= 45)]) * 1e3,
                   "drift": abs(np.polyfit(t, x, 1)[0]),
                   "ptp": float(np.ptp(x)), "seconds": len(x) / fs}
    return out, None


def main(argv=None):
    p = argparse.ArgumentParser(
        allow_abbrev=False, description=__doc__.splitlines()[0])
    p.add_argument("sweep_dir", help="a sf_sweep sw_<stamp> directory")
    p.add_argument("--axes", default="z,y,z2,y2")
    args = p.parse_args(argv)
    axes = [a.strip().lower() for a in args.axes.split(",") if a.strip()]

    pair = {}
    for when in ("before", "after"):
        d = os.path.join(args.sweep_dir, f"null_{when}")
        if not os.path.isdir(d):
            print(f"[error] no {d} -- the sweep was run with "
                  f"--null-seconds 0, or stopped before it got there",
                  file=sys.stderr)
            return 13
        got, why = read_null(d, axes)
        if got is None:
            print(f"[error] {why}", file=sys.stderr)
            return 13
        pair[when] = got

    print(f"drive-off records, first {WIN_S:.0f} s, "
          f"{F0:g} Hz in fT/rtHz and drift in pT/min")
    hdr = (f"{'axis':<6}{'before':>10}{'after':>10}{'ratio':>8}{'sigma':>8}"
           f"   {'drift b/a':>14}{'white b/a':>16}")
    print(hdr)
    print("-" * len(hdr))
    verdicts = []
    for ax in axes:
        b, a = pair["before"].get(ax), pair["after"].get(ax)
        if not (b and a):
            continue
        ratio = a["v005"] / b["v005"]
        nsig = np.log(ratio) / SIGMA_LN
        verdicts.append(abs(nsig))
        print(f"{ax.upper():<6}{b['v005']:>10.0f}{a['v005']:>10.0f}"
              f"{ratio:>8.2f}{nsig:>+8.1f}   "
              f"{b['drift']:>6.1f}/{a['drift']:<7.1f}"
              f"{b['white']:>7.1f}/{a['white']:<8.1f}")
    if not verdicts:
        print("[error] no axis was readable in both records", file=sys.stderr)
        return 13
    print(f"\nsigma_ln = {SIGMA_LN:.2f} from the campaign's run-to-run "
          f"scatter (CV 0.29)")
    worst = max(verdicts)
    if worst <= RESOLVE_SIGMA:
        print(f"[verdict] the largest move is {worst:.1f} sigma. Inside the "
              f"+-{RESOLVE_SIGMA:g} sigma this bench can resolve, so the "
              f"sweep did not change the plateau by an amount these two "
              f"records can see. That is NOT 'it changed nothing' -- it is "
              f"the resolution.")
    else:
        print(f"[verdict] the largest move is {worst:.1f} sigma, outside the "
              f"+-{RESOLVE_SIGMA:g} sigma of run-to-run scatter. The sweep "
              f"left the walls in a different state; degauss before the next "
              f"measurement and treat the sweep's own later points as taken "
              f"on a drifting shield.")
    print("[note ] the white band should NOT move. If it did, suspect the "
          "sensor or the wiring rather than the shield.")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
