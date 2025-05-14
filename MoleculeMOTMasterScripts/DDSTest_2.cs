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
        Parameters["PatternLength"] = 100000;
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

      


        // Slowing Chirp
        Parameters["SlowingChirpStartTime"] = 250;//360; //400;// 380;
        Parameters["SlowingChirpDuration"] = 1200;////1400;//1160; //1160
        Parameters["SlowingChirpStartValue"] = 0.0;//0.0
        Parameters["SlowingChirpEndValue"] = -0.30; // -0.5 is 480MHz

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
        Parameters["slowingCoilsValue"] = 1.0; //1.05;
        Parameters["slowingCoilsOffTime"] = (int)Parameters["slowingAOMOffStart"]; // 1500;

        // B Field
        Parameters["MOTCoilsSwitchOn"] = 0;
        Parameters["MOTCoilsSwitchOff"] = 20000;
        Parameters["MOTCoilsCurrentValue"] = 1.0; // 0.65;

        // Shim fields
        Parameters["xShimLoadCurrent"] = -2.21;
        Parameters["yShimLoadCurrent"] = -2.13;
        Parameters["zShimLoadCurrent"] = -0.17;


        Parameters["xShimBlueMOTCurrent"] = -2.21;// 10.0;// -1.35;
        Parameters["yShimBlueMOTCurrent"] = -2.13;// -10.0;// -1.92;
        Parameters["zShimBlueMOTCurrent"] = -0.17;// 0.00;// -0.22;


        // v0 Light Switch
        Parameters["MOTAOMStartTime"] = 15000;
        Parameters["MOTAOMDuration"] = 500;

        // v0 Light Intensity
        Parameters["v0IntensityRampStartTime"] = 5000;
        Parameters["v0IntensityRampDuration"] = 500;
        Parameters["v0IntensityRampStartValue"] = 7.2; //5.6
        Parameters["v0IntensityEndValue"] = 8.0;//7.8
        Parameters["v0IntensityMolassesValue"] = 5.6;
        Parameters["v0IntensityRampBackTime"] = 20000;

        Parameters["V00EOMsidebandRatio"] = 5.5;
        Parameters["V00AOMSidebandAmplitude"] = 1.0;

        // Compression of MOT
        Parameters["MOTCompressoinStartTime"] = 4000;
        Parameters["MOTCompressoinDuratoin"] = 1000;
        Parameters["MOTCompressoinHoldDuratoin"] = 1000;
        Parameters["MOTCoilsCompressionValue"] = 1.75; 

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

        //Conveyor belt frequency
        Parameters["SidebandFreqCVB1"] = 193.64 / 2.0; //+ F = 1Delta 97.50MHz 195.00MHz
        Parameters["SidebandFreqCVB2"] = 306.00 / 2.0; //- F = 0
        Parameters["SidebandFreqCVB3"] = 340.67 / 2.0; //- F = 2sig- 170.78MHz 341.56MHz
        Parameters["SidebandFreqCVB4"] = 342.42 / 2.0; //+ F = 2sig+ 170.48MHz 340.96MHz

        // DDS parameters

        Parameters["DDSFreq0"] = 171.40;
        Parameters["DDSFreq1"] = 98.00;
        Parameters["DDSFreq2"] = 171.80;
        Parameters["DDSFreq3"] = 171.58;

        Parameters["DDSLambdFreq0"] = 171.40;
        Parameters["DDSLambdFreq1"] = 98.00;
        Parameters["DDSLambdFreq2"] = 171.80;
        Parameters["DDSLambdFreq3"] = 171.58;

        Parameters["DDSAmp0"] = 1.0;
        Parameters["DDSAmp1"] = 1.0;
        Parameters["DDSAmp2"] = 1.0;
        Parameters["DDSAmp3"] = 1.0;

        Parameters["DDSMOTRampEndAmp0"] = 0.1;
        Parameters["DDSMOTRampEndAmp1"] = 0.1;
        Parameters["DDSMOTRampEndAmp2"] = 0.1;
        Parameters["DDSMOTRampEndAmp3"] = 0.1;


        Parameters["BXAOMAttenuation"] = 3.0;
        Parameters["SlowingRepumoAttenuation"] = 6.2;

        //Sideband Amplitudes


        //Parameters["SidebandAmp1"] = 4.0;
        //Parameters["SidebandAmp2"] = 4.4;
        //Parameters["SidebandAmp3"] = 7.0;
        //Parameters["SidebandAmp4"] = 6.0;

        //Parameters["SidebandAmp1"] = 4.5;
        //Parameters["SidebandAmp2"] = 8.0;
        //Parameters["SidebandAmp3"] = 7.5;
        //Parameters["SidebandAmp4"] = 7.5;


        Parameters["SidebandAmp1"] = 8.0;
        Parameters["SidebandAmp2"] = 8.0;
        Parameters["SidebandAmp3"] = 8.0;
        Parameters["SidebandAmp4"] = 8.0;

        Parameters["SidebandImAmp1"] = 8.0;
        Parameters["SidebandImAmp2"] = 8.0;
        Parameters["SidebandImAmp3"] = 8.0;
        Parameters["SidebandImAmp4"] = 8.0;

        Parameters["SidebandAmpBlue1"] = 6.0;
        Parameters["SidebandAmpBlue2"] = 8.0;
        Parameters["SidebandAmpBlue3"] = 10.0;
        Parameters["SidebandAmpBlue4"] = 4.4; //7.5



        //10% saturation, Nov 14, 2024

        //Parameters["SidebandAmpRampEnd1"] = 3.3;
        //Parameters["SidebandAmpRampEnd2"] = 3.4;
        //Parameters["SidebandAmpRampEnd3"] = 3.5;
        //Parameters["SidebandAmpRampEnd4"] = 3.5;

        //Parameters["SidebandAmpRampEnd1"] = 6.0;
        //Parameters["SidebandAmpRampEnd2"] = 4.5;
        //Parameters["SidebandAmpRampEnd3"] = 3.8;
        //Parameters["SidebandAmpRampEnd4"] = 3.6;

        // Dec 13th 2024

        Parameters["SidebandAmpRampEnd1"] = 3.5;
        Parameters["SidebandAmpRampEnd2"] = 4.0;
        Parameters["SidebandAmpRampEnd3"] = 5.0;//4.2;
        Parameters["SidebandAmpRampEnd4"] = 4.0;//4.2;

        //VCO Calibration
        //VCO frequency in MHz = offset + vol * gradient
        Parameters["POS300OffsetFreq"] = 129.2;
        Parameters["POS300Gradient"] = 10.6; 
        Parameters["POS150OffsetFreq"] = 62.6;
        Parameters["POS150Gradient"] = 7.68;

        Parameters["BlueMOTField"] = 0.8;

        Parameters["FrequencySettleTime"] = 50;
        Parameters["LambdaCoolingDuration"] = 500;
        Parameters["BlueMOTRampDuration"] = 2000;
        Parameters["BlueMOTDuration"] = 2000;
        Parameters["FreeExpTime"] = 200;
        Parameters["ImageInBlueMOT"] = 0;


    }

    private void prePatternSetup()
    {

        NeanderthalDDSController.Controller DDSCtrl = (NeanderthalDDSController.Controller)(Activator.GetObject(typeof(NeanderthalDDSController.Controller), "tcp://localhost:1818/controller.rem"));
        // Add number of command equal to the number of total triggers to DDS
        // Parameter is a list of length 16

        // Initial MOT
        DDSCtrl.addPatternToBufferSingle(new List<double> { 
            (double)Parameters["DDSFreq0"], (double)Parameters["DDSAmp0"], 0.0, 0.0,
            (double)Parameters["DDSFreq1"], (double)Parameters["DDSAmp1"], 0.0, 0.0,
            (double)Parameters["DDSFreq2"], (double)Parameters["DDSAmp2"], 0.0, 0.0,
            (double)Parameters["DDSFreq3"], (double)Parameters["DDSAmp3"], 0.0, 0.0 });


        // motEndTime
        DDSCtrl.addPatternToBufferSingle(new List<double> {
            (double)Parameters["DDSLambdFreq0"], 0.0, 0.0, 0.0,
            (double)Parameters["DDSLambdFreq1"], 0.0, 0.0, 0.0,
            (double)Parameters["DDSLambdFreq2"], 0.0, 0.0, 0.0,
            (double)Parameters["DDSLambdFreq3"], 0.0, 0.0, 0.0 });

        // lambdaCoolingStart
        DDSCtrl.addPatternToBufferSingle(new List<double> {
            (double)Parameters["DDSLambdFreq0"], (double)Parameters["DDSAmp0"], 0.0, 0.0,
            (double)Parameters["DDSLambdFreq1"], (double)Parameters["DDSAmp1"], 0.0, 0.0,
            (double)Parameters["DDSLambdFreq2"], (double)Parameters["DDSAmp2"], 0.0, 0.0,
            (double)Parameters["DDSLambdFreq3"], (double)Parameters["DDSAmp3"], 0.0, 0.0 });

        // lambdaCoolingEnd
        DDSCtrl.addPatternToBufferSingle(new List<double> {
            (double)Parameters["DDSFreq0"], 0.0, 0.0, 0.0,
            (double)Parameters["DDSFreq1"], 0.0, 0.0, 0.0,
            (double)Parameters["DDSFreq2"], 0.0, 0.0, 0.0,
            (double)Parameters["DDSFreq3"], 0.0, 0.0, 0.0 });

        DDSCtrl.writePatternToCard();

    }

    public override PatternBuilder32 GetDigitalPattern()
    {
        prePatternSetup();
        PatternBuilder32 p = new PatternBuilder32();
        int patternStartBeforeQ = (int)Parameters["TCLBlockStart"];
        int motCompressionStartTime = patternStartBeforeQ + (int)Parameters["MOTCompressoinStartTime"];
        int motCompressoinEndTime = motCompressionStartTime + (int)Parameters["MOTCompressoinDuratoin"];
        int motEndTime = motCompressoinEndTime + (int)Parameters["MOTCompressoinHoldDuratoin"];
        int lambdaCoolingStart = motEndTime + (int)Parameters["FrequencySettleTime"];
        int lambdaCoolingEnd = lambdaCoolingStart + (int)Parameters["LambdaCoolingDuration"];
        int blueMOTRampEnd = lambdaCoolingEnd + (int)Parameters["BlueMOTRampDuration"];
        int blueMOTEnd = blueMOTRampEnd + (int)Parameters["BlueMOTDuration"];
        int imageTime = blueMOTEnd + (int)Parameters["FreeExpTime"];

        int BXShutterClose = patternStartBeforeQ + (int)Parameters["BXShutterClose"];

        MOTMasterScriptSnippet lm = new LoadMoleculeMOT(p, Parameters);  // This is how you load "preset" patterns.


        p.Pulse(patternStartBeforeQ, (int)Parameters["SlowingChirpStartTime"], (2 * (int)Parameters["SlowingChirpDuration"]) + 20000, "bXLockBlock"); // Want it to be blocked for whole time that bX laser is moved
        //p.Pulse(patternStartBeforeQ, 100, 100, "bXSlowingAOM"); //first pulse to slowing AOM
        p.Pulse(patternStartBeforeQ, (int)Parameters["SlowingChirpStartTime"] - 100, (int)Parameters["SlowingChirpDuration"] + 100, "bXSlowingAOM"); //first pulse to slowing AOM
        p.Pulse(patternStartBeforeQ, (int)Parameters["slowingRepumpAOMOnStart"], (int)Parameters["SlowingChirpStartTime"] + (int)Parameters["SlowingChirpDuration"] - (int)Parameters["slowingRepumpAOMOnStart"], "v10SlowingAOM"); //first pulse to slowing repump AOM

        //p.Pulse(patternStartBeforeQ, 4000, (int)Parameters["Frame0TriggerDuration"], "cameraTrigger"); //camera trigger for first frame
        p.Pulse(0, blueMOTEnd - (int)Parameters["Frame0TriggerDuration"], (int)Parameters["Frame0TriggerDuration"], "cameraTrigger");
        // Pulse(delay time, start time, duration, channel)
        //p.Pulse(0, lambdaCoolingEnd + (int)Parameters["ImageInBlueMOT"], (int)Parameters["Frame0TriggerDuration"], "cameraTrigger");
        //p.Pulse(patternStartBeforeQ, 4000, (int)Parameters["Frame0TriggerDuration"], "cameraTrigger"); //camera trigger for first frame
        //p.Pulse(0, imageTime, (int)Parameters["Frame0TriggerDuration"], "cameraTrigger");

        // DDS trigger
        p.Pulse(patternStartBeforeQ, 0, 10, "DDSTrigger"); // Always send a trigger at time 0
        p.Pulse(patternStartBeforeQ, 1000, 10, "DDSTrigger");
        p.Pulse(patternStartBeforeQ, 2000, 10, "DDSTrigger");
        p.Pulse(patternStartBeforeQ, 3000, 10, "DDSTrigger");
        

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

        p.AddEdge("v0ddsSwitchC", 0, false);
        p.AddEdge("v0ddsSwitchD", 0, false);
        
        p.AddEdge("v0rfswitch1", motEndTime, true);
        p.AddEdge("v0rfswitch2", motEndTime, true);
        p.AddEdge("v0rfswitch3", motEndTime, true);
        p.AddEdge("v0rfswitch4", motEndTime, true);
        
        // switch on lambda sideband
        p.AddEdge("v0rfswitch1", lambdaCoolingStart, false);
        p.AddEdge("v0rfswitch4", lambdaCoolingStart, false);
        p.AddEdge("v0ddsSwitchA", lambdaCoolingStart, true);
        p.AddEdge("v0ddsSwitchB", lambdaCoolingStart, true);

        // switch on blue mot sideband
        
        p.AddEdge("v0ddsSwitchC", lambdaCoolingEnd, true);
        p.AddEdge("v0ddsSwitchD", lambdaCoolingEnd, true);
        p.AddEdge("v0rfswitch3", lambdaCoolingEnd, false);

        // switch off blue mot
        
        p.AddEdge("v0ddsSwitchA", blueMOTEnd, false);
        p.AddEdge("v0ddsSwitchB", blueMOTEnd, false);
        p.AddEdge("v0ddsSwitchC", blueMOTEnd, false);
        p.AddEdge("v0ddsSwitchD", blueMOTEnd, false);

        p.AddEdge("v0rfswitch1", blueMOTEnd, true);
        p.AddEdge("v0rfswitch3", blueMOTEnd, true);
        p.AddEdge("v0rfswitch4", blueMOTEnd, true);

        p.AddEdge("v0rfswitch1", imageTime, false);
        p.AddEdge("v0rfswitch2", imageTime, false);
        p.AddEdge("v0rfswitch3", imageTime, false);
        p.AddEdge("v0rfswitch4", imageTime, false);
        
        
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
        int blueMOTRampEnd = lambdaCoolingEnd + (int)Parameters["BlueMOTRampDuration"];
        int blueMOTEnd = blueMOTRampEnd + (int)Parameters["BlueMOTDuration"];
        int imageTime = blueMOTEnd + (int)Parameters["FreeExpTime"];

        MOTMasterScriptSnippet lm = new LoadMoleculeMOT(p, Parameters);

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
        p.AddAnalogValue("BXAttenuation", 0, 0.0);
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
        p.AddAnalogValue("MOTCoilsCurrent", motEndTime, -0.0);
        p.AddLinearRamp("MOTCoilsCurrent", lambdaCoolingEnd, (int)Parameters["BlueMOTRampDuration"], (double)Parameters["BlueMOTField"]);
        p.AddAnalogValue("MOTCoilsCurrent", blueMOTEnd, -0.0);
        p.AddAnalogValue("MOTCoilsCurrent", imageTime-50, (double)Parameters["MOTCoilsCurrentValue"]);
        p.AddAnalogValue("MOTCoilsCurrent", imageTime + 1000, -0.00);


        // Shim Fields
        p.AddAnalogValue("xShimCoilCurrent", 0, (double)Parameters["xShimLoadCurrent"]);
        //p.AddAnalogValue("xShimCoilCurrent", lambdaCoolingEnd, (double)Parameters["xShimBlueMOTCurrent"]);
        p.AddLinearRamp("xShimCoilCurrent", lambdaCoolingEnd, (int)Parameters["BlueMOTRampDuration"], (double)Parameters["xShimBlueMOTCurrent"]);
        p.AddAnalogValue("xShimCoilCurrent", blueMOTEnd, (double)Parameters["xShimLoadCurrent"]);
        p.AddAnalogValue("yShimCoilCurrent", 0, (double)Parameters["yShimLoadCurrent"]);
        //p.AddAnalogValue("yShimCoilCurrent", lambdaCoolingEnd, (double)Parameters["yShimBlueMOTCurrent"]);
        p.AddLinearRamp("yShimCoilCurrent", lambdaCoolingEnd, (int)Parameters["BlueMOTRampDuration"], (double)Parameters["yShimBlueMOTCurrent"]);
        p.AddAnalogValue("yShimCoilCurrent", blueMOTEnd, (double)Parameters["yShimLoadCurrent"]);
        p.AddAnalogValue("zShimCoilCurrent", 0, (double)Parameters["zShimLoadCurrent"]);
        //p.AddAnalogValue("zShimCoilCurrent", lambdaCoolingEnd, (double)Parameters["zShimBlueMOTCurrent"]);
        p.AddLinearRamp("zShimCoilCurrent", lambdaCoolingEnd, (int)Parameters["BlueMOTRampDuration"], (double)Parameters["zShimBlueMOTCurrent"]);
        p.AddAnalogValue("zShimCoilCurrent", blueMOTEnd, (double)Parameters["zShimLoadCurrent"]);

        p.AddAnalogValue("xShimCoilCurrent", imageTime-50, (double)Parameters["xShimLoadCurrent"]);
        p.AddAnalogValue("yShimCoilCurrent", imageTime-50, (double)Parameters["yShimLoadCurrent"]);
        p.AddAnalogValue("zShimCoilCurrent", imageTime-50, (double)Parameters["zShimLoadCurrent"]);


        p.AddAnalogValue("v00EOMAmp", 0, (double)Parameters["V00EOMsidebandRatio"]); //24/03/2023

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

        
        p.AddAnalogValue("Rf1Amp", lambdaCoolingEnd, (double)Parameters["SidebandAmpBlue1"]);
        p.AddAnalogValue("Rf2Amp", lambdaCoolingEnd, (double)Parameters["SidebandAmpBlue2"]);
        p.AddAnalogValue("Rf3Amp", lambdaCoolingEnd, (double)Parameters["SidebandAmpBlue3"]);
        p.AddAnalogValue("Rf4Amp", lambdaCoolingEnd, (double)Parameters["SidebandAmpBlue4"]);
        

        p.AddLinearRamp("Rf1Amp", (int)Parameters["v0IntensityRampStartTime"], (int)Parameters["v0IntensityRampDuration"], (double)Parameters["SidebandAmpRampEnd1"]);
        p.AddLinearRamp("Rf2Amp", (int)Parameters["v0IntensityRampStartTime"], (int)Parameters["v0IntensityRampDuration"], (double)Parameters["SidebandAmpRampEnd2"]);
        p.AddLinearRamp("Rf3Amp", (int)Parameters["v0IntensityRampStartTime"], (int)Parameters["v0IntensityRampDuration"], (double)Parameters["SidebandAmpRampEnd3"]);
        p.AddLinearRamp("Rf4Amp", (int)Parameters["v0IntensityRampStartTime"], (int)Parameters["v0IntensityRampDuration"], (double)Parameters["SidebandAmpRampEnd4"]);

        
        p.AddAnalogValue("Rf1Amp", imageTime, (double)Parameters["SidebandImAmp1"]);
        p.AddAnalogValue("Rf2Amp", imageTime, (double)Parameters["SidebandImAmp2"]);
        p.AddAnalogValue("Rf3Amp", imageTime, (double)Parameters["SidebandImAmp3"]);
        p.AddAnalogValue("Rf4Amp", imageTime, (double)Parameters["SidebandImAmp4"]);

        p.AddAnalogValue("Rf1Freq", imageTime, ((double)Parameters["SidebandFreq1"] - (double)Parameters["POS150OffsetFreq"]) / (double)Parameters["POS150Gradient"]);
        p.AddAnalogValue("Rf2Freq", imageTime, ((double)Parameters["SidebandFreq2"] - (double)Parameters["POS300OffsetFreq"]) / (double)Parameters["POS300Gradient"]);
        p.AddAnalogValue("Rf3Freq", imageTime, ((double)Parameters["SidebandFreq3"] - (double)Parameters["POS300OffsetFreq"]) / (double)Parameters["POS300Gradient"]);
        p.AddAnalogValue("Rf4Freq", imageTime, ((double)Parameters["SidebandFreq4"] - (double)Parameters["POS300OffsetFreq"]) / (double)Parameters["POS300Gradient"]);
        


        //v0 chirp
        p.AddAnalogValue("v00Chirp", 0, 0.0);
        

        return p;
    }

}
