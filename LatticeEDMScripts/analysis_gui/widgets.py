"""Reusable widgets: settings forms, plot pages, results tables, the group
editor, the file selector and the log console."""

import csv
import os
import sys
import traceback

import numpy as np
from qtpy import QtCore, QtGui, QtWidgets
from matplotlib.figure import Figure
from matplotlib.backends.backend_qtagg import FigureCanvasQTAgg, NavigationToolbar2QT

from analysis_gui import data


# ---------------------------------------------------------------- log console

class _Stream(QtCore.QObject):
    """File-like object that sends what is written to it to a Qt signal, so
    prints from any thread end up in the log console. Also writes to the
    original stream when there is one (there isn't under pythonw)."""
    text_written = QtCore.Signal(str)

    def __init__(self, original):
        super().__init__()
        self.original = original

    def write(self, text):
        if self.original is not None:
            try:
                self.original.write(text)
            except Exception:
                pass
        if text:
            self.text_written.emit(text)

    def flush(self):
        if self.original is not None:
            try:
                self.original.flush()
            except Exception:
                pass


class LogConsole(QtWidgets.QPlainTextEdit):
    """Shows everything printed (the library prints a lot of useful detail)"""

    def __init__(self, parent=None):
        super().__init__(parent)
        self.setReadOnly(True)
        self.setMaximumBlockCount(20000)
        self.setFont(QtGui.QFontDatabase.systemFont(QtGui.QFontDatabase.FixedFont))
        self._streams = []

    def capture_output(self):
        for name in ("stdout", "stderr"):
            stream = _Stream(getattr(sys, name))
            stream.text_written.connect(self._append)
            setattr(sys, name, stream)
            self._streams.append(stream)

    def _append(self, text):
        cursor = self.textCursor()
        cursor.movePosition(QtGui.QTextCursor.End)
        cursor.insertText(text)
        self.setTextCursor(cursor)
        self.ensureCursorVisible()


# ------------------------------------------------------------- settings form

class SettingsForm(QtWidgets.QWidget):
    """A form built from a spec, a list of
        ("--", "Section title")
        (key, label, kind, default)          or with a tooltip as 5th item
    kinds: "float", "optfloat" (blank = None), "floats" (comma list, blank =
    None), "int", "bool", "text", or a list of choices. values() parses and
    checks everything, raising ValueError naming the bad field."""
    changed = QtCore.Signal()

    def __init__(self, spec, parent=None):
        super().__init__(parent)
        self.spec = [s for s in spec if s[0] != "--"]
        self.widgets = {}
        layout = QtWidgets.QFormLayout(self)
        layout.setContentsMargins(0, 0, 0, 0)
        layout.setFieldGrowthPolicy(QtWidgets.QFormLayout.AllNonFixedFieldsGrow)
        for item in spec:
            if item[0] == "--":
                title = QtWidgets.QLabel("<b>%s</b>" % item[1])
                layout.addRow(title)
                continue
            key, label, kind, default = item[:4]
            tip = item[4] if len(item) > 4 else ""
            w = self._make(kind, default)
            if tip:
                w.setToolTip(tip)
            self.widgets[key] = (w, kind, label)
            layout.addRow(label, w)

    def _make(self, kind, default):
        if kind == "bool":
            w = QtWidgets.QCheckBox()
            w.setChecked(bool(default))
            w.toggled.connect(self.changed)
        elif kind == "int":
            w = QtWidgets.QSpinBox()
            w.setRange(-1000000, 1000000)
            w.setValue(int(default))
            w.valueChanged.connect(self.changed)
        elif isinstance(kind, (list, tuple)):
            w = QtWidgets.QComboBox()
            w.addItems(list(kind))
            w.setCurrentText(str(default))
            w.currentTextChanged.connect(self.changed)
        else:
            w = QtWidgets.QLineEdit("" if default is None else str(default))
            w.editingFinished.connect(self.changed)
        return w

    def raw(self, key):
        """Text/bool value of a field as stored in the settings file"""
        w, kind, _ = self.widgets[key]
        if kind == "bool":
            return w.isChecked()
        if kind == "int":
            return w.value()
        if isinstance(kind, (list, tuple)):
            return w.currentText()
        return w.text()

    def set_raw(self, key, value):
        if key not in self.widgets or value is None:
            return
        w, kind, _ = self.widgets[key]
        if kind == "bool":
            w.setChecked(str(value).lower() in ("true", "1"))
        elif kind == "int":
            try:
                w.setValue(int(value))
            except (TypeError, ValueError):
                pass
        elif isinstance(kind, (list, tuple)):
            if str(value) in kind:
                w.setCurrentText(str(value))
        else:
            w.setText(str(value))

    def values(self):
        out = {}
        for key, (w, kind, label) in self.widgets.items():
            raw = self.raw(key)
            try:
                if kind == "float":
                    out[key] = float(raw)
                elif kind == "optfloat":
                    out[key] = float(raw) if raw.strip() else None
                elif kind == "floats":
                    parts = [p for p in raw.replace(";", ",").split(",") if p.strip()]
                    out[key] = [float(p) for p in parts] if parts else None
                else:
                    out[key] = raw
            except ValueError:
                raise ValueError("%s: '%s' is not a number" % (label.rstrip(":"), raw))
        return out

    def save(self, qsettings, group):
        qsettings.beginGroup(group)
        for key in self.widgets:
            qsettings.setValue(key, self.raw(key))
        qsettings.endGroup()

    def load(self, qsettings, group):
        qsettings.beginGroup(group)
        for key in self.widgets:
            if qsettings.contains(key):
                self.set_raw(key, qsettings.value(key))
        qsettings.endGroup()


