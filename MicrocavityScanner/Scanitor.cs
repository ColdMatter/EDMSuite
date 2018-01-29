using Data;
using Data.Scans;
using ScanMaster.Acquire;
using System;
using System.Diagnostics; //for stopwatch
using System.Threading;

namespace MicrocavityScanner.Acquire
{
    public delegate void DataEventHandler(object sender, DataEventArgs e);
    public delegate void GUIUpdateEventHandler(object sender, GUIUpdateEventArgs e);
    public delegate void ScanFinishedEventHandler(object sender, EventArgs e);
    public delegate void PosUpdateEventHandler(object sender, UpdatePosArgs e);

    public class Scanitor
    {
        private AcquisitorConfiguration config;
        public AcquisitorConfiguration Configuration
        {
            set { config = value; }
            get { return config; }
        }
        public event DataEventHandler Data;
        public event GUIUpdateEventHandler GUIUpdate;
        public event ScanFinishedEventHandler ScanFinished;
        public event PosUpdateEventHandler PosUpdate;
        public object ScanitorMonitorLock = new Object();

        private TransferCavityLock2012.Controller tclController;
        //public ScanMaster.Controller smController;

        private ScanMaster.Acquire.Acquisitor smAcquisitor;

        private ScanMaster.Acquire.AcquisitorConfiguration smConfig;

        private static Controller controllerInstance;

        private Thread acquireThread;

        enum ScanitorState { stopped, running, stopping };
        private ScanitorState backendState = ScanitorState.stopped;

        public double slowLaserValue;
        public double fastLaserValue;

        private ShotGatherer shotgatherer;
        
        public void Initialise()
        {
            fastLaserValue = GetValue(Controller.GetController().laserSettings["FastLaser"]);
            slowLaserValue = GetValue(Controller.GetController().laserSettings["SlowLaser"]);
            UpdatePosArgs evArgs = new UpdatePosArgs();
            evArgs.position = new double[2];
            evArgs.position[1] = slowLaserValue;
            evArgs.position[0] = fastLaserValue;
            UpdatePosition(evArgs);
        }

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
            //FastAnalogInputPlugin directanalogPlugin = new FastAnalogInputPlugin();
            //FastMultiInputShotGathererPlugin directshotPlugin = new FastMultiInputShotGathererPlugin();
            //MicrocavityPatternPlugin directpgPlugin = new MicrocavityPatternPlugin();

            //get settings lasers
            slowLaserValue = GetValue(Controller.GetController().laserSettings["SlowLaser"]);
            //Controller.GetController().scanSettings["SlowPos"] = slowLaserValue;
            fastLaserValue = GetValue(Controller.GetController().laserSettings["FastLaser"]);
            //Controller.GetController().scanSettings["FastPos"] = fastLaserValue;

            //move to the start of the scan

            SoftStep(slowLaserValue, "SlowLaser", Controller.GetController().
                       scanSettings["SlowAxisStart"],50);
            //slowLaserValue = GetValue(Controller.GetController().laserSettings["SlowLaser"]);
            //Controller.GetController().scanSettings["SlowPos"] = slowLaserValue;
            SoftStep(fastLaserValue, "FastLaser", Controller.GetController().
                       scanSettings["FastAxisStart"],50);
            //fastLaserValue = GetValue(Controller.GetController().laserSettings["FastLaser"]);
            //Controller.GetController().scanSettings["FastPos"] = fastLaserValue;

            //Get plugins
            //string thestate = Convert.ToString(smController.appState);
            //directanalogPlugin.AcquisitionStarting();
            //directshotPlugin.PreInitialiseSettings();
            //directshotPlugin.ReInitialiseSettings(Controller.GetController().scanSettings["Exposure"]);
            
            //directshotPlugin.AcquisitionStarting();

            shotgatherer = new ShotGatherer();
            shotgatherer.InitialiseSettings(Controller.GetController().scanSettings["Exposure"]);
            shotgatherer.AcquisitionStarting();

            long timerInitialise = timer.ElapsedMilliseconds;

            controllerInstance.appState = Controller.AppState.running;
            controllerInstance.GUIUpdate();

