using System;
using System.Numerics;
using System.Globalization;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.IO;
using Microsoft.Win32;

using NationalInstruments;
using NationalInstruments.DAQmx;
using DAQ.Environment;
using DAQ.HAL;
using DSPLib;

namespace ConfocalControl
{
    
    class DFGPlugin
    {
        #region Class members

        // Dependencies should refer to this instance only 
        private static DFGPlugin controllerInstance;
        public static DFGPlugin GetController()
        {
            if (controllerInstance == null)
            {
                controllerInstance = new DFGPlugin();
            }
            return controllerInstance;
        }

        // Define Opt state
        private enum DFGState { stopped, running, stopping, stepping };
        private DFGState backendState = DFGState.stopped;
        private enum WavemeterScanState { stopped, running, stopping };
        private WavemeterScanState wavemeterState = WavemeterScanState.stopped;
        private enum TripletScanState { stopped, running, stopping };
        private TripletScanState tripletState = TripletScanState.stopped;

        private enum TeraScanState { stopped, running, stopping, stepping };
        private TeraScanState teraState = TeraScanState.stopped;
        private enum TeraScanSegmentState { stopped, running, stopping, unfinished };
        private TeraScanSegmentState teraSegmentState = TeraScanSegmentState.stopped;
        private enum TeraLaserState { stopped, running, stopping };
        private TeraLaserState teraLaser = TeraLaserState.stopped;

        // Settings
        public PluginSettings Settings { get; set; }

        // Laser
        private ICEBlocDFG dfg;
        public ICEBlocDFG DFG { get { return dfg; } }

        // Bound event managers to class
        // Wavemeter scan
        public event SpectraScanDataEventHandler WavemeterData;
        public event MultiChannelScanFinishedEventHandler WavemeterScanFinished;
        public event SpectraScanExceptionEventHandler WavemeterScanProblem;
        // Triplet search
        public event TeraSingleDataEventHandler TripletData;
        public event TeraSingleDataEventHandler TripletDFTData;
        public event MultiChannelScanFinishedEventHandler TripletScanFinished;
        public event SpectraScanExceptionEventHandler TripletScanProblem;
        // TeraScan
        public event TeraScanFullDataEventHandler TeraData;
        public event TeraScanSingleChannelDataEventHandler TeraTotalOnlyData;
        public event TeraScanSingleChannelDataEventHandler TeraSegmentOnlyData;
        public event MultiChannelScanFinishedEventHandler TeraSegmentScanFinished;
        public event MultiChannelScanFinishedEventHandler TeraScanFinished;
        public event SpectraScanExceptionEventHandler TeraScanProblem;
        // Status Messaging
        public event UpdateStatusEventHandler UpdateStatusBar;

        // Constants relating to sample acquisition
        private int MINNUMBEROFSAMPLES = 10;
        private double TRUESAMPLERATE = 1000;
        private double MAXSCANPOINTS = 0.5*Math.Pow(10,5);
        private int pointsPerExposure;
        private double sampleRate;

        // Keep track of data for wavemeter scan
        private List<Point>[] wavemeterAnalogBuffer;
        private List<Point>[] wavemeterCounterBuffer;
        public Hashtable wavemeterHistoricSettings;

        // Keep track of data for triplet search
        private string tripletFolder;
        private double[,] tripletAnalogBuffer;
        private List<double[]> tripletCounterBuffer;
        public Hashtable tripletHistoricSettings;

        // Keep track of data for teraScan
        private List<Point>[] fastAnalogBuffer;
        private List<Point>[] fastCounterBuffer;
        private double[] fastLatestCounters;
        private double[] teraScanDisplayTotalWaveform;
        private double[] teraScanDisplaySegmentWaveform;
        private bool teraScan_display_is_current_segment;
        private string teraScan_current_channel_type;
        private int teraScan_current_display_channel_index;
        private TeraScanDataHolder teraScanBuffer;
        public TeraScanDataHolder teraScanBufferAccess { get { return teraScanBuffer; } }
        private double teraLatestLambda;
        public double latestLambda { get { return teraLatestLambda; } }

        // Keep track of tasks
        private Task triggerTask;
        private DigitalSingleChannelWriter triggerWriter;
        private Task freqOutTask;
        private List<Task> counterTasks;
        private List<CounterSingleChannelReader> counterReaders;
        private Task analoguesTask;
        private AnalogMultiChannelReader analoguesReader;

        double SPEEDOFLIGHT = 299792458;

        #endregion

        #region Initialization

        public void LoadSettings()
        {
            Settings = PluginSaveLoad.LoadSettings("DFG");
        }

        public DFGPlugin()
        {
            dfg = new ICEBlocDFG();

            LoadSettings();
            if (Settings.Keys.Count != 27)
            {
                Settings["wavelength"] = 1200.0;

                Settings["wavemeterScanStart"] = 1200.0;
                Settings["wavemeterScanStop"] = 1300.0;
                Settings["wavemeterScanPoints"] = 101;

                Settings["counterChannels"] = new List<string> { "APD0" };
                Settings["analogueChannels"] = new List<string> { };
                Settings["analogueLowHighs"] = new Dictionary<string, double[]>();

                Settings["wavemeter_channel_type"] = "Counters";
                Settings["wavemeter_display_channel_index"] = 0;

                Settings["tripletStart"] = 1200.0;
                Settings["tripletStop"] = 1300.0;
                Settings["tripletScanPoints"] = 101;
                Settings["tripletInt"] = 2.0;
                Settings["tripletRate"] = 10000.0;

                Settings["TeraScanRepeatType"] = "single";
                Settings["TeraScanType"] = "medium";
                Settings["TeraScanStart"] = (double)1150;
                Settings["TeraScanStop"] = (double)1155;
                Settings["TeraScanRate"] = 500;
                Settings["TeraScanUnits"] = "MHz/s";
                Settings["TeraScanMultiLambda"] = new double[] { 1150, 1160, 1170, 1180, 1190 };
                Settings["TeraScanMultiRange"] = 10.0;
                Settings["tera_channel_type"] = "Lambda";
                Settings["tera_display_channel_index"] = 0;
                Settings["tera_display_current_segment"] = true;

                Settings["triplet_channel_type"] = "Counters";
                Settings["triplet_display_channel_index"] = 0;
            }

            wavemeterHistoricSettings = new Hashtable();
            wavemeterHistoricSettings["counterChannels"] = (List<string>)Settings["counterChannels"];
            wavemeterHistoricSettings["analogueChannels"] = (List<string>)Settings["analogueChannels"];

            tripletHistoricSettings = new Hashtable();
            tripletHistoricSettings["counterChannels"] = (List<string>)Settings["counterChannels"];
            tripletHistoricSettings["analogueChannels"] = (List<string>)Settings["analogueChannels"];

            teraScanBuffer = new TeraScanDataHolder(((List<string>)Settings["counterChannels"]).Count, ((List<string>)Settings["analogueChannels"]).Count, Settings);

            triggerTask = null;
            freqOutTask = null;
            counterTasks = null;
            analoguesTask = null;

            triggerWriter = null;
            counterReaders = null;
            analoguesReader = null;
        }

        #endregion

        #region Shared functions

        public bool IsRunning()
        {
            return backendState == DFGState.running;
        }

        public bool TeraLaserIsRunning()
        {
            return teraLaser == TeraLaserState.running;
        }

        public bool WavemeterScanIsRunning()
        {
            return wavemeterState == WavemeterScanState.running;
        }

        public bool TripletScanIsRunning()
        {
            return tripletState == TripletScanState.running;
        }

        public bool TeraScanIsRunning()
        {
            return teraState == TeraScanState.running;
        }

        public bool TeraSegmentIsRunning()
        {
            return teraSegmentState == TeraScanSegmentState.running;
        }

        private bool CheckIfStopping()
        {
            return backendState == DFGState.stopping;
        }

        public void StopAcquisition()
        {
            backendState = DFGState.stopping;
        }

        private int SetAndLockWavelength(double wavelength)
        {
            return dfg.wavelength("mir", wavelength, true);
        }

        #endregion

        #region Wavemeter Scan

        private void CalculateParameters()
        {
            double _sampleRate = (double)TimeTracePlugin.GetController().Settings["sampleRate"];
            if (_sampleRate * MINNUMBEROFSAMPLES >= TRUESAMPLERATE)
            {
                pointsPerExposure = MINNUMBEROFSAMPLES;
                sampleRate = _sampleRate * pointsPerExposure;
            }
            else
            {
                pointsPerExposure = Convert.ToInt32(TRUESAMPLERATE / _sampleRate);
                sampleRate = _sampleRate * pointsPerExposure;
            }
        }

        public bool WavemeterAcceptableSettings()
        {
            if ((double)Settings["wavemeterScanStart"] >= (double)Settings["wavemeterScanStop"] || (int)Settings["wavemeterScanPoints"] < 1)
            {
                MessageBox.Show("Wavemeter scan settings unacceptable.");
                return false;
            }
            else
            {
                return true;
            }
        }

