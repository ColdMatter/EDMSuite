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
        Parameters["PatternLength"] = 250000;
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
        Parameters["MOTLoadTime"] = 100000;
        Parameters["CameraTriggerDelayAfterFirstImage"] = 8000;
        Parameters["Frame0TriggerDuration"] = 15;
        Parameters["NormalMOTHoldTime"] = 100;
        Parameters["TriggerJitter"] = 3;
        Parameters["WaitBeforeImage"] = 100;
        Parameters["FreeExpansionTime"] = 0;


        //Rb light
        Parameters["ImagingFrequency"] = 1.0; //2.58 resonance
        Parameters["MOTCoolingLoadingFrequency"] = 4.6;//5.4 usewd to be
        Parameters["MOTRepumpLoadingFrequency"] = 6.6; //6.9
        Parameters["DARKMOTRepumpFrequency"] = 6.5; //6.9
        Parameters["DARKMOTCoolingFrequency"] = 4.4; //6.9
        Parameters["RbCoolingFrequencyRampDuration"] = 1;

        //Rb intenisty ramp down:
        Parameters["RbCoolingIntensityEndValue"] = 0.0;
        Parameters["RbCoolingIntensityRampDuration"] = 0;

        //DARK SPOT MOT:
        //Parameters["DARKMOTDuration"] = 1200;
        Parameters["DARKMOTDuration"] = 1200;


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
        Parameters["MOTCoilsCurrentValue"] = 1.0;//1.0; // 0.65;

        // Shim fields
        // Shim fields update 16/03/2023
        Parameters["xShimImagingCurrent"] = -1.00;
        Parameters["yShimImagingCurrent"] = -1.92;
        Parameters["zShimImagingCurrent"] = -0.60;
        /*
        Parameters["xShimImagingCurrent"] = -1.93;// -1.35 is zero
        Parameters["yShimImagingCurrent"] = -6.74;// -1.92 is zero
        Parameters["zShimImagingCurrent"] = -0.56;// -0.22 is zero
        */

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

        Parameters["Dummy"] = 0.0;

    }

    public override PatternBuilder32 GetDigitalPattern()
    {
        PatternBuilder32 p = new PatternBuilder32();
        int rbMOTLoadTime = (int)Parameters["TCLBlockStart"] + (int)Parameters["MOTLoadTime"];
        int rbDARKMOTStartTime = rbMOTLoadTime + (int)Parameters["NormalMOTHoldTime"];
        int rbDARKMOTEndTime = rbDARKMOTStartTime + (int)Parameters["DARKMOTDuration"];
        int cameraTrigger1 = rbDARKMOTEndTime + (int)Parameters["WaitBeforeImage"];
        int cameraTrigger2 = cameraTrigger1 + (int)Parameters["CameraTriggerDelayAfterFirstImage"]; //probe image
        int cameraTrigger3 = cameraTrigger2 + (int)Parameters["CameraTriggerDelayAfterFirstImage"]; //bg

        MOTMasterScriptSnippet lm = new LoadMoleculeMOT(p, Parameters);  // This is how you load "preset" patterns.          
        //   p.AddEdge("v00Shutter", 0, true);
        //p.Pulse(patternStartBeforeQ, 3000 - 1400, 10000, "bXSlowingShutter"); //Takes 14ms to start closing

        //p.Pulse(patternStartBeforeQ, (int)Parameters["Frame0Trigger"], (int)Parameters["Frame0TriggerDuration"], "cameraTrigger"); //camera trigger for first frame
        //p.AddEdge("rbAbsImagingBeam", 0, false);

        //Rb:

        p.AddEdge("rb3DCooling", 0, false);
        p.AddEdge("rb3DCooling", rbDARKMOTEndTime, true);
        p.AddEdge("rb3DCooling", cameraTrigger1, false);
        p.AddEdge("rb2DCooling", 0, false);
        p.AddEdge("rb2DCooling", rbMOTLoadTime, true);
        p.AddEdge("rbPushBeam", 0, false);
        p.AddEdge("rbPushBeam", rbMOTLoadTime, true);
        p.AddEdge("rbD1CoolingSwitch", 0, true);

        p.AddEdge("rbRepump", 0, false);
        //p.AddEdge("rbRepump", rbDARKMOTEndTime, true);
        p.AddEdge("rbRepump", rbDARKMOTStartTime, true);

        p.AddEdge("rbOpticalPumpingAOM", 0, false); //Using thic channel for controlling DARK SPOT repump AOM
        //p.AddEdge("rbOpticalPumpingAOM", cameraTrigger1, true);

        //Turn everything back on at end of sequence:

        //p.AddEdge("rbRepump", (int)Parameters["PatternLength"] - (int)Parameters["TurnAllLightOn"], false);
        //p.AddEdge("rb3DCooling", (int)Parameters["PatternLength"] - (int)Parameters["TurnAllLightOn"], false);
        //p.AddEdge("rb2DCooling", (int)Parameters["PatternLength"] - (int)Parameters["TurnAllLightOn"], false);
        //p.AddEdge("rbPushBeam", (int)Parameters["PatternLength"] - (int)Parameters["TurnAllLightOn"], false);


        //p.AddEdge("rb3DCooling", 0, true);

        p.AddEdge("rbAbsImagingBeam", 0, true); //Absorption imaging probe

        p.AddEdge("rbAbsImagingBeam", cameraTrigger1, false);
        p.AddEdge("rbAbsImagingBeam", cameraTrigger1 + 15, true);
        p.AddEdge("rbAbsImagingBeam", cameraTrigger2, false);
        p.AddEdge("rbAbsImagingBeam", cameraTrigger2 + 15, true);

        // Abs image
        p.Pulse(0, cameraTrigger1, (int)Parameters["Frame0TriggerDuration"], "rbAbsImgCamTrig"); //trigger camera to take image of cloud
        p.Pulse(0, cameraTrigger2, (int)Parameters["Frame0TriggerDuration"], "rbAbsImgCamTrig"); //trigger camera to take image of probe
        p.Pulse(0, cameraTrigger3, (int)Parameters["Frame0TriggerDuration"], "rbAbsImgCamTrig"); //trigger camera to take image of background

        //CaF camera trigger:
        p.Pulse(0, cameraTrigger1, (int)Parameters["Frame0TriggerDuration"], "cameraTrigger"); //camera trigger for first frame


        p.AddEdge("rb3DMOTShutter", 0, true);
        return p;
    }

    public override AnalogPatternBuilder GetAnalogPattern()
    {
        AnalogPatternBuilder p = new AnalogPatternBuilder((int)Parameters["PatternLength"]);
        int rbMOTLoadTime = (int)Parameters["MOTLoadTime"];
        int rbDARKMOTStartTime = rbMOTLoadTime + (int)Parameters["NormalMOTHoldTime"];
        int rbDARKMOTEndTime = rbDARKMOTStartTime + (int)Parameters["DARKMOTDuration"];
        int cameraTrigger1 = rbDARKMOTEndTime + (int)Parameters["WaitBeforeImage"];
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
        //p.AddChannel("rbRepumpFrequency");
        p.AddChannel("rbRepumpAttenuation");
        p.AddChannel("rbAbsImagingFrequency");


        // B Field
        p.AddAnalogValue("MOTCoilsCurrent", 0, (double)Parameters["MOTCoilsCurrentValue"]); //switch on MOT coils to load Rb MOT
        p.AddAnalogValue("MOTCoilsCurrent", rbDARKMOTEndTime, 0.0); //switch off coils after MOT is loaded

        // Shim Fields
        p.AddAnalogValue("xShimCoilCurrent", 0, (double)Parameters["xShimImagingCurrent"]);
        p.AddAnalogValue("yShimCoilCurrent", 0, (double)Parameters["yShimImagingCurrent"]);
        p.AddAnalogValue("zShimCoilCurrent", 0, (double)Parameters["zShimImagingCurrent"]);

        //Rb Laser intensities
        p.AddAnalogValue("rb3DCoolingAttenuation", 0, 0.0);

        //Rb Laser detunings
        p.AddAnalogValue("rb3DCoolingFrequency", 0, (double)Parameters["MOTCoolingLoadingFrequency"]);
        //p.AddAnalogValue("rbRepumpFrequency", 0, (double)Parameters["MOTRepumpLoadingFrequency"]);
        p.AddAnalogValue("rbAbsImagingFrequency", 0, (double)Parameters["ImagingFrequency"]);

        return p;
    }

}


