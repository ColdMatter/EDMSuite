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
        Parameters["MOTONorOFF"] = 10.0;
        Parameters["CMOTONorOFF"] = 10.0;
        Parameters["CloudImageONorOFF"] = 10.0;
        Parameters["BackgroundImageONorOFF"] = 10.0;

        // ------------- LOCAL PARAMETERS -------------

        // Parameters["BXAOMFrequencyMHz"] = 100.0;


        /* Parameters["V00F0AOMAmpMOTLoading"] = 0.76; // 0.97;
        Parameters["V00F1plusAOMAmpMOTLoading"] = 0.6;
        Parameters["V00F2AOMAmpMOTLoading"] = 0.7;
        Parameters["V00F1minusAOMAmpMOTLoading"] = 0.58;

        Parameters["V00F2DDSFrequencyMHz"] = 64.62;
        Parameters["V00F1plusDDSFrequencyMHz"] = 76.5;
        Parameters["V00F1minusDDSFrequencyMHz"] = 78.2;

        Parameters["MOTCompressionRampDuration"] = 0;
        Parameters["MOTCompressionHoldDuration"] = 0; // 3000;

        Parameters["V00F0AOMAmpMOTCompression"] = 0.76;
        Parameters["V00F1plusAOMAmpMOTCompression"] = 0.6;
        Parameters["V00F2AOMAmpMOTCompression"] = 0.7;
        Parameters["V00F1minusAOMAmpMOTCompression"] = 0.55;*/

        //Parameters["V00F0AOMAmpMOTCompression"] = 0.97;
        //Parameters["V00F1plusAOMAmpMOTCompression"] = 0.615;
        //Parameters["V00F2AOMAmpMOTCompression"] = 0.71;
        //Parameters["V00F1minusAOMAmpMOTCompression"] = 0.62;

        /*Parameters["V00F0AOMAmpImaging"] = 0.97;
        Parameters["V00F1plusAOMAmpImaging"] = 0.615;
        Parameters["V00F2AOMAmpImaging"] = 0.71;
        Parameters["V00F1minusAOMAmpImaging"] = 0.62;

        Parameters["V00F0AOMAmpMax"] = 0.97;
        Parameters["V00F1plusAOMAmpMax"] = 0.615;
        Parameters["V00F2AOMAmpMax"] = 0.71;
        Parameters["V00F1minusAOMAmpMax"] = 0.62;*/

        // common and finishing
        Parameters["PatternLength"] = 60000;
        Parameters["TCLBlockStart"] = 5000;
        Parameters["ResetTime"] = 50000;

        // imaging
        Parameters["CameraTriggerStartDelay"] = 0; // imaging
        Parameters["CameraTriggerDuration"] = 500;
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
        int imagingLightStart = motLoadingStop + (int)Parameters["CameraTriggerStartDelay"];
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
            -1500,
            1100,
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
                slowingAOMStart,
                slowingChirpStop - slowingAOMStart,
                "BXAOM2"
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

        }


        // MOT + CMOT + imaging
        p.Pulse(
            patternStartBeforeQ,
            (int)Parameters["V00AOMONStartTime"],
            imagingLightStop,
            "V00R0AOM"
        );
        p.Pulse(
            patternStartBeforeQ,
            (int)Parameters["V00AOMONStartTime"],
            imagingLightStop,
            "V00R1plusAOMredMOT"
        );
        p.Pulse(
            patternStartBeforeQ,
            (int)Parameters["V00AOMONStartTime"],
            imagingLightStop,
            "V00B2AOMmolasses"
        );
        p.Pulse(
            patternStartBeforeQ,
            (int)Parameters["V00AOMONStartTime"],
            imagingLightStop,
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
        int imagingLightStart = motLoadingStop + (int)Parameters["CameraTriggerStartDelay"];
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

        
        p.AddAnalogValue(
            "motCoils",
            cameraStop,
            (double)Parameters["MOTCoilsOffValue"]
        );

        if ((double)Parameters["CloudImageONorOFF"] > 5.0)
        {
            p.AddAnalogValue(
                "V00R0AOMVCOAmp",
                cameraStart,
                (double)Parameters["V00F0AOMAmpImaging"]
            );
            p.AddAnalogValue(
                "V00R1plusAOMAmp",
                cameraStart,
                (double)Parameters["V00F1plusAOMAmpImaging"]
            );
            p.AddAnalogValue(
                "V00B2AOMAmp",
                cameraStart,
                (double)Parameters["V00F2AOMAmpImaging"]
            );
            p.AddAnalogValue(
                "V00B1minusAOMAmp",
                cameraStart,
                (double)Parameters["V00F1minusAOMAmpImaging"]
            );
            p.AddAnalogValue(
                "V00R0AOMVCOFreq",
                cameraStart,
                (double)Parameters["V00F0AOMFreqImaging"]
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
