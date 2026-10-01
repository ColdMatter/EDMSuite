"""Molecular constants for YbF electronic states, by vibrational level and Yb isotope.

UNITS WARNING, inherited from the legacy library and deliberately preserved:
  * Every dict here stores energies/constants in cm^-1.
  * X_State was written as GHz values divided by `c` (= 29.9792458 GHz/cm^-1), so the
    stored numbers are cm^-1 while the literature source is in GHz.
  * get_converted_ground_params() multiplies back by `c` and returns GHz.
  * A_Pi_*, State_561, State_557 and the 4f states are cm^-1 natively.
Mixing the two is the most likely silent error in this code; always know which one a
function expects.

Extracted verbatim from YbF_spectroscopy_library.py on 2026-09-16 — the physics code is byte-identical to the
version that produced the group's published assignments. Do not retype values here;
edit and re-run Tests/test_levels_regression.py if a constant genuinely changes.
"""

from .units import c


# Note: 174 constants are converted from MHz to cm^-1 using speed of light 'c'
# For other even isotopes, gammas and hyperfine parameters are assumed to be 
# the same as for 174.
X_State = {
    "v0": {
        174: {
            "iso": 0.32, "Tv": 0, "Bpp": 7233.8271e-3 / c, "dpp": 2.388e-7,
            "g0": -13.41679e-3 / c, "g1": 3.9840e-6 / c, "g2": -25e-12 / c,
            "Xb": (170.26374 - 85.4028 / 3) * 1e-3 / c, "Xc": 85.4028e-3 / c, 
            "XC": 13.099e-6 / c
        },
        172: {
            "iso": 0.219, "Tv": 0, "Bpp": 0.241717, "dpp": 2.48e-7,
            "g0": -13.41679e-3 / c, "g1": 3.9840e-6 / c, "g2": -25e-12 / c,
            "Xb": (170.26374 - 85.4028 / 3) * 1e-3 / c, "Xc": 85.4028e-3 / c, 
            "XC": 13.099e-6 / c
        },
        176: {
            "iso": 0.127, "Tv": 0, "Bpp": 0.241247, "dpp": 2.549e-7,
            "g0": -13.41679e-3 / c, "g1": 3.9840e-6 / c, "g2": -25e-12 / c,
            "Xb": (170.26374 - 85.4028 / 3) * 1e-3 / c, "Xc": 85.4028e-3 / c, 
            "XC": 13.099e-6 / c
        }
    },
    "v1": {
        174: {
            "iso": 0.32, "Tv": 502.15090, "Bpp": 7188.8919e-3 / c, "dpp": 2.303e-7,
            "g0": -33.81036e-3 / c, "g1": 4.3205e-6 / c, "g2": -28e-12 / c,
            "Xb": (168.770 - 86.7120 / 3) * 1e-3 / c, "Xc": 86.7120e-3 / c, 
            "XC": 17.20e-6 / c
        },
        172: {
            "iso": 0.219, "Tv": 502.4629, "Bpp": 0.240177, "dpp": 2.369e-7,
            "g0": -33.81036e-3 / c, "g1": 4.3205e-6 / c, "g2": -28e-12 / c,
            "Xb": (168.770 - 86.7120 / 3) * 1e-3 / c, "Xc": 86.7120e-3 / c, 
            "XC": 17.20e-6 / c
        },
        176: {
            "iso": 0.127, "Tv": 501.8905, "Bpp": 0.239729, "dpp": 2.487e-7,
            "g0": -33.81036e-3 / c, "g1": 4.3205e-6 / c, "g2": -28e-12 / c,
            "Xb": (168.770 - 86.7120 / 3) * 1e-3 / c, "Xc": 86.7120e-3 / c, 
            "XC": 17.20e-6 / c
        }
    },
    "v2": {
        174: {
            "iso": 0.32, "Tv": 999.81290, "Bpp": 7144.20e-3 / c, "dpp": 6.649e-6 / c,
            "g0": -54.20393e-3 / c, "g1": 4.657e-6 / c, "g2": -31e-12 / c,
            "Xb": 137.936e-3 / c, "Xc": 88.0212e-3 / c, "XC": 21.301e-6 / c
        },
        172: {
            "iso": 0.219, "Tv": 1000.4293, "Bpp": 0.238617, "dpp": 2.102e-7,
            "g0": -54.20393e-3 / c, "g1": 4.657e-6 / c, "g2": -31e-12 / c,
            "Xb": 137.936e-3 / c, "Xc": 88.0212e-3 / c, "XC": 21.301e-6 / c
        },
        176: {
            "iso": 0.127, "Tv": 999.3216, "Bpp": 0.238137, "dpp": 2.104e-7,
            "g0": -54.20393e-3 / c, "g1": 4.657e-6 / c, "g2": -31e-12 / c,
            "Xb": 137.936e-3 / c, "Xc": 88.0212e-3 / c, "XC": 21.301e-6 / c
        }
    },
    "v3": {
        174: {
            "iso": 0.32, "Tv": 1493.0215, "Bpp": 7099.63e-3 / c, "dpp": 6.394e-6 / c,
            "g0": -74.5975e-3 / c, "g1": 4.9935e-6 / c, "g2": -34e-12 / c,
            "Xb": 136.006e-3 / c, "Xc": 89.3304e-3 / c, "XC": 25.402e-6 / c
        },
        172: {
            "iso": 0.219, "Tv": 1493.899, "Bpp": 0.237067, "dpp": 2.102e-7,
            "g0": -74.5975e-3 / c, "g1": 4.9935e-6 / c, "g2": -34e-12 / c,
            "Xb": 136.006e-3 / c, "Xc": 89.3304e-3 / c, "XC": 25.402e-6 / c
        }
    }
}

