using System;
using System.Collections.Generic;
using DAQ.Pattern;
using DAQ.Analog;
using MOTMaster;
using MOTMaster.SnippetLibrary;

namespace MOTMaster.SnippetLibrary.CaF
{
    // Lambda-enhanced grey molasses: MOT coils off, DDS to the lambda tones.
    // Shared by LambdaCooling1 and LambdaCooling2, which differ only in their stage name:
    // the stage starts at <stage>StartTime (ticks after the Q-switch) and its DDS event is
    // called <stage>.
    public abstract class LambdaCooling : MOTMasterScriptSnippet
    {
        private readonly string stage;

        protected LambdaCooling(string stage, PatternBuilder32 p, Dictionary<String, Object> parameters)
        {
            this.stage = stage;
            AddDigitalSnippet(p, parameters);
        }
        protected LambdaCooling(string stage, AnalogPatternBuilder p, Dictionary<String, Object> parameters)
        {
            this.stage = stage;
            AddAnalogSnippet(p, parameters);
        }
        protected LambdaCooling(string stage, DDSPatternBuilder p, Dictionary<String, Object> parameters)
        {
            this.stage = stage;
            AddDDSSnippet(p, parameters);
        }

        public void AddDigitalSnippet(PatternBuilder32 p, Dictionary<String, Object> parameters)
        {
        }

        public void AddAnalogSnippet(AnalogPatternBuilder p, Dictionary<String, Object> parameters)
        {
            p.AddAnalogValue("MOTCoilsCurrent", (int)parameters[stage + "StartTime"], (double)parameters["MOTCoilsOffValue"]);
        }

        public void AddDDSSnippet(DDSPatternBuilder p, Dictionary<String, Object> parameters)
        {
            p.AddEvent(stage, (int)parameters[stage + "StartTime"],
                CaFDDS.LambdaFrequencies(parameters), CaFDDS.LambdaAmplitudes(parameters));
        }
    }
}
