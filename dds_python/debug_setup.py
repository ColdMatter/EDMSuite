"""Start from the known-good minimal DDS setup and add one difference at a time,
to find which one makes SPCM_DDS_CMD_EXEC_NOW return err 267 ("setup isn't valid").
"""

import ctypes

from spcm_core.pyspcm import (
    spcm_hOpen, spcm_vClose, spcm_dwSetParam_i32, spcm_dwSetParam_i64,
    spcm_dwSetParam_d64, spcm_dwGetErrorInfo_i32,
)
from spcm_core.regs import *  # noqa: F403

ROUTING = [(1 << 47) - 1, 1 << 47, 1 << 48, 1 << 49]


def errtext(h, err):
    buf = ctypes.create_string_buffer(ERRORTEXTLEN)
    spcm_dwGetErrorInfo_i32(h, None, None, buf)
    return "err %d: %s" % (err, buf.value.decode(errors="replace").strip())


def run(label, all_channels=False, writesetup=False, routing=False,
        set_trg_src=False, cmd_i64=False, prime=False, prime_force=True):
    h = spcm_hOpen(ctypes.create_string_buffer(b"/dev/spcm0"))
    try:
        chans = 0xF if all_channels else CHANNEL0
        spcm_dwSetParam_i32(h, SPC_CHENABLE, chans)
        for ch in range(4 if all_channels else 1):
            spcm_dwSetParam_i32(h, SPC_ENABLEOUT0 + 100 * ch, 0)
            spcm_dwSetParam_i32(h, SPC_AMP0 + 100 * ch, 500)
            spcm_dwSetParam_i32(h, SPC_FILTER0 + 100 * ch, 0)
        spcm_dwSetParam_i32(h, SPC_CARDMODE, SPC_REP_STD_DDS)
        spcm_dwSetParam_i32(h, SPC_CLOCKMODE, SPC_CM_INTPLL)
        if writesetup:
            spcm_dwSetParam_i32(h, SPC_M2CMD, M2CMD_CARD_WRITESETUP)
        if routing:
            for i, mask in enumerate(ROUTING):
                spcm_dwSetParam_i64(h, SPC_DDS_CORES_ON_CH0 + i, mask)
        spcm_dwSetParam_i32(h, SPC_M2CMD,
                            M2CMD_CARD_START | M2CMD_CARD_ENABLETRIGGER)
        if prime:
            # An initial EXEC_AT_TRG + WRITE_TO_CARD block, as in the manual's
            # start sequence.
            spcm_dwSetParam_d64(h, SPC_DDS_CORE0_AMP, 0.0)
            spcm_dwSetParam_d64(h, SPC_DDS_CORE0_FREQ, 100e6)
            spcm_dwSetParam_i32(h, SPC_DDS_CMD, SPCM_DDS_CMD_EXEC_AT_TRG)
            spcm_dwSetParam_i32(h, SPC_DDS_CMD, SPCM_DDS_CMD_WRITE_TO_CARD)
            if prime_force:
                spcm_dwSetParam_i32(h, SPC_M2CMD, M2CMD_CARD_FORCETRIGGER)
        if set_trg_src:
            spcm_dwSetParam_i32(h, SPC_DDS_TRG_SRC, SPCM_DDS_TRG_SRC_TIMER)
        spcm_dwSetParam_d64(h, SPC_DDS_CORE0_FREQ, 100e6)

        setter = spcm_dwSetParam_i64 if cmd_i64 else spcm_dwSetParam_i32
        err = setter(h, SPC_DDS_CMD, SPCM_DDS_CMD_EXEC_NOW)
        print("  %-44s %s" % (label, "ok" if not err else errtext(h, err)))
        spcm_dwSetParam_i32(h, SPC_M2CMD, M2CMD_CARD_STOP)
    finally:
        spcm_vClose(h)


def main():
    print("without a priming EXEC_AT_TRG block:")
    run("baseline (1 ch, i32 cmd)")
    run("+ all 4 channels", all_channels=True)
    run("+ routing", routing=True)

    print("\nwith a priming EXEC_AT_TRG + WRITE_TO_CARD + FORCETRIGGER:")
    run("baseline", prime=True)
    run("+ all 4 channels", all_channels=True, prime=True)
    run("+ routing", routing=True, prime=True)
    run("+ trg_src=TIMER", set_trg_src=True, prime=True)
    run("+ cmd via i64", cmd_i64=True, prime=True)
    run("all together", all_channels=True, writesetup=True, routing=True,
        set_trg_src=True, prime=True)

    print("\nwith priming but no FORCETRIGGER:")
    run("baseline", prime=True, prime_force=False)


if __name__ == "__main__":
    main()
