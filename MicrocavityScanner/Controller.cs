using System;
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
        /// with ScanMaster to control scans of multiple parameters that are not limited
        /// to the hardware in the way that ScanMaster is. 
        /// </summary>
    public class Controller : MarshalByRefObject
    {
        #region Class members

        public enum AppState { stopped, running };
        private MainForm mainForm;
        private Acquire.Scanitor scanitor;
        public Acquire.Scanitor Scanitor
        {
            get { return scanitor; }
        }

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
            // make an scanitor and connect ourself to its events
            scanitor = new Scanitor();
            //I haven't written these events yet:
            //acquisitor.Data += new DataEventHandler(DataHandler);
            //acquisitor.ScanFinished += new ScanFinishedEventHandler(ScanFinishedHandler);

            mainForm = new MainForm(this);
            mainForm.Show();

            // Get access to any other applications required
            Environs.Hardware.ConnectApplications();

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

        public void StartAquire()
        { 

        }
    }
}
