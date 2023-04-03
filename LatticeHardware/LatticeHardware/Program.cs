using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
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
using NationalInstruments.DAQmx;
//using NationalInstruments.UI;
//using NationalInstruments.UI.WindowsForms;
//using NationalInstruments.Visa;
using System.Linq;

using DAQ.HAL;
using DAQ.Environment;
using Data;

using System.Diagnostics;




namespace LatticeHardware
{
    public class Controller : MarshalByRefObject
    {
        #region Constants

        #endregion

        #region Setup

        Hashtable digitalTasks = new Hashtable();
        Hashtable digitalInputTasks = new Hashtable();

        Task cryoon;
        Task cryooff;

        ControlWindow window;

        // without this method, any remote connections to this object will time out after
        // five minutes of inactivity.
        // It just overrides the lifetime lease system completely.
        public override Object InitializeLifetimeService()
        {
            return null;
        }
        public void Start()
        {
            // make the digital tasks
            //CreateDigitalTask("notEOnOff");
            //CreateDigitalTask("eOnOff");
            CreateDigitalTask("cryoon");
            CreateDigitalTask("cryoofff");

            // make the control window
            window = new ControlWindow();
            window.controller = this;

            Application.Run(window);
        }
        private void CreateDigitalTask(String name)
        {
            Task digitalTask = new Task(name);
            ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels[name]).AddToTask(digitalTask);
            digitalTask.Control(TaskAction.Verify);
            digitalTasks.Add(name, digitalTask);
        }

        #endregion




        static class Program
        {
            /// <summary>
            ///  The main entry point for the application.
            /// </summary>
            [STAThread]
            static void Main()
            {
                Application.SetHighDpiMode(HighDpiMode.SystemAware);
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new Form1());
            }
        }





    }
}
