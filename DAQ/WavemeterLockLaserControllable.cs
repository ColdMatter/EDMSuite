namespace DAQ.WavemeterLock
{
    public interface WavemeterLockLaserControllable
    {
        void ConfigureSetLaserVoltage(double initVoltage);
        void SetLaserVoltage(double voltage);
        void DisposeLaserTask();
    }
}
