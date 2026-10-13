"""The S(f) sweep: plan, scope, and one full --go in the simulated lab.

Simulated only: open_lab(sim=True) everywhere, nothing touches hardware.
"""

import glob
import json
import math
import os
import re

import numpy as np
import pytest

from bfield.config import load_config
from bfield.devices.scope import ScopeError
from bfield.devices.simulated import (COIL_K_NT_PER_MA,
                                      sim_shielding_factor)
from bfield.errors import ConfigError
from bfield.lab import open_lab
from bfield.procedures import sf_sweep as sf
from bfield.ui import UI, Context


def settings(**kw):
    base = dict(mode="plan", seed=123)
    base.update(kw)
    return sf.SweepSettings(**base)


# ------------------------------------------------------------------ plan

def test_shuffle_reproducible_from_seed():
    a, seed_a = sf.build_points(settings(seed=42))
    b, seed_b = sf.build_points(settings(seed=42))
    c, _ = sf.build_points(settings(seed=43))
    assert seed_a == seed_b == 42
    assert [p.f_hz for p in a] == [p.f_hz for p in b]
    assert [p.f_hz for p in a] != [p.f_hz for p in c]


def test_control_point_first_and_last():
    pts, _ = sf.build_points(settings(control_hz=10.0))
    assert pts[0].f_hz == pts[-1].f_hz == 10.0
    assert pts[0].is_control and pts[-1].is_control
    assert pts[-1].dirname == "p11_10Hz_again"
    assert sorted(p.f_hz for p in pts[1:-1]) == \
        [0.1, 0.2, 0.5, 1, 2, 5, 25, 40, 70]
    assert len({p.dirname for p in pts}) == len(pts)


def test_point_durations():
    assert sf.point_seconds(0.1, 90, 900) == 900.0
    assert sf.point_seconds(0.05, 90, 900) == 1800.0
    assert sf.point_seconds(0.07, 90, 900) == 1320.0   # 1285.7 -> whole min
    assert sf.point_seconds(70, 90, 900) == 900.0
    # one length for every point: the longest need wins
    pts, _ = sf.build_points(settings(freqs="0.07,5", control_hz=5.0))
    assert {p.seconds for p in pts} == {1320.0}


def test_plan_refusals():
    for kw in (dict(freqs="5,5"), dict(freqs="1,2", control_hz=5.0),
               dict(freqs="700,5")):
        with pytest.raises(ConfigError):
            sf.build_points(settings(**kw))


def test_hz_tag_and_gate():
    assert sf.hz_tag(0.1) == "0p1Hz" and sf.hz_tag(70) == "70Hz"
    assert sf.scope_gate(4.98, 450, 5.0, 1200) is None
    assert "planned" in sf.scope_gate(1428.57, 450, 0.1, 1200 * 10)
    assert "Nyquist" in sf.scope_gate(700, 450, 600, 1200)
    assert "outside" in sf.scope_gate(5, 5000, 5, 1200)


def test_monitor_resistor_sizing_refused(tmp_path):
    lab = open_lab(load_config(), sim=True, sim_dir=str(tmp_path))
    s = settings(amp_ma=450, rmon_ohm=5.0)          # 2.25 V > 0.99 x 2 V
    assert sf.SfSweep(Context(UI()), lab, s).run() == 13


def test_plan_mode_touches_nothing(tmp_path):
    lab = open_lab(load_config(), sim=True, sim_dir=str(tmp_path))
    before = set(os.listdir(tmp_path))
    assert sf.SfSweep(Context(UI()), lab, settings()).run() == 0
    assert set(os.listdir(tmp_path)) == before


# ----------------------------------------------------------------- scope

