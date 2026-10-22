using MOTMaster;

using System;
using System.Collections.Generic;

using DAQ.Pattern;
using DAQ.Analog;

// Scope check for linked mode: run on PH-BONESAW with "Linked" ticked (RbTweezer is the secondary).
// Without linked mode this script fails, because tScopeTest is an RbTweezer channel.
//
// What to probe (each pulse is 10 ms, starting 10 ms into the pattern):
//   PH-BONESAW  v0ddsSwitchA  /PXI1Slot4 port 2 line 0   (spare CaF line)
//   RbTweezer   tScopeTest    /PXI1Slot5 port 2 line 0   (spare Rb line)
//   PH-BONESAW  rbTrigger     /PXI1Slot4 port 1 line 6   (added by MOTMaster: 100 us pulse at t = 0)
//
// The two test pulses should coincide, apart from the trigger/cable delay. Measure the Rb pulse's lag
// behind the CaF pulse and set SecondaryTriggerDelay in globalParameters.json to it in 10 us ticks.
public class Patterns : MOTMasterScript
{
    public Patterns()
    {
        Parameters = new Dictionary<string, object>();
        LoadGlobalParameters();

        Parameters["PatternLength"] = 20000;
        Parameters["PulseStart"] = 1000;
        Parameters["PulseDuration"] = 1000;
    }

    public override PatternBuilder32 GetDigitalPattern()
    {
        PatternBuilder32 p = new PatternBuilder32();
        int start = (int)Parameters["PulseStart"];
        int duration = (int)Parameters["PulseDuration"];

        p.Pulse(start, 0, duration, "v0ddsSwitchA");
        p.Pulse(start, 0, duration, "tScopeTest");

        return p;
    }

    public override AnalogPatternBuilder GetAnalogPattern()
    {
        return new AnalogPatternBuilder((int)Parameters["PatternLength"]);
    }
}
