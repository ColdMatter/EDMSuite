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
        Parameters["MOTLoadingDuration"] = 2000;
        Parameters["MOTRetainingDuration"] = 2000;
        Parameters["MolassesDuration"] = 100;
        Parameters["V00MOTLoadingFreq"] = 4.0;
        Parameters["V00MOTRetainingFreq"] = 4.0;
        Parameters["V00MolassesFreq"] = 4.0;
        Parameters["V00MOTLoadingAmplitude"] = 0.31;
        Parameters["V00MOTRetainingAmplitude"] = 0.31;
        Parameters["V00MolassesAmplitude"] = 0.31;
        Parameters["V00AOMFinalDelay"] = 1000;

        // B Field
        Parameters["MOTCoilsStartTime"] = 0;
        Parameters["MOTLoadingMagFieldValue"] = 6.5;            // 6.5 V should be 1 A
        Parameters["MOTRetainingMagFieldValue"] = 6.5;
        Parameters["MOTCoilsOffValue"] = 0.0;

        // Camera trigger properties
        Parameters["CameraTriggerStart"] = 10;
        Parameters["CameraTriggerDuration"] = 100;

        // Switching control for iterative operations.
        // values higher than 5.0 leads to active state.
        Parameters["yagONorOFF"] = 10.0;
        Parameters["slowingONorOFF"] = 10.0;
    }

    public override PatternBuilder32 GetDigitalPattern()
    {
        PatternBuilder32 p = new PatternBuilder32();
        int patternStartBeforeQ = (int)Parameters["TCLBlockStart"];
        int slowingAOMStart = (int)Parameters["BXAOMPreBunchingStartTime"] + (int)Parameters["BXAOMPreBunchingDuration"] + (int)Parameters["BXAOMFreeFlightDuration"];
        int slowingChirpStart = slowingAOMStart + (int)Parameters["BXAOMPostBunchingDuration"];
        int slowingChirpStop = slowingChirpStart + (int)Parameters["BXAOMChirpDuration"];

        int motRetaingStart = (int)Parameters["MOTLoadingDuration"];
        int molassesStart = motRetaingStart + (int)Parameters["MOTRetainingDuration"];
        int cameraTrigger = molassesStart + (int)Parameters["CameraTriggerStart"];
        int coolingAOMoff = cameraTrigger + (int)Parameters["CameraTriggerDuration"] + (int)Parameters["V00AOMFinalDelay"];

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
            0,
            coolingAOMoff,
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
                (int)Parameters["BXAOMPreBunchingStartTime"],
                slowingChirpStop - (int)Parameters["BXAOMPreBunchingStartTime"],
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
            cameraTrigger,
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

        int slowingChirpStart = (int)Parameters["BXAOMPreBunchingStartTime"] + (int)Parameters["BXAOMPreBunchingDuration"] + (int)Parameters["BXAOMFreeFlightDuration"] + (int)Parameters["BXAOMPostBunchingDuration"];
        int slowingChirpStop = slowingChirpStart + (int)Parameters["BXAOMChirpDuration"];

        int motRetaingStart = (int)Parameters["MOTLoadingDuration"];
        int molassesStart = motRetaingStart + (int)Parameters["MOTRetainingDuration"];
        int cameraTrigger = molassesStart + (int)Parameters["CameraTriggerStart"];
        int coolingAOMoff = cameraTrigger + (int)Parameters["CameraTriggerDuration"] + (int)Parameters["V00AOMFinalDelay"];


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
                slowingChirpStop + 1000,
                1000,
                (double)Parameters["BXChirpStartValue"]
            );
        }

        p.AddAnalogValue(
            "motCoils",
            (int)Parameters["MOTCoilsStartTime"],
            (double)Parameters["MOTLoadingMagFieldValue"]
        );
        p.AddAnalogValue(
            "motCoils",
            motRetaingStart,
            (double)Parameters["MOTRetainingMagFieldValue"]
        );
        p.AddAnalogValue(
            "motCoils",
            molassesStart,
            (double)Parameters["MOTCoilsOffValue"]
        );


        p.AddAnalogValue(
            "VCOAmplitudeOutput",
            0,
            (double)Parameters["V00MOTLoadingAmplitude"]
        );
        p.AddAnalogValue(
            "VCOAmplitudeOutput",
            motRetaingStart,
            (double)Parameters["V00MOTRetainingAmplitude"]
        );
        p.AddAnalogValue(
            "VCOAmplitudeOutput",
            molassesStart,
            (double)Parameters["V00MolassesAmplitude"]
        );

        p.AddAnalogValue(
            "VCOFreqOutput",
            0,
            (double)Parameters["V00MOTLoadingFreq"]
        );
        p.AddAnalogValue(
            "VCOFreqOutput",
            motRetaingStart,
            (double)Parameters["V00MOTRetainingFreq"]
        );
        p.AddAnalogValue(
            "VCOFreqOutput",
            molassesStart,
            (double)Parameters["V00MolassesFreq"]
        );

        return p;
    }

    public override AnalogStaticBuilder GetAnalogStatic()
    {
        AnalogStaticBuilder p = new AnalogStaticBuilder((int)Parameters["PatternLength"]);

        return p;
    }
}
