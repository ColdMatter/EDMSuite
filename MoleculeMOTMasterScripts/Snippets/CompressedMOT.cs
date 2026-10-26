using System;
using System.Collections.Generic;
using DAQ.Pattern;
using DAQ.Analog;
using MOTMaster;
using MOTMaster.SnippetLibrary;

namespace MOTMaster.SnippetLibrary.CaF
{
    // Compressed MOT: ramp the MOT coils up to MOTCoilsCompressionValue while ramping the
    // MOT light down to RampEndAmpDDS1-4, both over CompressRampDownDuration.
    // Starts at CMOTStartTime (ticks after the Q-switch).
    public class CompressedMOT : MOTMasterScriptSnippet
    {
        public CompressedMOT(PatternBuilder32 p, Dictionary<String, Object> parameters)
        {
            AddDigitalSnippet(p, parameters);
        }
        public CompressedMOT(AnalogPatternBuilder p, Dictionary<String, Object> parameters)
        {
            AddAnalogSnippet(p, parameters);
        }
        public CompressedMOT(DDSPatternBuilder p, Dictionary<String, Object> parameters)
        {
            AddDDSSnippet(p, parameters);
        }

        public void AddDigitalSnippet(PatternBuilder32 p, Dictionary<String, Object> parameters)
        {
        }

        public void AddAnalogSnippet(AnalogPatternBuilder p, Dictionary<String, Object> parameters)
        {
            p.AddLinearRamp("MOTCoilsCurrent", (int)parameters["CMOTStartTime"], (int)parameters["CompressRampDownDuration"], (double)parameters["MOTCoilsCompressionValue"]);
        }

        public void AddDDSSnippet(DDSPatternBuilder p, Dictionary<String, Object> parameters)
        {
            int start = (int)parameters["CMOTStartTime"];
            p.AddEvent("RampStart", start, CaFDDS.MOTFrequencies(parameters), CaFDDS.MOTAmplitudes(parameters),
                null, CaFDDS.FourChannels(parameters, "RampAmplitudeDDS"));
            p.AddEvent("RampEnd", start + (int)parameters["CompressRampDownDuration"],
                CaFDDS.MOTFrequencies(parameters), CaFDDS.FourChannels(parameters, "RampEndAmpDDS"));
        }
    }
}
