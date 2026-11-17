"""Shared processing functions of LatticeEDM_analysis_library (used by the
analysis GUI), on synthetic scans. Where the library already had a function
doing the same calculation, the new one must give the same numbers."""

import numpy as np
import pytest

pytestmark = pytest.mark.slow   # imports the library (pythonnet + SharedCode.dll)

EDM = pytest.importorskip("LatticeEDM_analysis_library")
import tools
from analysis_fakes import (FakeScan, four_shot_scan, onoff_scan, scan_settings, time_axis)

GATES = (22., 30., 60., 70.)


def spp_to_gettofs(tofs):
    """[detector, point, time, shot] -> GetTOFs layout [detector, point*shot, time]"""
    D, P, T, S = tofs.shape
    return np.transpose(tofs, (0, 1, 3, 2)).reshape(D, P*S, T)


# ------------------------------------------------------------- gated counts

def test_gated_onoff_matches_GatedAvgCountsOnOff():
    rng = np.random.default_rng(1)
    A = onoff_scan(rng.uniform(1, 2, 12), rng.uniform(2, 3, 12), np.arange(12), shots=3, noise=0.05)
    _, raw = scan_settings(12, 3)
    t = time_axis()
    on_ref, off_ref = EDM.GatedAvgCountsOnOff(FakeScan(raw), A["on"][0], A["off"][0], t, t, *GATES)
    G = EDM.GatedOnOff(A, *GATES)
    np.testing.assert_allclose(G["on"], on_ref, rtol=1e-10)
    np.testing.assert_allclose(G["off"], off_ref, rtol=1e-10)
    np.testing.assert_allclose(G["ratio"], on_ref/off_ref, rtol=1e-10)


def test_gated_onoff_sem_is_spread_of_shots():
    A = onoff_scan([1.]*4, [2.]*4, np.arange(4), shots=5, noise=0.02, seed=3)
    G = EDM.GatedOnOff(A, *GATES)
    Tconv = A["settings"]["Tconv"]
    shots = EDM.GatedShots(A["on"][0], A["time on"], *GATES, Tconv=Tconv)
    np.testing.assert_allclose(G["on err"], np.std(shots, axis=1)/np.sqrt(5))
    assert np.all(G["on err"] > 0)


def test_gate_outside_tof_raises():
    A = onoff_scan([1.], [1.], [0.])
    with pytest.raises(ValueError, match="No TOF samples"):
        EDM.GatedOnOff(A, 100., 110., 60., 70.)


def test_missing_detector_raises():
    A = onoff_scan([1.], [1.], [0.])
    with pytest.raises(ValueError, match="detector"):
        EDM.GatedOnOff(A, *GATES, detector=1)


def test_four_shot_gated_matches_GetGatedAvgCounts4Shot():
    rng = np.random.default_rng(2)
    A = four_shot_scan(rng.uniform(1, 2, 10), rng.uniform(2, 3, 10), np.linspace(-1, 0, 10),
                       shots=2, noise=0.05)
    _, raw = scan_settings(10, 2)
    t = time_axis()
    for shot in ["on", "off"]:
        tofs = spp_to_gettofs(A[shot])
        ref = EDM.GetGatedAvgCounts4Shot(FakeScan(raw), tofs[0] - tofs[1], t, 27, 37)
        np.testing.assert_allclose(EDM.FourShotGated(A, 27, 37)[shot], ref, rtol=1e-10)


# --------------------------------------------------------------- Duration

def test_fit_decay_and_scattering_rate():
    x = np.linspace(0, 6000, 40)
    y = tools.exp_decay(x, 0.7, 1200., 0.3)
    fit = EDM.FitDecay(x, y + 0.001*np.random.default_rng(0).standard_normal(40), [1., 2000., 0.])
    assert fit["ok"]
    assert fit["best fit"][1] == pytest.approx(1200., rel=0.02)
    rate, err = EDM.ScatteringRate(1000., 100., 0.9)
    assert rate == pytest.approx(-1/(1000*np.log(0.9)))
    assert err == pytest.approx(rate*0.1)   # relative error of tau


def test_fit_decay_failure_is_reported_not_raised():
    fit = EDM.FitDecay(np.arange(3.), np.array([np.nan, 1., 2.]), [1., 1., 0.])
    assert not fit["ok"]
    assert np.all(np.isnan(fit["best fit"]))


