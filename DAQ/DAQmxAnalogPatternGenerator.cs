using DAQ.Environment;
using DAQ.HAL;
using NationalInstruments.DAQmx;

namespace DAQ.Analog
{
    public class DAQMxAnalogPatternGenerator
    {
        Task analogOutputTask;
        string analogTaskName;
        string clock_line;
        string pattern_trigger;

        public void Configure(AnalogPatternBuilderSingleBoard aPattern, int clockRate)
        {
            Configure(aPattern, clockRate, false, true);
        }

        public void Configure(AnalogPatternBuilderSingleBoard aPattern, int clockRate, bool loop)
        {
            Configure(aPattern, clockRate, loop, true);
        }

        public void Configure(string taskName, AnalogPatternBuilderSingleBoard aPattern, int clockRate, bool loop, bool internalClk)
        {
            analogOutputTask = new Task(taskName);
            analogTaskName = taskName;
            clock_line = analogTaskName + "ClockLine";
            pattern_trigger = analogTaskName + "PatternTrigger";
            configure_AO(aPattern, clockRate, loop, internalClk);
        }

        public void Configure(AnalogPatternBuilderSingleBoard aPattern, int clockRate, bool loop, bool internalClk)
        {
            analogOutputTask = new Task("AO");
            analogTaskName = "AO";
            clock_line = analogTaskName + "ClockLine";
            pattern_trigger = analogTaskName + "PatternTrigger";
            configure_AO(aPattern, clockRate, loop, internalClk);
        }

        public void configure_AO(AnalogPatternBuilderSingleBoard aPattern, int clockRate, bool loop, bool internalClk)
        {

            foreach (string keys in aPattern.AnalogPatterns.Keys)
            {
                AddToAnalogOutputTask(analogOutputTask, keys);
            }

            string clockSource;

            if (internalClk == true)
            {
                clockSource = "";
                analogOutputTask.ExportSignals.SampleClockOutputTerminal = (string)Environment.Environs.Hardware.GetInfo(clock_line);
            }
            else
            {
                clockSource = (string)Environment.Environs.Hardware.GetInfo(clock_line);
            }

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

            analogOutputTask.Timing.ConfigureSampleClock(
                    clockSource,
                    clockRate,
                    SampleClockActiveEdge.Rising,
                    sqm,
                    aPattern.PatternLength);

            analogOutputTask.Triggers.StartTrigger.ConfigureDigitalEdgeTrigger(
                (string)Environs.Hardware.GetInfo(pattern_trigger),
                DigitalEdgeStartTriggerEdge.Rising);



            analogOutputTask.Control(TaskAction.Verify);

        }

        public void OutputPatternAndWait(double[,] pattern)
        {
            AnalogMultiChannelWriter writer = new AnalogMultiChannelWriter(analogOutputTask.Stream);
            writer.WriteMultiSample(false, pattern);
            // analogOutputTask.Done += new TaskDoneEventHandler(analogOutputTask_Done);
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

        private void AnalogOutputTaskDone(object sender, TaskDoneEventArgs e)
        {
            if (analogOutputTask != null)
            {
                analogOutputTask.Dispose();
            }
        }
    }
}