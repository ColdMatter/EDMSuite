using System;
using System.Collections.Generic;
using System.Text;
using DAQ.Environment;
using DAQ.HAL;
using NationalInstruments.DAQmx;

namespace DAQ.HAL
{
    /// <summary>
    /// A class to represent a KJLC 903 cold cathode pressure gauge
    /// </summary>
    public class Lesker903Gauge
    {
        private Task readPressureTask;
        private AnalogSingleChannelReader pressureReader;

        private double lastPressure = 0;

        private const double VOLTAGE_LOWER_BOUND = 0;
        private const double VOLTAGE_UPPER_BOUND = 10;
        private const double GAUGE_CAL = 10.875;
        private const double MAX_PRESSURE = 1E-3;

        private double highPressureWarningLevel = MAX_PRESSURE;
        
        public Lesker903Gauge(string name, string channelName)
        {
            readPressureTask = new Task("Read pressure -" + name);
            ((AnalogInputChannel)Environs.Hardware.AnalogInputChannels[channelName]).AddToTask(readPressureTask, VOLTAGE_LOWER_BOUND, VOLTAGE_UPPER_BOUND);
            pressureReader = new AnalogSingleChannelReader(readPressureTask.Stream);
            readPressureTask.Control(TaskAction.Verify);
        }

        public double Pressure
        {
            get
            {
                readPressureTask.Start();
                double voltage = pressureReader.ReadSingleSample();
                readPressureTask.Stop();

                //convert the read voltage to a pressure
                lastPressure = Math.Pow(10, (voltage - GAUGE_CAL));

                return lastPressure;
            }
        }

        public double WarningLevel
        {
            get
            {
                return highPressureWarningLevel;
            }
            set
            {
                highPressureWarningLevel = value;
            }
        }


        public bool OverPressure
        {
            get
            {
                return (lastPressure >= highPressureWarningLevel);
            }
        }
    }
}
