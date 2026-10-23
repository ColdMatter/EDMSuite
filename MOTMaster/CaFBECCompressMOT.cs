using System;
using System.Collections.Generic;

using DAQ.Pattern;
using DAQ.Analog;
using DAQ;

namespace MOTMaster.SnippetLibrary
{
    /*
     * MOT compression.
     * Light:  V00 AOMs on for the whole window. Ramp to compression values.
     * Field:  Coils and shims ramp to their CMOT values then hold.
     */

    public class CaFBECCompressMOT : MOTMasterScriptSnippet
    {
        private static readonly string[] V00AOMs =
        {
            "V00R0AOM", "V00R1plusAOMredMOT", "V00B2AOMmolasses", "V00B1minusAOM"
        };

        public CaFBECCompressMOT(PatternBuilder32 p, Dictionary<String, Object> Parameters)
        {
            AddDigitalSnippet(p, Parameters);
        }

        public CaFBECCompressMOT(AnalogPatternBuilder p, Dictionary<String, Object> Parameters)
        {
            AddAnalogSnippet(p, Parameters);
        }

        public void AddDigitalSnippet(PatternBuilder32 p, Dictionary<String, Object> Parameters)
        {
            int patternStart = (int)Parameters["PatternStart"];
            int compressionStart = (int)Parameters["MOTCompressionStart"];
            int compressionStop = compressionStart
                                  + (int)Parameters["MOTCompressionRampDuration"]
                                  + (int)Parameters["MOTCompressionHoldDuration"];

            foreach (string aom in V00AOMs)
                p.Pulse(patternStart, compressionStart, compressionStop - compressionStart, aom);
        }

        public void AddAnalogSnippet(AnalogPatternBuilder p, Dictionary<String, Object> Parameters)
        {
            p.AddChannel("motCoils");
            p.AddChannel("V00R0AOMVCOAmp");
            p.AddChannel("V00R1plusAOMAmp");
            p.AddChannel("V00B2AOMAmp");
            p.AddChannel("V00B1minusAOMAmp");
            p.AddChannel("ShimCoilX");
            p.AddChannel("ShimCoilY");
            p.AddChannel("ShimCoilZ");

            if ((double)Parameters["CMOTONorOFF"] <= 5.0) return;

            int compressionStart = (int)Parameters["MOTCompressionStart"];
            int rampDuration = (int)Parameters["MOTCompressionRampDuration"];

            p.AddLinearRamp("motCoils",         compressionStart, rampDuration, (double)Parameters["MOTCompressionFieldValue"]);
            p.AddLinearRamp("V00R0AOMVCOAmp",   compressionStart, rampDuration, (double)Parameters["V00F0AOMAmpMOTCompression"]);
            p.AddLinearRamp("V00R1plusAOMAmp",  compressionStart, rampDuration, (double)Parameters["V00F1plusAOMAmpMOTCompression"]);
            p.AddLinearRamp("V00B2AOMAmp",      compressionStart, rampDuration, (double)Parameters["V00F2AOMAmpMOTCompression"]);
            p.AddLinearRamp("V00B1minusAOMAmp", compressionStart, rampDuration, (double)Parameters["V00F1minusAOMAmpMOTCompression"]);
            p.AddLinearRamp("ShimCoilX",        compressionStart, rampDuration, (double)Parameters["XShimCoilsCMOTValue"]);
            p.AddLinearRamp("ShimCoilY",        compressionStart, rampDuration, (double)Parameters["YShimCoilsCMOTValue"]);
            p.AddLinearRamp("ShimCoilZ",        compressionStart, rampDuration, (double)Parameters["ZShimCoilsCMOTValue"]);
        }
    }
}
