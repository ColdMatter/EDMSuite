"""The Analysis tab: the noise spectrum of a recorded run.

The amplitude spectral density in fT/rtHz against Hz, computed the house
way (bfield.analysis.spectra.spliced_asd: Welch with linear detrend,
200 s segments below 1 Hz and 20 s above, spliced at 1 Hz -- the
convention behind every published plateau number). It reads the run's
*_pT.txt files and meta.json (fs, status, which sensor is which), so it
works on any run folder: the last record, or any old run under the data
folder.

One number per channel: the white noise, the mean ASD from 10 Hz up
to the Nyquist frequency (both limits editable). Mains and carrier lines
in that range raise a mean slightly; the median is in the exported CSV
header for comparison.

Export data writes the plotted spectrum as CSV: frequency, then one
column per channel, in fT/rtHz, with the run and the method in '#'
header lines.
"""

from __future__ import annotations

import os
import threading

import numpy as np
from PySide6.QtCore import QObject, Qt, Signal
from PySide6.QtWidgets import (QCheckBox, QDoubleSpinBox, QFileDialog,
                               QFormLayout, QGroupBox, QHBoxLayout, QLabel,
                               QHeaderView, QLineEdit, QPushButton,
                               QSplitter,
                               QTableWidget,
                               QTableWidgetItem, QVBoxLayout, QWidget)

from bfield.gui.plot_export import PlotActions

WHITE_FROM_HZ = 10.0
COLOURS = {"z": "#1f77b4", "y": "#ff7f0e", "z2": "#2ca02c", "y2": "#d62728",
           "x": "#9467bd", "x2": "#8c564b"}


def white_noise(f, a, lo=WHITE_FROM_HZ, hi=0.0):
    """(mean, median) ASD over lo <= f <= hi (hi 0 = up to Nyquist)."""
    hi = hi if hi and hi > lo else float(f[-1])
    m = (f >= lo) & (f <= hi) & np.isfinite(a)
    if not m.any():
        return float("nan"), float("nan")
    return float(np.mean(a[m])), float(np.median(a[m]))


def compute_spectra(run_dir, channels, lowseg, hiseg, split, skip_s=0.0,
                    white_lo=WHITE_FROM_HZ, white_hi=0.0):
    """{channel: (f, asd_fT)} and info for one run (no Qt: testable).
    skip_s drops the start of the record (a settling transient; the old
    sync5 analysis dropped 300 s)."""
    from bfield.analysis.spectra import spliced_asd
    from bfield.io.runs import channel_files, read_channel, read_meta
    meta = read_meta(run_dir) or {}
    files = channel_files(run_dir)
    out, bands = {}, {}
    fs_used = None
    for ch in channels:
        path = files.get(ch)
        if path is None or not path.endswith("_pT.txt"):
            continue
        x, head = read_channel(path)
        fs = float(meta.get("fs_hz") or head.get("fs") or 0)
        if not fs:
            raise ValueError(f"no sample rate for {os.path.basename(path)}")
        fs_used = fs
        x = x[np.isfinite(x)]
        x = x[int(max(skip_s, 0.0) * fs):]
        if x.size < 64:
            raise ValueError(f"{ch}: nothing left after skipping "
                             f"{skip_s:g} s")
        f, a = spliced_asd(x, fs, lowseg=lowseg, hiseg=hiseg, split=split)
        a_ft = a * 1e3                         # pT/rtHz -> fT/rtHz
        out[ch] = (f, a_ft)
        mean, median = white_noise(f, a_ft, white_lo, white_hi)
        bands[ch] = {"white": mean, "white_median": median,
                     "white_band": (white_lo, white_hi or fs / 2.0),
                     "seconds": x.size / fs}
    return out, bands, meta, fs_used


