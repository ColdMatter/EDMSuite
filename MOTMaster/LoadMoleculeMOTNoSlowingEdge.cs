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
            p.Pulse(0, 0, (int)parameters["PatternLength"] - 100, "flowEnable");

            int patternStartBeforeQ = (int)parameters["TCLBlockStart"];
            p.Pulse(patternStartBeforeQ, -(int)parameters["FlashToQ"], (int)parameters["QSwitchPulseDuration"], "flashLamp"); //trigger the flashlamp
            p.Pulse((int)parameters["TCLBlockStart"], 0, 10, "aoPatternTrigger");  //THIS TRIGGERS THE ANALOG PATTERN. The analog pattern will start at the same time as the Q-switch is fired.
            p.Pulse(patternStartBeforeQ, 0, (int)parameters["QSwitchPulseDuration"], "qSwitch"); //trigger the Q switch
            p.Pulse(patternStartBeforeQ, -(int)parameters["HeliumShutterToQ"], (int)parameters["HeliumShutterDuration"], "heliumShutter");
        }

        public void AddAnalogSnippet(AnalogPatternBuilder p, Dictionary<String, Object> parameters)
        {
            int patternStartBeforeQ = (int)parameters["TCLBlockStart"];
            
            p.AddChannel("slowingChirp");
            p.AddChannel("slowingCoilsCurrent");
            p.AddChannel("MOTCoilsCurrent");

            // Slowing Chirp
            p.AddAnalogValue("slowingChirp", 0, (double)parameters["SlowingChirpStartValue"]);
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
            p.AddLinearRamp("slowingChirp", (int)parameters["SlowingChirpStartTime"] + (int)parameters["SlowingChirpDuration"] + 10000, (int)parameters["SlowingChirpDuration"], (double)parameters["SlowingChirpStartValue"]);

        }
    }
}
