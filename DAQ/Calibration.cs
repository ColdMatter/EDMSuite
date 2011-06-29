using System;
using System.Collections.Generic;
using System.Text;

namespace DAQ.HAL
{
    public abstract class Calibration
    {
        public abstract double Convert(double input);
    }
}
