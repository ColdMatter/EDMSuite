using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Lifetime;
using Data;
using Data.Scans;
using NationalInstruments.DAQmx;
using DAQ.Environment;
using DAQ.HAL;
using DAQ.Remoting;
using System.Windows.Forms;

namespace DecelerationLaserLock
{
    /// <summary>
    /// A class for controlling the laser frequency. Contains a method for locking the laser to a stabilized
    /// reference cavity.
    /// </summary>
    public class LaserController : MarshalByRefObject
    {

        private const double UPPER_VOLTAGE_LIMIT = 10.0; //volts
        private const double LOWER_VOLTAGE_LIMIT = -10.0; //volts
        private const double UPPER_SETPOINT_LIMIT = 100.0; //volts
        private const double LOWER_SETPOINT_LIMIT = -100.0; //volts
        private const int SAMPLES_PER_READ = 10;
        private const int READS_PER_FEEDBACK = 4;
        private const int SLEEPING_TIME = 500; //milliseconds
        private const int LATENCY = 10000; //milliseconds
        private const int CAVITY_FWHM = 150; //MHz
        private const double CAVITY_PEAK_HEIGHT = 4.0; //volts
        private const int LASER_SCAN_CALIBRATION = 200; //MHz/volt

        private const int HARDWARE_CONTROL_TALK_PERIOD = 2000; //milliseconds
        
        private double proportionalGain;
        private double integralGain;
//        private double derivativeGain;
        private double setPoint;
        private double deviation;
        private double integratedDeviation;
                
        private ScanMaster.Controller scanMaster;
        private DAQ.Analyze.GaussianFitter fitter;
        private MoleculeMOTHardwareControl.MainController hardwareControl;
        
        private MainForm ui;

        private Task outputTask;
        private AnalogOutputChannel laserChannel;
        private AnalogSingleChannelWriter laserWriter;

        private Task inputTask;
        private AnalogInputChannel cavityChannel;
        private AnalogSingleChannelReader cavityReader;
        //private AnalogMultiChannelReader cavityReader;

        private Task inputrefTask;
        private AnalogInputChannel cavityrefChannel;
        private AnalogSingleChannelReader cavityrefReader;

        public enum ControllerState { free, busy, stopping };
        private ControllerState status = ControllerState.free;

        private System.Threading.Timer hardwareControlTimer;
        private TimerCallback timerDelegate;

        private double[] latestData;
        private double[] latestrefData;

        // without this method, any remote connections to this object will time out after
        // five minutes of inactivity.
        // It just overrides the lifetime lease system completely.
        public override Object InitializeLifetimeService()
        {
            return null;
        }

        #region Setup

        public void Start()
        {
            proportionalGain = 0;
            integralGain = 0;
//            derivativeGain = 0;
                        
            ui = new MainForm();
            ui.controller = this;

            // get access to ScanMaster and the DecelerationHardwareController
            RemotingHelper.ConnectScanMaster();
            RemotingHelper.ConnectMoleculeMOTHardwareControl();

            scanMaster = new ScanMaster.Controller();
            hardwareControl = new MoleculeMOTHardwareControl.MainController();
            fitter = new DAQ.Analyze.GaussianFitter();

           
            if (!Environs.Debug)
            {
                outputTask = new Task("LaserControllerOutput");
                laserChannel =
                        (AnalogOutputChannel)Environs.Hardware.AnalogOutputChannels["laser"];
                laserChannel.AddToTask(outputTask, -10, 10);
                outputTask.Control(TaskAction.Verify);
                laserWriter = new AnalogSingleChannelWriter(outputTask.Stream);

                inputTask = new Task("LaserControllerInput");
                cavityChannel = (AnalogInputChannel)Environs.Hardware.AnalogInputChannels["lockcavity"];
                cavityChannel.AddToTask(inputTask, -10, 10);
                cavityReader = new AnalogSingleChannelReader(inputTask.Stream);

                inputrefTask = new Task("ReferenceLaserControllerInput");
                cavityrefChannel = (AnalogInputChannel)Environs.Hardware.AnalogInputChannels["refcavity"];
                cavityrefChannel.AddToTask(inputrefTask, -10, 10);
                cavityrefReader = new AnalogSingleChannelReader(inputrefTask.Stream);
            }

            timerDelegate = new TimerCallback(TalkToHardwareControl);
            hardwareControlTimer = new System.Threading.Timer(timerDelegate, null, 5000, HARDWARE_CONTROL_TALK_PERIOD);

            Application.Run(ui);
        }

        #endregion

        #region Public properties

        public ControllerState Status
        {
            get { return status; }
            set { status = value; }
        }

        public double SetPoint
        {
            get { return setPoint; }
            set { setPoint = value; }
        }

