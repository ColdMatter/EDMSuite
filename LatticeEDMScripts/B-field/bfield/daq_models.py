"""The NI DAQ cards the recorder can run on, and which one this bench has.

config/lab.toml picks the card:

    [daq]
    model = "pcie6321"     # or "usb6008"

A config with no model is a USB-6008 one (every run before 2026-10-20
recorded through the USB-6008, and their session jsons carry no model).

Opening a Lab makes its card the active model (``active()``), so code
that has no config at hand -- the recorder's --map parsers -- checks
inputs, ranges and rates against the card actually in use.
"""

from __future__ import annotations

from dataclasses import dataclass

from bfield.errors import ConfigError


@dataclass(frozen=True)
class DaqModel:
    key: str                  # the config value, "usb6008"
    title: str                # "USB-6008", stamped into data files
    n_inputs: int             # differential inputs, ai0..ai(n-1)
    vranges: tuple            # differential input ranges, +/- V
    aggregate_max: float      # S/s across all channels
    adc_bits: int

    @property
    def inputs(self):
        return tuple(f"ai{i}" for i in range(self.n_inputs))

    @property
    def input_rx(self):
        return rf"ai[0-{self.n_inputs - 1}]"

    @property
    def input_span(self):
        return f"ai0..ai{self.n_inputs - 1}"

    @property
    def inputs_text(self):
        """'the USB-6008 has 4 differential inputs, ai0..ai3'."""
        return (f"the {self.title} has {self.n_inputs} differential inputs, "
                f"{self.input_span}")

    @property
    def max_vrange(self):
        return max(self.vranges)

    def lsb(self, vrange):
        """One ADC code at +/-vrange, in volts."""
        return 2.0 * vrange / 2 ** self.adc_bits

    def smallest_vrange(self, peak_v):
        """Smallest range that still leaves some headroom, None if none
        fits."""
        from bfield.constants import PROBE_HEADROOM
        return next((r for r in self.vranges
                     if peak_v * PROBE_HEADROOM <= r), None)


USB6008 = DaqModel("usb6008", "USB-6008", 4, (1.0, 2.0, 5.0, 10.0, 20.0),
                   10_000, 12)
# 8 differential pairs (ai0/ai8 ... ai7/ai15), 250 kS/s aggregate
PCIE6321 = DaqModel("pcie6321", "PCIe-6321", 8, (0.2, 1.0, 5.0, 10.0),
                    250_000, 16)

MODELS = {m.key: m for m in (USB6008, PCIE6321)}
LEGACY_MODEL = USB6008        # a config that names no model

_active = LEGACY_MODEL


def model_of(config):
    """The card a config names ([daq] model)."""
    d = (config.section("daq") if hasattr(config, "section")
         else (config or {}).get("daq")) or {}
    key = str(d.get("model") or "").strip().lower()
    if not key:
        return LEGACY_MODEL
    try:
        return MODELS[key]
    except KeyError:
        raise ConfigError(f"[daq] model {key!r} is not one of "
                          f"{', '.join(MODELS)}")


def set_active(config):
    """Make the config's card the active model; returns it."""
    global _active
    _active = model_of(config)
    return _active


def active():
    return _active
