"""One form per procedure. A form knows its fields and turns them into a
job: a callable run(ctx, lab) -> exit code, the same call a run/*.py
script makes. Fields left at their defaults take config/lab.toml.
"""

from __future__ import annotations

from PySide6.QtWidgets import (QCheckBox, QComboBox, QDoubleSpinBox,
                               QFormLayout, QHBoxLayout, QLabel, QLineEdit,
                               QPushButton, QSpinBox, QVBoxLayout, QWidget)

from bfield.errors import ConfigError
from bfield.operations.degauss_cycle import (DegaussSession, DegaussSettings,
                                             parse_amps)


def _num(lo, hi, value, decimals=1, step=1.0, suffix=""):
    w = QDoubleSpinBox()
    w.setRange(lo, hi)
    w.setDecimals(decimals)
    w.setSingleStep(step)
    w.setValue(value)
    if suffix:
        w.setSuffix(" " + suffix)
    return w


def _int(lo, hi, value):
    w = QSpinBox()
    w.setRange(lo, hi)
    w.setValue(value)
    return w


def _combo(items, current=None):
    w = QComboBox()
    w.addItems(items)
    if current is not None:
        w.setCurrentText(current)
    return w


def _text(value="", placeholder=""):
    w = QLineEdit(value)
    if placeholder:
        w.setPlaceholderText(placeholder)
    return w


class HeightGrip(QWidget):
    """A thin bar under a widget; drag it to make the widget taller or
    shorter."""

    def __init__(self, target, minimum=60):
        super().__init__()
        from PySide6.QtCore import Qt
        self.target = target
        self.minimum = minimum
        self.setFixedHeight(8)
        self.setCursor(Qt.SizeVerCursor)
        self.setToolTip("drag to resize")
        self.setAttribute(Qt.WA_StyledBackground, True)   # paint the bar
        self.setStyleSheet("background:#cfcfcf; border-radius:3px;")
        self._start = None

    def mousePressEvent(self, ev):
        self._start = (ev.globalPosition().y(), self.target.height())

    def mouseMoveEvent(self, ev):
        if self._start is None:
            return
        y0, h0 = self._start
        h = max(self.minimum, int(h0 + ev.globalPosition().y() - y0))
        self.target.setFixedHeight(h)

    def mouseReleaseEvent(self, ev):
        self._start = None


