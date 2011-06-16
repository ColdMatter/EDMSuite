using System;


namespace ScanMaster.Analyze
{
    /// <summary>
    /// A class to fit sinc-squareds. This sinc-squared has a peak height of 1.
    /// The w parameter that is returned is the FWHM.
    /// </summary>
    public class SincFitter : ScanMaster.Analyze.PeakFitter
    {

        public SincFitter()
        {
            Name = "Sinc squared";
            model = sinc;
        }

        // takes the parameters (in this order, in the double[])
        // N: background
        // Q: signal
        // c: centre
        // w: width
        private void sinc(double[] parameters, double[] x, ref double result, object obj)
        {
            double n = parameters[0];
            double q = parameters[1];
            double c = parameters[2];
            double w = parameters[3];
            if (x[0] == c) x[0] += 1e-3; // watch out for divide by zero
            double z = 2.78 * (x[0] - c) / w;
            result = n - q * Math.Pow(Math.Sin(z) / z, 2);
        }

        public override string ParameterReport
        {
            get
            {
                return "n: " + lastFittedParameters[0].ToString("G3") +
                    " q: " + lastFittedParameters[1].ToString("G3") +
                    " c: " + lastFittedParameters[2].ToString("G6") +
                    " w: " + lastFittedParameters[3].ToString("G3");
            }
        }
    }
}
