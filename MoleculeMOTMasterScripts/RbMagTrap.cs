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
        Parameters["PatternLength"] = 100000;

        // Camera
        Parameters["Frame0TriggerDuration"] = 10;

        Parameters["LoadingDuration"] = 50000;
        Parameters["MolassesDuration"] = 800;
        Parameters["OpticalPumpingDuration"] = 1;
        Parameters["MagTrapDuration"] = 10000;
        Parameters["MOTWaitBeforeImage"] = 500;

        Parameters["MOTCoilsCurrentValue"] = 0.32;
        Parameters["MOTCoilsCurrentRampEndValue"] = 0.32;
        Parameters["MOTCoilsCurrentRampDuration"] = 1000;
        Parameters["MOTCoilsCurrentWaitAfterRamp"] = 2500;
        Parameters["MOTCoilsCurrentMagTrapValue"] = 1.2;

        // Intensity
        Parameters["rbCoolingMOTIntensity"] = 2.4;
        Parameters["rbCoolingMOTIntensityRampEndValue"] = 2.4;
        Parameters["rbCoolingMOTIntensityRampDuration"] = 1000;
        Parameters["rbCoolingMolassesIntensity"] = 2.4;

        // Shim fields
        Parameters["xShimLoadCurrent"] = 4.41;// 2.41;// 1.41;
        Parameters["yShimLoadCurrent"] = 0.3; //2.4
        Parameters["zShimLoadCurrent"] = -6.39; //-6.39
    }

    public override PatternBuilder32 GetDigitalPattern()
    {
        PatternBuilder32 p = new PatternBuilder32();

        int motCompressStartTime = (int)Parameters["LoadingDuration"];
        int molassesStartTime = motCompressStartTime + (int)Parameters["MOTCoilsCurrentRampDuration"] + (int)Parameters["MOTCoilsCurrentWaitAfterRamp"];
        int opticalPumpingStartTime = molassesStartTime + (int)Parameters["MolassesDuration"];
        int magTrapStartTime = opticalPumpingStartTime + (int)Parameters["OpticalPumpingDuration"];
        int motRecaptureTime = magTrapStartTime + (int)Parameters["MagTrapDuration"];
        int imageTime = motRecaptureTime + (int)Parameters["MOTWaitBeforeImage"];

        p.Pulse(0, 0, 10, "aoPatternTrigger");  // THIS TRIGGERS THE ANALOG PATTERN. The analog pattern will start at the same time as the Q-switch is fired.
        
        p.Pulse(0, opticalPumpingStartTime, motRecaptureTime - opticalPumpingStartTime, "rbCoolingAOM");

        p.AddEdge("rbOpticalPumpingAOM", 0, true); // Turn off for most of sequence
        p.DownPulse(0, opticalPumpingStartTime, (int)Parameters["OpticalPumpingDuration"], "rbOpticalPumpingAOM"); // Turn on for optical pumping step
        p.AddEdge("rbOpticalPumpingAOM", imageTime + 2000, false); // Turn back on safely after imaging has finished

        p.Pulse(0, magTrapStartTime - 1600, motRecaptureTime - magTrapStartTime + 1600 - 1200, "rbCoolingShutter"); // Takes 16ms to start closing, 14ms to fully open
        p.Pulse(0, magTrapStartTime - 1400, motRecaptureTime - magTrapStartTime + 1400, "rbOpticalPumpingShutter"); // Takes 14ms to start closing, don't really care when it opens again as not required for imaging

        //p.Pulse(0, molassesStartTime - 500, (int)Parameters["Frame0TriggerDuration"], "cameraTrigger2");

        p.Pulse(0, imageTime, (int)Parameters["Frame0TriggerDuration"], "cameraTrigger2");

        return p;
    }

    public override AnalogPatternBuilder GetAnalogPattern()
    {
        AnalogPatternBuilder p = new AnalogPatternBuilder((int)Parameters["PatternLength"]);

        int motCompressStartTime = (int)Parameters["LoadingDuration"];
        int molassesStartTime = motCompressStartTime + (int)Parameters["MOTCoilsCurrentRampDuration"] + (int)Parameters["MOTCoilsCurrentWaitAfterRamp"];
        int opticalPumpingStartTime = molassesStartTime + (int)Parameters["MolassesDuration"];
        int magTrapStartTime = opticalPumpingStartTime + (int)Parameters["OpticalPumpingDuration"];
        int motRecaptureTime = magTrapStartTime + (int)Parameters["MagTrapDuration"];
        int imageTime = motRecaptureTime + (int)Parameters["MOTWaitBeforeImage"];

        // Add Analog Channels

        p.AddChannel("MOTCoilsCurrent");
        p.AddChannel("xShimCoilCurrent");
        p.AddChannel("yShimCoilCurrent");
        p.AddChannel("zShimCoilCurrent");
        p.AddChannel("rbCoolingIntensity");

        // B Field
        p.AddAnalogValue("MOTCoilsCurrent", 0, (double)Parameters["MOTCoilsCurrentValue"]);
        p.AddLinearRamp("MOTCoilsCurrent", motCompressStartTime, (int)Parameters["MOTCoilsCurrentRampDuration"], (double)Parameters["MOTCoilsCurrentRampEndValue"]);
        p.AddAnalogValue("MOTCoilsCurrent", molassesStartTime, 0.0);
        p.AddAnalogValue("MOTCoilsCurrent", magTrapStartTime, (double)Parameters["MOTCoilsCurrentMagTrapValue"]);
        p.AddAnalogValue("MOTCoilsCurrent", motRecaptureTime, (double)Parameters["MOTCoilsCurrentValue"]);
        p.AddAnalogValue("MOTCoilsCurrent", imageTime + 2000, 0.0); // Turn off MOT coils at the end of the sequence

        // Shim Fields
        p.AddAnalogValue("xShimCoilCurrent", 0, (double)Parameters["xShimLoadCurrent"]);
        p.AddAnalogValue("yShimCoilCurrent", 0, (double)Parameters["yShimLoadCurrent"]);
        p.AddAnalogValue("zShimCoilCurrent", 0, (double)Parameters["zShimLoadCurrent"]);

        // Cooling light intensity
        p.AddAnalogValue("rbCoolingIntensity", 0, (double)Parameters["rbCoolingMOTIntensity"]);
        p.AddLinearRamp("rbCoolingIntensity", motCompressStartTime, (int)Parameters["rbCoolingMOTIntensityRampDuration"], (double)Parameters["rbCoolingMOTIntensityRampEndValue"]);
        p.AddAnalogValue("rbCoolingIntensity", molassesStartTime, (double)Parameters["rbCoolingMolassesIntensity"]);
        p.AddAnalogValue("rbCoolingIntensity", molassesStartTime + 100, 1.79);
        p.AddAnalogValue("rbCoolingIntensity", molassesStartTime + 200, 1.65);
        p.AddAnalogValue("rbCoolingIntensity", molassesStartTime + 300, 1.48);
        p.AddAnalogValue("rbCoolingIntensity", molassesStartTime + 400, 1.397);
        p.AddAnalogValue("rbCoolingIntensity", molassesStartTime + 600, 1.316);
        p.AddAnalogValue("rbCoolingIntensity", motRecaptureTime, (double)Parameters["rbCoolingMOTIntensity"]);

        return p;
    }

}
