using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media.Media3D;
using System.Threading;
using Microsoft.Win32;

using NationalInstruments;
using NationalInstruments.DAQmx;
using DAQ.Environment;
using DAQ.HAL;

namespace ConfocalControl
{
    class FastMultiChannelRasterScan
    {
        #region Class members

        // Dependencies should refer to this instance only 
        private static FastMultiChannelRasterScan controllerInstance;
        public static FastMultiChannelRasterScan GetController()
        {
            if (controllerInstance == null)
            {
                controllerInstance = new FastMultiChannelRasterScan();
            }
            return controllerInstance;
        }

        // Settings
        public PluginSettings scanSettings { get; set; }

        // Keeping track of parameters for sample acquisition
        private int MINNUMBEROFSAMPLES = 10;
        private double TRUESAMPLERATE = 1000;
        private int pointsPerExposure;
        private double sampleRate;
        private double[,] waveform;

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
        public MultiChannelData dataOutputHistory { get { return dataOutputs; } }

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
            scanSettings = PluginSaveLoad.LoadSettings("multiChannelConfocalScan");
        }

        private void InitialiseSettings()
        {
            LoadSettings();

            if (scanSettings.Keys.Count != 9)
            {
                scanSettings["GalvoXStart"] = (double)0;
                scanSettings["GalvoXEnd"] = (double)1;
                scanSettings["GalvoXRes"] = (double)21;
                scanSettings["GalvoYStart"] = (double)0;
                scanSettings["GalvoYEnd"] = (double)1;
                scanSettings["GalvoYRes"] = (double)21;

                scanSettings["counterChannels"] = new List<string> { "APD0", "APD1" };
                scanSettings["analogueChannels"] = new List<string> { };
                scanSettings["analogueLowHighs"] = new Dictionary<string, double[]>();
            }
        }

        public FastMultiChannelRasterScan()
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

        private void GetGalvoWaveform()
        {
            int numberofSamples = Convert.ToInt32((double)scanSettings["GalvoYRes"] * (double)scanSettings["GalvoXRes"] * (pointsPerExposure + 1));
            waveform = new double[2, numberofSamples];

            // Snake the raster
            bool IsSnaked = false;
            bool inverted = true;

            // Loop for Y axis
            for (double YNumber = 0;
                    YNumber < (double)scanSettings["GalvoYRes"] + 1;
                    YNumber++)
            {
                if (IsSnaked)
                {
                    if (inverted) inverted = false;
                    else inverted = true;
                }
                else inverted = false;

                // Calculate new Y galvo point from current scan point 
                double currentGalvoYpoint = (double)scanSettings["GalvoYStart"] + YNumber *
                        ((double)scanSettings["GalvoYEnd"] -
                        (double)scanSettings["GalvoYStart"]) /
                        ((double)scanSettings["GalvoYRes"]);

                // Loop for X axis
                for (double _XNumber = 0;
                        _XNumber < (double)scanSettings["GalvoXRes"] + 1;
                        _XNumber++)
                {
                    double XNumber;
                    if (inverted) XNumber = (double)scanSettings["GalvoXRes"] - 1 - _XNumber;
                    else XNumber = _XNumber;

                    // Calculate new X galvo point from current scan point 
                    double currentGalvoXpoint = (double)scanSettings["GalvoXStart"] + XNumber *
                    ((double)scanSettings["GalvoXEnd"] -
                    (double)scanSettings["GalvoXStart"]) /
                    ((double)scanSettings["GalvoXRes"]);

                    for (int n = 0; n < (pointsPerExposure+1); n++)
                    {
                        int currentPosition = Convert.ToInt32((YNumber * (double)scanSettings["GalvoXRes"] + XNumber) * (pointsPerExposure + 1) + n);
                        waveform[0, currentPosition] = currentGalvoXpoint;
                        waveform[1, currentPosition] = currentGalvoYpoint;
                    }
                }
            }
        }

