﻿using MOTMaster;
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

        // Camera
        Parameters["CameraTriggerDuration"] = 10;
        Parameters["CameraExposure"] = 1000;

        // Delays
        Parameters["MolassesDelay"] = 600;
        Parameters["WaitBeforeImage"] = 500;
        Parameters["MOTRecaptureDelay"] = 100;

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
        Parameters["slowingCoilsValue"] = 0.42; //1.1;
        Parameters["slowingCoilsOffTime"] = 4000;

        // Shim fields
        Parameters["xShimLoadCurrent"] = -1.35;// -1.35 is zero
        Parameters["yShimLoadCurrent"] = -1.92;// -1.92 is zero
        Parameters["zShimLoadCurrent"] = -0.22;// -0.22 is zero
        
        // v0 Light intensity
        Parameters["v0IntensityRampStartValue"] = 5.6;
        Parameters["v0IntensityRampEndValue"] = 8.0;
        Parameters["v0IntensityCMOTValue"] = 8.0;
        Parameters["v0IntensityMolassesValue"] = 5.6;

        // v0 light durations
        Parameters["MOTLoadDuration"] = 4000;
        Parameters["v0IntensityRampDuration"] = 100;
        Parameters["MOTHoldDuration"] = 2000;
        Parameters["CMOTRampDuration"] = 800;
        Parameters["CMOTHoldDuration"] = 100;
        Parameters["MolassesHoldDuration"] = 600;

        // v0 Light Frequency (0.0 for 114.1MHz)
        Parameters["v0FrequencyMOTValue"] = 0.0;
        Parameters["v0FrequencyCMOTValue"] = 3.5;
        Parameters["v0FrequencyMolassesValue"] = 22.0;

        // B field
        Parameters["MOTFieldValue"] = 1.0;
        Parameters["CMOTFieldValue"] = 1.0;

        // OP
        Parameters["OPDuration"] = 150;

        //v0aomCalibrationValues
        Parameters["calibGradient"] = 11.4;


    }

    public override PatternBuilder32 GetDigitalPattern()
    {
        PatternBuilder32 p = new PatternBuilder32();
        int patternStartBeforeQ = (int)Parameters["TCLBlockStart"];
        int motLoadEndTime = patternStartBeforeQ + (int)Parameters["MOTLoadDuration"];
        int firstImageTime = motLoadEndTime + (int)Parameters["WaitBeforeImage"];
        int motRampStartTime = firstImageTime + (int)Parameters["CameraExposure"];
        int motRampEndTime = motRampStartTime + (int)Parameters["v0IntensityRampDuration"];
        int cmotRampStartTime = motRampEndTime + (int)Parameters["MOTHoldDuration"];
        int cmotRampEndTime = cmotRampStartTime + (int)Parameters["CMOTRampDuration"];
        int cmotEndTime = cmotRampEndTime + (int)Parameters["CMOTHoldDuration"];
        int molassesStartTime = cmotEndTime + (int)Parameters["MolassesDelay"];
        int molassesEndTime = molassesStartTime + (int)Parameters["MolassesHoldDuration"];
        int motRecaptureTime = molassesEndTime + (int)Parameters["MOTRecaptureDelay"];
        int finalImageTime = motRecaptureTime + (int)Parameters["WaitBeforeImage"];
        int endOfTime = finalImageTime + 3000;

        MOTMasterScriptSnippet lm = new LoadMoleculeMOT(p, Parameters);
        //V00 AOM switch:
        p.Pulse(0, cmotEndTime, (int)Parameters["MolassesDelay"], "v00MOTAOM");
        //Camera triggers:
        p.Pulse(0, firstImageTime, (int)Parameters["CameraTriggerDuration"], "cameraTrigger");
        p.Pulse(0, finalImageTime, (int)Parameters["CameraTriggerDuration"], "cameraTrigger");
        // bX shutter
        p.Pulse(0, motLoadEndTime, endOfTime, "bXSlowingShutter");

        return p;
    }

    public override AnalogPatternBuilder GetAnalogPattern()
    {
        AnalogPatternBuilder p = new AnalogPatternBuilder((int)Parameters["PatternLength"]);

        MOTMasterScriptSnippet lm = new LoadMoleculeMOT(p, Parameters);

        int motLoadEndTime = (int)Parameters["MOTLoadDuration"];
        int firstImageTime = motLoadEndTime + (int)Parameters["WaitBeforeImage"];
        int motRampStartTime = firstImageTime + (int)Parameters["CameraExposure"];
        int motRampEndTime = motRampStartTime + (int)Parameters["v0IntensityRampDuration"];
        int cmotRampStartTime = motRampEndTime + (int)Parameters["MOTHoldDuration"];
        int cmotRampEndTime = cmotRampStartTime + (int)Parameters["CMOTRampDuration"];
        int cmotEndTime = cmotRampEndTime + (int)Parameters["CMOTHoldDuration"];
        int molassesStartTime = cmotEndTime + (int)Parameters["MolassesDelay"];
        int molassesEndTime = molassesStartTime + (int)Parameters["MolassesHoldDuration"];
        int motRecaptureTime = molassesEndTime + (int)Parameters["MOTRecaptureDelay"];
        int finalImageTime = motRecaptureTime + (int)Parameters["WaitBeforeImage"];
        int endOfTime = finalImageTime + 3000;

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
        p.AddAnalogValue("MOTCoilsCurrent", 0, (double)Parameters["MOTFieldValue"]);
        p.AddLinearRamp("MOTCoilsCurrent", cmotRampStartTime, (int)Parameters["CMOTRampDuration"], (double)Parameters["CMOTFieldValue"]);
        p.AddAnalogValue("MOTCoilsCurrent", cmotRampEndTime, -0.05);
        p.AddAnalogValue("MOTCoilsCurrent", motRecaptureTime, (double)Parameters["MOTFieldValue"]);
        p.AddAnalogValue("MOTCoilsCurrent", endOfTime, -0.05);
        // Shim Fields
        p.AddAnalogValue("xShimCoilCurrent", 0, (double)Parameters["xShimLoadCurrent"]);
        p.AddAnalogValue("yShimCoilCurrent", 0, (double)Parameters["yShimLoadCurrent"]);
        p.AddAnalogValue("zShimCoilCurrent", 0, (double)Parameters["zShimLoadCurrent"]);
        // v0 Intensity
        p.AddAnalogValue("v00Intensity", 0, (double)Parameters["v0IntensityRampStartValue"]);
        p.AddLinearRamp("v00Intensity", motRampStartTime, (int)Parameters["v0IntensityRampDuration"], (double)Parameters["v0IntensityRampEndValue"]);
        p.AddAnalogValue("v00Intensity", cmotRampStartTime, (double)Parameters["v0IntensityCMOTValue"]);
        p.AddAnalogValue("v00Intensity", cmotEndTime, (double)Parameters["v0IntensityMolassesValue"]);
        p.AddAnalogValue("v00Intensity", molassesEndTime, (double)Parameters["v0IntensityRampStartValue"]);
        // v0 Frequency
        p.AddAnalogValue("v00Frequency", 0, 10.0 - (double)Parameters["v0FrequencyMOTValue"] / (double)Parameters["calibGradient"]);
        p.AddAnalogValue("v00Frequency", cmotRampStartTime, 10.0 - (double)Parameters["v0FrequencyCMOTValue"] / (double)Parameters["calibGradient"]);
        p.AddLinearRamp("v00Frequency", cmotEndTime, 100, 10.0 - (double)Parameters["v0FrequencyMolassesValue"] / (double)Parameters["calibGradient"]);
        //p.AddAnalogValue("v00Frequency", cmotEndTime + 100, 10.0 - (double)Parameters["v0FrequencyMolassesValue"] / (double)Parameters["calibGradient"]);
        p.AddAnalogValue("v00Frequency", molassesEndTime, 10.0 - (double)Parameters["v0FrequencyMOTValue"] / (double)Parameters["calibGradient"]);
        // v0 frequency with EOM
        p.AddAnalogValue("v00EOMAmp", 0, 4.85);


        return p;
    }

}
