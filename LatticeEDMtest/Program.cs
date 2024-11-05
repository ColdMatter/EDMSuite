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
using DAQ;
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
        //Cryo Control






        #region Setup
        //Alicat Flow Control
        AlicatFlowController Alicat = new AlicatFlowController("ASRL15::INSTR");
        AnapicoSynth anapico = (AnapicoSynth)Environs.Hardware.Instruments["anapicoSYN420"];
        double AnapicoCWFrequencyCH1 = 14500000000; // in Hz
        double AnapicoCWFrequencyCH2 = 14500000000; // in Hz
        double AnapicoPowerCH1 = 20.0;                // in dBm
        double AnapicoPowerCH2 = 20.0;                // in dBm
        double AnapicoFMDeviationCH1 = 10000000;    // in Hz
        double AnapicoFMDeviationCH2 = 10000000;    // in Hz
        string AnapicoCurrentList;
        #endregion

        #region Alicat
        public void AlicatFlowSet(string ControllerAddress, string flowrate)
        {
            lock (Alicat)
            { 
                query = Alicat.SetSetpoint(ControllerAddress, flowrate);
               // 

            }
        }
        #endregion

        #region Pressure Monitors

        //// Create variable names for storing data////
        ///Right we create new pressure gauges and relock and connect eveyr update, innecesary and hsoul dbe changed///
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
            
            InfluxDBDataLogger data = InfluxDBDataLogger.Measurement("Pressure").Tag("name", "Source Pressure");
            data = data.Field("Lattice Machine", avgPressureSource);
            data = data.Timestamp(DateTime.UtcNow);
            data.Write("https://ccmmonitoring.ph.ic.ac.uk:8086", "Lattice EDM", "CentreForColdMatter");

            //SL Dec 07 2023
            if (avgPressureSource > 0)
            {
               
                string avgPressureSourceExpForm = avgPressureSource.ToString("E");
                form.SetTextBox(form.textBoxSourcePressure, avgPressureSourceExpForm.ToString());

                //sourcePressureMonitor.DisposePressureTask();
            }
            else
            {
                //Display error message
                string avgPressureSourceExpForm = "Error Low";
                form.SetTextBox(form.textBoxSourcePressure, avgPressureSourceExpForm.ToString());
                //sourcePressureMonitor.DisposePressureTask();
            }
            //SL end

            double P_avg_Down = P_Samps_Down.Average();

            InfluxDBDataLogger dataD = InfluxDBDataLogger.Measurement("Pressure").Tag("name", "Downstream Pressure");
            dataD = dataD.Field("Lattice Machine", P_avg_Down);
            dataD = dataD.Timestamp(DateTime.UtcNow);
            dataD.Write("https://ccmmonitoring.ph.ic.ac.uk:8086", "Lattice EDM", "CentreForColdMatter");

            string P_avg_Down_ExpForm = P_avg_Down.ToString("E");

            //update UI monitor text boxes
            //form.SetTextBox(form.textBoxSourcePressure, avgPressureSourceExpForm.ToString());
            //sourcePressureMonitor.DisposePressureTask();
            form.SetTextBox(form.textBoxDownstreamPressure, P_avg_Down_ExpForm.ToString());
            //downstreamPressureMonitor.DisposePressureTask();

        }

        private Thread PTMonitorPollThread;
        private int PTMonitorPollPeriod = 1000;
        private bool PTMonitorFlag;
        private string query;
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
                    break; 
                }


                // Subtract from the poll period the amount of time taken to perform the contents of this loop (so that the temperature and pressure are polled at the correct frequency)
                watch.Stop(); // Stop the stopwatch that was started at the start of the for loop
                int TimeElapsedMeasuringP = Convert.ToInt32(watch.ElapsedMilliseconds);
                int ThreadWaitPeriod = PTMonitorPollPeriod - TimeElapsedMeasuringP; // Subtract the time elapsed from the user defined poll period
                if (ThreadWaitPeriod < 0)// If the result of the above subtraction was negative, set the value to zero so that Thread.Sleep() doesn't throw an exception
                {
                    ThreadWaitPeriod = 0;
                }

                System.GC.Collect();
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
            // ---- PortName = "COM14", Baud Rate = 19200, Parity = None,
            // ---- Data Bits = 8, Stop Bits = One, Handshake = None
            SerialPort _serialPort = new SerialPort("COM14", 19200, Parity.None, 8, StopBits.One);
            _serialPort.Handshake = Handshake.None;

            // "sp_DataReceived" is a custom method that I have created
            //_serialPort.DataReceived += new SerialDataReceivedEventHandler(sp_DataReceived);

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
        #endregion

        #region Anapico

        public void EnableAnapico(bool enable)
        {
            // UpdateAnapicoRAMList(MwSwitchState);
            // UpdateAnapicoSYN420RAMList(MwSwitchState);
            anapico.Connect();
            if (enable)
            {
                anapico.CWFrequencyCH1 = AnapicoCWFrequencyCH1;
                anapico.CWFrequencyCH2 = AnapicoCWFrequencyCH2;
                anapico.Enabled = true;
            }
            else
            {
                anapico.Enabled = false;
            }
            anapico.Disconnect();
        }

        public void EnablePulseMode(bool enable)
        {
            anapico.Connect();
            if (enable)
            {
                anapico.EnablePulseMode = true;
            }
            else
            {
                anapico.EnablePulseMode = false;
            }
            anapico.Disconnect();
        }

        public void EnableFMCH1(bool enable)
        {
            anapico.Connect();
            if (enable)
            {
                anapico.FrequencyModulationCH1Enabled = true;
            }
            else
            {
                anapico.FrequencyModulationCH1Enabled = false;
            }
            anapico.Disconnect();
        }

        public void EnableFMCH2(bool enable)
        {
            anapico.Connect();
            if (enable)
            {
                anapico.FrequencyModulationCH2Enabled = true;
            }
            else
            {
                anapico.FrequencyModulationCH2Enabled = false;
            }
            anapico.Disconnect();
        }

        public void UpdateAnapicoPowerCH1()
        {
            anapico.Connect();
            anapico.PowerCH1 = AnapicoPowerCH1;
            anapico.Disconnect();
        }

        public void UpdateAnapicoPowerCH2()
        {
            anapico.Connect();
            anapico.PowerCH2 = AnapicoPowerCH2;
            anapico.Disconnect();
        }

        public void UpdateAnapicoCWCH1()
        {
            anapico.Connect();
            anapico.CWFrequencyCH1 = AnapicoCWFrequencyCH1;
            anapico.Disconnect();
        }

        public void UpdateAnapicoCWCH2()
        {
            anapico.Connect();
            anapico.CWFrequencyCH2 = AnapicoCWFrequencyCH2;
            anapico.Disconnect();
        }

        public void UpdateAnapicoFMDeviationCH1()
        {
            anapico.Connect();
            anapico.FMDeviationCH1 = AnapicoFMDeviationCH1;
            anapico.Disconnect();
        }

        public void UpdateAnapicoFMDeviationCH2()
        {
            anapico.Connect();
            anapico.FMDeviationCH2 = AnapicoFMDeviationCH2;
            anapico.Disconnect();
        }

        public void SetAnapicoCWFrequencyCH1(double freq) { AnapicoCWFrequencyCH1 = freq; }
        public void SetAnapicoCWFrequencyCH2(double freq) { AnapicoCWFrequencyCH2 = freq; }
        public void SetAnapicoPowerCH1(double power) { AnapicoPowerCH1 = power; }
        public void SetAnapicoPowerCH2(double power) { AnapicoPowerCH2 = power; }
        public void SetAnapicoFMDeviationCH1(double dev) { AnapicoFMDeviationCH1 = dev; }
        public void SetAnapicoFMDeviationCH2(double dev) { AnapicoFMDeviationCH2 = dev; }

        // When writing a list to RAM, the data has to be transferred according to the IEEE 488.2 Definite Length Block Response Data format.
        // This is #<number of digits that follows this><number of data bytes><data>
        // <data> has to be the form <frequency in Hz>;<power in dBm>;<dwell on time>;<dwell off time>\r\n<next frequency in Hz>...
        //public void UpdateAnapicoRAMList(bool trueState)
        //{
        //    try
        //    {
        //        bool currentMwListSweepStatus = MwListSweepEnabled;
        //        MwListSweepEnabled = false;
        //        anapico.Connect();
        //        if (trueState)
        //        {
        //            string list = AnapicoCWFrequencyCH1.ToString() + ";15;" + AnapicoPumpMWDwellOnTime.ToString("E") + ";" + AnapicoPumpMWDwellOffTime.ToString("E") + "\r\n"
        //                + AnapicoFrequency0.ToString() + ";15;" + AnapicoBottomProbeMWDwellOnTime.ToString("E") + ";" + AnapicoBottomProbeMWDwellOffTime.ToString("E") + "\r\n"
        //                + AnapicoFrequency1.ToString() + ";15;" + AnapicoTopProbeMWDwellOnTime.ToString("E") + ";" + AnapicoTopProbeMWDwellOffTime.ToString("E") + "\r\n";

        //            int numBytes = list.Length;

        //            int numDigits = numBytes.ToString().Length;

        //            string sendList = "#" + numDigits.ToString() + numBytes.ToString() + list;

        //            AnapicoBottomProbeMWf0Indicator = true;
        //            AnapicoBottomProbeMWf1Indicator = false;
        //            AnapicoTopProbeMWf0Indicator = false;
        //            AnapicoTopProbeMWf1Indicator = true;

        //            anapico.WriteList(sendList);
        //        }
        //        else
        //        {
        //            string list = AnapicoCWFrequencyCH1.ToString() + ";15;" + AnapicoPumpMWDwellOnTime.ToString() + ";" + AnapicoPumpMWDwellOffTime.ToString() + "\r\n"
        //                + AnapicoFrequency1.ToString() + ";15;" + AnapicoBottomProbeMWDwellOnTime.ToString() + ";" + AnapicoBottomProbeMWDwellOffTime.ToString() + "\r\n"
        //                + AnapicoFrequency0.ToString() + ";15;" + AnapicoTopProbeMWDwellOnTime.ToString() + ";" + AnapicoTopProbeMWDwellOffTime.ToString() + "\r\n";

        //            int numBytes = list.Length;

        //            int numDigits = numBytes.ToString().Length;

        //            string sendList = "#" + numDigits.ToString() + numBytes.ToString() + list;

        //            AnapicoBottomProbeMWf0Indicator = false;
        //            AnapicoBottomProbeMWf1Indicator = true;
        //            AnapicoTopProbeMWf0Indicator = true;
        //            AnapicoTopProbeMWf1Indicator = false;

        //            anapico.WriteList(sendList);
        //        }
        //        anapico.Disconnect();
        //        MwListSweepEnabled = currentMwListSweepStatus;
        //    }
        //    catch
        //    {
        //        //If the command fails, try waiting a second and turnning the synth on and off. This also re-loads the list into the memmory
        //        Thread.Sleep((int)(1000));
        //        EnableAnapico(false);
        //        EnableAnapico(true);
        //    }
        //}

        /*public void UpdateAnapicoSYN420RAMList(bool trueState)
        {
            //The anapico has a fixed output power of +23dBm, so writing power values to the RAM does nothing. Channel 1 is sent to the pump region and bottom probe, and Ch2 is sent to the top probe.
            try
            {
                string[] chList = new string[2];

                bool currentMwListSweepStatus = MwListSweepEnabled;
                MwListSweepEnabled = false;
                anapico.Connect();
                if (trueState)
                {
                    string ch1list = AnapicoCWFrequency.ToString() + ";23;" + AnapicoPumpMWDwellOnTime.ToString("E") + ";" + AnapicoPumpMWDwellOffTime.ToString("E") + "\r\n"
                        + AnapicoFrequency0.ToString() + ";23;" + AnapicoBottomProbeMWDwellOnTime.ToString("E") + ";" + AnapicoBottomProbeMWDwellOffTime.ToString("E") + "\r\n";

                    string ch2list = AnapicoFrequency1.ToString() + ";23;" + (AnapicoPumpMWDwellOnTime + AnapicoPumpMWDwellOffTime + AnapicoBottomProbeMWDwellOnTime + AnapicoBottomProbeMWDwellOffTime + AnapicoTopProbeMWDwellOnTime).ToString() + ";" + AnapicoTopProbeMWDwellOffTime.ToString() + "\r\n";

                    chList[0] = ch2list;
                    chList[1] = ch1list;
                    
                }
                else
                {
                    string ch1list = AnapicoCWFrequency.ToString() + ";23;" + AnapicoPumpMWDwellOnTime.ToString() + ";" + AnapicoPumpMWDwellOffTime.ToString() + "\r\n"
                        + AnapicoFrequency1.ToString() + ";23;" + AnapicoBottomProbeMWDwellOnTime.ToString() + ";" + AnapicoBottomProbeMWDwellOffTime.ToString() + "\r\n";
                    string ch2list = AnapicoFrequency0.ToString() + ";23;" + (AnapicoPumpMWDwellOnTime + AnapicoPumpMWDwellOffTime + AnapicoBottomProbeMWDwellOnTime + AnapicoBottomProbeMWDwellOffTime + AnapicoTopProbeMWDwellOnTime).ToString() +";" +  AnapicoTopProbeMWDwellOffTime.ToString() + "\r\n";

                    chList[0] = ch2list;
                    chList[1] = ch1list;

                }
                anapico.WriteList(chList);

                anapico.Disconnect();
                MwListSweepEnabled = currentMwListSweepStatus;
            }
            catch
            {
                //If the command fails, try waiting a second and turnning the synth on and off. This also re-loads the list into the memmory
                Thread.Sleep((int)(1000));
                EnableAnapico(false);
                EnableAnapico(true);
            }
        }*/

        public void EnableAnapicoListSweep(bool enable)
        {
            anapico.Connect();
            if (enable)
            {
                anapico.ListSweepEnabled = true;
            }
            else
            {
                anapico.ListSweepEnabled = false;
            }
            anapico.Disconnect();
        }



        // When reading a list to RAM, the data is transferred according to the IEEE 488.2 Definite Length Block Response Data format.
        // This is #<number of digits that follows this><number of data bytes><data>
        // <data> is in the form <frequency in Hz>;<power in dBm>;<dwell on time>;<dwell off time>\r\n<next frequency in Hz>...
        public void GetAnapicoCurrentList()
        {
            anapico.Connect();

            string list = anapico.ReadList();
            int numDigits = Convert.ToInt32(list[1].ToString());
            string subList = list.Substring(numDigits + 2);

            char[] delimiters = { ';', '\r', '\n' };
            string[] splitList = subList.Split(delimiters);

            string displayList = string.Empty;

            for (int i = 0; i < splitList.Length / 4; ++i)
            {
                int j = 4 * i;
                string num = Convert.ToString(i + 1);
                displayList += "Frequency " + num + " (Hz): " + splitList[j] + "\r\n"
                    + "Dwell on time " + num + " (s): " + splitList[j + 2] + "\r\n"
                    + "Dwell off time " + num + " (s): " + splitList[j + 3] + "\r\n";
            }

            AnapicoCurrentList = displayList;

            anapico.Disconnect();
        }

        public void GetAnapicoCWFreqs()
        {
            anapico.Connect();
            string displayList = "CH1 CW Freq: " + Convert.ToString(anapico.CWFrequencyCH1) + " Hz\r\n" + "CH2 CW Freq: " + Convert.ToString(anapico.CWFrequencyCH2) + " Hz";
            AnapicoCurrentList = displayList;
            anapico.Disconnect();
        }
        #endregion
    }

}