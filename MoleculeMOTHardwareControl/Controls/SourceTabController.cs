using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using NationalInstruments.DAQmx;
using DAQ.HAL;
using DAQ.Environment;
using System.Windows.Forms;
using NewFocus.PicomotorApp;
using Newport.DeviceIOLib;
using System.Diagnostics;

namespace MoleculeMOTHardwareControl.Controls
{
    public class SourceTabController : GenericController
    {
        private double referenceResistance = 47120; // Reference resistor for reading in temperature from source thermistor
        
        private SourceTabView castView; // Convenience to avoid lots of casting in methods 
        private AnalogSingleChannelReader sourceTempReader;
        private AnalogSingleChannelReader sf6TempReader;
        private AnalogSingleChannelReader sourcePressureReader;
        private AnalogSingleChannelReader MOTPressureReader;
        private AnalogSingleChannelReader sourceTempReader2; //4K diode
        private AnalogSingleChannelReader sourceTempReader3; //4K diode new (07/06/2023)
        private AnalogSingleChannelReader sourceTempReader40K;
        private AnalogSingleChannelReader sourceTempReader40KDiode;//40K diode (07/06/2023)
        private AnalogSingleChannelReader sf6FlowReader;
        private AnalogSingleChannelReader he6FlowReader;

        private DigitalSingleChannelWriter cryoWriter;
        private DigitalSingleChannelWriter heaterWriter;
        private DigitalSingleChannelWriter heaterWriter40K;
        private DigitalSingleChannelWriter heaterWriterMaster;
        private DigitalSingleChannelWriter heaterWriterSF6;
        private DigitalSingleChannelWriter sf6ValveWriter;
        private DigitalSingleChannelWriter heValveWriter;
        

        private bool isCycling = false;
        private bool isCycleTempReached = false;
        private bool finishedHeating = true;

        private bool isHolding = false;
        private bool is40KHolding = false;
        private bool isSF6Holding = false;
        private bool maxTempReached = true;
        private bool maxTempReached40K = true;
        private bool flowEnableFlag = false;
        private bool valveEnableFlag = false;
        private double[] tofData, tofTime;

        private bool isHeaterOn = false;
        private bool isHeaterOn40K = false;
        private bool isHeaterOnSF6 = false;
        private bool isCryoOn = false;
        private bool isHeaterOnMaster = false;
        private bool isPanic = false;

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
        private double hardTempLimInK = 300.0;
        private double hardTempLimInC = 27.0;
        private double softTempLimInK = 293.0;
        private double softTempLimInC = 20.0;

        Stopwatch cycleHoldTimer = new Stopwatch();

