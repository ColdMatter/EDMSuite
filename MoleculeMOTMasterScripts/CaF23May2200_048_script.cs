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

        // Camera
        Parameters["Frame0Trigger"] = 4000;
        Parameters["Frame0TriggerDuration"] = 1000;
        Parameters["CameraTriggerTransverseTime"] = 120;

        //PMT
        Parameters["PMTTriggerDuration"] = 10;

        // Slowing
        Parameters["slowingAOMOnStart"] = 160; //180
        Parameters["PMTTrigger"] = 5000;
        Parameters["slowingAOMOnDuration"] = 45000;
        Parameters["slowingAOMOffStart"] = 1760;//started from 1520
        Parameters["slowingAOMOffDuration"] = 40000;
        Parameters["slowingRepumpAOMOnStart"] = 0;//started from 0
        Parameters["slowingRepumpAOMOnDuration"] = 45000;
        Parameters["slowingRepumpAOMOffStart"] = 1760;//1520
        Parameters["slowingRepumpAOMOffDuration"] = 35000;

        // Slowing Chirp
        Parameters["SlowingChirpStartTime"] = 360;// 380;
        Parameters["SlowingChirpDuration"] = 1400; //1160
        Parameters["SlowingChirpStartValue"] = 0.0;//0.0
        Parameters["SlowingChirpEndValue"] = -1.3; //-1.25

        // Slowing field
        Parameters["slowingCoilsValue"] = 0.42; //1.05;
        Parameters["slowingCoilsOffTime"] = 1500;

        // B Field
        Parameters["MOTCoilsSwitchOn"] = 0;
        Parameters["MOTCoilsSwitchOff"] = 20000;
        Parameters["MOTCoilsCurrentValue"] = 1.0;//1.0; // 0.65;

        // Shim fields
        Parameters["xShimLoadCurrent"] = -1.35;// -1.35 is zero
        Parameters["yShimLoadCurrent"] = -1.92;// -1.92 is zero
        Parameters["zShimLoadCurrent"] = -0.56;// -0.56,   -0.22 is zero

        // v0 Light Switch
        Parameters["MOTAOMStartTime"] = 15000;
        Parameters["MOTAOMDuration"] = 500;

        // v0 Light Intensity
        Parameters["v0IntensityRampStartValue"] = 7.2;

        // v0 Light Frequency
        Parameters["v0FrequencyStartValue"] = 10.0; //9.0

        // triggering delay (10V = 1 second)
        // Parameters["triggerDelay"] = 5.0;

        // v0 F=1 (dodgy code using an analogue output to control a TTL)
        Parameters["v0F1AOMStartValue"] = 5.0;
        Parameters["v0F1AOMOffValue"] = 0.0;

        Parameters["MOTLoadDuration"] = 4000;
        Parameters["V0IntensityRampDuration"] = 400;
        Parameters["CMOTRampDuration"] = 600;
        Parameters["v0IntensityRampEndValue"] = 8.0;
        Parameters["CMOTGradient"] = 3.5;

        Parameters["BlueMolassesDelay"] = 1;
        Parameters["BlueMolassesDuration"] = 300;
        Parameters["FreeExpansionTime"] = 100;

        Parameters["calibGradient"] = 11.4;
        Parameters["v0FrequencyMOTValue"] = 0.0;
        Parameters["v0FrequencyMolassesValue"] = 20.0;

        Parameters["ImageDelay"] = 1000;



    }

    public override PatternBuilder32 GetDigitalPattern()
    {
        PatternBuilder32 p = new PatternBuilder32();
        int patternStartBeforeQ = (int)Parameters["TCLBlockStart"];
        int cmotStartTime = patternStartBeforeQ + (int)Parameters["MOTLoadDuration"];
        int v0IntensityRampStartTime = cmotStartTime + (int)Parameters["CMOTRampDuration"];
        int v0IntensityRampEndTime = v0IntensityRampStartTime + (int)Parameters["V0IntensityRampDuration"];
        int blueMolassesStartTime = v0IntensityRampEndTime + (int)Parameters["BlueMolassesDelay"];
        int blueMolassesEndTime = blueMolassesStartTime + (int)Parameters["BlueMolassesDuration"];
        int imageTime = blueMolassesEndTime + (int)Parameters["FreeExpansionTime"];

        MOTMasterScriptSnippet lm = new LoadMoleculeMOT(p, Parameters);  // This is how you load "preset" patterns.          

        p.Pulse(0, imageTime, (int)Parameters["Frame0TriggerDuration"], "cameraTrigger"); //camera trigger for first frame

        p.Pulse(0, v0IntensityRampEndTime, blueMolassesStartTime - v0IntensityRampEndTime, "v00MOTAOM");
        p.Pulse(0, blueMolassesEndTime, imageTime - blueMolassesEndTime, "v00MOTAOM");

        return p;
    }

    public override AnalogPatternBuilder GetAnalogPattern()
    {
        AnalogPatternBuilder p = new AnalogPatternBuilder((int)Parameters["PatternLength"]);
        MOTMasterScriptSnippet lm = new LoadMoleculeMOT(p, Parameters);

        int cmotStartTime = (int)Parameters["MOTLoadDuration"];
        int v0IntensityRampStartTime = cmotStartTime + (int)Parameters["CMOTRampDuration"];
        int v0IntensityRampEndTime = v0IntensityRampStartTime + (int)Parameters["V0IntensityRampDuration"];
        int blueMolassesStartTime = v0IntensityRampEndTime + (int)Parameters["BlueMolassesDelay"];
        int blueMolassesEndTime = blueMolassesStartTime + (int)Parameters["BlueMolassesDuration"];
        int imageTime = blueMolassesEndTime + (int)Parameters["FreeExpansionTime"];

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
        p.AddAnalogValue("MOTCoilsCurrent", 0, (double)Parameters["MOTCoilsCurrentValue"]);
        p.AddLinearRamp("MOTCoilsCurrent", cmotStartTime, (int)Parameters["CMOTRampDuration"], (double)Parameters["CMOTGradient"]);
        p.AddAnalogValue("MOTCoilsCurrent", v0IntensityRampEndTime, -0.05);

        // Shim Fields
        p.AddAnalogValue("xShimCoilCurrent", 0, (double)Parameters["xShimLoadCurrent"]);
        p.AddAnalogValue("yShimCoilCurrent", 0, (double)Parameters["yShimLoadCurrent"]);
        p.AddAnalogValue("zShimCoilCurrent", 0, (double)Parameters["zShimLoadCurrent"]);

        // trigger delay
        // p.AddAnalogValue("triggerDelay", 0, (double)Parameters["triggerDelay"]);

        // F=0
        p.AddAnalogValue("v00EOMAmp", 0, 4.4);

        // v0 Intensity Ramp
        p.AddAnalogValue("v00Intensity", 0, (double)Parameters["v0IntensityRampStartValue"]);
        p.AddLinearRamp("v00Intensity", v0IntensityRampStartTime, (int)Parameters["V0IntensityRampDuration"], (double)Parameters["v0IntensityRampEndValue"]);
        p.AddAnalogValue("v00Intensity", v0IntensityRampEndTime, (double)Parameters["v0IntensityRampStartValue"]);

        // v0 Frequency Ramp
        p.AddAnalogValue("v00Frequency", 0, 10.0 - (double)Parameters["v0FrequencyMOTValue"] / (double)Parameters["calibGradient"]);
        p.AddAnalogValue("v00Frequency", v0IntensityRampEndTime, 10.0 - (double)Parameters["v0FrequencyMolassesValue"] / (double)Parameters["calibGradient"]);
        p.AddAnalogValue("v00Frequency", blueMolassesEndTime, 10.0 - (double)Parameters["v0FrequencyMOTValue"] / (double)Parameters["calibGradient"]);

        return p;
    }

}
