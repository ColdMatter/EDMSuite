using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using NationalInstruments.DAQmx;
using DAQ.HAL;
using DAQ.Environment;
using System.Diagnostics;

namespace ZeemanSisyphusHardwareControl.Controls
{
    public class SourceTabController : GenericController
    {
        

        private SourceTabView castView; // Convenience to avoid lots of casting in methods
        private ControllerState state = ControllerState.STOPPED;
        private AnalogSingleChannelReader therm4KRTReader;
        private AnalogSingleChannelReader therm4KReader;
        private AnalogSingleChannelReader therm40KReader;
        private AnalogSingleChannelReader thermSF6Reader;
        private AnalogSingleChannelReader vRefReader;
        private double lowTempThreshCelcius = -20.0;
        private AnalogSingleChannelReader sourcePressureReaderNear;
        private AnalogSingleChannelReader sourcePressureReaderFar;
        private DigitalSingleChannelWriter cryoWriter;
        private DigitalSingleChannelWriter heaterWriter;
        private bool isCycling = false;
        private bool finishedHeating = true;
        private bool isHolding = false;
        private bool maxTempReached = true;
        private bool isRecording = false;
        private System.Windows.Forms.Timer readTimer;

        private enum ControllerState
        {
            RUNNING, STOPPED
        };

        protected override GenericView CreateControl()
        {
            castView = new SourceTabView(this);
            return castView;
        }

        public bool IsCyling
        {
            get { return isCycling; }
            set { this.isCycling = value; }
        }

        public bool IsHolding
        {
            get { return isHolding; }
            set { this.isHolding = value; }
        }

        public bool IsRecording
        {
            get { return isRecording; }
            set { this.isRecording = value; }

        }

        public SourceTabController()
        {
            InitReadTimer();

            therm4KReader = CreateAnalogInputReader("4Kthermistor");
            therm40KReader = CreateAnalogInputReader("40Kthermistor");
            thermSF6Reader = CreateAnalogInputReader("SF6thermistor");

            // vRefReader = CreateAnalogInputReader("thermVref");
            
            cryoWriter = CreateDigitalOutputWriter("cryoCooler");
            heaterWriter = CreateDigitalOutputWriter("sourceHeater");
            sourcePressureReaderNear = CreateAnalogInputReader("sourcePressureNear");
            sourcePressureReaderFar = CreateAnalogInputReader("sourcePressureFar");
        }

        private void InitReadTimer()
        {
            readTimer = new System.Windows.Forms.Timer();
            readTimer.Interval = 2000;
            readTimer.Tick += new EventHandler(UpdateTemperature);
            readTimer.Tick += new EventHandler(UpdatePressure);

        }

        protected double ConvertVoltageToResistance(double voltage, double reference)
        {
            return castView.GetReferenceResistance() * voltage / (reference - voltage);
        }

        protected double Convert10kResistanceToCelcius(double resistance)
        {
            // Function is now redundant (superceded by SteinhartHartEquation()): keep for posterity.
            // Constants for Steinhart & Hart equation
            double A = 0.001125308852122;
            double B = 0.000234711863267;
            double D = 0.000000085663516;

            return 1 / (A + B * Math.Log(resistance) + D * Math.Pow(Math.Log(resistance), 3)) - 273.15;
        }

        protected double SteinhartHartEquation(double resistance, bool lowTemp, bool kelvin = false)
        {
            //double[] lowTempConstants = new double[] {
            //    -0.8448254546374075,
            //    0.5207880924960208,
            //    -0.10709667105482057,
            //    0.007405708155883199
            //};

            double[] lowTempConstants = new double[] {
                -0.398963,
                0.253231,
                -0.0539933,
                0.00391135
            };

            double[] roomTempConstants = new double[] {
                0.0011279,
                0.00023429,
                0.0,
                0.000000087298
            };

            double[] constants = lowTemp ? lowTempConstants : roomTempConstants;

            return 1 / (constants[0] + constants[1] * Math.Log(resistance) + constants[2] * Math.Pow(Math.Log(resistance), 2) + constants[3] * Math.Pow(Math.Log(resistance), 3)) - (kelvin ? 0 : 273.15);
        }

