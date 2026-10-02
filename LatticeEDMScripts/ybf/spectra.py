"""Stick-spectrum generation (Boltzmann-weighted) and branch extraction helpers.

Extracted verbatim from YbF_spectroscopy_library.py on 2026-09-16 — the physics code is byte-identical to the
version that produced the group's published assignments. Do not retype values here;
edit and re-run Tests/test_levels_regression.py if a constant genuinely changes.
"""

import numpy as np
import pandas as pd
from .units import CM_INV_TO_THZ, hck
from .levels import Ue4f, eRotor


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


_UNIT_LABELS = {"Wavenumber_cm-1": "cm-1", "Frequency_THz": "THz"}


def branch_table(df, value_cols=("Wavenumber_cm-1", "Frequency_THz")):
    """Pivot a GetSpectra_N/GetSpectra_J output so branches become columns and
    the ground-state quantum number (N'' or J'') becomes the row index.

    This is the display tuned in Bulk analysis scripts/4f spectroscopy.py:
    lets you eyeball predicted transition frequencies branch-by-branch, and
    cross-check them against saved or experimentally measured values.

    With more than one entry in value_cols (the default: cm-1 and THz), columns
    become a (Branch, Unit) MultiIndex so both units sit side by side per branch.
    """
    df = df.copy()
    df.columns = df.columns.str.strip()
    index_col = "J''" if "J''" in df.columns else "N''"
    value_cols = list(value_cols)

    table = df.pivot(index=index_col, columns="Branch", values=value_cols).sort_index()
    if len(value_cols) > 1:
        table = table.swaplevel(axis=1).sort_index(axis=1, level=0, sort_remaining=False)
        table = table.rename(columns=_UNIT_LABELS, level=1)
    table.index.name = index_col.strip("'")
    return table
