# Spectrum DDS driver + GUI — handoff

Self-contained context for continuing this work in a fresh session. Everything
needed is here; nothing has to be re-derived from the repo or the manual.

Repo: `c:\ControlPrograms\EDMSuite`  ·  Branch: `new-dds-driver`  ·  Base commit:
`9ede46b5 "dds manual, remove old dds driver"`

---

## 1. The task

Build, **from scratch**, a new driver for the Spectrum DDS card used by the
MoleculeMOT (cafmot) experiment, plus a GUI for displaying/editing DDS patterns.
The old `NeanderthalDDSController` was deliberately deleted; do not resurrect it
or let its internals influence the design. The one thing to preserve is **how DDS
patterns are defined in MOTMaster experiment scripts**.

Only MoleculeMOT matters. Ignore Lattice, AlF, EDM, Sympathetic, Tweezer folders
— except that **nothing may break for them** (see §3).

---

## 2. Decisions already made by the user — do not re-ask

| Question | Answer |
|---|---|
| Hardware access | Yes, may drive the card. **Keep core amplitudes below 0.25.** |
| Where the GUI lives | **Standalone C# app**, not a tab in MoleculeMOTHardwareControl |
| GUI must do | Status/diagnostics · visualise pattern · edit + save patterns · manual control (all four) |
| DDS↔timing relationship | **The digital pattern triggers the DDS** (CLAUDE.md's "DDS fires first" is a typo) |
| Which trigger line | **`DDS_Analog_Trg`** = `/PXI1Slot4` port 0 line 2. One line fans out to both the DDS Trg0 input and the AO board start triggers. Scripts already pulse it; no script change needed. (The user first said `DDSTrigger` port2/line3, then corrected to `DDS_Analog_Trg` when shown that all ~20 live scripts use it.) |
| Cores vs channels | **4 separate physical output channels**, one core each |
| Timing realisation | **External trigger for step 0, internal timer for the rest**; last step re-arms to the card trigger |
| Script-facing format | **Keep the legacy `Dictionary<string, List<List<double>>>`** so all ~130 scripts keep working, **and** add a typed `DDSPatternBuilder` helper |
| Extra features wanted | **XIO marker output** · **save the DDS pattern with run data**. *Not* wanted: phase control, >4 cores |
| Process model | **Standalone GUI app owns the card handle**, publishes a `MarshalByRefObject` over .NET remoting on **TCP 1818**; MOTMaster gets a proxy under `#if DDS` |

---

## 3. Hard constraints

**SAFETY — the user was emphatic:**
> "Do not ever run a motmaster script yourself, there is possibility of human harm
> or equipment damage if done at the wrong time. wait for me for the full test
> with motmaster. interacting with the dds only is safe"

So: never run a MOTMaster script, never fire a sequence, never drive the NI
pattern generators. Talking to the Spectrum DDS card alone is fine, including
enabling its outputs, provided core amplitude ≤ 0.25. Use
`M2CMD_CARD_FORCETRIGGER` instead of a real digital pulse when testing.

**NON-CAFMOT EXPERIMENTS — also emphatic:**
> "other experiments use master but not the dds. nothing must break for
> experiments not using it. use an "if DDS" flag on any shared code"

All MOTMaster-side changes go behind the `DDS` compile symbol, which only the
`CaF|AnyCPU` configuration defines (`MOTMaster.csproj:101-107`).

---

## 4. Verified hardware facts

Probed directly on the card — **trust these over the manual**.

| Property | Value |
|---|---|
| Card | **M4i.9622-x8**, `SPC_PCITYP = 0x79622`, serial 22805, 4 output channels |
| Firmware | `SPC_PCIEXTFEATURES = 0x20` = `SPCM_FEAT_EXTFW_DDS50`. No AWG, no pulse-generator option. |
| Card modes | `SPC_AVAILCARDMODES = 0x4000000` — **DDS is the only mode**; no firmware-switching risk |
| Cores | `SPC_DDS_NUM_CORES = 50` |
| Sample rate | 1 250 000 000 S/s |
| Frequency | 0 – 625 MHz, step 0.291038 Hz |
| Amplitude | −1 … +1, step 3.05185e−5 |
| Freq slope | ±1.90734863280903e14 Hz/s, step 2775.5575615628914 Hz/s |
| Amp slope | ±305174.6170612535 /s, step 298.03231910153505 /s |
| Command queue | `SPC_DDS_QUEUE_CMD_MAX = 4098` (SINGLE transfer mode) |
| Trigger timer | **83.2 ns … 27.487790730 s, 6.4 ns steps** (bisected; matches the manual) |
| Driver DLL | `spcm_win64.dll`, already in `C:\Windows\System32` |

### 4.1 Errata — the manual is wrong about these

1. **Out-of-range writes are REJECTED, not clamped.** `SPC_DDS_TRG_TIMER = 30 s`
   returns err 257 "value not allowed" and the register silently keeps its old
   value. Any layer that ignores return codes will run with stale settings.
   → The C# wrapper must throw on every non-zero return.
   *(An earlier note in this project claiming "timer maxes at 1 s" was an artifact
   of exactly this mistake — it is wrong; the range is the full 27.49 s.)*

2. **DDS XIO register numbers in the manual's tables are mangled** by the
   PDF→markdown conversion. Real values from `spcm_core.regs`:
   - `SPC_DDS_X0_MODE = 608060`, `X1 = 608061`, `X2 = 608062`
   - `SPC_DDS_X_MANUAL_OUTPUT = 608010`
   - The manual's 608011/12/13 are actually `SPC_DDS_NUM_QUEUED_CMD_IN_SW`,
     `SPC_DDS_DATA_TRANSFER_MODE`, `SPC_DDS_TRG_COUNT`.
   → Always use the constants from `spcm_core.regs` / `regs.h`, never the PDF.

3. **Illegal core→channel routing masks are silently snapped, not rejected.**
   Writing `SPC_DDS_CORES_ON_CH0 = 1` returns err 0 and reads back
   `0x7FFFFFFFFFFF`. → Always read back and verify routing.

4. **Undocumented precondition: the first DDS command block after
   `M2CMD_CARD_START` must be `EXEC_AT_TRG` + `WRITE_TO_CARD`.** Until one such
   block has been flushed, `SPCM_DDS_CMD_EXEC_NOW` fails with err 267 "the setup
   isn't valid". This means the manual's own "simple example for fixed frequency
   output" cannot work as the first thing you do. `FORCETRIGGER` is *not* needed
   to satisfy the precondition, only to make the primed block go live.
   Established by `debug_setup.py`; the driver must prime on open, and the GUI's
   manual-control tab depends on priming having happened.

5. **`SPC_TRIG_ORMASK` defaults to `SPC_TMASK_NONE`, not `SPC_TMASK_SOFTWARE`.**
   Chapter 11 says the software trigger is the default; on this card it is not,
   so `SPCM_DDS_TRG_SRC_CARD` has no trigger source until one is configured.
   The production setting is
   ```
   SPC_TRIG_EXT0_MODE   = SPC_TM_POS
   SPC_TRIG_EXT0_LEVEL0 = 1500        # mV, TTL-safe
   SPC_TRIG_TERM        = 0           # hi-Z; see open questions
   SPC_TRIG_ORMASK      = SPC_TMASK_EXT0
   SPC_TRIG_ANDMASK     = SPC_TMASK_NONE
   ```
   `SPC_TMASK_SOFTWARE` is actively harmful here: it fires the instant the card
   starts and burns through the queued blocks (observed: three steps, then
   `QUEUE_UNDERRUN`).

6. **`M2CMD_CARD_FORCETRIGGER` works repeatedly in DDS mode**, advancing the
   engine exactly one step per force, whatever the OR mask contains. It is
   therefore a faithful stand-in for the digital pulse on `DDS_Analog_Trg`, and
   the whole arm/trigger/step/re-arm cycle can be validated without the NI
   boards. (An earlier note in this file guessed the card free-runs under the
   software-trigger default — wrong on both counts.)

7. **The three status/counter registers all need care** (`debug_counters.py`):
   - `SPC_DDS_QUEUE_CMD_COUNT` **excludes the block held in the shadow
     registers.** One block is always loaded ahead, so the count hits zero one
     event *before* the pattern ends. Block size is exactly the number of
     register writes in it (17 for four cores + trigger source, 19 with XIO and
     a timer), which makes it a good integrity check.
   - `SPCM_DDS_STAT_WAITING_FOR_TRG` tracks **the queue, not the trigger
     engine**: with the queue empty it reads "idle" even though the trigger
     logic is armed and listening. It is meaningful only just after a
     `WRITE_TO_CARD` — which is exactly where `WaitUntilArmed` uses it — and it
     **glitches low for an instant at every trigger boundary**, so it must never
     be used as an "is the shot finished" test.
   - `SPC_DDS_TRG_COUNT` reads **one lower than the number of triggers actually
     processed** after a `RESET` (the first trigger is not counted), and does not
     catch up later. Differences between two readings are exact, so compare
     deltas and never absolute values.
   The only reliable end-of-shot test is `TRG_COUNT` advancing by the number of
   events.

8. **`M2CMD_CARD_WRITESETUP` is rejected once the card is started** — error 288,
   "card is still running, access not possible". Chapter 9 says output level and
   enable *may* be changed while running, and they can: writing `SPC_ENABLEOUT0`
   and `SPC_AMP0` on a running card succeeds and reads back. It is only the
   `WRITESETUP` command that must be skipped. So anything touching the output
   stage after `Open()` writes the register and stops there.
   (Corollary: calling `Open()` twice would re-run the setup and hit this, so it
   now returns early when the card is already open.)

9. **The per-event XIO marker cannot be routed to a pin on this card.**
   Chapter 17's recipe is `SPCM_X0_MODE = SPCM_XMODE_DDS` followed by
   `SPC_DDS_X0_MODE = SPCM_DDS_XMODE_MANUAL`. The DDS-side half is accepted and
   reads back; the card-level half is rejected with error 257 "value not
   allowed", because `SPCM_XMODE_DDS` (0x4, which the driver also calls
   `SPCM_XMODE_DIGIN`) is **absent from `SPCM_X0_AVAILMODES = 0xF70732B`**.
   What the X lines *do* offer: `ASYNCOUT`, `TRIGOUT`, `RUNSTATE`, `ARMSTATE`,
   `CONTOUTMARK`, `REFCLKOUT`, `SYSCLKOUT` — none of them per-event.
   The driver therefore attempts the routing, records the outcome in
   `XioMarkersRouted`, and carries on rather than failing `Open()`. The Status
   tab says so in as many words. A pattern's XIO column is still edited, saved
   and stored with the run — it just drives nothing. **Needs a scope to confirm,
   and is worth raising with Spectrum.**

