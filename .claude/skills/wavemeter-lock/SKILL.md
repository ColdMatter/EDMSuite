---
name: wavemeter-lock
description: Editing, building, or verifying the WavemeterLock client (WavemeterLock/, WavemeterLock.exe) and WavemeterLockServer (WaveMeterExample/, "Wavemeter Lock Server.exe") — the .NET Remoting-based laser wavelength lock ("WML"). Covers Controller.cs's mainLoop/checkBlockLoop/updateLockMaster, the Laser PI controller (Lasers.cs), the GUI (LockForm/LockControlPanel), the server's wlmData.dll callback (WaveMeterExample/Controller.cs, wlmData.cs), the DAQmx helpers (DAQ/DAQMxWavemeterLock*.cs), per-experiment laser config (DAQ/WavemeterLockConfig.cs, AddSlaveLaser/AddLaserConfiguration/AddLockBlock), and the TCP channel wiring in DAQ/EnvironsHelper.cs. Use this whenever a task touches the wavemeter lock's C# internals, adds/reconfigures a locked laser for an experiment, or asks how to build WavemeterLock/WavemeterLockServer (which configuration to pass, why a plain `Debug`/`Release` build fails). This skill does NOT launch or drive the GUI — that would engage real laser lock hardware (analog voltage outputs, DAQmx digital reads) which is out of scope per this repo's safety rules; only building/compiling is automated here. Running the apps and doing anything with a live wavemeter/laser is done by the user, not this skill.
---