class SensorPicker(QWidget):
    """A checklist of the configured sensors (number, QuSpin label, USB
    serial, location) and, under it, where each chosen one plugs in."""

    def __init__(self, config, default=(1,), exactly=None,
                 with_master=False):
        super().__init__()
        from PySide6.QtWidgets import QListWidget
        self.config = config
        self.exactly = exactly
        self.with_master = with_master
        lay = QVBoxLayout(self)
        lay.setContentsMargins(0, 0, 0, 0)
        self.list = QListWidget()
        self.list.setToolTip("Sensors from config/sensors.toml -- add or "
                             "edit them with Manage sensors.")
        self.list.setFixedHeight(150)
        self.list.itemChanged.connect(self.update_plan)
        lay.addWidget(self.list)
        lay.addWidget(HeightGrip(self.list))
        # which sensor drives the modulation (two or more)
        self.master_row = QWidget()
        mrow = QHBoxLayout(self.master_row)
        mrow.setContentsMargins(0, 0, 0, 0)
        mrow.addWidget(QLabel("Modulation master:"))
        self.master_box = QComboBox()
        self.master_box.setToolTip(
            "With two or more sensors one drives the modulation (command "
            "78) and the others follow (command 73; QuSpin: one master per "
            "bench). This only decides which head gets the master command, "
            "not where it plugs in.")
        self.master_box.currentIndexChanged.connect(lambda *_: None)
        mrow.addWidget(self.master_box, 1)
        lay.addWidget(self.master_row)
        row = QHBoxLayout()
        self.apply_btn = QPushButton("Apply to Sensors panel")
        self.apply_btn.setToolTip(
            "Show the chosen sensors on the Sensors panel (right), with "
            "where each one plugs in. Talks to no hardware.")
        self.apply_btn.clicked.connect(self.apply)
        row.addWidget(self.apply_btn)
        self.manage_btn = QPushButton("Manage sensors...")
        self.manage_btn.setToolTip("Add, edit or remove sensors: number, "
                                   "head ID, USB ID, location note")
        self.manage_btn.clicked.connect(
            lambda: self.on_manage and self.on_manage())
        row.addWidget(self.manage_btn)
        row.addStretch(1)
        lay.addLayout(row)
        self.on_manage = None         # the window sets this
        self.axes_source = None       # the form's axes choice (callable)
        self.fill(config, default)
        # only says something when the choice cannot run (too many
        # sensors, the sweep needs two, ...)
        self.plan = QLabel()
        self.plan.setWordWrap(True)
        self.plan.setStyleSheet("color:#c62828")
        lay.addWidget(self.plan)
        self.on_apply = None          # the window sets this
        self.update_plan()

    def fill(self, config, checked):
        """(Re)build the list from the config, ticking `checked`."""
        from PySide6.QtCore import Qt
        from PySide6.QtWidgets import QListWidgetItem
        from bfield.sensors import load_sensors
        self.config = config
        self.sensors = load_sensors(config)
        self.list.blockSignals(True)
        self.list.clear()
        for s in self.sensors:
            item = QListWidgetItem(
                f"{s.number}   {s.label}    USB {s.usb_serial}    "
                f"{s.wiring}    " + (s.location or "(location not set)"))
            item.setData(Qt.UserRole, s.number)
            item.setFlags(item.flags() | Qt.ItemIsUserCheckable)
            item.setCheckState(Qt.Checked if s.number in checked
                               else Qt.Unchecked)
            self.list.addItem(item)
        self.list.blockSignals(False)
        if hasattr(self, "plan"):
            self.update_plan()

    def reload(self, config):
        """After Manage sensors: the new list, the same ticks and master."""
        keep, master = self.numbers(), self.master()
        self.fill(config, keep)
        if master is not None:
            self.set_master(master)

    def master(self):
        """The chosen master's number, or None (one sensor / no choice)."""
        if not self.master_box.isEnabled() or self.master_box.count() == 0:
            return None
        return self.master_box.currentData()

    def set_master(self, number):
        i = self.master_box.findData(number)
        if i >= 0:
            self.master_box.setCurrentIndex(i)

    def _refresh_master(self):
        nums = self.numbers()
        current = self.master_box.currentData()
        self.master_box.blockSignals(True)
        self.master_box.clear()
        by_num = {s.number: s for s in self.sensors}
        for n in nums:
            if n in by_num:
                self.master_box.addItem(by_num[n].display, n)
        if current in nums:
            self.set_master(current)
        self.master_box.blockSignals(False)
        several = len(nums) >= 2
        self.master_box.setEnabled(several)
        self.master_row.setVisible(self.with_master and several)

    def numbers(self):
        from PySide6.QtCore import Qt
        return [self.list.item(i).data(Qt.UserRole)
                for i in range(self.list.count())
                if self.list.item(i).checkState() == Qt.Checked]

    def set_numbers(self, numbers):
        from PySide6.QtCore import Qt
        for i in range(self.list.count()):
            it = self.list.item(i)
            it.setCheckState(Qt.Checked if it.data(Qt.UserRole) in numbers
                             else Qt.Unchecked)

    def selection(self):
        """bfield.sensors.Selection, or ConfigError with the reason."""
        from bfield.sensors import Selection
        nums = self.numbers()
        if not nums:
            raise ConfigError("choose at least one sensor")
        if self.exactly and len(nums) != self.exactly:
            raise ConfigError(f"this procedure needs exactly {self.exactly} "
                              f"sensors ({len(nums)} chosen)")
        axes = self.axes_source() if self.axes_source else None
        return Selection(self.config, nums,
                         master=self.master() if self.with_master else None,
                         axes=axes)

    def update_plan(self, *_):
        self._refresh_master()
        try:
            self.selection()
            self.plan.setText("")
            self.plan.setVisible(False)
            self.apply_btn.setEnabled(True)
        except ConfigError as e:
            self.plan.setText(str(e))
            self.plan.setVisible(True)
            self.apply_btn.setEnabled(False)

    def apply(self):
        try:
            sel = self.selection()
        except ConfigError as e:
            self.plan.setText(str(e))
            self.plan.setVisible(True)
            return
        if self.on_apply is not None:
            self.on_apply(sel.info())


AXES_CHOICES = {
    "default (Z for one sensor, Z and Y for more)": None,
    "Z only (single-axis mode)": "z",
    "Y only (single-axis mode)": "y",
    "Z and Y (dual-axis mode)": "z,y",
}


class Job:
    """What the window runs: a title, the call, and extras."""

    def __init__(self, title, fn, sweep=None, outdir_hint=None):
        self.title = title
        self.fn = fn
        self.sweep = sweep             # SweepSettings, for the auto-operator
        self.outdir_hint = outdir_hint


