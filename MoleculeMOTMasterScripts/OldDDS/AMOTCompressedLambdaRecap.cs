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
        Parameters["Frame0Trigger"] = 4000; 
        Parameters["Frame0TriggerDuration"] = 1000;
        Parameters["CameraTriggerTransverseTime"] = 120;
        Parameters["FrameTriggerInterval"] = 1100;
        Parameters["waitbeforeimage"] = 1;

        //PMT
        Parameters["PMTTriggerDuration"] = 10;




        // Slowing Chirp, QuantelLaser
        Parameters["SlowingChirpStartTime"] = 550;//360; //400;// 380;
        Parameters["SlowingChirpDuration"] = 1150;////1400;//1160; //1160
        Parameters["SlowingChirpStartValue"] = 0.0;//0.0
        Parameters["SlowingChirpEndValue"] = -1.25; // -0.5 is 480MHz

        // Slowing
        Parameters["slowingAOMOnStart"] = (int)Parameters["SlowingChirpStartTime"] - 100;//160
        Parameters["slowingAOMOnDuration"] = 45000;



        Parameters["slowingAOMOffStart"] = (int)Parameters["SlowingChirpStartTime"] + (int)Parameters["SlowingChirpDuration"]; 
        Parameters["slowingAOMOffDuration"] = 40000;//40000;

        Parameters["BXShutterClose"] = (int)Parameters["slowingAOMOffStart"] - 650;


        Parameters["slowingRepumpSwitchDelay"] = 0;
        Parameters["slowingRepumpAOMOnStart"] = 0;//started from 0
        //Parameters["slowingRepumpAOMOffStart"] = 1450;
        //Parameters["slowingRepumpAOMOffStart"] = (int)Parameters["SlowingChirpStartTime"] + (int)Parameters["SlowingChirpDuration"] + (int)Parameters["slowingRepumpSwitchDelay"];
        Parameters["slowingRepumpAOMOffDuration"] = 35000;

        // Slowing field
        Parameters["slowingCoilsValue"] = 2.0; //1.05;
        Parameters["slowingCoilsOffTime"] = (int)Parameters["slowingAOMOffStart"]; // 1500;

        // B Field
        Parameters["MOTCoilsSwitchOn"] = 0;
        Parameters["MOTCoilsSwitchOff"] = 18000;
        Parameters["MOTCoilsCurrentValue"] = 1.0; // 0.65;

        // Shim fields
        Parameters["xShimLoadCurrent"] = -2.21;
        Parameters["yShimLoadCurrent"] = -2.13;
        Parameters["zShimLoadCurrent"] = -0.17;

        // v0 Light Switch
        Parameters["MOTAOMStartTime"] = 15000;
        Parameters["MOTAOMDuration"] = 500;

        // v0 Light Intensity
        Parameters["v0IntensityRampStartTime"] = 7000;
        Parameters["v0IntensityRampDuration"] = 200;
        Parameters["v0IntensityRampStartValue"] = 7.2; //5.6
        Parameters["v0IntensityEndValue"] = 8.0;//7.8
        Parameters["v0IntensityMolassesValue"] = 5.6;
        Parameters["v0IntensityRampBackTime"] = 20000;

        Parameters["V00EOMsidebandRatio"] = 5.5;
        Parameters["V00AOMSidebandAmplitude"] = 1.0;

        // Compression of MOT
        Parameters["MOTCompressoinStartTime"] = 5000;
        Parameters["MOTCompressoinDuratoin"] = 500;
        Parameters["MOTCompressoinHoldDuratoin"] = 1000;
        Parameters["MOTCoilsCompressionValue"] = 2.0;



        // v0 Light Frequency
        Parameters["v0FrequencyStartValue"] = 10.0; //9.0


        // v0 F=1 (dodgy code using an analogue output to control a TTL)
        Parameters["v0F1AOMStartValue"] = 5.0;
        Parameters["v0F1AOMOffValue"] = 0.0;
        Parameters["dummy"] = 0.0;



        //- AOM order
        
        //Lambda configuration
        Parameters["SidebandFreq1"] = 228.00 / 2.0; //+ F = 1- 
        Parameters["SidebandFreq2"] = 306.00 / 2.0; //- F = 0
        Parameters["SidebandFreq3"] = 380.00 / 2.0; //- F = 2
        Parameters["SidebandFreq4"] = 354.00 / 2.0; //+ F = 1+

        Parameters["BXAOMAttenuation"] = 5.0;
        Parameters["SlowingRepumoAttenuation"] = 6.2;

        //Sideband Amplitudes

        //21.5.25
        Parameters["SidebandAmp1"] = 10.0;
        Parameters["SidebandAmp2"] = 10.0;
        Parameters["SidebandAmp3"] = 10.0;
        Parameters["SidebandAmp4"] = 10.0;

        Parameters["SidebandImAmp1"] = 10.0;
        Parameters["SidebandImAmp2"] = 10.0;
        Parameters["SidebandImAmp3"] = 10.0;
        Parameters["SidebandImAmp4"] = 10.0;

        // 6Feb25

       // Parameters["SidebandAmp1"] = 6.7;
        //Parameters["SidebandAmp2"] = 7.7;
        //Parameters["SidebandAmp3"] = 8.0;
        //Parameters["SidebandAmp4"] = 8.0;

        //Parameters["SidebandImAmp1"] = 6.7;
        //Parameters["SidebandImAmp2"] = 7.7;
        //Parameters["SidebandImAmp3"] = 8.0;
        //Parameters["SidebandImAmp4"] = 8.0;

        //10% saturation, 6Feb25

        Parameters["SidebandAmpRampEnd1"] = 3.7;
        Parameters["SidebandAmpRampEnd2"] = 3.7;
        Parameters["SidebandAmpRampEnd3"] = 4.0;
        Parameters["SidebandAmpRampEnd4"] = 3.7;

        Parameters["SidebandLambda1"] = 6.7; //5.6mw
        Parameters["SidebandLambda2"] = 0.0; 
        Parameters["SidebandLambda3"] = 0.0; 
        Parameters["SidebandLambda4"] = 3.85; //2.9mw


        //VCO Calibration
        //VCO frequency in MHz = offset + vol * gradient
        Parameters["POS300OffsetFreq"] = 129.2;
        Parameters["POS300Gradient"] = 10.6; 
        Parameters["POS150OffsetFreq"] = 62.6;
        Parameters["POS150Gradient"] = 7.68;

        Parameters["FrequencySettleTime"] = 200;
        Parameters["LambdaCoolingDuration"] = 1;
        Parameters["FreeExpTime"] = 1;

        



    }

    public override PatternBuilder32 GetDigitalPattern()
    {
        PatternBuilder32 p = new PatternBuilder32();
        int patternStartBeforeQ = (int)Parameters["TCLBlockStart"];
        int motCompressionStartTime = patternStartBeforeQ + (int)Parameters["MOTCompressoinStartTime"];
        int motCompressoinEndTime = motCompressionStartTime + (int)Parameters["MOTCompressoinDuratoin"];
        int motEndTime = motCompressoinEndTime + (int)Parameters["MOTCompressoinHoldDuratoin"];
        int lambdaCoolingStart = motEndTime + (int)Parameters["FrequencySettleTime"];
        int lambdaCoolingEnd = lambdaCoolingStart + (int)Parameters["LambdaCoolingDuration"];
        int imageTime = lambdaCoolingEnd + (int)Parameters["FreeExpTime"];
        int BXShutterClose = patternStartBeforeQ + (int)Parameters["BXShutterClose"];
        int compressedmotimgtime = (int)Parameters["v0IntensityRampStartTime"] -500;



        MOTMasterScriptSnippet lm = new LoadMoleculeMOTNoSlowingEdge(p, Parameters);  // This is how you load "preset" patterns.


        p.Pulse(patternStartBeforeQ, (int)Parameters["SlowingChirpStartTime"], (2 * (int)Parameters["SlowingChirpDuration"]) + 20000, "bXLockBlock"); // Want it to be blocked for whole time that bX laser is moved
        //p.Pulse(patternStartBeforeQ, 100, 100, "bXSlowingAOM"); //first pulse to slowing AOM
        p.Pulse(patternStartBeforeQ, (int)Parameters["SlowingChirpStartTime"] - 100, (int)Parameters["SlowingChirpDuration"] + 100, "bXSlowingAOM"); //first pulse to slowing AOM
        p.Pulse(patternStartBeforeQ, (int)Parameters["slowingRepumpAOMOnStart"], (int)Parameters["SlowingChirpStartTime"] + (int)Parameters["SlowingChirpDuration"] - (int)Parameters["slowingRepumpAOMOnStart"], "v10SlowingAOM"); //first pulse to slowing repump AOM

        p.Pulse(patternStartBeforeQ, (int)Parameters["Frame0Trigger"], (int)Parameters["Frame0TriggerDuration"], "cameraTrigger");
        //p.Pulse(0, motCompressoinEndTime-500, (int)Parameters["Frame0TriggerDuration"], "cameraTrigger"); //camera trigger for first frame
        //p.Pulse(0, imageTime, (int)Parameters["Frame0TriggerDuration"], "cameraTrigger"); //camera trigger for first frame
        //p.Pulse(patternStartBeforeQ, 20000, (int)Parameters["Frame0TriggerDuration"], "cameraTrigger"); //background

        p.Pulse(patternStartBeforeQ, 2000, 10, "tofTrigger");

        p.AddEdge("rb2DMOTShutter", 0, true);
        p.AddEdge("rb2DMOTShutter", 5000, false);

        p.AddEdge("cafOptPumpingAOM", 0, true); // false for switch off
        p.AddEdge("cafOptPumpingShutter", 0, true); // true for switch off

        p.AddEdge("v0rfswitch1", 0, false);
        p.AddEdge("v0rfswitch2", 0, false);
        p.AddEdge("v0rfswitch3", 0, false);
        p.AddEdge("v0rfswitch4", 0, false);

        p.AddEdge("v0ddsSwitchA", 0, false);
        p.AddEdge("v0ddsSwitchB", 0, false);


        p.AddEdge("v0rfswitch1", motEndTime, true);
        p.AddEdge("v0rfswitch2", motEndTime, true);
        p.AddEdge("v0rfswitch3", motEndTime, true);
        p.AddEdge("v0rfswitch4", motEndTime, true);

        // switch on lambda sideband
        p.AddEdge("v0rfswitch1", lambdaCoolingStart, false);
        p.AddEdge("v0rfswitch4", lambdaCoolingStart, false);
        p.AddEdge("v0ddsSwitchA", lambdaCoolingStart, true);
        p.AddEdge("v0ddsSwitchB", lambdaCoolingStart, true);

        // switch off lambda sideband
        p.AddEdge("v0rfswitch1", lambdaCoolingEnd, true);
        p.AddEdge("v0rfswitch4", lambdaCoolingEnd, true);
        p.AddEdge("v0ddsSwitchA", lambdaCoolingEnd, false);
        p.AddEdge("v0ddsSwitchB", lambdaCoolingEnd, false);

        p.AddEdge("v0rfswitch1", imageTime, false);
        p.AddEdge("v0rfswitch2", imageTime, false);
        p.AddEdge("v0rfswitch3", imageTime, false);
        p.AddEdge("v0rfswitch4", imageTime, false);

        // QCL shutter
        //p.Pulse(patternStartBeforeQ, (int)Parameters["Frame0Trigger"]+ (int)Parameters["Frame0TriggerDuration"], 10000, "QCLShutter");
        p.Pulse(0, lambdaCoolingStart - 200, (int)Parameters["LambdaCoolingDuration"], "QCLShutter");

        p.AddEdge("TweezerChamberRbMOTAOMs", 1000, true);
        p.AddEdge("TweezerChamberRbMOTAOMs", 10000, false);



        p.AddEdge("bXSlowingShutter", 0, true);
        p.AddEdge("bXSlowingShutter", BXShutterClose, false);
        p.AddEdge("bXSlowingShutter", 26000, true);



        return p;
    }

    public override AnalogPatternBuilder GetAnalogPattern()
    {
        AnalogPatternBuilder p = new AnalogPatternBuilder((int)Parameters["PatternLength"]);

        int motCompressionStartTime = (int)Parameters["MOTCompressoinStartTime"];
        int motCompressoinEndTime = motCompressionStartTime + (int)Parameters["MOTCompressoinDuratoin"];
        int motEndTime = motCompressoinEndTime + (int)Parameters["MOTCompressoinHoldDuratoin"];
        int lambdaCoolingStart = motEndTime + (int)Parameters["FrequencySettleTime"];
        int lambdaCoolingEnd = lambdaCoolingStart + (int)Parameters["LambdaCoolingDuration"];
        int imageTime = lambdaCoolingEnd + (int)Parameters["FreeExpTime"];

        MOTMasterScriptSnippet lm = new LoadMoleculeMOTNoSlowingEdge(p, Parameters);

        // Add Analog Channels
        
        p.AddChannel("v00Intensity");
        p.AddChannel("v00Frequency");
        p.AddChannel("xShimCoilCurrent");
        p.AddChannel("yShimCoilCurrent");
        p.AddChannel("zShimCoilCurrent");
        p.AddChannel("BXAttenuation");
        p.AddChannel("v00EOMAmp");
        p.AddChannel("v00Chirp");
        p.AddChannel("lightSwitch");
        p.AddChannel("TCoolSidebandVCO");
        p.AddChannel("v0AOMSidebandAmp");
        p.AddChannel("Rf1Freq");
        p.AddChannel("Rf2Freq");
        p.AddChannel("Rf3Freq");
        p.AddChannel("Rf4Freq");
        p.AddChannel("Rf1Amp");
        p.AddChannel("Rf2Amp");
        p.AddChannel("Rf3Amp");
        p.AddChannel("Rf4Amp");
        p.AddChannel("SlowingRepumpAttenuation");

        //Switch BX AOM via analog output Mar 05 2024
        //p.AddAnalogValue("BXAttenuation", 0, 0.0);
        p.AddAnalogValue("BXAttenuation", (int)Parameters["slowingAOMOnStart"], (double)Parameters["BXAOMAttenuation"]);
        p.AddAnalogValue("BXAttenuation", (int)Parameters["slowingAOMOffStart"], 0.0);
        //p.AddAnalogValue("BXAttenuation", (int)Parameters["PatternLength"] - 10000, (double)Parameters["BXAOMAttenuation"]);

        p.AddAnalogValue("lightSwitch", 0, 0.0);
        //p.AddAnalogValue("lightSwitch", 1000, 2.0);

        p.AddAnalogValue("TCoolSidebandVCO", 0, 5.15); //5.15V, 63.5MHz
        p.AddAnalogValue("SlowingRepumpAttenuation", 0, (double)Parameters["SlowingRepumoAttenuation"]);
        p.AddAnalogValue("v0AOMSidebandAmp", 0, (double)Parameters["V00AOMSidebandAmplitude"]);

        // Slowing field
        p.AddAnalogValue("slowingCoilsCurrent", 0, (double)Parameters["slowingCoilsValue"]);
        p.AddAnalogValue("slowingCoilsCurrent", (int)Parameters["SlowingChirpStartTime"] + (int)Parameters["SlowingChirpDuration"], 0.0);

        // B Field
        p.AddAnalogValue("MOTCoilsCurrent", 0, (double)Parameters["MOTCoilsCurrentValue"]);
        p.AddLinearRamp("MOTCoilsCurrent", motCompressionStartTime, (int)Parameters["MOTCompressoinDuratoin"], (double)Parameters["MOTCoilsCompressionValue"]);
        p.AddAnalogValue("MOTCoilsCurrent", motEndTime, -0.2);
        p.AddAnalogValue("MOTCoilsCurrent", imageTime-175, 1.0); // -175 added to account for time it takes for B field current to respond to change in control voltage from a negative set point
        p.AddAnalogValue("MOTCoilsCurrent", imageTime + 1000, 0.0);


        // MOT Shim Fields
        p.AddAnalogValue("xShimCoilCurrent", 0, (double)Parameters["xShimLoadCurrent"]);
        p.AddAnalogValue("yShimCoilCurrent", 0, (double)Parameters["yShimLoadCurrent"]);
        p.AddAnalogValue("zShimCoilCurrent", 0, (double)Parameters["zShimLoadCurrent"]);
        p.AddAnalogValue("v00EOMAmp", 0, (double)Parameters["V00EOMsidebandRatio"]); //24/03/2023

        // Recap MOT Shim Fields
        p.AddAnalogValue("xShimCoilCurrent", imageTime, (double)Parameters["xShimLoadCurrent"]);
        p.AddAnalogValue("yShimCoilCurrent", imageTime, (double)Parameters["yShimLoadCurrent"]);
        p.AddAnalogValue("zShimCoilCurrent", imageTime, (double)Parameters["zShimLoadCurrent"]);

        // v0 Intensity Ramp
        p.AddAnalogValue("v00Intensity", 0, (double)Parameters["v0IntensityRampStartValue"]);

        // v0 Frequency Ramp
        p.AddAnalogValue("v00Frequency", 0, (double)Parameters["v0FrequencyStartValue"]);

        //Sideband VCOs
        p.AddAnalogValue("Rf1Freq", 0, ((double)Parameters["SidebandFreq1"] - (double)Parameters["POS150OffsetFreq"]) / (double)Parameters["POS150Gradient"]);
        p.AddAnalogValue("Rf2Freq", 0, ((double)Parameters["SidebandFreq2"] - (double)Parameters["POS300OffsetFreq"]) / (double)Parameters["POS300Gradient"]);
        p.AddAnalogValue("Rf3Freq", 0, ((double)Parameters["SidebandFreq3"] - (double)Parameters["POS300OffsetFreq"]) / (double)Parameters["POS300Gradient"]);
        p.AddAnalogValue("Rf4Freq", 0, ((double)Parameters["SidebandFreq4"] - (double)Parameters["POS300OffsetFreq"]) / (double)Parameters["POS300Gradient"]);

        p.AddAnalogValue("Rf1Amp", 0, (double)Parameters["SidebandAmp1"]);
        p.AddAnalogValue("Rf2Amp", 0, (double)Parameters["SidebandAmp2"]);
        p.AddAnalogValue("Rf3Amp", 0, (double)Parameters["SidebandAmp3"]);
        p.AddAnalogValue("Rf4Amp", 0, (double)Parameters["SidebandAmp4"]);




        p.AddLinearRamp("Rf1Amp", (int)Parameters["v0IntensityRampStartTime"], (int)Parameters["v0IntensityRampDuration"], (double)Parameters["SidebandAmpRampEnd1"]);
        p.AddLinearRamp("Rf2Amp", (int)Parameters["v0IntensityRampStartTime"], (int)Parameters["v0IntensityRampDuration"], (double)Parameters["SidebandAmpRampEnd2"]);
        p.AddLinearRamp("Rf3Amp", (int)Parameters["v0IntensityRampStartTime"], (int)Parameters["v0IntensityRampDuration"], (double)Parameters["SidebandAmpRampEnd3"]);
        p.AddLinearRamp("Rf4Amp", (int)Parameters["v0IntensityRampStartTime"], (int)Parameters["v0IntensityRampDuration"], (double)Parameters["SidebandAmpRampEnd4"]);

        // Lambda cooling sidebands
       
        p.AddAnalogValue("Rf1Amp", lambdaCoolingStart, (double)Parameters["SidebandLambda1"]);
        p.AddAnalogValue("Rf2Amp", lambdaCoolingStart, (double)Parameters["SidebandLambda2"]);
        p.AddAnalogValue("Rf3Amp", lambdaCoolingStart, (double)Parameters["SidebandLambda3"]);
        p.AddAnalogValue("Rf4Amp", lambdaCoolingStart, (double)Parameters["SidebandLambda4"]);


  
        p.AddAnalogValue("Rf1Amp", imageTime, (double)Parameters["SidebandImAmp1"]);
        p.AddAnalogValue("Rf2Amp", imageTime, (double)Parameters["SidebandImAmp2"]);
        p.AddAnalogValue("Rf3Amp", imageTime, (double)Parameters["SidebandImAmp3"]);
        p.AddAnalogValue("Rf4Amp", imageTime, (double)Parameters["SidebandImAmp4"]);

        //to here
        //v0 chirp
        p.AddAnalogValue("v00Chirp", 0, 0.0);
        

        return p;
    }

}
