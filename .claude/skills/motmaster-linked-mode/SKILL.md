---
name: motmaster-linked-mode
description: Use when working on or with MOTMaster linked mode - one script driving two MOTMasters (a primary and a secondary, e.g. PH-BONESAW + RbTweezer), the "Linked" checkbox, "Take back control", SecondaryTriggerDelay, LinkRouting, LinkedPrimary, LinkedScriptBuilder, _link.txt, or an error from a linked run. Not needed for ordinary single-machine MOTMaster work (use motmaster-controller for that).
---

# MOTMaster linked mode

## What it is for

One MOTMaster script drives the pattern generators of **two** machines, as if they were
one experiment. The **primary** (cafmot: PH-BONESAW) runs the script and fires the
**secondary** (cafmot: RbTweezer, `tcp://10.59.3.2:1187`) with a TTL trigger, so both
patterns play together. Run as usual from the primary's GUI or from Python
(`SetScriptPath` / `Go`).

It is **opt-in per machine**: only a machine whose Hardware class declares a
`MOTMasterLinkConfig` gets the feature. Every other machine, and every run with Linked
unticked, behaves exactly as before. Most MOTMaster users never touch it, and changes here
must keep it that way.

Safety: running scripts, starting MOTMaster and handling hardware are the user's job (see
the repo `CLAUDE.md`).

## How it works

1. **Link.** The primary's Hardware class has a `MOTMasterLinkConfig`, so its window
   shows a *Linked to ...* checkbox. Ticking it calls `SetLinked(true)`, which claims the
   secondary over .NET Remoting (`ClaimLink`). The secondary shows a gold banner and
   refuses local runs until the link is released, or until its user presses **Take back
   control** (only allowed between runs).
2. **Build both sides from one script.** On `Go`, the primary runs `GetSequence()` inside
   a `LinkRouting` context: the pattern builders emit only the primary's own channels.
   Every channel name must exist on exactly one machine. MOTMaster adds the trigger
   pulse on the config's `TriggerChannel` at tick 0; scripts must not drive it.
3. **Prepare the secondary.** The primary sends the script text and its full parameter
   set (`LinkRunRequest`). The secondary compiles that text, builds its own channels with
   the primary's parameters (`LoadGlobalParameters` uses them, not its own json), arms
   its pattern generator to wait for the trigger, and replies with hashes of the script
   and parameters, its clock rates and its git revision (`LinkReadyInfo`). The primary
   refuses the run on any mismatch, and only warns if the git revisions differ.
4. **Run.** For each shot, the primary waits until the secondary is armed, then plays its
   pattern, whose trigger pulse starts the secondary. At the end it checks that the
   secondary played every shot. Both machines save a zip containing a `_link.txt` with
   the same `LinkRunId`.
5. **Failures** in a linked run always throw to the `Go` caller: the GUI shows a box, and
   Python gets the exception. Remote errors carry the secondary's real message
   (`LinkRemoting`). If the primary goes quiet, an armed secondary ends its run by itself
   after about one pattern length plus 10 s.

**What a pair of machines needs.**
- **Primary:** its Hardware class declares a `MOTMasterLinkConfig`.
- **Secondary:** any running MOTMaster can be one; it needs no config of its own.
- **The secondary's Hardware class:**
  - It must be in the shared `DAQ.dll`, because the primary constructs it to look up the
    secondary's channel names. So its constructor must not touch hardware.
  - It must set `PGTriggerLine`, the input it arms on, rising edge.
- **No shared channel names:** no channel name may appear on both machines.
- **Clocks:** both machines need the same clock rates, with digital equal to analog on
  each.
- **Debug mode:** a secondary in Debug mode refuses linked runs.
- **Script requirements:**
  - It needs at least one event on the secondary's master pattern board.
  - If it has secondary analog events, the primary needs a rising edge on one of
    `AnalogStartChannels`.
  - It needs a `SecondaryTriggerDelay` parameter.
- **Wiring:** the primary's trigger channel connects to the secondary's
  `PGTriggerLine`, and TCP port 1187 must be reachable.

**Timing.** Script times use the primary's frame. A secondary digital event at `t` plays
at secondary tick `t - SecondaryTriggerDelay`. An analog event plays at
`t + A - SecondaryTriggerDelay`, where `A` is the first rising edge of the primary's
analog-start channel (`DDS_Analog_Trg`). `SecondaryTriggerDelay` is an ordinary
parameter (cafmot: `globalParameters.json`, General group), in clock ticks. Single-tick
events that would land before tick 0 are moved to 0 and logged; a pulse or ramp that
would start before tick 0 is an error.

## How the code is laid out

The rule throughout: **no link means the old code path, statement for statement.**
`LinkRouting.Current` is null in every unlinked run, and `Controller.goClaimed` reads
`linkedPrimary` once into `link`, so `link == null` runs exactly what it always did.

