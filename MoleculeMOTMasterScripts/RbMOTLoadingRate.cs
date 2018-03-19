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
        Parameters["PatternLength"] = 500000;

        // Camera
        Parameters["Frame0TriggerDuration"] = 10;

        Parameters["LoadingDuration"] = 250000;
        Parameters["MOTCoilsCurrentValue"] = 0.32;
    }

    public override PatternBuilder32 GetDigitalPattern()
    {
        PatternBuilder32 p = new PatternBuilder32();
        int cameraTriggerTime = (int)Parameters["LoadingDuration"];

        p.Pulse(0, 0, 10, "aoPatternTrigger");  //THIS TRIGGERS THE ANALOG PATTERN. The analog pattern will start at the same time as the Q-switch is fired.
        p.Pulse(0, cameraTriggerTime, (int)Parameters["Frame0TriggerDuration"], "cameraTrigger");

          
        return p;
    }

    public override AnalogPatternBuilder GetAnalogPattern()
    {
        AnalogPatternBuilder p = new AnalogPatternBuilder((int)Parameters["PatternLength"]);

        int cameraTriggerTime = (int)Parameters["LoadingDuration"];

        // Add Analog Channels
        
        p.AddChannel("MOTCoilsCurrent");

        // B Field
        p.AddAnalogValue("MOTCoilsCurrent", 0, (double)Parameters["MOTCoilsCurrentValue"]);
        p.AddAnalogValue("MOTCoilsCurrent", cameraTriggerTime + 100, 0.0);

        return p;
   }

}
