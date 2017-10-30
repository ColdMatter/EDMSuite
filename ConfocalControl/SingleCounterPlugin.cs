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

    public class SingleCounterPlugin
    {
        #region Class members

        private enum CounterState { stopped, running, stopping};
        private CounterState counterState = CounterState.stopped;

        // Class settings
        private PluginSettings settings;
        public PluginSettings Settings
        {
            get { return settings; }
            set { settings = value; }
        }

        // Keep track of tasks
        private Task freqOutTask;
        private Task samplingTask;
        private CounterSingleChannelReader countReader;
        private AnalogSingleChannelReader analogReader;

        // Keep track of data
        private double _latestCount;
        public List<double> data_buffer;
        public double latestData;

        // Bound event managers to class
        public event SetTextBoxHandler setTextBox;
        public event SetWaveFormHandler setWaveForm;
        public event DaqExceptionEventHandler DaqProblem;

        // Refer to only one instance of singlecounter 
        private static SingleCounterPlugin controllerInstance;

        public static SingleCounterPlugin GetController()
        {
            if (controllerInstance == null)
            {
                controllerInstance = new SingleCounterPlugin();
            }
            return controllerInstance;
        }

        private Object thisLock = new Object(); 

        #endregion

        public void LoadSettings()
        {
            settings = PluginSaveLoad.LoadSettings("singleCounter");
        }

        private void InitialiseSettings()
        {
            LoadSettings();
            if (settings.Keys.Count != 6)
            {
                settings["sampleRate"] = (double)100;
                settings["channel_type"] = "Counters";
                settings["analogueLowHighs"] = new double[] { -5, 5 };
                settings["channel"] = "APD0";
                settings["bufferSize"] = (int)10000;
                settings["binNumber"] = (int)20;
                return;
            }
        }

        public SingleCounterPlugin() 
        {
            InitialiseSettings();
            setTextBox = null;
            setWaveForm = null;
            
            _latestCount = 0;
            latestData = 0;
        }

        public double GetExposure()
        {
            return 1 / (double)settings["sampleRate"];
        }

        public void UpdateExposure(double exp)
        {
            settings["sampleRate"] = (double) 1 / exp;
        }

        public int GetBufferSize()
        {
            return (int)settings["bufferSize"];
        }

        public void UpdateBufferSize(int buff)
        {
            settings["bufferSize"] = buff;
        }

        public bool IsRunning()
        {
            return counterState == CounterState.running;
        }

        public Point[] HistogramFromBuffer()
        {
            if (data_buffer.Count > 1)
            {
                double min = data_buffer.Min();
                double max = data_buffer.Max();
                double[] centerVals;
                int[] _hist = Statistics.Histogram(data_buffer.ToArray(), min, max, (int)settings["binNumber"], out centerVals);
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
            if (IsRunning() || MultiChannelRasterScan.GetController().IsRunning())
            {
                throw new DaqException("Counter already running");
            }

            data_buffer = new List<double>();

            // Set up clock task
            freqOutTask = new Task("sample clock task");

            // Continuous pulse train
            freqOutTask.COChannels.CreatePulseChannelFrequency(
                ((CounterChannel)Environs.Hardware.CounterChannels["SampleClock"]).PhysicalChannel,
                "photon counter clocking signal",
                COPulseFrequencyUnits.Hertz,
                COPulseIdleState.Low,
                0,
                (double)settings["sampleRate"],
                0.5);

            freqOutTask.Timing.ConfigureImplicit(SampleQuantityMode.ContinuousSamples);

            switch ((string)settings["channel_type"])
            {
                case "Counters":
                    // Set up an edge-counting task
                    samplingTask = new Task("buffered edge counter gatherer " + (string)settings["channel"]);

                    // Count upwards on rising edges starting from zero
                    samplingTask.CIChannels.CreateCountEdgesChannel(
                        ((CounterChannel)Environs.Hardware.CounterChannels[(string)settings["channel"]]).PhysicalChannel,
                        "edge counter",
                        CICountEdgesActiveEdge.Rising,
                        0,
                        CICountEdgesCountDirection.Up);

                    // Take one sample within a window determined by sample rate using clock task
                    samplingTask.Timing.ConfigureSampleClock(
                        (string)Environs.Hardware.GetInfo("SampleClockReader"),
                        (double)settings["sampleRate"],
                        SampleClockActiveEdge.Rising,
                        SampleQuantityMode.ContinuousSamples);

                    samplingTask.Control(TaskAction.Verify);

                    // Set up a reader for the edge counter
                    countReader = new CounterSingleChannelReader(samplingTask.Stream);
                    break;

                case "Analogues":
                    // Set up an analogue sampling task
                    samplingTask = new Task("buffered edge counter gatherer " + (string)settings["channel"]);

                    string channelName = (string)settings["channel"];
                    double inputRangeLow = ((double[])settings["analogueLowHighs"])[0];
                    double inputRangeHigh = ((double[])settings["analogueLowHighs"])[1];

                    ((AnalogInputChannel)Environs.Hardware.AnalogInputChannels[channelName]).AddToTask(
                        samplingTask,
                        inputRangeLow,
                        inputRangeHigh
                        );

                    // Take one sample within a window determined by sample rate using clock task
                    samplingTask.Timing.ConfigureSampleClock(
                        (string)Environs.Hardware.GetInfo("SampleClockReader"),
                        (double)settings["sampleRate"],
                        SampleClockActiveEdge.Rising,
                        SampleQuantityMode.ContinuousSamples);

                    samplingTask.Control(TaskAction.Verify);

                    // Set up a reader for the edge counter
                    analogReader = new AnalogSingleChannelReader(samplingTask.Stream);
                    break;

                default:
                    throw new DaqException("Did not understand time trace input type.");
            }
        }

        public void AcquisitionFinished()
        {
            samplingTask.Dispose(); 
            freqOutTask.Dispose();

            freqOutTask = null;
            samplingTask = null;
            countReader = null;
            analogReader = null;

            _latestCount = 0;
            latestData = 0;
            counterState = CounterState.stopped;
        }

        private void PreArm() 
        {
            freqOutTask.Start();
            samplingTask.Start();
            counterState = CounterState.running;
        }

        private void ArmAndWaitContinuous()
        {
            if (data_buffer.Count > (int)settings["bufferSize"])
            {
                data_buffer.RemoveRange(0, data_buffer.Count - (int)settings["bufferSize"]);
            }


            // Read all the data from the buffer - update buffer position
            double[] _data;
            switch ((string)settings["channel_type"])
            {
                case "Counters":
                    _data = countReader.ReadMultiSampleInt32(-1).Select(Convert.ToDouble).ToArray();

                    if (_data.Length != 0)
                    {
                        double[] data = new double[_data.Length];
                        data[0] = _data[0] - _latestCount;
                        _latestCount = _data[_data.Length - 1];

                        if (_data.Length > 1)
                        {
                            for (int i = 1; i < _data.Length; i++)
                            {
                                data[i] = _data[i] - _data[i - 1];
                            }
                        }

                        if (data_buffer.Count < ((int)settings["bufferSize"] - data.Length))
                        {
                            data_buffer.AddRange(data);
                        }
                        else
                        {
                            data_buffer.RemoveRange(0, data.Length);
                            data_buffer.AddRange(data);
                        }

                    }

                    break;

                case "Analogues":
                    _data = analogReader.ReadMultiSample(-1);

                    if (_data.Length != 0)
                    {
                        if (data_buffer.Count < ((int)settings["bufferSize"] - _data.Length))
                        {
                            data_buffer.AddRange(_data);
                        }
                        else
                        {
                            data_buffer.RemoveRange(0, _data.Length);
                            data_buffer.AddRange(_data);
                        }

                    }

                    break;

                default:
                    throw new DaqException("Did not understand time trace input type.");
            }

            if (data_buffer.Count < 1)
            {
                latestData = 0;
            }
            else
            {
                latestData = data_buffer[data_buffer.Count - 1];
            }
        }

        private void PostArm()
        {
            // Stop the counter; the job's done
            samplingTask.Stop();
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
                    if (setTextBox != null)
                    {
                        setTextBox(latestData);
                    }

                    if (setWaveForm != null)
                    {
                        setWaveForm(data_buffer.ToArray(), HistogramFromBuffer());
                    }

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

        public void StopContinuousAcquisition()
        {
            counterState = CounterState.stopping;
        }

        public void SaveData(string fileName)
        {
            string directory = Environs.FileSystem.GetDataDirectory((string)Environs.FileSystem.Paths["scanMasterDataPath"]);

            List<string> lines = new List<string>();
            lines.Add(DateTime.Today.ToString("dd-MM-yyyy") + " " + DateTime.Now.ToString("HH:mm:ss"));
            lines.Add("Exposure =, " + GetExposure().ToString()); lines.Add("");

            foreach (int pnt in data_buffer)
            {
                string line = pnt.ToString();
                lines.Add("data, " + line);
            }

            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.InitialDirectory = directory;
            saveFileDialog.FileName = fileName;

            if (saveFileDialog.ShowDialog() == true)
            {
                System.IO.File.WriteAllLines(saveFileDialog.FileName, lines.ToArray());
            }
        }

        public void SaveHistogram(string fileName)
        {
            string directory = Environs.FileSystem.GetDataDirectory((string)Environs.FileSystem.Paths["scanMasterDataPath"]);

            Point[] pnts = HistogramFromBuffer();

            List<string> lines = new List<string>();
            lines.Add(DateTime.Today.ToString("dd-MM-yyyy") + " " + DateTime.Now.ToString("HH-mm-ss"));
            lines.Add("Exposure =, " + GetExposure().ToString()); lines.Add("Number, freq"); lines.Add("");

            foreach (Point pnt in pnts)
            {
                string line = pnt.X.ToString() + ", " + pnt.Y.ToString();
                lines.Add("data, " + line);
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
