using System;
using System.Collections.Generic;
using System.Text;

using Data.EDM;

namespace Analysis.EDM
{
    public class GatedDetectorData : DetectorData
    {
        public GatedDetectorExtractSpec Gate;

        public double SubtractedBackground = 0.0;

        private delegate double[] GatedDetectorExtractFunction(int index, double startTime, double endTime);

        public static GatedDetectorData ExtractFromBlock(Block b, GatedDetectorExtractSpec gate)
        {
            GatedDetectorData gd = new GatedDetectorData();
            GatedDetectorExtractFunction f;
            if (gate.Integrate) f = new GatedDetectorExtractFunction(b.GetTOFIntegralArray);
            else f = new GatedDetectorExtractFunction(b.GetTOFMeanArray);
            double[] rawData = f(gate.Index, gate.GateLow, gate.GateHigh);
            if (gate.BackgroundSubtract)
            {
                TOFFitResults results = (new TOFFitter()).FitTOF(b.GetAverageTOF(gate.Index));
                double bg = results.Background * (gate.GateHigh - gate.GateLow);
                double[] bgSubData = new double[rawData.Length];
                for (int i = 0; i < rawData.Length; i++) bgSubData[i] = rawData[i] - bg;
                gd.PointValues.AddRange(bgSubData);
                gd.SubtractedBackground = bg;
            }
            else
            {
                gd.PointValues.AddRange(rawData);
            }
            gd.Gate = gate;
            return gd;
        }

        // this is a funny kind of division! The mean of d1 is left unchanged, but d1 is
        // divided through point by point by d2. It's probably an abuse of operator overloading.
        public static GatedDetectorData operator /(GatedDetectorData d1, GatedDetectorData d2)
        {
            GatedDetectorData d3 = new GatedDetectorData();
            d3.Gate = d1.Gate;
            d3.SubtractedBackground = d1.SubtractedBackground;
            double d2Mean = 0;
            for (int i = 0; i < d2.PointValues.Count; i++) d2Mean += d2.PointValues[i];
            d2Mean /= d2.PointValues.Count;
            for (int i = 0; i < d1.PointValues.Count; i++)
                d3.PointValues.Add((d2Mean * d1.PointValues[i]) / d2.PointValues[i]);
            return d3;
        }
    }
}
