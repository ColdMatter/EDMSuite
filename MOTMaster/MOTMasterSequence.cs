using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DAQ.Analog;
using DAQ.Pattern;

namespace MOTMaster
{
    public class MOTMasterSequence
    {
        public PatternBuilder32 DigitalPattern;
        public AnalogPatternBuilder AnalogPattern;
    }
}
