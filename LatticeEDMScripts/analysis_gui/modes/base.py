"""Common layout and behaviour of the analysis tabs.

A tab has, on the left, the data selector, a settings form (and optionally a
group editor) and a Run button; on the right, pages of plots above tables of
results. Subclasses give the settings spec, build their pages and tables in
build(), and do the work in analyse(settings), which returns a results object
that the page render functions and the tables read."""

import traceback

import numpy as np
from matplotlib import rcParams
from qtpy import QtCore, QtWidgets

import LatticeEDM_analysis_library as EDM
from analysis_gui import data
from analysis_gui.widgets import (DataSelector, GroupEditor, PlotPage, ResultsTable,
                                  SettingsForm, show_message)


def colors():
    return rcParams['axes.prop_cycle'].by_key()['color']


def color(i):
    c = colors()
    return c[i % len(c)]


def gate_lines(ax, s, keys=("SigStart", "SigEnd", "BkgStart", "BkgEnd")):
    """Dashed lines at the gates in settings s (signal black, background grey)"""
    for k in keys:
        if k in s and s[k] is not None:
            ax.axvline(s[k], color="black" if k.startswith("Sig") else "0.5",
                       linestyle="--", linewidth=1)


class ModeTab(QtWidgets.QWidget):
    NAME = "Analysis"
    KEY = "analysis"           # settings group name
    KEYWORD = ""               # default file keyword
    SETTINGS = []              # SettingsForm spec
    GROUPS = None              # (headers, tooltip) for a group editor, or None
    DESCRIPTION = ""

    def __init__(self, folder_getter, qsettings, parent=None):
        super().__init__(parent)
        self.folder_getter = folder_getter
        self.qsettings = qsettings
        self.scans = {}
        self.results = None
        self.last_settings = None
        self.pages = []
        self.tables = {}
        self.worker = None

        # ---- left: data, settings, run
        left = QtWidgets.QWidget()
        left_layout = QtWidgets.QVBoxLayout(left)
        if self.DESCRIPTION:
            desc = QtWidgets.QLabel(self.DESCRIPTION)
            desc.setWordWrap(True)
            left_layout.addWidget(desc)
        self.selector = DataSelector(folder_getter, self.KEYWORD)
        self.selector.load_requested.connect(self.load)
        self.selector.cancel_button.clicked.connect(self.cancel_load)
        left_layout.addWidget(self.selector)

        box = QtWidgets.QGroupBox("Settings")
        box_layout = QtWidgets.QVBoxLayout(box)
        self.form = SettingsForm(self.SETTINGS)
        box_layout.addWidget(self.form)
        if any(k in self.form.widgets for k in ("SigStart", "BkgStart")):
            gates = QtWidgets.QPushButton("Gates from file")
            gates.setToolTip("Fill in the gates saved by ScanMaster in the first loaded scan")
            gates.clicked.connect(self.gates_from_file)
            box_layout.addWidget(gates)
        left_layout.addWidget(box)

        self.groups = None
        if self.GROUPS is not None:
            headers, tip = self.GROUPS
            gbox = QtWidgets.QGroupBox("Groups")
            gl = QtWidgets.QVBoxLayout(gbox)
            self.groups = GroupEditor(headers, tip)
            gl.addWidget(self.groups)
            hint = QtWidgets.QLabel(tip)
            hint.setWordWrap(True)
            hint.setStyleSheet("color: gray")
            gl.addWidget(hint)
            left_layout.addWidget(gbox)

        self.run_button = QtWidgets.QPushButton("Run analysis  (F5)")
        self.run_button.setStyleSheet("font-weight: bold; padding: 6px")
        self.run_button.clicked.connect(self.run)
        left_layout.addWidget(self.run_button)
        left_layout.addStretch()

        scroll = QtWidgets.QScrollArea()
        scroll.setWidget(left)
        scroll.setWidgetResizable(True)
        scroll.setMinimumWidth(340)
        scroll.setHorizontalScrollBarPolicy(QtCore.Qt.ScrollBarAlwaysOff)

        # ---- right: plots above results
        self.plot_tabs = QtWidgets.QTabWidget()
        self.plot_tabs.currentChanged.connect(self._page_shown)
        self.table_tabs = QtWidgets.QTabWidget()
        right = QtWidgets.QSplitter(QtCore.Qt.Vertical)
        right.addWidget(self.plot_tabs)
        right.addWidget(self.table_tabs)
        right.setStretchFactor(0, 3)
        right.setStretchFactor(1, 1)

        splitter = QtWidgets.QSplitter(QtCore.Qt.Horizontal)
        splitter.addWidget(scroll)
        splitter.addWidget(right)
        splitter.setStretchFactor(1, 1)
        layout = QtWidgets.QHBoxLayout(self)
        layout.setContentsMargins(2, 2, 2, 2)
        layout.addWidget(splitter)

        QtWidgets.QShortcut(QtCore.Qt.Key_F5, self, self.run)

        self.build()
        self.restore()
        for page in self.pages:
            if not self.scans:
                show_message(page.figure, "Load data to start")
                page.canvas.draw_idle()

    # ---------------------------------------------------------- building
    def build(self):
        """Subclasses add their pages and tables here"""

    def add_page(self, title, render):
        page = PlotPage(title, render)
        self.pages.append(page)
        self.plot_tabs.addTab(page, title)
        return page

    def add_table(self, key, title):
        table = ResultsTable(title)
        self.tables[key] = table
        self.table_tabs.addTab(table, title)
        return table

    def add_tof_page(self):
        """Page showing the TOFs of one scan point (or all) of a scan, with the gates"""
        page = self.add_page("TOF", self.render_tof)
        page.add_combo("scan", "Scan")
        page.add_spin("point", "Point", -1, 100000, -1, special="all",
                      tip="Scan point to show, averaged over its shots ('all' = every point)")
        return page

    # ---------------------------------------------------------- settings
    def settings(self):
        s = self.form.values()
        if self.groups is not None:
            s["groups"] = self.groups.groups()
        return s

    def save(self):
        self.form.save(self.qsettings, self.KEY)
        self.selector.save(self.qsettings, self.KEY + "_data")
        if self.groups is not None:
            self.groups.save(self.qsettings, self.KEY + "_groups/list")

    def restore(self):
        self.form.load(self.qsettings, self.KEY)
        self.selector.load(self.qsettings, self.KEY + "_data")
        if self.groups is not None:
            self.groups.load(self.qsettings, self.KEY + "_groups/list")

    def gates_from_file(self):
        if not self.scans:
            QtWidgets.QMessageBox.information(self, "Gates", "Load a file first.")
            return
        first = next(iter(self.scans.values()))["settings"]
        found = []
        for key, skey in [("SigStart", "gate SigStart"), ("SigEnd", "gate SigEnd"),
                          ("BkgStart", "gate BkgStart"), ("BkgEnd", "gate BkgEnd")]:
            if key in self.form.widgets and first.get(skey) is not None:
                self.form.set_raw(key, first[skey])
                found.append(key)
        if not found:
            QtWidgets.QMessageBox.information(self, "Gates", "No gates saved in this file.")

    # ---------------------------------------------------------- loading
    def load(self, files, passes):
        if self.worker is not None:
            return
        self.save()
        self.selector.set_busy(True)
        self.selector.progress.setRange(0, len(files))
        self.selector.progress.setValue(0)
        self.worker = data.LoadWorker(files, passes, self)
        self.worker.progress.connect(self._progress)
        self.worker.loaded.connect(self._loaded)
        self.worker.finished.connect(self._worker_done)
        print("\n=== %s: loading %d file(s) from %s" % (self.NAME, len(files), self.folder_getter()))
        self.worker.start()

    def cancel_load(self):
        if self.worker is not None:
            self.worker.cancel()

    def _progress(self, i, n, name):
        self.selector.progress.setValue(i)
        self.selector.status.setText("Loading %s (%d of %d)..." % (name, i + 1, n) if name else "")

    def _worker_done(self):
        self.worker = None
        self.selector.set_busy(False)

    def _loaded(self, scans, problems):
        self.scans = scans
        n_files = len({data.label_id(l) for l in scans})
        text = "%d scan%s from %d file%s loaded" % (len(scans), "" if len(scans) == 1 else "s",
                                                    n_files, "" if n_files == 1 else "s")
        if problems:
            text += "\n" + "\n".join(problems)
            for p in problems:
                print("WARNING: " + p)
        self.selector.status.setText(text)
        for label, arrays in scans.items():
            note = self.check_scan(arrays)
            if note:
                print("WARNING: %s %s" % (label, note))
        if scans:
            self.run()

    def check_scan(self, arrays):
        """A note if a scan doesn't look like this tab's kind of data"""
        return ""

    # ---------------------------------------------------------- analysis
    def run(self):
        if not self.scans:
            QtWidgets.QMessageBox.information(self, self.NAME, "Load some data first.")
            return
        try:
            s = self.settings()
        except ValueError as e:
            QtWidgets.QMessageBox.warning(self, "Settings", str(e))
            return
        self.save()
        print("\n=== %s analysis of %d scan(s)" % (self.NAME, len(self.scans)))
        QtWidgets.QApplication.setOverrideCursor(QtCore.Qt.WaitCursor)
        try:
            self.results = self.analyse(s)
            self.last_settings = s
        except ValueError as e:
            QtWidgets.QApplication.restoreOverrideCursor()
            print("Analysis stopped: %s" % e)
            QtWidgets.QMessageBox.warning(self, self.NAME, str(e))
            return
        except Exception as e:
            QtWidgets.QApplication.restoreOverrideCursor()
            print(traceback.format_exc())
            QtWidgets.QMessageBox.critical(self, self.NAME,
                                           "The analysis failed:\n%s\n\nSee the log for details." % e)
            return
        QtWidgets.QApplication.restoreOverrideCursor()
        for table in self.tables.values():
            table.export_dir = self.folder_getter()
        self.fill_tables()
        self.update_pages()

    def analyse(self, s):
        raise NotImplementedError

    def fill_tables(self):
        """Subclasses fill self.tables from self.results"""

    def selector_items(self, name):
        """Items of the page combos called name"""
        if name == "scan":
            return list(self.scans.keys())
        return []

    def update_pages(self):
        for page in self.pages:
            for name, widget in page.selectors.items():
                if isinstance(widget, QtWidgets.QComboBox):
                    page.set_items(name, self.selector_items(name))
            page.dirty = True
        current = self.plot_tabs.currentWidget()
        if current is not None:
            current.refresh()

    def _page_shown(self, _):
        page = self.plot_tabs.currentWidget()
        if page is not None and page.dirty and self.results is not None:
            page.refresh()

    # ---------------------------------------------------------- shared pages
    def tof_traces(self, arrays, point, s):
        """[(label, time in ms, TOF)] for the TOF page: On and Off shots of the
        detector in the settings, averaged over shots (and points if point < 0)"""
        detector = int(s.get("detector", 0))
        traces = []
        for shot, label in [("on", "On"), ("off", "Off")]:
            if arrays[shot] is None:
                continue
            tofs = EDM.DetectorTOFs(arrays, shot, detector)
            traces.append((label, arrays["time " + shot]*1000, self._point_mean(tofs, point)))
        return traces

    @staticmethod
    def _point_mean(tofs, point):
        """Mean over shots of one point of [point, time, shot], or over all"""
        if point >= tofs.shape[0]:
            raise ValueError("This scan has %d points (0 to %d)" % (tofs.shape[0], tofs.shape[0] - 1))
        if point < 0:
            return np.mean(tofs, axis=(0, 2))
        return np.mean(tofs[point], axis=1)

    def render_tof(self, page, fig):
        label = page.value("scan")
        if label not in self.scans or self.last_settings is None:
            show_message(fig, "No scan")
            return
        s = self.last_settings
        point = page.value("point")
        ax = fig.add_subplot()
        for i, (name, t, trace) in enumerate(self.tof_traces(self.scans[label], point, s)):
            ax.plot(t, trace, color=color(i), label=name)
        gate_lines(ax, s)
        settings = self.scans[label]["settings"]
        n = settings["shotsPerPoint"]
        ax.set_title("%s, %s, averaged over %s" % (
            label, "all points" if point < 0 else "point %d" % point,
            "all shots" if point < 0 else "%s shots" % n))
        ax.set_xlabel("time (ms)")
        ax.set_ylabel("Signal (V)")
        ax.legend()
