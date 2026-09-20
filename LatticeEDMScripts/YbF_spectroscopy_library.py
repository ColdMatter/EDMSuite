# -*- coding: utf-8 -*-
"""
Created on Sat Aug  8 15:36:53 2026

Formulas and constants for YbF.

Transferred from Mathmatica Notebooks on Aug 8th 2026.

Sources of constants and models:
    - X Σ: Sauer et al., JCP 105, 7412 (1996)
    
The aim of this file is to create an easy-to-use library with sufficient
commenting for new group members.

Note this file is meant as a library of equations and constants, not 
for calculations.

All constants are using the highest precision version from Jongseok
in his July 2026 Mathmatica notebook.

### Notes:
    - X 176 doesn't have constants for v3

### Dictionaries of constants:
    - Standard structure (X & A states):
        X = {
            174: {"v0": {}, "v1": {}...}
            ...
            }
    - [561] and [557] are treated separately.
        Does it make sense to label with vibrational levels? Left it as v0 for now
    - 4f states: by Ja and Omega and grouped as above
        - Note, it also has a fit to the [561] and [557] perturbed states. Those
          are labelled as [18.71] and [18.58]. The dictionaries don't have nested 
          structures like in others.

### Questions:
    - Do we need Hönl-London intensity factors? It wasn't included in the 
      original Mathematica code. 

@author: sl5119, Simeng Li
"""

#%% Packages and constants
import numpy as np
import pandas as pd
import matplotlib.pyplot as plt
from matplotlib.backends.backend_pdf import PdfPages
from scipy.signal import find_peaks, savgol_filter
import seaborn as sns
#import matplotlib.lines as mlines  #This only works when called inside the function

import os
import tkinter as tk
from tkinter import filedialog
from scipy.optimize import curve_fit
import math

from scipy.optimize import least_squares

c = 29.9792458 # speed of light in in cm*GHz
hck = 6.626*1.38065*2.99792458e-1 # h*c/k_B in cm/K to get rid of cm^-1 when 
                                  # doing Boltzmann distribution

# Conversion factor: c (in cm/s) / 1e12 Hz/THz = 0.0299792458
CM_INV_TO_THZ = 2.99792458e10 / 1e12  # 0.0299792458

#%% Spectroscopy Constants
'''All constants are using the highest precision version from Jongseok
   in his July 2026 Mathmatica notebook.
'''
'''  For X Σ ground states:
    (iso, tv, b, d, g0, g1, g2, Xb, Xc, XC)

'''

# ==============================================================================
# RESTRUCTURED SPECTROSCOPIC CONSTANTS
# Hierarchy: State -> Vibrational Level ("v0", "v1", ...) -> Isotope (174, 172, 176)
# ==============================================================================

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


''' For A Pi 1/2 excited states, Dunfield paper:          
        *This only has v=0 because v=1 is strongly mixed with 4f Ja=5/2.
        See [561] and [557] below.
        **The "v0" label is kept here to keep dictionary structure consistent
'''
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


''' For A Pi 3/2 excited states, Dunfield paper:                  '''
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

''' Updated in Aug 2026, Simeng's re-fit. Sig-fig doesn't represent precision'''
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

''' For the perturbed states, [561] and [557]
    Note:
        They originate from mixing of A Pi 1/2 v=1 and 4f Ja=5/2. The old
        labelling was [18.6]0.5 v=0 and v=1 respectively.
    
    [561] are Jongseok's constants from his spectra (2017 paper)
    [557] are Dunfield's constants
'''
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


''' For 4f states from Stefan Popa's 2024 paper
    It also includes a new fit to the [561] and [557] perturbed states
    using the 4f model. They're labelled as [18.71] and [18.58].
'''

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
# --- Perturbed / Transition States (Stefan 2024 model fits) ---

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
              

#%% Energy models
'''X Σ, Ben's' paper [Sauer et al., JCP 105, 7412 (1996)]
    F1 refers to J=N+1/2.
    tv is the energy of the vibrational state.
    
    Effective hamiltonian:
        H = γ * dot_product(S*N) + b * dot_product(I*S) + c*I_z*S_z + C * dot_product(I*N)
    
    The hyperfine levels are grouped by J for each N level.
    Note in equations n is used for quantum number N, to be consistent with 
    equations in Mathmatica notebooks.
'''
def eRotor(n, Bpp, dpp, **kwargs):
    '''
    The rigid rotor model(?)
    This gives the rotational level

    Parameters
    ----------
    n : int
        Rotation quantum number.
    Bpp : TYPE
        DESCRIPTION.
    dpp : TYPE
        DESCRIPTION.

    '''
    return Bpp*n*(n+1) - dpp*n**2*(n+1)**2

def gamma_spinRot(n, g0, g1, g2, **kwargs):
    '''
    Spin-rotation "constant"
    Expansion contracted to the 2nd power of (n+1)
    
    g0, 1, 2... are the constant of each power of (n+1) terms.
    This accounts for centrifugal effects under spin-rotation coupling 
    
    *The g here are short for gamma, which are the hyperfine parameters
    
    Parameters
    ----------
    n : int
        Rotation quantum number.
    g0 : flaot
        Leading term of the spin-rotation "constant".
    g1 : float
        Centrifugal distortion term.
    g2 : float
        Quadratic centrifual correction.

    '''
    return g0 + g1*n*(n+1) + g2*n**2*(n+1)**2

def F1(n, Tv, Bpp, dpp, g0, g1, g2, Xb, Xc, XC, **kwargs):
    '''
    Generate hyperfine energy levels of the e states, J=N+1/2
    Eqn A2 & A4
    
    Parameters (neglect duplicates from equations above)
    ----------
    tv : float
        Initial offset(?).
    Xb : float
        Same as b in eRotor.
    Xc : float
        DESCRIPTION.
    XC : float
        DESCRIPTION.

    Returns
    -------
    1D array (term1, term2), float
        term1: E_N+1 for the F = N+1 hyperfine level, eqn A2
        term2: E_-N for the F = N hyperfine level, eqn A4
               This is labelled as the N_l state

    '''
    common = Tv + eRotor(n, Bpp, dpp)
    gamma = gamma_spinRot(n, g0, g1, g2)
    term1 = gamma * n/2 + Xb/4 + Xc/(4*(2*n+3)) + XC*n/2
    term2 = -(gamma + Xb + XC)/4 - np.sqrt((gamma-XC)**2 * (2*n+1)**2 +\
            (2*Xb + Xc - 2*XC) * (2*Xb + Xc - 2*gamma))/4
    
    return np.array([term1, term2]) + common

def F2(n, Tv, Bpp, dpp, g0, g1, g2, Xb, Xc, XC, **kwargs):
    '''
    Generate hyperfine energy levels of the f states, J=N-1/2
    Eqn A2 & A4

    Returns
    -------
    1D array (term1, term2), float
        term1: E_N-1 for the F = N-1 hyperfine level, eqn A2
        term2: E_+N for the F = N hyperfine level, eqn A4
               This is labelled as the N_h state

    '''
    common = Tv + eRotor(n, Bpp, dpp)
    gamma = gamma_spinRot(n, g0, g1, g2)
    term1 = -gamma * (n+1)/2 + Xb/4 - Xc/(4*(2*n-1)) - XC*(n+1)/2
    term2 = -(gamma + Xb + XC)/4 + np.sqrt((gamma-XC)**2 * (2*n+1)**2 +\
            (2*Xb + Xc - 2*XC) * (2*Xb + Xc - 2*gamma))/4
    
    if n == 0:
        return np.array([np.nan, np.nan]) + common  #can't have negative J = N-1/2
    else:
        return np.array([term1, term2]) + common

def X_hyperfine(n, Tv, Bpp, dpp, g0, g1, g2, Xb, Xc, XC, **kwargs):
    '''
    Generate hyperfine energy levels of the X state
    Eqn A2 & A4
    
    This replaces the separate F1 & F2 definitions
    
    Parameters (neglect duplicates from equations above)
    ----------
    tv : float
        Initial offset.
    Xb : float
        Same as b in eRotor.
    Xc : float
        DESCRIPTION.
    XC : float
        DESCRIPTION.

    Returns
    -------
    *Originally F1: J=N+1
    1D array (term1, term2), float
        term1: E_N+1 for the F = N+1 hyperfine level, eqn A2
        term2: E_-N for the F = N hyperfine level, eqn A4

    *Originally F2: J=N-1
    1D array (term1, term2), float
        term1: E_N-1 for the F = N-1 hyperfine level, eqn A2
        term2: E_+N for the F = N hyperfine level, eqn A4


    '''
    
    common = Tv + eRotor(n, Bpp, dpp)
    gamma = gamma_spinRot(n, g0, g1, g2)
    
    #Terms that was in F1:
    term_Nplus1 = gamma * n/2 + Xb/4 + Xc/(4*(2*n+3)) + XC*n/2
    term_minusN = -(gamma + Xb + XC)/4 - np.sqrt((gamma-XC)**2 * (2*n+1)**2 +\
            (2*Xb + Xc - 2*XC) * (2*Xb + Xc - 2*gamma))/4
        
    #Terms that was in F2:
    term_Nminus1 = -gamma * (n+1)/2 + Xb/4 - Xc/(4*(2*n-1)) - XC*(n+1)/2
    term_plusN = -(gamma + Xb + XC)/4 + np.sqrt((gamma-XC)**2 * (2*n+1)**2 +\
            (2*Xb + Xc - 2*XC) * (2*Xb + Xc - 2*gamma))/4
        
    if n == 0:
        return np.array([term_Nplus1, term_minusN, np.nan, np.nan]) + common  #can't have negative J = N-1/2
    else:
        return np.array([term_Nplus1, term_minusN, term_plusN, term_Nminus1]) + common
    

'''A Π_1/2, 1995 Dunfield paper [J Mol Spectroscopy 174, 433 (1995)]
'''
def Ue(j, t, b, ad, d, p2q, dp2q, **kwargs):
    """
    e-parity diagonal energy for <Pi_{1/2}|Pi_{1/2}>
    """
    j_half = j + 0.5
    rotational_term = (b - ad / 2) * (j_half**2)
    distortion_term = d * (j_half**4 + j_half**2 - 1)
    lambda_doubling = j_half * (p2q + dp2q * (j_half**2)) / 2
    
    return t + rotational_term - distortion_term - lambda_doubling


def Uf(j, t, b, ad, d, p2q, dp2q, **kwargs):
    """
    f-parity diagonal energy for <Pi_{1/2}|Pi_{1/2}>
    """
    j_half = j + 0.5
    rotational_term = (b - ad / 2) * (j_half**2)
    distortion_term = d * (j_half**4 + j_half**2 - 1)
    lambda_doubling = j_half * (p2q + dp2q * (j_half**2)) / 2
    
    return t + rotational_term - distortion_term + lambda_doubling

'''A Π_3/2, 1995 Dunfield paper [J Mol Spectroscopy 174, 433 (1995)]

   The equations for e and f are actually identical because ΔΩ = 3 here so
   there's no first order Λ-doubling term.
'''
def Ue3JCP(j, t, b, ad, d, **kwargs):
    """
    e-parity diagonal energy for <Pi_{3/2}|Pi_{3/2}>
    """
    j_half = j + 0.5
    rotational_term = (b + ad / 2) * (j_half**2 - 2)
    distortion_term = d * (j_half**4 - 3 * (j_half**2) + 3)
    
    return t + rotational_term - distortion_term


def Uf3JCP(j, t, b, ad, d, **kwargs):
    """
    f-parity diagonal energy for <Pi_{3/2}|Pi_{3/2}>
    """
    j_half = j + 0.5
    rotational_term = (b + ad / 2) * (j_half**2 - 2)
    distortion_term = d * (j_half**4 - 3 * (j_half**2) + 3)
    
    return t + rotational_term - distortion_term

'''4f states, Stefan Popa's paper (2024), eqn 9'''
def Ue4f(j, t, b, delta, zeta, **kwargs):
    """
    e-parity energy for 4f state
    """
    rotational_eff = b + (15 * b**2) / delta
    spin_rotation = (b / 2) * zeta * (2 * j + 1)
    
    return t + rotational_eff * j * (j + 1) - spin_rotation


def Uf4f(j, t, b, delta, zeta, **kwargs):
    """
    f-parity energy for 4f state
    """
    rotational_eff = b + (15 * b**2) / delta
    spin_rotation = (b / 2) * zeta * (2 * j + 1)
    
    return t + rotational_eff * j * (j + 1) + spin_rotation



#%% Functions to calculate branches
'''Regarding branch naming (Dunfield paper, page 2-3):
    "Typical 2P1/2–2S/ transitions consist of six
    branches, P12, (Q1, R12), (P1, Q12), and R1 (7). The branches in parentheses 
    are only observed as separate branches if the spin–rotation splitting in the
    2Sigma state is resolved.
    In future discussions, they will be referred to as Q1 and P1, respectively. 
    The first three branches come from the f levels in the 2Pi state and the last 
    three branches are from the e levels."
    
   For clarity, I'm labelling them as P_12, (Q_11, R_12), (P_11, Q_12), and R_11

Changes from Aug.14th 2026:
    - X ground states are merged into 1 equation for all hyperfine levels
    - New branch definitions: (assume the effective J of each N is J=N+1)
        - O-branch: ΔJ = -2, ie the old P_12 branch. 2 hyperfine transitions.
        - P-branch: ΔJ = -1, ie the old (P_11 + Q_12) branch. 4 hyperfine.
        - Q_branch: ΔJ = 0, ie the old (Q_11 + R_12) branch. 4 hyperfine.
        - R_branch: ΔJ = +1, ie the old R_11 branch. 2 hyperfine transitions.


I'm keeping the old functions for now in case we want to go back to them.
'''

# =============================================================================
# Refactored Ground State Manifold (4 Branches: O, P, Q, R)
# =============================================================================

def _transition_energy_X(j_prime, excited_params, e_func, n_double_prime, ground_params, mask=None):
    """
    Calculates transition energy array for hyperfine components of a ground N level.
    `mask` selects specific hyperfine levels [F=N+1, F=N_minus, F=N_plus, F=N-1].
    """
    energies = e_func(j_prime, **excited_params) - X_hyperfine(n_double_prime, **ground_params)
    if mask is not None:
        energies = energies[mask]
    return energies

def O_X(n, excited_params, ground_params, e_func=Ue):
    """O branch (Delta J = -2): J' = J''- 2 where J''= N + 1/2. Contains F = N+ and F = N-1 (indices 2, 3)."""
    return _transition_energy_X(n+0.5 - 2.0, excited_params, e_func, n, ground_params, mask=[2, 3])

def P_X(n, excited_params, ground_params, e_func=Ue):
    """P branch (Delta J = -1): J' = J''- 1 where J''= N + 1/2. Contains all 4 hyperfine transitions."""
    return _transition_energy_X(n+0.5 - 1.0, excited_params, e_func, n, ground_params)

