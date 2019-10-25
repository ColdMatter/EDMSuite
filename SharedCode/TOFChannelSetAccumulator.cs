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

        public void Add(ChannelSet<TOF> val)
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

        public void Add(ChannelSet<TOFWithError> val)
        {
            if (Channels.Count == 0)
            {
                foreach (string channel in val.Channels) AddChannel(channel, new TOFChannelAccumulator());
                Count = 0;
            }
            foreach (string channel in val.Channels)
                ((TOFChannelAccumulator)GetChannel(channel)).Add((TOFWithErrorChannel)val.GetChannel(channel));
            Count++;
        }

        public ChannelSet<TOFWithError> GetResult()
        {
            ChannelSet<TOFWithError> cs = new ChannelSet<TOFWithError>();
            foreach (string channel in Channels)
                cs.AddChannel(channel, ((TOFChannelAccumulator)GetChannel(channel)).GetResult());
            return cs;
        }
    }
}
