"""The degausser's streamed waveform as arrays, and its figure.

The same three panels as Twinleaf's DG Control VI: current against
time, voltage against time, and current against voltage (the I-V loop,
whose shape shows the walls saturating). Used for the PNG saved after
every cycle and for replaying a saved stream_cNN.csv; the live window
(bfield.gui.dg_live) draws the same thing while a cycle runs.

The stream has no time column. Time is row index / row rate; the rate
comes from the session's QC (soak rows / commanded soak) when known,
else the nominal rate measured on the bench.
"""

from __future__ import annotations

import csv
import json
import os

import numpy as np

STREAM_ROW_HZ_NOMINAL = 348.0       # bench 08-12; 08-14 streams ran ~400
PHASE_LABELS = {4: "measuring", 5: "ramping", 6: "soaking", 7: "taper",
                8: "cooldown", 0: "idle", -3: "FAULT"}
PHASE_COLOURS = {4: "#9e9e9e", 5: "#ff9800", 6: "#e53935", 7: "#1e88e5",
                 8: "#43a047", 0: "#9e9e9e", -3: "#000000"}


def column_indices(columns):
    """(current, voltage, status) column indices; None when absent."""
    from bfield.operations.degauss_cycle import (find_current_column,
                                                 find_status_column,
                                                 find_voltage_column)
    return (find_current_column(columns), find_voltage_column(columns),
            find_status_column(columns))


def rows_to_arrays(rows, columns):
    """(I, V, status) float arrays from stream rows. A missing column
    gives NaNs; unparseable cells give NaN."""
    ci, vi, si = column_indices(columns or [])
    n = len(rows)
    out = []
    for idx in (ci, vi, si):
        a = np.full(n, np.nan)
        if idx is not None:
            for k, row in enumerate(rows):
                try:
                    a[k] = float(row[idx])
                except (TypeError, ValueError, IndexError):
                    pass
        out.append(a)
    return tuple(out)


def phase_changes(status):
    """[(row index, code)] where the status code changes (NaN skipped)."""
    out = []
    last = None
    for k, s in enumerate(status):
        if not np.isfinite(s):
            continue
        code = int(s)
        if code != last:
            out.append((k, code))
            last = code
    return out


def load_stream(path):
    """(columns, rows) of a stream_cNN.csv written by degauss_cycle."""
    with open(path, newline="", encoding="utf-8") as fh:
        reader = csv.reader(fh)
        columns = next(reader, [])
        rows = [tuple(r) for r in reader if r]
    return columns, rows


def session_info(csv_path):
    """What the degauss_session.json beside a stream says about it:
    {"rate_hz", "title"}; empty values when there is no json."""
    folder, name = os.path.split(os.path.abspath(csv_path))
    info = {"rate_hz": None, "title": name}
    try:
        with open(os.path.join(folder, "degauss_session.json"),
                  encoding="utf-8") as fh:
            doc = json.load(fh)
    except (OSError, ValueError):
        return info
    for cyc in doc.get("cycles") or []:
        if cyc.get("stream_csv") != name:
            continue
        info["rate_hz"] = (cyc.get("qc") or {}).get("stream_rate_hz")
        w = cyc.get("verified") or cyc.get("requested") or {}
        info["taper_s"] = w.get("duration")
        info["title"] = cycle_title(cyc, doc.get("coil"),
                                    os.path.basename(folder))
    return info


def rate_from_taper(status, taper_s):
    """Row rate from the taper-phase row count and the commanded taper
    length, for streams whose session recorded no rate (older runs).
    None unless the taper visibly ran to its end (cooldown follows)."""
    if not taper_s or taper_s <= 0:
        return None
    codes = status[np.isfinite(status)].astype(int)
    if 8 not in codes:
        return None
    n_taper = int((codes == 7).sum())
    return n_taper / float(taper_s) if n_taper > 100 else None


