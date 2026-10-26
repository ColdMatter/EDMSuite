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
        Parameters["PatternLength"] = 1000000;

        // Camera
        Parameters["TweezerImageDur"] = 100;

        // Slowing Chirp, 5W ALS laser
        Parameters["SlowingChirpStartTime"] = 250;//360; //400;// 380;
        Parameters["SlowingChirpDuration"] = 1250;////1400;//1160; //1160

        // Timings
        Parameters["ShutterEdgeDur"] = (int)Parameters["MagtrapDuration"] + (int)Parameters["HandoverDur"] + (int)Parameters["MoveTime"] + (int)Parameters["HandoverDur"];
        Parameters["TweezerCoilsOff"] = 50;
        Parameters["FinalCoilsHold"] = 4000;


        // Typed into this script before snippets; the snippets read it as a parameter.
        Parameters["tMagTrapField"] = 0.48;
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
        Parameters["MagtrapStartTime"] = Parameters["LambdaCooling2EndTime"];
        Parameters["TransferHandoverStartTime"] = (int)Parameters["MagtrapStartTime"] + (int)Parameters["MagtrapDuration"];
        Parameters["TransportStartTime"] = (int)Parameters["TransferHandoverStartTime"] + (int)Parameters["HandoverDur"];

        // Tweezer chamber: tweezer coils off and lambda molasses for TweezerCoilsOff
        Parameters["TweezerHandoverStartTime"] = (int)Parameters["TransportStartTime"] + (int)Parameters["MoveTime"];
        Parameters["tMolassesStartTime"] = (int)Parameters["TweezerHandoverStartTime"] + (int)Parameters["HandoverDur"];
        Parameters["LightOff2StartTime"] = (int)Parameters["tMolassesStartTime"] + (int)Parameters["TweezerCoilsOff"];

        // Back to the MOT chamber
        Parameters["TransferHandoverBackStartTime"] = (int)Parameters["tMolassesStartTime"] + (int)Parameters["TweezerCoilsOff"];
        Parameters["TransportBackStartTime"] = (int)Parameters["TransferHandoverBackStartTime"] + (int)Parameters["HandoverDur"];
        Parameters["MOTHandoverBackStartTime"] = (int)Parameters["TransportBackStartTime"] + (int)Parameters["MoveTime"];
        Parameters["MOTHoldStartTime"] = (int)Parameters["MOTHandoverBackStartTime"] + (int)Parameters["HandoverDur"];
        Parameters["CoilsOffTime"] = (int)Parameters["MOTHoldStartTime"] + (int)Parameters["FinalCoilsHold"];

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

        // Lambda molasses in the tweezer chamber. Not LambdaCooling: the MOT coils are already off.
        p.AddEvent("TweezerLambdaMolasses", (int)Parameters["tMolassesStartTime"], CaFDDS.LambdaFrequencies(Parameters), CaFDDS.LambdaAmplitudes(Parameters));
        new LightOff2(p, Parameters);

        p.AddEvent("image", (int)Parameters["MOTHoldStartTime"] + 250, CaFDDS.BlueMOTFrequencies(Parameters), CaFDDS.BlueMOTAmplitudes(Parameters));

        return p.Pattern;
    }

    public override PatternBuilder32 GetDigitalPattern()
    {
        PatternBuilder32 p = new PatternBuilder32();
        SetTimeline();
        int patternStartBeforeQ = (int)Parameters["TCLBlockStart"];
        int blueMOTEnd = (int)Parameters["BlueMOTEndTime"];

        new LoadMOT(p, Parameters);

        // CAMERA //
        p.Pulse(patternStartBeforeQ, blueMOTEnd - (int)Parameters["Frame0TriggerDuration"], (int)Parameters["Frame0TriggerDuration"], "cameraTrigger"); //camera trigger for first frame
        p.Pulse(patternStartBeforeQ, (int)Parameters["MOTHoldStartTime"], (int)Parameters["FinalCoilsHold"], "cameraTrigger"); //Tweezer camera trig

        // MOT Shutters
        p.Pulse(patternStartBeforeQ, blueMOTEnd - 1600, (int)Parameters["ShutterEdgeDur"], "MOT1Shutter");
        p.Pulse(patternStartBeforeQ, blueMOTEnd - 600, (int)Parameters["ShutterEdgeDur"] - 950, "MOT2Shutter");
        p.Pulse(patternStartBeforeQ, blueMOTEnd - 800, (int)Parameters["ShutterEdgeDur"] - 500, "MOT3Shutter");

        // Tweezer Chamber CaF Light Shutter
        p.AddEdge("TweezerChamberMOTShutter", patternStartBeforeQ + (int)Parameters["tMolassesStartTime"] - 1000, true); //Shutter CaF light to tweezer chamber - OPEN
        p.AddEdge("TweezerChamberMOTShutter", patternStartBeforeQ + (int)Parameters["TransferHandoverBackStartTime"], false);

        // Transport
        p.AddEdge("transportTrack", 0, true);
        p.DownPulse(patternStartBeforeQ, (int)Parameters["TransportStartTime"], (int)Parameters["TransportEdgeDur"] + (int)Parameters["TransportCoilsAtTweezersDur"], "transportTrack");

        // YaG Compensating Shots
        p.Pulse(patternStartBeforeQ, (int)Parameters["PatternLength"] - (int)Parameters["YagPreFire1"] - (int)Parameters["FlashToQ"], (int)Parameters["QSwitchPulseDuration"], "flashLamp");
        p.Pulse(patternStartBeforeQ, (int)Parameters["PatternLength"] - (int)Parameters["YagPreFire1"], (int)Parameters["QSwitchPulseDuration"], "qSwitch");

        p.Pulse(patternStartBeforeQ, (int)Parameters["PatternLength"] - (int)Parameters["YagPreFire"] - (int)Parameters["FlashToQ"], (int)Parameters["QSwitchPulseDuration"], "flashLamp");
        p.Pulse(patternStartBeforeQ, (int)Parameters["PatternLength"] - (int)Parameters["YagPreFire"], (int)Parameters["QSwitchPulseDuration"], "qSwitch");

        return p;
    }

    public override AnalogPatternBuilder GetAnalogPattern()
    {
        AnalogPatternBuilder p = new AnalogPatternBuilder((int)Parameters["PatternLength"]);
        SetTimeline();
        int handoverBack = (int)Parameters["TransferHandoverBackStartTime"];
        int motHandoverBack = (int)Parameters["MOTHandoverBackStartTime"];
        int coilsOff = (int)Parameters["CoilsOffTime"];

        new LoadMOT(p, Parameters);
        p.AddChannel("transferCoils");
        p.AddChannel("TweezerCoils");

        // MOT chamber
        new CompressedMOT(p, Parameters);
        new LambdaCooling1(p, Parameters);
        new BlueMOT(p, Parameters);
        new LambdaCooling2(p, Parameters);
        new Magtrap(p, Parameters);
        new HandoverToTransferCoils(p, Parameters);

        // Tweezer chamber
        new HandoverToTweezerCoils(p, Parameters);
        p.AddAnalogValue("TweezerCoils", (int)Parameters["tMolassesStartTime"], 0.0); // tweezer coils off for molasses
        p.AddAnalogValue("TweezerCoils", handoverBack - 1, (double)Parameters["tMagTrapField"]); // back on, for the ramp down to start from

        // Hand back to the transfer coils, then to the MOT coils
        p.AddLinearRamp("transferCoils", handoverBack, (int)Parameters["HandoverDur"], 10.0);
        p.AddLinearRamp("TweezerCoils", handoverBack, (int)Parameters["HandoverDur"], 0.0);
        p.AddLinearRamp("MOTCoilsCurrent", motHandoverBack, (int)Parameters["HandoverDur"], (double)Parameters["BlueMOTField"]);
        p.AddLinearRamp("transferCoils", motHandoverBack, (int)Parameters["HandoverDur"], (double)Parameters["TransferCoilsOffValue"]);
        p.AddAnalogValue("MOTCoilsCurrent", (int)Parameters["MOTHoldStartTime"], (double)Parameters["BlueMOTField"]); // hold in MOT coils

        // All coils off
        p.AddAnalogValue("TweezerCoils", coilsOff, 0.0);
        p.AddAnalogValue("MOTCoilsCurrent", coilsOff, 0.0);
        p.AddAnalogValue("transferCoils", coilsOff, 0.0);

        return p;
    }
}
