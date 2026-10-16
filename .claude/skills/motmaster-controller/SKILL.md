---
name: motmaster-controller
description: Editing or extending the MOTMaster controller and pattern-generator plumbing — Controller.cs, ControllerWindow, MOTMasterScript, PatternBuilder32/AnalogPatternBuilder, hardware channel registration (AddDigitalOutputChannel), the runtime script-compilation model, globalParameters.json / LoadGlobalParameters, the pattern visualiser ("View Pattern"), the parameter-file editor ("Edit parameter file"), or checking whether a MoleculeMOTMasterScripts edit will actually run (verify-scripts.ps1). Applies to any experiment on the suite (AlF, EDM variants, Rb tweezer, buffer gas, ...) for the shared MOTMaster/DAQ plumbing, but the concrete details here — machine name, Hardware/FileSystem subclass, build configuration, script folder, globalParameters.json — are all cafmot/PH-BONESAW's; adapt them per-experiment as this skill itself explains. Use this whenever the task touches MOTMaster's C# internals or a *MOTMasterScripts/*.cs pattern script, or answers a question about how MOTMaster is wired together — building it, adding a channel, adding a shared parameter, editing the GUI, why a script won't compile, or what a symptom like "MSB4126" or a duplicate-channel exception means.
---

# MOTMaster controller

MOTMaster is the WinForms app that compiles a `.cs` pattern script on the fly and drives
NI digital/analog pattern generators from it. This skill is the map of how `Controller`,
`ControllerWindow`, `MOTMasterScript`, and the pattern builders fit together, so you don't
have to re-derive it from scratch each session.

## Keeping this skill current

This skill is a living map, not a one-time snapshot. Whenever work in this area produces
something worth remembering — you learn how some piece actually works that isn't written
down here, a new feature gets added (to MOTMaster itself, or to the pattern
visualiser/parameter editor), something documented here turns out to be wrong, or the
user corrects a claim this skill makes — **ask the user whether this skill (SKILL.md or
the files under `references/`) should be updated to reflect it**, rather than letting the
knowledge live only in that conversation. Don't update it silently or unprompted; ask
first, since the whole point is that it stays accurate rather than accumulating
unreviewed edits.

## Scope: written for cafmot, not universal

This skill was written from the **cafmot** experiment, on machine **PH-BONESAW**. The
*shared* plumbing it describes — `Controller`, `ControllerWindow`, `MOTMasterScript`, the
pattern builders, the runtime script-compilation model, the two generic GUI features —
is genuinely shared: `MOTMaster/` and `DAQ/`'s base classes are one codebase used by every
experiment on the suite. But most of the **concrete specifics** are cafmot's alone:

| This skill says | Actually means, specifically |
|---|---|
| "this experiment" / "this machine" | cafmot / PH-BONESAW |
| `MoleculeMOTHardware.cs` / `PHBonesawFileSystem.cs` | cafmot's `Hardware`/`FileSystem` subclasses — every other experiment has its own pair, chosen by `EnvironsHelper.cs`'s machine-name switch |
| `CaF` build configuration | cafmot's config name; another experiment builds under its own (see the `Configuration` conditions in `MOTMaster.csproj`/`DAQ.csproj`) |
| `MoleculeMOTMasterScripts/` | cafmot's script folder; e.g. AlF's is `AlFMOTMasterScripts/` |
| `globalParameters.json` / `LoadGlobalParameters()` | a cafmot-specific convention added to the shared `MOTMasterScript` base class — another experiment's scripts may not use it at all, or may have their own file |
| `#if DDS` / Spectrum DDS integration | cafmot-only (only the `CaF` config defines `DDS`) — irrelevant to any experiment without that card |
| channel names, board/port/line numbers | cafmot's `MoleculeMOTHardware.cs` only — meaningless for another experiment's hardware map |

**If asked to do something for a different experiment** (AlF, an EDM variant, Rb tweezer,
buffer gas, ...): use this skill for the shared `Controller`/`ControllerWindow`/
`MOTMasterScript`/pattern-builder mechanics and the general working style (verify via
compilation, not by launching the GUI), but go find that experiment's own `Hardware`/
`FileSystem` subclass, build configuration, and script folder rather than assuming
cafmot's apply. `verify-scripts.ps1` already takes `-Configuration`/`-ScriptFolder`
params for exactly this. If it's unclear which machine/experiment you're on, check
`Environment.MachineName` against `DAQ/EnvironsHelper.cs`'s switch statement, or ask.

## Safety — read first

**Never run a MOTMaster script, never fire the experiment, never interact with
experiment hardware.** That's the user's action, at a time of their choosing — see the
repo's top-level `CLAUDE.md`. Concretely: don't call `Controller.Run()`/`Go()`/`RunReplica()`,
don't click Run/Go in a live GUI, don't launch `MOTMaster.exe` and click through it.

