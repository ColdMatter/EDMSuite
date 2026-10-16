"""The package's one source of time.

Hardware runs use real time (speed 1). The simulated lab can run faster
so an overnight relaxation series finishes in minutes: every sleep,
deadline and timestamp in the package goes through here, so they all
speed up together. Speed != 1 is refused unless the simulated backend
is selected (bfield.lab.open_lab enforces that).
"""

from __future__ import annotations

import threading
import time as _time
from datetime import datetime, timedelta

_state = {"speed": 1.0, "real0": _time.monotonic(), "sim0": _time.monotonic(),
          "wall0": _time.time()}
_lock = threading.Lock()


def set_speed(speed, wall=None):
    """Run the clock `speed` times faster than real time (sim only).

    `wall` re-anchors the clock's wall time (a recorder child takes its
    parent's accelerated time, so both stamp the same timeline).
    """
    if not (speed > 0):
        raise ValueError("clock speed must be > 0")
    with _lock:
        sim0, wall0 = monotonic(), time()
        _state["real0"] = _time.monotonic()
        _state["sim0"] = sim0
        _state["wall0"] = wall0 if wall is None else float(wall)
        _state["speed"] = float(speed)


def speed():
    return _state["speed"]


def monotonic():
    s = _state
    return s["sim0"] + (_time.monotonic() - s["real0"]) * s["speed"]


def time():
    s = _state
    return s["wall0"] + (_time.monotonic() - s["real0"]) * s["speed"]


def now():
    """datetime of the (possibly accelerated) clock."""
    return datetime.fromtimestamp(time())


def iso_now():
    return now().isoformat(timespec="seconds")


def real_seconds(sim_seconds):
    """How long a wait of `sim_seconds` really takes."""
    if sim_seconds is None:
        return None
    return max(0.0, sim_seconds / _state["speed"])


def sleep(seconds):
    _time.sleep(real_seconds(seconds))


def wait(event, seconds):
    """threading.Event.wait in clock seconds."""
    return event.wait(real_seconds(seconds))


def join(thread, seconds=None):
    thread.join(real_seconds(seconds))


def strftime(fmt):
    return now().strftime(fmt)


def plus(dt, seconds):
    return dt + timedelta(seconds=seconds)
