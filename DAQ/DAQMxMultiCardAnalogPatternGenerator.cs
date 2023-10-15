using DAQ.Environment;
using DAQ.HAL;
using NationalInstruments.DAQmx;

namespace DAQ.Analog
{
    public class DAQMxMultiCardAnalogPatternGenerator
    {
        Task analogOutputTask;

        public void Configure(AnalogPatternBuilder aPattern, int clockRate)
        {
            Configure(aPattern, clockRate, false, true);
        }

        public void Configure(AnalogPatternBuilder aPattern, int clockRate, bool loop)
        {
            Configure(aPattern, clockRate, loop, true);
        }

        public void Configure(AnalogPatternBuilder aPattern, int clockRate, bool loop, bool internalClk)
        {
            analogOutputTask = new Task();

            // NOTE: The code below has been disabled due to an issue with its functionality.
            // This was originally named "DAQMxAnalogPatternGenerator - Copy.cs", which conflicted with "DAQMxAnalogPatternGenerator.cs".
            // After verifying no references to this class, the file/class was renamed to resolve the conflict.
            // If you wish to restore or repurpose this code, please review its intended functionality and ensure it meets requirements.
            // Comment written by Simon Swarbrick 2023.
            // Code originally created by Luke Caldwell 2021.

            //foreach (string keys in aPattern.AnalogPatterns.Keys)
            //{
            //    AddToAnalogOutputTask(analogOutputTask, keys);
            //}
            string clockSource;

            if (internalClk == true)
            {
                clockSource = "";
                analogOutputTask.ExportSignals.SampleClockOutputTerminal = (string)Environs.Hardware.GetInfo("AOClockLine");
            }
            else
                clockSource = (string)Environs.Hardware.GetInfo("AOClockLine");

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

        private void AddToAnalogOutputTask(Task task, string channel)
        {
            AnalogOutputChannel c = ((AnalogOutputChannel)Environs.Hardware.AnalogOutputChannels[channel]);
            c.AddToTask(task, c.RangeLow, c.RangeHigh);
        }
    }
}