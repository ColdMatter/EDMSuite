"""Deadlines for vendor calls that can block forever.

The QZFM package has no read timeout and tio's RPCs can hang on a
wedged port, so every call runs in a worker thread with a deadline; on
timeout the caller's on_timeout (close the port) unblocks the worker.
"""

from __future__ import annotations

import threading

from bfield import clock
from bfield.errors import LinkTimeout


def run_with_watchdog(fn, timeout, desc, on_timeout=None, heartbeat=None,
                      heartbeat_s=None, warn=None):
    """Run fn in a worker thread with a deadline (clock seconds).

    heartbeat(elapsed, remaining) fires every heartbeat_s. Wall clock
    only: the worker holds the serial lock, so a status read here would
    just queue behind it.
    """
    result, error = [], []

    def work():
        try:
            result.append(fn())
        except BaseException as e:  # re-raised in the caller
            error.append(e)

    worker = threading.Thread(target=work, daemon=True)
    started = clock.monotonic()
    worker.start()
    if heartbeat and heartbeat_s:
        while True:
            clock.join(worker, heartbeat_s)
            if not worker.is_alive():
                break
            elapsed = clock.monotonic() - started
            if elapsed >= timeout:
                break
            heartbeat(elapsed, timeout - elapsed)
    else:
        clock.join(worker, timeout)
    if worker.is_alive():
        extra = ""
        if on_timeout is not None:
            try:
                on_timeout()
            except Exception as e:
                # whether the port closed decides whether a retry can
                # work at all, so it goes in the message
                extra = (f"; the emergency close ALSO failed "
                         f"({type(e).__name__}: {e}) -- the port is stuck, "
                         "unplug the sensor USB before retrying")
                if warn is not None:
                    warn(f"emergency close after '{desc}' failed: "
                         f"{type(e).__name__}: {e}")
        raise LinkTimeout(f"{desc} did not finish within {timeout:g} s{extra}")
    if error:
        raise error[0]
    return result[0] if result else None
