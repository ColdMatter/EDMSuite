using System;
using System.IO;
using System.Collections.Generic;
using System.Threading;
using System.Xml.Serialization;
using System.Linq;
using System.Windows;
using Microsoft.Win32;

using NationalInstruments.DAQmx;

using DAQ.Environment;
using DAQ.HAL;
using Data;

namespace ConfocalControl
{
    public delegate void SetTextBoxHandler(int value);
    public delegate void SetWaveFormHandler(int[] values, Point[] hist);
    public delegate void DaqExceptionEventHandler(DaqException e);

    public class SingleCounterPlugin
    {
        #region Class members

        private enum CounterState { stopped, running, stopping};
        private CounterState counterState = CounterState.stopped;

        // Class settings
        private PluginSettings settings = new PluginSettings("singleCounter");
        public PluginSettings Settings
        {
            get { return settings; }
            set { settings = value; }
        }

        // Keep track of tasks
        private Task freqOutTask;
        private Task countingTask;
        private CounterSingleChannelReader countReader;

        // Keep track of data
        private int _latestCount;
        public List<int> data_buffer;
        public int latestData;

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

        private void InitialiseSettings()
        {
            settings.LoadSettings();
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
                int max = data_buffer.Max();
                int min = data_buffer.Min();
                Point[] _hist = new Point[max - min + 1];

                for (int i = 0; i < (max - min + 1); i++)
                {
                    int _histX = min + i;
                    int _histY = (data_buffer.Where(x => x == (min + i))).Count();
                    _hist[i] = new Point(_histX, _histY);
                }
                return _hist;
            }
            else
            {
                return null;
            }
        }

        public void AcquisitionStarting()
        {
            if (counterState == CounterState.running)
            {
                throw new Exception("Counter already running");
            }

            data_buffer = new List<int>();

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

            // Set up an edge-counting task
            countingTask = new Task("buffered edge counter gatherer " + (string)settings["channel"]);

            // Count upwards on rising edges starting from zero
            countingTask.CIChannels.CreateCountEdgesChannel(
                ((CounterChannel)Environs.Hardware.CounterChannels[(string)settings["channel"]]).PhysicalChannel,
                "edge counter",
                CICountEdgesActiveEdge.Rising,
                0,
                CICountEdgesCountDirection.Up);

            // Take one sample within a window determined by sample rate using clock task
            countingTask.Timing.ConfigureSampleClock(
                (string)Environs.Hardware.GetInfo("SampleClockReader"),
                (double)settings["sampleRate"],
                SampleClockActiveEdge.Rising,
                SampleQuantityMode.ContinuousSamples,
                100);

            countingTask.Control(TaskAction.Verify);

            // Set up a reader for the edge counter
            countReader = new CounterSingleChannelReader(countingTask.Stream);
        }

        public void ContinuousAcquisitionStarting()
        {
            if (counterState == CounterState.running)
            {
                throw new Exception("Counter already running");
            }

            data_buffer = new List<int>();

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

            // Set up an edge-counting task
            countingTask = new Task("buffered edge counter gatherer " + (string)settings["channel"]);

            // Count upwards on rising edges starting from zero
            countingTask.CIChannels.CreateCountEdgesChannel(
                ((CounterChannel)Environs.Hardware.CounterChannels[(string)settings["channel"]]).PhysicalChannel,
                "edge counter",
                CICountEdgesActiveEdge.Rising,
                0,
                CICountEdgesCountDirection.Up);

            // Take one sample within a window determined by sample rate using clock task
            countingTask.Timing.ConfigureSampleClock(
                "/Dev1/PFI15",
                (double)settings["sampleRate"],
                SampleClockActiveEdge.Rising,
                SampleQuantityMode.ContinuousSamples);

            countingTask.Control(TaskAction.Verify);

            // Set up a reader for the edge counter
            countReader = new CounterSingleChannelReader(countingTask.Stream);
        }

        public void AcquisitionFinished()
        {
            lock (thisLock)
            {
                freqOutTask.Dispose();
                countingTask.Dispose();

                _latestCount = 0;
                latestData = 0;
                counterState = CounterState.stopped;
            }
        }

        public void PreArm() 
        {
            freqOutTask.Start();
            countingTask.Start();
            counterState = CounterState.running;
        }

        public int ArmAndWait()
        {
            lock (thisLock)
            {
                // Read all the data from the buffer - data is then removed from buffer
                countReader.ReadMultiSampleInt32(-1);
                // read 2 samples when they become available
                int[] _data = countReader.ReadMultiSampleInt32(2);
                latestData = _data[1] - _data[0];
                return latestData;
            }
        }

        public void ArmAndWaitContinuous()
        {
            if (data_buffer.Count > (int)settings["bufferSize"])
            {
                data_buffer.RemoveRange(0, data_buffer.Count - (int)settings["bufferSize"]);
            }


            // Read all the data from the buffer - data is then removed from buffer
            int[] _data = countReader.ReadMultiSampleInt32(-1);

            if (_data.Length != 0)
            {
                int[] data = new int[_data.Length];
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

            if (data_buffer.Count < 1)
            {
                latestData = 0;
            }
            else
            {
                latestData = data_buffer[data_buffer.Count - 1];
            }
        }

        public void PostArm()
        {
            // Stop the counter; the job's done
            countingTask.Stop();
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

        public void SaveData()
        {
            string directory = Environs.FileSystem.GetDataDirectory((string)Environs.FileSystem.Paths["scanMasterDataPath"]);
            string fileName = "timetrace_try.txt";

            List<string> lines = new List<string>();
            lines.Add("Exposure = " + GetExposure().ToString()); lines.Add("");

            foreach (int pnt in data_buffer)
            {
                string line = pnt.ToString();
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

        public void SaveHistogram()
        {
            string directory = Environs.FileSystem.GetDataDirectory((string)Environs.FileSystem.Paths["scanMasterDataPath"]);
            string fileName = "hist_try.txt";

            Point[] pnts = HistogramFromBuffer();

            List<string> lines = new List<string>();
            lines.Add("Exposure = " + GetExposure().ToString()); lines.Add("Number, freq"); lines.Add("");

            foreach (Point pnt in pnts)
            {
                string line = pnt.X.ToString() + ", " + pnt.Y.ToString();
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
    }
}
