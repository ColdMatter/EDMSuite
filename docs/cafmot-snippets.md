# CaF MOTMaster snippets: findings so far and decisions

Branch `cafmot-snippets`, 2026-10-22. Nothing has been built yet. This note covers what we found in the current scripts, and what has been decided, including how snippets get their timing.

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
- A snippet does not know what comes before or after it, so the same snippet can go anywhere in a shot, and the order of the snippet lines does not matter.
- The cost is that every script writes out its timeline: the 24 scripts that share the load-to-lambda-cooling stages each carry the same lines for them.
- The snippet calls are written three times, once per pattern, as scripts already do today.

### Rejected: each snippet defines its own times

The alternative was for each snippet to set its own times, e.g. `BlueMOT` setting `BlueMOTStartTime = LambdaCooling1EndTime`, so that a script is only a list of snippet calls. It was rejected because each snippet then names the stage before it, so a stage cannot be moved, dropped or used in a different place without a different snippet; gaps between stages such as `OPGap` live inside snippets; and the order of the snippet lines matters.

## Still to discuss after this

- Which scripts get converted first, and whether all 26 do.
- How the same stage is reused in a different place, e.g. a blue MOT in the tweezer chamber, which uses `TweezerCoils` and `tBlueMOTField` instead of `MOTCoilsCurrent` and `BlueMOTField`. Since a snippet only gets `(p, parameters)`, the channel and field cannot be passed in; this is the same problem as running lambda cooling twice.
- Imaging/camera triggers, which vary the most.
- How converted scripts are checked against today's patterns before anyone runs them.
