using System;
using System.Collections.Generic;
using System.Text;

namespace DAQ.HAL
{
    public class PolynomialCalibration : Calibration
    {
        // y = coefficients[0] + coefficients[1] x + coefficients[2] x^2 etc.
        double[] coefficients;
        public PolynomialCalibration(double[] coefficients, double rangeLow, double rangeHigh)
        {
            this.coefficients = coefficients;
            this.rangeLow = rangeLow;
            this.rangeHigh = rangeHigh;
        }
        public PolynomialCalibration(double[] coefficients)
        {
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
            return output;
        }
    }
}
