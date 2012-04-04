using MOTMaster;
using MOTMaster.SnippetLibrary;

using System;
using System.Collections.Generic;

using DAQ.Pattern;
using DAQ.Analog;

// This script can be used to test any aspect of taking an absorption image of atoms in the magnetic trap.

public class Patterns : MOTMasterScript
{


    public Patterns()
    {
        Parameters = new Dictionary<string, object>();
        Parameters["PatternLength"] = 160000;
        Parameters["MOTStartTime"] = 1000;
        Parameters["MOTCoilsCurrent"] = 0.0;
        Parameters["MOTLoadTime"] = 100000;
        Parameters["NumberOfFrames"] = 2;
        Parameters["Frame0TriggerDuration"] = 10;
        Parameters["Frame0Trigger"] = 100010;
        Parameters["Frame1TriggerDuration"] = 10;
        Parameters["Frame1Trigger"] = 100200;
        Parameters["ExposureTime"] = 1; //Remember to change this when the MM camera shutter value is changed in text file!
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

        p.DownPulse(150000, 0, 50, "CameraTrigger");
        p.DownPulse(155000, 0, 50, "CameraTrigger");

        // loads the mag trap by switching off the MOT beams
        p.AddEdge("aom0enable", (int)Parameters["MOTLoadTime"], false);
        p.AddEdge("aom1enable", (int)Parameters["MOTLoadTime"], false);
        //p.AddEdge("aom2enable", (int)Parameters["Frame0Trigger"], false); //switches off Zeeman beam 
        p.AddEdge("aom2enable", (int)Parameters["MOTLoadTime"], false);
        //p.AddEdge("aom3enable", (int)Parameters["MOTLoadTime"], false);
        p.DownPulse((int)Parameters["MOTLoadTime"], 0, (int)Parameters["Frame0Trigger"] - (int)Parameters["MOTLoadTime"], "aom3enable");
        //p.Pulse((int)Parameters["Frame1Trigger"], 0, (int)Parameters["ExposureTime"], "aom3enable");

        return p;
    }

    public override AnalogPatternBuilder GetAnalogPattern()
    {
        AnalogPatternBuilder p = new AnalogPatternBuilder((int)Parameters["PatternLength"]);

        MOTMasterScriptSnippet lm = new SHLoadMOT(p, Parameters);

        p.AddChannel("aom2frequency");
        p.AddChannel("aom3frequency");

        p.AddAnalogValue("coil0current", 0, 0);
        p.AddAnalogValue("aom2frequency", (int)Parameters["MOTStartTime"], 180.875);
        p.AddAnalogValue("aom3frequency", (int)Parameters["MOTStartTime"], 220.875);
        p.AddAnalogValue("aom2frequency", (int)Parameters["Frame0Trigger"], 200.875);
        p.AddAnalogValue("aom3frequency", (int)Parameters["Frame0Trigger"], 200.875);
        p.AddAnalogValue("coil0current", 100100, 0);

        p.SwitchAllOffAtEndOfPattern();
        return p;
    }

}