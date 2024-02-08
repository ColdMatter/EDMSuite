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
    /// A base class to represent slave lasers.
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
        public double sumedNoise = 0.0;
        public double RMSNoise = 0.0;
        public int loopCount = 0;

        public bool isBlocked = false;
        public bool logData = false;
        public enum LaserState
        {
            FREE, LOCKED, OUTOFRANGE
        };

        public LaserState lState = LaserState.FREE;

        public virtual double setFrequency { get; set; } //THz
        public double currentFrequency { get; set; } //THz
        public double FrequencyError { get; set; } //THz
        public bool isOutOfRange { get; set; } //Mark if output voltage is out of range

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
                if (value < LowerVoltageLimit) //Keeps output voltage in the range of the board
                {
                    currentVoltage = LowerVoltageLimit;
                    isOutOfRange = true;
                }
                else if (value > UpperVoltageLimit)
                {
                    currentVoltage = UpperVoltageLimit;
                    isOutOfRange = true;
                }
                else
                {
                    currentVoltage = value;
                    isOutOfRange = false;
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
            isOutOfRange = false;
           
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
                sumedNoise += (FrequencyError * 1000000) * (FrequencyError * 1000000);
                loopCount++;
                RMSNoise = Math.Sqrt(sumedNoise / (double)loopCount);
            }
            
        }

        public virtual void UpdateBlockedLock()
        {
            FrequencyError = currentFrequency - setFrequency;
        }



        public virtual void ResetOutput() //Clear the I lock output
        {
            CurrentVoltage = offsetVoltage;
            CurrentVoltage = currentVoltage;
            summedWavelengthDifference = 0;
        }

        public void DisposeLaserControl()
        {
            laser.DisposeLaserTask();
        }
    }
}
