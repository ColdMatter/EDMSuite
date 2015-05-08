using System;
using System.Threading;

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
        private string trigger;

        private Task readAIsTask;
        private Dictionary<string, AnalogInputChannel> channels;

        private AnalogMultiChannelReader analogReader;


        

        public DAQMxTCL2012ExtTriggeredMultiReadHelper(string[] inputs, string trigger)
        {
            this.analogInputs = inputs;
            this.trigger = trigger;
        }
        public DAQMxTCL2012ExtTriggeredMultiReadHelper(string[] inputs)
        {
            this.analogInputs = inputs;
            trigger = "analogTrigger2";
        }

        #region Methods for configuring the hardware

        //The photodiode inputs have been bundled into one task. We never read one photodiode without reading
        //the other.
        public void ConfigureReadAI(int numberOfMeasurements, bool autostart) //AND CAVITY VOLTAGE!!! 
        {
            //readAIsTask = new Task("readAI");
            readAIsTask = new Task();

            channels = new Dictionary<string, AnalogInputChannel>();
            foreach (string s in analogInputs)
            {
                AnalogInputChannel channel = (AnalogInputChannel)Environs.Hardware.AnalogInputChannels[s];
                channels.Add(s, channel);
            }

            foreach (KeyValuePair<string, AnalogInputChannel> pair in channels)
            {
                pair.Value.AddToTask(readAIsTask, 0, 10);
            }

            if (autostart == false)
            {
                 readAIsTask.Timing.ConfigureSampleClock(
                    "",
                    50000,
                    SampleClockActiveEdge.Rising,
                    SampleQuantityMode.FiniteSamples, numberOfMeasurements);

                readAIsTask.Triggers.StartTrigger.ConfigureDigitalEdgeTrigger(
                    (string)Environs.Hardware.GetInfo(trigger),
                    DigitalEdgeStartTriggerEdge.Rising);
            }
            readAIsTask.Control(TaskAction.Verify);
            analogReader = new AnalogMultiChannelReader(readAIsTask.Stream);
        }
       

        #endregion

        #region Methods for controlling hardware

        public double[,] ReadAI(int numberOfMeasurements)
        {
            double[,] data = new double[analogInputs.Length , numberOfMeasurements];//Cheezy Bugfix
            try
            {
                data = analogReader.ReadMultiSample(numberOfMeasurements);
                readAIsTask.WaitUntilDone();
            }
            catch (DaqException e)
            {
                //data = null;
                System.Diagnostics.Debug.WriteLine(e.Message.ToString());
                DisposeAITask();
                ConfigureReadAI(numberOfMeasurements, false);
            }
            
            return data;
        }

       
        public void DisposeAITask()
        {
            readAIsTask.Dispose();
        }
        
        #endregion

    }
}
