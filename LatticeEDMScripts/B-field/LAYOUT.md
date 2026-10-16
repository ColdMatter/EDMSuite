# B-field — layout of the code

Control and analysis code for characterising the lattice eEDM magnetic
shield: QuSpin QZFM magnetometers read through an NI USB-6008, the
Twinleaf DG degausser, and the S(f) shielding-factor sweep with a
Tektronix scope on the drive current.

This is a reorganisation of `Desktop/IROP_research/scripts` (five
programs) and `Desktop/IROP_research/analysis`, written for whoever
picks the work up next. **The behaviour, the checks and the output
files are the same as the old scripts'**; what changed is where things
live and how they are wired together. The old scripts are untouched
and stay the reference until each new piece has been used at the bench.

Measurement data is not in this folder. It lives under `data_root` in
`config/lab.toml`: the Box folder
`Box/LatticeEDM/EDM chamber/Magnetic shield/Test data`, which holds a
copy of the old `IROP_research/result` tree (copied 2026-09-24) and
every new run. If it moves, change that one line.

---

## 1. The four layers

```
 run/*.py            4. ENTRY POINTS   read the command line, pick the lab,
 (a GUI, later)                        call one procedure, print, exit.
        |
 bfield/procedures/  3. PROCEDURES     whole measurements: a QZFM session,
        |                              the overnight relaxation, the S(f) sweep.
        |                              Only call operations.
 bfield/operations/  2. OPERATIONS     one experimental action each: set a head
        |                              up, record, heal, standby, one degauss
        |                              cycle, the modulation-sync check.
 bfield/devices/     1. DEVICES        one class per instrument. Talk to the
                                       hardware (or its simulation) and nothing
                                       else: no experiment logic.
```

Rules that hold everywhere below `run/`:

* **No `print`, `input()` or `sys.exit`.** Messages, questions to the
  operator and progress go through a `Context` (`bfield/ui.py`), which
  forwards to a UI — the terminal today, a GUI later.
* **Errors are exceptions.** Each carries the exit code the old scripts
  used (`bfield/errors.py`); only `run/*.py` turns it into an exit code.
* **Stopping** is a flag on the Context (`ctx.request_stop()`); Ctrl-C
  in a terminal does the same thing. A stop mid-degauss still sends
  `degauss.abort` and confirms idle; a stop mid-record still lets the
  recorder write its `meta.json`.
* **Bench facts live in `config/lab.toml`**, not in flags: serials,
  wiring, fs, vrange, ports. Every session json keeps a copy of the
  settings it ran with.
* **Time goes through `bfield/clock.py`**, so the simulator can run an
  overnight series in minutes.

## 2. Folder map

```
B-field/
  LAYOUT.md                  this file
  README.md                  install + first commands
  pyproject.toml             the package "bfield"; extras [lab] [test] [gui]
  config/
    lab.toml                 the bench: data_root, DAQ defaults,
                             degausser port, scope
    sensors.toml             the numbered sensors (head ID, USB ID, DAQ
                             inputs for Z and Y, location note) -- the
                             GUI's Manage sensors
    lab.local.toml           (optional, git-ignored) per-machine overrides
  run/                       4. entry points -- what you type
    qzfm.py                  QZFM heads: --status --setup --zero --record
                             --stream --standby --list-ports
    record.py                the recorder on its own: --probe,
                             --hardware-check, a bare-DAQ record
    degauss.py               degausser: --status --check --run
    relax.py                 overnight degauss + relaxation series: --plan --go
    sf_sweep.py              S(f) sweep: --plan --precheck --go --dc
    gui.py                   the GUI (simulation only for now)
  bfield/
    gui/                     the GUI: window, procedure forms, Qt UI,
                             the simulator's auto-operator
    config.py                load lab.toml (+ lab.local.toml)
    lab.py                   the bench or the simulator: hands out devices
    ui.py                    UI base class, ConsoleUI, Context
    errors.py                the exception family and its exit codes
    clock.py                 time (real, or sped up in the simulator)
    constants.py             instrument facts (gain table, rails, carrier,
                             DG recipe, USB-6008 limits)
    sensors.py               the numbered sensors, their DAQ wiring; which
                             ones and which axes a run records
    channels.py              names by sensor number: s2, s2z (S2Z on
                             disk), the channel map s1z:ai0,...
    heads.py                 head identity: FTDI serial <-> QuSpin name
    cli.py                   shared by run/*.py (lab flags, exit codes)
    devices/                 1. DEVICES
      qzfm.py                QZFMSensor (serial link, zero, calibrate,
                             standby), HealthTracker
      daq.py                 USB6008 (nidaqmx)
      degausser.py           TwinleafDG (tio)
      scope.py               TBS1052B (pyvisa)
      watchdog.py            deadlines for vendor calls that can hang
      simulated.py           fake QZFM package, fake tio device, SimUSB6008,
                             the shared "world" file, fault injection
      simulated_scope.py     fake VISA scope
    acquisition/             the recorder (its own process -- see 3.)
      recorder.py            the old qzfm_record.py
      child.py               start it, stop it (stop file -> signal -> kill)
    operations/              2. OPERATIONS
      sensor_setup.py        run_setup, heal, zero_and_calibrate, standby,
                             sync roles, cell-lock wait
      record.py              record_run, guard_record, housekeeping pollers
      sync_check.py          are the two heads on one modulation clock?
      degauss_cycle.py       DegaussSession: readback gate, QC, abort
    procedures/              3. PROCEDURES
      qzfm_session.py        QZFMSession + QZFMPlan (the old qzfm_serial)
      relaxation.py          the old degauss_relax
      sf_sweep.py            the old sf_sweep
    io/
      files.py               atomic json, run-folder naming, artifact rules
      runs.py                loaders for run folders (analysis side)
    analysis/
      spectra.py             shared ASD / band statistics
      allan.py               overlapping Allan deviation
  analysis_scripts/          the per-bucket analysis scripts (same bucket
                             names as the data) + run_audit.py + README.md
  tests/                     pytest, all in the simulated lab
```

