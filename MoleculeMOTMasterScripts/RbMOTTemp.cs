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
        Parameters["PatternLength"] = 200000;

        // Camera
        Parameters["Frame0TriggerDuration"] = 10;

        Parameters["LoadingDuration"] = 150000;
        Parameters["MolassesDuration"] = 1;
        Parameters["ExpansionTime"] = 1000;
        Parameters["MOTCoilsCurrentValue"] = 0.32; 
        Parameters["MOTCoilsCurrentRampEndValue"] = 0.32;
        Parameters["MOTCoilsCurrentRampDuration"] = 1000;
        Parameters["MOTCoilsCurrentWaitAfterRamp"] = 2500;

        // Intensity
        Parameters["rbCoolingMOTIntensity"] = 2.4;
        Parameters["rbCoolingMolassesIntensity"] = 2.4;

        // Shim fields
        Parameters["xShimLoadCurrent"] = 2.41;// 2.41;// 1.41;
        Parameters["yShimLoadCurrent"] = 0.3; //2.4
        Parameters["zShimLoadCurrent"] = -6.39; //-6.39
    }

    public override PatternBuilder32 GetDigitalPattern()
    {
        PatternBuilder32 p = new PatternBuilder32();

        int motCompressStartTime = (int)Parameters["LoadingDuration"];
        int molassesStartTime = motCompressStartTime + (int)Parameters["MOTCoilsCurrentRampDuration"] + (int)Parameters["MOTCoilsCurrentWaitAfterRamp"];
        int releaseTime = molassesStartTime + (int)Parameters["MolassesDuration"];
        int imageTime = releaseTime + (int)Parameters["ExpansionTime"];

        p.Pulse(0, 0, 10, "aoPatternTrigger");  // THIS TRIGGERS THE ANALOG PATTERN. The analog pattern will start at the same time as the Q-switch is fired.

        p.Pulse(0, releaseTime, (int)Parameters["ExpansionTime"], "rbCoolingAOM");

        p.AddEdge("rbOpticalPumpingAOM", 0, true); // Turn off for most of sequence
        p.AddEdge("rbOpticalPumpingAOM", imageTime + 2000, false); // Turn back on safely after imaging has finished

        p.Pulse(0, imageTime, (int)Parameters["Frame0TriggerDuration"], "cameraTrigger2");

        return p;
    }

    public override AnalogPatternBuilder GetAnalogPattern()
    {
        AnalogPatternBuilder p = new AnalogPatternBuilder((int)Parameters["PatternLength"]);

        int motCompressStartTime = (int)Parameters["LoadingDuration"];
        int molassesStartTime = motCompressStartTime + (int)Parameters["MOTCoilsCurrentRampDuration"] + (int)Parameters["MOTCoilsCurrentWaitAfterRamp"];
        int releaseTime = molassesStartTime + (int)Parameters["MolassesDuration"];
        int imageTime = releaseTime + (int)Parameters["ExpansionTime"];

        // Add Analog Channels

        p.AddChannel("MOTCoilsCurrent");
        p.AddChannel("xShimCoilCurrent");
        p.AddChannel("yShimCoilCurrent");
        p.AddChannel("zShimCoilCurrent");
        p.AddChannel("rbCoolingIntensity");

        // B Field
        p.AddAnalogValue("MOTCoilsCurrent", 0, (double)Parameters["MOTCoilsCurrentValue"]);
        p.AddAnalogValue("MOTCoilsCurrent", molassesStartTime, 0.0);

        // Shim Fields
        p.AddAnalogValue("xShimCoilCurrent", 0, (double)Parameters["xShimLoadCurrent"]);
        p.AddAnalogValue("yShimCoilCurrent", 0, (double)Parameters["yShimLoadCurrent"]);
        p.AddAnalogValue("zShimCoilCurrent", 0, (double)Parameters["zShimLoadCurrent"]);

        // Cooling light intensity
        p.AddAnalogValue("rbCoolingIntensity", 0, (double)Parameters["rbCoolingMOTIntensity"]);
        p.AddAnalogValue("rbCoolingIntensity", molassesStartTime, (double)Parameters["rbCoolingMolassesIntensity"]);
        p.AddAnalogValue("rbCoolingIntensity", molassesStartTime + 100, 1.79);
        p.AddAnalogValue("rbCoolingIntensity", molassesStartTime + 200, 1.65);
        p.AddAnalogValue("rbCoolingIntensity", molassesStartTime + 300, 1.48);
        p.AddAnalogValue("rbCoolingIntensity", molassesStartTime + 400, 1.397);
        p.AddAnalogValue("rbCoolingIntensity", molassesStartTime + 600, 1.316);
        p.AddAnalogValue("rbCoolingIntensity", imageTime, (double)Parameters["rbCoolingMOTIntensity"]);

        return p;
    }

}
