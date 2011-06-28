using System;
using System.Collections.Generic;
using System.Text;

namespace DAQ.HAL
{
    public class HardwareCalibrationLibrary
    {
        private Dictionary<string, HardwareCalibration> calibrations = new Dictionary<string,HardwareCalibration>();
        public Dictionary<string, HardwareCalibration> Calibrations
        {
            get { return calibrations; }
        }
    }
}
