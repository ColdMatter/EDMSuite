using MOTMaster;
using System;
using System.Collections.Generic;

using DAQ.Pattern;
using DAQ.Analog;


public class Patterns : MOTMasterScript
/*
 * This script is designed to create the blue MOT
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
        Parameters["MOTCompressionDuration"] = 300;
        Parameters["MOTFieldDecayDuration"] = 150;
        Parameters["LambdaMolassesDuration"] = 300;
        Parameters["BlueMOTFirstRampDuration"] = 1000;
        Parameters["BlueMOTSecondRampDuration"] = 1000;

        Parameters["V00R0AOMVCOFreqMOTLoading"] = 3.1;
        Parameters["V00R0AOMVCOFreqMOTRetaining"] = 3.1;
        Parameters["V00R0AOMVCOFreqMOTCompression"] = 3.1;
        Parameters["V00R0AOMVCOFreqImaging"] = 3.1;
        Parameters["V00R0AOMVCOAmpMOTLoading"] = 0.37;
        Parameters["V00R0AOMVCOAmpMOTRetaining"] = 0.40;
        Parameters["V00R0AOMVCOAmpMOTCompression"] = 0.42;
        Parameters["V00R0AOMVCOAmpImaging"] = 0.30;
        Parameters["V00R1plusAOMAmpMOTLoading"] = 0.65;
        Parameters["V00R1plusAOMAmpMOTRetaining"] = 0.7;
        Parameters["V00R1plusAOMAmpMOTCompression"] = 0.8;
        Parameters["V00R1plusAOMAmpBlueMOT"] = 0.8;
        Parameters["V00R1plusAOMAmpImaging"] = 0.8;
        Parameters["V00B2AOMAmpMolasses"] = 1.0;
        Parameters["V00B2AOMAmpBlueMOT"] = 1.0;

        // B Field
        Parameters["SlowingCoilFieldValue"] = 2.0;
        Parameters["MOTCoilsStartTime"] = 0;
        Parameters["MOTCoilsStopTime"] = 10000;
        Parameters["MOTLoadingFieldValue"] = 3.0;
        Parameters["MOTCompressionFieldValue"] = 6.0;
        Parameters["BlueMOTCoilsFirstOnValue"] = -2.0;
        Parameters["BlueMOTCoilsSecondOnValue"] = -6.0;
        Parameters["MOTCoilsOffValue"] = 0.0;

        // Shim Coils
        Parameters["XShimCoilsOnValue"] = -5.0;
        Parameters["YShimCoilsOnValue"] = -4.0;
        Parameters["ZShimCoilsOnValue"] = 0.0;
        Parameters["XShimCoilsBlueMOTValue"] = -5.0;
        Parameters["YShimCoilsBlueMOTValue"] = -4.0;
        Parameters["ZShimCoilsBlueMOTValue"] = 0.0;
        Parameters["XShimCoilsOffValue"] = 0.0;
        Parameters["YShimCoilsOffValue"] = 0.0;
        Parameters["ZShimCoilsOffValue"] = 0.0;

        // Camera trigger properties
        Parameters["CameraTriggerStartDelay"] = 100;
        Parameters["CameraTriggerDuration"] = 200;

        // Switching control for iterative operations.
        // values higher than 5.0 leads to active state.
        Parameters["yagONorOFF"] = 10.0;
        Parameters["slowingONorOFF"] = 10.0;
        Parameters["shutterONorOFF"] = 1.0;

        Parameters["AD9959ConfigIndex"] = 0;

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
        int lambdaMolassesStart = motCompressionStop + (int)Parameters["MOTFieldDecayDuration"];
        int lambdaMolassesStop = lambdaMolassesStart + (int)Parameters["LambdaMolassesDuration"];
        int blueMOTFirstStop = lambdaMolassesStop + (int)Parameters["BlueMOTFirstRampDuration"];
        int blueMOTSecondStop = blueMOTFirstStop + (int)Parameters["BlueMOTSecondRampDuration"];
        int cameraStart = blueMOTSecondStop + (int)Parameters["CameraTriggerStartDelay"];
        int cameraStop = cameraStart + (int)Parameters["CameraTriggerDuration"];

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
            cameraStart,
            cameraStop-cameraStart,
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
            cameraStart,
            cameraStop-cameraStart,
            "V00R1plusAOM"
        );

        p.Pulse(
            patternStartBeforeQ,
            lambdaMolassesStop,
            blueMOTSecondStop - lambdaMolassesStop,
            "V00R1plusAOMJump"
        );

        p.Pulse(
            patternStartBeforeQ,
            (int)Parameters["V00AOMONStartTime"],
            cameraStop,
            "V00R0EOM"
        );

        p.Pulse(
            patternStartBeforeQ,
            lambdaMolassesStart,
            blueMOTSecondStop - lambdaMolassesStart,
            "V00B2AOM"
        );

        p.Pulse(
            patternStartBeforeQ,
            lambdaMolassesStart,
            lambdaMolassesStop - lambdaMolassesStart,
            "V00B1minusAOM"
        );

        p.Pulse(
            patternStartBeforeQ,
            lambdaMolassesStop,
            blueMOTSecondStop - lambdaMolassesStop,
            "V00B1minusAOMJump"
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
            cameraStart,
            (int)Parameters["CameraTriggerDuration"],
            "cameraTrigger"
        );

        // For dipole trap probe alignment
        /*
        if ((double)Parameters["shutterONorOFF"] > 5.0)
        {
            p.Pulse(
                patternStartBeforeQ,
                0,
                cameraStart,
                "shutter"
            );
        }
        */
        return p;
    }

    public override AnalogPatternBuilder GetAnalogPattern()
    {
        AnalogPatternBuilder p = new AnalogPatternBuilder((int)Parameters["PatternLength"]);
        p.AddChannel("BXChirp");
        p.AddChannel("SlowingBField");
        p.AddChannel("V00R0AOMVCOFreq");
        p.AddChannel("V00R0AOMVCOAmp");
        p.AddChannel("V00R1plusAOMAmp");
        p.AddChannel("V00B2AOMAmp");
        p.AddChannel("motCoils");
        p.AddChannel("ShimCoilX");
        p.AddChannel("ShimCoilY");
        p.AddChannel("ShimCoilZ");

        int slowingChirpStart = (int)Parameters["BXAOMFreeFlightDuration"] + (int)Parameters["BXAOMPostBunchingDuration"];
        int slowingChirpStop = slowingChirpStart + (int)Parameters["BXAOMChirpDuration"];

        int motLoadingStop = (int)Parameters["MOTLoadingDuration"];
        int motRetainingStop = motLoadingStop + (int)Parameters["MOTRetainingDuration"];
        int motCompressionStop = motRetainingStop + (int)Parameters["MOTCompressionDuration"];
        int lambdaMolassesStart = motCompressionStop + (int)Parameters["MOTFieldDecayDuration"];
        int lambdaMolassesStop = lambdaMolassesStart + (int)Parameters["LambdaMolassesDuration"];
        int blueMOTFirstStop = lambdaMolassesStop + (int)Parameters["BlueMOTFirstRampDuration"];
        int blueMOTSecondStop = blueMOTFirstStop + (int)Parameters["BlueMOTSecondRampDuration"];
        int cameraStart = blueMOTSecondStop + (int)Parameters["CameraTriggerStartDelay"];
        int cameraStop = cameraStart + (int)Parameters["CameraTriggerDuration"];

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

        p.AddLinearRamp(
            "motCoils",
            lambdaMolassesStop,
            (int)Parameters["BlueMOTFirstRampDuration"],
            (double)Parameters["BlueMOTCoilsFirstOnValue"]
        );
        p.AddLinearRamp(
            "motCoils",
            blueMOTFirstStop,
            (int)Parameters["BlueMOTSecondRampDuration"],
            (double)Parameters["BlueMOTCoilsSecondOnValue"]
        );
        p.AddAnalogValue(
           "motCoils",
           blueMOTSecondStop,
           (double)Parameters["MOTCoilsOffValue"]
        );

        p.AddAnalogValue(
            "V00R0AOMVCOFreq",
            0,
            (double)Parameters["V00R0AOMVCOFreqMOTLoading"]
        );
        p.AddAnalogValue(
            "V00R0AOMVCOAmp",
            0,
            (double)Parameters["V00R0AOMVCOAmpMOTLoading"]
        );
        p.AddAnalogValue(
           "V00R1plusAOMAmp",
           0,
           (double)Parameters["V00R1plusAOMAmpMOTLoading"]
        );
        p.AddAnalogValue(
            "V00R0AOMVCOFreq",
            motLoadingStop,
            (double)Parameters["V00R0AOMVCOFreqMOTRetaining"]
        );
        p.AddAnalogValue(
            "V00R0AOMVCOAmp",
            motLoadingStop,
            (double)Parameters["V00R0AOMVCOAmpMOTRetaining"]
        );
        p.AddAnalogValue(
            "V00R1plusAOMAmp",
            motLoadingStop,
            (double)Parameters["V00R1plusAOMAmpMOTRetaining"]
        );
        p.AddAnalogValue(
            "V00R0AOMVCOFreq",
            motRetainingStop,
            (double)Parameters["V00R0AOMVCOFreqMOTCompression"]
        );
        p.AddAnalogValue(
            "V00R0AOMVCOAmp",
            motRetainingStop,
            (double)Parameters["V00R0AOMVCOAmpMOTCompression"]
        );
        p.AddAnalogValue(
            "V00R1plusAOMAmp",
            motRetainingStop,
            (double)Parameters["V00R1plusAOMAmpMOTCompression"]
        );
        p.AddAnalogValue(
            "V00R0AOMVCOFreq",
            motCompressionStop,
            (double)Parameters["V00R0AOMVCOFreqImaging"]
        );
        p.AddAnalogValue(
            "V00R0AOMVCOAmp",
            motCompressionStop,
            (double)Parameters["V00R0AOMVCOAmpImaging"]
        );
        p.AddAnalogValue(
           "V00R1plusAOMAmp",
           motCompressionStop,
           (double)Parameters["V00R1plusAOMAmpBlueMOT"]
        );
        p.AddAnalogValue(
           "V00R1plusAOMAmp",
           cameraStart,
           (double)Parameters["V00R1plusAOMAmpImaging"]
        );
        p.AddAnalogValue(
           "V00B2AOMAmp",
           0,
           (double)Parameters["V00B2AOMAmpMolasses"]
        );
        p.AddAnalogValue(
           "V00B2AOMAmp",
           lambdaMolassesStop,
           (double)Parameters["V00B2AOMAmpBlueMOT"]
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
        p.AddLinearRamp(
            "ShimCoilX",
            lambdaMolassesStop,
            (int)Parameters["BlueMOTFirstRampDuration"],
            (double)Parameters["XShimCoilsBlueMOTValue"]
        );
        p.AddLinearRamp(
            "ShimCoilY",
            lambdaMolassesStop,
            (int)Parameters["BlueMOTFirstRampDuration"],
            (double)Parameters["YShimCoilsBlueMOTValue"]
        );
        p.AddLinearRamp(
           "ShimCoilZ",
           lambdaMolassesStop,
           (int)Parameters["BlueMOTFirstRampDuration"],
           (double)Parameters["ZShimCoilsBlueMOTValue"]
       );
        p.AddAnalogValue(
            "ShimCoilX",
            blueMOTSecondStop,
            (double)Parameters["XShimCoilsOffValue"]
        );
        p.AddAnalogValue(
            "ShimCoilY",
            blueMOTSecondStop,
            (double)Parameters["YShimCoilsOffValue"]
        );
        p.AddAnalogValue(
            "ShimCoilZ",
            blueMOTSecondStop,
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
