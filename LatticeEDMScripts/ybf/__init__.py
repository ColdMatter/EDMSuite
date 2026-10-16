"""YbF molecular structure: constants, energy levels, transitions, stick spectra.

Extracted verbatim from YbF_spectroscopy_library.py on 2026-09-16 — the physics code is byte-identical to the
version that produced the group's published assignments. Do not retype values here;
edit and re-run Tests/test_levels_regression.py if a constant genuinely changes.

Typical use
-----------
    from ybf import constants, levels
    gp = constants.get_converted_ground_params('v0', 174)   # GHz
    levels.X_hyperfine(1, **gp)                             # 4 hyperfine components of N=1
"""

from . import units, constants, levels, transitions, spectra
from .units import c, hck, CM_INV_TO_THZ
from .constants import (X_State, A_Pi_12, A_Pi_32, A_Pi_32_new, State_561, State_557,
                        State_4f_7212, State_4f_7232,
                        get_converted_ground_params, get_initial_p0_dict_ghz)
from .levels import (eRotor, gamma_spinRot, F1, F2, X_hyperfine,
                     Ue, Uf, Ue3JCP, Uf3JCP, Ue4f, Uf4f)

__all__ = ["units", "constants", "levels", "transitions", "spectra",
           "c", "hck", "CM_INV_TO_THZ",
           "X_State", "A_Pi_12", "A_Pi_32", "A_Pi_32_new", "State_561", "State_557",
           "State_4f_7212", "State_4f_7232",
           "get_converted_ground_params", "get_initial_p0_dict_ghz",
           "eRotor", "gamma_spinRot", "F1", "F2", "X_hyperfine",
           "Ue", "Uf", "Ue3JCP", "Uf3JCP", "Ue4f", "Uf4f"]
__version__ = "0.1.0"
