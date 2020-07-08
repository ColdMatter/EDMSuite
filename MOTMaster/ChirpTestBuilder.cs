using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DAQ.Pattern;
using DAQ.Analog;
using DAQ;

namespace MOTMaster.SnippetLibrary
{
    public class ChirpTest: MOTMasterScriptSnippet
    {
        public ChirpTest(PatternBuilder32 p, Dictionary<String, Object> parameters)
        {
            AddDigitalSnippet(p, parameters);
        }
        public ChirpTest(AnalogPatternBuilder p, Dictionary<String, Object> parameters)
        {
            AddAnalogSnippet(p, parameters);
        }


        public void AddDigitalSnippet(PatternBuilder32 p, Dictionary<String, Object> parameters)
        {
            int patternStartBeforeQ = (int)parameters["TCLBlockStart"];
            p.Pulse(patternStartBeforeQ, (int)parameters["SlowingChirpStartTime"], (2 * (int)parameters["SlowingChirpDuration"]) + 200, "bXLockBlock"); // Want it to be blocked for whole time that bX laser is moved
           
 
        }

        public void AddAnalogSnippet(AnalogPatternBuilder p, Dictionary<String, Object> parameters)
        {
            p.AddChannel("slowingChirp");
            p.AddChannel("slowingCoilsCurrent");
            p.AddChannel("MOTCoilsCurrent");

            // Slowing Chirp
            p.AddAnalogValue("slowingChirp", 0, (double)parameters["SlowingChirpStartValue"]);
            p.AddLinearRamp("slowingChirp", (int)parameters["SlowingChirpStartTime"], (int)parameters["SlowingChirpDuration"], (double)parameters["SlowingChirpEndValue"]);
            p.AddLinearRamp("slowingChirp", (int)parameters["SlowingChirpStartTime"] + (int)parameters["SlowingChirpDuration"] + 200, (int)parameters["SlowingChirpDuration"], (double)parameters["SlowingChirpStartValue"]);
            

        }
    }
}