# ---------------------------------------------------------------- group editor

class GroupEditor(QtWidgets.QWidget):
    """Two-column table of named groups of file IDs (the "types" dictionaries
    of the scripts), with add/remove buttons."""
    changed = QtCore.Signal()

    def __init__(self, headers=("Group", "File IDs"), tip="", parent=None):
        super().__init__(parent)
        layout = QtWidgets.QVBoxLayout(self)
        layout.setContentsMargins(0, 0, 0, 0)
        self.table = QtWidgets.QTableWidget(0, 2)
        self.table.setHorizontalHeaderLabels(list(headers))
        self.table.horizontalHeader().setStretchLastSection(True)
        self.table.verticalHeader().setVisible(False)
        self.table.setMinimumHeight(110)
        self.table.setToolTip(tip)
        self.table.itemChanged.connect(lambda _: self.changed.emit())
        layout.addWidget(self.table)
        buttons = QtWidgets.QHBoxLayout()
        add = QtWidgets.QPushButton("Add group")
        add.clicked.connect(lambda: self.add_row("", ""))
        remove = QtWidgets.QPushButton("Remove")
        remove.clicked.connect(self.remove_selected)
        buttons.addWidget(add)
        buttons.addWidget(remove)
        buttons.addStretch()
        layout.addLayout(buttons)

    def add_row(self, name, ids):
        r = self.table.rowCount()
        self.table.insertRow(r)
        self.table.setItem(r, 0, QtWidgets.QTableWidgetItem(name))
        self.table.setItem(r, 1, QtWidgets.QTableWidgetItem(ids))

    def remove_selected(self):
        rows = sorted({i.row() for i in self.table.selectedIndexes()}, reverse=True)
        for r in rows:
            self.table.removeRow(r)
        self.changed.emit()

    def groups(self):
        """[(name, ids text)] of the rows with something in them"""
        out = []
        for r in range(self.table.rowCount()):
            name = self.table.item(r, 0).text().strip() if self.table.item(r, 0) else ""
            ids = self.table.item(r, 1).text().strip() if self.table.item(r, 1) else ""
            if ids:
                out.append((name or ids, ids))
        return out

    def set_groups(self, groups):
        self.table.setRowCount(0)
        for name, ids in groups:
            self.add_row(name, ids)

    def save(self, qsettings, key):
        qsettings.setValue(key, [[n, i] for n, i in self.groups()])

    def load(self, qsettings, key):
        value = qsettings.value(key)
        if value:
            try:
                self.set_groups([(str(v[0]), str(v[1])) for v in value])
            except (TypeError, IndexError):
                pass


# --------------------------------------------------------------- file selector

