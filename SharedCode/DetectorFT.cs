using System;
using System.Collections.Generic;
using System.Text;

//using NationalInstruments;
//using NationalInstruments.Analysis.Dsp;

namespace Analysis.EDM
{
    // Note that this has been temporarily disabled to remove the dependency of SharedCode on the
    // NI libraries.
    [Serializable]
    public class DetectorFT
    {
        public DetectorFT(double[] fft)
        {
            this.FFT = fft;
        }

        public double[] FFT;

        public static DetectorFT MakeFT(GatedDetectorData data, int average)
        {
            //double[] dataList = new double[data.PointValues.Count];
            //data.PointValues.CopyTo(dataList);
            //ComplexDouble[] fft = Transforms.RealFft(dataList);
            //// extract the magnitude and reduce by averaging
            //int reducedFourierLength = dataList.Length / average;
            //double[] reducedFourier = new double[reducedFourierLength];
            //double rootLength = Math.Sqrt(dataList.Length); // makes it agree with Mathematica
            //for (int i = 0; i < reducedFourierLength; i++)
            //{
            //    double ptVal = 0.0;
            //    for (int j = 0; j < average; j++) ptVal += fft[(i * average) + j].Magnitude;
            //    ptVal /= average;
            //    reducedFourier[i] = ptVal / rootLength;
            //}
            //return new DetectorFT(reducedFourier);

            // TEMP: return a blank FT
            int reducedFourierLength = data.PointValues.Count / average;
            double[] reducedFourier = new double[reducedFourierLength];
            return new DetectorFT(reducedFourier);
  
        }
    }
}