What **is** safe and needs no permission, because none of it configures or triggers a
board:
- Reading/editing any `.cs` file under `MOTMaster/`, `DAQ/`, or `*MOTMasterScripts/`.
- Building `MOTMaster.csproj` / `DAQ.csproj` with `msbuild`.
- Compiling a pattern script standalone with `csc.exe` (see below) — this exercises
  `Controller.compileFromFile`'s exact logic without instantiating `Controller` or
  touching `Environs.Hardware`.
- Editing `globalParameters.json` or any parameter file directly.

If a task needs the pattern visualiser or parameter editor actually clicked through in a
running GUI, that's the user's to do — hand them what to check instead of launching it
yourself.

## Orientation

| Where | What |
|---|---|
| `MOTMaster/Controller.cs` | Everything: hardware ownership, script compile/load, run loop, save. |
| `MOTMaster/ControllerWindow.cs`/`.Designer.cs` | Thin WinForms shell — handlers are one-line calls into `Controller`. |
| `MOTMaster/MOTMasterScript.cs` | Abstract base every pattern script derives from; `LoadGlobalParameters()` lives here. |
| `MOTMaster/PatternViewer.cs`, `ChannelDescriptor.cs` | The pattern visualiser ("View Pattern"). |
| `MOTMaster/ParameterWindow.cs`, `ParameterFileManager.cs`, `ParameterEntry.cs`, `ParameterGroup.cs` | The generic parameter-file editor ("Parameters → Edit parameter file") — grouped JSON parameter files. |
| `DAQ/Hardware.cs`, `DAQ/MoleculeMOTHardware.cs` | Channel registration (`AddDigitalOutputChannel` etc.) for this experiment. |
| `DAQ/PatternBuilder32*.cs`, `DAQ/AnalogPatternBuilder*.cs`, `DAQ/AnalogStaticBuilder.cs` | Digital/analog pattern builders scripts call. |
| `DAQ/EnvironsHelper.cs`, `DAQ/PHBonesawFileSystem.cs` | Per-machine dispatch to `Environs.Hardware`/`Environs.FileSystem`. |
| `MoleculeMOTMasterScripts/*.cs` (top-level only) | The actual pattern scripts for this experiment. |
| `MoleculeMOTMasterScripts/globalParameters.json` | Shared parameter defaults (grouped), loaded + flattened via `LoadGlobalParameters()`. |
| `.claude/skills/motmaster-controller/verify-scripts.ps1` | Compiles + instantiates + pattern-builds every top-level script without opening MOTMaster — "will MOTMaster accept this edit". See Verifying a change below. |

Read [references/architecture.md](references/architecture.md) first for how it all
fits together (compilation model, machine dispatch, a run end-to-end, and the dead code
to ignore). Then:
- [references/script-authoring.md](references/script-authoring.md) — writing/editing a
  pattern script: the `PatternBuilder32`/`AnalogPatternBuilder` API, `LoadGlobalParameters`,
  adding a shared parameter.
- [references/gui-features.md](references/gui-features.md) — the pattern visualiser and
  parameter editor: how they're wired, what's safe to extend.

For the Spectrum DDS card (`#if DDS`, CaF only) see the separate `spectrum-dds` skill —
this skill covers the MOTMaster side of that integration only in passing.

## Verifying a change

MOTMaster never prebuilds the scripts project — every script is compiled at runtime
against `MOTMaster.exe` + `DAQ.dll` + whatever's in `AdditionalMOTMasterAssemblies` (see
architecture.md). So there are two independent things to verify, and neither requires
opening the GUI:

**1. `MOTMaster.csproj`/`DAQ.csproj` itself still builds**, after touching `Controller.cs`,
`ControllerWindow`, `MOTMasterScript.cs`, or anything under `DAQ/`:

```powershell
& "C:\Program Files (x86)\Microsoft Visual Studio\2019\Enterprise\MSBuild\Current\Bin\amd64\MSBuild.exe" `
    "C:\ControlPrograms\EDMSuite\MOTMaster\MOTMaster.csproj" /p:Configuration=CaF /p:Platform=AnyCPU /nologo /v:minimal
