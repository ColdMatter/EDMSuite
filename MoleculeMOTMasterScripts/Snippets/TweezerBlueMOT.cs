using System;
using System.Collections.Generic;
using DAQ.Pattern;
using DAQ.Analog;
using MOTMaster;

namespace MOTMaster.SnippetLibrary.CaF
{
    // Blue MOT in the tweezer chamber: tweezer coils to tBlueMOTField.
    // Starts at tBlueMOTStartTime. See BlueMOTStage.
    public class TweezerBlueMOT : BlueMOTStage
    {
        public TweezerBlueMOT(PatternBuilder32 p, Dictionary<String, Object> parameters) : base("tBlueMOT", "TweezerCoils", "tBlueMOTField", p, parameters) { }
        public TweezerBlueMOT(AnalogPatternBuilder p, Dictionary<String, Object> parameters) : base("tBlueMOT", "TweezerCoils", "tBlueMOTField", p, parameters) { }
        public TweezerBlueMOT(DDSPatternBuilder p, Dictionary<String, Object> parameters) : base("tBlueMOT", "TweezerCoils", "tBlueMOTField", p, parameters) { }
    }
}
