using System;
using System.Collections.Generic;
using DAQ.Pattern;
using DAQ.Analog;
using MOTMaster;

namespace MOTMaster.SnippetLibrary.CaF
{
    // Light off at LightOff1StartTime. See LightOff.
    public class LightOff1 : LightOff
    {
        public LightOff1(PatternBuilder32 p, Dictionary<String, Object> parameters) : base("LightOff1", p, parameters) { }
        public LightOff1(AnalogPatternBuilder p, Dictionary<String, Object> parameters) : base("LightOff1", p, parameters) { }
        public LightOff1(DDSPatternBuilder p, Dictionary<String, Object> parameters) : base("LightOff1", p, parameters) { }
    }
}
