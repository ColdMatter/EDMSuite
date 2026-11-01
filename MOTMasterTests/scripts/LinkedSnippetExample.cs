using MOTMaster;
using MOTMaster.SnippetLibrary;

using System;
using System.Collections.Generic;

using DAQ.Pattern;
using DAQ.Analog;

// Test-only linked script that loads the molecule MOT through a snippet, as the cafmot scripts do, used by
// MOTMasterTests\LinkedScriptBuilderTests.cs. Like LinkedExample.cs it lives in a subfolder so the harness
// does not compile it; the tests compile it at run time with CodeDom.
//
// CaF (primary):         everything LoadMoleculeMOTNoSlowingEdge drives, plus DDS_Analog_Trg at the Q-switch
// RbTweezer (secondary): tCoolSwitch (digital), tMOTcc (analog)
public class Patterns : MOTMasterScript
{
    public Patterns()
    {
        Parameters = new Dictionary<string, object>();
        LoadGlobalParameters();

        // Set every value the script and the snippet use, so the tests don't depend on globalParameters.json.
        Parameters["PatternLength"] = 50000;
        Parameters["TCLBlockStart"] = 4000;
        Parameters["SecondaryTriggerDelay"] = 2;
        Parameters["FlashToQ"] = 16;
        Parameters["QSwitchPulseDuration"] = 10;
        Parameters["HeliumShutterToQ"] = 100;
        Parameters["HeliumShutterDuration"] = 1550;
        Parameters["SlowingChirpStartTime"] = 340;
        Parameters["SlowingChirpDuration"] = 1160;
        Parameters["SlowingChirpStartValue"] = 0.0;
        Parameters["SlowingChirpEndValue"] = -1.25;
    }

    public override PatternBuilder32 GetDigitalPattern()
    {
        PatternBuilder32 p = new PatternBuilder32();
        int patternStartBeforeQ = (int)Parameters["TCLBlockStart"];

        MOTMasterScriptSnippet lm = new LoadMoleculeMOTNoSlowingEdge(p, Parameters);
        p.Pulse(patternStartBeforeQ, 0, (int)Parameters["QSwitchPulseDuration"], "DDS_Analog_Trg");

        p.AddEdge("tCoolSwitch", 1000, true);
        p.AddEdge("tCoolSwitch", 1500, false);
        return p;
    }

    public override AnalogPatternBuilder GetAnalogPattern()
    {
        AnalogPatternBuilder a = new AnalogPatternBuilder((int)Parameters["PatternLength"]);

        MOTMasterScriptSnippet lm = new LoadMoleculeMOTNoSlowingEdge(a, Parameters);

        a.AddChannel("tMOTcc");
        a.AddAnalogValue("tMOTcc", 100, 2.5);
        return a;
    }
}
