using System;
using System.IO;
using System.Collections.Generic;
using System.Threading;
using System.Xml.Serialization;
using System.Linq;
using System.Windows;
using Microsoft.Win32;

using NationalInstruments.DAQmx;
using NationalInstruments.Analysis.Math;

using DAQ.Environment;
using DAQ.HAL;
using Data;

namespace ConfocalControl
{
    public delegate void SetTextBoxHandler(double value);
    public delegate void SetWaveFormHandler(double[] values, Point[] hist);
    public delegate void DaqExceptionEventHandler(DaqException e);

    public class TimeTracePlugin
    {
        #region Class members

        private enum CounterState { stopped, running, stopping};
        private CounterState counterState = CounterState.stopped;

        // Class Settings
        public PluginSettings Settings { get; set; }

        // Keep track of tasks
        private Task freqOutTask;
        private List<Task> counterTasks;
        private List<CounterSingleChannelReader> counterReaders;
        private Task analoguesTask;
        private AnalogMultiChannelReader analoguesReader;

        // Keep track of data
        private List<double>[] analogBuffer;
        private List<double>[] counterBuffer;
        private double[] latestAnalogs;
        private double[] latestCounters;

        // Keep track of latest settings
        private double historicSampleRate;
        public List<string> historicCounterChannels;
        public List<string> historicAnalogueChannels;

        // Bound event managers to class
        public event SetTextBoxHandler setTextBox;
        public event SetWaveFormHandler setWaveForm;
        public event DaqExceptionEventHandler DaqProblem;

        // Refer to only one instance of singlecounter 
        private static TimeTracePlugin controllerInstance;

        public static TimeTracePlugin GetController()
        {
            if (controllerInstance == null)
            {
                controllerInstance = new TimeTracePlugin();
            }
            return controllerInstance;
        }

        #endregion

        public void LoadSettings()
        {
            Settings = PluginSaveLoad.LoadSettings("singleCounter");
        }

        private void InitialiseSettings()
        {
            LoadSettings();
            if (Settings.Keys.Count != 8)
            {
                Settings["sampleRate"] = (double)100;
                Settings["bufferSize"] = (int)10000;

                Settings["counterChannels"] = new List<string> { "APD0", "APD1" };
                Settings["analogueChannels"] = new List<string> { "AI2" };
                Settings["analogueLowHighs"] = new Dictionary<string, double[]>();
                ((Dictionary<string, double[]>)Settings["analogueLowHighs"])["AI2"] = new double[] { -5, 5 }; 

                Settings["channel_type"] = "Counters";
                Settings["display_channel_index"] = 0;

                Settings["binNumber"] = (int)20;
            }

            historicCounterChannels = new List<string>((List<string>)Settings["counterChannels"]);
            historicAnalogueChannels = new List<string>((List<string>)Settings["analogueChannels"]);
        }

        public TimeTracePlugin() 
        {
            InitialiseSettings();
            setTextBox = null;
            setWaveForm = null;

            freqOutTask = null;
            counterReaders = null;
            analoguesReader = null;
        }

        public double GetExposure()
        {
            return 1 / (double)Settings["sampleRate"];
        }

        public void UpdateExposure(double exp)
        {
            Settings["sampleRate"] = (double) 1 / exp;
        }

        public int GetBufferSize()
        {
            return (int)Settings["bufferSize"];
        }

        public void UpdateBufferSize(int buff)
        {
            Settings["bufferSize"] = buff;
        }

        public bool IsRunning()
        {
            return counterState == CounterState.running;
        }

        private Point[] HistogramFromBuffer(double[] data_buffer)
        {
            if (data_buffer.Length > 1)
            {
                double min = data_buffer.Min();
                double max = data_buffer.Max();
                double[] centerVals;
                int[] _hist = Statistics.Histogram(data_buffer, min, max, (int)Settings["binNumber"], out centerVals);
                Point[] hist = new Point[_hist.Length];

                for (int i = 0; i < _hist.Length; i++)
                {
                    hist[i] = new Point(centerVals[i], _hist[i]);
                }

                return hist;
            }
            else
            {
                return null;
            }
        }

