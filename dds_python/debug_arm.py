"""Walk the exact prologue-then-pattern sequence, printing state at each stage.

debug_force.py established that M2CMD_CARD_FORCETRIGGER *does* step the DDS
engine, repeatedly, with SPC_DDS_TRG_COUNT trailing by one. So the "card never
armed" stall is not a trigger-source problem. The remaining suspect is that
prime(force=True) queues exactly one EXEC_AT_TRG block and then immediately
consumes it, leaving nothing queued -- SPCM_DDS_STAT_WAITING_FOR_TRG appears to
report "a block is queued and waiting", not "the trigger engine is listening".

This prints trg_count / DDS status / queue fill after every stage so the exact
transition that loses the armed state is visible. Outputs stay disabled.

    poetry run python debug_arm.py
"""

from spcm_core.regs import (
    SPC_TRIG_ORMASK,
    SPC_TRIG_ANDMASK,
    SPC_TRIG_EXT0_MODE,
    SPC_TRIG_EXT0_LEVEL0,
    SPC_TMASK_NONE,
    SPC_TMASK_EXT0,
    SPC_TM_POS,
    SPC_DDS_TRG_SRC,
    SPCM_DDS_TRG_SRC_CARD,
)

from spectrum_dds import DDSCard, CORE_FOR_CHANNEL
from pattern import compile_to_card


def report(card, stage):
    queued, qmax = card.queue_fill()
    print("  %-34s trg=%-4d queue=%-4d status=%s"
          % (stage, card.trg_count, queued, card.status_text()))


def main():
    from run_pattern import demo_pattern
    pattern = demo_pattern()

    with DDSCard() as card:
        card.configure_dds(enable_outputs=False)

        # Production trigger engine: rising edge on Trg0, ~1.5 V, no software
        # trigger. FORCETRIGGER still works with this, so bench tests stay valid.
        card.set_i(SPC_TRIG_EXT0_MODE, SPC_TM_POS)
        card.set_i(SPC_TRIG_EXT0_LEVEL0, 1500)
        card.set_i(SPC_TRIG_ORMASK, SPC_TMASK_EXT0)
        card.set_i(SPC_TRIG_ANDMASK, SPC_TMASK_NONE)

        card.reset_dds()
        card.route_four_channels()
        card.start()
        report(card, "after START")

        card.silence_all_cores()
        for ch, core in enumerate(CORE_FOR_CHANNEL):
            card.set_core(core, freq=pattern.events[0].channels[ch].freq, amp=0.0)
        card.set_i(SPC_DDS_TRG_SRC, SPCM_DDS_TRG_SRC_CARD)
        card.exec_at_trg()
        report(card, "prologue queued (not flushed)")

        card.write_to_card()
        report(card, "prologue flushed")

        card.force_trigger()
        report(card, "prologue forced live")

        print("\n-- queue the pattern --")
        compile_to_card(card, pattern)
        report(card, "pattern queued (not flushed)")

        card.write_to_card()
        report(card, "pattern flushed")

        print("\n-- step it by hand, one force per event --")
        for k in range(len(pattern) + 1):
            card.force_trigger()
            report(card, "force %d" % (k + 1))

        card.stop()
        print("\nfinal status: %s" % card.status_text())


if __name__ == "__main__":
    main()
