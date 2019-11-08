using System;
using System.Collections.Generic;
using System.Text;

namespace DAQ.HAL
{
    public class LinearCalibration : Calibration
    {
        // y = m (x-c) + b
        double m, c, b;
        public LinearCalibration(double m, double c, double b, double rangeLow, double rangeHigh)
        {
            this.m = m;
            this.b = b;
            this.c = c;
            this.rangeLow = rangeLow;
            this.rangeHigh = rangeHigh;
        }
        public LinearCalibration(double m, double c, double b)
        {
            this.m = m;
            this.b = b;
            this.c = c;
            this.rangeLow = 0.0;
            this.rangeHigh = 0.0;
        }
        public override double Convert(double input)
        {
            CheckRange(input);
            return m * (input - c) + b;
        }
    }
}
