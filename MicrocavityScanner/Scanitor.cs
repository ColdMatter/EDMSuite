using System;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DAQ.Environment;
using DAQ.TransferCavityLock2012;
using ScanMaster;
using ScanMaster.Acquire.Plugins;
using ScanMaster.Acquire;
using MicrocavityScanner.GUI;
using Data;
using Data.Scans;
using System.Diagnostics; //for stopwatch

namespace MicrocavityScanner.Acquire
{
    public delegate void DataEventHandler(object sender, DataEventArgs e);
    public delegate void ScanFinishedEventHandler(object sender, EventArgs e);

    public class Scanitor
    {
        private AcquisitorConfiguration config;
        public AcquisitorConfiguration Configuration
        {
            set { config = value; }
            get { return config; }
        }
        public event DataEventHandler Data;
        public event ScanFinishedEventHandler ScanFinished;
        public object ScanitorMonitorLock = new Object();

        private TransferCavityLock2012.Controller tclController;
        public ScanMaster.Controller smController;

        private ScanMaster.Acquire.Acquisitor smAcquisitor;

        private ScanMaster.Acquire.AcquisitorConfiguration smConfig;

        private static Controller controllerInstance;

        private Thread acquireThread;

        enum ScanitorState { stopped, running, stopping };
        private ScanitorState backendState = ScanitorState.stopped;

        public double slowLaserValue;
        public double fastLaserValue;
        
        public void StartScan()
        {
            ConnectRemoting();

            controllerInstance = Controller.GetController();

            acquireThread = new Thread(new ThreadStart(this.Acquire));
            acquireThread.Name = "Microcavity Scanitor";
            acquireThread.Priority = ThreadPriority.Normal;
            backendState = ScanitorState.running;
            acquireThread.Start();
        }

        public void StopScan()
        {
            lock (this)
            {
                backendState = ScanitorState.stopping;
            }
            //Monitor.Pulse(ScanitorMonitorLock);
            //Monitor.Exit(ScanitorMonitorLock);
        }

