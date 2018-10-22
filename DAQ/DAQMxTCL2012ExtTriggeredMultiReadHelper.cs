using System;
using System.Threading;

using NationalInstruments;
using NationalInstruments.DAQmx;
using DAQ.Environment;
using DAQ.HAL;
using System.Collections.Generic;

namespace DAQ.TransferCavityLock2012
{
    public class DAQMxTCL2012ExtTriggeredMultiReadHelper : TransferCavity2012Lockable
    {
        //In this helper, the cavity is scanned externally (not from the computer). The software waits for a trigger pulse to start scanning.
        //An additional AI read is added to this helper for reading off the cavity voltage.
        //This helper doesn't deal with locking the laser.

        private string[] analogInputs;
        private string[] digitalInputs;
        private string trigger;

        private Task readAIsTask;
        private Task readDIsTask;

        private AnalogMultiChannelReader analogReader;
        private DigitalMultiChannelReader digitalReader;

        public DAQMxTCL2012ExtTriggeredMultiReadHelper(string[] analogInputs, string trigger)
        {
            this.analogInputs = analogInputs;
            this.trigger = trigger;
        }

        public DAQMxTCL2012ExtTriggeredMultiReadHelper(string[] analogInputs, string[] digitalInputs, string trigger)
        {
            this.analogInputs = analogInputs;
            this.digitalInputs = digitalInputs;
            this.trigger = trigger;
        }

        public DAQMxTCL2012ExtTriggeredMultiReadHelper(string[] inputs) // Legacy compatability!
        {
            this.analogInputs = inputs;
            trigger = "analogTrigger2";
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

        private void ConfigureReadAI(int numberOfMeasurements, double sampleRate, bool triggerSense, bool autostart) //AND CAVITY VOLTAGE!!! 
        {
            readAIsTask = new Task("readAIsTask");

            foreach (string inputName in analogInputs)
            {
                AnalogInputChannel channel = (AnalogInputChannel)Environs.Hardware.AnalogInputChannels[inputName];
                channel.AddToTask(readAIsTask, 0, 10);
            }

            SampleClockActiveEdge clockEdge = SampleClockActiveEdge.Rising;
            DigitalEdgeStartTriggerEdge triggerEdge = triggerSense ? DigitalEdgeStartTriggerEdge.Rising : DigitalEdgeStartTriggerEdge.Falling;

            if (!autostart)
            {
                // Use internal clock
                readAIsTask.Timing.ConfigureSampleClock("", sampleRate, clockEdge, SampleQuantityMode.FiniteSamples, numberOfMeasurements);
                readAIsTask.Triggers.StartTrigger.ConfigureDigitalEdgeTrigger(trigger, triggerEdge);
            }

            readAIsTask.Control(TaskAction.Verify);
            analogReader = new AnalogMultiChannelReader(readAIsTask.Stream);


            // Commiting now apparently saves time when we actually run the task
            readAIsTask.Control(TaskAction.Commit);
        }

        private void ConfigureReadDI(int numberOfMeasurements, double sampleRate, bool triggerSense)
        {
            readDIsTask = new Task("readDIsTask");

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
            data.AnalogData = analogReader.ReadMultiSample(numberOfMeasurements);

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
