using MOTMaster;
using System;
using System.Collections.Generic;

using DAQ.Pattern;
using DAQ.Analog;


public class Patterns : MOTMasterScript

{
    public Patterns()
    {
        Parameters = new Dictionary<string, object>();
        Parameters["PatternLength"] = 50000;
        Parameters["TCLBlockStart"] = 10000;
        Parameters["FlashToQ"] = 16;
        Parameters["QSwitchPulseDuration"] = 10;

        // Slowing parameters
        //Parameters["BXAOMFreeFlightDuration"] = 450;
        //Parameters["BXAOMPostBunchingDuration"] = 100;
        //Parameters["BXAOMChirpDuration"] = 550;
        //Parameters["BXChirpStartValue"] = 0.0;
        //Parameters["SlowingMHzPerVolt"] = -1350.0;             // Azurlight BX laser 1V --> -1350 MHz
        //Parameters["SlowingChirpMHzSpan"] = 360.0;             // 564.582580 THz is the resonance with -100 MHz AOM
        Parameters["SlowingCoilFieldValue"] = 5.0;
        Parameters["SlowingCoilTurnOffTime"] = 2000;
        Parameters["SlowingBeamStartTime"] = 200;
        Parameters["SlowingBeamDuration"] = 10;

        // Switching control for iterative operations.
        // values higher than 5.0 leads to active state.
        Parameters["yagONorOFF"] = 10.0;
        Parameters["slowingONorOFF"] = 10.0;

    }

    public override PatternBuilder32 GetDigitalPattern()
    {
        PatternBuilder32 p = new PatternBuilder32();
        int patternStartBeforeQ = (int)Parameters["TCLBlockStart"];
        int slowingAOMStart = (int)Parameters["SlowingBeamStartTime"];
        //int slowingAOMStart = (int)Parameters["BXAOMFreeFlightDuration"];
        //int slowingChirpStart = slowingAOMStart + (int)Parameters["BXAOMPostBunchingDuration"];
        //int slowingChirpStop = slowingChirpStart + (int)Parameters["BXAOMChirpDuration"];

        p.Pulse(patternStartBeforeQ, 0, 10, "analogPatternTrigger");

        // For blocking wavemeter lock
        //p.Pulse(
        //    patternStartBeforeQ,
        //    0,
        //    25000,
        //    "blockTCL"
        //);

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

        if ((double)Parameters["slowingONorOFF"] > 5.0)
        {

            p.Pulse(
                patternStartBeforeQ,
                slowingAOMStart,
                (int)Parameters["SlowingBeamDuration"],
                "BXAOM"
            );

            p.Pulse(
                patternStartBeforeQ,
                slowingAOMStart,
                10000,
                "BXSidebands"
            );

            //p.Pulse(
            //    patternStartBeforeQ,
            //    slowingAOMStart,
            //    (int)Parameters["SlowingBeamDuration"],
            //    "RepumpAOM"
            //);

            //p.Pulse(
            //    patternStartBeforeQ,
            //    0,
            //    10000,
            //    "RepumpBroadening"
            //);


        }

        p.Pulse(
            patternStartBeforeQ,
            0,
            10000,
            "V00R0AOM"
        );

        p.Pulse(
            patternStartBeforeQ,
            0,
            10000,
            "V00R1plusAOM"
        );

        return p;
    }

    public override AnalogPatternBuilder GetAnalogPattern()
    {
        AnalogPatternBuilder p = new AnalogPatternBuilder((int)Parameters["PatternLength"]);
        
        p.AddChannel("motCoils");
        p.AddChannel("SlowingBField"); 
        p.AddChannel("V00R0AOMVCOFreq");
        p.AddChannel("V00R0AOMVCOAmp");
        p.AddChannel("V00R1plusAOMAmp");

        //int slowingChirpStart = (int)Parameters["BXAOMFreeFlightDuration"] + (int)Parameters["BXAOMPostBunchingDuration"];
        //int slowingChirpStop = slowingChirpStart + (int)Parameters["BXAOMChirpDuration"];

        //if ((double)Parameters["slowingONorOFF"] > 5.0)
        //{

        //    p.AddLinearRamp(
        //        "BXChirp",
        //        slowingChirpStart,
        //        slowingChirpStop - slowingChirpStart,
        //        (double)Parameters["SlowingChirpMHzSpan"] / (double)Parameters["SlowingMHzPerVolt"]
        //    );
        //    p.AddLinearRamp(
        //        "BXChirp",
        //        slowingChirpStop + 500,
        //        1000,
        //        (double)Parameters["BXChirpStartValue"]
        //    );
        //}

        p.AddAnalogValue(
            "motCoils",
            0,
            0.0
        );

        p.AddAnalogValue(
            "SlowingBField",
            0,
            (double)Parameters["SlowingCoilFieldValue"]
        );

        p.AddAnalogValue(
            "SlowingBField",
            300,
            5.0
        );

        p.AddAnalogValue(
            "SlowingBField",
            (int)Parameters["SlowingCoilTurnOffTime"],
            0.0
        );

        p.AddAnalogValue(
            "V00R0AOMVCOFreq",
            0,
            3.1
        );

        p.AddAnalogValue(
            "V00R0AOMVCOAmp",
            0,
            0.34
        );

        p.AddAnalogValue(
            "V00R1plusAOMAmp",
            0,
            0.74
        );

        return p;
    }

    public override AnalogStaticBuilder GetAnalogStatic()
    {
        AnalogStaticBuilder p = new AnalogStaticBuilder((int)Parameters["PatternLength"]);

        return p;
    }
}
