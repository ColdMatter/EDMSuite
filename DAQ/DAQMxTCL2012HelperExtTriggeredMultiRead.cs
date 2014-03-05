using System;
using System.Threading;

using NationalInstruments.DAQmx;
using DAQ.Environment;
using DAQ.HAL;

namespace DAQ.TransferCavityLock2012
{
    public class DAQMxTCL2012HelperExtTriggeredMultiRead : TransferCavity2012Lockable
    {
        //In this helper, the cavity is scanned externally (not from the computer). The software waits for a trigger pulse to start scanning.
        //An additional AI read is added to this helper. It's for reading off the cavity voltage.
        //I think this file is obsolete
        private Task outputLaserTask; //Some stuff to let you write to laser
        private AnalogOutputChannel laserChannel;
        private AnalogSingleChannelWriter laserWriter;

        private Task readAIsTask;
        private AnalogInputChannel referenceLaserChannel; // the signal from the reference He-Ne lock
        private AnalogInputChannel lockingLaserChannel; // signal from the laser we are trying to lock
        private AnalogInputChannel cavityVoltageChannel;
        private AnalogMultiChannelReader analogReader;

        private string laserChannelName;
        private string cavityReadChannelName;
        private string masterPDChannelName;
        private string slavePDChannelName;
        private string AITriggerInputName;

        public DAQMxTCL2012HelperExtTriggeredMultiRead()
        {
            laserChannelName = "laser";
            masterPDChannelName = "p2";
            slavePDChannelName = "p1";
            AITriggerInputName = "analogTrigger2";
            cavityReadChannelName = "cavityVoltageRead";
        }

        public DAQMxTCL2012HelperExtTriggeredMultiRead(string laser, string masterPD,string slavePD, string cavityRead, string aiTriggerInput)
        {
            laserChannelName = laser;
            masterPDChannelName = masterPD;
            slavePDChannelName = slavePD;
            AITriggerInputName = aiTriggerInput;
            cavityReadChannelName = cavityRead;
        }

        #region Methods for configuring the hardware

        //The photodiode inputs have been bundled into one task. We never read one photodiode without reading
        //the other.
        public void ConfigureReadAI(int numberOfMeasurements, bool autostart) //AND CAVITY VOLTAGE!!! 
        {
            readAIsTask = new Task("readAI");
            referenceLaserChannel = (AnalogInputChannel)Environs.Hardware.AnalogInputChannels[masterPDChannelName];
            lockingLaserChannel = (AnalogInputChannel)Environs.Hardware.AnalogInputChannels[slavePDChannelName];
            cavityVoltageChannel = (AnalogInputChannel)Environs.Hardware.AnalogInputChannels[cavityReadChannelName];
            referenceLaserChannel.AddToTask(readAIsTask, 0, 10);
            lockingLaserChannel.AddToTask(readAIsTask, 0, 10);
            cavityVoltageChannel.AddToTask(readAIsTask, 0, 10);
            if (autostart == false)
            {
                 readAIsTask.Timing.ConfigureSampleClock(
                    "",
                    66000,
                    SampleClockActiveEdge.Rising,
                    SampleQuantityMode.FiniteSamples, numberOfMeasurements);
                readAIsTask.Triggers.StartTrigger.ConfigureDigitalEdgeTrigger(
                    (string)Environs.Hardware.GetInfo(AITriggerInputName),
                    DigitalEdgeStartTriggerEdge.Rising);
            }
            readAIsTask.Control(TaskAction.Verify);
            analogReader = new AnalogMultiChannelReader(readAIsTask.Stream);
        }
        //This takes in a voltage. A bit cheezy, but I needed the laser
        // voltage to be set as soon value as soon as it gets configured.
        public void ConfigureSetLaserVoltage(double voltage)
        {
            outputLaserTask = new Task("FeedbackToLaser");
            laserChannel =
                    (AnalogOutputChannel)Environs.Hardware.AnalogOutputChannels[laserChannelName];
            laserChannel.AddToTask(outputLaserTask, -10, 10);
            outputLaserTask.Control(TaskAction.Verify);
            laserWriter = new AnalogSingleChannelWriter(outputLaserTask.Stream);
            laserWriter.WriteSingleSample(true, voltage);
            outputLaserTask.Start();
        }

        #endregion

        #region Methods for controlling hardware

        public double[,] ReadAI(int numberOfMeasurements)
        {
            double[,] data = new double[3 ,numberOfMeasurements];//Cheezy Bugfix
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

        public void SetLaserVoltage(double voltage) 
        {
            outputLaserTask.Stop();
            laserWriter.WriteSingleSample(true, voltage);
            outputLaserTask.Start();
        }
        
        public void DisposeAITask()
        {
            readAIsTask.Dispose();
        }
        public void DisposeLaserTask()
        {
            outputLaserTask.Dispose();
        }
        #endregion

    }
}
