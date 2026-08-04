"""Stage 1 steps 1, 2 and 6: identity, capabilities, routing, timer clamp.

Read-only apart from card-mode/routing/timer register writes. Outputs stay
disabled, so nothing reaches the AOMs.

    poetry run python dds_python/probe.py
"""

from spectrum_dds import (
    DDSCard,
    ROUTING_FOUR_CHANNEL,
    CORE_FOR_CHANNEL,
    SpcmError,
)
from spcm_core.regs import (
    SPCM_FEAT_EXTFW_DDS20,
    SPCM_FEAT_EXTFW_DDS50,
    SPCM_FEAT_EXTFW_AWG,
    SPCM_FEAT_EXTFW_PULSEGEN,
    SPC_REP_STD_DDS,
    SPC_DDS_CORES_ON_CH0,
    SPC_DDS_CORES_ON_CH1,
    SPC_DDS_CORES_ON_CH2,
    SPC_DDS_CORES_ON_CH3,
)


def section(title):
    print("\n== %s %s" % (title, "=" * max(0, 60 - len(title))))


def main():
    with DDSCard() as card:
        section("identity")
        ident = card.identity()
        print("  card type       : 0x%X" % ident["pcityp"])
        print("  serial          : %d" % ident["serial"])
        print("  channels        : %d" % ident["channels"])
        print("  sample rate     : %d S/s" % ident["samplerate"])
        print("  avail cardmodes : 0x%X (DDS=0x%X -> %s)"
              % (ident["availcardmodes"], SPC_REP_STD_DDS,
                 bool(ident["availcardmodes"] & SPC_REP_STD_DDS)))
        feat = ident["extfeatures"]
        print("  ext features    : 0x%X" % feat)
        for name, bit in [("DDS20", SPCM_FEAT_EXTFW_DDS20),
                          ("DDS50", SPCM_FEAT_EXTFW_DDS50),
                          ("AWG", SPCM_FEAT_EXTFW_AWG),
                          ("PULSEGEN", SPCM_FEAT_EXTFW_PULSEGEN)]:
            print("      %-9s %s" % (name, "yes" if feat & bit else "no"))

        card.configure_dds(enable_outputs=False)

        section("capabilities")
        for k, v in card.capabilities().items():
            print("  %-16s %s" % (k, v))

        section("routing")
        regs = [SPC_DDS_CORES_ON_CH0, SPC_DDS_CORES_ON_CH1,
                SPC_DDS_CORES_ON_CH2, SPC_DDS_CORES_ON_CH3]
        print("  default         : %s" % [hex(card.get_i(r)) for r in regs])
        try:
            got = card.route_four_channels()
            print("  four-channel    : %s  OK" % [hex(m) for m in got])
        except SpcmError as e:
            print("  four-channel    : FAILED -- %s" % e)
            return
        for ch, core in enumerate(CORE_FOR_CHANNEL):
            print("      DDS%d -> core %-2d -> Ch%d" % (ch + 1, core, ch))

        section("routing is snapped, not rejected")
        # Ask for a single core on Ch0, which is not a legal configuration.
        card.set_i(SPC_DDS_CORES_ON_CH0, 1)
        snapped = card.get_i(SPC_DDS_CORES_ON_CH0)
        print("  asked Ch0 = 0x1, card gave 0x%X -> %s"
              % (snapped, "snapped (as expected)" if snapped != 1 else "accepted?!"))
        card.route_four_channels()

        section("trigger timer quantisation and range")
        # Range is 83.2 ns .. 27.4878 s in 6.4 ns steps. Out-of-range values are
        # REJECTED (err 257), not clamped, so a caller that ignores return codes
        # keeps running with whatever was set before.
        for req in [1e-8, 1e-7, 1e-6, 1e-5, 1e-4, 1e-3, 0.01, 0.1, 1.0, 2.0, 27.0, 30.0]:
            try:
                print("  request %-10g -> %.9g s" % (req, card.set_trg_timer(req)))
            except SpcmError as e:
                print("  request %-10g -> REJECTED (%s)" % (req, str(e).split(": ")[-1]))

        section("queue")
        count, cmax = card.queue_fill()
        print("  commands queued : %d / %d" % (count, cmax))
        print("  status          : %s" % card.status_text())


if __name__ == "__main__":
    main()
