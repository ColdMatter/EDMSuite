using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

using Data.EDM;
using Utility;


namespace Analysis.EDM
{
    /// <summary>
    /// This config contains the list of TOF-demodulated detectors,
    /// gated-then-demodulated detectors, and point detectors
    /// </summary>
    [Serializable]
    public class DemodulationConfig
    {
        public string Name = "";
        private readonly List<string> _tofDetectorList = new List<string>();
        private readonly List<string> _gatedDetectorList = new List<string>();
        private readonly List<string> _pointDetectorList = new List<string>();
        private readonly GatedDemodulationConfig _gateConfig = new GatedDemodulationConfig();

        public void AddTOFDetector(string detector)
        {
            _tofDetectorList.Add(detector);
        }

        public void AddGatedDetector(string detector, Gate gate)
        {
            _gatedDetectorList.Add(detector);
            _gateConfig.AddGate(detector, gate);
        }

        public void AddPointDetector(string detector)
        {
            _pointDetectorList.Add(detector);
        }

        public List<string> TOFDetectors
        {
            get
            {
                return _tofDetectorList;
            }
        }

        public List<string> GatedDetectors
        {
            get
            {
                return _gatedDetectorList;
            }
        }

        public List<string> PointDetectors
        {
            get
            {
                return _pointDetectorList;
            }
        }

        public GatedDemodulationConfig Gates
        {
            get
            {
                return _gateConfig;
            }
        }

        public static DemodulationConfig MakeStandardDemodulationConfig()
        {
            DemodulationConfig dc = new DemodulationConfig();

            dc.AddTOFDetector("asymmetry");
            dc.AddTOFDetector("bottomProbeScaled");
            dc.AddTOFDetector("topProbeNoBackground");
            dc.AddTOFDetector("battery");
            dc.AddTOFDetector("asymmetryShotNoiseVariance");

            dc.AddGatedDetector("magnetometer", Gate.WideGate());
            dc.AddGatedDetector("gnd", Gate.WideGate());
            dc.AddGatedDetector("rfCurrent", Gate.WideGate());
            dc.AddGatedDetector("reflectedrf1Amplitude", Gate.WideGate());
            dc.AddGatedDetector("reflectedrf2Amplitude", Gate.WideGate());

            dc.AddPointDetector("PhaseLockFrequency");
            dc.AddPointDetector("PhaseLockError");
            dc.AddPointDetector("NorthCurrent");
            dc.AddPointDetector("SouthCurrent");
            dc.AddPointDetector("BottomDetectorBackground");
            dc.AddPointDetector("TopDetectorBackground");

            return dc;
        }

        public static DemodulationConfig MakeLiveAnalysisConfig()
        {
            DemodulationConfig dc = new DemodulationConfig();

            dc.AddTOFDetector("asymmetry");
            dc.AddGatedDetector("bottomProbeScaled", Gate.WideGate());
            dc.AddGatedDetector("topProbeNoBackground", Gate.WideGate());

            dc.AddGatedDetector("magnetometer", Gate.WideGate());
            dc.AddGatedDetector("rfCurrent", Gate.WideGate());
            dc.AddGatedDetector("reflectedrf1Amplitude", Gate.WideGate());
            dc.AddGatedDetector("reflectedrf2Amplitude", Gate.WideGate());

            dc.AddPointDetector("PhaseLockFrequency");
            dc.AddPointDetector("PhaseLockError");
            dc.AddPointDetector("NorthCurrent");
            dc.AddPointDetector("SouthCurrent");
            dc.AddPointDetector("BottomDetectorBackground");
            dc.AddPointDetector("TopDetectorBackground");

            return dc;
        }
    }
}