def cycle_title(rec, coil=None, where=None):
    """'cycle 1: 8 A, 15 Hz, soak 15 s, taper 300 s -- ok' from a cycle
    record (degauss_session.json "cycles" entry)."""
    w = rec.get("verified") or rec.get("requested") or {}

    def g(key, unit):
        v = w.get(key)
        return f"{v:g} {unit}" if isinstance(v, (int, float)) else f"? {unit}"
    parts = [f"cycle {rec.get('index', '?')}: {g('current', 'A')}, "
             f"{g('frequency', 'Hz')}, soak {g('soaktime', 's')}, "
             f"taper {g('duration', 's')}"]
    if coil:
        parts.append(f"coil {coil}")
    if rec.get("outcome"):
        parts.append(str(rec["outcome"]))
    if where:
        parts.append(where)
    return " -- ".join(parts)


def save_figure(path, I, V, status, rate_hz=None, title=""):
    """The three-panel PNG (matplotlib, no display needed)."""
    import matplotlib
    matplotlib.use("Agg")
    import matplotlib.pyplot as plt

    rate = rate_hz or STREAM_ROW_HZ_NOMINAL
    t = np.arange(len(I)) / rate
    fig = plt.figure(figsize=(11, 9))
    gs = fig.add_gridspec(3, 1, height_ratios=[1, 1, 2.2], hspace=0.35)
    ax_i = fig.add_subplot(gs[0])
    ax_v = fig.add_subplot(gs[1], sharex=ax_i)
    ax_l = fig.add_subplot(gs[2])
    ax_i.plot(t, I, lw=0.4, color="#1e88e5")
    ax_v.plot(t, V, lw=0.4, color="#1e88e5")
    ax_i.set_ylabel("current (A)")
    ax_v.set_ylabel("voltage (V)")
    ax_v.set_xlabel(f"time (s, rows / {rate:.0f} Hz)")
    for k, code in phase_changes(status):
        for ax in (ax_i, ax_v):
            ax.axvline(t[k], color=PHASE_COLOURS.get(code, "k"), lw=0.8,
                       ls="--")
        ax_i.annotate(PHASE_LABELS.get(code, str(code)), (t[k], 1.0),
                      xycoords=("data", "axes fraction"), fontsize=8,
                      rotation=90, va="top", ha="right",
                      color=PHASE_COLOURS.get(code, "k"),
                      bbox=dict(fc="white", ec="none", alpha=0.8, pad=1))
    ax_l.plot(V, I, lw=0.3, color="#1e88e5")
    ax_l.set_xlabel("voltage (V)")
    ax_l.set_ylabel("current (A)")
    for ax in (ax_i, ax_v, ax_l):
        ax.grid(alpha=0.3)
    if title:
        fig.suptitle(title, fontsize=10)
    fig.savefig(path, dpi=110)
    plt.close(fig)
    return path


def figure_for_stream(csv_path, png_path=None):
    """Write the PNG for a saved stream; returns its path."""
    columns, rows = load_stream(csv_path)
    I, V, status = rows_to_arrays(rows, columns)
    info = session_info(csv_path)
    rate = info["rate_hz"] or rate_from_taper(status, info.get("taper_s"))
    png_path = png_path or os.path.splitext(csv_path)[0] + ".png"
    return save_figure(png_path, I, V, status, rate, info["title"])


def figure_command(csv_path, png_path, rate_hz=None, title=""):
    """The command that draws a saved stream's PNG in its own process.

    The degausser session uses it: matplotlib holds the interpreter lock
    for ~0.3 s drawing a 75k-row cycle, and tio's serial thread ends the
    whole process (os._exit, bench 2026-11-04) when it falls ~0.2 s
    behind. A child process has its own lock."""
    import sys
    cmd = [sys.executable, "-m", "bfield.analysis.dg_waveform",
           csv_path, png_path, "--title", title or ""]
    if rate_hz:
        cmd += ["--rate", repr(float(rate_hz))]
    return cmd


def main(argv=None):
    import argparse
    p = argparse.ArgumentParser(
        description="Draw a degauss stream's PNG (current, voltage, I-V).")
    p.add_argument("csv")
    p.add_argument("png", nargs="?", default=None)
    p.add_argument("--rate", type=float, default=None)
    p.add_argument("--title", default=None)
    a = p.parse_args(argv)
    if a.rate is None and a.title is None:
        figure_for_stream(a.csv, a.png)
        return 0
    columns, rows = load_stream(a.csv)
    I, V, status = rows_to_arrays(rows, columns)
    save_figure(a.png or os.path.splitext(a.csv)[0] + ".png", I, V, status,
                a.rate, a.title or "")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
