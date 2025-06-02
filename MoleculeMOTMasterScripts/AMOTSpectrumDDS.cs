using MOTMaster;
using MOTMaster.SnippetLibrary;

using System;
using System.Collections.Generic;

using DAQ.Pattern;
using DAQ.Analog;
using DAQ.Environment;
using System.Threading;

// This script is supposed to be the basic script for loading a molecule MOT.
// Note that times are all in units of the clock periods of the two pattern generator boards (at present, both are 10us).
// All times are relative to the Q switch, though note that this is not the first event in the pattern.
public class Patterns : MOTMasterScript
{

    
    //EnvironsHelper eHelper = new EnvironsHelper((String)System.Environment.GetEnvironmentVariables()["COMPUTERNAME"]);
    //WavemeterLock.Controller wmlController = (WavemeterLock.Controller)(Activator.GetObject(typeof(WavemeterLock.Controller), "tcp://" + (String)System.Environment.GetEnvironmentVariables()["COMPUTERNAME"] + ":" + eHelper.wavemeterLockTCPChannel.ToString() + "/controller.rem"));
    
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
        Parameters["SlowingChirpEndValue"] = -0.30; // -0.5 is 480MHz

        // Slowing
        Parameters["slowingAOMOnStart"] = (int)Parameters["SlowingChirpStartTime"] - 100;//160
        Parameters["slowingAOMOnDuration"] = 45000;



        Parameters["slowingAOMOffStart"] = (int)Parameters["SlowingChirpStartTime"] + (int)Parameters["SlowingChirpDuration"]; 
        Parameters["slowingAOMOffDuration"] = 40000;//40000;


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
        Parameters["MOTCoilsSwitchOff"] = 25000;
        Parameters["MOTCoilsCurrentValue"] = 1.0; // 0.65;

        // Shim fields
        Parameters["xShimLoadCurrent"] = 0.0;
        Parameters["yShimLoadCurrent"] = 0.0;
        Parameters["zShimLoadCurrent"] = -0.22;


        // v0 Light Switch
        Parameters["MOTAOMStartTime"] = 15000;
        Parameters["MOTAOMDuration"] = 500;

        // v0 Light Intensity
        Parameters["v0IntensityRampStartTime"] = 5000;
        Parameters["v0IntensityRampDuration"] = 400;
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
        Parameters["SidebandFreq1"] = 114.07; //+ F = 1- 
        Parameters["SidebandFreq2"] = 156.17; //- F = 0
        Parameters["SidebandFreq3"] = 188.00; //- F = 2
        Parameters["SidebandFreq4"] = 175.44; //+ F = 1+

        Parameters["BXAOMAttenuation"] = 3.0;
        //Parameters["BXAOMFrequency"] = 5.8; //113MHz
        Parameters["SlowingRepumoAttenuation"] = 6.2;

        //Sideband Amplitudes

        // Recalibrated 06/02/25

        Parameters["SidebandAmp1"] = 6.7;
        Parameters["SidebandAmp2"] = 7.7;
        Parameters["SidebandAmp3"] = 8.0;
        Parameters["SidebandAmp4"] = 8.0;

        Parameters["SidebandImAmp1"] = 6.7;
        Parameters["SidebandImAmp2"] = 7.7;
        Parameters["SidebandImAmp3"] = 8.0;
        Parameters["SidebandImAmp4"] = 8.0;


        //VCO Calibration
        //VCO frequency in MHz = offset + vol * gradient
        Parameters["POS300OffsetFreq"] = 129.2;
        Parameters["POS300Gradient"] = 10.6; 
        Parameters["POS150OffsetFreq"] = 62.6;
        Parameters["POS150Gradient"] = 7.68;

        Parameters["PatternStartTime"] = 0;

        Parameters["SidebandAmpDDS1"] = 0.19;
        Parameters["SidebandAmpDDS2"] = 0.43;
        Parameters["SidebandAmpDDS3"] = 0.94;
        Parameters["SidebandAmpDDS4"] = 0.55;

