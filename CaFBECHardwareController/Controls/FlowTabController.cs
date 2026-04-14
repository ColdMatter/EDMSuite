using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using NationalInstruments.DAQmx;
using DAQ.HAL;
using DAQ.Environment;

namespace CaFBECHardwareController.Controls
{
    public class FlowTabController : GenericController
    {
        private FlowTabView castView; // Convenience to avoid lots of casting in methods 

        private AnalogSingleChannelReader sf6FlowReader;
        private AnalogSingleChannelReader he6FlowReader;
        private DigitalSingleChannelWriter sf6ValveWriter;
        private DigitalSingleChannelWriter heValveWriter;

        private bool flowEnableFlag = false;
        private bool valveEnableFlag = false;

        private System.Windows.Forms.Timer readTimer;

        //lock object is used to make sure that the two threads do not try to get the handle for the DAQ module at the same time.
        private readonly object acquisition_lock = new object();

        // Analog Output limits and current voltage.
        private double AO0Max = 5.0;
        private double AO0Min = 0.0;
        private double AO1Max = 5.0;
        private double AO1Min = 0.0;
        private double[] AOVoltage = new double[] { 0.0, 0.0 };

        //Flow Conversions for flow monitor in sccm per Volt
        double sf6FlowConversion, heFlowConversion;

        private int PlotChannel = 0;
        private int flowTimeoutCount = 15;

        protected override GenericView CreateControl()
        {
            castView = new FlowTabView(this);
            return castView;
        }

        public int FlowTimeOut
        {
            get { return flowTimeoutCount * 2000; }
            set
            {
                this.flowTimeoutCount = value / 2000;
            }
        }

        public double[] AnalogOutLimits
        {
            get
            {
                double[] limits = new double[] { AO0Min, AO0Max, AO1Min, AO1Max };
                return limits;
            }

        }

        public FlowTabController()
        {
            InitReadTimer();

            sf6ValveWriter = CreateDigitalOutputWriter("sf6Valve");
            heValveWriter = CreateDigitalOutputWriter("heValve");
            sf6FlowReader = CreateAnalogInputReader("sf6FlowMonitor");
            he6FlowReader = CreateAnalogInputReader("heFlowMonitor");

            sf6FlowConversion = (double)Environs.Hardware.GetInfo("flowConversionSF6");
            heFlowConversion = (double)Environs.Hardware.GetInfo("flowConversionHe");
        }

        private void AcquistionFinished(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            readTimer.Start();
            if (flowEnableFlag == true)
            {
                castView.FlowEnable();
                flowEnableFlag = false;
            }
        }

        private void InitReadTimer()
        {
            readTimer = new System.Windows.Forms.Timer();
            readTimer.Interval = 100;
            readTimer.Tick += new EventHandler(UpdateReadings);
        }

        protected double GetSF6Flow()
        {
            //double sf6FlowVoltage = sf6FlowReader.ReadSingleSample();
            //return sf6FlowVoltage * sf6FlowConversion;
            double[] sf6FlowVoltage = sf6FlowReader.ReadMultiSample(500);
            return sf6FlowVoltage.Average() * sf6FlowConversion;

        }

        protected double GetHeFlow()
        {
            //double heFlowVoltage = he6FlowReader.ReadSingleSample();
            double[] heFlowVoltage = he6FlowReader.ReadMultiSample(500);
            return heFlowVoltage.Average() * heFlowConversion;

        }

        protected void UpdateReadings(object anObject, EventArgs eventArgs)
        {
            lock (acquisition_lock)
            {

                double sf6Flow = GetSF6Flow();
                double heFlow = GetHeFlow();

                castView.UpdateFlowRates(sf6Flow, heFlow);
            }
        }

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

        public void ToggleDigitalOutput(int channel, bool state)
        {
            lock (acquisition_lock)
            {
                if (channel == 2)
                {
                    sf6ValveWriter.WriteSingleSampleSingleLine(true, state);
                }
                else if (channel == 3)
                {
                    heValveWriter.WriteSingleSampleSingleLine(true, state);
                }
            }
        }

        public void SwitchOutputAOVoltage(int channel)
        {
            lock (acquisition_lock)
            {
                Task analogOutTask = new Task();
                switch (channel)
                {
                    case 0:
                        ((AnalogOutputChannel)Environs.Hardware.AnalogOutputChannels["sf6FlowSet"]).AddToTask(analogOutTask, AO0Min, AO0Max);
                        break;
                    case 1:
                        ((AnalogOutputChannel)Environs.Hardware.AnalogOutputChannels["heFlowSet"]).AddToTask(analogOutTask, AO1Min, AO1Max);
                        break;
                }

                AnalogSingleChannelWriter analogwriter = new AnalogSingleChannelWriter(analogOutTask.Stream);
                bool[] switchStatus = castView.GetAnalogOutputEnableStatus();
                if (switchStatus[channel] == true)
                    analogwriter.WriteSingleSample(true, AOVoltage[channel]);
                else
                    analogwriter.WriteSingleSample(true, 0.0);

                analogOutTask.Stop();
                analogOutTask.Dispose();
            }
        }

        public void SetAnalogOutput(int channel, double voltage)
        {
            AOVoltage[channel] = voltage;
            SwitchOutputAOVoltage(channel);
        }
    }
}
