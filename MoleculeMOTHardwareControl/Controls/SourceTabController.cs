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
        private ControllerState state = ControllerState.STOPPED;
        private AnalogSingleChannelReader sourceTempReader;
        private DigitalSingleChannelWriter cryoWriter;
        private DigitalSingleChannelWriter heaterWriter;
        private AnalogSingleChannelReader vRefReader;
        private bool isCycling = false;
        private bool finishedHeating = true;
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

        public SourceTabController()
        {
            InitReadTimer();

            sourceTempReader = CreateAnalogInputReader("sourceTemp");
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

        protected double GetTemperature()
        {
            double vRef = 5.0; //vRefReader.ReadSingleSample();
            double sourceTempVoltage = sourceTempReader.ReadSingleSample();
            double sourceTempResistance = ConvertVoltageToResistance(sourceTempVoltage, vRef);
            return Convert10kResistanceToCelcius(sourceTempResistance);
        }

        protected void UpdateTemperature(object anObject, EventArgs eventArgs)
        {
            double temp = GetTemperature();
            if (temp < -34)
            {
                castView.UpdateCurrentTemperature("<-34");
            }
            else
            {
                castView.UpdateCurrentTemperature(temp.ToString("F2"));
            }
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
        
    }
}
