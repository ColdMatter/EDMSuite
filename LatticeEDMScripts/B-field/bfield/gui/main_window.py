"""The B-field main window.

Left: one tab per procedure, Start / Abort. Right: the operations log,
a live plot of what the recorder is recording, and the simulated lab
(its state, fault injection, the simulated bench).

SIMULATION ONLY for now: the bench option is shown but disabled
(GUI_ALLOW_HARDWARE). Nothing the window can start touches hardware.
"""

from __future__ import annotations

import collections
import os
import threading
import traceback

from PySide6.QtCore import Qt, QTimer
from PySide6.QtGui import QFont, QTextCursor
from PySide6.QtWidgets import (QCheckBox, QComboBox, QDialog,
                               QDialogButtonBox, QDoubleSpinBox, QFormLayout,
                               QGroupBox, QHBoxLayout, QLabel, QLineEdit,
                               QMainWindow, QMessageBox, QPlainTextEdit,
                               QProgressBar, QPushButton, QRadioButton,
                               QScrollArea, QSizePolicy,
                               QSplitter, QTableWidget, QTableWidgetItem,
                               QTabWidget, QVBoxLayout, QWidget)

from bfield.config import load_config
from bfield.devices.simulated import World
from bfield.errors import BFieldError
from bfield.gui.forms import FORMS
from bfield.gui.qt_ui import Bridge, QtUI
from bfield.gui.analysis_panel import AnalysisPanel
from bfield.gui.plot_export import PlotActions
from bfield.gui.sensor_panel import SensorPanel
from bfield.gui.sim_operator import SimOperator
from bfield.lab import SIM_DATA_DIR, open_lab
from bfield.sensors import load_sensors
from bfield.ui import Context

# The bench is off-limits from the GUI until it has been tried out in
# the simulator. Flip only on purpose, with someone at the bench.
GUI_ALLOW_HARDWARE = False

EXIT_WORDS = {
    0: "completed", 1: "crashed / unexpected error", 10: "device not found",
    11: "DAQ setup", 12: "DAQ read", 13: "refused: configuration",
    20: "stopped early / aborted", 30: "flagged: saturation or QC refused",
    31: "flagged: no signal", 32: "bad data / readback refused",
    40: "output or meta not written", 41: "recorder still holds the DAQ",
    130: "stopped by the operator",
}
LIVE_WINDOW_S = 20.0
NOMINAL_V_PER_NT = 2.7          # 1x gain; the plot is a rough pT view
CHANNEL_COLOURS = {"V_z": "#1f77b4", "V_y": "#ff7f0e", "V_z2": "#2ca02c",
                   "V_y2": "#d62728"}


def exit_words(code):
    return EXIT_WORDS.get(code, "unknown code")


# ---------------------------------------------------------------- dialogs

class TextDialog(QDialog):
    """A typed reply, with the whole prompt visible (monospace)."""

    def __init__(self, parent, prompt):
        super().__init__(parent)
        self.setWindowTitle("Operator input")
        lay = QVBoxLayout(self)
        label = QLabel(prompt.strip("\n"))
        label.setFont(QFont("Consolas", 9))
        label.setTextInteractionFlags(Qt.TextSelectableByMouse)
        lay.addWidget(label)
        self.edit = QLineEdit()
        self.edit.setPlaceholderText("leave empty = Enter")
        lay.addWidget(self.edit)
        box = QDialogButtonBox()
        self.ok = box.addButton("OK (Enter)", QDialogButtonBox.AcceptRole)
        self.abort = box.addButton("Abort run", QDialogButtonBox.RejectRole)
        box.accepted.connect(self.accept)
        box.rejected.connect(self.reject)
        lay.addWidget(box)


# -------------------------------------------------------------- live plot

