using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using NationalInstruments.DAQmx;
using DAQ.HAL;
using DAQ.Environment;

namespace CaFBECHadwareController.Controls
{
    public class SourceTabController : GenericController
    {
        private double referenceResistance = 47120; // Reference resistor for reading in temperature from source thermistor

        private SourceTabView castView; // Convenience to avoid lots of casting in methods 

        private AnalogSingleChannelReader sf6FlowReader;
        private AnalogSingleChannelReader he6FlowReader;
        private DigitalSingleChannelWriter sf6ValveWriter;
        private DigitalSingleChannelWriter heValveWriter;

        private bool flowEnableFlag = false;
        private bool valveEnableFlag = false;
        private double[] tofData, tofTime;

        private System.Windows.Forms.Timer readTimer;

        private static string logfilePath = (string)Environs.FileSystem.Paths["SourceLogPath"];
        private static string toffilePath = (string)Environs.FileSystem.Paths["ToFFilesPath"];

        private int tof_timeout_count = 0;
        private string tof_signal_address, tof_trigger_address, power_mon_address;
        private int tof_samplerate, tof_num_of_samples;
        //Runs time of flight acquisition in a different thread to prevent user interface from hanging.
        System.ComponentModel.BackgroundWorker ToFWorker;
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
            castView = new SourceTabView(this);
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

        public int SamplingRate
        {
            get { return tof_samplerate; }
            set
            {
                this.tof_samplerate = value;
                set_Time_Axis();
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

        public SourceTabController()
        {
            InitReadTimer();

            sf6ValveWriter = CreateDigitalOutputWriter("sf6Valve");
            heValveWriter = CreateDigitalOutputWriter("heValve");
            sf6FlowReader = CreateAnalogInputReader("sf6FlowMonitor");
            he6FlowReader = CreateAnalogInputReader("heFlowMonitor");

            sf6FlowConversion = (double)Environs.Hardware.GetInfo("flowConversionSF6");
            heFlowConversion = (double)Environs.Hardware.GetInfo("flowConversionHe");
            tof_signal_address = (string)Environs.Hardware.GetInfo("ToFPMTSignal");
            tof_trigger_address = (string)Environs.Hardware.GetInfo("ToFTrigger");
            tof_samplerate = 100000;
            tof_num_of_samples = 1000;

            //Runs the Time of flight signal acquisition of the Molecular beam in a separate thread.
            ToFWorker = new System.ComponentModel.BackgroundWorker();
            ToFWorker.DoWork += AcquireTOF;
            ToFWorker.RunWorkerCompleted += AcquistionFinished;

            //Initializing an array for time axis of the Time of flight plot.
            tofTime = new double[tof_num_of_samples];
            set_Time_Axis();

        }

        private void set_Time_Axis()
        {
            for (int i = 0; i < tof_num_of_samples; i++)
                tofTime[i] = 1000.0 * (double)i / (double)tof_samplerate;
        }

        private void AcquistionFinished(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            readTimer.Start();
            if (flowEnableFlag == true)
            {
                castView.FlowEnable();
                flowEnableFlag = false;
            }
            if (valveEnableFlag == true)
            {
                castView.ValveOpen();
                valveEnableFlag = false;
            }
        }

        private void AcquireTOF(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            lock (acquisition_lock)
            {
                NationalInstruments.DAQmx.Task myTask = new NationalInstruments.DAQmx.Task();
                AIChannel aiChannel;

                switch (PlotChannel)
                {
                    case 0:
                        myTask.AIChannels.CreateVoltageChannel(tof_signal_address, "", AITerminalConfiguration.Rse, 0.0, 10.0, AIVoltageUnits.Volts);
                        break;
                    default:
                        myTask.AIChannels.CreateVoltageChannel(tof_signal_address, "", AITerminalConfiguration.Rse, 0.0, 10.0, AIVoltageUnits.Volts);
                        break;
                }

                // Configure timing specs and increase buffer size
                myTask.Timing.ConfigureSampleClock("", tof_samplerate, SampleClockActiveEdge.Rising, SampleQuantityMode.FiniteSamples, tof_num_of_samples);
                DigitalEdgeStartTriggerEdge triggerEdge = DigitalEdgeStartTriggerEdge.Rising;
                myTask.Triggers.StartTrigger.ConfigureDigitalEdgeTrigger(tof_trigger_address, triggerEdge);

                // Verify the task
                myTask.Control(TaskAction.Verify);
                // Start the task
                myTask.Start();
                myTask.Stream.Timeout = 2000;
                try
                {
                    AnalogSingleChannelReader reader = new AnalogSingleChannelReader(myTask.Stream);
                    tofData = reader.ReadMultiSample(1000);
                    if (castView.ToFEnabled())
                        castView.UpdateGraph(tofTime, tofData);

                    if (castView.AutomaticFlowControlEnabled())
                    {
                        flowEnableFlag = true;
                    }

                    if (castView.AutomaticValveControlEnabled())
                    {
                        valveEnableFlag = true;
                    }

                    tof_timeout_count = 0;
                    if (castView.SaveTraceStatus())
                    {
                        using (System.IO.StreamWriter file =
                            new System.IO.StreamWriter(toffilePath + "Tof_" + DateTime.Now.ToString("yyyy-MM-dd--HH-mm-ss-fff") + ".txt", false))
                        {
                            file.WriteLine("Sampling Rate: " + tof_samplerate.ToString() + ", Number of points :" + tof_num_of_samples.ToString());
                            foreach (double line in tofData)
                                file.WriteLine(line);
                            file.Flush();
                        }
                    }
                }
                catch (NationalInstruments.DAQmx.DaqException ex)
                {
                    //If no trigger is detected within the timeout duration, this DAQmx exception is raised. 
                    //This catch clause catches this exception and prevents the program from crashing.
                    tof_timeout_count++;

                }
                finally
                {
                    myTask.Stop();
                    myTask.Dispose();
                }
            }

        }

        private void InitReadTimer()
        {
            readTimer = new System.Windows.Forms.Timer();
            readTimer.Interval = 100;
            readTimer.Tick += new EventHandler(UpdateReadings);
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

                if (castView.ToFEnabled() || castView.AutomaticFlowControlEnabled() || castView.AutomaticValveControlEnabled())
                {

                    //System.Threading.Thread ToFAcquireThread = new Thread(new ThreadStart(AcquireTOF));
                    //ToFAcquireThread.Start();
                    readTimer.Stop();
                    ToFWorker.RunWorkerAsync();
                    //AcquireTOF();
                }

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

        public void SetPlotChannel(int channelID)
        {
            PlotChannel = channelID;
        }
    }
}
