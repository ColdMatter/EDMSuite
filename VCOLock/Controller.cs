using System;
using System.Collections;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using DAQ.Environment;
using DAQ.HAL;
using DAQ.TransferCavityLock;
using NationalInstruments.DAQmx;

namespace VCOLock
{
    /// <summary>
    /// Class for PI locking of a voltage controlled oscillator
    /// </summary>
    public class Controller
    {
        ControlWindow window;
        FrequencyCounter counter = (FrequencyCounter)Environs.Hardware.Instruments["counter"];
        
        /// <summary>
        /// (Not main) entry point for application 
        /// </summary>
        public void Start()
        {
            window = new ControlWindow();
            window.controller = this;
            Application.Run(window);
        }

        #region variables for lock

        /// <summary>
        /// Set point for generator frequency to be compared to
        /// </summary>
        public double FrequencySetPoint{ 
            get
            {
                return double.Parse(window.freqSetpointTextBox.Text);
            } 
            set
            {
                window.SetTextBox(window.freqSetpointTextBox, value.ToString());
            } 
        }

        /// <summary>
        /// Current VCO output frequency
        /// </summary>
        public double CurrentFrequency
        {
            get
            {
                UpdateFrequencyCounter();
                return double.Parse(window.freqCounterTextBox.Text);
            }
            set
            {
                window.SetTextBox(window.freqCounterTextBox, value.ToString());
            }
        }

        /// <summary>
        /// Proportional gain
        /// </summary>
        public double Kp
        {
            get
            {
                return double.Parse(window.intGainTextBox.Text);
            }
            set
            {
                window.SetTextBox(window.intGainTextBox, value.ToString());
            }
        }

        /// <summary>
        /// Integral gain
        /// </summary>
        public double Ki
        {
            get
            {
                return double.Parse(window.intGainTextBox.Text);
            }
            set
            {
                window.SetTextBox(window.intGainTextBox, value.ToString());
            }
        }

        /// <summary>
        /// Poll interval (ms)
        /// </summary>
        public int PollPeriod
        {
            get
            {
                return int.Parse(window.pollPeriodTextBox.Text);
            }
            set
            {
                window.SetTextBox(window.pollPeriodTextBox, value.ToString());
            }
        }

        /// <summary>
        /// Frequency poll worker thread
        /// </summary>
        private Thread freqPollThread;
        
        /// <summary>
        /// Thread lock for poll worker
        /// </summary>
        private object errorSigMonitorLock = new object();
        
        /// <summary>
        /// Manual reset event for freq poll worker
        /// </summary>
        ManualResetEvent stop = new ManualResetEvent(false);

        #endregion

        #region frequency measurement methods
        /// <summary>
        /// Method to only update the current measured frequency
        /// </summary>
        public void UpdateFrequencyCounter()
        {
            counter.Connect();
            double fr = counter.Frequency / 1e6;
            counter.Disconnect();
            CurrentFrequency = fr;
        }

        /// <summary>
        /// Method for updating the error signal plot
        /// </summary>
        public void UpdateErrorSigGraph()
        {
            double errorVal = FrequencySetPoint - CurrentFrequency;
            window.PlotYAppend(window.errorSigGraph, window.errorSigPlot,
                new double[] { errorVal });
        }

        /// <summary>
        /// Method to start measuring output frequency of VCO
        /// </summary>
        public void StartPoll()
        {
            lock (errorSigMonitorLock)
            {
                freqPollThread = new Thread(new ThreadStart(errorSigMonitorPollWorker));
                window.EnableControl(window.startPollButton, false);
                window.EnableControl(window.stopPollButton, true);
                freqPollThread.Start();
            }
        }

        /// <summary>
        /// Method to stop measuring output frequency of VCO
        /// </summary>
        public void StopPoll()
        {
            lock (errorSigMonitorLock) stop.Set();
        }

        /// <summary>
        /// Worker method for polling VCO frequency
        /// </summary>
        private void errorSigMonitorPollWorker()
        {
            while (!stop.WaitOne(0))
            {
                Thread.Sleep(PollPeriod);
                UpdateErrorSigGraph();
                if (window.propLockEnable.Checked || window.intLockEnable.Checked)
                {
                    double outputV = ComputeOutputVoltage();
                    UpdateVoltageOutput(outputV);

                }
            }
            stop.Reset();
            window.EnableControl(window.startPollButton, true);
            window.EnableControl(window.stopPollButton, false);
        }

        #endregion

        #region PI Control

        /// <summary>
        /// Method for calculating the feedback voltage using
        /// proportional gain Kp and integral gain Ki
        /// </summary>
        /// <returns>Feedback voltage to output</returns>
        public double ComputeOutputVoltage()
        {
            return 0.0;
        }

        /// <summary>
        /// Method to update Analog output of DAQ
        /// </summary>
        /// <param name="outputV">Required output voltage</param>
        public void UpdateVoltageOutput(double outputV)
        {
        }
        
        #endregion
    }
}
