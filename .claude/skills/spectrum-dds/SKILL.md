---
name: spectrum-dds
description: Working with the Spectrum M4i.9622 DDS card that drives the MoleculeMOT (cafmot) AOMs — its driver (SpectrumDDS/), its GUI (SpectrumDDSController/), and its MOTMaster integration. Use this whenever the task touches the DDS card, RF/AOM frequencies or amplitudes, DDS patterns, GetDDSPattern, addDDSPattern, DDSPatternBuilder, spcm_win64.dll, spcm_core, SPC_DDS_* registers, DDS_Analog_Trg, core-to-channel routing, spcmdrv_debug.txt, driver debug log level/path, or the spectrum_dds_chapters manual — and also when a symptom points at it without naming it: RF not coming out of an AOM, a MOT script's frequencies not changing, "card is still running", "value not allowed", "the setup isn't valid", queue underrun/overrun, a pattern running one step out of sequence, missed shots or the software hanging during a run, or the CaF configuration failing to build. The card contradicts its own manual in nine documented places, so reason from this skill rather than from the PDF.
---

# Spectrum DDS (M4i.9622) — MoleculeMOT

Four RF outputs driving the cafmot AOMs. One external trigger per shot starts a
pattern; the card's internal timer steps through the rest of it.

**The manual is wrong in nine places.** Every fact below was measured on the card
in this rack. When the PDF in `spectrum_dds_chapters/` disagrees with this file,
the PDF is wrong — it is also a mangled PDF-to-markdown conversion, so its tables
and register numbers are actively unsafe.

## Safety — read before touching anything

**Never run a MOTMaster script and never fire the experiment.** Running a pattern
at the wrong moment risks human harm and equipment damage. Anything that drives
the NI pattern generators, fires a sequence, or triggers the experiment is done by
the user, at a time of their choosing.

Talking to the DDS card alone is safe and needs no permission: open it, read
registers, set routing, queue patterns, force triggers. Keep per-core amplitude
within the driver's clamp, and use `M2CMD_CARD_FORCETRIGGER` instead of the real
digital pulse — the line that triggers the DDS also starts the AO boards, so
driving it for real belongs to the user.

Enabling an output stage while all core amplitudes are zero emits nothing and is
fine. Enabling one with amplitude set sends RF to an AOM — check with the user
first unless they have just asked for exactly that.

## Orientation

| Where | What |
|---|---|
| `SpectrumDDS/` | Driver class library (net461). `SpcmCard` P/Invoke, `SpcmRegs`, `DDSPattern`, `DDSPatternCompiler`, `SpectrumDDSDriver`. |
| `SpectrumDDSController/` | Standalone WinForms app. Owns the card handle, publishes remoting on TCP 1818. |
| `MOTMaster/` | Integration, entirely behind `#if DDS`. |
| `dds_python/` | Python bench scripts |
| `spectrum_dds_chapters/` | The manual. Chapter 17 = DDS50 (this card), 11 = triggers, 12 = multi-purpose IO. |

Read `references/codebase.md` before changing any C# or MOTMaster wiring — it has
the integration points, the script-facing data contract, the `#if DDS` rule and the
build layout. Read `references/bench.md` before driving the card from Python or
PowerShell — the environment has traps that waste a lot of time.

## The card

M4i.9622-x8, serial 22805, `SPC_PCITYP = 0x79622`. `SPC_AVAILCARDMODES =
0x4000000` — **DDS is the only mode**, so there is no AWG to fall back on and no
firmware-switching risk. 50 cores, 1.25 GS/s, `SPCM_FEAT_EXTFW_DDS50`.

| Limit | Value |
|---|---|
| Frequency | 0 – 625 MHz, step 0.291038 Hz |
| Amplitude | −1 … +1, step 3.05185e−5 |
| Frequency slope | ±1.90735e14 Hz/s |
| Amplitude slope | ±305175 /s |
| Command queue | 4098 (SINGLE transfer mode) |
| Trigger timer | 83.2 ns … 27.487790730 s, 6.4 ns steps |

Read these from `SPC_DDS_AVAIL_*` rather than hard-coding them; `DDSCapabilities`
already does, and `DDSPattern.Validate` checks against them.

