#!/usr/bin/env python3
"""Analytic multilayer model of the MS-1L shielding factor S(f).

Single-shell physics: a thin spherical shell of permeability mu_r,
conductivity sigma, mean radius a and thickness t transmits an axial AC
field with the classic complex factor

    S1(f) = | cosh(kt) + (1/3) (ka/mu_r + 2 (mu_r-1)^2/(mu_r ka)) sinh(kt) |,
    k = (1+j)/delta,  delta = skin depth in the wall.

Its f->0 limit is the exact thin-shell magnetostatic factor
1 + 2 (mu_r-1)^2 t / (3 mu_r a)  (~ 1 + 2 mu_r t/3a for mu_r >> 1); for
mu_r = 1 it reduces to the pure eddy-current shell |1 + j omega tau|. Four shells are
combined with the standard coupling product
S_tot = S_N * prod S_i (1 - (a_i/a_i+1)^3), applied to the per-shell
magnitudes (an engineering approximation above the corner).

The MS-1L wall thickness is not published, so t, mu_r and the layer gap
are swept over documented ranges. Outputs: the S(f) prediction band
over the sweep's frequency plan, the wall thickness the S_DC > 1e6 spec
implies, and the model-level link between the S(f) eddy corner and the
magnetization-noise knee f_magn of PREDICTION.md.
Writes shield_model_summary.json and fig_shield_model.png.
Self-checks: --test.
"""

from __future__ import annotations

import argparse
import json
import os
import sys
from datetime import datetime

import numpy as np

MU0 = 4e-7 * np.pi
SIGMA = 1.6e6           # mu-metal conductivity [S/m] (as in PREDICTION.md)
SPEC_S_DC = 1e6         # Twinleaf spec
N_LAYERS = 4
A_INNER = 0.075         # innermost shell mean radius [m] (bore ~14 cm dia)
LENGTH = 0.16           # bore length [m]; the sphere model ignores it -- the
                        # cylinder DC formulas below bracket the anisotropy

# sweep ranges (the unknowns)
T_MM = (0.5, 1.0, 1.5)          # wall thickness per layer
MU_R = (1.5e4, 2.5e4, 4.0e4)    # small-signal permeability
GAP_M = (0.008, 0.012, 0.016)   # radial gap between shells

F_MAGN_1MM_HZ = 0.38    # magnetization knee at t=1 mm, mu_r=2.5e4,
                        # tan(delta)=0.04 (PREDICTION.md section 1)

# the sweep's frequency plan
PLAN_HZ = (0.06, 0.13, 0.31, 0.71, 1.7, 3.7, 7.3, 11.7, 17.3, 23.7, 37.0, 73.0)

HERE = os.path.dirname(os.path.abspath(__file__))
C_BAND, C_MED, C_SPEC, C_MUTE, C_INK = ("#D7E3EA", "#2E6E8E", "#C0504D",
                                        "#7A7F85", "#33373B")


def require(ok, msg, code=13):
    if not ok:
        print(f"[error] {msg}", file=sys.stderr)
        raise SystemExit(code)


# ---------------------------------------------------------------- physics

def shell_S(f, a, t, mu_r, sigma=SIGMA):
    """Complex-magnitude shielding factor of one thin spherical shell."""
    f = np.asarray(f, dtype=float)
    out = np.empty_like(f)
    mag = 2.0 * (mu_r - 1.0) ** 2 / (3.0 * mu_r)   # magnetostatic strength
    zero = f <= 0
    out[zero] = 1.0 + mag * t / a
    fz = f[~zero]
    delta = np.sqrt(2.0 / (2.0 * np.pi * fz * MU0 * mu_r * sigma))
    k = (1.0 + 1.0j) / delta
    S = np.cosh(k * t) + (k * a / (3.0 * mu_r) + mag / (k * a)) * np.sinh(k * t)
    out[~zero] = np.abs(S)
    return out


