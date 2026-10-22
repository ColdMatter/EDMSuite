"""The GUI, driven headless (Qt offscreen) in the simulated lab."""

import os
import time

import pytest

os.environ.setdefault("QT_QPA_PLATFORM", "offscreen")
QtWidgets = pytest.importorskip("PySide6.QtWidgets")

from bfield import clock  # noqa: E402
from bfield.gui import main_window as mw  # noqa: E402


@pytest.fixture(scope="module")
def app():
    a = QtWidgets.QApplication.instance() or QtWidgets.QApplication([])
    yield a


@pytest.fixture
def win(app, tmp_path):
    w = mw.MainWindow(sim_dir=str(tmp_path / "sim"))
    w.speed.setValue(60)
    yield w
    if w.running():
        w.abort()
        wait_done(app, w, 60)
    clock.set_speed(1.0)
    w.close()
    # delete it while Qt is up: left to interpreter exit, pyqtgraph's
    # legend anchors fire on half-destroyed items and print tracebacks
    w.deleteLater()
    app.processEvents()


def wait_done(app, w, timeout, until=None):
    t0 = time.monotonic()
    while time.monotonic() - t0 < timeout:
        app.processEvents()
        if until is not None and until():
            return True
        if until is None and w.worker is None and w.last_code is not None:
            return w.last_code
        time.sleep(0.02)
    raise AssertionError("the GUI run did not finish in time; log tail:\n"
                         + w.log.toPlainText()[-3000:])


def form(w, cls):
    for i, f in enumerate(w.form_list):
        if isinstance(f, cls):
            w.forms.setCurrentIndex(i)
            return f
    raise KeyError(cls)


def test_bench_is_disabled(win):
    assert not mw.GUI_ALLOW_HARDWARE
    assert not win.rb_bench.isEnabled() and win.rb_sim.isChecked()


def test_apply_shows_sensors_without_running(app, win):
    f = form(win, mw.FORMS[0])
    assert win.forms.tabText(win.forms.currentIndex()) == "QZFM"
    f.pick.set_numbers([2])
    f.pick.apply_btn.click()
    app.processEvents()
    cards = win.sensor_panel.cards
    assert list(cards) == ["s2"] and "Sensor 2" in \
        cards["s2"].findChildren(QtWidgets.QLabel)[0].text()
    assert win.worker is None                    # nothing was started
    assert all(c.state == "pending" for c in cards["s2"].chips.values())
    # an impossible choice disables Apply and says why
    f.pick.set_numbers([])
    assert not f.pick.apply_btn.isEnabled() and f.pick.plan.text()
    # Defaults rebuilds the form; Apply still reaches the panel
    f.reset()
    f.pick.set_numbers([1, 2])
    f.pick.apply_btn.click()
    app.processEvents()
    assert set(win.sensor_panel.cards) == {"s1", "s2"}


def test_qzfm_record_with_live_data(app, win):
    f = form(win, mw.FORMS[0])
    f.quick_test()
    win.start()
    assert wait_done(app, win, 180) == 0
    assert win.live.total["V_s1z"] == 30 * 1200
    assert "finished: exit 0" in win.log.toPlainText()
    # the indicator cards: one per sensor, every stage through, the
    # calibration factors and the compensation shown
    cards = win.sensor_panel.cards
    assert set(cards) == {"s1", "s2"}
    a = cards["s1"]
    for stage in ("connect", "auto start", "gain / mode", "field zero",
                  "calibrate", "guard record", "sync check"):
        assert a.chips[stage].state == "passed", stage
    assert a.chips["standby"].state == "pending"
    assert a.factors["z"].text() == "0.80" and a.factors["y"].text() == "0.77"
    assert cards["s2"].factors["z"].text() == "0.82"
    assert a.comp["Bz_pT"].text() != "--"
    assert a.residual.text() != "--"
    assert "Sensor 1" in a.findChildren(QtWidgets.QLabel)[0].text()


def test_degauss_run(app, win):
    f = form(win, mw.FORMS[1])
    f.quick_test()
    win.start()
    assert wait_done(app, win, 180) == 0
    text = win.log.toPlainText()
    assert "[auto] yes -- Is the QZFM 19V power switch OFF" in text
    assert "t0 =" in text


