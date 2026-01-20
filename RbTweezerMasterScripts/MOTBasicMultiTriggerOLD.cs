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
        Parameters["PatternLength"] = 250000;


        // tMOT coils
        Parameters["tRbMOTccStart"] = 10;
        Parameters["tRbMOTccDurantion"] = 200000;
        Parameters["tMOTccValue"] = 0.01;
        Parameters["tTOF"] = 100;
        Parameters["tMOTExpo"] = 500;



        // tRb AOMs
        Parameters["tMOTCool"] = 3.2; // 3.85 V (=108.3 MHx)
        Parameters["tMOTRep"] = 3.0;
        //Parameters["tMOTCoolImg"] = 3.6; // 3.85 V (=108.3 MHx)
        //Parameters["tMOTRepImg"] = 3.0;


    }
    // Digital Patter
    public override PatternBuilder32 GetDigitalPattern()
    {
        int rbMOTStartTime = (int)Parameters["tRbMOTccStart"];
        int rbMOTEndTime = rbMOTStartTime + (int)Parameters["tRbMOTccDurantion"];
        int tRbMOTImaging = rbMOTEndTime + (int)Parameters["tTOF"];
        int End = (int)Parameters["PatternLength"] - 100;
        PatternBuilder32 p = new PatternBuilder32();
        // Analog Board trigger
        p.Pulse(0, 0, 10, "aoTrigger");

        // Laser
        p.Pulse(0, 0, (int)Parameters["tRbMOTccDurantion"], "tCoolSwitch");
        p.Pulse(0, 0, (int)Parameters["tRbMOTccDurantion"], "tRepSwitch");

        // Coil trigger
        p.Pulse(0, 0, (int)Parameters["tRbMOTccDurantion"], "tMOTccTrig");

        // Imaging
        p.Pulse(tRbMOTImaging, 0, (int)Parameters["tMOTExpo"], "tCoolSwitch");
        p.Pulse(tRbMOTImaging, 0, (int)Parameters["tMOTExpo"], "tRepSwitch");
        p.Pulse(tRbMOTImaging, 0, 100, "tMOTCamTrig");

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
        int tRbMOTImaging = rbMOTEndTime + (int)Parameters["tTOF"];



        AnalogPatternBuilder p = new AnalogPatternBuilder((int)Parameters["PatternLength"]);

        p.AddChannel("tMOTcc");
        p.AddChannel("tRbCoolVCO");
        p.AddChannel("tRbRepVCO");

        // Coils
        p.AddAnalogValue("tMOTcc", rbMOTStartTime, (double)Parameters["tMOTccValue"]);// MOT coils
        p.AddAnalogValue("tMOTcc", rbMOTEndTime, 0);

        // Lasers
        p.AddAnalogValue("tRbCoolVCO", rbMOTStartTime, (double)Parameters["tMOTCool"]);// Cooling Frequency
        p.AddAnalogValue("tRbRepVCO", rbMOTStartTime, (double)Parameters["tMOTRep"]);// Repump Frequency

        //p.AddAnalogValue("tRbCoolVCO", rbMOTEndTime, (double)Parameters["tMOTCoolImg"]);// Cooling Frequency for Fluo Img
        //p.AddAnalogValue("tRbRepVCO", rbMOTEndTime, (double)Parameters["tMOTRepImg"]);// Repump Frequency for Fluo Img



        return p;
    }

    /*
    public override AnalogStaticBuilder GetAnalogStatic()
    {
        AnalogStaticBuilder p = new AnalogStaticBuilder((int)Parameters["PatternLength"]);

        return p;
    }*/

}
