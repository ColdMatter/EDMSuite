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
        private static string[] Names = { "Cell Temperature Monitor", "S1 Temperature Monitor", "S2 Temperature Monitor", "SF6 Temperature Monitor"};
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
            window.SetDateTimePickerValue(window.dateTimePickerStopHeatingAndTurnCryoOn, InitialDateTime);
            window.SetDateTimePickerValue(window.dateTimePickerHeatersTurnOff, InitialDateTime);
            window.SetDateTimePickerValue(window.dateTimePickerRefreshModeTurnHeatersOff, InitialDateTime);
            // Set flags
            refreshModeHeaterTurnOffDateTimeFlag = false;
            refreshModeCryoTurnOnDateTimeFlag = false;
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
                if(status == Off) window.SetTextBox(window.tbCryoState, "OFF");
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
            if(status == Off) // If off, then try to turn it on
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
            if(Value > 0)
            {
                tempController.SetControlSetpoint(Output, Value);
            }
        }

        private void EnableOutput3or4(int Output, bool OnOff)
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

        private void IsOutputEnabled(int Output)
        {
            string S1HeaterOutput = tempController.QueryHeaterRange(Output);
            string trimResponse = S1HeaterOutput.Trim();// Trim in case there are unexpected white spaces.
            string status = trimResponse.Substring(0, 1); // Take the first character of the string.
            if (status == "1") HeatersEnabled = true;
            else HeatersEnabled = false;
        }

        

        

        private Thread refreshModeThread;
        private bool refreshModeCancelFlag;
        private Object refreshModeLock;
        private bool refreshModeActive = false;
        private bool refreshAtRoomTemperature;
        private bool refreshModeHeaterTurnOffDateTimeFlag = false;
        private bool refreshModeCryoTurnOnDateTimeFlag = false;
        private double refreshTemperature;
        private bool HeatersEnabled;

        internal void RefreshModeEnableUIElements(bool StartStop) // Start = true (elements to enable/disable when starting refresh mode)
        {
            window.EnableControl(window.btStartRefreshMode, !StartStop); // window.btStartRefreshMode.Enabled = false when starting refresh mode (for example)
            window.EnableControl(window.btCancelRefreshMode, StartStop); // window.btCancelRefreshMode.Enabled = true when starting refresh mode (for example)
            window.EnableControl(window.checkBoxCryoEnable, !StartStop);
            window.EnableControl(window.btUpdateHeaterControlStage2, !StartStop);
            window.EnableControl(window.btUpdateHeaterControlStage1, !StartStop);
            window.EnableControl(window.btHeatersTurnOffWaitStart, !StartStop);
            
        }

        internal void StartRefreshMode()
        {
            if (refreshModeHeaterTurnOffDateTimeFlag)
            {
                if(refreshModeCryoTurnOnDateTimeFlag)
                {
                    if (window.dateTimePickerStopHeatingAndTurnCryoOn.Value > window.dateTimePickerRefreshModeTurnHeatersOff.Value)
                    {
                        if (window.dateTimePickerStopHeatingAndTurnCryoOn.Value > DateTime.Now)
                        {
                            if(window.dateTimePickerRefreshModeTurnHeatersOff.Value > DateTime.Now)
                            {
                                refreshModeThread = new Thread(new ThreadStart(refreshModeWorker));
                                RefreshModeEnableUIElements(true);
                                refreshModeLock = new Object();
                                refreshModeCancelFlag = false;
                                refreshModeActive = true;
                                refreshModeThread.Start();
                                UpdateRefreshTemperature();
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
            refreshModeCancelFlag = true;
        }
        private void refreshModeWorker()
        {
            if (!refreshModeCancelFlag) InitializeSourceRefreshWithoutLakeShore();
            if (!refreshModeCancelFlag) WarmUpSourceWithoutLakeShore();
            if (!refreshModeCancelFlag) RefreshModeWait();// Wait at desired temperature, until the user defined datetime
            if (!refreshModeCancelFlag) CoolDownSourceWithoutLakeShore();
            if (refreshModeCancelFlag)
            {
                window.SetTextBox(window.tbRefreshModeStatus, "Refresh mode cancelled");
                StopStage1DigitalHeaterControl(); // turn heaters setpoint loop off
                StopStage2DigitalHeaterControl(); // turn heaters setpoint loop off
                EnableDigitalHeaters(1, false); // turn heaters off
                EnableDigitalHeaters(2, false); // turn heaters off
            }
            RefreshModeEnableUIElements(false);
            refreshModeActive = false;
        }


        public static class SourceRefreshConstants
        {
            public static Double TurbomolecularPumpUpperPressureLimit { get { return 0.0008; } } // 1e-4 mbar
            public static Double NeonEvaporationCycleTemperatureMax { get { return 40; } }  // Kelvin
            public static Int16 S1LakeShoreHeaterOutput { get { return 3; } }  // 
            public static Int16 S2LakeShoreHeaterOutput { get { return 4; } }  // 
            public static Double TemperatureSetpointDecrementValue { get { return 0.5; } } // Kelvin
            public static Double TemperatureSetpointIncrementValue { get { return 0.5; } } // Kelvin
            public static Int32 NeonEvaporationCycleWaitTime { get { return 5000; } } // milli seconds
            public static Double CryoStartingPressure { get { return 0.00005; } } // 5e-5 mbar
            public static Double CryoStoppingPressure { get { return 0.00001; } } // 1e-5 mbar
            public static Double CryoStartingTemperatureMax { get { return 310; } } // Kelvin
            public static Double RefreshingTemperature { get { return 300; } } // Kelvin
            public static Int32 WarmupMonitoringWait { get { return 3000; } } // milli seconds
            public static Int32 CoolDownWait { get { return 3000; } } // milli seconds
        }

        public void EnableRefreshModeRoomTemperature(bool Enable)
        {
            refreshAtRoomTemperature = Enable;
        }
        public void HeaterTurnOffDateTimeSpecified()
        {
            refreshModeHeaterTurnOffDateTimeFlag = true;
        }
        public void CryoTurnOnDateTimeSpecified()
        {
            refreshModeCryoTurnOnDateTimeFlag = true;
        }
        public void UpdateRefreshTemperature()
        {
            string RefreshTemperatureInput = window.tbRefreshModeTemperatureSetpoint.Text;
            double parseddouble;
            if (Double.TryParse(RefreshTemperatureInput, out parseddouble))
            {
                refreshTemperature = parseddouble;
            }
            else MessageBox.Show("Unable to parse refresh temperature string. Ensure that a number has been written, with no additional non-numeric characters.", "", MessageBoxButtons.OK);
        }

        private void InitializeSourceRefresh() // This won't work, but isn't being used yet - problem for another day!
        {
            // Stop/Start pressure and temperature monitoring loops
            StopPressureMonitorPoll();
            StopTempMonitorPoll();
            Thread.Sleep(4000); // Wait for monitoring loops to notice flags and finish operation
            pressureMonitorFlag = false;
            tempMonitorFlag = false;
            StartPressureMonitorPoll();
            StartTempMonitorPoll();
            Thread.Sleep(2000);

            // Check if heaters are enabled
            IsOutputEnabled(SourceRefreshConstants.S2LakeShoreHeaterOutput);

            //Poll cryo status
        }
        private void InitializeSourceRefreshWithoutLakeShore()
        {
            window.SetTextBox(window.tbRefreshModeStatus, "Starting initialization process");
        }

        /// <summary>
        /// Controls the process of incrementally warming up the source. This is done gradually so that the neon evaporates at a steady rate - reducing the risk to the turbomolecular pump.
        /// A process map (flow diagram) of this code can be found on OneNote in "Equipment + Apparatus" > "Hardware Controller" > "Source refresh mode process maps"
        /// </summary>
        private void EvaporateAndPumpNeon()
        {
            double TemperatureSetpoint = 0;

            for (; ; )// for (; ; ) is an infinite loop, equivalent to while(true)
            {
                if (lastPressure >= SourceRefreshConstants.TurbomolecularPumpUpperPressureLimit) // If the pressure is too high for the turbo pump, reduce the setpoint of the heaters
                {
                    if(HeatersEnabled)
                    {
                        TemperatureSetpoint -= SourceRefreshConstants.TemperatureSetpointDecrementValue;
                        SetHeaterSetpoint(SourceRefreshConstants.S2LakeShoreHeaterOutput, TemperatureSetpoint);
                    }
                }
                else // If the pressure is safe for the turbo pump, increase the setpoint of the heaters
                {
                    TemperatureSetpoint += SourceRefreshConstants.TemperatureSetpointIncrementValue;
                    SetHeaterSetpoint(SourceRefreshConstants.S2LakeShoreHeaterOutput, TemperatureSetpoint);
                    if (TemperatureSetpoint >= SourceRefreshConstants.NeonEvaporationCycleTemperatureMax)
                    {
                        if (lastPressure <= SourceRefreshConstants.CryoStoppingPressure)
                        {
                            break;
                        }
                    }
                }

                Thread.Sleep(SourceRefreshConstants.NeonEvaporationCycleWaitTime);
            }
        }
        private void EvaporateAndPumpNeonWithoutLakeShore()
        {
            double TemperatureSetpoint = 0.0;
            if (!refreshModeCancelFlag)
            {
                window.SetTextBox(window.tbHeaterTempSetpointStage2, TemperatureSetpoint.ToString());
                UpdateStage2TemperatureSetpoint();
                window.SetTextBox(window.tbHeaterTempSetpointStage1, TemperatureSetpoint.ToString());
                UpdateStage1TemperatureSetpoint();
                StartStage1DigitalHeaterControl();
                StartStage2DigitalHeaterControl();
                window.SetTextBox(window.tbRefreshModeStatus, "Starting neon evaporation cycle");
            }

            for (; ; )// for (; ; ) is an infinite loop, equivalent to while(true)
            {
                if (refreshModeCancelFlag) break;
                if (lastPressure >= SourceRefreshConstants.TurbomolecularPumpUpperPressureLimit)
                {
                    if (Stage1HeaterControlFlag & Stage2HeaterControlFlag)
                    {
                        TemperatureSetpoint -= SourceRefreshConstants.TemperatureSetpointDecrementValue;
                        window.SetTextBox(window.tbHeaterTempSetpointStage2,TemperatureSetpoint.ToString());
                        UpdateStage2TemperatureSetpoint();
                        window.SetTextBox(window.tbHeaterTempSetpointStage1, TemperatureSetpoint.ToString());
                        UpdateStage1TemperatureSetpoint();
                    }
                }
                else
                {
                    TemperatureSetpoint += SourceRefreshConstants.TemperatureSetpointIncrementValue;
                    window.SetTextBox(window.tbHeaterTempSetpointStage2, TemperatureSetpoint.ToString());
                    UpdateStage2TemperatureSetpoint();
                    window.SetTextBox(window.tbHeaterTempSetpointStage1, TemperatureSetpoint.ToString());
                    UpdateStage1TemperatureSetpoint();
                    if (TemperatureSetpoint >= SourceRefreshConstants.NeonEvaporationCycleTemperatureMax)
                    {
                        if (lastPressure <= SourceRefreshConstants.CryoStoppingPressure)
                        {
                            break;
                        }
                    }
                }
                
                Thread.Sleep(SourceRefreshConstants.NeonEvaporationCycleWaitTime);
            }
        }

        private void TurnOffCryoAndWarmup()
        {
            TurnOffCryoCooler(); // The pressure should be checked before this function is used (see process maps on OneNote)

            SetHeaterSetpoint(SourceRefreshConstants.S2LakeShoreHeaterOutput, SourceRefreshConstants.RefreshingTemperature);

            for(; ; )
            {
                if(lastPressure < SourceRefreshConstants.TurbomolecularPumpUpperPressureLimit)
                {
                    if(!HeatersEnabled) // if heaters turned off then turn them on
                    {
                        EnableOutput3or4(SourceRefreshConstants.S2LakeShoreHeaterOutput, true); // turn on heaters
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
                        EnableOutput3or4(SourceRefreshConstants.S2LakeShoreHeaterOutput, false); // turn heaters off
                    }
                }
                Thread.Sleep(SourceRefreshConstants.WarmupMonitoringWait);
            }

        }
        private void TurnOffCryoAndWarmupWithoutLakeShore()
        {
            if (!refreshModeCancelFlag) // If refresh mode has been cancelled then skip these functions
            {
                EnableCryoDigitalControl(false); // The pressure should be checked before this function is used (see process maps on OneNote)
                window.SetTextBox(window.tbHeaterTempSetpointStage2, refreshTemperature.ToString());
                UpdateStage2TemperatureSetpoint();
                window.SetTextBox(window.tbHeaterTempSetpointStage1, refreshTemperature.ToString());
                UpdateStage1TemperatureSetpoint();
                window.SetTextBox(window.tbRefreshModeStatus, "Starting warmup");
            }

            // Monitor the pressure as the source heats up. If the pressure gets too high for the turbo, then turn off the heaters. If pressure is low enough, then turn on the heaters.
            for (; ; )
            {
                if (refreshModeCancelFlag) break; // If refresh mode has been cancelled then exit this loop (check on every iteration of the loop)
                if (window.dateTimePickerStopHeatingAndTurnCryoOn.Value < DateTime.Now) break; // If the user requested that the cryo turns off before the temperature has reached the specified value, then exit this loop.
                TimeSpan TimeLeft = window.dateTimePickerStopHeatingAndTurnCryoOn.Value - DateTime.Now; // Update user interface with the time left until the cryo turns off.
                window.SetTextBox(window.tbHowLongUntilHeatingStopsAndCryoTurnsOn, TimeLeft.ToString(@"d\.hh\:mm\:ss")); // Update textbox to inform user how long is left until the heating process will be forced to stop early and the cryo turned on
                if (lastPressure < SourceRefreshConstants.TurbomolecularPumpUpperPressureLimit)
                {
                    if (!Stage1HeaterControlFlag | !Stage2HeaterControlFlag) // if heaters turned off then turn them on
                    {
                        StartStage1DigitalHeaterControl(); // turn heaters setpoint loop on
                        StartStage2DigitalHeaterControl(); // turn heaters setpoint loop on
                        EnableDigitalHeaters(1, true); // turn heaters on
                        EnableDigitalHeaters(2, true); // turn heaters on
                    }
                    if (Double.Parse(lastCellTemp) >= refreshTemperature) // If the source has reached the desired temperature, then break the loop
                    {
                        break;
                    }
                }
                else
                {
                    if (Stage1HeaterControlFlag | Stage2HeaterControlFlag) // if heaters are on
                    {
                        StopStage1DigitalHeaterControl(); // turn heaters setpoint loop off
                        StopStage2DigitalHeaterControl(); // turn heaters setpoint loop off
                        EnableDigitalHeaters(1, false); // turn heaters off
                        EnableDigitalHeaters(2, false); // turn heaters off
                    }
                }
                Thread.Sleep(SourceRefreshConstants.WarmupMonitoringWait); // Iterate the loop according to this time interval
                window.SetTextBox(window.tbRefreshModeStatus, "Still warming"); // Update refresh mode status textbox
            }
        }


        /// <summary>
        /// Incrementally warms up the source so that the turbomolecular pump isn't at risk of large quantities of neon from being evaporated quickly. 
        /// Once the source is warm enough, the risk becomes low and the cryo can be turned off.
        /// </summary>
        private void WarmUpSource()
        {
            EvaporateAndPumpNeon();
            TurnOffCryoAndWarmup();
        }
        private void WarmUpSourceWithoutLakeShore()
        {
            if (!refreshModeCancelFlag) EvaporateAndPumpNeonWithoutLakeShore();
            if (!refreshModeCancelFlag) TurnOffCryoAndWarmupWithoutLakeShore();
        }
        
        public void RefreshModeWait()
        {
            //If the source reaches the desired temperature before the user defined cryo turn off time, then wait until this time.
            for (; ; )
            {
                if (refreshModeCancelFlag) break; // If refresh mode has been cancelled then exit this loop (check on every iteration of the loop)
                if (window.dateTimePickerStopHeatingAndTurnCryoOn.Value < DateTime.Now) break; // If the user requested that the cryo turns off before the temperature has reached the specified value, then exit this loop.
                if (window.dateTimePickerRefreshModeTurnHeatersOff.Value < DateTime.Now) break; // Break loop when the user defined datetime is reached

                if (refreshAtRoomTemperature) // If the user has stated that the source should bake (maintain user defined temperature whilst cryo is off)
                {
                    if (Stage1HeaterControlFlag | Stage2HeaterControlFlag) // if heaters are on, turn them off
                    {
                        StopStage1DigitalHeaterControl(); // turn heaters setpoint loop off
                        StopStage2DigitalHeaterControl(); // turn heaters setpoint loop off
                        EnableDigitalHeaters(1, false); // turn heaters off
                        EnableDigitalHeaters(2, false); // turn heaters off
                        window.SetTextBox(window.tbRefreshModeStatus, "Waiting at room temperature"); // Update refresh mode status textbox
                    }
                }
                else
                {
                    if (!Stage1HeaterControlFlag | !Stage2HeaterControlFlag) // if heaters turned off, turn them on
                    {
                        StartStage1DigitalHeaterControl(); // turn heaters setpoint loop on
                        StartStage2DigitalHeaterControl(); // turn heaters setpoint loop on
                        EnableDigitalHeaters(1, true); // turn heaters on
                        EnableDigitalHeaters(2, true); // turn heaters on
                        window.SetTextBox(window.tbRefreshModeStatus, "Waiting at desired refreshing temperature"); // Update refresh mode status textbox
                    }
                }

                TimeSpan TimeLeft = window.dateTimePickerStopHeatingAndTurnCryoOn.Value - DateTime.Now; // Update user interface with the time left until the cryo turns off.
                window.SetTextBox(window.tbHowLongUntilHeatingStopsAndCryoTurnsOn, TimeLeft.ToString(@"d\.hh\:mm\:ss")); // Update textbox to inform user how long is left until the heating process will be forced to stop early and the cryo turned on
                Thread.Sleep(SourceRefreshConstants.WarmupMonitoringWait); // Iterate the loop according to this time interval
            }
        }

        private void CoolDownSource()
        {
            if(HeatersEnabled)
            {
                EnableOutput3or4(SourceRefreshConstants.S2LakeShoreHeaterOutput, false);
            }
            for(; ; )
            {
                if(Double.Parse(lastS1Temp) <= SourceRefreshConstants.CryoStartingTemperatureMax & Double.Parse(lastS2Temp) <= SourceRefreshConstants.CryoStartingTemperatureMax)
                { break; }
                Thread.Sleep(SourceRefreshConstants.CoolDownWait);
            }
            for(; ; )
            {
                if(lastPressure <= SourceRefreshConstants.CryoStartingPressure)
                { break; }
                Thread.Sleep(SourceRefreshConstants.CoolDownWait);
            }
            TurnOnCryoCooler();
        }
        private void CoolDownSourceWithoutLakeShore()
        {
            if (!refreshModeCancelFlag)
            {
                StopStage1DigitalHeaterControl(); // turn heaters setpoint loop off
                StopStage2DigitalHeaterControl(); // turn heaters setpoint loop off
                EnableDigitalHeaters(1, false); // turn heaters off
                EnableDigitalHeaters(2, false); // turn heaters off
                window.SetTextBox(window.tbRefreshModeStatus, "Stopping heaters"); // Update refresh mode status textbox
            }
            for(; ; )
            {
                if (refreshModeCancelFlag) break;
                window.SetTextBox(window.tbRefreshModeStatus, "Waiting for temperature to reach the safe operating range for cryo to turn on"); // Update refresh mode status textbox
                if(Double.Parse(lastS1Temp) <= SourceRefreshConstants.CryoStartingTemperatureMax & Double.Parse(lastS2Temp) <= SourceRefreshConstants.CryoStartingTemperatureMax)
                { break; }
                Thread.Sleep(SourceRefreshConstants.CoolDownWait);
            }
            for(; ; )
            {
                if (refreshModeCancelFlag) break;
                window.SetTextBox(window.tbRefreshModeStatus, "Waiting for the pressure to reach a low enough value for the cryo to turn on"); // Update refresh mode status textbox
                if(lastPressure <= SourceRefreshConstants.CryoStartingPressure)
                { break; }
                Thread.Sleep(SourceRefreshConstants.CoolDownWait);
            }
            if (!refreshModeCancelFlag)
            {
                window.SetTextBox(window.tbRefreshModeStatus, "Starting cryo");
                EnableCryoDigitalControl(true);
            }
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
            if(Channel == 1)
            {
                SetDigitalLine("heatersS1TriggerDigitalOutputTask", Enable);
                window.SetCheckBox(window.checkBoxEnableHeatersS1, Enable);
            }
            else
            {
                if(Channel == 2)
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
            Stage1HeaterControlFlag = false;
            window.EnableControl(window.btStartHeaterControlStage1, true);
            window.EnableControl(window.btStopHeaterControlStage1, false);
            window.EnableControl(window.checkBoxEnableHeatersS1, true);
        }
        public void StopStage2DigitalHeaterControl()
        {
            Stage2HeaterControlFlag = false;
            window.EnableControl(window.btStartHeaterControlStage2, true);
            window.EnableControl(window.btStopHeaterControlStage2, false);
            window.EnableControl(window.checkBoxEnableHeatersS2, true);
        }

        public void UpdateStage2TemperatureSetpoint()
        {
            Stage2TemperatureSetpoint = Double.Parse(window.tbHeaterTempSetpointStage2.Text);
        }
        public void UpdateStage1TemperatureSetpoint()
        {
            Stage1TemperatureSetpoint = Double.Parse(window.tbHeaterTempSetpointStage1.Text);
        }

        public void ControlHeaters()
        {
            if(Stage2HeaterControlFlag)
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
                    EnableDigitalHeaters(1,false);
                    EnableDigitalHeaters(2,false);
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

        private double lastPressure;
        private int pressureMovingAverageSampleLength = 10;
        private Queue<double> pressureSamples = new Queue<double>();
        private string sourceSeries = "Source Pressure";

        public void UpdatePressureMonitor()
        {
            //sample the pressure
            lastPressure = sourcePressureMonitor.Pressure;

            //add samples to Queues for averaging
            pressureSamples.Enqueue(lastPressure);

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
            lastPressure = sourcePressureMonitor.Pressure;
            DateTime localDate = DateTime.Now;

            //plot the most recent samples
            window.AddPointToChart(window.chart1, sourceSeries, localDate, lastPressure);
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

        private Thread pressureMonitorPollThread;
        private int pressureMonitorPollPeriod = 100;
        private int pressureMonitorLogPeriod = 1000;
        private int pressureLoggingRate;
        private bool pressureMonitorFlag;
        private Object pressureMonitorLock;

        internal void StartPressureMonitorPoll()
        {
            pressureMonitorPollThread = new Thread(new ThreadStart(pressureMonitorPollWorker));
            pressureMonitorPollPeriod = Int32.Parse(window.tbPressurePollPeriod.Text);
            pressureMovingAverageSampleLength = Int32.Parse(window.tbPressureSampleLength.Text);
            pressureMonitorLogPeriod = Int32.Parse(window.tbpressureMonitorLogPeriod.Text) * 1000; // Convert from seconds to milliseconds
            pressureLoggingRate = pressureMonitorLogPeriod / pressureMonitorPollPeriod;
            window.EnableControl(window.btStartPressureMonitorPoll, false);
            window.EnableControl(window.btStopPressureMonitorPoll, true);
            pressureSamples.Clear();
            pressureMonitorLock = new Object();
            pressureMonitorFlag = false;
            pressureMonitorPollThread.Start();
        }
        internal void StopPressureMonitorPoll()
        {
            pressureMonitorFlag = true;
        }
        private void pressureMonitorPollWorker()
        {
            int count = 0;

            for (; ; )// for (; ; ) is an infinite loop, equivalent to while(true)
            {
                Thread.Sleep(pressureMonitorPollPeriod);
                ++count;
                lock (pressureMonitorLock)
                {
                    UpdatePressureMonitor();
                    PlotLastPressure();
                    if (count == pressureLoggingRate)
                    {
                        PlotLastPressure();

                        if (window.cbLogPressureData.Checked)
                        {
                            pressureDataSerializer.AddData(new PressureMonitorDataLog(DateTime.Now,
                                pressureMonitorPollPeriod,
                                lastPressure));
                        }

                        count = 0;
                    }
                    if (pressureMonitorFlag)
                    {
                        pressureMonitorFlag = false;
                        break;
                    }
                }
            }
            window.EnableControl(window.btStartPressureMonitorPoll, true);
            window.EnableControl(window.btStopPressureMonitorPoll, false);
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
        public string lastSF6Temp;
        private string cellTSeries = "Cell Temperature";
        private string S1TSeries = "S1 Temperature";
        private string S2TSeries = "S2 Temperature";
        private string SF6TSeries = "SF6 Temperature";

        public void UpdateAllTempMonitors()
        {
            //sample the temperatures
            receivedData = tempController.GetTemperature(0,"K");
            TemperatureArray = receivedData.Split(',');
            if (TemperatureArray.Length == 8)
            {
                lastCellTemp = TemperatureArray[0];
                lastS1Temp = TemperatureArray[1];
                lastS2Temp = TemperatureArray[2];
                lastSF6Temp = TemperatureArray[3];
                window.SetTextBox(window.tbTCell, lastCellTemp);
                window.SetTextBox(window.tbTS1, lastS1Temp);
                window.SetTextBox(window.tbTS2, lastS2Temp);
                window.SetTextBox(window.tbTSF6, lastSF6Temp);
            }
            else
            {
                window.SetTextBox(window.tbTCell, "err_UpdateAllTempMonitors");
                window.SetTextBox(window.tbTS1, "err_UpdateAllTempMonitors");
                window.SetTextBox(window.tbTS2, "err_UpdateAllTempMonitors");
                window.SetTextBox(window.tbTSF6, "err_UpdateAllTempMonitors");
            }
        }

        public void PlotLastTemperatures()
        {
            DateTime localDate = DateTime.Now;
            double CellTemp = Double.Parse(lastCellTemp);
            double S1Temp = Double.Parse(lastS1Temp);
            double S2Temp = Double.Parse(lastS2Temp);
            double SF6Temp = Double.Parse(lastSF6Temp);

            //plot the most recent samples
            window.AddPointToChart(window.chart2, cellTSeries, localDate, CellTemp);
            window.AddPointToChart(window.chart2, S1TSeries, localDate, S1Temp);
            window.AddPointToChart(window.chart2, S2TSeries, localDate, S2Temp);
            window.AddPointToChart(window.chart2, SF6TSeries, localDate, SF6Temp);
        }



        private Thread tempMonitorPollThread;
        private int tempMonitorPollPeriod = 100;
        private bool tempMonitorFlag;
        private Object tempMonitorLock;

        internal void StartTempMonitorPoll()
        {
            tempMonitorPollThread = new Thread(new ThreadStart(tempMonitorPollWorker));
            tempMonitorPollPeriod = Int32.Parse(window.tbTempPollPeriod.Text);
            window.EnableControl(window.btStartTempMonitorPoll, false);
            window.EnableControl(window.btStopTempMonitorPoll, true);
            tempMonitorLock = new Object();
            tempMonitorFlag = false;
            tempMonitorPollThread.Start();
        }
        internal void StopTempMonitorPoll()
        {
            tempMonitorFlag = true;
        }
        private void tempMonitorPollWorker()
        {
            int count = 0;

            for (; ; )// for (; ; ) is an infinite loop, equivalent to while(true)
            {
                Thread.Sleep(tempMonitorPollPeriod);
                ++count;
                lock (tempMonitorLock)
                {
                    //UpdateAllTempMonitors(); 
                    UpdateAllTempMonitorsUsingDAQ();
                    if (tempMonitorFlag)
                    {
                        tempMonitorFlag = false;
                        break;
                    }
                }
            }
            window.EnableControl(window.btStartTempMonitorPoll, true);
            window.EnableControl(window.btStopTempMonitorPoll, false);
        }

        #endregion

        #region Temperature and Pressure Monitors using DAQ
        // Whilst the LakeShore is not in operation, the silicon diodes can still be monitored by measuring the voltage drop across them (which is temperature dependent).
        // This region contains the functions required to use this functionality.
        // Change the tempMonitorPollWorker() function in region "Temperature Monitors" to revert back to using the LakeShore Controller.


        double[] tempMonitorsData;

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
                window.SetTextBox(window.tbTS1, lastS1Temp);
                window.SetTextBox(window.tbTS2, lastS2Temp);
                window.SetTextBox(window.tbTSF6, lastSF6Temp);
            }
            else
            {
                window.SetTextBox(window.tbTCell, "err_UpdateAllTempMonitorsUsingDAQ");
                window.SetTextBox(window.tbTS1, "err_UpdateAllTempMonitorsUsingDAQ");
                window.SetTextBox(window.tbTS2, "err_UpdateAllTempMonitorsUsingDAQ");
                window.SetTextBox(window.tbTSF6, "err_UpdateAllTempMonitorsUsingDAQ");
            }
        }

        

        private Thread PTMonitorPollThread;
        private int PTMonitorPollPeriod = 1000;
        private bool PTMonitorFlag;
        private Object PTMonitorLock;

        internal void StartPTMonitorPoll()
        {
            PTMonitorPollThread = new Thread(() =>
            {
                PTMonitorPollWorker();
            });
            PTMonitorPollThread.IsBackground = true; // When the application is closed, this thread will also immediately stop. This is lazy coding, but it works and shouldn't cause any problems. This means it is a background thread of the main (UI) thread, so it will end with the main thread.
            PTMonitorPollPeriod = Int32.Parse(window.tbTandPPollPeriod.Text);
            pressureMovingAverageSampleLength = 10; 
            Stage2HeaterControlFlag = false;
            Stage1HeaterControlFlag = false;
            UpdateStage1TemperatureSetpoint();
            UpdateStage2TemperatureSetpoint();
            window.EnableControl(window.btStartTandPMonitoring, false);
            window.EnableControl(window.btStopTandPMonitoring, true);
            window.EnableControl(window.checkBoxSF6TempPlot, true);
            window.EnableControl(window.checkBoxS2TempPlot, true);
            window.EnableControl(window.checkBoxS1TempPlot, true);
            window.EnableControl(window.checkBoxCellTempPlot, true);
            window.EnableControl(window.checkBoxBeamlinePressurePlot, true);
            window.EnableControl(window.checkBoxSourcePressurePlot, true);
            window.EnableControl(window.btUpdateHeaterControlStage2, true);
            window.EnableControl(window.btStartHeaterControlStage2, true);
            window.EnableControl(window.btStartHeaterControlStage1, true);
            window.EnableControl(window.btUpdateHeaterControlStage1, true);
            window.EnableControl(window.btStartRefreshMode, true);
            pressureSamples.Clear();
            PTMonitorLock = new Object();
            PTMonitorFlag = false;
            PTMonitorPollThread.Start();
        }
        internal void StopPTMonitorPoll()
        {
            if(refreshModeActive)
            {
                MessageBox.Show("Refresh mode is currently active. To stop temperature and pressure monitoring, please first cancel refresh mode and ensure that the apparatus is in a safe state to be left unmonitored.", "Refresh Mode Exception", MessageBoxButtons.OK);
            }
            else
            {
                StopStage1DigitalHeaterControl();
                StopStage2DigitalHeaterControl();
                EnableDigitalHeaters(1, false);
                EnableDigitalHeaters(2, false);
                PTMonitorFlag = true;
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
                    UpdateAllTempMonitorsUsingDAQ();
                    PlotLastTemperatures();
                    UpdatePressureMonitor();
                    PlotLastPressure();
                    ControlHeaters();
                    if (PTMonitorFlag)
                    {
                        PTMonitorFlag = false;
                        break;
                    }
                }
            }
            window.EnableControl(window.btStartTandPMonitoring, true);
            window.EnableControl(window.btStopTandPMonitoring, false);
            window.EnableControl(window.checkBoxSF6TempPlot, false);
            window.EnableControl(window.checkBoxS2TempPlot, false);
            window.EnableControl(window.checkBoxS1TempPlot, false);
            window.EnableControl(window.checkBoxCellTempPlot, false);
            window.EnableControl(window.checkBoxBeamlinePressurePlot, false);
            window.EnableControl(window.checkBoxSourcePressurePlot, false);
            window.EnableControl(window.btUpdateHeaterControlStage2, false);
            window.EnableControl(window.btStopHeaterControlStage2, false);
            window.EnableControl(window.btStartHeaterControlStage2, false);
            window.EnableControl(window.btStopHeaterControlStage1, false);
            window.EnableControl(window.btStartHeaterControlStage1, false);
            window.EnableControl(window.btUpdateHeaterControlStage1, false);
            Stage2HeaterControlFlag = false;
            Stage1HeaterControlFlag = false;
        }

        #endregion

        #region Plotting functions

        public void ChangePlotYAxisScale(int ChartNumber)
        {
            if(ChartNumber == 1)
            {
                string YScale = window.comboBoxPlot1ScaleY.Text; // Read the Y scale mode chosen by the user in the UI
                window.ChangeChartYScale(window.chart1, YScale);
                window.SetAxisYIsStartedFromZero(window.chart1, false);
            }
            else
            {
                if(ChartNumber == 2)
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
    }
}
