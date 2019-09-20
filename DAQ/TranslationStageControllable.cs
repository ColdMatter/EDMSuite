using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DAQ
{
    public interface TranslationStageControllable
    {
        void TSConnect();
        void TSDisconnect();
        void TSInitialize(double acceleration, double deceleration, double distance, double velocity);
        void TSOn();
        void TSGo();
        void TSOff();
        void TSRead();
        void TSReturn();
        void TSRestart();
        void TSClear();
        void TSAutoTriggerEnable();
        void TSAutoTriggerDisable();
    }
}
