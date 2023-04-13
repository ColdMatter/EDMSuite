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
        Parameters["PatternLength"] = 80000;
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
        Parameters["MicrowavePulseDuration"] = 6;// 15;
        Parameters["SecondMicrowavePulseDuration"] = 5;
        Parameters["WaitBeforeRecapture"] = 0;
        Parameters["MOTWaitBeforeImage"] = 300;

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
        Parameters["SlowingChirpHoldDuration"] = 16000;

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
        Parameters["yShimOPCurrent"] = 2.0;// -1.92 is zero
        Parameters["zShimOPCurrent"] = -0.22;// -0.22 is zero

        //Shim fields blow away
        Parameters["xShimBlowAwayCurrent"] = 5.0;// -1.35 is zero
        Parameters["yShimBlowAwayCurrent"] = 0.0;// -1.92 is zero
        Parameters["zShimBlowAwayCurrent"] = -6.0;// -0.22 is zero




        // v0 Light Intensity
        Parameters["v0IntensityRampDuration"] = 500;
        Parameters["MOTHoldTime"] = 1000;
        Parameters["v0IntensityRampStartValue"] = 5.6;
        Parameters["v0IntensityRampEndValue"] = 7.78;
        Parameters["v0IntensityMolassesValue"] = 5.6;
        Parameters["v0IntensityF0PumpValue"] = 9.3; //9.3

        // v0 Light Frequency
        Parameters["v0FrequencyMOTValue"] = 0.0; //set this to 0.0 for 114.1MHz 
        Parameters["v0FrequencyMolassesValue"] = 22.0; //set this to MHz detuning desired if doing frequency jump (positive for blue detuning)
        Parameters["v0FrequencyF0PumpValue"] = 2.0; //set this to MHz detuning desired if doing frequency jump (positive for blue detuning)

        // v0 pumping EOM
        Parameters["v0EOMMOTValue"] = 4.85;
        Parameters["v0EOMPumpValue"] = 1.7; //2.5
        Parameters["v0F0PumpDuration"] = 50;

        // v0 Light Frequency
        Parameters["v0FrequencyStartValue"] = 0.0; //set this to 0.0 for 114.1MHz 
        Parameters["v0FrequencyNewValue"] = 22.0; //set this to MHz detuning desired if doing frequency jump (positive for blue detuning)
        Parameters["v0FrequencyOPValue"] = 4.0;

        //v0aomCalibrationValues
        Parameters["calibGradient"] = 11.4;

        Parameters["PumpDuration"] = 150;

        Parameters["BlowAwayDuration"] = 100;
        Parameters["PokeDetuningValue"] = -1.4;
        Parameters["MQTHoldDuration"] = 10000;



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
        int pumpStartTime = molassesEndTime;
        int pumpEndTime = pumpStartTime + (int)Parameters["PumpDuration"];
        int microwavePulseTime = pumpEndTime + 10;
        int blowAwayTime = microwavePulseTime + (int)Parameters["MicrowavePulseDuration"];
        int secondMicrowavePulseTime = blowAwayTime + (int)Parameters["MQTHoldDuration"] + (int)Parameters["BlowAwayDuration"] + 50;
        int motRecaptureTime = secondMicrowavePulseTime + (int)Parameters["SecondMicrowavePulseDuration"] + (int)Parameters["WaitBeforeRecapture"];
        int finalImageTime = motRecaptureTime + (int)Parameters["MOTWaitBeforeImage"];
        MOTMasterScriptSnippet lm = new LoadMoleculeMOTNoSlowingEdge(p, Parameters);


        //Microwave pulse:
        p.Pulse(patternStartBeforeQ, microwavePulseTime, (int)Parameters["MicrowavePulseDuration"], "microwaveB");
        p.Pulse(patternStartBeforeQ, secondMicrowavePulseTime, (int)Parameters["SecondMicrowavePulseDuration"], "microwaveA");

        //V00 AOM switch:
        p.Pulse(patternStartBeforeQ, motSwitchOffTime, (int)Parameters["MolassesDelay"], "v00MOTAOM"); // pulse off the MOT light whilst MOT fields are turning off and V00 detuning is jumped
        p.Pulse(patternStartBeforeQ, pumpStartTime, motRecaptureTime - pumpStartTime, "v00MOTAOM"); // turn off the MOT light for microwave pulse

        p.Pulse(patternStartBeforeQ, firstImageTime, (int)Parameters["Frame0TriggerDuration"], "cameraTrigger"); // camera trigger for picture of MOT at 20 percent intensity
        //p.Pulse(patternStartBeforeQ, finalImageTime, (int)Parameters["Frame0TriggerDuration"], "cameraTrigger"); // camera trigger


        p.AddEdge("dipoleTrapAOM", 0, true);
        p.AddEdge("dipoleTrapAOM", patternStartBeforeQ + pumpStartTime, false);
        p.AddEdge("dipoleTrapAOM", patternStartBeforeQ + pumpEndTime, true);

        // Blow away:
        p.Pulse(patternStartBeforeQ, blowAwayTime, (int)Parameters["BlowAwayDuration"], "bXSlowingAOM");
        p.AddEdge("bXSlowingAOM", patternStartBeforeQ + (int)Parameters["slowingAOMOffStart"] + (int)Parameters["slowingAOMOffDuration"], true);
        p.AddEdge("v10SlowingAOM", patternStartBeforeQ + (int)Parameters["slowingRepumpAOMOffStart"] + (int)Parameters["slowingRepumpAOMOffDuration"], true);

        //Mechanical CaF shutters:
        p.Pulse(patternStartBeforeQ, secondMicrowavePulseTime - 1500, finalImageTime - secondMicrowavePulseTime + 4000, "bXSlowingShutter"); //B-X shutter closed after blow away
        p.Pulse(patternStartBeforeQ, microwavePulseTime - 1200, motRecaptureTime - microwavePulseTime - 900, "v00MOTShutter"); //V00 shutter closed after optical pumping an opened for recpature into MOT

        return p;
    }

    public override AnalogPatternBuilder GetAnalogPattern()
    {
        AnalogPatternBuilder p = new AnalogPatternBuilder((int)Parameters["PatternLength"]);

        MOTMasterScriptSnippet lm = new LoadMoleculeMOTNoSlowingEdge(p, Parameters);

        int motLoadEndTime = (int)Parameters["MOTLoadDuration"];
        int firstImageTime = motLoadEndTime + (int)Parameters["v0IntensityRampDuration"];
        int motSwitchOffTime = firstImageTime + (int)Parameters["MOTHoldTime"];
        int molassesStartTime = motSwitchOffTime + (int)Parameters["MolassesDelay"];
        int molassesEndTime = molassesStartTime + (int)Parameters["MolassesHoldTime"];
        int pumpStartTime = molassesEndTime;
        int pumpEndTime = pumpStartTime + (int)Parameters["PumpDuration"];
        int microwavePulseTime = pumpEndTime + 10;
        int blowAwayTime = microwavePulseTime + (int)Parameters["MicrowavePulseDuration"];
        int secondMicrowavePulseTime = blowAwayTime + (int)Parameters["MQTHoldDuration"] + (int)Parameters["BlowAwayDuration"] + 50;
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
        p.AddLinearRamp("MOTCoilsCurrent", motLoadEndTime, (int)Parameters["v0IntensityRampDuration"], (double)Parameters["MOTCoilsCurrentRampEndValue"]);
        p.AddAnalogValue("MOTCoilsCurrent", motSwitchOffTime, -0.05);
        p.AddAnalogValue("MOTCoilsCurrent", blowAwayTime, 1.0);
        p.AddAnalogValue("MOTCoilsCurrent", secondMicrowavePulseTime, -0.05);
        p.AddAnalogValue("MOTCoilsCurrent", motRecaptureTime, 1.0);
        p.AddAnalogValue("MOTCoilsCurrent", finalImageTime + 1100, 0.0);


        //Shim fields for OP
        p.AddAnalogValue("xShimCoilCurrent", 0, (double)Parameters["xShimOPCurrent"]);
        p.AddAnalogValue("yShimCoilCurrent", 0, (double)Parameters["yShimOPCurrent"]);
        p.AddAnalogValue("zShimCoilCurrent", 0, (double)Parameters["zShimOPCurrent"]);

        // Shim Fields for blow away
        p.AddAnalogValue("xShimCoilCurrent", microwavePulseTime, (double)Parameters["xShimBlowAwayCurrent"]);
        p.AddAnalogValue("yShimCoilCurrent", microwavePulseTime, (double)Parameters["yShimBlowAwayCurrent"]);
        p.AddAnalogValue("zShimCoilCurrent", microwavePulseTime, (double)Parameters["zShimBlowAwayCurrent"]);

        // v0 Intensity Ramp
        p.AddAnalogValue("v00Intensity", 0, (double)Parameters["v0IntensityRampStartValue"]);
        p.AddLinearRamp("v00Intensity", motLoadEndTime, (int)Parameters["v0IntensityRampDuration"], (double)Parameters["v0IntensityRampEndValue"]);
        p.AddAnalogValue("v00Intensity", motSwitchOffTime, (double)Parameters["v0IntensityMolassesValue"]);
        p.AddAnalogValue("v00Intensity", motRecaptureTime, (double)Parameters["v0IntensityRampEndValue"]);

        // v0 EOM
        p.AddAnalogValue("v00EOMAmp", 0, (double)Parameters["v0EOMMOTValue"]);

        // v0 Frequency Ramp
        p.AddAnalogValue("v00Frequency", 0, 10.0 - (double)Parameters["v0FrequencyStartValue"] / (double)Parameters["calibGradient"]);
        p.AddAnalogValue("v00Frequency", motSwitchOffTime, 10.0 - (double)Parameters["v0FrequencyNewValue"] / (double)Parameters["calibGradient"]);//jump to blue detuning for blue molasses
        p.AddAnalogValue("v00Frequency", motRecaptureTime, 10.0 - (double)Parameters["v0FrequencyStartValue"] / (double)Parameters["calibGradient"]); //jump aom frequency back to normal for imaging

        return p;
    }

}
