"""File-level contracts shared by every program: atomic json, run-folder
naming and collision rules, the recorder's artifact list."""

from __future__ import annotations

import contextlib
import glob
import itertools
import json
import math
import os

from bfield import clock


def sanitize(obj):
    """JSON-safe copy: non-finite floats become None (NaN from a missing
    cell-temp frame is not valid JSON)."""
    if isinstance(obj, float):
        return obj if math.isfinite(obj) else None
    if isinstance(obj, dict):
        return {k: sanitize(v) for k, v in obj.items()}
    if isinstance(obj, (list, tuple)):
        return [sanitize(v) for v in obj]
    return obj


def write_json_atomic(path, doc, *, indent=2):
    """Write doc to path so that a power cut never leaves a torn file.

    Raises on failure (OSError, or TypeError for an unserialisable doc);
    the caller decides if that is fatal. The tmp file sits beside path
    (os.replace is atomic only within one filesystem) and is removed on
    a failed write.
    """
    tmp = path + ".tmp"
    try:
        with open(tmp, "w", newline="\n", encoding="utf-8") as fh:
            json.dump(doc, fh, indent=indent)
            fh.write("\n")
            fh.flush()
            os.fsync(fh.fileno())
        os.replace(tmp, path)
    except BaseException:
        with contextlib.suppress(OSError):
            os.remove(tmp)
        raise


def write_text_atomic(path, text):
    """Like write_json_atomic, for a text file (config/sensors.toml)."""
    tmp = path + ".tmp"
    try:
        with open(tmp, "w", newline="\n", encoding="utf-8") as fh:
            fh.write(text if text.endswith("\n") else text + "\n")
            fh.flush()
            os.fsync(fh.fileno())
        os.replace(tmp, path)
    except BaseException:
        with contextlib.suppress(OSError):
            os.remove(tmp)
        raise


def read_json(path):
    try:
        with open(path, encoding="utf-8") as fh:
            return json.load(fh)
    except (OSError, ValueError):
        return None


def read_meta(run_dir):
    """The run's *.meta.json, or None."""
    hits = sorted(glob.glob(os.path.join(run_dir, "*.meta.json")))
    return read_json(hits[0]) if hits else None


# --------------------------------------------------- run-directory layout

def outdir_occupied(path):
    """True when a new session written here would collide with an old one.

    Missing and empty directories are free; a blocking file or any
    existing entry is occupied.
    """
    if not os.path.exists(path):
        return False
    if not os.path.isdir(path):
        return True
    try:
        with os.scandir(path) as it:
            return next(it, None) is not None
    except OSError:
        return True


def unique_outdir(path):
    """`path`, or `path1`, `path2`, ... first name that is not occupied.
    Does not create the directory."""
    path = os.path.normpath(path)
    if not outdir_occupied(path):
        return path
    parent, name = os.path.split(path)
    if not name:
        parent, name = os.path.split(parent.rstrip("\\/"))
    if not name:
        raise ValueError(f"cannot number outdir {path!r}")
    for n in itertools.count(1):
        candidate = os.path.join(parent, f"{name}{n}")
        if not outdir_occupied(candidate):
            return candidate
    raise RuntimeError("unreachable")


def alloc_run_dir(outdir, index):
    """<outdir>/Rnn_HHMM, numbered _2, _3 on a same-minute collision."""
    stamp = clock.strftime("%H%M")
    base = os.path.join(outdir, f"R{index:02d}_{stamp}")
    candidate, n = base, 1
    while os.path.exists(candidate):
        n += 1
        candidate = f"{base}_{n}"
    return candidate


def stamped_dir(parent, prefix, *, create=True):
    """<parent>/<prefix>_YYYYMMDD_HHMMSS (degauss dg_*, relax rx_*, sweep sw_*)."""
    path = os.path.join(parent, f"{prefix}_{clock.strftime('%Y%m%d_%H%M%S')}")
    if create:
        os.makedirs(path, exist_ok=False)
    return path


def rel_to(path, root):
    """path relative to root with forward slashes (ledgers stay portable)."""
    try:
        return os.path.relpath(path, root).replace(os.sep, "/")
    except ValueError:  # other drive on Windows
        return os.path.abspath(path).replace(os.sep, "/")


# File names that mean "a recording happened here". house*.csv and *.log
# are not: they belong to the parent and appear before the recorder.
RECORDER_ARTIFACT_GLOBS = ("meta.json", "*.meta.json", "*_pT.txt",
                           "*_V.txt", "*_raw.csv", "*_pT_per_m.txt")


def recorder_leftovers(outdir):
    """Every recorder artifact already present in outdir, any config.

    Scans all artifact globs, not just the current config's paths; the
    per-config scan missed old files and let the shared raw CSV be
    truncated (2026-08-07 review, High 8).
    """
    if not os.path.isdir(outdir):
        return []
    return sorted({os.path.basename(p)
                   for pat in RECORDER_ARTIFACT_GLOBS
                   for p in glob.glob(os.path.join(outdir, pat))})


def rollback_block(handles_offsets):
    """Truncate every output file back to its block-start offset.

    Keeps all outputs mutually consistent at the last complete block
    after a mid-block failure. Returns the failures ("name: error")
    instead of suppressing them.
    """
    failed = []
    for fh, off in handles_offsets:
        try:
            fh.seek(off)
            fh.truncate()
            fh.flush()
        except Exception as e:
            failed.append(f"{getattr(fh, 'name', '?')}: "
                          f"{type(e).__name__}: {e}")
    return failed
