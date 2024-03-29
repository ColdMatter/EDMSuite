﻿using MOTMaster;
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
        Parameters["HeliumShutterToQ"] = 100;
        Parameters["HeliumShutterDuration"] = 1550;

        Parameters["RbMOTLoadTime"] = 0;


        Parameters["MolassesDelay"] = 100;
        Parameters["MolassesHoldTime"] = 600; //1200
        Parameters["MicrowavePulseDuration"] = 10;// 15;
        Parameters["SecondMicrowavePulseDuration"] = 11;// 15;
        Parameters["WaitBeforeRecapture"] = 100;
        Parameters["MOTWaitBeforeImage"] = 10;
        Parameters["xShimZeemanSplitValue"] = 0.0;

        // Camera
        Parameters["Frame0TriggerDuration"] = 10;

        //
        Parameters["MOTLoadDuration"] = 4000;

        // Slowing
        Parameters["slowingAOMOnStart"] = 180; //started from 250
        Parameters["slowingAOMOnDuration"] = 45000;
        Parameters["slowingAOMOffStart"] = 1520;//started from 1520
        Parameters["slowingAOMOffDuration"] = 40000;
        Parameters["slowingRepumpAOMOnStart"] = 0;//started from 0
        Parameters["slowingRepumpAOMOnDuration"] = 45000;
        Parameters["slowingRepumpAOMOffStart"] = 1520;//1520
        Parameters["slowingRepumpAOMOffDuration"] = 35000;

        // Slowing Chirp
        Parameters["SlowingChirpStartTime"] = 380;// 380;
        Parameters["SlowingChirpDuration"] = 1160; //1160
        Parameters["SlowingChirpStartValue"] = 0.0;//0.0
        Parameters["SlowingChirpEndValue"] = -1.25; //-1.25

        // Slowing field
        Parameters["slowingCoilsValue"] = 0.42;
        Parameters["slowingCoilsOffTime"] = 1500;

        // B Field
        Parameters["MOTCoilsSwitchOn"] = 0;
        Parameters["MOTCoilsCurrentRampStartValue"] = 1.0;
        Parameters["MOTCoilsCurrentMolassesValue"] = -0.05;// -0.01; //0.21

        // Shim fields
        Parameters["xShimLoadCurrent"] = -1.35;// -1.35 is zero
        Parameters["yShimLoadCurrent"] = 2.2;// -1.92 is zero // 2.0
        Parameters["zShimLoadCurrent"] = -0.22;// -0.22 is zero

        // v0 Light Intensity
        Parameters["v0IntensityRampDuration"] = 300;
        Parameters["MOTHoldTime"] = 1000;
        Parameters["v0IntensityRampStartValue"] = 5.6;
        Parameters["v0IntensityRampEndValue"] = 7.78;
        Parameters["v0IntensityMolassesValue"] = 5.6;
        Parameters["v0IntensityF0PumpValue"] = 9.3; //9.3

        // v0 Light Frequency
        Parameters["v0FrequencyMOTValue"] = 0.0; //set this to 0.0 for 114.1MHz 
        Parameters["v0FrequencyMolassesValue"] = 20.0; //set this to MHz detuning desired if doing frequency jump (positive for blue detuning)
        Parameters["v0FrequencyF0PumpValue"] = 2.0; //set this to MHz detuning desired if doing frequency jump (positive for blue detuning)

        // v0 pumping EOM
        Parameters["v0EOMMOTValue"] = 4.4;
        Parameters["v0EOMPumpValue"] = 1.0; //2.5
        Parameters["v0F0PumpDuration"] = 100;

        // v0 Light Frequency
        Parameters["v0FrequencyStartValue"] = 0.0; //set this to 0.0 for 114.1MHz 
        Parameters["v0FrequencyNewValue"] = 20.0; //set this to MHz detuning desired if doing frequency jump (positive for blue detuning)
        Parameters["v0FrequencyOPValue"] = 4.0;

        //v0aomCalibrationValues
        Parameters["calibGradient"] = 11.4;


    }

    public override PatternBuilder32 GetDigitalPattern()
    {
        PatternBuilder32 p = new PatternBuilder32();
        int patternStartBeforeQ = (int)Parameters["TCLBlockStart"];
        int motLoadEndTime = (int)Parameters["MOTLoadDuration"];
        int firstImageTime = motLoadEndTime + (int)Parameters["v0IntensityRampDuration"];
        int motSwitchOffTime = firstImageTime + (int)Parameters["MOTHoldTime"];
        int molassesStartTime = motSwitchOffTime + (int)Parameters["MolassesDelay"];
        int molassesEndTime = molassesStartTime + (int)Parameters["MolassesHoldTime"];
        int v0F0PumpStartTime = molassesEndTime;
        int v0F0PumpEndTime = v0F0PumpStartTime + (int)Parameters["v0F0PumpDuration"];
        int microwavePulseTime = v0F0PumpEndTime + 80;
        int secondMicrowavePulseTime = microwavePulseTime + 5 + (int)Parameters["MicrowavePulseDuration"];
        int motRecaptureTime = secondMicrowavePulseTime + (int)Parameters["SecondMicrowavePulseDuration"] + (int)Parameters["WaitBeforeRecapture"];
        int finalImageTime = motRecaptureTime + (int)Parameters["MOTWaitBeforeImage"];
        MOTMasterScriptSnippet lm = new LoadMoleculeMOT(p, Parameters);  // This is how you load "preset" patterns. 


        //Microwave pulse:
        p.Pulse(patternStartBeforeQ, microwavePulseTime, (int)Parameters["MicrowavePulseDuration"], "microwaveA");
        p.Pulse(patternStartBeforeQ, secondMicrowavePulseTime, (int)Parameters["SecondMicrowavePulseDuration"], "microwaveB");

        //V00 AOM switch:
        p.Pulse(patternStartBeforeQ, motSwitchOffTime, (int)Parameters["MolassesDelay"], "v00MOTAOM"); // pulse off the MOT light whilst MOT fields are turning off and V00 detuning is jumped
        p.Pulse(patternStartBeforeQ, v0F0PumpEndTime, motRecaptureTime - v0F0PumpEndTime, "v00MOTAOM"); // turn off the MOT light for microwave pulse

        p.Pulse(patternStartBeforeQ, firstImageTime, (int)Parameters["Frame0TriggerDuration"], "cameraTrigger"); // camera trigger for picture of MOT at 20 percent intensity
        p.Pulse(patternStartBeforeQ, finalImageTime, (int)Parameters["Frame0TriggerDuration"], "cameraTrigger"); // camera trigger

        p.Pulse(patternStartBeforeQ, motLoadEndTime, (int)Parameters["Frame0TriggerDuration"], "rbAbsImgCamTrig");

        return p;
    }

    public override AnalogPatternBuilder GetAnalogPattern()
    {
        AnalogPatternBuilder p = new AnalogPatternBuilder((int)Parameters["PatternLength"]);

        MOTMasterScriptSnippet lm = new LoadMoleculeMOT(p, Parameters);

        int motLoadEndTime = (int)Parameters["MOTLoadDuration"];
        int firstImageTime = motLoadEndTime + (int)Parameters["v0IntensityRampDuration"];
        int motSwitchOffTime = firstImageTime + (int)Parameters["MOTHoldTime"];
        int molassesStartTime = motSwitchOffTime + (int)Parameters["MolassesDelay"];
        int molassesEndTime = molassesStartTime + (int)Parameters["MolassesHoldTime"];
        int v0F0PumpStartTime = molassesEndTime;
        int v0F0PumpEndTime = v0F0PumpStartTime + (int)Parameters["v0F0PumpDuration"];
        int microwavePulseTime = v0F0PumpEndTime + 80;
        int secondMicrowavePulseTime = microwavePulseTime + 5 + (int)Parameters["MicrowavePulseDuration"];
        int motRecaptureTime = secondMicrowavePulseTime + (int)Parameters["SecondMicrowavePulseDuration"] + (int)Parameters["WaitBeforeRecapture"];
        int finalImageTime = motRecaptureTime + (int)Parameters["MOTWaitBeforeImage"];

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
        p.AddAnalogValue("MOTCoilsCurrent", 0, 1.0);
        p.AddAnalogValue("MOTCoilsCurrent", motSwitchOffTime, -0.05);
        p.AddAnalogValue("MOTCoilsCurrent", motRecaptureTime, 1.0);
        p.AddAnalogValue("MOTCoilsCurrent", finalImageTime + 1100, 0.0);


        // Shim Fields
        p.AddAnalogValue("xShimCoilCurrent", 0, (double)Parameters["xShimLoadCurrent"]);
        p.AddAnalogValue("yShimCoilCurrent", 0, (double)Parameters["yShimLoadCurrent"]);
        p.AddAnalogValue("zShimCoilCurrent", 0, (double)Parameters["zShimLoadCurrent"]);

        // v0 Intensity Ramp
        p.AddAnalogValue("v00Intensity", 0, (double)Parameters["v0IntensityRampStartValue"]);
        p.AddLinearRamp("v00Intensity", motLoadEndTime, (int)Parameters["v0IntensityRampDuration"], (double)Parameters["v0IntensityRampEndValue"]);
        p.AddAnalogValue("v00Intensity", motSwitchOffTime, (double)Parameters["v0IntensityMolassesValue"]);
        p.AddAnalogValue("v00Intensity", v0F0PumpStartTime, (double)Parameters["v0IntensityF0PumpValue"]);
        p.AddAnalogValue("v00Intensity", motRecaptureTime, (double)Parameters["v0IntensityRampEndValue"]);

        // v0 EOM
        p.AddAnalogValue("v00EOMAmp", 0, (double)Parameters["v0EOMMOTValue"]);
        p.AddAnalogValue("v00EOMAmp", v0F0PumpStartTime, (double)Parameters["v0EOMPumpValue"]);
        p.AddAnalogValue("v00EOMAmp", v0F0PumpEndTime, (double)Parameters["v0EOMMOTValue"]);



        // v0 Frequency Ramp
        p.AddAnalogValue("v00Frequency", 0, 10.0 - (double)Parameters["v0FrequencyStartValue"] / (double)Parameters["calibGradient"]);
        p.AddAnalogValue("v00Frequency", motSwitchOffTime, 10.0 - (double)Parameters["v0FrequencyNewValue"] / (double)Parameters["calibGradient"]);//jump to blue detuning for blue molasses
        p.AddAnalogValue("v00Frequency", v0F0PumpStartTime, 10.0 - (double)Parameters["v0FrequencyOPValue"] / (double)Parameters["calibGradient"]);//jump to OP value
        p.AddAnalogValue("v00Frequency", motRecaptureTime, 10.0 - (double)Parameters["v0FrequencyStartValue"] / (double)Parameters["calibGradient"]); //jump aom frequency back to normal for imaging

        return p;
    }

}
