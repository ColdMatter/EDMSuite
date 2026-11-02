"""Every analysis tab of the GUI runs on synthetic scans and draws every plot
without errors (offscreen, no data files needed)."""

import os

import numpy as np
import pytest

os.environ.setdefault("QT_QPA_PLATFORM", "offscreen")
pytestmark = pytest.mark.slow   # imports the library (pythonnet + SharedCode.dll) and Qt

from analysis_gui import env
env.setup()
QtWidgets = pytest.importorskip("qtpy.QtWidgets")
from qtpy import QtCore

import tools
from analysis_fakes import four_shot_scan, onoff_scan


@pytest.fixture(scope="module")
def app():
    return QtWidgets.QApplication.instance() or QtWidgets.QApplication([])


@pytest.fixture
def make_tab(app, tmp_path, monkeypatch):
    messages = []
    for name in ("warning", "critical", "information"):
        monkeypatch.setattr(QtWidgets.QMessageBox, name,
                            staticmethod(lambda *a, n=name: messages.append((n, a[2] if len(a) > 2 else a))))

    def make(cls):
        qs = QtCore.QSettings(str(tmp_path / "settings.ini"), QtCore.QSettings.IniFormat)
        tab = cls(lambda: str(tmp_path), qs)
        tab.messages = messages
        return tab
    return make


def run_and_draw(tab, scans, settings=None, groups=None):
    tab.scans = scans
    for k, v in (settings or {}).items():
        tab.form.set_raw(k, v)
    if groups is not None:
        tab.groups.set_groups(groups)
    tab.run()
    assert not tab.messages, tab.messages
    assert tab.results is not None
    for page in tab.pages:
        page.refresh()
        assert page.last_error is None, page.title + "\n" + page.last_error
    return tab.results


def test_duration_tab(make_tab):
    from analysis_gui.modes.duration import DurationTab
    x = np.linspace(100, 5000, 20)
    extra = {"out:pluginName": "ScanMaster.Acquire.Plugins.PGOutputPlugin", "out:parameter": "v0Duration"}
    scans = {}
    for i, label in enumerate(["001_0", "001_1", "002_0", "002_1"]):
        on = 2*tools.exp_decay(x, 0.6, 1000. + 200*i, 0.4)
        scans[label] = onoff_scan(on, np.full(20, 2.), x, noise=0.01, seed=i, extra=extra)
    tab = make_tab(DurationTab)
    r = run_and_draw(tab, scans, {"SigStart": 20, "SigEnd": 30, "BkgStart": 60, "BkgEnd": 70})
    assert set(r["groups"]) == {"All good fits"}
    assert r["scans"]["001_0"]["fit"]["best fit"][1] == pytest.approx(1000., rel=0.05)
    r = run_and_draw(tab, scans, {"Exclude": "002_1"}, [("first", "001"), ("both", "001, 002")])
    assert r["groups"]["both"]["members"] == ["001_0", "001_1", "002_0"]
    assert len(tab.tables["groups"].rows) == 2


@pytest.mark.parametrize("xaxis", ["native", "frequency", "velocity"])
def test_setpoint_tab(make_tab, xaxis):
    from analysis_gui.modes.setpoint import SetpointTab
    sp = np.linspace(-1, 1, 21)
    scans = {}
    for k in range(3):
        on = 2*(1 - 0.3*np.exp(-(sp - 0.2)**2/(2*0.1**2)))
        order = slice(None) if k % 2 == 0 else slice(None, None, -1)   # up-down scan
        scans["004_%d" % k] = onoff_scan(on[order], np.full(21, 2.), sp[order], noise=0.01, seed=k,
                                         wm=np.full(21, 270.26))
    tab = make_tab(SetpointTab)
    r = run_and_draw(tab, scans, {"SigStart": 20, "SigEnd": 30, "BkgStart": 60, "BkgEnd": 70,
                                  "XAxis": xaxis})
    assert all(s["axis"]["mode"] == "setpoint" for s in r["scans"].values())
    assert r["average"] is not None and len(r["average"]["x"]) == 21
    assert len(r["stack"]["bin x"]) == 21   # auto bin width = scan step
    if xaxis == "native":
        assert r["fit"]["best fit"][0] == pytest.approx(0.2, abs=0.02)


