using MOTMaster;
using MOTMaster.SnippetLibrary;
using MOTMaster.SnippetLibrary.CaF;

using System;
using System.Collections.Generic;

using DAQ.Pattern;
using DAQ.Analog;

// This script is supposed to be the basic script for loading a molecule MOT.
// Note that times are all in units of the clock periods of the two pattern generator boards (at present, both are 10us).
// All times are relative to the Q switch, though note that this is not the first event in the pattern.
public class Patterns : MOTMasterScript
{
    public Patterns()
    {
        Parameters = new Dictionary<string, object>();
        LoadGlobalParameters();
        Parameters["PatternLength"] = 365000 + 30000;
        Parameters["YagPreFire"] = 45000;


        Parameters["ThorCamExposure"] = 250;
        Parameters["OrcaExposure"] = 1000;

        // Shim Coils
        Parameters["xShimLoadCurrent"] = -3.20;
        Parameters["yShimLoadCurrent"] = -1.35;
        Parameters["zShimLoadCurrent"] = 0.22;

        // Mag Trap
        Parameters["MagTrapField"] = 1.42;

        // Slowing Chirp, 5W ALS laser
        Parameters["SlowingChirpStartTime"] = 300;//360; //400;// 380;
        Parameters["SlowingChirpDuration"] = 1250;////1400;//1160; //1160

        // Timings
        Parameters["FreeExpTime"] = 1;
        Parameters["OpticalPumpingDuration"] = 100;
        Parameters["Magtrap1Duration"] = 1000;
        Parameters["BlueMOTDuration"] = 1000;
        Parameters["LambdaCoolingDuration"] = 300;
        Parameters["LambdaCooling2Duration"] = 300;
        Parameters["MagTrapField"] = 1.6;

        Parameters["HandoverDur"] = 10000;  // can I scan this?
        Parameters["MoveDuration"] = 154000; //done

        Parameters["tMagTrapField"] = 1.0; //done
        Parameters["tMagtrapDuration"] = 1; // done
        Parameters["tDropDuration"] = 1; //done
        Parameters["tImageExpo"] = 1000;

        Parameters["tBlueMOTDuration"] = 30000;
        Parameters["tBlueMOTField"] = 0.48; //0.48 Rb = 30G/cm

        Parameters["tCoilOffDur"] = 1; //done

        // Typed into this script before snippets; the snippets read them as parameters.
        Parameters["TransferCoilsOffValue"] = -0.1;
    }

    // The whole shot, in ticks after the Q-switch. Called at the top of each Get...Pattern,
    // after scan values are in; the snippets read these times.
    private void SetTimeline()
    {
        // MOT chamber
        Parameters["CMOTStartTime"] = (int)Parameters["CompressRampDownStartTime"];
        Parameters["CMOTEndTime"] = (int)Parameters["CMOTStartTime"] + (int)Parameters["CompressRampDownDuration"] + (int)Parameters["CompressRampDownHoldDuration"];
        Parameters["LambdaCooling1StartTime"] = Parameters["CMOTEndTime"];
        Parameters["LambdaCooling1EndTime"] = (int)Parameters["LambdaCooling1StartTime"] + (int)Parameters["LambdaCoolingDuration"];
        Parameters["BlueMOTStartTime"] = Parameters["LambdaCooling1EndTime"];
        Parameters["BlueMOTEndTime"] = (int)Parameters["BlueMOTStartTime"] + (int)Parameters["BlueMOTRampDuration"] + (int)Parameters["BlueMOTDuration"];
        Parameters["LambdaCooling2StartTime"] = Parameters["BlueMOTEndTime"];
        Parameters["LambdaCooling2EndTime"] = (int)Parameters["LambdaCooling2StartTime"] + (int)Parameters["LambdaCooling2Duration"];
        Parameters["LightOff1StartTime"] = Parameters["LambdaCooling2EndTime"];
        Parameters["OPStartTime"] = (int)Parameters["LambdaCooling2EndTime"] + (int)Parameters["OPGap"];
        Parameters["OPEndTime"] = (int)Parameters["OPStartTime"] + (int)Parameters["OpticalPumpingDuration"];
        Parameters["MagtrapStartTime"] = Parameters["OPEndTime"];
        Parameters["TransferHandoverStartTime"] = (int)Parameters["MagtrapStartTime"] + (int)Parameters["Magtrap1Duration"];
        Parameters["TransportStartTime"] = (int)Parameters["TransferHandoverStartTime"] + (int)Parameters["HandoverDur"];

        // Tweezer chamber
        Parameters["TweezerHandoverStartTime"] = (int)Parameters["TransportStartTime"] + (int)Parameters["MoveDuration"];
        Parameters["tMagtrapStartTime"] = (int)Parameters["TweezerHandoverStartTime"] + (int)Parameters["HandoverDur"];
        Parameters["tCoilsOffTime"] = (int)Parameters["tMagtrapStartTime"] + (int)Parameters["tMagtrapDuration"];
        Parameters["tBlueMOTStartTime"] = (int)Parameters["tCoilsOffTime"] + (int)Parameters["tCoilOffDur"];
        Parameters["tBlueMOTEndTime"] = (int)Parameters["tBlueMOTStartTime"] + (int)Parameters["BlueMOTRampDuration"] + (int)Parameters["tBlueMOTDuration"];
        Parameters["TransportBackStartTime"] = (int)Parameters["tBlueMOTEndTime"] + (int)Parameters["tImageExpo"] + 10000;

        Parameters["bXSlowingShutterDuration"] = (int)Parameters["MagtrapDuration"] + 21000;
    }

