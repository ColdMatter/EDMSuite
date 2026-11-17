# 30-minute simultaneous run (sync5): analysis

2026-07-06, 17:09-17:39. One QZFM (Z3D0-AB3Q, dual-axis mode) inside the closed bench
shield, after the wiring repair. Z is digitized twice at the same time: the QuSpin GUI
digital stream (203.18 Hz) and the analog output through an NI USB-6008 (1200.06 Hz,
differential, +/-1 V). Y is recorded once, on the second analog channel. Unless stated, the
analog pT files are calibrated by x1.029 and the first 5 min (a Z settling transient) are
excluded from the noise statistics.

Every number below was computed from the raw files and then independently re-computed in a
separate check pass; this note keeps only what survived, states the qualified or refuted
items honestly, gives the physical picture, and ranks the follow-ups. All analyses are
consolidated in `analyze_sync5.py` (one `run_<topic>()` function each); all figures are in this folder.

---

## Calibration (settled)

- **Effective analog gain 2.624 V/nT** (nominal 2.7, i.e. -3%), from the 50 Hz mains line
  that both records see on the same Z axis. The pT files in `sync5_30min/` were written with
  the nominal gain, so multiply them by 1.029. The 1-min run (sync4) gave 2.61 — consistent.
- **GUI sample clock 203.18 Hz** (label says 200), transferred from the DAQ clock through the
  same 50 Hz line. Stopwatch gave 203.5 +/- 1 and the 06-30 elapsed-time estimate ~203.2.
- **The two records align without any timecode**: cross-correlating the shared Z signal gives
  the GUI start offset (+2.75 s here) to ~10 ms. No hardware trigger is needed to compare the
  two paths.
- After the 50 Hz calibration the two Z readouts agree in scale to 2.3% in the drift band. The
  absolute scale still rests on the factory (GUI) calibration until a coil calibration is done.
- **DC is not field.** analog Z -77.1, Y -1.3, GUI Z -70.1 pT (path offset +7 pT). Each axis
  carries an intrinsic sensor zero offset (nT class); only a 180-degree flip separates the
  offset from a real field.

---

## What the noise is made of

A coherent three-band picture of this closed-shield record.

- **White band (3-40 Hz), ~21 fT/rtHz analog / ~20 fT/rtHz GUI:** Gaussian, stationary,
  per-axis independent sensor noise. Not a common field (common component < ~7 fT), not shield
  Johnson noise (sits at/above the 6-16 fT mu-metal Johnson band), not DAQ-limited
  (quantization ~7 fT, ADC dithered at ~8.5x step RMS). Sensor-limited, consistent with the
  QZFM dual-axis regime. The floor is ~1.7x above the 06-30 baseline, as a pure amplitude
  scaling at unchanged noise colour.

- **1/f knee region (~1-5 Hz):** the smooth flicker (alpha ~ 0.9) term overtakes the white
  floor near 4-5 Hz. This is where the amplitude scaling from 06-30 shows up (A ~2x) with no
  change in spectral slope.

- **Low-frequency drift band (<0.05 Hz), rising as f^-0.93:** a single, fixed-direction
  (+29 deg from +Z), bounded, mean-reverting fluctuation common to Z and Y (coherence 0.96,
  r=0.92) and steeper than the f^-0.5 magnetization law — environmental field leakage / slow
  field drift through the bench shield, not shield magnetization noise. Superimposed is the
  one-off Z-only ~44 pT / tau ~185 s settling transient, a per-axis servo effect carrying 95%
  of the Z drift variance in the first 5 min.

- **Line forest (>100 Hz analog):** dominated by the QZFM cell-modulation carrier (~922.9 Hz
  effective) aliasing into the band as the 277/294 Hz doublet — an internal instrument
  artifact, not a field. The true mains 50 Hz line is modest and Z-preferential; a 59.917 Hz
  external comb and the 203 Hz GUI-clock leak are also present. None of this touches the
  3-40 Hz measurement band.

---

## Findings in detail

**Stationarity of the white floor.** After excluding the first 5 min, the 3-40 Hz white floor
of analog Z (90 s window, 30 s hop) is stationary: median **21.17 fT/rtHz**, relative std
**2.30%**, with a tiny monotone improvement (slope -0.040 fT/rtHz/min, -4.4% end-to-end). No
bursts, no field steps: sample-to-sample outliers above 6x the MAD sigma (3.48 pT) number 664
samples (0.031%), sign-balanced, largest jump 13.5 pT — heavy-tailed white/quantization noise,
not glitches. The first-5-min transient does not raise the broadband floor (ratio 1.006); it
is a DC/low-frequency excursion only.

**Settling transient (Z-only).** After Field Zero the analog Z field relaxes as a single
decaying exponential onto its baseline: amplitude **A ~= 43-44 pT**, time constant **tau ~=
180-190 s** (single-exp R^2 = 0.993; A and tau are coupled, so quote them together). A second
exponential is not supported. **Y shows no comparable transient** (38.9 pT p-p in Z vs 4.9 pT
in Y over the first 400 s). The first 5 min carries **95.0%** of the Z residual drift variance.
Recommended wait after Field Zero **>= 500 s**. The Z-only asymmetry is the signature: an
ambient field would appear on both axes, so this is a per-axis servo/zero re-equilibration.

