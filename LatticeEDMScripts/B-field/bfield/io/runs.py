"""Read-only loaders for recorded run folders.

A run folder is any folder holding a <session>.meta.json written by the
recorder (qzfm_record.py or bfield). Next to it sit one
<session>_<CHANNEL>_pT.txt per magnetic channel, <session>_<AUX>_V.txt
per raw-volt aux channel and <session>_raw.csv. Nothing here writes.
"""

from __future__ import annotations

import glob
import os
import re

import numpy as np

from bfield.io.files import read_json, read_meta

__all__ = ["data_root", "read_channel_header", "read_channel", "read_meta",
           "find_runs", "channel_files", "load_run"]

# '# key=value key=value' pairs; values have no spaces (the recorder
# writes them that way)
_PAIR_RX = re.compile(r"([A-Za-z_]+)=(\S+)")
_NUMERIC = {"fs", "vrange", "cal", "baseline_m"}


def data_root():
    """DATA_ROOT from config/lab.toml [paths] (lab.local.toml on top)."""
    from bfield.config import load_config
    return load_config().data_root


def read_channel_header(path):
    """The '# ' header of a *_pT.txt / *_V.txt file as a dict.

    Keys as the recorder wrote them: channel (or aux), head, session,
    start, fs, gain, cal, vrange, device, ch, sensor, source, plus
    'unit' from the "one value per line, unit=..." line. fs, vrange and
    cal become floats ('cal' is pT/V; the "pT/V" suffix is dropped).
    'kind' is "pT", "V" or "gradient" from the first line.
    """
    head = {}
    with open(path, encoding="utf-8", errors="replace") as fh:
        for line in fh:
            if not line.startswith("#"):
                break
            text = line[1:].strip()
            # "QZFM acquisition", or "QZFM two-head acquisition" (the
            # pre-merge qzfm_3axis.py header)
            if text.startswith("QZFM") and "acquisition |" in text:
                head["kind"] = "V" if "aux=" in text else "pT"
            elif text.startswith("QZFM gradient"):
                head["kind"] = "gradient"
            for key, val in _PAIR_RX.findall(text):
                key = key.lower()
                if key in _NUMERIC:
                    try:
                        head[key] = float(val)
                    except ValueError:
                        head[key] = val
                else:
                    head.setdefault(key, val)
    if "aux" in head and "channel" not in head:
        head["channel"] = head["aux"]
    return head


def read_channel(path):
    """(samples, header) of one channel file; samples as a float array."""
    return np.loadtxt(path, comments="#", ndmin=1), read_channel_header(path)


def channel_files(run_dir):
    """{channel name (lower case): path} for the *_pT.txt and *_V.txt files.

    The channel is the last '_' field before the suffix, as the recorder
    names them ({session}_{CHANNEL}_pT.txt).
    """
    out = {}
    for suffix in ("_pT.txt", "_V.txt"):
        for path in sorted(glob.glob(os.path.join(run_dir, f"*{suffix}"))):
            stem = os.path.basename(path)[:-len(suffix)]
            out[stem.rsplit("_", 1)[-1].lower()] = path
    return out


def find_runs(root):
    """Every run folder under root (a folder holding a *.meta.json), sorted."""
    hits = glob.glob(os.path.join(root, "**", "*.meta.json"), recursive=True)
    return sorted({os.path.dirname(p) for p in hits})


def load_run(run_dir):
    """{'meta': dict or None, 'channels': {name: array}, 'headers': {...}}."""
    channels, headers = {}, {}
    for name, path in channel_files(run_dir).items():
        channels[name], headers[name] = read_channel(path)
    return {"dir": run_dir, "meta": read_meta(run_dir),
            "channels": channels, "headers": headers}


# ------------------------------------------------------------ run notes

# per-channel quality limits (the recorder's own: rail-limit 1 %, pinned
# in most blocks, flat below 0.1 mV peak-to-peak)
RAIL_FLAG = 0.01
PINNED_FLAG = 0.5
FLAT_PTP_V = 1e-4
NOTE_FILES = ("NOTES.md", "README.md", "notes.txt", "NOTES.txt")
NOTE_MAX_BYTES = 40_000


def short_flag(text):
    """A few words for a flag (a checkbox label): 'ADC rail 77%'."""
    m = re.search(r"([0-9.]+)%", text)
    pct = f" {float(m.group(1)):.0f}%" if m else ""
    for key, word in (("ADC input at the rail", "ADC rail"),
                      ("sensor output at its rail", "sensor rail"),
                      ("pinned", "pinned"), ("non-finite", "non-finite"),
                      ("flat", "flat"), ("calibration factor", "cal factor")):
        if key in text:
            return word + (pct if word in ("ADC rail", "sensor rail",
                                           "pinned") else "")
    return text.split(" (")[0][:24]


