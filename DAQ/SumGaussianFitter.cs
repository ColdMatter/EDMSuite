using System;


namespace DAQ.Analyze
{
   public class SumGaussianFitter:DAQ.Analyze.PeakFitter
    {
       


         public SumGaussianFitter()
        {
            Name = "SumGaussian";
            model = sumGaussian;
        }

        // takes the parameters (in this order, in the double[])
        // N1: background
        // Q1: signal
        // c1: centre
        // w1: width
        // N2: background
        // Q2: signal
        // c2: centre
        // w2: width

        protected void sumGaussian(double[] parameters, double[] x, ref double func, object obj)
        {
            double n1 = parameters[0];
            double q1 = parameters[1];
            double c1 = parameters[2];
            double w1 = parameters[3];
            double n2 = parameters[4];
            double q2 = parameters[5];
            double c2 = parameters[6];
            double w2 = parameters[7];
            if (w1 == 0) w1 = 0.001; // watch out for divide by zero
            if (w2 == 0) w2 = 0.001;
            func = n1 + q1 * Math.Exp(-Math.Pow(x[0] - c1, 2) / ((2 / 5.52) * Math.Pow(w1, 2))) + n2 + q2 * Math.Exp(-Math.Pow(x[0] - c2, 2) / ((2 / 5.52) * Math.Pow(w2, 2)));
        }

        public override string ParameterReport
        {
            get
            {
                return "n1: " + lastFittedParameters[0].ToString("G3") +
                    " q1: " + lastFittedParameters[1].ToString("G3") +
                    " c1: " + lastFittedParameters[2].ToString("G6") +
                    " w1: " + lastFittedParameters[3].ToString("G3")+
                    " n2: " + lastFittedParameters[4].ToString("G3") +
                    " q2: " + lastFittedParameters[5].ToString("G3") +
                    " c2: " + lastFittedParameters[6].ToString("G6") +
                    " w2: " + lastFittedParameters[7].ToString("G3");
            }
        }
    }
}

  
