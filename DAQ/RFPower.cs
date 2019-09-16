using System;

namespace DAQ.HAL
{
    /// <summary>
    /// Represents a (GPIB) RF power monitor
    /// </summary>
    public abstract class RFPowerMonitor : GPIBInstrument
    {
        public RFPowerMonitor(String visaAddress)
            : base(visaAddress)
        { }

        public abstract double Power
        {
            get;
        }

    }
}