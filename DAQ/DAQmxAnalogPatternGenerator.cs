using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

using DAQ.Environment;
using DAQ.HAL;
using Data;
using Data.Scans;
using DAQ.Analog;

using NationalInstruments;
using NationalInstruments.DAQmx;

namespace DAQ.Analog
{
    public class DAQMxAnalogPatternGenerator
    {
        Task analogOutputTask;

        public void Configure(AnalogPatternBuilder aPattern, int clockRate)
        {
            Configure(aPattern, clockRate, false);
        }

        public void Configure(AnalogPatternBuilder aPattern, int clockRate, bool loop)
        {
            analogOutputTask = new Task();
            foreach (string keys in aPattern.AnalogPatterns.Keys)
            {
                AddToAnalogOutputTask(analogOutputTask, keys);
            }
            string clockSource = "";

            SampleQuantityMode sqm;
            if (loop)
            {
                sqm = SampleQuantityMode.ContinuousSamples;
                analogOutputTask.Stream.WriteRegenerationMode = WriteRegenerationMode.AllowRegeneration;

            }
            else
            {
                sqm = SampleQuantityMode.FiniteSamples;
                analogOutputTask.Stream.WriteRegenerationMode = WriteRegenerationMode.DoNotAllowRegeneration;
            }


            analogOutputTask.Timing.ConfigureSampleClock(clockSource, clockRate,
                    SampleClockActiveEdge.Rising, sqm,
                    aPattern.PatternLength);
            analogOutputTask.Triggers.StartTrigger.ConfigureDigitalEdgeTrigger(
                    (string)Environs.Hardware.GetInfo("AOPatternTrigger"), DigitalEdgeStartTriggerEdge.Rising);

            analogOutputTask.Control(TaskAction.Verify);

        }

        public void OutputPatternAndWait(double[,] pattern)
        {
            AnalogMultiChannelWriter writer = new AnalogMultiChannelWriter(analogOutputTask.Stream);
            writer.WriteMultiSample(false, pattern);
            analogOutputTask.Start();
        }

        public void StopPattern()
        {
            analogOutputTask.Stop();
            analogOutputTask.Dispose();

        }

        #region private methods for creating timed Tasks/channels

        private void AddToAnalogOutputTask(Task task, string channel)
        {
            AnalogOutputChannel c = ((AnalogOutputChannel)Environs.Hardware.AnalogOutputChannels[channel]);
            c.AddToTask(task, c.RangeLow, c.RangeHigh);
        }
        #endregion

    }
}