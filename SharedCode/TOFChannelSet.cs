using System;
using System.Collections.Generic;
using System.Text;

using Data;

namespace Analysis.EDM
{
    [Serializable]
    public class TOFChannelSet : ChannelSet<TOF>
    {
        static public TOFChannelSet operator +(TOFChannelSet t1, TOFChannelSet t2)
        {
            TOFChannelSet t = new TOFChannelSet();
            foreach (string channel in t1.Channels)
                t.AddChannel(channel, (TOFChannel)t1.GetChannel(channel) + (TOFChannel)t2.GetChannel(channel));
            return t;
        }

        static public TOFChannelSet operator -(TOFChannelSet t1, TOFChannelSet t2)
        {
            TOFChannelSet t = new TOFChannelSet();
            //t.SwitchMasks = t1.SwitchMasks;
            //t.Channels = new Channel<TOFWithError>[t1.Channels.Length];
            foreach (string channel in t1.Channels)
                t.AddChannel(channel, (TOFChannel)t1.GetChannel(channel) - (TOFChannel)t2.GetChannel(channel));
            return t;
        }

        static public TOFChannelSet operator /(TOFChannelSet t, double d)
        {
            TOFChannelSet temp = new TOFChannelSet();
            //temp.SwitchMasks = t.SwitchMasks;
            foreach (string channel in t.Channels) 
                temp.AddChannel(channel, (TOFChannel)t.GetChannel(channel) / d); 
            return temp;
        }

        static public TOFChannelSet operator *(TOFChannelSet t, double d)
        {
            TOFChannelSet temp = new TOFChannelSet();
            //temp.SwitchMasks = t.SwitchMasks;
            foreach (string channel in t.Channels)
                temp.AddChannel(channel, (TOFChannel)t.GetChannel(channel) * d); 
            return temp;
        }

        private const int NUMBER_OF_CHANNELS = 527;
        // makes a random TOFChannelSet, using random TOFChannels.
        public static TOFChannelSet Random()
        {   
            TOFChannelSet tcs = new TOFChannelSet();
            for (int i = 0; i < NUMBER_OF_CHANNELS; i++) tcs.AddChannel("c" + i, TOFChannel.Random());
            return tcs;
        }
    }
}