        private void ContinuousAcquisitionStarting()
        {
            // Check if other parts of the software are running
            if (IsRunning() || FastMultiChannelRasterScan.GetController().IsRunning() || CounterOptimizationPlugin.GetController().IsRunning() || SolsTiSPlugin.GetController().IsRunning() || DFGPlugin.GetController().IsRunning())
            {
                throw new DaqException("Counter already running");
            }

            // Update historic settings
            historicSampleRate = (double)Settings["sampleRate"];
            historicCounterChannels = (List<string>)Settings["counterChannels"];
            historicAnalogueChannels = (List<string>)Settings["analogueChannels"];

            // Reset buffers
            int numberOfAnalogs = ((List<string>)Settings["analogueChannels"]).Count;
            analogBuffer = new List<double>[numberOfAnalogs];
            latestAnalogs = new double[numberOfAnalogs];
            for (int i = 0; i < numberOfAnalogs; i++)
            {
                analogBuffer[i] = new List<double>();
                latestAnalogs[i] = 0;
            }

            int numberOfCounters = ((List<string>)Settings["counterChannels"]).Count;
            counterBuffer = new List<double>[numberOfCounters];
            latestCounters = new double[numberOfCounters];
            for (int i = 0; i < numberOfCounters; i++)
            {
                counterBuffer[i] = new List<double>();
                latestCounters[i] = 0;
            }

            // Set up clock task
            freqOutTask = new Task("sample clock task");

            // Continuous pulse train
            freqOutTask.COChannels.CreatePulseChannelFrequency(
                ((CounterChannel)Environs.Hardware.CounterChannels["SampleClock"]).PhysicalChannel,
                "photon counter clocking signal",
                COPulseFrequencyUnits.Hertz,
                COPulseIdleState.Low,
                0,
                (double)Settings["sampleRate"],
                0.5);

            freqOutTask.Timing.ConfigureImplicit(SampleQuantityMode.ContinuousSamples);

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
                    (double)Settings["sampleRate"],
                    SampleClockActiveEdge.Falling,
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
                    (double)Settings["sampleRate"],
                    SampleClockActiveEdge.Falling,
                    SampleQuantityMode.ContinuousSamples);

                analoguesTask.Control(TaskAction.Verify);

                DaqStream analogStream = analoguesTask.Stream;
                analoguesReader = new AnalogMultiChannelReader(analogStream);

