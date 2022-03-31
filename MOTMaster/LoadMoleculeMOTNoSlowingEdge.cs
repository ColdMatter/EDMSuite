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
            // The (new) digital pattern card on PXI Chassis is now the master card. The following pulse triggers the (old) pattern card on PCI slot.
            p.Pulse(0, 0, 10, "slavePatternCardTrigger"); 

            int patternStartBeforeQ = (int)parameters["TCLBlockStart"] + (int)parameters["RbMOTLoadTime"];
            int slowingChirpStartTime = (int)parameters["SlowingChirpStartTime"];
            int slowingNewDetuningTime = slowingChirpStartTime + (int)parameters["SlowingChirpDuration"];
            int slowingChirpBackTime = slowingNewDetuningTime + (int)parameters["SlowingChirpHoldDuration"];
            int slowingChirpFinishedTime = slowingChirpBackTime + (int)parameters["SlowingChirpDuration"];

            p.Pulse(patternStartBeforeQ, slowingChirpStartTime, slowingChirpFinishedTime - slowingChirpStartTime, "bXLockBlock"); ; // Want it to be blocked for whole time that bX laser is moved
            p.Pulse(patternStartBeforeQ, -(int)parameters["FlashToQ"], (int)parameters["QSwitchPulseDuration"], "flashLamp"); //trigger the flashlamp
            p.Pulse((int)parameters["TCLBlockStart"], 0, 10, "aoPatternTrigger");  //THIS TRIGGERS THE ANALOG PATTERN. The analog pattern will start at the same time as the Q-switch is fired.
            p.Pulse(patternStartBeforeQ, 0, (int)parameters["QSwitchPulseDuration"], "qSwitch"); //trigger the Q switch
            p.Pulse(patternStartBeforeQ, -(int)parameters["HeliumShutterToQ"], (int)parameters["HeliumShutterDuration"], "heliumShutter");
            p.Pulse(patternStartBeforeQ, (int)parameters["slowingAOMOnStart"], (int)parameters["slowingAOMOffStart"] - (int)parameters["slowingAOMOnStart"], "bXSlowingAOM"); //first pulse to slowing AOM
            p.Pulse(patternStartBeforeQ, (int)parameters["slowingRepumpAOMOnStart"], (int)parameters["slowingRepumpAOMOffStart"] - (int)parameters["slowingRepumpAOMOnStart"], "v10SlowingAOM"); //first pulse to slowing repump AOM
            
        }

        public void AddAnalogSnippet(AnalogPatternBuilder p, Dictionary<String, Object> parameters)
        {
            int patternStartBeforeQ = (int)parameters["TCLBlockStart"];
            int slowingChirpStartTime = (int)parameters["SlowingChirpStartTime"] + (int)parameters["RbMOTLoadTime"];
            int slowingNewDetuningTime = slowingChirpStartTime + (int)parameters["SlowingChirpDuration"];
            int slowingChirpBackTime = slowingNewDetuningTime + (int)parameters["SlowingChirpHoldDuration"];
            int slowingChirpFinishedTime = slowingChirpBackTime + (int)parameters["SlowingChirpDuration"];

            p.AddChannel("slowingChirp");
            p.AddChannel("slowingCoilsCurrent");
            p.AddChannel("MOTCoilsCurrent");
            p.AddChannel("newAnalogTest");

            // Slowing Chirp
            p.AddAnalogValue("slowingChirp", 0, (double)parameters["SlowingChirpStartValue"]);
            p.AddPolynomialRamp("slowingChirp",
            slowingChirpStartTime,
            slowingChirpStartTime + (int)parameters["SlowingChirpDuration"],
            (double)parameters["SlowingChirpEndValue"],
            1.0,                    // Parameters["SlowingChirpUpperThreshold"]
            -1.5,                   // Parameters["SlowingChirpLowerThreshold"]
            1.0,                    // Parameters["weight1"]
            -0.5,                   // Parameters["weight2"]
            1.0 / 6.0,              // Parameters["weight3"]
            -1.0 / 24.0);           // Parameters["weight4"]
            p.AddLinearRamp("slowingChirp", slowingNewDetuningTime, 200, (double)parameters["PokeDetuningValue"]);
            p.AddLinearRamp("slowingChirp", slowingChirpBackTime, (int)parameters["SlowingChirpDuration"], (double)parameters["SlowingChirpStartValue"]);

        }
    }
}
