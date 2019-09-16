using System;
using System.Threading;

using NationalInstruments.DAQmx;
using DAQ.Environment;
using DAQ.HAL;

namespace DAQ.TransferCavityLock2012
{
    public class DAQMxTCL2012LaserControlHelper : TransferCavityLock2012LaserControllable
    {
        //In this helper, there is a single analog output channel for controlling the laser. each slave laser class must have one.
        private Task outputLaserTask; //Some stuff to let you write to laser
        private AnalogOutputChannel laserChannel;
        private AnalogSingleChannelWriter laserWriter;

        private string laserChannelName;
        
        public DAQMxTCL2012LaserControlHelper()
        {
            laserChannelName = "laser";
        }

        public DAQMxTCL2012LaserControlHelper(string laser)
        {
            laserChannelName = laser;
        }


        #region Methods for configuring the hardware
        
        //This takes in a voltage. A bit cheezy, but I needed the laser
        // voltage to be set as soon value as soon as it gets configured.
        public void ConfigureSetLaserVoltage(double voltage)
        {
            outputLaserTask = new Task("FeedbackToLaser" + laserChannelName);
            laserChannel =
                    (AnalogOutputChannel)Environs.Hardware.AnalogOutputChannels[laserChannelName];
            laserChannel.AddToTask(outputLaserTask, laserChannel.RangeLow, laserChannel.RangeHigh);
            outputLaserTask.Control(TaskAction.Verify);
            laserWriter = new AnalogSingleChannelWriter(outputLaserTask.Stream);
            laserWriter.WriteSingleSample(true, voltage);

            //outputLaserTask.Start();
        
        }

        #endregion

        #region Methods for controlling hardware

        public void SetLaserVoltage(double voltage)
        {
            outputLaserTask.Start();
            laserWriter.WriteSingleSample(true, voltage);
            outputLaserTask.Stop();
        }
        public void DisposeLaserTask()
        {
            outputLaserTask.Dispose();
        }
        #endregion

    }
}
