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
    /// A class to represent the laser you are trying to lock.
    /// It knows how to calculate what voltage to send to the laser based on the fit coefficients from the data,
    /// and it knows how to control the laser (through a helper interface only).
    /// </summary>
    public class MasterLaser
    {
        public MasterLaser()
        {
            lState = LaserState.FREE;
            
        }

        private double gain;      

        public Controller controller;
        private TransferCavityLock2012LaserControllable laser;
        
        public enum LaserState
        {
            FREE, LOCKING, LOCKED
        };
        public LaserState lState = LaserState.FREE;


        public void ArmLock()
        {
            
            lState = LaserState.LOCKING;
        }
        public void Lock()
        {
            lState = LaserState.LOCKED;
        }
        public void DisengageLock()
        {
            lState = LaserState.FREE;
        }
        public void DisposeLaserControl()
        {
            laser.DisposeLaserTask();
        }
        public void SetLaserVoltage()
        {
            laser.SetLaserVoltage(VoltageToLaser);
        }

        private double voltageToLaser;
        public double VoltageToLaser
        {
            get
            {
                return voltageToLaser;
            }
            set
            {
                voltageToLaser = value;
            }
        }
        public double Gain
        {
            get
            {
                return gain;
            }
            set
            {
                gain = value;
            }
        }

        private double laserSetPoint;
        public double LaserSetPoint
        {
            get
            {
                return laserSetPoint;
            }
            set
            {
                laserSetPoint = value;
            }
        }
        public void CalculateLaserSetPoint(double[] masterFitCoefficients, double[] slaveFitCoefficients)
        {
            LaserSetPoint = calculateLaserSetPoint(masterFitCoefficients, slaveFitCoefficients);
        }
       
        #region privates


        private double calculateLaserSetPoint(double[] masterFitCoefficients, double[] slaveFitCoefficients)
        {
            return slaveFitCoefficients[1] - masterFitCoefficients[1];
        }




        public double calculateDeviationFromSetPoint(double laserSetPoint,
    double[] masterFitCoefficients, double[] slaveFitCoefficients)
        {
            double currentPeakSeparation = new double();
            currentPeakSeparation = slaveFitCoefficients[1] - masterFitCoefficients[1];
            return currentPeakSeparation - LaserSetPoint;

        }


        #endregion


       }
}
