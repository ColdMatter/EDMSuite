using System;
using System.Collections.Generic;
using System.Text;
using NationalInstruments.Analysis.Math;

namespace DAQ.HAL
{
    public class PolynomialCalibration : Calibration
    {
        // y = coefficients[0] + coefficients[1] x + coefficients[2] x^2 etc.
        double[] coefficients;
        public PolynomialCalibration(double[] coefficients)
        {
            this.coefficients = coefficients;
        }

        public override double Convert(double input)
        {
            double output = 0.0;
            for (int i = 0; i < coefficients.GetLength(0); i++)
            {
                output = output + coefficients[i] * (Math.Pow(input, i));
            }
            return output;
        }
    }
}
