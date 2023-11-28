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
        Parameters["PatternLength"] = 2000;


        // Linear Out

        Parameters["Start"] = 0.0;//0.0
        Parameters["End"] = 10.0;
      
    }

    public override PatternBuilder32 GetDigitalPattern()
    {
        PatternBuilder32 p = new PatternBuilder32();

        //MOTMasterScriptSnippet lm = new LoadMoleculeMOT(p, Parameters); // This is how you load "preset" patterns.          
        //   p.AddEdge("v00Shutter", 0, true);
        //p.Pulse(patternStartBeforeQ, 3000 - 1400, 10000, "bXSlowingShutter"); //Takes 14ms to start closing

        //p.Pulse(patternStartBeforeQ, (int)Parameters["Frame0Trigger"], (int)Parameters["Frame0TriggerDuration"], "cameraTrigger"); //camera trigger for first frame

        //p.AddEdge("bXSlowingShutter", patternStartBeforeQ + (int)Parameters["slowingAOMOnStart"] + (int)Parameters["slowingAOMOffStart"] - 1650, true);
        //p.AddEdge("bXSlowingShutter", patternStartBeforeQ + (int)Parameters["slowingAOMOffStart"] + (int)Parameters["slowingAOMOffDuration"], false);
        p.AddEdge("q",0,true);
        p.AddEdge("q",10,false);

        return p;
    }

    public override AnalogPatternBuilder GetAnalogPattern()
    {
        AnalogPatternBuilder p = new AnalogPatternBuilder((int)Parameters["PatternLength"]);

        // Add Analog Channels
        
        p.AddChannel("VECSEL2_PZO");

        //p.AddAnalogPulse("VECSEL2_PZO", 0, 500, 5, 0);
        //p.AddAnalogValue("VECSEL2_PZO", 0, 0);
        p.AddAnalogValue("VECSEL2_PZO", 0, (double)Parameters["Start"]);
        p.AddLinearRamp("VECSEL2_PZO", 1, (int)Parameters["PatternLength"]-2 , (double)Parameters["End"]);
        //p.AddAnalogValue("VECSEL2_PZO", 0, 2);

        return p;
   }

}