### 4.2 Core → channel routing (forced by hardware)

Channel 0 accepts **only** 47 or 50 cores. Channels 1/2/3 accept **exactly**
core 47 / 48 / 49 or nothing. Anything else is snapped. The only legal
four-independent-output configuration is:

```
SPC_DDS_CORES_ON_CH0 = 0x7FFFFFFFFFFF   // cores 0..46
SPC_DDS_CORES_ON_CH1 = 1 << 47          // core 47
SPC_DDS_CORES_ON_CH2 = 1 << 48          // core 48
SPC_DDS_CORES_ON_CH3 = 1 << 49          // core 49
```

| Script channel | Core | Output |
|---|---|---|
| `...DDS1` | 0  | Ch0 |
| `...DDS2` | 47 | Ch1 |
| `...DDS3` | 48 | Ch2 |
| `...DDS4` | 49 | Ch3 |

**Cores 1–46 are also summed onto Ch0** and must be explicitly zeroed at init,
or leftovers leak into that output. Keep this mapping in exactly one place.

---

## 5. Timing model — CONFIRMED ON HARDWARE

The question that drives the whole compiler: when the engine waits for trigger
k+1, whose `SPC_DDS_TRG_TIMER` is in force?

**Answer: the LIVE one — i.e. block `k` carries the trigger source and timer that
govern the wait for event `k+1`, not its own arrival time.**

