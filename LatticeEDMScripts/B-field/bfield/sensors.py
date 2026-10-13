"""The sensor heads: numbered, labelled, located -- and which ones a run uses.

config/lab.toml lists every physical QZFM head once:

    [[sensors]]
    number = 1                 # stays with the head, wherever it sits
    label = "Z3D0-AB3Q"        # the QuSpin serial printed on the head
    usb_serial = "A10QHMQMA"   # what the PC sees on the COM port (FTDI)
    location = ""              # where it is in the experiment

The list lives in config/sensors.toml (the GUI's "Manage sensors"
dialog rewrites it; it takes precedence over any [[sensors]] left in
lab.toml).

A run picks some of them (``--sensors 1,2`` / the GUI checklist). The
DAQ has a fixed number of input SLOTS ([daq] slots, two on the
USB-6008: ai0/ai1 and ai2/ai3); the chosen sensors fill the slots in
the order given, and the program says which sensor must be plugged into
which inputs. With two sensors one is the modulation master: the first
by default, or the one chosen (``--master 2``) -- that only decides which
head gets the master command, not where it plugs in.

Inside the program the slots are still called heads "a" and "b" -- that
is what the data files (house_a.csv, meta.json "heads"/"head_ids") and
the analysis scripts use -- and each run records the full mapping
(number, label, USB serial, location, inputs) in its session json and in
every meta.json ("sensor_info"), so a run says which physical head
produced which column.
"""

from __future__ import annotations

from dataclasses import dataclass

import os
import re

from bfield.constants import HEADS
from bfield.errors import ConfigError
from bfield.heads import FTDI_SERIAL_RX

# internal slot labels, in slot order (what the data files call them)
SLOT_HEADS = HEADS
DEFAULT_SLOTS = (("ai0", "ai1"), ("ai2", "ai3"))


@dataclass(frozen=True)
class Sensor:
    number: int
    label: str          # QuSpin serial, printed on the head
    usb_serial: str     # FTDI serial, what the COM-port match uses
    location: str = ""

    @property
    def display(self):
        """'sensor 1 (Z3D0-AB3Q)' or 'sensor 1 (Z3D0-AB3Q, north end)'."""
        extra = f", {self.location}" if self.location else ""
        return f"sensor {self.number} ({self.label}{extra})"

    @property
    def long(self):
        return (f"sensor {self.number}: {self.label} (USB {self.usb_serial})"
                + (f" at {self.location}" if self.location else
                   " -- location not set"))


def load_sensors(config):
    """Every configured sensor, by number. Refuses duplicates."""
    rows = config.get("sensors") if hasattr(config, "get") else None
    if not rows:
        # an old config: [qzfm.heads] a/b + [qzfm.head_aliases]
        q = config.section("qzfm") if hasattr(config, "section") else {}
        heads = q.get("heads") or {}
        aliases = q.get("head_aliases") or {}
        rows = [{"number": i + 1, "usb_serial": heads[h],
                 "label": aliases.get(heads[h], heads[h])}
                for i, h in enumerate(sorted(heads))]
    out, seen = [], {}
    for r in rows:
        try:
            s = Sensor(int(r["number"]), str(r["label"]).strip(),
                       str(r["usb_serial"]).strip(),
                       str(r.get("location", "") or "").strip())
        except (KeyError, TypeError, ValueError) as e:
            raise ConfigError(f"bad [[sensors]] entry {r!r} in the config "
                              f"({type(e).__name__}: {e}) -- each needs "
                              "number, label and usb_serial")
        for key, val in (("number", s.number), ("label", s.label),
                         ("usb_serial", s.usb_serial)):
            if (key, val) in seen:
                raise ConfigError(f"two [[sensors]] entries share {key} "
                                  f"{val!r}")
            seen[(key, val)] = s
        out.append(s)
    return sorted(out, key=lambda s: s.number)


