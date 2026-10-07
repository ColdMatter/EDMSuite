"""Numbered sensors: the config list, the run selection, the records."""

import glob
import json
import os

import pytest

from bfield.config import load_config, with_overrides
from bfield.errors import ConfigError
from bfield.procedures.qzfm_session import QZFMPlan, QZFMSession
from bfield.sensors import (Selection, display_of, load_sensors,
                            parse_numbers)


def test_config_sensors_and_aliases():
    from bfield import heads
    cfg = load_config()
    s = load_sensors(cfg)
    assert [x.number for x in s] == [1, 2]
    assert s[0].label == "Z3D0-AB3Q" and s[0].usb_serial == "A10QHMQMA"
    assert heads.same_head("A10QHJ1ZA", "Z3D0-AB3S")


def test_selection_fills_slots_in_the_order_given():
    sel = Selection(load_config(), [2, 1])
    assert sel.device_name == "a:A10QHJ1ZA,b:A10QHMQMA"
    assert sel.map == "z:a:ai0,y:a:ai1,z2:b:ai2,y2:b:ai3"
    info = sel.info()
    assert (info[0]["number"], info[0]["modulation"]) == (2, "master")
    assert info[1]["inputs"] == {"z": "ai2", "y": "ai3"}
    assert "sensor 2 (Z3D0-AB3S) -> DAQ ai0 (Z), ai1 (Y)" in sel.plug_lines()[0]


def test_one_sensor_goes_into_slot_one():
    sel = Selection(load_config(), [2])
    assert sel.device_name == "A10QHJ1ZA" and sel.map == "z:a:ai0,y:a:ai1"


def test_refusals():
    cfg = load_config()
    with pytest.raises(ConfigError):
        Selection(cfg, [3])                       # not configured
    with pytest.raises(ConfigError):
        parse_numbers("1,1")
    more = with_overrides(cfg, {})
    more["sensors"] = cfg["sensors"] + [
        {"number": 3, "label": "Z3D0-XXXX", "usb_serial": "A10QHXXXXA"}]
    with pytest.raises(ConfigError, match="input slots"):
        Selection(more, [1, 2, 3])                # USB-6008: two per run
    dup = with_overrides(cfg, {})
    dup["sensors"] = cfg["sensors"] + [
        {"number": 1, "label": "Z3D0-YYYY", "usb_serial": "A10QHYYYYA"}]
    with pytest.raises(ConfigError, match="share number"):
        load_sensors(dup)


def test_location_is_shown():
    rec = {"number": 4, "label": "Z3D0-AB3Q", "location": "north end"}
    assert display_of(rec) == "sensor 4 (Z3D0-AB3Q, north end)"


def test_a_third_sensor_runs_in_the_simulator(sim_lab, tmp_path):
    """A sensor added to the config gets a simulated head; chosen alone it
    sits in slot 1, and every record says which head it was."""
    ctx, lab = sim_lab(60.0)
    lab.config["sensors"] = lab.config["sensors"] + [
        {"number": 3, "label": "Z3D0-NEW1", "usb_serial": "A10QHNEW1A",
         "location": "lid"}]
    from bfield.devices.simulated import World
    lab.world = World(lab.world.path, sensors=load_sensors(lab.config))
    plan = QZFMPlan.for_sensors(lab.config, [3], seconds=10,
                                outdir=str(tmp_path / "s3"))
    assert QZFMSession(ctx, lab, plan).record() == 0
    meta = json.load(open(glob.glob(os.path.join(
        plan.outdir, "R01_*", "*.meta.json"))[0]))
    assert meta["sensor_info"][0]["number"] == 3
    assert meta["sensor_info"][0]["location"] == "lid"
    assert meta["channel_map"]["z"].endswith("ai0")
    lines = [m for _t, m, _l in ctx.ui.lines]
    assert any("sensor 3 (Z3D0-NEW1, lid) -> DAQ ai0 (Z)" in m for m in lines)


def test_sensors_file_round_trip(tmp_path):
    from bfield.sensors import Sensor, save_sensors
    import shutil
    from bfield.config import CONFIG_DIR
    shutil.copy(os.path.join(CONFIG_DIR, "lab.toml"), tmp_path / "lab.toml")
    cfg = load_config(str(tmp_path / "lab.toml"), install_aliases=False)
    s = [Sensor(1, "Z3D0-AB3Q", "A10QHMQMA", 'north "end", lid'),
         Sensor(3, "Z3D0-NEW1", "A10QHNEW1A", "")]
    save_sensors(cfg.sensors_path, s)
    back = load_sensors(load_config(str(tmp_path / "lab.toml"),
                                    install_aliases=False))
    assert back == s


def test_check_sensor():
    from bfield.sensors import Sensor, check_sensor
    a = Sensor(1, "Z3D0-AB3Q", "A10QHMQMA")
    with pytest.raises(ConfigError, match="already has usb_serial"):
        check_sensor(Sensor(2, "Z3D0-XXXX", "A10QHMQMA"), [a])
    with pytest.raises(ConfigError, match="head ID"):
        check_sensor(Sensor(2, "", "A10QHXXXXA"), [a])
    warns = check_sensor(Sensor(2, "my head", "com5"), [a])
    assert len(warns) == 2


def test_chosen_master_gets_the_master_command(sim_lab, tmp_path):
    ctx, lab = sim_lab(60.0)
    sel = Selection(lab.config, [1, 2], master=2)
    assert sel.master_head == "b"
    assert [r["modulation"] for r in sel.info()] == ["slave", "master"]
    plan = QZFMPlan.for_sensors(lab.config, [1, 2], master=2,
                                outdir=str(tmp_path / "m2"))
    assert QZFMSession(ctx, lab, plan).setup() == 0
    heads = lab.world.read()["heads"]
    assert heads["A10QHJ1ZA"]["master"] and not heads["A10QHMQMA"]["master"]
    doc = json.load(open(os.path.join(plan.outdir, "qzfm_session.json")))
    assert doc["setup"]["sync_roles"] == {"a": "slave", "b": "master"}
    with pytest.raises(ConfigError):
        Selection(lab.config, [1, 2], master=3)
