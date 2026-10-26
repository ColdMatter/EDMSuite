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
        Parameters["PatternLength"] = 50000;

        // Shim Coils
        Parameters["xShimLoadCurrent"] = -3.20;
        Parameters["yShimLoadCurrent"] = -1.35;
        Parameters["zShimLoadCurrent"] = 0.22;

        // Mag Trap
        Parameters["MagTrapField"] = 1.42;

        // Slowing Chirp, 5W ALS laser
        Parameters["SlowingChirpStartTime"] = 250;//360; //400;// 380;
        Parameters["SlowingChirpDuration"] = 1250;////1400;//1160; //1160

        // Timings
        Parameters["FreeExpTime"] = 1;
        Parameters["OpticalPumpingDuration"] = 100;
        Parameters["MagtrapDuration"] = 1;
        Parameters["BlueMOTDuration"] = 1000;
        Parameters["LambdaCoolingDuration"] = 300;
        Parameters["LambdaCooling2Duration"] = 300;

        Parameters["lambdaImageDuration"] = 1500;

    }

    // The whole shot, in ticks after the Q-switch. Called at the top of each Get...Pattern,
    // after scan values are in; the snippets read these times.
    private void SetTimeline()
    {
        Parameters["CMOTStartTime"] = (int)Parameters["CompressRampDownStartTime"];
        Parameters["CMOTEndTime"] = (int)Parameters["CMOTStartTime"] + (int)Parameters["CompressRampDownDuration"] + (int)Parameters["CompressRampDownHoldDuration"];
        Parameters["LambdaCooling1StartTime"] = Parameters["CMOTEndTime"];
        Parameters["LambdaCooling1EndTime"] = (int)Parameters["LambdaCooling1StartTime"] + (int)Parameters["LambdaCoolingDuration"];
        Parameters["BlueMOTStartTime"] = Parameters["LambdaCooling1EndTime"];
        Parameters["BlueMOTEndTime"] = (int)Parameters["BlueMOTStartTime"] + (int)Parameters["BlueMOTRampDuration"] + (int)Parameters["BlueMOTDuration"];
        Parameters["LambdaCooling2StartTime"] = Parameters["BlueMOTEndTime"];
        Parameters["LambdaCooling2EndTime"] = (int)Parameters["LambdaCooling2StartTime"] + (int)Parameters["LambdaCooling2Duration"];
        Parameters["LightOff1StartTime"] = Parameters["LambdaCooling2EndTime"];
        Parameters["OPStartTime"] = (int)Parameters["LambdaCooling2EndTime"] + (int)Parameters["OPGap"];
        Parameters["OPEndTime"] = (int)Parameters["OPStartTime"] + (int)Parameters["OpticalPumpingDuration"];
        Parameters["LambdaImageStartTime"] = Parameters["OPEndTime"];
        Parameters["LambdaImageEndTime"] = (int)Parameters["LambdaImageStartTime"] + (int)Parameters["lambdaImageDuration"];
        Parameters["LightOff2StartTime"] = Parameters["LambdaImageEndTime"]; // drop
        Parameters["BackgroundImageTime"] = (int)Parameters["LambdaImageEndTime"] + 10000;

        Parameters["bXSlowingShutterDuration"] = (int)Parameters["MagtrapDuration"] + 21000;
    }

    public override Dictionary<string, List<List<double>>> GetDDSPattern()
    {
        DDSPatternBuilder p = new DDSPatternBuilder();
        SetTimeline();

        new LoadMOT(p, Parameters);
        new CompressedMOT(p, Parameters);
        new LambdaCooling1(p, Parameters);
        new BlueMOT(p, Parameters);
        new LambdaCooling2(p, Parameters);
        new LightOff1(p, Parameters);

        // Image with the lambda-cooling light after optical pumping
        p.AddEvent("LambdaImage", (int)Parameters["LambdaImageStartTime"], CaFDDS.LambdaFrequencies(Parameters), CaFDDS.LambdaAmplitudes(Parameters));
        new LightOff2(p, Parameters);
        p.AddEvent("background", (int)Parameters["BackgroundImageTime"], CaFDDS.BlueMOTFrequencies(Parameters), CaFDDS.BlueMOTAmplitudes(Parameters));

        return p.Pattern;
    }

    public override PatternBuilder32 GetDigitalPattern()
    {
        PatternBuilder32 p = new PatternBuilder32();
        SetTimeline();
        int patternStartBeforeQ = (int)Parameters["TCLBlockStart"];

        new LoadMOT(p, Parameters);
        new OpticalPumping(p, Parameters);

        // CAMERA //
        p.Pulse(patternStartBeforeQ, (int)Parameters["LambdaImageEndTime"] - 1000, (int)Parameters["BlueMOTDuration"], "cameraTrigger");

        return p;
    }

    public override AnalogPatternBuilder GetAnalogPattern()
    {
        AnalogPatternBuilder p = new AnalogPatternBuilder((int)Parameters["PatternLength"]);
        SetTimeline();

        new LoadMOT(p, Parameters);
        new CompressedMOT(p, Parameters);
        new LambdaCooling1(p, Parameters);
        new BlueMOT(p, Parameters);
        new LambdaCooling2(p, Parameters);

        return p;
    }
}
