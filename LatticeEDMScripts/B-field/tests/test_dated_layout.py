"""The dated run-folder layout: <data_root>/Oct 2026/15/001_SingleSensorZ."""

import os

import pytest

from bfield.io import files
from bfield.procedures.qzfm_session import QZFMPlan, QZFMSession


def test_day_dir_and_numbering(tmp_path, monkeypatch):
    monkeypatch.setattr(files.clock, "strftime",
                        lambda fmt: {"%m": "10", "%Y": "2026",
                                     "%d": "05"}[fmt])
    day = files.day_dir(str(tmp_path))
    assert day == os.path.join(str(tmp_path), "Oct 2026", "05")
    first = files.dated_run_dir(str(tmp_path), "Single Sensor Z!")
    assert os.path.basename(first) == "001_Single-Sensor-Z"
    assert os.path.isdir(first)
    # numbered through the day, across names; gaps and strays ignored
    os.makedirs(os.path.join(day, "007_Degauss"))
    os.makedirs(os.path.join(day, "notes"))
    open(os.path.join(day, "12_not_a_run.txt"), "w").close()
    assert os.path.basename(files.dated_run_dir(str(tmp_path), "X")) \
        == "008_X"
    with pytest.raises(ValueError):
        files.dated_run_dir(str(tmp_path), "  ***  ")


def test_clean_run_name():
    assert files.clean_run_name("  AB3S alone, master off ") == \
        "AB3S-alone-master-off"
    assert files.clean_run_name("Sf/Sweep:axial") == "SfSweepaxial"
    assert len(files.clean_run_name("a" * 200)) == 60


def test_record_goes_to_the_dated_folder(sim_lab):
    ctx, lab = sim_lab(60.0)
    lab.config["paths"]["layout"] = "dated"
    p = QZFMPlan.for_sensors(lab.config, [2], seconds=20)
    assert QZFMSession(ctx, lab, p).record() == 0
    day = files.day_dir(lab.data_root)
    assert os.path.dirname(p.outdir) == day
    assert os.path.basename(p.outdir) == "001_SingleSensorZ"
    assert any(d.startswith("R01_") for d in os.listdir(p.outdir))
    # the next one counts up, and a given name is used
    p2 = QZFMPlan.for_sensors(lab.config, [1, 2], seconds=20,
                              name="both sensors")
    assert QZFMSession(ctx, lab, p2).record() == 0
    assert os.path.basename(p2.outdir) == "002_both-sensors"
    # the Analysis tab opens the record inside a run folder
    from bfield.gui.analysis_panel import record_inside
    inner = record_inside(p.outdir)
    assert inner and os.path.basename(inner).startswith("R01_")


def test_old_layout_still_available(sim_lab):
    ctx, lab = sim_lab(60.0)
    lab.config["paths"]["layout"] = "buckets"
    p = QZFMPlan.for_sensors(lab.config, [1], seconds=20)
    assert QZFMSession(ctx, lab, p).record() == 0
    assert os.path.dirname(p.outdir) == lab.data_root
    assert os.path.basename(p.outdir).startswith("newdata")
