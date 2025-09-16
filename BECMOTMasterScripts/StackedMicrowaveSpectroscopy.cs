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
        Parameters["TCLBlockStart"] = 5000;
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
        Parameters["MOTLoadingDuration"] = 4000;
        Parameters["V00R0AOMVCOFreqMOTLoading"] = 3.1;
        Parameters["V00R0AOMVCOAmpMOTLoading"] = 0.32;
        Parameters["V00R1plusAOMAmpMOTLoading"] = 0.72;
        Parameters["V00R0EOMAmpMOT"] = 0.53;
        Parameters["MOTCoilsStartTime"] = 0;
        Parameters["MOTLoadingFieldValue"] = 3.0;
        Parameters["XShimCoilsMOTValue"] = -0.361;
        Parameters["YShimCoilsMOTValue"] = -0.07;
        Parameters["ZShimCoilsMOTValue"] = 0.393;

        // Compressed MOT
        Parameters["CMOTONorOFF"] = 10.0;
        Parameters["MOTCompressionRampDuration"] = 600;
        Parameters["MOTCompressionDuration"] = 400;
        Parameters["MOTFieldDecayDuration"] = 150;
        Parameters["V00R0AOMVCOFreqMOTCompression"] = 3.1;
        Parameters["V00R0AOMVCOAmpMOTCompression"] = 0.42;
        Parameters["V00R1plusAOMAmpMOTCompression"] = 0.63;
        Parameters["MOTCompressionFieldValue"] = 6.0;
        Parameters["XShimCoilsCMOTValue"] = -0.361;
        Parameters["YShimCoilsCMOTValue"] = -0.07;
        Parameters["ZShimCoilsCMOTValue"] = 0.393;

        // Molasses
        Parameters["MolassesONorOFF"] = 10.0;
        Parameters["LambdaMolassesHoldDuration"] = 1000;
        Parameters["LambdaMolassesRampDuration"] = 1;
        Parameters["V00B2AOMAmpMolassesStart"] = 1.0; // can't be used here
        Parameters["V00B2AOMAmpMolassesEnd"] = 1.0; // can't be used here
        Parameters["XShimCoilsLambdaMolassesValue"] = -0.361;
        Parameters["YShimCoilsLambdaMolassesValue"] = -0.07;
        Parameters["ZShimCoilsLambdaMolassesValue"] = 0.393;
        Parameters["deltaDetuning"] = 0.0; //dummy

        // Optical Pumping and Microwaves
        Parameters["OpticalPumpingONorOFF"] = 10.0;
        Parameters["MicrowavesONorOFF"] = 10.0;
        Parameters["OpticalPumpingDuration"] = 10;
        Parameters["MicrowavesOnStartDelay"] = 0;
        Parameters["MicrowavesOnDuration"] = 5;
        Parameters["MicrowavesTotalDuration"] = 5;
        Parameters["V00R0EOMAmpOpticalPumping"] = 1.0;
        Parameters["V00R0AOMVCOFreqOpticalPumping"] = 3.1;
        Parameters["V00R0AOMVCOAmpOpticalPumping"] = 0.49;
        Parameters["V00R1plusAOMAmpOpticalPumping"] = 0.61;
        Parameters["OpticalPumpingPower"] = 0.0; // dummy
        Parameters["MicrowaveFrequency"] = 10000.0; // dummy
        Parameters["MicrowaveAmplitude"] = 10.0; // dummy

        // Recapture
        Parameters["MOTRecaptureONorOFF"] = 10.0;
        Parameters["MOTRecaptureStartDelay"] = 0;
        Parameters["MOTRecaptureDuration"] = 200;
        Parameters["V00R0AOMVCOFreqMOTRecapture"] = 3.1;
        Parameters["V00R0AOMVCOAmpMOTRecapture"] = 0.32;
        Parameters["V00R1plusAOMAmpMOTRecapture"] = 0.72;

        // Imaging
        Parameters["MOTImageONorOFF"] = 10.0;
        Parameters["CloudImageONorOFF"] = 10.0;
        Parameters["BackgroundImageONorOFF"] = 10.0;
        Parameters["CameraTriggerDuration"] = 1000;
        Parameters["CameraExposureDelay"] = 39; // EM CCD mode speed 2
        Parameters["MOTImageTime"] = 3000;
        Parameters["BackgroundImageTime"] = 25000;
        Parameters["V00R0AOMVCOFreqImaging"] = 3.1;
        Parameters["V00R0AOMVCOAmpImaging"] = 0.32;
        Parameters["V00R1plusAOMAmpImaging"] = 0.72;

        // Common and Finishing
        Parameters["ResetTime"] = 40000;
        Parameters["V00R0AOMVCOAmpMax"] = 0.30;
        Parameters["V00R1plusAOMAmpMax"] = 0.8;
        Parameters["MOTCoilsOffValue"] = 0.0;
        Parameters["XShimCoilsOffValue"] = -0.361;
        Parameters["YShimCoilsOffValue"] = -0.07;
        Parameters["ZShimCoilsOffValue"] = 0.393;

    }

    public override PatternBuilder32 GetDigitalPattern()
    {
        PatternBuilder32 p = new PatternBuilder32();
        int patternStartBeforeQ = (int)Parameters["TCLBlockStart"];
        int slowingAOMStart = (int)Parameters["BXAOMFreeFlightDuration"];
        int slowingChirpStart = slowingAOMStart + (int)Parameters["BXAOMPostBunchingDuration"];
        int slowingChirpStop = slowingChirpStart + (int)Parameters["BXAOMChirpDuration"];

        int motLoadingStop = (int)Parameters["MOTLoadingDuration"];
        int motCompressionStop = motLoadingStop + (int)Parameters["MOTCompressionRampDuration"] + (int)Parameters["MOTCompressionDuration"];
        int lambdaMolassesStart = motCompressionStop + (int)Parameters["MOTFieldDecayDuration"];
        int lambdaMolassesRampStart = lambdaMolassesStart + (int)Parameters["LambdaMolassesHoldDuration"];
        int lambdaMolassesStop = lambdaMolassesRampStart + (int)Parameters["LambdaMolassesRampDuration"];
        int opticalPumpingStop = lambdaMolassesStop + (int)Parameters["OpticalPumpingDuration"];
        int microwavesStop = opticalPumpingStop + (int)Parameters["MicrowavesTotalDuration"];
        int motRecaptureStart = microwavesStop + (int)Parameters["MOTRecaptureStartDelay"];
        int motRecaptureStop = motRecaptureStart + (int)Parameters["MOTRecaptureDuration"];
        int cameraStart = motRecaptureStop - (int)Parameters["CameraExposureDelay"];
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
                (int)Parameters["MOTImageTime"] - (int)Parameters["CameraExposureDelay"],
                (int)Parameters["CameraTriggerDuration"],
                "cameraTrigger"
            );
        }

        // Lambda Molasses
        if ((double)Parameters["MolassesONorOFF"] > 5.0)
        {
            p.Pulse(
                patternStartBeforeQ,
                lambdaMolassesStart,
                lambdaMolassesStop - lambdaMolassesStart,
                "V00B2AOM"
            );
            p.Pulse(
                patternStartBeforeQ,
                lambdaMolassesStart,
                lambdaMolassesStop - lambdaMolassesStart,
                "V00B1minusAOM"
            );
        }
        

        // Optical Pumping
        if ((double)Parameters["OpticalPumpingONorOFF"] > 5.0)
        {
            p.Pulse(
                patternStartBeforeQ,
                lambdaMolassesStop,
                (int)Parameters["OpticalPumpingDuration"],
                "V00R0AOM"
            );
            p.Pulse(
                patternStartBeforeQ,
                lambdaMolassesStop,
                (int)Parameters["OpticalPumpingDuration"],
                "V00R1plusAOM"
            );
        }

        // Microwaves
        if ((double)Parameters["MicrowavesONorOFF"] > 5.0)
        {
            p.Pulse(
                patternStartBeforeQ,
                opticalPumpingStop + (int)Parameters["MicrowavesOnStartDelay"],
                (int)Parameters["MicrowavesOnDuration"],
                "MicrowaveSwitch"
            );
        }

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
            (int)Parameters["BackgroundImageTime"],
            (int)Parameters["CameraTriggerDuration"],
            "V00R0AOM"
        );
        p.Pulse(
            patternStartBeforeQ,
            (int)Parameters["BackgroundImageTime"],
            (int)Parameters["CameraTriggerDuration"],
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

        // Common and finishing steps
        p.Pulse(
            patternStartBeforeQ,
            slowingChirpStop,
            cameraStop - slowingChirpStop,
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
        int motCompressionStop = motLoadingStop + (int)Parameters["MOTCompressionRampDuration"] + (int)Parameters["MOTCompressionDuration"];
        int lambdaMolassesStart = motCompressionStop + (int)Parameters["MOTFieldDecayDuration"];
        int lambdaMolassesRampStart = lambdaMolassesStart + (int)Parameters["LambdaMolassesHoldDuration"];
        int lambdaMolassesStop = lambdaMolassesRampStart + (int)Parameters["LambdaMolassesRampDuration"];
        int opticalPumpingStop = lambdaMolassesStop + (int)Parameters["OpticalPumpingDuration"];
        int microwavesStop = opticalPumpingStop + (int)Parameters["MicrowavesTotalDuration"];
        int motRecaptureStart = microwavesStop + (int)Parameters["MOTRecaptureStartDelay"];
        int motRecaptureStop = motRecaptureStart + (int)Parameters["MOTRecaptureDuration"];
        int cameraStart = motRecaptureStop - (int)Parameters["CameraExposureDelay"];
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
                slowingChirpStop,
                (double)Parameters["XShimCoilsMOTValue"]
            );
            p.AddAnalogValue(
                "ShimCoilY",
                slowingChirpStop,
                (double)Parameters["YShimCoilsMOTValue"]
            );
            p.AddAnalogValue(
                "ShimCoilZ",
                slowingChirpStop,
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
            // EOM MOT sidebands
            p.AddAnalogValue(
               "V00B2AOMAmp", 
               0,
               (double)Parameters["V00R0EOMAmpMOT"]
            );
        }

        if ((double)Parameters["CMOTONorOFF"] > 5.0)
        {
            p.AddLinearRamp(
                "motCoils",
                motLoadingStop,
                (int)Parameters["MOTCompressionRampDuration"],
                (double)Parameters["MOTCompressionFieldValue"]
            );
            p.AddLinearRamp(
                "V00R0AOMVCOFreq",
                motLoadingStop,
                (int)Parameters["MOTCompressionRampDuration"],
                (double)Parameters["V00R0AOMVCOFreqMOTCompression"]
            );
            p.AddLinearRamp(
                "V00R0AOMVCOAmp",
                motLoadingStop,
                (int)Parameters["MOTCompressionRampDuration"],
                (double)Parameters["V00R0AOMVCOAmpMOTCompression"]
            );
            p.AddLinearRamp(
                "V00R1plusAOMAmp",
                motLoadingStop,
                (int)Parameters["MOTCompressionRampDuration"],
                (double)Parameters["V00R1plusAOMAmpMOTCompression"]
            );
            p.AddLinearRamp(
                "ShimCoilX",
                motLoadingStop,
                (int)Parameters["MOTCompressionRampDuration"],
                (double)Parameters["XShimCoilsCMOTValue"]
            );
            p.AddLinearRamp(
                "ShimCoilY",
                motLoadingStop,
                (int)Parameters["MOTCompressionRampDuration"],
                (double)Parameters["YShimCoilsCMOTValue"]
            );
            p.AddLinearRamp(
                "ShimCoilZ",
                motLoadingStop,
                (int)Parameters["MOTCompressionRampDuration"],
                (double)Parameters["ZShimCoilsCMOTValue"]
            );
            /*p.AddAnalogValue(
                "motCoils",
                motCompressionStop,
                (double)Parameters["MOTCoilsOffValue"]
            );*/
            p.AddAnalogValue(
                "motCoils",
                motCompressionStop,
                0.0
            );
            /*p.AddAnalogValue(
                "motCoils",
                motCompressionStop + 100,
                -6.0
            );
            p.AddAnalogValue(
                "motCoils",
                motCompressionStop + 500,
                0.0
            );*/
            /*p.AddLinearRamp(
                "motCoils",
                motCompressionStop + 220,
                220,
                -0.5
            );
            p.AddLinearRamp(
                "motCoils",
                motCompressionStop + 440,
                200,
                -0.0
            );*/
            /*p.AddAnalogValue(
                "motCoils",
                motCompressionStop,
                -2.0
            );*/
            /*p.AddAnalogValue(
                "motCoils",
                motCompressionStop + 300,
                0.0
            );*/
        }

        if ((double)Parameters["MolassesONorOFF"] > 5.0)
        {
            p.AddLinearRamp(
                "ShimCoilX",
                motCompressionStop,
                (int)Parameters["MOTFieldDecayDuration"],
                (double)Parameters["XShimCoilsLambdaMolassesValue"]
            );
            p.AddLinearRamp(
                "ShimCoilY",
                motCompressionStop,
                (int)Parameters["MOTFieldDecayDuration"],
                (double)Parameters["YShimCoilsLambdaMolassesValue"]
            );
            p.AddLinearRamp(
                "ShimCoilZ",
                motCompressionStop,
                (int)Parameters["MOTFieldDecayDuration"],
                (double)Parameters["ZShimCoilsLambdaMolassesValue"]
            );
            /*p.AddAnalogValue( // channel used for EOM here
               "V00B2AOMAmp",
               lambdaMolassesStart,
               (double)Parameters["V00B2AOMAmpMolassesStart"]
            );
            p.AddLinearRamp(
                "V00B2AOMAmp",
                lambdaMolassesRampStart,
                (int)Parameters["LambdaMolassesRampDuration"],
                (double)Parameters["V00B2AOMAmpMolassesEnd"]
            );*/
        }

        if ((double)Parameters["OpticalPumpingONorOFF"] > 5.0)
        {
            p.AddAnalogValue(
                "V00R0AOMVCOAmp",
                motCompressionStop,
                (double)Parameters["V00R0AOMVCOAmpOpticalPumping"]
            );
            p.AddAnalogValue(
                "V00R0AOMVCOFreq",
                motCompressionStop,
                (double)Parameters["V00R0AOMVCOFreqOpticalPumping"]
            );
            p.AddAnalogValue(
               "V00R1plusAOMAmp",
               motCompressionStop,
               (double)Parameters["V00R1plusAOMAmpOpticalPumping"]
            );
            // EOM sidebands
            p.AddAnalogValue(
               "V00B2AOMAmp",
               motCompressionStop,
               (double)Parameters["V00R0EOMAmpOpticalPumping"]
            );
        }


        // MOT recapture
        if ((double)Parameters["MOTRecaptureONorOFF"] > 5.0)
        {
            p.AddAnalogValue(
                "ShimCoilX",
                motRecaptureStart,
                (double)Parameters["XShimCoilsMOTValue"]
            );
            p.AddAnalogValue(
                "ShimCoilY",
                motRecaptureStart,
                (double)Parameters["YShimCoilsMOTValue"]
            );
            p.AddAnalogValue(
                "ShimCoilZ",
                motRecaptureStart,
                (double)Parameters["ZShimCoilsMOTValue"]
            );
            p.AddAnalogValue(
                "V00R0AOMVCOAmp",
                opticalPumpingStop,
                (double)Parameters["V00R0AOMVCOAmpMOTRecapture"]
            );
            p.AddAnalogValue(
                "V00R0AOMVCOFreq",
                opticalPumpingStop,
                (double)Parameters["V00R0AOMVCOFreqMOTRecapture"]
            );
            p.AddAnalogValue(
                "V00R1plusAOMAmp",
                opticalPumpingStop,
                (double)Parameters["V00R1plusAOMAmpMOTRecapture"]
            );
            p.AddAnalogValue(
                "motCoils",
                motRecaptureStart,
                (double)Parameters["MOTLoadingFieldValue"]
            );
            // EOM MOT sidebands
            p.AddAnalogValue(
               "V00B2AOMAmp",
               opticalPumpingStop + 1,
               (double)Parameters["V00R0EOMAmpMOT"]
            );
        }

        if ((double)Parameters["CloudImageONorOFF"] > 5.0)
        {
            p.AddAnalogValue(
                "V00R1plusAOMAmp",
                cameraStart,
                (double)Parameters["V00R1plusAOMAmpImaging"]
            );
            p.AddAnalogValue(
                "V00R0AOMVCOFreq",
                cameraStart,
                (double)Parameters["V00R0AOMVCOFreqImaging"]
            );
            p.AddAnalogValue(
                "V00R0AOMVCOAmp",
                cameraStart,
                (double)Parameters["V00R0AOMVCOAmpImaging"]
            );
        }


        // Common and finishing steps
        p.AddAnalogValue(
            "V00B2AOMAmp",
            cameraStop,
            (double)Parameters["V00R0EOMAmpMOT"]
        );
        p.AddAnalogValue(
            "V00R0AOMVCOAmp",
            (int)Parameters["ResetTime"],
            (double)Parameters["V00R0AOMVCOAmpMax"]
        );
        p.AddAnalogValue(
           "V00R1plusAOMAmp",
           (int)Parameters["ResetTime"],
           (double)Parameters["V00R1plusAOMAmpMax"]
        );
        p.AddAnalogValue(
            "ShimCoilX",
            (int)Parameters["ResetTime"],
            (double)Parameters["XShimCoilsOffValue"]
        );
        p.AddAnalogValue(
            "ShimCoilY",
            (int)Parameters["ResetTime"],
            (double)Parameters["YShimCoilsOffValue"]
        );
        p.AddAnalogValue(
            "ShimCoilZ",
            (int)Parameters["ResetTime"],
            (double)Parameters["ZShimCoilsOffValue"]
        );
        p.AddAnalogValue(
            "motCoils",
            cameraStop + 100,
            (double)Parameters["MOTCoilsOffValue"]
        );


        p.AddAnalogValue(
            "CavityRamp",
            0,
            -2.0
        );
        p.AddLinearRamp(
            "CavityRamp",
            lambdaMolassesStop,
            150,
            0.0
        );
        p.AddAnalogValue(
            "CavityRamp",
            25000,
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
