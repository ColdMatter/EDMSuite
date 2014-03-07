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
        Task voltageOutputTask; 
        
        /// <summary>
        /// (Not main) entry point for application 
        /// </summary>
        public void Start()
        {
            // Setup output task
            voltageOutputTask = CreateAnalogOutputTask("VCO_Out");
            outMax = ((AnalogOutputChannel)Environs.Hardware.AnalogOutputChannels["VCO_Out"]).RangeHigh;
            outMin = ((AnalogOutputChannel)Environs.Hardware.AnalogOutputChannels["VCO_Out"]).RangeLow;

            // Make GUI
            window = new ControlWindow();
            window.controller = this;
            Application.Run(window);
        }

        #region variables for lock
        
        /// <summary>
        /// Max range of VCO
        /// </summary>
        private double outMax;
        /// <summary>
        /// Min range of VCO
        /// </summary>
        private double outMin;

        /// <summary>
        /// Set point for generator frequency to be compared to
        /// </summary>
        public double FrequencySetPoint{ 
            get
            {
                return (double)window.setPointNumericUpDown.Value;
            } 
            set
            {
                window.SetNumericBox(window.setPointNumericUpDown, (decimal)value);
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
        /// Output Voltage
        /// </summary>
        public double OutputVoltage
        {
            get
            {
                return (double)window.outputVoltageNumericUpDown.Value;
            }
            set
            {
                window.SetNumericBox(window.outputVoltageNumericUpDown, (decimal)value);
            }
        }

        /// <summary>
        /// Proportional gain
        /// </summary>
        public double Kp
        {
            get
            {
                return (double)window.propGainNumeric.Value;
            }
            set
            {
                window.SetNumericBox(window.propGainNumeric, (decimal)value);
            }
        }

        /// <summary>
        /// Integral gain
        /// </summary>
        public double Ki
        {
            get
            {
                return (double)window.intGainNumeric.Value;
            }
            set
            {
                window.SetNumericBox(window.intGainNumeric, (decimal)value);
            }
        }

        /// <summary>
        /// Current value of proportional term in lock
        /// </summary>
        private double propTerm { get; set; }
        /// <summary>
        /// Current value of integral term in lock
        /// </summary>
        private double intTerm { get; set; }

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
        public void UpdateErrorSigGraph(double errorVal)
        {
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
                double errorVal = FrequencySetPoint - CurrentFrequency;
                UpdateErrorSigGraph(errorVal);
                if (window.propLockEnable.Checked || window.intLockEnable.Checked)
                {
                    ComputeOutputVoltage(errorVal);
                }
                SetAnalogOutput(voltageOutputTask, OutputVoltage);
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
        public void ComputeOutputVoltage(double errorVal)
        {
            // Optionally change sign of gain
            double sign;
            if (window.reverseCheckBox.Checked)
            {
                sign = -1.0;
            }
            else
            {
                sign = +1.0;
            }

            // Calculate the porpotinal term if 
            // proportional lock is enabled
            if (window.propLockEnable.Checked)
            {
                propTerm = sign * Kp * errorVal;
            }
            else
            {
                propTerm = 0.0;
            }

            // Calculate the integral term if 
            // integral lock is enabled
            if (window.intLockEnable.Checked)
            {
                intTerm += sign * Ki * errorVal;
            }
            else
            {
                intTerm = 0.0;
            }
            // This stops intTerm from running away uncontrollably
            if (intTerm < outMin) intTerm = outMin;
            if (intTerm > outMax) intTerm = outMax;
            
            double outputV = propTerm + intTerm;

            // Limit range of feedback voltage
            if (outputV > outMax) outputV = outMax;
            if (outputV < outMin) outputV = outMin;

            OutputVoltage = outputV; 
        }

        /// <summary>
        /// Method to create a analog output task
        /// </summary>
        /// <param name="channel">Channel name as defined in Hardware class</param>
        /// <returns>The output task</returns>
        private Task CreateAnalogOutputTask(string channel)
        {
            Task task = new Task(channel);
            AnalogOutputChannel c = ((AnalogOutputChannel)Environs.Hardware.AnalogOutputChannels[channel]);
            c.AddToTask(
                task,
                c.RangeLow,
                c.RangeHigh
                );
            task.Control(TaskAction.Verify);
            return task;
        }

        /// <summary>
        /// Method for outputing a voltage on the DAQ
        /// </summary>
        /// <param name="task">NI-DAQ output task</param>
        /// <param name="voltage">Voltage to output. 
        /// Note must be between the limits outMax and outMin.
        /// The range is limited elsewhere in the code</param>
        private void SetAnalogOutput(Task task, double voltage)
        {
            AnalogSingleChannelWriter writer = new AnalogSingleChannelWriter(task.Stream);
            writer.WriteSingleSample(true, voltage);
            task.Control(TaskAction.Unreserve);
        }
        
        #endregion
    }
}
