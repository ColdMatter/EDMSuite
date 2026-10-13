"""Shared analysis helpers: spectra, band statistics, block statistics,
run-file loading.

Each function is the MAJORITY version of a helper that the IROP analysis
scripts each carried a private copy of (analysis_scripts/README.md has
the variant table). A minority variant that a report figure depends on
is kept here too, under its own name, rather than behind a flag.

Ported scripts import from here only where their own copy was
AST-identical to the function below, so no figure changed by the move.
New analysis code should import from here instead of copying.

Units follow the data: pT in, pT/sqrt(Hz) out (the ported scripts
multiply by 1000 for fT/sqrt(Hz) themselves).
"""

from __future__ import annotations

import glob
import json
import os

import numpy as np
from scipy.signal import welch


class AnalysisError(ValueError):
    """Input the analysis cannot use (too short, missing, corrupt)."""


def _require(cond, msg):
    if not cond:
        raise AnalysisError(msg)


# ----------------------------------------------------------------- spectra

def welch_asd(x, fs, nperseg):
    """One-sided amplitude spectral density, Welch with linear detrend.

    nperseg is in samples and is capped at the record length. Returns
    (f, asd) with asd in units/sqrt(Hz).
    """
    f, pxx = welch(x, fs=fs, nperseg=min(len(x), int(nperseg)),
                   detrend="linear")
    return f, np.sqrt(pxx)


def welch_asd_checked(x, fs, nperseg):
    """welch_asd, refusing a non-finite result (corrupt input).

    The variant in gui_baseline/plot_results.py,
    failed_analog/plot_3axis_diagnostic.py and
    prediction_B2/predict_ms1l_noise_budget.py; nperseg must already be
    an int there.
    """
    f, pxx = welch(x, fs=fs, nperseg=min(len(x), nperseg), detrend="linear")
    a = np.sqrt(pxx)
    _require(np.isfinite(a).all(), "ASD came out non-finite -- corrupt input?")
    return f, a


def spliced_asd(x, fs, lowseg=200.0, hiseg=20.0, split=1.0):
    """ASD from two Welch passes spliced at `split` Hz.

    Long segments (lowseg seconds) below the split resolve the 0.05 Hz
    band; short segments (hiseg seconds) above it average down the
    white floor. The house convention behind every published plateau
    number (200 s / 20 s, split at 1 Hz).
    """
    def one(seg_s):
        f, p = welch(x, fs=fs, nperseg=min(len(x), int(seg_s * fs)),
                     detrend="linear")
        return f, np.sqrt(p)

    fl, al = one(lowseg)
    fh, ah = one(hiseg)
    ml = (fl > 0) & (fl < split)
    mh = fh >= split
    return np.concatenate([fl[ml], fh[mh]]), np.concatenate([al[ml], ah[mh]])


def spliced_asd_capped(x, fs, lowseg=200.0, hiseg=20.0, split=1.0):
    """spliced_asd with the long segment capped at half the record.

    Guarantees at least two low-frequency segments on a short record.
    Used by ms1l_1_predegauss/relax_series.py (fig_relax_series.png, a
    report figure); on a 900 s record it equals spliced_asd.
    """
    def one(seg_s):
        f, p = welch(x, fs=fs, nperseg=min(len(x), int(seg_s * fs)),
                     detrend="linear")
        return f, np.sqrt(p)

    fl, al = one(min(lowseg, len(x) / fs / 2))
    fh, ah = one(hiseg)
    ml = (fl > 0) & (fl < split)
    mh = fh >= split
    return np.concatenate([fl[ml], fh[mh]]), np.concatenate([al[ml], ah[mh]])


# ---------------------------------------------------------- band statistics

def band_median(f, a, lo, hi):
    """Median of a over lo <= f <= hi, as a float.

    An empty band gives numpy's nan plus a RuntimeWarning (the majority
    behaviour; see band_median_or_nan for the quiet version).
    """
    m = (f >= lo) & (f <= hi)
    return float(np.median(a[m]))


def band_median_or_nan(f, a, lo, hi):
    """band_median returning nan, without a warning, for an empty band.

    Used by ms1l_1_predegauss/relax_series.py (a report figure).
    """
    m = (f >= lo) & (f <= hi)
    return float(np.median(a[m])) if m.any() else float("nan")


def band_median_required(f, asd, lo, hi):
    """band_median that refuses an empty band (calibration/cal_daq_floor.py)."""
    m = (f >= lo) & (f <= hi)
    _require(m.any(), f"no spectral bins in {lo}-{hi} Hz")
    return float(np.median(asd[m]))


def block_stats(x, fs, block_s=60.0):
    """Block-mean statistics of a record.

    Splits x into whole blocks of block_s seconds (at least 3) and fits
    a line to the block means against time in minutes. Returns
    (mean, sem, slope_per_min, slope_sigma, n_blocks); the slope sigma
    comes from the least-squares covariance with the residual variance.
    """
    n = int(block_s * fs)
    nb = x.size // n
    _require(nb >= 3, f"record too short for block stats ({nb} blocks)")
    means = x[:nb * n].reshape(nb, n).mean(axis=1)
    tmin = (np.arange(nb) + 0.5) * block_s / 60.0
    mean = float(means.mean())
    sem = float(means.std(ddof=1) / np.sqrt(nb))
    A = np.vstack([tmin, np.ones(nb)]).T
    coef, res, *_ = np.linalg.lstsq(A, means, rcond=None)
    resid = means - A @ coef
    s2 = float(resid @ resid) / max(nb - 2, 1)
    cov00 = s2 * np.linalg.inv(A.T @ A)[0, 0]
    return mean, sem, float(coef[0]), float(np.sqrt(cov00)), nb


# ------------------------------------------------------------ run files

def find_meta(run_dir):
    """(meta dict, session prefix) of the ONE *.meta.json in run_dir."""
    metas = glob.glob(os.path.join(run_dir, "*.meta.json"))
    _require(len(metas) == 1,
             f"{run_dir}: expected one meta.json, found {len(metas)}")
    with open(metas[0], encoding="utf-8") as fh:
        return json.load(fh), os.path.basename(metas[0])[:-len(".meta.json")]


def load_raw_csv(path):
    """A recorder *_raw.csv as {channel: volts array}.

    Line 1 is the '# QZFM raw voltages' comment, line 2 the header
    (sample,V_z,V_y,...); the V_ prefix is dropped from the keys.
    """
    with open(path, encoding="utf-8") as fh:
        fh.readline()
        names = fh.readline().strip().split(",")
    data = np.genfromtxt(path, delimiter=",", skip_header=2)
    return {n[2:]: data[:, i] for i, n in enumerate(names)
            if n.startswith("V_")}


def load_pT(path):
    """One-column sample file (a *_pT.txt, or a GUI export) as an array.

    Skips '#', '%' and '//' comment lines and takes the first number of
    every other line; refuses a missing file, no samples, more than 16
    unparsable lines, or non-finite values.
    """
    _require(os.path.exists(path),
             f"missing data file: {path} (copy the measurement into this "
             f"folder first)")
    rows, bad = [], 0
    with open(path, "r") as fh:
        for line in fh:
            line = line.strip()
            if not line or line.startswith(("#", "%", "//")):
                continue
            try:
                rows.append(float(line.replace(",", " ").split()[0]))
            except ValueError:
                bad += 1
    _require(rows, f"{path}: no numeric samples")
    _require(bad <= 16, f"{path}: {bad} unparsable lines -- corrupt file?")
    x = np.asarray(rows, dtype=float)
    _require(np.isfinite(x).all(), f"{path}: contains NaN/inf")
    return x
