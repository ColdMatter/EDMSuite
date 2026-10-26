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

        // Camera
        Parameters["TweezerImageDur"] = 4000;

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
        Parameters["SlowingChirpStartTime"] = 200;//360; //400;// 380;
        Parameters["SlowingChirpDuration"] = 1500;////1400;//1160; //1160

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

        Parameters["MagtrapDuration"] = 10;

        // END OF PATTERN //

        Parameters["ExternalHold"] = 1000;

        Parameters["ShutterEdgeDur"] = (int)Parameters["MagtrapDuration"] + (int)Parameters["HandoverDur"] + (int)Parameters["ExternalHold"] + (int)Parameters["HandoverDur"];

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
        Parameters["MagtrapStartTime"] = Parameters["LambdaCooling2EndTime"];
        Parameters["TransferHandoverStartTime"] = (int)Parameters["MagtrapStartTime"] + (int)Parameters["MagtrapDuration"];
        Parameters["HandbackStartTime"] = (int)Parameters["TransferHandoverStartTime"] + (int)Parameters["HandoverDur"] + (int)Parameters["ExternalHold"];
        Parameters["ImageTime"] = (int)Parameters["HandbackStartTime"] + (int)Parameters["HandoverDur"];

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

        // Image with the blue-MOT tones
        p.AddEvent("Image", (int)Parameters["ImageTime"], CaFDDS.BlueMOTFrequencies(Parameters), CaFDDS.BlueMOTAmplitudes(Parameters));

        return p.Pattern;
    }

    public override PatternBuilder32 GetDigitalPattern()
    {
        PatternBuilder32 p = new PatternBuilder32();
        SetTimeline();
        int patternStartBeforeQ = (int)Parameters["TCLBlockStart"];
        int BlueMOTEnd = patternStartBeforeQ + (int)Parameters["BlueMOTEndTime"];

        new LoadMOT(p, Parameters);

        // CAMERA //
        p.Pulse(patternStartBeforeQ, (int)Parameters["Frame0Trigger"], (int)Parameters["Frame0TriggerDuration"], "cameraTrigger"); //camera trigger for first frame
        p.Pulse(patternStartBeforeQ, (int)Parameters["ImageTime"], (int)Parameters["TweezerImageDur"], "cameraTrigger"); //Tweezer camera trig

        // MOT Shutters
        p.Pulse(0, BlueMOTEnd - 1600, (int)Parameters["ShutterEdgeDur"], "MOT1Shutter");
        p.Pulse(0, BlueMOTEnd - 600, (int)Parameters["ShutterEdgeDur"] - 950, "MOT2Shutter");
        p.Pulse(0, BlueMOTEnd - 800, (int)Parameters["ShutterEdgeDur"] - 500, "MOT3Shutter");

        // Transport
        p.AddEdge("transportTrack", 0, true);

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

        new LoadMOT(p, Parameters);
        p.AddChannel("transferCoils");
        p.AddChannel("TweezerCoils");

        new CompressedMOT(p, Parameters);
        new LambdaCooling1(p, Parameters);
        new BlueMOT(p, Parameters);
        new LambdaCooling2(p, Parameters);
        new Magtrap(p, Parameters);
        new HandoverToTransferCoils(p, Parameters);

        // Hand back to the MOT coils
        p.AddLinearRamp("transferCoils", (int)Parameters["HandbackStartTime"], (int)Parameters["HandoverDur"], 0.0);
        p.AddLinearRamp("MOTCoilsCurrent", (int)Parameters["HandbackStartTime"], (int)Parameters["HandoverDur"], (double)Parameters["MOTCoilsCurrentValue"]);

        // Hold in MOT coils
        p.AddAnalogValue("MOTCoilsCurrent", (int)Parameters["ImageTime"], (double)Parameters["MOTCoilsCurrentValue"]);
        p.AddAnalogValue("MOTCoilsCurrent", (int)Parameters["ImageTime"] + (int)Parameters["TweezerImageDur"], 0.0);

        return p;
    }
}
