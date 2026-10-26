# CaF MOTMaster snippets: findings, decisions and what is built

Branch `cafmot-snippets`, started 2026-10-22. This note covers what we found in the current scripts, what has been decided, and (from 2026-10-23) what has been built: the MOTMaster changes, the snippet library and snippet versions of 23 scripts, which sit next to the originals for testing. See "Built so far" at the end.

## The goal

Break the cafmot pattern scripts in `MoleculeMOTMasterScripts/` into snippets: chunks of a MOTMaster pattern that scripts plug in instead of copy-pasting the same lines. Only the scripts directly in that folder are in scope (26 today). Subfolders (`OLD`, `ODT_scripts_Aug26`, `Spectroscopy_Aug26`, ...) are left alone; MOTMaster does not list them anyway.

A snippet today is a class implementing `MOTMasterScriptSnippet` (`MOTMaster/MOTMasterScriptSnippet.cs`), which has two methods, one for the digital pattern and one for the analog pattern. Scripts already use one: `new LoadMoleculeMOTNoSlowingEdge(p, Parameters)`.

## What the 26 scripts have in common

| Stage | Scripts that have it | How much it varies |
|---|---|---|
| Load, slowing, MOT, compressed MOT, first lambda cooling | 24 of 26 | Identical line for line, in digital, analog and DDS |
| Blue MOT, then second lambda cooling | 20 | Barely |
| Magtrap ending | `3.*`, `BX_*` | Same shape, some numbers differ |
| Handover MOT coils → transfer coils, move, handover → tweezer coils | `4.*` to `8.*`, `TweezerRESONANTlMAGE` | Same shape, numbers often typed in (e.g. tweezer-coil field 1.8, 0.48, or a parameter) |
| Tweezer-chamber ending (blue MOT, red MOT, lambda + free fall, trip back) | `5.*` to `8.*` | One-offs per script |
| Camera triggers | all | Least uniform: about 20 different `cameraTrigger` lines |

The two outliers in the first row are `0. DDSFourSidebands5W_noDDS` and `BX_spectroscopy`, which have no compressed MOT or lambda stage.

Other repetition:

- Every script works out its stage times (`lambdaCoolingStart`, `BlueMOTEnd`, ...) three times, once each in the DDS, digital and analog methods. The digital copy is shifted by `TCLBlockStart`.
- 25 of the 26 scripts carry their own copy of the `addDDSPattern` helper. Only `2.a DDSBlueMOT` builds its DDS pattern with `DDSPatternBuilder`.

Places where the scripts have drifted apart:

- Six scripts never pulse `DDS_Analog_Trg`: `1. LambdaRecap`, `3. MagTrap_withBM`, `4. BM_move_coils_test`, `4. BlueMOT_transport_test`, `4. TCoil_test_back_forth_movement`, `4. TCoil_test_no_movement_1.5secHold`.
- `bXSlowingShutter` has four different versions.

## Decided so far

1. **Snippets are stages of the shot:** slowing/load, MOT, compressed MOT, lambda cooling, blue MOT, handover/transport, optical pumping, magtrap, imaging.
2. **Snippets cover DDS as well as digital and analog.** `MOTMasterScriptSnippet` gets a third method, `AddDDSSnippet(DDSPatternBuilder p, ...)`, inside `#if DDS`, so only the CaF build sees it. The four snippets already compiled into MOTMaster.exe (`LoadMoleculeMOT`, `LoadMoleculeMOTDualSpecies`, `LoadMoleculeMOTNoSlowingEdge`, `SHLoadMOT`) get an empty `#if DDS` stub, which the CaF build needs in order to compile. Nothing changes for other experiments. Scripts using DDS snippets build their DDS pattern with `DDSPatternBuilder` instead of their own copy of `addDDSPattern`.
3. **`DDS_Analog_Trg` is fired in every script.** The six scripts listed above will gain it when converted.
4. **Other drift becomes parameters.** Where scripts differ (shutter timings, typed-in fields, fixed gaps like `+ 100`), the snippet reads a parameter with a sensible default and each script keeps its current value. Behaviour only changes where something was clearly a mistake, as with the missing trigger.
5. **Snippets live in `MoleculeMOTMasterScripts/Snippets/`, one class per file, the file named after the class.** They are compiled when a script is loaded, not built into MOTMaster.exe, so editing a snippet needs no rebuild.
6. **Only the snippets a script uses are compiled with it.** When a script loads, MOTMaster removes comments and string literals from it, finds names that match files in `Snippets/`, and compiles those, following snippets that use other snippets. A broken snippet only affects the scripts that use it.
7. **The script defines when each stage runs.** Snippets take only `(p, parameters)`; each script writes every stage's start and end time into `Parameters` and snippets read them. Details below.

