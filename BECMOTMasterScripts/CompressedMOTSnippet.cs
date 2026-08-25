using MOTMaster;
using MOTMaster.SnippetLibrary;

using System;
using System.IO;
using System.Collections.Generic;

using DAQ.Pattern;
using DAQ.Analog;
using DAQ.Environment;
using DAQ;


public class Patterns : MOTMasterScript
/*
 * The time unit in this script is in multiple of 10 micro second.
 * The other unit is Volt.
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

        // common and finishing
        Parameters["PatternLength"] = 60000;
        Parameters["ResetTime"] = 50000;

        // imaging
        Parameters["CameraTriggerStartDelay"] = 0;
        Parameters["CameraTriggerDuration"] = 1000;
        Parameters["BackgroundImageTime"] = 35000;

    }

    public override PatternBuilder32 GetDigitalPattern()
    {
        PatternBuilder32 p = new PatternBuilder32();

        Parameters["MOTCompressionStart"] = (int)Parameters["MOTLoadingDuration"];
        Parameters["MOTCompressionStop"] = (int)Parameters["MOTCompressionStart"] + (int)Parameters["MOTCompressionRampDuration"] + (int)Parameters["MOTCompressionHoldDuration"];
        Parameters["ImagingStart"] = (int)Parameters["MOTCompressionStop"] + (int)Parameters["CameraTriggerStartDelay"];
        Parameters["ImagingStop"] = (int)Parameters["ImagingStart"] + (int)Parameters["CameraTriggerDuration"];

        MOTMasterScriptSnippet loadMOT = new CaFBECLoadMOT(p, Parameters);
        MOTMasterScriptSnippet compressMOT = new CaFBECCompressMOT(p, Parameters);
        MOTMasterScriptSnippet image = new CaFBECRedImaging(p, Parameters);

        return p;
    }

    public override AnalogPatternBuilder GetAnalogPattern()
    {
        AnalogPatternBuilder p = new AnalogPatternBuilder((int)Parameters["PatternLength"]);
        
        p.AddChannel("motCoils");

        Parameters["MOTCompressionStart"] = (int)Parameters["MOTLoadingDuration"];
        Parameters["MOTCompressionStop"] = (int)Parameters["MOTCompressionStart"] + (int)Parameters["MOTCompressionRampDuration"] + (int)Parameters["MOTCompressionHoldDuration"];
        Parameters["ImagingStart"] = (int)Parameters["MOTCompressionStop"] + (int)Parameters["CameraTriggerStartDelay"];
        Parameters["ImagingStop"] = (int)Parameters["ImagingStart"] + (int)Parameters["CameraTriggerDuration"];

        MOTMasterScriptSnippet loadMOT = new CaFBECLoadMOT(p, Parameters);
        MOTMasterScriptSnippet compressMOT = new CaFBECCompressMOT(p, Parameters);
        MOTMasterScriptSnippet image = new CaFBECRedImaging(p, Parameters);

        // Turn off coils
        p.AddAnalogValue(
            "motCoils",
            (int)Parameters["ImagingStop"],
            (double)Parameters["MOTCoilsOffValue"]
        );

        MOTMasterScriptSnippet reset = new CaFBECReset(p, Parameters);

        return p;
    }

    public override AnalogStaticBuilder GetAnalogStatic()
    {
        AnalogStaticBuilder p = new AnalogStaticBuilder((int)Parameters["PatternLength"]);
        return p;
    }
}