**Allan / Hadamard stability.** All three channels share one stability curve: white noise
(tau^-0.5) down to a best-stability knee of **93 / 88 / 92 fT** (analog Z / Y / GUI Z) at tau
~= 0.06-0.08 s, then a clean random-walk rise (tau^+0.5) with no flicker floor. The 06-30
reference reaches 53 fT — this run is **~1.75x worse** at the knee. Hadamard sits far below
Allan at large tau (462 vs 1121 fT at tau = 130 s), confirming a dominant linear-drift
component.

**1/f model fit — amplitude scaling, not a colour change.** Fitting
asd(f) = sqrt(w^2 + (A/f^alpha)^2) over 0.005-40 Hz: all four channels are firmly flicker
(**alpha ~ 0.86-0.91**, nowhere near 0.5 diffusive or 2.0 random-walk). The 06-30 -> 07-06
degradation is broadband amplitude scaling: **A up ~1.97x, white floor up ~1.69x**. Analog Z
and GUI Z fit identically, so the DAQ path does not distort the low-frequency colour.

**Spectral line census.** The analog line forest is dominated **not by mains** but by two
instrument-internal cell-modulation alias lines: **277.1 Hz (868 fT/rtHz, ~49x floor)** and
**294.0 Hz (358 fT/rtHz, ~21x)**, both Z-Y coherence 0.99-1.00. The arithmetic checks:
277.1 = 1200.06 - 922.94 Hz effective carrier, 294.0 = 1200.06 - 906.1 Hz (carrier minus a
16.9 Hz sideband). The true mains 50 Hz line is modest (**337 fT/rtHz analog, 345 GUI**,
coherence 0.695), couples mainly to Z. A comb of an external **59.917 Hz** line pervades both
axes with ~0.9 coherence and is not a DAQ sub-harmonic. The 203 Hz GUI-clock line leaks into
the analog stream. No line clears 5x in the 0.3-3 Hz mechanical band.

**Amplitude distributions and the ADC.** In the 3-40 Hz band all three channels are Gaussian.
The full-band analog Z/Y heavy tails (excess kurtosis +1.05) live **entirely above 100 Hz** and
never reach the band-limited (~101 Hz Nyquist) GUI stream. The USB-6008 digitizes on an exact
**512.00 uV code grid**, implying a true differential full scale of **+/-1.0486 V**, not the
nominal +/-1 V. Sensor noise exceeds the quantization step by ~8.5x (heavily dithered); raw
ENOB ~8.9 bit, and 24x oversampling+decimation recovers near-ideal gain (~+2 bits).

**Drift geometry.** The slow (<0.05 Hz) drift is confined to a single fixed direction in the
Z-Y plane: PCA principal axis **+28.8 deg from +Z**, aspect 5.6, PC1 carrying **96.9%** of the
variance, r = +0.92. LS drift rate +0.59 (Z) / +0.34 (Y) pT/min. The process is
**anti-persistent / mean-reverting** (MSD Hurst ~0.2, saturating above ~100 s), not a free
random walk.

**Physical noise-model check.** The 0.05-2 Hz Z excess follows ASD ~ f^-0.93 (PSD ~ 1/f^1.9),
**decisively steeper than the f^-0.5 magnetization prediction** of Lee & Romalis (a
magnetization fit would demand an unphysical ~84 Hz knee). So the low-frequency rise is
environmental field leakage / slow field drift, not shield magnetization noise. The white
floor sits at/above the 6-16 fT mu-metal Johnson band, so it is sensor-limited. sync5 is ~2.4x
noisier than 06-30 at 0.05 Hz (1743 vs 713 fT/rtHz).

**Digital chain is bit-exact.** pT = (count-8388608)/100 reproduces the GUI Formatted file with
max|diff| = 0.000000 pT over all 367,809 samples.

---

## Limits and open questions

- **The path cross-spectrum is a <1 Hz check only.** analog-Z x GUI-Z coherence is ~1 below
  ~0.1 Hz and ~0.85 at 1 Hz, but at the 3-15 Hz floor it collapses to ~0.1-0.4: the ~22 ms
  residual GUI-clock jitter rotates the phase across the band. So the two-path comparison
  confirms chain agreement at low frequency but says nothing about the floor. (An earlier
  "0.78 recovery at the floor" was a mislabelled broadband alignment correlation, not a
  coherence — it does not hold.)
- **The per-axis vs common floor split is an upper limit, not a detection.** Every
  bias-correction puts the common floor well below 21 fT, but the excess coherence barely
  clears the finite-average bias. It establishes common < ~7 fT and cannot exclude a shared
  laser/temperature term at the 5-7 fT level.