def Q_X(n, excited_params, ground_params, e_func=Uf):
    """Q branch (Delta J = 0): J' = J'' where J''= N + 1/2. Contains all 4 hyperfine transitions."""
    return _transition_energy_X(n+0.5, excited_params, e_func, n, ground_params)

def R_X(n, excited_params, ground_params, e_func=Ue):
    """R branch (Delta J = +1): J' = J''+ 1 where J''= N + 1/2. Contains F = N+1 and F = N- (indices 0, 1)."""
    return _transition_energy_X(n+0.5 + 1.0, excited_params, e_func, n, ground_params, mask=[0, 1])


# Dictionary mappings updated for O, P, Q, R scheme
# Format: (branch, J' calculation from J''=N+0.5, excited state function)
x_APi12_branches_new = {
    "O": (O_X, lambda n: n+0.5 - 2.0, Uf),
    "P": (P_X, lambda n: n+0.5 - 1.0, Ue),
    "Q": (Q_X, lambda n: n+0.5, Uf),
    "R": (R_X, lambda n: n+0.5 + 1.0, Ue),
}

x_APi32_branches_new = {
    "O": (O_X, lambda n: n+0.5 - 2.0, Uf3JCP),
    "P": (P_X, lambda n: n+0.5 - 1.0, Ue3JCP),
    "Q": (Q_X, lambda n: n+0.5, Uf3JCP),
    "R": (R_X, lambda n: n+0.5 + 1.0, Ue3JCP),
}

# Maps hyperfine quantum number to line in branches
x_branches_hyperfine = {
    "O": ["N+", "N-1"],
    "P": ["N+1", "N-", "N+", "N-1"],
    "Q": ["N+1", "N-", "N+", "N-1"],
    "R": ["N+1", "N-"]
    }

'''X ground state needs its own set of equations since J is not a good quantum number'''
# Helper core to handle the energy difference
def _transition_energy(j_prime, excited_params, e_func, j_double_prime, ground_params, g_func):
    '''
    Set equation form for branch transition calculations

    Parameters
    ----------
    j_prime : float or int
        Excited state angular momentum quantum number.
    excited_params : dictionary
        DESCRIPTION.
    e_func : function name
        DESCRIPTION.
    j_double_prime : float or int
        Ground state angular momentum quantum number.
    ground_params : dictionary
        DESCRIPTION.
    g_func : function name
        DESCRIPTION.

    Returns
    -------
    TYPE
        DESCRIPTION.
    '''
    return e_func(j_prime, **excited_params) - g_func(j_double_prime, **ground_params)


# =============================================================================
# X Ground State Manifold (N-indexed, Hund's case b)
# =============================================================================

def P_11_X(n, excited_params, ground_params, e_func=Ue, g_func=F1):
    """P11 branch: Delta J = -1 (J' = N - 1/2, upper e-parity)"""
    return _transition_energy(n - 0.5, excited_params, e_func, n, ground_params, g_func)

def Q_11_X(n, excited_params, ground_params, e_func=Uf, g_func=F1):
    """Q11 branch: Delta J = 0 (J' = N + 1/2, upper f-parity)"""
    return _transition_energy(n + 0.5, excited_params, e_func, n, ground_params, g_func)

def R_11_X(n, excited_params, ground_params, e_func=Ue, g_func=F1):
    """R11 branch: Delta J = +1 (J' = N + 3/2, upper e-parity)"""
    return _transition_energy(n + 1.5, excited_params, e_func, n, ground_params, g_func)

def P_12_X(n, excited_params, ground_params, e_func=Uf, g_func=F2):
    """P12 branch: Delta J = -1 (J' = N - 3/2, upper f-parity)"""
    return _transition_energy(n - 1.5, excited_params, e_func, n, ground_params, g_func)

def Q_12_X(n, excited_params, ground_params, e_func=Ue, g_func=F2):
    """Q12 branch: Delta J = 0 (J' = N - 1/2, upper e-parity)"""
    return _transition_energy(n - 0.5, excited_params, e_func, n, ground_params, g_func)

def R_12_X(n, excited_params, ground_params, e_func=Uf, g_func=F2):
    """R12 branch: Delta J = +1 (J' = N + 1/2, upper f-parity)"""
    return _transition_energy(n + 0.5, excited_params, e_func, n, ground_params, g_func)



'''Generalised for the rest, where J is a good quantum number in both 
   excited state and ground state. For now it's most useful for 4f transitions

   The equations for excited and ground states need to be specified by user as
   it depends on the application. 
   
   Default is set to 4f transitions.
   
   Label example: P_(ground state e/f-parity)(excited state e/f-parity)
'''


# =============================================================================
# General J-Good Quantum Number States (J-indexed, e.g., 4f transitions)
# =============================================================================

def Pff(j, excited_params, ground_params, e_func=Uf, g_func=Uf4f):
    """P branch: Delta J = -1 (ground f-parity, excited f-parity)"""
    return _transition_energy(j - 1, excited_params, e_func, j, ground_params, g_func)

def Qfe(j, excited_params, ground_params, e_func=Ue, g_func=Uf4f):
    """Q branch: Delta J = 0 (ground f-parity, excited e-parity)"""
    return _transition_energy(j, excited_params, e_func, j, ground_params, g_func)

def Rff(j, excited_params, ground_params, e_func=Uf, g_func=Uf4f):
    """R branch: Delta J = +1 (ground f-parity, excited f-parity)"""
    return _transition_energy(j + 1, excited_params, e_func, j, ground_params, g_func)

def Pee(j, excited_params, ground_params, e_func=Ue, g_func=Ue4f):
    """P branch: Delta J = -1 (ground e-parity, excited e-parity)"""
    return _transition_energy(j - 1, excited_params, e_func, j, ground_params, g_func)

def Qef(j, excited_params, ground_params, e_func=Uf, g_func=Ue4f):
    """Q branch: Delta J = 0 (ground e-parity, excited f-parity)"""
    return _transition_energy(j, excited_params, e_func, j, ground_params, g_func)

def Ree(j, excited_params, ground_params, e_func=Ue, g_func=Ue4f):
    """R branch: Delta J = +1 (ground e-parity, excited e-parity)"""
    return _transition_energy(j + 1, excited_params, e_func, j, ground_params, g_func)

# Dictionary linking branch names to their respective functions and J' offset formulas
# lambda n creats an lambda function of n, which basically calculates J' for each n
x_APi12_branches = {
    # Format: (func, j_prime_calc, j_double_prime_calc, e_func, g_func)
    "P11": (P_11_X, lambda n: n - 0.5, lambda n: n + 0.5, Ue, F1),
    "Q11": (Q_11_X, lambda n: n + 0.5, lambda n: n + 0.5, Uf, F1),
    "R11": (R_11_X, lambda n: n + 1.5, lambda n: n + 0.5, Ue, F1),
    "P12": (P_12_X, lambda n: n - 1.5, lambda n: n - 0.5, Uf, F2),
    "Q12": (Q_12_X, lambda n: n - 0.5, lambda n: n - 0.5, Ue, F2),
    "R12": (R_12_X, lambda n: n + 0.5, lambda n: n - 0.5, Uf, F2),
}

x_APi32_branches = {
    # Format: (func, j_prime_calc, j_double_prime_calc, e_func, g_func)
    "P11": (P_11_X, lambda n: n - 0.5, lambda n: n + 0.5, Ue3JCP, F1),
    "Q11": (Q_11_X, lambda n: n + 0.5, lambda n: n + 0.5, Uf3JCP, F1),
    "R11": (R_11_X, lambda n: n + 1.5, lambda n: n + 0.5, Ue3JCP, F1),
    "P12": (P_12_X, lambda n: n - 1.5, lambda n: n - 0.5, Uf3JCP, F2),
    "Q12": (Q_12_X, lambda n: n - 0.5, lambda n: n - 0.5, Ue3JCP, F2),
    "R12": (R_12_X, lambda n: n + 0.5, lambda n: n - 0.5, Uf3JCP, F2),
}

def GetSpectra_N(n_ground_start, n_ground_end, transition, excited_params, \
                 ground_params, temp):
    lines = []

    for branch_name, (func, j_prime_calc, j_dp_calc, e_func, g_func) in transition.items():
        # N starts at 0 for Hund's case (b) ground states
        for n in range(n_ground_start, n_ground_end+1):
            j_prime = j_prime_calc(n)
            j_double_prime = j_dp_calc(n)

            # Quantum mechanics check: Neither J' nor J'' can be less than 0.5
            if j_prime < 0.5 or j_double_prime < 0.5:
                continue

            # Calculate transition wavenumber
            nu = func(
                n,
                excited_params=excited_params,
                ground_params=ground_params,
                e_func=e_func,
                g_func=g_func
            )
            
            # Convert to frequency in THz
            freq_thz = nu * CM_INV_TO_THZ
            
            # Ground state rotational energy in cm^-1 (xRotor equivalent)
            XRotor = eRotor(n, **ground_params)

            # Thermal population calculation matching: iso * (2N + 1) * Exp[-hck * E_ground / T]
            population = ground_params["iso"] * (2*n+1) * np.exp(- (hck * XRotor) / temp)
            
            lines.append({
                "Branch": branch_name,
                "N''": n,
                "J'": j_prime,
                "Wavenumber_cm-1": nu,
                "Frequency_THz": freq_thz,
                "Population": population
            })

    # Return as a clean Pandas DataFrame
    return pd.DataFrame(lines)

def GetSpectra_N_new(n_ground_start, n_ground_end, transition, excited_params, ground_params, temp):
    lines = []

    for branch_name, (func, j_prime_calc, e_func) in transition.items():
        for n in range(n_ground_start, n_ground_end + 1):
            j_prime = j_prime_calc(n)

            # Quantum mechanics check: J' cannot be less than 0.5
            if j_prime < 0.5:
                continue

            # Transition frequency/wavenumber
            nu = func(
                n,
                excited_params=excited_params,
                ground_params=ground_params,
                e_func=e_func
            )
            
            # Convert to frequency in THz
            freq_thz = nu * CM_INV_TO_THZ
            
            # Ground state rotational energy in cm^-1 (xRotor equivalent)
            XRotor = eRotor(n, **ground_params)

            # Thermal population calculation: iso * (2N + 1) * Exp[-hck * E_ground / T]
            population = ground_params["iso"] * (2 * n + 1) * np.exp(-(hck * XRotor) / temp)
            
            lines.append({
                "Branch": branch_name,
                "N''": n,
                "J'": j_prime,
                "Wavenumber_cm-1": nu,
                "Frequency_THz": freq_thz,
                "Population": population
            })

    return pd.DataFrame(lines)

# =============================================================================
# Branch Mapping for J-Good States (e.g., 4f -> 4f or 4f -> A transitions)
# =============================================================================
branches_4f = {
    # Format: (func, j_prime_calc, e_func, g_func)
    ### Note, the constants for upper states need to use Popa's constants
    "Pff": (Pff, lambda j: j - 1.0, Uf4f, Uf4f),
    "Qfe": (Qfe, lambda j: j,       Ue4f, Uf4f),
    "Rff": (Rff, lambda j: j + 1.0, Uf4f, Uf4f),
    "Pee": (Pee, lambda j: j - 1.0, Ue4f, Ue4f),
    "Qef": (Qef, lambda j: j,       Uf4f, Ue4f),
    "Ree": (Ree, lambda j: j + 1.0, Ue4f, Ue4f),
}

branches_4f_APi12 = {
    # Format: (func, j_prime_calc, e_func, g_func)
    ### Note, the constants for upper states need to use Popa's constants
    "Pff": (Pff, lambda j: j - 1.0, Uf, Uf4f),
    "Qfe": (Qfe, lambda j: j,       Ue, Uf4f),
    "Rff": (Rff, lambda j: j + 1.0, Uf, Uf4f),
    "Pee": (Pee, lambda j: j - 1.0, Ue, Ue4f),
    "Qef": (Qef, lambda j: j,       Uf, Ue4f),
    "Ree": (Ree, lambda j: j + 1.0, Ue, Ue4f),
}

branches_4f_APi32 = {
    # Format: (func, j_prime_calc, e_func, g_func)
    ### Note, the constants for upper states need to use Popa's constants
    "Pff": (Pff, lambda j: j - 1.0, Uf3JCP, Uf4f),
    "Qfe": (Qfe, lambda j: j,       Ue3JCP, Uf4f),
    "Rff": (Rff, lambda j: j + 1.0, Uf3JCP, Uf4f),
    "Pee": (Pee, lambda j: j - 1.0, Ue3JCP, Ue4f),
    "Qef": (Qef, lambda j: j,       Uf3JCP, Ue4f),
    "Ree": (Ree, lambda j: j + 1.0, Ue3JCP, Ue4f),
}


def GetSpectra_J(j_ground_start, j_ground_end, transition, excited_params, \
                 ground_params, temp, ref_func=Ue4f, j_ref=1.5):
    """
    Generate transition spectra for systems where J is a good quantum number 
    in both lower and upper states (e.g., 4f manifolds).
    
    Parameters
    ----------
    j_ground_start : float
        Starting J'' value (e.g., 0.5 for half-integer states).
    j_ground_end : float
        Ending J'' value.
    transition : dict
        Branch dictionary containing (func, j_prime_calc, e_func, g_func).
    excited_params : dict
        Parameters for upper state.
    ground_params : dict
        Parameters for lower state.
    temp : float
        Temperature in Kelvin for Boltzmann population weighting.
    ref_func : function name
        Ground state energy function for calculating the reference energy. 
        Default is Ue4f for 4f states.
    j_ref : float
        A fixed J for calculating the reference energy. Default is 1.5 for 
        4f states
        
    Returns
    -------
    pd.DataFrame
        DataFrame with Branch, J'', J', Wavenumber, Frequency (THz), and Population.
    """
    lines = []

    # Step by 1.0 to preserve half-integer (or integer) J values
    j_values = np.arange(j_ground_start, j_ground_end + 0.5, 1.0)
    # Ground reference energy at j_ground_start (e.g., J'' = 1.5)
    e_ref = ref_func(j_ref, **ground_params)
    
    for branch_name, (func, j_prime_calc, e_func, g_func) in transition.items():
        for j in j_values:
            j_prime = j_prime_calc(j)

            # Quantum mechanics check: J' and J'' cannot be unphysical (< 0.5)
            if j < 0.5 or j_prime < 0.5:
                continue

            # Calculate transition wavenumber (cm^-1)
            nu = func(
                j,
                excited_params=excited_params,
                ground_params=ground_params,
                e_func=e_func,
                g_func=g_func
            )

            # Convert to frequency in THz
            freq_thz = nu * CM_INV_TO_THZ

            # Energy relative to J'' = j_ground_start (e.g. 1.5)
            e_ground = g_func(j, **ground_params)
            e_rel = e_ground - e_ref

            # Boltzmann thermal population: iso * (2J'' + 1) * exp(-hck * E_ground / T)
            iso = ground_params.get("iso", 1.0)
            population = iso * (2 * j + 1) * np.exp(- (hck * e_rel) / temp)

            lines.append({
                "Branch": branch_name,
                "J''": j,
                "J'": j_prime,
                "Wavenumber_cm-1": nu,
                "Frequency_THz": freq_thz,
                "Population": population
            })

    return pd.DataFrame(lines)

