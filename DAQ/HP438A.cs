using System;
using System.Collections.Generic;
using System.Text;


using DAQ.Environment;

namespace DAQ.HAL
{
    public class HP438A : RFPowerMonitor
    {
        public HP438A(String visaAddress)
            : base(visaAddress)
        { }

        private string sensor = "AP";

        public string Sensor
        {
            set
            {
                sensor = value;
            }
        }

        // returns the power in dBm.
        public override double Power
        {
            get
            {
                if (!Environs.Debug)
                {
                    Connect();
                    Write(sensor);
                    // ensures the device is in logarithmic mode.
                    Write("LG");
                    // TR2 - measurement only taken after some factory set settling time for the sensor.
                    Write("TR2");
                    string pow = Read();
                    Write("TR3");
                    Disconnect();
                    return Double.Parse(pow);
                }
                else
                {
                    return 3 + 0.05 * (new Random()).NextDouble();
                }
            }
        }

       
    }
}

