# analysis_scripts/ — the IROP analysis, ported

These are the analysis scripts from `IROP_research/analysis/`, copied bucket
by bucket. **The only change is how they find the data.** Everything they
compute is unchanged: on the real data, the ported and original scripts print
the same numbers and write byte-identical figures (see "Verification" below).
The original Japanese notes are kept as `README_original_JP.md`, and the
per-bucket `.md` notes are copied alongside the scripts.

## Where the data comes from

Every script imports `_paths.py` first. `_paths.py` puts the B-field project
on `sys.path` and sets `DATA_ROOT`, the folder that holds the run buckets
(`gui_baseline/`, `synctest/`, ...). `DATA_ROOT` is set in this order:

1. the environment variable `BFIELD_DATA_ROOT`, if set;
2. `config/lab.local.toml` `[paths] data_root` (per machine, git-ignored);
3. `config/lab.toml` `[paths] data_root`, which points at the Box folder
   `C:/Users/cafmot/Box/LatticeEDM/EDM chamber/Magnetic shield/Test data`
   (a copy of the old `IROP_research/result` tree plus the new runs).

If the data moves again, change `data_root`. Nothing in the scripts needs
to be edited.

Inside the scripts, inputs are still spelled the old way,
`"result/<bucket>/<run>/<file>"`, so they stay readable and greppable.
`rel_data()` / `data_join()` map the `result/` prefix onto `DATA_ROOT`.

## How to run a script

From `B-field/` (with `.venv` active):

```
python analysis_scripts/synctest/plot_asd_compare.py
python analysis_scripts/degaussrelax/plot_relax_trends.py <DATA_ROOT>/degaussrelax/rx_20260818_183900
python analysis_scripts/run_audit.py                   # audits every run under DATA_ROOT
python analysis_scripts/run_audit.py <DATA_ROOT>/ms1l_3_degaussed
python analysis_scripts/runs_compare/runs_compare.py --test   # tool self-tests
```

- **Where figures go:** each script writes its figures next to itself, e.g.
  `analysis_scripts/synctest/fig_*.png`. `*.png`, `*.svg` and `*.csv` are
  git-ignored by `LatticeEDMScripts/.gitignore`.
- **Paths for run-folder tools:** the tools that take a run folder as an
  argument need a full path, since they no longer run from inside the
  IROP tree. These are `runs_compare`, `cross_spectrum`, `calibration/*`,
  `map_residual`, `mot_suscept`, `shield_sf/*` and `degaussrelax/*`.
- **Unicode output:** in a pipe or a non-UTF-8 console, set
  `PYTHONIOENCODING=utf-8`. Otherwise `modetest*/plot_asd_mode_compare*.py`
  crash on the `′` in "A′" while printing. The same happens in the
  original scripts.
- **gnuplot:** `ms1l_3_degaussed/plot_degauss_conditions.py` draws its figure
  with gnuplot, which is not installed on the lab PC. It prints its numbers,
  then fails at the plotting step, exactly as the original does.

## Bucket → data

