"""The degauss-relaxation program, in the simulated lab only."""

import glob
import json
import os

import pytest

from bfield.errors import ConfigError
from bfield.procedures.relaxation import (RelaxSettings, ladder_at,
                                          parse_at, plan_duty_cycle,
                                          point_dir_name, run, validate)
from conftest import AutoUI


def quick(**kw):
    """A short evening: 60 s taper, two 30 s points, no checkout."""
    base = dict(at="1,3", seconds=30, taper=60, checkout_seconds=0)
    base.update(kw)
    return RelaxSettings(**base)


def session_doc(parent):
    rx = glob.glob(os.path.join(parent, "rx_*"))
    assert len(rx) == 1
    with open(os.path.join(rx[0], "degauss_relax_session.json"),
              encoding="utf-8") as fh:
        return rx[0], json.load(fh)


# ------------------------------------------------------------ plan logic

def test_parse_at_and_ladder():
    assert parse_at("5, 20,45") == [5.0, 20.0, 45.0]
    for bad in ("", "5,5", "20,5", "x", "-1"):
        with pytest.raises(ConfigError):
            parse_at(bad)
    assert ladder_at(60, 3) == [60, 120, 180]
    with pytest.raises(ConfigError):
        ladder_at(300, 1)
    assert point_dir_name(1, 5) == "p1_t5m"
    assert point_dir_name(2, 7.5) == "p2_t7p5m"


def test_duty_cycle_naps_and_late_points():
    duty = plan_duty_cycle([5, 20, 45, 80, 115], 900, 0.0, 5.0, 0.8)
    assert [d["sleep_after"] for d in duty] == [False, False, True, True,
                                                False]
    assert duty[0]["wake_at_min"] == 0.0
    assert duty[3]["wake_at_min"] == 75
    assert duty[0]["gap_after_min"] < 0          # p2 starts late
    never = plan_duty_cycle([5, 20, 45], 900, None, 5.0)
    assert not any(d["sleep_after"] for d in never)


def test_validate_resolves_and_refuses(sim_lab):
    ctx, lab = sim_lab()
    s = RelaxSettings()
    validate(ctx, lab, s, "plan")
    assert (s.amp, s.freq, s.taper, s.coil) == (8.0, 15.0, 300.0,
                                                "parallel4")
    assert s.device_name == "A10QHMQMA" and s.dg_port == "COM5"
    with pytest.raises(ConfigError):             # strong on parallel
        validate(ctx, lab, RelaxSettings(strong=True), "plan")
    with pytest.raises(ConfigError):             # series finish is 3 Hz
        validate(ctx, lab, RelaxSettings(winding="series", freq=5.0),
                 "plan")
    with pytest.raises(ConfigError):             # one head named twice
        validate(ctx, lab, RelaxSettings(
            device_name="a:A10QHMQMA,b:Z3D0-AB3Q"), "plan")
    with pytest.raises(ConfigError):             # --every with --at
        validate(ctx, lab, RelaxSettings(every=30, at="5,10"), "plan")


def test_plan_touches_nothing(sim_lab, tmp_path):
    ctx, lab = sim_lab()
    s = RelaxSettings(outdir=str(tmp_path / "rx"))
    assert run(ctx, lab, s, "plan") == 0
    assert not os.path.exists(tmp_path / "rx")
    assert any(tag == "plan" for tag, _m, _l in ctx.ui.lines)


# --------------------------------------------------------- whole evening

def test_go_full_evening(sim_lab, tmp_path):
    ctx, lab = sim_lab(60.0)
    parent = str(tmp_path / "relax")
    code = run(ctx, lab, quick(outdir=parent), "go")
    assert code == 0
    rx, doc = session_doc(parent)
    assert doc["outcome"] == "completed"
    assert doc["t0"] and doc["t0_source"].endswith("degauss_session.json")
    assert doc["duty_cycle"]["final_state"] == "asleep"
    assert [p["dir_name"] for p in doc["points"]] == ["p1_t1m", "p2_t3m"]
    assert all(p["outcome"] == "ok" for p in doc["points"])
    assert doc["points"][0]["woke_from_unpowered"] is True
    assert doc["points"][0]["t_setup_overhead_min"] is not None
    names = [s["step"] for s in doc["steps"]]
    assert names == ["sensor_unpowered", "degauss_p1", "setup", "point_1",
                     "point_2", "standby_final"]
    # the layout the analysis scripts read
    assert glob.glob(os.path.join(rx, "dg_pass1", "dg_*",
                                  "degauss_session.json"))
    for p in doc["points"]:
        pdir = os.path.join(rx, p["dir_name"])
        assert os.path.exists(os.path.join(pdir, "qzfm_session.json"))
        metas = [m for m in glob.glob(os.path.join(pdir, "R*",
                                                   "*.meta.json"))]
        assert len(metas) == 1
        with open(metas[0], encoding="utf-8") as fh:
            assert json.load(fh)["status"] == "ok"
    assert os.path.exists(os.path.join(rx, "standby_final",
                                       "qzfm_standby_session.json"))
    assert os.path.exists(os.path.join(rx, "relax_summary.txt"))
    assert os.path.exists(os.path.join(parent, "relax_session.log"))
    # every hand step went through the UI
    assert ctx.ui.asked and "19V" in ctx.ui.asked[0]
    assert any("UNPLUG" in m for m in ctx.ui.paused)
    assert any("19V switch OFF" in m for m in ctx.ui.paused)


def test_go_degauss_fault_aborts_before_points(sim_lab, tmp_path):
    ctx, lab = sim_lab(60.0)
    lab.world.inject(dg_fault_at_s=5.0)
    parent = str(tmp_path / "relax")
    code = run(ctx, lab, quick(outdir=parent), "go")
    assert code == 20
    _rx, doc = session_doc(parent)
    assert doc["outcome"].startswith("aborted: degauss pass 1 exited 30")
    assert doc["points"] == [] and doc["t0"] is None
    assert doc["steps"][-1]["outcome"] == "skipped (wake never attempted)"


class StopAtPause(AutoUI):
    """The operator presses Abort at the post-degauss pause."""

    def __init__(self):
        super().__init__()
        self.ctx = None

    def acknowledge(self, message):
        super().acknowledge(message)
        if "UNPLUG" in message:
            self.ctx.request_stop()


def test_stop_still_shuts_the_sensor_down(sim_lab, tmp_path):
    ctx, lab = sim_lab(60.0)
    ui = StopAtPause()
    ctx.ui = ui
    ui.ctx = ctx
    parent = str(tmp_path / "relax")
    code = run(ctx, lab, quick(outdir=parent, checkout_seconds=20), "go")
    assert code == 130
    _rx, doc = session_doc(parent)
    assert doc["outcome"] == "interrupted"
    # the checkout was attempted, so the final standby ran and took
    assert doc["final_standby"]["dark_confirmed"] is True
    assert doc["duty_cycle"]["final_state"] == "asleep"
