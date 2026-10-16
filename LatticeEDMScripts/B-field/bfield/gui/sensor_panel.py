"""Per-sensor indicator cards: setup stages, LEDs, calibration factors,
field-zero compensation.

Fed by the procedure's status channel (Context.status): a "sensors"
event creates one card per sensor of the run, "stage" events light the
stage chips, and every status frame read from a head ("frame") updates
its LEDs, compensation and cell temperature. The same events come from
real heads at the bench, so this panel is not simulator-only.
"""

from __future__ import annotations

import math

from PySide6.QtCore import Qt
from PySide6.QtGui import QFont
from PySide6.QtWidgets import (QFrame, QGridLayout, QHBoxLayout, QLabel,
                               QScrollArea, QVBoxLayout, QWidget)

from bfield import clock
from bfield.constants import CAL_FACTOR_WARN, COMP_ABORT_PT

STAGES = ("connect", "auto start", "gain / mode", "field zero", "calibrate",
          "guard record", "sync check", "standby")
STAGE_LOOK = {
    # state: (background, text colour, symbol)
    "pending": ("#eeeeee", "#888888", ""),
    "running": ("#fff3cd", "#7a5a00", "…"),
    "passed": ("#d4edda", "#1e6b34", "✓"),
    "skipped": ("#e2e6ea", "#495057", "–"),
    "warn": ("#ffe5b4", "#8a4b00", "!"),
    "failed": ("#f8d7da", "#a3202f", "✗"),
    "stopped": ("#f1d4d7", "#7d3b42", "■"),
}
LEDS = (("laser_on", "laser on"), ("cellT_lock", "cell temp lock"),
        ("laser_lock", "laser lock"), ("zeroed", "field zeroed"),
        ("master", "modulation master"))
COMP_KEYS = (("B0_pT", "B0"), ("By_pT", "By"), ("Bz_pT", "Bz"))
FACTOR_AXES = ("z", "y", "x")


def _fmt(v, spec):
    if v is None or (isinstance(v, float) and not math.isfinite(v)):
        return "--"
    return format(v, spec)


class StageChip(QLabel):
    def __init__(self, name):
        super().__init__()
        self.name = name
        self.state = "pending"
        self.setAlignment(Qt.AlignCenter)
        self.setMinimumWidth(96)
        self.set_state("pending", None)

    def set_state(self, state, detail):
        self.state = state
        bg, fg, sym = STAGE_LOOK.get(state, STAGE_LOOK["pending"])
        self.setText(f"{sym} {self.name}".strip())
        self.setStyleSheet(f"QLabel {{background:{bg}; color:{fg}; "
                           "border-radius:4px; padding:3px 6px;}")
        self.setToolTip(f"{self.name}: {state}"
                        + (f"\n{detail}" if detail else ""))


class Led(QLabel):
    def __init__(self, label, off_is_bad=True):
        super().__init__()
        self.label = label
        self.off_is_bad = off_is_bad    # "not master" is not a fault
        self.set_on(None)

    def set_on(self, on):
        off = "#c62828" if self.off_is_bad else "#9e9e9e"
        colour = "#2e7d32" if on else ("#bdbdbd" if on is None else off)
        word = "?" if on is None else ("on" if on else "off")
        self.setText(f'<span style="color:{colour}">●</span> '
                     f"{self.label}")
        self.setToolTip(f"{self.label}: {word}")


class Value(QLabel):
    def __init__(self, width=58):
        super().__init__("--")
        self.setMinimumWidth(width)
        self.setFont(QFont("Consolas", 10))
        self.setAlignment(Qt.AlignRight | Qt.AlignVCenter)

    def put(self, text, level=None, tip=None):
        self.setText(text)
        colour = {"warn": "#b26a00", "bad": "#c62828"}.get(level, "")
        self.setStyleSheet(f"color:{colour}" if colour else "")
        if tip:
            self.setToolTip(tip)


