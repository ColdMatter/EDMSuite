"""The sensor heads: numbered, labelled, located -- and which ones a run uses.

config/lab.toml lists every physical QZFM head once:

    [[sensors]]
    number = 1                 # stays with the head, wherever it sits
    label = "Z3D0-AB3Q"        # the QuSpin serial printed on the head
    usb_serial = "A10QHMQMA"   # what the PC sees on the COM port (FTDI)
    location = ""              # where it is in the experiment

The list lives in config/sensors.toml (the GUI's "Manage sensors"
dialog rewrites it; it takes precedence over any [[sensors]] left in
lab.toml). Each sensor also says which DAQ inputs its outputs are wired
to -- a QZFM records up to two axes at once, Z and Y:

    z_input = "ai0"            # its Z output on ai0 ("" = Z not recorded)
    y_input = "ai1"            # its Y output on ai1 ("" = Y not recorded)

A run picks some of them (``--sensors 1,2`` / the GUI checklist) and the
axes to record (``--axes``; default Z only -- single-axis mode, 1.35x
quieter -- for one sensor, Z and Y for two); each sensor records those
of them it is wired for, on its own inputs. Two sensors in the
list may share an input (the cable is moved between runs) -- they just
cannot be chosen together. With two sensors one is the modulation
master: the first by default, or the one chosen (``--master 2``).

Everything is named by the sensor number (bfield.channels): sensor 2 is
"s2" in the session json and house_S2.csv, its Z channel "s2z" in
meta.json and R01_1631_S2Z_pT.txt. Each run also records the full
mapping (number, label, USB serial, location, inputs) in its session
json and in every meta.json ("sensor_info"). A run takes as many
sensors as the DAQ has inputs for (eight on the PCIe-6321, four on the
USB-6008: one axis each, or half as many on both).
"""

from __future__ import annotations

from dataclasses import dataclass

import os
import re

from bfield.channels import channel_name, key_number, sensor_key
from bfield.daq_models import model_of
from bfield.errors import ConfigError
from bfield.heads import FTDI_SERIAL_RX, same_head

DEFAULT_SLOTS = (("ai0", "ai1"), ("ai2", "ai3"))


AXES = ("z", "y")                 # what a dual-axis QZFM can record
# the DAQ inputs (and so the sensors per run: each needs at least one
# input) are the card's -- bfield.daq_models, [daq] model


@dataclass(frozen=True)
class Sensor:
    number: int
    label: str          # QuSpin serial, printed on the head
    usb_serial: str     # FTDI serial, what the COM-port match uses
    location: str = ""
    z_input: str = ""   # DAQ input its Z output is wired to ("" = none)
    y_input: str = ""   # DAQ input its Y output is wired to ("" = none)

    @property
    def inputs(self):
        """{axis: input} for the axes this sensor records, Z first."""
        return {ax: pin for ax, pin in (("z", self.z_input),
                                        ("y", self.y_input)) if pin}

    @property
    def wiring(self):
        """'Z->ai0, Y->ai1', or 'no DAQ inputs set'."""
        return (", ".join(f"{a.upper()}->{p}" for a, p in self.inputs.items())
                or "no DAQ inputs set")

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
        sl = slots(config)
        rows = [{"number": i + 1, "usb_serial": heads[h],
                 "label": aliases.get(heads[h], heads[h]),
                 "z_input": sl[i][0] if i < len(sl) else "",
                 "y_input": sl[i][1] if i < len(sl) else ""}
                for i, h in enumerate(sorted(heads))]
    inputs = model_of(config).inputs
    out, seen = [], {}
    for r in rows:
        try:
            s = Sensor(int(r["number"]), str(r["label"]).strip(),
                       str(r["usb_serial"]).strip(),
                       str(r.get("location", "") or "").strip(),
                       _pin(r.get("z_input"), inputs),
                       _pin(r.get("y_input"), inputs))
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


def _pin(value, inputs):
    pin = str(value or "").strip().lower()
    if pin and pin not in inputs:
        raise ConfigError(f"DAQ input {value!r} is not one of "
                          f"{', '.join(inputs)} (or empty)")
    return pin


def aliases(config):
    """{usb_serial: label}: both ids of a head count as one sensor."""
    return {s.usb_serial: s.label for s in load_sensors(config)}


def slots(config):
    """The DAQ input slots, [(z_input, y_input), ...]."""
    raw = (config.section("daq").get("slots")
           if hasattr(config, "section") else None) or DEFAULT_SLOTS
    return [tuple(str(p).lower() for p in pair) for pair in raw]


def default_map(config):
    """The old slot wiring as a map for sensors 1 and 2 (for a sensor
    list with no inputs set)."""
    parts = []
    for i, pair in enumerate(slots(config)):
        for axis, pin in zip(AXES, pair):
            parts.append(f"{channel_name(i + 1, axis)}:{pin}")
    return ",".join(parts)


