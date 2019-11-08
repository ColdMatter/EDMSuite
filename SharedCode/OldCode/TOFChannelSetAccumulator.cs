//using System;
//using System.Collections.Generic;
//using System.Text;

//using Data;
//using Analysis;

//namespace Analysis.EDM
//{
//    // Note that it would make some sense here to define a generic base class 
//    // that captures the notion of an accumulatable channelset as in 
//    // [ChannelSetAccumulator : ChannelSet<TAccumulator>, IAccumulator<ChannelSet<TData>>]. That
//    // class could then be subclassed to TOFChannelSetAccumulator if that object needs any specific
//    // functionality. I haven't done that yet, as the class heirarchy is already pretty huge.
//    public class TOFChannelSetAccumulator : ChannelSet<TOFAccumulator>, IAccumulator<TOFChannelSet>
//    {
//        public void Add(TOFChannelSet val)
//        {
//            if (Channels.Count == 0)
//            {
//                foreach (string[] channel in val.Channels) AddChannel(channel, new TOFChannelAccumulator());
//                // The accumulator gets the BlockConfig of the first block added to it. This is about the
//                // best that we can do.
//                Config = val.Config;
//                Count = 0;
//            }
//            foreach (string[] channel in val.Channels) 
//                ((TOFChannelAccumulator)GetChannel(channel)).Add((TOFChannel)val.GetChannel(channel));
//            Count++;
//        }

//        public TOFChannelSet GetResult()
//        {
//            TOFChannelSet cs = new TOFChannelSet();
//            foreach (string[] channel in Channels)
//                cs.AddChannel(channel, ((TOFChannelAccumulator)GetChannel(channel)).GetResult());
//            cs.Config = Config;
//            cs.Count = Count;
//            return cs;
//        }
//    }
//}