def test_sweep_go_with_auto_operator(app, win):
    f = form(win, mw.FORMS[3])
    f.quick_test()
    f.mode.setCurrentText("go")
    f.seed.setValue(1)
    win.start()
    assert wait_done(app, win, 300) == 0
    text = win.log.toPlainText()
    assert "[auto] generator set to 25 Hz, 450 mA" in text
    assert win.world.read().get("coil") is None      # drive left off


def test_relaxation_plan_then_go(app, win):
    f = form(win, mw.FORMS[2])
    win.start()
    assert wait_done(app, win, 60) == 0             # plan: touches nothing
    f.quick_test()
    f.mode.setCurrentText("go")
    win.last_code = None
    win.start()
    assert wait_done(app, win, 400) == 0
    assert "relaxation" in win.log.toPlainText().lower()


def test_abort_mid_record(app, win):
    f = form(win, mw.FORMS[0])
    f.seconds.setValue(3600)
    win.start()
    wait_done(app, win, 180, until=lambda: win.live.total["V_s1z"] > 0)
    win.abort()
    assert wait_done(app, win, 120) == 130


def test_manual_dialogs(app, win):
    """Auto-operator off: the checklist questions come up as dialogs."""
    from PySide6.QtCore import QTimer
    win.auto_op.setChecked(False)
    f = form(win, mw.FORMS[1])
    f.quick_test()
    seen = []

    def click_yes():
        box = QtWidgets.QApplication.activeModalWidget()
        if isinstance(box, QtWidgets.QMessageBox):
            seen.append(box.text())
            for b in box.buttons():
                if b.text() == "Yes":
                    b.click()
    timer = QTimer()
    timer.timeout.connect(click_yes)
    timer.start(100)
    win.start()
    try:
        assert wait_done(app, win, 180) == 0
    finally:
        timer.stop()
    assert len(seen) == 2 and "19V" in seen[0]


def test_master_choice_and_manage_sensors(app, tmp_path):
    """Add a sensor the way you would at the bench (simulated): plug it
    in, Manage sensors, Add, type the head ID, Scan USB, Save -- on a
    copy of the config, never the real config/sensors.toml."""
    import shutil
    from bfield.config import CONFIG_DIR, load_config
    from bfield.gui.sensor_manager import PortPicker, SensorManager
    for name in ("lab.toml", "sensors.toml"):
        shutil.copy(os.path.join(CONFIG_DIR, name), tmp_path / name)
    cfg = load_config(str(tmp_path / "lab.toml"))
    w = mw.MainWindow(config=cfg, sim_dir=str(tmp_path / "sim"))
    try:
        f = form(w, mw.FORMS[0])
        # the master choice: shown with two sensors, reaches the panel
        assert f.pick.master_row.isVisibleTo(f.pick)
        f.pick.set_master(2)
        f.pick.apply_btn.click()
        app.processEvents()
        cards = w.sensor_panel.cards
        assert cards["s2"].rec["modulation"] == "master"
        f.pick.set_numbers([1])
        assert not f.pick.master_row.isVisibleTo(f.pick)
        # a new sensor appears on a new port
        usb = w.world.plug_in_new_head()
        from bfield.sensors import load_sensors
        dlg = SensorManager(w, w.config, load_sensors(w.config), w.scan_usb)
        dlg.add_sensor()
        r = dlg.table.rowCount() - 1
        dlg.table.item(r, 1).setText("Z3D0-NEW9")
        dlg.table.item(r, 5).setText("test bench, lid")
        # its wiring: Z on ai0, Y on ai1 -- sensor 1's inputs, so the two
        # cannot run together, which the dialog points out
        for col, pin in ((3, "ai0"), (4, "ai1")):
            box = dlg.table.cellWidget(r, col)
            box.setCurrentIndex(box.findData(pin))
        assert "ai0 is used by sensor 1 Z and sensor 3 Z" in \
            dlg.sharing.text()
        ports, simulated = w.scan_usb()
        assert simulated and usb in [p[1] for p in ports]
        picker = PortPicker(dlg, ports, {"A10QHMQMA": "sensor 1",
                                         "A10QHJ1ZA": "sensor 2"}, True)
        assert picker.chosen() == usb            # the unowned port
        dlg.table.item(r, 2).setText(picker.chosen())
        dlg.save()
        assert dlg.saved
        w.reload_config()
        saved = open(tmp_path / "sensors.toml", encoding="utf-8").read()
        assert "Z3D0-NEW9" in saved and usb in saved
        assert [it for it in range(f.pick.list.count())
                if "Z3D0-NEW9" in f.pick.list.item(it).text()]
        # and it records in the simulator, alone, as sensor 3
        f.pick.set_numbers([3])
        f.quick_test()
        w.speed.setValue(60)
        w.start()
        assert wait_done(app, w, 180) == 0
        assert "Sensor 3" in w.sensor_panel.cards["s3"].findChildren(
            QtWidgets.QLabel)[0].text()
    finally:
        clock.set_speed(1.0)
        w.close()