def test_scope_sentinel_and_reading(tmp_path):
    lab = open_lab(load_config(), sim=True, sim_dir=str(tmp_path))
    ctx = Context(UI())
    scope = lab.scope(ctx)
    assert "TBS 1052B" in scope.idn and scope.resource.startswith("USB")
    # no drive: every cycle measurement is the 9.9E37 sentinel
    with pytest.raises(ScopeError, match="not available"):
        scope.read_drive(0.3)
    lab.world.set_coil(freq_hz=4.98, amp_mA=450.0, dc_mA=20.0)
    f, i, dc, note = scope.read_drive(0.3)
    assert f == pytest.approx(4.98)
    assert i == pytest.approx(450.0, rel=1e-4)
    assert dc == pytest.approx(20.0, rel=1e-4)
    assert note == ""
    # an old scope without CMEan: offset via MEAN, and it says so
    lab.world.inject(scope_no_cmean=True)
    note = lab.scope(ctx).read_drive(0.3)[3]
    assert "no CMEan" in note
    # DC level through MEAN, signed
    lab.world.set_coil(freq_hz=None, dc_mA=-1000.0)
    i_dc, _ = scope.read_dc(0.3)
    assert i_dc == pytest.approx(-1000.0, rel=1e-4)
    scope.close()


# ------------------------------------------------------------ full sweep

class BenchUI(UI):
    """Plays the operator: sets the (simulated) generator when asked."""

    wants_live_data = False

    def __init__(self, world, amp_ma=450.0):
        self.world = world
        self.amp_ma = amp_ma
        self.prompts = []
        self.lines = []

    def log(self, stamp, tag, msg, level="info"):
        self.lines.append((tag, msg))

    def confirm(self, question):
        return True

    def ask_text(self, prompt):
        self.prompts.append(prompt)
        m = re.search(r"point \d+/\d+.*?: ([0-9.]+) Hz for", prompt)
        if m:
            self.world.set_coil(freq_hz=float(m.group(1)),
                                amp_mA=self.amp_ma, orientation="axial")
        elif "drive off?" in prompt:
            self.world.clear_coil()
        return ""


def lockin_pT(x, fs, f):
    t = np.arange(x.size) / fs
    x = x - x.mean()
    return 2.0 * abs(np.mean(x * np.exp(-2j * math.pi * f * t)))


