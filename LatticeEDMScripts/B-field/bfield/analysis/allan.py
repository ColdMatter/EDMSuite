"""Allan deviation of a field record.

The overlapping Allan deviation of the field samples themselves (in the
time-and-frequency language the samples are "frequency" data, one every
1/fs s):

    sigma^2(tau) = 1/2 < (ybar_{k+m} - ybar_k)^2 >,   tau = m / fs,

where ybar_k is the mean of m consecutive samples starting at k, and the
average runs over every start k (overlapping). Same units as the input:
pT in, pT out.

How to read it next to the ASD: for white noise with a one-sided ASD of
S (units/rtHz), sigma(tau) = S / sqrt(2 tau) -- a slope of -1/2 on the
log-log plot, sigma(1 s) = S / sqrt(2). Where the curve stops falling is
where drift and 1/f noise take over; a linear drift gives a slope of +1.

No dependency beyond numpy (allantools' "oadev" gives the same numbers).
"""

from __future__ import annotations

import numpy as np

from bfield.analysis.spectra import AnalysisError

LONGEST_FRACTION = 0.25     # longest tau by default: a quarter of the record


def tau_grid(n, fs, per_decade=10, max_tau=0.0):
    """Averaging lengths m (samples), log-spaced, unique, 1 .. the longest.

    max_tau 0 = a quarter of the record; it is capped at a third (fewer
    than three independent intervals is not an estimate)."""
    longest = n / fs * LONGEST_FRACTION if not max_tau or max_tau <= 0 \
        else max_tau
    m_max = int(min(longest * fs, (n - 1) // 3))
    if m_max < 1:
        raise AnalysisError(f"record too short for an Allan deviation "
                            f"({n} samples)")
    k = max(int(np.ceil(np.log10(m_max) * per_decade)), 1)
    m = np.unique(np.round(np.logspace(0, np.log10(m_max), k + 1))
                  .astype(np.int64))
    return m[(m >= 1) & (m <= m_max)]


def overlapping_adev(x, fs, per_decade=10, max_tau=0.0, detrend=False):
    """(tau_s, adev, err) of the samples x taken at fs.

    err is a rough 1-sigma statistical error, adev / sqrt(number of
    independent tau intervals - 1) -- enough for error bars, not a full
    equivalent-degrees-of-freedom treatment. detrend removes a linear
    drift first (it otherwise rises as tau at long tau)."""
    y = np.asarray(x, dtype=float)
    if not np.isfinite(y).all():
        raise AnalysisError("non-finite samples -- drop them first")
    n = y.size
    if detrend and n > 1:
        t = np.arange(n)
        y = y - np.polyval(np.polyfit(t, y, 1), t)
    y = y - y.mean()                      # keeps the running sum small
    m = tau_grid(n, fs, per_decade, max_tau)
    c = np.concatenate([[0.0], np.cumsum(y)])
    adev = np.empty(m.size)
    for i, mi in enumerate(m):
        # ybar_{k+m} - ybar_k = (c[k+2m] - 2 c[k+m] + c[k]) / m
        d = (c[2 * mi:] - 2.0 * c[mi:n + 1 - mi] + c[:n + 1 - 2 * mi]) / mi
        adev[i] = np.sqrt(0.5 * np.mean(d * d))
    independent = np.maximum(n // m - 1, 1)
    return m / fs, adev, adev / np.sqrt(independent)


def at_tau(tau, adev, t):
    """adev interpolated (log-log) at tau = t; nan outside the curve."""
    if t < tau[0] or t > tau[-1]:
        return float("nan")
    return float(np.exp(np.interp(np.log(t), np.log(tau), np.log(adev))))


def minimum(tau, adev):
    """(lowest adev, its tau) -- where averaging stops helping."""
    i = int(np.argmin(adev))
    return float(adev[i]), float(tau[i])
