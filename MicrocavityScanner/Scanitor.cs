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
using MicrocavityScanner.GUI;
using Data;
using Data.Scans;

namespace MicrocavityScanner.Acquire
{
    public delegate void DataEventHandler(object sender, DataEventArgs e);

    public class Scanitor
    {
        public event DataEventHandler Data;
        public object ScanitorMonitorLock = new Object();

        private TransferCavityLock2012.Controller tclController;
        private ScanMaster.Controller smController;

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
            Monitor.Pulse(ScanitorMonitorLock);
            Monitor.Exit(ScanitorMonitorLock);
        }

        private void Acquire()
        {
            // lock a monitor onto the acquisitor, to synchronise with the controller
            // when acquiring a set number of scans - the monitor is released in
            // AcquisitionFinishing()
            Monitor.Enter(ScanitorMonitorLock);

            //get hold of the plugins
            //*** this is wrong I need to get hold of the instances 
            //that already exist not create new ones***
            FastAnalogInputPlugin directanalogPlugin = new FastAnalogInputPlugin();
            FastMultiInputShotGathererPlugin directshotPlugin = new FastMultiInputShotGathererPlugin();
            //MicrocavityPatternPlugin directpgPlugin = new MicrocavityPatternPlugin();

            //get settings lasers
            slowLaserValue = tclController.GetLaserSetpoint(
                Controller.GetController().laserSettings["SlowLaser"]);
            fastLaserValue = tclController.GetLaserSetpoint(
                Controller.GetController().laserSettings["FastLaser"]);

            //move to the start of the scan
            tclController.SetLaserSetpoint(
                Controller.GetController().laserSettings["SlowLaser"],
                Controller.GetController().scanSettings["SlowAxisStart"]);
            slowLaserValue = tclController.GetLaserSetpoint(
                Controller.GetController().laserSettings["SlowLaser"]);
            tclController.SetLaserSetpoint(
                Controller.GetController().laserSettings["FastLaser"],
                Controller.GetController().scanSettings["FastAxisStart"]);
            fastLaserValue = tclController.GetLaserSetpoint(
                Controller.GetController().laserSettings["FastLaser"]);

            //Get plugins through ScanMaster
            //****Here*****
            string thestate = Convert.ToString(smController.appState);
            directanalogPlugin.AcquisitionStarting();
            directshotPlugin.ReInitialiseSettings();
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

            //loop for slow axis
            for (double slowNumber = 0;
                slowNumber < Controller.GetController().scanSettings["SlowAxisRes"]; 
                slowNumber++)
            {
                //move to new slow axis point
                double currentslowpoint = Controller.GetController().
                    scanSettings["SlowAxisStart"] + slowNumber *
                    (Controller.GetController().scanSettings["SlowAxisEnd"] -
                    Controller.GetController().scanSettings["SlowAxisStart"]) /
                    Controller.GetController().scanSettings["SlowAxisRes"];
               tclController.SetLaserSetpoint(
                    Controller.GetController().laserSettings["SlowLaser"],
                    currentslowpoint);

                //prep the data for fast scan
                ScanPoint sp = new ScanPoint();
                sp.ScanParameter = currentslowpoint;

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

                //loop for fast axis
                for (double fastNumber = 0;
                    fastNumber < Controller.GetController().scanSettings["FastAxisRes"];
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

                    // send up the data bundle
                    DataEventArgs evArgs = new DataEventArgs();
                    evArgs.point = sp;
                    OnData(evArgs);
                }
            }  
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
    }

    public class DataEventArgs : EventArgs
    {
        public ScanPoint point;
    }
}
