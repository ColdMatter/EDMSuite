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
        Parameters["PatternLength"] = 50000;

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

        // Slowing AOMS

        // Slowing B field

        //Parameters["slowingCoilsOffTime"] = (int)Parameters["slowingAOMOffStart"]; // 1500;

        // MOT LOAD //

        // MOT B field

        // MOT Sidebands

        //freqs

        //amps for max optical power. dont go higher the amplifiers will saturate

        // CMOT //

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

        Parameters["BlueMOTDuration"] = 1000;
        Parameters["FreeExpTime"] = 1;

        // END OF PATTERN //

    }

    public override Dictionary<string, List<List<double>>> GetDDSPattern()
    {
        DDSPatternBuilder p = new DDSPatternBuilder();

        int CompressRampDownStartTime = (int)Parameters["CompressRampDownStartTime"];
        int CompressRampDownEndTime = CompressRampDownStartTime + (int)Parameters["CompressRampDownDuration"];
        int lambdaCoolingStart = CompressRampDownEndTime + (int)Parameters["CompressRampDownHoldDuration"];
        int BlueMOTRampStart = lambdaCoolingStart + (int)Parameters["LambdaCoolingDuration"];
        int BlueMOTRampEnd = BlueMOTRampStart + (int)Parameters["BlueMOTRampDuration"];
        int BlueMOTEnd = BlueMOTRampEnd + (int)Parameters["BlueMOTDuration"];
        int imageTime = BlueMOTEnd + (int)Parameters["FreeExpTime"];

        double[] motFrequencies = {
            (double)Parameters["MOTFreqDDS1"], (double)Parameters["MOTFreqDDS2"],
            (double)Parameters["MOTFreqDDS3"], (double)Parameters["MOTFreqDDS4"] };
        double[] motAmplitudes = {
            (double)Parameters["MOTAmpDDS1"], (double)Parameters["MOTAmpDDS2"],
            (double)Parameters["MOTAmpDDS3"], (double)Parameters["MOTAmpDDS4"] };
        double[] rampAmplitudeSlopes = {
            (double)Parameters["RampAmplitudeDDS1"], (double)Parameters["RampAmplitudeDDS2"],
            (double)Parameters["RampAmplitudeDDS3"], (double)Parameters["RampAmplitudeDDS4"] };
        double[] rampEndAmplitudes = {
            (double)Parameters["RampEndAmpDDS1"], (double)Parameters["RampEndAmpDDS2"],
            (double)Parameters["RampEndAmpDDS3"], (double)Parameters["RampEndAmpDDS4"] };
        double[] lambdaFrequencies = {
            (double)Parameters["LambdaF1minus"], (double)Parameters["MOTFreqDDS2"],
            (double)Parameters["MOTFreqDDS3"], (double)Parameters["LambdaF1plus"] };
        double[] lambdaAmplitudes = {
            (double)Parameters["Lambda1Amp"], (double)Parameters["LightoffDDS2"],
            (double)Parameters["LightoffDDS3"], (double)Parameters["Lambda2Amp"] };
        double[] blueMOTFrequencies = {
            (double)Parameters["FreqCVB1"], (double)Parameters["FreqCVB2"],
            (double)Parameters["FreqCVB3"], (double)Parameters["FreqCVB4"] };
        double[] blueMOTAmplitudes = {
            (double)Parameters["BMOTAmpDDS1"], (double)Parameters["BMOTAmpDDS2"],
            (double)Parameters["BMOTAmpDDS3"], (double)Parameters["BMOTAmpDDS4"] };
        double[] resonanceFrequencies = {
            (double)Parameters["ResonanceDDS1"], (double)Parameters["ResonanceDDS2"],
            (double)Parameters["ResonanceDDS3"], (double)Parameters["ResonanceDDS4"] };
        // As before: LightoffDDS2 on all four channels, not LightoffDDS1 to 4.
        double[] lightOffAmplitudes = {
            (double)Parameters["LightoffDDS2"], (double)Parameters["LightoffDDS2"],
            (double)Parameters["LightoffDDS2"], (double)Parameters["LightoffDDS2"] };

        p.AddEvent("MOT", 0, motFrequencies, motAmplitudes);

        p.AddEvent("RampStart", CompressRampDownStartTime, motFrequencies, motAmplitudes,
            null, rampAmplitudeSlopes);

        p.AddEvent("RampEnd", CompressRampDownEndTime, motFrequencies, rampEndAmplitudes);

        p.AddEvent("LambdaCooling", lambdaCoolingStart, lambdaFrequencies, lambdaAmplitudes);

        p.AddEvent("BlueMOTRampStart", BlueMOTRampStart, blueMOTFrequencies, blueMOTAmplitudes);

        p.AddEvent("FreeExpTime", BlueMOTEnd, resonanceFrequencies, lightOffAmplitudes);

        p.AddEvent("image", imageTime + 500, motFrequencies, motAmplitudes);

        return p.Pattern;
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
        // int imageTime = BlueMOTEnd - (int)Parameters["Frame0TriggerDuration"];
        int imageTime = BlueMOTEnd + (int)Parameters["FreeExpTime"];

        //  SETUP //

        // Time of flight PMT trigger
        p.Pulse(patternStartBeforeQ, 2000, 10, "tofTrigger");

        // CAMERA //

        //p.Pulse(patternStartBeforeQ, (int)Parameters["Frame0Trigger"], (int)Parameters["Frame0TriggerDuration"], "cameraTrigger"); //camera trigger for first frame
        p.Pulse(0, imageTime- (int)Parameters["BMTriggerDuration"], (int)Parameters["BMTriggerDuration"], "cameraTrigger");  //camera trigger imaging blue mot
        //p.Pulse(0, BlueMOTRampEnd, (int)Parameters["Frame0TriggerDuration"], "cameraTrigger");  //camera trigger imaging blue mot
        //p.Pulse(0, imageTime+500, (int)Parameters["Frame0TriggerDuration"], "cameraTrigger");  //camera trigger imaging blue mot
        //p.Pulse(patternStartBeforeQ, (int)Parameters["MOTCoilsSwitchOff"] + 1000, (int)Parameters["Frame0TriggerDuration"], "cameraTrigger");

        // SLOWING //

        // preset load
        MOTMasterScriptSnippet lm = new LoadMoleculeMOTNoSlowingEdge(p, Parameters);
        p.Pulse(patternStartBeforeQ, 0, (int)Parameters["QSwitchPulseDuration"], "DDS_Analog_Trg");

        // Slowing AOMs
        p.Pulse(patternStartBeforeQ, (int)Parameters["SlowingChirpStartTime"], (2 * (int)Parameters["SlowingChirpDuration"]) + 20000, "bXLockBlock"); // Want it to be blocked for whole time that bX laser is moved
        //p.Pulse(patternStartBeforeQ, 100, 100, "bXSlowingAOM"); //first pulse to slowing AOM
        p.Pulse(patternStartBeforeQ, (int)Parameters["SlowingChirpStartTime"] - 100, (int)Parameters["SlowingChirpDuration"] + 100, "bXSlowingAOM"); //first pulse to slowing AOM
        p.Pulse(patternStartBeforeQ, (int)Parameters["slowingRepumpAOMOnStart"], (int)Parameters["SlowingChirpStartTime"] + (int)Parameters["SlowingChirpDuration"] - (int)Parameters["slowingRepumpAOMOnStart"], "v10SlowingAOM"); //first pulse to slowing repump AOM

        // BX Shutter
        p.Pulse(patternStartBeforeQ, (int)Parameters["SlowingChirpStartTime"] + (int)Parameters["SlowingChirpDuration"] + 200, (int)Parameters["MOTCoilsSwitchOff"] - ((int)Parameters["SlowingChirpStartTime"] + (int)Parameters["SlowingChirpDuration"] + 200), "bXSlowingShutter");

        //p.Pulse(patternStartBeforeQ, 0, 100000, "MOT1Shutter");
        //p.Pulse(patternStartBeforeQ, 0, 100000, "MOT2Shutter");
        //p.Pulse(patternStartBeforeQ, 0, 100000, "MOT3Shutter");

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
        int imageTime = BlueMOTEnd + (int)Parameters["FreeExpTime"];

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
        p.AddAnalogValue("MOTCoilsCurrent", (int)Parameters["MOTCoilsSwitchOff"], 0.0);
        p.AddAnalogValue("MOTCoilsCurrent", imageTime - 100, 1.0);
        p.AddAnalogValue("MOTCoilsCurrent", imageTime + (int)Parameters["Frame0TriggerDuration"], 0.0);

        //p.AddAnalogValue("lightSwitch", 1000, 2.0);

        return p;
    }

}
