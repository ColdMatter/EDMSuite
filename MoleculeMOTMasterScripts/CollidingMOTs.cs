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
        Parameters["PatternLength"] = 60000; //300000

        Parameters["TCLBlockStart"] = 2000; // This is a time before the Q switch
        Parameters["TCLBlockDuration"] = 8000;
        Parameters["FlashToQ"] = 16; // This is a time before the Q switch
        Parameters["QSwitchPulseDuration"] = 10;
        Parameters["FlashPulseDuration"] = 10;
        Parameters["HeliumShutterToQ"] = 100;
        Parameters["HeliumShutterDuration"] = 1550;

        Parameters["RbMOTLoadTime"] = 1000;
        Parameters["FreeExpansionTime"] = 100;

        // Camera
        Parameters["Frame0Trigger"] = 4000;
        Parameters["Frame0TriggerDuration"] = 10;
        Parameters["TimeBetweenTriggers"] = 2300;
        Parameters["NoOfTriggers"] = 15;

        Parameters["loadingTime"] = 4000;

        //PMT
        Parameters["PMTTrigger"] = 5000;
        Parameters["PMTTriggerDuration"] = 10;

        // Slowing
        Parameters["slowingAOMOnStart"] = 280; //started from 250
        Parameters["slowingAOMOnDuration"] = 45000;
        Parameters["slowingAOMOffStart"] = 1520;//started from 1500
        Parameters["slowingAOMOffDuration"] = 40000;
        Parameters["slowingRepumpAOMOnStart"] = 0;//started from 0
        Parameters["slowingRepumpAOMOnDuration"] = 45000;
        Parameters["slowingRepumpAOMOffStart"] = 1520;//1520
        Parameters["slowingRepumpAOMOffDuration"] = 35000;

        // Slowing Chirp
        Parameters["SlowingChirpStartTime"] = 380;
        Parameters["SlowingChirpDuration"] = 1160;
        Parameters["SlowingChirpStartValue"] = 0.0;
        Parameters["SlowingChirpEndValue"] = -1.25;
        Parameters["SlowingChirpHoldDuration"] = 8000;

        // Slowing field
        Parameters["slowingCoilsValue"] = 0.42;
        Parameters["slowingCoilsOffTime"] = 1500;

        // BX poke
        Parameters["PokeDetuningValue"] = -1.37;//-1.37
        Parameters["PokeDuration"] = 300;

        // B Field
        Parameters["MOTCoilsSwitchOn"] = 0;
        Parameters["MOTCoilsCurrentValueRb"] = 0.65;
        Parameters["MOTCoilsCurrentValueCaF"] = 0.65;

        // Shim fields
        Parameters["xShimLoadCurrent"] = 0.0;//3.6
        Parameters["yShimLoadCurrent"] = 0.0;//-0.12
        Parameters["zShimLoadCurrent"] = 0.0;//-5.35

        //Shim fields for imaging
        Parameters["xShimImagingCurrent"] = -1.93;// -1.35 is zero
        Parameters["yShimImagingCurrent"] = -6.74;// -1.92 is zero
        Parameters["zShimImagingCurrent"] = -0.56;// -0.22 is zero


        // v0 Light Switch
        Parameters["MOTAOMStartTime"] = 15000;
        Parameters["MOTAOMDuration"] = 500;

        // v0 Light Intensity
        Parameters["v0IntensityRampStartTime"] = 5000;
        Parameters["v0IntensityRampDuration"] = 400;//400
        Parameters["v0IntensityRampStartValue"] = 5.6;
        Parameters["v0IntensityEndValue"] = 5.6; //7.78

        // v0 Light Frequency
        Parameters["v0FrequencyStartValue"] = 10.0;
        Parameters["v0FrequencyNewValue"] = 10.0;

        // triggering delay (10V = 1 second)
        // Parameters["triggerDelay"] = 5.0;

        // v0 F=1 (dodgy code using an analogue output to control a TTL)
        Parameters["v0F1AOMStartValue"] = 5.0;
        Parameters["v0F1AOMOffValue"] = 0.0;

        //Rb light
        Parameters["ImagingFrequency"] = 1.5; //2.1
        Parameters["MOTCoolingLoadingFrequency"] = 4.6; //4.6
        Parameters["MOTRepumpLoadingFrequency"] = 6.6; //6.9
        Parameters["RbRepumpSwitch"] = 0.0; // 0.0 will keep it on and 10.0 will switch it off
        Parameters["mwSwitch"] = 0.0; // greater than 5.0 will switch off the microwave

    }

    public override PatternBuilder32 GetDigitalPattern()
    {
        PatternBuilder32 p = new PatternBuilder32();

        int patternStartBeforeQ = (int)Parameters["TCLBlockStart"];
        int rbMOTLoadingStartTime = patternStartBeforeQ;
        int rbMOTLoadingEndTime = rbMOTLoadingStartTime + (int)Parameters["RbMOTLoadTime"];
        int moleculeMOTLoadingStartTime = rbMOTLoadingEndTime;
        int moleculeMOTLoadingEndTime = moleculeMOTLoadingStartTime + (int)Parameters["loadingTime"];
        int firstImageTime = moleculeMOTLoadingEndTime + (int)Parameters["v0IntensityRampDuration"];
        int lastImageTime = firstImageTime + (int)Parameters["TimeBetweenTriggers"] * (int)Parameters["NoOfTriggers"];


        for (int t = 0; t < (int)Parameters["RbMOTLoadTime"]; t += 50000)
        {
            p.Pulse(patternStartBeforeQ + t, -(int)Parameters["FlashToQ"], (int)Parameters["QSwitchPulseDuration"], "flashLamp"); //trigger the flashlamp
            p.Pulse(patternStartBeforeQ + t, 0, (int)Parameters["QSwitchPulseDuration"], "qSwitch"); //trigger the Q switch
        }
        

        MOTMasterScriptSnippet lm = new LoadMoleculeMOTNoSlowingEdge(p, Parameters);
        
        for (int t = firstImageTime; t < lastImageTime; t += (int)Parameters["TimeBetweenTriggers"])
        {
            p.Pulse(0, t, (int)Parameters["Frame0TriggerDuration"], "cameraTrigger");
        }
        
        // Use only incase of allignment
        // p.Pulse(0, lastImageTime, (int)Parameters["Frame0TriggerDuration"], "cameraTrigger");


        //Rb:

        p.AddEdge("rb3DCooling", 0, false);
        p.AddEdge("rb3DCooling", lastImageTime + 1000, true);
        p.AddEdge("rb2DCooling", 0, false);
        p.AddEdge("rb2DCooling", rbMOTLoadingEndTime, true);
        p.AddEdge("rbPushBeam", 0, false);
        p.AddEdge("rbPushBeam", rbMOTLoadingEndTime, true);


        //Turn everything back on at end of sequence:

        //p.Pulse(0, firstImageTime, (int)Parameters["Frame0TriggerDuration"], "cameraTrigger"); //camera trigger for first frame


        p.AddEdge("rbAbsImagingBeam", 0, true); //Absorption imaging probe

        p.AddEdge("rbAbsImagingBeam", lastImageTime + 1100, false);
        p.AddEdge("rbAbsImagingBeam", lastImageTime + 1100 + 15, true);
        p.AddEdge("rbAbsImagingBeam", lastImageTime + 12200, false);
        p.AddEdge("rbAbsImagingBeam", lastImageTime + 12200 + 15, true);


        p.Pulse(0, lastImageTime + 1100, (int)Parameters["Frame0TriggerDuration"], "rbAbsImgCamTrig"); //trigger camera to take image of cloud
        p.Pulse(0, lastImageTime + 12200, (int)Parameters["Frame0TriggerDuration"], "rbAbsImgCamTrig"); //trigger camera to take image of probe
        p.Pulse(0, lastImageTime + 21200, (int)Parameters["Frame0TriggerDuration"], "rbAbsImgCamTrig"); //trigger camera to take image of background

        //p.AddEdge("rb3DCooling", (int)Parameters["PatternLength"] - (int)Parameters["TurnAllLightOn"], false);
        //p.AddEdge("rb2DCooling", (int)Parameters["PatternLength"] - (int)Parameters["TurnAllLightOn"], false);
        //p.AddEdge("rbPushBeam", (int)Parameters["PatternLength"] - (int)Parameters["TurnAllLightOn"], false);

        p.AddEdge("rb3DMOTShutter", 0, true);
        p.AddEdge("rbPushBamAbsorptionShutter", 0, false);
        p.AddEdge("rbOPShutter", 0, true);

        //if ((double)Parameters["mwSwitch"] < 5.0)
        //{
        //p.Pulse(0, firstImageTime, lastImageTime - firstImageTime, "microwaveA");
        //}

        return p;
    }

    public override AnalogPatternBuilder GetAnalogPattern()
    {
        AnalogPatternBuilder p = new AnalogPatternBuilder((int)Parameters["PatternLength"]);

        MOTMasterScriptSnippet lm = new LoadMoleculeMOTNoSlowingEdge(p, Parameters);

        int rbMOTLoadingStartTime = 0;
        int rbMOTLoadingEndTime = rbMOTLoadingStartTime + (int)Parameters["RbMOTLoadTime"];
        int moleculeMOTLoadingStartTime = rbMOTLoadingEndTime;
        int moleculeMOTLoadingEndTime = moleculeMOTLoadingStartTime + (int)Parameters["loadingTime"];
        int firstImageTime = moleculeMOTLoadingEndTime + (int)Parameters["v0IntensityRampDuration"] + 2500;
        int lastImageTime = firstImageTime + (int)Parameters["TimeBetweenTriggers"] * (int)Parameters["NoOfTriggers"];


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


        // Slowing field
        p.AddAnalogValue("slowingCoilsCurrent", moleculeMOTLoadingStartTime, (double)Parameters["slowingCoilsValue"]);
        p.AddAnalogValue("slowingCoilsCurrent", moleculeMOTLoadingStartTime + (int)Parameters["slowingCoilsOffTime"], 0.0);

        // B Field
        p.AddAnalogValue("MOTCoilsCurrent", rbMOTLoadingStartTime, (double)Parameters["MOTCoilsCurrentValueRb"]);
        p.AddAnalogValue("MOTCoilsCurrent", moleculeMOTLoadingStartTime, (double)Parameters["MOTCoilsCurrentValueCaF"]);
        p.AddAnalogValue("MOTCoilsCurrent", lastImageTime + 1000, 0.0);

        // Shim Fields
        p.AddAnalogValue("xShimCoilCurrent", 0, (double)Parameters["xShimLoadCurrent"]);
        p.AddAnalogValue("yShimCoilCurrent", 0, (double)Parameters["yShimLoadCurrent"]);
        p.AddAnalogValue("zShimCoilCurrent", 0, (double)Parameters["zShimLoadCurrent"]);

        //Shim fields for imaging
        p.AddAnalogValue("xShimCoilCurrent", lastImageTime, (double)Parameters["xShimImagingCurrent"]);
        p.AddAnalogValue("yShimCoilCurrent", lastImageTime, (double)Parameters["yShimImagingCurrent"]);
        p.AddAnalogValue("zShimCoilCurrent", lastImageTime, (double)Parameters["zShimImagingCurrent"]);

        // trigger delay
        // p.AddAnalogValue("triggerDelay", 0, (double)Parameters["triggerDelay"]);

        // F=0
        p.AddAnalogValue("v00EOMAmp", 0, 4.85);

        // v0 Intensity Ramp
        p.AddAnalogValue("v00Intensity", 0, (double)Parameters["v0IntensityRampStartValue"]);
        p.AddLinearRamp("v00Intensity", moleculeMOTLoadingEndTime, (int)Parameters["v0IntensityRampDuration"], (double)Parameters["v0IntensityEndValue"]);

        // v0 Frequency Ramp
        p.AddAnalogValue("v00Frequency", 0, (double)Parameters["v0FrequencyStartValue"]);
        p.AddAnalogValue("v00Frequency", moleculeMOTLoadingEndTime, (double)Parameters["v0FrequencyNewValue"]);

        //v0 chirp
        p.AddAnalogValue("v00Chirp", 0, 0.0);

        //Rb Laser detunings
        p.AddAnalogValue("rb3DCoolingFrequency", 0, (double)Parameters["MOTCoolingLoadingFrequency"]);
        p.AddAnalogValue("rbRepumpFrequency", 0, (double)Parameters["MOTRepumpLoadingFrequency"]);
        p.AddAnalogValue("rbAbsImagingFrequency", 0, (double)Parameters["ImagingFrequency"]);


        p.AddAnalogValue("rbRepumpAttenuation", 0, (double)Parameters["RbRepumpSwitch"]);
        p.AddAnalogValue("rb3DCoolingAttenuation", 0, 0.0);

        return p;
    }

}
