using System;


namespace DAQ.Analyze
{
    /// <summary>
    /// A class to fit Gaussians. Note that the standard normalised definition of
    /// the Gaussian is not used, rather one that has a peak height of 1 is used.
    /// This makes the estimated amplitude parameter more meaningful (i.e. it's the
    /// height of the peak). The w parameter that is returned is the FWHM. Again, this
    /// is slightly different to the usual definition of the Gaussian.
    /// </summary>
    public class ExponentialFitter : DAQ.Analyze.exponentialPeakFitter
    {

        public ExponentialFitter()
        {
            Name = "Exponential";
            model = exponential;
        }

        // takes the parameters (in this order, in the double[])
        // N: background
        // Q: signal
        // c: centre
        // w: width
        protected void exponential(double[] parameters, double[] x, ref double func, object obj)
        {
            double bg = parameters[0];
            double amp = parameters[1];
            double tau = parameters[2];
            if (tau == 0) tau = 100; // watch out for divide by zero
            func = bg + amp * Math.Exp(-x[0]/tau );
        }

        public override string ParameterReport
        {
            get
            {
                return " bg: " + lastFittedParameters[0].ToString("G3") +
                    "\n Ampl.: " + lastFittedParameters[1].ToString("G3") +
                    "\n tau: " + lastFittedParameters[2].ToString("G3");
                    
            }
        }

    }
}