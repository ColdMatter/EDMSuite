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
        private AnalogSingleChannelReader vRefReader;
        private double lowTempThreshCelcius = -34.0;
        private AnalogSingleChannelReader sourcePressureReader;
        private DigitalSingleChannelWriter cryoWriter;
        private DigitalSingleChannelWriter heaterWriter;
        private bool isCycling = false;
        private bool finishedHeating = true;
        private bool isHolding = false;
        private bool maxTempReached = true;
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

        public SourceTabController()
        {
            InitReadTimer();

            therm4KRTReader = CreateAnalogInputReader("4KRTthermistor");
            therm4KReader = CreateAnalogInputReader("4Kthermistor");
            vRefReader = CreateAnalogInputReader("thermVref");
            cryoWriter = CreateDigitalOutputWriter("cryoCooler");
            heaterWriter = CreateDigitalOutputWriter("sourceHeater");
            sourcePressureReader = CreateAnalogInputReader("sourcePressure");

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
            double[] lowTempConstants = new double[] {
                -0.8448254546374075,
                0.5207880924960208,
                -0.10709667105482057,
                0.007405708155883199
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

        protected double[] GetTemperature()
        {
            double vRef = vRefReader.ReadSingleSample();
            double therm4KRTVoltage = therm4KRTReader.ReadSingleSample();
            double therm4KRTResistance = ConvertVoltageToResistance(therm4KRTVoltage, vRef);
            double[] tempFromRTTherm = { SteinhartHartEquation(therm4KRTResistance, false), 0 };

            double therm4KVoltage = therm4KReader.ReadSingleSample();
            double therm4KResistance = ConvertVoltageToResistance(therm4KVoltage, vRef);
            double[] tempFromLTTherm = { SteinhartHartEquation(therm4KResistance, true), 1 };

            return (Double.IsNaN(tempFromRTTherm[0]) || (tempFromRTTherm[0] < lowTempThreshCelcius)) ? tempFromLTTherm : tempFromRTTherm;
        }

        protected void UpdateTemperature(object anObject, EventArgs eventArgs)
        {
            double[] tempInfo = GetTemperature();
            double temp = tempInfo[0];

            castView.UpdateCurrentTemperature(tempInfo[0].ToString("F2"), tempInfo[1] == 1);

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
                else if (temp < cycleLimit - 3 && maxTempReached)
                {
                    SetHeaterState(true);
                    maxTempReached = false;
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
            return Math.Pow(10.0,(voltage - 10.875));//conversion for cold cathode ionization gauge series 903
        }

        protected double GetPressure()
        {
            double sourcePressureVoltage = sourcePressureReader.ReadSingleSample();
            return ConvertVoltageToPressure(sourcePressureVoltage);
        }

        protected void UpdatePressure(object anObject, EventArgs eventArgs)
        {
            double pressure = GetPressure();
            
            {
                castView.UpdateCurrentPressure(pressure.ToString("e2"));
        
            }
        }
        
    }
}
