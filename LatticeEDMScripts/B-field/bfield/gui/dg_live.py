"""Degausser current / voltage / I-V traces: live and replayed.

What Twinleaf's DG Control VI shows, drawn from the stream the degauss
program already captures: current and voltage against time with the
phase changes marked, and the I-V loop. Every cycle's loop stays on the
I-V plot (older ones paler) so loops can be compared, e.g. the
amplitudes of a stepped --check.

  DgTraces      the plots, a QWidget (the GUI's degauss tabs use it)
  DgLiveWindow  a top-level window around it for run/degauss.py --plot
  DgReplay      DgTraces plus file buttons: saved stream_cNN.csv files
  replay()      run/degauss.py --replay

A display only. The cycle never waits on it: closing the window just
stops the drawing, and DegaussSession guards every call (the first
error drops the display with one warning).
"""

from __future__ import annotations

import os
import time

import numpy as np
from PySide6.QtCore import Qt
from PySide6.QtWidgets import (QApplication, QFileDialog, QHBoxLayout,
                               QLabel, QPushButton, QVBoxLayout, QWidget)

from bfield import clock
from bfield.analysis.dg_waveform import (PHASE_COLOURS, PHASE_LABELS,
                                         STREAM_ROW_HZ_NOMINAL,
                                         column_indices, load_stream,
                                         phase_changes, rate_from_taper,
                                         rows_to_arrays, session_info)

DRAW_MIN_S = 1.0           # --plot: shortest gap between redraws (real s)
PUMP_S = 0.03            # event-loop slice while waiting between polls
LOOP_COLOURS = ["#1e88e5", "#e53935", "#43a047", "#fb8c00", "#8e24aa",
                "#00897b", "#6d4c41", "#3949ab"]


def _app():
    return QApplication.instance() or QApplication([])


