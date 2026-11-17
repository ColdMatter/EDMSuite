"""Zeeman levels of 174YbF: X2Sigma+ (one rotational level N) and A2Pi1/2 J' = 1/2.

Standalone helpers: NOT used by levels, transitions or spectra. Energies in MHz, field in gauss, field along z.
Energies are relative to the zero-field centroid of the manifold (the rotational energy is left out).

X(v, N):  H = gamma(N) N.S + b I.S + c I_z S_z + C I.N                    (constants.X_State, as levels.X_hyperfine)
             + gS muB S.B + gl muB [S.B - (S.n)(n.B)] - gI muN I.B        (constants.X_zeeman, constants.gI_F19)
          diagonalised exactly within N (the closed-form X_hyperfine levels are reproduced at B = 0).
A J'=1/2: H = eps I.J + gJ muB J.B - gI muN I.B,                           eps from levels.A12_hfs_split (Sauer 1999),
          gJ = (2/3)(gL' - gS/2 +/- gl')  (+ for e, - for f)                (constants.A_Pi_12_zeeman, Ma 2009 fit B).
          Only the J' = 1/2 level is in the basis: mixing with J' = 3/2 (4.4 GHz away) shifts levels by > 0.1 MHz
          above roughly 60 G (ChillTheBeams full-basis comparison).

Typical use
-----------
    from ybf import zeeman
    import numpy as np
    B = np.linspace(0, 50, 201)
    X = zeeman.X_zeeman_levels(B)               # list of dicts: label, F, mF, B, E (E in MHz, one value per field)
    A = zeeman.A12_zeeman_levels(B, 'e')
    zeeman.low_field_gF(zeeman.X_zeeman_levels([0, 1e-3]))
    # {'F=1l': 0.071, 'F=0': 0.0, 'F=2': 0.500, 'F=1h': 0.429}
"""

import numpy as np

from . import constants, levels

MU_B_MHZ_PER_G = 1.39962449361          # Bohr magneton / h, MHz/G (CODATA 2018)
MU_N_MHZ_PER_G = MU_B_MHZ_PER_G / 1836.15267343   # nuclear magneton / h, MHz/G


def _jm(j):
    """Angular momentum matrices [Jx, Jy, Jz] for spin j, basis m = j ... -j."""
    m = np.arange(j, -j - 1, -1)
    d = len(m)
    jp = np.zeros((d, d))
    for k in range(1, d):
        jp[k - 1, k] = np.sqrt(j * (j + 1) - m[k] * (m[k] + 1))
    return [(jp + jp.T) / 2, (jp - jp.T) / 2j, np.diag(m).astype(complex)]


def _dot(A, B):
    return sum(a @ b for a, b in zip(A, B))


def X_zeeman_matrices(n, vib_state="v0", iso=174, gS=None, gl=None, gI=None):
    '''
    Matrices of the X(v, N = n) hyperfine + Zeeman Hamiltonian in the product basis |N mN>|S mS>|I mI>.

    Returns
    -------
    H0 : (d, d) array, MHz        field-free hyperfine Hamiltonian (centroid at zero)
    Hz : (d, d) array, MHz/G      Zeeman operator per gauss, field along z
    Fz, F2 : (d, d) arrays        F_z and F^2 (to label states)
    '''
    p = constants.get_converted_ground_params(vib_state, iso)          # GHz
    zx = constants.X_zeeman
    gS = zx["gS"] if gS is None else gS
    gl = zx["gl"] if gl is None else gl
    gI = constants.gI_F19 if gI is None else gI

    e2, eN = np.eye(2), np.eye(2 * n + 1)
    N = [np.kron(np.kron(a, e2), e2) for a in _jm(n)]
    S = [np.kron(np.kron(eN, a), e2) for a in _jm(0.5)]
    I = [np.kron(np.kron(eN, e2), a) for a in _jm(0.5)]
    NS, IS, NI = _dot(N, S), _dot(I, S), _dot(N, I)

    gam = levels.gamma_spinRot(n, **p) * 1e3
    b, c, C = p["Xb"] * 1e3, p["Xc"] * 1e3, p["XC"] * 1e3
    # within one N: <n_i n_j> = d_ij/3 + k[(N_i N_j + N_j N_i)/2 - d_ij N(N+1)/3],  k = -2/((2N-1)(2N+3))
    k = 0.0 if n == 0 else -2 / ((2 * n - 1) * (2 * n + 3))
    dip = k * ((NI @ NS + NS @ NI) / 2 - n * (n + 1) / 3 * IS)          # sum_ij (n_i n_j - d_ij/3) I_i S_j
    H0 = gam * NS + b * IS + c * IS / 3 + c * dip + C * NI

    SnnB = S[2] / 3 + k * ((NS @ N[2] + N[2] @ NS) / 2 - n * (n + 1) / 3 * S[2])   # (S.n)(n.z)
    Hz = gS * MU_B_MHZ_PER_G * S[2] + gl * MU_B_MHZ_PER_G * (S[2] - SnnB) - gI * MU_N_MHZ_PER_G * I[2]

    F = [a + s + i for a, s, i in zip(N, S, I)]
    return H0, Hz, F[2], _dot(F, F)


