# Linked MOTMaster: one script across two MOTMaster systems

Date: 2026-10-22
Status: draft for review
Branch: `feature/linked-motmaster`

## Intent

One physical experiment is driven by two sets of NI pattern hardware in different
places, each owned by its own MOTMaster on its own PC. Today that means two scripts
with duplicated parameters, kept in sync by hand, plus manually putting the second
MOTMaster into triggered mode before each run.

This feature lets a user write **one ordinary MOTMaster script** that uses channels
from both machines' hardware files. They select and run it on the **primary**
MOTMaster, either from the GUI or from Python through pythonnet. The primary takes
control of the **secondary** over TCP, gives it the same script and parameters, arms
it in triggered mode, and starts it with a TTL from one of the primary's digital
lines. Times in the script mean the same physical moment on both systems, using the
primary's time conventions, after a configurable offset for trigger and cable delay.

First use: **PH-BONESAW (cafmot, `MoleculeMOTHardware`) as primary**, and **RbTweezer
(10.59.3.2, `RbTweezerHardware`) as secondary**. The mechanism must be general: any
pair of experiments on this repo can be linked by configuration alone.

## Goals

- A single `.cs` script, written in the usual style (`PatternBuilder32` /
  `AnalogPatternBuilder` calls with channel names and times), drives channels on both
  systems.
- Each channel's owner is worked out from the two hardware classes. Scripts don't tag
  channels with a system.
- Time conventions follow the primary's (cafmot's) definitions for digital and analog
  patterns, even for channels on the secondary.
- One action on the primary (GUI Run, or Python `Go(dict)`) runs the whole linked
  experiment, including arming the secondary in triggered mode.
- The secondary runs **exactly** the script text and parameter values the primary
  ran, guaranteed by construction and checked before every run.
- A configurable trigger-delay offset is applied to the secondary's timings.
- It all works from Python through the existing remoting object, with no change
  needed to existing `SetScriptPath` / `Go(dict)` scan loops.
- While linked, the secondary can't be driven by anything except the primary.

## Non-goals

- More than one secondary. The design shouldn't rule it out, but v1 supports exactly
  one.
- Sharing a clock reference between the machines. There's no hardware for this at
  the moment (see Risks: clock drift).
- Guaranteeing that the two machines run identical MOTMaster/DAQ builds. That gets
  recorded and warned about, never enforced (see Build provenance).
- Camera and image handling. pytweezer runs as a TCP server setup across several
  machines and already takes care of image data. This feature doesn't touch it.
- Changing anything about camera, DDS or hardware-controller behaviour on either side.
- Changing how non-linked runs behave.

## Anti-goals (would count as failure even if it "works")

- **Any change to behaviour for experiments that don't use linking.** `Controller.cs`,
  `MOTMasterScript.cs`, `DAQMxPatternGenerator` and the DAQ pattern builders are
  shared by every experiment on the suite. With linking off, or on a machine with no
  link configured, every code path must behave exactly as it does today.
- The primary firing a shot while the secondary isn't armed, or isn't running the same
  script and parameters. Every such case aborts the shot.
- The secondary being run locally (GUI button, its own Python) while it belongs to the
  primary.
- Hidden per-machine parameter drift. The secondary's local `globalParameters.json`
  must not leak into a linked run.
- A silent timing error. Any ambiguity about where a secondary event lands is an
  error, not a guess.

## Safety

Per the repo `CLAUDE.md`: nothing in development or testing runs a script, fires a
pattern or touches NI hardware. Verification is by compilation (`msbuild`,
`verify-scripts.ps1`) and by unit-level tests of pure logic: channel partitioning,
time mapping, fingerprinting, and the arm/lock state machine with the hardware calls
stubbed. The first live linked run is done by the user.

## Current state: facts the design relies on