def aliases(config):
    """{usb_serial: label}: both ids of a head count as one sensor."""
    return {s.usb_serial: s.label for s in load_sensors(config)}


def slots(config):
    """The DAQ input slots, [(z_input, y_input), ...]."""
    raw = (config.section("daq").get("slots")
           if hasattr(config, "section") else None) or DEFAULT_SLOTS
    out = [tuple(str(p).lower() for p in pair) for pair in raw]
    if len(out) > len(SLOT_HEADS):
        raise ConfigError(f"[daq] slots lists {len(out)} slots; the "
                          f"recorder handles at most {len(SLOT_HEADS)} "
                          "dual-axis sensors per run")
    return out


def default_map(config):
    """The two-slot map with no particular sensors: z/y on slot 1,
    z2/y2 on slot 2."""
    sl = slots(config)
    parts = []
    for i, head in enumerate(SLOT_HEADS[:len(sl)]):
        suffix = "" if i == 0 else str(i + 1)
        parts += [f"z{suffix}:{head}:{sl[i][0]}", f"y{suffix}:{head}:{sl[i][1]}"]
    return ",".join(parts)


def display_of(rec):
    """How messages name a sensor from its info record."""
    if rec.get("number") is not None:
        loc = rec.get("location")
        return (f"sensor {rec['number']} ({rec.get('label')}"
                + (f", {loc}" if loc else "") + ")")
    return f"head {rec.get('head')} ({rec.get('usb_serial')})"


def info_for_serials(config, serials, pins):
    """Info records when serials were given directly (--device-name):
    {head: serial}, {head: {"z": aiN, "y": aiN}} -> [record]."""
    by_id = {}
    for s in load_sensors(config):
        by_id[s.usb_serial] = s
        by_id[s.label] = s
    out = []
    for head, serial in serials.items():
        s = by_id.get(serial)
        out.append({"head": head, "number": s.number if s else None,
                    "label": s.label if s else None,
                    "usb_serial": s.usb_serial if s else serial,
                    "location": s.location if s else "",
                    "inputs": pins.get(head, {}), "modulation": None})
    return out


def parse_numbers(text):
    """'1,2' -> [1, 2]."""
    try:
        nums = [int(t) for t in str(text).replace(" ", "").split(",") if t]
    except ValueError:
        raise ConfigError(f"sensors {text!r}: give numbers, e.g. 1,2")
    if not nums:
        raise ConfigError("no sensor chosen")
    if len(set(nums)) != len(nums):
        raise ConfigError(f"sensors {text!r} names a sensor twice")
    return nums


def check_sensor(s, others=()):
    """Refusals for a new or edited sensor; returns warnings (soft)."""
    if not s.label:
        raise ConfigError("the sensor head ID (QuSpin label) is required")
    if not s.usb_serial:
        raise ConfigError("the USB ID is required (Scan USB finds it)")
    if s.number < 1:
        raise ConfigError("sensor numbers start at 1")
    for o in others:
        for key in ("number", "label", "usb_serial"):
            if getattr(o, key) == getattr(s, key):
                raise ConfigError(f"{o.display} already has {key} "
                                  f"{getattr(s, key)!r}")
    warnings = []
    if not s.label.upper().startswith("Z3D0"):
        warnings.append(f"head ID {s.label!r} does not look like a QuSpin "
                        "label (Z3D0-...)")
    if not FTDI_SERIAL_RX.match(s.usb_serial):
        warnings.append(f"USB ID {s.usb_serial!r} does not look like an "
                        "FTDI serial (8-10 capitals and digits, e.g. "
                        "A10QHMQMA)")
    return warnings


def _toml_str(text):
    """A TOML basic string."""
    out = str(text).replace("\\", "\\\\").replace('"', '\\"')
    out = re.sub(r"[\x00-\x1f]", " ", out)
    return f'"{out}"'


