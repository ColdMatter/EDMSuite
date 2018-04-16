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

        private IAsyncResult digitalResult;

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

        public DAQMxTCL2012ExtTriggeredMultiReadHelper(string[] inputs)
        {
            this.analogInputs = inputs;
            trigger = "analogTrigger2";
        }

        #region Methods for configuring the hardware


        public void ConfigureHardware(int numberOfMeasurements, double sampleRate, bool triggerSense, bool autostart)
        {
            ConfigureReadAI(numberOfMeasurements, sampleRate, triggerSense, autostart);
            if (digitalInputs.Length > 0)
            {
                ConfigureReadDI(numberOfMeasurements, sampleRate, triggerSense);
            }
        }

        public void ConfigureReadAI(int numberOfMeasurements, double sampleRate, bool triggerSense, bool autostart) //AND CAVITY VOLTAGE!!! 
        {
            readAIsTask = new Task();

            foreach (string inputName in analogInputs)
            {
                AnalogInputChannel channel = (AnalogInputChannel)Environs.Hardware.AnalogInputChannels[inputName];
                channel.AddToTask(readAIsTask, 0, 10);
            }

            SampleClockActiveEdge clockEdge = SampleClockActiveEdge.Rising;
            DigitalEdgeStartTriggerEdge triggerEdge = triggerSense ? DigitalEdgeStartTriggerEdge.Rising : DigitalEdgeStartTriggerEdge.Falling;

            if (!autostart)
            {
                readAIsTask.Timing.ConfigureSampleClock("", sampleRate, clockEdge, SampleQuantityMode.FiniteSamples, numberOfMeasurements);
                readAIsTask.Triggers.StartTrigger.ConfigureDigitalEdgeTrigger(trigger, triggerEdge);
            }

            readAIsTask.Control(TaskAction.Verify);
            analogReader = new AnalogMultiChannelReader(readAIsTask.Stream);
        }

        public void ConfigureReadDI(int numberOfMeasurements, double sampleRate, bool triggerSense)
        {
            readDIsTask = new Task();

            foreach (string inputName in digitalInputs)
            {
                DigitalInputChannel channel = (DigitalInputChannel)Environs.Hardware.DigitalInputChannels[inputName];
                channel.AddToTask(readDIsTask);
            }

            SampleClockActiveEdge clockEdge = SampleClockActiveEdge.Rising;
            DigitalEdgeStartTriggerEdge triggerEdge = triggerSense ? DigitalEdgeStartTriggerEdge.Rising : DigitalEdgeStartTriggerEdge.Falling;

            string device = ((AnalogInputChannel)Environs.Hardware.AnalogInputChannels[analogInputs[0]]).Device;

            readDIsTask.Timing.ConfigureSampleClock(device + "/ai/SampleClock", sampleRate, clockEdge, SampleQuantityMode.FiniteSamples, numberOfMeasurements);
            readDIsTask.Triggers.StartTrigger.ConfigureDigitalEdgeTrigger(device + "/ai/StartTrigger", triggerEdge);

            readDIsTask.Control(TaskAction.Verify);
            digitalReader = new DigitalMultiChannelReader(readDIsTask.Stream);
        }


        #endregion

        #region Methods for controlling hardware

        public TCLReadData Read(int numberOfMeasurements)
        {
            TCLReadData data = new TCLReadData();

            if (digitalInputs.Length > 0)
            {
                digitalResult = digitalReader.BeginReadWaveform(numberOfMeasurements, null, readDIsTask);
            }

            data.AnalogData = analogReader.ReadMultiSample(numberOfMeasurements);

            if (digitalInputs.Length > 0)
            {
                readDIsTask.WaitUntilDone();
                data.DigitalData = digitalReader.EndReadWaveform(digitalResult);
            }

            return data;
        }

        public void DisposeReadTask()
        {
            readAIsTask.Dispose();
            readDIsTask.Dispose();
        }

        #endregion

    }
}
