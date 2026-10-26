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
        Parameters["PatternLength"] = 700000;

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
        Parameters["Magtrap1Duration"] = 1000;
        Parameters["BlueMOTDuration"] = 1000;
        Parameters["LambdaCoolingDuration"] = 300;
        Parameters["LambdaCooling2Duration"] = 300;

        Parameters["HandoverDur"] = 10000;
        Parameters["MoveDuration"] = 200000;
        Parameters["tMagtrapDuration"] = 200000;
        Parameters["Magtrap2Duration"] = 1000;

        // Typed into this script before snippets; the snippets read them as parameters.
        Parameters["tMagTrapField"] = 1.8;
        Parameters["TransferCoilsOffValue"] = -0.1;
    }

    // The whole shot, in ticks after the Q-switch. Called at the top of each Get...Pattern,
    // after scan values are in; the snippets read these times.
    private void SetTimeline()
    {
        // MOT chamber
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
        Parameters["MagtrapStartTime"] = Parameters["OPEndTime"];
        Parameters["TransferHandoverStartTime"] = (int)Parameters["MagtrapStartTime"] + (int)Parameters["Magtrap1Duration"];
        Parameters["TransportStartTime"] = (int)Parameters["TransferHandoverStartTime"] + (int)Parameters["HandoverDur"];

        // Tweezer chamber
        Parameters["TweezerHandoverStartTime"] = (int)Parameters["TransportStartTime"] + (int)Parameters["MoveDuration"];
        Parameters["tMagtrapStartTime"] = (int)Parameters["TweezerHandoverStartTime"] + (int)Parameters["HandoverDur"];

        // Back in the MOT chamber: magtrap, then recapture in a blue MOT
        Parameters["TransferHandoverBackStartTime"] = (int)Parameters["tMagtrapStartTime"] + (int)Parameters["tMagtrapDuration"];
        Parameters["TransportBackStartTime"] = (int)Parameters["TransferHandoverBackStartTime"] + (int)Parameters["HandoverDur"];
        Parameters["MOTHandoverBackStartTime"] = (int)Parameters["TransportBackStartTime"] + (int)Parameters["MoveDuration"];
        Parameters["Magtrap2EndTime"] = (int)Parameters["MOTHandoverBackStartTime"] + (int)Parameters["HandoverDur"] + (int)Parameters["Magtrap2Duration"];
        Parameters["BlueMOT2StartTime"] = (int)Parameters["Magtrap2EndTime"] + (int)Parameters["FreeExpTime"];
        Parameters["BlueMOT2EndTime"] = (int)Parameters["BlueMOT2StartTime"] + (int)Parameters["BlueMOTRampDuration"] + (int)Parameters["BlueMOTDuration"];
        Parameters["LightOff2StartTime"] = Parameters["BlueMOT2EndTime"];
        Parameters["BackgroundTime"] = (int)Parameters["BlueMOT2EndTime"] + 10000;

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
        new BlueMOT2(p, Parameters);
        new LightOff2(p, Parameters);

        p.AddEvent("background", (int)Parameters["BackgroundTime"], CaFDDS.BlueMOTFrequencies(Parameters), CaFDDS.BlueMOTAmplitudes(Parameters));

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
        p.Pulse(patternStartBeforeQ, (int)Parameters["BlueMOTEndTime"] - (int)Parameters["BlueMOTDuration"], (int)Parameters["BlueMOTDuration"] / 2, "cameraTrigger"); //camera trigger for first frame
        p.Pulse(patternStartBeforeQ, (int)Parameters["BlueMOT2EndTime"] - (int)Parameters["BlueMOTDuration"], (int)Parameters["BlueMOTDuration"] / 2, "cameraTrigger"); //Tweezer camera trig

        // MOT Shutters
        p.AddEdge("MOT1Shutter", patternStartBeforeQ + (int)Parameters["LambdaCooling2EndTime"], true);
        p.AddEdge("MOT2Shutter", patternStartBeforeQ + (int)Parameters["LambdaCooling2EndTime"], true);
        p.AddEdge("MOT3Shutter", patternStartBeforeQ + (int)Parameters["LambdaCooling2EndTime"], true);

        p.AddEdge("MOT1Shutter", patternStartBeforeQ + (int)Parameters["Magtrap2EndTime"] - 2000, false);
        p.AddEdge("MOT2Shutter", patternStartBeforeQ + (int)Parameters["Magtrap2EndTime"] - 2000, false);
        p.AddEdge("MOT3Shutter", patternStartBeforeQ + (int)Parameters["Magtrap2EndTime"] - 2000, false);

        // Transport
        p.AddEdge("transportTrack", 0, true);
        p.AddEdge("transportTrack", patternStartBeforeQ + (int)Parameters["TransportStartTime"], false);
        p.AddEdge("transportTrack", patternStartBeforeQ + (int)Parameters["TransportBackStartTime"], true);

        return p;
    }

    public override AnalogPatternBuilder GetAnalogPattern()
    {
        AnalogPatternBuilder p = new AnalogPatternBuilder((int)Parameters["PatternLength"]);
        SetTimeline();
        int handoverBack = (int)Parameters["TransferHandoverBackStartTime"];
        int motHandoverBack = (int)Parameters["MOTHandoverBackStartTime"];

        new LoadMOT(p, Parameters);
        p.AddChannel("transferCoils");
        p.AddChannel("TweezerCoils");

        // MOT chamber
        new CompressedMOT(p, Parameters);
        new LambdaCooling1(p, Parameters);
        new BlueMOT(p, Parameters);
        new LambdaCooling2(p, Parameters);
        p.AddAnalogValue("MOTCoilsCurrent", (int)Parameters["MagtrapStartTime"], (double)Parameters["BlueMOTField"]); // magtrap
        new HandoverToTransferCoils(p, Parameters);

        // Tweezer chamber
        new HandoverToTweezerCoils(p, Parameters);

        // Hand back to the transfer coils, then to the MOT coils
        p.AddLinearRamp("transferCoils", handoverBack, (int)Parameters["HandoverDur"], 10.0);
        p.AddLinearRamp("TweezerCoils", handoverBack, (int)Parameters["HandoverDur"], 0.0);
        p.AddLinearRamp("MOTCoilsCurrent", motHandoverBack, (int)Parameters["HandoverDur"], (double)Parameters["BlueMOTField"]);
        p.AddLinearRamp("transferCoils", motHandoverBack, (int)Parameters["HandoverDur"], (double)Parameters["TransferCoilsOffValue"]);

        // Reload into blue MOT
        p.AddAnalogValue("MOTCoilsCurrent", (int)Parameters["Magtrap2EndTime"], (double)Parameters["MOTCoilsOffValue"]);
        new BlueMOT2(p, Parameters);
        p.AddAnalogValue("MOTCoilsCurrent", (int)Parameters["BlueMOT2EndTime"], (double)Parameters["MOTCoilsOffValue"]); // off for background

        return p;
    }
}
