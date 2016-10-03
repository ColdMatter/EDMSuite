using MOTMaster;
using MOTMaster.SnippetLibrary;

using System;
using System.Collections.Generic;

using DAQ.Pattern;
using DAQ.Analog;

// This script can be used to test any aspect of the absorption imaging. 

public class Patterns : MOTMasterScript
{


    public Patterns()
    {
        Parameters = new Dictionary<string, object>();
        Parameters["PatternLength"] = 200000;
        Parameters["MOTStartTime"] = 1000;
        Parameters["MOTCoilsCurrent"] = 10.0;
        Parameters["MOTLoadTime"] = 100000; //this is not a duration, it is used to define a time at which the MOT should be switched off (10 sec)
        Parameters["NumberOfFrames"] = 2;
        Parameters["Frame0TriggerDuration"] = 50;
        Parameters["Frame0Trigger"] = 98000;
        Parameters["Frame1TriggerDuration"] = 50;
        Parameters["Frame1Trigger"] = 100500;
        //Parameters["Frame2TriggerDuration"] = 50;
        //Parameters["Frame2Trigger"] = 16000;
        //Parameters["Frame3TriggerDuration"] = 50;
        //Parameters["Frame3Trigger"] = 16500;
        //Parameters["Frame4TriggerDuration"] = 50;
        //Parameters["Frame4Trigger"] = 17000; 
        Parameters["TSAcceleration"] = 10.0;
        Parameters["TSDeceleration"] = 10.0;
        Parameters["TSDistance"] = 0.0;
        Parameters["TSVelocity"] = 10.0;

    }

    public override PatternBuilder32 GetDigitalPattern()
    {
        PatternBuilder32 p = new PatternBuilder32();

        MOTMasterScriptSnippet lm = new SHLoadMOT(p, Parameters);  // This is how you load "preset" patterns.

        p.Pulse(0, 0, 1, "AnalogPatternTrigger");  //NEVER CHANGE THIS!!!! IT TRIGGERS THE ANALOG PATTERN!

        p.Pulse(10000, 0, 100000, "TranslationStageTrigger");

        p.AddEdge("CameraTrigger", 0, true);
        p.DownPulse((int)Parameters["Frame0Trigger"], 0, (int)Parameters["Frame0TriggerDuration"], "CameraTrigger");
        p.DownPulse((int)Parameters["Frame1Trigger"], 0, (int)Parameters["Frame1TriggerDuration"], "CameraTrigger");
        //p.DownPulse((int)Parameters["Frame2Trigger"], 0, (int)Parameters["Frame2TriggerDuration"], "CameraTrigger");
        //p.DownPulse((int)Parameters["Frame3Trigger"], 0, (int)Parameters["Frame3TriggerDuration"], "CameraTrigger");
        //p.DownPulse((int)Parameters["Frame4Trigger"], 0, (int)Parameters["Frame4TriggerDuration"], "CameraTrigger");

        p.DownPulse(190000, 0, 50, "CameraTrigger");
        p.DownPulse(195000, 0, 50, "CameraTrigger");

        //p.AddEdge("aom0enable", (int)Parameters["MOTLoadTime"], false);
        //p.AddEdge("aom1enable", (int)Parameters["MOTLoadTime"], false);
        //p.AddEdge("aom2enable", (int)Parameters["Frame0Trigger"], false);

        return p;
    }

    public override AnalogPatternBuilder GetAnalogPattern()
    {
        AnalogPatternBuilder p = new AnalogPatternBuilder((int)Parameters["PatternLength"]);

        MOTMasterScriptSnippet lm = new SHLoadMOT(p, Parameters);

        p.AddChannel("aom2frequency");
        p.AddChannel("aom3frequency");

        p.AddAnalogValue("coil0current", 0, 0);
        p.AddAnalogValue("aom2frequency", (int)Parameters["MOTStartTime"], 190.875);
        p.AddAnalogValue("aom3frequency", (int)Parameters["MOTStartTime"], 210.875);
        p.AddAnalogValue("aom2frequency", (int)Parameters["Frame0Trigger"] - 1, 200.875);
        p.AddAnalogValue("aom3frequency", (int)Parameters["Frame0Trigger"] - 1, 200.875);
        p.AddAnalogValue("coil0current", (int)Parameters["MOTLoadTime"], 0);

        p.SwitchAllOffAtEndOfPattern();
        return p;
    }

}
