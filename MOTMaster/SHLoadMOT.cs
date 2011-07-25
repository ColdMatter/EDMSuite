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
            
        }

        public void AddAnalogSnippet(AnalogPatternBuilder p, Dictionary<String, Object> parameters)
        {
            p.AddChannel("coil0current");

            p.AddAnalogPulse("coil0current", 0, (int)parameters["MOTLoadDuration"], (double)parameters["MOTCoilsCurrent"], 0);
        }

       
    }
}
