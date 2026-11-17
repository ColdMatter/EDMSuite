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
        Parameters["PatternLength"] = 365000;
        Parameters["YagPreFire"] = 45000;


        Parameters["ThorCamExposure"] = 250;
        Parameters["OrcaExposure"] = 1000;

        // Shim Coils
        Parameters["xShimLoadCurrent"] = -3.20;
        Parameters["yShimLoadCurrent"] = -1.35;
        Parameters["zShimLoadCurrent"] = 0.22;

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

        Parameters["HandoverDur"] = 10000;  // can I scan this?
        Parameters["MoveDuration"] = 154000; //done

        Parameters["tMagTrapField"] = 1.0; //done
        Parameters["tMagtrapDuration"] = 1; // done
        Parameters["tDropDuration"] = 1; //done
        Parameters["tImageExpo"] = 1000;

        Parameters["tBlueMOTDuration"] = 1000;
        Parameters["tBlueMOTField"] = 1.8;
        Parameters["tLambdaDur"] = 2000;

        Parameters["tFreeFall"] = 1000;

        Parameters["tCoilOffDur"] = 1; //done
    }

    public override Dictionary<string, List<List<double>>> GetDDSPattern()
    {
        Dictionary<string, List<List<double>>> p = new Dictionary<string, List<List<double>>>();

        int CompressRampDownStartTime = (int)Parameters["CompressRampDownStartTime"];
        int CompressRampDownEndTime = CompressRampDownStartTime + (int)Parameters["CompressRampDownDuration"];
        int lambdaCoolingStart = CompressRampDownEndTime + (int)Parameters["CompressRampDownHoldDuration"];
        int BlueMOTRampStart = lambdaCoolingStart + (int)Parameters["LambdaCoolingDuration"];
        int BlueMOTRampEnd = BlueMOTRampStart + (int)Parameters["BlueMOTRampDuration"];
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
        int tBlueMOTRampEndImage = tBlueMOTRampStart + (int)Parameters["BlueMOTRampDuration"];
        int tLambdaStart = tBlueMOTRampEndImage + (int)Parameters["tBlueMOTDuration"];
        int tLambdaEnd = tLambdaStart + (int)Parameters["tLambdaDur"];
        int tCaFImage = tLambdaEnd + (int)Parameters["tFreeFall"];
        int Move2Start = tCaFImage + 10000;


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

        addDDSPattern(p, "tLambdaStart", tLambdaStart,
            (double)Parameters["LambdaF1minus"], (double)Parameters["MOTFreqDDS2"], (double)Parameters["MOTFreqDDS3"], (double)Parameters["LambdaF1plus"],
            (double)Parameters["Lambda1Amp"], (double)Parameters["LightoffDDS2"], (double)Parameters["LightoffDDS3"], (double)Parameters["Lambda2Amp"]);

        addDDSPattern(p, "FreeFall", tLambdaEnd,
            (double)Parameters["ResonanceDDS1"], (double)Parameters["ResonanceDDS2"], (double)Parameters["ResonanceDDS3"], (double)Parameters["ResonanceDDS4"],
            (double)Parameters["LightoffDDS1"], (double)Parameters["LightoffDDS1"], (double)Parameters["LightoffDDS1"], (double)Parameters["LightoffDDS1"]);

        addDDSPattern(p, "tCaFImage", tCaFImage,
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
        PatternBuilder32 p = new PatternBuilder32();
        int patternStartBeforeQ = (int)Parameters["TCLBlockStart"];
        int CompressRampDownStartTime = (int)Parameters["CompressRampDownStartTime"] + patternStartBeforeQ;
        int CompressRampDownEndTime = CompressRampDownStartTime + (int)Parameters["CompressRampDownDuration"];
        int lambdaCoolingStart = CompressRampDownEndTime + (int)Parameters["CompressRampDownHoldDuration"];
        int BlueMOTRampStart = lambdaCoolingStart + (int)Parameters["LambdaCoolingDuration"];
        int BlueMOTRampEnd = BlueMOTRampStart + (int)Parameters["BlueMOTRampDuration"];
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
        int tBlueMOTRampEndImage = tBlueMOTRampStart + (int)Parameters["BlueMOTRampDuration"];
        int tLambdaStart = tBlueMOTRampEndImage + (int)Parameters["tBlueMOTDuration"];
        int tLambdaEnd = tLambdaStart + (int)Parameters["tLambdaDur"];
        int tCaFImage = tLambdaEnd + (int)Parameters["tFreeFall"];
        int Move2Start = tCaFImage + 10000;

        //  SETUP //
        // Optical pumping
        p.AddEdge("QCLShutter", 0, true);

        // Time of flight PMT trigger
        p.Pulse(patternStartBeforeQ, 2000, 10, "tofTrigger");

        // CAMERA //
        p.Pulse(0, BlueMOTEnd - (int)Parameters["ThorCamExposure"], (int)Parameters["ThorCamExposure"], "cameraTrigger"); //camera trigger for first frame
        //p.Pulse(BlueMOTEnd2 - (int)Parameters["BlueMOTDuration"], 0, (int)Parameters["BlueMOTDuration"]/2, "cameraTrigger"); //Tweezer camera trig
        //p.Pulse(background, 0, (int)Parameters["BlueMOTDuration"]/2, "cameraTrigger"); //Tweezer camera trig

        //p.Pulse(0, tLambdaStart - (int)Parameters["OrcaExposure"], (int)Parameters["OrcaExposure"], "rbAbsImgCamTrig");

        p.Pulse(0, tCaFImage, 5000, "rbAbsImgCamTrig");

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

        p.AddEdge("MOT1Shutter", tBlueMOTRampEndImage + (int)Parameters["BlueMOTDuration"] + 2000, false);
        p.AddEdge("MOT2Shutter", tBlueMOTRampEndImage + (int)Parameters["BlueMOTDuration"] + 2000, false);
        p.AddEdge("MOT3Shutter", tBlueMOTRampEndImage + (int)Parameters["BlueMOTDuration"] + 2000, false);

        // Tweezer Chamber CaF Light Shutter
        p.AddEdge("TweezerChamberMOTShutter", tBlueMOTRampEndImage - 20000, true); //Shutter CaF light to tweezer chamber - OPEN
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
        int OPStart = lambdaCooling2end + 100;
        int Magtrap1Start = OPStart + (int)Parameters["OpticalPumpingDuration"];
        int Handover1Start = Magtrap1Start + (int)Parameters["Magtrap1Duration"];
        int Move1Start = Handover1Start + (int)Parameters["HandoverDur"];
        // Tweezer Chamber
        int tHandover1Start = Move1Start + (int)Parameters["MoveDuration"];
        int tMagtrapStart = tHandover1Start + (int)Parameters["HandoverDur"];
        int tCoilsOff = tMagtrapStart + (int)Parameters["tMagtrapDuration"];
        int tBlueMOTRampStart = tCoilsOff + (int)Parameters["tCoilOffDur"];
        int tBlueMOTRampEndImage = tBlueMOTRampStart + (int)Parameters["BlueMOTRampDuration"];
        int tLambdaStart = tBlueMOTRampEndImage + (int)Parameters["tBlueMOTDuration"];
        int tLambdaEnd = tLambdaStart + (int)Parameters["tLambdaDur"];
        int tCaFImage = tLambdaEnd + (int)Parameters["tFreeFall"];
        int Move2Start = tCaFImage + 10000;

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

        // SET UP //

        // Shim Fields
        p.AddAnalogValue("xShimCoilCurrent", 0, (double)Parameters["xShimLoadCurrent"]);
        p.AddAnalogValue("yShimCoilCurrent", 0, (double)Parameters["yShimLoadCurrent"]);
        p.AddAnalogValue("zShimCoilCurrent", 0, (double)Parameters["zShimLoadCurrent"]);

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
        // MOT -> Compression -> Blue MOT -> Magtrap
        p.AddAnalogValue("MOTCoilsCurrent", 0, (double)Parameters["MOTCoilsCurrentValue"]);                                                                         // ON  - MOT
        p.AddAnalogValue("transferCoils", 0, -0.1);
        p.AddLinearRamp("MOTCoilsCurrent", CompressRampDownStartTime, (int)Parameters["CompressRampDownDuration"], (double)Parameters["MOTCoilsCompressionValue"]); // Ramp up -MOT Compress
        p.AddAnalogValue("MOTCoilsCurrent", lambdaCoolingStart, (double)Parameters["MOTCoilsOffValue"]);                                                            // OFF - lambda molasses
        p.AddLinearRamp("MOTCoilsCurrent", BlueMOTRampStart, (int)Parameters["BlueMOTRampDuration"], (double)Parameters["BlueMOTField"]);                           // Ramp up - Blue MOT
        p.AddAnalogValue("MOTCoilsCurrent", BlueMOTEnd, (double)Parameters["MOTCoilsOffValue"]);                                                                    // OFF - lambda molasses 2
        p.AddAnalogValue("MOTCoilsCurrent", Magtrap1Start, (double)Parameters["MagTrapField"]);                                                                      // ON  - magtrap

        // Handover to transport coils
        p.AddLinearRamp("MOTCoilsCurrent", Handover1Start, (int)Parameters["HandoverDur"], 0.0); 
        p.AddLinearRamp("transferCoils", Handover1Start, (int)Parameters["HandoverDur"], 10.0); 

        // Handover to tweezer coils
        p.AddLinearRamp("transferCoils", tHandover1Start, (int)Parameters["HandoverDur"], -0.1);
        p.AddLinearRamp("TweezerCoils", tHandover1Start, (int)Parameters["HandoverDur"], (double)Parameters["tMagTrapField"]);

        // Drop 
        p.AddAnalogValue("TweezerCoils", tCoilsOff, 0.0);

        // Ramp into blue mot in tweezer chamber
        p.AddLinearRamp("TweezerCoils", tBlueMOTRampStart, (int)Parameters["BlueMOTRampDuration"], (double)Parameters["tBlueMOTField"]);
        p.AddAnalogValue("TweezerCoils", tLambdaStart, 0.0);

        return p;
    }

}
