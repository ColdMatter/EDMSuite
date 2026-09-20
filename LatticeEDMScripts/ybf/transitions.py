"""Branch transition energies and branch lookup tables built from the level functions.

Extracted verbatim from YbF_spectroscopy_library.py on 2026-09-16 — the physics code is byte-identical to the
version that produced the group's published assignments. Do not retype values here;
edit and re-run Tests/test_levels_regression.py if a constant genuinely changes.
"""

from .levels import F1, F2, Ue, Ue3JCP, Ue4f, Uf, Uf3JCP, Uf4f, X_hyperfine


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
