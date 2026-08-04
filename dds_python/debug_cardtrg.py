"""Diagnose why the card never reports SPCM_DDS_STAT_WAITING_FOR_TRG.

run_pattern.py stalls at "card never armed": after handing the DDS trigger
source to SPCM_DDS_TRG_SRC_CARD the status register never sets
WAITING_FOR_TRG. Two candidate explanations:

  (a) SPC_TRIG_ORMASK defaults to SPC_TMASK_SOFTWARE, so the card free-runs
      triggers, the queue drains instantly and the engine never parks armed.
      Signature: SPC_DDS_TRG_COUNT climbs on its own.
  (b) WAITING_FOR_TRG is transient / cleared on read, and the engine really is
      armed. Signature: trg_count static, status reads back idle.

This only reads registers and queues silent (zero-amplitude) commands, so
nothing comes out of the SMAs.

    poetry run python debug_cardtrg.py
"""

import time

from spcm_core.regs import (
    SPC_TRIG_ORMASK,
    SPC_TRIG_ANDMASK,
    SPC_TRIG_CH_ORMASK0,
    SPC_TRIG_CH_ANDMASK0,
    SPC_TRIG_EXT0_MODE,
    SPC_TRIG_EXT0_LEVEL0,
    SPC_TRIG_TERM,
    SPC_TRIG_AVAILORMASK,
    SPC_TRIG_EXT_AVAILMODES,
    SPC_DDS_TRG_SRC,
    SPCM_DDS_TRG_SRC_CARD,
)

from spectrum_dds import DDSCard, CORE_FOR_CHANNEL


def dump_trigger_regs(card, label):
    print("\n-- trigger registers (%s) --" % label)
    for name, reg in [
        ("SPC_TRIG_ORMASK", SPC_TRIG_ORMASK),
        ("SPC_TRIG_ANDMASK", SPC_TRIG_ANDMASK),
        ("SPC_TRIG_CH_ORMASK0", SPC_TRIG_CH_ORMASK0),
        ("SPC_TRIG_CH_ANDMASK0", SPC_TRIG_CH_ANDMASK0),
        ("SPC_TRIG_EXT0_MODE", SPC_TRIG_EXT0_MODE),
        ("SPC_TRIG_EXT0_LEVEL0", SPC_TRIG_EXT0_LEVEL0),
        ("SPC_TRIG_TERM", SPC_TRIG_TERM),
        ("SPC_TRIG_AVAILORMASK", SPC_TRIG_AVAILORMASK),
        ("SPC_TRIG_EXT_AVAILMODES", SPC_TRIG_EXT_AVAILMODES),
    ]:
        try:
            print("  %-24s = 0x%X" % (name, card.get_i32(reg) & 0xFFFFFFFF))
        except Exception as exc:  # some registers are read-only/unsupported
            print("  %-24s : %s" % (name, exc))


def observe(card, seconds=2.0, label=""):
    """Watch trg_count and status for a while without touching anything."""
    t0 = time.perf_counter()
    first = card.trg_count
    seen_armed = False
    while time.perf_counter() - t0 < seconds:
        if card.status & 0x1:  # SPCM_DDS_STAT_WAITING_FOR_TRG
            seen_armed = True
    last = card.trg_count
    print("  %s trg_count %d -> %d (%+d in %.1f s), armed-flag seen: %s, status now: %s"
          % (label, first, last, last - first, seconds, seen_armed, card.status_text()))
    return last - first, seen_armed


def main():
    with DDSCard() as card:
        card.configure_dds(enable_outputs=False)
        card.route_four_channels()
        dump_trigger_regs(card, "defaults, before START")

        card.reset_dds()
        card.route_four_channels()
        card.start()

        # Prologue exactly as run_pattern.py does it: silence, hand the DDS
        # trigger to the card input, prime.
        card.silence_all_cores()
        for ch, core in enumerate(CORE_FOR_CHANNEL):
            card.set_core(core, freq=(80 + 10 * ch) * 1e6, amp=0.0)
        card.set_i(SPC_DDS_TRG_SRC, SPCM_DDS_TRG_SRC_CARD)
        card.prime(force=True)

        print("\n-- after prologue with TRG_SRC = CARD --")
        queued, qmax = card.queue_fill()
        print("  queue %d / %d" % (queued, qmax))
        delta, armed = observe(card, 2.0, "idle:")

        if delta > 0:
            print("\n=> (a) confirmed: the card is free-running triggers.")
        elif not armed:
            print("\n=> (b) likely: no triggers, but WAITING_FOR_TRG never sets.")
        else:
            print("\n=> engine parks armed as expected; the blocker is elsewhere.")

        # Does an explicit external-only trigger setup change the picture?
        card.stop()
        card.set_i(SPC_TRIG_ORMASK, 0)
        card.set_i(SPC_TRIG_ANDMASK, 0)
        card.set_i(SPC_TRIG_CH_ORMASK0, 0)
        card.set_i(SPC_TRIG_CH_ANDMASK0, 0)
        dump_trigger_regs(card, "ORMASK/ANDMASK cleared")

        card.start()
        card.silence_all_cores()
        card.set_i(SPC_DDS_TRG_SRC, SPCM_DDS_TRG_SRC_CARD)
        card.prime(force=True)
        print("\n-- after prologue with ORMASK = 0 --")
        observe(card, 2.0, "idle:")

        # And does FORCETRIGGER still step the engine in that configuration?
        before = card.trg_count
        card.force_trigger()
        time.sleep(0.2)
        print("  FORCETRIGGER: trg_count %d -> %d" % (before, card.trg_count))

        card.stop()
        print("\nfinal status: %s" % card.status_text())


if __name__ == "__main__":
    main()
