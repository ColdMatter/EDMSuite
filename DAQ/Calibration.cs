using System;
using System.Collections.Generic;
using System.Text;

namespace DAQ.HAL
{
    public abstract class Calibration
    {
        protected double rangeLow;
        protected double rangeHigh;

        public abstract double Convert(double input);
        public void CheckRange(double input)
        {
            if (rangeLow != rangeHigh)
            {
                if (input < rangeLow || input > rangeHigh)
                {
                    throw new CalibrationRangeException();
                }
                else { }
            }
        }
        public class CalibrationRangeException : ArgumentOutOfRangeException { };
    }
}
