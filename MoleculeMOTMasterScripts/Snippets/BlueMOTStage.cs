using System;
using System.Collections.Generic;
using DAQ.Pattern;
using DAQ.Analog;
using MOTMaster;
using MOTMaster.SnippetLibrary;

namespace MOTMaster.SnippetLibrary.CaF
{
    // Blue MOT: ramp a pair of coils to a field over BlueMOTRampDuration and switch the DDS
    // to the blue-MOT tones. Leaves the coils on: whatever comes next sets them.
    // Shared by BlueMOT, BlueMOT2 and TweezerBlueMOT, which choose the stage name, the coils
    // and the field parameter. The stage starts at <stage>StartTime (ticks after the
    // Q-switch) and its DDS event is called <stage>RampStart.
    public abstract class BlueMOTStage : MOTMasterScriptSnippet
    {
        private readonly string stage;
        private readonly string coils;
        private readonly string field;

        protected BlueMOTStage(string stage, string coils, string field, PatternBuilder32 p, Dictionary<String, Object> parameters)
        {
            this.stage = stage; this.coils = coils; this.field = field;
            AddDigitalSnippet(p, parameters);
        }
        protected BlueMOTStage(string stage, string coils, string field, AnalogPatternBuilder p, Dictionary<String, Object> parameters)
        {
            this.stage = stage; this.coils = coils; this.field = field;
            AddAnalogSnippet(p, parameters);
        }
        protected BlueMOTStage(string stage, string coils, string field, DDSPatternBuilder p, Dictionary<String, Object> parameters)
        {
            this.stage = stage; this.coils = coils; this.field = field;
            AddDDSSnippet(p, parameters);
        }

        public void AddDigitalSnippet(PatternBuilder32 p, Dictionary<String, Object> parameters)
        {
        }

        public void AddAnalogSnippet(AnalogPatternBuilder p, Dictionary<String, Object> parameters)
        {
            p.AddChannel(coils);
            p.AddLinearRamp(coils, (int)parameters[stage + "StartTime"], (int)parameters["BlueMOTRampDuration"], (double)parameters[field]);
        }

        public void AddDDSSnippet(DDSPatternBuilder p, Dictionary<String, Object> parameters)
        {
            p.AddEvent(stage + "RampStart", (int)parameters[stage + "StartTime"],
                CaFDDS.BlueMOTFrequencies(parameters), CaFDDS.BlueMOTAmplitudes(parameters));
        }
    }
}
