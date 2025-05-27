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
        Parameters["RotationTime"] = 2500;
        Parameters["MolassesDelay"] = 100;
        Parameters["MolassesHoldTime"] = 1400;
        Parameters["MolassesRampDuration"] = 200;
        Parameters["SingleFreqMolassesDuration"] = 500;//200;
        Parameters["RotationOffRampDuration"] = 500;
        Parameters["WaitBeforeImage"] = 500;
        Parameters["ExpansionTime"] = 200;

        Parameters["v00ChirpDuration"] = 10;// 200;
        Parameters["v00ChirpWait"] = 60;// 100;
        Parameters["v00ChirpAmplitude"] = 0.9;// 0.8; //0.4; // 0.4V on PC ~ 0.2V on TCL = 70.5MHz

        Parameters["v0F0PumpDuration"] = 10;
        Parameters["MOTPictureTriggerTime"] = 4000;
        Parameters["MicrowavePulseDuration"] =  3;// 15;

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
        Parameters["slowingCoilsValue"] = 8.0; //1.1;
        Parameters["slowingCoilsOffTime"] = 1500;

        // B Field
        Parameters["MOTCoilsSwitchOn"] = 0;
        Parameters["MOTCoilsCurrentRampStartValue"] = 0.6;// 0.65;
        Parameters["MOTCoilsCurrentRampStartTime"] = 4000;
        Parameters["MOTCoilsCurrentRampEndValue"] = 1.5;// 0.65;// 1.5;
        Parameters["MOTCoilsCurrentRampDuration"] = 1000;
        Parameters["MOTCoilsCurrentMolassesValue"] = -0.01; //0.06
        Parameters["MOTCoilsCurrentLevitateValue"] = 0.0;// 0.65;
        Parameters["TopCoilShuntLevitateValue"] = 0.0;// 8.0;
        Parameters["CoilsSwitchOffTime"] = 20000;

        // Shim fields
        Parameters["xShimLoadCurrent"] = 2.7;// 1.6;// 1.195; // 1.202;// 1.219;
        Parameters["yShimLoadCurrent"] = -0.12;// -0.155; //2.4
        Parameters["zShimLoadCurrent"] = -5.35; //0.26

        // v0 Light Intensity
        Parameters["v0IntensityRampStartTime"] = 5500;
        Parameters["v0IntensityRampDuration"] = 400;
        Parameters["v0IntensityRampStartValue"] = 5.8;
        Parameters["v0IntensityRampEndValue"] = 8.465;
        Parameters["v0IntensityMolassesValue"] = 5.8;
        Parameters["v0IntensityF0PumpValue"] = 9.33;
        Parameters["v0IntensitySingleFreqMolassesValue"] = 5.8;// 5.8;
        Parameters["v0IntensitySingleFreqMolassesStepValue"] = 5.8;// 8.465;// 9.535;
        Parameters["v0IntensityImageValue"] = 5.8;

        // v0 Light Frequency
        Parameters["v0FrequencyStartValue"] = 0.0; //set this to 0.0 for 114.1MHz 
        Parameters["v0FrequencyNewValue"] = 20.0; //set this to MHz detuning desired if doing frequency jump (positive for blue detuning)
        Parameters["v0FrequencyF0PumpValue"] = 0.0;
        Parameters["v0FrequencyImageValue"] = 0.0;

        // v0 pumping EOM
        Parameters["v0EOMMOTValue"] = 5.3;
        Parameters["v0EOMPumpValue"] = 3.3;

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
        int v0F0PumpStartTime = molassesRampTime + (int)Parameters["MolassesRampDuration"];
        int v00ChirpTime = v0F0PumpStartTime + (int)Parameters["v0F0PumpDuration"];
        int singleFrequencyMolassesTime = v00ChirpTime + (int)Parameters["v00ChirpDuration"] + (int)Parameters["v00ChirpWait"];
        int releaseTime = singleFrequencyMolassesTime + (int)Parameters["SingleFreqMolassesDuration"];
        int motRecaptureTime = releaseTime + (int)Parameters["ExpansionTime"];
        int cameraTriggerTime = motRecaptureTime + (int)Parameters["WaitBeforeImage"];

        MOTMasterScriptSnippet lm = new LoadMoleculeMOT(p, Parameters);  // This is how you load "preset" patterns. 

        p.Pulse(patternStartBeforeQ, (int)Parameters["MOTSwitchOffTime"], (int)Parameters["MolassesDelay"], "v00MOTAOM"); //pulse off the MOT light whilst MOT fields are turning off
        p.Pulse(patternStartBeforeQ, v00ChirpTime, motRecaptureTime - v00ChirpTime, "v00MOTAOM");
        p.Pulse(patternStartBeforeQ, singleFrequencyMolassesTime + 400, (int)Parameters["MicrowavePulseDuration"], "microwaveA");
        p.Pulse(patternStartBeforeQ, (int)Parameters["MOTSwitchOffTime"] - 1400, cameraTriggerTime - (int)Parameters["MOTSwitchOffTime"] + 3000, "bXSlowingShutter"); //Takes 14ms to start closing
       // p.Pulse(patternStartBeforeQ, v00ChirpTime, (int)Parameters["v00ChirpDuration"] + (int)Parameters["v00ChirpWait"] + (int)Parameters["SingleFreqMolassesDuration"], "v00Sidebands");
        p.Pulse(patternStartBeforeQ, v00ChirpTime, 2 * (int)Parameters["v00ChirpDuration"] + (int)Parameters["v00ChirpWait"] + (int)Parameters["SingleFreqMolassesDuration"] + 200, "v00LockBlock");
        p.Pulse(patternStartBeforeQ, (int)Parameters["MOTPictureTriggerTime"], (int)Parameters["Frame0TriggerDuration"], "cameraTrigger"); // camera trigger for picture of initial MOT
        p.Pulse(patternStartBeforeQ, cameraTriggerTime, (int)Parameters["Frame0TriggerDuration"], "cameraTrigger"); //camera trigger for first frame

        return p;
    }

    public override AnalogPatternBuilder GetAnalogPattern()
    {
        AnalogPatternBuilder p = new AnalogPatternBuilder((int)Parameters["PatternLength"]);

        MOTMasterScriptSnippet lm = new LoadMoleculeMOT(p, Parameters);

        int molassesStartTime = (int)Parameters["MOTSwitchOffTime"] + (int)Parameters["MolassesDelay"];
        int molassesRampTime = molassesStartTime + (int)Parameters["MolassesHoldTime"];
        int v0F0PumpStartTime = molassesRampTime + (int)Parameters["MolassesRampDuration"];
        int v00ChirpTime = v0F0PumpStartTime + (int)Parameters["v0F0PumpDuration"];
        int singleFrequencyMolassesTime = v00ChirpTime + (int)Parameters["v00ChirpDuration"] + (int)Parameters["v00ChirpWait"];
        int releaseTime = singleFrequencyMolassesTime + (int)Parameters["SingleFreqMolassesDuration"];
        int motRecaptureTime = releaseTime + (int)Parameters["ExpansionTime"];
        int cameraTriggerTime = motRecaptureTime + (int)Parameters["WaitBeforeImage"];

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
        p.AddAnalogValue("MOTCoilsCurrent", motRecaptureTime, (double)Parameters["MOTCoilsCurrentRampStartValue"]);
        p.AddAnalogValue("MOTCoilsCurrent", (int)Parameters["CoilsSwitchOffTime"], -0.01);

        // Top coil shunt
        //p.AddAnalogValue("topCoilShunt", 0, 0.0);

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
        p.AddAnalogValue("v00Intensity", v0F0PumpStartTime, (double)Parameters["v0IntensityF0PumpValue"]);
        p.AddAnalogValue("v00Intensity", releaseTime, (double)Parameters["v0IntensitySingleFreqMolassesValue"]);
        p.AddAnalogValue("v00Intensity", motRecaptureTime, (double)Parameters["v0IntensityImageValue"]);

        // v0 Chirp
        //p.AddAnalogValue("v00Chirp", 0, 0.0);
        //p.AddLinearRamp("v00Chirp", v00ChirpTime, (int)Parameters["v00ChirpDuration"], (double)Parameters["v00ChirpAmplitude"]);
        //p.AddLinearRamp("v00Chirp", releaseTime, (int)Parameters["v00ChirpDuration"], 0.0);

        // v0 EOM
        p.AddAnalogValue("v00EOMAmp", 0, (double)Parameters["v0EOMMOTValue"]);
        p.AddAnalogValue("v00EOMAmp", v0F0PumpStartTime, (double)Parameters["v0EOMPumpValue"]);
        p.AddAnalogValue("v00EOMAmp", releaseTime, (double)Parameters["v0EOMMOTValue"]);

        // v0 Frequency Ramp
        p.AddAnalogValue("v00Frequency", 0, ((double)Parameters["lockAomFrequency"] - (double)Parameters["v0FrequencyStartValue"] / 2 - (double)Parameters["calibOffset"]) / (double)Parameters["calibGradient"]);
        p.AddAnalogValue("v00Frequency", molassesStartTime, ((double)Parameters["lockAomFrequency"] - (double)Parameters["v0FrequencyNewValue"] / 2 - (double)Parameters["calibOffset"]) / (double)Parameters["calibGradient"]);//jump to blue detuning
        p.AddAnalogValue(
            "v00Frequency",
            v0F0PumpStartTime,
            ((double)Parameters["lockAomFrequency"] - (double)Parameters["v0FrequencyF0PumpValue"] / 2 - (double)Parameters["calibOffset"]) / (double)Parameters["calibGradient"]
        );
       // p.AddAnalogValue("v00Frequency", singleFrequencyMolassesTime, ((double)Parameters["lockAomFrequency"] - (double)Parameters["v0FrequencyNewValue"] / 2 - (double)Parameters["calibOffset"]) / (double)Parameters["calibGradient"]);//jump to blue detuning
     //   p.AddAnalogValue("v00Frequency", releaseTime, ((double)Parameters["lockAomFrequency"] - (double)Parameters["v0FrequencyImageValue"] / 2 - (double)Parameters["calibOffset"]) / (double)Parameters["calibGradient"]); //jump aom frequency back to normal for imaging 
        p.AddAnalogValue("v00Frequency", cameraTriggerTime + 5000, ((double)Parameters["lockAomFrequency"] - (double)Parameters["v0FrequencyStartValue"] / 2 - (double)Parameters["calibOffset"]) / (double)Parameters["calibGradient"]); //jump aom frequency back to normal for imaging 


        return p;
    }

}
