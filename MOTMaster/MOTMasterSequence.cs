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
        //This is used to specify whether the sequence needs to generate patterns on many cards. This is mainly here to ensure old sequences will still work.
        public bool multipleCards = false;
        public PatternBuilder32 DigitalPattern;
        public AnalogPatternBuilder AnalogPattern;

        //These dictionaries are identified by a device string in the MOTMasterSequene.
        public Dictionary<string, PatternBuilder32> DigitalPatterns;
        public Dictionary<string, AnalogPatternBuilder> AnalogPatterns;
        public Dictionary<string, HSDIOPatternBuilder> HSDIOPatterns;  
            }
    }
}