Measured by `timing_model.py`: prologue timer 0.6 s, then blocks carrying
0.2 / 0.4 / 0.8 s → observed intervals **0.5944 / 0.2001 / 0.4003 / 0.7999 s**.
The alternative model predicted 0.2/0.4/0.8 and is ruled out by 400 ms.

Because one block is always held in the shadow registers, the prologue must be
**consumed** before the pattern is queued — otherwise the external trigger at
t = 0 executes the prologue instead of event 0 and the whole pattern slips by one
step. `prime(force=True)` does exactly that, and correctly leaves the card
reporting "idle" (empty queue, empty shadow) until the first pattern is flushed.

Resulting compilation, for events sorted by time `t[0..N-1]`:

```
prologue:  TRG_SRC = CARD; all cores silenced
           EXEC_AT_TRG; WRITE_TO_CARD; FORCETRIGGER    // also primes (erratum 4)
                                                        // and clears the shadow

block k:   core params for event k (freq, amp, freq_slope, amp_slope x4)
           XIO mask for event k
           if k < N-1:  TRG_SRC = TIMER; TRG_TIMER = t[k+1] - t[k]
           if k = N-1:  TRG_SRC = CARD                 // re-arm for the next shot
           EXEC_AT_TRG

finally:   WRITE_TO_CARD
```

Budget ≈ 20 commands per event; the 4098-slot queue is ample for the ~7-event
scripts (a shot is ~120–140 commands). Since the queue is consumed by a shot,
the driver must re-queue per shot — or, better, queue several shots ahead and
top up as it drains (an improvement over the old driver, which re-armed each
shot and needed a `waitUntilArmed` handshake).

---

## 6. Codebase knowledge (expensive to re-derive)

### 6.1 Script-facing DDS pattern contract

`MOTMaster/MOTMasterScript.cs`:

```csharp
public virtual Dictionary<string, List<List<double>>> GetDDSPattern()
    => new Dictionary<string, List<List<double>>>();

public MOTMasterSequence GetSequence()   // not virtual
{
    MOTMasterSequence s = new MOTMasterSequence();
    s.DigitalPattern = GetDigitalPattern();
    s.AnalogPattern  = GetAnalogPattern();
    s.AnalogStatic   = GetAnalogStatic();
    s.DDSPattern     = GetDDSPattern();
    return s;
}
```

