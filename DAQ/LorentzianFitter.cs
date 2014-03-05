using System;


namespace DAQ.Analyze
{
    /// <summary>
    /// A class to fit Lorentzians. Note that the standard normalised definition of
    /// the Lorentzian is not used, rather one that has a peak height of 1 is used.
    /// This makes the estimated amplitude parameter more meaningful (i.e. it's the
    /// height of the peak). The w parameter that is returned is the FWHM.
    /// </summary>
    public class LorentzianFitter : DAQ.Analyze.PeakFitter
    {

        public LorentzianFitter()
        {
            Name = "Lorentzian";
            model = lorentzian;
        }

        // takes the parameters (in this order, in the double[])
        // N: background
        // Q: signal
        // c: centre
        // w: width
        private void lorentzian(double[] parameters, double[] x, ref double result, object obj)
        {
            double n = parameters[0];
            double q = parameters[1];
            double c = parameters[2];
            double w = parameters[3];
            if (w == 0) w = 0.001; // watch out for divide by zero
            result = n + q * (1 / (1 + (((x[0] - c) * (x[0] - c)) / ((w / 2) * (w / 2)))));
        }

        public override string ParameterReport
        {
            get
            {
                return "n: " + lastFittedParameters[0].ToString("G3") +
                    " q: " + lastFittedParameters[1].ToString("G3") +
                    " c: " + lastFittedParameters[2].ToString("G8") +
                    " w: " + lastFittedParameters[3].ToString("G3");
            }
        }

        public double[] Parameters
        {
            get
            {
                return lastFittedParameters;
            }
        }

    }
}