**Remoting and run flow**
- `MOTMaster/Runner.cs` publishes `Controller` over .NET Remoting:
  `TcpChannel(1187)`, `controller.rem`. pycaf / pytweezer connect with
  `Activator.GetObject(MOTMaster.Controller, "tcp://<host>:1187/controller.rem")` and
  call `SetScriptPath`, `Go(dict)`, `SetTriggered`, `SetIterations`,
  `SetRunUntilStopped`, `SaveToggle`, `GetParameters`. Any new public method on
  `Controller` is automatically callable from Python.
- `Controller.SetTriggered(bool)` already exists. With it on, `initializeHardware`
  configures `pgMaster` to wait for a start trigger on the hardware's
  `"PGTriggerLine"` Info (RbTweezer: `/PXI1Slot5/PFI5`).
- A run is `Go(dict)` → `goClaimed`: `prepareScript` (compile file + `EditDictionary`)
  → `getSequenceFromScript` → `buildPattern` → loop of `runPattern` → `save`.
  `runPattern` is `initializeHardware` → `run` → busy-wait on
  `pgMaster.TaskRunning && status == running` → `releaseHardware`. `Go` holds
  `runLock` / `runInProgress` so that only one run happens at a time.
- `run()` calls `pgMaster.OutputPattern(pattern)`, which writes, starts the task and
  then **sleeps for one full pattern length** (`SleepOnePattern`) before returning.
  `OutputPattern(pattern, false)` exists and returns right after `pgTask.Start()`.
  So `Stop()` can't cut short a triggered wait that's inside that sleep, and
  "armed" can't be reported from after `run()` returns.
- Errors that **don't** reach a remote caller today: a failed script compile
  (`prepareScript` returns null and shows a MessageBox) and both
  `InsufficientPatternLengthException`s (caught in `goClaimed` and shown in a
  MessageBox). In all three cases `Go` returns normally. Other exceptions are logged
  and rethrown to the caller.

**Scripts and time conventions**
- Scripts construct their own `PatternBuilder32` / `AnalogPatternBuilder`, which look
  each channel name up in `Environs.Hardware.DigitalOutputChannels` /
  `AnalogOutputChannels` (`Hashtable`; a miss gives `null` and then a
  `NullReferenceException` in `GetBoard`). So channel routing has to happen inside
  the builders, not in the script.
- **cafmot has two time zeros.** Digital times count from the pattern generator
  start, tick 0. Analog and DDS times count from the `DDS_Analog_Trg` pulse, which
  scripts fire at `TCLBlockStart` (4000 ticks by default; e.g.
  `2.a DDSBlueMOT.cs:210`). That pulse triggers both the DDS and the analog board.
  Some older scripts use the `aoPatternTrigger` channel for the same purpose.
- **RbTweezer has one time zero.** Its `AOPatternTrigger` is `pgBoard/do/StartTrigger`,
  so analog tick 0 is the same as digital tick 0.
- `MOTMasterScript.LoadGlobalParameters()` reads `globalParameters.json` from the
  **local** `scriptListPath`. On the secondary that's a different file.
- `DDSPatternBuilder` (in `MOTMasterScript.cs`) is plain .NET collections code with no
  dependency on `SpectrumDDS`. It's inside `#if DDS` only, so RbTweezer's MOTMaster
  can't compile the 21 top-level cafmot scripts that use it. `GetDDSPattern()` itself
  is virtual on the base class in every build. The `MOTMaster.SnippetLibrary` classes
  (e.g. `LoadMoleculeMOTNoSlowingEdge`) are compiled into every build.

**Hardware**
- Both hardware classes are compiled into **every** configuration of `DAQ.dll`
  (unconditional `<Compile Include>` in `DAQ.csproj`). Constructing
  `RbTweezerHardware` only fills name→channel tables (`AddDigitalOutputChannel`,
  `AddAnalogOutputChannel`, `Info.Add`) and opens no hardware. The builder must
  confirm this for the `Hardware` base constructor and for `WavemeterLockConfig`
  before relying on it.
- `MoleculeMOTHardware` and `RbTweezerHardware` share no output channel names today.
  Most of RbTweezer's start with `t`, but not all (e.g. digital `aoTrigger`).