class LivePlot(QWidget):
    VIEWS = ("field", "field, mean removed", "1 s block means",
             "ASD of the window")

    def __init__(self, folder=lambda: "", say=None):
        super().__init__()
        import pyqtgraph as pg
        pg.setConfigOptions(antialias=False, background="w", foreground="k")
        lay = QVBoxLayout(self)
        row = QHBoxLayout()
        self.info = QLabel("No data yet -- live samples appear while a "
                           "record is running.")
        self.info.setWordWrap(True)     # or its width pins the window's
        row.addWidget(self.info, 1)
        row.addWidget(QLabel("view:"))
        self.view = QComboBox()
        self.view.addItems(self.VIEWS)
        self.view.setToolTip("ASD: amplitude spectral density of the window "
                             "(4 s Hann segments). A healthy chain shows a "
                             "white floor near 23 fT/rtHz at 30-45 Hz.")
        self.view.currentTextChanged.connect(self._view_changed)
        row.addWidget(self.view)
        lay.addLayout(row)
        self.plot = pg.PlotWidget()
        self.plot.setLabel("left", "field (pT, nominal 2.7 V/nT)")
        self.plot.setLabel("bottom", "time in the window (s)")
        legend = self.plot.addLegend()
        legend.setBrush(pg.mkBrush(255, 255, 255, 220))
        self.plot.showGrid(x=True, y=True, alpha=0.3)
        self.plot.setDownsampling(auto=True, mode="peak")
        self.plot.setClipToView(True)
        lay.addWidget(self.plot)
        from bfield import clock
        self.actions = PlotActions(
            self.plot,
            name=lambda: (f"live_{self.view.currentText().split()[0].lower()}"
                          f"_{clock.strftime('%Y%m%d_%H%M%S')}"),
            folder=folder, say=say)
        lay.addWidget(self.actions)
        self.curves = {}
        self.buf = {}
        self.fs = 1200.0
        self.total = collections.Counter()
        self.timer = QTimer(self)
        self.timer.timeout.connect(self.redraw)
        self.timer.start(250)

    def clear(self):
        self.buf.clear()
        self.total.clear()
        for c in self.curves.values():
            c.setData([], [])
        self.info.setText("Waiting for the recorder...")

    def add(self, name, samples, fs):
        self.fs = fs
        n = int(LIVE_WINDOW_S * fs)
        if name not in self.buf:
            self.buf[name] = collections.deque(maxlen=n)
        self.buf[name].extend(samples)
        self.total[name] += len(samples)

    def _view_changed(self, view):
        asd = view.startswith("ASD")
        self.plot.setLogMode(x=asd, y=asd)
        if asd:
            self.plot.setLabel("left", "ASD (fT/rtHz, nominal 2.7 V/nT)")
            self.plot.setLabel("bottom", "frequency (Hz)")
        else:
            self.plot.setLabel("left", "field (pT, nominal 2.7 V/nT)")
            self.plot.setLabel("bottom", "time in the window (s)")
        self.plot.enableAutoRange()
        self.redraw()

    def _series(self, y_pT):
        """(x, y) for the current view."""
        import numpy as np
        view = self.view.currentText()
        fs = self.fs
        if view == "field":
            return np.arange(y_pT.size) / fs, y_pT
        if view == "field, mean removed":
            return np.arange(y_pT.size) / fs, y_pT - y_pT.mean()
        if view == "1 s block means":
            n = int(fs)
            k = y_pT.size // n
            if k < 1:
                return np.array([]), np.array([])
            means = y_pT[:k * n].reshape(k, n).mean(axis=1)
            return np.arange(k) + 0.5, means
        # ASD, fT/rtHz
        from scipy.signal import welch
        nper = min(y_pT.size, int(4 * fs))
        if nper < 64:
            return np.array([]), np.array([])
        f, p = welch(y_pT, fs=fs, window="hann", nperseg=nper)
        keep = f > 0
        return f[keep], np.sqrt(p[keep]) * 1000.0

    def redraw(self):
        import numpy as np
        if not self.buf:
            return
        for name, dq in self.buf.items():
            if name not in self.curves:
                self.curves[name] = self.plot.plot(
                    [], [], name=name.replace("V_", "").upper(),
                    pen=CHANNEL_COLOURS.get(name, "#555555"))
                self.actions.restyle()          # the new curve's width
            y = np.asarray(dq, dtype=float) * 1000.0 / NOMINAL_V_PER_NT
            x, yy = self._series(y)
            self.curves[name].setData(x, yy)
        n = max(self.total.values())
        extra = ""
        if self.view.currentText().startswith("ASD") and "V_z" in self.buf:
            x, yy = self._series(np.asarray(self.buf["V_z"], dtype=float)
                                 * 1000.0 / NOMINAL_V_PER_NT)
            band = (x >= 30) & (x <= 45)
            if band.any():
                extra = (f"; Z floor 30-45 Hz median "
                         f"{float(np.median(yy[band])):.1f} fT/rtHz")
        self.info.setText(f"{len(self.buf)} channel(s), {n / self.fs:.0f} s "
                          f"received; last {LIVE_WINDOW_S:g} s (raw volts x "
                          f"nominal gain, not calibrated){extra}")


