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

        p.Pulse(patternStartBeforeQ, 0, 10, "analogPatternTrigger");

        return p;
    }

    public override AnalogPatternBuilder GetAnalogPattern()
    {
        AnalogPatternBuilder p = new AnalogPatternBuilder((int)Parameters["PatternLength"]);

        p.AddChannel("motCoils");
        p.AddChannel("SlowingBField");

        return p;
    }

    public override AnalogStaticBuilder GetAnalogStatic()
    {
        AnalogStaticBuilder p = new AnalogStaticBuilder((int)Parameters["PatternLength"]);

        return p;
    }
}
