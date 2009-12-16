using System;
using System.Collections.Generic;
using System.Text;

using Data;

namespace Analysis.EDM
{
    [Serializable]
    public class TOFChannelSet : ChannelSet<TOFWithError>
    {
        static public TOFChannelSet operator +(TOFChannelSet t1, TOFChannelSet t2)
        {
            TOFChannelSet t = new TOFChannelSet();
            t.Config = t1.Config;
            t.SwitchMasks = t1.SwitchMasks;
            t.Channels = new Channel<TOFWithError>[t1.Channels.Length];
            for (int i = 0; i < t.Channels.Length; i++)
                t.Channels[i] = (TOFChannel)t1.Channels[i] + (TOFChannel)t2.Channels[i];
            return t;
        }

        static public TOFChannelSet operator -(TOFChannelSet t1, TOFChannelSet t2)
        {
            TOFChannelSet t = new TOFChannelSet();
            t.Config = t1.Config;
            t.SwitchMasks = t1.SwitchMasks;
            t.Channels = new Channel<TOFWithError>[t1.Channels.Length];
            for (int i = 0; i < t.Channels.Length; i++)
                t.Channels[i] = (TOFChannel)t1.Channels[i] - (TOFChannel)t2.Channels[i];
            return t;
        }

        static public TOFChannelSet operator /(TOFChannelSet t, double d)
        {
            TOFChannelSet temp = new TOFChannelSet();
            temp.SwitchMasks = t.SwitchMasks;
            temp.Config = t.Config;
            temp.Channels = new Channel<TOFWithError>[t.Channels.Length];
            for (int i = 0; i < temp.Channels.Length; i++)
                temp.Channels[i] = (TOFChannel)t.Channels[i] / d;
            return temp;
        }
    }
}
