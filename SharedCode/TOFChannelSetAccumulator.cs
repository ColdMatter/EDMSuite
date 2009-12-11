using System;
using System.Collections.Generic;
using System.Text;

using Data;
using Analysis;

namespace Analysis.EDM
{
    public class TOFChannelSetAccumulator : ChannelSet<TOFAccumulator>, IAccumulator<ChannelSet<TOFWithError>>
    {
        public void Add(ChannelSet<TOFWithError> val)
        {
            if (Channels == null)
            {
                Channels = new TOFChannelAccumulator[val.Channels.Length];
                for (int i = 0; i < Channels.Length; i++) Channels[i] = new TOFChannelAccumulator();
                SwitchMasks = val.SwitchMasks;
            }
            for (int i = 0; i < Channels.Length; i++) ((TOFChannelAccumulator)Channels[i]).Add((TOFChannel)val.Channels[i]);
        }

        public ChannelSet<TOFWithError> GetResult()
        {
            ChannelSet<TOFWithError> cs = new ChannelSet<TOFWithError>();
            cs.Channels = new Channel<TOFWithError>[Channels.Length];
            for (int i = 0; i < Channels.Length; i++) cs.Channels[i] = ((TOFChannelAccumulator)Channels[i]).GetResult();
            cs.SwitchMasks = SwitchMasks;
            return cs;
        }
    }
}