- Both machines use the `MMConfig` default 100 kHz digital and analog clocks.
  `pgMaster` on each runs on its own internal timebase. On cafmot, the second PG
  (`PGSlave0`) is externally clocked and starts on a `slavePatternCardTrigger` pulse,
  so it lags master tick 0 slightly.
- There is precedent for "wait until a remote device is armed before each shot": the
  DDS path, `ddsController.WaitUntilArmed(1.0)` in `goClaimed`.

## Design: decided parts

### Roles and link configuration

- A machine can act as primary only if its `Hardware` subclass declares a link in
  `Info`, following the existing `Info.Add("MotMasterConfiguration", mmConfig)`
  pattern. The link holds:
  - the secondary's remoting URL (`tcp://10.59.3.2:1187/controller.rem`),
  - the secondary's `Hardware` class (`DAQ.HAL.RbTweezerHardware`),
  - the name of the primary's digital output channel wired to the secondary's
    `PGTriggerLine`. This must be on the **master** pattern board (cafmot's second PG
    starts late). The physical line is chosen by the user before the first live run.
  - the name(s) of the primary's analog-start channel(s) (cafmot: `DDS_Analog_Trg`,
    and `aoPatternTrigger` for older scripts). See Timing.
  A machine with no link entry shows no new UI and runs exactly as today.
- Linked mode is switched on and off at runtime on the primary: a GUI checkbox, plus a
  remotable `SetLinked(bool)` for Python. It is off at startup.

### Channel ownership (no TCP involved)

- When linked mode is on, the primary creates the secondary's `Hardware` class
  locally and reads its digital and analog output channel names.
- Each channel name a script uses is owned by the primary if it's in the primary's
  `Environs.Hardware`, and by the secondary if it's in the secondary's class. A name
  in **both** or in **neither** is an error before anything is armed, and the message
  names the channel.
- Each machine's builders emit only the channels that machine owns, apply that
  machine's time mapping, and ignore the other machine's channels. This is controlled
  by a routing context the `Controller` sets around `GetSequence()`. **When no
  context is set (all non-linked runs), the builders behave exactly as today,
  including today's failure on an unknown name.**

### Same script, same parameters, by construction

- The secondary never uses its own copy of the script. The primary sends the **script
  file's text**, and the secondary compiles that text (written to a temp file, since
  `compileFromFile` takes a path).
- **DDS scripts:** `DDSPatternBuilder` moves outside `#if DDS` so that every build can
  compile scripts that use it. This is purely additive: the code that arms and drives
  the Spectrum card stays CaF-only, and RbTweezer doesn't need the DDS software. The
  secondary ignores the sequence's DDS pattern. The primary arms its own DDS exactly as
  today.
- The primary resolves the **complete** parameter set (script defaults, then the
  primary's `globalParameters.json`, then any `Go(dict)` overrides) and sends it. On
  the secondary this set **replaces** `script.Parameters` outright after
  instantiation; it is not merged. The secondary's own `globalParameters.json` has no
  effect on linked runs.
- **Fingerprint check:** the secondary computes a hash of the script text it compiled
  and of the parameter set it ended up with, in a canonical, order-independent form.
  It returns both when it reports ready. The primary compares them with its own, and
  any mismatch aborts the run before the first shot fires.
- Both fingerprints and a shared link-run ID (made by the primary per `Go`) are saved
  in both machines' run zips, so a pair of runs can be matched and checked
  afterwards.

### Locking the secondary

- When the primary turns linked mode on, it **claims** the secondary over remoting.
  While claimed, the secondary:
  - refuses `Run`/`Go`/`SetScriptPath`/`SetTriggered` from anyone other than the
    primary's link calls, with a clear error message (this covers pytweezer or any
    other Python on the secondary side too);
  - disables its Run button and script selection, and shows a banner such as
    "Controlled by PH-BONESAW (linked run)".
- Claiming fails if the secondary is already running or already claimed by someone
  else. The primary then reports this and stays unlinked.
- The primary releases the claim when linked mode is turned off or MOTMaster closes
  normally.
