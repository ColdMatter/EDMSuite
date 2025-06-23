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
    public class SourceTabController : GenericController
    {
        private SourceTabView castView; // Convenience to avoid lots of casting in methods

        private double[] tofTime;
        private double[] tofTimeAbs;
        private double[,] tofData;
        private double[,] tofDataAbs;

        private static string toffilePath = (string)Environs.FileSystem.Paths["ToFFilesPath"];

        private int tof_timeout_count = 0;
        private string tof_pmt_address, tof_abs_address, tof_trigger_address, tof_abs_trigger_address;
        private int tof_samplerate, tof_samplerate_abs, tof_num_of_samples;
        //Runs time of flight acquisition in a different thread to prevent user interface from hanging.
        System.ComponentModel.BackgroundWorker ToFWorker;
        System.ComponentModel.BackgroundWorker ToFWorkerAbs;
        //lock object is used to make sure that the two threads do not try to get the handle for the DAQ module at the same time.
        private readonly object acquisition_lock = new object();
        private readonly object acquisition_lock_abs = new object();

        private bool acquisitionRunning = false;

        //private AnalogSingleChannelReader sf6FlowReader;
        //private AnalogSingleChannelReader he6FlowReader;
        //private DigitalSingleChannelWriter sf6ValveWriter;
        //private DigitalSingleChannelWriter heValveWriter;

        //private bool flowEnableFlag = false;
        //private bool valveEnableFlag = false;

        //private System.Windows.Forms.Timer readTimer;

        //Flow Conversions for flow monitor in sccm per Volt
        //double sf6FlowConversion, heFlowConversion;

        //private int flowTimeoutCount = 15;

        // Analog Output limits and current voltage.
        //private double AO0Max = 5.0;
        //private double AO0Min = 0.0;
        //private double AO1Max = 5.0;
        //private double AO1Min = 0.0;
        //private double[] AOVoltage = new double[] { 0.0, 0.0 };

        protected override GenericView CreateControl()
        {
            castView = new SourceTabView(this);
            return castView;
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

        public int SamplingRateAbs
        {
            get { return tof_samplerate_abs; }
            set
            {
                this.tof_samplerate_abs = value;
                set_Time_Axis_abs();
            }
        }

        //public int FlowTimeOut
        //{
        //    get { return flowTimeoutCount * 2000; }
        //    set
        //    {
        //        this.flowTimeoutCount = value / 2000;
        //    }
        //}

        //public double[] AnalogOutLimits
        //{
        //    get
        //    {
        //        double[] limits = new double[] { AO0Min, AO0Max, AO1Min, AO1Max };
        //        return limits;
        //    }

        //}

        public SourceTabController()
        {
            //InitReadTimer();

            tof_pmt_address = (string)Environs.Hardware.GetInfo("ToFPMTSignal");
            tof_abs_address = (string)Environs.Hardware.GetInfo("ToFAbsorptionSignal");
            tof_trigger_address = (string)Environs.Hardware.GetInfo("ToFTrigger");
            tof_abs_trigger_address = (string)Environs.Hardware.GetInfo("ToFAbsorptionTrigger");
            tof_samplerate = 80000;
            tof_samplerate_abs = 250000;
            tof_num_of_samples = 1000;

            //Runs the Time of flight signal acquisition of the molecular beam in a separate thread.
            ToFWorker = new System.ComponentModel.BackgroundWorker();
            ToFWorker.DoWork += AcquireTOF;
            ToFWorker.RunWorkerCompleted += AcquistionFinished;
            ToFWorker.WorkerSupportsCancellation = true;

            ToFWorkerAbs = new System.ComponentModel.BackgroundWorker();
            ToFWorkerAbs.DoWork += AcquireTOFAbs;
            ToFWorkerAbs.RunWorkerCompleted += AcquistionFinishedAbs;
            ToFWorkerAbs.WorkerSupportsCancellation = true;

            //Initializing an array for time axis of the Time of flight plot.
            tofTime = new double[tof_num_of_samples];
            tofTimeAbs = new double[tof_num_of_samples];
            set_Time_Axis();
            set_Time_Axis_abs();

            //sf6ValveWriter = CreateDigitalOutputWriter("sf6Valve");
            //heValveWriter = CreateDigitalOutputWriter("heValve");
            //sf6FlowReader = CreateAnalogInputReader("sf6FlowMonitor");
            //he6FlowReader = CreateAnalogInputReader("heFlowMonitor");

            //sf6FlowConversion = (double)Environs.Hardware.GetInfo("flowConversionSF6");
            //heFlowConversion = (double)Environs.Hardware.GetInfo("flowConversionHe");

        }

        private void set_Time_Axis()
        {
            for (int i = 0; i < tof_num_of_samples; i++)
                tofTime[i] = 1000.0 * (double)i / (double)tof_samplerate;
        }

        private void set_Time_Axis_abs()
        {
            for (int i = 0; i < tof_num_of_samples; i++)
                tofTimeAbs[i] = 1000.0 * (double)i / (double)tof_samplerate_abs;
        }

        //private void InitReadTimer()
        //{
        //    readTimer = new System.Windows.Forms.Timer();
        //    readTimer.Interval = 1000;
        //    readTimer.Tick += new EventHandler(UpdateReadings);
        //}

        //protected double GetSF6Flow()
        //{
        //    double[] sf6FlowVoltage = sf6FlowReader.ReadMultiSample(500);
        //    return sf6FlowVoltage.Average() * sf6FlowConversion;
        //}

        //protected double GetHeFlow()
        //{
        //    double[] heFlowVoltage = he6FlowReader.ReadMultiSample(500);
        //    return heFlowVoltage.Average() * heFlowConversion;
        //}

        //protected void UpdateReadings(object anObject, EventArgs eventArgs)
        //{
        //    lock (acquisition_lock)
        //    {

        //        double sf6Flow = GetSF6Flow();
        //        double heFlow = GetHeFlow();

        //        castView.UpdateFlowRates(sf6Flow, heFlow);
        //    }
        //}

        private void AcquistionFinished(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            if (acquisitionRunning)
            {
                ToFWorker.RunWorkerAsync();
            }
        }

        private void AcquireTOF(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            lock (acquisition_lock)
            {
                NationalInstruments.DAQmx.Task myTask = new NationalInstruments.DAQmx.Task();

                myTask.AIChannels.CreateVoltageChannel(tof_pmt_address, "", AITerminalConfiguration.Rse, 0.0, 10.0, AIVoltageUnits.Volts);

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
                    AnalogMultiChannelReader reader = new AnalogMultiChannelReader(myTask.Stream);
                    tofData = reader.ReadMultiSample(tof_num_of_samples);

                    int dataLength = tofData.GetLength(1);
                    double[] pmtData = new double[dataLength];
                    for (int i = 0; i < dataLength; i++)
                    {
                        pmtData[i] = tofData[0, i];
                    }

                    if (castView.ToFEnabled())
                        castView.UpdateGraphPMT(tofTime, pmtData);

                    tof_timeout_count = 0;
                    if (castView.SaveTraceStatus())
                    {
                        using (System.IO.StreamWriter file =
                            new System.IO.StreamWriter(toffilePath + "Tof_PMT_" + DateTime.Now.ToString("yyyy-MM-dd--HH-mm-ss-fff") + ".txt", false))
                        {
                            file.WriteLine("Sampling Rate: " + tof_samplerate.ToString() + ", Number of points :" + tof_num_of_samples.ToString());
                            foreach (double line in pmtData)
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

        private void AcquistionFinishedAbs(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            if (acquisitionRunning)
            {
                ToFWorkerAbs.RunWorkerAsync();
            }
        }

        private void AcquireTOFAbs(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            lock (acquisition_lock_abs)
            {
                NationalInstruments.DAQmx.Task myTaskAbs = new NationalInstruments.DAQmx.Task();

                myTaskAbs.AIChannels.CreateVoltageChannel(tof_abs_address, "", AITerminalConfiguration.Rse, 0.0, 10.0, AIVoltageUnits.Volts);

                // Configure timing specs and increase buffer size
                myTaskAbs.Timing.ConfigureSampleClock("", tof_samplerate_abs, SampleClockActiveEdge.Rising, SampleQuantityMode.FiniteSamples, tof_num_of_samples);
                DigitalEdgeStartTriggerEdge triggerEdge = DigitalEdgeStartTriggerEdge.Rising;
                myTaskAbs.Triggers.StartTrigger.ConfigureDigitalEdgeTrigger(tof_abs_trigger_address, triggerEdge);

                // Verify the task
                myTaskAbs.Control(TaskAction.Verify);
                // Start the task
                myTaskAbs.Start();
                myTaskAbs.Stream.Timeout = 2000;
                try
                {
                    AnalogMultiChannelReader reader = new AnalogMultiChannelReader(myTaskAbs.Stream);
                    tofDataAbs = reader.ReadMultiSample(tof_num_of_samples);

                    int dataLength = tofDataAbs.GetLength(1);
                    double[] absData = new double[dataLength];
                    for (int i = 0; i < dataLength; i++)
                    {
                        absData[i] = tofDataAbs[0, i];
                    }

                    if (castView.ToFEnabledAbs())
                        castView.UpdateGraphAbs(tofTimeAbs, absData);

                    tof_timeout_count = 0;
                    if (castView.SaveTraceStatusAbs())
                    {
                        using (System.IO.StreamWriter file =
                            new System.IO.StreamWriter(toffilePath + "Tof_Absorption_" + DateTime.Now.ToString("yyyy-MM-dd--HH-mm-ss-fff") + ".txt", false))
                        {
                            file.WriteLine("Sampling Rate: " + tof_samplerate_abs.ToString() + ", Number of points :" + tof_num_of_samples.ToString());
                            foreach (double line in absData)
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
                    myTaskAbs.Stop();
                    myTaskAbs.Dispose();
                }
            }

        }

        //public void SwitchOutputAOVoltage(int channel)
        //{
        //    lock (acquisition_lock)
        //    {
        //        Task analogOutTask = new Task();
        //        switch (channel)
        //        {
        //            case 0:
        //                ((AnalogOutputChannel)Environs.Hardware.AnalogOutputChannels["sf6FlowSet"]).AddToTask(analogOutTask, AO0Min, AO0Max);
        //                break;
        //            case 1:
        //                ((AnalogOutputChannel)Environs.Hardware.AnalogOutputChannels["heFlowSet"]).AddToTask(analogOutTask, AO1Min, AO1Max);
        //                break;
        //        }

        //        AnalogSingleChannelWriter analogwriter = new AnalogSingleChannelWriter(analogOutTask.Stream);
        //        bool[] switchStatus = castView.GetAnalogOutputEnableStatus();
        //        if (switchStatus[channel] == true)
        //            analogwriter.WriteSingleSample(true, AOVoltage[channel]);
        //        else
        //            analogwriter.WriteSingleSample(true, 0.0);

        //        analogOutTask.Stop();
        //        analogOutTask.Dispose();
        //    }
        //}

        //public void SetAnalogOutput(int channel, double voltage)
        //{
        //    AOVoltage[channel] = voltage;
        //    SwitchOutputAOVoltage(channel);
        //}

        public void ToggleReading()
        {
            if (acquisitionRunning)
            {
                acquisitionRunning = false;
                ToFWorker.CancelAsync();
                ToFWorkerAbs.CancelAsync();
                castView.UpdateReadButton(true);
                //readTimer.Stop();
            }
            else
            {
                acquisitionRunning = true;
                ToFWorker.RunWorkerAsync();
                ToFWorkerAbs.RunWorkerAsync();
                castView.UpdateReadButton(false);
                //readTimer.Start();
            }
        }
    }
}
