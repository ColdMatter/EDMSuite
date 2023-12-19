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
        Parameters["TCLBlockStart"] = 1000;
        Parameters["FlashToQ"] = 16;
        Parameters["QSwitchPulseDuration"] = 10;

        // Camera trigger properties
        Parameters["CameraTriggerStart"] = 2000;
        Parameters["CameraTriggerDuration"] = 2000;
        Parameters["CameraTriggerInterval"] = 3000;
        Parameters["NumberOfTriggers"] = 1;


        // Fundamental slowing calibration parameters
        double slowingMHzPerVolt = -1350.0;             // Azurlight BX laser 1V --> -1350 MHz
        double slowingRepumpMHzPerVolt = 330.0;         // Precilaser V1 laser 1V --> 330 MHz
        double slowingToRepumpFreqRatio = 0.845;        // 496.958758/564.582380

        // Requires modifications
        Parameters["SlowingChirpMHzSpan"] = 310.0;
        Parameters["SlowingDuration"] = 1200;
        Parameters["SlowingHoldTime"] = 200;
        double repumpChirpSpanInVoltage = ((double)Parameters["SlowingChirpMHzSpan"] * slowingToRepumpFreqRatio) / slowingRepumpMHzPerVolt;

        // AOM switch on timings
        Parameters["V00AOMONStartTime"] = 0;
        Parameters["V00AOMONDuration"] = 12500;
        Parameters["BXAOMONStartTime"] = 150;
        Parameters["BXAOMONDuration"] = (int)Parameters["SlowingDuration"] + (int)Parameters["SlowingHoldTime"];
        Parameters["V10AOMONStartTime"] = 150;
        Parameters["V10AOMONDuration"] = 12500;

        // Slowing chirp parameters
        Parameters["BXChirpStartTime"] = (int)Parameters["SlowingHoldTime"];
        Parameters["BXChirpDuration"] = (int)Parameters["SlowingDuration"];
        Parameters["BXChirpStartValue"] = 0.0;
        Parameters["BXChirpEndValue"] = (double)Parameters["SlowingChirpMHzSpan"] / slowingMHzPerVolt;

        Parameters["V10ChirpStartTime"] = (int)Parameters["SlowingHoldTime"];
        Parameters["V10ChirpDuration"] = (int)Parameters["SlowingDuration"];
        Parameters["V10ChirpStartValue"] = -0.75; // -0.78, -0.63 (50MHz offset)
        Parameters["V10ChirpEndValue"] = -0.75 + repumpChirpSpanInVoltage;

        // B Field
        Parameters["MOTCoilsStartTime"] = 1550;
        Parameters["MOTCoilsStopTime"] = 9000;
        Parameters["MOTCoilsOnValue"] = -0.6;
        Parameters["MOTCoilsOffValue"] = 0.0;

        // Switching control for iterative operations.
        // values higher than 5.0 leads to active state.
        Parameters["yagONorOFF"] = 10.0;
        Parameters["slowingONorOFF"] = 1.0;
        Parameters["chirpONorOFF"] = 1.0;
        Parameters["slowingRepumpJumpONorOFF"] = 10.0;
        Parameters["magneticFieldONorOFF"] = 10.0;
    }

    public override PatternBuilder32 GetDigitalPattern()
    {
        PatternBuilder32 p = new PatternBuilder32();
        int patternStartBeforeQ = (int)Parameters["TCLBlockStart"];

        p.Pulse(patternStartBeforeQ, 0, 10, "analogPatternTrigger");
        p.Pulse(
            patternStartBeforeQ,
            (int)Parameters["BXAOMONStartTime"],
            (int)Parameters["BXChirpDuration"] + (int)Parameters["BXChirpStartTime"] + 20000,
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
        p.Pulse(
            patternStartBeforeQ,
            (int)Parameters["V10AOMONStartTime"],
            (int)Parameters["V10AOMONDuration"],
            "slowingRepumpAOM"
        );

        if ((double)Parameters["slowingONorOFF"] > 5.0)
        {
            p.Pulse(
                patternStartBeforeQ,
                (int)Parameters["BXAOMONStartTime"],
                (int)Parameters["BXAOMONDuration"],
                "slowingAOM"
            );
        }
        
        for (int t = 0; t < (int)Parameters["NumberOfTriggers"]; t++) {
            p.Pulse(
                patternStartBeforeQ,
                (int)Parameters["CameraTriggerStart"] + (t * (int)Parameters["CameraTriggerInterval"]),
                (int)Parameters["CameraTriggerDuration"],
                "cameraTrigger"
            );
        }
        
        return p;
    }

    public override AnalogPatternBuilder GetAnalogPattern()
    {
        AnalogPatternBuilder p = new AnalogPatternBuilder((int)Parameters["PatternLength"]);
        p.AddChannel("slowingChirp");
        p.AddChannel("slowingRepumpChirp");
        p.AddChannel("motCoils");

        if ((double)Parameters["slowingRepumpJumpONorOFF"] > 5.0)
        {
            p.AddLinearRamp(
                "slowingRepumpChirp",
                (int)Parameters["V10AOMONStartTime"],
                50,
                (double)Parameters["V10ChirpStartValue"]
            );
            if ((double)Parameters["chirpONorOFF"] < 5.0) {
                p.AddLinearRamp(
                    "slowingRepumpChirp",
                    (int)Parameters["V10ChirpStartTime"] + (int)Parameters["V10ChirpDuration"] + 50,
                    50,
                    0.0
                );
            }  
        }
        
        if ((double)Parameters["chirpONorOFF"] > 5.0)
        {
            p.AddLinearRamp(
                "slowingChirp",
                (int)Parameters["BXChirpStartTime"],
                (int)Parameters["BXChirpDuration"],
                (double)Parameters["BXChirpEndValue"]
            );
            p.AddLinearRamp(
                "slowingChirp",
                (int)Parameters["BXChirpStartTime"] + (int)Parameters["BXChirpDuration"] + 200,
                100,
                (double)Parameters["BXChirpStartValue"]
            );
            if ((double)Parameters["slowingRepumpJumpONorOFF"] > 5.0)
            {
                p.AddLinearRamp(
                    "slowingRepumpChirp",
                    (int)Parameters["V10ChirpStartTime"],
                    (int)Parameters["V10ChirpDuration"],
                    (double)Parameters["V10ChirpEndValue"]
                );
            }

        }

        if ((double)Parameters["magneticFieldONorOFF"] > 5.0)
        {
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
        }
        
        return p;
    }

    public override AnalogStaticBuilder GetAnalogStatic()
    {
        AnalogStaticBuilder p = new AnalogStaticBuilder((int)Parameters["PatternLength"]);

        return p;
    }
}
