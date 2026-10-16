using System;
using System.Globalization;
using System.Text;

namespace SpectrumDDS
{
    /// <summary>
    /// What the card says it can do, read from its SPC_DDS_AVAIL_* registers.
    /// </summary>
    /// <remarks>
    /// Read from the card rather than hard-coded, because these are what
    /// <see cref="DDSPattern.Validate"/> checks against and the card rejects
    /// out-of-range writes outright. For the M4i.9622 in this experiment the
    /// values come out as 0 to 625 MHz in 0.291 Hz steps, amplitude +/-1 in
    /// 3.05e-5 steps, and 50 cores.
    /// </remarks>
    [Serializable]
    public class DDSCapabilities
    {
        public int CoreCount { get; set; }
        public int QueueCommandMaximum { get; set; }

        public double FrequencyMinimum { get; set; }
        public double FrequencyMaximum { get; set; }
        public double FrequencyStep { get; set; }

        public double AmplitudeMinimum { get; set; }
        public double AmplitudeMaximum { get; set; }
        public double AmplitudeStep { get; set; }

        public double FrequencySlopeMinimum { get; set; }
        public double FrequencySlopeMaximum { get; set; }
        public double FrequencySlopeStep { get; set; }

        public double AmplitudeSlopeMinimum { get; set; }
        public double AmplitudeSlopeMaximum { get; set; }
        public double AmplitudeSlopeStep { get; set; }

        public static DDSCapabilities Read(SpcmCard card)
        {
            return new DDSCapabilities
            {
                CoreCount = card.GetInt(SpcmRegs.SPC_DDS_NUM_CORES),
                QueueCommandMaximum = card.GetInt(SpcmRegs.SPC_DDS_QUEUE_CMD_MAX),

                FrequencyMinimum = card.GetDouble(SpcmRegs.SPC_DDS_AVAIL_FREQ_MIN),
                FrequencyMaximum = card.GetDouble(SpcmRegs.SPC_DDS_AVAIL_FREQ_MAX),
                FrequencyStep = card.GetDouble(SpcmRegs.SPC_DDS_AVAIL_FREQ_STEP),

                AmplitudeMinimum = card.GetDouble(SpcmRegs.SPC_DDS_AVAIL_AMP_MIN),
                AmplitudeMaximum = card.GetDouble(SpcmRegs.SPC_DDS_AVAIL_AMP_MAX),
                AmplitudeStep = card.GetDouble(SpcmRegs.SPC_DDS_AVAIL_AMP_STEP),

                FrequencySlopeMinimum = card.GetDouble(SpcmRegs.SPC_DDS_AVAIL_FREQ_SLOPE_MIN),
                FrequencySlopeMaximum = card.GetDouble(SpcmRegs.SPC_DDS_AVAIL_FREQ_SLOPE_MAX),
                FrequencySlopeStep = card.GetDouble(SpcmRegs.SPC_DDS_AVAIL_FREQ_SLOPE_STEP),

                AmplitudeSlopeMinimum = card.GetDouble(SpcmRegs.SPC_DDS_AVAIL_AMP_SLOPE_MIN),
                AmplitudeSlopeMaximum = card.GetDouble(SpcmRegs.SPC_DDS_AVAIL_AMP_SLOPE_MAX),
                AmplitudeSlopeStep = card.GetDouble(SpcmRegs.SPC_DDS_AVAIL_AMP_SLOPE_STEP),
            };
        }

        public override string ToString()
        {
            StringBuilder s = new StringBuilder();
            CultureInfo c = CultureInfo.InvariantCulture;
            s.AppendFormat(c, "cores           {0}\r\n", CoreCount);
            s.AppendFormat(c, "queue           {0} commands\r\n", QueueCommandMaximum);
            s.AppendFormat(c, "frequency       {0:g6} to {1:g6} MHz, step {2:g6} Hz\r\n",
                FrequencyMinimum / 1e6, FrequencyMaximum / 1e6, FrequencyStep);
            s.AppendFormat(c, "amplitude       {0:g4} to {1:g4}, step {2:g6}\r\n",
                AmplitudeMinimum, AmplitudeMaximum, AmplitudeStep);
            s.AppendFormat(c, "frequency slope {0:g6} to {1:g6} Hz/s, step {2:g6}\r\n",
                FrequencySlopeMinimum, FrequencySlopeMaximum, FrequencySlopeStep);
            s.AppendFormat(c, "amplitude slope {0:g6} to {1:g6} /s, step {2:g6}\r\n",
                AmplitudeSlopeMinimum, AmplitudeSlopeMaximum, AmplitudeSlopeStep);
            return s.ToString();
        }
    }

    /// <summary>Card identity, for the GUI's status tab.</summary>
    [Serializable]
    public class DDSCardIdentity
    {
        public int CardType { get; set; }
        public int SerialNumber { get; set; }
        public int ExtendedFeatures { get; set; }
        public int AvailableCardModes { get; set; }
        public int ChannelCount { get; set; }
        public long SampleRate { get; set; }

        public static DDSCardIdentity Read(SpcmCard card)
        {
            return new DDSCardIdentity
            {
                CardType = card.GetInt(SpcmRegs.SPC_PCITYP),
                SerialNumber = card.GetInt(SpcmRegs.SPC_PCISERIALNO),
                ExtendedFeatures = card.GetInt(SpcmRegs.SPC_PCIEXTFEATURES),
                AvailableCardModes = card.GetInt(SpcmRegs.SPC_AVAILCARDMODES),
                ChannelCount = card.GetInt(SpcmRegs.SPC_MIINST_MODULES) *
                               card.GetInt(SpcmRegs.SPC_MIINST_CHPERMODULE),
                SampleRate = card.GetLong(SpcmRegs.SPC_SAMPLERATE),
            };
        }

        /// <summary>e.g. "M4i.9622" -- the low 16 bits of SPC_PCITYP are the model.</summary>
        public string Model
        {
            get { return string.Format("M4i.{0:X4}", CardType & 0xFFFF); }
        }

        public bool HasDDS50 { get { return (ExtendedFeatures & SpcmRegs.SPCM_FEAT_EXTFW_DDS50) != 0; } }

        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture,
                "{0} serial {1}, {2} channels, {3:g6} MS/s, DDS50 firmware {4}",
                Model, SerialNumber, ChannelCount, SampleRate / 1e6, HasDDS50 ? "present" : "ABSENT");
        }
    }
}
