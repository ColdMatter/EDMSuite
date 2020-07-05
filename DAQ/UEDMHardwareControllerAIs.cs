using System;
using System.Collections.Generic;
using System.Text;
using DAQ.Environment;
using DAQ.HAL;
using NationalInstruments.DAQmx;


namespace DAQ.HAL
{
    public class UEDMHardwareControllerAIs
    {
        private Task readAI11;
        private AnalogSingleChannelReader AI11Reader;
        private Task readAI12;
        private AnalogSingleChannelReader AI12Reader;
        private Task readAI13;
        private AnalogSingleChannelReader AI13Reader;
        private Task readAI14;
        private AnalogSingleChannelReader AI14Reader;
        private Task readAI15;
        private AnalogSingleChannelReader AI15Reader;

        private const int NumberOfAverages = 5;
        private const int NumberOfChannels = 5; // Number of AI being measured

        private const double VOLTAGE_LOWER_BOUND = 0;  // volts
        private const double VOLTAGE_UPPER_BOUND = 1.65; // volts

        public UEDMHardwareControllerAIs()
        {

        }

        public UEDMHardwareControllerAIs(string[] name, string[] channelName)
        {
            if (!Environs.Debug)
            {
                readAI11 = new Task("Read AI11 -" + name[0]);
                ((AnalogInputChannel)Environs.Hardware.AnalogInputChannels[channelName[0]]).AddToTask(readAI11, VOLTAGE_LOWER_BOUND, VOLTAGE_UPPER_BOUND);
                AI11Reader = new AnalogSingleChannelReader(readAI11.Stream);
                readAI11.Control(TaskAction.Verify);

                readAI12 = new Task("Read AI12 -" + name[1]);
                ((AnalogInputChannel)Environs.Hardware.AnalogInputChannels[channelName[1]]).AddToTask(readAI12, VOLTAGE_LOWER_BOUND, VOLTAGE_UPPER_BOUND);
                AI12Reader = new AnalogSingleChannelReader(readAI12.Stream);
                readAI12.Control(TaskAction.Verify);

                readAI13 = new Task("Read AI13 -" + name[2]);
                ((AnalogInputChannel)Environs.Hardware.AnalogInputChannels[channelName[2]]).AddToTask(readAI13, VOLTAGE_LOWER_BOUND, VOLTAGE_UPPER_BOUND);
                AI13Reader = new AnalogSingleChannelReader(readAI13.Stream);
                readAI13.Control(TaskAction.Verify);

                readAI14 = new Task("Read AI14 -" + name[3]);
                ((AnalogInputChannel)Environs.Hardware.AnalogInputChannels[channelName[3]]).AddToTask(readAI14, VOLTAGE_LOWER_BOUND, VOLTAGE_UPPER_BOUND);
                AI14Reader = new AnalogSingleChannelReader(readAI14.Stream);
                readAI14.Control(TaskAction.Verify);

                readAI15 = new Task("Read AI15 -" + name[4]);
                ((AnalogInputChannel)Environs.Hardware.AnalogInputChannels[channelName[4]]).AddToTask(readAI15, VOLTAGE_LOWER_BOUND, VOLTAGE_UPPER_BOUND);
                AI15Reader = new AnalogSingleChannelReader(readAI15.Stream);
                readAI15.Control(TaskAction.Verify);
            }
        }

        
        public double[] AIVoltages()
        {
            double[,] rawVoltages = new double[NumberOfChannels, NumberOfAverages];
            for (int n = 0; n < NumberOfAverages; n++)
            {
                readAI11.Start();
                double voltageAI11 = AI11Reader.ReadSingleSample();
                readAI11.Stop();

                readAI12.Start();
                double voltageAI12 = AI12Reader.ReadSingleSample();
                readAI12.Stop();

                readAI13.Start();
                double voltageAI13 = AI13Reader.ReadSingleSample();
                readAI13.Stop();

                readAI14.Start();
                double voltageAI14 = AI14Reader.ReadSingleSample();
                readAI14.Stop();

                readAI15.Start();
                double voltageAI15 = AI15Reader.ReadSingleSample();
                readAI15.Stop();

                double[] Voltages = { voltageAI11, voltageAI12, voltageAI13, voltageAI14, voltageAI15 };

                for (int ii = 0; ii < 3; ii++)
                {
                    rawVoltages[ii, n] = Voltages[ii];
                }
            }

            double voltageAI11Sum = 0;
            double voltageAI12Sum = 0;
            double voltageAI13Sum = 0;
            double voltageAI14Sum = 0;
            double voltageAI15Sum = 0;

            for (int n = 0; n < NumberOfAverages; n++)
            {
                voltageAI11Sum += rawVoltages[0, n];
                voltageAI12Sum += rawVoltages[1, n];
                voltageAI13Sum += rawVoltages[2, n];
                voltageAI14Sum += rawVoltages[3, n];
                voltageAI15Sum += rawVoltages[4, n];
            }

            double averageVoltageAI11 = voltageAI11Sum / NumberOfAverages;
            double averageVoltageAI12 = voltageAI12Sum / NumberOfAverages;
            double averageVoltageAI13 = voltageAI13Sum / NumberOfAverages;
            double averageVoltageAI14 = voltageAI14Sum / NumberOfAverages;
            double averageVoltageAI15 = voltageAI15Sum / NumberOfAverages;

            double[] averageVoltages = { averageVoltageAI11, averageVoltageAI12, averageVoltageAI13, averageVoltageAI14, averageVoltageAI15 };

            return averageVoltages;
        }
    }
}
