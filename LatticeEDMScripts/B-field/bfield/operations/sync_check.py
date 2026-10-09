"""Are two heads running on one modulation clock?

Moved from qzfm_serial.py (welch_power, carrier_lines, unalias_hz,
check_modulation_sync). Reads the guard record's pT files: two unsynced
heads put two carrier lines in the aliased band, and their spacing is
the beat that lands in the data.
"""

from __future__ import annotations

import glob
import os

from bfield.constants import MOD_CARRIER_HZ
from bfield.operations.record import parse_map

# the modulation carrier leaks into the analog output and aliases
# (922.9 Hz -> 277.1 Hz at fs 1200); 906.0 is a single-head sideband
KNOWN_SINGLE_HEAD_HZ = (MOD_CARRIER_HZ, 906.0)
# +/-8 Hz keeps the 906 Hz sideband (16.8 Hz away after folding) out of
# the window while still resolving a beat of up to 16 Hz
CARRIER_SEARCH_HZ = 8.0
EDM_BAND_HZ = (0.05, 1.0)      # a beat landing in here is the bad case
# averaged segments: a single periodogram's band max sits several times
# above the median on white noise alone, faking a second carrier
CARRIER_SEG_S = 4.0            # 0.25 Hz bins: fine enough to split a 0.79 Hz beat
CARRIER_MIN_RATIO = 20.0       # peak power over band-median power


def welch_power(x, fs, seg_s=CARRIER_SEG_S):
    """Averaged periodogram: (freq, power). Hann, 50% overlap."""
    import numpy as np

    nper = min(len(x), max(int(seg_s * fs), 16))
    step = max(nper // 2, 1)
    win = np.hanning(nper)
    norm = (win ** 2).sum() * fs
    acc, k = None, 0
    for start in range(0, len(x) - nper + 1, step):
        seg = x[start:start + nper]
        seg = seg - seg.mean()
        p = np.abs(np.fft.rfft(seg * win)) ** 2 / norm
        acc = p if acc is None else acc + p
        k += 1
    return np.fft.rfftfreq(nper, d=1.0 / fs), acc / k


def unalias_hz(peak_hz, fs):
    """Map an aliased peak back to the carrier that made it.

    Not `fs - peak`: that is right at fs=1200 only. At fs=800 the
    carrier sits above the fold and the same subtraction returned
    677 Hz for a 923 Hz line.
    """
    n = round(MOD_CARRIER_HZ / fs)          # which multiple it folded around
    side = 1.0 if MOD_CARRIER_HZ >= n * fs else -1.0
    return n * fs + side * peak_hz


def carrier_lines(series, fs):
    """Strongest line per channel in the aliased-carrier band:
    {channel: (carrier_hz, power)}."""
    import numpy as np

    alias = abs(MOD_CARRIER_HZ - round(MOD_CARRIER_HZ / fs) * fs)
    lo, hi = alias - CARRIER_SEARCH_HZ, alias + CARRIER_SEARCH_HZ
    found = {}
    for name, x in series.items():
        x = np.asarray(x, dtype=float)
        x = x[np.isfinite(x)]
        if x.size < int(CARRIER_SEG_S * fs):    # too short to average
            continue
        freq, power = welch_power(x, fs)
        band = (freq >= max(lo, 0.0)) & (freq <= min(hi, fs / 2))
        if not band.any():
            continue
        idx = np.flatnonzero(band)
        peak = idx[int(np.argmax(power[idx]))]
        floor = float(np.median(power[idx]))
        if floor <= 0 or power[peak] < CARRIER_MIN_RATIO * floor:
            continue                            # nothing standing out
        found[name] = (unalias_hz(float(freq[peak]), fs), float(power[peak]))
    return found


def check_modulation_sync(ctx, run_dir, fs, map_spec, names=None):
    """Verdicts: no beat resolved / not synced / unknown -- never
    "synced": this only upper-bounds the beat (0.5 Hz at CARRIER_SEG_S,
    while the eEDM band starts at 0.05 Hz)."""
    # keep in step with the recorder's "{session}_{CHANNEL}_pT.txt";
    # renaming those files makes the sync check go blind silently
    stems = sorted(glob.glob(os.path.join(run_dir, "*_pT.txt")))
    series = {}
    for path in stems:
        name = os.path.basename(path).rsplit("_pT.txt", 1)[0].rsplit("_", 1)[-1]
        try:
            import numpy as np
            series[name.lower()] = np.loadtxt(path)
        except Exception as e:
            ctx.warn(f"sync check: cannot read {os.path.basename(path)} "
                     f"({type(e).__name__}: {e})")
    if not series:
        return {"verdict": "unknown", "why": "no channel files to read",
                "carriers_hz": {}, "beat_hz": None, "in_edm_band": False}

    head_of = {name: head for name, head, _ in parse_map(map_spec)}
    lines = carrier_lines(series, fs)
    # one carrier per head: the loudest channel that head owns
    per_head = {}
    for name, (hz, amp) in lines.items():
        head = head_of.get(name)
        if head is None:
            continue
        if head not in per_head or amp > per_head[head][1]:
            per_head[head] = (hz, amp)
    carriers = {h: round(v[0], 2) for h, v in sorted(per_head.items())}

    if len(carriers) < 2:
        missing = sorted(set(head_of.values()) - set(carriers))
        return {"verdict": "unknown",
                "why": ("no carrier line above the floor on head "
                        + (", ".join(missing) or "either")
                        + " -- gain too low, or the line is under the floor"),
                "carriers_hz": carriers, "beat_hz": None,
                "in_edm_band": False}

    (ha, fa), (hb, fb) = sorted(carriers.items())
    na = (names or {}).get(ha, f"head {ha}")
    nb = (names or {}).get(hb, f"head {hb}")
    beat = abs(fa - fb)
    bin_hz = 1.0 / CARRIER_SEG_S
    resolution = 2 * bin_hz
    if beat <= resolution:
        return {"verdict": "no beat resolved",
                "why": (f"{na} at {fa:.2f} Hz, {nb} at "
                        f"{fb:.2f} Hz -- any beat is under the "
                        f"{resolution:.2f} Hz resolved here; an in-band "
                        f"beat (eEDM band starts {EDM_BAND_HZ[0]:g} Hz) "
                        f"would look the same"),
                "carriers_hz": carriers, "beat_hz": round(beat, 3),
                "beat_resolution_hz": resolution,
                # False = "no in-band beat seen", never "there is none"
                "in_edm_band": False}
    # a sideband one head makes on its own is not a second oscillator
    known = [k for k in KNOWN_SINGLE_HEAD_HZ
             if any(abs(f - k) < 1.0 for f in (fa, fb))]
    in_band = EDM_BAND_HZ[0] <= beat <= EDM_BAND_HZ[1]
    return {"verdict": "not synced",
            "why": (f"{na} at {fa:.2f} Hz, {nb} at {fb:.2f} Hz "
                    f"-> beat {beat:.2f} Hz"
                    + (" -- IN THE eEDM BAND" if in_band else "")
                    + (f" (known single-head lines: "
                       f"{', '.join(f'{k:g}' for k in known)})"
                       if known else "")),
            "carriers_hz": carriers, "beat_hz": round(beat, 3),
            "in_edm_band": in_band}
