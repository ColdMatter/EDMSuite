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
        Parameters["PatternLength"] = 200000;
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
        Parameters["CameraTriggerDelayAfterFirstImage"] = 15000;
        Parameters["Frame0TriggerDuration"] = 1;
        Parameters["TriggerJitter"] = 3;
        Parameters["OPDuration"] = 0;
        Parameters["FreeExpansionTime"] = 1;
        Parameters["ImageDelay"] = 0;


        //Rb light


        Parameters["ImagingFrequency"] = 3.0; 
        Parameters["ProbePumpTime"] = 50; //This is for investigating the time it takes atoms to reach the strectched state when taking an absorption image
        Parameters["MOTCoolingLoadingFrequency"] = 4.6; //13/03/2023
        Parameters["MOTRepumpLoadingFrequency"] = 6.6; //6.6
        Parameters["RepumpImagingFrequency"] = 6.6;

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
        Parameters["MOTCoilsCurrentValue"] = 0.5;//1.0; // 0.65;

        // Shim fields
        //Parameters["xShimLoadCurrent"] = 0.0;// -1.35 is zero
        //Parameters["yShimLoadCurrent"] = 10.0;// -1.92 is zero
        //Parameters["zShimLoadCurrent"] = 1.5;// -0.56,   -0.22 is zero

        Parameters["xShimLoadCurrent"] = -1.35//-10.0;// -1.35 is zero
        Parameters["yShimLoadCurrent"] = -1.92//-5.0;// -1.92 is zero
        Parameters["zShimLoadCurrent"] = -0.22//-2.0;// -0.56,   -0.22 is zero

        Parameters["xShimMolassesCurrent"] = -2.0;
        Parameters["yShimMolassesCurrent"] = -1.92;
        Parameters["zShimMolassesCurrent"] = -0.3;

        //Parameters["xShimLoadCurrent"] = -8.0;// -1.35 is zero
        //Parameters["yShimLoadCurrent"] = 5.0;// -1.92 is zero
        //Parameters["zShimLoadCurrent"] = -0.5;// -0.56,   -0.22 is zero

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

        Parameters["image2DMOTTime"] = 100;
        Parameters["RbRepumpSwitch"] = 0.0; // 0.0 will keep it on and 10.0 will switch it off

        Parameters["MolassesDuration"] = 5000;
        Parameters["MolassesFrequency"] = 3.0;
        Parameters["DipoleTrapHoldTime"] = 4000;
        Parameters["ODTDelay"] = 200;

        Parameters["DarkMOTDuration"] = 3000;
        Parameters["DarkMOTField"] = 0.5;
        Parameters["DarkMOTRepumpIntensityRampDuration"] = 5000;
        Parameters["ModulationPeriod"] = 16.6;

        Parameters["LatticeRampDuration"] = 1000;
        Parameters["DarkMOTCoolingDetuning"] = 2.5;

        Parameters["DipoleTrapRampDuration"] = 10000;
        Parameters["DipoleTrapRampEndIntensity"] = 2.0;
        Parameters["RampStartDelay"] = 600;

        Parameters["GrayMolassesDelay"] = 400;
        Parameters["GrayMolassesDuration"] = 40;

        Parameters["FrequencyJumpValue"] = -0.95;
        Parameters["CoolingLaserFrequencyJumpValue"] = 4.3; //4.5; 
        Parameters["GrayMolassesCoolingLightIntensity"] = 0.0;
        Parameters["GrayMolassesRepumpLightIntensity"] = 0.0;

        Parameters["ModulationDelay"] = 10;
        Parameters["ModulationDuration"] = 5000;

    }

    public override PatternBuilder32 GetDigitalPattern()
    {
        PatternBuilder32 p = new PatternBuilder32();
        int patternStartBeforeQ = (int)Parameters["TCLBlockStart"];
        int rbMOTLoadTime = patternStartBeforeQ + (int)Parameters["MOTLoadTime"];
        int rbDARKMOTStartTime = rbMOTLoadTime + (int)Parameters["MOTHoldTime"];
        int rbDARKMOTEndTime = rbDARKMOTStartTime + (int)Parameters["DarkMOTDuration"];
        int dipoleTrapEndTime = rbDARKMOTEndTime + (int)Parameters["DipoleTrapHoldTime"];
        int cameraTrigger1 = dipoleTrapEndTime + (int)Parameters["FreeExpansionTime"];
        int cameraTrigger2 = cameraTrigger1 + (int)Parameters["CameraTriggerDelayAfterFirstImage"]; //probe image
        int cameraTrigger3 = cameraTrigger2 + (int)Parameters["CameraTriggerDelayAfterFirstImage"]; //bg

        MOTMasterScriptSnippet lm = new LoadMoleculeMOT(p, Parameters);  // This is how you load "preset" patterns.          

        //Rb:
        
        p.AddEdge("rb3DCooling", 0, false);
        p.AddEdge("rb3DCooling", rbDARKMOTEndTime, true);
        //p.AddEdge("rb3DCooling", grayMolassesStartTime, false);
        //p.AddEdge("rb3DCooling", grayMolassesEndTime, true);
        p.AddEdge("rb3DCooling", dipoleTrapEndTime - 10, false);
        p.AddEdge("rb3DCooling", dipoleTrapEndTime, true);

        p.AddEdge("rb2DCooling", 0, false);
        p.AddEdge("rb2DCooling", rbMOTLoadTime, true);

        p.AddEdge("rbPushBeam", 0, false);
        p.AddEdge("rbPushBeam", rbMOTLoadTime - 200, true);

        p.AddEdge("rbRepump", 0, false);
        p.AddEdge("rbRepump", rbDARKMOTStartTime, true);
        //p.AddEdge("rbRepump", grayMolassesStartTime, false);
        //p.AddEdge("rbRepump", grayMolassesEndTime - 1, true);
        //p.AddEdge("rbRepump", cameraTrigger1, false);
        p.AddEdge("rbRepump", dipoleTrapEndTime - 10, false);
        p.AddEdge("rbRepump", dipoleTrapEndTime, true);

        p.AddEdge("rbOpticalPumpingAOM", 0, false);
        p.AddEdge("rbOpticalPumpingAOM", rbDARKMOTEndTime, true);


        p.AddEdge("rbAbsImagingBeam", 0, true); //Absorption imaging probe

        p.AddEdge("rbAbsImagingBeam", cameraTrigger1, false);
        p.AddEdge("rbAbsImagingBeam", cameraTrigger1 + 10, true);
        p.AddEdge("rbAbsImagingBeam", cameraTrigger2, false);
        p.AddEdge("rbAbsImagingBeam", cameraTrigger2 + 10, true);

        // Abs image
        p.Pulse(0, cameraTrigger1, (int)Parameters["Frame0TriggerDuration"], "rbAbsImgCamTrig");
        //p.Pulse(0, cameraTrigger2, (int)Parameters["Frame0TriggerDuration"], "rbAbsImgCamTrig");
        //p.Pulse(0, cameraTrigger3, (int)Parameters["Frame0TriggerDuration"], "rbAbsImgCamTrig");
        // Fluorescence image
        p.Pulse(0, dipoleTrapEndTime - 10, (int)Parameters["Frame0TriggerDuration"], "cameraTrigger");
        //p.Pulse(0, rbDARKMOTEndTime - 100, 5, "cameraTrigger");
        //p.Pulse(0, cameraTrigger1, (int)Parameters["Frame0TriggerDuration"], "cameraTrigger");

        p.AddEdge("dipoleTrapAOM", 0, false);
        p.AddEdge("dipoleTrapAOM", rbDARKMOTStartTime, true);
        p.AddEdge("dipoleTrapAOM", dipoleTrapEndTime, false);

        //p.AddEdge("rb3DMOTShutter", 0, true);
        //p.AddEdge("rbOPShutter", 0, true);
        //p.AddEdge("rb2DMOTShutter", 0, true);

        //modulation
        //p.AddEdge("cafOptPumpingAOM", 0, false);
        //p.AddEdge("cafOptPumpingAOM", rbDARKMOTEndTime + (int)Parameters["ModulationDelay"], true);
        //p.AddEdge("cafOptPumpingAOM", rbDARKMOTEndTime + (int)Parameters["ModulationDelay"] + (int)Parameters["ModulationDuration"], false);

        p.AddEdge("microwaveB", 0, false);
        
        return p;
    }

    public override AnalogPatternBuilder GetAnalogPattern()
    {
        AnalogPatternBuilder p = new AnalogPatternBuilder((int)Parameters["PatternLength"]);
        int rbMOTLoadTime = (int)Parameters["MOTLoadTime"];
        int rbDARKMOTStartTime = rbMOTLoadTime + (int)Parameters["MOTHoldTime"];
        int rbDARKMOTEndTime = rbDARKMOTStartTime + (int)Parameters["DarkMOTDuration"];
        int dipoleTrapEndTime = rbDARKMOTEndTime + (int)Parameters["DipoleTrapHoldTime"];
        int cameraTrigger1 = dipoleTrapEndTime + (int)Parameters["FreeExpansionTime"];
        int cameraTrigger2 = cameraTrigger1 + (int)Parameters["CameraTriggerDelayAfterFirstImage"]; //probe image
        int cameraTrigger3 = cameraTrigger2 + (int)Parameters["CameraTriggerDelayAfterFirstImage"]; //bg

        MOTMasterScriptSnippet lm = new LoadMoleculeMOT(p, Parameters);

        // Add Analog Channels
        
        p.AddChannel("xShimCoilCurrent");
        p.AddChannel("yShimCoilCurrent");
        p.AddChannel("zShimCoilCurrent");

        // Add Rb Analog channels
        p.AddChannel("rb3DCoolingFrequency");
        //p.AddChannel("rb3DCoolingAttenuation");
        p.AddChannel("rbAbsImagingFrequency");
        p.AddChannel("rbRepumpOffsetLock");
        p.AddChannel("rbOffsetLock");

        // Shim Fields
        p.AddAnalogValue("xShimCoilCurrent", 0, (double)Parameters["xShimLoadCurrent"]);
        p.AddAnalogValue("yShimCoilCurrent", 0, (double)Parameters["yShimLoadCurrent"]);
        p.AddAnalogValue("zShimCoilCurrent", 0, (double)Parameters["zShimLoadCurrent"]);

        p.AddAnalogValue("xShimCoilCurrent", rbDARKMOTEndTime, (double)Parameters["xShimMolassesCurrent"]);
        p.AddAnalogValue("yShimCoilCurrent", rbDARKMOTEndTime, (double)Parameters["yShimMolassesCurrent"]);
        p.AddAnalogValue("zShimCoilCurrent", rbDARKMOTEndTime, (double)Parameters["zShimMolassesCurrent"]);

        //p.AddAnalogValue("xShimCoilCurrent", cameraTrigger2, 0.0);
        //p.AddAnalogValue("yShimCoilCurrent", cameraTrigger2, 0.0);
        //p.AddAnalogValue("zShimCoilCurrent", cameraTrigger2, 0.0);

        // B Field
        p.AddAnalogValue("MOTCoilsCurrent", 0, (double)Parameters["MOTCoilsCurrentValue"]); //switch on MOT coils to load Rb MOT
        p.AddAnalogValue("MOTCoilsCurrent", rbDARKMOTStartTime, (double)Parameters["DarkMOTField"]);
        p.AddAnalogValue("MOTCoilsCurrent", rbDARKMOTEndTime, -0.05); //switch off coils after MOT is loaded

        //Rb Laser intensities
        //p.AddAnalogValue("rbRepumpAttenuation", 0, 0.0);
        //p.AddAnalogValue("rbRepumpAttenuation", rbDARKMOTEndTime, (double)Parameters["GrayMolassesRepumpLightIntensity"]);
        //p.AddAnalogValue("rbRepumpAttenuation", grayMolassesEndTime, 0.0);

        //p.AddAnalogValue("rb3DCoolingAttenuation", 0, 0.0);
        //p.AddAnalogValue("rb3DCoolingAttenuation", rbDARKMOTEndTime, (double)Parameters["GrayMolassesCoolingLightIntensity"]);
        //p.AddAnalogValue("rb3DCoolingAttenuation", grayMolassesEndTime, 0.0);



        //Rb Laser detunings
        p.AddAnalogValue("rbAbsImagingFrequency", 0, (double)Parameters["ImagingFrequency"]);

        p.AddAnalogValue("rb3DCoolingFrequency", 0, (double)Parameters["MOTCoolingLoadingFrequency"]);
        //p.AddAnalogValue("rb3DCoolingFrequency", rbDARKMOTStartTime, (double)Parameters["DarkMOTCoolingDetuning"]);
        //p.AddAnalogValue("rb3DCoolingFrequency", rbDARKMOTEndTime, (double)Parameters["MOTCoolingLoadingFrequency"]);
        //p.AddAnalogValue("rb3DCoolingFrequency", cameraTrigger1 - 100, 6.0);

        //p.AddAnalogValue("rbRepumpFrequency", 0, (double)Parameters["MOTRepumpLoadingFrequency"]);
        //p.AddAnalogValue("rbRepumpFrequency", cameraTrigger1 - 100, (double)Parameters["RepumpImagingFrequency"]);
        //p.AddAnalogValue("rbRepumpFrequency", cameraTrigger3, (double)Parameters["MOTRepumpLoadingFrequency"]);

        //Dipole trap intensity
        //p.AddAnalogValue("transferCoils", 0, 6.5);
        //p.AddAnalogValue("transferCoils", rbDARKMOTStartTime - 100, 2.0);
        //p.AddLinearRamp("transferCoils", dipoleTrapRampStartTime, (int)Parameters["DipoleTrapRampDuration"], 6.5);

        p.AddAnalogValue("rbRepumpOffsetLock", 0, 1.55);
        //p.AddLinearRamp("rbRepumpOffsetLock", rbDARKMOTEndTime, (int)Parameters["GrayMolassesDelay"], 0.88 - (double)Parameters["FrequencyJumpValue"]);
        //p.AddLinearRamp("rbRepumpOffsetLock", grayMolassesEndTime, (int)Parameters["GrayMolassesDelay"], 0.88);
        //p.AddAnalogValue("rbRepumpPiezoOffsetVoltage", 0, 0.0);
        //p.AddLinearRamp("rbRepumpPiezoOffsetVoltage", rbDARKMOTEndTime, (int)Parameters["GrayMolassesDelay"], -(double)Parameters["FrequencyJumpValue"] * 1.282);
        //p.AddLinearRamp("rbRepumpPiezoOffsetVoltage", grayMolassesEndTime, (int)Parameters["GrayMolassesDelay"], 0.0);

        p.AddAnalogValue("rbOffsetLock", 0, 0.925);
        //p.AddLinearRamp("rbOffsetLock", rbDARKMOTEndTime, (int)Parameters["GrayMolassesDelay"], (double)Parameters["CoolingLaserFrequencyJumpValue"]);
        //p.AddLinearRamp("rbOffsetLock", grayMolassesStartTime + 150, 350, 3.0);
        //p.AddLinearRamp("rbOffsetLock", grayMolassesEndTime, (int)Parameters["GrayMolassesDelay"], 0.78);
        
        return p;
    }

}


