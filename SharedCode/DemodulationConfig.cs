using System;
using System.Collections.Generic;
using System.Text;

using Data.EDM;


namespace Analysis.EDM
{
    // Note that all FWHM based gates have been disabled to remove SharedCode's dependence on the
    // NI analysis libraries.

    /// <summary>
    /// This is a bit confusing looking, but it's pretty simple to use. Instances of this class
    /// tell the BlockDemodulator how to extract the data. The class also provides a standard
    /// library of configurations, accessed through the static member GetStandardDemodulationConfig.
    /// This is usually the way you'll want to use the class, although you are of course free to build
    /// your own configurations on the fly.
    /// 
    /// I have a nagging feeling that there might be a simpler way to do this, but I can't see it at
    /// the minute.
    /// </summary>
    [Serializable]
    public class DemodulationConfig
    {
        public Dictionary<string, GatedDetectorExtractSpec> GatedDetectorExtractSpecs =
            new Dictionary<string, GatedDetectorExtractSpec>();
        public List<string> PointDetectorChannels = new List<string>();
        public String AnalysisTag = "";

        // static members for making standard configs
        private static Dictionary<string, DemodulationConfigBuilder> standardConfigs =
            new Dictionary<string, DemodulationConfigBuilder>();

        // ratio of distance from source for the two detectors
        // private static double kDetectorDistanceRatio = 1715.0/1500.0;

        public static DemodulationConfig GetStandardDemodulationConfig(string name, Block b)
        {
            return (standardConfigs[name])(b);
        }

        static DemodulationConfig()
        {
            // here we stock the class' static library of configs up with some standard configs

            // a wide gate - integrate everything
            DemodulationConfigBuilder wide = delegate(Block b)
            {

                DemodulationConfig dc;
                GatedDetectorExtractSpec dg0, dg1, dg2, dg3, dg4, dg5, dg6, dg7, dg8, dg9, dg10, dg11;

                dc = new DemodulationConfig();
                dc.AnalysisTag = "wide";
                dg0 = GatedDetectorExtractSpec.MakeWideGate("bottomProbe", true);
                dg1 = GatedDetectorExtractSpec.MakeWideGate("topProbe", true);
                dg2 = GatedDetectorExtractSpec.MakeWideGate("magnetometer", false);
                dg3 = GatedDetectorExtractSpec.MakeWideGate("gnd", false);
                dg4 = GatedDetectorExtractSpec.MakeWideGate("battery", false);
                dg5 = GatedDetectorExtractSpec.MakeWideGate("rfCurrent", false);
                dg6 = GatedDetectorExtractSpec.MakeWideGate("reflectedrf1Amplitude", false);
                dg7 = GatedDetectorExtractSpec.MakeWideGate("reflectedrf2Amplitude", false);
                dg8 = GatedDetectorExtractSpec.MakeWideGate("bottomProbeNoBackground", true);
                dg9 = GatedDetectorExtractSpec.MakeWideGate("topProbeNoBackground", true);
                dg10 = GatedDetectorExtractSpec.MakeWideGate("bottomProbeScaled", true);
                dg11 = GatedDetectorExtractSpec.MakeWideGate("asymmetry", true);

                dc.GatedDetectorExtractSpecs.Add(dg0.Name, dg0);
                dc.GatedDetectorExtractSpecs.Add(dg1.Name, dg1);
                dc.GatedDetectorExtractSpecs.Add(dg2.Name, dg2);
                dc.GatedDetectorExtractSpecs.Add(dg3.Name, dg3);
                dc.GatedDetectorExtractSpecs.Add(dg4.Name, dg4);
                dc.GatedDetectorExtractSpecs.Add(dg5.Name, dg5);
                dc.GatedDetectorExtractSpecs.Add(dg6.Name, dg6);
                dc.GatedDetectorExtractSpecs.Add(dg7.Name, dg7);
                dc.GatedDetectorExtractSpecs.Add(dg8.Name, dg8);
                dc.GatedDetectorExtractSpecs.Add(dg9.Name, dg9);
                dc.GatedDetectorExtractSpecs.Add(dg10.Name, dg10);
                dc.GatedDetectorExtractSpecs.Add(dg11.Name, dg11);

                dc.PointDetectorChannels.Add("MiniFlux1");
                dc.PointDetectorChannels.Add("MiniFlux2");
                dc.PointDetectorChannels.Add("MiniFlux3");
                dc.PointDetectorChannels.Add("NorthCurrent");
                dc.PointDetectorChannels.Add("SouthCurrent");
                dc.PointDetectorChannels.Add("PumpPD");
                dc.PointDetectorChannels.Add("ProbePD");

                return dc;
            };
            standardConfigs.Add("wide", wide);

            // to generate channel values for the live block analysis for BlockHead
            DemodulationConfigBuilder liveBlockAnalysis = delegate(Block b)
            {
                var dc = new DemodulationConfig();
                dc.AnalysisTag = "liveBlockAnalysis";

                var dg0 = GatedDetectorExtractSpec.MakeGate("asymmetry", 2700, 2900, true);
                var dg1 = GatedDetectorExtractSpec.MakeGate("bottomProbeScaled", 2700, 2900, true);
                var dg2 = GatedDetectorExtractSpec.MakeGate("topProbeNoBackground", 2700, 2900, true);
                var dg3 = GatedDetectorExtractSpec.MakeWideGate("magnetometer", false);

                dc.GatedDetectorExtractSpecs.Add(dg0.Name, dg0);
                dc.GatedDetectorExtractSpecs.Add(dg1.Name, dg1);
                dc.GatedDetectorExtractSpecs.Add(dg2.Name, dg2);
                dc.GatedDetectorExtractSpecs.Add(dg3.Name, dg3);

                dc.PointDetectorChannels.Add("NorthCurrent");
                dc.PointDetectorChannels.Add("SouthCurrent");

                return dc;
            };
            standardConfigs.Add("liveBlockAnalysis", liveBlockAnalysis);

            // "background" gate
            DemodulationConfigBuilder background = delegate(Block b)
            {

                DemodulationConfig dc;
                GatedDetectorExtractSpec dg0, dg1;

                dc = new DemodulationConfig();
                dc.AnalysisTag = "background";
                dg0 = new GatedDetectorExtractSpec();
                dg0.Name = "bottomProbe";
                dg0.GateLow = 2800;
                dg0.GateHigh = 2900;
                dg0.Integrate = false;
                dg1 = new GatedDetectorExtractSpec();
                dg1.Name = "topProbe";
                dg1.GateLow = 3200;
                dg1.GateHigh = 3300;
                dg0.Integrate = false;

                dc.GatedDetectorExtractSpecs.Add(dg0.Name, dg0);
                dc.GatedDetectorExtractSpecs.Add(dg1.Name, dg1);


                return dc;
            };
            standardConfigs.Add("background", background);
        }

