using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using Microsoft.Win32;

using NationalInstruments;
using NationalInstruments.DAQmx;
using DAQ.Environment;
using DAQ.HAL;

namespace ConfocalControl
{
    public delegate void SpectraScanDataEventHandler(Point[] data);
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

        // Define Opt state
        private enum SolsTisState { stopped, running, stopping };
        private SolsTisState backendState = SolsTisState.stopped;
        private enum WavemeterScanState { stopped, running, stopping };
        private WavemeterScanState wavemeterState = WavemeterScanState.stopped;
        private enum FastScanState { stopped, running, stopping };
        private FastScanState fastState = FastScanState.stopped;
        private enum TeraScanState { stopped, running, stopping };
        private TeraScanState teraState = TeraScanState.stopped;

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

        public event SpectraScanDataEventHandler TeraData;
        public event MultiChannelScanFinishedEventHandler TeraScanFinished;
        public event SpectraScanExceptionEventHandler TeraScanProblem;

        // Constants relating to sample acquisition
        private int MINNUMBEROFSAMPLES = 10;
        private double TRUESAMPLERATE = 1000;
        private int pointsPerExposure;
        private double sampleRate;

        // Keep track of data
        private List<Point>[] wavemeterAnalogBuffer;
        private List<Point>[] wavemeterCounterBuffer;
        private List<Point>[] fastAnalogBuffer;
        private List<Point>[] fastCounterBuffer;
        private double[] fastLatestCounters;
        private List<Point> teraScanWavelengthBuffer;

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

        #endregion

        #region Initialization

        public void LoadSettings()
        {
            Settings = PluginSaveLoad.LoadSettings("solstis");
        }

        public SolsTiSPlugin()
        {
            string computer_ip = "192.168.1.23";
            solstis = new ICEBlocSolsTiS(computer_ip);

            LoadSettings();
            if (Settings.Keys.Count != 22)
            {
                Settings["wavelength"] = 785.0;

                Settings["wavemeterScanStart"] = 784.0;
                Settings["wavemeterScanStop"] = 785.0;
                Settings["wavemeterScanPoints"] = 100;

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

                Settings["TeraScanType"] = "fine";
                Settings["TeraScanStart"] = (double)743.0;
                Settings["TeraScanStop"] = (double)745.0;
                Settings["TeraScanRate"] = 10;
                Settings["TeraScanUnits"] = "GHz/s";

                return;
            }

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
            return SolsTiSPlugin.GetController().Solstis.set_wave_m(wavelength, true);
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
                if (IsRunning() || TimeTracePlugin.GetController().IsRunning() || FastMultiChannelRasterScan.GetController().IsRunning() || CounterOptimizationPlugin.GetController().IsRunning())
                {
                    throw new DaqException("Counter already running");
                }

                wavemeterState = WavemeterScanState.running;
                backendState = SolsTisState.running;
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

                double inputRangeLow = 0;
                double inputRangeHigh = 1;

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
                        if ((int)Settings["wavemeter_display_channel_index"] >= 0 && (int)Settings["wavemeter_display_channel_index"] < ((List<string>)Settings["counterChannels"]).Count)
                        {
                            WavemeterData(wavemeterCounterBuffer[(int)Settings["wavemeter_display_channel_index"]].ToArray());
                        }
                        else WavemeterData(null);
                    }
                    break;

                case "Analogues":
                    if (WavemeterData != null)
                    {
                        if ((int)Settings["wavemeter_display_channel_index"] >= 0 && (int)Settings["wavemeter_display_channel_index"] < ((List<string>)Settings["analogueChannels"]).Count)
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
            lines.Add("Exposure = " + TimeTracePlugin.GetController().GetExposure().ToString());
            lines.Add("Lambda start = " + ((double)Settings["wavemeterScanStart"]).ToString() + ", Lambda stop = " + ((double)Settings["wavemeterScanStop"]).ToString() + ", Lambda resolution = " + ((int)Settings["wavemeterScanPoints"]).ToString());

            string descriptionString = "Lambda ";
            foreach (string channel in (List<string>)Settings["counterChannels"])
            {
                descriptionString = descriptionString + channel + " ";
            }
            foreach (string channel in (List<string>)Settings["analogueChannels"])
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
                if (IsRunning() || TimeTracePlugin.GetController().IsRunning() || FastMultiChannelRasterScan.GetController().IsRunning() || CounterOptimizationPlugin.GetController().IsRunning())
                {
                    throw new DaqException("Counter already running");
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
                    SolsTiSPlugin.GetController().Solstis.monitor_a(2, true);
                    break;

                case "resonator_continuous":
                    SolsTiSPlugin.GetController().Solstis.monitor_a(6, true);
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
            int reply = SolsTiSPlugin.GetController().Solstis.fast_scan_start((string)Settings["fastScanType"], (double)Settings["fastScanWidth"], (double)Settings["fastScanTime"], true);

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
            SolsTiSPlugin.GetController().Solstis.fast_scan_stop((string)Settings["fastScanType"], false);

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
            try
            {
                if (IsRunning() || TimeTracePlugin.GetController().IsRunning() || FastMultiChannelRasterScan.GetController().IsRunning() || CounterOptimizationPlugin.GetController().IsRunning())
                {
                    throw new DaqException("Counter already running");
                }

                teraState = TeraScanState.running;
                backendState = SolsTisState.running;

                TeraScanConfigure();
                // TeraScanAcquisitionStarting();
                TeraScanSynchronousAcquire();
            }
            catch (Exception e)
            {
                if (TeraScanProblem != null) TeraScanProblem(e);
            }
        }

        public void TeraScanConfigure()
        {
            SolsTiSPlugin.GetController().Solstis.terascan_output("start", 0, 50, "on");

            string scanType = (string)Settings["TeraScanType"];
            double startLambda = (double)Settings["TeraScanStart"];
            double stopLambda = (double)Settings["TeraScanStop"];
            int scanRate = (int)Settings["TeraScanRate"];
            string scanUnits = (string)Settings["TeraScanUnits"];
            SolsTiSPlugin.GetController().Solstis.scan_stitch_initialise(scanType, startLambda, stopLambda, scanRate, scanUnits);
        }

        public void TeraScanAcquisitionStarting()
        {
            return;
        }

        public void TeraScanSynchronousAcquire()
        {
            SolsTiSPlugin.GetController().Solstis.scan_stitch_op("fine", "start", false);

            Dictionary<string, object> reply = SolsTiSPlugin.GetController().Solstis.ReceiveCustomMessage("automatic_output");
            double wavelength = (double)reply["wavelength"];
            string status = (string)reply["status"];
            MessageBox.Show(status);
            MessageBox.Show(Convert.ToString(wavelength));

            SolsTiSPlugin.GetController().Solstis.terascan_continue();

        }

        #endregion

    }

    public class TeraScanDataHolder
    {

    }

    public class SegmentDataHolder 
    {

    }
}
