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
        Parameters["TCLBlockDuration"] = 11000;
        Parameters["FlashToQ"] = 16; // This is a time before the Q switch
        Parameters["QSwitchPulseDuration"] = 10;
        Parameters["FlashPulseDuration"] = 10;

        Parameters["MOTSwitchOffTime"] = 6300;
        Parameters["MolassesDelay"] = 100;
        Parameters["MolassesHoldTime"] = 600;
        Parameters["MolassesRampDuration"] = 200;
        Parameters["v0F0PumpDuration"] = 10;
        Parameters["MOTPictureTriggerTime"] = 4000;
        Parameters["MicrowavePulseDuration"] = 14;
        Parameters["MicrowaveSecondPulseDuration"] = 7;
        Parameters["MagneticTrapDuration"] = 5000; // shouldnt be less than 40ms or shutters bounce
        Parameters["MOTWaitBeforeImage"] = 0;
        Parameters["xShimZeemanSplitValue"] = 0.0;

        // BX poke
        Parameters["PokeDetuningValue"] = -1.35;
        Parameters["PokeDuration"] = 100;

        // Camera
        Parameters["Frame0TriggerDuration"] = 10;

        //PMT
        Parameters["PMTTrigger"] = 4000;
        Parameters["PMTTriggerDuration"] = 10;

        // Slowing
        Parameters["slowingAOMOnStart"] = 250;
        Parameters["slowingAOMOnDuration"] = 45000;
        Parameters["slowingAOMOffStart"] = 1500;
        Parameters["slowingAOMOffDuration"] = (int)Parameters["PatternLength"] - 10000;
        Parameters["slowingRepumpAOMOnStart"] = 250;
        Parameters["slowingRepumpAOMOnDuration"] = (int)Parameters["PatternLength"] - 5000;
        Parameters["slowingRepumpAOMOffStart"] = 1700;
        Parameters["slowingRepumpAOMOffDuration"] = (int)Parameters["PatternLength"] - 10000;

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
        Parameters["MOTCoilsCurrentRampStartValue"] = 0.65;
        Parameters["MOTCoilsCurrentRampStartTime"] = 4000;
        Parameters["MOTCoilsCurrentRampEndValue"] = 1.5;
        Parameters["MOTCoilsCurrentRampDuration"] = 1000;
        Parameters["MOTCoilsCurrentMolassesValue"] = 0.0; //0.21
        Parameters["MOTCoilsCurrentMagneticTrapValue"] = 1.5;

        Parameters["CoilsSwitchOffTime"] = (int)Parameters["PatternLength"] - 10000;

        // Shim fields
        Parameters["xShimLoadCurrent"] = 0.0;// 3.0;
        Parameters["yShimLoadCurrent"] = 0.0;
        Parameters["zShimLoadCurrent"] = -6.0;//3.32; //-6.82;

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
        Parameters["v0EOMMOTValue"] = 5.65;
        Parameters["v0EOMPumpValue"] = 4.3; //3.5

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
        int magTrapStartTime = blowAwayTime + (int)Parameters["PokeDuration"] + (int)Parameters["MicrowaveSecondPulseDuration"]; //Empty space same length as second microwave pulse to make as similar as possible to N=1 script
        int microwaveSecondPulseTime = magTrapStartTime + (int)Parameters["MagneticTrapDuration"];
        int motRecaptureTime = microwaveSecondPulseTime + (int)Parameters["MicrowavePulseDuration"];
        int imageTime = motRecaptureTime + (int)Parameters["MOTWaitBeforeImage"];

        MOTMasterScriptSnippet lm = new LoadMoleculeMOTNoSlowingEdge(p, Parameters);  // This is how you load "preset" patterns. 

        p.Pulse(patternStartBeforeQ, microwavePulseTime, (int)Parameters["MicrowavePulseDuration"], "microwaveIO"); //now linked to B channel
        p.Pulse(patternStartBeforeQ, microwaveSecondPulseTime, (int)Parameters["MicrowavePulseDuration"], "microwaveIO"); // now linked to B channel

        p.Pulse(patternStartBeforeQ, (int)Parameters["MOTSwitchOffTime"], (int)Parameters["MolassesDelay"], "v00AOM"); // pulse off the MOT light whilst MOT fields are turning off
        p.Pulse(patternStartBeforeQ, microwavePulseTime, motRecaptureTime - microwavePulseTime, "v00AOM"); // turn off the MOT light for microwave pulse

        p.Pulse(patternStartBeforeQ, blowAwayTime, (int)Parameters["PokeDuration"], "aom"); // Blow away pulse during magnetic trap 
        p.AddEdge("aom", patternStartBeforeQ + (int)Parameters["slowingAOMOffStart"] + (int)Parameters["slowingAOMOffDuration"], true); // send slowing aom high and hold it high

        //p.Pulse(patternStartBeforeQ, blowAwayTime - 600, (int)Parameters["MagneticTrapDuration"], "bXShutter");// takes 12ms to shut, want 1 ms poke, don't care when it reopens
        //p.Pulse(patternStartBeforeQ, magTrapStartTime - 1100, (int)Parameters["MagneticTrapDuration"] - 1000, "v00Shutter");// takes 12ms to start to shut (need open for optical pumping step), 21ms to reopen, want open at end of trap but closed at start

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
        int magTrapStartTime = blowAwayTime + (int)Parameters["PokeDuration"] + (int)Parameters["MicrowaveSecondPulseDuration"]; //Empty space same length as second microwave pulse to make as similar as possible to N=1 script
        int microwaveSecondPulseTime = magTrapStartTime + (int)Parameters["MagneticTrapDuration"];
        int motRecaptureTime = microwaveSecondPulseTime + (int)Parameters["MicrowaveSecondPulseDuration"];
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
        p.AddAnalogValue("MOTCoilsCurrent", magTrapStartTime, (double)Parameters["MOTCoilsCurrentMagneticTrapValue"]);
        p.AddAnalogValue("MOTCoilsCurrent", motRecaptureTime, (double)Parameters["MOTCoilsCurrentRampStartValue"]);
        p.AddAnalogValue("MOTCoilsCurrent", (int)Parameters["CoilsSwitchOffTime"], 0.0);





        //dodgy way of checking frequency after second microwave pulse
        //p.AddAnalogValue("yShimCoilCurrent", 0, 0.0);
        // p.AddAnalogValue("yShimCoilCurrent", motRecaptureTime, 5.0);

        // Shim Fields
        p.AddAnalogValue("xShimCoilCurrent", 0, (double)Parameters["xShimLoadCurrent"]);
        //  p.AddAnalogValue("yShimCoilCurrent", 0, (double)Parameters["yShimLoadCurrent"]);
        p.AddAnalogValue("zShimCoilCurrent", 0, (double)Parameters["zShimLoadCurrent"]);
        //p.AddLinearRamp("zShimCoilCurrent",v0F0PumpStartTime,50,(double)Parameters["zShimZeemanSplitValue"]);
        //p.AddAnalogValue("xShimCoilCurrent", v0F0PumpStartTime, (double)Parameters["xShimZeemanSplitValue"]);//used to be at v0f0pump time
        //p.AddAnalogValue("zShimCoilCurrent", motRecaptureTime, (double)Parameters["zShimLoadCurrent"]);

        //p.AddAnalogValue("xShimCoilCurrent", v0F0PumpStartTime, 5.0);
        //p.AddAnalogValue("xShimCoilCurrent", motRecaptureTime, (double)Parameters["xShimLoadCurrent"]);

        // v0 Intensity Ramp
        p.AddAnalogValue("v00Intensity", 0, (double)Parameters["v0IntensityRampStartValue"]);
        p.AddLinearRamp("v00Intensity", (int)Parameters["v0IntensityRampStartTime"], (int)Parameters["v0IntensityRampDuration"], (double)Parameters["v0IntensityRampEndValue"]);
        p.AddAnalogValue("v00Intensity", molassesStartTime, (double)Parameters["v0IntensityMolassesValue"]);
        p.AddAnalogValue("v00Intensity", molassesRampTime + 50, 7.4);
        p.AddAnalogValue("v00Intensity", molassesRampTime + 200, 7.83);
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