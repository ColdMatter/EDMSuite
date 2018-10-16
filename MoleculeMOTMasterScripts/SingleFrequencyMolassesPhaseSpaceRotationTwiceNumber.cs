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
        Parameters["HeliumShutterToQ"] = 100;
        Parameters["HeliumShutterDuration"] = 1550;

        Parameters["MOTSwitchOffTime"] = 6300;
        Parameters["RotationTime"] =  2500;// 2500;
        Parameters["MolassesDelay"] = 100;
        Parameters["MolassesHoldTime"] = 600;
        Parameters["MolassesRampDuration"] = 200;
        Parameters["SingleFreqMolassesDuration"] = 500;
        Parameters["SingleFreqMolassesTwoDuration"] = 500;// 500;
        Parameters["WaitBeforeImage"] = 500;
        Parameters["FieldDecayTime"] =  300;// 300;

        Parameters["v00ChirpDuration"] = 10;// 200;
        Parameters["v00ChirpWait"] = 100;// 100;
        Parameters["v00ChirpAmplitude"] = 0.9;// 0.4V on PC ~ 0.2V on TCL = 70.5MHz
        Parameters["v00ChirpTwoAmplitude"] = 0.9;

        Parameters["WaitBeforeMOT"] = 1000;

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
        Parameters["slowingCoilsValue"] = 5.0;
        Parameters["slowingCoilsOffTime"] = 1500;

        // B Field
        Parameters["MOTCoilsSwitchOn"] = 0;
        Parameters["MOTCoilsCurrentRampStartValue"] = 0.55;
        Parameters["MOTCoilsCurrentRampStartTime"] = 4000;
        Parameters["MOTCoilsCurrentRampEndValue"] = 1.3;
        Parameters["MOTCoilsCurrentRampDuration"] = 1000;
        Parameters["MOTCoilsCurrentMolassesValue"] = -0.01;
        Parameters["MOTCoilsCurrentLevitateValue"] = 1.8;
        Parameters["MOTCoilsCurrentCancelValue"] = 1.8;
        Parameters["TopCoilShuntLevitateValue"] = 8.0;
        Parameters["CoilsSwitchOffTime"] = 20000;

        // Shim fields
        Parameters["xShimLoadCurrent"] = 1.3;
        Parameters["yShimLoadCurrent"] = -0.6;
        Parameters["zShimLoadCurrent"] = -6.0;

        // v0 Light Intensity
        Parameters["v0IntensityRampStartTime"] = 5500;
        Parameters["v0IntensityRampDuration"] = 400;
        Parameters["v0IntensityRampStartValue"] = 5.8;
        Parameters["v0IntensityRampEndValue"] = 8.465;
        Parameters["v0IntensityMolassesValue"] = 5.8;
        Parameters["v0IntensitySingleFreqMolassesValue"] = 5.8;
        Parameters["v0IntensitySingleFreqMolassesTwo"] = 5.8;
        Parameters["v0IntensityImageValue"] = 5.8;// 7.61;
        // v0 Light Frequency
        Parameters["v0FrequencyStartValue"] = 0.0; //set this to 0.0 for 114.1MHz 
        Parameters["v0FrequencyNewValue"] = 20.0; //set this to MHz detuning desired if doing frequency jump (positive for blue detuning)
        Parameters["v0FrequencyImageValue"] = 0.0;

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
        int molassesRampTime = molassesStartTime + (int)Parameters["MolassesHoldTime"];
        int v00ChirpTime = molassesRampTime + (int)Parameters["MolassesRampDuration"];
        int singleFrequencyMolassesTime = v00ChirpTime + (int)Parameters["v00ChirpDuration"] + (int)Parameters["v00ChirpWait"];
        int harmonicTrapOnTime = singleFrequencyMolassesTime + (int)Parameters["SingleFreqMolassesDuration"];
        int harmonicTrapOffTime = harmonicTrapOnTime + (int)Parameters["RotationTime"];
        int singleFrequencyMolassesTimeTwo = harmonicTrapOffTime + (int)Parameters["FieldDecayTime"];
        int releaseTime = singleFrequencyMolassesTimeTwo + (int)Parameters["SingleFreqMolassesTwoDuration"];
        int recaptureTime = releaseTime + (int)Parameters["v00ChirpDuration"] + (int)Parameters["v00ChirpWait"] + (int)Parameters["WaitBeforeMOT"];
        int cameraTriggerTime = recaptureTime + (int)Parameters["WaitBeforeImage"];

        MOTMasterScriptSnippet lm = new LoadMoleculeMOT(p, Parameters);  // This is how you load "preset" patterns. 

        p.Pulse(patternStartBeforeQ, (int)Parameters["MOTSwitchOffTime"], (int)Parameters["MolassesDelay"], "v00MOTAOM"); //pulse off the MOT light whilst MOT fields are turning off
        p.Pulse(patternStartBeforeQ, v00ChirpTime, (int)Parameters["v00ChirpDuration"] + (int)Parameters["v00ChirpWait"], "v00MOTAOM"); //pulse MOT light off during frequency chirp
        p.Pulse(patternStartBeforeQ, (int)Parameters["MOTSwitchOffTime"] - 1400, cameraTriggerTime - (int)Parameters["MOTSwitchOffTime"] + 3000, "bXSlowingShutter"); //Takes 14ms to start closing
        p.Pulse(patternStartBeforeQ, harmonicTrapOnTime, singleFrequencyMolassesTimeTwo - harmonicTrapOnTime, "v00MOTAOM"); //pulse off the MOT light during harmonic trap
        p.Pulse(patternStartBeforeQ, releaseTime, recaptureTime - releaseTime, "v00MOTAOM"); //pulse off the MOT light during free expansion
        p.Pulse(patternStartBeforeQ, v00ChirpTime, (int)Parameters["v00ChirpDuration"] + (int)Parameters["v00ChirpWait"] + (int)Parameters["SingleFreqMolassesDuration"], "v00Sidebands");//sidebands off for single freq molasses
        p.Pulse(patternStartBeforeQ, v00ChirpTime, releaseTime + (int)Parameters["v00ChirpDuration"] - v00ChirpTime, "v00LockBlock");//tcl blocked during single freq molasses
        p.Pulse(patternStartBeforeQ, singleFrequencyMolassesTimeTwo, releaseTime - singleFrequencyMolassesTimeTwo, "v00Sidebands");//sidebands off for single freq molasses 2       
        p.Pulse(patternStartBeforeQ, molassesStartTime, harmonicTrapOffTime + 20 - molassesStartTime, "bottomCoilDirection");
        p.Pulse(patternStartBeforeQ, harmonicTrapOffTime + 20, releaseTime - (harmonicTrapOffTime + 20), "topCoilDirection");

        p.Pulse(patternStartBeforeQ, 4000, (int)Parameters["Frame0TriggerDuration"], "cameraTrigger"); //camera trigger for first frame
        p.Pulse(patternStartBeforeQ, cameraTriggerTime, (int)Parameters["Frame0TriggerDuration"], "cameraTrigger"); //camera trigger for first frame

        return p;
    }

    public override AnalogPatternBuilder GetAnalogPattern()
    {
        AnalogPatternBuilder p = new AnalogPatternBuilder((int)Parameters["PatternLength"]);

        MOTMasterScriptSnippet lm = new LoadMoleculeMOT(p, Parameters);

        int molassesStartTime = (int)Parameters["MOTSwitchOffTime"] + (int)Parameters["MolassesDelay"];
        int molassesRampTime = molassesStartTime + (int)Parameters["MolassesHoldTime"];
        int v00ChirpTime = molassesRampTime + (int)Parameters["MolassesRampDuration"];
        int singleFrequencyMolassesTime = v00ChirpTime + (int)Parameters["v00ChirpDuration"] + (int)Parameters["v00ChirpWait"];
        int harmonicTrapOnTime = singleFrequencyMolassesTime + (int)Parameters["SingleFreqMolassesDuration"];
        int harmonicTrapOffTime = harmonicTrapOnTime + (int)Parameters["RotationTime"];
        int singleFrequencyMolassesTimeTwo = harmonicTrapOffTime + (int)Parameters["FieldDecayTime"];
        int releaseTime = singleFrequencyMolassesTimeTwo + (int)Parameters["SingleFreqMolassesTwoDuration"];
        int recaptureTime = releaseTime + (int)Parameters["v00ChirpDuration"] + (int)Parameters["v00ChirpWait"] + (int)Parameters["WaitBeforeMOT"];
        int cameraTriggerTime = recaptureTime + (int)Parameters["WaitBeforeImage"];

        // Add Analog Channels
        p.AddChannel("v00Intensity");
        p.AddChannel("v00Frequency");
        p.AddChannel("xShimCoilCurrent");
        p.AddChannel("yShimCoilCurrent");
        p.AddChannel("zShimCoilCurrent");
        p.AddChannel("v00EOMAmp");
        p.AddChannel("v00Chirp");
        p.AddChannel("topCoilShunt");

        // Slowing field
        p.AddAnalogValue("slowingCoilsCurrent", 0, (double)Parameters["slowingCoilsValue"]);
        p.AddAnalogValue("slowingCoilsCurrent", (int)Parameters["slowingCoilsOffTime"], 0.0);

        //// B Field
        p.AddAnalogValue("MOTCoilsCurrent", (int)Parameters["MOTCoilsSwitchOn"], (double)Parameters["MOTCoilsCurrentRampStartValue"]);
        p.AddLinearRamp("MOTCoilsCurrent", (int)Parameters["MOTCoilsCurrentRampStartTime"], (int)Parameters["MOTCoilsCurrentRampDuration"], (double)Parameters["MOTCoilsCurrentRampEndValue"]);
        p.AddAnalogValue("MOTCoilsCurrent", (int)Parameters["MOTSwitchOffTime"], (double)Parameters["MOTCoilsCurrentMolassesValue"]);
        p.AddAnalogValue("MOTCoilsCurrent", harmonicTrapOnTime, (double)Parameters["MOTCoilsCurrentLevitateValue"]);
        p.AddAnalogValue("MOTCoilsCurrent", harmonicTrapOffTime, (double)Parameters["MOTCoilsCurrentMolassesValue"]);
        p.AddAnalogValue("MOTCoilsCurrent", harmonicTrapOffTime + 50, (double)Parameters["MOTCoilsCurrentCancelValue"]);
        p.AddAnalogValue("MOTCoilsCurrent", harmonicTrapOffTime + 250, (double)Parameters["MOTCoilsCurrentMolassesValue"]);
        p.AddAnalogValue("MOTCoilsCurrent", recaptureTime, (double)Parameters["MOTCoilsCurrentRampStartValue"]);
        p.AddAnalogValue("MOTCoilsCurrent", (int)Parameters["CoilsSwitchOffTime"], -0.01);

        // Top coil shunt
        p.AddAnalogValue("topCoilShunt", 0, 0.0);
        p.AddAnalogValue("topCoilShunt", harmonicTrapOnTime, (double)Parameters["TopCoilShuntLevitateValue"]);
        p.AddAnalogValue("topCoilShunt", harmonicTrapOffTime, 0.0);

        //// Shim Fields
        p.AddAnalogValue("xShimCoilCurrent", 0, (double)Parameters["xShimLoadCurrent"]);
        p.AddAnalogValue("yShimCoilCurrent", 0, (double)Parameters["yShimLoadCurrent"]);
        p.AddAnalogValue("zShimCoilCurrent", 0, (double)Parameters["zShimLoadCurrent"]);


        // v0 Intensity Ramp
        p.AddAnalogValue("v00Intensity", 0, (double)Parameters["v0IntensityRampStartValue"]);
        p.AddLinearRamp("v00Intensity", (int)Parameters["v0IntensityRampStartTime"], (int)Parameters["v0IntensityRampDuration"], (double)Parameters["v0IntensityRampEndValue"]);
        p.AddAnalogValue("v00Intensity", molassesStartTime, (double)Parameters["v0IntensityMolassesValue"]);
        p.AddAnalogValue("v00Intensity", molassesRampTime + 50, 7.4);
        p.AddAnalogValue("v00Intensity", molassesRampTime + 100, 7.83);
        p.AddAnalogValue("v00Intensity", molassesRampTime + 150, 8.09);
        p.AddAnalogValue("v00Intensity", singleFrequencyMolassesTime, (double)Parameters["v0IntensitySingleFreqMolassesValue"]);
        p.AddAnalogValue("v00Intensity", singleFrequencyMolassesTimeTwo, (double)Parameters["v0IntensitySingleFreqMolassesTwo"]);
        //p.AddAnalogValue("v00Intensity", singleFrequencyMolassesTime + 50, 7.21); //change
        //p.AddAnalogValue("v00Intensity", singleFrequencyMolassesTime + 100, 7.61); //change
        //p.AddAnalogValue("v00Intensity", singleFrequencyMolassesTime + 200, 8.09); //change
        p.AddAnalogValue("v00Intensity", recaptureTime, (double)Parameters["v0IntensityImageValue"]);

        // v0 Chirp
        p.AddAnalogValue("v00Chirp", 0, 0.0);
        p.AddLinearRamp("v00Chirp", v00ChirpTime, (int)Parameters["v00ChirpDuration"], (double)Parameters["v00ChirpAmplitude"]);
        p.AddLinearRamp("v00Chirp", releaseTime, (int)Parameters["v00ChirpDuration"], 0.0);

        // F=0
        p.AddAnalogValue("v00EOMAmp", 0, 5.3);

        // v0 Frequency Ramp
        p.AddAnalogValue("v00Frequency", 0, ((double)Parameters["lockAomFrequency"] - (double)Parameters["v0FrequencyStartValue"] / 2 - (double)Parameters["calibOffset"]) / (double)Parameters["calibGradient"]);
        p.AddAnalogValue("v00Frequency", molassesStartTime, ((double)Parameters["lockAomFrequency"] - (double)Parameters["v0FrequencyNewValue"] / 2 - (double)Parameters["calibOffset"]) / (double)Parameters["calibGradient"]);//jump to blue detuning
        p.AddAnalogValue("v00Frequency", releaseTime, ((double)Parameters["lockAomFrequency"] - (double)Parameters["v0FrequencyStartValue"] / 2 - (double)Parameters["calibOffset"]) / (double)Parameters["calibGradient"]); //jump aom frequency for imaging

        return p;
    }

}
