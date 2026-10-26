using System;
using System.Collections.Generic;
using DAQ.Pattern;
using DAQ.Analog;
using MOTMaster;
using MOTMaster.SnippetLibrary;

namespace MOTMaster.SnippetLibrary.CaF
{
    // Optical pumping: the QCL shutter is held open (high) from the start of the shot and
    // closes for OpticalPumpingDuration from OPStartTime (ticks after the Q-switch).
    public class OpticalPumping : MOTMasterScriptSnippet
    {
        public OpticalPumping(PatternBuilder32 p, Dictionary<String, Object> parameters)
        {
            AddDigitalSnippet(p, parameters);
        }
        public OpticalPumping(AnalogPatternBuilder p, Dictionary<String, Object> parameters)
        {
            AddAnalogSnippet(p, parameters);
        }
        public OpticalPumping(DDSPatternBuilder p, Dictionary<String, Object> parameters)
        {
            AddDDSSnippet(p, parameters);
        }

        public void AddDigitalSnippet(PatternBuilder32 p, Dictionary<String, Object> parameters)
        {
            p.AddEdge("QCLShutter", 0, true);
            p.DownPulse((int)parameters["TCLBlockStart"] + (int)parameters["OPStartTime"], 0, (int)parameters["OpticalPumpingDuration"], "QCLShutter");
        }

        public void AddAnalogSnippet(AnalogPatternBuilder p, Dictionary<String, Object> parameters)
        {
        }

        public void AddDDSSnippet(DDSPatternBuilder p, Dictionary<String, Object> parameters)
        {
        }
    }
}
