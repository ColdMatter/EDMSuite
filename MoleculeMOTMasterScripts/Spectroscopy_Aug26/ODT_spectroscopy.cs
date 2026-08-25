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
        Parameters["PatternLength"] = 55000;
        Parameters["TCLBlockStart"] = 4000; // This is a time before the Q switch
        Parameters["TCLBlockDuration"] = 4000;
        Parameters["FlashToQ"] = 16; // This is a time before the Q switch
        Parameters["QSwitchPulseDuration"] = 10;
        Parameters["FlashPulseDuration"] = 10;
        Parameters["HeliumShutterToQ"] = 300;
        Parameters["HeliumShutterDuration"] = 2000;

        Parameters["YagPreFire"] = 35000;
        Parameters["YagPreFire1"] = 70000;

        // Camera
        Parameters["Frame0Trigger"] = 4000;

        Parameters["Frame0TriggerDuration"] = 1000;
        Parameters["Frame0TriggerDuration1"] = 2000;
        Parameters["NormImageDur"] = 1000;
        Parameters["TempTriggerDuration"] = 1000;
        Parameters["CameraTriggerTransverseTime"] = 120;
        Parameters["FrameTriggerInterval"] = 1100;
        Parameters["waitbeforeimage"] = 1;

        //PMT
        Parameters["PMTTriggerDuration"] = 10;

        // Shim fields
        Parameters["xShimLoadCurrent"] = -1.85;
        Parameters["yShimLoadCurrent"] = -3.0;
        Parameters["zShimLoadCurrent"] = 0.22;

        Parameters["xShimBM"] = -6.8;//-5.2;
        Parameters["yShimBM"] = -5.8;//2.0;
        Parameters["zShimBM"] = -0.2;//3.68;

        Parameters["xShimSpec"] = -1.85;
        Parameters["yShimSpec"] = -3.0;
        Parameters["zShimSpec"] = 5.22;


        // SLOWING //

        // Slowing Chirp

        Parameters["SlowingChirpStartTime"] = 400;//360; //400;// 380;
        Parameters["SlowingChirpDuration"] = 1050;////1400;//1160; //1160
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

        Parameters["MOTAmpDDS1"] = 0.25;
        Parameters["MOTAmpDDS2"] = 0.6;
        Parameters["MOTAmpDDS3"] = 0.35;
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
        Parameters["FreqCVB2"] = 139.75;
        Parameters["FreqCVB3"] = 175.817; //- F = 2sig- delta_a
        Parameters["FreqCVB4"] = 177.442; //+ F = 2sig+ delta_b

        Parameters["OpticalPumpAmpDDS1"] = 0.042; // -1
        Parameters["OpticalPumpAmpDDS3"] = 0.05; // 2
        Parameters["OpticalPumpAmpDDS4"] = 0.0134; // +1

        Parameters["OpticalPumpFreqDDS1"] = 111.57;// -1
        Parameters["OpticalPumpFreqDDS3"] = 185.54; // 2
        Parameters["OpticalPumpFreqDDS4"] = 172.94; // +1

        Parameters["FreqCVB1recap"] = 102.5; //+ F = 1Delta 
        Parameters["FreqCVB3recap"] = 175.817; //- F = 2sig- delta_a
        Parameters["FreqCVB4recap"] = 177.442; //+ F = 2sig+ delta_b

        Parameters["LambdaCoolingDuration"] = 150;
        Parameters["LambdaPreODTDuration"] = 0;

        Parameters["BlueMOTField"] = 1.3;
        Parameters["BlueMOTField1"] = 1.42;

        Parameters["BlueMOTRampDuration"] = 5000;
        Parameters["ODTBMLoadDuration"] = 1000; // dont change without changing amplitude ramp
        Parameters["ODTLambdaLoadDuration"] = 2500;

        Parameters["OpticalPumpDuration"] = 50;
        Parameters["shim_settle_on"] = 5000;
        Parameters["shim_settle_off"] = 5000;
        Parameters["QCL_dur"] = 2000;
        Parameters["QCL_max_time"] = 5000;

        Parameters["ODTHoldTime"] = (int)Parameters["OpticalPumpDuration"] + (int)Parameters["shim_settle_on"] + (int)Parameters["QCL_max_time"] + (int)Parameters["shim_settle_on"];

        // END OF PATTERN //

        Parameters["MOTCoilsSwitchOff"] = 50000;

        Parameters["ODTdelay"] = 0;
        Parameters["ODT_atten"] = 10.0;

        Parameters["MoleculesFallOut"] = 500;

    }

    public override Dictionary<string, List<List<double>>> GetDDSPattern()
    {
        Dictionary<string, List<List<double>>> p = new Dictionary<string, List<List<double>>>();


        int CompressRampDownStartTime = (int)Parameters["CompressRampDownStartTime"];
        int CompressRampDownEndTime = CompressRampDownStartTime + (int)Parameters["CompressRampDownDuration"];
        int lambdaCoolingStart = CompressRampDownEndTime + (int)Parameters["CompressRampDownHoldDuration"];
        int BlueMOTRampStart = lambdaCoolingStart + (int)Parameters["LambdaCoolingDuration"];
        int ODTBMImage = BlueMOTRampStart + (int)Parameters["BlueMOTRampDuration"];
        int ODTBMLoadStart = ODTBMImage + (int)Parameters["Frame0TriggerDuration"];
        int ODTLambdaLoadStart = ODTBMLoadStart + (int)Parameters["ODTBMLoadDuration"];
        int MoleculesFallOutStart = ODTLambdaLoadStart + (int)Parameters["ODTLambdaLoadDuration"];
        int ODTImage = MoleculesFallOutStart + (int)Parameters["MoleculesFallOut"];
        int OpticalPumpingStart = ODTImage + (int)Parameters["Frame0TriggerDuration"];
        int ShimOn = OpticalPumpingStart + (int)Parameters["OpticalPumpDuration"];
        int QCLStart = ShimOn + (int)Parameters["shim_settle_on"];
        int ShimOff = QCLStart + (int)Parameters["QCL_max_time"];
        int RecapBM = ShimOff + (int)Parameters["shim_settle_off"];
        int BG = RecapBM + (int)Parameters["ODTHoldTime"];



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
            (double)Parameters["FreqCVB1"], (double)Parameters["FreqCVB2"], (double)Parameters["FreqCVB3"], (double)Parameters["FreqCVB4"],
            (double)Parameters["MOTAmpDDS1"], (double)Parameters["MOTAmpDDS2"], (double)Parameters["MOTAmpDDS3"], (double)Parameters["MOTAmpDDS4"]);
        
        addDDSPattern(p, "BMPowerRampStart", ODTBMLoadStart,
            (double)Parameters["FreqCVB1"], (double)Parameters["FreqCVB2"], (double)Parameters["FreqCVB3"], (double)Parameters["FreqCVB4"],
            (double)Parameters["MOTAmpDDS1"], (double)Parameters["MOTAmpDDS2"], (double)Parameters["MOTAmpDDS3"], (double)Parameters["MOTAmpDDS4"],
            0.0, 0.0, 0.0, 0.0, 0.0, (double)Parameters["RampAmplitudeDDS2"], 0.0, (double)Parameters["RampAmplitudeDDS4"]);
        
        addDDSPattern(p, "BMPowerRampEnd", ODTBMLoadStart + 999,
            (double)Parameters["FreqCVB1"], (double)Parameters["FreqCVB2"], (double)Parameters["FreqCVB3"], (double)Parameters["FreqCVB4"],
            (double)Parameters["MOTAmpDDS1"], (double)Parameters["RampEndAmpDDS2"], (double)Parameters["MOTAmpDDS3"], (double)Parameters["RampEndAmpDDS4"]);
        
        addDDSPattern(p, "ODTLambdaLoadStart", ODTLambdaLoadStart,
            (double)Parameters["LambdaF1minusLoad"], (double)Parameters["MOTFreqDDS2"], (double)Parameters["MOTFreqDDS3"], (double)Parameters["LambdaF1plusLoad"],
            (double)Parameters["Lambda0AmpLoad"], (double)Parameters["LightoffDDS2"], (double)Parameters["LightoffDDS3"], (double)Parameters["Lambda3AmpLoad"]);

        addDDSPattern(p, "MoleculesFallOutStart", MoleculesFallOutStart,
            (double)Parameters["MOTFreqDDS1"], (double)Parameters["MOTFreqDDS2"], (double)Parameters["MOTFreqDDS3"], (double)Parameters["LambdaF1plus"],
            (double)Parameters["LightoffDDS1"], (double)Parameters["LightoffDDS2"], (double)Parameters["LightoffDDS3"], (double)Parameters["LightoffDDS4"]);

        addDDSPattern(p, "ODTImage", ODTImage,
            (double)Parameters["LambdaF1minusLoad"], (double)Parameters["MOTFreqDDS2"], (double)Parameters["MOTFreqDDS3"], (double)Parameters["LambdaF1plusLoad"],
            (double)Parameters["Lambda0AmpLoad"], (double)Parameters["LightoffDDS2"], (double)Parameters["LightoffDDS3"], (double)Parameters["Lambda3AmpLoad"]);

        addDDSPattern(p, "OpticalPump", OpticalPumpingStart,
            (double)Parameters["OpticalPumpFreqDDS1"], (double)Parameters["MOTFreqDDS2"], (double)Parameters["OpticalPumpFreqDDS3"], (double)Parameters["OpticalPumpFreqDDS4"],
            (double)Parameters["OpticalPumpAmpDDS1"], (double)Parameters["LightoffDDS2"], (double)Parameters["OpticalPumpAmpDDS3"], (double)Parameters["OpticalPumpAmpDDS4"]);
        
        addDDSPattern(p, "Spectroscopy", ShimOn,
            (double)Parameters["MOTFreqDDS1"], (double)Parameters["MOTFreqDDS2"], (double)Parameters["MOTFreqDDS3"], (double)Parameters["LambdaF1plus"],
            (double)Parameters["LightoffDDS1"], (double)Parameters["LightoffDDS2"], (double)Parameters["LightoffDDS3"], (double)Parameters["LightoffDDS4"]);

        addDDSPattern(p, "BMrecap", RecapBM,
            (double)Parameters["FreqCVB1"], (double)Parameters["FreqCVB2"], (double)Parameters["FreqCVB3"], (double)Parameters["FreqCVB4"],
            (double)Parameters["MOTAmpDDS1"], (double)Parameters["MOTAmpDDS2"], (double)Parameters["MOTAmpDDS3"], (double)Parameters["MOTAmpDDS4"]);

        addDDSPattern(p, "blowaway", RecapBM + 2000,
            (double)Parameters["MOTFreqDDS1"], (double)Parameters["MOTFreqDDS2"], (double)Parameters["MOTFreqDDS3"], (double)Parameters["MOTFreqDDS4"],
            (double)Parameters["MOTAmpDDS1"], (double)Parameters["MOTAmpDDS2"], (double)Parameters["MOTAmpDDS3"], (double)Parameters["MOTAmpDDS4"]);

        addDDSPattern(p, "BG", BG - 2000,
            (double)Parameters["FreqCVB1"], (double)Parameters["FreqCVB2"], (double)Parameters["FreqCVB3"], (double)Parameters["FreqCVB4"],
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
        int ODTBMImage = BlueMOTRampStart + (int)Parameters["BlueMOTRampDuration"];
        int ODTBMLoadStart = ODTBMImage + (int)Parameters["Frame0TriggerDuration"];
        int ODTLambdaLoadStart = ODTBMLoadStart + (int)Parameters["ODTBMLoadDuration"];
        int MoleculesFallOutStart = ODTLambdaLoadStart + (int)Parameters["ODTLambdaLoadDuration"];
        int ODTImage = MoleculesFallOutStart + (int)Parameters["MoleculesFallOut"];
        int OpticalPumpingStart = ODTImage + (int)Parameters["Frame0TriggerDuration"];
        int ShimOn = OpticalPumpingStart + (int)Parameters["OpticalPumpDuration"];
        int QCLStart = ShimOn + (int)Parameters["shim_settle_on"];
        int ShimOff = QCLStart + (int)Parameters["QCL_max_time"];
        int RecapBM = ShimOff + (int)Parameters["shim_settle_off"];
        int BG = RecapBM + (int)Parameters["ODTHoldTime"];

        p.AddEdge("v0ddsSwitchD", 0, true);
        p.AddEdge("v0ddsSwitchD", (int)Parameters["PatternLength"] - 1000, false); //trigger for rb experiment

        //  SETUP //

        // Time of flight PMT trigger
        p.Pulse(patternStartBeforeQ, 2000, 10, "tofTrigger");

        p.Pulse(patternStartBeforeQ, 0, (int)Parameters["QSwitchPulseDuration"], "DDS_Analog_Trg");  // DDS trigger

        // CAMERA //

        // size / position imaging
        //p.Pulse(0, image, (int)Parameters["TempTriggerDuration"], "cameraTrigger"); //camera trigger for temperature

        // recap imaging

        p.Pulse(0, ODTImage, (int)Parameters["Frame0TriggerDuration"], "cameraTrigger"); //camera trigger for first frame
        p.Pulse(0, RecapBM + 500, (int)Parameters["Frame0TriggerDuration"], "cameraTrigger");

        //p.Pulse(0, BG, (int)Parameters["Frame0TriggerDuration"], "cameraTrigger"); // in sequence background

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
        
        if ((int)Parameters["PatternLength"] > 80000)
        {
            /*
            p.Pulse(patternStartBeforeQ, (int)Parameters["PatternLength"] - (int)Parameters["YagPreFire1"] - (int)Parameters["FlashToQ"], (int)Parameters["QSwitchPulseDuration"], "flashLamp");
            p.Pulse(patternStartBeforeQ, (int)Parameters["PatternLength"] - (int)Parameters["YagPreFire1"], (int)Parameters["QSwitchPulseDuration"], "qSwitch");
            */
            p.Pulse(patternStartBeforeQ, (int)Parameters["PatternLength"] - (int)Parameters["YagPreFire"] - (int)Parameters["FlashToQ"], (int)Parameters["QSwitchPulseDuration"], "flashLamp");
            p.Pulse(patternStartBeforeQ, (int)Parameters["PatternLength"] - (int)Parameters["YagPreFire"], (int)Parameters["QSwitchPulseDuration"], "qSwitch");
        }
        
        // MOT beam shutters - block the MOT light during the dipole trap
        p.Pulse(0, ShimOn - 1600, (int)Parameters["ODTHoldTime"], "MOT1Shutter");
        p.Pulse(0, ShimOn - 600, (int)Parameters["ODTHoldTime"] - 950, "MOT2Shutter");
        p.Pulse(0, ShimOn - 800, (int)Parameters["ODTHoldTime"] - 500, "MOT3Shutter");

        p.Pulse(0, QCLStart, (int)Parameters["QCL_dur"], "QCLShutter");
        //p.Pulse(0, QCLStart, (int)Parameters["QCL_dur"], "microwaveC");

        return p;
    }

    public override AnalogPatternBuilder GetAnalogPattern()
    {
        AnalogPatternBuilder p = new AnalogPatternBuilder((int)Parameters["PatternLength"]);


        int CompressRampDownStartTime = (int)Parameters["CompressRampDownStartTime"];
        int CompressRampDownEndTime = CompressRampDownStartTime + (int)Parameters["CompressRampDownDuration"];
        int lambdaCoolingStart = CompressRampDownEndTime + (int)Parameters["CompressRampDownHoldDuration"];
        int BlueMOTRampStart = lambdaCoolingStart + (int)Parameters["LambdaCoolingDuration"];
        int ODTBMImage = BlueMOTRampStart + (int)Parameters["BlueMOTRampDuration"];
        int ODTBMLoadStart = ODTBMImage + (int)Parameters["Frame0TriggerDuration"];
        int ODTLambdaLoadStart = ODTBMLoadStart + (int)Parameters["ODTBMLoadDuration"];
        int MoleculesFallOutStart = ODTLambdaLoadStart + (int)Parameters["ODTLambdaLoadDuration"];
        int ODTImage = MoleculesFallOutStart + (int)Parameters["MoleculesFallOut"];
        int OpticalPumpingStart = ODTImage + (int)Parameters["Frame0TriggerDuration"];
        int ShimOn = OpticalPumpingStart + (int)Parameters["OpticalPumpDuration"];
        int QCLStart = ShimOn + (int)Parameters["shim_settle_on"];
        int ShimOff = QCLStart + (int)Parameters["QCL_max_time"];
        int RecapBM = ShimOff + (int)Parameters["shim_settle_off"];
        int BG = RecapBM + (int)Parameters["ODTHoldTime"];

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
        //p.AddChannel("DipoleRetroX");
        //p.AddChannel("DipoleRetroY");

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
        
        p.AddAnalogValue("xShimCoilCurrent", ShimOn, (double)Parameters["xShimSpec"]);
        p.AddAnalogValue("yShimCoilCurrent", ShimOn, (double)Parameters["yShimSpec"]);
        p.AddAnalogValue("zShimCoilCurrent", ShimOn, (double)Parameters["zShimSpec"]);

        p.AddAnalogValue("xShimCoilCurrent", ShimOff, (double)Parameters["xShimLoadCurrent"]);
        p.AddAnalogValue("yShimCoilCurrent", ShimOff, (double)Parameters["yShimLoadCurrent"]);
        p.AddAnalogValue("zShimCoilCurrent", ShimOff, (double)Parameters["zShimLoadCurrent"]);
        

        // SLOWING //

        // switch on and off with AOM


        p.AddAnalogValue("BXAOM1att", 0, (double)Parameters["BXAOM1att"]);
        p.AddAnalogValue("BXAOM2att", 0, (double)Parameters["BXAOM2att"]);

        p.AddAnalogValue("BXAOM1att", (int)Parameters["SlowingChirpStartTime"] + (int)Parameters["SlowingChirpDuration"], 10.0);
        p.AddAnalogValue("BXAOM2att", (int)Parameters["SlowingChirpStartTime"] + (int)Parameters["SlowingChirpDuration"], 10.0);


        //p.AddAnalogValue("BXAttenuation", (int)Parameters["SlowingChirpStartTime"] - 100, (double)Parameters["BXAOMAttenuation"]); from 1W 
        //p.AddAnalogValue("BXAttenuation", (int)Parameters["SlowingChirpStartTime"] + (int)Parameters["SlowingChirpDuration"], 0.1);

        p.AddAnalogValue("BXAttenuation", 0, (double)Parameters["BXAttenuation"]); //vva for Tcool, correct sideband structure

        p.AddAnalogValue("SlowingRepumpAttenuation", 0, (double)Parameters["SlowingRepumpAttenuation"]);

        // Slowing B field
        p.AddAnalogValue("slowingCoilsCurrent", 0, (double)Parameters["slowingCoilsValue"]);
        p.AddAnalogValue("slowingCoilsCurrent", (int)Parameters["SlowingChirpStartTime"] + (int)Parameters["SlowingChirpDuration"], 0.0);


        // MOT //

        // B Field
        p.AddAnalogValue("MOTCoilsCurrent", 0, (double)Parameters["MOTCoilsCurrentValue"]);

        // CMOT //

        p.AddLinearRamp("MOTCoilsCurrent", CompressRampDownStartTime, (int)Parameters["CompressRampDownDuration"], (double)Parameters["MOTCoilsCompressionValue"]);
        p.AddAnalogValue("MOTCoilsCurrent", lambdaCoolingStart, (double)Parameters["MOTCoilsOffValue"]); // switch off for molasses
        p.AddLinearRamp("MOTCoilsCurrent", BlueMOTRampStart, (int)Parameters["BlueMOTRampDuration"], (double)Parameters["BlueMOTField"]);
        p.AddAnalogValue("MOTCoilsCurrent", ODTLambdaLoadStart, (double)Parameters["MOTCoilsOffValue"]); // switch off for molasses
        p.AddAnalogValue("MOTCoilsCurrent", RecapBM, (double)Parameters["BlueMOTField1"]);
        p.AddAnalogValue("MOTCoilsCurrent", RecapBM + (int)Parameters["Frame0TriggerDuration1"] + 1000, 0.0);

        //Dipole trap

        // dipole trap sequence

        p.AddAnalogValue("ODT70att", 0, 10.0);
        p.AddAnalogValue("ODT90att", 0, 0.0);

        p.AddAnalogValue("ODT70att", BlueMOTRampStart, 0.0);
        p.AddAnalogValue("ODT90att", BlueMOTRampStart, (double)Parameters["ODT_atten"]);

        p.AddAnalogValue("ODT70att", RecapBM, 10.0);
        p.AddAnalogValue("ODT90att", RecapBM, 0.0);

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
