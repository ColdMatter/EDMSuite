using System;
using System.Collections.Generic;
using DAQ.Pattern;
using DAQ.Analog;
using MOTMaster;
using MOTMaster.SnippetLibrary;

namespace MOTMaster.SnippetLibrary.CaF
{
    // Magnetic trap in the MOT chamber: MOT coils to MOTCoilsMagtrapValue at MagtrapStartTime
    // (ticks after the Q-switch). Leaves the coils on: whatever comes next sets them.
    public class Magtrap : MOTMasterScriptSnippet
    {
        public Magtrap(PatternBuilder32 p, Dictionary<String, Object> parameters)
        {
            AddDigitalSnippet(p, parameters);
        }
        public Magtrap(AnalogPatternBuilder p, Dictionary<String, Object> parameters)
        {
            AddAnalogSnippet(p, parameters);
        }
        public Magtrap(DDSPatternBuilder p, Dictionary<String, Object> parameters)
        {
            AddDDSSnippet(p, parameters);
        }

        public void AddDigitalSnippet(PatternBuilder32 p, Dictionary<String, Object> parameters)
        {
        }

        public void AddAnalogSnippet(AnalogPatternBuilder p, Dictionary<String, Object> parameters)
        {
            p.AddAnalogValue("MOTCoilsCurrent", (int)parameters["MagtrapStartTime"], (double)parameters["MOTCoilsMagtrapValue"]);
        }

        public void AddDDSSnippet(DDSPatternBuilder p, Dictionary<String, Object> parameters)
        {
        }
    }
}