| analysis_scripts/ | reads DATA_ROOT/ | contents |
|---|---|---|
| `gui_baseline/` | `gui_baseline/` | `plot_results.py`: closed vs open shield, ASD and Allan |
| `failed_analog/` | `failed_analog/` | `plot_3axis_diagnostic.py` (the data is invalid; kept as a teaching example) |
| `synctest/` | `synctest/`, `gui_baseline/` | `analyze_sync5.py` (the full sync5 analysis), `plot_asd_compare.py`, `plot_gain_vs_frequency.py`, `ANALYSIS.md` |
| `modetest/`, `modetest2/` | `modetest/`, `modetest2/`, `gui_baseline/` | mode-sandwich ASD and settling plots; `manifest.csv` and `runs_compare_summary.json` are earlier outputs, kept as records |
| `twohead_sync/` | `twohead_sync01/`, `twohead_sync02/` | `plot_sync02.py` |
| `ms1l_1_predegauss/` | `ms1l_1_predegauss/`, `modetest/`, `synctest/` | `plot_asd_ms1l_vs_bench.py`, `relax_series.py` |
| `ms1l_2_degaussfail/` | `ms1l_2_degaussfail/`, `ms1l_1_predegauss/` | wide-window and GUI-vs-analog plots |
| `ms1l_3_degaussed/` | `ms1l_3_degaussed/`, `ms1l_2_degaussfail/` | `plot_relax_all.py` (the canonical relaxation table), plateau and degauss-condition plots, `find_sine.py` |
| `degaussrelax/` | one `degaussrelax/rx_<stamp>/`, given as an argument | evening, trends, field-zero and cal-factor plots; they read `degauss_relax_session.json` and each point's `qzfm_session.json` |
| `shield_sf/` | a `shield_sf/sw_<stamp>/` or `dc_<stamp>/`, given as an argument | `shield_sf_ac.py` (S(f) from an AC sweep), `shield_sf_dc.py`, `sweep_nulls.py` |
| `prediction_B2/`, `shield_model/` | `gui_baseline/`, `failed_analog/` (B2 only) | predictions written before the measurement, with their `.md` and `.json` records |
| `calibration/`, `cross_spectrum/`, `map_residual/`, `mot_suscept/`, `runs_compare/` | run folders given as arguments | general tools; each has a `--test` self-test |
| `run_audit.py` | the whole tree (default `DATA_ROOT`) | lists quantities that should be zero: rails, non-finite samples, pinned outputs, exit codes, ADC codes |

Changes beyond the data path:

- **Calibration ledger:** the default ledger is now
  `DATA_ROOT/calibration/ledger.jsonl`, and run references in it are
  relative to `DATA_ROOT`. They used to be relative to the IROP repo, i.e.
  `result/...`. No ledger existed yet.
- **`synctest/analyze_sync5.py`:** this script had the author's Mac paths
  (`/Users/haruyaosawa/...`) hard-coded inside most of its `run_*` functions,
  so it could not run on the lab PC. These paths now point at `DATA_ROOT`
  (inputs) and the script's own folder (outputs).
- **`ms1l_2_degaussfail/plot_gui_vs_analog_0727.py`:** this one only worked
  when run from the IROP repo root. It now resolves its inputs through
  `rel_data()` and writes its figure next to itself.
- **`prediction_B2/rehearsal_b2.py`:** it now finds
  `runs_compare/runs_compare.py` inside `analysis_scripts/`.

## Shared helpers and their variants

`bfield/analysis/spectra.py` holds the majority version of every helper that
the scripts each carried a private copy of. A script imports from it **only
where its own copy was AST-identical** (docstrings aside); 21 scripts now do.
Every other copy stays in its script unchanged, so no number moved. New code
should import from `bfield.analysis.spectra`.

