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
    public class DemodulationConfigNewScheme
    {
        public Dictionary<string, GatedDetectorExtractSpec> GatedDetectorExtractSpecs =
            new Dictionary<string, GatedDetectorExtractSpec>();
        public List<string> PointDetectorChannels = new List<string>();
        public String AnalysisTag = "";

        // static members for making standard configs
        private static Dictionary<string, DemodulationConfigBuilderNewScheme> standardConfigs =
            new Dictionary<string, DemodulationConfigBuilderNewScheme>();

        // ratio of distance from source for the two detectors
        private static double kDetectorDistanceRatio = 1715.0/1500.0;

        public static DemodulationConfigNewScheme GetStandardDemodulationConfig(string name, Block b)
        {
            return (standardConfigs[name])(b);
        }

        static DemodulationConfigNewScheme()
        {
            // here we stock the class' static library of configs up with some standard configs

            // a wide gate - integrate everything
            DemodulationConfigBuilderNewScheme wide = delegate(Block b)
            {

                DemodulationConfigNewScheme dc;
                GatedDetectorExtractSpec dg0, dg1, dg2, dg3, dg4, dg5, dg6, dg7, dg8, dg9, dg10, dg11;

                dc = new DemodulationConfigNewScheme();
                dc.AnalysisTag = "wide";
                dg0 = GatedDetectorExtractSpec.MakeWideGate(0);
                dg0.Name = "bottomProbe";
                dg0.GateLow = 2100;
                dg0.GateHigh = 3400;
                dg1 = GatedDetectorExtractSpec.MakeWideGate(1);
                dg1.Name = "topProbe";
                dg1.GateLow = 2100;
                dg1.GateHigh = 3400;
                dg2 = GatedDetectorExtractSpec.MakeWideGate(2);
                dg2.Name = "magnetometer";
                dg2.Integrate = false;
                dg3 = GatedDetectorExtractSpec.MakeWideGate(3);
                dg3.Name = "gnd";
                dg3.Integrate = false;
                dg4 = GatedDetectorExtractSpec.MakeWideGate(4);
                dg4.Name = "battery";
                dg4.GateLow = 2100;
                dg4.GateHigh = 3400;
                dg5 = GatedDetectorExtractSpec.MakeWideGate(5);
                dg5.Name = "rfCurrent";
                dg5.Integrate = false;
                dg6 = GatedDetectorExtractSpec.MakeWideGate(6);
                dg6.Name = "reflectedrf1Amplitude";
                dg6.GateLow = 2100;
                dg6.GateHigh = 3400;
                dg7 = GatedDetectorExtractSpec.MakeWideGate(7);
                dg7.Name = "reflectedrf2Amplitude";
                dg7.GateLow = 2100;
                dg7.GateHigh = 3400;
                dg8 = GatedDetectorExtractSpec.MakeWideGate(8);
                dg8.Name = "bottomProbeNoBackground";
                dg8.GateLow = 2100;
                dg8.GateHigh = 3400;
                dg9 = GatedDetectorExtractSpec.MakeWideGate(9);
                dg9.Name = "topProbeNoBackground";
                dg9.GateLow = 2100;
                dg9.GateHigh = 3400;
                dg10 = GatedDetectorExtractSpec.MakeWideGate(10);
                dg10.Name = "bottomProbeScaled";
                dg10.GateLow = 2100;
                dg10.GateHigh = 3400;
                dg11 = GatedDetectorExtractSpec.MakeWideGate(11);
                dg11.Name = "asymmetry";
                dg11.GateLow = 2100;
                dg11.GateHigh = 3400;

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

            // for time-slicing the data up into 10us time bins
            for (int i = 0; i < 20; i++) AddFixedSliceConfig("slice" + i, 2700 + i * 10, 10);


            //// for testing out different centred-gate widths
            //for (int i = 1; i < 10; i++)
            //    AddFixedSliceConfig("wgate" + i, 2190, i * 20);

            // "background" gate
            DemodulationConfigBuilderNewScheme background = delegate(Block b)
            {

                DemodulationConfigNewScheme dc;
                GatedDetectorExtractSpec dg0, dg1;

                dc = new DemodulationConfigNewScheme();
                dc.AnalysisTag = "background";
                dg0 = GatedDetectorExtractSpec.MakeWideGate(0);
                dg0.GateLow = 2800;
                dg0.GateHigh = 2900;
                dg0.Name = "bottomProbe";
                dg0.Integrate = false;
                dg1 = GatedDetectorExtractSpec.MakeWideGate(1);
                dg1.Name = "topProbe";
                dg1.GateLow = 3200;
                dg1.GateHigh = 3300;
                dg0.Integrate = false;

                dc.GatedDetectorExtractSpecs.Add(dg0.Name, dg0);
                dc.GatedDetectorExtractSpecs.Add(dg1.Name, dg1);


                return dc;
            };
            standardConfigs.Add("background", background);

            //// add some fixed gate slices - the first three are the 1.1 sigma centre portion and two
            //// non-overlapping portions either side.
            //AddFixedSliceConfig("cgate11Fixed", 2156, 90); // This is normally ("cgate11Fixed", 2156, 90)
            //AddFixedSliceConfig("vfastFixed", 2025, 41);
            //AddFixedSliceConfig("vslowFixed", 2286, 41);
            //// these "nudge" gates are chosen to, hopefully, tweak the 09_10 dataset so that the
            //// RF1F channel, DB-normed in the non-linear way, is reduced to near zero
            //AddFixedSliceConfig("nudgeGate1", 2161, 90);
            //AddFixedSliceConfig("nudgeGate2", 2169, 90);
            //AddFixedSliceConfig("nudgeGate3", 2176, 90);
            //AddFixedSliceConfig("nudgeGate4", 2174, 90);
            //AddFixedSliceConfig("nudgeGate5", 2188, 90);
            //AddFixedSliceConfig("nudgeGate6", 2198, 90);
            //AddFixedSliceConfig("nudgeGate7", 2208, 90);
            //AddFixedSliceConfig("nudgeGate8", 2228, 90);
            //AddFixedSliceConfig("wideNudgeGate1", 2198, 100);
            //AddFixedSliceConfig("narrowNudgeGate1", 2198, 75);
            //AddFixedSliceConfig("narrowNudgeGate2", 2198, 65);
            //AddFixedSliceConfig("narrowNudgeGate3", 2198, 55);
            //AddFixedSliceConfig("narrowNudgeGate4", 2198, 45);

            //// these two are the fast and slow halves of the 1.1 sigma central gate.
            //AddFixedSliceConfig("fastFixed", 2110, 45);
            //AddFixedSliceConfig("slowFixed", 2201, 45);
            //// two fairly wide gates that take in most of the slow and fast molecules.
            //// They've been chosed to try and capture the wiggliness of our fast-slow
            //// wiggles.
            //AddFixedSliceConfig("widefastFixed", 1950, 150);
            //AddFixedSliceConfig("wideslowFixed", 2330, 150);
            //// A narrow centre gate for correlation analysis
            //AddFixedSliceConfig("cgateNarrowFixed", 2175, 25);
            //// A gate containing no molecules to look for edms caused by rf pickup
            //AddFixedSliceConfig("preMolecularBackground", 1900, 50);
            //// A demodulation config for Kr
            //AddFixedSliceConfig("centreFixedKr", 2950, 90);


        }

        private static void AddFixedSliceConfig(string name, double centre, double width)
        {
            // the slow half of the fwhm
            DemodulationConfigBuilderNewScheme dcb = delegate(Block b)
            {
                DemodulationConfigNewScheme dc;
                GatedDetectorExtractSpec dg0, dg1, dg2, dg3, dg4, dg5, dg6, dg7, dg8;

                dc = new DemodulationConfigNewScheme();
                dc.AnalysisTag = name;
                dg0 = new GatedDetectorExtractSpec();
                dg0.Index = 0;
                dg0.Name = "bottom";
                dg0.BackgroundSubtract = false;
                dg0.GateLow = (int)((centre - width) * kDetectorDistanceRatio);
                dg0.GateHigh = (int)((centre + width) * kDetectorDistanceRatio);
                dg1 = new GatedDetectorExtractSpec();
                dg1.Index = 1;
                dg1.Name = "top";
                dg1.BackgroundSubtract = false;
                dg1.GateLow = (int)(centre - width);
                dg1.GateHigh = (int)(centre + width);
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
                dg6 = new GatedDetectorExtractSpec();
                dg6.Index = 6;
                dg6.Name = "reflectedrf1Amplitude";
                dg6.BackgroundSubtract = false;
                dg6.GateLow = 819;
                dg6.GateHigh = 821;
                dg7 = new GatedDetectorExtractSpec();
                dg7.Index = 7;
                dg7.Name = "reflectedrf2Amplitude";
                dg7.BackgroundSubtract = false;
                dg7.GateLow = 1799;
                dg7.GateHigh = 1801;
                dg8 = new GatedDetectorExtractSpec();
                dg8.Index = 8;
                dg8.Name = "asymmetry";
                dg8.BackgroundSubtract = false;
                dg8.GateLow = (int)(centre - width);
                dg8.GateHigh = (int)(centre + width);


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
            standardConfigs.Add(name, dcb);
        }


    }


    public delegate DemodulationConfigNewScheme DemodulationConfigBuilderNewScheme(Block b);
}