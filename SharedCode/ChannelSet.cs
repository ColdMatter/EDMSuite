using System;
using System.Collections.Generic;
using System.Text;

namespace Analysis.EDM
{
    /// <summary>
    /// This class represents a collection of Channels. The type is parameterised by the
    /// type that the channels contain. Note that you can subclass the Channel class
    /// to give different types of channel specific behaviours.
    /// </summary>
    [Serializable]
    public class ChannelSet<T>
    {
        public Channel<T>[] Channels;

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
