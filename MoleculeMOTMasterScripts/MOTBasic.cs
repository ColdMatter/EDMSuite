using MOTMaster;
using MOTMaster.SnippetLibrary;

using System;
using System.Collections.Generic;

using DAQ.Pattern;
using DAQ.Analog;

// This script is supposed to be the basic script for loading a molecule MOT
public class Patterns : MOTMasterScript
{
    public Patterns()
    {
        Parameters = new Dictionary<string, object>();
        Parameters["PatternLength"] = 50000;
        Parameters["FlashToQ"] = 16;
        Parameters["QSwitchPulseDuration"] = 10;
        Parameters["FlashPulseDuration"] = 10;
        Parameters["MOTStartTime"] = 1000;
        Parameters["MOTCoilsCurrent"] = 10.0;
        Parameters["NumberOfFrames"] = 2;
        Parameters["Frame0TriggerDuration"] = 100;
        Parameters["Frame0Trigger"] = 110000;
       
        
    }

    public override PatternBuilder32 GetDigitalPattern()
    {
        PatternBuilder32 p = new PatternBuilder32();

        p.Pulse(0, 0, 10, "AnalogPatternTrigger");  //NEVER CHANGE THIS!!!! IT TRIGGERS THE ANALOG PATTERN!#

        p.Pulse(0, 0, (int)Parameters["QSwitchPulseDuration"], "flash"); //trigger the flashlamp
        p.Pulse(0, (int)Parameters["FlashToQ"], (int)Parameters["QSwitchPulseDuration"], "q"); //trigger the Q switch


        return p;
    }

    public override AnalogPatternBuilder GetAnalogPattern()
    {
        AnalogPatternBuilder p = new AnalogPatternBuilder((int)Parameters["PatternLength"]);
        p.AddChannel("slowingChirp");

        p.AddAnalogValue("slowingChirp", 0, 2);
    //    p.AddAnalogValue("slowingChirp", 11, 3);
   //     p.AddAnalogValue("slowingChirp", 12, 4);
       // p.AddAnalogValue("slowingChirp", 5000, 0);

        p.SwitchAllOffAtEndOfPattern();
        return p;
    }

}
