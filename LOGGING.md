# Event logging — where to look when something went wrong

Both MOTMaster and SpectrumDDSController keep a small, persistent, plain-text
record of the errors they hit and the events worth remembering. The point is to
make "what happened yesterday?" answerable from a file you can open in Notepad,
rather than from a 50–100 MB/day vendor log or from an in-memory tally that died
with the process.

These logs are written best-effort and are for diagnosis only. Nothing in the
experiment reads them, and nothing depends on them existing.

---

## The files

Both live in **`Logs\` at the root of the EDMSuite tree** — beside `EDMSuite.sln`,
alongside `MOTMaster\`, `DAQ\` and the rest. The folder is created on the first
write and is git-ignored, so the logs never show up in `git status`.

| File | Written by | What is in it |
|---|---|---|
| `Logs\motmaster_{date}.log` | MOTMaster | MOTMaster's own errors: crashes, script compile failures, pattern-length failures. |
| `Logs\controller_events_{date}.log` | SpectrumDDSController **and** MOTMaster | Everything DDS: end-of-run tallies, card errors, the shot loop dying, every complaint the controller window puts on screen. |
| `spcmdrv_debug.txt`, wherever the driver's log path points (conventionally `E:\SpectrumLog`) | the Spectrum driver (vendor) | Every driver call. Huge. Covered in [SpectrumDDS/README.md](SpectrumDDS/README.md#debug-logging), not here. |
| `SpectrumDDSController\bin\CaF\startup-error.txt` | SpectrumDDSController | Last-ditch record of a failure while the window was still being built. Overwritten each time; also copied into the dated log above. |

A new calendar day starts a new file — the date is in the filename, so there is
no rotation to go wrong and no chance of losing yesterday. **Nothing deletes
these files.** They are small (a normal day is a few kB), but clean them out by
hand occasionally, as with the vendor's.

The location is worked out at runtime, by walking up from wherever the
executable is until `EDMSuite.sln` turns up — the apps run out of
`<project>\bin\<config>\`, so that is two or three levels up. There is no
hard-coded drive letter anywhere: not every machine that runs this code has an
`E:`. If the marker is not found — a copied-out deployment with no source tree
around it — the logs land in a `Logs` folder beside the executable instead.

Note the vendor's driver log is **not** in here. That one goes wherever the
driver's own registry setting points, which the Status tab edits and this code
does not touch.

---

## The format

One line per event, tab-separated:

```
2026-08-13 14:22:07	Controller	14:22:07  sent   200  fired   198  missed    2  card triggers    1399  |  ready   <<< MISSED 2
2026-08-13 14:23:11	MainWindow: Could not arm the card	System.InvalidOperationException: the card is not open
   at SpectrumDDS.SpcmCard.CheckOpen() ...
