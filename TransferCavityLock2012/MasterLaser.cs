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
        private double voltageError;

        public MasterLaser(string feedbackChannel, string photoDiode, Cavity cavity)
            : base(feedbackChannel, photoDiode, cavity)
        {
        }

        public override double VoltageError
        {
            get 
            {
                switch (lPeakFindingMode)
                {
                    case PeakFindingMode.DIGITAL:
                        voltageError = Fit.Centre - LaserSetPoint;
                        break;
                    case PeakFindingMode.ANALOG:
                        voltageError = PeakRampPosition - LaserSetPoint;
                        break;
                }
                return voltageError; 
            }
        }

        public override double VoltageErrorDifferenceFromLast
        {
            get 
            {
                //Not implementing proportional feedback for master laser
                return 0;
            }
        }
    }
}
