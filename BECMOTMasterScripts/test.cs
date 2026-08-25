using MOTMaster;
using System;
using System.Collections.Generic;

using DAQ.Pattern;
using DAQ.Analog;


public class Patterns : MOTMasterScript
/*
 * This script is designed to create the basic MOT, nothing fancy
 * The time unit in this script is in multiple of 10 micro second.
 * The other unit is Volt.
 * */
{
    public Patterns()
    {
        Parameters = new Dictionary<string, object>();
        Parameters["PatternLength"] = 200000;
        Parameters["TCLBlockStart"] = 5000;

        // Rb 2D MOT 
        Parameters["PushBeamONorOFF"] = 10.0;
        Parameters["Rb2DStart"] = -1500;
        Parameters["Rb2DDuration"] = 100000;
        Parameters["Rb2DCoolingVCOAmp"] = 0.58;
        Parameters["RbPushBeamStart"] = 0;
        Parameters["RbPushBeamDuration"] = 55000;
        Parameters["RbPushBeamAOMVCOAmpPush"] = 7.5;

        // Rb 3D MOT
        Parameters["Rb3DMOTStart"] = 0; // 3D Cooling AOM
        Parameters["Rb3DMOTDuration"] = 100000;
        Parameters["Rb3DMOTVCAAmp"] = 3.0;
        Parameters["RbFrequencyDummy"] = 10.0;
        Parameters["RbAmpDummy"] = 0.2;
        Parameters["RbShimDummy"] = 0.2;
        Parameters["Rb3DMOTBField"] = 1.9;            // ~3.6 V should be 1 A, 3.0 optimal recently
        Parameters["XShimCoilsMOTValue"] = 0.0;
        Parameters["YShimCoilsMOTValue"] = 0.0;
        Parameters["ZShimCoilsMOTValue"] = 0.0;

        // Rb CMOT
        Parameters["CMOTONorOFF"] = 1.0;
        Parameters["RbCMOTRampDuration"] = 1; // 550 
        Parameters["RbCMOTDuration"] = 1;
        Parameters["RbCMOTVCAAmp"] = 3.0;
        Parameters["RbCMOTBField"] = 6.0; // 2.0 
        Parameters["XShimCoilsCMOTValue"] = 0.0;
        Parameters["YShimCoilsCMOTValue"] = 0.0;
        Parameters["ZShimCoilsCMOTValue"] = 0.0;

        // Rb Molasses
        Parameters["MolassesONorOFF"] = 1.0;
        Parameters["RbMOTFieldDecayDuration"] = 1;
        Parameters["RbMolassesRampDuration"] = 1;
        Parameters["RbMolassesHoldDuration"] = 250;//250;
        Parameters["RbMolassesVCAAmpStart"] = 2.8;
        Parameters["RbMolassesVCAAmpEnd"] = 2.8;
        Parameters["XShimCoilsMolassesValue"] = 0.0;
        Parameters["YShimCoilsMolassesValue"] = 0.0;
        Parameters["ZShimCoilsMolassesValue"] = 0.0;
        Parameters["RbMolassesShimFieldDelay"] = 550;
        Parameters["RbMolassesShimFieldRampDuration"] = 100;

        // ODT
        Parameters["ODTONorOFF"] = 1.0;
        Parameters["ODTOnlyDuration"] = 100; // duration not used yet

        // Camera trigger properties
        Parameters["BackgroundImageONorOFF"] = 1.0;
        Parameters["CameraTriggerStartDelay"] = 1;
        Parameters["CameraTriggerDuration"] = 100;
        Parameters["BackgroundImageTime"] = 170000; 
        Parameters["CameraExposureDelay"] = 0; 
        Parameters["ThorlabsCameraExposureDelay"] = 2000;
        Parameters["ImagingLightDelay"] = 0;
        Parameters["Camera2TriggerStart"] = 7000;
        Parameters["Camera2TriggerDuration"] = 1;

        // Common and Finishing
        Parameters["MOTCoilsOffValue"] = 0.0;
        Parameters["XShimCoilsOffValue"] = 0.0;
        Parameters["YShimCoilsOffValue"] = 0.0;
        Parameters["ZShimCoilsOffValue"] = 0.0;

    }

    public override PatternBuilder32 GetDigitalPattern()
    {
        PatternBuilder32 p = new PatternBuilder32();
        int patternStartBeforeQ = (int)Parameters["TCLBlockStart"];
        int Rb3DMOTStart = (int)Parameters["Rb3DMOTStart"];
        int Rb3DMOTStop = Rb3DMOTStart + (int)Parameters["Rb3DMOTDuration"];
        int RbCMOTStop = Rb3DMOTStop + (int)Parameters["RbCMOTRampDuration"] + (int)Parameters["RbCMOTDuration"];
        int RbMolassesRampStart = RbCMOTStop + (int)Parameters["RbMOTFieldDecayDuration"];
        int RbMolassesStart = RbMolassesRampStart + (int)Parameters["RbMolassesRampDuration"];
        int RbMolassesStop = RbMolassesStart + (int)Parameters["RbMolassesHoldDuration"];
        int imagingLightStart = RbCMOTStop + (int)Parameters["CameraTriggerStartDelay"];
        int CameraStart = RbCMOTStop + (int)Parameters["CameraTriggerStartDelay"] - (int)Parameters["CameraExposureDelay"];
        int ThorlabsCameraStart = RbMolassesStop + (int)Parameters["CameraTriggerStartDelay"] - (int)Parameters["ThorlabsCameraExposureDelay"];
        int CameraStop = CameraStart + (int)Parameters["CameraTriggerDuration"];

        p.Pulse(patternStartBeforeQ, 0, 10, "analogPatternTrigger");

        return p;
    }

    public override AnalogPatternBuilder GetAnalogPattern()
    {
        AnalogPatternBuilder p = new AnalogPatternBuilder((int)Parameters["PatternLength"]);
        p.AddChannel("RbOpAbsBeamAOMVCOAmp");

        p.AddAnalogValue(
            "RbOpAbsBeamAOMVCOAmp",
            0,
            10.0
        );

        return p;
    }

    public override AnalogStaticBuilder GetAnalogStatic()
    {
        AnalogStaticBuilder p = new AnalogStaticBuilder((int)Parameters["PatternLength"]);

        return p;
    }
}
