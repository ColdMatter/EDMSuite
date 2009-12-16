using System;
using System.Collections.Generic;
using System.Text;

using Data;
using Analysis;

namespace Analysis.EDM
{
    // Note that it would make some sense here to define a generic base class 
    // that captures the notion of an accumulatable channelset as in 
    // [ChannelSetAccumulator : ChannelSet<TAccumulator>, IAccumulator<ChannelSet<TData>>]. That
    // class could then be subclassed to TOFChannelSetAccumulator if that object needs any specific
    // functionality. I haven't done that yet, as the class heirarchy is already pretty huge.
    public class TOFChannelSetAccumulator : ChannelSet<TOFAccumulator>, IAccumulator<TOFChannelSet>
    {
        public void Add(TOFChannelSet val)
        {
            if (Channels == null)
            {
                Channels = new TOFChannelAccumulator[val.Channels.Length];
                for (int i = 0; i < Channels.Length; i++) Channels[i] = new TOFChannelAccumulator();
                SwitchMasks = val.SwitchMasks;
                // The accumulator gets the BlockConfig of the first block added to it. This is about the
                // best that we can do.
                Config = val.Config;
            }
            for (int i = 0; i < Channels.Length; i++) ((TOFChannelAccumulator)Channels[i]).Add((TOFChannel)val.Channels[i]);
        }

        public TOFChannelSet GetResult()
        {
            TOFChannelSet cs = new TOFChannelSet();
            cs.Channels = new TOFChannel[Channels.Length];
            for (int i = 0; i < Channels.Length; i++) cs.Channels[i] = ((TOFChannelAccumulator)Channels[i]).GetResult();
            cs.SwitchMasks = SwitchMasks;
            cs.Config = Config;
            return cs;
        }
    }
}
