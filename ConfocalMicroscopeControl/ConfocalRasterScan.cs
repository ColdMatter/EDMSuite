using System;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using DAQ.Environment;
using ScanMaster.Acquire.Plugins;
using ScanMaster.Acquire;
using Data;
using Data.Scans;
using System.Diagnostics;

namespace ConfocalMicroscopeControl
{
    ///<summary>
    /// Plug-in for confocal raster scans. No fancy DAQ pattern builder is necessary here. 
    /// Borrows heavily from Kyle's Microcavity Scanitor.
    /// </summary>
    /// 

    // Uses delegate multicasting to compose and invoke event manager methods in series 
    public delegate void DataEventHandler(object sender, DataEventArgs e);
    public delegate void ScanFinishedEventHandler(object sender, EventArgs e);

    class ConfocalRasterScan
    {
        #region Class members

        // Scan state, Implement !
        public enum ScanState { stopped, running };
        public ScanState scanState = ScanState.stopped;

        // Bound event managers to class
        public event DataEventHandler Data;
        public event ScanFinishedEventHandler ScanFinished;

        // 
        public object ScanitorMonitorLock = new Object();

        // Fetch galvo controller 
        private ConfocalMicroscopeControl.GalvoPairPlugin gController;

        // Link to Controller, not bound to particular instance of RasterScan 
        private static Controller controllerInstance;

        // From System.Threading 
        private Thread acquireThread;

        // Define RasterScan state
        enum RasterScanState { stopped, running, stopping };
        private RasterScanState backendState = RasterScanState.stopped;

        // Keeping track of galvo positions
        private double galvoXValue;
        private double galvoYValue;

        #endregion

        #region Methods

        public void StartScan()
        {

            // Get running instance of controller 
            controllerInstance = Controller.GetController();
            gController = ConfocalMicroscopeControl.GalvoPairPlugin.GetController();

            // Define unparametrerized start method for thread
            acquireThread = new Thread(new ThreadStart(this.Acquire));
            acquireThread.Name = "Confocal Raster Scan";
            acquireThread.Priority = ThreadPriority.Normal;
            backendState = RasterScanState.running;

            // Start thread
            acquireThread.Start();
        }

        public void StopScan()
        {
            // Ensures no other thread tried to access this object while the method is running 
            lock (this)
            {
                backendState = RasterScanState.stopping;
            }
        }

