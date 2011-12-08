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
        Parameters["MOTLoadDuration"] = 100000;
        Parameters["MOTStartTime"] = 0;
        Parameters["PatternLength"] = 200000;
        Parameters["NumberOfFrames"] = 3;
        Parameters["ReleaseTime"] = 1;
        Parameters["Frame0TriggerDuration"] = 100;
        Parameters["Frame0Trigger"] = 95000;
        Parameters["Frame1TriggerDuration"] = 100;
        Parameters["Frame1Trigger"] = 100002;
        Parameters["Frame2TriggerDuration"] = 100;
        Parameters["Frame2Trigger"] = 115000;
        Parameters["MOTCoilsCurrent"] = 17.0;
    }

    public override PatternBuilder32 GetDigitalPattern()
    {
        PatternBuilder32 p = new PatternBuilder32();

        MOTMasterScriptSnippet lm = new SHLoadMOT(p, Parameters);  // This is how you load "preset" patterns.

        p.Pulse(0, 0, 1, "AnalogPatternTrigger");  //NEVER CHANGE THIS!!!! IT TRIGGERS THE ANALOG PATTERN!

        p.AddEdge("CameraTrigger", 0, true);
        p.DownPulse((int)Parameters["Frame0Trigger"], 0, (int)Parameters["Frame0TriggerDuration"], "CameraTrigger");
        p.DownPulse((int)Parameters["Frame1Trigger"], 0, (int)Parameters["Frame1TriggerDuration"], "CameraTrigger");
        p.DownPulse((int)Parameters["Frame2Trigger"], 0, (int)Parameters["Frame2TriggerDuration"], "CameraTrigger");        

        p.DownPulse(120000, 0, 50, "CameraTrigger");
        p.DownPulse(130000, 0, 50, "CameraTrigger");

        p.DownPulse((int)Parameters["MOTLoadDuration"], 0, (int)Parameters["ReleaseTime"], "aom0enable");
        p.DownPulse((int)Parameters["MOTLoadDuration"], 0, (int)Parameters["ReleaseTime"], "aom1enable");
        p.DownPulse((int)Parameters["MOTLoadDuration"], 0, (int)Parameters["ReleaseTime"], "aom3enable");
        p.AddEdge("aom2enable", (int)Parameters["Frame0Trigger"] - 1, false);
        
        return p;
    }

    public override AnalogPatternBuilder GetAnalogPattern()
    {
        AnalogPatternBuilder p = new AnalogPatternBuilder((int)Parameters["PatternLength"]);


        MOTMasterScriptSnippet lm = new SHLoadMOT(p, Parameters);

        p.AddAnalogPulse("coil0current", (int)Parameters["MOTLoadDuration"], (int)Parameters["ReleaseTime"], 0, (double)Parameters["MOTCoilsCurrent"]);
        p.AddAnalogValue("coil0current", 110000, 0);

        p.SwitchAllOffAtEndOfPattern();
        return p;
    }

}
