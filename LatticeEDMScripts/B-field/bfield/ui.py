"""How a procedure talks to the human at the bench.

Operations and procedures never print, never call input() and never
exit. They go through a Context, which forwards to a UI:

  ConsoleUI  -- the terminal (run/*.py)
  a GUI      -- implements the same four methods (log panel, dialogs,
                progress bar, live plot)

The Context also owns the operations log (<outdir>/*.log, one stamped
line per message), the structured event list that lands in the session
json, and the stop flag an Abort button sets.
"""

from __future__ import annotations

import contextlib
import os
import sys
import threading

from bfield import clock
from bfield.errors import ConfigError, Stopped


class UI:
    """Base UI: every method optional. A GUI subclasses this."""

    def log(self, stamp, tag, msg, level="info"):
        """One operations-log line. level: info | warn | error."""

    def progress(self, frac, what):
        """frac in 0..1, or None for an indeterminate wait."""

    def confirm(self, question):
        """Yes/no question for the operator; True only on an explicit yes."""
        return False

    def acknowledge(self, message):
        """Tell the operator to do something by hand; return when done."""

    def ask_text(self, prompt):
        """A typed reply ("" = just Enter). A UI without text entry
        treats it as an acknowledgement: Enter, the default action."""
        self.acknowledge(prompt)
        return ""

    def data(self, name, samples, fs):
        """Live samples for plotting (a list/array of floats)."""

    def status(self, kind, data):
        """Structured state for indicators (the log says the same in
        words). kind: "sensors" (the run's sensor list), "stage" (one
        setup stage of one sensor), "frame" (a status frame read from a
        head: LEDs, compensation, cell temperature)."""


class ConsoleUI(UI):
    """The terminal. Questions need a real terminal unless the operator
    pre-confirmed on the command line (assume_yes)."""

    def __init__(self, assume_yes=False, stream=None):
        self.assume_yes = assume_yes
        self.stream = stream
        robust_console()

    def log(self, stamp, tag, msg, level="info"):
        # console: time of day is enough; the log file keeps the date
        out = self.stream or (sys.stderr if level != "info" else sys.stdout)
        print(f"[{stamp[11:]}] [{tag}] {msg}", file=out, flush=True)

    def progress(self, frac, what):
        pass    # the log lines already carry progress in a terminal

    def _interactive(self):
        return sys.stdin.isatty() and sys.stdout.isatty()

    def confirm(self, question):
        if self.assume_yes:
            print(f"{question} [pre-confirmed on the command line]",
                  flush=True)
            return True
        if not self._interactive():
            raise ConfigError(
                "this step needs the operator: run in a terminal, or "
                "pre-confirm on the command line")
        return input(f"{question} [y/N]: ").strip().lower() in ("y", "yes")

    def acknowledge(self, message):
        if not self._interactive():
            raise ConfigError(
                f"this step needs the operator ({message}) -- run in a "
                "terminal")
        input(f"{message}\n  press Enter when done: ")

    def ask_text(self, prompt):
        if not self._interactive():
            raise ConfigError(
                f"this step needs the operator ({prompt.strip()}) -- run in "
                "a terminal")
        return input(prompt)


class Context:
    """What an operation gets besides its devices and settings."""

    def __init__(self, ui=None, config=None, stop_event=None):
        self.ui = ui or UI()
        self.config = config
        self.stop_event = stop_event or threading.Event()
        self.events = []
        self.fh = None
        self.path = None
        self.t0 = clock.monotonic()

    def child(self, stop_event=None):
        """A Context for a sub-session (one relaxation point, one degauss
        pass): same UI, config and stop flag, but its own log file and
        event list -- so the sub-session's ledger gets its own events and
        opening its log does not close the parent's. A fresh stop_event
        lets cleanup (the final standby) run after an Abort."""
        return Context(self.ui, self.config, stop_event or self.stop_event)

    # -- operations log -------------------------------------------------

    def reset_log(self):
        """One run, one log. A fresh list, not clear(): sessions already
        written keep the events they were built with."""
        self.close_log()
        self.events = []
        self.t0 = clock.monotonic()

    def open_log(self, path):
        if self.fh is not None:
            return
        try:
            os.makedirs(os.path.dirname(os.path.abspath(path)), exist_ok=True)
            self.fh = open(path, "a", newline="\n", encoding="utf-8")
        except OSError as e:
            # never fail a measurement over the log file
            self.ui.log(clock.iso_now(), "warn",
                        f"cannot open the operations log {path}: {e}", "warn")
            return
        self.path = path

    def close_log(self):
        if self.fh is not None:
            with contextlib.suppress(Exception):
                self.fh.close()
        self.fh = None

    def write(self, tag, msg, level="info"):
        stamp = clock.iso_now()
        self.ui.log(stamp, tag, msg, level)
        if self.fh is not None:
            with contextlib.suppress(Exception):
                self.fh.write(f"[{stamp}] [{tag}] {msg}\n")
                self.fh.flush()
        self.events.append({"t": stamp,
                            "elapsed_s": round(clock.monotonic() - self.t0, 1),
                            "tag": tag, "msg": msg})

    def say(self, tag, msg):
        self.write(tag, msg)

    def warn(self, msg, sink=None):
        """A warning; `sink` (a list in a record) keeps it with the data."""
        self.write("warn", msg, "warn")
        if sink is not None:
            sink.append(msg)

    def error(self, msg):
        self.write("fatal", msg, "error")

    def progress(self, frac, what):
        self.ui.progress(frac, what)

    def data(self, name, samples, fs):
        self.ui.data(name, samples, fs)

    def status(self, kind, **data):
        try:
            self.ui.status(kind, data)
        except Exception:
            pass        # an indicator must never break a measurement

    # -- the operator ---------------------------------------------------

    def ask(self, question, refused):
        """Yes/no gate: returns on yes, raises ConfigError(refused) else."""
        self.write("ask", question)
        if not self.ui.confirm(question):
            self.write("ask", f"declined -- {refused}")
            raise ConfigError(refused)
        self.write("ask", "confirmed by the operator")

    def ask_text(self, prompt):
        """A typed reply from the operator, stripped ("" = Enter)."""
        self.check_stop()
        self.write("ask", prompt.strip().splitlines()[-1] if prompt.strip()
                   else "(reply)")
        reply = self.ui.ask_text(prompt)
        self.check_stop()
        return (reply or "").strip()

    def pause(self, message):
        """A hand step; returns once the operator says it is done."""
        self.write("hand", message)
        self.ui.acknowledge(message)
        self.write("hand", "operator: done")

    # -- stopping -------------------------------------------------------

    def request_stop(self):
        self.stop_event.set()

    def stop_requested(self):
        return self.stop_event.is_set()

    def check_stop(self):
        """Raise Stopped when the operator pressed Abort."""
        if self.stop_event.is_set():
            raise Stopped("stopped by the operator")

    def sleep(self, seconds, tick=1.0):
        """clock.sleep that an Abort interrupts."""
        deadline = clock.monotonic() + seconds
        while True:
            self.check_stop()
            left = deadline - clock.monotonic()
            if left <= 0:
                return
            clock.wait(self.stop_event, min(tick, left))


def robust_console():
    """Reconfigure stdout/stderr with errors="replace".

    The lab console is Japanese Windows (cp932) and the vendor libraries
    (QuSpin QZFM, Twinleaf tio) print non-ASCII; an unattended
    UnicodeEncodeError out of a print would otherwise end a run.
    """
    for stream in (sys.stdout, sys.stderr):
        with contextlib.suppress(Exception):
            stream.reconfigure(errors="replace")
