"""Lab configuration: config/lab.toml, with config/lab.local.toml on top.

The config is a plain nested dict so a GUI can show and edit it and a
session json can store it verbatim. `load_config()` also installs the
sensors' serial pairs (QuSpin label <-> USB serial) into bfield.heads,
so every same-head check in the process uses the bench's own table.
"""

from __future__ import annotations

import copy
import os
import tomllib

from bfield import heads

PACKAGE_DIR = os.path.dirname(os.path.abspath(__file__))
PROJECT_DIR = os.path.dirname(PACKAGE_DIR)
CONFIG_DIR = os.path.join(PROJECT_DIR, "config")
DEFAULT_CONFIG = os.path.join(CONFIG_DIR, "lab.toml")
LOCAL_CONFIG = os.path.join(CONFIG_DIR, "lab.local.toml")
SENSORS_FILE = "sensors.toml"     # the sensor list, next to lab.toml


class ConfigFileError(Exception):
    code = 13


def _merge(base, extra):
    out = copy.deepcopy(base)
    for k, v in extra.items():
        if isinstance(v, dict) and isinstance(out.get(k), dict):
            out[k] = _merge(out[k], v)
        else:
            out[k] = copy.deepcopy(v)
    return out


def _read(path):
    try:
        with open(path, "rb") as fh:
            return tomllib.load(fh)
    except FileNotFoundError:
        raise ConfigFileError(f"config file {path} not found")
    except tomllib.TOMLDecodeError as e:
        raise ConfigFileError(f"config file {path} is not valid TOML: {e}")


class LabConfig(dict):
    """The merged config dict plus where it came from."""

    sources: list
    sensors_path: str = ""

    @property
    def data_root(self):
        return os.path.normpath(self["paths"]["data_root"])

    def bucket(self, key):
        """Absolute path of a default bucket, e.g. bucket('relax')."""
        return os.path.join(self.data_root, self["paths"][f"bucket_{key}"])

    def section(self, name):
        return self.get(name) or {}


def load_config(path=None, *, local=True, install_aliases=True):
    """Read the lab config. `path` replaces config/lab.toml; the local
    override is merged on top unless local=False."""
    path = path or DEFAULT_CONFIG
    doc = _read(path)
    sources = [os.path.abspath(path)]
    local_path = os.path.join(os.path.dirname(os.path.abspath(path)),
                              "lab.local.toml")
    if local and os.path.exists(local_path):
        doc = _merge(doc, _read(local_path))
        sources.append(local_path)
    # the sensor list: its own file, so the GUI can rewrite it without
    # touching lab.toml; it replaces any [[sensors]] in lab.toml
    sensors_path = os.path.join(os.path.dirname(os.path.abspath(path)),
                                SENSORS_FILE)
    if os.path.exists(sensors_path):
        doc["sensors"] = _read(sensors_path).get("sensors") or []
        sources.append(sensors_path)
    cfg = LabConfig(doc)
    cfg.sensors_path = sensors_path
    cfg.sources = sources
    if install_aliases:
        from bfield.sensors import aliases
        heads.set_aliases(aliases(cfg))
    return cfg


def with_overrides(cfg, overrides):
    """A copy of cfg with a nested dict of overrides merged in (tests,
    the simulated lab, a GUI edit before saving)."""
    out = LabConfig(_merge(cfg, overrides))
    out.sources = list(getattr(cfg, "sources", [])) + ["<overrides>"]
    return out