#%% Some handy functions to assist plotting
def extract_branch_data(df, use_wavenumber=False):
    """
    Extracts branch-by-branch arrays from GetSpectra_N or GetSpectra_J DataFrames.

    Parameters
    ----------
    df : pd.DataFrame
        Output DataFrame from GetSpectra_N or GetSpectra_J.
    use_wavenumber : bool, default=False
        If True, returns 'Wavenumber_cm-1' for x. Otherwise returns 'Frequency_THz'.

    Returns
    -------
    dict
        A dictionary mapping each branch name (e.g., 'P11') to a sub-dict:
        {
            'x': np.ndarray,      # Frequency (THz) or Wavenumber (cm^-1)
            'y': np.ndarray,      # Population (Intensity)
            'q_num': np.ndarray   # Ground state N'' or J'' quantum numbers
        }
    """
    # Identify whether the quantum number is N'' or J''
    q_col = "N''" if "N''" in df.columns else "J''"
    freq_col = "Wavenumber_cm-1" if use_wavenumber else "Frequency_THz"

    branch_data = {}
    for branch, group in df.groupby("Branch", sort=False):
        raw_x = group[freq_col].to_numpy()
        raw_y = group["Population"].to_numpy()
        raw_q = group[q_col].to_numpy()

        # Check if entries are 2-element arrays/lists (e.g., X-state hyperfine split)
        if len(raw_x) > 0 and isinstance(raw_x[0], (np.ndarray, list, tuple)):
            # Count elements per row (e.g., 2) to dynamically repeat metadata
            repeats = [len(item) for item in raw_x]
            
            x_flat = np.concatenate(raw_x).astype(float)
            y_flat = np.repeat(raw_y, repeats).astype(float)
            q_flat = np.repeat(raw_q, repeats)
        else:
            x_flat = raw_x.astype(float)
            y_flat = raw_y.astype(float)
            q_flat = raw_q

        branch_data[branch] = {
            "x": x_flat,
            "y": y_flat,
            "q_num": q_flat
        }

    return branch_data


def plot_ybf_spectrum(
    df,
    center_freq_thz=568.664588,
    xlim=None,  # e.g., (-50, 50) in GHz, or None for auto-scale
    use_wavenumber=False,
    measured_x=None,  # Optional 1D array of measured x-data
    measured_y=None,  # Optional 1D array of measured y-data
    measured_in_thz=False,  # If True, converts measured_x from THz to GHz offset
    scale_sticks_to_measured=True,  # Scales theoretical stick heights to match measured peak height
    min_stick_fraction=0.02,  # Minimum stick height as a fraction of peak height (e.g., 0.02 = 2%)
    stick_offset_ghz=0.0,  # Global offset in GHz applied to theoretical sticks
    figsize=(11, 5),
):
    """Plots theoretical stick spectrum with optional measured spectrum overlay,

    scaling theoretical stick heights to match measured data, applying a
    minimum height floor, and applying a global X-offset to the sticks.
    """
    # 1. Extract branch data
    branches = extract_branch_data(df, use_wavenumber=use_wavenumber)

    # 2. Apply frequency offset to convert THz offset -> GHz for sticks (+ global stick_offset_ghz)
    for branch_name, data in branches.items():
        data["x_offset"] = (
            (data["x"] - center_freq_thz) * 1000.0
        ) + stick_offset_ghz

    # 3. Process measured spectrum if provided
    has_measured = measured_x is not None and measured_y is not None
    if has_measured:
        m_x = np.asarray(measured_x, dtype=float)
        m_y = np.asarray(measured_y, dtype=float)

        # Convert THz -> GHz offset if needed
        if measured_in_thz:
            m_x = (m_x - center_freq_thz) * 1000.0

        # Scale theoretical sticks to match measured peak height
        if scale_sticks_to_measured:
            max_stick = max(df["Population"])

            # Find peak measured signal (in visible region if xlim is specified)
            if xlim is not None:
                mask_m = (
                    (m_x >= xlim[0]) & (m_x <= xlim[1]) & (~np.isnan(m_y))
                )
                max_meas = (
                    np.nanmax(m_y[mask_m])
                    if np.any(mask_m)
                    else np.nanmax(m_y)
                )
            else:
                max_meas = np.nanmax(m_y) if len(m_y) > 0 else 0

            # Multiply stick heights by scaling ratio
            if max_stick > 0 and max_meas > 0 and not np.isnan(max_meas):
                scale_factor = max_meas / max_stick
                for data in branches.values():
                    data["y"] = data["y"] * scale_factor

    # 4. Determine y-limit based on visible range (xlim)
    if xlim is not None:
        visible_y = []
        for data in branches.values():
            mask = (data["x_offset"] >= xlim[0]) & (data["x_offset"] <= xlim[1])
            visible_y.extend(data["y"][mask])

        if has_measured:
            mask_m = (
                (m_x >= xlim[0]) & (m_x <= xlim[1]) & (~np.isnan(m_y))
            )
            if np.any(mask_m):
                visible_y.extend(m_y[mask_m])

        max_pop = np.nanmax(visible_y) if visible_y else max(df["Population"])
    else:
        all_stick_y = [y for data in branches.values() for y in data["y"]]
        max_stick_y = (
            max(all_stick_y) if all_stick_y else max(df["Population"])
        )
        max_pop = (
            max(max_stick_y, np.nanmax(m_y)) if has_measured else max_stick_y
        )

    # Calculate threshold floor so zero/weak intensity lines stay visible
    min_height_floor = (
        max_pop * min_stick_fraction if min_stick_fraction is not None else 0.0
    )

    # 5. Plot setup
    fig, ax = plt.subplots(figsize=figsize)
    cmap = plt.get_cmap("tab10")

    # Plot measured continuous trace underneath sticks (zorder=1)
    if has_measured:
        ax.plot(
            m_x,
            m_y,
            color="black",
            alpha=0.35,
            linewidth=1.2,
            label="Measured",
            zorder=1,
        )

    # Plot theoretical sticks on top (zorder=2)
    for i, (branch_name, data) in enumerate(branches.items()):
        color = cmap(i % 10)
        x_vals = data["x_offset"]

        # Clamp stick heights to the minimum threshold floor
        y_vals = (
            np.maximum(data["y"], min_height_floor)
            if min_height_floor > 0
            else data["y"]
        )

        ax.vlines(
            x=x_vals,
            ymin=0,
            ymax=y_vals,
            label=branch_name,
            color=color,
            linewidth=1.8,
            zorder=2,
        )

        # Annotate quantum numbers (N'' or J'')
        for x_val, y_val, q_val in zip(x_vals, y_vals, data["q_num"]):
            if xlim is not None and not (xlim[0] <= x_val <= xlim[1]):
                continue

            q_label = (
                str(int(q_val))
                if float(q_val).is_integer()
                else f"{q_val:.1f}"
            )

            ax.text(
                x=x_val,
                y=y_val + (max_pop * 0.015),
                s=q_label,
                ha="center",
                va="bottom",
                fontsize=8,
                color=color,
                zorder=3,
            )

    # 6. Apply xlim if provided
    if xlim is not None:
        ax.set_xlim(xlim)

    # 7. Styling
    ax.set_xlabel(
        f"Frequency Offset (GHz) from {center_freq_thz} (THz)", fontsize=11
    )
    ax.set_ylabel("Relative Population / Intensity", fontsize=11)
    ax.set_title(
        r"YbF $A\,^2\Pi_{3/2} \leftarrow X\,^2\Sigma^+$ Spectrum"
        r" ($^{174}\mathrm{YbF}, v''=1$), with offset %g GHz to predictions"%stick_offset_ghz,
        fontsize=12,
    )

    ax.set_ylim(bottom=0, top=max_pop * 1.15)
    ax.grid(True, linestyle=":", alpha=0.5)
    ax.legend(
        title="Branch / Trace",
        bbox_to_anchor=(1.02, 1),
        loc="upper left",
        frameon=True,
    )

    plt.tight_layout()
    return fig, ax

def plot_ybf_spectrum_multi_isotope(
    dfs_by_isotope,  # Dict: {174: df_174, 172: df_172, 176: df_176}
    center_freq_thz=568.664588,
    xlim=None,  # e.g., (-50, 50) in GHz
    use_wavenumber=False,
    measured_x=None,
    measured_y=None,
    measured_in_thz=False,
    scale_sticks_to_measured=True,
    min_stick_fraction=0.02,
    stick_offset_ghz=0.0,  # Accepts float/int OR Dict: {174: -3.15, 172: -1.8, 176: -2.0}
    x_branches_hyperfine=None,  # Dict mapping branch -> hyperfine labels
    figsize=(12, 5),
    ytitle="Relative Population / Intensity",
):
    """Plots multi-isotope theoretical stick spectra overlaid on measured data.

    Line styles:
    - 174: Solid ('-')
    - 172: Dashed ('--')
    - 176: Dotted (':')
    """
    
    # Default hyperfine mapping if none provided
    if x_branches_hyperfine is None:
        x_branches_hyperfine = {
            "O": ["N+", "N-1"],
            "P": ["N+1", "N-", "N+", "N-1"],
            "Q": ["N+1", "N-", "N+", "N-1"],
            "R": ["N+1", "N-"],
        }
    
    # Define line style and label mapping per isotope
    style_map = {
        174: {"linestyle": "-", "label": "$^{174}$YbF"},
        172: {"linestyle": "--", "label": "$^{172}$YbF"},
        176: {"linestyle": ":", "label": "$^{176}$YbF"},
    }

    import matplotlib.lines as mlines

    # Normalize stick_offset_ghz into a dictionary mapping isotope -> offset
    if isinstance(stick_offset_ghz, dict):
        offset_map = stick_offset_ghz
    elif isinstance(stick_offset_ghz, (int, float)):
        offset_map = {iso: float(stick_offset_ghz) for iso in dfs_by_isotope.keys()}
    else:
        offset_map = {iso: 0.0 for iso in dfs_by_isotope.keys()}

    # 1. Process measured spectrum if provided
    has_measured = measured_x is not None and measured_y is not None
    if has_measured:
        m_x = np.asarray(measured_x, dtype=float)
        m_y = np.asarray(measured_y, dtype=float)
        if measured_in_thz:
            m_x = (m_x - center_freq_thz) * 1000.0

    # 2. Extract branch data for all isotopes and apply isotope-specific offsets
    all_iso_branches = {}
    max_stick_174 = 0.0

    for iso, df in dfs_by_isotope.items():
        iso_offset = offset_map.get(iso, 0.0)
        branches = extract_branch_data(df, use_wavenumber=use_wavenumber)
        for branch_name, data in branches.items():
            data["x_offset"] = (
                (data["x"] - center_freq_thz) * 1000.0
            ) + iso_offset

        all_iso_branches[iso] = branches

        if iso == 174:
            max_stick_174 = max(df["Population"])

    # Fallback if 174 isn't in the dataset
    if max_stick_174 == 0.0 and dfs_by_isotope:
        first_df = next(iter(dfs_by_isotope.values()))
        max_stick_174 = max(first_df["Population"])

    # 3. Scale theoretical sticks to match measured peak height
    if has_measured and scale_sticks_to_measured and max_stick_174 > 0:
        if xlim is not None:
            mask_m = (m_x >= xlim[0]) & (m_x <= xlim[1]) & (~np.isnan(m_y))
            max_meas = (
                np.nanmax(m_y[mask_m]) if np.any(mask_m) else np.nanmax(m_y)
            )
        else:
            max_meas = np.nanmax(m_y) if len(m_y) > 0 else 0

        if max_meas > 0 and not np.isnan(max_meas):
            scale_factor = max_meas / max_stick_174
            for iso, branches in all_iso_branches.items():
                for data in branches.values():
                    data["y"] = data["y"] * scale_factor

    # 4. Determine y-limit based on visible range (xlim)
    if xlim is not None:
        visible_y = []
        for branches in all_iso_branches.values():
            for data in branches.values():
                mask = (data["x_offset"] >= xlim[0]) & (
                    data["x_offset"] <= xlim[1]
                )
                visible_y.extend(data["y"][mask])

        if has_measured:
            mask_m = (m_x >= xlim[0]) & (m_x <= xlim[1]) & (~np.isnan(m_y))
            if np.any(mask_m):
                visible_y.extend(m_y[mask_m])

        max_pop = np.nanmax(visible_y) if visible_y else 1.0
    else:
        all_stick_y = [
            y
            for branches in all_iso_branches.values()
            for data in branches.values()
            for y in data["y"]
        ]
        max_stick_y = max(all_stick_y) if all_stick_y else 1.0
        max_pop = (
            max(max_stick_y, np.nanmax(m_y)) if has_measured else max_stick_y
        )

    # Floor threshold for small sticks
    min_height_floor = (
        max_pop * min_stick_fraction if min_stick_fraction is not None else 0.0
    )

    # 5. Plot setup
    fig, ax = plt.subplots(figsize=figsize)
    cmap = plt.get_cmap("tab10")

    # Trace: Measured spectrum
    if has_measured:
        ax.plot(
            m_x,
            m_y,
            color="black",
            alpha=0.35,
            linewidth=1.2,
            label="Measured",
            zorder=1,
        )

    # Trace: Multi-isotope stick loop
    branch_colors = {}
    for iso, branches in all_iso_branches.items():
        # Get line style properties for the current isotope
        iso_props = style_map.get(iso, {"linestyle": "-", "label": f"Iso {iso}"})
        ls = iso_props["linestyle"]

        for i, (branch_name, data) in enumerate(branches.items()):
            color = cmap(i % 10)
            branch_colors[branch_name] = color
            x_vals = data["x_offset"]

            # Floor clamp
            y_vals = (
                np.maximum(data["y"], min_height_floor)
                if min_height_floor > 0
                else data["y"]
            )

            # Vertical sticks with isotope linestyle
            ax.vlines(
                x=x_vals,
                ymin=0,
                ymax=y_vals,
                color=color,
                linestyle=ls,
                linewidth=1.5,
                alpha=0.85,
                zorder=2,
            )
            
            # Retrieve hyperfine label order for the current branch
            hf_patterns = x_branches_hyperfine.get(branch_name, [])
            
            # Quantum number annotations (new code: enumerate(zip(...)))
            for idx, (x_val, y_val, q_val) in enumerate(zip(x_vals, y_vals, data["q_num"])):
                if xlim is not None and not (xlim[0] <= x_val <= xlim[1]):
                    continue

                n_label = (
                    str(int(q_val))
                    if float(q_val).is_integer()
                    else f"{q_val:.1f}"
                )
                
                # Append hyperfine string if pattern exists
                if hf_patterns:
                    hf_str = hf_patterns[idx % len(hf_patterns)]
                    q_label = f"{n_label}, {hf_str}"
                else:
                    q_label = n_label

                ax.text(
                    x=x_val,
                    y=y_val + (max_pop * 0.015),
                    s=q_label,
                    rotation=90,
                    ha="center",
                    va="bottom",
                    fontsize=7,
                    color=color,
                    zorder=3,
                )

    # 6. Apply xlim if provided
    if xlim is not None:
        ax.set_xlim(xlim)

    # 7. Dynamic Title Offset Formatting
    if isinstance(stick_offset_ghz, dict):
        offset_str = ", ".join([f"{iso}: {off:g}" for iso, off in stick_offset_ghz.items()])
        title_offset = f", with global offsets {{{offset_str}}} GHz"
    else:
        title_offset = r", with offset %g GHz to predictions" % stick_offset_ghz

    # 8. Legend & Styling
    ax.set_xlabel(
        f"Frequency Offset (GHz) from {center_freq_thz} THz", fontsize=11
    )
    ax.set_ylabel(ytitle, fontsize=11)
    ax.set_title(
        r"YbF $A\,^2\Pi_{3/2} \leftarrow X\,^2\Sigma^+$ Spectrum with Predictions and Assigned Peaks"
        + title_offset,
        fontsize=12,
    )
    ax.set_ylim(bottom=0, top=max_pop * 1.30)
    ax.grid(True, linestyle=":", alpha=0.5)

    # Build custom legend entries for Isotope line styles
    legend_elements = []
    if has_measured:
        legend_elements.append(
            mlines.Line2D(
                [],
                [],
                color="black",
                alpha=0.5,
                linewidth=1.2,
                label="Measured",
            )
        )

    for iso in dfs_by_isotope.keys():
        props = style_map.get(iso, {"linestyle": "-", "label": f"Iso {iso}"})
        legend_elements.append(
            mlines.Line2D(
                [],
                [],
                color="gray",
                linestyle=props["linestyle"],
                linewidth=1.5,
                label=props["label"],
            )
        )

    # Add branch colors to legend
    for b_name, b_color in branch_colors.items():
        legend_elements.append(
            mlines.Line2D(
                [], [], color=b_color, linewidth=1.5, label=f"Branch {b_name}"
            )
        )

    ax.legend(
        handles=legend_elements,
        title="Key",
        bbox_to_anchor=(1.02, 1),
        loc="upper left",
        frameon=True,
    )

    plt.tight_layout()
    return fig, ax