    public override Dictionary<string, List<List<double>>> GetDDSPattern()
    {
        DDSPatternBuilder p = new DDSPatternBuilder();
        SetTimeline();

        new LoadMOT(p, Parameters);
        new CompressedMOT(p, Parameters);
        new LambdaCooling1(p, Parameters);
        new BlueMOT(p, Parameters);
        new LambdaCooling2(p, Parameters);
        new LightOff1(p, Parameters);
        new TweezerBlueMOT(p, Parameters);

        return p.Pattern;
    }

    public override PatternBuilder32 GetDigitalPattern()
    {
        PatternBuilder32 p = new PatternBuilder32();
        SetTimeline();
        int patternStartBeforeQ = (int)Parameters["TCLBlockStart"];
        int tBlueMOTRampEnd = (int)Parameters["tBlueMOTStartTime"] + (int)Parameters["BlueMOTRampDuration"];

        new LoadMOT(p, Parameters);
        new OpticalPumping(p, Parameters);

        // CAMERA //
        p.Pulse(patternStartBeforeQ, (int)Parameters["BlueMOTEndTime"] - (int)Parameters["ThorCamExposure"], (int)Parameters["ThorCamExposure"], "cameraTrigger"); //camera trigger for first frame
        p.Pulse(patternStartBeforeQ, (int)Parameters["tBlueMOTEndTime"] - (int)Parameters["OrcaExposure"], (int)Parameters["OrcaExposure"], "rbAbsImgCamTrig");

        // MOT Shutters
        p.AddEdge("MOT1Shutter", patternStartBeforeQ + (int)Parameters["LambdaCooling2EndTime"], true);
        p.AddEdge("MOT2Shutter", patternStartBeforeQ + (int)Parameters["LambdaCooling2EndTime"], true);
        p.AddEdge("MOT3Shutter", patternStartBeforeQ + (int)Parameters["LambdaCooling2EndTime"], true);

        p.AddEdge("MOT1Shutter", patternStartBeforeQ + tBlueMOTRampEnd + (int)Parameters["BlueMOTDuration"] + 2000, false);
        p.AddEdge("MOT2Shutter", patternStartBeforeQ + tBlueMOTRampEnd + (int)Parameters["BlueMOTDuration"] + 2000, false);
        p.AddEdge("MOT3Shutter", patternStartBeforeQ + tBlueMOTRampEnd + (int)Parameters["BlueMOTDuration"] + 2000, false);

        // Tweezer Chamber CaF Light Shutter
        p.AddEdge("TweezerChamberMOTShutter", patternStartBeforeQ + tBlueMOTRampEnd - 20000, true); //Shutter CaF light to tweezer chamber - OPEN
        p.AddEdge("TweezerChamberMOTShutter", patternStartBeforeQ + (int)Parameters["TransportBackStartTime"], false);

        // Transport
        p.AddEdge("transportTrack", 0, true);
        p.AddEdge("transportTrack", patternStartBeforeQ + (int)Parameters["TransportStartTime"], false);
        p.AddEdge("transportTrack", patternStartBeforeQ + (int)Parameters["TransportBackStartTime"], true);

        // YaG Compensating Shots
        p.Pulse(patternStartBeforeQ, (int)Parameters["PatternLength"] - (int)Parameters["YagPreFire"] - (int)Parameters["FlashToQ"], (int)Parameters["QSwitchPulseDuration"], "flashLamp");
        p.Pulse(patternStartBeforeQ, (int)Parameters["PatternLength"] - (int)Parameters["YagPreFire"], (int)Parameters["QSwitchPulseDuration"], "qSwitch");

        return p;
    }

    public override AnalogPatternBuilder GetAnalogPattern()
    {
        AnalogPatternBuilder p = new AnalogPatternBuilder((int)Parameters["PatternLength"]);
        SetTimeline();

        new LoadMOT(p, Parameters);
        p.AddChannel("transferCoils");
        p.AddChannel("TweezerCoils");
        p.AddAnalogValue("transferCoils", 0, -0.1);

        // MOT chamber
        new CompressedMOT(p, Parameters);
        new LambdaCooling1(p, Parameters);
        new BlueMOT(p, Parameters);
        new LambdaCooling2(p, Parameters);
        p.AddAnalogValue("MOTCoilsCurrent", (int)Parameters["MagtrapStartTime"], (double)Parameters["MagTrapField"]); // magtrap
        new HandoverToTransferCoils(p, Parameters);

        // Tweezer chamber
        new HandoverToTweezerCoils(p, Parameters);
        p.AddAnalogValue("TweezerCoils", (int)Parameters["tCoilsOffTime"], 0.0); // drop
        new TweezerBlueMOT(p, Parameters);
        p.AddAnalogValue("TweezerCoils", (int)Parameters["tBlueMOTEndTime"], 0.0);

        return p;
    }
}
