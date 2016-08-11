using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DAQ.Analog;
using DAQ.Pattern;
using Newtonsoft.Json;

namespace MOTMaster
{
    [Serializable,JsonObject]
    public class MOTMasterSequence
    {
        //This is used to specify whether the sequence needs to generate patterns on many cards. This is mainly here to ensure old sequences will still work.
        public bool multipleCards = false;
        public PatternBuilder32 DigitalPattern;
        public AnalogPatternBuilder AnalogPattern;
        //TODO make the Digital and Analog Patterns generic, as in they do not need to know about the hardware
        //These dictionaries are identified by a device string in the MOTMasterSequene.
        public List<PatternBuilder32> DigitalPatterns;
        public List<AnalogPatternBuilder> AnalogPatterns;
    }
}