Consequences of 5 and 6:

- `Controller.compileFromFile` (shared code) changes. It is switched on by a path entry that only cafmot's `PHBonesawFileSystem.cs` sets, the same way `AdditionalMOTMasterAssemblies` works, so other experiments compile exactly as before.
- The runtime compiler only accepts C# 5, so snippets cannot use newer syntax such as `$"..."` strings. Scripts already have this limit.
- New snippets go in their own namespace, so they cannot clash with the ones in MOTMaster.exe.
- `verify-scripts.ps1` copies `compileFromFile`, so it needs the same change.

## How a snippet knows when it starts

A snippet is given only the pattern and the parameter dictionary. `0. DDSFourSidebands5W_noDDS` adds one with `new LoadMoleculeMOTNoSlowingEdge(p, Parameters)` and nothing else, and the snippet reads every time it needs from the dictionary (`TCLBlockStart`, `SlowingChirpStartTime`, `SlowingChirpDuration`, ...). There is no start-time argument, and new snippets keep that shape: constructors take `(p, parameters)` only.

So a stage's start time has to get into the parameters. For the load stage this is easy, because its times are already plain parameters. Later stages start at times the scripts work out today (`lambdaCoolingStart`, `BlueMOTEnd`, ...).

Two facts about MOTMaster that this relies on:

- Scan values are applied after the script object is built and before its patterns are built (`prepareScript` calls `EditDictionary` and then `getSequenceFromScript`). A start time written into `Parameters` in the constructor would therefore miss a scanned duration. It has to be written inside the `Get...Pattern` methods.
- The dictionary saved with each run is `script.Parameters` after the patterns are built, so start times written during pattern building are recorded with the shot. They are not in the parameter list the GUI shows, so they cannot be scanned directly; scan the durations they come from. They must not also be set in the constructor, or the scan value would be overwritten.

**Decided: the script defines the times.** Each script writes every stage's start and end time (`BlueMOTStartTime`, `BlueMOTEndTime`, ...) into `Parameters`, and snippets only read them. Each time is defined in one place, the script. Times are always counted from the Q-switch; digital snippets add `TCLBlockStart` themselves, as `LoadMoleculeMOTNoSlowingEdge` already does, so scripts never handle that offset.

A snippet then looks like this:

```csharp
namespace MOTMaster.SnippetLibrary.CaF
{
    // Blue MOT: ramp the MOT coils to BlueMOTField and switch the DDS to the blue-MOT tones.
    // Runs from BlueMOTStartTime to BlueMOTEndTime (ticks after the Q-switch).
    public class BlueMOT : MOTMasterScriptSnippet
    {
        public BlueMOT(PatternBuilder32 p, Dictionary<String, Object> parameters)
        { AddDigitalSnippet(p, parameters); }
        public BlueMOT(AnalogPatternBuilder p, Dictionary<String, Object> parameters)
        { AddAnalogSnippet(p, parameters); }
        public BlueMOT(DDSPatternBuilder p, Dictionary<String, Object> parameters)
        { AddDDSSnippet(p, parameters); }

        public void AddDigitalSnippet(PatternBuilder32 p, Dictionary<String, Object> parameters) { }

        public void AddAnalogSnippet(AnalogPatternBuilder p, Dictionary<String, Object> parameters)
        {
            p.AddLinearRamp("MOTCoilsCurrent", (int)parameters["BlueMOTStartTime"], (int)parameters["BlueMOTRampDuration"], (double)parameters["BlueMOTField"]);
            p.AddAnalogValue("MOTCoilsCurrent", (int)parameters["BlueMOTEndTime"], (double)parameters["MOTCoilsOffValue"]);
        }

        public void AddDDSSnippet(DDSPatternBuilder p, Dictionary<String, Object> parameters)
        {
            p.AddEvent("BlueMOTRampStart", (int)parameters["BlueMOTStartTime"],
                new double[] { (double)parameters["FreqCVB1"], (double)parameters["FreqCVB2"], (double)parameters["FreqCVB3"], (double)parameters["FreqCVB4"] },
                new double[] { (double)parameters["BMOTAmpDDS1"], (double)parameters["BMOTAmpDDS2"], (double)parameters["BMOTAmpDDS3"], (double)parameters["BMOTAmpDDS4"] });
        }
    }
}
```

