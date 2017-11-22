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
    public class LoadMoleculeMOTNoSlowingEdge : MOTMasterScriptSnippet
    {
        public LoadMoleculeMOTNoSlowingEdge(PatternBuilder32 p, Dictionary<String, Object> parameters)
        {
            AddDigitalSnippet(p, parameters);
        }
        public LoadMoleculeMOTNoSlowingEdge(AnalogPatternBuilder p, Dictionary<String, Object> parameters)
        {
            AddAnalogSnippet(p, parameters);
        }


        public void AddDigitalSnippet(PatternBuilder32 p, Dictionary<String, Object> parameters)
        {
            int patternStartBeforeQ = (int)parameters["TCLBlockStart"];
            p.Pulse(0, 0, (int)parameters["TCLBlockDuration"], "tclBlock");
            p.Pulse(patternStartBeforeQ, -(int)parameters["FlashToQ"], (int)parameters["QSwitchPulseDuration"], "flashLamp"); //trigger the flashlamp
            p.Pulse(patternStartBeforeQ, 0, 10, "aoPatternTrigger");  //THIS TRIGGERS THE ANALOG PATTERN. The analog pattern will start at the same time as the Q-switch is fired.
            p.Pulse(patternStartBeforeQ, 0, (int)parameters["QSwitchPulseDuration"], "qSwitch"); //trigger the Q switch
            p.Pulse(patternStartBeforeQ, (int)parameters["slowingAOMOnStart"], (int)parameters["slowingAOMOffStart"] - (int)parameters["slowingAOMOnStart"], "bXSlowingAOM"); //first pulse to slowing AOM
           // p.AddEdge("aom", patternStartBeforeQ + (int)parameters["slowingAOMOffStart"] + (int)parameters["slowingAOMOffDuration"], true); // send slowing aom high and hold it high
           // p.Pulse(patternStartBeforeQ, (int)parameters["slowingAOMOffStart"] + (int)parameters["slowingAOMOffDuration"], (int)parameters["slowingAOMOnDuration"] - ((int)parameters["slowingAOMOffStart"] - (int)parameters["slowingAOMOnStart"]) - (int)parameters["slowingAOMOffDuration"], "aom"); //second pulse to slowing AOM
            p.Pulse(patternStartBeforeQ, (int)parameters["slowingRepumpAOMOnStart"], (int)parameters["slowingRepumpAOMOffStart"] - (int)parameters["slowingRepumpAOMOnStart"], "v10SlowingAOM"); //first pulse to slowing repump AOM
            p.AddEdge("v10SlowingAOM", patternStartBeforeQ + (int)parameters["slowingRepumpAOMOffStart"] + (int)parameters["slowingRepumpAOMOffDuration"], true); // send slowing repump aom high and hold it high
            //    p.Pulse(patternStartBeforeQ, (int)Parameters["slowingRepumpAOMOffStart"] + (int)Parameters["slowingRepumpAOMOffDuration"], (int)Parameters["slowingRepumpAOMOnDuration"] - ((int)Parameters["slowingRepumpAOMOffStart"] - (int)Parameters["slowingRepumpAOMOnStart"]) - (int)Parameters["slowingRepumpAOMOffDuration"], "aom2"); //second pulse to slowing repump AOM
            
        }

        public void AddAnalogSnippet(AnalogPatternBuilder p, Dictionary<String, Object> parameters)
        {
            p.AddChannel("slowingChirp");
            p.AddChannel("MOTCoilsCurrent");
            p.AddChannel("slowingCoilsCurrent");

            // Slowing Chirp
            p.AddAnalogValue("slowingChirp", 0, (double)parameters["SlowingChirpStartValue"]);
            p.AddLinearRamp("slowingChirp", (int)parameters["SlowingChirpStartTime"], (int)parameters["SlowingChirpDuration"], (double)parameters["SlowingChirpEndValue"]);
            p.AddLinearRamp("slowingChirp", (int)parameters["SlowingChirpStartTime"] + (int)parameters["SlowingChirpDuration"], 1000, (double)parameters["PokeDetuningValue"]);
            //p.AddAnalogValue("slowingChirp", 5000, (double)parameters["PokeDetuningValue"]);
            p.AddLinearRamp("slowingChirp", 1500, (int)parameters["SlowingChirpDuration"], (double)parameters["SlowingChirpStartValue"]);

           
        

        }
    }
}
