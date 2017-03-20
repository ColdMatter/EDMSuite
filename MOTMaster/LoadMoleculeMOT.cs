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
    public class LoadMoleculeMOT : MOTMasterScriptSnippet
    {
        public LoadMoleculeMOT(PatternBuilder32 p, Dictionary<String, Object> parameters)
        {
            AddDigitalSnippet(p, parameters);
        }
        public LoadMoleculeMOT(AnalogPatternBuilder p, Dictionary<String, Object> parameters)
        {
            AddAnalogSnippet(p, parameters);
        }


        public void AddDigitalSnippet(PatternBuilder32 p, Dictionary<String, Object> parameters)
        {
            int patternStartBeforeQ = (int)parameters["TCLBlockStart"];
            p.Pulse(0, 0, (int)parameters["TCLBlockDuration"], "tclBlock");
            p.Pulse(patternStartBeforeQ, -(int)parameters["FlashToQ"], (int)parameters["QSwitchPulseDuration"], "flash"); //trigger the flashlamp
            p.Pulse(patternStartBeforeQ, 0, 10, "AnalogPatternTrigger");  //THIS TRIGGERS THE ANALOG PATTERN. The analog pattern will start at the same time as the Q-switch is fired.
            p.Pulse(patternStartBeforeQ, 0, (int)parameters["QSwitchPulseDuration"], "q"); //trigger the Q switch
            p.Pulse(patternStartBeforeQ, (int)parameters["slowingAOMOnStart"], (int)parameters["slowingAOMOffStart"] - (int)parameters["slowingAOMOnStart"], "aom"); //first pulse to slowing AOM
            p.AddEdge("aom", patternStartBeforeQ + (int)parameters["slowingAOMOffStart"] + (int)parameters["slowingAOMOffDuration"], true); // send slowing aom high and hold it high
           // p.Pulse(patternStartBeforeQ, (int)parameters["slowingAOMOffStart"] + (int)parameters["slowingAOMOffDuration"], (int)parameters["slowingAOMOnDuration"] - ((int)parameters["slowingAOMOffStart"] - (int)parameters["slowingAOMOnStart"]) - (int)parameters["slowingAOMOffDuration"], "aom"); //second pulse to slowing AOM
            p.Pulse(patternStartBeforeQ, (int)parameters["slowingRepumpAOMOnStart"], (int)parameters["slowingRepumpAOMOffStart"] - (int)parameters["slowingRepumpAOMOnStart"], "aom2"); //first pulse to slowing repump AOM
            p.AddEdge("aom2", patternStartBeforeQ + (int)parameters["slowingRepumpAOMOffStart"] + (int)parameters["slowingRepumpAOMOffDuration"], true); // send slowing repump aom high and hold it high
            //    p.Pulse(patternStartBeforeQ, (int)Parameters["slowingRepumpAOMOffStart"] + (int)Parameters["slowingRepumpAOMOffDuration"], (int)Parameters["slowingRepumpAOMOnDuration"] - ((int)Parameters["slowingRepumpAOMOffStart"] - (int)Parameters["slowingRepumpAOMOnStart"]) - (int)Parameters["slowingRepumpAOMOffDuration"], "aom2"); //second pulse to slowing repump AOM
            p.Pulse(patternStartBeforeQ, (int)parameters["PMTTrigger"], (int)parameters["PMTTriggerDuration"], "detector"); // trigger data acquistion from PMT
        }

        public void AddAnalogSnippet(AnalogPatternBuilder p, Dictionary<String, Object> parameters)
        {
            p.AddChannel("slowingChirp");
            p.AddChannel("MOTCoilsCurrent");

            // Slowing Chirp
            p.AddAnalogValue("slowingChirp", 0, (double)parameters["SlowingChirpStartValue"]);
            p.AddLinearRamp("slowingChirp", (int)parameters["SlowingChirpStartTime"], (int)parameters["SlowingChirpDuration"], (double)parameters["SlowingChirpEndValue"]);
            p.AddLinearRamp("slowingChirp", (int)parameters["SlowingChirpStartTime"] + (int)parameters["SlowingChirpDuration"]+200, (int)parameters["SlowingChirpDuration"], (double)parameters["SlowingChirpStartValue"]);

           
        

        }
    }
}
