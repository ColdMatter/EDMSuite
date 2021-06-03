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

namespace UEDMHardwareControl
{
    public class UEDMController : MarshalByRefObject
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        /// 

        #region Constants

        // Gauges
        private double initialSourceGaugeCorrectionFactor = 4.1;
        private double initialBeamlineGaugeCorrectionFactor = 4.35;
        private double initialDetectionGaugeCorrectionFactor = 1;

        //Current Leakage Monitor calibration 
        //Convention for monitor to plate mapping:
        //west -> monitor1
        //east -> monitor2
        private static double westVolt2FreqSlope = 2000;
        private static double eastVolt2FreqSlope = 2000;
        private static double westFreq2AmpSlope = 1;
        private static double eastFreq2AmpSlope = 1;
        private static double westOffset = 0;
        private static double eastOffset = 0;
        private static double currentMonitorMeasurementTime = 0.01;

        #endregion

        #region Setup

        // hardware
        private static string[] Names = { "Cell Temperature Monitor", "S1 Temperature Monitor", "S2 Temperature Monitor", "SF6 Temperature Monitor" };
        private static string[] ChannelNames = { "cellTemperatureMonitor", "S1TemperatureMonitor", "S2TemperatureMonitor", "SF6TemperatureMonitor" };

        private static string[] AINames = { "AI11", "AI12", "AI13", "AI14", "AI15" };
        private static string[] AIChannelNames = { "AI11", "AI12", "AI13", "AI14", "AI15" };
        
        // Temperature sensors
        LakeShore336TemperatureController tempController = (LakeShore336TemperatureController)Environs.Hardware.Instruments["tempController"];
        SiliconDiodeTemperatureMonitors tempMonitors = new SiliconDiodeTemperatureMonitors(Names, ChannelNames);

        // Pressure gauges
        // The following gauges have the same voltage-mbar conversion is used for the AgilentFRG720Gauge.
        AgilentFRG720Gauge beamlinePressureMonitor = new AgilentFRG720Gauge("Pressure gauge beamline", "pressureGaugeBeamline");
        AgilentFRG720Gauge sourcePressureMonitor = new AgilentFRG720Gauge("Pressure gauge source", "pressureGaugeSource"); 
        AgilentFRG720Gauge detectionPressureMonitor = new AgilentFRG720Gauge("Pressure gauge detection", "pressureGaugeDetection");

        // Misc analogue inputs
        UEDMHardwareControllerAIs hardwareControllerAIs = new UEDMHardwareControllerAIs(AINames, AIChannelNames);

        // Flow controllers
        FlowControllerMKSPR4000B neonFlowController = (FlowControllerMKSPR4000B)Environs.Hardware.Instruments["neonFlowController"];

        Hashtable digitalTasks = new Hashtable();
        Hashtable digitalInputTasks = new Hashtable();
        
        //Leakage monitors
        //LeakageMonitor westLeakageMonitor = new LeakageMonitor("westLeakage", westVolt2FreqSlope, westFreq2AmpSlope, westOffset);
        //LeakageMonitor eastLeakageMonitor = new LeakageMonitor("eastLeakage", eastVolt2FreqSlope, eastFreq2AmpSlope, eastOffset);
        LeakageMonitor westLeakageMonitor =  new LeakageMonitor((CounterChannel)Environs.Hardware.CounterChannels["westLeakage"], westVolt2FreqSlope, westOffset, currentMonitorMeasurementTime);
        LeakageMonitor eastLeakageMonitor =  new LeakageMonitor((CounterChannel)Environs.Hardware.CounterChannels["eastLeakage"], eastVolt2FreqSlope, eastOffset, currentMonitorMeasurementTime);
        
        Task cPlusOutputTask;
        Task cMinusOutputTask;
        Task cPlusMonitorInputTask;
        Task cMinusMonitorInputTask;


        //Task cryoTriggerDigitalOutputTask;

        // Heater digital outputs
        Task heatersS2TriggerDigitalOutputTask;
        Task heatersS1TriggerDigitalOutputTask;

