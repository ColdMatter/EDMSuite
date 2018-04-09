using MOTMaster;
using MOTMaster.SnippetLibrary;

using System;
using System.Collections.Generic;

using DAQ.Pattern;
using DAQ.Analog;

public class Patterns : MOTMasterScript
{
    public Patterns()
    {
        Parameters = new Dictionary<string, object>();
        Parameters["PatternLength"] = 100000;

        // Camera
        Parameters["Frame0TriggerDuration"] = 10;

        Parameters["LoadingDuration"] = 50000;
        Parameters["ExpansionTime"] = 300;
        Parameters["MOTCoilsCurrentValue"] = 0.32;
    }

    public override PatternBuilder32 GetDigitalPattern()
    {
        PatternBuilder32 p = new PatternBuilder32();
        int motSwitchOffTime = (int)Parameters["LoadingDuration"];
        int imageTime = motSwitchOffTime + (int)Parameters["ExpansionTime"];

        p.Pulse(0, 0, 10, "aoPatternTrigger");  //THIS TRIGGERS THE ANALOG PATTERN. The analog pattern will start at the same time as the Q-switch is fired.
        p.Pulse(0, motSwitchOffTime, (int)Parameters["ExpansionTime"], "rbCoolingAOM");
        p.Pulse(0, imageTime, (int)Parameters["Frame0TriggerDuration"], "cameraTrigger2");


        return p;
    }

    public override AnalogPatternBuilder GetAnalogPattern()
    {
        AnalogPatternBuilder p = new AnalogPatternBuilder((int)Parameters["PatternLength"]);

        int motSwitchOffTime = (int)Parameters["LoadingDuration"];
        int imageTime = motSwitchOffTime + (int)Parameters["ExpansionTime"];

        // Add Analog Channels

        p.AddChannel("MOTCoilsCurrent");

        // B Field
        p.AddAnalogValue("MOTCoilsCurrent", 0, (double)Parameters["MOTCoilsCurrentValue"]);
        p.AddAnalogValue("MOTCoilsCurrent", motSwitchOffTime, 0.0);

        return p;
    }

}