#%% Automatic line assignment
def assign_spectral_lines(
    dfs_by_isotope,
    measured_x,
    measured_y,
    center_freq_thz=568.681,
    stick_offset_ghz=-3.15,
    tolerance_ghz=0.5,
    measured_in_thz=True,
    peak_prominence=0.01,
    use_wavenumber=False,
):
    """Matches experimental peaks against predicted transitions from dfs_by_isotope.

    Handles O, P, Q, R branches and 4 hyperfine components (f1, f2, f3, f4).
    Returns a pandas DataFrame of assigned peaks.
    """
    if isinstance(stick_offset_ghz, dict):
        offset_map = stick_offset_ghz
    elif isinstance(stick_offset_ghz, (int, float)):
        offset_map = {
            iso: float(stick_offset_ghz) for iso in dfs_by_isotope.keys()
        }
    else:
        offset_map = {iso: 0.0 for iso in dfs_by_isotope.keys()}

    m_x = np.asarray(measured_x, dtype=float)
    m_y = np.asarray(measured_y, dtype=float)

    m_x_ghz = (m_x - center_freq_thz) * 1000.0 if measured_in_thz else m_x

    peak_indices, _ = find_peaks(m_y, prominence=peak_prominence)
    exp_peak_x = m_x_ghz[peak_indices]
    exp_peak_y = m_y[peak_indices]

    assignments = []

    def _extract_freq_list(cell_val):
        if cell_val is None:
            return []
        if isinstance(cell_val, (list, tuple, np.ndarray)):
            return [
                float(v)
                for v in cell_val
                if v is not None and not np.isnan(float(v))
            ]
        if pd.isna(cell_val):
            return []
        if isinstance(cell_val, str):
            clean_str = cell_val.strip("[]() ")
            tokens = clean_str.replace(",", " ").split()
            out = []
            for t in tokens:
                try:
                    out.append(float(t))
                except ValueError:
                    pass
            return out
        try:
            return [float(cell_val)]
        except (ValueError, TypeError):
            return []

    for iso, df in dfs_by_isotope.items():
        iso_offset = offset_map.get(iso, 0.0)
        iso_str = f"{iso}YbF" if not str(iso).endswith("YbF") else str(iso)

        branch_col = next(
            (c for c in ["Branch", "branch"] if c in df.columns), None
        )
        q_col = next(
            (
                c
                for c in ["N''", "N_double_prime", 'N"', "N"]
                if c in df.columns
            ),
            None,
        )
        freq_col = next(
            (
                c
                for c in [
                    "Frequency_THz",
                    "Wavenumber_cm-1",
                    "Frequency",
                    "Wavenumber",
                ]
                if c in df.columns
            ),
            None,
        )

        if not (branch_col and q_col and freq_col):
            continue

        for _, row in df.iterrows():
            branch_name = str(row[branch_col])
            q_val = row[q_col]
            freq_list = _extract_freq_list(row[freq_col])

            for hf_idx, raw_freq in enumerate(freq_list):
                if hf_idx >= 4:
                    break

                multiplier = 29.9792458 if use_wavenumber else 1000.0
                pred_freq = (
                    raw_freq - center_freq_thz
                ) * multiplier + iso_offset

                if len(exp_peak_x) == 0:
                    continue

                distances = np.abs(exp_peak_x - pred_freq)
                nearest_idx = np.argmin(distances)
                min_dist = distances[nearest_idx]

                if min_dist <= tolerance_ghz:
                    exp_freq = exp_peak_x[nearest_idx]
                    exp_intensity = exp_peak_y[nearest_idx]
                    residual = exp_freq - pred_freq

                    hf_comp = hf_idx + 1
                    q_num_float = float(q_val)
                    q_str = (
                        str(int(q_num_float))
                        if q_num_float.is_integer()
                        else f"{q_num_float:.1f}"
                    )
                    peak_label = (
                        f"{iso_str} {branch_name}({q_str})_f{hf_comp}"
                    )

                    assignments.append(
                        {
                            "Isotope": iso_str,
                            "Branch": branch_name,
                            "N_double_prime": q_val,
                            "Hyperfine_Comp": hf_comp,
                            "Peak_Label": peak_label,
                            "Pred_Freq_GHz": pred_freq,
                            "Exp_Freq_GHz": exp_freq,
                            "Exp_Intensity": exp_intensity,
                            "Residual_GHz": residual,
                            "Abs_Error_GHz": min_dist,
                        }
                    )

    df_assignments = pd.DataFrame(assignments)

    if not df_assignments.empty:
        # Keep closest unique match per theoretical component
        df_assignments = df_assignments.sort_values("Abs_Error_GHz").drop_duplicates(
            subset=["Isotope", "Branch", "N", "Hyperfine_Comp"],
            keep="first",
        )
        df_assignments = df_assignments.sort_values(
            ["Isotope", "Branch", "N", "Hyperfine_Comp"]
        ).reset_index(drop=True)

        rms_res = np.sqrt(np.mean(df_assignments["Residual_GHz"] ** 2))
        print(
            f"Successfully assigned {len(df_assignments)} lines. RMS Residual: {rms_res:.4f} GHz"
        )
    else:
        print("No matches found within the given tolerance.")

    return df_assignments




def plot_residuals_vs_n(
    df_assignments,
    tolerance_ghz=0.5,
    figsize=None,
    branch_colors=None,
    iso_markers=None,
    hf_labels=None,
):
    """Plots frequency residuals (Exp - Pred) vs N'' with full customization.

    Parameters:
    -----------
    df_assignments : pd.DataFrame
        Assigned spectral lines containing 'N_double_prime', 'Residual_GHz',
        'Branch', 'Isotope', and 'Hyperfine_Comp'.
    tolerance_ghz : float
        Tolerance boundaries to display as red dotted lines.
    figsize : tuple, optional
        Custom figure size tuple (width, height).
    branch_colors : dict, optional
        Custom color mapping for branches.
        Example: {'P': '#ff7f0e', 'Q': '#2ca02c', 'R': '#d62728'}
    iso_markers : dict, optional
        Custom marker mapping for isotopes.
        Example: {'172YbF': 'o', '174YbF': 's', '176YbF': '^'}
    hf_labels : dict, optional
        Custom titles mapping component index to physical quantum states.
        Example: {1: 'F=N+1', 2: 'F=N-', 3: 'F=N+', 4: 'F=N-1'}
    """
    if df_assignments.empty:
        print("Assignment DataFrame is empty. Nothing to plot.")
        return None, None

    hf_comps = sorted(df_assignments["Hyperfine_Comp"].unique())
    n_panels = len(hf_comps)

    cols = 2 if n_panels > 1 else 1
    rows = math.ceil(n_panels / cols)

    if figsize is None:
        figsize = (6 * cols, 4.2 * rows)

    fig, axes = plt.subplots(
        rows, cols, figsize=figsize, sharex=True, sharey=True
    )
    axes_flat = (
        np.array([axes]).flatten()
        if n_panels == 1
        else (axes.flatten() if hasattr(axes, "flatten") else [axes])
    )

    # 1. Fallback or user-defined branch colors
    if branch_colors is None:
        unique_branches = sorted(
            df_assignments["Branch"].astype(str).unique()
        )
        palette = sns.color_palette("Set1", len(unique_branches))
        branch_colors = dict(zip(unique_branches, palette))

    # 2. Fallback or user-defined isotope markers
    if iso_markers is None:
        unique_isos = sorted(df_assignments["Isotope"].astype(str).unique())
        markers = ["o", "s", "^", "D", "v", "<", ">", "p"]
        iso_markers = {
            iso: markers[i % len(markers)]
            for i, iso in enumerate(unique_isos)
        }

    # 3. Fallback or user-defined subplot titles/labels
    if hf_labels is None:
        hf_labels = {comp: f"Component Index: {comp}" for comp in hf_comps}
    
    # Track handles and labels across all subplots to form a single master legend
    all_handles, all_labels = [], []
    
    for idx, hf_comp in enumerate(hf_comps):
        ax = axes_flat[idx]
        df_sub = df_assignments[
            df_assignments["Hyperfine_Comp"] == hf_comp
        ].copy()

        ax.axhline(0, color="black", linestyle="--", linewidth=1, alpha=0.7)
        ax.axhline(
            tolerance_ghz,
            color="red",
            linestyle=":",
            linewidth=1,
            alpha=0.5,
        )
        ax.axhline(
            -tolerance_ghz,
            color="red",
            linestyle=":",
            linewidth=1,
            alpha=0.5,
        )

        if not df_sub.empty:
            sns.scatterplot(
                data=df_sub,
                x="N",
                y="Residual_GHz",
                hue="Branch",
                style="Isotope",
                palette=branch_colors,
                markers=iso_markers,
                s=70,
                alpha=0.85,
                ax=ax,
                legend="brief",
            )

            for collection in ax.collections:
                collection.set_edgecolor("black")
                collection.set_linewidth(0.5)

            # Extract local handles and labels
            h, l = ax.get_legend_handles_labels()
            for handle, label in zip(h, l):
                if label not in all_labels:
                    all_handles.append(handle)
                    all_labels.append(label)

            # Remove local legend box so panels remain clean
            if ax.get_legend():
                ax.get_legend().remove()

        title_text = hf_labels.get(hf_comp, f"Component Index: {hf_comp}")
        ax.set_title(title_text, fontweight="bold")
        ax.set_xlabel("")
        ax.set_ylabel("")
        ax.grid(True, linestyle=":", alpha=0.5)

    for j in range(idx + 1, len(axes_flat)):
        fig.delaxes(axes_flat[j])

    fig.supxlabel(r"$N''$ (Lower State Quantum Number)", fontsize=15)
    fig.supylabel(r"Residual $\Delta\nu$ (GHz) [Exp - Pred]", fontsize=15)

    if n_panels > 0 and axes_flat[0].get_legend():
        axes_flat[0].legend(
            bbox_to_anchor=(1.05, 1), loc="upper left", frameon=True
        )
        
    if all_handles:
        fig.legend(
            all_handles,
            all_labels,
            bbox_to_anchor=(0.53, 0.95),
            loc="upper left",
            frameon=True,
            fontsize=12,
            title_fontsize=13,
        )

    plt.tight_layout()
    return fig, axes



