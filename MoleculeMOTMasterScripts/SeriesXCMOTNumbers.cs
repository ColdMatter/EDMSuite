using MOTMaster;
using MOTMaster.SnippetLibrary;

using System;
using System.Collections.Generic;
using System.Collections;

using DAQ.Pattern;
using DAQ.Analog;

// This script creates a MOT of CaF and then ramps down the intensity to increase lifetime
// Then for a brief time it ramps up the magnetic field gradient to compress the MOT
// After the compression, an image is taken
public class Patterns : MOTMasterScript
{
    public Patterns()
    {
        Parameters = new Dictionary<string, object>();
        Parameters["PatternLength"] = 50000;
        Parameters["TCLBlockStart"] = 2000; // This is a time before the Q switch
        Parameters["TCLBlockDuration"] = 8000;
        Parameters["FlashToQ"] = 16; // This is a time before the Q switch
        Parameters["QSwitchPulseDuration"] = 10;
        Parameters["FlashPulseDuration"] = 10;
        Parameters["HeliumShutterToQ"] = 100;
        Parameters["HeliumShutterDuration"] = 1550;

        // Camera
        Parameters["CameraTriggerDuration"] = 10;
        Parameters["CameraExposure"] = 1000;

        // Delays
        Parameters["ExpansionTime"] = 50;
        Parameters["WaitBeforeImage"] = 100;

        // Slowing
        Parameters["slowingAOMOnStart"] = 240; //started from 250
        Parameters["slowingAOMOnDuration"] = 45000;
        Parameters["slowingAOMOffStart"] = 1520;//started from 1520
        Parameters["slowingAOMOffDuration"] = 40000;
        Parameters["slowingRepumpAOMOnStart"] = 0;//started from 0
        Parameters["slowingRepumpAOMOnDuration"] = 45000;
        Parameters["slowingRepumpAOMOffStart"] = 1520;//1520
        Parameters["slowingRepumpAOMOffDuration"] = 35000;

        // Slowing Chirp
        Parameters["SlowingChirpStartTime"] = 380;// 380;
        Parameters["SlowingChirpDuration"] = 1160; //1160
        Parameters["SlowingChirpStartValue"] = 0.0;//0.0
        Parameters["SlowingChirpEndValue"] = -1.25; //-1.25

        // Slowing field
        Parameters["slowingCoilsValue"] = 0.42; //1.1;
        Parameters["slowingCoilsOffTime"] = 4000;

        // Shim fields
        Parameters["xShimLoadCurrent"] = -1.35;// -1.35 is zero
        Parameters["yShimLoadCurrent"] = -1.92;// -1.92 is zero
        Parameters["zShimLoadCurrent"] = -0.22;// -0.22 is zero

        // v0 Light intensity
        Parameters["v0IntensityRampStartValue"] = 5.6;
        Parameters["v0IntensityRampEndValue"] = 8.0;
        Parameters["v0IntensityCMOTValue"] = 8.0;

        // v0 light durations
        Parameters["MOTLoadDuration"] = 4000;
        Parameters["v0IntensityRampDuration"] = 100;
        Parameters["MOTHoldDuration"] = 2000;
        Parameters["CMOTRampDuration"] = 800;
        Parameters["CMOTHoldDuration"] = 100;

        // v0 Light Frequency (0.0 for 114.1MHz)
        Parameters["v0FrequencyMOTValue"] = 0.0;
        Parameters["v0FrequencyCMOTValue"] = 3.5;

        // B field
        Parameters["MOTFieldValue"] = 1.0;
        Parameters["CMOTFieldValue"] = 3.0;

        //v0aomCalibrationValues
        Parameters["calibGradient"] = 11.4;


    }

