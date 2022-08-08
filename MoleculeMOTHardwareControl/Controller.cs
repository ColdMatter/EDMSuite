using System;
using System.Globalization;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.Linq;
using DAQ;
using DAQ.Environment;
using DAQ.HAL;
using DAQ.TransferCavityLock;
using NationalInstruments.DAQmx;
using NationalInstruments;
using MoleculeMOTHardwareControl.Controls;

namespace MoleculeMOTHardwareControl
{
    public class Controller : MarshalByRefObject, ExperimentReportable
    {
        private ControlWindow mainWindow;

        public Dictionary<string, GenericController> tabs = new Dictionary<string, GenericController>()
        {
            //{ "Windfreak Synthesizer", new WindfreakTabController((WindfreakSynth)Environs.Hardware.Instruments["windfreak"]) },
            { "General Hardware", new SourceTabController() },
            //{ "Gigatronics Synthesizer 1", new GigatronicsTabController((Gigatronics7100Synth)Environs.Hardware.Instruments["gigatronics 1"]) },
            //{ "Gigatronics Synthesizer 2", new GigatronicsTabController((Gigatronics7100Synth)Environs.Hardware.Instruments["gigatronics 2"]) },
            { "XPS Track", new TrackController() }
            
        };

        // Without this method, any remote connections to this object will time out after
        // 5 minutes of inactivity. It just overrides the lifetime lease system completely.
        public override Object InitializeLifetimeService()
        {
            return null;
        }
        
        public void Start()
        {
            mainWindow = new ControlWindow();
            mainWindow.controller = this;
            AddTabs();
            Application.Run(mainWindow);
        }

        private void AddTabs()
        {
            foreach (KeyValuePair<string, GenericController> tab in tabs)
            {
                TabPage tabPage = new TabPage(tab.Key);
                GenericView view = tab.Value.view;
                tabPage.Controls.Add(view);
                mainWindow.AddTabPage(tabPage);
            }
        }

        public Dictionary<string, object> GetExperimentReport()
        {
            Dictionary<string, object> report = new Dictionary<string,object>();
            // Add the reports from each tab controller
            foreach (KeyValuePair<string, GenericController> tab in tabs)
            {
                Dictionary<string, object> subReport = tab.Value.Report();
                if (subReport != null)
                {
                    Dictionary<string, object> prefixedSubReport = new Dictionary<string, object>();
                    foreach (KeyValuePair<string, object> item in subReport)
                    {
                        prefixedSubReport.Add(tab.Key + " - " + item.Key, item.Value);
                    }

                    report = report.Concat(prefixedSubReport).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
                }
            }
            return report;
        }
    }
}