- **My call:** if the primary dies without releasing, the secondary stays locked until
  a user presses a "Break link" button on the secondary, after a confirmation prompt.
  There's no automatic timeout, because Python scans can legitimately pause for a
  long time between shots, and an auto-release would let a local run slip in
  mid-scan.

### Timing, trigger and offset

All script times follow the **primary's** conventions, as the user decided:

- **Digital** times (in `GetDigitalPattern`) count from primary PG tick 0.
- **Analog** times (in `GetAnalogPattern`) count from the primary's analog start, which
  is the first rising edge of the configured analog-start channel (cafmot:
  `DDS_Analog_Trg`) in the primary's built digital pattern. Call that tick `A`.
- **My call:** `A` is read from the built primary pattern, not from a named
  parameter, so it stays correct whatever a script calls its timing parameters. If no
  configured analog-start channel has a rising edge, or the edge is ambiguous (more
  than one configured channel fires, at different ticks), and the script has
  secondary analog events, the run stops with an error.

Trigger and mapping onto the secondary, whose analog and digital zeros coincide:

- **My call:** MOTMaster adds the trigger pulse to the configured trigger channel
  automatically at primary tick 0 (a short fixed-width pulse; width is the builder's
  call). Scripts don't write it, and a script that also drives that channel is an
  error.
- A new parameter **`SecondaryTriggerDelay`** (in clock ticks, like every other
  script time) sets the offset. It is an ordinary parameter (default in the primary's
  `globalParameters.json`, overridable per script), so it is saved with every run and
  can be scanned from Python. It may be zero or negative.
- A secondary **digital** event at script time `t` lands at secondary tick
  `t − SecondaryTriggerDelay`.
- A secondary **analog** event at script time `t` lands at secondary tick
  `t + A − SecondaryTriggerDelay`.
- **My call (events before the trigger):** a single-tick event (`AddEdge`,
  `AddAnalogValue`) that would land before secondary tick 0, typically an initial value
  at `t = 0`, is moved to tick 0, and the run log lists each one. If moving them would
  change the order of two events on the same channel, that is an error. A multi-tick
  operation (`Pulse`, `DownPulse`, `AddAnalogPulse`, `AddLinearRamp`,
  `AddPolynomialRamp`) that would start before tick 0 is **an error**, never shifted
  or truncated, because either would silently change a ramp or pulse.
- The secondary's pattern length is whatever its mapped events need, within the
  script's `PatternLength`. The builder decides the exact rule, as long as the
  secondary ends no later than the primary.
- The run refuses to start if the two machines' digital or analog clock frequencies
  differ.

### Shot sequencing

For one primary `Go(dict)` with linking on:

1. Primary: compile and prepare the script as usual. Build its own sequence with
   routing set to "primary". Check channel ownership and the trigger channel, and find
   `A`.
2. Primary → secondary: `PrepareLinked(linkRunId, scriptText, fullParameters, A,
   iterations | runUntilStopped, saveEnable, batchNumber)`. The secondary compiles,
   replaces the parameters, builds its sequence with routing set to "secondary" and
   the time mapping applied, and returns fingerprints, its clock frequencies and its
   build provenance, or an error. It then starts its own run thread in triggered mode
   (the call returns immediately).
3. Primary: compare fingerprints and clocks, and abort on mismatch or error.
4. For each shot: the secondary configures and starts its PG task and **only then**
   reports that this shot is armed (its PG is waiting for the trigger). Because
   `OutputPattern(pattern)` sleeps for a whole pattern after starting, the secondary's
   linked path must use `OutputPattern(pattern, false)` or an equivalent, and signal
   "armed" straight after the task starts. This is a linked-only path; the existing
   call is unchanged for everyone else. The primary waits for that signal, with a
   timeout in the style of the DDS `WaitUntilArmed`, then runs its own `runPattern`,
   which emits the TTL. A timeout, or the secondary reporting an error, stops the run
   on both sides.
5. End of run (iterations done, `Stop()` on the primary, or an error): the primary
   tells the secondary to stop. The secondary must be able to abandon a triggered
   wait promptly: stop the task and release hardware, then set safe outputs through
   its existing `finally`.