    public override PatternBuilder32 GetDigitalPattern()
    {
        PatternBuilder32 p = new PatternBuilder32();
        int patternStartBeforeQ = (int)Parameters["TCLBlockStart"];
        int motLoadEndTime = patternStartBeforeQ + (int)Parameters["MOTLoadDuration"];
        int motRampEndTime = motLoadEndTime + (int)Parameters["v0IntensityRampDuration"];
        int cmotRampStartTime = motRampEndTime + (int)Parameters["MOTHoldDuration"];
        int cmotRampEndTime = cmotRampStartTime + (int)Parameters["CMOTRampDuration"];
        int imageTime = cmotRampEndTime + (int)Parameters["CMOTHoldDuration"];
        int endOfTime = imageTime + 3000;

        MOTMasterScriptSnippet lm = new LoadMoleculeMOT(p, Parameters);
        //Camera triggers:
        p.Pulse(0, imageTime, (int)Parameters["CameraTriggerDuration"], "cameraTrigger");
        // bX shutter
        p.Pulse(0, motLoadEndTime, endOfTime, "bXSlowingShutter");

        return p;
    }

    public override AnalogPatternBuilder GetAnalogPattern()
    {
        AnalogPatternBuilder p = new AnalogPatternBuilder((int)Parameters["PatternLength"]);

        MOTMasterScriptSnippet lm = new LoadMoleculeMOT(p, Parameters);

        int motLoadEndTime = (int)Parameters["MOTLoadDuration"];
        int motRampEndTime = motLoadEndTime + (int)Parameters["v0IntensityRampDuration"];
        int cmotRampStartTime = motRampEndTime + (int)Parameters["MOTHoldDuration"];
        int cmotRampEndTime = cmotRampStartTime + (int)Parameters["CMOTRampDuration"];
        int imageTime = cmotRampEndTime + (int)Parameters["CMOTHoldDuration"];
        int endOfTime = imageTime + 3000;

        // Add Analog Channels
        p.AddChannel("v00Intensity");
        p.AddChannel("v00Frequency");
        p.AddChannel("xShimCoilCurrent");
        p.AddChannel("yShimCoilCurrent");
        p.AddChannel("zShimCoilCurrent");
        p.AddChannel("v00EOMAmp");
        
        // Slowing field
        p.AddAnalogValue("slowingCoilsCurrent", 0, (double)Parameters["slowingCoilsValue"]);
        p.AddAnalogValue("slowingCoilsCurrent", (int)Parameters["slowingCoilsOffTime"], 0.0);
        // B Field
        p.AddAnalogValue("MOTCoilsCurrent", 0, (double)Parameters["MOTFieldValue"]);
        p.AddLinearRamp("MOTCoilsCurrent", cmotRampStartTime, (int)Parameters["CMOTRampDuration"], (double)Parameters["CMOTFieldValue"]);
        p.AddAnalogValue("MOTCoilsCurrent", endOfTime, -0.05);
        // Shim Fields
        p.AddAnalogValue("xShimCoilCurrent", 0, (double)Parameters["xShimLoadCurrent"]);
        p.AddAnalogValue("yShimCoilCurrent", 0, (double)Parameters["yShimLoadCurrent"]);
        p.AddAnalogValue("zShimCoilCurrent", 0, (double)Parameters["zShimLoadCurrent"]);
        // v0 Intensity
        p.AddAnalogValue("v00Intensity", 0, (double)Parameters["v0IntensityRampStartValue"]);
        p.AddLinearRamp("v00Intensity", motLoadEndTime, (int)Parameters["v0IntensityRampDuration"], (double)Parameters["v0IntensityRampEndValue"]);
        p.AddAnalogValue("v00Intensity", cmotRampStartTime, (double)Parameters["v0IntensityCMOTValue"]);
        p.AddAnalogValue("v00Intensity", imageTime, (double)Parameters["v0IntensityRampStartValue"]);
        // v0 Frequency
        p.AddAnalogValue("v00Frequency", 0, 10.0 - (double)Parameters["v0FrequencyMOTValue"] / (double)Parameters["calibGradient"]);
        p.AddAnalogValue("v00Frequency", cmotRampStartTime, 10.0 - (double)Parameters["v0FrequencyCMOTValue"] / (double)Parameters["calibGradient"]);
        p.AddAnalogValue("v00Frequency", imageTime, 10.0 - (double)Parameters["v0FrequencyMOTValue"] / (double)Parameters["calibGradient"]);
        // v0 frequency with EOM
        p.AddAnalogValue("v00EOMAmp", 0, 4.85);
        

        return p;
    }

}