# ------------------------------------------------------- simulated lab

class SimPanel(QWidget):
    """The simulated world: state, faults, and the simulated bench."""

    def __init__(self, world, sensors=()):
        super().__init__()
        self.world = world
        self.sensors = list(sensors)          # bfield.sensors.Sensor
        lay = QVBoxLayout(self)

        self.state = QTableWidget(0, 8)
        self.state.setHorizontalHeaderLabels(
            ["sensor", "QuSpin label", "USB serial", "laser on", "locked",
             "zeroed", "master", "plugged into"])
        self.state.verticalHeader().setVisible(False)
        self.state.setEditTriggers(QTableWidget.NoEditTriggers)
        self.state.setMaximumHeight(170)
        box = QGroupBox("Simulated sensors (live)")
        bl = QVBoxLayout(box)
        bl.addWidget(self.state)
        self.other = QLabel()
        bl.addWidget(self.other)
        lay.addWidget(box)

        faults = QGroupBox("Fault injection (applies from the next reading)")
        fl = QFormLayout(faults)
        self.f_target = QComboBox()
        self._fill_targets()
        fl.addRow("faults on the sensor", self.f_target)
        self.f_calib = QCheckBox("answers no Calibrate")
        self.f_zero = QCheckBox("field zero never settles")
        self.f_rail = QCheckBox("ai1 (slot 1 Y input) at the QZFM rail")
        self.f_flat = QCheckBox("ai0 (slot 1 Z input) dead / flat")
        self.f_nan = QCheckBox("a NaN in DAQ block 3")
        self.f_dg = QDoubleSpinBox()
        self.f_dg.setRange(0, 600)
        self.f_dg.setSuffix(" s into the taper (0 = off)")
        self.f_laser = QDoubleSpinBox()
        self.f_laser.setRange(0, 36000)
        self.f_laser.setSuffix(" s after auto start (0 = off)")
        for w in (self.f_calib, self.f_zero, self.f_rail, self.f_flat,
                  self.f_nan):
            fl.addRow(w)
        fl.addRow("degausser faults", self.f_dg)
        fl.addRow("the sensor's laser lock drops", self.f_laser)
        row = QHBoxLayout()
        apply_f = QPushButton("Apply faults")
        apply_f.clicked.connect(self.apply_faults)
        clear_f = QPushButton("Clear faults")
        clear_f.clicked.connect(self.clear_faults)
        row.addWidget(apply_f)
        row.addWidget(clear_f)
        row.addStretch(1)
        fl.addRow(row)
        lay.addWidget(faults)

        bench = QGroupBox("Simulated bench: the S(f) coil drive "
                          "(what you would set on the generator)")
        cl = QFormLayout(bench)
        self.c_freq = QDoubleSpinBox()
        self.c_freq.setRange(0, 600)
        self.c_freq.setDecimals(2)
        self.c_freq.setValue(5.0)
        self.c_freq.setSuffix(" Hz (0 = DC)")
        self.c_amp = QDoubleSpinBox()
        self.c_amp.setRange(0, 5000)
        self.c_amp.setValue(450)
        self.c_amp.setSuffix(" mA (0-peak, or the DC level)")
        self.c_orient = QComboBox()
        self.c_orient.addItems(["axial", "transverse"])
        cl.addRow("frequency", self.c_freq)
        cl.addRow("amplitude", self.c_amp)
        cl.addRow("orientation", self.c_orient)
        row = QHBoxLayout()
        on = QPushButton("Drive on")
        on.clicked.connect(self.coil_on)
        off = QPushButton("Drive off")
        off.clicked.connect(self.world.clear_coil)
        row.addWidget(on)
        row.addWidget(off)
        row.addStretch(1)
        cl.addRow(row)
        lay.addWidget(bench)

        plug = QPushButton("Plug in a new sensor (rehearse adding one)")
        plug.setToolTip("A head the sensor list does not know yet appears "
                        "on a new COM port")
        plug.clicked.connect(self.plug_new)
        lay.addWidget(plug)
        self.reset_btn = QPushButton("Reset the simulated lab (all heads "
                                     "cold, no faults, drive off)")
        self.reset_btn.clicked.connect(self.reset)
        lay.addWidget(self.reset_btn)
        lay.addStretch(1)

        self.timer = QTimer(self)
        self.timer.timeout.connect(self.refresh)
        self.timer.start(1000)
        self.refresh()

    def _fill_targets(self):
        self.f_target.clear()
        for s in self.sensors:
            self.f_target.addItem(s.display, s.usb_serial)

    def set_sensors(self, world, sensors):
        """The sensor list changed (Manage sensors)."""
        self.world = world
        self.sensors = list(sensors)
        self._fill_targets()
        self.refresh()

    def plug_new(self):
        usb = self.world.plug_in_new_head()
        self.refresh()
        QMessageBox.information(
            self, "New simulated sensor",
            f"A new simulated sensor is plugged in, USB ID {usb}. Add it "
            "with Manage sensors (QZFM tab): Add sensor, type a head ID, "
            "Scan USB and pick the port no sensor owns.")

    def refresh(self):
        doc = self.world.read()
        heads = doc.get("heads", {})
        number = {s.usb_serial: s.number for s in self.sensors}
        label = {s.usb_serial: s.label for s in self.sensors}
        pins = {}
        for pin, (serial, axis) in (doc.get("wiring") or {}).items():
            pins.setdefault(serial, []).append(f"{pin} ({axis.upper()})")
        self.state.setRowCount(len(heads))
        rows = sorted(heads.items(), key=lambda kv: number.get(kv[0], 999))
        for r, (serial, h) in enumerate(rows):
            vals = [str(number.get(serial, "not in list")),
                    label.get(serial, h.get("label", "")), serial,
                    h.get("laser_on"), h.get("locked"), h.get("zeroed"),
                    h.get("master"), ", ".join(sorted(pins.get(serial, [])))
                    or "-"]
            for c, v in enumerate(vals):
                text = ("yes" if v else "no") if isinstance(v, bool) else str(v)
                self.state.setItem(r, c, QTableWidgetItem(text))
        coil = doc.get("coil")
        if coil:
            drive = (f"{coil['freq_hz']:g} Hz, {coil['amp_mA']:g} mA"
                     if coil.get("freq_hz") else f"DC {coil['dc_mA']:g} mA")
            drive += f" ({coil.get('orientation')})"
        else:
            drive = "off"
        inject = doc.get("inject") or {}
        self.other.setText(
            f"degausser driving: {'YES' if doc.get('dg_driving') else 'no'}"
            f"    coil drive: {drive}    faults: "
            + (", ".join(sorted(inject)) if inject else "none"))

    def apply_faults(self):
        a = self.f_target.currentData() or "A10QHMQMA"
        inject = {}
        if self.f_calib.isChecked():
            inject["calib_silent"] = [a]
        if self.f_zero.isChecked():
            inject["zero_never_settles"] = [a]
        if self.f_rail.isChecked():
            inject["rail_axis"] = ["ai1"]
        if self.f_flat.isChecked():
            inject["flat_axis"] = ["ai0"]
        if self.f_nan.isChecked():
            inject["nonfinite_block"] = 3
        if self.f_dg.value() > 0:
            inject["dg_fault_at_s"] = self.f_dg.value()
        if self.f_laser.value() > 0:
            inject["laser_drop_at_s"] = {a: self.f_laser.value()}
        self.world.update(lambda d: d.update(inject=inject))
        self.refresh()

    def clear_faults(self):
        for w in (self.f_calib, self.f_zero, self.f_rail, self.f_flat,
                  self.f_nan):
            w.setChecked(False)
        self.f_dg.setValue(0)
        self.f_laser.setValue(0)
        self.world.update(lambda d: d.update(inject={}))
        self.refresh()

    def coil_on(self):
        f = self.c_freq.value()
        if f > 0:
            self.world.set_coil(freq_hz=f, amp_mA=self.c_amp.value(),
                                orientation=self.c_orient.currentText())
        else:
            self.world.set_coil(freq_hz=None, dc_mA=self.c_amp.value(),
                                orientation=self.c_orient.currentText())
        self.refresh()

    def reset(self):
        self.world.write(World.default(self.sensors))
        self.clear_faults()


