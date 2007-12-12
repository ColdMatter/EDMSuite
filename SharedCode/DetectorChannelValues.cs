using System;
using System.Collections.Generic;
using System.Text;

namespace Analysis.EDM
{
    [Serializable]
    public class DetectorChannelValues
    {
        public double[] Values;
        public double[] Errors;

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

        public double GetValue(int channelIndex)
        {
            return Values[channelIndex];
        }

        public double GetValue(string[] switches)
        {
            return Values[GetChannelIndex(switches)];
        }

        public double GetError(int channelIndex)
        {
            return Errors[channelIndex];
        }

        public double GetError(string[] switches)
        {
            return Errors[GetChannelIndex(switches)];
        }
    }
}