def write_spectra_csv(path, result, run_dir, method):
    """frequency_Hz, then one ASD column per channel (fT/rtHz). '#' lines
    say which run, how it was computed, and each channel's white noise.
    Channels on different frequency grids get one block each."""
    spectra, bands, meta, fs = result
    head = ["# amplitude spectral density, fT/rtHz",
            f"# run: {run_dir}",
            f"# session: {meta.get('session')}  start: "
            f"{meta.get('start_iso')}  fs: {fs:g} Hz  status: "
            f"{meta.get('status')} (exit {meta.get('exit_code')})",
            "# method: Welch, Hann, linear detrend, spliced; "
            + "; ".join(f"{k} {v:g}" for k, v in method.items())]
    for ch, b in bands.items():
        lo, hi = b["white_band"]
        head.append(f"# {ch}: white noise {lo:g}-{hi:g} Hz: mean "
                    f"{b['white']:.3f}, median {b['white_median']:.3f}")
    chans = list(spectra)
    grids = [spectra[c][0] for c in chans]
    same = all(g.shape == grids[0].shape and np.allclose(g, grids[0])
               for g in grids)
    with open(path, "w", newline="\n", encoding="utf-8") as fh:
        fh.write("\n".join(head) + "\n")
        if same:
            fh.write("frequency_Hz," + ",".join(f"{c}_fT_per_rtHz"
                                                 for c in chans) + "\n")
            cols = np.column_stack([grids[0]] + [spectra[c][1]
                                                 for c in chans])
            np.savetxt(fh, cols, delimiter=",", fmt="%.6g")
        else:
            for c in chans:
                fh.write(f"frequency_Hz,{c}_fT_per_rtHz\n")
                np.savetxt(fh, np.column_stack(spectra[c]), delimiter=",",
                           fmt="%.6g")


class _Done(QObject):
    finished = Signal(object)


