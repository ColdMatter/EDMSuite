"""The GUI's implementation of bfield.ui.UI.

A procedure runs in a worker thread and calls these methods from there.
Nothing here touches a widget directly: every call becomes a Qt signal
that the main window handles on the GUI thread. The questions (confirm,
acknowledge, ask_text) block the worker until the window has an answer
-- or until Abort is pressed, which answers them with a stop.
"""

from __future__ import annotations

import threading

from PySide6.QtCore import QObject, Signal

from bfield.errors import Stopped
from bfield.ui import UI


class Question:
    """One question in flight between the worker and the window."""

    def __init__(self, kind, text):
        self.kind = kind            # "confirm" | "acknowledge" | "text"
        self.text = text
        self.answer = None
        self.stopped = False
        self.done = threading.Event()

    def reply(self, answer):
        self.answer = answer
        self.done.set()

    def stop(self):
        self.stopped = True
        self.done.set()


class Bridge(QObject):
    """Signals from the worker thread to the window (queued by Qt)."""
    log = Signal(str, str, str, str)          # stamp, tag, msg, level
    progress = Signal(object, str)            # frac (or None), what
    ask = Signal(object)                      # Question
    data = Signal(str, object, float)         # channel, samples, fs
    status = Signal(str, object)              # kind, data (indicators)


class QtUI(UI):
    """Forward everything to the window. `operator` (optional) answers
    the hand steps itself -- the simulated bench's auto-operator."""

    wants_live_data = True

    def __init__(self, bridge, stop_event, operator=None):
        self.bridge = bridge
        self.stop_event = stop_event
        self.operator = operator
        self.pending = None

    # -- one-way
    def log(self, stamp, tag, msg, level="info"):
        self.bridge.log.emit(stamp, tag, msg, level)

    def progress(self, frac, what):
        self.bridge.progress.emit(frac, what)

    def data(self, name, samples, fs):
        self.bridge.data.emit(name, list(samples), float(fs))

    def status(self, kind, data):
        self.bridge.status.emit(kind, dict(data))

    # -- questions
    def _ask(self, kind, text):
        if self.stop_event.is_set():
            raise Stopped("stopped by the operator")
        q = Question(kind, text)
        self.pending = q
        self.bridge.ask.emit(q)
        # wake up now and then so an Abort pressed elsewhere is seen
        while not q.done.wait(0.2):
            if self.stop_event.is_set():
                q.stop()
        self.pending = None
        if q.stopped:
            raise Stopped("stopped by the operator at a question")
        return q.answer

    def confirm(self, question):
        if self.operator is not None:
            return self.operator.confirm(question)
        return bool(self._ask("confirm", question))

    def acknowledge(self, message):
        if self.operator is not None:
            return self.operator.acknowledge(message)
        self._ask("acknowledge", message)

    def ask_text(self, prompt):
        if self.operator is not None:
            return self.operator.ask_text(prompt)
        return self._ask("text", prompt) or ""
