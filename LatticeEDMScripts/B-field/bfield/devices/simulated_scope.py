"""The simulated lab's oscilloscope: a fake pyvisa ResourceManager.

Answers the SCPI queries bfield.devices.scope.TBS1052B sends, from the
world's "coil" block (World.set_coil): the drive the operator "set" on
the generator is what the scope reads back across the monitor resistor.
With no drive, cycle measurements return the 9.9E37 not-available
sentinel, exactly like the real instrument.

Fault injection (world "inject"):
  scope_freq_factor  f   the FREQuency reading is f x the drive
                         (the stale-timebase misread of 2026-08-20)
  scope_no_cmean     true  the scope ignores TYPe CMEan (old firmware)
"""

from __future__ import annotations

import math

SIM_RESOURCE = "USB0::0x0699::0x0368::SIM0001::INSTR"
NOT_AVAILABLE = "9.9E37"


class FakeScopeResource:
    def __init__(self, world):
        self.world = world
        self.timeout = None
        self.type = "CRMS"
        self.source = "CH1"
        self.closed = False

    def write(self, cmd):
        c = cmd.strip()
        up = c.upper()
        if up.startswith("MEASUREMENT:IMMED:TYPE ") or \
                up.startswith("MEASU:IMM:TYP "):
            want = c.split()[-1].upper()
            inject = self.world.read().get("inject", {})
            if want == "CMEAN" and inject.get("scope_no_cmean"):
                return          # an old scope keeps its previous type
            self.type = want
        elif up.startswith("MEASUREMENT:IMMED:SOURCE "):
            self.source = c.split()[-1].upper()
        return len(cmd)

    def _coil(self):
        return self.world.read().get("coil") or {}

    def _value(self):
        coil = self._coil()
        rmon = float(coil.get("rmon_ohm", 0.3))
        amp = float(coil.get("amp_mA") or 0.0) * 1e-3 * rmon     # V 0-p
        dc = float(coil.get("dc_mA") or 0.0) * 1e-3 * rmon       # V
        f = coil.get("freq_hz")
        cycling = bool(f) and amp > 0
        t = self.type
        if t == "MEAN":
            return f"{dc:.6E}", "V"
        if not cycling:
            return NOT_AVAILABLE, ("Hz" if t == "FREQUENCY" else "V")
        if t == "CRMS":
            return f"{math.sqrt(amp * amp / 2.0 + dc * dc):.6E}", "V"
        if t == "FREQUENCY":
            k = float(self.world.read().get("inject", {})
                      .get("scope_freq_factor", 1.0))
            return f"{float(f) * k:.6E}", "Hz"
        if t == "CMEAN":
            return f"{dc:.6E}", "V"
        return NOT_AVAILABLE, "V"

    def query(self, cmd):
        up = cmd.strip().upper()
        if up == "*IDN?":
            return "TEKTRONIX,TBS 1052B-EDU,SIM0001,CF:91.1CT FV:v2.52 (sim)\n"
        if up == "*ESR?":
            return "0\n"
        if up == "ALLEV?":
            return '0,"No events to report - queue empty"\n'
        if up == "MEASUREMENT:IMMED:TYPE?":
            return f"{self.type}\n"
        if up == "MEASUREMENT:IMMED:VALUE?":
            return self._value()[0] + "\n"
        if up == "MEASUREMENT:IMMED:UNITS?":
            return f'"{self._value()[1]}"\n'
        return "0\n"

    def close(self):
        self.closed = True


class FakeResourceManager:
    def __init__(self, world):
        self.world = world

    def list_resources(self):
        # VISA lists the COM ports too (QZFM heads, DG)
        return ("ASRL3::INSTR", "ASRL4::INSTR", "ASRL5::INSTR", SIM_RESOURCE)

    def open_resource(self, resource):
        if resource != SIM_RESOURCE:
            raise OSError(f"VI_ERROR_RSRC_NFOUND: {resource}")
        return FakeScopeResource(self.world)


class FakeScopeBackend:
    """Callable backend for TBS1052B(backend=...)."""

    def __init__(self, world):
        self.world = world

    def __call__(self):
        return FakeResourceManager(self.world)
