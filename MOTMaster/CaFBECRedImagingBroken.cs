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
    public class CaFBECRedImaging : MOTMasterScriptSnippet
    {

        public CaFBECRedImaging(PatternBuilder32 p, Dictionary<String, Object> Parameters)
        {
            AddDigitalSnippet(p, Parameters);
        }
        public CaFBECRedImaging(AnalogPatternBuilder p, Dictionary<String, Object> Parameters)
        {
            AddAnalogSnippet(p, Parameters);
        }

        public void AddDigitalSnippet(PatternBuilder32 p, Dictionary<String, Object> Parameters)
        {
        }
            //int cameraStart = (int)Parameters["ImagingStart"] - (int)Parameters["CameraExposureDelayCCDMode"];
            //int bgCameraStart = (int)Parameters["BackgroundImageTime"] - (int)Parameters["CameraExposureDelayCCDMode"];

            //if ((double)Parameters["CloudImageONorOFF"] > 5.0)
            //{
            //    p.Pulse(
            //        (int)Parameters["PatternStart"],
            //        cameraStart,
            //        (int)Parameters["CameraTriggerDuration"],
            //        "cameraTrigger"
            //    );
            //}

            //// Background Imaging
            //p.Pulse(
            //    (int)Parameters["PatternStart"],
            //    (int)Parameters["BackgroundImageTime"],
            //    (int)Parameters["CameraTriggerDuration"],
            //    "V00R0AOM"
            //);
            //p.Pulse(
            //    (int)Parameters["PatternStart"],
            //    (int)Parameters["BackgroundImageTime"],
            //    (int)Parameters["CameraTriggerDuration"],
            //    "V00R1plusAOMredMOT"
            //);
            //p.Pulse(
            //    (int)Parameters["PatternStart"],
            //    (int)Parameters["BackgroundImageTime"],
            //    (int)Parameters["CameraTriggerDuration"],
            //    "V00B2AOMmolasses"
            //);
            //p.Pulse(
            //    (int)Parameters["PatternStart"],
            //    (int)Parameters["BackgroundImageTime"],
            //    (int)Parameters["CameraTriggerDuration"],
            //    "V00B1minusAOM"
            //);

            //if ((double)Parameters["BackgroundImageONorOFF"] > 5.0)
            //{
            //    p.Pulse(
            //        (int)Parameters["PatternStart"],
            //        bgCameraStart,
            //        (int)Parameters["CameraTriggerDuration"],
            //        "cameraTrigger"
            //    );
            //}

        public void AddAnalogSnippet(AnalogPatternBuilder p, Dictionary<String, Object> Parameters)
        {
            //p.AddChannel("V00R0AOMVCOAmp");
            //p.AddChannel("V00R1plusAOMAmp");
            //p.AddChannel("V00B2AOMAmp");
            //p.AddChannel("V00B1minusAOMAmp");

            //int motLoadingStop = (int)Parameters["MOTLoadingDuration"];
            //int motCompressionStop = motLoadingStop + (int)Parameters["MOTCompressionRampDuration"] + (int)Parameters["MOTCompressionHoldDuration"];
            //int imagingLightStart = motCompressionStop + (int)Parameters["CameraTriggerStartDelay"];
            //int cameraStart = imagingLightStart - (int)Parameters["CameraExposureDelayCCDMode"];

            //if ((double)Parameters["CloudImageONorOFF"] > 5.0)
            //{
            //    p.AddAnalogValue(
            //        "V00R0AOMVCOAmp",
            //        cameraStart,
            //        (double)Parameters["V00F0AOMAmpImaging"]
            //    );
            //    p.AddAnalogValue(
            //        "V00R1plusAOMAmp",
            //        cameraStart,
            //        (double)Parameters["V00F1plusAOMAmpImaging"]
            //    );
            //    p.AddAnalogValue(
            //        "V00B2AOMAmp",
            //        cameraStart,
            //        (double)Parameters["V00F2AOMAmpImaging"]
            //    );
            //    p.AddAnalogValue(
            //        "V00B1minusAOMAmp",
            //        cameraStart,
            //        (double)Parameters["V00F1minusAOMAmpImaging"]
            //    );

        }
    }
}
