"""One definition of the regression grid, evaluated against two providers.

`compute_grid` is called twice: once with the legacy YbF_spectroscopy_library module
(to mint Tests/data/golden_levels.json) and once with the `ybf` package (in the test).
Both must agree to machine precision. Add a case here when you add physics.
"""

import numpy as np

X_VIBS = ('v0', 'v1', 'v2')
N_GRID = (0, 1, 2, 3, 5, 8)
J_GRID = (0.5, 1.5, 2.5, 3.5, 6.5)


def _rec(out, label, value):
    out[label] = np.atleast_1d(np.asarray(value, dtype=float)).ravel().tolist()


def get_4f_branch_scenarios(constants):
    """Ground state plus the three excited-state scenarios pinned for the 4f
    branch functions (Pff/Qfe/Rff/Pee/Qef/Ree): 4f-to-4f, 4f-to-A(2Pi_1/2), and
    4f-to-A(2Pi_3/2). Shared between compute_grid and branch_frequency_tables.py
    so both use the same definitions.
    """
    ground_4f = constants.State_4f_7212['v0'][174]
    scenarios = (
        ('4f7212->4f7232', 'branches_4f', constants.State_4f_7232['v0'][174]),
        ('4f7212->APi12', 'branches_4f_APi12', constants.A_Pi_12['v0'][174]),
        ('4f7212->APi32', 'branches_4f_APi32', constants.A_Pi_32['v0'][174]),
    )
    return ground_4f, scenarios


def compute_grid(constants, levels, transitions):
    """Return {label: [values]} for every pinned quantity. Units as noted per label."""
    out = {}

    # --- X(2Sigma+) hyperfine levels, GHz (params converted from the cm^-1 store) ---
    for vib in X_VIBS:
        for iso in sorted(constants.X_State[vib]):
            gp = constants.get_converted_ground_params(vib, iso)
            for n in N_GRID:
                _rec(out, f'X_hyperfine|{vib}|{iso}|N={n}|GHz', levels.X_hyperfine(n, **gp))
                _rec(out, f'F1|{vib}|{iso}|N={n}|GHz', levels.F1(n, **gp))
                _rec(out, f'F2|{vib}|{iso}|N={n}|GHz', levels.F2(n, **gp))
                _rec(out, f'eRotor|{vib}|{iso}|N={n}|GHz',
                     levels.eRotor(n, gp['Bpp'], gp['dpp']))
                _rec(out, f'gamma_spinRot|{vib}|{iso}|N={n}|GHz',
                     levels.gamma_spinRot(n, gp['g0'], gp['g1'], gp['g2']))

    # --- excited states, cm^-1 (stored units, no conversion) ---
    for iso in sorted(constants.A_Pi_12['v0']):
        pr = constants.A_Pi_12['v0'][iso]
        for j in J_GRID:
            _rec(out, f'Ue_APi12|v0|{iso}|J={j}|cm-1', levels.Ue(j, **pr))
            _rec(out, f'Uf_APi12|v0|{iso}|J={j}|cm-1', levels.Uf(j, **pr))
    for iso in sorted(constants.A_Pi_32['v0']):
        pr = constants.A_Pi_32['v0'][iso]
        for j in J_GRID:
            if j < 1.5:
                continue
            _rec(out, f'Ue_APi32|v0|{iso}|J={j}|cm-1', levels.Ue3JCP(j, **pr))
            _rec(out, f'Uf_APi32|v0|{iso}|J={j}|cm-1', levels.Uf3JCP(j, **pr))
    for name in ('State_4f_7212', 'State_4f_7232'):
        store = getattr(constants, name)
        for iso in sorted(store['v0']):
            pr = store['v0'][iso]
            for j in J_GRID:
                _rec(out, f'Ue4f_{name}|v0|{iso}|J={j}|cm-1', levels.Ue4f(j, **pr))
                _rec(out, f'Uf4f_{name}|v0|{iso}|J={j}|cm-1', levels.Uf4f(j, **pr))

    # --- X -> A(2Pi_1/2) branch transition energies, cm^-1 ---
    ground = constants.X_State['v0'][174]
    excited = constants.A_Pi_12['v0'][174]
    for branch in ('O_X', 'P_X', 'Q_X', 'R_X'):
        fn = getattr(transitions, branch)
        for n in (1, 2, 4, 7):
            try:
                _rec(out, f'{branch}|X_v0_174->APi12_v0_174|N={n}|cm-1',
                     fn(n, excited, ground))
            except Exception as exc:                      # pragma: no cover
                out[f'{branch}|N={n}|ERROR'] = [repr(exc)]

    # --- 4f-ground J-good branch transitions (Pff/Qfe/Rff/Pee/Qef/Ree), cm^-1 ---
    ground_4f, scenarios = get_4f_branch_scenarios(constants)
    for scenario_name, dict_name, excited_4f in scenarios:
        branch_dict = getattr(transitions, dict_name)
        for branch_name, (func, j_prime_calc, e_func, g_func) in branch_dict.items():
            for j in J_GRID:
                j_prime = j_prime_calc(j)
                if j < 0.5 or j_prime < 0.5:
                    continue
                try:
                    value = func(j, excited_params=excited_4f, ground_params=ground_4f,
                                 e_func=e_func, g_func=g_func)
                    _rec(out, f'{branch_name}|{scenario_name}|J={j}|cm-1', value)
                except Exception as exc:                  # pragma: no cover
                    out[f'{branch_name}|{scenario_name}|J={j}|ERROR'] = [repr(exc)]
    return out
