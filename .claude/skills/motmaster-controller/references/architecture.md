# MOTMaster architecture

Core classes, in `MOTMaster/` unless noted. This covers what actually runs — see
"Dead code" at the bottom for files that look real but aren't.

## The pieces

| Class | File | Role |
|---|---|---|
| `Runner` | `Runner.cs` | `Main()`. Opens a `.NET Remoting` TCP channel on port 1187 exposing the `Controller` as `controller.rem` (this is how Python scan scripts drive a run), then calls `controller.StartApplication()`. |
| `Controller` | `Controller.cs` | Everything: owns the hardware handles, compiles scripts, builds patterns, runs them, saves data. `MarshalByRefObject` so it's remotable. |
| `ControllerWindow` | `ControllerWindow.cs` / `.Designer.cs` | The WinForms shell. Thin — every button/menu handler is a one-line call into `Controller`. |
| `MOTMasterScript` | `MOTMasterScript.cs` | Abstract base every pattern script derives from. `GetDigitalPattern()`, `GetAnalogPattern()`, `GetAnalogStatic()` (virtual, defaults to empty), `GetDDSPattern()` (virtual, defaults to empty, `#if DDS` only). |
| `MOTMasterSequence` | `MOTMasterSequence.cs` | Plain bag: `DigitalPattern` (`PatternBuilder32`), `AnalogPattern` (`AnalogPatternBuilder`), `AnalogStatic` (`AnalogStaticBuilder`), `DDSPattern`. Built by `MOTMasterScript.GetSequence()`. |
| `PatternBuilder32` / `PatternBuilder32SingleBoard` | `DAQ/PatternBuilder32*.cs` | Digital pattern builder. Looks up each named channel's board/bit via `Environs.Hardware.DigitalOutputChannels`. |
| `AnalogPatternBuilder` / `AnalogPatternBuilderSingleBoard` | `DAQ/AnalogPatternBuilder*.cs` | Analog pattern builder (ramps, pulses). Same board-routing pattern. |
| `AnalogStaticBuilder` | `DAQ/AnalogStaticBuilder.cs` | Analog channels that hold one value for the whole shot rather than following a pattern. |

Script authoring API (what a `.cs` script in `*MOTMasterScripts/` actually calls) is in
[script-authoring.md](script-authoring.md).

## Where a script lives and how it's found

Never prebuilt. `Controller.ScriptLookupAndDisplay()` lists every `*.cs` file directly in
`Environs.FileSystem.Paths["scriptListPath"]` (per-machine, e.g.
`C:\ControlPrograms\EDMSuite\MoleculeMOTMasterScripts` on PH-BONESAW — see
`DAQ/PHBonesawFileSystem.cs`) into the combo box. **Subfolders are never scanned** —
`OLD`, `OldDDS`, `preJuly26`, `ODT_scripts_Aug26`, `Spectroscopy_Aug26` etc. hold retired
scripts that the GUI cannot even select.

## Compilation: the part that trips people up

`Controller.compileFromFile()` (bottom of `Controller.cs`) uses `CSharpCodeProvider` /
`CodeDomProvider.CompileAssemblyFromFile` to compile the selected `.cs` file **at the
moment it's selected**, referencing only:

1. `MOTMasterExePath` + `MotMaster.exe` itself (so the script can derive from
   `MOTMasterScript` and use `MOTMaster` types),
2. `DAQ.dll`,
3. whatever is in `Environs.FileSystem.Paths["AdditionalMOTMasterAssemblies"]` — on
   PH-BONESAW/CaF this is a one-item list containing `SpectrumDDS.dll` only.

There is **no MoleculeMOTMasterScripts.csproj build in this loop at all.** That project
exists purely as an IDE convenience (open the folder as a project, get IntelliSense) and
has no `CaF` build configuration — trying to `msbuild` it directly fails with `MSB4126`.
It is never invoked by anything real.

**Consequence for editing scripts:** the only trustworthy "does this still compile" check
is reproducing `compileFromFile`'s exact reference set, which is what
[`verify-scripts.ps1`](../verify-scripts.ps1) does — see the top-level SKILL.md.

The compiled script is loaded with `Assembly.LoadFrom` + `Activator.CreateInstance`,
finding whichever class in the file `IsClass == true` (in practice: exactly one, named
`Patterns` by convention in every current script).

## Machine/experiment dispatch

`DAQ/EnvironsHelper.cs` switches on `Environment.MachineName` to decide which
`Hardware` subclass and which `FileSystem` subclass get wired up as `Environs.Hardware`
/ `Environs.FileSystem`. This machine (`PH-BONESAW`) gets `MoleculeMOTHardware` +
`PHBonesawFileSystem`. Every other experiment on the suite (AlF, EDM variants, Rb
tweezer, buffer gas, ...) has its own pair, entirely separate hardware channel maps and
paths — **don't assume anything channel-related generalizes across experiments.**

`PHBonesawFileSystem.Paths` (the keys `Controller.cs` reads at startup):

