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
        Parameters["PatternLength"] = 100000;
        Parameters["Void"] = 0;
        Parameters["CameraDelay"] = 1000;
        Parameters["MOT_probe_on"] = true;
        Parameters["Abs_delay"] = 10000;

        switchConfiguration = new Dictionary<string, List<bool>>
            {
                {"MOT_probe_on", new List<bool>{true, false}}
            };
    }

    public override PatternBuilder32 GetDigitalPattern()
    {
        PatternBuilder32 p = new PatternBuilder32();
        int cameraDelay = Convert.ToInt32(Parameters["CameraDelay"]);
        int absDelay = Convert.ToInt32(Parameters["Abs_delay"]);

        p.AddEdge("VECSEL2_Shutter", 0, true);
        if (!(bool)Parameters["MOT_probe_on"])
            p.AddEdge("VECSEL2_Shutter", 1, false);

        p.AddEdge("q", absDelay + 14, true);
        p.AddEdge("q", absDelay + 100, false);

        p.AddEdge("flash", absDelay + 0, true);
        p.AddEdge("flash", absDelay + 100, false);

        p.AddEdge("detector", absDelay + cameraDelay, true);
        p.AddEdge("detector", absDelay + cameraDelay + 10, false);

        return p;
    }

    public override AnalogPatternBuilder GetAnalogPattern()
    {
        AnalogPatternBuilder p = new AnalogPatternBuilder((int)Parameters["PatternLength"]);
        p.AddChannel("VECSEL3_AOM_VCA");
        p.AddAnalogValue("VECSEL3_AOM_VCA", 0, 0);
        //p.AddAnalogValue("VECSEL2_PZO", 0, 2);

        return p;
   }

}
