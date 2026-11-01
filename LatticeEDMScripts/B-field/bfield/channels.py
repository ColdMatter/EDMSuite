"""Sensor and channel names: everything is named by the sensor number.

    sensor 2            key  "s2"    (house_S2.csv, ledger keys)
    sensor 2, Z axis    channel "s2z" (R01_1631_S2Z_pT.txt, meta keys)

Lower case inside the program and in json keys, upper case in file names
and on screen -- the way the old single-letter channels (z / Z) worked,
so the analysis tools that look a channel up by name read both.

The recorder channel map lists each recorded channel and its DAQ input:

    s1z:ai0,s1y:ai1,s2z:ai2
"""

from __future__ import annotations

import re

from bfield.daq_models import active as daq_model
from bfield.errors import ConfigError

KEY_RX = re.compile(r"s([1-9][0-9]?)")
CHANNEL_RX = re.compile(r"s([1-9][0-9]?)([xyz])")
AXES_PER_SENSOR = 2            # a dual-axis QZFM has two analog outputs


def sensor_key(number):
    """1 -> 's1'."""
    return f"s{int(number)}"


def key_number(key):
    """'s1' -> 1 (None for anything else)."""
    m = KEY_RX.fullmatch(str(key).lower())
    return int(m.group(1)) if m else None


def channel_name(number_or_key, axis):
    """(1, 'z') or ('s1', 'z') -> 's1z'."""
    key = (number_or_key if isinstance(number_or_key, str)
           else sensor_key(number_or_key))
    return f"{key.lower()}{axis.lower()}"


def split_channel(name):
    """'s1z' -> ('s1', 'z'); (None, None) for any other name."""
    m = CHANNEL_RX.fullmatch(str(name).lower())
    if not m:
        return None, None
    return f"s{m.group(1)}", m.group(2)


def label(name):
    """How a channel or sensor key is shown: 's1z' -> 'S1Z'."""
    return str(name).upper()


def parse_map(spec):
    """'s1z:ai0,s1y:ai1,s2z:ai2' -> ordered [(channel, sensor key, aiN)].

    Refuses what the recorder refuses, so a bad map fails here and not
    three minutes of bring-up later."""
    def require(ok, msg):
        if not ok:
            raise ConfigError(msg)

    entries = []
    for item in (spec or "").split(","):
        item = item.strip()
        require(bool(item), f"the map has an empty entry (stray comma?) in "
                            f"{spec!r}")
        name, sep, phys = (p.strip().lower() for p in item.partition(":"))
        key, axis = split_channel(name)
        require(sep and key is not None,
                f"bad map entry {item!r} (want sensor channel:input, e.g. "
                "s1z:ai0 = sensor 1's Z output on ai0)")
        require(re.fullmatch(daq_model().input_rx, phys),
                f"bad input {phys!r} in the map; {daq_model().inputs_text}")
        entries.append((name, key, phys))
    require(bool(entries), "the map is empty")
    names = [e[0] for e in entries]
    require(len(set(names)) == len(names), "the map repeats a channel")
    pins = [e[2] for e in entries]
    dupes = sorted({p for p in pins if pins.count(p) > 1})
    require(not dupes,
            f"the map puts {', '.join(dupes)} on more than one channel; "
            f"{daq_model().inputs_text}")
    for key in {e[1] for e in entries}:
        n = sum(1 for e in entries if e[1] == key)
        require(n <= AXES_PER_SENSOR,
                f"the map gives sensor {key_number(key)} {n} channels; a "
                "dual-axis QZFM has only two analog outputs")
    return entries


def map_keys(spec):
    """The sensor keys a map covers, in map order."""
    out = []
    for _n, key, _p in parse_map(spec):
        if key not in out:
            out.append(key)
    return out


def pins_of(spec, key, axes=None):
    """{axis: aiN} of one sensor in a map."""
    out = {split_channel(n)[1]: p for n, k, p in parse_map(spec) if k == key}
    if axes is None:
        return out
    return {ax: out[ax] for ax in axes if ax in out}


def restrict(spec, keys):
    """The map with only these sensors' channels."""
    return ",".join(f"{n}:{p}" for n, k, p in parse_map(spec) if k in keys)


def shared_axis_pairs(names):
    """Channels that read the same axis on different sensors, as pairs
    (first sensor's channel, other's): s1z/s2z, s1z/s3z, ..."""
    out = []
    split = [(n, *split_channel(n)) for n in names]
    for i, (a, ka, axa) in enumerate(split):
        if ka is None:
            continue
        for b, kb, axb in split[i + 1:]:
            if kb is not None and axb == axa and kb != ka:
                out.append((a, b))
    return out


# plot colours: sensor 1 Z/Y, sensor 2 Z/Y, ... (the old z/y/z2/y2 the same)
PALETTE = ("#1f77b4", "#ff7f0e", "#2ca02c", "#d62728", "#9467bd",
           "#8c564b", "#e377c2", "#7f7f7f")
OLD_COLOURS = {"z": 0, "y": 1, "z2": 2, "y2": 3, "x": 4, "x2": 5}


def colour(name):
    """A plot colour for a channel (s1z, or an old z/y/z2/y2)."""
    name = str(name).lower()
    if name.startswith("v_"):
        name = name[2:]
    key, axis = split_channel(name)
    if key is not None:
        i = 2 * (key_number(key) - 1) + (0 if axis == "z" else 1)
        return PALETTE[i % len(PALETTE)]
    if name in OLD_COLOURS:
        return PALETTE[OLD_COLOURS[name]]
    return "#555555"


def channel_sort_key(name):
    """Sensor number, then Z before Y before X; old names (z, y, z2...)
    after, in their old order."""
    key, axis = split_channel(name)
    if key is not None:
        return (0, key_number(key), "zyx".find(axis), name)
    return (1, 0, "zyx".find(name[:1]), name)
