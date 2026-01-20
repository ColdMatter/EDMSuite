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
        Parameters["t0"] = 1000;
        Parameters["CameraDelay"] = 1500;

        Parameters["AOM1V"] = 10.0;
        Parameters["AOM1On"] = -100;
        Parameters["AOM1Off"] = 1000;

        Parameters["AOM2V"] = 10.0;
        Parameters["AOM2On"] = 100;
        Parameters["AOM2Off"] = 300;

        //switchConfiguration = new Dictionary<string, List<object>>
        //    {
        //        {"AOM1V", new List<object>{10.0, 0.0, 10.0, 0.0}},
        //        {"AOM2V", new List<object>{10.0, 10.0, 0.0, 0.0}}
        //    };
        switchConfiguration = new Dictionary<string, List<object>>
            {
                {"AOM1V", new List<object>{0.0, 0.0}},
                {"AOM2V", new List<object>{10.0, 0.0}}
            };
    }

    public override PatternBuilder32 GetDigitalPattern()
    {
        PatternBuilder32 p = new PatternBuilder32();
        int cameraDelay = Convert.ToInt32(Parameters["CameraDelay"]);
        int t0 = Convert.ToInt32(Parameters["t0"]);

        //MOTMasterScriptSnippet lm = new LoadMoleculeMOT(p, Parameters); // This is how you load "preset" patterns.          
        //   p.AddEdge("v00Shutter", 0, true);
        //p.Pulse(patternStartBeforeQ, 3000 - 1400, 10000, "bXSlowingShutter"); //Takes 14ms to start closing

        //p.Pulse(patternStartBeforeQ, (int)Parameters["Frame0Trigger"], (int)Parameters["Frame0TriggerDuration"], "cameraTrigger"); //camera trigger for first frame

        //p.AddEdge("bXSlowingShutter", patternStartBeforeQ + (int)Parameters["slowingAOMOnStart"] + (int)Parameters["slowingAOMOffStart"] - 1650, true);
        //p.AddEdge("bXSlowingShutter", patternStartBeforeQ + (int)Parameters["slowingAOMOffStart"] + (int)Parameters["slowingAOMOffDuration"], false);
        p.AddEdge("q",t0,true);
        p.AddEdge("q",t0 + 100,false);

        p.AddEdge("flash", t0 - 14, true);
        p.AddEdge("flash", t0 + 100, false);

        p.AddEdge("detector", t0 + cameraDelay, true);
        p.AddEdge("detector", t0 + cameraDelay + 10, false);

        p.AddEdge("He_Shutter", 0, true);
        p.AddEdge("He_Shutter", 2*t0, false);

        return p;
    }

    public override AnalogPatternBuilder GetAnalogPattern()
    {
        AnalogPatternBuilder p = new AnalogPatternBuilder((int)Parameters["PatternLength"]);
        int t0 = Convert.ToInt32(Parameters["t0"]);
        int aom1on = Convert.ToInt32(Parameters["AOM1On"]);
        int aom1off = Convert.ToInt32(Parameters["AOM1Off"]);

        int aom2on = Convert.ToInt32(Parameters["AOM2On"]);
        int aom2off = Convert.ToInt32(Parameters["AOM2Off"]);

        p.AddChannel("AOM1_VCA");
        p.AddChannel("AOM2_VCA");

        p.AddAnalogValue("AOM1_VCA", t0 + aom1on, Convert.ToDouble(Parameters["AOM1V"]));
        p.AddAnalogValue("AOM1_VCA", t0 + aom1off, 0);

        p.AddAnalogValue("AOM2_VCA", t0 + aom2on, Convert.ToDouble(Parameters["AOM2V"]));
        p.AddAnalogValue("AOM2_VCA", t0 + aom2off, 0);

        //p.AddAnalogValue("VECSEL2_PZO", 0, 2);

        return p;
   }

}