## Core → channel routing

Forced by hardware. Channel 0 accepts only 47 or 50 cores; channels 1/2/3 accept
exactly core 47/48/49 or nothing. The only legal four-output configuration:

```
SPC_DDS_CORES_ON_CH0 = 0x7FFFFFFFFFFF   // cores 0..46
SPC_DDS_CORES_ON_CH1 = 1 << 47
SPC_DDS_CORES_ON_CH2 = 1 << 48
SPC_DDS_CORES_ON_CH3 = 1 << 49
```

| Script channel | Core | Output |
|---|---|---|
| `...DDS1` | 0 | Ch0 |
| `...DDS2` | 47 | Ch1 |
| `...DDS3` | 48 | Ch2 |
| `...DDS4` | 49 | Ch3 |

**Cores 1–46 are summed onto Ch0 as well**, so anything left in them from a
previous run leaks into that output. Silence every core, not just the four in use.

This mapping is non-obvious and belongs in exactly one place —
`SpectrumDDSDriver.CoreForChannel` / `FourChannelRouting`. If you find it
open-coded anywhere else, that is a bug.

## Timing model — confirmed on the card

The question the whole compiler rests on: when the engine waits for trigger k+1,
whose `SPC_DDS_TRG_TIMER` is in force?

**The live one.** Parameters in a command block go live at the trigger that *ends*
the block, and the engine then waits using those now-live registers. So **block k
carries the trigger source and timer governing the wait for event k+1**, not its
own arrival time. Measured: a prologue timer of 0.6 s followed by blocks carrying
0.2 / 0.4 / 0.8 s gave intervals of 0.594 / 0.200 / 0.400 / 0.800 s. The
alternative model predicts 0.2 / 0.4 / 0.8 and is ruled out by 400 ms.

```
prologue:  TRG_SRC = CARD; all cores silenced
           EXEC_AT_TRG; WRITE_TO_CARD; FORCETRIGGER

block k:   core params for event k; XIO mask
           k < N-1:  TRG_SRC = TIMER, TRG_TIMER = t[k+1] - t[k]
           k = N-1:  TRG_SRC = CARD          // re-arm for the next shot
           EXEC_AT_TRG

finally:   WRITE_TO_CARD
```

The prologue's `FORCETRIGGER` is not decoration. The card always holds one block in
its shadow registers, so if the prologue is still sitting there when the pattern is
queued, the external trigger at t = 0 executes the prologue instead of event 0 and
**the whole pattern slips by one step**. Forcing consumes it, correctly leaving the
card reading "idle" until the first pattern is flushed.

Budget: 18 commands per block, plus a timer in all but the last. A 6-event pattern
is 113 commands and reads back as 94 queued — the difference is the block in the
shadow registers, which is a useful integrity check.

## The nine errata

**1. Out-of-range writes are rejected, not clamped.** Writing 30 s to
`SPC_DDS_TRG_TIMER` returns error 257 "value not allowed" and the register keeps
its old value. Code that ignores return codes runs happily on stale settings with
no sign anything went wrong — so check every call. `SpcmCard` throws on any
non-zero return, which is why it is the only thing that talks to the DLL.

**2. The manual's DDS XIO register numbers are mangled.** Real values from
`spcm_core.regs`: `SPC_DDS_X0/X1/X2_MODE = 608060/608061/608062`,
`SPC_DDS_X_MANUAL_OUTPUT = 608010`. The manual's 608011–608013 are actually
`NUM_QUEUED_CMD_IN_SW`, `DATA_TRANSFER_MODE` and `TRG_COUNT` — writing them would
silently corrupt unrelated state. Take register numbers from `regs.h` or
`SpcmRegs.cs`, never from the PDF.

**3. Illegal routing masks are silently snapped, not rejected.** Writing
`CORES_ON_CH0 = 1` returns success and reads back `0x7FFFFFFFFFFF`. Read routing
back and compare after every write.

**4. The first command block after `M2CMD_CARD_START` must be `EXEC_AT_TRG` +
`WRITE_TO_CARD`.** Until one has been flushed, `EXEC_NOW` fails with error 267
"the setup isn't valid" — so the manual's own "simple example for fixed frequency
output" cannot work as the first thing you do. The driver primes on open, and
manual tone control depends on that having happened.

