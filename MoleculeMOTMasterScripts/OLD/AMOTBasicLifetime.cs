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
        Parameters["Frame0Trigger"] = 5000;
        Parameters["Frame0TriggerDuration"] = 1000;
        Parameters["CameraTriggerTransverseTime"] = 120;
        Parameters["FrameTriggerInterval"] = 1100;
        Parameters["waitbeforeimage"] = 1600;

        //PMT
        Parameters["PMTTriggerDuration"] = 10;

        // Slowing
        //Parameters["slowingAOMOnStart"] = 180 + (int)Parameters["SlowingDelayTime"]; //180
        Parameters["slowingAOMOnStart"] = 200;//160
        Parameters["PushStart"] = 200;
        Parameters["PushEnd"] = 400;
        Parameters["slowingAOMOnDuration"] = 45000;

        Parameters["slowingAOMOffStart"] = 1800;//started from 1520
        //Parameters["slowingAOMOffStart"] = 1600;
        //Parameters["slowingAOMOffStart"] = 1000;
        Parameters["slowingAOMOffDuration"] = 40000;// 60000;


        
        Parameters["slowingRepumpAOMOnStart"] = 0;//started from 0
        Parameters["slowingRepumpAOMOffStart"] = 1800;// 1760;//1520
        //Parameters["slowingRepumpAOMOffStart"] = 1600;//1520
        Parameters["slowingRepumpAOMOffDuration"] = 35000;//60000;


        // Slowing Chirp
        Parameters["SlowingChirpStartTime"] = 400;//360; //400;// 380;
        //Parameters["SlowingChirpStartTime"] = 400;
        //Parameters["SlowingChirpStartTime"] = 100;
        Parameters["SlowingChirpDuration"] = 1400;////1400;//1160; //1160
        //Parameters["SlowingChirpDuration"] = 1200;
        //Parameters["SlowingChirpDuration"] = 1000;
        Parameters["SlowingChirpStartValue"] = 0.0;//0.0
        Parameters["SlowingChirpEndValue"] = -1.25;//-1.25; //-1.25 //225MHz/V 120m/s/V

        // Slowing field
        Parameters["slowingCoilsValue"] = 0.4; //1.05;
        Parameters["slowingCoilsOffTime"] = 2000; // 1500;

        // B Field
        Parameters["MOTCoilsSwitchOn"] = 0;
        Parameters["MOTCoilsSwitchOff"] = 20000;
        Parameters["MOTCoilsCurrentValue"] = 1.0; // 0.65;

        // Shim fields
        Parameters["xShimLoadCurrent"] = -1.35;//3.6
        Parameters["yShimLoadCurrent"] = -1.92;//-0.12
        Parameters["zShimLoadCurrent"] = -0.22;//-5.35


        // v0 Light Switch
        Parameters["MOTAOMStartTime"] = 15000;
        Parameters["MOTAOMDuration"] = 500;

        // v0 Light Intensity
        Parameters["v0IntensityRampStartTime"] = 4000;
        Parameters["v0IntensityRampDuration"] = 400;
        Parameters["v0IntensityRampStartValue"] = 7.2; //5.6
        Parameters["v0IntensityEndValue"] = 7.2;//7.8
        Parameters["v0IntensityMolassesValue"] = 5.6;
        Parameters["v0IntensityRampBackTime"] = 20000;

        Parameters["V00EOMsidebandRatio"] = 4.0;
        Parameters["V00AOMSidebandAmplitude"] = 1.0;

        // v0 Light Frequency
        Parameters["v0FrequencyStartValue"] = 10.0; //9.0

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


        MOTMasterScriptSnippet lm = new LoadMoleculeMOT(p, Parameters);  // This is how you load "preset" patterns.

        
        for (int i = 0; i < 12; i++)
        {
            p.Pulse(patternStartBeforeQ, (int)Parameters["Frame0Trigger"] + i * 3000 , (int)Parameters["Frame0TriggerDuration"], "cameraTrigger");
        }
        
        // p.Pulse(patternStartBeforeQ, (int)Parameters["Frame0Trigger"], (int)Parameters["Frame0TriggerDuration"], "cameraTrigger"); //camera trigger for first frame
        //p.Pulse(patternStartBeforeQ, (int)Parameters["Frame0Trigger"], (int)Parameters["Frame0TriggerDuration"], "cameraTrigger"); //camera trigger for first frame
        //p.Pulse(patternStartBeforeQ, (int)Parameters["v0IntensityRampStartTime"] + (int)Parameters["v0IntensityRampDuration"] + (int)Parameters["waitbeforeimage"], (int)Parameters["Frame0TriggerDuration"], "cameraTrigger"); //camera trigger for first frame p.Pulse(patternStartBeforeQ, 2000, 10, "tofTrigger");

        //p.Pulse(patternStartBeforeQ, (int)Parameters["Frame0Trigger"] + (int)Parameters["Frame0Trigger"], (int)Parameters["FrameTriggerInterval"], "cameraTrigger"); //camera trigger for first frame
        //p.Pulse(patternStartBeforeQ, 4000, (int)Parameters["Frame0TriggerDuration"], "rbAbsImgCamTrig");

        //p.Pulse(patternStartBeforeQ, (int)Parameters["CameraTriggerTransverseTime"], (int)Parameters["Frame0TriggerDuration"], "rbAbsImgCamTrig"); //camera trigger for first frame
        //p.Pulse(patternStartBeforeQ, 400, (int)Parameters["Frame0TriggerDuration"], "cameraTrigger");

        //p.Pulse(patternStartBeforeQ, 0, (int)Parameters["Frame0TriggerDuration"], "test01");//New pattern card test channel
        //p.AddEdge("dipoleTrapAOM", 4000, true);

        p.AddEdge("rb2DMOTShutter", 0, true);
        p.AddEdge("rb2DMOTShutter", 5000, false);

        p.AddEdge("cafOptPumpingAOM", 0, true); // false for switch off
        p.AddEdge("cafOptPumpingShutter", 0, true); // true for switch off

        p.AddEdge("TweezerChamberRbMOTAOMs", 1000, true);
        p.AddEdge("TweezerChamberRbMOTAOMs", 10000, false);

        //p.AddEdge("cafPushSwitch", 0, false);
        p.AddEdge("cafPushSwitch", 200, true);
        p.AddEdge("cafPushSwitch", 400, false);

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
        p.AddChannel("v00EOMAmp");
        p.AddChannel("v00Chirp");
        p.AddChannel("lightSwitch");
        p.AddChannel("TCoolSidebandVCO");
        p.AddChannel("v0AOMSidebandAmp");

        p.AddAnalogValue("lightSwitch", 0, 0.0);
        //p.AddAnalogValue("lightSwitch", 1000, 2.0);

        p.AddAnalogValue("TCoolSidebandVCO", 0, 5.1); //5.1V, 63.9MHz
        //p.AddAnalogValue("v0AOMSidebandAmp", 0, (double)Parameters["V00AOMSidebandAmplitude"]);
        //p.AddAnalogValue("TCoolSidebandVCO", 0, 4.5); //63.9MHz
        // Slowing field
        p.AddAnalogValue("slowingCoilsCurrent", 0, (double)Parameters["slowingCoilsValue"]);
        p.AddAnalogValue("slowingCoilsCurrent", (int)Parameters["slowingCoilsOffTime"], 0.0);

        // B Field
        p.AddAnalogValue("MOTCoilsCurrent", 0, (double)Parameters["MOTCoilsCurrentValue"]);
        p.AddAnalogValue("MOTCoilsCurrent", (int)Parameters["MOTCoilsSwitchOff"], 0.0);


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
        //p.AddAnalogValue("v00Intensity", 3900, 7.2);
        //p.AddLinearRamp("v00Intensity", (int)Parameters["v0IntensityRampStartTime"], (int)Parameters["v0IntensityRampDuration"], (double)Parameters["v0IntensityEndValue"]);
        //p.AddAnalogValue("v00Intensity", (int)Parameters["v0IntensityRampStartTime"] + (int)Parameters["v0IntensityRampDuration"] + (int)Parameters["waitbeforeimage"], (double)Parameters["v0IntensityRampStartValue"]);

        // v0 Frequency Ramp
        p.AddAnalogValue("v00Frequency", 0, (double)Parameters["v0FrequencyStartValue"]);
        //p.AddAnalogValue("v00Frequency", 4000, 9.0);

        //v0 chirp
        p.AddAnalogValue("v00Chirp", 0, 0.0);

        //p.AddAnalogValue("transferCoils", 0, 1.0);

        //p.AddAnalogValue("MOTCoilsCurrent", 0, (double)Parameters["Dummy"]);

        return p;
    }

}
