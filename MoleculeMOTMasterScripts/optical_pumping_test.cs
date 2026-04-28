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
        Parameters["PatternLength"] = 40000;
        Parameters["TCLBlockStart"] = 4000; // This is a time before the Q switch
        Parameters["TCLBlockDuration"] = 4000;
        Parameters["FlashToQ"] = 16; // This is a time before the Q switch
        Parameters["QSwitchPulseDuration"] = 10;
        Parameters["FlashPulseDuration"] = 10;
        Parameters["HeliumShutterToQ"] = 300;
        Parameters["HeliumShutterDuration"] = 2000;

        // Camera
        Parameters["Frame0Trigger"] = 5000;

        Parameters["Frame0TriggerDuration"] = 1000;
        Parameters["Frame0TriggerDuration1"] = 1000;
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
        /*
        Parameters["xShim"] = -2.29;
        Parameters["yShim"] = -2.42;
        Parameters["zShim"] = 10.0;
        */
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

        Parameters["OpticalPumpAmpDDS1"] = 0.042; // -1
        Parameters["OpticalPumpAmpDDS3"] = 0.05; // 2
        Parameters["OpticalPumpAmpDDS4"] = 0.0134; // +1

        Parameters["OpticalPumpFreqDDS1"] = 111.57;// -1
        Parameters["OpticalPumpFreqDDS3"] = 185.54; // 2
        Parameters["OpticalPumpFreqDDS4"] = 172.94; // +1

        // CMOT //

        Parameters["CompressRampDownStartTime"] = 6000;
        Parameters["CompressRampDownDuration"] = 1000;
        Parameters["CompressRampDownHoldDuration"] = 500;
        Parameters["MOTCoilsCompressionValue"] = 1.75;


        Parameters["MOTCoilsOffValue"] = -0.1;

        //11% from lambda measurement
        /*
        Parameters["RampEndAmpDDS1"] = 8.1717e-02; // 7.5%
        Parameters["RampAmplitudeDDS1"] = -1.5828e-04;
        */

        Parameters["RampEndAmpDDS1"] = 8.9600e-02; // 10%
        Parameters["RampAmplitudeDDS1"] = -1.5040e-04;
        Parameters["RampEndAmpDDS2"] = 1.6442e-01; // 30% 
        Parameters["RampAmplitudeDDS2"] = -3.3558e-04;
        Parameters["RampEndAmpDDS3"] = 1.3804e-01; // 14%
        Parameters["RampAmplitudeDDS3"] = -3.0196e-04;
        Parameters["RampEndAmpDDS4"] = 5.0039e-02; //27.5%
        Parameters["RampAmplitudeDDS4"] = -4.9961e-05;


        Parameters["Lambda1Amp"] = 0.24; // full
        Parameters["Lambda2Amp"] = 0.1; // full


        Parameters["dummy"] = 0.0;


        Parameters["LightoffDDS1"] = 0.0;
        Parameters["LightoffDDS2"] = 0.0;
        Parameters["LightoffDDS3"] = 0.0;
        Parameters["LightoffDDS4"] = 0.0;

        Parameters["LambdaF1minus"] = 97.28;
        Parameters["LambdaF1plus"] = 171.00;

        Parameters["SF"] = 95.0;

        Parameters["ResonanceDDS1"] = 111.42;
        Parameters["ResonanceDDS2"] = 156.17;
        Parameters["ResonanceDDS3"] = 188.04;
        Parameters["ResonanceDDS4"] = 174.29;


        Parameters["LambdaCoolingDuration"] = 2000;
        Parameters["QCL_dur"] = 10;
        Parameters["QCL_max_time"] = 1000;
        Parameters["OpticalPumpDuration"] = 500;
        Parameters["MagTrapHoldTime"] = 10000;
        Parameters["shim_settle_on"] = 0;
        Parameters["shim_settle_off"] = 0;
        Parameters["MOTrecaplight_delay"] = 200;

        // END OF PATTERN //

        Parameters["MOTCoilsSwitchOff"] = 25000;

    }

    

    public override Dictionary<string, List<List<double>>> GetDDSPattern()
    {
        Dictionary<string, List<List<double>>> p = new Dictionary<string, List<List<double>>>();

        int CompressRampDownStartTime = (int)Parameters["CompressRampDownStartTime"];
        int CompressRampDownEndTime = CompressRampDownStartTime + (int)Parameters["CompressRampDownDuration"];
        int LambdaCoolingStart = CompressRampDownEndTime + (int)Parameters["CompressRampDownHoldDuration"];
        int OpticalPumpingStart = LambdaCoolingStart + (int)Parameters["LambdaCoolingDuration"];
        int ShimOn = OpticalPumpingStart + (int)Parameters["OpticalPumpDuration"];
        int MagTrapStartTime = ShimOn + (int)Parameters["shim_settle_on"];
        int ShimOff = MagTrapStartTime + (int)Parameters["MagTrapHoldTime"];
        int Recap = ShimOff + (int)Parameters["shim_settle_off"];

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

        addDDSPattern(p, "LambdaCooling", LambdaCoolingStart,
            (double)Parameters["LambdaF1minus"], (double)Parameters["MOTFreqDDS2"], (double)Parameters["MOTFreqDDS3"], (double)Parameters["LambdaF1plus"],
            (double)Parameters["Lambda1Amp"], (double)Parameters["LightoffDDS2"], (double)Parameters["LightoffDDS3"], (double)Parameters["Lambda2Amp"]);

        addDDSPattern(p, "OpticalPump", OpticalPumpingStart,
            (double)Parameters["MOTFreqDDS1"], (double)Parameters["MOTFreqDDS2"], (double)Parameters["OpticalPumpFreqDDS3"], (double)Parameters["OpticalPumpFreqDDS4"],
            (double)Parameters["LightoffDDS1"], (double)Parameters["LightoffDDS2"], (double)Parameters["OpticalPumpAmpDDS3"], (double)Parameters["OpticalPumpAmpDDS4"]);

        addDDSPattern(p, "MagTrapStart", ShimOn,
            (double)Parameters["MOTFreqDDS1"], (double)Parameters["MOTFreqDDS2"], (double)Parameters["MOTFreqDDS3"], (double)Parameters["LambdaF1plus"],
            (double)Parameters["LightoffDDS1"], (double)Parameters["LightoffDDS2"], (double)Parameters["LightoffDDS3"], (double)Parameters["LightoffDDS4"]);

        addDDSPattern(p, "MOTRECAPRampStart", Recap,
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
        int LambdaCoolingStart = CompressRampDownEndTime + (int)Parameters["CompressRampDownHoldDuration"];
        int OpticalPumpingStart = LambdaCoolingStart + (int)Parameters["LambdaCoolingDuration"];
        int ShimOn = OpticalPumpingStart + (int)Parameters["OpticalPumpDuration"];
        int MagTrapStartTime = ShimOn + (int)Parameters["shim_settle_on"];
        int ShimOff = MagTrapStartTime + (int)Parameters["MagTrapHoldTime"];
        int Recap = ShimOff + (int)Parameters["shim_settle_off"];

        p.AddEdge("v0ddsSwitchD", 0, true);
        p.AddEdge("v0ddsSwitchD", (int)Parameters["PatternLength"] - 1000, false); //trigger for rb experiment

        //  SETUP //

        // Time of flight PMT trigger
        p.Pulse(patternStartBeforeQ, 2000, 10, "tofTrigger");

        // CAMERA //


        p.Pulse(patternStartBeforeQ, (int)Parameters["Frame0Trigger"], (int)Parameters["Frame0TriggerDuration"], "cameraTrigger"); //camera trigger for first frame
        
        p.Pulse(0, Recap, (int)Parameters["Frame0TriggerDuration"], "cameraTrigger");

        p.Pulse(0,  (int)Parameters["PatternLength"] - (int)Parameters["Frame0TriggerDuration"] - 10, (int)Parameters["Frame0TriggerDuration"], "cameraTrigger"); //camera trigger for first frame

        
        //check temperature imaging
        // p.Pulse(0, OpticalPumpingStart, (int)Parameters["OpticalPumpDuration"], "opticalPumpingAOM");
        // SLOWING //

        // preset load
        MOTMasterScriptSnippet lm = new LoadMoleculeMOTNoSlowingEdge(p, Parameters);

        // Slowing AOMs
        p.Pulse(patternStartBeforeQ, (int)Parameters["SlowingChirpStartTime"] - 100, (2 * (int)Parameters["SlowingChirpDuration"]) + 20000, "bXLockBlock"); // Want it to be blocked for whole time that bX laser is moved
        p.Pulse(patternStartBeforeQ, (int)Parameters["SlowingChirpStartTime"] - 100, (int)Parameters["SlowingChirpDuration"] + 100, "bXSlowingAOM"); //first pulse to slowing AOM
        p.Pulse(patternStartBeforeQ, 0, (int)Parameters["SlowingChirpStartTime"] + (int)Parameters["SlowingChirpDuration"], "v10SlowingAOM"); //first pulse to slowing repump AOM



        // BX Shutter
        p.Pulse(patternStartBeforeQ, (int)Parameters["SlowingChirpStartTime"] + (int)Parameters["SlowingChirpDuration"], (int)Parameters["MOTCoilsSwitchOff"] - ((int)Parameters["SlowingChirpStartTime"] + (int)Parameters["SlowingChirpDuration"] + 200), "bXSlowingShutter");
   

        return p;
    }

    public override AnalogPatternBuilder GetAnalogPattern()
    {
        AnalogPatternBuilder p = new AnalogPatternBuilder((int)Parameters["PatternLength"]);

        int CompressRampDownStartTime = (int)Parameters["CompressRampDownStartTime"];
        int CompressRampDownEndTime = CompressRampDownStartTime + (int)Parameters["CompressRampDownDuration"];
        int LambdaCoolingStart = CompressRampDownEndTime + (int)Parameters["CompressRampDownHoldDuration"];
        int OpticalPumpingStart = LambdaCoolingStart + (int)Parameters["LambdaCoolingDuration"];
        int ShimOn = OpticalPumpingStart + (int)Parameters["OpticalPumpDuration"];
        int MagTrapStartTime = ShimOn + (int)Parameters["shim_settle_on"];
        int ShimOff = MagTrapStartTime + (int)Parameters["MagTrapHoldTime"];
        int recap = ShimOff + (int)Parameters["shim_settle_off"];

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



        MOTMasterScriptSnippet lm = new LoadMoleculeMOTNoSlowingEdge(p, Parameters);


        // SET UP //

        // Shim Fields

        p.AddAnalogValue("xShimCoilCurrent", 0, (double)Parameters["xShimLoadCurrent"]);
        p.AddAnalogValue("yShimCoilCurrent", 0, (double)Parameters["yShimLoadCurrent"]);
        p.AddAnalogValue("zShimCoilCurrent", 0, (double)Parameters["zShimLoadCurrent"]);


        // SLOWING //

        // switch on and off with AOM


        p.AddAnalogValue("BXAOM1att", 0, (double)Parameters["BXAOM1att"]);
        p.AddAnalogValue("BXAOM2att", 0, (double)Parameters["BXAOM2att"]);

        p.AddAnalogValue("BXAOM1att", (int)Parameters["SlowingChirpStartTime"] + (int)Parameters["SlowingChirpDuration"], 10.0);
        p.AddAnalogValue("BXAOM2att", (int)Parameters["SlowingChirpStartTime"] + (int)Parameters["SlowingChirpDuration"], 10.0);

        p.AddAnalogValue("TCoolSidebandVCO", 0, 5.15); //5.15V, 63.5MHz
        p.AddAnalogValue("BXAttenuation", 0, (double)Parameters["BXAttenuation"]); //vva for Tcool, correct sideband structure

        p.AddAnalogValue("SlowingRepumpAttenuation", 0, (double)Parameters["SlowingRepumpAttenuation"]);

        // Slowing B field
        p.AddAnalogValue("slowingCoilsCurrent", 0, (double)Parameters["slowingCoilsValue"]);
        p.AddAnalogValue("slowingCoilsCurrent", (int)Parameters["SlowingChirpStartTime"] + (int)Parameters["SlowingChirpDuration"], 0.0);

        // MOT //

        // B Field
        p.AddAnalogValue("MOTCoilsCurrent", 0, (double)Parameters["MOTCoilsCurrentValue"]);
        p.AddLinearRamp("MOTCoilsCurrent", CompressRampDownStartTime, (int)Parameters["CompressRampDownDuration"], (double)Parameters["MOTCoilsCompressionValue"]);
        p.AddAnalogValue("MOTCoilsCurrent", OpticalPumpingStart, 0);
        p.AddAnalogValue("MOTCoilsCurrent", ShimOn, (double)Parameters["MOTCoilsCompressionValue"]);
        p.AddAnalogValue("MOTCoilsCurrent", (int)Parameters["MOTCoilsSwitchOff"], 0.0);

        return p;
    }

}
