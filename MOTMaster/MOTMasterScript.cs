using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DAQ.Pattern;
using DAQ.Analog;

namespace MOTMaster
{
    public abstract class MOTMasterScript
    {
        public abstract PatternBuilder32 GetDigitalPattern();
        public abstract AnalogPatternBuilder GetAnalogPattern();

        public MOTMasterSequence GetSequence()
        {
            MOTMasterSequence s = new MOTMasterSequence();
            s.DigitalPattern = GetDigitalPattern();
            s.AnalogPattern = GetAnalogPattern();
            return s;
        }
    }
}