class ProcedureForm(QWidget):
    title = ""
    blurb = ""

    def __init__(self, config):
        super().__init__()
        self.config = config
        outer = QVBoxLayout(self)
        about = QLabel(self.blurb)
        about.setWordWrap(True)
        about.setObjectName("blurb")
        outer.addWidget(about)
        self.form = QFormLayout()
        outer.addLayout(self.form)
        self.build()
        row = QHBoxLayout()
        quick = QPushButton("Quick-test values")
        quick.setToolTip("Short durations, for trying the procedure out in "
                         "the simulator")
        quick.clicked.connect(self.quick_test)
        defaults = QPushButton("Defaults")
        defaults.clicked.connect(self.reset)
        row.addWidget(quick)
        row.addWidget(defaults)
        row.addStretch(1)
        outer.addLayout(row)
        outer.addStretch(1)

    def add(self, label, widget, tip=None):
        if tip:
            widget.setToolTip(tip)
        self.form.addRow(label, widget)
        return widget

    def outdir(self):
        t = self.out.text().strip()
        return t or None

    def run_name(self):
        t = self.name_edit.text().strip()
        return t or None

    def add_outdir(self, auto_name="automatic"):
        self.name_edit = self.add(
            "Run name", _text(placeholder=f"empty = {auto_name}"),
            "The run's folder is <data folder>/<Mon YYYY>/<DD>/<NNN>_<name>, "
            "NNN counting up through the day. Spaces become '-'.")
        self.out = self.add("Output folder", _text(
            placeholder="empty = the dated run folder under the data folder"),
            "A folder here replaces the dated run folder (the run name is "
            "then not used)")

    # overridden
    def build(self):
        raise NotImplementedError

    def reset(self):
        # the rebuilt sensor checklist keeps its Apply hook
        old = getattr(self, "pick", None)
        hooks = ((old.on_apply, old.on_manage) if old is not None
                 else (None, None))
        for i in reversed(range(self.form.rowCount())):
            self.form.removeRow(i)
        self.build()
        if old is not None and hasattr(self, "pick"):
            self.pick.on_apply, self.pick.on_manage = hooks

    def quick_test(self):
        pass

    def job(self):
        raise NotImplementedError


# ----------------------------------------------------------------- QZFM

class QZFMForm(ProcedureForm):
    title = "QZFM"
    blurb = ("Bring the heads up, zero, calibrate, prove the recording with "
             "a guard record -- and, in Record mode, take one record. Same "
             "as run/qzfm.py.")

    def build(self):
        self.mode = self.add("Mode", _combo(
            ["record", "setup", "zero", "standby", "status"], "record"))
        self.pick = self.add("Sensors", SensorPicker(self.config, (1, 2),
                                                     with_master=True))
        self.axes = self.add("Axes to record", _combo(list(AXES_CHOICES)),
                             "Single-axis mode is 1.35x quieter than dual "
                             "(white floor 14.8 vs 20.0 fT/rtHz, previous "
                             "student's notes): record one axis when the "
                             "floor is the measurement. Each sensor records "
                             "the chosen axes it is wired for.")
        self.pick.axes_source = self.axes_value
        self.axes.currentIndexChanged.connect(self.pick.update_plan)
        self.seconds = self.add("Record length", _num(1, 36000, 900, 0, 60,
                                                      "s"))
        self.sync = self.add("Modulation sync", _combo(
            ["default", "master-slave", "none", "independent"]),
            "default: master-slave with two or more sensors, none with "
            "one")
        self.require = self.add("Require cal factors", QCheckBox())
        self.notes = self.add("Notes", _text(placeholder="stored in meta.json"))
        self.add_outdir("e.g. SingleSensorZ, TwoSensorsZY")

    def axes_value(self):
        return AXES_CHOICES.get(self.axes.currentText())

    def quick_test(self):
        self.seconds.setValue(30)

    def job(self):
        from bfield.procedures.qzfm_session import QZFMPlan, QZFMSession
        mode = self.mode.currentText()
        sel = self.pick.selection()
        master = self.pick.master()
        sync = self.sync.currentText()
        plan_kw = dict(seconds=self.seconds.value(),
                       axes=self.axes_value(),
                       sync=None if sync == "default" else sync,
                       require_factors=self.require.isChecked(),
                       notes=self.notes.text().strip() or None,
                       outdir=self.outdir(), name=self.run_name())

        def run(ctx, lab):
            plan = QZFMPlan.for_sensors(lab.config, sel.numbers,
                                        master=master, **plan_kw)
            session = QZFMSession(ctx, lab, plan)
            if mode == "status":
                for head, sensor, s, blank in session.status():
                    leds = ", ".join(f"{k} {'on' if s[k] else 'OFF'}" for k in
                                     ("laser_on", "cellT_lock", "laser_lock",
                                      "zeroed"))
                    ctx.say("status",
                            f"{sensor.display}: {leds}; compensation B0 "
                            f"{s['B0_pT']:.1f} By {s['By_pT']:.1f} Bz "
                            f"{s['Bz_pT']:.1f} pT; cell T error "
                            f"{s['cellT_err']:g}"
                            + (" -- CAUTION: blank frame" if blank else ""))
                return 0
            return {"record": session.record, "setup": session.setup,
                    "zero": session.zero, "standby": session.standby}[mode]()

        return Job(f"QZFM {mode} (sensors "
                   f"{', '.join(map(str, sel.numbers))})", run)


