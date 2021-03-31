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
            //p.AddTrigger("digitalPattern2", patternStartBeforeQ, -patternStartBeforeQ, 10, "patternBoard2Trigger");
            p.Pulse(patternStartBeforeQ, (int)parameters["SlowingChirpStartTime"], (2 * (int)parameters["SlowingChirpDuration"]) + 200, "bXLockBlock"); // Want it to be blocked for whole time that bX laser is moved
            p.Pulse(patternStartBeforeQ, -(int)parameters["FlashToQ"], (int)parameters["QSwitchPulseDuration"], "flashLamp"); //trigger the flashlamp
            p.Pulse(patternStartBeforeQ, 0, 10, "aoPatternTrigger");  //THIS TRIGGERS THE ANALOG PATTERN. The analog pattern will start at the same time as the Q-switch is fired.//trigger the Q switch !!!
            p.Pulse(patternStartBeforeQ, 0, (int)parameters["QSwitchPulseDuration"], "qSwitch"); 
            p.Pulse(patternStartBeforeQ, -(int)parameters["HeliumShutterToQ"], (int)parameters["HeliumShutterDuration"], "heliumShutter");
            p.Pulse(patternStartBeforeQ, (int)parameters["slowingAOMOnStart"], (int)parameters["slowingAOMOffStart"] - (int)parameters["slowingAOMOnStart"], "bXSlowingAOM"); //first pulse to slowing AOM
            p.AddEdge("bXSlowingAOM", patternStartBeforeQ + (int)parameters["slowingAOMOffStart"] + (int)parameters["slowingAOMOffDuration"], true); // send slowing aom high and hold it high
            p.Pulse(patternStartBeforeQ, (int)parameters["slowingRepumpAOMOnStart"], (int)parameters["slowingRepumpAOMOffStart"] - (int)parameters["slowingRepumpAOMOnStart"], "v10SlowingAOM"); //first pulse to slowing repump AOM
            p.AddEdge("v10SlowingAOM", patternStartBeforeQ + (int)parameters["slowingRepumpAOMOffStart"] + (int)parameters["slowingRepumpAOMOffDuration"], true); // send slowing repump aom high and hold it high
            //p.Pulse(patternStartBeforeQ, (int)parameters["PMTTrigger"], (int)parameters["PMTTriggerDuration"], "detector"); // trigger data acquistion from PMT
        }

        public void AddAnalogSnippet(AnalogPatternBuilder p, Dictionary<String, Object> parameters)
        {
            p.AddChannel("slowingChirp");
            p.AddChannel("slowingCoilsCurrent");
            p.AddChannel("MOTCoilsCurrent");

            // Slowing Chirp
            p.AddAnalogValue("slowingChirp", 0, (double)parameters["SlowingChirpStartValue"]);
            //p.AddLinearRamp("slowingChirp", (int)parameters["SlowingChirpStartTime"], (int)parameters["SlowingChirpDuration"], (double)parameters["SlowingChirpEndValue"]);
            p.AddPolynomialRamp("slowingChirp",
            (int)parameters["SlowingChirpStartTime"],
            (int)parameters["SlowingChirpStartTime"] + (int)parameters["SlowingChirpDuration"],
            (double)parameters["SlowingChirpEndValue"],
            1.0,                    // Parameters["SlowingChirpUpperThreshold"]
            -1.5,                   // Parameters["SlowingChirpLowerThreshold"]
            1.0,                    // Parameters["weight1"]
            -0.5,                   // Parameters["weight2"]
            1.0 / 6.0,              // Parameters["weight3"]
            -1.0 / 24.0);           // Parameters["weight4"]
            p.AddLinearRamp("slowingChirp", (int)parameters["SlowingChirpStartTime"] + (int)parameters["SlowingChirpDuration"] + 200, (int)parameters["SlowingChirpDuration"], (double)parameters["SlowingChirpStartValue"]);
            

        }
    }
}
