# B2 decision tree — synthetic dress rehearsal (before MS-1L data)

2026-07-08. Companion to `PREDICTION.md`: before the first MS-1L record
exists, the section-4 decision tree was exercised end-to-end on synthetic
data. Harness: `rehearsal_b2.py` (same directory); results:
`rehearsal/rehearsal_summary.json` and three figures.

## What was tested

For each scenario (E leakage / M magnetization / S sensor), 24 records
per record length {0.5, 1, 2, 4 h} were generated with the PREDICTION.md
section-2 model, with the nuisance parameters drawn from their documented
ranges every time (f_magn 0.17-1.5 Hz, S_bench 3e3-1e5, S_MS1L 3e5-1e6,
sensor floor 7-15 fT/rtHz) plus the standard contamination (DC offset,
50 Hz line). Each record went through the same measurement the real
chain makes (0.05 Hz level from >= 400 s Welch segments, 3-10 Hz white
floor, white+1/f slope fit) and the section-4 bands applied mechanically:
S above 400 fT/rtHz, E/M below 95, mixed between. Separately: three
contamination cases, and one full pass of real 2 h records through
`runs_compare.py` (files, gate and all).

## Results

| true scenario | 0.5 h | 1 h | 2 h | 4 h |
|---|---|---|---|---|
| E (leakage) | 92% | 79% | 92% | 88% |
| M (magnetization) | 100% | 100% | 100% | 100% |
| S (sensor 1/f) | 100% | 100% | 100% | 100% |

1. **The headline branch works, fast.** "S or not" is decided correctly
   in every trial already at 30 minutes — PREDICTION.md section 6's
   minimum-duration claim holds. The first half-hour record inside the
   MS-1L is genuinely decisive.

2. **E's misses are structural, not statistical.** The E accuracy does
   not improve with record length: the ~10-20% of E draws with a strong
   leak (high S_bench/S_MS1L ratio) land above the 95 fT/rtHz edge and
   the tree calls them "mixed". Since the follow-up for "mixed" and for
   "E confirmed" is the same (cross-spectrum + degauss pair), the fix is
   wording, not measurement: read the 100-300 band as "leak or mixed",
   not as a distinct state.

3. **The slope cannot separate E from M (negative result, quantified).**
   The one handle a single sensor has on E vs M is the excess slope
   (leak carries the bench f^-0.93, magnetization is f^-0.5). It fails:
   fitted alpha medians are 0.53 (E) vs 0.50 (M), and only 4-8 of 24 E
   trials cross an alpha=0.7 split, at any length up to 4 h. The reason
   is physical -- at MS-1L levels the leak and the shield's own
   magnetization noise are comparable, and the fit follows the mixture.
   The tree's "one sensor cannot separate E from M" is confirmed with
   numbers; do not spend lab hours trying.

4. **Stationarity is the one thing that breaks the tree.** A 20 pT step
   in mid-record (a door, a moved instrument) pushed the 0.05 Hz level
   to ~1200 fT/rtHz and turned every M trial into a false "S" (0/8
   survive). By contrast, an untrimmed settling transient is harmless
   (8/8 survive; the tau ~ 185 s transient's energy sits below 0.05 Hz).
   Practical rule for the lab: log events during the record, and split
   the record in half before classifying — if the two halves disagree
   at 0.05 Hz by more than the Welch error, do not classify; re-record.

5. **The white-floor branch behaves.** A quieter-than-spec look-alike
   (sensor + shield noise both low) keeps the 0.05 Hz classification
   intact and is flagged by the white branch as "gain-slip suspect",
   which is exactly the section-4 instruction (calibrate first).

6. **The production chain digests MS-1L-style data unchanged.** Three
   2 h records (one per scenario) written as real run directories went
   through `runs_compare.py` with no code changes: floors 18-21 fT/rtHz,
   0.05 Hz levels 73 / 82 / 614 fT/rtHz, classes as expected.

## What this changes in the lab plan

- The first MS-1L look can be 30 min; spend the long stationary hours
  on the record that carries the error bar (>= 2 h, +/-8%), not on the
  classification.
- Add the split-half stationarity check to the acceptance routine before
  quoting any 0.05 Hz number.
- E vs M separation stays with the two-head cross-spectrum and the
  degauss before/after pair; the slope is not a shortcut.

## Reproduce

```bash
python analysis/prediction_B2/rehearsal_b2.py            # ~10 min
python analysis/prediction_B2/rehearsal_b2.py --quick    # smoke
python analysis/prediction_B2/rehearsal_b2.py --test     # self-checks
```
