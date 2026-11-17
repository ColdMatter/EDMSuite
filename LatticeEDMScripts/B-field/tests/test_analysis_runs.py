"""bfield.io.runs: header parsing round-trips the recorder's header format."""

import json

import numpy as np

from bfield.io import runs

# exactly the header qzfm_record.py writes (Recorder.open)
PT_HEADER = (
    "# QZFM acquisition | channel=Z head=a session=R01_1242\n"
    "# start=2026-08-07T12:42:11 fs=1200.0 Hz gain=1x cal=370.4 pT/V "
    "vrange=2.0 V\n"
    "# device=Dev2 ch=Dev2/ai0 sensor=Z3D0-AB3Q source=analog(USB-6008)\n"
    "# one value per line, unit=pT\n")
AUX_HEADER = (
    "# QZFM acquisition | aux=MON session=R01_1242\n"
    "# start=2026-08-07T12:42:11 fs=1200.0 Hz vrange=2.0 V\n"
    "# device=Dev2 ch=Dev2/ai3 source=analog(USB-6008)\n"
    "# one value per line, unit=V (raw ADC volts, uncalibrated)\n")


def make_run(tmp_path):
    run = tmp_path / "R01_1242"
    run.mkdir()
    (run / "R01_1242_Z_pT.txt").write_text(PT_HEADER + "1.25\n-3.50\n")
    (run / "R01_1242_MON_V.txt").write_text(AUX_HEADER + "0.01\n0.02\n")
    (run / "R01_1242.meta.json").write_text(json.dumps({"fs_hz": 1200.0,
                                                         "status": "ok"}))
    return run


def test_pt_header_round_trip(tmp_path):
    run = make_run(tmp_path)
    h = runs.read_channel_header(str(run / "R01_1242_Z_pT.txt"))
    assert h == {"kind": "pT", "channel": "Z", "head": "a",
                 "session": "R01_1242", "start": "2026-08-07T12:42:11",
                 "fs": 1200.0, "gain": "1x", "cal": 370.4, "vrange": 2.0,
                 "device": "Dev2", "ch": "Dev2/ai0", "sensor": "Z3D0-AB3Q",
                 "source": "analog(USB-6008)", "unit": "pT"}


def test_aux_header_and_channels(tmp_path):
    run = make_run(tmp_path)
    h = runs.read_channel_header(str(run / "R01_1242_MON_V.txt"))
    assert h["kind"] == "V" and h["channel"] == "MON" and h["unit"] == "V"
    loaded = runs.load_run(str(run))
    assert loaded["meta"]["status"] == "ok"
    assert sorted(loaded["channels"]) == ["mon", "z"]
    np.testing.assert_allclose(loaded["channels"]["z"], [1.25, -3.5])


def test_old_two_head_header(tmp_path):
    p = tmp_path / "R_Y2_pT.txt"
    p.write_text("# QZFM two-head acquisition | channel=Y2 head=b(A10QHJ1ZA)"
                 " session=R\n# start=x fs=1200.0 Hz gain=1x cal=370.4 pT/V"
                 " vrange=2.0 V\n# one value per line, unit=pT\n0.5\n")
    h = runs.read_channel_header(str(p))
    assert h["kind"] == "pT" and h["head"] == "b(A10QHJ1ZA)"


def test_find_runs(tmp_path):
    make_run(tmp_path)
    (tmp_path / "empty").mkdir()
    assert [p.replace("\\", "/").rsplit("/", 1)[-1]
            for p in runs.find_runs(str(tmp_path))] == ["R01_1242"]
