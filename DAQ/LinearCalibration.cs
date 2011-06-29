using System;
using System.Collections.Generic;
using System.Text;

namespace DAQ.HAL
{
    public class LinearCalibration : Calibration
    {
        // y = m (x-c) + b
        double m, c, b;
        public LinearCalibration(double m, double c, double b)
        {
            this.m = m;
            this.b = b;
            this.c = c;
        }
        public override double Convert(double input)
        {
            return m * (input - c) + b;
        }
    }
}