        private void Acquire()
        {
            Stopwatch timer = new System.Diagnostics.Stopwatch();
            timer.Start();

            // lock a monitor onto the acquisitor, to synchronise with the controller
            // when acquiring a set number of scans - the monitor is released in
            // AcquisitionFinishing()
            Monitor.Enter(ScanitorMonitorLock);

            //get hold of the plugins
            FastAnalogInputPlugin directanalogPlugin = new FastAnalogInputPlugin();
            FastMultiInputShotGathererPlugin directshotPlugin = new FastMultiInputShotGathererPlugin();
            //MicrocavityPatternPlugin directpgPlugin = new MicrocavityPatternPlugin();

            //get settings lasers
            slowLaserValue = tclController.GetLaserSetpoint(
                Controller.GetController().laserSettings["SlowLaser"]);
            fastLaserValue = tclController.GetLaserSetpoint(
                Controller.GetController().laserSettings["FastLaser"]);

            //move to the start of the scan
            
            SoftStep(slowLaserValue, "SlowLaser", Controller.GetController().
                       scanSettings["SlowAxisStart"],666);
            slowLaserValue = tclController.GetLaserSetpoint(
                Controller.GetController().laserSettings["SlowLaser"]);
            SoftStep(fastLaserValue, "FastLaser", Controller.GetController().
                       scanSettings["FastAxisStart"],333);
            fastLaserValue = tclController.GetLaserSetpoint(
                Controller.GetController().laserSettings["FastLaser"]);

            //Get plugins through ScanMaster
            //****Here*****
            string thestate = Convert.ToString(smController.appState);
            directanalogPlugin.AcquisitionStarting();
            directshotPlugin.ReInitialiseSettings(Controller.GetController().scanSettings["Exposure"]);
            directshotPlugin.AcquisitionStarting();

            //directpgPlugin.AcquisitionStarting();

            //smAcquisitor.AcquireStart(1);
            //object listofchannels = smConfig.analogPlugin.Settings["channelList"];
            //smConfig.analogPlugin.AcquisitionStarting();
            //smController.Acquisitor.Configuration.
            //    analogPlugin.AcquisitionStarting();
            //smController.Acquisitor.Configuration.
            //    shotGathererPlugin.AcquisitionStarting();
            //analogPlugin.AcquisitionStarting();
            //shotPlugin.AcquisitionStarting();

            long timerInitialise = timer.ElapsedMilliseconds;

            controllerInstance.appState = Controller.AppState.running;
            controllerInstance.GUIUpdate();

            //loop for slow axis
            for (double slowNumber = 0;
                slowNumber < Controller.GetController().scanSettings["SlowAxisRes"]; 
                slowNumber++)
            {
                //reset fast axis
                SoftStep(fastLaserValue, "FastLaser", Controller.GetController().
                        scanSettings["FastAxisStart"],333);
                fastLaserValue = tclController.GetLaserSetpoint(
                Controller.GetController().laserSettings["FastLaser"]);

                //move to new slow axis point if axes unlinked
                double currentslowpoint = Controller.GetController().
                        scanSettings["SlowAxisStart"] + slowNumber *
                        (Controller.GetController().scanSettings["SlowAxisEnd"] -
                        Controller.GetController().scanSettings["SlowAxisStart"]) /
                        Controller.GetController().scanSettings["SlowAxisRes"];
                if (Controller.GetController().LinkAxes)
                {
                    SoftStep(slowLaserValue, "SlowLaser", Controller.GetController().
                            scanSettings["SlowAxisStart"],666);
                    slowLaserValue = tclController.GetLaserSetpoint(
                        Controller.GetController().laserSettings["SlowLaser"]);
                }
                else
                {
                    tclController.SetLaserSetpoint(
                         Controller.GetController().laserSettings["SlowLaser"],
                         currentslowpoint);
                }

                //prep the data for fast scan
                ScanPoint sp = new ScanPoint();
                if (Controller.GetController().LinkAxes)
                {
                    sp.ScanParameter = 0;
                }
                else
                {
                    sp.ScanParameter = currentslowpoint;
                }

                //take analog data for each slow scan
                directanalogPlugin.ScanStarting();
                directanalogPlugin.ArmAndWait();
                directanalogPlugin.ScanFinished();
                //smController.Acquisitor.Configuration.
                //    analogPlugin.ArmAndWait();
                //sp.Analogs.AddRange(smController.
                //    Acquisitor.Configuration.analogPlugin.Analogs);
                sp.Analogs.AddRange(directanalogPlugin.Analogs);

                //pattern generator start
                //directpgPlugin.ScanStarting();             

                long timerSlowMove = timer.ElapsedMilliseconds;

                //loop for fast axis
                for (double fastNumber = 0;
                    fastNumber < controllerInstance.scanSettings["FastAxisRes"];
                    fastNumber++)
                {
                    //move to new fast axis point
                    double currentfastpoint = Controller.GetController().
                        scanSettings["FastAxisStart"] + fastNumber *
                        (Controller.GetController().scanSettings["FastAxisEnd"] -
                        Controller.GetController().scanSettings["FastAxisStart"]) /
                        Controller.GetController().scanSettings["FastAxisRes"];
                    tclController.SetLaserSetpoint(Controller.GetController().
                        laserSettings["FastLaser"],currentfastpoint);
                    fastLaserValue = tclController.GetLaserSetpoint(
                        Controller.GetController().laserSettings["FastLaser"]);

                    //move slow axis if axes linked
                    if (Controller.GetController().LinkAxes)
                    {
                        double linkedslowpoint = Controller.GetController().
                            scanSettings["SlowAxisStart"] + fastNumber *
                            (Controller.GetController().scanSettings["SlowAxisEnd"] -
                            Controller.GetController().scanSettings["SlowAxisStart"]) /
                            Controller.GetController().scanSettings["FastAxisRes"];
                        tclController.SetLaserSetpoint(
                             Controller.GetController().laserSettings["SlowLaser"],
                             linkedslowpoint);
                        slowLaserValue = tclController.GetLaserSetpoint(
                        Controller.GetController().laserSettings["SlowLaser"]);
                    }

                    long timerFastMove = timer.ElapsedMilliseconds;

                    //start the shot plugin
                    //smController.Acquisitor.Configuration.
                    //    shotGathererPlugin.PreArm();
                    directshotPlugin.PreArm();

                    //take datapoint
                    //smController.Acquisitor.Configuration.
                    //    shotGathererPlugin.ArmAndWait();
                    directshotPlugin.ArmAndWait();

                    // read out the data
                    //sp.OnShots.Add(smController.Acquisitor.
                    //    Configuration.shotGathererPlugin.Shot);
                    sp.OnShots.Add(directshotPlugin.Shot);

                    //I am adding the fast axis laser position to the 
                    //Off shots this is a horrible fudge but it is the 
                    //quickest way to easily plot in realtime
                    Shot offs = new Shot();
                    TOF offt = new TOF();
                    double[] offlist = { currentfastpoint };
                    offt.Data = offlist;
                    offs.TOFs.Add(offt);
                    sp.OffShots.Add(offs);

                    //smController.Acquisitor.Configuration.
                    //    shotGathererPlugin.PostArm();
                    directshotPlugin.PostArm();
                    //directpgPlugin.ScanFinished();

                    long timerTakeShot = timer.ElapsedMilliseconds;

                    // send up the data bundle
                    DataEventArgs evArgs = new DataEventArgs();
                    evArgs.point = sp;
                    OnData(evArgs);

                    long timerProcessShot = timer.ElapsedMilliseconds;

                    // check for exit
                    if (CheckIfStopping())
                    {
                        //OnScanFinished();
                        directanalogPlugin.AcquisitionFinished();
                        directshotPlugin.AcquisitionFinished();
                        AcquisitionFinishing();
                        return;
                    }
                }
                OnScanFinished();
            }
            directanalogPlugin.AcquisitionFinished();
            directshotPlugin.AcquisitionFinished();

            AcquisitionFinishing();
        }

