using System;
using System.Collections;
using System.IO;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Lifetime;
using System.Runtime.Serialization;
using System.Threading;
using System.Windows.Forms;
using System.Text;

using DAQ.Environment;
using DAQ.HAL;
using Data;
using Data.Scans;
using MOTMaster.PatternControl;

namespace MOTMaster
{
    /// <summary>
    /// Here's MOTMaster's controller. It's supposed to be much simpler than ScanMaster's counterpart.
    /// Its only role is to generate a pattern along with a few commands like ("take data") 
    /// to the hardware controller.
    /// It has an interface to allow the user to create a pattern and a big "go" button. That's it. 
    /// </summary>
    public class Controller : MarshalByRefObject
    {

        #region Class members

        public enum AppState { stopped, running };

        private ControllerWindow controllerWindow;
        //This is the Acquisitor's counterpart. (the "rebranding" was done because this doesn't acquire anything)
        private PatternController pc;
        public PatternController PC
        {
            get { return pc; }
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

            // ask the remoting system for access to the SympatheticHardwareController
            RemotingConfiguration.RegisterWellKnownClientType(
                Type.GetType("SympatheticHardwareControl.Controller, SympatheticHardwareControl"),
                "tcp://localhost:1180/controller.rem"
                );

            // make an acquisitor and connect ourself to its events
            pc = new PatternController();
            
            controllerWindow = new ControllerWindow();
            controllerWindow.Show();

            // Get access to any other applications required
            Environs.Hardware.ConnectApplications();

            // run the main event loop
            Application.Run(controllerWindow);

        }

        // When the main window gets told to shut, it calls this function.
        // In here things that need to be done before the application stops
        // are sorted out.
        public void StopApplication()
        {
            
        }

        #endregion

        #region Local functions - these should only be called locally

        #endregion

        #region Remote functions - these functions can be called remotely

        public void PatternStart()
        {
            if (appState != AppState.running)
            {
                // start the acquisition
                pc.PatternStart();
                appState = AppState.running;
            }
        }

        public void PatternStop()
        {
            if (appState == AppState.running)
            {
                pc.PatternStop();
                appState = AppState.stopped;
            }
        }

        public void SetPatternAndWait(int numberOfScans)
        {
            Monitor.Enter(pc.PatternMonitorLock);
            this.PatternStart();
            // check that acquisition is underway. Provided it is, release the lock
            if (appState == AppState.running) Monitor.Wait(pc.PatternMonitorLock);
            Monitor.Exit(pc.PatternMonitorLock);
            PatternStop();
        }

        bool patternRunning = false;

        

        #endregion

        #region Private functions

       

        #endregion
    }
}

