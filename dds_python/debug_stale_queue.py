"""Why the DDS output lags MOTMaster, and whether the flush actually clears it.

Symptom on the scope: the card plays several old patterns before it gets to the
one the running script asked for. Nothing on the card replaces a queued shot --
a second pattern goes *behind* the first -- so every needless arm puts the output
one whole shot further behind, and the queue survives EXEC_NOW, a stopped shot
loop and a new pattern being loaded.

Two things to establish:

  1. queueing twice really does stack, rather than overwriting;
  2. the sequence SpectrumDDSDriver.DiscardQueuedShots uses -- STOP, trigger
     setup, WRITESETUP, RESET, routing, START, prime -- empties the queue on a
     card that has been running, and leaves it able to run a fresh pattern.

That flush stops the card, so it takes the RF down with it. It therefore runs
only when a new pattern is loaded or a manual tone applied; the end of a run
leaves the channels holding the last event's state, and the shot the loop had
armed sits queued until the next load clears it.

Outputs stay disabled throughout and every step is driven by FORCETRIGGER, so
this touches nothing but the Spectrum card.

    poetry run python debug_stale_queue.py
"""

import time

from spcm_core.regs import (
    SPC_DDS_TRG_SRC,
    SPCM_DDS_TRG_SRC_CARD,
    SPC_M2CMD,
    M2CMD_CARD_WRITESETUP,
)

from spectrum_dds import DDSCard, CORE_FOR_CHANNEL
from pattern import Pattern, ChannelState, Event, compile_to_card

STEP_S = 0.05


def small_pattern(f0_mhz):
    """Three events, amplitudes zero -- only the queue behaviour matters here."""
    def chans(offset):
        return [ChannelState(freq=(f0_mhz + offset + 10 * i) * 1e6, amp=0.0)
                for i in range(4)]
    return Pattern([Event("a", 0.000, chans(0)),
                    Event("b", STEP_S, chans(1)),
                    Event("c", 2 * STEP_S, chans(2))])


def report(card, stage):
    queued, _ = card.queue_fill()
    print("  %-30s queue=%-5d trg=%-4d status=%s"
          % (stage, queued, card.trg_count, card.status_text()))
    return queued


def discard_queued_shots(card):
    """What SpectrumDDSDriver.DiscardQueuedShots does, in the same order."""
    card.stop()
    card.configure_card_trigger()
    card.set_i(SPC_M2CMD, M2CMD_CARD_WRITESETUP)
    card.reset_dds()
    card.route_four_channels()
    card.start()
    card.silence_all_cores()
    card.set_i(SPC_DDS_TRG_SRC, SPCM_DDS_TRG_SRC_CARD)
    card.prime(force=True)


def main():
    pattern = small_pattern(80.0)
    steps = len(pattern)

    with DDSCard() as card:
        card.configure_dds(enable_outputs=False)
        card.reset_dds()
        card.route_four_channels()
        card.start()

        card.silence_all_cores()
        card.set_i(SPC_DDS_TRG_SRC, SPCM_DDS_TRG_SRC_CARD)
        card.prime(force=True)
        report(card, "primed")

        print("\n-- 1. does a second arm stack behind the first? --")
        compile_to_card(card, pattern)
        card.write_to_card()
        one = report(card, "armed once")

        compile_to_card(card, pattern)
        card.write_to_card()
        two = report(card, "armed again without firing")

        if two >= one + one:
            print("  STACKS: the second pattern is queued behind the first, not"
                  " instead of it.")
        else:
            print("  did NOT stack (%d then %d) -- re-check the diagnosis." % (one, two))

        print("\n-- 2. does the flush clear it? --")
        discard_queued_shots(card)
        after = report(card, "after DiscardQueuedShots")
        if after != 0:
            print("  FAILED: %d commands survived the flush." % after)
            card.stop()
            return

        print("\n-- 3. is the card still usable afterwards? --")
        baseline = card.trg_count
        compile_to_card(card, pattern)
        card.write_to_card()
        report(card, "fresh pattern armed")

        # One force starts it; the block timers step through the rest.
        card.force_trigger()
        time.sleep(steps * STEP_S + 0.2)
        advanced = card.trg_count - baseline
        report(card, "after one trigger")
        print("  triggers advanced by %d, expected %d: %s"
              % (advanced, steps, "OK" if advanced == steps else "WRONG"))

        card.silence_all_cores()
        card.exec_now()
        card.write_to_card()
        card.stop()
        print("\nfinal status: %s" % card.status_text())


if __name__ == "__main__":
    main()
