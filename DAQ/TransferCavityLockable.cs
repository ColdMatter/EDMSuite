using System;
using System.Collections;

namespace DAQ.TransferCavityLock
{
    /// <summary>
    /// This is an interface for all the capabilities necessary for a transfer cavity lock.
    /// </summary>
    public interface TransferCavityLockable
    {

        void ConfigureCavityScan(int numberOfSteps, bool autostart);
        void ConfigureReadPhotodiodes(int numberOfMeasurements, bool autostart);
        void ConfigureSetLaserVoltage(double voltage);
        void ConfigureScanTrigger();

        void ScanCavity(double[] rampVoltages, bool autostart);
        double[,] ReadPhotodiodes(int numberOfMeasurements);

        void StartScan();
        void StopScan();
        
        void SetLaserVoltage(double voltage);
        void ReleaseCavityHardware();
        void ReleaseLaser();
        void SendScanTriggerAndWaitUntilDone();

    }
    

}
