using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DAQ.Pattern;
using DAQ.Analog;
using DAQ;

namespace MOTMaster.SnippetLibrary
{
    public class SHLoadMOT : MOTMasterScriptSnippet
    {
        public SHLoadMOT(PatternBuilder32 p, Dictionary<String, Object> parameters)
        {
            AddDigitalSnippet(p, parameters);
        }
        public SHLoadMOT(AnalogPatternBuilder p, Dictionary<String, Object> parameters)
        {
            AddAnalogSnippet(p, parameters);
        }


        public override void AddDigitalSnippet(PatternBuilder32 p, Dictionary<String, Object> parameters)
        {
            p.Pulse(2, 0, 1, "CameraTrigger");
        }

        public override void AddAnalogSnippet(AnalogPatternBuilder p, Dictionary<String, Object> parameters)
        {
            p.AddAnalogPulse("laser", 1, 2, 4, 2);
            p.AddAnalogValue("laser", 5, -2);
        }
    }
}
