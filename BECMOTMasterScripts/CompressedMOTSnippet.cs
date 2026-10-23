using MOTMaster;
using MOTMaster.SnippetLibrary;

using System;
using System.Collections.Generic;

using DAQ.Pattern;
using DAQ.Analog;
using DAQ.Environment;


/*
 * Compressed MOT, built from snippets. Should reproduce CompressedMOT.cs.
 *
 *   LoadMOT  ->  CompressMOT  ->  RedImaging  ...  Reset
 *
 * Each snippet only knows its own window. The phase boundaries are set once,
 * in the TIMELINE block below, from the global durations. Adjacent phases
 * share a boundary time, so one phase's falling edge and the next phase's
 * rising edge coincide.
 */
public class Patterns : MOTMasterScript
{
    public Patterns()
    {
        // Load global parameters
        string dataPath = (string)Environs.FileSystem.Paths["MOTMasterDataPath"];
        string element = (string)Environs.Hardware.GetInfo("Element");
        var io = new MMDataIOHelper(dataPath, element);
        string globalParameterPath = "C:\\ControlPrograms\\EDMSuite\\BECMOTMasterScripts\\globalParameters.txt";
        Parameters = new Dictionary<string, object>(io.LoadDictionary(globalParameterPath));

        // ------------- TOGGLES -------------

        Parameters["yagONorOFF"] = 10.0;
        Parameters["SlowingONorOFF"] = 10.0;
        Parameters["SlowingShutterONorOFF"] = 1.0;
        Parameters["MOTONorOFF"] = 10.0;
        Parameters["CMOTONorOFF"] = 10.0;
        Parameters["CloudImageONorOFF"] = 10.0;
        Parameters["BackgroundImageONorOFF"] = 1.0;
        Parameters["RepumpChirp"] = 1.0;

        // ------------- LOCAL PARAMETERS -------------

        // common and finishing
        Parameters["PatternLength"] = 70000;
        Parameters["PatternStart"] = 5000;
        Parameters["ResetTime"] = 60000;

        // slowing
        Parameters["HeShutterDuration"] = 1350;
        Parameters["BXChirpMHzSpan"] = 340.0;

        // imaging
        Parameters["CameraTriggerStartDelay"] = 1;
        Parameters["CameraTriggerDuration"] = 1000;
        Parameters["BackgroundImageTime"] = 45000;

        Parameters["V00F0DDSFrequencyMHz"] = 0;
        Parameters["V00F2DDSFrequencyMHz"] = 0;

        // ------------- TIMELINE (relative to PatternStart) -------------

        Parameters["MOTCompressionStart"] = (int)Parameters["MOTLoadingDuration"];

        Parameters["ImagingStart"] = (int)Parameters["MOTCompressionStart"]
                                     + (int)Parameters["MOTCompressionRampDuration"]
                                     + (int)Parameters["MOTCompressionHoldDuration"];
    }

    public override PatternBuilder32 GetDigitalPattern()
    {
        PatternBuilder32 p = new PatternBuilder32();

        new CaFBECLoadMOT(p, Parameters);       // must be first: fires analogPatternTrigger
        new CaFBECCompressMOT(p, Parameters);
        new CaFBECRedImaging(p, Parameters);

        return p;
    }

    public override AnalogPatternBuilder GetAnalogPattern()
    {
        AnalogPatternBuilder p = new AnalogPatternBuilder((int)Parameters["PatternLength"]);

        // Channels not driven by any snippet but kept in the pattern
        p.AddChannel("CavityRamp");
        p.AddChannel("ODTVVAControl");

        new CaFBECLoadMOT(p, Parameters);
        new CaFBECCompressMOT(p, Parameters);
        new CaFBECRedImaging(p, Parameters);
        new CaFBECReset(p, Parameters);

        return p;
    }

    public override AnalogStaticBuilder GetAnalogStatic()
    {
        return new AnalogStaticBuilder((int)Parameters["PatternLength"]);
    }
}
