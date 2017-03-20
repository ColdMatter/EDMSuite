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
        Parameters["FlashToQ"] = 16; // This is a time before the Q switch
        Parameters["QSwitchPulseDuration"] = 10;
        Parameters["FlashPulseDuration"] = 10;

        // Camera
        Parameters["Frame0Trigger"] = 450;
        Parameters["Frame0TriggerDuration"] = 10;

        //PMT
        Parameters["PMTTrigger"] = 4000;
        Parameters["PMTTriggerDuration"] = 10;

        // Slowing
        Parameters["slowingAOMOnStart"] = 450;
        Parameters["slowingAOMOnDuration"] = 45000;
        Parameters["slowingAOMOffStart"] = 650;
        Parameters["slowingAOMOffDuration"] = 35000;
        Parameters["slowingRepumpAOMOnStart"] = 0;
        Parameters["slowingRepumpAOMOnDuration"] = 45000;
        Parameters["slowingRepumpAOMOffStart"] = 1700;
        Parameters["slowingRepumpAOMOffDuration"] = 35000;

        // Slowing Chirp
        Parameters["SlowingChirpStartTime"] = 340;
        Parameters["SlowingChirpDuration"] = 1160;
        Parameters["SlowingChirpStartValue"] = 0.0;
        Parameters["SlowingChirpEndValue"] = 0.0;

        // B Field
        Parameters["MOTCoilsSwitchOn"] = 10000;
        Parameters["MOTCoilsSwitchOff"] = 20000;
        Parameters["MOTCoilsRampActive"] = false;
        Parameters["MOTCoilsCurrentRampStartTime"] = 1500;
        Parameters["MOTCoilsCurrentRampDuration"] = 1000;
        Parameters["MOTCoilsCurrentRampStartValue"] = 0.8;
        Parameters["MOTCoilsCurrentRampEndValue"] = 1.2;

        Parameters["MOTBOPCoilsCurrentStartValue"] = 10.0;
        Parameters["MOTBOPCoilsCurrentMolassesValue"] = 0.2;

        // Shim fields
        Parameters["xShimLoadCurrent"] = -3.9;
        Parameters["zShimLoadCurrent"] = 1.9;

        // v0 Light Switch
        Parameters["MOTAOMStartTime"] = 15000;
        Parameters["MOTAOMDuration"] = 500;

        // v0 Light Intensity
        Parameters["v0IntensityRampStartTime"] = 5000;
        Parameters["v0IntensityRampDuration"] = 2000;
        Parameters["v0IntensityRampStartValue"] = 5.0;
        Parameters["v0IntensityRampEndValue"] = 10.0;

        // v0 Light Frequency
        Parameters["v0FrequencyRampStartTime"] = 10000;
        Parameters["v0FrequencyRampDuration"] = 2000;
        Parameters["v0FrequencyRampStartValue"] = 9.0;
        Parameters["v0FrequencyRampEndValue"] = 9.0;


    }

    public override PatternBuilder32 GetDigitalPattern()
    {
        PatternBuilder32 p = new PatternBuilder32();
        int patternStartBeforeQ = (int)Parameters["TCLBlockStart"];

        MOTMasterScriptSnippet lm = new LoadMoleculeMOT(p, Parameters);  // This is how you load "preset" patterns.          

        p.Pulse(patternStartBeforeQ, (int)Parameters["Frame0Trigger"], (int)Parameters["Frame0TriggerDuration"], "cameraTrigger"); //camera trigger for first frame


        return p;
    }

    public override AnalogPatternBuilder GetAnalogPattern()
    {
        AnalogPatternBuilder p = new AnalogPatternBuilder((int)Parameters["PatternLength"]);

        MOTMasterScriptSnippet lm = new LoadMoleculeMOT(p, Parameters);

        // Add Analog Channels

        p.AddChannel("v0IntensityRamp");
        p.AddChannel("v0FrequencyRamp");
        p.AddChannel("xShimCoilCurrent");
        p.AddChannel("zShimCoilCurrent");


        // B Field
        // For the delta electronica box (bottom MOT coil) - top coil is in digital section
        p.AddAnalogValue("MOTCoilsCurrent", (int)Parameters["MOTCoilsSwitchOn"], (double)Parameters["MOTCoilsCurrentRampStartValue"]);
        p.AddAnalogValue("MOTCoilsCurrent", (int)Parameters["MOTCoilsSwitchOff"], 0);

        // For BOP
        p.AddAnalogValue("MOTBOPCoilsCurrent", (int)Parameters["MOTCoilsSwitchOn"], (double)Parameters["MOTBOPCoilsCurrentStartValue"]);
        p.AddAnalogValue("MOTBOPCoilsCurrent", (int)Parameters["MOTCoilsSwitchOff"], (double)Parameters["MOTBOPCoilsCurrentMolassesValue"]);


        // Shim Fields
        p.AddAnalogValue("xShimCoilCurrent", 0, (double)Parameters["xShimLoadCurrent"]);
        p.AddAnalogValue("zShimCoilCurrent", 0, (double)Parameters["zShimLoadCurrent"]);

        // v0 Intensity Ramp
        p.AddAnalogValue("v0IntensityRamp", 0, (double)Parameters["v0IntensityRampStartValue"]);

        // v0 Frequency Ramp
        p.AddAnalogValue("v0FrequencyRamp", 0, (double)Parameters["v0FrequencyRampStartValue"]);



        p.SwitchAllOffAtEndOfPattern();
        return p;
    }

}
