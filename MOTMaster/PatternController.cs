using System;
using System.Threading;
using System.Windows.Forms;
using System.Collections;
using DAQ.Environment;
using Data;
using Data.Scans;

namespace MOTMaster.PatternControl
{
    public delegate void PatternFinishedEventHandler(object sender, EventArgs e);

    /// <summary>
    /// This is the counterpart for ScanMaster's Acquisitor. It doesn't acquire anymore so I "rebranded" it.
    /// </summary>
    public class PatternController
    {
        public event PatternFinishedEventHandler PatternFinished;
        public object PatternMonitorLock = new Object();

        private Thread patternThread;

        enum PatternState { stopped, running, stopping };
        private PatternState backendState = PatternState.stopped;

        

        public void PatternStart()
        {
            patternThread = new Thread(new ThreadStart(this.SendPattern));
            patternThread.Name = "MOTMaster PatternController";
            patternThread.Priority = ThreadPriority.Normal;
            backendState = PatternState.running;
            patternThread.Start();
        }

        public void PatternStop()
        {
            lock (this)
            {
                backendState = PatternState.stopping;
            }
        }


        private void SendPattern()
        {
            // the Monitor is left over from Acquisitor. Don't get it, so left it in. Jony's comment:
            // lock a monitor onto the acquisitor, to synchronise with the controller
            // when acquiring a set number of scans - the monitor is released in
            // AcquisitionFinishing() (not true. Released here now)
            Monitor.Enter(PatternMonitorLock);
            PatternStart();
            backendState = PatternState.stopped;
            // set the controller state to stopped
            Controller.GetController().appState = Controller.AppState.stopped;
            Monitor.Pulse(PatternMonitorLock);
            Monitor.Exit(PatternMonitorLock);
            return;
        }

    }

}