- **"Noise colour unchanged 06-30 -> 07-06" is method-dependent.** Only the weighting-invariant
  statement is safe: both runs are firmly flicker, and the degradation is amplitude scaling.
  The exact alpha equality is not robust.
- **The 1.75x floor rise rests on n=2 runs** (06-30 vs sync5), which differ in warm-up, re-zero,
  degauss history and environment at once. It could be day-to-day scatter until repeated.
- **Field vs sensor is unresolvable with one sensor.** Geometry alone cannot separate a real
  external field from a common-mode sensor zero-drift. Every "external field" vs "sensor-common"
  statement (drift, common floor, transient) is an interpretation until a second sensor or a
  180-degree flip is available.
- **One 25-min realization** gives only ~7-8 independent cycles at 0.05 Hz, so the specific
  low-frequency exponents are summaries, not precise indices. Note the drift is best read as a
  *bounded, mean-reverting* fluctuation whose PSD is merely steeper than f^-0.5 (ruling out
  magnetization), not a genuine integrated random walk.

---

## Next measurements (ranked)

1. **Two-sensor cross-spectrum, same axis, same shield.** The single highest-value test. Two
   QZFMs (>6 cm apart) on analog into one USB-6008: the coherent part is real field, the
   incoherent part is per-sensor noise. Converts the ~7 fT common-floor *upper limit* into a
   *measurement* and resolves the field-vs-sensor ambiguity behind the drift, the floor split,
   the model check and the transient at once.
2. **Same-session repeatability series (>= 3 short runs) + a single-axis run.** Puts an error
   bar on the 21 fT floor, kills the n=2 confound behind the 1.75x claim, and settles the
   dual-vs-single-axis floor-penalty question.
3. **QZFM-off / input-short DAQ-only floor with the repaired wiring.** Directly measures the
   ~7 fT DAQ-chain floor (currently only inferred by quadrature). Trivial to run.
4. **Log the QZFM digital housekeeping (cell-temperature error, Field-Zero residuals) during a
   drift run.** Correlate against the 0.02-0.2 Hz Z-Y common drift and the tau ~185 s transient.
   If drift tracks cell temperature, the sensor-common branch is confirmed without a second
   sensor. Nearly free — the stream already carries it.
5. **Coil-calibration injection for the shielding transfer function S(f) and absolute gain.**
   A stepped/swept known field measures S(f) and replaces the factory-gain-dependent 2.624 V/nT
   with a traceable calibration, turning relative ASDs into absolute numbers against the
   50 fT/rtHz Lattice target.
6. **Higher-fs short capture (>= 2 kHz).** Confirms the 922.9 Hz cell-mod carrier + 906 Hz
   sideband (now only inferred from aliases) and sources the unlabeled 59.917 Hz comb.
7. **MOT off/on/switching susceptibility run.** Every result here is a MOT-off quiet baseline;
   the shield sits ~0.5 m from the MOT quadrupole — the decisive environmental test for the
   Lattice application.

---

## Figures (all in this folder)

Headline (from `analyze_sync5.py`, `run_headline`):

- `fig_sync5_asd.png` — amplitude spectra of the three channels vs the 06-30 reference,
  5 mHz-100 Hz.
- `fig_sync5_drift.png` — slow drift: Z from both readouts overlaid, Y tracking Z.
- `fig_sync5_coherence.png` — analog-Z x GUI-Z (timing wander) and Z x Y (common drift, 50 Hz
  check).
- `fig_sync5_allan.png` — Allan deviation of all three records.

Per-topic (from the `run_*` functions in `analyze_sync5.py`):

- `lines_census_spectrum.png` — annotated 0.2-600 Hz line spectrum with 50 Hz amplitude- and
  centre-frequency-vs-time panels.
- `nonstat_spectrogram_rolling.png` — spectrogram plus rolling 3-40 Hz floor and 0.1 Hz
  amplitude, showing the stationary noise field and the 5-min transient cut.
- `allan_hadamard_sync5.png` — Allan and Hadamard deviations for all three channels plus 06-30.
- `slope1f_asd_model_fits.png`, `slope1f_0630_vs_0706_GUIZ.png` — white+power-law fits and the
  direct 06-30 vs 07-06 GUI-Z overlay.
- `decomp_asd_decomposition.png` — two-axis and two-path ASD decompositions.
- `transient_first5min_Z_Y_expfit.png` — Z single-exp settling vs flat Y.
- `driftvec_hodograph.png` — Y-vs-Z drift hodograph with PCA axes, MSD and low-f PSD.
- `distrib_histograms_qq_quantization.png` — amplitude histograms, Q-Q, and the 512-uV code
  grid.
- `models_sync5_asd_models.png`, `models_sync5_excess_slope.png` — measured ASDs vs physical
  models, and the low-f excess slope vs f^-0.5 / f^-1.
- `guiclock_coherence_impact.png` — analog-Z x GUI-Z coherence, full record vs steady half.

Two secondary diagnostics are also written by the functions above but are not needed for the
results (`guiclock_lag_vs_time.png`, `transient_Z_fit_residuals.png`).
