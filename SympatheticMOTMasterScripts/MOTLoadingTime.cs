using MOTMaster;
using MOTMaster.SnippetLibrary;

using System;
using System.Collections.Generic;

using DAQ.Pattern;
using DAQ.Analog;


    public class Patterns : MOTMasterScript
    {


        public Patterns()
        {
            Parameters = new Dictionary<string, object>();
            Parameters["PatternLength"] = 170000;
            Parameters["MOTStartTime"] = 0;
            Parameters["MOTCoilsCurrent"] = 17.0;
            Parameters["NumberOfFrames"] = 6;
            Parameters["Frame0TriggerDuration"] = 100;
            Parameters["Frame0Trigger"] = 5000;
            Parameters["Frame1TriggerDuration"] = 100;
            Parameters["Frame1Trigger"] = 125000;
        }

        public override PatternBuilder32 GetDigitalPattern()
        {
            PatternBuilder32 p = new PatternBuilder32();

            //MOTMasterScriptSnippet lm = new SHLoadMOT(p, Parameters);  // This is how you load "preset" patterns.

            p.Pulse(0, 0, 1, "AnalogPatternTrigger");  //NEVER CHANGE THIS!!!! IT TRIGGERS THE ANALOG PATTERN!

            p.AddEdge("aom0enable", (int)Parameters["MOTStartTime"], true);
            p.AddEdge("aom1enable", (int)Parameters["MOTStartTime"], true);
            p.AddEdge("aom2enable", (int)Parameters["MOTStartTime"], true);
            p.AddEdge("aom3enable", (int)Parameters["MOTStartTime"], true);
            p.AddEdge("CameraTrigger", 0, true);
            p.DownPulse((int)Parameters["Frame0Trigger"], 0, (int)Parameters["Frame0TriggerDuration"], "CameraTrigger");
            p.DownPulse((int)Parameters["Frame1Trigger"], 0, (int)Parameters["Frame1TriggerDuration"], "CameraTrigger");

            p.DownPulse(10000, 0, 50, "CameraTrigger");
            p.DownPulse(50000, 0, 50, "CameraTrigger");
            p.DownPulse(150000, 0, 50, "CameraTrigger");
            p.DownPulse(160000, 0, 50, "CameraTrigger");

            return p;
        }

        public override AnalogPatternBuilder GetAnalogPattern()
        {
            AnalogPatternBuilder p = new AnalogPatternBuilder((int)Parameters["PatternLength"]);

            p.AddChannel("coil0current");

            p.AddAnalogValue("coil0current", (int)Parameters["MOTStartTime"], (double)Parameters["MOTCoilsCurrent"]);
            //MOTMasterScriptSnippet lm = new SHLoadMOT(p, Parameters);

            //p.AddAnalogValue("coil0current", 120000, 0);
            return p;
        }

    }
