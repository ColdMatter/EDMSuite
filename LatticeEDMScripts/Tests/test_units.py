"""Guards the one conversion that silently corrupts everything if it drifts."""

import numpy as np

from ybf import constants, units


def test_ghz_per_wavenumber():
    assert units.c == 29.9792458
    np.testing.assert_allclose(units.CM_INV_TO_THZ, units.c / 1000.0, rtol=1e-12)


def test_ground_params_are_returned_in_ghz():
    """X_State stores cm^-1; get_converted_ground_params must hand back GHz."""
    stored = constants.X_State['v0'][174]
    ghz = constants.get_converted_ground_params('v0', 174)
    np.testing.assert_allclose(ghz['Bpp'], stored['Bpp'] * units.c, rtol=1e-12)
    np.testing.assert_allclose(ghz['Bpp'], 7.2338271, rtol=1e-9)
