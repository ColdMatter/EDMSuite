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
        Parameters["MOTLoadingDuration"] = 3500;
        Parameters["MOTRetainingRampDuration"] = 500;
        Parameters["MOTRetainingDuration"] = 0;
        Parameters["V00R0AOMVCOFreqMOTLoading"] = 3.1;
        Parameters["V00R0AOMVCOFreqMOTRetaining"] = 3.1;
        Parameters["V00R0AOMVCOAmpMOTLoading"] = 0.32;
        Parameters["V00R0EOMVCOAmpMOTLoading"] = -1.1;
        Parameters["V00R0AOMVCOAmpMOTRetaining"] = 0.42;
        Parameters["V00R1plusAOMAmpMOTLoading"] = 0.72;
        Parameters["V00R1plusAOMAmpMOTRetaining"] = 0.63;
        Parameters["MOTCoilsStartTime"] = 0;
        Parameters["MOTLoadingFieldValue"] = 3.0;
        Parameters["XShimCoilsMOTValue"] = 0.0;
        Parameters["YShimCoilsMOTValue"] = 0.0;
        Parameters["ZShimCoilsMOTValue"] = 0.0;

        // Compressed MOT
        Parameters["CMOTONorOFF"] = 10.0;
        Parameters["MOTCompressionRampDuration"] = 100;
        Parameters["MOTCompressionDuration"] = 500;
        Parameters["MOTFieldDecayDuration"] = 150;
        Parameters["V00R0AOMVCOFreqMOTCompression"] = 3.1;
        Parameters["V00R0AOMVCOAmpMOTCompression"] = 0.42;
        Parameters["V00R1plusAOMAmpMOTCompression"] = 0.63;
        Parameters["MOTCompressionFieldValue"] = 6.0;

        // Molasses
        Parameters["MolassesONorOFF"] = 10.0;
        Parameters["LambdaMolassesRampDuration"] = 0;
        Parameters["LambdaMolassesHoldDuration"] = 10;
        Parameters["V00B2AOMAmpMolassesStart"] = 1.0;
        Parameters["V00B2AOMAmpMolassesEnd"] = 1.0;
        Parameters["XShimCoilsLambdaMolassesValue"] = 0.0;
        Parameters["YShimCoilsLambdaMolassesValue"] = 0.0;
        Parameters["ZShimCoilsLambdaMolassesValue"] = 0.0;

        // Optical Pumping
        Parameters["OpticalPumpingONorOFF"] = 1.0;
        Parameters["OpticalPumpingStartDelay"] = 0;
        Parameters["OpticalPumpingDuration"] = 0;
        Parameters["V00R0AOMVCOAmpOpticalPumping"] = 0.55;    //0.55
        Parameters["V00R1plusAOMAmpOpticalPumping"] = 0.59;   // 0.59
        Parameters["V00R0EOMVCOAmpOpticalPumping"] = -2.2;
        Parameters["V00R0AOMVCOFreqOpticalPumping"] = 3.3;

        // Microwave pumping
        Parameters["MicrowaveONorOFF"] = 1.0;
        Parameters["MicrowaveStartDelay"] = 7;
        Parameters["MicrowavesOnDuration"] = 7;

        // Magnetic trap
        Parameters["QuadrupoleTrapONorOFF"] = 1.0;
        Parameters["QuadrupoleTrapStartDelay"] = 1;
        Parameters["QuadrupoleTrapOnDuration"] = 1;
        Parameters["QuadrupoleTrapFieldValue"] = 0.0;

        // MOT Recaptured
        Parameters["MOTRecaptureONorOFF"] = 10.0;
        Parameters["MOTRecaptureStartDelay"] = 0;
        Parameters["MOTRecaptureOnDuration"] = 1000;

        // Imaging
        Parameters["MOTImageONorOFF"] = 10.0;
        Parameters["CloudImageONorOFF"] = 10.0;
        Parameters["BackgroundImageONorOFF"] = 10.0;
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
        int opticalPumpingStart = lambdaMolassesStop + (int)Parameters["OpticalPumpingStartDelay"];
        int opticalPumpingStop = opticalPumpingStart + (int)Parameters["OpticalPumpingDuration"];
        int microwaveStart = opticalPumpingStop + (int)Parameters["MicrowaveStartDelay"];
        int microwaveStop = microwaveStart + (int)Parameters["MicrowavesOnDuration"];
        int quadrupoleTrapStart = microwaveStop + (int)Parameters["QuadrupoleTrapStartDelay"];
        int quadrupoleTrapStop = quadrupoleTrapStart + (int)Parameters["QuadrupoleTrapOnDuration"];
        int motRecaptureStart = quadrupoleTrapStop + (int)Parameters["MOTRecaptureStartDelay"];
        int motRecaptureStop = motRecaptureStart + (int)Parameters["MOTRecaptureOnDuration"];
        int cameraStart = motRecaptureStop - (int)Parameters["CameraExposureDelay"] + (int)Parameters["CameraTriggerStartDelay"];
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
            

        // Lambda Molasses + Optical pumping
        p.Pulse(
            patternStartBeforeQ,
            lambdaMolassesRampStart,
            lambdaMolassesStop - lambdaMolassesRampStart,
            "V00B2AOM"
        );
        p.Pulse(
            patternStartBeforeQ,
            lambdaMolassesRampStart,
            lambdaMolassesStop - lambdaMolassesRampStart,
            "V00B1minusAOM"
        );

        // MOT recapture + Imaging
        p.Pulse(
            patternStartBeforeQ,
            motRecaptureStart,
            cameraStop - motRecaptureStart,
            "V00R0AOM"
        );

        p.Pulse(
            patternStartBeforeQ,
            motRecaptureStart,
            cameraStop - motRecaptureStart,
            "V00R1plusAOM"
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
        p.Pulse(
            patternStartBeforeQ,
            bgCameraStart- motLoadingStop,
            (int)Parameters["CameraTriggerDuration"] + 1000,
            "V00R0AOM"
        );
        p.Pulse(
            patternStartBeforeQ,
            bgCameraStart - motLoadingStop,
            (int)Parameters["CameraTriggerDuration"] + 1000,
            "V00R1plusAOM"
        );
        if ((double)Parameters["BackgroundImageONorOFF"] > 5.0)
        {
            p.Pulse(
                patternStartBeforeQ,
                bgCameraStart,
                (int)Parameters["CameraTriggerDuration"],
                "cameraTrigger"
            );
        }
 
        // Microwave
        if ((double)Parameters["MicrowaveONorOFF"] > 5.0)
        {
            p.Pulse(
                patternStartBeforeQ,
                microwaveStart,
                (int)Parameters["MicrowavesOnDuration"],
                "MicrowaveSwitch"
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
        int opticalPumpingStart = lambdaMolassesStop + (int)Parameters["OpticalPumpingStartDelay"];
        int opticalPumpingStop = opticalPumpingStart + (int)Parameters["OpticalPumpingDuration"];
        int microwaveStart = opticalPumpingStop + (int)Parameters["MicrowaveStartDelay"];
        int microwaveStop = microwaveStart + (int)Parameters["MicrowavesOnDuration"];
        int quadrupoleTrapStart = microwaveStop + (int)Parameters["QuadrupoleTrapStartDelay"];
        int quadrupoleTrapStop = quadrupoleTrapStart + (int)Parameters["QuadrupoleTrapOnDuration"];
        int motRecaptureStart = quadrupoleTrapStop + (int)Parameters["MOTRecaptureStartDelay"];
        int motRecaptureStop = motRecaptureStart + (int)Parameters["MOTRecaptureOnDuration"];
        int cameraStart = motRecaptureStop - (int)Parameters["CameraExposureDelay"] + (int)Parameters["CameraTriggerStartDelay"];
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
        
        if ((double)Parameters["OpticalPumpingONorOFF"] > 5.0)
        {
            p.AddAnalogValue(
               "V00R0EOMVCOAmp",
               motCompressionStop,
               (double)Parameters["V00R0EOMVCOAmpOpticalPumping"]
            );
            p.AddAnalogValue(
                "V00R0AOMVCOAmp",
                motCompressionStop,
                (double)Parameters["V00R0AOMVCOAmpOpticalPumping"]
            );
            p.AddAnalogValue(
               "V00R1plusAOMAmp",
               motCompressionStop,
               (double)Parameters["V00R1plusAOMAmpOpticalPumping"]
            );
            p.AddAnalogValue(
                "V00R0AOMVCOFreq",
                motCompressionStop,
                (double)Parameters["V00R0AOMVCOFreqOpticalPumping"]
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

        if ((double)Parameters["MicrowaveONorOFF"] > 5.0)
        {
        }

        if ((double)Parameters["QuadrupoleTrapONorOFF"] > 5.0)
        {
            p.AddAnalogValue(
                "motCoils",
                quadrupoleTrapStart,
                (double)Parameters["QuadrupoleTrapFieldValue"]
            );
            p.AddAnalogValue(
                "motCoils",
                quadrupoleTrapStop,
                (double)Parameters["MOTCoilsOffValue"]
            );
        }


        if ((double)Parameters["MOTRecaptureONorOFF"] > 5.0)
        {
            p.AddAnalogValue(
                "motCoils",
                motRecaptureStart,
                (double)Parameters["MOTLoadingFieldValue"]
            );
            p.AddAnalogValue(
                "V00R0AOMVCOAmp",
                motRecaptureStart,
                (double)Parameters["V00R0AOMVCOAmpMOTLoading"]
            );
            p.AddAnalogValue(
               "V00R1plusAOMAmp",
               motRecaptureStart,
               (double)Parameters["V00R1plusAOMAmpMOTLoading"]
            );
            p.AddAnalogValue(
               "V00R0EOMVCOAmp",
               motRecaptureStart,
               (double)Parameters["V00R0EOMVCOAmpMOTLoading"]
            );
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
