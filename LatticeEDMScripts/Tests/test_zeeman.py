"""ybf.zeeman: X(N) and A2Pi1/2 J'=1/2 Zeeman levels (MHz, gauss)."""

import numpy as np
import pytest

from ybf import constants, levels, zeeman

MUB = zeeman.MU_B_MHZ_PER_G


@pytest.mark.parametrize('v', ['v0', 'v1', 'v2', 'v3'])
@pytest.mark.parametrize('n', [0, 1, 2, 3])
def test_zero_field_matches_X_hyperfine(v, n):
    p = constants.get_converted_ground_params(v, 174)
    ref = levels.X_hyperfine(n, **p) - (p['Tv'] + levels.eRotor(n, **p))
    ref = np.sort(ref[~np.isnan(ref)]) * 1e3
    got = np.unique(np.round([c['E'][0] for c in zeeman.X_zeeman_levels([0.0], n=n, vib_state=v)], 6))
    np.testing.assert_allclose(got, ref, atol=1e-6)


def test_X_N1_labels_and_counts():
    X = zeeman.X_zeeman_levels([0.0, 1.0])
    assert len(X) == 12
    counts = {lab: sum(c['label'] == lab for c in X) for lab in {c['label'] for c in X}}
    assert counts == {'F=1l': 3, 'F=0': 1, 'F=2': 5, 'F=1h': 3}
    E0 = {c['label']: c['E'][0] for c in X}
    assert E0['F=1h'] > E0['F=2'] > E0['F=0'] > E0['F=1l']


def test_X_N1_low_field_gF():
    g = zeeman.low_field_gF(zeeman.X_zeeman_levels([0.0, 1e-4]))
    assert g['F=0'] == 0.0
    assert g['F=2'] == pytest.approx(0.5, abs=2e-3)
    assert g['F=1h'] == pytest.approx(0.4286, abs=5e-4)
    assert g['F=1l'] == pytest.approx(0.0713, abs=5e-4)


def test_X_stretched_state_is_linear():
    # |N=1, mN=1; mS=+1/2; mI=+1/2> is an exact eigenstate at every field
    B = np.array([0.0, 50.0, 100.0])
    c = [c for c in zeeman.X_zeeman_levels(B) if c['mF'] == 2][0]
    slope = np.diff(c['E']) / np.diff(B)
    assert slope[0] == pytest.approx(slope[1], rel=1e-12)


def test_A12_gF_matches_fit_B():
    # g_F(F'=1) = gJ/2 - gI muN/(2 muB); ChillTheBeams' model file: -0.2031 (e), +0.2782 (f)
    ge = zeeman.low_field_gF(zeeman.A12_zeeman_levels([0.0, 1e-4], 'e'))
    gf = zeeman.low_field_gF(zeeman.A12_zeeman_levels([0.0, 1e-4], 'f'))
    assert ge["F'=1"] == pytest.approx(-0.2031, abs=1e-4)
    assert gf["F'=1"] == pytest.approx(+0.2782, abs=1e-4)
    assert zeeman.A12_gJ('e', **constants.A_Pi_12_zeeman['v0'][174]) == pytest.approx(-0.4034, abs=1e-4)


def test_A12_zero_field_split_is_sauer():
    for parity in ('e', 'f'):
        A = zeeman.A12_zeeman_levels([0.0], parity)
        E = {c['label']: c['E'][0] for c in A}
        eps = levels.A12_hfs_split(0.5, parity, **constants.A_Pi_12_hfs['v0'][174])
        assert E["F'=1"] - E["F'=0"] == pytest.approx(eps, abs=1e-9)


def test_bad_parity():
    with pytest.raises(ValueError):
        zeeman.A12_gJ('+', **constants.A_Pi_12_zeeman['v0'][174])