class SensorCard(QFrame):
    def __init__(self, rec):
        super().__init__()
        self.rec = rec
        self.setFrameShape(QFrame.StyledPanel)
        lay = QVBoxLayout(self)

        title = QLabel(self._title(rec))
        f = title.font()
        f.setPointSize(f.pointSize() + 2)
        f.setBold(True)
        title.setFont(f)
        lay.addWidget(title)
        sub = QLabel(self._subtitle(rec))
        sub.setStyleSheet("color:#555")
        sub.setWordWrap(True)
        lay.addWidget(sub)

        # two rows of four, in order, so a narrow window still fits them
        chips = QGridLayout()
        chips.setHorizontalSpacing(6)
        chips.setVerticalSpacing(4)
        self.chips = {}
        for i, name in enumerate(STAGES):
            c = StageChip(name)
            self.chips[name] = c
            chips.addWidget(c, i // 4, i % 4)
        chips.setColumnStretch(4, 1)
        lay.addLayout(chips)

        leds = QHBoxLayout()
        self.leds = {}
        for key, label in LEDS:
            led = Led(label, off_is_bad=(key != "master"))
            self.leds[key] = led
            leds.addWidget(led)
        leds.addStretch(1)
        lay.addLayout(leds)

        grid = QGridLayout()
        grid.setHorizontalSpacing(8)
        grid.addWidget(QLabel("<b>Calibration factor</b>"), 0, 0)
        self.factors = {}
        for i, ax in enumerate(FACTOR_AXES):
            grid.addWidget(QLabel(ax.upper()), 0, 1 + 2 * i, Qt.AlignRight)
            v = Value()
            self.factors[ax] = v
            grid.addWidget(v, 0, 2 + 2 * i)
        comp_label = QLabel("<b>Compensation (pT)</b>")
        comp_label.setToolTip(
            "Field-zero compensation: the field the sensor's own coils "
            "cancel after Field Zero, per "
            "axis, as it reports it (QuSpin's names B0, By, Bz). Near "
            f"+/-{COMP_ABORT_PT / 1000:g} nT the run aborts: the coil range "
            "is +/-50 nT.")
        grid.addWidget(comp_label, 1, 0)
        self.comp = {}
        for i, (key, label) in enumerate(COMP_KEYS):
            grid.addWidget(QLabel(label), 1, 1 + 2 * i, Qt.AlignRight)
            v = Value(64)
            self.comp[key] = v
            grid.addWidget(v, 1, 2 + 2 * i)
        grid.addWidget(QLabel("<b>Zero residual</b>"), 2, 0)
        self.residual = Value()
        grid.addWidget(self.residual, 2, 2)
        grid.addWidget(QLabel("pT/s"), 2, 3)
        grid.addWidget(QLabel("<b>Cell T err</b>"), 2, 4, 1, 1,
                       Qt.AlignRight)
        self.cellt = Value()
        grid.addWidget(self.cellt, 2, 6)
        grid.setColumnStretch(7, 1)
        lay.addLayout(grid)
        self.updated = QLabel("no status frame yet")
        self.updated.setStyleSheet("color:#888")
        lay.addWidget(self.updated)

    @staticmethod
    def _title(rec):
        if rec.get("number") is not None:
            return f"Sensor {rec['number']}   {rec.get('label') or ''}"
        return f"Head {rec.get('head')}   {rec.get('usb_serial')}"

    @staticmethod
    def _subtitle(rec):
        bits = [f"USB {rec.get('usb_serial')}",
                rec.get("location") or "location not set"]
        pins = rec.get("inputs") or {}
        if pins:
            bits.append("DAQ " + ", ".join(f"{p} ({a.upper()})"
                                           for a, p in pins.items()))
        if rec.get("modulation"):
            bits.append(f"modulation {rec['modulation']}")
        return "   ·   ".join(bits)

    def stage(self, name, state, detail=None, **data):
        chip = self.chips.get(name)
        if chip is not None:
            chip.set_state(state, detail)
        if name == "calibrate" and "factors" in data:
            self.show_factors(data.get("factors") or {})
        if name == "field zero" and data.get("residual") is not None:
            r = data["residual"]
            self.residual.put(_fmt(r, ".1f"))
        if name == "gain / mode" and "role" in data:
            self.leds["master"].set_on(data["role"] == "master")

    def show_factors(self, factors):
        for ax, v in self.factors.items():
            f = factors.get(ax)
            if f is None:
                v.put("--", "bad" if ax in ("z", "y") else None,
                       f"no {ax} factor parsed from the Calib reply")
            else:
                v.put(f"{f:.2f}", "warn" if f > CAL_FACTOR_WARN else None,
                       f"{ax}: {f:g}" + (f" -- above {CAL_FACTOR_WARN:g}, "
                                          "the cell may need service"
                                          if f > CAL_FACTOR_WARN else ""))

    def frame(self, fr):
        for key, led in self.leds.items():
            if key in fr:
                led.set_on(bool(fr[key]))
        for key, v in self.comp.items():
            x = fr.get(key)
            level = None
            if isinstance(x, float) and math.isfinite(x):
                if abs(x) > COMP_ABORT_PT:
                    level = "bad"
                elif abs(x) > 0.66 * COMP_ABORT_PT:
                    level = "warn"
            v.put(_fmt(x, ".1f"), level)
        c = fr.get("cellT_err")
        self.cellt.put(_fmt(c, ".4f"),
                        "warn" if isinstance(c, float) and math.isfinite(c)
                        and abs(c) > 0.05 else None)
        self.updated.setText(f"last status frame {clock.strftime('%H:%M:%S')}")

    def mark_unfinished(self, state):
        for chip in self.chips.values():
            if chip.state == "running":
                chip.set_state(state, "the run ended during this stage")


class SensorPanel(QWidget):
    def __init__(self):
        super().__init__()
        outer = QVBoxLayout(self)
        outer.setContentsMargins(0, 0, 0, 0)
        self.hint = QLabel("Sensor indicators appear when a procedure opens "
                           "its sensors (QZFM session, relaxation, S(f) "
                           "sweep).")
        self.hint.setWordWrap(True)
        self.hint.setStyleSheet("color:#666; padding:6px")
        outer.addWidget(self.hint)
        scroll = QScrollArea()
        scroll.setWidgetResizable(True)
        self.inner = QWidget()
        self.cards_lay = QVBoxLayout(self.inner)
        self.cards_lay.addStretch(1)
        scroll.setWidget(self.inner)
        outer.addWidget(scroll, 1)
        self.cards = {}

    def set_sensors(self, info):
        """A new session: one fresh card per sensor (stages reset)."""
        for card in self.cards.values():
            card.setParent(None)
            card.deleteLater()
        self.cards = {}
        for rec in info:
            card = SensorCard(rec)
            # keyed like the status events: the sensor key ("s1")
            self.cards[rec.get("sensor", rec.get("head"))] = card
            self.cards_lay.insertWidget(self.cards_lay.count() - 1, card)
        self.hint.setVisible(not self.cards)

    def on_status(self, kind, data):
        if kind == "sensors":
            self.set_sensors(data.get("sensors") or [])
        elif kind == "stage":
            card = self.cards.get(data.get("head"))
            if card is not None:
                extra = {k: v for k, v in data.items()
                         if k not in ("head", "stage", "state", "detail")}
                card.stage(data.get("stage"), data.get("state"),
                           data.get("detail"), **extra)
        elif kind == "frame":
            card = self.cards.get(data.get("head"))
            if card is not None:
                card.frame(data.get("frame") or {})

    def run_ended(self, code):
        state = ("stopped" if code == 130 else "failed") if code else None
        if state:
            for card in self.cards.values():
                card.mark_unfinished(state)