        private void Acquire()
            // Main method for looping over scan parameters, aquiring scan outputs and connecting to controller for display
        {
            Stopwatch timer = new System.Diagnostics.Stopwatch();
            timer.Start();

            // Lock monitor to this thread until Exit is executed in AcquisitionFinishing()
            Monitor.Enter(ScanitorMonitorLock);

            // Get hold of counter plugin
            SingleCounterPlugin directshotPlugin = new SingleCounterPlugin();

            // Get Galvo settings.
            galvoXValue = gController.GetGalvoXSetpoint();
            galvoYValue = gController.GetGalvoYSetpoint();

            // Move to the start of the scan.
            gController.SetGalvoXSetpoint(
                         (double)controllerInstance.scanSettings["GalvoXStart"]);
            galvoXValue = gController.GetGalvoXSetpoint();

            gController.SetGalvoYSetpoint(
                         (double)controllerInstance.scanSettings["GalvoYStart"]);
            galvoYValue = gController.GetGalvoYSetpoint();

            // Get plugins
            directshotPlugin.ReInitialiseSettings((double)controllerInstance.scanSettings["Exposure"]);
            directshotPlugin.AcquisitionStarting();

            // Time initialization step
            long timerInitialise = timer.ElapsedMilliseconds;

            // Update controller state and GUI. Implement this
            scanState = ScanState.running;

            // Loop for X axis
            for (double XNumber = 0;
                XNumber < (double)controllerInstance.scanSettings["GalvoXRes"];
                XNumber++)
            {
                // Reset Y axis for new line
                gController.SetGalvoYSetpoint(
                             (double)controllerInstance.scanSettings["GalvoYStart"]);
                galvoYValue = gController.GetGalvoYSetpoint();

                // Calculate new X galvo point from current scan point 
                double currentGalvoXpoint = (double)controllerInstance.
                    scanSettings["GalvoXStart"] + XNumber *
                    ((double)controllerInstance.scanSettings["GalvoXEnd"] -
                    (double)controllerInstance.scanSettings["GalvoXStart"]) /
                    (double)controllerInstance.scanSettings["GalvoXRes"];

                // Move X galvo to new scan point 
                gController.SetGalvoXSetpoint(currentGalvoXpoint);

                // Measure X galvo 
                galvoXValue = gController.GetGalvoXSetpoint();

                // Prep the data for fast scan
                ScanPoint sp = new ScanPoint();
                sp.ScanParameter = galvoXValue;
        
                // Time X galvo move
                long timerGalvoXMove = timer.ElapsedMilliseconds;

                // Loop for Y axis
                for (double YNumber = 0;
                    YNumber < (double)controllerInstance.scanSettings["GalvoYRes"];
                    YNumber++)
                {
                    // Calculate new Y galvo point from current scan point 
                    double currentGalvoYpoint = (double)controllerInstance.
                        scanSettings["GalvoYStart"] + YNumber *
                        ((double)controllerInstance.scanSettings["GalvoYEnd"] -
                        (double)controllerInstance.scanSettings["GalvoYStart"]) /
                        (double)controllerInstance.scanSettings["GalvoYRes"];

                    // Move Y galvo to new scan point
                    gController.SetGalvoYSetpoint(currentGalvoYpoint);

                    // Measure Y galvo 
                    galvoYValue = gController.GetGalvoYSetpoint();

                    // Time Y galvo move
                    long timerFastMove = timer.ElapsedMilliseconds;

                    // Start the shot plugin
                    directshotPlugin.PreArm();

                    // Take datapoint
                    directshotPlugin.ArmAndWait();

                    // Read out the data
                    sp.OnShots.Add(directshotPlugin.Shot);

                    // I am adding the fast axis laser position to the 
                    // Off shots this is a horrible fudge but it is the 
                    // quickest way to easily plot in realtime
                    Shot offs = new Shot();
                    TOF offt = new TOF();
                    double[] offlist = { galvoYValue };
                    offt.Data = offlist;
                    offs.TOFs.Add(offt);
                    sp.OffShots.Add(offs);

                    // Stop the plugin.
                    directshotPlugin.PostArm();

                    // Time acquisition
                    long timerTakeShot = timer.ElapsedMilliseconds;

                    // Send up the data bundle
                    DataEventArgs evArgs = new DataEventArgs();
                    evArgs.point = sp;

                    // Implement this
                    OnData(evArgs);

                    // Time full processing of shot
                    long timerProcessShot = timer.ElapsedMilliseconds;

                    // Check if scan exit.
                    if (CheckIfStopping())
                    {
                        // Quit plugin
                        directshotPlugin.AcquisitionFinished();
                        AcquisitionFinishing();
                        return;
                    }
                }
                
                OnScanFinished();
            }

            // Again? 
            directshotPlugin.AcquisitionFinished();
            AcquisitionFinishing();
        }

        private void AcquisitionFinishing()
        {
            Monitor.Pulse(ScanitorMonitorLock);
            Monitor.Exit(ScanitorMonitorLock);
        }

        private bool CheckIfStopping()
        {
            lock (this)
            {
                if (backendState == RasterScanState.stopping)
                {
                    backendState = RasterScanState.stopped;
                    return true;
                }
                else return false;
            }
        }

        private void OnData(DataEventArgs e)
        {
            if (Data != null) Data(this, e);
        }

        private void OnScanFinished()
        {
            if (ScanFinished != null) ScanFinished(this, new EventArgs());
        }

        #endregion 
    }

    public class DataEventArgs : EventArgs
    {
        public ScanPoint point;
    }
}
