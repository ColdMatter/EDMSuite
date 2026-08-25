using System;

namespace SpectrumDDS
{
    /// <summary>
    /// The subset of Spectrum's regs.h that this driver uses.
    /// </summary>
    /// <remarks>
    /// Every value here was read out of the installed <c>spcm_core.regs</c> rather
    /// than transcribed from the PDF manual: the manual's DDS multi-purpose-IO
    /// tables are mangled by its PDF-to-text conversion and give
    /// <c>SPC_DDS_X0_MODE</c> as 608011, which is really
    /// <c>SPC_DDS_NUM_QUEUED_CMD_IN_SW</c>. Writing that would silently corrupt an
    /// unrelated register. Do not "correct" anything here against the manual.
    /// </remarks>
    public static class SpcmRegs
    {
        public const int ERRORTEXTLEN = 200;

        // -- card command and status ----------------------------------------
        public const int SPC_M2CMD = 100;
        public const int M2CMD_CARD_RESET = 0x1;
        public const int M2CMD_CARD_WRITESETUP = 0x2;
        public const int M2CMD_CARD_START = 0x4;
        public const int M2CMD_CARD_ENABLETRIGGER = 0x8;
        public const int M2CMD_CARD_FORCETRIGGER = 0x10;
        public const int M2CMD_CARD_DISABLETRIGGER = 0x20;
        public const int M2CMD_CARD_STOP = 0x40;

        public const int SPC_M2STATUS = 110;
        public const int M2STAT_CARD_PRETRIGGER = 0x1;
        public const int M2STAT_CARD_TRIGGER = 0x2;
        public const int M2STAT_CARD_READY = 0x4;

        /// <summary>Writes a custom line to the driver's debug log. Driver-global -- called with a NULL device handle.</summary>
        public const int SPC_WRITE_TO_LOG = 121;

        // -- card identity ---------------------------------------------------
        public const int SPC_PCITYP = 2000;
        public const int SPC_PCISERIALNO = 2030;
        public const int SPC_PCIFEATURES = 2120;
        public const int SPC_PCIEXTFEATURES = 2121;
        public const int SPCM_FEAT_EXTFW_DDS50 = 0x20;
        public const int SPC_MIINST_MODULES = 1100;
        public const int SPC_MIINST_CHPERMODULE = 1110;

        // -- card setup ------------------------------------------------------
        public const int SPC_CARDMODE = 9500;
        public const int SPC_AVAILCARDMODES = 9501;
        public const int SPC_REP_STD_DDS = 0x4000000;
        public const int SPC_CHENABLE = 11000;
        public const int SPC_CLOCKMODE = 20200;
        public const int SPC_CM_INTPLL = 1;
        public const int SPC_SAMPLERATE = 20000;

        // Per-channel output stage. The four channels are 100 registers apart.
        public const int SPC_ENABLEOUT0 = 30091;
        public const int SPC_AMP0 = 30010;
        public const int SPC_FILTER0 = 30080;
        public const int ChannelRegisterStride = 100;

        public static int EnableOut(int channel) { return SPC_ENABLEOUT0 + channel * ChannelRegisterStride; }
        public static int Amp(int channel) { return SPC_AMP0 + channel * ChannelRegisterStride; }
        public static int Filter(int channel) { return SPC_FILTER0 + channel * ChannelRegisterStride; }

        // -- card trigger engine ---------------------------------------------
        public const int SPC_TRIG_ORMASK = 40410;
        public const int SPC_TRIG_ANDMASK = 40430;
        public const int SPC_TRIG_AVAILORMASK = 40400;
        public const int SPC_TRIG_EXT0_MODE = 40510;
        public const int SPC_TRIG_EXT0_LEVEL0 = 42320;
        public const int SPC_TRIG_EXT0_LEVEL1 = 42330;
        public const int SPC_TRIG_TERM = 40110;

        public const int SPC_TMASK_NONE = 0x0;
        public const int SPC_TMASK_SOFTWARE = 0x1;
        public const int SPC_TMASK_EXT0 = 0x2;
        public const int SPC_TMASK_EXT1 = 0x4;

        public const int SPC_TM_NONE = 0x0;
        public const int SPC_TM_POS = 0x1;
        public const int SPC_TM_NEG = 0x2;
        public const int SPC_TM_BOTH = 0x4;
        public const int SPC_TM_HIGH = 0x8;
        public const int SPC_TM_LOW = 0x10;

        // -- DDS command queue -----------------------------------------------
        public const int SPC_DDS_CMD = 608003;
        public const int SPCM_DDS_CMD_RESET = 0x1;
        public const int SPCM_DDS_CMD_EXEC_AT_TRG = 0x2;
        public const int SPCM_DDS_CMD_EXEC_NOW = 0x4;
        public const int SPCM_DDS_CMD_WRITE_TO_CARD = 0x8;

