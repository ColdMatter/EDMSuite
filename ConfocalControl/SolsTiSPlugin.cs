using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;

using NationalInstruments;
using NationalInstruments.DAQmx;
using DAQ.Environment;
using DAQ.HAL;

namespace ConfocalControl
{
    public class SolsTiSPlugin
    {
        #region Class members

        // Dependencies should refer to this instance only 
        private static SolsTiSPlugin controllerInstance;
        public static SolsTiSPlugin GetController()
        {
            if (controllerInstance == null)
            {
                controllerInstance = new SolsTiSPlugin();
            }
            return controllerInstance;
        }

        // Define Opt state
        private enum ScanState { stopped, running, stopping };
        private ScanState backendState = ScanState.stopped;

        // Settings
        public PluginSettings Settings {get; set;}

        // Laser
        private ICEBlocSolsTiS solstis;
        public ICEBlocSolsTiS Solstis { get { return solstis; } }

        // Keep track of tasks
        private List<Point> pointHistory;
        private Task triggerTask;
        private DigitalSingleChannelWriter triggerWriter;
        private Task freqOutTask;
        private string counterChannel;
        private Task counterTask;
        private CounterSingleChannelReader counterReader;

        #endregion

        #region Initialization

        public void LoadSettings()
        {
            Settings = PluginSaveLoad.LoadSettings("solstis");
        }

        public SolsTiSPlugin()
        {
            string computer_ip = "192.168.1.23";
            solstis = new ICEBlocSolsTiS(computer_ip);

            LoadSettings();
            if (Settings.Keys.Count != 1)
            {
                Settings["wavelength"] = 785.0;

                Settings["wavemeterScanStart"] = 780.0;
                Settings["wavemeterScanStop"] = 790.0;
                Settings["wavemeterScanPoints"] = 100;
                return;
            }

            triggerTask = null;
            freqOutTask = null;
            counterTask = null;

            triggerWriter = null;
            counterReader = null;
        }

        #endregion

        #region Wavemeter Scan



        #endregion

    }
}
