using MOTMaster;
using MOTMaster.SnippetLibrary;

using System;
using System.Collections.Generic;

using DAQ.Pattern;
using DAQ.Analog;

// This script is supposed to be the basic script for loading a molecule MOT.
// Note that times are all in units of the clock periods of the two pattern generator boards (at present, both are 10us).
// All times are relative to the Q switch, though note that this is not the first event in the pattern.
public class Patterns : MOTMasterScript
{
    public Patterns()
    {
        Parameters = new Dictionary<string, object>();
        Parameters["PatternLength"] = 50000;
        Parameters["Void"] = 0;
        Parameters["CameraDelay"] = 0;
        Parameters["YAGSwitch"] = true;

        switchConfiguration = new Dictionary<string, List<object>>
            {
                {"YAGSwitch", new List<object>{true, false}}
            };
    }

    public override PatternBuilder32 GetDigitalPattern()
    {
        PatternBuilder32 p = new PatternBuilder32();
        int cameraDelay = Convert.ToInt32(Parameters["CameraDelay"]);

        //MOTMasterScriptSnippet lm = new LoadMoleculeMOT(p, Parameters); // This is how you load "preset" patterns.          
        //   p.AddEdge("v00Shutter", 0, true);
        //p.Pulse(patternStartBeforeQ, 3000 - 1400, 10000, "bXSlowingShutter"); //Takes 14ms to start closing

        //p.Pulse(patternStartBeforeQ, (int)Parameters["Frame0Trigger"], (int)Parameters["Frame0TriggerDuration"], "cameraTrigger"); //camera trigger for first frame

        //p.AddEdge("bXSlowingShutter", patternStartBeforeQ + (int)Parameters["slowingAOMOnStart"] + (int)Parameters["slowingAOMOffStart"] - 1650, true);
        //p.AddEdge("bXSlowingShutter", patternStartBeforeQ + (int)Parameters["slowingAOMOffStart"] + (int)Parameters["slowingAOMOffDuration"], false);
        p.AddEdge("q",10000+0,true);
        p.AddEdge("q",10000+100,false);

        if ((bool)Parameters["YAGSwitch"])
        {
            p.AddEdge("flash", 10000-14, true);
            p.AddEdge("flash", 10000+100, false);
        }

        p.AddEdge("detector", 10000 + cameraDelay, true);
        p.AddEdge("detector", 10000 + cameraDelay + 10, false);

        p.AddEdge("VECSEL2_Shutter", 0, true);
        p.AddEdge("VECSEL2_Shutter", 1, false);
        p.AddEdge("VECSEL2_Shutter", 20000, true);

        p.AddEdge("He_Shutter", 0, true);
        p.AddEdge("He_Shutter", 20000, false);

        return p;
    }

    public override AnalogPatternBuilder GetAnalogPattern()
    {
        AnalogPatternBuilder p = new AnalogPatternBuilder((int)Parameters["PatternLength"]);
        p.AddChannel("AOM1_VCA");
        p.AddAnalogValue("AOM1_VCA", 0, 0);
        //p.AddAnalogValue("VECSEL2_PZO", 0, 2);

        return p;
   }

}
