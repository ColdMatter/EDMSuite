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

        Parameters["TurnAllLightOn"] = 1000;

        // Camera
        Parameters["MOTLoadTime"] = 50000;
        Parameters["Frame1TriggerDelay"] = 20000;
        Parameters["Frame2TriggerDelay"] = 20000;
        Parameters["Frame0TriggerDuration"] = 15;
        Parameters["TriggerJitter"] = 3;
        Parameters["WaitBeforeImage"] = 10;


        //Rb light
        Parameters["ImagingFrequency"] = 2.6; //2.1
        Parameters["MOTCoolingLoadingFrequency"] = 5.0;
        Parameters["MOTRepumpLoadingFrequency"] = 6.9; //6.9
        Parameters["CMOTDetuning"] = 1.5;


        //Rb molasses after CMOT:
        Parameters["MolassesDuration"] = 800;
        Parameters["MolassesDetuning"] = 5.4;
        Parameters["FreeExpansionTime"] = 1000;
        Parameters["MolassesLightIntensityRampDuration"] = 100;
        Parameters["MolassesCoolingLightEndValue"] = 0.0;
        Parameters["MolassesRepumpIntensity"] = 7.0;
        Parameters["MolassesFrequencyEndValue"] = 5.0;


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
        Parameters["MOTCoilsCurrentRampDuration"] = 10000;
        Parameters["CMOTCurrentValue"] = 1.0;

        // Shim fields
        Parameters["xShimLoadCurrent"] = 0.0;//3.6
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

        MOTMasterScriptSnippet lm = new LoadMoleculeMOT(p, Parameters);  // This is how you load "preset" patterns.          
        int firstCameraTrigger = (int)Parameters["MOTLoadTime"] + (int)Parameters["MOTCoilsCurrentRampDuration"] + (int)Parameters["MolassesDuration"] + (int)Parameters["WaitBeforeImage"] + (int)Parameters["FreeExpansionTime"] - (int)Parameters["TriggerJitter"];
        //int secondCameraTrigger = firstCameraTrigger + (int)Parameters["Frame1TriggerDelay"];
        //int thirdCameraTrigger = secondCameraTrigger + (int)Parameters["Frame2TriggerDelay"];

        //Rb:

        p.AddEdge("rb3DCooling", 0, false);
        p.AddEdge("rb3DCooling", (int)Parameters["MOTLoadTime"] + (int)Parameters["MOTCoilsCurrentRampDuration"] + (int)Parameters["MolassesDuration"], true);
        p.AddEdge("rb2DCooling", 0, false);
        p.AddEdge("rb2DCooling", (int)Parameters["MOTLoadTime"], true);
        p.AddEdge("rbPushBeam", 0, false);
        p.AddEdge("rbPushBeam", (int)Parameters["MOTLoadTime"], true);

        //p.AddEdge("rb3DCooling", 0, true);

        p.AddEdge("rbAbsImagingBeam", 0, true);
        p.AddEdge("rb3DCooling", firstCameraTrigger + (int)Parameters["TriggerJitter"], false);
        p.AddEdge("rb3DCooling", firstCameraTrigger + (int)Parameters["TriggerJitter"] + (int)Parameters["Frame0TriggerDuration"], true);

        // Abs image
        p.Pulse(0, firstCameraTrigger, (int)Parameters["Frame0TriggerDuration"], "cameraTrigger"); //1st camera frame


        //p.AddEdge("rbAbsImagingBeam", secondCameraTrigger + (int)Parameters["TriggerJitter"], false);
        //p.AddEdge("rbAbsImagingBeam", secondCameraTrigger + (int)Parameters["TriggerJitter"] + (int)Parameters["Frame0TriggerDuration"], true);

        // Rb probe
        //p.Pulse(0, secondCameraTrigger, (int)Parameters["Frame0TriggerDuration"], "rbAbsImgCamTrig"); //2nd camera frame
        // Rb Bg
        //p.Pulse(0, thirdCameraTrigger, (int)Parameters["Frame0TriggerDuration"], "rbAbsImgCamTrig"); //3rd camera frame


        //Turn everything back on at end of sequence:

        p.AddEdge("rb3DCooling", (int)Parameters["PatternLength"] - (int)Parameters["TurnAllLightOn"], false);
        p.AddEdge("rb2DCooling", (int)Parameters["PatternLength"] - (int)Parameters["TurnAllLightOn"], false);
        p.AddEdge("rbPushBeam", (int)Parameters["PatternLength"] - (int)Parameters["TurnAllLightOn"], false);


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

        // Add Rb Analog channels
        p.AddChannel("rb3DCoolingFrequency");
        p.AddChannel("rb3DCoolingAttenuation");
        p.AddChannel("rbRepumpFrequency");
        p.AddChannel("rbRepumpAttenuation");
        p.AddChannel("rbAbsImagingFrequency");

        // Slowing field
        p.AddAnalogValue("slowingCoilsCurrent", 0, (double)Parameters["slowingCoilsValue"]);
        //p.AddAnalogValue("slowingCoilsCurrent", (int)Parameters["slowingCoilsOffTime"], 0.0);


        // B Field
        p.AddAnalogValue("MOTCoilsCurrent", (int)Parameters["MOTCoilsSwitchOn"], (double)Parameters["MOTCoilsCurrentValue"]);
        //Ramp B field gradient for CMOT:
        p.AddLinearRamp("MOTCoilsCurrent", (int)Parameters["MOTLoadTime"], (int)Parameters["MOTCoilsCurrentRampDuration"], (double)Parameters["CMOTCurrentValue"]);
        p.AddAnalogValue("MOTCoilsCurrent", (int)Parameters["MOTLoadTime"] + (int)Parameters["MOTCoilsCurrentRampDuration"], 0.0);



        // Shim Fields
        p.AddAnalogValue("xShimCoilCurrent", 0, (double)Parameters["xShimLoadCurrent"]);
        p.AddAnalogValue("yShimCoilCurrent", 0, (double)Parameters["yShimLoadCurrent"]);
        p.AddAnalogValue("zShimCoilCurrent", 0, (double)Parameters["zShimLoadCurrent"]);

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


        //Rb Laser intensities
        p.AddAnalogValue("rb3DCoolingAttenuation", 0, 0.0);


        //Rb Laser detunings
        p.AddAnalogValue("rb3DCoolingFrequency", 0, (double)Parameters["MOTCoolingLoadingFrequency"]);
        p.AddAnalogValue("rbRepumpFrequency", 0, (double)Parameters["MOTRepumpLoadingFrequency"]);
        p.AddAnalogValue("rbAbsImagingFrequency", 0, (double)Parameters["ImagingFrequency"]);
        p.AddAnalogValue("rb3DCoolingAttenuation", 0, 0.0);

        //Ramp cooling frequency detuning for CMOT:
        p.AddLinearRamp("rb3DCoolingFrequency", (int)Parameters["MOTLoadTime"], (int)Parameters["MOTCoilsCurrentRampDuration"], (double)Parameters["CMOTDetuning"]);

        //Jump cooling frequency for molasses:
        p.AddAnalogValue("rb3DCoolingFrequency", (int)Parameters["MOTLoadTime"] + (int)Parameters["MOTCoilsCurrentRampDuration"], (double)Parameters["MolassesDetuning"]);

        //Jump repump intensity for molasses:
        p.AddAnalogValue("rbRepumpAttenuation", (int)Parameters["MOTLoadTime"] + (int)Parameters["MOTCoilsCurrentRampDuration"], (double)Parameters["MolassesRepumpIntensity"]);
        //p.AddAnalogValue("rbRepumpAttenuation", (int)Parameters["MOTLoadTime"] + (int)Parameters["MOTCoilsCurrentRampDuration"] + (int)Parameters["MolassesDuration"], 4.4);// for imaging switch to full intensity

        //Ramp cooling light intensity fot Rb molasses in 6 steps:
        //p.AddLinearRamp("rb3DCoolingAttenuation", (int)Parameters["MOTLoadTime"] + (int)Parameters["MOTCoilsCurrentRampDuration"], (int)Parameters["MolassesLightIntensityRampDuration"], (double)Parameters["MolassesCoolingLightEndValue"]);
        p.AddAnalogValue("rb3DCoolingAttenuation", (int)Parameters["MOTLoadTime"] + (int)Parameters["MOTCoilsCurrentRampDuration"], 4.1);
        p.AddAnalogValue("rb3DCoolingAttenuation", (int)Parameters["MOTLoadTime"] + (int)Parameters["MOTCoilsCurrentRampDuration"] + 100, 5.7);
        p.AddAnalogValue("rb3DCoolingAttenuation", (int)Parameters["MOTLoadTime"] + (int)Parameters["MOTCoilsCurrentRampDuration"] + 200, 6.1);
        p.AddAnalogValue("rb3DCoolingAttenuation", (int)Parameters["MOTLoadTime"] + (int)Parameters["MOTCoilsCurrentRampDuration"] + 300, 6.9);
        p.AddAnalogValue("rb3DCoolingAttenuation", (int)Parameters["MOTLoadTime"] + (int)Parameters["MOTCoilsCurrentRampDuration"] + 400, 7.2);
        p.AddAnalogValue("rb3DCoolingAttenuation", (int)Parameters["MOTLoadTime"] + (int)Parameters["MOTCoilsCurrentRampDuration"] + 600, 7.5);

        //Try a detuning ramp on top of intensity ramp:
        //p.AddLinearRamp("rb3DCoolingFrequency", (int)Parameters["MOTLoadTime"] + (int)Parameters["MOTCoilsCurrentRampDuration"] + 100, 700 , (double)Parameters["MolassesFrequencyEndValue"]);

        //Reset all laser frequencies just before the end of pattern to optimum MOT loading values:
        p.AddAnalogValue("rb3DCoolingFrequency", (int)Parameters["PatternLength"] - (int)Parameters["TurnAllLightOn"], (double)Parameters["MOTCoolingLoadingFrequency"]);
        p.AddAnalogValue("rbRepumpFrequency", (int)Parameters["PatternLength"] - (int)Parameters["TurnAllLightOn"], (double)Parameters["MOTRepumpLoadingFrequency"]);
        return p;
    }

}


