"""Stage 1 steps 3 and 5: compile a realistic pattern and run it repeatedly.

M2CMD_CARD_FORCETRIGGER stands in for the digital pulse on DDS_Analog_Trg, so
this exercises the whole arm / trigger / step / re-arm cycle without touching
the NI pattern generators or firing the experiment.

    poetry run python dds_python/run_pattern.py            # outputs off
    poetry run python dds_python/run_pattern.py --outputs  # RF on, for a scope

With --outputs the four SMA outputs carry the pattern, so only use it when the
AOM drivers are safe to drive. Core amplitudes are clamped to MAX_CORE_AMP.
"""

import argparse
import time

from spcm_core.regs import (
    SPC_DDS_TRG_SRC,
    SPCM_DDS_TRG_SRC_CARD,
    SPCM_DDS_STAT_WAITING_FOR_TRG,
    SPCM_DDS_STAT_QUEUE_UNDERRUN,
    SPCM_DDS_STAT_QUEUE_OVERRUN,
)

from spectrum_dds import DDSCard, CORE_FOR_CHANNEL
from pattern import Pattern, ChannelState, Event, compile_to_card


def demo_pattern():
    """A pattern shaped like the MOT scripts: hold, ramp down, hold, jump.

    Times in seconds; the real scripts work in 10 us ticks and convert to ms.
    """
    def chans(freqs_mhz, amps, famp_slopes=None, amp_slopes=None):
        famp_slopes = famp_slopes or [0.0] * 4
        amp_slopes = amp_slopes or [0.0] * 4
        return [ChannelState(freq=freqs_mhz[i] * 1e6, amp=amps[i],
                             freq_slope=famp_slopes[i], amp_slope=amp_slopes[i])
                for i in range(4)]

    f_mot = [80.0, 90.0, 100.0, 110.0]
    a_mot = [0.20, 0.20, 0.20, 0.20]

    return Pattern([
        Event("MOT", 0.000, chans(f_mot, a_mot), xio=0b001),
        # ramp all four amplitudes 0.20 -> 0.05 over 50 ms
        Event("RampStart", 0.100,
              chans(f_mot, a_mot, amp_slopes=[(0.05 - 0.20) / 0.050] * 4), xio=0b001),
        Event("RampEnd", 0.150, chans(f_mot, [0.05] * 4), xio=0b000),
        # sweep channel 1 by +5 MHz over 100 ms
        Event("Lambda", 0.200,
              chans([75.0, 90.0, 100.0, 115.0], [0.10] * 4,
                    famp_slopes=[5e6 / 0.100, 0, 0, 0]), xio=0b010),
        Event("Image", 0.300, chans(f_mot, [0.0] * 4), xio=0b100),
        Event("Idle", 0.400, chans(f_mot, [0.0] * 4), xio=0b000),
    ])


def wait_until_armed(card, timeout=1.0):
    """Wait for a flushed command block to park waiting for its trigger.

    Only meaningful straight after a WRITE_TO_CARD: the flag tracks the command
    queue, not the trigger engine (see DDSCard.armed).
    """
    deadline = time.perf_counter() + timeout
    while time.perf_counter() < deadline:
        if card.status & SPCM_DDS_STAT_WAITING_FOR_TRG:
            return True
    return False


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("--outputs", action="store_true",
                    help="enable the analog outputs (RF really comes out)")
    ap.add_argument("--shots", type=int, default=5)
    ap.add_argument("--level-mv", type=int, default=500)
    args = ap.parse_args()

    pattern = demo_pattern()

    with DDSCard() as card:
        card.configure_dds(out_level_mv=args.level_mv,
                           enable_outputs=args.outputs)
        # Clear any engine state left behind by a previous session: a still-live
        # TRG_SRC = TIMER drains each block as fast as it is queued, so the card
        # never parks armed. RESET also zeroes SPC_DDS_TRG_COUNT.
        card.reset_dds()
        card.route_four_channels()

        caps = card.capabilities()
        problems = pattern.validate(caps, caps["queue_cmd_max"])
        if problems:
            print("pattern rejected:")
            for p in problems:
                print("  - %s" % p)
            return
        print("pattern validated: %d events, ~%d commands (queue holds %d)"
              % (len(pattern), pattern.command_estimate(), caps["queue_cmd_max"]))
        print("outputs %s at %d mV\n"
              % ("ENABLED" if args.outputs else "disabled", args.level_mv))

        card.start()

        # Prologue: silence everything and hand the trigger to the card input.
        # This is also the priming block the card requires before EXEC_NOW works.
        card.silence_all_cores()
        for ch, core in enumerate(CORE_FOR_CHANNEL):
            card.set_core(core, freq=pattern.events[0].channels[ch].freq, amp=0.0)
        card.set_i(SPC_DDS_TRG_SRC, SPCM_DDS_TRG_SRC_CARD)
        card.prime(force=True)

        steps = len(pattern)
        span = pattern.events[-1].time_s - pattern.events[0].time_s
        before_shots = card.trg_count

        for shot in range(1, args.shots + 1):
            compile_to_card(card, pattern)
            card.write_to_card()

            # The flag tracks the queue, so it only goes high once the blocks
            # have actually reached the card.
            if not wait_until_armed(card):
                print("shot %d: card never armed (status %s)" % (shot, card.status_text()))
                break

            queued, _ = card.queue_fill()
            trg_before = card.trg_count

            t0 = time.perf_counter()
            card.force_trigger()

            # The first block goes live on the force; the internal timer walks
            # the rest. Neither of the obvious "is it finished" tests works:
            # QUEUE_CMD_COUNT hits zero one event early because the card always
            # holds one block in its shadow registers, and WAITING_FOR_TRG
            # glitches low for an instant at every trigger boundary. TRG_COUNT is
            # monotonic, so count the N triggers the N blocks consume.
            deadline = t0 + span + 1.0
            while (card.trg_count - trg_before) < steps and time.perf_counter() < deadline:
                pass
            elapsed = time.perf_counter() - t0

            status = card.status
            flags = []
            if status & SPCM_DDS_STAT_QUEUE_UNDERRUN:
                flags.append("UNDERRUN")
            if status & SPCM_DDS_STAT_QUEUE_OVERRUN:
                flags.append("OVERRUN")
            print("shot %d: queued %3d cmds, ran in %.4f s (span %.4f s), "
                  "trg +%d (expected %d), total %d%s"
                  % (shot, queued, elapsed, span,
                     card.trg_count - trg_before, steps, card.trg_count,
                     "  " + "|".join(flags) if flags else ""))

        print("\ntriggers over %d shots: %d (expected %d)"
              % (args.shots, card.trg_count - before_shots, args.shots * steps))

        # Leave the card quiet.
        card.silence_all_cores()
        card.exec_now()
        card.write_to_card()
        card.stop()
        print("\nfinal status: %s" % card.status_text())


if __name__ == "__main__":
    main()
