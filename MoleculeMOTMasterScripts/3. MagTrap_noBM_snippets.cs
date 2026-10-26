using MOTMaster;
using MOTMaster.SnippetLibrary;
using MOTMaster.SnippetLibrary.CaF;

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
        LoadGlobalParameters();
        Parameters["PatternLength"] = 55000;

        // Camera

        //PMT

        // Shim fields
        Parameters["xShimLoadCurrent"] = -1.85;
        Parameters["yShimLoadCurrent"] = -3.0;
        Parameters["zShimLoadCurrent"] = 0.22;

        // SLOWING //

        // Slowing Chirp, 5W ALS laser
        Parameters["SlowingChirpStartTime"] = 250;//360; //400;// 380;
        Parameters["SlowingChirpDuration"] = 1250;////1400;//1160; //1160

        // Slowing AOMS

        // Slowing B field

        // MOT LOAD //

        // MOT B field

        // MOT Sidebands

        //freqs

        //amps for max optical power. dont go higher the amplifiers will saturate

        // CMOT //

        // magtrap //

        //12% from lambda measurement

        // lambda rf amps

        Parameters["FreeExpTime"] = 1;

        // END OF PATTERN //

    }

    // The whole shot, in ticks after the Q-switch. Called at the top of each Get...Pattern,
    // after scan values are in; the snippets read these times.
    private void SetTimeline()
    {
        Parameters["CMOTStartTime"] = (int)Parameters["CompressRampDownStartTime"];
        Parameters["CMOTEndTime"] = (int)Parameters["CMOTStartTime"] + (int)Parameters["CompressRampDownDuration"] + (int)Parameters["CompressRampDownHoldDuration"];
        Parameters["LambdaCooling1StartTime"] = Parameters["CMOTEndTime"];
        Parameters["LambdaCooling1EndTime"] = (int)Parameters["LambdaCooling1StartTime"] + (int)Parameters["LambdaCoolingDuration"];
        Parameters["MagtrapStartTime"] = Parameters["LambdaCooling1EndTime"];
        Parameters["MagtrapEndTime"] = (int)Parameters["MagtrapStartTime"] + (int)Parameters["MagtrapDuration"];
        Parameters["ImageTime"] = (int)Parameters["MagtrapEndTime"] + (int)Parameters["FreeExpTime"];

        Parameters["bXSlowingShutterDuration"] = (int)Parameters["MagtrapDuration"] + 21000;
    }

    public override Dictionary<string, List<List<double>>> GetDDSPattern()
    {
        DDSPatternBuilder p = new DDSPatternBuilder();
        SetTimeline();

        new LoadMOT(p, Parameters);
        new CompressedMOT(p, Parameters);
        new LambdaCooling1(p, Parameters);

        // Light off for the magtrap. Unlike LightOff, at the MOT frequencies, not the resonance ones.
        p.AddEvent("Magtrap", (int)Parameters["MagtrapStartTime"], CaFDDS.MOTFrequencies(Parameters), CaFDDS.Same(Parameters, "LightoffDDS1"));

        p.AddEvent("image", (int)Parameters["ImageTime"], CaFDDS.MOTFrequencies(Parameters), CaFDDS.MOTAmplitudes(Parameters));

        return p.Pattern;
    }

    public override PatternBuilder32 GetDigitalPattern()
    {
        PatternBuilder32 p = new PatternBuilder32();
        SetTimeline();
        int patternStartBeforeQ = (int)Parameters["TCLBlockStart"];
        int lambdaCoolingStart = patternStartBeforeQ + (int)Parameters["LambdaCooling1StartTime"];

        new LoadMOT(p, Parameters);

        // CAMERA //
        p.Pulse(patternStartBeforeQ, (int)Parameters["Frame0Trigger"], (int)Parameters["Frame0TriggerDuration"], "cameraTrigger"); //camera trigger for first frame
        p.Pulse(patternStartBeforeQ, (int)Parameters["ImageTime"] + 200, (int)Parameters["Frame0TriggerDuration"], "cameraTrigger");  //camera trigger for MOT recap

        // MOT Shutters
        p.Pulse(0, lambdaCoolingStart - 1600, (int)Parameters["MagtrapDuration"], "MOT1Shutter");
        p.Pulse(0, lambdaCoolingStart - 600, (int)Parameters["MagtrapDuration"] - 950, "MOT2Shutter");
        p.Pulse(0, lambdaCoolingStart - 800, (int)Parameters["MagtrapDuration"] - 500, "MOT3Shutter");

        // YaG Compensating Shots
        p.Pulse(patternStartBeforeQ, (int)Parameters["PatternLength"] - (int)Parameters["YagPreFire"] - (int)Parameters["FlashToQ"], (int)Parameters["QSwitchPulseDuration"], "flashLamp");
        p.Pulse(patternStartBeforeQ, (int)Parameters["PatternLength"] - (int)Parameters["YagPreFire"], (int)Parameters["QSwitchPulseDuration"], "qSwitch");

        return p;
    }

    public override AnalogPatternBuilder GetAnalogPattern()
    {
        AnalogPatternBuilder p = new AnalogPatternBuilder((int)Parameters["PatternLength"]);
        SetTimeline();

        new LoadMOT(p, Parameters);
        new CompressedMOT(p, Parameters);
        new LambdaCooling1(p, Parameters);
        new Magtrap(p, Parameters);

        // In-MOT imaging
        p.AddAnalogValue("MOTCoilsCurrent", (int)Parameters["ImageTime"], (double)Parameters["MOTCoilsCurrentValue"]);
        p.AddAnalogValue("MOTCoilsCurrent", (int)Parameters["ImageTime"] + (int)Parameters["Frame0TriggerDuration"] + 500, (double)Parameters["MOTCoilsOffValue"]);

        return p;
    }
}
