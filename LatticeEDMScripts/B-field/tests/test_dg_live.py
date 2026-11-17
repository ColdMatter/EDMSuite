"""The live degauss window, headless (Qt offscreen), on a simulated cycle."""

import json
import os

import pytest

os.environ.setdefault("QT_QPA_PLATFORM", "offscreen")
pytest.importorskip("PySide6.QtWidgets")
pytest.importorskip("pyqtgraph")

from bfield.gui.dg_live import DgLiveWindow  # noqa: E402
from bfield.operations.degauss_cycle import (DegaussSession,  # noqa: E402
                                             DegaussSettings)


def settings(tmp_path, **kw):
    base = dict(mode="run", port="COM5", amp=8.0, coil="parallel4",
                taper=20.0, soak=5.0, confirm_checklist=True,
                outdir=str(tmp_path / "dg"))
    base.update(kw)
    return DegaussSettings(**base)


def doc_rows(sess):
    with open(os.path.join(sess.outdir, "degauss_session.json")) as fh:
        return json.load(fh)["cycles"][0]["n_rows"]


def test_window_draws_the_cycle(sim_lab, tmp_path):
    ctx, lab = sim_lab(5.0)
    live = DgLiveWindow("test")
    assert live.win is None                 # opens at the first cycle
    sess = DegaussSession(ctx, lab, settings(tmp_path), live=live)
    assert sess.run() == 0
    assert sess.live is live                # never dropped
    tr = live.traces
    assert len(tr.I) == tr.n == doc_rows(sess) > 1000
    assert abs(max(abs(tr.I)) - 8.0) < 0.1
    vx, iy = tr.loops[0].getData()
    assert len(vx) > 1000
    # ramping, soaking, taper, cooldown at least, each marked
    assert len(tr.phase_lines) >= 2 * 4
    assert "outcome: ok" in tr.label.text()
    path = str(tmp_path / "live.png")
    live.win.grab().save(path)
    assert os.path.getsize(path) > 5_000
    live.close()


def test_check_overlays_the_loops(sim_lab, tmp_path):
    ctx, lab = sim_lab(5.0)
    live = DgLiveWindow("test")
    sess = DegaussSession(ctx, lab, settings(tmp_path, mode="check", amp=None,
                                             amps=[2.0, 4.0]), live=live)
    assert sess.run() == 0
    assert len(live.loops) == 2
    assert len(live.traces.legend.items) == 2
    live.close()


def test_replay_overlays_saved_streams(sim_lab, tmp_path, monkeypatch):
    from bfield.gui import dg_live
    ctx, lab = sim_lab(60.0)
    sess = DegaussSession(ctx, lab, settings(tmp_path, taper=10.0, soak=2.0))
    assert sess.run() == 0
    csv_path = os.path.join(sess.outdir, "stream_c01.csv")
    shown = {}

    def fake_hold(self, say=None):
        shown["loops"] = len(self.loops)
        shown["n"] = self.traces.n
        self.close()
    monkeypatch.setattr(dg_live.DgLiveWindow, "hold", fake_hold)
    dg_live.replay([csv_path, csv_path], say=lambda *a: None)
    assert shown["loops"] == 2 and shown["n"] > 1000
