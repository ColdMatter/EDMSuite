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

namespace TransferCavityLock
{
    /// <summary>
    /// A class for locking the laser using a transfer cavity.
    /// </summary>
    public class DeadBolt : MarshalByRefObject
    {

        #region Definitions

        private const double UPPER_LC_VOLTAGE_LIMIT = 10.0; //volts LC: Laser control
        private const double LOWER_LC_VOLTAGE_LIMIT = -10.0; //volts LC: Laser control
        private const double UPPER_CC_VOLTAGE_LIMIT = 10.0; //volts CC: Cavity control
        private const double LOWER_CC_VOLTAGE_LIMIT = 0; //volts CC: Cavity control
        private const double UPPER_SETPOINT_LIMIT = 100.0; //volts
        private const double LOWER_SETPOINT_LIMIT = -100.0; //volts

        private const int HARDWARE_CONTROL_TALK_PERIOD = 2000; //milliseconds

        private ScanMaster.Controller scanMaster;
        private ScanMaster.Analyze.GaussianFitter fitter;
        private DecelerationHardwareControl.Controller hardwareControl;

        private MainForm ui;

        private Task outputLaserTask; //Some stuff to let you write to laser
        private AnalogOutputChannel laserChannel;
        private AnalogSingleChannelWriter laserWriter;

        private Task outputCavityTask; //Some stuff to let you write to piezo driver
        private AnalogOutputChannel cavityChannel;
        private AnalogSingleChannelWriter cavityWriter;

        private Task inputLaserTask;
        private AnalogInputChannel p1Channel; //p1 is the signal from the laser we are trying to lock
        private AnalogSingleChannelReader p1Reader;
        //private AnalogMultiChannelReader cavityReader;

        private Task inputRefLaserTask;
        private AnalogInputChannel p2Channel; //p2 is the signal from the reference He-Ne lock
        private AnalogSingleChannelReader p2Reader;

        public enum ControllerState { free, busy, stopping }; //Don't know what this is. Inherited from Mike's LaserLock
        private ControllerState status = ControllerState.free;

        private System.Threading.Timer hardwareControlTimer;
        private TimerCallback timerDelegate;

        private double[] latestData;
        private double[] latestrefData;

        public string RampChannel;
        public string RampTriggerMethod;
        public bool Ramping = false;

        // without this method, any remote connections to this object will time out after
        // five minutes of inactivity.
        // It just overrides the lifetime lease system completely.
        public override Object InitializeLifetimeService()
        {
            return null;
        }

        #endregion

        #region Setup

        public void Start()
        {
            ui = new MainForm();
            ui.controller = this;

            // get access to ScanMaster and the DecelerationHardwareController
            RemotingHelper.ConnectScanMaster();
            RemotingHelper.ConnectDecelerationHardwareControl();

            scanMaster = new ScanMaster.Controller();
            hardwareControl = new DecelerationHardwareControl.Controller();
            fitter = new ScanMaster.Analyze.GaussianFitter();

            if (!Environs.Debug)
            {
                outputLaserTask = new Task("FeedbackToLaser");
                laserChannel =
                        (AnalogOutputChannel)Environs.Hardware.AnalogOutputChannels["laser"];
                laserChannel.AddToTask(outputLaserTask, -10, 10);
                outputLaserTask.Control(TaskAction.Verify);
                laserWriter = new AnalogSingleChannelWriter(outputLaserTask.Stream);

                outputCavityTask = new Task("CavityPiezoVoltage");
                cavityChannel =
                        (AnalogOutputChannel)Environs.Hardware.AnalogOutputChannels["cavity"];
                cavityChannel.AddToTask(outputCavityTask, 0, 10);
                outputCavityTask.Control(TaskAction.Verify);
                cavityWriter = new AnalogSingleChannelWriter(outputCavityTask.Stream);

                inputLaserTask = new Task("CavityPeaksFromLaser");
                p1Channel = (AnalogInputChannel)Environs.Hardware.AnalogInputChannels["p1"];
                p1Channel.AddToTask(inputLaserTask, 0, 10);
                p1Reader = new AnalogSingleChannelReader(inputLaserTask.Stream);

                inputRefLaserTask = new Task("CavityPeaksFromRefLaser");
                p2Channel = (AnalogInputChannel)Environs.Hardware.AnalogInputChannels["p2"];
                p2Channel.AddToTask(inputRefLaserTask, 0, 10);
                p2Reader = new AnalogSingleChannelReader(inputRefLaserTask.Stream);
            }

            Application.Run(ui);
        }

        #endregion