```

Ran clean this session (only pre-existing NI-assembly-version warnings, nothing new):
`MOTMaster -> C:\ControlPrograms\EDMSuite\MOTMaster\bin\CaF\MOTMaster.exe`.

**2. Every top-level pattern script still compiles, instantiates, and builds a runnable
pattern** — after touching a script, adding a hardware channel, or changing
`globalParameters.json` / `LoadGlobalParameters()`. This is a real check, not just "does
it compile": it also catches a typo'd channel name or a pattern that doesn't fit
`PatternLength`, which a bare compile can't. Run it with **`verify-scripts.ps1`** (in
this skill folder — needs only `csc.exe`, no .NET SDK or VS Code):

```powershell
cd C:\ControlPrograms\EDMSuite
powershell -ExecutionPolicy Bypass -File .claude\skills\motmaster-controller\verify-scripts.ps1
```

It compiles `ScriptSmokeTest.cs` together with `ScriptCompileCheck.cs` (both in this
folder — the latter reproduces `Controller.compileFromFile` + `loadScriptFromDLL` +
`GetSequence()` + `buildPattern` for each script) via `csc.exe` and runs it, reporting a
pass/fail per file. Takes `-Configuration`/`-ScriptFolder` for a different experiment's
scripts.

Last run: **15 of 19 scripts pass, 4 fail** — all four fail for the same
real reason (`"cafOptPumpingAOM"` isn't a registered channel name in
`MoleculeMOTHardware.cs`), not a tooling problem. See the failure text for the four
scripts affected. Run **build step 1 first** if you've touched `MOTMaster.csproj`/
`DAQ.csproj` — the script check references their `bin\CaF\` output, so a stale build
there gives a false pass or a false fail unrelated to your edit.

**`verify-scripts.ps1` must run under PowerShell, not Git Bash/the Bash tool.**
`csc.exe`'s `/nologo`-style flags get mangled into bogus MSYS paths when invoked through
Bash — this cost real time to diagnose once already. Use the PowerShell tool.

## Gotchas

- **`MoleculeMOTMasterScripts.csproj` is a trap.** It looks like the real build unit for
  scripts (it's a `.csproj`, it's in the `.sln`) but has no `CaF` configuration and isn't
  part of any real build or deploy path — it exists only so an IDE can index the folder.
  `msbuild`-ing it directly fails with `MSB4126`. Verify scripts with
  `verify-scripts.ps1`, not by building this project.
- **Subfolders under `*MOTMasterScripts/` are invisible to MOTMaster.** `ScriptLookupAndDisplay`
  only lists files directly in the folder. `OLD`, `OldDDS`, `preJuly26`, etc. are dead —
  don't "fix" a script in there expecting it to matter, and don't scan them when doing a
  sweep across "all the scripts."
- **A hardware channel collision doesn't fail loudly.** Two names on the same
  `(device, port, line)` compile fine, register fine, and just both toggle the same
  physical bit — the pattern viewer papers over it by joining the names with `/`
  (see architecture.md). Grep for the triple before adding a channel.
- **An unregistered channel name doesn't fail loudly either — it's a `NullReferenceException`
  two frames away, with no channel name in the message.** `DigitalOutputChannels`/
  `AnalogOutputChannels` are `Hashtable`s (same class of gotcha as the `MOTMasterEXEPath`
  case in architecture.md), so `Environs.Hardware.DigitalOutputChannels[typo'dName]`
  returns `null` instead of throwing, and the crash happens one line later in
  `PatternBuilder32.GetBoard`/`AnalogPatternBuilder.GetBoard`. `ScriptCompileCheck.cs`
  recognizes this exact shape and adds a hint pointing back at the channel-name cause —
  confirmed live: this is exactly what's wrong with 4 of the 19 current
  `MoleculeMOTMasterScripts` (all reference `"cafOptPumpingAOM"`, which doesn't exist in
  `MoleculeMOTHardware.cs` — see the Verifying-a-change section).
- **`*.csproj` is globally gitignored in this repo** (`.gitignore` line 27, alongside
  `*.dll`) — every tracked `.csproj` (`MOTMaster.csproj`, `DAQ.csproj`, ...) had to be
  `git add -f`'d in at some point. Adding a
  new project (or copying one for another experiment) and forgetting `-f` means `git
  status` shows it as untracked forever and a normal `git add .` silently never picks it
  up — check `git check-ignore -v <path>.csproj` if a project you just added isn't
  showing up staged.
- **`LoadGlobalParameters()` must be called before any script-specific `Parameters[...]`
  assignment**, or the shared default silently wins over the script's intended override.
- **Stray duplicate files exist and are not compiled**: `Controller - Copy*.cs`,
  `Controller__.cs`, `PatternBuilder32 - Copy.cs`, `AnalogPatternBuilder2.cs`,
  `AnalogPatternBuilderUntouched.cs`. `PreviewWindow.cs` *is* compiled but never
  instantiated — don't confuse it with `PatternViewer`, the one actually wired up. Check
  a `.csproj`'s `<Compile>` list before trusting that a file you found is live.
- **This codebase is shared across many experiments** (see `EnvironsHelper.cs`'s
  machine-name switch) with minimal documentation. A change to `Controller.cs`,
  `ControllerWindow`, or `MOTMasterScript.cs` affects every experiment; a change under
  `MoleculeMOTMasterScripts/` or `DAQ/MoleculeMOTHardware.cs` affects only this one.
  Don't assume behavior generalizes, and don't refactor shared code beyond what the task
  needs — see the repo's `CLAUDE.md`.