Two separate WinForms .NET Framework 4.x apps talking over .NET Remoting/TCP — not a single project despite the shared "wavemeter lock" name. There is no headless test harness and this skill does not attempt to launch either app: doing so would drive real laser hardware, which is out of scope (see `CLAUDE.md`'s safety section). The only automated check here is **compiling both projects**, via `.claude/skills/wavemeter-lock/build.ps1`.

All paths below are relative to the repo root (`C:\ControlPrograms\EDMSuite`).

## Architecture (for editing)

- **Client**: `WavemeterLock/WavemeterLock.csproj` → `WavemeterLock.exe`. Entry point `WavemeterLock/Runner.cs`. Runs on the laser-control PC, one instance per client, connects to a server over Remoting.
- **Server**: `WaveMeterExample/WavemeterLockServer.csproj` → `Wavemeter Lock Server.exe`. Entry point `WaveMeterExample/Runner.cs`. Runs on the PC physically connected to the Highfinesse wavemeter; wraps `wlmData.dll` (`WaveMeterExample/wlmData.cs`).
- Both register a `MarshalByRefObject` `Controller` on a `TcpChannel` (`RemotingServices.Marshal(controller, "controller.rem")`). The client gets a proxy via `Activator.GetObject(...)` (`WavemeterLock/Controller.cs`) and calls remote methods (`getFrequency`, `getMeasurementStatus`, `displayWavelength`, ...) — every wavemeter read is a network round trip. **Don't rename/remove/change the signature of any `Controller` remote method** — other client machines on the network depend on this exact surface, and you can't see them from here.
- TCP ports/hostnames per-computer are in `DAQ/EnvironsHelper.cs` (`serverTCPChannel`, `wavemeterLockTCPChannel`, `serverComputerName`), keyed by `Environment.MachineName` in a big `switch`. This machine (`PH-BONESAW`) is its own server (`serverComputerName = "PH-BONESAW"`, `wavemeterLockTCPChannel = 5555`, ~line 188-196) with the `MoleculeMOTHardware` laser config.
- **Server** is event-driven at the DLL level: `WLM.Instantiate(..., cNotifyInstallCallback, callbackObj, 0)` (`WaveMeterExample/Controller.cs`) registers a native callback that fires when `wlmData.dll` completes a measurement — the server itself doesn't poll hardware in a loop. The wavemeter callback only reliably fires on its internal multi-channel switcher's "channel 1" pass (see `WavemeterLock/readme.md`'s "Current issues" section) — this hardware/firmware settling time is what caps the real lock update rate at ~10Hz; it's not something client code controls.
- **Client** lock loop: `Controller.mainLoop()`/`polling()` in `WavemeterLock/Controller.cs`, started as a background `Thread` from `startWML()`. Polls the server's shared boolean flag (`getMeasurementStatus`), and on `true` calls `updateLockMaster()` → per-laser `Laser.UpdateLock()` (`WavemeterLock/Lasers.cs`), a **PI controller** (no D term) writing `SetLaserVoltage` via `DAQ/DAQMxWavemeterLockLaserControlHelper.cs`. A second background loop, `checkBlockLoop()`, polls each laser's lock-block digital line via `DAQ/DAQMxWavemeterLockBlockHelper.cs`. **Both loops are intentionally throttled with `Thread.Sleep(2)`** (added for a CPU-usage fix) — if you touch them, preserve that: real measurement updates must still fire exactly once per real new measurement, never skipped/coalesced/doubled.
- **Per-experiment laser config**: `DAQ/WavemeterLockConfig.cs` (`AddSlaveLaser(name, analogChannel, wlmChannelNum)`, `AddLaserConfiguration(name, setPointTHz, PGain, IGain)`, `AddLockBlock(name, digitalChannel)`). Built per-experiment in that experiment's `DAQ/*Hardware.cs` (e.g. `DAQ/MoleculeMOTHardware.cs` around line 279 builds the `cafmot` laser set: `v0`..`v3`, `BX`, `TCool`). **This is what you edit to add/retune a locked laser** — not `WavemeterLock/Controller.cs` itself, unless you're changing shared lock-loop behavior for every experiment.
- GUI: `WavemeterLock/LockForm.cs`/`.Designer.cs` (main window, per-laser panels) and `WavemeterLock/LockControlPanel.cs` (per-laser controls, error-signal chart, gain buttons). `LockForm`'s `timer1` (implicit 100ms default interval, not the lock loop) refreshes displayed text/LEDs via extra remote calls — separate from the lock loop's own remoting.

## Build (agent path — the only automated check)

Neither `WavemeterLock.csproj` nor `WavemeterLockServer.csproj` can be built directly with a bare `/p:Configuration=Debug` or `Release` — their project references (`DAQ.csproj`, `SharedCode.csproj`) don't define those configurations, only experiment-named ones (`CaF`, `AlF`, `ZeemanSisyphus`, ...). `EDMSuite.sln` defines a **`Wavemeter`** solution configuration that maps each project to one it actually has (WavemeterLock/WavemeterLockServer → `Release`, DAQ → `ZeemanSisyphus`, SharedCode → its own `Wavemeter` config). Build through the `.sln`, not the bare `.csproj`.

Run the wrapper script (verified working this session):

```powershell
cd C:\ControlPrograms\EDMSuite
powershell -ExecutionPolicy Bypass -File .claude\skills\wavemeter-lock\build.ps1
```

It locates `MSBuild.exe` via `vswhere` (falling back to the known VS2019 Enterprise path on this machine), builds just the `WavemeterLock` and `WavemeterLockServer` targets (their `DAQ`/`SharedCode` dependencies build too, but none of the ~25 other unrelated experiment projects in the solution), and prints the output exe paths. Exit code 0 = build succeeded.

Confirmed this session:
```
BUILD SUCCEEDED
  Client: C:\ControlPrograms\EDMSuite\WavemeterLock\bin\Release\WavemeterLock.exe
  Server: C:\ControlPrograms\EDMSuite\WaveMeterExample\bin\Release\Wavemeter Lock Server.exe
```
Only warnings emitted are pre-existing `MSB3270` processor-architecture mismatches against the `NationalInstruments.DAQmx` reference (MSIL project vs x86 NI assembly) — this is expected/harmless and unrelated to any edit; it shows up on every build of anything referencing DAQmx on this machine (see `motmaster-controller` skill for the same warning on `MOTMaster.csproj`). Treat *new* errors or warnings beyond this pattern as real.

If you'd rather run MSBuild by hand instead of the script:
```powershell
& "C:\Program Files (x86)\Microsoft Visual Studio\2019\Enterprise\MSBuild\Current\Bin\amd64\MSBuild.exe" `
    "C:\ControlPrograms\EDMSuite\EDMSuite.sln" /t:"WavemeterLock;WavemeterLockServer" `
    /p:Configuration=Wavemeter /p:Platform="Any CPU" /nologo /v:minimal
```

## What this skill does NOT do

No GUI launch, no screenshots, no driver for clicking through `LockForm`/`ServerForm`. Engaging a lock in the client writes real analog voltages to a laser piezo/PZT via NI DAQmx (`DAQ/DAQMxWavemeterLockLaserControlHelper.SetLaserVoltage`), and the server needs an actual Highfinesse wavemeter connected via `wlmData.dll` to do anything meaningful. Per this repo's `CLAUDE.md`, interacting with experiment hardware is done by the user, at a time of their choosing — not automated here. If you need to verify a runtime behavior change (not just "does it compile"), hand the built exes to the user and describe what to look for (e.g. the lock-rate readout, the error-signal chart) — don't launch them yourself.

## Gotchas

- **Building the bare `.csproj` fails**, even with a seemingly-reasonable `/p:Configuration=Release`: `DAQ.csproj`/`SharedCode.csproj` project references don't have that configuration and MSBuild errors with `The BaseOutputPath/OutputPath property is not set for project 'DAQ.csproj'`. Always build via `EDMSuite.sln` with `/p:Configuration=Wavemeter` (see above), not the individual project file.
- **`WavemeterLock/Fakes/NationalInstruments.DAQmx.fakes`** produces a `Some fakes could not be generated` warning on every build — pre-existing, unrelated to DAQmx driver version or your changes; ignore it.
- **The wavemeter's ~10Hz update ceiling is a hardware fact, not a client bug.** The server's `measurementAcquired` callback only reliably fires on the wavemeter's internal channel-switcher's channel-1 pass (documented in `WavemeterLock/readme.md`'s own "Frequently occurred issues"/"Current issues" sections). Don't try to "fix" this by changing client polling logic — it won't move the ceiling, only waste CPU (see the `Thread.Sleep(2)` throttles already in `mainLoop()`/`checkBlockLoop()`).
- **No true server→client push notification.** `readme.md` explains subscribing to the server's C# event across the Remoting boundary throws a `System.Security` exception in this environment — hence the polled boolean-flag workaround (`measurementStatus`/`getMeasurementStatus`/`resetMeasurementStatus`) that `mainLoop()` polls. Don't "simplify" this into a real event subscription without testing across an actual two-machine Remoting setup — it's not a trivial swap.
