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
        Parameters["TCLBlockStart"] = 10000;
        Parameters["FlashToQ"] = 16;
        Parameters["QSwitchPulseDuration"] = 10;

        // Fundamental slowing calibration parameters
        double slowingMHzPerVolt = -1350.0;             // Azurlight BX laser 1V --> -1350 MHz
        Parameters["SlowingChirpMHzSpan"] = 360.0;      // 564.582580 THz is the resonance with -110 MHz AOM

        // Slowing parameters
        Parameters["BXAOMPreBunchingStartTime"] = 0;
        Parameters["BXAOMPreBunchingDuration"] = 0;
        Parameters["BXAOMFreeFlightDuration"] = 400;
        Parameters["BXAOMPostBunchingDuration"] = 100;
        Parameters["BXAOMChirpDuration"] = 600;
        Parameters["BXChirpStartValue"] = 0.0;
        Parameters["BXChirpEndValue"] = (double)Parameters["SlowingChirpMHzSpan"] / slowingMHzPerVolt;
        Parameters["BXShutterONStartTime"] = -3000;
        Parameters["BXShutterONDuration"] = 2000;

        // cooling and trapping parameters
        Parameters["V00AOMONStartTime"] = 0;
        Parameters["V00AOMONDuration"] = 17000;
        Parameters["V00MOTLoadingTime"] = 2000;
        Parameters["VCOFreqOutputValue"] = 4.0;
        Parameters["VCOAmplitudeOutputValue"] = 0.32;

        // B Field
        Parameters["SlowingCoilFieldValue"] = 2.0;
        Parameters["MOTCoilsStartTime"] = 0;
        Parameters["MOTCoilsStopTime"] = 17000;
        Parameters["MOTCoilsOnValue"] = 3.6;            // ~3.6 V should be 1 A
        Parameters["MOTCoilsOffValue"] = 0.0;

        // Camera trigger properties
        Parameters["CameraTriggerStart"] = 4000;
        Parameters["CameraTriggerDuration"] = 50;

        // Switching control for iterative operations.
        // values higher than 5.0 leads to active state.
        Parameters["yagONorOFF"] = 10.0;
        Parameters["slowingONorOFF"] = 10.0;
        Parameters["SlowingPokeStart"] = 4000;
        Parameters["SlowingPokeDuration"] = 100;
        Parameters["OscillationDuration"] = 50;

        // Added to scan DDS
        Parameters["DDSInitFrequency"] = 110.0;
        Parameters["DDSInitAmplitude"] = 250;

    }

    public override PatternBuilder32 GetDigitalPattern()
    {
        PatternBuilder32 p = new PatternBuilder32();
        int patternStartBeforeQ = (int)Parameters["TCLBlockStart"];
        int slowingAOMStart = (int)Parameters["BXAOMPreBunchingStartTime"] + (int)Parameters["BXAOMPreBunchingDuration"] + (int)Parameters["BXAOMFreeFlightDuration"];
        int slowingChirpStart = slowingAOMStart + (int)Parameters["BXAOMPostBunchingDuration"];
        int slowingChirpStop = slowingChirpStart + (int)Parameters["BXAOMChirpDuration"];

        p.Pulse(patternStartBeforeQ, 0, 10, "analogPatternTrigger");
        p.Pulse(
            patternStartBeforeQ,
            0,
            slowingChirpStop + 1000 + 1000 + 20000,
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
            "coolingAOM"
        );

        if ((double)Parameters["slowingONorOFF"] > 5.0)
        {
            if ((int)Parameters["BXAOMPreBunchingDuration"] > 0)
            {
                p.Pulse(
                    patternStartBeforeQ,
                    (int)Parameters["BXAOMPreBunchingStartTime"],
                    (int)Parameters["BXAOMPreBunchingDuration"],
                    "slowingAOM"
                );
            }

            p.Pulse(
                patternStartBeforeQ,
                slowingAOMStart,
                slowingChirpStop - slowingAOMStart,
                "slowingAOM"
            );

            p.Pulse(
                patternStartBeforeQ,
                (int)Parameters["SlowingPokeStart"],
                (int)Parameters["SlowingPokeDuration"],
                "slowingAOM"
            );

            p.Pulse(
                patternStartBeforeQ,
                (int)Parameters["BXAOMPreBunchingStartTime"],
                8000,
                "BXFiberEOM"
            );

            p.Pulse(
                patternStartBeforeQ,
                0,
                slowingChirpStop,
                "slowingRepumpAOM"
            );

        }

        p.Pulse(
            patternStartBeforeQ,
            (int)Parameters["SlowingPokeStart"] + (int)Parameters["OscillationDuration"],
            (int)Parameters["CameraTriggerDuration"],
            "cameraTrigger"
        );

        return p;
    }

    public override AnalogPatternBuilder GetAnalogPattern()
    {
        AnalogPatternBuilder p = new AnalogPatternBuilder((int)Parameters["PatternLength"]);
        p.AddChannel("slowingChirp");
        p.AddChannel("VCOFreqOutput");
        p.AddChannel("VCOAmplitudeOutput");
        p.AddChannel("motCoils");
        p.AddChannel("SlowingBField");

        int slowingChirpStart = (int)Parameters["BXAOMPreBunchingStartTime"] + (int)Parameters["BXAOMPreBunchingDuration"] + (int)Parameters["BXAOMFreeFlightDuration"] + (int)Parameters["BXAOMPostBunchingDuration"];
        int slowingChirpStop = slowingChirpStart + (int)Parameters["BXAOMChirpDuration"];

        if ((double)Parameters["slowingONorOFF"] > 5.0)
        {

            p.AddLinearRamp(
                "slowingChirp",
                slowingChirpStart,
                slowingChirpStop - slowingChirpStart,
                (double)Parameters["BXChirpEndValue"]
            );
            p.AddLinearRamp(
                "slowingChirp",
                slowingChirpStop + 10000,
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
            "VCOFreqOutput",
            0,
            (double)Parameters["VCOFreqOutputValue"]
        );
        p.AddAnalogValue(
            "VCOAmplitudeOutput",
            0,
            (double)Parameters["VCOAmplitudeOutputValue"]
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