| Key | Value on PH-BONESAW |
|---|---|
| `scriptListPath` | `MoleculeMOTMasterScripts\` |
| `MOTMasterExePath` | `MOTMaster\bin\CaF\` |
| `daqDLLPath` | `DAQ\bin\CaF\daq.dll` |
| `MOTMasterDataPath` | Box-synced folder — where completed runs are saved/zipped |
| `AdditionalMOTMasterAssemblies` | `[ SpectrumDDS.dll ]` |
| `HardwareClassPath` | Path to `MoleculeMOTHardware.cs` itself (used only for the saved run's metadata, not loaded) |

**Known harmless quirk:** `Controller.cs` reads the exe-path key as `"MOTMasterEXEPath"`
(capital EXE); every `FileSystem` subclass in the repo defines it as `"MOTMasterExePath"`.
`Paths` is a `Hashtable` (case-sensitive, returns `null` on a miss rather than throwing),
so that particular lookup always misses and `motMasterPath` ends up as just
`"//MotMaster.exe"` with no directory. Scripts still compile fine in practice — the
.NET Framework's C# compiler evidently falls back to resolving the bare assembly name
against the running process's own base directory. Leave this alone unless you're
specifically chasing a reference-resolution failure; it's cosmetic, not functional.

## Hardware channel registration

`DAQ/Hardware.cs` (base class) exposes `AddDigitalOutputChannel(name, device, port,
line)`, `AddAnalogOutputChannel(name, physicalChannel[, rangeLow, rangeHigh])`, etc.
`MoleculeMOTHardware.cs` (`DAQ/MoleculeMOTHardware.cs`) calls these in its constructor to
build the name → board/bit map that `PatternBuilder32`/`AnalogPatternBuilder` look up by
channel name. `DigitalOutputChannel.BitNumber` comes from `port`/`line` (`line + 8*port`,
see `PatternBuilder32.ChannelFromNIPort`).

**Two names can legitimately share a bit.** `Controller.GenerateListOfDigitalChannelNames`
(used only by the pattern viewer, to label channels) merges same-bit duplicates with
`"/"` rather than throwing, because `MoleculeMOTHardware.cs` really does register two
names on `digitalPatternBoardAddress` port 1 line 6: `"rbCoolingAOM"` (line ~116) and the
commented-`//broken!` `"broken1"` (line ~146). If you add a new channel, **grep for the
`(device, port, line)` triple first** — the compiler won't catch a collision, and the
pattern generator itself will just OR both names' edges onto one physical bit.

## A run, end to end (`Controller.Go`)

1. `prepareScript` compiles + instantiates the script, optionally overriding `Parameters`
   from a dictionary (this is how a Python scan injects swept values after compilation).
2. `getSequenceFromScript` calls the script's `GetSequence()`.
3. If DDS is in use (`#if DDS`, CaF only — see the `spectrum-dds` skill), `armDDS` loads
   the DDS pattern onto the Spectrum card via `SpectrumDDSController` over remoting on
   port 1818, before anything else happens.
4. `buildPattern` calls `BuildPattern` on the digital/analog builders, turning the edge
   list into a sample array of length `Parameters["PatternLength"]`.
5. Loop (`RunUntilStopped` or a fixed iteration count): `runPattern` → `initializeHardware`
   (configure the NI boards for this sequence) → `run` (write patterns to the boards,
   trigger) → wait for `pgMaster.TaskRunning` to clear → `releaseHardware`.
6. Save (`ioHelper.StoreRun`) unless `saveEnable` is off: zips the script `.cs`, the
   resolved parameters, camera attributes, hardware report, and any images into
   `MOTMasterDataPath\yyyy\MMM\dd\`.

**Per project safety rules, do not exercise this path.** Nothing in this skill launches
MOTMaster or calls `Run()`/`Go()` — see the top-level SKILL.md.

## Dead code — don't let it waste your time

The `MOTMaster/` folder has several files that look like alternate implementations but
are not compiled or not wired to anything. Confirmed by checking `MOTMaster.csproj`'s
`<Compile>` list and by grepping for callers:

- `Controller - Copy.cs`, `Controller - Copy (2).cs`, `Controller__.cs` — stray backups
  of `Controller.cs`, not in the `.csproj` at all. If you're grepping for something and
  three near-identical hits come back, check which file is actually compiled.
- `PreviewWindow.cs` / `.Designer.cs` / `.resx` — **is** compiled, but nothing
  instantiates `PreviewWindow` anywhere. It's an older, apparently-abandoned attempt at
  the same idea as the `PatternViewer` (this skill's pattern visualiser). Don't confuse
  the two: `PatternViewer` is the one wired to the "View Pattern" button.
- `DAQ/AnalogPatternBuilder2.cs`, `AnalogPatternBuilderUntouched.cs`,
  `PatternBuilder32 - Copy.cs` — duplicate classes with the same name as the real ones;
  not in `DAQ.csproj`'s `<Compile>` list, so they can't even coexist at compile time with
  the real ones in the same build. Ignore them.

If you're not sure whether something is live, check `<Compile Include=...>` in the
relevant `.csproj` first, then grep for actual callers — don't assume a file's presence
means it runs.
