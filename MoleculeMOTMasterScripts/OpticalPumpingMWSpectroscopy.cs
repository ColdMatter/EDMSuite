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
        Parameters["HeliumShutterToQ"] = 100;
        Parameters["HeliumShutterDuration"] = 1550;

        Parameters["RbMOTLoadTime"] = 0;


        Parameters["MolassesDelay"] = 100;
        Parameters["MolassesHoldTime"] = 500; //1200
        Parameters["MicrowavePulseDuration"] = 13;// 15;
        Parameters["WaitBeforeRecapture"] = 100;
        Parameters["MOTWaitBeforeImage"] = 10;

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
        Parameters["MOTCoilsCurrentRampEndValue"] = 3.0;

        // Shim fields
        Parameters["xShimLoadCurrent"] = -1.35;// -1.35 is zero
        Parameters["yShimLoadCurrent"] = -1.92;// -1.92 is zero
        Parameters["zShimLoadCurrent"] = -0.22;// -0.22 is zero

        //Shim fields for OP
        Parameters["xShimOPCurrent"] = -1.35;// -1.35 is zero
        Parameters["yShimOPCurrent"] = 4.0;// -1.92 is zero
        Parameters["zShimOPCurrent"] = -0.22;// -0.22 is zero

        Parameters["ShimFieldSettlingDuration"] = 200;


        // v0 Light Intensity
        Parameters["v0IntensityRampDuration"] = 500;
        Parameters["MOTHoldTime"] = 1200;
        Parameters["v0IntensityRampStartValue"] = 5.6;
        Parameters["v0IntensityRampEndValue"] = 7.78;
        Parameters["v0IntensityMolassesValue"] = 5.6;
        Parameters["v0IntensityF0PumpValue"] = 9.3; //9.3

        // v0 Light Frequency
        Parameters["v0FrequencyMOTValue"] = 0.0; //set this to 0.0 for 114.1MHz 
        Parameters["v0FrequencyF0PumpValue"] = 2.0; //set this to MHz detuning desired if doing frequency jump (positive for blue detuning)

        // v0 pumping EOM
        Parameters["v0EOMMOTValue"] = 4.85;
        Parameters["v0EOMPumpValue"] = 1.7; //2.5
        Parameters["v0F0PumpDuration"] = 50;

        // v0 Light Frequency
        Parameters["v0FrequencyStartValue"] = 0.0; //set this to 0.0 for 114.1MHz 
        Parameters["v0FrequencyNewValue"] = 23.0; //set this to MHz detuning desired if doing frequency jump (positive for blue detuning)
        Parameters["v0FrequencyOPValue"] = 4.0;

        //v0aomCalibrationValues
        Parameters["calibGradient"] = 11.4;

        Parameters["PumpDuration"] = 20;




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
        int pumpStartTime = molassesEndTime + (int)Parameters["ShimFieldSettlingDuration"];
        int pumpEndTime = pumpStartTime + (int)Parameters["PumpDuration"];
        int microwavePulseTime = pumpEndTime + 10;
        int motRecaptureTime = microwavePulseTime + (int)Parameters["MicrowavePulseDuration"] + (int)Parameters["WaitBeforeRecapture"];
        int finalImageTime = motRecaptureTime + (int)Parameters["MOTWaitBeforeImage"];
        MOTMasterScriptSnippet lm = new LoadMoleculeMOT(p, Parameters);  // This is how you load "preset" patterns. 


        //Microwave pulse:
        p.Pulse(patternStartBeforeQ, microwavePulseTime, (int)Parameters["MicrowavePulseDuration"], "microwaveA");

        //V00 AOM switch:
        p.Pulse(patternStartBeforeQ, motSwitchOffTime, (int)Parameters["MolassesDelay"], "v00MOTAOM"); // pulse off the MOT light whilst MOT fields are turning off and V00 detuning is jumped
        p.Pulse(patternStartBeforeQ, pumpStartTime, motRecaptureTime - pumpStartTime, "v00MOTAOM"); // turn off the MOT light for microwave pulse

        p.Pulse(patternStartBeforeQ, firstImageTime, (int)Parameters["Frame0TriggerDuration"], "cameraTrigger"); // camera trigger for picture of MOT at 20 percent intensity
        p.Pulse(patternStartBeforeQ, finalImageTime, (int)Parameters["Frame0TriggerDuration"], "cameraTrigger"); // camera trigger

        //p.AddEdge("dipoleTrapAOM", 0, false);

        p.AddEdge("cafOptPumpingAOM", 0, false);
        //p.AddEdge("cafOptPumpingAOM", patternStartBeforeQ + pumpStartTime, true);
        //p.AddEdge("cafOptPumpingAOM", patternStartBeforeQ + pumpEndTime, false);

        p.AddEdge("rb2DMOTShutter", 0, true);

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
        int pumpStartTime = molassesEndTime + (int)Parameters["ShimFieldSettlingDuration"];
        int pumpEndTime = pumpStartTime + (int)Parameters["PumpDuration"];
        int microwavePulseTime = pumpEndTime + 10;
        int motRecaptureTime = microwavePulseTime + (int)Parameters["MicrowavePulseDuration"] + (int)Parameters["WaitBeforeRecapture"];
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

        //Shim fields
        p.AddAnalogValue("xShimCoilCurrent", 0, (double)Parameters["xShimLoadCurrent"]);
        p.AddAnalogValue("yShimCoilCurrent", 0, (double)Parameters["yShimLoadCurrent"]);
        p.AddAnalogValue("zShimCoilCurrent", 0, (double)Parameters["zShimLoadCurrent"]);

        //Shim fields for OP
        p.AddAnalogValue("xShimCoilCurrent", molassesEndTime, (double)Parameters["xShimOPCurrent"]);
        p.AddAnalogValue("yShimCoilCurrent", molassesEndTime, (double)Parameters["yShimOPCurrent"]);
        p.AddAnalogValue("zShimCoilCurrent", molassesEndTime, (double)Parameters["zShimOPCurrent"]);

        //Switch shim fields back to zero lab field
        p.AddAnalogValue("xShimCoilCurrent", motRecaptureTime, (double)Parameters["xShimLoadCurrent"]);
        p.AddAnalogValue("yShimCoilCurrent", motRecaptureTime, (double)Parameters["yShimLoadCurrent"]);
        p.AddAnalogValue("zShimCoilCurrent", motRecaptureTime, (double)Parameters["zShimLoadCurrent"]);

        // v0 Intensity Ramp
        p.AddAnalogValue("v00Intensity", 0, (double)Parameters["v0IntensityRampStartValue"]);
        p.AddLinearRamp("v00Intensity", motLoadEndTime, (int)Parameters["v0IntensityRampDuration"], (double)Parameters["v0IntensityRampEndValue"]);
        p.AddAnalogValue("v00Intensity", motSwitchOffTime, (double)Parameters["v0IntensityMolassesValue"]);
        p.AddAnalogValue("v00Intensity", motRecaptureTime, (double)Parameters["v0IntensityRampEndValue"]);

        // v0 EOM
        p.AddAnalogValue("v00EOMAmp", 0, (double)Parameters["v0EOMMOTValue"]);
        //p.AddAnalogValue("v00EOMAmp", 0, 1.0);

        // v0 Frequency Ramp
        p.AddAnalogValue("v00Frequency", 0, 10.0 - (double)Parameters["v0FrequencyStartValue"] / (double)Parameters["calibGradient"]);
        p.AddAnalogValue("v00Frequency", motSwitchOffTime, 10.0 - (double)Parameters["v0FrequencyNewValue"] / (double)Parameters["calibGradient"]);//jump to blue detuning for blue molasses
        p.AddAnalogValue("v00Frequency", motRecaptureTime, 10.0 - (double)Parameters["v0FrequencyStartValue"] / (double)Parameters["calibGradient"]); //jump aom frequency back to normal for imaging

        return p;
    }

}
