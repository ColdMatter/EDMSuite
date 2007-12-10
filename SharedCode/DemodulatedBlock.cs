using System;
using System.Collections.Generic;
using System.Text;

using EDMConfig;

namespace Analysis.EDM
{
    [Serializable]
    public class DemodulatedBlock
    {
        public DateTime TimeStamp;
        public BlockConfig Config;
        public DemodulationConfig DemodulationConfig;

        private List<DetectorChannelValues> channelList = new List<DetectorChannelValues>();
        public DetectorFT NormFourier;

        // debug stuff - to be removed
        public List<uint> switchStates;
    }
}