            //loop for slow axis
            for (double slowNumber = 0;
                slowNumber < Controller.GetController().scanSettings["SlowAxisRes"]; 
                slowNumber++)
            {
                if (backendState == ScanitorState.running)
                {
                    //reset fast axis
                    SoftStep(fastLaserValue, "FastLaser", Controller.GetController().
                        scanSettings["FastAxisStart"], 50);
                    //fastLaserValue = GetValue(Controller.GetController().laserSettings["FastLaser"]);
                    //Controller.GetController().scanSettings["FastPos"] = fastLaserValue;
                }

                //move to new slow axis point if axes unlinked
                double currentslowpoint = Controller.GetController().
                        scanSettings["SlowAxisStart"] + slowNumber *
                        (Controller.GetController().scanSettings["SlowAxisEnd"] -
                        Controller.GetController().scanSettings["SlowAxisStart"]) /
                        (Controller.GetController().scanSettings["SlowAxisRes"]-1);
                if (Controller.GetController().LinkAxes)
                {
                    if (backendState == ScanitorState.running)
                    {
                        SoftStep(slowLaserValue, "SlowLaser", Controller.GetController().
                                scanSettings["SlowAxisStart"], 50);
                    }
                    //slowLaserValue = GetValue(Controller.GetController().laserSettings["SlowLaser"]);
                    //Controller.GetController().scanSettings["SlowPos"] = slowLaserValue;
                }
                else
                {
                    MoveTo(Controller.GetController().laserSettings["SlowLaser"],currentslowpoint);
                }
                //slowLaserValue = GetValue(Controller.GetController().laserSettings["SlowLaser"]);
                //Controller.GetController().scanSettings["SlowPos"] = slowLaserValue;

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
                //directanalogPlugin.ScanStarting();
                //directanalogPlugin.ArmAndWait();
                //directanalogPlugin.ScanFinished();
                //sp.Analogs.AddRange(directanalogPlugin.Analogs);            

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
                        (Controller.GetController().scanSettings["FastAxisRes"]-1);
                    if (backendState == ScanitorState.running)
                    {
                        MoveTo(Controller.GetController().laserSettings["FastLaser"], currentfastpoint);
                    }
                    //fastLaserValue = GetValue(Controller.GetController().laserSettings["FastLaser"]);
                    //Controller.GetController().scanSettings["FastPos"] = fastLaserValue;


                    //move slow axis if axes linked

                    if (Controller.GetController().LinkAxes)
                    {
                        double linkedslowpoint = Controller.GetController().
                            scanSettings["SlowAxisStart"] + fastNumber *
                            (Controller.GetController().scanSettings["SlowAxisEnd"] -
                            Controller.GetController().scanSettings["SlowAxisStart"]) /
                            (Controller.GetController().scanSettings["FastAxisRes"]-1);
                        if (backendState == ScanitorState.running)
                        {
                            MoveTo(Controller.GetController().laserSettings["SlowLaser"],linkedslowpoint);
                        }
                        //slowLaserValue = GetValue(Controller.GetController().laserSettings["SlowLaser"]);
                        //Controller.GetController().scanSettings["SlowPos"] = slowLaserValue;
                    }

                    long timerFastMove = timer.ElapsedMilliseconds;

                    //start the shot plugin
                    //directshotPlugin.PreArm();
                    shotgatherer.PreArm();

                    //take datapoint
                    //directshotPlugin.ArmAndWait();
                    shotgatherer.ArmAndWait();

                    // read out the data
                    //sp.OnShots.Add(directshotPlugin.Shot);
                    sp.OnShots.Add(shotgatherer.Shot);

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
                    //directshotPlugin.PostArm();
                    shotgatherer.PostArm();

                    long timerTakeShot = timer.ElapsedMilliseconds;

                    // check for exit
                    if (CheckIfStopping())
                    {
                        //OnScanFinished();
                        // send up the data bundle
                        DataEventArgs newevArgs = new DataEventArgs();
                        newevArgs.point = sp;
                        OnData(newevArgs);

                        //directanalogPlugin.AcquisitionFinished();
                        //directshotPlugin.AcquisitionFinished();
                        shotgatherer.AcquisitionFinished();
                        OnScanFinished();
                        AcquisitionFinishing();
                        controllerInstance.GUIUpdate();
                        return;
                    }
                    // keep the GUI updated
                    GUIUpdateEventArgs tempevArgs = new GUIUpdateEventArgs();
                    tempevArgs.point = sp;
                    TempUpdate(tempevArgs);

                }
                // send up the data bundle
                DataEventArgs evArgs = new DataEventArgs();
                evArgs.point = sp;
                OnData(evArgs);

