"""Tektronix TBS 1052B-EDU oscilloscope over USBTMC (pyvisa).

Moved from IROP_research/scripts/sf_sweep.py (class Scope). The scope
sits on the monitor resistor in series with the S(f) drive coil and is
read with MEASUrement:IMMed, so the drive current is recorded, not typed.

Read-only on purpose: never changes timebase, scale or trigger. The
operator sets the scope; reconfiguring the instrument here would be
adjusting the measurement being made.

Manual gotchas: a failed measurement returns 9.9E37, not an error; a
finite value can still carry an *ESR? warning ("Low signal amplitude").

`backend` is a callable returning a pyvisa-like ResourceManager: the
real pyvisa.ResourceManager on the bench, the simulated lab's
bfield.devices.simulated_scope.FakeScopeBackend otherwise.
"""

from __future__ import annotations

import contextlib
import math

from bfield.errors import BFieldError

SCOPE_BAD = 9.0e37          # >= this is the not-available sentinel
SCOPE_TIMEOUT_MS = 10_000   # a 0.1 Hz cycle-RMS needs a 10 s window on screen
DEFAULT_SCOPE_CH = "CH1"


class ScopeError(BFieldError):
    """The scope was asked for a number and did not give a usable one."""
    code = 21


def real_visa_backend():
    import pyvisa                       # lab-only dependency
    return pyvisa.ResourceManager()


class TBS1052B:
    """Read-only USBTMC link to the monitor-resistor channel."""

    def __init__(self, ctx, resource=None, channel=DEFAULT_SCOPE_CH,
                 backend=None):
        self.ctx = ctx
        self.channel = channel or DEFAULT_SCOPE_CH
        rm = (backend or real_visa_backend)()
        if resource is None:
            # VISA lists every COM port as ASRL (QZFM heads, DG); the
            # scope is on the rear USB port, so only USB:: qualifies
            every = list(rm.list_resources())
            found = [r for r in every
                     if r.upper().startswith("USB") and "::INSTR" in r]
            if not found:
                serial_note = (f" ({len(every)} serial port(s) seen -- "
                               f"QZFM and DG links, not the scope)"
                               if every else "")
                raise ScopeError(
                    f"no USB instrument found{serial_note}. pyvisa needs "
                    f"a backend (NI-VISA, installed with NI-DAQmx, or "
                    f"TekVISA) to see anything. Check the rear USB "
                    f"Device port, then run: python -c "
                    f"\"import pyvisa; print(pyvisa.ResourceManager()"
                    f".list_resources())\"")
            if len(found) > 1:
                raise ScopeError(
                    f"{len(found)} USB instruments are connected "
                    f"({', '.join(found)}) -- name the scope with "
                    f"--scope-resource or [scope] resource in the config")
            resource = found[0]
        self.resource = resource
        self.dev = rm.open_resource(resource)
        self.dev.timeout = SCOPE_TIMEOUT_MS
        self.idn = self.dev.query("*IDN?").strip()
        # bare numbers, not ":MEASUREMENT:IMMED:VALUE 2.46E0"
        self.dev.write("HEADer OFF")
        # a stray LOCk ALL has no front-panel undo and the operator
        # needs the knobs; one defensive unlock costs nothing
        self.dev.write("UNLock ALL")
        self.dev.write(f"MEASUrement:IMMed:SOUrce {self.channel}")
        self.dev.query("*ESR?")             # clear anything already pending
        self._cmean = None                  # probed on first read_drive

    def close(self):
        with contextlib.suppress(Exception):
            self.dev.close()

    def _immediate(self, kind, want_unit):
        self.dev.write(f"MEASUrement:IMMed:TYPe {kind}")
        raw = self.dev.query("MEASUrement:IMMed:VALue?").strip()
        unit = self.dev.query("MEASUrement:IMMed:UNIts?").strip().strip('"')
        esr = self.dev.query("*ESR?").strip()
        try:
            value = float(raw)
        except ValueError:
            raise ScopeError(f"{kind} on {self.channel} came back as "
                             f"{raw!r}, which is not a number") from None
        if not math.isfinite(value) or abs(value) >= SCOPE_BAD:
            events = "?"
            with contextlib.suppress(Exception):
                events = self.dev.query("ALLEv?").strip()
            raise ScopeError(
                f"{kind} on {self.channel} is not available "
                f"({raw}): {events}. A cycle measurement -- so the drive "
                f"is off, the channel is not displayed, or under one "
                f"whole cycle is on screen (0.1 Hz needs about 2 s/div)")
        if unit != want_unit:
            raise ScopeError(f"{kind} on {self.channel} came back in "
                             f"{unit!r}, expected {want_unit!r} -- is the "
                             f"probe set to a current or math source?")
        return value, esr

    def _cycle_mean_type(self):
        """CMEan if this scope has it, else MEAN -- probed once.

        Full-record MEAN over a non-integer cycle count reads up to
        ~13% of the amplitude as fake DC (trigger phase); CMEan gates
        whole cycles. A scope without CMEan silently keeps its old
        type on the write, hence the echo-back probe.
        """
        if self._cmean is None:
            self.dev.write("MEASUrement:IMMed:TYPe CMEan")
            echo = self.dev.query("MEASUrement:IMMed:TYPe?").strip()
            self._cmean = echo.strip('"').upper().startswith("CME")
            self.dev.query("*ESR?")     # clear anything the probe raised
        return "CMEan" if self._cmean else "MEAN"

    def read_dc(self, rmon_ohm):
        """(current mA, notes) for a steady level -- MEAN, nothing else.

        No cycles on screen at DC, so CRMs/FREQuency/CMEan cannot work
        and MEAN is exact. MEAN is signed, so the polarity is checked
        rather than believed.
        """
        mean, esr = self._immediate("MEAN", "V")
        note = ""
        if esr not in ("0", ""):
            with contextlib.suppress(Exception):
                note = self.dev.query("ALLEv?").strip()
        return mean / rmon_ohm * 1e3, note

    def read_drive(self, rmon_ohm):
        """(frequency Hz, current mA 0-p, DC offset mA, notes).

        Cycle RMS, not peak-to-peak: p-p carries the noise excursion.
        CRMs includes any DC, so the line amplitude is
        Vp = sqrt(2 (V_crms^2 - V_mean^2)) -- the lock-in sees only the
        line. Offset uses CMEan where available (see _cycle_mean_type).
        """
        vrms, esr_v = self._immediate("CRMs", "V")
        freq, esr_f = self._immediate("FREQuency", "Hz")
        mtype = self._cycle_mean_type()
        mean, esr_m = self._immediate(mtype, "V")
        if vrms <= 0:
            raise ScopeError(f"cycle RMS on {self.channel} is {vrms:g} V")
        # clamped at zero: |MEAN| >= V_crms is all offset and no line;
        # check_amp() then reports a missing drive, not a small one
        vp = math.sqrt(max(2.0 * (vrms ** 2 - mean ** 2), 0.0))
        i_ma = vp / rmon_ohm * 1e3
        warned = [e for e in (esr_v, esr_f, esr_m) if e not in ("0", "")]
        note = ""
        if warned:
            with contextlib.suppress(Exception):
                note = self.dev.query("ALLEv?").strip()
        if mtype == "MEAN":
            note = (note + "; " if note else "") + (
                "offset via full-record MEAN (no CMEan): up to ~13% of "
                "the amplitude can be trigger phase, not DC")
        return freq, i_ma, mean / rmon_ohm * 1e3, note
