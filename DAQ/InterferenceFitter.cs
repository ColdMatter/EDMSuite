using System;

namespace DAQ.Analyze
{
    /// <summary>
    /// A class to fit sinc-squareds. This sinc-squared has a peak height of 1.
    /// The w parameter that is returned is the FWHM.
    /// </summary>
    public class InterferenceFitter : DAQ.Analyze.Fitter
    {

        public InterferenceFitter()
        {
            Name = "Interference";
            model = sineSquared;
        }

        // takes the parameters (in this order, in the double[])
        // N: background
        // Q: signal
        // c: centre
        // w: width
        private void sineSquared(double[] parameters, double[] x, ref double result, object obj)
        {
            double n = parameters[0];
            double q = parameters[1];
            double c = parameters[2];
            double w = parameters[3];
            result = n + q * Math.Pow(Math.Cos((x[0] - c) / w), 2);
        }

        public override double[] SuggestParameters(double[] xDat, double[] yDat, double scanStart, double scanEnd)
        {
            double yMax = 0;
            double yMin = double.MaxValue;
            for (int i = 0; i < yDat.Length; i++)
            {
                if (yDat[i] > yMax) yMax = yDat[i];
                if (yDat[i] < yMin) yMin = yDat[i];
            }
            return new double[] { yMin, yMax - yMin, 2.5, 0.4 };
        }

        public override string ParameterReport
        {
            get
            {
                return "n: " + lastFittedParameters[0].ToString("G3") +
                    " q: " + lastFittedParameters[1].ToString("G3") +
                    " c: " + lastFittedParameters[2].ToString("G6") +
                    " w: " + lastFittedParameters[3].ToString("G3") +
                    " d: " + ((lastFittedParameters[2] - 2.5) * 2000).ToString("G3");
            }
        }
    }
}
