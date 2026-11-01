# Linked MOTMaster Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Let one MOTMaster script drive channels on two MOTMaster systems. The primary claims the secondary over .NET Remoting, sends it the exact script text and parameters, arms it in triggered mode, and fires it with a TTL.

**Architecture:**
- A thread-local `LinkRouting` context in `DAQ.Pattern` tells the existing pattern builders which channels belong to this machine and how to shift their times. With no context set, the builders behave exactly as today.
- The primary-side orchestration is `LinkedPrimary`, which talks to an `ILinkSecondary` interface. The secondary side is `LinkedSecondaryState` plus new `Controller` methods.
- Building a sequence for either side goes through `LinkedScriptBuilder`, which is testable without hardware.
- `Controller` gains hooks only on the linked paths.

**Tech Stack:** C# 7.3, .NET Framework 4.5.2 (DAQ) / 4.6.1 (MOTMaster), .NET Remoting (TCP 1187), WinForms, NI DAQmx. Tests are a small console harness compiled with the VS2019 Roslyn `csc.exe` and run from PowerShell.

**Spec:** `docs/superpowers/specs/2026-10-22-linked-motmaster-design.md`. Read it before any task.

## Global Constraints

- **Safety:** never run MOTMaster, call `Run`/`Go`/`RunReplica`, or touch NI hardware. Verify only by `msbuild`, the test harness, and `verify-scripts.ps1`. The first live linked run is the user's.
- **Non-linked behaviour must not change.** With `LinkRouting.Current == null`, and on any machine whose `Hardware` has no `MOTMasterLinkConfig`, every existing code path behaves exactly as today. That includes today's `NullReferenceException` on an unknown channel name and today's MessageBox-and-return error handling.
- Work on branch `feature/linked-motmaster`. Never commit to `master`.
- New `.cs` files must be added to `DAQ/DAQ.csproj` / `MOTMaster/MOTMaster.csproj` `<Compile Include>`. These `.csproj` files are already tracked, so `git add` works on them directly.
- Build commands, which must stay clean (only the pre-existing NI warnings):
  - `& "C:\Program Files (x86)\Microsoft Visual Studio\2019\Enterprise\MSBuild\Current\Bin\amd64\MSBuild.exe" C:\ControlPrograms\EDMSuite\MOTMaster\MOTMaster.csproj /p:Configuration=CaF /p:Platform=AnyCPU /nologo /v:minimal`
  - the same with `/p:Configuration=RbTweezer`.
- Run everything under the **PowerShell** tool, never Git Bash, because `csc` `/flags` get mangled into MSYS paths.
- Parameter name `SecondaryTriggerDelay` (int, clock ticks). Info key `"MOTMasterLink"`. Saved file suffix `_link.txt`.
- Remoting message types are `[Serializable]` and contain only strings, ints, bools, `string[]` and `Dictionary<string, object>`.
- Match the surrounding code style: `SharedCode.AppLog.Write/Error` for logging, and plain classes with no new NuGet packages.

## Review Focus

1. **A script calls `GetValue` on another system's analog channel** (e.g. starting a ramp from its current value). Expected: a `LinkRoutingException` naming the channel, not a `NullReferenceException`. Test in Task 2.
2. **The secondary is unreachable at link time, or was restarted mid-scan so its claim is gone.** Expected: a `LinkedRunException` that names the URL or says to re-link, rather than a hang or a raw remoting error. Test in Task 4.
3. **The user presses Stop on the primary while it is waiting for the secondary to arm.** Expected: the wait returns promptly with no timeout error, and the secondary is told to end. Test in Task 4.
4. **Linked mode is on but the script uses only primary channels.** Expected: a clear error saying the script uses no secondary channels and to turn linked mode off. Test in Task 5.
5. **A Python scan overrides `SecondaryTriggerDelay` (or any timing parameter) per `Go(dict)`.** Expected: each run's secondary pattern uses that run's value. Test in Task 5.

---

### Task 1: Test harness and `LinkRouting` core

**Files:**
- Create: `MOTMasterTests/run-tests.ps1`, `MOTMasterTests/TestRunner.cs`, `MOTMasterTests/TestHardware.cs`, `MOTMasterTests/LinkRoutingTests.cs`
- Create: `DAQ/LinkRouting.cs`
- Modify: `DAQ/DAQ.csproj` (add `<Compile Include="LinkRouting.cs" />`)

**Interfaces:**
- Produces (namespace `DAQ.Pattern`, file `DAQ/LinkRouting.cs`):
  - `public enum LinkSide { Primary, Secondary }`
  - `public class LinkRoutingException : ApplicationException` with a `(string message)` constructor
  - `public sealed class LinkRouting`:
    - `static LinkRouting Current { get; }`, backed by a `[ThreadStatic]` field
    - `static IDisposable Enter(LinkRouting routing)`: throws `InvalidOperationException` if a context is already active on this thread; `Dispose` clears it
    - `static DigitalOutputChannel LookupDigital(string name)` / `static AnalogOutputChannel LookupAnalog(string name)`: look in `Current.Local` when a context is set, otherwise in `Environs.Hardware`, using the same `Hashtable` cast as today
    - `static void ThrowIfActive(string what)`
    - `static LinkRouting ForPrimary(Hardware local, Hardware other, string triggerChannel, IEnumerable<string> analogStartChannels)`
    - `static LinkRouting ForSecondary(Hardware local, Hardware other, int triggerDelay, int analogOrigin)`
    - `LinkSide Side`, `Hardware Local`
    - `bool IsLocalDigital(string name)`, `bool IsLocalAnalog(string name)`
    - `int ShiftDigital(int time)`, `int ShiftAnalog(int time)`: pure mappings
    - `int MapDigitalPoint(string name, int time)`, `int MapDigitalSpan(string name, int start)`, `int MapAnalogPoint(string name, int time)`, `int MapAnalogSpan(string name, int start)`
    - `void NoteDigitalRise(string name, int time)`
    - `int? AnalogOrigin()`
    - `int AnalogLengthExtension { get; }`, `IList<string> ClampedEvents { get; }`, `bool UsedOtherChannels { get; }`, `bool UsedOtherAnalog { get; }`
