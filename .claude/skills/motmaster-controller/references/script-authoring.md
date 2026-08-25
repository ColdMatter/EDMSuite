# Writing / editing a pattern script

A script is one `.cs` file directly inside `*MOTMasterScripts\` (top-level only — see
[architecture.md](architecture.md)) containing exactly one class deriving from
`MOTMasterScript`. By convention it's named `Patterns`.

## Skeleton

```csharp
using MOTMaster;
using System;
using System.Collections.Generic;
using DAQ.Pattern;
using DAQ.Analog;

public class Patterns : MOTMasterScript
{
    public Patterns()
    {
        Parameters = new Dictionary<string, object>();
        LoadGlobalParameters();              // must come first - see below
        Parameters["PatternLength"] = 50000; // script-specific override, wins over the global default
        // ... more Parameters[...] = ... assignments ...
    }

    public override PatternBuilder32 GetDigitalPattern()
    {
        PatternBuilder32 p = new PatternBuilder32();
        p.AddEdge("qSwitch", 1000, true);
        p.AddEdge("qSwitch", 1010, false);
        return p;
    }

    public override AnalogPatternBuilder GetAnalogPattern()
    {
        AnalogPatternBuilder p = new AnalogPatternBuilder((int)Parameters["PatternLength"]);
        p.AddAnalogValue("someShim", 0, (double)Parameters["xShimBM"]);
        return p;
    }

    // GetAnalogStatic() and GetDDSPattern() are virtual with empty defaults -
    // only override if the script actually needs them.
}
```

Times are in pattern-clock ticks (10 us / tick, both boards, on this experiment — check
`config.DigitalPatternClockFrequency` / `AnalogPatternClockFrequency` if unsure for
another experiment). Channel names passed as strings must match a name registered in
`Environs.Hardware` (`MoleculeMOTHardware.cs` for this experiment) — a typo throws a
`KeyNotFoundException` at compile-then-run time (when `GetSequence()` executes), not at
`csc` compile time, since the lookup is a runtime dictionary access.

## `PatternBuilder32` (digital) — `DAQ/PatternBuilder32.cs`

| Method | Effect |
|---|---|
| `AddEdge(channelName, time, sense)` | Set channel to `sense` (bool) at `time`. This is the only real primitive — everything else composes it. |
| `Pulse(startTime, delay, duration, channelName)` | `AddEdge(true)` at `start+delay`, `AddEdge(false)` at `start+delay+duration`. Returns `delay+duration`. |
| `DownPulse(...)` | Same but inverted sense. |
| `Clear()` | Wipe the board's pattern (called between runs, not usually from a script). |

Multi-board note: a channel's board is resolved automatically from
`Environs.Hardware.DigitalOutputChannels[name].Device`, so scripts almost never need to
think about which physical board a channel lives on.

## `AnalogPatternBuilder` (analog) — `DAQ/AnalogPatternBuilder.cs`

| Method | Effect |
|---|---|
| `AddAnalogValue(channelName, time, value)` | Set channel to `value` (step) at `time`. |
| `AddAnalogPulse(channelName, startTime, duration, value, finalValue)` | Pulse to `value` for `duration`, then settle at `finalValue`. |
| `AddLinearRamp(channelName, startTime, steps, finalValue)` | Linear ramp from the current value to `finalValue` over `steps` ticks starting at `startTime`. |
| `AddPolynomialRamp(channelName, startTime, stopTime, finalValue, upperThreshold, lowerThreshold, w1, w2, w3, w4)` | Weighted polynomial ramp; see call sites in existing scripts (e.g. cooling ramps) before reusing — the weight semantics aren't documented anywhere except by example. |
| `GetValue(channelName, time)` | Read back what the pattern currently holds at `time` (for chaining ramps). |
| `SwitchOffAtEndOfPattern(channelName)` / `SwitchAllOffAtEndOfPattern()` | Force a channel to 0 at the last sample. |

## `AnalogStaticBuilder` — `DAQ/AnalogStaticBuilder.cs`

For channels that hold one value for the whole shot (not a time-varying pattern).
Override `GetAnalogStatic()` only if the script uses one; the base class default is an
empty builder.

## `LoadGlobalParameters()` — shared defaults

Added to `MOTMasterScript` (`MOTMaster/MOTMasterScript.cs`) to stop 19 near-identical
scripts from hand-copying the same 90+ constants. It reads
`<scriptListPath>\globalParameters.txt` (tab-delimited `Name\tValue\tType`, same format
`ParameterFileManager` uses, editable through **Parameters → Edit parameter file** in the
GUI) and writes every entry into `Parameters`.

**Call it immediately after `Parameters = new Dictionary<string, object>();` and before
any script-specific `Parameters[...]` assignment** — later assignments in the
constructor overwrite what the global file loaded, which is exactly how a script
overrides one shared default without touching the shared file. If you call it after
script-specific assignments, the global value wins instead and silently clobbers the
script's intended override.

**Adding a new shared parameter:** add a row to `globalParameters.txt`
(`Name\tValue\tType`, `Type` is `System.Int32` or `System.Double`), then remove the
matching `Parameters["Name"] = ...;` line from every script that used that exact value.
Leave the line in place (untouched) in any script that needs a different value — the
per-script assignment still runs after `LoadGlobalParameters()` and wins. Re-run
[`verify-scripts.ps1`](../verify-scripts.ps1) afterward: a typo in the new row's `Type`
column (anything `Type.GetType()` can't resolve) silently falls back to `double` in
`ParameterFileManager.ReadFile`, which then throws an `InvalidCastException` inside
`Convert.ChangeType` at compile-then-run time if a script unboxes it with `(int)` —
the verify script won't catch that (it's a runtime cast, not a compile error), so a
quick check is to cross-reference the new parameter's cast sites by hand.

## `DDSPatternBuilder` — `#if DDS` only (CaF configuration)

Covered in the `spectrum-dds` skill (`.claude/skills/spectrum-dds/`), not here — read
that skill's `references/codebase.md` before touching `GetDDSPattern()` or any
`AddEvent(...)` call. In short: replaces the old copy-pasted `addDDSPattern` helper,
same units (MHz, fractional amplitude, ticks), gated so non-CaF experiments never see it.
