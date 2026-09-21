using MOTMaster;
using MOTMaster.SnippetLibrary;

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
        Parameters["SlowingChirpDuration"] = 1400;////1400;//1160; //1160

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

        Parameters["ShutterEdgeDur"] = (int)Parameters["MagtrapDuration"] + (int)Parameters["HandoverDur"] + (int)Parameters["MoveTime"] + (int)Parameters["HandoverDur"];

    }

    public override Dictionary<string, List<List<double>>> GetDDSPattern()
    {
        Dictionary<string, List<List<double>>> p = new Dictionary<string, List<List<double>>>();

        //CAF CHAMBER
        int CompressRampDownStartTime = (int)Parameters["CompressRampDownStartTime"];
        int CompressRampDownEndTime = CompressRampDownStartTime + (int)Parameters["CompressRampDownDuration"];
        int lambdaCoolingStart = CompressRampDownEndTime + (int)Parameters["CompressRampDownHoldDuration"];
        int BlueMOTRampStart = lambdaCoolingStart + (int)Parameters["LambdaCoolingDuration"];
        int BlueMOTRampEnd = BlueMOTRampStart + (int)Parameters["BlueMOTRampDuration"];
        int BlueMOTEnd = BlueMOTRampEnd + (int)Parameters["BlueMOTDuration"];
        int lambdaCooling2end = BlueMOTEnd + (int)Parameters["LambdaCooling2Duration"];
        int HandoverStart = lambdaCooling2end + (int)Parameters["MagtrapDuration"];
        int MoveStart = HandoverStart + (int)Parameters["HandoverDur"];
        //TWEEZER CHAMBER
        int TweezerHandoverStart = MoveStart + (int)Parameters["MoveTime"];
        int TweezerHandoverEnd = TweezerHandoverStart + (int)Parameters["HandoverDur"];
        int ImageTweezerChamber = TweezerHandoverEnd + (int)Parameters["TweezerCoilsHold"];

        addDDSPattern(p, "MOT", 0,
            (double)Parameters["MOTFreqDDS1"], (double)Parameters["MOTFreqDDS2"], (double)Parameters["MOTFreqDDS3"], (double)Parameters["MOTFreqDDS4"],
            (double)Parameters["MOTAmpDDS1"], (double)Parameters["MOTAmpDDS2"], (double)Parameters["MOTAmpDDS3"], (double)Parameters["MOTAmpDDS4"]);

        addDDSPattern(p, "RampStart", CompressRampDownStartTime,
            (double)Parameters["MOTFreqDDS1"], (double)Parameters["MOTFreqDDS2"], (double)Parameters["MOTFreqDDS3"], (double)Parameters["MOTFreqDDS4"],
            (double)Parameters["MOTAmpDDS1"], (double)Parameters["MOTAmpDDS2"], (double)Parameters["MOTAmpDDS3"], (double)Parameters["MOTAmpDDS4"],
            0.0, 0.0, 0.0, 0.0, (double)Parameters["RampAmplitudeDDS1"], (double)Parameters["RampAmplitudeDDS2"], (double)Parameters["RampAmplitudeDDS3"], (double)Parameters["RampAmplitudeDDS4"]);

        addDDSPattern(p, "RampEnd", CompressRampDownEndTime,
            (double)Parameters["MOTFreqDDS1"], (double)Parameters["MOTFreqDDS2"], (double)Parameters["MOTFreqDDS3"], (double)Parameters["MOTFreqDDS4"],
            (double)Parameters["RampEndAmpDDS1"], (double)Parameters["RampEndAmpDDS2"], (double)Parameters["RampEndAmpDDS3"], (double)Parameters["RampEndAmpDDS4"]);

        addDDSPattern(p, "LambdaCooling1", lambdaCoolingStart,
            (double)Parameters["LambdaF1minus"], (double)Parameters["MOTFreqDDS2"], (double)Parameters["MOTFreqDDS3"], (double)Parameters["LambdaF1plus"],
            (double)Parameters["Lambda1Amp"], (double)Parameters["LightoffDDS2"], (double)Parameters["LightoffDDS3"], (double)Parameters["Lambda2Amp"]);

        addDDSPattern(p, "BlueMOTRampStart", BlueMOTRampStart,
            (double)Parameters["FreqCVB1"], (double)Parameters["FreqCVB2"], (double)Parameters["FreqCVB3"], (double)Parameters["FreqCVB4"],
            (double)Parameters["BMOTAmpDDS1"], (double)Parameters["BMOTAmpDDS2"], (double)Parameters["BMOTAmpDDS3"], (double)Parameters["BMOTAmpDDS4"]);

        addDDSPattern(p, "LambdaCooling2", BlueMOTEnd,
            (double)Parameters["LambdaF1minus"], (double)Parameters["MOTFreqDDS2"], (double)Parameters["MOTFreqDDS3"], (double)Parameters["LambdaF1plus"],
            (double)Parameters["Lambda1Amp"], (double)Parameters["LightoffDDS2"], (double)Parameters["LightoffDDS3"], (double)Parameters["Lambda2Amp"]);

        addDDSPattern(p, "Magtrap", lambdaCooling2end,
            (double)Parameters["ResonanceDDS1"], (double)Parameters["ResonanceDDS2"], (double)Parameters["ResonanceDDS3"], (double)Parameters["ResonanceDDS4"],
            (double)Parameters["LightoffDDS1"], (double)Parameters["LightoffDDS1"], (double)Parameters["LightoffDDS1"], (double)Parameters["LightoffDDS1"]);

        addDDSPattern(p, "image", ImageTweezerChamber,
            (double)Parameters["ResonanceDDS1"], (double)Parameters["ResonanceDDS2"], (double)Parameters["ResonanceDDS3"], (double)Parameters["ResonanceDDS4"],
            (double)Parameters["MOTAmpDDS1"], (double)Parameters["MOTAmpDDS2"], (double)Parameters["MOTAmpDDS3"], (double)Parameters["MOTAmpDDS4"]);

        return p;
    }

    public void addDDSPattern(Dictionary<string, List<List<double>>> p, String name, int time, double freq1, double freq2, double freq3, double freq4, double amp1, double amp2, double amp3, double amp4,
    double freqSlope1 = 0.0, double freqSlope2 = 0.0, double freqSlope3 = 0.0, double freqSlope4 = 0.0, double ampSlope1 = 0.0, double ampSlope2 = 0.0, double ampSlope3 = 0.0, double ampSlope4 = 0.0)
    {

        // List<double> timeDelay, List<double> freq, List<double> amp, List<double> freq_slpoe, List<double> amp_slpoe
        List<double> timePar = new List<double>();
        timePar.Add(time / 100.0);
        List<double> freq = new List<double>();
        freq.Add(freq1);
        freq.Add(freq2);
        freq.Add(freq3);
        freq.Add(freq4);
        List<double> amp = new List<double>();
        amp.Add(amp1);
        amp.Add(amp2);
        amp.Add(amp3);
        amp.Add(amp4);
        // Scale ramp slope by 100 to convert 10 us clock periods to ms
        List<double> freqSlope = new List<double>();
        freqSlope.Add(freqSlope1 * 100.0);
        freqSlope.Add(freqSlope2 * 100.0);
        freqSlope.Add(freqSlope3 * 100.0);
        freqSlope.Add(freqSlope4 * 100.0);
        List<double> ampSlope = new List<double>();
        ampSlope.Add(ampSlope1 * 100.0);
        ampSlope.Add(ampSlope2 * 100.0);
        ampSlope.Add(ampSlope3 * 100.0);
        ampSlope.Add(ampSlope4 * 100.0);

        var patternEvent = new List<List<double>>
        {
            timePar,
            freq,
            amp,
            freqSlope,
            ampSlope
        };

        p.Add(name, patternEvent);

    }

    public override PatternBuilder32 GetDigitalPattern()
    {
        PatternBuilder32 p = new PatternBuilder32();
        //CAF CHAMBER
        int patternStartBeforeQ = (int)Parameters["TCLBlockStart"];
        int CompressRampDownStartTime = (int)Parameters["CompressRampDownStartTime"] + patternStartBeforeQ;
        int CompressRampDownEndTime = CompressRampDownStartTime + (int)Parameters["CompressRampDownDuration"];
        int lambdaCoolingStart = CompressRampDownEndTime + (int)Parameters["CompressRampDownHoldDuration"];
        int BlueMOTRampStart = lambdaCoolingStart + (int)Parameters["LambdaCoolingDuration"];
        int BlueMOTRampEnd = BlueMOTRampStart + (int)Parameters["BlueMOTRampDuration"];
        int BlueMOTEnd = BlueMOTRampEnd + (int)Parameters["BlueMOTDuration"];
        int lambdaCooling2end = BlueMOTEnd + (int)Parameters["LambdaCooling2Duration"];
        int HandoverStart = lambdaCooling2end + (int)Parameters["MagtrapDuration"];
        int MoveStart = HandoverStart + (int)Parameters["HandoverDur"];
        //TWEEZER CHAMBER
        int TweezerHandoverStart = MoveStart + (int)Parameters["MoveTime"];
        int TweezerHandoverEnd = TweezerHandoverStart + (int)Parameters["HandoverDur"];
        int ImageTweezerChamber = TweezerHandoverEnd + (int)Parameters["TweezerCoilsHold"];

        //  SETUP //

        // Time of flight PMT trigger
        p.Pulse(patternStartBeforeQ, 2000, 10, "tofTrigger");

        // CAMERA //

        p.Pulse(patternStartBeforeQ, (int)Parameters["Frame0Trigger"], (int)Parameters["Frame0TriggerDuration"], "cameraTrigger"); //camera trigger for first frame

        //p.Pulse(0, ImageTweezerChamber, (int)Parameters["Frame0TriggerDuration"], "cameraTrigger");  //camera trigger for MOT recap 

        p.Pulse(0, ImageTweezerChamber, (int)Parameters["TweezerImageDur"], "cameraTrigger"); //Tweezer camera trig

        //p.Pulse(patternStartBeforeQ, imageTime + (int)Parameters["Frame0TriggerDuration"] + 5000, (int)Parameters["Frame0TriggerDuration"], "cameraTrigger");//camera trigger bg

        // SLOWING //

        // preset load
        MOTMasterScriptSnippet lm = new LoadMoleculeMOTNoSlowingEdge(p, Parameters);
        p.Pulse(patternStartBeforeQ, 0, (int)Parameters["QSwitchPulseDuration"], "DDS_Analog_Trg");  // DDS trigger

        // Slowing AOMs
        p.Pulse(patternStartBeforeQ, (int)Parameters["SlowingChirpStartTime"], (2 * (int)Parameters["SlowingChirpDuration"]) + 20000, "bXLockBlock"); // Want it to be blocked for whole time that bX laser is moved
        //p.Pulse(patternStartBeforeQ, 100, 100, "bXSlowingAOM"); //first pulse to slowing AOM
        p.Pulse(patternStartBeforeQ, (int)Parameters["SlowingChirpStartTime"] - 100, (int)Parameters["SlowingChirpDuration"] + 100, "bXSlowingAOM"); //first pulse to slowing AOM
        p.Pulse(patternStartBeforeQ, (int)Parameters["slowingRepumpAOMOnStart"], (int)Parameters["SlowingChirpStartTime"] + (int)Parameters["SlowingChirpDuration"] - (int)Parameters["slowingRepumpAOMOnStart"], "v10SlowingAOM"); //first pulse to slowing repump AOM

        // BX Shutter
        p.Pulse(patternStartBeforeQ, (int)Parameters["SlowingChirpStartTime"] + (int)Parameters["SlowingChirpDuration"] + 200, (int)Parameters["MagtrapDuration"] + 21000, "bXSlowingShutter");

        //   p.Pulse(patternStartBeforeQ, BlueMOTEnd-3000, (int)Parameters["MagtrapDuration"]-900, "MOT1Shutter");
        // p.Pulse(patternStartBeforeQ, BlueMOTEnd-1200, (int)Parameters["MagtrapDuration"]-1500, "MOT2Shutter");
        //  p.Pulse(patternStartBeforeQ, BlueMOTEnd-1200, (int)Parameters["MagtrapDuration"]-1500, "MOT3Shutter");

        p.Pulse(0, BlueMOTEnd - 1600, (int)Parameters["ShutterEdgeDur"], "MOT1Shutter");
        p.Pulse(0, BlueMOTEnd - 600, (int)Parameters["ShutterEdgeDur"] - 950, "MOT2Shutter");
        p.Pulse(0, BlueMOTEnd - 800, (int)Parameters["ShutterEdgeDur"] - 500, "MOT3Shutter");

        p.AddEdge("test10", ImageTweezerChamber - 250, true); //Shutter CaF light to tweezer chamber - OPEN
        //p.AddEdge("test10", ImageTweezerChamber+3000, false);
        //p.Pulse(0, ImageTweezerChamber, (int)Parameters["ShutterEdgeDur"]-10000, "test10"); // SHUT DURING MAG TRAP & TRANSPORT / HANDOVER

        p.AddEdge("transportTrack", 0, true);

        //p.DownPulse(0, MoveStart, (int)Parameters["TransportEdgeDur"] + (int)Parameters["TransportCoilsAtTweezersDur"], "transportTrack");

        p.Pulse(patternStartBeforeQ, (int)Parameters["PatternLength"] - (int)Parameters["YagPreFire1"] - (int)Parameters["FlashToQ"], (int)Parameters["QSwitchPulseDuration"], "flashLamp");
        p.Pulse(patternStartBeforeQ, (int)Parameters["PatternLength"] - (int)Parameters["YagPreFire1"], (int)Parameters["QSwitchPulseDuration"], "qSwitch");

        p.Pulse(patternStartBeforeQ, (int)Parameters["PatternLength"] - (int)Parameters["YagPreFire"] - (int)Parameters["FlashToQ"], (int)Parameters["QSwitchPulseDuration"], "flashLamp");
        p.Pulse(patternStartBeforeQ, (int)Parameters["PatternLength"] - (int)Parameters["YagPreFire"], (int)Parameters["QSwitchPulseDuration"], "qSwitch");

        return p;
    }

    public override AnalogPatternBuilder GetAnalogPattern()
    {
        AnalogPatternBuilder p = new AnalogPatternBuilder((int)Parameters["PatternLength"]);

        int CompressRampDownStartTime = (int)Parameters["CompressRampDownStartTime"];
        int CompressRampDownEndTime = CompressRampDownStartTime + (int)Parameters["CompressRampDownDuration"];
        int lambdaCoolingStart = CompressRampDownEndTime + (int)Parameters["CompressRampDownHoldDuration"];
        int BlueMOTRampStart = lambdaCoolingStart + (int)Parameters["LambdaCoolingDuration"];
        int BlueMOTRampEnd = BlueMOTRampStart + (int)Parameters["BlueMOTRampDuration"];
        int BlueMOTEnd = BlueMOTRampEnd + (int)Parameters["BlueMOTDuration"];
        int lambdaCooling2end = BlueMOTEnd + (int)Parameters["LambdaCooling2Duration"];
        int HandoverStart = lambdaCooling2end + (int)Parameters["MagtrapDuration"];
        int MoveStart = HandoverStart + (int)Parameters["HandoverDur"];
        //TWEEZER CHAMBER
        int TweezerHandoverStart = MoveStart + (int)Parameters["MoveTime"];
        int TweezerHandoverEnd = TweezerHandoverStart + (int)Parameters["HandoverDur"];
        int ImageTweezerChamber = TweezerHandoverEnd + (int)Parameters["TweezerCoilsHold"];

        MOTMasterScriptSnippet lm = new LoadMoleculeMOTNoSlowingEdge(p, Parameters);

        // Add Analog Channels

        p.AddChannel("TCoolSidebandVCO");
        p.AddChannel("BXAttenuation");
        p.AddChannel("xShimCoilCurrent");
        p.AddChannel("yShimCoilCurrent");
        p.AddChannel("zShimCoilCurrent");
        p.AddChannel("SlowingRepumpAttenuation");
        p.AddChannel("BXAOM1att");
        p.AddChannel("BXAOM2att");
        p.AddChannel("ODT90att");
        p.AddChannel("ODT70att");
        p.AddChannel("transferCoils");
        p.AddChannel("TweezerCoils");
        //p.AddChannel("DipoleRetroX");
        //p.AddChannel("DipoleRetroY");

        // SET UP //

        // Shim Fields
        p.AddAnalogValue("xShimCoilCurrent", 0, (double)Parameters["xShimLoadCurrent"]);
        p.AddAnalogValue("yShimCoilCurrent", 0, (double)Parameters["yShimLoadCurrent"]);
        p.AddAnalogValue("zShimCoilCurrent", 0, (double)Parameters["zShimLoadCurrent"]);
        /*
        p.AddAnalogValue("xShimCoilCurrent", BlueMOTRampStart, (double)Parameters["xShimBM"]);
        p.AddAnalogValue("yShimCoilCurrent", BlueMOTRampStart, (double)Parameters["yShimBM"]);
        p.AddAnalogValue("zShimCoilCurrent", BlueMOTRampStart, (double)Parameters["zShimBM"]);
        */
        // SLOWING //

        // switch on and off with AOM
        /*
        p.AddAnalogValue("BXAttenuation", (int)Parameters["SlowingChirpStartTime"] - 100, (double)Parameters["BXAOMAttenuation"]);
        p.AddAnalogValue("BXAttenuation", (int)Parameters["SlowingChirpStartTime"] + (int)Parameters["SlowingChirpDuration"], 0.1);
        */
        p.AddAnalogValue("BXAOM1att", 0, (double)Parameters["BXAOM1att"]);
        p.AddAnalogValue("BXAOM2att", 0, (double)Parameters["BXAOM2att"]);

        p.AddAnalogValue("BXAOM1att", (int)Parameters["SlowingChirpStartTime"] + (int)Parameters["SlowingChirpDuration"], 10.0);
        p.AddAnalogValue("BXAOM2att", (int)Parameters["SlowingChirpStartTime"] + (int)Parameters["SlowingChirpDuration"], 10.0);

        p.AddAnalogValue("SlowingRepumpAttenuation", 0, (double)Parameters["SlowingRepumpAttenuation"]);

        // Slowing B field
        p.AddAnalogValue("slowingCoilsCurrent", 0, (double)Parameters["slowingCoilsValue"]);
        p.AddAnalogValue("slowingCoilsCurrent", (int)Parameters["SlowingChirpStartTime"] + (int)Parameters["SlowingChirpDuration"], 0.0);

        // TCOOL

        p.AddAnalogValue("TCoolSidebandVCO", 0, -3.461); //5.15V, 63.5MHz

        // MOT //

        // B Field
        p.AddAnalogValue("MOTCoilsCurrent", 0, (double)Parameters["MOTCoilsCurrentValue"]);

        // CMOT //

        p.AddLinearRamp("MOTCoilsCurrent", CompressRampDownStartTime, (int)Parameters["CompressRampDownDuration"], (double)Parameters["MOTCoilsCompressionValue"]);
        p.AddAnalogValue("MOTCoilsCurrent", lambdaCoolingStart, (double)Parameters["MOTCoilsOffValue"]); // switch off for molasses
        p.AddLinearRamp("MOTCoilsCurrent", BlueMOTRampStart, (int)Parameters["BlueMOTRampDuration"], (double)Parameters["BlueMOTField"]);
        p.AddAnalogValue("MOTCoilsCurrent", BlueMOTEnd, (double)Parameters["MOTCoilsOffValue"]);
        p.AddAnalogValue("MOTCoilsCurrent", lambdaCooling2end, (double)Parameters["MOTCoilsMagtrapValue"]);

        //Handover to Transport coils
        p.AddLinearRamp("MOTCoilsCurrent", HandoverStart, (int)Parameters["HandoverDur"], 0.0);
        p.AddLinearRamp("transferCoils", HandoverStart, (int)Parameters["HandoverDur"], 10.0);

        //Handover to Tweezer coils
        p.AddLinearRamp("transferCoils", TweezerHandoverStart, (int)Parameters["HandoverDur"], 0.0);
        p.AddLinearRamp("MOTCoilsCurrent", TweezerHandoverStart, (int)Parameters["HandoverDur"], (double)Parameters["MOTCoilsCurrentValue"]);

        //Hold in tweezer coils
        p.AddAnalogValue("MOTCoilsCurrent", TweezerHandoverEnd, (double)Parameters["MOTCoilsCurrentValue"]);
        p.AddAnalogValue("MOTCoilsCurrent", ImageTweezerChamber + (int)Parameters["TweezerImageDur"], 0.0);

        return p;
    }

}
