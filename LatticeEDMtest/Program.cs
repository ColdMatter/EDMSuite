using NationalInstruments.DAQmx;
using System;
using System.Timers;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Lifetime;
using System.Threading;
using System.Windows.Forms;
using NationalInstruments;
using NationalInstruments.DAQmx;
//using NationalInstruments.VisaNS;
using System.Linq;
using System.IO.Ports;
using System.Windows.Forms.DataVisualization.Charting;
using DAQ.HAL;
using DAQ.Environment;
using System.Diagnostics;
using Data;

namespace LatticeHardwareControl
{
    public class Program : MarshalByRefObject
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        //Task cryoCoolertask;
        //private static double pressure = 0;

        //public Task pressuretask;

        private double initialSourceGaugeCorrectionFactor = 4.1;


        public override Object InitializeLifetimeService()
        {
            return null;
        }

        public void Start()
        {
            // make the control window
            form = new Form1();
            form.controller = this;
            Application.Run(form);
        }

        Form1 form;


        #region Pressure Monitors

        //// Create variable names for storing data////
        // For pressure monitors:
        private double lastSourcePressure;
        private double P_Down_last;

        private double SourceGaugeCorrectionFactor;
        private int pressureMovingAverageSampleLength = 10;


        //// Create queues to gather data
        // For pressure monitors:
        private Queue<double> pressureSamplesSource = new Queue<double>();
        private Queue<double> P_Samps_Down = new Queue<double>();

        private string sourceSeries = "Source";

        public void UpdatePressureMonitor()
        {
            // For Leybold Display 2 (Source & Downstream):
            LeyboldPTR225PressureGauge sourcePressureMonitor = new LeyboldPTR225PressureGauge("PressureGaugeSource", "pressureGaugeS");
            LeyboldPTR225PressureGauge downstreamPressureMonitor = new LeyboldPTR225PressureGauge("PressureGaugeDownstream", "Pressure_Downstream");

            //sample the pressure
            lock (LatticeHardwareControllerDAQCardLock) // Lock access to the DAQ card
            {
                lastSourcePressure = sourcePressureMonitor.Pressure;
                P_Down_last = downstreamPressureMonitor.Pressure;
            }

            //add samples to Queues for averaging
            pressureSamplesSource.Enqueue(lastSourcePressure);
            P_Samps_Down.Enqueue(P_Down_last);


            //drop samples when array is larger than the moving average sample length
            while (P_Samps_Down.Count > pressureMovingAverageSampleLength)
            {
                P_Samps_Down.Dequeue();
            }

            while (pressureSamplesSource.Count > pressureMovingAverageSampleLength)
            {
                pressureSamplesSource.Dequeue();
            }

            //average samples
            double avgPressureSource = pressureSamplesSource.Average();
            //SL Dec 07 2023
            if (avgPressureSource > 0)
            {
                string avgPressureSourceExpForm = avgPressureSource.ToString("E");
                form.SetTextBox(form.textBoxSourcePressure, avgPressureSourceExpForm.ToString());
                sourcePressureMonitor.DisposePressureTask();
            }
            else
            {
                //Display error message
                string avgPressureSourceExpForm = "Error Low";
                form.SetTextBox(form.textBoxSourcePressure, avgPressureSourceExpForm.ToString());
                sourcePressureMonitor.DisposePressureTask();
            }
            //SL end

            double P_avg_Down = P_Samps_Down.Average();
            string P_avg_Down_ExpForm = P_avg_Down.ToString("E");

            //update UI monitor text boxes
            //form.SetTextBox(form.textBoxSourcePressure, avgPressureSourceExpForm.ToString());
            //sourcePressureMonitor.DisposePressureTask();
            form.SetTextBox(form.textBoxDownstreamPressure, P_avg_Down_ExpForm.ToString());
            downstreamPressureMonitor.DisposePressureTask();
        }

        private Thread PTMonitorPollThread;
        private int PTMonitorPollPeriod = 1000;
        private bool PTMonitorFlag;
        private readonly object LatticeHardwareControllerDAQCardLock = new object(); // Object for locking access to the DAQ card used for this hardware controller