def plot_residuals_by_isotope(
    df_assignments,
    tolerance_ghz=0.01,  # Default to 10 MHz tolerance band
    figsize=None,
    branch_colors=None,
    hf_labels=None,
):
    """Generates separate 4-panel residual plots for each isotope in df_assignments."""
    if df_assignments.empty:
        print("Assignment DataFrame is empty. Nothing to plot.")
        return {}

    # Define standard branch order & colors
    branch_order = ["O", "P", "Q", "R"]
    default_colors = {
        "O": "#1f77b4",
        "P": "#ff7f0e",
        "Q": "#2ca02c",
        "R": "#d62728",
    }
    if branch_colors is None:
        branch_colors = default_colors

    unique_isos = sorted(df_assignments["Isotope"].astype(str).unique())
    figures_dict = {}

    for iso in unique_isos:
        df_iso = df_assignments[df_assignments["Isotope"] == iso].copy()

        hf_comps = sorted(df_iso["Hyperfine_Comp"].unique())
        n_panels = len(hf_comps)
        cols = 2 if n_panels > 1 else 1
        rows = math.ceil(n_panels / cols)

        if figsize is None:
            current_figsize = (6 * cols, 4.2 * rows)
        else:
            current_figsize = figsize

        fig, axes = plt.subplots(
            rows,
            cols,
            figsize=current_figsize,
            sharex=True,
            sharey=True,
        )
        axes_flat = (
            np.array([axes]).flatten()
            if n_panels == 1
            else (axes.flatten() if hasattr(axes, "flatten") else [axes])
        )

        if hf_labels is None:
            local_hf_labels = {
                comp: f"Component Index: {comp}" for comp in hf_comps
            }
        else:
            local_hf_labels = hf_labels

        all_handles, all_labels = [], []

        for idx, hf_comp in enumerate(hf_comps):
            ax = axes_flat[idx]
            df_sub = df_iso[df_iso["Hyperfine_Comp"] == hf_comp].copy()

            ax.axhline(0, color="black", linestyle="--", linewidth=1, alpha=0.7)
            ax.axhline(
                tolerance_ghz,
                color="red",
                linestyle=":",
                linewidth=1,
                alpha=0.5,
            )
            ax.axhline(
                -tolerance_ghz,
                color="red",
                linestyle=":",
                linewidth=1,
                alpha=0.5,
            )

            if not df_sub.empty:
                sns.scatterplot(
                    data=df_sub,
                    x="N",
                    y="Residual_GHz",
                    hue="Branch",
                    hue_order=[
                        b for b in branch_order if b in df_sub["Branch"].values
                    ],
                    palette=branch_colors,
                    marker="o",
                    s=70,
                    alpha=0.85,
                    ax=ax,
                    legend="brief",
                )

                for collection in ax.collections:
                    collection.set_edgecolor("black")
                    collection.set_linewidth(0.5)

                h, l = ax.get_legend_handles_labels()
                for handle, label in zip(h, l):
                    if label not in all_labels:
                        all_handles.append(handle)
                        all_labels.append(label)

                if ax.get_legend():
                    ax.get_legend().remove()

            title_text = local_hf_labels.get(
                hf_comp, f"Component Index: {hf_comp}"
            )
            ax.set_title(title_text, fontweight="bold", fontsize=14)
            ax.set_xlabel("")
            ax.set_ylabel("")
            ax.grid(True, linestyle=":", alpha=0.5)

        # Clear remaining unused subplot axes if any
        for j in range(idx + 1, len(axes_flat)):
            fig.delaxes(axes_flat[j])

        # Axis Labels and Overall Title
        fig.supxlabel(r"$N''$ (Lower State Quantum Number)", fontsize=15)
        fig.supylabel(r"Residual $\Delta\nu$ (GHz) [Exp - Pred]", fontsize=15)
        fig.suptitle(f"Frequency Residuals: {iso}", fontsize=16, y=0.98)

        # Reorder legend entries to strictly follow O, P, Q, R order
        sorted_legend = sorted(
            zip(all_handles, all_labels),
            key=lambda x: (
                branch_order.index(x[1]) if x[1] in branch_order else 99
            ),
        )
        if sorted_legend:
            sorted_handles, sorted_labels = zip(*sorted_legend)
            fig.legend(
                sorted_handles,
                sorted_labels,
                bbox_to_anchor=(0.52, 0.93),
                loc="upper left",
                frameon=True,
                fontsize=12,
                title="Branch",
                title_fontsize=13,
            )

        plt.tight_layout(rect=[0, 0, 1, 0.95])
        figures_dict[iso] = (fig, axes)

    return figures_dict


def plot_spectrum_with_assignments(
    df_assignments,
    dfs_by_isotope=None,
    measured_x=None,
    measured_y=None,
    center_freq_thz=0.0,
    measured_in_thz=True,
    use_wavenumber=False,
    xlim=None,
    figsize=(14, 6),
    label_fontsize=7,
    show_prediction_sticks=True,
    scale_sticks_to_measured=True,
    min_stick_fraction=0.2,
    stick_offset_ghz=None,
    branch_colors=None,
    iso_markers=None,
    iso_linestyles=None,
    hf_labels=None,
    title=None,
    ax=None,
):
    """Plots experimental spectrum with peak assignments and prediction sticks.

    Includes hyperfine labels on assignments/predictions and isotope markers +
    linestyles in the legend.
    """
    
    import matplotlib.lines as mlines
    
    if stick_offset_ghz is None:
        stick_offset_ghz = {}

    if ax is None:
        fig, ax = plt.subplots(figsize=figsize)
    else:
        fig = ax.get_figure()

    # Convert experimental spectrum to relative frequency offset
    m_x_ghz = None
    max_meas = 1.0
    if measured_x is not None and measured_y is not None:
        m_x = np.asarray(measured_x, dtype=float)
        m_y = np.asarray(measured_y, dtype=float)
        max_meas = np.nanmax(m_y) if len(m_y) > 0 else 1.0

        if use_wavenumber:
            m_x_ghz = (m_x - center_freq_thz) * 29.9792458
        elif measured_in_thz:
            m_x_ghz = (m_x - center_freq_thz) * 1000.0
        else:
            m_x_ghz = m_x - center_freq_thz

    # Default styling definitions
    if branch_colors is None:
        branch_colors = {
            "O": "#1f77b4",
            "P": "#ff7f0e",
            "Q": "#2ca02c",
            "R": "#d62728",
            "Branch O": "#1f77b4",
            "Branch P": "#ff7f0e",
            "Branch Q": "#2ca02c",
            "Branch R": "#d62728",
        }

    if iso_markers is None:
        iso_markers = {
            "172YbF": "o",
            "174YbF": "s",
            "176YbF": "^",
            172: "o",
            174: "s",
            176: "^",
        }

    if iso_linestyles is None:
        iso_linestyles = {
            "174YbF": "-",
            "172YbF": "--",
            "176YbF": ":",
            174: "-",
            172: "--",
            176: ":",
        }

    if hf_labels is None:
        hf_labels = {1: "N+1", 2: "N-", 3: "N+", 4: "N-1"}

    # 1. Plot experimental spectrum trace
    if m_x_ghz is not None:
        ax.plot(
            m_x_ghz,
            m_y,
            color="gray",
            alpha=0.5,
            linewidth=0.8,
            label="Measured",
            zorder=1,
        )

    # 2. Draw predicted line sticks and hyperfine text annotations
    if show_prediction_sticks and dfs_by_isotope is not None:
        for iso_key, df_iso in dfs_by_isotope.items():
            iso_num = (
                int("".join(filter(str.isdigit, str(iso_key))))
                if any(char.isdigit() for char in str(iso_key))
                else iso_key
            )
            offset = stick_offset_ghz.get(
                iso_num, stick_offset_ghz.get(str(iso_key), 0.0)
            )
            ls = iso_linestyles.get(
                str(iso_key),
                iso_linestyles.get(
                    iso_num, iso_linestyles.get(f"{iso_num}YbF", ":")
                ),
            )

            for _, row in df_iso.iterrows():
                freq_val = row.get("Frequency_THz", row.get("Frequency", None))
                if freq_val is None:
                    continue

                freqs = (
                    freq_val
                    if isinstance(freq_val, (list, np.ndarray))
                    else [freq_val]
                )

                branch = str(row.get("Branch", ""))
                color = branch_colors.get(
                    branch, branch_colors.get(f"Branch {branch}", "gray")
                )
                q_val = row.get("N''", row.get("N_double_prime", ""))
                q_str = (
                    str(int(q_val))
                    if isinstance(q_val, (int, float))
                    and float(q_val).is_integer()
                    else str(q_val)
                )

                for hf_idx, pred_freq in enumerate(freqs, start=1):
                    if use_wavenumber:
                        stick_x = (
                            pred_freq - center_freq_thz
                        ) * 29.9792458 + offset
                    else:
                        stick_x = (
                            pred_freq - center_freq_thz
                        ) * 1000.0 + offset

                    if xlim is not None and not (
                        xlim[0] <= stick_x <= xlim[1]
                    ):
                        continue

                    pop = row.get("Population", 1.0)
                    stick_height = (
                        pop * max_meas if scale_sticks_to_measured else pop
                    )
                    stick_height = max(
                        stick_height, max_meas * min_stick_fraction
                    )

                    ax.vlines(
                        x=stick_x,
                        ymin=0,
                        ymax=stick_height,
                        colors=color,
                        linestyles=ls,
                        alpha=0.6,
                        linewidth=1.0,
                        zorder=2,
                    )

                    hf_name = hf_labels.get(hf_idx, f"h_{hf_idx}")
                    stick_label = f"{q_str}, {hf_name}"

                    ax.text(
                        x=stick_x,
                        y=stick_height + (max_meas * 0.015),
                        s=stick_label,
                        ha="center",
                        va="bottom",
                        fontsize=label_fontsize - 1,
                        color=color,
                        rotation=90,
                        alpha=0.85,
                        zorder=3,
                    )

    # 3. Plot assigned experimental peaks with Hyperfine info included in the label
    if df_assignments is not None and not df_assignments.empty:
        for _, row in df_assignments.iterrows():
            x_pos = row["Exp_Freq_GHz"]
            y_pos = row["Exp_Intensity"]
            branch = str(row["Branch"])
            iso_str = str(row["Isotope"])
            q_val = row["N_double_prime"]
            hf_comp = row.get("Hyperfine_Comp", None)

            if xlim is not None and not (xlim[0] <= x_pos <= xlim[1]):
                continue

            color = branch_colors.get(
                branch, branch_colors.get(f"Branch {branch}", "tab:blue")
            )
            marker = iso_markers.get(
                iso_str, iso_markers.get(str(iso_str), "o")
            )

            ax.scatter(
                x_pos,
                y_pos,
                color=color,
                marker=marker,
                s=35,
                edgecolor="black",
                linewidth=0.5,
                zorder=4,
            )

            q_str = (
                str(int(q_val))
                if float(q_val).is_integer()
                else f"{q_val:.1f}"
            )
            clean_b = branch.replace("Branch ", "")

            # Append Hyperfine State to assigned peak string
            hf_suffix = ""
            if hf_comp is not None:
                hf_state_str = hf_labels.get(hf_comp, f"h_{hf_comp}")
                hf_suffix = f" {hf_state_str}"

            peak_label = f"{iso_str} {clean_b}({q_str}){hf_suffix}"

            ax.text(
                x=x_pos,
                y=y_pos + (max_meas * 0.03),
                s=peak_label,
                ha="center",
                va="bottom",
                fontsize=label_fontsize,
                color=color,
                fontweight="bold",
                rotation=90,
                zorder=5,
            )

    if xlim is not None:
        ax.set_xlim(xlim)

    ax.set_ylim(bottom=0, top=max_meas * 1.35)
    ax.grid(True, linestyle=":", alpha=0.3)

    if title:
        ax.set_title(title, fontsize=10, style="italic")
    elif stick_offset_ghz:
        offset_str = ", ".join(f"{k}: {v}" for k, v in stick_offset_ghz.items())
        ax.set_title(
            rf"$\mathrm{{YbF}}\ A^2\Pi_{{3/2}} \leftarrow X^2\Sigma^+$ Spectrum with Predictions and Assigned Peaks, with global offsets {{{offset_str}}} GHz",
            fontsize=10,
        )

    unit_str = "cm$^{-1}$" if use_wavenumber else "GHz"
    ax.set_xlabel(
        f"Frequency Offset ({unit_str}) from {center_freq_thz} THz",
        fontsize=9,
        style="italic",
    )
    ax.set_ylabel("PMT signal normalised to X-A (a.u.)", fontsize=9)

    # Extended legend combining trace, isotope linestyles, markers, and branch colors
    legend_elements = [
        mlines.Line2D(
            [], [], color="gray", alpha=0.5, linewidth=0.8, label="Measured"
        ),
        mlines.Line2D(
            [],
            [],
            color="gray",
            marker=iso_markers.get("174YbF", "s"),
            linestyle=iso_linestyles.get("174YbF", "-"),
            linewidth=1,
            markersize=5,
            label=r"$^{174}\mathrm{YbF}$",
        ),
        mlines.Line2D(
            [],
            [],
            color="gray",
            marker=iso_markers.get("172YbF", "o"),
            linestyle=iso_linestyles.get("172YbF", "--"),
            linewidth=1,
            markersize=5,
            label=r"$^{172}\mathrm{YbF}$",
        ),
        mlines.Line2D(
            [],
            [],
            color="gray",
            marker=iso_markers.get("176YbF", "^"),
            linestyle=iso_linestyles.get("176YbF", ":"),
            linewidth=1,
            markersize=5,
            label=r"$^{176}\mathrm{YbF}$",
        ),
    ]

    for b_name in ["O", "P", "Q", "R"]:
        if b_name in branch_colors or f"Branch {b_name}" in branch_colors:
            c = branch_colors.get(b_name, branch_colors.get(f"Branch {b_name}"))
            legend_elements.append(
                mlines.Line2D(
                    [],
                    [],
                    color=c,
                    linewidth=1.2,
                    label=rf"$\mathit{{Branch\ {b_name}}}$",
                )
            )

    ax.legend(
        handles=legend_elements,
        title=r"$\mathit{Key}$",
        bbox_to_anchor=(1.02, 1),
        loc="upper left",
        frameon=True,
    )

    plt.tight_layout()
    return fig, ax

#%% Interactive tools for saving
def save_fig_to_pdf_slices(
    fig,
    ax,
    total_range_ghz=(-5.0, 50.0),
    window_size_ghz=10.0,
    overlap_ghz=2.0,
    a4_size_inches=(11.69, 8.27)  # Standard A4 Landscape
):
    """
    Slices an existing Matplotlib figure into sequential PDF pages,
    enforcing axis clipping to clean up margin bleeding and preserving external legends.
    """
    
    import tkinter as tk
    from tkinter import filedialog
    
    # 1. Pop-up file dialog
    root = tk.Tk()
    root.withdraw()
    root.attributes("-topmost", True)

    filepath = filedialog.asksaveasfilename(
        title="Save Multi-Page PDF Spectrum",
        defaultextension=".pdf",
        filetypes=[("PDF files", "*.pdf"), ("All files", "*.*")]
    )
    root.destroy()

    if not filepath:
        print("Export cancelled.")
        return

    # 2. Force clipping on all plot elements (texts, lines, collections)
    # Prevents text/sticks outside current xlim from bleeding into the paper margins
    for txt in ax.texts:
        txt.set_clip_on(True)
    for line in ax.lines:
        line.set_clip_on(True)
    for coll in ax.collections:
        coll.set_clip_on(True)

    # 3. Store original figure state
    orig_xlim = ax.get_xlim()
    orig_title = ax.get_title()
    orig_size = fig.get_size_inches()

    # 4. Calculate frequency window boundaries
    start_f, end_f = total_range_ghz
    step = window_size_ghz - overlap_ghz
    windows = []
    curr = start_f

    while curr < end_f:
        w_end = min(curr + window_size_ghz, end_f)
        windows.append((curr, w_end))
        curr += step
        if curr >= end_f or w_end == end_f:
            break

    try:
        # Resize figure to A4 landscape proportions during export
        fig.set_size_inches(a4_size_inches[0], a4_size_inches[1], forward=True)

        with PdfPages(filepath) as pdf:
            for idx, (xlim_low, xlim_high) in enumerate(windows, 1):
                ax.set_xlim(xlim_low, xlim_high)
                
                # Format page title with cleaner 2-line layout
                ax.set_title(
                    f"{orig_title}\nPage {idx} of {len(windows)} ({xlim_low:.1f} to {xlim_high:.1f} GHz)",
                    fontsize=10,
                    pad=10
                )
                
                # bbox_inches='tight' dynamically expands canvas boundaries to keep legend in frame
                pdf.savefig(fig, bbox_inches='tight')

        print(f"Successfully exported {len(windows)} pages to: {filepath}")

    finally:
        # Restore original figure size, x-limits, and title
        fig.set_size_inches(orig_size[0], orig_size[1], forward=True)
        ax.set_xlim(orig_xlim)
        ax.set_title(orig_title)
        fig.tight_layout()
        
