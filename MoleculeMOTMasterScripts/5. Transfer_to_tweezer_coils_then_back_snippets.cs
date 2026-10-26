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

        //PMT

        // Shim fields

        // SLOWING //

        // Slowing Chirp
        /*
        Parameters["SlowingChirpStartTime"] = 500;//360; //400;// 380;
        Parameters["SlowingChirpDuration"] = 1100;////1400;//1160; //1160
        Parameters["SlowingChirpStartValue"] = 0.0;//0.0
        Parameters["SlowingChirpEndValue"] = -1.25; // -0.5 is 480MHz
        */

        // Slowing Chirp, 5W ALS laser
        Parameters["SlowingChirpStartTime"] = 250;//360; //400;// 380;
        Parameters["SlowingChirpDuration"] = 1250;////1400;//1160; //1160

        // Slowing AOMS

        // Slowing B field

        //Parameters["slowingCoilsOffTime"] = (int)Parameters["slowingAOMOffStart"]; // 1500;

        // MOT LOAD //

        // MOT B field

        // MOT Sidebands

        //freqs

        //amps for max optical power. dont go higher the amplifiers will saturate

        // CMOT //

        // magtrap //

        // INTENSITY RAMP DOWN //

        // Ramp end rf amps
        //Parameters["RampEndAmpDDS1"] = 0.091; // 10%
        //Parameters["RampEndAmpDDS2"] = 0.1; // 10%
        //Parameters["RampEndAmpDDS3"] = 0.114; // 10%
        //Parameters["RampEndAmpDDS4"] = 0.036; // 10%

        //// Ramp end rf amps
        //Parameters["RampEndAmpDDS1"] = 0.08; // 7%
        //Parameters["RampEndAmpDDS2"] = 0.077; // 7%
        //Parameters["RampEndAmpDDS3"] = 0.1; // 7%
        //Parameters["RampEndAmpDDS4"] = 0.032; // 7%

        // Ramp end rf amps
        /*
        Parameters["RampEndAmpDDS1"] = 0.07; // 5%
        Parameters["RampEndAmpDDS2"] = 0.056; // 5%
        Parameters["RampEndAmpDDS3"] = 0.086; // 5%
        Parameters["RampEndAmpDDS4"] = 0.027; // 5%
        */

        // ramp down mot parameters 15%
        /*
        Parameters["RampEndAmpDDS1"] = 0.105; //15%
        Parameters["RampAmplitudeDDS1"] = -0.00014500000000000003; //15%
        Parameters["RampEndAmpDDS2"] = 0.128; //15%
        Parameters["RampAmplitudeDDS2"] = -0.000472; //15%
        Parameters["RampEndAmpDDS3"] = 0.131; //15%
        Parameters["RampAmplitudeDDS3"] = -0.00021899999999999998; //15%
        Parameters["RampEndAmpDDS4"] = 0.042; //15%
        Parameters["RampAmplitudeDDS4"] = -5.8e-05; //15%
        */

        //12% from lambda measurement

        // lambda rf amps

        // Ramp slopes
        //Parameters["RampAmplitudeDDS1"] = ((double)Parameters["RampEndAmpDDS1"] - (double)Parameters["MOTAmpDDS1"]) / ((double)(int)Parameters["v0IntensityRampDuration"]);
        //Parameters["RampAmplitudeDDS2"] = ((double)Parameters["RampEndAmpDDS2"] - (double)Parameters["MOTAmpDDS2"]) / ((double)(int)Parameters["v0IntensityRampDuration"]);
        //Parameters["RampAmplitudeDDS3"] = ((double)Parameters["RampEndAmpDDS3"] - (double)Parameters["MOTAmpDDS3"]) / ((double)(int)Parameters["v0IntensityRampDuration"]);
        //Parameters["RampAmplitudeDDS4"] = ((double)Parameters["RampEndAmpDDS4"] - (double)Parameters["MOTAmpDDS4"]) / ((double)(int)Parameters["v0IntensityRampDuration"]);

        //Conveyor belt frequency
        // Parameters["FreqCVB2"] = 163.20;
        // Parameters["FreqCVB2"] = 0.0;

        // END OF PATTERN //
        Parameters["TweezerCoilsHold"] = 10000;
        Parameters["FinalCoilsHold"] = 10000;
        Parameters["ShutterEdgeDur"] = (int)Parameters["MagtrapDuration"] + (int)Parameters["HandoverDur"] + (int)Parameters["MoveTime"] + (int)Parameters["HandoverDur"] + (int)Parameters["MoveTime"];




        // Typed into this script before snippets; the snippets read it as a parameter.
        Parameters["tMagTrapField"] = 1.8;
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

        // Tweezer chamber: hold in the tweezer coils
        Parameters["TweezerHandoverStartTime"] = (int)Parameters["TransportStartTime"] + (int)Parameters["MoveTime"];
        Parameters["tMagtrapStartTime"] = (int)Parameters["TweezerHandoverStartTime"] + (int)Parameters["HandoverDur"];

        // Back to the MOT chamber
        Parameters["TransferHandoverBackStartTime"] = (int)Parameters["tMagtrapStartTime"] + (int)Parameters["TweezerCoilsHold"];
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

        p.AddEvent("image", (int)Parameters["MOTHoldStartTime"] + 250, CaFDDS.BlueMOTFrequencies(Parameters), CaFDDS.BlueMOTAmplitudes(Parameters));

        return p.Pattern;
    }

    public override PatternBuilder32 GetDigitalPattern()
    {
        PatternBuilder32 p = new PatternBuilder32();
        SetTimeline();
        int patternStartBeforeQ = (int)Parameters["TCLBlockStart"];
        int blueMOTEnd = (int)Parameters["BlueMOTEndTime"];
        int motHandoverBack = (int)Parameters["MOTHandoverBackStartTime"];

        new LoadMOT(p, Parameters);

        // CAMERA //
        p.Pulse(patternStartBeforeQ, (int)Parameters["Frame0Trigger"], (int)Parameters["Frame0TriggerDuration"], "cameraTrigger"); //camera trigger for first frame
        p.Pulse(patternStartBeforeQ, (int)Parameters["MOTHoldStartTime"] + 250, (int)Parameters["FinalCoilsHold"], "cameraTrigger"); //Tweezer camera trig

        // CaF MOT Shutters
        p.AddEdge("MOT1Shutter", patternStartBeforeQ + blueMOTEnd - 1600, true);
        p.AddEdge("MOT2Shutter", patternStartBeforeQ + blueMOTEnd - 600, true);
        p.AddEdge("MOT3Shutter", patternStartBeforeQ + blueMOTEnd - 800, true);

        p.AddEdge("MOT1Shutter", patternStartBeforeQ + motHandoverBack - 1600, false);
        p.AddEdge("MOT2Shutter", patternStartBeforeQ + motHandoverBack - 600, false);
        p.AddEdge("MOT3Shutter", patternStartBeforeQ + motHandoverBack - 800, false);

        // Transport
        p.AddEdge("transportTrack", 0, true);
        p.AddEdge("transportTrack", patternStartBeforeQ + (int)Parameters["TransportStartTime"], false);
        p.AddEdge("transportTrack", patternStartBeforeQ + (int)Parameters["TransportBackStartTime"], true);

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
        p.AddAnalogValue("TweezerCoils", (int)Parameters["tMagtrapStartTime"], (double)Parameters["tMagTrapField"]); // hold in tweezer coils

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
