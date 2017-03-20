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
        Parameters["TCLBlockDuration"] = 15000;
        Parameters["FlashToQ"] = 16; // This is a time before the Q switch
        Parameters["QSwitchPulseDuration"] = 10;
        Parameters["FlashPulseDuration"] = 10;

        Parameters["MOTSwitchOffTime"] = 7500;
        Parameters["ExpansionTime"] = 500;

        // Camera
        //Parameters["Frame0Trigger"] = 8000;
        Parameters["Frame0TriggerDuration"] = 10;

        //PMT
        Parameters["PMTTrigger"] = 4000;
        Parameters["PMTTriggerDuration"] = 10;

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
        Parameters["SlowingChirpEndValue"] = -1.3;

        // B Field
        Parameters["MOTCoilsSwitchOn"] = 0;
        Parameters["MOTCoilsRampActive"] = false;
        Parameters["MOTCoilsCurrentRampStartTime"] = 6000;
        Parameters["MOTCoilsCurrentRampDuration"] = 2000;
        Parameters["MOTCoilsCurrentRampStartValue"] = 0.75;
        Parameters["MOTCoilsCurrentRampEndValue"] = 1.2;

        Parameters["MOTBOPCoilsCurrentStartValue"] = 10.0;

        // v0 Light Intensity
        Parameters["v0IntensityValue"] = 5.0;

        // v0 Light Frequency
        Parameters["v0FrequencyLoadValue"] = 0.0; //set this to 0.0 for 114.1MHz 
        Parameters["v0FrequencyNewValue"] = 0.0; //set this to MHz detuning desired if doing frequency jump (positive for blue detuning)
        Parameters["v0FrequencyImagingValue"] = 0.0; //frequency used for taking the image

        //v0aomCalibrationValues
        Parameters["lockAomFrequency"] = 114.1;
        Parameters["calibOffset"] = 64.2129;
        Parameters["calibGradient"] = 5.55075;

        // Shim fields
        Parameters["xShimLoadCurrent"] = -3.9;
        Parameters["zShimLoadCurrent"] = 1.9;
    }

    public override PatternBuilder32 GetDigitalPattern()
    {
        PatternBuilder32 p = new PatternBuilder32();
        int patternStartBeforeQ = (int)Parameters["TCLBlockStart"];

        MOTMasterScriptSnippet lm = new LoadMoleculeMOT(p, Parameters);  // This is how you load "preset" patterns.          

        p.Pulse(patternStartBeforeQ, (int)Parameters["MOTSwitchOffTime"], (int)Parameters["ExpansionTime"], "motAOM"); //pulse off the MOT light to release the cloud
        p.Pulse(patternStartBeforeQ, (int)Parameters["MOTSwitchOffTime"] + (int)Parameters["ExpansionTime"], (int)Parameters["Frame0TriggerDuration"], "cameraTrigger"); //camera trigger for first frame

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
        p.AddAnalogValue("MOTCoilsCurrent", (int)Parameters["MOTSwitchOffTime"], 0);

        // For BOP
        p.AddAnalogValue("MOTBOPCoilsCurrent", (int)Parameters["MOTCoilsSwitchOn"], (double)Parameters["MOTBOPCoilsCurrentStartValue"]);
        p.AddAnalogValue("MOTBOPCoilsCurrent", (int)Parameters["MOTSwitchOffTime"], 0);

        // v0 Intensity Ramp
        p.AddAnalogValue("v0IntensityRamp", 0, (double)Parameters["v0IntensityValue"]);

        // Shim Fields
        p.AddAnalogValue("xShimCoilCurrent", 0, (double)Parameters["xShimLoadCurrent"]);
        p.AddAnalogValue("zShimCoilCurrent", 0, (double)Parameters["zShimLoadCurrent"]);

        // v0 Frequency Ramp
        p.AddAnalogValue("v0FrequencyRamp", 0, ((double)Parameters["lockAomFrequency"] - (double)Parameters["v0FrequencyLoadValue"] / 2 - (double)Parameters["calibOffset"]) / (double)Parameters["calibGradient"]);
        p.AddAnalogValue("v0FrequencyRamp", 5000, ((double)Parameters["lockAomFrequency"] - (double)Parameters["v0FrequencyNewValue"] / 2 - (double)Parameters["calibOffset"]) / (double)Parameters["calibGradient"]);
        p.AddAnalogValue("v0FrequencyRamp", (int)Parameters["MOTSwitchOffTime"] + (int)Parameters["ExpansionTime"], ((double)Parameters["lockAomFrequency"] - (double)Parameters["v0FrequencyImagingValue"] / 2 - (double)Parameters["calibOffset"]) / (double)Parameters["calibGradient"]);

        p.SwitchAllOffAtEndOfPattern();
        return p;
    }

}
