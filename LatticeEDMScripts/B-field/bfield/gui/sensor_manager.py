"""Manage sensors: the table behind config/sensors.toml.

One row per physical QZFM head: number (stays with the head), head ID
(the QuSpin label printed on it), USB ID (the FTDI serial its COM port
reports) and a free location note. The USB ID does not have to be typed:
plug in only the new sensor, press Scan USB, and pick the port that no
sensor owns yet. (The head ID cannot be read over USB -- type it from
the label on the head.)
"""

from __future__ import annotations

from PySide6.QtCore import Qt
from PySide6.QtWidgets import (QAbstractItemView, QDialog, QDialogButtonBox,
                               QHBoxLayout, QHeaderView, QLabel, QMessageBox,
                               QPushButton, QTableWidget, QTableWidgetItem,
                               QVBoxLayout)

from bfield.errors import ConfigError
from bfield.sensors import Sensor, check_sensor, save_sensors

COLUMNS = ("No.", "Head ID (QuSpin label)", "USB ID", "Location / note")


class PortPicker(QDialog):
    """The scan result: every serial port, and which sensor owns it."""

    def __init__(self, parent, ports, owners, simulated):
        super().__init__(parent)
        self.setWindowTitle("USB scan" + (" (simulated lab)" if simulated
                                          else ""))
        self.resize(720, 300)
        lay = QVBoxLayout(self)
        lay.addWidget(QLabel(
            "Serial ports found. A QZFM shows up as an FTDI 'USB Serial "
            "Port'. To find a new sensor's USB ID, plug in only that "
            "sensor and pick the port no sensor owns yet."
            + ("\n(Simulated lab: these are the simulated ports. 'Plug in "
               "a new sensor' on the Simulated lab tab adds one.)"
               if simulated else "")))
        self.table = QTableWidget(len(ports), 4)
        self.table.setHorizontalHeaderLabels(
            ["Port", "USB ID", "Description", "Owned by"])
        self.table.setEditTriggers(QAbstractItemView.NoEditTriggers)
        self.table.setSelectionBehavior(QAbstractItemView.SelectRows)
        self.table.setSelectionMode(QAbstractItemView.SingleSelection)
        self.table.verticalHeader().setVisible(False)
        self.table.horizontalHeader().setSectionResizeMode(
            2, QHeaderView.Stretch)
        self.table.setColumnWidth(3, 230)
        first_free = None
        for r, (port, usb, desc) in enumerate(ports):
            owner = owners.get(usb, "")
            for c, v in enumerate((port, usb, desc, owner or "-- not "
                                   "assigned --")):
                self.table.setItem(r, c, QTableWidgetItem(v))
            if not owner and first_free is None and usb not in ("", "-"):
                first_free = r
        if first_free is not None:
            self.table.selectRow(first_free)
        lay.addWidget(self.table)
        box = QDialogButtonBox()
        self.use = box.addButton("Use this USB ID", QDialogButtonBox.AcceptRole)
        box.addButton(QDialogButtonBox.Cancel)
        box.accepted.connect(self.accept)
        box.rejected.connect(self.reject)
        lay.addWidget(box)
        if not ports:
            self.use.setEnabled(False)

    def chosen(self):
        rows = self.table.selectionModel().selectedRows()
        if not rows:
            return None
        return self.table.item(rows[0].row(), 1).text()


