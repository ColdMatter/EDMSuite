using System;
using System.Collections.Generic;
using Data;
using EDMConfig;

namespace Analysis.EDM
{
    [Serializable]
    public class TOFDemodulatedBlock
    {
        public DateTime TimeStamp;
        public BlockConfig Config;
        public string Detector;
        public int DetectorIndex;
        public double DetectorCalibration;
        public TOFChannelSet TOFChannels;

        public TOFDemodulatedBlock()
        {
            TOFChannels = TOFChannelSet.Zero();
        }

        // This is a convenience function that pulls out the TOF Channel for a particular detector and switch
        // It returns a TOF
        public TOF GetTOFChannel(string[] switches)
        {
            return TOFChannels.GetChannel(switches).Difference;
        }
    }
}
