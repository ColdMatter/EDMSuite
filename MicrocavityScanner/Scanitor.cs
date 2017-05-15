using System;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DAQ.Environment;
using DAQ.TransferCavityLock2012;
using ScanMaster;
using MicrocavityScanner.GUI;

namespace MicrocavityScanner.Acquire
{
    public class Scanitor
    {
        private TransferCavityLock2012.Controller tclController;
        private ScanMaster.Controller smController;

        private static Controller controllerInstance;

        private Thread acquireThread;

        enum ScanitorState { stopped, running, stopping };
        private ScanitorState backendState = ScanitorState.stopped;

        private double slowLaserValue;
        private double fastLaserValue;

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

        private void Acquire()
        {
            //get settings lasers
            slowLaserValue = tclController.GetLaserSetpoint(
                Controller.GetController().laserSettings["SlowLaser"]);
            fastLaserValue = tclController.GetLaserSetpoint(
                Controller.GetController().laserSettings["FastLaser"]);

            //move to the start of the scan
            tclController.SetLaserSetpoint(
                Controller.GetController().laserSettings["SlowLaser"],
                Controller.GetController().scanSettings["SlowAxisStart"]);
            tclController.SetLaserSetpoint(
                Controller.GetController().laserSettings["FastLaser"],
                Controller.GetController().scanSettings["FastAxisStart"]);
            
                //loop for slow axis
                //move to new slow axis point

                //loop for fast axis
                //move to new fast axis point

                //take datapoint
                //end fast loop
                //end slow loop   
        }

    public void ConnectRemoting()
        {
            // connect the TCL controller over remoting network connection
            tclController = (TransferCavityLock2012.Controller) (Activator.GetObject(typeof(TransferCavityLock2012.Controller), "tcp://localhost:1190/controller.rem"));

            // connect the ScanMaster controller over remoting network connection
            smController = (ScanMaster.Controller) (Activator.GetObject(typeof(TransferCavityLock2012.Controller), "tcp://localhost:1170/controller.rem"));
        }

        public void SetLaserV(string channel,double setV)
        {
            tclController.SetLaserSetpoint(channel, setV);
        }
    }
}
