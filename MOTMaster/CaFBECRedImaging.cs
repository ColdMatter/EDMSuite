using System;
using System.Collections.Generic;

using DAQ.Pattern;
using DAQ.Analog;
using DAQ;

namespace MOTMaster.SnippetLibrary
{
    /*
     * Absorption/fluorescence imaging with the V00 (red) light, plus a
     * background image later in the pattern.
     *
     * Window:  [ImagingStart, ImagingStart + CameraTriggerStartDelay + CameraTriggerDuration]
     *          ImagingStart is set by the calling script's timeline.
     *
     * The MOT coils are switched off at the end of the exposure.
     */
    public class CaFBECRedImaging : MOTMasterScriptSnippet
    {
        private static readonly string[] V00AOMs =
        {
            "V00R0AOM", "V00R1plusAOMredMOT", "V00B2AOMmolasses", "V00B1minusAOM"
        };

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
            int patternStart = (int)Parameters["PatternStart"];
            int imagingStart = (int)Parameters["ImagingStart"];
            int imagingLightStart = imagingStart + (int)Parameters["CameraTriggerStartDelay"];
            int imagingStop = imagingLightStart + (int)Parameters["CameraTriggerDuration"];
            int cameraStart = imagingLightStart - (int)Parameters["CameraExposureDelayCCDMode"];

            int bgImageStart = (int)Parameters["BackgroundImageTime"];
            int bgCameraStart = bgImageStart - (int)Parameters["CameraExposureDelayCCDMode"];
            int exposure = (int)Parameters["CameraTriggerDuration"];

            // Cloud image
            foreach (string aom in V00AOMs)
                p.Pulse(patternStart, imagingStart, imagingStop - imagingStart, aom);

            if ((double)Parameters["CloudImageONorOFF"] > 5.0)
                p.Pulse(patternStart, cameraStart, exposure, "cameraTrigger");

            // Background image
            foreach (string aom in V00AOMs)
                p.Pulse(patternStart, bgImageStart, exposure, aom);

            if ((double)Parameters["BackgroundImageONorOFF"] > 5.0)
                p.Pulse(patternStart, bgCameraStart, exposure, "cameraTrigger");
        }

        public void AddAnalogSnippet(AnalogPatternBuilder p, Dictionary<String, Object> Parameters)
        {
            p.AddChannel("V00R0AOMVCOAmp");
            p.AddChannel("V00R1plusAOMAmp");
            p.AddChannel("V00B2AOMAmp");
            p.AddChannel("V00B1minusAOMAmp");
            p.AddChannel("motCoils");

            int imagingLightStart = (int)Parameters["ImagingStart"] + (int)Parameters["CameraTriggerStartDelay"];
            int cameraStart = imagingLightStart - (int)Parameters["CameraExposureDelayCCDMode"];
            int cameraStop = cameraStart + (int)Parameters["CameraTriggerDuration"];

            if ((double)Parameters["CloudImageONorOFF"] > 5.0)
            {
                p.AddAnalogValue("V00R0AOMVCOAmp", cameraStart, (double)Parameters["V00F0AOMAmpImaging"]);
                p.AddAnalogValue("V00R1plusAOMAmp", cameraStart, (double)Parameters["V00F1plusAOMAmpImaging"]);
                p.AddAnalogValue("V00B2AOMAmp", cameraStart, (double)Parameters["V00F2AOMAmpImaging"]);
                p.AddAnalogValue("V00B1minusAOMAmp", cameraStart, (double)Parameters["V00F1minusAOMAmpImaging"]);
            }

            p.AddAnalogValue("motCoils", cameraStop, (double)Parameters["MOTCoilsOffValue"]);
        }
    }
}
