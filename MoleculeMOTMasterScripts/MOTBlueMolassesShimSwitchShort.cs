using MOTMaster;
using MOTMaster.SnippetLibrary;

using System;
using System.Collections.Generic;
using System.Collections;

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
        Parameters["ExpansionTime"] = 500;
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
        Parameters["slowingCoilsValue"] = 1.05;
        Parameters["slowingCoilsOffTime"] = 1500;

        // B Field
        Parameters["MOTCoilsSwitchOn"] = 0;
        Parameters["MOTCoilsCurrentRampStartValue"] = 0.65;
        Parameters["MOTCoilsCurrentRampStartTime"] = 4000;
        Parameters["MOTCoilsCurrentRampEndValue"] = 1.5;
        Parameters["MOTCoilsCurrentRampDuration"] = 1000;
        Parameters["MOTCoilsCurrentMolassesValue"] = 0.0; //0.06
        Parameters["CoilsSwitchOffTime"] = 20000;

        // Shim fields
        Parameters["xShimLoadCurrent"] = 0.0;
        Parameters["yShimLoadCurrent"] = 0.0;
        Parameters["zShimLoadCurrent"] = 8.5;
        Parameters["shimFieldSwitchTime"] = 6200;

        // v0 Light Intensity
        Parameters["v0IntensityRampStartTime"] = 5500;
        Parameters["v0IntensityRampDuration"] = 400;
        Parameters["v0IntensityRampStartValue"] = 5.0;
        Parameters["v0IntensityRampEndValue"] = 7.95;
        Parameters["v0IntensityMolassesValue"] = 5.0;

        // v0 Light Frequency
        Parameters["v0FrequencyStartValue"] = 0.0; //set this to 0.0 for 114.1MHz 
        Parameters["v0FrequencyNewValue"] = 30.0; //set this to MHz detuning desired if doing frequency jump (positive for blue detuning)

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
        int releaseTime = molassesStartTime + (int)Parameters["MolassesDuration"];
        int cameraTriggerTime = releaseTime + (int)Parameters["ExpansionTime"];

        MOTMasterScriptSnippet lm = new LoadMoleculeMOT(p, Parameters);  // This is how you load "preset" patterns. 

        p.AddEdge("v00Shutter", 0, true);
        p.Pulse(patternStartBeforeQ, (int)Parameters["MOTSwitchOffTime"], (int)Parameters["MolassesDelay"], "v00AOM"); //pulse off the MOT light whilst MOT fields are turning off
        p.Pulse(patternStartBeforeQ, (int)Parameters["MOTSwitchOffTime"] + (int)Parameters["MolassesDelay"] + (int)Parameters["MolassesDuration"], (int)Parameters["ExpansionTime"], "v00AOM"); //pulse off the MOT light to release the cloud
        p.Pulse(patternStartBeforeQ, cameraTriggerTime, (int)Parameters["Frame0TriggerDuration"], "cameraTrigger"); //camera trigger for first frame

        return p;
    }

    public override AnalogPatternBuilder GetAnalogPattern()
    {
        AnalogPatternBuilder p = new AnalogPatternBuilder((int)Parameters["PatternLength"]);

        MOTMasterScriptSnippet lm = new LoadMoleculeMOT(p, Parameters);

        int molassesStartTime = (int)Parameters["MOTSwitchOffTime"] + (int)Parameters["MolassesDelay"];
        int releaseTime = molassesStartTime + (int)Parameters["MolassesDuration"];
        int cameraTriggerTime = releaseTime + (int)Parameters["ExpansionTime"];

        // Add Analog Channels
        p.AddChannel("v00Intensity");
        p.AddChannel("v00Frequency");
        p.AddChannel("xShimCoilCurrent");
        p.AddChannel("yShimCoilCurrent");
        p.AddChannel("zShimCoilCurrent");
        p.AddChannel("v00EOMAmp");

        // Slowing field
        p.AddAnalogValue("slowingCoilsCurrent", 0, (double)Parameters["slowingCoilsValue"]);
        p.AddAnalogValue("slowingCoilsCurrent", (int)Parameters["slowingCoilsOffTime"], 0.0);

        // B Field
        p.AddAnalogValue("MOTCoilsCurrent", (int)Parameters["MOTCoilsSwitchOn"], (double)Parameters["MOTCoilsCurrentRampStartValue"]);
        p.AddLinearRamp("MOTCoilsCurrent", (int)Parameters["MOTCoilsCurrentRampStartTime"], (int)Parameters["MOTCoilsCurrentRampDuration"], (double)Parameters["MOTCoilsCurrentRampEndValue"]);
        p.AddAnalogValue("MOTCoilsCurrent", (int)Parameters["MOTSwitchOffTime"], (double)Parameters["MOTCoilsCurrentMolassesValue"]);
        p.AddAnalogValue("MOTCoilsCurrent", (int)Parameters["CoilsSwitchOffTime"], 0.0);

        // Shim Fields
        p.AddAnalogValue("xShimCoilCurrent", 0, (double)Parameters["xShimLoadCurrent"]);
        p.AddAnalogValue("yShimCoilCurrent", 0, (double)Parameters["yShimLoadCurrent"]);
        p.AddAnalogValue("zShimCoilCurrent", 0, (double)Parameters["zShimLoadCurrent"]);

        // v0 Intensity Ramp
        p.AddAnalogValue("v00Intensity", 0, (double)Parameters["v0IntensityRampStartValue"]);
        p.AddLinearRamp("v00Intensity", (int)Parameters["v0IntensityRampStartTime"], (int)Parameters["v0IntensityRampDuration"], (double)Parameters["v0IntensityRampEndValue"]);
        p.AddAnalogValue("v00Intensity", molassesStartTime, (double)Parameters["v0IntensityMolassesValue"]);
        p.AddAnalogValue("v00Intensity", molassesStartTime + 50, 6.76);
        p.AddAnalogValue("v00Intensity", molassesStartTime + 100, 7.24);
        p.AddAnalogValue("v00Intensity", molassesStartTime + 150, 7.54);
        p.AddAnalogValue("v00Intensity", cameraTriggerTime, (double)Parameters["v0IntensityRampStartValue"]);

        //F=0
        p.AddAnalogValue("v00EOMAmp", 0, 5.5);

        // v0 Frequency Ramp
        p.AddAnalogValue("v00Frequency", 0, ((double)Parameters["lockAomFrequency"] - (double)Parameters["v0FrequencyStartValue"] / 2 - (double)Parameters["calibOffset"]) / (double)Parameters["calibGradient"]);
        p.AddAnalogValue("v00Frequency", molassesStartTime, ((double)Parameters["lockAomFrequency"] - (double)Parameters["v0FrequencyNewValue"] / 2 - (double)Parameters["calibOffset"]) / (double)Parameters["calibGradient"]);//jump to blue detuning
        p.AddAnalogValue("v00Frequency", cameraTriggerTime, ((double)Parameters["lockAomFrequency"] - (double)Parameters["v0FrequencyStartValue"] / 2 - (double)Parameters["calibOffset"]) / (double)Parameters["calibGradient"]); //jump aom frequency back to normal for imaging 

        

        p.SwitchAllOffAtEndOfPattern();
        return p;
    }

}