        protected double DiodeTempSensor(double voltage, int forceUnit = 0)
        {
            double[] bestFitParams = new double[] { 1934.7888045273014, -64462.659285767455, 1.276416501414742e6, -1.4641921851200823e7, 1.0783133969590467e8, -5.380059453769246e8, 1.865448582858507e9, -4.513828578778938e9, 7.465355337382778e9, -7.883349924406745e9, 4.2206775553140235e9, 3.9523108322669417e8, -1.5718836209714313e9, 2.5380804756535333e7, 5.465370084659193e8, 9.52193486635682e7, -1.6213638247297624e8, -9.711404423315269e7, 1.0996369042515364e7, 4.01165628171435e7, 2.0092946365901615e7, -1.147408402908425e6, -7.89986217815785e6, -5.364892916489303e6, -1.2963370698529975e6, 876246.0830894151, 1.1808794928657783e6, 686489.6300404054, 173394.7913404117, -92148.55246723139, -145190.25946118278, -100277.13098662965, -41627.6266443905, -3341.9276876675935, 11745.614774628208, 12609.450960216696, 8093.775238022651, 3453.8216915200114, 515.2637902204726, -728.9307841738207, -924.2198793088207, -675.8092061673293, -357.4384070791826, -121.98967703121092, 3.8105629441827262, 48.30675249829439, 49.04021270954457, 33.71062293050828, 17.508612591490245, 6.1330592052553685, 0.0628408961746619, -2.174301817759938, -2.3416133762335396, -1.692562798071984, -0.9406020358210435, -0.38103444987903495, -0.059434486649667856, 0.07803204787712965, 0.10557726120559542, 0.08412304350698938, 0.05094378722829566, 0.023155388252641337, 0.005728360318250693, -0.002579631452138557, -0.0049376938445728135, -0.004301916572534432, -0.0027072481654802443, -0.0012364304182879756, -0.00026951845660130115, 0.0001937339959166064, 0.0003089942168465374, 0.00024645029476956753, 0.00013188340875776717, 0.00003558998229822812, -0.000018122273530209032, -0.00003274961097680359, -0.00002347215627681461, -6.599209985531554e-6, 5.7596426562489434e-6, 7.396549856862535e-6, -2.7506877372290873e-6 };

            double tempKelvin = 0;
            double retval = 0;

            for (int i = 0; i <= 80; i++)
            {
                tempKelvin = tempKelvin + bestFitParams[i] * Math.Pow(voltage, i);
            }

            if ((forceUnit == 0 && tempKelvin > 173.2) || (forceUnit == 1))
            {
                // Use celcius if the temperature is above 173.2 K (-100 C) and forceUnit = 0
                // OR if forceUnit = 1
                retval = tempKelvin - 273.15;
            }
            else
            {
                // Otherwise use Kelvin
                retval = tempKelvin;
            }

            return retval;
        }

        public double[] GetTemperature()
        {
            double thermSF6Voltage = thermSF6Reader.ReadSingleSample();
            double therm40KVoltage = therm40KReader.ReadSingleSample();
            double therm4KVoltage = therm4KReader.ReadSingleSample();

            Console.Write(therm40KVoltage);

            double thermSF6Temp = DiodeTempSensor(thermSF6Voltage);
            double therm40KTemp = DiodeTempSensor(therm40KVoltage);
            double therm4KTemp = DiodeTempSensor(therm4KVoltage);

            double[] retval = { thermSF6Temp, therm40KTemp, therm4KTemp };
            return retval;
        }

