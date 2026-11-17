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


# -- the waveform figure and the live window ---------------------------

class FakeLive:
    """Records what the session tells the live window."""

    def __init__(self, explode_on=None):
        self.calls = []
        self.explode_on = explode_on
        self.max_rows = 0

    def _rec(self, name, *a):
        self.calls.append(name)
        if name == self.explode_on:
            raise RuntimeError("display broke")

    def start_cycle(self, idx, title, label=None):
        self._rec("start_cycle", idx, title)

    def update(self, rows, columns):
        self.max_rows = max(self.max_rows, len(rows))
        self._rec("update")

    def end_cycle(self, rec):
        self._rec("end_cycle", rec)

    def wait(self, seconds):
        self._rec("wait")
        from bfield import clock
        clock.sleep(seconds)


def test_cycle_saves_its_waveform_figure(sim_lab, tmp_path):
    ctx, lab = sim_lab(60.0)
    sess = DegaussSession(ctx, lab, settings(tmp_path))
    assert sess.run() == 0
    cyc = doc_of(sess)["cycles"][0]
    assert cyc["figure"] == "stream_c01.png"
    png = os.path.join(sess.outdir, cyc["figure"])
    assert os.path.getsize(png) > 10_000


def test_live_window_sees_the_whole_cycle(sim_lab, tmp_path):
    ctx, lab = sim_lab(60.0)
    live = FakeLive()
    sess = DegaussSession(ctx, lab, settings(tmp_path), live=live)
    assert sess.run() == 0
    assert live.calls[0] == "start_cycle" and live.calls[-1] == "end_cycle"
    assert live.calls.count("update") > 3 and "wait" in live.calls
    assert live.max_rows == doc_of(sess)["cycles"][0]["n_rows"]


def test_check_starts_one_live_cycle_per_amplitude(sim_lab, tmp_path):
    ctx, lab = sim_lab(60.0)
    live = FakeLive()
    sess = DegaussSession(ctx, lab, settings(tmp_path, mode="check",
                                             amps=[1.0, 2.0], amp=None),
                          live=live)
    assert sess.run() == 0
    assert live.calls.count("start_cycle") == 2
    assert live.calls.count("end_cycle") == 2


def test_a_broken_live_window_never_breaks_the_cycle(sim_lab, tmp_path):
    ctx, lab = sim_lab(60.0)
    sess = DegaussSession(ctx, lab, settings(tmp_path),
                          live=FakeLive(explode_on="update"))
    assert sess.run() == 0
    d = doc_of(sess)
    assert d["outcome"] == "completed"
    assert any("live plot stopped" in w for w in d["warnings"])
    assert sess.live is None


def test_aborted_cycle_still_gets_its_figure(sim_lab, tmp_path):
    ctx, lab = sim_lab(60.0)
    lab.world.inject(dg_fault_at_s=20.0)
    live = FakeLive()
    sess = DegaussSession(ctx, lab, settings(tmp_path), live=live)
    assert sess.run() == 30
    cyc = doc_of(sess)["cycles"][0]
    assert cyc["figure"] == "stream_c01.png"
    assert live.calls[-1] == "end_cycle"


def test_figure_from_a_saved_stream(sim_lab, tmp_path):
    from bfield.analysis.dg_waveform import (figure_for_stream, load_stream,
                                             rows_to_arrays, session_info)
    ctx, lab = sim_lab(60.0)
    sess = DegaussSession(ctx, lab, settings(tmp_path))
    assert sess.run() == 0
    csv_path = os.path.join(sess.outdir, "stream_c01.csv")
    cols, rows = load_stream(csv_path)
    I, V, st = rows_to_arrays(rows, cols)
    assert abs(max(abs(I)) - 8.0) < 0.1
    info = session_info(csv_path)
    assert info["rate_hz"] and "8 A" in info["title"]
    out = figure_for_stream(csv_path, str(tmp_path / "replay.png"))
    assert os.path.getsize(out) > 10_000


class SlowLive(FakeLive):
    """A live window whose redraw takes `cost` real seconds."""

    def __init__(self, cost):
        super().__init__()
        self.cost = cost

    def update(self, rows, columns):
        import time
        time.sleep(self.cost)
        super().update(rows, columns)


def test_a_slow_live_window_never_overflows_the_stream(sim_lab, tmp_path):
    # bench 2026-11-04: --plot redraws slowed the polls, the 400-row
    # drain fell behind ~398 rows/s, and tio's data.iter() called
    # os._exit(0) in cooldown -- nothing saved. At x5 a 0.3 s redraw is
    # ~520 rows per poll: the old drain overflowed, the queue drain keeps up
    ctx, lab = sim_lab(5.0)
    sess = DegaussSession(ctx, lab, settings(tmp_path), live=SlowLive(0.3))
    assert sess.run() == 0
    d = doc_of(sess)
    cyc = d["cycles"][0]
    assert d["outcome"] == "completed" and cyc["outcome"] == "ok"
    assert cyc["stream_queue_peak"] < 950
    assert not any("queue" in w for w in d["warnings"])