def A12_gJ(parity, gS, gLp, glp, **kwargs):
    '''g_J of A2Pi1/2 J' = 1/2: (2/3)(gL' - gS/2 + gl') for e, (2/3)(gL' - gS/2 - gl') for f (Ma 2009 fit B convention).'''
    if parity not in ("e", "f"):
        raise ValueError("parity must be 'e' or 'f'")
    return (2 / 3) * (gLp - gS / 2 + (glp if parity == "e" else -glp))


def A12_zeeman_matrices(parity, vib_state="v0", iso=174, gI=None):
    '''Matrices of the A2Pi1/2 J' = 1/2 hyperfine + Zeeman Hamiltonian, basis |J mJ>|I mI>; as X_zeeman_matrices.'''
    hfs = constants.A_Pi_12_hfs[vib_state][iso]
    g = constants.A_Pi_12_zeeman[vib_state][iso]
    gI = constants.gI_F19 if gI is None else gI
    eps = levels.A12_hfs_split(0.5, parity, **hfs)        # E(F=1) - E(F=0); for J = I = 1/2 this is the I.J coefficient
    gJ = A12_gJ(parity, **g)
    J = [np.kron(a, np.eye(2)) for a in _jm(0.5)]
    I = [np.kron(np.eye(2), a) for a in _jm(0.5)]
    H0 = eps * _dot(I, J)
    Hz = gJ * MU_B_MHZ_PER_G * J[2] - gI * MU_N_MHZ_PER_G * I[2]
    F = [a + b for a, b in zip(J, I)]
    return H0, Hz, F[2], _dot(F, F)


def _curves(H0, Hz, Fz, F2, B, mixed_label):
    '''Adiabatic Zeeman curves, one per state: diagonalise each m_F block (m_F is exact for B along z), sort the
    eigenvalues at every field (no crossings within a block), and label each curve by its zero-field F.'''
    B = np.atleast_1d(np.asarray(B, dtype=float))
    mFs = np.round(np.real(np.diag(Fz)), 6)
    out = []
    for mF in sorted(set(mFs)):
        idx = np.where(mFs == mF)[0]
        h0, hz, f2 = H0[np.ix_(idx, idx)], Hz[np.ix_(idx, idx)], F2[np.ix_(idx, idx)]
        E = np.array([np.linalg.eigvalsh(h0 + b * hz) for b in B])
        # zero-field labels: eigenvectors of h0 with an infinitesimal field to fix the order
        w0, v0 = np.linalg.eigh(h0 + 1e-9 * hz)
        Fq = [int(round((-1 + np.sqrt(1 + 4 * np.real(v0[:, i].conj() @ f2 @ v0[:, i]))) / 2)) for i in range(len(w0))]
        for i in range(len(idx)):
            out.append(dict(label=mixed_label(Fq[i], w0[i], Fq), F=Fq[i], mF=float(mF), B=B, E=E[:, i]))
    return out


def X_zeeman_levels(B, n=1, vib_state="v0", iso=174, **g):
    '''
    Zeeman curves of X(v, N = n), MHz relative to the zero-field centroid.

    Returns a list of dicts with keys 'label' (e.g. 'F=1l', 'F=1h', 'F=2'; h/l = upper/lower of the two F = N levels),
    'F', 'mF' and 'E' (array over B). Keyword arguments gS, gl, gI override the stored g-factors.
    '''
    H0, Hz, Fz, F2 = X_zeeman_matrices(n, vib_state, iso, **g)
    w = np.linalg.eigvalsh(H0)
    FN = sorted({round(x, 6) for x, f in zip(w, _zero_field_F(H0, F2)) if f == n})

    def lab(F, e, Fs):
        if F == n and len(FN) == 2:
            return f"F={F}h" if e > np.mean(FN) else f"F={F}l"
        return f"F={F}"
    return _curves(H0, Hz, Fz, F2, B, lab)


def A12_zeeman_levels(B, parity="e", vib_state="v0", iso=174, gI=None):
    '''Zeeman curves of A2Pi1/2 J' = 1/2 (parity 'e' or 'f'), MHz relative to the zero-field centroid; as X_zeeman_levels.'''
    H0, Hz, Fz, F2 = A12_zeeman_matrices(parity, vib_state, iso, gI)
    return _curves(H0, Hz, Fz, F2, B, lambda F, e, Fs: f"F'={F}")


def _zero_field_F(H0, F2):
    w, v = np.linalg.eigh(H0)
    return [int(round((-1 + np.sqrt(1 + 4 * np.real(v[:, i].conj() @ F2 @ v[:, i]))) / 2)) for i in range(len(w))]


def low_field_gF(curves):
    '''
    Low-field g_F of each labelled level, E = g_F muB B m_F, from the first step of curves whose field grid starts at
    B = 0. Use a small first step (e.g. B = [0, 1e-3]) for an accurate value. Returns {label: g_F} (0 for F = 0).
    '''
    out = {}
    for c in curves:
        if c["mF"] == 0:
            out.setdefault(c["label"], [])
            continue
        if c["B"][0] != 0:
            raise ValueError("the field grid must start at B = 0")
        slope = (c["E"][1] - c["E"][0]) / (c["B"][1] - c["B"][0])
        out.setdefault(c["label"], []).append(slope / (MU_B_MHZ_PER_G * c["mF"]))
    return {k: (float(np.mean(v)) if v else 0.0) for k, v in out.items()}
