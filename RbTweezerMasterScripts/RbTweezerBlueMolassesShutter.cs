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

        // tMOT coils
        Parameters["tMOTccValue"] = 0.48;
        Parameters["tBlueMolassesccValue"] = 0.48;
        Parameters["tMOTccSwitch"] = 5.5;

        Parameters["tShimXccValue"] = 0.0; //  9.0; // 0.0 // 0.0
        Parameters["tShimYccValue"] = 2.0; // -6.0; // 4.0 // 2.0
        Parameters["tShimZccValue"] = 1.0; // 1.2; // -0.8 //1.0

        // Shim Coils Molasses
        Parameters["tMolassesShimXccValue"] = -1.6;//0.0 // -1.6
        Parameters["tMolassesShimYccValue"] = -0.6;//-1.5 //-0.6
        Parameters["tMolassesShimZccValue"] = 0.7;//1.5 //0.7

        // Timing
        Parameters["tRbMOTStart"] = 10;
        Parameters["tRbMOTDuration"] = 100000;
        Parameters["TOF1"] = 200;
        Parameters["tRbBlueMolassesDuration"] = 5000;
        Parameters["TOF"] = 400;
        Parameters["tMOTExpo"] = 30000;
        Parameters["BGimg"] = 25000; // Time between img and BG 6000

        // tRb AOMs
        Parameters["tMOTCool"] = 3.2; //Cool at 2.4 - 2.6
        Parameters["tMOTRep"] = 3.0;
        Parameters["tMOTCoolImg"] = 2.6; // 3.6 - 4.0, resonance is 4.0
        Parameters["tMOTRepImg"] = 2.6; // 3.0
        Parameters["tMOTCoolMol"] = 1.0;
        Parameters["tMOTRepMol"] = 1.0;

        Parameters["tBlueMolassesVCOCool"] = 5.0;
        Parameters["tBlueMolassesVCOImg"] = 5.0;
        Parameters["tBlueMolassesVVA"] = 8.0;

    }
    // Digital Patter
    public override PatternBuilder32 GetDigitalPattern()
    {
        int rbMOTStart = (int)Parameters["tRbMOTStart"];
        int TOF1Start = rbMOTStart + (int)Parameters["tRbMOTDuration"];
        int rbBlueMolassesStart = TOF1Start + (int)Parameters["TOF1"];
        int TOFStart = rbBlueMolassesStart + (int)Parameters["tRbBlueMolassesDuration"];
        int imagingStart = TOFStart + (int)Parameters["TOF"];
        int imagingBGStart = imagingStart + (int)Parameters["tMOTExpo"] + (int)Parameters["BGimg"] + 10;
        int TweezerStart = (int)Parameters["tRbMOTStart"];//tRbMOTStart
        int TweezerEnd = imagingStart + (int)Parameters["tMOTExpo"];
        int TweezerStartAgain = TweezerEnd + (int)Parameters["BGimg"];
        int End = (int)Parameters["PatternLength"] - 100;

        PatternBuilder32 p = new PatternBuilder32();

        // Analog Board trigger
        p.Pulse(0, 0, 10, "aoTrigger");

        // MOT Lasers
        p.Pulse(rbMOTStart, 0, (int)Parameters["tRbMOTDuration"], "tCoolSwitch"); // Red MOT Coolng Pulse
        p.Pulse(rbMOTStart, 0, (int)Parameters["tRbMOTDuration"], "tRepSwitch"); // Red MOT Repump Pulse

        // Blue Molasses Lasers
        p.AddEdge("tD1AOMSwitch", rbMOTStart, true); // D1 switch off at Start
        p.AddEdge("tD1AOMSwitch", rbBlueMolassesStart, false); // D1 switch on for blue molasses
        p.AddEdge("tD1AOMSwitch", TOFStart, true); // D1 switch off at end of blue molasses

        //p.Pulse(rbBlueMolassesStart, 0, (int)Parameters["tRbBlueMolassesDuration"], "tCoolSwitch");
        //p.Pulse(rbBlueMolassesStart, 0, (int)Parameters["tRbBlueMolassesDuration"], "tRepSwitch");

        // Coil trigger
        p.Pulse(rbMOTStart, 0, (int)Parameters["tRbMOTDuration"], "tMOTccTrig");

        // Imaging
        p.Pulse(imagingStart, 0, (int)Parameters["tMOTExpo"], "tCoolSwitch"); // Red MOT imaging 
        p.Pulse(imagingStart, 0, (int)Parameters["tMOTExpo"], "tRepSwitch"); // Red MOT imaging

        //p.AddEdge("tD1AOMSwitch", imagingStart, false); 
        //p.AddEdge("tD1AOMSwitch", imagingStart + (int)Parameters["tMOTExpo"], true); 

        p.Pulse(imagingBGStart, 0, (int)Parameters["tMOTExpo"], "tCoolSwitch"); // Red MOT background imaging
        p.Pulse(imagingBGStart, 0, (int)Parameters["tMOTExpo"], "tRepSwitch"); // Red MOT background imaging

        //p.AddEdge("tD1AOMSwitch", imagingBGStart, false);
        //p.AddEdge("tD1AOMSwitch", imagingBGStart + (int)Parameters["tMOTExpo"], true);

        p.Pulse(imagingStart, 0, 100, "tMOTCamTrig"); // Pike cam MOT imaging
        p.Pulse(imagingStart, 0, 1000, "tHamCamTrig"); // Ham cam tweezer imaging
        //p.Pulse(imagingBGStart, 0, 1000, "tHamCamTrig"); // Ham cam tweezer background imaging

        // Tweezer AOM switch
        p.AddEdge("tTweezerSwitch", rbBlueMolassesStart, true); // Turn on Tweezer // + 50000
        p.AddEdge("tTweezerSwitch", TweezerEnd, false);
        p.AddEdge("tTweezerSwitch", TweezerStartAgain, true);

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
        int imagingBGStart = imagingStart + (int)Parameters["tMOTExpo"] + (int)Parameters["BGimg"] + 10;
        int TweezerStart = (int)Parameters["tRbMOTStart"];
        int TweezerEnd = imagingStart + (int)Parameters["tMOTExpo"];
        int TweezerStartAgain = TweezerEnd + (int)Parameters["BGimg"];
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
        p.AddAnalogValue("tMOTccSwitch", TOF1Start + 200, (double)Parameters["tMOTccSwitch"]);// MOT coils switch-off (RSD)

        p.AddAnalogValue("tMOTcc", rbMOTStart, (double)Parameters["tMOTccValue"]);// MOT coils
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

        // MOT Lasers
        p.AddAnalogValue("tRbCoolVCO", rbMOTStart, (double)Parameters["tMOTCool"]);// Cooling Frequency
        p.AddAnalogValue("tRbRepVCO", rbMOTStart, (double)Parameters["tMOTRep"]);// Repump Frequency

        // Molasses Laser
        p.AddAnalogValue("tRbD1VCO", rbMOTStart, (double)Parameters["tBlueMolassesVCOCool"]);// D1 AOM VCO Cooling
        p.AddAnalogValue("tRbD1VVA", rbMOTStart, (double)Parameters["tBlueMolassesVVA"]);// D1 AOM VVA

        p.AddAnalogValue("tRbCoolVCO", TOF1Start, (double)Parameters["tMOTCoolMol"]);// Molasses Cooling Frequency
        p.AddAnalogValue("tRbRepVCO", TOF1Start, (double)Parameters["tMOTRepMol"]);// MolassesRepump Frequency

        // Imaging lasers
        p.AddAnalogValue("tRbCoolVCO", TOFStart, (double)Parameters["tMOTCoolImg"]);// Cooling Frequency for Fluo Img
        p.AddAnalogValue("tRbRepVCO", TOFStart, (double)Parameters["tMOTRepImg"]);// Repump Frequency for Fluo Img

        p.AddAnalogValue("tRbD1VCO", TOFStart, (double)Parameters["tBlueMolassesVCOImg"]);// D1 AOM VCO Cooling

        return p;
    }

    /*
    public override AnalogStaticBuilder GetAnalogStatic()
    {
        AnalogStaticBuilder p = new AnalogStaticBuilder((int)Parameters["PatternLength"]);

        return p;
    }*/

}
