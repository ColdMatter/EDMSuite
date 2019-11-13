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
        Parameters["PatternLength"] = 50000;
        Parameters["TCLBlockStart"] = 4000; // This is a time before the Q switch
        Parameters["TCLBlockDuration"] = 15000;
        Parameters["FlashToQ"] = 16; // This is a time before the Q switch
        Parameters["QSwitchPulseDuration"] = 10;
        Parameters["FlashPulseDuration"] = 10;
        Parameters["HeliumShutterToQ"] = 100;
        Parameters["HeliumShutterDuration"] = 1550;

        Parameters["MOTLoadTime"] = 4000;
        Parameters["MOTHoldTime"] = 500;
        Parameters["FreeExpansionTime"] = 800;
        Parameters["FullIntensityBlueMolassesDuration"] = 500;
        Parameters["BlueMolassesStartDelay"] = 10;

        // Camera
        Parameters["Frame0Trigger"] = 4000;
        Parameters["Frame0TriggerDuration"] = 10;
        Parameters["ExposureTime"] = 1000;

        //PMT
        Parameters["PMTTrigger"] = 5000;
        Parameters["PMTTriggerDuration"] = 10;


        // Slowing
        Parameters["slowingAOMOnStart"] = 260; //started from 250
        Parameters["slowingAOMOnDuration"] = 45000;
        Parameters["slowingAOMOffStart"] = 1520;//started from 1500
        Parameters["slowingAOMOffDuration"] = 40000;
        Parameters["slowingRepumpAOMOnStart"] = 0;//started from 0
        Parameters["slowingRepumpAOMOnDuration"] = 45000;
        Parameters["slowingRepumpAOMOffStart"] = 1520;
        Parameters["slowingRepumpAOMOffDuration"] = 35000;

        // Slowing Chirp
        Parameters["SlowingChirpStartTime"] = 360;// 340;
        Parameters["SlowingChirpDuration"] = 1160;
        Parameters["SlowingChirpStartValue"] = 0.0;//0.0
        Parameters["SlowingChirpEndValue"] = -1.25;

        // Slowing field
        Parameters["slowingCoilsValue"] = 0.42; //1.05;
        Parameters["slowingCoilsOffTime"] = 1500;

        // B Field
        Parameters["MOTCoilsSwitchOn"] = 0;
        Parameters["MOTCoilsSwitchOff"] = 20000;
        Parameters["MOTCoilsCurrentValue"] = 1.0;//1.0; // 0.65;
        Parameters["CMOTRampDuration"] = 1000;
        Parameters["CMOTEndValue"] = 2.0;
        Parameters["CMOTHoldTime"] = 500;

        // Shim fields
        Parameters["xShimLoadCurrent"] = 6.0;//3.6
        Parameters["yShimLoadCurrent"] = 0.0;//-0.12
        Parameters["zShimLoadCurrent"] = 1.5;//-5.35


        // v0 Light Switch
        Parameters["MOTAOMStartTime"] = 15000;
        Parameters["MOTAOMDuration"] = 500;

        // v0 Light Intensity
        Parameters["v0IntensityRampDuration"] = 600;
        Parameters["v0IntensityRampStartValue"] = 5.8;
        Parameters["v0IntensityRampEndValue"] = 8.465;
        Parameters["v0IntensityMolassesValue"] = 5.8;
        Parameters["v0IntensityMolassesRampDuration"] = 100;
        Parameters["v0IntensityMolassesEndValue"] = 7.83;

        // triggering delay (10V = 1 second)
        // Parameters["triggerDelay"] = 5.0;

        // v0 F=1 (dodgy code using an analogue output to control a TTL)
        Parameters["v0FrequencyStartValue"] = 0.0; //set this to 0.0 for 114.1MHz 
        Parameters["v0FrequencyNewValue"] = 20.0; //set this to MHz detuning desired if doing frequency jump (positive for blue detuning)

        //v0aomCalibrationValues
        Parameters["lockAomFrequency"] = 114.1;
        Parameters["calibOffset"] = 64.2129;
        Parameters["calibGradient"] = 5.55075;




    }

    public override PatternBuilder32 GetDigitalPattern()
    {
        PatternBuilder32 p = new PatternBuilder32();
        int patternStartBeforeQ = (int)Parameters["TCLBlockStart"];

        MOTMasterScriptSnippet lm = new LoadMoleculeMOT(p, Parameters);  // This is how you load "preset" patterns.          
        //   p.AddEdge("v00Shutter", 0, true);
        int motSwitchOffTime = patternStartBeforeQ + (int)Parameters["MOTLoadTime"] + (int)Parameters["v0IntensityRampDuration"] + (int)Parameters["MOTHoldTime"];
        int blueMolassesStartTime = motSwitchOffTime + (int)Parameters["BlueMolassesStartDelay"];
        int fullIntensityBlueMolassesEndTime = blueMolassesStartTime + (int)Parameters["FullIntensityBlueMolassesDuration"];
        int blueMolassesEndTime = fullIntensityBlueMolassesEndTime + (int)Parameters["v0IntensityMolassesRampDuration"];
        int cameraTrigger = blueMolassesEndTime + (int)Parameters["FreeExpansionTime"];

        p.Pulse(0, motSwitchOffTime, (int)Parameters["BlueMolassesStartDelay"], "v00MOTAOM"); //Pulse off V00 while MOT coils are being switched off and frequency is being jumped to the blue
        p.Pulse(0, blueMolassesEndTime, (int)Parameters["FreeExpansionTime"], "v00MOTAOM"); //Pulse off V00 while MOT coils are being switched off and frequency is being jumped to the blue
        p.Pulse(0, cameraTrigger, (int)Parameters["Frame0TriggerDuration"], "cameraTrigger"); //camera trigger for first frame





        return p;
    }

    public override AnalogPatternBuilder GetAnalogPattern()
    {
        AnalogPatternBuilder p = new AnalogPatternBuilder((int)Parameters["PatternLength"]);

        MOTMasterScriptSnippet lm = new LoadMoleculeMOT(p, Parameters);

        int motSwitchOffTime = (int)Parameters["MOTLoadTime"] + (int)Parameters["v0IntensityRampDuration"] + (int)Parameters["MOTHoldTime"];
        int blueMolassesStartTime = motSwitchOffTime + (int)Parameters["BlueMolassesStartDelay"];
        int fullIntensityBlueMolassesEndTime = blueMolassesStartTime + (int)Parameters["FullIntensityBlueMolassesDuration"];
        int blueMolassesEndTime = fullIntensityBlueMolassesEndTime + (int)Parameters["v0IntensityMolassesRampDuration"];
        int cameraTrigger = blueMolassesEndTime + (int)Parameters["FreeExpansionTime"];

        // Add Analog Channels

        p.AddChannel("v00Intensity");
        p.AddChannel("v00Frequency");
        p.AddChannel("xShimCoilCurrent");
        p.AddChannel("yShimCoilCurrent");
        p.AddChannel("zShimCoilCurrent");
        p.AddChannel("v00EOMAmp");
        p.AddChannel("v00Chirp");


        // Slowing field
        p.AddAnalogValue("slowingCoilsCurrent", 0, (double)Parameters["slowingCoilsValue"]);
        p.AddAnalogValue("slowingCoilsCurrent", (int)Parameters["slowingCoilsOffTime"], 0.0);

        // B Field
        p.AddAnalogValue("MOTCoilsCurrent", (int)Parameters["MOTCoilsSwitchOn"], (double)Parameters["MOTCoilsCurrentValue"]);
        p.AddAnalogValue("MOTCoilsCurrent", motSwitchOffTime, -0.05); //switch off MOT coils for blue molasses
        
        // Shim Fields
        p.AddAnalogValue("xShimCoilCurrent", 0, (double)Parameters["xShimLoadCurrent"]);
        p.AddAnalogValue("yShimCoilCurrent", 0, (double)Parameters["yShimLoadCurrent"]);
        p.AddAnalogValue("zShimCoilCurrent", 0, (double)Parameters["zShimLoadCurrent"]);

        // F=0
        p.AddAnalogValue("v00EOMAmp", 0, 4.7);
        
        // v0 Intensity Ramp
        p.AddAnalogValue("v00Intensity", 0, (double)Parameters["v0IntensityRampStartValue"]);
        p.AddLinearRamp("v00Intensity", (int)Parameters["MOTLoadTime"], (int)Parameters["v0IntensityRampDuration"], (double)Parameters["v0IntensityRampEndValue"]);
        p.AddAnalogValue("v00Intensity", blueMolassesStartTime, (double)Parameters["v0IntensityMolassesValue"]);
        p.AddLinearRamp("v00Intensity", fullIntensityBlueMolassesEndTime, (int)Parameters["v0IntensityMolassesRampDuration"], (double)Parameters["v0IntensityMolassesEndValue"]);
        //p.AddAnalogValue("v00Intensity", fullIntensityBlueMolassesEndTime + 50, 7.4);
        //p.AddAnalogValue("v00Intensity", fullIntensityBlueMolassesEndTime + 100, 7.83);
        //p.AddAnalogValue("v00Intensity", fullIntensityBlueMolassesEndTime + 150, 8.09);
        p.AddAnalogValue("v00Intensity", cameraTrigger, (double)Parameters["v0IntensityRampStartValue"]);
        
        // v0 Frequency Ramp
        p.AddAnalogValue("v00Frequency", 0, ((double)Parameters["lockAomFrequency"] - (double)Parameters["v0FrequencyStartValue"] / 2 - (double)Parameters["calibOffset"]) / (double)Parameters["calibGradient"]);
        p.AddAnalogValue("v00Frequency", motSwitchOffTime, ((double)Parameters["lockAomFrequency"] - (double)Parameters["v0FrequencyNewValue"] / 2 - (double)Parameters["calibOffset"]) / (double)Parameters["calibGradient"]);//jump to blue detuning
        p.AddAnalogValue("v00Frequency", cameraTrigger, ((double)Parameters["lockAomFrequency"] - (double)Parameters["v0FrequencyStartValue"] / 2 - (double)Parameters["calibOffset"]) / (double)Parameters["calibGradient"]); //jump aom frequency back to normal for imaging 
        
        //v0 chirp
        p.AddAnalogValue("v00Chirp", 0, 0.0);
        
        return p;
    }

}
