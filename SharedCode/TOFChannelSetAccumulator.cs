using System;
using System.Collections.Generic;
using System.Text;

using Data;
using Analysis;

namespace Analysis.EDM
{
    public class TOFChannelSetAccumulator : ChannelSet<TOFAccumulator>
    {
        private int Count;

        public void Add(TOFChannelSet val)
        {
            if (Channels.Count == 0)
            {
                foreach (string channel in val.Channels) AddChannel(channel, new TOFChannelAccumulator());
                Count = 0;
            }
            foreach (string channel in val.Channels) 
                ((TOFChannelAccumulator)GetChannel(channel)).Add((TOFChannel)val.GetChannel(channel));
            Count++;
        }

        public TOFChannelSet GetResult()
        {
            TOFChannelSet cs = new TOFChannelSet();
            foreach (string channel in Channels)
                cs.AddChannel(channel, ((TOFChannelAccumulator)GetChannel(channel)).GetResult());
            return cs;
        }
    }
}
