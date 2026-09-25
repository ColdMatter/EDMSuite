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
        Parameters["CameraDelay"] = 100;
        Parameters["SlowerDelay"] = 100;
        Parameters["SlowerWidth"] = 200;
        Parameters["ShutterDelay"] = 10000;
        Parameters["SlowerSwitch"] = true;
        Parameters["PumpDelay"] = 0;
        Parameters["PumpDelaySwitch"] = 0;
        Parameters["PumpWidth"] = 50;
        Parameters["PumpSwitch"] = true;
        this.switchConfiguration = new Dictionary<string, List<object>>
            {
                {"SlowerSwitch", new List<object>{true, false, false}},
                {"PumpDelaySwitch", new List<object>{0,0,0}},
                {"PumpSwitch", new List<object>{true,true,false}},
            };
    }

    public override PatternBuilder32 GetDigitalPattern()
    {
        PatternBuilder32 p = new PatternBuilder32();
        int cameraDelay = Convert.ToInt32(Parameters["CameraDelay"]);
        int shutterDelay = Convert.ToInt32(Parameters["ShutterDelay"]);

        //MOTMasterScriptSnippet lm = new LoadMoleculeMOT(p, Parameters); // This is how you load "preset" patterns.          
        //   p.AddEdge("v00Shutter", 0, true);

        //Shutter takes ~40 ms +- 10 ms to start opening and closing. Shutter delay of 100 ms ensures shutter is fully open. Add shutter delay to all timings so that shutter opening is the first event in the pattern

        p.AddEdge("q",shutterDelay,true);
        p.AddEdge("q",100 + shutterDelay, false);

        p.AddEdge("flash", 0 + shutterDelay-14, true);
        p.AddEdge("flash", 100 + shutterDelay, false);

        p.AddEdge("detector", cameraDelay + shutterDelay, true);
        p.AddEdge("detector", cameraDelay + 10 + shutterDelay, false);

        //Shutter begins closed, then immediately triggers to open.
        p.AddEdge("VECSEL2_Shutter", 0, true);
        p.AddEdge("VECSEL2_Shutter", 1, false);
        //Close shutter after 200 ms after experiment is complete to limit repump hitting cell
        p.AddEdge("VECSEL2_Shutter", 2 * shutterDelay, true);

        return p;
    }

    public override AnalogPatternBuilder GetAnalogPattern()
    {
        int slowerDelay = Convert.ToInt32(Parameters["SlowerDelay"]);
        int slowerWidth = Convert.ToInt32(Parameters["SlowerWidth"]);
        int shutterDelay = Convert.ToInt32(Parameters["ShutterDelay"]);
        int pumpDelay = Convert.ToInt32(Parameters["PumpDelay"]) + Convert.ToInt32(Parameters["PumpDelaySwitch"]);
        int pumpWidth = Convert.ToInt32(Parameters["PumpWidth"]);
        AnalogPatternBuilder p = new AnalogPatternBuilder((int)Parameters["PatternLength"]);
        p.AddChannel("AOM1_VCA");
        p.AddChannel("
            ");

        if (pumpDelay + shutterDelay != 0 || !(bool)Parameters["PumpSwitch"])
            p.AddAnalogValue("AOM1_VCA", 0, 0);


        if (slowerDelay + shutterDelay != 0)
        {
            p.AddAnalogValue("AOM1_VCA", 0, 0);
            p.AddAnalogValue("AOM2_VCA", 0, 0);
        }
        if ((bool)Parameters["PumpSwitch"])
        {
            p.AddAnalogValue("AOM1_VCA", shutterDelay + pumpDelay, 10);
            p.AddAnalogValue("AOM1_VCA", shutterDelay + pumpDelay + pumpWidth, 0);
        }
        if ((bool)Parameters["SlowerSwitch"])
        {
            p.AddAnalogValue("AOM2_VCA", shutterDelay + slowerDelay, 10);
            p.AddAnalogValue("AOM2_VCA", shutterDelay + slowerDelay + slowerWidth, 0);
        }
        
        return p;
   }

}
