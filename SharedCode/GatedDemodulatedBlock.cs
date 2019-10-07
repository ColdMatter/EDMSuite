using System;
using System.Collections.Generic;
using Data;
using EDMConfig;

namespace Analysis.EDM
{
    [Serializable]
    public class GatedDemodulatedBlock : DemodulatedBlock
    {
        public GatedDemodulationConfig GateConfig;

        public GatedDemodulatedBlock()
        {
            this.DataType = DemodulatedBlockType.GATED;
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
