using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAQ.WavemeterLock
{
    public interface WavemeterLockLaserControllable
    {
        void ConfigureSetLaserVoltage(double initVoltage);
        void SetLaserVoltage(double voltage);
        void DisposeLaserTask();
    }
}
