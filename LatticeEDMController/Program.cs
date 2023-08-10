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
using NationalInstruments.UI.WindowsForms;
using NationalInstruments.VisaNS;
using System.Linq;
using System.IO.Ports;
using System.Windows.Forms.DataVisualization.Charting;
using DAQ.HAL;
using DAQ.Environment;
using System.Diagnostics;
using Data;

namespace LatticeEDMController
{
    public class LatticeController : MarshalByRefObject
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        LatticeEDMController form;
        AlicatFlowController FlowControllers = (AlicatFlowController)Environs.Hardware.Instruments["FlowControllers"];


        public void SetSetpointHelium()
        {
            //if (!Environs.Debug)
            //{
            int inputflowrate = Int32.Parse(form.tbHeliumSetpoint.Text);
            int flowrate = (inputflowrate / 10) * 64000;
            string flowratestring = flowrate.ToString();
            string output = FlowControllers.SetFlow("a", flowratestring);
            form.SetTextBox(form.tbHeliumFlowReadout, output);
            //}
        }
    }
}
