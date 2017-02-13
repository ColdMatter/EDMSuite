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

        Parameters["ExpansionTime"] = 200;

        // Camera
        Parameters["Frame0Trigger"] = 8000 + (int)Parameters["ExpansionTime"];
        Parameters["Frame0TriggerDuration"] = 10;

        // Slowing
        Parameters["slowingAOMOnStart"] = 250;
        Parameters["slowingAOMOnDuration"] = 45000;
        Parameters["slowingAOMOffStart"] = 1500;
        Parameters["slowingAOMOffDuration"] = 35000;
        Parameters["slowingRepumpAOMOnStart"] = 250;
        Parameters["slowingRepumpAOMOnDuration"] = 45000;
        Parameters["slowingRepumpAOMOffStart"] = 1500;
        Parameters["slowingRepumpAOMOffDuration"] = 35000;

        // Slowing Chirp
        Parameters["SlowingChirpStartTime"] = 340;
        Parameters["SlowingChirpDuration"] = 1160;
        Parameters["SlowingChirpStartValue"] = 0.0;
        Parameters["SlowingChirpEndValue"] = -1.2;

        // B Field
        Parameters["MOTCoilsSwitchOn"] = 0;
        Parameters["MOTCoilsSwitchOff"] = 8000;
        Parameters["MOTCoilsRampActive"] = false;
        Parameters["MOTCoilsCurrentRampStartTime"] = 6000;
        Parameters["MOTCoilsCurrentRampDuration"] = 2000;
        Parameters["MOTCoilsCurrentRampStartValue"] = 0.8;
        Parameters["MOTCoilsCurrentRampEndValue"] = 1.2;


        // v0 Light Switch
        Parameters["MOTAOMStartTime"] = 9500;
        Parameters["MOTAOMDuration"] = Parameters["ExpansionTime"];

        // v0 Light Intensity
        Parameters["v0IntensityRampStartTime"] = 5000;
        Parameters["v0IntensityRampDuration"] = 2000;
        Parameters["v0IntensityRampStartValue"] = 5.0;
        Parameters["v0IntensityRampEndValue"] = 10.0;

        // v0 Light Frequency
        Parameters["v0FrequencyRampStartTime"] = 10000;
        Parameters["v0FrequencyRampDuration"] = 2000;
        Parameters["v0FrequencyRampStartValue"] = 8.35;
        Parameters["v0FrequencyRampEndValue"] = 9.10; 
       
        
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
        p.Pulse(patternStartBeforeQ, (int)Parameters["MOTCoilsSwitchOn"], (int)Parameters["MOTCoilsSwitchOff"] - (int)Parameters["MOTCoilsSwitchOn"], "bTrigger"); //B field pulse
        p.Pulse(patternStartBeforeQ, (int)Parameters["Frame0Trigger"], (int)Parameters["Frame0TriggerDuration"], "cameraTrigger"); //camera trigger for first frame
        
        return p;
    }

    public override AnalogPatternBuilder GetAnalogPattern()
    {
        AnalogPatternBuilder p = new AnalogPatternBuilder((int)Parameters["PatternLength"]);

        // Add Analog Channels
        p.AddChannel("slowingChirp");
        p.AddChannel("v0IntensityRamp");
        p.AddChannel("v0FrequencyRamp");
        p.AddChannel("MOTCoilsCurrent");

        // B Field
        p.AddAnalogValue("MOTCoilsCurrent", (int)Parameters["MOTCoilsSwitchOn"], (double)Parameters["MOTCoilsCurrentRampStartValue"]);
        if ((bool)Parameters["MOTCoilsRampActive"])
        {
            p.AddLinearRamp("MOTCoilsCurrent", (int)Parameters["MOTCoilsCurrentRampStartTime"], (int)Parameters["MOTCoilsCurrentRampDuration"], (double)Parameters["MOTCoilsCurrentRampEndValue"]);
        }
        p.AddAnalogValue("MOTCoilsCurrent", (int)Parameters["MOTCoilsSwitchOff"], 0);
        
        // Slowing Chirp
        p.AddAnalogValue("slowingChirp", 0, (double)Parameters["SlowingChirpStartValue"]);
        p.AddLinearRamp("slowingChirp", (int)Parameters["SlowingChirpStartTime"], (int)Parameters["SlowingChirpDuration"], (double)Parameters["SlowingChirpEndValue"]);
        p.AddLinearRamp("slowingChirp", (int)Parameters["SlowingChirpStartTime"] + (int)Parameters["SlowingChirpDuration"], (int)Parameters["SlowingChirpDuration"], (double)Parameters["SlowingChirpStartValue"]);

        // v0 Intensity Ramp
        p.AddAnalogValue("v0IntensityRamp", 0, (double)Parameters["v0IntensityRampStartValue"]);
        p.AddLinearRamp("v0IntensityRamp", (int)Parameters["v0IntensityRampStartTime"], (int)Parameters["v0IntensityRampDuration"], (double)Parameters["v0IntensityRampEndValue"]);
        p.AddAnalogValue("v0IntensityRamp", (int)Parameters["MOTAOMStartTime"], (double)Parameters["v0IntensityRampStartValue"]);

        // v0 Frequency Ramp
        p.AddAnalogValue("v0FrequencyRamp", 0, (double)Parameters["v0FrequencyRampStartValue"]);
        p.AddLinearRamp("v0FrequencyRamp", (int)Parameters["v0FrequencyRampStartTime"], (int)Parameters["v0FrequencyRampDuration"], (double)Parameters["v0FrequencyRampEndValue"]);

        p.SwitchAllOffAtEndOfPattern();
        return p;
   }

}
