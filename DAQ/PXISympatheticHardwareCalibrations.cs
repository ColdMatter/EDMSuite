using System;
using System.Collections.Generic;
using System.Text;

namespace DAQ.HAL
{
    public class PXISympatheticHardwareCalibrations : HardwareCalibrationLibrary
    {
        public PXISympatheticHardwareCalibrations()
        {
            HardwareCalibration c1p = new chamber1PressureCalibration();
            Calibrations.Add("chamber1Pressure", c1p);
        }
    }

    public class chamber1PressureCalibration : HardwareCalibration
    {
        public override double ConvertFromVoltage(double voltage)
        {
            return 10000 * voltage;
        }

        public override double ConvertToVoltage(double otherUnit)
        {
            return otherUnit / 10000;
        }
    }
}
