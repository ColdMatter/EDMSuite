using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DAQ.Analog;
using DAQ.Pattern;

namespace MOTMaster2
{
    [Serializable]
    public class MOTMasterSequence
    {
        public PatternBuilder32 DigitalPattern;
        public AnalogPatternBuilder AnalogPattern;
        public MMAIConfiguration AIConfiguration;
        public HSDIOPatternBuilder HSDIOPattern;
        public MuquansBuilder MuquansPattern;
    }
}
