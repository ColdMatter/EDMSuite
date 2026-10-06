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
        // If another program (e.g. TCL/WavemeterLock) is currently locking this channel, the initial
        // voltage is not written, so starting this program doesn't knock that laser out of lock.
        public void ConfigureSetLaserVoltage(double voltage)
        {
            outputLaserTask = new Task("FeedbackToLaser" + laserChannelName);
            laserChannel =
                    (AnalogOutputChannel)Environs.Hardware.AnalogOutputChannels[laserChannelName];
            laserChannel.AddToTask(outputLaserTask, laserChannel.RangeLow, laserChannel.RangeHigh);
            outputLaserTask.Control(TaskAction.Verify);
            laserWriter = new AnalogSingleChannelWriter(outputLaserTask.Stream);
            string owner;
            if (SharedAnalogOutput.IsClaimedByOther(laserChannel, out owner))
            {
                Console.WriteLine(laserChannelName + ": not setting initial voltage, channel is in use by " + owner);
                return;
            }
            SharedAnalogOutput.WriteSingleSample(outputLaserTask, laserWriter, laserChannel, voltage);
        }

        #endregion

        #region Methods for controlling hardware

        public void SetLaserVoltage(double voltage)
        {
            string owner;
            TrySetLaserVoltage(voltage, out owner);
        }

        /// <summary>
        /// Writes the voltage unless another program has claimed this channel. Returns false (with the owner)
        /// if the channel is in use elsewhere, or if the write itself failed (owner is then null).
        /// </summary>
        public bool TrySetLaserVoltage(double voltage, out string owner)
        {
            owner = null;
            bool alreadyClaimed = SharedAnalogOutput.IsClaimedByThisProcess(laserChannel);
            if (!alreadyClaimed && !SharedAnalogOutput.TryClaim(laserChannel, out owner)) return false;
            try
            {
                return SharedAnalogOutput.WriteSingleSample(outputLaserTask, laserWriter, laserChannel, voltage);
            }
            finally
            {
                // A one-off write (e.g. manual voltage while unlocked) only holds the claim while writing.
                if (!alreadyClaimed) SharedAnalogOutput.Release(laserChannel);
            }
        }

        /// <summary>Claim this channel for the duration of a lock, so other programs won't write to it.</summary>
        public bool TryClaimChannel(out string owner)
        {
            return SharedAnalogOutput.TryClaim(laserChannel, out owner);
        }

        public void ReleaseChannel()
        {
            SharedAnalogOutput.Release(laserChannel);
        }

        public void DisposeLaserTask()
        {
            SharedAnalogOutput.Release(laserChannel);
            outputLaserTask.Dispose();
        }
        #endregion

    }
}