        public void WavemeterSynchronousStartScan()
        {
            try
            {
                if (IsRunning() || TimeTracePlugin.GetController().IsRunning() || FastMultiChannelRasterScan.GetController().IsRunning() || CounterOptimizationPlugin.GetController().IsRunning() || SolsTiSPlugin.GetController().IsRunning())
                {
                    throw new DaqException("Daq already running");
                }

                wavemeterState = WavemeterScanState.running;
                backendState = DFGState.running;
                wavemeterHistoricSettings["wavemeterScanStart"] = (double)Settings["wavemeterScanStart"];
                wavemeterHistoricSettings["wavemeterScanStop"] = (double)Settings["wavemeterScanStop"];
                wavemeterHistoricSettings["wavemeterScanPoints"] = (int)Settings["wavemeterScanPoints"];
                wavemeterHistoricSettings["exposure"] = TimeTracePlugin.GetController().GetExposure();
                wavemeterHistoricSettings["counterChannels"] = (List<string>)Settings["counterChannels"];
                wavemeterHistoricSettings["analogueChannels"] = (List<string>)Settings["analogueChannels"];
                wavemeterHistoricSettings["analogueLowHighs"] = (Dictionary<string, double[]>)Settings["analogueLowHighs"];
                WavemeterSynchronousAcquisitionStarting();
                WavemeterSynchronousAcquire();
            }
            catch (Exception e)
            {
                if (WavemeterScanProblem != null) WavemeterScanProblem(e);
            }
        }

        private void WavemeterSynchronousAcquisitionStarting()
        {
            wavemeterAnalogBuffer = new List<Point>[((List<string>)wavemeterHistoricSettings["analogueChannels"]).Count];
            for (int i = 0; i < ((List<string>)wavemeterHistoricSettings["analogueChannels"]).Count; i++)
            {
                wavemeterAnalogBuffer[i] = new List<Point>();
            }
            wavemeterCounterBuffer = new List<Point>[((List<string>)wavemeterHistoricSettings["counterChannels"]).Count];
            for (int i = 0; i < ((List<string>)wavemeterHistoricSettings["counterChannels"]).Count; i++)
            {
                wavemeterCounterBuffer[i] = new List<Point>();
            }

            // Define sample rate 
            CalculateParameters();

            // Set up trigger task
            triggerTask = new Task("pause trigger task");

            // Digital output
            ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["StartTrigger"]).AddToTask(triggerTask);

            triggerTask.Control(TaskAction.Verify);

            DaqStream triggerStream = triggerTask.Stream;
            triggerWriter = new DigitalSingleChannelWriter(triggerStream);

            triggerWriter.WriteSingleSampleSingleLine(true, false);

            // Set up clock task
            freqOutTask = new Task("sample clock task");

            // Finite pulse train
            freqOutTask.COChannels.CreatePulseChannelFrequency(
                ((CounterChannel)Environs.Hardware.CounterChannels["SampleClock"]).PhysicalChannel,
                "photon counter clocking signal",
                COPulseFrequencyUnits.Hertz,
                COPulseIdleState.Low,
                0,
                sampleRate,
                0.5);

            freqOutTask.Triggers.StartTrigger.ConfigureDigitalEdgeTrigger(
                (string)Environs.Hardware.GetInfo("StartTriggerReader"),
                DigitalEdgeStartTriggerEdge.Rising);

            freqOutTask.Triggers.StartTrigger.Retriggerable = true;


            freqOutTask.Timing.ConfigureImplicit(SampleQuantityMode.FiniteSamples, pointsPerExposure + 1);

            freqOutTask.Control(TaskAction.Verify);

            freqOutTask.Start();

            // Set up edge-counting tasks
            counterTasks = new List<Task>();
            counterReaders = new List<CounterSingleChannelReader>();

            for (int i = 0; i < ((List<string>)wavemeterHistoricSettings["counterChannels"]).Count; i++)
            {
                string channelName = ((List<string>)wavemeterHistoricSettings["counterChannels"])[i];

                counterTasks.Add(new Task("buffered edge counters " + channelName));

                // Count upwards on rising edges starting from zero
                counterTasks[i].CIChannels.CreateCountEdgesChannel(
                    ((CounterChannel)Environs.Hardware.CounterChannels[channelName]).PhysicalChannel,
                    "edge counter " + channelName,
                    CICountEdgesActiveEdge.Rising,
                    0,
                    CICountEdgesCountDirection.Up);

                // Take one sample within a window determined by sample rate using clock task
                counterTasks[i].Timing.ConfigureSampleClock(
                    (string)Environs.Hardware.GetInfo("SampleClockReader"),
                    sampleRate,
                    SampleClockActiveEdge.Rising,
                    SampleQuantityMode.ContinuousSamples);

                counterTasks[i].Control(TaskAction.Verify);

                DaqStream counterStream = counterTasks[i].Stream;
                counterReaders.Add(new CounterSingleChannelReader(counterStream));

                // Start tasks
                counterTasks[i].Start();
            }

            // Set up analogue sampling tasks
            analoguesTask = new Task("analogue sampler");

            for (int i = 0; i < ((List<string>)wavemeterHistoricSettings["analogueChannels"]).Count; i++)
            {
                string channelName = ((List<string>)wavemeterHistoricSettings["analogueChannels"])[i];

                double inputRangeLow = ((Dictionary<string, double[]>)wavemeterHistoricSettings["analogueLowHighs"])[channelName][0];
                double inputRangeHigh = ((Dictionary<string, double[]>)wavemeterHistoricSettings["analogueLowHighs"])[channelName][1];

                ((AnalogInputChannel)Environs.Hardware.AnalogInputChannels[channelName]).AddToTask(
                    analoguesTask,
                    inputRangeLow,
                    inputRangeHigh
                    );
            }

            if (((List<string>)wavemeterHistoricSettings["analogueChannels"]).Count != 0)
            {
                analoguesTask.Timing.ConfigureSampleClock(
                    (string)Environs.Hardware.GetInfo("SampleClockReader"),
                    sampleRate,
                    SampleClockActiveEdge.Rising,
                    SampleQuantityMode.ContinuousSamples);

                analoguesTask.Control(TaskAction.Verify);

                DaqStream analogStream = analoguesTask.Stream;
                analoguesReader = new AnalogMultiChannelReader(analogStream);

                // Start tasks
                analoguesTask.Start();
            }
        }

        private void WavemeterSynchronousAcquire()
        // Main method for looping over scan parameters, aquiring scan outputs and connecting to controller for display
        {
            // Go to start of scan
            int report = SetAndLockWavelength((double)wavemeterHistoricSettings["wavemeterScanStart"]);
            if (report == 1) throw new Exception("set_wave_m start: task failed");

            // Main loop
            for (double i = 0;
                    i < (int)wavemeterHistoricSettings["wavemeterScanPoints"];
                    i++)
            {
                double currentWavelength = (double)wavemeterHistoricSettings["wavemeterScanStart"] + i *
                    ((double)wavemeterHistoricSettings["wavemeterScanStop"] - (double)wavemeterHistoricSettings["wavemeterScanStart"]) /
                    ((int)wavemeterHistoricSettings["wavemeterScanPoints"] - 1);

                report = SetAndLockWavelength(currentWavelength);

                if (report == 0)
                {
                    // Start trigger task
                    triggerWriter.WriteSingleSampleSingleLine(true, true);

                    // Read counter data
                    List<double[]> counterLatestData = new List<double[]>();
                    foreach (CounterSingleChannelReader counterReader in counterReaders)
                    {
                        counterLatestData.Add(counterReader.ReadMultiSampleDouble(pointsPerExposure + 1));
                    }

                    // Read analogue data
                    double[,] analogLatestData = null;
                    if (((List<string>)wavemeterHistoricSettings["analogueChannels"]).Count != 0)
                    {
                        analogLatestData = analoguesReader.ReadMultiSample(pointsPerExposure + 1);
                    }

                    // re-init the trigger 
                    triggerWriter.WriteSingleSampleSingleLine(true, false);

                    // Store counter data
                    for (int j = 0; j < counterLatestData.Count; j++)
                    {
                        double[] latestData = counterLatestData[j];
                        double data = latestData[latestData.Length - 1] - latestData[0];
                        Point pnt = new Point(currentWavelength, data);
                        wavemeterCounterBuffer[j].Add(pnt);
                    }

                    // Store analogue data
                    if (((List<string>)wavemeterHistoricSettings["analogueChannels"]).Count != 0)
                    {
                        for (int j = 0; j < analogLatestData.GetLength(0); j++)
                        {
                            double sum = 0;
                            for (int k = 0; k < analogLatestData.GetLength(1) - 1; k++)
                            {
                                sum = sum + analogLatestData[j, k];
                            }
                            double average = sum / (analogLatestData.GetLength(1) - 1);
                            Point pnt = new Point(currentWavelength, average);
                            wavemeterAnalogBuffer[j].Add(pnt);
                        }
                    }

                    WavemeterOnData();

                    // Check if scan exit.
                    if (CheckIfStopping())
                    {
                        wavemeterState = WavemeterScanState.stopping;
                        report = SetAndLockWavelength((double)wavemeterHistoricSettings["wavemeterScanStart"]);
                        if (report == 1) throw new Exception("set_wave_m end: task failed");
                        // Quit plugins
                        WavemeterAcquisitionFinishing();
                        WavemeterOnScanFinished();
                        return;
                    }
                }
            }

            WavemeterAcquisitionFinishing();
            WavemeterOnScanFinished();
        }

