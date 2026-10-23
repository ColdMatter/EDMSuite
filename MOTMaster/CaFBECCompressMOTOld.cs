using System;
using System.Collections.Generic;
/*using System.Linq;
using System.Text;
using System.Threading.Tasks;*/
using DAQ.Pattern;
using DAQ.Analog;
using DAQ;

namespace MOTMaster.SnippetLibrary
{
    public class CaFBECCompressMOT : MOTMasterScriptSnippet
    {

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
            int motCompressionStart = (int)Parameters["MOTLoadingDuration"];
            int motCompressionStop = motCompressionStart + (int)Parameters["MOTCompressionRampDuration"] + (int)Parameters["MOTCompressionHoldDuration"];

            p.Pulse(
                (int)Parameters["PatternStart"],
                (int)Parameters["V00AOMONStartTime"],
                motCompressionStop,
                "V00R0AOM"
            );
            p.Pulse(
                (int)Parameters["PatternStart"],
                (int)Parameters["V00AOMONStartTime"],
                motCompressionStop,
                "V00R1plusAOMredMOT"
            );
            p.Pulse(
                (int)Parameters["PatternStart"],
                (int)Parameters["V00AOMONStartTime"],
                motCompressionStop,
                "V00B2AOMmolasses"
            );
            p.Pulse(
                (int)Parameters["PatternStart"],
                (int)Parameters["V00AOMONStartTime"],
                motCompressionStop,
                "V00B1minusAOM"
            );
        }

        public void AddAnalogSnippet(AnalogPatternBuilder p, Dictionary<String, Object> Parameters)
        {
            p.AddChannel("motCoils");
            p.AddChannel("V00R0AOMVCOFreq");
            p.AddChannel("V00R0AOMVCOAmp");
            p.AddChannel("V00R1plusAOMAmp");

            int motCompressionStart = (int)Parameters["MOTLoadingDuration"];
            int motCompressionStop = motCompressionStart + (int)Parameters["MOTCompressionRampDuration"] + (int)Parameters["MOTCompressionHoldDuration"];

            if ((double)Parameters["CMOTONorOFF"] > 5.0)
            {
                p.AddLinearRamp(
                    "motCoils",
                    motCompressionStart,
                    (int)Parameters["MOTCompressionRampDuration"],
                    (double)Parameters["MOTCompressionFieldValue"]
                );
                p.AddLinearRamp(
                    "V00R0AOMVCOAmp",
                    motCompressionStart,
                    (int)Parameters["MOTCompressionRampDuration"],
                    (double)Parameters["V00F0AOMAmpMOTCompression"]
                );
                p.AddLinearRamp(
                    "V00R1plusAOMAmp",
                    motCompressionStart,
                    (int)Parameters["MOTCompressionRampDuration"],
                    (double)Parameters["V00F1plusAOMAmpMOTCompression"]
                );
                p.AddLinearRamp(
                    "V00B2AOMAmp",
                    motCompressionStart,
                    (int)Parameters["MOTCompressionRampDuration"],
                    (double)Parameters["V00F2AOMAmpMOTCompression"]
                );
                p.AddLinearRamp(
                    "V00B1minusAOMAmp",
                    motCompressionStart,
                    (int)Parameters["MOTCompressionRampDuration"],
                    (double)Parameters["V00F1minusAOMAmpMOTCompression"]
                );
                p.AddLinearRamp(
                    "ShimCoilX",
                    motCompressionStart,
                    (int)Parameters["MOTCompressionRampDuration"],
                    (double)Parameters["XShimCoilsCMOTValue"]
                );
                p.AddLinearRamp(
                    "ShimCoilY",
                    motCompressionStart,
                    (int)Parameters["MOTCompressionRampDuration"],
                    (double)Parameters["YShimCoilsCMOTValue"]
                );
                p.AddLinearRamp(
                    "ShimCoilZ",
                    motCompressionStart,
                    (int)Parameters["MOTCompressionRampDuration"],
                    (double)Parameters["ZShimCoilsCMOTValue"]
                );

        }
    }
}
