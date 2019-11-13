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
    public class LoadMoleculeMOTDualSpecies : MOTMasterScriptSnippet
    {
        public LoadMoleculeMOTDualSpecies(PatternBuilder32 p, Dictionary<String, Object> parameters)
        {
            AddDigitalSnippet(p, parameters);
        }
        public LoadMoleculeMOTDualSpecies(AnalogPatternBuilder p, Dictionary<String, Object> parameters)
        {
            AddAnalogSnippet(p, parameters);
        }


        public void AddDigitalSnippet(PatternBuilder32 p, Dictionary<String, Object> parameters)
        {
            int patternStartBeforeQ = (int)parameters["TCLBlockStart"] + (int)parameters["MOTLoadTime"] + (int)parameters["CMOTCoilsCurrentRampDuration"]; //!!!
            p.Pulse(patternStartBeforeQ, (int)parameters["SlowingChirpStartTime"], (2 * (int)parameters["SlowingChirpDuration"]) + 200, "bXLockBlock"); // Want it to be blocked for whole time that bX laser is moved
            p.Pulse(patternStartBeforeQ, -(int)parameters["FlashToQ"], (int)parameters["QSwitchPulseDuration"], "flashLamp"); //trigger the flashlamp
            p.Pulse((int)parameters["TCLBlockStart"], 0, 10, "aoPatternTrigger");  //THIS TRIGGERS THE ANALOG PATTERN. The analog pattern will start at the same time as the Q-switch is fired.//trigger the Q switch  !!!1st parameter was patternStartBeforeQ
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
            /*p.AddAnalogValue("slowingChirp", 0, (double)parameters["SlowingChirpStartValue"]);
            p.AddLinearRamp("slowingChirp", (int)parameters["SlowingChirpStartTime"], (int)parameters["SlowingChirpDuration"], (double)parameters["SlowingChirpEndValue"]);
            p.AddLinearRamp("slowingChirp", (int)parameters["SlowingChirpStartTime"] + (int)parameters["SlowingChirpDuration"] + 200, (int)parameters["SlowingChirpDuration"], (double)parameters["SlowingChirpStartValue"]);
            */

            // Slowing Chirp
            p.AddAnalogValue("slowingChirp", (int)parameters["MOTLoadTime"] + (int)parameters["CMOTCoilsCurrentRampDuration"], (double)parameters["SlowingChirpStartValue"]); //!!!
            p.AddLinearRamp("slowingChirp", (int)parameters["MOTLoadTime"] + (int)parameters["CMOTCoilsCurrentRampDuration"] + (int)parameters["SlowingChirpStartTime"], (int)parameters["SlowingChirpDuration"], (double)parameters["SlowingChirpEndValue"]); //!!!
            p.AddLinearRamp("slowingChirp", (int)parameters["MOTLoadTime"] + (int)parameters["CMOTCoilsCurrentRampDuration"] + (int)parameters["SlowingChirpStartTime"] + (int)parameters["SlowingChirpDuration"] + 200, (int)parameters["SlowingChirpDuration"], (double)parameters["SlowingChirpStartValue"]); //!!!

            //(int)parameters["MOTLoadTime"] + (int)parameters["CMOTCoilsCurrentRampDuration"]

        }
    }
}
