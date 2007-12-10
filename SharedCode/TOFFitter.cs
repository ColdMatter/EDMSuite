using System;
using System.Collections.Generic;
using System.Text;

using NationalInstruments.Analysis.Math;

using Data;

namespace Analysis
{
    public class TOFFitter
    {
        public TOFFitResults FitTOF(TOF t)
        {   
            TOFFitResults results = new TOFFitResults();
            double[] coefficients = new double[4];
            // guess some reasonable values
            coefficients[0] = t.Data[0];
            coefficients[1] = ArrayOperation.GetMax(t.Data) - coefficients[0];
            double lastT = t.Times[t.Times.Length - 1];
            coefficients[2] = (t.Times[0] + lastT) / 2;
            coefficients[3] = (lastT - t.Times[0]) / 5;
            double mse = 0;

            // this is totally tedious!
            double[] times = new double[t.Times.Length];
            for (int i = 0 ; i < t.Times.Length ; i++) times[i] = (double)t.Times[i];

            CurveFit.NonLinearFit(times, t.Data, new ModelFunctionCallback(gaussian), coefficients, out mse, 1000000);
            results.Background = coefficients[0];
            results.Amplitude = coefficients[1];
            results.Centre = coefficients[2];
            results.Width = Math.Abs(coefficients[3]);

            return results;
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
            return n + q * Math.Exp(- ( 5.54518 * Math.Pow(x - c, 2)) / (2 * Math.Pow(w, 2)));
        }
    }

    public class TOFFitResults
    {
        public double Centre;
        public double Width;
        public double Amplitude;
        public double Background;
    }
}
