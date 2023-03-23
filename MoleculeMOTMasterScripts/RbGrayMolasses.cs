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
        Parameters["PatternLength"] = 150000;
        Parameters["TCLBlockStart"] = 4000; // This is a time before the Q switch
        Parameters["TCLBlockDuration"] = 15000;
        Parameters["FlashToQ"] = 16; // This is a time before the Q switch
        Parameters["QSwitchPulseDuration"] = 10;
        Parameters["FlashPulseDuration"] = 10;
        Parameters["HeliumShutterToQ"] = 100;
        Parameters["HeliumShutterDuration"] = 1550;

        Parameters["PushBeamFrequency"] = 5.0;


        Parameters["MOTHoldTime"] = 0;
        Parameters["TurnAllLightOn"] = 1000;


        // Camera
        Parameters["MOTLoadTime"] = 100000;
        Parameters["CameraTriggerDelayAfterFirstImage"] = 8000;
        Parameters["Frame0TriggerDuration"] = 50;
        Parameters["TriggerJitter"] = 3;
        Parameters["OPDuration"] = 0;
        Parameters["FreeExpansionTime"] = 0;
        Parameters["ImageDelay"] = 0;


        //Rb light


        Parameters["ImagingFrequency"] = 2.7;
        Parameters["ProbePumpTime"] = 50; //This is for investigating the time it takes atoms to reach the strectched state when taking an absorption image
        Parameters["MOTCoolingLoadingFrequency"] = 5.0;// it was 5.0 @ 27.04.2022
        Parameters["MOTRepumpLoadingFrequency"] = 6.6; //6.6

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
        Parameters["xShimLoadCurrent"] = -2.5;
        Parameters["yShimLoadCurrent"] = -1.92;
        Parameters["zShimLoadCurrent"] = -0.3;

        //Shim fields for imaging
        Parameters["xShimImagingCurrent"] = -1.93;
        Parameters["yShimImagingCurrent"] = -6.74;
        Parameters["zShimImagingCurrent"] = -0.56;

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

        Parameters["FluorescenceImageDelay"] = 0;

        Parameters["Det"] = 4.9;

        Parameters["FreeExpTime"] = 1000;
        Parameters["image2DMOTTime"] = 100;
        Parameters["RbRepumpSwitch"] = 0.0; // 0.0 will keep it on and 10.0 will switch it off

        Parameters["GrayMolassesDelay"] = 300;
        Parameters["GrayMolassesDuration"] = 500;
        Parameters["NormalMolassesDuration"] = 1000;

        Parameters["FrequencyJumpValue"] = -0.7;
        Parameters["CoolingFrequencyJumpValue"] = 4.0;

    }

    public override PatternBuilder32 GetDigitalPattern()
    {
        PatternBuilder32 p = new PatternBuilder32();
        int patternStartBeforeQ = (int)Parameters["TCLBlockStart"];
        int rbMOTLoadTime = patternStartBeforeQ + (int)Parameters["MOTLoadTime"];
        int normalMolassesEndTime = rbMOTLoadTime + (int)Parameters["NormalMolassesDuration"];
        int grayMolassesStartTime = normalMolassesEndTime + (int)Parameters["GrayMolassesDelay"];
        int grayMolassesEndTime = grayMolassesStartTime + (int)Parameters["GrayMolassesDuration"];
        int cameraTrigger1 = grayMolassesEndTime + (int)Parameters["GrayMolassesDelay"] + (int)Parameters["FreeExpTime"];
        int cameraTrigger2 = cameraTrigger1 + (int)Parameters["CameraTriggerDelayAfterFirstImage"]; //probe image
        int cameraTrigger3 = cameraTrigger2 + (int)Parameters["CameraTriggerDelayAfterFirstImage"]; //bg

        MOTMasterScriptSnippet lm = new LoadMoleculeMOT(p, Parameters);  // This is how you load "preset" patterns.          
                                                                         //Rb:

        p.AddEdge("rb3DCooling", 0, false);
        p.AddEdge("rb3DCooling", normalMolassesEndTime, true);
        p.AddEdge("rb3DCooling", grayMolassesStartTime, false);
        p.AddEdge("rb3DCooling", grayMolassesEndTime, true);
        p.AddEdge("rb3DCooling", cameraTrigger1, false);
        p.AddEdge("rb2DCooling", 0, false);
        p.AddEdge("rb2DCooling", rbMOTLoadTime, true);
        p.AddEdge("rbPushBeam", 0, false);
        p.AddEdge("rbPushBeam", rbMOTLoadTime - 200, true);

        p.AddEdge("rbRepump", 0, false);
        p.AddEdge("rbRepump", normalMolassesEndTime - 500, true);
        p.AddEdge("rbRepump", grayMolassesStartTime, false);
        p.AddEdge("rbRepump", grayMolassesEndTime, true);
        p.AddEdge("rbRepump", cameraTrigger1, false);


        //Turn everything back on at end of sequence:

        //p.AddEdge("rb3DCooling", (int)Parameters["PatternLength"] - (int)Parameters["TurnAllLightOn"], false);
        //p.AddEdge("rb2DCooling", (int)Parameters["PatternLength"] - (int)Parameters["TurnAllLightOn"], false);
        //p.AddEdge("rbPushBeam", (int)Parameters["PatternLength"] - (int)Parameters["TurnAllLightOn"], false);


        p.AddEdge("rbAbsImagingBeam", 0, true); //Absorption imaging probe

        //p.AddEdge("rbAbsImagingBeam", cameraTrigger1, false);
        //p.AddEdge("rbAbsImagingBeam", cameraTrigger1 + 10, true);
        //p.AddEdge("rbAbsImagingBeam", cameraTrigger2, false);
        //p.AddEdge("rbAbsImagingBeam", cameraTrigger2 + 10, true);

        // Abs image
        //p.Pulse(0, cameraTrigger1, (int)Parameters["Frame0TriggerDuration"], "rbAbsImgCamTrig");

        p.Pulse(0, cameraTrigger1, (int)Parameters["Frame0TriggerDuration"], "cameraTrigger");

        p.AddEdge("rb3DMOTShutter", 0, true);
        p.AddEdge("rbOPShutter", 0, true);
        p.AddEdge("rb2DMOTShutter", 0, true);

        p.AddEdge("rbOpticalPumpingAOM", 0, true);


        return p;
    }

    public override AnalogPatternBuilder GetAnalogPattern()
    {
        AnalogPatternBuilder p = new AnalogPatternBuilder((int)Parameters["PatternLength"]);
        int rbMOTLoadTime = (int)Parameters["MOTLoadTime"];
        int normalMolassesEndTime = rbMOTLoadTime + (int)Parameters["NormalMolassesDuration"];
        int grayMolassesStartTime = normalMolassesEndTime + (int)Parameters["GrayMolassesDelay"];
        int grayMolassesEndTime = grayMolassesStartTime + (int)Parameters["GrayMolassesDuration"];
        int cameraTrigger1 = grayMolassesEndTime + (int)Parameters["GrayMolassesDelay"] + (int)Parameters["FreeExpTime"];
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
        //p.AddChannel("rbRepumpAttenuation");
        p.AddChannel("rbAbsImagingFrequency");
        p.AddChannel("rbRepumpOffsetLock");
        //p.AddChannel("rbRepumpPiezoOffsetVoltage");
        p.AddChannel("rbOffsetLock");

        // Shim Fields
        p.AddAnalogValue("xShimCoilCurrent", 0, (double)Parameters["xShimLoadCurrent"]);
        p.AddAnalogValue("yShimCoilCurrent", 0, (double)Parameters["yShimLoadCurrent"]);
        p.AddAnalogValue("zShimCoilCurrent", 0, (double)Parameters["zShimLoadCurrent"]);

        // B Field
        p.AddAnalogValue("MOTCoilsCurrent", 0, (double)Parameters["MOTCoilsCurrentValue"]); //switch on MOT coils to load Rb MOT
        p.AddAnalogValue("MOTCoilsCurrent", rbMOTLoadTime, -0.05); //switch off coils after MOT is loaded

        //Rb Laser intensities
        //p.AddAnalogValue("rbRepumpAttenuation", 0, (double)Parameters["RbRepumpSwitch"]);
        p.AddAnalogValue("rb3DCoolingAttenuation", 0, 0.0);


        //Rb Laser detunings
        p.AddAnalogValue("rb3DCoolingFrequency", 0, (double)Parameters["MOTCoolingLoadingFrequency"]);
        p.AddAnalogValue("rb3DCoolingFrequency", grayMolassesEndTime, 6.0);
        //p.AddAnalogValue("rbRepumpFrequency", 0, (double)Parameters["MOTRepumpLoadingFrequency"]);
        p.AddAnalogValue("rbAbsImagingFrequency", 0, (double)Parameters["ImagingFrequency"]);

        p.AddAnalogValue("rbRepumpOffsetLock", 0, 0.88);
        p.AddLinearRamp("rbRepumpOffsetLock", normalMolassesEndTime, (int)Parameters["GrayMolassesDelay"], 0.88 - (double)Parameters["FrequencyJumpValue"]);
        p.AddLinearRamp("rbRepumpOffsetLock", grayMolassesEndTime, (int)Parameters["GrayMolassesDelay"], 0.88);
        //p.AddAnalogValue("rbRepumpPiezoOffsetVoltage", 0, 0.0);
        //p.AddLinearRamp("rbRepumpPiezoOffsetVoltage", normalMolassesEndTime, (int)Parameters["GrayMolassesDelay"], -(double)Parameters["FrequencyJumpValue"] * 1.282);
        //p.AddLinearRamp("rbRepumpPiezoOffsetVoltage", grayMolassesEndTime, (int)Parameters["GrayMolassesDelay"], 0.0);

        p.AddAnalogValue("rbOffsetLock", 0, 0.88);
        p.AddAnalogValue("rbOffsetLock", normalMolassesEndTime, (double)Parameters["CoolingFrequencyJumpValue"]);
        p.AddAnalogValue("rbOffsetLock", grayMolassesEndTime, 0.88);

        return p;
    }

}