                // Start tasks
                analoguesTask.Start();
            }
        }

        public void AcquisitionFinished()
        {
            freqOutTask.Dispose();
            foreach (Task counterTask in counterTasks)
            {
                counterTask.Dispose();
            }
            analoguesTask.Dispose(); 
            freqOutTask.Dispose();

            freqOutTask = null;
            counterReaders = null;
            analoguesReader = null;

            counterState = CounterState.stopped;
        }

        private void PreArm() 
        {
            counterState = CounterState.running;
            freqOutTask.Start();
        }

        private void ArmAndWaitContinuous()
        {
            // Read counter data
            for (int i = 0; i < counterBuffer.Length; i++)
            {
                if (counterBuffer[i].Count > (int)Settings["bufferSize"])
                {
                    counterBuffer[i].RemoveRange(0, counterBuffer[i].Count - (int)Settings["bufferSize"]);
                }

                double[] counterRead = counterReaders[i].ReadMultiSampleDouble(-1);

                if (counterRead.Length > 0)
                {
                    double[] dataRead = new double[counterRead.Length];
                    dataRead[0] = counterRead[0] - latestCounters[i];
                    latestCounters[i] = counterRead[counterRead.Length - 1];

                    if (counterRead.Length > 1)
                    {
                        for (int j = 1; j < counterRead.Length; j++)
                        {
                            dataRead[j] = counterRead[j] - counterRead[j - 1];
                        }
                    }

                    if (counterBuffer[i].Count > ((int)Settings["bufferSize"] - dataRead.Length))
                    {
                        counterBuffer[i].RemoveRange(0, dataRead.Length);
                    }

                    counterBuffer[i].AddRange(dataRead);
                }
            }

            // Read analogue data
            if (((List<string>)Settings["analogueChannels"]).Count != 0)
            {
                double[,] analogRead = analoguesReader.ReadMultiSample(-1);

                for (int i = 0; i < analogRead.GetLength(0); i++)
                {
                    if (analogBuffer[i].Count > (int)Settings["bufferSize"])
                    {
                        analogBuffer[i].RemoveRange(0, analogBuffer[i].Count - (int)Settings["bufferSize"]);
                    }

                    if (analogRead.GetLength(1) > 0)
                    {
                        latestAnalogs[i] = analogRead[i, analogRead.GetLength(1) - 1];

                        if (analogBuffer[i].Count > ((int)Settings["bufferSize"] - analogRead.GetLength(1)))
                        {
                            analogBuffer[i].RemoveRange(0, analogRead.GetLength(1));
                        }

                        for (int j = 0; j < analogRead.GetLength(1); j++)
			            {
			                analogBuffer[i].Add(analogRead[i, j]);
			            }
                    }
                }
            }
        }

        private void PostArm()
        {
            // Stop the counter; the job's done
            freqOutTask.Stop();
        }

        public void ContinuousAcquisition()
        {
            try
            {
                ContinuousAcquisitionStarting();
                PreArm();
                while (counterState == CounterState.running)
                {
                    ArmAndWaitContinuous();
                    OnData();
                    Thread.Sleep(10);
                }
                PostArm();
                AcquisitionFinished();
            }
            catch (DaqException e)
            {
                if (DaqProblem != null) DaqProblem(e);
            }
        }

        private void OnData()
        {
            switch ((string)Settings["channel_type"])
            {
                case "Counters":
                    if (setTextBox != null && setWaveForm != null)
                    {
                        if ((int)Settings["display_channel_index"] >= 0 && (int)Settings["display_channel_index"] < historicCounterChannels.Count)
                        {
                            double[] dataWaveform = counterBuffer[(int)Settings["display_channel_index"]].Skip(1).ToArray();
                            if (dataWaveform.Length > 0) setTextBox(dataWaveform[dataWaveform.Length - 1] * (double)Settings["sampleRate"]);
                            setWaveForm(dataWaveform, HistogramFromBuffer(dataWaveform));
                        }
                        else setWaveForm(null, null);
                    }
                    break;

                case "Analogues":
                    if (setTextBox != null && setWaveForm != null)
                    {
                        if ((int)Settings["display_channel_index"] >= 0 && (int)Settings["display_channel_index"] < historicAnalogueChannels.Count)
                        {
                            double[] dataWaveform = analogBuffer[(int)Settings["display_channel_index"]].Skip(1).ToArray();
                            if (dataWaveform.Length > 0) setTextBox(dataWaveform[dataWaveform.Length - 1]);
                            setWaveForm(dataWaveform, HistogramFromBuffer(dataWaveform));
                        }
                        else setWaveForm(null, null);
                    }
                    break;

                default:
                    throw new DaqException("Did not understand data type");
            }

        }

        public void StopContinuousAcquisition()
        {
            counterState = CounterState.stopping;
        }

        public void RequestHistoricData()
        {
            if (analogBuffer != null && counterBuffer != null && !IsRunning()) OnData();
        }

        public void DeleteHistoricData()
        {
            int numberOfAnalogs = ((List<string>)Settings["analogueChannels"]).Count;
            analogBuffer = new List<double>[numberOfAnalogs];
            latestAnalogs = new double[numberOfAnalogs];
            for (int i = 0; i < numberOfAnalogs; i++)
            {
                analogBuffer[i] = new List<double>();
                latestAnalogs[i] = 0;
            }

            int numberOfCounters = ((List<string>)Settings["counterChannels"]).Count;
            counterBuffer = new List<double>[numberOfCounters];
            latestCounters = new double[numberOfCounters];
            for (int i = 0; i < numberOfCounters; i++)
            {
                counterBuffer[i] = new List<double>();
                latestCounters[i] = 0;
            }
        }

        public void SaveData(string fileName)
        {
            string directory = Environs.FileSystem.GetDataDirectory((string)Environs.FileSystem.Paths["scanMasterDataPath"]);

            List<string> lines = new List<string>();
            lines.Add(DateTime.Today.ToString("dd-MM-yyyy") + " " + DateTime.Now.ToString("HH:mm:ss"));
            lines.Add("Exposure = " + (1 / historicSampleRate).ToString()); lines.Add("");

            string descriptionString = "";
            foreach (string channel in historicCounterChannels)
            {
                descriptionString = descriptionString + channel + " ";
            }
            foreach (string channel in historicAnalogueChannels )
            {
                descriptionString = descriptionString + channel + " ";
            }
            lines.Add(descriptionString);

            int minLength = -1;
            foreach (List<double> counterData in counterBuffer)
            {
                if (minLength == -1) minLength = counterData.Count;
                else if (counterData.Count < minLength) minLength = counterData.Count;
            }
            foreach (List<double> analogData in analogBuffer)
            {
                if (analogData.Count < minLength) minLength = analogData.Count;
            }

            if (minLength != 0)
            {
                for (int i = 0; i < minLength; i++)
                {
                    string line = "";
                    foreach (List<double> counterData in counterBuffer)
                    {
                        line = line + counterData[i].ToString() + " ";
                    }
                    foreach (List<double> analogData in analogBuffer)
                    {
                        line = line + analogData[i].ToString() + " ";
                    }
                    lines.Add(line);
                }
            }

            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.InitialDirectory = directory;
            saveFileDialog.FileName = fileName;

            if (saveFileDialog.ShowDialog() == true)
            {
                System.IO.File.WriteAllLines(saveFileDialog.FileName, lines.ToArray());
            }
        }
    }
}
