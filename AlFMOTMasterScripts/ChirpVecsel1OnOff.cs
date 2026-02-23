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
        Parameters["CameraDelay"] = 100;
        //1V -> ~200 MHz Vecsel 1
        //2V -> ~400 MHz Vecsel 1
        Parameters["v1_amp"] = -2.5;
        Parameters["v1_offset"] = 2.5;
        Parameters["ChirpLength"] = 1000;
        Parameters["v1_hold_time"] = 1000;
        Parameters["Switch"] = true;

        switchConfiguration = new Dictionary<string, List<object>>
            {
                {"Switch", new List<object>{true, false}}
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
        //p.AddEdge("bXSlowingShutter", patternStartBeforeQ + (int)Parameters["slowingAOMOffStart"] + (int)Parameters["slowingAOMOffDuration"], false)

        p.AddEdge("q",0,true);
        p.AddEdge("q",100,false);

        //p.AddEdge("flash", 0, true);
        //p.AddEdge("flash", 100, false);

        p.AddEdge("detector", cameraDelay, true);
        p.AddEdge("detector", cameraDelay + 10, false);

        return p;
    }

    public override AnalogPatternBuilder GetAnalogPattern()
    {
        int chirpLen = Convert.ToInt32(Parameters["ChirpLength"]);
        int holdTime = Convert.ToInt32(Parameters["v1_hold_time"]);

        AnalogPatternBuilder p = new AnalogPatternBuilder((int)Parameters["PatternLength"]);
        p.AddChannel("AOM1_VCA");
        p.AddChannel("VECSEL1_CHIRP");

        p.AddAnalogValue("VECSEL1_CHIRP", 0, (double)Parameters["v1_offset"]);
        if (!(bool)Parameters["Switch"]) return p;
        p.AddLinearRamp("VECSEL1_CHIRP", 1, chirpLen, (double)Parameters["v1_offset"] + Convert.ToDouble(Parameters["v1_amp"]));
        p.AddLinearRamp("VECSEL1_CHIRP", chirpLen + 2 + holdTime, chirpLen, (double)Parameters["v1_offset"]);

        return p;
   }

}
