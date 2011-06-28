using System;
using System.Collections.Generic;
using System.Text;

namespace DAQ.HAL
{
    public abstract class HardwareCalibration
    {
        public abstract double ConvertFromVoltage(double voltage);
        public abstract double ConvertToVoltage(double otherUnit);
    }
}
