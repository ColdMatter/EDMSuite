using System;
using System.Collections.Generic;
using System.Text;

namespace DAQ.HAL
{
    public class LogarithmicCalibration : Calibration
    {
        // y = a Log_b(x) + c
        double a, b, c;
        public LogarithmicCalibration(double a, double b, double c, double rangeLow, double rangeHigh)
        {
            this.a = a;
            this.b = b;
            this.c = c;
            this.rangeLow = rangeLow;
            this.rangeHigh = rangeHigh;
        }
        public LogarithmicCalibration(double a, double b, double c)
        {
            this.a = a;
            this.b = b;
            this.c = c;
            this.rangeLow = 0.0;
            this.rangeHigh = 0.0;
        }

        public override double Convert(double input)
        {
            CheckRange(input);
            return (a * Math.Log(input, b)) + c;
        }
    }
}
