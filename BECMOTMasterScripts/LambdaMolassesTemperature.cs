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
        Parameters["CloudImageONorOFF"] = 10.0;
        Parameters["BackgroundImageONorOFF"] = 10.0;

        // ------------- LOCAL PARAMETERS -------------

        Parameters["MOTCompressionRampDuration"] = 600;
        Parameters["MOTCompressionHoldDuration"] = 400;
        Parameters["LambdaMolassesHoldDuration"] = 200;

        Parameters["V00F2AOMAmpMolassesStart"] = 0.66; // 0.71;
        Parameters["V00F1minusAOMAmpMolassesStart"] = 0.62;
        Parameters["V00F2AOMAmpMolassesEnd"] = 0.71; // currently not used
        Parameters["V00F1minusAOMAmpMolassesEnd"] = 0.62; // currently not used

        Parameters["V00F0AOMAmpImaging"] = 0.97;
        Parameters["V00F1plusAOMAmpImaging"] = 0.615;
        Parameters["V00F2AOMAmpImaging"] = 0.71;
        Parameters["V00F1minusAOMAmpImaging"] = 0.62;

        Parameters["V00F0AOMAmpMax"] = 0.97;
        Parameters["V00F1plusAOMAmpMax"] = 0.615;
        Parameters["V00F2AOMAmpMax"] = 0.71;
        Parameters["V00F1minusAOMAmpMax"] = 0.62;

        // temp for driving yshim with slowing coils
        // Parameters["YShimCoilsMOTValue"] = 2.0;
        //Parameters["YShimCoilsCMOTValue"] = 2.0;
        //Parameters["YShimCoilsLambdaMolassesValue"] = 0.0;
        //Parameters["YShimCoilsOffValue"] = 0.0;

        // common and finishing
        Parameters["PatternLength"] = 60000;
        Parameters["TCLBlockStart"] = 5000;
        Parameters["ResetTime"] = 50000;

        // imaging
        Parameters["CameraTriggerStartDelay"] = 10;
        Parameters["CameraTriggerDuration"] = 100;
        Parameters["BackgroundImageTime"] = 35000;

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
        int imagingLightStart = lambdaMolassesStop + (int)Parameters["CameraTriggerStartDelay"];
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


            // switch F=2 frequency to lambda molasses
            p.Pulse(
                patternStartBeforeQ,
                lambdaMolassesStart,
                lambdaMolassesStop - lambdaMolassesStart,
                "DDSTTLP2"
            );

            // turn F=2 light on
            p.Pulse(
                patternStartBeforeQ,
                lambdaMolassesStart,
                lambdaMolassesStop - lambdaMolassesStart,
                "V00B2AOMmolasses"
            );

            // switch F=1- frequency to lambda molasses
            p.Pulse(
                patternStartBeforeQ,
                lambdaMolassesStart,
                lambdaMolassesStop - lambdaMolassesStart,
                "DDSTTLP3"
            );

            // turn F=1- light on
            p.Pulse(
                patternStartBeforeQ,
                lambdaMolassesStart,
                lambdaMolassesStop - lambdaMolassesStart,
                "V00B1minusAOM"
            );

        }

        // imaging
        p.Pulse(
            patternStartBeforeQ,
            imagingLightStart,
            (int)Parameters["CameraTriggerDuration"],
            "V00R0AOM"
        );
        p.Pulse(
            patternStartBeforeQ,
            imagingLightStart,
            (int)Parameters["CameraTriggerDuration"],
            "V00R1plusAOMredMOT"
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
            "V00R1plusAOMredMOT"
        );
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
            "V00B1minusAOM"
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
        int imagingLightStart = lambdaMolassesStop + (int)Parameters["CameraTriggerStartDelay"];
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
            /*p.AddAnalogValue(
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
            );*/
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

        if ((double)Parameters["CloudImageONorOFF"] > 5.0)
        {
            p.AddAnalogValue(
                "V00R0AOMVCOAmp",
                imagingLightStart,
                (double)Parameters["V00F0AOMAmpImaging"]
            );
            p.AddAnalogValue(
                "V00R1plusAOMAmp",
                imagingLightStart,
                (double)Parameters["V00F1plusAOMAmpImaging"]
            );
            p.AddAnalogValue(
                "V00B2AOMAmp",
                imagingLightStart,
                (double)Parameters["V00F2AOMAmpImaging"]
            );
            p.AddAnalogValue(
                "V00B1minusAOMAmp",
                imagingLightStart,
                (double)Parameters["V00F1minusAOMAmpImaging"]
            );
            p.AddAnalogValue(
                "V00R0AOMVCOFreq",
                imagingLightStart,
                (double)Parameters["V00F0AOMFreqImaging"]
            );

        }
        p.AddAnalogValue(
            "CavityRamp",
            0,
            -1.25
        );

        p.AddLinearRamp(
            "CavityRamp",
            lambdaMolassesStart,
            200,
            0
        );
        p.AddAnalogValue(
            "CavityRamp",
            imagingLightStop,
            -1.25
        );
        p.AddLinearRamp(
            "CavityRamp",
            (int)Parameters["BackgroundImageTime"],
            200,
            0
        );
        p.AddAnalogValue(
            "CavityRamp",
            (int)Parameters["ResetTime"],
            -1.25
        );


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
            "ShimCoilY",
            (int)Parameters["ResetTime"],
            0.0
        );
        p.AddAnalogValue(
            "ShimCoilZ",
            (int)Parameters["ResetTime"],
            (double)Parameters["ZShimCoilsOffValue"]
        );
        p.AddAnalogValue(
            "motCoils",
            cameraStop,
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
