using MOTMaster;
using MOTMaster.SnippetLibrary;

using System;
using System.Collections.Generic;

using DAQ.Pattern;
using DAQ.Analog;

// This script is used to measure the loading time of the MOT, it first dumps the MOT, then loads it and takes two images,
// one of the MOT and one background. The trigger for the MOT image can then be scanned using the scripting console.
    public class Patterns : MOTMasterScript
    {


        public Patterns()
        {
            Parameters = new Dictionary<string, object>();
            Parameters["PatternLength"] = 170000;
            Parameters["MOTStartTime"] = 1000;
            Parameters["MOTCoilsCurrent"] = 17.0;
            Parameters["NumberOfFrames"] = 2;
            Parameters["Frame0TriggerDuration"] = 100;
            Parameters["Frame0Trigger"] = 5000;
            Parameters["Frame1TriggerDuration"] = 100;
            Parameters["Frame1Trigger"] = 125000;
        }

        public override PatternBuilder32 GetDigitalPattern()
        {
            PatternBuilder32 p = new PatternBuilder32();

            MOTMasterScriptSnippet lm = new SHLoadMOT(p, Parameters);  // This is how you load "preset" patterns.

            p.Pulse(0, 0, 1, "AnalogPatternTrigger");  //NEVER CHANGE THIS!!!! IT TRIGGERS THE ANALOG PATTERN!

            p.AddEdge("CameraTrigger", 0, true);
            p.DownPulse((int)Parameters["Frame0Trigger"], 0, (int)Parameters["Frame0TriggerDuration"], "CameraTrigger");
            p.DownPulse((int)Parameters["Frame1Trigger"], 0, (int)Parameters["Frame1TriggerDuration"], "CameraTrigger");

            p.DownPulse(150000, 0, 50, "CameraTrigger");
            p.DownPulse(160000, 0, 50, "CameraTrigger");

            return p;
        }

        public override AnalogPatternBuilder GetAnalogPattern()
        {
            AnalogPatternBuilder p = new AnalogPatternBuilder((int)Parameters["PatternLength"]);

            MOTMasterScriptSnippet lm = new SHLoadMOT(p, Parameters);

            p.AddAnalogValue("coil0current", 0, 0);
            p.AddAnalogValue("coil0current", 120000, 0);

            p.SwitchAllOffAtEndOfPattern();
            return p;
        }

    }
