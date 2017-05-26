using System;
using System.Collections.Generic;
using System.Windows.Forms;
using DAQ;
using DAQ.Environment;
using DAQ.TransferCavityLock2012;
using DAQ.HAL;
using Data;
using Data.Scans;
using MicrocavityScanner.Acquire;
using MicrocavityScanner.GUI;
using ScanMaster;

namespace MicrocavityScanner
{
    /// <summary>
    /// The controller is the main part of MicrocavityScanner. This Program interacts
    /// with ScanMaster to control scans of multiple parameters that are much simpler
    /// than what ScanMaster can do. 
    /// </summary>
    public class Controller : MarshalByRefObject
    {
        #region Class members
        private TransferCavityLock2012.Controller tclController;
        private ScanMaster.Controller smController;

        public enum AppState { stopped, running };
        private MainForm mainForm;
        private Acquire.Scanitor scanitor;
        public Acquire.Scanitor Scanitor
        {
            get { return scanitor; }
        }

        private DataStore dataStore = new DataStore();
        public DataStore DataStore
        {
            get { return dataStore; }
        }

        public Dictionary<string, double> scanSettings = new Dictionary<string, double>();
        public Dictionary<string, string> laserSettings = new Dictionary<string, string>();

        private static Controller controllerInstance;
        public AppState appState = AppState.stopped;
        
        #endregion



        #region Initialisation

        // This is the right way to get a reference to the controller. You shouldn't create a
        // controller yourself.
        public static Controller GetController()
        {
            if (controllerInstance == null)
            {
                controllerInstance = new Controller();
            }
            return controllerInstance;
        }

        // without this method, any remote connections to this object will time out after
        // five minutes of inactivity.
        // It just overrides the lifetime lease system completely.
        public override Object InitializeLifetimeService()
        {
            return null;
        }

        // This function is called at the very start of application execution.
        public void StartApplication()
        {
            // Get access to any other applications required
            Environs.Hardware.ConnectApplications();

            // make an scanitor and connect ourself to its events
            scanitor = new Scanitor();
            //I haven't written these events yet:
            scanitor.Data += new DataEventHandler(DataHandler);
            //acquisitor.ScanFinished += new ScanFinishedEventHandler(ScanFinishedHandler);

            mainForm = new MainForm(this);
            mainForm.Show();

            // run the main event loop
            Application.Run(mainForm);

        }
        // When the main window gets told to shut, it calls this function.
        // In here things that need to be done before the application stops
        // are sorted out.
        public void StopApplication()
        {
            //AcquireStop();
        }

        #endregion

        #region Local 

        public void StartAcquire()
        {
            appState = AppState.running;
            scanitor.StartScan();
        }

        public void StopAcquire()
        {
            if (appState == AppState.running)
            {
                scanitor.StopScan();
                appState = AppState.stopped;
            }
        }

        // This function is registered to handle data events from the Scanitor.
        // Note well that this will be called on the acquisitor thread (meaning
        // no direct GUI manipulation in this function).
        private void DataHandler(object sender, DataEventArgs e)
        {
            lock (this)
            {
                // store the datapoint
                dataStore.AddScanPoint(e.point);

                // tell the viewers to handle the data point.
                mainForm.UpdateGraphs(e.point);
            }
        }

        // Selecting Acquire->Stop on the front panel will result in this function being called.
        public void AcquireStop()
        {
            if (appState == AppState.running)
            {
                scanitor.StopScan();
                appState = AppState.stopped;
            }
        }

        public void SaveData()
        {
            smController = (ScanMaster.Controller)(Activator.GetObject(typeof(ScanMaster.Controller), "tcp://localhost:1170/controller.rem"));
            smController.SaveData();
        }

        #endregion
    }
}
