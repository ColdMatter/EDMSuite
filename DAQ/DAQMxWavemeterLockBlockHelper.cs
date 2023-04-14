using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NationalInstruments.DAQmx;
using DAQ.Environment;
using DAQ.HAL;

namespace DAQ.WavemeterLock
{
    public delegate void blockHandler();
    public class DAQMxWavemeterLockBlockHelper
    {
        public Task readDItask;
        private DigitalInputChannel diChannel;
        private DigitalSingleChannelReader reader;
        public event blockHandler laserBlocked;
        public event blockHandler laserRelocked;
        public bool isBlocked = false;

        private string digitalChannelName;
        private string laser;

        public DAQMxWavemeterLockBlockHelper(string laserName, string channel)
        {
            digitalChannelName = channel;
            laser = laserName;
            ConfigureBlockChannel();
        }

        public void ConfigureBlockChannel()
        {
            readDItask = new Task("BlockLaserReader" + digitalChannelName);
            diChannel =
                    (DigitalInputChannel)Environs.Hardware.DigitalInputChannels[digitalChannelName];
            diChannel.AddToTask(readDItask);
            readDItask.Timing.ConfigureChangeDetection(diChannel.PhysicalChannel, diChannel.PhysicalChannel, SampleQuantityMode.ContinuousSamples);
            readDItask.SynchronizeCallbacks = true;
            readDItask.DigitalChangeDetection += readDItask_DigitalChangeDetection;
            readDItask.Control(TaskAction.Verify);
            reader = new DigitalSingleChannelReader(readDItask.Stream);
            readDItask.Start();

        }

        private void readDItask_DigitalChangeDetection(object sender, DigitalChangeDetectionEventArgs e)
        {
            bool[] data = reader.ReadSingleSampleMultiLine();
            isBlocked = !data[0];
        }

    }
}
