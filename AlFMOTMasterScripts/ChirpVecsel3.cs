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
        Parameters["v1_amp"] = 0.0;
        //1V -> 300 MHz Vecsel 2
        //0.67V - > 600 MHz UV chirp
        //1V -> 800 MHz VECSEL3
        //0.5V/10ms -> ~80 MHz/ms 
        Parameters["v0_amp"] = 0.5;
        Parameters["v1_offset"] = 0.0;
        Parameters["v0_offset"] = 0.0;
        Parameters["ChirpLength"] = 1000;
        Parameters["v1_hold_time"] = 1000;
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

        p.AddEdge("q",14,true);
        p.AddEdge("q",100,false);

        p.AddEdge("flash", 0, true);
        p.AddEdge("flash", 100, false);

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
        p.AddChannel("VECSEL3_CHIRP");
        //p.AddAnalogValue("AOM1_VCA", 0, 0);

        // Start at : 323.449929 THz (-200 m/s from (3/2, 2))
        // End at   : 323.450201 THz (+2.5 Gamma from (3/2, 2), roughly resonant on F1=5/2)
        //p.AddAnalogValue("VECSEL1_CHIRP", 0, (double)Parameters["v1_offset"]);
        //p.AddLinearRamp("VECSEL1_CHIRP", 1, chirpLen, (double)Parameters["v1_offset"] + (double)Parameters["v1_amp"]);
        //p.AddLinearRamp("VECSEL1_CHIRP", chirpLen + holdTime + 2, chirpLen, (double)Parameters["v1_offset"]);


        // Start at : 329.390497 THz (-200 m/s from (3/2, 2))
        // End at   : 329.390662 THz (-50  m/s from (3/2, 2))
        p.AddAnalogValue("VECSEL3_CHIRP", 0, (double)Parameters["v0_offset"]);
        p.AddLinearRamp("VECSEL3_CHIRP", 1, chirpLen, (double)Parameters["v0_offset"] + (double)Parameters["v0_amp"]);
        p.AddLinearRamp("VECSEL3_CHIRP", chirpLen + 2, chirpLen, (double)Parameters["v0_offset"]);

        return p;
   }

}