class DataSelector(QtWidgets.QGroupBox):
    """Keyword + file IDs + list of matching files, and the Load button.
    Checking files in the list fills in the IDs and the other way round;
    no IDs means all matching files, as sele = [] in the scripts."""
    load_requested = QtCore.Signal(list, bool)

    def __init__(self, folder_getter, keyword, parent=None):
        super().__init__("Data", parent)
        self.folder_getter = folder_getter
        self._syncing = False
        layout = QtWidgets.QGridLayout(self)

        self.keyword = QtWidgets.QLineEdit(keyword)
        self.keyword.setToolTip("Files matching *keyword*.zip in the folder. Wildcards (*, ?) are allowed.")
        self.keyword.editingFinished.connect(self.refresh)
        self.ids = QtWidgets.QLineEdit()
        self.ids.setPlaceholderText("empty = all matching files")
        self.ids.setToolTip("File IDs to load, e.g. 001, 002, 005 (the sele list). Empty loads every matching file.")
        self.ids.textEdited.connect(self._ids_edited)
        self.passes = QtWidgets.QCheckBox("Load each pass")
        self.passes.setChecked(True)
        self.passes.setToolTip("Load every pass of a file as its own scan (001_0, 001_1, ...), "
                               "as LoadPasses = True. Off: only the average of the passes.")
        layout.addWidget(QtWidgets.QLabel("Keyword"), 0, 0)
        layout.addWidget(self.keyword, 0, 1)
        layout.addWidget(QtWidgets.QLabel("File IDs"), 1, 0)
        layout.addWidget(self.ids, 1, 1)
        layout.addWidget(self.passes, 2, 0, 1, 2)

        self.list = QtWidgets.QListWidget()
        self.list.setMinimumHeight(90)
        self.list.itemChanged.connect(self._item_changed)
        layout.addWidget(self.list, 3, 0, 1, 2)

        buttons = QtWidgets.QHBoxLayout()
        self.find_button = QtWidgets.QPushButton("Find files")
        self.find_button.clicked.connect(self.refresh)
        self.load_button = QtWidgets.QPushButton("Load")
        self.load_button.setToolTip("Load the selected files (all matching files if none are selected) and run the analysis")
        self.load_button.clicked.connect(self._load)
        self.cancel_button = QtWidgets.QPushButton("Cancel")
        self.cancel_button.setEnabled(False)
        buttons.addWidget(self.find_button)
        buttons.addWidget(self.load_button)
        buttons.addWidget(self.cancel_button)
        layout.addLayout(buttons, 4, 0, 1, 2)
        self.progress = QtWidgets.QProgressBar()
        self.progress.setVisible(False)
        layout.addWidget(self.progress, 5, 0, 1, 2)
        self.status = QtWidgets.QLabel("")
        self.status.setWordWrap(True)
        layout.addWidget(self.status, 6, 0, 1, 2)
        self.files = []

    def refresh(self):
        folder = self.folder_getter()
        self.files = data.find_files(folder, self.keyword.text())
        self._syncing = True
        self.list.clear()
        for fid, path in self.files:
            item = QtWidgets.QListWidgetItem(os.path.basename(path))
            item.setFlags(item.flags() | QtCore.Qt.ItemIsUserCheckable)
            item.setCheckState(QtCore.Qt.Unchecked)
            item.setData(QtCore.Qt.UserRole, fid)
            item.setToolTip(path)
            self.list.addItem(item)
        self._syncing = False
        self._ids_edited()
        if not folder or not os.path.isdir(folder):
            self.status.setText("Choose a data folder at the top")
        else:
            self.status.setText("%d matching file%s" % (len(self.files), "" if len(self.files) == 1 else "s"))

    def _ids_edited(self, *_):
        if self._syncing:
            return
        wanted = set(data.parse_ids(self.ids.text()))
        self._syncing = True
        for i in range(self.list.count()):
            item = self.list.item(i)
            item.setCheckState(QtCore.Qt.Checked if item.data(QtCore.Qt.UserRole) in wanted
                               else QtCore.Qt.Unchecked)
        self._syncing = False

    def _item_changed(self, _):
        if self._syncing:
            return
        ids = []
        for i in range(self.list.count()):
            item = self.list.item(i)
            fid = item.data(QtCore.Qt.UserRole)
            if item.checkState() == QtCore.Qt.Checked and fid not in ids:
                ids.append(fid)
        # Keep IDs typed for files that aren't in the list (e.g. another keyword)
        listed = {f for f, _ in self.files}
        extra = [i for i in data.parse_ids(self.ids.text()) if i not in listed]
        self._syncing = True
        self.ids.setText(", ".join(ids + extra))
        self._syncing = False

    def selected_files(self):
        """[(ID, path)] to load, and IDs asked for that have no file"""
        wanted = data.parse_ids(self.ids.text())
        if not wanted:
            return list(self.files), []
        chosen = [(fid, p) for fid, p in self.files if fid in wanted]
        missing = [i for i in wanted if i not in {f for f, _ in chosen}]
        return chosen, missing

    def _load(self):
        if not self.files:
            self.refresh()
        files, missing = self.selected_files()
        if missing:
            print("No matching file for ID(s): " + ", ".join(missing))
        if not files:
            self.status.setText("No files to load")
            return
        self.load_requested.emit(files, self.passes.isChecked())

    def set_busy(self, busy):
        self.load_button.setEnabled(not busy)
        self.find_button.setEnabled(not busy)
        self.cancel_button.setEnabled(busy)
        self.progress.setVisible(busy)

    def save(self, qsettings, group):
        qsettings.setValue(group + "/keyword", self.keyword.text())
        qsettings.setValue(group + "/ids", self.ids.text())
        qsettings.setValue(group + "/passes", self.passes.isChecked())

    def load(self, qsettings, group):
        if qsettings.contains(group + "/keyword"):
            self.keyword.setText(str(qsettings.value(group + "/keyword")))
        if qsettings.contains(group + "/ids"):
            self.ids.setText(str(qsettings.value(group + "/ids")))
        if qsettings.contains(group + "/passes"):
            self.passes.setChecked(str(qsettings.value(group + "/passes")).lower() in ("true", "1"))


