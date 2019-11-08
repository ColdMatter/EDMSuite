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
using DAQ.HAL;
using DAQ.Environment;
using System.Diagnostics;

namespace UltracoldEDMHardwareControl
{
    static class Controller : LakeShore336TemperatureController
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        # region Class members

        // hardware
        //LakeShore336TemperatureController tempController = (LakeShore336TemperatureController)Environs.Hardware.Instruments["green"];

        # endregion

        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new ControlWindow());
        }
    }
}