A_Pi_12 = {
    "v0": {
        172: {
            "t": 18106.2265, "b": 0.248064, "ad": 1.1848e-3,
            "d": 2.538e-7, "p2q": -0.39667, "dp2q": 1.142e-6
        },
        174: {
            "t": 18106.1991, "b": 0.247758, "ad": 1.1864e-3,
            "d": 2.453e-7, "p2q": -0.39635, "dp2q": 1.173e-6
        },
        176: {
            "t": 18106.1714, "b": 0.247579, "ad": 1.1875e-3,
            "d": 2.607e-7, "p2q": -0.39553, "dp2q": 1.006e-6
        }
    }
}

A_Pi_32 = {
    "v0": {
        172: {
            "t": 19471.5237, "b": 0.248064, "ad": 1.1848e-3, "d": 2.538e-7
        },
        174: {
            "t": 19471.4899, "b": 0.247758, "ad": 1.1864e-3, "d": 2.453e-7
        },
        176: {
            "t": 19471.4574, "b": 0.247579, "ad": 1.1875e-3, "d": 2.607e-7
        }
    }
}

A_Pi_32_new = {
    "v0": {
        172: {
            "t": 19471.4369507, "b": 0.24834752, "ad": 1.1848e-3, "d": 2.538e-7
        },
        174: {
            "t": 19471.3828462, "b": 0.247948233, "ad": 1.1864e-3, "d": 2.453e-7
        },
        176: {
            "t": 19471.3760815, "b": 0.24789174, "ad": 1.1875e-3, "d": 2.607e-7
        }
    }
}

State_561 = {
    "v0": {
        172: {
            "t": 18705.0802, "b": 0.257242, "ad": 0,
            "d": -5.963e-7, "p2q": -1.00031334, "dp2q": -10.042e-5
        },
        174: {
            "t": 18704.9353, "b": 0.256964, "ad": 0,
            "d": -5.934e-7, "p2q": -1.00284405, "dp2q": -9.910e-5
        },
        176: {
            "t": 18704.8073, "b": 0.256820, "ad": 0,
            "d": -5.691e-7, "p2q": -1.00439729, "dp2q": -9.846e-5
        }
    }
}

State_557 = {
    "v0": {
        172: {
            "t": 18580.6837, "b": 0.255680, "ad": 1.1864e-3,
            "d": 9.984e-7, "p2q": -0.90021, "dp2q": 8.441e-5
        },
        174: {
            "t": 18580.5317, "b": 0.255305, "ad": 1.1864e-3,
            "d": 9.670e-7, "p2q": -0.89614, "dp2q": 8.265e-5
        },
        176: {
            "t": 18580.3792, "b": 0.255095, "ad": 1.1848e-3,
            "d": 9.834e-7, "p2q": -0.89350, "dp2q": 8.249e-5
        }
    }
}

# --- 4f Inner Shell States (Stefan 2024 model) ---
State_4f_7212 = {
    "v0": {
        174: {
            "t": 8474.390,
            "b": 0.26737,
            "delta": -538.151,
            "zeta": 3.8760,
        }
    },
    "v1": {
        174: {
            "t": 9090.675,
            "b": 0.2656,
            "delta": -538.151,
            "zeta": 3.8756,
        }
    }
}

State_4f_7232 = {
    "v0": {
        174: {
            "t": 9012.541,
            "b": 0.26597,
            "delta": -538.151,
            "zeta": 0.0,
        }
    }
}

# [557] State (~18580 cm⁻¹)
rule_1858 = {
    "t": 18580.574,   # +/- 0.002
    "b": 0.25466,     # +/- 0.00006
    "delta": -538.151,
    "zeta": -1.7698,  # +/- 0.001
}

# [561] State (~18705 cm⁻¹)
rule_1871 = {
    "t": 18705.007,   # +/- 0.002
    "b": 0.25687,     # +/- 0.00006
    "delta": -538.151,
    "zeta": -1.9607,  # +/- 0.001
}

# 1. Convert Ground State Constants to GHz
def get_converted_ground_params(vib_state="v1", iso=174):
    """Pulls ground state parameters from X_State and converts cm^-1 values to GHz."""
    g_params = X_State[vib_state][iso].copy()
    g_params_ghz = {}

    for key, val in g_params.items():
        if key == "iso":
            g_params_ghz[key] = val
        else:
            g_params_ghz[key] = val * c

    return g_params_ghz

# 2. Extract Initial Upper State Guesses in GHz
def get_initial_p0_dict_ghz(upper_vib="v0", iso=174):
    """Returns initial guesses for upper state parameters [t, b, ad, d] in GHz as a dictionary."""
    a_params = A_Pi_32[upper_vib][iso]
    return {
        "t": a_params["t"] * c,
        "b": a_params["b"] * c,
        "ad": a_params["ad"] * c,
        "d": a_params["d"] * c,
    }
