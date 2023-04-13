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

        public static GatedDemodulationConfig MakeStandardWideGateConfig()
        {
            GatedDemodulationConfig gateConfig = new GatedDemodulationConfig();
            gateConfig.Name = "Standard gate set";
            gateConfig.AddGate("asymmetry", Gate.WideGate());
            gateConfig.AddGate("bottomProbeScaled", Gate.WideGate());
            gateConfig.AddGate("topProbeNoBackground", Gate.WideGate());
            gateConfig.AddGate("magnetometer", new Gate(1000, 1800, false));
            gateConfig.AddGate("gnd", Gate.WideGate());
            gateConfig.AddGate("battery", Gate.WideGate());
            gateConfig.AddGate("rfCurrent", Gate.WideGate());
            gateConfig.AddGate("reflectedrf1Amplitude", Gate.WideGate());
            gateConfig.AddGate("reflectedrf2Amplitude", Gate.WideGate());
            gateConfig.AddGate("bottomProbeNoBackground", Gate.WideGate());
            gateConfig.AddGate("bottomProbe", Gate.WideGate());
            gateConfig.AddGate("topProbe", Gate.WideGate());

            return gateConfig;
        }

        public int LiveAnalysisGateTimeStartScaled { get { return liveAnalysisGateTimeStartScaled; } }

        public int LiveAnalysisGateTimeEndScaled { get { return liveAnalysisGateTimeEndScaled; } }

        //private int liveAnalysisGateTimeStartBottom = 2550;
        //private int liveAnalysisGateTimeEndBottom = 2650;

        //private int liveAnalysisGateTimeStartScaled = 2933;
        //private int liveAnalysisGateTimeEndScaled = 3048;

        private int liveAnalysisGateTimeStartBottom = 2390;
        private int liveAnalysisGateTimeEndBottom = 2490;

        private int liveAnalysisGateTimeStartScaled = 2748;
        private int liveAnalysisGateTimeEndScaled = 2863;


        public static GatedDemodulationConfig MakeLiveAnalysisGateConfig()
        {
            GatedDemodulationConfig gateConfig = new GatedDemodulationConfig();
            gateConfig.Name = "Live analysis gate set";
            gateConfig.AddGate("asymmetry", new Gate(gateConfig.LiveAnalysisGateTimeStartScaled, gateConfig.LiveAnalysisGateTimeEndScaled, false));
            gateConfig.AddGate("bottomProbeScaled", new Gate(gateConfig.LiveAnalysisGateTimeStartScaled, gateConfig.LiveAnalysisGateTimeEndScaled, true));
            gateConfig.AddGate("topProbeNoBackground", new Gate(gateConfig.LiveAnalysisGateTimeStartScaled, gateConfig.LiveAnalysisGateTimeEndScaled, true));
            gateConfig.AddGate("magnetometer", new Gate(1000, 1800, false));
            gateConfig.AddGate("gnd", Gate.WideGate());
            gateConfig.AddGate("battery", Gate.WideGate());
            gateConfig.AddGate("rfCurrent", Gate.WideGate());
            //gateConfig.AddGate("reflectedrf1Amplitude", Gate.WideGate());
            //gateConfig.AddGate("reflectedrf2Amplitude", Gate.WideGate());
            gateConfig.AddGate("bottomProbeNoBackground", new Gate(2390, 2490, true));
            gateConfig.AddGate("bottomProbe", new Gate(2390, 2490, true));
            gateConfig.AddGate("topProbe", new Gate(gateConfig.LiveAnalysisGateTimeStartScaled, gateConfig.LiveAnalysisGateTimeEndScaled, true));

            return gateConfig;
        }
    }
}