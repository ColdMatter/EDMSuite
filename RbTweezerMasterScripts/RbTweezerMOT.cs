using MOTMaster;
using System;
using System.Collections.Generic;

using DAQ.Pattern;
using DAQ.Analog;


public class Patterns : MOTMasterScript
/*
 * This is a test script
 * */
{
    public Patterns()
    {
        Parameters = new Dictionary<string, object>();

        // General
        Parameters["PatternLength"] = 150000;
        Parameters["TCLBlockStart"] = 0;


        // tMOT coils
        Parameters["tRbMOTccStart"] = 10;
        Parameters["tRbMOTccDurantion"] = 100000;
        Parameters["tMOTccValue"] = 0.48;
        Parameters["tMOTccSwitch"] = 5.5;
        Parameters["TOF"] = 1;
        Parameters["tMOTExpo"] = 300;
        Parameters["tShimXccValue"] = 0.0; //  9.0; // 0.0
        Parameters["tShimYccValue"] = 3.6; // -6.0; // 4.0
        Parameters["tShimZccValue"] = 0.3; // 1.2; // -0.8

        // tRb AOMs
        Parameters["tMOTCool"] = 3.2; //Cool at 2.4 - 2.6
        Parameters["tMOTRep"] = 3.0;
        Parameters["tMOTCoolImg"] = 4.0; // 3.6 - 4.0, resonance is 4.0
        Parameters["tMOTRepImg"] = 3.0; // 3.0


    }
    // Digital Patter
    public override PatternBuilder32 GetDigitalPattern()
    {
        int rbMOTStartTime = (int)Parameters["tRbMOTccStart"];
        int rbMOTEndTime = rbMOTStartTime + (int)Parameters["tRbMOTccDurantion"];
        int tRbMOTImaging = rbMOTEndTime + (int)Parameters["TOF"];
        int End = (int)Parameters["PatternLength"] - 100;
        PatternBuilder32 p = new PatternBuilder32();

        // Analog Board trigger
        p.Pulse(0, 0, 10, "aoTrigger");

        // Laser
        p.Pulse(0, 0, (int)Parameters["tRbMOTccDurantion"] - 200, "tCoolSwitch");
        p.Pulse(0, 0, (int)Parameters["tRbMOTccDurantion"] - 200, "tRepSwitch");

        // Coil trigger
        p.Pulse(0, 0, (int)Parameters["tRbMOTccDurantion"], "tMOTccTrig");

        // Imaging
        p.Pulse(tRbMOTImaging, 0, (int)Parameters["tMOTExpo"], "tCoolSwitch");
        p.Pulse(tRbMOTImaging, 0, (int)Parameters["tMOTExpo"], "tRepSwitch");

        p.Pulse(tRbMOTImaging, 0, 100, "tMOTCamTrig");
        p.Pulse(tRbMOTImaging, 0, 1000, "tHamCamTrig");

        //p.AddEdge("tRbFluo", rbMOTStartTime, true);
        //p.AddEdge("tRbFluo", tRbMOTImaging, false);
        //p.AddEdge("tRbFluo", tRbMOTImaging, true);
        p.AddEdge("tD1AOMSwitch", rbMOTStartTime, true); // D1 switch off at Start
        p.AddEdge("tD1AOMSwitch", End, false); // D1 switch off at Start

        // END
        p.AddEdge("tCoolSwitch", End, true);
        p.AddEdge("tRepSwitch", End, true);
        //p.AddEdge("tRbFluo", End, true);

        return p;
    }
    // Analog Pattern

    public override AnalogPatternBuilder GetAnalogPattern()
    {
        int rbMOTStartTime = (int)Parameters["tRbMOTccStart"];
        int rbMOTEndTime = rbMOTStartTime + (int)Parameters["tRbMOTccDurantion"];
        int tRbMOTImaging = rbMOTEndTime + (int)Parameters["TOF"];

        AnalogPatternBuilder p = new AnalogPatternBuilder((int)Parameters["PatternLength"]);

        p.AddChannel("tMOTcc");
        p.AddChannel("tShimXcc");
        p.AddChannel("tShimYcc");
        p.AddChannel("tShimZcc");
        p.AddChannel("tRbCoolVCO");
        p.AddChannel("tRbRepVCO");
        p.AddChannel("tMOTccSwitch");

        p.AddAnalogValue("tMOTccSwitch", rbMOTStartTime, 0.0);// MOT coils switch-on (RSD)
        p.AddAnalogValue("tMOTccSwitch", rbMOTEndTime, (double)Parameters["tMOTccSwitch"]);// MOT coils switch-off (RSD)

        // Coils
        p.AddAnalogValue("tMOTcc", rbMOTStartTime, (double)Parameters["tMOTccValue"]);// MOT coils
        p.AddAnalogValue("tMOTcc", rbMOTEndTime, 0);
        //p.AddLinearRamp("tMOTcc", rbMOTEndTime, (int)Parameters["tMagFieldRampDownDuration"], 0.0); // Ramp down

        p.AddAnalogValue("tShimXcc", rbMOTStartTime, (double)Parameters["tShimXccValue"]);// Shim coils X
        p.AddAnalogValue("tShimXcc", tRbMOTImaging, 0);
        //p.AddLinearRamp("tShimXcc", rbMOTEndTime, (int)Parameters["tMagFieldRampDownDuration"], 0.0); // Ramp down

        p.AddAnalogValue("tShimYcc", rbMOTStartTime, (double)Parameters["tShimYccValue"]);// Shim coils Y
        p.AddAnalogValue("tShimYcc", tRbMOTImaging, 0);
        //p.AddLinearRamp("tShimYcc", rbMOTEndTime, (int)Parameters["tMagFieldRampDownDuration"], 0.0); // Ramp down

        p.AddAnalogValue("tShimZcc", rbMOTStartTime, (double)Parameters["tShimZccValue"]);// Shim coils Z
        p.AddAnalogValue("tShimZcc", tRbMOTImaging, 0);
        //p.AddLinearRamp("tShimZcc", rbMOTEndTime, (int)Parameters["tMagFieldRampDownDuration"], 0.0); // Ramp down

        // Lasers
        p.AddAnalogValue("tRbCoolVCO", rbMOTStartTime, (double)Parameters["tMOTCool"]);// Cooling Frequency
        p.AddAnalogValue("tRbRepVCO", rbMOTStartTime, (double)Parameters["tMOTRep"]);// Repump Frequency

        p.AddAnalogValue("tRbCoolVCO", rbMOTEndTime, (double)Parameters["tMOTCoolImg"]);// Cooling Frequency for Fluo Img
        p.AddAnalogValue("tRbRepVCO", rbMOTEndTime, (double)Parameters["tMOTRepImg"]);// Repump Frequency for Fluo Img



        return p;
    }

    /*
    public override AnalogStaticBuilder GetAnalogStatic()
    {
        AnalogStaticBuilder p = new AnalogStaticBuilder((int)Parameters["PatternLength"]);

        return p;
    }*/

}