def test_analysis_tab_and_figure_export(app, win, tmp_path):
    """Record, then the Analysis tab: Last record -> spectrum in fT/rtHz
    with the 0.05 Hz band and the 30-45 Hz floor; export PNG and SVG and
    copy to the clipboard, from Analysis and from Live data."""
    import numpy as np
    from bfield.gui import plot_export
    f = form(win, mw.FORMS[0])
    f.quick_test()
    f.seconds.setValue(60)
    win.start()
    assert wait_done(app, win, 180) == 0
    an = win.analysis
    assert an.last_btn.isEnabled()
    an.last_btn.click()
    assert set(an.chan_checks) == {"s1z", "s1y", "s2z", "s2y"}
    assert "sensor 2 (Z3D0-AB3S)" in an.chan_checks["s2z"].text()
    an.compute()
    wait_done(app, win, 120, until=lambda: an.result is not None)
    spectra, bands, meta, fs = an.result
    assert fs == 1200.0 and meta["source"] == "simulate"
    # the simulator's white floor is 23 fT/rtHz (+ lines, so the mean
    # over 10 Hz - Nyquist sits a little above it)
    assert 18 < bands["s1z"]["white"] < 40
    assert bands["s1z"]["white_median"] <= bands["s1z"]["white"]
    assert an.table.rowCount() == 4 and an.table.columnCount() == 2
    # a custom title, then the spectrum as CSV
    an.title_edit.setText("my title")
    assert an.plot.plotItem.titleLabel.text == "my title"
    from bfield.gui.analysis_panel import write_spectra_csv
    csv = tmp_path / "asd.csv"
    write_spectra_csv(str(csv), an.result, an.run_dir, {"split (Hz)": 1.0})
    lines = csv.read_text().splitlines()
    head = [ln for ln in lines if ln.startswith("#")]
    assert any("white noise" in ln and "mean" in ln for ln in head)
    cols = next(ln for ln in lines if ln.startswith("frequency_Hz"))
    assert cols == ("frequency_Hz,s1z_fT_per_rtHz,s1y_fT_per_rtHz,"
                    "s2z_fT_per_rtHz,s2y_fT_per_rtHz")
    rows = [ln for ln in lines if ln[:1].isdigit()]
    assert len(rows) == len(spectra["s1z"][0])
    # the full black frame, and the style applied
    assert an.plot.plotItem.getAxis("top").isVisible()
    assert an.plot.plotItem.getAxis("right").isVisible()
    assert an.plot.plotItem.getViewBox().border.widthF() == 2
    for widget, name in ((an.plot, "asd"), (win.live.plot, "live")):
        png, svg = tmp_path / f"{name}.png", tmp_path / f"{name}.svg"
        plot_export.export_png(widget, str(png))
        plot_export.export_svg(widget, str(svg))
        assert png.stat().st_size > 5000 and svg.stat().st_size > 1000
        img = plot_export.copy_image(widget)
        assert not img.isNull()
        assert not QtWidgets.QApplication.clipboard().image().isNull()
    # skipping more than the record is refused, not a crash
    from bfield.gui.analysis_panel import compute_spectra
    with pytest.raises(ValueError):
        compute_spectra(an.run_dir, ["s1z"], 200, 20, 1.0, skip_s=1e6)
    assert np.all(np.diff(spectra["s1z"][0]) > 0)
    # the Allan deviation of the same run: its settings, plot and numbers
    from bfield.gui.analysis_panel import ADEV, write_allan_csv
    an.kind.setCurrentText(ADEV)
    assert an.result is None and an.table.columnCount() == 4
    assert not an.asd_box.isVisibleTo(an) and an.adev_box.isVisibleTo(an)
    assert "Allan" in an.plot.plotItem.getAxis("left").labelText
    an.compute()
    wait_done(app, win, 120, until=lambda: an.result is not None)
    curves, abands, _, _ = an.result
    tau, adev, err = curves["s1z"]
    assert tau[-1] <= 60 / 4 + 1e-9 and np.all(err > 0)
    # white noise S alone would give S/sqrt(2) at 1 s; the simulator's
    # low-frequency noise (what tau ~ 1 s sees) only adds to it
    assert abands["s1z"]["sigma_1s"] > \
        0.8 * bands["s1z"]["white"] / np.sqrt(2)
    assert abands["s1z"]["min"] <= np.min(adev) + 1e-12
    assert an.table.rowCount() == 4
    assert an._fig_name().startswith("adev_")
    csv = tmp_path / "adev.csv"
    write_allan_csv(str(csv), an.result, an.run_dir, {"points per decade": 10})
    lines = csv.read_text().splitlines()
    assert any("sigma(1 s)" in ln for ln in lines if ln.startswith("#"))
    assert next(ln for ln in lines if ln.startswith("tau_s")).startswith(
        "tau_s,s1z_fT,s1z_err_fT,s1y_fT")
    # the coherence between the sensors: same axis pairs only by default
    from bfield.gui.analysis_panel import (COH, channel_pairs,
                                           compute_coherence,
                                           write_coherence_csv)
    an.kind.setCurrentText(COH)
    assert an.result is None and an.coh_box.isVisibleTo(an)
    assert not an.plot.plotItem.getAxis("left").logMode
    an.coh_seg.setValue(10)
    an.compute()
    wait_done(app, win, 120, until=lambda: an.result is not None)
    curves, cbands, _, _ = an.result
    assert set(curves) == {("s1z", "s2z"), ("s1y", "s2y")}
    f, c = curves[("s1z", "s2z")]
    assert f[1] == pytest.approx(0.1) and np.all((c >= 0) & (c <= 1 + 1e-9))
    assert 0 < cbands[("s1z", "s2z")]["level95"] < 1
    assert an.table.rowCount() == 2 and an._fig_name().startswith("coh_")
    csv = tmp_path / "coh.csv"
    write_coherence_csv(str(csv), an.result, an.run_dir, {"segment (s)": 10})
    assert next(ln for ln in csv.read_text().splitlines()
                if ln.startswith("frequency_Hz")) == \
        "frequency_Hz,s1z-s2z_coherence,s1y-s2y_coherence"
    assert len(channel_pairs(["s1z", "s1y", "s2z", "s2y"], False)) == 6
    assert channel_pairs(["z", "y", "z2", "y2"]) == [("z", "z2"), ("y", "y2")]
    with pytest.raises(ValueError):
        compute_coherence(an.run_dir, ["s1z", "s1y"])
    an.kind.setCurrentText(ADEV)
    assert an.plot.plotItem.getAxis("left").logMode
    # the Live data view over the last window
    win.live.view.setCurrentText("Allan deviation of the window")
    win.live.redraw()
    x, y = next(iter(win.live.curves.values())).getData()
    assert x is not None and len(x) > 5 and np.all(y > 0)
    assert "Allan deviation at 1 s" in win.live.info.text()


def test_analysis_shows_why_a_run_was_flagged(app, win):
    """A railed channel (fault injected in the simulator): the Analysis
    tab says why, marks and unticks the channel, and shows the notes."""
    f = form(win, mw.FORMS[0])
    f.quick_test()
    f.notes.setText("rail test, operator note")
    win.start()
    # after the setup (whose guard record would catch it and stop), once
    # the main record runs: sensor 1's Y input (ai1) goes to the rail
    wait_done(app, win, 300,
              until=lambda: "] recording " in win.log.toPlainText())
    win.world.inject(rail_axis=["ai1"])
    code = wait_done(app, win, 300)
    win.world.update(lambda d: d.update(inject={}))
    assert code == 30, win.log.toPlainText()[-1500:]
    an = win.analysis
    an.last_btn.click()
    assert "ADC input rail" in an.info.text()
    assert not an.chan_checks["s1y"].isChecked()
    assert "⚠" in an.chan_checks["s1y"].text()
    assert an.chan_checks["s1z"].isChecked()
    md = an.notes.toMarkdown()
    assert "rail test, operator note" in md and "Flagged channels" in md
