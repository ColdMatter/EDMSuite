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

        public List<DetectorChannelValues> ChannelValues = new List<DetectorChannelValues>();
        public DetectorFT NormFourier;

    }
}
