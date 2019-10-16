using System;
using System.Collections.Generic;
using Data;
using EDMConfig;

namespace Analysis.EDM
{
    [Serializable]
    public class GatedDemodulatedBlock : DemodulatedBlock
    {
        public GatedDemodulationConfig GateConfig { get; set; }

        public GatedDemodulatedBlock(DateTime timeStamp, BlockConfig config, List<string> pointDetectors, GatedDemodulationConfig gateConfig)
            :base(timeStamp, config, DemodulatedBlockType.GATED, pointDetectors)
        {
            this.GateConfig = gateConfig;
        }

        public double[] GetChannelValueAndError(string[] switches, string detector)
        {
            ChannelSet channelSet = GetChannelSet(detector);
            GatedChannel channel = (GatedChannel)channelSet.GetChannel(switches);
            return new double[] { channel.Difference.Value, channel.Difference.Error };
        }

        public double[] GetChannelValueAndError(string channelName, string detector)
        {
            ChannelSet channelSet = GetChannelSet(detector);
            GatedChannel channel = (GatedChannel)channelSet.GetChannel(channelName);
            return new double[] { channel.Difference.Value, channel.Difference.Error };
        }
    }
}