# ------------------------------------------------------------------ plot page

class PlotPage(QtWidgets.QWidget):
    """One matplotlib figure with its toolbar and optional selectors above it.
    render(page, figure) draws the figure; it is called when the page is shown
    after the results changed, or when one of its selectors changes."""

    def __init__(self, title, render, parent=None):
        super().__init__(parent)
        self.title = title
        self.render_func = render
        self.dirty = True
        self.last_error = None
        self.selectors = {}
        layout = QtWidgets.QVBoxLayout(self)
        layout.setContentsMargins(2, 2, 2, 2)
        self.header = QtWidgets.QHBoxLayout()
        self.header.addStretch()
        layout.addLayout(self.header)
        self.figure = Figure(constrained_layout=True)
        self.canvas = FigureCanvasQTAgg(self.figure)
        self.toolbar = NavigationToolbar2QT(self.canvas, self)
        layout.addWidget(self.toolbar)
        layout.addWidget(self.canvas, 1)

    def _add_header(self, label, widget):
        # before the stretch, which stays last
        if label:
            self.header.insertWidget(self.header.count() - 1, QtWidgets.QLabel(label))
        self.header.insertWidget(self.header.count() - 1, widget)

    def add_combo(self, name, label):
        combo = QtWidgets.QComboBox()
        combo.setMinimumContentsLength(14)
        combo.setSizeAdjustPolicy(QtWidgets.QComboBox.AdjustToContents)
        combo.currentTextChanged.connect(lambda _: self.refresh())
        self.selectors[name] = combo
        self._add_header(label, combo)
        return combo

    def add_spin(self, name, label, minimum, maximum, value, special=None, tip=""):
        spin = QtWidgets.QSpinBox()
        spin.setRange(minimum, maximum)
        spin.setValue(value)
        if special:
            spin.setSpecialValueText(special)
        spin.setToolTip(tip)
        spin.valueChanged.connect(lambda _: self.refresh())
        self.selectors[name] = spin
        self._add_header(label, spin)
        return spin

    def add_check(self, name, label, checked=False, tip=""):
        box = QtWidgets.QCheckBox(label)
        box.setChecked(checked)
        box.setToolTip(tip)
        box.toggled.connect(lambda _: self.refresh())
        self.selectors[name] = box
        self._add_header("", box)
        return box

    def set_items(self, name, items):
        """Fill a combo, keeping the current choice if it is still there"""
        combo = self.selectors[name]
        current = combo.currentText()
        combo.blockSignals(True)
        combo.clear()
        combo.addItems(list(items))
        if current in items:
            combo.setCurrentText(current)
        combo.blockSignals(False)

    def value(self, name):
        w = self.selectors[name]
        if isinstance(w, QtWidgets.QComboBox):
            return w.currentText()
        if isinstance(w, QtWidgets.QCheckBox):
            return w.isChecked()
        return w.value()

    def refresh(self):
        self.figure.clear()
        self.last_error = None
        try:
            self.render_func(self, self.figure)
        except Exception as e:
            self.last_error = traceback.format_exc()
            print("Plot '%s' failed:\n%s" % (self.title, self.last_error))
            self.figure.clear()
            show_message(self.figure, "Could not draw this plot:\n%s" % e)
        self.canvas.draw_idle()
        self.dirty = False