**5. `SPC_TRIG_ORMASK` defaults to `SPC_TMASK_NONE`, not `SPC_TMASK_SOFTWARE`.**
Chapter 11 says otherwise. Without configuring it, `SPCM_DDS_TRG_SRC_CARD` has no
trigger source at all and the engine simply never advances. `SPC_TMASK_SOFTWARE` is
worse than useless here: it fires the instant the card starts and burns through the
queued blocks into `QUEUE_UNDERRUN`. Production setting:

```
SPC_TRIG_EXT0_MODE   = SPC_TM_POS      // DDS_Analog_Trg lands on Trg0
SPC_TRIG_EXT0_LEVEL0 = 1500            // mV, safe for 3.3 V or 5 V TTL
SPC_TRIG_TERM        = 0               // hi-Z; 50 ohm not yet decided
SPC_TRIG_ORMASK      = SPC_TMASK_EXT0
SPC_TRIG_ANDMASK     = SPC_TMASK_NONE
```

**6. `M2CMD_CARD_FORCETRIGGER` works repeatedly**, advancing the engine exactly one
step per call, whatever the OR mask holds. That makes it a faithful stand-in for the
digital pulse, so the whole arm/trigger/step/re-arm cycle can be exercised without
the NI boards.

**7. The status and counter registers all need care.** This trio has caused more
wrong conclusions than anything else on the card:

- `SPC_DDS_QUEUE_CMD_COUNT` **excludes the block held in the shadow registers**, so
  it reaches zero one event *before* a pattern ends. Never use it as "finished".
- `SPCM_DDS_STAT_WAITING_FOR_TRG` tracks **the queue, not the trigger engine**. With
  an empty queue it reads idle even though the trigger logic is armed and listening,
  and it **glitches low for an instant at every trigger boundary**. It is meaningful
  only just after a `WRITE_TO_CARD` — which is exactly where `WaitUntilArmed` uses
  it — and useless as an end-of-shot test.
- `SPC_DDS_TRG_COUNT` reads **one lower than the number of triggers processed** after
  a reset and never catches up. Compare differences, never absolute values.

The only reliable end-of-shot test is `TRG_COUNT` advancing by the number of events,
measured from a baseline taken while the card is quiescent — take it at arming time,
because sampling it after the trigger races the counter's lag and loses a count
roughly half the time.

**8. `M2CMD_CARD_WRITESETUP` is rejected once the card is started**, with error 288
"card is still running, access not possible". Output level and enable *can* be
changed on a running card — the register writes succeed and read back — it is only
the `WRITESETUP` command that must be skipped. So anything touching the output stage
after open writes the register and stops there. This also means re-running the setup
(calling `Open()` twice) will hit it.

**9. The per-event XIO marker cannot reach a pin on this card.** Chapter 17's recipe
is `SPCM_X0_MODE = SPCM_XMODE_DDS` then `SPC_DDS_X0_MODE = SPCM_DDS_XMODE_MANUAL`.
The DDS-side half is accepted and reads back; the card-level half is rejected with
error 257, because `SPCM_XMODE_DDS` (0x4, also called `SPCM_XMODE_DIGIN`) is absent
from `SPCM_X0_AVAILMODES = 0xF70732B`. The available modes are `ASYNCOUT`,
`TRIGOUT`, `RUNSTATE`, `ARMSTATE`, `CONTOUTMARK`, `REFCLKOUT`, `SYSCLKOUT` — none
per-event. A pattern's XIO column is still edited, saved and stored with the run; it
just drives nothing. `SpectrumDDSDriver.XioMarkersRouted` records this and the GUI
status tab says so, so it fails loudly rather than silently.

## Debug logging

The driver's own debug log (`spcmdrv_debug.txt`, historically `E:\SpectrumLog\`)
is not something the driver exposes cleanly through the SDK — but it is not a
black box either. Two undocumented-but-discoverable facts, both confirmed live
on this card without touching MOTMaster or firing anything:

