using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using DAQ.Environment;
using DAQ.HAL;
using ScanMaster;
using ScanMaster.Acquire;
using ScanMaster.Acquire.Plugin;
using Data;
using Data.Scans;

using NationalInstruments.DAQmx;
using NationalInstruments;

namespace ConfocalMicroscopeControl
{
    class Controller
    {
        MainWindow window;

        #region Class members

        // Dependencies should refer to this instance only 
        private static Controller controllerInstance;
        public static Controller GetController()
        {
            if (controllerInstance == null)
            {
                controllerInstance = new Controller();
            }
            return controllerInstance;
        }

        // Settings dictionnary
        private PluginSettings _rasterScanSettings = new PluginSettings();
        public PluginSettings scanSettings
        {
            get { return _rasterScanSettings; }
            set { _rasterScanSettings = value; }
        }

        // Storing raster Scan data
        private DataStore dataStore = new DataStore();
        public DataStore DataStore
        {
            get { return dataStore; }
        }

        // Get galvo controller
        private GalvoPairPlugin _galvos;
        public GalvoPairPlugin GalvoPair
        {
            get { return _galvos; }
        }

        #endregion

        #region Initialisation

        private void InitialiseSettings()
        {
            _rasterScanSettings["GalvoXStart"] = (double)0;
            _rasterScanSettings["GalvoXEnd"] = (double)1;
            _rasterScanSettings["GalvoXRes"] = (double)100;
            _rasterScanSettings["GalvoYStart"] = (double)0;
            _rasterScanSettings["GalvoYEnd"] = (double)1;
            _rasterScanSettings["GalvoYRes"] = (double)100;
            _rasterScanSettings["Exposure"] = (double)100;
        }

        public Controller() 
        {
            InitialiseSettings();
        }

        #endregion

        #region Methods

        public void StartApplication()
        {
            window = new MainWindow(this);
            window.Show();
        }

        public void StopApplication()
        {
            // Implement
        }

        private void DataHandler(object sender, DataEventArgs e)
        {
            lock (this)
            {
                // Implement
                // store the datapoint
                dataStore.AddScanPoint(e.point);

                // tell the viewers to handle the data point.
                // mainForm.UpdateGraphs(e.point);
            }
        }

        private void ScanFinishedHandler(object sender, EventArgs e)
        {
            // Implement
        }

        #endregion 
    }
}
