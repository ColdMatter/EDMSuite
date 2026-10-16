"""QZFM sessions end to end in the simulated lab."""

import json
import os
import threading

import pytest

from bfield import clock
from bfield.errors import ConfigError
from bfield.procedures.qzfm_session import QZFMPlan, QZFMSession, validate
from conftest import heads_on

TWO = "A10QHMQMA,A10QHJ1ZA"     # serials -> sensors 1 and 2


def plan(lab, tmp_path, name, **kw):
    return QZFMPlan.from_config(lab.config, outdir=str(tmp_path / name), **kw)


def ledger(p, name="qzfm_session.json"):
    return json.load(open(os.path.join(p.outdir, name)))


def test_validate_refusals(sim_lab):
    _ctx, lab = sim_lab()
    with pytest.raises(ConfigError):
        validate(QZFMPlan.from_config(lab.config, device_name=""), "run")
    with pytest.raises(ConfigError):   # one sensor named twice
        validate(QZFMPlan.from_config(
            lab.config, device_name="a:A10QHMQMA,b:Z3D0-AB3Q"), "run")
    with pytest.raises(ConfigError):   # not in the sensor list
        QZFMPlan.from_config(lab.config, device_name="A10QHXXXA")
    with pytest.raises(ConfigError):   # s2 is not that serial
        QZFMPlan.from_config(lab.config, device_name="s2:A10QHMQMA")
    with pytest.raises(ConfigError):   # inf seconds
        validate(QZFMPlan.from_config(lab.config, device_name="A10QHMQMA",
                                      seconds=float("inf")), "run")
    with pytest.raises(ConfigError):   # reserved recorder flag
        validate(QZFMPlan.from_config(lab.config, device_name="A10QHMQMA",
                                      extra_args=["--no-rail-check"]), "run")


def test_two_head_record_cold_start(sim_lab, tmp_path):
    ctx, lab = sim_lab(60.0)
    p = plan(lab, tmp_path, "rec", device_name=TWO, seconds=30)
    code = QZFMSession(ctx, lab, p).record()
    assert code == 0
    doc = ledger(p)
    assert doc["outcome"] == "completed"
    assert doc["setup"]["guard_exit"] == 0
    # everything keyed by sensor number
    assert p.device_name == "s1:A10QHMQMA,s2:A10QHJ1ZA"
    assert p.map == "s1z:ai0,s1y:ai1,s2z:ai2,s2y:ai3"
    assert doc["setup"]["sync_roles"] == {"s1": "master", "s2": "slave"}
    assert doc["setup"]["heads"]["s2"]["cal_factors"] == {
        "z": 0.82, "y": 0.79, "x": 0.99}
    assert [r["sensor"] for r in doc["sensors"]] == ["s1", "s2"]
    run = doc["runs"][0]
    assert run["outcome"] == "ok" and run["exit_code"] == 0
    assert os.path.exists(os.path.join(p.outdir, "qzfm_session.log"))
    runs = [d for d in os.listdir(p.outdir) if d.startswith("R01_")]
    assert len(runs) == 1
    files = os.listdir(os.path.join(p.outdir, runs[0]))
    assert "house_S1.csv" in files and "house_S2.csv" in files
    assert "child.log" in files
    for ch in ("S1Z", "S1Y", "S2Z", "S2Y"):
        assert f"{runs[0]}_{ch}_pT.txt" in files
    meta = json.load(open(os.path.join(p.outdir, runs[0],
                                       f"{runs[0]}.meta.json")))
    assert meta["topology"] == "numbered"
    assert meta["sensor_ids"] == {"s1": "A10QHMQMA", "s2": "A10QHJ1ZA"}
    assert meta["sensor_of"]["s2y"] == "s2"
    assert meta["cal_factors"]["s2z"] == 0.82
    assert {tuple(pr["channels"]) for pr in meta["pairs"]} == {
        ("s1z", "s2z"), ("s1y", "s2y")}
    assert set(meta["rail_fraction"]) == {"s1z", "s1y", "s2z", "s2y"}


def test_live_samples_reach_the_ui(sim_lab, tmp_path):
    ctx, lab = sim_lab(20.0)
    heads_on(lab)
    ctx.ui.wants_live_data = True
    p = plan(lab, tmp_path, "live", device_name="A10QHMQMA", seconds=20)
    assert QZFMSession(ctx, lab, p).record() == 0
    # the recorder's raw volts, tailed from its csv while it records
    assert len(ctx.ui.samples.get("V_s1z", [])) == 20 * 1200


def test_guard_heal_then_stop(sim_lab, tmp_path):
    ctx, lab = sim_lab(60.0)
    heads_on(lab)
    lab.world.inject(rail_axis=["ai0"])
    p = plan(lab, tmp_path, "heal", device_name="A10QHMQMA")
    code = QZFMSession(ctx, lab, p).setup()
    assert code == 20
    doc = ledger(p)
    assert doc["n_guards"] == 2 and len(doc["heals"]) == 1
    assert doc["outcome"].startswith("aborted: SessionStop")


def test_calib_silence_warns_or_refuses(sim_lab, tmp_path):
    ctx, lab = sim_lab(60.0)
    heads_on(lab)
    lab.world.inject(calib_silent=["A10QHMQMA"])
    p = plan(lab, tmp_path, "silent", device_name="A10QHMQMA")
    assert QZFMSession(ctx, lab, p).setup() == 0
    doc = ledger(p)
    assert doc["setup"]["cal_factors"] == {}
    assert "no line containing 'Calib'" in doc["setup"]["raw_calib_message"]
    p2 = plan(lab, tmp_path, "strict", device_name="A10QHMQMA",
              require_factors=True)
    assert QZFMSession(ctx, lab, p2).setup() == 13


def test_standby_confirms_dark(sim_lab, tmp_path):
    ctx, lab = sim_lab(60.0)
    heads_on(lab)
    p = plan(lab, tmp_path, "sb", device_name=TWO)
    assert QZFMSession(ctx, lab, p).standby() == 0
    doc = ledger(p, "qzfm_standby_session.json")
    assert all(s["dark_confirmed"] for s in doc["standby"].values())


def test_stop_during_record_keeps_the_meta(sim_lab, tmp_path):
    ctx, lab = sim_lab(20.0)
    heads_on(lab)
    p = plan(lab, tmp_path, "stop", device_name="A10QHMQMA", seconds=600)

    def press_abort():
        # wait until the record is running, then press Abort
        for _ in range(600):
            if any(m.startswith("recording") for _t, m, _l in ctx.ui.lines):
                break
            threading.Event().wait(0.05)
        threading.Event().wait(0.5)
        ctx.request_stop()

    threading.Thread(target=press_abort, daemon=True).start()
    code = QZFMSession(ctx, lab, p).record()
    clock.set_speed(1.0)
    assert code == 130
    run = [d for d in os.listdir(p.outdir) if d.startswith("R01_")][0]
    meta = [f for f in os.listdir(os.path.join(p.outdir, run))
            if f.endswith(".meta.json")]
    assert meta, "the child must write its meta.json on a stop"
    m = json.load(open(os.path.join(p.outdir, run, meta[0])))
    assert m["status"] == "interrupted" and m["n_samples"] > 0
    assert ledger(p)["outcome"].startswith("interrupted")
