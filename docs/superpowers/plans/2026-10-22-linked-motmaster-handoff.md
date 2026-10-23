# Linked MOTMaster: handoff notes

Status on 2026-10-22: the code is finished on branch `feature/linked-motmaster`. That branch is **not merged and not pushed**, and **nothing has been rebuilt** in the normal `bin` folders on either PC. The MOTMaster that is running on each machine is still the old build.

What the feature does: one MOTMaster script can drive channels on both PH-BONESAW (CaF, the primary) and RbTweezer (the secondary). You tick **Linked** on the primary, and then run as usual, from the GUI or from Python with `SetScriptPath`/`Go`. Design and details: [the spec](../specs/2026-10-22-linked-motmaster-design.md). The full set of live checks: [the live-run checklist](2026-10-22-linked-motmaster-live-checklist.md).

---

## 1. Get the code onto both PCs

- **PH-BONESAW** (`C:\ControlPrograms\EDMSuite`) already has the branch checked out.
- **RbTweezer** (10.59.3.2) needs the same branch. It exists only on PH-BONESAW, so first push it (`git push -u origin feature/linked-motmaster`) or copy it across. Then on RbTweezer run `git fetch` and `git checkout feature/linked-motmaster`.
  - Both machines must be on the same commit. Each run records each machine's git commit and warns in the log if they differ.

## 2. Rebuild

Close **MOTMaster** on the PC first. A running MOTMaster locks its `bin` folder, and the build fails with MSB3027 on `DAQ.dll`. On PH-BONESAW, if the build complains about a locked SpectrumDDS file, close **SpectrumDDSController** too.

### PH-BONESAW: `CaF` configuration

```powershell
& "C:\Program Files (x86)\Microsoft Visual Studio\2019\Enterprise\MSBuild\Current\Bin\amd64\MSBuild.exe" `
    C:\ControlPrograms\EDMSuite\MOTMaster\MOTMaster.csproj /p:Configuration=CaF /p:Platform=AnyCPU /nologo /v:minimal
