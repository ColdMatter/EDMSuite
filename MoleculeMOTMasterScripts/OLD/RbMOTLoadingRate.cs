using MOTMaster;
using MOTMaster.SnippetLibrary;

using System;
using System.Collections.Generic;

using DAQ.Pattern;
using DAQ.Analog;

public class Patterns : MOTMasterScript
{
    public Patterns()
    {
        Parameters = new Dictionary<string, object>();
        Parameters["PatternLength"] = 1000000;

        // Camera
        Parameters["Frame0TriggerDuration"] = 10;

        Parameters["LoadingDuration"] = 800000;
        Parameters["MolassesDuration"] = 1;
        Parameters["ExpansionTime"] = 1000;
        Parameters["MOTCoilsCurrentValue"] = 0.32;

        // Intensity
        Parameters["rbCoolingMOTIntensity"] = 2.4;

        // Frequency
        Parameters["rbCoolingMOTLoadingFrequency"] = 3.03;
        Parameters["rbCoolingImagingFrequency"] = 3.49;

        // Shim fields
        Parameters["xShimLoadCurrent"] = 2.41;// 2.41;// 1.41;
        Parameters["yShimLoadCurrent"] = 0.3; //2.4
        Parameters["zShimLoadCurrent"] = -6.39; //-6.39
    }

    public override PatternBuilder32 GetDigitalPattern()
    {
        PatternBuilder32 p = new PatternBuilder32();

        int imageTime = (int)Parameters["LoadingDuration"];

        p.Pulse(0, 0, 10, "aoPatternTrigger");  // THIS TRIGGERS THE ANALOG PATTERN. The analog pattern will start at the same time as the Q-switch is fired.

        p.AddEdge("rbOpticalPumpingAOM", 0, true); // Turn off for most of sequence
        p.AddEdge("rbOpticalPumpingAOM", imageTime + 2000, false); // Turn back on safely after imaging has finished

        p.Pulse(0, imageTime, (int)Parameters["Frame0TriggerDuration"], "cameraTrigger2");

        return p;
    }

    public override AnalogPatternBuilder GetAnalogPattern()
    {
        AnalogPatternBuilder p = new AnalogPatternBuilder((int)Parameters["PatternLength"]);

        int imageTime = (int)Parameters["LoadingDuration"];

        // Add Analog Channels

        p.AddChannel("MOTCoilsCurrent");
        p.AddChannel("xShimCoilCurrent");
        p.AddChannel("yShimCoilCurrent");
        p.AddChannel("zShimCoilCurrent");
        p.AddChannel("rbCoolingIntensity");
        p.AddChannel("rbCoolingFrequency");

        // B Field
        p.AddAnalogValue("MOTCoilsCurrent", 0, (double)Parameters["MOTCoilsCurrentValue"]);
        p.AddAnalogValue("MOTCoilsCurrent", imageTime + 2000, 0.0);

        // Shim Fields
        p.AddAnalogValue("xShimCoilCurrent", 0, (double)Parameters["xShimLoadCurrent"]);
        p.AddAnalogValue("yShimCoilCurrent", 0, (double)Parameters["yShimLoadCurrent"]);
        p.AddAnalogValue("zShimCoilCurrent", 0, (double)Parameters["zShimLoadCurrent"]);

        // Cooling Intensity
        p.AddAnalogValue("rbCoolingIntensity", 0, (double)Parameters["rbCoolingMOTIntensity"]);

        // Cooling frequency
        p.AddAnalogValue("rbCoolingFrequency", 0, (double)Parameters["rbCoolingMOTLoadingFrequency"]);
        p.AddAnalogValue("rbCoolingFrequency", imageTime - 200, (double)Parameters["rbCoolingImagingFrequency"]);

        return p;
    }

}
