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
            int cameraStart = (int)Parameters["ImagingStart"] - (int)Parameters["CameraExposureDelayCCDMode"];
            int bgCameraStart = (int)Parameters["BackgroundImageTime"] - (int)Parameters["CameraExposureDelayCCDMode"];

            if ((double)Parameters["CloudImageONorOFF"] > 5.0)
            {
                p.Pulse(
                    (int)Parameters["PatternStart"],
                    (int)Parameters["ImagingStart"],
                    (int)Parameters["CameraTriggerDuration"],
                    "V00R0AOM"
                );
                p.Pulse(
                    (int)Parameters["PatternStart"],
                    (int)Parameters["ImagingStart"],
                    (int)Parameters["CameraTriggerDuration"],
                    "V00R1plusAOMredMOT"
                );
                p.Pulse(
                    (int)Parameters["PatternStart"],
                    cameraStart,
                    (int)Parameters["CameraTriggerDuration"],
                    "cameraTrigger"
                );
            }

            if ((double)Parameters["BackgroundImageONorOFF"] > 5.0)
            {
                p.Pulse(
                    (int)Parameters["PatternStart"],
                    (int)Parameters["BackgroundImageTime"],
                    (int)Parameters["CameraTriggerDuration"],
                    "V00R0AOM"
                );
                p.Pulse(
                    (int)Parameters["PatternStart"],
                    (int)Parameters["BackgroundImageTime"],
                    (int)Parameters["CameraTriggerDuration"],
                    "V00R1plusAOMredMOT"
                );
                p.Pulse(
                    (int)Parameters["PatternStart"],
                    bgCameraStart,
                    (int)Parameters["CameraTriggerDuration"],
                    "cameraTrigger"
                );
            }
        }

        public void AddAnalogSnippet(AnalogPatternBuilder p, Dictionary<String, Object> Parameters)
        {
            p.AddChannel("V00R0AOMVCOFreq");
            p.AddChannel("V00R0AOMVCOAmp");
            p.AddChannel("V00R1plusAOMAmp");

            if ((double)Parameters["CloudImageONorOFF"] > 5.0)
            {
                p.AddAnalogValue(
                    "V00R1plusAOMAmp",
                    (int)Parameters["ImagingStart"],
                    (double)Parameters["V00R1plusAOMAmpImaging"]
                );
                p.AddAnalogValue(
                    "V00R0AOMVCOFreq",
                    (int)Parameters["ImagingStart"],
                    (double)Parameters["V00R0AOMVCOFreqImaging"]
                );
                p.AddAnalogValue(
                    "V00R0AOMVCOAmp",
                    (int)Parameters["ImagingStart"],
                    (double)Parameters["V00R0AOMVCOAmpImaging"]
                );

            }

        }
    }
}