class DgTraces(QWidget):
    """Current and voltage against time (the cycle being drawn) over the
    I-V loops (every cycle since clear())."""

    def __init__(self, parent=None):
        import pyqtgraph as pg

        from bfield.gui.plot_export import legend_outside

        super().__init__(parent)
        self.pg = pg
        lay = QVBoxLayout(self)
        lay.setContentsMargins(2, 2, 2, 2)
        self.label = QLabel("no degauss cycle yet")
        self.label.setWordWrap(True)
        lay.addWidget(self.label)
        glw = pg.GraphicsLayoutWidget()
        glw.setBackground("w")
        lay.addWidget(glw, 2)
        self.p_i = glw.addPlot(row=0, col=0)
        self.p_v = glw.addPlot(row=1, col=0)
        self.p_v.setXLink(self.p_i)
        self.p_i.setLabel("left", "current", units="A")
        self.p_v.setLabel("left", "voltage", units="V")
        self.p_v.setLabel("bottom", "time", units="s")
        for p in (self.p_i, self.p_v):
            p.showGrid(x=True, y=True, alpha=0.3)
            p.setDownsampling(auto=True, mode="peak")
            p.setClipToView(True)
        self.loop_widget = pg.PlotWidget(background="w")
        lay.addWidget(self.loop_widget, 2)
        self.p_l = self.loop_widget.plotItem
        self.p_l.setLabel("bottom", "voltage", units="V")
        self.p_l.setLabel("left", "current", units="A")
        self.p_l.showGrid(x=True, y=True, alpha=0.3)
        self.legend = legend_outside(self.loop_widget)
        self.c_i = self.p_i.plot(pen=pg.mkPen(LOOP_COLOURS[0], width=1))
        self.c_v = self.p_v.plot(pen=pg.mkPen(LOOP_COLOURS[0], width=1))
        self.loops = []
        self.c_l = None
        self.phase_lines = []
        self.title = ""
        self._reset_cycle()

    def _reset_cycle(self):
        self.n = 0
        self.I = np.empty(0)
        self.V = np.empty(0)
        self.S = np.empty(0)
        self.rate = None
        self.t_first = None
        self.last_code = None
        self.dirty = False

    def clear(self):
        """Forget every cycle (the loops too)."""
        self._clear_phase_lines()
        for c in self.loops:
            self.p_l.removeItem(c)
        self.legend.clear()
        self.loops = []
        self.c_l = None
        self.c_i.setData([], [])
        self.c_v.setData([], [])
        self._reset_cycle()
        self.label.setText("no degauss cycle yet")

    def _clear_phase_lines(self):
        for line in self.phase_lines:
            line._plot.removeItem(line)
        self.phase_lines = []

    def start_cycle(self, idx, title, label=None):
        self._clear_phase_lines()
        self._reset_cycle()
        self.c_i.setData([], [])
        self.c_v.setData([], [])
        pg = self.pg
        # earlier loops stay, paler, for comparison
        for c in self.loops:
            col = pg.mkPen(c.opts["pen"]).color()
            col.setAlpha(90)
            c.setPen(pg.mkPen(col, width=1))
        colour = LOOP_COLOURS[len(self.loops) % len(LOOP_COLOURS)]
        self.c_l = self.p_l.plot(pen=pg.mkPen(colour, width=1),
                                 name=label or f"cycle {idx}")
        self.loops.append(self.c_l)
        self.c_i.setPen(pg.mkPen(colour, width=1))
        self.c_v.setPen(pg.mkPen(colour, width=1))
        self.title = title
        self.label.setText(title)

    def add_rows(self, new, columns, draw=True):
        """Append stream rows to the cycle being drawn. draw=False only
        marks the plots stale (flush() draws them): a GUI coalesces a
        fast simulated stream into a few redraws a second."""
        if not new:
            return
        if self.t_first is None:
            self.t_first = clock.monotonic()
        i, v, s = rows_to_arrays(new, columns)
        start = self.n
        self.I = np.concatenate([self.I, i])
        self.V = np.concatenate([self.V, v])
        self.S = np.concatenate([self.S, s])
        self.n += len(new)
        for k, code in phase_changes(s):
            if code == self.last_code:
                continue
            self.last_code = code
            self._phase_line(start + k, code)
        if draw:
            self._redraw()
        else:
            self.dirty = True
        phase = PHASE_LABELS.get(self.last_code, str(self.last_code))
        peak = np.nanmax(np.abs(i)) if np.isfinite(i).any() else float("nan")
        self.label.setText(f"{self.title}    |    {phase}, "
                           f"|I| {peak:.2f} A, {self.n} rows")

    def _phase_line(self, k, code):
        pg = self.pg
        col = PHASE_COLOURS.get(code, "#000000")
        for p, show_label in ((self.p_i, True), (self.p_v, False)):
            line = pg.InfiniteLine(
                pos=0, angle=90, pen=pg.mkPen(col, style=Qt.DashLine),
                label=PHASE_LABELS.get(code, str(code)) if show_label
                else None,
                # staggered, so close phase changes don't overprint
                labelOpts={"position": (0.92, 0.72, 0.52)[
                    len(self.phase_lines) // 2 % 3], "color": col})
            line._row = k
            line._plot = p
            p.addItem(line)
            self.phase_lines.append(line)

    def flush(self):
        if self.dirty:
            self._redraw()

    def _redraw(self, rate=None):
        self.dirty = False
        if rate:
            self.rate = rate
        elif self.t_first is not None:
            dt = clock.monotonic() - self.t_first
            # measured arrival rate once a few seconds are in, else nominal
            self.rate = (self.n / dt if dt > 3.0
                         else (self.rate or STREAM_ROW_HZ_NOMINAL))
        r = self.rate or STREAM_ROW_HZ_NOMINAL
        t = np.arange(self.n) / r
        self.c_i.setData(t, self.I)
        self.c_v.setData(t, self.V)
        if self.c_l is not None:
            self.c_l.setData(self.V, self.I)
        for line in self.phase_lines:
            line.setPos(line._row / r)

    def end_cycle(self, rec):
        """The cycle's record is final: exact row rate, outcome."""
        rate = (rec.get("qc") or {}).get("stream_rate_hz")
        if self.n:
            self._redraw(rate)
        self.label.setText(f"{self.title}    |    outcome: "
                           f"{rec.get('outcome')}"
                           + (f", {self.n} rows at {rate:.0f}/s" if rate
                              else ""))

    def show_stream(self, idx, path, label=None):
        """Draw a saved stream_cNN.csv as one more cycle; False if it has
        no current column."""
        columns, rows = load_stream(path)
        if column_indices(columns)[0] is None:
            return False
        info = session_info(path)
        self.start_cycle(idx, info["title"], label=label or stream_label(path))
        self.add_rows(rows, columns)
        self._redraw(info["rate_hz"]
                     or rate_from_taper(self.S, info.get("taper_s"))
                     or STREAM_ROW_HZ_NOMINAL)
        return True


def stream_label(path):
    """'001_Degauss/stream_c01.csv' -- the run folder and the file."""
    p = os.path.abspath(path)
    return os.path.basename(os.path.dirname(p)) + "/" + os.path.basename(p)


class DgLiveWindow:
    """DgTraces in its own window, for run/degauss.py --plot. Opens at
    the first start_cycle (not at construction: the safety questions come
    first, on the terminal, and an unpumped window would sit there 'not
    responding'). The session calls update() with its whole row list."""

    def __init__(self, title="Degauss"):
        import pyqtgraph  # noqa: F401  -- fail now, not mid-cycle
        self.window_title = title
        self.win = None
        self.traces = None
        self.closed = False
        self.fed = 0            # rows of the current cycle already drawn
        self.t_drawn = 0.0      # time.monotonic() of the last redraw
        self.draw_cost = 0.0    # seconds the last redraw took

    def _build(self):
        self.app = _app()
        outer = self

        class Window(QWidget):
            def closeEvent(self, ev):
                outer.closed = True
                ev.accept()

        self.win = Window()
        self.win.setWindowTitle(self.window_title)
        self.win.resize(1000, 850)
        lay = QVBoxLayout(self.win)
        self.traces = DgTraces()
        lay.addWidget(self.traces)
        self.win.show()
        self.pump()

    @property
    def loops(self):
        return self.traces.loops if self.traces is not None else []

    def start_cycle(self, idx, title, label=None):
        if self.closed:
            return
        if self.win is None:
            self._build()
        self.fed = 0
        self.traces.start_cycle(idx, title, label)
        self.pump()

    def update(self, rows, columns):
        if self.win is None or self.closed or len(rows) <= self.fed:
            self.pump()
            return
        # the redraw runs on the session's thread, between stream polls:
        # at most one a second, and never more than ~1/4 of the time
        self.traces.add_rows(rows[self.fed:], columns, draw=False)
        self.fed = len(rows)
        now = time.monotonic()
        if now - self.t_drawn >= max(DRAW_MIN_S, 4.0 * self.draw_cost):
            self.traces.flush()
            self.t_drawn = time.monotonic()
            self.draw_cost = self.t_drawn - now
        self.pump()

    def end_cycle(self, rec):
        if self.win is None or self.closed:
            return
        self.traces.end_cycle(rec)
        self.pump()

    def pump(self):
        if self.win is not None and not self.closed:
            self.app.processEvents()

    def wait(self, seconds):
        """clock.sleep(seconds) that keeps the window responsive."""
        end = time.monotonic() + clock.real_seconds(seconds)
        while True:
            self.pump()
            left = end - time.monotonic()
            if left <= 0:
                return
            time.sleep(min(PUMP_S, left))

    def hold(self, say=None):
        """Keep the window up until the operator closes it."""
        if self.win is None or self.closed:
            return
        if say:
            say("plot", "close the plot window to finish")
        while not self.closed:
            self.app.processEvents()
            time.sleep(PUMP_S)

    def close(self):
        if self.win is not None and not self.closed:
            self.closed = True
            self.win.close()
            self.app.processEvents()


class DgReplay(QWidget):
    """Saved degauss streams on DgTraces: open files (loops overlaid),
    the last run's streams, clear."""

    def __init__(self, data_root=lambda: "", say=None, parent=None):
        super().__init__(parent)
        self.data_root = data_root
        self.say = say or (lambda m: None)
        self.last_run = None
        self.shown = []
        lay = QVBoxLayout(self)
        lay.setContentsMargins(2, 2, 2, 2)
        bar = QHBoxLayout()
        self.open_btn = QPushButton("Open streams...")
        self.open_btn.setToolTip("Pick one or more stream_cNN.csv files "
                                 "from degauss runs; their I-V loops are "
                                 "overlaid")
        self.open_btn.clicked.connect(self.browse)
        self.last_btn = QPushButton("Last run")
        self.last_btn.setToolTip("The streams of the degauss run this "
                                 "window ran last")
        self.last_btn.setEnabled(False)
        self.last_btn.clicked.connect(self.show_last_run)
        self.clear_btn = QPushButton("Clear")
        self.clear_btn.clicked.connect(self.clear)
        for b in (self.open_btn, self.last_btn, self.clear_btn):
            bar.addWidget(b)
        bar.addStretch(1)
        lay.addLayout(bar)
        self.files = QLabel("Open the stream_cNN.csv files of a degauss "
                            "run (each run folder has them, with a PNG of "
                            "the same plots).")
        self.files.setWordWrap(True)
        lay.addWidget(self.files)
        self.traces = DgTraces()
        lay.addWidget(self.traces, 1)

    def browse(self):
        start = self.last_run or self.data_root() or ""
        paths, _ = QFileDialog.getOpenFileNames(
            self, "Degauss streams", start,
            "Degauss streams (stream_c*.csv);;CSV files (*.csv)")
        if paths:
            self.load(paths)

    def set_last_run(self, folder):
        self.last_run = folder
        self.last_btn.setEnabled(bool(folder))

    def show_last_run(self):
        if not self.last_run or not os.path.isdir(self.last_run):
            return
        paths = sorted(os.path.join(self.last_run, f)
                       for f in os.listdir(self.last_run)
                       if f.startswith("stream_c") and f.endswith(".csv"))
        if not paths:
            self.say(f"[replay] no streams in {self.last_run} yet")
            return
        self.clear()
        self.load(paths)

    def load(self, paths):
        for path in paths:
            try:
                ok = self.traces.show_stream(len(self.shown) + 1, path)
            except (OSError, ValueError) as e:
                self.say(f"[replay] {path}: {e}")
                continue
            if not ok:
                self.say(f"[replay] {path}: no current column -- skipped")
                continue
            self.shown.append(path)
        self.files.setText("Shown: " + ";  ".join(stream_label(p)
                                                  for p in self.shown))

    def clear(self):
        self.traces.clear()
        self.shown = []
        self.files.setText("")


def replay(paths, say=print):
    """Saved stream_cNN.csv files in their own window (loops overlaid);
    returns once it is closed."""
    win = DgLiveWindow("Degauss replay")
    for k, path in enumerate(paths, start=1):
        if win.win is None:
            win._build()
        if not win.traces.show_stream(k, path):
            say("plot", f"{path}: no current column -- skipped")
    win.pump()
    win.hold(say=say)
