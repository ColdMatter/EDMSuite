using System;
using System.Threading;
using System.Diagnostics;

using NationalInstruments;
using NationalInstruments.DAQmx;
using DAQ.Environment;
using DAQ.HAL;
using System.Collections.Generic;

namespace DAQ.TransferCavityLock2012
{
    public class DAQMxTCL2012AnalogPeakFinderReadHelper : TransferCavity2012Lockable
    {
        //In this helper, the cavity is scanned externally (not from the computer). The software waits for a trigger pulse to start scanning.
        //The sample clock is derived externally from TTL pulses that indicate the peak positions of the lasers in the cavities.
        //Each clock edge will tell TCL to sample from the inputs once.
        //The number of measurements here should be equal to the number of peaks we expect to have in a single scan.
        //This helper doesn't deal with locking the laser.

        private string[] analogInputs;
        private string[] digitalInputs;
        private string trigger;
        private string extTTLClock;
        private string cavityName; // each cavity should have its own read helper

        private Task readAIsTask;
        private Task readDIsTask;

        private AnalogMultiChannelReader analogReader;
        private DigitalMultiChannelReader digitalReader;

        public DAQMxTCL2012AnalogPeakFinderReadHelper(string[] analogInputs, string trigger, string extTTLClock, string cavityName)
        {
            this.analogInputs = analogInputs;
            this.trigger = trigger;
            this.extTTLClock = extTTLClock;
            this.cavityName = cavityName;
        }

        public DAQMxTCL2012AnalogPeakFinderReadHelper(string[] analogInputs, string[] digitalInputs, string trigger, string extTTLClock, string cavityName)
        {
            this.analogInputs = analogInputs;
            this.digitalInputs = digitalInputs;
            this.trigger = trigger;
            this.extTTLClock = extTTLClock;
            this.cavityName = cavityName;
        }

        public DAQMxTCL2012AnalogPeakFinderReadHelper(string[] inputs) // Legacy compatability!
        {
            this.analogInputs = inputs;
            trigger = "analogTrigger2";
            // use internal clock
            extTTLClock = "";
            cavityName = "";
        }

        #region Methods for configuring the hardware


        public void ConfigureHardware(int numberOfMeasurements, double sampleRate, bool triggerSense, bool autostart)
        {
            if (digitalInputs.Length > 0)
            {
                ConfigureReadDI(numberOfMeasurements, sampleRate, triggerSense);
            }
            ConfigureReadAI(numberOfMeasurements, sampleRate, triggerSense, autostart);
        }

        private void ConfigureReadAI(int numberOfMeasurements, double sampleRate, bool triggerSense, bool autostart) 
        {
            readAIsTask = new Task(String.Join("readAIsFor", cavityName));

            foreach (string inputName in analogInputs)
            {
                AnalogInputChannel channel = (AnalogInputChannel)Environs.Hardware.AnalogInputChannels[inputName];
                channel.AddToTask(readAIsTask, 0, 10);
            }

            SampleClockActiveEdge clockEdge = SampleClockActiveEdge.Rising;
            DigitalEdgeStartTriggerEdge triggerEdge = triggerSense ? DigitalEdgeStartTriggerEdge.Rising : DigitalEdgeStartTriggerEdge.Falling;

            if (!autostart)
            {
                // Use peak finder output (series of TTL) as external clock, set sample rate to max of the DAQ sample rate
                readAIsTask.Timing.ConfigureSampleClock(extTTLClock, sampleRate, clockEdge, SampleQuantityMode.FiniteSamples, numberOfMeasurements);
                readAIsTask.Triggers.StartTrigger.ConfigureDigitalEdgeTrigger(trigger, triggerEdge);
            }

            readAIsTask.Control(TaskAction.Verify);
            analogReader = new AnalogMultiChannelReader(readAIsTask.Stream);


            // Commiting now apparently saves time when we actually run the task
            readAIsTask.Control(TaskAction.Commit);
        }

        private void ConfigureReadDI(int numberOfMeasurements, double sampleRate, bool triggerSense)
        {
            readDIsTask = new Task(String.Join("readDIsFor", cavityName));

            foreach (string inputName in digitalInputs)
            {
                DigitalInputChannel channel = (DigitalInputChannel)Environs.Hardware.DigitalInputChannels[inputName];
                channel.AddToTask(readDIsTask);
            }

            SampleClockActiveEdge clockEdge = SampleClockActiveEdge.Rising;
            DigitalEdgeStartTriggerEdge triggerEdge = triggerSense ? DigitalEdgeStartTriggerEdge.Rising : DigitalEdgeStartTriggerEdge.Falling;

            // Get the device that the analog inputs are on so we can use sample clock as well to sync timing
            string device = ((AnalogInputChannel)Environs.Hardware.AnalogInputChannels[analogInputs[0]]).Device;

            readDIsTask.Timing.ConfigureSampleClock(device + "/ai/SampleClock", sampleRate, clockEdge, SampleQuantityMode.FiniteSamples, numberOfMeasurements);
            readDIsTask.Triggers.StartTrigger.ConfigureDigitalEdgeTrigger(device + "/ai/StartTrigger", triggerEdge);

            readDIsTask.Control(TaskAction.Verify);
            digitalReader = new DigitalMultiChannelReader(readDIsTask.Stream);

            // Commiting now apparently saves time when we actually run the task
            readDIsTask.Control(TaskAction.Commit);
        }


        #endregion

        #region Methods for controlling hardware

        public TCLReadData Read(int numberOfMeasurements)
        {
            // Devices need to be explicitly started and stopped each time though so that digital task is always started before analog one
            if (digitalInputs.Length > 0)
            {
                readDIsTask.Start();
            }
            readAIsTask.Start();

            TCLReadData data = new TCLReadData();

            if (digitalInputs.Length > 0)
            {
                data.DigitalData = digitalReader.ReadWaveform(numberOfMeasurements);
            }
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            data.AnalogData = analogReader.ReadMultiSample(numberOfMeasurements);
            stopWatch.Stop();
            long elapsedTime = stopWatch.ElapsedMilliseconds;

            if (digitalInputs.Length > 0)
            {
                readDIsTask.Stop();
            }
            readAIsTask.Stop();

            return data;
        }

        public void DisposeReadTask()
        {
            readAIsTask.Dispose();

            if (digitalInputs.Length > 0)
            {
                readDIsTask.Dispose();
            }
        }

        #endregion

    }
}
