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
        Parameters["Frame0Trigger"] = 5500;
        Parameters["Frame0TriggerDuration"] = 10;

        //PMT
        Parameters["PMTTrigger"] = 4000;
        Parameters["PMTTriggerDuration"] = 10;

        // Slowing
        Parameters["slowingAOMOnStart"] = 250;
        Parameters["slowingAOMOnDuration"] = 45000;
        Parameters["slowingAOMOffStart"] = 1500;
        Parameters["slowingAOMOffDuration"] = 35000;
        Parameters["slowingRepumpAOMOnStart"] = 0;
        Parameters["slowingRepumpAOMOnDuration"] = 45000;
        Parameters["slowingRepumpAOMOffStart"] = 1700;
        Parameters["slowingRepumpAOMOffDuration"] = 35000;

        // Slowing Chirp
        Parameters["SlowingChirpStartTime"] = 340;
        Parameters["SlowingChirpDuration"] = 1160;
        Parameters["SlowingChirpStartValue"] = 0.0;
        Parameters["SlowingChirpEndValue"] = -1.3;

        // Slowing field
        Parameters["slowingCoilsValue"] = 1.1;
        Parameters["slowingCoilsOffTime"] = 1500;

        // B Field
        Parameters["MOTCoilsSwitchOn"] = 0;
        Parameters["MOTCoilsSwitchOff"] = 6500;
        Parameters["MOTCoilsCurrentRampStartTime"] = 4000;
        Parameters["MOTCoilsCurrentRampDuration"] = 1000;
        Parameters["MOTCoilsCurrentTopRampStartValue"] = 0.65;
        Parameters["MOTCoilsCurrentTopRampEndValue"] = 1.5;
        Parameters["MOTCoilsCurrentBottomRampStartValue"] = 0.65;
        Parameters["MOTCoilsCurrentBottomRampEndValue"] = 1.5;

        // Shim fields
        Parameters["xShimLoadCurrent"] = 1.7;
        Parameters["yShimLoadCurrent"] = 0.0;

        // v0 Light Switch
        Parameters["MOTAOMStartTime"] = 15000;
        Parameters["MOTAOMDuration"] = 500;

        // v0 Light Intensity
        Parameters["v0IntensityRampStartTime"] = 5000;
        Parameters["v0IntensityRampDuration"] = 2000;
        Parameters["v0IntensityRampStartValue"] = 5.0;
        Parameters["v0IntensityRampEndValue"] = 5.0;

        // v0 Light Frequency
        Parameters["v0FrequencyRampStartTime"] = 10000;
        Parameters["v0FrequencyRampDuration"] = 2000;
        Parameters["v0FrequencyRampStartValue"] = 9.0;
        Parameters["v0FrequencyRampEndValue"] = 9.0;

        // triggering delay (10V = 1 second)
        // Parameters["triggerDelay"] = 5.0;


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
        p.AddChannel("yShimCoilCurrent");

        // Slowing field
        p.AddAnalogValue("slowingCoilsCurrent", 0, (double)Parameters["slowingCoilsValue"]);
        p.AddAnalogValue("slowingCoilsCurrent", (int)Parameters["slowingCoilsOffTime"], 0.0);

        // B Field
        p.AddAnalogValue("MOTCoilsCurrentTop", (int)Parameters["MOTCoilsSwitchOn"], (double)Parameters["MOTCoilsCurrentTopRampStartValue"]);
        p.AddLinearRamp("MOTCoilsCurrentTop", (int)Parameters["MOTCoilsCurrentRampStartTime"], (int)Parameters["MOTCoilsCurrentRampDuration"], (double)Parameters["MOTCoilsCurrentTopRampEndValue"]);
        p.AddAnalogValue("MOTCoilsCurrentTop", (int)Parameters["MOTCoilsSwitchOff"], 0.0);
        p.AddAnalogValue("MOTCoilsCurrentBottom", (int)Parameters["MOTCoilsSwitchOn"], (double)Parameters["MOTCoilsCurrentBottomRampStartValue"]);
        p.AddLinearRamp("MOTCoilsCurrentBottom", (int)Parameters["MOTCoilsCurrentRampStartTime"], (int)Parameters["MOTCoilsCurrentRampDuration"], (double)Parameters["MOTCoilsCurrentBottomRampEndValue"]);
        p.AddAnalogValue("MOTCoilsCurrentBottom", (int)Parameters["MOTCoilsSwitchOff"], 0.0);

        // Shim Fields
        p.AddAnalogValue("xShimCoilCurrent", 0, (double)Parameters["xShimLoadCurrent"]);
        p.AddAnalogValue("yShimCoilCurrent", 0, (double)Parameters["yShimLoadCurrent"]);

        // trigger delay
        // p.AddAnalogValue("triggerDelay", 0, (double)Parameters["triggerDelay"]);

        // v0 Intensity Ramp
        p.AddAnalogValue("v0IntensityRamp", 0, (double)Parameters["v0IntensityRampStartValue"]);

        // v0 Frequency Ramp
        p.AddAnalogValue("v0FrequencyRamp", 0, (double)Parameters["v0FrequencyRampStartValue"]);



        p.SwitchAllOffAtEndOfPattern();
        return p;
    }

}
