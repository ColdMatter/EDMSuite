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
        Parameters["PatternLength"] = 20000;
        Parameters["Void"] = 0;
        //Parameters["CameraDelay"] = 10000;
        Parameters["CameraWidth"] = 100;

    }

    public override PatternBuilder32 GetDigitalPattern()
    {
        PatternBuilder32 p = new PatternBuilder32();
        int pulseWidth = Convert.ToInt32(Parameters["CameraWidth"]);
        //int pulseDelay = Convert.ToInt32(Parameters["CameraDelay"]);

        //MOTMasterScriptSnippet lm = new LoadMoleculeMOT(p, Parameters); // This is how you load "preset" patterns.          
        //   p.AddEdge("v00Shutter", 0, true);

        //Shutter takes ~40 ms +- 10 ms to start opening and closing. Shutter delay of 100 ms ensures shutter is fully open. Add shutter delay to all timings so that shutter opening is the first event in the pattern

        p.AddEdge("q",0, true);
        p.AddEdge("q",100, false);

        ////Shutter begins closed, then immediately triggers to open.
        //p.AddEdge("VECSEL2_Shutter", 0, true);
        //p.AddEdge("VECSEL2_Shutter", 1, false);
        ////Close shutter after 200 ms after experiment is complete to limit repump hitting cell
        //p.AddEdge("VECSEL2_Shutter", 2 * shutterDelay, true);

        p.AddEdge("detector", 0, true);
        p.AddEdge("detector", 0 + pulseWidth, false);

        return p;
    }

    public override AnalogPatternBuilder GetAnalogPattern()
    {
        AnalogPatternBuilder p = new AnalogPatternBuilder((int)Parameters["PatternLength"]);
        p.AddChannel("AOM1_VCA");
        p.AddChannel("AOM2_VCA");

        p.AddAnalogValue("AOM1_VCA", 0, 0);
        //p.AddAnalogValue("AOM1_VCA", pumpWidth, 0);

        p.AddAnalogValue("AOM2_VCA", 0, 0);
        //p.AddAnalogValue("AOM2_VCA", pumpWidth, 0);

        
        return p;
   }

}
