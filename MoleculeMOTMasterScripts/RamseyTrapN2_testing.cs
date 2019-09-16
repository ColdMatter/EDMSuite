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
        Parameters["PatternLength"] = 1;// 50000;
        Parameters["TCLBlockStart"] = 1;// 2000; // This is a time before the Q switch
        Parameters["TCLBlockDuration"] = 1;// 8000;
        Parameters["FlashToQ"] = 1;// 16; // This is a time before the Q switch
        Parameters["QSwitchPulseDuration"] = 1;// 10;
        Parameters["FlashPulseDuration"] = 1;// 10;
        Parameters["HeliumShutterToQ"] = 1;// 100;
        Parameters["HeliumShutterDuration"] = 1;// 1550;

        Parameters["MOTSwitchOffTime"] = 1;// 6300;
        Parameters["MolassesDelay"] = 1;// 100;
        Parameters["MolassesHoldTime"] = 1;//1400;
        Parameters["MolassesRampDuration"] = 1;//200;
        Parameters["v0F0PumpDuration"] = 1;//10;
        Parameters["MOTPictureTriggerTime"] = 1;//4000;
        Parameters["MicrowavePulseDuration"] = 1;//3;
        Parameters["SecondMicrowavePulseDuration"] = 1;//3;
        Parameters["MagTrapDuration"] = 1;//2500;
        Parameters["MOTWaitBeforeImage"] = 1;//500;
        Parameters["RamseyPulseDuration"] = 1;//2;
        Parameters["RamseyWaitTime"] = 1;


        // Camera
        Parameters["Frame0TriggerDuration"] = 1;//10;

        //PMT
        Parameters["PMTTrigger"] = 4000;
        Parameters["PMTTriggerDuration"] = 1;// 10;

        // BX poke
        Parameters["PokeDetuningValue"] = -1.45;
        Parameters["PokeDuration"] = 1;//300;// 100;

        // Slowing
        Parameters["slowingAOMOnStart"] = 250;
        Parameters["slowingAOMOnDuration"] = 1;//45000;
        Parameters["slowingAOMOffStart"] = 1500;
        Parameters["slowingAOMOffDuration"] = 1;//40000;
        Parameters["slowingRepumpAOMOnStart"] = 250;
        Parameters["slowingRepumpAOMOnDuration"] = 1;// 45000;
        Parameters["slowingRepumpAOMOffStart"] = 1700;
        Parameters["slowingRepumpAOMOffDuration"] = 1;//40000;

        // Slowing Chirp
        Parameters["SlowingChirpStartTime"] = 1;//340;
        Parameters["SlowingChirpDuration"] = 1;//1160;
        Parameters["SlowingChirpStartValue"] = 0.0;
        Parameters["SlowingChirpEndValue"] = -1.3;
        Parameters["SlowingChirpHoldDuration"] = 1;//8000;

        // Slowing field
        Parameters["slowingCoilsValue"] = 8.0;
        Parameters["slowingCoilsOffTime"] = 1;//1500;

        // B Field
        Parameters["MOTCoilsSwitchOn"] = 0;
        Parameters["MOTCoilsCurrentRampStartValue"] = 0.6;
        Parameters["MOTCoilsCurrentRampStartTime"] = 1;//4000;
        Parameters["MOTCoilsCurrentRampEndValue"] = 1.5;
        Parameters["MOTCoilsCurrentRampDuration"] = 1;//1000;
        Parameters["MOTCoilsCurrentMolassesValue"] = -0.05;// -0.01; //0.21
        Parameters["MOTCoilsOffsetFieldValue"] = -0.05;// -0.01;
        Parameters["MOTCoilsCurrentMagTrapValue"] = 1.2;// 0.6;

        Parameters["CoilsSwitchOffTime"] = 1;//40000;

        // Shim fields
        Parameters["xShimLoadCurrent"] = 3.6;// 2.7;
        Parameters["yShimLoadCurrent"] = -0.12;// -0.12;
        Parameters["zShimLoadCurrent"] = -5.35;// -5.35; 

        // v0 Light Intensity
        Parameters["v0IntensityRampStartTime"] = 1;//5500;
        Parameters["v0IntensityRampDuration"] = 1;//400;
        Parameters["v0IntensityRampStartValue"] = 5.8;
        Parameters["v0IntensityRampEndValue"] = 8.465;
        Parameters["v0IntensityMolassesValue"] = 5.8;
        Parameters["v0IntensityF0PumpValue"] = 9.0;
        Parameters["v0IntensityImageValue"] = 5.8;

        // v0 Light Frequency
        Parameters["v0FrequencyMOTValue"] = 0.0; //set this to 0.0 for 114.1MHz 
        Parameters["v0FrequencyMolassesValue"] = 20.0; //set this to MHz detuning desired if doing frequency jump (positive for blue detuning)
        Parameters["v0FrequencyF0PumpValue"] = 0.0; //set this to MHz detuning desired if doing frequency jump (positive for blue detuning)

        // v0 pumping EOM
        Parameters["v0EOMMOTValue"] = 5.3;
        Parameters["v0EOMPumpValue"] = 3.3;

        //v0aomCalibrationValues
        Parameters["lockAomFrequency"] = 114.1;
        Parameters["calibOffset"] = 64.2129;
        Parameters["calibGradient"] = 5.55075;


    }

    public override PatternBuilder32 GetDigitalPattern()
    {
        PatternBuilder32 p = new PatternBuilder32();
        int patternStartBeforeQ = (int)Parameters["TCLBlockStart"];
        int molassesStartTime = (int)Parameters["MOTSwitchOffTime"] + (int)Parameters["MolassesDelay"];
        int molassesRampTime = molassesStartTime + (int)Parameters["MolassesHoldTime"];
        int v0F0PumpStartTime = molassesRampTime + (int)Parameters["MolassesRampDuration"];
        int microwavePulseTime = v0F0PumpStartTime + (int)Parameters["v0F0PumpDuration"];
        int blowAwayTime = microwavePulseTime + (int)Parameters["MicrowavePulseDuration"];
        int secondMicrowavePulseTime = blowAwayTime + (int)Parameters["PokeDuration"];
        int magTrapStartTime = secondMicrowavePulseTime + (int)Parameters["SecondMicrowavePulseDuration"];
        int firstRamseyPulseTime = magTrapStartTime + (int)Parameters["MagTrapDuration"];
        int secondRamseyPulseTime = firstRamseyPulseTime + (int)Parameters["RamseyWaitTime"] + (int)Parameters["RamseyPulseDuration"];
        int motRecaptureTime = secondRamseyPulseTime + (int)Parameters["RamseyPulseDuration"];
        int imageTime = motRecaptureTime + (int)Parameters["MOTWaitBeforeImage"];

        MOTMasterScriptSnippet lm = new LoadMoleculeMOTNoSlowingEdge(p, Parameters);  // This is how you load "preset" patterns. 

        p.Pulse(patternStartBeforeQ, microwavePulseTime, (int)Parameters["MicrowavePulseDuration"], "microwaveB");
        p.Pulse(patternStartBeforeQ, secondMicrowavePulseTime, (int)Parameters["SecondMicrowavePulseDuration"], "microwaveA");
        p.Pulse(patternStartBeforeQ, firstRamseyPulseTime, (int)Parameters["RamseyPulseDuration"], "microwaveC");
        p.Pulse(patternStartBeforeQ, secondRamseyPulseTime, (int)Parameters["RamseyPulseDuration"], "microwaveC");


        p.Pulse(patternStartBeforeQ, (int)Parameters["MOTSwitchOffTime"], (int)Parameters["MolassesDelay"], "v00MOTAOM"); // pulse off the MOT light whilst MOT fields are turning off
        p.Pulse(patternStartBeforeQ, microwavePulseTime, motRecaptureTime - microwavePulseTime, "v00MOTAOM"); // turn off the MOT light for microwave pulse

        p.Pulse(patternStartBeforeQ, blowAwayTime, (int)Parameters["PokeDuration"], "bXSlowingAOM"); // Blow away
        p.AddEdge("bXSlowingAOM", patternStartBeforeQ + (int)Parameters["slowingAOMOffStart"] + (int)Parameters["slowingAOMOffDuration"], true); // send slowing aom high and hold it high
        p.AddEdge("v10SlowingAOM", patternStartBeforeQ + (int)Parameters["slowingRepumpAOMOffStart"] + (int)Parameters["slowingRepumpAOMOffDuration"], true); // send slowing repump aom high and hold it high

        p.Pulse(patternStartBeforeQ, secondMicrowavePulseTime - 1400, (int)Parameters["MagTrapDuration"] + 3000, "bXSlowingShutter"); //Takes 14ms to start closing
        p.Pulse(patternStartBeforeQ, microwavePulseTime - 1600, motRecaptureTime - microwavePulseTime + 1600 - 1080, "v00MOTShutter");

        //p.Pulse(patternStartBeforeQ, offsetFieldTurnOnTime, offsetFieldTurnOffTime - offsetFieldTurnOnTime, "bottomCoilDirection");
        // p.Pulse(patternStartBeforeQ, blowAwayTime, offsetFieldTurnOnTime - blowAwayTime, "bottomCoilDirection");

        p.Pulse(patternStartBeforeQ, (int)Parameters["MOTPictureTriggerTime"], (int)Parameters["Frame0TriggerDuration"], "cameraTrigger"); // camera trigger for picture of initial MOT
        p.Pulse(patternStartBeforeQ, imageTime, (int)Parameters["Frame0TriggerDuration"], "cameraTrigger"); // camera trigger

        return p;
    }

    public override AnalogPatternBuilder GetAnalogPattern()
    {
        AnalogPatternBuilder p = new AnalogPatternBuilder((int)Parameters["PatternLength"]);

        MOTMasterScriptSnippet lm = new LoadMoleculeMOTNoSlowingEdge(p, Parameters);

        int patternStartBeforeQ = (int)Parameters["TCLBlockStart"];
        int molassesStartTime = (int)Parameters["MOTSwitchOffTime"] + (int)Parameters["MolassesDelay"];
        int molassesRampTime = molassesStartTime + (int)Parameters["MolassesHoldTime"];
        int v0F0PumpStartTime = molassesRampTime + (int)Parameters["MolassesRampDuration"];
        int microwavePulseTime = v0F0PumpStartTime + (int)Parameters["v0F0PumpDuration"];
        int blowAwayTime = microwavePulseTime + (int)Parameters["MicrowavePulseDuration"];
        int secondMicrowavePulseTime = blowAwayTime + (int)Parameters["PokeDuration"];
        int magTrapStartTime = secondMicrowavePulseTime + (int)Parameters["SecondMicrowavePulseDuration"];
        int firstRamseyPulseTime = magTrapStartTime + (int)Parameters["MagTrapDuration"];
        int secondRamseyPulseTime = firstRamseyPulseTime + (int)Parameters["RamseyWaitTime"] + (int)Parameters["RamseyPulseDuration"];
        int motRecaptureTime = secondRamseyPulseTime + (int)Parameters["RamseyPulseDuration"];
        int imageTime = motRecaptureTime + (int)Parameters["MOTWaitBeforeImage"];

        // Add Analog Channels
        p.AddChannel("v00Intensity");
        p.AddChannel("v00Frequency");
        p.AddChannel("xShimCoilCurrent");
        p.AddChannel("yShimCoilCurrent");
        p.AddChannel("v00EOMAmp");
        p.AddChannel("zShimCoilCurrent");

        // Slowing field
        p.AddAnalogValue("slowingCoilsCurrent", 0, (double)Parameters["slowingCoilsValue"]);
        p.AddAnalogValue("slowingCoilsCurrent", (int)Parameters["slowingCoilsOffTime"], 0.0);

        // B Field
        p.AddAnalogValue("MOTCoilsCurrent", (int)Parameters["MOTCoilsSwitchOn"], (double)Parameters["MOTCoilsCurrentRampStartValue"]);
        p.AddLinearRamp("MOTCoilsCurrent", (int)Parameters["MOTCoilsCurrentRampStartTime"], (int)Parameters["MOTCoilsCurrentRampDuration"], (double)Parameters["MOTCoilsCurrentRampEndValue"]);
        p.AddAnalogValue("MOTCoilsCurrent", (int)Parameters["MOTSwitchOffTime"], (double)Parameters["MOTCoilsCurrentMolassesValue"]);
        p.AddAnalogValue("MOTCoilsCurrent", magTrapStartTime, (double)Parameters["MOTCoilsCurrentMagTrapValue"]);
        p.AddAnalogValue("MOTCoilsCurrent", motRecaptureTime, (double)Parameters["MOTCoilsCurrentRampStartValue"]);
        p.AddAnalogValue("MOTCoilsCurrent", (int)Parameters["CoilsSwitchOffTime"], (double)Parameters["MOTCoilsCurrentMolassesValue"]);

        // Shim Fields
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
        return p;
    }

}
