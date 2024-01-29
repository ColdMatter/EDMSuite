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
        Parameters["CameraTriggerDuration"] = 1000;

        // Fundamental slowing calibration parameters
        double slowingMHzPerVolt = -1350.0;             // Azurlight BX laser 1V --> -1350 MHz

        // Requires modifications
        Parameters["SlowingChirpMHzSpan"] = 220.0;     // 300.0 MHz is standard
        Parameters["SlowingDuration"] = 1000;
        Parameters["SlowingHoldTime"] = 250;

        // AOM switch on timings
        Parameters["V00AOMONStartTime"] = 0;
        Parameters["V00AOMONDuration"] = 17000;
        Parameters["BXAOMONStartTime"] = 200;
        Parameters["BXAOMONDuration"] = (int)Parameters["SlowingDuration"] + (int)Parameters["SlowingHoldTime"];
        Parameters["V10AOMONStartTime"] = 2;
        Parameters["V10AOMONDuration"] = (int)Parameters["SlowingDuration"] + (int)Parameters["SlowingHoldTime"] + 20;

        // Slowing chirp parameters
        Parameters["BXChirpStartTime"] = (int)Parameters["SlowingHoldTime"];
        Parameters["BXChirpDuration"] = (int)Parameters["SlowingDuration"];
        Parameters["BXChirpStartValue"] = 0.0;
        Parameters["BXChirpEndValue"] = (double)Parameters["SlowingChirpMHzSpan"] / slowingMHzPerVolt;

        // B Field
        Parameters["MOTCoilsStartTime"] = 0;
        Parameters["MOTCoilsStopTime"] = 17000;
        Parameters["MOTCoilsOnValue"] = -0.4;   //0.4 for 1A
        Parameters["MOTCoilsOffValue"] = 0.0;

        // Switching control for iterative operations.
        // values higher than 5.0 leads to active state.
        Parameters["yagONorOFF"] = 10.0;
        Parameters["slowingONorOFF"] = 10.0;
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
        if ((double)Parameters["slowingONorOFF"] > 5.0)
        {
            p.Pulse(
                patternStartBeforeQ,
                (int)Parameters["BXAOMONStartTime"],
                (int)Parameters["BXAOMONDuration"],
                "slowingAOM"
            );
            p.Pulse(
                patternStartBeforeQ,
                (int)Parameters["V10AOMONStartTime"],
                (int)Parameters["V10AOMONDuration"],
                "slowingRepumpAOM"
            );
        }

        for(int i=0; i<(int)Parameters["NumberOfTriggers"]; i++)
        {
            p.Pulse(
                patternStartBeforeQ,
                (int)Parameters["CameraTriggerStart"],
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

        p.AddLinearRamp(
            "slowingChirp",
            (int)Parameters["BXChirpStartTime"],
            (int)Parameters["BXChirpDuration"],
            (double)Parameters["BXChirpEndValue"]
        );
        p.AddLinearRamp(
            "slowingChirp",
            (int)Parameters["BXChirpStartTime"] + (int)Parameters["BXChirpDuration"] + 200,
            (int)Parameters["BXChirpDuration"],
            (double)Parameters["BXChirpStartValue"]
        );
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

        return p;
    }

    public override AnalogStaticBuilder GetAnalogStatic()
    {
        AnalogStaticBuilder p = new AnalogStaticBuilder((int)Parameters["PatternLength"]);

        return p;
    }
}