def stack_S(f, radii, t, mu_r, sigma=SIGMA, p=3):
    """Multilayer coupling: S_tot = S_N * prod_i S_i (1 - (a_i/a_i+1)^p),
    shells ordered inner to outer; p=3 spherical. Applied to per-shell
    magnitudes -- exact at DC, an approximation once eddy phases matter."""
    radii = np.asarray(radii, dtype=float)
    S = np.array([shell_S(f, a, t, mu_r, sigma) for a in radii])
    total = S[-1].copy()
    for i in range(len(radii) - 1):
        total = total * S[i] * (1.0 - (radii[i] / radii[i + 1]) ** p)
    return total


def cylinder_dc(radii, t, mu_r):
    """DC brackets for the real (cylindrical) geometry: transverse uses
    S_i = 1 + mu_r t/(2a) with p=2 coupling; the axial factor of a closed
    cylinder of this aspect ratio sits between transverse and sphere."""
    s_t = np.array([1.0 + mu_r * t / (2.0 * a) for a in radii])
    total = s_t[-1]
    for i in range(len(radii) - 1):
        total = total * s_t[i] * (1.0 - (radii[i] / radii[i + 1]) ** 2)
    return float(total)


def sweep_params():
    for t_mm in T_MM:
        for mu_r in MU_R:
            for gap in GAP_M:
                radii = A_INNER + gap * np.arange(N_LAYERS)
                yield {"t_mm": t_mm, "mu_r": mu_r, "gap_m": gap,
                       "radii": radii}


def fit_corner(f, S):
    """Corner of the shield_sf guide S_dc*sqrt(1+(f/fc)^2), fitted on the
    same 0.01-20 Hz range the tool uses."""
    m = (f >= 0.01) & (f <= 20.0)
    s_dc = float(S[m][0])
    from scipy.optimize import curve_fit

    def guide(ff, fc):
        return np.log(s_dc * np.sqrt(1.0 + (ff / fc) ** 2))

    popt, _ = curve_fit(guide, f[m], np.log(S[m]), p0=[5.0],
                        bounds=(0.01, 1e3), maxfev=20000)
    return float(popt[0])


def min_t_for_spec(mu_r, gap, spec=SPEC_S_DC):
    """Smallest wall thickness whose spherical S_DC meets the spec."""
    radii = A_INNER + gap * np.arange(N_LAYERS)
    for t_mm in np.arange(0.05, 3.01, 0.05):
        if stack_S(np.array([0.0]), radii, t_mm * 1e-3, mu_r)[0] >= spec:
            return float(t_mm)
    return None


# ---------------------------------------------------------------- main

