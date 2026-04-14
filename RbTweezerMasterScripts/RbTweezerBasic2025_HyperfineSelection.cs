using DAQ.Analog;
using DAQ.Pattern;
using MOTMaster;
using System.Collections.Generic;


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
        Parameters["tScanCount"] = 0;

        // Offset Lock
        Parameters["tRbOffsetVoltageRep"] = 1.675; // 1.8 V
        Parameters["tRbOffsetVoltageCool"] = 3.3; //3.2 V

        // MOT coils
        Parameters["tMOTccValue"] = 0.48; //0.48
        Parameters["tBlueMOTccValue"] = 0.0; // 1.8 for blue MOT
        Parameters["tcMOTccValue"] = 0.48;
        Parameters["tMOTccSwitch"] = 5.5;

        // Shim Coils Default
        Parameters["tShimXccValue"] = 7.5; // 8.8; //8.8; //  0.0
        Parameters["tShimYccValue"] = -2.5; // 3.8;//3.8; // -2.0
        Parameters["tShimZccValue"] = -0.4; // 0.3; //0.3; // -0.85

        // Shim Coils Molasses
        Parameters["tMolassesShimXccValue"] = 9.0; // 8.8 // 10.0
        Parameters["tMolassesShimYccValue"] = 3.29; // 3.8 // 3.29
        Parameters["tMolassesShimZccValue"] = 0.48; // 0.3 // 0.52

        // Shim Coils Blue MOT
        Parameters["tBlueMOTShimXccValue"] = 0.0; //0.0; // 8.0
        Parameters["tBlueMOTShimYccValue"] = 0.0; //2.5; // -5.0
        Parameters["tBlueMOTShimZccValue"] = 0.0; //-1.0; // -7.0

        // tRb AOMs 
        // MOT Cooling and Repump VCO & VVA
        Parameters["tMOTCool"] = 3.8; //Cool at 2.4 - 2.6
        Parameters["tMOTRep"] = 3.0;
        Parameters["tMOTCoolVVA"] = 2.0; // between 0 and 1 V
        Parameters["tMOTRepVVA"] = 2.0;

        // Imaging Cooling and Repump VCO & VVA
        Parameters["tImgCool"] = 0.5; // 3.6 - 4.0, resonance is 4.0
        Parameters["tImgRep"] = 1.5; // 3.0
        Parameters["tImgCoolVVA"] = 0.6;
        Parameters["tImgRepVVA"] = 1.2;

        // Molasses Cooling and Repump VCO & VVA
        Parameters["tMolCool"] = 0.2;
        Parameters["tMolRep"] = 2.5;
        Parameters["tMolCoolVVA"] = 0.7;
        Parameters["tMolRepVVA"] = 0.8;

        // Pump and Push VCO & VVA
        Parameters["tPushVCO"] = 5.0;
        Parameters["tPumpVCO"] = 2.5;
        Parameters["tPushVVA"] = 2.0;
        Parameters["tPumpVVA"] = 0.7;

        // Blue MOT Cooling and Repump VCO and VVA
        Parameters["tBlueMotVCO"] = 3.0; // 5.0 for blue MOT
        Parameters["tBlueMotVCORamp"] = 3.0; // 5.0 for blue MOT
        Parameters["tBlueMotVVA"] = 10.0; //  3.5;// 6.5 for blue MOT
        Parameters["tBlueMotVVARamp"] = 10.0; //  3.5;// 6.5 for blue MOT
        Parameters["tD1SidebandBlueMOT"] = 0.24; // 0.24 for blue MOT 

        // Blue Molasses Cooling and Repump VCO and VVA
        Parameters["tBlueMolassesVCOCool"] = 3.0; // 3.0
        Parameters["tBlueMolassesVCOCoolRamp"] = 3.0; // 6.0
        Parameters["tBlueMolassesVVA"] = 10.0; //  3.5; // 10.0
        Parameters["tBlueMolassesVVARamp"] = 10.0; //  3.5; // 4.0, 10.0
        Parameters["tD1SidebandBlueMolasses"] = 0.5;

        // Tweezer Control
        Parameters["tTweezerSetVCO"] = 9.0; // 10 -> 110 MHz
        Parameters["tTweezerLoadVCO"] = 9.0; // 
        Parameters["tTweezerImgVCO"] = 9.0; // 1 -> 100% power

        // Timing
        Parameters["tRbMOTStart"] = 10;
        Parameters["tDropTime"] = 10;
        Parameters["tRbRedMOTDuration"] = 50000;
        Parameters["tMolassesDelay"] = 1;
        Parameters["tRbRedMolassesDuration"] = 20000;
        Parameters["TOF1"] = 1;
        Parameters["TOF2"] = 10;
        Parameters["PumpPulseDuration"] = 200;
        Parameters["TOF3"] = 100;
        Parameters["PushPulseDuration"] = 5;
        Parameters["TOF4"] = 3000;
        Parameters["tMOTExposure"] = 200;
        Parameters["tTweezerExposure"] = 10000;

    }
    // Digital Pattern
    public override PatternBuilder32 GetDigitalPattern()
    {
        int RedMOTStart = (int)Parameters["tRbMOTStart"];
        int RedMolDelayStart = RedMOTStart + (int)Parameters["tRbRedMOTDuration"];
        int RedMolassesStart = RedMolDelayStart + (int)Parameters["tMolassesDelay"];
        int TOF1Start = RedMolassesStart + (int)Parameters["tRbRedMolassesDuration"];
        int Img1Start = TOF1Start + (int)Parameters["TOF1"];
        int TOF2Start = Img1Start + (int)Parameters["tTweezerExposure"];
        int PumpPulseStart = TOF2Start + (int)Parameters["TOF2"];
        int TOF3Start = PumpPulseStart + (int)Parameters["PumpPulseDuration"];
        int PushPulseStart = TOF3Start + (int)Parameters["TOF3"];
        int TOF4Start = PushPulseStart + (int)Parameters["PushPulseDuration"];
        int Img2Start = TOF4Start + (int)Parameters["TOF4"];
        int End = (int)Parameters["PatternLength"] - 100;

        PatternBuilder32 p = new PatternBuilder32();

        // Analog Board trigger
        p.Pulse(0, 0, 10, "aoTrigger");
        p.AddEdge("tVerticalShutter", RedMOTStart, true);

        // D1 Light
        p.AddEdge("tD1AOMSwitch", RedMOTStart, true); // Turn off D1 light for sequence

        // Red MOT Laser
        p.Pulse(RedMOTStart, 0, (int)Parameters["tRbRedMOTDuration"], "tCoolSwitch"); // Red MOT Coolng Pulse
        p.Pulse(RedMOTStart, 0, (int)Parameters["tRbRedMOTDuration"], "tRepSwitch"); // Red MOT Repump Pulse

        // Red Molasses Laser
        p.Pulse(RedMolassesStart, 0, (int)Parameters["tRbRedMolassesDuration"], "tCoolSwitch"); // Red Molasses Coolng Pulse
        p.Pulse(RedMolassesStart, 0, (int)Parameters["tRbRedMolassesDuration"], "tRepSwitch"); // Red Molasses Repump Pulse

        // Tweezer
        p.AddEdge("tTweezerModSwitch", RedMOTStart, true); // Turn OFF tweezer modulation 
        p.AddEdge("tTweezerSwitch", RedMolassesStart - (int)Parameters["tDropTime"], true); // Turn OFF tweezer to drop atoms before true loading
        p.AddEdge("tTweezerSwitch", RedMolassesStart, false); // Turn ON tweezer switch at start of molasses loading

        // Imaging 1
        p.Pulse(Img1Start, 0, (int)Parameters["tTweezerExposure"], "tCoolSwitch"); // Red MOT cooling imaging pulse - CHANGE EXPOSURE DEPENDING ON WHAT IS BEING IMAGED
        p.Pulse(Img1Start, 0, (int)Parameters["tTweezerExposure"], "tRepSwitch"); // Red MOT repump imaging pulse - CHANGE EXPOSURE DEPENDING ON WHAT IS BEING IMAGED

        // Optical Pump Pulse
        p.Pulse(PumpPulseStart, 0, (int)Parameters["PumpPulseDuration"], "tRepSwitch"); // Red Molasses Coolng Pulse

        // Push Pulse
        p.AddEdge("tVerticalShutter", TOF3Start, false);
        p.Pulse(PushPulseStart, 0, (int)Parameters["PushPulseDuration"], "tCoolSwitch"); // Red Molasses Coolng Pulse
        p.AddEdge("tVerticalShutter", TOF4Start, true);

        // Imaging 2
        p.Pulse(Img2Start, 0, (int)Parameters["tTweezerExposure"], "tCoolSwitch"); // Red MOT cooling background imaging pulse
        p.Pulse(Img2Start, 0, (int)Parameters["tTweezerExposure"], "tRepSwitch"); // Red MOT repump background imaging pulse

        // Camera Triggering
        //p.Pulse(Img1Start, 0, 100, "tMOTCamTrig"); // Trigger MOT camera
        p.Pulse(Img1Start, 0, (int)Parameters["tTweezerExposure"], "tHamCamTrig"); // Trigger tweezer camera for image
        p.Pulse(Img2Start, 0, (int)Parameters["tTweezerExposure"], "tHamCamTrig"); // Trigger tweezer camera for background

        // END
        p.AddEdge("tCoolSwitch", End, true);
        p.AddEdge("tRepSwitch", End, true);
        p.AddEdge("tD1AOMSwitch", End, false); // Turn on D1 light

        return p;
    }

    // Analog Pattern

    public override AnalogPatternBuilder GetAnalogPattern()
    {
        int RedMOTStart = (int)Parameters["tRbMOTStart"];
        int RedMolDelayStart = RedMOTStart + (int)Parameters["tRbRedMOTDuration"];
        int RedMolassesStart = RedMolDelayStart + (int)Parameters["tMolassesDelay"];
        int TOF1Start = RedMolassesStart + (int)Parameters["tRbRedMolassesDuration"];
        int Img1Start = TOF1Start + (int)Parameters["TOF1"];
        int TOF2Start = Img1Start + (int)Parameters["tTweezerExposure"];
        int PumpPulseStart = TOF2Start + (int)Parameters["TOF2"];
        int TOF3Start = PumpPulseStart + (int)Parameters["PumpPulseDuration"];
        int PushPulseStart = TOF3Start + (int)Parameters["TOF3"];
        int TOF4Start = PushPulseStart + (int)Parameters["PushPulseDuration"];
        int Img2Start = TOF4Start + (int)Parameters["TOF4"];
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
        p.AddChannel("tRbD2CoolVVA");
        p.AddChannel("tRbD2RepVVA");
        p.AddChannel("tRbD2RepOffset");
        p.AddChannel("tRbD2CoolOffset");
        p.AddChannel("tTweezerVCO");
        p.AddChannel("tTweezerVCA");

        // General
        p.AddAnalogValue("tRbD2RepOffset", RedMOTStart, (double)Parameters["tRbOffsetVoltageRep"]); // Offset Lock
        p.AddAnalogValue("tRbD2CoolOffset", RedMOTStart, (double)Parameters["tRbOffsetVoltageCool"]); // Offset Lock

        // MOT Coils
        p.AddAnalogValue("tMOTccSwitch", RedMOTStart, 0.0); // MOT coils switch-on (RSD)
        p.AddAnalogValue("tMOTccSwitch", RedMolassesStart, (double)Parameters["tMOTccSwitch"]); // MOT coils switch-off (RSD)

        p.AddAnalogValue("tMOTcc", RedMOTStart, (double)Parameters["tMOTccValue"]); // MOT Mag field 
        p.AddLinearRamp("tMOTcc", RedMolDelayStart, (int)Parameters["tMolassesDelay"], 0.01); // Ramp down magnetic field
        //p.AddAnalogValue("tMOTcc", RedMolDelayStart, 0.01);

        // Shim Coils
        p.AddAnalogValue("tShimXcc", RedMOTStart, (double)Parameters["tShimXccValue"]); // Default shim coils X
        p.AddAnalogValue("tShimYcc", RedMOTStart, (double)Parameters["tShimYccValue"]); // Default shim coils Y
        p.AddAnalogValue("tShimZcc", RedMOTStart, (double)Parameters["tShimZccValue"]); // Default shim coils Z

        p.AddAnalogValue("tShimXcc", RedMolDelayStart, (double)Parameters["tMolassesShimXccValue"]); // Molasses shim coils X
        p.AddAnalogValue("tShimYcc", RedMolDelayStart, (double)Parameters["tMolassesShimYccValue"]); // Molasses shim coils Y
        p.AddAnalogValue("tShimZcc", RedMolDelayStart, (double)Parameters["tMolassesShimZccValue"]); // Molasses shim coils Z

        p.AddAnalogValue("tShimXcc", End, 0.01); // End shim coils X
        p.AddAnalogValue("tShimYcc", End, 0.01); // End shim coils Y
        p.AddAnalogValue("tShimZcc", End, 0.01); // End shim coils Z

        // Red MOT 
        p.AddAnalogValue("tRbCoolVCO", RedMOTStart, (double)Parameters["tMOTCool"]);
        p.AddAnalogValue("tRbRepVCO", RedMOTStart, (double)Parameters["tMOTRep"]);
        p.AddAnalogValue("tRbD2CoolVVA", RedMOTStart, (double)Parameters["tMOTCoolVVA"]);
        p.AddAnalogValue("tRbD2RepVVA", RedMOTStart, (double)Parameters["tMOTRepVVA"]);

        // Red Molasses 
        p.AddAnalogValue("tRbCoolVCO", RedMolDelayStart, (double)Parameters["tMolCool"]);
        p.AddAnalogValue("tRbRepVCO", RedMolDelayStart, (double)Parameters["tMolRep"]);
        p.AddAnalogValue("tRbD2CoolVVA", RedMolDelayStart, (double)Parameters["tMolCoolVVA"]);
        p.AddAnalogValue("tRbD2RepVVA", RedMolDelayStart, (double)Parameters["tMolRepVVA"]);

        // Pump Pulses
        p.AddAnalogValue("tRbCoolVCO", TOF2Start, (double)Parameters["tPumpVCO"]);
        p.AddAnalogValue("tRbD2CoolVVA", TOF2Start, (double)Parameters["tPumpVVA"]);

        // Push Pulses
        p.AddAnalogValue("tRbCoolVCO", TOF3Start, (double)Parameters["tPushVCO"]);
        p.AddAnalogValue("tRbD2CoolVVA", TOF3Start, (double)Parameters["tPushVVA"]);

        // Imaging 
        p.AddAnalogValue("tRbCoolVCO", TOF1Start, (double)Parameters["tImgCool"]);
        p.AddAnalogValue("tRbRepVCO", TOF1Start, (double)Parameters["tImgRep"]);
        p.AddAnalogValue("tRbD2CoolVVA", TOF1Start, (double)Parameters["tImgCoolVVA"]);
        p.AddAnalogValue("tRbD2RepVVA", TOF1Start, (double)Parameters["tImgRepVVA"]);

        p.AddAnalogValue("tRbCoolVCO", TOF4Start, (double)Parameters["tImgCool"]);
        p.AddAnalogValue("tRbRepVCO", TOF4Start, (double)Parameters["tImgRep"]);
        p.AddAnalogValue("tRbD2CoolVVA", TOF4Start, (double)Parameters["tImgCoolVVA"]);
        p.AddAnalogValue("tRbD2RepVVA", TOF4Start, (double)Parameters["tImgRepVVA"]);

        // Tweezer
        p.AddAnalogValue("tTweezerVCO", RedMOTStart, (double)Parameters["tTweezerSetVCO"]);
        p.AddAnalogValue("tTweezerVCO", RedMolassesStart, (double)Parameters["tTweezerLoadVCO"]);
        p.AddAnalogValue("tTweezerVCO", TOF1Start, (double)Parameters["tTweezerImgVCO"]); // Shift tweezer power to imaging power for consistent AC Stark shift

        // D1 Light
        p.AddAnalogValue("tRbD1VCO", RedMOTStart, (double)Parameters["tBlueMolassesVCOCool"]); // D1 AOM VCO Cooling
        p.AddAnalogValue("tRbD1VVA", RedMOTStart, (double)Parameters["tBlueMolassesVVA"]);// D1 AOM VVA
        p.AddAnalogValue("tD1SidebandControl", RedMOTStart, (double)Parameters["tD1SidebandBlueMolasses"]);

        // D2 Light End
        p.AddAnalogValue("tRbCoolVCO", End, (double)Parameters["tMOTCool"]);
        p.AddAnalogValue("tRbRepVCO", End, (double)Parameters["tMOTRep"]);
        p.AddAnalogValue("tRbD2CoolVVA", End, (double)Parameters["tMOTCoolVVA"]);
        p.AddAnalogValue("tRbD2RepVVA", End, (double)Parameters["tMOTRepVVA"]);



        return p;
    }

    /*
    public override AnalogStaticBuilder GetAnalogStatic()
    {
        AnalogStaticBuilder p = new AnalogStaticBuilder((int)Parameters["PatternLength"]);

        return p;
    }*/

}
