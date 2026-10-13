# MS-1L multilayer shielding model — S(f) predicted before measurement

2026-07-08 (references updated 2026-08-10). Companion prediction to
`shield_sf_ac.py` (the way
`PREDICTION.md` is the companion to the noise measurement): an analytic
model of the 4-layer MS-1L shielding factor, with the unpublished wall
thickness treated as the unknown it is. Computation:
`predict_shield_sf.py` (this directory); numbers:
`shield_model_summary.json`; figure: `fig_shield_model.png`.

## 1. Model

One thin spherical shell (mean radius a, wall t, permeability mu_r,
conductivity sigma) transmits a uniform AC field with

    S1(f) = | cosh(kt) + ( ka/(3 mu_r) + 2(mu_r-1)^2/(3 mu_r ka) ) sinh(kt) |,
    k = (1+j)/delta,   delta = sqrt(2 / (2 pi f mu0 mu_r sigma)).

Verified limits (symbolic check in a computer-algebra kernel and in the
script's `--test`):

- f -> 0: S = 1 + 2 (mu_r-1)^2 t / (3 mu_r a) — the exact thin-shell
  magnetostatic factor (the familiar 1 + 2 mu_r t/3a for mu_r >> 1);
- mu_r = 1: S = |1 + j omega tau|, tau = mu0 sigma t a/3 — the pure
  eddy-current shell (plus an O(t/a) correction from the cosh term).

Four shells combine with the standard coupling product
S_tot = S_N prod S_i (1 - (a_i/a_{i+1})^3). Stated approximations: sphere
instead of the real cylinder (the cylinder's transverse DC values, with
p=2 coupling, run ~9x lower and bracket the anisotropy); magnitudes
multiplied above the corner where phases matter; no openings; the
coupling product is not trusted for near-touching shells.

Sweep over the unknowns: t = 0.5/1.0/1.5 mm, mu_r = 1.5/2.5/4.0 x 1e4,
layer gap = 8/12/16 mm, with a_1 = 75 mm (bore ~14 cm), sigma = 1.6e6 S/m.

## 2. What the model says

**DC.** S_DC (sphere) spans 1.7e5 - 1.7e9 over the sweep, median ~2e7;
the transverse-cylinder bracket runs 1.9e4 - 2.0e8. The spec S_DC > 1e6
is met by 24 of 27 parameter sets and, read backwards, **implies a wall
of at least 0.25-0.80 mm per layer** (depending on mu_r and gap). The
spec is thus consistent with an ordinary benchtop construction and gives
the first indirect handle on the unpublished wall thickness.

**AC rise.** Eddy currents raise S(f) once the skin depth approaches the
wall. Fitted to the same S_dc*sqrt(1+(f/fc)^2) guide the measurement
uses, the corner lands at **fc = 1.7-96 Hz** across the sweep -- inside
the 12-frequency measurement plan. The shape of the measured S(f) over
0.06-73 Hz therefore pins t (thick walls bend early, thin walls stay
flat to the top of the plan).

**Cross-check with the noise knee.** Both fc (eddy corner) and f_magn
(the magnetization-noise knee in `PREDICTION.md`) scale as
1/(mu_r sigma t^2), so their ratio is nearly parameter-free:
**fc / f_magn = 16-38** over the whole sweep. The S(f) fit and the
noise-spectrum fit are two independent measurements that must agree on
the same wall — if they do not, the model (or a hole/seam) is wrong.
This is the acceptance test's internal consistency check, free of
Twinleaf's data sheet.

## 3. Consequence for the S(f) measurement plan

With an interior detection limit of ~30 fT for a line (300 s record,
~20 fT/rtHz floor), a measured S saturates at about
S_max ~ B_applied / 30 fT. An external drive of 200 nT can only *detect*
S up to ~7e6 — below the model's median. So the plan should expect
**lower bounds at most frequencies** (shield_sf_ac.py handles them as
first-class results), and a drive of ~1 uT applied (still sub-pT inside)
is needed to convert the median prediction into detections. The
acceptance criterion "S_DC > 1e6" is decidable either way: a lower bound
above 1e6 passes the spec.

## 4. Reproduce

```bash
python analysis/shield_model/predict_shield_sf.py          # model + figure
python analysis/shield_model/predict_shield_sf.py --test   # limit checks
```
