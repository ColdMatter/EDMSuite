"""Narrow down why SPCM_DDS_CMD_EXEC_NOW returns err 267 (setup isn't valid)."""

from spcm_core.regs import (
    SPC_DDS_TRG_SRC,
    SPCM_DDS_TRG_SRC_NONE,
    SPCM_DDS_TRG_SRC_TIMER,
    SPC_DDS_CMD,
    SPCM_DDS_CMD_EXEC_NOW,
)

from spectrum_dds import DDSCard, SpcmError


def attempt(label, fn):
    try:
        fn()
        print("  %-46s OK" % label)
        return True
    except SpcmError as e:
        print("  %-46s FAIL: %s" % (label, str(e).split("-> ")[-1]))
        return False


def trial(enable_outputs, do_reset, trg_src, use_i32):
    label = "out=%d reset=%d src=%s i32=%d" % (
        enable_outputs, do_reset, trg_src, use_i32)
    with DDSCard() as card:
        try:
            card.configure_dds(out_level_mv=500, enable_outputs=enable_outputs)
            card.route_four_channels()
            if do_reset:
                card.reset_dds()
                card.route_four_channels()
            card.start()
            card.set_i(SPC_DDS_TRG_SRC, trg_src)
            if use_i32:
                from spcm_core.pyspcm import spcm_dwSetParam_i32
                err = spcm_dwSetParam_i32(card.handle, SPC_DDS_CMD, SPCM_DDS_CMD_EXEC_NOW)
                if err:
                    raise SpcmError("i32 exec_now err %d" % err)
            else:
                card.exec_now()
            card.write_to_card()
            print("  %-46s OK" % label)
        except SpcmError as e:
            print("  %-46s FAIL: %s" % (label, str(e).split("-> ")[-1]))
        finally:
            try:
                card.stop()
            except SpcmError:
                pass


def main():
    print("EXEC_NOW acceptance matrix:")
    for enable_outputs in (0, 1):
        for do_reset in (0, 1):
            for trg_src in (SPCM_DDS_TRG_SRC_NONE, SPCM_DDS_TRG_SRC_TIMER):
                trial(enable_outputs, do_reset, trg_src, use_i32=False)


if __name__ == "__main__":
    main()
