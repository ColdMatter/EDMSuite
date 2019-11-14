using System;
using System.Collections.Generic;
using System.Text;
using DAQ.Environment;
using DAQ.HAL;
using NationalInstruments.DAQmx;


namespace DAQ.HAL
{
    public class SiliconDiodeTemperatureMonitors
    {
        private Task readCellTemperatureTask;
        private AnalogSingleChannelReader cellTemperatureReader;
        private Task readS1TemperatureTask;
        private AnalogSingleChannelReader s1TemperatureReader;
        private Task readS2TemperatureTask;
        private AnalogSingleChannelReader s2TemperatureReader;
        private Task readSF6TemperatureTask;
        private AnalogSingleChannelReader sf6TemperatureReader;

        private double[] Range2to12K = { 1.294390, 1.680000, 6.429274, -7.514262, -0.725882, -1.117846, -0.562041, -0.360239, -0.229751, -0.135713, -0.068203, -0.029755 };
        private double[] Range12to24point5K = { 1.1123, 1.38373, 17.244846, -7.964373, 0.625343, -0.105068, 0.292196, -0.344492, 0.27167, -0.151722, 0.12132, -0.035566, 0.045966 };
        private double[] Range24point5to100K = { 0.909416, 1.122751, 82.017868, -59.064244, -1.356615, 1.055396, 0.837341, 0.431875, 0.440840, -0.061588, 0.209414, -0.120882, 0.055734, -0.035974 };
        private double[] Range100to500K = { 0.070000, 0.997990, 306.592351, -205.393808, -4.695680, -2.031603, -0.071792, -0.437682, 0.176352, -0.182516, 0.064687, -0.027019, 0.010019 };

        private const double VoltageAt2K = 1.634720; // Voltage across diode when temperature is 1.2 Kelvin
        private const double VoltageAt12K = 1.334990; // Voltage across diode when temperature is 12 Kelvin
        private const double VoltageAt24Point5K = 1.1226855; // Voltage across diode when temperature is 24.5 Kelvin
        private const double VoltageAt100K = 0.986974; // Voltage across diode when temperature is 100 Kelvin
        private const double VoltageAt500K = 0.090681; // Voltage across diode when temperature is 500 Kelvin

        private double[] SourceTemperatures;
        private const int NumberOfAverages = 5;

        private const double VOLTAGE_LOWER_BOUND = 0;  // volts
        private const double VOLTAGE_UPPER_BOUND = 1.65; // volts

        public SiliconDiodeTemperatureMonitors(string[] name, string[] channelName)
        {
            readCellTemperatureTask = new Task("Read cell temperature -" + name[0]);
            ((AnalogInputChannel)Environs.Hardware.AnalogInputChannels[channelName[0]]).AddToTask(readCellTemperatureTask, VOLTAGE_LOWER_BOUND, VOLTAGE_UPPER_BOUND);
            cellTemperatureReader = new AnalogSingleChannelReader(readCellTemperatureTask.Stream);
            readCellTemperatureTask.Control(TaskAction.Verify);

            readS1TemperatureTask = new Task("Read S1 temperature -" + name[1]);
            ((AnalogInputChannel)Environs.Hardware.AnalogInputChannels[channelName[1]]).AddToTask(readS1TemperatureTask, VOLTAGE_LOWER_BOUND, VOLTAGE_UPPER_BOUND);
            s1TemperatureReader = new AnalogSingleChannelReader(readS1TemperatureTask.Stream);
            readS1TemperatureTask.Control(TaskAction.Verify);

            readS2TemperatureTask = new Task("Read S2 temperature -" + name[2]);
            ((AnalogInputChannel)Environs.Hardware.AnalogInputChannels[channelName[2]]).AddToTask(readS2TemperatureTask, VOLTAGE_LOWER_BOUND, VOLTAGE_UPPER_BOUND);
            s2TemperatureReader = new AnalogSingleChannelReader(readS2TemperatureTask.Stream);
            readS2TemperatureTask.Control(TaskAction.Verify);

            readSF6TemperatureTask = new Task("Read SF6 temperature -" + name[3]);
            ((AnalogInputChannel)Environs.Hardware.AnalogInputChannels[channelName[3]]).AddToTask(readSF6TemperatureTask, VOLTAGE_LOWER_BOUND, VOLTAGE_UPPER_BOUND);
            sf6TemperatureReader = new AnalogSingleChannelReader(readSF6TemperatureTask.Stream);
            readSF6TemperatureTask.Control(TaskAction.Verify);
        }
        

