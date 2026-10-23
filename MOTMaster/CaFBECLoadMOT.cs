using System;
using System.Collections.Generic;

using DAQ.Pattern;
using DAQ.Analog;
using DAQ;

namespace MOTMaster.SnippetLibrary
{
    /*
     MOT loading phase: YAG, buffer-gas shutter, slowing, MOT light + field.
     Also fires the analog pattern trigger and the TCL block, so this
     snippet must be the first one called in GetDigitalPattern().
     */

    public class CaFBECLoadMOT : MOTMasterScriptSnippet
    {
        private static readonly string[] V00AOMs =
        {
            "V00R0AOM", "V00R1plusAOMredMOT", "V00B2AOMmolasses", "V00B1minusAOM"
        };

        public CaFBECLoadMOT(PatternBuilder32 p, Dictionary<String, Object> Parameters)
        {
            AddDigitalSnippet(p, Parameters);
        }

        public CaFBECLoadMOT(AnalogPatternBuilder p, Dictionary<String, Object> Parameters)
        {
            AddAnalogSnippet(p, Parameters);
        }

        public void AddDigitalSnippet(PatternBuilder32 p, Dictionary<String, Object> Parameters)
        {
            int patternStart = (int)Parameters["PatternStart"];

            int slowingAOMStart = (int)Parameters["BXAOMFreeFlightDuration"];
            int slowingChirpStart = slowingAOMStart + (int)Parameters["BXAOMPostBunchingDuration"];
            int slowingChirpStop = slowingChirpStart + (int)Parameters["BXAOMChirpDuration"];

            int motLoadingStart = (int)Parameters["V00AOMONStartTime"];
            int motLoadingStop = (int)Parameters["MOTLoadingDuration"];

            // Housekeeping
            p.Pulse(patternStart, 0, 10, "analogPatternTrigger");
            p.EnforceTimeOrdering(false);
            p.Pulse(patternStart, 0, (int)Parameters["TCLBlockLength"], "blockTCL");
            p.Pulse(patternStart, (int)Parameters["HeShutterStart"], (int)Parameters["HeShutterDuration"], "BXShutter");

            // Source
            p.Pulse(patternStart, -(int)Parameters["FlashToQ"], (int)Parameters["QSwitchPulseDuration"], "flash");
            if ((double)Parameters["yagONorOFF"] > 5.0)
                p.Pulse(patternStart, 0, (int)Parameters["QSwitchPulseDuration"], "q");

            // Slowing
            if ((double)Parameters["SlowingONorOFF"] > 5.0)
            {
                p.Pulse(patternStart, slowingAOMStart, slowingChirpStop - slowingAOMStart, "BXAOM");
                p.Pulse(patternStart, slowingAOMStart, slowingChirpStop - slowingAOMStart, "BXAOM2");
                p.Pulse(patternStart, 10, 5000, "BXSidebands");
                p.Pulse(patternStart, 0, slowingChirpStop, "RepumpAOM");
                p.Pulse(patternStart, 0, slowingChirpStop, "RepumpBroadening");
            }

            // Currently using 2nd camera trigger for BX shutter (closed on high)
            if ((double)Parameters["SlowingShutterONorOFF"] > 5.0)
                p.Pulse(patternStart, 0, (int)Parameters["BackgroundImageTime"], "camera2Trigger");

            // MOT light
            foreach (string aom in V00AOMs)
                p.Pulse(patternStart, motLoadingStart, motLoadingStop - motLoadingStart, aom);
        }

        public void AddAnalogSnippet(AnalogPatternBuilder p, Dictionary<String, Object> Parameters)
        {
            p.AddChannel("BXChirp");
            p.AddChannel("SlowingBField");
            p.AddChannel("motCoils");
            p.AddChannel("ShimCoilX");
            p.AddChannel("ShimCoilY");
            p.AddChannel("ShimCoilZ");
            p.AddChannel("V00R0AOMVCOFreq");
            p.AddChannel("V00R0AOMVCOAmp");
            p.AddChannel("V00R1plusAOMAmp");
            p.AddChannel("V00B2AOMAmp");
            p.AddChannel("V00B1minusAOMAmp");
            p.AddChannel("V00R0EOMAmp");

            int slowingChirpStart = (int)Parameters["BXAOMFreeFlightDuration"] + (int)Parameters["BXAOMPostBunchingDuration"];
            int slowingChirpStop = slowingChirpStart + (int)Parameters["BXAOMChirpDuration"];

            if ((double)Parameters["SlowingONorOFF"] > 5.0)
            {
                p.AddLinearRamp("BXChirp", slowingChirpStart, slowingChirpStop - slowingChirpStart,
                                (double)Parameters["BXChirpMHzSpan"] / (double)Parameters["BXMHzPerVolt"]);
                p.AddLinearRamp("BXChirp", slowingChirpStop + 5000, 1000, (double)Parameters["BXChirpStartValue"]);
                p.AddAnalogValue("SlowingBField", 0, (double)Parameters["SlowingCoilFieldValue"]);
                p.AddAnalogValue("SlowingBField", slowingChirpStop + 400, 0.0);
                p.AddAnalogValue("ShimCoilY", 0, (double)Parameters["YShimCoilsSlowingValue"]);
            }

            if ((double)Parameters["MOTONorOFF"] > 5.0)
            {
                p.AddAnalogValue("motCoils", (int)Parameters["MOTCoilsStartTime"], (double)Parameters["MOTLoadingFieldValue"]);
                p.AddAnalogValue("ShimCoilX", slowingChirpStop, (double)Parameters["XShimCoilsMOTValue"]);
                p.AddAnalogValue("ShimCoilY", slowingChirpStop, (double)Parameters["YShimCoilsMOTValue"]);
                p.AddAnalogValue("ShimCoilZ", slowingChirpStop, (double)Parameters["ZShimCoilsMOTValue"]);
                p.AddAnalogValue("V00R0AOMVCOAmp", 0, (double)Parameters["V00F0AOMAmpMOTLoading"]);
                p.AddAnalogValue("V00R1plusAOMAmp", 0, (double)Parameters["V00F1plusAOMAmpMOTLoading"]);
                p.AddAnalogValue("V00B2AOMAmp", 0, (double)Parameters["V00F2AOMAmpMOTLoading"]);
                p.AddAnalogValue("V00B1minusAOMAmp", 0, (double)Parameters["V00F1minusAOMAmpMOTLoading"]);
            }
        }
    }
}
