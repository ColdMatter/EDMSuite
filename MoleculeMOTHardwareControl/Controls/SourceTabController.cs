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
        private AnalogSingleChannelReader sourcePressureReader;
        private AnalogSingleChannelReader sourceTempReader2;

        private DigitalSingleChannelWriter cryoWriter;
        private DigitalSingleChannelWriter heaterWriter;
        
        private bool isCycling = false;
        private bool finishedHeating = true;

        private bool isHolding = false;
        private bool maxTempReached = true;
        private double[] tofData, tofTime; 

        private System.Windows.Forms.Timer readTimer;
        
        private static string logfilePath = (string)Environs.FileSystem.Paths["SourceLogPath"];
        private static string toffilePath = (string)Environs.FileSystem.Paths["ToFFilesPath"];

        private string tof_signal_address, tof_trigger_address;
        private int tof_samplerate, tof_num_of_samples;
        
        //Runs time of flight acquisition in a different thread to prevent user interface from hanging.
        System.ComponentModel.BackgroundWorker ToFWorker;

        //lock object is used to make sure that the two threads do not try to get the handle for the DAQ module at the same time.
        private readonly object acquisition_lock = new object();

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

            sourcePressureReader = CreateAnalogInputReader("sourcePressure");
            sourceTempReader2 = CreateAnalogInputReader("sourceTemp2");
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
            for (int i = 0; i < tof_num_of_samples; i++)
                tofTime[i]=i;
        }

        private void AcquistionFinished(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            readTimer.Start();
        }

        private void AcquireTOF(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            lock (acquisition_lock)
            {
                NationalInstruments.DAQmx.Task myTask = new NationalInstruments.DAQmx.Task();
                AIChannel aiChannel;
                aiChannel = myTask.AIChannels.CreateVoltageChannel(tof_signal_address, "", AITerminalConfiguration.Rse, 0.0, 10.0, AIVoltageUnits.Volts);
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
                    castView.UpdateGraph(tofTime, tofData);
                    if (castView.SaveTraceStatus())
                    {
                        using (System.IO.StreamWriter file =
                            new System.IO.StreamWriter(toffilePath + "Tof_" + DateTime.Now.ToString("yyyy-MM-dd--HH-mm-ss-fff") + ".txt", false))
                        {
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

        protected double GetSourcePressure()
        {
            double sourcePressureVoltage = sourcePressureReader.ReadSingleSample();
            return Math.Pow(10, (sourcePressureVoltage - 6.8) / 0.6);
        }

        protected double GetSourceTemperature4K()
        {
            double sourceTemp2 = sourceTempReader2.ReadSingleSample();
            return sourceTemp2;
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
            lock (acquisition_lock)
            {
                double sourceTemp = GetSourceTemperature();
                double sourceTemp2Volt = GetSourceTemperature4K();
                double sourcePressure = GetSourcePressure();
                double sourceTemp2Temp;

                //Interpolation function (4th order polynomial) to convert voltage to temperature in K
                if (sourceTemp2Volt > 1.1)
                    sourceTemp2Temp = -2015.5 + (6350.82 * sourceTemp2Volt) - (7250.48 * Math.Pow(sourceTemp2Volt, 2)) + (3607.18 * Math.Pow(sourceTemp2Volt, 3)) - (664.69 * Math.Pow(sourceTemp2Volt, 4));
                else
                    sourceTemp2Temp = 535.7 - (396.908 * sourceTemp2Volt) - (107.416 * Math.Pow(sourceTemp2Volt, 2)) + (180.776 * Math.Pow(sourceTemp2Volt, 3)) - (119.994 * Math.Pow(sourceTemp2Volt, 4));

                castView.UpdateCurrentSourceTemperature2(sourceTemp2Temp.ToString("0.##") + " K");
                castView.UpdateCurrentSourcePressure(sourcePressure.ToString());

                if (sourceTemp < -34)
                {
                    castView.UpdateCurrentSourceTemperature("<-34 C");
                }
                else
                {
                    castView.UpdateCurrentSourceTemperature(sourceTemp.ToString("F2") + " C");
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

                if (castView.LogStatus())
                {
                    DateTime dt = DateTime.Now;
                    String filename = logfilePath + "" + DateTime.Now.ToString("yyyy-MM-dd") + ".txt";
                    
                    if (!System.IO.File.Exists(filename))
                    {
                        string header = "Time \t Source_Pressure \t Source_Temperature(in Volts) \t Source_Temperature(in K) \t SF6_Temperature(in degree C)";
                        using (System.IO.StreamWriter file =
                        new System.IO.StreamWriter(filename, false))
                            file.WriteLine(header);
                    }

                    using (System.IO.StreamWriter file =
                        new System.IO.StreamWriter(filename, true))
                    {
                        file.WriteLine(dt.TimeOfDay.ToString() + "\t" + sourcePressure.ToString() + "\t" + sourceTemp2Volt.ToString() + "\t" + sourceTemp2Temp.ToString() + "\t" + sf6Temp.ToString());
                        file.Flush();
                    }
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


                if (castView.ToFEnabled())
                {

                    //System.Threading.Thread ToFAcquireThread = new Thread(new ThreadStart(AcquireTOF));
                    //ToFAcquireThread.Start();
                    readTimer.Stop();
                    ToFWorker.RunWorkerAsync();
                    //AcquireTOF();
                }

            }
        }

        public void SetCryoState(bool state) 
        {
            lock (acquisition_lock)
            {
                cryoWriter.WriteSingleSampleSingleLine(true, state);
                castView.SetCryoState(state);
            }
        }

        public void SetHeaterState(bool state)
        {
            lock (acquisition_lock)
            {
                heaterWriter.WriteSingleSampleSingleLine(true, state);
                castView.SetHeaterState(state);
            }
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
