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
        Parameters["MOTCoilsCurrentValue"] = 1.0;//1.0; // 0.65;

        // Shim fields
        //Parameters["xShimLoadCurrent"] = 0.0;// -1.35 is zero
        //Parameters["yShimLoadCurrent"] = 10.0;// -1.92 is zero
        //Parameters["zShimLoadCurrent"] = 1.5;// -0.56,   -0.22 is zero

        Parameters["xShimLoadCurrent"] = -1.90;
        Parameters["yShimLoadCurrent"] = -1.92;//-5.0;// -1.92 is zero
        Parameters["zShimLoadCurrent"] = -0.55;//-2.0;// -0.56,   -0.22 is zero

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


        Parameters["RbOffsetLockSetPoint"] = 0.88;
        Parameters["RbRepumpOffsetLockSetPoint"] = 1.55;

       
        Parameters["DipoleTrapHoldTime"] = 6000;
        Parameters["RbD1VCOFrequency"] = 1.5; //96 MHz
        Parameters["RbD1VCOFrequencyRamp"] = 2.0;
        Parameters["RbD1Attenuation"] = 10.0;
        Parameters["D1MolassesDuration"] = 15000;
        Parameters["D1MolassesDurationRamp"] = 200;
        Parameters["EvolutionInDipoleTrap"] = 10000;

        Parameters["FrequencySweepDwellTime"] = 50000;

    }

    public override PatternBuilder32 GetDigitalPattern()
    {
        PatternBuilder32 p = new PatternBuilder32();
        int patternStartBeforeQ = (int)Parameters["TCLBlockStart"];
        int rbMOTLoadTime = patternStartBeforeQ + (int)Parameters["MOTLoadTime"];
        int rbDARKMOTStartTime = rbMOTLoadTime + (int)Parameters["MOTHoldTime"];
        int rbDARKMOTEndTime = rbDARKMOTStartTime + (int)Parameters["DarkMOTDuration"];
        int d1MolassesStartTime = rbDARKMOTEndTime + (int)Parameters["DipoleTrapHoldTime"];
        int d1MolassesDuration = (int)Parameters["D1MolassesDuration"] + (int)Parameters["D1MolassesDurationRamp"];
        int frequencySwitchTriggerTime = d1MolassesStartTime + (int)Parameters["D1MolassesDuration"] - (int)Parameters["FrequencySweepDwellTime"];
        int dipoleTrapEndTime = rbDARKMOTEndTime + (int)Parameters["DipoleTrapHoldTime"] + d1MolassesDuration + (int)Parameters["EvolutionInDipoleTrap"];
        int freeExpansionEndTime = dipoleTrapEndTime + (int)Parameters["FreeExpansionTime"];
        int cameraTrigger1 = freeExpansionEndTime;
        int cameraTrigger2 = cameraTrigger1 + (int)Parameters["CameraTriggerDelayAfterFirstImage"]; //probe image
        int cameraTrigger3 = cameraTrigger2 + (int)Parameters["CameraTriggerDelayAfterFirstImage"]; //bg

        MOTMasterScriptSnippet lm = new LoadMoleculeMOT(p, Parameters);  // This is how you load "preset" patterns.          

        //Rb:

        p.AddEdge("rb3DCooling", 0, false);
        p.AddEdge("rb3DCooling", rbDARKMOTEndTime, true);
        p.AddEdge("rb3DCooling", cameraTrigger1, false);
        p.AddEdge("rb3DCooling", cameraTrigger1 + 100, true);

        p.AddEdge("rb2DCooling", 0, false);
        p.AddEdge("rb2DCooling", rbMOTLoadTime, true);

        p.AddEdge("rbPushBeam", 0, false);
        p.AddEdge("rbPushBeam", rbMOTLoadTime - 200, true);

        p.AddEdge("rbRepump", 0, false);
        p.AddEdge("rbRepump", rbDARKMOTStartTime, true);
        p.AddEdge("rbRepump", cameraTrigger1, false);
        p.AddEdge("rbRepump", cameraTrigger1 + 100, true);

        //p.AddEdge("rbRepump", freeExpansionEndTime, false);
        //p.AddEdge("rbRepump", cameraTrigger1, true);

        p.AddEdge("rbOpticalPumpingAOM", 0, false);
        p.AddEdge("rbOpticalPumpingAOM", rbDARKMOTEndTime, true);



        p.AddEdge("rbAbsImagingBeam", 0, true); //Absorption imaging probe

        //p.AddEdge("rbAbsImagingBeam", rbDARKMOTEndTime, false);
        //p.AddEdge("rbAbsImagingBeam", rbDARKMOTEndTime + 5, true);

        //p.AddEdge("rbAbsImagingBeam", cameraTrigger1, false);
        //p.AddEdge("rbAbsImagingBeam", cameraTrigger1 + 5, true);
        //p.AddEdge("rbAbsImagingBeam", cameraTrigger2, false);
        //p.AddEdge("rbAbsImagingBeam", cameraTrigger2 + 5, true);

        // Abs image
        //p.Pulse(0, rbDARKMOTEndTime, (int)Parameters["Frame0TriggerDuration"], "rbAbsImgCamTrig");
        p.Pulse(0, cameraTrigger1, (int)Parameters["Frame0TriggerDuration"], "rbAbsImgCamTrig");
        p.Pulse(0, cameraTrigger2, (int)Parameters["Frame0TriggerDuration"], "rbAbsImgCamTrig");
        p.Pulse(0, cameraTrigger3, (int)Parameters["Frame0TriggerDuration"], "rbAbsImgCamTrig");
        // Fluorescence image
        p.Pulse(0, cameraTrigger1, (int)Parameters["Frame0TriggerDuration"], "cameraTrigger");
        //p.Pulse(0, rbDARKMOTEndTime - 100, 5, "cameraTrigger");
        //p.Pulse(0, cameraTrigger1, (int)Parameters["Frame0TriggerDuration"], "cameraTrigger");

        p.AddEdge("dipoleTrapAOM", 0, false);
        p.AddEdge("dipoleTrapAOM", rbDARKMOTStartTime, true);
        p.AddEdge("dipoleTrapAOM", dipoleTrapEndTime, false);

        p.AddEdge("rbD1CoolingSwitch", 0, true);


        p.AddEdge("rbD1CoolingSwitch", d1MolassesStartTime, false);
        p.AddEdge("rbD1CoolingSwitch", d1MolassesStartTime + (int)Parameters["D1MolassesDuration"] - 10, true);
        p.AddEdge("rbD1CoolingSwitch", d1MolassesStartTime + (int)Parameters["D1MolassesDuration"], false);
        p.AddEdge("rbD1CoolingSwitch", d1MolassesStartTime + d1MolassesDuration, true);


        p.Pulse(0, frequencySwitchTriggerTime, 10, "rbDDSFrequencySwitch");
        
        p.AddEdge("microwaveB", 0, false);

        p.AddEdge("rb3DCooling", (int)Parameters["PatternLength"] - (int)Parameters["TurnAllLightOn"], false);
        p.AddEdge("rb2DCooling", (int)Parameters["PatternLength"] - (int)Parameters["TurnAllLightOn"], false);
        p.AddEdge("rbRepump", (int)Parameters["PatternLength"] - (int)Parameters["TurnAllLightOn"], false);
        p.AddEdge("rbPushBeam", (int)Parameters["PatternLength"] - (int)Parameters["TurnAllLightOn"], false);

        return p;
    }

    public override AnalogPatternBuilder GetAnalogPattern()
    {
        AnalogPatternBuilder p = new AnalogPatternBuilder((int)Parameters["PatternLength"]);
        int rbMOTLoadTime = (int)Parameters["MOTLoadTime"];
        int rbDARKMOTStartTime = rbMOTLoadTime + (int)Parameters["MOTHoldTime"];
        int rbDARKMOTEndTime = rbDARKMOTStartTime + (int)Parameters["DarkMOTDuration"];
        int d1MolassesStartTime = rbDARKMOTEndTime + (int)Parameters["DipoleTrapHoldTime"];
        int d1MolassesDuration = (int)Parameters["D1MolassesDuration"] + (int)Parameters["D1MolassesDurationRamp"];
        int frequencySwitchTriggerTime = d1MolassesStartTime + (int)Parameters["D1MolassesDuration"] - (int)Parameters["FrequencySweepDwellTime"];
        int dipoleTrapEndTime = rbDARKMOTEndTime + (int)Parameters["DipoleTrapHoldTime"] + d1MolassesDuration + (int)Parameters["EvolutionInDipoleTrap"];
        int freeExpansionEndTime = dipoleTrapEndTime + (int)Parameters["FreeExpansionTime"];
        int cameraTrigger1 = freeExpansionEndTime;
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
        p.AddChannel("rbD1CoolingAttenuation");
        p.AddChannel("rbD1VCO");
        p.AddChannel("DipoleTrapLaserControl");

        // Shim Fields
        p.AddAnalogValue("xShimCoilCurrent", 0, (double)Parameters["xShimLoadCurrent"]);
        p.AddAnalogValue("yShimCoilCurrent", 0, (double)Parameters["yShimLoadCurrent"]);
        p.AddAnalogValue("zShimCoilCurrent", 0, (double)Parameters["zShimLoadCurrent"]);


        // B Field
        p.AddAnalogValue("MOTCoilsCurrent", 0, (double)Parameters["MOTCoilsCurrentValue"]); //switch on MOT coils to load Rb MOT
        p.AddAnalogValue("MOTCoilsCurrent", rbDARKMOTStartTime, (double)Parameters["DarkMOTField"]);
        p.AddAnalogValue("MOTCoilsCurrent", rbDARKMOTEndTime, -0.05); //switch off coils after MOT is loaded

        //Rb Laser detunings
        p.AddAnalogValue("rbAbsImagingFrequency", 0, (double)Parameters["ImagingFrequency"]);

        p.AddAnalogValue("rb3DCoolingFrequency", 0, (double)Parameters["MOTCoolingLoadingFrequency"]);

        p.AddAnalogValue("rbRepumpOffsetLock", 0, (double)Parameters["RbRepumpOffsetLockSetPoint"]);

        p.AddAnalogValue("rbOffsetLock", 0, (double)Parameters["RbOffsetLockSetPoint"]);

        p.AddAnalogValue("DipoleTrapLaserControl", 0, 10.0);

        p.AddAnalogValue("rbD1VCO", 0, (double)Parameters["RbD1VCOFrequency"]);
        //p.AddAnalogValue("rbD1VCO", d1MolassesStartTime + (int)Parameters["D1MolassesDuration"], (double)Parameters["RbD1VCOFrequencyRamp"]);
        //p.AddAnalogValue("rbD1VCO", (int)Parameters["PatternLength"] - (int)Parameters["TurnAllLightOn"], (double)Parameters["RbD1VCOFrequency"]);

        p.AddAnalogValue("rbD1CoolingAttenuation", 0, (double)Parameters["RbD1Attenuation"]);

        return p;
    }

}


