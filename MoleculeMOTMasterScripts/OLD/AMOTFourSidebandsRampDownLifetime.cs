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
        Parameters["TCLBlockStart"] = 4000; // This is a time before the Q switch
        Parameters["TCLBlockDuration"] = 15000;
        Parameters["FlashToQ"] = 16; // This is a time before the Q switch
        Parameters["QSwitchPulseDuration"] = 10;
        Parameters["FlashPulseDuration"] = 10;
        Parameters["HeliumShutterToQ"] = 300;
        Parameters["HeliumShutterDuration"] = 2000;

        // Camera
        Parameters["Frame0Trigger"] = 5000;
        Parameters["Frame0TriggerDuration"] = 1000;
        Parameters["CameraTriggerTransverseTime"] = 120;
        Parameters["FrameTriggerInterval"] = 1100;
        Parameters["waitbeforeimage"] = 1;

        //PMT
        Parameters["PMTTriggerDuration"] = 10;

        // Slowing Chirp
        Parameters["SlowingChirpStartTime"] = 200;//360; //400;// 380;
        Parameters["SlowingChirpDuration"] = 1200;////1400;//1160; //1160
        Parameters["SlowingChirpStartValue"] = 0.0;//0.0
        Parameters["SlowingChirpEndValue"] = -0.3;//-1.25; //-1.25 //225MHz/V 120m/s/V

        // Slowing
        Parameters["slowingAOMOnStart"] = (int)Parameters["SlowingChirpStartTime"] - 100;//160
        Parameters["slowingAOMOnDuration"] = 45000;



        Parameters["slowingAOMOffStart"] = (int)Parameters["SlowingChirpStartTime"] + (int)Parameters["SlowingChirpDuration"];
        Parameters["slowingAOMOffDuration"] = 40000;//40000;



        Parameters["slowingRepumpAOMOnStart"] = 0;//started from 0
        Parameters["slowingRepumpAOMOffStart"] = (int)Parameters["SlowingChirpStartTime"] + (int)Parameters["SlowingChirpDuration"];
        Parameters["slowingRepumpAOMOffDuration"] = 35000;

        // Slowing field
        Parameters["slowingCoilsValue"] = 0.4; //1.05;
        Parameters["slowingCoilsOffTime"] = (int)Parameters["slowingAOMOffStart"]; // 1500;

        // B Field
        Parameters["MOTCoilsSwitchOn"] = 0;
        Parameters["MOTCoilsSwitchOff"] = 20000;
        Parameters["MOTCoilsCurrentValue"] = 1.0; // 0.65;

        // Shim fields
        Parameters["xShimLoadCurrent"] = 0.0;
        Parameters["yShimLoadCurrent"] = 0.0;// -1.92;
        Parameters["zShimLoadCurrent"] = 0.0;// -0.22;

        // Shim fields ramp 
        Parameters["xShimRamp"] = 10.0;
        Parameters["yShimRamp"] = -10.0;// -1.92;
        Parameters["zShimRamp"] = 0.0;// -0.22;


        // v0 Light Switch
        Parameters["MOTAOMStartTime"] = 15000;
        Parameters["MOTAOMDuration"] = 500;

        // v0 Light Intensity
        Parameters["v0IntensityRampStartTime"] = 4000;
        Parameters["v0IntensityRampDuration"] = 500;
        Parameters["v0IntensityRampStartValue"] = 7.2; //5.6
        Parameters["v0IntensityEndValue"] = 8.0;//7.8
        Parameters["v0IntensityMolassesValue"] = 5.6;
        Parameters["v0IntensityRampBackTime"] = 20000;

        Parameters["V00EOMsidebandRatio"] = 5.5;
        Parameters["V00AOMSidebandAmplitude"] = 1.0;


        // v0 Light Frequency
        Parameters["v0FrequencyStartValue"] = 10.0; //9.0


        // v0 F=1 (dodgy code using an analogue output to control a TTL)
        Parameters["v0F1AOMStartValue"] = 5.0;
        Parameters["v0F1AOMOffValue"] = 0.0;
        Parameters["dummy"] = 0.0;



        //- AOM order
        // V0 blue from original for 80MHz
        // 494.432408THz
        /*
        Parameters["SidebandFreq1"] = 248.00 / 2.0; //+ F = 1- 
        Parameters["SidebandFreq2"] = 397.00 / 2.0; //- F = 2
        Parameters["SidebandFreq3"] = 318.50 / 2.0; //- F = 0
        Parameters["SidebandFreq4"] = 372.00 / 2.0; //+ F = 1+
        */
        //Lambda configuration
        Parameters["SidebandFreq1"] = 228.00 / 2.0; //+ F = 1- 
        Parameters["SidebandFreq2"] = 306.00 / 2.0; //- F = 0
        Parameters["SidebandFreq3"] = 380.00 / 2.0; //- F = 2
        Parameters["SidebandFreq4"] = 354.00 / 2.0; //+ F = 1+


        Parameters["BXAOMAttenuation"] = 3.0;

        //Sideband Amplitudes

        Parameters["SidebandAmp1"] = 4.0;
        Parameters["SidebandAmp2"] = 4.4;
        Parameters["SidebandAmp3"] = 7.0;
        Parameters["SidebandAmp4"] = 6.0;

        //20% saturation, Nov 19, 2024

        Parameters["SidebandAmpRampEnd1"] = 3.51;
        Parameters["SidebandAmpRampEnd2"] = 3.64;
        Parameters["SidebandAmpRampEnd3"] = 4.50;
        Parameters["SidebandAmpRampEnd4"] = 3.81;

        //10% saturation, Nov 14, 2024
        //Parameters["SidebandAmpRampEnd1"] = 3.3;
        //Parameters["SidebandAmpRampEnd2"] = 3.4;
        //Parameters["SidebandAmpRampEnd3"] = 3.5;
        //Parameters["SidebandAmpRampEnd4"] = 3.5;

        //Parameters["SidebandAmpRampEnd1"] = 3.8;
        //Parameters["SidebandAmpRampEnd2"] = 4.5;
        //Parameters["SidebandAmpRampEnd3"] = 4.5;
        //Parameters["SidebandAmpRampEnd4"] = 4.2;


        /*
        Parameters["SidebandAmpRampEnd1"] = 4.5;
        Parameters["SidebandAmpRampEnd2"] = 8.0;
        Parameters["SidebandAmpRampEnd3"] = 7.5;
        Parameters["SidebandAmpRampEnd4"] = 7.5;
        */
        // 1%
        //Parameters["SidebandAmpRampEnd1"] = 4.3;
        //Parameters["SidebandAmpRampEnd2"] = 3.1;
        //Parameters["SidebandAmpRampEnd3"] = 3.0;
        //Parameters["SidebandAmpRampEnd4"] = 2.4;


        //Parameters["SidebandAmpRampEnd1"] = 4.6;
        //Parameters["SidebandAmpRampEnd2"] = 3.4;
        //Parameters["SidebandAmpRampEnd3"] = 3.2;
        //Parameters["SidebandAmpRampEnd4"] = 3.0;

        //VCO Calibration
        //VCO frequency in MHz = offset + vol * gradient
        Parameters["POS300OffsetFreq"] = 129.2;
        Parameters["POS300Gradient"] = 10.6; 
        Parameters["POS150OffsetFreq"] = 62.6;
        Parameters["POS150Gradient"] = 7.68;

        Parameters["MOTHoldTime"] = 6000;
        Parameters["FreeExpTime"] = 1;

    }

    public override PatternBuilder32 GetDigitalPattern()
    {
        PatternBuilder32 p = new PatternBuilder32();
        int patternStartBeforeQ = (int)Parameters["TCLBlockStart"];
        int v0IntensityRampStart = patternStartBeforeQ + (int)Parameters["v0IntensityRampStartTime"];
        int v0IntensityRampEnd = v0IntensityRampStart + (int)Parameters["v0IntensityRampDuration"];
        int motHoldEnd = v0IntensityRampEnd + (int)Parameters["MOTHoldTime"];
        int imageTime = motHoldEnd + (int)Parameters["FreeExpTime"];


        MOTMasterScriptSnippet lm = new LoadMoleculeMOT(p, Parameters);  // This is how you load "preset" patterns.


        p.Pulse(patternStartBeforeQ, (int)Parameters["SlowingChirpStartTime"], (2 * (int)Parameters["SlowingChirpDuration"]) + 20000, "bXLockBlock"); // Want it to be blocked for whole time that bX laser is moved
        //p.Pulse(patternStartBeforeQ, 100, 100, "bXSlowingAOM"); //first pulse to slowing AOM
        p.Pulse(patternStartBeforeQ, (int)Parameters["SlowingChirpStartTime"] - 100, (int)Parameters["SlowingChirpDuration"] + 100, "bXSlowingAOM"); //first pulse to slowing AOM
        p.Pulse(patternStartBeforeQ, (int)Parameters["slowingRepumpAOMOnStart"], (int)Parameters["SlowingChirpStartTime"] + (int)Parameters["SlowingChirpDuration"] - (int)Parameters["slowingRepumpAOMOnStart"], "v10SlowingAOM"); //first pulse to slowing repump AOM

        //p.Pulse(patternStartBeforeQ, 3000, 1000, "cameraTrigger");
        p.Pulse(0, imageTime, (int)Parameters["Frame0TriggerDuration"], "cameraTrigger"); //camera trigger for first frame
        p.Pulse(patternStartBeforeQ, 2000, 10, "tofTrigger");

        p.AddEdge("rb2DMOTShutter", 0, true);
        p.AddEdge("rb2DMOTShutter", 5000, false);

        p.AddEdge("cafOptPumpingAOM", 0, true); // false for switch off
        p.AddEdge("cafOptPumpingShutter", 0, true); // true for switch off

        p.AddEdge("v0rfswitch1", 0, false);
        p.AddEdge("v0rfswitch2", 0, false);
        p.AddEdge("v0rfswitch3", 0, false);
        p.AddEdge("v0rfswitch4", 0, false);
        
        
        
        p.AddEdge("TweezerChamberRbMOTAOMs", 1000, true);
        p.AddEdge("TweezerChamberRbMOTAOMs", 10000, false);

      

        p.AddEdge("bXSlowingShutter", 0, false);
        p.AddEdge("bXSlowingShutter", 20000, true);



        return p;
    }

    public override AnalogPatternBuilder GetAnalogPattern()
    {
        AnalogPatternBuilder p = new AnalogPatternBuilder((int)Parameters["PatternLength"]);
        int v0IntensityRampStart = (int)Parameters["v0IntensityRampStartTime"];
        int v0IntensityRampEnd = v0IntensityRampStart + (int)Parameters["v0IntensityRampDuration"];
        int motHoldEnd = v0IntensityRampEnd + (int)Parameters["MOTHoldTime"];
        int imageTime = motHoldEnd + (int)Parameters["FreeExpTime"];

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

        //Switch BX AOM via analog output Mar 05 2024
        p.AddAnalogValue("BXAttenuation", 0, 0.0);
        p.AddAnalogValue("BXAttenuation", (int)Parameters["slowingAOMOnStart"], (double)Parameters["BXAOMAttenuation"]);
        p.AddAnalogValue("BXAttenuation", (int)Parameters["slowingAOMOffStart"], 0.0);
        p.AddAnalogValue("BXAttenuation", (int)Parameters["PatternLength"] - 10000, (double)Parameters["BXAOMAttenuation"]);

        p.AddAnalogValue("lightSwitch", 0, 0.0);
        //p.AddAnalogValue("lightSwitch", 1000, 2.0);

        p.AddAnalogValue("TCoolSidebandVCO", 0, 5.15); //5.15V, 63.5MHz
        p.AddAnalogValue("v0AOMSidebandAmp", imageTime, (double)Parameters["V00AOMSidebandAmplitude"]);

        // Slowing field
        p.AddAnalogValue("slowingCoilsCurrent", 0, (double)Parameters["slowingCoilsValue"]);
        p.AddAnalogValue("slowingCoilsCurrent", (int)Parameters["slowingCoilsOffTime"], 0.0);
        p.AddAnalogValue("slowingCoilsCurrent", (int)Parameters["slowingCoilsOffTime"], 0.0);

        // B Field
        p.AddAnalogValue("MOTCoilsCurrent", 0, (double)Parameters["MOTCoilsCurrentValue"]);
        p.AddAnalogValue("MOTCoilsCurrent", motHoldEnd + 1000, 0.0);

        //p.AddAnalogValue("MOTCoilsCurrent", imageTime, (double)Parameters["MOTCoilsCurrentValue"]);
        //p.AddAnalogValue("MOTCoilsCurrent", imageTime + 1000, 0.0);


        // Shim Fields
        p.AddAnalogValue("xShimCoilCurrent", 0, (double)Parameters["xShimLoadCurrent"]);
        p.AddAnalogValue("yShimCoilCurrent", 0, (double)Parameters["yShimLoadCurrent"]);
        p.AddAnalogValue("zShimCoilCurrent", 0, (double)Parameters["zShimLoadCurrent"]);

        //p.AddLinearRamp("xShimCoilCurrent", v0IntensityRampStart, (int)Parameters["v0IntensityRampDuration"], (double)Parameters["xShimRamp"]);
        //p.AddLinearRamp("yShimCoilCurrent", v0IntensityRampStart, (int)Parameters["v0IntensityRampDuration"], (double)Parameters["yShimRamp"]);
        //p.AddLinearRamp("zShimCoilCurrent", v0IntensityRampStart, (int)Parameters["v0IntensityRampDuration"], (double)Parameters["zShimRamp"]);

        //p.AddAnalogValue("xShimCoilCurrent", motHoldEnd, (double)Parameters["xShimLoadCurrent"]);
        //p.AddAnalogValue("yShimCoilCurrent", motHoldEnd, (double)Parameters["yShimLoadCurrent"]);
        //p.AddAnalogValue("zShimCoilCurrent", motHoldEnd, (double)Parameters["zShimLoadCurrent"]);


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
        
        
        p.AddLinearRamp("Rf1Amp", v0IntensityRampStart, (int)Parameters["v0IntensityRampDuration"], (double)Parameters["SidebandAmpRampEnd1"]);
        p.AddLinearRamp("Rf2Amp", v0IntensityRampStart, (int)Parameters["v0IntensityRampDuration"], (double)Parameters["SidebandAmpRampEnd2"]);
        p.AddLinearRamp("Rf3Amp", v0IntensityRampStart, (int)Parameters["v0IntensityRampDuration"], (double)Parameters["SidebandAmpRampEnd3"]);
        p.AddLinearRamp("Rf4Amp", v0IntensityRampStart, (int)Parameters["v0IntensityRampDuration"], (double)Parameters["SidebandAmpRampEnd4"]);
        

        p.AddAnalogValue("Rf1Amp", motHoldEnd, (double)Parameters["SidebandAmp1"]);
        p.AddAnalogValue("Rf2Amp", motHoldEnd, (double)Parameters["SidebandAmp2"]);
        p.AddAnalogValue("Rf3Amp", motHoldEnd, (double)Parameters["SidebandAmp3"]);
        p.AddAnalogValue("Rf4Amp", motHoldEnd, (double)Parameters["SidebandAmp4"]);
        

        //v0 chirp
        p.AddAnalogValue("v00Chirp", 0, 0.0);


        return p;
    }

}
