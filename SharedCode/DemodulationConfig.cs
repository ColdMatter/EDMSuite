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
            
            // a wide gate - integrate everything
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

            // fwhm of the tof pulse for top and norm, wide gates for everything else.
            AddSliceConfig("fwhm", 0, 1);
            // narrower than fwhm, takes only the center hwhm
            AddSliceConfig("hwhm", 0, 0.5);
            // only the fast half of the fwhm
            AddSliceConfig("fast", -0.5, 0.5);
            // the slow half of the fwhm
            AddSliceConfig("slow", 0.5, 0.5);

            // now some finer slices
            double d = -1.4;
            for (int i = 0; i < 15; i++)
            {
                AddSliceConfig("slice" + i , d, 0.2);
                d += 0.2;
            }
        }

        private static void AddSliceConfig(string name, double offset, double width)
        {
            // the slow half of the fwhm
            DemodulationConfigBuilder dcb = delegate(Block b)
            {
                DemodulationConfig dc;
                GatedDetectorExtractSpec dg0, dg1, dg2, dg3, dg4;

                dc = new DemodulationConfig();
                dc.AnalysisTag = name;
                dg0 = GatedDetectorExtractSpec.MakeGateFWHM(b, 0, offset, width);
                dg0.Name = "top";
                dg0.BackgroundSubtract = true;
                dg1 = GatedDetectorExtractSpec.MakeGateFWHM(b, 1, offset, width);
                dg1.Name = "norm";
                dg0.BackgroundSubtract = true;
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
            standardConfigs.Add(name, dcb);
        }

     }


     public delegate DemodulationConfig DemodulationConfigBuilder(Block b);
}
