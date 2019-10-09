using System;
using System.Collections.Generic;
using Data;
using EDMConfig;

namespace Analysis.EDM
{
    [Serializable]
    public class TOFDemodulatedBlock : DemodulatedBlock
    {
        public TOFDemodulatedBlock(DateTime timeStamp, BlockConfig config, List<string> pointDetectors)
            :base(timeStamp, config, DemodulatedBlockType.TOF, pointDetectors)
        {
        }

        public TOF GetTOFChannel(string[] switches, string detector)
        {
            ChannelSet channelSet = GetChannelSet(detector);
            return (TOF)channelSet.GetChannel(switches).Difference;
        }

        public TOF GetTOFChannel(string channelName, string detector)
        {
            ChannelSet channelSet = GetChannelSet(detector);
            return (TOF)channelSet.GetChannel(channelName).Difference;
        }
    }
}
