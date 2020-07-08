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
        Parameters["PatternLength"] = 70000;
        Parameters["TCLBlockStart"] = 4000; // This is a time before the Q switch
        Parameters["TCLBlockDuration"] = 15000;
        Parameters["FlashToQ"] = 16; // This is a time before the Q switch
        Parameters["QSwitchPulseDuration"] = 10;
        Parameters["FlashPulseDuration"] = 10;
        Parameters["HeliumShutterToQ"] = 100;
        Parameters["HeliumShutterDuration"] = 1550;

        Parameters["PushBeamFrequency"] = 5.0;


        Parameters["MOTHoldTime"] = 100;
        Parameters["TurnAllLightOn"] = 1000;


        // Camera
        Parameters["MOT3DLoadTime"] = 5000;
        Parameters["MOT2DLoadTime"] = 1000;
        Parameters["CameraTriggerDelayAfterFirstImage"] = 8000;
        Parameters["Frame0TriggerDuration"] = 15;
        Parameters["TriggerJitter"] = 3;
        Parameters["WaitBeforeImage"] = 250;
        Parameters["FreeExpansionTime"] = 0;


        //Rb light
        Parameters["ImagingFrequency"] = 1.7; //2.6 new resonance
        Parameters["ProbePumpTime"] = 0; //This is for investigating the time it takes atoms to reach the strectched state when taking an absorption image
        Parameters["MOTCoolingLoadingFrequency"] = 4.4;//5.4 usewd to be
        Parameters["MOTRepumpLoadingFrequency"] = 6.6; //6.9



        //PMT
        Parameters["PMTTrigger"] = 5000;
        Parameters["PMTTriggerDuration"] = 10;

        // Slowing
        Parameters["slowingAOMOnStart"] = 250; //started from 250
        Parameters["slowingAOMOnDuration"] = 45000;
        Parameters["slowingAOMOffStart"] = 1500;//started from 1500
        Parameters["slowingAOMOffDuration"] = 40000;
        Parameters["slowingRepumpAOMOnStart"] = 0;//started from 0
        Parameters["slowingRepumpAOMOnDuration"] = 45000;
        Parameters["slowingRepumpAOMOffStart"] = 1520;
        Parameters["slowingRepumpAOMOffDuration"] = 35000;

        // Slowing Chirp
        Parameters["SlowingChirpStartTime"] = 340;// 340;
        Parameters["SlowingChirpDuration"] = 1160;
        Parameters["SlowingChirpStartValue"] = 0.0;
        Parameters["SlowingChirpEndValue"] = -1.25;

        // Slowing field
        Parameters["slowingCoilsValue"] = 8.0; //1.05;
        Parameters["slowingCoilsOffTime"] = 1500;

        // B Field
        Parameters["MOTCoilsSwitchOn"] = 0;
        Parameters["MOTCoilsSwitchOff"] = 100000;
        Parameters["MOTCoilsCurrentValue"] = 1.0;//1.0; // 0.65;

        // Shim fields
        Parameters["xShimLoadCurrent"] = 3.6;//3.6
        Parameters["yShimLoadCurrent"] = 0.0;//-0.12
        Parameters["zShimLoadCurrent"] = 0.0;//-5.35


        // v0 Light Switch
        Parameters["MOTAOMStartTime"] = 15000;
        Parameters["MOTAOMDuration"] = 500;

        // v0 Light Intensity
        Parameters["v0IntensityRampStartTime"] = 5000;
        Parameters["v0IntensityRampDuration"] = 2000;
        Parameters["v0IntensityRampStartValue"] = 5.8;
        Parameters["v0IntensityMolassesValue"] = 5.8;

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
        int patternStartBeforeQ = (int)Parameters["TCLBlockStart"];
        int rb2DMOTLoadStartTime = patternStartBeforeQ;
        int rb2DMOTLoadEndTime = patternStartBeforeQ + (int)Parameters["MOT2DLoadTime"];
        int rb3DMOTTurnOnTime = rb2DMOTLoadEndTime;
        int rb3DMOTTurnOffTime = rb3DMOTTurnOnTime + (int)Parameters["MOT3DLoadTime"];
        int cameraTrigger1 = rb3DMOTTurnOffTime + (int)Parameters["WaitBeforeImage"];
        int cameraTrigger2 = cameraTrigger1 + (int)Parameters["CameraTriggerDelayAfterFirstImage"]; //probe image
        int cameraTrigger3 = cameraTrigger2 + (int)Parameters["CameraTriggerDelayAfterFirstImage"]; //bg

        MOTMasterScriptSnippet lm = new LoadMoleculeMOT(p, Parameters);  // This is how you load "preset" patterns.          
        //   p.AddEdge("v00Shutter", 0, true);
        //p.Pulse(patternStartBeforeQ, 3000 - 1400, 10000, "bXSlowingShutter"); //Takes 14ms to start closing

        //p.Pulse(patternStartBeforeQ, (int)Parameters["Frame0Trigger"], (int)Parameters["Frame0TriggerDuration"], "cameraTrigger"); //camera trigger for first frame
        //p.AddEdge("rbAbsImagingBeam", 0, false);

        //Rb:
        p.AddEdge("rb2DCooling", rb2DMOTLoadStartTime, false);
        p.AddEdge("rb2DCooling", rb2DMOTLoadEndTime, true);

        p.AddEdge("rb3DCooling", rb3DMOTTurnOnTime, false);
        p.AddEdge("rb3DCooling", rb3DMOTTurnOffTime, true);

        //p.AddEdge("rbPushBeam", rb2DMOTLoadStartTime, false);
        //p.AddEdge("rbPushBeam", rb2DMOTLoadEndTime, true);

        p.AddEdge("rbRepump", 0, false);
        //p.AddEdge("rbRepump", cameraTrigger3, true);



        //Turn everything back on at end of sequence:
        /*
        p.AddEdge("rb3DCooling", (int)Parameters["PatternLength"] - (int)Parameters["TurnAllLightOn"], false);
        p.AddEdge("rb2DCooling", (int)Parameters["PatternLength"] - (int)Parameters["TurnAllLightOn"], false);
        p.AddEdge("rbPushBeam", (int)Parameters["PatternLength"] - (int)Parameters["TurnAllLightOn"], false);
        */

        //p.AddEdge("rb3DCooling", 0, true);

        p.AddEdge("rbAbsImagingBeam", 0, true); //Absorption imaging probe

        p.AddEdge("rbAbsImagingBeam", cameraTrigger1 - (int)Parameters["ProbePumpTime"], false);
        p.AddEdge("rbAbsImagingBeam", cameraTrigger1 + 15, true);
        p.AddEdge("rbAbsImagingBeam", cameraTrigger2, false);
        p.AddEdge("rbAbsImagingBeam", cameraTrigger2 + 15, true);

        // Abs image
        p.Pulse(0, cameraTrigger1, (int)Parameters["Frame0TriggerDuration"], "rbAbsImgCamTrig");
        //p.Pulse(0, cameraTrigger2, (int)Parameters["Frame0TriggerDuration"], "rbAbsImgCamTrig"); //trigger camera to take image of probe
        //p.Pulse(0, cameraTrigger3, (int)Parameters["Frame0TriggerDuration"], "rbAbsImgCamTrig"); //trigger camera to take image of background
        //p.Pulse(0, rbMOTLoadTime, (int)Parameters["Frame0TriggerDuration"], "cameraTrigger");

        //CaF camera trigger:
        //p.Pulse(0, rbMOTLoadTime - 100, (int)Parameters["Frame0TriggerDuration"], "cameraTrigger"); //camera trigger for first frame
        //p.Pulse(0, rbMOTLoadTime - 100, (int)Parameters["Frame0TriggerDuration"], "cameraTrigger"); //trigger camera to take image of cloud
        //p.Pulse(0, rbMOTLoadTime - 200, (int)Parameters["Frame0TriggerDuration"], "rbAbsImgCamTrig");
        //p.Pulse(0, cameraTrigger1-200, (int)Parameters["Frame0TriggerDuration"], "cameraTrigger");
        //p.Pulse(0, cameraTrigger1, (int)Parameters["Frame0TriggerDuration"], "cameraTrigger");


        p.AddEdge("rb2DMOTShutter", 0, false);
        return p;
    }

    public override AnalogPatternBuilder GetAnalogPattern()
    {
        AnalogPatternBuilder p = new AnalogPatternBuilder((int)Parameters["PatternLength"]);
        int rb2DMOTLoadStartTime = 0;
        int rb2DMOTLoadEndTime = patternStartBeforeQ + (int)Parameters["MOT2DLoadTime"];
        int rb3DMOTTurnOnTime = rb2DMOTLoadEndTime;
        int rb3DMOTTurnOffTime = rb3DMOTTurnOnTime + (int)Parameters["MOT3DLoadTime"];
        int cameraTrigger1 = rb3DMOTTurnOffTime + (int)Parameters["WaitBeforeImage"];
        int cameraTrigger2 = cameraTrigger1 + (int)Parameters["CameraTriggerDelayAfterFirstImage"]; //probe image
        int cameraTrigger3 = cameraTrigger2 + (int)Parameters["CameraTriggerDelayAfterFirstImage"]; //bg

        MOTMasterScriptSnippet lm = new LoadMoleculeMOT(p, Parameters);

        // Add Analog Channels

        p.AddChannel("v00Intensity");
        p.AddChannel("v00Frequency");
        p.AddChannel("xShimCoilCurrent");
        p.AddChannel("yShimCoilCurrent");
        p.AddChannel("zShimCoilCurrent");
        p.AddChannel("v00EOMAmp");
        p.AddChannel("v00Chirp");

        // Add Rb Analog channels
        p.AddChannel("rb3DCoolingFrequency");
        p.AddChannel("rb3DCoolingAttenuation");
        p.AddChannel("rbRepumpFrequency");
        p.AddChannel("rbRepumpAttenuation");
        p.AddChannel("rbAbsImagingFrequency");


        // B Field
        p.AddAnalogValue("MOTCoilsCurrent", rb3DMOTTurnOnTime, (double)Parameters["MOTCoilsCurrentValue"]); //switch on MOT coils to load Rb MOT
        p.AddAnalogValue("MOTCoilsCurrent", rb3DMOTTurnOffTime, 0.0); //switch off coils after MOT is loaded

        // Shim Fields
        p.AddAnalogValue("xShimCoilCurrent", 0, (double)Parameters["xShimLoadCurrent"]);
        p.AddAnalogValue("yShimCoilCurrent", 0, (double)Parameters["yShimLoadCurrent"]);
        p.AddAnalogValue("zShimCoilCurrent", 0, (double)Parameters["zShimLoadCurrent"]);
        /*
        // trigger delay
        // p.AddAnalogValue("triggerDelay", 0, (double)Parameters["triggerDelay"]);

        // F=0
        p.AddAnalogValue("v00EOMAmp", 0, 5.2);

        // v0 Intensity Ramp
        p.AddAnalogValue("v00Intensity", 0, (double)Parameters["v0IntensityRampStartValue"]);

        // v0 Frequency Ramp
        p.AddAnalogValue("v00Frequency", 0, (double)Parameters["v0FrequencyStartValue"]);

        //v0 chirp
        p.AddAnalogValue("v00Chirp", 0, 0.0);
        */

        //Rb Laser intensities
        p.AddAnalogValue("rbRepumpAttenuation", 0, 0.0);
        p.AddAnalogValue("rb3DCoolingAttenuation", 0, 0.0);


        //Rb Laser detunings
        p.AddAnalogValue("rb3DCoolingFrequency", 0, (double)Parameters["MOTCoolingLoadingFrequency"]);
        p.AddAnalogValue("rbRepumpFrequency", 0, (double)Parameters["MOTRepumpLoadingFrequency"]);
        p.AddAnalogValue("rbAbsImagingFrequency", 0, (double)Parameters["ImagingFrequency"]);



        //CMOT detuning
        //p.AddAnalogValue("rb3DCoolingFrequency", (int)Parameters["MOTLoadTime"], (double)Parameters["CMOTFrequency"]);
        return p;
    }

}


