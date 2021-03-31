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
                foreach (string channel in val.Channels) AddChannel(channel, new TOFAccumulator());
                Count = 0;
            }
            foreach (string channel in val.Channels)
                ((TOFAccumulator)GetChannel(channel)).Add((TOF)val.GetChannel(channel));
            Count++;
        }

        public void Add(ChannelSet<TOFWithError> val)
        {
            if (Channels.Count == 0)
            {
                foreach (string channel in val.Channels) AddChannel(channel, new TOFAccumulator());
                Count = 0;
            }
            foreach (string channel in val.Channels)
                ((TOFAccumulator)GetChannel(channel)).Add((TOFWithError)val.GetChannel(channel));
            Count++;
        }

        public ChannelSet<TOFWithError> GetResult()
        {
            ChannelSet<TOFWithError> cs = new ChannelSet<TOFWithError>();
            foreach (string channel in Channels)
                cs.AddChannel(channel, ((TOFAccumulator)GetChannel(channel)).GetResult());
            return cs;
        }
    }
}
