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
        Parameters["Frame0Trigger"] = 4000;
        Parameters["Frame0TriggerDuration"] = 100;
        Parameters["CameraTriggerTransverseTime"] = 120;
        Parameters["FrameTriggerInterval"] = 1100;
        Parameters["waitbeforeimage"] = 1;

        //PMT
        Parameters["PMTTriggerDuration"] = 10;

        // Slowing
        Parameters["slowingAOMOnStart"] = 200;//160
        Parameters["slowingAOMOnDuration"] = 45000;
        
        Parameters["slowingAOMOffStart"] = 1800;
        Parameters["slowingAOMOffDuration"] = 40000;//40000;


        
        Parameters["slowingRepumpAOMOnStart"] = 0;//started from 0
        Parameters["slowingRepumpAOMOffStart"] = 1800;// 1760;//1520
        Parameters["slowingRepumpAOMOffDuration"] = 35000;


        // Slowing Chirp
        Parameters["SlowingChirpStartTime"] = 600;//360; //400;// 380;
        Parameters["SlowingChirpDuration"] = 1200;////1400;//1160; //1160
        Parameters["SlowingChirpStartValue"] = 0.0;//0.0
        Parameters["SlowingChirpEndValue"] = -0.3;//-1.25; //-1.25 //225MHz/V 120m/s/V

        // Slowing field
        Parameters["slowingCoilsValue"] = 0.4; //1.05;
        Parameters["slowingCoilsOffTime"] = 2000; // 1500;

        // B Field
        Parameters["MOTCoilsSwitchOn"] = 0;
        Parameters["MOTCoilsSwitchOff"] = 20000;
        Parameters["MOTCoilsCurrentValue"] = 1.0; // 0.65;

        // Shim fields
        Parameters["xShimLoadCurrent"] = -1.60;
        Parameters["yShimLoadCurrent"] = -1.92;
        Parameters["zShimLoadCurrent"] = -0.22;


        // v0 Light Switch
        Parameters["MOTAOMStartTime"] = 15000;
        Parameters["MOTAOMDuration"] = 500;

        // v0 Light Intensity
        Parameters["v0IntensityRampStartTime"] = 5000;
        Parameters["v0IntensityRampDuration"] = 2000;
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

        //Parameters["MolassesDetuning"] = 25.00;
        Parameters["MolassesDetuning1"] = 25.00;
        Parameters["MolassesDetuning2"] = 27.05; //+ F = 2
        Parameters["MolassesDetuning3"] = 29.68; //- F = 2

        Parameters["SidebandFreqMolasses1"] = (double)Parameters["SidebandFreq1"] - (double)Parameters["MolassesDetuning1"] / 2.0; //+ F = 1- 
        //Parameters["SidebandFreqMolasses2"] = (double)Parameters["SidebandFreq2"] - (double)Parameters["MolassesDetuning"] / 2.0; //- F = 0
        Parameters["SidebandFreqMolasses3"] = (double)Parameters["SidebandFreq3"] - (double)Parameters["MolassesDetuning3"] / 2.0; //- F = 2
        Parameters["SidebandFreqMolasses4"] = (double)Parameters["SidebandFreq3"] - (double)Parameters["MolassesDetuning2"] / 2.0; //+ F = 2


        Parameters["BXAOMAttenuation"] = 4.0;

        //Sideband Amplitudes

        Parameters["SidebandAmp1"] = 6.0;
        Parameters["SidebandAmp2"] = 7.0;
        Parameters["SidebandAmp3"] = 7.0;
        Parameters["SidebandAmp4"] = 7.0;

        //10%
        
        Parameters["SidebandAmpRampEnd1"] = 4.8;
        Parameters["SidebandAmpRampEnd2"] = 3.6;
        Parameters["SidebandAmpRampEnd3"] = 3.4;
        Parameters["SidebandAmpRampEnd4"] = 3.2;

        // 1%
        //Parameters["SidebandAmpRampEnd1"] = 4.3;
        //Parameters["SidebandAmpRampEnd2"] = 3.1;
        //Parameters["SidebandAmpRampEnd3"] = 3.0;
        //Parameters["SidebandAmpRampEnd4"] = 2.4;


        //Parameters["SidebandAmpRampEnd1"] = 4.6;
        //Parameters["SidebandAmpRampEnd2"] = 3.4;
        //Parameters["SidebandAmpRampEnd3"] = 3.2;
        //Parameters["SidebandAmpRampEnd4"] = 3.0;

        Parameters["SidebandAmpRampMolasses1"] = 10.0;
        Parameters["SidebandAmpRampMolasses2"] = 7.0;
        Parameters["SidebandAmpRampMolasses3"] = 6.0;
        Parameters["SidebandAmpRampMolasses4"] = 4.2;

        //VCO Calibration
        //VCO frequency in MHz = offset + vol * gradient
        Parameters["POS300OffsetFreq"] = 129.2;
        Parameters["POS300Gradient"] = 10.6; 
        Parameters["POS150OffsetFreq"] = 62.6;
        Parameters["POS150Gradient"] = 7.68;

        Parameters["MOTHoldTime"] = 100;
        Parameters["FrequencySettleTime"] = 100;
        Parameters["MolassesDuration"] = 300;
        Parameters["FreeExpTime"] = 500;

    }

    public override PatternBuilder32 GetDigitalPattern()
    {
        PatternBuilder32 p = new PatternBuilder32();
        int patternStartBeforeQ = (int)Parameters["TCLBlockStart"];
        int v0IntensityRampStart = patternStartBeforeQ + (int)Parameters["v0IntensityRampStartTime"];
        int v0IntensityRampEnd = v0IntensityRampStart + (int)Parameters["v0IntensityRampDuration"];
        int motHoldEnd = v0IntensityRampEnd + (int)Parameters["MOTHoldTime"];
        int molassesStart = motHoldEnd + (int)Parameters["FrequencySettleTime"];
        int molassesEnd = molassesStart + (int)Parameters["MolassesDuration"];
        int imageTime = molassesEnd + (int)Parameters["FreeExpTime"];


        MOTMasterScriptSnippet lm = new LoadMoleculeMOT(p, Parameters);  // This is how you load "preset" patterns.

        //p.Pulse(patternStartBeforeQ, (int)Parameters["Frame0Trigger"], (int)Parameters["Frame0TriggerDuration"], "cameraTrigger");
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

        p.AddEdge("v0rfswitch1", motHoldEnd, true);
        p.AddEdge("v0rfswitch2", motHoldEnd, true);
        p.AddEdge("v0rfswitch3", motHoldEnd, true);
        p.AddEdge("v0rfswitch4", motHoldEnd, true);

        p.AddEdge("v0rfswitch1", molassesStart, false);
        //p.AddEdge("v0rfswitch2", molassesStart, false);
        //p.AddEdge("v0rfswitch3", molassesStart, false);
        p.AddEdge("v0rfswitch4", molassesStart, false);

        p.AddEdge("v0rfswitch1", molassesEnd, true);
        //p.AddEdge("v0rfswitch2", molassesEnd, true);
        //p.AddEdge("v0rfswitch3", molassesEnd, true);
        p.AddEdge("v0rfswitch4", molassesEnd, true);

        p.AddEdge("v0rfswitch1", imageTime, false);
        p.AddEdge("v0rfswitch2", imageTime, false);
        p.AddEdge("v0rfswitch3", imageTime, false);
        p.AddEdge("v0rfswitch4", imageTime, false);

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
        int molassesStart = motHoldEnd + (int)Parameters["FrequencySettleTime"];
        int molassesEnd = molassesStart + (int)Parameters["MolassesDuration"];
        int imageTime = molassesEnd + (int)Parameters["FreeExpTime"];

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

        p.AddAnalogValue("TCoolSidebandVCO", 0, 5.1); //5.1V, 63.9MHz
        p.AddAnalogValue("v0AOMSidebandAmp", 0, (double)Parameters["V00AOMSidebandAmplitude"]);

        // Slowing field
        p.AddAnalogValue("slowingCoilsCurrent", 0, (double)Parameters["slowingCoilsValue"]);
        p.AddAnalogValue("slowingCoilsCurrent", (int)Parameters["slowingCoilsOffTime"], 0.0);

        // B Field
        p.AddAnalogValue("MOTCoilsCurrent", 0, (double)Parameters["MOTCoilsCurrentValue"]);
        p.AddAnalogValue("MOTCoilsCurrent", motHoldEnd, 0.0);

        //p.AddAnalogValue("MOTCoilsCurrent", imageTime, (double)Parameters["MOTCoilsCurrentValue"]);
        //p.AddAnalogValue("MOTCoilsCurrent", imageTime + (int)Parameters["Frame0TriggerDuration"], 0.0);


        // Shim Fields
        p.AddAnalogValue("xShimCoilCurrent", 0, (double)Parameters["xShimLoadCurrent"]);
        p.AddAnalogValue("yShimCoilCurrent", 0, (double)Parameters["yShimLoadCurrent"]);
        p.AddAnalogValue("zShimCoilCurrent", 0, (double)Parameters["zShimLoadCurrent"]);
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

        p.AddAnalogValue("Rf1Freq", motHoldEnd, ((double)Parameters["SidebandFreqMolasses1"] - (double)Parameters["POS150OffsetFreq"]) / (double)Parameters["POS150Gradient"]);
        //p.AddAnalogValue("Rf2Freq", motHoldEnd, ((double)Parameters["SidebandFreqMolasses2"] - (double)Parameters["POS300OffsetFreq"]) / (double)Parameters["POS300Gradient"]);
        p.AddAnalogValue("Rf3Freq", motHoldEnd, ((double)Parameters["SidebandFreqMolasses3"] - (double)Parameters["POS300OffsetFreq"]) / (double)Parameters["POS300Gradient"]);
        p.AddAnalogValue("Rf4Freq", motHoldEnd, ((double)Parameters["SidebandFreqMolasses4"] - (double)Parameters["POS300OffsetFreq"]) / (double)Parameters["POS300Gradient"]);
        
        //p.AddAnalogValue("Rf1Freq", molassesEnd, ((double)Parameters["SidebandFreq1"] - (double)Parameters["POS150OffsetFreq"]) / (double)Parameters["POS150Gradient"]);
        //p.AddAnalogValue("Rf2Freq", molassesEnd, ((double)Parameters["SidebandFreq2"] - (double)Parameters["POS300OffsetFreq"]) / (double)Parameters["POS300Gradient"]);
        //p.AddAnalogValue("Rf3Freq", molassesEnd, ((double)Parameters["SidebandFreq3"] - (double)Parameters["POS300OffsetFreq"]) / (double)Parameters["POS300Gradient"]);
        //p.AddAnalogValue("Rf4Freq", molassesEnd, ((double)Parameters["SidebandFreq4"] - (double)Parameters["POS300OffsetFreq"]) / (double)Parameters["POS300Gradient"]);
        

        p.AddAnalogValue("Rf1Amp", 0, (double)Parameters["SidebandAmp1"]);
        p.AddAnalogValue("Rf2Amp", 0, (double)Parameters["SidebandAmp2"]);
        p.AddAnalogValue("Rf3Amp", 0, (double)Parameters["SidebandAmp3"]);
        p.AddAnalogValue("Rf4Amp", 0, (double)Parameters["SidebandAmp4"]);


        p.AddLinearRamp("Rf1Amp", v0IntensityRampStart, (int)Parameters["v0IntensityRampDuration"], (double)Parameters["SidebandAmpRampEnd1"]);
        p.AddLinearRamp("Rf2Amp", v0IntensityRampStart, (int)Parameters["v0IntensityRampDuration"], (double)Parameters["SidebandAmpRampEnd2"]);
        p.AddLinearRamp("Rf3Amp", v0IntensityRampStart, (int)Parameters["v0IntensityRampDuration"], (double)Parameters["SidebandAmpRampEnd3"]);
        p.AddLinearRamp("Rf4Amp", v0IntensityRampStart, (int)Parameters["v0IntensityRampDuration"], (double)Parameters["SidebandAmpRampEnd4"]);


        p.AddAnalogValue("Rf1Amp", molassesStart, (double)Parameters["SidebandAmpRampMolasses1"]);
        p.AddAnalogValue("Rf2Amp", molassesStart, (double)Parameters["SidebandAmpRampMolasses2"]);
        p.AddAnalogValue("Rf3Amp", molassesStart, (double)Parameters["SidebandAmpRampMolasses3"]);
        p.AddAnalogValue("Rf4Amp", molassesStart, (double)Parameters["SidebandAmpRampMolasses4"]);
        
        p.AddAnalogValue("Rf1Amp", imageTime, (double)Parameters["SidebandAmp1"]);
        p.AddAnalogValue("Rf2Amp", imageTime, (double)Parameters["SidebandAmp2"]);
        p.AddAnalogValue("Rf3Amp", imageTime, (double)Parameters["SidebandAmp3"]);
        p.AddAnalogValue("Rf4Amp", imageTime, (double)Parameters["SidebandAmp4"]);
        

        //v0 chirp
        p.AddAnalogValue("v00Chirp", 0, 0.0);


        return p;
    }

}
