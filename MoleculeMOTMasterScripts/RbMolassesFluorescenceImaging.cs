﻿using MOTMaster;
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
        Parameters["HeliumShutterToQ"] = 100;
        Parameters["HeliumShutterDuration"] = 1550;

        Parameters["PushBeamFrequency"] = 5.0;

        Parameters["FluorescenceImaginDetuning"] = 3.0;
        Parameters["ExposureTime"] = 15;


        Parameters["MOTHoldTime"] = 100;
        Parameters["TurnAllLightOn"] = 1000;


        // Camera
        Parameters["MOTLoadTime"] = 50000;
        Parameters["CameraTriggerDelayAfterFirstImage"] = 8000;
        Parameters["Frame0TriggerDuration"] = 15;
        Parameters["TriggerJitter"] = 3;
        Parameters["WaitBeforeImage"] = 0;
        Parameters["FreeExpansionTime"] = 2000;


        //Rb light


        Parameters["ImagingFrequency"] = 2.1; //2.6 new resonance
        Parameters["ProbePumpTime"] = 0; //This is for investigating the time it takes atoms to reach the strectched state when taking an absorption image
        Parameters["MOTCoolingLoadingFrequency"] = 4.6;//5.4 usewd to be
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
        Parameters["xShimLoadCurrent"] = -1.35;//3.6
        Parameters["yShimLoadCurrent"] = -1.94;//-0.12
        Parameters["zShimLoadCurrent"] = -0.22;//-5.35


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

        // Molasses parameters:
        Parameters["MolassesFrequnecyRampDuration"] = 1000;
        Parameters["MolassesHoldDuration"] = 1000;
        Parameters["MolassesEndFrequency"] = 0.5;


    }

    public override PatternBuilder32 GetDigitalPattern()
    {
        PatternBuilder32 p = new PatternBuilder32();
        int patternStartBeforeQ = (int)Parameters["TCLBlockStart"];
        int rbMOTLoadTime = patternStartBeforeQ + (int)Parameters["MOTLoadTime"];
        int rbMOTSwitchOffTime = rbMOTLoadTime + (int)Parameters["MOTHoldTime"];
        int molassesStartTime = rbMOTSwitchOffTime;
        int molassesEndTime = rbMOTSwitchOffTime + (int)Parameters["MolassesFrequnecyRampDuration"] + (int)Parameters["MolassesHoldDuration"];
        int cameraTrigger1 = molassesEndTime + (int)Parameters["FreeExpansionTime"];
        int cameraTrigger2 = cameraTrigger1 + (int)Parameters["CameraTriggerDelayAfterFirstImage"]; //probe image
        int cameraTrigger3 = cameraTrigger2 + (int)Parameters["CameraTriggerDelayAfterFirstImage"]; //bg

        MOTMasterScriptSnippet lm = new LoadMoleculeMOT(p, Parameters);  // This is how you load "preset" patterns.          
        //   p.AddEdge("v00Shutter", 0, true);
        //p.Pulse(patternStartBeforeQ, 3000 - 1400, 10000, "bXSlowingShutter"); //Takes 14ms to start closing

        //p.Pulse(patternStartBeforeQ, (int)Parameters["Frame0Trigger"], (int)Parameters["Frame0TriggerDuration"], "cameraTrigger"); //camera trigger for first frame
        //p.AddEdge("rbAbsImagingBeam", 0, false);

        //Rb:

        p.AddEdge("rb3DCooling", 0, false);
        p.AddEdge("rb3DCooling", molassesEndTime, true);
        p.AddEdge("rb3DCooling", cameraTrigger1, false);
        p.AddEdge("rb3DCooling", cameraTrigger1 + (int)Parameters["ExposureTime"], true);
        p.AddEdge("rb2DCooling", 0, false);
        p.AddEdge("rb2DCooling", rbMOTLoadTime, true);
        p.AddEdge("rbPushBeam", 0, false);
        p.AddEdge("rbPushBeam", rbMOTLoadTime - 200, true);

        p.AddEdge("rbRepump", 0, false);



        //Turn everything back on at end of sequence:
        /*
        p.AddEdge("rb3DCooling", (int)Parameters["PatternLength"] - 10, false);
        p.AddEdge("rb2DCooling", (int)Parameters["PatternLength"] - 10, false);
        p.AddEdge("rbPushBeam", (int)Parameters["PatternLength"] - 10, false);
        */
        p.AddEdge("rbAbsImagingBeam", 0, true);


        // Imaging
        p.Pulse(0, cameraTrigger1, (int)Parameters["Frame0TriggerDuration"], "cameraTrigger");


        p.AddEdge("rb2DMOTShutter", 0, false);
        p.AddEdge("rb3DMOTShutter", 0, true);
        p.AddEdge("rbOPShutter", 0, false);


        // test new digital pattern board
        //p.AddEdge("test00", 0, false);
        //p.AddEdge("test00", 100, true);


        return p;
    }

    public override AnalogPatternBuilder GetAnalogPattern()
    {
        AnalogPatternBuilder p = new AnalogPatternBuilder((int)Parameters["PatternLength"]);
        int rbMOTLoadTime = (int)Parameters["MOTLoadTime"];
        int rbMOTSwitchOffTime = rbMOTLoadTime + (int)Parameters["MOTHoldTime"];
        int molassesStartTime = rbMOTSwitchOffTime;
        int molassesEndTime = rbMOTSwitchOffTime + (int)Parameters["MolassesFrequnecyRampDuration"] + (int)Parameters["MolassesHoldDuration"];
        int cameraTrigger1 = molassesEndTime + (int)Parameters["FreeExpansionTime"];
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
        p.AddAnalogValue("MOTCoilsCurrent", 0, (double)Parameters["MOTCoilsCurrentValue"]); //switch on MOT coils to load Rb MOT
        p.AddAnalogValue("MOTCoilsCurrent", rbMOTSwitchOffTime, -0.05); //switch off coils after MOT is loaded

        // Shim Fields
        p.AddAnalogValue("xShimCoilCurrent", 0, (double)Parameters["xShimLoadCurrent"]);
        p.AddAnalogValue("yShimCoilCurrent", 0, (double)Parameters["yShimLoadCurrent"]);
        p.AddAnalogValue("zShimCoilCurrent", 0, (double)Parameters["zShimLoadCurrent"]);

        //Rb Laser intensities
        p.AddAnalogValue("rbRepumpAttenuation", 0, 0.0);
        p.AddAnalogValue("rb3DCoolingAttenuation", 0, 0.0);

        //Rb Laser detunings
        p.AddAnalogValue("rb3DCoolingFrequency", 0, (double)Parameters["MOTCoolingLoadingFrequency"]);
        p.AddAnalogValue("rbRepumpFrequency", 0, (double)Parameters["MOTRepumpLoadingFrequency"]);
        p.AddAnalogValue("rbAbsImagingFrequency", 0, (double)Parameters["ImagingFrequency"]);

        //MolassesDetuningRamp:
        p.AddLinearRamp("rb3DCoolingFrequency", molassesStartTime, (int)Parameters["MolassesFrequnecyRampDuration"], (double)Parameters["MolassesEndFrequency"]);

        p.AddAnalogValue("rb3DCoolingFrequency", molassesEndTime, (double)Parameters["FluorescenceImaginDetuning"]);
        return p;
    }

}