def save_dataframe_interactive(
    df, default_name="fitted_spectral_lines.csv"
):
    """Pops up a native file explorer window to select a save location and exports the DataFrame to CSV."""
    # Create and hide the root Tkinter window so an extra blank GUI window doesn't linger
    root = tk.Tk()
    root.withdraw()
    root.attributes("-topmost", True)  # Bring window to front

    # Prompt user with native 'Save As' dialog
    file_path = filedialog.asksaveasfilename(
        title="Save Fitted DataFrame to CSV",
        initialfile=default_name,
        defaultextension=".csv",
        filetypes=[("CSV Files", "*.csv"), ("All Files", "*.*")],
    )

    root.destroy()  # Clean up GUI memory

    # Export if a path was chosen
    if file_path:
        df.to_csv(file_path, index=False)
        print(f"Saved {len(df)} rows to: {file_path}")
        return file_path
    else:
        print("Export cancelled.")
        return None
#%% Peak detection
def detect_spectral_peaks(
    frequencies, 
    intensities, 
    height=0.1, 
    prominence=0.05, 
    distance=10, 
    smooth=True,
    window_length=15,
    polyorder=3
):
    """
    Detects recognizable peaks in a 1D spectrum.

    Parameters
    ----------
    frequencies : array-like
        1D array of frequency offsets (e.g., GHz).
    intensities : array-like
        1D array of signal intensity values (e.g., PMT signal).
    height : float or tuple, optional
        Minimum signal intensity required to qualify as a peak.
    prominence : float, optional
        Required height of peak relative to surrounding baseline/noise.
    distance : int, optional
        Minimum index separation between neighboring peaks.
    smooth : bool, optional
        If True, applies a Savitzky-Golay filter to reduce high-frequency noise.
    window_length : int, optional
        Window length for Savitzky-Golay filter (must be an odd integer).
    polyorder : int, optional
        Polynomial order for Savitzky-Golay filter.

    Returns
    -------
    peak_indices : np.ndarray
        Array of integer indices where peaks occur in the original dataset.
    peak_positions : np.ndarray
        Array of frequency coordinates corresponding to each peak index.
    """
    frequencies = np.asarray(frequencies)
    intensities = np.asarray(intensities)

    # Optional smoothing pass to stabilize peak detection against raw PMT noise
    y_signal = intensities
    if smooth and len(intensities) >= window_length:
        y_signal = savgol_filter(intensities, window_length=window_length, polyorder=polyorder)

    # Peak detection using prominence and intensity thresholding
    peak_indices, _ = find_peaks(
        y_signal, 
        height=height, 
        prominence=prominence, 
        distance=distance
    )

    peak_positions = frequencies[peak_indices]

    return peak_indices, peak_positions

#%% Assignment tools and plotter for the new branch convension
def plot_residuals_vs_n_new(
    df_assignments, tolerance_ghz=0.5, figsize=(12, 10)
):
    """Plots frequency residuals (Exp - Pred) vs N'' across 4 subplots (one for each hyperfine state)."""
    fig, axes = plt.subplots(2, 2, figsize=figsize, sharex=True, sharey=True)
    axes = axes.flatten()

    hf_labels = [
        r"$F = N+1$",
        r"$F = N-$",
        r"$F = N+$",
        r"$F = N-1$",
    ]
    branch_colors = {
        "O": "#1f77b4",
        "P": "#ff7f0e",
        "Q": "#2ca02c",
        "R": "#d62728",
    }
    iso_markers = {"172YbF": "o", "174YbF": "s", "176YbF": "^"}

    for hf_comp in range(1, 5):
        ax = axes[hf_comp - 1]
        df_sub = df_assignments[
            df_assignments["Hyperfine_Comp"] == hf_comp
        ].copy()

        ax.axhline(0, color="black", linestyle="--", linewidth=1, alpha=0.7)
        ax.axhline(
            tolerance_ghz,
            color="red",
            linestyle=":",
            linewidth=1,
            alpha=0.5,
        )
        ax.axhline(
            -tolerance_ghz,
            color="red",
            linestyle=":",
            linewidth=1,
            alpha=0.5,
        )

        if not df_sub.empty:
            sns.scatterplot(
                data=df_sub,
                x="N_double_prime",
                y="Residual_GHz",
                hue="Branch",
                style="Isotope",
                palette=branch_colors,
                markers=iso_markers,
                s=70,
                alpha=0.85,
                ax=ax,
                legend=(hf_comp == 2),
            )

        ax.set_title(
            f"Hyperfine State: {hf_labels[hf_comp-1]}", fontweight="bold"
        )
        ax.grid(True, linestyle=":", alpha=0.5)

    fig.supxlabel(r"$N''$ (Lower State Quantum Number)", fontsize=12)
    fig.supylabel(
        r"Residual $\Delta\nu$ (GHz) [Exp - Pred]", fontsize=12
    )

    # Clean up legend placement
    axes[1].legend(bbox_to_anchor=(1.05, 1), loc="upper left", title="Key")
    plt.tight_layout()
    return fig, axes




def plot_spectrum_with_assignments_new(
    df_assignments,
    dfs_by_isotope,
    measured_x,
    measured_y,
    center_freq_thz=568.681,
    measured_in_thz=True,
    xlim=None,
    figsize=(16, 10),
    label_fontsize=7,
    scale_sticks_to_measured=True,
    min_stick_fraction=0.2,
    stick_offset_ghz={174: -3.15, 172: -2.0, 176: -2.0},
    use_wavenumber=False,
):
    """Plots spectrum and theoretical assignments split into a 2x2 grid for f1, f2, f3, f4."""
    
    import matplotlib.lines as mlines
    
    m_x = np.asarray(measured_x, dtype=float)
    m_y = np.asarray(measured_y, dtype=float)
    m_x_ghz = (m_x - center_freq_thz) * 1000.0 if measured_in_thz else m_x

    fig, axes = plt.subplots(2, 2, figsize=figsize, sharex=True, sharey=True)
    axes = axes.flatten()

    hf_labels = [
        r"$F = N+1 \ (f_1)$",
        r"$F = N \ (f_2)$",
        r"$F = N \ (f_3)$",
        r"$F = N-1 \ (f_4)$",
    ]
    branch_colors = {
        "O": "#1f77b4",
        "P": "#ff7f0e",
        "Q": "#2ca02c",
        "R": "#d62728",
    }
    style_map = {
        174: {"linestyle": "-", "marker": "s", "label": r"$^{174}$YbF"},
        172: {"linestyle": "--", "marker": "o", "label": r"$^{172}$YbF"},
        176: {"linestyle": ":", "marker": "^", "label": r"$^{176}$YbF"},
    }

    max_meas = np.nanmax(m_y) if len(m_y) > 0 else 1.0

    for hf_idx in range(4):
        ax = axes[hf_idx]
        hf_comp = hf_idx + 1

        # Base experimental spectrum
        ax.plot(
            m_x_ghz,
            m_y,
            color="black",
            alpha=0.25,
            linewidth=1.0,
            zorder=1,
        )

        # Matched experimental assignments for this hyperfine state
        if df_assignments is not None and not df_assignments.empty:
            df_sub = df_assignments[
                df_assignments["Hyperfine_Comp"] == hf_comp
            ]
            for _, row in df_sub.iterrows():
                x_pos = row["Exp_Freq_GHz"]
                y_pos = row["Exp_Intensity"]
                branch = str(row["Branch"])
                iso_str = str(row["Isotope"])
                q_val = row["N_double_prime"]

                if xlim is not None and not (xlim[0] <= x_pos <= xlim[1]):
                    continue

                iso_num = int("".join(filter(str.isdigit, iso_str)))
                color = branch_colors.get(branch, "gray")
                marker = style_map.get(iso_num, {}).get("marker", "o")

                ax.scatter(
                    x_pos,
                    y_pos,
                    color=color,
                    marker=marker,
                    s=35,
                    edgecolor="black",
                    linewidth=0.5,
                    zorder=4,
                )

                q_str = (
                    str(int(q_val))
                    if float(q_val).is_integer()
                    else f"{q_val:.1f}"
                )
                peak_label = rf"$^{{{iso_num}}}\mathrm{{YbF}}\ {branch}({q_str})$"
                ax.text(
                    x=x_pos,
                    y=y_pos + (max_meas * 0.03),
                    s=peak_label,
                    ha="center",
                    va="bottom",
                    fontsize=label_fontsize,
                    color=color,
                    rotation=90,
                    zorder=5,
                )

        ax.set_title(
            f"Hyperfine State: {hf_labels[hf_idx]}", fontweight="bold"
        )
        ax.set_ylim(bottom=0, top=max_meas * 1.35)
        ax.minorticks_on()
        ax.grid(
            visible=True,
            which="major",
            linestyle="-",
            linewidth=0.5,
            alpha=0.4,
        )
        ax.grid(
            visible=True,
            which="minor",
            linestyle=":",
            linewidth=0.3,
            alpha=0.3,
        )

    if xlim is not None:
        axes[0].set_xlim(xlim)

    fig.supxlabel(
        f"Frequency Offset (GHz) from {center_freq_thz} THz", fontsize=12
    )
    fig.supylabel("Intensity / PMT Signal", fontsize=12)

    # Build Legend
    legend_elements = [
        mlines.Line2D(
            [],
            [],
            color="black",
            alpha=0.3,
            linewidth=1,
            label="Measured Spectrum",
        )
    ]
    for iso, props in style_map.items():
        legend_elements.append(
            mlines.Line2D(
                [],
                [],
                color="black",
                marker=props["marker"],
                linestyle="None",
                label=props["label"],
            )
        )
    for b_name, b_color in branch_colors.items():
        legend_elements.append(
            mlines.Line2D(
                [],
                [],
                color=b_color,
                linewidth=1.5,
                label=f"Branch {b_name}",
            )
        )

    axes[1].legend(
        handles=legend_elements,
        title="Key",
        bbox_to_anchor=(1.05, 1),
        loc="upper left",
    )
    plt.tight_layout()
    return fig, axes
#%% Peak fitting
def multi_gaussian_fixed_bg(x, baseline, *params):
    """Sum of N Gaussians evaluated on a fixed global baseline."""
    y = np.full_like(x, baseline, dtype=float)
    for i in range(0, len(params), 3):
        amp, mu, sigma = params[i : i + 3]
        y += amp * np.exp(-0.5 * ((x - mu) / sigma) ** 2)
    return y

def fit_all_assigned_lines_enhanced(
    x_data,
    y_data,
    lines_df,
    freq_col="Frequency_Offset_GHz",
    global_baseline=None,
    initial_fwhm_ghz=0.08,  # ~80 MHz initial guess
    min_fwhm_ghz=0.03,  # 30 MHz floor
    max_fwhm_ghz=0.10,  # 100 MHz ceiling
    max_center_shift=0.25,
    max_cluster_gap=1.2,
):
    """Cluster-based peak fitting locked to a single uniform global baseline."""
    x_data = np.asarray(x_data, dtype=float)
    y_data = np.asarray(y_data, dtype=float)

    # Clean NaNs and sort
    valid_mask = ~np.isnan(x_data) & ~np.isnan(y_data)
    x_data, y_data = x_data[valid_mask], y_data[valid_mask]

    sort_idx = np.argsort(x_data)
    x_data, y_data = x_data[sort_idx], y_data[sort_idx]

    # Determine global baseline across the entire spectrum
    if global_baseline is None:
        global_baseline = float(np.percentile(y_data, 5))
    print(f"Global Baseline set to: {global_baseline:.4f}")

    # Explicit fit model defined inside function scope
    def model_func(x, *params):
        y = np.full_like(x, global_baseline, dtype=float)
        for i in range(0, len(params), 3):
            amp, mu, sigma = params[i : i + 3]
            y += amp * np.exp(-0.5 * ((x - mu) / sigma) ** 2)
        return y

    initial_sigma = initial_fwhm_ghz / 2.35482
    min_sigma = min_fwhm_ghz / 2.35482
    max_sigma = max_fwhm_ghz / 2.35482

    df_sorted = lines_df.copy().sort_values(by=freq_col).reset_index(drop=True)
    num_lines = len(df_sorted)

    for col in [
        "Fitted_Center",
        "Center_Err",
        "Fitted_Amp",
        "Amp_Err",
        "Fitted_Sigma",
        "Fitted_FWHM",
        "FWHM_Err",
        "Integrated_Area",
        "Cluster_ID",
    ]:
        df_sorted[col] = np.nan

    full_fit_y = np.full_like(y_data, global_baseline, dtype=float)
    popt_dict = {}
    peak_counts = df_sorted["Peak_Index"].value_counts().to_dict()

    # 1. Group peaks into local clusters
    clusters = []
    current_cluster = [0]
    for i in range(1, num_lines):
        prev_center = df_sorted.loc[i - 1, freq_col]
        curr_center = df_sorted.loc[i, freq_col]
        if (curr_center - prev_center) <= max_cluster_gap:
            current_cluster.append(i)
        else:
            clusters.append(current_cluster)
            current_cluster = [i]
    if current_cluster:
        clusters.append(current_cluster)

    # 2. Fit clusters
    for c_idx, cluster_indices in enumerate(clusters):
        cluster_rows = df_sorted.loc[cluster_indices]

        c_min = cluster_rows[freq_col].min() - (3 * initial_sigma + max_center_shift)
        c_max = cluster_rows[freq_col].max() + (3 * initial_sigma + max_center_shift)

        win_mask = (x_data >= c_min) & (x_data <= c_max)
        x_win, y_win = x_data[win_mask], y_data[win_mask]

        if len(x_win) < 5:
            continue

        p0 = []
        lower_bounds = []
        upper_bounds = []

        for _, row in cluster_rows.iterrows():
            mu_init = float(row[freq_col])
            idx_near = np.argmin(np.abs(x_win - mu_init))
            amp_guess = max(0.001, y_win[idx_near] - global_baseline) / peak_counts.get(
                row["Peak_Index"], 1
            )

            p0.extend([amp_guess, mu_init, initial_sigma])
            lower_bounds.extend([0.0, mu_init - max_center_shift, min_sigma])
            upper_bounds.extend([
                np.max(y_win) * 1.5,
                mu_init + max_center_shift,
                max_sigma,
            ])

        try:
            popt, pcov = curve_fit(
                model_func,
                x_win,
                y_win,
                p0=p0,
                bounds=(lower_bounds, upper_bounds),
                maxfev=5000 * len(cluster_rows),
            )
            perr = np.sqrt(np.diag(pcov))

            popt_dict[c_idx] = popt
            full_fit_y[win_mask] = model_func(x_win, *popt)

            for i, orig_idx in enumerate(cluster_indices):
                base_idx = i * 3
                amp, center, sigma = popt[base_idx : base_idx + 3]
                amp_err, center_err, sigma_err = perr[base_idx : base_idx + 3]

                df_sorted.loc[orig_idx, "Cluster_ID"] = c_idx
                df_sorted.loc[orig_idx, "Fitted_Center"] = center
                df_sorted.loc[orig_idx, "Center_Err"] = center_err
                df_sorted.loc[orig_idx, "Fitted_Amp"] = amp
                df_sorted.loc[orig_idx, "Amp_Err"] = amp_err
                df_sorted.loc[orig_idx, "Fitted_Sigma"] = sigma
                df_sorted.loc[orig_idx, "Fitted_FWHM"] = 2.35482 * sigma
                df_sorted.loc[orig_idx, "FWHM_Err"] = 2.35482 * sigma_err
                df_sorted.loc[orig_idx, "Integrated_Area"] = (
                    amp * sigma * np.sqrt(2 * np.pi)
                )

        except Exception as e:
            print(f"Warning: Cluster {c_idx} fit did not converge: {e}")

    return df_sorted, full_fit_y, popt_dict, global_baseline