| helper | version | used by | how it differs |
|---|---|---|---|
| `spliced_asd` | **majority** → `spectra.spliced_asd` | 13 scripts: degaussrelax evening/trends, modetest(2) compare, ms1l_1 vs_bench, ms1l_2 widewindow, ms1l_3 n8_vs_n16/plateau_4days/postdg0729/relax0729_series/relax_all, shield_sf/sweep_nulls, synctest/plot_asd_compare | 200 s / 20 s Welch, spliced at 1 Hz |
| | capped → `spectra.spliced_asd_capped` | ms1l_1_predegauss/relax_series.py (report figure) | long segment capped at half the record length; identical on a 900 s record |
| | via `welch_asd` (local) | runs_compare, synctest/analyze_sync5 | calls `welch_asd(x, fs, lowseg*fs)`; numerically the same as the majority, kept local because the code differs |
| `band_median(f, a, lo, hi)` | **majority** → `spectra.band_median` | modetest(2) compare, ms1l_1 vs_bench, ms1l_2 widewindow, synctest/plot_asd_compare | `float(median)`; an empty band gives nan plus a warning |
| | nan-quiet → `spectra.band_median_or_nan` | ms1l_1_predegauss/relax_series.py (report figure) | returns nan without calling median on an empty band |
| | local | ms1l_3 n8_vs_n16, plateau_4days | returns `np.float64`, not `float` (same value) |
| | local, `band_median_required` in spectra | calibration/cal_daq_floor.py | refuses an empty band through the script's `require` |
| | local | degaussrelax/plot_relax_trends.py | different signature: median over f0/1.25 to f0·1.25, falls back to the nearest bin, returns fT (×1000) |
| `welch_asd` | **majority** → `spectra.welch_asd` | cal_daq_floor, mot_suscept, rehearsal_b2, runs_compare, analyze_sync5 | `nperseg` cast to int |
| | local, `welch_asd_checked` in spectra | gui_baseline, failed_analog, predict_ms1l_noise_budget (report figure) | no int cast; refuses a non-finite ASD through the script's `require` |
| `block_stats` | local, majority → `spectra.block_stats` | cal_daq_floor, cal_flip, runs_compare | returns (mean, sem, slope, slope σ, n); lstsq with covariance |
| | local | map_residual | `polyfit`, returns (mean, sem, slope) only |
| | local | mot_suscept | block length capped at record/4; returns (mean, sem, slope) |
| `load_raw_csv` | **majority** → `spectra.load_raw_csv` | cal_daq_floor, cal_tone, cross_spectrum | opens with utf-8 |
| | local | shield_sf/shield_sf_ac.py | platform default encoding; `removeprefix` (same result) |
| `find_meta` | one version, kept local (uses the script's `require`) → `spectra.find_meta` | 7 tools | — |
| `load_pT` | one version, kept local (uses `require`) → `spectra.load_pT` | gui_baseline, failed_analog, predict_ms1l_noise_budget | — |

`spectra.py` raises `AnalysisError` (a `ValueError`), not the scripts'
`require` exception. That is why copies that call `require` stay local even
when their text matches.

`bfield/io/runs.py` provides read-only loaders for new code: `find_runs`,
`load_run`, `read_channel` / `read_channel_header` (the parsed
`*_pT.txt` / `*_V.txt` header), `channel_files`, `read_meta` and `data_root()`.

## Fields the scripts rely on in the recorder output

When the recorder changes, check these first:

- **`meta.json`:**
  - `status == "ok"` and `exit_code == 0` (otherwise the tool refuses
    without `--force`), plus `fs_hz`, `fs_effective_hz`, `cal_v_per_nT_used`,
    `cal_factors`, `rail_fraction`, `qzfm_rail_fraction`,
    `pinned_block_fraction`, `n_nonfinite`, `adc_unique_codes`,
    `warnings`, `duration_s`, `pairs[].axis/blockmean_r`, `axes`, `heads`,
    `head_ids`, `gradients`, `start_iso`.
  - `source == "simulate"` makes runs_compare / cross_spectrum /
    map_residual / mot_suscept / shield_sf_ac / cal_* refuse the run
    without `--allow-sim`.
  - `cross_spectrum` (no `--pair`) requires `"qzfm_3axis.py"` in
    {`software`, `software_legacy`}; `map_residual` requires
    `"qzfm_gradient.py"` there.
  - Nothing reads `common_version`.
- **degaussrelax plots:**
  - `rx_<stamp>/degauss_relax_session.json`: `t0` (ISO), and
    `points[]` with `dir_name`, `t_actual_min`, `t_sched_min`,
    `woke_from_unpowered` and `exit_code`.
  - Each point's `qzfm_session.json`: `runs[0].t0` (ISO).
  - `p*/R*/house_{a,b}.csv` with columns `B0_pT`, `By_pT`, `Bz_pT`.
  - The first `p*/*/*.meta.json` per point, and its
    `<session>_<AXIS>_pT.txt` files.
- **shield_sf_dc:** `dc_<stamp>/sf_dc_session.json`.

## Verification (2026-09-24)

- **`run_audit`:** the old and new versions give identical output on all
  201 runs, with and without `--all`.
- **Scripts on the real data:** 28 scripts across every data bucket print the
  same output as the originals (run from a scratch copy of the old tree),
  apart from file paths. The 47 figures written in both runs are
  byte-identical, and `plot_degauss_conditions`'s gnuplot `.dat`
  intermediates are identical too.
- **`analyze_sync5.py`:** the old version cannot run here (Mac paths). The
  ported one runs in full, and its regenerated `models_summary.json`
  matches the original's 43 numbers exactly.
- **Self-tests:** all 12 tool `--test` suites pass.
- **pytest:** `tests/test_analysis_*.py` passes.