# ----------------------------------------------------------- the window

class MainWindow(QMainWindow):
    def __init__(self, config=None, sim_dir=None):
        super().__init__()
        self.config = config or load_config()
        # re-read from the same file after an edit (tests use a copy)
        self.config_path = (getattr(self.config, "sources", None)
                            or [None])[0]
        self.sim_dir = os.path.abspath(sim_dir or SIM_DATA_DIR)
        os.makedirs(self.sim_dir, exist_ok=True)
        self.world = World(os.path.join(self.sim_dir, "world.json"),
                           sensors=load_sensors(self.config))
        self.bridge = Bridge()
        self.bridge.log.connect(self.on_log)
        self.bridge.progress.connect(self.on_progress)
        self.bridge.ask.connect(self.on_ask)
        self.bridge.data.connect(self.on_data)
        self.bridge.status.connect(self.on_status)
        self.stop_event = None
        self.worker = None
        self.last_code = None
        self.finished_jobs = []
        self.setWindowTitle("B-field -- shield characterisation "
                            "(SIMULATION ONLY)")
        self.fit_to_screen()
        self._build()
        self.done_timer = QTimer(self)
        self.done_timer.timeout.connect(self.poll_worker)
        self.done_timer.start(200)

    # -- layout ---------------------------------------------------------

    def _build(self):
        central = QWidget()
        outer = QVBoxLayout(central)

        top = QHBoxLayout()
        top.addWidget(QLabel("Lab:"))
        self.rb_sim = QRadioButton("Simulated")
        self.rb_sim.setChecked(True)
        self.rb_bench = QRadioButton("Bench (hardware)")
        self.rb_bench.setEnabled(GUI_ALLOW_HARDWARE)
        self.rb_bench.setToolTip("Disabled: the GUI is simulation-only for "
                                 "now. Use run/*.py at the bench.")
        top.addWidget(self.rb_sim)
        top.addWidget(self.rb_bench)
        top.addSpacing(20)
        top.addWidget(QLabel("Clock speed x"))
        self.speed = QDoubleSpinBox()
        self.speed.setRange(1, 1000)
        self.speed.setDecimals(0)
        self.speed.setValue(30)
        self.speed.setToolTip("The simulator runs this many times faster "
                              "than real time (a 900 s record takes 30 s "
                              "at x30)")
        top.addWidget(self.speed)
        top.addSpacing(20)
        self.auto_op = QCheckBox("Auto-operator")
        self.auto_op.setChecked(True)
        self.auto_op.setToolTip("Answer every question and do every hand "
                                "step automatically (the simulator only). "
                                "Off: you are asked, as at the bench.")
        top.addWidget(self.auto_op)
        top.addStretch(1)
        # the path can be long: shown shortened, in full as a tooltip, and
        # it may shrink (it used to fix the window wider than the screen)
        data = QLabel(f"data: {self.sim_dir}")
        data.setToolTip(self.sim_dir)
        data.setMinimumWidth(60)
        data.setSizePolicy(QSizePolicy.Ignored, QSizePolicy.Preferred)
        data.setAlignment(Qt.AlignRight | Qt.AlignVCenter)
        top.addWidget(data, 1)
        open_btn = QPushButton("Open folder")
        open_btn.clicked.connect(lambda: os.startfile(self.sim_dir))
        top.addWidget(open_btn)
        outer.addLayout(top)

        split = QSplitter(Qt.Horizontal)
        left = QWidget()
        ll = QVBoxLayout(left)
        self.forms = QTabWidget()
        # each form scrolls, so the tallest one (the S(f) sweep) does not
        # set the window's minimum height
        self.form_list = []
        for cls in FORMS:
            f = cls(self.config)
            self.form_list.append(f)
            scroll = QScrollArea()
            scroll.setWidgetResizable(True)
            scroll.setFrameShape(QScrollArea.NoFrame)
            scroll.setWidget(f)
            self.forms.addTab(scroll, cls.title)
        self.hook_pickers()
        ll.addWidget(self.forms)
        buttons = QHBoxLayout()
        self.start_btn = QPushButton("Start")
        self.start_btn.setMinimumHeight(36)
        self.start_btn.clicked.connect(self.start)
        self.abort_btn = QPushButton("Abort")
        self.abort_btn.setMinimumHeight(36)
        self.abort_btn.setEnabled(False)
        self.abort_btn.setStyleSheet("QPushButton:enabled "
                                     "{background:#c62828;color:white}")
        self.abort_btn.clicked.connect(self.abort)
        buttons.addWidget(self.start_btn)
        buttons.addWidget(self.abort_btn)
        ll.addLayout(buttons)
        self.status = QLabel("Idle.")
        self.status.setWordWrap(True)
        ll.addWidget(self.status)
        split.addWidget(left)

        # right: indicators / live data / simulated lab on top, the log
        # underneath (half and half; drag the divider to change it)
        right_split = QSplitter(Qt.Vertical)
        right = QTabWidget()
        self.sensor_panel = SensorPanel()
        right.addTab(self.sensor_panel, "Sensors")
        self.live = LivePlot(folder=lambda: self.sim_dir,
                             say=lambda m: self.append(m, "info"))
        right.addTab(self.live, "Live data")
        # Browse opens in the data folder (the Box Test data)
        self.analysis = AnalysisPanel(data_root=lambda: self.config.data_root,
                                      say=lambda m: self.append(m, "info"))
        right.addTab(self.analysis, "Analysis")
        self.sim_panel = SimPanel(self.world, load_sensors(self.config))
        # scrollable, so the tall panel does not pin the top half's height
        # (a tab widget is as tall as its tallest tab; the divider to the
        # log could not move while it was)
        sim_scroll = QScrollArea()
        sim_scroll.setWidgetResizable(True)
        sim_scroll.setWidget(self.sim_panel)
        right.addTab(sim_scroll, "Simulated lab")
        self.right = right
        right_split.addWidget(right)
        log_box = QGroupBox("Log")
        lb = QVBoxLayout(log_box)
        lb.setContentsMargins(4, 4, 4, 4)
        self.log = QPlainTextEdit()
        self.log.setReadOnly(True)
        self.log.setFont(QFont("Consolas", 9))
        self.log.setMaximumBlockCount(20000)
        lb.addWidget(self.log)
        right_split.addWidget(log_box)
        right_split.setChildrenCollapsible(False)
        right_split.setSizes([500, 500])          # half and half
        self.right_split = right_split
        split.addWidget(right_split)
        split.setSizes([500, 900])
        outer.addWidget(split, 1)

        self.progress = QProgressBar()
        self.progress.setRange(0, 1000)
        self.progress.setFormat("%p%")
        self.progress_label = QLabel("")
        pl = QHBoxLayout()
        pl.addWidget(self.progress_label)
        pl.addWidget(self.progress, 1)
        outer.addLayout(pl)
        self.setCentralWidget(central)

    def fit_to_screen(self):
        """Start at a size that fits the screen's free area (taskbar
        excluded), centred: at most 1400 x 860, at most 92% x 88% of it."""
        from PySide6.QtGui import QGuiApplication
        screen = self.screen() or QGuiApplication.primaryScreen()
        if screen is None:
            self.resize(1280, 760)
            return
        area = screen.availableGeometry()
        w = min(1400, int(area.width() * 0.92))
        h = min(860, int(area.height() * 0.88))
        self.resize(w, h)
        self.move(area.x() + (area.width() - w) // 2,
                  area.y() + (area.height() - h) // 2)

    def hook_pickers(self):
        """Point every sensor checklist's Apply button at the panel (also
        after a form's Defaults button rebuilds it)."""
        from bfield.gui.forms import SensorPicker
        for i in range(self.forms.count()):
            for pick in self.form_list[i].findChildren(SensorPicker):
                pick.on_apply = self.apply_sensors
                pick.on_manage = self.manage_sensors

    def scan_usb(self):
        """The serial ports a USB scan finds. Simulation only for now: the
        simulated lab's ports (a real scan comes with the bench mode)."""
        if GUI_ALLOW_HARDWARE and self.rb_bench.isChecked():
            from bfield.devices.qzfm import list_serial_ports
            return list_serial_ports(), False
        return self.world.ports(), True

    def manage_sensors(self):
        from bfield.gui.sensor_manager import SensorManager
        if self.running():
            QMessageBox.information(self, "Run in progress",
                                    "Edit the sensors once the run has "
                                    "finished.")
            return
        dlg = SensorManager(self, self.config, load_sensors(self.config),
                            self.scan_usb)
        if dlg.exec() and dlg.saved:
            self.reload_config()
            self.append("[sensors] sensor list saved to "
                        f"{self.config.sensors_path}", "info")

    def reload_config(self):
        """Re-read the config after the sensor list changed: the
        checklists, the simulated lab and its panel follow."""
        from bfield.gui.forms import SensorPicker
        self.config = load_config(self.config_path)
        sensors = load_sensors(self.config)
        self.world = World(self.world.path, sensors=sensors)
        self.sim_panel.set_sensors(self.world, sensors)
        for f in self.form_list:
            f.config = self.config
            for pick in f.findChildren(SensorPicker):
                pick.reload(self.config)

    def apply_sensors(self, info):
        """Show a sensor choice on the Sensors panel (nothing is opened)."""
        if self.running():
            QMessageBox.information(
                self, "Run in progress",
                "The Sensors panel is showing the running procedure's "
                "sensors; apply again once it has finished.")
            return
        self.sensor_panel.set_sensors(info)
        self.right.setCurrentIndex(0)
        from bfield.sensors import display_of
        self.append("[sensors] shown: " + ";  ".join(
            display_of(r) + " -> DAQ " + ", ".join(
                f"{p} ({a.upper()})" for a, p in (r.get("inputs") or {}).items())
            for r in info), "info")

    # -- running --------------------------------------------------------

    def running(self):
        return self.worker is not None and self.worker.is_alive()

    def start(self):
        if self.running():
            return
        if not self.rb_sim.isChecked():
            QMessageBox.warning(self, "Bench disabled",
                                "The GUI is simulation-only for now.")
            return
        form = self.form_list[self.forms.currentIndex()]
        try:
            job = form.job()
        except BFieldError as e:
            QMessageBox.warning(self, "Settings refused", str(e))
            return
        self.stop_event = threading.Event()
        ui = QtUI(self.bridge, self.stop_event)
        speed = self.speed.value()
        auto = self.auto_op.isChecked()
        self.live.clear()
        self.progress.setValue(0)
        self.progress_label.setText("")
        self.append(f"\n===== {job.title} -- simulated, x{speed:g}"
                    + (", auto-operator" if auto else "") + " =====", "info")
        self.status.setText(f"Running: {job.title}")
        self.start_btn.setEnabled(False)
        self.abort_btn.setEnabled(True)
        self.sim_panel.reset_btn.setEnabled(False)
        self.last_code = None
        self._result = {}

        def work():
            code = 1
            try:
                lab = open_lab(self.config, sim=True, speed=speed,
                               sim_dir=self.sim_dir)
                ctx = Context(ui, config=lab.config,
                              stop_event=self.stop_event)
                if auto:
                    ui.operator = SimOperator(lab.world, ctx.say, job.sweep)
                code = job.fn(ctx, lab)
                if code is None:
                    code = 0
            except BFieldError as e:
                self.bridge.log.emit("", "fatal",
                                     f"ERROR[{e.code}] {type(e).__name__}: {e}",
                                     "error")
                code = e.code
            except BaseException as e:      # a bug: say where
                self.bridge.log.emit("", "fatal",
                                     f"crashed: {type(e).__name__}: {e}\n"
                                     + traceback.format_exc(), "error")
                code = 1
            finally:
                if auto:
                    try:
                        self.world.clear_coil()
                    except Exception:
                        pass
            self._result = {"code": code, "title": job.title}

        self.worker = threading.Thread(target=work, daemon=True,
                                       name="bfield-procedure")
        self.worker.start()

    def poll_worker(self):
        if self.worker is None or self.worker.is_alive():
            return
        res = getattr(self, "_result", {}) or {}
        self.worker = None
        code = res.get("code", 1)
        self.last_code = code
        self.finished_jobs.append((res.get("title"), code))
        words = exit_words(code)
        self.sensor_panel.run_ended(code)
        self.append(f"===== finished: exit {code} ({words}) =====",
                    "info" if code == 0 else "warn")
        self.status.setText(f"Finished: {res.get('title')} -- exit {code} "
                            f"({words})")
        self.start_btn.setEnabled(True)
        self.abort_btn.setEnabled(False)
        self.sim_panel.reset_btn.setEnabled(True)
        if code == 0:
            self.progress.setValue(1000)

    def abort(self):
        if self.stop_event is not None:
            self.stop_event.set()
            self.append("[abort] stop requested -- the procedure stops at "
                        "its next safe point (a degauss is aborted and "
                        "confirmed idle, a recorder writes its meta.json)",
                        "warn")
            self.status.setText("Stopping...")

    def closeEvent(self, event):
        if self.running():
            self.abort()
            QMessageBox.information(
                self, "Still stopping",
                "A procedure is still running; it has been told to stop. "
                "Close the window again once it has finished.")
            event.ignore()
            return
        event.accept()

    # -- signals from the worker ----------------------------------------

    def append(self, text, level):
        colour = {"warn": "#b26a00", "error": "#c62828"}.get(level)
        cursor = self.log.textCursor()
        cursor.movePosition(QTextCursor.End)
        self.log.setTextCursor(cursor)
        if colour:
            import html
            self.log.appendHtml(f'<span style="color:{colour}">'
                                f"{html.escape(text)}</span>"
                                .replace("\n", "<br>"))
        else:
            self.log.appendPlainText(text)

    def on_log(self, stamp, tag, msg, level):
        t = stamp[11:] if len(stamp) > 11 else stamp
        self.append(f"[{t}] [{tag}] {msg}", level)

    def on_progress(self, frac, what):
        self.progress_label.setText(what)
        if frac is None:
            self.progress.setRange(0, 0)
        else:
            self.progress.setRange(0, 1000)
            self.progress.setValue(int(max(0.0, min(frac, 1.0)) * 1000))

    def on_data(self, name, samples, fs):
        self.live.add(name, samples, fs)

    def on_status(self, kind, data):
        if kind == "record" and data.get("run_dir"):
            self.analysis.set_last_run(data["run_dir"])
        self.sensor_panel.on_status(kind, data)

    def on_ask(self, q):
        if q.done.is_set():
            return
        if q.kind == "confirm":
            box = QMessageBox(self)
            box.setWindowTitle("Operator: confirm")
            box.setText(q.text)
            yes = box.addButton("Yes", QMessageBox.YesRole)
            box.addButton("No", QMessageBox.NoRole)
            stop = box.addButton("Abort run", QMessageBox.RejectRole)
            box.exec()
            if box.clickedButton() is stop:
                self.abort()
                q.stop()
            else:
                q.reply(box.clickedButton() is yes)
        elif q.kind == "acknowledge":
            box = QMessageBox(self)
            box.setWindowTitle("Operator: hand step")
            box.setText(q.text)
            done = box.addButton("Done", QMessageBox.AcceptRole)
            stop = box.addButton("Abort run", QMessageBox.RejectRole)
            box.exec()
            if box.clickedButton() is done:
                q.reply(True)
            else:
                self.abort()
                q.stop()
            del stop
        else:
            dlg = TextDialog(self, q.text)
            if dlg.exec() == QDialog.Accepted:
                q.reply(dlg.edit.text())
            else:
                self.abort()
                q.stop()