        public SiliconDiodeTemperatureMonitors()
        {
        }

        double[,] rawVoltages = new double[4, NumberOfAverages];
        //double[] averageVoltages = new double[4];

        public double[] Temperature()
        {
            for (int n = 0; n < NumberOfAverages; n++)
            {
                readCellTemperatureTask.Start();
                double voltageCell = cellTemperatureReader.ReadSingleSample();
                readCellTemperatureTask.Stop();

                readS1TemperatureTask.Start();
                double voltageS1 = s1TemperatureReader.ReadSingleSample();
                readS1TemperatureTask.Stop();

                readS2TemperatureTask.Start();
                double voltageS2 = s2TemperatureReader.ReadSingleSample();
                readS2TemperatureTask.Stop();

                readSF6TemperatureTask.Start();
                double voltageSF6 = sf6TemperatureReader.ReadSingleSample();
                readSF6TemperatureTask.Stop();

                double[] Voltages = { voltageCell, voltageS1, voltageS2, voltageSF6 };
                
                for(int ii = 0; ii < 3; ii++)
                {
                    rawVoltages[ii, n] = Voltages[ii];
                }
            }

            double cellVoltageSum = 0;
            double s1VoltageSum = 0;
            double s2VoltageSum = 0;
            double sf6VoltageSum = 0;

            for(int n = 0; n < NumberOfAverages; n++)
            {
                cellVoltageSum += rawVoltages[0, n];
                s1VoltageSum += rawVoltages[1, n];
                s2VoltageSum += rawVoltages[2, n];
                sf6VoltageSum += rawVoltages[3, n]; 
            }

            double averageCellVoltage = cellVoltageSum / NumberOfAverages;
            double averageS1Voltage = s1VoltageSum / NumberOfAverages;
            double averageS2Voltage = s2VoltageSum / NumberOfAverages;
            double averagSF6Voltage = sf6VoltageSum / NumberOfAverages;

            double[] averageVoltages = { averageCellVoltage, averageS1Voltage, averageS2Voltage, averagSF6Voltage };

            //convert the read voltages to temperature
            double[] SourceTemperatures = VoltageTemperatureConversion(averageVoltages);

            return SourceTemperatures;
        }

        private double[] VoltageTemperatureConversion(double[] Voltages)
        {
            double[] Temperatures = new double[4];
            
            for (int i = 0; i<4; i++)
            {
                Temperatures[i] = ChebychevPolynomialsConversion(Voltages[i]);
            }

            return Temperatures;
        }

        private double ChebychevPolynomialsConversion(double Voltage)
        {
            double Temperature;

            if (Voltage >= VoltageAt12K & Voltage < VoltageAt2K)
            {
                Temperature = ChebychevPolynomialsLoop(Voltage, Range2to12K);
            }
            else
            {
                if (Voltage >= VoltageAt24Point5K & Voltage < VoltageAt12K)
                {
                    Temperature = ChebychevPolynomialsLoop(Voltage, Range12to24point5K);
                }
                else
                {
                    if (Voltage >= VoltageAt100K & Voltage < VoltageAt24Point5K)
                    {
                        Temperature = ChebychevPolynomialsLoop(Voltage, Range24point5to100K);
                    }
                    else
                    {
                        if (Voltage > VoltageAt500K & Voltage < VoltageAt100K) 
                        {
                            Temperature = ChebychevPolynomialsLoop(Voltage, Range100to500K);
                        }
                        else
                        {
                            if (Voltage > VoltageAt2K) Temperature = 0; // To avoid NaN - Below temperature measurement range 
                            else Temperature = 9999.9999; // To avoid NaN - Above temperature measurement range 
                        }
                    }
                }
            }

            return Temperature;
        }

        private double ChebychevPolynomialsLoop(double Voltage, double[] Constants)
        {
            double X = ((Voltage - Constants[0]) - (Constants[1] - Voltage)) / (Constants[1] - Constants[0]);
            double T = 0;

            for (int i = 0; i < Constants.Length-2; i++)
            {
                T += (Constants[i+2] * Math.Cos(i * Math.Acos(X)));
            }

            return T;
        }


    }
}
