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

        public Dictionary<string, int> DetectorIndices = new Dictionary<string, int>();
        public List<DetectorChannelValues> ChannelValues = new List<DetectorChannelValues>();
        public DetectorFT NormFourier;

    }
}
