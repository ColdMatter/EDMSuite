"""Stage 1 step 4/5: settle the DDS command-block timing semantics.

The question the C# pattern compiler depends on:

  When the DDS engine is waiting for trigger k+1, whose SPC_DDS_TRG_TIMER is in
  force -- the one written in block k (which went live at trigger k), or the one
  written in block k+1 (still sitting in the shadow registers)?

  hypothesis A: the *live* timer governs, i.e. block k carries the interval to
                event k+1. The compiler must write the gap t[k+1]-t[k] into
                block k.
  hypothesis B: the *shadow* timer governs, i.e. block k carries its own arrival
                time. The compiler shifts by one step.

The test writes a prologue with a distinctive timer and then blocks with
different timers, and times the trigger events. Under A the first interval is
the prologue's; under B it is block 0's.

Outputs stay disabled throughout -- only trigger timing is measured, so nothing
reaches the AOMs.

    poetry run python dds_python/timing_model.py
"""

from spcm_core.regs import (
    SPC_DDS_TRG_SRC,
    SPCM_DDS_TRG_SRC_TIMER,
    SPCM_DDS_TRG_SRC_CARD,
)

from spectrum_dds import DDSCard, CORE_FOR_CHANNEL

PROLOGUE_TIMER = 0.6
BLOCK_TIMERS = [0.2, 0.4, 0.8]


def main():
    with DDSCard() as card:
        card.configure_dds(enable_outputs=False)
        card.route_four_channels()
        card.reset_dds()
        card.route_four_channels()
        card.start()

        # -- prologue: establish a live trigger source and a distinctive timer.
        #    This doubles as the priming block the card demands before it will
        #    accept any EXEC_NOW, so it must be EXEC_AT_TRG + FORCETRIGGER.
        card.set_i(SPC_DDS_TRG_SRC, SPCM_DDS_TRG_SRC_TIMER)
        card.set_trg_timer(PROLOGUE_TIMER)
        card.silence_all_cores()
        for ch, core in enumerate(CORE_FOR_CHANNEL):
            card.set_core(core, freq=(80 + 10 * ch) * 1e6, amp=0.0)
        card.prime(force=True)

        # -- one block per step, each carrying its own timer value.
        for timer in BLOCK_TIMERS:
            card.set_trg_timer(timer)
            card.exec_at_trg()

        # -- hand back to the external trigger so the card idles armed.
        card.set_i(SPC_DDS_TRG_SRC, SPCM_DDS_TRG_SRC_CARD)
        card.exec_at_trg()

        queued, qmax = card.queue_fill()
        print("queued before write : %d / %d" % (queued, qmax))
        card.write_to_card()

        n = len(BLOCK_TIMERS) + 1
        events = card.watch_triggers(n, timeout=15.0)
        card.stop()

        print("\ntrigger events (seconds after WRITE_TO_CARD):")
        prev = 0.0
        intervals = []
        for idx, t in events:
            intervals.append(t - prev)
            print("  trigger %d at %7.4f s   interval %7.4f s" % (idx, t, t - prev))
            prev = t

        if not intervals:
            print("\nno triggers seen -- check status: %s" % card.status_text())
            return

        expect_a = [PROLOGUE_TIMER] + BLOCK_TIMERS
        expect_b = BLOCK_TIMERS + [BLOCK_TIMERS[-1]]

        def err(expected):
            return max(abs(m - e) for m, e in zip(intervals, expected[:len(intervals)]))

        err_a, err_b = err(expect_a), err(expect_b)
        print("\n  expected under A (live timer governs)   : %s  max error %.4f s"
              % (expect_a[:len(intervals)], err_a))
        print("  expected under B (shadow timer governs) : %s  max error %.4f s"
              % (expect_b[:len(intervals)], err_b))

        tol = 0.03
        if err_a < tol and err_b >= tol:
            print("\n=> HYPOTHESIS A holds: block k carries the interval to event k+1.")
        elif err_b < tol and err_a >= tol:
            print("\n=> HYPOTHESIS B holds: block k carries its own arrival time.")
        else:
            print("\n=> INCONCLUSIVE -- neither model fits within %.0f ms." % (tol * 1e3))

        print("\nstatus after run: %s" % card.status_text())


if __name__ == "__main__":
    main()
