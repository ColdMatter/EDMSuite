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


def test_each_sensor_uses_its_own_inputs():
    """Wiring is per sensor (sensors.toml): sensor 2 is on ai2/ai3 whether
    it is chosen first, second or alone."""
    cfg = load_config()
    sel = Selection(cfg, [2, 1])
    assert sel.device_name == "s2:A10QHJ1ZA,s1:A10QHMQMA"
    assert sel.map == "s2z:ai2,s2y:ai3,s1z:ai0,s1y:ai1"
    assert list(sel.heads) == ["s2", "s1"]
    info = sel.info()
    assert (info[0]["number"], info[0]["modulation"]) == (2, "master")
    assert info[0]["inputs"] == {"z": "ai2", "y": "ai3"}
    assert info[0]["sensor"] == "s2"
    assert info[0]["channels"] == ["s2z", "s2y"]
    assert "sensor 2 (Z3D0-AB3S) -> DAQ ai2 (Z), ai3 (Y)" in sel.plug_lines()[0]


def test_axes_to_record():
    cfg = load_config()
    # one sensor: Z only by default (single-axis mode, quieter)
    one = Selection(cfg, [2])
    assert one.axes == "z" and one.map == "s2z:ai2"
    assert Selection(cfg, [2], axes="z,y").map == "s2z:ai2,s2y:ai3"
    # two sensors: every wired axis by default; or what is asked
    assert Selection(cfg, [1, 2]).axes == "z,y"
    assert Selection(cfg, [1, 2], axes="y").map == "s1y:ai1,s2y:ai3"


def test_a_sensor_wired_for_one_axis():
    cfg = with_overrides(load_config(), {})
    cfg["sensors"] = [
        {"number": 1, "label": "Z3D0-AB3Q", "usb_serial": "A10QHMQMA",
         "z_input": "ai0", "y_input": "ai1"},
        {"number": 2, "label": "Z3D0-AB3S", "usb_serial": "A10QHJ1ZA",
         "z_input": "", "y_input": "ai2"}]
    sel = Selection(cfg, [1, 2])
    # sensor 2 records Y only
    assert sel.map == "s1z:ai0,s1y:ai1,s2y:ai2"
    assert sel.info()[1]["inputs"] == {"y": "ai2"}
    with pytest.raises(ConfigError, match="wired for Y->ai2"):
        Selection(cfg, [2], axes="z")
    # sensor 2 alone defaults to its one wired axis
    assert Selection(cfg, [2]).map == "s2y:ai2"


