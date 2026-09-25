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
        Parameters["PatternLength"] = 100000;
        Parameters["TCLBlockStart"] = 0;


        // tMOT coils
        Parameters["tRbMOTccStart"] = 10;
        Parameters["tRbMOTccDurantion"] = 50000;
        Parameters["tMOTccValue"] = 0.18;
        Parameters["TOF"] = 300;
        Parameters["tMOTExpo"] = 100;
        Parameters["tShimXccValue"] =3.0; // 8.0
        Parameters["tShimYccValue"] = -9.0; // -5.0 
        Parameters["tShimZccValue"] = 0.5; // 1.8

        // tRb AOMs
        Parameters["tMOTCool"] = 3.4; //Cool at 2.4 - 2.6
        Parameters["tMOTRep"] = 2.6;
        Parameters["tMOTCoolImg"] = 5.0; // 3.6 - 4.0, resonance is 4.0
        Parameters["tMOTRepImg"] = 3.2; // 3.0


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
        p.Pulse(rbMOTStartTime, 0, (int)Parameters["tRbMOTccDurantion"], "tCoolSwitch");
        p.Pulse(rbMOTStartTime, 0, (int)Parameters["tRbMOTccDurantion"], "tRepSwitch");

        // Coil trigger
        p.Pulse(0, 0, (int)Parameters["tRbMOTccDurantion"], "tMOTccTrig");

        // Imaging
        p.Pulse(tRbMOTImaging, 0, (int)Parameters["tMOTExpo"], "tCoolSwitch");
        p.Pulse(tRbMOTImaging, 0, (int)Parameters["tMOTExpo"], "tRepSwitch");
        p.Pulse(tRbMOTImaging-1000, 0, 100, "tMOTCamTrig");
        p.Pulse(tRbMOTImaging, 0, 1000, "tHamCamTrig");

        p.AddEdge("tRbFluo", rbMOTStartTime, true);
        p.AddEdge("tRbFluo", tRbMOTImaging, false);
        //p.AddEdge("tRbFluo", tRbMOTImaging, true);


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

        // Coils
        p.AddAnalogValue("tMOTcc", rbMOTStartTime, (double)Parameters["tMOTccValue"]);// MOT coils
        p.AddAnalogValue("tMOTcc", rbMOTEndTime, 0);

        p.AddAnalogValue("tShimXcc", rbMOTStartTime, (double)Parameters["tShimXccValue"]);// Shim coils X
        p.AddAnalogValue("tShimXcc", tRbMOTImaging + 1000, 0);

        p.AddAnalogValue("tShimYcc", rbMOTStartTime, (double)Parameters["tShimYccValue"]);// Shim coils Y
        p.AddAnalogValue("tShimYcc", tRbMOTImaging + 1000, 0);

        p.AddAnalogValue("tShimZcc", rbMOTStartTime, (double)Parameters["tShimZccValue"]);// Shim coils Z
        p.AddAnalogValue("tShimZcc", tRbMOTImaging + 1000, 0);

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
