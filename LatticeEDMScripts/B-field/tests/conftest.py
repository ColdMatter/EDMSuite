"""Shared fixtures. Every test runs in the SIMULATED lab -- nothing here
may touch hardware."""

import os
import sys

import pytest

PROJECT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
if PROJECT not in sys.path:
    sys.path.insert(0, PROJECT)

from bfield.config import load_config  # noqa: E402
from bfield.lab import open_lab  # noqa: E402
from bfield.ui import UI, Context  # noqa: E402


class AutoUI(UI):
    """Answers every question yes and records what it was shown."""

    def __init__(self):
        self.lines = []
        self.asked = []
        self.paused = []
        self.samples = {}

    def log(self, stamp, tag, msg, level="info"):
        self.lines.append((tag, msg, level))

    def confirm(self, question):
        self.asked.append(question)
        return True

    def acknowledge(self, message):
        self.paused.append(message)

    def data(self, name, samples, fs):
        self.samples.setdefault(name, []).extend(samples)


@pytest.fixture
def sim_lab(tmp_path):
    """factory: sim_lab(speed) -> (ctx, lab) in a fresh simulated world."""
    def make(speed=60.0):
        lab = open_lab(load_config(), sim=True, speed=speed,
                       sim_dir=str(tmp_path / "sim"))
        ctx = Context(AutoUI(), config=lab.config)
        return ctx, lab
    return make


def heads_on(lab, zeroed=True):
    """Put every simulated head in the locked state (skips auto start)."""
    lab.world.update(lambda d: [h.update(laser_on=True, locked=True,
                                         zeroed=zeroed)
                                for h in d["heads"].values()])
