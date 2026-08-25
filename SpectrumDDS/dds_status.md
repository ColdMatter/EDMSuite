# DDS status register

The card reports its state through one register, `SPC_DDS_STATUS`, read back as a
bitmask (`DDSStatus` in [SpectrumDDSDriver.cs](SpectrumDDSDriver.cs)). It's a
`[Flags]` enum — more than one bit can be set at once.

```csharp
public enum DDSStatus
{
    Idle = 0,
    WaitingForTrigger = SpcmRegs.SPCM_DDS_STAT_WAITING_FOR_TRG,   // 0x1
    QueueUnderrun     = SpcmRegs.SPCM_DDS_STAT_QUEUE_UNDERRUN,    // 0x2
    QueueOverrun      = SpcmRegs.SPCM_DDS_STAT_QUEUE_OVERRUN,     // 0x4
}
```

## Idle (0)

No bits set. Nothing pending, nothing wrong.

## WaitingForTrigger (0x1)

The manual's definition: "DDS is waiting for a trigger event." In practice this is
less useful than it sounds — it tracks the **command queue**, not the trigger
engine itself. With an empty queue it reads idle even though the card is still
armed and listening on `DDS_Analog_Trg`, and it glitches low for an instant at
every trigger boundary.

It's only meaningful right after a `WRITE_TO_CARD` — which is exactly where
`WaitUntilArmed` checks it — and useless as an end-of-shot test.

## QueueUnderrun (0x2)

The manual's definition: a trigger or timer event arrived before the finalizing
`EXEC_AT_TRG` command was sent, so the engine had nothing valid to step to. Two
ways this actually shows up on this card:

- **Misconfigured trigger source.** `SPC_TRIG_ORMASK` defaults to
  `SPC_TMASK_NONE`, not software, so leaving it unconfigured means the engine
  never advances at all. Putting `SPC_TMASK_SOFTWARE` in the mask instead fires
  the instant the card starts and burns straight through the queue into
  underrun.
- **A real trigger landing in the re-arm deadtime.** After a pattern finishes,
  MOTMaster has to write the whole next pattern (~20 register writes per event)
  before the next trigger arrives. MOTMaster does try to avoid this —
  [MOTMaster/Controller.cs:489](../MOTMaster/Controller.cs#L489) calls
  `ddsController.WaitUntilArmed(1.0)` before firing — but the return value is
  discarded and `runPattern` is called unconditionally right after, by design:
  the comment above it says "if it is still not armed after a second, go
  anyway and let the tally at the end report the miss." So underrun from this
  path means arming took longer than that 1 s budget, not that MOTMaster
  skipped the check. See "Arming has to win the race" below for what eats that
  budget.

**Underrun is latched.** Once set it stays set until `SPCM_DDS_CMD_RESET`
(issued by `PrepareForNewPattern`), so a status of `QueueUnderrun` means "at
least one has happened since the last pattern load," not "happening right now."
Count *onsets* in the log, not raw reads.

## QueueOverrun (0x4)

You tried to queue a command while the queue (4098 deep) was already full. In
this driver this mostly shows up if pattern compilation or queueing logic goes
wrong, rather than as a normal operating condition — a correctly-sized pattern
shouldn't get near it.

## Arming has to win the race

Every shot has a deadtime: the pattern ends, the queue empties, and the whole
next pattern has to be written in before the next external trigger arrives —
about 20 register writes per event, so ~283 for the 15-event MOT pattern.
`WaitUntilArmed(1.0)` exists to stop MOTMaster firing into that window, but it's
a 1-second budget, not a guarantee — see `QueueUnderrun` above for how it gets
overrun and fires anyway.

What actually eats the budget:

- **Debug log level.** At level 3 ("log all"), every one of those register
  writes is also a formatted log line — ~1.1 ms each, measured — so an arm
  takes ~0.35 s instead of a few ms, and an I/O stall on the log file has
  turned single arms into 34, 54, and 93-second hangs on a real run. Keep
  logging at a low level for normal operation; only raise it while actively
  chasing something (Status tab → Debug log level).
- **Lock contention (historical, now fixed).** `WaitUntilArmed` used to hold
  `cardLock` while polling, which blocked `ArmForNextShot` — the very thing it
  was waiting for — guaranteeing its own timeout. Fixed by polling outside the
  lock. Worth knowing if this ever regresses: holding a lock across any wait is
  the general trap, since the lock exists to keep a multi-register sequence
  uninterrupted, and waiting is not one of those.

## Diagnosing from the README

The README's troubleshooting entry for this is intentionally terse — "Something
is wrong with the trigger setup; close and reopen the card" — because
closing/reopening resets the DDS engine and clears the latch, which resolves the
common case (a stale `TRG_SRC = TIMER` or a bad OR mask left over from a previous
session).

If it recurs, the cause is almost always the re-arm deadtime race, not a one-off
glitch. Check the `sent / fired / missed` tally printed at the end of a run —
`missed` counting up is the same underlying symptom as `QueueUnderrun`, just
reported at a higher level.

Two other counters look like status but aren't:

- `SPC_DDS_QUEUE_CMD_COUNT` excludes the block sitting in the shadow registers,
  so it reaches zero one event *before* a pattern actually ends.
- `SPC_DDS_TRG_COUNT` reads one lower than the number of triggers processed
  after a reset and never catches up. Compare differences, never absolute
  values.

See `.claude/skills/spectrum-dds/SKILL.md` (errata 5 and 7, and "Arming has to
win the race") for how these were traced on the bench.
