using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using NationalInstruments.DAQmx;
using DAQ.HAL;
using DAQ.Environment;

namespace MoleculeMOTHardwareControl.Controls
{
    public class SourceTabController : GenericController
    {
        private double referenceResistance = 47120; // Reference resistor for reading in temperature from source thermistor

        private SourceTabView castView; // Convenience to avoid lots of casting in methods 
        private AnalogSingleChannelReader sourceTempReader;
        private AnalogSingleChannelReader sf6TempReader;
        private DigitalSingleChannelWriter cryoWriter;
        private DigitalSingleChannelWriter heaterWriter;
        private bool isCycling = false;
        private bool finishedHeating = true;

        private bool isHolding = false;
        private bool maxTempReached = true;
        private System.Windows.Forms.Timer readTimer;

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

            sourceTempReader = CreateAnalogInputReader("sourceTemp");
            sf6TempReader = CreateAnalogInputReader("sf6Temp");
            cryoWriter = CreateDigitalOutputWriter("cryoCooler");
            heaterWriter = CreateDigitalOutputWriter("sourceHeater");
        }

        private void InitReadTimer()
        {
            readTimer = new System.Windows.Forms.Timer();
            readTimer.Interval = 2000;
            readTimer.Tick += new EventHandler(UpdateTemperature);
        }

        protected double ConvertVoltageToResistance(double voltage, double reference)
        {
            return referenceResistance * voltage / (reference - voltage);
        }

        protected double Convert10kResistanceToCelcius(double resistance)
        {
            // Constants for Steinhart & Hart equation
            double A = 0.001125308852122;
            double B = 0.000234711863267;
            double C = 0.000000085663516;

            return 1 / (A + B * Math.Log(resistance) + C * Math.Pow(Math.Log(resistance), 3)) - 273.15;
            
        }

        protected double GetSourceTemperature()
        {
            double vRef = 5.0; //vRefReader.ReadSingleSample();
            double sourceTempVoltage = sourceTempReader.ReadSingleSample();
            double sourceTempResistance = ConvertVoltageToResistance(sourceTempVoltage, vRef);
            return Convert10kResistanceToCelcius(sourceTempResistance);
        }

        protected double GetSF6Temperature()
        {
            double vRef = 5.0; //vRefReader.ReadSingleSample();
            double sf6TempVoltage = sf6TempReader.ReadSingleSample();
            double sf6TempResistance = ConvertVoltageToResistance(sf6TempVoltage, vRef);
            return Convert10kResistanceToCelcius(sf6TempResistance);
        }

        protected void UpdateTemperature(object anObject, EventArgs eventArgs)
        {
            double sourceTemp = GetSourceTemperature();
            if (sourceTemp < -34)
            {
                castView.UpdateCurrentSourceTemperature("<-34");
            }
            else
            {
                castView.UpdateCurrentSourceTemperature(sourceTemp.ToString("F2"));
            }
            double sf6Temp = GetSF6Temperature();
            if (sf6Temp < -34)
            {
                castView.UpdateCurrentSF6Temperature("<-34");
            }
            else
            {
                castView.UpdateCurrentSF6Temperature(sf6Temp.ToString("F2"));
            }

            if (IsCyling)
            {
                double cycleLimit = castView.GetCycleLimit();
                if (!finishedHeating && sourceTemp > cycleLimit)
                {
                    finishedHeating = true;
                    SetHeaterState(false);
                    SetCryoState(true);
                }
            }
            if (IsHolding)
            {
                double cycleLimit = castView.GetCycleLimit();
                if (sourceTemp < cycleLimit && !maxTempReached)
                {
                    SetHeaterState(true);
                }
                else if (sourceTemp > cycleLimit && !maxTempReached)
                {
                    SetHeaterState(false);
                    maxTempReached = true;
                }
                else if (sourceTemp < cycleLimit - 3 && maxTempReached)
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
                double temp = GetSourceTemperature();
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
        
    }
}
