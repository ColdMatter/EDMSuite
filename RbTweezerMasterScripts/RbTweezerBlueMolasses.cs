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
        Parameters["PatternLength"] = 200000;
        Parameters["TCLBlockStart"] = 0;
        Parameters["tScanCount"] = 0.26;

        // tMOT coils
        Parameters["tMOTccValue"] = 0.48;
        Parameters["tBlueMolassesccValue"] = 0.48;
        Parameters["tMOTccSwitch"] = 5.5;
        Parameters["tShimXccValue"] = 8.8; //  9.0; // 0.0
        Parameters["tShimYccValue"] = 3.8;//-3.0; // -6.0; // 4.0
        Parameters["tShimZccValue"] = 0.3; // 1.2; // -0.8

        // Shim Coils Molasses
        Parameters["tMolassesShimXccValue"] = 8.8;//0.0// 8.8 (flipped coils)
        Parameters["tMolassesShimYccValue"] = 3.8;//-1.5 // 3.0
        Parameters["tMolassesShimZccValue"] = 0.3;//1.5 //0.3

        // Timing
        Parameters["TOF1"] = 200;   
        Parameters["TOF"] = 100;
        Parameters["tMOTExpo"] = 300;
        Parameters["tRbMOTStart"] = 10;
        Parameters["tRbMOTDuration"] = 130000;
        Parameters["tRbBlueMolassesDuration"] = 500;

        // tRb AOMs
        Parameters["tMOTCool"] = 3.2; //Cool at 2.4 - 2.6
        Parameters["tMOTRep"] = 1.6;
        Parameters["tMOTCoolImg"] = 3.2; // 3.6 - 4.0, resonance is 4.0
        Parameters["tMOTRepImg"] = 3.0; // 3.0
        Parameters["tMOTCoolMol"] = 1.0;
        Parameters["tMOTRepMol"] = 1.9;

        Parameters["tBlueMolassesVCOCool"] = 3.0;
        Parameters["tBlueMolassesVCOCoolRamp"] = 6.0;
        Parameters["tBlueMolassesVCOImg"] = 5.0;
        Parameters["tBlueMolassesVVA"] = 10.0;
        Parameters["tBlueMolassesVVARamp"] = 4.0;


    }
    // Digital Patter
    public override PatternBuilder32 GetDigitalPattern()
    {
        int rbMOTStart = (int)Parameters["tRbMOTStart"];
        int TOF1Start = rbMOTStart + (int)Parameters["tRbMOTDuration"];
        int rbBlueMolassesStart = TOF1Start + (int)Parameters["TOF1"];
        int TOFStart = rbBlueMolassesStart + (int)Parameters["tRbBlueMolassesDuration"];
        int imagingStart = TOFStart + (int)Parameters["TOF"];
        int End = (int)Parameters["PatternLength"] - 100;
        PatternBuilder32 p = new PatternBuilder32();

        // Analog Board trigger
        p.Pulse(0, 0, 10, "aoTrigger");

        // Laser
        p.Pulse(rbMOTStart, 0, (int)Parameters["tRbMOTDuration"], "tCoolSwitch"); // Red MOT Coolng Pulse
        p.Pulse(rbMOTStart, 0, (int)Parameters["tRbMOTDuration"], "tRepSwitch"); // Red MOT Repump Pulse

        p.AddEdge("tD1AOMSwitch", rbMOTStart, true); // D1 switch off at Start

        // Molasses Lasers

       //p.Pulse(rbBlueMolassesStart, 0, (int)Parameters["tRbBlueMolassesDuration"], "tCoolSwitch"); //D2 Molasses Cooling
      // p.Pulse(rbBlueMolassesStart, 0, (int)Parameters["tRbBlueMolassesDuration"], "tRepSwitch"); //D2 Molasses Repump

       p.AddEdge("tD1AOMSwitch", rbBlueMolassesStart, false);
       p.AddEdge("tD1AOMSwitch", TOFStart, true);

        // Coil trigger
        p.Pulse(rbMOTStart, 0, (int)Parameters["tRbMOTDuration"], "tMOTccTrig");

        // Imaging
        p.Pulse(imagingStart, 0, (int)Parameters["tMOTExpo"], "tCoolSwitch"); // Red MOT imaging 
        p.Pulse(imagingStart, 0, (int)Parameters["tMOTExpo"], "tRepSwitch"); // Red MOT imaging

        //p.AddEdge("tD1AOMSwitch", imagingStart, false);
        //p.AddEdge("tD1AOMSwitch", imagingStart + (int)Parameters["tMOTExpo"], true);

        p.Pulse(imagingStart, 0, 100, "tMOTCamTrig");
        p.Pulse(imagingStart, 0, 1000, "tHamCamTrig");

        // END
        p.AddEdge("tCoolSwitch", End, true);
        p.AddEdge("tRepSwitch", End, true);
        p.AddEdge("tD1AOMSwitch", End, false);

        return p;
    }
    // Analog Pattern

    public override AnalogPatternBuilder GetAnalogPattern()
    {
        int rbMOTStart = (int)Parameters["tRbMOTStart"];
        int TOF1Start = rbMOTStart + (int)Parameters["tRbMOTDuration"];
        int rbBlueMolassesStart = TOF1Start + (int)Parameters["TOF1"];
        int TOFStart = rbBlueMolassesStart + (int)Parameters["tRbBlueMolassesDuration"];
        int imagingStart = TOFStart + (int)Parameters["TOF"];
        int End = (int)Parameters["PatternLength"] - 100;

        AnalogPatternBuilder p = new AnalogPatternBuilder((int)Parameters["PatternLength"]);

        p.AddChannel("tMOTcc");
        p.AddChannel("tShimXcc");
        p.AddChannel("tShimYcc");
        p.AddChannel("tShimZcc");
        p.AddChannel("tRbCoolVCO");
        p.AddChannel("tRbRepVCO");
        p.AddChannel("tRbD1VCO");
        p.AddChannel("tRbD1VVA");
        p.AddChannel("tMOTccSwitch");

        // Coils


        p.AddAnalogValue("tMOTccSwitch", rbMOTStart, 0.0);// MOT coils switch-on (RSD)
        p.AddAnalogValue("tMOTccSwitch", TOF1Start+200, (double)Parameters["tMOTccSwitch"]);// MOT coils switch-off (RSD)
        
        p.AddAnalogValue("tMOTcc", rbMOTStart, (double)Parameters["tMOTccValue"]);// MOT coils
        //p.AddLinearRamp("tMOTcc", TOF1Start, 200, 0.0); // Ramp down
        p.AddAnalogValue("tMOTcc", TOF1Start, 0.000);

        p.AddAnalogValue("tShimXcc", rbMOTStart, (double)Parameters["tShimXccValue"]);// Shim coils X
        p.AddAnalogValue("tShimXcc", TOF1Start, (double)Parameters["tMolassesShimXccValue"]);// Shim coils X - Molasses
        p.AddAnalogValue("tShimXcc", End, 0.0);

        //p.AddAnalogValue("tShimYcc", rbMOTStart, (double)Parameters["tMolassesShimYccValue"]);// Shim coils Y
        p.AddAnalogValue("tShimYcc", rbMOTStart, (double)Parameters["tShimYccValue"]);// Shim coils Y
        p.AddAnalogValue("tShimYcc", TOF1Start, (double)Parameters["tMolassesShimYccValue"]);// Shim coils Y - Molasses
        p.AddAnalogValue("tShimYcc", End, 0.0);
        
        //p.AddAnalogValue("tShimZcc", rbMOTStart, (double)Parameters["tMolassesShimZccValue"]);// Shim coils Z
        p.AddAnalogValue("tShimZcc", rbMOTStart, (double)Parameters["tShimZccValue"]);// Shim coils Z
        p.AddAnalogValue("tShimZcc", TOF1Start, (double)Parameters["tMolassesShimZccValue"]);// Shim coils Z - Molasses
        p.AddAnalogValue("tShimZcc", End, 0.0);

        // Lasers
        p.AddAnalogValue("tRbCoolVCO", rbMOTStart, (double)Parameters["tMOTCool"]);// Cooling Frequency
        p.AddAnalogValue("tRbRepVCO", rbMOTStart, (double)Parameters["tMOTRep"]);// Repump Frequency

        // Molasses Laser VCOs
        //p.AddAnalogValue("tRbCoolVCO", TOF1Start, (double)Parameters["tMOTCoolMol"]);// D2 Molasses Cooling Frequency
        //p.AddAnalogValue("tRbRepVCO", TOF1Start, (double)Parameters["tMOTRepMol"]);// D2 MolassesRepump Frequency

        p.AddAnalogValue("tRbCoolVCO", TOFStart, (double)Parameters["tMOTCoolImg"]);// Cooling Frequency for Fluo Img
        p.AddAnalogValue("tRbRepVCO", TOFStart, (double)Parameters["tMOTRepImg"]);// Repump Frequency for Fluo Img

        p.AddAnalogValue("tRbD1VCO", rbMOTStart, (double)Parameters["tBlueMolassesVCOCool"]);// D1 AOM VCO Cooling
        p.AddLinearRamp("tRbD1VCO", rbBlueMolassesStart, (int)Parameters["tRbBlueMolassesDuration"], (double)Parameters["tBlueMolassesVCOCoolRamp"]); // D1 AOM VCO Ramp down
        p.AddAnalogValue("tRbD1VCO", End, (double)Parameters["tBlueMolassesVCOCool"]);// D1 AOM VCO Cooling
        //p.AddAnalogValue("tRbD1VCO", TOFStart, (double)Parameters["tBlueMolassesVCOImg"]);// D1 AOM VCO Cooling

        p.AddAnalogValue("tRbD1VVA", rbMOTStart, (double)Parameters["tBlueMolassesVVA"]);// D1 AOM VVA
        p.AddLinearRamp("tRbD1VVA", rbBlueMolassesStart, (int)Parameters["tRbBlueMolassesDuration"], (double)Parameters["tBlueMolassesVVARamp"]); // D1 AOM VVA Ramp down
        p.AddAnalogValue("tRbD1VVA", End, (double)Parameters["tBlueMolassesVVA"]);// D1 AOM VVA

        return p;
    }

    /*
    public override AnalogStaticBuilder GetAnalogStatic()
    {
        AnalogStaticBuilder p = new AnalogStaticBuilder((int)Parameters["PatternLength"]);

        return p;
    }*/

}
