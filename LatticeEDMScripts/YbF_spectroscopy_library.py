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
#import matplotlib.lines as mlines

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
'''

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
    stick_offset_ghz=0.0,
    figsize=(12, 5),
    ytitle="Relative Population / Intensity"
):
    """Plots multi-isotope theoretical stick spectra overlaid on measured data.

    Line styles:
    - 174: Solid ('-')
    - 172: Dashed ('--')
    - 176: Dotted (':')
    """
    # Define line style and label mapping per isotope
    style_map = {
        174: {"linestyle": "-", "label": "$^{174}$YbF"},
        172: {"linestyle": "--", "label": "$^{172}$YbF"},
        176: {"linestyle": ":", "label": "$^{176}$YbF"},
    }
    
    import matplotlib.lines as mlines
    
    # 1. Process measured spectrum if provided
    has_measured = measured_x is not None and measured_y is not None
    if has_measured:
        m_x = np.asarray(measured_x, dtype=float)
        m_y = np.asarray(measured_y, dtype=float)
        if measured_in_thz:
            m_x = (m_x - center_freq_thz) * 1000.0

    # 2. Extract branch data for all isotopes and apply offsets
    all_iso_branches = {}
    max_stick_174 = 0.0

    for iso, df in dfs_by_isotope.items():
        branches = extract_branch_data(df, use_wavenumber=use_wavenumber)
        for branch_name, data in branches.items():
            data["x_offset"] = (
                (data["x"] - center_freq_thz) * 1000.0
            ) + stick_offset_ghz

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

            # Quantum number annotations
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
                    fontsize=7,
                    color=color,
                    zorder=3,
                )

    # 6. Apply xlim if provided
    if xlim is not None:
        ax.set_xlim(xlim)

    # 7. Legend & Styling
    ax.set_xlabel(
        f"Frequency Offset (GHz) from {center_freq_thz}", fontsize=11
    )
    ax.set_ylabel(ytitle, fontsize=11)
    ax.set_title(
        r"YbF $A\,^2\Pi_{3/2} \leftarrow X\,^2\Sigma^+$ Multi-Isotope Spectrum"\
            +", with offset %g GHz to predictions"%stick_offset_ghz,
        fontsize=12,
    )
    ax.set_ylim(bottom=0, top=max_pop * 1.15)
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
#%%    
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
