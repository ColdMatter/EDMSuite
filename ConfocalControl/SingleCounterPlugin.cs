using System;
using System.Threading;
using System.Xml.Serialization;

using NationalInstruments.DAQmx;

using DAQ.Environment;
using DAQ.FakeData;
using DAQ.HAL;
using Data;
//using ScanMaster.Acquire.Plugin;

namespace ConfocalControl
{
    public class SingleCounterPlugin
    {
        #region Class members

        private PluginSettings settings = new PluginSettings();
        public PluginSettings Settings
        {
            get { return settings; }
            set { settings = value; }
        }

        private Task countingTask;
        private CounterSingleChannelReader countReader;

        private double[] latestData;

        #endregion

        private void InitialiseSettings()
        {
            settings["channel"] = "ConfocalAPD";
            settings["sampleRate"] = (double)1000;
        }

        public SingleCounterPlugin() 
        {
            InitialiseSettings();
        }

        public double data;

        public void ReInitialiseSettings(double exp)
        {
            InitialiseSettings();
            settings["sampleRate"] = 1 / exp;
            int settingscheck = (int)settings["sampleRate"];
        }

        public void AcquisitionStarting()
        {
            // Set up an edge-counting task
            countingTask = new Task("buffered edge counter gatherer " + (string)settings["channel"]);

            // Count upwards on rising edges starting from zero
            countingTask.CIChannels.CreateCountEdgesChannel(
                ((CounterChannel)Environs.Hardware.CounterChannels[(string)settings["channel"]]).PhysicalChannel,
                "edge counter",
                CICountEdgesActiveEdge.Rising,
                0,
                CICountEdgesCountDirection.Up);

            // Take one sample within a window determined by samplerate using internal clock
            countingTask.Timing.ConfigureSampleClock("",
                (double)settings["sampleRate"],
                SampleClockActiveEdge.Rising,
                SampleQuantityMode.FiniteSamples,
                1);

            countingTask.Control(TaskAction.Verify);

            // Set up a reader for the edge counter
            countReader = new CounterSingleChannelReader(countingTask.Stream);
        }

        public void AcquisitionFinished()
        {
            countingTask.Dispose();
        }

        public void PreArm() 
        {
            lock (this)
            {
                countingTask.Start();
            }
        }

        public void ArmAndWait()
        {
            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();

            lock (this)
            {
                // read the data into a temporary array once all the samples have been acquired
                double data = countReader.ReadSingleSampleDouble();           
            }
        }

        public void PostArm()
        {
            lock (this)
            {
                // Stop the counter; the job's done
                countingTask.Stop();
            }
        }

        // Re-write
        public Shot Shot
        {
            get
            {
                lock (this)
                {
                    Shot s = new Shot();
                    TOF t = new TOF();
                    t.Data = latestData;
                    s.TOFs.Add(t);
                    return s;
                }
            }
        }
    }
}