def channel_flags(meta):
    """{channel: [problem, ...]} from a meta.json's per-channel numbers.

    "bad" problems make the channel's spectrum untrustworthy (clipped,
    pinned, flat or non-finite samples); "warn" ones are worth knowing.
    Returns {channel: [(level, text), ...]} for flagged channels only.
    """
    out = {}

    def add(ch, level, text):
        out.setdefault(ch.lower(), []).append((level, text))

    for ch, v in (meta.get("rail_fraction") or {}).items():
        if v and v > RAIL_FLAG:
            add(ch, "bad", f"ADC input at the rail {v:.1%} of the record "
                           f"(+/-{meta.get('vrange', '?')} V window too small)")
    for ch, v in (meta.get("qzfm_rail_fraction") or {}).items():
        if v and v > RAIL_FLAG:
            add(ch, "bad", f"sensor output at its rail {v:.1%} of the record "
                           "(saturated: re-zero or lower gain)")
    for ch, v in (meta.get("pinned_block_fraction") or {}).items():
        if v and v > PINNED_FLAG:
            add(ch, "bad", f"output pinned to one ADC code in {v:.0%} of "
                           "blocks (stuck input or soft rail)")
    for ch, v in (meta.get("n_nonfinite") or {}).items():
        if v:
            add(ch, "bad", f"{v} non-finite samples")
    for ch, v in (meta.get("flat_ptp_V") or {}).items():
        if v is not None and v < FLAT_PTP_V:
            add(ch, "bad", f"flat: peak-to-peak {v * 1e3:.3f} mV (dead "
                           "sensor or unwired input)")
    for ch, f in (meta.get("cal_factors") or {}).items():
        if isinstance(f, (int, float)) and f > 1.5:
            add(ch, "warn", f"calibration factor {f:g} above 1.5 (the "
                            "sensor cell may need service)")
    return out


def _session_items(run_dir):
    """Lines from the sensor-session ledger that recorded this run (the
    qzfm_session.json in the run folder or the one above it)."""
    for where in (run_dir, os.path.dirname(os.path.normpath(run_dir))):
        path = os.path.join(where, "qzfm_session.json")
        doc = read_json(path) if os.path.exists(path) else None
        if not doc:
            continue
        lines = []
        setup = doc.get("setup") or {}
        for w in setup.get("warnings") or []:
            lines.append(f"setup warning: {w}")
        sync = setup.get("sync") or {}
        if sync.get("verdict"):
            lines.append(f"modulation sync: {sync['verdict']}"
                         + (f" -- {sync.get('why')}" if sync.get("why")
                            else ""))
        for h in doc.get("heals") or []:
            lines.append(f"re-zero + re-calibrate at {h.get('time')}: "
                         f"{h.get('why')}")
        name = os.path.basename(os.path.normpath(run_dir))
        for r in doc.get("runs") or []:
            if str(r.get("dir") or "").replace("\\", "/").endswith(name):
                lines.append(f"run outcome in the session: {r.get('outcome')}"
                             f" (exit {r.get('exit_code')})")
        if doc.get("outcome") and doc.get("outcome") != "completed":
            lines.append(f"session outcome: {doc['outcome']}")
        return path, lines
    return None, []


def run_notes(run_dir):
    """Everything recorded about a run's quality, in one dict:

      status, exit_code, meaning   from meta.json
      reason                       the error message, if any
      warnings                     the recorder's warnings
      operator_notes               the free-text notes given at the start
      channels                     channel_flags(meta)
      session_file, session        lines from qzfm_session.json
      note_files                   [(path, text)] NOTES.md / README.md
                                   in the run folder or the one above
    """
    from bfield.errors import exit_meaning
    meta = read_meta(run_dir) or {}
    code = meta.get("exit_code")
    err = meta.get("error") or {}
    session_file, session = _session_items(run_dir)
    files = []
    for where in (run_dir, os.path.dirname(os.path.normpath(run_dir))):
        for name in NOTE_FILES:
            path = os.path.join(where, name)
            if os.path.exists(path) and \
                    os.path.getsize(path) <= NOTE_MAX_BYTES:
                with open(path, encoding="utf-8", errors="replace") as fh:
                    files.append((path, fh.read()))
    return {"status": meta.get("status"), "exit_code": code,
            "meaning": exit_meaning(code) if code is not None else None,
            "reason": err.get("message"),
            "warnings": list(meta.get("warnings") or []),
            "operator_notes": meta.get("notes") or "",
            "channels": channel_flags(meta),
            "session_file": session_file, "session": session,
            "note_files": files}