        public void WavemeterAcquisitionFinishing()
        {
            triggerTask.Dispose();
            freqOutTask.Dispose();
            foreach (Task counterTask in counterTasks)
            {
                counterTask.Dispose();
            }
            analoguesTask.Dispose();

            triggerTask = null;
            freqOutTask = null;
            counterTasks = null;
            analoguesTask = null;

            triggerWriter = null;
            counterReaders = null;
            analoguesReader = null;

            wavemeterState = WavemeterScanState.stopped;
            backendState = DFGState.stopped;
        }

        private void WavemeterOnData()
        {
            switch ((string)Settings["wavemeter_channel_type"])
            {
                case "Counters":
                    if (WavemeterData != null)
                    {
                        if ((int)Settings["wavemeter_display_channel_index"] >= 0 && (int)Settings["wavemeter_display_channel_index"] < ((List<string>)wavemeterHistoricSettings["counterChannels"]).Count)
                        {
                            WavemeterData(wavemeterCounterBuffer[(int)Settings["wavemeter_display_channel_index"]].ToArray());
                        }
                        else WavemeterData(null);
                    }
                    break;

                case "Analogues":
                    if (WavemeterData != null)
                    {
                        if ((int)Settings["wavemeter_display_channel_index"] >= 0 && (int)Settings["wavemeter_display_channel_index"] < ((List<string>)wavemeterHistoricSettings["analogueChannels"]).Count)
                        {
                            WavemeterData(wavemeterAnalogBuffer[(int)Settings["wavemeter_display_channel_index"]].ToArray());
                        }
                        else WavemeterData(null);
                    }
                    break;

                default:
                    throw new Exception("Did not understand data type");
            }
        }

        private void WavemeterOnScanFinished()
        {
            if (WavemeterScanFinished != null) WavemeterScanFinished();
        }

        public void RequestWavemeterHistoricData()
        {
            if (wavemeterAnalogBuffer != null && wavemeterCounterBuffer != null) WavemeterOnData();
        }

