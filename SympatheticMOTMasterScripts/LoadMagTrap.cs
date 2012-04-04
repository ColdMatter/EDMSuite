using MOTMaster;
using MOTMaster.SnippetLibrary;

using System;
using System.Collections.Generic;

using DAQ.Pattern;
using DAQ.Analog;

// This script loads the magnetic trap. The MOT is loaded, then the AOMs are switched off to load the mag trap,
// three images are taken, one of the MOT, one of mag trap (AOMs are flashed on to obtain a fluorescence image 
// of the atoms loaded into the mag trap) and finally a background image with no atoms. 
public class Patterns : MOTMasterScript
{


    public Patterns()
    {
        Parameters = new Dictionary<string, object>();
        Parameters["PatternLength"] = 200000;
        Parameters["MOTStartTime"] = 1000;
        Parameters["MOTCoilsCurrent"] = 10.0;
        Parameters["MOTLoadDuration"] = 100000;
        Parameters["MagTrapDuration"] = 100;
        Parameters["NumberOfFrames"] = 3;
        Parameters["Frame0TriggerDuration"] = 100;
        Parameters["Frame0Trigger"] = 95000;
        Parameters["Frame1TriggerDuration"] = 100;
        Parameters["Frame1Trigger"] = 100100;
        Parameters["Frame2TriggerDuration"] = 100;
        Parameters["Frame2Trigger"] = 140000;
        Parameters["CameraExposure"] = 100; // NOTE this does not change the camera exposure time, you have to change the 
                                           // value in the camera attributes file, this is used to switch the Zeeman light
                                           // when an image is taken. 
        Parameters["aom2Detuning"] = 190.875;
        Parameters["aom3Detuning"] = 210.875;
        Parameters["MagRampTime"] = 99900;
        Parameters["MagTrapCurrent"] = 10.0;
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

        p.Pulse(40000, 0, 100000, "TranslationStageTrigger");

        p.AddEdge("CameraTrigger", 0, true);
        p.DownPulse((int)Parameters["Frame0Trigger"], 0, (int)Parameters["Frame0TriggerDuration"], "CameraTrigger");
        p.DownPulse((int)Parameters["Frame1Trigger"], 0, (int)Parameters["Frame1TriggerDuration"], "CameraTrigger");
        p.DownPulse((int)Parameters["Frame2Trigger"], 0, (int)Parameters["Frame2TriggerDuration"], "CameraTrigger");

        p.DownPulse(190000, 0, 50, "CameraTrigger");
        p.DownPulse(195000, 0, 50, "CameraTrigger");

        // switches off the Zeeman and absorption beams to avoid reloading the MOT whilst imaging, and to allow fluorescence images to be obtained
        p.DownPulse((int)Parameters["Frame0Trigger"], 0, (int)Parameters["CameraExposure"], "aom2enable");
        p.DownPulse((int)Parameters["Frame0Trigger"], 0, (int)Parameters["CameraExposure"], "aom3enable");

        // loads the mag trap, Zeeman light is not switched back on again to avoid MOT reloading
        p.DownPulse((int)Parameters["MOTLoadDuration"], 0, (int)Parameters["MagTrapDuration"], "aom0enable");
        p.DownPulse((int)Parameters["MOTLoadDuration"], 0, (int)Parameters["MagTrapDuration"], "aom1enable");
        p.AddEdge("aom2enable", (int)Parameters["MOTLoadDuration"], false);
        p.AddEdge("aom3enable", (int)Parameters["MOTLoadDuration"], false);

        return p;
    }

    public override AnalogPatternBuilder GetAnalogPattern()
    {
        AnalogPatternBuilder p = new AnalogPatternBuilder((int)Parameters["PatternLength"]);

        MOTMasterScriptSnippet lm = new SHLoadMOT(p, Parameters);

        p.AddChannel("aom2frequency");
        p.AddChannel("aom3frequency");

        p.AddAnalogValue("coil0current", 0, 0);
        p.AddAnalogValue("aom2frequency", 0, (double)Parameters["aom2Detuning"]);
        p.AddAnalogValue("aom3frequency", 0, (double)Parameters["aom3Detuning"]);
        //p.AddAnalogValue("coil0current", (int)Parameters["MagRampTime"], (double)Parameters["MagTrapCurrent"]);
        p.AddAnalogValue("coil0current", 130000, 0);

        p.SwitchAllOffAtEndOfPattern();
        return p;
    }

}