- Produces (namespace `MOTMasterTests`):
  - `Assert.Equal<T>(T expected, T actual, string what = null)`, `Assert.True(bool, string)`
  - `TException Assert.Throws<TException>(Action)`
  - `TestHardware : Hardware` with `TestHardware Digital(string name, string device, int port, int line)` and `TestHardware Analog(string name, string physicalChannel)`, both fluent and calling the protected `Add*OutputChannel`.

Rules that `LinkRouting` must implement (from the spec):
- **Ownership.** A name in `local` only → local; in `other` only → other side, and sets `UsedOtherChannels` (plus `UsedOtherAnalog` for analog). A name in both → `LinkRoutingException` containing `"is defined on both"`. A name in neither → `LinkRoutingException` containing `"is not a channel on"`. Both messages name the channel and the two hardware class names. On the primary, `IsLocalDigital(triggerChannel)` throws `LinkRoutingException` containing `"reserved for the secondary trigger"`.
- **Primary side.** All `Map*`/`Shift*` calls return their input unchanged and `AnalogLengthExtension == 0`.
- **Secondary side.**
  - `ShiftDigital(t) = t − triggerDelay`, `ShiftAnalog(t) = t + analogOrigin − triggerDelay`, `AnalogLengthExtension = analogOrigin − triggerDelay`.
  - `Map*Point`: a shifted time below 0 becomes 0, and a line of the form `"<channel> <digital|analog> t=<orig> -> 0 (was <shifted>)"` is added to `ClampedEvents`. If any event on the same channel lands at tick 0 after another event already did, and either of them was clamped, throw `LinkRoutingException` containing `"clamped to the trigger"`.
  - `Map*Span`: a shifted start below 0 throws `LinkRoutingException` containing `"starts before the secondary is triggered"`.
- **`NoteDigitalRise`** records the earliest rise per channel, only for `analogStartChannels`. `AnalogOrigin()` returns null when none rose, the tick when exactly one distinct tick was recorded, and throws `LinkRoutingException` containing `"analog start is ambiguous"` when the configured channels rose at different ticks.

- [ ] **Step 1: Commit the design work already on the branch**

```powershell
git add docs/superpowers/specs/2026-10-22-linked-motmaster-design.md docs/superpowers/plans/2026-10-22-linked-motmaster.md CLAUDE.md .claude/skills/motmaster-controller/SKILL.md .claude/skills/motmaster-python-integration/SKILL.md
git commit -m "docs: linked MOTMaster design and plan; CaF timing note; skill notes"
```

- [ ] **Step 2: Write the harness**

`run-tests.ps1` takes a `-Configuration` parameter (default `CaF`). It compiles every `MOTMasterTests\*.cs` with `C:\Program Files (x86)\Microsoft Visual Studio\2019\Enterprise\MSBuild\Current\Bin\Roslyn\csc.exe`:
- flags `/nologo /target:exe /langversion:7.3`
- references `MOTMaster\bin\$Configuration\MOTMaster.exe` and `MOTMaster\bin\$Configuration\DAQ.dll`
- output `MOTMaster\bin\$Configuration\MOTMasterTests.exe`, so that DAQ, MOTMaster and NI assemblies resolve from that folder

