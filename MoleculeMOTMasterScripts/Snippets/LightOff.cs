using System;
using System.Collections.Generic;
using DAQ.Pattern;
using DAQ.Analog;
using MOTMaster;
using MOTMaster.SnippetLibrary;

namespace MOTMaster.SnippetLibrary.CaF
{
    // Switch the cooling light off: DDS to the resonance frequencies at LightoffDDS1 on all
    // four channels, e.g. for a magtrap or a free fall.
    // Shared by LightOff1 and LightOff2, which differ only in their stage name: the light goes
    // off at <stage>StartTime (ticks after the Q-switch) and the DDS event is called <stage>.
    public abstract class LightOff : MOTMasterScriptSnippet
    {
        private readonly string stage;

        protected LightOff(string stage, PatternBuilder32 p, Dictionary<String, Object> parameters)
        {
            this.stage = stage;
            AddDigitalSnippet(p, parameters);
        }
        protected LightOff(string stage, AnalogPatternBuilder p, Dictionary<String, Object> parameters)
        {
            this.stage = stage;
            AddAnalogSnippet(p, parameters);
        }
        protected LightOff(string stage, DDSPatternBuilder p, Dictionary<String, Object> parameters)
        {
            this.stage = stage;
            AddDDSSnippet(p, parameters);
        }

        public void AddDigitalSnippet(PatternBuilder32 p, Dictionary<String, Object> parameters)
        {
        }

        public void AddAnalogSnippet(AnalogPatternBuilder p, Dictionary<String, Object> parameters)
        {
        }

        public void AddDDSSnippet(DDSPatternBuilder p, Dictionary<String, Object> parameters)
        {
            p.AddEvent(stage, (int)parameters[stage + "StartTime"],
                CaFDDS.ResonanceFrequencies(parameters), CaFDDS.Same(parameters, "LightoffDDS1"));
        }
    }
}