# -------------------------------------------------------------- degauss

class DegaussForm(ProcedureForm):
    title = "Degauss"
    blurb = ("Twinleaf DG: status, a stepped saturation check, or one full "
             "cycle with the readback gate and taper QC. Same as "
             "run/degauss.py. You are asked the safety checklist before "
             "anything is driven.")

    def build(self):
        d = self.config.section("degausser")
        self.mode = self.add("Mode", _combo(["run", "check", "status"], "run"))
        self.amp = self.add("Amplitude (run)", _num(0.1, 10, 8.0, 2, 0.5, "A"))
        self.amps = self.add("Amplitudes (check)", _text("1,1.5,2,2.5"),
                             "increasing, comma separated")
        self.freq = self.add("Frequency", _num(3, 60, 15.0, 1, 1, "Hz"))
        self.soak = self.add("Soak", _num(0, 120, 15.0, 1, 1, "s"))
        self.taper = self.add("Taper", _num(5, 600, 300.0, 0, 10, "s"))
        self.coil = self.add("Coil label", _text("parallel4"),
                             "the connected winding -- required for a run")
        self.max_amp = self.add("Max amplitude", _num(
            0.1, 10, float(d.get("max_amp_A", 8.0)), 2, 0.5, "A"))
        self.add_outdir("Degauss (DegaussCheck for check)")

    def quick_test(self):
        self.taper.setValue(60)

    def job(self):
        mode = self.mode.currentText()
        s = DegaussSettings(
            mode=mode, port=self.config.section("degausser").get("port"),
            amp=self.amp.value(),
            amps=parse_amps(self.amps.text()) if mode == "check" else None,
            freq=self.freq.value(), soak=self.soak.value(),
            taper=self.taper.value(), max_amp=self.max_amp.value(),
            coil=self.coil.text().strip() or None, outdir=self.outdir(),
            name=self.run_name())
        return Job(f"Degauss {mode}",
                   lambda ctx, lab: DegaussSession(ctx, lab, s).run())


# ----------------------------------------------------------- relaxation

class RelaxForm(ProcedureForm):
    title = "Relaxation"
    blurb = ("The evening program: degauss the shield, then timed records at "
             "t0 + the listed minutes, with sensor standby in the gaps. "
             "Plan shows the schedule and touches nothing. Same as "
             "run/relax.py.")

    def build(self):
        self.mode = self.add("Mode", _combo(["plan", "go"], "plan"))
        self.pick = self.add("Sensors", SensorPicker(self.config, (1,)))
        # the relaxation records Z (single-axis), as the old program did
        self.pick.axes_source = lambda: "z"
        self.at = self.add("Points at (min after t0)", _text("5,20,45,80,115"))
        self.seconds = self.add("Record length", _num(1, 7200, 900, 0, 60,
                                                      "s"))
        self.checkout = self.add("Checkout record", _num(0, 600, 20, 0, 5,
                                                         "s"))
        self.winding = self.add("Winding", _combo(["parallel", "series"],
                                                  "parallel"))
        self.taper = self.add("Taper", _num(0, 600, 0, 0, 10, "s"),
                              "0 = the winding's default (300 s)")
        self.no_sleep = self.add("No standby between points", QCheckBox())
        self.add_outdir("Relaxation")

    def quick_test(self):
        self.at.setText("1,3")
        self.seconds.setValue(30)
        self.checkout.setValue(0)
        self.taper.setValue(60)

    def job(self):
        from bfield.procedures import relaxation as rx
        mode = self.mode.currentText()
        s = rx.RelaxSettings(
            device_name=self.pick.selection().device_name,
            map=self.pick.selection().map,
            at=self.at.text().strip(), seconds=self.seconds.value(),
            checkout_seconds=self.checkout.value(),
            winding=self.winding.currentText(),
            taper=self.taper.value() or None,
            no_sleep=self.no_sleep.isChecked(), outdir=self.outdir(),
            name=self.run_name())
        return Job(f"Relaxation {mode}",
                   lambda ctx, lab: rx.run(ctx, lab, s, mode))


