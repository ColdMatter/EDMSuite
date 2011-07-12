using System;
using System.Collections.Generic;
using System.Text;
using NationalInstruments.Analysis.Math;

namespace DAQ.HAL
{
    public class ExponentialAndPolynomialCalibration : Calibration
    {
        // y =  a Exp[m(x-c)] + coefficients[0] + coefficients[1] x + coefficients[2] x^2 etc.
        double[] coefficients;
        double a, c, m;
        

        public ExponentialAndPolynomialCalibration(double a, double c, double m, double[] coefficients, double rangeLow, double rangeHigh)
        {
            this.a = a;
            this.c = c;
            this.m = m;
            this.coefficients = coefficients;
            this.rangeLow = rangeLow;
            this.rangeHigh = rangeHigh;
            
        }
        public ExponentialAndPolynomialCalibration(double a, double c, double m, double[] coefficients)
        {
            this.a = a;
            this.c = c;
            this.m = m;
            this.coefficients = coefficients;
            this.rangeLow = 0.0;
            this.rangeHigh = 0.0;

        }
        public override double Convert(double input)
        {
            CheckRange(input);
            double output = 0.0;
            for (int i = 0; i < coefficients.Length; i++)
            {
                output = output + coefficients[i] * (Math.Pow(input, i));
            }
            output = output + a * Math.Exp(m * (input - c));
            return output;
        }
    }
}
