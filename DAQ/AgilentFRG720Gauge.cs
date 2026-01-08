using System;
using System.Collections.Generic;
using System.Text;
using DAQ.Environment;
using DAQ.HAL;
using NationalInstruments.DAQmx;

namespace DAQ.HAL
{
    /// <summary>
    /// A class to represent a Agilent FRG720 Pirani Bayard-Alpert pressure gauge
    /// 
    /// Quote from Agilent website:
    /// "The FRG-720 and FRG-730 combine Agilent's Pirani and Bayard-Alpert 
    /// sensor into a single compact design that provides measuring capability 
    /// from 5 x 10^-10 mbar to atmosphere (3.8 x 10^10 Torr to atmosphere). 
    /// Combining these two technologies into a single unit reduces complexity 
    /// and integration challenges while protecting the Bayard-Alpert sensor 
    /// from premature burnout."  .
    /// </summary>
    public class AgilentFRG720Gauge
    {
        private Task readPressureTask;
        private AnalogSingleChannelReader pressureReader;

        private double lastPressure = 0;


        private const double VOLTAGE_LOWER_BOUND = 0;  // volts
        private const double VOLTAGE_UPPER_BOUND = 10; // volts
        private const double GAUGE_OFFSET = 7.75;      // volts
        private const double GAUGE_FACTOR = 0.75;      // volts
        private const double GAUGE_CONSTANT = 0;       // dimensionless (mbar = 0, Pa = 2, Torr = -0.125)
        private const double MAX_PRESSURE = 1000;      // mbar

        private double highPressureWarningLevel = MAX_PRESSURE;

        public AgilentFRG720Gauge(string name, string channelName)
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
                readPressureTask.Control(TaskAction.Unreserve);

                //convert the read voltage to a pressure
                lastPressure = Math.Pow(10, ((voltage - GAUGE_OFFSET) / GAUGE_FACTOR) + GAUGE_CONSTANT);

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