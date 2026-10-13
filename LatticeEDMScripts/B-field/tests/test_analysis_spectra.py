"""bfield.analysis.spectra on synthetic data (no files, no hardware)."""

import numpy as np
import pytest

from bfield.analysis import spectra

FS = 1200.0


def white(level_per_rthz, seconds, seed=1):
    """White noise whose one-sided ASD is level_per_rthz."""
    rng = np.random.default_rng(seed)
    sigma = level_per_rthz * np.sqrt(FS / 2.0)
    return rng.normal(0.0, sigma, int(seconds * FS))


def test_welch_asd_white_level():
    x = white(0.02, 300.0)                       # 20 fT/rtHz in pT
    f, a = spectra.welch_asd(x, FS, 20 * FS)
    band = spectra.band_median(f, a, 30.0, 45.0)
    # median of a chi-distributed ASD sits a little under the mean level
    assert band == pytest.approx(0.02, rel=0.05)


def test_spliced_asd_splits_at_1hz_and_keeps_level():
    x = white(0.05, 900.0)
    f, a = spectra.spliced_asd(x, FS)
    lo, hi = f < 1.0, f >= 1.0
    assert lo.any() and hi.any()
    assert np.all(np.diff(f) > 0)                # one monotonic axis
    # 200 s segments below the split: 0.005 Hz bins
    assert np.min(np.diff(f[lo])) == pytest.approx(1 / 200.0)
    assert np.min(np.diff(f[hi])) == pytest.approx(1 / 20.0)
    assert spectra.band_median(f, a, 0.02, 0.9) == pytest.approx(0.05, rel=0.2)
    assert spectra.band_median(f, a, 30, 45) == pytest.approx(0.05, rel=0.05)


def test_spliced_asd_capped_equals_plain_on_a_long_record():
    x = white(0.05, 900.0)
    for u, v in zip(spectra.spliced_asd(x, FS), spectra.spliced_asd_capped(x, FS)):
        np.testing.assert_array_equal(u, v)
    short = white(0.05, 120.0)
    f, _ = spectra.spliced_asd_capped(short, FS)
    assert np.min(np.diff(f[f < 1.0])) == pytest.approx(1 / 60.0)


def test_band_median_variants_on_empty_band():
    f = np.linspace(0, 10, 11)
    a = np.ones_like(f)
    assert np.isnan(spectra.band_median_or_nan(f, a, 20, 30))
    with pytest.raises(spectra.AnalysisError):
        spectra.band_median_required(f, a, 20, 30)


def test_welch_asd_checked_refuses_nan():
    x = white(0.02, 10.0)
    x[5] = np.nan
    # scipy's detrend refuses NaN itself; AnalysisError is a ValueError too
    with pytest.raises(ValueError):
        spectra.welch_asd_checked(x, FS, 2400)
    f, a = spectra.welch_asd_checked(white(0.02, 10.0), FS, 2400)
    assert np.isfinite(a).all()


def test_block_stats_recovers_drift():
    t = np.arange(int(600 * FS)) / FS
    x = 5.0 + 0.5 * (t / 60.0) + white(0.001, 600.0)   # 0.5 pT/min
    mean, sem, slope, slope_sigma, nb = spectra.block_stats(x, FS)
    assert nb == 10
    assert slope == pytest.approx(0.5, abs=1e-3)
    assert mean == pytest.approx(5.0 + 0.5 * 5.0, abs=1e-2)
    with pytest.raises(spectra.AnalysisError):
        spectra.block_stats(x[: int(120 * FS)], FS)


def test_load_raw_csv_and_find_meta(tmp_path):
    raw = tmp_path / "s_raw.csv"
    raw.write_text("# QZFM raw voltages | session=s start=x fs=1200.0\n"
                   "sample,V_z,V_y,V_mon\n0,0.1,0.2,0.3\n1,0.4,0.5,0.6\n")
    cols = spectra.load_raw_csv(str(raw))
    assert sorted(cols) == ["mon", "y", "z"]
    np.testing.assert_allclose(cols["y"], [0.2, 0.5])
    (tmp_path / "s.meta.json").write_text('{"fs_hz": 1200.0}')
    meta, prefix = spectra.find_meta(str(tmp_path))
    assert meta["fs_hz"] == 1200.0 and prefix == "s"


def test_load_pT_skips_headers(tmp_path):
    p = tmp_path / "x_Z_pT.txt"
    p.write_text("# QZFM acquisition | channel=Z\n# one value per line\n"
                 "1.5\n-2.25\n3\n")
    np.testing.assert_allclose(spectra.load_pT(str(p)), [1.5, -2.25, 3.0])
