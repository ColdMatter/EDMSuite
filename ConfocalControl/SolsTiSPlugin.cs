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
    public delegate void SpectraScanDataEventHandler(Point[] data);
    public delegate void TeraSingleDataEventHandler(double[] data);
    public delegate void TeraScanFullDataEventHandler(double[] dataTotal, double[] dataSegment, bool reset);
    public delegate void TeraScanSingleChannelDataEventHandler(double[] dataTotal, bool reset);
    public delegate void SpectraScanExceptionEventHandler(Exception e);

    public class SolsTiSPlugin
    {

        #region Class members

        // Dependencies should refer to this instance only 
        private static SolsTiSPlugin controllerInstance;
        public static SolsTiSPlugin GetController()
        {
            if (controllerInstance == null)
            {
                controllerInstance = new SolsTiSPlugin();
            }
            return controllerInstance;
        }

        // Define states
        private enum SolsTisState { stopped, running, stopping, stepping };
        private SolsTisState backendState = SolsTisState.stopped;
        private enum WavemeterScanState { stopped, running, stopping };
        private WavemeterScanState wavemeterState = WavemeterScanState.stopped;
        private enum FastScanState { stopped, running, stopping };
        private FastScanState fastState = FastScanState.stopped;
        private enum TeraScanState { stopped, running, stopping, stepping };
        private TeraScanState teraState = TeraScanState.stopped;
        private enum TeraScanSegmentState { stopped, running, stopping, unfinished };
        private TeraScanSegmentState teraSegmentState = TeraScanSegmentState.stopped;
        private enum TeraLaserState { stopped, running, stopping };
        private TeraLaserState teraLaser = TeraLaserState.stopped;
        private enum TripletScanState { stopped, running, stopping };
        private TripletScanState tripletState = TripletScanState.stopped;

        // Settings
        public PluginSettings Settings { get; set; }

        // Laser
        private ICEBlocSolsTiS solstis;
        public ICEBlocSolsTiS Solstis { get { return solstis; } }

        // Bound event managers to class
        public event SpectraScanDataEventHandler WavemeterData;
        public event MultiChannelScanFinishedEventHandler WavemeterScanFinished;
        public event SpectraScanExceptionEventHandler WavemeterScanProblem;

        public event SpectraScanDataEventHandler FastData;
        public event MultiChannelScanFinishedEventHandler FastScanFinished;
        public event SpectraScanExceptionEventHandler FastScanProblem;

        public event TeraScanFullDataEventHandler TeraData;
        public event TeraScanSingleChannelDataEventHandler TeraTotalOnlyData;
        public event TeraScanSingleChannelDataEventHandler TeraSegmentOnlyData;
        public event MultiChannelScanFinishedEventHandler TeraSegmentScanFinished;
        public event MultiChannelScanFinishedEventHandler TeraScanFinished;
        public event SpectraScanExceptionEventHandler TeraScanProblem;

        public event TeraSingleDataEventHandler TripletData;
        public event TeraSingleDataEventHandler TripletDFTData;
        public event MultiChannelScanFinishedEventHandler TripletScanFinished;
        public event SpectraScanExceptionEventHandler TripletScanProblem;

        // Constants relating to sample acquisition
        private int MINNUMBEROFSAMPLES = 10;
        private double TRUESAMPLERATE = 1000;
        private int pointsPerExposure;
        private double sampleRate;

        // Keep track of data
        private List<Point>[] wavemeterAnalogBuffer;
        private List<Point>[] wavemeterCounterBuffer;
        public Hashtable wavemeterHistoricSettings;
        private List<Point>[] fastAnalogBuffer;
        private List<Point>[] fastCounterBuffer;
        private double[] fastLatestCounters;

        // Keep track of data for teraScan
        private double[] teraScanDisplayTotalWaveform;
        private double[] teraScanDisplaySegmentWaveform;
        private bool teraScan_display_is_current_segment;
        private string teraScan_current_channel_type;
        private int teraScan_current_display_channel_index;
        private TeraScanDataHolder teraScanBuffer;
        public TeraScanDataHolder teraScanBufferAccess { get { return teraScanBuffer; } }
        private double teraLatestLambda;
        public double latestLambda { get { return teraLatestLambda; } }

        // Keep track of data for triplet search
        private string tripletFolder;
        private double[,] tripletAnalogBuffer;
        private List<double[]> tripletCounterBuffer;
        public Hashtable tripletHistoricSettings;

        // Keep track of tasks
        private Task triggerTask;
        private DigitalSingleChannelWriter triggerWriter;
        private Task freqOutTask;
        private List<Task> counterTasks;
        private List<CounterSingleChannelReader> counterReaders;
        private Task analoguesTask;
        private AnalogMultiChannelReader analoguesReader;
        private Task voltageTask;
        private AnalogSingleChannelReader voltageReader;

        double SPEEDOFLIGHT = 299792458;

        #endregion

        #region Initialization

        public void LoadSettings()
        {
            Settings = PluginSaveLoad.LoadSettings("solstis");
        }

        public SolsTiSPlugin()
        {
            solstis = new ICEBlocSolsTiS();

            LoadSettings();
            if (Settings.Keys.Count != 35)
            {
                Settings["wavelength"] = 785.0;

                Settings["wavemeterScanStart"] = 784.0;
                Settings["wavemeterScanStop"] = 785.0;
                Settings["wavemeterScanPoints"] = 101;

                Settings["counterChannels"] = new List<string> { "APD0" };
                Settings["analogueChannels"] = new List<string> { };
                Settings["analogueLowHighs"] = new Dictionary<string, double[]>();

                Settings["fastScanWidth"] = (double)50;
                Settings["fastScanTime"] = (double)100;
                Settings["fastScanType"] = "etalon_single";

                Settings["wavemeter_channel_type"] = "Counters";
                Settings["wavemeter_display_channel_index"] = 0;
                
                Settings["displayType"] = "Time"; 
                Settings["fast_channel_type"] = "Counters";
                Settings["fast_display_channel_index"] = 0;
                Settings["fastVoltageChannel"] = "AI2";
                Settings["fastVoltageLowHigh"] = new double[] { 0, 5 };

                Settings["TeraScanRepeatType"] = "single";
                Settings["TeraScanType"] = "fine";
                Settings["TeraScanStart"] = (double)743.0;
                Settings["TeraScanStop"] = (double)743.5;
                Settings["TeraScanRate"] = 500;
                Settings["TeraScanUnits"] = "MHz/s";
                Settings["TeraScanMultiLambda"] = new double[] { 750, 760, 770, 780, 790 };
                Settings["TeraScanMultiRange"] = 10.0;
                Settings["tera_channel_type"] = "Lambda";
                Settings["tera_display_channel_index"] = 0;
                Settings["tera_display_current_segment"] = true;

                Settings["tripletStart"] = 784.0;
                Settings["tripletStop"] = 785.0;
                Settings["tripletScanPoints"] = 101;
                Settings["tripletInt"] = 2.0;
                Settings["tripletRate"] = 1000.0;

                Settings["triplet_channel_type"] = "Counters";
                Settings["triplet_display_channel_index"] = 0;
            }

            wavemeterHistoricSettings = new Hashtable();
            wavemeterHistoricSettings["counterChannels"] = (List<string>)Settings["counterChannels"];
            wavemeterHistoricSettings["analogueChannels"] = (List<string>)Settings["analogueChannels"];

            teraScanBuffer = new TeraScanDataHolder(((List<string>)Settings["counterChannels"]).Count, ((List<string>)Settings["analogueChannels"]).Count, Settings);

            tripletHistoricSettings = new Hashtable();
            tripletHistoricSettings["counterChannels"] = (List<string>)Settings["counterChannels"];
            tripletHistoricSettings["analogueChannels"] = (List<string>)Settings["analogueChannels"];

            triggerTask = null;
            freqOutTask = null;
            counterTasks = null;
            analoguesTask = null;
            voltageTask = null;

            triggerWriter = null;
            counterReaders = null;
            analoguesReader = null;
            voltageReader = null;
        }

        #endregion

        #region Shared functions

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

        public bool IsRunning()
        {
            return backendState == SolsTisState.running;
        }

        public bool WavemeterScanIsRunning()
        {
            return wavemeterState == WavemeterScanState.running;
        }

        public bool FastScanIsRunning()
        {
            return fastState == FastScanState.running;
        }

        public bool TeraScanIsRunning()
        {
            return teraState == TeraScanState.running;
        }

        public bool TeraSegmentIsRunning()
        {
            return teraSegmentState == TeraScanSegmentState.running;
        }

        public bool TripletScanIsRunning()
        {
            return tripletState == TripletScanState.running;
        }

        private bool CheckIfStopping()
        {
            return backendState == SolsTisState.stopping;
        }

        public void StopAcquisition()
        {
            backendState = SolsTisState.stopping;
        }

        #endregion

        #region Wavemeter Scan

        private int SetAndLockWavelength(double wavelength)
        {
            return Solstis.set_wave_m(wavelength, true);
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
                if (IsRunning() || TimeTracePlugin.GetController().IsRunning() || FastMultiChannelRasterScan.GetController().IsRunning() || CounterOptimizationPlugin.GetController().IsRunning() || DFGPlugin.GetController().IsRunning())
                {
                    throw new DaqException("Daq already running");
                }

                wavemeterState = WavemeterScanState.running;
                backendState = SolsTisState.running;
                wavemeterHistoricSettings["wavemeterScanStart"] = (double)Settings["wavemeterScanStart"];
                wavemeterHistoricSettings["wavemeterScanStop"] = (double)Settings["wavemeterScanStop"];
                wavemeterHistoricSettings["wavemeterScanPoints"] = (int)Settings["wavemeterScanPoints"];
                wavemeterHistoricSettings["exposure"] = TimeTracePlugin.GetController().GetExposure();
                wavemeterHistoricSettings["counterChannels"] = (List<string>)Settings["counterChannels"];
                wavemeterHistoricSettings["analogueChannels"] = (List<string>)Settings["analogueChannels"];
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
            wavemeterAnalogBuffer = new List<Point>[((List<string>)Settings["analogueChannels"]).Count];
            for (int i = 0; i < ((List<string>)Settings["analogueChannels"]).Count; i++)
            {
                wavemeterAnalogBuffer[i] = new List<Point>();
            }
            wavemeterCounterBuffer = new List<Point>[((List<string>)Settings["counterChannels"]).Count];
            for (int i = 0; i < ((List<string>)Settings["counterChannels"]).Count; i++)
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
        }

        private void WavemeterSynchronousAcquire()
        // Main method for looping over scan parameters, aquiring scan outputs and connecting to controller for display
        {
            // Go to start of scan
            int report = SetAndLockWavelength((double)Settings["wavemeterScanStart"]);
            if (report == 1) throw new Exception("set_wave_m start: task failed");

            // Main loop
            for (double i = 0;
                    i < (int)Settings["wavemeterScanPoints"];
                    i++)

            {
                double currentWavelength = (double)Settings["wavemeterScanStart"] + i * 
                    ((double)Settings["wavemeterScanStop"] - (double)Settings["wavemeterScanStart"]) /
                    ((int)Settings["wavemeterScanPoints"] - 1);

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
                    if (((List<string>)Settings["analogueChannels"]).Count != 0)
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
                    if (((List<string>)Settings["analogueChannels"]).Count != 0)
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
                        report = SetAndLockWavelength((double)Settings["wavemeterScanStart"]);
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
            backendState = SolsTisState.stopped;
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

        #region Fast Scan

        public bool FastScanAcceptableSettings()
        {
            if ((double)Settings["fastScanWidth"] <= 0.01 || (double)Settings["fastScanTime"] <= 0.01 || (double)Settings["fastScanTime"] >= 10000)
            {
                MessageBox.Show("FastScan settings unacceptable.");
                return false;
            }
            else
            {
                return true;
            }
        }

        public void FastSynchronousStartScan()
        {
            try
            {
                if (IsRunning() || TimeTracePlugin.GetController().IsRunning() || FastMultiChannelRasterScan.GetController().IsRunning() || CounterOptimizationPlugin.GetController().IsRunning() || DFGPlugin.GetController().IsRunning())
                {
                    throw new DaqException("Daq already running");
                }

                fastState = FastScanState.running;
                backendState = SolsTisState.running;

                FastSynchronousAcquisitionStarting();
                FastSynchronousAcquire();
            }
            catch (Exception e)
            {
                if (FastScanProblem != null) FastScanProblem(e);
            }
        }

        private void FastSynchronousAcquisitionStarting()
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

            // Set up voltage sampling tasks
            voltageTask = new Task("voltage sampler");

            string voltageChannelName = (string)Settings["fastVoltageChannel"];

            double voltageInputRangeLow = ((double[])Settings["fastVoltageLowHigh"])[0];
            double voltageInputRangeHigh = ((double[])Settings["fastVoltageLowHigh"])[1];

            ((AnalogInputChannel)Environs.Hardware.AnalogInputChannels[voltageChannelName]).AddToTask(
                voltageTask,
                voltageInputRangeLow,
                voltageInputRangeHigh
                );

            voltageTask.Timing.ConfigureSampleClock(
                (string)Environs.Hardware.GetInfo("SampleClockReader"),
                sampleRate,
                SampleClockActiveEdge.Rising,
                SampleQuantityMode.ContinuousSamples);

            voltageTask.Control(TaskAction.Verify);

            DaqStream voltageStream = voltageTask.Stream;
            voltageReader = new AnalogSingleChannelReader(voltageStream);

            // Start tasks
            voltageTask.Start();

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

            // Change laser output signal and etalon lock
            switch ((string)Settings["fastScanType"])
            {
                case "etalon_continuous":
                    Solstis.monitor_a(2, true);
                    break;

                case "resonator_continuous":
                    Solstis.monitor_a(6, true);
                    break;

                default:
                    throw new Exception("Did not understand scan type");
            }
        }

        private void FastSynchronousAcquire()
        {
            Thread thread = new Thread(new ThreadStart(StartLaserFastScan));
            thread.IsBackground = true;
            thread.Start();
            freqOutTask.Start();

            while (backendState == SolsTisState.running)
            {
                AcquireContinuous();
                FastOnData();
                Thread.Sleep(10);
            }

            FastOnScanFinished();
        }

        private void StartLaserFastScan()
        {
            int reply = Solstis.fast_scan_start((string)Settings["fastScanType"], (double)Settings["fastScanWidth"], (double)Settings["fastScanTime"], true);

            switch (reply)
            {
                case -1:
                    throw new Exception("empty reply");

                case 0:
                    break;

                case 1:
                    throw new Exception("Failed, scan width too great for current tuning position.");

                default:
                    throw new Exception("did not understand etalon reply");
            }

            // backendState = SolsTisState.stopping;
            // fastState = FastScanState.stopping;
        }

        private void AcquireContinuous()
        {
            // Read voltage data
            double[] voltageRead = voltageReader.ReadMultiSample(-1);

            if (voltageRead.Length > 0)
            {
                // Read counter data
                for (int i = 0; i < fastCounterBuffer.Length; i++)
                {
                    double[] counterRead = counterReaders[i].ReadMultiSampleDouble(voltageRead.Length);

                    if (counterRead.Length > 0)
                    {
                        Point[] dataRead = new Point[counterRead.Length];
                        dataRead[0] = new Point(voltageRead[0], counterRead[0] - fastLatestCounters[i]);
                        fastLatestCounters[i] = counterRead[counterRead.Length - 1];

                        if (counterRead.Length > 1)
                        {
                            for (int j = 1; j < counterRead.Length; j++)
                            {
                                dataRead[j] = new Point(voltageRead[j], counterRead[j] - counterRead[j - 1]);
                            }
                        }

                        fastCounterBuffer[i].AddRange(dataRead);
                    }
                }

                // Read analogue data
                if (((List<string>)Settings["analogueChannels"]).Count != 0)
                {
                    double[,] analogRead = analoguesReader.ReadMultiSample(voltageRead.Length);

                    for (int i = 0; i < analogRead.GetLength(0); i++)
                    {
                        if (analogRead.GetLength(1) > 0)
                        {
                            for (int j = 0; j < analogRead.GetLength(1); j++)
                            {
                                fastAnalogBuffer[i].Add(new Point(voltageRead[j], analogRead[i, j]));
                            }
                        }
                    }
                }
            }
        }

        public void FastAcquisitionFinishing()
        {
            Solstis.fast_scan_stop((string)Settings["fastScanType"], false);

            freqOutTask.Dispose();
            foreach (Task counterTask in counterTasks)
            {
                counterTask.Dispose();
            }
            analoguesTask.Dispose();
            voltageTask.Dispose();

            freqOutTask = null;
            counterTasks = null;
            analoguesTask = null;
            voltageTask = null;

            counterReaders = null;
            analoguesReader = null;
            voltageReader = null;

            fastState = FastScanState.stopped;
            backendState = SolsTisState.stopped;
        }

        private void FastOnData()
        {
            switch ((string)Settings["fast_channel_type"])
            {
                case "Counters":
                    if (FastData != null)
                    {
                        if ((int)Settings["fast_display_channel_index"] >= 0 && (int)Settings["fast_display_channel_index"] < ((List<string>)Settings["counterChannels"]).Count)
                        {
                            FastData(fastCounterBuffer[(int)Settings["fast_display_channel_index"]].Skip(1).ToArray());
                        }
                        else FastData(null);
                    }
                    break;

                case "Analogues":
                    if (FastData != null)
                    {
                        if ((int)Settings["fast_display_channel_index"] >= 0 && (int)Settings["fast_display_channel_index"] < ((List<string>)Settings["analogueChannels"]).Count)
                        {
                            FastData(fastAnalogBuffer[(int)Settings["fast_display_channel_index"]].Skip(1).ToArray());
                        }
                        else FastData(null);
                    }
                    break;

                default:
                    throw new Exception("Did not understand data type");
            }
        }

        private void FastOnScanFinished()
        {
            FastAcquisitionFinishing();
            if (FastScanFinished != null) FastScanFinished();
        }

        public void RequestFastHistoricData()
        {
            if (fastAnalogBuffer != null && fastCounterBuffer != null) FastOnData();
        }

        public void SaveFastData(string fileName, bool automatic)
        {
            string directory = Environs.FileSystem.GetDataDirectory((string)Environs.FileSystem.Paths["scanMasterDataPath"]);

            List<string> lines = new List<string>();
            lines.Add(DateTime.Today.ToString("dd-MM-yyyy") + " " + DateTime.Now.ToString("HH:mm:ss"));
            lines.Add("Exposure = " + TimeTracePlugin.GetController().GetExposure().ToString());
            lines.Add("Width = " + ((double)Settings["fastScanWidth"]).ToString() + ", Time = " + ((double)Settings["fastScanTime"]).ToString());

            string descriptionString = "Voltage ";
            foreach (string channel in (List<string>)Settings["counterChannels"])
            {
                descriptionString = descriptionString + channel + " ";
            }
            foreach (string channel in (List<string>)Settings["analogueChannels"])
            {
                descriptionString = descriptionString + channel + " ";
            }
            lines.Add(descriptionString);

            for (int i = 0; i < fastCounterBuffer[0].Count; i++)
            {
                string line = fastCounterBuffer[0][i].X.ToString() + " ";
                foreach (List<Point> counterData in fastCounterBuffer)
                {
                    line = line + counterData[i].Y.ToString() + " ";
                }
                foreach (List<Point> analogData in fastAnalogBuffer)
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

        #region TeraScan

        public bool TeraScanAcceptableSettings()
        {
            return true;
        }

        public void StartTeraScan()
        {
            switch ((string)Settings["TeraScanRepeatType"])
            {
                case "single":
                    StartSingleTeraScan();
                    break;
                case "multi":
                    StartMultiTeraScan();
                    break;
                default:
                    break;
            }
        }

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
            backendState = SolsTisState.running;

            for (int i = 0; i < ((double[])Settings["TeraScanMultiLambda"]).Length; i++)
            {
                if (teraState == TeraScanState.stepping && backendState == SolsTisState.stepping)
                {
                    teraState = TeraScanState.running;
                    backendState = SolsTisState.running;
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
                    while (backendState == SolsTisState.running && teraState == TeraScanState.running)
                    {
                        autoOutput = Solstis.ReceiveCustomMessage("automatic_output", true);
                        if (autoOutput.Count > 0)
                        {
                            object item;
                            autoOutput.TryGetValue("report", out item);
                            if (item != null)
                            {
                                teraState = TeraScanState.stepping;
                                backendState = SolsTisState.stepping;
                                break;
                            }
                            else
                            {
                                break;
                            }
                        }
                        Thread.Sleep(10);
                    }
                    if (backendState == SolsTisState.running && teraState == TeraScanState.running)
                    {
                        TeraScanSegmentStart();
                    }
                    else { break; }
                }

                if (teraLaser == TeraLaserState.stopped)
                {
                    int reply = Solstis.scan_stitch_op((string)Settings["TeraScanType"], "stop", false, true);
                }
                else
                {
                    int reply = Solstis.scan_stitch_op((string)Settings["TeraScanType"], "stop", false, false);
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

        public void StartSingleTeraScan()
        {
            //try
            //{
                if (IsRunning() || TimeTracePlugin.GetController().IsRunning() || FastMultiChannelRasterScan.GetController().IsRunning() || CounterOptimizationPlugin.GetController().IsRunning() || DFGPlugin.GetController().IsRunning())
                {
                    throw new DaqException("Counter already running");
                }

                teraState = TeraScanState.running;
                backendState = SolsTisState.running;
                
                TeraScanInitialise();
                teraScanBuffer = new TeraScanDataHolder(((List<string>)Settings["counterChannels"]).Count, ((List<string>)Settings["analogueChannels"]).Count, Settings);
                teraLatestLambda = (double)Settings["TeraScanStart"];
                TeraScanAcquisitionStarting();
                while (true)
                {
                    Dictionary<string, object> autoOutput = new Dictionary<string, object>();
                    while (backendState == SolsTisState.running && teraState == TeraScanState.running)
                    {
                        autoOutput = Solstis.ReceiveCustomMessage("automatic_output", true);
                        if (autoOutput.Count > 0)
                        {
                            object item;
                            autoOutput.TryGetValue("report", out item);
                            if (item != null)
                            {
                                teraState = TeraScanState.stopping;
                                backendState = SolsTisState.stopping;
                                break;
                            }
                            else
                            {
                                break;
                            }
                        }
                        Thread.Sleep(10);
                    }
                    if (backendState == SolsTisState.running && teraState == TeraScanState.running)
                    {
                        TeraScanSegmentStart();
                    }
                    else { break; }
                }
                TeraScanAcquisitionStopping();

            //}
            //catch (Exception e)
            //{
            //    if (TeraScanProblem != null) TeraScanProblem(e);
            //}
        }

        private void TeraScanInitialise()
        {
            Solstis.terascan_output("start", 0, 2, "on");

            string scanType = (string)Settings["TeraScanType"];
            double startLambda = (double)Settings["TeraScanStart"];
            double stopLambda = (double)Settings["TeraScanStop"];
            int scanRate = (int)Settings["TeraScanRate"];
            string scanUnits = (string)Settings["TeraScanUnits"];
            int reply = Solstis.scan_stitch_initialise(scanType, startLambda, stopLambda, scanRate, scanUnits);
        }

        private void TeraScanAcquisitionStarting()
        {
            // Turn on correct display
            teraScan_display_is_current_segment = (bool)Settings["tera_display_current_segment"];
            teraScan_current_channel_type = (string)Settings["tera_channel_type"];
            teraScan_current_display_channel_index = (int)Settings["tera_display_channel_index"];

            int reply = Solstis.scan_stitch_op((string)Settings["TeraScanType"], "start", true, false);
            switch (reply)
            {
                case 0:
                    return;
                default:
                    throw new Exception("Couldn't start tera scan");
            }
        }

        public void TeraScanAcquisitionStopping()
        {
            if (teraLaser == TeraLaserState.stopped)
            {
                int reply = Solstis.scan_stitch_op((string)Settings["TeraScanType"], "stop", false, true);
            }
            else
            {
                int reply = Solstis.scan_stitch_op((string)Settings["TeraScanType"], "stop", false, false);
                while (teraLaser != TeraLaserState.stopped)
                {
                    Thread.Sleep(100);
                }
            }

            teraState = TeraScanState.stopped;
            backendState = SolsTisState.stopped;
            if (TeraScanFinished != null) TeraScanFinished();
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
        }

        private void TeraScanSegmentAcquisitionStart()
        {
            freqOutTask.Start();
            bool isFirst = true;

            DateTime displayStartTime = DateTime.Now;
            DateTime displayCurrentTime = DateTime.Now;
            List<double> displayData = new List<double>();

            while (backendState == SolsTisState.running &&  teraSegmentState == TeraScanSegmentState.running)
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
                        if ((displayCurrentTime - displayStartTime).TotalMilliseconds > 100  && displayData.Count > 0)
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
            freqOutTask.Dispose();
            foreach (Task counterTask in counterTasks)
            {
                counterTask.Dispose();
            }
            analoguesTask.Dispose();

            freqOutTask = null;
            counterTasks = null;
            analoguesTask = null;

            counterReaders = null;
            analoguesReader = null;
        }

        private void TeraScanSegmentLaserStart()
        {
            teraLaser = TeraLaserState.running;
            string status = "scan";
            Solstis.terascan_continue();

            while (status == "scan")
            {
                Dictionary<string, object> autoOutput = Solstis.ReceiveCustomMessage("automatic_output", true);

                if (autoOutput.Count != 0)
                {
                    try
                    {
                        teraLatestLambda = Convert.ToDouble(autoOutput["wavelength"]);
                        status = (string)autoOutput["status"];
                        if (backendState != SolsTisState.running || teraState != TeraScanState.running)
                        {
                            teraLaser = TeraLaserState.stopping;
                        }
                    }
                    catch (KeyNotFoundException)
                    {
                        teraLaser = TeraLaserState.stopping;
                    }
                }
                else
                {
                    continue;
                }
            }

            if (status != "end")
            {
                teraSegmentState = TeraScanSegmentState.unfinished;
                MessageBox.Show(status);
            }
            else { teraLaser = TeraLaserState.stopped; teraSegmentState = TeraScanSegmentState.stopped; }
        }

        private void TeraScanSegmentStart()
        {
            teraLatestLambda = -1;
            teraScanBuffer.AddNewSegment();
            TeraScanSegmentAcquisitionStarting();
            teraSegmentState = TeraScanSegmentState.running;
            Thread thread = new Thread(new ThreadStart(TeraScanSegmentLaserStart));
            thread.IsBackground = true;
            thread.Start();
            while (teraLatestLambda < 0) { Thread.Sleep(1); }
            TeraScanSegmentAcquisitionStart();
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
                        teraScanDisplaySegmentWaveform  = teraScanBuffer.GetCurrentSegment().GetCounterData(index).ToArray();
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
            SegmentDataHolder segment = SolsTiSPlugin.GetController().teraScanBuffer.GetSegment(segmentIndex);
            int index;
            switch ((string)SolsTiSPlugin.GetController().Settings["tera_channel_type"])
            {
                case "Counters":
                    index = (int)SolsTiSPlugin.GetController().Settings["tera_display_channel_index"];
                    if (index >= 0 && index < ((List<string>)SolsTiSPlugin.GetController().Settings["counterChannels"]).Count)
                    {
                        TeraSegmentOnlyData(segment.GetCounterData(index).ToArray(), true);
                    }
                    else { TeraSegmentOnlyData(new double[] { }, true); }
                    break;

                case "Analogues":
                    index = (int)SolsTiSPlugin.GetController().Settings["tera_display_channel_index"];
                    if (index >= 0 && index < ((List<string>)SolsTiSPlugin.GetController().Settings["analogueChannels"]).Count)
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
            //try
            //{
                if (IsRunning() || TimeTracePlugin.GetController().IsRunning() || FastMultiChannelRasterScan.GetController().IsRunning() || CounterOptimizationPlugin.GetController().IsRunning() || DFGPlugin.GetController().IsRunning())
                {
                    throw new DaqException("Daq already running");
                }

                tripletState = TripletScanState.running;
                backendState = SolsTisState.running;

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
            //}
            //catch (Exception e)
            //{
            //    if (TripletScanProblem != null) TripletScanProblem(e);
            //}
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
                        tripletAnalogBuffer = new double[((List<string>)tripletHistoricSettings["analogueChannels"]).Count,pointsPerExposure];
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
            backendState = SolsTisState.stopped;
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

    }

    public class MultiChannelLineDataStore
    {
        protected int numberCounterChannels;
        protected int numberAnalogChannels;
        protected List<double>[] counterDataStore;
        protected List<double>[] analogDataStore;

        public MultiChannelLineDataStore(int number_of_counter_channels, int number_of_analog_channels)
        {
            numberCounterChannels = number_of_counter_channels;
            numberAnalogChannels = number_of_analog_channels;

            counterDataStore = new List<double>[number_of_counter_channels];
            for (int i = 0; i < number_of_counter_channels; i++)
            {
                counterDataStore[i] = new List<double>();
            }

            analogDataStore = new List<double>[number_of_analog_channels];
            for (int i = 0; i < number_of_analog_channels; i++)
            {
                analogDataStore[i] = new List<double>();
            }
        }

        public List<double> GetCounterData(int counter_channel_number)
        {
            if (counter_channel_number >= numberCounterChannels) return null;
            else return counterDataStore[counter_channel_number];
        }

        protected void SetCounterData(List<double>[] counterStore)
        {
            counterDataStore = counterStore;
        }

        public void AddtoCounterData(int counter_channel_number, double pnt)
        {
            counterDataStore[counter_channel_number].Add(pnt);
        }

        public List<double> GetAnalogueData(int analog_channel_number)
        {
            if (analog_channel_number >= numberAnalogChannels) return null;
            else return analogDataStore[analog_channel_number];
        }

        protected void SetAnalogueData(List<double>[] analogStore)
        {
            analogDataStore = analogStore;
        }

        public void AddtoAnalogueData(int analog_channel_number, double pnt)
        {
            analogDataStore[analog_channel_number].Add(pnt);
        }

        public int Count()
        {
            if (numberCounterChannels != 0) return counterDataStore[0].Count;
            else if (numberAnalogChannels != 0) return analogDataStore[0].Count;
            else return 0;
        }
    }

    public class SegmentDataHolder : MultiChannelLineDataStore
    {
        private List<double> wavelengthDataStore;

        public SegmentDataHolder(int number_of_counter_channels, int number_of_analog_channels)
            : base(number_of_counter_channels, number_of_analog_channels)
        {
            wavelengthDataStore = new List<double>();
        }

        public List<double> GetWavelengthData()
        {
            return wavelengthDataStore;
        }

        protected void SetWavelengthData(List<double> wavelengthStore)
        {
            wavelengthDataStore = wavelengthStore;
        }

        public void AddtoWavelengthData(double pnt)
        {
            wavelengthDataStore.Add(pnt);
        }
    }

    public class TeraScanDataHolder
    {
        private int _numberCounterChannels;
        public int numberCounterChannels { get { return _numberCounterChannels; } }
        private int _numberAnalogChannels;
        public int numberAnalogChannels { get {return _numberAnalogChannels; } }
        private List<SegmentDataHolder> segments;
        private int currentIndex;
        public int currentSegmentIndex { get { return currentIndex; } }
        public double[] latestCounters;
        private List<double> historicLambdaData;
        private List<double>[] historicCounterData;
        private List<double>[] historicAnalogueData;
        protected Hashtable settings;
        public Hashtable historicSettings { get { return settings; } }

        public TeraScanDataHolder(int number_of_counter_channels, int number_of_analog_channels, PluginSettings currentSettings)
        {
            _numberCounterChannels = number_of_counter_channels;
            _numberAnalogChannels = number_of_analog_channels;
            segments = new List<SegmentDataHolder>();
            currentIndex = -1;
            historicLambdaData = new List<double>();
            historicCounterData = new List<double>[_numberCounterChannels];
            for (int i = 0; i < _numberCounterChannels; i++)
            {
                historicCounterData[i] = new List<double>();
            }
            historicAnalogueData = new List<double>[_numberAnalogChannels];
            for (int i = 0; i < _numberAnalogChannels; i++)
            {
                historicAnalogueData[i] = new List<double>();
            }
            settings = new Hashtable();
            foreach (string key in currentSettings.Keys)
            {
                settings[key] = currentSettings[key];
            }
            settings["sampleRate"] = (double)TimeTracePlugin.GetController().Settings["sampleRate"];
        }

        public SegmentDataHolder GetSegment(int index)
        {
            return segments[index];
        }

        public SegmentDataHolder GetCurrentSegment()
        {
            return segments[currentIndex];
        }

        public void AddNewSegment()
        {
            SegmentDataHolder segment = new SegmentDataHolder(_numberCounterChannels, _numberAnalogChannels);
            segments.Add(segment);
            currentIndex++;
            latestCounters = new double[_numberCounterChannels];
            for (int i = 0; i < numberCounterChannels; i++)
            {
                latestCounters[i] = 0;
            }
        }

        public void AddLambdaDataToCurrentSegment(double pnt)
        {
            segments[currentIndex].AddtoWavelengthData(pnt);
            historicLambdaData.Add(pnt);
        }

        public List<double> GetLambdaData()
        {
            return historicLambdaData;
        }

        public void AddCounterDataToCurrentSegment(int counter_channel_number, double pnt)
        {
            segments[currentIndex].AddtoCounterData(counter_channel_number, pnt);
            historicCounterData[counter_channel_number].Add(pnt);
        }

        public List<double> GetCounterData(int index)
        {
            if (index >= 0 && index < _numberCounterChannels) { return historicCounterData[index]; }
            else { return null; }
        }

        public void AddAnalogueDataToCurrentSegment(int analog_channel_number, double pnt)
        {
            segments[currentIndex].AddtoAnalogueData(analog_channel_number, pnt);
            historicAnalogueData[analog_channel_number].Add(pnt);
        }

        public List<double> GetAnalogueData(int index)
        {
            if (index >= 0 && index < _numberAnalogChannels) { return historicAnalogueData[index]; }
            else { return null; }
        }
    }

}
