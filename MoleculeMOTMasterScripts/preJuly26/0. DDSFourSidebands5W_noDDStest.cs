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
        Parameters["TCLBlockDuration"] = 4000;
        Parameters["FlashToQ"] = 16; // This is a time before the Q switch
        Parameters["QSwitchPulseDuration"] = 10;
        Parameters["FlashPulseDuration"] = 10;
        Parameters["HeliumShutterToQ"] = 300;
        Parameters["HeliumShutterDuration"] = 2000;

        // Camera
        Parameters["Frame0Trigger"] = 4000;
        Parameters["Frame1Trigger"] = 5500;
        Parameters["Frame0TriggerDuration"] = 1000;
        Parameters["CameraTriggerTransverseTime"] = 120;
        Parameters["FrameTriggerInterval"] = 1100;
        Parameters["waitbeforeimage"] = 1;

        //PMT
        Parameters["PMTTriggerDuration"] = 10;




        // Slowing Chirp, 5W ALS laser250, 1250)
        
        Parameters["SlowingChirpStartTime"] = 300;//360; //400;// 380;
        Parameters["SlowingChirpDuration"] = 1250;////1400;//1160; //1160
        /*
        Parameters["SlowingChirpStartTime"] = 160;//360; //400;// 380;
        Parameters["SlowingChirpDuration"] = 1400;////1400;//1160; //1160
        */
        Parameters["SlowingChirpStartValue"] = 0.0;//0.0
        Parameters["SlowingChirpEndValue"] = -0.3; // -0.5 is 480MHz

        Parameters["BXAOM1att"] = 3.5;//7.85;//3.5;//7.2;
        Parameters["BXAOM2att"] = 3.7;

        Parameters["BXAttenuation"] = 0.91;

        /*
        
        // Parameters from most recent 5W
        Parameters["SlowingChirpStartTime"] = 250;//360; //400;// 380;
        Parameters["SlowingChirpDuration"] = 1200;////1400;//1160; //1160
        Parameters["SlowingChirpStartValue"] = 0.0;//0.0
        Parameters["SlowingChirpEndValue"] = -0.30; // -0.5 is 480MHz

        //Parameters from a feb 24 script
        
        Parameters["SlowingChirpStartTime"] = 200;//360; //400;// 380;
        Parameters["SlowingChirpDuration"] = 1200;////1400;//1160; //1160
        Parameters["SlowingChirpStartValue"] = 0.0;//0.0
        Parameters["SlowingChirpEndValue"] = -0.30; // -0.5 is 480MHz
        
        // Slowing Chirp, QuantelLaser
        Parameters["SlowingChirpStartTime"] = 500;//360; //400;// 380;
        Parameters["SlowingChirpDuration"] = 1100;////1400;//1160; //1160
        Parameters["SlowingChirpStartValue"] = 0.0;//0.0
        Parameters["SlowingChirpEndValue"] = -1.25; // -0.5 is 480MHz
        */


        // Slowing
        //Parameters["slowingAOMOnStart"] = (int)Parameters["SlowingChirpStartTime"] - 100;//160
        Parameters["slowingAOMOnDuration"] = 45000; //not used



        //Parameters["slowingAOMOffStart"] = (int)Parameters["SlowingChirpStartTime"] + (int)Parameters["SlowingChirpDuration"]; 
        Parameters["slowingAOMOffDuration"] = 40000; //40000;

        //Parameters["BXShutterClose"] = (int)Parameters["slowingAOMOffStart"] - 650;


        Parameters["slowingRepumpSwitchDelay"] = 0;
        Parameters["slowingRepumpAOMOnStart"] = 0;//started from 0
        Parameters["slowingRepumpAOMOffStart"] = 0;
        //Parameters["slowingRepumpAOMOffStart"] = 1450;
        //Parameters["slowingRepumpAOMOffStart"] = (int)Parameters["SlowingChirpStartTime"] + (int)Parameters["SlowingChirpDuration"] + (int)Parameters["slowingRepumpSwitchDelay"];
        Parameters["slowingRepumpAOMOffDuration"] = 35000;

        // Slowing field
        Parameters["slowingCoilsValue"] = 2.0; //1.05;
        //Parameters["slowingCoilsOffTime"] = (int)Parameters["slowingAOMOffStart"]; // 1500;

        // B Field
        Parameters["MOTCoilsSwitchOn"] = 0;
        Parameters["MOTCoilsSwitchOff"] = 40000;
        Parameters["MOTCoilsCurrentValue"] = 1.0; // 0.65;

        // Shim fields
        Parameters["xShimLoadCurrent"] = -2.21;
        Parameters["yShimLoadCurrent"] = -2.13;
        Parameters["zShimLoadCurrent"] = 0.41;


        // v0 Light Switch
        Parameters["MOTAOMStartTime"] = 15000;
        Parameters["MOTAOMDuration"] = 500;

        // v0 Light Intensity
        Parameters["v0IntensityRampStartTime"] = 5000;
        Parameters["v0IntensityRampDuration"] = 200;
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
        //Lambda configuration
        Parameters["SidebandFreq1"] = 228.00 / 2.0; //+ F = 1- 
        Parameters["SidebandFreq2"] = 306.00 / 2.0; //- F = 0
        Parameters["SidebandFreq3"] = 380.00 / 2.0; //- F = 2
        Parameters["SidebandFreq4"] = 354.00 / 2.0; //+ F = 1+

        Parameters["BXAOMAttenuation"] = 10.0;
        //Parameters["BXAOMFrequency"] = 5.8; //113MHz
        Parameters["SlowingRepumoAttenuation"] = 6.2;

        //Sideband Amplitudes
        // Recalibrated 30/04/2025
        Parameters["SidebandAmp1"] = 10.0;//6.7;
        Parameters["SidebandAmp2"] = 10.0;//7.7;
        Parameters["SidebandAmp3"] = 10.0;//8.0;
        Parameters["SidebandAmp4"] = 10.0;//8.0;

        Parameters["SidebandImAmp1"] = 10.0;//8.0;// 4.0;
        Parameters["SidebandImAmp2"] = 10.0;//8.0;// 4.5;
        Parameters["SidebandImAmp3"] = 10.0;//8.0;// 6.0;
        Parameters["SidebandImAmp4"] = 10.0;//8.0;// 4.7;

        //VCO Calibration
        //VCO frequency in MHz = offset + vol * gradient
        Parameters["POS300OffsetFreq"] = 129.2;
        Parameters["POS300Gradient"] = 10.6;
        Parameters["POS150OffsetFreq"] = 62.6;
        Parameters["POS150Gradient"] = 7.68;

        //sidebands
        Parameters["MOTFreqDDS0"] = 114.07; //+ F = 1- 
        Parameters["MOTFreqDDS1"] = 156.17; //- F = 0
        Parameters["MOTFreqDDS2"] = 188.04; //- F = 2. DONE
        Parameters["MOTFreqDDS3"] = 175.44; //+ F = 1+

        //amps for max optical power. dont go higher the amplifiers will saturate
        Parameters["MOTAmpDDS0"] = 0.25;
        Parameters["MOTAmpDDS1"] = 0.6;
        Parameters["MOTAmpDDS2"] = 0.35;
        Parameters["MOTAmpDDS3"] = 0.1;

    }

    public override Dictionary<string, List<List<double>>> GetDDSPattern()
    {
        Dictionary<string, List<List<double>>> p = new Dictionary<string, List<List<double>>>();

        addDDSPattern(p, "MOT", 0,
            (double)Parameters["MOTFreqDDS0"], (double)Parameters["MOTFreqDDS1"], (double)Parameters["MOTFreqDDS2"], (double)Parameters["MOTFreqDDS3"],
            (double)Parameters["MOTAmpDDS0"], (double)Parameters["MOTAmpDDS1"], (double)Parameters["MOTAmpDDS2"], (double)Parameters["MOTAmpDDS3"]);

        return p;
    }

    public void addDDSPattern(Dictionary<string, List<List<double>>> p, String name, int time, double freq1, double freq2, double freq3, double freq4, double amp1, double amp2, double amp3, double amp4,
    double freqSlope1 = 0.0, double freqSlope2 = 0.0, double freqSlope3 = 0.0, double freqSlope4 = 0.0, double ampSlope1 = 0.0, double ampSlope2 = 0.0, double ampSlope3 = 0.0, double ampSlope4 = 0.0)
    {

        // List<double> timeDelay, List<double> freq, List<double> amp, List<double> freq_slpoe, List<double> amp_slpoe
        List<double> timePar = new List<double>();
        timePar.Add(time / 100.0);
        List<double> freq = new List<double>();
        freq.Add(freq1);
        freq.Add(freq2);
        freq.Add(freq3);
        freq.Add(freq4);
        List<double> amp = new List<double>();
        amp.Add(amp1);
        amp.Add(amp2);
        amp.Add(amp3);
        amp.Add(amp4);
        // Scale ramp slope by 100 to convert 10 us clock periods to ms
        List<double> freqSlope = new List<double>();
        freqSlope.Add(freqSlope1 * 100.0);
        freqSlope.Add(freqSlope2 * 100.0);
        freqSlope.Add(freqSlope3 * 100.0);
        freqSlope.Add(freqSlope4 * 100.0);
        List<double> ampSlope = new List<double>();
        ampSlope.Add(ampSlope1 * 100.0);
        ampSlope.Add(ampSlope2 * 100.0);
        ampSlope.Add(ampSlope3 * 100.0);
        ampSlope.Add(ampSlope4 * 100.0);

        var patternEvent = new List<List<double>>
        {
            timePar,
            freq,
            amp,
            freqSlope,
            ampSlope
        };

        p.Add(name, patternEvent);
    }

    public override PatternBuilder32 GetDigitalPattern()
    {
        PatternBuilder32 p = new PatternBuilder32();
        int patternStartBeforeQ = (int)Parameters["TCLBlockStart"];
        //int BXShutterClose = patternStartBeforeQ + (int)Parameters["BXShutterClose"];

        p.AddEdge("rbAbsImagingBeam", 0, true);

        MOTMasterScriptSnippet lm = new LoadMoleculeMOTNoSlowingEdge(p, Parameters);  // This is how you load "preset" patterns.

        p.Pulse(patternStartBeforeQ, (int)Parameters["SlowingChirpStartTime"]-100, (2 * (int)Parameters["SlowingChirpDuration"])+20000, "bXLockBlock"); // Want it to be blocked for whole time that bX laser is moved
        //p.Pulse(patternStartBeforeQ, 100, 100, "bXSlowingAOM"); //first pulse to slowing AOM
        //p.Pulse(patternStartBeforeQ, (int)Parameters["SlowingChirpStartTime"] - 100, (int)Parameters["SlowingChirpDuration"] + 100, "bXSlowingAOM"); //first pulse to slowing AOM

        p.AddEdge("bXSlowingAOM", 0, true);


        p.Pulse(patternStartBeforeQ, 0, (int)Parameters["SlowingChirpStartTime"] + (int)Parameters["SlowingChirpDuration"] - (int)Parameters["slowingRepumpAOMOffStart"], "v10SlowingAOM"); //first pulse to slowing repump AOM

        // BX Shutter
        p.Pulse(patternStartBeforeQ, (int)Parameters["SlowingChirpStartTime"] + (int)Parameters["SlowingChirpDuration"], (int)Parameters["MOTCoilsSwitchOff"] - ((int)Parameters["SlowingChirpStartTime"] + (int)Parameters["SlowingChirpDuration"] + 200), "bXSlowingShutter");
        
        //p.AddEdge("bXSlowingShutter", 0, true);

        p.Pulse(patternStartBeforeQ, (int)Parameters["Frame0Trigger"], (int)Parameters["Frame0TriggerDuration"], "cameraTrigger"); //camera trigger for first frame
        
        //p.Pulse(patternStartBeforeQ, (int)Parameters["Frame1Trigger"], (int)Parameters["Frame0TriggerDuration"], "cameraTrigger"); //camera trigger for recap

        //p.Pulse(patternStartBeforeQ, (int)Parameters["MOTCoilsSwitchOff"] + 1000, (int)Parameters["Frame0TriggerDuration"], "cameraTrigger");

        //p.Pulse(patternStartBeforeQ, (int)Parameters["MOTCoilsSwitchOff"] + 1000, (int)Parameters["Frame0TriggerDuration"], "cameraTrigger"); // in sequence background
        //live lifetime
        //for (int i = 0; i < 12; i++)
        //{
        //    p.Pulse(patternStartBeforeQ, (int)Parameters["Frame0Trigger"] + i * 3000, (int)Parameters["Frame0TriggerDuration"], "cameraTrigger");
        //}


        p.Pulse(patternStartBeforeQ, 2000, 10, "tofTrigger");

        //p.AddEdge("rb2DMOTShutter", 0, true);
        //p.AddEdge("rb2DMOTShutter", 5000, false);

        //p.AddEdge("cafOptPumpingAOM", 0, true); // false for switch off
        //p.AddEdge("cafOptPumpingShutter", 0, true); // true for switch off

        p.AddEdge("v0rfswitch1", 0, false);
        p.AddEdge("v0rfswitch2", 0, false);
        p.AddEdge("v0rfswitch3", 0, false);
        p.AddEdge("v0rfswitch4", 0, false);
        p.AddEdge("v0ddsSwitchA", 0, false);
        p.AddEdge("v0ddsSwitchB", 0, false);
        p.AddEdge("v0ddsSwitchC", 0, false);
        p.AddEdge("v0ddsSwitchD", 0, true);
        p.AddEdge("v0ddsSwitchD", (int)Parameters["PatternLength"]-1000, false);

        p.AddEdge("TweezerChamberRbMOTAOMs", 1000, true);
        p.AddEdge("TweezerChamberRbMOTAOMs", 10000, false);


      


        return p;
    }

    public override AnalogPatternBuilder GetAnalogPattern()
    {
        AnalogPatternBuilder p = new AnalogPatternBuilder((int)Parameters["PatternLength"]);

        MOTMasterScriptSnippet lm = new LoadMoleculeMOTNoSlowingEdge(p, Parameters);

        // Add Analog Channels

        p.AddChannel("TCoolSidebandVCO");
        p.AddChannel("BXAttenuation");
        p.AddChannel("xShimCoilCurrent");
        p.AddChannel("yShimCoilCurrent");
        p.AddChannel("zShimCoilCurrent");
        p.AddChannel("SlowingRepumpAttenuation");
        p.AddChannel("BXAOM1att");
        p.AddChannel("BXAOM2att");
        p.AddChannel("ODT90att");
        p.AddChannel("ODT70att");
        //p.AddChannel("DipoleRetroX");
        //p.AddChannel("DipoleRetroY");

        // New BX power control

        //p.AddAnalogValue("BXAOM1att", 0, (double)Parameters["BXAOM1att"]);
        //p.AddAnalogValue("BXAOM2att", 0, (double)Parameters["BXAOM2att"]);

        p.AddAnalogValue("BXAOM1att", 0, 8.0);
        p.AddAnalogValue("BXAOM2att", 0, 8.0);

        //p.AddAnalogValue("BXAOM1att", (int)Parameters["SlowingChirpStartTime"] + (int)Parameters["SlowingChirpDuration"], 10.0);
        //p.AddAnalogValue("BXAOM2att", (int)Parameters["SlowingChirpStartTime"] + (int)Parameters["SlowingChirpDuration"], 10.0);

        p.AddAnalogValue("TCoolSidebandVCO", 0, 5.15); //5.15V, 63.5MHz
        p.AddAnalogValue("BXAttenuation", 0, (double)Parameters["BXAttenuation"]); //vva for Tcool, correct sideband structure


        //p.AddAnalogValue("TCoolSidebandVCO", 0, -3.461); //5.15V, 63.5MHz
        p.AddAnalogValue("SlowingRepumpAttenuation", 0, (double)Parameters["SlowingRepumoAttenuation"]);

        // Slowing field

        p.AddAnalogValue("slowingCoilsCurrent", 0, (double)Parameters["slowingCoilsValue"]);
        p.AddAnalogValue("slowingCoilsCurrent", (int)Parameters["SlowingChirpStartTime"] + (int)Parameters["SlowingChirpDuration"], 0.0);

        // TCOOL

        p.AddAnalogValue("TCoolSidebandVCO", 0, -3.461); //5.15V, 63.5MHz

        // MOT //

        // B Field
        p.AddAnalogValue("MOTCoilsCurrent", 0, (double)Parameters["MOTCoilsCurrentValue"]);
        p.AddAnalogValue("MOTCoilsCurrent", (int)Parameters["MOTCoilsSwitchOff"], 0.0);


        // Shim Fields
        p.AddAnalogValue("xShimCoilCurrent", 0, (double)Parameters["xShimLoadCurrent"]);
        p.AddAnalogValue("yShimCoilCurrent", 0, (double)Parameters["yShimLoadCurrent"]);
        p.AddAnalogValue("zShimCoilCurrent", 0, (double)Parameters["zShimLoadCurrent"]);

        return p;
    }

}
