using MOTMaster;
using MOTMaster.SnippetLibrary;

using System;
using System.Collections.Generic;
using System.Threading;
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
        Parameters["Frame0TriggerDuration"] = 1000;
        Parameters["CameraTriggerTransverseTime"] = 120;
        Parameters["FrameTriggerInterval"] = 1100;
        Parameters["waitbeforeimage"] = 1;

        //PMT
        Parameters["PMTTriggerDuration"] = 10;


        // Slowing Chirp, 5W ALS laser
        //Parameters["SlowingChirpStartTime"] = 250;//360; //400;// 380;
        //Parameters["SlowingChirpDuration"] = 1200;////1400;//1160; //1160
        //Parameters["SlowingChirpStartValue"] = 0.0;//0.0
        //Parameters["SlowingChirpEndValue"] = -0.30; // -0.5 is 480MHz

        // Slowing Chirp, QuantelLaser
        Parameters["SlowingChirpStartTime"] = 500;//360; //400;// 380;
        Parameters["SlowingChirpDuration"] = 1100;////1400;//1160; //1160
        Parameters["SlowingChirpStartValue"] = 0.0;//0.0
        Parameters["SlowingChirpEndValue"] = -1.25; // -0.5 is 480MHz

        // Slowing
        //Parameters["slowingAOMOnStart"] = (int)Parameters["SlowingChirpStartTime"] - 100;//160
        Parameters["slowingAOMOnDuration"] = 45000; //not used



        //Parameters["slowingAOMOffStart"] = (int)Parameters["SlowingChirpStartTime"] + (int)Parameters["SlowingChirpDuration"]; 
        Parameters["slowingAOMOffDuration"] = 40000; //40000;

        //Parameters["BXShutterClose"] = (int)Parameters["slowingAOMOffStart"] - 650;


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
        Parameters["MOTCoilsSwitchOff"] = 25000;
        Parameters["MOTCoilsCurrentValue"] = 1.0; // 0.65;

        // Shim fields
        Parameters["xShimLoadCurrent"] = -2.21;
        Parameters["yShimLoadCurrent"] = -2.13;
        Parameters["zShimLoadCurrent"] = -0.17;


        // v0 Light Switch
        Parameters["MOTAOMStartTime"] = 15000;
        Parameters["MOTAOMDuration"] = 500;

       
        Parameters["dummy"] = 0.0;



        //- AOM order

        //Red MOT configuration
        Parameters["MOTFreqDDS1"] = 114.07; //+ F = 1- 
        Parameters["MOTFreqDDS2"] = 156.17; //- F = 0
        Parameters["MOTFreqDDS3"] = 188.00; //- F = 2
        Parameters["MOTFreqDDS4"] = 175.44; //+ F = 1+

        Parameters["MOTAmpDDS1"] = 1.0;
        Parameters["MOTAmpDDS2"] = 1.0;
        Parameters["MOTAmpDDS3"] = 1.0;
        Parameters["MOTAmpDDS4"] = 1.0;

        Parameters["BXAOMAttenuation"] = 10.0;
        //Parameters["BXAOMFrequency"] = 5.8; //113MHz
        Parameters["SlowingRepumoAttenuation"] = 6.2;

        Parameters["Switchtime"] = 6000;



    }


    private void prePatternSetup()
    {

        NeanderthalDDSController.Controller DDSCtrl = (NeanderthalDDSController.Controller)(Activator.GetObject(typeof(NeanderthalDDSController.Controller), "tcp://localhost:1818/controller.rem"));
        DDSCtrl.setBreakFlag(true);
        DDSCtrl.clearPatternList();

        addDDSPattern(DDSCtrl, "MOT", 0,
            (double)Parameters["MOTFreqDDS1"], (double)Parameters["MOTFreqDDS2"], (double)Parameters["MOTFreqDDS3"], (double)Parameters["MOTFreqDDS4"],
            (double)Parameters["MOTAmpDDS1"], (double)Parameters["MOTAmpDDS2"], (double)Parameters["MOTAmpDDS3"], (double)Parameters["MOTAmpDDS4"]);

        DDSCtrl.setBreakFlag(false);
        runDDSPattern(DDSCtrl);



    }


    public override PatternBuilder32 GetDigitalPattern()
    {
        prePatternSetup();
        PatternBuilder32 p = new PatternBuilder32();
        int patternStartBeforeQ = (int)Parameters["TCLBlockStart"];
        //int BXShutterClose = patternStartBeforeQ + (int)Parameters["BXShutterClose"];


        MOTMasterScriptSnippet lm = new LoadMoleculeMOTNoSlowingEdge(p, Parameters);  // This is how you load "preset" patterns.


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


        //these are the shutter settings for the fiber eom slowing light setup
        // p.AddEdge("bXSlowingShutter", 0, false);
        // p.AddEdge("bXSlowingShutter", 20000, true);

        p.AddEdge("bXSlowingShutter", 0, true);
        p.AddEdge("bXSlowingShutter", (int)Parameters["SlowingChirpStartTime"] + (int)Parameters["SlowingChirpDuration"] - 650, false);
        p.AddEdge("bXSlowingShutter", 26000, true);



        return p;
    }

    public override AnalogPatternBuilder GetAnalogPattern()
    {
        AnalogPatternBuilder p = new AnalogPatternBuilder((int)Parameters["PatternLength"]);

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

        p.AddChannel("SlowingRepumpAttenuation");
        p.AddChannel("BXFreq");

        //Switch BX AOM via analog output Mar 05 2024
        //p.AddAnalogValue("BXAttenuation", 0, 0.1);
        p.AddAnalogValue("BXAttenuation", (int)Parameters["SlowingChirpStartTime"] - 100, (double)Parameters["BXAOMAttenuation"]);
        p.AddAnalogValue("BXAttenuation", (int)Parameters["SlowingChirpStartTime"] + (int)Parameters["SlowingChirpDuration"], 0.1);
        //p.AddAnalogValue("BXAttenuation", (int)Parameters["PatternLength"] - 10000, (double)Parameters["BXAOMAttenuation"]);


        p.AddAnalogValue("lightSwitch", 0, 0.0);
        //p.AddAnalogValue("lightSwitch", 1000, 2.0);

        p.AddAnalogValue("TCoolSidebandVCO", 0, -3.461); //5.15V, 63.5MHz
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


        return p;
    }

    public void addDDSPattern(NeanderthalDDSController.Controller DDSCtrl, String name, int time, double freq1, double freq2, double freq3, double freq4, double amp1, double amp2, double amp3, double amp4,
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
        List<double> ampSlpoe = new List<double>();
        ampSlpoe.Add(ampSlope1 * 100.0);
        ampSlpoe.Add(ampSlope2 * 100.0);
        ampSlpoe.Add(ampSlope3 * 100.0);
        ampSlpoe.Add(ampSlope4 * 100.0);

        DDSCtrl.addParToPatternList(name, timePar, freq, amp, freqSlope, ampSlpoe);

    }

    public void runDDSPattern(NeanderthalDDSController.Controller DDSCtrl)
    {
        DDSCtrl.startRepetitivePattern();
    }

}
