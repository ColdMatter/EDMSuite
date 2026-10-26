using System;
using System.Collections.Generic;
using DAQ.Pattern;
using DAQ.Analog;
using MOTMaster;

namespace MOTMaster.SnippetLibrary.CaF
{
    // Lambda cooling from LambdaCooling1StartTime. See LambdaCooling.
    public class LambdaCooling1 : LambdaCooling
    {
        public LambdaCooling1(PatternBuilder32 p, Dictionary<String, Object> parameters) : base("LambdaCooling1", p, parameters) { }
        public LambdaCooling1(AnalogPatternBuilder p, Dictionary<String, Object> parameters) : base("LambdaCooling1", p, parameters) { }
        public LambdaCooling1(DDSPatternBuilder p, Dictionary<String, Object> parameters) : base("LambdaCooling1", p, parameters) { }
    }
}