def test_setpoint_tab_wm_lock(make_tab):
    from analysis_gui.modes.setpoint import SetpointTab
    sp = np.linspace(-100, 100, 21)   # GHz
    scans = {"007_0": onoff_scan(np.full(21, 1.5), np.full(21, 2.), sp, extra={"out:offset": 288.73},
                                 wm=np.full(21, 542.8))}
    tab = make_tab(SetpointTab)
    r = run_and_draw(tab, scans, {"SigStart": 20, "SigEnd": 30, "BkgStart": 60, "BkgEnd": 70,
                                  "XAxis": "native", "FitMode": "none"})
    assert r["scans"]["007_0"]["axis"]["mode"] == "WM lock"
    np.testing.assert_allclose(r["scans"]["007_0"]["x"], sp*1e3)


def test_slowing_tab(make_tab):
    from analysis_gui.modes.slowing import SlowingTab
    sp = np.linspace(-1.7, -0.7, 30)
    scans = {}
    for i, fid in enumerate(["011", "012"]):
        for k in range(3):
            off = 2*np.exp(-4*np.log(2)*(sp + 1.0)**2/0.2**2)
            on = 1.8*np.exp(-4*np.log(2)*(sp + 1.05 - 0.02*i)**2/0.2**2)
            order = slice(None) if k % 2 == 0 else slice(None, None, -1)
            scans["%s_%d" % (fid, k)] = four_shot_scan(on[order], off[order], sp[order], noise=0.01, seed=10*i + k)
    tab = make_tab(SlowingTab)
    r = run_and_draw(tab, scans, {"SigStart": 20, "SigEnd": 30})
    assert set(r["groups"]) == {"011 011", "012 012"}
    assert all(g["fit"] is not None for g in r["groups"].values())
    assert r["summary"]["011"]["unit"] == "m/s"
    r = run_and_draw(tab, scans, {"Exclude": "012_2", "TOFxMin": -100, "TOFxMax": 100},
                     [("both", "011+012"), ("each", "011, 012")])
    assert r["groups"]["both 011+012"]["members"] == ["011_0", "011_1", "011_2", "012_0", "012_1"]
    assert len(r["summary"]["each"]["X"]) == 2


def test_tof_collector_tab(make_tab):
    from analysis_gui.modes.tofcollector import TOFCollectorTab
    extra = {"out:pluginName": "ScanMaster.Acquire.Plugins.NullOutputPlugin"}
    scans = {"011_0": onoff_scan(np.full(10, 1.), np.full(10, 2.), np.arange(10), shots=1, noise=0.02,
                                 extra=extra),
             "012_0": four_shot_scan(np.full(10, 1.), np.full(10, 2.), np.arange(10), shots=1, noise=0.02)}
    tab = make_tab(TOFCollectorTab)
    r = run_and_draw(tab, scans, {"SigStart": 20, "SigEnd": 30, "BkgStart": 60, "BkgEnd": 70, "Chunk": 4})
    assert r["scans"]["011_0"]["ratio mean"] == pytest.approx(0.5, rel=0.02)
    assert r["scans"]["012_0"]["four shot"]
    assert r["scans"]["012_0"]["ratio mean"] == pytest.approx(0.5, rel=0.02)


def test_settings_errors_are_reported(make_tab):
    from analysis_gui.modes.duration import DurationTab
    tab = make_tab(DurationTab)
    tab.scans = {"001_0": onoff_scan([1.], [2.], [0.])}
    tab.form.set_raw("SigStart", "abc")
    tab.run()
    assert tab.messages and "Signal start" in str(tab.messages[0])