def test_refusals():
    cfg = load_config()
    with pytest.raises(ConfigError):
        Selection(cfg, [3])                       # not configured
    with pytest.raises(ConfigError):
        parse_numbers("1,1")
    more = with_overrides(cfg, {})
    more["sensors"] = cfg["sensors"] + [
        {"number": 3, "label": "Z3D0-XXXX", "usb_serial": "A10QHXXXXA",
         "z_input": "ai0"}]
    many = with_overrides(cfg, {"daq": {"model": "pcie6321"}})
    many["sensors"] = [
        {"number": n, "label": f"Z3D0-AAA{n}", "usb_serial": f"A10QHAA{n}A",
         "z_input": f"ai{(n - 1) % 8}"} for n in range(1, 10)]
    with pytest.raises(ConfigError, match="PCIe-6321 has 8 inputs"):
        Selection(many, list(range(1, 10)))       # eight DAQ inputs
    few = with_overrides(cfg, {"daq": {"model": "usb6008"}})
    few["sensors"] = [dict(r, z_input=f"ai{(r['number'] - 1) % 4}")
                      for r in many["sensors"][:5]]
    with pytest.raises(ConfigError, match="USB-6008 has 4 inputs"):
        Selection(few, [1, 2, 3, 4, 5])           # four DAQ inputs
    # sensors 1 and 3 share ai0: fine in the list, refused together
    from bfield.sensors import input_sharing
    assert any("ai0" in n for n in input_sharing(load_sensors(more)))
    with pytest.raises(ConfigError, match="ai0 is wired to both"):
        Selection(more, [1, 3])
    nowire = with_overrides(cfg, {})
    nowire["sensors"] = cfg["sensors"] + [
        {"number": 4, "label": "Z3D0-WWWW", "usb_serial": "A10QHWWWWA"}]
    with pytest.raises(ConfigError, match="no DAQ inputs"):
        Selection(nowire, [4])
    bad = with_overrides(cfg, {"daq": {"model": "pcie6321"}})
    bad["sensors"] = [dict(cfg["sensors"][0], z_input="ai8")]
    with pytest.raises(ConfigError, match="ai8"):
        load_sensors(bad)                         # ai8 is ai0's minus pin
    bad["sensors"] = [dict(cfg["sensors"][0], z_input="ai7")]
    assert load_sensors(bad)[0].z_input == "ai7"
    old = with_overrides(cfg, {"daq": {"model": "usb6008"}})
    old["sensors"] = bad["sensors"]
    with pytest.raises(ConfigError, match="ai7"):
        load_sensors(old)
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
         "location": "lid", "z_input": "ai0", "y_input": "ai1"}]
    from bfield.devices.simulated import World
    lab.world = World(lab.world.path, sensors=load_sensors(lab.config))
    plan = QZFMPlan.for_sensors(lab.config, [3], seconds=10,
                                outdir=str(tmp_path / "s3"))
    assert QZFMSession(ctx, lab, plan).record() == 0
    meta_path = glob.glob(os.path.join(plan.outdir, "R01_*",
                                       "*.meta.json"))[0]
    meta = json.load(open(meta_path))
    assert meta["sensor_info"][0]["number"] == 3
    assert meta["sensor_info"][0]["location"] == "lid"
    assert meta["channels"]["s3z"].endswith("ai0")
    assert meta["sensor_ids"] == {"s3": "A10QHNEW1A"}
    run_dir = os.path.dirname(meta_path)
    assert glob.glob(os.path.join(run_dir, "*_S3Z_pT.txt"))
    assert os.path.exists(os.path.join(run_dir, "house_S3.csv"))
    lines = [m for _t, m, _l in ctx.ui.lines]
    assert any("sensor 3 (Z3D0-NEW1, lid) -> DAQ ai0 (Z)" in m for m in lines)


def test_sensors_file_round_trip(tmp_path):
    from bfield.sensors import Sensor, save_sensors
    import shutil
    from bfield.config import CONFIG_DIR
    shutil.copy(os.path.join(CONFIG_DIR, "lab.toml"), tmp_path / "lab.toml")
    cfg = load_config(str(tmp_path / "lab.toml"), install_aliases=False)
    s = [Sensor(1, "Z3D0-AB3Q", "A10QHMQMA", 'north "end", lid', "ai0",
                "ai1"),
         Sensor(3, "Z3D0-NEW1", "A10QHNEW1A", "", "", "ai3")]
    save_sensors(cfg.sensors_path, s)
    back = load_sensors(load_config(str(tmp_path / "lab.toml"),
                                    install_aliases=False))
    assert back == s


def test_check_sensor():
    from bfield.sensors import Sensor, check_sensor
    a = Sensor(1, "Z3D0-AB3Q", "A10QHMQMA", "", "ai0", "ai1")
    with pytest.raises(ConfigError, match="already has usb_serial"):
        check_sensor(Sensor(2, "Z3D0-XXXX", "A10QHMQMA", "", "ai2"), [a])
    with pytest.raises(ConfigError, match="head ID"):
        check_sensor(Sensor(2, "", "A10QHXXXXA", "", "ai2"), [a])
    with pytest.raises(ConfigError, match="at least one DAQ input"):
        check_sensor(Sensor(2, "Z3D0-XXXX", "A10QHXXXXA"), [a])
    with pytest.raises(ConfigError, match="cannot both go"):
        check_sensor(Sensor(2, "Z3D0-XXXX", "A10QHXXXXA", "", "ai2", "ai2"))
    warns = check_sensor(Sensor(2, "my head", "com5", "", "ai2"), [a])
    assert len(warns) == 2


