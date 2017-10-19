using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media.Media3D;
using System.Threading;

using NationalInstruments;
using NationalInstruments.DAQmx;
using DAQ.Environment;

using DAQ.HAL;

namespace ConfocalControl
{
    // Uses delegate multicasting to compose and invoke event manager methods in series 
    public delegate void MultiChannelDataEventHandler(MultiChannelData ps);
    public delegate void MultiChannelScanFinishedEventHandler();
    public delegate void MultiChannelLineFinishedEventHandler(MultiChannelData ps);

    public class MultiChannelRasterScan
    {
        #region Class members

        // Dependencies should refer to this instance only 
        private static MultiChannelRasterScan controllerInstance;
        public static MultiChannelRasterScan GetController()
        {
            if (controllerInstance == null)
            {
                controllerInstance = new MultiChannelRasterScan();
            }
            return controllerInstance;
        }

        // Settings
        private PluginSettings _rasterScanSettings = new PluginSettings("multiChannelConfocalScan");
        public PluginSettings scanSettings
        {
            get { return _rasterScanSettings; }
            set { _rasterScanSettings = value; }
        }

        // Galvo controller
        GalvoPairPlugin galvoPair;

        // Bound event managers to class
        public event MultiChannelDataEventHandler Data;
        public event MultiChannelLineFinishedEventHandler LineFinished;
        public event MultiChannelScanFinishedEventHandler ScanFinished;
        public event DaqExceptionEventHandler DaqProblem;

        // Define RasterScan state
        private enum RasterScanState { stopped, running, stopping };
        private RasterScanState backendState = RasterScanState.stopped;

        // Keeping track of data
        List<double[]> counterLatestData;
        private double[,] analogLatestData;
        private MultiChannelData dataOutputs;
        private MultiChannelData dataLines;

        // Keep track of tasks
        private Task triggerTask;
        private DigitalWaveform triggerWaveform;
        private DigitalSingleChannelWriter triggerWriter;
        private Task freqOutTask;
        private List<Task> counterTasks;
        private List<CounterSingleChannelReader> counterReaders;
        private Task analoguesTask;
        private AnalogMultiChannelReader analoguesReader;

        #endregion

        #region Init

        private void InitialiseSettings()
        {
            //_rasterScanSettings.LoadSettings();

            if (_rasterScanSettings.Keys.Count == 0)
            {
                _rasterScanSettings["GalvoXStart"] = (double)0;
                _rasterScanSettings["GalvoXEnd"] = (double)1;
                _rasterScanSettings["GalvoXRes"] = (double)21;
                _rasterScanSettings["GalvoYStart"] = (double)0;
                _rasterScanSettings["GalvoYEnd"] = (double)1;
                _rasterScanSettings["GalvoYRes"] = (double)21;

                _rasterScanSettings["counterChannels"] = new List<string> {"APD0", "APD1" };
                _rasterScanSettings["analogueChannels"] = new List<string> {"AI0","AI1"};
                _rasterScanSettings["analogueLowHighs"] = new List<double[]> { new double[] { -5, 5 }, new double[] { -5, 5 } };
                _rasterScanSettings["pointsPerExposure"] = (double)10;

                return;
            }
        }

        public MultiChannelRasterScan() 
        {
            InitialiseSettings();

            counterLatestData = null;
            analogLatestData = null;

            triggerTask = null;
            freqOutTask = null;
            counterTasks = null;
            analoguesTask = null;

            triggerWriter = null;
            counterReaders = null;
            analoguesReader = null;
        }

        #endregion

        #region Synchronous methods

        public void SynchronousStartScan()
        {
            try
            {
                dataOutputs = new MultiChannelData(((List<string>)scanSettings["counterChannels"]).Count,
                                                    ((List<string>)scanSettings["analogueChannels"]).Count);

                backendState = RasterScanState.running;
                SynchronousAcquisitionStarting();
                SynchronousAcquire();
            }
            catch (DaqException e)
            {
                if (DaqProblem != null) DaqProblem(e);
            }
        }

