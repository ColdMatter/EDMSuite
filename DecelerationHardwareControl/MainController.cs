using System;
using System.Globalization;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using DAQ.Environment;
using DAQ.HAL;
using DAQ.TransferCavityLock;
using NationalInstruments.DAQmx;
using NationalInstruments;
using MoleculeMOTHardwareControl.Controls;

namespace MoleculeMOTHardwareControl
{
    public class MainController : MarshalByRefObject
    {
        private ControlWindow mainWindow;

        private string[] tabNames =
            new string[] {
                "Windfreak Synthesizer"
            };
        private GenericController[] tabControllers = 
            new GenericController[] { 
                new WindfreakTabController((WindfreakSynth)Environs.Hardware.Instruments["windfreak"])
            };
        
        public void Start()
        {
            mainWindow = new ControlWindow();
            mainWindow.controller = this;
            AddTabs();
            Application.Run(mainWindow);
        }

        private void AddTabs()
        {
            int numTabs = tabNames.Length;
            if (!(numTabs == tabControllers.Length)) throw new ArgumentException();
            for (int i = 0; i < numTabs; i++)
            {
                TabPage tabPage = new TabPage(tabNames[i]);
                GenericView view = tabControllers[i].view;
                tabPage.Controls.Add(view);
                mainWindow.AddTabPage(tabPage);
            }
        }
    }
}