"""Zero-field energy levels from effective Hamiltonians.

X(2Sigma+): eRotor / gamma_spinRot / F1 / F2 / X_hyperfine (hyperfine, case (b)).
A(2Pi_1/2), A(2Pi_3/2), [4f] states: Ue / Uf and variants (parity-resolved, case (a)).

Extracted verbatim from YbF_spectroscopy_library.py on 2026-09-16 — the physics code is byte-identical to the
version that produced the group's published assignments. Do not retype values here;
edit and re-run Tests/test_levels_regression.py if a constant genuinely changes.
"""

import numpy as np


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
