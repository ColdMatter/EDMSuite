using System;
using System.Collections.Generic;
using System.Text;

using Data.EDM;


namespace Analysis.EDM
{
    /// <summary>
    /// This config consists of a set of gates used for each detector, as well
    /// as the gating function used (integrate/mean). 
    /// </summary>
    [Serializable]
    public class GatedDemodulationConfig
    {
        public string Name;
        private readonly Dictionary<string, Gate> DetectorGates = new Dictionary<string, Gate>();
        private readonly List<string> PointDetectorList = new List<string>();

        public void AddGate(string detector, Gate gate)
        {
            if (DetectorGates.ContainsKey(detector)) DetectorGates.Remove(detector);
            DetectorGates.Add(detector, gate);
        }

        public void AddGate(string detector, int gateLow, int gateHigh, bool integrate)
        {
            Gate gate = new Gate(gateLow, gateHigh, integrate);
            if (DetectorGates.ContainsKey(detector)) DetectorGates.Remove(detector);
            DetectorGates.Add(detector, gate);
        }

        public Gate GetGate(string detector)
        {
            return DetectorGates[detector];
        }

        public List<string> Gates
        {
            get
            {
                Dictionary<string, Gate>.KeyCollection keys = DetectorGates.Keys;
                List<string> keyArray = new List<string>();
                foreach (string key in keys) keyArray.Add(key);
                return keyArray;
            }
        }

        public void AddPointDetector(string pdName)
        {
            if (!PointDetectorList.Contains(pdName)) PointDetectorList.Add(pdName);
        }

        public List<string> PointDetectors
        {
            get
            {
                return PointDetectorList;
            }
        }

        public static GatedDemodulationConfig MakeStandardWideGateConfig()
        {
            GatedDemodulationConfig gateConfig = new GatedDemodulationConfig();
            gateConfig.AddGate("asymmetry", Gate.WideGate());
            gateConfig.AddGate("bottomProbeScaled", Gate.WideGate());
            gateConfig.AddGate("topProbeNoBackground", Gate.WideGate());
            gateConfig.AddGate("magnetometer", Gate.WideGate());
            gateConfig.AddPointDetector("NorthCurrent");
            gateConfig.AddPointDetector("SouthCurrent");

            return gateConfig;
        }
    }
}