The script example below is trimmed to the stages that `7. OP_transfer_to_tweezer_coils_blueMOT` shares with most scripts. `OPGap` is today's typed-in `+ 100`, made a parameter.

Because a snippet cannot be handed a name, a stage that runs twice in one shot needs a way to tell the two apart (DDS event names must be unique, and each needs its own times). Lambda cooling runs twice, so it is two classes, `LambdaCooling1` and `LambdaCooling2`, using `LambdaCooling1StartTime`/`EndTime` and `LambdaCooling2StartTime`/`EndTime`. How to share the code between them is part of the reuse question further down.

### A script

```csharp
public class Patterns : MOTMasterScript
{
    // ... constructor as now ...

    // The whole shot, relative to the Q-switch. Called at the top of each Get...Pattern, after scan values are in.
    private void SetTimeline()
    {
        Parameters["CMOTStartTime"]           = (int)Parameters["CompressRampDownStartTime"];
        Parameters["CMOTEndTime"]             = (int)Parameters["CMOTStartTime"] + (int)Parameters["CompressRampDownDuration"] + (int)Parameters["CompressRampDownHoldDuration"];
        Parameters["LambdaCooling1StartTime"] = Parameters["CMOTEndTime"];
        Parameters["LambdaCooling1EndTime"]   = (int)Parameters["LambdaCooling1StartTime"] + (int)Parameters["LambdaCoolingDuration"];
        Parameters["BlueMOTStartTime"]        = Parameters["LambdaCooling1EndTime"];
        Parameters["BlueMOTEndTime"]          = (int)Parameters["BlueMOTStartTime"] + (int)Parameters["BlueMOTRampDuration"] + (int)Parameters["BlueMOTDuration"];
        Parameters["LambdaCooling2StartTime"] = Parameters["BlueMOTEndTime"];
        Parameters["LambdaCooling2EndTime"]   = (int)Parameters["LambdaCooling2StartTime"] + (int)Parameters["LambdaCooling2Duration"];
        Parameters["OPStartTime"]             = (int)Parameters["LambdaCooling2EndTime"] + (int)Parameters["OPGap"];
        // ... and so on for optical pumping, magtrap and handover ...
    }

    public override AnalogPatternBuilder GetAnalogPattern()
    {
        AnalogPatternBuilder p = new AnalogPatternBuilder((int)Parameters["PatternLength"]);
        SetTimeline();
        new LoadMOT(p, Parameters);
        new CompressedMOT(p, Parameters);
        new LambdaCooling1(p, Parameters);
        new BlueMOT(p, Parameters);
        new LambdaCooling2(p, Parameters);
        new OpticalPumping(p, Parameters);
        new Magtrap(p, Parameters);
        new HandoverToTransferCoils(p, Parameters);
        // ... script-specific lines ...
        return p;
    }
    // GetDigitalPattern and GetDDSPattern call SetTimeline() and the same snippets.
}
```

Why this way:

- The whole timeline is in one place, and changing a gap between stages is a one-line edit.
- A snippet does not know what comes before or after it, so the same snippet can go anywhere in a shot. The order of the snippet lines still matters, though (corrected 2026-10-23): a linear ramp starts from whatever value its channel has at that time among the events added so far, and digital edges on a channel must be added in time order. So scripts call snippets in time order.
- The cost is that every script writes out its timeline: the 24 scripts that share the load-to-lambda-cooling stages each carry the same lines for them.
- The snippet calls are written three times, once per pattern, as scripts already do today.

### Rejected: each snippet defines its own times

The alternative was for each snippet to set its own times, e.g. `BlueMOT` setting `BlueMOTStartTime = LambdaCooling1EndTime`, so that a script is only a list of snippet calls. It was rejected because each snippet then names the stage before it, so a stage cannot be moved, dropped or used in a different place without a different snippet; gaps between stages such as `OPGap` live inside snippets; and the order of the snippet lines matters.

## Built so far (2026-10-23)

Nothing here has been run on the experiment yet. Every converted script has been checked against its original off the experiment (see "How the conversions were checked"), but MOTMaster itself has not been rebuilt on PH-BONESAW yet.

### MOTMaster (shared code, switched on only for cafmot)

