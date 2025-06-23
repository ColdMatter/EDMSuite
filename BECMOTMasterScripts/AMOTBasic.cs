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
        Parameters["V00AOMONDuration"] = 30000;
        Parameters["V00MOTLoadingTime"] = 2500;
        Parameters["VCOFreqOutputValue"] = 3.1;
        Parameters["VCOAmplitudeAbsorptionOutputValue"] = 0.34;//0.375;
        Parameters["VCAAmplitudeAbsorptionOutputValue"] = 0.74;//0.68;
        Parameters["VCOAmplitudeOutputValue"] = 0.32;
        Parameters["VCAAmplitudeOutputValue"] = 0.72;
        Parameters["VCAAmplitudeBgImaging"] = 0.712;

        // microwave timing
        Parameters["MicrowavesOnStart"] = 1000;
        Parameters["MicrowavesOnDuration"] = 100;

        // B Field
        Parameters["SlowingCoilFieldValue"] = 5.0;
        Parameters["MOTCoilsStartTime"] = 0;
        Parameters["MOTCoilsStopTime"] = 20000;
        Parameters["MOTCoilsOnValue"] = -3.0;            // ~3.6 V should be 1 A, 3.0 optimal recently
        Parameters["MOTCoilsOffValue"] = 0.0;

        // Shim Coils
        Parameters["XShimCoilsOnValue"] = 0.0;
        Parameters["YShimCoilsOnValue"] = 0.0;
        Parameters["ZShimCoilsOnValue"] = 0.0;
        Parameters["XShimCoilsOffValue"] = 0.0;
        Parameters["YShimCoilsOffValue"] = 0.0;
        Parameters["ZShimCoilsOffValue"] = 0.0;

        // Camera trigger properties
        Parameters["CameraTriggerStart"] = 3000;
        Parameters["CameraTriggerDuration"] = 1000;
        Parameters["CameraExposureDelay"] = 78; // a constant of nature for Normal CCD
        Parameters["ImagingLightDelay"] = 0;

        // Switching control for iterative operations.
        // values higher than 5.0 leads to active state.
        Parameters["yagONorOFF"] = 10.0;
        Parameters["slowingONorOFF"] = 10.0;
        Parameters["transverseCoolingONorOFF"] = 1.0;

        // Added to scan DDS
        Parameters["DDSInitFrequency"] = 6.0;
        Parameters["DDSInitAmplitude"] = 220;

    }

    public override PatternBuilder32 GetDigitalPattern()
    {
        PatternBuilder32 p = new PatternBuilder32();
        int patternStartBeforeQ = (int)Parameters["TCLBlockStart"];
        int slowingAOMStart = (int)Parameters["BXAOMFreeFlightDuration"];
        int slowingChirpStart = slowingAOMStart + (int)Parameters["BXAOMPostBunchingDuration"];
        int slowingChirpStop = slowingChirpStart + (int)Parameters["BXAOMChirpDuration"];

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
            (int)Parameters["V00AOMONDuration"],
            "V00R0AOM"
        );

        p.Pulse(
            patternStartBeforeQ,
            (int)Parameters["V00AOMONStartTime"],
            (int)Parameters["V00AOMONDuration"],
            "V00R1plusAOM"
        );

        /*p.Pulse(
            patternStartBeforeQ,
            (int)Parameters["BackgroundImageTime"] - (int)Parameters["CameraTriggerStart"],
            (int)Parameters["CameraTriggerStart"] + (int)Parameters["CameraTriggerDuration"] + (int)Parameters["ImagingLightDelay"],
            "V00R0AOM"
        );*/

        /*p.Pulse(
            patternStartBeforeQ,
            (int)Parameters["BackgroundImageTime"] - (int)Parameters["CameraTriggerStart"],
            (int)Parameters["CameraTriggerStart"] + (int)Parameters["CameraTriggerDuration"] + (int)Parameters["ImagingLightDelay"],
            "V00R1plusAOM"
        );*/

        p.Pulse(
            patternStartBeforeQ,
            (int)Parameters["V00AOMONStartTime"] - 50,
            slowingChirpStop + 50,
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
                slowingAOMStart,
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


            if ((double)Parameters["transverseCoolingONorOFF"] > 5.0)
            {
                p.Pulse(
                    patternStartBeforeQ,
                    1,
                    slowingAOMStart,
                    "BXTCEOMSwitch"
                );

                p.Pulse(
                    patternStartBeforeQ,
                    1,
                    2000,
                    "BXTCAOMSwitch"
                );
            }

        }

        /*p.Pulse(
            patternStartBeforeQ,
            (int)Parameters["MicrowavesOnStart"],
            (int)Parameters["MicrowavesOnDuration"],
            "MicrowaveSwitch"
        );*/

        p.Pulse(
            patternStartBeforeQ,
            (int)Parameters["CameraTriggerStart"] - (int)Parameters["CameraExposureDelay"],
            (int)Parameters["CameraTriggerDuration"],
            "cameraTrigger"
        );

        p.Pulse(
            patternStartBeforeQ,
            (int)Parameters["CameraTriggerStart"],
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
        p.AddChannel("V00R0AOMVCOFreq");
        p.AddChannel("V00R0AOMVCOAmp");
        p.AddChannel("V00R1plusAOMAmp");
        p.AddChannel("V00R0EOMVCOAmp");
        p.AddChannel("motCoils");
        p.AddChannel("ShimCoilX");
        p.AddChannel("ShimCoilY");
        p.AddChannel("ShimCoilZ");
        p.AddChannel("SlowingBField");

        int slowingChirpStart = (int)Parameters["BXAOMFreeFlightDuration"] + (int)Parameters["BXAOMPostBunchingDuration"];
        int slowingChirpStop = slowingChirpStart + (int)Parameters["BXAOMChirpDuration"];

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
                slowingChirpStop + 500,
                1000,
                (double)Parameters["BXChirpStartValue"]
            );
        }

        p.AddAnalogValue(
            "motCoils",
            (int)Parameters["MOTCoilsStartTime"],
            (double)Parameters["MOTCoilsOnValue"]
        );

        p.AddAnalogValue(
           "motCoils",
           (int)Parameters["MOTCoilsStopTime"],
           (double)Parameters["MOTCoilsOffValue"]
        );

        p.AddAnalogValue(
            "V00R0AOMVCOFreq",
            0,
            (double)Parameters["VCOFreqOutputValue"]
        );

        p.AddAnalogValue(
            "V00R0AOMVCOAmp",
            0,
            (double)Parameters["VCOAmplitudeAbsorptionOutputValue"]
        );
        p.AddAnalogValue(
            "V00R1plusAOMAmp",
            0,
            (double)Parameters["VCAAmplitudeAbsorptionOutputValue"]
        );
        p.AddAnalogValue(
            "V00R0AOMVCOAmp",
            (int)Parameters["BXAOMFreeFlightDuration"],
            (double)Parameters["VCOAmplitudeOutputValue"]
        );
        p.AddAnalogValue(
            "V00R1plusAOMAmp",
            (int)Parameters["BXAOMFreeFlightDuration"],
            (double)Parameters["VCAAmplitudeOutputValue"]
        );

        p.AddAnalogValue(
            "V00R0AOMVCOFreq",
            (int)Parameters["CameraTriggerStart"] - 50,
            3.1
        );
        p.AddAnalogValue(
            "V00R0AOMVCOAmp",
            (int)Parameters["CameraTriggerStart"] - 50,
            0.30
        );
        p.AddAnalogValue(
            "V00R1plusAOMAmp",
            (int)Parameters["CameraTriggerStart"] - 50,
            1.0
        );

        p.AddAnalogValue(
            "V00R0AOMVCOAmp",
            (int)Parameters["CameraTriggerStart"] + 2000,
            (double)Parameters["VCOAmplitudeOutputValue"]
        );
        p.AddAnalogValue(
            "V00R1plusAOMAmp",
            (int)Parameters["CameraTriggerStart"] + 2000,
            (double)Parameters["VCAAmplitudeOutputValue"]
        );

        p.AddAnalogValue(
            "V00R1plusAOMAmp",
            (int)Parameters["BackgroundImageTime"] - 50,
            (double)Parameters["VCAAmplitudeBgImaging"]
        );
        p.AddAnalogValue(
            "V00R0AOMVCOAmp",
            (int)Parameters["BackgroundImageTime"] - 50,
            0.30
        );

        p.AddAnalogValue(
            "V00R1plusAOMAmp",
            (int)Parameters["BackgroundImageTime"] + (int)Parameters["CameraTriggerDuration"],
            1.0
        );

        p.AddAnalogValue(
           "V00R0EOMVCOAmp",
           0,
           -1.0
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

        return p;
    }

    public override AnalogStaticBuilder GetAnalogStatic()
    {
        AnalogStaticBuilder p = new AnalogStaticBuilder((int)Parameters["PatternLength"]);

        return p;
    }
}
