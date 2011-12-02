using System;
using System.Collections.Generic;
using System.Text;
using NationalInstruments.Analysis.Math;

namespace DAQ.HAL
{
    public class PowerCalibration : Calibration
    {
        // a * bs^[m(x-c)] + b
        double a, m, b, c, bs;
        public PowerCalibration(double a, double b, double c, double m, double bs)
        {
            this.a = a;
            this.b = b;
            this.c = c;
            this.m = m;
            this.bs = bs;
            this.rangeLow = 0.0;
            this.rangeHigh = 0.0;
        }
        public PowerCalibration(double a, double b, double c, double m, double bs, double rangeLow, double rangeHigh)
        {
            this.a = a;
            this.b = b;
            this.c = c;
            this.m = m;
            this.bs = bs;
            this.rangeLow = rangeLow;
            this.rangeHigh = rangeHigh;
        }
        public override double Convert(double input)
        {
            CheckRange(input);
            
            return a * Math.Pow(bs, m * (input - c)) + b;
        }
    }
}
