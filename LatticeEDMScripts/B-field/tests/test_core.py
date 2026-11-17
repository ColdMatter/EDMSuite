"""Config, head identity, clock, files, the calibrate parser."""

import os

import pytest

from bfield import clock, heads
from bfield.config import load_config, with_overrides
from bfield.devices.qzfm import HealthTracker, parse_calib_message
from bfield.errors import ConfigError
from bfield.io.files import outdir_occupied, unique_outdir, write_json_atomic
from bfield.lab import open_lab


def test_config_loads_and_installs_aliases():
    cfg = load_config()
    assert cfg["sensors"][0]["usb_serial"] == "A10QHMQMA"
    assert heads.same_head("A10QHMQMA", "Z3D0-AB3Q")
    assert not heads.same_head("A10QHMQMA", "A10QHJ1ZA")
    assert not heads.same_head("", "")


def test_local_override_is_merged(tmp_path):
    base = tmp_path / "lab.toml"
    base.write_text('[paths]\ndata_root = "X"\nbucket_record = "newdata"\n'
                    '[daq]\nfs_hz = 1200.0\nvrange_V = 2.0\n')
    (tmp_path / "lab.local.toml").write_text('[daq]\nvrange_V = 5.0\n')
    cfg = load_config(str(base), install_aliases=False)
    assert cfg["daq"]["vrange_V"] == 5.0 and cfg["daq"]["fs_hz"] == 1200.0
    assert len(cfg.sources) == 2
    cfg2 = with_overrides(cfg, {"daq": {"fs_hz": 800.0}})
    assert cfg2["daq"]["fs_hz"] == 800.0 and cfg["daq"]["fs_hz"] == 1200.0


def test_head_is_unknown():
    assert heads.head_is_unknown("A10QZZZZA")
    assert not heads.head_is_unknown("A10QHMQMA")
    assert not heads.head_is_unknown("Z3D0-AB3Q")


def test_speed_refused_on_the_bench():
    with pytest.raises(ConfigError):
        open_lab(load_config(), sim=False, speed=10.0)


def test_clock_speed(tmp_path):
    open_lab(load_config(), sim=True, speed=100.0, sim_dir=str(tmp_path))
    t0 = clock.monotonic()
    clock.sleep(5.0)                      # 0.05 s real
    assert 4.5 < clock.monotonic() - t0 < 30.0
    clock.set_speed(1.0)


def test_unique_outdir(tmp_path):
    p = tmp_path / "newdata"
    assert unique_outdir(str(p)) == os.path.normpath(str(p))
    p.mkdir()
    assert not outdir_occupied(str(p))           # empty is free
    (p / "x").write_text("x")
    assert unique_outdir(str(p)).endswith("newdata1")


def test_write_json_atomic(tmp_path):
    path = str(tmp_path / "a.json")
    write_json_atomic(path, {"a": 1})
    assert open(path).read().strip().startswith("{")
    with pytest.raises(TypeError):
        write_json_atomic(path, {"a": object()})
    assert not os.path.exists(path + ".tmp")
    assert '"a": 1' in open(path).read()          # old file survives


def test_parse_calib_message_both_shapes():
    assert parse_calib_message("Calib. Fact. : 0.64 (z)") == {"z": 0.64}
    assert parse_calib_message("x:1.02,y:0.98") == {"x": 1.02, "y": 0.98}
    # outside 0.1..3.0 is dropped (15.90 event, 2026-08-19)
    assert parse_calib_message("Calib. Fact. : 15.90 (z) 0.8 (y)") == \
        {"y": 0.8}


def test_health_tracker_ignores_one_blank_frame():
    t = HealthTracker()
    blank = {k: float("nan") for k in ("B0_pT", "By_pT", "Bz_pT",
                                       "cellT_err")}
    blank.update(laser_on=False, cellT_lock=False, laser_lock=False,
                 zeroed=False)
    assert t.check(blank) is None
    ok = {"B0_pT": 1.0, "By_pT": 1.0, "Bz_pT": 1.0, "cellT_err": 0.0,
          "laser_on": True, "cellT_lock": True, "laser_lock": True,
          "zeroed": True}
    assert t.check(ok) is None
    assert "laser lock lost" in t.check(dict(ok, laser_lock=False))
