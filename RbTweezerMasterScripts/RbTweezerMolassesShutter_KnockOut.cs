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
        Parameters["PatternLength"] = 120000;
        Parameters["TCLBlockStart"] = 0;

        //Timing
        Parameters["tRbMOTccStart"] = 10;
        Parameters["tRbMOTccDuration"] = 50000; // Loading the MOT
        Parameters["TOF1"] = 200; // Allowing the magnetic field to decay before the molasses
        Parameters["tRbMolassesDuration"] = 1200; // Molasses
        Parameters["TOF2"] = 3000; // Freefall and expansion
        Parameters["tMOTExpo"] = 20000; // Imaging exposure time
        Parameters["BGimg"] = 6000; // Time between img and BG
        Parameters["tKnock"] = 2000;

        // MOT coils
        Parameters["tMOTccValue"] = 0.12;

        // Shim Coils MOT
        Parameters["tShimXccValue"] = 0.0; // 8.0s
        Parameters["tShimYccValue"] = 0.0; // -5.0 
        Parameters["tShimZccValue"] = 0.0; // 1.8

        // Shim Coils Mol
        Parameters["tMolassesShimXccValue"] = 2.0;  // 2.0
        Parameters["tMolassesShimYccValue"] = 1.5;  // 1.5
        Parameters["tMolassesShimZccValue"] = 0.5;  // 0.5 

        // tRb AOMs
        Parameters["tMOTCool"] = 3.2; // 3.2 V for max atom number
        Parameters["tMOTRep"] = 3.0;
        Parameters["tMOTCoolImg"] = 2.8; // 4.2 V On-resonance
        Parameters["tMOTRepImg"] = 2.8; // 3.0
        Parameters["tMOTCoolKnock"] = 5.0; // 4.2 V On-resonance
        Parameters["tMOTRepKnock"] = 5.0; // 3.0
        Parameters["tMOTCoolMol"] = 0.0;
        Parameters["tMOTRepMol"] = 0.9;

        // Tweezer
        Parameters["tTweezerStart"] = 50000; // Tweezer start before MOT off
        

    }
    // Digital Patter
    public override PatternBuilder32 GetDigitalPattern()
    {
        int rbMOTStart = (int)Parameters["tRbMOTccStart"];
        int TOF1Start = rbMOTStart + (int)Parameters["tRbMOTccDuration"];
        int rbMolassesStart = TOF1Start + (int)Parameters["TOF1"];
        int TOF2Start = rbMolassesStart + (int)Parameters["tRbMolassesDuration"];
        int imagingStart = TOF2Start + (int)Parameters["TOF2"];
        int imagingBGStart = imagingStart + (int)Parameters["tMOTExpo"] + (int)Parameters["BGimg"];
        int TweezerStart = rbMOTStart +  (int)Parameters["tRbMOTccDuration"] - (int)Parameters["tTweezerStart"];
        int TweezerEnd = imagingStart + (int)Parameters["tMOTExpo"];
        int TweezerStartAgain = TweezerEnd + (int)Parameters["BGimg"];

        int End = (int)Parameters["PatternLength"] - 100;

        PatternBuilder32 p = new PatternBuilder32();

        // Analog Board trigger
        p.Pulse(0, 0, 10, "aoTrigger");

        // MOT Lasers
        p.Pulse(rbMOTStart, 0, (int)Parameters["tRbMOTccDuration"], "tCoolSwitch");
        p.Pulse(rbMOTStart, 0, (int)Parameters["tRbMOTccDuration"], "tRepSwitch");

        // MOT Coil trigger
        p.Pulse(rbMOTStart, 0, (int)Parameters["tRbMOTccDuration"] + (int)Parameters["TOF1"] + (int)Parameters["tRbMolassesDuration"], "tMOTccTrig");

        // Molasses Lasers
        p.Pulse(rbMolassesStart, 0, (int)Parameters["tRbMolassesDuration"] , "tCoolSwitch");
        p.Pulse(rbMolassesStart, 0, (int)Parameters["tRbMolassesDuration"] - 200, "tRepSwitch");

        // Knock out light
        p.Pulse(TOF2Start+1, 0, (int)Parameters["tKnock"]-2, "tCoolSwitch");
        //p.Pulse(TOF2Start+1, 0, (int)Parameters["tKnock"]-2, "tRepSwitch");

        // Imaging Lasers
        p.Pulse(imagingStart, 0, (int)Parameters["tMOTExpo"], "tCoolSwitch");
        p.Pulse(imagingStart, 0, (int)Parameters["tMOTExpo"], "tRepSwitch");

        p.Pulse(imagingBGStart, 0, (int)Parameters["tMOTExpo"], "tCoolSwitch");
        p.Pulse(imagingBGStart, 0, (int)Parameters["tMOTExpo"], "tRepSwitch");

        // Imaging Cam Trigger
        p.Pulse(imagingStart, 0, 100, "tMOTCamTrig"); // Pike trigger
        p.Pulse(imagingStart, 0, 100, "tHamCamTrig"); // HamCam trigger img
        p.Pulse(imagingBGStart, 0, 100, "tHamCamTrig"); // HamCam trigger img

        // Pilot Laser
        p.AddEdge("tRbFluo", rbMOTStart, true); // Turn off at start of pattern
        p.AddEdge("tRbFluo", imagingStart, false); // Turn on during imaging

        // Tweezer Shutter
        p.Pulse(TweezerStart, 0, TweezerEnd - TweezerStart, "tTweezerShutter");
        p.AddEdge("tTweezerShutter", TweezerStartAgain, true); // Turn on Tweezer
        //p.Pulse(TweezerEnd, 0,  (int)Parameters["BGimg"], "tTweezerShutter");

        // Tweezer AOM switch
        p.Pulse(TweezerEnd, 0, TweezerStartAgain - TweezerEnd, "tTweezerSwitch");
        //p.AddEdge("tTweezerSwitch", TweezerStart, true); // Turn on Tweezer
        //p.AddEdge("tTweezerSwitch", TweezerEnd, true);
        //p.AddEdge("tTweezerSwitch", TweezerStartAgain, false);

        // END
        p.AddEdge("tCoolSwitch", End, true); // Turn on cooling laser
        p.AddEdge("tRepSwitch", End, true); // Turn on repump laser

        return p;
    }
    // Analog Pattern

    public override AnalogPatternBuilder GetAnalogPattern()
    {
        int rbMOTStart = (int)Parameters["tRbMOTccStart"];
        int TOF1Start = rbMOTStart + (int)Parameters["tRbMOTccDuration"];
        int rbMolassesStart = TOF1Start + (int)Parameters["TOF1"];
        int TOF2Start = rbMolassesStart + (int)Parameters["tRbMolassesDuration"];
        int imagingStart = TOF2Start + (int)Parameters["TOF2"];

        AnalogPatternBuilder p = new AnalogPatternBuilder((int)Parameters["PatternLength"]);

        // Analog Channels
        p.AddChannel("tMOTcc");
        p.AddChannel("tShimXcc");
        p.AddChannel("tShimYcc");
        p.AddChannel("tShimZcc");
        p.AddChannel("tRbCoolVCO");
        p.AddChannel("tRbRepVCO");

        // MOT Coils
        p.AddAnalogValue("tMOTcc", rbMOTStart, (double)Parameters["tMOTccValue"]);
        p.AddAnalogValue("tMOTcc", TOF1Start, 0);
        //p.AddLinearRamp("tMOTcc", TOF1Start, 100, 0.0); // Ramp down

        // Shim Coils
        p.AddAnalogValue("tShimXcc", rbMOTStart, (double)Parameters["tShimXccValue"]);// Shim coils X - MOT
        p.AddAnalogValue("tShimXcc", TOF1Start, (double)Parameters["tMolassesShimXccValue"]);// Shim coils X - Molasses
        p.AddAnalogValue("tShimXcc", imagingStart + (int)Parameters["tMOTExpo"], 0); // Step down
        //p.AddLinearRamp("tShimXcc", TOF2Start, 100, 0.0); // Ramp down

        p.AddAnalogValue("tShimYcc", rbMOTStart, (double)Parameters["tShimYccValue"]);// Shim coils Y - MOT
        p.AddAnalogValue("tShimYcc", TOF1Start, (double)Parameters["tMolassesShimYccValue"]);// Shim coils X - Molasses
        p.AddAnalogValue("tShimYcc", imagingStart + (int)Parameters["tMOTExpo"], 0); // Step down
        //p.AddLinearRamp("tShimYcc", TOF2Start, 100, 0.0); // Ramp down

        p.AddAnalogValue("tShimZcc", rbMOTStart, (double)Parameters["tShimZccValue"]);// Shim coils Z - MOT
        p.AddAnalogValue("tShimZcc", TOF1Start, (double)Parameters["tMolassesShimZccValue"]);// Shim coils X - Molasses
        p.AddAnalogValue("tShimZcc", imagingStart + (int)Parameters["tMOTExpo"], 0); // Step down
        //p.AddLinearRamp("tShimZcc", TOF2Start, 100, 0.0); // Ramp down



        // MOT Laser VCOs
        p.AddAnalogValue("tRbCoolVCO", rbMOTStart, (double)Parameters["tMOTCool"]);// MOT Cooling Frequency
        p.AddAnalogValue("tRbRepVCO", rbMOTStart, (double)Parameters["tMOTRep"]);// MOT Repump Frequency

        // Molasses Laser VCOs
        p.AddAnalogValue("tRbCoolVCO", TOF1Start, (double)Parameters["tMOTCoolMol"]);// Molasses Cooling Frequency
        p.AddAnalogValue("tRbRepVCO", TOF1Start, (double)Parameters["tMOTRepMol"]);// MolassesRepump Frequency

        // Imaging Laser VCOs
        p.AddAnalogValue("tRbCoolVCO", TOF2Start, (double)Parameters["tMOTCoolKnock"]);// Cooling knock Frequency
        p.AddAnalogValue("tRbRepVCO", TOF2Start, (double)Parameters["tMOTRepKnock"]);// Repump knock Frequency

        p.AddAnalogValue("tRbCoolVCO", TOF2Start + (int)Parameters["tKnock"], (double)Parameters["tMOTCoolImg"]);// Img Cooling Frequency
        p.AddAnalogValue("tRbRepVCO", TOF2Start + (int)Parameters["tKnock"], (double)Parameters["tMOTRepImg"]);// Img Repump Frequency


        return p;
    }

    /*
    public override AnalogStaticBuilder GetAnalogStatic()
    {
        AnalogStaticBuilder p = new AnalogStaticBuilder((int)Parameters["PatternLength"]);

        return p;
    }*/

}
