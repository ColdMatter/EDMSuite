# analysis/ — 解析コードと図

`result/` が測定データ、ここがそれを読むコードと、コードが吐いた図・表・ノート。
**バケット名は `result/` と同じにする**（`analysis/synctest/` は `result/synctest/` を読む）。
図は `analysis/<バケット>/fig_*.png` に置き、**レポートはそれをそのまま引く**
（`report/main.tex` の `\graphicspath{{../analysis/}}`）。

## 1. データのバケットに対応する解析

| analysis/ | 読む result/ | 中身 |
|---|---|---|
| `gui_baseline/` | `gui_baseline/` | `plot_results.py`＋図 2（closed vs open の ASD・Allan） |
| `failed_analog/` | `failed_analog/` | `plot_3axis_diagnostic.py`＋図 3（データは無効・図は教材として保持） |
| `synctest/` | `synctest/` | `analyze_sync5.py`（解析一式）・`plot_asd_compare.py`・`plot_gain_vs_frequency.py`＋図 20＋統合ノート `ANALYSIS.md` |
| `modetest/` | `modetest/` | `plot_asd_mode_compare.py`・`plot_mode_settling.py`・`plot_mode_settling_combined.py`＋図 6＋`manifest.csv` |
| `modetest2/` | `modetest2/` | `plot_asd_mode_compare2.py`・`plot_mode_settling2.py`＋図 6＋`manifest.csv` |
| `twohead_sync/` | `twohead_sync01/`＋`twohead_sync02/` | `plot_sync02.py`＋図 5。**1 本のスクリプトが 2 セッション両方を読む**ので、バケット名は単数のまま |
| `ms1l_1_predegauss/` | `ms1l_1_predegauss/` | `plot_asd_ms1l_vs_bench.py`・`relax_series.py`＋図 2 |
| `ms1l_2_degaussfail/` | `ms1l_2_degaussfail/` | `plot_widewindow_0728.py`・`plot_gui_vs_analog_0727.py`・`plot_gui_vs_analog_0727_split.py`＋図 9 |
| `ms1l_3_degaussed/` | `ms1l_3_degaussed/`（＋比較のため `ms1l_2_degaussfail/` を 1 本） | `plot_relax_all.py`（正典）・`plot_plateau_points.py`・`plot_plateau_4days.py`・`plot_degauss_conditions.py`・`plot_n8_vs_n16_asd.py`・`plot_postdg0729.py`・`plot_postdg0817.py`・`plot_relax0729_series.py`・`find_sine.py`＋図 13 |
| `degaussrelax/` | `degaussrelax/` | `plot_relax_evening.py`（正典の表と重ね書き）・`plot_relax_trends.py`（キリの良い f と τ の折れ線＋Allan 偏差）・`plot_relax_fieldzero.py`（各 wake の Field Zero 値＝内部 DC 場）・`plot_relax_calfactor.py`（各 wake の Calibrate 係数）。いずれも `rx_<stamp>` を 1 個渡す |

**解析コードを持たないデータバケット**（意図的）:

- `result/coiltest/` — 両軸レールで定量不可（`result/README.md` の判断）。図にする値が無い。
- `result/degaussingtest/` — DG 側の記録。パネル画像と CSV は各ディレクトリの `README.md`／`NOTES.md` が読み解いており、Python は要らない。
  - 例外が 1 枚: `ms1l_3_degaussed/fig_dg_panel_5A_taper_orbit.png` は DG パネル由来だが、**この repo に生成スクリプトが無い**（手作業で作った図）。レポートが引いているので置き場はレポートの参照に従い `ms1l_3_degaussed/` のまま。

## 2. run ディレクトリを引数に取るツール（特定のバケットに属さない）

どれも「どのランでも読める」設計で、対応する `result/` バケットを持たない。
測定手順の設計書は `knowledgement/design/*_DESIGN.md`。

| ツール | 何をする |
|---|---|
| `run_audit.py` | **`result/` の全ランを横断**して、ゼロであるべき量（レール率・非有限数・pinned・exit code・ADC コード数）を並べる。チャネル単位で読む。exit は常に 0＝報告であって判定ではない |
| `runs_compare/runs_compare.py` | 複数ランの ASD・床・ドリフトを同じ土俵で比べる |
| `cross_spectrum/cross_spectrum.py` | 2 台のクロススペクトル（センサー床より下を見る手） |
| `calibration/cal_{daq_floor,flip,stability,tone}.py` | 較正 4 種（DAQ 床・フリップ・安定度・トーン） |
| `map_residual/map_residual.py` | 残留磁場マップ |
| `mot_suscept/mot_suscept.py` | MOT 感受率 |
| `shield_sf/shield_sf_ac.py` | **交流掃引から遮蔽係数 S(f)** を出す。`--test` で自己検査（11 本） |

**`run_audit.py` だけが `analysis/` 直下にある。**入力が木全体なので、どのバケットにも属さない。
`python analysis/run_audit.py` の呼び方が KB とレポートに書かれているので、この位置は動かさない。

## 3. データを持たない予測（測定の前に書いたもの）

| | |
|---|---|
| `prediction_B2/` | MS-1L 低周波ノイズの**測定前**予言（`PREDICTION.md`・`REHEARSAL.md`・`b2_summary.md`＋`predict_ms1l_noise_budget.py`・`rehearsal_b2.py`＋図 1） |
| `shield_model/` | S(f) の予測モデル（`SHIELD_MODEL.md`＋`predict_shield_sf.py`＋図 1） |

## 置くときの約束

- **新しい測定バケットを `result/` に作ったら、解析も同じ名前で作る。**
- 図は生成スクリプトと同じディレクトリに吐く（`out = os.path.join(HERE, ...)`）。
- リポジトリルートは `REPO = os.path.dirname(os.path.dirname(HERE))`（`analysis/<バケット>/x.py` の深さ）。
  直下に置くスクリプトは `os.path.dirname(HERE)`。**深さを間違えるとデータを見失う**ので、
  移動したら実際に走らせて確かめる。
- **レポートが引いている図は動かさない**（動かすなら `report/main.tex` と `main_JP.tex` の両方を同時に直す）。
  08-18 現在レポートが引く 12 枚: `modetest/fig_asd_mode_compare_0708.png`・
  `ms1l_1_predegauss/fig_asd_ms1l_vs_bench_0713.png`・`ms1l_1_predegauss/fig_relax_series.png`・
  `ms1l_3_degaussed/fig_{degauss_conditions,dg_panel_5A_taper_orbit,plateau_points,postdg0729_asd,relax0729_field,relax_all}.png`・
  `prediction_B2/fig_b2_ms1l_prediction.png`・`synctest/fig_gain_vs_frequency.png`・`twohead_sync/fig_sync_repeat.png`。
