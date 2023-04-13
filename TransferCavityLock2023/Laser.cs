using DAQ.Environment;
using DAQ.HAL;
using DAQ;
using DAQ.TransferCavityLock2012;
using NationalInstruments.DAQmx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TransferCavityLock2023
{
    /// <summary>
    /// A base class to represent commonalities of Slave and Master lasers.
    /// </summary>
    public abstract class Laser
    {
        public double Gain { get; set; }
        public string Name;
        public string FeedbackChannel;
        public double? peakLocation { get; set; }
        public double[] LatestScanData { get; set; }
        public String PhotoDiodeChannel;
        public Cavity ParentCavity;
        private TransferCavityLock2012LaserControllable laser;
        protected bool lockBlocked;

        public enum LaserState
        {
            FREE, LOCKING, LOCKED
        };
        public LaserState lState = LaserState.FREE;

        public virtual double LaserSetPoint { get; set; }
        public abstract double VoltageError { get; }

        public double UpperVoltageLimit
        {
            get
            {
                return ((AnalogOutputChannel)Environs.Hardware.AnalogOutputChannels[FeedbackChannel]).RangeHigh;
            }
        }

        public double LowerVoltageLimit
        {
            get
            {
                return ((AnalogOutputChannel)Environs.Hardware.AnalogOutputChannels[FeedbackChannel]).RangeLow;
            }
        }

        private double currentVoltage;
        public double CurrentVoltage
        {
            get
            {
                return currentVoltage;
            }

            set
            {
                if (value < LowerVoltageLimit) // Want to make sure we don't try to send voltage that is too high or low so TCL doesn't crash
                {
                    currentVoltage = LowerVoltageLimit;
                }
                else if (value > UpperVoltageLimit)
                {
                    currentVoltage = UpperVoltageLimit;
                }
                else
                {
                    currentVoltage = value;
                }
                laser.SetLaserVoltage(currentVoltage);
            }
        }

        public Laser(string feedbackChannel, string photoDiode, Cavity cavity)
        {
            laser = new DAQMxTCL2012LaserControlHelper(feedbackChannel);
            lState = LaserState.FREE;
            Name = feedbackChannel;
            FeedbackChannel = feedbackChannel;
            PhotoDiodeChannel = photoDiode;
            ParentCavity = cavity;
            laser.ConfigureSetLaserVoltage(0.0);
        }

        public void ArmLock()
        {
            lState = LaserState.LOCKING;
        }

        protected virtual void Lock()
        {
            lState = LaserState.LOCKED;
        }

        public void DisengageLock()
        {
            lState = LaserState.FREE;
        }

        public bool IsLocked
        {
            get { return lState == LaserState.LOCKED; }
        }

        public virtual void UpdateScan(double[] rampData, double[] scanData, bool shouldBlock)
        {
            LatestScanData = scanData;
            lockBlocked = shouldBlock;
            if (!lockBlocked)
            {
                switch (lState)
                {

                    case LaserState.LOCKED:
                        peakLocation = PeakFinder.findPeak(rampData, scanData);
                        break;

                    case LaserState.LOCKING:
                        peakLocation = PeakFinder.findPeak(rampData, scanData);
                        Lock();
                        break;

                    case LaserState.FREE:
                        peakLocation = null;
                        break;
                }
            }
        }

        public virtual void UpdateLock()
        {
            if (lState == LaserState.LOCKED)
            {
                CurrentVoltage = CurrentVoltage + Gain * VoltageError;
            }
        }

        public void DisposeLaserControl()
        {
            laser.DisposeLaserTask();
        }
    }
}
