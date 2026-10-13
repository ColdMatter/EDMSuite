"""One exception family for the whole package.

Every error carries the exit code the old scripts used for it, so the
run scripts in run/ can keep exiting with the same numbers (analyses
and the run ledgers record them). Only run/*.py turns an exception into
an exit code; everything below raises.

  1   unexpected / crash / link timeout
  10  device not found or would not connect (DAQ, degausser)
  11  DAQ setup (driver, task)
  12  DAQ read
  13  configuration (bad settings, busy or missing port)
  20  stopped early: sensor abort, session stop, operator stop
  30  saturation / QC refused (data kept, walls not trusted)
  31  no signal: flat or pinned channel
  32  bad data (non-finite samples); degausser readback refused
  40  output or meta could not be written
  41  the recorder child survived every stop -- it still holds the DAQ
  130 Ctrl-C / Abort button
"""

from __future__ import annotations


class BFieldError(Exception):
    code = 1


# ----------------------------------------------------------- configuration

class ConfigError(BFieldError):
    code = 13


class PortBusyError(ConfigError):
    """The serial port is held by someone else (QuSpin GUI, DG Control)."""


class PortLostError(ConfigError):
    """The port opened, then the device went away (USB re-enumerated or
    lost power); Windows raises "handle is invalid". Seen 2026-08-05."""


# ------------------------------------------------------------- acquisition

class DaqDeviceNotFound(BFieldError):
    code = 10


class DaqSetupError(BFieldError):
    code = 11


class DaqReadError(BFieldError):
    code = 12


class SaturationError(BFieldError):
    code = 30


class NoSignalError(BFieldError):
    code = 31


class BadDataError(BFieldError):
    code = 32


class OutputIOError(BFieldError):
    code = 40


class RecorderStuckError(BFieldError):
    # a recorder that survives kill still holds the USB-6008; the next
    # run then fails with an unrelated-looking DAQ-busy error
    code = 41


# ------------------------------------------------------------ QZFM sensor

class SensorError(BFieldError):
    """A designed failure talking to a QZFM head."""
    code = 1


class LinkTimeout(SensorError):
    code = 1


class SensorAbort(SensorError):
    code = 20


class SessionStop(SensorError):
    code = 20


# --------------------------------------------------------------- degausser

class DgError(BFieldError):
    code = 1


class DgConnectError(DgError):
    code = 10


class ReadbackError(DgError):
    # the trigger gate refused the waveform readback; nothing was driven
    code = 32


# --------------------------------------------------------------- procedure

class AbortRun(BFieldError):
    code = 20


class Stopped(BFieldError):
    """The operator pressed Abort (GUI) -- the Ctrl-C of a GUI run."""
    code = 130


def error_record(e, code=None):
    """The {code, type, message} dict every ledger stores for an error."""
    return {"code": getattr(e, "code", 1) if code is None else code,
            "type": type(e).__name__, "message": str(e)}


# recorder exit codes, so a ledger and an overnight log say what "exit 11" was
EXIT_MEANING = {
    0: "ok",
    1: "acquisition error",
    10: "DAQ device not found",
    11: "DAQ setup: nidaqmx / driver / device",
    12: "DAQ read",
    13: "child config error -- bad arguments from here",
    30: "saturation: ADC window or sensor output railed",
    31: "no signal: flat or pinned channel",
    32: "bad data: non-finite samples",
    40: "output/meta write failed",
    130: "child interrupted (Ctrl-C)",
}


def exit_meaning(code):
    return EXIT_MEANING.get(code, "unknown recorder code")
