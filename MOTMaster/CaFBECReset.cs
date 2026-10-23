using System;
using System.Collections.Generic;

using DAQ.Pattern;
using DAQ.Analog;
using DAQ;

namespace MOTMaster.SnippetLibrary
{
    // End-of-shot reset: return V00 amplitudes to max and shims coils to off.
 
    public class CaFBECReset : MOTMasterScriptSnippet
    {
        public CaFBECReset(PatternBuilder32 p, Dictionary<String, Object> Parameters)
        {
            AddDigitalSnippet(p, Parameters);
        }

        public CaFBECReset(AnalogPatternBuilder p, Dictionary<String, Object> Parameters)
        {
            AddAnalogSnippet(p, Parameters);
        }

        public void AddDigitalSnippet(PatternBuilder32 p, Dictionary<String, Object> Parameters)
        {
        }

        public void AddAnalogSnippet(AnalogPatternBuilder p, Dictionary<String, Object> Parameters)
        {
            p.AddChannel("V00R0AOMVCOAmp");
            p.AddChannel("V00R1plusAOMAmp");
            p.AddChannel("V00B2AOMAmp");
            p.AddChannel("V00B1minusAOMAmp");
            p.AddChannel("ShimCoilX");
            p.AddChannel("ShimCoilY");
            p.AddChannel("ShimCoilZ");
            p.AddChannel("motCoils");

            int resetTime = (int)Parameters["ResetTime"];

            p.AddAnalogValue("V00R0AOMVCOAmp", resetTime, (double)Parameters["V00F0AOMAmpMax"]);
            p.AddAnalogValue("V00R1plusAOMAmp", resetTime, (double)Parameters["V00F1plusAOMAmpMax"]);
            p.AddAnalogValue("V00B2AOMAmp", resetTime, (double)Parameters["V00F2AOMAmpMax"]);
            p.AddAnalogValue("V00B1minusAOMAmp", resetTime, (double)Parameters["V00F1minusAOMAmpMax"]);
            p.AddAnalogValue("ShimCoilX", resetTime, (double)Parameters["XShimCoilsOffValue"]);
            p.AddAnalogValue("ShimCoilY", resetTime, (double)Parameters["YShimCoilsOffValue"]);
            p.AddAnalogValue("ShimCoilZ", resetTime, (double)Parameters["ZShimCoilsOffValue"]);
            p.AddAnalogValue("motCoils", resetTime, (double)Parameters["MOTCoilsOffValue"]);
        }
    }
}