def test_full_go_in_simulator(tmp_path):
    lab = open_lab(load_config(), sim=True, speed=60.0,
                   sim_dir=str(tmp_path))
    ui = BenchUI(lab.world)
    ctx = Context(ui, config=lab.config)
    # a space in the path, like the Box data folder
    outdir = os.path.join(tmp_path, "shield sf", "sw_test_axial")
    s = sf.SweepSettings.from_config(
        lab.config, mode="go", freqs="5,25", control_hz=5.0, seed=1,
        min_cycles=10.0, min_seconds=20.0, null_seconds=10.0,
        outdir=outdir)
    code = sf.SfSweep(ctx, lab, s).run()
    assert code == 0, ui.lines[-15:]

    names = sorted(os.listdir(outdir))
    for want in ("setup", "null_before", "null_after", "p01_5Hz",
                 "p02_25Hz", "p03_5Hz_again", "sf_sweep.log",
                 "sf_sweep_session.json"):
        assert want in names, names
    assert os.path.exists(os.path.join(outdir, "setup", "qzfm_session.json"))
    for d in ("null_before", "p01_5Hz", "p02_25Hz", "p03_5Hz_again"):
        run = os.path.join(outdir, d)
        assert os.path.exists(os.path.join(run, f"{d}.meta.json"))
        for ch in ("Z", "Y", "Z2", "Y2"):
            assert os.path.exists(os.path.join(run, f"{d}_{ch}_pT.txt"))
        assert not os.path.exists(os.path.join(run, "STOP"))

    doc = json.load(open(os.path.join(outdir, "sf_sweep_session.json"),
                         encoding="utf-8"))
    assert doc["outcome"] == "completed"
    assert doc["seed"] == 1
    assert doc["order"] == [5.0, 25.0, 5.0]
    assert [p["verdict"]["exit_code"] for p in doc["points"]] == [0, 0, 0]
    assert all(doc["settings"]["from_scope"].values())
    assert set(doc["nulls"]) == {"before", "after"}
    assert "shield_sf_ac.py" in doc["analysis_command"]
    assert "--i-ma 450,450,450" in doc["analysis_command"]
    meta = json.load(open(os.path.join(outdir, "p01_5Hz",
                                       "p01_5Hz.meta.json")))
    assert meta["notes"].startswith("sf 5 Hz 450 mA")
    assert meta["cal_factors"]            # traceable to the setup's factors

    # the drive is in the data: on z (axial) at the expected shielded
    # amplitude, not on y, and not in the null
    expect = 450.0 * COIL_K_NT_PER_MA * 1000.0 / sim_shielding_factor(5.0)
    z = np.loadtxt(glob.glob(os.path.join(outdir, "p01_5Hz", "*_Z_pT.txt"))[0])
    y = np.loadtxt(glob.glob(os.path.join(outdir, "p01_5Hz", "*_Y_pT.txt"))[0])
    zn = np.loadtxt(glob.glob(os.path.join(outdir, "null_before",
                                           "*_Z_pT.txt"))[0])
    assert lockin_pT(z, 1200.0, 5.0) == pytest.approx(expect, rel=0.1)
    assert lockin_pT(y, 1200.0, 5.0) < 0.1 * expect
    assert lockin_pT(zn, 1200.0, 5.0) < 0.1 * expect

    # the printed analysis command runs as-is on the simulated sweep: the
    # ported shield_sf_ac.py accepts it (--allow-sim, since the meta says
    # source "simulate") and writes its results
    cmd = doc["analysis_command"]
    assert "--allow-sim" in cmd
    import shlex
    import subprocess
    import sys
    argv = shlex.split(cmd.replace("\\", "/"), posix=True)
    assert argv[0] == "python"
    r = subprocess.run([sys.executable, *argv[1:]], capture_output=True,
                       text=True, timeout=300,
                       env=dict(os.environ, MPLBACKEND="Agg",
                                PYTHONIOENCODING="utf-8"))
    assert r.returncode == 0, r.stdout[-2000:] + r.stderr[-2000:]
    out = argv[argv.index("--outdir") + 1]
    assert os.listdir(out)


class DcUI(BenchUI):
    def ask_text(self, prompt):
        self.prompts.append(prompt)
        m = re.search(r"switch thrown to ([+-])\?", prompt)
        if m:
            sign = 1.0 if m.group(1) == "+" else -1.0
            self.world.set_coil(freq_hz=None, dc_mA=sign * 1000.0)
        return ""


def test_dc_mode_in_simulator(tmp_path):
    lab = open_lab(load_config(), sim=True, speed=60.0,
                   sim_dir=str(tmp_path))
    ui = DcUI(lab.world)
    outdir = os.path.join(tmp_path, "shield_sf", "dc_test_axial")
    s = sf.SweepSettings.from_config(
        lab.config, mode="dc", dc_blocks=1, dc_dwell=15.0, null_seconds=0.0,
        skip_setup=True, outdir=outdir)
    # skip_setup: bring the simulated heads up by hand
    lab.world.update(lambda d: [h.update(laser_on=True, locked=True,
                                         zeroed=True)
                                for h in d["heads"].values()])
    assert sf.SfSweep(Context(ui), lab, s).run() == 0, ui.lines[-10:]
    doc = json.load(open(os.path.join(outdir, "sf_dc_session.json"),
                         encoding="utf-8"))
    assert doc["outcome"] == "completed"
    assert [d["sign"] for d in doc["dwells"]] == list(sf.DC_PATTERN)
    assert [round(d["i_ma"]) for d in doc["dwells"]] == \
        [1000, -1000, -1000, 1000]
    assert doc["settings"]["k_geom_nT_per_mA"] == pytest.approx(20 / 9)
    for d in doc["dwells"]:
        name = os.path.basename(d["dir"])
        assert re.fullmatch(r"b01d[1-4]_[pm]", name)
        assert glob.glob(os.path.join(outdir, name, "*_Z_pT.txt"))
