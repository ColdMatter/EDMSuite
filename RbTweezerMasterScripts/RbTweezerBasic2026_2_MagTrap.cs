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
        Parameters["PatternLength"] = 200000;
        Parameters["TCLBlockStart"] = 0;
        Parameters["tScanCount"] = 0;
        Parameters["BackgroundDrop"] = 1;
        Parameters["TweezerModulation"] = 0;

        // Offset Lock
        Parameters["tRbOffsetVoltageRep"] = 1.675; // 1.8 V
        Parameters["tRbOffsetVoltageCool"] = 3.3; //3.2 V

        // MOT coils
        Parameters["tMOTccValue"] = 0.48; //0.48
        Parameters["tcMOTccValue"] = 0.48;
        Parameters["tMOTccSwitch"] = 5.5;
        Parameters["tIntMagTrapccValue"] = 1.8;
        Parameters["tExtMagTrapccValue"] = 0.0;

        // Shim Coils Default
        Parameters["tMOTShimXccValue"] = 8.00; // 8.8; //8.8; //  0.0
        Parameters["tMOTShimYccValue"] = -2.369; // 3.8;//3.8; // -2.0
        Parameters["tMOTShimZccValue"] = 0.10; // 0.3; //0.3; // -0.85

        // Shim Coils Molasses
        Parameters["tMolassesShimXccValue"] = 10.0000; // 8.8 // 10.0
        Parameters["tMolassesShimYccValue"] = 3.2271; // 3.8 // 3.29
        Parameters["tMolassesShimZccValue"] = 0.4492; // 0.3 // 0.52

        Parameters["tMagTrapShimXccValue"] = 0.0;
        Parameters["tMagTrapShimYccValue"] = 0.0;
        Parameters["tMagTrapShimZccValue"] = 0.0;

        // tRb AOMs 
        // MOT Cooling and Repump - Loading
        Parameters["tMOTLoadCoolVCO"] = 4.1813; //Cool at 2.4 - 2.6
        Parameters["tMOTLoadRepVCO"] = 3.00;
        Parameters["tMOTLoadCoolVVA"] = 1.1702; // between 0 and 1 V
        Parameters["tMOTLoadRepVVA"] = 2.00;

        // MOT Cooling and Repump - Compression
        Parameters["tcMOTLoadCoolVCO"] = 4.1813; //Cool at 2.4 - 2.6
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
        Parameters["tImgCoolVCO1"] = 4.5; // 3.6 - 4.0, resonance is 4.0
        Parameters["tImgRepVCO1"] = 3.00; // 3.0
        Parameters["tImgCoolVVA1"] = 5.00;
        Parameters["tImgRepVVA1"] = 5.00;

        Parameters["tImgCoolVCO2"] = 0.2; // 3.6 - 4.0, resonance is 4.0
        Parameters["tImgRepVCO2"] = 2.00; // 3.0
        Parameters["tImgCoolVVA2"] = 0.44;
        Parameters["tImgRepVVA2"] = 0.41;

        // Tweezer Control
        Parameters["tTweezerSetVCO"] = 9.0; // 10 -> 110 MHz
        Parameters["tTweezerLoadVCO"] = 9.0; // 
        Parameters["tTweezerImgVCO"] = 9.0; // 1 -> 100% power
        Parameters["tTweezerSetVVA0"] = 0.0;
        Parameters["tTweezerSetVVA1"] = 0.0;
        Parameters["tTweezerModulationVCF"] = 10.0;

        // Timing
        Parameters["tMOTLoadingStart"] = 10;
        Parameters["tMOTLoadingDuration"] = 80000;
        Parameters["tMOTCompressionDuration"] = 10000;
        Parameters["tMolassesDuration"] = 1;
        Parameters["tIntMagTrapRampDuration"] = 10;
        Parameters["tIntMagTrapHoldDuration"] = 10000;
        Parameters["tExtMagTrapRampDuration"] = 1;
        Parameters["tExtMagTrapHoldDuration"] = 1;
        Parameters["tIntMagTrapHoldDuration2"] = 1;
        Parameters["tDelay1"] = 1;
        Parameters["tMOTExposure"] = 100;
        Parameters["tTweezerExposure1"] = 100;

        Parameters["tTweezerModulationDuration"] = 1000;
    }

    // Digital Pattern
    public override PatternBuilder32 GetDigitalPattern()
    {
        int MOTLoadingStart = (int)Parameters["tMOTLoadingStart"];
        int MOTCompressionStart = MOTLoadingStart + (int)Parameters["tMOTLoadingDuration"];
        int MolassesStart = MOTCompressionStart + (int)Parameters["tMOTCompressionDuration"];
        int IntMagTrapRampStart = MolassesStart + (int)Parameters["tMolassesDuration"];
        int IntMagTrapHoldStart = IntMagTrapRampStart + (int)Parameters["tIntMagTrapRampDuration"];
        int IntExtMagTrapTransferStart = IntMagTrapHoldStart + (int)Parameters["tIntMagTrapHoldDuration"];
        int ExtMagTrapHoldStart = IntExtMagTrapTransferStart + (int)Parameters["tExtMagTrapRampDuration"];
        int ExtIntTrapTransferStart = ExtMagTrapHoldStart + (int)Parameters["tExtMagTrapHoldDuration"];
        int IntMagTrapHoldStart2 = ExtIntTrapTransferStart + (int)Parameters["tExtMagTrapRampDuration"];
        int Delay1Start = IntMagTrapHoldStart2 + (int)Parameters["tIntMagTrapHoldDuration2"];
        int Img1Start = Delay1Start + (int)Parameters["tDelay1"];
        int End = (int)Parameters["PatternLength"] - 100;

        PatternBuilder32 p = new PatternBuilder32();

        // Analog Board trigger
        p.Pulse(0, 0, 10, "aoTrigger");

        // D1 Light
        p.AddEdge("tD1AOMSwitch", MOTLoadingStart, true); // Turn off D1 light for sequence

        // Rb87 D2 Cooling and Repump Light - Loading Stage
        p.AddEdge("tCoolSwitch", MOTLoadingStart, true);
        p.AddEdge("tRepSwitch", MOTLoadingStart, true);

        p.AddEdge("tCoolSwitch", IntMagTrapRampStart, false);
        p.AddEdge("tRepSwitch", IntMagTrapRampStart - 100, false);

        // Imaging 1
        p.Pulse(Img1Start, 0, (int)Parameters["tMOTExposure"], "tCoolSwitch");
        p.Pulse(Img1Start, 0, (int)Parameters["tMOTExposure"], "tRepSwitch");
        p.Pulse(Img1Start, 0, (int)Parameters["tMOTExposure"], "tHamCamTrig");
        p.Pulse(Img1Start, 0, (int)Parameters["tMOTExposure"], "tMOTCamTrig");

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
        int MolassesStart = MOTCompressionStart + (int)Parameters["tMOTCompressionDuration"];
        int IntMagTrapRampStart = MolassesStart + (int)Parameters["tMolassesDuration"];
        int IntMagTrapHoldStart = IntMagTrapRampStart + (int)Parameters["tIntMagTrapRampDuration"];
        int IntExtMagTrapTransferStart = IntMagTrapHoldStart + (int)Parameters["tIntMagTrapHoldDuration"];
        int ExtMagTrapHoldStart = IntExtMagTrapTransferStart + (int)Parameters["tExtMagTrapRampDuration"];
        int ExtIntTrapTransferStart = ExtMagTrapHoldStart + (int)Parameters["tExtMagTrapHoldDuration"];
        int IntMagTrapHoldStart2 = ExtIntTrapTransferStart + (int)Parameters["tExtMagTrapRampDuration"];
        int Delay1Start = IntMagTrapHoldStart2 + (int)Parameters["tIntMagTrapHoldDuration2"];
        int Img1Start = Delay1Start + (int)Parameters["tDelay1"];
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
        p.AddChannel("tMagneticTrapcc");

        // General
        p.AddAnalogValue("tRbD2RepOffset", MOTLoadingStart, (double)Parameters["tRbOffsetVoltageRep"]); // Offset Lock
        p.AddAnalogValue("tRbD2CoolOffset", MOTLoadingStart, (double)Parameters["tRbOffsetVoltageCool"]); // Offset Lock

        // MOT Coils - Switch - Load
        p.AddAnalogValue("tMOTccSwitch", MOTLoadingStart, 0.0); // MOT coils switch-on (RSD)
        p.AddAnalogValue("tMOTccSwitch", MolassesStart, (double)Parameters["tMOTccSwitch"]); // MOT coils switch-off (RSD)

        // MOT Coils - Switch - Mag Trapping
        p.AddAnalogValue("tMOTccSwitch", IntMagTrapRampStart, 0.0); // MOT coils switch-on (RSD)
        p.AddAnalogValue("tMOTccSwitch", ExtMagTrapHoldStart, (double)Parameters["tMOTccSwitch"]); // MOT coils switch-off (RSD)
        p.AddAnalogValue("tMOTccSwitch", ExtIntTrapTransferStart, 0.0); // MOT coils switch-on (RSD)
        p.AddAnalogValue("tMOTccSwitch", Delay1Start, (double)Parameters["tMOTccSwitch"]); // MOT coils switch-off (RSD)

        // MOT Coils - Current Control - Load
        p.AddAnalogValue("tMOTcc", MOTLoadingStart, (double)Parameters["tMOTccValue"]); // MOT Mag field 
        p.AddLinearRamp("tMOTcc", MOTCompressionStart, (int)Parameters["tMOTCompressionDuration"], (double)Parameters["tcMOTccValue"]); // Ramp up magnetic field
        p.AddAnalogValue("tMOTcc", MolassesStart, 0.0);

        // MOT Coils - Current Control - Int Mag Trap 1
        p.AddLinearRamp("tMOTcc", IntMagTrapRampStart, (int)Parameters["tIntMagTrapRampDuration"], (double)Parameters["tIntMagTrapccValue"]); // Ramp up magnetic field
        //p.AddLinearRamp("tMOTcc", IntExtMagTrapTransferStart, (int)Parameters["tExtMagTrapRampDuration"], 0.0); // Ramp up magnetic field
        p.AddAnalogValue("tMOTcc", IntExtMagTrapTransferStart, 0.0);

        // MOT Coils - Current Control - Int Mag Trap 2
        //p.AddLinearRamp("tMOTcc", ExtIntTrapTransferStart, (int)Parameters["tExtMagTrapRampDuration"], (double)Parameters["tIntMagTrapccValue"]); // Ramp up magnetic field
        //p.AddAnalogValue("tMOTcc", Delay1Start, 0.0);

        // Ext Coils
        p.AddAnalogValue("tMagneticTrapcc", MOTLoadingStart, 0.0);
        p.AddLinearRamp("tMagneticTrapcc", IntExtMagTrapTransferStart, (int)Parameters["tExtMagTrapRampDuration"], (double)Parameters["tExtMagTrapccValue"]); // Ramp up magnetic field
        p.AddLinearRamp("tMagneticTrapcc", ExtIntTrapTransferStart, (int)Parameters["tExtMagTrapRampDuration"], 0.0); // Ramp up magnetic field

        // Shim Coils - Current Control
        p.AddAnalogValue("tShimXcc", MOTLoadingStart, (double)Parameters["tMOTShimXccValue"]); // Default shim coils X
        p.AddAnalogValue("tShimYcc", MOTLoadingStart, (double)Parameters["tMOTShimYccValue"]); // Default shim coils Y
        p.AddAnalogValue("tShimZcc", MOTLoadingStart, (double)Parameters["tMOTShimZccValue"]); // Default shim coils Z

        p.AddAnalogValue("tShimXcc", MolassesStart, (double)Parameters["tMolassesShimXccValue"]); // Molasses shim coils X
        p.AddAnalogValue("tShimYcc", MolassesStart, (double)Parameters["tMolassesShimYccValue"]); // Molasses shim coils Y
        p.AddAnalogValue("tShimZcc", MolassesStart, (double)Parameters["tMolassesShimZccValue"]); // Molasses shim coils Z

        p.AddAnalogValue("tShimXcc", IntMagTrapRampStart, (double)Parameters["tMOTShimXccValue"]); // Default shim coils X
        p.AddAnalogValue("tShimYcc", IntMagTrapRampStart, (double)Parameters["tMOTShimYccValue"]); // Default shim coils Y
        p.AddAnalogValue("tShimZcc", IntMagTrapRampStart, (double)Parameters["tMOTShimZccValue"]); // Default shim coils Z

        p.AddLinearRamp("tShimXcc", IntMagTrapHoldStart, (int)Parameters["tIntMagTrapHoldDuration"], (double)Parameters["tMagTrapShimXccValue"]);
        p.AddLinearRamp("tShimYcc", IntMagTrapHoldStart, (int)Parameters["tIntMagTrapHoldDuration"], (double)Parameters["tMagTrapShimYccValue"]);
        p.AddLinearRamp("tShimZcc", IntMagTrapHoldStart, (int)Parameters["tIntMagTrapHoldDuration"], (double)Parameters["tMagTrapShimZccValue"]);

        p.AddAnalogValue("tShimXcc", End, 0.0); // End shim coils X
        p.AddAnalogValue("tShimYcc", End, 0.0); // End shim coils Y
        p.AddAnalogValue("tShimZcc", End, 0.0); // End shim coils Z

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
        p.AddAnalogValue("tRbCoolVCO", MolassesStart - 1, (double)Parameters["tMolCoolVCO1"]);
        p.AddAnalogValue("tRbRepVCO", MolassesStart - 1, (double)Parameters["tMolRepVCO1"]);
        p.AddAnalogValue("tRbD2CoolVVA", MolassesStart - 1, (double)Parameters["tMolCoolVVA1"]);
        p.AddAnalogValue("tRbD2RepVVA", MolassesStart - 1, (double)Parameters["tMolRepVVA1"]);

        p.AddLinearRamp("tRbCoolVCO", MolassesStart, (int)Parameters["tMolassesDuration"], (double)Parameters["tMolCoolVCO2"]);
        p.AddLinearRamp("tRbRepVCO", MolassesStart, (int)Parameters["tMolassesDuration"], (double)Parameters["tMolRepVCO2"]);
        p.AddLinearRamp("tRbD2CoolVVA", MolassesStart, (int)Parameters["tMolassesDuration"], (double)Parameters["tMolCoolVVA2"]);
        p.AddLinearRamp("tRbD2RepVVA", MolassesStart, (int)Parameters["tMolassesDuration"], (double)Parameters["tMolRepVVA2"]);

        // Initial Imaging 
        p.AddAnalogValue("tRbCoolVCO", IntMagTrapRampStart, (double)Parameters["tImgCoolVCO1"]);
        p.AddAnalogValue("tRbRepVCO", IntMagTrapRampStart, (double)Parameters["tImgRepVCO1"]);
        p.AddAnalogValue("tRbD2CoolVVA", IntMagTrapRampStart, (double)Parameters["tImgCoolVVA1"]);
        p.AddAnalogValue("tRbD2RepVVA", IntMagTrapRampStart, (double)Parameters["tImgRepVVA1"]);

        // Tweezer
        p.AddAnalogValue("tTweezerModVCF", MOTLoadingStart, (double)Parameters["tTweezerModulationVCF"]);
        p.AddAnalogValue("tTweezerVCO", MOTLoadingStart, (double)Parameters["tTweezerSetVCO"]);
        p.AddAnalogValue("tTweezerVCA", MOTLoadingStart, (double)Parameters["tTweezerSetVVA0"]);

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