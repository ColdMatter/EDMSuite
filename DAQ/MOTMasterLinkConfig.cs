using System;

namespace DAQ.HAL
{
    /// <summary>
    /// Declares that this machine can act as the primary of a linked MOTMaster pair. Put one in
    /// a Hardware subclass with <c>Info.Add(MOTMasterLinkConfig.InfoKey, config)</c>.
    /// A machine without one shows no linked-mode UI and behaves exactly as before.
    /// </summary>
    [Serializable]
    public class MOTMasterLinkConfig
    {
        public const string InfoKey = "MOTMasterLink";

        /// <summary>Remoting URL of the secondary MOTMaster, e.g. tcp://10.59.3.2:1187/controller.rem</summary>
        public string SecondaryUrl;

        /// <summary>Full type name of the secondary's Hardware class, e.g. DAQ.HAL.RbTweezerHardware</summary>
        public string SecondaryHardwareType;

        /// <summary>Name of the primary's digital channel wired to the secondary's PGTriggerLine.</summary>
        public string TriggerChannel;

        /// <summary>Names of the primary's analog-start channel(s), e.g. DDS_Analog_Trg.</summary>
        public string[] AnalogStartChannels;

        /// <summary>Width of the trigger pulse, in primary digital clock ticks.</summary>
        public int TriggerPulseTicks = 10;

        /// <summary>How long the primary waits for the secondary to arm each shot.</summary>
        public int ArmTimeoutMs = 5000;

        /// <summary>How long the primary waits for the secondary to finish after being told to end.</summary>
        public int EndTimeoutMs = 30000;
    }
}
