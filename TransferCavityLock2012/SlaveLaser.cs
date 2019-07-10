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
        private double voltageError;
        private double previousVoltageError;
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

        public override double VoltageError
        {
            get
            {
                double voltageDifferenceFromMaster = 0;

                switch (lPeakFindingMode)
                {
                    case PeakFindingMode.DIGITAL:
                        voltageDifferenceFromMaster = Fit.Centre - ParentCavity.Master.Fit.Centre;
                        break;
                    case PeakFindingMode.ANALOG:
                        voltageDifferenceFromMaster = PeakRampPosition - ParentCavity.Master.PeakRampPosition;
                        break;
                }
                voltageError = voltageDifferenceFromMaster - LaserSetPoint;
                return voltageError;
                
            }
        }

        public override double VoltageErrorDifferenceFromLast
        {
            get 
            {
                return voltageError - previousVoltageError;
            }
        }

        public double PreviousVoltageError
        {
            get
            {
                return previousVoltageError;
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

        protected override void Lock()
        {
            base.Lock();
            // Set the initial lock point
            LaserSetPoint = Fit.Centre - ParentCavity.Master.Fit.Centre;
            // Initialise error tracking
            previousVoltageError = 0.0;
            oldFrequencyErrors = new List<double>();
            lockCount = 0;
        }

        public override void UpdateLock()
        {
            if (!lockBlocked)
            {
                base.UpdateLock();
                if (lState == LaserState.LOCKED)
                {
                    previousVoltageError = voltageError;
                    oldFrequencyErrors.Add(FrequencyError);
                    if (oldFrequencyErrors.Count > ParentCavity.Controller.numScanAverages)
                    {
                        oldFrequencyErrors.RemoveAt(0);
                    }
                    lockCount++;
                }
            }
        }
    }
}