        public void SaveWavemeterData(string fileName, bool automatic)
        {
            string directory = Environs.FileSystem.GetDataDirectory((string)Environs.FileSystem.Paths["scanMasterDataPath"]);

            List<string> lines = new List<string>();
            lines.Add(DateTime.Today.ToString("dd-MM-yyyy") + " " + DateTime.Now.ToString("HH:mm:ss"));
            lines.Add("Exposure = " + ((double)wavemeterHistoricSettings["exposure"]).ToString());
            lines.Add("Lambda start = " + ((double)wavemeterHistoricSettings["wavemeterScanStart"]).ToString() + ", Lambda stop = " + ((double)wavemeterHistoricSettings["wavemeterScanStop"]).ToString() + ", Lambda resolution = " + ((int)wavemeterHistoricSettings["wavemeterScanPoints"]).ToString());

            string descriptionString = "Lambda ";
            foreach (string channel in (List<string>)wavemeterHistoricSettings["counterChannels"])
            {
                descriptionString = descriptionString + channel + " ";
            }
            foreach (string channel in (List<string>)wavemeterHistoricSettings["analogueChannels"])
            {
                descriptionString = descriptionString + channel + " ";
            }
            lines.Add(descriptionString);

            for (int i = 0; i < wavemeterCounterBuffer[0].Count; i++)
            {
                string line = wavemeterCounterBuffer[0][i].X.ToString() + " ";
                foreach (List<Point> counterData in wavemeterCounterBuffer)
                {
                    line = line + counterData[i].Y.ToString() + " ";
                }
                foreach (List<Point> analogData in wavemeterAnalogBuffer)
                {
                    line = line + analogData[i].Y.ToString() + " ";
                }
                lines.Add(line);
            }

            if (automatic) System.IO.File.WriteAllLines(directory + fileName, lines.ToArray());
            else
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.InitialDirectory = directory;
                saveFileDialog.FileName = fileName;

                if (saveFileDialog.ShowDialog() == true)
                {
                    System.IO.File.WriteAllLines(saveFileDialog.FileName, lines.ToArray());
                }
            }
        }

        #endregion

        #region Triplet Scan

        public bool TripletAcceptableSettings()
        {
            if ((double)Settings["tripletStart"] >= (double)Settings["tripletStop"] || (int)Settings["tripletScanPoints"] < 1 || (double)Settings["tripletInt"] <= 0 || (double)Settings["tripletRate"] <= 0)
            {
                MessageBox.Show("Triplet scan settings unacceptable.");
                return false;
            }
            else
            {
                return true;
            }
        }

        public void TripletStartScan()
        {
            try
            {
                if (IsRunning() || TimeTracePlugin.GetController().IsRunning() || FastMultiChannelRasterScan.GetController().IsRunning() || CounterOptimizationPlugin.GetController().IsRunning() || SolsTiSPlugin.GetController().IsRunning())
                {
                    throw new DaqException("Daq already running");
                }

                tripletState = TripletScanState.running;
                backendState = DFGState.running;

                tripletHistoricSettings["tripletStart"] = (double)Settings["tripletStart"];
                tripletHistoricSettings["tripletStop"] = (double)Settings["tripletStop"];
                tripletHistoricSettings["tripletScanPoints"] = (int)Settings["tripletScanPoints"];
                tripletHistoricSettings["tripletInt"] = (double)Settings["tripletInt"];
                tripletHistoricSettings["tripletRate"] = (double)Settings["tripletRate"];
                tripletHistoricSettings["counterChannels"] = (List<string>)Settings["counterChannels"];
                tripletHistoricSettings["analogueChannels"] = (List<string>)Settings["analogueChannels"];
                tripletHistoricSettings["analogueLowHighs"] = (Dictionary<string, double[]>)Settings["analogueLowHighs"];

                string directory = Environs.FileSystem.GetDataDirectory((string)Environs.FileSystem.Paths["scanMasterDataPath"]);
                String hour = DateTime.Now.ToString("HH", DateTimeFormatInfo.InvariantInfo);
                String minutes = DateTime.Now.ToString("mm", DateTimeFormatInfo.InvariantInfo);
                String seconds = DateTime.Now.ToString("ss", DateTimeFormatInfo.InvariantInfo);
                tripletFolder = directory + hour + "-" + minutes + "-" + seconds + "\\";
                if (!Directory.Exists(tripletFolder)) Directory.CreateDirectory(tripletFolder);

                TripletAcquisitionStarting();
                TripletAcquire();
            }
            catch (Exception e)
            {
                if (TripletScanProblem != null) TripletScanProblem(e);
            }
        }

        private void TripletAcquisitionStarting()
        {
            pointsPerExposure = Convert.ToInt32((double)tripletHistoricSettings["tripletRate"] * (double)tripletHistoricSettings["tripletInt"]);
            sampleRate = (double)tripletHistoricSettings["tripletRate"];

            // Set up trigger task
            triggerTask = new Task("pause trigger task");

            // Digital output
            ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["StartTrigger"]).AddToTask(triggerTask);

            triggerTask.Control(TaskAction.Verify);

            DaqStream triggerStream = triggerTask.Stream;
            triggerWriter = new DigitalSingleChannelWriter(triggerStream);

            triggerWriter.WriteSingleSampleSingleLine(true, false);

            // Set up clock task
            freqOutTask = new Task("sample clock task");

            // Finite pulse train
            freqOutTask.COChannels.CreatePulseChannelFrequency(
                ((CounterChannel)Environs.Hardware.CounterChannels["SampleClock"]).PhysicalChannel,
                "photon counter clocking signal",
                COPulseFrequencyUnits.Hertz,
                COPulseIdleState.Low,
                0,
                sampleRate,
                0.5);

            freqOutTask.Triggers.StartTrigger.ConfigureDigitalEdgeTrigger(
                (string)Environs.Hardware.GetInfo("StartTriggerReader"),
                DigitalEdgeStartTriggerEdge.Rising);

            freqOutTask.Triggers.StartTrigger.Retriggerable = true;


            freqOutTask.Timing.ConfigureImplicit(SampleQuantityMode.FiniteSamples, pointsPerExposure + 1);

            freqOutTask.Control(TaskAction.Verify);

            freqOutTask.Start();

            // Set up edge-counting tasks
            Task counterTask;
            counterTasks = new List<Task>();
            counterReaders = new List<CounterSingleChannelReader>();

            for (int i = 0; i < ((List<string>)tripletHistoricSettings["counterChannels"]).Count; i++)
            {
                string channelName = ((List<string>)tripletHistoricSettings["counterChannels"])[i];

                counterTask = new Task("buffered edge counters " + channelName);
                counterTask.Stream.Timeout = Convert.ToInt32((double)tripletHistoricSettings["tripletInt"] * 2000);
                counterTasks.Add(counterTask);

                // Count upwards on rising edges starting from zero
                counterTasks[i].CIChannels.CreateCountEdgesChannel(
                    ((CounterChannel)Environs.Hardware.CounterChannels[channelName]).PhysicalChannel,
                    "edge counter " + channelName,
                    CICountEdgesActiveEdge.Rising,
                    0,
                    CICountEdgesCountDirection.Up);

                // Take one sample within a window determined by sample rate using clock task
                counterTasks[i].Timing.ConfigureSampleClock(
                    (string)Environs.Hardware.GetInfo("SampleClockReader"),
                    sampleRate,
                    SampleClockActiveEdge.Rising,
                    SampleQuantityMode.ContinuousSamples);

                counterTasks[i].Control(TaskAction.Verify);

                DaqStream counterStream = counterTasks[i].Stream;
                counterReaders.Add(new CounterSingleChannelReader(counterStream));

                // Start tasks
                counterTasks[i].Start();
            }

            // Set up analogue sampling tasks
            analoguesTask = new Task("analogue sampler");
            analoguesTask.Stream.Timeout = Convert.ToInt32((double)tripletHistoricSettings["tripletInt"] * 2000);

            for (int i = 0; i < ((List<string>)tripletHistoricSettings["analogueChannels"]).Count; i++)
            {
                string channelName = ((List<string>)tripletHistoricSettings["analogueChannels"])[i];

                double inputRangeLow = ((Dictionary<string, double[]>)tripletHistoricSettings["analogueLowHighs"])[channelName][0];
                double inputRangeHigh = ((Dictionary<string, double[]>)tripletHistoricSettings["analogueLowHighs"])[channelName][1];

                ((AnalogInputChannel)Environs.Hardware.AnalogInputChannels[channelName]).AddToTask(
                    analoguesTask,
                    inputRangeLow,
                    inputRangeHigh
                    );
            }

            if (((List<string>)tripletHistoricSettings["analogueChannels"]).Count != 0)
            {
                analoguesTask.Timing.ConfigureSampleClock(
                    (string)Environs.Hardware.GetInfo("SampleClockReader"),
                    sampleRate,
                    SampleClockActiveEdge.Rising,
                    SampleQuantityMode.ContinuousSamples);

                analoguesTask.Control(TaskAction.Verify);

                DaqStream analogStream = analoguesTask.Stream;
                analoguesReader = new AnalogMultiChannelReader(analogStream);

                // Start tasks
                analoguesTask.Start();
            }
        }

        private void TripletAcquire()
        // Main method for looping over scan parameters, aquiring scan outputs and connecting to controller for display
        {
            // Go to start of scan
            int report = SetAndLockWavelength((double)tripletHistoricSettings["tripletStart"]);
            if (report == 1) throw new Exception("set_wave_m start: task failed");

            // Main loop
            for (double i = 0;
                    i < (int)tripletHistoricSettings["tripletScanPoints"];
                    i++)
            {
                double currentWavelength = (double)tripletHistoricSettings["tripletStart"] + i *
                    ((double)tripletHistoricSettings["tripletStop"] - (double)tripletHistoricSettings["tripletStart"]) /
                    ((int)tripletHistoricSettings["tripletScanPoints"] - 1);

                report = SetAndLockWavelength(currentWavelength);

                if (report == 0)
                {
                    // Start trigger task
                    triggerWriter.WriteSingleSampleSingleLine(true, true);

                    if (pointsPerExposure <= 5000)
                    {
                        // Read counter data
                        tripletCounterBuffer = new List<double[]>();
                        double[] fooCounter;
                        double[] latestTripletCounterData;
                        foreach (CounterSingleChannelReader counterReader in counterReaders)
                        {
                            fooCounter = counterReader.ReadMultiSampleDouble(pointsPerExposure + 1);
                            latestTripletCounterData = new double[fooCounter.Length - 1];
                            for (int j = 0; j < latestTripletCounterData.Length; j++)
                            {
                                latestTripletCounterData[j] = fooCounter[j + 1] - fooCounter[j];
                            }
                            tripletCounterBuffer.Add(latestTripletCounterData);
                        }

                        // Read analogue data
                        if (((List<string>)tripletHistoricSettings["analogueChannels"]).Count != 0)
                        {
                            tripletAnalogBuffer = analoguesReader.ReadMultiSample(pointsPerExposure);
                            analoguesReader.ReadMultiSample(1);
                        }
                    }

                    else
                    {
                        // Set up stores
                        tripletCounterBuffer = new List<double[]>();
                        double[] fooCounter;
                        foreach (CounterSingleChannelReader counterReader in counterReaders)
                        {
                            tripletCounterBuffer.Add(new double[pointsPerExposure]);
                        }
                        tripletAnalogBuffer = new double[((List<string>)tripletHistoricSettings["analogueChannels"]).Count, pointsPerExposure];
                        double[,] fooAnalogue;

                        int remainder = pointsPerExposure % 1000;
                        int quotient = pointsPerExposure / 1000;
                        int shift = 0;
                        for (int m = 0; m < quotient; m++)
                        {
                            // Read counter data
                            for (int n = 0; n < counterReaders.Count; n++)
                            {
                                if (m == 0)
                                {
                                    fooCounter = counterReaders[n].ReadMultiSampleDouble(1000 + 1);
                                    for (int j = 0; j < (fooCounter.Length - 1); j++)
                                    {
                                        tripletCounterBuffer[n][shift + j] = fooCounter[j + 1] - fooCounter[j];
                                    }
                                }
                                else
                                {
                                    fooCounter = counterReaders[n].ReadMultiSampleDouble(1000);
                                    for (int j = 0; j < fooCounter.Length; j++)
                                    {
                                        if (j == 0)
                                        {
                                            tripletCounterBuffer[n][shift] = fooCounter[0] - tripletCounterBuffer[n][shift - 1];
                                        }
                                        else
                                        {
                                            tripletCounterBuffer[n][shift + j] = fooCounter[j] - fooCounter[j - 1];
                                        }
                                    }
                                }
                            }

                            // Read analogue data
                            if (((List<string>)tripletHistoricSettings["analogueChannels"]).Count != 0)
                            {
                                fooAnalogue = analoguesReader.ReadMultiSample(1000);

                                for (int n = 0; n < fooAnalogue.GetLength(0); n++)
                                {
                                    for (int j = 0; j < fooAnalogue.GetLength(1); j++)
                                    {
                                        tripletAnalogBuffer[n, shift + j] = fooAnalogue[n, j];
                                    }
                                }
                            }

                            shift += 1000;
                        }

                        // Read remainder counter data
                        for (int n = 0; n < counterReaders.Count; n++)
                        {
                            fooCounter = counterReaders[n].ReadMultiSampleDouble(remainder);
                            for (int j = 0; j < fooCounter.Length; j++)
                            {
                                if (j == 0)
                                {
                                    tripletCounterBuffer[n][shift] = fooCounter[0] - tripletCounterBuffer[n][shift - 1];
                                }
                                else
                                {
                                    tripletCounterBuffer[n][shift + j] = fooCounter[j] - fooCounter[j - 1];
                                }
                            }
                        }

                        // Read remainder analogue data
                        if (((List<string>)tripletHistoricSettings["analogueChannels"]).Count != 0)
                        {
                            fooAnalogue = analoguesReader.ReadMultiSample(remainder);

                            for (int n = 0; n < fooAnalogue.GetLength(0); n++)
                            {
                                for (int j = 0; j < fooAnalogue.GetLength(1); j++)
                                {
                                    tripletAnalogBuffer[n, shift + j] = fooAnalogue[n, j];
                                }
                            }
                            analoguesReader.ReadMultiSample(1);
                        }
                    }

                    // re-init the trigger 
                    triggerWriter.WriteSingleSampleSingleLine(true, false);

                    // Broadcast
                    TripletOnData();

                    // Save data
                    TripletSaveData(currentWavelength);

                    // Check if scan exit.
                    if (CheckIfStopping())
                    {
                        tripletState = TripletScanState.stopping;
                        report = SetAndLockWavelength((double)Settings["tripletStart"]);
                        if (report == 1) throw new Exception("set_wave_m end: task failed");
                        // Quit plugins
                        TripletAcquisitionFinishing();
                        TripletOnScanFinished();
                        return;
                    }
                }
            }

            TripletAcquisitionFinishing();
            TripletOnScanFinished();
        }

        public void TripletAcquisitionFinishing()
        {
            triggerTask.Dispose();
            freqOutTask.Dispose();
            foreach (Task counterTask in counterTasks)
            {
                counterTask.Dispose();
            }
            analoguesTask.Dispose();

            triggerTask = null;
            freqOutTask = null;
            counterTasks = null;
            analoguesTask = null;

            triggerWriter = null;
            counterReaders = null;
            analoguesReader = null;

            tripletState = TripletScanState.stopped;
            backendState = DFGState.stopped;
        }

        public void RequestTripletHistoricData()
        {
            if (tripletAnalogBuffer != null && tripletCounterBuffer != null) TripletOnData();
        }

        private void TripletOnData()
        {
            switch ((string)Settings["triplet_channel_type"])
            {
                case "Counters":
                    if (TripletData != null)
                    {
                        if ((int)Settings["triplet_display_channel_index"] >= 0 && (int)Settings["triplet_display_channel_index"] < ((List<string>)tripletHistoricSettings["counterChannels"]).Count)
                        {
                            double[] selectedData = tripletCounterBuffer[(int)Settings["triplet_display_channel_index"]].ToArray();

                            TripletData(selectedData);

                            UInt32 length = (uint)selectedData.Length;
                            DFT dft = new DFT();
                            dft.Initialize(length);
                            Complex[] cSpectrum = dft.Execute(selectedData);
                            double[] lmSpectrum = DSP.ConvertComplex.ToMagnitude(cSpectrum);
                            TripletDFTData(lmSpectrum);
                        }
                        else TripletData(null);
                    }
                    break;

                case "Analogues":
                    if (TripletData != null)
                    {
                        if ((int)Settings["triplet_display_channel_index"] >= 0 && (int)Settings["triplet_display_channel_index"] < ((List<string>)tripletHistoricSettings["analogueChannels"]).Count)
                        {
                            int row_size = tripletAnalogBuffer.GetLength(1);
                            double[] selectedData = tripletAnalogBuffer.Cast<double>().Skip((int)Settings["triplet_display_channel_index"] * row_size).Take(row_size).ToArray();

                            TripletData(selectedData);

                            UInt32 length = (uint)selectedData.Length;
                            DFT dft = new DFT();
                            dft.Initialize(length);
                            Complex[] cSpectrum = dft.Execute(selectedData);
                            double[] lmSpectrum = DSP.ConvertComplex.ToMagnitude(cSpectrum);
                            TripletDFTData(lmSpectrum);
                        }
                        else TripletData(null);
                    }
                    break;

                default:
                    throw new Exception("Did not understand data type");
            }
        }

        private void TripletSaveData(double currentWavelength)
        {
            List<string> lines = new List<string>();
            lines.Add(DateTime.Today.ToString("dd-MM-yyyy") + " " + DateTime.Now.ToString("HH:mm:ss"));
            lines.Add("Rate = " + ((double)tripletHistoricSettings["tripletRate"]).ToString() + ", Int = " + ((double)tripletHistoricSettings["tripletInt"]).ToString());
            lines.Add("Lambda start = " + ((double)tripletHistoricSettings["tripletStart"]).ToString() + ", Lambda stop = " + ((double)tripletHistoricSettings["tripletStop"]).ToString() + ", Lambda resolution = " + ((int)tripletHistoricSettings["tripletScanPoints"]).ToString());
            lines.Add("Current lambda = " + currentWavelength.ToString());

            string descriptionString = "";
            foreach (string channel in (List<string>)tripletHistoricSettings["counterChannels"])
            {
                descriptionString = descriptionString + channel + " ";
            }
            foreach (string channel in (List<string>)tripletHistoricSettings["analogueChannels"])
            {
                descriptionString = descriptionString + channel + " ";
            }
            lines.Add(descriptionString);

            for (int i = 0; i < tripletCounterBuffer[0].Length; i++)
            {
                string line = "";
                foreach (double[] counterData in tripletCounterBuffer)
                {
                    line = line + counterData[i].ToString() + " ";
                }
                if (tripletAnalogBuffer != null)
                {
                    int column_size = tripletAnalogBuffer.GetLength(0);
                    for (int j = 0; j < column_size; j++)
                    {
                        line = line + tripletAnalogBuffer[j, i].ToString() + " ";
                    }
                }
                lines.Add(line);
            }

            string fileName = DateTime.Today.ToString("yy-MM-dd") + "_" + DateTime.Now.ToString("HH-mm-ss") + "_triplet_search_lambda_" + currentWavelength.ToString() + ".txt";

            System.IO.File.WriteAllLines(tripletFolder + fileName, lines.ToArray());
        }

        private void TripletOnScanFinished()
        {
            if (TripletScanFinished != null) TripletScanFinished();
        }

        #endregion

        #region TeraScan

        public bool TeraScanAcceptableSettings()
        {
            return true;
        }

        public void StartTeraScan()
        {
            try
            {
                if (teraState != TeraScanState.stopped)
                {
                    var result = System.Windows.Forms.MessageBox.Show("TeraScan is already running. Quit this scan?", "TeraScan Problem", System.Windows.Forms.MessageBoxButtons.YesNo);
                    if (result == System.Windows.Forms.DialogResult.Yes) { throw new ScanAlreadyRunningException("The Scan is Already Running."); }
                    else { throw new CancelTeraScanActionException(); }
                }

                double pointsEstimate = ScanPointsEstimate();
                int blocks = (int)Math.Ceiling(pointsEstimate / MAXSCANPOINTS);

                switch ((string)Settings["TeraScanRepeatType"])
                {
                    case "single":
                        if (pointsEstimate > MAXSCANPOINTS)
                        {
                            StartLongTeraScan(blocks);
                        }
                        else
                        {
                            StartSingleTeraScan();
                        }
                        break;
                    case "multi":
                        //StartMultiTeraScan();
                        break;
                    default:
                        break;
                }
            }
            catch (CancelTeraScanActionException)
            {
            }
            catch (Exception e)
            {
                if (TeraScanProblem != null) TeraScanProblem(e);
            }
            
            //finally
            //{
            //    if (TeraScanFinished != null) TeraScanFinished();
            //}
        }

        //currently not reached
        public void StartMultiTeraScan()
        {
            if (IsRunning() || TimeTracePlugin.GetController().IsRunning() || FastMultiChannelRasterScan.GetController().IsRunning() || CounterOptimizationPlugin.GetController().IsRunning() || DFGPlugin.GetController().IsRunning())
            {
                throw new DaqException("Counter already running");
            }

            teraScanBuffer = new TeraScanDataHolder(((List<string>)Settings["counterChannels"]).Count, ((List<string>)Settings["analogueChannels"]).Count, Settings);

            double initStartLambda = (double)Settings["TeraScanStart"];
            double initStopLambda = (double)Settings["TeraScanStop"];

            teraState = TeraScanState.running;
            backendState = DFGState.running;

            for (int i = 0; i < ((double[])Settings["TeraScanMultiLambda"]).Length; i++)
            {
                if (teraState == TeraScanState.stepping && backendState == DFGState.stepping)
                {
                    teraState = TeraScanState.running;
                    backendState = DFGState.running;
                }

                if ((teraState == TeraScanState.running) == false)
                {
                    break;
                }

                Settings["TeraScanStart"] = SPEEDOFLIGHT / (SPEEDOFLIGHT / ((double[])Settings["TeraScanMultiLambda"])[i] + (double)Settings["TeraScanMultiRange"] / 2);
                Settings["TeraScanStop"] = SPEEDOFLIGHT / (SPEEDOFLIGHT / ((double[])Settings["TeraScanMultiLambda"])[i] - (double)Settings["TeraScanMultiRange"] / 2);

                TeraScanInitialise();
                teraLatestLambda = (double)Settings["TeraScanStart"];
                TeraScanAcquisitionStarting();
                while (true)
                {
                    Dictionary<string, object> autoOutput = new Dictionary<string, object>();
                    while (backendState == DFGState.running && teraState == TeraScanState.running)
                    {
                        autoOutput = DFG.ReceiveCustomMessage("automatic_output", true);
                        //autoOutput = DFG.ReceiveCustomMessage("terascan_output", true);
                        if (autoOutput.Count > 0)
                        {
                            object item;
                            autoOutput.TryGetValue("report", out item);
                            if (item != null)
                            {
                                teraState = TeraScanState.stepping;
                                backendState = DFGState.stepping;
                                break;
                            }
                            else
                            {
                                break;
                            }
                        }
                        Thread.Sleep(10);
                    }
                    if (backendState == DFGState.running && teraState == TeraScanState.running)
                    {
                        TeraScanSegmentStart();
                    }
                    else { break; }
                }

                if (teraLaser == TeraLaserState.stopped)
                {
                    int reply = DFG.scan_stitch_op((string)Settings["TeraScanType"], "stop", false, true);
                }
                else
                {
                    int reply = DFG.scan_stitch_op((string)Settings["TeraScanType"], "stop", false, false);
                    while (teraLaser != TeraLaserState.stopped)
                    {
                        Thread.Sleep(100);
                    }
                }

            }

            Settings["TeraScanStart"] = initStartLambda;
            Settings["TeraScanStop"] = initStopLambda;
            TeraScanAcquisitionStopping();
        }

        public double ScanPointsEstimate()
        {
            double scanPoints = 0.0;
            double sampleRate = (double)TimeTracePlugin.GetController().Settings["sampleRate"];
            double scanRate = Convert.ToDouble((int)Settings["TeraScanRate"]);
            double initStartLambda = (double)Settings["TeraScanStart"];
            double initStopLambda = (double)Settings["TeraScanStop"];
            switch ((string)Settings["TeraScanUnits"])
            {
                case "MHz/s":
                    scanPoints = (sampleRate * (SPEEDOFLIGHT/(scanRate * Math.Pow(10,-3))) *(1.0 / initStartLambda - 1.0 / initStopLambda));
                    break;
                case "GHz/s":
                    scanPoints = (sampleRate * (SPEEDOFLIGHT / (scanRate)) * (1.0 / initStartLambda - 1.0 / initStopLambda));
                    break;
            }
            return scanPoints;
        }

        public void StartLongTeraScan(int blocks)
        {
            double initStartLambda = (double)Settings["TeraScanStart"];
            double initStopLambda = (double)Settings["TeraScanStop"]; 
            
            try
            {
                double rangeLambda = initStopLambda - initStartLambda;

                double sectionLength = rangeLambda / (blocks);

                for (int i = 0; i < blocks; i++)
                {
                    Settings["TeraScanStart"] = initStartLambda + (i * sectionLength);
                    Settings["TeraScanStop"] = initStartLambda + ((i + 1) * sectionLength);

                    StartSingleTeraScan();

                    double[] blank = new double[] { teraLatestLambda };
                    TeraTotalOnlyData(blank, true);
                    SaveTeraScanData(DateTime.Today.ToString("yy-MM-dd") + "_" + DateTime.Now.ToString("HH-mm-ss") + "_teraScan_segment.txt", true);
                    UpdateStatusBar("Saving Data...");
                }
            }
            catch (Exception e)
            {
                if (TeraScanProblem != null) TeraScanProblem(e);
            }
            finally
            {
                Settings["TeraScanStart"] = initStartLambda;
                Settings["TeraScanStop"] = initStopLambda;
                TeraScanAcquisitionStopping();
                TeraScanSegmentAcquisitionEnd();
            }
        }

            public void StartSingleTeraScan()
        {
            if (IsRunning() || TimeTracePlugin.GetController().IsRunning() || FastMultiChannelRasterScan.GetController().IsRunning() || CounterOptimizationPlugin.GetController().IsRunning() || DFGPlugin.GetController().IsRunning())
            {
                throw new DaqException("Counter already running");
            }

            Console.WriteLine("Starting Terascan...");
            UpdateStatusBar("TeraScan starting...");

            teraState = TeraScanState.running;
         
            TeraScanInitialise();
            teraScanBuffer = new TeraScanDataHolder(((List<string>)Settings["counterChannels"]).Count, ((List<string>)Settings["analogueChannels"]).Count, Settings);
            teraLatestLambda = (double)Settings["TeraScanStart"];
            TeraScanAcquisitionStarting();
            backendState = DFGState.running;

            while (true)
            {
                Dictionary<string, object> autoOutput = new Dictionary<string, object>();
                while (backendState == DFGState.running && teraState == TeraScanState.running )
                {
                    autoOutput = DFG.ReceiveCustomMessage("automatic_output", true);

                    if (autoOutput.Count > 0)
                    {
                        object item;
                        autoOutput.TryGetValue("report", out item);
                        if (item != null)
                        {
                            teraState = TeraScanState.stopping;
                            backendState = DFGState.stopping;
                            break;
                        }
                        else
                        {
                            break;
                        }
                    }
                    Thread.Sleep(10);
                }
                if (backendState == DFGState.running && teraState == TeraScanState.running)
                {
                    TeraScanSegmentStart();
                }
                else { break; }
            }
            TeraScanAcquisitionStopping();
            backendState = DFGState.stopped;
            UpdateStatusBar("TeraScan Finished");
        }

        private void TeraScanInitialise()
        {
            DFG.terascan_output("start", 0, 2, "on");

            string scanType = (string)Settings["TeraScanType"];
            double startLambda = (double)Settings["TeraScanStart"];
            double stopLambda = (double)Settings["TeraScanStop"];
            int scanRate = (int)Settings["TeraScanRate"];
            string scanUnits = (string)Settings["TeraScanUnits"];
            int reply = DFG.scan_stitch_initialise(scanType, startLambda, stopLambda, scanRate, scanUnits);
            Console.WriteLine("Scan Stitch Initialised...");
            TeraLaserState = TeraLaserState.running;
            switch (reply)
            {
                case 0:
                    return;
                default:
                    throw new Exception("couldn't initialise tera scan");
            }
        }

        private void TeraScanAcquisitionStarting()
        {
            // Turn on correct display
            teraScan_display_is_current_segment = (bool)Settings["tera_display_current_segment"];
            teraScan_current_channel_type = (string)Settings["tera_channel_type"];
            teraScan_current_display_channel_index = (int)Settings["tera_display_channel_index"];

            string scanType = (string)Settings["TeraScanType"];
            int reply = dfg.scan_stitch_op(scanType, "start", true, false);
            switch (reply)
            {
                case 0:
                    return;
                default:
                    throw new Exception("couldn't start tera scan");
            }
        }

        public void TeraScanAcquisitionStopping()
        {

            teraState = TeraScanState.stopped;

            if (teraLaser == TeraLaserState.stopped)
            {
                int reply = DFG.scan_stitch_op((string)Settings["TeraScanType"], "stop", false, true);
            }
            else
            {
                int reply = DFG.scan_stitch_op((string)Settings["TeraScanType"], "stop", false, false);
                while (teraLaser != TeraLaserState.stopped) 
                {
                    Thread.Sleep(100);
                }
            }
            backendState = DFGState.stopped;
        }

        private void TeraScanSegmentAcquisitionStarting()
        {
            // Initialise containers
            fastAnalogBuffer = new List<Point>[((List<string>)Settings["analogueChannels"]).Count];
            for (int i = 0; i < ((List<string>)Settings["analogueChannels"]).Count; i++)
            {
                fastAnalogBuffer[i] = new List<Point>();
            }
            fastCounterBuffer = new List<Point>[((List<string>)Settings["counterChannels"]).Count];
            fastLatestCounters = new double[((List<string>)Settings["counterChannels"]).Count];
            for (int i = 0; i < ((List<string>)Settings["counterChannels"]).Count; i++)
            {
                fastCounterBuffer[i] = new List<Point>();
                fastLatestCounters[i] = 0;
            }

            // Define sample rate 
            pointsPerExposure = 1;
            sampleRate = (double)TimeTracePlugin.GetController().Settings["sampleRate"];

            // Set up clock task
            freqOutTask = new Task("sample clock task");

            // Finite pulse train
            freqOutTask.COChannels.CreatePulseChannelFrequency(
                ((CounterChannel)Environs.Hardware.CounterChannels["SampleClock"]).PhysicalChannel,
                "photon counter clocking signal",
                COPulseFrequencyUnits.Hertz,
                COPulseIdleState.Low,
                0,
                sampleRate,
                0.5);


            freqOutTask.Timing.ConfigureImplicit(SampleQuantityMode.ContinuousSamples);

            freqOutTask.Control(TaskAction.Verify);

            // Set up edge-counting tasks
            counterTasks = new List<Task>();
            counterReaders = new List<CounterSingleChannelReader>();

            for (int i = 0; i < ((List<string>)Settings["counterChannels"]).Count; i++)
            {
                string channelName = ((List<string>)Settings["counterChannels"])[i];

                counterTasks.Add(new Task("buffered edge counters " + channelName));

                // Count upwards on rising edges starting from zero
                counterTasks[i].CIChannels.CreateCountEdgesChannel(
                    ((CounterChannel)Environs.Hardware.CounterChannels[channelName]).PhysicalChannel,
                    "edge counter " + channelName,
                    CICountEdgesActiveEdge.Rising,
                    0,
                    CICountEdgesCountDirection.Up);

                // Take one sample within a window determined by sample rate using clock task
                counterTasks[i].Timing.ConfigureSampleClock(
                    (string)Environs.Hardware.GetInfo("SampleClockReader"),
                    sampleRate,
                    SampleClockActiveEdge.Rising,
                    SampleQuantityMode.ContinuousSamples);

                counterTasks[i].Control(TaskAction.Verify);

                DaqStream counterStream = counterTasks[i].Stream;
                counterReaders.Add(new CounterSingleChannelReader(counterStream));

                // Start tasks
                counterTasks[i].Start();
            }

            // Set up analogue sampling tasks
            analoguesTask = new Task("analogue sampler");

            for (int i = 0; i < ((List<string>)Settings["analogueChannels"]).Count; i++)
            {
                string channelName = ((List<string>)Settings["analogueChannels"])[i];

                double inputRangeLow = ((Dictionary<string, double[]>)Settings["analogueLowHighs"])[channelName][0];
                double inputRangeHigh = ((Dictionary<string, double[]>)Settings["analogueLowHighs"])[channelName][1];

                ((AnalogInputChannel)Environs.Hardware.AnalogInputChannels[channelName]).AddToTask(
                    analoguesTask,
                    inputRangeLow,
                    inputRangeHigh
                    );
            }

            if (((List<string>)Settings["analogueChannels"]).Count != 0)
            {
                analoguesTask.Timing.ConfigureSampleClock(
                    (string)Environs.Hardware.GetInfo("SampleClockReader"),
                    sampleRate,
                    SampleClockActiveEdge.Rising,
                    SampleQuantityMode.ContinuousSamples);

                analoguesTask.Control(TaskAction.Verify);

                DaqStream analogStream = analoguesTask.Stream;
                analoguesReader = new AnalogMultiChannelReader(analogStream);

                // Start tasks
                analoguesTask.Start();
            }


            Console.WriteLine("Segment Acquisition Started...");
        }

        private void TeraScanSegmentAcquisitionStart()
        {
            while (latestLambda < 0 && backendState == DFGState.running && teraSegmentState == TeraScanSegmentState.running && teraState == TeraScanState.running)
            {
                Thread.Sleep(1);
            }//This waits until the laser is actually scanning
            freqOutTask.Start();
            bool isFirst = true;

            DateTime displayStartTime = DateTime.Now;
            DateTime displayCurrentTime = DateTime.Now;
            List<double> displayData = new List<double>();

            while (backendState == DFGState.running && teraSegmentState == TeraScanSegmentState.running && teraState == TeraScanState.running)
            {
               // Read first counter
                double[] counterRead = counterReaders[0].ReadMultiSampleDouble(-1);
                int dataReadLength = counterRead.Length;

                if (dataReadLength > 0)
                {
                    if (isFirst)
                    {
                        teraScanBuffer.latestCounters[0] = counterRead[counterRead.Length - 1];

                        for (int i = 1; i < teraScanBuffer.numberCounterChannels; i++)
                        {
                            counterRead = counterReaders[i].ReadMultiSampleDouble(dataReadLength);
                            teraScanBuffer.latestCounters[i] = counterRead[counterRead.Length - 1];
                        }

                        if (((List<string>)Settings["analogueChannels"]).Count != 0)
                        {
                            double[,] analogRead = analoguesReader.ReadMultiSample(dataReadLength);
                        }

                        TeraSegmentOnlyData(new double[] { }, true);

                        isFirst = false;
                    }

                    else
                    {
                        double dataLambda = teraLatestLambda;
                        teraScanBuffer.AddLambdaDataToCurrentSegment(dataLambda);
                        if (teraScan_current_channel_type == "Lambda")
                        {
                            displayData.Add(dataLambda);
                        }

                        double dataRead = counterRead[0] - teraScanBuffer.latestCounters[0];
                        teraScanBuffer.AddCounterDataToCurrentSegment(0, dataRead);
                        teraScanBuffer.latestCounters[0] = counterRead[counterRead.Length - 1];
                        if (teraScan_current_channel_type == "Counters" && teraScan_current_display_channel_index == 0)
                        {
                            displayData.Add(dataRead);
                        }

                        if (dataReadLength > 1)
                        {
                            for (int j = 1; j < counterRead.Length; j++)
                            {
                                dataLambda = teraLatestLambda;
                                teraScanBuffer.AddLambdaDataToCurrentSegment(dataLambda);
                                if (teraScan_current_channel_type == "Lambda")
                                {
                                    displayData.Add(dataLambda);
                                }

                                dataRead = counterRead[j] - counterRead[j - 1];
                                teraScanBuffer.AddCounterDataToCurrentSegment(0, dataRead);
                                if (teraScan_current_channel_type == "Counters" && teraScan_current_display_channel_index == 0)
                                {
                                    displayData.Add(dataRead);
                                }
                            }
                        }

                        // Read other counter data
                        for (int i = 1; i < teraScanBuffer.numberCounterChannels; i++)
                        {
                            counterRead = counterReaders[i].ReadMultiSampleDouble(dataReadLength);

                            dataRead = counterRead[0] - teraScanBuffer.latestCounters[i];
                            teraScanBuffer.AddCounterDataToCurrentSegment(i, dataRead);
                            teraScanBuffer.latestCounters[i] = counterRead[counterRead.Length - 1];
                            if (teraScan_current_channel_type == "Counters" && teraScan_current_display_channel_index == i)
                            {
                                displayData.Add(dataRead);
                            }

                            if (counterRead.Length > 1)
                            {
                                for (int j = 1; j < counterRead.Length; j++)
                                {
                                    dataRead = counterRead[j] - counterRead[j - 1];
                                    teraScanBuffer.AddCounterDataToCurrentSegment(i, dataRead);
                                    if (teraScan_current_channel_type == "Counters" && teraScan_current_display_channel_index == i)
                                    {
                                        displayData.Add(dataRead);
                                    }
                                }
                            }
                        }

                        // Read analogue data
                        if (((List<string>)Settings["analogueChannels"]).Count != 0)
                        {
                            double[,] analogRead = analoguesReader.ReadMultiSample(dataReadLength);

                            for (int i = 0; i < analogRead.GetLength(0); i++)
                            {
                                for (int j = 0; j < analogRead.GetLength(1); j++)
                                {
                                    teraScanBuffer.AddAnalogueDataToCurrentSegment(i, analogRead[i, j]);
                                    if (teraScan_current_channel_type == "Analogues" && teraScan_current_display_channel_index == i)
                                    {
                                        displayData.Add(analogRead[i, j]);
                                    }
                                }
                            }
                        }

                        // Broadcast
                        displayCurrentTime = DateTime.Now;
                        if ((displayCurrentTime - displayStartTime).TotalMilliseconds > 100 && displayData.Count > 0)
                        {
                            if (teraScan_display_is_current_segment != (bool)Settings["tera_display_current_segment"] || teraScan_current_channel_type != (string)Settings["tera_channel_type"] || teraScan_current_display_channel_index != (int)Settings["tera_display_channel_index"])
                            {
                                teraScan_display_is_current_segment = (bool)Settings["tera_display_current_segment"];
                                teraScan_current_channel_type = (string)Settings["tera_channel_type"];
                                teraScan_current_display_channel_index = (int)Settings["tera_display_channel_index"];
                                TeraScanChangeDisplay();
                                if (teraScan_display_is_current_segment) { TeraData(teraScanDisplayTotalWaveform, teraScanDisplaySegmentWaveform, true); }
                                else { TeraTotalOnlyData(teraScanDisplayTotalWaveform, true); }
                            }
                            else
                            {
                                teraScanDisplayTotalWaveform = displayData.ToArray();
                                teraScanDisplaySegmentWaveform = displayData.ToArray();
                                if (teraScan_display_is_current_segment) { TeraData(teraScanDisplayTotalWaveform, teraScanDisplaySegmentWaveform, false); }
                                else { TeraTotalOnlyData(teraScanDisplayTotalWaveform, false); }
                            }
                            displayStartTime = DateTime.Now;
                            displayData = new List<double>();
                        }
                    }
                }
            }

            // Final Broadcast
            if (teraScan_display_is_current_segment != (bool)Settings["tera_display_current_segment"] || teraScan_current_channel_type != (string)Settings["tera_channel_type"] || teraScan_current_display_channel_index != (int)Settings["tera_display_channel_index"])
            {
                teraScan_display_is_current_segment = (bool)Settings["tera_display_current_segment"];
                teraScan_current_channel_type = (string)Settings["tera_channel_type"];
                teraScan_current_display_channel_index = (int)Settings["tera_display_channel_index"];
                TeraScanChangeDisplay();
                if (teraScan_display_is_current_segment) { TeraData(teraScanDisplayTotalWaveform, teraScanDisplaySegmentWaveform, true); }
                else { TeraTotalOnlyData(teraScanDisplayTotalWaveform, true); }
            }
            else
            {
                teraScanDisplayTotalWaveform = displayData.ToArray();
                teraScanDisplaySegmentWaveform = displayData.ToArray();
                if (teraScan_display_is_current_segment) { TeraData(teraScanDisplayTotalWaveform, teraScanDisplaySegmentWaveform, false); }
                else { TeraTotalOnlyData(teraScanDisplayTotalWaveform, false); }
            }

            freqOutTask.Stop();
        }

        public void TeraScanSegmentAcquisitionEnd()
        {
            try { freqOutTask.Dispose(); } catch (NullReferenceException e) { }
            try
            {
                foreach (Task counterTask in counterTasks)
                {
                    try
                    {
                        counterTask.Dispose();
                    }
                    catch (NullReferenceException e)
                    {
                    }
                }
            }
            catch (NullReferenceException e){}
            try { analoguesTask.Dispose(); } catch (NullReferenceException e) { }

            freqOutTask = null;
            counterTasks = null;
            analoguesTask = null;

            counterReaders = null;
            analoguesReader = null;

            UpdateStatusBar("Segment Finished");

            Console.WriteLine("Segment Acquisition End");
        }

        private void TeraScanSegmentLaserStart()
        {
            teraLaser = TeraLaserState.running;
            string status = "scan";
            
            DFG.terascan_continue();
            Console.WriteLine("Continue Terascan...");
            UpdateStatusBar("Laser Running...");

            while (teraLatestLambda < (Double)Settings["TeraScanStop"] && teraLaser != TeraLaserState.stopped)
            {
                Dictionary<string, object> autoOutput = DFG.ReceiveCustomMessage("automatic_output", true);

                if (autoOutput.Count != 0)
                {
                    try
                    {
                        object latestWavelength = null;
                        autoOutput.TryGetValue("wavelength", out latestWavelength);
                        if(latestWavelength != null)
                        {
                            teraLatestLambda = Convert.ToDouble(latestWavelength);
                        }
                        object latestStatus = null;
                        if (autoOutput.TryGetValue("status", out latestStatus))
                        {
                            status = Convert.ToString(latestStatus);
                        }
                        if (backendState != DFGState.running || teraState != TeraScanState.running || status == "start")
                        {
                            teraLaser = TeraLaserState.stopped;
                        }
                        if (status == "end")
                        {
                            teraLaser = TeraLaserState.stopped;
                            teraSegmentState = TeraScanSegmentState.stopped;
                            if (teraLatestLambda > (Double)Settings["TeraScanStop"]) 
                            {
                                teraState = TeraScanState.stopped;
                            }
                        }
                    }
                    catch (KeyNotFoundException e)
                    {
                        // Get stack trace for the exception with source file information
                        var st = new System.Diagnostics.StackTrace(e, true);
                        // Get the top stack frame
                        var frame = st.GetFrame(0);
                        // Get the line number from the stack frame
                        var line = frame.GetFileLineNumber();
                        teraLaser = TeraLaserState.stopped;
                        teraSegmentState = TeraScanSegmentState.stopped;
                        teraState = TeraScanState.stopped;
                    }
                }
                else
                {

                    UpdateStatusBar("Waiting for Laser...");
                    //Console.WriteLine("Waiting for Laser Output...");
                    continue;
                }
            }

            if (status != "end")
            {
                teraSegmentState = TeraScanSegmentState.running;
                UpdateStatusBar("Segment not finished");
                Console.WriteLine("Laser has stopped before segment came to an end.");
                //MessageBox.Show(status);
            }
            else 
            { 
                teraLaser = TeraLaserState.stopped; 
                teraSegmentState = TeraScanSegmentState.stopped;
                UpdateStatusBar("Segment ended");
                Console.WriteLine("Segment ended.");
            }

        }

        private void TeraScanSegmentStart()
        {
            UpdateStatusBar("Starting Segment..."); 
            if (teraLatestLambda < (Double)Settings["TeraScanStop"]) { teraLatestLambda = -1; }
            teraScanBuffer.AddNewSegment();
            TeraScanSegmentAcquisitionStarting();
            teraSegmentState = TeraScanSegmentState.running;
            Thread thread = new Thread(new ThreadStart(TeraScanSegmentLaserStart));
            thread.IsBackground = true;
            thread.Start();
            TeraScanSegmentAcquisitionStart();
            while (teraLatestLambda < 0 && teraSegmentState == TeraScanSegmentState.running)
            {
                Thread.Sleep(100);
                if (teraLaser != TeraLaserState.stopped)
                {
                    UpdateStatusBar("Laser stopped but still acquiring");
                }
            }
            TeraScanSegmentAcquisitionEnd();
            // SaveTeraScanData(DateTime.Today.ToString("yy-MM-dd") + "_" + DateTime.Now.ToString("HH-mm-ss") + "_teraScan_segment.txt", true);
            if (TeraSegmentScanFinished != null) { TeraSegmentScanFinished(); }
        }

        private void TeraScanChangeDisplay()
        {
            int index;
            switch ((string)Settings["tera_channel_type"])
            {
                case "Counters":
                    index = (int)Settings["tera_display_channel_index"];
                    if (index >= 0 && index < ((List<string>)Settings["counterChannels"]).Count)
                    {
                        teraScanDisplayTotalWaveform = teraScanBuffer.GetCounterData(index).ToArray();
                        teraScanDisplaySegmentWaveform = teraScanBuffer.GetCurrentSegment().GetCounterData(index).ToArray();
                    }
                    break;

                case "Analogues":
                    index = (int)Settings["tera_display_channel_index"];
                    if (index >= 0 && index < ((List<string>)Settings["analogueChannels"]).Count)
                    {
                        teraScanDisplayTotalWaveform = teraScanBuffer.GetAnalogueData(index).ToArray();
                        teraScanDisplaySegmentWaveform = teraScanBuffer.GetCurrentSegment().GetAnalogueData(index).ToArray();
                    }
                    break;

                case "Lambda":
                    teraScanDisplayTotalWaveform = teraScanBuffer.GetLambdaData().ToArray();
                    teraScanDisplaySegmentWaveform = teraScanBuffer.GetCurrentSegment().GetWavelengthData().ToArray();
                    break;
                default:
                    throw new Exception("Did not understand data type");
            }
        }

        public void RequestSegmentData(int segmentIndex)
        {
            SegmentDataHolder segment = DFGPlugin.GetController().teraScanBuffer.GetSegment(segmentIndex);
            int index;
            switch ((string)DFGPlugin.GetController().Settings["tera_channel_type"])
            {
                case "Counters":
                    index = (int)DFGPlugin.GetController().Settings["tera_display_channel_index"];
                    if (index >= 0 && index < ((List<string>)DFGPlugin.GetController().Settings["counterChannels"]).Count)
                    {
                        TeraSegmentOnlyData(segment.GetCounterData(index).ToArray(), true);
                    }
                    else { TeraSegmentOnlyData(new double[] { }, true); }
                    break;

                case "Analogues":
                    index = (int)DFGPlugin.GetController().Settings["tera_display_channel_index"];
                    if (index >= 0 && index < ((List<string>)DFGPlugin.GetController().Settings["analogueChannels"]).Count)
                    {
                        TeraSegmentOnlyData(segment.GetAnalogueData(index).ToArray(), true);
                    }
                    else { TeraSegmentOnlyData(new double[] { }, true); }
                    break;

                case "Lambda":
                    TeraSegmentOnlyData(segment.GetWavelengthData().ToArray(), true);
                    break;
                default:
                    throw new Exception("Did not understand data type");
            }
        }

        public void RequestTeraHistoricData()
        {
            if (teraScanBuffer.currentSegmentIndex >= 0) { TeraScanChangeDisplay(); TeraData(teraScanDisplayTotalWaveform, teraScanDisplaySegmentWaveform, true); }
        }

        public void SaveTeraScanData(string fileName, bool automatic)
        {
            string directory = Environs.FileSystem.GetDataDirectory((string)Environs.FileSystem.Paths["scanMasterDataPath"]);

            List<string> lines = new List<string>();
            lines.Add(DateTime.Today.ToString("dd-MM-yyyy") + " " + DateTime.Now.ToString("HH:mm:ss"));
            lines.Add("Sample rate = " + ((double)teraScanBuffer.historicSettings["sampleRate"]).ToString());
            lines.Add("Type: " + (string)teraScanBuffer.historicSettings["TeraScanType"]);
            lines.Add("Lambda start = " + ((double)teraScanBuffer.historicSettings["TeraScanStart"]).ToString() + ", Lambda stop = " + ((double)teraScanBuffer.historicSettings["TeraScanStop"]).ToString() + ", Frequency rate = " + ((int)teraScanBuffer.historicSettings["TeraScanRate"]).ToString() + (string)teraScanBuffer.historicSettings["TeraScanUnits"]);
            lines.Add("");

            string descriptionString = "Segment Index Lambda";

            foreach (string channel in (List<string>)teraScanBuffer.historicSettings["counterChannels"])
            {
                descriptionString = descriptionString + " " + channel;
            }
            foreach (string channel in (List<string>)teraScanBuffer.historicSettings["analogueChannels"])
            {
                descriptionString = descriptionString + " " + channel;
            }
            lines.Add(descriptionString);


            for (int i = 0; i < teraScanBuffer.currentSegmentIndex + 1; i++)
            {
                SegmentDataHolder segment = teraScanBuffer.GetSegment(i);
                if (segment.GetWavelengthData().Count > 0)
                {
                    for (int j = 0; j < segment.GetWavelengthData().Count; j++)
                    {
                        string line = i.ToString() + " " + j.ToString() + " " + segment.GetWavelengthData()[j];
                        for (int n = 0; n < teraScanBuffer.numberCounterChannels; n++)
                        {
                            line = line + " " + segment.GetCounterData(n)[j].ToString();
                        }
                        for (int m = 0; m < teraScanBuffer.numberAnalogChannels; m++)
                        {
                            line = line + " " + segment.GetAnalogueData(m)[j].ToString();
                        }
                        lines.Add(line);

                    }
                }
            }

            if (automatic) System.IO.File.WriteAllLines(directory + fileName, lines.ToArray());
            else
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.InitialDirectory = directory;
                saveFileDialog.FileName = fileName;

                if (saveFileDialog.ShowDialog() == true)
                {
                    System.IO.File.WriteAllLines(saveFileDialog.FileName, lines.ToArray());
                }
            }
        }

        #endregion

    }

}