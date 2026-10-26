using System;
using System.Collections.Generic;
using DAQ.Pattern;
using DAQ.Analog;
using MOTMaster;

namespace MOTMaster.SnippetLibrary.CaF
{
    // A second blue MOT in the MOT chamber, e.g. to recapture after a magtrap: MOT coils to BlueMOTField.
    // Starts at BlueMOT2StartTime. See BlueMOTStage.
    public class BlueMOT2 : BlueMOTStage
    {
        public BlueMOT2(PatternBuilder32 p, Dictionary<String, Object> parameters) : base("BlueMOT2", "MOTCoilsCurrent", "BlueMOTField", p, parameters) { }
        public BlueMOT2(AnalogPatternBuilder p, Dictionary<String, Object> parameters) : base("BlueMOT2", "MOTCoilsCurrent", "BlueMOTField", p, parameters) { }
        public BlueMOT2(DDSPatternBuilder p, Dictionary<String, Object> parameters) : base("BlueMOT2", "MOTCoilsCurrent", "BlueMOTField", p, parameters) { }
    }
}
