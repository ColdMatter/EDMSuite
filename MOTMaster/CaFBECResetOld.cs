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

            int motLoadingStop = (int)Parameters["MOTLoadingDuration"];
            int motCompressionStop = motLoadingStop + (int)Parameters["MOTCompressionRampDuration"] + (int)Parameters["MOTCompressionHoldDuration"];
            int imagingLightStart = motCompressionStop + (int)Parameters["CameraTriggerStartDelay"];
            int cameraStart = imagingLightStart - (int)Parameters["CameraExposureDelayCCDMode"];
            int cameraStop = cameraStart + (int)Parameters["CameraTriggerDuration"];

            p.AddAnalogValue(
                "V00R0AOMVCOAmp",
                (int)Parameters["ResetTime"],
                (double)Parameters["V00F0AOMAmpMax"]
            );
            p.AddAnalogValue(
               "V00R1plusAOMAmp",
               (int)Parameters["ResetTime"],
               (double)Parameters["V00F1plusAOMAmpMax"]
            );
            p.AddAnalogValue(
                "V00B2AOMAmp",
                (int)Parameters["ResetTime"],
                (double)Parameters["V00F2AOMAmpMax"]
            );
            p.AddAnalogValue(
               "V00B1minusAOMAmp",
               (int)Parameters["ResetTime"],
               (double)Parameters["V00F1minusAOMAmpMax"]
            );
            p.AddAnalogValue(
                "ShimCoilX",
                (int)Parameters["ResetTime"],
                (double)Parameters["XShimCoilsOffValue"]
            );
            p.AddAnalogValue(
                "ShimCoilY",
                (int)Parameters["ResetTime"],
                (double)Parameters["YShimCoilsOffValue"]
            );
            p.AddAnalogValue(
                "ShimCoilZ",
                (int)Parameters["ResetTime"],
                (double)Parameters["ZShimCoilsOffValue"]
            );
            p.AddAnalogValue(
                "motCoils",
                cameraStop,
                (double)Parameters["MOTCoilsOffValue"]
            );

        }
    }
}
