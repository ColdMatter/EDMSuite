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


@author: sl5119, Simeng Li
"""

#%% Packages and constants
import numpy as np
import pandas as pd

c = 29.9792458 # speed of light in in cm*GHz
hck = 6.626*1.38065*2.99792458e-1 # h*c/k_B in cm/K to get rid of cm^-1 when 
                                  # doing Boltzmann distribution
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
    term1 = gamma * n/2 + Xb/4 + Xc/(4*(2*n+3)) + Xc*n/2
    term2 = -(gamma + Xb + XC)/4 - np.sqrt((gamma-XC)**2 * (2*n+1)**2 +\
            (2*Xb + Xc - 2*Xc) * (2*Xb + Xc - 2*gamma))/4
    
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
    term1 = -gamma * (n+1)/2 + Xb/4 + Xc/(4*(2*n-1)) - Xc*(n+1)/2
    term2 = -(gamma + Xb + XC)/4 + np.sqrt((gamma-XC)**2 * (2*n+1)**2 +\
            (2*Xb + Xc - 2*Xc) * (2*Xb + Xc - 2*gamma))/4
    
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

'''4f states, Stefan Popa's paper (2024)'''



#%% Constants
'''All constants are using the highest precision version from Jongseok
   in his July 2026 Mathmatica notebook.
'''
'''  For X Σ ground states:
    (iso, tv, b, d, g0, g1, g2, Xb, Xc, XC)

'''

X_State = {
    # Note: 174 constants are converted from MHz to cm^-1
    174: {
        "v0": {
            "iso": 0.32, "Tv": 0, "Bpp": 7233.8271e-3 / c, "dpp": 2.388e-7,
            "g0": -13.41679e-3 / c, "g1": 3.9840e-6 / c, "g2": -25e-12 / c,
            "Xb": (170.26374 - 85.4028 / 3) * 1e-3 / c, "Xc": 85.4028e-3 / c, "XC": 13.099e-6 / c
        },
        "v1": {
            "iso": 0.32, "Tv": 502.15090, "Bpp": 7188.8919e-3 / c, "dpp": 2.303e-7,
            "g0": -33.81036e-3 / c, "g1": 4.3205e-6 / c, "g2": -28e-12 / c,
            "Xb": (168.770 - 86.7120 / 3) * 1e-3 / c, "Xc": 86.7120e-3 / c, "XC": 17.20e-6 / c
        },
        "v2": {
            "iso": 0.32, "Tv": 999.81290, "Bpp": 7144.20e-3 / c, "dpp": 6.649e-6 / c,
            "g0": -54.20393e-3 / c, "g1": 4.657e-6 / c, "g2": -31e-12 / c,
            "Xb": 137.936e-3 / c, "Xc": 88.0212e-3 / c, "XC": 21.301e-6 / c
        },
        "v3": {
            "iso": 0.32, "Tv": 1493.0215, "Bpp": 7099.63e-3 / c, "dpp": 6.394e-6 / c,
            "g0": -74.5975e-3 / c, "g1": 4.9935e-6 / c, "g2": -34e-12 / c,
            "Xb": 136.006e-3 / c, "Xc": 89.3304e-3 / c, "XC": 25.402e-6 / c
        }
    },
    # For other even isotopes, gammas and hyperfine parameters are assumed 
    # to be the same as for 174. (x172v3rules are deduced)
    172: {
        "v0": {
            "iso": 0.219, "Tv": 0, "Bpp": 0.241717, "dpp": 2.48e-7,
            "g0": -13.41679e-3 / c, "g1": 3.9840e-6 / c, "g2": -25e-12 / c,
            "Xb": (170.26374 - 85.4028 / 3) * 1e-3 / c, "Xc": 85.4028e-3 / c, "XC": 13.099e-6 / c
        },
        "v1": {
            "iso": 0.219, "Tv": 502.4629, "Bpp": 0.240177, "dpp": 2.369e-7,
            "g0": -33.81036e-3 / c, "g1": 4.3205e-6 / c, "g2": -28e-12 / c,
            "Xb": (168.770 - 86.7120 / 3) * 1e-3 / c, "Xc": 86.7120e-3 / c, "XC": 17.20e-6 / c
        },
        "v2": {
            "iso": 0.219, "Tv": 1000.4293, "Bpp": 0.238617, "dpp": 2.102e-7,
            "g0": -54.20393e-3 / c, "g1": 4.657e-6 / c, "g2": -31e-12 / c,
            "Xb": 137.936e-3 / c, "Xc": 88.0212e-3 / c, "XC": 21.301e-6 / c
        },
        "v3": {
            "iso": 0.219, "Tv": 1493.899, "Bpp": 0.237067, "dpp": 2.102e-7,
            "g0": -74.5975e-3 / c, "g1": 4.9935e-6 / c, "g2": -34e-12 / c,
            "Xb": 136.006e-3 / c, "Xc": 89.3304e-3 / c, "XC": 25.402e-6 / c
        }
    },
    176: {
        "v0": {
            "iso": 0.127, "Tv": 0, "Bpp": 0.241247, "dpp": 2.549e-7,
            "g0": -13.41679e-3 / c, "g1": 3.9840e-6 / c, "g2": -25e-12 / c,
            "Xb": (170.26374 - 85.4028 / 3) * 1e-3 / c, "Xc": 85.4028e-3 / c, "XC": 13.099e-6 / c
        },
        "v1": {
            "iso": 0.127, "Tv": 501.8905, "Bpp": 0.239729, "dpp": 2.487e-7,
            "g0": -33.81036e-3 / c, "g1": 4.3205e-6 / c, "g2": -28e-12 / c,
            "Xb": (168.770 - 86.7120 / 3) * 1e-3 / c, "Xc": 86.7120e-3 / c, "XC": 17.20e-6 / c
        },
        "v2": {
            "iso": 0.127, "Tv": 999.3216, "Bpp": 0.238137, "dpp": 2.104e-7,
            "g0": -54.20393e-3 / c, "g1": 4.657e-6 / c, "g2": -31e-12 / c,
            "Xb": 137.936e-3 / c, "Xc": 88.0212e-3 / c, "XC": 21.301e-6 / c
        }
    }
}

''' For A Pi 1/2 excited states, Dunfield paper:          
        *This only has v=0 because v=1 is strongly mixed with 4f Ja=5/2.
        See [561] and [557] below.
        **The "v0" label is kept here to keep dictionary structure consistent
'''
A_Pi_12 = {
    172: {
        "v0": {
            "t": 18106.2265, "b": 0.248064, "ad": 1.1848e-3,
            "d": 2.538e-7, "p2q": -0.39667, "dp2q": 1.142e-6
        }
    },
    174: {
        "v0": {
            "t": 18106.1991, "b": 0.247758, "ad": 1.1864e-3,
            "d": 2.453e-7, "p2q": -0.39635, "dp2q": 1.173e-6
        }
    },
    176: {
        "v0": {
            "t": 18106.1714, "b": 0.247579, "ad": 1.1875e-3,
            "d": 2.607e-7, "p2q": -0.39553, "dp2q": 1.006e-6
        }
    }
}


''' For A Pi 3/2 excited states, Dunfield paper:                 '''
A_Pi_32 = {
    172: {
        "v0": {
            "t": 19471.5237,
            "b": 0.248064,
            "ad": 1.1848e-3,
            "d": 2.538e-7,
        }
    },
    174: {
        "v0": {
            "t": 19471.4899,
            "b": 0.247758,
            "ad": 1.1864e-3,
            "d": 2.453e-7,
        }
    },
    176: {
        "v0": {
            "t": 19471.4574,
            "b": 0.247579,
            "ad": 1.1875e-3,
            "d": 2.607e-7,
        }
    },
}

''' For the purturbed states, [561] and [557]
    Note:
        They originate from mixing of A Pi 1/2 v=1 and 4f Ja=5/2. The old
        labelling was [18.6]0.5 v=0 and v=1 respectively.
    
    [561] are Jongseok's constants from his spectra (2017 paper)
    [557] are Dunfield's constants
'''
State_561 = {
    172: {
        "v0": {
            "t": 18705.0802, "b": 0.257242, "ad": 0,
            "d": -5.963e-7, "p2q": -1.00031334, "dp2q": -10.042e-5
        }
    },
    174: {
        "v0": {
            "t": 18704.9353, "b": 0.256964, "ad": 0,
            "d": -5.934e-7, "p2q": -1.00284405, "dp2q": -9.910e-5
        }
    },
    176: {
        "v0": {
            "t": 18704.8073, "b": 0.256820, "ad": 0,
            "d": -5.691e-7, "p2q": -1.00439729, "dp2q": -9.846e-5
        }
    }
}

State_557 = {
    172: {
        "v1": {
            "t": 18580.6837, "b": 0.255680, "ad": 1.1864e-3,
            "d": 9.984e-7, "p2q": -0.90021, "dp2q": 8.441e-5
        }
    },
    174: {
        "v1": {
            "t": 18580.5317, "b": 0.255305, "ad": 1.1864e-3,
            "d": 9.670e-7, "p2q": -0.89614, "dp2q": 8.265e-5
        }
    },
    176: {
        "v1": {
            "t": 18580.3792, "b": 0.255095, "ad": 1.1848e-3,
            "d": 9.834e-7, "p2q": -0.89350, "dp2q": 8.249e-5
        }
    }
}
#%% Example run for getting hyperfine structures of X under rotational
#   levels N = 0-2 for each vibrational levels v = 0-3
# 3. Create an empty list to accumulate row dictionaries
results_list = []

# Define the range of rotational quantum numbers you want to calculate
n_values = range(0, 3)  # Example: n from 0 to 2

# 4. Run the nested loop and collect data
for n in n_values:
    for vib, rules in X_State[174].items():
        # Evaluate functions (returns numpy arrays of [term1, term2])
        f1_levels = F1(n, **rules)
        f2_levels = F2(n, **rules)
        
        # Flatten assignments to match your specific state notations
        row = {
            "n": n,
            "v": vib,
            "F1_F=N+1": f1_levels[0],
            "F1_F=N_l": f1_levels[1],
            "F2_F=N-1": f2_levels[0],
            "F2_F=N_h": f2_levels[1]
        }
        
        results_list.append(row)

# 5. Convert the accumulated list of dictionaries into a Pandas DataFrame
df = pd.DataFrame(results_list)

# Print a preview of the structured dataset (first 8 rows)
#print(df.head(8))
print(df)

#%% For all isotopes
# List to accumulate row dictionaries
results_list = []

# Define the range of rotational quantum numbers (e.g., N from 0 to 2)
n_values = range(0, 3)

# Nested loop over Isotopes, Vibrational levels, and Rotational levels
for iso_num, vib_dict in X_State.items():
    for vib, rules in vib_dict.items():
        for n in n_values:
            # Evaluate functions (returns numpy arrays of [term1, term2])
            f1_levels = F1(n, **rules)
            f2_levels = F2(n, **rules)
            
            # Store structured result for this isotope, vibrational level, and N
            row = {
                "isotope": iso_num,
                "v": vib,
                "n": n,
                "F1_F=N+1": f1_levels[0],
                "F1_F=N_l": f1_levels[1],
                "F2_F=N-1": f2_levels[0],
                "F2_F=N_h": f2_levels[1]
            }
            
            results_list.append(row)

# Convert the accumulated list into a Pandas DataFrame
df = pd.DataFrame(results_list)

#%% Display the complete DataFrame
print(df[174].head(8))
#%%
import matplotlib.pyplot as plt

# Filter down to just version 0 data
v0_data = df[df["v"] == "v0"]

# Plot N_l and N_h states versus quantum number n
plt.plot(v0_data["n"], v0_data["F1_F=N_l"], label="F1, F=N_l", marker="o")
plt.plot(v0_data["n"], v0_data["F2_F=N_h"], label="F2, F=N_h", marker="s")

plt.xlabel("Quantum Number (n)")
plt.ylabel("Hyperfine Energy Level")
plt.legend()
plt.show()