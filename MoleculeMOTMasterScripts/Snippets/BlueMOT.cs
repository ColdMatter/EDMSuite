using System;
using System.Collections.Generic;
using DAQ.Pattern;
using DAQ.Analog;
using MOTMaster;

namespace MOTMaster.SnippetLibrary.CaF
{
    // Blue MOT in the MOT chamber: MOT coils to BlueMOTField.
    // Starts at BlueMOTStartTime. See BlueMOTStage.
    public class BlueMOT : BlueMOTStage
    {
        public BlueMOT(PatternBuilder32 p, Dictionary<String, Object> parameters) : base("BlueMOT", "MOTCoilsCurrent", "BlueMOTField", p, parameters) { }
        public BlueMOT(AnalogPatternBuilder p, Dictionary<String, Object> parameters) : base("BlueMOT", "MOTCoilsCurrent", "BlueMOTField", p, parameters) { }
        public BlueMOT(DDSPatternBuilder p, Dictionary<String, Object> parameters) : base("BlueMOT", "MOTCoilsCurrent", "BlueMOTField", p, parameters) { }
    }
}