```

`timestamp`, then `source`, then the message. Exceptions are written with their
full stack, so an entry can run to several lines — the timestamp at the start of
a line is what marks a new event.

The `source` column says where it came from, and is worth grepping:

**In `motmaster_{date}.log`**

| Source | Means |
|---|---|
| `Runner` | An unhandled exception. MOTMaster either died or put up the runtime's crash dialog. |
| `Controller.Go (analog pattern length)` / `(digital pattern length)` | `PatternLength` in the script is too short for the events it asks for. |
| `Controller.compileFromFile <path>` | The script would not compile. The path of the offending script is in the source column. |
| `Controller.loadScriptFromDLL` | It compiled but would not load. |

**In `controller_events_{date}.log`**

| Source | Means |
|---|---|
| `Controller` | An end-of-run tally — the same line the Status tab shows. |
| `MOTMaster` | The same tally as MOTMaster counted it. |
| `MOTMaster.armDDS (...)` | The run was stopped before it started: controller unreachable, card would not open, or card refused the pattern. |
| `MOTMaster.stopDDS`, `MOTMaster.ReportRunTally` | End-of-run housekeeping failed. The run itself still saved and reported. |
| `Controller.RunShots` | **The shot loop died mid-run.** Everything after this point in the run fired into a card that was no longer arming. |
| `Controller.WaitUntilArmed` | The card was closed from the GUI while MOTMaster was waiting on it. |
| `MainWindow: <what>` | Every error dialog the controller window has ever shown, with the stack the dialog did not show you. |
| `MainWindow.*` | Errors the window swallowed silently — status reads, pattern adoption, shutdown. |
| `Runner`, `Runner (startup)`, `Runner (remoting port 1818)` | The controller crashed, failed to start, or could not claim port 1818 (usually a leftover copy already running). |

---

## The two questions these exist to answer

**"Did we miss shots, and when?"** Grep the DDS log for `MISSED`:

```powershell
Select-String -Path Logs\controller_events_*.log -Pattern MISSED
```

Every run writes a tally whether it missed anything or not, so the absence of a
tally for a run means the run never reported — which is itself a finding. A run
that missed shots *with* a `QUEUE_UNDERRUN` in its status text was fired into
during the re-queue deadtime; one that missed them without was simply too slow
to arm, which usually points at the driver's debug log level. There is more on
that distinction in [SpectrumDDS/README.md](SpectrumDDS/README.md#debug-logging).

**"Why did it die?"** Look for `Runner` in whichever app's log, at the time it
went. Before this existed, a throw on MOTMaster's run thread killed the process
with nothing on screen and nothing on disk to say why.

Each run writes its tally twice, once from each side — `MOTMaster` counts what
it sent, `Controller` samples the card. They should agree. When they do not, the
disagreement is the interesting part.

---

## What these logs will not tell you

**A missing line is not proof that nothing happened.** Logging is deliberately
best-effort: every write is wrapped in a catch that swallows everything, so that
a full disk or an unwritable folder can never change what the experiment does.
If the write failed, the event simply did not get recorded. The apps behave
identically either way and will not warn you.

**They are not a substitute for the vendor log.** These record what the
applications did. What the *driver* did is in `spcmdrv_debug.txt`, and for
anything below the level of "the card refused this" you will still need it.

**MessageBox text is unchanged.** Nothing was moved out of a dialog and into a
log; the logging was added alongside. If you saw a dialog, its text and the
stack behind it are now also in the file.

---

## Changing where they go

Both loggers expose a public `Directory` field, set before first use:

```csharp
SharedCode.AppLog.Directory = @"D:\somewhere_else";
SpectrumDDS.DdsLog.Directory = @"D:\somewhere_else";
```

There is no settings UI and no `DAQ.Environment` wiring for this — the default
works on any machine with a copy of the source tree, and pointing it somewhere
else is a code change, on purpose. If you do move them out of the tree, note
that `.gitignore` only covers `/Logs/`.

---

## For the next person editing this

The two loggers are deliberately separate classes, not one shared one:

- [`SharedCode/AppLog.cs`](SharedCode/AppLog.cs) — `SharedCode` is already an
  unconditional project reference of MOTMaster, so every experiment gets this
  for free.
- [`SpectrumDDS/DdsLog.cs`](SpectrumDDS/DdsLog.cs) — SpectrumDDSController
  references `SpectrumDDS` and nothing else. Pointing it at `SharedCode` would
  drag DAQmx, Mongo and SharpZipLib into a deliberately standalone app.

They are otherwise mirror images. Both are `static`, both take one lock, both
retry a few times on `IOException` (the DDS file has two *processes* writing to
it), and both swallow everything.

**MOTMaster is shared by every experiment on this suite**, not just the
MoleculeMOT. Every logging call in it is one added line next to existing code —
no changed control flow, no changed dialog text, no changed return values. Keep
it that way. The DDS-specific calls in MOTMaster sit under `#if DDS`, which only
the CaF configuration defines, so they are inert for the other experiments.

One thing that is *not* purely additive and is worth knowing about: both
`Runner.cs` files attach a handler to `Application.ThreadException`. Merely
attaching to that event suppresses the `ThreadExceptionDialog` WinForms would
otherwise show, and silently continues — so both handlers reproduce the default
explicitly (show the same dialog, `Application.Exit()` on Abort). Do not
"simplify" that away. `AppDomain.UnhandledException` needs no such care; it is
notification-only and the process still terminates exactly as before.