class SensorManager(QDialog):
    def __init__(self, parent, config, sensors, scan):
        """scan() -> ([(port, usb, description)], simulated: bool)."""
        super().__init__(parent)
        self.config = config
        self.scan_fn = scan
        self.saved = False
        self.setWindowTitle("Manage sensors")
        self.resize(820, 380)
        lay = QVBoxLayout(self)
        lay.addWidget(QLabel(
            "One row per QZFM head. The number stays with the head. Type the "
            "head ID from its label; Scan USB finds the USB ID. Saved to "
            "config/sensors.toml."))
        self.table = QTableWidget(0, len(COLUMNS))
        self.table.setHorizontalHeaderLabels(COLUMNS)
        self.table.verticalHeader().setVisible(False)
        self.table.setSelectionBehavior(QAbstractItemView.SelectRows)
        self.table.setSelectionMode(QAbstractItemView.SingleSelection)
        h = self.table.horizontalHeader()
        h.setSectionResizeMode(0, QHeaderView.ResizeToContents)
        h.setSectionResizeMode(3, QHeaderView.Stretch)
        self.table.setColumnWidth(1, 190)
        self.table.setColumnWidth(2, 150)
        for s in sensors:
            self._add_row(s.number, s.label, s.usb_serial, s.location)
        lay.addWidget(self.table)

        row = QHBoxLayout()
        add = QPushButton("Add sensor")
        add.clicked.connect(self.add_sensor)
        remove = QPushButton("Remove selected")
        remove.clicked.connect(self.remove_selected)
        scan = QPushButton("Scan USB...")
        scan.setToolTip("List the serial ports and pick the new sensor's "
                        "USB ID for the selected row")
        scan.clicked.connect(self.scan)
        for b in (add, remove, scan):
            row.addWidget(b)
        row.addStretch(1)
        lay.addLayout(row)
        self.msg = QLabel("")
        self.msg.setWordWrap(True)
        self.msg.setStyleSheet("color:#c62828")
        lay.addWidget(self.msg)

        box = QDialogButtonBox()
        self.save_btn = box.addButton("Save", QDialogButtonBox.AcceptRole)
        box.addButton(QDialogButtonBox.Cancel)
        box.accepted.connect(self.save)
        box.rejected.connect(self.reject)
        lay.addWidget(box)

    # -- rows -------------------------------------------------------------

    def _add_row(self, number, label="", usb="", location=""):
        r = self.table.rowCount()
        self.table.insertRow(r)
        for c, v in enumerate((str(number), label, usb, location)):
            item = QTableWidgetItem(v)
            if c == 0:
                item.setTextAlignment(Qt.AlignCenter)
            self.table.setItem(r, c, item)
        return r

    def add_sensor(self):
        nums = [int(n) for n in (self._cell(r, 0) for r in
                                 range(self.table.rowCount()))
                if n.strip().isdigit()]
        r = self._add_row(max(nums, default=0) + 1)
        self.table.selectRow(r)
        self.table.editItem(self.table.item(r, 1))

    def remove_selected(self):
        rows = self.table.selectionModel().selectedRows()
        if rows:
            self.table.removeRow(rows[0].row())

    def _cell(self, r, c):
        item = self.table.item(r, c)
        return item.text().strip() if item is not None else ""

    def sensors(self):
        """The table as Sensor objects; ConfigError on a bad row."""
        out = []
        for r in range(self.table.rowCount()):
            num = self._cell(r, 0)
            if not num.isdigit():
                raise ConfigError(f"row {r + 1}: the number must be a whole "
                                  f"number (got {num!r})")
            s = Sensor(int(num), self._cell(r, 1), self._cell(r, 2),
                       self._cell(r, 3))
            out.append(s)
        return out

    # -- scan -------------------------------------------------------------

    def scan(self):
        rows = self.table.selectionModel().selectedRows()
        if not rows:
            self.msg.setText("Select the row the USB ID is for (or Add "
                             "sensor first), then Scan USB.")
            return
        r = rows[0].row()
        try:
            ports, simulated = self.scan_fn()
        except Exception as e:
            self.msg.setText(f"USB scan failed: {e}")
            return
        owners = {}
        for i in range(self.table.rowCount()):
            usb = self._cell(i, 2)
            if usb and i != r:
                owners[usb] = (f"sensor {self._cell(i, 0)} "
                               f"({self._cell(i, 1) or 'no head ID'})")
        dlg = PortPicker(self, ports, owners, simulated)
        if dlg.exec() == QDialog.Accepted and dlg.chosen():
            self.table.setItem(r, 2, QTableWidgetItem(dlg.chosen()))
            self.msg.setText("")

    # -- save -------------------------------------------------------------

    def validate(self):
        """(sensors, warnings) or ConfigError."""
        sensors = self.sensors()
        warnings = []
        for i, s in enumerate(sensors):
            warnings += [f"sensor {s.number}: {w}"
                         for w in check_sensor(s, sensors[:i])]
        return sensors, warnings

    def save(self):
        try:
            sensors, warnings = self.validate()
        except ConfigError as e:
            self.msg.setText(str(e))
            return
        if warnings:
            ans = QMessageBox.question(
                self, "Save anyway?",
                "\n".join(warnings) + "\n\nSave these sensors anyway?")
            if ans != QMessageBox.Yes:
                return
        try:
            save_sensors(self.config.sensors_path, sensors)
        except OSError as e:
            self.msg.setText(f"could not write {self.config.sensors_path}: "
                             f"{e}")
            return
        self.saved = True
        self.accept()
