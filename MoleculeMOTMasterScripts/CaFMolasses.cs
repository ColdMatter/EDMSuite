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
        Parameters["TCLBlockStart"] = 4000; // This is a time before the Q switch
        Parameters["TCLBlockDuration"] = 15000;
        Parameters["FlashToQ"] = 16; // This is a time before the Q switch
        Parameters["QSwitchPulseDuration"] = 10;
        Parameters["FlashPulseDuration"] = 10;
        Parameters["HeliumShutterToQ"] = 300;
        Parameters["HeliumShutterDuration"] = 2000;

        // Camera
        Parameters["Frame0Trigger"] = 5000;
        Parameters["Frame0TriggerDuration"] = 10;
        Parameters["CameraTriggerTransverseTime"] = 120;
        Parameters["FrameTriggerInterval"] = 1100;
        Parameters["waitbeforeimage"] = 1;

        //PMT
        Parameters["PMTTriggerDuration"] = 10;

        // Slowing
        Parameters["slowingAOMOnStart"] = 160;//160
        Parameters["PushStart"] = 200;
        Parameters["PushEnd"] = 400;
        Parameters["slowingAOMOnDuration"] = 45000;
        
        Parameters["slowingAOMOffStart"] = 1750;//1760;//started from 1520
        Parameters["slowingAOMOffDuration"] = 40000;


        
        Parameters["slowingRepumpAOMOnStart"] = 0;//started from 0
        Parameters["slowingRepumpAOMOffStart"] = 1750;// 1760;//1520
        Parameters["slowingRepumpAOMOffDuration"] = 35000;


        // Slowing Chirp
        Parameters["SlowingChirpStartTime"] = 400;
        Parameters["SlowingChirpDuration"] = 1350;
        Parameters["SlowingChirpStartValue"] = 0.0;//0.0
        Parameters["SlowingChirpEndValue"] = -1.2;

        // Slowing field
        Parameters["slowingCoilsValue"] = 0.4; //1.05;
        Parameters["slowingCoilsOffTime"] = 2000; // 1500;

        // B Field
        Parameters["MOTCoilsSwitchOn"] = 0;
        Parameters["MOTCoilsSwitchOff"] = 20000;
        Parameters["MOTCoilsCurrentValue"] = 1.0; // 0.65;

        // Shim fields
        Parameters["xShimLoadCurrent"] = -1.35;
        Parameters["yShimLoadCurrent"] = -1.92;
        Parameters["zShimLoadCurrent"] = -0.22;


        // v0 Light Switch
        Parameters["MOTAOMStartTime"] = 15000;
        Parameters["MOTAOMDuration"] = 500;

        // v0 Light Intensity
        Parameters["v0IntensityRampStartTime"] = 5000;
        Parameters["v0IntensityRampDuration"] = 500;
        Parameters["v0IntensityRampStartValue"] = 7.2; //5.6
        Parameters["v0IntensityEndValue"] = 8.2;//7.8
        Parameters["v0IntensityMolassesValue"] = 7.2;
        Parameters["v0IntensityMolassesEndValue"] = 8.0;
        Parameters["v0IntensityRampBackTime"] = 20000;
        Parameters["V00EOMsidebandRatio"] = 5.0;

        //Timeing
        Parameters["MOTHoldTime"] = 100;
        Parameters["FrequencySettleTime"] = 100;
        Parameters["MolassesDuration"] = 500;
        Parameters["FreeExpTime"] = 800;


        // v0 Light Frequency
        Parameters["v0FrequencyStartValue"] = 10.0; //9.0
        Parameters["v0FrequencyMolassesJumpValue"] = 1.9;

        // triggering delay (10V = 1 second)
        // Parameters["triggerDelay"] = 5.0;

        // v0 F=1 (dodgy code using an analogue output to control a TTL)
        Parameters["v0F1AOMStartValue"] = 5.0;
        Parameters["v0F1AOMOffValue"] = 0.0;
        Parameters["dummy"] = 0.0;


    }

    public override PatternBuilder32 GetDigitalPattern()
    {
        PatternBuilder32 p = new PatternBuilder32();
        int patternStartBeforeQ = (int)Parameters["TCLBlockStart"];
        int motLoadTime = patternStartBeforeQ + (int)Parameters["v0IntensityRampStartTime"];
        int motRampEndTime = motLoadTime + (int)Parameters["v0IntensityRampDuration"];
        int motEndTime = motRampEndTime + (int)Parameters["MOTHoldTime"];
        int molassesStartTime = motEndTime + (int)Parameters["FrequencySettleTime"];
        int molassesEndTime = molassesStartTime + (int)Parameters["MolassesDuration"];
        int imageTime = molassesEndTime + (int)Parameters["FreeExpTime"];


        MOTMasterScriptSnippet lm = new LoadMoleculeMOT(p, Parameters);  // This is how you load "preset" patterns.

        p.AddEdge("v00MOTAOM", 0, false);
        p.AddEdge("v00MOTAOM", motEndTime, true);

        p.AddEdge("v00MOTAOM", molassesStartTime, false);
        p.AddEdge("v00MOTAOM", molassesEndTime, true);

        p.AddEdge("v00MOTAOM", imageTime, false);

        //p.Pulse(0, motLoadTime - 1000, (int)Parameters["Frame0TriggerDuration"], "cameraTrigger");
        //p.Pulse(0, molassesStartTime, (int)Parameters["Frame0TriggerDuration"], "cameraTrigger");
        p.Pulse(0, imageTime, (int)Parameters["Frame0TriggerDuration"], "cameraTrigger");
        //p.Pulse(0, 12000, (int)Parameters["Frame0TriggerDuration"], "cameraTrigger");
        p.Pulse(patternStartBeforeQ, 2000, 10, "tofTrigger");

        p.AddEdge("rb2DMOTShutter", 0, true);
        p.AddEdge("rb2DMOTShutter", 5000, false);

        p.AddEdge("cafOptPumpingAOM", 0, true); // false for switch off
        p.AddEdge("cafOptPumpingShutter", 0, true); // true for switch off


        return p;
    }

    public override AnalogPatternBuilder GetAnalogPattern()
    {
        AnalogPatternBuilder p = new AnalogPatternBuilder((int)Parameters["PatternLength"]);

        int motLoadTime = (int)Parameters["v0IntensityRampStartTime"];
        int motRampEndTime = motLoadTime + (int)Parameters["v0IntensityRampDuration"];
        int motEndTime = motRampEndTime + (int)Parameters["MOTHoldTime"];
        int molassesStartTime = motEndTime + (int)Parameters["FrequencySettleTime"];
        int molassesEndTime = molassesStartTime + (int)Parameters["MolassesDuration"];
        int imageTime = molassesEndTime + (int)Parameters["FreeExpTime"];

        MOTMasterScriptSnippet lm = new LoadMoleculeMOT(p, Parameters);

        // Add Analog Channels

        p.AddChannel("v00Intensity");
        p.AddChannel("v00Frequency");
        p.AddChannel("xShimCoilCurrent");
        p.AddChannel("yShimCoilCurrent");
        p.AddChannel("zShimCoilCurrent");
        p.AddChannel("v00EOMAmp");
        p.AddChannel("v00Chirp");
        p.AddChannel("lightSwitch");
        p.AddChannel("TCoolSidebandVCO");

        p.AddAnalogValue("lightSwitch", 0, 0.0);
        //p.AddAnalogValue("lightSwitch", 1000, 2.0);

        p.AddAnalogValue("TCoolSidebandVCO", 0, 5.1); //5.1V, 63.9MHz
        //p.AddAnalogValue("TCoolSidebandVCO", 0, 4.5); //63.9MHz
        // Slowing field
        p.AddAnalogValue("slowingCoilsCurrent", 0, (double)Parameters["slowingCoilsValue"]);
        p.AddAnalogValue("slowingCoilsCurrent", (int)Parameters["slowingCoilsOffTime"], 0.0);

        // B Field
        p.AddAnalogValue("MOTCoilsCurrent", 0, (double)Parameters["MOTCoilsCurrentValue"]);
        p.AddAnalogValue("MOTCoilsCurrent", motEndTime, 0.0);
        //p.AddAnalogValue("MOTCoilsCurrent", imageTime, (double)Parameters["MOTCoilsCurrentValue"]);
        p.AddAnalogValue("MOTCoilsCurrent", imageTime + 1100, 0.0);



        // Shim Fields
        p.AddAnalogValue("xShimCoilCurrent", 0, (double)Parameters["xShimLoadCurrent"]);
        p.AddAnalogValue("yShimCoilCurrent", 0, (double)Parameters["yShimLoadCurrent"]);
        p.AddAnalogValue("zShimCoilCurrent", 0, (double)Parameters["zShimLoadCurrent"]);

        // trigger delay
        // p.AddAnalogValue("triggerDelay", 0, (double)Parameters["triggerDelay"]);

        // F=0
        //p.AddAnalogValue("v00EOMAmp", 0, 4.4); // 4.4
        p.AddAnalogValue("v00EOMAmp", 0, (double)Parameters["V00EOMsidebandRatio"]); //24/03/2023

        // v0 Intensity Ramp
        p.AddAnalogValue("v00Intensity", 0, (double)Parameters["v0IntensityRampStartValue"]);
        p.AddLinearRamp("v00Intensity", motLoadTime, (int)Parameters["v0IntensityRampDuration"], (double)Parameters["v0IntensityEndValue"]);
        p.AddAnalogValue("v00Intensity", motEndTime, (double)Parameters["v0IntensityMolassesValue"]);
        p.AddLinearRamp("v00Intensity", molassesStartTime, (int)Parameters["MolassesDuration"], (double)Parameters["v0IntensityMolassesEndValue"]);
        p.AddAnalogValue("v00Intensity", imageTime, (double)Parameters["v0IntensityRampStartValue"]);

        // v0 Frequency Ramp
        p.AddAnalogValue("v00Frequency", 0, (double)Parameters["v0FrequencyStartValue"]);
        p.AddAnalogValue("v00Frequency", motEndTime, (double)Parameters["v0FrequencyStartValue"] - (double)Parameters["v0FrequencyMolassesJumpValue"]);
        p.AddAnalogValue("v00Frequency", imageTime, (double)Parameters["v0FrequencyStartValue"]);

        //v0 chirp
        p.AddAnalogValue("v00Chirp", 0, 0.0);

        //p.AddAnalogValue("transferCoils", 0, 1.0);

        //p.AddAnalogValue("MOTCoilsCurrent", 0, (double)Parameters["Dummy"]);

        return p;
    }

}
