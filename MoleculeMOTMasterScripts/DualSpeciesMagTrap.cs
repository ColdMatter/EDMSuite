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
        Parameters["PatternLength"] = 1500000;
        Parameters["TCLBlockStart"] = 800001; // This is a time before the Q switch
        Parameters["TCLBlockDuration"] = 8000;
        Parameters["FlashToQ"] = 16; // This is a time before the Q switch
        Parameters["QSwitchPulseDuration"] = 10;
        Parameters["FlashPulseDuration"] = 10;
        Parameters["HeliumShutterToQ"] = 100;
        Parameters["HeliumShutterDuration"] = 1550;

        Parameters["MOTSwitchOffTime"] = 6300;
        Parameters["MolassesDelay"] = 100;
        Parameters["MolassesDuration"] = 800;
        Parameters["v0F0PumpDuration"] = 10;
        Parameters["MOTPictureTriggerTime"] = 4000;
        Parameters["MicrowavePulseDuration"] = 6;
        Parameters["SecondMicrowavePulseDuration"] = 5;
        Parameters["MagTrapDuration"] = 100000;
        Parameters["MOTWaitBeforeImage"] = 500;

        // Camera
        Parameters["Frame0TriggerDuration"] = 10;

        //PMT
        Parameters["PMTTrigger"] = 4000;
        Parameters["PMTTriggerDuration"] = 10;

        // BX poke
        Parameters["PokeDetuningValue"] = -1.35;
        Parameters["PokeDuration"] = 200;

        // Slowing
        Parameters["slowingAOMOnStart"] = 250;
        Parameters["slowingAOMOnDuration"] = 45000;
        Parameters["slowingAOMOffStart"] = 1500;
        Parameters["slowingAOMOffDuration"] = 40000;
        Parameters["slowingRepumpAOMOnStart"] = 250;
        Parameters["slowingRepumpAOMOnDuration"] = 45000;
        Parameters["slowingRepumpAOMOffStart"] = 1700;
        Parameters["slowingRepumpAOMOffDuration"] = 40000;

        // Slowing Chirp
        Parameters["SlowingChirpStartTime"] = 340;
        Parameters["SlowingChirpDuration"] = 1160;
        Parameters["SlowingChirpStartValue"] = 0.0;
        Parameters["SlowingChirpEndValue"] = -1.3;
        Parameters["SlowingChirpHoldDuration"] = 8000;

        // Slowing field
        Parameters["slowingCoilsValue"] = 5.0;
        Parameters["slowingCoilsOffTime"] = 1500;

        // B Field
        Parameters["MOTCoilsCurrentRampStartValue"] = 0.6;
        Parameters["MOTCoilsCurrentRampStartTime"] = 4000;
        Parameters["MOTCoilsCurrentRampEndValue"] = 1.5;
        Parameters["MOTCoilsCurrentRampDuration"] = 1000;
        Parameters["MOTCoilsCurrentMolassesValue"] = 0.0; //0.21
        Parameters["MOTCoilsCurrentMagTrapValue"] = 1.2;// 1.2;// 0.6;

        Parameters["CoilsSwitchOffTime"] = 500000;

        // Shim fields
        Parameters["xShimLoadCurrent"] = 2.41;// 1.41;
        Parameters["yShimLoadCurrent"] = 0.3; //2.4
        Parameters["zShimLoadCurrent"] = -6.39; //-6.39

        // v0 Light Intensity
        Parameters["v0IntensityRampStartTime"] = 5500;
        Parameters["v0IntensityRampDuration"] = 400;
        Parameters["v0IntensityRampStartValue"] = 5.8;
        Parameters["v0IntensityRampEndValue"] = 8.465;
        Parameters["v0IntensityMolassesValue"] = 5.8;
        Parameters["v0IntensityF0PumpValue"] = 9.33;
        Parameters["v0IntensityImageValue"] = 5.8;

        // v0 Light Frequency
        Parameters["v0FrequencyMOTValue"] = 0.0; //set this to 0.0 for 114.1MHz 
        Parameters["v0FrequencyMolassesValue"] = 30.0; //set this to MHz detuning desired if doing frequency jump (positive for blue detuning)
        Parameters["v0FrequencyF0PumpValue"] = 0.0; //set this to MHz detuning desired if doing frequency jump (positive for blue detuning)

        // v0 pumping EOM
        Parameters["v0EOMMOTValue"] = 5.3;
        Parameters["v0EOMPumpValue"] = 3.3; // 4.3; //3.5

        //v0aomCalibrationValues
        Parameters["lockAomFrequency"] = 114.1;
        Parameters["calibOffset"] = 64.2129;
        Parameters["calibGradient"] = 5.55075;

        // Rb Intensity
        Parameters["rbCoolingMOTIntensity"] = 2.4;
        Parameters["rbCoolingMOTIntensityRampEndValue"] = 2.4;
        Parameters["rbCoolingMOTIntensityRampDuration"] = 1000;
        Parameters["rbCoolingMolassesIntensity"] = 2.4;

        Parameters["rbLoadingDuration"] = 800000;
        Parameters["rbOpticalPumpingDuration"] = 1;
        Parameters["rbExpansionDuration"] = 600;

        //RbField
        Parameters["rbMOTCoilsLoadValue"] = 0.32;
    }

    public override PatternBuilder32 GetDigitalPattern()
    {
        PatternBuilder32 p = new PatternBuilder32();
        int patternStartBeforeQ = (int)Parameters["TCLBlockStart"];
        int motSwitchOffTime = patternStartBeforeQ + (int)Parameters["MOTSwitchOffTime"];
        int molassesStartTime = motSwitchOffTime + (int)Parameters["MolassesDelay"];
        int v0F0PumpStartTime = molassesStartTime + (int)Parameters["MolassesDuration"];
        int microwavePulseTime = v0F0PumpStartTime + (int)Parameters["v0F0PumpDuration"];
        int blowAwayTime = microwavePulseTime + (int)Parameters["MicrowavePulseDuration"];
        int secondMicrowavePulseTime = blowAwayTime + (int)Parameters["PokeDuration"];
        int magTrapStartTime = secondMicrowavePulseTime + (int)Parameters["SecondMicrowavePulseDuration"];
        int motRecaptureTime = magTrapStartTime + (int)Parameters["MagTrapDuration"];
        int imageTime = motRecaptureTime + (int)Parameters["MOTWaitBeforeImage"];

        MOTMasterScriptSnippet lm = new LoadMoleculeMOTDualSpecies(p, Parameters);  // This is how you load "preset" patterns. 

        //int yagFiresBeforeSlowing = ((int)Parameters["TCLBlockStart"] - (int)Parameters["FlashToQ"]) / 50000;
        //int firstYAGFire = patternStartBeforeQ - yagFiresBeforeSlowing * 50000;
        //for (int t = firstYAGFire; t < (int)Parameters["PatternLength"]; t += 50000)
        //{
        //    p.Pulse(t, -(int)Parameters["FlashToQ"], (int)Parameters["QSwitchPulseDuration"], "flashLamp"); //trigger the flashlamp
        //    p.Pulse(t, 0, (int)Parameters["QSwitchPulseDuration"], "qSwitch"); //trigger the Q switch
        //}

        p.Pulse(patternStartBeforeQ - 100000, -(int)Parameters["FlashToQ"], (int)Parameters["QSwitchPulseDuration"], "flashLamp"); //trigger the flashlamp
        p.Pulse(patternStartBeforeQ - 100000, 0, (int)Parameters["QSwitchPulseDuration"], "qSwitch"); //trigger the Q switch
        p.Pulse(patternStartBeforeQ - 50000, -(int)Parameters["FlashToQ"], (int)Parameters["QSwitchPulseDuration"], "flashLamp"); //trigger the flashlamp
        p.Pulse(patternStartBeforeQ - 50000, 0, (int)Parameters["QSwitchPulseDuration"], "qSwitch"); //trigger the Q switch
        p.Pulse(patternStartBeforeQ, -(int)Parameters["FlashToQ"], (int)Parameters["QSwitchPulseDuration"], "flashLamp"); //trigger the flashlamp
        p.Pulse(patternStartBeforeQ, 0, (int)Parameters["QSwitchPulseDuration"], "qSwitch"); //trigger the Q switch

        p.Pulse(0, microwavePulseTime, (int)Parameters["MicrowavePulseDuration"], "microwaveA");
        //  p.Pulse(patternStartBeforeQ, microwavePulseTime, (int)Parameters["MicrowavePulseDuration"], "microwaveB");
        p.Pulse(0, secondMicrowavePulseTime, (int)Parameters["SecondMicrowavePulseDuration"], "microwaveB"); // now linked to A channel

        p.Pulse(0, motSwitchOffTime, (int)Parameters["MolassesDelay"], "v00MOTAOM"); // pulse off the MOT light whilst MOT fields are turning off
        p.Pulse(0, microwavePulseTime, motRecaptureTime - microwavePulseTime, "v00MOTAOM"); // turn off the MOT light for microwave pulse

        p.Pulse(0, blowAwayTime, (int)Parameters["PokeDuration"], "bXSlowingAOM"); // Blow away
        p.AddEdge("bXSlowingAOM", patternStartBeforeQ + (int)Parameters["slowingAOMOffStart"] + (int)Parameters["slowingAOMOffDuration"], true); // send slowing aom high and hold it high
        p.AddEdge("v10SlowingAOM", patternStartBeforeQ + (int)Parameters["slowingRepumpAOMOffStart"] + (int)Parameters["slowingRepumpAOMOffDuration"], true); // send slowing repump aom high and hold it high

        p.Pulse(0, secondMicrowavePulseTime - 1400, (int)Parameters["MagTrapDuration"] + 3000, "bXSlowingShutter"); //Takes 14ms to start closing
        p.Pulse(0, microwavePulseTime - 1500, motRecaptureTime - microwavePulseTime + 1500 - 1100, "v00MOTShutter");

        p.Pulse(patternStartBeforeQ, (int)Parameters["MOTPictureTriggerTime"], (int)Parameters["Frame0TriggerDuration"], "cameraTrigger"); // camera trigger for picture of initial MOT
        p.Pulse(0, imageTime, (int)Parameters["Frame0TriggerDuration"], "cameraTrigger"); // camera trigger

        //rb Pattern
        p.AddEdge("rbCoolingAOM", 0, true);
        p.AddEdge("rbCoolingAOM", patternStartBeforeQ - (int)Parameters["rbLoadingDuration"], false);
        p.Pulse(0, molassesStartTime, (int)Parameters["rbExpansionDuration"], "rbCoolingAOM");
        p.Pulse(0, magTrapStartTime - (int)Parameters["rbOpticalPumpingDuration"], motRecaptureTime - (magTrapStartTime - (int)Parameters["rbOpticalPumpingDuration"]), "rbCoolingAOM");

        p.AddEdge("rbOpticalPumpingAOM", 0, true); // Turn off for most of sequence
        p.DownPulse(0, magTrapStartTime - (int)Parameters["rbOpticalPumpingDuration"], (int)Parameters["rbOpticalPumpingDuration"], "rbOpticalPumpingAOM"); // Turn on for optical pumping step
        p.AddEdge("rbOpticalPumpingAOM", imageTime + 2000, false); // Turn back on safely after imaging has finished

        p.Pulse(0, magTrapStartTime - 1600, motRecaptureTime - magTrapStartTime + 1600 - 1200, "rbCoolingShutter"); // Takes 16ms to start closing, 14ms to fully open
        p.Pulse(0, magTrapStartTime - 1400, motRecaptureTime - magTrapStartTime + 1400, "rbOpticalPumpingShutter"); // Takes 14ms to start closing, don't really care when it opens again as not required for imaging

        p.Pulse(0, molassesStartTime - 600, (int)Parameters["Frame0TriggerDuration"], "cameraTrigger2");

        p.Pulse(0, imageTime, (int)Parameters["Frame0TriggerDuration"], "cameraTrigger2");

        return p;
    }

    public override AnalogPatternBuilder GetAnalogPattern()
    {
        AnalogPatternBuilder p = new AnalogPatternBuilder((int)Parameters["PatternLength"]);

        MOTMasterScriptSnippet lm = new LoadMoleculeMOTDualSpecies(p, Parameters);

        int patternStartBeforeQ = (int)Parameters["TCLBlockStart"];
        int motSwitchOffTime = patternStartBeforeQ + (int)Parameters["MOTSwitchOffTime"];
        int molassesStartTime = motSwitchOffTime + (int)Parameters["MolassesDelay"];
        int v0F0PumpStartTime = molassesStartTime + (int)Parameters["MolassesDuration"];
        int microwavePulseTime = v0F0PumpStartTime + (int)Parameters["v0F0PumpDuration"];
        int blowAwayTime = microwavePulseTime + (int)Parameters["MicrowavePulseDuration"];
        int secondMicrowavePulseTime = blowAwayTime + (int)Parameters["PokeDuration"];
        int magTrapStartTime = secondMicrowavePulseTime + (int)Parameters["SecondMicrowavePulseDuration"];
        int motRecaptureTime = magTrapStartTime + (int)Parameters["MagTrapDuration"];
        int imageTime = motRecaptureTime + (int)Parameters["MOTWaitBeforeImage"];


        // Add Analog Channels
        p.AddChannel("v00Intensity");
        p.AddChannel("v00Frequency");
        p.AddChannel("xShimCoilCurrent");
        p.AddChannel("yShimCoilCurrent");
        p.AddChannel("v00EOMAmp");
        p.AddChannel("zShimCoilCurrent");
        p.AddChannel("rbCoolingIntensity");

        // Slowing field
        p.AddAnalogValue("slowingCoilsCurrent", 0, (double)Parameters["slowingCoilsValue"]);
        p.AddAnalogValue("slowingCoilsCurrent", patternStartBeforeQ + (int)Parameters["slowingCoilsOffTime"], 0.0);

        // B Field
        p.AddAnalogValue("MOTCoilsCurrent", 0, (double)Parameters["rbMOTCoilsLoadValue"]);
        p.AddLinearRamp("MOTCoilsCurrent", patternStartBeforeQ - (int)Parameters["MOTCoilsCurrentRampDuration"], (int)Parameters["MOTCoilsCurrentRampDuration"], (double)Parameters["MOTCoilsCurrentRampStartValue"]);
        p.AddLinearRamp("MOTCoilsCurrent", patternStartBeforeQ + (int)Parameters["MOTCoilsCurrentRampStartTime"], (int)Parameters["MOTCoilsCurrentRampDuration"], (double)Parameters["MOTCoilsCurrentRampEndValue"]);
        p.AddAnalogValue("MOTCoilsCurrent", motSwitchOffTime, (double)Parameters["MOTCoilsCurrentMolassesValue"]);
        p.AddAnalogValue("MOTCoilsCurrent", magTrapStartTime, (double)Parameters["MOTCoilsCurrentMagTrapValue"]);
        p.AddAnalogValue("MOTCoilsCurrent", motRecaptureTime, (double)Parameters["MOTCoilsCurrentRampStartValue"]);
        p.AddAnalogValue("MOTCoilsCurrent", patternStartBeforeQ + (int)Parameters["CoilsSwitchOffTime"], 0.0);

        // Shim Fields
        p.AddAnalogValue("xShimCoilCurrent", 0, (double)Parameters["xShimLoadCurrent"]);
        p.AddAnalogValue("yShimCoilCurrent", 0, (double)Parameters["yShimLoadCurrent"]);
        p.AddAnalogValue("zShimCoilCurrent", 0, (double)Parameters["zShimLoadCurrent"]);

        // v0 Intensity Ramp
        p.AddAnalogValue("v00Intensity", 0, (double)Parameters["v0IntensityRampStartValue"]);
        p.AddLinearRamp("v00Intensity", patternStartBeforeQ + (int)Parameters["v0IntensityRampStartTime"], (int)Parameters["v0IntensityRampDuration"], (double)Parameters["v0IntensityRampEndValue"]);
        p.AddAnalogValue("v00Intensity", molassesStartTime, (double)Parameters["v0IntensityMolassesValue"]);
        p.AddAnalogValue("v00Intensity", molassesStartTime + 650, 7.4);
        p.AddAnalogValue("v00Intensity", molassesStartTime + 700, 7.83);
        p.AddAnalogValue("v00Intensity", molassesStartTime + 750, 8.09);
        p.AddAnalogValue("v00Intensity", v0F0PumpStartTime, (double)Parameters["v0IntensityF0PumpValue"]);
        p.AddAnalogValue("v00Intensity", motRecaptureTime, (double)Parameters["v0IntensityImageValue"]);

        // v0 EOM
        p.AddAnalogValue("v00EOMAmp", 0, (double)Parameters["v0EOMMOTValue"]);
        p.AddAnalogValue("v00EOMAmp", v0F0PumpStartTime, (double)Parameters["v0EOMPumpValue"]);
        p.AddAnalogValue("v00EOMAmp", motRecaptureTime, (double)Parameters["v0EOMMOTValue"]);

        // v0 Frequency Ramp
        p.AddAnalogValue("v00Frequency", 0, ((double)Parameters["lockAomFrequency"] - (double)Parameters["v0FrequencyMOTValue"] / 2 - (double)Parameters["calibOffset"]) / (double)Parameters["calibGradient"]);
        p.AddAnalogValue(
            "v00Frequency",
            molassesStartTime,
            ((double)Parameters["lockAomFrequency"] - (double)Parameters["v0FrequencyMolassesValue"] / 2 - (double)Parameters["calibOffset"]) / (double)Parameters["calibGradient"]
        );
        p.AddAnalogValue(
            "v00Frequency",
            v0F0PumpStartTime,
            ((double)Parameters["lockAomFrequency"] - (double)Parameters["v0FrequencyF0PumpValue"] / 2 - (double)Parameters["calibOffset"]) / (double)Parameters["calibGradient"]
        );
        p.AddAnalogValue(
            "v00Frequency",
            motRecaptureTime,
            ((double)Parameters["lockAomFrequency"] - (double)Parameters["v0FrequencyMOTValue"] / 2 - (double)Parameters["calibOffset"]) / (double)Parameters["calibGradient"]
        );

        // Rb Cooling light intensity
        p.AddAnalogValue("rbCoolingIntensity", 0, (double)Parameters["rbCoolingMOTIntensity"]);
        p.AddLinearRamp("rbCoolingIntensity", patternStartBeforeQ + (int)Parameters["v0IntensityRampStartTime"], (int)Parameters["v0IntensityRampDuration"], (double)Parameters["rbCoolingMOTIntensityRampEndValue"]);
        p.AddAnalogValue("rbCoolingIntensity", motSwitchOffTime, (double)Parameters["rbCoolingMolassesIntensity"]);
        //p.AddAnalogValue("rbCoolingIntensity", molassesStartTime + (int)Parameters["rbExpansionDuration"] + 100, 1.79);
        //p.AddAnalogValue("rbCoolingIntensity", molassesStartTime + (int)Parameters["rbExpansionDuration"] + 200, 1.65);
        //p.AddAnalogValue("rbCoolingIntensity", molassesStartTime + (int)Parameters["rbExpansionDuration"] + 300, 1.48);
        //p.AddAnalogValue("rbCoolingIntensity", molassesStartTime + (int)Parameters["rbExpansionDuration"] + 400, 1.397);
        //p.AddAnalogValue("rbCoolingIntensity", molassesStartTime + (int)Parameters["rbExpansionDuration"] + 600, 1.316);
        p.AddAnalogValue("rbCoolingIntensity", motRecaptureTime, (double)Parameters["rbCoolingMOTIntensity"]);

        return p;
    }


}
