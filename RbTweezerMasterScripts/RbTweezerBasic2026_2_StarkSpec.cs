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
        Parameters["PatternLength"] = 105000;
        Parameters["TCLBlockStart"] = 0;
        Parameters["tScanCount"] = 0;
        Parameters["BackgroundDrop"] = 0;
        Parameters["TweezerModulation"] = 0;

        // Offset Lock
        Parameters["tRbOffsetVoltageRep"] = 1.675; // 1.8 V
        Parameters["tRbOffsetVoltageCool"] = 3.3; //3.2 V

        // MOT coils
        Parameters["tMOTccValue"] = 0.48; //0.48
        Parameters["tcMOTccValue"] = 0.48;
        Parameters["tMOTccSwitch"] = 5.5;

        // Shim Coils Default
        Parameters["tMOTShimXccValue"] = 8.00; // 8.8; //8.8; //  0.0
        Parameters["tMOTShimYccValue"] = -2.369; // 3.8;//3.8; // -2.0
        Parameters["tMOTShimZccValue"] = 0.10; // 0.3; //0.3; // -0.85

        // Shim Coils Molasses
        Parameters["tMolassesShimXccValue"] = 10.0000; // 8.8 // 10.0
        Parameters["tMolassesShimYccValue"] = 3.2271; // 3.8 // 3.29
        Parameters["tMolassesShimZccValue"] = 0.4492; // 0.3 // 0.52

        Parameters["tBiasShimZccValue"] = 0.96;

        // tRb AOMs 
        // MOT Cooling and Repump - Loading
        Parameters["tMOTLoadCoolVCO"] = 4.1813; //Cool at 2.4 - 2.6
        Parameters["tMOTLoadRepVCO"] = 3.00;
        Parameters["tMOTLoadCoolVVA"] = 1.1702; // between 0 and 1 V
        Parameters["tMOTLoadRepVVA"] = 2.00;

        // MOT Cooling and Repump - Compression
        Parameters["tcMOTLoadCoolVCO"] = 3.3768; //Cool at 2.4 - 2.6
        Parameters["tcMOTLoadRepVCO"] = 3.00;
        Parameters["tcMOTLoadCoolVVA"] = 1.1702; // between 0 and 1 V
        Parameters["tcMOTLoadRepVVA"] = 2.00;

        // Molasses Cooling and Repump VCO & VVA
        Parameters["tMolCoolVCO1"] = 1.6361;
        Parameters["tMolRepVCO1"] = 3.00;
        Parameters["tMolCoolVVA1"] = 0.5132;
        Parameters["tMolRepVVA1"] = 0.41;

        Parameters["tMolCoolVCO2"] = 1.0828;
        Parameters["tMolRepVCO2"] = 3.00;
        Parameters["tMolCoolVVA2"] = 0.4545;
        Parameters["tMolRepVVA2"] = 0.41;

        // Imaging Cooling and Repump VCO & VVA
        Parameters["tImgCoolVCO"] = 0.911; // 3.6 - 4.0, resonance is 4.0
        Parameters["tImgRepVCO"] = 2.00; // 3.0
        Parameters["tImgCoolVVA"] = 0.43;
        Parameters["tImgRepVVA"] = 0.42;

        // Pump and Push VCO & VVA
        Parameters["tPushCoolVCO"] = 4.5;
        Parameters["tPushCoolVVA"] = 2.0;

        Parameters["tPumpCoolVCO"] = 5.0;
        Parameters["tPumpCoolVVA"] = 10.0;
        Parameters["tPumpRepVCO"] = 3.0;
        Parameters["tPumpRepVVA"] = 2.0;

        Parameters["tProbeCoolVCO"] = 10.0;
        Parameters["tProbeCoolVVA"] = 3.0;

        // Tweezer Control
        Parameters["tTweezerSetVCO"] = 9.0; // 10 -> 110 MHz
        Parameters["tTweezerLoadVCO"] = 9.0; // 
        Parameters["tTweezerImgVCO"] = 9.0; // 1 -> 100% power
        Parameters["tTweezerSetVVA0"] = 0.0;
        Parameters["tTweezerSetVVA1"] = 0.0;
        Parameters["tTweezerModulationVCF"] = 10.0;

        // Timing
        Parameters["tMOTLoadingStart"] = 10;
        Parameters["tMOTLoadingDuration"] = 60000;
        Parameters["tMOTCompressionDuration"] = 10000;
        Parameters["tMolassesDuration"] = 10000;
        Parameters["tDelay1"] = 1000;
        Parameters["tTweezerExposure1"] = 10000;
        Parameters["tDelay2"] = 1000;
        Parameters["tSpecPumpDuration"] = 10;
        Parameters["tSpecProbeDuration"] = 70;
        Parameters["tSpecPurgeDuration"] = 500;
        Parameters["tDelay3"] = 1000;
        Parameters["tTweezerExposure2"] = 10000;

        Parameters["tMOTExposure"] = 1;
        Parameters["tTweezerModulationDuration"] = 1000;
    }

    // Digital Pattern
    public override PatternBuilder32 GetDigitalPattern()
    {
        int MOTLoadingStart = (int)Parameters["tMOTLoadingStart"];
        int MOTCompressionStart = MOTLoadingStart + (int)Parameters["tMOTLoadingDuration"];
        int MolassesStart = MOTCompressionStart + (int)Parameters["tMOTCompressionDuration"] + 1;
        int Delay1Start = MolassesStart + (int)Parameters["tMolassesDuration"];
        int Img1Start = Delay1Start + (int)Parameters["tDelay1"];
        int Delay2Start = Img1Start + (int)Parameters["tTweezerExposure1"];
        int SpecPumpStart = Delay2Start + (int)Parameters["tDelay2"];
        int SpecProbeStart = SpecPumpStart + (int)Parameters["tSpecPumpDuration"] + 10;
        int SpecPurgeStart = SpecProbeStart + (int)Parameters["tSpecProbeDuration"];
        int Delay3Start = SpecPurgeStart + (int)Parameters["tSpecPurgeDuration"];
        int Img2Start = Delay3Start + (int)Parameters["tDelay3"];
        int End = (int)Parameters["PatternLength"] - 100;

        PatternBuilder32 p = new PatternBuilder32();

        // Analog Board trigger
        p.Pulse(0, 0, 10, "aoTrigger");
        p.AddEdge("tVerticalShutter", MOTLoadingStart, true);

        // D1 Light
        p.AddEdge("tD1AOMSwitch", MOTLoadingStart, true); // Turn off D1 light for sequence

        // Rb87 D2 Cooling and Repump Light - Loading Stage
        p.AddEdge("tCoolSwitch", MOTLoadingStart, true);
        p.AddEdge("tRepSwitch", MOTLoadingStart, true);

        p.AddEdge("tCoolSwitch", Delay1Start, false);
        p.AddEdge("tRepSwitch", Delay1Start, false);

        // Tweezer
        p.AddEdge("tTweezerModSwitch", MOTLoadingStart, true); // Turn OFF tweezer modulation 

        if ((int)Parameters["TweezerModulation"] == 1)
        {
            p.AddEdge("tTweezerModSwitch", Delay2Start, false); // Turn ON tweezer modulation
            p.AddEdge("tTweezerModSwitch", Delay2Start + (int)Parameters["tTweezerModulationDuration"], true); // Turn OFF tweezer modulation
        }

        p.AddEdge("tTweezerSwitch", MOTLoadingStart, true); // Turn OFF tweezer light 
        p.AddEdge("tTweezerSwitch", MOTCompressionStart, false); // Turn ON tweezer light for loading

        if ((int)Parameters["BackgroundDrop"] == 1) 
        {
            p.AddEdge("tTweezerSwitch", Delay2Start, true); 
            p.AddEdge("tTweezerSwitch", Img2Start, false);
        }

        // Imaging 1
        p.Pulse(Img1Start, 0, (int)Parameters["tTweezerExposure1"] - 500, "tCoolSwitch"); 
        p.Pulse(Img1Start, 0, (int)Parameters["tTweezerExposure1"], "tRepSwitch"); 
        p.Pulse(Img1Start, 0, (int)Parameters["tTweezerExposure1"], "tHamCamTrig");

        // Pump and Knockout Spectroscopy
        p.AddEdge("tVerticalShutter", Delay2Start, false);

        p.AddEdge("tD1AOMSwitch", SpecPumpStart, false);
        p.AddEdge("tD1AOMSwitch", SpecPumpStart + (int)Parameters["tSpecPumpDuration"], true);
        p.Pulse(SpecPumpStart, 0, (int)Parameters["tSpecPumpDuration"], "tRepSwitch");

        p.AddEdge("tD1AOMSwitch", SpecProbeStart, false);
        p.AddEdge("tD1AOMSwitch", SpecProbeStart + (int)Parameters["tSpecProbeDuration"], true);

        p.Pulse(SpecPurgeStart, 0, (int)Parameters["tSpecPurgeDuration"], "tCoolSwitch");

        p.AddEdge("tVerticalShutter", Delay3Start, true);

        // Imaging 2
        p.Pulse(Img2Start, 0, (int)Parameters["tTweezerExposure2"], "tCoolSwitch"); 
        p.Pulse(Img2Start, 0, (int)Parameters["tTweezerExposure2"], "tRepSwitch"); 
        p.Pulse(Img2Start, 0, (int)Parameters["tTweezerExposure2"], "tHamCamTrig"); 

        // END
        p.AddEdge("tCoolSwitch", End, true);
        p.AddEdge("tRepSwitch", End, true);
        p.AddEdge("tD1AOMSwitch", End, false); // Turn on D1 light

        return p;
    }

    // Analog Pattern

    public override AnalogPatternBuilder GetAnalogPattern()
    {
        int MOTLoadingStart = (int)Parameters["tMOTLoadingStart"];
        int MOTCompressionStart = MOTLoadingStart + (int)Parameters["tMOTLoadingDuration"];
        int MolassesStart = MOTCompressionStart + (int)Parameters["tMOTCompressionDuration"] + 1;
        int Delay1Start = MolassesStart + (int)Parameters["tMolassesDuration"];
        int Img1Start = Delay1Start + (int)Parameters["tDelay1"];
        int Delay2Start = Img1Start + (int)Parameters["tTweezerExposure1"];
        int SpecPumpStart = Delay2Start + (int)Parameters["tDelay2"];
        int SpecProbeStart = SpecPumpStart + (int)Parameters["tSpecPumpDuration"] + 10;
        int SpecPurgeStart = SpecProbeStart + (int)Parameters["tSpecProbeDuration"];
        int Delay3Start = SpecPurgeStart + (int)Parameters["tSpecPurgeDuration"];
        int Img2Start = Delay3Start + (int)Parameters["tDelay3"];
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
        p.AddAnalogValue("tRbD2RepOffset", MOTLoadingStart, (double)Parameters["tRbOffsetVoltageRep"]); // Offset Lock
        p.AddAnalogValue("tRbD2CoolOffset", MOTLoadingStart, (double)Parameters["tRbOffsetVoltageCool"]); // Offset Lock

        // MOT Coils - Switch
        p.AddAnalogValue("tMOTccSwitch", MOTLoadingStart, 0.0); // MOT coils switch-on (RSD)
        p.AddAnalogValue("tMOTccSwitch", MolassesStart, (double)Parameters["tMOTccSwitch"]); // MOT coils switch-off (RSD)

        // MOT Coils - Current Control
        p.AddAnalogValue("tMOTcc", MOTLoadingStart, (double)Parameters["tMOTccValue"]); // MOT Mag field 
        p.AddLinearRamp("tMOTcc", MOTCompressionStart, (int)Parameters["tMOTCompressionDuration"], (double)Parameters["tcMOTccValue"]); // Ramp up magnetic field
        p.AddAnalogValue("tMOTcc", MolassesStart, 0.0);

        // Shim Coils - Current Control
        p.AddAnalogValue("tShimXcc", MOTLoadingStart, (double)Parameters["tMOTShimXccValue"]); // Default shim coils X
        p.AddAnalogValue("tShimYcc", MOTLoadingStart, (double)Parameters["tMOTShimYccValue"]); // Default shim coils Y
        p.AddAnalogValue("tShimZcc", MOTLoadingStart, (double)Parameters["tMOTShimZccValue"]); // Default shim coils Z

        p.AddAnalogValue("tShimXcc", MolassesStart, (double)Parameters["tMolassesShimXccValue"]); // Molasses shim coils X
        p.AddAnalogValue("tShimYcc", MolassesStart, (double)Parameters["tMolassesShimYccValue"]); // Molasses shim coils Y
        p.AddAnalogValue("tShimZcc", MolassesStart, (double)Parameters["tMolassesShimZccValue"]); // Molasses shim coils Z

        p.AddAnalogValue("tShimZcc", Delay2Start, (double)Parameters["tBiasShimZccValue"]);
        p.AddAnalogValue("tShimZcc", Delay3Start, (double)Parameters["tMolassesShimZccValue"]);

        p.AddAnalogValue("tShimXcc", End, 0.01); // End shim coils X
        p.AddAnalogValue("tShimYcc", End, 0.01); // End shim coils Y
        p.AddAnalogValue("tShimZcc", End, 0.01); // End shim coils Z

        // MOT Loading
        p.AddAnalogValue("tRbCoolVCO", MOTLoadingStart, (double)Parameters["tMOTLoadCoolVCO"]);
        p.AddAnalogValue("tRbRepVCO", MOTLoadingStart, (double)Parameters["tMOTLoadRepVCO"]);
        p.AddAnalogValue("tRbD2CoolVVA", MOTLoadingStart, (double)Parameters["tMOTLoadCoolVVA"]);
        p.AddAnalogValue("tRbD2RepVVA", MOTLoadingStart, (double)Parameters["tMOTLoadRepVVA"]);

        // MOT Compression
        p.AddLinearRamp("tRbCoolVCO", MOTCompressionStart, (int)Parameters["tMOTCompressionDuration"], (double)Parameters["tcMOTLoadCoolVCO"]);
        p.AddLinearRamp("tRbRepVCO", MOTCompressionStart, (int)Parameters["tMOTCompressionDuration"], (double)Parameters["tcMOTLoadRepVCO"]);
        p.AddLinearRamp("tRbD2CoolVVA", MOTCompressionStart, (int)Parameters["tMOTCompressionDuration"], (double)Parameters["tcMOTLoadCoolVVA"]);
        p.AddLinearRamp("tRbD2RepVVA", MOTCompressionStart, (int)Parameters["tMOTCompressionDuration"], (double)Parameters["tcMOTLoadRepVVA"]);

        // Molasses Ramp 
        p.AddAnalogValue("tRbCoolVCO", MolassesStart -1, (double)Parameters["tMolCoolVCO1"]);
        p.AddAnalogValue("tRbRepVCO", MolassesStart - 1, (double)Parameters["tMolRepVCO1"]);
        p.AddAnalogValue("tRbD2CoolVVA", MolassesStart - 1, (double)Parameters["tMolCoolVVA1"]);
        p.AddAnalogValue("tRbD2RepVVA", MolassesStart - 1, (double)Parameters["tMolRepVVA1"]);

        p.AddLinearRamp("tRbCoolVCO", MolassesStart, (int)Parameters["tMolassesDuration"], (double)Parameters["tMolCoolVCO2"]);
        p.AddLinearRamp("tRbRepVCO", MolassesStart, (int)Parameters["tMolassesDuration"], (double)Parameters["tMolRepVCO2"]);
        p.AddLinearRamp("tRbD2CoolVVA", MolassesStart, (int)Parameters["tMolassesDuration"], (double)Parameters["tMolCoolVVA2"]);
        p.AddLinearRamp("tRbD2RepVVA", MolassesStart, (int)Parameters["tMolassesDuration"], (double)Parameters["tMolRepVVA2"]);

        // Initial Imaging 
        p.AddAnalogValue("tRbCoolVCO", Img1Start, (double)Parameters["tImgCoolVCO"]);
        p.AddAnalogValue("tRbRepVCO", Img1Start, (double)Parameters["tImgRepVCO"]);
        p.AddAnalogValue("tRbD2CoolVVA", Img1Start, (double)Parameters["tImgCoolVVA"]);
        p.AddAnalogValue("tRbD2RepVVA", Img1Start, (double)Parameters["tImgRepVVA"]);

        // Pump, Probe and Purge Pulses
        p.AddAnalogValue("tRbD1VCO", MOTLoadingStart, (double)Parameters["tPumpCoolVCO"]);
        p.AddAnalogValue("tRbD1VVA", MOTLoadingStart, (double)Parameters["tPumpCoolVVA"]);
        p.AddAnalogValue("tRbRepVCO", Img1Start, (double)Parameters["tPumpRepVCO"]);
        p.AddAnalogValue("tRbD2RepVVA", Img1Start, (double)Parameters["tPumpRepVVA"]);

        p.AddAnalogValue("tRbD1VCO", SpecPumpStart + (int)Parameters["tSpecPumpDuration"], (double)Parameters["tProbeCoolVCO"]);
        p.AddAnalogValue("tRbD1VVA", SpecPumpStart + (int)Parameters["tSpecPumpDuration"], (double)Parameters["tProbeCoolVVA"]);

        p.AddAnalogValue("tRbCoolVCO", Delay2Start, (double)Parameters["tPushCoolVCO"]);
        p.AddAnalogValue("tRbD2CoolVVA", Delay2Start, (double)Parameters["tPushCoolVVA"]);

        // Final Imaging 
        p.AddAnalogValue("tRbCoolVCO", Img2Start, (double)Parameters["tImgCoolVCO"]);
        p.AddAnalogValue("tRbRepVCO", Img2Start, (double)Parameters["tImgRepVCO"]);
        p.AddAnalogValue("tRbD2CoolVVA", Img2Start, (double)Parameters["tImgCoolVVA"]);
        p.AddAnalogValue("tRbD2RepVVA", Img2Start, (double)Parameters["tImgRepVVA"]);

        // Tweezer
        p.AddAnalogValue("tTweezerModVCF", MOTLoadingStart, (double)Parameters["tTweezerModulationVCF"]);

        p.AddAnalogValue("tTweezerVCO", MOTLoadingStart, (double)Parameters["tTweezerSetVCO"]);
        p.AddAnalogValue("tTweezerVCO", MOTCompressionStart, (double)Parameters["tTweezerLoadVCO"]);
        p.AddAnalogValue("tTweezerVCO", Img1Start, (double)Parameters["tTweezerImgVCO"]); // Shift tweezer power to imaging power for consistent AC Stark shift

        p.AddAnalogValue("tTweezerVCA", MOTLoadingStart, (double)Parameters["tTweezerSetVVA0"]);
        p.AddLinearRamp("tTweezerVCA", Delay2Start, (int)Parameters["tDelay2"], (double)Parameters["tTweezerSetVVA1"]);
        p.AddAnalogValue("tTweezerVCA", End, (double)Parameters["tTweezerSetVVA0"]);

        // D2 Light End
        p.AddAnalogValue("tRbCoolVCO", End, (double)Parameters["tMOTLoadCoolVCO"]);
        p.AddAnalogValue("tRbRepVCO", End, (double)Parameters["tMOTLoadRepVCO"]);
        p.AddAnalogValue("tRbD2CoolVVA", End, (double)Parameters["tMOTLoadCoolVVA"]);
        p.AddAnalogValue("tRbD2RepVVA", End, (double)Parameters["tMOTLoadRepVVA"]);

        return p;
    }

    /*
    public override AnalogStaticBuilder GetAnalogStatic()
    {
        AnalogStaticBuilder p = new AnalogStaticBuilder((int)Parameters["PatternLength"]);

        return p;
    }*/

}
