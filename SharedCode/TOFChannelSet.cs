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
            foreach (string[] channel in t1.Channels)
                t.AddChannel(channel, (TOFChannel)t1.GetChannel(channel) + (TOFChannel)t2.GetChannel(channel));
            return t;
        }

        static public TOFChannelSet operator -(TOFChannelSet t1, TOFChannelSet t2)
        {
            TOFChannelSet t = new TOFChannelSet();
            //t.SwitchMasks = t1.SwitchMasks;
            //t.Channels = new Channel<TOFWithError>[t1.Channels.Length];
            foreach (string[] channel in t1.Channels)
                t.AddChannel(channel, (TOFChannel)t1.GetChannel(channel) - (TOFChannel)t2.GetChannel(channel));
            return t;
        }

        static public TOFChannelSet operator /(TOFChannelSet t, double d)
        {
            TOFChannelSet temp = new TOFChannelSet();
            //temp.SwitchMasks = t.SwitchMasks;
            foreach (string[] channel in t.Channels) 
                temp.AddChannel(channel, (TOFChannel)t.GetChannel(channel) / d); 
            return temp;
        }

        static public TOFChannelSet operator *(TOFChannelSet t, double d)
        {
            TOFChannelSet temp = new TOFChannelSet();
            //temp.SwitchMasks = t.SwitchMasks;
            foreach (string[] channel in t.Channels)
                temp.AddChannel(channel, (TOFChannel)t.GetChannel(channel) * d); 
            return temp;
        }
    }
}
