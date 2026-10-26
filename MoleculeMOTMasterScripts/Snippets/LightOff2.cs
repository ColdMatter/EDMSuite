using System;
using System.Collections.Generic;
using DAQ.Pattern;
using DAQ.Analog;
using MOTMaster;

namespace MOTMaster.SnippetLibrary.CaF
{
    // Light off at LightOff2StartTime. See LightOff.
    public class LightOff2 : LightOff
    {
        public LightOff2(PatternBuilder32 p, Dictionary<String, Object> parameters) : base("LightOff2", p, parameters) { }
        public LightOff2(AnalogPatternBuilder p, Dictionary<String, Object> parameters) : base("LightOff2", p, parameters) { }
        public LightOff2(DDSPatternBuilder p, Dictionary<String, Object> parameters) : base("LightOff2", p, parameters) { }
    }
}