```

(Building `MOTMaster` with the `CaF` configuration in Visual Studio is equivalent.)

Check that these files now have today's timestamp. They are the paths `PHBonesawFileSystem.cs` points MOTMaster at:
- `MOTMaster\bin\CaF\MOTMaster.exe`
- `DAQ\bin\CaF\daq.dll`

### RbTweezer: `RbTweezer` configuration

In Visual Studio, pick the `RbTweezer` configuration (any platform: all four map to `RbTweezer|Any CPU`) and build the solution. It builds only SharedCode, DAQ and MOTMaster. From the command line, build `MOTMaster.csproj` with `/p:Configuration=RbTweezer /p:Platform=AnyCPU` (the MSBuild path may differ from PH-BONESAW's).

**Check that both of these files have today's timestamp.** They are the paths `RbTweezerFileSystem.cs` points MOTMaster at:
- `MOTMaster\bin\RbTweezer\MOTMaster.exe`
- `DAQ\bin\RbTweezer\daq.dll`

Start MOTMaster from `MOTMaster\bin\RbTweezer\`. Builds left in `bin\x86\RbTweezer\` or `bin\ZeemanSisyphus\` (from the old solution mapping) are stale and may be 32-bit; check that no shortcut points at them.

### Optional sanity check (no hardware)

After the PH-BONESAW build, run `powershell -ExecutionPolicy Bypass -File MOTMasterTests\run-tests.ps1` from the repo root. Expect every test to pass (60 as of 2026-10-23).

## 3. Before the first linked run

- **Wiring.** Connect PH-BONESAW `/PXI1Slot4` port 1 line 6 (`rbTrigger`) to RbTweezer `/PXI1Slot5` **PFI5** (RbTweezer's `PGTriggerLine`).
- **New test line on RbTweezer.** RbTweezer `/PXI1Slot5` port 2 line 0 is newly registered as `tScopeTest`. Nothing in `RbTweezerHardware.cs` used it before, but **check nothing is plugged into it** before running.
- **Network.** Check that PH-BONESAW can reach `10.59.3.2` on TCP **1187** (firewall).
- **Start both MOTMasters.** On PH-BONESAW, tick **Linked to 10.59.3.2**. RbTweezer should show a gold "Controlled by PH-BONESAW (linked run)" banner, with its Run and script controls greyed out.

## 4. Test script: `LinkedScopeTest`

The script is `MoleculeMOTMasterScripts\LinkedScopeTest.cs`, run **on PH-BONESAW with Linked ticked**. Run unlinked it fails, because `tScopeTest` is an RbTweezer channel.

Each test pulse is **10 ms long and starts 10 ms into a 200 ms pattern**.

| Probe | Machine | Board / line | What you should see |
|---|---|---|---|
| `rbTrigger` | PH-BONESAW | `/PXI1Slot4` port 1 line 6 | 100 µs pulse at t = 0 (added by MOTMaster, not the script) |
| `v0ddsSwitchA` | PH-BONESAW | `/PXI1Slot4` port 2 line 0 | 10 ms pulse from t = 10 ms |
| `tScopeTest` | RbTweezer | `/PXI1Slot5` port 2 line 0 | 10 ms pulse from t = 10 ms, lagging `v0ddsSwitchA` by the trigger/cable delay |

Trigger the scope on `rbTrigger` or `v0ddsSwitchA`. Then:

1. Measure how far the RbTweezer pulse lags the CaF pulse.
2. Set `SecondaryTriggerDelay` (General group of `MoleculeMOTMasterScripts\globalParameters.json`) to that lag **in clock ticks of 10 µs**. It is 0 now. RbTweezer's events are then moved earlier by that amount, so the two pulses line up.
3. Rerun and confirm the two pulses coincide, to within one tick (10 µs).

The pulse start and length can be changed per run as the parameters `PulseStart` and `PulseDuration` (in ticks).

What a correct run also produces: both machines save a zip, and each zip contains a `_link.txt`. The two files have the same `LinkRunId`, script hash and parameter hash.

## 5. If something goes wrong

- **"never saw the trigger"** from `Go`: RbTweezer armed but no edge arrived on PFI5. Check the wiring, and check that `rbTrigger` pulses on the scope.
- **"did not arm"**: RbTweezer could not get ready within 5 s. Look at the RbTweezer MOTMaster log for the reason.
- **"has taken back control"**: RbTweezer pressed **Take back control**. PH-BONESAW turns Linked off by itself; tick it again to re-link.
- **Using RbTweezer on its own**: press **Take back control** on RbTweezer whenever no linked run is in progress. There is no confirmation. The button is greyed out during a linked run.
- **RbTweezer stuck with the banner** after PH-BONESAW crashes or closes: press **Take back control** on RbTweezer. If the crash was mid-run, the armed run first ends by itself after about one pattern length plus 10 s with no contact from the primary, and the button then becomes available.
- **Ticking Linked takes ~20 s and then fails**: RbTweezer's MOTMaster isn't running or isn't reachable (TCP connect timeout).

## 6. Other changes on this branch you should know about

- **`rbTrigger` (renamed)**
  - `v0ddsSwitchD` is renamed to `rbTrigger`.
  - The `v0rfswitch1-4` / `v0ddsSwitchA-D` lines were removed from `0. DDSFourSidebands5W_noDDS.cs` and `BX_spectroscopy.cs`, so unlinked runs of those scripts no longer pulse that line. Only linked runs drive `rbTrigger`, and a script that drives it itself is refused while linked.
  - Scripts in subfolders (e.g. `OldDDS\`) still use the old name. MOTMaster never loads those folders.
- **`SecondaryTriggerDelay` = 0** was added to `globalParameters.json`. It has no effect on unlinked runs.
- **`DDSPatternBuilder`** now compiles in every build configuration; the code that drives the DDS card is still CaF-only.
- **Script check** (`verify-scripts.ps1`): the expected result is 27 scripts, 22 pass and 5 fail.
  - Four failures are older: `LambdaRecap`, two `4. TCoil_test_*`, `TweezerRESONANTlMAGE`.
  - `LinkedScopeTest` fails here by design, because the check runs unlinked.
- **Unrelated local change**: `MoleculeMOTHardwareControl\obj\CaF\DesignTimeResolveAssemblyReferencesInput.cache` is modified and uncommitted. That predates this work.
- **Not done**: View Pattern on a linked script shows only the CaF channels. Detecting linked mode from the script instead of the checkbox was discussed and deferred.