def plot_fitted_spectrum(
    x_data,
    y_data,
    full_fit_y,
    df_fitted,
    global_baseline,
    freq_col="Frequency_Offset_GHz",
    zoom_range=None,
    figsize=(14, 5),
    dpi=120,
    show_individual_gaussians=True,
    annotate_labels=True,
    label_fontsize=8,
    label_rotation=90,
):
    """Plots multi-Gaussian profiles against a unified global baseline."""
    x_data = np.asarray(x_data, dtype=float)
    y_data = np.asarray(y_data, dtype=float)

    fig, ax = plt.subplots(figsize=figsize, dpi=dpi)

    ax.plot(
        x_data,
        y_data,
        color="black",
        alpha=0.35,
        label="Measured Spectrum",
        lw=1,
    )
    ax.plot(
        x_data,
        full_fit_y,
        color="crimson",
        lw=1.4,
        label="Total Fit Envelope",
    )

    if zoom_range is not None:
        x_min, x_max = zoom_range
        visible_mask = (x_data >= x_min) & (x_data <= x_max)
        df_visible = df_fitted[
            (df_fitted["Fitted_Center"] >= x_min)
            & (df_fitted["Fitted_Center"] <= x_max)
        ]
    else:
        visible_mask = np.ones_like(x_data, dtype=bool)
        df_visible = df_fitted

    # Plot single Gaussians directly on top of the global baseline floor
    if show_individual_gaussians or annotate_labels:
        for _, row in df_visible.iterrows():
            center = row.get("Fitted_Center")
            amp = row.get("Fitted_Amp")
            sigma = row.get("Fitted_Sigma")

            if pd.isna(center) or pd.isna(amp) or pd.isna(sigma):
                continue

            if show_individual_gaussians:
                x_sub_mask = visible_mask & (
                    np.abs(x_data - center) <= 4 * sigma
                )
                x_sub = x_data[x_sub_mask]

                if len(x_sub) > 0:
                    comp_y = global_baseline + amp * np.exp(
                        -0.5 * ((x_sub - center) / sigma) ** 2
                    )
                    ax.plot(x_sub, comp_y, "--", lw=0.8, alpha=0.7)

            if annotate_labels and "Label" in row and pd.notna(row["Label"]):
                label_text = str(row["Label"])
                peak_top = global_baseline + amp

                ax.annotate(
                    label_text,
                    xy=(center, peak_top),
                    xytext=(0, 5),
                    textcoords="offset points",
                    rotation=label_rotation,
                    ha="center",
                    va="bottom",
                    fontsize=label_fontsize,
                    alpha=0.85,
                )

    if zoom_range is not None:
        ax.set_xlim(zoom_range[0], zoom_range[1])
        y_visible = y_data[visible_mask]
        if len(y_visible) > 0:
            y_min, y_max = np.min(y_visible), np.max(y_visible)
            ax.set_ylim(
                y_min - 0.05 * (y_max - y_min), y_max + 0.35 * (y_max - y_min)
            )

    ax.set_title("Spectral Fit Profile & Line Deconvolution", fontsize=11)
    ax.set_xlabel(freq_col.replace("_", " "), fontsize=10)
    ax.set_ylabel("Intensity", fontsize=10)
    ax.legend(loc="upper right", frameon=True, facecolor="white", framealpha=0.9)
    ax.grid(True, linestyle=":", alpha=0.5)
    plt.tight_layout()
    plt.show()

    
#%% Fortrat diagram

def plot_fortrat_diagrams(
    df_fitted,
    isotope=None,  # Set to specific string (e.g. "174YbF"), or None for ALL
    freq_col="Fitted_Center",
    err_col="Center_Err",
    n_col="N",
    branch_col="Branch",
    comp_col="Component",
    iso_col="Isotope",
    figsize=(14, 9),
    dpi=120,
):
    """Plots 2x2 Fortrat diagrams with a single combined legend outside the right margin."""
    df = df_fitted.copy()
    import matplotlib.lines as mlines
    # Filter by specific isotope if requested
    if isotope is not None and iso_col in df.columns:
        df = df[df[iso_col] == isotope]
        plot_title = f"Fortrat Diagrams — {isotope}"
    else:
        plot_title = "Fortrat Diagrams (All Isotopes)"

    hyperfine_components = ["F=N+1", "F=N-", "F=N+", "F=N-1"]
    branches = ["O", "P", "Q", "R"]

    # Base colors for Branches
    branch_colors = {
        "O": "#1f77b4",  # Blue
        "P": "#ff7f0e",  # Orange
        "Q": "#2ca02c",  # Green
        "R": "#d62728",  # Red
    }

    # Markers for Isotopes
    iso_markers = ["o", "s", "^", "D", "v", "<", ">", "p"]
    unique_isotopes = (
        sorted(df[iso_col].dropna().unique())
        if iso_col in df.columns
        else ["All"]
    )
    marker_map = {
        iso: iso_markers[i % len(iso_markers)]
        for i, iso in enumerate(unique_isotopes)
    }

    fig, axes = plt.subplots(
        2, 2, figsize=figsize, dpi=dpi, sharex=True, sharey=True
    )
    axes = axes.flatten()

    for idx, comp in enumerate(hyperfine_components):
        ax = axes[idx]
        df_comp = df[df[comp_col] == comp]

        # Group by Isotope -> Branch
        for iso in unique_isotopes:
            df_iso = (
                df_comp[df_comp[iso_col] == iso]
                if iso_col in df_comp.columns
                else df_comp
            )
            marker = marker_map.get(iso, "o")

            for branch in branches:
                df_sub = df_iso[df_iso[branch_col] == branch].dropna(
                    subset=[freq_col, n_col]
                )

                if df_sub.empty:
                    continue

                # Sort strictly by N
                df_sub = df_sub.sort_values(by=n_col)

                x = df_sub[n_col].values
                y = df_sub[freq_col].values
                yerr = (
                    df_sub[err_col].values
                    if err_col in df_sub.columns
                    else None
                )

                color = branch_colors.get(branch, "gray")

                # Vertical error bars
                ax.errorbar(
                    x,
                    y,
                    yerr=yerr,
                    fmt=marker,
                    color=color,
                    ecolor=color,
                    elinewidth=1.0,
                    capsize=3,
                    capthick=1.0,
                    markersize=5,
                    alpha=0.85,
                )

                # Smooth connecting branch line
                ax.plot(
                    x,
                    y,
                    linestyle="--",
                    color=color,
                    alpha=0.4,
                    linewidth=1.0,
                )

        ax.set_title(f"Hyperfine State: {comp}", fontsize=11, fontweight="bold")
        ax.grid(True, linestyle=":", alpha=0.5)

    # -------------------------------------------------------------
    # BUILD SINGLE COMBINED LEGEND FOR RIGHT MARGIN
    # -------------------------------------------------------------
    combined_handles = []

    # Section 1: Branches Header & Items
    combined_handles.append(
        mlines.Line2D(
            [],
            [],
            color="none",
            label="$\\bf{Branches}$",
        )
    )
    for b, color in branch_colors.items():
        if b in df[branch_col].values:
            combined_handles.append(
                mlines.Line2D(
                    [],
                    [],
                    color=color,
                    marker="o",
                    linestyle="--",
                    markersize=5,
                    label=f"Branch {b}",
                )
            )

    # Section 2: Isotopes Header & Items (if multiple exist)
    if len(unique_isotopes) > 1:
        # Spacer
        combined_handles.append(mlines.Line2D([], [], color="none", label=""))
        combined_handles.append(
            mlines.Line2D(
                [],
                [],
                color="none",
                label="$\\bf{Isotopes}$",
            )
        )
        for iso in unique_isotopes:
            combined_handles.append(
                mlines.Line2D(
                    [],
                    [],
                    color="black",
                    marker=marker_map[iso],
                    linestyle="None",
                    markersize=5,
                    label=str(iso),
                )
            )

    # Place the single legend attached to the top-right subplot (axes[1])
    axes[1].legend(
        handles=combined_handles,
        loc="upper left",
        bbox_to_anchor=(1.02, 1.0),
        borderaxespad=0.0,
        fontsize=8.5,
        framealpha=0.9,
        handletextpad=0.8,
    )

    # Outer axis labeling
    for ax in axes[2:]:
        ax.set_xlabel("Rotational Quantum Number ($N$)", fontsize=10)
    for ax in [axes[0], axes[2]]:
        ax.set_ylabel("Fitted Frequency Offset (GHz)", fontsize=10)

    fig.suptitle(plot_title, fontsize=13, fontweight="bold", y=0.98)

    # Ensure layout fits the outer legend without clipping
    plt.tight_layout(rect=[0, 0, 0.88, 0.96])
    plt.show()
    
#%% Data manipulation
def prepare_fitting_data(
    df_fitted, center_freq_thz=568.681, aggregate_hyperfine=False
):
    """Extracts experimental frequencies and uncertainties from df_fitted for 
    upper-state constant refitting."""
    df = df_fitted.copy()

    # Calculate absolute experimental frequencies
    df["Exp_Freq_Offset_GHz"] = df["Fitted_Center"]
    df["Exp_Freq_Abs_GHz"] = (center_freq_thz * 1000.0) + df[
        "Exp_Freq_Offset_GHz"
    ]
    df["Exp_Freq_Abs_THz"] = center_freq_thz + (
        df["Exp_Freq_Offset_GHz"] / 1000.0
    )
    df["Exp_Uncertainty_GHz"] = df["Center_Err"]

    if "N" in df.columns:
        df["N_double_prime"] = df["N"]

    isotope_data = {}

    for iso, df_iso in df.groupby("Isotope"):
        df_clean = df_iso[
            [
                "Isotope",
                "Branch",
                "N_double_prime",
                "Component",
                "Exp_Freq_Offset_GHz",
                "Exp_Freq_Abs_GHz",
                "Exp_Uncertainty_GHz",
            ]
        ].copy()

        df_clean = df_clean.sort_values(
            by=["Branch", "N_double_prime"]
        ).reset_index(drop=True)
        isotope_data[str(iso)] = df_clean

    return isotope_data

def clean_fit_params(fit_df):
    """Cleans parameter names by removing '(FIXED)', primes, and extra spaces,

    returning a dictionary of values in cm-1 (or GHz if needed).
    """
    clean_dict = {}
    for _, row in fit_df.iterrows():
        # Clean parameter key: "ad' (FIXED)" -> "ad", "b'" -> "b"
        key = (
            row["Parameter"]
            .replace("(FIXED)", "")
            .replace("'", "")
            .strip()
            .lower()
        )

        clean_dict[key] = row["Fitted (cm-1)"]  # Use row["Fitted (GHz)"] if needed

    return clean_dict

def update_assigned_residuals(
    df_assigned,
    APi32_new_dict,
    vib_state="v1",
    center_freq_thz=568.681,
):
    """Recalculates 'Residual_GHz' in Lines_assigned using refined constants

    from APi32v0_new without altering the original assigned DataFrame structure.
    """
    c_cm_ghz = 29.9792458  # cm^-1 to GHz
    df_updated = df_assigned.copy()
    new_residuals = []

    # Detect the correct experimental offset column name
    exp_col = None
    possible_cols = [
        "Exp_Freq_Offset_GHz",
        "ency_Offse",
        "Exp_Offset_GHz",
        "Frequency_Offset",
    ]
    for col in possible_cols:
        if col in df_updated.columns:
            exp_col = col
            break

    if exp_col is None:
        # Fallback to the second column if matching by name fails
        exp_col = df_updated.columns[1]

    for _, row in df_updated.iterrows():
        # 1. Parse row metadata
        iso_str = str(row["Isotope"]).replace("YbF", "").strip()
        iso_num = int(iso_str) if iso_str.isdigit() else 174

        branch = str(row["Branch"]).strip().upper().replace("BRANCH ", "")
        comp_str = str(row["Component"]).strip()
        N_pp = int(row["N"] if "N" in row else row["N_double_prime"])

        # 2. Get upper and lower parameters for this isotope
        fitted_params_cm = APi32_new_dict[iso_num]
        excited_params_ghz = {
            k: v * c_cm_ghz for k, v in fitted_params_cm.items()
        }

        ground_params_ghz = get_converted_ground_params(
            vib_state=vib_state, iso=iso_num
        )

        # 3. Compute new predicted frequency
        if branch not in x_APi32_branches_new:
            new_residuals.append(np.nan)
            continue

        branch_func, j_calc_lambda, e_func = x_APi32_branches_new[branch]
        trans_energies_ghz = branch_func(
            n=N_pp,
            excited_params=excited_params_ghz,
            ground_params=ground_params_ghz,
            e_func=e_func,
        )

        comp_idx = HYPERFINE_BRANCH_INDEX_MAP.get(branch, {}).get(comp_str)
        if comp_idx is None or comp_idx >= len(trans_energies_ghz):
            new_residuals.append(np.nan)
            continue

        calc_abs_ghz = trans_energies_ghz[comp_idx]
        calc_offset_ghz = calc_abs_ghz - (center_freq_thz * 1000.0)

        # 4. Calculate updated residual: Exp - Pred (in GHz)
        exp_offset_ghz = row[exp_col]
        res_ghz = exp_offset_ghz - calc_offset_ghz
        new_residuals.append(res_ghz)

    df_updated["Residual_GHz"] = new_residuals

    # Ensure Hyperfine_Comp exists for the plotter mapping
    if "Hyperfine_Comp" not in df_updated.columns and "Component" in df_updated:
        df_updated["Hyperfine_Comp"] = df_updated["Component"]

    return df_updated