# --------------------------------------------------------------- S(f)

class SweepForm(ProcedureForm):
    title = "S(f) sweep"
    blurb = ("The shielding-factor sweep: the generator is set by hand for "
             "each frequency (the auto-operator does it in the simulator). "
             "Plan shows the order and touches nothing. Same as "
             "run/sf_sweep.py.")

    def build(self):
        from bfield.procedures import sf_sweep as sf
        self.mode = self.add("Mode", _combo(["plan", "precheck", "go", "dc"],
                                            "plan"))
        self.freqs = self.add("Frequencies", _text(sf.DEFAULT_FREQS), "Hz")
        self.control = self.add("Control frequency", _num(
            0.01, 500, sf.DEFAULT_CONTROL_HZ, 2, 1, "Hz"))
        self.amp = self.add("Drive amplitude", _num(10, 2000, sf.DEFAULT_AMP_MA,
                                                    0, 10, "mA"))
        self.min_seconds = self.add("Min point length", _num(
            1, 7200, sf.DEFAULT_MIN_SECONDS, 0, 60, "s"))
        self.min_cycles = self.add("Min cycles", _num(1, 1000,
                                                      sf.DEFAULT_MIN_CYCLES, 0))
        self.null_seconds = self.add("Null record", _num(
            0, 7200, sf.DEFAULT_NULL_SECONDS, 0, 60, "s"))
        self.orientation = self.add("Orientation", _combo(
            sorted(sf.ORIENTATION_MAP), sf.DEFAULT_ORIENTATION))
        self.seed = self.add("Shuffle seed", _int(-1, 10 ** 9, -1),
                             "-1 = pick one (it is recorded)")
        self.dc_ma = self.add("DC current (dc)", _num(10, 5000, sf.DEFAULT_DC_MA,
                                                      0, 50, "mA"))
        self.dc_dwell = self.add("DC dwell (dc)", _num(
            1, 3600, sf.DEFAULT_DC_DWELL_S, 0, 10, "s"))
        self.dc_blocks = self.add("DC blocks (dc)", _int(1, 100,
                                                         sf.DEFAULT_DC_BLOCKS))
        self.pick = self.add("Sensors (two)",
                             SensorPicker(self.config, (1, 2), exactly=2))
        self.pick.axes_source = lambda: "z,y"
        self.skip_setup = self.add("Skip sensor setup", QCheckBox())
        self.add_outdir("e.g. SfSweepAxial, SfPrecheckAxial")

    def sweep_map(self):
        """Both sensors' wired axes; the driven axis (Z axial, Y
        transverse) must be recorded on both."""
        from bfield.procedures import sf_sweep as sf
        sel = self.pick.selection()
        driven = sf.ORIENTATION_AXIS[self.orientation.currentText()]
        chans = sel.channels()
        on_driven = {k for _n, k, ax, _p in chans if ax == driven}
        if len(on_driven) < 2:
            raise ConfigError(
                f"the {self.orientation.currentText()} sweep needs both "
                f"sensors' {driven.upper()} wired (Manage sensors); recorded "
                f"here: {', '.join(n.upper() for n, *_ in chans)}")
        return sel.map

    def quick_test(self):
        self.freqs.setText("5,25")
        self.control.setValue(5)
        self.min_seconds.setValue(20)
        self.min_cycles.setValue(10)
        self.null_seconds.setValue(10)
        self.dc_dwell.setValue(15)
        self.dc_blocks.setValue(1)

    def job(self):
        from bfield.procedures import sf_sweep as sf
        mode = self.mode.currentText()
        kw = dict(mode=mode, freqs=self.freqs.text().strip(),
                  control_hz=self.control.value(), amp_ma=self.amp.value(),
                  min_seconds=self.min_seconds.value(),
                  min_cycles=self.min_cycles.value(),
                  null_seconds=self.null_seconds.value(),
                  orientation=self.orientation.currentText(),
                  seed=None if self.seed.value() < 0 else self.seed.value(),
                  dc_ma=self.dc_ma.value(), dc_dwell=self.dc_dwell.value(),
                  dc_blocks=self.dc_blocks.value(),
                  skip_setup=self.skip_setup.isChecked(),
                  ids=self.pick.selection().device_name,
                  map=self.sweep_map(),
                  outdir=self.outdir(), name=self.run_name())
        s = sf.SweepSettings.from_config(self.config, **kw)
        return Job(f"S(f) {mode}",
                   lambda ctx, lab: sf.SfSweep(ctx, lab, s).run(), sweep=s)


FORMS = (QZFMForm, DegaussForm, RelaxForm, SweepForm)
