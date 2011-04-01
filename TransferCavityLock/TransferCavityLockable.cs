using System;
using System.Collections;

namespace TransferCavityLock
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
        void StartCavityScan();
        void StopCavityScan();

        double[,] ReadPhotodiodes(int numberOfMeasurements);
        void StartReadingPhotodiodes();
        void StopReadingPhotodiodes();

        void SetLaserVoltage(double voltage);
        void ReleaseHardwareControl();
        void ScanAndWait();


      
    }
    

}
