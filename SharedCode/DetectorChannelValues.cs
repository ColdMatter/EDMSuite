using System;
using System.Collections.Generic;
using System.Text;

namespace Analysis.EDM
{
    [Serializable]
    class DetectorChannelValues
    {
        Dictionary<string, int> switchBits = new Dictionary<string, int>();
        internal double[] values;
        internal double[] errors;

        public int GetChannelIndex(string channelName)
        {
            return 0;
        }

        public double GetChannel(int channelIndex)
        {
            return 0;
        }

        public double GetChannelError(int channelIndex)
        {
            return 0;
        }
    }
}
