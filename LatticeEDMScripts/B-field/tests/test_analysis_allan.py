"""bfield.analysis.allan on synthetic data (no files, no hardware)."""

import numpy as np
import pytest

from bfield.analysis import allan
from bfield.analysis.spectra import AnalysisError


def test_white_noise_follows_one_over_root_tau():
    # white noise of one-sided ASD S: sigma(tau) = S / sqrt(2 tau)
    fs, S = 1200.0, 0.023
    rng = np.random.default_rng(1)
    x = rng.normal(0, S * np.sqrt(fs / 2), int(300 * fs))
    tau, a, err = allan.overlapping_adev(x, fs)
    assert tau[0] == pytest.approx(1 / fs) and np.all(np.diff(tau) > 0)
    assert tau[-1] <= 300 / 4 + 1e-9
    for t in (0.01, 0.1, 1.0, 10.0):
        assert allan.at_tau(tau, a, t) == pytest.approx(
            S / np.sqrt(2 * t), rel=0.1)
    assert np.all(err > 0) and np.all(np.diff(err / a) >= -1e-12)


def test_drift_rises_and_detrend_removes_it():
    fs = 100.0
    rng = np.random.default_rng(2)
    t = np.arange(int(200 * fs)) / fs
    x = rng.normal(0, 1, t.size) + 0.5 * t
    tau, a, _ = allan.overlapping_adev(x, fs)
    low, at = allan.minimum(tau, a)
    assert a[-1] > 10 * low and at < tau[-1]
    # a linear drift r gives r tau / sqrt(2) at long tau
    assert a[-1] == pytest.approx(0.5 * tau[-1] / np.sqrt(2), rel=0.1)
    _, a2, _ = allan.overlapping_adev(x, fs, detrend=True)
    assert a2[-1] < 0.1 * a[-1]


def test_matches_the_definition():
    # brute force: averages of m samples, every start, half mean square
    fs = 10.0
    x = np.random.default_rng(3).normal(size=500)
    tau, a, _ = allan.overlapping_adev(x, fs, per_decade=5)
    for t, v in zip(tau, a):
        m = int(round(t * fs))
        ybar = np.array([x[k:k + m].mean() for k in range(x.size - m + 1)])
        d = ybar[m:] - ybar[:-m]
        assert v == pytest.approx(np.sqrt(0.5 * np.mean(d ** 2)), rel=1e-9)


def test_limits_and_refusals():
    tau, _, _ = allan.overlapping_adev(np.ones(1000) + 1e-3 *
                                       np.arange(1000) % 7, 10.0,
                                       max_tau=1e6)
    assert tau[-1] <= 999 // 3 / 10.0          # capped at a third
    with pytest.raises(AnalysisError):
        allan.overlapping_adev([1.0, 2.0], 10.0)
    with pytest.raises(AnalysisError):
        allan.overlapping_adev([1.0, np.nan] * 50, 10.0)
    assert np.isnan(allan.at_tau(np.array([1.0, 2.0]),
                                 np.array([1.0, 1.0]), 5.0))