        private void SynchronousAcquisitionStarting()
        {
            // Define sample rate 
            double exposure = 1 / (double)SingleCounterPlugin.GetController().Settings["sampleRate"];
            double sample_rate = (double)scanSettings["pointsPerExposure"] / exposure;

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
                sample_rate,
                0.5);

            freqOutTask.Triggers.StartTrigger.ConfigureDigitalEdgeTrigger(
                (string)Environs.Hardware.GetInfo("StartTriggerReader"),
                DigitalEdgeStartTriggerEdge.Rising);

            freqOutTask.Triggers.StartTrigger.Retriggerable = true;


            freqOutTask.Timing.ConfigureImplicit(SampleQuantityMode.FiniteSamples, Convert.ToInt32((double)scanSettings["pointsPerExposure"]) + 1);

            freqOutTask.Control(TaskAction.Verify);

            freqOutTask.Start();

            // Set up edge-counting tasks
            counterTasks = new List<Task>();
            counterReaders = new List<CounterSingleChannelReader>();

            for (int i = 0; i < ((List<string>)scanSettings["counterChannels"]).Count; i++)
            {
                string channelName = ((List<string>)scanSettings["counterChannels"])[i];

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
                    sample_rate,
                    SampleClockActiveEdge.Rising,
                    SampleQuantityMode.ContinuousSamples);

                counterTasks[i].Control(TaskAction.Verify);

                DaqStream counterStream = counterTasks[i].Stream;
                //counterStream.ReadOverwriteMode = ReadOverwriteMode.OverwriteUnreadSamples;
                counterReaders.Add(new CounterSingleChannelReader(counterStream));

                // Start tasks
                counterTasks[i].Start();
            }

            // Set up analogue sampling tasks
            analoguesTask = new Task("analogue sampler");

            for (int i = 0; i < ((List<string>)scanSettings["analogueChannels"]).Count; i++)
            {
                string channelName = ((List<string>)scanSettings["analogueChannels"])[i];

                double inputRangeLow = ((List<double[]>)scanSettings["analogueLowHighs"])[i][0];
                double inputRangeHigh = ((List<double[]>)scanSettings["analogueLowHighs"])[i][1];

                ((AnalogInputChannel)Environs.Hardware.AnalogInputChannels[channelName]).AddToTask(
                    analoguesTask,
                    inputRangeLow,
                    inputRangeHigh
                    );
            }

            if (((List<string>)scanSettings["analogueChannels"]).Count != 0)
            {
                analoguesTask.Timing.ConfigureSampleClock(
                    (string)Environs.Hardware.GetInfo("SampleClockReader"),
                    sample_rate,
                    SampleClockActiveEdge.Rising,
                    SampleQuantityMode.ContinuousSamples);

                analoguesTask.Control(TaskAction.Verify);

                DaqStream analogStream = analoguesTask.Stream;
                //analogStream.ReadOverwriteMode = ReadOverwriteMode.OverwriteUnreadSamples;
                analoguesReader = new AnalogMultiChannelReader(analogStream);

                // Start tasks
                analoguesTask.Start();
            }

