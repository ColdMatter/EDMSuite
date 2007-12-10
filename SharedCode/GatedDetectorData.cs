using System;
using System.Collections.Generic;
using System.Text;

using Data.EDM;

namespace Analysis.EDM
{
    public class GatedDetectorData
    {
        public List<double> PointValues = new List<double>();

        public DetectorExtractSpec Gate;

        public double SubtractedBackground = 0.0;


        private delegate double[] DetectorExtractFunction(int index, double startTime, double endTime);

        public static GatedDetectorData ExtractFromBlock(Block b, DetectorExtractSpec gate)
        {
            GatedDetectorData gd = new GatedDetectorData();
            DetectorExtractFunction f;
            if (gate.Integrate) f = new DetectorExtractFunction(b.GetTOFIntegralArray);
            else f = new DetectorExtractFunction(b.GetTOFMeanArray);
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
    }
}
