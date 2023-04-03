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
        Parameters["TCLBlockStart"] = 4000; // This is a time before the Q switch
        Parameters["TCLBlockDuration"] = 15000;

        Parameters["TriggerStart"] = 1000;
        Parameters["TriggerDuration"] = 1000;
        Parameters["testValue1"] = 0.5;
        Parameters["testValue2"] = 1.7;

    }

    public override PatternBuilder32 GetDigitalPattern()
    {
        PatternBuilder32 p = new PatternBuilder32();
        int patternStartBeforeQ = (int)Parameters["TCLBlockStart"];

        p.Pulse(patternStartBeforeQ, 0, 10, "aoTrigger");
        p.Pulse(patternStartBeforeQ, (int)Parameters["TriggerStart"], (int)Parameters["TriggerDuration"], "cameraTrigger");
        
        return p;
    }

    public override AnalogPatternBuilder GetAnalogPattern()
    {
        AnalogPatternBuilder p = new AnalogPatternBuilder((int)Parameters["PatternLength"]);

        p.AddChannel("steppingBBias");

        p.AddAnalogValue("steppingBBias", 0, 0.0);
        p.AddAnalogValue("steppingBBias", (int)Parameters["TriggerStart"], (double)Parameters["testValue1"]);
        p.AddAnalogValue("steppingBBias", (int)Parameters["TriggerStart"] + (int)Parameters["TriggerDuration"], (double)Parameters["testValue2"]);

        return p;
    }

}
