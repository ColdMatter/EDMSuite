using DAQ.Environment;
using DAQ.HAL;
using DAQ.WavemeterLock;
using NationalInstruments.DAQmx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace WavemeterLock
{
    /// <summary>
    /// A base class to represent commonalities of Slave and Master lasers.
    /// </summary>
    public class Laser
    {

        public double PGain { get; set; }
        public double IGain { get; set; }
        public string Name;
        public string FeedbackChannel { get; set; }
        public double offsetVoltage { get; set; }
        public int WLMChannel { get; set; }
        public double summedWavelengthDifference = 0;
        private DAQMxWavemeterLockLaserControlHelper laser;

        public enum LaserState
        {
            FREE, LOCKED, OUTOFRANGE
        };

        public LaserState lState = LaserState.FREE;

        public virtual double setFrequency { get; set; }
        public double currentFrequency { get; set; }
        public double FrequencyError { get; set; }

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
                if (value < LowerVoltageLimit) // Want to make sure we don't try to send voltage that is too high or low so WML doesn't crash
                {
                    currentVoltage = LowerVoltageLimit;
                    lState = LaserState.OUTOFRANGE;
                }
                else if (value > UpperVoltageLimit)
                {
                    currentVoltage = UpperVoltageLimit;
                    lState = LaserState.OUTOFRANGE;
                }
                else
                {
                    currentVoltage = value;
                    if (lState == LaserState.OUTOFRANGE)
                        lState = LaserState.LOCKED;//If output voltage returns back to range, identify laser as locked
                }
                
                    laser.SetLaserVoltage(currentVoltage);
                
               
            }
            
        }

        public Laser(string name, string feedbackChannel, DAQMxWavemeterLockLaserControlHelper chelper)
        {
            laser = chelper;
            lState = LaserState.FREE;
            Name = name;
            FeedbackChannel = feedbackChannel;
            laser.ConfigureSetLaserVoltage(0.0);
            PGain = 0;
            IGain = 0;
            offsetVoltage = 0;
           
        }



        public void Lock()
        {
            lState = LaserState.LOCKED;
        }

        public void DisengageLock()
        {
            lState = LaserState.FREE;
        }

        

       
      
        public virtual void UpdateLock()
        {
            if (lState == LaserState.LOCKED)
            {
                FrequencyError = currentFrequency - setFrequency;
                summedWavelengthDifference += FrequencyError;
                CurrentVoltage = IGain * summedWavelengthDifference + PGain * FrequencyError + offsetVoltage;
            }
            
        }

        

        public virtual void ResetOutput()
        {
            CurrentVoltage = PGain * FrequencyError + offsetVoltage;
            CurrentVoltage = currentVoltage;
            summedWavelengthDifference = 0;
        }

        public void DisposeLaserControl()
        {
            laser.DisposeLaserTask();
        }
    }
}