        //private static void AddFixedSliceConfig(string name, double centre, double width)
        //{
        //    // the slow half of the fwhm
        //    DemodulationConfigBuilder dcb = delegate(Block b)
        //    {
        //        DemodulationConfig dc;
        //        GatedDetectorExtractSpec dg0, dg1, dg2, dg3, dg4, dg5, dg6, dg7, dg8;

        //        dc = new DemodulationConfig();
        //        dc.AnalysisTag = name;
        //        dg0 = new GatedDetectorExtractSpec();
        //        dg0.Index = 0;
        //        dg0.Name = "bottom";
        //        dg0.BackgroundSubtract = false;
        //        dg0.GateLow = (int)((centre - width) * kDetectorDistanceRatio);
        //        dg0.GateHigh = (int)((centre + width) * kDetectorDistanceRatio);
        //        dg1 = new GatedDetectorExtractSpec();
        //        dg1.Index = 1;
        //        dg1.Name = "top";
        //        dg1.BackgroundSubtract = false;
        //        dg1.GateLow = (int)(centre - width);
        //        dg1.GateHigh = (int)(centre + width);
        //        dg2 = GatedDetectorExtractSpec.MakeWideGate(2);
        //        dg2.Name = "magnetometer";
        //        dg2.Integrate = false;
        //        dg3 = GatedDetectorExtractSpec.MakeWideGate(3);
        //        dg3.Name = "gnd";
        //        dg3.Integrate = false;
        //        dg4 = GatedDetectorExtractSpec.MakeWideGate(4);
        //        dg4.Name = "battery";
        //        dg5 = GatedDetectorExtractSpec.MakeWideGate(5);
        //        dg5.Name = "rfCurrent";
        //        dg5.Integrate = false;
        //        dg6 = new GatedDetectorExtractSpec();
        //        dg6.Index = 6;
        //        dg6.Name = "reflectedrf1Amplitude";
        //        dg6.BackgroundSubtract = false;
        //        dg6.GateLow = 819;
        //        dg6.GateHigh = 821;
        //        dg7 = new GatedDetectorExtractSpec();
        //        dg7.Index = 7;
        //        dg7.Name = "reflectedrf2Amplitude";
        //        dg7.BackgroundSubtract = false;
        //        dg7.GateLow = 1799;
        //        dg7.GateHigh = 1801;
        //        dg8 = new GatedDetectorExtractSpec();
        //        dg8.Index = 8;
        //        dg8.Name = "asymmetry";
        //        dg8.BackgroundSubtract = false;
        //        dg8.GateLow = (int)(centre - width);
        //        dg8.GateHigh = (int)(centre + width);


        //        dc.GatedDetectorExtractSpecs.Add(dg0.Name, dg0);
        //        dc.GatedDetectorExtractSpecs.Add(dg1.Name, dg1);
        //        dc.GatedDetectorExtractSpecs.Add(dg2.Name, dg2);
        //        dc.GatedDetectorExtractSpecs.Add(dg3.Name, dg3);
        //        dc.GatedDetectorExtractSpecs.Add(dg4.Name, dg4);
        //        dc.GatedDetectorExtractSpecs.Add(dg5.Name, dg5);
        //        dc.GatedDetectorExtractSpecs.Add(dg6.Name, dg6);
        //        dc.GatedDetectorExtractSpecs.Add(dg7.Name, dg7);

        //        dc.PointDetectorChannels.Add("MiniFlux1");
        //        dc.PointDetectorChannels.Add("MiniFlux2");
        //        dc.PointDetectorChannels.Add("MiniFlux3");
        //        dc.PointDetectorChannels.Add("NorthCurrent");
        //        dc.PointDetectorChannels.Add("SouthCurrent");
        //        dc.PointDetectorChannels.Add("PumpPD");
        //        dc.PointDetectorChannels.Add("ProbePD");

        //        return dc;
        //    };
        //    standardConfigs.Add(name, dcb);
        //}


    }


    public delegate DemodulationConfig DemodulationConfigBuilder(Block b);
}