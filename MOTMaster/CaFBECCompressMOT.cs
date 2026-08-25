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
            p.Pulse(
                (int)Parameters["PatternStart"],
                (int)Parameters["MOTCompressionStart"],
                (int)Parameters["MOTCompressionRampDuration"] + (int)Parameters["MOTCompressionHoldDuration"],
                "V00R0AOM"
                );
            p.Pulse(
                (int)Parameters["PatternStart"],
                (int)Parameters["MOTCompressionStart"], 
                (int)Parameters["MOTCompressionRampDuration"] + (int)Parameters["MOTCompressionHoldDuration"],
                "V00R1plusAOMredMOT"
            );
        }

        public void AddAnalogSnippet(AnalogPatternBuilder p, Dictionary<String, Object> Parameters)
        {
            p.AddChannel("motCoils");
            p.AddChannel("V00R0AOMVCOFreq");
            p.AddChannel("V00R0AOMVCOAmp");
            p.AddChannel("V00R1plusAOMAmp");

            if ((double)Parameters["CMOTONorOFF"] > 5.0)
            {
                p.AddLinearRamp(
                    "motCoils",
                    (int)Parameters["MOTCompressionStart"],
                    (int)Parameters["MOTCompressionRampDuration"],
                    (double)Parameters["MOTCompressionFieldValue"]
                );
                p.AddLinearRamp(
                    "V00R0AOMVCOFreq",
                    (int)Parameters["MOTCompressionStart"],
                    (int)Parameters["MOTCompressionRampDuration"],
                    (double)Parameters["V00R0AOMVCOFreqMOTCompression"]
                );
                p.AddLinearRamp(
                    "V00R0AOMVCOAmp",
                    (int)Parameters["MOTCompressionStart"],
                    (int)Parameters["MOTCompressionRampDuration"],
                    (double)Parameters["V00R0AOMVCOAmpMOTCompression"]
                );
                p.AddLinearRamp(
                    "V00R1plusAOMAmp",
                    (int)Parameters["MOTCompressionStart"],
                    (int)Parameters["MOTCompressionRampDuration"],
                    (double)Parameters["V00R1plusAOMAmpMOTCompression"]
                );
            }
        }
    }
}