`MOTMaster/MOTMasterSequence.cs` carries it as a `[Serializable]` field
`public Dictionary<string, List<List<double>>> DDSPattern;`.

**Format** — key = arbitrary unique event label; value = exactly 5 inner lists:

| index | contents | length | units |
|---|---|---|---|
| 0 | time | 1 | **ms** after the DDS trigger |
| 1 | frequency | 4 | **MHz** |
| 2 | amplitude | 4 | fraction 0–1 |
| 3 | frequency slope | 4 | **MHz/ms** |
| 4 | amplitude slope | 4 | **1/ms** |

Events are unordered in the dictionary; the driver must sort on `[0][0]`.
SI conversions: `t_s = v/1000`, `f_Hz = v*1e6`, `df_Hz/s = v*1e9`,
`da_/s = v*1e3`.

Canonical example: `MoleculeMOTMasterScripts/0. DDSBlueMOT.cs:210-296`. A helper
`addDDSPattern(...)` is **copy-pasted verbatim into all ~130 scripts** (it is not
in MOTMaster) — it takes `time` in 10 µs ticks, divides by 100 to get ms, and
multiplies slopes by 100. Replacing this copy-paste with a real
`DDSPatternBuilder` in MOTMaster is part of the job.

~128 scripts override `GetDDSPattern`; a couple `return null`, so handle null.

### 6.2 Where the old driver hooked into MOTMaster

All behind `#if DDS`, recoverable via `git show b3b41384:MOTMaster/Controller.cs`:

```csharp
private NeanderthalDDSController.Controller DDSCtrl;                 // field

// in StartApplication()
DDSCtrl = (NeanderthalDDSController.Controller)Activator.GetObject(
    typeof(NeanderthalDDSController.Controller), "tcp://127.0.0.1:1818/controller.rem");

// in Go(dict), right after getSequenceFromScript(script), before buildPattern()
if (sequence.DDSPattern != null && sequence.DDSPattern.Count > 0) {
    DDSCtrl.PrepareForNewPattern();
    DDSCtrl.patternList = sequence.DDSPattern;
    DDSCtrl.InvokeParameterUpdatedSafely();
    DDSCtrl.startRepetitivePattern();
    ddsInUse = true;
}

// inside BOTH iteration loops, immediately before runPattern(sequence)
if (ddsInUse) DDSCtrl.waitUntilArmed(1.0);
runPattern(sequence);
if (ddsInUse) ddsTriggersSent++;

// after the loop
int ddsFired = DDSCtrl.patternsFired;
int ddsReceived = DDSCtrl.getCardTriggerCount();
```

Minimum remoting surface to reproduce:
`PrepareForNewPattern()`, settable `patternList`, `StartRepetitivePattern()`,
`WaitUntilArmed(double)`, `PatternsFired`, `GetCardTriggerCount()`.

Old publishing side (`NeanderthalDDSController/Runner.cs`):
```csharp
TcpChannel clientChannel = new TcpChannel(1818);
ChannelServices.RegisterChannel(clientChannel, false);
RemotingServices.Marshal(controller, "controller.rem");
```
Port map: **1172** hardware control / reporter, **1187** MOTMaster, **1818** DDS.

### 6.3 Hardware & trigger topology (cafmot)

`DAQ/MoleculeMOTHardware.cs`:
```
digitalPattern   /Dev1       PCI-6534   slave
digitalPattern2  /PXI1Slot4  PXI-6535   MASTER pattern generator
analogPattern    /PXI1Slot2  PXIe-6738  "AO",       start trigger /PXI1Slot2/PFI4
analogPattern2   /PXI1Slot7  PXI-6738   "SecondAO", start trigger /PXI1Slot7/PFI4
```
```csharp
AddDigitalOutputChannel("DDSTrigger",     digitalPatternBoardAddress2, 2, 3);  // legacy, unused
AddDigitalOutputChannel("DDS_Analog_Trg", digitalPatternBoardAddress2, 0, 2);  // THE one
```
Both digital and analog clocks are 100 kHz → 1 tick = 10 µs. Scripts fire
`p.Pulse(patternStartBeforeQ, 0, (int)Parameters["QSwitchPulseDuration"], "DDS_Analog_Trg")`,
so **t = 0 in a DDS pattern is the Q-switch instant**.

Environment selection: `DAQ/EnvironsHelper.cs:188` → `case "PH-BONESAW"` gives
`MoleculeMOTHardware` + `PHBonesawFileSystem`. PH-BONESAW is the cafmot MOTMaster PC.

### 6.4 Solution / project layout

- Everything targets **.NET Framework v4.6.1**, old-style (non-SDK) csproj,
  `ToolsVersion 12.0`, mostly `LangVersion 7.3`. The deleted DDS project was the
  lone SDK-style exception (`net461`, `LangVersion 9.0`) — a fine template.