                long timerProcessShot = timer.ElapsedMilliseconds;
            }
            OnScanFinished();
            //directanalogPlugin.AcquisitionFinished();
            //directshotPlugin.AcquisitionFinished();
            shotgatherer.AcquisitionFinished();
            AcquisitionFinishing();
            controllerInstance.GUIUpdate();
        }

        private double GetValue(string output)
        {
            string cSwitch = output.Substring(0, 3);
            double value = 0.0;

            switch (cSwitch)
            {
                case "tcl":
                    {
                        value = TCLGet(output);
                        break;
                    }
                case "Ana":
                    {
                        value = AnalogGet(output);
                        break;
                    }
                default:
                    {
                        break;
                    }
            }
            return value;
        }

        private double AnalogGet(string channel)
        {
            double value = 0;

            NationalInstruments.DAQmx.Task inputTask = new NationalInstruments.DAQmx.Task("input task");
            double min = -10;
            double max = 10;
            string daqchannel = "";
            if (channel == "Analog AO0")
            {
                daqchannel = "/dev1/AI2";
            }
            else if (channel == "Analog AO1")
            {
                daqchannel = "/dev1/AI5";
            }
            inputTask.AIChannels.CreateVoltageChannel(daqchannel, "readchan", NationalInstruments.DAQmx.AITerminalConfiguration.Rse
                , min, max, NationalInstruments.DAQmx.AIVoltageUnits.Volts);
            NationalInstruments.DAQmx.AnalogSingleChannelReader reader;
            reader = new NationalInstruments.DAQmx.AnalogSingleChannelReader(inputTask.Stream);
            value = reader.ReadSingleSample();
            inputTask.Dispose();
            return value;
        }

        private double TCLGet(string laser)
        {
            double value = tclController.GetLaserSetpoint(laser);
            return value;
        }

        private void MoveTo(string output, double newpoint)
        {
            string cSwitch = output.Substring(0, 3);

            switch (cSwitch)
            {
                case "tcl":
                {
                    TCLMove(output, newpoint);
                    break;
                }
                case "Ana":
                {
                    AnalogMove(output, newpoint);
                    break;
                }
                default:
                {
                    break;
                }
            }
            slowLaserValue = GetValue(Controller.GetController().laserSettings["SlowLaser"]);
            fastLaserValue = GetValue(Controller.GetController().laserSettings["FastLaser"]);
            UpdatePosArgs evArgs = new UpdatePosArgs();
            evArgs.position = new double[2];
            evArgs.position[1] = slowLaserValue;
            evArgs.position[0] = fastLaserValue;
            UpdatePosition(evArgs);
        }

        public void JogTo(string output, double newpoint)
        {
            MoveTo(Controller.GetController().laserSettings[output], newpoint);
            fastLaserValue = GetValue(Controller.GetController().laserSettings["FastLaser"]);
            Controller.GetController().scanSettings["FastPos"] = fastLaserValue;
            slowLaserValue = GetValue(Controller.GetController().laserSettings["SlowLaser"]);
            Controller.GetController().scanSettings["SlowPos"] = slowLaserValue;
            GUIUpdateEventArgs tempevArgs = new GUIUpdateEventArgs();
        }

        private void AnalogMove(string channel, double newpoint)
        {

            NationalInstruments.DAQmx.Task outputTask = new NationalInstruments.DAQmx.Task("output task");
            double min = -10;
            double max = 10; 
            if (channel == "Analog AO0")
            {
                min = -4;
                max = 4;
            }
            string daqchannel = "dev1/" + channel.Substring(7,3);
            outputTask.AOChannels.CreateVoltageChannel(daqchannel, "scanchan"
                , min, max, NationalInstruments.DAQmx.AOVoltageUnits.Volts);
            NationalInstruments.DAQmx.AnalogSingleChannelWriter writer;
            writer = new NationalInstruments.DAQmx.AnalogSingleChannelWriter(outputTask.Stream);
            writer.WriteSingleSample(true,newpoint);
            outputTask.Dispose();
        }

        private void TCLMove(string laser, double newpoint)
        {
            tclController.SetLaserSetpoint(laser, newpoint);
        }

        private void SoftStep(double currentpoint,string laser, double newpoint, int delay)
        {
            double minstep = 0.001;
            if (Controller.GetController().laserSettings[laser].Substring(0, 3) == "Ana")
            {
                minstep = 0.05;
            }

            if (Math.Abs(newpoint - currentpoint) >
               minstep)
            {
                Stopwatch softtimer = new System.Diagnostics.Stopwatch();
                softtimer.Start();

                int softloops = (int)Math.Floor(Math.Abs((newpoint - currentpoint) / minstep));
                for (int i = 0; i < softloops; i++)
                {
                    long softloop = softtimer.ElapsedMilliseconds;
                    MoveTo(Controller.GetController().laserSettings[laser], currentpoint + i * (newpoint - currentpoint) / softloops);
                    // check for exit
                    if (CheckIfStopping())
                    {
                        //OnScanFinished();

                        OnScanFinished();
                        return;
                    }
                    Thread.Sleep(delay);
                }
                if (backendState == ScanitorState.running)
                {
                    MoveTo(laser, newpoint);
                }

            }
            else
            {
                MoveTo(laser, newpoint);
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
                if (backendState == ScanitorState.stopping || backendState == ScanitorState.stopped)
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
            //smController = (ScanMaster.Controller) (Activator.GetObject(typeof(ScanMaster.Controller), "tcp://localhost:1170/controller.rem"));
            //smAcquisitor = smController.Acquisitor;
            //smConfig = smController.Acquisitor.Configuration;
        }

        public void SetLaserV(string channel,double setV)
        {
            tclController.SetLaserSetpoint(channel, setV);
        }

        protected virtual void TempUpdate(GUIUpdateEventArgs e)
        {
            if (GUIUpdate != null) GUIUpdate(this, e);
        }

        protected virtual void OnData(DataEventArgs e)
        {
            if (Data != null) Data(this, e);
        }

        protected virtual void OnScanFinished()
        {
            if (ScanFinished != null) ScanFinished(this, new EventArgs());
        }

        protected virtual void UpdatePosition(UpdatePosArgs e)
        {
            if (PosUpdate != null) PosUpdate(this, e);
        }
    }

    public class GUIUpdateEventArgs : EventArgs
    {
        public ScanPoint point;
    }

    public class DataEventArgs : EventArgs
    {
        public ScanPoint point;
    }

    public class UpdatePosArgs : EventArgs
    {
        public double[] position;
    }
}
