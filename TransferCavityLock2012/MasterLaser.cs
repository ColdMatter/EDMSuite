using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DAQ.Environment;
using DAQ.TransferCavityLock2012;
using NationalInstruments.DAQmx;
using DAQ.HAL;

namespace TransferCavityLock2012
{
    public class MasterLaser : Laser
    {
        public MasterLaser(string feedbackChannel, string photoDiode, Cavity cavity)
            : base(feedbackChannel, photoDiode, cavity)
        {
        }

        public override void UpdateLock()
        {
            switch (lState)
            {
                case LaserState.LOCKING:
                    CurrentVoltage = CurrentVoltage - Gain * (LaserSetPoint - Fit.Centre);
                    Lock();
                    break;

                case LaserState.LOCKED:
                    CurrentVoltage = CurrentVoltage - Gain * (LaserSetPoint - Fit.Centre);
                    break;

                case LaserState.FREE:
                    break;
            }
        }
    }
}