        #region Public properties

        public ControllerState Status
        {
            get { return status; }
            set { status = value; }
        }

        // the getter asks the hardware controller for the laser frequency control voltage
        // the setter sets the value and tells the hardware controller about it
        public double LaserVoltage
        {
            get
            {
                return hardwareControl.LaserFrequencyControlVoltage;
            }
            set
            {
                if (value >= LOWER_LC_VOLTAGE_LIMIT && value <= UPPER_LC_VOLTAGE_LIMIT)
                {
                    if (!Environs.Debug)
                    {
                        laserWriter.WriteSingleSample(true, value);
                        outputLaserTask.Control(TaskAction.Unreserve);
                    }
                    else
                    {
                        // Debug mode, do nothing
                    }
                    hardwareControl.LaserFrequencyControlVoltage = value;
                }
                else
                {
                    // Out of range, do nothing
                }
            }
        }
        // the getter asks the hardware controller for the cavity control voltage
        // the setter sets the value and tells the hardware controller about it
        public double CavityVoltage
        {
            get
            {
                return hardwareControl.CavityControlVoltage;
            }
            set
            {
                if (value >= LOWER_CC_VOLTAGE_LIMIT && value <= UPPER_CC_VOLTAGE_LIMIT)
                {
                    if (!Environs.Debug)
                    {
                        cavityWriter.WriteSingleSample(true, value);
                        outputCavityTask.Control(TaskAction.Unreserve);
                    }
                    else
                    {
                        // Debug mode, do nothing
                    }
                    hardwareControl.CavityControlVoltage = value;
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
        /// A function to repeatedly scan the voltage. It's meant to be triggered by pressing the
        /// "start ramping" button, and only stop when "stop ramping" is pressed.
        /// </summary>

        public void ScanVoltage(int delayAtEachStep, string aoChannel)
        {
            AnalogSingleChannelWriter writeChannel;
            double readChannel;
            Task taskChannel;
            double[] r;
                        
            if (aoChannel == "laser")
            {
                r = new double[2];
                r[0] = LOWER_LC_VOLTAGE_LIMIT;
                r[1] = UPPER_LC_VOLTAGE_LIMIT;
                writeChannel = laserWriter;
                readChannel = hardwareControl.LaserFrequencyControlVoltage;
                taskChannel = outputLaserTask;
                ui.AddToTextBox("Starting ramps");
                while (this.Ramping == true)
                {
                    scanVoltageRange(r[0], r[1], writeChannel, readChannel, taskChannel, delayAtEachStep);
                }
            }
            if (aoChannel == "cavity")
            {
                r = new double[2];
                r[0] = LOWER_CC_VOLTAGE_LIMIT;
                r[1] = UPPER_CC_VOLTAGE_LIMIT;
                writeChannel = cavityWriter;
                readChannel = hardwareControl.CavityControlVoltage;
                taskChannel = outputCavityTask;
                ui.AddToTextBox("Starting ramps");
                while (this.Ramping == true)
                {
                    scanVoltageRange(r[0], r[1], writeChannel, readChannel, taskChannel, delayAtEachStep);
                }
            }
            else
            {
                //Do nothing
            }
            
        }

        #endregion

        #region Private methods

        /// <summary>
        /// A generic ramping function. Can be used to ramp either the cavity or the laser. 
        /// Need to tell it the final voltage "v" and which channels to do the ramp on.
        /// </summary>

        private void rampToVoltage(double v, AnalogSingleChannelWriter writeChannel, 
            double readChannel, Task taskChannel, int delayAtEachStep)
        {
            int steps = 20;
            double voltage = readChannel;
            double stepsize = (v - voltage) / steps;

            for (int i = 1; i <= steps; i++)
            {
                voltage += stepsize;
                writeChannel.WriteSingleSample(true, voltage);  //Write new value to channel
                readChannel = voltage;          //Tell hardware control about it
                Thread.Sleep(delayAtEachStep);
            }
            taskChannel.Control(TaskAction.Unreserve);
        }

        /// <summary>
        /// A function to scan across the voltage range set by the limits high and low.
        /// </summary>

        private void scanVoltageRange(double low, double high, AnalogSingleChannelWriter writeChannel, 
            double readChannel, Task taskChannel, int delayAtEachStep)
        {
            double voltage = low;
            writeChannel.WriteSingleSample(true, low);  //Set initial voltage to low
            readChannel = voltage;          //Tell hardware control about it

            rampToVoltage(high, writeChannel, readChannel, taskChannel, delayAtEachStep);
        }


        #endregion
    }
}