def test_chosen_master_gets_the_master_command(sim_lab, tmp_path):
    ctx, lab = sim_lab(60.0)
    sel = Selection(lab.config, [1, 2], master=2)
    assert sel.master_head == "s2"
    assert [r["modulation"] for r in sel.info()] == ["slave", "master"]
    plan = QZFMPlan.for_sensors(lab.config, [1, 2], master=2,
                                outdir=str(tmp_path / "m2"))
    assert QZFMSession(ctx, lab, plan).setup() == 0
    heads = lab.world.read()["heads"]
    assert heads["A10QHJ1ZA"]["master"] and not heads["A10QHMQMA"]["master"]
    doc = json.load(open(os.path.join(plan.outdir, "qzfm_session.json")))
    assert doc["setup"]["sync_roles"] == {"s1": "slave", "s2": "master"}
    with pytest.raises(ConfigError):
        Selection(lab.config, [1, 2], master=3)


def test_device_names_resolve_to_sensor_numbers():
    from bfield.sensors import resolve_device_name
    cfg = load_config()
    assert resolve_device_name(cfg, "A10QHJ1ZA") == [2]
    assert resolve_device_name(cfg, "Z3D0-AB3S,A10QHMQMA") == [2, 1]
    assert resolve_device_name(cfg, "a:A10QHMQMA,b:A10QHJ1ZA") == [1, 2]
    assert resolve_device_name(cfg, "s2:A10QHJ1ZA") == [2]
    with pytest.raises(ConfigError, match="not in the sensor list"):
        resolve_device_name(cfg, "A10QHXXXA")
    with pytest.raises(ConfigError, match="twice"):
        resolve_device_name(cfg, "A10QHMQMA,Z3D0-AB3Q")
    with pytest.raises(ConfigError, match="is sensor 1, not sensor 2"):
        resolve_device_name(cfg, "s2:A10QHMQMA")


def test_three_sensors_on_one_axis(sim_lab, tmp_path):
    """More than two sensors: three on Z, one input each -- one master,
    two slaves, and every channel named by its sensor."""
    ctx, lab = sim_lab(60.0)
    lab.config["sensors"] = [
        {"number": 1, "label": "Z3D0-AB3Q", "usb_serial": "A10QHMQMA",
         "z_input": "ai0"},
        {"number": 2, "label": "Z3D0-AB3S", "usb_serial": "A10QHJ1ZA",
         "z_input": "ai1"},
        {"number": 5, "label": "Z3D0-NEW5", "usb_serial": "A10QHNEW5A",
         "z_input": "ai3"}]
    from bfield.devices.simulated import World
    lab.world = World(lab.world.path, sensors=load_sensors(lab.config))
    plan = QZFMPlan.for_sensors(lab.config, [1, 2, 5], seconds=10,
                                outdir=str(tmp_path / "three"))
    assert plan.map == "s1z:ai0,s2z:ai1,s5z:ai3"
    assert QZFMSession(ctx, lab, plan).record() == 0
    doc = json.load(open(os.path.join(plan.outdir, "qzfm_session.json")))
    assert doc["setup"]["sync_roles"] == {"s1": "master", "s2": "slave",
                                          "s5": "slave"}
    assert set(doc["setup"]["sync"]["carriers_hz"]) == {"s1", "s2", "s5"}
    run_dir = glob.glob(os.path.join(plan.outdir, "R01_*"))[0]
    names = sorted(os.listdir(run_dir))
    for want in ("house_S1.csv", "house_S2.csv", "house_S5.csv"):
        assert want in names
    for ch in ("S1Z", "S2Z", "S5Z"):
        assert any(n.endswith(f"_{ch}_pT.txt") for n in names), names
    meta = json.load(open(glob.glob(os.path.join(run_dir,
                                                 "*.meta.json"))[0]))
    assert {tuple(p["channels"]) for p in meta["pairs"]} == {
        ("s1z", "s2z"), ("s1z", "s5z"), ("s2z", "s5z")}
