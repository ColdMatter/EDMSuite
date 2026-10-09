"""Starting and stopping the recorder child process.

Moved from qzfm_common.py (stop_child_ladder, new_process_group_kwargs).
Gentlest first: stop file -> signal -> terminate -> kill.
"""

from __future__ import annotations

import contextlib
import os
import signal
import subprocess
import sys

from bfield import clock
from bfield.config import PROJECT_DIR

RECORDER_MODULE = "bfield.acquisition.recorder"


def recorder_argv():
    """How to start the recorder: this interpreter, the package module."""
    return [sys.executable, "-m", RECORDER_MODULE]


def new_process_group_kwargs():
    """Popen kwargs: own process group, PYTHONIOENCODING=utf-8:replace,
    and the package on PYTHONPATH.

    On Windows CTRL_BREAK_EVENT can only target a process group, and a
    child sharing the console group would drag the parent down. The
    encoding protects the child's interpreter startup on the cp932 lab
    console; setdefault, so an operator's own value wins.
    """
    env = dict(os.environ)
    env.setdefault("PYTHONIOENCODING", "utf-8:replace")
    path = env.get("PYTHONPATH", "")
    if PROJECT_DIR not in path.split(os.pathsep):
        env["PYTHONPATH"] = (PROJECT_DIR + (os.pathsep + path if path
                                            else ""))
    kw = {"env": env}
    if os.name == "nt":
        kw["creationflags"] = subprocess.CREATE_NEW_PROCESS_GROUP
    return kw


def stop_child_ladder(proc, *, stop_file=None, stop_file_wait_s=15.0,
                      signal_wait_s=15.0, term_wait_s=10.0,
                      kill_wait_s=5.0, allow_signal=True, warn=None):
    """Stop one child, gentlest rung first; returns how it ended:

      "dead"           already exited before we did anything
      "stopped-file"   exited after the stop file (its cleanup ran)
      "stopped-signal" exited after SIGINT/CTRL_BREAK (its cleanup ran)
      "forced"         terminate/kill -- cleanup did NOT run; treat the
                       instrument state as unknown
      "alive"          still running, still holds its hardware

    Waits are real seconds (they bound a process, not an experiment).
    """
    def say(msg):
        if warn is not None:
            warn(msg)

    if proc.poll() is not None:
        return "dead"
    try:
        if stop_file:
            try:
                with open(stop_file, "w", newline="\n",
                          encoding="utf-8") as fh:
                    fh.write(clock.strftime("%Y-%m-%dT%H:%M:%S") + "\n")
                proc.wait(timeout=stop_file_wait_s)
                return "stopped-file"
            except subprocess.TimeoutExpired:
                say(f"the child did not stop within {stop_file_wait_s:g} s "
                    "of the stop file -- escalating")
            except Exception as e:
                say(f"could not write the stop file {stop_file} "
                    f"({type(e).__name__}: {e}) -- falling back to a signal")
        if allow_signal:
            try:
                if os.name == "nt":
                    proc.send_signal(signal.CTRL_BREAK_EVENT)
                else:
                    proc.send_signal(signal.SIGINT)
                proc.wait(timeout=signal_wait_s)
                return "stopped-signal"
            except Exception as e:
                say(f"graceful stop failed ({type(e).__name__}: {e}) -- "
                    "escalating; the child's cleanup will not run")
    except KeyboardInterrupt:
        # Ctrl-C during a polite wait must not abandon the ladder:
        # force the child down first, then re-raise.
        say("interrupted mid-stop -- forcing the child down first")
        _force_child(proc, term_wait_s, kill_wait_s)
        raise
    ki = _force_child(proc, term_wait_s, kill_wait_s)
    how = "forced" if proc.poll() is not None else "alive"
    if ki is not None:
        raise ki
    return how


def _force_child(proc, term_wait_s, kill_wait_s):
    """terminate, then kill. Returns a KeyboardInterrupt caught during
    the waits so the caller can re-raise after the child is down."""
    ki = None
    for act, wait_s in ((proc.terminate, term_wait_s),
                        (proc.kill, kill_wait_s)):
        if proc.poll() is not None:
            break
        with contextlib.suppress(Exception):
            act()
        try:
            proc.wait(timeout=wait_s)
        except KeyboardInterrupt as e:
            ki = e
        except Exception:
            pass
    return ki


def stopped_clean(how):
    """True when the ladder did not force the child down."""
    return how in ("dead", "stopped-file", "stopped-signal")
