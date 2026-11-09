"""A(2Pi_1/2) 19F hyperfine helpers (Sauer et al., J. Chem. Phys. 110, 8424 (1999))."""

import numpy as np
import pytest

from ybf import constants, levels

HFS = constants.A_Pi_12_hfs["v0"][174]


def test_j_half_values():
    # J=1/2: factor (2J+1)^2/(8J(J+1)) = 2/3
    assert levels.A12_hfs_split(0.5, 'e', **HFS) == pytest.approx((-0.8 - 4.6) * 2 / 3)   # -3.6, inverted
    assert levels.A12_hfs_split(0.5, 'f', **HFS) == pytest.approx((-0.8 + 4.6) * 2 / 3)   # +2.53


def test_high_j_limit_is_eta_A_perp_over_2():
    # Sauer 1999 Table I: at high J the A_perp term dominates, eps -> eta*A_perp/2
    for parity, eta in (('e', -1), ('f', 1)):
        assert levels.A12_hfs_split(1000.5, parity, **HFS) == pytest.approx(eta * HFS["A_perp"] / 2, abs=2e-3)


@pytest.mark.parametrize('j', [0.5, 1.5, 2.5, 7.5])
@pytest.mark.parametrize('parity', ['e', 'f'])
def test_shifts_keep_centre_of_gravity_and_give_split(j, parity):
    up, lo = levels.A12_hfs_shifts(j, parity, **HFS)
    assert up - lo == pytest.approx(levels.A12_hfs_split(j, parity, **HFS))
    # weights 2F+1: F=J+1/2 -> 2J+2, F=J-1/2 -> 2J
    assert (2*j + 2) * up + (2*j) * lo == pytest.approx(0.0, abs=1e-12)


def test_bad_parity():
    with pytest.raises(ValueError):
        levels.A12_hfs_split(0.5, '+', **HFS)
