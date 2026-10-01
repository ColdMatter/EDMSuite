# Codebase: driver, GUI and MOTMaster integration

## The non-negotiable rule: `#if DDS`

EDMSuite is shared by experiments that have no DDS — Lattice, AlF, EDM,
Sympathetic, Tweezer. **Nothing may break for them.** Every MOTMaster-side change
sits behind the `DDS` compile symbol, which only the `CaF|AnyCPU` configuration
defines (`MOTMaster/MOTMaster.csproj`). Their builds come out byte-for-byte
unaffected, and the regression check is to build one and confirm no `SpectrumDDS`
artefact appears in its output directory.

The `ProjectReference`s to `SpectrumDDS` and `SpectrumDDSController` in
`MOTMaster.csproj` carry `Condition="$(DefineConstants.Contains('DDS'))"` for the
same reason. In `EDMSuite.sln` both projects have an `ActiveCfg` line for every
solution configuration but a `Build.0` line only for the CaF ones.

## Layout

`SpectrumDDS/` (net461 class library, AnyCPU): `SpcmCard` P/Invoke that throws on
any non-zero return, `SpcmRegs` constants taken from `spcm_core.regs`,
`DDSChannelState`/`DDSEvent`/`DDSPattern`, `DDSCapabilities` reading
`SPC_DDS_AVAIL_*`, `DDSPatternCompiler` holding the timing model,
`DDSPatternFile` (JSON via `DataContractJsonSerializer`), `SpectrumDDSDriver`
for lifecycle, routing, priming, arm/run and the clamp, and
`DebugLogSettings` for the driver's debug-log registry key and day-boundary
log rotation (see the skill's "Debug logging" section).

`SpectrumDDSController/` (WinExe, x64, owns the card handle): `Runner` publishes on
TCP 1818 as `controller.rem`, `Controller` is the `MarshalByRefObject` remoting
surface, `MainWindow` has the Status / Pattern / Manual tabs, and
`Properties/licenses.licx` is required — see below.

`SpectrumDDS` is AnyCPU because it is pure IL and only P/Invokes; it is the *host*
that must be 64-bit, since `spcm_win64.dll` is 64-bit only. Bind by name —
`[DllImport("spcm_win64.dll")]`, which resolves from System32 — and note the x64
build exports **undecorated** names (no `_name@N`; that is 32-bit stdcall). The
project this replaced used an absolute `HintPath` to `SpcmDrv64.NET.dll` on
someone's E: drive and was unbuildable anywhere else.

Ports across the suite: **1172** hardware control and reporter, **1187** MOTMaster,
**1818** the DDS.

## The script-facing contract — do not change it

~130 MOTMaster scripts produce a `Dictionary<string, List<List<double>>>` from
`GetDDSPattern()`. Keeping it as the wire format is what lets all of them go on
working untouched. The key is an arbitrary unique event label; the value is exactly
five lists:

| index | contents | length | units |
|---|---|---|---|
| 0 | time | 1 | **ms** after the DDS trigger |
| 1 | frequency | 4 | **MHz** |
| 2 | amplitude | 4 | fraction 0–1 |
| 3 | frequency slope | 4 | **MHz/ms** |
| 4 | amplitude slope | 4 | **1/ms** |

Events are unordered in the dictionary — sort on `[0][0]`. SI conversions:
`t_s = v/1000`, `f_Hz = v*1e6`, `df_Hz/s = v*1e9`, `da_/s = v*1e3`.
`DDSPattern.FromLegacyDictionary` / `ToLegacyDictionary` do this and round-trip
exactly. A few scripts `return null`, so handle it.

t = 0 is the pulse on `DDS_Analog_Trg` (`/PXI1Slot4` port 0 line 2), which the
scripts fire at the Q-switch instant. One line fans out to both the DDS Trg0 input
and the AO board start triggers — which is why driving it is not something to do
casually.

`DDSPatternBuilder` in `MOTMaster/MOTMasterScript.cs` replaces the `addDDSPattern`
helper copy-pasted verbatim into every script. Same units: time in 10 µs pattern
ticks (`time / 100` gives ms), slopes given per tick and stored per ms (hence ×100).

## MOTMaster hook points

Four places in `MOTMaster/Controller.cs`, all `#if DDS`:

1. **Field + proxy** in `StartApplication()` —
   `Activator.GetObject(typeof(SpectrumDDSController.Controller), "tcp://127.0.0.1:1818/controller.rem")`.
   Nothing connects until first use, so MOTMaster still starts with the DDS
   controller closed.
2. **Load and arm** after `getSequenceFromScript()` — `PrepareForNewPattern()`,
   set `patternList`, `StartRepetitivePattern()`. Skipped when the script has no
   DDS pattern.
3. **`WaitUntilArmed(1.0)` before `runPattern(sequence)` in *both* iteration
   loops** — run-until-stopped and fixed-iteration. Missing it in one loop is an
   easy and invisible mistake. It is a busy-spin (`Thread.Sleep(0)`) on
   `SPC_DDS_STATUS`, so it is also the loop that turns a high debug-log level
   into per-shot overhead — see the skill's "Debug logging" section.
4. **Report after the loop** — sent, fired, missed, card count.

`MOTMaster/MMDataIOHelper.cs` `storeDDSPattern` writes `*_ddsPattern.json` into the
run zip beside the digital and analog patterns.

The remoting surface `Controller` must keep exposing: `PrepareForNewPattern()`,
settable `patternList`, `StartRepetitivePattern()`, `WaitUntilArmed(double)`,
`PatternsFired`, `GetCardTriggerCount()`.

Concurrency: `SpcmCard` serialises individual calls so a status poll cannot land
inside another thread's, but a *sequence* of writes is not atomic — queueing a
pattern is over a hundred writes that must not interleave with a manual `EXEC_NOW`.
That is what `Controller.CardLock` is for. Anything that waits on the card takes an
abort callback so a stop request does not block for seconds; without that, a
stop-then-start can leave two threads arming the same card.

## Building

MSBuild lives at
`C:\Program Files (x86)\Microsoft Visual Studio\2019\Enterprise\MSBuild\Current\Bin\MSBuild.exe`.

```powershell
& $msbuild MOTMaster\MOTMaster.csproj /t:Build /p:Configuration=CaF /p:Platform=AnyCPU /v:minimal /nologo
```

**MOTMaster and MoleculeMOTHardwareControl are often running** and lock
`bin\CaF\DAQ.dll`, which fails the copy step even though compilation succeeded.
Do not kill them. To verify a build, redirect the output:
`/p:OutputPath="$env:TEMP\mmbuild\"`. A real build into `bin\CaF` needs the user to
close those apps.

`MoleculeMOTMasterScripts.csproj` compiles only `Properties\AssemblyInfo.cs`; the
scripts are not in the Compile group and the project exists for IntelliSense.
MOTMaster compiles scripts at runtime with `CSharpCodeProvider`, referencing
`MotMaster.exe`, `daq.dll` and everything in
`Environs.FileSystem.Paths["AdditionalMOTMasterAssemblies"]` — set for PH-BONESAW
(the cafmot MOTMaster PC) in `DAQ/PHBonesawFileSystem.cs`, pointing at
`SpectrumDDS.dll`. **A path in that list that does not exist makes every script
compile fail** — that is how the deleted old driver broke the whole experiment.

## Measurement Studio (NI) UI gotchas

The GUI uses `ScatterGraph`, not `WaveformGraph`: pattern events are unevenly
spaced in time and `WaveformGraph` plots against a sample index.

- **`Properties/licenses.licx` is required.** Without it the control throws
  `LicenseException` at construction. SDK-style projects do not search the
  `AssemblyFoldersEx` registry keys older projects rely on, so the NI references
  are pointed at the GAC copies (19.0.45.49154, public key `4febd62461bf11a4` —
  the version every other project here uses, and the one the licx must name).
- **A `ScatterGraph` built in code has no axes at all.** The designer normally adds
  them, so `XAxes[0]` throws until you `Add` an `XAxis` and `YAxis` yourself.
- **`NullReferenceException` at `NationalInstruments.Internal.SlcpHelper.d()` is
  benign.** NI's licensing helper throws and catches it internally, once, at
  startup. It only surfaces because Visual Studio is set to break on thrown
  exceptions. Untick `NullReferenceException` in Exception Settings, or enable Just
  My Code.
- `MainWindow.Designer.cs` is hand-written and will not round-trip through the VS
  form designer — it uses property initialisers, builds repeated rows in a loop, and
  sets `SplitterDistance` in `OnLoad` because it cannot be set before the container
  is sized.

## Decisions that are the user's, not yours

The amplitude clamp is the one that bites: each channel defaults to 0.25, agreed
for unsupervised bench work, but `0. DDSBlueMOT.cs` asks for **0.6 on DDS2 and 0.35
on DDS3**, so real patterns are rejected until someone raises those two. It is per
channel — `SpectrumDDSDriver.MaximumAmplitudes[]`, set via `SetMaximumAmplitude`
or the clamp column on the Manual tab — but what the production values should be is
a physics decision, not a coding one.

Similarly open, and worth asking rather than assuming: per-channel output level
(500 mV is an arbitrary test value), trigger termination and logic level, whether
the card should hold the last event's state between shots or return to the first,
and whether to queue several shots ahead instead of re-arming per shot.

The GUI remembers the clamps, the last manual frequency and amplitude per channel,
and the output level in `%APPDATA%\SpectrumDDSController\settings.json`
(`ControllerSettings`). Restoring them fills the spinners only — no tone is applied
until Apply now is pressed — and the output stages are enabled after a successful
open, which emits nothing because `Open()` leaves every core silenced.
