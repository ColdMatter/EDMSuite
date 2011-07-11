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
            this.rangeLow = 0.0;
            this.rangeHigh = 0.0;
        }
        public ExponentialCalibration(double a, double b, double c, double m, double rangeLow, double rangeHigh)
        {
            this.a = a;
            this.b = b;
            this.c = c;
            this.m = m;
            this.rangeLow = rangeLow;
            this.rangeHigh = rangeHigh;
        }
        public override double Convert(double input)
        {
            CheckRange(input);
            return a * Math.Exp(m * (input - c)) + b;
        }
    }
}