def run_model(outdir, no_figures=False):
    f = np.logspace(np.log10(0.01), np.log10(100.0), 300)
    curves, rows = [], []
    for p in sweep_params():
        S = stack_S(f, p["radii"], p["t_mm"] * 1e-3, p["mu_r"])
        s_dc = float(S[0])
        fc = fit_corner(f, S)
        f_magn = F_MAGN_1MM_HZ * (1.0 / p["t_mm"]) ** 2 * (2.5e4 / p["mu_r"])
        curves.append(S)
        rows.append({"t_mm": p["t_mm"], "mu_r": p["mu_r"],
                     "gap_mm": p["gap_m"] * 1e3,
                     "S_dc_sphere": s_dc,
                     "S_dc_cyl_transverse": cylinder_dc(
                         p["radii"], p["t_mm"] * 1e-3, p["mu_r"]),
                     "fc_hz": fc, "f_magn_hz": f_magn,
                     "fc_over_fmagn": fc / f_magn,
                     "meets_spec": s_dc >= SPEC_S_DC})
    curves = np.array(curves)
    lo, med, hi = (np.min(curves, axis=0), np.median(curves, axis=0),
                   np.max(curves, axis=0))

    print(f"=== shield model: {N_LAYERS} spherical shells, a1={A_INNER} m, "
          f"sweep t={T_MM} mm x mu_r={MU_R} x gap={GAP_M} m ===")
    print(f"[dc  ] S_DC sphere: {min(r['S_dc_sphere'] for r in rows):.2e} "
          f"- {max(r['S_dc_sphere'] for r in rows):.2e}   "
          f"(cyl transverse {min(r['S_dc_cyl_transverse'] for r in rows):.2e}"
          f" - {max(r['S_dc_cyl_transverse'] for r in rows):.2e})")
    n_meet = sum(r["meets_spec"] for r in rows)
    print(f"[spec] S_DC > 1e6 met by {n_meet}/{len(rows)} parameter sets")
    tmins = {}
    for mu_r in MU_R:
        for gap in GAP_M:
            tmins[f"mu{mu_r:g}_gap{gap * 1e3:g}mm"] = min_t_for_spec(mu_r, gap)
    tvals = [v for v in tmins.values() if v]
    print(f"[t   ] spec implies wall t >= {min(tvals):.2f}-{max(tvals):.2f} mm"
          " depending on mu_r and gap")
    fcs = [r["fc_hz"] for r in rows]
    ratios = [r["fc_over_fmagn"] for r in rows]
    print(f"[ac  ] eddy corner fc = {min(fcs):.1f}-{max(fcs):.1f} Hz  "
          f"(guide-fit); fc/f_magn = {min(ratios):.0f}-{max(ratios):.0f} "
          "(t-independent link to the noise knee)")

    plan = []
    for f0 in PLAN_HZ:
        j = int(np.argmin(np.abs(f - f0)))
        plan.append({"f_hz": f0, "S_lo": float(lo[j]),
                     "S_med": float(med[j]), "S_hi": float(hi[j])})
        print(f"[plan] {f0:>6.2f} Hz  S = {med[j]:.2e}  "
              f"({lo[j]:.1e} - {hi[j]:.1e})")

    os.makedirs(outdir, exist_ok=True)
    summary = {
        "program": "predict_shield_sf.py",
        "generated": datetime.now().isoformat(timespec="seconds"),
        "inputs": {"n_layers": N_LAYERS, "a_inner_m": A_INNER,
                   "sigma_s_per_m": SIGMA, "sweep":
                   {"t_mm": T_MM, "mu_r": MU_R, "gap_m": GAP_M}},
        "results": {"rows": rows, "plan_points": plan,
                    "t_min_for_spec_mm": tmins,
                    "fc_over_fmagn": [round(r, 1) for r in ratios]},
        "warnings": ["sphere model; the real cylinder is anisotropic",
                     "coupling product applied to magnitudes above the corner",
                     "coupling formula invalid for near-touching shells",
                     "openings and feedthroughs ignored"],
        "pass": None,
    }
    path = os.path.join(outdir, "shield_model_summary.json")
    with open(path, "w", newline="\n") as fh:
        json.dump(summary, fh, indent=1)
    print(f"[out ] {path}")

    if not no_figures:
        import matplotlib
        matplotlib.use("Agg")
        import matplotlib.pyplot as plt
        plt.rcParams.update({
            "figure.dpi": 150, "font.size": 11, "axes.titleweight": "bold",
            "axes.spines.top": False, "axes.spines.right": False,
            "axes.edgecolor": C_MUTE, "text.color": C_INK,
            "legend.frameon": False,
        })
        fig, ax = plt.subplots(figsize=(7.5, 4.8))
        ax.fill_between(f, lo, hi, color=C_BAND,
                        label="parameter sweep (t, mu_r, gap)")
        ax.loglog(f, med, color=C_MED, lw=1.4, label="median model")
        ax.axhline(SPEC_S_DC, color=C_SPEC, ls="--", lw=1.0)
        ax.annotate("spec S_DC = 1e6", (0.012, SPEC_S_DC), va="bottom",
                    fontsize=9, color=C_SPEC)
        for f0 in PLAN_HZ:
            ax.axvline(f0, color=C_MUTE, lw=0.5, alpha=0.4)
        ax.annotate("measurement plan frequencies", (0.15, lo.min() * 1.2),
                    fontsize=8, color=C_MUTE)
        ax.set_xlabel("frequency [Hz]")
        ax.set_ylabel("shielding factor S(f)")
        ax.set_title("MS-1L multilayer model: what the S(f) sweep should see")
        ax.legend(fontsize=9, loc="upper left")
        fig.savefig(os.path.join(outdir, "fig_shield_model.png"),
                    bbox_inches="tight")
        plt.close(fig)
        print(f"[out ] {os.path.join(outdir, 'fig_shield_model.png')}")
    return summary


