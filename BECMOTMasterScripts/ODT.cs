using MOTMaster;
using System;
using System.Collections.Generic;

using DAQ.Pattern;
using DAQ.Analog;
using DAQ.Environment;


public class Patterns : MOTMasterScript
/*
 * This script is designed to be stacked with different phases of the 
 * experiment separately.
 * */
{
    public Patterns()
    {

        // Load global parameters
        string dataPath = (string)Environs.FileSystem.Paths["MOTMasterDataPath"];
        string element = (string)Environs.Hardware.GetInfo("Element");
        var io = new MMDataIOHelper(dataPath, element);
        string globalParameterPath = "C:\\ControlPrograms\\EDMSuite\\BECMOTMasterScripts\\globalParameters.txt";
        var globalParameters = io.LoadDictionary(globalParameterPath);
        Parameters = new Dictionary<string, object>(globalParameters);

        // ------------- TOGGLES -------------

        Parameters["yagONorOFF"] = 10.0;
        Parameters["SlowingONorOFF"] = 10.0;
        Parameters["SlowingShutterONorOFF"] = 10.0;
        Parameters["MOTONorOFF"] = 10.0;
        Parameters["CMOTONorOFF"] = 10.0;
        Parameters["MolassesONorOFF"] = 10.0;
        Parameters["BlueMOTONorOFF"] = 10.0;
        Parameters["4thSidebandONorOFF"] = 10.0;
        Parameters["ODTONorOFF"] = 10.0;
        Parameters["CloudImageONorOFF"] = 10.0;
        Parameters["BackgroundImageONorOFF"] = 10.0;

        // ------------- LOCAL PARAMETERS -------------

        Parameters["MOTCompressionRampDuration"] = 600;
        Parameters["MOTCompressionHoldDuration"] = 400;
        Parameters["LambdaMolassesHoldDuration"] = 200;

        /*Parameters["BlueMOTFirstRampDuration"] = 5000;
        Parameters["BlueMOTFirstFieldValue"] = -5.0;
        Parameters["BlueMOTSecondFieldValue"] = -5.0; // not used at the moment
        Parameters["BlueMOTHoldDuration"] = 100;*/

        // 4th sideband
        Parameters["V00F0AOMAmpBlueMOT"] = 0.74;

        Parameters["V00F2AOMAmpBlueMOT"] = 0.715;
        Parameters["V00F1plusAOMAmpBlueMOT"] = 0.615;
        Parameters["V00F1minusAOMAmpBlueMOT"] = 0.62;

        // ramp end values for blue MOT
        Parameters["V00F2AOMAmpBlueMOTEnd"] = 0.61;
        Parameters["V00F1plusAOMAmpBlueMOTEnd"] = 0.615;
        Parameters["V00F1minusAOMAmpBlueMOTEnd"] = 0.52;
        Parameters["V00F0AOMAmpBlueMOTEnd"] = 0.59;


        /*

            // 4th sideband
            Parameters["V00F0AOMAmpBlueMOT"] = 0.89;

            // ramp end values for blue MOT
            Parameters["V00F2AOMAmpBlueMOTEnd"] = 0.62;
            Parameters["V00F1plusAOMAmpBlueMOTEnd"] = 0.615;
            Parameters["V00F1minusAOMAmpBlueMOTEnd"] = 0.58;
            Parameters["V00F0AOMAmpBlueMOTEnd"] = 0.59;*/

        // ODT
        Parameters["ODTStartDelay"] = 1; //from end of molasses
        Parameters["ODTOnlyDuration"] = 15000; // 15000; // after blue MOT stop
        Parameters["ODTVVAControlVoltage"] = 5.3;


        // common and finishing
        Parameters["PatternLength"] = 70000;
        Parameters["TCLBlockStart"] = 5000;
        Parameters["ResetTime"] = 60000;

        // imaging
        Parameters["CameraTriggerStartDelay"] = 10000; // imaging time from blue MOT stop
        Parameters["CameraTriggerDuration"] = 1000;
        Parameters["BackgroundImageTime"] = 50000;

        // dummy
        Parameters["deltaAMHz"] = 0.0;
        Parameters["deltaBMHz"] = 0.0;
        Parameters["onePhotonDetuningMHz"] = 0.0;
    }

    public override PatternBuilder32 GetDigitalPattern()
    {
        PatternBuilder32 p = new PatternBuilder32();

        int patternStartBeforeQ = (int)Parameters["TCLBlockStart"];
        int slowingAOMStart = (int)Parameters["BXAOMFreeFlightDuration"];
        int slowingChirpStart = slowingAOMStart + (int)Parameters["BXAOMPostBunchingDuration"];
        int slowingChirpStop = slowingChirpStart + (int)Parameters["BXAOMChirpDuration"];

        int motLoadingStop = (int)Parameters["MOTLoadingDuration"];
        int motCompressionStop = motLoadingStop + (int)Parameters["MOTCompressionRampDuration"] + (int)Parameters["MOTCompressionHoldDuration"];
        int lambdaMolassesStart = motCompressionStop + (int)Parameters["MOTFieldDecayDuration"];
        int lambdaMolassesRampStart = lambdaMolassesStart + (int)Parameters["LambdaMolassesHoldDuration"];
        int lambdaMolassesStop = lambdaMolassesRampStart + (int)Parameters["LambdaMolassesRampDuration"];
        int blueMOTSecondRampStart = lambdaMolassesStop + (int)Parameters["BlueMOTFirstRampDuration"];
        int blueMOTHoldStart = blueMOTSecondRampStart + (int)Parameters["BlueMOTSecondRampDuration"];
        int blueMOTStop = blueMOTHoldStart + (int)Parameters["BlueMOTHoldDuration"];
        //int lambdaMolasses2Stop = blueMOTStop + (int)Parameters["LambdaMolasses2HoldDuration"];
        int odtStart = lambdaMolassesStop + (int)Parameters["ODTStartDelay"];
        int odtStop = blueMOTStop + (int)Parameters["ODTOnlyDuration"];
        int imagingLightStart = blueMOTStop + (int)Parameters["CameraTriggerStartDelay"];
        int imagingLightStop = imagingLightStart + (int)Parameters["CameraTriggerDuration"];
        int cameraStart = imagingLightStart - (int)Parameters["CameraExposureDelayCCDMode"];
        int cameraStop = cameraStart + (int)Parameters["CameraTriggerDuration"];
        int bgCameraStart = (int)Parameters["BackgroundImageTime"] - (int)Parameters["CameraExposureDelayCCDMode"];

        p.Pulse(patternStartBeforeQ, 0, 10, "analogPatternTrigger");
        p.EnforceTimeOrdering(false);

        p.Pulse(
            patternStartBeforeQ,
            0,
            25000,
            "blockTCL"
        );

        p.Pulse(
            patternStartBeforeQ,
            (int)Parameters["HeShutterStart"],
            (int)Parameters["HeShutterDuration"],
            "BXShutter"
        );


        if ((double)Parameters["yagONorOFF"] > 5.0)
        {
            p.Pulse(
                patternStartBeforeQ,
                -(int)Parameters["FlashToQ"],
                (int)Parameters["QSwitchPulseDuration"],
                "flash"
            );

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
                5000,
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
            p.Pulse(
                patternStartBeforeQ,
                slowingAOMStart,
                slowingChirpStop - slowingAOMStart,
                "BXAOM2"
            );
        }
        // currently using 2nd camera trigger for BX trigger (closed on high)
        if ((double)Parameters["SlowingShutterONorOFF"] > 5.0)
        {
            p.Pulse(
                patternStartBeforeQ,
                0,
                (int)Parameters["BackgroundImageTime"],
                "camera2Trigger"
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
            "V00R1plusAOMredMOT"
        );
        p.Pulse(
            patternStartBeforeQ,
            (int)Parameters["V00AOMONStartTime"],
            motCompressionStop,
            "V00B2AOMmolasses"
        );
        p.Pulse(
            patternStartBeforeQ,
            (int)Parameters["V00AOMONStartTime"],
            motCompressionStop,
            "V00B1minusAOM"
        );

        // Lambda molasses
        if ((double)Parameters["MolassesONorOFF"] > 5.0)
        {
            p.Pulse(
                patternStartBeforeQ,
                lambdaMolassesStart,
                lambdaMolassesStop - lambdaMolassesStart,
                "V00B2AOMmolasses"
            );
            p.Pulse(
                patternStartBeforeQ,
                lambdaMolassesStart,
                lambdaMolassesStop - lambdaMolassesStart,
                "DDSTTLP2"
            );
            p.Pulse(
                patternStartBeforeQ,
                lambdaMolassesStart,
                lambdaMolassesStop - lambdaMolassesStart,
                "V00B1minusAOM"
            );
            p.Pulse(
                patternStartBeforeQ,
                lambdaMolassesStart,
                lambdaMolassesStop - lambdaMolassesStart,
                "DDSTTLP3"
            );
        }

        // Blue MOT and imagning
        if ((double)Parameters["BlueMOTONorOFF"] > 5.0)
        {
            p.Pulse(
                patternStartBeforeQ,
                lambdaMolassesStop,
                blueMOTStop - lambdaMolassesStop,
                "V00B1minusAOM"
            );
            p.Pulse(
                patternStartBeforeQ,
                lambdaMolassesStop,
                blueMOTStop - lambdaMolassesStop,
                "DDSTTLP3"
            );
            p.Pulse(
                patternStartBeforeQ,
                lambdaMolassesStop,
                blueMOTStop - lambdaMolassesStop,
                "V00R1plusAOMredMOT"
            );
            p.Pulse(
                patternStartBeforeQ,
                lambdaMolassesStop,
                blueMOTStop - lambdaMolassesStop,
                "DDSTTLP1"
            );
            p.Pulse(
                patternStartBeforeQ,
                lambdaMolassesStop,
                blueMOTStop - lambdaMolassesStop,
                "V00B2AOMblueMOT"
            );
            if ((double)Parameters["4thSidebandONorOFF"] > 5.0)
            {
                p.Pulse(
                    patternStartBeforeQ,
                    lambdaMolassesStop,
                    blueMOTStop - lambdaMolassesStop,
                    "DDSTTLP0"
                );
                p.Pulse(
                    patternStartBeforeQ,
                    lambdaMolassesStop,
                    blueMOTStop - lambdaMolassesStop,
                    "V00R0AOM"
                );
            }
        }

        // ODT
        if ((double)Parameters["ODTONorOFF"] > 5.0)
        {
            p.Pulse(
                patternStartBeforeQ,
                odtStart,
                odtStop - odtStart,
                "DipoleTrapAOM"
            );
        }


        if ((double)Parameters["CloudImageONorOFF"] > 5.0)
        {
            p.Pulse(
                patternStartBeforeQ,
                cameraStart,
                (int)Parameters["CameraTriggerDuration"],
                "cameraTrigger"
            );
            p.Pulse(
                patternStartBeforeQ,
                imagingLightStart,
                (int)Parameters["CameraTriggerDuration"],
                "V00B2AOMmolasses"
            );
            p.Pulse(
                patternStartBeforeQ,
                imagingLightStart,
                (int)Parameters["CameraTriggerDuration"],
                "DDSTTLP2"
            );
            p.Pulse(
                patternStartBeforeQ,
                imagingLightStart,
                (int)Parameters["CameraTriggerDuration"],
                "V00B1minusAOM"
            );
            p.Pulse(
                patternStartBeforeQ,
                imagingLightStart,
                (int)Parameters["CameraTriggerDuration"],
                "DDSTTLP3"
            );
        }


        // Background Imaging
        p.Pulse(
            patternStartBeforeQ,
            (int)Parameters["BackgroundImageTime"],
            (int)Parameters["CameraTriggerDuration"],
            "V00B2AOMmolasses"
        );
        p.Pulse(
            patternStartBeforeQ,
            (int)Parameters["BackgroundImageTime"],
            (int)Parameters["CameraTriggerDuration"],
            "DDSTTLP2"
        );
        p.Pulse(
            patternStartBeforeQ,
            (int)Parameters["BackgroundImageTime"],
            (int)Parameters["CameraTriggerDuration"],
            "V00B1minusAOM"
        );
        p.Pulse(
            patternStartBeforeQ,
            (int)Parameters["BackgroundImageTime"],
            (int)Parameters["CameraTriggerDuration"],
            "DDSTTLP3"
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
        p.AddChannel("V00B1minusAOMAmp");
        p.AddChannel("V00R0EOMAmp");
        p.AddChannel("V00B2AOMAmp");
        p.AddChannel("motCoils");
        p.AddChannel("ShimCoilX");
        p.AddChannel("ShimCoilY");
        p.AddChannel("ShimCoilZ");
        p.AddChannel("CavityRamp");
        p.AddChannel("ODTVVAControl");

        int slowingChirpStart = (int)Parameters["BXAOMFreeFlightDuration"] + (int)Parameters["BXAOMPostBunchingDuration"];
        int slowingChirpStop = slowingChirpStart + (int)Parameters["BXAOMChirpDuration"];

        int motLoadingStop = (int)Parameters["MOTLoadingDuration"];
        int motCompressionStop = motLoadingStop + (int)Parameters["MOTCompressionRampDuration"] + (int)Parameters["MOTCompressionHoldDuration"];
        int lambdaMolassesStart = motCompressionStop + (int)Parameters["MOTFieldDecayDuration"];
        int lambdaMolassesRampStart = lambdaMolassesStart + (int)Parameters["LambdaMolassesHoldDuration"];
        int lambdaMolassesStop = lambdaMolassesRampStart + (int)Parameters["LambdaMolassesRampDuration"];
        int blueMOTSecondRampStart = lambdaMolassesStop + (int)Parameters["BlueMOTFirstRampDuration"];
        int blueMOTHoldStart = blueMOTSecondRampStart + (int)Parameters["BlueMOTSecondRampDuration"];
        int blueMOTStop = blueMOTHoldStart + (int)Parameters["BlueMOTHoldDuration"];
        //int lambdaMolasses2Stop = blueMOTStop + (int)Parameters["LambdaMolasses2HoldDuration"];
        int odtStart = lambdaMolassesStop + (int)Parameters["ODTStartDelay"];
        int odtStop = blueMOTStop + (int)Parameters["ODTOnlyDuration"];
        int imagingLightStart = blueMOTStop + (int)Parameters["CameraTriggerStartDelay"]; 
        int imagingLightStop = imagingLightStart + (int)Parameters["CameraTriggerDuration"];
        int cameraStart = imagingLightStart - (int)Parameters["CameraExposureDelayCCDMode"];
        int cameraStop = cameraStart + (int)Parameters["CameraTriggerDuration"];
        int bgCameraStart = (int)Parameters["BackgroundImageTime"] - (int)Parameters["CameraExposureDelayCCDMode"];

        if ((double)Parameters["SlowingONorOFF"] > 5.0)
        {
            p.AddLinearRamp(
                "BXChirp",
                slowingChirpStart,
                slowingChirpStop - slowingChirpStart,
                (double)Parameters["BXChirpMHzSpan"] / (double)Parameters["BXMHzPerVolt"]
            );
            p.AddLinearRamp(
                "BXChirp",
                slowingChirpStop + 5000,
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
            p.AddAnalogValue(
                "ShimCoilY",
                0,
                (double)Parameters["YShimCoilsSlowingValue"]
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
                (double)Parameters["V00F0AOMFreqMOTLoading"]
            );
            p.AddAnalogValue(
                "V00R0AOMVCOAmp",
                0,
                (double)Parameters["V00F0AOMAmpMOTLoading"]
            );
            p.AddAnalogValue(
               "V00R1plusAOMAmp",
               0,
               (double)Parameters["V00F1plusAOMAmpMOTLoading"]
            );
            p.AddAnalogValue(
               "V00B2AOMAmp",
               0,
               (double)Parameters["V00F2AOMAmpMOTLoading"]
            );
            p.AddAnalogValue(
               "V00B1minusAOMAmp",
               0,
               (double)Parameters["V00F1minusAOMAmpMOTLoading"]
            );

            p.AddAnalogValue(
               "V00R0EOMAmp",
               0,
               0.8
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
                (double)Parameters["V00F0AOMFreqMOTCompression"]
            );
            p.AddLinearRamp(
                "V00R0AOMVCOAmp",
                motLoadingStop,
                (int)Parameters["MOTCompressionRampDuration"],
                (double)Parameters["V00F0AOMAmpMOTCompression"]
            );
            p.AddLinearRamp(
                "V00R1plusAOMAmp",
                motLoadingStop,
                (int)Parameters["MOTCompressionRampDuration"],
                (double)Parameters["V00F1plusAOMAmpMOTCompression"]
            );
            p.AddLinearRamp(
                "V00B2AOMAmp",
                motLoadingStop,
                (int)Parameters["MOTCompressionRampDuration"],
                (double)Parameters["V00F2AOMAmpMOTCompression"]
            );
            p.AddLinearRamp(
                "V00B1minusAOMAmp",
                motLoadingStop,
                (int)Parameters["MOTCompressionRampDuration"],
                (double)Parameters["V00F1minusAOMAmpMOTCompression"]
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

        }
        p.AddAnalogValue(
            "motCoils",
            motCompressionStop,
            (double)Parameters["MOTCoilsOffValue"]
        );

        if ((double)Parameters["MolassesONorOFF"] > 5.0)
        {
            p.AddLinearRamp(
                "ShimCoilX",
                motCompressionStop + 550,
                100,
                (double)Parameters["XShimCoilsLambdaMolassesValue"]
            );
            p.AddLinearRamp(
                "ShimCoilY",
                motCompressionStop + 550,
                100,
                (double)Parameters["YShimCoilsLambdaMolassesValue"]
            );
            p.AddLinearRamp(
                "ShimCoilZ",
                motCompressionStop + 550,
                100,
                (double)Parameters["ZShimCoilsLambdaMolassesValue"]
            );
            p.AddAnalogValue(
                "V00B2AOMAmp",
                motCompressionStop,
                (double)Parameters["V00F2AOMAmpMolassesStart"]
            );
            /*p.AddLinearRamp(
                "V00B2AOMAmp",
                lambdaMolassesRampStart,
                (int)Parameters["LambdaMolassesRampDuration"],
                (double)Parameters["V00B2AOMAmpMolassesEnd"]
            );*/
            p.AddAnalogValue(
                "V00B1minusAOMAmp",
                motCompressionStop,
                (double)Parameters["V00F1minusAOMAmpMolassesStart"]
            );
            /*p.AddLinearRamp(
                "V00B2AOMAmp",
                lambdaMolassesRampStart,
                (int)Parameters["LambdaMolassesRampDuration"],
                (double)Parameters["V00B1minusAOMAmpMolassesEnd"]
            );*/
        }

        if ((double)Parameters["BlueMOTONorOFF"] > 5.0)
        {
            /*p.AddLinearRamp(
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
            );*/
            p.AddLinearRamp(
                "motCoils",
                lambdaMolassesStop,
                (int)Parameters["BlueMOTFirstRampDuration"],
                (double)Parameters["BlueMOTFirstFieldValue"]
            );
            p.AddLinearRamp(
                "motCoils",
                blueMOTSecondRampStart,
                (int)Parameters["BlueMOTSecondRampDuration"],
                (double)Parameters["BlueMOTFirstFieldValue"] //change here if second ramp needed
            );
            p.AddAnalogValue(
                "V00B2AOMAmp",
                lambdaMolassesStop,
                (double)Parameters["V00F2AOMAmpBlueMOT"]
            );
            p.AddLinearRamp(
                "V00B2AOMAmp",
                blueMOTHoldStart,
                (int)Parameters["BlueMOTHoldDuration"],
                (double)Parameters["V00F2AOMAmpBlueMOTEnd"]
            );
            p.AddAnalogValue(
                "V00R1plusAOMAmp",
                lambdaMolassesStop,
                (double)Parameters["V00F1plusAOMAmpBlueMOT"]
            );
            p.AddLinearRamp(
                "V00R1plusAOMAmp",
                blueMOTHoldStart,
                (int)Parameters["BlueMOTHoldDuration"],
                (double)Parameters["V00F1plusAOMAmpBlueMOTEnd"]
            );
            p.AddAnalogValue(
                "V00B1minusAOMAmp",
                lambdaMolassesStop,
                (double)Parameters["V00F1minusAOMAmpBlueMOT"]
            );
            p.AddLinearRamp(
                "V00B1minusAOMAmp",
                blueMOTHoldStart,
                (int)Parameters["BlueMOTHoldDuration"],
                (double)Parameters["V00F1minusAOMAmpBlueMOTEnd"]
            );
            p.AddAnalogValue(
                "motCoils",
                blueMOTStop,
                (double)Parameters["MOTCoilsOffValue"]
            );
            if ((double)Parameters["4thSidebandONorOFF"] > 5.0)
            {
                p.AddAnalogValue(
                    "V00R0AOMVCOAmp",
                    lambdaMolassesStop,
                    (double)Parameters["V00F0AOMAmpBlueMOT"]
                );
                p.AddLinearRamp(
                    "V00R0AOMVCOAmp",
                    blueMOTHoldStart,
                    (int)Parameters["BlueMOTHoldDuration"],
                    (double)Parameters["V00F0AOMAmpBlueMOTEnd"]
                );
            }
        }

        if ((double)Parameters["ODTONorOFF"] > 5.0)
        {
            p.AddAnalogValue(
                "ODTVVAControl",
                0,
                (double)Parameters["ODTVVAControlVoltage"]
            );
        }

        // imaging
        if ((double)Parameters["CloudImageONorOFF"] > 5.0)
        {
            p.AddAnalogValue(
                "V00B1minusAOMAmp",
                imagingLightStart,
                (double)Parameters["V00F1minusAOMAmpMolassesStart"]
            );
            p.AddAnalogValue(
                "V00B2AOMAmp",
                imagingLightStart,
                (double)Parameters["V00F2AOMAmpMolassesStart"]
            );
        }

            // Common and finishing steps

            p.AddAnalogValue(
            "V00R0AOMVCOAmp",
            (int)Parameters["ResetTime"],
            (double)Parameters["V00F0AOMAmpMax"]
        );
        p.AddAnalogValue(
           "V00R1plusAOMAmp",
           (int)Parameters["ResetTime"],
           (double)Parameters["V00F1plusAOMAmpMax"]
        );
        p.AddAnalogValue(
            "V00B2AOMAmp",
            (int)Parameters["ResetTime"],
            (double)Parameters["V00F2AOMAmpMax"]
        );
        p.AddAnalogValue(
           "V00B1minusAOMAmp",
           (int)Parameters["ResetTime"],
           (double)Parameters["V00F1minusAOMAmpMax"]
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

        return p;
    }

    public override AnalogStaticBuilder GetAnalogStatic()
    {
        AnalogStaticBuilder p = new AnalogStaticBuilder((int)Parameters["PatternLength"]);

        return p;
    }

}