def show_message(figure, text):
    """Draw text in the middle of an empty figure"""
    figure.clear()
    figure.text(0.5, 0.5, text, ha="center", va="center", wrap=True, fontsize=11, color="0.3")


# --------------------------------------------------------------- results table

def _fmt(value):
    if value is None:
        return ""
    if isinstance(value, (bool, np.bool_)):
        return "yes" if value else "no"
    if isinstance(value, (float, np.floating)):
        if not np.isfinite(value):
            return "nan"
        return "%.6g" % value
    return str(value)


class _Item(QtWidgets.QTableWidgetItem):
    """Table cell that sorts numbers by value, not as text"""

    def __lt__(self, other):
        a = self.data(QtCore.Qt.UserRole)
        b = other.data(QtCore.Qt.UserRole)
        if a is not None and b is not None:
            return a < b
        return super().__lt__(other)


class ResultsTable(QtWidgets.QWidget):
    """Table of result rows (list of dicts) with copy and CSV export"""

    def __init__(self, title, parent=None):
        super().__init__(parent)
        self.title = title
        self.rows = []
        self.columns = []
        layout = QtWidgets.QVBoxLayout(self)
        layout.setContentsMargins(2, 2, 2, 2)
        self.table = QtWidgets.QTableWidget()
        self.table.setEditTriggers(QtWidgets.QAbstractItemView.NoEditTriggers)
        self.table.setSelectionMode(QtWidgets.QAbstractItemView.ExtendedSelection)
        self.table.setSortingEnabled(True)
        layout.addWidget(self.table)
        buttons = QtWidgets.QHBoxLayout()
        copy = QtWidgets.QPushButton("Copy")
        copy.setToolTip("Copy the selected cells (or the whole table) to paste into Excel/Origin")
        copy.clicked.connect(self.copy)
        export = QtWidgets.QPushButton("Export CSV...")
        export.clicked.connect(self.export)
        buttons.addWidget(copy)
        buttons.addWidget(export)
        buttons.addStretch()
        layout.addLayout(buttons)
        QtWidgets.QShortcut(QtGui.QKeySequence.Copy, self.table, self.copy)
        self.export_dir = None

    def set_rows(self, rows):
        self.rows = rows
        columns = []
        for row in rows:
            for key in row:
                if key not in columns:
                    columns.append(key)
        self.table.setSortingEnabled(False)
        self.table.clear()
        self.table.setColumnCount(len(columns))
        self.table.setRowCount(len(rows))
        self.table.setHorizontalHeaderLabels(columns)
        for r, row in enumerate(rows):
            for c, key in enumerate(columns):
                value = row.get(key)
                item = _Item(_fmt(value))
                if isinstance(value, (int, float, np.integer, np.floating)) and not isinstance(value, bool):
                    item.setData(QtCore.Qt.UserRole, float(value))
                self.table.setItem(r, c, item)
        self.table.resizeColumnsToContents()
        self.table.setSortingEnabled(True)
        self.columns = columns

    def _cells(self, selected_only):
        rows = range(self.table.rowCount())
        cols = range(self.table.columnCount())
        if selected_only:
            idx = self.table.selectedIndexes()
            if idx:
                rows = sorted({i.row() for i in idx})
                cols = sorted({i.column() for i in idx})
        header = [self.table.horizontalHeaderItem(c).text() for c in cols]
        body = [[self.table.item(r, c).text() if self.table.item(r, c) else "" for c in cols] for r in rows]
        return header, body

    def copy(self):
        header, body = self._cells(True)
        text = "\n".join("\t".join(line) for line in [header] + body)
        QtWidgets.QApplication.clipboard().setText(text)

    def export(self):
        if not self.rows:
            return
        start = os.path.join(self.export_dir or "", self.title.replace(" ", "_") + ".csv")
        path, _ = QtWidgets.QFileDialog.getSaveFileName(self, "Export " + self.title, start,
                                                        "CSV files (*.csv);;Text files (*.txt)")
        if not path:
            return
        sep = "," if path.lower().endswith(".csv") else "\t"
        with open(path, "w", newline="") as f:
            writer = csv.writer(f, delimiter=sep)
            writer.writerow(self.columns)
            for row in self.rows:
                writer.writerow([_csv_value(row.get(k)) for k in self.columns])
        print("Saved %d rows to %s" % (len(self.rows), path))


def _csv_value(value):
    if value is None:
        return ""
    if isinstance(value, (float, np.floating)):
        return repr(float(value))
    return str(value)
