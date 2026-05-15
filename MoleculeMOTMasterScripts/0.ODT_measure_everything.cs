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
        Parameters["PatternLength"] = 65000;
        Parameters["TCLBlockStart"] = 4000; // This is a time before the Q switch
        Parameters["TCLBlockDuration"] = 4000;
        Parameters["FlashToQ"] = 16; // This is a time before the Q switch
        Parameters["QSwitchPulseDuration"] = 10;
        Parameters["FlashPulseDuration"] = 10;
        Parameters["HeliumShutterToQ"] = 300;
        Parameters["HeliumShutterDuration"] = 2000;

        // Camera
        Parameters["Frame0Trigger"] = 4000;

        Parameters["Frame0TriggerDuration"] = 1000;
        Parameters["Frame0TriggerDuration1"] = 2000;
        Parameters["TempTriggerDuration"] = 1000;
        Parameters["CameraTriggerTransverseTime"] = 120;
        Parameters["FrameTriggerInterval"] = 1100;
        Parameters["waitbeforeimage"] = 1;

        //PMT
        Parameters["PMTTriggerDuration"] = 10;

        // Shim fields
        Parameters["xShimLoadCurrent"] = -2.0;
        Parameters["yShimLoadCurrent"] = -2.4;
        Parameters["zShimLoadCurrent"] = 0.32;

        Parameters["xShimBM"] = -2.0;//-5.2;
        Parameters["yShimBM"] = -2.4;//2.0;
        Parameters["zShimBM"] = 0.32;//3.68;

        // SLOWING //

        // Slowing Chirp

        Parameters["SlowingChirpStartTime"] = 380;//360; //400;// 380;
        Parameters["SlowingChirpDuration"] = 1100;////1400;//1160; //1160
        Parameters["SlowingChirpStartValue"] = 0.0;//0.0
        Parameters["SlowingChirpEndValue"] = -0.3; // -0.5 is 480MHz

        Parameters["BXAOM1att"] = 3.5;//7.85;//3.5;//7.2;
        Parameters["BXAOM2att"] = 3.7;

        Parameters["BXAttenuation"] = 0.91; // TCool 

        // Slowing AOMS

        Parameters["BXAOMAttenuation"] = 10.0;
        Parameters["slowingRepumpAOMOnStart"] = 0;
        Parameters["SlowingRepumpAttenuation"] = 6.2;

        // Slowing B field

        Parameters["slowingCoilsValue"] = 2.0; //1.05;
        //Parameters["slowingCoilsOffTime"] = (int)Parameters["slowingAOMOffStart"]; // 1500;

        // MOT LOAD //

        // MOT B field

        Parameters["MOTCoilsSwitchOn"] = 0;
        Parameters["MOTCoilsCurrentValue"] = 1.0; // 0.65;

        // MOT Sidebands

        //freqs
        Parameters["MOTFreqDDS1"] = 114.07; //+ F = 1-              Parameters["Lambda1"] = 98.00;
        Parameters["MOTFreqDDS2"] = 156.17; //- F = 0            
        Parameters["MOTFreqDDS3"] = 188.04; //- F = 2. DONE
        Parameters["MOTFreqDDS4"] = 175.44; //+ F = 1+             Parameters["Lambda3"] = 171.00;

        //amps for max optical power. dont go higher the amplifiers will saturate
        Parameters["MOTAmpDDS1"] = 0.24;
        Parameters["MOTAmpDDS2"] = 0.5;
        Parameters["MOTAmpDDS3"] = 0.44;
        Parameters["MOTAmpDDS4"] = 0.1;

        // CMOT //

        Parameters["CompressRampDownStartTime"] = 5000;
        Parameters["CompressRampDownDuration"] = 1000;
        Parameters["CompressRampDownHoldDuration"] = 500;
        Parameters["MOTCoilsCompressionValue"] = 1.75;


        Parameters["MOTCoilsOffValue"] = -0.1;


        //12% from lambda measurement

        Parameters["RampEndAmpDDS1"] = 0.097; //12%
        Parameters["RampAmplitudeDDS1"] = -0.000153; //12%
        Parameters["RampEndAmpDDS2"] = 0.113; //12%
        Parameters["RampAmplitudeDDS2"] = -0.00048699999999999997; //12%
        Parameters["RampEndAmpDDS3"] = 0.122; //12%
        Parameters["RampAmplitudeDDS3"] = -0.00022799999999999999; //12%
        Parameters["RampEndAmpDDS4"] = 0.039; //12%
        Parameters["RampAmplitudeDDS4"] = -6.1000000000000005e-05; //12%


        Parameters["Lambda1Amp"] = 0.24; // full
        Parameters["Lambda2Amp"] = 0.1; // full


        Parameters["dummy"] = 0.0;


        Parameters["LightoffDDS1"] = 0.0;
        Parameters["LightoffDDS2"] = 0.0;
        Parameters["LightoffDDS3"] = 0.0;
        Parameters["LightoffDDS4"] = 0.0;

        Parameters["LambdaF1minus"] = 97.28;
        Parameters["LambdaF1plus"] = 171.00;

        Parameters["LambdaF1minusLoad"] = 92.45;
        Parameters["LambdaF1plusLoad"] = 166.00;

        Parameters["Lambda0AmpLoad"] = 0.24; // full
        Parameters["Lambda3AmpLoad"] = 0.1; // full

        Parameters["ResonanceDDS1"] = 111.42;
        Parameters["ResonanceDDS2"] = 156.17;
        Parameters["ResonanceDDS3"] = 188.04;
        Parameters["ResonanceDDS4"] = 174.29;

        //Conveyor belt frequency
        Parameters["FreqCVB1"] = 102.5; //+ F = 1Delta 
        Parameters["FreqCVB3"] = 175.817; //- F = 2sig- delta_a
        Parameters["FreqCVB4"] = 177.442; //+ F = 2sig+ delta_b

        Parameters["AmpCVB1"] = 0.24;
        Parameters["AmpCVB3"] = 0.3;
        Parameters["AmpCVB4"] = 0.06;

        Parameters["LambdaCoolingDuration"] = 400;
        Parameters["LambdaPreODTDuration"] = 0;

        Parameters["BlueMOTField"] = 1.0;
        Parameters["BlueMOTField1"] = 1.2;

        Parameters["BlueMOTRampDuration"] = 6000;
        Parameters["ODTBMLoadDuration"] = 1200; // dont change without changing amplitude ramp
        Parameters["ODTLambdaLoadDuration"] = 2000;

        Parameters["ODTHoldTime"] = 20000;
        Parameters["FreeExpTime"] = 1;

        // END OF PATTERN //

        Parameters["MOTCoilsSwitchOff"] = 60000;

        Parameters["ODTdelay"] = 0;

        Parameters["DipoleRetroX"] = 2.0;
        Parameters["DipoleRetroY"] = 2.0;

    }

    public override Dictionary<string, List<List<double>>> GetDDSPattern()
    {
        Dictionary<string, List<List<double>>> p = new Dictionary<string, List<List<double>>>();


        int CompressRampDownStartTime = (int)Parameters["CompressRampDownStartTime"];
        int CompressRampDownEndTime = CompressRampDownStartTime + (int)Parameters["CompressRampDownDuration"];
        int lambdaCoolingStart = CompressRampDownEndTime + (int)Parameters["CompressRampDownHoldDuration"];
        int BlueMOTRampStart = lambdaCoolingStart + (int)Parameters["LambdaCoolingDuration"];
        int ODTBMLoadStart = BlueMOTRampStart + (int)Parameters["BlueMOTRampDuration"];
        int ODTLambdaLoadStart = ODTBMLoadStart + (int)Parameters["ODTBMLoadDuration"];
        int ODTLoadEnd = ODTLambdaLoadStart + (int)Parameters["ODTLambdaLoadDuration"];
        int Recap = ODTLoadEnd + (int)Parameters["ODTHoldTime"];

        addDDSPattern(p, "MOT1", 0,
            (double)Parameters["MOTFreqDDS1"], (double)Parameters["MOTFreqDDS2"], (double)Parameters["MOTFreqDDS3"], (double)Parameters["MOTFreqDDS4"],
            (double)Parameters["MOTAmpDDS1"], (double)Parameters["MOTAmpDDS2"], (double)Parameters["MOTAmpDDS3"], (double)Parameters["MOTAmpDDS4"]);

        addDDSPattern(p, "RampStart", CompressRampDownStartTime,
            (double)Parameters["MOTFreqDDS1"], (double)Parameters["MOTFreqDDS2"], (double)Parameters["MOTFreqDDS3"], (double)Parameters["MOTFreqDDS4"],
            (double)Parameters["MOTAmpDDS1"], (double)Parameters["MOTAmpDDS2"], (double)Parameters["MOTAmpDDS3"], (double)Parameters["MOTAmpDDS4"],
            0.0, 0.0, 0.0, 0.0, (double)Parameters["RampAmplitudeDDS1"], (double)Parameters["RampAmplitudeDDS2"], (double)Parameters["RampAmplitudeDDS3"], (double)Parameters["RampAmplitudeDDS4"]);

        addDDSPattern(p, "RampEnd", CompressRampDownEndTime,
            (double)Parameters["MOTFreqDDS1"], (double)Parameters["MOTFreqDDS2"], (double)Parameters["MOTFreqDDS3"], (double)Parameters["MOTFreqDDS4"],
            (double)Parameters["RampEndAmpDDS1"], (double)Parameters["RampEndAmpDDS2"], (double)Parameters["RampEndAmpDDS3"], (double)Parameters["RampEndAmpDDS4"]);

        addDDSPattern(p, "LambdaCooling", lambdaCoolingStart,
            (double)Parameters["LambdaF1minusLoad"], (double)Parameters["MOTFreqDDS2"], (double)Parameters["MOTFreqDDS3"], (double)Parameters["LambdaF1plusLoad"],
            (double)Parameters["Lambda1Amp"], (double)Parameters["LightoffDDS2"], (double)Parameters["LightoffDDS3"], (double)Parameters["Lambda2Amp"]);

        addDDSPattern(p, "BlueMOTRampStart", BlueMOTRampStart,
            (double)Parameters["FreqCVB1"], (double)Parameters["MOTFreqDDS2"], (double)Parameters["FreqCVB3"], (double)Parameters["FreqCVB4"],
            (double)Parameters["MOTAmpDDS1"], (double)Parameters["LightoffDDS2"], (double)Parameters["MOTAmpDDS3"], (double)Parameters["MOTAmpDDS4"]);

        addDDSPattern(p, "ODTLambdaLoadStart", ODTLambdaLoadStart,
            (double)Parameters["LambdaF1minusLoad"], (double)Parameters["MOTFreqDDS2"], (double)Parameters["MOTFreqDDS3"], (double)Parameters["LambdaF1plusLoad"],
            (double)Parameters["Lambda0AmpLoad"], (double)Parameters["LightoffDDS2"], (double)Parameters["LightoffDDS3"], (double)Parameters["Lambda3AmpLoad"]);

        addDDSPattern(p, "ODTHold", ODTLoadEnd,
            (double)Parameters["MOTFreqDDS1"], (double)Parameters["MOTFreqDDS2"], (double)Parameters["MOTFreqDDS3"], (double)Parameters["MOTFreqDDS4"],
            (double)Parameters["LightoffDDS1"], (double)Parameters["LightoffDDS2"], (double)Parameters["LightoffDDS3"], (double)Parameters["LightoffDDS4"]);

        addDDSPattern(p, "LambdaImageStart", Recap - 5000,
            (double)Parameters["LambdaF1minusLoad"], (double)Parameters["MOTFreqDDS2"], (double)Parameters["MOTFreqDDS3"], (double)Parameters["LambdaF1plusLoad"],
            (double)Parameters["Lambda0AmpLoad"], (double)Parameters["LightoffDDS2"], (double)Parameters["LightoffDDS3"], (double)Parameters["Lambda3AmpLoad"]);

        addDDSPattern(p, "LambdaImageEnd", Recap - 3000,
            (double)Parameters["LambdaF1minusLoad"], (double)Parameters["MOTFreqDDS2"], (double)Parameters["MOTFreqDDS3"], (double)Parameters["LambdaF1plusLoad"],
            (double)Parameters["Lambda0AmpLoad"], (double)Parameters["LightoffDDS2"], (double)Parameters["LightoffDDS3"], (double)Parameters["Lambda3AmpLoad"]);

        addDDSPattern(p, "BlueMOTRECAPRampStart", Recap,
            (double)Parameters["MOTFreqDDS1"], (double)Parameters["MOTFreqDDS2"], (double)Parameters["MOTFreqDDS3"], (double)Parameters["MOTFreqDDS4"],
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
        int patternStartBeforeQ = (int)Parameters["TCLBlockStart"];
        int CompressRampDownStartTime = (int)Parameters["CompressRampDownStartTime"] + patternStartBeforeQ;
        int CompressRampDownEndTime = CompressRampDownStartTime + (int)Parameters["CompressRampDownDuration"];
        int lambdaCoolingStart = CompressRampDownEndTime + (int)Parameters["CompressRampDownHoldDuration"];
        int BlueMOTRampStart = lambdaCoolingStart + (int)Parameters["LambdaCoolingDuration"];
        int ODTBMLoadStart = BlueMOTRampStart + (int)Parameters["BlueMOTRampDuration"];
        int ODTLambdaLoadStart = ODTBMLoadStart + (int)Parameters["ODTBMLoadDuration"];
        int ODTLoadEnd = ODTLambdaLoadStart + (int)Parameters["ODTLambdaLoadDuration"];
        int Recap = ODTLoadEnd + (int)Parameters["ODTHoldTime"];

        p.AddEdge("v0ddsSwitchD", 0, true);
        p.AddEdge("v0ddsSwitchD", (int)Parameters["PatternLength"] - 1000, false); //trigger for rb experiment

        //  SETUP //

        // Time of flight PMT trigger
        p.Pulse(patternStartBeforeQ, 2000, 10, "tofTrigger");

        // CAMERA //

        // size / position imaging
        //p.Pulse(0, image, (int)Parameters["TempTriggerDuration"], "cameraTrigger"); //camera trigger for temperature

        // recap imaging

        //p.Pulse(patternStartBeforeQ, (int)Parameters["Frame0Trigger"], (int)Parameters["Frame0TriggerDuration"], "cameraTrigger"); //camera trigger for first frame
        //p.Pulse(0, ODTBMLoadStart, 200, "cameraTrigger");//camera trigger for recap
        p.Pulse(0, Recap - 5000, (int)Parameters["Frame0TriggerDuration1"], "cameraTrigger");//camera trigger for recap
        //p.Pulse(0, Recap + 500, (int)Parameters["Frame0TriggerDuration"], "cameraTrigger");
        //p.Pulse(patternStartBeforeQ, (int)Parameters["MOTCoilsSwitchOff"] + 2000, (int)Parameters["Frame0TriggerDuration"], "cameraTrigger");

        // SLOWING //

        // preset load
        MOTMasterScriptSnippet lm = new LoadMoleculeMOTNoSlowingEdge(p, Parameters);

        // Slowing AOMs
        p.Pulse(patternStartBeforeQ, (int)Parameters["SlowingChirpStartTime"] - 100, (2 * (int)Parameters["SlowingChirpDuration"]) + 20000, "bXLockBlock"); // Want it to be blocked for whole time that bX laser is moved
        //p.Pulse(patternStartBeforeQ, 100, 100, "bXSlowingAOM"); //first pulse to slowing AOM
        p.Pulse(patternStartBeforeQ, (int)Parameters["SlowingChirpStartTime"] - 100, (int)Parameters["SlowingChirpDuration"] + 100, "bXSlowingAOM"); //first pulse to slowing AOM
        p.Pulse(patternStartBeforeQ, 0, (int)Parameters["SlowingChirpStartTime"] + (int)Parameters["SlowingChirpDuration"], "v10SlowingAOM"); //first pulse to slowing repump AOM

        // BX Shutter
        p.Pulse(patternStartBeforeQ, (int)Parameters["SlowingChirpStartTime"] + (int)Parameters["SlowingChirpDuration"], (int)Parameters["MOTCoilsSwitchOff"] - ((int)Parameters["SlowingChirpStartTime"] + (int)Parameters["SlowingChirpDuration"] + 200), "bXSlowingShutter");

        return p;
    }

    public override AnalogPatternBuilder GetAnalogPattern()
    {
        AnalogPatternBuilder p = new AnalogPatternBuilder((int)Parameters["PatternLength"]);

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
        //p.AddChannel("DipoleRetroX");
        //p.AddChannel("DipoleRetroY");





        int CompressRampDownStartTime = (int)Parameters["CompressRampDownStartTime"];
        int CompressRampDownEndTime = CompressRampDownStartTime + (int)Parameters["CompressRampDownDuration"];
        int lambdaCoolingStart = CompressRampDownEndTime + (int)Parameters["CompressRampDownHoldDuration"];
        int BlueMOTRampStart = lambdaCoolingStart + (int)Parameters["LambdaCoolingDuration"];
        int ODTBMLoadStart = BlueMOTRampStart + (int)Parameters["BlueMOTRampDuration"];
        int ODTLambdaLoadStart = ODTBMLoadStart + (int)Parameters["ODTBMLoadDuration"];
        int ODTLoadEnd = ODTLambdaLoadStart + (int)Parameters["ODTLambdaLoadDuration"];
        int Recap = ODTLoadEnd + (int)Parameters["ODTHoldTime"];

        MOTMasterScriptSnippet lm = new LoadMoleculeMOTNoSlowingEdge(p, Parameters);

        // Add Analog Channels
        /*
        p.AddChannel("v00Intensity");
        p.AddChannel("v00Frequency");
        p.AddChannel("v00EOMAmp");
        p.AddChannel("v00Chirp");
        p.AddChannel("lightSwitch");
        p.AddChannel("Rf1Freq");
        p.AddChannel("Rf2Freq");
        p.AddChannel("Rf3Freq");
        p.AddChannel("Rf4Freq");
        p.AddChannel("Rf1Amp");
        p.AddChannel("Rf2Amp");
        p.AddChannel("Rf3Amp");
        p.AddChannel("Rf4Amp");
        */
        /*
        p.AddAnalogValue("DipoleRetroX", 0, (double)Parameters["DipoleRetroX"]);
        p.AddAnalogValue("DipoleRetroY", 0, (double)Parameters["DipoleRetroY"]);
        */

        // SET UP //

        // Shim Fields
        p.AddAnalogValue("xShimCoilCurrent", 0, (double)Parameters["xShimLoadCurrent"]);
        p.AddAnalogValue("yShimCoilCurrent", 0, (double)Parameters["yShimLoadCurrent"]);
        p.AddAnalogValue("zShimCoilCurrent", 0, (double)Parameters["zShimLoadCurrent"]);

        p.AddAnalogValue("xShimCoilCurrent", BlueMOTRampStart, (double)Parameters["xShimBM"]);
        p.AddAnalogValue("yShimCoilCurrent", BlueMOTRampStart, (double)Parameters["yShimBM"]);
        p.AddAnalogValue("zShimCoilCurrent", BlueMOTRampStart, (double)Parameters["zShimBM"]);

        p.AddAnalogValue("xShimCoilCurrent", ODTLambdaLoadStart, (double)Parameters["xShimLoadCurrent"]);
        p.AddAnalogValue("yShimCoilCurrent", ODTLambdaLoadStart, (double)Parameters["yShimLoadCurrent"]);
        p.AddAnalogValue("zShimCoilCurrent", ODTLambdaLoadStart, (double)Parameters["zShimLoadCurrent"]);


        // SLOWING //

        // switch on and off with AOM


        p.AddAnalogValue("BXAOM1att", 0, (double)Parameters["BXAOM1att"]);
        p.AddAnalogValue("BXAOM2att", 0, (double)Parameters["BXAOM2att"]);

        p.AddAnalogValue("BXAOM1att", (int)Parameters["SlowingChirpStartTime"] + (int)Parameters["SlowingChirpDuration"], 10.0);
        p.AddAnalogValue("BXAOM2att", (int)Parameters["SlowingChirpStartTime"] + (int)Parameters["SlowingChirpDuration"], 10.0);


        //p.AddAnalogValue("BXAttenuation", (int)Parameters["SlowingChirpStartTime"] - 100, (double)Parameters["BXAOMAttenuation"]); from 1W 
        //p.AddAnalogValue("BXAttenuation", (int)Parameters["SlowingChirpStartTime"] + (int)Parameters["SlowingChirpDuration"], 0.1);

        p.AddAnalogValue("TCoolSidebandVCO", 0, 5.15); //5.15V, 63.5MHz
        p.AddAnalogValue("BXAttenuation", 0, (double)Parameters["BXAttenuation"]); //vva for Tcool, correct sideband structure

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
        p.AddAnalogValue("MOTCoilsCurrent", ODTLambdaLoadStart, (double)Parameters["MOTCoilsOffValue"]); // switch off for molasses
        p.AddAnalogValue("MOTCoilsCurrent", Recap, (double)Parameters["MOTCoilsCurrentValue"]);
        p.AddAnalogValue("MOTCoilsCurrent", Recap + (int)Parameters["Frame0TriggerDuration1"] + 1000, 0.0);

        //Dipole trap

        // retro reflect mirror


        //p.AddAnalogValue("DipoleRetroY", 0, 0.0);

        // dipole trap sequence

        p.AddAnalogValue("ODT70att", 0, 10.0);
        p.AddAnalogValue("ODT90att", 0, 0.0);

        p.AddAnalogValue("ODT70att", ODTBMLoadStart + 200, 0.0);
        p.AddAnalogValue("ODT90att", ODTBMLoadStart + 200, 10.0);

        p.AddAnalogValue("ODT70att", Recap, 10.0);
        p.AddAnalogValue("ODT90att", Recap, 0.0);

        // config 1: Dipole trap off

        //p.AddAnalogValue("ODT70att", 0, 10.0);
        //p.AddAnalogValue("ODT90att", 0, 0.0);

        // config 2: Dipole trap alignment

        //p.AddAnalogValue("ODT70att", 0, 10.0);
        //p.AddAnalogValue("ODT90att", 0, 0.6);


        // config 3: Dipole trap on
        //p.AddAnalogValue("ODT70att", 0, 0.0);
        //p.AddAnalogValue("ODT90att", 0, 10.0);


        return p;
    }

}
