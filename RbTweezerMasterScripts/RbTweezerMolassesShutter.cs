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
        Parameters["tMOTccSwitch"] = 5.5;

        //Parameters["tShimXccValue"] = 0.0; //  9.0; // 0.0
        //Parameters["tShimYccValue"] = 2.0; // -6.0; // 4.0
        //Parameters["tShimZccValue"] = 1.0; // 1.2; // -0.8


        Parameters["tShimXccValue"] = 7.5; //  7.5
        Parameters["tShimYccValue"] = -8.0; // -8.0
        Parameters["tShimZccValue"] = -2.3; // -2.3

        // Shim Coils Molasses
        Parameters["tMolassesShimXccValue"] = 8.8;//0.0
        Parameters["tMolassesShimYccValue"] = 3.8;//-1.5
        Parameters["tMolassesShimZccValue"] = 0.3;//1.5

        // Timing
        Parameters["tRbMOTStart"] = 10;
        Parameters["tRbMOTDuration"] = 50000;
        Parameters["TOF1"] = 200;
        Parameters["tRbMolassesDuration"] = 500;
        Parameters["TOF"] = 600;
        Parameters["tMOTExpo"] = 30000;
        Parameters["BGimg"] = 25000; // Time between img and BG 6000

        // tRb AOMs
        Parameters["tMOTCool"] = 3.2; //Cool at 2.4 - 2.6
        Parameters["tMOTRep"] = 3.0;
        Parameters["tMOTCoolImg"] = 3.5; // 3.6 - 4.0, resonance is 4.0
        Parameters["tMOTRepImg"] = 2.5; // 3.0
        Parameters["tMOTCoolMol"] = 2.0;
        Parameters["tMOTRepMol"] = 2.0;

    }
    // Digital Patter
    public override PatternBuilder32 GetDigitalPattern()
    {
        int rbMOTStart = (int)Parameters["tRbMOTStart"];
        int TOF1Start = rbMOTStart + (int)Parameters["tRbMOTDuration"];
        int rbMolassesStart = TOF1Start + (int)Parameters["TOF1"];
        int TOFStart = rbMolassesStart + (int)Parameters["tRbMolassesDuration"];
        int imagingStart = TOFStart + (int)Parameters["TOF"];
        int imagingBGStart = imagingStart + (int)Parameters["tMOTExpo"] + (int)Parameters["BGimg"] + 10;
        int TweezerStart = (int)Parameters["tRbMOTStart"];
        int TweezerEnd = imagingStart + (int)Parameters["tMOTExpo"];
        int TweezerStartAgain = TweezerEnd + (int)Parameters["BGimg"];
        int End = (int)Parameters["PatternLength"] - 100;

        PatternBuilder32 p = new PatternBuilder32();

        // Analog Board trigger
        p.Pulse(0, 0, 10, "aoTrigger");
        p.AddEdge("tD1AOMSwitch", rbMOTStart, true); // D1 switch off at Start

        // MOT Lasers
        p.Pulse(rbMOTStart, 0, (int)Parameters["tRbMOTDuration"], "tCoolSwitch"); // Red MOT Coolng Pulse
        p.Pulse(rbMOTStart, 0, (int)Parameters["tRbMOTDuration"], "tRepSwitch"); // Red MOT Repump Pulse

        // Molasses Lasers
        p.Pulse(rbMolassesStart, 0, (int)Parameters["tRbMolassesDuration"], "tCoolSwitch");
        p.Pulse(rbMolassesStart, 0, (int)Parameters["tRbMolassesDuration"], "tRepSwitch");

        // Coil trigger
        p.Pulse(rbMOTStart, 0, (int)Parameters["tRbMOTDuration"], "tMOTccTrig");

        // Imaging
        p.Pulse(imagingStart, 0, (int)Parameters["tMOTExpo"], "tCoolSwitch"); // Red MOT imaging 
        p.Pulse(imagingStart, 0, (int)Parameters["tMOTExpo"], "tRepSwitch"); // Red MOT imaging

        p.Pulse(imagingBGStart, 0, (int)Parameters["tMOTExpo"], "tCoolSwitch"); // Red MOT background imaging
        p.Pulse(imagingBGStart, 0, (int)Parameters["tMOTExpo"], "tRepSwitch"); // Red MOT background imaging

        p.Pulse(imagingStart, 0, 100, "tMOTCamTrig"); // Pike cam MOT imaging
        p.Pulse(imagingStart, 0, 100, "tHamCamTrig"); // Ham cam tweezer imaging
        p.Pulse(imagingBGStart, 0, 100, "tHamCamTrig"); // Ham cam tweezer background imaging

        // Tweezer AOM switch
        p.AddEdge("tTweezerSwitch", TweezerStart, true); // Turn on Tweezer
        p.AddEdge("tTweezerSwitch", TweezerEnd, false);
        p.AddEdge("tTweezerSwitch", TweezerStartAgain, true);
        //p.AddEdge("tTweezerModSwitch", TweezerStart, false); // Turn off modulation

        // END
        p.AddEdge("tCoolSwitch", End, true);
        p.AddEdge("tRepSwitch", End, true);

        return p;
    }
    // Analog Pattern

    public override AnalogPatternBuilder GetAnalogPattern()
    {
        int rbMOTStart = (int)Parameters["tRbMOTStart"];
        int TOF1Start = rbMOTStart + (int)Parameters["tRbMOTDuration"];
        int rbMolassesStart = TOF1Start + (int)Parameters["TOF1"];
        int TOFStart = rbMolassesStart + (int)Parameters["tRbMolassesDuration"];
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

        // MOT Laser VCOs
        p.AddAnalogValue("tRbCoolVCO", rbMOTStart, (double)Parameters["tMOTCool"]);// MOT Cooling Frequency
        p.AddAnalogValue("tRbRepVCO", rbMOTStart, (double)Parameters["tMOTRep"]);// MOT Repump Frequency

        // Molasses Laser VCOs
        p.AddAnalogValue("tRbCoolVCO", TOF1Start, (double)Parameters["tMOTCoolMol"]);// Molasses Cooling Frequency
        p.AddAnalogValue("tRbRepVCO", TOF1Start, (double)Parameters["tMOTRepMol"]);// MolassesRepump Frequency

        // Imaging Laser VCOs
        p.AddAnalogValue("tRbCoolVCO", TOFStart, (double)Parameters["tMOTCoolImg"]);// Img Cooling Frequency
        p.AddAnalogValue("tRbRepVCO", TOFStart, (double)Parameters["tMOTRepImg"]);// Img Repump Frequency

        return p;
    }

    /*
    public override AnalogStaticBuilder GetAnalogStatic()
    {
        AnalogStaticBuilder p = new AnalogStaticBuilder((int)Parameters["PatternLength"]);

        return p;
    }*/

}
