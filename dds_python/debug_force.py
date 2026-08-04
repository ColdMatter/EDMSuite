"""Can M2CMD_CARD_FORCETRIGGER step the DDS engine more than once?

debug_cardtrg2.py found that with SPC_DDS_TRG_SRC = SPCM_DDS_TRG_SRC_CARD the
engine arms (WAITING_FOR_TRG) but a single FORCETRIGGER did not advance
SPC_DDS_TRG_COUNT. Chapter 11 says FORCETRIGGER "forces a trigger event if the
hardware is still waiting for a trigger event", which suggests the card-level
trigger engine latches once and then ignores further forces -- in which case
FORCETRIGGER is not a usable stand-in for the external pulse and bench tests
must use SPCM_DDS_TRG_SRC_TIMER for step 0 instead.

This queues a long run of EXEC_AT_TRG blocks and then hammers FORCETRIGGER,
reporting trg_count after each, under a few trigger-engine configurations.
Outputs stay disabled.

    poetry run python debug_force.py
"""

import time

from spcm_core.regs import (
    SPC_TRIG_ORMASK,
    SPC_TRIG_ANDMASK,
    SPC_TRIG_EXT0_MODE,
    SPC_TRIG_EXT0_LEVEL0,
    SPC_TMASK_NONE,
    SPC_TMASK_EXT0,
    SPC_TM_POS,
    SPC_M2STATUS,
    M2CMD_CARD_ENABLETRIGGER,
    SPC_M2CMD,
    SPC_DDS_TRG_SRC,
    SPCM_DDS_TRG_SRC_CARD,
)

from spectrum_dds import DDSCard, CORE_FOR_CHANNEL

N_BLOCKS = 8


def setup(card, ormask, ext0=False):
    card.stop()
    card.reset_dds()
    if ext0:
        card.set_i(SPC_TRIG_EXT0_MODE, SPC_TM_POS)
        card.set_i(SPC_TRIG_EXT0_LEVEL0, 1500)
    card.set_i(SPC_TRIG_ORMASK, ormask)
    card.set_i(SPC_TRIG_ANDMASK, SPC_TMASK_NONE)
    card.route_four_channels()
    card.start()

    card.silence_all_cores()
    for ch, core in enumerate(CORE_FOR_CHANNEL):
        card.set_core(core, freq=(80 + 10 * ch) * 1e6, amp=0.0)
    card.set_i(SPC_DDS_TRG_SRC, SPCM_DDS_TRG_SRC_CARD)
    card.exec_at_trg()
    for _ in range(N_BLOCKS):
        card.set_i(SPC_DDS_TRG_SRC, SPCM_DDS_TRG_SRC_CARD)
        card.exec_at_trg()
    card.write_to_card()


def hammer(card, label, reenable=False):
    print("\n=== %s ===" % label)
    print("  start: trg_count=%d status=%s m2=0x%X"
          % (card.trg_count, card.status_text(), card.get_i32(SPC_M2STATUS)))
    for i in range(1, 6):
        if reenable:
            card.set_i(SPC_M2CMD, M2CMD_CARD_ENABLETRIGGER)
        card.force_trigger()
        time.sleep(0.05)
        print("  force %d: trg_count=%d status=%-16s m2=0x%X"
              % (i, card.trg_count, card.status_text(), card.get_i32(SPC_M2STATUS)))


def main():
    with DDSCard() as card:
        card.configure_dds(enable_outputs=False)

        setup(card, SPC_TMASK_EXT0, ext0=True)
        hammer(card, "ORMASK=EXT0, plain FORCETRIGGER")

        setup(card, SPC_TMASK_EXT0, ext0=True)
        hammer(card, "ORMASK=EXT0, ENABLETRIGGER before each force", reenable=True)

        setup(card, SPC_TMASK_NONE)
        hammer(card, "ORMASK=NONE, plain FORCETRIGGER")

        card.stop()
        print("\nfinal status: %s" % card.status_text())


if __name__ == "__main__":
    main()