        public const int SPC_DDS_QUEUE_CMD_COUNT = 608006;
        public const int SPC_DDS_QUEUE_CMD_MAX = 608005;
        public const int SPC_DDS_NUM_QUEUED_CMD_IN_SW = 608011;
        public const int SPC_DDS_DATA_TRANSFER_MODE = 608012;
        public const int SPCM_DDS_DTM_SINGLE = 0;
        public const int SPCM_DDS_DTM_DMA = 1;

        // -- DDS trigger ------------------------------------------------------
        public const int SPC_DDS_TRG_SRC = 608000;
        public const int SPCM_DDS_TRG_SRC_NONE = 0;
        public const int SPCM_DDS_TRG_SRC_TIMER = 1;
        public const int SPCM_DDS_TRG_SRC_CARD = 2;
        public const int SPC_DDS_TRG_TIMER = 608001;
        public const int SPC_DDS_TRG_COUNT = 608013;

        public const int SPC_DDS_STATUS = 608004;
        public const int SPCM_DDS_STAT_WAITING_FOR_TRG = 0x1;
        public const int SPCM_DDS_STAT_QUEUE_UNDERRUN = 0x2;
        public const int SPCM_DDS_STAT_QUEUE_OVERRUN = 0x4;

        // -- DDS cores ---------------------------------------------------------
        public const int SPC_DDS_NUM_CORES = 608007;
        public const int SPC_DDS_CORES_ON_CH0 = 608040;

        public const int SPC_DDS_CORE0_FREQ = 603000;
        public const int SPC_DDS_CORE0_FREQ_SLOPE = 604000;
        public const int SPC_DDS_CORE0_AMP = 605000;
        public const int SPC_DDS_CORE0_AMP_SLOPE = 606000;
        public const int SPC_DDS_CORE0_PHASE = 607000;

        public static int CoresOnChannel(int channel) { return SPC_DDS_CORES_ON_CH0 + channel; }
        public static int CoreFreq(int core) { return SPC_DDS_CORE0_FREQ + core; }
        public static int CoreFreqSlope(int core) { return SPC_DDS_CORE0_FREQ_SLOPE + core; }
        public static int CoreAmp(int core) { return SPC_DDS_CORE0_AMP + core; }
        public static int CoreAmpSlope(int core) { return SPC_DDS_CORE0_AMP_SLOPE + core; }
        public static int CorePhase(int core) { return SPC_DDS_CORE0_PHASE + core; }

        // -- DDS capability registers -------------------------------------------
        public const int SPC_DDS_AVAIL_FREQ_MIN = 608500;
        public const int SPC_DDS_AVAIL_FREQ_MAX = 608501;
        public const int SPC_DDS_AVAIL_FREQ_STEP = 608502;
        public const int SPC_DDS_AVAIL_FREQ_SLOPE_MIN = 608503;
        public const int SPC_DDS_AVAIL_FREQ_SLOPE_MAX = 608504;
        public const int SPC_DDS_AVAIL_FREQ_SLOPE_STEP = 608505;
        public const int SPC_DDS_AVAIL_AMP_MIN = 608506;
        public const int SPC_DDS_AVAIL_AMP_MAX = 608507;
        public const int SPC_DDS_AVAIL_AMP_STEP = 608508;
        public const int SPC_DDS_AVAIL_AMP_SLOPE_MIN = 608509;
        public const int SPC_DDS_AVAIL_AMP_SLOPE_MAX = 608510;
        public const int SPC_DDS_AVAIL_AMP_SLOPE_STEP = 608511;
        public const int SPC_DDS_AVAIL_PHASE_MIN = 608512;
        public const int SPC_DDS_AVAIL_PHASE_MAX = 608513;
        public const int SPC_DDS_AVAIL_PHASE_STEP = 608514;

        // -- multi-purpose IO ----------------------------------------------------
        public const int SPCM_X0_MODE = 600200;
        public const int SPCM_XMODE_DISABLE = 0x0;
        public const int SPCM_XMODE_DDS = 0x4;

        public const int SPC_DDS_X0_MODE = 608060;
        public const int SPC_DDS_X_MANUAL_OUTPUT = 608010;
        public const int SPCM_DDS_XMODE_MANUAL = 1;
        public const int SPCM_DDS_XMODE_WAITING_FOR_TRG = 2;
        public const int SPCM_DDS_XMODE_EXEC = 3;

        public static int XMode(int line) { return SPCM_X0_MODE + line; }
        public static int DDSXMode(int line) { return SPC_DDS_X0_MODE + line; }
    }
}
