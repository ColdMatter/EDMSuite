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
    /// This config consists of a set of gates used for each detector, as well
    /// as the gating function used (integrate/mean). 
    /// </summary>
    [Serializable]
    public class GatedDemodulationConfig
    {
        public string Name = "";
        private readonly List<string> pointDetectorList = new List<string>();
        private readonly List<string> gatedDetectorList = new List<string>();
        private readonly List<Gate> gateList = new List<Gate>();

        public void AddGate(string detector, Gate gate)
        {
            gateList.Add(gate);
            gatedDetectorList.Add(detector);
        }

        public void AddGate(string detector, int gateLow, int gateHigh, bool integrate)
        {
            Gate gate = new Gate(gateLow, gateHigh, integrate);
            gateList.Add(gate);
            gatedDetectorList.Add(detector);
        }

        public Gate GetGate(string detector)
        {
            return gateList[gatedDetectorList.IndexOf(detector)];
        }

        public List<Gate> Gates
        {
            get
            {
                return gateList;
            }
        }

        public List<string> GatedDetectors
        {
            get
            {
                return gatedDetectorList;
            }
        }

        public void AddPointDetector(string pdName)
        {
            if (!pointDetectorList.Contains(pdName)) pointDetectorList.Add(pdName);
        }

        public List<string> PointDetectors
        {
            get
            {
                return pointDetectorList;
            }
        }

        public static GatedDemodulationConfig MakeStandardWideGateConfig()
        {
            GatedDemodulationConfig gateConfig = new GatedDemodulationConfig();
            gateConfig.Name = "Standard gate set";
            gateConfig.AddGate("asymmetry", Gate.WideGate());
            gateConfig.AddGate("bottomProbeScaled", Gate.WideGate());
            gateConfig.AddGate("topProbeNoBackground", Gate.WideGate());
            gateConfig.AddGate("magnetometer", new Gate(1000, 1800, false));
            gateConfig.AddGate("gnd", new Gate(1000, 1800, false));
            gateConfig.AddGate("battery", new Gate(1000, 1800, false));
            gateConfig.AddGate("rfCurrent", new Gate(1000, 1800, false));
            gateConfig.AddGate("reflectedrf1Amplitude", new Gate(1000, 1800, false));
            gateConfig.AddGate("reflectedrf2Amplitude", new Gate(1000, 1800, false));
            gateConfig.AddGate("bottomProbeNoBackground", Gate.WideGate());

            return gateConfig;
        }
    }
}