        // the getter asks the hardware controller for the laser frequency control voltage
        // the setter sets the value and tells the hardware controller and the front-panel control about it
        public double LaserVoltage
        {
            get
            {
                return hardwareControl.LaserFrequencyControlVoltage;
            }
            set
            {
                if (value >= LOWER_VOLTAGE_LIMIT && value <= UPPER_VOLTAGE_LIMIT)
                {
                    if (!Environs.Debug)
                    {
                        laserWriter.WriteSingleSample(true, value);
                        outputTask.Control(TaskAction.Unreserve);
                    }
                    else
                    {
                        // Debug mode, do nothing
                    }
                    hardwareControl.LaserFrequencyControlVoltage = value;
                    ui.SetControlVoltageNumericEditorValue(value);
                }
                else
                {
                    // Out of range, do nothing
                }
            }
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Parks the laser on a resonance. Uses ScanMaster to scan the laser and fit to the spectrum  in order
        /// to locate the resonance. Then adjusts the laser frequency to park the laser on the resonance.
        /// Note that this method parks but doesn't lock the laser
        /// </summary>
        public void Park()
        {
            double[] fitresult;
            ui.AddToTextBox("Attempting to park..." + Environment.NewLine);
            status = ControllerState.busy;
            try
            {
                scanMaster.AcquireAndWait(ui.ScansPerPark);
                Scan scan = scanMaster.DataStore.AverageScan;
                if (scan.Points.Count != 0)
                {
                    fitresult = FitSpectrum(scan);
                    double centreVoltage = fitresult[2];
                    double scanStart = (double)scanMaster.GetOutputSetting("start");
                    double scanEnd = (double)scanMaster.GetOutputSetting("end");
                    if (centreVoltage > scanStart && centreVoltage < scanEnd)
                    {
                        if (!Environs.Debug)
                        {
                            RampToVoltage(centreVoltage);
                            ui.AddToTextBox("Parked at " + centreVoltage + " volts." + Environment.NewLine);
                            ui.SetControlVoltageNumericEditorValue(centreVoltage);
                        }
                        else ui.AddToTextBox("Ramping to " + centreVoltage + " volts. \n");
                    }
                    else ui.AddToTextBox("Failed - Unable to locate the resonance." + Environment.NewLine);
                }
                else ui.AddToTextBox("Failed - Nothing to fit." + Environment.NewLine);
            }
            catch (System.Net.Sockets.SocketException)
            {
                ui.AddToTextBox("The connection to ScanMaster was refused. Make sure that ScanMaster is running." + Environment.NewLine);
            }
            status = ControllerState.free;
        }
        
        /// <summary>
        /// Locks the laser to a reference cavity. This method runs continuously until the laser is unlocked.
        /// When this method is called, the reference cavity is read in order to establish the lock point.
        /// The reference cavity is then read continuously and adjustments fed-back to the laser.
        /// </summary>
        public void Lock()
        {
            double singleValue;
            double singlerefValue;
            double averageValue = 0;
            int reads = 0;
            bool firstTime = true;
            DateTime oldCavityStamp = new DateTime();
            DateTime newCavityStamp = new DateTime();
            newCavityStamp = DateTime.Now;
            
            status = ControllerState.busy;
            ui.ControlVoltageEditorEnabledState(false);
            hardwareControl.LaserLocked = true;
            hardwareControl.SetAnalogOutputBlockedStatus("laser", true);
            
            while (status == ControllerState.busy)
            {
                if (!Environs.Debug)
                {
                    oldCavityStamp = newCavityStamp;
                    // read the cavity
                    if (hardwareControl.AnalogInputsAvailable)//scanmaster not running
                    {
                        singleValue = 0;
                        inputTask.Start();
                        latestData = cavityReader.ReadMultiSample(SAMPLES_PER_READ);
                        inputTask.Stop();

                        singlerefValue = 0;
                        inputrefTask.Start();
                        latestrefData = cavityrefReader.ReadMultiSample(SAMPLES_PER_READ);
                        inputrefTask.Stop();

                        foreach (double d in latestData) singleValue += d;
                        foreach (double e in latestrefData) singlerefValue += e;
                        singleValue = singleValue / SAMPLES_PER_READ;
                        singlerefValue = singlerefValue / SAMPLES_PER_READ;
                        if (singleValue > 4.7)
                        {
                            hardwareControl.diodeSaturationError();
                        }
                        else
                        {
                            hardwareControl.diodeSaturation();
                        }
                        singleValue = singleValue / singlerefValue;
                        newCavityStamp = DateTime.Now;
                        hardwareControl.UpdateLockCavityData(singleValue);
                        //hardwareControl.UpdateLockCavityData(singleValue / singlerefValue);
                    }
                    else
                    {
                        singleValue = hardwareControl.LastCavityData;
                        newCavityStamp = hardwareControl.LastCavityTimeStamp;
                    }
                    
                    // provided the last cavity read is recent, do something with it
                    if (hardwareControl.TimeSinceLastCavityRead < LATENCY && newCavityStamp.CompareTo(oldCavityStamp) != 0)
                    {
                        // if this is the first read since throwing the lock, the result defines the set-point
                        if (firstTime)
                        {
                            integratedDeviation = 0;
                            // make sure we don't have a value that's out of range
                            if (singleValue > UPPER_SETPOINT_LIMIT) singleValue = UPPER_SETPOINT_LIMIT;
                            if (singleValue < LOWER_SETPOINT_LIMIT) singleValue = LOWER_SETPOINT_LIMIT;
                            setPoint = singleValue;
                            ui.SetSetPointNumericEditorValue(setPoint);
                            firstTime = false;
                        }
                        // otherwise, use the last read to update the running average
                        else
                        {
                            averageValue += singleValue;
                            reads++;
                            deviation = averageValue / reads - setPoint;
                            integratedDeviation = integratedDeviation + deviation * hardwareControl.TimeSinceLastCavityRead;
                        }
                        // is it time to feed-back to the laser
                        if (reads != 0 && (reads >= READS_PER_FEEDBACK || ui.SpeedSwitchState))
                        {
                            averageValue = averageValue / reads;
                            deviation = averageValue - setPoint;
                            LaserVoltage = LaserVoltage + SignOfFeedback * proportionalGain * deviation + 
                                integralGain * integratedDeviation; //other terms to go here 
                            // update the deviation plot
                            ui.DeviationPlotXYAppend(deviation);
                            // reset the variables
                            averageValue = 0;
                            reads = 0;
                        }
                    }
                }           
                else
                {
                    // Debug mode
                    ui.DeviationPlotXYAppend(hardwareControl.LaserFrequencyControlVoltage - setPoint);
                }
                Thread.Sleep(SLEEPING_TIME);
            }
            // we're out of the while loop - revert to the unlocked state
            status = ControllerState.free;
            hardwareControl.LaserLocked = false;
            ui.ControlVoltageEditorEnabledState(true);
            hardwareControl.SetAnalogOutputBlockedStatus("laser", false);
        }

        public void SetProportionalGain(double frontPanelValue)
        {
            // the pre-factor of 0.2 is chosen so that a front-panel setting of 5 (mid-range) is close to the threshold for oscillation
            // empirical evidence says a value of 0.4 makes 5 the oscillation threshold
            proportionalGain = 0.4 * CAVITY_FWHM / (CAVITY_PEAK_HEIGHT * LASER_SCAN_CALIBRATION) * frontPanelValue;
        }

        public void SetIntegralGain(double frontPanelValue)
        {
            // integralGain = proportionalGain / T where T is the integral time. hardwareControl.TimeSinceLastCavityRead is used
            // below which is probably too short, so prefactor should be << 1 ?
            integralGain = 0.2 * CAVITY_FWHM / (CAVITY_PEAK_HEIGHT * LASER_SCAN_CALIBRATION * hardwareControl.TimeSinceLastCavityRead) * frontPanelValue;
        }

        #endregion

        #region Private methods

        private void RampToVoltage(double v)
        {
            int steps = 20;
            int delayAtEachStep = 50;
            double laserVoltage = hardwareControl.LaserFrequencyControlVoltage;
            double stepsize = (v - laserVoltage) / steps;
           
            for (int i = 1; i <= steps; i++)
            {
                laserVoltage += stepsize;
                laserWriter.WriteSingleSample(true, laserVoltage);
                hardwareControl.LaserFrequencyControlVoltage = laserVoltage;
                Thread.Sleep(delayAtEachStep);
            }
            outputTask.Control(TaskAction.Unreserve);
        }

        private double[] FitSpectrum(Scan s)
        {
            double[] xDat = s.ScanParameterArray;
            double scanStart = xDat[0];
            double scanEnd = xDat[xDat.Length - 1];
            TOF avTof = (TOF)s.GetGatedAverageOnShot(scanStart, scanEnd).TOFs[0];
            double gateStart = avTof.GateStartTime;
            double gateEnd = avTof.GateStartTime + avTof.Length * avTof.ClockPeriod;
            double[] yDat = s.GetTOFOnIntegralArray(0, gateStart, gateEnd);
            fitter.Fit(xDat, yDat, fitter.SuggestParameters(xDat, yDat, scanStart, scanEnd));
            string report = fitter.ParameterReport;

            string[] tokens = report.Split(' ');

            double[] fitresult = new double[4];
            for (int i = 0; i < fitresult.Length; i++) fitresult[i] = Convert.ToDouble(tokens[2 * i + 1]);

            return fitresult;

        }     

        // returns -1 for locking to positve slope, +1 for locking to negative slope
        private int SignOfFeedback
        {
            get
            {
                if (ui.SlopeSwitchState) return -1;
                else return +1;
            }
        }

        private void TalkToHardwareControl(Object stateInfo)
        {
            //ui.SetControlVoltageNumericEditorValue = hardwareControl.LaserFrequencyControlVoltage;
            ui.SetLockCheckBox(hardwareControl.LaserLocked);
        }

        #endregion

    }
}
