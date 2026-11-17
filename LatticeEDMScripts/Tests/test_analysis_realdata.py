"""The shared processing functions give the same numbers as the original
library functions (as the *_SL.py scripts use them) on real ScanMaster files.
Skipped where the data folder isn't available."""

import os

import numpy as np
import pytest

pytestmark = pytest.mark.slow

EDM = pytest.importorskip("LatticeEDM_analysis_library")


def data_file(*parts):
    try:
        drive = EDM.get_datadrive()
    except KeyError:
        pytest.skip("no data folder for this computer")
    path = os.path.join(drive, *parts)
    if not os.path.isfile(path):
        pytest.skip("data file not available: " + path)
    return path


@pytest.fixture(scope="module")
def duration_scan():
    return EDM.ReadAverageScanInZippedXML(data_file("Oct 2026", "26", "003_V0Duration_v0v1v2v3.zip"))


@pytest.fixture(scope="module")
def four_shot_scan():
    path = data_file("Oct 2026", "26", "010_Slowing_angledprobe_setpoint_scanv0v1v2v3_MW_4fv0v1RQ.zip")
    return EDM.ReadAverageScanInZippedXML(path)


def test_extract_matches_library(duration_scan):
    A = EDM.ExtractScanArrays(duration_scan)
    TimeOn, DataOn, TimeOff, DataOff = EDM.GetTOFsSPP(duration_scan)
    np.testing.assert_array_equal(A["on"], DataOn)
    np.testing.assert_array_equal(A["off"], DataOff)
    np.testing.assert_array_equal(A["scan params"], EDM.GetScanParameterArray(duration_scan))
    analog0 = np.array([p.Analogs[0] for p in duration_scan.Points])
    if duration_scan.ScanSettings["analog:offset"] is None:
        # Duration (PGOutputPlugin) files don't save one; GetScanFreqArrayTHz fails on them
        np.testing.assert_array_equal(A["wm THz"], analog0)
    else:
        np.testing.assert_allclose(A["wm THz"], EDM.GetScanFreqArrayTHz(duration_scan))


def test_wm_matches_GetScanFreqArrayTHz(four_shot_scan):
    A = EDM.ExtractScanArrays(four_shot_scan)
    np.testing.assert_allclose(A["wm THz"], EDM.GetScanFreqArrayTHz(four_shot_scan))


def test_duration_matches_DurationRunSingle(duration_scan):
    gates = (21., 24., 60., 70.)
    _, fit_ref, x_ref, ratio_ref = EDM.DurationRunSingle(duration_scan, "", *gates, ToPrint=False, plotFit=False)
    A = EDM.ExtractScanArrays(duration_scan)
    G = EDM.GatedOnOff(A, *gates)
    np.testing.assert_allclose(G["ratio"], ratio_ref, rtol=1e-12)
    fit = EDM.FitDecay(A["scan params"], G["ratio"], [1., A["settings"]["end"]/3, 0.])
    # Same data and model: equal to the optimiser's convergence tolerance
    np.testing.assert_allclose(fit["best fit"], fit_ref["best fit"], rtol=1e-6)
    np.testing.assert_allclose(fit["error"], fit_ref["error"], rtol=1e-4)


def test_four_shot_matches_script_path(four_shot_scan):
    # setpointScanONOFF_4shot_batch_SL.py: GetTOFs, YAG On - YAG Off, GetGatedAvgCounts4Shot
    TimeOn, DataOn, TimeOff, DataOff = EDM.GetTOFs(four_shot_scan)
    on_ref = EDM.GetGatedAvgCounts4Shot(four_shot_scan, DataOn[0] - DataOn[1], TimeOn, 32, 42)
    off_ref = EDM.GetGatedAvgCounts4Shot(four_shot_scan, DataOff[0] - DataOff[1], TimeOff, 32, 42)
    G = EDM.FourShotGated(EDM.ExtractScanArrays(four_shot_scan), 32, 42)
    np.testing.assert_allclose(G["on"], on_ref, rtol=1e-12)
    np.testing.assert_allclose(G["off"], off_ref, rtol=1e-12)
