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

        //private ScanMaster.Controller scanMaster;
        //private ScanMaster.Analyze.GaussianFitter fitter;
        //private SympatheticHardwareControl.Controller hardwareControl;

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

        private string rampChannel = "laser";
        private string rampTriggerMethod = "int";
        private volatile bool ramping = false;
        public object rampStopLock = new object();
        
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

        // the getter asks the hardware controller for the laser frequency control voltage
        // the setter sets the value and tells the hardware controller about it
       // public double LaserVoltage
       // {
       //     get
       //     {
       //         return hardwareControl.LaserFrequencyControlVoltage;
       //     }
       //     set
       //     {
       //         if (value >= LOWER_LC_VOLTAGE_LIMIT && value <= UPPER_LC_VOLTAGE_LIMIT)
       //         {
       //             if (!Environs.Debug)
       //             {
       //                 laserWriter.WriteSingleSample(true, value);
       //                 outputLaserTask.Control(TaskAction.Unreserve);
       //             }
       //             else
       //             {
       //                // Debug mode, do nothing
       //             }
                    //hardwareControl.LaserFrequencyControlVoltage = value;
       //         }
       //         else
       //         {
       //            // Out of range, do nothing
       //         }
       //    }
       // }
        // the getter asks the hardware controller for the cavity control voltage
        // the setter sets the value and tells the hardware controller about it
       // public double CavityVoltage
       // {
       //     get
       //     {
       //         return hardwareControl.CavityControlVoltage;
       //     }
       //     set
       //     {
       //         if (value >= LOWER_CC_VOLTAGE_LIMIT && value <= UPPER_CC_VOLTAGE_LIMIT)
       //         {
       //             if (!Environs.Debug)
       //             {
       //                 cavityWriter.WriteSingleSample(true, value);
       //                 outputCavityTask.Control(TaskAction.Unreserve);
       //             }
       //             else
       //             {
       //                 // Debug mode, do nothing
       //             }
       //             //hardwareControl.CavityControlVoltage = value;
       //         }
       //         else
       //         {
       //             // Out of range, do nothing
       //         }
       //     }
       // }

        #endregion

        #region Public methods

        /// <summary>
        /// A function to repeatedly scan the voltage. It's meant to be triggered by pressing the
        /// "start ramping" button, and only stop when "stop ramping" is pressed.
        /// </summary>

        public void startRamp()
        {
            Thread rampThread = new Thread(new ThreadStart(rampLoop));
            rampThread.Start();
            //rampLoop();
        }
        

        

        /// <summary>
        /// A few functions for setting and getting the values rampTriggerMethod, rampChannel and ramping.
        /// </summary>
        public string RampTriggerMethod
        {
            set
            {
                rampTriggerMethod = value;
            }
            get { return rampTriggerMethod; }
        }
        public string RampChannel
        {
            set
            {
                rampChannel = value;
            }
            get { return rampChannel; }
        }
        public bool Ramping
        {
            set
            {
                ramping = value;
            }
            get { return ramping; }
        }
        
        #endregion

        #region Private methods

        /// <summary>
        /// A function to scan across the voltage range set by the limits high and low.
        /// </summary>

        private void scanVoltageRange(double low, double high, AnalogSingleChannelWriter writeChannel, 
            double readChannel, Task taskChannel, int delayAtEachStep)
        {
            int steps = 100;
            double stepsize = (high - low) / steps;
            double voltage = low;

            writeChannel.WriteSingleSample(true, low);  //Set initial voltage to low

            for (int i = 1; i <= steps; i++)
            {
                voltage += stepsize;
                writeChannel.WriteSingleSample(true, voltage);  //Write new value to channel
                Thread.Sleep(delayAtEachStep);
            }
        }

        private void scanFullVoltageRange(int delayAtEachStep, string aoChannel)
        {
            AnalogSingleChannelWriter writeChannel;
            double readChannel = 0;
            Task taskChannel;
            double[] r;
                        
            if (aoChannel == "laser")
            {
                r = new double[2];
                r[0] = LOWER_LC_VOLTAGE_LIMIT;
                r[1] = UPPER_LC_VOLTAGE_LIMIT;
                writeChannel = laserWriter;
           //     readChannel = hardwareControl.LaserFrequencyControlVoltage;
                taskChannel = outputLaserTask;
                ui.AddToTextBox("Starting ramps");
                while (ramping == true)
                {
                    scanVoltageRange(r[0], r[1], writeChannel, readChannel, taskChannel, delayAtEachStep);
                    if(ramping == false)
                    { break; }
                    else{};
                }
            }
            if (aoChannel == "cavity")
            {
                r = new double[2];
                r[0] = LOWER_CC_VOLTAGE_LIMIT;
                r[1] = UPPER_CC_VOLTAGE_LIMIT;
                writeChannel = cavityWriter;
            //    readChannel = hardwareControl.CavityControlVoltage;
                taskChannel = outputCavityTask;
                ui.AddToTextBox("Starting ramps");
                
                while (ramping == true)
                {
                    scanVoltageRange(r[0], r[1], writeChannel, readChannel, taskChannel, delayAtEachStep);
                    if (ramping == false)
                    { break; }
                    else { };
                }
            }
            else
            {
                //Do nothing
            }
            
        }

        private void rampLoop()
        {
            for(; ; )
            {
                ui.AddToTextBox("Starting to ramp");
                scanFullVoltageRange(1, rampChannel);
                lock (rampStopLock)
                {
                    if (ramping == false)
                    {
                        if (rampChannel == "laser")
                        {
                            laserWriter.WriteSingleSample(true, 0);
                        }
                        if (rampChannel == "cavity")
                        {
                            cavityWriter.WriteSingleSample(true, 0);
                        }
                        return;
                    }
                }
            }
        }

        #endregion
    }
    


}
