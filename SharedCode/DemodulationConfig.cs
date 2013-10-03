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

        private static double kDetectorDistanceRatio = 3.842;

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
                GatedDetectorExtractSpec dg0, dg1, dg2, dg3, dg4, dg5, dg6, dg7;

                dc = new DemodulationConfig();
                dc.AnalysisTag = "wide";
                dg0 = GatedDetectorExtractSpec.MakeWideGate(0);
                dg0.Name = "top";
                dg1 = GatedDetectorExtractSpec.MakeWideGate(1);
                dg1.Name = "norm";
                dg2 = GatedDetectorExtractSpec.MakeWideGate(2);
                dg2.Name = "magnetometer";
                dg2.Integrate = false;
                dg3 = GatedDetectorExtractSpec.MakeWideGate(3);
                dg3.Name = "gnd";
                dg3.Integrate = false;
                dg4 = GatedDetectorExtractSpec.MakeWideGate(4);
                dg4.Name = "battery";
                dg5 = GatedDetectorExtractSpec.MakeWideGate(5);
                dg5.Name = "rfCurrent";
                dg5.Integrate = false;
                dg6 = GatedDetectorExtractSpec.MakeWideGate(6);
                dg6.Name = "reflectedrf1Amplitude";
                dg7 = GatedDetectorExtractSpec.MakeWideGate(7);
                dg7.Name = "reflectedrf2Amplitude";

                dc.GatedDetectorExtractSpecs.Add(dg0.Name, dg0);
                dc.GatedDetectorExtractSpecs.Add(dg1.Name, dg1);
                dc.GatedDetectorExtractSpecs.Add(dg2.Name, dg2);
                dc.GatedDetectorExtractSpecs.Add(dg3.Name, dg3);
                dc.GatedDetectorExtractSpecs.Add(dg4.Name, dg4);
                dc.GatedDetectorExtractSpecs.Add(dg5.Name, dg5);
                dc.GatedDetectorExtractSpecs.Add(dg6.Name, dg6);
                dc.GatedDetectorExtractSpecs.Add(dg7.Name, dg7);

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

            //// fwhm of the tof pulse for top and norm, wide gates for everything else.
            //AddSliceConfig("fwhm", 0, 1);
            //// narrower than fwhm, takes only the center hwhm
            //AddSliceConfig("hwhm", 0, 0.5);
            //// only the fast half of the fwhm (NOT TRUE - 01Jul08 JH)
            //AddSliceConfig("fast", -0.5, 0.5);
            //// the slow half of the fwhm (NOT TRUE - 01Jul08 JH)
            //AddSliceConfig("slow", 0.5, 0.5);
            //// the fastest and slowest molecules, used for estimating any tof related systematic.
            //// these gates don't overlap with the usual centred analysis gates (fwhm and cgate11).
            //AddSliceConfig("vfast", -0.85, 0.5);
            //AddSliceConfig("vslow", 0.85, 0.5);

            // for testing out different centred-gate widths
            for (int i = 1; i < 10; i++)
                AddFixedSliceConfig("wgate" + i, 2190, i * 20);

            for (int i = 1; i < 30; i++)
                AddFixedSliceConfig("pgate" + i, 2090 + i*10, 10);


            //// testing different gate centres. "slide0" is centred at -0.7 fwhm, "slide14"
            //// is centred and +0.7 fwhm.
            //for (int i = 0; i < 15; i++)
            //    AddSliceConfig("slide" + i, (((double)i) / 10.0) - 0.7, 1);

            //// now some finer slices
            //double d = -1.4;
            //for (int i = 0; i < 15; i++)
            //{
            //    AddSliceConfig("slice" + i, d, 0.2);
            //    d += 0.2;
            //}
            
            //// optimised gates for spring 2009 run
            //AddSliceConfig("optimum1", 0.3, 1.1);
            //AddSliceConfig("optimum2", 0.2, 1.1);

            // "background" gate
            DemodulationConfigBuilder background = delegate(Block b)
            {

                DemodulationConfig dc;
                GatedDetectorExtractSpec dg0, dg1;

                dc = new DemodulationConfig();
                dc.AnalysisTag = "background";
                dg0 = GatedDetectorExtractSpec.MakeWideGate(0);
                dg0.GateLow = 2550;
                dg0.GateHigh = 2600;
                dg0.Name = "top";
                dg1 = GatedDetectorExtractSpec.MakeWideGate(1);
                dg1.Name = "norm";
                dg1.GateLow = 750;
                dg1.GateHigh = 800;

                dc.GatedDetectorExtractSpecs.Add(dg0.Name, dg0);
                dc.GatedDetectorExtractSpecs.Add(dg1.Name, dg1);


                return dc;
            };
            standardConfigs.Add("background", background);

            // add some fixed gate slices - the first three are the 1.1 sigma centre portion and two
            // non-overlapping portions either side.
            AddFixedSliceConfig("cgate11Fixed", 2156, 90); // This is normally ("cgate11Fixed", 2156, 90)
            AddFixedSliceConfig("vfastFixed", 2025, 41);
            AddFixedSliceConfig("vslowFixed", 2286, 41);
            // these "nudge" gates are chosen to, hopefully, tweak the 09_10 dataset so that the
            // RF1F channel, DB-normed in the non-linear way, is reduced to near zero
            AddFixedSliceConfig("nudgeGate1", 2161, 90);
            AddFixedSliceConfig("nudgeGate2", 2169, 90);
            AddFixedSliceConfig("nudgeGate3", 2176, 90);
            AddFixedSliceConfig("nudgeGate4", 2174, 90);
            AddFixedSliceConfig("nudgeGate5", 2188, 90);
            AddFixedSliceConfig("nudgeGate6", 2198, 90);
            AddFixedSliceConfig("nudgeGate7", 2208, 90);
            AddFixedSliceConfig("nudgeGate8", 2228, 90);
            AddFixedSliceConfig("wideNudgeGate1", 2198, 100);
            AddFixedSliceConfig("narrowNudgeGate1", 2198, 75);
            AddFixedSliceConfig("narrowNudgeGate2", 2198, 65);
            AddFixedSliceConfig("narrowNudgeGate3", 2198, 55);
            AddFixedSliceConfig("narrowNudgeGate4", 2198, 45);

            // these two are the fast and slow halves of the 1.1 sigma central gate.
            AddFixedSliceConfig("fastFixed", 2110, 45);
            AddFixedSliceConfig("slowFixed", 2201, 45);
            // two fairly wide gates that take in most of the slow and fast molecules.
            // They've been chosed to try and capture the wiggliness of our fast-slow
            // wiggles.
            AddFixedSliceConfig("widefastFixed", 1950, 150);
            AddFixedSliceConfig("wideslowFixed", 2330, 150);
            // A narrow centre gate for correlation analysis
            AddFixedSliceConfig("cgateNarrowFixed", 2175, 25);
            // A gate containing no molecules to look for edms caused by rf pickup
            AddFixedSliceConfig("preMolecularBackground", 1900, 50);
            // A demodulation config for Kr
            AddFixedSliceConfig("centreFixedKr", 2950, 90);


        }

        //private static void AddSliceConfig(string name, double offset, double width)
        //{
        //    // the slow half of the fwhm
        //    DemodulationConfigBuilder dcb = delegate(Block b)
        //    {
        //        DemodulationConfig dc;
        //        GatedDetectorExtractSpec dg0, dg1, dg2, dg3, dg4;

        //        dc = new DemodulationConfig();
        //        dc.AnalysisTag = name;
        //        dg0 = GatedDetectorExtractSpec.MakeGateFWHM(b, 0, offset, width);
        //        dg0.Name = "top";
        //        dg0.BackgroundSubtract = true;
        //        dg1 = GatedDetectorExtractSpec.MakeGateFWHM(b, 1, offset, width);
        //        dg1.Name = "norm";
        //        dg1.BackgroundSubtract = true;
        //        dg2 = GatedDetectorExtractSpec.MakeWideGate(2);
        //        dg2.Name = "mag1";
        //        dg2.Integrate = false;
        //        dg3 = GatedDetectorExtractSpec.MakeWideGate(3);
        //        dg3.Name = "short";
        //        dg3.Integrate = false;
        //        dg4 = GatedDetectorExtractSpec.MakeWideGate(4);
        //        dg4.Name = "battery";

        //        dc.GatedDetectorExtractSpecs.Add(dg0.Name, dg0);
        //        dc.GatedDetectorExtractSpecs.Add(dg1.Name, dg1);
        //        dc.GatedDetectorExtractSpecs.Add(dg2.Name, dg2);
        //        dc.GatedDetectorExtractSpecs.Add(dg3.Name, dg3);
        //        dc.GatedDetectorExtractSpecs.Add(dg4.Name, dg4);

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

        private static void AddFixedSliceConfig(string name, double centre, double width)
        {
            // the slow half of the fwhm
            DemodulationConfigBuilder dcb = delegate(Block b)
            {
                DemodulationConfig dc;
                GatedDetectorExtractSpec dg0, dg1, dg2, dg3, dg4, dg5, dg6, dg7;

                //This dodgy bit of code is to make sure that the reflected rf power meters
                // only select a single point in the centre of the rf pulse. It won't work
                // if either of the rf pulses is centred at a time not divisible w/o rem. by 10us

                int rf1CT = (int)b.Config.Settings["rf1CentreTime"];
                int rf2CT = (int)b.Config.Settings["rf2CentreTime"];

                dc = new DemodulationConfig();
                dc.AnalysisTag = name;
                dg0 = new GatedDetectorExtractSpec();
                dg0.Index = 0;
                dg0.Name = "top";
                dg0.BackgroundSubtract = false;
                dg0.GateLow = (int)(centre - width);
                dg0.GateHigh = (int)(centre + width);
                dg1 = new GatedDetectorExtractSpec();
                dg1.Index = 1;
                dg1.Name = "norm";
                dg1.BackgroundSubtract = false;
                dg1.GateLow = (int)((centre - width) / kDetectorDistanceRatio);
                dg1.GateHigh = (int)((centre + width) / kDetectorDistanceRatio);
                dg2 = GatedDetectorExtractSpec.MakeWideGate(2);
                dg2.Name = "magnetometer";
                dg2.Integrate = false;
                dg3 = GatedDetectorExtractSpec.MakeWideGate(3);
                dg3.Name = "gnd";
                dg3.Integrate = false;
                dg4 = GatedDetectorExtractSpec.MakeWideGate(4);
                dg4.Name = "battery";
                dg4.Integrate = false; //Add this in to analyse By in 3 axis internal magnetometer tests
                dg5 = GatedDetectorExtractSpec.MakeWideGate(5);
                dg5.Name = "rfCurrent";
                dg5.Integrate = false;
                dg6 = new GatedDetectorExtractSpec();
                dg6.Index = 6;
                dg6.Name = "reflectedrf1Amplitude";
                dg6.BackgroundSubtract = false;
                dg6.GateLow = (rf1CT/10) -1;
                dg6.GateHigh = (rf1CT / 10) + 1;
                dg7 = new GatedDetectorExtractSpec();
                dg7.Index = 7 ;
                dg7.Name = "reflectedrf2Amplitude";
                dg7.BackgroundSubtract = false;
                dg7.GateLow = (rf2CT / 10) - 1;
                dg7.GateHigh = (rf2CT / 10) + 1;


                dc.GatedDetectorExtractSpecs.Add(dg0.Name, dg0);
                dc.GatedDetectorExtractSpecs.Add(dg1.Name, dg1);
                dc.GatedDetectorExtractSpecs.Add(dg2.Name, dg2);
                dc.GatedDetectorExtractSpecs.Add(dg3.Name, dg3);
                dc.GatedDetectorExtractSpecs.Add(dg4.Name, dg4);
                dc.GatedDetectorExtractSpecs.Add(dg5.Name, dg5);
                dc.GatedDetectorExtractSpecs.Add(dg6.Name, dg6);
                dc.GatedDetectorExtractSpecs.Add(dg7.Name, dg7);

                dc.PointDetectorChannels.Add("MiniFlux1");
                dc.PointDetectorChannels.Add("MiniFlux2");
                dc.PointDetectorChannels.Add("MiniFlux3");
                dc.PointDetectorChannels.Add("NorthCurrent");
                dc.PointDetectorChannels.Add("SouthCurrent");
                dc.PointDetectorChannels.Add("PumpPD");
                dc.PointDetectorChannels.Add("ProbePD");
                dc.PointDetectorChannels.Add("PhaseLockFrequency");

                return dc;
            };
            standardConfigs.Add(name, dcb);
        }


     }


     public delegate DemodulationConfig DemodulationConfigBuilder(Block b);
}