#%% Fitting constants
# Maps hyperfine component label string to the index within each branch's calculated array
# Note:
# O Branch returns mask=[2, 3] -> idx 0 corresponds to N+, idx 1 corresponds to N-1
# R Branch returns mask=[0, 1] -> idx 0 corresponds to N+1, idx 1 corresponds to N-
# P & Q Branches return all 4 -> idx 0: N+1, 1: N-, 2: N+, 3: N-1

HYPERFINE_BRANCH_INDEX_MAP = {
    "O": {"F=N+": 0, "N+": 0, "F=N-1": 1, "N-1": 1},
    "P": {
        "F=N+1": 0,
        "N+1": 0,
        "F=N-": 1,
        "N-": 1,
        "F=N+": 2,
        "N+": 2,
        "F=N-1": 3,
        "N-1": 3,
    },
    "Q": {
        "F=N+1": 0,
        "N+1": 0,
        "F=N-": 1,
        "N-": 1,
        "F=N+": 2,
        "N+": 2,
        "F=N-1": 3,
        "N-1": 3,
    },
    "R": {"F=N+1": 0, "N+1": 0, "F=N-": 1, "N-": 1},
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


# 3. Flexible Residual Calculator
def compute_residuals_flexible(
    active_param_values,
    active_param_names,
    fixed_params_ghz,
    df_iso,
    ground_params_ghz,
    center_freq_thz=568.681,
):
    """Computes residuals by merging active fit parameters with fixed constants."""
    # Reconstruct complete excited_params dictionary
    excited_params = fixed_params_ghz.copy()
    for name, val in zip(active_param_names, active_param_values):
        excited_params[name] = val

    residuals = []

    for _, row in df_iso.iterrows():
        N_pp = int(row["N_double_prime"])
        branch = str(row["Branch"]).strip().upper().replace("BRANCH ", "")
        comp_str = str(row["Component"]).strip()

        if branch not in x_APi32_branches_new:
            continue

        branch_func, j_calc_lambda, e_func = x_APi32_branches_new[branch]
        J_prime = j_calc_lambda(N_pp)

        if J_prime < 1.5:  # Minimum J for A^2Pi_3/2
            continue

        trans_energies_ghz = branch_func(
            n=N_pp,
            excited_params=excited_params,
            ground_params=ground_params_ghz,
            e_func=e_func,
        )

        comp_idx = HYPERFINE_BRANCH_INDEX_MAP.get(branch, {}).get(comp_str)

        if comp_idx is None or comp_idx >= len(trans_energies_ghz):
            continue

        calc_abs_ghz = trans_energies_ghz[comp_idx]

        if np.isnan(calc_abs_ghz):
            continue

        calc_offset_ghz = calc_abs_ghz - (center_freq_thz * 1000.0)
        exp_offset = row["Exp_Freq_Offset_GHz"]
        err = max(row["Exp_Uncertainty_GHz"], 1e-4)

        residuals.append((exp_offset - calc_offset_ghz) / err)

    return np.array(residuals)


# 4. Master Refit Function with Selective Freezing

def run_isotope_refit(
    df_iso,
    iso_num=174,
    vib_state="v1",
    vib_state_upper="v0",
    fixed_params=None,
    center_freq_thz=568.681,
):
    """Refits upper-state constants [t', b', ad', d'] using ground state vib_state.

    Prints line counts, degrees of freedom, and reduced Chi2.
    """
    if fixed_params is None:
        fixed_params = {}

    # 1. Hyperfine Map
    HYPERFINE_BRANCH_INDEX_MAP = {
        "O": {"F=N+": 0, "N+": 0, "F=N-1": 1, "N-1": 1},
        "P": {
            "F=N+1": 0,
            "N+1": 0,
            "F=N-": 1,
            "N-": 1,
            "F=N+": 2,
            "N+": 2,
            "F=N-1": 3,
            "N-1": 3,
        },
        "Q": {
            "F=N+1": 0,
            "N+1": 0,
            "F=N-": 1,
            "N-1": 1,
            "F=N+": 2,
            "N+": 2,
            "F=N-1": 3,
            "N-1": 3,
        },
        "R": {"F=N+1": 0, "N+1": 0, "F=N-": 1, "N-": 1},
    }

    # 2. Local Ground State Conversion to GHz
    g_params = X_State[vib_state][iso_num].copy()
    ground_params_ghz = {
        k: (v * c if k != "iso" else v) for k, v in g_params.items()
    }

    # 3. Local Upper State Guesses in GHz
    a_params = A_Pi_32[vib_state_upper][iso_num]
    all_initial_ghz = {
        "t": a_params["t"] * c,
        "b": a_params["b"] * c,
        "ad": a_params["ad"] * c,
        "d": a_params["d"] * c,
    }

    # 4. Process Fixed vs Active Parameters
    fixed_params_ghz = {k: v * c for k, v in fixed_params.items()}
    active_param_names = [
        p for p in ["t", "b", "ad", "d"] if p not in fixed_params
    ]
    x0_active_ghz = [all_initial_ghz[p] for p in active_param_names]

    # 5. Embedded Residual Calculator
    def compute_residuals_local(active_param_values):
        excited_params = fixed_params_ghz.copy()
        for name, val in zip(active_param_names, active_param_values):
            excited_params[name] = val

        residuals = []
        for _, row in df_iso.iterrows():
            N_pp = int(row["N_double_prime"])
            branch = str(row["Branch"]).strip().upper().replace("BRANCH ", "")
            comp_str = str(row["Component"]).strip()

            if branch not in x_APi32_branches_new:
                continue

            branch_func, j_calc_lambda, e_func = x_APi32_branches_new[branch]
            J_prime = j_calc_lambda(N_pp)
            if J_prime < 1.5:  # Minimum J for A^2Pi_3/2 state
                continue

            trans_energies_ghz = branch_func(
                n=N_pp,
                excited_params=excited_params,
                ground_params=ground_params_ghz,
                e_func=e_func,
            )

            comp_idx = HYPERFINE_BRANCH_INDEX_MAP.get(branch, {}).get(comp_str)
            if comp_idx is None or comp_idx >= len(trans_energies_ghz):
                continue

            calc_abs_ghz = trans_energies_ghz[comp_idx]
            if np.isnan(calc_abs_ghz):
                continue

            calc_offset_ghz = calc_abs_ghz - (center_freq_thz * 1000.0)
            exp_offset = row["Exp_Freq_Offset_GHz"]
            err = max(row["Exp_Uncertainty_GHz"], 1e-4)

            residuals.append((exp_offset - calc_offset_ghz) / err)

        return np.array(residuals)

    # 6. Perform Optimization
    res = least_squares(compute_residuals_local, x0=x0_active_ghz, method="lm")

    # 7. Extract Line Counts & Statistics
    n_lines = len(res.fun)
    k_params = len(res.x)
    dof = n_lines - k_params
    reduced_chi2 = np.sum(res.fun**2) / dof if dof > 0 else np.nan

    cov = np.linalg.inv(res.jac.T @ res.jac) * reduced_chi2
    p_errs_active_ghz = np.sqrt(np.diag(cov))

    # 8. Build DataFrame Output
    table_rows = []
    active_idx = 0
    for p in ["t", "b", "ad", "d"]:
        p_label = f"{p}'"
        if p in fixed_params:
            val_cm = fixed_params[p]
            val_ghz = val_cm * c
            table_rows.append(
                {
                    "Parameter": f"{p_label} (FIXED)",
                    "Initial (cm-1)": val_cm,
                    "Fitted (cm-1)": val_cm,
                    "Std_Err (cm-1)": 0.0,
                    "Fitted (GHz)": val_ghz,
                    "Std_Err (GHz)": 0.0,
                }
            )
        else:
            val_ghz = res.x[active_idx]
            err_ghz = p_errs_active_ghz[active_idx]
            init_cm = all_initial_ghz[p] / c
            table_rows.append(
                {
                    "Parameter": p_label,
                    "Initial (cm-1)": init_cm,
                    "Fitted (cm-1)": val_ghz / c,
                    "Std_Err (cm-1)": err_ghz / c,
                    "Fitted (GHz)": val_ghz,
                    "Std_Err (GHz)": err_ghz,
                }
            )
            active_idx += 1

    results_df = pd.DataFrame(table_rows)

    # Print summary with line count
    print(
        f"\n=== Refit Results for {iso_num}YbF (Ground: {vib_state}) ==="
        f"\n  • Fit Lines (N): {n_lines}"
        f"\n  • Active Params (k): {k_params}"
        f"\n  • Degrees of Freedom (dof): {dof}"
        f"\n  • Reduced Chi2: {reduced_chi2:.4f}"
    )

    return results_df, res

#%% Example code   
if __name__ == "__main__":
    print("--- Running Local YbF Spectroscopy Library Tests ---")
#% Example run for getting hyperfine structures of X under rotational
#   levels N = 0-2 for each vibrational levels v = 0-3
# 3. Create an empty list to accumulate row dictionaries
    results_list = []

    # Define the range of rotational quantum numbers you want to calculate
    n_values = range(0, 3)  # Example: n from 0 to 2

    # 4. Run the nested loop and collect data
    for n in n_values:
        for iso, rules in X_State["v0"].items():
            # Evaluate functions (returns numpy arrays of [term1, term2])
            f1_levels = F1(n, **rules)
            f2_levels = F2(n, **rules)
            
            # Flatten assignments to match your specific state notations
            row = {
                "n": n,
                "isotope": iso,
                "F1_F=N+1": f1_levels[0],
                "F1_F=N_l": f1_levels[1],
                "F2_F=N-1": f2_levels[0],
                "F2_F=N_h": f2_levels[1],
                "d_F1 (MHz)": (f1_levels[0]-f1_levels[1])* CM_INV_TO_THZ * 1e6,
                "d_F2 (MHz)": (f2_levels[0]-f2_levels[1])* CM_INV_TO_THZ * 1e6
            }
            
            results_list.append(row)

    # 5. Convert the accumulated list of dictionaries into a Pandas DataFrame
    df = pd.DataFrame(results_list)

    # Print a preview of the structured dataset (first 8 rows)
    #print(df.head(8))
    print(df)
    
    #% Example to get list of spectra lines
    # 1. Generate A 2Pi_1/2 spectrum
    df_Pi12_v0 = GetSpectra_N(
        n_ground_start = 0,
        n_ground_end = 10,
        transition = x_APi12_branches,
        excited_params = A_Pi_12["v0"][174],
        ground_params = X_State["v0"][174],
        temp = 4
    )
    
    # 2. Generate A 2Pi_3/2 spectrum using the new upper-state functions
    df_Pi32_v1 = GetSpectra_N(
        n_ground_start = 0,
        n_ground_end = 10,
        transition = x_APi32_branches,
        excited_params = A_Pi_32["v0"][174],
        ground_params = X_State["v1"][174],
        temp = 4
        )
    
    df_Pi32_v1_new = GetSpectra_N_new(
        n_ground_start = 0,
        n_ground_end = 10,
        transition = x_APi32_branches_new,
        excited_params = A_Pi_32["v0"][174],
        ground_params = X_State["v1"][174],
        temp = 4
        )
    
    df_4f72_v1 = GetSpectra_J(
        j_ground_start = 0.5,
        j_ground_end = 9.5,
        transition = branches_4f,
        excited_params = rule_1871,
        ground_params = State_4f_7212["v1"][174],
        temp = 4
        )
    
    # Zoomed-in to central region (-50 GHz to +50 GHz)
    plot_ybf_spectrum(df_Pi32_v1, center_freq_thz=568.664588, xlim=(-1, 1))
    
    
    
    State_4fv1_32 = Ue4f(1.5, **State_4f_7212["v1"][174])
    State_561_32_Dunfield = Uf(1.5, **State_561["v0"][174])
    State_561_32_Popa = Uf4f(1.5, **rule_1871)
    
    State_561_12_Dunfield = Ue(0.5, **State_561["v0"][174])
    State_561_12_Popa = Ue4f(0.5, **rule_1871)
    
    State_561_32_Dunfield_172 = Uf(1.5, **State_561["v0"][172])
    State_561_12_Dunfield_172 = Ue(0.5, **State_561["v0"][172])
    
    State_561_32_Dunfield_176 = Uf(1.5, **State_561["v0"][176])
    State_561_12_Dunfield_176 = Ue(0.5, **State_561["v0"][176])
    
    trans_Dunfield_32 = (State_561_32_Dunfield - State_4fv1_32)*CM_INV_TO_THZ
    trans_Dunfield_12 = (State_561_12_Dunfield - State_4fv1_32)*CM_INV_TO_THZ
    
    trans_Popa_32 = (State_561_32_Popa - State_4fv1_32)*CM_INV_TO_THZ
    trans_Popa_12 = (State_561_12_Popa - State_4fv1_32)*CM_INV_TO_THZ
    
    Split_561_Dunfield = (State_561_32_Dunfield - State_561_12_Dunfield)*CM_INV_TO_THZ*1e3
    Split_561_Popa = (State_561_32_Popa - State_561_12_Popa)*CM_INV_TO_THZ*1e3
    
    Split_561_Dunfield_172 = (State_561_32_Dunfield_172 - State_561_12_Dunfield_172)*CM_INV_TO_THZ*1e3
    Split_561_Dunfield_176 = (State_561_32_Dunfield_176 - State_561_12_Dunfield_176)*CM_INV_TO_THZ*1e3
    
    print("For transitions from 4fv1 J=3/2 e:")
    print("         Using Dunfield constants for [561]:")
    print("              To [561] J=3/2: %.9g THz"%trans_Dunfield_32)
    print("              To [561] J=1/2: %.9g THz"%trans_Dunfield_12)
    print("         Using Popa constants for [561]:")
    print("              To [561] J=3/2: %.9g THz"%trans_Popa_32)
    print("              To [561] J=1/2: %.9g THz"%trans_Popa_12)
    print("Splitting between [561] J=3/2 and J=1/2:")
    print("     174:")
    print("         J=3/2 - J=1/2 Dunfield: %g GHz"%Split_561_Dunfield)
    print("         J=3/2 - J=1/2 Popa: %g GHz"%Split_561_Popa)
    print("     172:")
    print("         J=3/2 - J=1/2 Dunfield: %g GHz"%Split_561_Dunfield_172)
    print("     176:")
    print("         J=3/2 - J=1/2 Dunfield: %g GHz"%Split_561_Dunfield_176)
