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

        Parameters["CameraTriggerStart"] = 1800;
        Parameters["CameraTriggerDuration"] = 2000;


        // Fundamental slowing parameters
        double slowingMHzPerVolt = -1350.0;             // Azurlight BX laser 1V --> -1350 MHz
        double slowingRepumpMHzPerVolt = 330.0;         // Precilaser V1 laser 1V --> 330 MHz
        double slowingToRepumpFreqRatio = 0.845;        // 496.958758/564.582380

        // Requires modifications
        double slowingChirpMHzSpan = 310.0;
        int slowingDuration = 800;


        double repumpChirpSpanInVoltage = (slowingChirpMHzSpan * slowingToRepumpFreqRatio) / slowingRepumpMHzPerVolt;


        // Derived slowing parameters, try not to change
        Parameters["SlowingAOMONStartTime"] = 10;
        Parameters["SlowingChirpStartTime"] = 100;
        Parameters["SlowingAOMONDuration"] = slowingDuration + 100;
        Parameters["SlowingChirpDuration"] = slowingDuration;
        Parameters["SlowingChirpStartValue"] = 0.0;
        Parameters["SlowingChirpEndValue"] = slowingChirpMHzSpan / slowingMHzPerVolt;

        Parameters["RepumpStartValue"] = 0.0;
        Parameters["RepumpChirpStartValue"] = -0.5;
        Parameters["RepumpChirpEndValue"] = -0.5 + repumpChirpSpanInVoltage;

        // B Field
        Parameters["MOTCoilsStartTime"] = 0;
        Parameters["MOTCoilsStopTime"] = 20000;
        Parameters["MOTCoilsOnValue"] = -1.0;
        Parameters["MOTCoilsOffValue"] = 0.0;

        // Switching control for iterative operations.
        // Voltage higher than 5.0 leads to active state.
        Parameters["yagONorOFF"] = 10.0;
        Parameters["slowingONorOFF"] = 10.0;
        Parameters["slowingRepumpONorOFF"] = 10.0;
        Parameters["chirpONorOFF"] = 10.0;
        Parameters["magneticFieldONorOFF"] = 1.0;
    }

    public override PatternBuilder32 GetDigitalPattern()
    {
        PatternBuilder32 p = new PatternBuilder32();
        int patternStartBeforeQ = (int)Parameters["TCLBlockStart"];
        int repumpAOMON = patternStartBeforeQ + (int)Parameters["SlowingAOMONStartTime"];
        int repumpAOMOFF = repumpAOMON + (int)Parameters["SlowingAOMONDuration"];
        int allOFF = (int)Parameters["PatternLength"] - patternStartBeforeQ - 100;

        p.Pulse(
            patternStartBeforeQ,
            (int)Parameters["SlowingAOMONStartTime"],
            (2 * (int)Parameters["SlowingChirpDuration"]) + 1000,
            "blockTCL"
        );
        p.Pulse(
            patternStartBeforeQ,
            -(int)Parameters["FlashToQ"],
            (int)Parameters["QSwitchPulseDuration"],
            "flash"
        );
        p.Pulse(patternStartBeforeQ, 0, 10, "analogPatternTrigger");
        if ((double)Parameters["yagONorOFF"] > 5.0)
        {
            p.Pulse(patternStartBeforeQ, 0, (int)Parameters["QSwitchPulseDuration"], "q");
        }
        p.Pulse(patternStartBeforeQ, 0, allOFF, "coolingAOM");


        if ((double)Parameters["slowingONorOFF"] > 5.0)
        {
            p.Pulse(
                patternStartBeforeQ,
                (int)Parameters["SlowingAOMONStartTime"],
                (int)Parameters["SlowingAOMONDuration"],
                "slowingAOM"
            );
        }
        // p.AddEdge("slowingRepumpAOM", repumpAOMON, true);

        if ((double)Parameters["slowingRepumpONorOFF"] > 5.0)
        {
            p.AddEdge("slowingRepumpAOM", repumpAOMON, true);
            p.AddEdge("slowingRepumpAOM", repumpAOMOFF, false);
        }

        p.Pulse(
            patternStartBeforeQ,
            (int)Parameters["CameraTriggerStart"],
            (int)Parameters["CameraTriggerDuration"],
            "cameraTrigger"
        );

        return p;
    }

    public override AnalogPatternBuilder GetAnalogPattern()
    {
        AnalogPatternBuilder p = new AnalogPatternBuilder((int)Parameters["PatternLength"]);
        p.AddChannel("slowingChirp");
        p.AddChannel("slowingRepumpChirp");
        p.AddChannel("motCoils");

        if ((double)Parameters["chirpONorOFF"] > 5.0)
        {
            p.AddLinearRamp(
                "slowingChirp",
                (int)Parameters["SlowingChirpStartTime"],
                (int)Parameters["SlowingChirpDuration"],
                (double)Parameters["SlowingChirpEndValue"]
            );
            p.AddLinearRamp(
                "slowingChirp",
                (int)Parameters["SlowingChirpStartTime"] + (int)Parameters["SlowingChirpDuration"] + 200,
                100,
                (double)Parameters["SlowingChirpStartValue"]
            );

            p.AddLinearRamp(
                "slowingRepumpChirp",
                (int)Parameters["SlowingAOMONStartTime"],
                50,
                (double)Parameters["RepumpChirpStartValue"]
            );

            p.AddLinearRamp(
                "slowingRepumpChirp",
                (int)Parameters["SlowingChirpStartTime"],
                (int)Parameters["SlowingChirpDuration"],
                (double)Parameters["RepumpChirpEndValue"]
            );

            p.AddLinearRamp(
                "slowingRepumpChirp",
                (int)Parameters["SlowingChirpStartTime"] + (int)Parameters["SlowingChirpDuration"] + 50,
                50,
                (double)Parameters["RepumpStartValue"]
            );
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
