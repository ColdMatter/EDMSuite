using System;
using System.Collections.Generic;
using DAQ.Pattern;
using DAQ.Analog;
using MOTMaster;

namespace MOTMaster.SnippetLibrary.CaF
{
    // Lambda cooling from LambdaCooling2StartTime. See LambdaCooling.
    public class LambdaCooling2 : LambdaCooling
    {
        public LambdaCooling2(PatternBuilder32 p, Dictionary<String, Object> parameters) : base("LambdaCooling2", p, parameters) { }
        public LambdaCooling2(AnalogPatternBuilder p, Dictionary<String, Object> parameters) : base("LambdaCooling2", p, parameters) { }
        public LambdaCooling2(DDSPatternBuilder p, Dictionary<String, Object> parameters) : base("LambdaCooling2", p, parameters) { }
    }
}
