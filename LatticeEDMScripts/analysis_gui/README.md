# LatticeEDM analysis GUI

A window for the standard analyses of ScanMaster data, built on
`LatticeEDM_analysis_library.py`. It follows the batch scripts in
`analysis scripts/`, which are unchanged and still work as before.

| Tab | Data | Follows |
|---|---|---|
| Duration | v0Duration scans, 2-shot On/Off | `Duration_batch_SL.py` |
| Setpoint scan | TCL setpoint (V) or WM lock (GHz from an offset) scans, 2-shot On/Off | `setpointScan_batch_SL.py` |
| Slowing (4-shot) | setpoint scans with slowing On/Off and YAG On/Off shots | `setpointScanONOFF_4shot_batch_SL.py` |
| TOF collector | nothing scanned, On/Off statistics (2- or 4-shot) | `TOFcollectorONOFF_SL.py` |

## Starting it

* **Desktop shortcut** (once per computer): with the Python you analyse data
  with (Spyder's), run from `LatticeEDMScripts`

      python -m analysis_gui.shortcut

  This puts "LatticeEDM Analysis" on the desktop. It starts the GUI with that
  Python's `pythonw.exe`, without a console window.
* Or double-click `LatticeEDM Analysis.pyw` (if .pyw files open with that Python),
* or run `python -m analysis_gui` from `LatticeEDMScripts`.

It uses the code in the repository as it is, so a `git pull` or an edit of the
library takes effect the next time it starts. Nothing needs building.

## Using it

1. **Data folder** at the top: usually a date, e.g. `...\data\Oct 2026\26`.
   It starts at the data drive of `data_locations.json`.
2. In a tab, the **keyword** picks the files (`Duration` matches
   `*Duration*.zip`). **File IDs** is the `sele` list. Leave it empty to load every
   matching file, or tick files in the list. **Load each pass** is `LoadPasses`.
3. **Load** reads the files in the background. Big files take a few seconds
   each (the .NET reader is the slow part). Loaded files are kept in memory, so
   changing a gate and pressing **Run analysis (F5)** is quick.
   *File → Forget loaded files* frees the memory.
4. **Groups** play the part of the `types` dictionaries of the scripts. In the
   Slowing tab, `004+005, 012` averages 004 and 005 together and fits 012 on its own.
5. Every plot has the matplotlib toolbar (zoom, save). Result tables can be
   copied (Ctrl+C, pastes into Excel/Origin) or exported to CSV.
6. **Gates from file** fills in the gates ScanMaster saved with the scan.

The **log** at the bottom shows everything the library prints, including
skipped scans and failed fits. Settings, groups and the folder are
remembered between sessions.

## Where the code is

* `LatticeEDM_analysis_library.py`, section *Shared processing functions*:
  the calculations, as numpy functions with no plotting, usable from scripts
  too (`EDM.ExtractScanArrays(scan)`, then e.g. `EDM.GatedOnOff(...)`).
* `analysis_gui/modes/*.py`: one file per tab (settings, analysis, plots).
* `Tests/test_analysis_*.py`: the functions on synthetic scans, the tabs
  offscreen, and agreement with the original library functions on real files.

## Differences from the scripts

The numbers are the same as the scripts' except here:

* **Error bars** of gated TOFs are the standard error over the shots of a
  point, in ms.V. The Duration and Setpoint ratios also get error bars.
* **Duration**: the scattering rate error is σ_τ/(τ²|ln BR|).
  With no groups, all scans with good fits are averaged and fitted like a group.
* **Setpoint scan**: each scan decides its own x axis (the script uses the
  last file's). Stacked data are binned on the plotted x axis, in its units, with
  bins centred on the scan points. The default bin width is the scan step.
  *Exclude scans* also applies to the average over passes.
* **TOF collector**: each TOF has the mean of its background window
  subtracted (the script uses one sample at the window start). Errors are
  std/√N, and the ratio SEM is std/√(points) (the script uses std/points).
* **Slowing (4-shot)**: the same as the script. Every group in the table is
  shown on the Compare and TOFs-in-range pages.