- **Level, path and append-mode are an ordinary registry key**, the same one
  Spectrum Control Center's Debugging tab edits:
  `HKCU\SOFTWARE\Spectrum GmbH\spcm-driver\Debug`, values `LogLevel` (DWORD),
  `LogPath` (string, a directory) and `LogAppend` (DWORD). There is no
  `FileName` value — the driver always writes a fixed `spcmdrv_debug.txt` into
  `LogPath`. `SpectrumDDS/DebugLogSettings.cs` reads and writes this directly,
  and `SpectrumDDSController` has its own "Debug log level / path" panel on the
  Status tab — Control Center is no longer needed for this.
- **Registry changes are not picked up by an already-open connection.**
  Confirmed on the bench: changing `LogLevel` in the registry while a card was
  open and logging kept the old verbosity going indefinitely, with no sign of
  it being re-read. A level or path change only takes effect the next time the
  card is opened — plan any UI or script around that, not around it applying
  live.

**Custom lines can be written into the log from code**, via
`spcm_dwSetParam_ptr(NULL, SPC_WRITE_TO_LOG, text, length)` — `SPC_WRITE_TO_LOG
= 121`, confirmed against the installed `spcm_core.regs`, not the PDF (chapter
7 names the feature but not the register number). It is driver-global, not
tied to a device handle — the manual's own example and this driver's
`SpcmCard.WriteLogLine` both call it with a NULL handle — so it works whether
or not a card is open, which also makes it a safe way to bench-test logging
behaviour without opening the device at all.

**Level 3 ("log all, including library calls") logs every single register
read**, including ones inside a tight polling loop. `SpectrumDDSDriver.WaitUntilArmed`
is a busy-spin (`Thread.Sleep(0)`) on `SPC_DDS_STATUS`, called once per shot
from MOTMaster — at level 3 every poll in that spin is a log write before the
call returns. This is a real, demonstrated candidate for missed shots and
hangs during a run, not just a performance nit: found by tracing a live
missed-frames investigation back to this loop, in a deployment that had been
running at level 3 continuously. There is no way to keep per-call tracing on
selectively for just the parts you care about — level is all-or-nothing — so
the fix is to run at a low level for normal operation and only raise it while
actively chasing something, then reopen the card for the change to apply.

Because the driver's filename is fixed, "per-day files" don't come from the
registry either — `DebugLogSettings.RotateIfNewDay()` renames the active file
aside once it is no longer today's (`spcmdrv_log_{date}_level{n}.txt` — date
before level so alphabetical order in a file browser is also date order),
driven off the file's own last-write timestamp, not persisted state. Renaming
the active file while a connection is writing to it is safe and non-disruptive — proved
live: the driver creates a fresh file on its very next write, with no
interruption to whatever process is still open. That is also why rotation
does not need to coordinate with anything holding the card open.

## Diagnosing from a symptom

| Symptom | Look at |
|---|---|
| Engine never advances, status stays idle | Erratum 5 — trigger engine unconfigured. |
| Queue drains as fast as it is filled | A live `TRG_SRC = TIMER` left from a previous session. Reset the DDS engine on open. |
| `QUEUE_UNDERRUN` right after start | Erratum 5 — software trigger in the OR mask. |
| Pattern runs one step out of sequence | The prologue was not consumed before queueing. See the timing model. |
| Error 267 "the setup isn't valid" | Erratum 4 — not primed. |
| Error 288 "card is still running" | Erratum 8 — a `WRITESETUP` after start. |
| Error 257 "value not allowed" | Erratum 1 — out of range, or erratum 9 if it is an X-line mode. |
| Shot "ends" one event early | Erratum 7 — `QUEUE_CMD_COUNT` used as a completion test. |
| RF leaking on Ch0 | Cores 1–46 not silenced. |
| Nothing on an AOM despite a correct pattern | Output stage disabled, or amplitude above the clamp so the pattern was rejected. |
| Missed shots / hangs during a run | Check the debug log level (Status tab). Level 3 logs every poll inside `WaitUntilArmed`'s per-shot busy-spin — see Debug logging above. |

## Working style

Prefer read-back over faith. The card snaps some values, rejects others, and reports
several of its own counters in ways that do not mean what they appear to. A read-back
assertion costs nothing and has caught most of the errata above.
