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
using CaFBECHardwareController.Controls;

namespace CaFBECHardwareController
{
    public class Controller : MarshalByRefObject, ExperimentReportable
    {
        private ControlWindow mainWindow;

        public Dictionary<string, GenericController> tabs = new Dictionary<string, GenericController>()
        {

            { "Source", new SourceTabController() },
            { "Temperature", new TTabController() },
            { "Pressure", new PTabController() },
            { "Gigatronics", new GigatronicsTabController() }

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
            Dictionary<string, object> report = new Dictionary<string, object>();
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

        //public void SetWindfreakFrequency(double freq)
        //{
        //    WindfreakTabController windfreakController = tabs["Windfreak"] as WindfreakTabController;
        //    if (windfreakController != null)
        //    {
        //        windfreakController.SetFrequency(freq, false);
        //    }
        //}

        //public void SetWindfreakAmplitude(double amp)
        //{
        //    WindfreakTabController windfreakController = tabs["Windfreak"] as WindfreakTabController;
        //    if (windfreakController != null)
        //    {
        //        windfreakController.SetAmplitude(amp, false);
        //    }
        //}

        //public double GetWindfreakFrequency()
        //{
        //    double freq = 0.0;
        //    WindfreakTabController windfreakController = tabs["Windfreak"] as WindfreakTabController;
        //    if (windfreakController != null)
        //    {
        //        freq = windfreakController.GetFrequency(false);
        //    }
        //    return freq;
        //}

        //public double GetWindfreakAmplitude()
        //{
        //    double amp = 0.0;
        //    WindfreakTabController windfreakController = tabs["Windfreak"] as WindfreakTabController;
        //    if (windfreakController != null)
        //    {
        //        amp = windfreakController.GetAmplitude(false);
        //    }
        //    return amp;
        //}

        public void SetGigatronicsFrequency(double freq)
        {
            GigatronicsTabController gigatronicsController = tabs["Gigatronics"] as GigatronicsTabController;
            if (gigatronicsController != null)
            {
                gigatronicsController.SetFrequency(freq);
            }
        }

        public void SetGigatronicsAmplitude(double amp)
        {
            GigatronicsTabController gigatronicsController = tabs["Gigatronics"] as GigatronicsTabController;
            if (gigatronicsController != null)
            {
                gigatronicsController.SetAmplitude(amp);
            }
        }

        public double GetGigatronicsFrequency()
        {
            double freq = 0.0;
            GigatronicsTabController gigatronicsController = tabs["Gigatronics"] as GigatronicsTabController;
            if (gigatronicsController != null)
            {
                freq = gigatronicsController.GetFrequency();
            }
            return freq;
        }

        public double GetGigatronicsAmplitude()
        {
            double amp = 0.0;
            GigatronicsTabController gigatronicsController = tabs["Gigatronics"] as GigatronicsTabController;
            if (gigatronicsController != null)
            {
                amp = gigatronicsController.GetAmplitude();
            }
            return amp;
        }
    }
}