It then runs the exe and exits with its exit code. `TestRunner.Main` uses reflection to find every `public static void Test*()` in namespace `MOTMasterTests`, runs each one, prints `PASS name` or `FAIL name: <exception>`, prints a total, and returns 1 if anything failed. A test may take an optional `string[] args` (the test's argument list) to receive the repo root.

- [ ] **Step 3: Write the failing tests in `LinkRoutingTests.cs`**

Fixtures:
- `local = new TestHardware().Digital("pA","/D1",0,0).Digital("trig","/D1",0,1).Digital("dds","/D1",0,2).Analog("aA","/A1/ao0")`
- `other = new TestHardware().Digital("tB","/D9",0,0).Analog("tC","/A9/ao0")`
- `both = new TestHardware().Digital("pA","/D9",0,5)`

```csharp
public static void TestOwnership() {
    var r = LinkRouting.ForPrimary(local, other, "trig", new[] { "dds" });
    Assert.True(r.IsLocalDigital("pA"), "pA local");
    Assert.True(!r.IsLocalDigital("tB"), "tB other");
    Assert.True(r.UsedOtherChannels && !r.UsedOtherAnalog, "flags after digital");
    Assert.True(!r.IsLocalAnalog("tC") && r.UsedOtherAnalog, "analog flag");
    StringAssert(Assert.Throws<LinkRoutingException>(() => r.IsLocalDigital("nope")).Message, "is not a channel on", "nope");
    StringAssert(Assert.Throws<LinkRoutingException>(() => r.IsLocalDigital("trig")).Message, "reserved for the secondary trigger");
    var rb = LinkRouting.ForPrimary(local, both, "trig", new string[0]);
    StringAssert(Assert.Throws<LinkRoutingException>(() => rb.IsLocalDigital("pA")).Message, "is defined on both", "pA");
}
public static void TestPrimaryMappingIsIdentity()   // MapDigitalPoint("pA",-5)==-5; MapAnalogSpan("aA",7)==7; AnalogLengthExtension==0
public static void TestSecondaryShift() {           // ForSecondary(local, other, triggerDelay: 3, analogOrigin: 4000)
    // MapDigitalPoint("pA",10)==7; MapAnalogPoint("aA",10)==4007; AnalogLengthExtension==3997
}
public static void TestSecondaryClampPoint()        // delay 3: MapDigitalPoint("pA",1)==0; ClampedEvents.Count==1 and contains "pA digital t=1 -> 0 (was -2)"
public static void TestSecondaryClampCollision()    // delay 3: MapAnalogPoint("aA",0) then MapAnalogPoint("aA",3) -> throws "clamped to the trigger"
public static void TestSecondaryNoClampCollisionWhenUnclamped() // delay 0: MapAnalogPoint("aA",0) twice -> no throw (today's overwrite semantics)
public static void TestSecondarySpanBeforeTrigger() // delay 3: MapDigitalSpan("pA",2) throws "starts before the secondary is triggered"
public static void TestAnalogOrigin() {
    var r = LinkRouting.ForPrimary(local, other, "trig", new[] { "dds", "pA" });
    Assert.Equal<int?>(null, r.AnalogOrigin());
    r.NoteDigitalRise("dds", 4000); r.NoteDigitalRise("dds", 9000); r.NoteDigitalRise("pA", 4000);
    Assert.Equal<int?>(4000, r.AnalogOrigin());
    r.NoteDigitalRise("pA", 3000);
    StringAssert(Assert.Throws<LinkRoutingException>(() => r.AnalogOrigin()).Message, "analog start is ambiguous");
}
public static void TestEnterIsThreadLocalAndNotNested() // Enter sets Current; nested Enter throws InvalidOperationException; after Dispose Current==null; another thread sees null
```
(`StringAssert(msg, params string[] parts)` is a helper in `TestRunner.cs` that asserts every part occurs in `msg`.)

- [ ] **Step 4: Run and confirm the failure**

Build first: `MSBuild ... MOTMaster.csproj /p:Configuration=CaF`. Then run `powershell -ExecutionPolicy Bypass -File MOTMasterTests\run-tests.ps1`. Expected: the harness fails to compile, because `LinkRouting` does not exist yet.

- [ ] **Step 5: Implement `DAQ/LinkRouting.cs`** to the rules above, and add it to `DAQ.csproj`. Ownership looks at `Hardware.DigitalOutputChannels.ContainsKey` / `AnalogOutputChannels.ContainsKey`. Hardware names in messages come from `GetType().Name`.

- [ ] **Step 6: Rebuild and rerun.** Expected: all `LinkRoutingTests` pass.

- [ ] **Step 7: Commit**

```powershell
git add MOTMasterTests DAQ/LinkRouting.cs DAQ/DAQ.csproj
git commit -m "feat(daq): LinkRouting context for linked MOTMaster runs, with test harness"
```

---

### Task 2: Pattern builders honour `LinkRouting`

**Files:**
- Modify: `DAQ/PatternBuilder32.cs` (string-channel `AddEdge`/`Pulse`/`DownPulse`, and the legacy int-channel methods)
- Modify: `DAQ/AnalogPatternBuilder.cs` (both constructors, and every string-channel method)
- Modify: `DAQ/AnalogStaticBuilder.cs` (`AddStaticChannel`, `AddStaticAnalogValue`)
- Test: `MOTMasterTests/BuilderRoutingTests.cs`

**Interfaces:**
- Consumes: Task 1 `LinkRouting`.
- Produces: no new public API. Behaviour inside a context:

| Builder call | Other system's channel | This system's channel |
|---|---|---|
| `AddEdge(name,t,sense)` | no-op | if `sense`, `NoteDigitalRise(name,t)`; time = `MapDigitalPoint` |
| `Pulse(start,delay,dur,name)` | no-op, returns `delay+dur` | `NoteDigitalRise(name,start+delay)`; edges at `s=MapDigitalSpan(name,start+delay)` and `s+dur`; returns `delay+dur` |
| `DownPulse(start,delay,dur,name)` | no-op, returns `delay+dur` | `NoteDigitalRise(name,start+delay+dur)`; falling at `s=MapDigitalSpan(name,start+delay)`, rising at `s+dur` |
| `AnalogPatternBuilder(.., length)` | n/a | `PatternLength = length + (Current?.AnalogLengthExtension ?? 0)` |
| `AddChannel`, `SwitchOffAtEndOfPattern`, `AddStaticChannel`, `AddStaticAnalogValue` | no-op | unchanged |
| `AddAnalogValue` | no-op | time = `MapAnalogPoint` |
| `AddAnalogPulse`, `AddLinearRamp` | no-op | start = `MapAnalogSpan` |
| `AddPolynomialRamp` | no-op | start = `MapAnalogSpan`; `stopTime` shifted by the same delta |
| `GetValue(name,t)` | throw `LinkRoutingException` containing `"GetValue"` and the channel name | `Math.Max(0, ShiftAnalog(t))` |
| legacy `AddEdge(int..)`, `Pulse(int..)`, `DownPulse(int..)`, `Clear()` | `LinkRouting.ThrowIfActive("PatternBuilder32 legacy int-channel call")` | (same) |

Every channel lookup in these three files changes from `Environs.Hardware.*OutputChannels[name]` to `LinkRouting.LookupDigital/LookupAnalog(name)`. With no context set, that is the same Hashtable lookup, so an unknown name still gives `null` and the same `NullReferenceException`.

- [ ] **Step 1: Write the failing tests** using the Task 1 fixtures (`local` devices `/D1`, `/A1`), with each test body inside `using (LinkRouting.Enter(...))`:

```csharp
public static void TestPrimarySkipsOtherChannels() {
    // ForPrimary(local, other, "trig", {"dds"}): p.AddEdge("tB",5,true); p.Pulse(0,10,5,"pA"); a=new AnalogPatternBuilder(100); a.AddChannel("tC"); a.AddAnalogValue("tC",3,1.0);
    // Assert: p.Boards.Keys == {"/D1"}; a.Boards.Count == 0; return value of Pulse on "tB" == 15
}
public static void TestSecondaryShiftsDigitalAndAnalog() {
    // ForSecondary(local, other, triggerDelay: 3, analogOrigin: 40): p.Pulse(10,0,5,"pA"); p.BuildPattern(50);
    // Assert bit 0 of p.Boards["/D1"].Pattern is 0 at tick 6, 1 at ticks 7..11, 0 at tick 12
    // a = new AnalogPatternBuilder(20) -> a.PatternLength == 57; a.AddChannel("aA"); a.AddAnalogValue("aA",0,2.0); a.AddLinearRamp("aA",5,4,6.0);
    // Assert a.Boards["/A1"].AnalogPatterns["aA"] has key 37 == 2.0 and keys 42..45
}
public static void TestRiseNotedForAnalogOrigin()   // primary, analogStart {"dds"}: p.Pulse(4000,0,10,"dds") -> routing.AnalogOrigin()==4000
public static void TestGetValueOnOtherSystemThrows() // primary: a.GetValue("tC",0) -> LinkRoutingException containing "GetValue","tC"   [Review Focus 1]
public static void TestLegacyIntCallsRefusedWhenLinked() // in context: p.AddEdge(0,5,true) -> LinkRoutingException; outside a context the int overload is not exercised (needs Info)
public static void TestNoContextUnknownNameStillNullReference() // no context: new PatternBuilder32().AddEdge("definitelyNotAChannel",1,true) -> NullReferenceException (unchanged behaviour)
```

- [ ] **Step 2: Run the tests and confirm the new ones fail** (the builders ignore the context).
- [ ] **Step 3: Implement the table above** in the three files.
- [ ] **Step 4: Rebuild MOTMaster (CaF) and run the harness.** Expected: all tests pass.
- [ ] **Step 5: Regression check: run `.claude\skills\motmaster-controller\verify-scripts.ps1`.** Expected: the same result as before this change (last recorded: 15 pass, 4 fail, all four on `"cafOptPumpingAOM"`). Any other difference is a regression.
- [ ] **Step 6: Commit** `feat(daq): pattern builders route and shift channels inside a LinkRouting context`

---

### Task 3: Script base class: DDS builder everywhere, and linked parameters

**Files:**
- Modify: `MOTMaster/MOTMasterScript.cs`
- Test: `MOTMasterTests/ScriptBaseTests.cs`

**Interfaces:**
- Produces:
  - `DDSPatternBuilder` compiles in every configuration. Move it out of `#if DDS` and update its remarks paragraph about gating. Card-driving code elsewhere stays gated.
  - `public static Dictionary<string, object> LinkedParameterOverride { get; set; }` on `MOTMasterScript`, backed by a `[ThreadStatic]` field. When it is non-null, `LoadGlobalParameters()` copies **every** entry of the override into `Parameters` instead of reading `globalParameters.json`. When null, the method is unchanged.

- [ ] **Step 1: Write the failing tests**

```csharp
public static void TestDdsBuilderAvailable() // new DDSPatternBuilder().AddEvent("e",100,new double[4],new double[4]); Pattern["e"][0][0]==1.0
public static void TestLoadGlobalParametersUsesOverride() {
    // a test subclass whose constructor does: Parameters = new Dictionary<string,object>(); LoadGlobalParameters(); Parameters["Derived"] = (int)Parameters["Base"] + 1;
    // with LinkedParameterOverride = { "Base": 41, "Extra": 2.5 }: Derived==42, Extra==2.5, and no key from the real globalParameters.json (e.g. "BlueMOTDuration") is present
    // finally set the override to null; a new instance then contains "BlueMOTDuration" (the real file is read again)
}
```
- [ ] **Step 2: Run the tests and confirm they fail.** Expected: a compile error (no `LinkedParameterOverride`).
- [ ] **Step 3: Implement the two changes.**
- [ ] **Step 4: Build under CaF and run the harness (all pass). Then build under RbTweezer** with the MSBuild command from Global Constraints and `/p:Configuration=RbTweezer`. Expected: it builds clean.
- [ ] **Step 5: Prove a DDS script now compiles on the RbTweezer build:** run `verify-scripts.ps1 -Configuration RbTweezer`. Expected: no script fails with `CS0246 ... DDSPatternBuilder`. Other failures (CaF channel names under RbTweezer's hardware) are expected here and don't matter.
- [ ] **Step 6: Commit** `feat(motmaster): DDSPatternBuilder in every build; linked parameter override for LoadGlobalParameters`

---

### Task 4: Link messages, fingerprints, provenance, config, and both state machines

**Files:**
- Create: `DAQ/MOTMasterLinkConfig.cs` (namespace `DAQ.HAL`, next to `MMConfig`), and add it to `DAQ.csproj`
- Create: `MOTMaster/Linked/LinkMessages.cs`, `MOTMaster/Linked/LinkFingerprint.cs`, `MOTMaster/Linked/BuildProvenance.cs`, `MOTMaster/Linked/LinkedSecondaryState.cs`, `MOTMaster/Linked/LinkedPrimary.cs`, and add them to `MOTMaster.csproj`
- Test: `MOTMasterTests/LinkStateTests.cs`

**Interfaces:**
- Produces (namespace `DAQ.HAL`):
  - `[Serializable] public class MOTMasterLinkConfig`
    - `public const string InfoKey = "MOTMasterLink";`
    - fields: `string SecondaryUrl; string SecondaryHardwareType; string TriggerChannel; string[] AnalogStartChannels; int TriggerPulseTicks = 10; int ArmTimeoutMs = 5000; int EndTimeoutMs = 30000;`
- Produces (namespace `MOTMaster`):
  - `public class LinkedRunException : ApplicationException` with `(string)` and `(string, Exception)` constructors
  - `[Serializable] public class LinkRunRequest { string LinkRunId, PrimaryName, PrimaryHardwareType, ScriptFileName, ScriptText; Dictionary<string,object> Parameters; int AnalogOrigin; bool SaveEnable; int BatchNumber; }`
  - `[Serializable] public class LinkReadyInfo { string ScriptHash, ParameterHash, Provenance; int DigitalClockFrequency, AnalogClockFrequency; string[] ClampedEvents; }`
  - `[Serializable] public class LinkShotStatus { int ArmedShots; bool Running; string Error; }`
  - `public interface ILinkSecondary`:
    - `string ClaimLink(string primaryName)`
    - `void ReleaseLink(string token)`
    - `LinkReadyInfo PrepareLinkedRun(string token, LinkRunRequest request)`
    - `LinkShotStatus GetLinkShotStatus(string token)`
    - `void EndLinkedRun(string token)`
  - `public static class LinkFingerprint`:
    - `string OfText(string text)`: SHA-256 of the UTF-8 bytes, as lowercase hex
    - `string OfParameters(IDictionary<string, object> p)`: keys sorted ordinally; one line per key, `key \t value.GetType().FullName \t Convert.ToString(value, InvariantCulture)`. Doubles and floats use the `"R"` format, and null becomes `null`. Lines are joined with `\n`, then hashed with SHA-256.
  - `public static class BuildProvenance`:
    - `string Describe()`, computed once and cached. Format: `"git <sha>[ +local changes] | MOTMaster.exe <utc yyyy-MM-ddTHH:mm:ssZ> | DAQ.dll <utc>"`. The repo root is found by walking up from the MOTMaster.exe directory to the first folder containing `.git`, then running `git -C <root> rev-parse HEAD` and `git -C <root> status --porcelain --untracked-files=no`, each with a 5 s timeout. If git can't be run, the first part reads `"git unavailable"`.
    - `bool SameSource(string a, string b)` compares only the text before the first `" | "`.
  - `public sealed class LinkedSecondaryState`:
    - `string Claim(string primaryName, bool runInProgress)` returns a new GUID token. It throws `LinkedRunException` if already claimed (message names the holder) or if `runInProgress`.
    - `void Release(string token)` (wrong token is a no-op), `void Break()`, `void Validate(string token)` (throws `LinkedRunException` containing `"not linked"` / `"re-link"`)
    - `bool IsClaimed`, `string ClaimedBy`
    - `void ThrowIfClaimed(string action)`: message `"This MOTMaster is linked to <ClaimedBy>; <action> is refused until the link is released."`
    - `void RunStarted()` (sets ArmedShots=0, Running=true, Error=null), `void MarkArmed()` (ArmedShots++), `void Fail(string error)`, `void RunEnded()` (Running=false)
    - `LinkShotStatus Status(string token)` (validates first)
    - `event Action Changed`, raised on every state change, for the GUI
    - thread-safe with one lock
  - `public sealed class LinkedPrimary`:
    - `LinkedPrimary(ILinkSecondary secondary, MOTMasterLinkConfig config, Action<string> log)`
    - `void Claim(string primaryName)`: any exception is wrapped as `LinkedRunException` naming `config.SecondaryUrl`
    - `void Release()`: best effort, swallows errors
    - `LinkReadyInfo Begin(LinkRunRequest request, string scriptHash, string parameterHash, int digitalClock, int analogClock, string provenance)`:
      - throws `LinkedRunException` on a hash mismatch (message contains `"script differs"` or `"parameters differ"`), a clock mismatch (`"clock frequencies differ"`), or any secondary exception (wrapped, with `"re-link"` advice when the token is rejected)
      - logs each `ClampedEvents` line
      - logs `"WARNING: primary and secondary run different source revisions"` plus both provenances when `!SameSource`
      - fills `SaveInfo`
    - `bool WaitUntilArmed(int shot, Func<bool> keepWaiting)`: polls `GetLinkShotStatus` every 5 ms.
      - returns true once `ArmedShots >= shot`
      - returns false as soon as `keepWaiting()` is false
      - throws `LinkedRunException` if `Error != null` (message includes it), if `!Running` (`"secondary stopped"`), or after `ArmTimeoutMs` (`"did not arm"`)
    - `void End()`: `EndLinkedRun`, then poll until `!Running`; throws on secondary `Error` or after `EndTimeoutMs`
    - `void Abort()`: like `End` but never throws, and logs failures
    - `Dictionary<string, object> SaveInfo`, with keys `LinkRunId, Role, Partner, ScriptHash, ParameterHash, PrimaryProvenance, SecondaryProvenance, AnalogOrigin, SecondaryTriggerDelay, ClampedEvents` (`Role="primary"`, `ClampedEvents` joined with `"; "`)

- [ ] **Step 1: Write the failing tests**, using a `FakeSecondary : ILinkSecondary` with settable return values, settable exceptions, and a scripted sequence of `LinkShotStatus` values.

```csharp
public static void TestFingerprintOrderIndependentAndTypeSensitive()
    // OfParameters({a:1,b:2.0}) == OfParameters({b:2.0,a:1}); != OfParameters({a:1L,b:2.0}); != OfParameters({a:1,b:2.0000000001})
public static void TestFingerprintText() // OfText("abc") == "ba7816bf8f01cfea414140de5dae2223b00361a396177a9cb410ff61f20015ad"
public static void TestProvenanceSameSource() // SameSource("git x | MOTMaster.exe 1","git x | MOTMaster.exe 2") true; ("git x","git y") false
public static void TestSecondaryClaimRules()
    // Claim("P",false) -> token; second Claim throws "linked to P"; Claim while runInProgress throws; ThrowIfClaimed("Go") message exact;
    // Validate("wrong") throws "re-link"; Release(token) -> !IsClaimed; Break() clears a claim
public static void TestPrimaryBeginMismatches()   // fake returns a different ScriptHash -> "script differs"; a different ParameterHash -> "parameters differ"; a different clock -> "clock frequencies differ"
public static void TestPrimaryBeginWarnsOnProvenance() // different git part -> log contains "different source revisions"; same -> no warning
public static void TestPrimaryWaitArmed()         // statuses [0,running],[0,running],[1,running] -> WaitUntilArmed(1,()=>true) true
public static void TestPrimaryWaitErrorAndTimeout() // Error="boom" -> throws containing "boom"; ArmTimeoutMs=50 with ArmedShots stuck at 0 -> "did not arm"
public static void TestPrimaryWaitStopsOnUserStop() // keepWaiting false -> returns false within 100 ms, no throw   [Review Focus 3]
public static void TestPrimaryUnreachable()       // fake ClaimLink throws SocketException -> LinkedRunException message contains SecondaryUrl   [Review Focus 2]
public static void TestPrimaryClaimLost()         // fake PrepareLinkedRun throws LinkedRunException("not linked ... re-link") -> Begin throws, message contains "re-link"   [Review Focus 2]
```
- [ ] **Step 2: Run the tests and confirm they fail** (compile errors).
- [ ] **Step 3: Implement the files listed above.**
- [ ] **Step 4: Build under CaF and RbTweezer, then run the harness.** Expected: all tests pass.
- [ ] **Step 5: Commit** `feat(motmaster): linked-run messages, fingerprints, provenance and primary/secondary state`

---

### Task 5: `LinkedScriptBuilder`: building each side's sequence without hardware

**Files:**
- Create: `MOTMaster/Linked/LinkedScriptBuilder.cs`, and add it to `MOTMaster.csproj`
- Create: `MOTMasterTests/scripts/LinkedExample.cs`, a test-only script that is **not** in any script folder. It sits in a subfolder, so `run-tests.ps1`'s non-recursive `*.cs` glob must not compile it into the harness. It uses CaF `DDS_Analog_Trg`, `MOTCoilsCurrent` and a CaF digital channel, plus RbTweezer `tCoolSwitch` and `tMOTcc`. It calls `LoadGlobalParameters()`, then sets `PatternLength = 50000` and `SecondaryTriggerDelay = 2` itself (that parameter only reaches `globalParameters.json` in Task 9). It pulses `DDS_Analog_Trg` at `Parameters["TCLBlockStart"]`.
- Test: `MOTMasterTests/LinkedScriptBuilderTests.cs`, which loads scripts with a CodeDom loader copied from `ScriptCompileCheck` (references `MOTMaster.exe` and `DAQ.dll` from the harness folder)

**Interfaces:**
- Consumes: Tasks 1–4.
- Produces (namespace `MOTMaster`):
  - `public sealed class LinkedBuild { MOTMasterScript Script; MOTMasterSequence Sequence; LinkRouting Routing; int AnalogOrigin; int TriggerDelay; int DigitalLength; }`
  - `public static LinkedBuild BuildPrimary(MOTMasterScript script, Hardware local, Hardware other, MOTMasterLinkConfig config)`:
    - runs `script.GetSequence()` inside `LinkRouting.ForPrimary(local, other, config.TriggerChannel, config.AnalogStartChannels)`
    - throws `LinkedRunException` containing `"uses no channels on"` and the other hardware's name if `!UsedOtherChannels`
    - throws if `UsedOtherAnalog` and `AnalogOrigin()` is null (`"no analog start"`, naming the configured channels)
    - after leaving the context, adds `sequence.DigitalPattern.Pulse(0, 0, config.TriggerPulseTicks, config.TriggerChannel)`
    - `TriggerDelay = (int)script.Parameters["SecondaryTriggerDelay"]`; a missing key throws `LinkedRunException` containing `"SecondaryTriggerDelay"` and `"globalParameters.json"`
    - `DigitalLength = (int)Parameters["PatternLength"]`
    - does **not** call `BuildPattern`
  - `public static LinkedBuild BuildSecondary(LinkRunRequest request, Hardware local, Hardware other, Func<string, MOTMasterScript> compileAndLoad)`:
    - writes `ScriptText` to `%TEMP%\MOTMasterLinked\<LinkRunId>\<ScriptFileName>`
    - with `MOTMasterScript.LinkedParameterOverride = request.Parameters` (cleared in a `finally`), calls `compileAndLoad(path)`, which throws `LinkedRunException` on failure
    - sets `script.Parameters = new Dictionary<string, object>(request.Parameters)`
    - `D = (int)Parameters["SecondaryTriggerDelay"]`
    - runs `GetSequence()` inside `ForSecondary(local, other, D, request.AnalogOrigin)`
    - `DigitalLength = PatternLength − Math.Max(0, D)`
    - throws `LinkedRunException` containing `"no digital events on its master pattern board"` if `Sequence.DigitalPattern.Boards` lacks `local.GetInfo("PatternGeneratorBoard")`
  - `public static void BuildPatterns(LinkedBuild b)` calls `DigitalPattern.BuildPattern(b.DigitalLength)`, `AnalogPattern.BuildPattern()` and `AnalogStatic.BuildPattern()`.
  - `public static string ScriptPathFor(LinkRunRequest r)` returns the temp path above.

- [ ] **Step 1: Write the failing tests.** Fixtures:
  - `rb = new RbTweezerHardware()`
  - `caf = new CafWithTrigger()`, a test class deriving from `MoleculeMOTHardware`. After the base constructor it calls `AddDigitalOutputChannel("linkTestTrigger", "/PXI1Slot4", 3, 7)`, so the test has a trigger channel without touching the real hardware file. A shared bit is harmless in a build-only test.
  - config: `TriggerChannel = "linkTestTrigger"`, `AnalogStartChannels = {"DDS_Analog_Trg","aoPatternTrigger"}`, `TriggerPulseTicks = 10`

```csharp
public static void TestPrimaryBuildsOnlyCafAndAddsTrigger()
    // BuildPrimary(..LinkedExample..): no RbTweezer device among DigitalPattern.Boards/AnalogPattern.Boards; AnalogOrigin == TCLBlockStart;
    // after BuildPatterns the master board's "linkTestTrigger" bit is 1 on ticks 0..9 and 0 at tick 10
public static void TestSecondaryBuildsOnlyRbAtMappedTimes()
    // request built from the primary result with SecondaryTriggerDelay=2: an RbTweezer digital edge written at script t=1000 is at tick 998 on /PXI1Slot5;
    // an RbTweezer analog value written at analog t=100 is at key 100+TCLBlockStart-2 in AnalogPatterns["tMOTcc"]; no CaF device in either builder
public static void TestFingerprintsAgreeAcrossSides() // OfParameters(primary.Script.Parameters) == OfParameters(secondary.Script.Parameters); OfText(file text) equal
public static void TestLinkedOnButNoSecondaryChannels() // a real CaF script ("2.a DDSBlueMOT.cs") through BuildPrimary -> "uses no channels on"   [Review Focus 4]
public static void TestDelayOverridePerRun()          // the same request with SecondaryTriggerDelay 2, then 7: the edge moves from 998 to 993   [Review Focus 5]
public static void TestMissingDelayParameter()        // Parameters without SecondaryTriggerDelay -> "SecondaryTriggerDelay" and "globalParameters.json"
```
The tests take the repo root from the harness `args` to find the script files.

- [ ] **Step 2: Run the tests and confirm they fail.**
- [ ] **Step 3: Implement `LinkedScriptBuilder`.**
- [ ] **Step 4: Build under CaF and RbTweezer, then run the harness.** Expected: all tests pass.
- [ ] **Step 5: Commit** `feat(motmaster): LinkedScriptBuilder builds primary/secondary sequences for a linked script`

---

### Task 6: `Controller` secondary side, and saving link info

**Files:**
- Modify: `MOTMaster/Controller.cs`
- Modify: `MOTMaster/MMDataIOHelper.cs`

**Interfaces:**
- Consumes: Tasks 1–5.
- Produces:
  - `Controller : MarshalByRefObject, ILinkSecondary`, implementing the five `ILinkSecondary` methods
  - `public bool IsLockedByPrimary => secondaryState.IsClaimed;`, `public string LockedBy`
  - `public void BreakLink()`
  - `public event Action LinkStateChanged`, forwarded from `LinkedSecondaryState.Changed`
  - `MMDataIOHelper.LinkInfo` (`public Dictionary<String, Object>`, default null). When non-null, `saveToFiles` also writes `storeDictionary(saveFolder + fileTag + "_link.txt", LinkInfo)`.

Behaviour:
- **Guards.** The first line of `Go(Dictionary)`, `SetScriptPath`, `SetTriggered`, `Run()`, `Run(dict)` and `RunReplica` is `secondaryState.ThrowIfClaimed("<method name>")`. Nothing else in those methods changes.
- **`ClaimLink(primaryName)`** returns `secondaryState.Claim(primaryName, IsRunning)` and logs it with `SharedCode.AppLog.Write`.
- **`ReleaseLink(token)`** releases the claim. **`BreakLink()`** sets `status = stopped`, then calls `secondaryState.Break()`.
- **`PrepareLinkedRun(token, request)`**:
  1. `Validate` the token.
  2. Take the run claim exactly as `Go` does (`runLock`/`runInProgress`); throw `LinkedRunException` if a run is already in progress.
  3. Call `LinkedScriptBuilder.BuildSecondary(request, Environs.Hardware, other, compileAndLoadOrThrow)` and `BuildPatterns`. `other` is created with `Type.GetType(request.PrimaryHardwareType, true)` + `Activator.CreateInstance`. `compileAndLoadOrThrow` wraps `compileFromFile` + `loadScriptFromDLL`; it throws `LinkedRunException` with the compiler errors from `results.Errors` instead of showing a MessageBox.
  4. Compute `LinkReadyInfo` from the built script's parameters, `config.DigitalPatternClockFrequency` / `AnalogPatternClockFrequency`, `BuildProvenance.Describe()` and `routing.ClampedEvents`.
  5. Start a thread running `linkedSecondaryLoop`, then return.
  - If any step before the thread starts fails, release the run claim, clear `LinkedParameterOverride`, and rethrow.
- **`linkedSecondaryLoop`**:
  - `secondaryState.RunStarted()`
  - while `status == running`: `initializeHardware(sequence, forceTriggered: true)`, then `run(sequence, sleepAfterStart: false)`, then `secondaryState.MarkArmed()`, then the existing busy-wait on `pgMaster.TaskRunning`, then `releaseHardware()`
  - afterwards, if `request.SaveEnable`: set `ioHelper.LinkInfo` (Role `"secondary"`, Partner = `request.PrimaryName`, hashes, both provenances when known, `AnalogOrigin`, delay, clamps), save with the request's batch number, then set `LinkInfo` back to null
  - `catch`: `secondaryState.Fail(ex.Message)` and log
  - `finally`: `setSafeOutputs()`, `clearDigitalPattern`, release the run claim, `secondaryState.RunEnded()`, delete the temp script folder (best effort)
  - It never calls `armDDS`.
- **`EndLinkedRun(token)`** validates the token, then sets `status = stopped`.
- **`GetLinkShotStatus(token)`** returns `secondaryState.Status(token)`.
- **Signature changes, defaults identical to today:**
  - `private void run(MOTMasterSequence sequence, bool sleepAfterStart = true)`: its last line becomes `pgMaster.OutputPattern(pattern, sleepAfterStart)`
  - `private void initializeHardware(MOTMasterSequence sequence, bool forceTriggered = false)`: the `triggered` test becomes `triggered || forceTriggered`
- **Batch number.** The save helpers currently read `controllerWindow.GetSaveBatchNumber()`. Give `save(...)` an optional `int? batchNumber` parameter. The secondary passes the request's number; other callers are unchanged.

- [ ] **Step 1: Implement**, then build under CaF and RbTweezer. Expected: both build clean. This path drives hardware, so it is verified by build and by review, not by running it.
- [ ] **Step 2: Run the harness and `verify-scripts.ps1`.** Expected: unchanged results.
- [ ] **Step 3: Self-review against the spec**, sections "Locking the secondary" and "Shot sequencing" steps 2, 4, 5 and 6. In particular, check the `finally` ordering: safe outputs are set before the claim is released.
- [ ] **Step 4: Commit** `feat(motmaster): secondary side of linked runs (claim, prepare, triggered shot loop, link info in saves)`

---

### Task 7: `Controller` primary side, and errors that reach Python

**Files:**
- Modify: `MOTMaster/Controller.cs`

**Interfaces:**
- Consumes: Tasks 1–6.
- Produces:
  - `public void SetLinked(bool value)` and `public bool IsLinked` (remotable)
  - `public MOTMasterLinkConfig LinkConfig`: null when the hardware declares no link

Behaviour:
- **`StartApplication`** reads `LinkConfig = Environs.Hardware.GetInfo(MOTMasterLinkConfig.InfoKey) as MOTMasterLinkConfig`. It also adds the release to the existing `ApplicationExit` handler: `if (linkedPrimary != null) linkedPrimary.Release();`.
- **`SetLinked(true)`**:
  - throws `LinkedRunException` if `LinkConfig == null` (`"no link configured in <hardware>"`) or if `IsRunning`
  - proxy = `(ILinkSecondary)Activator.GetObject(typeof(Controller), LinkConfig.SecondaryUrl)`
  - `otherHardware = (Hardware)Activator.CreateInstance(Type.GetType(LinkConfig.SecondaryHardwareType, true))`
  - `lp = new LinkedPrimary(proxy, LinkConfig, log)`, then `lp.Claim(System.Environment.MachineName)`. Only after that succeeds: `linkedPrimary = lp`, update the window.
- **`SetLinked(false)`** releases and nulls `linkedPrimary`.
- **In `goClaimed`, when `linkedPrimary != null` at the start of the run** (captured in a local `link`):
  - `prepareScript` returning null throws `LinkedRunException("script did not compile or load")`.
  - The sequence comes from `LinkedScriptBuilder.BuildPrimary(script, Environs.Hardware, otherHardware, LinkConfig)` instead of `getSequenceFromScript`.
  - The DDS arming block is unchanged and runs first, exactly as now.
  - `buildPattern(sequence, build.DigitalLength)`.
  - Build a `LinkRunRequest`:
    - `LinkRunId = Guid`, `PrimaryName = MachineName`
    - `PrimaryHardwareType = Environs.Hardware.GetType().FullName + ", DAQ"`
    - `ScriptFileName = Path.GetFileName(scriptPath)`, `ScriptText = File.ReadAllText(scriptPath)`
    - `Parameters = script.Parameters`, `AnalogOrigin = build.AnalogOrigin`
    - `SaveEnable = saveEnable`, `BatchNumber = controllerWindow.GetSaveBatchNumber()`
  - Call `link.Begin(...)` with the local hashes, clocks and provenance.
  - Before every `runPattern(sequence)` in both loops: `if (!link.WaitUntilArmed(shot, () => status == RunningState.running)) break;`, with a 1-based `shot` counter.
  - After the loops: `link.End()`. In the method's `finally`: `if (begun && !ended) link.Abort();`.
  - Saving: `ioHelper.LinkInfo = link.SaveInfo` around `save(...)`, then null.
- **The two `InsufficientPatternLengthException` catches** gain `when (link == null)`, so linked runs propagate to `Go`'s existing rethrow. Non-linked behaviour is unchanged.
- **`ViewPattern()`** uses `LinkedScriptBuilder.BuildPrimary` when `linkedPrimary != null`, so linked scripts open instead of throwing. It shows primary channels only. **Follow-up, deferred as the spec allows:** both systems on one timeline.
- **`GetParameters()` / `GetSwitchConfiguration()`** are unchanged.

- [ ] **Step 1: Implement**, then build under CaF and RbTweezer. Expected: both clean.
- [ ] **Step 2: Run the harness and `verify-scripts.ps1`.** Expected: unchanged.
- [ ] **Step 3: Self-review against the spec**, sections "Shot sequencing" steps 1, 3, 4 and 5, "Errors in linked runs always throw", and "Python". Confirm by reading `goClaimed` that with `link == null` every statement executed is the same as before.
- [ ] **Step 4: Commit** `feat(motmaster): primary side of linked runs; linked failures always throw to the caller`

---

### Task 8: GUI on both sides

**Files:**
- Modify: `MOTMaster/ControllerWindow.cs`, `MOTMaster/ControllerWindow.Designer.cs`

**Interfaces:**
- Consumes: `Controller.LinkConfig`, `IsLinked`, `SetLinked`, `IsLockedByPrimary`, `LockedBy`, `BreakLink`, `LinkStateChanged`.
- Produces:
  - `linkedCheckBox`, shown only when `controller.LinkConfig != null`, labelled `"Linked to <host from SecondaryUrl>"`. When checked it calls `controller.SetLinked(true)`. On a `LinkedRunException` it shows a MessageBox with the message and unchecks the box without calling `SetLinked(false)`.
  - `linkBannerLabel`, shown only while locked: `"Controlled by <LockedBy> (linked run)"`, bold, on a highlight background.
  - `breakLinkButton`, shown only while locked. It asks for confirmation ("Break the link to <LockedBy>? Only do this if the primary MOTMaster has crashed or closed.") and then calls `controller.BreakLink()`.
- **`RefreshRunControls()`** additionally sets `enabled &= !controller.IsLockedByPrimary` for `runButton`, `ReplicateScriptToolStripMenuItem`, `selectScriptButton`, `lookupScriptsButton`, `scriptListComboBox` and `triggeredCheckBox`. It also updates the banner and the Break button. Subscribe it to `LinkStateChanged`.
- Place the new controls in free space without moving existing ones.

- [ ] **Step 1: Implement**, then build under CaF and RbTweezer. Expected: clean.
- [ ] **Step 2: Hand the user a manual GUI checklist**, without launching MOTMaster:
  - primary shows the checkbox only on PH-BONESAW
  - the secondary banner, the greyed controls, and Break link with its confirmation
  - unchecking releases the secondary
- [ ] **Step 3: Commit** `feat(motmaster): linked-mode controls on the primary, lock banner and Break link on the secondary`

---

### Task 9: Configure PH-BONESAW → RbTweezer, final verification

**Files:**
- Modify: `DAQ/MoleculeMOTHardware.cs`
- Modify: `MoleculeMOTMasterScripts/globalParameters.json`
- Modify: `.claude/skills/motmaster-controller/SKILL.md` and `.claude/skills/motmaster-python-integration/SKILL.md`, only if the user agrees (see Step 4)

- [ ] **Step 1: Stop and ask the user** which free line on the master pattern board `/PXI1Slot4` (`digitalPatternBoardAddress2`) is wired to RbTweezer `/PXI1Slot5/PFI5`. Grep the `(board, port, line)` triple in `MoleculeMOTHardware.cs` to confirm nothing uses it. Don't guess.
- [ ] **Step 2: Add the configuration.**
  - Channel: `AddDigitalOutputChannel("secondaryMOTMasterTrigger", digitalPatternBoardAddress2, <port>, <line>);`
  - Next to `MotMasterConfiguration`, add:

```csharp
Info.Add(MOTMasterLinkConfig.InfoKey, new MOTMasterLinkConfig
{
    SecondaryUrl = "tcp://10.59.3.2:1187/controller.rem",
    SecondaryHardwareType = "DAQ.HAL.RbTweezerHardware, DAQ",
    TriggerChannel = "secondaryMOTMasterTrigger",
    AnalogStartChannels = new[] { "DDS_Analog_Trg", "aoPatternTrigger" },
});
```

  - Add `{"name":"SecondaryTriggerDelay","value":"0","type":"System.Int32"}` to the `General` group of `globalParameters.json`, keeping alphabetical order.
- [ ] **Step 3: Final verification.**
  - Build MOTMaster under CaF and RbTweezer: both clean.
  - `MOTMasterTests\run-tests.ps1`: all pass.
  - `verify-scripts.ps1`: the same pass/fail set as recorded in Task 2 Step 5.
- [ ] **Step 4: Ask the user** whether to record in the skills:
  - `motmaster-controller`: linked mode's existence and where its code lives, `LinkRouting` and the "no context = unchanged" rule, and the test harness
  - `motmaster-python-integration`: `SetLinked`, and the fact that linked failures always throw
  Edit them only if the user says yes.
- [ ] **Step 5: Commit** `feat(caf): link PH-BONESAW MOTMaster to RbTweezer as secondary`
- [ ] **Step 6: Hand over the first-live-run checklist.** The user does all of this.
  1. Deploy the branch build to both PCs, CaF and RbTweezer configurations.
  2. Confirm `10.59.3.2:1187` is reachable from PH-BONESAW.
  3. Tick Linked on the primary and check the banner on the secondary.
  4. Run a minimal linked script with a scope on the trigger line and one RbTweezer output, and measure the delay to set `SecondaryTriggerDelay`.
  5. Check both run zips: matching `LinkRunId` and hashes in `_link.txt`.
