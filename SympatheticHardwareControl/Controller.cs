using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using DAQ.Environment;
using DAQ.HAL;
using NationalInstruments.DAQmx;
using System.Threading;
using System.Timers;

namespace SympatheticHardwareControl
{
    public class Controller : MarshalByRefObject
    {

        private static System.Threading.Timer stateTimer;

        ControlWindow window;
        Lesker903Gauge atomSourceGauge1;
        Lesker903Gauge atomSourceGauge2;
        
        public void Start()
        {
            window = new ControlWindow();
            window.controller = this;
            Application.Run(window);
        }

        public override Object InitializeLifetimeService()
        {
            return null;
        }

        //This method runs as soon as the window has been created. Put stuff to start up in here
        internal void WindowLoaded()
        {
            atomSourceGauge1 = new Lesker903Gauge("gauge 1", "atomsourcepressure1");
            atomSourceGauge2 = new Lesker903Gauge("gauge 2", "atomsourcepressure2");
            startRepetitiveTasks();
        }
          
        public void startRepetitiveTasks()
              {

              //   setUpAIChannels();
                 // Create the delegate that invokes methods for the timer.
                 TimerCallback pressureCallback = new TimerCallback(updatePressures);
                 stateTimer = new System.Threading.Timer(pressureCallback, null, 0, 500);
                 
              }

        private object updatePressureLock = new object();
        public void updatePressures(Object obj)
        {
            lock (updatePressureLock)
            {
                double pressure1 = atomSourceGauge1.Pressure;
                double pressure2 = atomSourceGauge2.Pressure;

                window.outputAIdata(window.pressureIndicator1, pressure1.ToString("0.##E+0"));
                window.outputAIdata(window.pressureIndicator2, pressure2.ToString("0.##E+0"));
                
                if (atomSourceGauge1.OverPressure)
                {
                    Console.WriteLine("Pressure 1 is too high!");
                }
                if (atomSourceGauge2.OverPressure)
                {
                    Console.WriteLine("Pressure 2 is too high!");
                }
            }
        }

                        
    }
}
