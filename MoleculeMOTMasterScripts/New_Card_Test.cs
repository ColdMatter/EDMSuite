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
        Parameters["PatternLength"] = 20000;
        Parameters["TCLBlockStart"] = 4000; // This is a time before the Q switch
        Parameters["TCLBlockDuration"] = 15000;
        Parameters["FlashToQ"] = 16; // This is a time before the Q switch
        Parameters["QSwitchPulseDuration"] = 10;
        Parameters["FlashPulseDuration"] = 10;
        Parameters["HeliumShutterToQ"] = 100;
        Parameters["HeliumShutterDuration"] = 1550;

        // Camera
        Parameters["Frame0Trigger"] = 4000;
        Parameters["Frame0TriggerDuration"] = 1000;
        Parameters["CameraTriggerTransverseTime"] = 120;
        
        //PMT
        Parameters["PMTTriggerDuration"] = 10;

        // Slowing
        Parameters["slowingAOMOnStart"] = 180; //180
        Parameters["PMTTrigger"] = 5000;
        Parameters["slowingAOMOnDuration"] = 45000;
        Parameters["slowingAOMOffStart"] = 1520;//started from 1520
        Parameters["slowingAOMOffDuration"] = 40000;
        Parameters["slowingRepumpAOMOnStart"] = 0;//started from 0
        Parameters["slowingRepumpAOMOnDuration"] = 45000;
        Parameters["slowingRepumpAOMOffStart"] = 1520;//1520
        Parameters["slowingRepumpAOMOffDuration"] = 65000;

        // Slowing Chirp
        Parameters["SlowingChirpStartTime"] = 380;// 380;
        Parameters["SlowingChirpDuration"] = 1160; //1160
        Parameters["SlowingChirpStartValue"] = 0.0;//0.0
        Parameters["SlowingChirpEndValue"] = -1.25; //-1.25

        // Slowing field
        Parameters["slowingCoilsValue"] = 0.42; //1.05;
        Parameters["slowingCoilsOffTime"] =  1500;
        
        // B Field
        Parameters["MOTCoilsSwitchOn"] = 0;
        Parameters["MOTCoilsSwitchOff"] = 5000;
        Parameters["MOTCoilsCurrentValue"] = 1.0;//1.0; // 0.65;

        // Shim fields
        Parameters["xShimLoadCurrent"] = 0.0;//3.6
        Parameters["yShimLoadCurrent"] = 0.0;//-0.12
        Parameters["zShimLoadCurrent"] =  0.0;//-5.35


        // v0 Light Switch
        Parameters["MOTAOMStartTime"] = 15000;
        Parameters["MOTAOMDuration"] = 500;

        // v0 Light Intensity
        Parameters["v0IntensityRampStartTime"] = 5000;
        Parameters["v0IntensityRampDuration"] = 2000;
        Parameters["v0IntensityRampStartValue"] = 5.6;
        Parameters["v0IntensityMolassesValue"] = 5.6;

        // v0 Light Frequency
        Parameters["v0FrequencyStartValue"] = 10.0; //9.0

        // triggering delay (10V = 1 second)
       // Parameters["triggerDelay"] = 5.0;

        // v0 F=1 (dodgy code using an analogue output to control a TTL)
        Parameters["v0F1AOMStartValue"] = 5.0;
        Parameters["v0F1AOMOffValue"] = 0.0;

       
        
    }

    public override PatternBuilder32 GetDigitalPattern()
    {
        PatternBuilder32 p = new PatternBuilder32();
        int patternStartBeforeQ = (int)Parameters["TCLBlockStart"];

        //MOTMasterScriptSnippet lm = new LoadMoleculeMOT(p, Parameters);  // This is how you load "preset" patterns.          
                
        //p.Pulse(patternStartBeforeQ, (int)Parameters["Frame0Trigger"], (int)Parameters["Frame0TriggerDuration"], "cameraTrigger"); //camera trigger for first frame
        //p.Pulse(patternStartBeforeQ, 100, (int)Parameters["Frame0TriggerDuration"], "rbAbsImgCamTrig");

        //p.Pulse(patternStartBeforeQ + (int)Parameters["MOTCoilsSwitchOff"] + 10000, (int)Parameters["Frame0Trigger"], (int)Parameters["Frame0TriggerDuration"], "cameraTrigger"); //camera trigger for first frame
        
        //p.Pulse(patternStartBeforeQ, (int)Parameters["CameraTriggerTransverseTime"], (int)Parameters["Frame0TriggerDuration"], "rbAbsImgCamTrig"); //camera trigger for first frame
        //p.Pulse(patternStartBeforeQ, 400, (int)Parameters["Frame0TriggerDuration"], "cameraTrigger");
        //p.AddEdge("dipoleTrapAOM", 0, false);
        p.AddEdge("aoPatternTrigger", 0, true);
        p.AddEdge("aoPatternTrigger", 100, false);
        //p.AddEdge("aoPatternTrigger", 20, false);

        p.AddEdge("dipoleTrapAOM", 0, false);
        p.AddEdge("dipoleTrapAOM", 10000, true);
        p.AddEdge("dipoleTrapAOM", 10100, false);

        p.AddEdge("test00", 0, false);
        p.AddEdge("test00", 10000, true);
        p.AddEdge("test00", 10002, false);
        p.AddEdge("test00", 10003, true);
        p.AddEdge("test00", 10004, false);

        p.AddEdge("test01", 0, false);
        p.AddEdge("test01", 9995, true);
        p.AddEdge("test01", 10000, false);
        //p.AddEdge("test00", 40, false);
        //p.AddEdge("rb2DMOTShutter", 0, false);

        return p;
    }

    public override AnalogPatternBuilder GetAnalogPattern()
    {
        AnalogPatternBuilder p = new AnalogPatternBuilder((int)Parameters["PatternLength"]);

        MOTMasterScriptSnippet lm = new LoadMoleculeMOT(p, Parameters);

        // Add Analog Channels

        //p.AddChannel("lightSwitch");

        //p.AddAnalogValue("lightSwitch", 0, 0.0);
        //p.AddAnalogValue("lightSwitch", 10000, 1.0);
        //p.AddAnalogValue("lightSwitch", 10100, 0.0);

        return p;
   }

}