class AnalysisPanel(QWidget):
    def __init__(self, data_root=lambda: "", say=None):
        super().__init__()
        import pyqtgraph as pg
        self.data_root = data_root
        self.say = say
        self.last_run = None
        self.run_dir = None
        self.result = None
        self.who = {}
        self._done = _Done()
        self._done.finished.connect(self._show)
        # settings and numbers on the left, the spectrum on the right
        # (the tab is wide and short; stacked, the plot got no height)
        outer = QHBoxLayout(self)
        outer.setContentsMargins(4, 4, 4, 4)
        split = QSplitter(Qt.Horizontal)
        outer.addWidget(split)
        left = QWidget()
        lay = QVBoxLayout(left)
        lay.setContentsMargins(0, 0, 4, 0)

        lay.addWidget(QLabel("<b>Run folder</b>"))
        self.path = QLineEdit()
        self.path.setPlaceholderText("a run folder (with a .meta.json and "
                                     "*_pT.txt files)")
        self.path.returnPressed.connect(lambda: self.load(self.path.text()))
        lay.addWidget(self.path)
        src = QHBoxLayout()
        browse = QPushButton("Browse...")
        browse.setToolTip("Any run folder: a simulated run, or real data "
                          "under the Box data folder")
        browse.clicked.connect(self.browse)
        src.addWidget(browse)
        self.last_btn = QPushButton("Last record")
        self.last_btn.setToolTip("The run the last QZFM record wrote")
        self.last_btn.setEnabled(False)
        self.last_btn.clicked.connect(lambda: self.load(self.last_run))
        src.addWidget(self.last_btn)
        src.addStretch(1)
        lay.addLayout(src)

        self.info = QLabel("Choose a run folder.")
        self.info.setWordWrap(True)
        self.info.setStyleSheet("color:#444")
        lay.addWidget(self.info)

        self.chan_box = QGroupBox("Channels")
        self.chan_lay = QVBoxLayout(self.chan_box)
        self.chan_checks = {}
        lay.addWidget(self.chan_box)
        meth = QGroupBox("Welch, linear detrend, spliced")
        meth.setToolTip("The house convention behind the published plateau "
                        "numbers: 200 s segments below 1 Hz, 20 s above")
        ml = QFormLayout(meth)
        self.lowseg = self._spin(1, 3600, 200, " s")
        self.hiseg = self._spin(0.5, 600, 20, " s")
        self.split = self._spin(0.01, 100, 1.0, " Hz", 2)
        ml.addRow("segment below split", self.lowseg)
        ml.addRow("segment above split", self.hiseg)
        ml.addRow("split at", self.split)
        self.skip = self._spin(0, 36000, 0, " s", 0)
        self.skip.setToolTip("Drop the start of the record, e.g. a "
                             "settling transient after zeroing (the old "
                             "sync5 analysis dropped 300 s)")
        ml.addRow("skip the first", self.skip)
        self.white_lo = self._spin(0.01, 1000, WHITE_FROM_HZ, " Hz", 1)
        self.white_hi = self._spin(0, 100000, 0, " Hz", 0)
        self.white_hi.setSpecialValueText("Nyquist")
        self.white_lo.setToolTip("White noise = mean ASD from here up")
        self.white_hi.setToolTip("...to here (Nyquist = fs/2)")
        ml.addRow("white noise from", self.white_lo)
        ml.addRow("white noise to", self.white_hi)
        lay.addWidget(meth)

        self.compute_btn = QPushButton("Compute spectrum")
        self.compute_btn.setEnabled(False)
        self.compute_btn.clicked.connect(self.compute)
        lay.addWidget(self.compute_btn)
        self.status = QLabel("")
        self.status.setWordWrap(True)
        lay.addWidget(self.status)

        self.table = QTableWidget(0, 2)
        self.table.setHorizontalHeaderLabels(["channel", "white noise"])
        self.table.horizontalHeaderItem(1).setToolTip(
            "mean ASD over the white-noise band (10 Hz to Nyquist by "
            "default), fT/rtHz")
        self.table.horizontalHeader().setSectionResizeMode(
            0, QHeaderView.ResizeToContents)
        self.table.verticalHeader().setVisible(False)
        self.table.setEditTriggers(QTableWidget.NoEditTriggers)
        self.table.horizontalHeader().setStretchLastSection(True)
        self.table.setMinimumHeight(118)
        lay.addWidget(QLabel("<b>Band levels</b> (fT/√Hz)"))
        lay.addWidget(self.table, 1)
        # the column scrolls, so it never sets the window's minimum height
        # (a long status reason made it taller still)
        from PySide6.QtWidgets import QScrollArea
        left_scroll = QScrollArea()
        left_scroll.setWidgetResizable(True)
        left_scroll.setFrameShape(QScrollArea.NoFrame)
        left_scroll.setHorizontalScrollBarPolicy(Qt.ScrollBarAsNeeded)
        left_scroll.setWidget(left)
        left_scroll.setMinimumWidth(250)
        split.addWidget(left_scroll)

        right = QWidget()
        rl = QVBoxLayout(right)
        rl.setContentsMargins(4, 0, 0, 0)
        trow = QHBoxLayout()
        trow.addWidget(QLabel("Plot title:"))
        self.title_edit = QLineEdit()
        self.title_edit.setPlaceholderText("default: the run folder's name")
        self.title_edit.textChanged.connect(self._apply_title)
        trow.addWidget(self.title_edit, 1)
        rl.addLayout(trow)
        self.plot = pg.PlotWidget()
        self.plot.setLogMode(x=True, y=True)
        self.plot.setLabel("left", "Amplitude spectral density (fT/√Hz)")
        self.plot.setLabel("bottom", "frequency (Hz)")
        self.plot.showGrid(x=True, y=True, alpha=0.3)
        self.plot.setMinimumHeight(60)
        legend = self.plot.addLegend()
        legend.setBrush(pg.mkBrush(255, 255, 255, 220))
        from PySide6.QtWidgets import QTextBrowser
        vsplit = QSplitter(Qt.Vertical)
        vsplit.addWidget(self.plot)
        notes_box = QGroupBox("Run notes")
        nl = QVBoxLayout(notes_box)
        nl.setContentsMargins(4, 2, 4, 4)
        self.notes = QTextBrowser()
        self.notes.setOpenExternalLinks(True)
        self.notes.setPlaceholderText(
            "What was recorded about the run's quality appears here: the "
            "recorder's verdict and reason, flagged channels, warnings, the "
            "operator's notes, the session's setup warnings, and any "
            "NOTES.md / README.md in the folder.")
        self.notes.setMinimumHeight(30)
        nl.addWidget(self.notes)
        vsplit.addWidget(notes_box)
        vsplit.setChildrenCollapsible(False)
        vsplit.setSizes([380, 220])
        rl.addWidget(vsplit, 1)
        self.actions = PlotActions(
            self.plot, name=self._fig_name,
            folder=lambda: self.run_dir or self.data_root(), say=say,
            export_data=self.export_data)
        rl.addWidget(self.actions)
        split.addWidget(right)
        split.setStretchFactor(1, 1)
        split.setSizes([270, 700])

    @staticmethod
    def _spin(lo, hi, value, suffix, decimals=1):
        w = QDoubleSpinBox()
        w.setRange(lo, hi)
        w.setDecimals(decimals)
        w.setValue(value)
        w.setSuffix(suffix)
        return w

    def _fig_name(self):
        if not self.run_dir:
            return "asd"
        return f"asd_{os.path.basename(os.path.normpath(self.run_dir))}"

    # -- choosing a run --------------------------------------------------

    def set_last_run(self, run_dir):
        self.last_run = run_dir
        self.last_btn.setEnabled(bool(run_dir))
        self.last_btn.setToolTip(f"The last record: {run_dir}")

    def browse(self):
        # the run already open's bucket, else the data folder (Box)
        start = (os.path.dirname(os.path.normpath(self.run_dir))
                 if self.run_dir else self.data_root() or "")
        d = QFileDialog.getExistingDirectory(self, "Run folder", start)
        if d:
            self.load(d)

    def load(self, run_dir):
        """Read the run's description and offer its channels."""
        from bfield.io.files import read_meta
        from bfield.io.runs import channel_files
        run_dir = (run_dir or "").strip().strip('"')
        files = channel_files(run_dir) if run_dir and os.path.isdir(run_dir) \
            else {}
        pt = sorted((c for c, p in files.items() if p.endswith("_pT.txt")),
                    key=lambda c: ("zyx".find(c[0]), c))
        if not pt:
            self.info.setText(f"No *_pT.txt channel files in {run_dir!r} -- "
                              "pick the run folder itself (e.g. R01_1435).")
            self.compute_btn.setEnabled(False)
            return False
        self.run_dir = run_dir
        self.path.setText(run_dir)
        meta = read_meta(run_dir) or {}
        who = {}
        for rec in meta.get("sensor_info") or []:
            for ch in ("z", "y"):
                name = ch if rec.get("head") == "a" else f"{ch}2"
                who[name] = (f"sensor {rec.get('number')} "
                             f"({rec.get('label')})")
        from html import escape
        from bfield.io.runs import run_notes, short_flag
        notes = run_notes(run_dir)
        self.run_notes = notes
        # a new run: the previous run's spectrum and numbers go
        self.plot.clear()
        self.plot.setTitle("")
        self.table.setRowCount(0)
        self.result = None
        self.status.setText("")
        status = meta.get("status", "?")
        warn = status != "ok"
        reason = notes.get("reason")
        self.info.setText(
            f"<b>{meta.get('session', os.path.basename(run_dir))}</b>  "
            f"start {meta.get('start_iso', '?')},  "
            f"{(meta.get('duration_s') or 0):.0f} s at "
            f"{meta.get('fs_hz', '?')} Hz,  status "
            + (f'<span style="color:#c62828"><b>{status}</b> (exit '
               f'{meta.get("exit_code")}: {escape(notes.get("meaning") or "")})'
               "</span>" if warn else status)
            + ("  <i>(simulated)</i>" if meta.get("source") == "simulate"
               else "")
            + (f'<br><span style="color:#c62828">{escape(reason)}</span>'
               if reason else "")
            + ("<br><b>Read the notes below:</b> "
               + ", ".join(os.path.basename(p)
                           for p, _ in notes.get("note_files") or [])
               + " in the folder" if notes.get("note_files") else ""))
        for cb in self.chan_checks.values():
            cb.setParent(None)
        self.chan_checks = {}
        flags = notes.get("channels") or {}
        for ch in pt:
            problems = flags.get(ch, [])
            bad = any(level == "bad" for level, _ in problems)
            text = ch.upper() + (f"  {who[ch]}" if ch in who else "")
            if problems:
                text += "   \u26a0 " + ", ".join(
                    short_flag(t) for _, t in problems)
            cb = QCheckBox(text)
            if problems:
                cb.setStyleSheet("color:#c62828" if bad else "color:#b26a00")
                cb.setToolTip("\n".join(t for _, t in problems)
                              + ("\n\nUnticked: its spectrum would show "
                                 "the fault, not the field. Tick it to "
                                 "plot it anyway." if bad else ""))
            # a clipped / pinned / flat / non-finite channel starts unticked
            cb.setChecked(not bad)
            self.chan_checks[ch] = cb
            self.chan_lay.addWidget(cb)
        self.notes.setMarkdown(self._notes_markdown(notes, who))
        self.compute_btn.setEnabled(True)
        self.who = who
        return True

    @staticmethod
    def _notes_markdown(n, who):
        """The run's recorded quality notes as markdown."""
        out = []
        if n.get("status") is not None:
            out.append(f"**Recorder:** {n['status']} (exit {n['exit_code']}: "
                       f"{n.get('meaning')})")
        if n.get("reason"):
            out.append(f"**Reason:** {n['reason']}")
        flags = n.get("channels") or {}
        if flags:
            out.append("**Flagged channels:**")
            for ch, problems in flags.items():
                name = ch.upper() + (f" ({who[ch]})" if ch in who else "")
                for level, text in problems:
                    out.append(f"- {name}: {text}"
                               + (" -- *warning*" if level == "warn" else ""))
        if n.get("warnings"):
            out.append("**Recorder warnings:**")
            out += [f"- {w}" for w in n["warnings"]]
        if n.get("operator_notes"):
            out.append(f"**Operator's notes:** {n['operator_notes']}")
        if n.get("session"):
            out.append("**From the session** "
                       f"(`{os.path.basename(n['session_file'])}`):")
            out += [f"- {line}" for line in n["session"]]
        for path, text in n.get("note_files") or []:
            out.append(f"---\n**{os.path.basename(path)}** "
                       f"(in `{os.path.basename(os.path.dirname(path))}`)"
                       f"\n\n{text}")
        if len(out) <= 1 and not flags:
            out.append("Nothing flagged; no notes recorded.")
        return "\n\n".join(out)

    # -- computing -------------------------------------------------------

    def compute(self):
        chans = [c for c, cb in self.chan_checks.items() if cb.isChecked()]
        if not chans or not self.run_dir:
            return
        self.compute_btn.setEnabled(False)
        self.status.setText("computing...")
        args = (self.run_dir, chans, self.lowseg.value(), self.hiseg.value(),
                self.split.value(), self.skip.value(),
                self.white_lo.value(), self.white_hi.value())

        def work():
            try:
                res = compute_spectra(*args)
            except Exception as e:
                res = e
            self._done.finished.emit(res)
        threading.Thread(target=work, daemon=True).start()

    def _show(self, res):
        self.compute_btn.setEnabled(True)
        if isinstance(res, Exception):
            self.status.setText(f"failed: {res}")
            return
        spectra, bands, meta, fs = res
        self.result = res
        self.plot.clear()
        folder = os.path.basename(os.path.normpath(self.run_dir))
        session = meta.get("session")
        self.default_title = (folder if not session or session == folder
                              else f"{folder}  (session {session})")
        self._apply_title()
        for ch, (f, a) in spectra.items():
            keep = (f > 0) & np.isfinite(a) & (a > 0)
            label = ch.upper() + (f" {self.who[ch]}" if ch in self.who
                                  else "")
            self.plot.plot(f[keep], a[keep], name=label,
                           pen=COLOURS.get(ch, "#555555"))
        self.actions.restyle()
        self.table.setRowCount(len(bands))
        for r, (ch, b) in enumerate(bands.items()):
            vals = (ch.upper(), f"{b['white']:.1f}")
            for c, v in enumerate(vals):
                item = QTableWidgetItem(v)
                if c:
                    item.setTextAlignment(Qt.AlignRight | Qt.AlignVCenter)
                self.table.setItem(r, c, item)
        secs = max((b["seconds"] for b in bands.values()), default=0)
        self.status.setText(
            f"{len(spectra)} channel(s), {secs:.0f} s at {fs:g} Hz; segments "
            f"{self.lowseg.value():g} s below / {self.hiseg.value():g} s "
            f"above {self.split.value():g} Hz"
            + (f"; first {self.skip.value():g} s skipped"
               if self.skip.value() else "")
            + "; white noise = mean ASD "
            + "{:g}-{:g} Hz".format(*next(iter(bands.values()))["white_band"])
            if bands else "")
        if self.say:
            self.say("[analysis] " + os.path.basename(self.run_dir) + ": "
                     + ";  ".join(f"{ch.upper()} white noise "
                                  f"{b['white']:.1f} fT/rtHz"
                                  for ch, b in bands.items()))

    def _apply_title(self, *_):
        text = self.title_edit.text().strip() or getattr(
            self, "default_title", "")
        self.plot.setTitle(text)
        self.actions.restyle()

    def export_data(self):
        """The plotted spectrum as CSV: frequency, then fT/rtHz per channel."""
        from PySide6.QtWidgets import QMessageBox
        if not self.result:
            QMessageBox.information(self, "Nothing to export",
                                    "Compute a spectrum first.")
            return
        spectra, bands, meta, fs = self.result
        start = os.path.join(self.run_dir or self.data_root() or "",
                             f"{self._fig_name()}.csv")
        path, _ = QFileDialog.getSaveFileName(self, "Export spectrum data",
                                              start, "CSV (*.csv)")
        if not path:
            return
        if not path.lower().endswith(".csv"):
            path += ".csv"
        try:
            write_spectra_csv(path, self.result, self.run_dir, {
                "segment below split (s)": self.lowseg.value(),
                "segment above split (s)": self.hiseg.value(),
                "split (Hz)": self.split.value(),
                "skipped at the start (s)": self.skip.value()})
        except OSError as e:
            QMessageBox.warning(self, "Export failed", f"{path}\n{e}")
            return
        if self.say:
            self.say(f"[analysis] spectrum data saved {path}")
