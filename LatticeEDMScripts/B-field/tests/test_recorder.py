"""The recorder child in the simulated lab: files, meta, quality gates."""

import glob
import json
import os
import subprocess
import sys

import pytest

from bfield.acquisition.child import new_process_group_kwargs
from conftest import heads_on

PROJECT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
OLD_META = glob.glob(r"C:/Users/cafmot/Desktop/IROP_research/result/"
                     r"twohead_sync02/R01_1242/*.meta.json")


def record(lab, outdir, *extra, seconds=5, topology="--2head"):
    kw = new_process_group_kwargs()
    cmd = [sys.executable, "-m", "bfield.acquisition.recorder", topology,
           "--sim", "--speed", "20", "--sim-dir", lab.sim_dir,
           "--seconds", str(seconds), f"--outdir={outdir}", *extra]
    return subprocess.run(cmd, capture_output=True, text=True, cwd=PROJECT,
                          timeout=120, **kw)


@pytest.fixture
def lab(sim_lab):
    _ctx, lab = sim_lab(20.0)
    return lab


def meta_of(outdir):
    return json.load(open(glob.glob(os.path.join(outdir,
                                                 "*.meta.json"))[0]))


def test_dead_sensor_is_flat(lab, tmp_path):
    out = str(tmp_path / "flat")
    r = record(lab, out)
    assert r.returncode == 31, r.stdout + r.stderr
    assert meta_of(out)["status"] == "flagged"


def test_clean_two_head_record(lab, tmp_path):
    heads_on(lab)
    out = str(tmp_path / "ok")
    r = record(lab, out, "--cal-factors", "z:0.8,y:0.77,z2:0.82,y2:0.79",
               seconds=6)
    assert r.returncode == 0, r.stdout + r.stderr
    names = sorted(os.listdir(out))
    assert names == ["ok.meta.json", "ok_Y2_pT.txt", "ok_Y_pT.txt",
                     "ok_Z2_pT.txt", "ok_Z_pT.txt", "ok_raw.csv"]
    meta = meta_of(out)
    assert meta["status"] == "ok" and meta["n_samples"] == 6 * 1200
    # analyses gate on these two exact values (cross_spectrum.py needs
    # qzfm_3axis.py; runs_compare & co. refuse "simulate" by default)
    assert meta["software_legacy"] == "qzfm_3axis.py"
    assert meta["source"] == "simulate"
    assert meta["cal_factors"] == {"z": 0.8, "y": 0.77, "z2": 0.82,
                                   "y2": 0.79}
    head = open(os.path.join(out, "ok_Z_pT.txt")).readline()
    assert head.startswith("# QZFM acquisition | channel=Z head=a session=ok")


@pytest.mark.skipif(not OLD_META, reason="no old meta.json on this machine")
def test_meta_has_every_key_the_old_recorder_wrote(lab, tmp_path):
    heads_on(lab)
    out = str(tmp_path / "keys")
    assert record(lab, out, seconds=3).returncode == 0
    old = json.load(open(OLD_META[0]))
    new = meta_of(out)
    assert set(old) <= set(new), set(old) - set(new)


def test_railed_output_is_30(lab, tmp_path):
    heads_on(lab)
    lab.world.inject(rail_axis=["ai1"])
    # at +/-1 V the ADC window clips first: the ADC-rail verdict
    out = str(tmp_path / "rail")
    r = record(lab, out, "--vrange", "1")
    assert r.returncode == 30, r.stdout + r.stderr
    assert "ADC input rail" in meta_of(out)["error"]["message"]
    # a window wider than the sensor's soft rail sees the QZFM rail itself
    out5 = str(tmp_path / "rail5")
    r = record(lab, out5, "--vrange", "5")
    assert r.returncode == 30, r.stdout + r.stderr
    assert "QZFM analog output rail" in meta_of(out5)["error"]["message"]


def test_nonfinite_block_is_32(lab, tmp_path):
    heads_on(lab)
    lab.world.inject(nonfinite_block=2)
    out = str(tmp_path / "nan")
    r = record(lab, out)
    assert r.returncode == 32, r.stdout + r.stderr


def test_refuses_to_overwrite_a_run(lab, tmp_path):
    heads_on(lab)
    out = str(tmp_path / "twice")
    assert record(lab, out, seconds=2).returncode == 0
    assert record(lab, out, seconds=2).returncode == 13


def test_bad_map_is_13(lab, tmp_path):
    r = record(lab, str(tmp_path / "m"), "--map", "z:a:ai0,,y:a:ai1")
    assert r.returncode == 13
