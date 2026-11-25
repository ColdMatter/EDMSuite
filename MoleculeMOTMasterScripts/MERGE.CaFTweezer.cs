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
        Parameters["PatternLength"] = 365000 + 30000;
        Parameters["YagPreFire"] = 45000;


        Parameters["ThorCamExposure"] = 250;
        Parameters["OrcaExposure"] = 4000;

        // Shim Coils
        Parameters["xShimLoadCurrent"] = -3.20;
        Parameters["yShimLoadCurrent"] = -1.35;
        Parameters["zShimLoadCurrent"] = 0.22;

        //Parameters["tMOTccSwitch"] = 5.5;

        // Mag Trap
        Parameters["MagTrapField"] = 1.42;

        // Slowing Chirp, 5W ALS laser
        Parameters["SlowingChirpStartTime"] = 300;//360; //400;// 380;
        Parameters["SlowingChirpDuration"] = 1250;////1400;//1160; //1160

        // Timings
        Parameters["FreeExpTime"] = 1;
        Parameters["OpticalPumpingDuration"] = 100;
        Parameters["Magtrap1Duration"] = 1000;
        Parameters["BlueMOTDuration"] = 1000;
        Parameters["LambdaCoolingDuration"] = 300;
        Parameters["LambdaCooling2Duration"] = 300;
        Parameters["MagTrapField"] = 1.6;

        // Hyperbolic blue-MOT field ramp - CaF chamber blue MOT ONLY (the tweezer-chamber blue MOT keeps its
        // linear ramp over BlueMOTRampDuration). B(t) = B0 / (1 - a*B0*t), i.e. 1/B falls linearly in time at
        // rate a (t in 10 us clock ticks). Coils step to B0 = BlueMOTInitialField at the start of the ramp, then
        // ramp hyperbolically until they reach BlueMOTField. Scan B0 and a; BlueMOTHyperbolicRampDuration is NOT
        // an input - it is recalculated every shot as (1/B0 - 1/BlueMOTField)/a, and everything after the CaF
        // blue-MOT ramp (blue MOT hold, lambda cooling 2, magtrap, transport, tweezer chamber) moves with it.
        // B0 must be non-zero and the same sign as BlueMOTField; a must have that sign too (a > 0 for positive fields).
        Parameters["BlueMOTInitialField"] = 0.1;
        Parameters["BlueMOTHyperbolicRampSpeed"] = 0.002; // "a"; 0.0016 with B0 = 0.142, BlueMOTField = 1.42 gives ~40 ms

        Parameters["HandoverDur"] = 10000;  // can I scan this?
        Parameters["MoveDuration"] = 154000; //done

        Parameters["tMagTrapField"] = 1.0; //done
        Parameters["tMagtrapDuration"] = 1; // done
        Parameters["tDropDuration"] = 1; //done

        Parameters["tBlueMOTDuration"] = 4000;
        Parameters["tBlueMOTField"] = 0.7; //0.48 Rb = 30G/cm

        Parameters["tCoilOffDur"] = 1; //done
        /*
        Parameters["tBlueMOTShimXccValue"] = 1.07; // 8.8 // 10.0
        Parameters["tBlueMOTShimYccValue"] = 0.0; // 3.8 // 3.29
        Parameters["tBlueMOTShimZccValue"] = -0.47; // 0.3 // 0.52
        */
        Parameters["tBlueMOTShimXccValue"] = 10.0; // 8.8 // 10.0
        Parameters["tBlueMOTShimYccValue"] = 0.0; // 3.8 // 3.29
        Parameters["tBlueMOTShimZccValue"] = -1.13; // 0.3 // 0.52

        // This makes the temperature 50uK
        Parameters["tMolassesShimXccValue"] = 3.0; // 8.8 // 10.0
        Parameters["tMolassesShimYccValue"] = 3.8; // 3.8 // 3.29
        Parameters["tMolassesShimZccValue"] = 0.3; // 0.3 // 0.52

        Parameters["tLambdaLoadingDuration"] = 3000;
        Parameters["tFreeFallDuration"] = 3500;
        Parameters["tImageExpo"] = 4000;
    }

    public override Dictionary<string, List<List<double>>> GetDDSPattern()
    {
        SetBlueMOTHyperbolicRampDuration();
        Dictionary<string, List<List<double>>> p = new Dictionary<string, List<List<double>>>();

        int CompressRampDownStartTime = (int)Parameters["CompressRampDownStartTime"];
        int CompressRampDownEndTime = CompressRampDownStartTime + (int)Parameters["CompressRampDownDuration"];
        int lambdaCoolingStart = CompressRampDownEndTime + (int)Parameters["CompressRampDownHoldDuration"];
        int BlueMOTRampStart = lambdaCoolingStart + (int)Parameters["LambdaCoolingDuration"];
        int BlueMOTRampEnd = BlueMOTRampStart + (int)Parameters["BlueMOTHyperbolicRampDuration"];
        int BlueMOTEnd = BlueMOTRampEnd + (int)Parameters["BlueMOTDuration"];
        int lambdaCooling2end = BlueMOTEnd + (int)Parameters["LambdaCooling2Duration"];
        int OPStart = lambdaCooling2end + 100;
        int Magtrap1Start = OPStart + (int)Parameters["OpticalPumpingDuration"];
        int Handover1Start = Magtrap1Start + (int)Parameters["Magtrap1Duration"];
        int Move1Start = Handover1Start + (int)Parameters["HandoverDur"];
        // Tweezer Chamber
        int tHandover1Start = Move1Start + (int)Parameters["MoveDuration"];
        int tMagtrapStart = tHandover1Start + (int)Parameters["HandoverDur"];
        int tCoilsOff = tMagtrapStart + (int)Parameters["tMagtrapDuration"];
        int tBlueMOTRampStart = tCoilsOff + (int)Parameters["tCoilOffDur"];
        int tBlueMOTRampEnd = tBlueMOTRampStart + (int)Parameters["BlueMOTRampDuration"];
        int tLambdaLoadingStart = tBlueMOTRampEnd + (int)Parameters["tBlueMOTDuration"];
        int tFreeFallStart = tLambdaLoadingStart + (int)Parameters["tLambdaLoadingDuration"];
        int tTweezerImage = tFreeFallStart + (int)Parameters["tFreeFallDuration"];
        int Move2Start = tTweezerImage + (int)Parameters["tImageExpo"] + 10000;


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

        addDDSPattern(p, "Image", tBlueMOTRampStart,
            (double)Parameters["FreqCVB1"], (double)Parameters["FreqCVB2"], (double)Parameters["FreqCVB3"], (double)Parameters["FreqCVB4"],
            (double)Parameters["BMOTAmpDDS1"], (double)Parameters["BMOTAmpDDS2"], (double)Parameters["BMOTAmpDDS3"], (double)Parameters["BMOTAmpDDS4"]);

        addDDSPattern(p, "tLambdaCooling", tLambdaLoadingStart,
            (double)Parameters["LambdaF1minus"], (double)Parameters["MOTFreqDDS2"], (double)Parameters["MOTFreqDDS3"], (double)Parameters["LambdaF1plus"],
            (double)Parameters["Lambda1Amp"], (double)Parameters["LightoffDDS2"], (double)Parameters["LightoffDDS3"], (double)Parameters["Lambda2Amp"]);

        addDDSPattern(p, "tFreeFall", tFreeFallStart,
            (double)Parameters["ResonanceDDS1"], (double)Parameters["ResonanceDDS2"], (double)Parameters["ResonanceDDS3"], (double)Parameters["ResonanceDDS4"],
            (double)Parameters["LightoffDDS1"], (double)Parameters["LightoffDDS1"], (double)Parameters["LightoffDDS1"], (double)Parameters["LightoffDDS1"]);

        addDDSPattern(p, "tImage", tTweezerImage,
            (double)Parameters["LambdaF1minus"], (double)Parameters["MOTFreqDDS2"], (double)Parameters["MOTFreqDDS3"], (double)Parameters["LambdaF1plus"],
            (double)Parameters["Lambda1Amp"], (double)Parameters["LightoffDDS2"], (double)Parameters["LightoffDDS3"], (double)Parameters["Lambda2Amp"]);


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
        SetBlueMOTHyperbolicRampDuration();
        PatternBuilder32 p = new PatternBuilder32();
        int patternStartBeforeQ = (int)Parameters["TCLBlockStart"];
        int CompressRampDownStartTime = (int)Parameters["CompressRampDownStartTime"] + patternStartBeforeQ;
        int CompressRampDownEndTime = CompressRampDownStartTime + (int)Parameters["CompressRampDownDuration"];
        int lambdaCoolingStart = CompressRampDownEndTime + (int)Parameters["CompressRampDownHoldDuration"];
        int BlueMOTRampStart = lambdaCoolingStart + (int)Parameters["LambdaCoolingDuration"];
        int BlueMOTRampEnd = BlueMOTRampStart + (int)Parameters["BlueMOTHyperbolicRampDuration"];
        int BlueMOTEnd = BlueMOTRampEnd + (int)Parameters["BlueMOTDuration"];
        int lambdaCooling2end = BlueMOTEnd + (int)Parameters["LambdaCooling2Duration"];
        int OPStart = lambdaCooling2end + 100;
        int Magtrap1Start = OPStart + (int)Parameters["OpticalPumpingDuration"];
        int Handover1Start = Magtrap1Start + (int)Parameters["Magtrap1Duration"];
        int Move1Start = Handover1Start + (int)Parameters["HandoverDur"];
        // Tweezer Chamber
        int tHandover1Start = Move1Start + (int)Parameters["MoveDuration"];
        int tMagtrapStart = tHandover1Start + (int)Parameters["HandoverDur"];
        int tCoilsOff = tMagtrapStart + (int)Parameters["tMagtrapDuration"];
        int tBlueMOTRampStart = tCoilsOff + (int)Parameters["tCoilOffDur"];
        int tBlueMOTRampEnd = tBlueMOTRampStart + (int)Parameters["BlueMOTRampDuration"];
        int tLambdaLoadingStart = tBlueMOTRampEnd + (int)Parameters["tBlueMOTDuration"];
        int tFreeFallStart = tLambdaLoadingStart + (int)Parameters["tLambdaLoadingDuration"];
        int tTweezerImage = tFreeFallStart + (int)Parameters["tFreeFallDuration"];
        int Move2Start = tTweezerImage + (int)Parameters["tImageExpo"] + 10000;

        p.Pulse(0, 0, 100, "tScopeTest"); //Needed to sync digital boards

        //  SETUP //
        // Optical pumping
        p.AddEdge("QCLShutter", 0, true);

        // Time of flight PMT trigger
        p.Pulse(patternStartBeforeQ, 2000, 10, "tofTrigger");

        // CAMERA //
        p.Pulse(0, BlueMOTEnd - (int)Parameters["ThorCamExposure"], (int)Parameters["ThorCamExposure"], "cameraTrigger"); //camera trigger for first frame thorcam

        p.Pulse(0, tLambdaLoadingStart - (int)Parameters["OrcaExposure"], (int)Parameters["OrcaExposure"], "tMOTCamTrig");
        p.Pulse(0, tLambdaLoadingStart - (int)Parameters["OrcaExposure"], (int)Parameters["OrcaExposure"], "tHamCamTrig");

        //p.Pulse(0, tTweezerImage, (int)Parameters["OrcaExposure"], "tHamCamTrig");
        
        //p.Pulse(0, tTweezerImage, (int)Parameters["tImageExpo"], "tHamCamTrig"); // EMCCD


        // preset load
        MOTMasterScriptSnippet lm = new LoadMoleculeMOTNoSlowingEdge(p, Parameters);
        p.Pulse(patternStartBeforeQ, 0, (int)Parameters["QSwitchPulseDuration"], "DDS_Analog_Trg");  // DDS trigger

        // Slowing AOMs
        p.Pulse(patternStartBeforeQ, (int)Parameters["SlowingChirpStartTime"], (2 * (int)Parameters["SlowingChirpDuration"]) + 20000, "bXLockBlock"); // Want it to be blocked for whole time that bX laser is moved
        p.Pulse(patternStartBeforeQ, (int)Parameters["SlowingChirpStartTime"] - 100, (int)Parameters["SlowingChirpDuration"] + 100, "bXSlowingAOM"); //first pulse to slowing AOM
        p.Pulse(patternStartBeforeQ, (int)Parameters["slowingRepumpAOMOnStart"], (int)Parameters["SlowingChirpStartTime"] + (int)Parameters["SlowingChirpDuration"] - (int)Parameters["slowingRepumpAOMOnStart"], "v10SlowingAOM"); //first pulse to slowing repump AOM

        // BX Shutter
        p.Pulse(patternStartBeforeQ, (int)Parameters["SlowingChirpStartTime"] + (int)Parameters["SlowingChirpDuration"] + 200, (int)Parameters["MagtrapDuration"] + 21000, "bXSlowingShutter");

        // MOT Shutters
        p.AddEdge("MOT1Shutter", lambdaCooling2end, true);
        p.AddEdge("MOT2Shutter", lambdaCooling2end, true);
        p.AddEdge("MOT3Shutter", lambdaCooling2end, true);

        p.AddEdge("MOT1Shutter", tBlueMOTRampEnd + (int)Parameters["BlueMOTDuration"] + 2000, false);
        p.AddEdge("MOT2Shutter", tBlueMOTRampEnd + (int)Parameters["BlueMOTDuration"] + 2000, false);
        p.AddEdge("MOT3Shutter", tBlueMOTRampEnd + (int)Parameters["BlueMOTDuration"] + 2000, false);

        // Tweezer Chamber CaF Light Shutter
        p.AddEdge("TweezerChamberMOTShutter", tBlueMOTRampEnd - 20000, true); //Shutter CaF light to tweezer chamber - OPEN
        p.AddEdge("TweezerChamberMOTShutter", Move2Start, false);

        // Optical pumping
        p.DownPulse(OPStart, 0, (int)Parameters["OpticalPumpingDuration"], "QCLShutter");

        // Transport
        p.AddEdge("transportTrack", 0, true);
        p.AddEdge("transportTrack", Move1Start, false);
        p.AddEdge("transportTrack", Move2Start, true);

        // YaG Compensating Shots
        //p.Pulse(patternStartBeforeQ, (int)Parameters["PatternLength"] - (int)Parameters["YagPreFire1"] - (int)Parameters["FlashToQ"], (int)Parameters["QSwitchPulseDuration"], "flashLamp");
        //p.Pulse(patternStartBeforeQ, (int)Parameters["PatternLength"] - (int)Parameters["YagPreFire1"], (int)Parameters["QSwitchPulseDuration"], "qSwitch");

        //p.Pulse(patternStartBeforeQ, (int)Parameters["PatternLength"] - (int)Parameters["YagPreFire"] - (int)Parameters["FlashToQ"], (int)Parameters["QSwitchPulseDuration"], "flashLamp");
        //p.Pulse(patternStartBeforeQ, (int)Parameters["PatternLength"] - (int)Parameters["YagPreFire"], (int)Parameters["QSwitchPulseDuration"], "qSwitch");

        return p;
    }

    public override AnalogPatternBuilder GetAnalogPattern()
    {
        SetBlueMOTHyperbolicRampDuration();
        AnalogPatternBuilder p = new AnalogPatternBuilder((int)Parameters["PatternLength"]);

        int CompressRampDownStartTime = (int)Parameters["CompressRampDownStartTime"];
        int CompressRampDownEndTime = CompressRampDownStartTime + (int)Parameters["CompressRampDownDuration"];
        int lambdaCoolingStart = CompressRampDownEndTime + (int)Parameters["CompressRampDownHoldDuration"];
        int BlueMOTRampStart = lambdaCoolingStart + (int)Parameters["LambdaCoolingDuration"];
        int BlueMOTRampEnd = BlueMOTRampStart + (int)Parameters["BlueMOTHyperbolicRampDuration"];
        int BlueMOTEnd = BlueMOTRampEnd + (int)Parameters["BlueMOTDuration"];
        int lambdaCooling2end = BlueMOTEnd + (int)Parameters["LambdaCooling2Duration"];
        int OPStart = lambdaCooling2end + 100;
        int Magtrap1Start = OPStart + (int)Parameters["OpticalPumpingDuration"];
        int Handover1Start = Magtrap1Start + (int)Parameters["Magtrap1Duration"];
        int Move1Start = Handover1Start + (int)Parameters["HandoverDur"];
        // Tweezer Chamber
        int tHandover1Start = Move1Start + (int)Parameters["MoveDuration"];
        int tMagtrapStart = tHandover1Start + (int)Parameters["HandoverDur"];
        int tCoilsOff = tMagtrapStart + (int)Parameters["tMagtrapDuration"];
        int tBlueMOTRampStart = tCoilsOff + (int)Parameters["tCoilOffDur"];
        int tBlueMOTRampEnd = tBlueMOTRampStart + (int)Parameters["BlueMOTRampDuration"];
        int tLambdaLoadingStart = tBlueMOTRampEnd + (int)Parameters["tBlueMOTDuration"];
        int tFreeFallStart = tLambdaLoadingStart + (int)Parameters["tLambdaLoadingDuration"];
        int tTweezerImage = tFreeFallStart + (int)Parameters["tFreeFallDuration"];
        int Move2Start = tTweezerImage + (int)Parameters["tImageExpo"] + 10000;

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
        p.AddChannel("tMOTcc");
        p.AddChannel("tShimXcc");
        p.AddChannel("tShimYcc");
        p.AddChannel("tShimZcc");
        p.AddChannel("tMOTccSwitch");
        // SET UP //

        // Shim Fields CaF chamber
        p.AddAnalogValue("xShimCoilCurrent", 0, (double)Parameters["xShimLoadCurrent"]);
        p.AddAnalogValue("yShimCoilCurrent", 0, (double)Parameters["yShimLoadCurrent"]);
        p.AddAnalogValue("zShimCoilCurrent", 0, (double)Parameters["zShimLoadCurrent"]);

        // Shim Fields Tweezer chamber
        p.AddAnalogValue("tShimXcc", 0, (double)Parameters["tBlueMOTShimXccValue"]); // Molasses shim coils X
        p.AddAnalogValue("tShimYcc", 0, (double)Parameters["tBlueMOTShimYccValue"]); // Molasses shim coils Y
        p.AddAnalogValue("tShimZcc", 0, (double)Parameters["tBlueMOTShimZccValue"]); // Molasses shim coils Z

        p.AddAnalogValue("tShimXcc", tLambdaLoadingStart, (double)Parameters["tMolassesShimXccValue"]); // Molasses shim coils X
        p.AddAnalogValue("tShimYcc", tLambdaLoadingStart, (double)Parameters["tMolassesShimYccValue"]); // Molasses shim coils Y
        p.AddAnalogValue("tShimZcc", tLambdaLoadingStart, (double)Parameters["tMolassesShimZccValue"]); // Molasses shim coils Z

        // Slowing
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

        // B Fields

        //Tweezer RSD
        p.AddAnalogValue("tMOTccSwitch", 0, 0.0); // MOT coils switch-on (RSD)
        p.AddAnalogValue("tMOTccSwitch", tLambdaLoadingStart, (double)Parameters["tMOTccSwitch"]); // MOT coils switch-off (RSD)

        // MOT -> Compression -> Blue MOT -> Magtrap
        p.AddAnalogValue("MOTCoilsCurrent", 0, (double)Parameters["MOTCoilsCurrentValue"]);                                                                         // ON  - MOT
        p.AddAnalogValue("transferCoils", 0, -0.1);
        p.AddLinearRamp("MOTCoilsCurrent", CompressRampDownStartTime, (int)Parameters["CompressRampDownDuration"], (double)Parameters["MOTCoilsCompressionValue"]); // Ramp up -MOT Compress
        p.AddAnalogValue("MOTCoilsCurrent", lambdaCoolingStart, (double)Parameters["MOTCoilsOffValue"]);                                                            // OFF - lambda molasses
        AddHyperbolicRamp(p, "MOTCoilsCurrent", BlueMOTRampStart, (int)Parameters["BlueMOTHyperbolicRampDuration"],                                                 // Hyperbolic ramp up - Blue MOT (CaF chamber only)
            Convert.ToDouble(Parameters["BlueMOTInitialField"]), Convert.ToDouble(Parameters["BlueMOTField"]),
            Convert.ToDouble(Parameters["BlueMOTHyperbolicRampSpeed"]));
        p.AddAnalogValue("MOTCoilsCurrent", BlueMOTEnd, (double)Parameters["MOTCoilsOffValue"]);                                                                    // OFF - lambda molasses 2
        p.AddAnalogValue("MOTCoilsCurrent", Magtrap1Start, (double)Parameters["MagTrapField"]);                                                                      // ON  - magtrap

        // Handover to transport coils
        p.AddLinearRamp("MOTCoilsCurrent", Handover1Start, (int)Parameters["HandoverDur"], 0.0); 
        p.AddLinearRamp("transferCoils", Handover1Start, (int)Parameters["HandoverDur"], 10.0); 

        // Handover to tweezer coils
        p.AddLinearRamp("transferCoils", tHandover1Start, (int)Parameters["HandoverDur"], -0.1);
        p.AddLinearRamp("tMOTcc", tHandover1Start, (int)Parameters["HandoverDur"], (double)Parameters["tMagTrapField"]);

        // Drop 
        p.AddAnalogValue("tMOTcc", tCoilsOff, 0.0);

        // Ramp into blue mot in tweezer chamber
        p.AddLinearRamp("tMOTcc", tBlueMOTRampStart, (int)Parameters["BlueMOTRampDuration"], (double)Parameters["tBlueMOTField"]);
        p.AddAnalogValue("tMOTcc", tLambdaLoadingStart, 0.0);

        return p;
    }

    // Sets BlueMOTHyperbolicRampDuration (CaF chamber blue MOT only) from the hyperbolic ramp parameters. Called at
    // the top of every Get*Pattern, because MOTMaster applies scan values (EditDictionary) after the constructor and
    // before GetSequence. The ramp reaches BlueMOTField at T = (1/B0 - 1/Bf)/a ticks; the duration is ceil(T) + 1 so
    // the last hyperbolic sample is strictly before T (no overshoot) and the final tick is exactly Bf.
    // BlueMOTRampDuration is left alone - the tweezer-chamber blue MOT still uses it.
    private void SetBlueMOTHyperbolicRampDuration()
    {
        double b0 = Convert.ToDouble(Parameters["BlueMOTInitialField"]);
        double bf = Convert.ToDouble(Parameters["BlueMOTField"]);
        double a = Convert.ToDouble(Parameters["BlueMOTHyperbolicRampSpeed"]);

        if (b0 == 0.0 || bf == 0.0 || Math.Sign(b0) != Math.Sign(bf))
            throw new ArgumentException("Hyperbolic blue-MOT ramp needs BlueMOTInitialField and BlueMOTField non-zero and of the same sign (got "
                + b0 + " -> " + bf + ").");
        if (a == 0.0 || Math.Sign(a) != Math.Sign(bf))
            throw new ArgumentException("BlueMOTHyperbolicRampSpeed (a = " + a + ") must be non-zero and the same sign as the fields.");

        double t = (1.0 / b0 - 1.0 / bf) / a;
        if (t < 0.0)
            throw new ArgumentException("With a = " + a + " the ramp moves away from BlueMOTField (" + b0 + " -> " + bf
                + ") and never reaches it. Check the sign of BlueMOTHyperbolicRampSpeed.");
        if (t > (int)Parameters["PatternLength"])
            throw new ArgumentException("Hyperbolic blue-MOT ramp would take " + Math.Round(t) + " ticks (B0 = " + b0 + ", a = " + a
                + "), longer than PatternLength (" + Parameters["PatternLength"] + ").");

        Parameters["BlueMOTHyperbolicRampDuration"] = (int)Math.Ceiling(t) + 1;
    }

    // Hyperbolic ramp B(t) = B0 / (1 - a*B0*t), sampled every clock tick from startTime, with the last
    // sample (startTime + steps - 1) set exactly to finalValue. Inputs are checked in SetBlueMOTHyperbolicRampDuration:
    // with B0, Bf and a all the same sign the ramp is monotonic and stops before the pole at t = 1/(a*B0).
    private void AddHyperbolicRamp(AnalogPatternBuilder p, string channel, int startTime, int steps,
        double initialValue, double finalValue, double a)
    {
        for (int i = 0; i < steps - 1; i++)
        {
            p.AddAnalogValue(channel, startTime + i, initialValue / (1.0 - a * initialValue * i));
        }
        p.AddAnalogValue(channel, startTime + steps - 1, finalValue);
    }

}
