using MOTMaster;

using System;
using System.Collections.Generic;

using DAQ.Pattern;
using DAQ.Analog;

// Test-only linked script, used by MOTMasterTests\LinkedScriptBuilderTests.cs. It is NOT in any script
// folder and is not compiled into the test harness (run-tests.ps1 globs MOTMasterTests\*.cs
// non-recursively and this file lives in a subfolder). The tests compile it at run time with CodeDom,
// exactly as MOTMaster compiles a real script.
//
// CaF (primary) channels:   DDS_Analog_Trg, motLightSwitch (digital), MOTCoilsCurrent (analog)
// RbTweezer (secondary):    tCoolSwitch (digital), tMOTcc (analog)
public class Patterns : MOTMasterScript
{
    public Patterns()
    {
        Parameters = new Dictionary<string, object>();
        LoadGlobalParameters();

        // The tests hardcode these values, so the script must not depend on the user's live, editable
        // globalParameters.json: set every value the script uses unconditionally after LoadGlobalParameters().
        Parameters["TCLBlockStart"] = 4000;
        Parameters["PatternLength"] = 50000;
        Parameters["SecondaryTriggerDelay"] = 2;
    }

    public override PatternBuilder32 GetDigitalPattern()
    {
        PatternBuilder32 p = new PatternBuilder32();
        int start = (int)Parameters["TCLBlockStart"];

        // The analog start: the first rising edge of DDS_Analog_Trg.
        p.Pulse(0, start, 10, "DDS_Analog_Trg");

        // A CaF digital channel.
        p.Pulse(0, 500, 100, "motLightSwitch");

        // RbTweezer: an edge at script t = 1000 lands at secondary tick 1000 - SecondaryTriggerDelay.
        p.AddEdge("tCoolSwitch", 1000, true);
        p.AddEdge("tCoolSwitch", 1500, false);
        return p;
    }

    public override AnalogPatternBuilder GetAnalogPattern()
    {
        AnalogPatternBuilder a = new AnalogPatternBuilder((int)Parameters["PatternLength"]);
        a.AddChannel("MOTCoilsCurrent");
        a.AddChannel("tMOTcc");

        a.AddAnalogValue("MOTCoilsCurrent", 0, 1.0);
        a.AddAnalogValue("MOTCoilsCurrent", 200, 2.0);

        // RbTweezer: analog t = 100 lands at secondary tick 100 + A - SecondaryTriggerDelay.
        a.AddAnalogValue("tMOTcc", 100, 2.5);
        return a;
    }
}