def display_of(rec):
    """How messages name a sensor from its info record."""
    if rec.get("number") is not None:
        loc = rec.get("location")
        return (f"sensor {rec['number']} ({rec.get('label')}"
                + (f", {loc}" if loc else "") + ")")
    return f"sensor ? ({rec.get('usb_serial')})"


def resolve_device_name(config, spec):
    """Sensors named by serial -> their numbers, in the order given.

    Takes 'A10QHMQMA' (USB ID or QuSpin label), a comma list of them,
    's2:A10QHJ1ZA' (key and serial), or the old 'a:SER,b:SER'. Every
    serial must be in the sensor list: data are filed under the sensor
    number, so an unlisted head has nowhere to go."""
    known = load_sensors(config)
    numbers = []
    for item in (p.strip() for p in str(spec or "").split(",")):
        if not item:
            raise ConfigError(f"device name {spec!r} has an empty entry")
        prefix, sep, serial = item.rpartition(":")
        serial = serial.strip()
        hit = next((s for s in known
                    if same_head(serial, s.usb_serial)
                    or same_head(serial, s.label)), None)
        if hit is None:
            raise ConfigError(
                f"{serial!r} is not in the sensor list (config/sensors.toml) "
                "-- add it in Manage sensors (QZFM tab); run/qzfm.py "
                "--sensors-list shows the list. Data are filed under the "
                "sensor number")
        want = key_number(prefix.strip()) if sep else None
        if want is not None and want != hit.number:
            raise ConfigError(f"{item!r}: {serial} is sensor {hit.number}, "
                              f"not sensor {want}")
        if hit.number in numbers:
            raise ConfigError(
                f"the device name names sensor {hit.number} twice ({spec!r}; "
                "a USB ID and its QuSpin label count as one head)")
        numbers.append(hit.number)
    return numbers


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
    if not s.inputs:
        raise ConfigError(f"sensor {s.number}: set at least one DAQ input "
                          "(Z and/or Y)")
    if s.z_input and s.z_input == s.y_input:
        raise ConfigError(f"sensor {s.number}: Z and Y cannot both go to "
                          f"{s.z_input}")
    warnings = []
    if not s.label.upper().startswith("Z3D0"):
        warnings.append(f"head ID {s.label!r} does not look like a QuSpin "
                        "label (Z3D0-...)")
    if not FTDI_SERIAL_RX.match(s.usb_serial):
        warnings.append(f"USB ID {s.usb_serial!r} does not look like an "
                        "FTDI serial (8-10 capitals and digits, e.g. "
                        "A10QHMQMA)")
    return warnings


def input_sharing(sensors):
    """Notes on sensors wired to the same input: they cannot be recorded
    together (fine when the cable is moved between runs)."""
    owners = {}
    for s in sensors:
        for ax, pin in s.inputs.items():
            owners.setdefault(pin, []).append(f"sensor {s.number} "
                                              f"{ax.upper()}")
    return [f"{pin} is used by {' and '.join(who)} -- those sensors cannot "
            "be recorded together" for pin, who in sorted(owners.items())
            if len(who) > 1]


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
             "#   z_input     the DAQ input its Z output is wired to (ai0..ai7",
             "#               on the PCIe-6321, ai0..ai3 on the USB-6008;",
             "#               or \"\" when Z is not recorded)",
             "#   y_input     the same for its Y output",
             ""]
    for s in sorted(sensors, key=lambda x: x.number):
        lines += ["[[sensors]]",
                  f"number = {int(s.number)}",
                  f"label = {_toml_str(s.label)}",
                  f"usb_serial = {_toml_str(s.usb_serial)}",
                  f"location = {_toml_str(s.location)}",
                  f"z_input = {_toml_str(s.z_input)}",
                  f"y_input = {_toml_str(s.y_input)}",
                  ""]
    os.makedirs(os.path.dirname(os.path.abspath(path)), exist_ok=True)
    write_text_atomic(path, "\n".join(lines))


