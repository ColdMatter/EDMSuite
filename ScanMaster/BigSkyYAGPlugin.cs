using System;
using System.Threading;

using DAQ.Environment;
using DAQ.HAL;
using ScanMaster.Acquire.Plugin;

namespace ScanMaster.Acquire.Plugins
{
    /// <summary>
    /// A plugin to control the not in the least bit Brilliant laser.
    /// </summary>
    [Serializable]
    public class BigSkyYAGPlugin : YAGPlugin
    {
        [NonSerialized]
        BrilliantLaser laser;

        protected override void InitialiseSettings()
        {
        }

        public override void AcquisitionStarting()
        {
            laser = (BrilliantLaser)Environs.Hardware.YAG;
            // this ensures the pattern generator has established itself (if not, an interlock fail
            // will result).
            Thread.Sleep(1000);
            laser.StartFlashlamps(false);
            // this gives the laser time to warm up before firing the Q-switch (too short and the
            // q-switch will not enable - if the first scan never works, this is the problem).
            Thread.Sleep(9500);
        }

        public override void ScanStarting()
        {
            laser.EnableQSwitch();
        }

        public override void ScanFinished()
        {
            laser.DisableQSwitch();
        }

        public override void AcquisitionFinished()
        {
            laser.StopFlashlamps();
        }

    }
}