- `EDMSuite.sln` uses **per-experiment solution configurations**; cafmot builds
  the **`CaF`** configuration → output `bin\CaF\`.
- Reference graph: `DAQ` ← `MOTMaster` ← {`MoleculeMOTHardwareControl`,
  `MoleculeMOTMasterScripts`}; `MOTMaster` also → `SharedCode`.
- `MoleculeMOTMasterScripts.csproj` compiles **only** `Properties\AssemblyInfo.cs`
  — the scripts are not in the Compile group; the project exists for IntelliSense.
  MOTMaster compiles scripts at runtime.
- NI references throughout: `NationalInstruments.Common 19.1.40`,
  `NationalInstruments.DAQmx 19.6.45.1`, `NationalInstruments.UI(.WindowsForms) 19.0.45`.

Runtime script compilation — `MOTMaster/Controller.cs:601` `compileFromFile()` uses
`CSharpCodeProvider.CompileAssemblyFromFile` with referenced assemblies =
`MotMaster.exe` + `daq.dll` + everything in
`Environs.FileSystem.Paths["AdditionalMOTMasterAssemblies"]`. That last list is
how a driver's types could be made visible to scripts.

### 6.5 Pre-existing breakage to fix (all caused by the deletion commit)

1. **`MOTMaster/Controller.cs:450-456`** — the fixed-iteration branch of `Go()`
   lost its `runPattern(sequence)` call and is now an empty loop body:
   ```csharp
   for (int i = 0; i < controllerWindow.GetIterations() && status == RunningState.running; i++)
   {
       if (!config.Debug)
       {
       }            // <-- runPattern(sequence); was deleted here
   }
   ```
   Only "run until stopped" still fires a pattern. **This is broken for every
   experiment** and must be fixed unconditionally, outside `#if DDS`.
2. `EDMSuite.sln:75` still lists `NeanderthalDDSController` (+ ~190 config lines
   at 5374-5566).
3. `MOTMaster/MOTMaster.csproj:559` — `ProjectReference` to the deleted project,
   conditional on `DefineConstants.Contains('DDS')`.
