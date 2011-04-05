using System;
using System.Threading;

using NationalInstruments.DAQmx;

using DAQ.Environment;
using DAQ.HAL;

// this is the DAQMx implementation of TransferCavityLock.
// Note to self: this is where Tasks are created, where read and write commands are kept.

namespace DAQ.TransferCavityLock
{
    
    public class DAQMxTransferCavityLockHelper : TransferCavityLockable
    {        

        private Task outputLaserTask; //Some stuff to let you write to laser
        private AnalogOutputChannel laserChannel;
        private AnalogSingleChannelWriter laserWriter;

        private Task outputCavityTask; //Some stuff to let you write to piezo driver
        private AnalogOutputChannel cavityChannel;
        private AnalogSingleChannelWriter cavityWriter;

        private Task readPhotodiodesTask;
        private AnalogInputChannel referenceLaserChannel; // the signal from the reference He-Ne lock
        private AnalogInputChannel lockingLaserChannel; // signal from the laser we are trying to lock
        private AnalogMultiChannelReader photodiodesReader;

        private Task sendScanTriggerTask;
        private DigitalOutputChannel sendTriggerChannel;
        private DigitalSingleChannelWriter triggerWriter;

        private string cavityChannelName;
        private string laserChannelName;
        private string masterPDChannelName;
        private string slavePDChannelName;
        private string photodiodeTriggerInputName;
        private string cavityTriggerInputName;
        private string triggerOutput;

        public DAQMxTransferCavityLockHelper()
        {
            cavityChannelName = "cavity";
            laserChannelName = "laser";
            masterPDChannelName = "p2";
            slavePDChannelName = "p1";
            cavityTriggerInputName = "analogTrigger3";
            photodiodeTriggerInputName = "analogTrigger2";
            triggerOutput = "cavityTriggerOut";
            
        }
        public DAQMxTransferCavityLockHelper
            (string cavity, string cavityTriggerInput, string laser, string masterPD, 
            string slavePD, string photodiodeTriggerInput, string triggerOutputName)
        {
            cavityChannelName = cavity;
            laserChannelName = laser;
            masterPDChannelName = masterPD;
            slavePDChannelName = slavePD;
            cavityTriggerInputName = cavityTriggerInput;
            photodiodeTriggerInputName = photodiodeTriggerInput;
            this.triggerOutput = triggerOutputName;
        }

        #region Methods for configuring the hardware

        public void ConfigureCavityScan(int numberOfSteps, bool autostart)
        {
            
            outputCavityTask = new Task("CavityPiezoVoltage");
            cavityChannel =
                        (AnalogOutputChannel)Environs.Hardware.AnalogOutputChannels[cavityChannelName];
            cavityChannel.AddToTask(outputCavityTask, -10, 10);
            outputCavityTask.AOChannels[0].DataTransferMechanism = AODataTransferMechanism.Dma;

            if (!autostart)
            {
                outputCavityTask.Timing.ConfigureSampleClock("", 500,
                SampleClockActiveEdge.Rising, SampleQuantityMode.FiniteSamples, 2 * numberOfSteps);
                outputCavityTask.Triggers.StartTrigger.ConfigureDigitalEdgeTrigger(
                        (string)Environs.Hardware.GetInfo(cavityTriggerInputName), DigitalEdgeStartTriggerEdge.Rising);
            }

            outputCavityTask.Control(TaskAction.Verify);
            cavityWriter = new AnalogSingleChannelWriter(outputCavityTask.Stream);

        }
        //The photodiode inputs have been bundled into one task. We never read one photodiode without reading
        //the other.
        public void ConfigureReadPhotodiodes(int numberOfMeasurements, bool autostart) 
        {
            readPhotodiodesTask = new Task("ReadPhotodiodes");
            referenceLaserChannel = (AnalogInputChannel)Environs.Hardware.AnalogInputChannels[masterPDChannelName];
            lockingLaserChannel = (AnalogInputChannel)Environs.Hardware.AnalogInputChannels[slavePDChannelName];
            referenceLaserChannel.AddToTask(readPhotodiodesTask, 0, 10);
            lockingLaserChannel.AddToTask(readPhotodiodesTask, 0, 10);
            
            if (!autostart)
            {
                readPhotodiodesTask.Timing.ConfigureSampleClock(
                   "",
                   500,
                   SampleClockActiveEdge.Rising,
                   SampleQuantityMode.FiniteSamples, 2 * numberOfMeasurements);
                readPhotodiodesTask.Triggers.StartTrigger.ConfigureDigitalEdgeTrigger(
                    (string)Environs.Hardware.GetInfo(photodiodeTriggerInputName),
                    DigitalEdgeStartTriggerEdge.Rising);
            }
            readPhotodiodesTask.Control(TaskAction.Verify);
            photodiodesReader = new AnalogMultiChannelReader(readPhotodiodesTask.Stream);
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
        public void ConfigureScanTrigger() 
        {
            sendScanTriggerTask = new Task("Send Cavity UnlockCavity Trigger");
            sendTriggerChannel = (DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels[triggerOutput];
            sendTriggerChannel.AddToTask(sendScanTriggerTask);
            sendScanTriggerTask.Control(TaskAction.Verify);
            triggerWriter = new DigitalSingleChannelWriter(sendScanTriggerTask.Stream);
            triggerWriter.WriteSingleSampleSingleLine(true, false);
            sendScanTriggerTask.Start();
        }

        #endregion

        #region Methods for controlling hardware


        public void ScanCavity(double[] rampVoltages, bool autostart) 
        {
            cavityWriter.WriteMultiSample(autostart, rampVoltages);

        }
        public double[,] ReadPhotodiodes(int numberOfMeasurements) 
        {
            double[,] tempData = new double[2, numberOfMeasurements];
            tempData = photodiodesReader.ReadMultiSample(numberOfMeasurements);
            return tempData;
        }
        public void SetLaserVoltage(double voltage) 
        {
            outputLaserTask.Stop();
            laserWriter.WriteSingleSample(true, voltage);
            outputLaserTask.Start();
        }

        public void SendScanTriggerAndWaitUntilDone() 
        {            
            triggerWriter.WriteSingleSampleSingleLine(true, false);
            sendScanTriggerTask.Stop();
            triggerWriter.WriteSingleSampleSingleLine(true, true);
            sendScanTriggerTask.Start();
            
            outputCavityTask.WaitUntilDone();
            readPhotodiodesTask.WaitUntilDone();
        }
        public void StartScan()
        {
            outputCavityTask.Start();
            readPhotodiodesTask.Start();
        }
        public void StopScan()
        {
            outputCavityTask.Stop();
            readPhotodiodesTask.Stop();
        }
        
        public void ReleaseHardwareControl()
        {
            outputLaserTask.Dispose();
            readPhotodiodesTask.Dispose();
            outputCavityTask.Dispose();
            sendScanTriggerTask.Dispose();
        }
        #endregion

    }
}


