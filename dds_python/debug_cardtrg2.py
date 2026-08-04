"""Follow-up to debug_cardtrg.py: the card trigger engine is simply not set up.

debug_cardtrg.py showed SPC_TRIG_ORMASK defaults to 0 (SPC_TMASK_NONE) on this
card -- not SPC_TMASK_SOFTWARE as chapter 11 implies -- so with
SPC_DDS_TRG_SRC = SPCM_DDS_TRG_SRC_CARD there is no trigger source at all: the
engine never arms, and even M2CMD_CARD_FORCETRIGGER produces nothing.

This tries the three plausible configurations and reports, for each, whether
WAITING_FOR_TRG sets, whether the card free-runs, and whether FORCETRIGGER
steps the engine. Outputs stay disabled.

    poetry run python debug_cardtrg2.py
"""

import time

from spcm_core.regs import (
    SPC_TRIG_ORMASK,
    SPC_TRIG_ANDMASK,
    SPC_TRIG_EXT0_MODE,
    SPC_TRIG_EXT0_LEVEL0,
    SPC_TRIG_TERM,
    SPC_TMASK_NONE,
    SPC_TMASK_SOFTWARE,
    SPC_TMASK_EXT0,
    SPC_TM_POS,
    SPC_M2STATUS,
    M2STAT_CARD_READY,
    M2STAT_CARD_TRIGGER,
    M2STAT_CARD_PRETRIGGER,
    SPC_DDS_TRG_SRC,
    SPCM_DDS_TRG_SRC_CARD,
    SPCM_DDS_STAT_WAITING_FOR_TRG,
)

from spectrum_dds import DDSCard, CORE_FOR_CHANNEL


def m2status_text(v):
    flags = []
    if v & M2STAT_CARD_PRETRIGGER:
        flags.append("PRETRIGGER")
    if v & M2STAT_CARD_TRIGGER:
        flags.append("TRIGGER")
    if v & M2STAT_CARD_READY:
        flags.append("READY")
    return "0x%X %s" % (v, "|".join(flags) if flags else "-")


def try_config(card, label, ormask, ext0_mode=None, term=None):
    print("\n=== %s ===" % label)
    card.stop()
    card.reset_dds()

    if ext0_mode is not None:
        card.set_i(SPC_TRIG_EXT0_MODE, ext0_mode)
        card.set_i(SPC_TRIG_EXT0_LEVEL0, 1500)
    if term is not None:
        card.set_i(SPC_TRIG_TERM, term)
    card.set_i(SPC_TRIG_ORMASK, ormask)
    card.set_i(SPC_TRIG_ANDMASK, SPC_TMASK_NONE)

    card.route_four_channels()
    card.start()

    card.silence_all_cores()
    for ch, core in enumerate(CORE_FOR_CHANNEL):
        card.set_core(core, freq=(80 + 10 * ch) * 1e6, amp=0.0)
    card.set_i(SPC_DDS_TRG_SRC, SPCM_DDS_TRG_SRC_CARD)
    card.exec_at_trg()
    card.write_to_card()

    # Queue a couple more EXEC_AT_TRG blocks so there is something for a
    # trigger to consume, then watch.
    for _ in range(3):
        card.set_i(SPC_DDS_TRG_SRC, SPCM_DDS_TRG_SRC_CARD)
        card.exec_at_trg()
    card.write_to_card()

    t0 = time.perf_counter()
    first = card.trg_count
    armed = False
    while time.perf_counter() - t0 < 1.0:
        if card.status & SPCM_DDS_STAT_WAITING_FOR_TRG:
            armed = True
    free_run = card.trg_count - first

    print("  m2status      : %s" % m2status_text(card.get_i32(SPC_M2STATUS)))
    print("  armed flag    : %s" % armed)
    print("  free-running  : %+d triggers in 1.0 s" % free_run)

    before = card.trg_count
    card.force_trigger()
    time.sleep(0.1)
    after = card.trg_count
    print("  FORCETRIGGER  : trg_count %d -> %d (%+d)" % (before, after, after - before))
    print("  dds status    : %s" % card.status_text())
    return armed, free_run, after - before


def main():
    with DDSCard() as card:
        card.configure_dds(enable_outputs=False)

        try_config(card, "ORMASK = NONE (current behaviour)", SPC_TMASK_NONE)
        try_config(card, "ORMASK = SOFTWARE", SPC_TMASK_SOFTWARE)
        try_config(card, "ORMASK = EXT0, pos edge, 1.5 V, hi-Z",
                   SPC_TMASK_EXT0, ext0_mode=SPC_TM_POS, term=0)

        card.stop()
        print("\nfinal status: %s" % card.status_text())


if __name__ == "__main__":
    main()
