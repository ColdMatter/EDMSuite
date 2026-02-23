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
        Parameters["PatternLength"] = 300000;
        Parameters["TCLBlockStart"] = 0;
        Parameters["tScanCount"] = 682;

        // tMOT coils
        Parameters["tMOTccValue"] = 0.48;
        Parameters["tBlueMOTccValue"] = 0.0; // 1.8 for blue MOT

        Parameters["tMOTccSwitch"] = 5.5;

        Parameters["tShimXccValue"] = 8.8; //  7.5
        Parameters["tShimYccValue"] = -0.8; // -8.0
        Parameters["tShimZccValue"] = -1.3; // -2.3

        // Shim Coils Molasses
        Parameters["tMolassesShimXccValue"] = 8.8;//8.8
        Parameters["tMolassesShimYccValue"] = 3.8;//3.8
        Parameters["tMolassesShimZccValue"] = 0.3;//0.3

        // Shim Coils Blue MOT
        Parameters["tBlueMOTShimXccValue"] = 8.8; // 0.0
        Parameters["tBlueMOTShimYccValue"] = 3.8; // 0.0
        Parameters["tBlueMOTShimZccValue"] = 0.3; // -3.0

        // Timing
        Parameters["TOF1"] = 1;   
        Parameters["TOF"] = 1;
        Parameters["tRbMOTStart"] = 10;
        Parameters["tMOTExpo"] = 10000;
        Parameters["tRbMOTDuration"] = 50000;
        Parameters["tRbBlueMolassesDuration"] = 5000; // 500
        Parameters["tRbBlueMOTRampDuration"] = 3000; // 500 for blue MOT
        Parameters["tRbBlueMOTDuration"] = 5000; // 500 for blue MOT



        // tRb AOMs
        Parameters["tMOTCool"] = 3.2; //Cool at 2.4 - 2.6
        Parameters["tMOTRep"] = 3.0;
        Parameters["tMOTCoolImg"] = 0.7; // 3.6 - 4.0, resonance is 4.0
        Parameters["tMOTRepImg"] = 0.7; // 3.0
        Parameters["tMOTCoolMol"] = 1.0;
        Parameters["tMOTRepMol"] = 3.0;

        Parameters["tBlueMolassesVCOCool"] = 6.0; // 3.0
        Parameters["tBlueMolassesVCOCoolRamp"] = 6.0; // 6.0
        Parameters["tBlueMolassesVCOImg"] = 5.0;
        Parameters["tBlueMolassesVVA"] = 10.0; // 10.0
        Parameters["tBlueMolassesVVARamp"] = 10.0; // 4.0
 
        Parameters["tBlueMotVCO"] = 6.0; // 5.0 for blue MOT
        Parameters["tBlueMotVVA"] =10.0;// 4.5 for blue MOT

        // tRb D1 sideband repump (limited between 0.15V and 0.45V)
        Parameters["tD1SidebandBlueMolasses"] = 0.28;
        Parameters["tD1SidebandBlueMOT"] = 0.28; // 0.24 for blue MOT 

        Parameters["TweezerExposure"] = 100000;
        Parameters["TweezerBackgroundWaitTime"] = 20;

    }
    // Digital Patter
    public override PatternBuilder32 GetDigitalPattern()
    {
        int rbMOTStart = (int)Parameters["tRbMOTStart"];
        int TOF1Start = rbMOTStart + (int)Parameters["tRbMOTDuration"];
        int rbBlueMolassesStart = TOF1Start + (int)Parameters["TOF1"];
        int rbBlueMOTRampStart = rbBlueMolassesStart + (int)Parameters["tRbBlueMolassesDuration"];
        int rbBlueMOTStart = rbBlueMOTRampStart + (int)Parameters["tRbBlueMOTRampDuration"];
        int TOFStart = rbBlueMOTStart + (int)Parameters["tRbBlueMOTDuration"];
        int imagingStart = TOFStart + (int)Parameters["TOF"];
        int TweezerOff1 = imagingStart + (int)Parameters["TweezerExposure"];
        int backgroundImageStart = TweezerOff1 + (int)Parameters["TweezerBackgroundWaitTime"];
        int TweezerOff2 = backgroundImageStart + (int)Parameters["TweezerExposure"];
        int End = (int)Parameters["PatternLength"] - 100;
        
        PatternBuilder32 p = new PatternBuilder32();

        // Analog Board trigger
        p.Pulse(0, 0, 10, "aoTrigger");

        // MOT Laser

        p.Pulse(rbMOTStart, 0, (int)Parameters["tRbMOTDuration"] , "tCoolSwitch"); // Red MOT Coolng Pulse //+ (int)Parameters["tRbBlueMOTDuration"]
        p.Pulse(rbMOTStart, 0, (int)Parameters["tRbMOTDuration"] , "tRepSwitch"); // Red MOT Repump Pulse


        
        p.AddEdge("tD1AOMSwitch", rbMOTStart, true); // D1 switch off at Start
        p.AddEdge("tD1AOMSwitch", rbBlueMolassesStart, false);
        p.AddEdge("tD1AOMSwitch", rbBlueMOTRampStart, true);
        p.AddEdge("tD1AOMSwitch", rbBlueMOTStart, false);
        p.AddEdge("tD1AOMSwitch", TOFStart, true);


        // Tweezer
        p.AddEdge("tTweezerSwitch", rbMOTStart, true); // Turn on Tweezer
        p.AddEdge("tTweezerSwitch", rbMOTStart+100, false); // Turn off Tweezer
        p.AddEdge("tTweezerSwitch", rbBlueMolassesStart, true); // Turn on Tweezer
        p.AddEdge("tTweezerSwitch", TweezerOff1, false);
        p.AddEdge("tTweezerSwitch", backgroundImageStart, true); // Turn on Tweezer
        p.AddEdge("tTweezerSwitch", TweezerOff2, false); // Turn on Tweezer
        p.AddEdge("tTweezerSwitch", End, true);


        // Molasses Lasers

        //p.Pulse(rbBlueMolassesStart, 0, (int)Parameters["tRbBlueMolassesDuration"], "tCoolSwitch"); //D2 Molasses Cooling
        //p.Pulse(rbBlueMolassesStart, 0, (int)Parameters["tRbBlueMolassesDuration"], "tRepSwitch"); //D2 Molasses Repump

        //p.AddEdge("tD1AOMSwitch", rbBlueMolassesStart, false);
        //p.AddEdge("tD1AOMSwitch", TOFStart, true);

        // Coil trigger
        p.Pulse(rbMOTStart, 0, (int)Parameters["tRbMOTDuration"], "tMOTccTrig");
        p.Pulse(rbBlueMOTRampStart, 0, (int)Parameters["tRbBlueMOTRampDuration"] + (int)Parameters["tRbBlueMOTDuration"], "tMOTccTrig");

        // Imaging

        //p.Pulse(imagingStart, 0, (int)Parameters["tMOTExpo"], "tCoolSwitch"); // Red MOT imaging 
        //p.Pulse(imagingStart, 0, (int)Parameters["tMOTExpo"], "tRepSwitch"); // Red MOT imaging

        //p.Pulse(rbBlueMOTStart, 0, (int)Parameters["tRbBlueMOTDuration"], "tCoolSwitch"); // Red MOT imaging
        //p.Pulse(rbBlueMOTStart, 0, (int)Parameters["tRbBlueMOTDuration"], "tRepSwitch"); // Red MOT imaging

        p.Pulse(imagingStart, 0, (int)Parameters["TweezerExposure"], "tCoolSwitch"); // Red MOT imaging 
        p.Pulse(imagingStart, 0, (int)Parameters["TweezerExposure"], "tRepSwitch"); // Red MOT imaging


        p.Pulse(backgroundImageStart, 0, (int)Parameters["TweezerExposure"], "tCoolSwitch"); // Red MOT imaging 
        p.Pulse(backgroundImageStart, 0, (int)Parameters["TweezerExposure"], "tRepSwitch"); // Red MOT imaging

        //p.AddEdge("tD1AOMSwitch", imagingStart, false);
        //p.AddEdge("tD1AOMSwitch", imagingStart + (int)Parameters["tMOTExpo"], true);

        //p.Pulse(TOFStart - 300, 0, 100, "tMOTCamTrig");
        //p.Pulse(TOFStart - 1000, 0, 1000, "tMOTCamTrig");
        p.Pulse(imagingStart, 0, 300, "tMOTCamTrig");
        p.Pulse(imagingStart, 0, 1000, "tHamCamTrig");
        p.Pulse(backgroundImageStart, 0, 1000, "tHamCamTrig");

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
        int rbBlueMOTRampStart = rbBlueMolassesStart + (int)Parameters["tRbBlueMolassesDuration"];
        int rbBlueMOTStart = rbBlueMOTRampStart + (int)Parameters["tRbBlueMOTRampDuration"];
        int TOFStart = rbBlueMOTStart + (int)Parameters["tRbBlueMOTDuration"];
        int imagingStart = TOFStart + (int)Parameters["TOF"];
        int TweezerOff1 = imagingStart + (int)Parameters["TweezerExposure"];
        int backgroundImageStart = TweezerOff1 + (int)Parameters["TweezerBackgroundWaitTime"];
        int TweezerOff2 = backgroundImageStart + (int)Parameters["TweezerExposure"];
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
        p.AddChannel("tD1SidebandControl");

        // Coils
        p.AddAnalogValue("tMOTccSwitch", rbMOTStart, 0.0);// MOT coils switch-on (RSD)
        p.AddAnalogValue("tMOTccSwitch", TOFStart, (double)Parameters["tMOTccSwitch"]);// MOT coils switch-off (RSD)


        p.AddAnalogValue("tMOTcc", rbMOTStart, (double)Parameters["tMOTccValue"]);// MOT coils
        p.AddAnalogValue("tMOTcc", TOF1Start, 0.00);
        p.AddLinearRamp("tMOTcc", rbBlueMOTRampStart, (int)Parameters["tRbBlueMOTRampDuration"], (double)Parameters["tBlueMOTccValue"]);
       // p.AddAnalogValue("tMOTcc", rbBlueMOTStart, (double)Parameters["tBlueMOTccValue"]);// MOT coils
       //p.AddLinearRamp("tMOTcc", rbBlueMOTStart+200, 10000, 0.48); // Ramp down
        p.AddAnalogValue("tMOTcc", TOFStart, 0.00);

        p.AddAnalogValue("tShimXcc", rbMOTStart, (double)Parameters["tShimXccValue"]);// Shim coils X
        p.AddAnalogValue("tShimXcc", TOF1Start, (double)Parameters["tMolassesShimXccValue"]);// Shim coils X - Molasses
        p.AddLinearRamp("tShimXcc", rbBlueMOTRampStart, (int)Parameters["tRbBlueMOTRampDuration"], (double)Parameters["tBlueMOTShimXccValue"]);
        //p.AddAnalogValue("tShimXcc", rbBlueMOTRampStart, (double)Parameters["tBlueMOTShimXccValue"]);// Shim coils X - Molasses
        p.AddAnalogValue("tShimXcc", End, 0.0);

        p.AddAnalogValue("tShimYcc", rbMOTStart, (double)Parameters["tShimYccValue"]);// Shim coils Y
        p.AddAnalogValue("tShimYcc", TOF1Start, (double)Parameters["tMolassesShimYccValue"]);// Shim coils X - Molasses
        p.AddLinearRamp("tShimYcc", rbBlueMOTRampStart, (int)Parameters["tRbBlueMOTRampDuration"], (double)Parameters["tBlueMOTShimYccValue"]);
        //p.AddAnalogValue("tShimYcc", rbBlueMOTRampStart, (double)Parameters["tBlueMOTShimYccValue"]);// Shim coils Y - Blue MOT
        p.AddAnalogValue("tShimYcc", End, 0.0);

        p.AddAnalogValue("tShimZcc", rbMOTStart, (double)Parameters["tShimZccValue"]);// Shim coils Z
        p.AddAnalogValue("tShimZcc", TOF1Start, (double)Parameters["tMolassesShimZccValue"]);// Shim coils X - Molasses
        p.AddLinearRamp("tShimZcc", rbBlueMOTRampStart, (int)Parameters["tRbBlueMOTRampDuration"], (double)Parameters["tBlueMOTShimZccValue"]);
        //p.AddAnalogValue("tShimZcc", rbBlueMOTRampStart, (double)Parameters["tBlueMOTShimZccValue"]);// Shim coils Z - Blue MOT
        p.AddAnalogValue("tShimZcc", End, 0.0);

        // Lasers
        p.AddAnalogValue("tRbCoolVCO", rbMOTStart, (double)Parameters["tMOTCool"]);// Cooling Frequency
        p.AddAnalogValue("tRbRepVCO", rbMOTStart, (double)Parameters["tMOTRep"]);// Repump Frequency

        // Molasses Laser VCOs

        //p.AddAnalogValue("tRbCoolVCO", rbBlueMOTStart, (double)Parameters["tMOTCoolMol"]);// D2 Molasses Cooling Frequency
        //p.AddAnalogValue("tRbRepVCO", rbBlueMOTStart, (double)Parameters["tMOTRepMol"]);// D2 MolassesRepump Frequency
        //p.AddAnalogValue("tRbCoolVCO", TOF1Start, (double)Parameters["tMOTCoolMol"]);// D2 Molasses Cooling Frequency
        //p.AddAnalogValue("tRbRepVCO", TOF1Start, (double)Parameters["tMOTRepMol"]);// D2 MolassesRepump Frequency

        p.AddAnalogValue("tRbCoolVCO", TOFStart, (double)Parameters["tMOTCoolImg"]);// Cooling Frequency for Fluo Img
        p.AddAnalogValue("tRbRepVCO", TOFStart, (double)Parameters["tMOTRepImg"]);// Repump Frequency for Fluo Img

        p.AddAnalogValue("tRbD1VCO", rbMOTStart, (double)Parameters["tBlueMolassesVCOCool"]);// D1 AOM VCO Cooling
        p.AddLinearRamp("tRbD1VCO", rbBlueMolassesStart, (int)Parameters["tRbBlueMolassesDuration"], (double)Parameters["tBlueMolassesVCOCoolRamp"]); // D1 AOM VCO Ramp down
        p.AddAnalogValue("tRbD1VCO", rbBlueMOTRampStart, (double)Parameters["tBlueMotVCO"]);// D1 AOM VCO Cooling
        //p.AddLinearRamp("tRbD1VCO", rbBlueMOTRampStart, (int)Parameters["tRbBlueMOTRampDuration"], (double)Parameters["tBlueMotVCO"]);
        p.AddAnalogValue("tRbD1VCO", TOFStart, (double)Parameters["tBlueMolassesVCOCool"]);

        p.AddAnalogValue("tRbD1VVA", rbMOTStart, (double)Parameters["tBlueMolassesVVA"]);// D1 AOM VVA
        p.AddLinearRamp("tRbD1VVA", rbBlueMolassesStart, (int)Parameters["tRbBlueMolassesDuration"], (double)Parameters["tBlueMolassesVVARamp"]); // D1 AOM VVA Ramp down
        p.AddAnalogValue("tRbD1VVA", rbBlueMOTRampStart, 0.0);// D1 AOM VVA
        p.AddAnalogValue("tRbD1VVA", rbBlueMOTStart, (double)Parameters["tBlueMotVVA"]);// D1 AOM VVA
        //p.AddLinearRamp("tRbD1VVA", rbBlueMOTRampStart, (int)Parameters["tRbBlueMOTRampDuration"], (double)Parameters["tBlueMotVVA"]);
        p.AddAnalogValue("tRbD1VVA", TOFStart, (double)Parameters["tBlueMolassesVVA"]);// D1 AOM VVA

        // D1 sidebands
        
        p.AddAnalogValue("tD1SidebandControl", rbBlueMOTStart, (double)Parameters["tD1SidebandBlueMOT"]);
        p.AddAnalogValue("tD1SidebandControl", End, (double)Parameters["tD1SidebandBlueMolasses"]);

        return p;
    }

    /*
    public override AnalogStaticBuilder GetAnalogStatic()
    {
        AnalogStaticBuilder p = new AnalogStaticBuilder((int)Parameters["PatternLength"]);

        return p;
    }*/

}
