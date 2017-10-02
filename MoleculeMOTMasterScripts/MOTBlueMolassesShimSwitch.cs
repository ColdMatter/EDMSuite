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
        Parameters["TCLBlockDuration"] = 8000;
        Parameters["FlashToQ"] = 16; // This is a time before the Q switch
        Parameters["QSwitchPulseDuration"] = 10;
        Parameters["FlashPulseDuration"] = 10;

        Parameters["MOTSwitchOffTime"] = 8000;
        Parameters["ExpansionTime"] = 800;
        Parameters["MolassesDelay"] = 100;
        Parameters["MolassesDuration"] = 100;

        // Camera
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
        Parameters["slowingRepumpAOMOffStart"] = 1700;
        Parameters["slowingRepumpAOMOffDuration"] = 35000;

        // Slowing Chirp
        Parameters["SlowingChirpStartTime"] = 340;
        Parameters["SlowingChirpDuration"] = 1160;
        Parameters["SlowingChirpStartValue"] = 0.0;
        Parameters["SlowingChirpEndValue"] = -1.3;

        // B Field
        Parameters["MOTCoilsSwitchOn"] = 0;
        Parameters["MOTCoilsCurrentRampStartValue"] = 0.75;
        Parameters["MOTCoilsCurrentRampStartTime"] = 4000;
        Parameters["MOTCoilsCurrentRampEndValue"] = 0.75;
        Parameters["MOTCoilsCurrentRampDuration"] = 1000;

        // Shim fields
        Parameters["xShimLoadCurrent"] = 0.42;
        Parameters["yShimLoadCurrent"] = 0.0;
        Parameters["zShimLoadCurrent"] = -0.4;
        Parameters["xShimMolassesCurrent"] = 0.0;
        Parameters["yShimMolassesCurrent"] = 0.06;
        Parameters["zShimMolassesCurrent"] = 0.1;
        Parameters["zShimEddySupressTime"] = 7200;
        Parameters["zShimFieldSwitchTime"] = 7300;
        Parameters["shimFieldSwitchTime"] = 7300;

        // v0 Light Intensity
        Parameters["v0IntensityRampStartTime"] = 5500;
        Parameters["v0IntensityRampDuration"] = 2000;
        Parameters["v0IntensityRampStartValue"] = 5.0;
        Parameters["v0IntensityRampEndValue"] = 7.94;
        Parameters["v0IntensityMolassesValue"] = 7.24;

        // v0 Light Frequency
        Parameters["v0FrequencyStartValue"] = 0.0; //set this to 0.0 for 114.1MHz 
        Parameters["v0FrequencyNewValue"] = 30.0; //set this to MHz detuning desired if doing frequency jump (positive for blue detuning)

        //v0aomCalibrationValues
        Parameters["lockAomFrequency"] = 114.1;
        Parameters["calibOffset"] = 64.2129;
        Parameters["calibGradient"] = 5.55075;
       
        
    }

    public override PatternBuilder32 GetDigitalPattern()
    {
        PatternBuilder32 p = new PatternBuilder32();
        int patternStartBeforeQ = (int)Parameters["TCLBlockStart"];

        MOTMasterScriptSnippet lm = new LoadMoleculeMOT(p, Parameters);  // This is how you load "preset" patterns. 

        p.Pulse(patternStartBeforeQ, (int)Parameters["MOTSwitchOffTime"], (int)Parameters["MolassesDelay"], "motAOM"); //pulse off the MOT light whilst MOT fields are turning off
        p.Pulse(patternStartBeforeQ, (int)Parameters["MOTSwitchOffTime"] + (int)Parameters["MolassesDelay"] + (int)Parameters["MolassesDuration"], (int)Parameters["ExpansionTime"], "motAOM"); //pulse off the MOT light to release the cloud
        p.Pulse(patternStartBeforeQ, (int)Parameters["MOTSwitchOffTime"] + (int)Parameters["MolassesDelay"] + (int)Parameters["MolassesDuration"] + (int)Parameters["ExpansionTime"], (int)Parameters["Frame0TriggerDuration"], "cameraTrigger"); //camera trigger for first frame

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
        p.AddChannel("zShimCoilCurrent");

        // B Field
        // For the delta electronica box (bottom MOT coil) - top coil is in digital section
        p.AddAnalogValue("MOTCoilsCurrent", (int)Parameters["MOTCoilsSwitchOn"], (double)Parameters["MOTCoilsCurrentRampStartValue"]);
        p.AddLinearRamp("MOTCoilsCurrent", (int)Parameters["MOTCoilsCurrentRampStartTime"], (int)Parameters["MOTCoilsCurrentRampDuration"], (double)Parameters["MOTCoilsCurrentRampEndValue"]);
        p.AddAnalogValue("MOTCoilsCurrent", (int)Parameters["MOTSwitchOffTime"], 0);

        // Shim Fields
        double zShimEddySupressCurrent = (double)Parameters["zShimMolassesCurrent"] - 0.7*((double)Parameters["zShimLoadCurrent"] - (double)Parameters["zShimMolassesCurrent"]);
        p.AddAnalogValue("xShimCoilCurrent", 0, (double)Parameters["xShimLoadCurrent"]);
        p.AddAnalogValue("xShimCoilCurrent", (int)Parameters["shimFieldSwitchTime"], (double)Parameters["xShimMolassesCurrent"]);
        p.AddAnalogValue("yShimCoilCurrent", 0, (double)Parameters["yShimLoadCurrent"]);
        p.AddAnalogValue("yShimCoilCurrent", (int)Parameters["shimFieldSwitchTime"], (double)Parameters["yShimMolassesCurrent"]);
        p.AddAnalogValue("zShimCoilCurrent", 0, (double)Parameters["zShimLoadCurrent"]);
        p.AddAnalogValue("zShimCoilCurrent", (int)Parameters["zShimEddySupressTime"], zShimEddySupressCurrent);
        p.AddAnalogValue("zShimCoilCurrent", (int)Parameters["zShimFieldSwitchTime"], (double)Parameters["zShimMolassesCurrent"]);

        // v0 Intensity Ramp
        p.AddAnalogValue("v0IntensityRamp", 0, (double)Parameters["v0IntensityRampStartValue"]);
        p.AddLinearRamp("v0IntensityRamp", (int)Parameters["v0IntensityRampStartTime"], (int)Parameters["v0IntensityRampDuration"], (double)Parameters["v0IntensityRampEndValue"]);
        p.AddAnalogValue("v0IntensityRamp", (int)Parameters["MOTSwitchOffTime"] + (int)Parameters["MolassesDelay"], (double)Parameters["v0IntensityMolassesValue"]);
        p.AddAnalogValue("v0IntensityRamp", (int)Parameters["MOTSwitchOffTime"] + (int)Parameters["MolassesDelay"] + (int)Parameters["MolassesDuration"] + (int)Parameters["ExpansionTime"], (double)Parameters["v0IntensityRampStartValue"]);

        // v0 Frequency Ramp
        p.AddAnalogValue("v0FrequencyRamp", 0, ((double)Parameters["lockAomFrequency"] - (double)Parameters["v0FrequencyStartValue"] / 2 - (double)Parameters["calibOffset"])/(double)Parameters["calibGradient"]) ;

        p.AddAnalogValue("v0FrequencyRamp", (int)Parameters["MOTSwitchOffTime"] + (int)Parameters["MolassesDelay"], ((double)Parameters["lockAomFrequency"] - (double)Parameters["v0FrequencyNewValue"] / 2 - (double)Parameters["calibOffset"]) / (double)Parameters["calibGradient"]);//jump to blue detuning
        p.AddAnalogValue("v0FrequencyRamp", (int)Parameters["MOTSwitchOffTime"] + (int)Parameters["MolassesDelay"] + (int)Parameters["MolassesDuration"] + (int)Parameters["ExpansionTime"], ((double)Parameters["lockAomFrequency"] - (double)Parameters["v0FrequencyStartValue"] / 2 - (double)Parameters["calibOffset"]) / (double)Parameters["calibGradient"]); //jump aom frequency back to normal for imaging 

        p.SwitchAllOffAtEndOfPattern();
        return p;
   }

}
