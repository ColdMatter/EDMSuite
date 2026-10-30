# B-field

Magnetic shield characterisation for the lattice eEDM experiment:
QZFM magnetometers, NI PCIe-6321 (or USB-6008) recording, Twinleaf DG degausser,
S(f) sweeps, and the analysis scripts. **See [LAYOUT.md](LAYOUT.md)
for how the code is organised and where each old IROP_research script
went.**

## Installing (lab PC, once)

```
cd LatticeEDMScripts\B-field
py -3.14 -m venv .venv
.venv\Scripts\python -m pip install -e ".[lab,test]"
```

The `[lab]` extra needs vendor runtimes pip cannot install: the
**NI-DAQmx** driver (PCIe-6321 / USB-6008) and a **VISA** runtime (TekVISA or
NI-VISA, for the scope). On any other machine `pip install -e ".[test]"`
is enough to run everything in the simulator.

Check `config/lab.toml` (data folder, head serials, wiring, degausser
port). Per-machine changes go in `config/lab.local.toml`.

## First commands

Rehearse in the simulated lab first (`--sim`; nothing is driven, data
goes to `sim_data/`):

```
.venv\Scripts\python run\qzfm.py --record --sensors 1,2 --seconds 60 --sim --speed 30
.venv\Scripts\python run\degauss.py --status --sim
.venv\Scripts\python run\relax.py --plan --sim
```

At the bench (close the QuSpin GUI and DG Control first — the ports are
exclusive):

```
.venv\Scripts\python run\qzfm.py --list-ports
.venv\Scripts\python run\qzfm.py --status --sensors 1,2
.venv\Scripts\python run\qzfm.py --record --sensors 1,2 --name Baseline
```

The GUI (simulation only for now):

```
.venv\Scripts\python -m pip install -e ".[gui]"
.venv\Scripts\python run\gui.py
```

Every script has `--help`. Tests: `.venv\Scripts\python -m pytest`.

The operating notes (how the sensors behave, what went wrong before)
are still `Desktop/IROP_research/experimentnote/`; read
`01_qzfm_operation.txt` before touching a sensor.
