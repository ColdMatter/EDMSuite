"""The auto-operator: plays the person at the bench in the simulator.

With it switched on, a simulated run needs no clicks: every yes/no is
"yes", every hand step is done at once, and where a real operator would
set the signal generator or throw the DC switch, this sets the simulated
coil drive in the world file -- so the recorded data really contains
the drive. Everything it does goes to the log, tagged "auto".

It never exists outside the simulated lab (the window refuses to build
one otherwise).
"""

from __future__ import annotations

import re


class SimOperator:
    def __init__(self, world, say, sweep=None):
        """sweep: the SweepSettings of an S(f) run (amplitude, control
        frequency, orientation, DC current), or None."""
        self.world = world
        self.say = say
        self.sweep = sweep

    def _note(self, msg):
        self.say("auto", msg)

    def confirm(self, question):
        self._note(f"yes -- {question}")
        return True

    def acknowledge(self, message):
        self._note(f"done -- {message}")

    def ask_text(self, prompt):
        s = self.sweep
        amp = getattr(s, "amp_ma", 450.0)
        orient = getattr(s, "orientation", "axial")
        m = re.search(r"point \d+/\d+.*?: ([0-9.]+) Hz for", prompt)
        if m:
            f = float(m.group(1))
            self.world.set_coil(freq_hz=f, amp_mA=amp, orientation=orient,
                                rmon_ohm=getattr(s, "rmon_ohm", 0.3))
            self._note(f"generator set to {f:g} Hz, {amp:g} mA")
            return ""
        if "drive off?" in prompt:
            self.world.clear_coil()
            self._note("drive off")
            return ""
        if "generator running" in prompt:
            f = getattr(s, "control_hz", 5.0)
            self.world.set_coil(freq_hz=f, amp_mA=amp, orientation=orient,
                                rmon_ohm=getattr(s, "rmon_ohm", 0.3))
            self._note(f"generator set to {f:g} Hz, {amp:g} mA (precheck)")
            return ""
        m = re.search(r"switch thrown to ([+-])\?", prompt)
        if m:
            sign = 1.0 if m.group(1) == "+" else -1.0
            dc = getattr(s, "dc_ma", 1000.0)
            self.world.set_coil(freq_hz=None, dc_mA=sign * dc,
                                orientation=orient)
            self._note(f"DC switch thrown to {m.group(1)} ({sign * dc:g} mA)")
            return ""
        if "offset down" in prompt:
            self._note("recording with the offset as it is")
            return "go"
        self._note("Enter")
        return ""
