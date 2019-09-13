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

        Parameters["MOTSwitchOffTime"] = 6300;
        Parameters["ResonantMolassesDuration"] = 1500;
        Parameters["MolassesDelay"] = 100;
        Parameters["MolassesDuration"] = 200;

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

        // Slowing field
        Parameters["slowingCoilsValue"] = 1.1;
        Parameters["slowingCoilsOffTime"] = 1500;

        // B Field
        Parameters["MOTCoilsSwitchOn"] = 0;
        Parameters["MOTCoilsCurrentTopRampStartValue"] = 0.65;
        Parameters["MOTCoilsCurrentBottomRampStartValue"] = 0.65;
        Parameters["MOTCoilsCurrentRampStartTime"] = 4000;
        Parameters["MOTCoilsCurrentTopRampEndValue"] = 1.5;
        Parameters["MOTCoilsCurrentBottomRampEndValue"] = 1.5;
        Parameters["MOTCoilsCurrentRampDuration"] = 1000;
        Parameters["MOTCoilsCurrentTopMolassesValue"] = 0.0; //0.06
        Parameters["MOTCoilsCurrentBottomMolassesValue"] = 0.0;
        Parameters["CoilsSwitchOffTime"] = 20000;

        // Shim fields
        Parameters["xShimLoadCurrent"] = 0.0;
        Parameters["yShimLoadCurrent"] = 0.0;
        Parameters["zShimLoadCurrent"] = -0.16;

        // v0 Light Intensity
        Parameters["v0IntensityRampStartTime"] = 5500;
        Parameters["v0IntensityRampDuration"] = 400;
        Parameters["v0IntensityRampStartValue"] = 5.0;
        Parameters["v0IntensityRampEndValue"] = 7.95;
        Parameters["v0IntensityMolassesValue"] = 5.0;
        Parameters["v0IntensityResonantMolassesValue"] = 8.75;

        // v0 Light Frequency
        Parameters["v0FrequencyMOTValue"] = 0.0; //set this to 0.0 for 114.1MHz 
        Parameters["v0FrequencyBlueMolassesValue"] = 30.0; //set this to MHz detuning desired if doing frequency jump (positive for blue detuning)
        Parameters["v0FrequencyResonantMolassesValue"] = 0.0; //set this to MHz detuning desired if doing frequency jump (positive for blue detuning)

        //v0aomCalibrationValues
        Parameters["lockAomFrequency"] = 114.1;
        Parameters["calibOffset"] = 64.2129;
        Parameters["calibGradient"] = 5.55075;

        // v0 F=1 (dodgy code using an analogue output to control a TTL)
        Parameters["v0F1AOMStartValue"] = 5.0;
        Parameters["v0F1AOMOffValue"] = 0.0;
    }

    public override PatternBuilder32 GetDigitalPattern()
    {
        PatternBuilder32 p = new PatternBuilder32();
        int patternStartBeforeQ = (int)Parameters["TCLBlockStart"];
        int molassesStartTime = (int)Parameters["MOTSwitchOffTime"] + (int)Parameters["MolassesDelay"];
        int resonantMolassesStartTime = molassesStartTime + (int)Parameters["MolassesDuration"];
        int cameraTriggerTime = resonantMolassesStartTime + (int)Parameters["ResonantMolassesDuration"];

        MOTMasterScriptSnippet lm = new LoadMoleculeMOT(p, Parameters);  // This is how you load "preset" patterns. 

        p.Pulse(patternStartBeforeQ, (int)Parameters["MOTSwitchOffTime"], (int)Parameters["MolassesDelay"], "motAOM"); //pulse off the MOT light whilst MOT fields are turning off
        p.Pulse(patternStartBeforeQ, cameraTriggerTime, (int)Parameters["Frame0TriggerDuration"], "cameraTrigger"); //camera trigger for first frame

        return p;
    }

    public override AnalogPatternBuilder GetAnalogPattern()
    {
        AnalogPatternBuilder p = new AnalogPatternBuilder((int)Parameters["PatternLength"]);

        MOTMasterScriptSnippet lm = new LoadMoleculeMOT(p, Parameters);

        int molassesStartTime = (int)Parameters["MOTSwitchOffTime"] + (int)Parameters["MolassesDelay"];
        int resonantMolassesStartTime = molassesStartTime + (int)Parameters["MolassesDuration"];
        int cameraTriggerTime = resonantMolassesStartTime + (int)Parameters["ResonantMolassesDuration"];

        // Add Analog Channels
        p.AddChannel("v0IntensityRamp");
        p.AddChannel("v0FrequencyRamp");
        p.AddChannel("xShimCoilCurrent");
        p.AddChannel("yShimCoilCurrent");
        p.AddChannel("triggerDelay");
        p.AddChannel("motAOMAmp");

        // Slowing field
        p.AddAnalogValue("slowingCoilsCurrent", 0, (double)Parameters["slowingCoilsValue"]);
        p.AddAnalogValue("slowingCoilsCurrent", (int)Parameters["slowingCoilsOffTime"], 0.0);

        // B Field
        p.AddAnalogValue("MOTCoilsCurrentTop", (int)Parameters["MOTCoilsSwitchOn"], (double)Parameters["MOTCoilsCurrentTopRampStartValue"]);
        p.AddLinearRamp("MOTCoilsCurrentTop", (int)Parameters["MOTCoilsCurrentRampStartTime"], (int)Parameters["MOTCoilsCurrentRampDuration"], (double)Parameters["MOTCoilsCurrentTopRampEndValue"]);
        p.AddAnalogValue("MOTCoilsCurrentTop", (int)Parameters["MOTSwitchOffTime"], (double)Parameters["MOTCoilsCurrentTopMolassesValue"]);
        p.AddAnalogValue("MOTCoilsCurrentTop", (int)Parameters["CoilsSwitchOffTime"], 0.0);
        p.AddAnalogValue("MOTCoilsCurrentBottom", (int)Parameters["MOTCoilsSwitchOn"], (double)Parameters["MOTCoilsCurrentBottomRampStartValue"]);
        p.AddLinearRamp("MOTCoilsCurrentBottom", (int)Parameters["MOTCoilsCurrentRampStartTime"], (int)Parameters["MOTCoilsCurrentRampDuration"], (double)Parameters["MOTCoilsCurrentBottomRampEndValue"]);
        p.AddAnalogValue("MOTCoilsCurrentBottom", (int)Parameters["MOTSwitchOffTime"], (double)Parameters["MOTCoilsCurrentBottomMolassesValue"]);
        p.AddAnalogValue("MOTCoilsCurrentBottom", (int)Parameters["CoilsSwitchOffTime"], 0.0);

        // Shim Fields
        p.AddAnalogValue("xShimCoilCurrent", 0, (double)Parameters["xShimLoadCurrent"]);
        p.AddAnalogValue("yShimCoilCurrent", 0, (double)Parameters["yShimLoadCurrent"]);
        p.AddAnalogValue("triggerDelay", 0, (double)Parameters["zShimLoadCurrent"]);

        // v0 Intensity Ramp
        p.AddAnalogValue("v0IntensityRamp", 0, (double)Parameters["v0IntensityRampStartValue"]);
        p.AddLinearRamp("v0IntensityRamp", (int)Parameters["v0IntensityRampStartTime"], (int)Parameters["v0IntensityRampDuration"], (double)Parameters["v0IntensityRampEndValue"]);
        p.AddAnalogValue("v0IntensityRamp", molassesStartTime, (double)Parameters["v0IntensityMolassesValue"]);
        p.AddAnalogValue("v0IntensityRamp", molassesStartTime + 50, 6.76);
        p.AddAnalogValue("v0IntensityRamp", molassesStartTime + 100, 7.24);
        p.AddAnalogValue("v0IntensityRamp", molassesStartTime + 150, 7.54);
        p.AddAnalogValue("v0IntensityRamp", resonantMolassesStartTime, (double)Parameters["v0IntensityResonantMolassesValue"]);
        p.AddAnalogValue("v0IntensityRamp", cameraTriggerTime, (double)Parameters["v0IntensityRampStartValue"]);

        // v0 F=1 AOM
        p.AddAnalogValue("motAOMAmp", 0, (double)Parameters["v0F1AOMStartValue"]);
        // Turn off F=1 AOM for pumping into F=1
        p.AddAnalogValue("motAOMAmp", resonantMolassesStartTime, (double)Parameters["v0F1AOMOffValue"]);
        // Turn back on for imaging
        p.AddAnalogValue("motAOMAmp", cameraTriggerTime, (double)Parameters["v0F1AOMStartValue"]);

        // v0 Frequency Ramp
        p.AddAnalogValue("v0FrequencyRamp", 0, ((double)Parameters["lockAomFrequency"] - (double)Parameters["v0FrequencyMOTValue"] / 2 - (double)Parameters["calibOffset"]) / (double)Parameters["calibGradient"]);
        p.AddAnalogValue("v0FrequencyRamp", molassesStartTime, ((double)Parameters["lockAomFrequency"] - (double)Parameters["v0FrequencyBlueMolassesValue"] / 2 - (double)Parameters["calibOffset"]) / (double)Parameters["calibGradient"]);//jump to blue detuning
        p.AddAnalogValue("v0FrequencyRamp", resonantMolassesStartTime, ((double)Parameters["lockAomFrequency"] - (double)Parameters["v0FrequencyResonantMolassesValue"] / 2 - (double)Parameters["calibOffset"]) / (double)Parameters["calibGradient"]);//jump to blue detuning
        p.AddAnalogValue("v0FrequencyRamp", cameraTriggerTime, ((double)Parameters["lockAomFrequency"] - (double)Parameters["v0FrequencyMOTValue"] / 2 - (double)Parameters["calibOffset"]) / (double)Parameters["calibGradient"]); //jump aom frequency back to normal for imaging 

        p.SwitchAllOffAtEndOfPattern();
        return p;
    }

}