def test_sim_tio_iterator_exits_on_a_full_queue():
    # the simulator keeps tio's trap, so a regression to data.iter() shows
    from bfield import clock
    from bfield.devices.simulated import SimTioExit, dg_backend

    class World:
        def read(self):
            return {}

        def update(self, f):
            pass

    dev = dg_backend(World())("COM5")
    it = dev.data.iter()
    next(it)
    clock.sleep(4.0)               # ~1400 rows arrive unread
    with pytest.raises(SimTioExit):
        next(it)
    assert dev._tio.pub_queue.qsize() == 1000 and dev.tossed > 0


def test_figure_is_drawn_out_of_process(sim_lab, tmp_path, monkeypatch):
    # bench 2026-11-04: an in-process matplotlib draw starved tio's serial
    # thread (~0.3 s) and tio ended the process before the json was saved
    from bfield.analysis import dg_waveform

    def in_process(*a, **k):
        raise AssertionError("figure drawn in the session's process")
    monkeypatch.setattr(dg_waveform, "save_figure", in_process)
    ctx, lab = sim_lab(60.0)
    sess = DegaussSession(ctx, lab, settings(tmp_path))
    assert sess.run() == 0
    cyc = doc_of(sess)["cycles"][0]
    assert cyc["figure"] == "stream_c01.png"
    assert os.path.exists(os.path.join(sess.outdir, "stream_c01.png"))


def test_cycle_logs_the_pc_clock(sim_lab, tmp_path):
    ctx, lab = sim_lab(20.0)
    sess = DegaussSession(ctx, lab, settings(tmp_path))
    assert sess.run() == 0
    cyc = doc_of(sess)["cycles"][0]
    timing = cyc["pc_timing"]
    # the simulated unit streams 348 rows/s of its own (= sim) time
    assert abs(timing["arrival_rate_hz"] - 348.0) < 20
    assert timing["rpc_probes"] >= 1 and timing["rpc_ms_max"] is not None
    soak = next(s for s in cyc["status_timeline"] if s["phase"] == "soaking")
    assert abs(soak["pc_seconds"] - soak["seconds"]) < 1.5


def test_pc_seconds_show_a_slow_unit():
    # 400 rows per unit second arriving at 348/s: the 15 s soak (6000
    # rows) takes ~17.2 s on the PC clock
    from bfield.operations.degauss_cycle import phase_timeline, pc_timing
    rows = [(0.0, 0.0, 0.0, 6.0)] * 6000 + [(0.0, 0.0, 0.0, 7.0)] * 400
    arrivals = [[k * 0.5, min(int(k * 0.5 * 348), len(rows))]
                for k in range(0, 37)]
    tl = phase_timeline(rows, ["degauss.A", "degauss.V", "degauss.Ohm",
                               "status"], rate_hz=400.0, arrivals=arrivals)
    assert tl[0]["seconds"] == 15.0
    assert abs(tl[0]["pc_seconds"] - 6000 / 348) < 0.6
    assert abs(pc_timing(arrivals, [])["arrival_rate_hz"] - 348) < 5


def test_decimation_is_set_for_the_run_and_put_back(sim_lab, tmp_path):
    ctx, lab = sim_lab(20.0)
    sess = DegaussSession(ctx, lab, settings(tmp_path, decimation=2))
    assert sess.run() == 0
    d = doc_of(sess)
    assert d["stream_before"]["decimation"] == 1
    assert d["stream_decimation"] == {"before": 1, "set": 2, "readback": 2,
                                      "restored": 1}
    cyc = d["cycles"][0]
    assert cyc["outcome"] == "ok"
    # half the rows: the soak anchor sees ~174 rows/s, the PC clock agrees
    assert abs(cyc["qc"]["stream_rate_hz"] - 174) < 10
    assert abs(cyc["pc_timing"]["arrival_rate_hz"] - 174) < 10


def test_decimation_only_for_drive_modes():
    from bfield.errors import ConfigError
    from bfield.operations.degauss_cycle import validate_settings
    with pytest.raises(ConfigError):
        validate_settings(DegaussSettings(mode="status", port="COM5",
                                          decimation=2))
    with pytest.raises(ConfigError):
        validate_settings(DegaussSettings(mode="run", port="COM5", amp=8.0,
                                          coil="parallel", decimation=0))


def test_20hz_is_not_flagged():
    from bfield.operations.degauss_cycle import validate_settings
    notes = validate_settings(DegaussSettings(mode="run", port="COM5",
                                              amp=8.0, freq=20.0,
                                              coil="parallel"))
    assert not any("freq" in n for n in notes)
