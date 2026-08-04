"""Reproduce the manual's minimal DDS start sequence, one step at a time."""

import ctypes

from spcm_core.pyspcm import (
    spcm_hOpen, spcm_vClose, spcm_dwSetParam_i32, spcm_dwSetParam_d64,
    spcm_dwGetErrorInfo_i32,
)
from spcm_core.regs import *  # noqa: F403


def main():
    h = spcm_hOpen(ctypes.create_string_buffer(b"/dev/spcm0"))
    print("open:", bool(h))

    def si(name, reg, val):
        err = spcm_dwSetParam_i32(h, reg, val)
        report(name, err)

    def sd(name, reg, val):
        err = spcm_dwSetParam_d64(h, reg, val)
        report(name, err)

    def report(name, err):
        if err:
            buf = ctypes.create_string_buffer(ERRORTEXTLEN)
            spcm_dwGetErrorInfo_i32(h, None, None, buf)
            print("  %-38s ERR %d: %s" % (name, err,
                  buf.value.decode(errors="replace").strip()))
        else:
            print("  %-38s ok" % name)

    si("SPC_CHENABLE=CH0", SPC_CHENABLE, CHANNEL0)
    si("SPC_ENABLEOUT0=0", SPC_ENABLEOUT0, 0)
    si("SPC_AMP0=500", SPC_AMP0, 500)
    si("SPC_FILTER0=0", SPC_FILTER0, 0)
    si("SPC_CARDMODE=DDS", SPC_CARDMODE, SPC_REP_STD_DDS)
    si("SPC_CLOCKMODE=INTPLL", SPC_CLOCKMODE, SPC_CM_INTPLL)
    si("M2CMD START|ENABLETRIGGER", SPC_M2CMD,
       M2CMD_CARD_START | M2CMD_CARD_ENABLETRIGGER)

    sd("CORE0_AMP=0.1", SPC_DDS_CORE0_AMP, 0.1)
    sd("CORE0_FREQ=100MHz", SPC_DDS_CORE0_FREQ, 100e6)
    si("CMD EXEC_AT_TRG", SPC_DDS_CMD, SPCM_DDS_CMD_EXEC_AT_TRG)
    si("CMD WRITE_TO_CARD", SPC_DDS_CMD, SPCM_DDS_CMD_WRITE_TO_CARD)
    si("M2CMD FORCETRIGGER", SPC_M2CMD, M2CMD_CARD_FORCETRIGGER)

    print("\nnow the same but with EXEC_NOW:")
    sd("CORE0_FREQ=110MHz", SPC_DDS_CORE0_FREQ, 110e6)
    si("CMD EXEC_NOW", SPC_DDS_CMD, SPCM_DDS_CMD_EXEC_NOW)
    si("CMD WRITE_TO_CARD", SPC_DDS_CMD, SPCM_DDS_CMD_WRITE_TO_CARD)

    print("\nand with TRG_SRC set explicitly first:")
    si("TRG_SRC=NONE", SPC_DDS_TRG_SRC, SPCM_DDS_TRG_SRC_NONE)
    si("CMD EXEC_AT_TRG", SPC_DDS_CMD, SPCM_DDS_CMD_EXEC_AT_TRG)
    si("CMD WRITE_TO_CARD", SPC_DDS_CMD, SPCM_DDS_CMD_WRITE_TO_CARD)
    sd("CORE0_FREQ=120MHz", SPC_DDS_CORE0_FREQ, 120e6)
    si("CMD EXEC_NOW", SPC_DDS_CMD, SPCM_DDS_CMD_EXEC_NOW)
    si("CMD WRITE_TO_CARD", SPC_DDS_CMD, SPCM_DDS_CMD_WRITE_TO_CARD)

    si("M2CMD STOP", SPC_M2CMD, M2CMD_CARD_STOP)
    spcm_vClose(h)


if __name__ == "__main__":
    main()
