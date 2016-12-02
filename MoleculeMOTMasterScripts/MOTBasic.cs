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
        Parameters["TCLBlockStart"] = 2000; // This is a time before the Q switch
        Parameters["TCLBlockDuration"] = 48000;
        Parameters["FlashToQ"] = 16; // This is a time before the Q switch
        Parameters["QSwitchPulseDuration"] = 10;
        Parameters["FlashPulseDuration"] = 10;
        Parameters["slowingAOMOnStart"] = 250;
        Parameters["slowingAOMOnDuration"] = 45000;
        Parameters["slowingAOMOffStart"] = 1500;
        Parameters["slowingAOMOffDuration"] = 35000;
        Parameters["slowingRepumpAOMOnStart"] = 250;
        Parameters["slowingRepumpAOMOnDuration"] = 45000;
        Parameters["slowingRepumpAOMOffStart"] = 1500;
        Parameters["slowingRepumpAOMOffDuration"] = 35000;
        Parameters["MOTAOMStartTime"] = 4000;
        Parameters["MOTAOMDuration"] = 5000;
        Parameters["BSwitchOn"] = 0;
        Parameters["BSwitchDuration"] = 35000;
        Parameters["MOTCoilsCurrent"] = 10.0;
        Parameters["Frame0Trigger"] = 2000;
        Parameters["Frame0TriggerDuration"] = 10;

        Parameters["SlowingChirpStartTime"] = 340;
        Parameters["SlowingChirpDuration"] = 1160;
        Parameters["SlowingChirpStartValue"] = 0.0;
        Parameters["SlowingChirpEndValue"] = -1.3;

        Parameters["v0IntensityRampStartTime"] = 100;
        Parameters["v0IntensityRampDuration"] = 2000;
        Parameters["v0IntensityRampStartValue"] = 4.0;
        Parameters["v0IntensityRampEndValue"] = 1.0;
       
       
        
    }

    public override PatternBuilder32 GetDigitalPattern()
    {
        PatternBuilder32 p = new PatternBuilder32();
        int patternStartBeforeQ = (int)Parameters["TCLBlockStart"];

        p.Pulse(0, 0, (int)Parameters["TCLBlockDuration"], "tclBlock");
        p.Pulse(patternStartBeforeQ, -(int)Parameters["FlashToQ"], (int)Parameters["QSwitchPulseDuration"], "flash"); //trigger the flashlamp
        p.Pulse(patternStartBeforeQ, 0, 10, "AnalogPatternTrigger");  //THIS TRIGGERS THE ANALOG PATTERN. The analog pattern will start at the same time as the Q-switch is fired.
        p.Pulse(patternStartBeforeQ, 0, (int)Parameters["QSwitchPulseDuration"], "q"); //trigger the Q switch
        p.Pulse(patternStartBeforeQ, (int)Parameters["slowingAOMOnStart"], (int)Parameters["slowingAOMOffStart"] - (int)Parameters["slowingAOMOnStart"], "aom"); //first pulse to slowing AOM
        p.Pulse(patternStartBeforeQ, (int)Parameters["slowingAOMOffStart"] + (int)Parameters["slowingAOMOffDuration"], (int)Parameters["slowingAOMOnDuration"] - ((int)Parameters["slowingAOMOffStart"] - (int)Parameters["slowingAOMOnStart"])  - (int)Parameters["slowingAOMOffDuration"], "aom"); //second pulse to slowing AOM
        p.Pulse(patternStartBeforeQ, (int)Parameters["slowingRepumpAOMOnStart"], (int)Parameters["slowingRepumpAOMOffStart"] - (int)Parameters["slowingRepumpAOMOnStart"], "aom2"); //first pulse to slowing repump AOM
        p.Pulse(patternStartBeforeQ, (int)Parameters["slowingRepumpAOMOffStart"] + (int)Parameters["slowingRepumpAOMOffDuration"], (int)Parameters["slowingRepumpAOMOnDuration"] - ((int)Parameters["slowingRepumpAOMOffStart"] - (int)Parameters["slowingRepumpAOMOnStart"]) - (int)Parameters["slowingRepumpAOMOffDuration"], "aom2"); //second pulse to slowing repump AOM
        p.Pulse(patternStartBeforeQ, (int)Parameters["MOTAOMStartTime"], (int)Parameters["MOTAOMDuration"], "motAOM"); //pulse off the MOT light to release the cloud
        p.Pulse(patternStartBeforeQ, (int)Parameters["BSwitchOn"], (int)Parameters["BSwitchDuration"], "bTrigger"); //B field pulse
        p.Pulse(patternStartBeforeQ, (int)Parameters["Frame0Trigger"], (int)Parameters["Frame0TriggerDuration"], "cameraTrigger"); //camera trigger for first frame
        
        return p;
    }

    public override AnalogPatternBuilder GetAnalogPattern()
    {
        AnalogPatternBuilder p = new AnalogPatternBuilder((int)Parameters["PatternLength"]);
        p.AddChannel("slowingChirp");
        p.AddChannel("v0IntensityRamp");

        p.AddAnalogValue("slowingChirp", 0, (double)Parameters["SlowingChirpStartValue"]);
        p.AddLinearRamp("slowingChirp", (int)Parameters["SlowingChirpStartTime"], (int)Parameters["SlowingChirpDuration"], (double)Parameters["SlowingChirpEndValue"]);
        p.AddLinearRamp("slowingChirp", (int)Parameters["SlowingChirpStartTime"] + (int)Parameters["SlowingChirpDuration"], (int)Parameters["SlowingChirpDuration"], (double)Parameters["SlowingChirpStartValue"]);

       // p.AddAnalogValue("v0IntensityRamp", 0, (double)Parameters["v0IntensityRampStartValue"]);
       // p.AddLinearRamp("v0IntensityRamp", (int)Parameters["v0IntensityRampStartTime"], (int)Parameters["v0IntensityRampDuration"], (double)Parameters["v0IntensityRampEndValue"]);


        //p.SwitchAllOffAtEndOfPattern();
        return p;
   }

}