        protected void UpdateTemperature(object anObject, EventArgs eventArgs)
        {
            double[] tempInfo = GetTemperature();
            double temp = tempInfo[2];
            double temp40K = tempInfo[1];
            double tempSF6 = tempInfo[0];

            castView.UpdateCurrentTemperature(temp.ToString("F4"));
            castView.UpdateCurrent40KTemperature(temp40K.ToString("F4"));
            castView.UpdateCurrentSF6Temperature(tempSF6.ToString("F2"));

            if (IsCyling)
            {
                double cycleLimit = castView.GetCycleLimit();
                if (!finishedHeating && temp > cycleLimit)
                { 
                    finishedHeating = true;
                    SetHeaterState(false);
                    SetCryoState(true);
                }
            }

            if (IsHolding)
            {
                double cycleLimit = castView.GetCycleLimit();
                if (temp < cycleLimit && !maxTempReached)
                {
                    SetHeaterState(true);
                }
                else if (temp > cycleLimit && !maxTempReached)
                {
                    SetHeaterState(false);
                    maxTempReached = true;
                }
                else if (temp < cycleLimit - 0.006 && maxTempReached)
                {
                    SetHeaterState(true);
                    maxTempReached = false;
                }
            }

            if (IsRecording)
            {
                using (System.IO.StreamWriter file =
            new System.IO.StreamWriter(@"D:\\Box Sync\\CaF MOT\\ZeemanSisyphus\\data\\temperatureData\\SF6Line.csv", true))
                {
                    file.WriteLine(tempSF6 + "," + temp + "," + temp40K);
                }

            }
        }

        public void SetCryoState(bool state) 
        {
            cryoWriter.WriteSingleSampleSingleLine(true, state);
            castView.SetCryoState(state);
        }

        public void SetHeaterState(bool state)
        {
            heaterWriter.WriteSingleSampleSingleLine(true, state);
            castView.SetHeaterState(state);
        }

        public void ToggleReading() 
        {
            if (!readTimer.Enabled)
            {
                readTimer.Start();
                castView.UpdateReadButton(false);
                castView.EnableControls(true);
            }
            else
            {
                readTimer.Stop();
                castView.UpdateReadButton(true);
                castView.EnableControls(false);
            }
        }

        public void ToggleRecording()
        {
            isRecording = !isRecording;
            castView.UpdateRecordButton(!isRecording);
        }

        public void ToggleCycling()
        {
            isCycling = !isCycling;
            castView.UpdateCycleButton(!isCycling);
            if (IsCyling)
            {
                SetHeaterState(true);
                SetCryoState(false);
                finishedHeating = false;
            }
        }

        public void ToggleHolding()
        {
            isHolding = !isHolding;
            castView.UpdateHoldButton(!isHolding);
            if (isHolding)
            {
                SetCryoState(false);

                double[] tempInfo = GetTemperature();
                double temp = tempInfo[0];

                double cycleLimit = castView.GetCycleLimit();
                if (temp < cycleLimit)
                {
                    SetHeaterState(true);
                    maxTempReached = false;
                }
                else
                {
                    SetHeaterState(false);
                    maxTempReached = true;
                }
            }
        }
        protected double ConvertVoltageToPressure(double voltage)
        {
            return Math.Pow(10.0,(voltage - 7.75)/0.75); //conversion for IONIVAC ITR 90 in mbar
        }

        protected double GetPressure(AnalogSingleChannelReader reader)
        {
            double sourcePressureVoltage = reader.ReadSingleSample();
            return ConvertVoltageToPressure(sourcePressureVoltage);
        }

        protected void UpdatePressure(object anObject, EventArgs eventArgs)
        {
            double pressureNear = GetPressure(sourcePressureReaderNear);
            double pressureFar = GetPressure(sourcePressureReaderFar);
            string[] pressures = { pressureNear.ToString("e2"), pressureFar.ToString("e2") };
            castView.UpdateCurrentPressure(pressures);
            if (IsRecording)
            {
                using (System.IO.StreamWriter file = new System.IO.StreamWriter(@"D:\\Box Sync\\CaF MOT\\ZeemanSisyphus\\data\\pressureData\\data.csv", true))
                {
                    file.WriteLine(pressureNear + "," + pressureFar);
                }

            }
        }
        
    }
}
