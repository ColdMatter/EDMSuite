using System;
using System.Collections.Generic;
using System.Text;
using NationalInstruments.Analysis.Math;

namespace DAQ.HAL
{
    public class ExponentialCalibration : Calibration
    {
        // a Exp[m(x-c)] + b
        double a, m, b, c;
        public ExponentialCalibration(double a, double b, double c, double m)
        {
            this.a = a;
            this.b = b;
            this.c = c;
            this.m = m;
        }
        public override double Convert(double input)
        {
            return a * Math.Exp(m * (input - c)) + b;
        }
    }
}
