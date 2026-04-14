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
        Parameters["PatternLength"] = 120000;
        Parameters["TCLBlockStart"] = 0;
        Parameters["tScanCount"] = 0;

        // Offset Lock
        Parameters["tRbOffsetVoltageRep"] = 1.675; // 1.8 V
        Parameters["tRbOffsetVoltageCool"] = 3.2; //3.2 V

        // MOT coils
        Parameters["tMOTccValue"] = 0.48; //0.48
        Parameters["tBlueMOTccValue"] = 0.0; // 1.8 for blue MOT
        Parameters["tcMOTccValue"] = 0.48;
        Parameters["tMOTccSwitch"] = 5.5;

        // Shim Coils Default
        Parameters["tShimXccValue"] = 10.0; // 8.8; //8.8; //  0.0
        Parameters["tShimYccValue"] = -2.0; // 3.8;//3.8; // -2.0
        Parameters["tShimZccValue"] = -0.5; // 0.3; //0.3; // -0.85

        // Shim Coils Molasses
        Parameters["tMolassesShimXccValue"] = 8.8; // 8.8 // 10.0
        Parameters["tMolassesShimYccValue"] = 3.8; // 3.8 // 3.29
        Parameters["tMolassesShimZccValue"] = 0.3; // 0.3 // 0.52

        // tRb AOMs 
        // MOT Cooling and Repump VCO & VVA
        Parameters["tMOTCool"] = 3.8; //Cool at 2.4 - 2.6
        Parameters["tMOTRep"] = 3.0;
        Parameters["tMOTCoolVVA"] = 1.0; // between 0 and 1 V
        Parameters["tMOTRepVVA"] = 1.0;

        // Imaging Cooling and Repump VCO & VVA
        Parameters["tImgCool"] = 1.0; // 3.6 - 4.0, resonance is 4.0
        Parameters["tImgRep"] = 1.0; // 3.0
        Parameters["tImgCoolVVA"] = 1.0;
        Parameters["tImgRepVVA"] = 1.0;

        // Push Out Pulse Cooling and Repump VCO & VVA
        Parameters["tPushCool"] = 3.0; // 3.6 - 4.0, resonance is 4.0
        Parameters["tPushRep"] = 1.0; // 3.0
        Parameters["tPushCoolVVA"] = 1.0;
        Parameters["tPushRepVVA"] = 1.0;

        // Blue Molasses Cooling and Repump VCO and VVA
        Parameters["tBlueMolassesVCOCool"] = 3.0; // 3.0
        Parameters["tBlueMolassesVVA"] = 10.0; //  3.5; // 10.0
        Parameters["tD1SidebandBlueMolasses"] = 0.6;

        // Tweezer Control
        Parameters["tTweezerModulationVCF"] = 0.0;
        Parameters["tTweezerSetVCO"] = 9.0; // 10 -> 110 MHz
        Parameters["tTweezerLoadVCA"] = 1.0; // 1 -> 100% power
        Parameters["tTweezerImgVCA"] = 1.0; // 1 -> 100% power

        // Timing
        Parameters["tRbMOTStart"] = 10;
        Parameters["tDropTime"] = 1000;
        Parameters["tRbRedMOTDuration"] = 50000;
        Parameters["tRbBlueMolassesDuration"] = 2000;
        Parameters["tTweezerPushPulseDelay"] = 1000;
        Parameters["tTweezerPushPulseDuration"] = 5;
        Parameters["tImgTOF"] = 1000;
        Parameters["tMOTExposure"] = 100;
        Parameters["tTweezerExposure"] = 5000;
        Parameters["tTweezerBackgroundDelay"] = 20000;

    }
    // Digital Pattern
    public override PatternBuilder32 GetDigitalPattern()
    {
        int RedMOTStart = (int)Parameters["tRbMOTStart"];
        int BlueMolassesStart = RedMOTStart + (int)Parameters["tRbRedMOTDuration"];
        int PushOutDelayStart = BlueMolassesStart + (int)Parameters["tRbBlueMolassesDuration"];
        int TweezerPushPulseStart = PushOutDelayStart + (int)Parameters["tTweezerPushPulseDelay"];
        int ImgTOFStart = TweezerPushPulseStart + (int)Parameters["tTweezerPushPulseDuration"];
        int ImgStart = ImgTOFStart + (int)Parameters["tImgTOF"];
        int BgTOFStart = ImgStart + (int)Parameters["tTweezerExposure"];
        int BgStart = BgTOFStart + (int)Parameters["tTweezerBackgroundDelay"];
        int End = (int)Parameters["PatternLength"] - 100;

        PatternBuilder32 p = new PatternBuilder32();

        // Analog Board trigger
        p.Pulse(0, 0, 10, "aoTrigger");

        // D1 Light
        p.AddEdge("tD1AOMSwitch", RedMOTStart, true); // Turn off D1 light for sequence
        p.AddEdge("tD1AOMSwitch", BlueMolassesStart, false); // Turn on D1 light for sequence
        p.AddEdge("tD1AOMSwitch", PushOutDelayStart, true); // Turn off D1 light for sequence

        // Red MOT Laser
        p.Pulse(RedMOTStart, 0, (int)Parameters["tRbRedMOTDuration"], "tCoolSwitch"); // Red MOT Cooling Pulse
        p.Pulse(RedMOTStart, 0, (int)Parameters["tRbRedMOTDuration"], "tRepSwitch"); // Red MOT Repump Pulse

        // Tweezer Switch and Modulation
        p.AddEdge("tTweezerModSwitch", RedMOTStart, true); // Turn OFF tweezer modulation at the start

        p.AddEdge("tTweezerSwitch", BlueMolassesStart - (int)Parameters["tDropTime"], true); // Turn OFF tweezer to drop atoms before true loading
        p.AddEdge("tTweezerSwitch", BlueMolassesStart, false); // Turn ON tweezer switch at start of molasses loading
        p.AddEdge("tTweezerSwitch", BgTOFStart, true); // Turn OFF tweezer switch after imaging to drop atoms
        p.AddEdge("tTweezerSwitch", BgStart, false); // Turn ON tweezer switch after background delay to img background

        // Push out pulse
        p.Pulse(TweezerPushPulseStart, 0, (int)Parameters["tTweezerPushPulseDuration"], "tCoolSwitch"); // Red MOT Cooling Pulse
        p.Pulse(TweezerPushPulseStart, 0, (int)Parameters["tTweezerPushPulseDuration"], "tRepSwitch"); // Red MOT Repump Pulse

        // Imaging
        p.Pulse(ImgStart, 0, (int)Parameters["tTweezerExposure"], "tCoolSwitch"); // Red MOT cooling imaging pulse - CHANGE EXPOSURE DEPENDING ON WHAT IS BEING IMAGED
        p.Pulse(ImgStart, 0, (int)Parameters["tTweezerExposure"], "tRepSwitch"); // Red MOT repump imaging pulse - CHANGE EXPOSURE DEPENDING ON WHAT IS BEING IMAGED
        p.Pulse(BgStart, 0, (int)Parameters["tTweezerExposure"], "tCoolSwitch"); // Red MOT cooling background imaging pulse
        p.Pulse(BgStart, 0, (int)Parameters["tTweezerExposure"], "tRepSwitch"); // Red MOT repump background imaging pulse

        p.Pulse(ImgStart, 0, 100, "tMOTCamTrig"); // Trigger MOT camera
        p.Pulse(ImgStart, 0, (int)Parameters["tTweezerExposure"], "tHamCamTrig"); // Trigger tweezer camera for image
        p.Pulse(BgStart, 0, (int)Parameters["tTweezerExposure"], "tHamCamTrig"); // Trigger tweezer camera for background

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
        int BlueMolassesStart = RedMOTStart + (int)Parameters["tRbRedMOTDuration"];
        int PushOutDelayStart = BlueMolassesStart + (int)Parameters["tRbBlueMolassesDuration"];
        int TweezerPushPulseStart = PushOutDelayStart + (int)Parameters["tTweezerPushPulseDelay"];
        int ImgTOFStart = TweezerPushPulseStart + (int)Parameters["tTweezerPushPulseDuration"];
        int ImgStart = ImgTOFStart + (int)Parameters["tImgTOF"];
        int BgTOFStart = ImgStart + (int)Parameters["tTweezerExposure"];
        int BgStart = BgTOFStart + (int)Parameters["tTweezerBackgroundDelay"];
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
        p.AddChannel("tTweezerModVCF");

        // General
        p.AddAnalogValue("tRbD2RepOffset", RedMOTStart, (double)Parameters["tRbOffsetVoltageRep"]); // Offset Lock
        p.AddAnalogValue("tRbD2CoolOffset", RedMOTStart, (double)Parameters["tRbOffsetVoltageCool"]); // Offset Lock

        // MOT Coils
        p.AddAnalogValue("tMOTccSwitch", RedMOTStart, 0.0); // MOT coils switch-on (RSD)
        p.AddAnalogValue("tMOTccSwitch", BlueMolassesStart, (double)Parameters["tMOTccSwitch"]); // MOT coils switch-off (RSD) 

        p.AddAnalogValue("tMOTcc", RedMOTStart, (double)Parameters["tMOTccValue"]); // MOT Mag field 
        p.AddLinearRamp("tMOTcc", BlueMolassesStart - 100, 100, 0.01); // Ramp down magnetic field 

        // Shim Coils
        p.AddAnalogValue("tShimXcc", RedMOTStart, (double)Parameters["tShimXccValue"]); // Default shim coils X
        p.AddAnalogValue("tShimYcc", RedMOTStart, (double)Parameters["tShimYccValue"]); // Default shim coils Y
        p.AddAnalogValue("tShimZcc", RedMOTStart, (double)Parameters["tShimZccValue"]); // Default shim coils Z

        p.AddAnalogValue("tShimXcc", BlueMolassesStart - 100, (double)Parameters["tMolassesShimXccValue"]); // Molasses shim coils X
        p.AddAnalogValue("tShimYcc", BlueMolassesStart - 100, (double)Parameters["tMolassesShimYccValue"]); // Molasses shim coils Y
        p.AddAnalogValue("tShimZcc", BlueMolassesStart - 100, (double)Parameters["tMolassesShimZccValue"]); // Molasses shim coils Z

        p.AddAnalogValue("tShimXcc", End, 0.01); // End shim coils X
        p.AddAnalogValue("tShimYcc", End, 0.01); // End shim coils Y
        p.AddAnalogValue("tShimZcc", End, 0.01); // End shim coils Z

        // Red MOT 
        p.AddAnalogValue("tRbCoolVCO", RedMOTStart, (double)Parameters["tMOTCool"]);
        p.AddAnalogValue("tRbRepVCO", RedMOTStart, (double)Parameters["tMOTRep"]);
        p.AddAnalogValue("tRbD2CoolVVA", RedMOTStart, (double)Parameters["tMOTCoolVVA"]);
        p.AddAnalogValue("tRbD2RepVVA", RedMOTStart, (double)Parameters["tMOTRepVVA"]);

        // Push Out pulse
        p.AddAnalogValue("tRbCoolVCO", PushOutDelayStart + 100, (double)Parameters["tPushCool"]);
        p.AddAnalogValue("tRbRepVCO", PushOutDelayStart + 100, (double)Parameters["tPushRep"]);
        p.AddAnalogValue("tRbD2CoolVVA", PushOutDelayStart + 100, (double)Parameters["tPushCoolVVA"]);
        p.AddAnalogValue("tRbD2RepVVA", PushOutDelayStart + 100, (double)Parameters["tPushRepVVA"]);

        // Imaging 
        p.AddAnalogValue("tRbCoolVCO", ImgTOFStart, (double)Parameters["tImgCool"]);
        p.AddAnalogValue("tRbRepVCO", ImgTOFStart, (double)Parameters["tImgRep"]);
        p.AddAnalogValue("tRbD2CoolVVA", ImgTOFStart, (double)Parameters["tImgCoolVVA"]);
        p.AddAnalogValue("tRbD2RepVVA", ImgTOFStart, (double)Parameters["tImgRepVVA"]);

        // Tweezer
        p.AddAnalogValue("tTweezerModVCF", RedMOTStart, (double)Parameters["tTweezerModulationVCF"]);
        p.AddAnalogValue("tTweezerVCO", RedMOTStart, (double)Parameters["tTweezerSetVCO"]);
        p.AddAnalogValue("tTweezerVCA", RedMOTStart, (double)Parameters["tTweezerLoadVCA"]);
        p.AddAnalogValue("tTweezerVCA", ImgTOFStart, (double)Parameters["tTweezerImgVCA"]); // Shift tweezer power to imaging power for consistent AC Stark shift

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
