using MOTMaster;
using System;
using System.Collections.Generic;

using DAQ.Pattern;
using DAQ.Analog;


public class Patterns : MOTMasterScript
/*
 * This script is designed to create the first MOT
 * The time unit in this script is in multiple of 10 micro second.
 * The other unit is Volt.
 * */
{
    public Patterns()
    {
        Parameters = new Dictionary<string, object>();
        Parameters["PatternLength"] = 50000;
        Parameters["TCLBlockStart"] = 10000;
        Parameters["FlashToQ"] = 16;
        Parameters["QSwitchPulseDuration"] = 10;

        
        // Slowing parameters
        Parameters["BXAOMFreeFlightDuration"] = 450;
        Parameters["BXAOMPostBunchingDuration"] = 100;
        Parameters["BXAOMChirpDuration"] = 550;
        Parameters["BXChirpStartValue"] = 0.0;
        Parameters["SlowingMHzPerVolt"] = -1350.0;             // Azurlight BX laser 1V --> -1350 MHz
        Parameters["SlowingChirpMHzSpan"] = 360.0;             // 564.582580 THz is the resonance with -100 MHz AOM
        
        // cooling and trapping parameters
        Parameters["V00AOMONStartTime"] = 0;
        Parameters["MOTLoadingDuration"] = 2000;
        Parameters["MOTRetainingDuration"] = 500;
        Parameters["MOTCompressionDuration"] = 500;
        Parameters["MolassesDuration"] = 300;
        Parameters["MOTFieldDecayDuration"] = 150;

        Parameters["V00AOMONDuration"] = 10000;
        Parameters["V00MOTLoadingFrequency"] = 3.1;
        Parameters["V00MOTRetainingFrequency"] = 3.1;
        Parameters["V00MOTCompressionFrequency"] = 3.1;
        Parameters["V00MolassesFrequency"] = 4.0;
        Parameters["V00MOTLoadingAmplitude"] = 0.30;
        Parameters["V00MOTRetainingAmplitude"] = 0.40;
        Parameters["V00MOTCompressionAmplitude"] = 0.40;
        Parameters["V00MolassesAmplitude"] = 0.30;
        Parameters["V00AOM2LoadingAmplitude"] = 1.0;
        Parameters["V00AOM2RetainingAmplitude"] = 0.68;
        Parameters["V00AOM2MolassesAmplitude"] = 1.0;
        

        // B Field
        Parameters["SlowingCoilFieldValue"] = 2.0;
        Parameters["MOTCoilsStartTime"] = 0;
        Parameters["MOTCoilsStopTime"] = 10000;
        Parameters["MOTLoadingFieldValue"] = 3.0;              // 3.6 V should be 1 A
        Parameters["MOTCompressionFieldValue"] = 6.0;
        Parameters["MOTCoilsOffValue"] = 0.0;

        // Shim Coils
        Parameters["XShimCoilsOnValue"] = -5.0;
        Parameters["XShimCoilsOffValue"] = 0.0;
        Parameters["YShimCoilsOnValue"] = -2.5;
        Parameters["YShimCoilsOffValue"] = 0.0;
        Parameters["ZShimCoilsOnValue"] = 2.5;
        Parameters["ZShimCoilsOffValue"] = 0.0;

        // Camera trigger properties
        Parameters["CameraTriggerStartDelay"] = 1;
        Parameters["CameraTriggerDuration"] = 200;

        // Switching control for iterative operations.
        // values higher than 5.0 leads to active state.
        Parameters["yagONorOFF"] = 10.0;
        Parameters["slowingONorOFF"] = 10.0;

    }

    public override PatternBuilder32 GetDigitalPattern()
    {
        PatternBuilder32 p = new PatternBuilder32();
        int patternStartBeforeQ = (int)Parameters["TCLBlockStart"];
        int slowingAOMStart = (int)Parameters["BXAOMFreeFlightDuration"];
        int slowingChirpStart = slowingAOMStart + (int)Parameters["BXAOMPostBunchingDuration"];
        int slowingChirpStop = slowingChirpStart + (int)Parameters["BXAOMChirpDuration"];

        int motLoadingStop = (int)Parameters["MOTLoadingDuration"];
        int motRetainingStop = motLoadingStop + (int)Parameters["MOTRetainingDuration"];
        int motCompressionStop = motRetainingStop + (int)Parameters["MOTCompressionDuration"];
        int molassesStop = motCompressionStop + (int)Parameters["MOTFieldDecayDuration"] + (int)Parameters["MolassesDuration"];

        p.Pulse(patternStartBeforeQ, 0, 10, "analogPatternTrigger");
        p.Pulse(
            patternStartBeforeQ,
            0,
            25000,
            "blockTCL"
        );

        p.Pulse(
            patternStartBeforeQ,
            -(int)Parameters["FlashToQ"],
            (int)Parameters["QSwitchPulseDuration"],
            "flash"
        );

        if ((double)Parameters["yagONorOFF"] > 5.0)
        {
            p.Pulse(patternStartBeforeQ, 0, (int)Parameters["QSwitchPulseDuration"], "q");
        }

        p.Pulse(
            patternStartBeforeQ,
            (int)Parameters["V00AOMONStartTime"],
            motCompressionStop,
            "V00R0AOM"
        );

        p.Pulse(
            patternStartBeforeQ,
            motCompressionStop + (int)Parameters["MOTFieldDecayDuration"],
            (int)Parameters["MolassesDuration"],
            "V00R0AOM"
        );

        p.Pulse(
            patternStartBeforeQ,
            molassesStop + (int)Parameters["CameraTriggerStartDelay"],
            (int)Parameters["CameraTriggerDuration"] + 1000,
            "V00R0AOM"
        );

        p.Pulse(
            patternStartBeforeQ,
            (int)Parameters["V00AOMONStartTime"],
            motCompressionStop,
            "V00R1plusAOM"
        );
        
        p.Pulse(
            patternStartBeforeQ,
            molassesStop + (int)Parameters["CameraTriggerStartDelay"],
            (int)Parameters["CameraTriggerDuration"] + 1000,
            "V00R1plusAOM"
        );

        p.Pulse(
            patternStartBeforeQ,
            (int)Parameters["V00AOMONStartTime"],
            molassesStop,
            "V00R0EOM"
        );

        p.Pulse(
            patternStartBeforeQ,
            molassesStop + (int)Parameters["CameraTriggerStartDelay"],
            (int)Parameters["CameraTriggerDuration"] + 1000,
            "V00R0EOM"
        );

        if ((double)Parameters["slowingONorOFF"] > 5.0)
        {
            p.Pulse(
                patternStartBeforeQ,
                slowingAOMStart,
                slowingChirpStop - slowingAOMStart,
                "BXAOM"
            );

            p.Pulse(
                patternStartBeforeQ,
                10,
                10000,
                "BXSidebands"
            );

            p.Pulse(
                patternStartBeforeQ,
                0,
                slowingChirpStop,
                "RepumpAOM"
            );

            p.Pulse(
                patternStartBeforeQ,
                0,
                slowingChirpStop,
                "RepumpBroadening"
            );

        }

        p.Pulse(
            patternStartBeforeQ,
            molassesStop + (int)Parameters["CameraTriggerStartDelay"],
            (int)Parameters["CameraTriggerDuration"],
            "cameraTrigger"
        );

        return p;
    }

    public override AnalogPatternBuilder GetAnalogPattern()
    {
        AnalogPatternBuilder p = new AnalogPatternBuilder((int)Parameters["PatternLength"]);
        p.AddChannel("BXChirp");
        p.AddChannel("V00R0AOMVCOFreq");
        p.AddChannel("V00R0AOMVCOAmp");
        p.AddChannel("motCoils");
        p.AddChannel("ShimCoilX");
        p.AddChannel("ShimCoilY");
        p.AddChannel("ShimCoilZ");
        p.AddChannel("SlowingBField");
        p.AddChannel("V00R1plusAOMAmp");


        int slowingChirpStart = (int)Parameters["BXAOMFreeFlightDuration"] + (int)Parameters["BXAOMPostBunchingDuration"];
        int slowingChirpStop = slowingChirpStart + (int)Parameters["BXAOMChirpDuration"];

        int motLoadingStop = (int)Parameters["MOTLoadingDuration"];
        int motRetainingStop = motLoadingStop + (int)Parameters["MOTRetainingDuration"];
        int motCompressionStop = motRetainingStop + (int)Parameters["MOTCompressionDuration"];
        int molassesStop = motCompressionStop + (int)Parameters["MOTFieldDecayDuration"] + (int)Parameters["MolassesDuration"];

        if ((double)Parameters["slowingONorOFF"] > 5.0)
        {

            p.AddLinearRamp(
                "BXChirp",
                slowingChirpStart,
                slowingChirpStop - slowingChirpStart,
                (double)Parameters["SlowingChirpMHzSpan"] / (double)Parameters["SlowingMHzPerVolt"]
            );
            p.AddLinearRamp(
                "BXChirp",
                slowingChirpStop + 1000,
                1000,
                (double)Parameters["BXChirpStartValue"]
            );
        }

        p.AddAnalogValue(
            "motCoils",
            (int)Parameters["MOTCoilsStartTime"],
            (double)Parameters["MOTLoadingFieldValue"]
        );

        p.AddAnalogValue(
           "motCoils",
           motRetainingStop,
           (double)Parameters["MOTCompressionFieldValue"]
        );

        p.AddAnalogValue(
           "motCoils",
           motCompressionStop,
           (double)Parameters["MOTCoilsOffValue"]
        );        

        p.AddAnalogValue(
            "V00R0AOMVCOFreq",
            0,
            (double)Parameters["V00MOTLoadingFrequency"]
        );
        p.AddAnalogValue(
            "V00R0AOMVCOAmp",
            0,
            (double)Parameters["V00MOTLoadingAmplitude"]
        );
        p.AddAnalogValue(
           "V00R1plusAOMAmp",
           0,
           (double)Parameters["V00AOM2LoadingAmplitude"]
        );
        p.AddAnalogValue(
            "V00R0AOMVCOFreq",
            motLoadingStop,
            (double)Parameters["V00MOTRetainingFrequency"]
        );
        p.AddAnalogValue(
            "V00R0AOMVCOAmp",
            motLoadingStop,
            (double)Parameters["V00MOTRetainingAmplitude"]
        );
        p.AddAnalogValue(
            "V00R1plusAOMAmp",
            motLoadingStop,
            (double)Parameters["V00AOM2RetainingAmplitude"]
        );
        p.AddAnalogValue(
            "V00R0AOMVCOFreq",
            motRetainingStop,
            (double)Parameters["V00MOTCompressionFrequency"]
        );
        p.AddAnalogValue(
            "V00R0AOMVCOAmp",
            motRetainingStop,
            (double)Parameters["V00MOTCompressionAmplitude"]
        );
        p.AddAnalogValue(
            "V00R0AOMVCOFreq",
            motCompressionStop,
            (double)Parameters["V00MolassesFrequency"]
        );
        p.AddAnalogValue(
            "V00R0AOMVCOAmp",
            motCompressionStop,
            (double)Parameters["V00MolassesAmplitude"]
        );
        p.AddAnalogValue(
           "V00R1plusAOMAmp",
           motCompressionStop,
           (double)Parameters["V00AOM2LoadingAmplitude"]
        );
        p.AddAnalogValue(
            "V00R0AOMVCOFreq",
            molassesStop,
            (double)Parameters["V00MOTLoadingFrequency"]
        );
        p.AddAnalogValue(
            "V00R0AOMVCOAmp",
            molassesStop,
            (double)Parameters["V00MOTLoadingAmplitude"]
        );

        p.AddAnalogValue(
            "ShimCoilX",
            0,
            (double)Parameters["XShimCoilsOnValue"]
        );
        p.AddAnalogValue(
            "ShimCoilY",
            0,
            (double)Parameters["YShimCoilsOnValue"]
        );
        p.AddAnalogValue(
            "ShimCoilZ",
            0,
            (double)Parameters["ZShimCoilsOnValue"]
        );
        p.AddAnalogValue(
            "ShimCoilX",
            (int)Parameters["MOTCoilsStopTime"],
            (double)Parameters["XShimCoilsOffValue"]
        );
        p.AddAnalogValue(
            "ShimCoilY",
            (int)Parameters["MOTCoilsStopTime"],
            (double)Parameters["YShimCoilsOffValue"]
        );
        p.AddAnalogValue(
            "ShimCoilZ",
            (int)Parameters["MOTCoilsStopTime"],
            (double)Parameters["ZShimCoilsOffValue"]
        );

        p.AddAnalogValue(
            "SlowingBField",
            0,
            (double)Parameters["SlowingCoilFieldValue"]
        );

        p.AddAnalogValue(
            "SlowingBField",
            slowingChirpStop + 400,
            0.0
        );

        return p;
    }

    public override AnalogStaticBuilder GetAnalogStatic()
    {
        AnalogStaticBuilder p = new AnalogStaticBuilder((int)Parameters["PatternLength"]);

        return p;
    }
}
