using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DAQ.Analog;
using DAQ.Pattern;

namespace MOTMaster
{
    [Serializable]
    public class MOTMasterSequence
    {
        public Dictionary<string , PatternBuilder32> DigitalPatterns;
        public AnalogPatternBuilder AnalogPattern;
        public MMAIConfiguration AIConfiguration;
        public HSDIOPatternBuilder HSDIOPattern;
    }
}