def test_average_decay_group_weighting():
    x = np.linspace(0, 5000, 20)
    curves = [tools.exp_decay(x, 0.6, 1000., 0.4) + d for d in (-0.01, 0.0, 0.01)]
    avg, sem, fit = EDM.AverageDecayGroup(curves, x, [0.5, 900., 0.5], Weighted=True)
    np.testing.assert_allclose(avg, curves[1])
    np.testing.assert_allclose(sem, np.std(curves, axis=0)/np.sqrt(3))
    assert fit["weighted"]
    assert fit["best fit"][1] == pytest.approx(1000., rel=1e-3)
    _, sem1, fit1 = EDM.AverageDecayGroup(curves[:1], x, [0.5, 900., 0.5])
    assert np.all(sem1 == 0) and not fit1["weighted"]


# ---------------------------------------------------------- setpoint scans

def _setpoint_scan(wm, extra=None):
    sp = np.linspace(-1, 1, len(wm))
    return onoff_scan([1.]*len(wm), [2.]*len(wm), sp, wm=wm, extra=extra)


def test_axis_auto_wm_lock_and_setpoint():
    sp = np.linspace(-1, 1, 11)
    # WM follows a WM lock scan in GHz: 1000 MHz per GHz -> WM
    A = _setpoint_scan(288.7 + sp*1e-3)
    assert EDM.SetpointScanAxis(A, 288)["mode"] == "WM"
    # WM on the TCL-scanned laser, -235 MHz/V < 800 -> not WM; no offset -> setpoint, fitted cal
    A = _setpoint_scan(288.7 - 235e-6*sp)
    ax = EDM.SetpointScanAxis(A, 288, TCLcal=-100., TCLcalerr=1.)
    assert ax["mode"] == "setpoint"
    assert ax["TCLconv"] == pytest.approx(-235., rel=1e-6)
    assert EDM.SetpointScanAxis(A, 288, TCLcal=-100., ManualTCL=True)["TCLconv"] == -100.
    # with a saved WM lock offset -> WM lock
    A = _setpoint_scan(288.7 - 235e-6*sp, extra={"out:offset": 288.73})
    ax = EDM.SetpointScanAxis(A, 288)
    assert ax["mode"] == "WM lock" and ax["offset"] == 288.73
    # WM on another laser -> manual calibration
    A = _setpoint_scan(np.full(11, 270.26))
    ax = EDM.SetpointScanAxis(A, 288, TCLcal=-235.1, TCLcalerr=0.83)
    assert ax["mode"] == "setpoint" and ax["TCLconv"] == -235.1 and "not 288" in ax["note"]
    with pytest.raises(ValueError):
        EDM.SetpointScanAxis(A, 288, Mode="WM lock")


def test_setpoint_curves_sorted_and_frequencies():
    sp = np.linspace(1, -1, 5)   # a down pass
    A = onoff_scan([1, 2, 3, 4, 5], [2.]*5, sp)
    ax = {"mode": "setpoint", "TCLconv": -200., "offset": None}
    C = EDM.SetpointScanCurves(A, ax, *GATES)
    np.testing.assert_allclose(C["X"], sp[::-1])
    assert np.all(np.diff(C["on"]) < 0)   # largest pulse was taken first, at the top setpoint
    f = EDM.SetpointToFreqTHz(C["X"], ax, SET=0., SETfreq=288.)
    np.testing.assert_allclose(f, 288. + C["X"]*-200e-6)
    lock = {"mode": "WM lock", "offset": 288.5, "TCLconv": None}
    np.testing.assert_allclose(EDM.SetpointToFreqTHz([2.], lock), [288.502])
    assert EDM.SetpointToFreqTHz([0.], ax) is None
    v = EDM.FreqToVelocity(542.8 - 1e-6, 542.8, 552e-9)
    assert v == pytest.approx(0.552)


def test_bin_xy():
    X = np.array([0., 0., 1., 1., 2.])
    Y = np.array([1., 3., 2., 4., 5.])
    bx, by, berr, bn = EDM.BinXY(X, Y, 1.)
    np.testing.assert_allclose(bx, [0., 1., 2.])
    np.testing.assert_allclose(by, [2., 3., 5.])
    np.testing.assert_array_equal(bn, [2, 2, 1])
    np.testing.assert_allclose(berr[:2], [1., 1.])   # std(ddof=1)/sqrt(2)
    assert berr[2] == pytest.approx(1.)              # single point: mean SEM of the others


def test_gaussian_peak_fit_finds_dip():
    x = np.linspace(-100, 100, 81)
    y = tools.Gaussian(x, 12., 15., -0.3, 1.)
    fit = EDM.GaussianPeakFit(x, y)
    assert fit["ok"]
    assert fit["best fit"][0] == pytest.approx(12., abs=1e-3)
    assert fit["FWHM"] == pytest.approx(15*2*np.sqrt(2*np.log(2)), rel=1e-4)


# ------------------------------------------------------------ 4-shot slowing

