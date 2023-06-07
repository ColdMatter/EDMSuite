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
        Parameters["PatternLength"] = 170000;
        Parameters["TCLBlockStart"] = 4000; // This is a time before the Q switch
        Parameters["TCLBlockDuration"] = 15000;
        Parameters["FlashToQ"] = 16; // This is a time before the Q switch
        Parameters["QSwitchPulseDuration"] = 10;
        Parameters["FlashPulseDuration"] = 10;
        Parameters["HeliumShutterToQ"] = 100;
        Parameters["HeliumShutterDuration"] = 1550;

        Parameters["PushBeamFrequency"] = 5.0;
        Parameters["UVLightShutOff"] = 0;

        Parameters["MOTHoldTime"] = 0;
        Parameters["TurnAllLightOn"] = 1000;


        // Camera
        Parameters["MOTLoadTime"] = 100000;
        Parameters["CameraTriggerDelayAfterFirstImage"] = 15000;
        Parameters["Frame0TriggerDuration"] = 5;
        Parameters["TriggerJitter"] = 3;
        Parameters["OPDuration"] = 0;
        Parameters["ImageDelay"] = 0;


        //Rb light


        Parameters["ImagingFrequency"] = 2.91;
        Parameters["ProbePumpTime"] = 50; //This is for investigating the time it takes atoms to reach the strectched state when taking an absorption image
        //Parameters["MOTCoolingLoadingFrequency"] = 3.4; //  13/03/2023
        //Parameters["MOTCoolingLoadingFrequency"] = 4.0;
        Parameters["MOTCoolingLoadingFrequency"] = 4.6; //13/03/2023
        //Parameters["MOTCoolingLoadingFrequency"] = 4.60; //  06/03/2023
        //Parameters["MOTCoolingLoadingFrequency"] = 5.0;// it was 5.0 @ 27.04.2022
        //Parameters["MOTCoolingLoadingFrequency"] = 4.3;

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
        /*        Parameters["xShimLoadCurrent"] = -1.0;// -1.35 is zero
                Parameters["yShimLoadCurrent"] = -1.0;// -1.92 is zero
                Parameters["zShimLoadCurrent"] = -0.0;// -0.56,   -0.22 is zero */
        Parameters["xShimLoadCurrent"] = -1.35;// -1.35 is zero
        Parameters["yShimLoadCurrent"] = -1.92;// -1.92 is zero
        Parameters["zShimLoadCurrent"] = -0.3;// -0.56,   -0.22 is zero

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
        Parameters["Dummy"] = 0.0;

        Parameters["FreeExpTime"] = 100;
        Parameters["image2DMOTTime"] = 100;
        Parameters["RbRepumpSwitch"] = 0.0; // 0.0 will keep it on and 10.0 will switch it off

        Parameters["CoolingAttenuation"] = 0.0;
        Parameters["RepumpAttenuation"] = 0.0;
        Parameters["RbOffsetLockSetPoint"] = 0.925;//1.23;// 0.87;
        Parameters["RbRepumpOffsetLockSetPoint"] = 1.55;

    }

    public override PatternBuilder32 GetDigitalPattern()
    {
        PatternBuilder32 p = new PatternBuilder32();
        int patternStartBeforeQ = (int)Parameters["TCLBlockStart"];
        int rbMOTLoadTime = patternStartBeforeQ + (int)Parameters["MOTLoadTime"];
        int rbMOTSwitchOffTime = rbMOTLoadTime + (int)Parameters["MOTHoldTime"];
        int cameraTrigger1 = rbMOTSwitchOffTime + (int)Parameters["FreeExpTime"];
        int cameraTrigger2 = cameraTrigger1 + (int)Parameters["CameraTriggerDelayAfterFirstImage"]; //probe image
        int cameraTrigger3 = cameraTrigger2 + (int)Parameters["CameraTriggerDelayAfterFirstImage"]; //bg

        MOTMasterScriptSnippet lm = new LoadMoleculeMOT(p, Parameters);  // This is how you load "preset" patterns.          
        //   p.AddEdge("v00Shutter", 0, true);
        //p.Pulse(patternStartBeforeQ, 3000 - 1400, 10000, "bXSlowingShutter"); //Takes 14ms to start closing

        //p.Pulse(patternStartBeforeQ, (int)Parameters["Frame0Trigger"], (int)Parameters["Frame0TriggerDuration"], "cameraTrigger"); //camera trigger for first frame
        //p.AddEdge("rbAbsImagingBeam", 0, false);

        //Rb:

        p.AddEdge("rb3DCooling", 0, false);
        p.AddEdge("rb3DCooling", rbMOTSwitchOffTime, true);
        p.AddEdge("rb2DCooling", 0, false);
        p.AddEdge("rb2DCooling", rbMOTLoadTime, true);
        p.AddEdge("rbPushBeam", 0, false);
        p.AddEdge("rbPushBeam", rbMOTLoadTime - 200, true);
        p.AddEdge("rbD1CoolingSwitch", 0, true);
        p.AddEdge("rbRepump", 0, false);
        p.AddEdge("rbRepump", rbMOTSwitchOffTime, true);

        //p.AddEdge("UVFlashSwitch", 0, true);
        p.AddEdge("UVFlashSwitch", 0, false);
        //p.AddEdge("UVFlashSwitch", rbMOTLoadTime - (int)Parameters["UVLightShutOff"], true);


        //Turn everything back on at end of sequence:

        p.AddEdge("rb3DCooling", (int)Parameters["PatternLength"] - (int)Parameters["TurnAllLightOn"], false);
        p.AddEdge("rb2DCooling", (int)Parameters["PatternLength"] - (int)Parameters["TurnAllLightOn"], false);
        p.AddEdge("rbPushBeam", (int)Parameters["PatternLength"] - (int)Parameters["TurnAllLightOn"], false);
        p.AddEdge("UVFlashSwitch", (int)Parameters["PatternLength"] - (int)Parameters["TurnAllLightOn"], true);

        p.AddEdge("rbAbsImagingBeam", 0, true); //Absorption imaging probe

        p.AddEdge("rbAbsImagingBeam", cameraTrigger1, false);
        p.AddEdge("rbAbsImagingBeam", cameraTrigger1 + 10, true);
        p.AddEdge("rbAbsImagingBeam", cameraTrigger2, false);
        p.AddEdge("rbAbsImagingBeam", cameraTrigger2 + 10, true);

        // Abs image
        //p.Pulse(0, 94000, (int)Parameters["Frame0TriggerDuration"], "rbAbsImgCamTrig");
        p.Pulse(0, cameraTrigger1, (int)Parameters["Frame0TriggerDuration"], "rbAbsImgCamTrig");
        p.Pulse(0, cameraTrigger2, (int)Parameters["Frame0TriggerDuration"], "rbAbsImgCamTrig"); //trigger camera to take image of probe
        p.Pulse(0, cameraTrigger3, (int)Parameters["Frame0TriggerDuration"], "rbAbsImgCamTrig"); //trigger camera to take image of background rbAbsImgCamTrig

        p.Pulse(0, cameraTrigger1 - 2000, (int)Parameters["Frame0TriggerDuration"], "cameraTrigger");

        p.AddEdge("rb3DMOTShutter", 0, true);
        p.AddEdge("rbOPShutter", 0, true);
        p.AddEdge("rb2DMOTShutter", 0, true);

        p.AddEdge("rbOpticalPumpingAOM", 0, true);

        p.AddEdge("microwaveB", 0, false);

        return p;
    }

    public override AnalogPatternBuilder GetAnalogPattern()
    {
        AnalogPatternBuilder p = new AnalogPatternBuilder((int)Parameters["PatternLength"]);
        int rbMOTLoadTime = (int)Parameters["MOTLoadTime"];
        int rbMOTSwitchOffTime = rbMOTLoadTime + (int)Parameters["MOTHoldTime"];
        int cameraTrigger1 = rbMOTSwitchOffTime + (int)Parameters["FreeExpTime"];
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
        
        p.AddChannel("rbAbsImagingFrequency");
        p.AddChannel("rbRepumpOffsetLock");
        p.AddChannel("rbOffsetLock");

        
        // Shim Fields
        p.AddAnalogValue("xShimCoilCurrent", 0, (double)Parameters["xShimLoadCurrent"]);
        p.AddAnalogValue("yShimCoilCurrent", 0, (double)Parameters["yShimLoadCurrent"]);
        p.AddAnalogValue("zShimCoilCurrent", 0, (double)Parameters["zShimLoadCurrent"]);

        // B Field
        p.AddAnalogValue("MOTCoilsCurrent", 0, (double)Parameters["MOTCoilsCurrentValue"]); //switch on MOT coils to load Rb MOT
        p.AddAnalogValue("MOTCoilsCurrent", rbMOTSwitchOffTime, -0.05); //switch off coils after MOT is loaded

        //Rb Laser intensities
        p.AddAnalogValue("rb3DCoolingAttenuation", 0, (double)Parameters["CoolingAttenuation"]);


        //Rb Laser detunings
        p.AddAnalogValue("rb3DCoolingFrequency", 0, (double)Parameters["MOTCoolingLoadingFrequency"]);
        p.AddAnalogValue("rbAbsImagingFrequency", 0, (double)Parameters["ImagingFrequency"]);

        p.AddAnalogValue("rbRepumpOffsetLock", 0, (double)Parameters["RbRepumpOffsetLockSetPoint"]);

        p.AddAnalogValue("rbOffsetLock", 0, (double)Parameters["RbOffsetLockSetPoint"]);
        
        return p;
    }

}


