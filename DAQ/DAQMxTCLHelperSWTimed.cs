using System;
using System.Threading;

using NationalInstruments.DAQmx;

using DAQ.Environment;
using DAQ.HAL;

// this is the DAQMx implementation of TransferCavityLock.
// Note to self: this is where Tasks are created, where read and write commands are kept.

namespace DAQ.TransferCavityLock
{
    
    public class DAQMxTCLHelperSWTimed : TransferCavityLockable
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

        private string cavityChannelName;
        private string laserChannelName;
        private string masterPDChannelName;
        private string slavePDChannelName;
        private string photodiodeTriggerInputName;
        private string cavityTriggerInputName;
        private string triggerOutput;

        public DAQMxTCLHelperSWTimed()
        {
            cavityChannelName = "cavity";
            laserChannelName = "laser";
            masterPDChannelName = "p2";
            slavePDChannelName = "p1";
            cavityTriggerInputName = "analogTrigger3";
            photodiodeTriggerInputName = "analogTrigger2";
            triggerOutput = "cavityTriggerOut";
            
        }
        public DAQMxTCLHelperSWTimed
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
            cavityChannel.AddToTask(outputCavityTask, 0, 10);
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
        }

        #endregion

        #region Methods for controlling hardware

        double[] rampV;
        double[,] pdData;
        public void ScanCavity(double[] rampVoltages, bool autostart) 
        {
            rampV = rampVoltages;
        }
        public double[,] ReadPhotodiodes(int numberOfMeasurements) 
        {
            return pdData;
        }
        public void SetLaserVoltage(double voltage) 
        {
            outputLaserTask.Stop();
            laserWriter.WriteSingleSample(true, voltage);
            outputLaserTask.Start();
        }

        public void SendScanTriggerAndWaitUntilDone() 
        {
            scanStop = false;
            pdData = new double[2, rampV.Length];
            for( int i = 0; i < rampV.Length; i++)
            {
                cavityWriter.WriteSingleSample(true, rampV[i]);
                double[] pds = photodiodesReader.ReadSingleSample();
                pdData[0, i] = pds[0];
                pdData[1, i] = pds[1];
                Thread.Sleep(2);
                lock (scanStopLock)
                {
                    if (scanStop) return;
                }
            }
        }
        public void StartScan()
        {
        }
        bool scanStop = false;
        object scanStopLock = new object();
        public void StopScan()
        {
            // not sure we need this lock? Jony
            lock (scanStopLock) scanStop = true;
        }
        
        public void ReleaseCavityHardware()
        {
            readPhotodiodesTask.Dispose();
            outputCavityTask.Dispose();
        }
        public void ReleaseLaser()
        {
            outputLaserTask.Dispose();
        }
        #endregion

    }
}


