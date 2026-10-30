using MOTMaster;
using MOTMaster.SnippetLibrary;

using System;
using System.Collections.Generic;

using DAQ.Pattern;
using DAQ.Analog;

// Linked-mode demo: Rb atoms into the tweezers (RbTweezer) and a CaF blue MOT (PH-BONESAW) in the same shot,
// each imaged by its own camera. Run on PH-BONESAW with "Linked" ticked; without linked mode this script
// fails, because the t* channels are RbTweezer channels.
//
// The CaF half is "2.a DDSBlueMOT.cs" and the Rb half is "RbTweezerBasic2026_2.cs", each with its own timing:
//   CaF  Q-switch at TCLBlockStart (40 ms), blue MOT imaged ~150-160 ms into the shot.
//   Rb   MOT load from 0.1 ms, molasses, tweezer images at ~410 ms and ~470 ms.
//
// Timing rules in a linked script:
//   Digital times (both machines) count from the PH-BONESAW pattern start, as in both original scripts.
//   Analog times (both machines) count from DDS_Analog_Trg, i.e. TCLBlockStart. The Rb analog times
//   therefore go through RbAnalogTime(), which subtracts TCLBlockStart, so they play exactly when they
//   did in RbTweezerBasic2026_2.cs.
//   SecondaryTriggerDelay comes from globalParameters.json.
//
// Differences from the two originals:
//   PatternLength is 60000 (Rb's). The CaF snippet is given CaFPatternLength (2.a's 50000) instead, so
//   flowEnable is still on to 499 ms.
//   Rb's TCLBlockStart = 0 is dropped: CaF needs the global value.
//   Rb's aoTrigger pulse is dropped: RbTweezer's analog board starts on its pattern generator's start trigger.
public class Patterns : MOTMasterScript
{
    public Patterns()
    {
        Parameters = new Dictionary<string, object>();
        LoadGlobalParameters();

        Parameters["PatternLength"] = 60000;

        // ---- CaF (2.a DDSBlueMOT) ----
        Parameters["CaFPatternLength"] = 50000; // 2.a's PatternLength: sets how long flowEnable is on
        Parameters["BMTriggerDuration"] = 1000;
        Parameters["BlueMOTDuration"] = 1000;
        Parameters["FreeExpTime"] = 1;

        // ---- Rb (RbTweezerBasic2026_2) ----
        // General
        Parameters["tScanCount"] = 0;
        Parameters["BackgroundDrop"] = 0;
        Parameters["TweezerModulation"] = 0;

        // Offset Lock
        Parameters["tRbOffsetVoltageRep"] = 1.675; // 1.8 V
        Parameters["tRbOffsetVoltageCool"] = 3.3; //3.2 V

        // MOT coils
        Parameters["tMOTccValue"] = 0.48; //0.48
        Parameters["tcMOTccValue"] = 0.0;
        Parameters["tMOTccSwitch"] = 5.5;

        // Shim Coils Default
        Parameters["tMOTShimXccValue"] = 1.07; // 8.8; //8.8; //  0.0
        Parameters["tMOTShimYccValue"] = 0.0; // 3.8;//3.8; // -2.0
        Parameters["tMOTShimZccValue"] = -0.47; // 0.3; //0.3; // -0.85

        // Shim Coils Molasses
        Parameters["tMolassesShimXccValue"] = 10.0000; // 8.8 // 10.0
        Parameters["tMolassesShimYccValue"] = 2.80; // 3.8 // 3.29
        Parameters["tMolassesShimZccValue"] = 0.408; // 0.3 // 0.52

        // tRb AOMs
        // MOT Cooling and Repump - Loading
        Parameters["tMOTLoadCoolVCO"] = 4.0; //Cool at 2.4 - 2.6
        Parameters["tMOTLoadRepVCO"] = 1.84;
        Parameters["tMOTLoadCoolVVA"] = 1.00; // between 0 and 1 V
        Parameters["tMOTLoadRepVVA"] = 1.00;

        // MOT Cooling and Repump - Compression
        Parameters["tcMOTLoadCoolVCO"] = 4.0; //Cool at 2.4 - 2.6
        Parameters["tcMOTLoadRepVCO"] = 3.0;
        Parameters["tcMOTLoadCoolVVA"] = 1.00; // between 0 and 1 V
        Parameters["tcMOTLoadRepVVA"] = 1.00;

        // Molasses Cooling and Repump VCO & VVA
        Parameters["tMolCoolVCO1"] = 1.10;
        Parameters["tMolRepVCO1"] = 3.00;
        Parameters["tMolCoolVVA1"] = 1.0;
        Parameters["tMolRepVVA1"] = 0.8;

        Parameters["tMolCoolVCO2"] = 2.73;
        Parameters["tMolRepVCO2"] = 3.00;
        Parameters["tMolCoolVVA2"] = 0.69;
        Parameters["tMolRepVVA2"] = 1.0;

        // Imaging Cooling and Repump VCO & VVA
        Parameters["tImgCoolVCO1"] = 3.0; // 3.6 - 4.0, resonance is 4.0
        Parameters["tImgRepVCO1"] = 3.00; // 3.0
        Parameters["tImgCoolVVA1"] = 1.5;
        Parameters["tImgRepVVA1"] = 1.0;

        Parameters["tImgCoolVCO2"] = 2.948; // 3.6 - 4.0, resonance is 4.0
        Parameters["tImgRepVCO2"] = 3.00; // 3.0
        Parameters["tImgCoolVVA2"] = 1.5;
        Parameters["tImgRepVVA2"] = 1.0;

        // Tweezer Control
        Parameters["tTweezerSetVCO"] = 9.0; // 10 -> 110 MHz
        Parameters["tTweezerLoadVCO"] = 9.0; //
        Parameters["tTweezerImgVCO"] = 9.0; // 1 -> 100% power
        Parameters["tTweezerSetVVA0"] = 0.0;
        Parameters["tTweezerSetVVA1"] = 0.0;
        Parameters["tTweezerModulationVCF"] = 10.0;

        // Timing
        Parameters["tMOTLoadingStart"] = 10;
        Parameters["tMOTLoadingDuration"] = 30000;
        Parameters["tMOTCompressionDuration"] = 1;
        Parameters["tMolassesDuration"] = 10000;
        Parameters["tDelay1"] = 1000;
        Parameters["tDelay2"] = 800;
        Parameters["tMOTExposure"] = 1;
        Parameters["tTweezerExposure1"] = 5000;
        Parameters["tTweezerExposure2"] = 5000;

        Parameters["tTweezerModulationDuration"] = 2000;
        Parameters["tDropDuration"] = 800;
    }