# ---------------------------------------------------------------- checks

def test_dc_limit(_):
    a, t, mu_r = 0.08, 1e-3, 2.5e4
    got = shell_S(np.array([1e-9]), a, t, mu_r)[0]
    want = 1.0 + 2.0 * (mu_r - 1.0) ** 2 * t / (3.0 * mu_r * a)
    assert abs(got - want) / want < 1e-6, f"{got} vs {want}"
    # and the mu_r >> 1 shorthand agrees to 0.01%
    assert abs(want - (1.0 + 2 * mu_r * t / (3 * a))) / want < 1e-4


def test_eddy_limit(_):
    # mu_r = 1: the shell is a plain conductor, S = |1 + j omega tau|,
    # tau = mu0 sigma t a / 3
    a, t = 0.08, 1e-3
    f0 = 100.0
    tau = MU0 * SIGMA * t * a / 3.0
    got = shell_S(np.array([f0]), a, t, 1.0)[0]
    want = abs(1.0 + 2j * np.pi * f0 * tau)
    assert abs(got - want) / want < 1e-3, f"{got} vs {want}"


def test_coupling(_):
    # at realistic gaps two shells sit between one shell and the full
    # product, and wide separation approaches the product. (The coupling
    # formula is not trusted for near-touching shells -- stated limitation.)
    f = np.array([0.0])
    s_in = shell_S(f, 0.08, 1e-3, 2.5e4)[0]
    for outer in (0.09, 0.10, 0.12):
        s_out = shell_S(f, outer, 1e-3, 2.5e4)[0]
        both = stack_S(f, [0.08, outer], 1e-3, 2.5e4)[0]
        assert max(s_in, s_out) < both <= s_in * s_out * 1.001, (outer, both)
    far = stack_S(f, [0.08, 0.8], 1e-3, 2.5e4)[0]
    s_out = shell_S(f, 0.8, 1e-3, 2.5e4)[0]
    assert far >= 0.99 * s_in * s_out, "distant shells must nearly multiply"


def test_monotone_rise(_):
    f = np.logspace(-2, 2, 50)
    S = stack_S(f, A_INNER + 0.012 * np.arange(4), 1e-3, 2.5e4)
    assert np.all(np.diff(S) > -1e-9), "S(f) must not fall with f"
    assert S[-1] > 10 * S[0], "eddy rise missing at 100 Hz"


def run_self_test():
    print("=== predict_shield_sf --test: limit checks ===")
    try:
        for test in (test_dc_limit, test_eddy_limit, test_coupling,
                     test_monotone_rise):
            test(None)
            print(f"[test] PASS: {test.__name__}")
        return 0
    except AssertionError as e:
        print(f"[test] FAIL: {e}", file=sys.stderr)
        return 1


def main(argv=None):
    p = argparse.ArgumentParser(
        description=__doc__,
        formatter_class=argparse.RawDescriptionHelpFormatter)
    p.add_argument("--outdir", default=HERE,
                   help="where the summary and figure go")
    p.add_argument("--no-figures", action="store_true",
                   help="skip the figure")
    p.add_argument("--test", action="store_true", help="limit self-checks")
    args = p.parse_args(argv)
    if args.test:
        return run_self_test()
    run_model(args.outdir, args.no_figures)
    return 0


if __name__ == "__main__":
    sys.exit(main())