        public void SynchronousStartScan()
        {
            try
            {
                if (IsRunning() || SingleCounterPlugin.GetController().IsRunning() || CounterOptimizationPlugin.GetController().IsRunning() || SolsTiSPlugin.GetController().IsRunning())
                {
                    throw new DaqException("Counter already running");
                }

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
            // Move to the start of the scan.
            GalvoPairPlugin.GetController().MoveOnlyAcquisitionStarting();
            GalvoPairPlugin.GetController().SetGalvoXSetpointAndWait(
                         (double)scanSettings["GalvoXStart"], null, null);

            GalvoPairPlugin.GetController().SetGalvoYSetpointAndWait(
                         (double)scanSettings["GalvoYStart"], null, null);
            GalvoPairPlugin.GetController().MoveOnlyAcquisitionFinished();

            // Define sample rate 
            CalculateParameters();

            // Get analogue sequence
            GetGalvoWaveform();

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
                0.9);

            freqOutTask.Triggers.StartTrigger.ConfigureDigitalEdgeTrigger(
                (string)Environs.Hardware.GetInfo("StartTriggerReader"),
                DigitalEdgeStartTriggerEdge.Rising);

            freqOutTask.Timing.ConfigureImplicit(SampleQuantityMode.FiniteSamples, waveform.GetLength(1));

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
                    sampleRate,
                    SampleClockActiveEdge.Falling,
                    SampleQuantityMode.ContinuousSamples,
                    waveform.GetLength(1) + 10);

                counterTasks[i].Control(TaskAction.Verify);

                DaqStream counterStream = counterTasks[i].Stream;
                counterReaders.Add(new CounterSingleChannelReader(counterStream));

