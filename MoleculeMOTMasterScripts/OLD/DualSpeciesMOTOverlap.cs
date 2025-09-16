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
        Parameters["MOTLoadTime"] = 100000;
        Parameters["CMOTCoilsCurrentRampDuration"] = 10000;
        Parameters["TCLBlockStart"] = 4000; // This is a time before the Q switch
        Parameters["TCLBlockDuration"] = 15000;
        Parameters["FlashToQ"] = 16; // This is a time before the Q switch
        Parameters["QSwitchPulseDuration"] = 10;
        Parameters["FlashPulseDuration"] = 10;
        Parameters["HeliumShutterToQ"] = 100;
        Parameters["HeliumShutterDuration"] = 1550;

        // Camera
        Parameters["Frame0Trigger"] = 4000;
        Parameters["Frame0TriggerDuration"] = 10;


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
        Parameters["MOTCoilsCurrentValue"] = 0.5;//1.0; // 0.65;
        Parameters["CMOTCurrentValue"] = 1.0;

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

        //Rb light
        Parameters["ImagingFrequency"] = 3.0; //2.1
        Parameters["MOTCoolingLoadingFrequency"] = 5.0;
        Parameters["MOTRepumpLoadingFrequency"] = 6.9; //6.9
        Parameters["CMOTDetuning"] = 1.5;


    }

    public override PatternBuilder32 GetDigitalPattern()
    {
        PatternBuilder32 p = new PatternBuilder32();
        int patternStartBeforeQ = (int)Parameters["TCLBlockStart"] + (int)Parameters["MOTLoadTime"] + (int)Parameters["CMOTCoilsCurrentRampDuration"];

        MOTMasterScriptSnippet lm = new LoadMoleculeMOTDualSpecies(p, Parameters);  // This is how you load "preset" patterns.          
        //   p.AddEdge("v00Shutter", 0, true);
        //p.Pulse(patternStartBeforeQ, 3000 - 1400, 10000, "bXSlowingShutter"); //Takes 14ms to start closing

        //Rb light switches:

        p.AddEdge("rb3DCooling", 0, false);
        p.AddEdge("rb3DCooling", (int)Parameters["MOTLoadTime"] + (int)Parameters["CMOTCoilsCurrentRampDuration"] + 50000, true);
        p.AddEdge("rb2DCooling", 0, false);
        p.AddEdge("rb2DCooling", (int)Parameters["MOTLoadTime"], true);//Switch off 2D MOT after 3D MOT is loaded
        p.AddEdge("rbPushBeam", 0, false);
        p.AddEdge("rbPushBeam", (int)Parameters["MOTLoadTime"], true);//Switch off push beam after 3D MOT is loaded


        //Here we send 4 camera triggers once the CaF MOT is loaded separated by 25 ms each to do a single experiment life time measurement:
        p.Pulse(patternStartBeforeQ, (int)Parameters["Frame0Trigger"], (int)Parameters["Frame0TriggerDuration"], "cameraTrigger"); //camera trigger for first frame
        //p.Pulse(patternStartBeforeQ, (int)Parameters["Frame0Trigger"] + 2500, (int)Parameters["Frame0TriggerDuration"], "cameraTrigger"); //camera trigger for 2nd frame
        //p.Pulse(patternStartBeforeQ, (int)Parameters["Frame0Trigger"] + 5000, (int)Parameters["Frame0TriggerDuration"], "cameraTrigger"); //camera trigger for 3rd frame
        //p.Pulse(patternStartBeforeQ, (int)Parameters["Frame0Trigger"] + 7500, (int)Parameters["Frame0TriggerDuration"], "cameraTrigger"); //camera trigger for 4th frame
        //p.Pulse(patternStartBeforeQ, (int)Parameters["Frame0Trigger"] + 10000, (int)Parameters["Frame0TriggerDuration"], "cameraTrigger"); //camera trigger for 4th frame

        p.Pulse(patternStartBeforeQ, (int)Parameters["Frame0Trigger"] + 7500, (int)Parameters["Frame0TriggerDuration"], "rbAbsImgCamTrig"); //camera trigger for 4th frame

        return p;
    }

    public override AnalogPatternBuilder GetAnalogPattern()
    {
        AnalogPatternBuilder p = new AnalogPatternBuilder((int)Parameters["PatternLength"]);

        MOTMasterScriptSnippet lm = new LoadMoleculeMOTDualSpecies(p, Parameters);

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
        
        //Rb Laser detunings starting values
        p.AddAnalogValue("rb3DCoolingFrequency", 0, (double)Parameters["MOTCoolingLoadingFrequency"]);
        p.AddAnalogValue("rbRepumpFrequency", 0, (double)Parameters["MOTRepumpLoadingFrequency"]);
        p.AddAnalogValue("rbAbsImagingFrequency", 0, (double)Parameters["ImagingFrequency"]);
        p.AddAnalogValue("rb3DCoolingAttenuation", 0, 0.0);

        
        // Slowing field
        p.AddAnalogValue("slowingCoilsCurrent", 0, (double)Parameters["slowingCoilsValue"]);
        p.AddAnalogValue("slowingCoilsCurrent", (int)Parameters["slowingCoilsOffTime"], 0.0);
        
        // B Field
        p.AddAnalogValue("MOTCoilsCurrent", (int)Parameters["MOTCoilsSwitchOn"], (double)Parameters["MOTCoilsCurrentValue"]);
        p.AddLinearRamp("MOTCoilsCurrent", (int)Parameters["MOTLoadTime"], (int)Parameters["CMOTCoilsCurrentRampDuration"], (double)Parameters["CMOTCurrentValue"]);//Ramp field to CaF loading gradient
        p.AddAnalogValue("MOTCoilsCurrent", (int)Parameters["MOTLoadTime"] + (int)Parameters["CMOTCoilsCurrentRampDuration"] + 50000, 0.0); //This will switch off the MOT coils 500 ms after CaF MOT loading sequence start
        
        // Shim Fields
        p.AddAnalogValue("xShimCoilCurrent", 0, (double)Parameters["xShimLoadCurrent"]);
        p.AddAnalogValue("yShimCoilCurrent", 0, (double)Parameters["yShimLoadCurrent"]);
        p.AddAnalogValue("zShimCoilCurrent", 0, (double)Parameters["zShimLoadCurrent"]);


        // F=0
        p.AddAnalogValue("v00EOMAmp", 0, 5.2);

        // v0 Intensity Ramp
        p.AddAnalogValue("v00Intensity", 0, (double)Parameters["v0IntensityRampStartValue"]);

        // v0 Frequency Ramp
        p.AddAnalogValue("v00Frequency", 0, (double)Parameters["v0FrequencyStartValue"]);

        //v0 chirp
        p.AddAnalogValue("v00Chirp", 0, 0.0);

        //Rb CMOT detuning ramp
        //p.AddLinearRamp("rb3DCoolingFrequency", (int)Parameters["MOTLoadTime"], (int)Parameters["CMOTCoilsCurrentRampDuration"], (double)Parameters["MOTCoolingLoadingFrequency"]);


        

        return p;
    }

}
