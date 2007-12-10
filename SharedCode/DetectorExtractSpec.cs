using System;
using System.Collections.Generic;
using System.Text;

using Data;
using Data.EDM;

namespace Analysis.EDM
{
    [Serializable]
    public class DetectorExtractSpec
    {
        public string name = "";
        public int Index;
        public int GateLow;
        public int GateHigh;
        public double OffsetFWHM;
        public double WidthFWHM;
        public bool Integrate = true;
        public bool BackgroundSubtract = false;

        public void SetGatesFromFWHM(TOF t)
        {
            TOFFitter fitter = new TOFFitter();
            TOFFitResults results = fitter.FitTOF(t);
            GateLow = (int)(results.Centre - (0.5 * WidthFWHM * results.Width) + (OffsetFWHM * results.Width));
            GateHigh = (int)(results.Centre + (0.5 * WidthFWHM * results.Width) + (OffsetFWHM * results.Width));
        }

        public static DetectorExtractSpec MakeGateFWHM(Block b, int detector, double offset, double width)
        {
            DetectorExtractSpec dg = new DetectorExtractSpec();
            dg.Index = detector;
            dg.OffsetFWHM = offset;
            dg.WidthFWHM = width;
            dg.SetGatesFromFWHM(b.GetAverageTOF(detector));
            return dg;
        }

        public static DetectorExtractSpec MakeWideGate(int detector)
        {
            DetectorExtractSpec dg = new DetectorExtractSpec();
            dg.Index = detector;
            dg.GateLow = 0;
            dg.GateHigh = 100000000;
            return dg;
        }
    }
}