        Parameters["SidebandImAmpDDS1"] = 0.19;
        Parameters["SidebandImAmpDDS2"] = 0.43;
        Parameters["SidebandImAmpDDS3"] = 0.94;
        Parameters["SidebandImAmpDDS4"] = 0.55;
        

    }

    private void prePatternSetup()
    {
        //NeanderthalDDSController.Controller DDSCtrl = (NeanderthalDDSController.Controller)(Activator.GetObject(typeof(NeanderthalDDSController.Controller), "tcp://localhost:1818/controller.rem"));
        /*
        addDDSPattern(DDSCtrl, "PatternStart", 0, (double)Parameters["SidebandFreq1"], (double)Parameters["SidebandFreq2"], (double)Parameters["SidebandFreq3"], (double)Parameters["SidebandFreq4"], 
            (double)Parameters["SidebandAmpDDS1"], (double)Parameters["SidebandAmpDDS2"], (double)Parameters["SidebandAmpDDS3"], (double)Parameters["SidebandAmpDDS4"]);
        addDDSPattern(DDSCtrl, "Image", (int)Parameters["Frame0Trigger"], (double)Parameters["SidebandFreq1"], (double)Parameters["SidebandFreq2"], (double)Parameters["SidebandFreq3"], (double)Parameters["SidebandFreq4"],
            (double)Parameters["SidebandImAmpDDS1"], (double)Parameters["SidebandImAmpDDS2"], (double)Parameters["SidebandImAmpDDS3"], (double)Parameters["SidebandImAmpDDS4"]);

        runDDSPattern(DDSCtrl);
        */
        //wmlController.setSlaveFrequency((string)Parameters["Laser"], (bool)Parameters["Switch"] ? (double)Parameters["OnFrequency"] : (double)Parameters["OffFrequency"]);
        //Thread.Sleep((int)Parameters["WaitTime"]);
    }


    public override PatternBuilder32 GetDigitalPattern()
    {
        //prePatternSetup();
        PatternBuilder32 p = new PatternBuilder32();
        int patternStartBeforeQ = (int)Parameters["TCLBlockStart"];


        MOTMasterScriptSnippet lm = new LoadMoleculeMOT(p, Parameters);  // This is how you load "preset" patterns.


        p.Pulse(patternStartBeforeQ, (int)Parameters["SlowingChirpStartTime"], (2 * (int)Parameters["SlowingChirpDuration"]) + 20000, "bXLockBlock"); // Want it to be blocked for whole time that bX laser is moved
        //p.Pulse(patternStartBeforeQ, 100, 100, "bXSlowingAOM"); //first pulse to slowing AOM
        p.Pulse(patternStartBeforeQ, (int)Parameters["SlowingChirpStartTime"] - 100, (int)Parameters["SlowingChirpDuration"] + 100, "bXSlowingAOM"); //first pulse to slowing AOM
        p.Pulse(patternStartBeforeQ, (int)Parameters["slowingRepumpAOMOnStart"], (int)Parameters["SlowingChirpStartTime"] + (int)Parameters["SlowingChirpDuration"] - (int)Parameters["slowingRepumpAOMOnStart"], "v10SlowingAOM"); //first pulse to slowing repump AOM


        p.Pulse(patternStartBeforeQ, (int)Parameters["Frame0Trigger"], (int)Parameters["Frame0TriggerDuration"], "cameraTrigger"); //camera trigger for first frame
        //p.Pulse(patternStartBeforeQ, 8000, (int)Parameters["Frame0TriggerDuration"], "cameraTrigger"); uncomment for second image


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

        p.AddEdge("TweezerChamberRbMOTAOMs", 1000, true);
        p.AddEdge("TweezerChamberRbMOTAOMs", 10000, false);

      

        p.AddEdge("bXSlowingShutter", 0, false);
        p.AddEdge("bXSlowingShutter", 20000, true);



        return p;
    }

    public override AnalogPatternBuilder GetAnalogPattern()
    {
        AnalogPatternBuilder p = new AnalogPatternBuilder((int)Parameters["PatternLength"]);

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
        p.AddChannel("BXFreq");

        //Switch BX AOM via analog output Mar 05 2024
        p.AddAnalogValue("BXAttenuation", 0, 0.0);
        p.AddAnalogValue("BXAttenuation", (int)Parameters["slowingAOMOnStart"], (double)Parameters["BXAOMAttenuation"]);
        p.AddAnalogValue("BXAttenuation", (int)Parameters["slowingAOMOffStart"], 0.0);
        p.AddAnalogValue("BXAttenuation", (int)Parameters["PatternLength"] - 10000, (double)Parameters["BXAOMAttenuation"]);

        //p.AddAnalogValue("BXFreq", 0, (double)Parameters["BXAOMFrequency"]);
        //p.AddAnalogValue("BXFreq", 0, 0.0);
        //p.AddAnalogValue("BXFreq", (int)Parameters["slowingAOMOnStart"], (double)Parameters["BXAOMFrequency"]);
        //p.AddAnalogValue("BXFreq", (int)Parameters["slowingAOMOffStart"], 0.0);
        //p.AddAnalogValue("BXFreq", (int)Parameters["PatternLength"] - 10000, BXAOMFrequency);

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
        p.AddAnalogValue("MOTCoilsCurrent", (int)Parameters["MOTCoilsSwitchOff"], 0.0);


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

        p.AddAnalogValue("Rf1Amp", 0, (double)Parameters["SidebandAmp1"]);
        p.AddAnalogValue("Rf2Amp", 0, (double)Parameters["SidebandAmp2"]);
        p.AddAnalogValue("Rf3Amp", 0, (double)Parameters["SidebandAmp3"]);
        p.AddAnalogValue("Rf4Amp", 0, (double)Parameters["SidebandAmp4"]);

        p.AddAnalogValue("Rf1Amp", (int)Parameters["Frame0Trigger"], (double)Parameters["SidebandImAmp1"]);
        p.AddAnalogValue("Rf2Amp", (int)Parameters["Frame0Trigger"], (double)Parameters["SidebandImAmp2"]);
        p.AddAnalogValue("Rf3Amp", (int)Parameters["Frame0Trigger"], (double)Parameters["SidebandImAmp3"]);
        p.AddAnalogValue("Rf4Amp", (int)Parameters["Frame0Trigger"], (double)Parameters["SidebandImAmp4"]);


        //v0 chirp
        p.AddAnalogValue("v00Chirp", 0, 0.0);
        

        return p;
    }

    public void addDDSPattern(NeanderthalDDSController.Controller DDSCtrl, String name, int time, double freq1, double freq2, double freq3, double freq4, double amp1, double amp2, double amp3, double amp4,
            double freqSlope1 = 0.0, double freqSlope2 = 0.0, double freqSlope3 = 0.0, double freqSlope4 = 0.0, double ampSlope1 = 0.0, double ampSlope2 = 0.0, double ampSlope3 = 0.0, double ampSlope4 = 0.0)
    {
        //List<double> timeDelay, List<double> freq, List<double> amp, List<double> freq_slpoe, List<double> amp_slpoe
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
        List<double> freqSlope = new List<double>();
        freqSlope.Add(freqSlope1);
        freqSlope.Add(freqSlope2);
        freqSlope.Add(freqSlope3);
        freqSlope.Add(freqSlope4);
        List<double> ampSlpoe = new List<double>();
        ampSlpoe.Add(ampSlope1);
        ampSlpoe.Add(ampSlope2);
        ampSlpoe.Add(ampSlope3);
        ampSlpoe.Add(ampSlope4);

        DDSCtrl.clearPatternList();
        DDSCtrl.addParToPatternList(name, timePar, freq, amp, freqSlope, ampSlpoe);

    }

    public void runDDSPattern(NeanderthalDDSController.Controller DDSCtrl)
    {
        DDSCtrl.openCard();
        DDSCtrl.startSinglePattern();
        // Wait till sequence ends
        DDSCtrl.closeCard();
    }
}