                // Start tasks
                counterTasks[i].Start();
            }

            // Set up analogue sampling tasks
            analoguesTask = new Task("analogue sampler");

            for (int i = 0; i < ((List<string>)scanSettings["analogueChannels"]).Count; i++)
            {
                string channelName = ((List<string>)scanSettings["analogueChannels"])[i];

                double inputRangeLow = ((Dictionary<string, double[]>)scanSettings["analogueLowHighs"])[channelName][0];
                double inputRangeHigh = ((Dictionary<string, double[]>)scanSettings["analogueLowHighs"])[channelName][1];

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
                    sampleRate,
                    SampleClockActiveEdge.Falling,
                    SampleQuantityMode.ContinuousSamples,
                    waveform.GetLength(1) + 10);

                analoguesTask.Control(TaskAction.Verify);

                DaqStream analogStream = analoguesTask.Stream;
                analoguesReader = new AnalogMultiChannelReader(analogStream);

                // Start tasks
                analoguesTask.Start();
            }

            // Start Galvos
            GalvoPairPlugin.GetController().MultiWriterAcquisitionStarting((string)Environs.Hardware.GetInfo("SampleClockReader"), sampleRate);
            GalvoPairPlugin.GetController().SetMultiGalvoSetpoint(waveform);
        }

        private void SynchronousAcquire()
        // Main method for looping over scan parameters, aquiring scan outputs and connecting to controller for display
        {
            // Start trigger task
            triggerWriter.WriteSingleSampleSingleLine(true, true);

            for (int YNumber = 0; YNumber < (double)scanSettings["GalvoYRes"] + 1; YNumber++)
            {
                for (int XNumber = 0; XNumber < (double)scanSettings["GalvoXRes"] + 1; XNumber++)
                {
                    // Read counter data
                    counterLatestData = new List<double[]>();
                    foreach (CounterSingleChannelReader counterReader in counterReaders)
                    {
                        counterLatestData.Add(counterReader.ReadMultiSampleDouble(pointsPerExposure + 1));
                    }

                    // Read analogue data
                    if (((List<string>)scanSettings["analogueChannels"]).Count != 0)
                    {
                        analogLatestData = analoguesReader.ReadMultiSample(pointsPerExposure + 1);
                    }

                    // Store counter data

                    for (int i = 0; i < counterLatestData.Count; i++)
                    {
                        double[] latestData = counterLatestData[i];
                        double data = latestData[latestData.Length - 1] - latestData[0];
                        Point3D pnt = new Point3D(XNumber + 1, YNumber + 1, data);
                        dataOutputs.AddtoCounterData(i, pnt);
                    }

                    // Store analogue data
                    if (((List<string>)scanSettings["analogueChannels"]).Count != 0)
                    {
                        for (int i = 0; i < analogLatestData.GetLength(0); i++)
                        {
                            double sum = 0;
                            for (int j = 0; j < analogLatestData.GetLength(1) - 1; j++)
                            {
                                sum = sum + analogLatestData[i, j];
                            }
                            double average = sum / (analogLatestData.GetLength(1) - 1);
                            Point3D pnt = new Point3D(XNumber + 1, YNumber + 1, average);
                            dataOutputs.AddtoAnalogueData(i, pnt);
                        }
                    }

                    // Check if scan exit.
                    if (CheckIfStopping())
                    {
                        // Quit plugins
                        AcquisitionFinishing();
                        return;
                    }
                }

                OnLineFinished(dataOutputs);
            }

            triggerWriter.WriteSingleSampleSingleLine(true, false);
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
            GalvoPairPlugin.GetController().MultiWriterAcquisitionFinished();

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
            return backendState == RasterScanState.stopping;
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
            else
            {
                return true;
            }
        }

        public void SaveDataAutomatic(string fileName)
        {
            string directory = Environs.FileSystem.GetDataDirectory((string)Environs.FileSystem.Paths["scanMasterDataPath"]);

            List<string> lines = new List<string>();
            lines.Add(DateTime.Today.ToString("dd-MM-yyyy") + " " + DateTime.Now.ToString("HH:mm:ss"));
            lines.Add("Exposure = " + SingleCounterPlugin.GetController().GetExposure().ToString());
            lines.Add("X start = " + ((double)scanSettings["GalvoXStart"]).ToString() + ", X stop = " + ((double)scanSettings["GalvoXEnd"]).ToString() + ", X resolution = " + ((double)scanSettings["GalvoXRes"]).ToString());
            lines.Add("Y start = " + ((double)scanSettings["GalvoYStart"]).ToString() + ", Y stop = " + ((double)scanSettings["GalvoYEnd"]).ToString() + ", Y resolution = " + ((double)scanSettings["GalvoYRes"]).ToString());
            lines.Add("");

            string descriptionString = "X Y";
            foreach (string channel in (List<string>)scanSettings["counterChannels"])
            {
                descriptionString = descriptionString + " " + channel;
            }
            foreach (string channel in (List<string>)scanSettings["analogueChannels"])
            {
                descriptionString = descriptionString + " " + channel;
            }
            lines.Add(descriptionString);

            foreach (double[] completeData in dataOutputs.TransposeData())
            {
                string line = "";
                foreach (double dataPnt in completeData)
                {
                    line = line + dataPnt.ToString() + " ";
                }
                lines.Add(line);
            }

            System.IO.File.WriteAllLines(directory + fileName, lines.ToArray());
        }

        public void SaveData(string fileName)
        {
            string directory = Environs.FileSystem.GetDataDirectory((string)Environs.FileSystem.Paths["scanMasterDataPath"]);

            List<string> lines = new List<string>();
            lines.Add(DateTime.Today.ToString("dd-MM-yyyy") + " " + DateTime.Now.ToString("HH:mm:ss"));
            lines.Add("Exposure = " + SingleCounterPlugin.GetController().GetExposure().ToString());
            lines.Add("X start = " + ((double)scanSettings["GalvoXStart"]).ToString() + ", X stop = " + ((double)scanSettings["GalvoXEnd"]).ToString() + ", X resolution = " + ((double)scanSettings["GalvoXRes"]).ToString());
            lines.Add("Y start = " + ((double)scanSettings["GalvoYStart"]).ToString() + ", Y stop = " + ((double)scanSettings["GalvoYEnd"]).ToString() + ", Y resolution = " + ((double)scanSettings["GalvoYRes"]).ToString());
            lines.Add("");

            string descriptionString = "X Y";
            foreach (string channel in (List<string>)scanSettings["counterChannels"])
            {
                descriptionString = descriptionString + " " + channel;
            }
            foreach (string channel in (List<string>)scanSettings["analogueChannels"])
            {
                descriptionString = descriptionString + " " + channel;
            }
            lines.Add(descriptionString);

            foreach (double[] completeData in dataOutputs.TransposeData())
            {
                string line = "";
                foreach (double dataPnt in completeData)
                {
                    line = line + dataPnt.ToString() + " ";
                }
                lines.Add(line);
            }

            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.InitialDirectory = directory;
            saveFileDialog.FileName = fileName;

            if (saveFileDialog.ShowDialog() == true)
            {
                System.IO.File.WriteAllLines(saveFileDialog.FileName, lines.ToArray());
            }
        }

        #endregion

    }
}
