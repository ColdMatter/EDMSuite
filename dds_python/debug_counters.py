"""Nail down what SPC_DDS_QUEUE_CMD_COUNT and SPC_DDS_TRG_COUNT actually mean.

run_pattern.py showed both behaving oddly: TRG_COUNT appears to trail the number
of triggers by one, and QUEUE_CMD_COUNT read 0 at the end of a shot yet left 19
commands behind at the start of the next. The driver's "is the shot finished"
test depends on getting this right, so measure it directly.

Every block here keeps SPC_DDS_TRG_SRC = SPCM_DDS_TRG_SRC_CARD, so the engine
advances only on an explicit FORCETRIGGER and nothing is racing us. Outputs stay
disabled.

    poetry run python debug_counters.py
"""

import time

from spcm_core.regs import (
    SPC_DDS_TRG_SRC,
    SPCM_DDS_TRG_SRC_CARD,
)

from spectrum_dds import DDSCard, CORE_FOR_CHANNEL

N_BLOCKS = 4
CMDS_PER_BLOCK = 4 * 4 + 2   # four cores x four params, XIO, trigger source


def report(card, stage):
    queued, _ = card.queue_fill()
    print("  %-28s queue=%-5d trg=%-4d status=%s"
          % (stage, queued, card.trg_count, card.status_text()))


def main():
    with DDSCard() as card:
        card.configure_dds(enable_outputs=False)
        card.reset_dds()
        card.route_four_channels()
        card.start()
        report(card, "after START+RESET")

        # Prologue, unforced: leave it queued so we can see the queue count for
        # a known number of commands.
        card.silence_all_cores()
        card.set_i(SPC_DDS_TRG_SRC, SPCM_DDS_TRG_SRC_CARD)
        card.exec_at_trg()
        card.write_to_card()
        report(card, "prologue flushed")

        # Blocks of a known, identical size, all waiting on the card trigger.
        for b in range(N_BLOCKS):
            for ch, core in enumerate(CORE_FOR_CHANNEL):
                card.set_core(core, freq=(80 + 10 * ch + b) * 1e6, amp=0.0,
                              freq_slope=0.0, amp_slope=0.0)
            card.set_i(SPC_DDS_TRG_SRC, SPCM_DDS_TRG_SRC_CARD)
            card.exec_at_trg()
        card.write_to_card()
        report(card, "%d blocks flushed" % N_BLOCKS)
        print("     (each block should be ~%d commands)" % CMDS_PER_BLOCK)

        print("\n-- one force per step, 0.2 s apart --")
        for i in range(1, N_BLOCKS + 3):
            card.force_trigger()
            time.sleep(0.2)
            report(card, "after force %d" % i)

        card.stop()
        print("\nfinal status: %s" % card.status_text())


if __name__ == "__main__":
    main()
