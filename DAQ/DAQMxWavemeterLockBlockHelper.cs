using DAQ.Environment;
using DAQ.HAL;
using NationalInstruments.DAQmx;

namespace DAQ.WavemeterLock
{
    public delegate void blockHandler();
    public class DAQMxWavemeterLockBlockHelper
    {
        public Task ReadDItask;
        private DigitalInputChannel DiChannel;
        private DigitalSingleChannelReader Reader;
        public event blockHandler LaserBlocked;
        public event blockHandler LaserRelocked;
        public bool IsBlocked = false;

        private string DigitalChannelName;
        private string Laser;

        public DAQMxWavemeterLockBlockHelper(string laserName, string channel)
        {
            DigitalChannelName = channel;
            Laser = laserName;
            ConfigureBlockChannel();
        }

        public void ConfigureBlockChannel()
        {
            ReadDItask = new Task("BlockLaserReader" + DigitalChannelName);
            DiChannel =
                    (DigitalInputChannel)Environs.Hardware.DigitalInputChannels[DigitalChannelName];
            DiChannel.AddToTask(ReadDItask);
            //readDItask.Timing.ConfigureChangeDetection(diChannel.PhysicalChannel, diChannel.PhysicalChannel, SampleQuantityMode.ContinuousSamples);
            //readDItask.SynchronizeCallbacks = true;
            //readDItask.DigitalChangeDetection += readDItask_DigitalChangeDetection;
            ReadDItask.Control(TaskAction.Verify);
            Reader = new DigitalSingleChannelReader(ReadDItask.Stream);
            ReadDItask.Start();

        }

        private void ReadDiTaskDigitalChangeDetection(object sender, DigitalChangeDetectionEventArgs e)
        {
            bool data = Reader.ReadSingleSampleSingleLine();
            IsBlocked = !data;
        }

        public void CheckLockBlockStatus()
        {
            IsBlocked = Reader.ReadSingleSampleSingleLine();
        }
    }
}