| Layer | Hardware? | Unit-tested? |
|---|---|---|
| `LinkRouting` (DAQ): channel ownership and time mapping, consulted by the builders | no | yes |
| `MOTMaster/Linked/*`: the protocol, state, hashes and script building | no | yes, with `FakeSecondary` |
| `Controller` / `ControllerWindow`: wiring to the real pattern generators and the GUI | yes | no; checked by a live run |

## Where the files are

| File | What's in it |
|---|---|
| `DAQ/MOTMasterLinkConfig.cs` | The config class: secondary URL and Hardware type, trigger channel, analog-start channels, timeouts. |
| `DAQ/MoleculeMOTHardware.cs` | cafmot's config (search for `MOTMasterLinkConfig`). The trigger channel is `rbTrigger`. |
| `DAQ/RbTweezerHardware.cs` | The secondary's channels and `PGTriggerLine` (the input it arms on). |
| `DAQ/LinkRouting.cs` | Thread-static routing context: which machine owns each channel, the reserved trigger channel, time shifts and clamping, analog origin `A`. |
| `DAQ/PatternBuilder32.cs`, `DAQ/AnalogPatternBuilder.cs`, `DAQ/AnalogStaticBuilder.cs` | Call into `LinkRouting` for each event. |
| `MOTMaster/Linked/LinkedPrimary.cs` | Primary side of the protocol: `Claim`, `Begin` (checks the secondary's reply), `WaitUntilArmed`, `End`, `Abort`, `Lost`. |
| `MOTMaster/Linked/LinkedSecondaryState.cs` | Secondary's claim token, shot counters and contact timeout. |
| `MOTMaster/Linked/LinkedScriptBuilder.cs` | `BuildPrimary` / `BuildSecondary`, the temp script folders under `%TEMP%\MOTMasterLinked`, `SecondaryTriggerDelay` and `PatternLength`. |
| `MOTMaster/Linked/LinkMessages.cs` | `ILinkSecondary`, the messages sent over Remoting, `LinkedRunException`, `LinkLostException`. |
| `MOTMaster/Linked/LinkFingerprint.cs`, `BuildProvenance.cs`, `LinkRemoting.cs` | Script and parameter hashes; the git revision and build times; showing secondary errors to the primary. |
| `MOTMaster/Controller.cs` | `#region Linked runs: primary side` (`SetLinked`, `IsLinked`) and `#region Linked runs: secondary side` (`ClaimLink`, `TakeBackControl`, `PrepareLinkedRun`, `linkedSecondaryLoop`, `releaseLinkedHardware`); the `link != null` branches in `goClaimed`. |
| `MOTMaster/MOTMasterScript.cs` | `LinkedParameterOverride`, so the secondary uses the primary's parameters. |
| `MOTMaster/ControllerWindow.cs` | The Linked checkbox (primary); the banner and Take back control button (secondary). |
| `MOTMaster/MMDataIOHelper.cs` | `LinkInfo`, saved as `_link.txt`. |
| `MOTMasterTests/` | Hardware-free unit tests (`LinkStateTests`, `LinkRoutingTests`, `LinkedScriptBuilderTests`, ...). Run them with `powershell -ExecutionPolicy Bypass -File MOTMasterTests\run-tests.ps1`. |
| `MoleculeMOTMasterScripts/LinkedScopeTest.cs` | A minimal linked script (one pulse on each machine) for checking the trigger delay on a scope. |
| `docs/superpowers/specs/2026-10-22-linked-motmaster-design.md` | The design and the reasons behind it. |
| `docs/superpowers/plans/2026-10-22-linked-motmaster-handoff.md`, `...-live-checklist.md` | Rebuilding both PCs, wiring, first-run checks. |

## Builds and machines (cafmot)

- **Both machines run the same commit.** A difference only produces a warning, but
  it means the two sides run different code.
- **PH-BONESAW** builds `MOTMaster.csproj` as `Configuration=CaF`.
- **RbTweezer** builds `MOTMaster.csproj` as `Configuration=RbTweezer /p:Platform=AnyCPU`
  from the command line:
  - The output is `MOTMaster\bin\RbTweezer\MOTMaster.exe`, and it must run 64-bit like
    CaF's, or the parameter hashes won't match.
  - Its scripts compile against `DAQ\bin\x86\RbTweezer\daq.dll`, which is built from
    `DAQ.csproj` as `RbTweezer|x86`.
- **Reaching RbTweezer:** `ssh cafmot@10.59.3.2`, which lands in cmd, so use `cd /d`.
  The logs are in `C:\ControlPrograms\EDMSuite\Logs\motmaster_<date>.log` on each
  machine.
- **Ask first:** a running MOTMaster locks its `bin` folder, so ask the user to close it
  before building.
