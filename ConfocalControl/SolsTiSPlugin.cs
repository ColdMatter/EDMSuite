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
    public delegate void WavemeterScanDataEventHandler(Point[] data);
    public delegate void WavemeterScanExceptionEventHandler(Exception e);

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
        private enum ScanState { stopped, running, stopping };
        private ScanState backendState = ScanState.stopped;

        // Settings
        public PluginSettings Settings { get; set; }

        // Laser
        private ICEBlocSolsTiS solstis;
        public ICEBlocSolsTiS Solstis { get { return solstis; } }

        // Bound event managers to class
        public event WavemeterScanDataEventHandler Data;
        public event MultiChannelScanFinishedEventHandler ScanFinished;
        public event WavemeterScanExceptionEventHandler WavemeterScanProblem;

        // Constants relating to sample acquisition
        private int MINNUMBEROFSAMPLES = 10;
        private double TRUESAMPLERATE = 1000;
        private int pointsPerExposure;
        private double sampleRate;

        // Keep track of data
        private List<Point>[] analogBuffer;
        private List<Point>[] counterBuffer;

        // Keep track of tasks
        private Task triggerTask;
        private DigitalSingleChannelWriter triggerWriter;
        private Task freqOutTask;
        private List<Task> counterTasks;
        private List<CounterSingleChannelReader> counterReaders;
        private Task analoguesTask;
        private AnalogMultiChannelReader analoguesReader;

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
            if (Settings.Keys.Count != 9)
            {
                Settings["wavelength"] = 785.0;

                Settings["wavemeterScanStart"] = 784.0;
                Settings["wavemeterScanStop"] = 785.0;
                Settings["wavemeterScanPoints"] = 100;

                Settings["counterChannels"] = new List<string> { "APD0", "APD1" };
                Settings["analogueChannels"] = new List<string> { };
                Settings["analogueLowHighs"] = new Dictionary<string, double[]>();

                Settings["channel_type"] = "Counters";
                Settings["display_channel_index"] = 0;
                return;
            }

            triggerTask = null;
            freqOutTask = null;
            counterTasks = null;
            analoguesTask = null;

            triggerWriter = null;
            counterReaders = null;
            analoguesReader = null;
        }

        #endregion

        #region Wavemeter Scan

        private int SetAndLockWavelength(double wavelength)
        {
            return SolsTiSPlugin.GetController().Solstis.move_wave_t(wavelength, true);

            //Dictionary<string, object> reply = SolsTiSPlugin.GetController().Solstis.poll_wave_m();

            //if (reply.Count == 0)
            //{
            //    throw new Exception("poll_wave_m: empty reply");
            //}
            //else
            //{
            //    switch ((int)reply["status"])
            //    {
            //        case 0:
            //            throw new Exception("poll_wave_m: tuning software not active");

            //        case 1:
            //            throw new Exception("poll_wave_m: no link to wavelength meter or no meter configured");

            //        case 2:
            //            throw new Exception("poll_wave_m: tuning in progress");

            //        case 3:
            //            return SolsTiSPlugin.GetController().Solstis.set_wave_m(wavelength, true);

            //        default:
            //            throw new Exception("poll_wave_m: did not understand reply");
            //    }
            //}
        }

        public bool AcceptableSettings()
        {
            if ((double)Settings["wavemeterScanStart"] >= (double)Settings["wavemeterScanStop"] || (int)Settings["wavemeterScanPoints"] < 1)
            {
                MessageBox.Show("Galvo X settings unacceptable.");
                return false;
            }
            else
            {
                return true;
            }
        }

        private void CalculateParameters()
        {
            double _sampleRate = (double)SingleCounterPlugin.GetController().Settings["sampleRate"];
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

        public void SynchronousStartScan()
        {
            //try
            //{
                if (IsRunning() || SingleCounterPlugin.GetController().IsRunning() || FastMultiChannelRasterScan.GetController().IsRunning() || CounterOptimizationPlugin.GetController().IsRunning())
                {
                    throw new DaqException("Counter already running");
                }

                backendState = ScanState.running;
                SynchronousAcquisitionStarting();
                SynchronousAcquire();
            //}
            //catch (Exception e)
            //{
            //    if (WavemeterScanProblem != null) WavemeterScanProblem(e);
            //}
        }

        private void SynchronousAcquisitionStarting()
        {
            analogBuffer = new List<Point>[((List<string>)Settings["analogueChannels"]).Count];
            for (int i = 0; i < ((List<string>)Settings["analogueChannels"]).Count; i++)
            {
                analogBuffer[i] = new List<Point>();
            }
            counterBuffer = new List<Point>[((List<string>)Settings["counterChannels"]).Count];
            for (int i = 0; i < ((List<string>)Settings["counterChannels"]).Count; i++)
            {
                counterBuffer[i] = new List<Point>();
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

                double inputRangeLow = ((List<double[]>)Settings["analogueLowHighs"])[i][0];
                double inputRangeHigh = ((List<double[]>)Settings["analogueLowHighs"])[i][1];

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

        private void SynchronousAcquire()
        // Main method for looping over scan parameters, aquiring scan outputs and connecting to controller for display
        {
            // Go to start of scan
            int report = SetAndLockWavelength((double)Settings["wavemeterScanStart"]);
            if (report == 1) throw new Exception("set_wave_m start: task failed");

            // Main loop
            for (double i = 0;
                    i < (int)Settings["wavemeterScanPoints"] + 1;
                    i++)

            {
                double currentWavelength = (double)Settings["wavemeterScanStart"] + i * 
                    ((double)Settings["wavemeterScanStop"] - (double)Settings["wavemeterScanStart"]) /
                    (int)Settings["wavemeterScanPoints"];

                DateTime dt1 = DateTime.Now;
                report = SetAndLockWavelength(currentWavelength);
                DateTime dt2 = DateTime.Now;
                TimeSpan span = dt2 - dt1;

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
                        Point pnt = new Point(currentWavelength, span.TotalMilliseconds);
                        counterBuffer[j].Add(pnt);
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
                            analogBuffer[j].Add(pnt);
                        }
                    }

                    OnData();

                    // Check if scan exit.
                    if (CheckIfStopping())
                    {
                        // Quit plugins
                        AcquisitionFinishing();
                        return;
                    }
                }
            }

            AcquisitionFinishing();
            OnScanFinished();
        }

        public void AcquisitionFinishing()
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

            backendState = ScanState.stopped;
        }

        private void OnData()
        {
            switch ((string)Settings["channel_type"])
            {
                case "Counters":
                    if (Data != null)
                    {
                        if ((int)Settings["display_channel_index"] >= 0 && (int)Settings["display_channel_index"] < ((List<string>)Settings["counterChannels"]).Count)
                        {
                            Data(counterBuffer[(int)Settings["display_channel_index"]].ToArray());
                        }
                        else Data(null);
                    }
                    break;

                case "Analogues":
                    if (Data != null)
                    {
                        if ((int)Settings["display_channel_index"] >= 0 && (int)Settings["display_channel_index"] < ((List<string>)Settings["analogueChannels"]).Count)
                        {
                            Data(analogBuffer[(int)Settings["display_channel_index"]].ToArray());
                        }
                        else Data(null);
                    }
                    break;

                default:
                    throw new Exception("Did not understand data type");
            }
        }

        private void OnScanFinished()
        {
            if (ScanFinished != null) ScanFinished();
            int report = SetAndLockWavelength((double)Settings["wavemeterScanStart"]);
            if (report == 1) throw new Exception("set_wave_m start: task failed");
        }

        public bool IsRunning()
        {
            return backendState == ScanState.running;
        }

        private bool CheckIfStopping()
        {
            return backendState == ScanState.stopping;
        }

        public void RequestHistoricData()
        {
            if (analogBuffer != null && counterBuffer != null) OnData();
        }

        public void SaveData(string fileName, bool automatic)
        {
            string directory = Environs.FileSystem.GetDataDirectory((string)Environs.FileSystem.Paths["scanMasterDataPath"]);

            List<string> lines = new List<string>();
            lines.Add(DateTime.Today.ToString("dd-MM-yyyy") + " " + DateTime.Now.ToString("HH:mm:ss"));
            lines.Add("Exposure = " + SingleCounterPlugin.GetController().GetExposure().ToString());
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

            for (int i = 0; i < counterBuffer[0].Count; i++)
            {
                string line = counterBuffer[0][i].X.ToString() + " ";
                foreach (List<Point> counterData in counterBuffer)
                {
                    line = line + counterData[i].Y.ToString() + " ";
                }
                foreach (List<Point> analogData in analogBuffer)
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

    }

}
