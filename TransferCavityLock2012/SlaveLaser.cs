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
    /// <summary>
    /// A class to represent the laser you are trying to lock.
    /// It knows how to calculate what voltage to send to the laser based on the fit coefficients from the data,
    /// and it knows how to control the laser (through a helper interface only).
    /// </summary>
    public class SlaveLaser : Laser
    {
        public double FSRCalibration { get; set; }
        public string BlockChannel { get; set; }
        private int lockCount;
        private List<double> oldFrequencyErrors;

        public SlaveLaser(string feedbackChannel, string photoDiode, Cavity cavity)
            : base(feedbackChannel, photoDiode, cavity)
        {
        }

        public int LockCount
        {
            get
            {
                return lockCount;
            }
        }

        public double VoltageError
        {
            get
            {
                double voltageDifferenceFromMaster = Fit.Centre - ParentCavity.Master.Fit.Centre;
                return voltageDifferenceFromMaster - LaserSetPoint;
            }
        }

        public double FrequencyError
        {
            get
            {
                return 1500 * VoltageError / FSRCalibration;
            }
        }

        public List<double> OldFrequencyErrors
        {
            get
            {
                return oldFrequencyErrors;
            }
        }

        public override double LaserSetPoint
        {
            get
            {
                return base.LaserSetPoint;
            }
            set
            {
                base.LaserSetPoint = value;
                ParentCavity.Controller.UpdateSetPointInGUI(ParentCavity.Name, Name, value);
            }
        }

        public override void UpdateScan(double[] rampData, double[] scanData, bool shouldBlock)
        {
            base.UpdateScan(rampData, scanData, shouldBlock);
            if (lState == LaserState.LOCKING && !lockBlocked)
            {
                double differenceFromMaster = Fit.Centre - ParentCavity.Master.Fit.Centre;
                LaserSetPoint = differenceFromMaster;
            }
        }

        public override void UpdateLock()
        {
            if (!lockBlocked)
            {
                switch (lState)
                {
                    case LaserState.LOCKING:
                        oldFrequencyErrors = new List<double>();
                        oldFrequencyErrors.Add(FrequencyError);
                        lockCount = 1;
                        Lock();
                        break;

                    case LaserState.LOCKED:
                        CurrentVoltage = CurrentVoltage + Gain * VoltageError;
                        oldFrequencyErrors.Add(FrequencyError);
                        if (oldFrequencyErrors.Count > ParentCavity.Controller.numScanAverages)
                        {
                            oldFrequencyErrors.RemoveAt(0);
                        }
                        lockCount++;
                        break;

                    case LaserState.FREE:
                        break;
                }
            }
        }
    }
}
