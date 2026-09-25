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
        Parameters["TCLBlockStart"] = 0;
        Parameters["tScanCount"] = 682;

        // tMOT coils
        Parameters["tMOTccValue"] = 0.48;
        Parameters["tBlueMOTccValue"] = 0.48;
        Parameters["tShimXccValue"] = 0.0; //  9.0; // 0.0
        Parameters["tShimYccValue"] = 0.0; // -6.0; // 4.0
        Parameters["tShimZccValue"] = 0.0; // 1.2; // -0.8

        // Shim Coils Molasses
        Parameters["tMolassesShimXccValue"] = 0.0;//-1.6
        Parameters["tMolassesShimYccValue"] = 0.0;//-0.6
        Parameters["tMolassesShimZccValue"] = 0.0;//0.7

        // Timing
        Parameters["TOF1"] = 200;   
        Parameters["TOF"] = 100;
        Parameters["tMOTExpo"] = 300;
        Parameters["tRbMOTStart"] = 10;
        Parameters["tRbMOTDuration"] = 100000;
        Parameters["tRbBlueMOTDuration"] = 100000;
        Parameters["tRbBlueMolassesDuration"] = 1;

        // tRb AOMs
        Parameters["tMOTCool"] = 3.2; //Cool at 2.4 - 2.6
        Parameters["tMOTRep"] = 3.0;
        Parameters["tMOTCoolImg"] = 2.5; // 3.6 - 4.0, resonance is 4.0
        Parameters["tMOTRepImg"] = 3.0; // 3.0
        Parameters["tMOTCoolMol"] = 3.2;
        Parameters["tMOTRepMol"] = 3.0;

        Parameters["tBlueMolassesVCOCool"] = 5.0;
        Parameters["tBlueMolassesVCOImg"] = 5.0;
        Parameters["tBlueMolassesVVA"] = 10.0;
        

    }
    // Digital Patter
    public override PatternBuilder32 GetDigitalPattern()
    {
        int rbMOTStart = (int)Parameters["tRbMOTStart"];
        int rbBlueMOTStart = rbMOTStart + (int)Parameters["tRbMOTDuration"];
        int TOF1Start = rbBlueMOTStart + (int)Parameters["tRbBlueMOTDuration"];
        int rbBlueMolassesStart = TOF1Start + (int)Parameters["TOF1"];
        int TOFStart = rbBlueMolassesStart + (int)Parameters["tRbBlueMolassesDuration"];
        int imagingStart = TOFStart + (int)Parameters["TOF"];
        int End = (int)Parameters["PatternLength"] - 100;
        PatternBuilder32 p = new PatternBuilder32();

        // Analog Board trigger
        p.Pulse(0, 0, 10, "aoTrigger");

        // MOT Laser

        p.Pulse(rbMOTStart, 0, (int)Parameters["tRbMOTDuration"] , "tCoolSwitch"); // Red MOT Coolng Pulse //+ (int)Parameters["tRbBlueMOTDuration"]
        p.Pulse(rbMOTStart, 0, (int)Parameters["tRbMOTDuration"] , "tRepSwitch"); // Red MOT Repump Pulse



        p.AddEdge("tD1AOMSwitch", rbMOTStart, true); // D1 switch off at Start

       p.AddEdge("tD1AOMSwitch", rbBlueMOTStart, false);
       p.AddEdge("tD1AOMSwitch", TOF1Start, true);



        // Molasses Lasers


        //p.Pulse(rbBlueMolassesStart, 0, (int)Parameters["tRbBlueMolassesDuration"], "tCoolSwitch"); //D2 Molasses Cooling
        //p.Pulse(rbBlueMolassesStart, 0, (int)Parameters["tRbBlueMolassesDuration"], "tRepSwitch"); //D2 Molasses Repump

        //p.AddEdge("tD1AOMSwitch", rbBlueMolassesStart, false);
        //p.AddEdge("tD1AOMSwitch", TOFStart, true);

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
        int rbBlueMOTStart = rbMOTStart + (int)Parameters["tRbMOTDuration"];
        int TOF1Start = rbBlueMOTStart + (int)Parameters["tRbBlueMOTDuration"];
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

        // Coils
        p.AddAnalogValue("tMOTcc", rbMOTStart, (double)Parameters["tMOTccValue"]);// MOT coils
        p.AddAnalogValue("tMOTcc", rbBlueMOTStart, (double)Parameters["tBlueMOTccValue"]);// MOT coils
       //p.AddLinearRamp("tMOTcc", rbBlueMOTStart+200, 10000, 0.48); // Ramp down
        p.AddAnalogValue("tMOTcc", TOF1Start, 0.00);

        p.AddAnalogValue("tShimXcc", rbMOTStart, (double)Parameters["tShimXccValue"]);// Shim coils X
        p.AddAnalogValue("tShimXcc", TOF1Start, (double)Parameters["tMolassesShimXccValue"]);// Shim coils X - Molasses
        p.AddAnalogValue("tShimXcc", End, 0.0);

        p.AddAnalogValue("tShimYcc", rbMOTStart, (double)Parameters["tShimYccValue"]);// Shim coils Y
        p.AddAnalogValue("tShimYcc", TOF1Start, (double)Parameters["tMolassesShimYccValue"]);// Shim coils X - Molasses
        p.AddAnalogValue("tShimYcc", End, 0.0);

        p.AddAnalogValue("tShimZcc", rbMOTStart, (double)Parameters["tShimZccValue"]);// Shim coils Z
        p.AddAnalogValue("tShimZcc", TOF1Start, (double)Parameters["tMolassesShimZccValue"]);// Shim coils X - Molasses
        p.AddAnalogValue("tShimZcc", End, 0.0);

        // Lasers
        p.AddAnalogValue("tRbCoolVCO", rbMOTStart, (double)Parameters["tMOTCool"]);// Cooling Frequency
        p.AddAnalogValue("tRbRepVCO", rbMOTStart, (double)Parameters["tMOTRep"]);// Repump Frequency

        // Molasses Laser VCOs

        p.AddAnalogValue("tRbCoolVCO", rbBlueMOTStart, (double)Parameters["tMOTCoolMol"]);// D2 Molasses Cooling Frequency
        p.AddAnalogValue("tRbRepVCO", rbBlueMOTStart, (double)Parameters["tMOTRepMol"]);// D2 MolassesRepump Frequency
        //p.AddAnalogValue("tRbCoolVCO", TOF1Start, (double)Parameters["tMOTCoolMol"]);// D2 Molasses Cooling Frequency
        //p.AddAnalogValue("tRbRepVCO", TOF1Start, (double)Parameters["tMOTRepMol"]);// D2 MolassesRepump Frequency

        p.AddAnalogValue("tRbCoolVCO", TOFStart, (double)Parameters["tMOTCoolImg"]);// Cooling Frequency for Fluo Img
        p.AddAnalogValue("tRbRepVCO", TOFStart, (double)Parameters["tMOTRepImg"]);// Repump Frequency for Fluo Img

        p.AddAnalogValue("tRbD1VCO", rbMOTStart, (double)Parameters["tBlueMolassesVCOCool"]);// D1 AOM VCO Cooling
        //p.AddAnalogValue("tRbD1VCO", TOFStart, (double)Parameters["tBlueMolassesVCOImg"]);// D1 AOM VCO Cooling

        p.AddAnalogValue("tRbD1VVA", rbMOTStart, (double)Parameters["tBlueMolassesVVA"]);// D1 AOM VVA

        return p;
    }

    /*
    public override AnalogStaticBuilder GetAnalogStatic()
    {
        AnalogStaticBuilder p = new AnalogStaticBuilder((int)Parameters["PatternLength"]);

        return p;
    }*/

}
