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

        //Timing
        Parameters["tRbMOTccStart"] = 10;
        Parameters["tRbMOTccDuration"] = 100000; // Loading the MOT
        Parameters["TOF1"] = 100; // Allowing the magnetic field to decay before the molasses
        Parameters["tRbMolassesDuration"] = 500; // Molasses
        Parameters["TOF2"] = 100; // Freefall and expansion
        Parameters["tMOTExpo"] = 300; // Imaging exposure time

        // MOT coils
        Parameters["tMOTccValue"] = 0.48;
        Parameters["tMOTccSwitch"] = 5.5;

        // Shim Coils MOT
        Parameters["tShimXccValue"] = 0.0; // 8.0
        Parameters["tShimYccValue"] = 0.0; // -5.0 
        Parameters["tShimZccValue"] = 0.0; // 1.8
        /*
        Parameters["tShimXccValue"] = 7.5; // 8.0
        Parameters["tShimYccValue"] = -8.0; // -5.0 
        Parameters["tShimZccValue"] = -2.3; // 1.8
        */

        // Shim Coils Mol
        Parameters["tMolassesShimXccValue"] = 8.8; 
        Parameters["tMolassesShimYccValue"] = 3.8;  
        Parameters["tMolassesShimZccValue"] = 0.3; 

        // tRb AOMs
        Parameters["tMOTCool"] = 3.2; // 3.2 V for max atom number
        Parameters["tMOTRep"] = 3.0;
        Parameters["tMOTCoolImg"] = 3.2; // 4.2 V On-resonance
        Parameters["tMOTRepImg"] = 3.0; // 3.0
        Parameters["tMOTCoolMol"] = 1.9;
        Parameters["tMOTRepMol"] = 1.9;

    }
    // Digital Patter
    public override PatternBuilder32 GetDigitalPattern()
    {
        int rbMOTStart = (int)Parameters["tRbMOTccStart"];
        int TOF1Start = rbMOTStart + (int)Parameters["tRbMOTccDuration"];
        int rbMolassesStart = TOF1Start + (int)Parameters["TOF1"];
        int TOF2Start = rbMolassesStart + (int)Parameters["tRbMolassesDuration"];
        int imagingStart = TOF2Start + (int)Parameters["TOF2"];
        int End = (int)Parameters["PatternLength"] - 100;

        PatternBuilder32 p = new PatternBuilder32();

        // Analog Board trigger
        p.Pulse(0, 0, 10, "aoTrigger");

        p.AddEdge("tD1AOMSwitch", rbMOTStart, true); // D1 switch off at Start

        // MOT Lasers
        p.Pulse(rbMOTStart, 0, (int)Parameters["tRbMOTccDuration"], "tCoolSwitch");
        p.Pulse(rbMOTStart, 0, (int)Parameters["tRbMOTccDuration"], "tRepSwitch");

        // MOT Coil trigger
        p.Pulse(rbMOTStart, 0, (int)Parameters["tRbMOTccDuration"], "tMOTccTrig");

        // Molasses Lasers
        p.Pulse(rbMolassesStart, 0, (int)Parameters["tRbMolassesDuration"], "tCoolSwitch");
        p.Pulse(rbMolassesStart, 0, (int)Parameters["tRbMolassesDuration"], "tRepSwitch");

        // Imaging Lasers
        p.Pulse(imagingStart, 0, (int)Parameters["tMOTExpo"], "tCoolSwitch");
        p.Pulse(imagingStart, 0, (int)Parameters["tMOTExpo"], "tRepSwitch");

        // Imaging Cam Trigger
        p.Pulse(imagingStart, 0, 100, "tMOTCamTrig"); // Pike trigger
        p.Pulse(imagingStart, 0, 1000, "tHamCamTrig"); // HamCam trigger

        // Pilot Laser
        // p.AddEdge("tRbFluo", rbMOTStart, true); // Turn off at start of pattern
        // p.AddEdge("tRbFluo", imagingStart, false); // Turn on during imaging

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
        p.AddChannel("tMOTccSwitch");

        // MOT Coils
        p.AddAnalogValue("tMOTccSwitch", rbMOTStart, 0.0);// MOT coils switch-on (RSD)
        p.AddAnalogValue("tMOTccSwitch", TOF1Start + 200, (double)Parameters["tMOTccSwitch"]);// MOT coils switch-off (RSD)

        p.AddAnalogValue("tMOTcc", rbMOTStart, (double)Parameters["tMOTccValue"]);
        p.AddAnalogValue("tMOTcc", TOF1Start, 0);
        //p.AddLinearRamp("tMOTcc", TOF1Start, 100, 0.0); // Ramp down

        // Shim Coils
        p.AddAnalogValue("tShimXcc", rbMOTStart, (double)Parameters["tShimXccValue"]);// Shim coils X - MOT
        p.AddAnalogValue("tShimXcc", TOF1Start, (double)Parameters["tMolassesShimXccValue"]);// Shim coils X - Molasses
        p.AddAnalogValue("tShimXcc", TOF2Start, 0); // Step down
        //p.AddLinearRamp("tShimXcc", TOF2Start, 100, 0.0); // Ramp down

        p.AddAnalogValue("tShimYcc", rbMOTStart, (double)Parameters["tShimYccValue"]);// Shim coils Y - MOT
        p.AddAnalogValue("tShimYcc", TOF1Start, (double)Parameters["tMolassesShimYccValue"]);// Shim coils X - Molasses
        p.AddAnalogValue("tShimYcc", TOF2Start, 0); // Step down
        //p.AddLinearRamp("tShimYcc", TOF2Start, 100, 0.0); // Ramp down

        p.AddAnalogValue("tShimZcc", rbMOTStart, (double)Parameters["tShimZccValue"]);// Shim coils Z - MOT
        p.AddAnalogValue("tShimZcc", TOF1Start, (double)Parameters["tMolassesShimZccValue"]);// Shim coils X - Molasses
        p.AddAnalogValue("tShimZcc", TOF2Start, 0); // Step down
        //p.AddLinearRamp("tShimZcc", TOF2Start, 100, 0.0); // Ramp down



        // MOT Laser VCOs
        p.AddAnalogValue("tRbCoolVCO", rbMOTStart, (double)Parameters["tMOTCool"]);// MOT Cooling Frequency
        p.AddAnalogValue("tRbRepVCO", rbMOTStart, (double)Parameters["tMOTRep"]);// MOT Repump Frequency

        // Molasses Laser VCOs
        p.AddAnalogValue("tRbCoolVCO", TOF1Start, (double)Parameters["tMOTCoolMol"]);// Molasses Cooling Frequency
        p.AddAnalogValue("tRbRepVCO", TOF1Start, (double)Parameters["tMOTRepMol"]);// MolassesRepump Frequency

        // Imaging Laser VCOs
        p.AddAnalogValue("tRbCoolVCO", TOF2Start, (double)Parameters["tMOTCoolImg"]);// Img Cooling Frequency
        p.AddAnalogValue("tRbRepVCO", TOF2Start, (double)Parameters["tMOTRepImg"]);// Img Repump Frequency


        return p;
    }

    /*
    public override AnalogStaticBuilder GetAnalogStatic()
    {
        AnalogStaticBuilder p = new AnalogStaticBuilder((int)Parameters["PatternLength"]);

        return p;
    }*/

}
