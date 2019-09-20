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
            p.AddEdge("aom0enable", (int)parameters["MOTStartTime"], true);
            p.AddEdge("aom1enable", (int)parameters["MOTStartTime"], true);
            p.AddEdge("aom2enable", (int)parameters["MOTStartTime"], true);
            p.AddEdge("aom3enable", (int)parameters["MOTStartTime"], true);
        }

        public void AddAnalogSnippet(AnalogPatternBuilder p, Dictionary<String, Object> parameters)
        {
            p.AddChannel("coil0current");

            p.AddAnalogValue("coil0current", (int)parameters["MOTStartTime"], (double)parameters["MOTCoilsCurrent"]);

        }

       
    }
}
