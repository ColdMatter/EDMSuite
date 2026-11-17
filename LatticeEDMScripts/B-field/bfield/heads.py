"""Sensor-head identity: every head has two ids.

The QuSpin label (Z3D0-AB3Q; calibrations are filed under it) and the
FTDI serial from the USB scan (A10QHMQMA; what the COM-port match and
meta.json see). The pairs come from config/lab.toml [[sensors]]
(label / usb_serial); load_config() installs them here.
"""

from __future__ import annotations

import re

# built-in copy of the measured pairs, used until a config is loaded
# (a bare import, the recorder child before it reads its job file)
_ALIASES = {
    "A10QHJ1ZA": "Z3D0-AB3S",
    "A10QHMQMA": "Z3D0-AB3Q",
}
FTDI_SERIAL_RX = re.compile(r"^[A-Z0-9]{8,10}$")
# a sensor serial as it appears in meta.json -- a typo is permanent
SENSOR_ID_RX = re.compile(r"[A-Za-z0-9][\w.-]{0,31}")

DEFAULT_SENSOR_ID = "Z3D0-AB3Q"    # head a
DEFAULT_SENSOR_ID_B = "Z3D0-AB3S"  # head b (second head, added 2026-07-28)
MEASURED_GAIN_HEAD = DEFAULT_SENSOR_ID   # the head 2.624 V/nT was measured on


def set_aliases(pairs):
    """Replace the FTDI serial -> QuSpin name table."""
    _ALIASES.clear()
    _ALIASES.update({str(k): str(v) for k, v in pairs.items()})


def aliases():
    return dict(_ALIASES)


def head_aliases(name):
    """Every id we know for one head, in one set."""
    out = {name}
    for a, b in _ALIASES.items():
        if name in (a, b):
            out.update((a, b))
    return out


def same_head(a, b):
    # blank ids never match: thin metadata must not read as "same head"
    # and carry the wrong V/nT
    if not a or not b:
        return False
    return bool(head_aliases(a) & head_aliases(b))


def head_is_unknown(name):
    """True for an FTDI serial with no QuSpin name attached: head unknown,
    not known-different."""
    return bool(name and name not in _ALIASES
                and not name.startswith("Z3D0")
                and FTDI_SERIAL_RX.match(name))
