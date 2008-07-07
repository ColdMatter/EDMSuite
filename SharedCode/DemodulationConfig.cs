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
    /// This is usally the way you'll want to use the class, although you are of course free to build
    /// your own configurations on the fly.
    /// 
    /// I have a nagging feeling that there might be a simpler way to do this, but I can't see it at
    /// the minute.
    /// </summary>
    [Serializable]
    public class DemodulationConfig
    {
        public Dictionary<string, DetectorExtractSpec> DetectorExtractSpecs = 
            new Dictionary<string, DetectorExtractSpec>();
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
                DetectorExtractSpec dg0, dg1, dg2, dg3, dg4;

                dc = new DemodulationConfig();
                dc.AnalysisTag = "fwhm";
                dg0 = DetectorExtractSpec.MakeGateFWHM(b, 0, 0, 1);
                dg0.Name = "top";
                dg1 = DetectorExtractSpec.MakeGateFWHM(b, 1, 0, 1);
                dg1.Name = "norm";
                dg2 = DetectorExtractSpec.MakeWideGate(2);
                dg2.Name = "mag1";
                dg2.Integrate = false;
                dg3 = DetectorExtractSpec.MakeWideGate(3);
                dg3.Name = "short";
                dg3.Integrate = false;
                dg4 = DetectorExtractSpec.MakeWideGate(4);
                dg4.Name = "battery";

                dc.DetectorExtractSpecs.Add(dg0.Name, dg0);
                dc.DetectorExtractSpecs.Add(dg1.Name, dg1);
                dc.DetectorExtractSpecs.Add(dg2.Name, dg2);
                dc.DetectorExtractSpecs.Add(dg3.Name, dg3);
                dc.DetectorExtractSpecs.Add(dg4.Name, dg4);

                return dc;
            };
            standardConfigs.Add("fwhm", fwhm);

            DemodulationConfigBuilder wide = delegate(Block b)
            {

                DemodulationConfig dc;
                DetectorExtractSpec dg0, dg1, dg2, dg3, dg4;

                dc = new DemodulationConfig();
                dc.AnalysisTag = "wide";
                dg0 = DetectorExtractSpec.MakeWideGate(0);
                dg0.Name = "top";
                dg1 = DetectorExtractSpec.MakeWideGate(1);
                dg1.Name = "norm";
                dg2 = DetectorExtractSpec.MakeWideGate(2);
                dg2.Name = "mag1";
                dg2.Integrate = false;
                dg3 = DetectorExtractSpec.MakeWideGate(3);
                dg3.Name = "short";
                dg3.Integrate = false;
                dg4 = DetectorExtractSpec.MakeWideGate(4);
                dg4.Name = "battery";

                dc.DetectorExtractSpecs.Add(dg0.Name, dg0);
                dc.DetectorExtractSpecs.Add(dg1.Name, dg1);
                dc.DetectorExtractSpecs.Add(dg2.Name, dg2);
                dc.DetectorExtractSpecs.Add(dg3.Name, dg3);
                dc.DetectorExtractSpecs.Add(dg4.Name, dg4);

                return dc;
            };
            standardConfigs.Add("wide", wide);

  
        }

     }

     public delegate DemodulationConfig DemodulationConfigBuilder(Block b);
}