4. `MoleculeMOTMasterScripts.csproj` — **unconditional** `ProjectReference` to it
   (with a different GUID than the sln's, `{67cbabe4-…}` vs `{0AFC0329-…}`).
5. `DAQ/PHBonesawFileSystem.cs:24-26` — adds the deleted
   `NeanderthalDDSController.exe` to `AdditionalMOTMasterAssemblies`, so **every
   runtime script compile on PH-BONESAW fails** until repointed.

Net effect: the `CaF` configuration does not build at all right now.

### 6.6 Useful things to reuse

- `MOTMaster/PreviewWindow.Designer.cs` — an unwired `NationalInstruments.UI.
  WindowsForms.WaveformGraph` + channel selector; `ControllerWindow.cs:164`
  `preview_button_Click` is an empty handler. Good starting point for plots.
- `MoleculeMOTHardwareControl/Controls/SourceTabView.cs` — live NI graph usage.
- `TransferCavityLock2012/UIHelper.cs` — thread-safe graph update helpers.
- `MoleculeMOTHardwareControl/Controls/GenericController.cs` + `GigatronicsTabController.cs`
  — the Controller/View pattern used across the suite (85 lines, smallest complete example).
- `MOTMaster/MMDataIOHelper.cs` `StoreRun(...)` — zips the .cs, parameters,
  hardware class, digital pattern (JSON) and analog patterns. **DDS pattern is
  not saved** — adding it is in scope.
- `MMConfig.cs` has vestigial `UseDDS` / `DdsUsed` flags that nothing reads;
  `MoleculeMOTHardware.cs:397` constructs `new MMConfig(false,false,true,false,false)`.

---

## 7. Environment quirks (will waste time if forgotten)

- **The Bash tool is sandboxed and silently kills any process that opens the
  Spectrum device** — `spcm_hOpen` causes exit code 0 with no output and no
  traceback. **Use the PowerShell tool for anything touching the card.**
- **Do not use PowerShell here-strings (`@'...'@`) to feed `python -c`** — the
  program arrives empty and python exits 0 silently. Either use a plain
  double-quoted single-line `-c`, or (better) write a `.py` file and run it.
- Multi-line strings in the PowerShell tool have also triggered spurious hook
  errors. Prefer script files.
- Python env: `poetry run python`, venv at
  `C:\Users\cafmot\AppData\Local\pypoetry\Cache\virtualenvs\edmsuite-valcwnu9-py3.14`.
  Deps already installed: `spcm ^1.13.3`, `nidaqmx`, numpy, scipy, matplotlib, jupyterlab.
- Correct low-level imports (the high-level `spcm` package also works but the
  low-level one mirrors the C API the C# driver will use):
  ```python
  from spcm_core.pyspcm import (
      spcm_hOpen, spcm_vClose,
      spcm_dwSetParam_i32, spcm_dwSetParam_i64, spcm_dwSetParam_d64,
      spcm_dwGetParam_i32, spcm_dwGetParam_i64, spcm_dwGetParam_d64,
      spcm_dwGetErrorInfo_i32,
  )
  from spcm_core.regs import *   # SPC_* and SPCM_DDS_* constants
  ```
  Note `spcm.pyspcm` and `spcm.constants` do **not** exist; it is `spcm_core.*`.
- The manual lives in `spectrum_dds_chapters/*.md` (24 files, pdf→markdown;
  tables mangled, no figures). Chapter 17 = DDS50 (this card), 16 = DDS20,
  11 = triggers, 12 = multi-purpose IO, 18 = pulse generator.

---

## 8. What exists so far

All new files are in `c:\ControlPrograms\EDMSuite\dds_python\`. **No C# has been
written yet, and no repo file outside `dds_python/` has been modified.**

| File | Status | Purpose |
|---|---|---|
| `spectrum_dds.py` | working | `DDSCard` context manager: open/close, checked register access, `configure_dds`, `route_four_channels` (with read-back verification), `prime`, `set_core`, `silence_all_cores`, `capabilities`, `identity`, `status_text`, `watch_triggers`. `MAX_CORE_AMP = 0.25` enforced. |
| `probe.py` | **passing** | Identity, feature bits, capabilities, routing incl. the snapping demo, timer quantisation/range, queue state. |
| `timer_range.py` | **passing** | Bisects the true timer range → 83.2 ns … 27.487790730 s, 6.4 ns steps. |
| `timing_model.py` | **passing** | Settles the block-boundary question. Prints "HYPOTHESIS A holds". |
| `debug_execnow.py` | diagnostic | Acceptance matrix that first exposed err 267. |
| `debug_minimal.py` | diagnostic | The manual's minimal start sequence, step by step. |
| `debug_setup.py` | diagnostic | Proves the priming precondition (erratum 4). |
| `pattern.py` | **passing** | Prototype `ChannelState` / `Event` / `Pattern` model, legacy dict ↔ SI conversion, `validate()` against capability registers, `timeline()` for plotting, `compile_to_card()`. Direct model for the C# classes. |
| `run_pattern.py` | **passing** | Builds a realistic 6-event pattern, validates, compiles, runs N shots with `FORCETRIGGER`, checks trigger counts and underrun/overrun. `--outputs` flag enables RF for a scope check. |
| `debug_cardtrg.py` | diagnostic | Showed `SPC_TRIG_ORMASK` defaults to `NONE` (erratum 5). |
| `debug_cardtrg2.py` | diagnostic | Compares `NONE` / `SOFTWARE` / `EXT0` trigger engines. |
| `debug_force.py` | diagnostic | Proves `FORCETRIGGER` steps the engine repeatedly (erratum 6). |
| `debug_arm.py` | diagnostic | Stage-by-stage walk of prologue → pattern → step, with queue and status at each point. |
| `debug_counters.py` | diagnostic | Pins down the queue / status / trigger-count semantics (erratum 7). |

Run them with e.g. `cd c:\ControlPrograms\EDMSuite\dds_python; poetry run python probe.py`.

---

## 9. The former blocker — resolved

`run_pattern.py` used to stall on shot 1 with "card never armed". Three separate
faults, all now fixed and all recorded as errata 5–7 above:

1. The card trigger engine was never configured, so `SPCM_DDS_TRG_SRC_CARD` had
   no source at all. `configure_card_trigger()` now sets `EXT0` / `SPC_TM_POS` /
   1500 mV.
2. The DDS engine was not reset between sessions, so a still-live
   `TRG_SRC = TIMER` drained each block as fast as it was queued.
3. The end-of-shot test used `QUEUE_CMD_COUNT == 0` (one event early — the
   shadow block is not counted) and then `WAITING_FOR_TRG` (which glitches low at
   every trigger boundary). It now waits for `TRG_COUNT` to advance by the number
   of events.

Current result — 20 consecutive shots, outputs disabled:

```
shot 1: queued  94 cmds, ran in 0.4013 s (span 0.4000 s), trg +6 (expected 6)
...
shot 20: queued  94 cmds, ran in 0.4019 s (span 0.4000 s), trg +6 (expected 6)

triggers over 20 shots: 120 (expected 120)
final status: idle
```

Every shot fires exactly its six triggers, the measured span matches the
programmed 0.4000 s to ~1.5 ms of software polling latency, the queue fill is a
stable 94 commands (= 19 + 19 + 19 + 19 + 18 for blocks 1–5, block 0 being in the
shadow registers), and there are no underruns or overruns. **Stage 1 steps 5 and
6 are green.**

Still outstanding in stage 1, both needing the user or a scope:
- step 3, single tone per channel — confirm the DDS1→Ch0 … DDS4→Ch3 map at an SMA;
- step 4, XIO marker timing on a scope.

---

## 10. What is built

### 10.1 `SpectrumDDS/` — driver class library (net461, AnyCPU)

| File | Contents |
|---|---|
| `SpcmRegs.cs` | Every register constant used, taken from the installed `spcm_core.regs`, never the PDF. |
| `SpcmCard.cs` | `[DllImport("spcm_win64.dll")]` on the **undecorated** x64 export names (verified with `dumpbin /EXPORTS`). Throws `SpcmException` on any non-zero return. Serialises individual calls so a status poll cannot land inside another thread's. |
| `DDSPattern.cs` | `DDSChannelState` / `DDSEvent` / `DDSPattern`, `FromLegacyDictionary` / `ToLegacyDictionary`, `Validate()`, `CommandCount()`, `Timeline()` for plotting. |
| `DDSCapabilities.cs` | `SPC_DDS_AVAIL_*` and card identity, read from the card. |
| `DDSPatternCompiler.cs` | The block-boundary timing model, in one place. |
| `DDSPatternFile.cs` | JSON load/save via `DataContractJsonSerializer` (the suite has no JSON package), in script units. |
| `SpectrumDDSDriver.cs` | Lifecycle, trigger engine, routing with read-back, priming, arm/run, status, `EXEC_NOW` manual output, amplitude clamp. Core→channel mapping lives here and nowhere else. |

### 10.2 `SpectrumDDSController/` — standalone WinForms app (WinExe, x64)

Owns the card handle, publishes `Controller` (a `MarshalByRefObject`) on TCP 1818
as `controller.rem`. Tabs: **Status** (identity, capabilities, queue fill, DDS
status, trigger count, patterns fired, polled at 4 Hz), **Pattern**
(`DataGridView` editor, `ScatterGraph` of frequency and amplitude against time
for all four channels with ramps drawn as real slopes, JSON load/save, load onto
card, arm, stop, **test trigger**), **Manual** (per-channel frequency/amplitude
with `EXEC_NOW`, output enables, output level, and the amplitude clamp).

"Test trigger" fires one DDS trigger in software so a pattern can be checked on a
scope with the experiment idle. It reaches the Spectrum card and nothing else — no
NI board, no sequence.

Two things worth knowing:
- It uses **`ScatterGraph`, not `WaveformGraph`**. Pattern events are unevenly
  spaced in time and `WaveformGraph` plots against a sample index.
- Measurement Studio controls throw `LicenseException` at construction unless the
  build embeds `Properties/licenses.licx`, and a `ScatterGraph` built in code
  starts with **no axes at all**, so `XAxes[0]` throws until you add them.

### 10.3 MOTMaster integration, all `#if DDS`

- `Controller.cs` — proxy in `StartApplication()`, load-and-arm after
  `getSequenceFromScript()`, `WaitUntilArmed(1.0)` before `runPattern` in **both**
  loops, sent/fired/missed report after.
- `MOTMasterScript.cs` — `DDSPatternBuilder`, replacing the `addDDSPattern`
  helper copy-pasted into every script, with identical units.
- `MMDataIOHelper.cs` — `storeDDSPattern` writes `*_ddsPattern.json` into the run
  zip beside the digital and analog patterns.

### 10.4 Pre-existing breakage, fixed

The empty `for` loop in `Controller.cs` now calls `runPattern(sequence)` again —
fixed **unconditionally**, since it was broken for every experiment.
`EDMSuite.sln`, `MOTMaster.csproj`, `MoleculeMOTMasterScripts.csproj` and
`PHBonesawFileSystem.cs` all point at the new projects.

---

## 11. Verification status

**Stage 1 (Python, card only) — green** except the two steps that need a scope:
20 consecutive shots, exact trigger counts, no underruns (§9).

**Stage 2 (C#) — green:**

| Check | Result |
|---|---|
| `SpectrumDDS` against the real card | 10/10 shots, 60/60 triggers, span 0.4015 s against a programmed 0.4000 s, queue steady at 94 commands, patterns fired 10 |
| Legacy dictionary round-trip | max error **0** |
| JSON round-trip | 6 events, span preserved |
| GUI standalone, MOTMaster closed | starts, all three tabs render, pattern plots with ramps as slopes |
| `CaF` configuration | `SharedCode`, `DAQ`, `SpectrumDDS`, `SpectrumDDSController`, `MOTMaster`, `MoleculeMOTHardwareControl`, `MoleculeMOTMasterScripts` all build |
| Non-CaF regression (`AlF`, `Sympathetic`, `EDM`) | all build, and **no SpectrumDDS artefact appears in their output** |
| Remoting surface over TCP 1818 | see below — passed |

The remoting path is the one piece MOTMaster wholly depends on, so it is tested
separately by a C# harness that runs the same calls
`MOTMaster.Controller` makes, against a live `SpectrumDDSController` process and
the real card (`scratchpad/RemotingTest.cs`, driven by `remoting.ps1`):

```
proxy is transparent : True
card open            : True
pattern over the wire: 4 events, span 200 ms, 75 commands
DDS triggers -- sent: 10, fired: 10, missed: 0, card counted: 40
arm failures         : 0
stop returned in     : 0.014 s
restart cycles       : survived
REMOTING TEST PASSED
```

`sent == fired`, no arm failures, and the card counted exactly 10 shots × 4 events.
The dictionary marshals by value in both directions, and `LoadedPattern` comes back
as a `DDSPattern`.

Two faults this found and fixed:
- `WaitForShot` ignored stop requests, so `StopRepetitivePattern` blocked for up to
  `span + 5 s`. Since it also dropped the thread handle on a timed-out join, a
  stop-then-start could leave **two shot threads arming the same card**. It now
  takes an abort callback and only forgets the thread once it has really gone.
- `LoadPattern` threw `NullReferenceException` rather than anything useful when the
  card was not open.

Note that PowerShell cannot drive this proxy — its object adapter throws inside
`InternalDeserializer` on the property setter. That is a PowerShell limitation, not
a fault in the remoting surface; test it from C#.

Real-script check — `0. DDSBlueMOT.cs` compiled the way MOTMaster compiles it,
its `GetDDSPattern()` taken and converted:

```
events        : 7          sorted span: 123.01 ms      commands: 132 of 4098
MOT                 0.000 ms   DDS1 114.0700 MHz  amp 0.2500   da  0.00000 /ms
RampStart          50.000 ms   DDS1 114.0700 MHz  amp 0.2500   da -0.01530 /ms
RampEnd            60.000 ms   DDS1 114.0700 MHz  amp 0.0970
LambdaCooling      65.000 ms   DDS1  97.2800 MHz  amp 0.2400
BlueMOTRampStart   68.000 ms   DDS1 102.5000 MHz  amp 0.2500
FreeExpTime       118.000 ms   DDS1 111.4200 MHz  amp 0.0000
image             123.010 ms   DDS1 114.0700 MHz  amp 0.2500
round-trip max error: 0
```

Every number matches the script source: `MOTFreqDDS1 = 114.07`,
`MOTAmpDDS1 = 0.25`, `CompressRampDownStartTime = 5000` ticks → 50 ms,
`+ CompressRampDownDuration = 1000` → 60 ms, and
`RampAmplitudeDDS1 = -0.000153` per tick × 100 → −0.0153 per ms.

**Stage 3 — for the user.** Real external trigger on `DDS_Analog_Trg`, a
50-iteration `0. DDSBlueMOT.cs` run, scope comparison, saved-run check.

---

## 12. Open questions, and one that now has teeth

- **The 0.25 amplitude clamp will reject the real MOT scripts.**
  `0. DDSBlueMOT.cs` asks for **0.6 on DDS2 and 0.35 on DDS3**. 0.25 was agreed as
  a limit for unsupervised bench testing, and it is still the per-channel default,
  but it has to be raised before any real run. The clamp is **per channel** --
  `SpectrumDDSDriver.MaximumAmplitudes[]`, set with `SetMaximumAmplitude(channel,
  value)` or from the clamp column on the Manual tab -- so DDS2 and DDS3 can be
  raised without leaving DDS1 and DDS4 unprotected. The values persist in
  `%APPDATA%\SpectrumDDSController\settings.json`. What the production numbers
  should be is still the user's call.
- What output level (mV) should each channel drive, and what do the AOM drivers
  expect? Tests have been using 500 mV arbitrarily.
- Trigger input termination is currently hi-Z (`SPC_TRIG_TERM = 0`) with the
  comparator at 1500 mV. Is `DDS_Analog_Trg` 3.3 V or 5 V TTL, and should it be
  50 Ω terminated?
- Between shots the card holds the **last** event's state. If the MOT state should
  be held instead, the pattern needs a final event replicating event 0.
  **Decided (2026-08-04): the channels hold the last event's state at the end of a
  run too.** MOTMaster stops the shot loop and touches nothing else, so the RF stays
  where the pattern left it until a new pattern is loaded. The queue is only ever
  cleared by `DiscardQueuedShots`, which has to stop the card and so takes the RF
  down with it — hence it runs on a pattern load or a manual tone, never at the end
  of a run.
- Should the driver queue several shots ahead rather than re-arming per shot? The
  queue holds ~30 shots of a typical pattern, and it would remove the
  `WaitUntilArmed` handshake entirely.
- `SpectrumDDSController.exe` triggers a **Windows Defender Firewall prompt** the
  first time it opens port 1818. Left unanswered — allowing it is a system change
  for the user to make.
- **XIO markers do not reach a pin on this card** (erratum 9). One of the
  requested features is therefore inert. Options: raise it with Spectrum, or use
  one of the modes the card does offer — `ARMSTATE` is the closest useful one, a
  level that goes high whenever the DDS engine is waiting for its trigger, which
  would at least mark shot boundaries on a scope.