        CmdLib8742 cmdLib;
        DeviceIOLib diolib;
        string devKey;

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
                double[] limits = new double[]{ AO0Min, AO0Max, AO1Min, AO1Max };
                return limits;
            }
            
        }

        public bool IsCyling
        {
            get { return isCycling; }
            set { this.isCycling = value; }
        }

        public bool IsHolding
        {
            get { return isHolding; }
            set
            { this.isHolding = value; }
        }

        public bool Is40KHolding
        {
            get { return is40KHolding; }
            set
            { this.is40KHolding = value; }
        }

        public bool IsSF6Holding
        {
            get { return isSF6Holding; }
            set
            { this.isSF6Holding = value; }
        }

        public SourceTabController()
        {
            InitReadTimer();

            sourceTempReader = CreateAnalogInputReader("sourceTemp");
            sf6TempReader = CreateAnalogInputReader("sf6Temp");
            cryoWriter = CreateDigitalOutputWriter("cryoCooler");
            heaterWriter = CreateDigitalOutputWriter("sourceHeater");
            heaterWriter40K = CreateDigitalOutputWriter("sourceHeater40K");
            heaterWriterMaster = CreateDigitalOutputWriter("sourceHeaterMaster");
            sf6ValveWriter = CreateDigitalOutputWriter("sf6Valve");
            heValveWriter = CreateDigitalOutputWriter("heValve");
            heaterWriterSF6 = CreateDigitalOutputWriter("sourceHeaterSF6");

            sourcePressureReader = CreateAnalogInputReader("sourcePressure");
            MOTPressureReader = CreateAnalogInputReader("MOTPressure");
            sourceTempReader2 = CreateAnalogInputReader("sourceTemp2");
            sourceTempReader3 = CreateAnalogInputReader("sourceTemp3");
            sourceTempReader40K = CreateAnalogInputReader("sourceTemp40K");
            sourceTempReader40KDiode = CreateAnalogInputReader("sourceTemp40KDiode");
            sf6FlowReader = CreateAnalogInputReader("sf6FlowMonitor");
            he6FlowReader = CreateAnalogInputReader("he6FlowMonitor");

            sf6FlowConversion = (double)Environs.Hardware.GetInfo("flowConversionSF6");
            heFlowConversion = (double)Environs.Hardware.GetInfo("flowConversionHe");
            tof_signal_address = (string)Environs.Hardware.GetInfo("ToFPMTSignal");
            power_mon_address = (string)Environs.Hardware.GetInfo("PowerMonitorPD");
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

            //Initialize Analog Outputs to 0V 
            SetAnalogOutput(0, 0.0);
            SetAnalogOutput(1, 0.0);
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

                switch(PlotChannel)
                {
                    case 0:
                        myTask.AIChannels.CreateVoltageChannel(tof_signal_address, "", AITerminalConfiguration.Rse, 0.0, 10.0, AIVoltageUnits.Volts);
                        break;
                    case 1:
                        myTask.AIChannels.CreateVoltageChannel(power_mon_address, "", AITerminalConfiguration.Rse, 0.0, 10.0, AIVoltageUnits.Volts);
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

        protected double GetSourcePressure()
        {
            double sourcePressureVoltage = sourcePressureReader.ReadSingleSample();
            return Math.Pow(10, (sourcePressureVoltage - 6.8) / 0.6);
        }

        protected double GetMOTPressure()
        {
            double MOTPressureVoltage = MOTPressureReader.ReadSingleSample();
            return Math.Pow(10, (MOTPressureVoltage - 7.75) / 0.75);
        }

        protected double GetSourceTemperature4K()
        {
            double[] ThermocoupleVoltLst = sourceTempReader2.ReadMultiSample(500);
            double ThermocoupleVolt = ThermocoupleVoltLst.Average();
            double temperature;

            //Interpolation function (4th order polynomial) to convert voltage to temperature in K
            if (ThermocoupleVolt > 1.1)
                temperature = -2015.5 + (6350.82 * ThermocoupleVolt) - (7250.48 * Math.Pow(ThermocoupleVolt, 2)) + (3607.18 * Math.Pow(ThermocoupleVolt, 3)) - (664.69 * Math.Pow(ThermocoupleVolt, 4));
            else
                temperature = 535.7 - (396.908 * ThermocoupleVolt) - (107.416 * Math.Pow(ThermocoupleVolt, 2)) + (180.776 * Math.Pow(ThermocoupleVolt, 3)) - (119.994 * Math.Pow(ThermocoupleVolt, 4));
            return temperature;

        }

        protected double GetSourceTemperatureDiode(AnalogSingleChannelReader reader)
        {
            double[] ThermocoupleVoltLst = reader.ReadMultiSample(500);
            double ThermocoupleVolt = ThermocoupleVoltLst.Average();
            double temperature;

            //Interpolation function (4th order polynomial) to convert voltage to temperature in K
            if (ThermocoupleVolt > 1.1)
                temperature = -2015.5 + (6350.82 * ThermocoupleVolt) - (7250.48 * Math.Pow(ThermocoupleVolt, 2)) + (3607.18 * Math.Pow(ThermocoupleVolt, 3)) - (664.69 * Math.Pow(ThermocoupleVolt, 4));
            else
                temperature = 535.7 - (396.908 * ThermocoupleVolt) - (107.416 * Math.Pow(ThermocoupleVolt, 2)) + (180.776 * Math.Pow(ThermocoupleVolt, 3)) - (119.994 * Math.Pow(ThermocoupleVolt, 4));
            return temperature;

        }

        protected double GetSourceTemperature()
        {
            double vRef = 5.1; //vRefReader.ReadSingleSample();
            double sourceTempVoltage = sourceTempReader.ReadSingleSample();
            double sourceTempResistance = ConvertVoltageToResistance(sourceTempVoltage, vRef);
            return Convert10kResistanceToCelcius(sourceTempResistance);
        }

        protected double Get40KTemperature()
        {
            double vRef = 5.1; //vRefReader.ReadSingleSample();
            double sourceTempVoltage = sourceTempReader40K.ReadSingleSample();
            double sourceTempResistance = ConvertVoltageToResistance(sourceTempVoltage, vRef);
            return Convert10kResistanceToCelcius(sourceTempResistance);
        }

        protected double GetSF6Temperature()
        {
            double vRef = 5.1; //vRefReader.ReadSingleSample();
            double sf6TempVoltage = sf6TempReader.ReadSingleSample();
            //double sf6TempResistance = sf6TempVoltage * 100000;
            double sf6TempResistance = ConvertVoltageToResistance(sf6TempVoltage, vRef);
            return Convert10kResistanceToCelcius(sf6TempResistance);
        }

        protected void UpdateReadings(object anObject, EventArgs eventArgs)
        {
            lock (acquisition_lock)
            {
                double sourceTemp = GetSourceTemperature();
                double sourceTemp2 = GetSourceTemperatureDiode(sourceTempReader2);
                double sourceTemp3 = GetSourceTemperatureDiode(sourceTempReader3);
                double sourcePressure = GetSourcePressure();
                double MOTPressure = GetMOTPressure();
                double source40KTemp = Get40KTemperature();
                double source40KTemp2 = GetSourceTemperatureDiode(sourceTempReader40KDiode);
                double sf6Flow = GetSF6Flow();
                double heFlow = GetHeFlow();
                double sf6Temp = GetSF6Temperature();

                // If time of flight trigger events fails to come for 10 consecutive times, stop trying to show the traces.
                if (tof_timeout_count >= flowTimeoutCount)
                {
                    //castView.DisableTOF();

                    if (castView.AutomaticFlowControlEnabled())
                        castView.FlowDisable();

                    if (castView.AutomaticValveControlEnabled())
                        castView.ValveClose();

                    tof_timeout_count = 0;
                }

                if (MOTPressure > Math.Pow(10, -3))
                    SetCryoState(false);

                if (IsCyling)
                {
                    checkCycleStatus(sourceTemp2);
                    /*double cycleLimit = castView.GetCycleLimit() + 273.0; //Cycle temperature in K
                    if (!finishedHeating && sourceTemp2 > cycleLimit)
                    {
                        finishedHeating = true;
                        SetHeaterState(false);
                        SetCryoState(true);
                    }
                    */
                }

                if (IsHolding)
                {

                    double cycleLimit = castView.GetCycleLimit() + 273.0; //Cycle temperature in K
                    if (sourceTemp2 < cycleLimit && !maxTempReached)
                    {
                        SetHeaterState(true);
                    }
                    else if (sourceTemp2 > cycleLimit && !maxTempReached)
                    {
                        SetHeaterState(false);
                        maxTempReached = true;
                    }
                    else if (sourceTemp2 < cycleLimit - 5 && maxTempReached)
                    {
                        SetHeaterState(true);
                        maxTempReached = false;
                    }


                }

                if (Is40KHolding)
                {

                    double cycleLimit = castView.GetCycleLimit40K() + 273.0; //Cycle temperature in K
                    //double cycleLimit = castView.GetCycleLimit40K();
                    if (source40KTemp2 < cycleLimit && !maxTempReached40K)
                    {
                        SetHeaterState40K(true);
                    }
                    else if (source40KTemp2 > cycleLimit && !maxTempReached40K)
                    {
                        SetHeaterState40K(false);
                        maxTempReached40K = true;
                    }
                    else if (source40KTemp2 < cycleLimit - 5 && maxTempReached40K)
                    {
                        SetHeaterState40K(true);
                        maxTempReached40K = false;
                    }


                }

                if (IsSF6Holding)
                {

                    //double cycleLimit = castView.GetCycleLimit40K() + 273.0; //Cycle temperature in K
                    double cycleLimit = castView.GetCycleLimitSF6();
                    if (sf6Temp < cycleLimit - 1.0)
                    {
                        SetHeaterStateSF6(true);
                    }
                    else if (sf6Temp > cycleLimit )
                    {
                        SetHeaterStateSF6(false);
                    }


                }

                //Safety check
                if (!isPanic) { 
                    //limitCheck(sourceTemp, softTempLimInC, hardTempLimInC);
                    limitCheck(sourceTemp2, softTempLimInK, hardTempLimInK);
                    limitCheck(sourceTemp3, softTempLimInK, hardTempLimInK);
                    //limitCheck(source40KTemp, softTempLimInC, hardTempLimInC);
                    limitCheck(source40KTemp2, softTempLimInK, hardTempLimInK);
                }

                SetHeaterStateMaster(isHeaterOn || isHeaterOn40K);

                castView.UpdateCurrentSourceTemperature2(sourceTemp2.ToString("0.##") + " K");
                castView.UpdateCurrentSourceTemperature3(sourceTemp3.ToString("0.##") + " K");
                castView.UpdateCurrentSourceTemperature40K2(source40KTemp2.ToString("0.##") + " K");
                castView.UpdateCurrentSourcePressure(sourcePressure.ToString("E4"));
                castView.UpdateCurrentMOTPressure(MOTPressure.ToString("E4"));
                castView.UpdateFlowRates(sf6Flow, heFlow);
                castView.UpdateAnalogOutputControls(AOVoltage[0], AOVoltage[1]);

                if (sourceTemp < -34)
                {
                    castView.UpdateCurrentSourceTemperature("<-34 C");
                }
                else
                {
                    castView.UpdateCurrentSourceTemperature(sourceTemp.ToString("F2") + " C");
                }
                
                if (sf6Temp < -34)
                {
                    castView.UpdateCurrentSF6Temperature("<-34 C");
                }
                else
                {
                    castView.UpdateCurrentSF6Temperature(sf6Temp.ToString("F2") + " C");
                }

                if (source40KTemp < -34)
                {
                    castView.UpdateCurrentSourceTemperature40K("<-34 C");
                }
                else
                {
                    castView.UpdateCurrentSourceTemperature40K(source40KTemp.ToString("F2") + " C");
                }

                if (castView.LogStatus())
                {
                    DateTime dt = DateTime.Now;
                    String filename = logfilePath + "" + DateTime.Now.ToString("yyyy-MM-dd") + ".txt";
                    
                    if (!System.IO.File.Exists(filename))
                    {
                        //string header = "Time \t Source_Pressure \t MOT_Chamber_Pressure \t Source_Temperature(in K) \t SF6_Temperature(in degree C) \t 40K_Temperature(in degree C)";
                        string header = "Time \t Source_Pressure \t MOT_Chamber_Pressure \t " +
                            "Source_Temperature(in C) \t Source_Temperature_1(in K) \t Source_Temperature_2(in K) \t " +
                            "SF6_Temperature(in degree C) \t " +
                            "40K_Temperature(in degree C) \t 40K_Temperature(in degree K)";
                        using (System.IO.StreamWriter file =
                        new System.IO.StreamWriter(filename, false))
                            file.WriteLine(header);
                    }

                    using (System.IO.StreamWriter file =
                        new System.IO.StreamWriter(filename, true))
                    {
                        //file.WriteLine(dt.TimeOfDay.ToString() + "\t" + sourcePressure.ToString() + "\t" + MOTPressure.ToString()  + "\t" + sourceTemp2.ToString() + "\t" + sf6Temp.ToString() + "\t" + source40KTemp.ToString());
                        file.WriteLine(dt.TimeOfDay.ToString() + "\t" + sourcePressure.ToString() + "\t" + MOTPressure.ToString() + "\t" +
                            sourceTemp.ToString() + "\t" + sourceTemp2.ToString() + "\t" + sourceTemp3.ToString() + "\t" +
                            sf6Temp.ToString() + "\t" +
                            source40KTemp.ToString() + "\t" + source40KTemp2.ToString());
                        file.Flush();
                    }
                }


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

        public void SetCryoState(bool state) 
        {
            isCryoOn = state;
            lock (acquisition_lock)
            {
                cryoWriter.WriteSingleSampleSingleLine(true, state);
                castView.SetCryoState(state);
            }
        }

        public void SetHeaterState(bool state)
        {
            isHeaterOn = state;
            lock (acquisition_lock)
            {
                heaterWriter.WriteSingleSampleSingleLine(true, state);
                castView.SetHeaterState(state);
            }
        }

        public void SetHeaterState40K(bool state)
        {
            isHeaterOn40K = state;
            lock (acquisition_lock)
            {
                heaterWriter40K.WriteSingleSampleSingleLine(true, state);
                castView.SetHeaterState40K(state);
            }
        }

        public void SetHeaterStateSF6(bool state)
        {
            isHeaterOnSF6 = state;
            lock (acquisition_lock)
            {
                heaterWriterSF6.WriteSingleSampleSingleLine(true, state);
                castView.SetHeaterStateSF6(state);
            }
        }

        public void SetHeaterStateMaster(bool state)
        {
            isHeaterOnMaster = state;
            lock (acquisition_lock)
            {
                heaterWriterMaster.WriteSingleSampleSingleLine(true, state);
            }
        }

        public void limitCheck(double temp, double softLim, double hardLim)
        {
            if (temp >= hardLim)
            {
                panic();
            }

            if (temp >= softLim)
            {
                stopHeating();
            }
        }

        /// <summary>
        /// Check the status of the cycle, stops cycle if hold time exceed cycleHoldTime
        /// </summary>
        private void checkCycleStatus(double temp)
        {
            double cycleLimit = castView.GetCycleLimit() + 273.0; //Cycle temperature in K
            
            //First time temperature reaches limit
            if (temp > cycleLimit && !isCycleTempReached)
            {
                isCycleTempReached = true;
                cycleHoldTimer.Restart();
                SetHeaterState(false);
            }

            //Anytime temperature reaches limit
            if (temp > cycleLimit && isCycleTempReached)
            {
                SetHeaterState(false);
            }

            //If temperature drops during holding
            if (temp < cycleLimit - 1.0 && isCycleTempReached)
            {
                SetHeaterState(true);
            }

            //If hold time reaches set value
            if (isCycleTempReached && cycleHoldTimer.Elapsed.TotalMinutes > castView.getCycleHoldTime())
            {
                SetHeaterState(false);
                isCycling = false;
                SetCryoState(true);
                cycleHoldTimer.Stop();
                isCycleTempReached = false;
                castView.UpdateCycleButton(!isCycling);
            }
        }

        /// <summary>
        /// Switch off all heaters and cryo
        /// </summary>
        public void panic()
        {
            SetHeaterState(false);
            SetHeaterState40K(false);
            SetCryoState(false);
            isPanic = true;
            //DisplayMessageWindow();
            //Thread messageThread = new Thread(DisplayMessageWindow);
            //messageThread.Start();

        }

        public void stopPanic()
        {
            isPanic = false;
        }

        void DisplayMessageWindow()
        {

            DialogResult result = MessageBox.Show("Temperature too high, system panci!", "Panic" , MessageBoxButtons.OK);

            // Check the result when the message box is closed
            if (result == DialogResult.OK)
            {
                this.stopPanic();
            }

        }

        /// <summary>
        /// Switch off all heaters
        /// </summary>
        public void stopHeating()
        {
            SetHeaterState(false);
            SetHeaterState40K(false);
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
            isCycleTempReached = false;
            castView.UpdateCycleButton(!isCycling);
            if (IsCyling)
            {
                SetHeaterState(true);
                SetCryoState(false);
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

            else
                SetHeaterState(false);
        }

        public void ToggleHolding40K()
        {
            is40KHolding = !is40KHolding;
            castView.UpdateHoldButton40K(!is40KHolding);
            if (is40KHolding)
            {
                SetCryoState(false);
                double temp = Get40KTemperature();
                double cycleLimit = castView.GetCycleLimit40K();
                if (temp < cycleLimit)
                {
                    SetHeaterState40K(true);
                    maxTempReached40K = false;
                }
                else
                {
                    SetHeaterState40K(false);
                    maxTempReached40K = true;
                }
            }

            else
                SetHeaterState40K(false);
        }

        public void ToggleHoldingSF6()
        {
            isSF6Holding = !isSF6Holding;
            castView.UpdateHoldButtonSF6(!isSF6Holding);
            if (is40KHolding)
            {
                double temp = GetSF6Temperature();
                double cycleLimit = castView.GetCycleLimitSF6();
                if (temp < cycleLimit)
                {
                    SetHeaterStateSF6(true);
                }
                else
                {
                    SetHeaterStateSF6(false);
                }
            }

            else
                SetHeaterStateSF6(false);
        }

        public void SwitchOutputAOVoltage(int channel)
        {
            lock (acquisition_lock)
            {
                Task analogOutTask = new Task();
                switch (channel)
                {
                    case 0:
                        ((AnalogOutputChannel)Environs.Hardware.AnalogOutputChannels["hardwareControlAO0"]).AddToTask(analogOutTask, AO0Min, AO0Max);
                        break;
                    case 1:
                        ((AnalogOutputChannel)Environs.Hardware.AnalogOutputChannels["hardwareControlAO1"]).AddToTask(analogOutTask, AO1Min, AO1Max);
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

        public void SetAnalogOutput(int channel, double voltage)
        {
            AOVoltage[channel] = voltage;
            SwitchOutputAOVoltage(channel);
        }
        
        public void SetPlotChannel(int channelID)
        {
            PlotChannel = channelID;
        }

        #region Yag motorized morror control

        public void connectDevice()
        {
            if (devKey != null) return;
            diolib = new DeviceIOLib();
            cmdLib = new CmdLib8742(diolib);
            diolib.DiscoverDevices(1, 5000);
            devKey = diolib.GetFirstDeviceKey();
        }

        public void moveYagX1 (int step)
        {
            //try
            //{
                cmdLib.RelativeMove(devKey, 1, step);
            //}

            //catch(Exception e)
            //{

            //} 
        }

        public void moveYagY1(int step)
        {
            try
            {
                cmdLib.RelativeMove(devKey, 2, step);
            }

            catch (Exception e)
            {

            }
        }

        public void moveYagX2(int step)
        {
            try
            {
                cmdLib.RelativeMove(devKey, 3, step);
            }

            catch (Exception e)
            {

            }
        }

        public void moveYagY2(int step)
        {
            try
            {
                cmdLib.RelativeMove(devKey, 4, step);
            }

            catch (Exception e)
            {

            }
        }

        #endregion
    }
}