class Selection:
    """The sensors chosen for one run, each on its own DAQ inputs.

    Keyed by sensor number: sensor 2 is "s2", its Z channel "s2z"
    (bfield.channels). The first chosen leads (set up first, and the
    default modulation master).
    """

    def __init__(self, config, numbers, master=None, axes=None):
        known = {s.number: s for s in load_sensors(config)}
        missing = [n for n in numbers if n not in known]
        if missing:
            raise ConfigError(
                f"sensor(s) {missing} not in the config; configured: "
                + ", ".join(str(n) for n in known))
        if len(set(numbers)) != len(numbers):
            raise ConfigError(f"sensors {list(numbers)} name a sensor twice")
        self.sensors = [known[n] for n in numbers]
        daq = model_of(config)
        if len(self.sensors) > daq.n_inputs:
            raise ConfigError(
                f"{len(self.sensors)} sensors chosen -- the {daq.title} has "
                f"{daq.n_inputs} inputs, so at most {daq.n_inputs} "
                "sensors per run")
        for s in self.sensors:
            if not s.inputs:
                raise ConfigError(f"{s.display} has no DAQ inputs set -- "
                                  "set them in Manage sensors")
        self.wanted = self._wanted(axes)
        for s in self.sensors:
            rec = [ax for ax in s.inputs if ax in self.wanted]
            if not rec:
                raise ConfigError(
                    f"{s.display} is wired for {s.wiring} but this run "
                    f"records {'+'.join(a.upper() for a in self.wanted)} -- "
                    "choose other axes, or set its inputs in Manage sensors")
            if len(self.sensors) == 1 and len(rec) < len(self.wanted):
                missing = [a.upper() for a in self.wanted
                           if a not in s.inputs]
                raise ConfigError(f"{s.display} has no {'/'.join(missing)} "
                                  "input set (Manage sensors)")
        used = {}
        for s in self.sensors:
            for ax, pin in self.recorded(s).items():
                if pin in used:
                    raise ConfigError(
                        f"{pin} is wired to both {used[pin]} and "
                        f"{s.display} {ax.upper()} -- they cannot be "
                        "recorded together; change an input in Manage "
                        "sensors")
                used[pin] = f"{s.display} {ax.upper()}"
        # sensor key ("s2") -> Sensor, in the order chosen
        self.heads = {sensor_key(s.number): s for s in self.sensors}
        # the modulation master (two or more sensors): its key
        self.master_head = None
        if len(self.sensors) > 1:
            if master is None:
                master = self.sensors[0].number
            if master not in self.numbers:
                raise ConfigError(f"master sensor {master} is not one of "
                                  f"the chosen sensors {self.numbers}")
            self.master_head = sensor_key(master)

    def _wanted(self, axes):
        """The axes this run records: given, or Z for one sensor (single-
        axis mode, quieter) and every wired axis for more."""
        if axes:
            want = [a.strip().lower() for a in str(axes).split(",")
                    if a.strip()]
            bad = [a for a in want if a not in AXES]
            if bad or not want or len(set(want)) != len(want):
                raise ConfigError(f"axes {axes!r}: Z and/or Y")
            return [a for a in AXES if a in want]
        if len(self.sensors) == 1:
            s = self.sensors[0]
            return ["z"] if s.z_input else ["y"]
        have = {ax for s in self.sensors for ax in s.inputs}
        return [a for a in AXES if a in have]

    def recorded(self, s):
        """{axis: input} of sensor s in this run."""
        return {ax: pin for ax, pin in s.inputs.items() if ax in self.wanted}

    @property
    def numbers(self):
        return [s.number for s in self.sensors]

    @property
    def device_name(self):
        """The serials in the form the sensor session takes:
        s1:A10QHMQMA,s2:A10QHJ1ZA."""
        return ",".join(f"{k}:{s.usb_serial}" for k, s in self.heads.items())

    @property
    def axes(self):
        """The axes recorded ('z,y', 'z' or 'y'): the sensors' mode."""
        return ",".join(self.wanted)

    def channels(self):
        """[(channel, sensor key, axis, input)] in map order."""
        return [(channel_name(k, ax), k, ax, pin)
                for k, s in self.heads.items()
                for ax, pin in self.recorded(s).items()]

    @property
    def map(self):
        """The recorder channel map, e.g. s1z:ai0,s1y:ai1,s2z:ai2."""
        return ",".join(f"{name}:{pin}"
                        for name, _k, _ax, pin in self.channels())

    def info(self):
        """Per-sensor records for the session json and meta.json."""
        out = []
        for key, s in self.heads.items():
            rec = self.recorded(s)
            out.append({"sensor": key, "number": s.number, "label": s.label,
                        "usb_serial": s.usb_serial, "location": s.location,
                        "inputs": rec,
                        "channels": [channel_name(key, ax) for ax in rec],
                        "wired": dict(s.inputs),
                        "modulation": (None if self.master_head is None
                                       else "master" if key ==
                                       self.master_head else "slave")})
        return out

    def plug_lines(self):
        """What has to be plugged where, one line per sensor."""
        lines = []
        for key, s in self.heads.items():
            role = (" -- modulation master"
                    if key == self.master_head else "")
            lines.append(f"{s.display} -> DAQ "
                         + ", ".join(f"{p} ({a.upper()})"
                                     for a, p in self.recorded(s).items())
                         + role)
        return lines
