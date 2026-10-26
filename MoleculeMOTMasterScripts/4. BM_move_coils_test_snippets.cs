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
        Parameters["PatternLength"] = 600000;

        // Camera
        Parameters["Frame0TriggerDuration"] = 10000;

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
        Parameters["SlowingChirpDuration"] = 1200;////1400;//1160; //1160

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

        Parameters["FreeExpTime"] = 6000;

        Parameters["TransportEdgeDur"] = 260000;

        // END OF PATTERN //

        Parameters["MoveTime"] = 160000; //DONT CHANGE- THIS IS THE TIME IT TAKES FROM TRANSPORT TRACK MOVE TRIG TO TWEEZER CHAMBER ARRIVAL
        //Parameters["ImageDelay"] = 340000;

        Parameters["LoadDur"] = 5000;


        // Typed into this script before snippets; the snippets read them as parameters.
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

        // Tweezer chamber
        Parameters["TweezerHandoverStartTime"] = (int)Parameters["TransportStartTime"] + (int)Parameters["MoveTime"];
        Parameters["TweezerLoadStartTime"] = (int)Parameters["TweezerHandoverStartTime"] + (int)Parameters["HandoverDur"];
        Parameters["TweezerLoadEndTime"] = (int)Parameters["TweezerLoadStartTime"] + (int)Parameters["LoadDur"];
        Parameters["ImageTime"] = (int)Parameters["TweezerLoadEndTime"] + (int)Parameters["FreeExpTime"];

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

        // Tweezer chamber: DDS1 at SF, the other channels at resonance and off
        double[] tweezerFrequencies = {
            (double)Parameters["SF"], (double)Parameters["ResonanceDDS2"],
            (double)Parameters["ResonanceDDS3"], (double)Parameters["ResonanceDDS4"] };
        double[] tweezerAmplitudes = {
            (double)Parameters["MOTAmpDDS1"], (double)Parameters["LightoffDDS1"],
            (double)Parameters["LightoffDDS1"], (double)Parameters["LightoffDDS1"] };
        p.AddEvent("LoadTweezer", (int)Parameters["TweezerLoadStartTime"], tweezerFrequencies, tweezerAmplitudes);
        p.AddEvent("FreeFall", (int)Parameters["TweezerLoadEndTime"], tweezerFrequencies, CaFDDS.Same(Parameters, "LightoffDDS1"));
        p.AddEvent("Image", (int)Parameters["ImageTime"], tweezerFrequencies, tweezerAmplitudes);

        return p.Pattern;
    }

    public override PatternBuilder32 GetDigitalPattern()
    {
        PatternBuilder32 p = new PatternBuilder32();
        SetTimeline();
        int patternStartBeforeQ = (int)Parameters["TCLBlockStart"];

        new LoadMOT(p, Parameters);
        p.AddEdge("test10", 0, true); //Shutter CaF light to tweezer chamber - OPEN

        // MOT Shutters
        p.Pulse(patternStartBeforeQ, (int)Parameters["BlueMOTEndTime"] - 1600, (int)Parameters["MagtrapDuration"], "MOT1Shutter");
        p.Pulse(patternStartBeforeQ, (int)Parameters["BlueMOTEndTime"] - 600, (int)Parameters["MagtrapDuration"] - 950, "MOT2Shutter");
        p.Pulse(patternStartBeforeQ, (int)Parameters["BlueMOTEndTime"] - 800, (int)Parameters["MagtrapDuration"] - 500, "MOT3Shutter");

        // Transport
        p.AddEdge("transportTrack", 0, true);
        p.DownPulse(patternStartBeforeQ, (int)Parameters["TransportStartTime"], (int)Parameters["TransportEdgeDur"], "transportTrack");

        // CAMERA //
        p.Pulse(patternStartBeforeQ, (int)Parameters["ImageTime"], (int)Parameters["Frame0TriggerDuration"], "rbAbsImgCamTrig");

        return p;
    }

    public override AnalogPatternBuilder GetAnalogPattern()
    {
        AnalogPatternBuilder p = new AnalogPatternBuilder((int)Parameters["PatternLength"]);
        SetTimeline();

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
        p.AddAnalogValue("TweezerCoils", (int)Parameters["TweezerLoadStartTime"], 0.0);

        return p;
    }
}
