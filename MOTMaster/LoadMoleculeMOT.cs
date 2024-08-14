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
            // The (new) digital pattern card on PXI Chassis is now the master card. The following pulse triggers the (old) pattern card on PCI slot.
            p.Pulse(0, 0, 10, "slavePatternCardTrigger");
            p.Pulse(0, 0, (int)parameters["PatternLength"] - 100, "flowEnable");

            int patternStartBeforeQ = (int)parameters["TCLBlockStart"];
            //p.AddTrigger("digitalPattern2", patternStartBeforeQ, -patternStartBeforeQ, 10, "patternBoard2Trigger");
            p.Pulse(0, (int)parameters["SlowingChirpStartTime"], (2 * (int)parameters["SlowingChirpDuration"]) + 20000, "bXLockBlock"); // Want it to be blocked for whole time that bX laser is moved
            p.Pulse(patternStartBeforeQ, -(int)parameters["FlashToQ"], (int)parameters["QSwitchPulseDuration"], "flashLamp"); //trigger the flashlamp
            p.Pulse(patternStartBeforeQ, 0, 10, "aoPatternTrigger");  //THIS TRIGGERS THE ANALOG PATTERN. The analog pattern will start at the same time as the Q-switch is fired.//trigger the Q switch !!!
            p.Pulse(patternStartBeforeQ, 0, (int)parameters["QSwitchPulseDuration"], "qSwitch"); 
            p.Pulse(patternStartBeforeQ, -(int)parameters["HeliumShutterToQ"], (int)parameters["HeliumShutterDuration"], "heliumShutter");
            p.Pulse(patternStartBeforeQ, (int)parameters["slowingAOMOnStart"], (int)parameters["slowingAOMOffStart"] - (int)parameters["slowingAOMOnStart"], "bXSlowingAOM"); //first pulse to slowing AOM
            //p.AddEdge("bXSlowingAOM", patternStartBeforeQ + (int)parameters["slowingAOMOffStart"] + (int)parameters["slowingAOMOffDuration"], true); // send slowing aom high and hold it high
            p.Pulse(patternStartBeforeQ, (int)parameters["slowingRepumpAOMOnStart"], (int)parameters["slowingRepumpAOMOffStart"] - (int)parameters["slowingRepumpAOMOnStart"], "v10SlowingAOM"); //first pulse to slowing repump AOM
            //p.AddEdge("v10SlowingAOM", patternStartBeforeQ + (int)parameters["slowingRepumpAOMOffStart"] + (int)parameters["slowingRepumpAOMOffDuration"], true); // send slowing repump aom high and hold it high
            //p.Pulse(patternStartBeforeQ, (int)parameters["PMTTrigger"], (int)parameters["PMTTriggerDuration"], "detector"); // trigger data acquistion from PMT
        }

        public void AddAnalogSnippet(AnalogPatternBuilder p, Dictionary<String, Object> parameters)
        {
            p.AddChannel("slowingChirp");
            p.AddChannel("slowingCoilsCurrent");
            p.AddChannel("MOTCoilsCurrent");
            //p.AddChannel("newAnalogTest");

            // Slowing Chirp
            p.AddAnalogValue("slowingChirp", 0, (double)parameters["SlowingChirpStartValue"]);
            //p.AddLinearRamp("slowingChirp", (int)parameters["SlowingChirpStartTime"], (int)parameters["SlowingChirpDuration"], (double)parameters["SlowingChirpEndValue"]);
            
            
            p.AddPolynomialRamp("slowingChirp",
            (int)parameters["SlowingChirpStartTime"],
            (int)parameters["SlowingChirpStartTime"] + (int)parameters["SlowingChirpDuration"],
            (double)parameters["SlowingChirpEndValue"],
            1.0,                    // Parameters["SlowingChirpUpperThreshold"]
            -1.5,                   // Parameters["SlowingChirpLowerThreshold"]
            1.0,                    // (double)parameters["weight1"],
            -0.5,                   // (double)parameters["weight2"],
            1.0/6.0,                // (double)parameters["weight3"],
            -1.0 / 24.0);           // (double)parameters["weight4"]
            

            //p.AddLinearRamp("slowingChirp", (int)parameters["SlowingChirpStartTime"], (int)parameters["SlowingChirpDuration"], (double)parameters["SlowingChirpEndValue"]);

            p.AddLinearRamp("slowingChirp", (int)parameters["SlowingChirpStartTime"] + (int)parameters["SlowingChirpDuration"] + 2500, (int)parameters["SlowingChirpDuration"], (double)parameters["SlowingChirpStartValue"]);
            
            
            /*
            addSigmoidRamp(p, "slowingChirp", (int)parameters["SlowingChirpStartTime"], (int)parameters["SlowingChirpDuration"],
                (double)parameters["SlowingChirpStartValue"], (double)parameters["SlowingChirpEndValue"]);
            addSigmoidRamp(p, "slowingChirp", (int)parameters["SlowingChirpStartTime"] + (int)parameters["SlowingChirpDuration"] + 200, 
                (int)parameters["SlowingChirpDuration"],
                (double)parameters["SlowingChirpEndValue"], (double)parameters["SlowingChirpStartValue"]);
            */
        }

        public void addSigmoidRamp(AnalogPatternBuilder p, string channel, int startTime, int rampDuration, double rampStartValue, double rampEndValue)
        {
            for (int t = startTime; t < rampDuration + startTime; t++)
            {
                p.AddAnalogValue(channel, t, sigmoid(t - startTime, rampDuration, rampStartValue, rampEndValue));
            }
        }

        public double sigmoid(int t, int tDuration, double startValue, double endValue)
        {
            return startValue + (endValue - startValue) / (1 + Math.Exp( - 8.0 * (t - tDuration / 2) / tDuration));
        }
    }
}
