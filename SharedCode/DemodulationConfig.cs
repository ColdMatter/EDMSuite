using System;
using System.Collections.Generic;
using System.Text;

using Data.EDM;

//** Ok
namespace Analysis.EDM
{
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

        public static DemodulationConfig GetStandardDemodulationConfig(string name, Block b)
        {
            return (standardConfigs[name])(b);
        }

        static DemodulationConfig()
        {
            // here we stock the class' static library of configs up with some standard configs

            // first, fwhm of the tof pulse for top and norm, wide gates for everything else.
            DemodulationConfigBuilder fwhm = delegate(Block b)
            {
                DemodulationConfig dc;
                GatedDetectorExtractSpec dg0, dg1, dg2, dg3, dg4;

                dc = new DemodulationConfig();
                dc.AnalysisTag = "fwhm";
                dg0 = GatedDetectorExtractSpec.MakeGateFWHM(b, 0, 0, 1);
                dg0.Name = "top";
                dg1 = GatedDetectorExtractSpec.MakeGateFWHM(b, 1, 0, 1);
                dg1.Name = "norm";
                dg2 = GatedDetectorExtractSpec.MakeWideGate(2);
                dg2.Name = "mag1";
                dg2.Integrate = false;
                dg3 = GatedDetectorExtractSpec.MakeWideGate(3);
                dg3.Name = "short";
                dg3.Integrate = false;
                dg4 = GatedDetectorExtractSpec.MakeWideGate(4);
                dg4.Name = "battery";

                dc.GatedDetectorExtractSpecs.Add(dg0.Name, dg0);
                dc.GatedDetectorExtractSpecs.Add(dg1.Name, dg1);
                dc.GatedDetectorExtractSpecs.Add(dg2.Name, dg2);
                dc.GatedDetectorExtractSpecs.Add(dg3.Name, dg3);
                dc.GatedDetectorExtractSpecs.Add(dg4.Name, dg4);

                dc.PointDetectorChannels.Add("MiniFlux1");
                dc.PointDetectorChannels.Add("MiniFlux2");
                dc.PointDetectorChannels.Add("MiniFlux3");
                dc.PointDetectorChannels.Add("NorthCurrent");
                dc.PointDetectorChannels.Add("SouthCurrent");

                return dc;
            };
            standardConfigs.Add("fwhm", fwhm);

            DemodulationConfigBuilder wide = delegate(Block b)
            {

                DemodulationConfig dc;
                GatedDetectorExtractSpec dg0, dg1, dg2, dg3, dg4;

                dc = new DemodulationConfig();
                dc.AnalysisTag = "wide";
                dg0 = GatedDetectorExtractSpec.MakeWideGate(0);
                dg0.Name = "top";
                dg1 = GatedDetectorExtractSpec.MakeWideGate(1);
                dg1.Name = "norm";
                dg2 = GatedDetectorExtractSpec.MakeWideGate(2);
                dg2.Name = "mag1";
                dg2.Integrate = false;
                dg3 = GatedDetectorExtractSpec.MakeWideGate(3);
                dg3.Name = "short";
                dg3.Integrate = false;
                dg4 = GatedDetectorExtractSpec.MakeWideGate(4);
                dg4.Name = "battery";

                dc.GatedDetectorExtractSpecs.Add(dg0.Name, dg0);
                dc.GatedDetectorExtractSpecs.Add(dg1.Name, dg1);
                dc.GatedDetectorExtractSpecs.Add(dg2.Name, dg2);
                dc.GatedDetectorExtractSpecs.Add(dg3.Name, dg3);
                dc.GatedDetectorExtractSpecs.Add(dg4.Name, dg4);

                dc.PointDetectorChannels.Add("MiniFlux1");
                dc.PointDetectorChannels.Add("MiniFlux2");
                dc.PointDetectorChannels.Add("MiniFlux3");
                dc.PointDetectorChannels.Add("NorthCurrent");
                dc.PointDetectorChannels.Add("SouthCurrent");

                return dc;
            };
            standardConfigs.Add("wide", wide);

            // narrower than fwhm, takes only the center hwhm
            DemodulationConfigBuilder hwhm = delegate(Block b)
            {
                DemodulationConfig dc;
                GatedDetectorExtractSpec dg0, dg1, dg2, dg3, dg4;

                dc = new DemodulationConfig();
                dc.AnalysisTag = "hwhm";
                dg0 = GatedDetectorExtractSpec.MakeGateFWHM(b, 0, 0, 0.5);
                dg0.Name = "top";
                dg1 = GatedDetectorExtractSpec.MakeGateFWHM(b, 1, 0, 0.5);
                dg1.Name = "norm";
                dg2 = GatedDetectorExtractSpec.MakeWideGate(2);
                dg2.Name = "mag1";
                dg2.Integrate = false;
                dg3 = GatedDetectorExtractSpec.MakeWideGate(3);
                dg3.Name = "short";
                dg3.Integrate = false;
                dg4 = GatedDetectorExtractSpec.MakeWideGate(4);
                dg4.Name = "battery";

                dc.GatedDetectorExtractSpecs.Add(dg0.Name, dg0);
                dc.GatedDetectorExtractSpecs.Add(dg1.Name, dg1);
                dc.GatedDetectorExtractSpecs.Add(dg2.Name, dg2);
                dc.GatedDetectorExtractSpecs.Add(dg3.Name, dg3);
                dc.GatedDetectorExtractSpecs.Add(dg4.Name, dg4);

                dc.PointDetectorChannels.Add("MiniFlux1");
                dc.PointDetectorChannels.Add("MiniFlux2");
                dc.PointDetectorChannels.Add("MiniFlux3");
                dc.PointDetectorChannels.Add("NorthCurrent");
                dc.PointDetectorChannels.Add("SouthCurrent");

                return dc;
            };
            standardConfigs.Add("hwhm", hwhm);

            // only the fast half of the fwhm
            DemodulationConfigBuilder fast = delegate(Block b)
            {
                DemodulationConfig dc;
                GatedDetectorExtractSpec dg0, dg1, dg2, dg3, dg4;

                dc = new DemodulationConfig();
                dc.AnalysisTag = "fast";
                dg0 = GatedDetectorExtractSpec.MakeGateFWHM(b, 0, -0.5, 0.5);
                dg0.Name = "top";
                dg1 = GatedDetectorExtractSpec.MakeGateFWHM(b, 1, -0.5, 0.5);
                dg1.Name = "norm";
                dg2 = GatedDetectorExtractSpec.MakeWideGate(2);
                dg2.Name = "mag1";
                dg2.Integrate = false;
                dg3 = GatedDetectorExtractSpec.MakeWideGate(3);
                dg3.Name = "short";
                dg3.Integrate = false;
                dg4 = GatedDetectorExtractSpec.MakeWideGate(4);
                dg4.Name = "battery";

                dc.GatedDetectorExtractSpecs.Add(dg0.Name, dg0);
                dc.GatedDetectorExtractSpecs.Add(dg1.Name, dg1);
                dc.GatedDetectorExtractSpecs.Add(dg2.Name, dg2);
                dc.GatedDetectorExtractSpecs.Add(dg3.Name, dg3);
                dc.GatedDetectorExtractSpecs.Add(dg4.Name, dg4);

                dc.PointDetectorChannels.Add("MiniFlux1");
                dc.PointDetectorChannels.Add("MiniFlux2");
                dc.PointDetectorChannels.Add("MiniFlux3");
                dc.PointDetectorChannels.Add("NorthCurrent");
                dc.PointDetectorChannels.Add("SouthCurrent");

                return dc;
            };
            standardConfigs.Add("fast", fast);

            // the slow half of the fwhm
            DemodulationConfigBuilder slow = delegate(Block b)
            {
                DemodulationConfig dc;
                GatedDetectorExtractSpec dg0, dg1, dg2, dg3, dg4;

                dc = new DemodulationConfig();
                dc.AnalysisTag = "slow";
                dg0 = GatedDetectorExtractSpec.MakeGateFWHM(b, 0, 0.5, 0.5);
                dg0.Name = "top";
                dg1 = GatedDetectorExtractSpec.MakeGateFWHM(b, 1, 0.5, 0.5);
                dg1.Name = "norm";
                dg2 = GatedDetectorExtractSpec.MakeWideGate(2);
                dg2.Name = "mag1";
                dg2.Integrate = false;
                dg3 = GatedDetectorExtractSpec.MakeWideGate(3);
                dg3.Name = "short";
                dg3.Integrate = false;
                dg4 = GatedDetectorExtractSpec.MakeWideGate(4);
                dg4.Name = "battery";

                dc.GatedDetectorExtractSpecs.Add(dg0.Name, dg0);
                dc.GatedDetectorExtractSpecs.Add(dg1.Name, dg1);
                dc.GatedDetectorExtractSpecs.Add(dg2.Name, dg2);
                dc.GatedDetectorExtractSpecs.Add(dg3.Name, dg3);
                dc.GatedDetectorExtractSpecs.Add(dg4.Name, dg4);

                dc.PointDetectorChannels.Add("MiniFlux1");
                dc.PointDetectorChannels.Add("MiniFlux2");
                dc.PointDetectorChannels.Add("MiniFlux3");
                dc.PointDetectorChannels.Add("NorthCurrent");
                dc.PointDetectorChannels.Add("SouthCurrent");

                return dc;
            };
            standardConfigs.Add("slow", slow);

        }

     }

     public delegate DemodulationConfig DemodulationConfigBuilder(Block b);
}
