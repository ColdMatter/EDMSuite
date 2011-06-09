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


        public void AddDigitalSnippet(PatternBuilder32 p, Dictionary<String, Object> parameters)
        {
            p.Pulse(2, 0, 1, "CameraTrigger");
        }

        public void AddAnalogSnippet(AnalogPatternBuilder p, Dictionary<String, Object> parameters)
        {
            p.AddAnalogPulse("laser", 1, 2, 4, 2);
            p.AddAnalogValue("laser", 5, -2);

            p.AddAnalogValue("cavity", 0, 4);
            p.AddAnalogValue("cavity", 1, 2);
            p.AddAnalogValue("cavity", 3, 4);
            p.AddAnalogValue("cavity", 4, 0);
            p.AddLinearRamp("cavity", (int)parameters["MOTLoadTime"], 5, 1);
        }

       
    }
}
