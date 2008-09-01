using System;
using System.Collections.Generic;
using System.Text;

using Data;
using Data.EDM;

namespace Analysis.EDM
{
    [Serializable]
    public class GatedDetectorExtractSpec
    {
        public string Name = "";
        public int Index;
        public int GateLow;
        public int GateHigh;
        public double OffsetFWHM;
        public double WidthFWHM;
        public bool Integrate = true;
        public bool BackgroundSubtract = false;
        public double Background;

        public void FitToTOF(TOF t)
        {
            TOFFitter fitter = new TOFFitter();
            TOFFitResults results = fitter.FitTOF(t);
            GateLow = (int)(results.Centre - (0.5 * WidthFWHM * results.Width) + (OffsetFWHM * results.Width));
            GateHigh = (int)(results.Centre + (0.5 * WidthFWHM * results.Width) + (OffsetFWHM * results.Width));
            Background = results.Background;
        }

        public static GatedDetectorExtractSpec MakeGateFWHM(Block b, int detector, double offset, double width)
        {
            GatedDetectorExtractSpec dg = new GatedDetectorExtractSpec();
            dg.Index = detector;
            dg.OffsetFWHM = offset;
            dg.WidthFWHM = width;
            dg.FitToTOF(b.GetAverageTOF(detector));
            return dg;
        }

        public static GatedDetectorExtractSpec MakeWideGate(int detector)
        {
            GatedDetectorExtractSpec dg = new GatedDetectorExtractSpec();
            dg.Index = detector;
            dg.GateLow = 0;
            dg.GateHigh = 100000000;
            return dg;
        }
    }
}
