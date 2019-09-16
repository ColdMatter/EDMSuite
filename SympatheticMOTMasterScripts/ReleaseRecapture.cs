using MOTMaster;
using MOTMaster.SnippetLibrary;

using System;
using System.Collections.Generic;

using DAQ.Pattern;
using DAQ.Analog;


//This is a release and recapture script that can be used to measure the temperature of the MOT. It switches on the MOT,
// takes an image, switches off the coils to release the MOT, switches them back on to recapture the MOT and then 
//takes another image of the cloud. Note this method requires light levels to be the same for all images taken   
public class Patterns : MOTMasterScript
{


    public Patterns()
    {
        Parameters = new Dictionary<string, object>();
        Parameters["MOTLoadDuration"] = 100000;
        Parameters["MOTStartTime"] = 1000;
        Parameters["PatternLength"] = 200000;
        Parameters["NumberOfFrames"] = 4;
        Parameters["ReleaseTime"] = 10;
        Parameters["Frame0TriggerDuration"] = 100;
        Parameters["Frame0Trigger"] = 95000;
        Parameters["Frame1TriggerDuration"] = 100;
        Parameters["Frame1Trigger"] = 100020;
        Parameters["Frame2TriggerDuration"] = 100;
        Parameters["Frame2Trigger"] = 120000;
        Parameters["Frame3TriggerDuration"] = 100;
        Parameters["Frame3Trigger"] = 130000;
        Parameters["MOTCoilsCurrent"] = 17.0;
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

        p.AddEdge("CameraTrigger", 0, true);
        p.DownPulse((int)Parameters["Frame0Trigger"], 0, (int)Parameters["Frame0TriggerDuration"], "CameraTrigger");
        p.DownPulse((int)Parameters["Frame1Trigger"], 0, (int)Parameters["Frame1TriggerDuration"], "CameraTrigger");
        p.DownPulse((int)Parameters["Frame2Trigger"], 0, (int)Parameters["Frame2TriggerDuration"], "CameraTrigger");
        p.DownPulse((int)Parameters["Frame3Trigger"], 0, (int)Parameters["Frame3TriggerDuration"], "CameraTrigger");

        p.DownPulse(160000, 0, 50, "CameraTrigger");
        p.DownPulse(170000, 0, 50, "CameraTrigger");

        p.DownPulse((int)Parameters["MOTLoadDuration"], 0, (int)Parameters["ReleaseTime"], "aom0enable");
        p.DownPulse((int)Parameters["MOTLoadDuration"], 0, (int)Parameters["ReleaseTime"], "aom1enable");
        p.DownPulse((int)Parameters["MOTLoadDuration"], 0, (int)Parameters["ReleaseTime"], "aom3enable");
        p.DownPulse((int)Parameters["MOTLoadDuration"], 0, 25000, "aom2enable");
        
        return p;
    }

    public override AnalogPatternBuilder GetAnalogPattern()
    {
        AnalogPatternBuilder p = new AnalogPatternBuilder((int)Parameters["PatternLength"]);

        MOTMasterScriptSnippet lm = new SHLoadMOT(p, Parameters);
        p.AddAnalogPulse("coil0current", (int)Parameters["MOTLoadDuration"], (int)Parameters["ReleaseTime"], 0, (double)Parameters["MOTCoilsCurrent"]);
        p.AddAnalogValue("coil0current", 115000, 0);

        p.SwitchAllOffAtEndOfPattern();
        return p;
    }

}