    // An Rb time (counted from the pattern start, as in RbTweezerBasic2026_2.cs) in the analog frame,
    // which counts from DDS_Analog_Trg.
    private int RbAnalogTime(int t)
    {
        return t - (int)Parameters["TCLBlockStart"];
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
        // As in 2.a DDSBlueMOT: LightoffDDS2 on all four channels, not LightoffDDS1 to 4.
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

        // ================= CaF (2.a DDSBlueMOT) =================

        int patternStartBeforeQ = (int)Parameters["TCLBlockStart"];
        int CompressRampDownStartTime = (int)Parameters["CompressRampDownStartTime"] + patternStartBeforeQ;
        int CompressRampDownEndTime = CompressRampDownStartTime + (int)Parameters["CompressRampDownDuration"];
        int lambdaCoolingStart = CompressRampDownEndTime + (int)Parameters["CompressRampDownHoldDuration"];
        int BlueMOTRampStart = lambdaCoolingStart + (int)Parameters["LambdaCoolingDuration"];
        int BlueMOTRampEnd = BlueMOTRampStart + (int)Parameters["BlueMOTRampDuration"];
        int BlueMOTEnd = BlueMOTRampEnd + (int)Parameters["BlueMOTDuration"];
        int imageTime = BlueMOTEnd + (int)Parameters["FreeExpTime"];

        // Time of flight PMT trigger
        p.Pulse(patternStartBeforeQ, 2000, 10, "tofTrigger");

        // CAMERA //
        p.Pulse(0, imageTime - (int)Parameters["BMTriggerDuration"], (int)Parameters["BMTriggerDuration"], "cameraTrigger");  //camera trigger imaging blue mot

        // preset load. The snippet's flowEnable pulse lasts PatternLength - 100, so give it CaF's pattern length.
        Dictionary<string, object> cafParameters = new Dictionary<string, object>(Parameters);
        cafParameters["PatternLength"] = (int)Parameters["CaFPatternLength"];
        MOTMasterScriptSnippet lm = new LoadMoleculeMOTNoSlowingEdge(p, cafParameters);
        p.Pulse(patternStartBeforeQ, 0, (int)Parameters["QSwitchPulseDuration"], "DDS_Analog_Trg");

        // Slowing AOMs
        p.Pulse(patternStartBeforeQ, (int)Parameters["SlowingChirpStartTime"], (2 * (int)Parameters["SlowingChirpDuration"]) + 20000, "bXLockBlock"); // Want it to be blocked for whole time that bX laser is moved
        p.Pulse(patternStartBeforeQ, (int)Parameters["SlowingChirpStartTime"] - 100, (int)Parameters["SlowingChirpDuration"] + 100, "bXSlowingAOM"); //first pulse to slowing AOM
        p.Pulse(patternStartBeforeQ, (int)Parameters["slowingRepumpAOMOnStart"], (int)Parameters["SlowingChirpStartTime"] + (int)Parameters["SlowingChirpDuration"] - (int)Parameters["slowingRepumpAOMOnStart"], "v10SlowingAOM"); //first pulse to slowing repump AOM

        // BX Shutter
        p.Pulse(patternStartBeforeQ, (int)Parameters["SlowingChirpStartTime"] + (int)Parameters["SlowingChirpDuration"] + 200, (int)Parameters["MOTCoilsSwitchOff"] - ((int)Parameters["SlowingChirpStartTime"] + (int)Parameters["SlowingChirpDuration"] + 200), "bXSlowingShutter");

        // ================= Rb (RbTweezerBasic2026_2) =================

        int MOTLoadingStart = (int)Parameters["tMOTLoadingStart"];
        int MOTCompressionStart = MOTLoadingStart + (int)Parameters["tMOTLoadingDuration"];
        int MolassesStart = MOTCompressionStart + (int)Parameters["tMOTCompressionDuration"] + 1;
        int Delay1Start = MolassesStart + (int)Parameters["tMolassesDuration"];
        int Img1Start = Delay1Start + (int)Parameters["tDelay1"];
        int Delay2Start = Img1Start + (int)Parameters["tTweezerExposure1"];
        int Img2Start = Delay2Start + (int)Parameters["tDelay2"];
        int End = (int)Parameters["PatternLength"] - 100;

        p.AddEdge("tVerticalShutter", MOTLoadingStart, true);

        // D1 Light
        p.AddEdge("tD1AOMSwitch", MOTLoadingStart, true); // Turn off D1 light for sequence

        // Rb87 D2 Cooling and Repump Light - Loading Stage
        p.AddEdge("tCoolSwitch", MOTLoadingStart, true);
        p.AddEdge("tRepSwitch", MOTLoadingStart, true);

        p.AddEdge("tCoolSwitch", Delay1Start, false);
        p.AddEdge("tRepSwitch", Delay1Start - 100, false);

        // Tweezer
        p.AddEdge("tTweezerModSwitch", MOTLoadingStart, true); // Turn OFF tweezer modulation

        if ((int)Parameters["TweezerModulation"] == 1)
        {
            p.AddEdge("tTweezerModSwitch", Delay2Start, false); // Turn ON tweezer modulation
            p.AddEdge("tTweezerModSwitch", Delay2Start + (int)Parameters["tTweezerModulationDuration"], true); // Turn OFF tweezer modulation
        }

        p.AddEdge("tTweezerSwitch", MOTLoadingStart, true); // Turn OFF tweezer light
        p.AddEdge("tTweezerSwitch", MOTCompressionStart, false); // Turn ON tweezer light for loading

        if ((int)Parameters["BackgroundDrop"] == 1)
        {
            p.AddEdge("tTweezerSwitch", Delay2Start, true); // Turn OFF tweezer switch after imaging to drop atoms
            p.AddEdge("tTweezerSwitch", Delay2Start + (int)Parameters["tDropDuration"], false); // Turn ON tweezer switch after background delay to img background
        }

        // Imaging 1
        p.Pulse(Img1Start, 0, (int)Parameters["tTweezerExposure1"], "tCoolSwitch");
        p.Pulse(Img1Start, 0, (int)Parameters["tTweezerExposure1"], "tRepSwitch");
        p.Pulse(Img1Start, 0, (int)Parameters["tTweezerExposure1"], "tHamCamTrig");         // Tweezer imaging trigger - initial image
        p.Pulse(Img1Start, 0, (int)Parameters["tMOTExposure"], "tMOTCamTrig");              // MOT imaging trigger

        // Imaging 2
        p.Pulse(Img2Start, 0, (int)Parameters["tTweezerExposure2"], "tCoolSwitch");
        p.Pulse(Img2Start, 0, (int)Parameters["tTweezerExposure2"], "tRepSwitch");
        p.Pulse(Img2Start, 0, (int)Parameters["tTweezerExposure2"], "tHamCamTrig");         // Tweezer imaging trigger - final image

        // END
        p.AddEdge("tCoolSwitch", End, true);
        p.AddEdge("tRepSwitch", End, true);
        p.AddEdge("tD1AOMSwitch", End, false); // Turn on D1 light

        return p;
    }

