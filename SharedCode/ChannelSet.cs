using System;
using System.Collections.Generic;
using System.Text;

using EDMConfig;

namespace Analysis.EDM
{
    /// <summary>
    /// This class represents a collection of Channels. The type is parameterised by the
    /// type that the channels contain. Note that you can subclass the Channel class
    /// to give different types of channel specific behaviours.
    /// 
    /// It is used to carry the results of demodulating a Block into its channels. It carries
    /// with it the Block's BlockConfig.
    /// </summary>
    [Serializable]
    public class ChannelSet<T>
    {
        public Channel<T>[] Channels;

        public BlockConfig Config;

        public Channel<T> GetChannel(string[] switches)
        {
            return Channels[GetChannelIndex(switches)];
        }

        public Dictionary<string, uint> SwitchMasks = new Dictionary<string, uint>();
        public uint GetChannelIndex(string[] switches)
        {
            if (switches[0] == "SIG") return 0;
            else
            {
                uint index = 0;
                foreach (string s in switches) index += SwitchMasks[s];
                return index;
            } 
        }
    }
}
