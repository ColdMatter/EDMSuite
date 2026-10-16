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

Or, chosen at the top ("calculate"), the overlapping Allan deviation in
fT against the averaging time tau (bfield.analysis.allan), with sigma at
1 s and the minimum (and its tau) per channel; its CSV has tau, then
each channel's deviation and a rough error.

Or the coherence between two sensors' channels against frequency: near 1
where both see the same field, near 0 where the noise is each sensor's
own (or local to it). By default only the same axis on different
sensors (S1Z with S2Z); the table gives the mean per band and the plot a
dashed line at the level unrelated noise reaches 5 % of the time.
"""

from __future__ import annotations

import glob
import os
import threading

import numpy as np
from PySide6.QtCore import QObject, Qt, Signal
from PySide6.QtWidgets import (QCheckBox, QComboBox, QDoubleSpinBox,
                               QFileDialog,
                               QFormLayout, QGroupBox, QHBoxLayout, QLabel,
                               QHeaderView, QLineEdit, QPushButton,
                               QSpinBox, QSplitter,
                               QTableWidget,
                               QTableWidgetItem, QVBoxLayout, QWidget)

from bfield.channels import PALETTE, split_channel
from bfield.channels import colour as channel_colour
from bfield.gui.plot_export import PlotActions

WHITE_FROM_HZ = 10.0
ASD, ADEV = "Amplitude spectral density", "Allan deviation"
COH = "Coherence between sensors"
KINDS = (ASD, ADEV, COH)
COH_SEGMENT_S = 100.0          # 0.01 Hz bins: the sub-Hz band is the point
# the bands the table averages the coherence over (0 = Nyquist)
COH_BANDS = ((0.0, 0.1), (0.1, 1.0), (1.0, 10.0), (10.0, 0.0))


def white_noise(f, a, lo=WHITE_FROM_HZ, hi=0.0):
    """(mean, median) ASD over lo <= f <= hi (hi 0 = up to Nyquist)."""
    hi = hi if hi and hi > lo else float(f[-1])
    m = (f >= lo) & (f <= hi) & np.isfinite(a)
    if not m.any():
        return float("nan"), float("nan")
    return float(np.mean(a[m])), float(np.median(a[m]))


def record_inside(folder):
    """A session folder (e.g. 001_SingleSensorZ, holding R01_1543/ and a
    _guard/) -> its record folder: the newest sub-folder with *_pT.txt
    files, guard records left out. None when the folder holds the
    channel files itself, or no record."""
    if not folder or not os.path.isdir(folder) or \
            glob.glob(os.path.join(folder, "*_pT.txt")):
        return None
    hits = [d for d in glob.glob(os.path.join(folder, "*", ""))
            if not os.path.basename(os.path.normpath(d)).startswith("_")
            and glob.glob(os.path.join(d, "*_pT.txt"))]
    if not hits:
        return None
    return os.path.normpath(max(hits, key=os.path.getmtime))


def _channels(run_dir, channels, skip_s):
    """(meta, [(channel, pT samples, fs)]) for the run's *_pT.txt files,
    non-finite samples and the first skip_s seconds dropped."""
    from bfield.io.runs import channel_files, read_channel, read_meta
    meta = read_meta(run_dir) or {}
    files = channel_files(run_dir)
    out = []
    for ch in channels:
        path = files.get(ch)
        if path is None or not path.endswith("_pT.txt"):
            continue
        x, head = read_channel(path)
        fs = float(meta.get("fs_hz") or head.get("fs") or 0)
        if not fs:
            raise ValueError(f"no sample rate for {os.path.basename(path)}")
        x = x[np.isfinite(x)]
        x = x[int(max(skip_s, 0.0) * fs):]
        if x.size < 64:
            raise ValueError(f"{ch}: nothing left after skipping "
                             f"{skip_s:g} s")
        out.append((ch, x, fs))
    return meta, out


def compute_spectra(run_dir, channels, lowseg, hiseg, split, skip_s=0.0,
                    white_lo=WHITE_FROM_HZ, white_hi=0.0):
    """{channel: (f, asd_fT)} and info for one run (no Qt: testable).
    skip_s drops the start of the record (a settling transient; the old
    sync5 analysis dropped 300 s)."""
    from bfield.analysis.spectra import spliced_asd
    meta, data = _channels(run_dir, channels, skip_s)
    out, bands = {}, {}
    fs_used = None
    for ch, x, fs in data:
        fs_used = fs
        f, a = spliced_asd(x, fs, lowseg=lowseg, hiseg=hiseg, split=split)
        a_ft = a * 1e3                         # pT/rtHz -> fT/rtHz
        out[ch] = (f, a_ft)
        mean, median = white_noise(f, a_ft, white_lo, white_hi)
        bands[ch] = {"white": mean, "white_median": median,
                     "white_band": (white_lo, white_hi or fs / 2.0),
                     "seconds": x.size / fs}
    return out, bands, meta, fs_used


def compute_allan(run_dir, channels, skip_s=0.0, per_decade=10,
                  max_tau=0.0, detrend=False):
    """{channel: (tau_s, adev_fT, err_fT)} and info for one run (no Qt).
    bands per channel: sigma at 1 s, the minimum and its tau."""
    from bfield.analysis import allan
    meta, data = _channels(run_dir, channels, skip_s)
    out, bands = {}, {}
    fs_used = None
    for ch, x, fs in data:
        fs_used = fs
        tau, a, err = allan.overlapping_adev(
            x, fs, per_decade=per_decade, max_tau=max_tau, detrend=detrend)
        a, err = a * 1e3, err * 1e3            # pT -> fT
        out[ch] = (tau, a, err)
        low, at = allan.minimum(tau, a)
        bands[ch] = {"sigma_1s": allan.at_tau(tau, a, 1.0), "min": low,
                     "tau_min": at, "seconds": x.size / fs}
    return out, bands, meta, fs_used


def _sensor_axis(ch):
    """'s1z' -> ('s1', 'z'); an old name: 'z2' -> ('2', 'z'), 'y' -> ('1', 'y')."""
    key, axis = split_channel(ch)
    if key is not None:
        return key, axis
    return ("2" if ch.endswith("2") else "1"), ch[:1]


def channel_pairs(chans, same_axis=True):
    """Every pair of channels, in order; same_axis: only one axis seen by
    two different sensors (s1z/s2z) -- the pairs that say field or
    sensor."""
    out = []
    for i, a in enumerate(chans):
        for b in chans[i + 1:]:
            (ka, xa), (kb, xb) = _sensor_axis(a), _sensor_axis(b)
            if not same_axis or (xa == xb and ka != kb):
                out.append((a, b))
    return out


def coherence_threshold(n_independent, p=0.05):
    """The coherence that unrelated noise exceeds with probability p in a
    single bin, for n independent averages: 1 - p^(1/(n-1))."""
    if n_independent < 2:
        return float("nan")
    return 1.0 - p ** (1.0 / (n_independent - 1))


def compute_coherence(run_dir, channels, skip_s=0.0, seg_s=COH_SEGMENT_S,
                      same_axis=True):
    """{(a, b): (f, coherence)} for channel pairs, and info (no Qt).

    Magnitude-squared coherence (Welch, Hann, 50 % overlap, linear
    detrend): 1 = the two channels see the same signal at that frequency,
    ~0 = unrelated. bands per pair: the mean coherence in COH_BANDS and the
    95 % level unrelated noise would reach, and its mean ('chance': a band
    mean near it means no shared signal). Both from the non-overlapping
    segment count, so on the high side."""
    from scipy import signal
    meta, data = _channels(run_dir, channels, skip_s)
    by_name = {ch: (x, fs) for ch, x, fs in data}
    pairs = channel_pairs([ch for ch, _, _ in data], same_axis)
    if not pairs:
        raise ValueError(
            "no pair to compare -- tick two channels"
            + (" on the same axis of two sensors (or untick 'same axis "
               "only')" if same_axis else ""))
    out, bands = {}, {}
    fs_used = None
    for a, b in pairs:
        (xa, fa), (xb, fb) = by_name[a], by_name[b]
        if fa != fb:
            raise ValueError(f"{a} and {b} have different sample rates")
        fs_used = fa
        n = min(xa.size, xb.size)
        nper = int(min(seg_s * fa, n // 2))
        if nper < 16:
            raise ValueError(f"{a}/{b}: too short for a coherence")
        f, c = signal.coherence(xa[:n], xb[:n], fa, nperseg=nper,
                                detrend="linear")
        out[(a, b)] = (f, c)
        means = {}
        for lo, hi in COH_BANDS:
            hi = hi or fa / 2.0
            m = (f > 0) & (f >= lo) & (f < hi)
            means[(lo, hi)] = float(np.mean(c[m])) if m.any() else float("nan")
        bands[(a, b)] = {"means": means, "seg_s": nper / fa,
                         "level95": coherence_threshold(n // nper),
                         # what unrelated noise averages to over a band
                         "chance": 1.0 / max(n // nper, 1),
                         "seconds": n / fa}
    return out, bands, meta, fs_used


def write_coherence_csv(path, result, run_dir, method):
    """frequency_Hz, then one coherence column per pair ('#' lines: the
    run, the method, each pair's band means and 95 % level)."""
    curves, bands, meta, fs = result
    head = _run_header("magnitude-squared coherence between channels",
                       run_dir, meta, fs)
    head.append("# method: Welch, Hann, 50% overlap, linear detrend; "
                + "; ".join(f"{k} {v:g}" for k, v in method.items()))
    for (a, b), bd in bands.items():
        head.append(f"# {a}-{b}: unrelated noise: mean ~{bd['chance']:.3f}, "
                    f"95% of bins below {bd['level95']:.3f}; band means "
                    + ", ".join(f"{lo:g}-{hi:g} Hz {v:.3f}"
                                for (lo, hi), v in bd["means"].items()))
    named = {f"{a}-{b}": v for (a, b), v in curves.items()}
    with open(path, "w", newline="\n", encoding="utf-8") as fh:
        fh.write("\n".join(head) + "\n")
        _write_columns(fh, named, "frequency_Hz", "coherence")


def _write_columns(fh, curves, xname, yname, with_err=False):
    """One table if every channel has the same x grid, else one block
    per channel."""
    chans = list(curves)
    grids = [curves[c][0] for c in chans]
    same = all(g.shape == grids[0].shape and np.allclose(g, grids[0])
               for g in grids)
    blocks = [chans] if same else [[c] for c in chans]
    for block in blocks:
        names, cols = [xname], [curves[block[0]][0]]
        for c in block:
            names.append(f"{c}_{yname}")
            cols.append(curves[c][1])
            if with_err:
                names.append(f"{c}_err_{yname}")
                cols.append(curves[c][2])
        fh.write(",".join(names) + "\n")
        np.savetxt(fh, np.column_stack(cols), delimiter=",", fmt="%.6g")


def _run_header(title, run_dir, meta, fs):
    return [f"# {title}",
            f"# run: {run_dir}",
            f"# session: {meta.get('session')}  start: "
            f"{meta.get('start_iso')}  fs: {fs:g} Hz  status: "
            f"{meta.get('status')} (exit {meta.get('exit_code')})"]


def write_allan_csv(path, result, run_dir, method):
    """tau_s, then the Allan deviation and its rough error per channel
    (fT), with '#' lines saying which run and how."""
    curves, bands, meta, fs = result
    head = _run_header("overlapping Allan deviation, fT", run_dir, meta, fs)
    head.append("# method: overlapping ADEV of the field samples; err = "
                "adev/sqrt(independent intervals - 1); "
                + "; ".join(f"{k} {v:g}" for k, v in method.items()))
    for ch, b in bands.items():
        head.append(f"# {ch}: sigma(1 s) {b['sigma_1s']:.3f}, minimum "
                    f"{b['min']:.3f} at tau {b['tau_min']:.4g} s")
    with open(path, "w", newline="\n", encoding="utf-8") as fh:
        fh.write("\n".join(head) + "\n")
        _write_columns(fh, curves, "tau_s", "fT", with_err=True)


def write_spectra_csv(path, result, run_dir, method):
    """frequency_Hz, then one ASD column per channel (fT/rtHz). '#' lines
    say which run, how it was computed, and each channel's white noise.
    Channels on different frequency grids get one block each."""
    spectra, bands, meta, fs = result
    head = _run_header("amplitude spectral density, fT/rtHz", run_dir,
                       meta, fs)
    head.append("# method: Welch, Hann, linear detrend, spliced; "
                + "; ".join(f"{k} {v:g}" for k, v in method.items()))
    for ch, b in bands.items():
        lo, hi = b["white_band"]
        head.append(f"# {ch}: white noise {lo:g}-{hi:g} Hz: mean "
                    f"{b['white']:.3f}, median {b['white_median']:.3f}")
    with open(path, "w", newline="\n", encoding="utf-8") as fh:
        fh.write("\n".join(head) + "\n")
        _write_columns(fh, spectra, "frequency_Hz", "fT_per_rtHz")


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
        what = QFormLayout()
        self.kind = QComboBox()
        self.kind.addItems(KINDS)
        self.kind.setToolTip(
            "Amplitude spectral density: noise against frequency (the "
            "house numbers).\nAllan deviation: how well the field averages "
            "down against averaging time tau -- white noise falls as "
            "1/sqrt(tau), drift and 1/f noise make it level off or rise.\n"
            "Coherence: per frequency, whether two sensors see the same "
            "thing (1: a field both see) or not (0: each sensor's own "
            "noise, or something local to it).")
        what.addRow("calculate", self.kind)
        self.skip = self._spin(0, 36000, 0, " s", 0)
        self.skip.setToolTip("Drop the start of the record, e.g. a "
                             "settling transient after zeroing (the old "
                             "sync5 analysis dropped 300 s)")
        what.addRow("skip the first", self.skip)
        lay.addLayout(what)
        meth = QGroupBox("Welch, linear detrend, spliced")
        self.asd_box = meth
        meth.setToolTip("The house convention behind the published plateau "
                        "numbers: 200 s segments below 1 Hz, 20 s above")
        ml = QFormLayout(meth)
        self.lowseg = self._spin(1, 3600, 200, " s")
        self.hiseg = self._spin(0.5, 600, 20, " s")
        self.split = self._spin(0.01, 100, 1.0, " Hz", 2)
        ml.addRow("segment below split", self.lowseg)
        ml.addRow("segment above split", self.hiseg)
        ml.addRow("split at", self.split)
        self.white_lo = self._spin(0.01, 1000, WHITE_FROM_HZ, " Hz", 1)
        self.white_hi = self._spin(0, 100000, 0, " Hz", 0)
        self.white_hi.setSpecialValueText("Nyquist")
        self.white_lo.setToolTip("White noise = mean ASD from here up")
        self.white_hi.setToolTip("...to here (Nyquist = fs/2)")
        ml.addRow("white noise from", self.white_lo)
        ml.addRow("white noise to", self.white_hi)
        lay.addWidget(meth)
        self.adev_box = QGroupBox("Overlapping Allan deviation")
        self.adev_box.setToolTip(
            "Of the field samples (pT files), every start point used. For "
            "white noise of ASD S: sigma(tau) = S / sqrt(2 tau).")
        al = QFormLayout(self.adev_box)
        self.per_decade = QSpinBox()
        self.per_decade.setRange(2, 50)
        self.per_decade.setValue(10)
        al.addRow("points per decade", self.per_decade)
        self.max_tau = self._spin(0, 1e6, 0, " s", 0)
        self.max_tau.setSpecialValueText("record / 4")
        self.max_tau.setToolTip("The longest averaging time (at most a "
                                "third of the record)")
        al.addRow("longest tau", self.max_tau)
        self.adev_detrend = QCheckBox("remove a linear drift first")
        self.adev_detrend.setToolTip(
            "Off: a drift shows as a rise ~ tau at long tau (often what "
            "you want to see). On: the noise without the drift.")
        al.addRow(self.adev_detrend)
        lay.addWidget(self.adev_box)
        self.coh_box = QGroupBox("Coherence (Welch, 50 % overlap)")
        self.coh_box.setToolTip(
            "Magnitude-squared coherence of each pair of ticked channels. "
            "Longer segments: finer frequency bins but fewer averages, so a "
            "higher level for unrelated noise (the dashed line).")
        cl = QFormLayout(self.coh_box)
        self.coh_seg = self._spin(1, 3600, COH_SEGMENT_S, " s", 0)
        self.coh_seg.setToolTip("Segment length: the frequency bin is "
                                "1/segment (100 s -> 0.01 Hz)")
        cl.addRow("segment", self.coh_seg)
        self.coh_same_axis = QCheckBox("same axis only (S1Z with S2Z)")
        self.coh_same_axis.setChecked(True)
        self.coh_same_axis.setToolTip(
            "Off: every pair of ticked channels, including Z with Y of one "
            "sensor")
        cl.addRow(self.coh_same_axis)
        lay.addWidget(self.coh_box)
        self.kind.currentIndexChanged.connect(self._kind_changed)

        self.compute_btn = QPushButton("Compute")
        self.compute_btn.setEnabled(False)
        self.compute_btn.clicked.connect(self.compute)
        lay.addWidget(self.compute_btn)
        self.status = QLabel("")
        self.status.setWordWrap(True)
        lay.addWidget(self.status)

        self.table = QTableWidget(0, 2)       # columns: _kind_changed
        self.table.horizontalHeader().setSectionResizeMode(
            0, QHeaderView.ResizeToContents)
        self.table.verticalHeader().setVisible(False)
        self.table.setEditTriggers(QTableWidget.NoEditTriggers)
        self.table.horizontalHeader().setStretchLastSection(True)
        self.table.setMinimumHeight(118)
        self.levels_label = QLabel("<b>Band levels</b> (fT/√Hz)")
        lay.addWidget(self.levels_label)
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
        self.result_kind = None
        self._kind_changed()

    def _kind_changed(self, *_):
        """Show the chosen calculation's settings; the plot and the
        numbers of the other one go."""
        kind = self.kind.currentText()
        adev, coh = kind == ADEV, kind == COH
        self.asd_box.setVisible(kind == ASD)
        self.adev_box.setVisible(adev)
        self.coh_box.setVisible(coh)
        if self.result is not None and self.result_kind != kind:
            self.plot.clear()
            self.table.setRowCount(0)
            self.result = None
            self.status.setText("")
        self.plot.setLogMode(x=True, y=not coh)
        self.plot.enableAutoRange()      # coherence pins y to 0-1
        if coh:
            self.plot.setLabel("left", "coherence |γ|²")
            self.plot.setLabel("bottom", "frequency (Hz)")
            self.levels_label.setText("<b>Mean coherence</b> per band")
            heads = ["pair"] + [
                f"{lo:g}–{hi:g} Hz" if hi else f"≥{lo:g} Hz"
                for lo, hi in COH_BANDS] + ["95 % level"]
            tips = [None] + ["mean coherence over the band"] * len(COH_BANDS) \
                + ["unrelated noise exceeds this in 5 % of bins"]
        elif adev:
            self.plot.setLabel("left", "Allan deviation (fT)")
            self.plot.setLabel("bottom", "averaging time τ (s)")
            self.levels_label.setText("<b>Allan deviation</b> (fT)")
            heads = ["channel", "σ(1 s)", "minimum", "at τ (s)"]
            tips = [None, "Allan deviation at tau = 1 s, fT",
                    "the lowest Allan deviation: where averaging stops "
                    "helping, fT", "the averaging time of the minimum"]
        else:
            self.plot.setLabel("left", "Amplitude spectral density (fT/√Hz)")
            self.plot.setLabel("bottom", "frequency (Hz)")
            self.levels_label.setText("<b>Band levels</b> (fT/√Hz)")
            heads = ["channel", "white noise"]
            tips = [None, "mean ASD over the white-noise band (10 Hz to "
                    "Nyquist by default), fT/rtHz"]
        self.table.setColumnCount(len(heads))
        self.table.setHorizontalHeaderLabels(heads)
        for c, tip in enumerate(tips):
            if tip:
                self.table.horizontalHeaderItem(c).setToolTip(tip)
        self.actions.restyle()

    @staticmethod
    def _spin(lo, hi, value, suffix, decimals=1):
        w = QDoubleSpinBox()
        w.setRange(lo, hi)
        w.setDecimals(decimals)
        w.setValue(value)
        w.setSuffix(suffix)
        return w

    def _fig_name(self):
        prefix = {ADEV: "adev", COH: "coh"}.get(self.kind.currentText(),
                                                "asd")
        if not self.run_dir:
            return prefix
        run = os.path.normpath(self.run_dir)
        parent = os.path.basename(os.path.dirname(run))
        if parent[:3].isdigit() and parent[3:4] == "_":   # 001_SingleSensorZ
            return f"{prefix}_{parent}"
        return f"{prefix}_{os.path.basename(run)}"

    # -- choosing a run --------------------------------------------------

    def set_last_run(self, run_dir):
        self.last_run = run_dir
        self.last_btn.setEnabled(bool(run_dir))
        self.last_btn.setToolTip(f"The last record: {run_dir}")

    def browse(self):
        # the run already open's folder, else today's dated folder, else
        # the data folder (Box)
        from bfield.io.files import day_dir
        today = day_dir(self.data_root()) if self.data_root() else ""
        start = (os.path.dirname(os.path.normpath(self.run_dir))
                 if self.run_dir else
                 today if today and os.path.isdir(today)
                 else self.data_root() or "")
        d = QFileDialog.getExistingDirectory(self, "Run folder", start)
        if d:
            self.load(d)

    def load(self, run_dir):
        """Read the run's description and offer its channels."""
        from bfield.io.files import read_meta
        from bfield.io.runs import channel_files
        run_dir = (run_dir or "").strip().strip('"')
        inner = record_inside(run_dir)
        if inner:
            run_dir = inner
        files = channel_files(run_dir) if run_dir and os.path.isdir(run_dir) \
            else {}
        from bfield.channels import channel_sort_key
        pt = sorted((c for c, p in files.items() if p.endswith("_pT.txt")),
                    key=channel_sort_key)
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
            text = f"sensor {rec.get('number')} ({rec.get('label')})"
            if rec.get("channels"):            # s1z, s1y: named by number
                names = rec["channels"]
            else:                              # an older run: a/b, z/z2
                names = [ch if rec.get("head") == "a" else f"{ch}2"
                         for ch in ("z", "y")]
            for name in names:
                who[name] = text
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
        kind = self.kind.currentText()
        if kind == ADEV:
            fn = compute_allan
            args = (self.run_dir, chans, self.skip.value(),
                    self.per_decade.value(), self.max_tau.value(),
                    self.adev_detrend.isChecked())
        elif kind == COH:
            fn = compute_coherence
            args = (self.run_dir, chans, self.skip.value(),
                    self.coh_seg.value(), self.coh_same_axis.isChecked())
        else:
            fn = compute_spectra
            args = (self.run_dir, chans, self.lowseg.value(),
                    self.hiseg.value(), self.split.value(), self.skip.value(),
                    self.white_lo.value(), self.white_hi.value())

        def work():
            try:
                res = (kind, fn(*args))
            except Exception as e:
                res = e
            self._done.finished.emit(res)
        threading.Thread(target=work, daemon=True).start()

    def _show(self, res):
        self.compute_btn.setEnabled(True)
        if isinstance(res, Exception):
            self.status.setText(f"failed: {res}")
            return
        kind, res = res
        if kind != self.kind.currentText():
            return                    # switched while it was computing
        if kind == ADEV:
            return self._show_allan(res)
        if kind == COH:
            return self._show_coherence(res)
        spectra, bands, meta, fs = res
        self.result = res
        self.result_kind = kind
        self.plot.clear()
        self._plot_title(meta)
        for ch, (f, a) in spectra.items():
            keep = (f > 0) & np.isfinite(a) & (a > 0)
            label = ch.upper() + (f" {self.who[ch]}" if ch in self.who
                                  else "")
            self.plot.plot(f[keep], a[keep], name=label,
                           pen=channel_colour(ch))
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

    def _plot_title(self, meta):
        folder = os.path.basename(os.path.normpath(self.run_dir))
        # a record inside a dated run folder: name the run (001_...) too
        parent = os.path.basename(os.path.dirname(
            os.path.normpath(self.run_dir)))
        if parent[:3].isdigit() and parent[3:4] == "_":
            folder = f"{parent} / {folder}"
        session = meta.get("session")
        same = session == os.path.basename(os.path.normpath(self.run_dir))
        self.default_title = (folder if not session or same
                              else f"{folder}  (session {session})")
        self._apply_title()

    def _show_allan(self, res):
        curves, bands, meta, fs = res
        self.result = res
        self.result_kind = ADEV
        self.plot.clear()
        self._plot_title(meta)
        for ch, (tau, a, _err) in curves.items():
            keep = np.isfinite(a) & (a > 0)
            label = ch.upper() + (f" {self.who[ch]}" if ch in self.who
                                  else "")
            self.plot.plot(tau[keep], a[keep], name=label,
                           pen=channel_colour(ch), symbol="o",
                           symbolSize=4,
                           symbolBrush=channel_colour(ch),
                           symbolPen=None)
        self.actions.restyle()
        self.table.setRowCount(len(bands))
        for r, (ch, b) in enumerate(bands.items()):
            vals = (ch.upper(), f"{b['sigma_1s']:.2f}", f"{b['min']:.2f}",
                    f"{b['tau_min']:.3g}")
            for c, v in enumerate(vals):
                item = QTableWidgetItem(v)
                if c:
                    item.setTextAlignment(Qt.AlignRight | Qt.AlignVCenter)
                self.table.setItem(r, c, item)
        secs = max((b["seconds"] for b in bands.values()), default=0)
        self.status.setText(
            f"{len(curves)} channel(s), {secs:.0f} s at {fs:g} Hz; "
            "overlapping Allan deviation, "
            f"{self.per_decade.value()} points per decade"
            + ("; linear drift removed" if self.adev_detrend.isChecked()
               else "")
            + (f"; first {self.skip.value():g} s skipped"
               if self.skip.value() else "")
            if bands else "")
        if self.say:
            self.say("[analysis] " + os.path.basename(self.run_dir) + ": "
                     + ";  ".join(f"{ch.upper()} Allan deviation "
                                  f"{b['sigma_1s']:.2f} fT at 1 s, min "
                                  f"{b['min']:.2f} fT at {b['tau_min']:.3g} s"
                                  for ch, b in bands.items()))

    def _show_coherence(self, res):
        import pyqtgraph as pg
        curves, bands, meta, fs = res
        self.result = res
        self.result_kind = COH
        self.plot.clear()
        self._plot_title(meta)
        for i, ((a, b), (f, c)) in enumerate(curves.items()):
            keep = (f > 0) & np.isfinite(c)
            self.plot.plot(f[keep], c[keep], name=f"{a.upper()}–{b.upper()}",
                           pen=PALETTE[i % len(PALETTE)])
        level = max((bd["level95"] for bd in bands.values()), default=0)
        if np.isfinite(level):
            self.plot.addItem(pg.InfiniteLine(
                pos=level, angle=0,
                pen=pg.mkPen("#666", width=1, style=Qt.DashLine)))
        self.plot.setYRange(0, 1.02, padding=0)
        self.actions.restyle()
        self.table.setRowCount(len(bands))
        for r, ((a, b), bd) in enumerate(bands.items()):
            vals = [f"{a.upper()}–{b.upper()}"] + [
                "–" if not np.isfinite(v) else f"{v:.2f}"
                for v in bd["means"].values()] + [f"{bd['level95']:.2f}"]
            for c, v in enumerate(vals):
                item = QTableWidgetItem(v)
                if c:
                    item.setTextAlignment(Qt.AlignRight | Qt.AlignVCenter)
                self.table.setItem(r, c, item)
        first = next(iter(bands.values()), None)
        self.status.setText(
            f"{len(curves)} pair(s), {first['seconds']:.0f} s at {fs:g} Hz; "
            f"{first['seg_s']:g} s segments ({1 / first['seg_s']:.3g} Hz "
            f"bins). Unrelated noise alone averages ~{first['chance']:.2f} "
            f"over a band and stays under the dashed "
            f"{first['level95']:.2f} in 95 % of bins"
            + (f"; first {self.skip.value():g} s skipped"
               if self.skip.value() else "")
            if first else "")
        if self.say:
            self.say("[analysis] " + os.path.basename(self.run_dir) + ": "
                     + ";  ".join(
                         f"{a.upper()}-{b.upper()} coherence "
                         + ", ".join(f"{lo:g}-{hi:g} Hz {v:.2f}"
                                     for (lo, hi), v in bd["means"].items())
                         for (a, b), bd in bands.items()))

    def _apply_title(self, *_):
        text = self.title_edit.text().strip() or getattr(
            self, "default_title", "")
        self.plot.setTitle(text)
        self.actions.restyle()

    def export_data(self):
        """The plotted curves as CSV: frequency and fT/rtHz per channel, or
        tau and the Allan deviation (with its error) in fT."""
        from PySide6.QtWidgets import QMessageBox
        if not self.result:
            QMessageBox.information(self, "Nothing to export",
                                    "Compute first.")
            return
        adev = self.result_kind == ADEV
        coh = self.result_kind == COH
        what = ("Allan deviation" if adev else "coherence" if coh
                else "spectrum")
        start = os.path.join(self.run_dir or self.data_root() or "",
                             f"{self._fig_name()}.csv")
        path, _ = QFileDialog.getSaveFileName(self, f"Export {what} data",
                                              start, "CSV (*.csv)")
        if not path:
            return
        if not path.lower().endswith(".csv"):
            path += ".csv"
        try:
            if adev:
                write_allan_csv(path, self.result, self.run_dir, {
                    "points per decade": self.per_decade.value(),
                    "longest tau (s, 0 = record/4)": self.max_tau.value(),
                    "linear drift removed": self.adev_detrend.isChecked(),
                    "skipped at the start (s)": self.skip.value()})
            elif coh:
                write_coherence_csv(path, self.result, self.run_dir, {
                    "segment (s)": self.coh_seg.value(),
                    "same axis only": self.coh_same_axis.isChecked(),
                    "skipped at the start (s)": self.skip.value()})
            else:
                write_spectra_csv(path, self.result, self.run_dir, {
                    "segment below split (s)": self.lowseg.value(),
                    "segment above split (s)": self.hiseg.value(),
                    "split (Hz)": self.split.value(),
                    "skipped at the start (s)": self.skip.value()})
        except OSError as e:
            QMessageBox.warning(self, "Export failed", f"{path}\n{e}")
            return
        if self.say:
            self.say(f"[analysis] {what} data saved {path}")
