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
        Parameters["PatternLength"] = 500000;
        Parameters["RbMOTLoadTime"] = 400000;
        Parameters["TCLBlockStart"] = 4000; // This is a time before the Q switch
        Parameters["TCLBlockDuration"] = 15000;
        Parameters["FlashToQ"] = 16; // This is a time before the Q switch
        Parameters["QSwitchPulseDuration"] = 10;
        Parameters["FlashPulseDuration"] = 10;

        // Camera
        Parameters["RbCameraTrigger"] = 6000;
        Parameters["BgCameraTrigger"] = 6000;
        Parameters["MeasCameraTriggerDelay"] = 4000;
        Parameters["Frame0TriggerDuration"] = 10;

        //PMT
        Parameters["PMTTrigger"] = 8000;
        Parameters["PMTTriggerDuration"] = 10;

        // Slowing
        Parameters["slowingAOMOnStart"] = 250;
        Parameters["slowingAOMOnDuration"] = 45000;
        Parameters["slowingAOMOffStart"] = 1500;
        Parameters["slowingAOMOffDuration"] = 35000;
        Parameters["slowingRepumpAOMOnStart"] = 0;
        Parameters["slowingRepumpAOMOnDuration"] = 45000;
        Parameters["slowingRepumpAOMOffStart"] = 1700;
        Parameters["slowingRepumpAOMOffDuration"] = 35000;

        // Slowing Chirp
        Parameters["SlowingChirpStartTime"] = 340;
        Parameters["SlowingChirpDuration"] = 1160;
        Parameters["SlowingChirpStartValue"] = 0.0;
        Parameters["SlowingChirpEndValue"] = -1.3;

        // Slowing field
        Parameters["slowingCoilsValue"] = 1.05;
        Parameters["slowingCoilsOffTime"] = 1500;

        // B Field
        Parameters["MOTCoilsSwitchOn"] = 0;
        Parameters["MOTCoilsSwitchOff"] = 40000;
        Parameters["MOTCoilsCurrentRbLoadValue"] = 0.32;
        Parameters["MOTCoilsCurrentCaFLoadValue"] = 0.65;
        Parameters["MOTCoilsCurrentCompressValue"] = 1.3;
        Parameters["MOTCoilsCurrentRampDuration"] = 5000;

        // Shim fields
        Parameters["xShimLoadCurrent"] = 0.0; //1.7
        Parameters["yShimLoadCurrent"] = 0.0;
        Parameters["zShimLoadCurrent"] = -6.82; //-6.82

        // v0 Light Intensity
        Parameters["v0IntensityRampDuration"] = 400;
        Parameters["v0IntensityRampStartValue"] = 5.8;
        Parameters["v0IntensityRampEndValue"] = 8.465;

        // v0 Light Frequency
        Parameters["v0FrequencyStartValue"] = 9.0;

        // triggering delay (10V = 1 second)
        // Parameters["triggerDelay"] = 5.0;

        // v0 F=1 (dodgy code using an analogue output to control a TTL)
        Parameters["v0F1AOMStartValue"] = 5.0;
        Parameters["v0F1AOMOffValue"] = 0.0;


    }

    public override PatternBuilder32 GetDigitalPattern()
    {
        PatternBuilder32 p = new PatternBuilder32();
        p.Pulse(0, 0, 10, "aoPatternTrigger");  //THIS TRIGGERS THE ANALOG PATTERN. The analog pattern will start at the same time as the Q-switch is fired.

        int patternStartBeforeQ = (int)Parameters["TCLBlockStart"] + (int)Parameters["RbMOTLoadTime"];

        p.Pulse(patternStartBeforeQ, -(int)Parameters["TCLBlockStart"], (int)Parameters["TCLBlockDuration"], "tclBlock");

        p.Pulse(patternStartBeforeQ, 0, (int)Parameters["RbCameraTrigger"], "rbCoolingAOM");

        p.Pulse(patternStartBeforeQ, -(int)Parameters["FlashToQ"], (int)Parameters["QSwitchPulseDuration"], "flashLamp"); //trigger the flashlamp
        p.Pulse(patternStartBeforeQ, 0, (int)Parameters["QSwitchPulseDuration"], "qSwitch"); //trigger the Q switch

        p.Pulse(patternStartBeforeQ, (int)Parameters["slowingAOMOnStart"], (int)Parameters["slowingAOMOffStart"] - (int)Parameters["slowingAOMOnStart"], "bXSlowingAOM"); //first pulse to slowing AOM
        p.AddEdge("bXSlowingAOM", patternStartBeforeQ + (int)Parameters["slowingAOMOffStart"] + (int)Parameters["slowingAOMOffDuration"], true); // send slowing aom high and hold it high

        p.Pulse(patternStartBeforeQ, (int)Parameters["slowingRepumpAOMOnStart"], (int)Parameters["slowingRepumpAOMOffStart"] - (int)Parameters["slowingRepumpAOMOnStart"], "v10SlowingAOM"); //first pulse to slowing repump AOM
        p.AddEdge("v10SlowingAOM", patternStartBeforeQ + (int)Parameters["slowingRepumpAOMOffStart"] + (int)Parameters["slowingRepumpAOMOffDuration"], true); // send slowing repump aom high and hold it high

        p.Pulse(patternStartBeforeQ, (int)Parameters["BgCameraTrigger"] + 1000 + (int)Parameters["v0IntensityRampDuration"], (int)Parameters["MeasCameraTriggerDelay"] - (1000 + (int)Parameters["v0IntensityRampDuration"]), "v00MOTAOM");

        p.Pulse(patternStartBeforeQ, (int)Parameters["RbCameraTrigger"], (int)Parameters["Frame0TriggerDuration"], "cameraTrigger2"); //camera trigger for first frame
        p.Pulse(patternStartBeforeQ, (int)Parameters["BgCameraTrigger"], (int)Parameters["Frame0TriggerDuration"], "cameraTrigger"); 
        p.Pulse(patternStartBeforeQ, (int)Parameters["BgCameraTrigger"] + (int)Parameters["MeasCameraTriggerDelay"], (int)Parameters["Frame0TriggerDuration"], "cameraTrigger");

        return p;
    }

    public override AnalogPatternBuilder GetAnalogPattern()
    {
        AnalogPatternBuilder p = new AnalogPatternBuilder((int)Parameters["PatternLength"]);

        int patternStartBeforeQ = (int)Parameters["TCLBlockStart"] + (int)Parameters["RbMOTLoadTime"];

        // Add Analog Channels

        p.AddChannel("v00Intensity");
        p.AddChannel("v00Frequency");
        p.AddChannel("xShimCoilCurrent");
        p.AddChannel("yShimCoilCurrent");
        p.AddChannel("zShimCoilCurrent");
        p.AddChannel("v00EOMAmp");
        p.AddChannel("slowingChirp");
        p.AddChannel("slowingCoilsCurrent");
        p.AddChannel("MOTCoilsCurrent");

        // Slowing Chirp
        p.AddAnalogValue("slowingChirp", 0, (double)Parameters["SlowingChirpStartValue"]);
        p.AddLinearRamp("slowingChirp", patternStartBeforeQ + (int)Parameters["SlowingChirpStartTime"], (int)Parameters["SlowingChirpDuration"], (double)Parameters["SlowingChirpEndValue"]);
        p.AddLinearRamp("slowingChirp", patternStartBeforeQ + (int)Parameters["SlowingChirpStartTime"] + (int)Parameters["SlowingChirpDuration"] + 200, (int)Parameters["SlowingChirpDuration"], (double)Parameters["SlowingChirpStartValue"]);

        // Slowing field
        p.AddAnalogValue("slowingCoilsCurrent", 0, (double)Parameters["slowingCoilsValue"]);
        p.AddAnalogValue("slowingCoilsCurrent", patternStartBeforeQ + (int)Parameters["slowingCoilsOffTime"], 0.0);

        // B Field
        p.AddAnalogValue("MOTCoilsCurrent", 0, (double)Parameters["MOTCoilsCurrentRbLoadValue"]);
        p.AddLinearRamp("MOTCoilsCurrent", patternStartBeforeQ - (int)Parameters["MOTCoilsCurrentRampDuration"], (int)Parameters["MOTCoilsCurrentRampDuration"], (double)Parameters["MOTCoilsCurrentCaFLoadValue"]);
        //p.AddLinearRamp("MOTCoilsCurrent", patternStartBeforeQ + (int)Parameters["BgCameraTrigger"] + 1000, (int)Parameters["MOTCoilsCurrentRampDuration"], (double)Parameters["MOTCoilsCurrentCompressValue"]);
        p.AddAnalogValue("MOTCoilsCurrent", patternStartBeforeQ + (int)Parameters["MOTCoilsSwitchOff"], 0.0);

        // Shim Fields
        p.AddAnalogValue("xShimCoilCurrent", 0, (double)Parameters["xShimLoadCurrent"]);
        p.AddAnalogValue("yShimCoilCurrent", 0, (double)Parameters["yShimLoadCurrent"]);
        p.AddAnalogValue("zShimCoilCurrent", 0, (double)Parameters["zShimLoadCurrent"]);

        // trigger delay
        // p.AddAnalogValue("triggerDelay", 0, (double)Parameters["triggerDelay"]);

        // F=0
        p.AddAnalogValue("v00EOMAmp", 0, 5.7);

        // v0 Intensity Ramp
        p.AddAnalogValue("v00Intensity", 0, (double)Parameters["v0IntensityRampStartValue"]);
        p.AddLinearRamp("v00Intensity", patternStartBeforeQ + (int)Parameters["BgCameraTrigger"] + 1000, (int)Parameters["v0IntensityRampDuration"], (double)Parameters["v0IntensityRampEndValue"]);
        p.AddAnalogValue("v00Intensity", patternStartBeforeQ + (int)Parameters["BgCameraTrigger"] + (int)Parameters["MeasCameraTriggerDelay"], (double)Parameters["v0IntensityRampStartValue"]);

        // v0 Frequency Ramp
        p.AddAnalogValue("v00Frequency", 0, (double)Parameters["v0FrequencyStartValue"]);

        return p;
    }

}