- `MOTMaster/ScriptSnippets.cs` (new): finds the snippet files a script uses, as in decisions 5 and 6.
- `Controller.compileFromFile` compiles those files along with the script, and `loadScriptFromDLL` then instantiates only the `MOTMasterScript` class (previously it created every class in the file and kept the last one, which breaks as soon as snippets are compiled in).
- `MMDataIOHelper` saves a copy of each snippet a run used (`<tag>_snippet_<Name>.cs`) next to `<tag>_script.cs`, so the zip records the code that ran. Re-running an old zip ("replica run") still compiles against today's `Snippets/` folder.
- All three only act when `Paths["MOTMasterSnippetPath"]` is set, which only `DAQ/PHBonesawFileSystem.cs` does. Other experiments compile and save exactly as before.
- `MOTMasterScriptSnippet` gains `AddDDSSnippet` inside `#if DDS`, with empty stubs in the four snippets compiled into MOTMaster.exe (decision 2).
- `.claude/skills/motmaster-controller/ScriptCompileCheck.cs` (behind `verify-scripts.ps1`) does the same, using `Snippets\` next to the scripts when it exists.

### The snippet library (`MoleculeMOTMasterScripts/Snippets/`, namespace `MOTMaster.SnippetLibrary.CaF`)

Each snippet sets its stage's state at the stage's start time and does not switch anything off at its end: the next stage, or a script line, does that. The comment at the top of each file says which parameters it reads.

| Snippet | What it does | Reads |
|---|---|---|
| `LoadMOT` | YAG, slowing light/chirp/coils, bX shutter, DDS_Analog_Trg, ToF trigger, shims, MOT coils on, DDS to the MOT tones. Wraps `LoadMoleculeMOTNoSlowingEdge`. | `bXSlowingShutterDuration`, `TCoolSidebandVCOValue` |
| `CompressedMOT` | MOT coils ramp up, DDS intensity ramp down | `CMOTStartTime` |
| `LambdaCooling1`, `LambdaCooling2` | MOT coils off, DDS to the lambda tones | `LambdaCooling1StartTime`, `LambdaCooling2StartTime` |
| `BlueMOT`, `BlueMOT2`, `TweezerBlueMOT` | Coils ramp to the blue-MOT field, DDS to the blue-MOT tones. `TweezerBlueMOT` uses `TweezerCoils` and `tBlueMOTField`. | `BlueMOTStartTime`, `BlueMOT2StartTime`, `tBlueMOTStartTime` |
| `LightOff1`, `LightOff2` | DDS to the resonance frequencies at `LightoffDDS1` | `LightOff1StartTime`, `LightOff2StartTime` |
| `OpticalPumping` | QCL shutter open from the start, closed for `OpticalPumpingDuration` | `OPStartTime` |
| `Magtrap` | MOT coils to `MOTCoilsMagtrapValue` | `MagtrapStartTime` |
| `HandoverToTransferCoils` | MOT coils to 0 and transfer coils to 10 V over `HandoverDur` | `TransferHandoverStartTime` |
| `HandoverToTweezerCoils` | Transfer coils to `TransferCoilsOffValue` and tweezer coils to `tMagTrapField` over `HandoverDur` | `TweezerHandoverStartTime` |

`CaFDDS` is a helper giving the four DDS frequencies or amplitudes for each stage.

Each snippet adds the analog channels it uses. Adding a channel that is already there is ignored: `AnalogPatternBuilderSingleBoard.AddChannel` now skips a duplicate instead of throwing, the same one-line change the `cafbec` branch made for its snippets, so the two branches merge cleanly there. This is the one change that affects every experiment: a script that adds a channel twice no longer fails.

This answers the earlier question of how a stage is reused in a different place: a base class holds the code and small subclasses choose the stage name, and where needed the coils and field parameter (`LambdaCooling`, `BlueMOTStage`, `LightOff`). The stage name sets both the time parameters it reads and its DDS event name, so two uses in one shot never clash.

New parameters in `globalParameters.json`, each defaulting to the value the scripts had typed in: `TCoolSidebandVCOValue` (-3.461), `TransferCoilsOffValue` (0.0; the 6.*, 7.* and 8.* scripts set -0.1), `OPGap` (100). Scripts that typed a tweezer-coil field (0.48 or 1.8) now set `tMagTrapField` to it in their constructor.

### Converted scripts

Each converted script is a new file, `<original name>_snippets.cs`, next to the untouched original, so both appear in MOTMaster's list. Its constructor is the original's, verbatim, plus any of the parameter lines above. Its pattern methods call `SetTimeline()` and then the snippets in time order. Lines that no snippet reproduces exactly (cameras, shutters, transport, one-off stages, the end-of-shot YAG shots) stay in the script, unchanged except that their times now come from the timeline.

Converted (23): `1. LambdaRecap`, `2.a DDSBlueMOT`, `2.b DDSBlueMOT_recap - Copy`, all three `3.*`, all six `4.*`, all three `5.*`, `6. OP_transfer_to_tweezer_coils_back`, all three `7.*`, `8. OP_transfer_to_tweezer_coils_blueMOT _load_tweezer`, `BX_BMOT_OP_magtrap`, `BX_OP_lambda`, `BX_OP_magtrap`.

Not converted (3):

- `0. DDSFourSidebands5W_noDDS` and `BX_spectroscopy`: their load stages differ from `LoadMOT` in too many ways (bX lock block, repump and shutter timings, TCool value, DDS parameter names), and nothing else applies.
- `TweezerRESONANTlMAGE`: broken today. Its constructor calls `LoadGlobalParameters()` and then replaces `Parameters` with a new dictionary, so it fails on `MoveTime`.

Behaviour changes in the converted scripts, all deliberate:

- The six scripts that never pulsed `DDS_Analog_Trg` now do (decision 3): `1. LambdaRecap`, `3. MagTrap_withBM`, `4. BM_move_coils_test`, `4. BlueMOT_transport_test`, `4. TCoil_test_back_forth_movement`, `4. TCoil_test_no_movement_1.5secHold`.
- In the 5.* scripts, the tweezer-coil hold value and the transfer coils' hand-back value now read `tMagTrapField` and `TransferCoilsOffValue`, instead of repeating the typed-in number. This is the same at default values.

Everything else, including what look like bugs, is reproduced exactly. Worth a look:

- `1. LambdaRecap`, `4. TCoil_test_back_forth_movement` and `4. TCoil_test_no_movement_1.5secHold` pulse `cafOptPumpingAOM`, which is not a channel in `MoleculeMOTHardware.cs`, so they fail to build in MOTMaster, before and after conversion. The pulse is where `DDS_Analog_Trg` normally goes and is commented "DDS trigger" in the 4.* ones.
- `1. LambdaRecap` and `2.b`: the background camera trigger and the bX shutter length count `TCLBlockStart` twice.
- 7.* and 8.*: the MOT shutters close at the tweezer blue-MOT (or red-MOT) ramp end + `BlueMOTDuration` + 2000, where `tBlueMOTDuration` (or `RedMOTDuration`) looks meant.
- `6.` and `BX_BMOT_OP_magtrap`: the magtrap uses `BlueMOTField`; the `MagTrapField` the script sets is never read.
- `3. Mag_trap_density_check` works out `ShutterEdgeDur` in its constructor, so scanning the durations it depends on does not move the shutters.
- `BX_OP_magtrap`: the MOT-shutter pulse lengths leave out `LambdaCoolingDuration`.
- `4. TCoil_test_no_movement_1.5secHold_BMOT_image` and `BX_spectroscopy` never call `LoadGlobalParameters()`.

### How the conversions were checked

There is no compiler for MOTMaster scripts on the laptop, and the cafmot PC is read-only for this work, so the check ran locally with a small .NET 8 program (kept out of the repo). It:

1. Compiles a script with the snippets it uses, as C# 5 (what MOTMaster's runtime compiler accepts), using the real `ScriptSnippets` code to find them.
2. Builds the patterns with DAQ's real `PatternBuilder32`, `AnalogPatternBuilder` and `MOTMasterScript` code, and a channel table read from `MoleculeMOTHardware.cs`.
3. Compares old and new: every digital channel's edges, every analog channel's values, every DDS event's time and values, and the saved parameters.

It compares at default parameters, then 500 times with random subsets of the parameters scaled by 0.5 to 1.5, as a scan would set them. A run counts when both scripts build, or both fail in the same way. Every converted script is identical in every such run, apart from `DDS_Analog_Trg` in the six scripts above. A deliberate one-tick change in a snippet is caught.

What this cannot check: MOTMaster's real compiler and assembly loading, the NI hardware and the DDS card itself. `verify-scripts.ps1` on PH-BONESAW covers the first, after MOTMaster is rebuilt.

## Still to do

- Rebuild MOTMaster (CaF) on PH-BONESAW and run `verify-scripts.ps1`. Then try the `_snippets` versions against the originals on the experiment.
- Once a converted script is trusted, delete the original and drop the `_snippets` suffix.
- Candidate snippets the conversion turned up:
  - the end-of-shot YAG compensating shots;
  - the MOT shutter pulses (`BlueMOTEnd` − 1600 / 600 / 800);
  - a recapture/imaging stage;
  - hand-back from the tweezer coils to the MOT coils;
  - a DDS-only lambda/imaging event.
- Imaging and camera triggers, which vary the most, are still script lines.