        private void CreateDigitalTask(String name)
        {
            if (!Environs.Debug)
            {
                Task digitalTask = new Task(name);
                ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels[name]).AddToTask(digitalTask);
                digitalTask.Control(TaskAction.Verify);
                digitalTasks.Add(name, digitalTask);
            }
        }

        private void SetDigitalLine(string name, bool value)
        {
            if (!Environs.Debug)
            {
                Task digitalTask = ((Task)digitalTasks[name]);
                DigitalSingleChannelWriter writer = new DigitalSingleChannelWriter(digitalTask.Stream);
                writer.WriteSingleSampleSingleLine(true, value);
                digitalTask.Control(TaskAction.Unreserve);
            }
        }


        // without this method, any remote connections to this object will time out after
        // five minutes of inactivity.
        // It just overrides the lifetime lease system completely.
        public override Object InitializeLifetimeService()
        {
            return null;
        }

        # endregion

        ControlWindow window;

        public void Start()
        {
            // Create digital output tasks
            //CreateDigitalTask("cryoTriggerDigitalOutputTask");
            CreateDigitalTask("heatersS2TriggerDigitalOutputTask");
            CreateDigitalTask("heatersS1TriggerDigitalOutputTask");

            CreateDigitalTask("Port00");
            CreateDigitalTask("Port01");
            CreateDigitalTask("Port02");
            CreateDigitalTask("Port03");
            Console.WriteLine("here");
            // digitial input tasks

            // initialise the current leakage monitors
            westLeakageMonitor.Initialize();
            eastLeakageMonitor.Initialize();

            // analog outputs
            //bBoxAnalogOutputTask = CreateAnalogOutputTask("bScan");

            // analog inputs
            //probeMonitorInputTask = CreateAnalogInputTask("probePD", 0, 5);
            
            cPlusMonitorInputTask = CreateAnalogInputTask("cPlusMonitor");
            cMinusMonitorInputTask = CreateAnalogInputTask("cMinusMonitor");



            // make the control window
            window = new ControlWindow();
            window.controller = this;

            Application.Run(window);
        }

        // this method runs immediately after the GUI sets up
        internal void WindowLoaded()
        {
            // Set initial datetime picker values for the user interface
            DateTime now = DateTime.Now;
            DateTime InitialDateTime = new DateTime(now.Year, now.Month, now.AddDays(1).Day, 4, 0, 0);
            window.SetDateTimePickerValue(window.dateTimePickerHeatersTurnOff, InitialDateTime);
            window.SetDateTimePickerValue(window.dateTimePickerRefreshModeTurnCryoOn, InitialDateTime);
            window.SetDateTimePickerValue(window.dateTimePickerRefreshModeTurnHeatersOff, InitialDateTime);
            window.SetDateTimePickerValue(window.dateTimePickerWarmUpModeTurnHeatersOff, InitialDateTime);
            window.SetDateTimePickerValue(window.dateTimePickerCoolDownModeTurnHeatersOff, InitialDateTime);
            window.SetDateTimePickerValue(window.dateTimePickerCoolDownModeTurnCryoOn, InitialDateTime);
            // Set flags
            refreshModeHeaterTurnOffDateTimeFlag = false;
            refreshModeCryoTurnOnDateTimeFlag = false;
            warmupModeHeaterTurnOffDateTimeFlag = false;
            CoolDownModeHeaterTurnOffDateTimeFlag = false;
            CoolDownModeCryoTurnOnDateTimeFlag = false;
            // Check that the LakeShore relay is set correctly 
            InitializeCryoControl();

            // Set the leakage current monitor textboxes to the default values.
            window.SetTextBox(window.southOffsetIMonitorTextBox, eastOffset.ToString());
            window.SetTextBox(window.northOffsetIMonitorTextBox, westOffset.ToString());
            window.SetTextBox(window.IMonitorMeasurementLengthTextBox, currentMonitorMeasurementTime.ToString());

            // Set initial parameters on PT monitoring tab
            int initialPTPollPeriod = 1000;
            UpdatePTMonitorPollPeriod(initialPTPollPeriod);
            UpdateGaugesCorrectionFactors(initialSourceGaugeCorrectionFactor, initialBeamlineGaugeCorrectionFactor, initialDetectionGaugeCorrectionFactor);
    }
        
        public void WindowClosing()
        {
            // Request that the PT monitoring thread stop
            StopPTMonitorPoll();
        }

        private Task CreateAnalogInputTask(string channel)
        {
            Task task = new Task("EDMHCIn" + channel);
            ((AnalogInputChannel)Environs.Hardware.AnalogInputChannels[channel]).AddToTask(
                task,
                0,
                10
            );
            task.Control(TaskAction.Verify);
            return task;
        }

        private Task CreateAnalogInputTask(string channel, double lowRange, double highRange)
        {
            Task task = new Task("EDMHCIn" + channel);
            ((AnalogInputChannel)Environs.Hardware.AnalogInputChannels[channel]).AddToTask(
                task,
                lowRange,
                highRange
            );
            task.Control(TaskAction.Verify);
            return task;
        }
        private double ReadAnalogInput(Task task)
        {
            AnalogSingleChannelReader reader = new AnalogSingleChannelReader(task.Stream);


            double val = reader.ReadSingleSample();
            task.Control(TaskAction.Unreserve);
            return val;
        }

        private double ReadAnalogInput(Task task, double sampleRate, int numOfSamples)
        {
            //Configure the timing parameters of the task
            task.Timing.ConfigureSampleClock("", sampleRate,
                SampleClockActiveEdge.Rising, SampleQuantityMode.FiniteSamples, numOfSamples);

            //Read in multiple samples
            AnalogSingleChannelReader reader = new AnalogSingleChannelReader(task.Stream);
            double[] valArray = reader.ReadMultiSample(numOfSamples);
            task.Control(TaskAction.Unreserve);

            //Calculate the average of the samples
            double sum = 0;
            for (int j = 0; j < numOfSamples; j++)
            {
                sum = sum + valArray[j];
            }
            double val = sum / numOfSamples;
            return val;
        }

        #region Windows API
        // The following methods can block windows shutdown (although the user will be able to force shutdown still)
        [DllImport("user32.dll")]
        public extern static bool ShutdownBlockReasonCreate(IntPtr hWnd, [MarshalAs(UnmanagedType.LPWStr)] string pwszReason);
        [DllImport("user32.dll")]
        public extern static bool ShutdownBlockReasonDestroy(IntPtr hWnd);

        private bool isBlocked = false;

        /// <summary>
        /// This method will stop Windows from shutting down - providing a reason to the user.
        /// </summary>
        /// <param name="hWnd"></param>
        /// <param name="strMessage"></param>
        private void StopShutdown(IntPtr hWnd, string strMessage)
        {
            try
            {
                //strMessage == Message to display in shutdown/logoff box
                if (ShutdownBlockReasonCreate(hWnd, strMessage))
                {
                    isBlocked = true;
                }
                else
                {
                    MessageBox.Show("StopShutdown Failed", "Stopping Windows shutdown", MessageBoxButtons.OKCancel);
                }
            }
            catch (Exception ext)
            {
                MessageBox.Show("++ StopShutdown Error:    " + ext.Message + " " + ext.StackTrace);
            }
        }

        /// <summary>
        /// This will reset the API shutdown block
        /// </summary>
        /// <param name="hWnd"></param>
        private void ResetShutdown(IntPtr hWnd)
        {
            try
            {

                if (ShutdownBlockReasonDestroy(hWnd))
                {
                    isBlocked = false;
                }
                else
                {
                    MessageBox.Show("ResetShutdown Failed","Resetting Windows API shutdown block",MessageBoxButtons.OKCancel);
                }
            }
            catch (Exception ext)
            {
                MessageBox.Show("++ ResetShutdown Error:    " + ext.Message + " " + ext.StackTrace);
            }
        }

        #endregion

        #region Menu controls

        public void Exit()
        {
            Application.Exit();
        }
        /// <summary>
        /// Function to save image of the current state of a plot in the application. 
        /// </summary>
        /// <param name="mychart"></param>
        public void SavePlotImage(Chart mychart)
        {
            Stream myStream;
            SaveFileDialog ff = new SaveFileDialog();

            ff.Filter = "jpg files (*.jpg)|*.jpg|All files (*.*)|*.*";
            ff.FilterIndex = 1;
            ff.RestoreDirectory = true;

            if (ff.ShowDialog() == DialogResult.OK)
            {
                if ((myStream = ff.OpenFile()) != null)
                {
                    using (myStream)
                    {
                        mychart.SaveImage(myStream, System.Drawing.Imaging.ImageFormat.Jpeg);
                    }
                }
            }
        }

        List<Image> ChartsToImages(List<Chart> charts)
        {
            var imageList = new List<Image>();
            foreach (var c in charts)
            {
                using (var ms = new MemoryStream())
                {
                    c.SaveImage(ms, ChartImageFormat.Png);
                    var bmp = System.Drawing.Bitmap.FromStream(ms);
                    imageList.Add(bmp);
                }
            }
            return imageList;
        }

        private static Image MergeImages(List<Image> imageList)
        {
            var finalSize = new Size();
            foreach (var image in imageList)
            {
                if (image.Width > finalSize.Width)
                {
                    finalSize.Width = image.Width;
                }
                finalSize.Height += image.Height;
            }
            var outputImage = new Bitmap(finalSize.Width, finalSize.Height);
            using (var gfx = Graphics.FromImage(outputImage))
            {
                var y = 0;
                foreach (var image in imageList)
                {
                    gfx.DrawImage(image, 0, y);
                    y += image.Height;
                }
            }
            return outputImage;
        }

        public void SaveMultipleChartImages(List<Chart> mycharts)
        {
            var imageList = ChartsToImages(mycharts);
            var finalImage = MergeImages(imageList);

            Stream myStream;
            SaveFileDialog ff = new SaveFileDialog();

            ff.Filter = "png files (*.png)|*.png|All files (*.*)|*.*";
            ff.FilterIndex = 1;
            ff.RestoreDirectory = true;

            if (ff.ShowDialog() == DialogResult.OK)
            {
                if ((myStream = ff.OpenFile()) != null)
                {
                    using (myStream)
                    {
                        finalImage.Save(myStream, System.Drawing.Imaging.ImageFormat.Png);
                    }
                }
            }
            
        }


        public string[] csvData;
        public void SavePlotDataToCSV(string csvContent)
        {
            // Displays a SaveFileDialog so the user can save the data
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.Filter = "CSV|*.csv";
            saveFileDialog1.Title = "Save a CSV File";
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                // If the file name is not an empty string open it for saving.
                if (saveFileDialog1.FileName != "")
                {
                    // Using stream writer class the chart points are exported. Create an instance of the stream writer class.
                    System.IO.StreamWriter file = new System.IO.StreamWriter(saveFileDialog1.FileName);
                    
                    // Write the datapoints into the file.
                    file.WriteLine(csvContent);

                    file.Close();
                }
            }

            //foreach (Series series in myChart.Series)
            //{
            //    string seriesName = series.Name;
            //    int pointCount = series.Points.Count;
            //    csvContent += "Unix Time Stamp (s)" + "," + "Full date/time" + "," + seriesName + " (" + Units + ")" + "\r\n"; // Header lines
            //
            //    for (int p = 0; p < pointCount; p++)
            //    {
            //        var points = series.Points[p];
            //
            //        string yValuesCSV = String.Empty;
            //
            //        DateTime xDateTime = DateTime.FromOADate(points.XValue); // points.XValue is assumed to be a double. It must be converted to a datetime so that we can choose the datetime string format easily.
            //        Int32 unixTimestamp = (Int32)(xDateTime.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
            //        string csvLine = unixTimestamp + "," + xDateTime.ToString("dd/MM/yyyy hh:mm:ss") + "," + points.YValues[0];
            //
            //        csvContent += csvLine + "\r\n";
            //
            //    }
            //    csvContent += "\r\n";
            //}
        }

        #endregion

        #region Cryo Control

        private int RelayNumber = 1;
        // Currently the LakeShore/cryo relay is set up such that the relay is in the normally open state
        private string Off = "0"; // I.e. connected in normally open state (NO)
        private string On = "1"; // Swap these if the wiring is changed to normally closed (NC)

        public void EnableCryoDigitalControl(bool enable) // Temporary function - used to control a digital output which connects to the cryo (true = on, false = off)
        {
            SetDigitalLine("cryoTriggerDigitalOutputTask", enable);
        }

        public void InitializeCryoControl()
        {
            if (!Environs.Debug)
            {
                string status;
                lock (LakeShore336Lock)
                {
                    status = tempController.QueryRelayStatus(RelayNumber); // Query the status of the relay.
                }
                if (status == On)
                {
                    string message = "Cryo control warning: The LakeShore 336 temperature controller Relay-" + RelayNumber + " is set such that the cryo will turn be on. \n\nClick OK to continue opening application.";
                    string caption = "Cryo Control Warning";
                    MessageBox.Show(message, caption, MessageBoxButtons.OK);
                    window.SetTextBox(window.tbCryoState, "ON");
                }
                else
                {
                    if (status == Off) window.SetTextBox(window.tbCryoState, "OFF");
                    else
                    {
                        window.SetTextBox(window.tbCryoState, "UNKNOWN");
                        string message = "Cryo control exception: When querying the state of LakeShore 336 temperature controller Relay-" + RelayNumber + ", an unexpected response was provided. The state of the relay is UNKNOWN. \n\nClick OK to continue.";
                        string caption = "Cryo Control Exception";
                        MessageBox.Show(message, caption, MessageBoxButtons.OK);
                    }
                }
            }
        }

        public string PollCryoStatus()
        {
            string status;
            lock (LakeShore336Lock)
            {
                status = tempController.QueryRelayStatus(RelayNumber); // Query the status of the relay.
            }
            if (status == Off | status == On)
            {
                if (status == Off) window.SetTextBox(window.tbCryoState, "OFF");
                else window.SetTextBox(window.tbCryoState, "ON");
            }
            else
            {
                window.SetTextBox(window.tbCryoState, "UNKNOWN");
                string message = "Cryo control exception: When querying the state of LakeShore 336 temperature controller Relay-" + RelayNumber + ", an unexpected response was provided. It is likely that the cryo has not been turned on. \n\nClick OK to continue.";
                string caption = "Cryo Control Exception";
                MessageBox.Show(message, caption, MessageBoxButtons.OK);
            }
            return status;
        }

        public void SetCryoState(bool setState)
        {
            if (setState) TurnOnCryoCooler();
            else TurnOffCryoCooler();
        }

        private void TurnOnCryoCooler()
        {
            string status = PollCryoStatus(); // Query the status of the relay.
            if (status == Off) // If off, then try to turn it on
            {
                bool relaySuccessFlag;
                lock (LakeShore336Lock)
                {
                    relaySuccessFlag = tempController.SetRelayParameters(RelayNumber, Int32.Parse(On)); //Turn cryo on
                }
                // Now check if the LakeShore relay has been changed correctly
                string newstatus = PollCryoStatus();
                if (newstatus == Off)// Cryo is still off - throw an exception
                {
                    string message = "Cryo control exception: The LakeShore 336 temperature controller Relay-" + RelayNumber + " has not been correctly changed. This means that the cryo cooler has not been turned on.\n\nClick OK to continue.";
                    string caption = "Cryo Control Exception";
                    MessageBox.Show(message, caption, MessageBoxButtons.OK);
                }
            }
            else
            {
                if (status == On) // The cryo was already turned on! Has some element of hardware been changed manually? Throw exception.
                {
                    string message = "Cryo control exception: Relay was already set such that the cryo should be on! Was the relay changed manually? Is the cryo set to remote mode?\n\nClick OK to continue.";
                    string caption = "Cryo Control Exception";
                    MessageBox.Show(message, caption, MessageBoxButtons.OK);
                }
            }
        }

        private void TurnOffCryoCooler()
        {
            string status = PollCryoStatus(); // Query the status of the relay.
            if (status == On) // If on, then try to turn it off
            {
                bool relaySuccessFlag;
                lock (LakeShore336Lock)
                {
                    relaySuccessFlag = tempController.SetRelayParameters(RelayNumber, Int32.Parse(Off));
                }
                // Now check if the LakeShore relay has been changed correctly
                string newstatus = PollCryoStatus();
                if (newstatus == On)// Cryo is still on - throw an exception
                {
                    string message = "Cryo control exception: The LakeShore 336 temperature controller Relay-" + RelayNumber + " has not been correctly changed. This means that the cryo cooler has not been turned off.\n\nClick OK to continue.";
                    string caption = "Cryo Control Exception";
                    MessageBox.Show(message, caption, MessageBoxButtons.OK);
                }
            }
            else
            {
                if (status == Off) // The cryo was already turned off! Has some element of hardware been changed manually? Throw exception.
                {
                    string message = "Cryo control exception: Relay was already set such that the cryo should be off! Was the relay changed manually? Is the cryo set to remote mode?\n\nClick OK to continue.";
                    string caption = "Cryo Control Exception";
                    MessageBox.Show(message, caption, MessageBoxButtons.OK);
                }
            }
        }

        #endregion

        #region Status

        private string LastStatusMessage = "";
        private bool StatusRepeatFlag;

        private void UpdateStatus(string str)
        {
            if(str!= LastStatusMessage)
            {
                window.AppendTextBox(window.tbStatus, str + Environment.NewLine);
                //LastStatusMessage = str;
            }
        }

        #endregion

        #region Source Modes

        private string SourceMode;

        // Source mode functions
        public void UpdateUITimeLeftIndicators()
        {
            if (SourceMode == "Refresh")
            {
                TimeSpan TimeLeftUntilCryoTurnsOn = CryoTurnOnDateTime - DateTime.Now; // Update user interface with the time left until the cryo turns on.
                window.SetTextBox(window.tbRefreshModeHowLongUntilCryoTurnsOn, TimeLeftUntilCryoTurnsOn.ToString(@"d\.hh\:mm\:ss")); // Update textbox to inform user how long is left until the heating process will be forced to stop early and the cryo turned on
                TimeSpan TimeLeftUntilHeatersTurnOff = HeatersTurnOffDateTime - DateTime.Now; // Update user interface with the time left until the heaters turn off.
                window.SetTextBox(window.tbRefreshModeHowLongUntilHeatersTurnOff, TimeLeftUntilHeatersTurnOff.ToString(@"d\.hh\:mm\:ss")); // Update textbox to inform user how long is left until the heating process will be forced to stop early and the cryo turned on
            }
            if (SourceMode == "Warmup")
            {
                TimeSpan TimeLeftUntilHeatersTurnOff = HeatersTurnOffDateTime - DateTime.Now; // Update user interface with the time left until the heaters turn off.
                window.SetTextBox(window.tbWarmUpModeHowLongUntilHeatersTurnOff, TimeLeftUntilHeatersTurnOff.ToString(@"d\.hh\:mm\:ss")); // Update textbox to inform user how long is left until the heating process will be stopped
            }
            if (SourceMode == "Cooldown")
            {
                TimeSpan TimeLeftUntilCryoTurnsOn = CryoTurnOnDateTime - DateTime.Now; // Update user interface with the time left until the cryo turns on.
                window.SetTextBox(window.tbCoolDownModeHowLongUntilCryoTurnsOn, TimeLeftUntilCryoTurnsOn.ToString(@"d\.hh\:mm\:ss")); // Update textbox to inform user how long is left until the heating process will be stopped
                TimeSpan TimeLeftUntilHeatersTurnOff = HeatersTurnOffDateTime - DateTime.Now; // Update user interface with the time left until the heaters turn off.
                window.SetTextBox(window.tbCoolDownModeHowLongUntilHeatersTurnOff, TimeLeftUntilHeatersTurnOff.ToString(@"d\.hh\:mm\:ss")); // Update textbox to inform user how long is left until the cryo is turned on
            }
        }
        public void ResetUITimeLeftIndicators()
        {
            if (SourceMode == "Refresh")
            {
                window.SetTextBox(window.tbRefreshModeHowLongUntilCryoTurnsOn, ""); // Clear textbox
                window.SetTextBox(window.tbRefreshModeHowLongUntilHeatersTurnOff, ""); // Clear textbox
            }
            if (SourceMode == "Warmup")
            {
                window.SetTextBox(window.tbWarmUpModeHowLongUntilHeatersTurnOff, ""); // Clear textbox
            }
            if (SourceMode == "Cooldown")
            {
                window.SetTextBox(window.tbCoolDownModeHowLongUntilCryoTurnsOn, ""); // Clear textbox
                window.SetTextBox(window.tbCoolDownModeHowLongUntilHeatersTurnOff, ""); // Clear textbox
            }
        }
        public void UpdateSourceModeStatus(string StatusUpdate,string mode)
        {
            if (StatusUpdate != LastSourceModeStatusMessage)
            {
                LastSourceModeStatusMessage = StatusUpdate;
                if (mode == "Refresh")
                {
                    window.AppendTextBox(window.tbRefreshModeStatus, StatusUpdate + Environment.NewLine);
                }
                if (mode == "Warmup")
                {
                    window.AppendTextBox(window.tbWarmUpModeStatus, StatusUpdate + Environment.NewLine);
                }
                if (mode == "Cooldown")
                {
                    window.AppendTextBox(window.tbCoolDownModeStatus, StatusUpdate + Environment.NewLine);
                }
            }
        }
        /// <summary>
        /// This method will write a message a textbox in the user interface. 
        /// The textbox written to will depend on the SourceMode that is currently being run. 
        /// If a source mode is not yet running, then use UpdateSourceModeStatus(string StatusUpdate,string mode), 
        /// where the mode can be input as an argument.
        /// </summary>
        /// <param name="StatusUpdate"></param>
        public void UpdateSourceModeStatus(string StatusUpdate)
        {
            UpdateSourceModeStatus(StatusUpdate, SourceMode);
        }
        public void UpdateSourceModeHeaterSetpoints(double setpoint)
        {
            // Second stage heater
            window.SetTextBox(window.tbHeaterTempSetpointStage2, setpoint.ToString());
            UpdateStage2TemperatureSetpoint();
            // First stage heater
            window.SetTextBox(window.tbHeaterTempSetpointStage1, setpoint.ToString());
            UpdateStage1TemperatureSetpoint();
            // Cell heater
            SetHeaterSetpoint(LakeShoreCellOutput, setpoint);
        }
        public void EnableSourceModeHeaters(bool Enable)
        {
            if (Enable)
            {
                if (!SourceModeHeatersEnabledFlag)
                {
                    // First stage heater
                    StartStage1HeaterControl(); // turn heaters setpoint loop on
                    // Second stage heater
                    StartStage2HeaterControl(); // turn heaters setpoint loop on
                    // Cell heater
                    EnableLakeShoreHeaterOutput1or2(LakeShoreCellOutput, 3);

                    SourceModeHeatersEnabledFlag = true;
                }
            }
            else
            {
                if (SourceModeHeatersEnabledFlag)
                {
                    // First stage heater
                    StopStage1HeaterControl(); // turn heaters setpoint loop off 
                    EnableDigitalHeaters(1, false); // turn heaters off (when stopped, the setpoint loop will leave the heaters in their last enabled/disabled state)
                    // Second stage heater
                    StopStage2HeaterControl(); // turn heaters setpoint loop off
                    EnableDigitalHeaters(2, false); // turn heaters off (when stopped, the setpoint loop will leave the heaters in their last enabled/disabled state)
                    // Cell heater
                    EnableLakeShoreHeaterOutput1or2(LakeShoreCellOutput, 0);

                    SourceModeHeatersEnabledFlag = false;
                }
            }
        }
        public void EnableWarmUpModeUIControls(bool Enable)
        {
            window.EnableControl(window.dateTimePickerWarmUpModeTurnHeatersOff, Enable);
            window.EnableControl(window.tbWarmUpModeHowLongUntilHeatersTurnOff, Enable);
            window.EnableControl(window.tbWarmUpModeTemperatureSetpoint, Enable);
            window.EnableControl(window.btWarmUpModeTemperatureSetpointUpdate, Enable);
            window.EnableControl(window.btStartWarmUpMode, Enable);
        }
        public void EnableRefreshModeUIControls(bool Enable)
        {
            window.EnableControl(window.btStartRefreshMode, Enable);
            window.EnableControl(window.tbRefreshModeTemperatureSetpoint, Enable);
            window.EnableControl(window.btRefreshModeTemperatureSetpointUpdate, Enable);
            window.EnableControl(window.dateTimePickerRefreshModeTurnHeatersOff, Enable);
            window.EnableControl(window.dateTimePickerRefreshModeTurnCryoOn, Enable);
            window.EnableControl(window.tbRefreshModeHowLongUntilCryoTurnsOn, Enable);
            window.EnableControl(window.tbRefreshModeHowLongUntilHeatersTurnOff, Enable);
        }
        public void EnableCoolDownModeUIControls(bool Enable)
        {
            window.EnableControl(window.btStartCoolDownMode, Enable);
            window.EnableControl(window.tbCoolDownModeTemperatureSetpoint, Enable);
            window.EnableControl(window.btCoolDownModeTemperatureSetpointUpdate, Enable);
            window.EnableControl(window.dateTimePickerCoolDownModeTurnHeatersOff, Enable);
            window.EnableControl(window.dateTimePickerCoolDownModeTurnCryoOn, Enable);
            window.EnableControl(window.tbCoolDownModeHowLongUntilCryoTurnsOn, Enable);
            window.EnableControl(window.tbCoolDownModeHowLongUntilHeatersTurnOff, Enable);
        }
        public void EnableOtherSourceModeUIControls(bool Enable)
        {
            // Enable user control of the cryo on/off status
            // Enable user control of the heater setpoint when in source mode
            window.EnableControl(window.btUpdateHeaterControlStage2, Enable);
            window.EnableControl(window.btUpdateHeaterControlStage1, Enable);
            window.EnableControl(window.btHeatersTurnOffWaitStart, Enable);
        }

        // Source mode parameters
        private DateTime HeatersTurnOffDateTime;
        private DateTime CryoTurnOnDateTime;
        private double GasEvaporationCycleTemperatureMax;
        private double TurbomolecularPumpUpperPressureLimit;
        private double WarmUpTemperatureSetpoint;
        private double CryoStoppingPressure;
        private double CryoStartingTemperatureMax;
        private double CryoStartingPressure;
        private string LastSourceModeStatusMessage;
        private bool SourceModeTemperatureSetpointUpdated;
        private bool SourceModeHeatersEnabledFlag = false;
        private bool sourceModeCancelFlag;
        private bool SourceModeActive = false;
        private int DesorbingPTPollPeriod;
        private int WarmupPTPollPeriod;
        private int SourceModeWaitPeriod;
        private int CoolDownPTPollPeriod;
        public void SetSourceModeConstants()
        {
            if (SourceMode == "Refresh")
            {
                GasEvaporationCycleTemperatureMax = SourceRefreshConstants.GasEvaporationCycleTemperatureMax;
                TurbomolecularPumpUpperPressureLimit = SourceRefreshConstants.TurbomolecularPumpUpperPressureLimit;
                DesorbingPTPollPeriod = SourceRefreshConstants.DesorbingPTPollPeriod;
                CryoStoppingPressure = SourceRefreshConstants.CryoStoppingPressure;
                WarmupPTPollPeriod = SourceRefreshConstants.WarmupPTPollPeriod;
                CoolDownPTPollPeriod = SourceRefreshConstants.CoolDownPTPollPeriod;
                CryoStartingTemperatureMax = SourceRefreshConstants.CryoStartingTemperatureMax;
                CryoStartingPressure = SourceRefreshConstants.CryoStartingPressure;
                SourceModeWaitPeriod = SourceRefreshConstants.SourceModeWaitPTPollPeriod;
                HeatersTurnOffDateTime = window.dateTimePickerRefreshModeTurnHeatersOff.Value;
                CryoTurnOnDateTime = window.dateTimePickerRefreshModeTurnCryoOn.Value;
            }
            if (SourceMode == "Warmup")
            {
                GasEvaporationCycleTemperatureMax = SourceWarmUpConstants.GasEvaporationCycleTemperatureMax;
                TurbomolecularPumpUpperPressureLimit = SourceWarmUpConstants.TurbomolecularPumpUpperPressureLimit;
                DesorbingPTPollPeriod = SourceWarmUpConstants.DesorbingPTPollPeriod;
                CryoStoppingPressure = SourceWarmUpConstants.CryoStoppingPressure;
                WarmupPTPollPeriod = SourceWarmUpConstants.WarmupPTPollPeriod;
                SourceModeWaitPeriod = SourceWarmUpConstants.SourceModeWaitPTPollPeriod;
                HeatersTurnOffDateTime = window.dateTimePickerWarmUpModeTurnHeatersOff.Value;
            }
            if (SourceMode == "Cooldown")
            {
                TurbomolecularPumpUpperPressureLimit = SourceCoolDownConstants.TurbomolecularPumpUpperPressureLimit;
                WarmupPTPollPeriod = SourceCoolDownConstants.WarmupPTPollPeriod;
                CoolDownPTPollPeriod = SourceCoolDownConstants.CoolDownPTPollPeriod;
                CryoStartingTemperatureMax = SourceCoolDownConstants.CryoStartingTemperatureMax;
                CryoStartingPressure = SourceCoolDownConstants.CryoStartingPressure;
                SourceModeWaitPeriod = SourceCoolDownConstants.SourceModeWaitPTPollPeriod;
                HeatersTurnOffDateTime = window.dateTimePickerCoolDownModeTurnHeatersOff.Value;
                CryoTurnOnDateTime = window.dateTimePickerCoolDownModeTurnCryoOn.Value;
            }
        }

        // Source mode processes - these are generalized so that they can be used by different source mode (refresh mode, warm up, etc.) using different parameters.
        private void InitializeSourceMode()
        {
            SetSourceModeConstants();
            SourceModeActive = true;
            SourceModeTemperatureSetpointUpdated = false; // reset this flag
            if (SourceMode == "Refresh")
            {
                UpdateSourceModeStatus("Initializing refresh mode (" + DateTime.Now.ToString("F", CultureInfo.CreateSpecificCulture("en-UK")) + ")");
                StopShutdown(refreshModeShutdownBlockHandle, refreshModeShutdownBlockReason);
                RefreshModeEnableUIElements(true);
                UpdateRefreshTemperature();
                UpdateStatus("Refresh mode started");
            }
            if (SourceMode == "Warmup")
            {
                UpdateSourceModeStatus("Initializing warmup mode (" + DateTime.Now.ToString("F", CultureInfo.CreateSpecificCulture("en-UK")) + ")");
                StopShutdown(warmupModeShutdownBlockHandle, warmupModeShutdownBlockReason);
                WarmUpModeEnableUIElements(true);
                UpdateWarmUpTemperature();
                UpdateStatus("Warm up mode started");
            }
            if (SourceMode == "Cooldown")
            {
                UpdateSourceModeStatus("Initializing cool down mode (" + DateTime.Now.ToString("F", CultureInfo.CreateSpecificCulture("en-UK")) + ")");
                StopShutdown(cooldownModeShutdownBlockHandle, cooldownModeShutdownBlockReason);
                CoolDownModeEnableUIElements(true);
                UpdateCoolDownTemperature();
                UpdateStatus("Cool down mode started");
            }
        }
        private void DesorbAndPumpGases()
        {
            if (!sourceModeCancelFlag)
            {
                UpdatePTMonitorPollPeriod(DesorbingPTPollPeriod);
                UpdateSourceModeHeaterSetpoints(GasEvaporationCycleTemperatureMax);
                UpdateSourceModeStatus("Starting desorption cycle");
            }

            for (; ; )// for (; ; ) is an infinite loop, equivalent to while(true)
            {
                var watch = System.Diagnostics.Stopwatch.StartNew();  // Use stopwatch to track how long it takes to measure values. This is subtracted from the poll period so that the loop is executed at the proper frequency.
                if (sourceModeCancelFlag) break; // Immediately break this for loop if the user has requested that source mode be cancelled
                
                UpdateUITimeLeftIndicators();
                
                if (lastSourcePressure >= TurbomolecularPumpUpperPressureLimit) // If the pressure is too high, then the heaters should be disabled so that the turbomolecular pump is not damaged
                {
                    if (Stage1HeaterControlFlag & Stage2HeaterControlFlag)
                    {
                        EnableSourceModeHeaters(false); // Disable heaters
                    }
                }
                else
                {
                    EnableSourceModeHeaters(true); // Enable heaters
                    if (lastS2Temp >= GasEvaporationCycleTemperatureMax) // Check if the S2 temperature has reached the end of the neon evaporation cycle (there should be little neon left to evaporate after S2 temperature = GasEvaporationCycleTemperatureMax)
                    {
                        if (lastSourcePressure <= CryoStoppingPressure) // If the pressure is low enough that the cryo cooler can be turned off, then break the for loop.
                        {
                            break;
                        }
                        UpdateSourceModeStatus("Neon evaporation cycle: temperature has reached setpoint, but pressure too high for cryo shutdown");
                    }
                }
                watch.Stop();
                //UpdateStatus(Convert.ToString(watch.ElapsedMilliseconds));
                Thread.Sleep(DesorbingPTPollPeriod);
            }
        }
        private void TurnOffCryoAndWarmup()
        {
            if (!sourceModeCancelFlag) // If source mode has been cancelled then skip these functions
            {
                // Wait for cryo pressure to reduce to a point at which the cryo can be turned off.
                for (; ; )
                {
                    if (sourceModeCancelFlag) break; // If source mode has been cancelled then exit this loop (check on every iteration of the loop)
                    if (lastSourcePressure <= CryoStoppingPressure) // If the pressure is low enough that the cryo cooler can be turned off, then break the for loop.
                    {
                        break;
                    }
                    UpdateUITimeLeftIndicators();
                    UpdateSourceModeStatus("Waiting for pressure to reduce before turning off the cryo. The cryo will be turned off when the source chamber pressure reaches " + CryoStoppingPressure.ToString() + " mbar.");
                    Thread.Sleep(WarmupPTPollPeriod); // Iterate the loop according to this time interval
                }
                //EnableCryoDigitalControl(false); // Turn off cryo
                SetCryoState(false);
                UpdateSourceModeStatus("Starting warmup - cryo turned off (" + DateTime.Now.ToString("F",CultureInfo.CreateSpecificCulture("en-UK")) + ")");
                UpdatePTMonitorPollPeriod(WarmupPTPollPeriod);

                // Monitor the pressure as the source heats up. 
                // If the pressure gets too high for the turbo, then turn off the heaters. 
                // If pressure is low enough, then turn on the heaters.
                for (; ; )
                {
                    // Check conditions that would lead to this loop being exited:
                    if (sourceModeCancelFlag) break; // If source mode has been cancelled then exit this loop (check on every iteration of the loop)
                    if (HeatersTurnOffDateTime < DateTime.Now) break; // If the user requested that the heaters turn off before the temperature setpoint has been reached, then exit this loop.

                    // Update UI indicators and check if the user has updated the warm up temperature:
                    UpdateUITimeLeftIndicators();
                    if (SourceModeTemperatureSetpointUpdated) // If the temperature setpoint is updated by the user, this if statement will update the hardware temperature setpoint.
                    {
                        UpdateSourceModeHeaterSetpoints(WarmUpTemperatureSetpoint); // Set heater setpoints to user defined value
                        SourceModeTemperatureSetpointUpdated = false; // Reset the flag
                    }

                    // Check is the source pressure is within safe limits for the turbomolecular pump:
                    if (lastSourcePressure < TurbomolecularPumpUpperPressureLimit) // If pressure is low, then turn the heaters on
                    {
                        if (!Stage1HeaterControlFlag | !Stage2HeaterControlFlag) // if heaters turned off then turn them on
                        {
                            EnableSourceModeHeaters(true); // Enable heaters
                        }
                        if (Double.Parse(lastS2TempString) >= WarmUpTemperatureSetpoint) // If the source has reached the desired temperature, then break the loop
                        {
                            break;
                        }
                        UpdateSourceModeStatus("Warming source: temperature setpoint " + WarmUpTemperatureSetpoint.ToString() + " Kelvin not yet reached."); // Update source mode status textbox
                    }
                    else // If the pressure is high, then turn the heaters off
                    {
                        UpdateSourceModeStatus("Warming source: heaters disabled because the source chamber pressure is above the safe operating limit for the turbo (" + TurbomolecularPumpUpperPressureLimit.ToString() + " mbar)");
                        if (Stage1HeaterControlFlag | Stage2HeaterControlFlag) // if heaters are on
                        {
                            EnableSourceModeHeaters(false); // disable heaters
                        }
                    }
                    Thread.Sleep(WarmupPTPollPeriod); // Iterate the loop according to this time interval
                }
            }
        }
        public void SourceModeWait()
        {
            if (!sourceModeCancelFlag)
            {
                UpdatePTMonitorPollPeriod(SourceModeWaitPeriod);
                //If the source reaches the desired temperature before the user defined heater turn off time, then wait until this time.
                for (; ; )
                {
                    // Check conditions that would lead to this loop being exited:
                    if (sourceModeCancelFlag) break; // If refresh mode has been cancelled then exit this loop (check on every iteration of the loop)
                    if (HeatersTurnOffDateTime < DateTime.Now)
                    {
                        UpdateSourceModeStatus("Heaters turned off (" + DateTime.Now.ToString("F", CultureInfo.CreateSpecificCulture("en-UK")) + ")");
                        break; // If the user requested that the heaters turn off at this time, then exit this loop.
                    }

                    // Check if the user has updated the temperature setpoint:
                    if (SourceModeTemperatureSetpointUpdated) // If the temperature setpoint is updated by the user, this if statement will update the hardware temperature setpoint.
                    {
                        UpdateSourceModeHeaterSetpoints(WarmUpTemperatureSetpoint); // Set heater setpoints to user defined value
                        SourceModeTemperatureSetpointUpdated = false; //Reset the flag
                    }
                    
                    
                    // Check is the source pressure is within safe limits for the turbomolecular pump:
                    if (lastSourcePressure < TurbomolecularPumpUpperPressureLimit) // If pressure is low, then turn the heaters on
                    {
                        if (!Stage1HeaterControlFlag | !Stage2HeaterControlFlag) // if heaters turned off then turn them on
                        {
                            EnableSourceModeHeaters(true); // Enable heaters
                        }
                        UpdateSourceModeStatus("Heating to/waiting at temperature setpoint (" + WarmUpTemperatureSetpoint + " Kelvin)"); // Update source mode status textbox
                    }
                    else // If the pressure is high, then turn the heaters off
                    {
                        UpdateSourceModeStatus("Heating to/waiting at " + WarmUpTemperatureSetpoint + " Kelvin. Heaters disabled because the source chamber pressure is above the safe operating limit for the turbo (" + TurbomolecularPumpUpperPressureLimit.ToString() + " mbar)");
                        if (Stage1HeaterControlFlag | Stage2HeaterControlFlag) // if heaters are on
                        {
                            EnableSourceModeHeaters(false); // Disable heaters
                        }
                    }
                    

                    UpdateUITimeLeftIndicators(); // Update user interface indicators to show how long is left until the heaters turn off and/or the cryo turns on
                    Thread.Sleep(SourceModeWaitPeriod); // Iterate the loop according to this time interval
                }
            }
        }
        private void CoolDownSource()
        {
            if (!sourceModeCancelFlag)
            {
                UpdatePTMonitorPollPeriod(CoolDownPTPollPeriod);
                EnableSourceModeHeaters(false); // Turn off heaters

                // Wait until the (user defined) cryo turn on time is reached:
                for (; ; )
                {
                    if (sourceModeCancelFlag) break;
                    if (CryoTurnOnDateTime < DateTime.Now) break; // Exit this loop when the user defined datetime is reached.
                    Thread.Sleep(CoolDownPTPollPeriod);
                    UpdateUITimeLeftIndicators(); // Update user interface indicators to show how long is left until the cryo turns on
                }

                if (!sourceModeCancelFlag) ResetUITimeLeftIndicators(); // Clears the user interface textboxes now that the timers have finished.

                // Wait until the source temperature is low enough for the cryo to be started:
                for (; ; )
                {
                    if (sourceModeCancelFlag) break;
                    if (Double.Parse(lastS1TempString) <= CryoStartingTemperatureMax & Double.Parse(lastS2TempString) <= CryoStartingTemperatureMax)
                    { break; }
                    UpdateSourceModeStatus("Waiting for temperature to reach the safe operating range for cryo to turn on"); // Update source mode status
                    Thread.Sleep(CoolDownPTPollPeriod);
                }

                // Wait for the pressure to be low enough for the cryo to be started:
                for (; ; )
                {
                    if (sourceModeCancelFlag) break;
                    if (lastSourcePressure <= CryoStartingPressure)
                    { break; }
                    UpdateSourceModeStatus("Waiting for the pressure to reach a low enough value for the cryo to turn on"); // Update source mode status
                    Thread.Sleep(CoolDownPTPollPeriod);
                }
                if (!sourceModeCancelFlag)
                {
                    UpdateSourceModeStatus("Starting cryo (" + DateTime.Now.ToString("F", CultureInfo.CreateSpecificCulture("en-UK")) + ")");
                    //EnableCryoDigitalControl(true); // Temporary digital control
                    SetCryoState(true);
                }
            }
        }

        // Refresh mode
        private Thread refreshModeThread;
        private Object refreshModeLock;
        private bool refreshModeHeaterTurnOffDateTimeFlag = false;
        private bool refreshModeCryoTurnOnDateTimeFlag = false;
        private static string refreshModeShutdownBlockReason = "Refresh mode enabled!";
        private static IntPtr refreshModeShutdownBlockHandle;
        /// <summary>
        /// When refresh mode starts or stops, various controls in the user interface need to be disabled or enabled so that the user doesn't accidently do something that will interfere with refresh mode. This function can be used to disable or enable the relevant UI components.
        /// </summary>
        /// <param name="StartStop"></param>
        internal void RefreshModeEnableUIElements(bool Enable) // elements to enable/disable when starting/finishing refresh mode
        {
            // Disable and enable to Start and Stop buttons respectively
            window.EnableControl(window.btStartRefreshMode, !Enable); // window.btStartRefreshMode.Enabled = false when starting refresh mode (for example)
            window.EnableControl(window.btCancelRefreshMode, Enable); // window.btCancelRefreshMode.Enabled = true when starting refresh mode (for example)
            // Disable user control of warmup mode when in refresh mode
            EnableWarmUpModeUIControls(!Enable);
            // Disable user control of cool down mode when in refresh mode
            EnableCoolDownModeUIControls(!Enable);
            // Disable other UI interface elements to prevent the user from performing an action that could interfere with refresh mode
            EnableOtherSourceModeUIControls(!Enable);
        }
        public void RefreshModeHeaterTurnOffDateTimeSpecified()
        {
            refreshModeHeaterTurnOffDateTimeFlag = true;
            UpdateRefreshModeHeaterTurnOffDateTime();
        }
        public void UpdateRefreshModeHeaterTurnOffDateTime()
        {
            HeatersTurnOffDateTime = window.dateTimePickerRefreshModeTurnHeatersOff.Value;
        }
        public void RefreshModeCryoTurnOnDateTimeSpecified()
        {
            refreshModeCryoTurnOnDateTimeFlag = true;
            UpdateRefreshModeCryoTurnOnDateTime();
        }
        public void UpdateRefreshModeCryoTurnOnDateTime()
        {
            CryoTurnOnDateTime = window.dateTimePickerRefreshModeTurnCryoOn.Value;
        }
        public void UpdateRefreshTemperature()
        {
            string RefreshTemperatureInput = window.tbRefreshModeTemperatureSetpoint.Text;
            double parseddouble;
            if (Double.TryParse(RefreshTemperatureInput, out parseddouble))
            {
                WarmUpTemperatureSetpoint = parseddouble;
                SourceModeTemperatureSetpointUpdated = true;
            }
            else MessageBox.Show("Unable to parse refresh temperature string. Ensure that a number has been written, with no additional non-numeric characters.", "", MessageBoxButtons.OK);
        }
        public void RefreshModeSetWindowsAPIShutdownHandle(IntPtr Handle)
        {
            refreshModeShutdownBlockHandle = Handle;
        }
        public void PrintRefreshModeConstants()
        {
            string mode = "Refresh";
            UpdateSourceModeStatus("Refresh mode constants updated. Here are the current settings:", mode);
            UpdateSourceModeStatus("TurbomolecularPumpUpperPressureLimit = "+ SourceRefreshConstants.TurbomolecularPumpUpperPressureLimit.ToString("E3")+" mbar", mode);
            UpdateSourceModeStatus("GasEvaporationCycleTemperatureMax = " + SourceRefreshConstants.GasEvaporationCycleTemperatureMax.ToString("E3") + " K", mode);
            UpdateSourceModeStatus("DesorbingPTPollPeriod = " + SourceRefreshConstants.DesorbingPTPollPeriod.ToString() + " ms", mode);
            UpdateSourceModeStatus("CryoStoppingPressure = " + SourceRefreshConstants.CryoStoppingPressure.ToString("E3") + " mbar", mode);
            UpdateSourceModeStatus("WarmupPTPollPeriod = " + SourceRefreshConstants.WarmupPTPollPeriod.ToString() + " ms", mode);
            UpdateSourceModeStatus("SourceModeWaitPTPollPeriod = " + SourceRefreshConstants.SourceModeWaitPTPollPeriod.ToString() + " ms", mode);
            UpdateSourceModeStatus("CoolDownPTPollPeriod = " + SourceRefreshConstants.CoolDownPTPollPeriod.ToString() + " ms", mode);
            UpdateSourceModeStatus("CryoStartingPressure = " + SourceRefreshConstants.CryoStartingPressure.ToString("E3") + " mbar", mode);
            UpdateSourceModeStatus("CryoStartingTemperatureMax = " + SourceRefreshConstants.CryoStartingTemperatureMax.ToString("E3") + " K", mode);
        }
        public void LoadRefreshModeOptionsDialog()
        {
            RefreshModeOptionsDialog changeRefreshModeOptionsDialog = new RefreshModeOptionsDialog();
            changeRefreshModeOptionsDialog.ShowDialog();


            if (changeRefreshModeOptionsDialog.DialogResult != DialogResult.Cancel) // If the user chooses to cancel the action then do nothing
            {
                if (changeRefreshModeOptionsDialog.DialogResult == DialogResult.OK)
                {
                    if (changeRefreshModeOptionsDialog.RefreshConstantChangedFlag)
                    {
                        SourceRefreshConstants.TurbomolecularPumpUpperPressureLimit = changeRefreshModeOptionsDialog.TurbomolecularPumpUpperPressureLimitDoubleValue;
                        SourceRefreshConstants.GasEvaporationCycleTemperatureMax = changeRefreshModeOptionsDialog.GasEvaporationCycleTemperatureMaxDoubleValue;
                        SourceRefreshConstants.DesorbingPTPollPeriod = changeRefreshModeOptionsDialog.DesorbingPTPollPeriodIntValue;
                        SourceRefreshConstants.CryoStoppingPressure = changeRefreshModeOptionsDialog.CryoStoppingPressureDoubleValue;
                        SourceRefreshConstants.WarmupPTPollPeriod = changeRefreshModeOptionsDialog.WarmupPTPollPeriodIntValue;
                        SourceRefreshConstants.SourceModeWaitPTPollPeriod = changeRefreshModeOptionsDialog.SourceModeWaitPTPollPeriodIntValue;
                        SourceRefreshConstants.CoolDownPTPollPeriod = changeRefreshModeOptionsDialog.CoolDownPTPollPeriodIntValue;
                        SourceRefreshConstants.CryoStartingPressure = changeRefreshModeOptionsDialog.CryoStartingPressureDoubleValue;
                        SourceRefreshConstants.CryoStartingTemperatureMax = changeRefreshModeOptionsDialog.CryoStartingTemperatureMaxDoubleValue;
                        PrintRefreshModeConstants();
                    }
                    else UpdateSourceModeStatus("Refresh mode options opened, but not changed.", "Refresh");
                }
            }
            else UpdateSourceModeStatus("Refresh mode options opened, but not changed.", "Refresh");
            changeRefreshModeOptionsDialog.Dispose();
        }
        

        public static class SourceRefreshConstants
        {
            // Global constants
            public static Double TurbomolecularPumpUpperPressureLimit = 0.0008;                          // 8e-4 mbar
            public static Int32 PTPollPeriodMinimum = UEDMController.PTMonitorPollPeriodLowerLimit;      // ms
            public static Double CryoMaxTemperatureWhenTurnedOff = 335;                                  // K

            // Warm up constants
            public static Double GasEvaporationCycleTemperatureMax = 40; // Kelvin
            public static Int16 S1LakeShoreHeaterOutput = 3;             // Output number on the LakeShore temperature controller
            public static Int16 S2LakeShoreHeaterOutput = 4;             // Output number on the LakeShore temperature controller
            public static Int32 DesorbingPTPollPeriod = 100;             // milli seconds
            public static Double CryoStoppingPressure = 0.00005;         // 5e-5 mbar
            public static Double RefreshingTemperature = 300;            // Kelvin
            public static Int32 WarmupPTPollPeriod = 1000;                // milli seconds

            // Constants once warm up temperature has been reached
            public static Int32 SourceModeWaitPTPollPeriod = 10000;       // milli seconds


            // Cool down
            public static Double CryoStartingPressure = 0.00005;         // 5e-5 mbar
            public static Double CryoStartingTemperatureMax = 320;       // Kelvin
            public static Int32 CoolDownPTPollPeriod = 15000;             // milli seconds
        }

        internal void StartRefreshMode()
        {
            UpdateRefreshModeHeaterTurnOffDateTime();
            UpdateRefreshModeCryoTurnOnDateTime();
            if (refreshModeHeaterTurnOffDateTimeFlag)
            {
                if (refreshModeCryoTurnOnDateTimeFlag)
                {
                    if (CryoTurnOnDateTime >= HeatersTurnOffDateTime)// The heaters can be stopped before or at the same time as the cryo turning on
                    {
                        if (CryoTurnOnDateTime > DateTime.Now) // The cryo cannot be turned on in the past - otherwise you should just turn on the cryo (instead of using refresh mode)
                        {
                            if (HeatersTurnOffDateTime > DateTime.Now) // The heaters cannot be turned off in the past - otherwise you should just turn off the heaters (instead of using refresh mode)
                            {
                                if (SourceModeTemperatureSetpointUpdated)
                                {
                                    refreshModeThread = new Thread(new ThreadStart(refreshModeWorker));
                                    SourceMode = "Refresh";
                                    refreshModeLock = new Object();
                                    sourceModeCancelFlag = false;
                                    refreshModeThread.Start();
                                }
                                else
                                {
                                    MessageBox.Show("Please provide a refresh mode temperature (and click update).\n\nRefresh mode not started.", "Refresh Mode Exception", MessageBoxButtons.OK);
                                }
                            }
                            else
                            {
                                MessageBox.Show("User has requested that the heaters turn off... in the past. Adjust the datetimes to sensible values.\n\nRefresh mode not started.\n\nDo these messages come across as passive agressive? I hope not...", "Refresh Mode Exception", MessageBoxButtons.OK);
                            }
                        }
                        else
                        {
                            MessageBox.Show("User has requested that the cryo turns on... in the past. Adjust the datetimes to sensible values.\n\nRefresh mode not started.", "Refresh Mode Exception", MessageBoxButtons.OK);
                        }
                    }
                    else
                    {
                        MessageBox.Show("User has requested that the cryo turns on before the heaters turn off - adjust the datetimes to sensible values.\n\nRefresh mode not started.", "Refresh Mode Exception", MessageBoxButtons.OK);
                    }
                }
                else
                {
                    MessageBox.Show("No datetime specified for turning on the cryo.\n\nRefresh mode not started.", "Refresh Mode Exception", MessageBoxButtons.OK);
                }
            }
            else
            {
                MessageBox.Show("No datetime specified for turning off the heaters.\n\nRefresh mode not started.", "Refresh Mode Exception", MessageBoxButtons.OK);
            }
        }
        internal void CancelRefreshMode()
        {
            sourceModeCancelFlag = true;
        }
        private void refreshModeWorker()
        {
            if (!sourceModeCancelFlag) InitializeSourceMode();
            if (!sourceModeCancelFlag) DesorbAndPumpGases(); // Controlled evaporation of gases from cryo pump
            if (!sourceModeCancelFlag) TurnOffCryoAndWarmup(); // Cryo turn off and controlled warm up off source
            if (!sourceModeCancelFlag) SourceModeWait(); // Wait at desired temperature, until the user defined datetime
            if (!sourceModeCancelFlag) CoolDownSource(); // Turn on cryo
            if (sourceModeCancelFlag) // If refresh mode is cancelled, then turn off the heaters before finishing.
            {
                UpdateSourceModeStatus("Refresh mode cancelled\n");
                EnableSourceModeHeaters(false); // Disable heaters
                ResetUITimeLeftIndicators();
                UpdateStatus("Refresh mode cancelled");
            }
            RefreshModeEnableUIElements(false); // Enable/disable UI elements that had been disabled/enabled whilst in refresh mode.
            SourceMode = ""; // Reset parameter
            SourceModeActive = false;
            ResetShutdown(refreshModeShutdownBlockHandle);
            UpdateStatus("Refresh mode finished");
        }

        // Warm up mode
        private Thread warmupModeThread;
        private Object warmupModeLock;
        private bool warmupModeHeaterTurnOffDateTimeFlag = false;
        private bool warmupModeTemperatureSetpointUpdated = false;
        private static string warmupModeShutdownBlockReason = "Warm up mode enabled!";
        private static IntPtr warmupModeShutdownBlockHandle;
        /// <summary>
        /// When warmup mode starts or stops, various controls in the user interface need to be disabled or enabled so that the user doesn't accidently do something that will interfere with warmup mode. This function is used to disable or enable the relevant UI components.
        /// </summary>
        /// <param name="StartStop"></param>
        internal void WarmUpModeEnableUIElements(bool Enable) // elements to enable/disable when starting/finishing warm up mode
        {
            // Disable and enable to Start and Cancel buttons respectively
            window.EnableControl(window.btStartWarmUpMode, !Enable); // window.btStartWarmUpMode.Enabled = false when starting warmup mode (for example)
            window.EnableControl(window.btCancelWarmUpMode, Enable); // window.btCancelWarmUpMode.Enabled = true when starting warmup mode (for example)
            // Disable user control of refresh mode when in warmup mode
            EnableRefreshModeUIControls(!Enable);
            // Disable user control of cool down mode when in warmup mode
            EnableCoolDownModeUIControls(!Enable);
            // Disable other UI interface elements to prevent the user from performing an action that could interfere with warm up mode
            EnableOtherSourceModeUIControls(!Enable);
        }
        public void WarmUpModeHeaterTurnOffDateTimeSpecified()
        {
            warmupModeHeaterTurnOffDateTimeFlag = true;
            UpdateWarmUpModeHeaterTurnOffDateTime();
        }
        public void UpdateWarmUpModeHeaterTurnOffDateTime()
        {
            HeatersTurnOffDateTime = window.dateTimePickerWarmUpModeTurnHeatersOff.Value;
        }
        public void UpdateWarmUpTemperature()
        {
            string WarmUpTemperatureInput = window.tbWarmUpModeTemperatureSetpoint.Text;
            double parseddouble;
            if (Double.TryParse(WarmUpTemperatureInput, out parseddouble))
            {
                WarmUpTemperatureSetpoint = parseddouble;
                SourceModeTemperatureSetpointUpdated = true;
                warmupModeTemperatureSetpointUpdated = true;
            }
            else MessageBox.Show("Unable to parse warm up temperature string. Ensure that a number has been written, with no additional non-numeric characters.", "", MessageBoxButtons.OK);
        }
        public void WarmupModeSetWindowsAPIShutdownHandle(IntPtr Handle)
        {
            warmupModeShutdownBlockHandle = Handle;
        }
        public void PrintWarmupModeConstants()
        {
            string mode = "Warmup";
            UpdateSourceModeStatus("Warm up mode constants updated. Here are the current settings:", mode);
            UpdateSourceModeStatus("TurbomolecularPumpUpperPressureLimit = " + SourceWarmUpConstants.TurbomolecularPumpUpperPressureLimit.ToString("E3") + " mbar", mode);
            UpdateSourceModeStatus("GasEvaporationCycleTemperatureMax = " + SourceWarmUpConstants.GasEvaporationCycleTemperatureMax.ToString("E3") + " K", mode);
            UpdateSourceModeStatus("DesorbingPTPollPeriod = " + SourceWarmUpConstants.DesorbingPTPollPeriod.ToString() + " ms", mode);
            UpdateSourceModeStatus("CryoStoppingPressure = " + SourceWarmUpConstants.CryoStoppingPressure.ToString("E3") + " mbar", mode);
            UpdateSourceModeStatus("WarmupPTPollPeriod = " + SourceWarmUpConstants.WarmupPTPollPeriod.ToString() + " ms", mode);
            UpdateSourceModeStatus("SourceModeWaitPTPollPeriod = " + SourceWarmUpConstants.SourceModeWaitPTPollPeriod.ToString() + " ms", mode);
        }
        public void LoadWarmupModeOptionsDialog()
        {
            WarmUpModeOptionsDialog warmupOptionsDialog = new WarmUpModeOptionsDialog();
            warmupOptionsDialog.ShowDialog();


            if (warmupOptionsDialog.DialogResult != DialogResult.Cancel) // If the user chooses to cancel the action then don't update SourceWarmUpConstants
            {
                if (warmupOptionsDialog.DialogResult == DialogResult.OK)
                {
                    if (warmupOptionsDialog.WarmupConstantChangedFlag)
                    {
                        SourceWarmUpConstants.TurbomolecularPumpUpperPressureLimit = warmupOptionsDialog.TurbomolecularPumpUpperPressureLimitDoubleValue;
                        SourceWarmUpConstants.GasEvaporationCycleTemperatureMax = warmupOptionsDialog.GasEvaporationCycleTemperatureMaxDoubleValue;
                        SourceWarmUpConstants.DesorbingPTPollPeriod = warmupOptionsDialog.DesorbingPTPollPeriodIntValue;
                        SourceWarmUpConstants.CryoStoppingPressure = warmupOptionsDialog.CryoStoppingPressureDoubleValue;
                        SourceWarmUpConstants.WarmupPTPollPeriod = warmupOptionsDialog.WarmupPTPollPeriodIntValue;
                        SourceWarmUpConstants.SourceModeWaitPTPollPeriod = warmupOptionsDialog.SourceModeWaitPTPollPeriodIntValue;
                        PrintWarmupModeConstants();
                    }
                    else UpdateSourceModeStatus("Warm up mode options opened, but not changed.", "Warmup");
                }
            }
            else UpdateSourceModeStatus("Warm up mode options opened, but not changed.", "Warmup");
            warmupOptionsDialog.Dispose();
        }

        public static class SourceWarmUpConstants
        {
            // Global constants
            public static Double TurbomolecularPumpUpperPressureLimit = 0.0008;                      // 8e-4 mbar
            public static Int32 PTPollPeriodMinimum = UEDMController.PTMonitorPollPeriodLowerLimit;  // ms

            // Warmup constants
            public static Double GasEvaporationCycleTemperatureMax = 40;         // Kelvin
            public static Int16 S1LakeShoreHeaterOutput = 3;                     // 
            public static Int16 S2LakeShoreHeaterOutput = 4;                     // 
            public static Int32 DesorbingPTPollPeriod = 100;                     // milli seconds
            public static Double CryoStoppingPressure = 0.00005;                 // 5e-5 mbar
            public static Int32 WarmupPTPollPeriod = 1000;                       // milli seconds
            public static Int32 SourceModeWaitPTPollPeriod = 10000;              // milli seconds
        }

        internal void StartWarmUpMode()
        {
            UpdateWarmUpModeHeaterTurnOffDateTime(); // Update the heater turn off datetime. This is in case, for example, the refresh mode heater turn off time was last updated.
            if (warmupModeHeaterTurnOffDateTimeFlag) // A flag specific to warm up mode - to avoid conflicts with other modes
            {
                if (HeatersTurnOffDateTime > DateTime.Now) // The heaters cannot be turned off in the past 
                {
                    if (warmupModeTemperatureSetpointUpdated)
                    {
                        warmupModeThread = new Thread(new ThreadStart(warmupModeWorker));
                        SourceMode = "Warmup";
                        warmupModeLock = new Object();
                        sourceModeCancelFlag = false;
                        warmupModeThread.Start();
                    }
                    else
                    {
                        MessageBox.Show("Please provide a warm up mode temperature (and click update).\n\nWarm up mode not started.", "Warm Up Mode Exception", MessageBoxButtons.OK);
                    }
                }
                else
                {
                    MessageBox.Show("User has requested that the heaters turn off in the past. Adjust the datetimes to sensible values.\n\nWarm up mode not started.\n\nDo these messages come across as passive agressive? I hope not...", "Warm Up Mode Exception", MessageBoxButtons.OK);
                }
            }
            else
            {
                MessageBox.Show("No datetime specified for turning off the heaters.\n\nWarm up mode not started.", "Warm Up Mode Exception", MessageBoxButtons.OK);
            }
        }
        internal void CancelWarmUpMode()
        {
            sourceModeCancelFlag = true;
        }
        private void warmupModeWorker()
        {
            if (!sourceModeCancelFlag) InitializeSourceMode();
            if (!sourceModeCancelFlag) DesorbAndPumpGases(); // Controlled evaporation of gases from cryo pump
            if (!sourceModeCancelFlag) TurnOffCryoAndWarmup(); // Cryo turn off and controlled warm up of source
            if (!sourceModeCancelFlag) SourceModeWait(); // Wait at desired temperature, until the user defined datetime
            if (sourceModeCancelFlag) // If warm up mode is cancelled, then turn off the heaters before finishing.
            {
                UpdateSourceModeStatus("Warm up mode cancelled\n");
                EnableSourceModeHeaters(false); // Disable heaters
                ResetUITimeLeftIndicators();
                UpdateStatus("Warm up mode canelled");
            }
            WarmUpModeEnableUIElements(false); // Enable/disable UI elements that had been disabled/enabled whilst in warm up mode.
            SourceMode = ""; // Reset parameter
            SourceModeActive = false;
            ResetShutdown(warmupModeShutdownBlockHandle);
            UpdateStatus("Warm up mode finished");
        }

        // Cool down mode
        private Thread CoolDownModeThread;
        private Object CoolDownModeLock;
        private bool CoolDownModeHeaterTurnOffDateTimeFlag = false;
        private bool CoolDownModeCryoTurnOnDateTimeFlag = false;
        private bool CoolDownModeTemperatureSetpointUpdated = false;
        private static string cooldownModeShutdownBlockReason = "Cool down mode enabled!";
        private static IntPtr cooldownModeShutdownBlockHandle;
        internal void CoolDownModeEnableUIElements(bool Enable) // UI elements to enable/disable when starting/finishing cool down mode
        {
            // Disable and enable to Start and Stop buttons respectively
            window.EnableControl(window.btStartCoolDownMode, !Enable); // window.btStartCoolDownMode.Enabled = false when starting cool down mode (for example)
            window.EnableControl(window.btCancelCoolDownMode, Enable); // window.btCancelCoolDownMode.Enabled = true when starting cool down mode (for example)
            // Disable user control of warmup mode when in cool down mode
            EnableWarmUpModeUIControls(!Enable);
            // Disable user control of refresh mode when in cool down mode
            EnableRefreshModeUIControls(!Enable);
            // Disable other UI interface elements to prevent the user from performing an action that could interfere with cool down mode
            EnableOtherSourceModeUIControls(!Enable);
        }
        public void CoolDownModeHeaterTurnOffDateTimeSpecified()
        {
            CoolDownModeHeaterTurnOffDateTimeFlag = true;
            UpdateCoolDownModeHeaterTurnOffDateTime();
        }
        public void UpdateCoolDownModeHeaterTurnOffDateTime()
        {
            HeatersTurnOffDateTime = window.dateTimePickerCoolDownModeTurnHeatersOff.Value;
        }
        public void CoolDownModeCryoTurnOnDateTimeSpecified()
        {
            CoolDownModeCryoTurnOnDateTimeFlag = true;
            UpdateCoolDownModeCryoTurnOnDateTime();
        }
        public void UpdateCoolDownModeCryoTurnOnDateTime()
        {
            CryoTurnOnDateTime = window.dateTimePickerCoolDownModeTurnCryoOn.Value;
        }
        public void UpdateCoolDownTemperature()
        {
            string WarmUpTemperatureInput = window.tbCoolDownModeTemperatureSetpoint.Text;
            double parseddouble;
            if (Double.TryParse(WarmUpTemperatureInput, out parseddouble))
            {
                WarmUpTemperatureSetpoint = parseddouble;
                SourceModeTemperatureSetpointUpdated = true;
                CoolDownModeTemperatureSetpointUpdated = true;
            }
            else MessageBox.Show("Unable to parse temperature string. Ensure that a number has been written, with no additional non-numeric characters.", "", MessageBoxButtons.OK);
        }
        public void CoolDownModeSetWindowsAPIShutdownHandle(IntPtr Handle)
        {
            cooldownModeShutdownBlockHandle = Handle;
        }
        public void PrintCooldownModeConstants()
        {
            string mode = "Cooldown";
            UpdateSourceModeStatus("Cool down mode constants updated. Here are the current settings:", mode);
            UpdateSourceModeStatus("TurbomolecularPumpUpperPressureLimit = " + SourceCoolDownConstants.TurbomolecularPumpUpperPressureLimit.ToString("E3") + " mbar", mode);
            UpdateSourceModeStatus("WarmupPTPollPeriod = " + SourceCoolDownConstants.WarmupPTPollPeriod.ToString() + " ms", mode);
            UpdateSourceModeStatus("SourceModeWaitPTPollPeriod = " + SourceCoolDownConstants.SourceModeWaitPTPollPeriod.ToString() + " ms", mode);
            UpdateSourceModeStatus("CoolDownPTPollPeriod = " + SourceCoolDownConstants.CoolDownPTPollPeriod.ToString() + " ms", mode);
            UpdateSourceModeStatus("CryoStartingPressure = " + SourceCoolDownConstants.CryoStartingPressure.ToString("E3") + " mbar", mode);
            UpdateSourceModeStatus("CryoStartingTemperatureMax = " + SourceCoolDownConstants.CryoStartingTemperatureMax.ToString("E3") + " K", mode);
        }
        public void LoadCooldownModeOptionsDialog()
        {
            CooldownModeOptionsDialog CooldownOptionsDialog = new CooldownModeOptionsDialog();
            CooldownOptionsDialog.ShowDialog();


            if (CooldownOptionsDialog.DialogResult != DialogResult.Cancel) // If the user chooses to cancel the action then do nothing
            {
                if (CooldownOptionsDialog.DialogResult == DialogResult.OK)
                {
                    if (CooldownOptionsDialog.CooldownConstantChangedFlag)
                    {
                        SourceCoolDownConstants.TurbomolecularPumpUpperPressureLimit = CooldownOptionsDialog.TurbomolecularPumpUpperPressureLimitDoubleValue;
                        SourceCoolDownConstants.WarmupPTPollPeriod = CooldownOptionsDialog.WarmupPTPollPeriodIntValue;
                        SourceCoolDownConstants.SourceModeWaitPTPollPeriod = CooldownOptionsDialog.SourceModeWaitPTPollPeriodIntValue;
                        SourceCoolDownConstants.CoolDownPTPollPeriod = CooldownOptionsDialog.CoolDownPTPollPeriodIntValue;
                        SourceCoolDownConstants.CryoStartingPressure = CooldownOptionsDialog.CryoStartingPressureDoubleValue;
                        SourceCoolDownConstants.CryoStartingTemperatureMax = CooldownOptionsDialog.CryoStartingTemperatureMaxDoubleValue;
                        PrintCooldownModeConstants();
                    }
                    else UpdateSourceModeStatus("Cool down mode options opened, but not changed.", "Cooldown");
                }
            }
            else UpdateSourceModeStatus("Cool down mode options opened, but not changed.", "Cooldown");
            CooldownOptionsDialog.Dispose();
        }


        public static class SourceCoolDownConstants
        {
            // Global constants
            public static Double TurbomolecularPumpUpperPressureLimit = 0.0008;  // 8e-4 mbar
            public static Int32 PTPollPeriodMinimum = UEDMController.PTMonitorPollPeriodLowerLimit;                      // ms
            public static Double CryoMaxTemperatureWhenTurnedOff = 335;         // K

            // Warm up constants
            public static Int16 S1LakeShoreHeaterOutput = 3;                     // 
            public static Int16 S2LakeShoreHeaterOutput = 4;                     // 
            public static Int32 WarmupPTPollPeriod = 3000;                       // milli seconds

            // Wait at desired temperature constants
            public static Int32 SourceModeWaitPTPollPeriod = 10000;              // milli seconds

            // Cool down constants
            public static Int32 CoolDownPTPollPeriod = 15000;                    // milli seconds
            public static Double CryoStartingPressure = 0.00005;                 // 5e-5 mbar
            public static Double CryoStartingTemperatureMax = 320;               // Kelvin
        }

        internal void StartCoolDownMode()
        {
            UpdateCoolDownModeHeaterTurnOffDateTime();
            UpdateCoolDownModeCryoTurnOnDateTime();
            if (CoolDownModeHeaterTurnOffDateTimeFlag)
            {
                if (CoolDownModeCryoTurnOnDateTimeFlag)
                {
                    if (CryoTurnOnDateTime >= HeatersTurnOffDateTime)// The heaters can be stopped before or at the same time as the cryo turning on
                    {
                        if (CryoTurnOnDateTime > DateTime.Now) // The cryo cannot be turned on in the past - otherwise you should just turn on the cryo (instead of using cool down mode)
                        {
                            if (HeatersTurnOffDateTime > DateTime.Now) // The heaters shouldn't be turned off in the past
                            {
                                if (CoolDownModeTemperatureSetpointUpdated)
                                {
                                    CoolDownModeThread = new Thread(new ThreadStart(CoolDownModeWorker));
                                    SourceMode = "Cooldown";
                                    CoolDownModeLock = new Object();
                                    sourceModeCancelFlag = false;
                                    CoolDownModeThread.Start();
                                }
                                else
                                {
                                    MessageBox.Show("Please provide a cool down mode temperature (and click update).\n\nCool down mode not started.", "Cool Down Mode Exception", MessageBoxButtons.OK);
                                }
                            }
                            else
                            {
                                MessageBox.Show("User has requested that the heaters turn off... in the past. Adjust the datetimes to sensible values.\n\nCool down mode not started.\n\nDo these messages come across as passive agressive? I hope not...", "Cool Down Mode Exception", MessageBoxButtons.OK);
                            }
                        }
                        else
                        {
                            MessageBox.Show("User has requested that the cryo turns on... in the past. Adjust the datetimes to sensible values.\n\nCool down mode not started.", "Cool Down Mode Exception", MessageBoxButtons.OK);
                        }
                    }
                    else
                    {
                        MessageBox.Show("User has requested that the cryo turns on before the heaters turn off - adjust the datetimes to sensible values.\n\nCool down mode not started.", "Cool Down Mode Exception", MessageBoxButtons.OK);
                    }
                }
                else
                {
                    MessageBox.Show("No datetime specified for turning on the cryo.\n\nCool down mode not started.", "Cool Down Mode Exception", MessageBoxButtons.OK);
                }
            }
            else
            {
                MessageBox.Show("No datetime specified for turning off the heaters.\n\nCool down mode not started.", "Cool Down Mode Exception", MessageBoxButtons.OK);
            }
        }
        internal void CancelCoolDownMode()
        {
            sourceModeCancelFlag = true;
        }
        private void CoolDownModeWorker()
        {
            if (!sourceModeCancelFlag) InitializeSourceMode();
            if (!sourceModeCancelFlag) SourceModeWait(); // Wait at desired temperature, until the user defined datetime
            if (!sourceModeCancelFlag) CoolDownSource(); // Turn on cryo
            if (sourceModeCancelFlag) // If cool down mode is cancelled, then turn off the heaters before finishing.
            {
                UpdateSourceModeStatus("Cool down mode cancelled\n");
                EnableSourceModeHeaters(false); // Disable heaters
                ResetUITimeLeftIndicators();
                UpdateStatus("Cool down mode canelled");
            }
            CoolDownModeEnableUIElements(false); // Enable/disable UI elements that had been disabled/enabled whilst in cool down mode.
            SourceMode = ""; // Reset parameter
            SourceModeActive = false;
            ResetShutdown(cooldownModeShutdownBlockHandle);
            UpdateStatus("Cool down mode finished");
        }

        #endregion

        #region Digital heater control

        public bool Stage1HeaterControlFlag;
        public bool Stage2HeaterControlFlag;

        public double Stage1TemperatureSetpoint;
        public double Stage2TemperatureSetpoint;

        public double SetpointVariation = 2;

        public void EnableDigitalHeaters(int Channel, bool Enable)
        {
            if (Channel == 1)
            {
                SetDigitalLine("heatersS1TriggerDigitalOutputTask", Enable);
                window.SetCheckBox(window.checkBoxEnableHeatersS1, Enable);
            }
            else
            {
                if (Channel == 2)
                {
                    SetDigitalLine("heatersS2TriggerDigitalOutputTask", Enable);
                    window.SetCheckBox(window.checkBoxEnableHeatersS2, Enable);
                }
            }
        }

        public void StartStage1HeaterControl()
        {
            Stage1HeaterControlFlag = true;
            window.EnableControl(window.btStartHeaterControlStage1, false);
            window.EnableControl(window.btStopHeaterControlStage1, true);
            window.EnableControl(window.checkBoxEnableHeatersS1, false);
        }
        public void StartStage2HeaterControl()
        {
            Stage2HeaterControlFlag = true;
            window.EnableControl(window.btStartHeaterControlStage2, false);
            window.EnableControl(window.btStopHeaterControlStage2, true);
            window.EnableControl(window.checkBoxEnableHeatersS2, false);
        }

        public void StopStage1HeaterControl()
        {
            Stage1HeaterControlFlag = false; // change control flag so that the temperature setpoint loop stops
            window.EnableControl(window.btStartHeaterControlStage1, true);
            window.EnableControl(window.btStopHeaterControlStage1, false);
            window.EnableControl(window.checkBoxEnableHeatersS1, true);
            EnableDigitalHeaters(1, false); // turn off heater
        }
        public void StopStage2HeaterControl()
        {
            Stage2HeaterControlFlag = false; // change control flag so that the temperature setpoint loop stops
            window.EnableControl(window.btStartHeaterControlStage2, true);
            window.EnableControl(window.btStopHeaterControlStage2, false);
            window.EnableControl(window.checkBoxEnableHeatersS2, true);
            EnableDigitalHeaters(2, false); // turn off heater
        }

        public void UpdateStage2TemperatureSetpoint()
        {
            Stage2TemperatureSetpoint = Double.Parse(window.tbHeaterTempSetpointStage2.Text);
        }
        public void UpdateStage1TemperatureSetpoint()
        {
            Stage1TemperatureSetpoint = Double.Parse(window.tbHeaterTempSetpointStage1.Text);
        }

        public bool monitorPressureWhenHeating = true;

        public void EnableMonitorPressureWhenHeating(bool Enable)
        {
            monitorPressureWhenHeating = Enable;
        }
        public void ControlHeaters()
        {

            if (lastSourcePressure >= SourceRefreshConstants.TurbomolecularPumpUpperPressureLimit & monitorPressureWhenHeating) // If the pressure is too high, then the heaters should be disabled so that the turbomolecular pump is not damaged
            {
                window.SetTextBox(window.tbHeaterControlStatus, "Pressure above safe limit for turbo. Heaters disabled.");
                EnableDigitalHeaters(1, false); // turn heaters off
                EnableDigitalHeaters(2, false); // turn heaters off
            }
            else
            {
                window.SetTextBox(window.tbHeaterControlStatus, "");
                if (Stage2HeaterControlFlag)
                {
                    if (Double.Parse(lastS2TempString) < Stage2TemperatureSetpoint)
                    {
                        EnableDigitalHeaters(2, true);
                    }
                    else EnableDigitalHeaters(2, false);
                }
                if (Stage1HeaterControlFlag)
                {
                    if (Double.Parse(lastS1TempString) < Stage1TemperatureSetpoint)
                    {
                        EnableDigitalHeaters(1, true);
                    }
                    else EnableDigitalHeaters(1, false);
                }

            }
        }

        // Turn off heaters thread.
        // This allows the user to define a time at which the heaters will be turned off. 
        // This can be used to heat the source a little whilst away for lunch - evaporating neon while you eat!
        private Thread turnHeatersOffWaitThread;
        private int turnHeatersOffWaitPeriod = 1000;
        private bool turnHeatersOffCancelFlag;
        private Object turnHeatersOffWaitLock;

        internal void StartTurnHeatersOffWait()
        {
            turnHeatersOffWaitThread = new Thread(new ThreadStart(turnHeatersOffWaitWorker));
            window.EnableControl(window.btHeatersTurnOffWaitStart, false);
            window.EnableControl(window.btHeatersTurnOffWaitCancel, true);
            turnHeatersOffWaitLock = new Object();
            turnHeatersOffCancelFlag = false;
            turnHeatersOffWaitThread.Start();
        }
        internal void CancelTurnHeatersOffWait()
        {
            turnHeatersOffCancelFlag = true;
        }
        private void turnHeatersOffWaitWorker()
        {
            for (; ; )// for (; ; ) is an infinite loop, equivalent to while(true)
            {
                Thread.Sleep(turnHeatersOffWaitPeriod);
                if (turnHeatersOffCancelFlag)
                {
                    break;
                }
                if (window.dateTimePickerHeatersTurnOff.Value < DateTime.Now)
                {
                    StopStage2HeaterControl();
                    StopStage1HeaterControl();
                    EnableDigitalHeaters(1, false);
                    EnableDigitalHeaters(2, false);
                    break;
                }
                TimeSpan TimeLeft = window.dateTimePickerHeatersTurnOff.Value - DateTime.Now;
                window.SetTextBox(window.tbHowLongUntilHeatersTurnOff, TimeLeft.ToString(@"d\.hh\:mm\:ss"));
            }
            window.EnableControl(window.btHeatersTurnOffWaitStart, true);
            window.EnableControl(window.btHeatersTurnOffWaitCancel, false);
            window.SetTextBox(window.tbHowLongUntilHeatersTurnOff, "");
        }

        #endregion

        #region Pressure monitors

        private double lastSourcePressure;
        private double lastBeamlinePressure;
        private double lastDetectionPressure;
        private double SourceGaugeCorrectionFactor;
        private double BeamlineGaugeCorrectionFactor;
        private double DetectionGaugeCorrectionFactor;
        private int pressureMovingAverageSampleLength = 10;
        private int PressureChartRollingPeriod;
        private bool PressureChartRollingPeriodSelected = false;
        private bool PressureChartRollingXAxis = false;
        private Queue<double> pressureSamplesSource = new Queue<double>();
        private Queue<double> pressureSamplesBeamline = new Queue<double>();
        private Queue<double> pressureSamplesDetection = new Queue<double>();
        private string sourceSeries = "Source Pressure";
        private string beamlineSeries = "Beamline Pressure";
        private string detectionSeries = "Detection Pressure";

        private int numberOfPressureDataPoints = 0;
        private int pressureDataPlotLimit = 10;

        public void UpdatePressureMonitor()
        {
            //sample the pressure
            lock (HardwareControllerDAQCardLock) // Lock access to the DAQ card
            {
                lastSourcePressure = sourcePressureMonitor.Pressure * SourceGaugeCorrectionFactor;
                lastBeamlinePressure = beamlinePressureMonitor.Pressure * BeamlineGaugeCorrectionFactor;
                lastDetectionPressure = detectionPressureMonitor.Pressure * DetectionGaugeCorrectionFactor;
            }

            //add samples to Queues for averaging
            pressureSamplesSource.Enqueue(lastSourcePressure);
            pressureSamplesBeamline.Enqueue(lastBeamlinePressure);
            pressureSamplesDetection.Enqueue(lastDetectionPressure);


            //drop samples when array is larger than the moving average sample length
            while (pressureSamplesSource.Count > pressureMovingAverageSampleLength)
            {
                pressureSamplesSource.Dequeue();
            }
            while (pressureSamplesBeamline.Count > pressureMovingAverageSampleLength)
            {
                pressureSamplesBeamline.Dequeue();
            }
            while (pressureSamplesDetection.Count > pressureMovingAverageSampleLength)
            {
                pressureSamplesDetection.Dequeue();
            }

            //average samples
            double avgPressureSource = pressureSamplesSource.Average();
            string avgPressureSourceExpForm = avgPressureSource.ToString("E");
            double avgPressureBeamline = pressureSamplesBeamline.Average();
            string avgPressureBeamlineExpForm = avgPressureBeamline.ToString("E");
            double avgPressureDetection = pressureSamplesDetection.Average();
            string avgPressureDetectionExpForm = avgPressureDetection.ToString("E");

            //update UI monitor text boxes
            window.SetTextBox(window.tbPSource, (avgPressureSourceExpForm).ToString());
            window.SetTextBox(window.tbPBeamline, (avgPressureBeamlineExpForm).ToString());
            window.SetTextBox(window.tbPDetection, (avgPressureDetectionExpForm).ToString());
        }

        public void ClearPressureMonitorAv()
        {
            pressureSamplesSource.Clear();
            pressureSamplesBeamline.Clear();
            pressureSamplesDetection.Clear();
        }

        public void PlotLastPressure()
        {
            //sample the pressure
            lastSourcePressure = sourcePressureMonitor.Pressure;
            DateTime localDate = DateTime.Now;

            //plot the most recent samples
            window.AddPointToChart(window.chart1, sourceSeries, localDate, lastSourcePressure);
            window.AddPointToChart(window.chart1, detectionSeries, localDate, lastSourcePressure);

            // Handle rolling time axis on pressure chart
            if (PressureChartRollingXAxis) // If the user has requested that the time axis of the chart rolls
            {
                UpdatePressureChartRollingTimeAxis();  // Update chart time axis
            }
        }
        public void PlotPressureArrays(double[] SourcePressures, double[] BeamlinePressures, double[] DetectionPressures, DateTime[] MeasurementDateTimes)
        {
            AddArrayOfPointsToChart(window.chart1, sourceSeries, MeasurementDateTimes, SourcePressures);
            AddArrayOfPointsToChart(window.chart1, beamlineSeries, MeasurementDateTimes, BeamlinePressures);
            AddArrayOfPointsToChart(window.chart1, detectionSeries, MeasurementDateTimes, DetectionPressures);

            if (PressureChartRollingXAxis)
            {
                UpdatePressureChartRollingTimeAxis();
                UpdatePressureChartRollingYAxis();
            }
        }

        private void UpdatePressureChartRollingTimeAxis()
        {
            DateTime xMin = DateTime.Now.AddMilliseconds(-PressureChartRollingPeriod);
            window.SetChartXAxisMinDateTime(window.chart1, xMin);
        }
        public void UpdatePressureChartRollingYAxis()
        {
            // Calculate the number of points being displayed on the chart (when rolling axis is being applied)
            double RawRatio = PressureChartRollingPeriod / PTMonitorPollPeriod;
            double RoundedUpRatio = Math.Ceiling(RawRatio);
            int numberOfPointsBeingDisplayed = Convert.ToInt32(RoundedUpRatio);

            window.UpdateChartYScaleWhenXAxisRolling(window.chart1, numberOfPointsBeingDisplayed);
        }
        public void EnablePressureChartRollingTimeAxis(bool Enable)
        {
            if (Enable)
            {
                if (PressureChartRollingPeriodSelected)
                {
                    PressureChartRollingXAxis = true;
                }
                else
                {
                    MessageBox.Show("Please select pressure chart rolling period.", "User input exception", MessageBoxButtons.OK);
                    window.SetCheckBox(window.cbEnablePressureChartRollingTimeAxis, false);
                }
            }
            else
            {
                PressureChartRollingXAxis = false;
                window.SetChartXAxisMinAuto(window.chart1);
                window.SetChartYAxisAuto(window.chart1);
            }
        }
        public void UpdatePressureChartRollingPeriod()
        {
            int PressureChartRollingPeriodParsedValue;
            if (Int32.TryParse(window.tbRollingPressureChartTimeAxisPeriod.Text, out PressureChartRollingPeriodParsedValue)) 
            {
                if (PTMonitorPollPeriod <= PressureChartRollingPeriodParsedValue * 1000)  //*1000 to convert seconds to ms
                {
                    PressureChartRollingPeriod = PressureChartRollingPeriodParsedValue * 1000; // Update pressure chart rolling period  //*1000 to convert seconds to ms
                    PressureChartRollingPeriodSelected = true;
                    window.SetTextBox(window.tbRollingPressureChartTimeAxisPeriodMonitor, PressureChartRollingPeriodParsedValue.ToString());
                }
                else MessageBox.Show("Rolling period less than the polling period of pressure and temperature.", "User input exception", MessageBoxButtons.OK);
            }
            else MessageBox.Show("Unable to parse pressure chart rolling period string. Ensure that an integer number has been written, with no additional non-numeric characters.", "", MessageBoxButtons.OK);
        }

        public void UpdateGaugesCorrectionFactorsUsingUIInputs()
        {
            double SourceGaugeCorrectionFactorParsedValue;
            double BeamlineGaugeCorrectionFactorParsedValue;
            double DetectionGaugeCorrectionFactorParsedValue;

            if (Double.TryParse(window.tbSourceGaugeCorrectionFactor.Text, out SourceGaugeCorrectionFactorParsedValue))
            {
                SourceGaugeCorrectionFactor = SourceGaugeCorrectionFactorParsedValue; // Update source gauge correction factor
                window.SetTextBox(window.tbSourceGaugeCorrectionFactorMonitor, SourceGaugeCorrectionFactor.ToString());// Update the monitor value
            }
            else MessageBox.Show("Unable to parse source gauge correction factor string. Ensure that a double format number has been written, with no additional non-numeric characters.", "", MessageBoxButtons.OK);

            if (Double.TryParse(window.tbBeamlineGaugeCorrectionFactor.Text, out BeamlineGaugeCorrectionFactorParsedValue))
            {
                BeamlineGaugeCorrectionFactor = BeamlineGaugeCorrectionFactorParsedValue; // Update beamline gauge correction factor
                window.SetTextBox(window.tbBeamlineGaugeCorrectionFactorMonitor, BeamlineGaugeCorrectionFactor.ToString());// Update the monitor value
            }
            else MessageBox.Show("Unable to parse beamline gauge correction factor string. Ensure that a double format number has been written, with no additional non-numeric characters.", "", MessageBoxButtons.OK);

            if (Double.TryParse(window.tbDetectionGaugeCorrectionFactor.Text, out DetectionGaugeCorrectionFactorParsedValue))
            {
                DetectionGaugeCorrectionFactor = DetectionGaugeCorrectionFactorParsedValue; // Update Detection gauge correction factor
                window.SetTextBox(window.tbDetectionGaugeCorrectionFactorMonitor, DetectionGaugeCorrectionFactor.ToString());// Update the monitor value
            }
            else MessageBox.Show("Unable to parse detection gauge correction factor string. Ensure that a double format number has been written, with no additional non-numeric characters.", "", MessageBoxButtons.OK);
        }
        public void UpdateGaugesCorrectionFactors(double SourceCorrection, double BeamlineCorrection, double DetectorCorrection)
        {
            SourceGaugeCorrectionFactor = SourceCorrection; // Update source gauge correction factor
            window.SetTextBox(window.tbSourceGaugeCorrectionFactorMonitor, SourceGaugeCorrectionFactor.ToString());// Update the monitor value
            
            BeamlineGaugeCorrectionFactor = BeamlineCorrection; // Update beamline gauge correction factor
            window.SetTextBox(window.tbBeamlineGaugeCorrectionFactorMonitor, BeamlineGaugeCorrectionFactor.ToString());// Update the monitor value
            
            DetectionGaugeCorrectionFactor = DetectorCorrection; // Update Detection gauge correction factor
            window.SetTextBox(window.tbDetectionGaugeCorrectionFactorMonitor, DetectionGaugeCorrectionFactor.ToString());// Update the monitor value
        }
        public void ResetGaugesCorrectionFactors()
        {
            UpdateGaugesCorrectionFactors(initialSourceGaugeCorrectionFactor, initialBeamlineGaugeCorrectionFactor, initialDetectionGaugeCorrectionFactor);
        }

        private JSONSerializer pressureDataSerializer;
        public void StartLoggingPressure()
        {
            pressureDataSerializer = new JSONSerializer();
            Console.Write("here1");
            string initialDataDir = Environs.FileSystem.GetDataDirectory((String)Environs.FileSystem.Paths["HardwareControllerDataPath"]);
            //string currentYear = DateTime.Now.Year.ToString();
            //string currentMonth = DateTime.Now.Month.ToString();
            //string currentDay = DateTime.Now.Day.ToString();
            Console.Write("here2");

            pressureDataSerializer.StartLogFile(initialDataDir + "Temperature and Pressure Records\\Pressure\\" +
                Environs.FileSystem.GenerateNextDataFileName() + ".json");
            pressureDataSerializer.StartProcessingData();
        }
        public void StopLoggingPressure()
        {
            pressureDataSerializer.EndLogFile();
        }

        # endregion

        #region Temperature Monitors

        private string receivedData;
        private int TempMovingAverageSampleLength = 2;
        private int TemperatureChartRollingPeriod;
        private bool TemperatureChartRollingPeriodSelected = false;
        private bool TemperatureChartRollingXAxis = false;
        //private Queue<double> TempSamples = new Queue<double>();
        public string[] TemperatureArray;
        public string lastCellTempString;
        public string lastS1TempString;
        public string lastS2TempString;
        public string lastNeonTempString;
        public string lastSF6TempString;
        public double lastCellTemp;
        public double lastS1Temp;
        public double lastS2Temp;
        public double lastNeonTemp;
        public double lastSF6Temp;
        private string cellTSeries = "Cell Temperature";
        private string S1TSeries = "S1 Temperature";
        private string S2TSeries = "S2 Temperature";
        private string SF6TSeries = "SF6 Temperature";
        private string neonTSeries = "Neon Temperature";

        public void TryParseTemperatureString(string TemperatureString, string SeriesName)
        {
            double temporaryTemperatureVariable;
            if (Double.TryParse(TemperatureString, out temporaryTemperatureVariable))
            {
                if(SeriesName == S1TSeries)
                {
                    lastS1Temp = temporaryTemperatureVariable;
                }
                else
                {
                    if (SeriesName == cellTSeries)
                    {
                        lastCellTemp = temporaryTemperatureVariable;
                    }
                    else
                    {
                        if (SeriesName == S2TSeries)
                        {
                            lastS2Temp = temporaryTemperatureVariable;
                        }
                        else
                        {
                            if (SeriesName == SF6TSeries)
                            {
                                lastSF6Temp = temporaryTemperatureVariable;
                            }
                            else
                            {
                                if (SeriesName == neonTSeries)
                                {
                                    lastNeonTemp = temporaryTemperatureVariable;
                                }
                            }
                        }
                    }
                }
            }
            else MessageBox.Show("Unable to parse temperature string.", "LakeShore 336 temperature measurement exception", MessageBoxButtons.OK);
        }

        public void UpdateAllTempMonitors()
        {
            //sample the temperatures
            lock (LakeShore336Lock) // Lock access to the LakeShore 336 temperature controller
            {
                receivedData = tempController.GetTemperature(0, "K"); // This will return a string containing all the temperature measurements
            }
            TemperatureArray = receivedData.Split(','); // Split the comma delimited string into its components
            if (TemperatureArray.Length == 8)
            {
                // Retreive the temperature measurements (strings) from the string array
                lastCellTempString = TemperatureArray[0]; // LakeShore Input A
                lastNeonTempString = TemperatureArray[1]; // LakeShore Input B
                lastS2TempString = TemperatureArray[2];   // LakeShore Input C
                lastSF6TempString = TemperatureArray[3];  // LakeShore Input D
                lastS1TempString = TemperatureArray[4];   // LakeShore Input D1

                // Update window textboxes so that the user can see the most recent temperature measurement
                window.SetTextBox(window.tbTCell, lastCellTempString);
                window.SetTextBox(window.tbTNeon, lastNeonTempString);
                window.SetTextBox(window.tbTS2, lastS2TempString);
                window.SetTextBox(window.tbTS1, lastS1TempString);
                window.SetTextBox(window.tbTSF6, lastSF6TempString);

                // TryParse the temperature strings and save the values to double variables
                TryParseTemperatureString(lastCellTempString, cellTSeries);
                TryParseTemperatureString(lastNeonTempString, neonTSeries);
                TryParseTemperatureString(lastS2TempString, S2TSeries);
                TryParseTemperatureString(lastS1TempString, S1TSeries);
                TryParseTemperatureString(lastSF6TempString, SF6TSeries);
            }
            else
            {
                window.SetTextBox(window.tbTCell, "err_UpdateAllTempMonitors");
                window.SetTextBox(window.tbTNeon, "err_UpdateAllTempMonitors");
                window.SetTextBox(window.tbTS2, "err_UpdateAllTempMonitors");
                window.SetTextBox(window.tbTSF6, "err_UpdateAllTempMonitors");
                window.SetTextBox(window.tbTNeon, "err_UpdateAllTempMonitors");
            }
        }

        // The following function isn't currently being used, but has been kept as a back up for when we don't have the LakeShore temperature controller.
        double[] tempMonitorsData;
        /// <summary>
        /// Function to measure the temperature of silicon diodes via the analogue inputs of our data acquisition system (i.e. when we don't have the LakeShore temperature controller)
        /// </summary>
        public void UpdateAllTempMonitorsUsingDAQ()
        {
            //sample the temperatures
            tempMonitorsData = tempMonitors.Temperature();
            if (tempMonitorsData.Length == 4)
            {

                lastCellTempString = tempMonitorsData[0].ToString("N6");
                lastS1TempString = tempMonitorsData[1].ToString("N6");
                lastS2TempString = tempMonitorsData[2].ToString("N6");
                lastSF6TempString = tempMonitorsData[3].ToString("N6");
                window.SetTextBox(window.tbTCell, lastCellTempString);
                window.SetTextBox(window.tbTNeon, lastS1TempString);
                window.SetTextBox(window.tbTS2, lastS2TempString);
                window.SetTextBox(window.tbTSF6, lastSF6TempString);
            }
            else
            {
                window.SetTextBox(window.tbTCell, "err_UpdateAllTempMonitorsUsingDAQ");
                window.SetTextBox(window.tbTNeon, "err_UpdateAllTempMonitorsUsingDAQ");
                window.SetTextBox(window.tbTS2, "err_UpdateAllTempMonitorsUsingDAQ");
                window.SetTextBox(window.tbTSF6, "err_UpdateAllTempMonitorsUsingDAQ");
            }
        }

        public void PlotLastTemperatures()
        {
            DateTime localDate = DateTime.Now;
            double CellTemp = lastCellTemp;
            double S1Temp = lastS1Temp;
            double S2Temp = lastS2Temp;
            double SF6Temp = lastSF6Temp;
            double NeonTemp = lastNeonTemp;

            //plot the most recent samples
            window.AddPointToChart(window.chart2, cellTSeries, localDate, CellTemp);
            window.AddPointToChart(window.chart2, S1TSeries, localDate, S1Temp);
            window.AddPointToChart(window.chart2, S2TSeries, localDate, S2Temp);
            window.AddPointToChart(window.chart2, SF6TSeries, localDate, SF6Temp);
            window.AddPointToChart(window.chart2, neonTSeries, localDate, NeonTemp);
            if (TemperatureChartRollingXAxis)
            {
                UpdateTemperatureChartRollingTimeAxis();
            }
        }
        public void PlotTemperatureArrays(double[] CellTemperatures, double[] S1Temperatures, double[] S2Temperature, double[] SF6Temperatures, double[] NeonTemperatures, DateTime[] MeasurementDateTimes)
        {
            AddArrayOfPointsToChart(window.chart2, cellTSeries, MeasurementDateTimes, CellTemperatures);
            AddArrayOfPointsToChart(window.chart2, S1TSeries, MeasurementDateTimes, S1Temperatures);
            AddArrayOfPointsToChart(window.chart2, S2TSeries, MeasurementDateTimes, S2Temperature);
            AddArrayOfPointsToChart(window.chart2, SF6TSeries, MeasurementDateTimes, SF6Temperatures);
            AddArrayOfPointsToChart(window.chart2, neonTSeries, MeasurementDateTimes, NeonTemperatures);

            if (TemperatureChartRollingXAxis)
            {
                UpdateTemperatureChartRollingTimeAxis();
                UpdateTemperatureChartRollingYAxis();
            }
        }

        private void UpdateTemperatureChartRollingTimeAxis()
        {
            DateTime xMin = DateTime.Now.AddMilliseconds(-TemperatureChartRollingPeriod);
            window.SetChartXAxisMinDateTime(window.chart2, xMin);
        }
        public void UpdateTemperatureChartRollingYAxis()
        {
            // Calculate the number of points being displayed on the chart (when rolling axis is being applied)
            double RawRatio = TemperatureChartRollingPeriod / PTMonitorPollPeriod;
            double RoundedUpRatio = Math.Ceiling(RawRatio);
            int numberOfPointsBeingDisplayed = Convert.ToInt32(RoundedUpRatio);

            window.UpdateChartYScaleWhenXAxisRolling(window.chart2, numberOfPointsBeingDisplayed);
        }
        public void EnableTemperatureChartRollingTimeAxis(bool Enable)
        {
            if (Enable)
            {
                if (TemperatureChartRollingPeriodSelected)
                {
                    TemperatureChartRollingXAxis = true;
                }
                else
                {
                    MessageBox.Show("Please select temperature chart rolling period.", "User input exception", MessageBoxButtons.OK);
                    window.SetCheckBox(window.cbEnableTemperatureChartRollingTimeAxis, false);
                }
            }
            else
            {
                TemperatureChartRollingXAxis = false;
                window.SetChartXAxisMinAuto(window.chart2);
                window.SetChartYAxisAuto(window.chart2);
            }
        }
        public void UpdateTemperatureChartRollingPeriodUsingUIInput()
        {
            int TemperatureChartRollingPeriodParsedValue;
            if (Int32.TryParse(window.tbRollingTemperatureChartTimeAxisPeriod.Text, out TemperatureChartRollingPeriodParsedValue))
            {
                if (PTMonitorPollPeriod <= TemperatureChartRollingPeriodParsedValue * 1000)  //*1000 to convert seconds to ms
                {
                    TemperatureChartRollingPeriod = TemperatureChartRollingPeriodParsedValue * 1000; // Update temperature chart rolling period  //*1000 to convert seconds to ms
                    TemperatureChartRollingPeriodSelected = true;
                    window.SetTextBox(window.tbRollingTemperatureChartTimeAxisPeriodMonitor, TemperatureChartRollingPeriodParsedValue.ToString());
                }
                else MessageBox.Show("Rolling period less than the polling period of pressure and temperature.", "User input exception", MessageBoxButtons.OK);
            }
            else MessageBox.Show("Unable to parse temperature chart rolling period string. Ensure that an integer number has been written, with no additional non-numeric characters.", "", MessageBoxButtons.OK);
        }

        #endregion

        #region Temperature and Pressure Monitoring/Plotting
        // If the LakeShore is not in operation, the silicon diodes can still be monitored by measuring the voltage drop across them (which is temperature dependent).

        private Thread PTMonitorPollThread;
        private Thread PTPlottingThread;
        private int PTMonitorPollPeriod;
        public static int PTMonitorPollPeriodLowerLimit = 100; // LakeShore Model 336 limited to 10 readings per second for each input.
        private bool PTMonitorFlag;
        private bool PTPlottingFlag;
        private readonly object LakeShore336Lock = new object(); // Object for locking access to the lakeshore - preventing multiple threads from accesing the LakeShore simultaneously
        private readonly object HardwareControllerDAQCardLock = new object(); // Object for locking access to the DAQ card used for this hardware controller
        public string csvDataTemperatureAndPressure = "";

        // Arrays for temperature and pressure measurements. These are used for plotting the temperatures and pressures as the plotting can lag the measurement of these quantities.
        private readonly object PTPlottingBufferLock = new object();
        static int MaxPlottingArrayLength = 10000;
        public int NumberOfPTMeasurementsInQueue = 0;
        public double[] CellTempPlottingArray = new double[MaxPlottingArrayLength];
        public double[] S1TempPlottingArray = new double[MaxPlottingArrayLength];
        public double[] S2TempPlottingArray = new double[MaxPlottingArrayLength];
        public double[] NeonTempPlottingArray = new double[MaxPlottingArrayLength];
        public double[] SF6TempPlottingArray = new double[MaxPlottingArrayLength];
        public double[] SourcePressurePlottingArray = new double[MaxPlottingArrayLength];
        public double[] BeamlinePressurePlottingArray = new double[MaxPlottingArrayLength];
        public double[] DetectionPressurePlottingArray = new double[MaxPlottingArrayLength];
        public DateTime[] DateTimePlottingArray = new DateTime[MaxPlottingArrayLength];
        // Buffer arrays for plotting
        public double[] CellTempPlottingArrayBuffer;
        public double[] S1TempPlottingArrayBuffer;
        public double[] S2TempPlottingArrayBuffer;
        public double[] NeonTempPlottingArrayBuffer;
        public double[] SF6TempPlottingArrayBuffer;
        public double[] SourcePressurePlottingArrayBuffer;
        public double[] BeamlinePressurePlottingArrayBuffer;
        public double[] DetectionPressurePlottingArrayBuffer;
        public DateTime[] DateTimePlottingArrayBuffer;

        /// <summary>
        /// Many user interface (UI) components need to be enabled/disabled so that the user can't perform actions that could be harmful to the experiment. This function combines this list of UI elements.
        /// </summary>
        /// <param name="StartStop"></param>
        internal void PTMonitorPollEnableUIElements(bool Enable) // Elements to enable/disable when starting/stopping pressure and temperaure monitoring
        {
            // Pressure and Temperature monitoring start/stop buttons
            window.EnableControl(window.btStartTandPMonitoring, !Enable); // window.btStartTandPMonitoring.Enabled = false when starting monitoring (for example)
            window.EnableControl(window.btStopTandPMonitoring, Enable); // window.btStopTandPMonitoring.Enabled = true when stopping monitoring (for example)
            // Heater control UI elements
            window.EnableControl(window.btUpdateHeaterControlStage2, Enable);
            window.EnableControl(window.btStartHeaterControlStage2, Enable);
            window.EnableControl(window.btStartHeaterControlStage1, Enable);
            window.EnableControl(window.btUpdateHeaterControlStage1, Enable);
            window.EnableControl(window.btHeatersTurnOffWaitStart, Enable);
            // Source modes UI elements
            window.EnableControl(window.btStartRefreshMode, Enable);
            window.EnableControl(window.btStartWarmUpMode, Enable);
            window.EnableControl(window.btStartCoolDownMode, Enable);

        }

        /// <summary>
        /// Update the pressure and temperature monitoring poll period using the user defined value. If another form of input is needed,
        /// consider using UpdatePTMonitorPollPeriod(int pollPeriod). This function will throw an exception if the poll period is too low.
        /// </summary>
        public void UpdatePTMonitorPollPeriodUsingUIValue()
        {
            int PTMonitorPollPeriodParseValue;
            if (Int32.TryParse(window.tbTandPPollPeriod.Text, out PTMonitorPollPeriodParseValue))
            {
                if (PTMonitorPollPeriodParseValue >= PTMonitorPollPeriodLowerLimit)
                {
                    PTMonitorPollPeriod = PTMonitorPollPeriodParseValue; // Update PT monitoring poll period
                    window.SetTextBox(window.tbTandPPollPeriodMonitor, PTMonitorPollPeriod.ToString());
                }
                else MessageBox.Show("Poll period value too small. The temperature and pressure can only be polled every " + PTMonitorPollPeriodLowerLimit.ToString() + " ms. The limiting factor is communication with the LakeShore temperature controller.", "User input exception", MessageBoxButtons.OK);
            }
            else MessageBox.Show("Unable to parse setpoint string. Ensure that an integer number has been written, with no additional non-numeric characters.", "", MessageBoxButtons.OK);
        }
        /// <summary>
        /// Update the pressure and temperature monitoring poll period using int input. This function will throw an exception if the poll period is too low.
        /// </summary>
        /// <param name="pollPeriod"></param>
        public void UpdatePTMonitorPollPeriod(int pollPeriod)
        {
            if (pollPeriod >= PTMonitorPollPeriodLowerLimit)
            {
                PTMonitorPollPeriod = pollPeriod; // Update PT monitoring poll period
                window.SetTextBox(window.tbTandPPollPeriodMonitor, pollPeriod.ToString());
            }
            else MessageBox.Show("Poll period value too small. The temperature and pressure can only be polled every " + PTMonitorPollPeriodLowerLimit.ToString() + " ms. The limiting factor is communication with the LakeShore temperature controller.", "User input exception", MessageBoxButtons.OK);
        }

        public void UpdatePTPlottingArrays(DateTime MeasurementDateTimeStamp)
        {
            lock (PTPlottingBufferLock)
            {
                CellTempPlottingArray[NumberOfPTMeasurementsInQueue] = lastCellTemp;
                S1TempPlottingArray[NumberOfPTMeasurementsInQueue] = lastS1Temp;
                S2TempPlottingArray[NumberOfPTMeasurementsInQueue] = lastS2Temp;
                NeonTempPlottingArray[NumberOfPTMeasurementsInQueue] = lastNeonTemp;
                SF6TempPlottingArray[NumberOfPTMeasurementsInQueue] = lastSF6Temp;
                SourcePressurePlottingArray[NumberOfPTMeasurementsInQueue] = lastSourcePressure;
                BeamlinePressurePlottingArray[NumberOfPTMeasurementsInQueue] = lastBeamlinePressure;
                DetectionPressurePlottingArray[NumberOfPTMeasurementsInQueue] = lastDetectionPressure;
                DateTimePlottingArray[NumberOfPTMeasurementsInQueue] = MeasurementDateTimeStamp;
                
                ++NumberOfPTMeasurementsInQueue; // Count the number of measurements added to the plotting queue
            }
        }
        public void ClearPTCSVData()
        {
            csvDataTemperatureAndPressure = "";
        }
        public void SetPTCSVHeaderLine()
        {
            csvDataTemperatureAndPressure += "Unix Time Stamp (ms)" + "," + "Full date/time" + "," + "Cell Temperature (K)" + "," + "S1 Temperature (K)" + "," + "S2 Temperature (K)" + "," + "SF6 Temperature (K)" + "," + "Source Pressure (mbar)" + "," + "Beamline Pressure (mbar)" + "," + "Detection Pressure (mbar)" + "\r\n"; // Header lines for csv file
        }
        public void ResetPTCSVData()
        {
            ClearPTCSVData();
            SetPTCSVHeaderLine();
        }
        public void SavePTDataToCSV()
        {
            SavePlotDataToCSV(csvDataTemperatureAndPressure);
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
            PTPlottingThread = new Thread(() =>
            {
                PTPlottingWorker();
            });
            PTPlottingThread.IsBackground = true; // When the application is closed, this thread will also immediately stop. 

            pressureMovingAverageSampleLength = 10;
            Stage2HeaterControlFlag = false;
            Stage1HeaterControlFlag = false;
            UpdateStage1TemperatureSetpoint();
            UpdateStage2TemperatureSetpoint();
            PTMonitorPollEnableUIElements(true);
            if (csvDataTemperatureAndPressure == "") SetPTCSVHeaderLine();
            pressureSamplesSource.Clear();
            pressureSamplesBeamline.Clear();
            pressureSamplesDetection.Clear();
            PTMonitorFlag = false;
            PTPlottingFlag = false;
            PTMonitorPollThread.Start();
            PTPlottingThread.Start();
        }
        internal void StopPTMonitorPoll()
        {
            if (SourceModeActive)
            {
                MessageBox.Show(SourceMode + " mode is currently active. To stop temperature and pressure monitoring, please first cancel refresh mode and ensure that the apparatus is in a safe state to be left unmonitored.", "Refresh Mode Exception", MessageBoxButtons.OK);
            }
            else
            {
                StopStage1HeaterControl();
                StopStage2HeaterControl();
                EnableDigitalHeaters(1, false);
                EnableDigitalHeaters(2, false);
                PTMonitorFlag = true;
                PTPlottingFlag = true;
            }
        }
        private void PTMonitorPollWorker()
        {
            int count = 0;
            //int NumberofMovingAveragePoints = 2;
            //int MaxChartPoints = 100; // Maximum number of points that will be plotted on a given chart

            for (; ; )// for (; ; ) is an infinite loop, equivalent to while(true)
            {
                var watch = System.Diagnostics.Stopwatch.StartNew();  // Use stopwatch to track how long it takes to measure pressure and temperature. This is subtracted from the poll period so that the loop is executed at the proper frequency.
                ++count;
                // Measure temperatures and pressures
                // Note that locks are used to prevent threads attempting to access the DAQ card or LakeShore 336 temperature controller simultaneously
                UpdateAllTempMonitors(); // Measure temperatures and update the window textboxes with the current values
                UpdatePressureMonitor(); // Measure pressures and update the window textboxes with the current values
                DateTime MeasurementDateTimeStamp = DateTime.Now;

                // Update the data plotting queue
                UpdatePTPlottingArrays(MeasurementDateTimeStamp);
                    
                if (PTMonitorFlag)
                {
                    PTMonitorFlag = false;
                    break;
                }

                // Append data to string (to be written to a CSV file when the user wishes to save the data)
                Double unixTimestamp = (Double)(DateTime.Now.Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds);
                string csvLine = unixTimestamp + "," + DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss.fff tt") + "," + lastCellTempString + "," + lastS1TempString + "," + lastS2TempString + "," + lastSF6TempString + "," + lastSourcePressure + "," + lastBeamlinePressure + "," + lastDetectionPressure + "\r\n";
                csvDataTemperatureAndPressure += csvLine;

                // Enable/disable the heaters that are controlled using NI card digital outputs (as opposed to the self-contained LakeShore 336 heaters/sensors)
                ControlHeaters(); 

                // Calculate and subtract from the poll period the amount of time taken to perform the contents of this loop (so that the temperature and pressure are polled at the correct frequency)
                watch.Stop(); // Stop the stopwatch that was started at the start of the for loop
                int ThreadWaitPeriod = PTMonitorPollPeriod - Convert.ToInt32(watch.ElapsedMilliseconds); // Subtract the time elapsed from the user defined poll period
                if (ThreadWaitPeriod < 0)// If the result of the above subtraction was negative, set the value to zero so that Thread.Sleep() doesn't throw an exception
                {
                    ThreadWaitPeriod = 0;
                    if (!StatusRepeatFlag)
                    {
                        UpdateStatus("Unable to meet poll period requirement");
                        StatusRepeatFlag = true;
                    }
                }
                else
                {
                    StatusRepeatFlag = false;
                }

                //UpdateStatus(Convert.ToString(watch.ElapsedMilliseconds)); // Debugging
                Thread.Sleep(ThreadWaitPeriod); // Wait until the next temperature/pressure measurements are to be made
            }
            PTMonitorPollEnableUIElements(false);
        }
        private void PTPlottingWorker()
        {
            int PlottingQueueLength = 0;
            for (; ; )
            {
                Thread.Sleep(PTMonitorPollPeriod); // Wait until more data is taken

                lock (PTPlottingBufferLock) // Use lock to prevent new measurements being added to the plotting queue whilst the plotting function is in operation.
                {
                    PlottingQueueLength = NumberOfPTMeasurementsInQueue;
                    NumberOfPTMeasurementsInQueue = 0; // Reset the number of items in the queue to zero
                    // Create buffer arrays that can store the data locally - allowing temperature and pressure measurements to continue unimpeded.
                    CellTempPlottingArrayBuffer = new double[PlottingQueueLength];
                    S1TempPlottingArrayBuffer = new double[PlottingQueueLength];
                    S2TempPlottingArrayBuffer = new double[PlottingQueueLength];
                    NeonTempPlottingArrayBuffer = new double[PlottingQueueLength];
                    SF6TempPlottingArrayBuffer = new double[PlottingQueueLength];
                    SourcePressurePlottingArrayBuffer = new double[PlottingQueueLength];
                    BeamlinePressurePlottingArrayBuffer = new double[PlottingQueueLength];
                    DetectionPressurePlottingArrayBuffer = new double[PlottingQueueLength];
                    DateTimePlottingArrayBuffer = new DateTime[PlottingQueueLength];

                    if (PlottingQueueLength > 0) // If no data has been recorded, then there is no need to perform this function.
                    {
                        // Copy the data to buffer arrays so that the lock can be released and the monitoring thread can continue to make measurements of the temperature and pressure
                        long sourceIndex = 0; // Starting index of the data to be copied from the source array
                        long destinationIndex = 0; // Starting index of where the data will be stored in the destination (buffer) array
                        long lengthOfDataToCopy = Convert.ToInt64(PlottingQueueLength); // Number of measurements saved to the plotting queue (i.e. how many elements of the data arrays are to be copied to the buffer arrays)
                        Array.Copy(CellTempPlottingArray, sourceIndex, CellTempPlottingArrayBuffer, destinationIndex, lengthOfDataToCopy);
                        Array.Copy(S1TempPlottingArray, sourceIndex, S1TempPlottingArrayBuffer, destinationIndex, lengthOfDataToCopy);
                        Array.Copy(S2TempPlottingArray, sourceIndex, S2TempPlottingArrayBuffer, destinationIndex, lengthOfDataToCopy);
                        Array.Copy(NeonTempPlottingArray, sourceIndex, NeonTempPlottingArrayBuffer, destinationIndex, lengthOfDataToCopy);
                        Array.Copy(SF6TempPlottingArray, sourceIndex, SF6TempPlottingArrayBuffer, destinationIndex, lengthOfDataToCopy);
                        Array.Copy(SourcePressurePlottingArray, sourceIndex, SourcePressurePlottingArrayBuffer, destinationIndex, lengthOfDataToCopy);
                        Array.Copy(BeamlinePressurePlottingArray, sourceIndex, BeamlinePressurePlottingArrayBuffer, destinationIndex, lengthOfDataToCopy);
                        Array.Copy(DetectionPressurePlottingArray, sourceIndex, DetectionPressurePlottingArrayBuffer, destinationIndex, lengthOfDataToCopy);
                        Array.Copy(DateTimePlottingArray, sourceIndex, DateTimePlottingArrayBuffer, destinationIndex, lengthOfDataToCopy);
                        
                        // Clear the temperature/pressure plotting arrays and release the lock so that measurements can continue whilst plotting is performed.
                        int sourceClearIndex = Convert.ToInt32(sourceIndex);
                        int lengthOfDataToClear = Convert.ToInt32(lengthOfDataToCopy);
                        Array.Clear(CellTempPlottingArray, sourceClearIndex, lengthOfDataToClear);
                        Array.Clear(S1TempPlottingArray, sourceClearIndex, lengthOfDataToClear);
                        Array.Clear(S2TempPlottingArray, sourceClearIndex, lengthOfDataToClear);
                        Array.Clear(NeonTempPlottingArray, sourceClearIndex, lengthOfDataToClear);
                        Array.Clear(SF6TempPlottingArray, sourceClearIndex, lengthOfDataToClear);
                        Array.Clear(SourcePressurePlottingArray, sourceClearIndex, lengthOfDataToClear);
                        Array.Clear(BeamlinePressurePlottingArray, sourceClearIndex, lengthOfDataToClear);
                        Array.Clear(DetectionPressurePlottingArray, sourceClearIndex, lengthOfDataToClear);
                        Array.Clear(DateTimePlottingArray, sourceClearIndex, lengthOfDataToClear);
                    }
                }

                if (PlottingQueueLength > 0)
                {
                    // Plot pressure data
                    PlotPressureArrays(SourcePressurePlottingArrayBuffer, BeamlinePressurePlottingArrayBuffer, DetectionPressurePlottingArrayBuffer, DateTimePlottingArrayBuffer);
                    // Plot temperature data
                    PlotTemperatureArrays(CellTempPlottingArrayBuffer, S1TempPlottingArrayBuffer, S2TempPlottingArrayBuffer, SF6TempPlottingArrayBuffer, NeonTempPlottingArrayBuffer, DateTimePlottingArrayBuffer);
                }

                PlottingQueueLength = 0; // Reset variable

                if (PTPlottingFlag) // If the user has requested that monitoring/plotting of temperature/pressure be stopped, then break this loop
                {
                    PTPlottingFlag = false;
                    break;
                }
            }
        }

        #endregion

        #region Plotting functions

        public void ChangePlotYAxisScale(int ChartNumber)
        {
            if (ChartNumber == 1)
            {
                string YScale = window.comboBoxPlot1ScaleY.Text; // Read the Y scale mode chosen by the user in the UI
                window.ChangeChartYScale(window.chart1, YScale);
                window.SetAxisYIsStartedFromZero(window.chart1, false);
            }
            else
            {
                if (ChartNumber == 2)
                {
                    string YScale = window.comboBoxPlot2ScaleY.Text; // Read the Y scale mode chosen by the user in the UI
                    window.ChangeChartYScale(window.chart2, YScale);
                    window.SetAxisYIsStartedFromZero(window.chart2, false);
                }
                else
                {
                    if (ChartNumber == 4)
                    {
                        string YScale = window.comboBoxAnalogueInputsChartScaleY.Text; // Read the Y scale mode chosen by the user in the UI
                        window.ChangeChartYScale(window.chart4, YScale);
                        window.SetAxisYIsStartedFromZero(window.chart4, false);
                    }
                }
            }

        }

        /// <summary>
        /// Will enable or disable a data set in a specified plot. This allows the user to choose which data to plot on a chart.
        /// </summary>
        /// <param name="chart"></param>
        /// <param name="series"></param>
        /// <param name="enable"></param>
        public void EnableChartSeries(Chart chart, string series, bool enable)
        {
            window.EnableChartSeries(chart, series, enable);
        }

        /// <summary>
        ///  Will delete the plot data for a given series (in a given chart/plot)
        /// </summary>
        /// <param name="chart"></param>
        /// <param name="series"></param>
        public void ClearChartSeriesData(Chart chart, string series)
        {
            window.ClearChartSeriesData(chart, series);
        }

        public void AddArrayOfPointsToChart(Chart myChart, string mySeries, DateTime[] xArray, double[] yArray)
        {
            int count = 0;
            foreach (double TemperatureValue in yArray)
            {
                window.AddPointToChart(myChart, mySeries, xArray[count], TemperatureValue);
                ++count;
            }
        }

        #endregion

        #region Neon Flow Controller

        private double lastNeonFlowAct;
        private double lastNeonFlowSetpoint;
        private double newNeonFlowSetpoint;
        private string neonFlowActSeries = "Neon Flow";
        private string neonFlowChannelNumber = "2"; // Channel number on the MKS PR4000B flow controller
        private double neonFlowUpperLimit = 100; // Maximum neon flow that the MKS PR4000B flow controller is capable of.
        private double neonFlowLowerLimit = 0; // Minimum neon flow that the MKS PR4000B flow controller is capable of.

        public void UpdateNeonFlowActMonitor()
        {
            //sample the neon flow (actual)
            lastNeonFlowAct = neonFlowController.QueryActualValue(neonFlowChannelNumber);

            //update text boxes
            window.SetTextBox(window.tbNeonFlowActual, lastNeonFlowAct.ToString("N3"));
        }

        public void UpdateNeonFlowSetpointMonitor()
        {
            //sample the neon flow (actual)
            lastNeonFlowSetpoint = neonFlowController.QuerySetpoint(neonFlowChannelNumber);

            //update text boxes
            window.SetTextBox(window.tbNeonFlowSetpoint, lastNeonFlowSetpoint.ToString("N3"));
        }

        public void PlotLastNeonFlowAct()
        {
            DateTime localDate = DateTime.Now;

            //plot the most recent sample
            window.AddPointToChart(window.chart3, neonFlowActSeries, localDate, lastNeonFlowAct);
        }


        private Thread NeonFlowMonitorPollThread;
        private int NeonFlowMonitorPollPeriod = 1000;
        private bool NeonFlowMonitorFlag;
        private bool NeonFlowSetPointFlag;
        private Object NeonFlowMonitorLock;

        internal void StartNeonFlowMonitorPoll()
        {
            NeonFlowMonitorPollThread = new Thread(new ThreadStart(NeonFlowActMonitorPollWorker));
            NeonFlowMonitorPollThread.IsBackground = true; // When the application is closed, this thread will also immediately stop. This is lazy coding, but it works and shouldnn't cause any problems. This means it is a background thread of the main (UI) thread, so it will end with the main thread.
            NeonFlowMonitorPollPeriod = Int32.Parse(window.tbNeonFlowActPollPeriod.Text);
            window.EnableControl(window.btStartNeonFlowActMonitor, false);
            window.EnableControl(window.btStopNeonFlowActMonitor, true);
            window.EnableControl(window.tbNewNeonFlowSetPoint, true);
            window.EnableControl(window.btSetNewNeonFlowSetpoint, true);
            NeonFlowMonitorLock = new Object();
            NeonFlowMonitorFlag = false;
            NeonFlowSetPointFlag = false;
            NeonFlowMonitorPollThread.Start();
        }
        internal void StopNeonFlowMonitorPoll()
        {
            NeonFlowMonitorFlag = true;
        }
        private void NeonFlowActMonitorPollWorker()
        {
            for (; ; )// for (; ; ) is an infinite loop, equivalent to while(true)
            {
                Thread.Sleep(NeonFlowMonitorPollPeriod);
                lock (NeonFlowMonitorLock)
                {
                    UpdateNeonFlowActMonitor();
                    PlotLastNeonFlowAct();
                    UpdateNeonFlowSetpointMonitor();
                    if (NeonFlowSetPointFlag)
                    {
                        neonFlowController.SetSetpoint(neonFlowChannelNumber, newNeonFlowSetpoint.ToString());
                        NeonFlowSetPointFlag = false;
                    }
                    if (NeonFlowMonitorFlag)
                    {
                        NeonFlowMonitorFlag = false;
                        break;
                    }
                }
            }
            window.EnableControl(window.btStartNeonFlowActMonitor, true);
            window.EnableControl(window.btStopNeonFlowActMonitor, false);
            window.EnableControl(window.tbNewNeonFlowSetPoint, false);
            window.EnableControl(window.btSetNewNeonFlowSetpoint, false);
        }

        public void SetNeonFlowSetpoint()
        {
            if (Double.TryParse(window.tbNewNeonFlowSetPoint.Text, out newNeonFlowSetpoint))
            {
                if (newNeonFlowSetpoint <= neonFlowUpperLimit & neonFlowLowerLimit <= newNeonFlowSetpoint)
                {
                    NeonFlowSetPointFlag = true; // set flag that will trigger the setpoint to be changed in NeonFlowActMonitorPollWorker()
                }
                else MessageBox.Show("Setpoint request is outside of the MKS PR4000B flow range (" + neonFlowLowerLimit.ToString() + " - " + neonFlowUpperLimit.ToString() + " SCCM)", "User input exception", MessageBoxButtons.OK);
            }
            else MessageBox.Show("Unable to parse setpoint string. Ensure that a number has been written, with no additional non-numeric characters.", "", MessageBoxButtons.OK);
        }

        #endregion

        #region LakeShore 336
        public string[] PIDValueStringArray;
        public double[] PIDValueDoubleArray;
        public string[] PIDValueString = { "P", "I", "D" };
        public double[] PIDValueLowerLimits = { 0, 0, 0 };
        public double[] PIDValueUpperLimits = { 1000, 1000, 200 };
        public void QueryPIDLoopValues()
        {
            if (window.comboBoxLakeShore336OutputsQuery.Text == "1" | window.comboBoxLakeShore336OutputsQuery.Text == "2")
            {
                lock (LakeShore336Lock)
                {
                    receivedData = tempController.QueryPIDLoopValues(Int32.Parse(window.comboBoxLakeShore336OutputsQuery.Text));
                }
                PIDValueStringArray = receivedData.Split(',');
                window.SetTextBox(window.tbLakeShore336PIDPValueOutput, PIDValueStringArray[0]);
                window.SetTextBox(window.tbLakeShore336PIDIValueOutput, PIDValueStringArray[1]);
                window.SetTextBox(window.tbLakeShore336PIDDValueOutput, PIDValueStringArray[2]);
            }
            else
            {
                string message = "Please select output 1 or 2";
                string caption = "User input exception";
                MessageBox.Show(message, caption, MessageBoxButtons.OK);
            }
        }

        public void SetPIDLoopValues()
        {
            if (window.comboBoxLakeShore336OutputsSet.Text == "1" | window.comboBoxLakeShore336OutputsSet.Text == "2")
            {
                bool PIDValuesInvalid = ValidatePIDLoopValues();

                if (!PIDValuesInvalid)
                {
                    lock (LakeShore336Lock)
                    {
                        tempController.SetPIDLoopValues(Int32.Parse(window.comboBoxLakeShore336OutputsSet.Text), PIDValueDoubleArray[0], PIDValueDoubleArray[1], PIDValueDoubleArray[2]);
                    }
                }
            }
            else
            {
                string message = "Please select output 1 or 2";
                string caption = "User input exception";
                MessageBox.Show(message, caption, MessageBoxButtons.OK);
            }
        }

        internal bool ValidatePIDLoopValues()
        {
            PIDValueStringArray = new string[3];
            PIDValueDoubleArray = new double[3];
            PIDValueStringArray[0] = window.tbLakeShore336PIDPValueInput.Text;
            PIDValueStringArray[1] = window.tbLakeShore336PIDIValueInput.Text;
            PIDValueStringArray[2] = window.tbLakeShore336PIDDValueInput.Text;
            bool PIDValuesInvalid = false;

            for (int i = 0; i < 3; i++)
            {
                if (Double.TryParse(PIDValueStringArray[i], out PIDValueDoubleArray[i]))
                {
                    if (PIDValueDoubleArray[i] < PIDValueLowerLimits[i])
                    {
                        PIDValuesInvalid = true;
                        MessageBox.Show(PIDValueString[i] + " value less than minimum value.", "", MessageBoxButtons.OK);
                    }
                    if (PIDValueDoubleArray[i] > PIDValueUpperLimits[i])
                    {
                        PIDValuesInvalid = true;
                        MessageBox.Show(PIDValueString[i] + " value greater than maximum value.", "", MessageBoxButtons.OK);
                    }
                }
                else
                {
                    PIDValuesInvalid = true;
                    MessageBox.Show("Unable to parse " + PIDValueString[i] + " value string. Ensure that a number has been written, with no additional non-numeric characters.", "", MessageBoxButtons.OK);
                }
            }

            return PIDValuesInvalid;
        }


        // Autotune
        public int AutotuneOutput;
        public bool AutotuneOutputSelected = false;
        public int AutotuneMode;
        public bool AutotuneModeSelected = false;

        public void AutotuneLakeShore336TemperatureControl()
        {
            if (AutotuneOutputSelected)
            {
                if (AutotuneModeSelected)
                {
                    lock (LakeShore336Lock)
                    {
                        tempController.AutotuneOutput(AutotuneOutput, AutotuneMode);
                    }
                }
                else
                {
                    string message = "Please select autotune mode";
                    string caption = "User input exception";
                    MessageBox.Show(message, caption, MessageBoxButtons.OK);
                }
            }
            else
            {
                string message = "Please select autotune output";
                string caption = "User input exception";
                MessageBox.Show(message, caption, MessageBoxButtons.OK);
            }
        }

        public void AutotuneOutputSelectionChanged()
        {
            AutotuneOutputSelected = true;
            AutotuneOutput = Int32.Parse(window.comboBoxLakeShore336OutputsAutotune.Text);
        }

        public void AutotuneModeSelectionChanged()
        {
            AutotuneModeSelected = true;
            string AutotuneModeString = window.comboBoxLakeShore336AutotuneModes.Text;
            if (AutotuneModeString == "P")
            {
                AutotuneMode = 0;
            }
            else
            {
                if (AutotuneModeString == "P and I")
                {
                    AutotuneMode = 1;
                }
                else
                {
                    AutotuneMode = 2;
                }
            }
        }

        public string[] AutotuneStatusInfo;
        public string StatusOutput;
        public void QueryAutotuneStatus()
        {
            StatusOutput = "";
            string status;
            lock (LakeShore336Lock)
            {
                status = tempController.QueryControlTuningStatus();
            }
            AutotuneStatusInfo = status.Split(',');

            if (AutotuneStatusInfo[0] == "0")
            {
                StatusOutput += "No active tuning\n";
            }
            else
            {
                StatusOutput += "Active tuning\n";
                if (AutotuneStatusInfo[1] == "1")
                {
                    StatusOutput += "Tuning output 1\n";
                }
                else
                {
                    StatusOutput += "Tuning output 2\n";
                }
            }

            if (AutotuneStatusInfo[2] == "0")
            {
                StatusOutput += "No tuning error\nCurrent stage in autotune process: ";
                StatusOutput += AutotuneStatusInfo[2];
            }
            else
            {
                StatusOutput += "Tuning error\n";
                if (AutotuneStatusInfo[3] == "0")
                {
                    StatusOutput += "Initial conditions not met when starting the autotune procedure, causing the autotuning process to never actually begin.\n";
                }
            }

            window.SetRichTextBox(window.rtbAutotuneStatus, StatusOutput);
        }


        // Heaters
        private int LakeShoreCellOutput = 1;

        public void SetHeaterSetpoint(int Output, double Value)
        {
            if (Value > 0)
            {
                lock (LakeShore336Lock)
                {
                    tempController.SetControlSetpoint(Output, Value);
                }
            }
        }
        private void EnableLakeShoreHeaterOutput3or4(int Output, bool OnOff)
        {
            lock (LakeShore336Lock)
            {
                if (OnOff)
                {
                    tempController.SetHeaterRange(Output, 1); // 1 = on
                }
                else
                {
                    tempController.SetHeaterRange(Output, 0); // 0 = off
                }
            }
        }
        /// <summary>
        /// Sets the range parameter for a given output.
        /// LakeShore 336 outputs 1 and 2 are the PID loop controlled heaters (100 Watts and 50 Watts respectively).
        /// For outputs 1 and 2: 0 = Off, 1 = Low, 2 = Medium and 3= High.
        /// </summary>
        /// <param name="Output"></param>
        /// <param name="range"></param>
        public void EnableLakeShoreHeaterOutput1or2(int Output, int range)
        {
            lock (LakeShore336Lock)
            {
                if (range != 0)
                {
                    tempController.SetHeaterRange(Output, range); // 1 = Low, 2 = Medium and 3= High
                }
                else
                {
                    tempController.SetHeaterRange(Output, range); // 0 = Off
                }
            }
        }
        private bool HeatersEnabled;
        private void IsOutputEnabled(int Output)
        {
            string HeaterOutput;
            lock (LakeShore336Lock)
            {
                HeaterOutput = tempController.QueryHeaterRange(Output);
            }
            
            string trimResponse = HeaterOutput.Trim();// Trim in case there are unexpected white spaces.
            string status = trimResponse.Substring(0, 1); // Take the first character of the string.
            if (status == "1") HeatersEnabled = true; // Heater Output is on
            else HeatersEnabled = false; // Heater Output is off
        }

        #endregion

        #region Analogue/Digital IO tab

        public void SetDigitalOutput(string PortName, bool Enable)
        {
            SetDigitalLine(PortName, Enable);
        }
        public void EnableAnalogueInputsMonitoringUIControls(bool Enable)
        {
            window.EnableControl(window.btStartMonitoringAnalogueInputs, !Enable); // Disable start button
            window.EnableControl(window.btStopMonitoringAnalogueInputs, Enable); // Enable stop button
        }
        public void UpdateAIMonitorPollPeriod()
        {
            int AIMonitorPollPeriodParseValue;
            if (Int32.TryParse(window.tbAnalogueMonitoringPollPeriod.Text, out AIMonitorPollPeriodParseValue))
            {
                if (AIMonitorPollPeriodParseValue >= AIMonitorPollPeriodLowerLimit)
                {
                    AnalogueInputsMonitorPollPeriod = AIMonitorPollPeriodParseValue; // Update PT monitoring poll period
                }
                else MessageBox.Show("Poll period value too small. The analogue inputs can only be polled every " + AIMonitorPollPeriodLowerLimit.ToString() + " ms.", "User input exception", MessageBoxButtons.OK);
            }
            else MessageBox.Show("Unable to parse setpoint string. Ensure that an integer number has been written, with no additional non-numeric characters.", "", MessageBoxButtons.OK);
        }
        public void UpdateAIPlottingArrays(DateTime MeasurementDateTimeStamp)
        {
            lock (PTPlottingBufferLock)
            {
                CellTempPlottingArray[NumberOfPTMeasurementsInQueue] = lastCellTemp;
                S1TempPlottingArray[NumberOfPTMeasurementsInQueue] = lastS1Temp;
                S2TempPlottingArray[NumberOfPTMeasurementsInQueue] = lastS2Temp;
                NeonTempPlottingArray[NumberOfPTMeasurementsInQueue] = lastNeonTemp;
                SF6TempPlottingArray[NumberOfPTMeasurementsInQueue] = lastSF6Temp;
                SourcePressurePlottingArray[NumberOfPTMeasurementsInQueue] = lastSourcePressure;
                BeamlinePressurePlottingArray[NumberOfPTMeasurementsInQueue] = lastBeamlinePressure;
                DetectionPressurePlottingArray[NumberOfPTMeasurementsInQueue] = lastDetectionPressure;
                DateTimePlottingArray[NumberOfPTMeasurementsInQueue] = MeasurementDateTimeStamp;

                ++NumberOfPTMeasurementsInQueue; // Count the number of measurements added to the plotting queue
            }
        }
        public void MonitorAnalogueInputs()
        {
            double[] aiData;
            lock (HardwareControllerDAQCardLock) // Lock access to the DAQ card
            {
                aiData = hardwareControllerAIs.AIVoltages(); // Read (and average) data from AI ports
            } // Release lock as soon as possible so that other monitoring threads can access it without unnecessary delay
            DateTime dateTimeStamp = DateTime.Now;
            double AI11Measurement;
            double AI12Measurement;
            double AI13Measurement;
            double AI14Measurement;
            double AI15Measurement;

            lock (AIPlottingBufferLock)
            {
                // Split array of measurements into local variables
                AI11Measurement = aiData[0];
                AI12Measurement = aiData[1];
                AI13Measurement = aiData[2];
                AI14Measurement = aiData[3];
                AI15Measurement = aiData[4];
                
                // Record data in arrays for plotting in a different thread
                AI11[NumberOfAIMeasurementsInQueue] = AI11Measurement;
                AI12[NumberOfAIMeasurementsInQueue] = AI12Measurement;
                AI13[NumberOfAIMeasurementsInQueue] = AI13Measurement;
                AI14[NumberOfAIMeasurementsInQueue] = AI14Measurement;
                AI15[NumberOfAIMeasurementsInQueue] = AI15Measurement;
                //Console.Write(Convert.ToString(AI15[NumberOfAIMeasurementsInQueue]) + "\n");
                AIDateTimePlottingArray[NumberOfAIMeasurementsInQueue] = dateTimeStamp;

                ++NumberOfAIMeasurementsInQueue; // Increment the number of items in the plotting queue
                // Release lock so that plotting can access the data
            }

            // Update the UI monitor textboxes. This is done here so that if the plotting is lagging, the monitor textboxes are working at the same frequency as the poll period.
            int count = 0;
            // Append data to string (to be written to a CSV file when the user wishes to save the data)
            Double unixTimestamp = (Double)(dateTimeStamp.Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds);
            string csvLine = unixTimestamp + "," + DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss.fff tt"); // Initialize next line to be written to CSV data string

            foreach (bool Flag in AIConversionEnabled)
            {
                double ConvertedValue;
                if (Flag)
                {
                    if (AISeries[count] == "AI11")
                    {
                        ConvertedValue = AIConversionMethods.OhmMeterConversionMethod(AI11Measurement);
                        window.SetTextBox(window.tbMonitorAI11, ConvertedValue.ToString("E"));
                        csvLine += "," + ConvertedValue.ToString("E");
                    }
                    if (AISeries[count] == "AI12")
                    {
                        ConvertedValue = AIConversionMethods.OhmMeterConversionMethod(AI12Measurement);
                        window.SetTextBox(window.tbMonitorAI12, ConvertedValue.ToString("E"));
                        csvLine += "," + ConvertedValue.ToString("E");
                    }
                    if (AISeries[count] == "AI13")
                    {
                        ConvertedValue = AIConversionMethods.OhmMeterConversionMethod(AI13Measurement);
                        window.SetTextBox(window.tbMonitorAI13, ConvertedValue.ToString("E"));
                        csvLine += "," + ConvertedValue.ToString("E");
                    }
                    if (AISeries[count] == "AI14")
                    {
                        ConvertedValue = AIConversionMethods.OhmMeterConversionMethod(AI14Measurement);
                        window.SetTextBox(window.tbMonitorAI14, ConvertedValue.ToString("E"));
                        csvLine += "," + ConvertedValue.ToString("E");
                    }
                    if (AISeries[count] == "AI15")
                    {
                        ConvertedValue = AIConversionMethods.OhmMeterConversionMethod(AI15Measurement);
                        window.SetTextBox(window.tbMonitorAI15, ConvertedValue.ToString("E"));
                        csvLine += "," + ConvertedValue.ToString("E");
                    } // Yes this is very horrible, but multidimensional arrays are almost as bad in C#. Will try and make this more pretty later
                }
                else
                {
                    if (AISeries[count] == "AI11")
                    {
                        window.SetTextBox(window.tbMonitorAI11, AI11Measurement.ToString("E"));
                        csvLine += "," + AI11Measurement.ToString("E");
                    }
                    if (AISeries[count] == "AI12")
                    {
                        window.SetTextBox(window.tbMonitorAI12, AI12Measurement.ToString("E"));
                        csvLine += "," + AI12Measurement.ToString("E");
                    }
                    if (AISeries[count] == "AI13")
                    {
                        window.SetTextBox(window.tbMonitorAI13, AI13Measurement.ToString("E"));
                        csvLine += "," + AI13Measurement.ToString("E");
                    }
                    if (AISeries[count] == "AI14")
                    {
                        window.SetTextBox(window.tbMonitorAI14, AI14Measurement.ToString("E"));
                        csvLine += "," + AI14Measurement.ToString("E");
                    }
                    if (AISeries[count] == "AI15")
                    {
                        window.SetTextBox(window.tbMonitorAI15, AI15Measurement.ToString("E"));
                        csvLine += "," + AI15Measurement.ToString("E");
                    } // Yes this is very horrible, but multidimensional arrays are almost as bad in C#. Will try and make this more pretty later
                }
                ++count;
            }

            csvLine += "\r\n";
            lock (AICSVDataLock)
            {
                csvDataAnalogueInputs += csvLine;
            }
        }
        private void UpdateAIChartRollingTimeAxis()
        {
            DateTime xMin = DateTime.Now.AddMilliseconds(-AIChartRollingPeriod);
            window.SetChartXAxisMinDateTime(window.chart4, xMin);
        }
        public void EnableAIChartRollingTimeAxis(bool Enable)
        {
            if (Enable)
            {
                if (AIChartRollingPeriodSelected)
                {
                    AIChartRollingXAxis = true;
                }
                else
                {
                    MessageBox.Show("Please select analogue input chart rolling period.", "User input exception", MessageBoxButtons.OK);
                    window.SetCheckBox(window.cbEnableAnalogueInputsChartRollingTimeAxis, false);
                }
            }
            else
            {
                AIChartRollingXAxis = false;
                window.SetChartXAxisMinAuto(window.chart4);
            }
        }
        public void UpdateAIChartRollingPeriod()
        {
            int AIChartRollingPeriodParsedValue;
            if (Int32.TryParse(window.tbAnalogueInputsChartRollingAxisPeriod.Text, out AIChartRollingPeriodParsedValue))
            {
                if (AnalogueInputsMonitorPollPeriod <= AIChartRollingPeriodParsedValue)
                {
                    AIChartRollingPeriod = AIChartRollingPeriodParsedValue; // Update AI chart rolling period
                    AIChartRollingPeriodSelected = true;
                }
                else MessageBox.Show("Rolling period less than the polling period of the plot.", "User input exception", MessageBoxButtons.OK);
            }
            else MessageBox.Show("Unable to parse chart rolling period string. Ensure that an integer number has been written, with no additional non-numeric characters.", "", MessageBoxButtons.OK);
        }
        public void ClearAllAIChartSeries()
        {
            foreach (string SeriesName in AISeries) 
            {
                ClearChartSeriesData(window.chart4, SeriesName);
            }
            foreach (string SeriesName in AIConversionSeries)
            {
                ClearChartSeriesData(window.chart4, SeriesName);
            }
            ClearAnalogueInputsCSVData();
        }
        public void ClearAnalogueInputsCSVData()
        {
            lock (AICSVDataLock)
            {
                csvDataAnalogueInputs = "";
                SetAnalogueInputsCSVHeaderLine();
                ClearAIMonitorTextBoxes();
            }
        }
        public void ClearAIMonitorTextBoxes()
        {
            window.SetTextBox(window.tbMonitorAI11, "");
            window.SetTextBox(window.tbMonitorAI12, "");
            window.SetTextBox(window.tbMonitorAI13, "");
            window.SetTextBox(window.tbMonitorAI14, "");
            window.SetTextBox(window.tbMonitorAI15, "");
        }
        public void SetAnalogueInputsCSVHeaderLine()
        {
            if (csvDataAnalogueInputs == "") csvDataAnalogueInputs += "Unix Time Stamp (ms)" + "," + "Full date/time" + "," + "AI11 (arb.)" + "," + "AI12 (arb.)" + "," + "AI13 (arb.)" + "," + "AI14 (arb.)" + "," + "AI15 (arb.)" + "," + "\r\n"; // Header lines for csv file
        }
        public void SaveAnalogueInputsDataToCSV()
        {
            lock (AICSVDataLock)
            {
                SavePlotDataToCSV(csvDataAnalogueInputs);
            }
        }

        public class AIConversionMethods
        {
            public static double OhmMeterConversionMethod(double Voltage)
            {
                double Vin = 4.9793; // Measured using DAQ AI12 with Ohm meter resistors connected
                double KnownResistor = 6839.5; // Calculated using a fit. This less precisely known resistor (6.8 kOhms) was calibrated against a better known resistor (100.0 Ohms) by varying (double) KnownResistor and measuring (double) outputResistance below.

                double outputResistance = KnownResistor * ((Vin / Voltage) - 1);

                return outputResistance; 
            }
            public static double[] OhmMeterSeriesConversion(double[] Voltages)
            {
                int ArrayLength = Voltages.Length;
                double[] OutputArray = new double[ArrayLength];
                int count = 0;
                foreach (double Voltage in Voltages)
                {
                    OutputArray[count] = OhmMeterConversionMethod(Voltage);
                    ++count;
                }
                return OutputArray;
            }
        }

        public void EnableConvertedAISeries(string AnalogueInputName, string AIConversionMethodName)
        {
            // Setup thread for conversion to be run on.
            // This prevents the UI from locking up if the AIChartSeriesLock is already taken by the AI plotting thread.
            Thread ConvertAnalogueInput = new Thread(() => EnableConvertedAISeriesWorker(AnalogueInputName, AIConversionMethodName));
            ConvertAnalogueInput.IsBackground = true; // When the application is closed, this thread will also immediately stop. This is lazy coding, but it works and shouldnn't cause any problems. This means it is a background thread of the main (UI) thread, so it will end with the main thread.
            ConvertAnalogueInput.Start();
        }
        public void EnableConvertedAISeriesWorker(string AnalogueInputName, string AIConversionMethodName)
        {
            SetAIConversionStatusTextBox("");
            
            int ConversionMethodIndex = Array.IndexOf(AIConversionMethodsNames, AIConversionMethodName);
            string Units = AIConversionUnits[ConversionMethodIndex];

            int AISeriesNameIndex = Array.IndexOf(AISeries, AnalogueInputName);
            string AIConvertedSeriesName = AIConversionSeries[AISeriesNameIndex];

            // Lock the chart series so that the plotting thread doesn't add any points whilst this operation is being performed.
            // Note that the monitoring thread can continue to read the analogue inputs whilst this conversion is taking place.
            lock (AIChartSeriesLock)
            {
                Console.Write(Convert.ToString(DateTime.Now) + "\n");
                if (AIConversionMethodName != "None") // If the user is converting to raw voltage to another quantity
                {
                    double[] OutputArray;
                    ClearChartSeriesData(window.chart4, AIConvertedSeriesName); // Clear the series data. If switching between conversions, this resets the series before the new data are added.

                    // Get current datapoints from analogue inputs chart series
                    AppendAIConversionStatusTextBox("Collecting raw data.");
                    Tuple<double[], DateTime[]> RawAISeriesData = GetRawAISeriesData(AnalogueInputName); // Returns the raw voltages and datetimes in a tuple format
                    double[] RawVoltages = RawAISeriesData.Item1; // Raw voltages to be converted
                    DateTime[] AISeriesDateTimes = RawAISeriesData.Item2; // DateTimes associated with the raw voltages

                    if (AIConversionMethodName == "Ohm Meter")
                    {
                        AppendAIConversionStatusTextBox("Applying conversion method to raw voltages.");
                        OutputArray = AIConversionMethods.OhmMeterSeriesConversion(RawVoltages);
                        AppendAIConversionStatusTextBox("Adding data to plot.");
                        AddArrayOfPointsToChart(window.chart4, AIConvertedSeriesName, AISeriesDateTimes, OutputArray);
                        EnableChartSeries(window.chart4, AnalogueInputName, false);
                        EnableChartSeries(window.chart4, AIConvertedSeriesName, CheckIfAISeriesEnabled(AnalogueInputName));
                        AIConversionEnabled[AISeriesNameIndex] = true;
                        CurrentAIConversionMethods[AISeriesNameIndex] = AIConversionMethodName;
                        AppendAIConversionStatusTextBox("Finished.");
                    }

                }
                else // I.e. if user has reverted back to raw voltage
                {
                    EnableChartSeries(window.chart4, AnalogueInputName, CheckIfAISeriesEnabled(AnalogueInputName)); // Enable the raw voltage data series if checkbox is checked (show on chart)
                    EnableChartSeries(window.chart4, AIConvertedSeriesName, false); // Disable the converted data series (hide on chart)
                    ClearChartSeriesData(window.chart4, AIConvertedSeriesName); // Clear the converted data series
                    AIConversionEnabled[AISeriesNameIndex] = false; // Reset flag
                    CurrentAIConversionMethods[AISeriesNameIndex] = AIConversionMethodName; // Reset conversion name to "None"
                }
            }

            SetAIUnits(AnalogueInputName, Units); // Set analogue input units
        }
        private Tuple<double[], DateTime[]> GetRawAISeriesData(string AnalogueInputSeriesName)
        {
            double[] AIYValues;
            DateTime[] AIXValues;
           
            int pointsCount = window.chart4.Series[AnalogueInputSeriesName].Points.Count; // Number of points in the series
            AIYValues = new double[pointsCount]; 
            AIXValues = new DateTime[pointsCount];

            for (int i = 0; i < pointsCount; ++i) // Annoyingly, there doesn't seem to be a way to simply get all of the YValues using one function. One must loop over the datapoints individually and extract the information that you want.
            {
                var point = window.chart4.Series[AnalogueInputSeriesName].Points[i]; // Get next datapoint
                AIYValues[i] = point.YValues[0]; // Get datapoint YValue
                AIXValues[i] = DateTime.FromOADate(point.XValue); // point.XValue assumed (by the computer) to be a double. FromOADate() is used because we know better than the computer!
            }

            return Tuple.Create(AIYValues, AIXValues); // Tuple is used to pair related values without having to define a new type
        }
        private void SetAIUnits(string AnalogueInputName, string Units)
        {
            if (AnalogueInputName == "AI11")
            {
                window.SetTextBox(window.tbAI11Units, Units);
            }
            else
            {
                if (AnalogueInputName == "AI12")
                {
                    window.SetTextBox(window.tbAI12Units, Units);
                }
                else
                {
                    if (AnalogueInputName == "AI13")
                    {
                        window.SetTextBox(window.tbAI13Units, Units);
                    }
                    else
                    {
                        if (AnalogueInputName == "AI14")
                        {
                            window.SetTextBox(window.tbAI14Units, Units);
                        }
                        else
                        {
                            if (AnalogueInputName == "AI15")
                            {
                                window.SetTextBox(window.tbAI15Units, Units);
                            }
                        }
                    }
                }
            }
        }
        private void AddConvertedAIPointsToChart(string AnalogueInputSeriesName, double[] RawVoltages, DateTime[] DateTimeStamps)
        {
            int AISeriesNameIndex = Array.IndexOf(AISeries, AnalogueInputSeriesName);
            string AIConvertedSeriesName = AIConversionSeries[AISeriesNameIndex];
            string ConversionMethod = CurrentAIConversionMethods[AISeriesNameIndex];
            double[] ConvertedValues;

            if (ConversionMethod == "Ohm Meter")
            {
                ConvertedValues = AIConversionMethods.OhmMeterSeriesConversion(RawVoltages);
                AddArrayOfPointsToChart(window.chart4, AIConvertedSeriesName, DateTimeStamps, ConvertedValues);
            }
        }
        private bool CheckIfAISeriesEnabled(string AnalogueInputSeriesName)
        {
            if (AnalogueInputSeriesName == "AI11")
            {
                if (window.cbPlotAnalogueInputAI11.Checked) return true;
                else return false;
            }
            if (AnalogueInputSeriesName == "AI12")
            {
                if (window.cbPlotAnalogueInputAI12.Checked) return true;
                else return false;
            }
            if (AnalogueInputSeriesName == "AI13")
            {
                if (window.cbPlotAnalogueInputAI13.Checked) return true;
                else return false;
            }
            if (AnalogueInputSeriesName == "AI14")
            {
                if (window.cbPlotAnalogueInputAI14.Checked) return true;
                else return false;
            }
            if (AnalogueInputSeriesName == "AI15")
            {
                if (window.cbPlotAnalogueInputAI15.Checked) return true;
                else return false;
            }
            else return false;
        }
        public void AppendAIConversionStatusTextBox(string StatusUpdate)
        {
            window.AppendTextBox(window.tbAIConversionStatus, StatusUpdate + Environment.NewLine);
        }
        public void SetAIConversionStatusTextBox(string StatusUpdate)
        {
            window.SetTextBox(window.tbAIConversionStatus, StatusUpdate);
        }
        
        private Thread AnalogueInputsMonitorPollThread;
        private Thread AnalogueInputsPlottingThread;
        private int AnalogueInputsMonitorPollPeriod = 1000;
        private int AIMonitorPollPeriodLowerLimit = 50;
        private bool AnalogueInputsMonitorFlag;
        private bool AnalogueInputsPlottingFlag;
        private Object AnalogueInputsMonitorLock;
        private string[] AISeries = { "AI11", "AI12", "AI13", "AI14", "AI15" }; // Names of series used in analogue inputs chart
        private string[] AIConversionSeries = { "AI11 Converted", "AI12 Converted", "AI13 Converted", "AI14 Converted", "AI15 Converted" }; // Names of converted series used in analogue inputs chart
        private bool[] AIConversionEnabled = { false, false, false, false, false}; // Flags to indicate whether or not the user has requested that an analogue input be converted using some pre-defined method
        private string[] CurrentAIConversionMethods = { "None", "None", "None", "None", "None"};
        private string[] AIConversionMethodsNames = { "None", "Ohm Meter" }; // Names of methods that can be applied to the AI data
        private string[] AIConversionUnits = { "(V)", "(Ohms)"}; // Units of converted values
        public string csvDataAnalogueInputs = "";
        
        // Plotting variables
        private readonly object AIPlottingBufferLock = new object();
        private readonly object AIChartSeriesLock = new object();
        private readonly object AICSVDataLock = new object();
        static int MaxAIPlottingArrayLength = 10000;
        public int NumberOfAIMeasurementsInQueue = 0;
        private int AIChartRollingPeriod;
        private bool AIChartRollingPeriodSelected = false;
        private bool AIChartRollingXAxis = false;
        // Arrays for analogue input measurements
        public double[] AI11 = new double[MaxAIPlottingArrayLength];
        public double[] AI12 = new double[MaxAIPlottingArrayLength];
        public double[] AI13 = new double[MaxAIPlottingArrayLength];
        public double[] AI14 = new double[MaxAIPlottingArrayLength];
        public double[] AI15 = new double[MaxAIPlottingArrayLength];
        public DateTime[] AIDateTimePlottingArray = new DateTime[MaxAIPlottingArrayLength];
        // Buffer arrays for plotting
        public double[] AI11Buffer;
        public double[] AI12Buffer;
        public double[] AI13Buffer;
        public double[] AI14Buffer;
        public double[] AI15Buffer;
        public DateTime[] AIDateTimePlottingArrayBuffer;

        internal void StartAnalogueInputsMonitorPoll()
        {
            // Setup monitoring and plotting threads
            AnalogueInputsMonitorPollThread = new Thread(new ThreadStart(AnalogueInputsMonitorPollWorker));
            AnalogueInputsMonitorPollThread.IsBackground = true; // When the application is closed, this thread will also immediately stop. This is lazy coding, but it works and shouldnn't cause any problems. This means it is a background thread of the main (UI) thread, so it will end with the main thread.
            AnalogueInputsPlottingThread = new Thread(new ThreadStart(AnalogueInputsPlottingWorker));
            AnalogueInputsPlottingThread.IsBackground = true; // When the application is closed, this thread will also immediately stop. This is lazy coding, but it works and shouldnn't cause any problems. This means it is a background thread of the main (UI) thread, so it will end with the main thread.
            
            AnalogueInputsMonitorPollPeriod = Int32.Parse(window.tbAnalogueMonitoringPollPeriod.Text);
            EnableAnalogueInputsMonitoringUIControls(true); // Enable/disable UI elements that the user should/shouldn't interact with whilst this process in running
            AnalogueInputsMonitorLock = new Object();
            AnalogueInputsMonitorFlag = false;
            AnalogueInputsPlottingFlag = false;
            SetAnalogueInputsCSVHeaderLine();
            
            // Start monitoring and plotting threads
            AnalogueInputsMonitorPollThread.Start();
            AnalogueInputsPlottingThread.Start();
        }
        internal void StopAnalogueInputsMonitorPoll()
        {
            

            UEDMSavePlotDataDialog saveAIDataDialog = new UEDMSavePlotDataDialog("Save data message", "Would you like to save the analogue inputs data now? \n\nThe data will not be cleared.");
            saveAIDataDialog.ShowDialog();
            if (saveAIDataDialog.DialogResult != DialogResult.Cancel) // If the user chooses to cancel the action of stopping AI monitoring, then don't perform any of these actions
            {
                AnalogueInputsMonitorFlag = true;
                AnalogueInputsPlottingFlag = true;

                if (saveAIDataDialog.DialogResult == DialogResult.Yes)
                {
                    lock (AICSVDataLock)
                    {
                        SavePlotDataToCSV(csvDataAnalogueInputs);
                    }
                }
            }
            saveAIDataDialog.Dispose();
        }
        private void AnalogueInputsMonitorPollWorker()
        {
            int count = 0;

            for (; ; )// for (; ; ) is an infinite loop, equivalent to while(true)
            {
                var watch = System.Diagnostics.Stopwatch.StartNew();  // Use stopwatch to track how long it takes to measure values. This is subtracted from the poll period so that the loop is executed at the proper frequency.
                ++count;
                lock (AnalogueInputsMonitorLock)
                {
                    // Measure AI ports and record data in plotting queue - the data will be plotted in a different thread.
                    MonitorAnalogueInputs();

                    if (AnalogueInputsMonitorFlag)
                    {
                        AnalogueInputsMonitorFlag = false;
                        break;
                    }
                }

                // Calculate and subtract from the poll period the amount of time taken to perform the contents of this loop (so that the AI ports are polled at the correct frequency)
                watch.Stop(); // Stop the stopwatch that was started at the start of the for loop
                int ThreadWaitPeriod = AnalogueInputsMonitorPollPeriod - Convert.ToInt32(watch.ElapsedMilliseconds); // Subtract the time elapsed from the user defined poll period
                if (ThreadWaitPeriod < 0) ThreadWaitPeriod = 0; // If the result of the above subtraction was negative, set the value to zero so that Thread.Sleep() doesn't throw an exception
                Thread.Sleep(ThreadWaitPeriod); // Wait until the next AI measurements are to be made
            }
            EnableAnalogueInputsMonitoringUIControls(false);
        }
        private void AnalogueInputsPlottingWorker()
        {
            int PlottingQueueLength = 0;
            for (; ; )
            {
                Thread.Sleep(AnalogueInputsMonitorPollPeriod); // Wait until more data is taken

                // Plot AI data
                lock (AIChartSeriesLock) // If the series data is currently being converted, then lock the series
                {
                    lock (AIPlottingBufferLock) // Use lock to prevent new measurements being added to the plotting queue whilst the plotting function is in operation.
                    {
                        PlottingQueueLength = NumberOfAIMeasurementsInQueue;
                        NumberOfAIMeasurementsInQueue = 0; // Reset the number of items in the queue to zero
                        // Create buffer arrays that can store the data locally - allowing AI measurements to continue unimpeded.
                        AI11Buffer = new double[PlottingQueueLength];
                        AI12Buffer = new double[PlottingQueueLength];
                        AI13Buffer = new double[PlottingQueueLength];
                        AI14Buffer = new double[PlottingQueueLength];
                        AI15Buffer = new double[PlottingQueueLength];
                        AIDateTimePlottingArrayBuffer = new DateTime[PlottingQueueLength];

                        if (PlottingQueueLength > 0) // If no data has been recorded, then there is no need to perform this function.
                        {
                            // Copy the data to buffer arrays so that the lock can be released and the monitoring thread can continue to make measurements of the AI ports
                            long sourceIndex = 0; // Starting index of the data to be copied from the source array
                            long destinationIndex = 0; // Starting index of where the data will be stored in the destination (buffer) array
                            long lengthOfDataToCopy = Convert.ToInt64(PlottingQueueLength); // Number of measurements saved to the plotting queue (i.e. how many elements of the data arrays are to be copied to the buffer arrays)
                            Array.Copy(AI11, sourceIndex, AI11Buffer, destinationIndex, lengthOfDataToCopy);
                            Array.Copy(AI12, sourceIndex, AI12Buffer, destinationIndex, lengthOfDataToCopy);
                            Array.Copy(AI13, sourceIndex, AI13Buffer, destinationIndex, lengthOfDataToCopy);
                            Array.Copy(AI14, sourceIndex, AI14Buffer, destinationIndex, lengthOfDataToCopy);
                            Array.Copy(AI15, sourceIndex, AI15Buffer, destinationIndex, lengthOfDataToCopy);
                            Array.Copy(AIDateTimePlottingArray, sourceIndex, AIDateTimePlottingArrayBuffer, destinationIndex, lengthOfDataToCopy);

                            // Clear the data plotting arrays and release the lock so that measurements can continue whilst plotting is performed.
                            int sourceClearIndex = Convert.ToInt32(sourceIndex);
                            int lengthOfDataToClear = Convert.ToInt32(lengthOfDataToCopy);
                            Array.Clear(AI11, sourceClearIndex, lengthOfDataToClear);
                            Array.Clear(AI12, sourceClearIndex, lengthOfDataToClear);
                            Array.Clear(AI13, sourceClearIndex, lengthOfDataToClear);
                            Array.Clear(AI14, sourceClearIndex, lengthOfDataToClear);
                            Array.Clear(AI15, sourceClearIndex, lengthOfDataToClear);
                            Array.Clear(AIDateTimePlottingArray, sourceClearIndex, lengthOfDataToClear);
                        }
                    }

                    if (PlottingQueueLength > 0)
                    {

                        AddArrayOfPointsToChart(window.chart4, AISeries[0], AIDateTimePlottingArrayBuffer, AI11Buffer);
                        AddArrayOfPointsToChart(window.chart4, AISeries[1], AIDateTimePlottingArrayBuffer, AI12Buffer);
                        AddArrayOfPointsToChart(window.chart4, AISeries[2], AIDateTimePlottingArrayBuffer, AI13Buffer);
                        AddArrayOfPointsToChart(window.chart4, AISeries[3], AIDateTimePlottingArrayBuffer, AI14Buffer);
                        AddArrayOfPointsToChart(window.chart4, AISeries[4], AIDateTimePlottingArrayBuffer, AI15Buffer);

                        int count = 0;
                        foreach (bool Flag in AIConversionEnabled)
                        {
                            if (Flag)
                            {
                                if (AISeries[count] == "AI11")
                                {
                                    AddConvertedAIPointsToChart(AISeries[count], AI11Buffer, AIDateTimePlottingArrayBuffer);
                                }
                                if (AISeries[count] == "AI12")
                                {
                                    AddConvertedAIPointsToChart(AISeries[count], AI12Buffer, AIDateTimePlottingArrayBuffer);
                                }
                                if (AISeries[count] == "AI13")
                                {
                                    AddConvertedAIPointsToChart(AISeries[count], AI13Buffer, AIDateTimePlottingArrayBuffer);
                                }
                                if (AISeries[count] == "AI14")
                                {
                                    AddConvertedAIPointsToChart(AISeries[count], AI14Buffer, AIDateTimePlottingArrayBuffer);
                                }
                                if (AISeries[count] == "AI15")
                                {
                                    AddConvertedAIPointsToChart(AISeries[count], AI15Buffer, AIDateTimePlottingArrayBuffer);
                                } // Yes this is very horrible, but multidimensional arrays are almost as bad in C#. Will try and make this more pretty later
                            }
                            ++count;
                        }

                        // Update chart
                        if (AIChartRollingXAxis)
                        {
                            UpdateAIChartRollingTimeAxis();
                        }
                    }
                }

                PlottingQueueLength = 0; // Reset variable

                if (AnalogueInputsPlottingFlag) // If the user has requested that monitoring/plotting of temperature/pressure be stopped, then break this loop
                {
                    AnalogueInputsPlottingFlag = false;
                    break;
                }
            }
        }

        #endregion

        #region E field

        public double LeakageMonitorMeasurementTime
        {
            set
            {
                window.SetTextBox(window.IMonitorMeasurementLengthTextBox, value.ToString());
            }
            get
            {
                return Double.Parse(window.IMonitorMeasurementLengthTextBox.Text);
            }
        }

        public void SetLeakageMonitorMeasurementTime(double time)
        {
            window.SetTextBox(window.IMonitorMeasurementLengthTextBox, time.ToString());
        }

        public double CPlusMonitorVoltage
        {
            get
            {
                return cPlusMonitorVoltage;
            }
        }

        public double CMinusMonitorVoltage
        {
            get
            {
                return cMinusMonitorVoltage;
            }
        }
        public double NorthCurrent
        {
            get
            {
                return lastNorthCurrent;
            }
        }

        public double SouthCurrent
        {
            get
            {
                return lastSouthCurrent;
            }
        }

        public double LastNorthCurrent
        {
            get { return westLeakageMonitor.GetCurrent(); }
        }

        public double LastSouthCurrent
        {
            get { return eastLeakageMonitor.GetCurrent(); }
        }

        private double cPlusMonitorVoltage;
        private double cMinusMonitorVoltage;
        private double lastNorthCurrent;
        private double lastSouthCurrent;
        private double lastWestFrequency;
        private double lastEastFrequency;
        private Queue<double> nCurrentSamples = new Queue<double>();
        private Queue<double> sCurrentSamples = new Queue<double>();
        private int movingAverageSampleLength = 10;

        public void PollVMonitor()
        {
            /*window.SetTextBox(window.cPlusVMonitorTextBox, 
                (cScale * voltageController.ReadInputVoltage(cPlusChan)).ToString());
            window.SetTextBox(window.cMinusVMonitorTextBox, 
                (cScale * voltageController.ReadInputVoltage(cMinusChan)).ToString());
            window.SetTextBox(window.gPlusVMonitorTextBox, 
                (gScale * voltageController.ReadInputVoltage(gPlusChan)).ToString());
            window.SetTextBox(window.gMinusVMonitorTextBox, 
                (gScale * voltageController.ReadInputVoltage(gMinusChan)).ToString());*/

            double cMonScale = 3000;//This converts the reading from the 1:10 V output to the full 30 kV range of the spellman PS (in volts)
            cPlusMonitorVoltage = cMonScale * ReadAnalogInput(cPlusMonitorInputTask);
            cMinusMonitorVoltage = cMonScale * ReadAnalogInput(cMinusMonitorInputTask);
        }
        public void UpdateVMonitorUI()
        {
            PollVMonitor();
            window.SetTextBox(window.cPlusVMonitorTextBox, CPlusMonitorVoltage.ToString());
            window.SetTextBox(window.cMinusVMonitorTextBox, CMinusMonitorVoltage.ToString());
        }

        public void ReconfigureIMonitors()
        {
            currentMonitorMeasurementTime = Double.Parse(window.IMonitorMeasurementLengthTextBox.Text);
            westFreq2AmpSlope = Double.Parse(window.leakageMonitorSlopeTextBox.Text);
            eastFreq2AmpSlope = Double.Parse(window.leakageMonitorSlopeTextBox.Text);
            westVolt2FreqSlope = Double.Parse(window.northV2FSlopeTextBox.Text);
            eastVolt2FreqSlope = Double.Parse(window.southV2FSlopeTextBox.Text);

            eastLeakageMonitor.MeasurementTime = currentMonitorMeasurementTime;
            westLeakageMonitor.MeasurementTime = currentMonitorMeasurementTime;
            westLeakageMonitor.F2ISlope = westFreq2AmpSlope;
            eastLeakageMonitor.F2ISlope = eastFreq2AmpSlope;
            westLeakageMonitor.V2FSlope = westVolt2FreqSlope;
            eastLeakageMonitor.V2FSlope = eastVolt2FreqSlope;
        }

        public void ReadIMonitor()
        {
            //double ground = ReadAnalogInput(groundedInputTask);
            lastNorthCurrent = westLeakageMonitor.GetCurrent();
            //ground = ReadAnalogInput(groundedInputTask);
            lastSouthCurrent = eastLeakageMonitor.GetCurrent();
        }

        private string currentSeriesEast = "Leakage Current East";
        private string currentSeriesWest = "Leakage Current West";
        private DateTime localDate;
       
        public void UpdateIMonitor()
        {
            ReconfigureIMonitors();

            //sample the leakage current
            //lastNorthCurrent = westLeakageMonitor.GetCurrent();
            //lastSouthCurrent = eastLeakageMonitor.GetCurrent();


            //This samples the frequency
            lastWestFrequency = westLeakageMonitor.getRawCount();
            lastEastFrequency = eastLeakageMonitor.getRawCount();

            lastNorthCurrent = ((lastWestFrequency - westOffset) / westVolt2FreqSlope);
            lastSouthCurrent = ((lastEastFrequency - eastOffset) / eastVolt2FreqSlope);

            //plot the most recent samples
            //window.PlotYAppend(window.leakageGraph, window.northLeakagePlot,
            //            new double[] { lastNorthCurrent });
            //window.PlotYAppend(window.leakageGraph, window.southLeakagePlot,
            //                        new double[] { lastSouthCurrent });

            //add date time
            localDate = DateTime.Now;
            //plot the most recent sample (UEDM Chart style)

            //window.chart5.Series[currentSeriesEast].Points.AddXY(localDate, lastSouthCurrent);
            //window.chart5.Series[currentSeriesWest].Points.AddXY(localDate, lastNorthCurrent);
            window.AddPointToIChart(window.chart5, currentSeriesEast, localDate, lastSouthCurrent);
            window.AddPointToIChart(window.chart5, currentSeriesWest, localDate, lastNorthCurrent);

            //add samples to Queues for averaging
            nCurrentSamples.Enqueue(lastNorthCurrent);
            sCurrentSamples.Enqueue(lastSouthCurrent);

            //drop samples when array is larger than the moving average sample length
            while (nCurrentSamples.Count > movingAverageSampleLength)
            {
                nCurrentSamples.Dequeue();
                sCurrentSamples.Dequeue();
            }

            //average samples
            double nAvCurr = nCurrentSamples.Average();
            double sAvCurr = sCurrentSamples.Average();
            double nAvCurrErr = Math.Sqrt((nCurrentSamples.Sum(d => Math.Pow(d - nAvCurr, 2))) / (nCurrentSamples.Count() - 1)) / (Math.Sqrt(nCurrentSamples.Count()));
            double sAvCurrErr = Math.Sqrt((sCurrentSamples.Sum(d => Math.Pow(d - sAvCurr, 2))) / (sCurrentSamples.Count() - 1)) / (Math.Sqrt(sCurrentSamples.Count()));

            //update text boxes
            window.SetTextBox(window.northIMonitorTextBox, (nAvCurr).ToString());
            window.SetTextBox(window.northIMonitorErrorTextBox, (nAvCurrErr).ToString());
            window.SetTextBox(window.southIMonitorTextBox, (sAvCurr).ToString());
            window.SetTextBox(window.southIMonitorErrorTextBox, (sAvCurrErr).ToString());
        }

        public void ClearIMonitorAv()
        {
            nCurrentSamples.Clear();
            sCurrentSamples.Clear();

        }

        public void ClearIMonitorChart()
        {
            ClearChartSeriesData(window.chart5, currentSeriesEast);
            ClearChartSeriesData(window.chart5, currentSeriesWest);
            
        }

        public void RescaleIMonitorChart()
        {
            window.chart5.ChartAreas[0].AxisY.Minimum = -20;
            window.SetChartYAxisAuto(window.chart5);
            //window.setaxisyisstartedfromzero(window.chart5, false);
        }

        public void CalibrateIMonitors()
        {
            ReconfigureIMonitors();

            eastLeakageMonitor.SetZero();
            westLeakageMonitor.SetZero();

            westOffset = westLeakageMonitor.Offset;
            eastOffset = eastLeakageMonitor.Offset;

            window.SetTextBox(window.southOffsetIMonitorTextBox, eastOffset.ToString());
            window.SetTextBox(window.northOffsetIMonitorTextBox, westOffset.ToString());
        }

        private Thread iMonitorPollThread;
        private int iMonitorPollPeriod = 200;
        private Object iMonitorLock;
        private bool iMonitorFlag;
        internal void StartIMonitorPoll()
        {
            iMonitorPollThread = new Thread(new ThreadStart(IMonitorPollWorker));
            window.EnableControl(window.startIMonitorPollButton, false);
            window.EnableControl(window.updateIMonitorButton, false);
            window.EnableControl(window.stopIMonitorPollButton, true);
            iMonitorPollPeriod = Int32.Parse(window.iMonitorPollPeriod.Text);
            movingAverageSampleLength = Int32.Parse(window.currentMonitorSampleLengthTextBox.Text);
            nCurrentSamples.Clear();
            sCurrentSamples.Clear();
            iMonitorLock = new Object();
            iMonitorFlag = false;
            iMonitorPollThread.Start();
        }

        internal void StopIMonitorPoll()
        {
            iMonitorFlag = true;
            window.EnableControl(window.updateIMonitorButton, true);
        }
        private void IMonitorPollWorker()
        {
            for (; ; )
            {
                Thread.Sleep(iMonitorPollPeriod);
                lock (iMonitorLock)
                {
                    UpdateIMonitor();
                    UpdateVMonitorUI();
                    if (window.logCurrentDataCheckBox.Checked)
                    {
                        string folder = @" C:\Users\ultraedm\Desktop\Leakage_Current_Tests\";
                        string fileName = "Plate_Test_East_neg_West_pos_210602_01.csv";
                        string fullPath = folder + fileName;
                        StreamWriter w;
                        w = new StreamWriter(fullPath, true);
                        string output = String.Format("West, {0,5:N2}, {1,7:0.00}, East, {2,5:N2}, {3,7:0.00}, {4,8:T}, {4,8:D}, Plus, {5,5:N3}, Minus, {6,5:N3}",
                          lastNorthCurrent, lastWestFrequency, lastSouthCurrent, lastEastFrequency, localDate, cPlusMonitorVoltage, cMinusMonitorVoltage);
                        w.WriteLine(output);
                        w.Close();                    
                    }
                    if (iMonitorFlag)
                    {
                        iMonitorFlag = false;
                        break;
                    }
                }
            }
            window.EnableControl(window.startIMonitorPollButton, true);
            window.EnableControl(window.stopIMonitorPollButton, false);
        }
        #endregion
    }
}