        private void PTMonitorPollWorker()
        {
            int count = 0;
            //int NumberofMovingAveragePoints = 2;
            //int MaxChartPoints = 100; // Maximum number of points that will be plotted on a given chart

            for (; ; )// for (; ; ) is an infinite loop, equivalent to while(true)
            {
                var watch = System.Diagnostics.Stopwatch.StartNew();  // Use stopwatch to track how long it takes to measure pressure and temperature. This is subtracted from the poll period so that the loop is executed at the proper frequency.
                ++count;
                // Measure pressures
                // Note that locks are used to prevent threads attempting to access the DAQ card
                UpdatePressureMonitor(); // Measure pressures and update the window textboxes with the current values


                if (PTMonitorFlag)
                {
                    PTMonitorFlag = false;
                    break;  //Is this closing the software?
                }


                // Subtract from the poll period the amount of time taken to perform the contents of this loop (so that the temperature and pressure are polled at the correct frequency)
                watch.Stop(); // Stop the stopwatch that was started at the start of the for loop
                int TimeElapsedMeasuringP = Convert.ToInt32(watch.ElapsedMilliseconds);
                int ThreadWaitPeriod = PTMonitorPollPeriod - TimeElapsedMeasuringP; // Subtract the time elapsed from the user defined poll period
                if (ThreadWaitPeriod < 0)// If the result of the above subtraction was negative, set the value to zero so that Thread.Sleep() doesn't throw an exception
                {
                    ThreadWaitPeriod = 0;
                }

                Thread.Sleep(ThreadWaitPeriod);
            }
        }

        internal void StartPTMonitorPoll()
        {
            // Setup pressure and temperature monitoring thread
            PTMonitorPollThread = new Thread(() =>
            {
                PTMonitorPollWorker();
            });
            PTMonitorPollThread.IsBackground = true; // When the application is closed, this thread will also immediately stop. This is lazy coding, but it works and shouldn't cause any problems. This means it is a background thread of the main (UI) thread, so it will end with the main thread.
                                                     //PTMonitorPollThread.Priority = ThreadPriority.AboveNormal;
                                                     // Setup pressure and temperature plotting thread


            pressureMovingAverageSampleLength = 10;
            pressureSamplesSource.Clear();
            PTMonitorFlag = false;
            PTMonitorPollThread.Start();

        }

        internal void StopPTMonitorPoll()
        {
            PTMonitorFlag = true;
        }

        //// Flow Controller codes
        /// <summary>
        /// /*
        /// </summary>

        internal void ConnectFlowControl()
        {
            // Setup gas flow controller
            // all of the options for a serial device
            // ---- can be sent through the constructor of the SerialPort class
            // ---- PortName = "COM9", Baud Rate = 19200, Parity = None,
            // ---- Data Bits = 8, Stop Bits = One, Handshake = None
            SerialPort _serialPort = new SerialPort("COM9", 19200, Parity.None, 8, StopBits.One);
            _serialPort.Handshake = Handshake.None;

            // "sp_DataReceived" is a custom method that I have created
            _serialPort.DataReceived += new SerialDataReceivedEventHandler(sp_DataReceived);

            // milliseconds _serialPort.ReadTimeout = 500;
            _serialPort.WriteTimeout = 500;

            // Opens serial port
            _serialPort.Open();

        }
        /*
        // delegate is used to write to a UI control from a non-UI thread
        private delegate void SetTextDeleg(string text);
        void sp_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            Thread.Sleep(500);
            string data = _serialPort.ReadLine();
            // Invokes the delegate on the UI thread, and sends the data that was received to the invoked method.
            // ---- The "si_DataReceived" method will be executed on the UI thread, which allows populating the textbox.
            this.BeginInvoke(new SetTextDeleg(si_DataReceived), new object[] { data });
        }

        private void si_DataReceived(string data) 
        { 
            textBox1.Text = data.Trim()
        }
        */
    }
}

        #endregion
}
