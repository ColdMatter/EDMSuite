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
        Parameters["PatternLength"] = 700000;
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
        Parameters["BMTriggerDuration"] = 1000;
        Parameters["Frame0TriggerDuration"] = 500;
        Parameters["TempTriggerDuration"] = 100;
        Parameters["CameraTriggerTransverseTime"] = 120;
        Parameters["FrameTriggerInterval"] = 1100;
        Parameters["waitbeforeimage"] = 1;

        //PMT
        Parameters["PMTTriggerDuration"] = 10;

        // Shim fields
        Parameters["xShimLoadCurrent"] = -3.20;
        Parameters["yShimLoadCurrent"] = -1.35;
        Parameters["zShimLoadCurrent"] = 0.22;
        /*
        Parameters["xShimBM"] = -2.29;
        Parameters["yShimBM"] = -2.42;
        Parameters["zShimBM"] = 0.377;
        */
        // SLOWING //

        // Slowing Chirp
        /*
        Parameters["SlowingChirpStartTime"] = 500;//360; //400;// 380;
        Parameters["SlowingChirpDuration"] = 1100;////1400;//1160; //1160
        Parameters["SlowingChirpStartValue"] = 0.0;//0.0
        Parameters["SlowingChirpEndValue"] = -1.25; // -0.5 is 480MHz
        */

        // Slowing Chirp, 5W ALS laser
        Parameters["SlowingChirpStartTime"] = 350;//360; //400;// 380;
        Parameters["SlowingChirpDuration"] = 1250;////1400;//1160; //1160
        Parameters["SlowingChirpStartValue"] = 0.0;//0.0
        Parameters["SlowingChirpEndValue"] = -0.30; // -0.5 is 480MHz

        Parameters["BXAOM1att"] = 3.5;//7.85;//3.5;//7.2;
        Parameters["BXAOM2att"] = 3.7;

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

        Parameters["BMOTAmpDDS1"] = 0.25;
        Parameters["BMOTAmpDDS2"] = 0.6;
        Parameters["BMOTAmpDDS3"] = 0.35;
        Parameters["BMOTAmpDDS4"] = 0.1;

        // CMOT //

        Parameters["CompressRampDownStartTime"] = 5000;
        Parameters["CompressRampDownDuration"] = 1000;
        Parameters["CompressRampDownHoldDuration"] = 500;
        Parameters["MOTCoilsCompressionValue"] = 1.75;


        Parameters["MOTCoilsOffValue"] = -0.1;

        // magtrap //

        Parameters["MOTCoilsMagtrapValue"] = 1.42;

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

        Parameters["RampEndAmpDDS1"] = 0.097; //12%
        Parameters["RampAmplitudeDDS1"] = -0.000153; //12%
        Parameters["RampEndAmpDDS2"] = 0.113; //12%
        Parameters["RampAmplitudeDDS2"] = -0.00048699999999999997; //12%
        Parameters["RampEndAmpDDS3"] = 0.122; //12%
        Parameters["RampAmplitudeDDS3"] = -0.00022799999999999999; //12%
        Parameters["RampEndAmpDDS4"] = 0.039; //12%
        Parameters["RampAmplitudeDDS4"] = -6.1000000000000005e-05; //12%

        // lambda rf amps
        Parameters["Lambda1Amp"] = 0.24; // full
        Parameters["Lambda2Amp"] = 0.1; // full


        // Ramp slopes
        //Parameters["RampAmplitudeDDS1"] = ((double)Parameters["RampEndAmpDDS1"] - (double)Parameters["MOTAmpDDS1"]) / ((double)(int)Parameters["v0IntensityRampDuration"]);
        //Parameters["RampAmplitudeDDS2"] = ((double)Parameters["RampEndAmpDDS2"] - (double)Parameters["MOTAmpDDS2"]) / ((double)(int)Parameters["v0IntensityRampDuration"]);
        //Parameters["RampAmplitudeDDS3"] = ((double)Parameters["RampEndAmpDDS3"] - (double)Parameters["MOTAmpDDS3"]) / ((double)(int)Parameters["v0IntensityRampDuration"]);
        //Parameters["RampAmplitudeDDS4"] = ((double)Parameters["RampEndAmpDDS4"] - (double)Parameters["MOTAmpDDS4"]) / ((double)(int)Parameters["v0IntensityRampDuration"]);

        Parameters["dummy"] = 0.0;


        Parameters["LightoffDDS1"] = 0.0;
        Parameters["LightoffDDS2"] = 0.0;
        Parameters["LightoffDDS3"] = 0.0;
        Parameters["LightoffDDS4"] = 0.0;

        Parameters["LambdaF1minus"] = 97.28;
        Parameters["LambdaF1plus"] = 171.00;

        Parameters["LambdaCoolingDuration"] = 300;
        Parameters["LambdaCooling2Duration"] = 300;

        //Conveyor belt frequency
        Parameters["FreqCVB1"] = 102.5; //+ F = 1Delta 
        // Parameters["FreqCVB2"] = 163.20;
        Parameters["FreqCVB2"] = 139.85;
        // Parameters["FreqCVB2"] = 0.0;
        Parameters["FreqCVB3"] = 175.88; //- F = 2sig- delta_a
        Parameters["FreqCVB4"] = 176.91; //+ F = 2sig+ delta_b

        Parameters["ResonanceDDS1"] = 111.42;
        Parameters["ResonanceDDS2"] = 156.17;
        Parameters["ResonanceDDS3"] = 188.04;
        Parameters["ResonanceDDS4"] = 174.29;


        Parameters["BlueMOTField"] = 1.42;
        Parameters["BlueMOTRampDuration"] = 4000;
        Parameters["BlueMOTDuration"] = 1000;
        Parameters["FreeExpTime"] = 1000;

        Parameters["TransportHoldDur"] = 500000;

        Parameters["MagtrapDuration"] = 1000;

        Parameters["HandoverDur"] = 10000;

        // END OF PATTERN //

        Parameters["MOTCoilsSwitchOff"] = 40000;

        Parameters["Delay"] = 2000;


        // Typed into this script before snippets; the snippets read them as parameters.
        Parameters["TCoolSidebandVCOValue"] = -3.461;
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
        Parameters["TransferHandoverEndTime"] = (int)Parameters["TransferHandoverStartTime"] + (int)Parameters["HandoverDur"];
        Parameters["ImageTime"] = (int)Parameters["TransferHandoverEndTime"] + (int)Parameters["TransportHoldDur"];
        Parameters["TransferCoilsOffTime"] = (int)Parameters["ImageTime"] + (int)Parameters["Frame0TriggerDuration"];

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
        p.AddEvent("image", (int)Parameters["ImageTime"], CaFDDS.BlueMOTFrequencies(Parameters), CaFDDS.BlueMOTAmplitudes(Parameters));

        return p.Pattern;
    }

    public override PatternBuilder32 GetDigitalPattern()
    {
        PatternBuilder32 p = new PatternBuilder32();
        SetTimeline();
        int patternStartBeforeQ = (int)Parameters["TCLBlockStart"];

        new LoadMOT(p, Parameters);

        // CAMERA //
        p.Pulse(patternStartBeforeQ, (int)Parameters["ImageTime"], (int)Parameters["Frame0TriggerDuration"], "cameraTrigger");  //camera trigger for MOT recap

        // MOT Shutters
        p.Pulse(patternStartBeforeQ, (int)Parameters["BlueMOTEndTime"] - 1600, (int)Parameters["MagtrapDuration"], "MOT1Shutter");
        p.Pulse(patternStartBeforeQ, (int)Parameters["BlueMOTEndTime"] - 600, (int)Parameters["MagtrapDuration"] - 950, "MOT2Shutter");
        p.Pulse(patternStartBeforeQ, (int)Parameters["BlueMOTEndTime"] - 800, (int)Parameters["MagtrapDuration"] - 500, "MOT3Shutter");

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

        new CompressedMOT(p, Parameters);
        new LambdaCooling1(p, Parameters);
        new BlueMOT(p, Parameters);
        new LambdaCooling2(p, Parameters);
        new Magtrap(p, Parameters);
        new HandoverToTransferCoils(p, Parameters);
        p.AddLinearRamp("transferCoils", (int)Parameters["TransferCoilsOffTime"], (int)Parameters["HandoverDur"], 0.0);

        return p;
    }
}
