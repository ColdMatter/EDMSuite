using MOTMaster;
using System;
using System.Collections.Generic;

using DAQ.Pattern;
using DAQ.Analog;


public class Patterns : MOTMasterScript
/*
 * This script is designed to create the first MOT
 * The time unit in this script is in multiple of 10 micro second.
 * The other unit is Volt.
 * */
{
    public Patterns()
    {
        Parameters = new Dictionary<string, object>();
        Parameters["PatternLength"] = 50000;
        Parameters["BackgroundImageTime"] = 25000; // half the pattern length
        Parameters["TCLBlockStart"] = 10000;
        Parameters["FlashToQ"] = 16;
        Parameters["QSwitchPulseDuration"] = 10;

        // Slowing parameters
        Parameters["BXAOMFreeFlightDuration"] = 450;
        Parameters["BXAOMPostBunchingDuration"] = 100;
        Parameters["BXAOMChirpDuration"] = 550;
        Parameters["BXChirpStartValue"] = 0.0;
        Parameters["SlowingMHzPerVolt"] = -1350.0;             // Azurlight BX laser 1V --> -1350 MHz
        Parameters["SlowingChirpMHzSpan"] = 360.0;             // 564.582580 THz is the resonance with -100 MHz AOM

        // cooling and trapping parameters
        Parameters["V00AOMONStartTime"] = 0;
        Parameters["MOTLoadingDuration"] = 2000;
        Parameters["MOTRetainingDuration"] = 500;
        Parameters["MOTCompressionRampDuration"] = 10;
        Parameters["MOTCompressionDuration"] = 300;
        Parameters["OpticalPumpingDelay"] = 100;
        Parameters["OpticalPumpingDuration"] = 100;
        Parameters["V00AOMONDuration"] = 30000;

        Parameters["V00R0AOMVCOFreqMOTLoading"] = 3.1;
        Parameters["V00R0AOMVCOFreqMOTRetaining"] = 3.1;
        Parameters["V00R0AOMVCOFreqMOTCompression"] = 3.1;
        Parameters["V00R0AOMVCOFreqImaging"] = 3.25;       /////
        Parameters["V00R0AOMVCOAmpMOTLoading"] = 0.34;
        Parameters["V00R0AOMVCOAmpMOTRetaining"] = 0.36;
        Parameters["V00R0AOMVCOAmpMOTCompression"] = 0.36;
        Parameters["V00R0AOMVCOAmpOpticalPumping"] = 0.55;
        //Parameters["V00R0AOMVCOAmpOpticalPumping"] = 0.3;
        Parameters["V00R0AOMVCOAmpImaging"] = 0.362;
        Parameters["V00R0AOMVCOAmpMax"] = 0.30;
        Parameters["V00R1plusAOMAmpMOTLoading"] = 0.74;
        Parameters["V00R1plusAOMAmpMOTRetaining"] = 0.68;
        Parameters["V00R1plusAOMAmpMOTCompression"] = 0.68;
        Parameters["V00R1plusAOMAmpOpticalPumping"] = 0.59;
        Parameters["V00R1plusAOMVCOAmpImaging"] = 0.59;
        Parameters["V00R1plusAOMAmpMax"] = 0.8;
        Parameters["V00B2AOMAmpMolassesStart"] = 0.72;
        Parameters["V00B2AOMAmpMolassesEnd"] = 0.72;
        Parameters["V00R0EOMVCOOpticalPumping"] = -2.2;

        // B Field
        Parameters["SlowingCoilFieldValue"] = 5.0;
        Parameters["MOTCoilsStartTime"] = 0;
        Parameters["MOTCoilsStopTime"] = 10000;
        Parameters["MOTLoadingFieldValue"] = 3.0;
        Parameters["MOTCompressionFieldValue"] = 6.0;
        Parameters["MOTCoilsOffValue"] = 0.0;

        // Shim Coils
        Parameters["XShimCoilsOnValue"] = -5.0;
        Parameters["XShimCoilsOffValue"] = 0.0;
        Parameters["YShimCoilsOnValue"] = 0.0;
        Parameters["YShimCoilsOffValue"] = 0.0;
        Parameters["ZShimCoilsOnValue"] = 0.0;
        Parameters["ZShimCoilsOffValue"] = 0.0;

        // Camera trigger properties
        Parameters["CameraTriggerStartDelay"] = 10;
        Parameters["CameraTriggerDuration"] = 200;
        Parameters["CameraExposureDelay"] = 78; // a constant of nature for Normal CCD

        // Switching control for iterative operations.
        // values higher than 5.0 leads to active state.
        Parameters["yagONorOFF"] = 10.0;
        Parameters["slowingONorOFF"] = 10.0;
        Parameters["opticalPumpingONorOFF"] = 1.0;

        Parameters["DummyFrequency"] = 1.0;

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
        int opticalPumpingStart = motCompressionStop + (int)Parameters["OpticalPumpingDelay"];
        int opticalPumpingStop = opticalPumpingStart + (int)Parameters["OpticalPumpingDuration"];
        int cameraStart = opticalPumpingStop + 100 - (int)Parameters["OpticalPumpingDuration"] + (int)Parameters["CameraTriggerStartDelay"];
        int cameraStop = cameraStart + (int)Parameters["CameraTriggerDuration"];

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



        if ((double)Parameters["opticalPumpingONorOFF"] > 5.0)
        {
            p.Pulse(
                patternStartBeforeQ,
                opticalPumpingStart,
                (int)Parameters["OpticalPumpingDuration"],
                "V00R0AOM"
            );

            p.Pulse(
                patternStartBeforeQ,
                opticalPumpingStart,
                (int)Parameters["OpticalPumpingDuration"],
                "V00R1plusAOM"
            );
        }
        

        p.Pulse(
            patternStartBeforeQ,
            cameraStart,
            (int)Parameters["CameraTriggerDuration"],
            "V00R1plusAOMJump"
        );

        /*p.Pulse(
            patternStartBeforeQ,
            cameraStart,
            (int)Parameters["CameraTriggerDuration"],
            "V00R1plusAOM"
        );*/

        p.Pulse(
            patternStartBeforeQ,
            (int)Parameters["BackgroundImageTime"],
            (int)Parameters["CameraTriggerDuration"],
            "V00R1plusAOMJump"
        );


        p.Pulse(
            patternStartBeforeQ,
            (int)Parameters["V00AOMONStartTime"] - 50,
            (int)Parameters["V00AOMONDuration"],
            "V00R0EOM"
        );

        if ((double)Parameters["slowingONorOFF"] > 5.0)
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

        p.Pulse(
            patternStartBeforeQ,
            slowingChirpStop,
            30000,
            "greenShutter"
        );

        p.Pulse(
            patternStartBeforeQ,
            cameraStart - (int)Parameters["CameraExposureDelay"],
            (int)Parameters["CameraTriggerDuration"],
            "cameraTrigger"
        );

        p.Pulse(
            patternStartBeforeQ,
            cameraStart,
            (int)Parameters["CameraTriggerDuration"],
            "camera2Trigger"
        );

        p.Pulse(
            patternStartBeforeQ,
            (int)Parameters["BackgroundImageTime"] - (int)Parameters["CameraExposureDelay"],
            (int)Parameters["CameraTriggerDuration"],
            "cameraTrigger"
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
        int opticalPumpingStart = motCompressionStop + (int)Parameters["OpticalPumpingDelay"];
        int opticalPumpingStop = opticalPumpingStart + (int)Parameters["OpticalPumpingDuration"];
        int cameraStart = opticalPumpingStop + 500 - (int)Parameters["OpticalPumpingDuration"] + (int)Parameters["CameraTriggerStartDelay"];
        int cameraStop = cameraStart + (int)Parameters["CameraTriggerDuration"];


        if ((double)Parameters["slowingONorOFF"] > 5.0)
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
        }

        p.AddAnalogValue(
            "motCoils",
            (int)Parameters["MOTCoilsStartTime"],
            (double)Parameters["MOTLoadingFieldValue"]
        );

        p.AddLinearRamp(
            "motCoils",
            motRetainingStop,
            (int)Parameters["MOTCompressionRampDuration"],
            (double)Parameters["MOTCompressionFieldValue"]
        );

        p.AddAnalogValue(
           "motCoils",
           motCompressionStop,
           (double)Parameters["MOTCoilsOffValue"]
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
            "V00R1plusAOMAmp",
            motRetainingStop,
            (double)Parameters["V00R1plusAOMAmpMOTCompression"]
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
        /*p.AddAnalogValue(
            "V00R0AOMVCOAmp",
            opticalPumpingStop,
            (double)Parameters["V00R0AOMVCOAmpImaging"]
        );
        p.AddAnalogValue(
            "V00R0AOMVCOFreq",
            opticalPumpingStop,
            (double)Parameters["V00R0AOMVCOFreqImaging"]
        );*/

        p.AddAnalogValue(
           "V00R1plusAOMAmp",
           opticalPumpingStop,
           (double)Parameters["V00R1plusAOMVCOAmpImaging"]
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
            "V00R0AOMVCOFreq",
            30000,
            (double)Parameters["V00R0AOMVCOFreqMOTLoading"]
        );


        p.AddAnalogValue(
           "V00R0EOMVCOAmp",
           0,
           -1.1
        );
        p.AddAnalogValue(
           "V00R0EOMVCOAmp",
           motCompressionStop,
           (double)Parameters["V00R0EOMVCOOpticalPumping"]
        );
        p.AddAnalogValue(
           "V00R0EOMVCOAmp",
           opticalPumpingStop,
           0.0
        );
        p.AddAnalogValue(
           "V00R0EOMVCOAmp",
           cameraStop,
           -1.1
        );

        p.AddAnalogValue(
            "ShimCoilX",
            0,
            (double)Parameters["XShimCoilsOnValue"]
        );
        p.AddAnalogValue(
            "ShimCoilY",
            0,
            (double)Parameters["YShimCoilsOnValue"]
        );
        p.AddAnalogValue(
            "ShimCoilZ",
            0,
            (double)Parameters["ZShimCoilsOnValue"]
        );
        p.AddAnalogValue(
            "ShimCoilX",
            (int)Parameters["MOTCoilsStopTime"],
            (double)Parameters["XShimCoilsOffValue"]
        );
        p.AddAnalogValue(
            "ShimCoilY",
            (int)Parameters["MOTCoilsStopTime"],
            (double)Parameters["YShimCoilsOffValue"]
        );
        p.AddAnalogValue(
            "ShimCoilZ",
            (int)Parameters["MOTCoilsStopTime"],
            (double)Parameters["ZShimCoilsOffValue"]
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
            "CavityRamp",
            0,
            -0.7
        );
        p.AddLinearRamp(
            "CavityRamp",
            cameraStart,
            100,
            0.7
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