def save_sensors(path, sensors):
    """Rewrite config/sensors.toml with this list (atomic)."""
    from bfield.io.files import write_text_atomic
    lines = ["# The sensor heads. Written by the GUI (Manage sensors); hand",
             "# edits are fine too. One block per physical QZFM head:",
             "#   number      stays with the head wherever it is mounted",
             "#   label       the QuSpin head ID printed on it (Z3D0-...)",
             "#   usb_serial  the USB ID the PC sees on its COM port (FTDI)",
             "#   location    free note: where it is in the experiment",
             ""]
    for s in sorted(sensors, key=lambda x: x.number):
        lines += ["[[sensors]]",
                  f"number = {int(s.number)}",
                  f"label = {_toml_str(s.label)}",
                  f"usb_serial = {_toml_str(s.usb_serial)}",
                  f"location = {_toml_str(s.location)}",
                  ""]
    os.makedirs(os.path.dirname(os.path.abspath(path)), exist_ok=True)
    write_text_atomic(path, "\n".join(lines))


class Selection:
    """The sensors chosen for one run, placed into the DAQ slots."""

    def __init__(self, config, numbers, master=None):
        known = {s.number: s for s in load_sensors(config)}
        missing = [n for n in numbers if n not in known]
        if missing:
            raise ConfigError(
                f"sensor(s) {missing} not in the config; configured: "
                + ", ".join(str(n) for n in known))
        self.sensors = [known[n] for n in numbers]
        self.slots = slots(config)
        if len(self.sensors) > len(self.slots):
            raise ConfigError(
                f"{len(self.sensors)} sensors chosen, but the DAQ has "
                f"{len(self.slots)} input slots ({self._slot_words()}) -- "
                f"at most {len(self.slots)} dual-axis sensors per run with "
                "this DAQ")
        self.heads = dict(zip(SLOT_HEADS, self.sensors))
        # the modulation master (two sensors): its slot head
        self.master_head = None
        if len(self.sensors) > 1:
            if master is None:
                master = self.sensors[0].number
            if master not in self.numbers:
                raise ConfigError(f"master sensor {master} is not one of "
                                  f"the chosen sensors {self.numbers}")
            self.master_head = next(h for h, s in self.heads.items()
                                    if s.number == master)

    def _slot_words(self):
        return ", ".join("/".join(p) for p in self.slots)

    @property
    def numbers(self):
        return [s.number for s in self.sensors]

    @property
    def device_name(self):
        """The serials in the form the sensor session takes."""
        if len(self.sensors) == 1:
            return self.sensors[0].usb_serial
        return ",".join(f"{h}:{s.usb_serial}" for h, s in self.heads.items())

    @property
    def map(self):
        """The recorder channel map: z/y for slot 1, z2/y2 for slot 2."""
        parts = []
        for i, (head, _s) in enumerate(self.heads.items()):
            zin, yin = self.slots[i]
            suffix = "" if i == 0 else str(i + 1)
            parts += [f"z{suffix}:{head}:{zin}", f"y{suffix}:{head}:{yin}"]
        return ",".join(parts)

    def info(self):
        """Per-sensor records for the session json and meta.json."""
        out = []
        for i, (head, s) in enumerate(self.heads.items()):
            zin, yin = self.slots[i]
            out.append({"head": head, "number": s.number, "label": s.label,
                        "usb_serial": s.usb_serial, "location": s.location,
                        "inputs": {"z": zin, "y": yin},
                        "modulation": (None if self.master_head is None
                                       else "master" if head ==
                                       self.master_head else "slave")})
        return out

    def plug_lines(self):
        """What has to be plugged where, one line per sensor."""
        lines = []
        for i, (head, s) in enumerate(self.heads.items()):
            zin, yin = self.slots[i]
            role = (" -- modulation master"
                    if head == self.master_head else "")
            lines.append(f"{s.display} -> DAQ {zin} (Z), {yin} (Y){role}")
        return lines
