using System;
using NationalInstruments.Analysis.Math;


namespace ScanMaster.Analyze
{
    /// <summary>
	/// A class to fit Gaussians. Note that the standard normalised definition of
	/// the Gaussian is not used, rather one that has a peak height of 1 is used.
	/// This makes the estimated amplitude parameter more meaningful (i.e. it's the
	/// height of the peak). The w parameter that is returned is the FWHM. Again, this
	/// is slightly different to the usual definition of the Gaussian.
	/// </summary>
    public class GaussianFitter : ScanMaster.Analyze.PeakFitter
    {

        public GaussianFitter()
        {
            Name = "Gaussian";
            model = new ModelFunctionCallback(gaussian);
        }

        // takes the parameters (in this order, in the double[])
        // N: background
        // Q: signal
        // c: centre
        // w: width
        private double gaussian(double x, double[] parameters)
        {
            double n = parameters[0];
            double q = parameters[1];
            double c = parameters[2];
            double w = parameters[3];
            if (w == 0) w = 0.001; // watch out for divide by zero
            return n + q * Math.Exp( - Math.Pow(x - c, 2) / ((2 / 5.52) * Math.Pow(w, 2)));
        }

        public override string ParameterReport
        {
            get
            {
                return "n: " + lastFittedParameters[0].ToString("G3") +
                    " q: " + lastFittedParameters[1].ToString("G3") +
                    " c: " + lastFittedParameters[2].ToString("G4") +
                    " w: " + lastFittedParameters[3].ToString("G3");
            }
        }
    }
}
