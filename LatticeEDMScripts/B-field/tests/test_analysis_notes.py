"""bfield.io.runs.run_notes: what a run recorded about its quality."""

import json

from bfield.io.runs import channel_flags, run_notes


def test_channel_flags():
    meta = {"vrange": 1.0, "rail_fraction": {"z": 0.0, "y": 0.774},
            "qzfm_rail_fraction": {"z": 0.0, "y": 0.0},
            "pinned_block_fraction": {"z": 0.0, "y": 0.75},
            "n_nonfinite": {"z": 0, "y": 0},
            "flat_ptp_V": {"z": 0.05, "y": 0.0},
            "cal_factors": {"z": 1.7, "y": 0.8}}
    fl = channel_flags(meta)
    assert set(fl) == {"z", "y"}
    assert [lvl for lvl, _ in fl["y"]] == ["bad", "bad", "bad"]
    assert "77.4%" in fl["y"][0][1]
    assert fl["z"] == [("warn", fl["z"][0][1])] and "1.7" in fl["z"][0][1]


def test_run_notes_reads_meta_session_and_notes_files(tmp_path):
    sess = tmp_path / "session"
    run = sess / "R01_1200"
    run.mkdir(parents=True)
    (run / "R01_1200.meta.json").write_text(json.dumps({
        "status": "flagged", "exit_code": 30,
        "error": {"code": 30, "message": "ADC input rail on: y"},
        "warnings": ["cal factor z:1.7 exceeds 1.5"],
        "notes": "door open", "rail_fraction": {"y": 0.5}}))
    (sess / "qzfm_session.json").write_text(json.dumps({
        "setup": {"warnings": ["head a: no factors"],
                  "sync": {"verdict": "not synced", "why": "beat 0.8 Hz"}},
        "heals": [{"time": "12:00", "why": "guard exit 30"}],
        "runs": [{"dir": "result/session/R01_1200", "outcome": "flagged",
                  "exit_code": 30}],
        "outcome": "recorder exit 30"}))
    (run / "NOTES.md").write_text("do not use: cal 15.90")
    n = run_notes(str(run))
    assert n["reason"] == "ADC input rail on: y"
    assert n["meaning"].startswith("saturation")
    assert n["operator_notes"] == "door open"
    assert "y" in n["channels"]
    assert any("not synced" in s for s in n["session"])
    assert any("guard exit 30" in s for s in n["session"])
    assert any("run outcome in the session: flagged" in s
               for s in n["session"])
    assert n["note_files"][0][1] == "do not use: cal 15.90"