def test_fit_slowing_on_off_recovers_shift():
    x = np.linspace(-10, 70, 30)
    off = tools.Gaussian_FWHM(x, 300., 40., 22., 50.)
    on = tools.Gaussian_FWHM(x, 250., 30., 28., 50.)
    res = EDM.FitSlowingOnOff(x, on, off, InVolts=False)
    assert res["peak shift"][0] == pytest.approx(-10., abs=1e-3)
    assert res["fit OFF"]["best fit"][0] == pytest.approx(1., rel=1e-4)   # normalised to Off
    assert res["shift unit"] == "m/s"


def test_fit_slowing_volts_converted_with_calibration():
    x = np.linspace(-1.7, -0.7, 30)
    off = tools.Gaussian_FWHM(x, 300., -1.2, 0.2, 50.)
    on = tools.Gaussian_FWHM(x, 300., -1.1, 0.2, 50.)
    res = EDM.FitSlowingOnOff(x, on, off, InVolts=True, CalTCLconv=-133.9, CalTCLconverr=6.5,
                              angle=45., wavelength=552.)
    fac = -1/np.cos(np.pi/4)*552e-3
    assert res["shift unit"] == "m/s"
    assert res["peak shift"][0] == pytest.approx(0.1*-133.9*fac, rel=1e-4)


def test_slowing_axis_choices():
    A = four_shot_scan([1.]*5, [1.]*5, np.linspace(-1.7, -0.7, 5))
    ax = EDM.SlowingXAxis(A, True, 542, 542.809124, 45.)
    assert ax["in volts"] and not ax["from setpoint"]
    ax = EDM.SlowingXAxis(A, True, 542, 542.809124, 45., CalTCLconv=-133.9, CalRestSP=-1.7, wavelength=552.)
    assert not ax["in volts"] and ax["from setpoint"]
    assert ax["x"][0] == pytest.approx(0.)
    A["wm THz"] = np.full(5, 542.809124 - 50e-6)
    ax = EDM.SlowingXAxis(A, False, 542, 542.809124, 45.)
    assert ax["source"] == "WM"
    assert ax["x"][0] == pytest.approx(tools.VelocityfromFshift(-50., 542.809124, 45.))


def test_average_slowing_scans_sorts_passes():
    sp = np.linspace(0, 1, 4)
    up = {"On": np.arange(4.), "Off": np.ones(4), "x": sp*10, "setpoint": sp, "x unit": "m/s"}
    down = {k: (v[::-1] if isinstance(v, np.ndarray) else v) for k, v in up.items()}
    avg = EDM.AverageSlowingScans([up, down])
    np.testing.assert_allclose(avg["On"], np.arange(4.))
    np.testing.assert_allclose(avg["On err"], 0.)
    assert EDM.AverageSlowingScans([up])["On err"] is None
    other = dict(up, setpoint=sp + 1)
    with pytest.raises(ValueError, match="same setpoints"):
        EDM.AverageSlowingScans([up, other])


def test_fit_shift_vs_time():
    fit = EDM.FitShiftVsTime([4., 8., 12.], [-4., -8., -12.], [0.5, 0.5, 0.5], FixOrigin=True)
    assert fit["best fit"][0] == pytest.approx(-1.) and fit["best fit"][1] == 0.
    assert EDM.FitShiftVsTime([0.], [0.], [1.], FixOrigin=True) is None
    fit = EDM.FitShiftVsTime([4., 8.], [-3., -7.], [0.5, 0.5], FixOrigin=False)
    assert fit["best fit"][1] == pytest.approx(1.)


# ------------------------------------------------------------- TOF collector

def test_tof_traces_order_and_background():
    A = onoff_scan([1., 2.], [1., 1.], [0, 1], shots=3)
    A["on"][0, 1, :, 2] += 7.   # last shot taken
    traces = EDM.TOFTraces(A, "on", BkgStart=60., BkgEnd=70.)
    assert traces.shape == (6, A["on"].shape[2])
    bkg = (A["time on"]*1000 > 60) & (A["time on"]*1000 < 70)
    np.testing.assert_allclose(np.mean(traces[:, bkg], axis=1), 0., atol=1e-12)
    assert np.max(traces[5]) == pytest.approx(2., rel=1e-6)   # constant offset removed
    four = EDM.TOFTraces(four_shot_scan([1.], [1.], [0]), "on", FourShot=True)
    assert np.min(four) == pytest.approx(0., abs=1e-9)


def test_chunk_means():
    traces = np.arange(10.)[:, None]*np.ones((10, 3))
    chunks = EDM.ChunkMeans(traces, 4)
    assert [(a, b) for a, b, _, _ in chunks] == [(0, 4), (4, 8), (8, 10)]
    np.testing.assert_allclose(chunks[0][2], 1.5)
