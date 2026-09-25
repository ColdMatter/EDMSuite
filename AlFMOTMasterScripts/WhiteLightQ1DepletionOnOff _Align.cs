using MOTMaster;
using MOTMaster.SnippetLibrary;

using System;
using System.Collections.Generic;

using DAQ.Pattern;
using DAQ.Analog;

// This script is for just using the Q1 on the slowing axis to deplete the TOF and probe with the Q1 in the MOT chamber. "Pump" refers to the VECSEL 2 Q(1) line, which is actually the probe. Slower is the VECSEL 3 on the Q(1) line

public class Patterns : MOTMasterScript
{
    public Patterns()
    {
        Parameters = new Dictionary<string, object>();
        Parameters["PatternLength"] = 100000;
        Parameters["Void"] = 0;
        Parameters["CameraDelay"] = 600;
        Parameters["SlowerDelay"] = 50;
        Parameters["SlowerDelaySwitch"] = 0;
        Parameters["SlowerWidth"] = 200;
        Parameters["ShutterDelay"] = 10000;
        Parameters["SlowerSwitch"] = true;
        Parameters["PumpDelay"] = 0;
        Parameters["PumpDelaySwitch"] = 0;
        Parameters["PumpWidth"] = 1500;
        Parameters["PumpSwitch"] = true;
        this.switchConfiguration = new Dictionary<string, List<object>>
            {
                {"SlowerSwitch", new List<object>{true, false, true}},
                {"SlowerDelaySwitch", new List<object>{0,0,100}},
            };
        this.switchConfiguration = new Dictionary<string, List<object>>
            {
                {"SlowerSwitch", new List<object>{true, false}},
                {"SlowerDelaySwitch", new List<object>{0,0}},
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

        p.AddEdge("He_Shutter", 0, true);
        p.AddEdge("He_Shutter", 2 * shutterDelay, false);

        //Shutter begins closed, then immediately triggers to open.
        p.AddEdge("VECSEL2_Shutter", 0, true);
        p.AddEdge("VECSEL2_Shutter", 1, false);
        //Close shutter after 200 ms after experiment is complete to limit repump hitting cell
        //p.AddEdge("VECSEL2_Shutter", 2 * shutterDelay, true);

        return p;
    }

    public override AnalogPatternBuilder GetAnalogPattern()
    {
        int slowerDelay = Convert.ToInt32(Parameters["SlowerDelay"]) + Convert.ToInt32(Parameters["SlowerDelaySwitch"]);
        int slowerWidth = Convert.ToInt32(Parameters["SlowerWidth"]);
        int shutterDelay = Convert.ToInt32(Parameters["ShutterDelay"]);
        int pumpDelay = Convert.ToInt32(Parameters["PumpDelay"]) + Convert.ToInt32(Parameters["PumpDelaySwitch"]);
        int pumpWidth = Convert.ToInt32(Parameters["PumpWidth"]);
        AnalogPatternBuilder p = new AnalogPatternBuilder((int)Parameters["PatternLength"]);
        p.AddChannel("AOM1_VCA");
        p.AddChannel("AOM2_VCA");

        if (pumpDelay + shutterDelay != 0 || !(bool)Parameters["PumpSwitch"])
            p.AddAnalogValue("AOM1_VCA", 0, 0);


        if (slowerDelay + shutterDelay != 0)
        {
            p.AddAnalogValue("AOM1_VCA", 0, 0);
            p.AddAnalogValue("AOM2_VCA", 0, 0);
        }

        p.AddAnalogValue("AOM1_VCA", shutterDelay + pumpDelay, 10);
        p.AddAnalogValue("AOM1_VCA", shutterDelay + pumpDelay + pumpWidth, 0);

        if ((bool)Parameters["SlowerSwitch"])
        {
            p.AddAnalogValue("AOM2_VCA", shutterDelay + slowerDelay, 10);
            p.AddAnalogValue("AOM2_VCA", shutterDelay + slowerDelay + slowerWidth, 0);
        }
        
        return p;
   }

}
