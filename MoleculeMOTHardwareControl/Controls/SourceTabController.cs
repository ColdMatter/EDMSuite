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
        private AnalogSingleChannelReader vRefReader;
        private bool isCycling = false;
        private System.Windows.Forms.Timer readTimer;

        public SourceTabController()
        {
            InitReadTimer();

            AnalogInputChannel sourceTempChannel = (AnalogInputChannel)Environs.Hardware.AnalogInputChannels["sourceTemperature"];
            Task sourceTempTask = new Task();
            sourceTempReader = new AnalogSingleChannelReader(sourceTempTask.Stream);

            //AnalogInputChannel vRefChannel = (AnalogInputChannel)Environs.Hardware.AnalogInputChannels["voltageReference"];
            //Task vRefTask = new Task();
            //vRefReader = new AnalogSingleChannelReader(vRefTask.Stream);
        }

        private void InitReadTimer()
        {
            readTimer = new System.Windows.Forms.Timer();
            readTimer.Interval = 2000;
            readTimer.Tick += new EventHandler(UpdateTemperature);
        }
        
        protected override GenericView CreateControl()
        {
            castView = new SourceTabView();
            return castView;
        }

        private enum ControllerState 
        {
            RUNNING, STOPPED
        };

        public bool IsCyling
        {
            get { return isCycling; }
            set { this.isCycling = value; }
        }

        protected double ConvertVoltageToResistance(double voltage, double reference)
        {
            return referenceResistance * voltage / (5 - voltage);
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
            DateTime timeStamp = DateTime.Now;
            double time = Convert.ToDouble(timeStamp);
            castView.UpdateGraph(time, temp);
            //if (IsCyling)
            //{
            //    double cycleLimit = castView.GetCycleLimit();
            //    if (temp > cycleLimit)
            //    {
            //        SetHeaterState(false);
            //        SetCryoState(true);
            //    }
            //}
        }

        public void SetCryoState() { }

        public void SetHeaterState() { }

        public void ToggleReading() 
        {
            if (!readTimer.Enabled)
            {
                readTimer.Start();
                castView.UpdateReadButton(false);
            }
            else
            {
                readTimer.Stop();
                castView.UpdateReadButton(true);
            }
        }

        public void StartCycling() { }

        public void StopCycling() { }
        
    }
}
