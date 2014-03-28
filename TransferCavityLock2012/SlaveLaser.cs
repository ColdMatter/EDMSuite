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
    public class SlaveLaser
    {
        public SlaveLaser(string name)
        {
            lState = LaserState.FREE;
            this.Name = name;
            laser = new DAQMxTCL2012LaserControlHelper(Name);
            laser.ConfigureSetLaserVoltage(0.0);
        }

        public string Name;
       
        private int increments = 0;          // for tweaking the laser set point
        private int decrements = 0;
        public double SetPointIncrementSize = 0.01;
        

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
            controller.UpdateUIState(Name, lState);
        }
        public void Lock()
        {
            lState = LaserState.LOCKED;
            controller.UpdateUIState(Name, lState);
        }
        public void DisengageLock()
        {
            lState = LaserState.FREE;
            laser.SetLaserVoltage(VoltageToLaser);  
            controller.UpdateUIState(Name, lState);
        }
        public void DisposeLaserControl()
        {
            laser.DisposeLaserTask();
        }
        public void SetLaserVoltage()
        {
            laser.SetLaserVoltage(VoltageToLaser);
        }


        public double UpperVoltageLimit
        {
            get
            {
                return ((AnalogOutputChannel)Environs.Hardware.AnalogOutputChannels[Name]).RangeHigh;
            }
        }

        public double LowerVoltageLimit
        {
            get
            {
                return ((AnalogOutputChannel)Environs.Hardware.AnalogOutputChannels[Name]).RangeLow;
            }
        }

        private double voltageToLaser = (double)Environs.Hardware.GetInfo("TCL_Default_VoltageToLaser");
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
        private double gain = (double)Environs.Hardware.GetInfo("TCL_Default_Gain");
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
        public void AddSetPointIncrement()
        {
            increments++;
        }
        public void AddSetPointDecrement()
        {
            decrements++;
        }

        public void CalculateLaserSetPoint(double[] masterFitCoefficients, double[] slaveFitCoefficients)
        {
            LaserSetPoint = calculateLaserSetPoint(masterFitCoefficients, slaveFitCoefficients);
        }
        public void RefreshLock(double[] masterFitCoefficients, double[] slaveFitCoefficients)
        {
            LaserSetPoint = tweakSetPoint(LaserSetPoint); //does nothing if not tweaked
            double shift = calculateDeviationFromSetPoint(LaserSetPoint, masterFitCoefficients, slaveFitCoefficients);
            VoltageToLaser = calculateNewVoltageToLaser(VoltageToLaser, shift);
            if (lState != LaserState.FREE)
            {
                SetLaserVoltage(); //Actually sends to Hardware.
            }
        }
       
        #region privates


        private double calculateLaserSetPoint(double[] masterFitCoefficients, double[] slaveFitCoefficients)
        {
            return slaveFitCoefficients[1] - masterFitCoefficients[1];
        }


        private double tweakSetPoint(double oldSetPoint)
        {
            double newSetPoint = oldSetPoint + SetPointIncrementSize * (increments - decrements);
            increments = 0;
            decrements = 0;
            return newSetPoint;
        }


        public double calculateDeviationFromSetPoint(double laserSetPoint,
            double[] masterFitCoefficients, double[] slaveFitCoefficients)
        {
            double currentPeakSeparation = new double();
            currentPeakSeparation = slaveFitCoefficients[1] - masterFitCoefficients[1];
            return currentPeakSeparation - LaserSetPoint;
            
        }

        private double calculateNewVoltageToLaser(double vtolaser, double measuredVoltageChange)
        {
            double newVoltage;
            if (vtolaser
                + Gain * measuredVoltageChange > UpperVoltageLimit
                || vtolaser
                + Gain * measuredVoltageChange < LowerVoltageLimit)
            {
                newVoltage = vtolaser;
            }
            else
            {
                newVoltage = vtolaser + Gain * measuredVoltageChange; //Feedback 
            }
            return newVoltage;
        }

        #endregion


       }
}
