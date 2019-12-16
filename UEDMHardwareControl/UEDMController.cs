using System;
using System.Timers;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Runtime.Remoting.Lifetime;
using System.Windows.Forms;
using NationalInstruments;
using NationalInstruments.DAQmx;
using NationalInstruments.VisaNS;
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
        # region Setup

        // hardware
        private static string[] Names = { "Cell Temperature Monitor", "S1 Temperature Monitor", "S2 Temperature Monitor", "SF6 Temperature Monitor" };
        private static string[] ChannelNames = { "cellTemperatureMonitor", "S1TemperatureMonitor", "S2TemperatureMonitor", "SF6TemperatureMonitor" };

        LakeShore336TemperatureController tempController = (LakeShore336TemperatureController)Environs.Hardware.Instruments["tempController"];
        AgilentFRG720Gauge sourcePressureMonitor = new AgilentFRG720Gauge("Pressure gauge source", "pressureGauge_source");
        AgilentFRG720Gauge beamlinePressureMonitor = new AgilentFRG720Gauge("Pressure gauge beamline", "pressureGauge_beamline");
        SiliconDiodeTemperatureMonitors tempMonitors = new SiliconDiodeTemperatureMonitors(Names, ChannelNames);

        FlowControllerMKSPR4000B neonFlowController = (FlowControllerMKSPR4000B)Environs.Hardware.Instruments["neonFlowController"];

        Hashtable digitalTasks = new Hashtable();
        Task cryoTriggerDigitalOutputTask;
        Task heatersS2TriggerDigitalOutputTask;
        Task heatersS1TriggerDigitalOutputTask;

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
            // make the digital tasks
            CreateDigitalTask("cryoTriggerDigitalOutputTask");
            CreateDigitalTask("heatersS2TriggerDigitalOutputTask");
            CreateDigitalTask("heatersS1TriggerDigitalOutputTask");

            // digitial input tasks

            // analog outputs
            //bBoxAnalogOutputTask = CreateAnalogOutputTask("bScan");

            // analog inputs
            //probeMonitorInputTask = CreateAnalogInputTask("probePD", 0, 5);



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
        }

        public void WindowClosing()
        {
            // Request that the PT monitoring thread stop
            StopPTMonitorPoll();
        }

        private void CreateDigitalTask(String name)
        {
            Task digitalTask = new Task(name);
            ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels[name]).AddToTask(digitalTask);
            digitalTask.Control(TaskAction.Verify);
            digitalTasks.Add(name, digitalTask);
        }

        private void SetDigitalLine(string name, bool value)
        {
            Task digitalTask = ((Task)digitalTasks[name]);
            DigitalSingleChannelWriter writer = new DigitalSingleChannelWriter(digitalTask.Stream);
            writer.WriteSingleSampleSingleLine(true, value);
            digitalTask.Control(TaskAction.Unreserve);
        }


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


        public string[] csvData;
        public void SavePlotDataToCSV(string csvContent)// to be created
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
        private string Off = "0"; // I.e. connected in normally open state (NO)
        private string On = "1"; // Swap these if the wiring is changed to normally closed (NC)

        public void EnableCryoDigitalControl(bool enable) // Temporary function - used to control a digital output which connects to the cryo (true = on, false = off)
        {
            SetDigitalLine("cryoTriggerDigitalOutputTask", enable);
        }

        public void InitializeCryoControl()
        {
            string status = tempController.QueryRelayStatus(RelayNumber); // Query the status of the relay.
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

        public string PollCryoStatus()
        {
            string status = tempController.QueryRelayStatus(RelayNumber); // Query the status of the relay.
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
                bool relaySuccessFlag = tempController.SetRelayParameters(RelayNumber, Int32.Parse(On)); //Turn cryo on
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
                bool relaySuccessFlag = tempController.SetRelayParameters(RelayNumber, Int32.Parse(Off));
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

        private void SetHeaterSetpoint(int Output, double Value)
        {
            if (Value > 0)
            {
                tempController.SetControlSetpoint(Output, Value);
            }
        }

        private void EnableLakeShoreHeaterOutput3or4(int Output, bool OnOff)
        {
            if (OnOff)
            {
                tempController.SetHeaterRange(Output, 1); // true = on
                HeatersEnabled = true;
            }
            else
            {
                tempController.SetHeaterRange(Output, 0); // false = off
                HeatersEnabled = false;
            }
        }

        /// <summary>
        /// Sets the range parameter for a given output.
        /// LakeShore 336 outputs 1 and 2 are the PID loop controlled heaters (100 Watts and 50 Watts respectively).
        /// For outputs 1 and 2: 0 = Off, 1 = Low, 2 = Medium and 3= High.
        /// </summary>
        /// <param name="Output"></param>
        /// <param name="range"></param>
        private void EnableLakeShoreHeaterOutput1or2(int Output, int range)
        {
            if (range != 0)
            {
                tempController.SetHeaterRange(Output, range); // 1 = Low, 2 = Medium and 3= High
                HeatersEnabled = true;
            }
            else
            {
                tempController.SetHeaterRange(Output, range); // 0 = Off
                HeatersEnabled = false;
            }
        }

        private void IsOutputEnabled(int Output)
        {
            string HeaterOutput = tempController.QueryHeaterRange(Output);
            string trimResponse = HeaterOutput.Trim();// Trim in case there are unexpected white spaces.
            string status = trimResponse.Substring(0, 1); // Take the first character of the string.
            if (status == "1") HeatersEnabled = true; // Heater Output is on
            else HeatersEnabled = false; // Heater Output is off
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
        public void UpdateSourceModeStatus(string StatusUpdate)
        {
            if (StatusUpdate != LastStatusMessage)
            {
                LastStatusMessage = StatusUpdate;
                if (SourceMode == "Refresh")
                {
                    window.AppendTextBox(window.tbRefreshModeStatus, StatusUpdate + Environment.NewLine);
                }
                if (SourceMode == "Warmup")
                {
                    window.AppendTextBox(window.tbWarmUpModeStatus, StatusUpdate + Environment.NewLine);
                }
                if (SourceMode == "Cooldown")
                {
                    window.AppendTextBox(window.tbCoolDownModeStatus, StatusUpdate + Environment.NewLine);
                }
            }
        }
        public void UpdateSourceModeHeaterSetpoints(double setpoint)
        {
            window.SetTextBox(window.tbHeaterTempSetpointStage2, setpoint.ToString());
            UpdateStage2TemperatureSetpoint();
            window.SetTextBox(window.tbHeaterTempSetpointStage1, setpoint.ToString());
            UpdateStage1TemperatureSetpoint();
        }
        public void EnableSourceModeHeaters(bool Enable)
        {
            if (Enable)
            {
                StartStage1DigitalHeaterControl(); // turn heaters setpoint loop on
                StartStage2DigitalHeaterControl(); // turn heaters setpoint loop on
            }
            else
            {
                StopStage1DigitalHeaterControl(); // turn heaters setpoint loop off 
                StopStage2DigitalHeaterControl(); // turn heaters setpoint loop off
                EnableDigitalHeaters(1, false); // turn heaters off (when stopped, the setpoint loop will leave the heaters in their last enabled/disabled state)
                EnableDigitalHeaters(2, false); // turn heaters off (when stopped, the setpoint loop will leave the heaters in their last enabled/disabled state)
            }
        }
        public void UpdateWarmToRoomTemperatureOnlyFlag()
        {
            if (SourceMode == "Refresh")
            {
                WarmToRoomTemperatureOnly = window.checkBoxRefreshSourceAtRoomTemperature.Checked;
            }
            else
            {
                if (SourceMode == "Warmup")
                {
                    WarmToRoomTemperatureOnly = window.checkBoxWarmUpSourceToRoomTemperature.Checked;
                }
                else
                {
                    if (SourceMode == "Cooldown")
                    {
                        WarmToRoomTemperatureOnly = window.checkBoxCoolDownSourceAtRoomTemperature.Checked;
                    }
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
            window.EnableControl(window.checkBoxWarmUpSourceToRoomTemperature, Enable);
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
            window.EnableControl(window.checkBoxRefreshSourceAtRoomTemperature, Enable);
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
            window.EnableControl(window.checkBoxCoolDownSourceAtRoomTemperature, Enable);
        }
        public void EnableOtherSourceModeUIControls(bool Enable)
        {
            // Enable user control of the cryo on/off status
            window.EnableControl(window.checkBoxCryoEnable, Enable);
            // Enable user control of the heater setpoint when in source mode
            window.EnableControl(window.btUpdateHeaterControlStage2, Enable);
            window.EnableControl(window.btUpdateHeaterControlStage1, Enable);
            window.EnableControl(window.btHeatersTurnOffWaitStart, Enable);
            // Disable clear pressure/temp data so that the user can't interfere with refresh mode
            window.EnableControl(window.btClearAllPressureData, Enable);
            window.EnableControl(window.btClearAllTempData, Enable);
            window.EnableControl(window.btClearSourcePressureData, Enable);
            window.EnableControl(window.btClearBeamlinePressureData, Enable);
            window.EnableControl(window.btClearCellTempData, Enable);
            window.EnableControl(window.btClearSF6TempData, Enable);
            window.EnableControl(window.btClearS2TempData, Enable);
            window.EnableControl(window.btClearS1TempData, Enable);
            window.EnableControl(window.btClearNeonTempData, Enable);
        }

        // Source mode parameters
        private DateTime HeatersTurnOffDateTime;
        private DateTime CryoTurnOnDateTime;
        private double NeonEvaporationCycleTemperatureMax;
        private double TurbomolecularPumpUpperPressureLimit;
        private double WarmUpTemperatureSetpoint;
        private double CryoStoppingPressure;
        private double CryoStartingTemperatureMax;
        private double CryoStartingPressure;
        private string LastStatusMessage;
        private bool SourceModeTemperatureSetpointUpdated;
        private bool HeatersEnabled;
        private bool sourceModeCancelFlag;
        private bool WarmToRoomTemperatureOnly;
        private bool SourceModeActive = false;
        private int NeonEvaporationCycleWaitTime;
        private int WarmupMonitoringWait;
        private int CoolDownWait;
        public void SetSourceModeConstants()
        {
            if (SourceMode == "Refresh")
            {
                NeonEvaporationCycleTemperatureMax = SourceRefreshConstants.NeonEvaporationCycleTemperatureMax;
                TurbomolecularPumpUpperPressureLimit = SourceRefreshConstants.TurbomolecularPumpUpperPressureLimit;
                NeonEvaporationCycleWaitTime = SourceRefreshConstants.NeonEvaporationCycleWaitTime;
                CryoStoppingPressure = SourceRefreshConstants.CryoStoppingPressure;
                WarmupMonitoringWait = SourceRefreshConstants.WarmupMonitoringWait;
                CoolDownWait = SourceRefreshConstants.CoolDownWait;
                CryoStartingTemperatureMax = SourceRefreshConstants.CryoStartingTemperatureMax;
                CryoStartingPressure = SourceRefreshConstants.CryoStartingPressure;
                HeatersTurnOffDateTime = window.dateTimePickerRefreshModeTurnHeatersOff.Value;
                CryoTurnOnDateTime = window.dateTimePickerRefreshModeTurnCryoOn.Value;
            }
            if (SourceMode == "Warmup")
            {
                NeonEvaporationCycleTemperatureMax = SourceWarmUpConstants.NeonEvaporationCycleTemperatureMax;
                TurbomolecularPumpUpperPressureLimit = SourceWarmUpConstants.TurbomolecularPumpUpperPressureLimit;
                NeonEvaporationCycleWaitTime = SourceWarmUpConstants.NeonEvaporationCycleWaitTime;
                CryoStoppingPressure = SourceWarmUpConstants.CryoStoppingPressure;
                WarmupMonitoringWait = SourceWarmUpConstants.WarmupMonitoringWait;
                HeatersTurnOffDateTime = window.dateTimePickerWarmUpModeTurnHeatersOff.Value;
            }
            if (SourceMode == "Cooldown")
            {
                TurbomolecularPumpUpperPressureLimit = SourceCoolDownConstants.TurbomolecularPumpUpperPressureLimit;
                WarmupMonitoringWait = SourceCoolDownConstants.WarmupMonitoringWait;
                CoolDownWait = SourceCoolDownConstants.CoolDownWait;
                CryoStartingTemperatureMax = SourceCoolDownConstants.CryoStartingTemperatureMax;
                CryoStartingPressure = SourceCoolDownConstants.CryoStartingPressure;
                HeatersTurnOffDateTime = window.dateTimePickerCoolDownModeTurnHeatersOff.Value;
                CryoTurnOnDateTime = window.dateTimePickerCoolDownModeTurnCryoOn.Value;
            }
        }

        // Source mode processes - these are generalized so that they can be used by different source mode (refresh mode, warm up, etc.) using different parameters.
        private void InitializeSourceRefresh()
        {
            window.SetTextBox(window.tbRefreshModeStatus, "Starting initialization process");
        }
        private void InitializeSourceMode()
        {
            UpdateSourceModeStatus("Starting initialization process");
            SetSourceModeConstants();
            SourceModeActive = true;
            SourceModeTemperatureSetpointUpdated = false; // reset this flag
            if (SourceMode == "Refresh")
            {
                RefreshModeEnableUIElements(true);
                UpdateRefreshTemperature();
                UpdateWarmToRoomTemperatureOnlyFlag();
            }
            if (SourceMode == "Warmup")
            {
                WarmUpModeEnableUIElements(true);
                UpdateWarmUpTemperature();
                UpdateWarmToRoomTemperatureOnlyFlag();
            }
            if (SourceMode == "Cooldown")
            {
                CoolDownModeEnableUIElements(true);
                UpdateCoolDownTemperature();
                UpdateWarmToRoomTemperatureOnlyFlag();
            }
        }
        /// <summary>
        /// Controls the process of incrementally warming up the source. This is done gradually so that the neon evaporates at a steady rate - reducing the risk to the turbomolecular pump.
        /// A process map (flow diagram) of this code can be found on OneNote in "Equipment + Apparatus" > "Hardware Controller" > "Source refresh mode process maps"
        /// </summary>
        private void EvaporateAndPumpNeon()
        {
            if (!sourceModeCancelFlag)
            {
                window.SetTextBox(window.tbHeaterTempSetpointStage2, SourceRefreshConstants.NeonEvaporationCycleTemperatureMax.ToString());
                UpdateStage2TemperatureSetpoint();
                window.SetTextBox(window.tbHeaterTempSetpointStage1, SourceRefreshConstants.NeonEvaporationCycleTemperatureMax.ToString());
                UpdateStage1TemperatureSetpoint();
                window.SetTextBox(window.tbRefreshModeStatus, "Starting neon evaporation cycle");
            }

            for (; ; )// for (; ; ) is an infinite loop, equivalent to while(true)
            {
                if (sourceModeCancelFlag) break; // Immediately break this for loop if the user has requested that refresh mode be cancelled
                UpdateUITimeLeftIndicators();
                if (lastSourcePressure >= SourceRefreshConstants.TurbomolecularPumpUpperPressureLimit) // If the pressure is too high, then the heaters should be disabled so that the turbomolecular pump is not damaged
                {
                    window.SetTextBox(window.tbRefreshModeStatus, "Neon evaporation cycle: pressure above turbo limit");
                    if (Stage1HeaterControlFlag & Stage2HeaterControlFlag)
                    {
                        EnableLakeShoreHeaterOutput3or4(SourceRefreshConstants.S1LakeShoreHeaterOutput, false); // turn off heaters
                        EnableLakeShoreHeaterOutput3or4(SourceRefreshConstants.S2LakeShoreHeaterOutput, false); // turn off heaters
                    }
                }
                else
                {
                    EnableLakeShoreHeaterOutput3or4(SourceRefreshConstants.S1LakeShoreHeaterOutput, true); // turn on heaters
                    EnableLakeShoreHeaterOutput3or4(SourceRefreshConstants.S2LakeShoreHeaterOutput, true); // turn on heaters
                    if (Double.Parse(lastS2Temp) >= SourceRefreshConstants.NeonEvaporationCycleTemperatureMax) // Check if the S2 temperature has reached the end of the neon evaporation cycle (there should be little neon left to evaporate after S2 temperature = NeonEvaporationCycleTemperatureMax)
                    {
                        if (lastSourcePressure <= SourceRefreshConstants.CryoStoppingPressure) // If the pressure is low enough that the cryo cooler can be turned off, then break the for loop.
                        {
                            break;
                        }
                        window.SetTextBox(window.tbRefreshModeStatus, "Neon evaporation cycle: temperature high enough, but pressure too high for cryo shutdown");
                    }
                    else
                    {
                        window.SetTextBox(window.tbRefreshModeStatus, "Neon evaporation cycle: pressure and temperature low - heating source");
                    }
                }

                Thread.Sleep(SourceRefreshConstants.NeonEvaporationCycleWaitTime);
            }
        }
        private void EvaporateAndPumpNeonWithoutLakeShore()
        {
            if (!sourceModeCancelFlag)
            {
                UpdateSourceModeHeaterSetpoints(NeonEvaporationCycleTemperatureMax);
                UpdateSourceModeStatus("Starting neon evaporation cycle");
            }

            for (; ; )// for (; ; ) is an infinite loop, equivalent to while(true)
            {
                if (sourceModeCancelFlag) break; // Immediately break this for loop if the user has requested that source mode be cancelled
                UpdateUITimeLeftIndicators();
                if (lastSourcePressure >= TurbomolecularPumpUpperPressureLimit) // If the pressure is too high, then the heaters should be disabled so that the turbomolecular pump is not damaged
                {
                    UpdateSourceModeStatus("Neon evaporation cycle: temperature has yet to reach setpoint, but pressure above turbo limit");
                    if (Stage1HeaterControlFlag & Stage2HeaterControlFlag)
                    {
                        EnableSourceModeHeaters(false); // Disable heaters
                    }
                }
                else
                {
                    EnableSourceModeHeaters(true); // Enable heaters
                    if (Double.Parse(lastCellTemp) >= NeonEvaporationCycleTemperatureMax) // Check if the S2 temperature has reached the end of the neon evaporation cycle (there should be little neon left to evaporate after S2 temperature = NeonEvaporationCycleTemperatureMax)
                    {
                        if (lastSourcePressure <= CryoStoppingPressure) // If the pressure is low enough that the cryo cooler can be turned off, then break the for loop.
                        {
                            break;
                        }
                        UpdateSourceModeStatus("Neon evaporation cycle: temperature has reached setpoint, but pressure too high for cryo shutdown");
                    }
                    else
                    {
                        UpdateSourceModeStatus("Neon evaporation cycle: pressure and temperature low - heating source");
                    }
                }

                Thread.Sleep(NeonEvaporationCycleWaitTime);
            }
        }
        private void TurnOffCryoAndWarmup()
        {
            TurnOffCryoCooler(); // The pressure should be checked before this function is used (see process maps on OneNote)

            SetHeaterSetpoint(SourceRefreshConstants.S2LakeShoreHeaterOutput, SourceRefreshConstants.RefreshingTemperature);

            for (; ; )
            {
                if (lastSourcePressure < SourceRefreshConstants.TurbomolecularPumpUpperPressureLimit)
                {
                    if (!HeatersEnabled) // if heaters turned off then turn them on
                    {
                        EnableLakeShoreHeaterOutput3or4(SourceRefreshConstants.S2LakeShoreHeaterOutput, true); // turn on heaters
                    }
                    if (Double.Parse(lastCellTemp) >= SourceRefreshConstants.RefreshingTemperature)
                    {
                        break;
                    }
                }
                else
                {
                    if (HeatersEnabled) // if heaters are on
                    {
                        EnableLakeShoreHeaterOutput3or4(SourceRefreshConstants.S2LakeShoreHeaterOutput, false); // turn heaters off
                    }
                }
                Thread.Sleep(SourceRefreshConstants.WarmupMonitoringWait);
            }

        }
        private void TurnOffCryoAndWarmupWithoutLakeShore()
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
                    UpdateSourceModeStatus("Waiting for pressure to reduce before turning off the cryo. The cryo will be turned off when the source chamber pressure reaches " + CryoStoppingPressure.ToString() + " mbar.");
                    Thread.Sleep(WarmupMonitoringWait); // Iterate the loop according to this time interval
                }
                EnableCryoDigitalControl(false); // Turn off cryo
                UpdateSourceModeStatus("Starting warmup");

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
                        if (Double.Parse(lastS2Temp) >= WarmUpTemperatureSetpoint) // If the source has reached the desired temperature, then break the loop
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
                    Thread.Sleep(WarmupMonitoringWait); // Iterate the loop according to this time interval
                }
            }
        }
        public void SourceModeWait()
        {
            //If the source reaches the desired temperature before the user defined heater turn off time, then wait until this time.
            for (; ; )
            {
                // Check conditions that would lead to this loop being exited:
                if (sourceModeCancelFlag) break; // If refresh mode has been cancelled then exit this loop (check on every iteration of the loop)
                if (HeatersTurnOffDateTime < DateTime.Now) break; // If the user requested that the heaters turn off at this time, then exit this loop.

                // Check if the user has updated the temperature setpoint:
                if (SourceModeTemperatureSetpointUpdated) // If the temperature setpoint is updated by the user, this if statement will update the hardware temperature setpoint.
                {
                    UpdateSourceModeHeaterSetpoints(WarmUpTemperatureSetpoint); // Set heater setpoints to user defined value
                    SourceModeTemperatureSetpointUpdated = false; //Reset the flag
                }

                if (WarmToRoomTemperatureOnly) // If the user has stated that the source should be left at room temperature, then turn off the heaters
                {
                    if (Stage1HeaterControlFlag | Stage2HeaterControlFlag) // if heaters are on, turn them off
                    {
                        EnableSourceModeHeaters(false);
                    }
                    UpdateSourceModeStatus("Waiting at room temperature"); // Update source mode status textbox
                }
                else // User wants source to be heated
                {
                    // Check is the source pressure is within safe limits for the turbomolecular pump:
                    if (lastSourcePressure < TurbomolecularPumpUpperPressureLimit) // If pressure is low, then turn the heaters on
                    {
                        if (!Stage1HeaterControlFlag | !Stage2HeaterControlFlag) // if heaters turned off then turn them on
                        {
                            EnableSourceModeHeaters(true); // Enable heaters
                        }
                        UpdateSourceModeStatus("Waiting at " + WarmUpTemperatureSetpoint + " Kelvin."); // Update source mode status textbox
                    }
                    else // If the pressure is high, then turn the heaters off
                    {
                        UpdateSourceModeStatus("Waiting at " + WarmUpTemperatureSetpoint + " Kelvin. Heaters disabled because the source chamber pressure is above the safe operating limit for the turbo (" + TurbomolecularPumpUpperPressureLimit.ToString() + " mbar)");
                        if (Stage1HeaterControlFlag | Stage2HeaterControlFlag) // if heaters are on
                        {
                            EnableSourceModeHeaters(false); // Disable heaters
                        }
                    }
                }

                UpdateUITimeLeftIndicators(); // Update user interface indicators to show how long is left until the heaters turn off and/or the cryo turns on
                Thread.Sleep(WarmupMonitoringWait); // Iterate the loop according to this time interval
            }
        }
        private void CoolDownSource()
        {
            if (HeatersEnabled)
            {
                EnableLakeShoreHeaterOutput3or4(SourceRefreshConstants.S2LakeShoreHeaterOutput, false);
            }
            for (; ; )
            {
                if (Double.Parse(lastS1Temp) <= SourceRefreshConstants.CryoStartingTemperatureMax & Double.Parse(lastS2Temp) <= SourceRefreshConstants.CryoStartingTemperatureMax)
                { break; }
                Thread.Sleep(SourceRefreshConstants.CoolDownWait);
            }
            for (; ; )
            {
                if (lastSourcePressure <= SourceRefreshConstants.CryoStartingPressure)
                { break; }
                Thread.Sleep(SourceRefreshConstants.CoolDownWait);
            }
            TurnOnCryoCooler();
        }
        private void CoolDownSourceWithoutLakeShore()
        {
            if (!sourceModeCancelFlag)
            {
                EnableSourceModeHeaters(false); // Turn off heaters
                UpdateSourceModeStatus("Heaters stopped for cryo turn on"); // Update status textbox

                // Wait until the (user defined) cryo turn on time is reached:
                for (; ; )
                {
                    if (sourceModeCancelFlag) break;
                    if (CryoTurnOnDateTime < DateTime.Now) break; // Exit this loop when the user defined datetime is reached.
                    Thread.Sleep(CoolDownWait);
                    UpdateUITimeLeftIndicators(); // Update user interface indicators to show how long is left until the cryo turns on
                }

                if (!sourceModeCancelFlag) ResetUITimeLeftIndicators(); // Clears the user interface textboxes now that the timers have finished.

                // Wait until the source temperature is low enough for the cryo to be started:
                for (; ; )
                {
                    if (sourceModeCancelFlag) break;
                    UpdateSourceModeStatus("Waiting for temperature to reach the safe operating range for cryo to turn on"); // Update source mode status
                    if (Double.Parse(lastS1Temp) <= CryoStartingTemperatureMax & Double.Parse(lastS2Temp) <= CryoStartingTemperatureMax)
                    { break; }
                    Thread.Sleep(CoolDownWait);
                }

                // Wait for the pressure to be low enough for the cryo to be started:
                for (; ; )
                {
                    if (sourceModeCancelFlag) break;
                    UpdateSourceModeStatus("Waiting for the pressure to reach a low enough value for the cryo to turn on"); // Update source mode status
                    if (lastSourcePressure <= CryoStartingPressure)
                    { break; }
                    Thread.Sleep(CoolDownWait);
                }
                if (!sourceModeCancelFlag)
                {
                    UpdateSourceModeStatus("Starting cryo");
                    EnableCryoDigitalControl(true);
                }
            }
        }

        // Refresh mode
        private Thread refreshModeThread;
        private Object refreshModeLock;
        private bool refreshModeHeaterTurnOffDateTimeFlag = false;
        private bool refreshModeCryoTurnOnDateTimeFlag = false;
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
        public void EnableRefreshModeRoomTemperature(bool Enable)
        {
            WarmToRoomTemperatureOnly = Enable;
            window.EnableControl(window.btRefreshModeTemperatureSetpointUpdate, !Enable); // Enable/disable user control of refresh temperature update button
            window.EnableControl(window.tbRefreshModeTemperatureSetpoint, !Enable); // Enable/disable user control of refresh temperature setpoint textbox
            if (Enable)
            {
                WarmUpTemperatureSetpoint = 295; // Approx room temperature
                window.SetTextBox(window.tbRefreshModeTemperatureSetpoint, "295");
            }
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

        public static class SourceRefreshConstants
        {
            public static Double TurbomolecularPumpUpperPressureLimit { get { return 0.0008; } } // 8e-4 mbar
            public static Double NeonEvaporationCycleTemperatureMax { get { return 30; } }  // Kelvin
            public static Int16 S1LakeShoreHeaterOutput { get { return 3; } }  // 
            public static Int16 S2LakeShoreHeaterOutput { get { return 4; } }  // 
            public static Int32 NeonEvaporationCycleWaitTime { get { return 5000; } } // milli seconds
            public static Double CryoStartingPressure { get { return 0.00005; } } // 5e-5 mbar
            public static Double CryoStoppingPressure { get { return 0.00001; } } // 1e-5 mbar
            public static Double CryoStartingTemperatureMax { get { return 310; } } // Kelvin
            public static Double RefreshingTemperature { get { return 300; } } // Kelvin
            public static Int32 WarmupMonitoringWait { get { return 3000; } } // milli seconds
            public static Int32 CoolDownWait { get { return 3000; } } // milli seconds
            public static Double NeonEvaporationPollPeriod { get { return 100; } } // milli seconds
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
                                if (SourceModeTemperatureSetpointUpdated | WarmToRoomTemperatureOnly)
                                {
                                    refreshModeThread = new Thread(new ThreadStart(refreshModeWorker));
                                    SourceMode = "Refresh";
                                    refreshModeLock = new Object();
                                    sourceModeCancelFlag = false;
                                    refreshModeThread.Start();
                                }
                                else
                                {
                                    MessageBox.Show("Please provide a refresh mode temperature (and click update) or select the \"Refresh at room temperature\" checkbox.\n\nRefresh mode not started.", "Refresh Mode Exception", MessageBoxButtons.OK);
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
            if (!sourceModeCancelFlag) EvaporateAndPumpNeonWithoutLakeShore(); // Controlled evaporation of neon from cryo pump
            if (!sourceModeCancelFlag) TurnOffCryoAndWarmupWithoutLakeShore(); // Cryo turn off and controlled warm up off source
            if (!sourceModeCancelFlag) SourceModeWait(); // Wait at desired temperature, until the user defined datetime
            if (!sourceModeCancelFlag) CoolDownSourceWithoutLakeShore(); // Turn on cryo
            if (sourceModeCancelFlag) // If refresh mode is cancelled, then turn off the heaters before finishing.
            {
                UpdateSourceModeStatus("Refresh mode cancelled");
                EnableSourceModeHeaters(false); // Disable heaters
                ResetUITimeLeftIndicators();
            }
            RefreshModeEnableUIElements(false); // Enable/disable UI elements that had been disabled/enabled whilst in refresh mode.
            SourceMode = ""; // Reset parameter
            SourceModeActive = false;
        }

        // Warm up mode
        private Thread warmupModeThread;
        private Object warmupModeLock;
        private bool warmupModeHeaterTurnOffDateTimeFlag = false;
        private bool warmupModeTemperatureSetpointUpdated = false;
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
        public void EnableWarmUpModeRoomTemperature(bool Enable)
        {
            WarmToRoomTemperatureOnly = Enable;
            window.EnableControl(window.btWarmUpModeTemperatureSetpointUpdate, !Enable); // Enable/disable user control of warm up mode temperature update button
            window.EnableControl(window.tbWarmUpModeTemperatureSetpoint, !Enable); // Enable/disable user control of warm up mode temperature setpoint textbox
            if (Enable)
            {
                WarmUpTemperatureSetpoint = 295; // Approx room temperature
                window.SetTextBox(window.tbWarmUpModeTemperatureSetpoint, "295");
            }
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

        public static class SourceWarmUpConstants
        {
            public static Double TurbomolecularPumpUpperPressureLimit { get { return 0.0008; } } // 8e-4 mbar
            public static Double NeonEvaporationCycleTemperatureMax { get { return 30; } }  // Kelvin
            public static Int16 S1LakeShoreHeaterOutput { get { return 3; } }  // 
            public static Int16 S2LakeShoreHeaterOutput { get { return 4; } }  // 
            public static Int32 NeonEvaporationCycleWaitTime { get { return 5000; } } // milli seconds
            public static Double CryoStoppingPressure { get { return 0.00001; } } // 1e-5 mbar
            public static Int32 WarmupMonitoringWait { get { return 3000; } } // milli seconds
            public static Double NeonEvaporationPollPeriod { get { return 100; } } // milli seconds
        }

        internal void StartWarmUpMode()
        {
            UpdateWarmUpModeHeaterTurnOffDateTime(); // Update the heater turn off datetime. This is in case, for example, the refresh mode heater turn off time was last updated.
            if (warmupModeHeaterTurnOffDateTimeFlag) // A flag specific to warm up mode - to avoid conflicts with other modes
            {
                if (HeatersTurnOffDateTime > DateTime.Now) // The heaters cannot be turned off in the past 
                {
                    if (warmupModeTemperatureSetpointUpdated | WarmToRoomTemperatureOnly)
                    {
                        warmupModeThread = new Thread(new ThreadStart(warmupModeWorker));
                        SourceMode = "Warmup";
                        warmupModeLock = new Object();
                        sourceModeCancelFlag = false;
                        warmupModeThread.Start();
                    }
                    else
                    {
                        MessageBox.Show("Please provide a warm up mode temperature (and click update) or select the \"Warm up to room temperature\" checkbox.\n\nWarm up mode not started.", "Warm Up Mode Exception", MessageBoxButtons.OK);
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
            if (!sourceModeCancelFlag) EvaporateAndPumpNeonWithoutLakeShore(); // Controlled evaporation of neon from cryo pump
            if (!sourceModeCancelFlag) TurnOffCryoAndWarmupWithoutLakeShore(); // Cryo turn off and controlled warm up of source
            if (!sourceModeCancelFlag) SourceModeWait(); // Wait at desired temperature, until the user defined datetime
            if (sourceModeCancelFlag) // If warm up mode is cancelled, then turn off the heaters before finishing.
            {
                UpdateSourceModeStatus("Warm up mode cancelled");
                EnableSourceModeHeaters(false); // Disable heaters
                ResetUITimeLeftIndicators();
            }
            WarmUpModeEnableUIElements(false); // Enable/disable UI elements that had been disabled/enabled whilst in warm up mode.
            SourceMode = ""; // Reset parameter
            SourceModeActive = false;
        }

        // Cool down mode
        private Thread CoolDownModeThread;
        private Object CoolDownModeLock;
        private bool CoolDownModeHeaterTurnOffDateTimeFlag = false;
        private bool CoolDownModeCryoTurnOnDateTimeFlag = false;
        private bool CoolDownModeTemperatureSetpointUpdated = false;
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
        public void EnableCoolDownModeRoomTemperature(bool Enable)
        {
            WarmToRoomTemperatureOnly = Enable;
            window.EnableControl(window.btCoolDownModeTemperatureSetpointUpdate, !Enable); // Enable/disable user control of cool down mode temperature update button
            window.EnableControl(window.tbCoolDownModeTemperatureSetpoint, !Enable); // Enable/disable user control of cool down mode temperature setpoint textbox
            if (Enable)
            {
                WarmUpTemperatureSetpoint = 295; // Approx room temperature
                window.SetTextBox(window.tbCoolDownModeTemperatureSetpoint, "295");
            }
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

        public static class SourceCoolDownConstants
        {
            public static Double TurbomolecularPumpUpperPressureLimit { get { return 0.0008; } } // 8e-4 mbar
            public static Int16 S1LakeShoreHeaterOutput { get { return 3; } }  // 
            public static Int16 S2LakeShoreHeaterOutput { get { return 4; } }  // 
            public static Double CryoStartingPressure { get { return 0.00005; } } // 5e-5 mbar
            public static Double CryoStartingTemperatureMax { get { return 310; } } // Kelvin
            public static Int32 WarmupMonitoringWait { get { return 3000; } } // milli seconds
            public static Int32 CoolDownWait { get { return 3000; } } // milli seconds
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
                                if (CoolDownModeTemperatureSetpointUpdated | WarmToRoomTemperatureOnly)
                                {
                                    CoolDownModeThread = new Thread(new ThreadStart(CoolDownModeWorker));
                                    SourceMode = "Cooldown";
                                    CoolDownModeLock = new Object();
                                    sourceModeCancelFlag = false;
                                    CoolDownModeThread.Start();
                                }
                                else
                                {
                                    MessageBox.Show("Please provide a cool down mode temperature (and click update) or select the \"Leave at room temperature until cryo is turned on\" checkbox.\n\nCool down mode not started.", "Cool Down Mode Exception", MessageBoxButtons.OK);
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
            if (!sourceModeCancelFlag) CoolDownSourceWithoutLakeShore(); // Turn on cryo
            if (sourceModeCancelFlag) // If cool down mode is cancelled, then turn off the heaters before finishing.
            {
                UpdateSourceModeStatus("Refresh mode cancelled");
                EnableSourceModeHeaters(false); // Disable heaters
                ResetUITimeLeftIndicators();
            }
            CoolDownModeEnableUIElements(false); // Enable/disable UI elements that had been disabled/enabled whilst in refresh mode.
            SourceMode = ""; // Reset parameter
            SourceModeActive = false;
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

        public void StartStage1DigitalHeaterControl()
        {
            Stage1HeaterControlFlag = true;
            window.EnableControl(window.btStartHeaterControlStage1, false);
            window.EnableControl(window.btStopHeaterControlStage1, true);
            window.EnableControl(window.checkBoxEnableHeatersS1, false);
        }
        public void StartStage2DigitalHeaterControl()
        {
            Stage2HeaterControlFlag = true;
            window.EnableControl(window.btStartHeaterControlStage2, false);
            window.EnableControl(window.btStopHeaterControlStage2, true);
            window.EnableControl(window.checkBoxEnableHeatersS2, false);
        }

        public void StopStage1DigitalHeaterControl()
        {
            Stage1HeaterControlFlag = false; // change control flag so that the temperature setpoint loop stops
            window.EnableControl(window.btStartHeaterControlStage1, true);
            window.EnableControl(window.btStopHeaterControlStage1, false);
            window.EnableControl(window.checkBoxEnableHeatersS1, true);
            EnableDigitalHeaters(1, false); // turn off heater
        }
        public void StopStage2DigitalHeaterControl()
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
                    if (Double.Parse(lastS2Temp) < Stage2TemperatureSetpoint)
                    {
                        EnableDigitalHeaters(2, true);
                    }
                    else EnableDigitalHeaters(2, false);
                }
                if (Stage1HeaterControlFlag)
                {
                    if (Double.Parse(lastS1Temp) < Stage1TemperatureSetpoint)
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
            window.EnableControl(window.checkBoxCryoEnable, false);
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
                    StopStage2DigitalHeaterControl();
                    StopStage1DigitalHeaterControl();
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

        #region Pressure monitor

        private double lastSourcePressure;
        private int pressureMovingAverageSampleLength = 10;
        private Queue<double> pressureSamples = new Queue<double>();
        private string sourceSeries = "Source Pressure";

        public void UpdatePressureMonitor()
        {
            //sample the pressure
            lastSourcePressure = sourcePressureMonitor.Pressure;

            //add samples to Queues for averaging
            pressureSamples.Enqueue(lastSourcePressure);

            //drop samples when array is larger than the moving average sample length
            while (pressureSamples.Count > pressureMovingAverageSampleLength)
            {
                pressureSamples.Dequeue();
            }

            //average samples
            double avgPressure = pressureSamples.Average();
            string avgPressureExpForm = avgPressure.ToString("E");

            //update text boxes
            window.SetTextBox(window.tbPSource, (avgPressureExpForm).ToString());
        }

        public void ClearPressureMonitorAv()
        {
            pressureSamples.Clear();
        }

        public void PlotLastPressure()
        {
            //sample the pressure
            lastSourcePressure = sourcePressureMonitor.Pressure;
            DateTime localDate = DateTime.Now;

            //plot the most recent samples
            window.AddPointToChart(window.chart1, sourceSeries, localDate, lastSourcePressure);
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
        //private Queue<double> TempSamples = new Queue<double>();
        public string[] TemperatureArray;
        public string lastCellTemp;
        public string lastS1Temp;
        public string lastS2Temp;
        public string lastNeonTemp;
        public string lastSF6Temp;
        private string cellTSeries = "Cell Temperature";
        private string S1TSeries = "S1 Temperature";
        private string S2TSeries = "S2 Temperature";
        private string SF6TSeries = "SF6 Temperature";
        private string neonTSeries = "Neon Temperature";

        public void UpdateAllTempMonitors()
        {
            //sample the temperatures
            receivedData = tempController.GetTemperature(0, "K");
            TemperatureArray = receivedData.Split(',');
            if (TemperatureArray.Length == 8)
            {
                lastCellTemp = TemperatureArray[0]; // LakeShore Input A
                lastNeonTemp = TemperatureArray[1]; // LakeShore Input B
                lastS2Temp = TemperatureArray[2];   // LakeShore Input C
                lastSF6Temp = TemperatureArray[3];  // LakeShore Input D1
                lastS1Temp = TemperatureArray[4];   // LakeShore Input D2
                window.SetTextBox(window.tbTCell, lastCellTemp);
                window.SetTextBox(window.tbTNeon, lastNeonTemp);
                window.SetTextBox(window.tbTS2, lastS2Temp);
                window.SetTextBox(window.tbTS1, lastS1Temp);
                window.SetTextBox(window.tbTSF6, lastSF6Temp);
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

                lastCellTemp = tempMonitorsData[0].ToString("N6");
                lastS1Temp = tempMonitorsData[1].ToString("N6");
                lastS2Temp = tempMonitorsData[2].ToString("N6");
                lastSF6Temp = tempMonitorsData[3].ToString("N6");
                window.SetTextBox(window.tbTCell, lastCellTemp);
                window.SetTextBox(window.tbTNeon, lastS1Temp);
                window.SetTextBox(window.tbTS2, lastS2Temp);
                window.SetTextBox(window.tbTSF6, lastSF6Temp);
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
            double CellTemp = Double.Parse(lastCellTemp);
            double S1Temp = Double.Parse(lastS1Temp);
            double S2Temp = Double.Parse(lastS2Temp);
            double SF6Temp = Double.Parse(lastSF6Temp);
            double NeonTemp = Double.Parse(lastNeonTemp);

            //plot the most recent samples
            window.AddPointToChart(window.chart2, cellTSeries, localDate, CellTemp);
            window.AddPointToChart(window.chart2, S1TSeries, localDate, S1Temp);
            window.AddPointToChart(window.chart2, S2TSeries, localDate, S2Temp);
            window.AddPointToChart(window.chart2, SF6TSeries, localDate, SF6Temp);
            window.AddPointToChart(window.chart2, neonTSeries, localDate, NeonTemp);
        }

        #endregion

        #region Temperature and Pressure Monitoring
        // If the LakeShore is not in operation, the silicon diodes can still be monitored by measuring the voltage drop across them (which is temperature dependent).

        private Thread PTMonitorPollThread;
        private int PTMonitorPollPeriod = 1000;
        private int PTMonitorPollPeriodLowerLimit = 100;
        private bool PTMonitorFlag;
        private Object PTMonitorLock;
        public string csvDataTemperatureAndPressure = "";

        /// <summary>
        /// Many user interface (UI) components need to be enabled/disabled so that the user can't perform actions that could be harmful to the experiment. This function combines this list of UI elements.
        /// </summary>
        /// <param name="StartStop"></param>
        internal void PTMonitorPollEnableUIElements(bool Enable) // Elements to enable/disable when starting/stopping pressure and temperaure monitoring
        {
            window.EnableControl(window.btStartTandPMonitoring, !Enable); // window.btStartTandPMonitoring.Enabled = false when starting monitoring (for example)
            window.EnableControl(window.btStopTandPMonitoring, Enable); // window.btStopTandPMonitoring.Enabled = true when stopping monitoring (for example)
            window.EnableControl(window.checkBoxSF6TempPlot, Enable);
            window.EnableControl(window.checkBoxS2TempPlot, Enable);
            window.EnableControl(window.checkBoxS1TempPlot, Enable);
            window.EnableControl(window.checkBoxCellTempPlot, Enable);
            window.EnableControl(window.checkBoxBeamlinePressurePlot, Enable);
            window.EnableControl(window.checkBoxSourcePressurePlot, Enable);
            window.EnableControl(window.btUpdateHeaterControlStage2, Enable);
            window.EnableControl(window.btStartHeaterControlStage2, Enable);
            window.EnableControl(window.btStartHeaterControlStage1, Enable);
            window.EnableControl(window.btUpdateHeaterControlStage1, Enable);
            window.EnableControl(window.btStartRefreshMode, Enable);
            window.EnableControl(window.btStartWarmUpMode, Enable);
            window.EnableControl(window.btStartCoolDownMode, Enable);

        }

        public void UpdatePTMonitorPollPeriod()
        {
            int PTMonitorPollPeriodParseValue;
            if (Int32.TryParse(window.tbTandPPollPeriod.Text, out PTMonitorPollPeriodParseValue))
            {
                if (PTMonitorPollPeriodParseValue >= PTMonitorPollPeriodLowerLimit)
                {
                    PTMonitorPollPeriod = PTMonitorPollPeriodParseValue; // Update PT monitoring poll period
                }
                else MessageBox.Show("Poll period value too small. The temperature and pressure can only be polled every " + PTMonitorPollPeriodLowerLimit.ToString() + " ms. The limiting factor is communication with the LakeShore temperature controller.", "User input exception", MessageBoxButtons.OK);
            }
            else MessageBox.Show("Unable to parse setpoint string. Ensure that an integer number has been written, with no additional non-numeric characters.", "", MessageBoxButtons.OK);
        }

        internal void StartPTMonitorPoll()
        {
            PTMonitorPollThread = new Thread(() =>
            {
                PTMonitorPollWorker();
            });
            PTMonitorPollThread.IsBackground = true; // When the application is closed, this thread will also immediately stop. This is lazy coding, but it works and shouldn't cause any problems. This means it is a background thread of the main (UI) thread, so it will end with the main thread.

            pressureMovingAverageSampleLength = 10;
            Stage2HeaterControlFlag = false;
            Stage1HeaterControlFlag = false;
            UpdateStage1TemperatureSetpoint();
            UpdateStage2TemperatureSetpoint();
            PTMonitorPollEnableUIElements(true);
            if (csvDataTemperatureAndPressure == "") csvDataTemperatureAndPressure += "Unix Time Stamp (ms)" + "," + "Full date/time" + "," + "Cell Temperature (K)" + "," + "S1 Temperature (K)" + "," + "S2 Temperature (K)" + "," + "SF6 Temperature (K)" + "," + "Source Pressure (mbar)" + "," + "Beamline Pressure (mbar)" + "\r\n"; // Header lines for csv file
            pressureSamples.Clear();
            PTMonitorLock = new Object();
            PTMonitorFlag = false;
            PTMonitorPollThread.Start();
        }
        internal void StopPTMonitorPoll()
        {
            if (SourceModeActive)
            {
                MessageBox.Show(SourceMode + " mode is currently active. To stop temperature and pressure monitoring, please first cancel refresh mode and ensure that the apparatus is in a safe state to be left unmonitored.", "Refresh Mode Exception", MessageBoxButtons.OK);
            }
            else
            {
                UEDMSavePlotDataDialog savePTDataDialog = new UEDMSavePlotDataDialog("Save data message", "Would you like to save the temperature and pressure data now? \n\nThe data will not be cleared.");
                savePTDataDialog.ShowDialog();
                if (savePTDataDialog.DialogResult != DialogResult.Cancel)
                {
                    StopStage1DigitalHeaterControl();
                    StopStage2DigitalHeaterControl();
                    EnableDigitalHeaters(1, false);
                    EnableDigitalHeaters(2, false);
                    PTMonitorFlag = true;
                    if (savePTDataDialog.DialogResult == DialogResult.Yes)
                    {
                        SavePlotDataToCSV(csvDataTemperatureAndPressure);
                    }
                }
                savePTDataDialog.Dispose();
            }
        }
        private void PTMonitorPollWorker()
        {
            int count = 0;

            for (; ; )// for (; ; ) is an infinite loop, equivalent to while(true)
            {
                Thread.Sleep(PTMonitorPollPeriod);
                ++count;
                lock (PTMonitorLock)
                {
                    UpdateAllTempMonitors();
                    PlotLastTemperatures();
                    UpdatePressureMonitor();
                    PlotLastPressure();

                    Double unixTimestamp = (Double)(DateTime.Now.Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds);
                    string csvLine = unixTimestamp + "," + DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss.fff tt") + "," + lastCellTemp + "," + lastS1Temp + "," + lastS2Temp + "," + lastSF6Temp + "," + lastSourcePressure + "\r\n";
                    csvDataTemperatureAndPressure += csvLine;

                    ControlHeaters();
                    if (PTMonitorFlag)
                    {
                        PTMonitorFlag = false;
                        break;
                    }
                }
            }
            PTMonitorPollEnableUIElements(false);
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
                receivedData = tempController.QueryPIDLoopValues(Int32.Parse(window.comboBoxLakeShore336OutputsQuery.Text));
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
                    tempController.SetPIDLoopValues(Int32.Parse(window.comboBoxLakeShore336OutputsSet.Text), PIDValueDoubleArray[0], PIDValueDoubleArray[1], PIDValueDoubleArray[2]);
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
                    tempController.AutotuneOutput(AutotuneOutput, AutotuneMode);
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
            string status = tempController.QueryControlTuningStatus();
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

        #endregion
    }
}