    public override AnalogPatternBuilder GetAnalogPattern()
    {
        AnalogPatternBuilder p = new AnalogPatternBuilder((int)Parameters["PatternLength"]);

        // ================= CaF (2.a DDSBlueMOT) =================

        int CompressRampDownStartTime = (int)Parameters["CompressRampDownStartTime"];
        int CompressRampDownEndTime = CompressRampDownStartTime + (int)Parameters["CompressRampDownDuration"];
        int lambdaCoolingStart = CompressRampDownEndTime + (int)Parameters["CompressRampDownHoldDuration"];
        int BlueMOTRampStart = lambdaCoolingStart + (int)Parameters["LambdaCoolingDuration"];
        int BlueMOTRampEnd = BlueMOTRampStart + (int)Parameters["BlueMOTRampDuration"];
        int BlueMOTEnd = BlueMOTRampEnd + (int)Parameters["BlueMOTDuration"];
        int imageTime = BlueMOTEnd + (int)Parameters["FreeExpTime"];

        MOTMasterScriptSnippet lm = new LoadMoleculeMOTNoSlowingEdge(p, Parameters);

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

        // Shim Fields
        p.AddAnalogValue("xShimCoilCurrent", 0, (double)Parameters["xShimLoadCurrent"]);
        p.AddAnalogValue("yShimCoilCurrent", 0, (double)Parameters["yShimLoadCurrent"]);
        p.AddAnalogValue("zShimCoilCurrent", 0, (double)Parameters["zShimLoadCurrent"]);

        // SLOWING //
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

        // MOT B field
        p.AddAnalogValue("MOTCoilsCurrent", 0, (double)Parameters["MOTCoilsCurrentValue"]);

        // CMOT //
        p.AddLinearRamp("MOTCoilsCurrent", CompressRampDownStartTime, (int)Parameters["CompressRampDownDuration"], (double)Parameters["MOTCoilsCompressionValue"]);
        p.AddAnalogValue("MOTCoilsCurrent", lambdaCoolingStart, (double)Parameters["MOTCoilsOffValue"]); // switch off for molasses
        p.AddLinearRamp("MOTCoilsCurrent", BlueMOTRampStart, (int)Parameters["BlueMOTRampDuration"], (double)Parameters["BlueMOTField"]);
        p.AddAnalogValue("MOTCoilsCurrent", BlueMOTEnd, (double)Parameters["MOTCoilsOffValue"]);
        p.AddAnalogValue("MOTCoilsCurrent", (int)Parameters["MOTCoilsSwitchOff"], 0.0);
        p.AddAnalogValue("MOTCoilsCurrent", imageTime - 100, 1.0);
        p.AddAnalogValue("MOTCoilsCurrent", imageTime + (int)Parameters["Frame0TriggerDuration"], 0.0);

        // ================= Rb (RbTweezerBasic2026_2) =================
        // Every Rb time goes through RbAnalogTime(): see the note at the top.

        int MOTLoadingStart = RbAnalogTime((int)Parameters["tMOTLoadingStart"]);
        int MOTCompressionStart = MOTLoadingStart + (int)Parameters["tMOTLoadingDuration"];
        int MolassesStart = MOTCompressionStart + (int)Parameters["tMOTCompressionDuration"] + 1;
        int Delay1Start = MolassesStart + (int)Parameters["tMolassesDuration"];
        int Img1Start = Delay1Start + (int)Parameters["tDelay1"];
        int Delay2Start = Img1Start + (int)Parameters["tTweezerExposure1"];
        int Img2Start = Delay2Start + (int)Parameters["tDelay2"];
        int End = RbAnalogTime((int)Parameters["PatternLength"] - 100);

        p.AddChannel("tMOTcc");
        p.AddChannel("tShimXcc");
        p.AddChannel("tShimYcc");
        p.AddChannel("tShimZcc");
        p.AddChannel("tRbCoolVCO");
        p.AddChannel("tRbRepVCO");
        p.AddChannel("tRbD1VCO");
        p.AddChannel("tRbD1VVA");
        p.AddChannel("tMOTccSwitch");
        p.AddChannel("tD1SidebandControl");
        p.AddChannel("tRbD2CoolVVA");
        p.AddChannel("tRbD2RepVVA");
        p.AddChannel("tRbD2RepOffset");
        p.AddChannel("tRbD2CoolOffset");
        p.AddChannel("tTweezerVCO");
        p.AddChannel("tTweezerVCA");
        p.AddChannel("tTweezerModVCF");

        // General
        p.AddAnalogValue("tRbD2RepOffset", MOTLoadingStart, (double)Parameters["tRbOffsetVoltageRep"]); // Offset Lock
        p.AddAnalogValue("tRbD2CoolOffset", MOTLoadingStart, (double)Parameters["tRbOffsetVoltageCool"]); // Offset Lock

        //// MOT COIL CURRENT CONTROL ////
        // MOT Coils - Switch
        p.AddAnalogValue("tMOTccSwitch", MOTLoadingStart, 0.0); // MOT coils switch-on (RSD)
        p.AddAnalogValue("tMOTccSwitch", MolassesStart, (double)Parameters["tMOTccSwitch"]); // MOT coils switch-off (RSD)

        // MOT Coils - Current Control
        p.AddAnalogValue("tMOTcc", MOTLoadingStart, (double)Parameters["tMOTccValue"]); // MOT Mag field
        p.AddLinearRamp("tMOTcc", MOTCompressionStart, (int)Parameters["tMOTCompressionDuration"], (double)Parameters["tcMOTccValue"]); // Ramp up magnetic field
        p.AddAnalogValue("tMOTcc", MOTCompressionStart + (int)Parameters["tMOTCompressionDuration"], 0.0);

        //// SHIM COIL CURRENT CONTROL ////
        // Shim Coils - MOT
        p.AddAnalogValue("tShimXcc", MOTLoadingStart, (double)Parameters["tMOTShimXccValue"]); // Default shim coils X
        p.AddAnalogValue("tShimYcc", MOTLoadingStart, (double)Parameters["tMOTShimYccValue"]); // Default shim coils Y
        p.AddAnalogValue("tShimZcc", MOTLoadingStart, (double)Parameters["tMOTShimZccValue"]); // Default shim coils Z

        // Shim Coils - Molasses
        p.AddAnalogValue("tShimXcc", MolassesStart, (double)Parameters["tMolassesShimXccValue"]); // Molasses shim coils X
        p.AddAnalogValue("tShimYcc", MolassesStart, (double)Parameters["tMolassesShimYccValue"]); // Molasses shim coils Y
        p.AddAnalogValue("tShimZcc", MolassesStart, (double)Parameters["tMolassesShimZccValue"]); // Molasses shim coils Z

        p.AddAnalogValue("tShimXcc", End, (double)Parameters["tMOTShimXccValue"]); // End shim coils X
        p.AddAnalogValue("tShimYcc", End, (double)Parameters["tMOTShimYccValue"]); // End shim coils Y
        p.AddAnalogValue("tShimZcc", End, (double)Parameters["tMOTShimZccValue"]); // End shim coils Z

        //// D2 LIGHT CONTROL ////
        // MOT Loading
        p.AddAnalogValue("tRbCoolVCO", MOTLoadingStart, (double)Parameters["tMOTLoadCoolVCO"]);
        p.AddAnalogValue("tRbRepVCO", MOTLoadingStart, (double)Parameters["tMOTLoadRepVCO"]);
        p.AddAnalogValue("tRbD2CoolVVA", MOTLoadingStart, (double)Parameters["tMOTLoadCoolVVA"]);
        p.AddAnalogValue("tRbD2RepVVA", MOTLoadingStart, (double)Parameters["tMOTLoadRepVVA"]);

        // MOT Compression
        p.AddLinearRamp("tRbCoolVCO", MOTCompressionStart, (int)Parameters["tMOTCompressionDuration"], (double)Parameters["tcMOTLoadCoolVCO"]);
        p.AddLinearRamp("tRbRepVCO", MOTCompressionStart, (int)Parameters["tMOTCompressionDuration"], (double)Parameters["tcMOTLoadRepVCO"]);
        p.AddLinearRamp("tRbD2CoolVVA", MOTCompressionStart, (int)Parameters["tMOTCompressionDuration"], (double)Parameters["tcMOTLoadCoolVVA"]);
        p.AddLinearRamp("tRbD2RepVVA", MOTCompressionStart, (int)Parameters["tMOTCompressionDuration"], (double)Parameters["tcMOTLoadRepVVA"]);

        // Molasses
        p.AddAnalogValue("tRbCoolVCO", MolassesStart - 1, (double)Parameters["tMolCoolVCO1"]);
        p.AddAnalogValue("tRbRepVCO", MolassesStart - 1, (double)Parameters["tMolRepVCO1"]);
        p.AddAnalogValue("tRbD2CoolVVA", MolassesStart - 1, (double)Parameters["tMolCoolVVA1"]);
        p.AddAnalogValue("tRbD2RepVVA", MolassesStart - 1, (double)Parameters["tMolRepVVA1"]);

        p.AddLinearRamp("tRbCoolVCO", MolassesStart, (int)Parameters["tMolassesDuration"], (double)Parameters["tMolCoolVCO2"]);
        p.AddLinearRamp("tRbRepVCO", MolassesStart, (int)Parameters["tMolassesDuration"], (double)Parameters["tMolRepVCO2"]);
        p.AddLinearRamp("tRbD2CoolVVA", MolassesStart, (int)Parameters["tMolassesDuration"], (double)Parameters["tMolCoolVVA2"]);
        p.AddLinearRamp("tRbD2RepVVA", MolassesStart, (int)Parameters["tMolassesDuration"], (double)Parameters["tMolRepVVA2"]);

        //// IMAGING ////
        // Initial Imaging
        p.AddAnalogValue("tRbCoolVCO", Img1Start, (double)Parameters["tImgCoolVCO1"]);
        p.AddAnalogValue("tRbRepVCO", Img1Start, (double)Parameters["tImgRepVCO1"]);
        p.AddAnalogValue("tRbD2CoolVVA", Img1Start, (double)Parameters["tImgCoolVVA1"]);
        p.AddAnalogValue("tRbD2RepVVA", Img1Start, (double)Parameters["tImgRepVVA1"]);

        // Final Imaging
        p.AddAnalogValue("tRbCoolVCO", Img2Start, (double)Parameters["tImgCoolVCO2"]);
        p.AddAnalogValue("tRbRepVCO", Img2Start, (double)Parameters["tImgRepVCO2"]);
        p.AddAnalogValue("tRbD2CoolVVA", Img2Start, (double)Parameters["tImgCoolVVA2"]);
        p.AddAnalogValue("tRbD2RepVVA", Img2Start, (double)Parameters["tImgRepVVA2"]);

        //// TWEEZER CONROL ////
        // Tweezer Modulation
        p.AddAnalogValue("tTweezerModVCF", MOTLoadingStart, (double)Parameters["tTweezerModulationVCF"]);

        // Tweezer VCO and VVA
        p.AddAnalogValue("tTweezerVCO", MOTLoadingStart, (double)Parameters["tTweezerSetVCO"]);
        p.AddAnalogValue("tTweezerVCO", MOTCompressionStart, (double)Parameters["tTweezerLoadVCO"]);
        p.AddAnalogValue("tTweezerVCO", Img1Start, (double)Parameters["tTweezerImgVCO"]); // Shift tweezer power to imaging power for consistent AC Stark shift
        p.AddAnalogValue("tTweezerVCA", MOTLoadingStart, (double)Parameters["tTweezerSetVVA0"]);
        p.AddLinearRamp("tTweezerVCA", Img2Start - 2500, 2500, (double)Parameters["tTweezerSetVVA1"]);
        p.AddAnalogValue("tTweezerVCA", End, (double)Parameters["tTweezerSetVVA0"]);

        //// D2 Light End ////
        p.AddAnalogValue("tRbCoolVCO", End, (double)Parameters["tMOTLoadCoolVCO"]);
        p.AddAnalogValue("tRbRepVCO", End, (double)Parameters["tMOTLoadRepVCO"]);
        p.AddAnalogValue("tRbD2CoolVVA", End, (double)Parameters["tMOTLoadCoolVVA"]);
        p.AddAnalogValue("tRbD2RepVVA", End, (double)Parameters["tMOTLoadRepVVA"]);

        return p;
    }
}
