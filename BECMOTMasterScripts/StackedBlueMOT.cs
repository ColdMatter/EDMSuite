using MOTMaster;
using System;
using System.Collections.Generic;

using DAQ.Pattern;
using DAQ.Analog;


public class Patterns : MOTMasterScript
/*
 * This script is designed to be stacked with different phases of the 
 * experiment separately.
 * */
{
    public Patterns()
    {
        Parameters = new Dictionary<string, object>();
        Parameters["PatternLength"] = 50000;
        Parameters["yagONorOFF"] = 10.0;
        Parameters["TCLBlockStart"] = 10000;
        Parameters["FlashToQ"] = 16;
        Parameters["QSwitchPulseDuration"] = 10;

        // Slowing parameters
        Parameters["SlowingONorOFF"] = 10.0;
        Parameters["BXAOMFreeFlightDuration"] = 450;
        Parameters["BXAOMPostBunchingDuration"] = 100;
        Parameters["BXAOMChirpDuration"] = 550;
        Parameters["BXChirpStartValue"] = 0.0;
        Parameters["SlowingMHzPerVolt"] = -1350.0;             // Azurlight BX laser 1V --> -1350 MHz
        Parameters["SlowingChirpMHzSpan"] = 360.0;             // 564.582580 THz is the resonance with -100 MHz AOM
        Parameters["SlowingCoilFieldValue"] = 5.0;

        // MOT
        Parameters["MOTONorOFF"] = 10.0;
        Parameters["V00AOMONStartTime"] = 0;
        Parameters["MOTLoadingDuration"] = 3000;
        Parameters["MOTRetainingDuration"] = 500;
        Parameters["V00R0AOMVCOFreqMOTLoading"] = 3.1;
        Parameters["V00R0AOMVCOFreqMOTRetaining"] = 3.1;
        Parameters["V00R0AOMVCOAmpMOTLoading"] = 0.34;
        Parameters["V00R0EOMVCOAmpMOTLoading"] = -1.1;
        Parameters["V00R0AOMVCOAmpMOTRetaining"] = 0.36;
        Parameters["V00R1plusAOMAmpMOTLoading"] = 0.74;
        Parameters["V00R1plusAOMAmpMOTRetaining"] = 0.68;
        Parameters["MOTCoilsStartTime"] = 0;
        Parameters["MOTLoadingFieldValue"] = 3.0;
        Parameters["XShimCoilsMOTValue"] = 0.0;
        Parameters["YShimCoilsMOTValue"] = 0.0;
        Parameters["ZShimCoilsMOTValue"] = 0.0;

        // Compressed MOT
        Parameters["CMOTONorOFF"] = 10.0;
        Parameters["MOTCompressionRampDuration"] = 10;
        Parameters["MOTCompressionDuration"] = 500;
        Parameters["MOTFieldDecayDuration"] = 150;
        Parameters["V00R0AOMVCOFreqMOTCompression"] = 3.1;
        Parameters["V00R0AOMVCOAmpMOTCompression"] = 0.36;
        Parameters["V00R1plusAOMAmpMOTCompression"] = 0.68;
        Parameters["MOTCompressionFieldValue"] = 6.0;

        // Molasses
        Parameters["MolassesONorOFF"] = 10.0;
        Parameters["LambdaMolassesRampDuration"] = 0;
        Parameters["LambdaMolassesHoldDuration"] = 100;
        Parameters["V00B2AOMAmpMolassesStart"] = 1.0;
        Parameters["V00B2AOMAmpMolassesEnd"] = 1.0;
        Parameters["XShimCoilsLambdaMolassesValue"] = 0.0;
        Parameters["YShimCoilsLambdaMolassesValue"] = 0.0;
        Parameters["ZShimCoilsLambdaMolassesValue"] = 0.0;

        // Blue MOT
        Parameters["BlueMOTONorOFF"] = 10.0;
        Parameters["BlueMOTFirstRampStartDelay"] = 0;
        Parameters["BlueMOTFirstRampDuration"] = 1000;
        Parameters["BlueMOTSecondRampDuration"] = 1000;
        Parameters["BlueMOTHoldDuration"] = 1000;

        // Imaging
        Parameters["MOTImageONorOFF"] = 1.0;
        Parameters["CloudImageONorOFF"] = 10.0;
        Parameters["BackgroundImageONorOFF"] = 1.0;
        Parameters["MOTCameraTriggerStartDelay"] = -500;
        Parameters["CameraTriggerStartDelay"] = 100;
        Parameters["CameraTriggerDuration"] = 500;
        Parameters["CameraExposureDelay"] = 78;
        Parameters["BackgroundImageTime"] = 25000;

        // Common and Finishing
        Parameters["V00R0AOMVCOAmpMax"] = 0.30;
        Parameters["V00R1plusAOMAmpMax"] = 0.8;
        Parameters["MOTCoilsOffValue"] = 0.0;
        Parameters["XShimCoilsOffValue"] = 0.0;
        Parameters["YShimCoilsOffValue"] = 0.0;
        Parameters["ZShimCoilsOffValue"] = 0.0;

        // Dummy variables for analysis purposes
        Parameters["AD9959ConfigIndex"] = 0;
        Parameters["2PhotonDetuningDummy"] = 0.0;

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
        int motCompressionStop = motRetainingStop + (int)Parameters["MOTCompressionRampDuration"] + (int)Parameters["MOTCompressionDuration"];
        int motCameraStart = motLoadingStop - (int)Parameters["CameraExposureDelay"] + (int)Parameters["MOTCameraTriggerStartDelay"];
        int lambdaMolassesRampStart = motCompressionStop + (int)Parameters["MOTFieldDecayDuration"];
        int lambdaMolassesStart = lambdaMolassesRampStart + (int)Parameters["LambdaMolassesRampDuration"];
        int lambdaMolassesStop = lambdaMolassesStart + (int)Parameters["LambdaMolassesHoldDuration"];
        int blueMOTFirstRampStart = lambdaMolassesStop + (int)Parameters["BlueMOTFirstRampStartDelay"];
        int blueMOTSecondRampStart = blueMOTFirstRampStart + (int)Parameters["BlueMOTFirstRampDuration"];
        int blueMOTHoldStart = blueMOTSecondRampStart + (int)Parameters["BlueMOTSecondRampDuration"];
        int blueMOTStop = blueMOTHoldStart + (int)Parameters["BlueMOTHoldDuration"];
        int cameraStart = lambdaMolassesStop - (int)Parameters["CameraExposureDelay"] + (int)Parameters["CameraTriggerStartDelay"];
        int cameraStop = cameraStart + (int)Parameters["CameraTriggerDuration"];
        int bgCameraStart = (int)Parameters["BackgroundImageTime"] - (int)Parameters["CameraExposureDelay"];

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

        if ((double)Parameters["SlowingONorOFF"] > 5.0)
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

        // MOT + CMOT
        p.Pulse(
            patternStartBeforeQ,
            (int)Parameters["V00AOMONStartTime"],
            motCompressionStop,
            "V00R0AOM"
        );
        p.Pulse(
            patternStartBeforeQ,
            (int)Parameters["V00AOMONStartTime"],
            motCompressionStop,
            "V00R1plusAOM"
        );
        if ((double)Parameters["MOTImageONorOFF"] > 5.0)
        {
            p.Pulse(
                patternStartBeforeQ,
                motCameraStart,
                (int)Parameters["CameraTriggerDuration"],
                "cameraTrigger"
            );
        }


        // Lambda Molasses + Blue MOT
        p.Pulse(
            patternStartBeforeQ,
            lambdaMolassesRampStart,
            blueMOTStop - lambdaMolassesRampStart,
            "V00B2AOM"
        );
        p.Pulse(
            patternStartBeforeQ,
            lambdaMolassesRampStart,
            blueMOTStop - lambdaMolassesRampStart,
            "V00B1minusAOM"
        );

        if ((double)Parameters["CloudImageONorOFF"] > 5.0)
        {
            p.Pulse(
                patternStartBeforeQ,
                cameraStart,
                (int)Parameters["CameraTriggerDuration"],
                "cameraTrigger"
            );
        }


        // Background Imaging
        if ((double)Parameters["BackgroundImageONorOFF"] > 5.0)
        {
            p.Pulse(
                patternStartBeforeQ,
                bgCameraStart - motLoadingStop,
                (int)Parameters["CameraTriggerDuration"] + 1000,
                "V00R0AOM"
            );
            p.Pulse(
                patternStartBeforeQ,
                bgCameraStart - motLoadingStop,
                (int)Parameters["CameraTriggerDuration"] + 1000,
                "V00R1plusAOM"
            );
            p.Pulse(
                patternStartBeforeQ,
                bgCameraStart,
                (int)Parameters["CameraTriggerDuration"],
                "cameraTrigger"
            );
        }

        // Common and finishing steps
        p.Pulse(
            patternStartBeforeQ,
            slowingChirpStop,
            30000,
            "greenShutter"
        );

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
        p.AddChannel("V00R0EOMVCOAmp");
        p.AddChannel("V00B2AOMAmp");
        p.AddChannel("motCoils");
        p.AddChannel("ShimCoilX");
        p.AddChannel("ShimCoilY");
        p.AddChannel("ShimCoilZ");
        p.AddChannel("CavityRamp");

        int slowingChirpStart = (int)Parameters["BXAOMFreeFlightDuration"] + (int)Parameters["BXAOMPostBunchingDuration"];
        int slowingChirpStop = slowingChirpStart + (int)Parameters["BXAOMChirpDuration"];

        int motLoadingStop = (int)Parameters["MOTLoadingDuration"];
        int motRetainingStop = motLoadingStop + (int)Parameters["MOTRetainingDuration"];
        int motCompressionStop = motRetainingStop + (int)Parameters["MOTCompressionRampDuration"] + (int)Parameters["MOTCompressionDuration"];
        int motCameraStart = motLoadingStop - (int)Parameters["CameraExposureDelay"] + (int)Parameters["MOTCameraTriggerStartDelay"];
        int lambdaMolassesRampStart = motCompressionStop + (int)Parameters["MOTFieldDecayDuration"];
        int lambdaMolassesStart = lambdaMolassesRampStart + (int)Parameters["LambdaMolassesRampDuration"];
        int lambdaMolassesStop = lambdaMolassesStart + (int)Parameters["LambdaMolassesHoldDuration"];
        int blueMOTFirstRampStart = lambdaMolassesStop + (int)Parameters["BlueMOTFirstRampStartDelay"];
        int blueMOTSecondRampStart = blueMOTFirstRampStart + (int)Parameters["BlueMOTFirstRampDuration"];
        int blueMOTHoldStart = blueMOTSecondRampStart + (int)Parameters["BlueMOTSecondRampDuration"];
        int blueMOTStop = blueMOTHoldStart + (int)Parameters["BlueMOTHoldDuration"];
        int cameraStart = lambdaMolassesStop - (int)Parameters["CameraExposureDelay"] + (int)Parameters["CameraTriggerStartDelay"];
        int cameraStop = cameraStart + (int)Parameters["CameraTriggerDuration"];
        int bgCameraStart = (int)Parameters["BackgroundImageTime"] - (int)Parameters["CameraExposureDelay"];


        if ((double)Parameters["SlowingONorOFF"] > 5.0)
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
        }

        if ((double)Parameters["MOTONorOFF"] > 5.0)
        {
            p.AddAnalogValue(
                "motCoils",
                (int)Parameters["MOTCoilsStartTime"],
                (double)Parameters["MOTLoadingFieldValue"]
            );
            p.AddAnalogValue(
                "ShimCoilX",
                0,
                (double)Parameters["XShimCoilsMOTValue"]
            );
            p.AddAnalogValue(
                "ShimCoilY",
                0,
                (double)Parameters["YShimCoilsMOTValue"]
            );
            p.AddAnalogValue(
                "ShimCoilZ",
                0,
                (double)Parameters["ZShimCoilsMOTValue"]
            );
            p.AddAnalogValue(
                "V00R0AOMVCOFreq",
                0,
                (double)Parameters["V00R0AOMVCOFreqMOTLoading"]
            );
            p.AddAnalogValue(
                "V00R0AOMVCOAmp",
                (int)Parameters["BXAOMFreeFlightDuration"],
                (double)Parameters["V00R0AOMVCOAmpMOTLoading"]
            );
            p.AddAnalogValue(
               "V00R1plusAOMAmp",
               (int)Parameters["BXAOMFreeFlightDuration"],
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
               "V00R0EOMVCOAmp",
               0,
               (double)Parameters["V00R0EOMVCOAmpMOTLoading"]
            );
        }

        if ((double)Parameters["CMOTONorOFF"] > 5.0)
        {
            p.AddLinearRamp(
                "motCoils",
                motRetainingStop,
                (int)Parameters["MOTCompressionRampDuration"],
                (double)Parameters["MOTCompressionFieldValue"]
            );
            p.AddAnalogValue(
                "V00R1plusAOMAmp",
                motRetainingStop,
                (double)Parameters["V00R1plusAOMAmpMOTCompression"]
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
                "motCoils",
                motCompressionStop,
                (double)Parameters["MOTCoilsOffValue"]
            );
        }

        if ((double)Parameters["MolassesONorOFF"] > 5.0)
        {
            p.AddAnalogValue(
               "V00B2AOMAmp",
               0,
               (double)Parameters["V00B2AOMAmpMolassesStart"]
            );
            p.AddLinearRamp(
                "V00B2AOMAmp",
                lambdaMolassesStart,
                (int)Parameters["LambdaMolassesRampDuration"],
                (double)Parameters["V00B2AOMAmpMolassesEnd"]
            );
            p.AddAnalogValue(
               "V00B2AOMAmp",
               cameraStop,
               1.0
            );
            p.AddAnalogValue(
                "ShimCoilX",
                motCompressionStop,
                (double)Parameters["XShimCoilsLambdaMolassesValue"]
            );
            p.AddAnalogValue(
                "ShimCoilY",
                motCompressionStop,
                (double)Parameters["YShimCoilsLambdaMolassesValue"]
            );
            p.AddAnalogValue(
                "ShimCoilZ",
                motCompressionStop,
                (double)Parameters["ZShimCoilsLambdaMolassesValue"]
            );
        }

        if ((double)Parameters["BlueMOTONorOFF"] > 5.0)
        { 
        }


        // Common and finishing steps
        p.AddAnalogValue(
            "motCoils",
            cameraStop,
            (double)Parameters["MOTCoilsOffValue"]
        );
        p.AddAnalogValue(
            "V00R0AOMVCOAmp",
            30000,
            (double)Parameters["V00R0AOMVCOAmpMax"]
        );
        p.AddAnalogValue(
           "V00R1plusAOMAmp",
           30000,
           (double)Parameters["V00R1plusAOMAmpMax"]
        );
        p.AddAnalogValue(
            "CavityRamp",
            0,
            0.4
        );
        p.AddLinearRamp(
            "CavityRamp",
            motCompressionStop,
            150,
            1.5
        );
        p.AddAnalogValue(
            "CavityRamp",
            25000,
            0.0
        );
        p.AddAnalogValue(
            "ShimCoilX",
            cameraStop,
            (double)Parameters["XShimCoilsOffValue"]
        );
        p.AddAnalogValue(
            "ShimCoilY",
            cameraStop,
            (double)Parameters["YShimCoilsOffValue"]
        );
        p.AddAnalogValue(
            "ShimCoilZ",
            cameraStop,
            (double)Parameters["ZShimCoilsOffValue"]
        );

        return p;
    }

    public override AnalogStaticBuilder GetAnalogStatic()
    {
        AnalogStaticBuilder p = new AnalogStaticBuilder((int)Parameters["PatternLength"]);

        return p;
    }
}
