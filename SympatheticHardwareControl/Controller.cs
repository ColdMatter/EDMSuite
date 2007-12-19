using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using System.Windows.Forms;
using DAQ.Environment;
using DAQ.HAL;
using NationalInstruments.DAQmx;
using System.Threading;
using System.Timers;

namespace SympatheticHardwareControl
{
    /// <summary>
    /// A hardware controller specific to the sympathetic cooling experiment
    /// </summary>
    public class Controller : MarshalByRefObject
    {

        private static System.Threading.Timer stateTimer;

        ControlWindow window;
        Lesker903Gauge atomSourceGauge1;
        Lesker903Gauge atomSourceGauge2;
        Hashtable digitalTasks = new Hashtable();

        // without this method, any remote connections to this object will time out after
        // five minutes of inactivity.
        // It just overrides the lifetime lease system completely.
        public override Object InitializeLifetimeService()
        {
            return null;
        }
        
        //get things started up
        public void Start()
        {
            if (!Environs.Debug)
            {
                //set up the pressure gauges
                atomSourceGauge1 = new Lesker903Gauge("gauge 1", "atomSourcePressure1");
                atomSourceGauge2 = new Lesker903Gauge("gauge 2", "atomSourcePressure2");

                //create the digital tasks
                CreateDigitalTask("hplusBurstEnable");
                CreateDigitalTask("hminusBurstEnable");
                CreateDigitalTask("vplusBurstEnable");
                CreateDigitalTask("vminusBurstEnable");
                CreateDigitalTask("hplusdc");
                CreateDigitalTask("hminusdc");
                CreateDigitalTask("vplusdc");
                CreateDigitalTask("vminusdc");
            }
            
            // make the window
            window = new ControlWindow();
            window.controller = this;
            Application.Run(window);
        }

        //This method runs as soon as the window has been created. Put stuff to start up in here
        internal void WindowLoaded()
        {
            startRepetitiveTasks();
        }
          
        public void startRepetitiveTasks()
        {
            // Create the delegate that invokes methods for the timer.
            TimerCallback pressureCallback = new TimerCallback(updatePressures);
            stateTimer = new System.Threading.Timer(pressureCallback, null, 0, 500);     
         }

        private object updatePressureLock = new object();
        public void updatePressures(Object obj)
        {
            double pressure1;
            double pressure2;

            lock (updatePressureLock)
            {
                if (!Environs.Debug) //do this if we have the hardware
                {
                    //get the pressures
                    pressure1 = atomSourceGauge1.Pressure;
                    pressure2 = atomSourceGauge2.Pressure;
                    //set the warning lights
                    window.setLedState(window.led1, atomSourceGauge1.OverPressure);
                    window.setLedState(window.led2, atomSourceGauge2.OverPressure);
                }
                else //this for debugging on a computer without the hardware
                {
                    Random rand = new Random();
                    pressure1 = 1E-5 * rand.NextDouble();
                    pressure2 = 1E-5 * rand.NextDouble();
                }
                //write out the latest pressure values
                window.outputAIdata(window.pressureIndicator1, pressure1.ToString("0.##E+0"));
                window.outputAIdata(window.pressureIndicator2, pressure2.ToString("0.##E+0"));
            }
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

        public void EnableHVBurst(string channelName, bool state)
        {
            if (!Environs.Debug)
            {
                SetDigitalLine(channelName, state);
            }
            else
            {
                Console.WriteLine("Set " + channelName + " to " + state.ToString());
            }
        }

        public void SetHVSwitchState(string channelName, bool state)
        {
            if (!Environs.Debug)
            {
                SetDigitalLine(channelName, state);
            }
            else
            {
                Console.WriteLine("Set " + channelName + " to " + state.ToString());
            }
        }

                        
    }
}
