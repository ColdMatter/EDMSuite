using MOTMaster;
using System;
using System.Collections.Generic;

using DAQ.Pattern;
using DAQ.Analog;


public class Patterns : MOTMasterScript
/*
 * This script is designed to only test the slowing.
 * The time unit in this script is in multiple of 10 micro second.
 * The other unit is volt.
 * */
{
    public Patterns()
    {
        Parameters = new Dictionary<string, object>();
        Parameters["PatternLength"] = 50000;
        Parameters["TCLBlockStart"] = 1000;
        Parameters["FlashToQ"] = 16;
        Parameters["QSwitchPulseDuration"] = 10;

        Parameters["SlowingAOMONStartTime"] = 50;
        Parameters["SlowingAOMONDuration"] = 600;
        Parameters["SlowingChirpStartTime"] = 380;
        Parameters["SlowingChirpDuration"] = 1160;
        Parameters["SlowingChirpStartValue"] = 0.0;
        Parameters["SlowingChirpEndValue"] = -1.0;

        Parameters["SlowingChirpRepumpStartTime"] = 380;
        Parameters["SlowingChirpRepumpDuration"] = 1160;
        Parameters["SlowingChirpRepumpStartValue"] = 0.0;
        Parameters["SlowingChirpRepumpEndValue"] = 1.0;
        Parameters["slowingONorOFF"] = 10.0;

    }

    public override PatternBuilder32 GetDigitalPattern()
    {
        PatternBuilder32 p = new PatternBuilder32();
        int patternStartBeforeQ = (int)Parameters["TCLBlockStart"];
        int repumpAOMON = patternStartBeforeQ + (int)Parameters["SlowingAOMONStartTime"];
        int repumpAOMOFF = repumpAOMON + (int)Parameters["SlowingAOMONDuration"] + 100;
        p.Pulse(patternStartBeforeQ, (int)Parameters["SlowingChirpStartTime"], (2 * (int)Parameters["SlowingChirpDuration"]) + 200, "blockTCL");
        p.Pulse(patternStartBeforeQ, -(int)Parameters["FlashToQ"], (int)Parameters["QSwitchPulseDuration"], "flash");
        p.Pulse(patternStartBeforeQ, 0, 10, "analogPatternTrigger");
        p.Pulse(patternStartBeforeQ, 0, (int)Parameters["QSwitchPulseDuration"], "q");
        if ((double)Parameters["slowingONorOFF"] > 5.0) {
            p.Pulse(patternStartBeforeQ, (int)Parameters["SlowingAOMONStartTime"], (int)Parameters["SlowingAOMONDuration"], "slowingAOM");
        }
        p.Pulse(patternStartBeforeQ, 0, (int)Parameters["PatternLength"]-patternStartBeforeQ-100, "coolingAOM");
        p.AddEdge("slowingRepumpAOM", patternStartBeforeQ, true);
        p.AddEdge("slowingRepumpAOM", repumpAOMON, false);
        p.AddEdge("slowingRepumpAOM", repumpAOMOFF, true);

        return p;
    }

    public override AnalogPatternBuilder GetAnalogPattern()
    {
        AnalogPatternBuilder p = new AnalogPatternBuilder((int)Parameters["PatternLength"]);
        p.AddChannel("slowingChirp");
        p.AddChannel("slowingRepumpChirp");

        
        /*
        p.AddPolynomialRamp(
            "slowingChirp",
            (int)Parameters["SlowingChirpStartTime"],
            (int)Parameters["SlowingChirpStartTime"] + (int)Parameters["SlowingChirpDuration"],
            (double)Parameters["SlowingChirpEndValue"],
                1.0,                    // Parameters["SlowingChirpUpperThreshold"]
                -1.5,                   // Parameters["SlowingChirpLowerThreshold"]
                1.0,                    // (double)parameters["weight1"],
                -0.5,                   // (double)parameters["weight2"],
                1.0 / 6.0,              // (double)parameters["weight3"],
                -1.0 / 24.0             // (double)parameters["weight4"]
        );
        p.AddLinearRamp(
            "slowingChirp",
            (int)Parameters["SlowingChirpStartTime"] + (int)Parameters["SlowingChirpDuration"] + 200,
            (int)Parameters["SlowingChirpDuration"],
            (double)Parameters["SlowingChirpStartValue"]
        );

        
        p.AddPolynomialRamp(
            "slowingRepumpChirp",
            (int)Parameters["SlowingChirpRepumpStartTime"],
            (int)Parameters["SlowingChirpRepumpStartTime"] + (int)Parameters["SlowingChirpRepumpDuration"],
            (double)Parameters["SlowingChirpRepumpEndValue"],
            1.0,                    // Parameters["SlowingChirpUpperThreshold"]
            -1.0,                   // Parameters["SlowingChirpLowerThreshold"]
            1.0,                    // (double)parameters["weight1"],
            -0.5,                   // (double)parameters["weight2"],
            1.0 / 6.0,              // (double)parameters["weight3"],
            -1.0 / 24.0             // (double)parameters["weight4"]
        );           
        p.AddLinearRamp(
            "slowingRepumpChirp",
            (int)Parameters["SlowingChirpRepumpStartTime"] + (int)Parameters["SlowingChirpRepumpDuration"] + 200,
            (int)Parameters["SlowingChirpRepumpDuration"],
            (double)Parameters["SlowingChirpRepumpStartValue"]
        );
        */
        
        return p;
    }

    public override AnalogStaticBuilder GetAnalogStatic()
    {
        AnalogStaticBuilder p = new AnalogStaticBuilder((int)Parameters["PatternLength"]);
 
        return p;
    }
}
