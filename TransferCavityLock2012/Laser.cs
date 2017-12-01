using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DAQ.Environment;
using DAQ.TransferCavityLock2012;
using NationalInstruments.DAQmx;
using DAQ.Environment;
using DAQ.HAL;

namespace TransferCavityLock2012
{
    /// <summary>
    /// A base class to represent commonalities of Slave and Master lasers.
    /// </summary>
    public abstract class Laser
    {
        public double Gain { get; set; }
        public string Name;
        public string FeedbackChannel;
        public LorentzianFit Fit { get; set; }
        public double[] LatestScanData { get; set; }
        public String PhotoDiodeChannel;
        public Cavity ParentCavity;
        private TransferCavityLock2012LaserControllable laser;
        private double laserSetPoint;

        public enum LaserState
        {
            FREE, LOCKING, LOCKED
        };
        public LaserState lState = LaserState.FREE;

        public virtual double LaserSetPoint { get; set; }

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
                currentVoltage = value;
                laser.SetLaserVoltage(value);
            }
        }

        public Laser(string feedbackChannel, string photoDiode, Cavity cavity)
        {
            lState = LaserState.FREE;
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

        protected void Lock()
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

        public virtual void UpdateScan(double[] rampData, double[] scanData)
        {
            LatestScanData = scanData;
            switch (lState)
            {
                case LaserState.LOCKING:
                    double background = scanData.Min();
                    double maximum = scanData.Max();
                    double amplitude = maximum - background;
                    double centre = rampData[Array.IndexOf(scanData, maximum)];
                    double width = (rampData.Max() - rampData.Min()) / 20;
                    LorentzianFit bestGuessFit = new LorentzianFit(background, amplitude, centre, width);
                    Fit = CavityScanFitHelper.FitLorentzianToData(rampData, scanData, bestGuessFit);
                    break;

                case LaserState.LOCKED:
                    Fit = CavityScanFitHelper.FitLorentzianToData(rampData, scanData, Fit);
                    break;

                case LaserState.FREE:
                    break;
            }
        }

        public abstract void UpdateLock();

        public void DisposeLaserControl()
        {
            laser.DisposeLaserTask();
        }
    }
}
