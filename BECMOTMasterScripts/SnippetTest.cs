using MOTMaster;
using MOTMaster.SnippetLibrary;

using System;
using System.Collections.Generic;

using DAQ.Pattern;
using DAQ.Analog;
using DAQ.Environment;


public class Patterns : MOTMasterScript
/*
 * This script is designed to create the basic MOT, nothing fancy
 * The time unit in this script is in multiple of 10 micro second.
 * The other unit is Volt.
 * */
{
    public Patterns()
    {
        /* --------- CREATE IOHELPER FOR DICTIONARY --------- */

        // define dataPath and element
        string dataPath = (string)Environs.FileSystem.Paths["MOTMasterDataPath"];
        string element = (string)Environs.Hardware.GetInfo("Element");

        // create instance of IO helper
        var io = new MMDataIOHelper(dataPath, element);

        /* --------- CREATE PARAMETERS DICTIONARY --------- */

        // load global and local parameters into separate dictionaries from .txt files
        string globalParameterPath = "C:\\ControlPrograms\\EDMSuite\\BECMOTMasterSnippets\\globalParameters.txt";
        //string localParameterPath = "C:\\ControlPrograms\\EDMSuite\\BECMOTMasterSnippets\\CMOTParameters.txt";

        var globalParameters = io.LoadDictionary(globalParameterPath);
        //var localParameters = io.LoadDictionary(localParameterPath);

        // combine into a single parameters dictionary
        Parameters = new Dictionary<string, object>(globalParameters);
        //foreach (var kv in localParameters){ Parameters[kv.Key] = kv.Value; } // overwrites if key already exists in globalParameters

        //Parameters = new Dictionary<string, object>();
        Parameters["PatternLength"] = 60000;

        Parameters["yagONorOFF"] = 10.0;
        Parameters["SlowingONorOFF"] = 10.0;
        Parameters["MOTONorOFF"] = 10.0;

        // Compressed MOT
        Parameters["CMOTONorOFF"] = 10.0;
        Parameters["MOTCompressionRampDuration"] = 600;
        Parameters["MOTCompressionDuration"] = 400;
        Parameters["V00R0AOMVCOFreqMOTCompression"] = 3.1;
        Parameters["V00R0AOMVCOAmpMOTCompression"] = 0.42;      
        Parameters["V00R1plusAOMAmpMOTCompression"] = 0.66;     
        Parameters["MOTCompressionFieldValue"] = 6.0;
        Parameters["XShimCoilsCMOTValue"] = 0.0;
        Parameters["YShimCoilsCMOTValue"] = 0.0;
        Parameters["ZShimCoilsCMOTValue"] = 0.0;

        // Imaging
        Parameters["CloudImageONorOFF"] = 10.0;
        Parameters["BackgroundImageONorOFF"] = 10.0;
        Parameters["CameraTriggerStartDelay"] = 1;
        Parameters["CameraTriggerDuration"] = 1000;
        Parameters["CameraExposureDelay"] = 78;
        Parameters["BackgroundImageTime"] = 35000;
        Parameters["V00R0AOMVCOFreqImaging"] = 3.1;
        Parameters["V00R0AOMVCOAmpImaging"] = 0.32;
        Parameters["V00R1plusAOMAmpImaging"] = 0.72;
        Parameters["ThorlabsCameraExposureDelay"] = 2000;

        // Common and Finishing
        Parameters["ResetTime"] = 50000;
        Parameters["V00R0AOMVCOAmpMax"] = 0.30;
        Parameters["V00R1plusAOMAmpMax"] = 0.8;
        Parameters["MOTCoilsOffValue"] = 0.0;
        Parameters["XShimCoilsOffValue"] = 0.0;
        Parameters["YShimCoilsOffValue"] = 0.0;
        Parameters["ZShimCoilsOffValue"] = 0.0;

        Parameters["DDSInitAmplitude"] = 0.0;   // dummy
    }

    public override PatternBuilder32 GetDigitalPattern()
    {
        PatternBuilder32 p = new PatternBuilder32();
        p.EnforceTimeOrdering(false);

        int patternStartBeforeQ = (int)Parameters["TCLBlockStart"];

        int motLoadingStop = (int)Parameters["MOTLoadingDuration"];
        int motCompressionStop = motLoadingStop + (int)Parameters["MOTCompressionRampDuration"] + (int)Parameters["MOTCompressionDuration"];
        int imagingLightStart = motCompressionStop + (int)Parameters["CameraTriggerStartDelay"];
        int imagingLightStop = imagingLightStart + (int)Parameters["CameraTriggerDuration"];
        int cameraStart = imagingLightStart - (int)Parameters["CameraExposureDelay"];
        int cameraStop = cameraStart + (int)Parameters["CameraTriggerDuration"];
        int ThorlabsCameraStart = imagingLightStart - (int)Parameters["ThorlabsCameraExposureDelay"];
        int bgCameraStart = (int)Parameters["BackgroundImageTime"] - (int)Parameters["CameraExposureDelay"];

        MOTMasterScriptSnippet lm = new CaFBECLoadMOT(p, Parameters);

        p.Pulse(
            patternStartBeforeQ,
            motLoadingStop,
            imagingLightStop,
            "V00R0AOM"
            );
        p.Pulse(
            patternStartBeforeQ,
            motLoadingStop,
            imagingLightStop,
            "V00R1plusAOMredMOT"
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
        p.AddChannel("V00R0EOMAmp");
        p.AddChannel("V00B2AOMAmp");
        p.AddChannel("motCoils");
        p.AddChannel("ShimCoilX");
        p.AddChannel("ShimCoilY");
        p.AddChannel("ShimCoilZ");
        p.AddChannel("CavityRamp");

        MOTMasterScriptSnippet lm = new CaFBECLoadMOT(p, Parameters);

        int slowingChirpStart = (int)Parameters["BXAOMFreeFlightDuration"] + (int)Parameters["BXAOMPostBunchingDuration"];
        int slowingChirpStop = slowingChirpStart + (int)Parameters["BXAOMChirpDuration"];

        int motLoadingStop = (int)Parameters["MOTLoadingDuration"];
        int motCompressionStop = motLoadingStop + (int)Parameters["MOTCompressionRampDuration"] + (int)Parameters["MOTCompressionDuration"];
        int imagingLightStart = motCompressionStop + (int)Parameters["CameraTriggerStartDelay"];
        int imagingLightStop = imagingLightStart + (int)Parameters["CameraTriggerDuration"];
        int cameraStart = imagingLightStart - (int)Parameters["CameraExposureDelay"];
        int cameraStop = cameraStart + (int)Parameters["CameraTriggerDuration"];
        int bgCameraStart = (int)Parameters["BackgroundImageTime"] - (int)Parameters["CameraExposureDelay"];

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
        }
        p.AddAnalogValue(
            "motCoils",
            cameraStop,
            (double)Parameters["MOTCoilsOffValue"]
        );

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

