using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DAQ.TransferCavityLock2012
{
    public interface TransferCavityLock2012LaserControllable
    {
        void ConfigureSetLaserVoltage(double initVoltage);
        void SetLaserVoltage(double voltage);
        void DisposeLaserTask();
    }
}
