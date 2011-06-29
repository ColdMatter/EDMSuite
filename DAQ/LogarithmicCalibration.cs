using System;
using System.Collections.Generic;
using System.Text;
using NationalInstruments.Analysis.Math;

namespace DAQ.HAL
{
    public class LogarithmicCalibration : Calibration
    {
        // y = a Log_b(x) + c
        double a, b, c;
        public LogarithmicCalibration(double a, double b, double c)
        {
            this.a = a;
            this.b = b;
            this.c = c;
        }

        public override double Convert(double input)
        {
            return (a * Math.Log(input, b)) + c;
        }
    }
}