6. Each machine saves its own run zip as usual, if saving is enabled. The secondary's
   zip contains the script text it actually compiled, the full parameter set, the
   link-run ID and the fingerprints.

`Stop()` on the primary (GUI or Python) stops both machines.

**Errors in linked runs always throw.** Any failure of a linked run (a compile error on
either side, a pattern-length error, a fingerprint or clock mismatch, an arm timeout, a
secondary error) must surface as an exception from the primary's `Go`, so Python sees
it. It must not end in a MessageBox-and-return. Non-linked runs keep today's
MessageBox behaviour unchanged.

### Build provenance (warn and record, never block)

- The two machines build under different configurations (e.g. `CaF` with `DDS`
  versus `RbTweezer`), so their `DAQ.dll` / `MOTMaster.exe` binaries can never be
  byte-identical. No binary check is attempted.
- Each machine records the source revision its MOTMaster is running (git commit, plus
  a flag for local uncommitted changes) and its build timestamp. The secondary returns
  this from `PrepareLinked`. Both machines save it in the run zip, and the primary
  logs a clear warning if the revisions differ. Whether the commit is captured at build
  time or read from the repo at startup is the builder's call.

### Python

- Everything is public methods on the existing `Controller` remoting object, so
  Python needs only, for example, `motmaster.SetLinked(True)` once on the primary.
  Existing `SetScriptPath` / `Go(dict)` scan loops then run linked unchanged.
- Linked scans are driven through the **primary**. While the secondary is linked, its
  own `Go` is refused (see Locking). pytweezer's multi-machine TCP setup handles image
  data independently and is out of scope.

### GUI

- Primary: a "Linked" checkbox, shown only when the hardware declares a link. The run
  log and status show the link state (claimed / armed / error) and any
  build-revision warning.
- Secondary: the lock banner, the disabled Run button and script selection, and
  "Break link".
- **My call:** "View Pattern" on the primary shows both systems' channels, with the
  secondary's mapped onto primary digital time. The builder may defer this to a
  follow-up if it turns out large.

## Left to the builder

- Exact names and signatures of the new remotable methods, and the shape of the link
  `Info` object.
- How the routing context reaches the builders (e.g. a static/ambient context in
  `DAQ.Pattern`, set and cleared by `Controller` in a `try/finally`), provided the
  no-context path is byte-for-byte today's behaviour.
- How the secondary abandons a triggered wait promptly on stop (step 5), within the
  same "linked-only path" constraint.
- Fingerprint algorithm and canonical parameter serialisation.
- Trigger pulse width, arm-wait timeout value, and where the link metadata goes inside
  the run zip.
- Whether the remoting calls to the secondary need a lease/timeout adjustment (see
  `InitializeLifetimeService` in `Controller`).
- Tests for the pure logic (ownership partitioning, time mapping including `A` and the
  clamp/error rules, fingerprinting, claim state machine with hardware stubbed), plus
  `verify-scripts.ps1` and `msbuild` for both the `CaF` and `RbTweezer`
  configurations.

## Risks and open items

- **Clock drift between the machines.** Each `pgMaster` runs on its own timebase, and
  there is no hardware to share a reference right now. Drift is a fractional error, so
  it's the same whether you count base-clock or 100 kHz ticks. It builds up only from
  each shot's trigger, because every shot is retriggered, and its size depends on the
  two chassis' reference accuracy. As an example, a 50 ppm mismatch gives about 50 µs
  (5 ticks) one second after the trigger. Sharing a 10 MHz reference later would
  remove it without any change to this design.
- **Which physical line carries the trigger.** A master-board digital output on
  PH-BONESAW, wired to RbTweezer's `/PXI1Slot5/PFI5`. The user chooses it before the
  first live run, and it must be checked against existing `(board, port, line)` use.
- **Network reachability.** The primary must reach `10.59.3.2:1187`. The secondary's
  `TcpChannel(1187)` is not bound to localhost, but firewall rules haven't been
  checked.
