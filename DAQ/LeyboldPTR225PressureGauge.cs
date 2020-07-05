using System;
using System.Collections.Generic;
using System.Text;
using DAQ.Environment;
using DAQ.HAL;
using NationalInstruments.DAQmx;

namespace DAQ.HAL
{
    /// <summary>
    /// A class to represent a Leybold PTR225 pressure gauge. The gauge has a range of 5E-3 to 1E-8 mbar.
    /// </summary>
    public class LeyboldPTR225PressureGauge
    {
        private Task readPressureTask;
        private AnalogSingleChannelReader pressureReader;

        private double lastPressure = 0;


        private const double VOLTAGE_LOWER_BOUND = 0;  // volts
        private const double VOLTAGE_UPPER_BOUND = 10; // volts
        private const double GAUGE_OFFSET = 12.66;      // volts
        private const double GAUGE_FACTOR = 1.33;      // volts
        private const double MAX_PRESSURE = 0.005;      // mbar

        private double highPressureWarningLevel = MAX_PRESSURE;

        public LeyboldPTR225PressureGauge(string name, string channelName)
        {
            if (!Environs.Debug)
            {
                readPressureTask = new Task("Read pressure -" + name);
                ((AnalogInputChannel)Environs.Hardware.AnalogInputChannels[channelName]).AddToTask(readPressureTask, VOLTAGE_LOWER_BOUND, VOLTAGE_UPPER_BOUND);
                pressureReader = new AnalogSingleChannelReader(readPressureTask.Stream);
                readPressureTask.Control(TaskAction.Verify);
            }
        }

        public double Pressure
        {
            get
            {
                readPressureTask.Start();
                double voltage = pressureReader.ReadSingleSample();
                readPressureTask.Stop();

                //convert the read voltage to a pressure
                lastPressure = Math.Pow(10, ((voltage - GAUGE_OFFSET) / GAUGE_FACTOR));

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