## 3. Sensors are numbered

`config/sensors.toml` lists every QZFM head once (`[[sensors]]`): a
**number** that stays with the physical head, its **label** (the QuSpin
head ID printed on it, e.g. Z3D0-AB3Q), its **usb_serial** (the USB ID
the PC sees on its COM port) and a free **location** note. Edit it from
the GUI (QZFM tab, **Manage sensors**: Add / Remove, and **Scan USB**,
which lists the serial ports and which sensor owns each -- plug in only
the new sensor and pick the unowned port) or by hand.
`run/qzfm.py --sensors-list` prints the table. The head ID cannot be
read over USB; it is typed once from the label.

Each sensor also records its **wiring**: `z_input` / `y_input`, the
USB-6008 input (ai0..ai3) its Z and Y outputs go to; either may be empty
(**Z -> DAQ** / **Y -> DAQ** in Manage sensors). A QZFM records up to two
axes at once. Two sensors in the list may share an input (the cable is
moved between runs); Manage sensors notes it, and a run refuses to use
both together. Old configs without inputs fall back to `[daq] slots`.

A run picks sensors by number (`--sensors 1,2`, or the checklist in the
GUI) -- as many as the DAQ has inputs for: the USB-6008 has four, so
four sensors on one axis, or two on both. Each records on its own
inputs; the program prints which output goes to which input. **Which
axes** are recorded is a separate run choice ("Axes to record" on the
QZFM tab): single-axis mode is 1.35x quieter than dual-axis (14.8 vs
20.0 fT/rtHz, previous student's notes), so the wiring says what *can*
be recorded and the run says what *is*. Default, as the old scripts: Z
only for one sensor, every wired axis (Z and Y) for more.
Relaxation records Z; the S(f) sweep needs the driven axis (Z axial, Y
transverse) wired on both of its two sensors. Sensors can also be named
by serial (`--device-name A10QHMQMA`); the serial is looked up in the
list, so the data are still filed under the number (a serial not in the
list is refused).

With two or more sensors, one is the modulation master (command 78) and
the others are slaves (command 73) -- QuSpin's fix for the slave's
sub-Hz sine (support 2026-08-06, confirmed on the bench 2026-10-15):
the first sensor by default, or the one chosen (`--master 2`, or
"Modulation master" in the GUI); that decides which head gets the master
command, not where it plugs in.

**Everything is named by the sensor number** (`bfield/channels.py`):
sensor 2 is `s2`, its Z channel `s2z` -- lower case inside the program
and in json keys, upper case on disk and on screen, the way the old
`z` / `Z` worked:

    R01_1631/R01_1631_S1Z_pT.txt   R01_1631_S2Y_pT.txt   house_S1.csv
    meta.json  "channels": {"s1z": "Dev2/ai0", ...}, "sensor_of":
               {"s1z": "s1"}, "sensor_ids": {"s1": "A10QHMQMA"},
               "pairs": [{"channels": ["s1z", "s2z"], ...}],
               "sensor_info": [{"sensor": "s1", "number": 1, ...}]
    qzfm_session.json  "setup": {"heads": {"s1": ...}, "sync_roles":
               {"s1": "master", "s2": "slave"}}, "sensors": [...]

The recorder runs these as its `--numbered` layout (`--map
s1z:ai0,s2z:ai2 --ids s1:SERIAL,s2:SERIAL`); every pair of channels on
the same axis gets a block-mean r. Runs before 2026-10-15 keep the old
names (`z`, `y`, `z2`, `y2`, heads `a` / `b`, `house_a.csv`); the
Analysis tab and the loaders read both, and the old per-bucket analysis
scripts keep working on the old data. The S(f) analysis
(`shield_sf_ac.py`, `shield_sf_dc.py`) takes the channels by name, and
the sweep passes `--heads s1z,s2z`.

## 3b. The recorder is its own process (kept on purpose)

`run/qzfm.py --record` sets the heads up in this process, then starts
`bfield/acquisition/recorder.py` as a **child process** for the actual
recording, exactly as `qzfm_serial.py` started `qzfm_record.py`. This
was a deliberate decision by the original author and is kept: a recorder
hung in a DAQ read can be killed without losing the sensor session, and
a crash on the sensor side cannot tear the data files. The parent stops
it gently first (a STOP file the child polls, so it writes its
`meta.json`), then with CTRL_BREAK, then terminate, then kill; a child
that survives all four is exit 41 ("still holds the DAQ").

The child's command line is the old recorder's, plus `--lab-file`
(which lab it is in). Its console goes to `<run>/child.log`.

## 4. The simulated lab

Every program takes `--sim` (and `--speed N` to run the clock N times
faster). Nothing touches hardware and data goes to `B-field/sim_data/`,
never to `data_root`:

```
python run/qzfm.py --record --sensors 1,2 --seconds 60 --sim --speed 30
python run/degauss.py --run --amp 8 --coil parallel4 --taper 60 --confirm-checklist --sim --speed 40
```

The fakes imitate the **vendor objects** (the QZFM package, the tio
device, the NI card, the VISA scope), so the real adapters — timeouts,
locks, the Calibrate read, the readback gate, the taper QC — run
unchanged on top of them. The simulated heads cold-start in ~100 s,
drop the cell lock for 16 s after a zero, answer Calibrate ~10 s later,
put out 23 fT/√Hz white noise, a 50 Hz line and the aliased 923 Hz
carrier (split by 0.8 Hz when the heads are not synced); the simulated
DG ramps, soaks, tapers, cools down, clamps out-of-range writes and can
fault. Faults are injected through the world file
(`lab.world.inject(...)`, listed at the top of `devices/simulated.py`).

Use it to rehearse a session, to try a change before the bench, and to
develop the GUI.

## 5. Where each old program went

| old (`IROP_research/scripts`) | new |
|---|---|
| `qzfm_serial.py --status/--setup/--zero/--run/--stream/--standby/--list-ports` | `run/qzfm.py` same modes (`--run` is also `--record`) → `procedures/qzfm_session.py` |
| `qzfm_serial.py --device-name a:SER,b:SER` | `run/qzfm.py --sensors 1,2` (numbered sensors from `config/sensors.toml`); `--device-name` still works and is looked up by serial |
| `qzfm_record.py` (direct use) | `run/record.py`, same flags → `acquisition/recorder.py` |
| `degauss.py --status/--check/--run` | `run/degauss.py` same flags; the port comes from the config → `operations/degauss_cycle.py` |
| `degauss_relax.py --plan/--go` | `run/relax.py` → `procedures/relaxation.py` |
| `sf_sweep.py --plan/--precheck/--go/--dc` | `run/sf_sweep.py` → `procedures/sf_sweep.py` |
| `qzfm_common.py` | split: `constants.py`, `heads.py`, `devices/daq.py`, `io/files.py`, `acquisition/child.py` |
| `SensorLink`, `HealthTracker` | `devices/qzfm.py` |
| `TioDg` | `devices/degausser.py` |
| `Scope` (in sf_sweep) | `devices/scope.py` |
| `analysis/<bucket>/*.py`, `run_audit.py` | `analysis_scripts/<bucket>/`, data found through `data_root` |

Output files are the same kinds as before: `*_pT.txt`, `*_raw.csv`,
`*.meta.json` (`software` now names the new module), `house*.csv`,
`qzfm_session.json`, `degauss_session.json`, `stream_cNN.csv`, the
`rx_*` and `sw_*` trees -- with channels and sensors named by number
since 2026-10-15 (section 3). The old layouts (`--1head` / `--2head` /
`--4head` in `run/record.py`) still write the old names and keep
`software_legacy`, which `cross_spectrum.py` and `map_residual.py` gate
on.

**Where new runs go.** With `[paths] layout = "dated"` (the default in
`config/lab.toml`), a run with no `--outdir` gets its own folder, as on
the main experiment:

    <data_root>/Oct 2026/15/001_SingleSensorZ

a month folder, a day folder (two digits, like MOTMaster's), then
`NNN_<name>`: NNN counts up through the day across every program, the
name is `--name` (the GUI's "Run name"; spaces become '-') or automatic:
`SingleSensorZ` / `TwoSensorsZY` (QZFM record), `DigitalStream`,
`Degauss` / `DegaussCheck`, `Relaxation`, `SfSweepAxial` /
`SfPrecheckAxial` / `SfDCAxial`. Inside is what the program always
wrote: a QZFM run folder holds `qzfm_session.json`, the log, `_guard/`
and the record `R01_HHMM/`; a relaxation holds its log and `rx_<stamp>/`;
a degauss or a sweep folder is the session itself (no `dg_` / `sw_`
level). `--outdir` still puts a run exactly where it says, and
`layout = "buckets"` brings back the old per-program buckets
(`newdata`, `degaussingtest`, `degaussrelax`, `shield_sf`), where the
previous student's runs are. The Analysis tab opens the record inside a
run folder, and Browse starts in today's folder.
The old analysis scripts were run on simulated relaxation and S(f)
output and read it.

### Deliberate differences from the old scripts

* Only the recorder is a separate process. Setup, degauss, standby and
  the relaxation/sweep steps now run in-process instead of as child
  programs, so the old per-child timeouts are replaced by the lower
  layers' own watchdogs (every serial call, the degauss cycle, the
  recorder's overrun limit).
* Where a ledger used to store a child's command line it now stores the
  equivalent settings (`plan` key). Session jsons gain `software` and
  `simulated`; `common_version` names the rewrite. The git-revision
  banner line is gone.
* Simulated runs write `"source": "simulate"` in meta.json, so the
  analysis tools refuse them unless given `--allow-sim` (as they did
  for the old simulator).
* Sensors are chosen by number from the config (`--sensors 1,2`); `--device-name`
  with serials still works.
* **Not yet run on the bench.** Everything has been exercised against
  the simulator only. The first bench session should be a short
  `run/qzfm.py --record` next to the old `qzfm_serial.py --run`, and
  compare the two runs' meta.json and spectra.

## 6. Analysis scripts

`analysis_scripts/<bucket>/` mirrors the data buckets, as before
(`analysis_scripts/synctest/` reads `<data_root>/synctest/`). The
scripts are the old ones with one change: they find the data through
`analysis_scripts/_paths.py` (`DATA_ROOT` = env var `BFIELD_DATA_ROOT`,
else `data_root` from `config/lab.local.toml`, else `config/lab.toml`).
Figures are still written next to each script. Where a helper function
was copied identically across scripts it now comes from
`bfield/analysis/spectra.py`; copies that differ (there are several
versions of `band_median` and `spliced_asd`) were left in their scripts
on purpose, and `analysis_scripts/README.md` lists which script uses
which variant. `run_audit.py` gives byte-identical output to the old one
on all 201 runs, and the checked scripts reproduce the old numbers and
figures exactly.

Note: `LatticeEDMScripts/.gitignore` ignores `*.png`, `*.csv` and
`*.svg`, so figures and the few small csv inputs (e.g.
`modetest/manifest.csv`) are not committed unless added with `-f`.

## 7. Adding things

* **A new instrument**: a class in `devices/` with a `backend` argument
  (the vendor object, so it can be faked), a fake in `simulated.py`,
  and a method on `Lab` that hands it out.
* **A new measurement**: an operation if it is one action, a procedure
  if it strings operations together, a thin script in `run/`. Talk to
  the operator only through `ctx.ask` (yes/no gate), `ctx.pause` (a hand
  step), `ctx.say` / `ctx.warn` and `ctx.progress`.
* **A new bench setting**: a key in `config/lab.toml`.
* **Tests**: `tests/`, in the simulated lab (`sim_lab` fixture in
  `tests/conftest.py`). Run `.venv\Scripts\python -m pytest`.

## 8. The GUI

`python run/gui.py` (needs `pip install -e .[gui]`). **Simulation only
for now:** the "Bench (hardware)" option is shown but disabled
(`GUI_ALLOW_HARDWARE = False` in `bfield/gui/main_window.py`); at the
bench, use `run/*.py`.

* Left: one tab per procedure (QZFM session, Degauss, Relaxation, S(f)
  sweep) with the main settings; everything else comes from
  `config/lab.toml`. "Quick-test values" fills in short durations for a
  simulator run. Start / Abort.
* Right, top: **Sensors** -- one card per sensor of the run: the setup
  stages as indicators (connect, auto start, gain / mode, field zero,
  calibrate, guard record, sync check, standby; running / passed /
  skipped / warning / failed, hover for details), the LEDs (laser on,
  cell temp lock, laser lock, field zeroed, modulation master), the
  calibration factors per axis, the three field-zero compensation
  values, zero residual and cell temperature error. They come from the
  procedure's status channel (`ctx.status`) and from every status frame
  read from a head, so they work the same with real sensors.
  Right, bottom: the log (drag the divider to resize).
* Also right, top: **Live data** (the recorder's samples as
  they are written: raw, mean removed, 1 s block means, the ASD of
  the last 20 s with the 30-45 Hz floor read out, or the Allan
  deviation of the last 20 s with Z at 1 s read out); **Simulated lab**
  (the simulated heads' state, fault injection, the simulated S(f)
  coil drive, reset).
* **Analysis** tab: the noise spectrum of any run folder (Last record,
  or Browse -- it opens in the data folder on Box): amplitude spectral
  density in fT/√Hz against Hz, Welch with linear detrend, 200 s segments
  below 1 Hz and 20 s above (the house convention,
  `bfield.analysis.spectra.spliced_asd`; it reproduces the old sync5
  analysis). One number per channel, the **white noise**: the mean ASD
  from 10 Hz to Nyquist (limits editable; mains and carrier lines raise
  the mean -- the median is in the CSV header). Spectra use the pT files
  as recorded (nominal gain). **Plot title** box (default: the run
  folder's name). **Export data...** writes the spectrum as CSV
  (frequency, one fT/√Hz column per channel, '#' header lines with the
  run, the method and each channel's white noise).
  Loading a run shows what was recorded about its quality
  (`bfield.io.runs.run_notes`): the recorder's verdict and reason,
  per-channel problems (rail, pinned, flat, non-finite -- such channels
  are marked ⚠ and start unticked; a calibration factor above 1.5 is a
  warning), the recorder's warnings, the operator's notes (the Notes box
  / `--notes`), the session's setup warnings, heals and sync verdict, and
  any NOTES.md / README.md in the run folder or the one above.
  **Calculate: Allan deviation** instead: the overlapping Allan
  deviation of the field samples in fT against the averaging time tau
  (`bfield.analysis.allan`; tau from 1/fs to a quarter of the record by
  default, points per decade and an optional linear-drift removal
  editable). Per channel: sigma at 1 s, the minimum and its tau. For
  white noise of ASD S, sigma(tau) = S / sqrt(2 tau), but at tau ~ 1 s
  the low-frequency noise (~0.5 Hz) usually dominates, not the 10 Hz+
  floor. Its CSV: tau, then each channel's deviation and a rough error
  (deviation / sqrt(independent intervals - 1)).
* Under the Live data and Analysis plots: **lines** (curve width) and
  **text** (axis, legend, title size) -- on screen and in every export;
  **Export figure...** (PNG at 2x, or SVG vector) and **Copy to
  clipboard** (the PNG). Plots have a full black frame. Exports scale the
  lines and the legend text with the image, so a copy looks like the
  screen (pyqtgraph's lines are 'cosmetic' and would otherwise stay 1 px).
* **Auto-operator** (top bar): answers every question and does every
  hand step itself, including setting the simulated generator for each
  S(f) point. Switch it off to get the real dialogs you would get at the
  bench (yes/no, "done", typed replies), each with an "Abort run".
* Clock speed: the simulator runs that many times faster.

Files: `bfield/gui/qt_ui.py` (the UI that forwards everything from the
worker thread to the window by Qt signals), `forms.py` (the procedure
tabs), `sensor_panel.py` (the indicator cards), `sim_operator.py` (the
auto-operator), `main_window.py`.
Tests: `tests/test_gui.py` drives the window headless.

How it plugs in -- nothing below `run/` knows about Qt:

* subclass `bfield.ui.UI`: `log` → a log panel, `confirm` → a yes/no
  dialog, `acknowledge` → an "I've done it" dialog, `ask_text` → a text
  box (the S(f) sweep's typed replies), `progress` → a progress bar,
  `data` → a live plot (set `wants_live_data = True` and
  the record operation streams the recorder's samples to it);
* run the procedure in a worker thread with that UI in its Context;
  the Abort button calls `ctx.request_stop()`;
* show and edit `config/lab.toml` for the settings panel;
* develop it against `--sim`.

Before enabling the bench in the GUI: use each procedure at the bench
from `run/*.py` first, then flip `GUI_ALLOW_HARDWARE` deliberately.
