"""The degausser session in the simulated lab: pass, refuse, fault, stop."""

import json
import os
import threading

from bfield.operations.degauss_cycle import (DegaussSession, DegaussSettings,
                                             qc_metrics, verified_readback)
from bfield.errors import ReadbackError

import pytest


def settings(tmp_path, **kw):
    base = dict(mode="run", port="COM5", amp=8.0, coil="parallel4",
                taper=60.0, confirm_checklist=True,
                outdir=str(tmp_path / "dg"))
    base.update(kw)
    return DegaussSettings(**base)


def doc_of(sess):
    return json.load(open(os.path.join(sess.outdir, "degauss_session.json")))


def test_full_cycle_passes_qc_and_gives_t0(sim_lab, tmp_path):
    ctx, lab = sim_lab(60.0)
    sess = DegaussSession(ctx, lab, settings(tmp_path))
    assert sess.run() == 0
    d = doc_of(sess)
    assert d["outcome"] == "completed" and d["t0"]
    cyc = d["cycles"][0]
    assert cyc["qc"]["verdict"] == "pass"
    assert cyc["qc"]["taper_measured_s"] == pytest.approx(60, abs=3)
    assert os.path.exists(os.path.join(sess.outdir, cyc["stream_csv"]))


def test_firmware_clamp_is_refused_before_driving(sim_lab, tmp_path):
    ctx, lab = sim_lab(60.0)
    # 9 A is inside max_amp 10 but the unit clamps it to 8.0 silently
    sess = DegaussSession(ctx, lab, settings(tmp_path, amp=9.0, max_amp=10.0))
    assert sess.run() == 32
    d = doc_of(sess)
    assert d["cycles"][0]["outcome"] == "refused-readback"
    assert d["cycles"][0]["trigger"] is None      # nothing driven


def test_unit_fault_is_flagged(sim_lab, tmp_path):
    ctx, lab = sim_lab(60.0)
    lab.world.inject(dg_fault_at_s=20.0)
    sess = DegaussSession(ctx, lab, settings(tmp_path))
    assert sess.run() == 30
    qc = doc_of(sess)["cycles"][0]["qc"]
    assert qc["verdict"] == "fail" and qc["fault"]
    assert doc_of(sess)["t0"] is None


def test_stop_mid_cycle_aborts_and_confirms_idle(sim_lab, tmp_path):
    ctx, lab = sim_lab(20.0)
    sess = DegaussSession(ctx, lab, settings(tmp_path))
    threading.Timer(1.5, ctx.request_stop).start()
    assert sess.run() == 130
    d = doc_of(sess)
    assert d["outcome"] == "interrupted"
    assert d["abort"] == {"attempted": True, "acked": True,
                          "confirmed": "idle"}


def test_checklist_declined_drives_nothing(sim_lab, tmp_path):
    ctx, lab = sim_lab(60.0)
    ctx.ui.confirm = lambda q: False
    sess = DegaussSession(ctx, lab, settings(tmp_path,
                                             confirm_checklist=False))
    assert sess.run() == 13
    assert sess.outdir is None                    # not even a folder


def test_readback_gate_unit():
    req = {"current": 2.0, "frequency": 15.0, "soaktime": 15.0,
           "duration": 300.0}
    assert verified_readback(req, dict(req), 8.0)["current"] == 2.0
    with pytest.raises(ReadbackError):
        verified_readback(req, dict(req, current=None), 8.0)
    with pytest.raises(ReadbackError):
        verified_readback(req, dict(req, frequency=3.0), 8.0)


def test_qc_fails_closed_without_a_current_column():
    rows = [(0.0, 1.0)] * 100
    assert qc_metrics(rows, ["time", "x"])["verdict"] == "fail"
