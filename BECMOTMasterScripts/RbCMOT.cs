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
        Parameters["RbPushBeamDuration"] = 95000;
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
        Parameters["BackgroundImageONorOFF"] = 10.0;
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

        p.Pulse(
            patternStartBeforeQ,
            (int)Parameters["Rb2DStart"],
            (int)Parameters["Rb2DDuration"],
            "Rb2DCoolingAOM"
        );

        p.Pulse(
            patternStartBeforeQ,
            (int)Parameters["Rb2DStart"],
            (int)Parameters["Rb2DDuration"],
            "Rb2DCoilsTTL"
        );
        if ((double)Parameters["PushBeamONorOFF"] > 5.0)
        {
            p.Pulse(
                patternStartBeforeQ,
                (int)Parameters["RbPushBeamStart"], // RbMolassesStart
                (int)Parameters["RbPushBeamDuration"],
                "RbPushBeamAOM"
            );
        }

        // 3D MOT 
        p.Pulse(
            patternStartBeforeQ,
            Rb3DMOTStart,
            RbCMOTStop - Rb3DMOTStart,
            "Rb3DCoolingAOM"
        );

        if ((double)Parameters["CMOTONorOFF"] > 5.0)
        {
            p.Pulse(
                patternStartBeforeQ,
                Rb3DMOTStop,
                RbCMOTStop - Rb3DMOTStop,
                "DDSTTLP0"
            );
            p.Pulse(
                patternStartBeforeQ,
                Rb3DMOTStop,
                RbCMOTStop - Rb3DMOTStop,
                "DDSTTLP1"
            );
        }

        // Rb molasses, DDS frequency 01
        if ((double)Parameters["MolassesONorOFF"] > 5.0)
        {
            p.Pulse(
            patternStartBeforeQ,
            RbMolassesRampStart,
            RbMolassesStop - RbMolassesRampStart,
            "Rb3DCoolingAOM"
        );

            p.Pulse(
                patternStartBeforeQ,
                RbMolassesRampStart,
                RbMolassesStop - RbMolassesRampStart,
                "DDSTTLP1"
            );
        }

        // odt
        if ((double)Parameters["ODTONorOFF"] > 5.0)
        {
            p.Pulse(
                patternStartBeforeQ,
                0,
                180000,//CameraStop + 100,
                "DipoleTrapAOM"
            );
        }
        // imaging with the on-resonance light
        p.Pulse(
            patternStartBeforeQ,
            imagingLightStart,
            (int)Parameters["CameraTriggerDuration"],
            "Rb3DCoolingAOM"
        );

        p.Pulse(
            patternStartBeforeQ,
            imagingLightStart,
            (int)Parameters["CameraTriggerDuration"],
            "DDSTTLP0"
        );

        p.Pulse(
            patternStartBeforeQ,
            CameraStart,
            (int)Parameters["CameraTriggerDuration"],
            "cameraTrigger"
        );

        p.Pulse(
            patternStartBeforeQ,
            ThorlabsCameraStart,
            (int)Parameters["CameraTriggerDuration"] + (int)Parameters["ThorlabsCameraExposureDelay"],
            "V00R0EOM" // second camera plugged into channel V00R0EOM for now
        );

        p.Pulse(
            patternStartBeforeQ,
            (int)Parameters["Camera2TriggerStart"],
            (int)Parameters["Camera2TriggerDuration"],
            "camera2Trigger"
        );

        if ((double)Parameters["BackgroundImageONorOFF"] > 5.0)
        {
            p.Pulse(
                patternStartBeforeQ,
                (int)Parameters["BackgroundImageTime"],
                (int)Parameters["CameraTriggerDuration"],
                "Rb3DCoolingAOM"
            );
            p.Pulse(
                patternStartBeforeQ,
                (int)Parameters["BackgroundImageTime"] - (int)Parameters["CameraExposureDelay"],
                (int)Parameters["CameraTriggerDuration"],
                "cameraTrigger"
            );
        }

        //p.Pulse(
        //    patternStartBeforeQ,
        //    0,
        //    1000,
        //    "RbOpAbsBeamAOM"
        //);

        return p;
    }

    public override AnalogPatternBuilder GetAnalogPattern()
    {
        AnalogPatternBuilder p = new AnalogPatternBuilder((int)Parameters["PatternLength"]);
        p.AddChannel("BXChirp");
        p.AddChannel("V00R0AOMVCOFreq");
        p.AddChannel("V00R0AOMVCOAmp");
        p.AddChannel("V00R1plusAOMAmp");
        p.AddChannel("V00R0EOMAmp");
        p.AddChannel("motCoils");
        p.AddChannel("ShimCoilX");
        p.AddChannel("ShimCoilY");
        p.AddChannel("ShimCoilZ");
        p.AddChannel("SlowingBField");
        p.AddChannel("ODTVVAControl");
        p.AddChannel("Rb3DCoolingAOMVCOAmp");
        p.AddChannel("Rb2DCoolingAOMVCOAmp");
        p.AddChannel("RbPushBeamAOMVCOAmp");
        p.AddChannel("RbOpAbsBeamAOMVCOAmp");

        int Rb3DMOTStart = (int)Parameters["Rb3DMOTStart"];
        int Rb3DMOTStop = Rb3DMOTStart + (int)Parameters["Rb3DMOTDuration"];
        int RbCMOTStop = Rb3DMOTStop + (int)Parameters["RbCMOTRampDuration"] + (int)Parameters["RbCMOTDuration"];
        int RbMolassesRampStart = RbCMOTStop + (int)Parameters["RbMOTFieldDecayDuration"];
        int RbMolassesStart = RbMolassesRampStart + (int)Parameters["RbMolassesRampDuration"];
        int RbMolassesStop = RbMolassesStart + (int)Parameters["RbMolassesHoldDuration"];
        int imagingLightStart = RbCMOTStop + (int)Parameters["CameraTriggerStartDelay"];
        int CameraStart = RbCMOTStop + (int)Parameters["CameraTriggerStartDelay"] - (int)Parameters["CameraExposureDelay"];
        int CameraStop = CameraStart + (int)Parameters["CameraTriggerDuration"];

        // common
        p.AddAnalogValue(
            "Rb2DCoolingAOMVCOAmp",
            0,
            (double)Parameters["Rb2DCoolingVCOAmp"]
        );

        // 2D MOT
        p.AddAnalogValue(
            "RbPushBeamAOMVCOAmp",
            0,
            (double)Parameters["RbPushBeamAOMVCOAmpPush"]
        );

        p.AddAnalogValue(
            "RbPushBeamAOMVCOAmp",
            (int)Parameters["RbPushBeamDuration"],
            0.0
        );

        // 3D MOT
        p.AddAnalogValue(
            "motCoils",
            Rb3DMOTStart,
            (double)Parameters["Rb3DMOTBField"]
        );
        p.AddAnalogValue(
            "Rb3DCoolingAOMVCOAmp",
            Rb3DMOTStart,
            (double)Parameters["Rb3DMOTVCAAmp"]
        );
        p.AddAnalogValue(
            "ShimCoilX",
            Rb3DMOTStart,
            (double)Parameters["XShimCoilsMOTValue"]
        );
        p.AddAnalogValue(
            "ShimCoilY",
            Rb3DMOTStart,
            (double)Parameters["YShimCoilsMOTValue"]
        );
        p.AddAnalogValue(
            "ShimCoilZ",
            Rb3DMOTStart,
            (double)Parameters["ZShimCoilsMOTValue"]
        );

        // CMOT
        if ((double)Parameters["CMOTONorOFF"] > 5.0)
        {
            p.AddLinearRamp(
                "motCoils",
                Rb3DMOTStop,
                (int)Parameters["RbCMOTRampDuration"],
                (double)Parameters["RbCMOTBField"]
            );
            p.AddLinearRamp(
                "Rb3DCoolingAOMVCOAmp",
                Rb3DMOTStop,
                (int)Parameters["RbCMOTRampDuration"],
                (double)Parameters["RbCMOTVCAAmp"]
            );
            p.AddAnalogValue(
                "ShimCoilX",
                Rb3DMOTStop,
                (double)Parameters["XShimCoilsCMOTValue"]
            );
            p.AddAnalogValue(
                "ShimCoilY",
                Rb3DMOTStop,
                (double)Parameters["YShimCoilsCMOTValue"]
            );
            p.AddAnalogValue(
                "ShimCoilZ",
                Rb3DMOTStop,
                (double)Parameters["ZShimCoilsCMOTValue"]
            );
        }

        // molasses
        /*p.AddAnalogValue(
            "motCoils",
            RbCMOTStop,
            0.0
        );
        p.AddAnalogValue(
            "Rb3DCoolingAOMVCOAmp",
            RbCMOTStop,
            (double)Parameters["RbMolassesVCAAmpStart"]
        );
        p.AddLinearRamp(
            "Rb3DCoolingAOMVCOAmp",
            RbMolassesRampStart,
            (int)Parameters["RbMolassesRampDuration"],
            (double)Parameters["RbMolassesVCAAmpEnd"]
        );

        p.AddLinearRamp(
            "ShimCoilX",
            RbCMOTStop + (int)Parameters["RbMolassesShimFieldDelay"],
            (int)Parameters["RbMolassesShimFieldRampDuration"],
            (double)Parameters["XShimCoilsMolassesValue"]
        );
        p.AddLinearRamp(
            "ShimCoilY",
            RbCMOTStop + (int)Parameters["RbMolassesShimFieldDelay"],
            (int)Parameters["RbMolassesShimFieldRampDuration"],
            (double)Parameters["YShimCoilsMolassesValue"]
        );
        p.AddLinearRamp(
            "ShimCoilZ",
            RbCMOTStop + (int)Parameters["RbMolassesShimFieldDelay"],
            (int)Parameters["RbMolassesShimFieldRampDuration"],
            (double)Parameters["ZShimCoilsMolassesValue"]
        );*/

        p.AddAnalogValue(
            "Rb3DCoolingAOMVCOAmp",
            RbCMOTStop,
            (double)Parameters["Rb3DMOTVCAAmp"]
        );

        // BG image and reset
        p.AddAnalogValue(
            "Rb3DCoolingAOMVCOAmp",
            (int)Parameters["BackgroundImageTime"] + (int)Parameters["CameraTriggerDuration"] + 100,
            3.0
        );
        p.AddAnalogValue(
            "ShimCoilX",
            (int)Parameters["BackgroundImageTime"],
            (double)Parameters["XShimCoilsOffValue"]
        );
        p.AddAnalogValue(
            "ShimCoilY",
            (int)Parameters["BackgroundImageTime"],
            (double)Parameters["YShimCoilsOffValue"]
        );
        p.AddAnalogValue(
            "ShimCoilZ",
            (int)Parameters["BackgroundImageTime"],
            (double)Parameters["ZShimCoilsOffValue"]
        );

        p.AddAnalogValue(
            "motCoils",
            CameraStop + 1000,
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
