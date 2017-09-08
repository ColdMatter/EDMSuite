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
        AnalogMultiChannelWriter writer;
        public void Configure(AnalogPatternBuilder aPattern, int clockRate)
        {
            Configure(aPattern, clockRate, false);
        }

        public void Configure(AnalogPatternBuilder aPattern, int clockRate,  bool loop)
        {
            analogOutputTask = new Task();
            foreach (string keys in aPattern.AnalogPatterns.Keys)
            {
                AddToAnalogOutputTask(analogOutputTask, keys);
            }
            string clockSource = "";

            SampleQuantityMode sqm;
            if(loop)
            {
                sqm = SampleQuantityMode.FiniteSamples;
                analogOutputTask.Stream.WriteRegenerationMode = WriteRegenerationMode.AllowRegeneration;
                //analogOutputTask.Triggers.SynchronizationType = TriggerSynchronizationType.Slave;
                
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
            if (writer == null)
            {
                writer = new AnalogMultiChannelWriter(analogOutputTask.Stream);
                writer.WriteMultiSample(false, pattern);
            }
            analogOutputTask.Start();
        }

        public void StopPattern()
        {
            analogOutputTask.Stop();
            analogOutputTask.Dispose();
            writer = null;
        }
        public void PauseLoop()
        {
            analogOutputTask.WaitUntilDone();
            analogOutputTask.Stop();
            
        }
        public void StartPattern()
        {
            if (writer == null) throw new Exception("No pattern written to card");
            //analogOutputTask.WaitUntilDone();
            //analogOutputTask.Stop();
            analogOutputTask.Start();
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