        private void SoftStep(double currentpoint,string laser, double newpoint, int delay)
        {
            double minstep = 0.01;

            if (Math.Abs(newpoint - currentpoint) >
               minstep)
            {
                int softloops = (int)Math.Floor(Math.Abs((newpoint - currentpoint) / minstep));
                for (int i = 0; i < softloops; i++)
                {
                    tclController.SetLaserSetpoint(Controller.GetController().
                        laserSettings[laser], currentpoint + i * (newpoint - currentpoint)/softloops);
                    Thread.Sleep(delay);
                }
                tclController.SetLaserSetpoint(Controller.GetController().
                        laserSettings[laser], newpoint);

            }
            else
            {
                tclController.SetLaserSetpoint(Controller.GetController().
                        laserSettings[laser], newpoint);
            }
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
                if (backendState == ScanitorState.stopping)
                {
                    backendState = ScanitorState.stopped;
                    return true;
                }
                else return false;
            }
        }

        public void DisconnectRemoting()
        {
            GC.Collect();
        }

        public void ConnectRemoting()
        {
            // connect the TCL controller over remoting network connection
            tclController = (TransferCavityLock2012.Controller) (Activator.GetObject(typeof(TransferCavityLock2012.Controller), "tcp://localhost:1190/controller.rem"));

            // connect the ScanMaster controller over remoting network connection
            smController = (ScanMaster.Controller) (Activator.GetObject(typeof(ScanMaster.Controller), "tcp://localhost:1170/controller.rem"));
            smAcquisitor = smController.Acquisitor;
            smConfig = smController.Acquisitor.Configuration;
        }

        public void SetLaserV(string channel,double setV)
        {
            tclController.SetLaserSetpoint(channel, setV);
        }

        protected virtual void OnData(DataEventArgs e)
        {
            if (Data != null) Data(this, e);
        }

        protected virtual void OnScanFinished()
        {
            if (ScanFinished != null) ScanFinished(this, new EventArgs());
        }
    }

    public class DataEventArgs : EventArgs
    {
        public ScanPoint point;
    }
}