            // Start Galvos
            GalvoPairPlugin.GetController().MoveOnlyAcquisitionStarting();
        }

        private void SynchronousAcquire()
        // Main method for looping over scan parameters, aquiring scan outputs and connecting to controller for display
        {
             // Move to the start of the scan.
            GalvoPairPlugin.GetController().SetGalvoXSetpointAndWait(
                         (double)scanSettings["GalvoXStart"], null, null);

            GalvoPairPlugin.GetController().SetGalvoYSetpointAndWait(
                         (double)scanSettings["GalvoYStart"], null, null);

            // Snake the raster
            bool inverted = true;

            // Loop for X axis
            for (double YNumber = 0;
                    YNumber < (double)scanSettings["GalvoYRes"];
                    YNumber++)
            {

                if (inverted) inverted = false;
                else inverted = true;

                dataLines = new MultiChannelData(((List<string>)scanSettings["counterChannels"]).Count,
                    ((List<string>)scanSettings["analogueChannels"]).Count);

                // Calculate new Y galvo point from current scan point 
                double currentGalvoYpoint = (double)scanSettings["GalvoYStart"] + YNumber *
                        ((double)scanSettings["GalvoYEnd"] -
                        (double)scanSettings["GalvoYStart"]) /
                        ((double)scanSettings["GalvoYRes"] - 1);

                // Move Y galvo to new scan point
                GalvoPairPlugin.GetController().SetGalvoYSetpointAndWait(currentGalvoYpoint, null, null);

                // Loop for X axis
                for (double _XNumber = 0;
                        _XNumber < (double)scanSettings["GalvoXRes"];
                        _XNumber++)
                {
                    double XNumber;
                    if (inverted) XNumber = (double)scanSettings["GalvoXRes"] - 1 - _XNumber;
                    else XNumber = _XNumber;

                    // Calculate new Y galvo point from current scan point 
                    double currentGalvoXpoint = (double)scanSettings["GalvoXStart"] + XNumber *
                    ((double)scanSettings["GalvoXEnd"] -
                    (double)scanSettings["GalvoXStart"]) /
                    ((double)scanSettings["GalvoXRes"] - 1);

                    // Move X galvo to new scan point 
                    GalvoPairPlugin.GetController().SetGalvoXSetpointAndWait(currentGalvoXpoint, null, null);

                    // Start trigger task
                    triggerWriter.WriteSingleSampleSingleLine(true, true);

                    // Read counter data
                    counterLatestData = new List<double[]>();
                    foreach (CounterSingleChannelReader counterReader in counterReaders)
                    {
                        counterLatestData.Add(counterReader.ReadMultiSampleDouble(Convert.ToInt32((double)scanSettings["pointsPerExposure"]) + 1));
                    }

                    // Read analogue data
                    if (((List<string>)scanSettings["analogueChannels"]).Count != 0)
                    {
                        analogLatestData = analoguesReader.ReadMultiSample(Convert.ToInt32((double)scanSettings["pointsPerExposure"]) + 1);
                    }

                    // re-init the trigger 
                    triggerWriter.WriteSingleSampleSingleLine(true, false);

                    // Store counter data

                    for (int i = 0; i < counterLatestData.Count; i++)
                    {
                        double[] latestData = counterLatestData[i];
                        double data = latestData[latestData.Length - 1] - latestData[0];
                        Point3D pnt = new Point3D(XNumber + 1, YNumber + 1, data);
                        Point3D pntLine = new Point3D(XNumber + 1, data, 0);
                        dataLines.AddtoCounterData(i, pntLine);
                        dataOutputs.AddtoCounterData(i, pnt);
                    }

                    // Store analogue data
                    for (int i = 0; i < analogLatestData.GetLength(0); i++)
                    {
                        double sum = 0;
                        for (int j = 0; j < analogLatestData.GetLength(1) - 1; j++)
                        {
                            sum = sum + analogLatestData[i, j];
                        }
                        double average = sum / (analogLatestData.GetLength(1) - 1);
                        Point3D pnt = new Point3D(XNumber + 1, YNumber + 1, average);
                        Point3D pntLine = new Point3D(XNumber + 1, average, 0);
                        dataLines.AddtoAnalogueData(i, pntLine);
                        dataOutputs.AddtoAnalogueData(i, pnt);
                    }

                    OnData(dataOutputs);

                    // Check if scan exit.
                    if (CheckIfStopping())
                    {
                        // Quit plugins
                        AcquisitionFinishing();
                        return;
                    }
                }

                OnLineFinished(dataLines);
            }

            AcquisitionFinishing();
            OnScanFinished();
        }

        #endregion

        #region Other methods

        public void StopScan()
        {
            backendState = RasterScanState.stopping;
        }

        public void AcquisitionFinishing()
        {
            GalvoPairPlugin.GetController().MoveOnlyAcquisitionFinished();

            triggerTask.Dispose();
            freqOutTask.Dispose();
            foreach (Task counterTask in counterTasks)
            {
                counterTask.Dispose();
            }
            analoguesTask.Dispose();

            counterLatestData = null;
            analogLatestData = null;

            triggerTask = null;
            freqOutTask = null;
            counterTasks = null;
            analoguesTask = null;

            triggerWriter = null;
            counterReaders = null;
            analoguesReader = null;

            backendState = RasterScanState.stopped;
        }

        private bool CheckIfStopping()
        {
            if (backendState == RasterScanState.stopping) return true;
            else return false;
        }

        public bool IsRunning()
        {
            return backendState == RasterScanState.running;
        }

        private void OnData(MultiChannelData dat)
        {
            if (Data != null) Data(dat);
        }

        private void OnLineFinished(MultiChannelData dat)
        {
            if (LineFinished != null) LineFinished(dat);
        }

        private void OnScanFinished()
        {
            if (ScanFinished != null) ScanFinished();
            GalvoPairPlugin.GetController().AcquisitionStarting();
            GalvoPairPlugin.GetController().SetGalvoXSetpoint((double)scanSettings["GalvoXStart"]);
            GalvoPairPlugin.GetController().SetGalvoYSetpoint((double)scanSettings["GalvoYStart"]);
            GalvoPairPlugin.GetController().AcquisitionFinished();
        }

        public bool AcceptableSettings()
        {
            if ((double)scanSettings["GalvoXStart"] >= (double)scanSettings["GalvoXEnd"] || (double)scanSettings["GalvoXRes"] < 1)
            {
                MessageBox.Show("Galvo X settings unacceptable.");
                return false;
            }
            else if ((double)scanSettings["GalvoYStart"] >= (double)scanSettings["GalvoYEnd"] || (double)scanSettings["GalvoYRes"] < 1)
            {
                MessageBox.Show("Galvo Y settings unacceptable.");
                return false;
            }
            else if ((double)scanSettings["pointsPerExposure"] < 1)
            {
                MessageBox.Show("Not enough point per exposure.");
                return false;
            }
            else
            {
                return true;
            }
        }

        public void SaveDataAutomatic()
        {
            return;
        }

        public void SaveData()
        {
            return;
        }

        #endregion

    }



    public class MultiChannelData
    {
        private List<Point3D> galvoXData;
        private List<Point3D> galvoYData;
        private List<Point3D>[] counterDataStore;
        private List<Point3D>[] analogDataStore;

        public MultiChannelData(int number_of_counter_channels, int number_of_analog_channels)
        {
            galvoXData = new List<Point3D>();
            galvoYData = new List<Point3D>();

            counterDataStore = new List<Point3D>[number_of_counter_channels];
            for (int i = 0; i < number_of_counter_channels; i++)
            {
                counterDataStore[i] = new List<Point3D>();
            }

            analogDataStore = new List<Point3D>[number_of_analog_channels];
            for (int i = 0; i < number_of_analog_channels; i++)
            {
                analogDataStore[i] = new List<Point3D>();
            }
        }

        public List<Point3D> GetGalvoXData()
        {
            return galvoXData;
        }

        public void AddtoGalvoXData(Point3D pnt)
        {
            galvoXData.Add(pnt);
        }

        public List<Point3D> GetGalvoYData()
        {
            return galvoYData;
        }

        public void AddtoGalvoYData(Point3D pnt)
        {
            galvoYData.Add(pnt);
        }

        public List<Point3D> GetCounterData(int counter_channel_number)
        {
            return counterDataStore[counter_channel_number];
        }

        public void AddtoCounterData(int counter_channel_number, Point3D pnt)
        {
            counterDataStore[counter_channel_number].Add(pnt);
        }

        public List<Point3D> GetAnalogueData(int analog_channel_number)
        {
            return analogDataStore[analog_channel_number];
        }

        public void AddtoAnalogueData(int analog_channel_number, Point3D pnt)
        {
            analogDataStore[analog_channel_number].Add(pnt);
        }
    }
}
