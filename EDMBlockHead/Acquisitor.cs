using System;
using System.Collections;
using System.Threading;
using NationalInstruments.DAQmx;

using DAQ.Environment;
using DAQ.FakeData;
using DAQ.HAL;
using Data;
using Data.EDM;
using EDMConfig;

using EDMBlockHead.Acquire.Channels;
using EDMBlockHead.Acquire.Input;

namespace EDMBlockHead.Acquire
{
	/// <summary>
	/// This class is the backend that takes EDM blocks.
	/// 
	/// One thing that is not obvious is the way that pattern output works. BlockHead asks ScanMaster
	/// to output the pattern. It selects the "Scan B" profile (heaven forbid if there isn't such a profile).
	/// This way there is no tedious mucking around with copying parameters. BlockHead copies out the
	/// pg settings from the profile into the block config. 
	/// </summary>
	public class Acquisitor
	{
		// stuff to coordinate the backend and frontend
		private Thread acquireThread;
		enum AcquisitorState {stopped, running, stopping};
		private AcquisitorState backendState = AcquisitorState.stopped;
		public Object MonitorLockObject = new Object();

		// the configuration for the block that is being taken
		private BlockConfig config;

		// daq variables
		ArrayList switchedChannels;
		ScannedAnalogInputCollection inputs;
        ScannedAnalogInputCollection magInputs;
		Task inputTask;
        Task singlePointInputTask;
 		AnalogMultiChannelReader inputReader;
        AnalogMultiChannelReader singlePointInputReader;
        ScanMaster.Controller scanMaster;
		EDMPhaseLock.MainForm phaseLock;
        //EDMHardwareControl.Controller hardwareController;         // new hardware controller
        UEDMHardwareControl.UEDMController hardwareController;
        EDMFieldLock.MainForm fieldLock;

		// calling this method starts acquisition
		public void Start(BlockConfig config) 
		{
			this.config = config;
			acquireThread = new Thread(new ThreadStart(this.Acquire));
			acquireThread.Name = "BlockHead Acquisitor";
			acquireThread.Priority = ThreadPriority.Highest;
			backendState = AcquisitorState.running;
			acquireThread.Start();
		}

        public void StartTargetStepper(BlockConfig config)
        {
            this.config = config;
            acquireThread = new Thread(new ThreadStart(this.AcquireAndStepTarget));
            acquireThread.Name = "BlockHead Acquisitor";
            acquireThread.Priority = ThreadPriority.Highest;
            backendState = AcquisitorState.running;
            acquireThread.Start();
        }

        public void StartMagDataAcquisition(BlockConfig config)
        {
            this.config = config;
            acquireThread = new Thread(new ThreadStart(this.AcquireMagData));
            acquireThread.Name = "BlockHead Acquisitor";
            acquireThread.Priority = ThreadPriority.Highest;
            backendState = AcquisitorState.running;
            acquireThread.Start();
        }

		// calling this method stops acquisition as soon as possible (usually after the current shot)
		public void Stop() 
		{
			lock(this)
			{
				backendState = AcquisitorState.stopping;
			}
		}


		// this is the method that actually takes the data. It is called by Start() and shouldn't
		// be called directly
		public void Acquire()
		{
			// lock onto something that the front end can see
			Monitor.Enter(MonitorLockObject);

			scanMaster = ScanMaster.Controller.GetController();
			phaseLock = new EDMPhaseLock.MainForm();
            //hardwareController = new EDMHardwareControl.Controller();     //UEDM use the new hardware controller
            hardwareController = new UEDMHardwareControl.UEDMController();
            //fieldLock = new EDMFieldLock.MainForm();
	
			// map modulations to physical channels
			MapChannels();

			// map the analog inputs
			MapAnalogInputs();

			Block b = new Block();
			b.Config = config;
			b.SetTimeStamp();
            foreach (ScannedAnalogInput channel in inputs.Channels)
            {
                b.detectors.Add(channel.Channel.Name);
            }
			
			try
			{
                Console.WriteLine("Trying to get SPData");
                // get things going
				AcquisitionStarting();
                
				// enter the main loop
				for (int point = 0 ; point < (int)config.Settings["numberOfPoints"] ; point++)
				{
					// set the switch states and impose the appropriate wait times
					ThrowSwitches(point);

					// take a point
					Shot s;
					EDMPoint p;
					if (Environs.Debug)
					{
						// just stuff a made up shot in
						//Thread.Sleep(10);
						s = DataFaker.GetFakeShot(1900,200,10,3,9);
						((TOF)s.TOFs[0]).Calibration = ((ScannedAnalogInput)inputs.Channels[0]).Calibration;
						p = new EDMPoint();
						p.Shot = s;
						//Thread.Sleep(20);
					}
					else
					{
 						// everything should be ready now so start the analog
						// input task (it will wait for a trigger)
						inputTask.Start();

						// get the raw data
                        double[,] analogData = inputReader.ReadMultiSample(inputs.GateLength);
                        inputTask.Stop();


						// extract the data for each scanned channel and put it in a TOF
						s = new Shot();
						for (int i = 0 ; i < inputs.Channels.Count ; i++)
						{
							// extract the raw data
							double[] rawData = new double[inputs.GateLength];
							for (int q = 0 ; q < inputs.GateLength ; q++) rawData[q] = analogData[i,q];
							
							ScannedAnalogInput ipt = (ScannedAnalogInput)inputs.Channels[i];
							// reduce the data
							double[] data = ipt.Reduce(rawData);
							TOF t = new TOF();
							t.Calibration = ipt.Calibration;
                            // the 1000000 is because clock period is in microseconds;
                            t.ClockPeriod = 1000000 / ipt.CalculateClockRate(inputs.RawSampleRate);
                            t.GateStartTime = inputs.GateStartTime;
                            // this is a bit confusing. The chop is measured in points, so the gate
                            // has to be adjusted by the number of points times the clock period!
                            if (ipt.ReductionMode == DataReductionMode.Chop)
                                t.GateStartTime += (ipt.ChopStart * t.ClockPeriod);
							t.Data = data;
							// the 1000000 is because clock period is in microseconds;
							t.ClockPeriod = 1000000 / ipt.CalculateClockRate(inputs.RawSampleRate);

							s.TOFs.Add(t);
						}

						p = new EDMPoint();
						p.Shot = s;

					}
					// do the "SinglePointData" (i.e. things that are measured once per point)
                    // We'll save the leakage monitor until right at the end.
					// keep an eye on what the phase lock is doing
                    p.SinglePointData.Add("PhaseLockFrequency", phaseLock.OutputFrequency);
                    p.SinglePointData.Add("PhaseLockError", phaseLock.PhaseError);
                    // scan the analog inputs
                    double[] spd;
                    // fake some data if we're in debug mode
                    if (Environs.Debug)
                    {
                        spd = new double[7];
                        spd[0] = 1;
                        spd[1] = 2;
                        spd[2] = 3;
                        spd[3] = 4;
                        spd[4] = 5;
                        spd[5] = 6;
                        spd[6] = 7;
                    }
                    else
                    {
                        singlePointInputTask.Start();
                        spd = singlePointInputReader.ReadSingleSample();
                        singlePointInputTask.Stop();
                    }
                    //p.SinglePointData.Add("topPD", spd[0]);
                    //p.SinglePointData.Add("bottomPD", spd[1]);
                    p.SinglePointData.Add("MiniFlux1", spd[0]);//near HV supplies
                    //p.SinglePointData.Add("MiniFlux2", spd[1]);
                    //p.SinglePointData.Add("MiniFlux3", spd[2]);
                    //p.SinglePointData.Add("CplusV", spd[5]);
                    //p.SinglePointData.Add("CminusV", spd[6]);
                    //p.SinglePointData.Add("ValveMonV", spd[3]);

                    //hardwareController.UpdateVMonitor();
                    //p.SinglePointData.Add("CplusV", hardwareController.CPlusMonitorVoltage);
                    //hardwareController.UpdateLaserPhotodiodes();
                    //p.SinglePointData.Add("ProbePD", hardwareController.ProbePDVoltage);
                    //p.SinglePointData.Add("PumpPD", hardwareController.PumpPDVoltage);
                    //hardwareController.UpdateMiniFluxgates();
                    //p.SinglePointData.Add("MiniFlux1", hardwareController.MiniFlux1Voltage);
                    //p.SinglePointData.Add("MiniFlux2", hardwareController.MiniFlux2Voltage);
                    //p.SinglePointData.Add("MiniFlux3", hardwareController.MiniFlux3Voltage);
                    hardwareController.ReadIMonitor();
                    //p.SinglePointData.Add("NorthCurrent", hardwareController.NorthCurrent);
                    //p.SinglePointData.Add("SouthCurrent", hardwareController.SouthCurrent);
                    p.SinglePointData.Add("WestCurrent", hardwareController.WestCurrent);
                    p.SinglePointData.Add("EastCurrent", hardwareController.EastCurrent);
                    //hardwareController.UpdatePiMonitor();
                    //p.SinglePointData.Add("piMonitor", hardwareController.PiFlipMonVoltage);
                    //p.SinglePointData.Add("CminusV", hardwareController.CMinusMonitorVoltage);

                    // Hopefully the leakage monitors will have finished reading by now.
                    // We join them, read out the data, and then launch another asynchronous
                    // acquisition. [If this is the first shot of the block, the leakage monitor
                    // measurement will have been launched in AcquisitionStarting() ].
                    //hardwareController.WaitForIMonitorAsync();
                    //p.SinglePointData.Add("NorthCurrent", hardwareController.NorthCurrent); 
                    //p.SinglePointData.Add("SouthCurrent", hardwareController.SouthCurrent);
                    //hardwareController.UpdateIMonitorAsync();

                    // randomise the Ramsey phase
                    // TODO: check whether the .NET rng is good enough
                    // TODO: reference where this number comes from
                    //double d = 2.3814 * (new Random().NextDouble());
                    //hardwareController.SetScramblerVoltage(d);

                    b.Points.Add(p);

					// update the front end
					Controller.GetController().GotPoint(point, p);

					if (CheckIfStopping()) 
					{
						// release hardware
						AcquisitionStopping();
						// signal anybody waiting on the lock that we're done
						Monitor.Pulse(MonitorLockObject);
						Monitor.Exit(MonitorLockObject);
						return;
					}
				}
			}		
			catch (Exception e)
			{
				// try and stop the experiment gracefully
				try
				{
					AcquisitionStopping();
				}
				catch (Exception) {}				// about the best that can be done at this stage
				Monitor.Pulse(MonitorLockObject);
				Monitor.Exit(MonitorLockObject);
				throw e;
			}

			AcquisitionStopping();
			
			// hand the new block back to the controller
			Controller.GetController().AcquisitionFinished(b);

			// signal anybody waiting on the lock that we're done
			Monitor.Pulse(MonitorLockObject);
			Monitor.Exit(MonitorLockObject);

		}

        public void AcquireAndStepTarget()
        {
            // lock onto something that the front end can see
            Monitor.Enter(MonitorLockObject);

            scanMaster = ScanMaster.Controller.GetController();
            phaseLock = new EDMPhaseLock.MainForm();
            //hardwareController = new EDMHardwareControl.Controller(); // new hardware controller
            hardwareController = new UEDMHardwareControl.UEDMController();

            // map modulations to physical channels
            MapChannels();

            // map the analog inputs
            MapAnalogInputs();

            Block b = new Block();
            b.Config = config;
            b.SetTimeStamp();

             

            foreach (ScannedAnalogInput channel in inputs.Channels)
            {
                b.detectors.Add(channel.Channel.Name);
            }

            try
            {
                // get things going
                AcquisitionStarting();
                int point = 0;
                // enter the main loop
                for (point = 0; point < (int)config.Settings["maximumNumberOfTimesToStepTarget"]; point++)
                {

                    // take a point
                    Shot s;
                    EDMPoint p;
                    if (Environs.Debug)
                    {
                        // just stuff a made up shot in
                        //Thread.Sleep(10);
                        s = DataFaker.GetFakeShot(1900, 200, 10, 3, 3);
                        ((TOF)s.TOFs[0]).Calibration = ((ScannedAnalogInput)inputs.Channels[0]).Calibration;
                        p = new EDMPoint();
                        p.Shot = s;
                        //Thread.Sleep(20);
                    }
                    else
                    {
                        // everything should be ready now so start the analog
                        // input task (it will wait for a trigger)
                        inputTask.Start();

                        // get the raw data
                        double[,] analogData = inputReader.ReadMultiSample(inputs.GateLength);
                        inputTask.Stop();


                        // extract the data for each scanned channel and put it in a TOF
                        s = new Shot();
                        for (int i = 0; i < inputs.Channels.Count; i++)
                        {
                            // extract the raw data
                            double[] rawData = new double[inputs.GateLength];
                            for (int q = 0; q < inputs.GateLength; q++) rawData[q] = analogData[i, q];

                            ScannedAnalogInput ipt = (ScannedAnalogInput)inputs.Channels[i];
                            // reduce the data
                            double[] data = ipt.Reduce(rawData);
                            TOF t = new TOF();
                            t.Calibration = ipt.Calibration;
                            // the 1000000 is because clock period is in microseconds;
                            t.ClockPeriod = 1000000 / ipt.CalculateClockRate(inputs.RawSampleRate);
                            t.GateStartTime = inputs.GateStartTime;
                            // this is a bit confusing. The chop is measured in points, so the gate
                            // has to be adjusted by the number of points times the clock period!
                            if (ipt.ReductionMode == DataReductionMode.Chop)
                                t.GateStartTime += (ipt.ChopStart * t.ClockPeriod);
                            t.Data = data;
                            // the 1000000 is because clock period is in microseconds;
                            t.ClockPeriod = 1000000 / ipt.CalculateClockRate(inputs.RawSampleRate);

                            s.TOFs.Add(t);
                        }

                        p = new EDMPoint();
                        p.Shot = s;

                    }
                    // do the "SinglePointData" (i.e. things that are measured once per point)
                    // We'll save the leakage monitor until right at the end.
                    // keep an eye on what the phase lock is doing
                    p.SinglePointData.Add("PhaseLockFrequency", phaseLock.OutputFrequency);
                    p.SinglePointData.Add("PhaseLockError", phaseLock.PhaseError);
                    // scan the analog inputs
                    double[] spd;
                    // fake some data if we're in debug mode
                    if (Environs.Debug)
                    {
                        spd = new double[7];
                        spd[0] = 1;
                        spd[1] = 2;
                        spd[2] = 3;
                        spd[3] = 4;
                        spd[4] = 5;
                        spd[5] = 6;
                        spd[6] = 7;
                    }
                    else
                    {
                        singlePointInputTask.Start();
                        spd = singlePointInputReader.ReadSingleSample();
                        singlePointInputTask.Stop();
                    }
                    p.SinglePointData.Add("topPD", spd[0]);
                    p.SinglePointData.Add("bottomPD", spd[1]);
                    p.SinglePointData.Add("MiniFlux1", spd[2]);
                    p.SinglePointData.Add("MiniFlux2", spd[3]);
                    p.SinglePointData.Add("MiniFlux3", spd[4]);
                    //p.SinglePointData.Add("CplusV", spd[5]);
                    //p.SinglePointData.Add("CminusV", spd[6]);
                    p.SinglePointData.Add("ValveMonV", spd[5]);

                    //hardwareController.UpdateVMonitor();
                    //p.SinglePointData.Add("CplusV", hardwareController.CPlusMonitorVoltage);
                    //hardwareController.UpdateLaserPhotodiodes();
                    //p.SinglePointData.Add("ProbePD", hardwareController.ProbePDVoltage);
                    //p.SinglePointData.Add("PumpPD", hardwareController.PumpPDVoltage);
                    //hardwareController.UpdateMiniFluxgates();
                    //p.SinglePointData.Add("MiniFlux1", hardwareController.MiniFlux1Voltage);
                    //p.SinglePointData.Add("MiniFlux2", hardwareController.MiniFlux2Voltage);
                    //p.SinglePointData.Add("MiniFlux3", hardwareController.MiniFlux3Voltage);
                    hardwareController.ReadIMonitor();
                    //p.SinglePointData.Add("NorthCurrent", hardwareController.NorthCurrent);
                    //p.SinglePointData.Add("SouthCurrent", hardwareController.SouthCurrent);
                    p.SinglePointData.Add("WestCurrent", hardwareController.WestCurrent);
                    p.SinglePointData.Add("EastCurrent", hardwareController.EastCurrent);

                    b.Points.Add(p);

                    // update the front end
                    Controller.GetController().GotPoint(point, p);


                    // Integrate the first toff and stop the sequence if the signal is sufficiently high
                    TOF detectorATOF = new TOF(); 
                    detectorATOF = (TOF)s.TOFs[0];
                    double sig = detectorATOF.Integrate((double)config.Settings["targetStepperGateStartTime"], (double)config.Settings["targetStepperGateEndTime"]);
                    if (sig > (double)config.Settings["minimumSignalToRun"])
                    {
                        Controller.GetController().TargetHealthy = true; 
                        Stop();
                        point = (int)config.Settings["maximumNumberOfTimesToStepTarget"] + 1;

                    }
                    else
                    {
                        Controller.GetController().TargetHealthy = false;
                        //hardwareController.StepTarget(2);
                    }
                    //if (CheckIfStopping())
                    //{
                        // release hardware
                    //    AcquisitionStopping();
                        // signal anybody waiting on the lock that we're done
                    //    Monitor.Pulse(MonitorLockObject);
                    //    Monitor.Exit(MonitorLockObject);
                    //    point = (int)config.Settings["maximumNumberOfTimesToStepTarget"];
                    //    return;
                    //}
                }
                AcquisitionStopping();

                // hand the new block back to the controller
                Controller.GetController().IntelligentAcquisitionFinished();

                // signal anybody waiting on the lock that we're done
                Monitor.Pulse(MonitorLockObject);
                Monitor.Exit(MonitorLockObject);
            }
            catch (Exception e)
            {
                // try and stop the experiment gracefully
                try
                {
                    AcquisitionStopping();
                }
                catch (Exception) { }				// about the best that can be done at this stage
                Monitor.Pulse(MonitorLockObject);
                Monitor.Exit(MonitorLockObject);
                throw e;
            }

        }

        public void AcquireMagData()
        {
            // lock onto something that the front end can see
            Monitor.Enter(MonitorLockObject);

            scanMaster = ScanMaster.Controller.GetController();
            phaseLock = new EDMPhaseLock.MainForm();
            //hardwareController = new EDMHardwareControl.Controller();     // new hardware controller
            hardwareController = new UEDMHardwareControl.UEDMController();

            // map modulations to physical channels
            MapChannels();

            // map the analog inputs
            MapMagInputs();

            Block b = new Block();
            b.Config = config;
            b.SetTimeStamp();
            foreach (ScannedAnalogInput channel in magInputs.Channels)
            {
                b.detectors.Add(channel.Channel.Name);
            }

            try
            {
                // get things going
                MagAcquisitionStarting();

                // enter the main loop
                for (int point = 0; point < (int)config.Settings["numberOfPoints"]; point++)
                {
                    // set the switch states and impose the appropriate wait times
                    var stopwatch = System.Diagnostics.Stopwatch.StartNew();
                    
                    ThrowSwitches(point);

                    stopwatch.Stop();
                    Console.WriteLine("Time to switch = " + stopwatch.ElapsedMilliseconds+" ms");
                    // take a point
                    Shot s;
                    EDMPoint p;
                    if (Environs.Debug)
                    {
                        // just stuff a made up shot in
                        //Thread.Sleep(10);
                        s = DataFaker.GetFakeShot(1900, 50, 10, 3, 3);
                        ((TOF)s.TOFs[0]).Calibration = ((ScannedAnalogInput)inputs.Channels[0]).Calibration;
                        p = new EDMPoint();
                        p.Shot = s;
                        //Thread.Sleep(20);
                    }
                    else
                    {
                        // everything should be ready now so start the analog
                        // input task (it will wait for a trigger)
                        var stopwatchAI = System.Diagnostics.Stopwatch.StartNew();
                        inputTask.Start();
                        
                        // get the raw data
                        double[,] analogData = inputReader.ReadMultiSample(magInputs.GateLength);
                        inputTask.Stop();
                        stopwatchAI.Stop();
                        Console.WriteLine("Time to collect data = " + stopwatchAI.ElapsedMilliseconds + " ms");


                        // extract the data for each scanned channel and put it in a TOF
                        s = new Shot();
                        var stopwatchstoringAI = System.Diagnostics.Stopwatch.StartNew();
                        for (int i = 0; i < magInputs.Channels.Count; i++)
                        {
                            // extract the raw data
                            double[] rawData = new double[magInputs.GateLength];
                            for (int q = 0; q < magInputs.GateLength; q++) rawData[q] = analogData[i, q];

                            ScannedAnalogInput ipt = (ScannedAnalogInput)magInputs.Channels[i];
                            // reduce the data
                            double[] data = ipt.Reduce(rawData);
                            TOF t = new TOF();
                            t.Calibration = ipt.Calibration;
                            // the 1000000 is because clock period is in microseconds;
                            t.ClockPeriod = 1000000 / ipt.CalculateClockRate(magInputs.RawSampleRate);
                            t.GateStartTime = magInputs.GateStartTime;
                            // this is a bit confusing. The chop is measured in points, so the gate
                            // has to be adjusted by the number of points times the clock period!
                            if (ipt.ReductionMode == DataReductionMode.Chop)
                                t.GateStartTime += (ipt.ChopStart * t.ClockPeriod);
                            t.Data = data;
                            // the 1000000 is because clock period is in microseconds;
                            t.ClockPeriod = 1000000 / ipt.CalculateClockRate(magInputs.RawSampleRate);

                            s.TOFs.Add(t);
                        }

                        p = new EDMPoint();
                        p.Shot = s;
                        stopwatchstoringAI.Stop();
                        Console.WriteLine("Time to store data = " + stopwatchstoringAI.ElapsedMilliseconds + " ms");

                    }
                    // do the "SinglePointData" (i.e. things that are measured once per point)
                    // We'll save the leakage monitor until right at the end.
                    // keep an eye on what the phase lock is doing

                    var stopwatchsinglepoint = System.Diagnostics.Stopwatch.StartNew();
                    //p.SinglePointData.Add("PhaseLockFrequency", phaseLock.OutputFrequency);
                    //p.SinglePointData.Add("PhaseLockError", phaseLock.PhaseError);
                    // scan the analog inputs
                    double[] spd;
                    // fake some data if we're in debug mode
                    //if (Environs.Debug)
                    //{
                    spd = new double[7];
                    spd[0] = 1;
                    spd[1] = 2;
                    spd[2] = 3;
                    //}
                    //else
                    //{
                    //    singlePointInputTask.Start();
                    //    spd = singlePointInputReader.ReadSingleSample();
                    //    singlePointInputTask.Stop();
                    //}
                    p.SinglePointData.Add("MiniFlux1", spd[0]);
                    //p.SinglePointData.Add("MiniFlux2", spd[1]);
                    //p.SinglePointData.Add("MiniFlux3", spd[2]);

                    //hardwareController.ReadIMonitor();
                    //p.SinglePointData.Add("NorthCurrent", hardwareController.NorthCurrent);
                    //p.SinglePointData.Add("SouthCurrent", hardwareController.SouthCurrent);
                    p.SinglePointData.Add("WestCurrent", spd[1]);// hardwareController.WestCurrent);
                    p.SinglePointData.Add("EastCurrent", spd[2]);// hardwareController.EastCurrent);

                    b.Points.Add(p);
                    stopwatchsinglepoint.Stop();
                    Console.WriteLine("Time to collect and store single point data = " + stopwatchsinglepoint.ElapsedMilliseconds + " ms");

                    // update the front end
                    Controller.GetController().GotPoint(point, p);

                    if (CheckIfStopping())
                    {
                        // release hardware
                        AcquisitionStopping();
                        // signal anybody waiting on the lock that we're done
                        Monitor.Pulse(MonitorLockObject);
                        Monitor.Exit(MonitorLockObject);
                        return;
                    }
                }
            }
            catch (Exception e)
            {
                // try and stop the experiment gracefully
                try
                {
                    AcquisitionStopping();
                }
                catch (Exception) { }				// about the best that can be done at this stage
                Monitor.Pulse(MonitorLockObject);
                Monitor.Exit(MonitorLockObject);
                throw e;
            }

            AcquisitionStopping();

            // hand the new block back to the controller
            Controller.GetController().MagDataAcquisitionFinished(b);

            // signal anybody waiting on the lock that we're done
            Monitor.Pulse(MonitorLockObject);
            Monitor.Exit(MonitorLockObject);

        }

		private void MapChannels()
		{
			switchedChannels = new ArrayList();
            hardwareController = new UEDMHardwareControl.UEDMController();        //new hardware controller EDMHardwareControl.Controller();

            TTLSwitchedChannel bChan = new TTLSwitchedChannel();
            bChan.Channel = "bSwitch";
            bChan.Invert = false;
            bChan.Modulation = config.GetModulationByName("B");
            switchedChannels.Add(bChan);

            TTLSwitchedChannel notBChan = new TTLSwitchedChannel();
            notBChan.Channel = "notB";
            notBChan.Invert = true;
            notBChan.Modulation = config.GetModulationByName("B");
            switchedChannels.Add(notBChan);

            TTLSwitchedChannel dbChan = new TTLSwitchedChannel();
            dbChan.Channel = "dB";
            dbChan.Invert = false;
            dbChan.Modulation = config.GetModulationByName("DB");
            switchedChannels.Add(dbChan);

            TTLSwitchedChannel notDBChan = new TTLSwitchedChannel();
            notDBChan.Channel = "notDB";
            notDBChan.Invert = true;
            notDBChan.Modulation = config.GetModulationByName("DB");
            switchedChannels.Add(notDBChan);

            //TTLSwitchedChannel piChan = new TTLSwitchedChannel();
            //piChan.Channel = "piFlipEnable";
            //piChan.Invert = false;
            //piChan.Modulation = config.GetModulationByName("PI");
            //switchedChannels.Add(piChan);

            //TTLSwitchedChannel testPlateVoltageChan = new TTLSwitchedChannel();
            //testPlateVoltageChan.Channel = "testPlateVoltageTTL";
            //testPlateVoltageChan.Invert = false;
            //testPlateVoltageChan.Modulation = config.GetModulationByName("E");
            //switchedChannels.Add(testPlateVoltageChan);

            //TTLSwitchedChannel notPIChan = new TTLSwitchedChannel();
            //notPIChan.Channel = "notPIFlipEnable";
            //notPIChan.Invert = true;
            //notPIChan.Modulation = config.GetModulationByName("PI");
            //switchedChannels.Add(notPIChan);

            //ESwitchChannel eChan = new ESwitchChannel();
            //eChan.Invert = false;
            //eChan.Modulation = config.GetModulationByName("E");
            //switchedChannels.Add(eChan);

            //ESwitchRFChannel eChan = new ESwitchRFChannel();
            //eChan.Invert = false;
            //eChan.Modulation = config.GetModulationByName("E");
            //eChan.stepSizeRF = +0.25;
            //switchedChannels.Add(eChan);

            //AnalogSwitchedChannel piVoltageChannel = new AnalogSwitchedChannel();
            //piVoltageChannel.Channel = "piFlipVoltage";
            ////piVoltageChannel.Offset = hardwareController.PiFlipVoltage;
            //piVoltageChannel.Modulation = config.GetModulationByName("RF2F");
            //switchedChannels.Add(piVoltageChannel);

            //AnalogSwitchedChannel rf1AChannel = new AnalogSwitchedChannel();
            //rf1AChannel.Channel = "rf1Attenuator";
            //rf1AChannel.Offset = 0.0;
            //rf1AChannel.Modulation = config.GetModulationByName("RF1A");
            //switchedChannels.Add(rf1AChannel);

            //AnalogSwitchedChannel rf2AChannel = new AnalogSwitchedChannel();
            //rf2AChannel.Channel = "rf2Attenuator";
            //rf2AChannel.Offset = 0.0;
            //rf2AChannel.Modulation = config.GetModulationByName("RF2A");
            //switchedChannels.Add(rf2AChannel);

            //AnalogSwitchedChannel rf1FChannel = new AnalogSwitchedChannel();
            //rf1FChannel.Channel = "rf1FM";
            //rf1FChannel.Offset = 0.0;
            //rf1FChannel.Modulation = config.GetModulationByName("RF1F");
            //switchedChannels.Add(rf1FChannel);

            //AnalogSwitchedChannel rf2FChannel = new AnalogSwitchedChannel();
            //rf2FChannel.Channel = "rf2FM";
            //rf2FChannel.Offset = 0.0;
            //rf2FChannel.Modulation = config.GetModulationByName("RF2F");
            //switchedChannels.Add(rf2FChannel);

            //HardwareControllerSwitchChannel bChan = new HardwareControllerSwitchChannel();
            //bChan.Channel = "bSwitch";
            //bChan.Invert = false;
            //bChan.Modulation = config.GetModulationByName("B");
            //switchedChannels.Add(bChan);

            //HardwareControllerSwitchChannel dbChan = new HardwareControllerSwitchChannel();
            //dbChan.Channel = "dB";
            //dbChan.Invert = false;
            //dbChan.Modulation = config.GetModulationByName("DB");
            //switchedChannels.Add(dbChan);

            HardwareControllerSwitchChannel eChan = new HardwareControllerSwitchChannel();
            eChan.Channel = "eChan";
            eChan.Modulation = config.GetModulationByName("E");
            switchedChannels.Add(eChan);

            //HardwareControllerSwitchChannel mwChan = new HardwareControllerSwitchChannel();
            //mwChan.Channel = "mwChan";
            //mwChan.Modulation = config.GetModulationByName("MW");
            //switchedChannels.Add(mwChan);

            //AnalogSwitchedChannel lf1Channel = new AnalogSwitchedChannel();
            //lf1Channel.Channel = "flPZT";
            //lf1Channel.Modulation = config.GetModulationByName("LF1");
            //switchedChannels.Add(lf1Channel);

            //HardwareControllerSwitchChannel lf1Channel = new HardwareControllerSwitchChannel();
            //lf1Channel.Channel = "probeAOM";
            //lf1Channel.Modulation = config.GetModulationByName("LF1");
            //switchedChannels.Add(lf1Channel);

            //HardwareControllerSwitchChannel lf2Channel = new HardwareControllerSwitchChannel();
            //lf2Channel.Channel = "pumpAOM";
            //lf2Channel.Modulation = config.GetModulationByName("LF2");
            //switchedChannels.Add(lf2Channel);

            //AnalogSwitchedChannel lf3Channel = new HardwareControllerSwitchChannel();
            //lf3Channel.Channel = "LF3";
            //lf3Channel.Modulation = config.GetModulationByName("LF3");
            //switchedChannels.Add(lf3Channel);

        }

        #region Map Inputs
        /* THIS VERSION FOR AR */
        // this sets up the scanned analog inputs. It's complicated a bit by the fact that
        // each input would ideally have a different clock rate and gateLength. The board
        // doesn't support that though.
        public void MapAnalogInputs()
        {
            inputs = new ScannedAnalogInputCollection();
            inputs.RawSampleRate = 10000; 
            inputs.GateStartTime = (int)scanMaster.GetShotSetting("gateStartTime"); // Classic used ~700
            inputs.GateLength = 1200; //Classic used 280

            ScannedAnalogInput detectorA = new ScannedAnalogInput();
            detectorA.Channel = (AnalogInputChannel)Environs.Hardware.AnalogInputChannels["detectorA"];
            detectorA.ReductionMode = DataReductionMode.Chop;
            detectorA.ChopStart = 0; //Classic had 140
            detectorA.ChopLength = 1140; //Classic had 80
            detectorA.LowLimit = 0;
            detectorA.HighLimit = 10;
            detectorA.Calibration = 225;
            inputs.Channels.Add(detectorA);

            ScannedAnalogInput detectorB = new ScannedAnalogInput();
            detectorB.Channel = (AnalogInputChannel)Environs.Hardware.AnalogInputChannels["detectorB"];
            detectorB.ReductionMode = DataReductionMode.Chop;
            detectorB.ChopStart = 0; //Classic had 180
            detectorB.ChopLength = 1140; //Classic had 80
            detectorB.LowLimit = 0;
            detectorB.HighLimit = 10;
            detectorB.Calibration = 225;
            inputs.Channels.Add(detectorB);

            //ScannedAnalogInput bottomProbe = new ScannedAnalogInput();
            //bottomProbe.Channel = (AnalogInputChannel)Environs.Hardware.AnalogInputChannels["bottomProbe"];
            //bottomProbe.ReductionMode = DataReductionMode.Chop;
            //bottomProbe.ChopStart = 140;
            //bottomProbe.ChopLength = 80;
            //bottomProbe.LowLimit = 0;
            //bottomProbe.HighLimit = 10;
            //bottomProbe.Calibration = 510; 
            //inputs.Channels.Add(bottomProbe);

            //ScannedAnalogInput topProbe = new ScannedAnalogInput();
            //topProbe.Channel = (AnalogInputChannel)Environs.Hardware.AnalogInputChannels["topProbe"];
            //topProbe.ReductionMode = DataReductionMode.Chop;
            //topProbe.ChopStart = 180;
            //topProbe.ChopLength = 80;
            //topProbe.LowLimit = 0;
            //topProbe.HighLimit = 10;
            //topProbe.Calibration = 510;
            //inputs.Channels.Add(topProbe);

            //ScannedAnalogInput mag = new ScannedAnalogInput();
            //mag.ReductionMode = DataReductionMode.Average;
            //mag.Channel = (AnalogInputChannel)Environs.Hardware.AnalogInputChannels["magnetometer"];
            //mag.AverageEvery = 20;
            //mag.LowLimit = -10;
            //mag.HighLimit = 10;
            //mag.Calibration = 0.00001;
            //inputs.Channels.Add(mag);

            ScannedAnalogInput mag = new ScannedAnalogInput();
            mag.ReductionMode = DataReductionMode.Average;
            mag.Channel = (AnalogInputChannel)Environs.Hardware.AnalogInputChannels["bartington_Y"];
            mag.AverageEvery = 800;
            mag.LowLimit = -10;
            mag.HighLimit = 10;
            mag.Calibration = 1e-5; //Bartington calibration is 1V = 10uT
            inputs.Channels.Add(mag);

            //ScannedAnalogInput gnd = new ScannedAnalogInput();
            //gnd.Channel = (AnalogInputChannel)Environs.Hardware.AnalogInputChannels["gnd"];
            //gnd.ReductionMode = DataReductionMode.Average;
            //gnd.AverageEvery = 20;
            //gnd.LowLimit = -1;
            //gnd.HighLimit = 1;
            //gnd.Calibration = 1;
            //inputs.Channels.Add(gnd);

            ScannedAnalogInput battery = new ScannedAnalogInput();
            battery.Channel = (AnalogInputChannel)Environs.Hardware.AnalogInputChannels["battery"];
            battery.ReductionMode = DataReductionMode.Chop;
            battery.ChopStart = 140;
            battery.ChopLength = 120;
            battery.LowLimit = 0;
            battery.HighLimit = 10;
            battery.Calibration = 1;
            inputs.Channels.Add(battery);

            /*ScannedAnalogInput battery2 = new ScannedAnalogInput();
            battery2.ReductionMode = DataReductionMode.Average;
            battery2.Channel = (AnalogInputChannel)Environs.Hardware.AnalogInputChannels["battery2"];
            battery2.AverageEvery = 20;
            battery2.LowLimit = -10;
            battery2.HighLimit = 10;
            battery2.Calibration = 1.0e-9 / 0.9; // analog output calibration is 0.9 V/nT
            inputs.Channels.Add(battery2);*/

            //ScannedAnalogInput rfCurrent = new ScannedAnalogInput();
            //rfCurrent.ReductionMode = DataReductionMode.Average;
            //rfCurrent.Channel = (AnalogInputChannel)Environs.Hardware.AnalogInputChannels["rfCurrent"];
            //rfCurrent.AverageEvery = 10; //Bandwidth of the ammeter is aprox 12kHz
            //rfCurrent.LowLimit = -10;
            //rfCurrent.HighLimit = 10;
            //inputs.Channels.Add(rfCurrent);

            //ScannedAnalogInput reflectedrfAmplitude = new ScannedAnalogInput();
            //reflectedrfAmplitude.Channel = (AnalogInputChannel)Environs.Hardware.AnalogInputChannels["reflectedrfAmplitude"];
            //reflectedrfAmplitude.ReductionMode = DataReductionMode.Chop;
            //reflectedrfAmplitude.ChopStart = 0;
            //reflectedrfAmplitude.ChopLength = 150;
            //reflectedrfAmplitude.LowLimit = -10;
            //reflectedrfAmplitude.HighLimit = 10;
            //inputs.Channels.Add(reflectedrfAmplitude);

            //ScannedAnalogInput incidentrfAmplitude = new ScannedAnalogInput();
            //incidentrfAmplitude.Channel = (AnalogInputChannel)Environs.Hardware.AnalogInputChannels["incidentrfAmplitude"];
            //incidentrfAmplitude.ReductionMode = DataReductionMode.Chop;
            //incidentrfAmplitude.ChopStart = 0;
            //incidentrfAmplitude.ChopLength = 150;
            //incidentrfAmplitude.LowLimit = -10;
            //incidentrfAmplitude.HighLimit = 10;
            //inputs.Channels.Add(incidentrfAmplitude);

            /*ScannedAnalogInput quSPinFSY = new ScannedAnalogInput();
            quSPinFSY.ReductionMode = DataReductionMode.Average;
            quSPinFSY.Channel = (AnalogInputChannel)Environs.Hardware.AnalogInputChannels["quSpinFS_Y"];
            quSPinFSY.AverageEvery = 20;
            quSPinFSY.LowLimit = -10;
            quSPinFSY.HighLimit = 10;
            quSPinFSY.Calibration = 1.0;
            inputs.Channels.Add(quSPinFSY);

            ScannedAnalogInput quSPinFSZ = new ScannedAnalogInput();
            quSPinFSZ.ReductionMode = DataReductionMode.Average;
            quSPinFSZ.Channel = (AnalogInputChannel)Environs.Hardware.AnalogInputChannels["quSpinFS_Z"];
            quSPinFSZ.AverageEvery = 20;
            quSPinFSZ.LowLimit = -10;
            quSPinFSZ.HighLimit = 10;
            quSPinFSZ.Calibration = 1.0;
            inputs.Channels.Add(quSPinFSZ);*/

        }

        // This version for magnetometer data taking with the QuSpins
        public void MapMagInputs()
        {
            DataReductionMode quspinReductionMode = DataReductionMode.Average;
            Int32 quspinChopStart = 0;
            Int32 quspinChopLength = 1140;
            Int32 quspinAverageEvery = 20;
            double quspinLowerLim = -5.5;
            double quspinUpperLim = 5.5;
            double quspinCalibration = 1.0e-9 / 8.1;


            magInputs = new ScannedAnalogInputCollection();
            magInputs.RawSampleRate = 10000;
            magInputs.GateStartTime = (int)scanMaster.GetShotSetting("gateStartTime");
            magInputs.GateLength = (int)(((int)scanMaster.GetPGSetting("flashlampPulseInterval")-50000)/(1000000/magInputs.RawSampleRate));//Changed from 1200 to scanMaster related to get as much data per shot //usually this is 280, I changed this to take more mag data per block (10 June 2021)

            Console.WriteLine((magInputs.GateLength).ToString());

            ScannedAnalogInput mag = new ScannedAnalogInput();
            mag.ReductionMode = DataReductionMode.Average;
            mag.Channel = (AnalogInputChannel)Environs.Hardware.AnalogInputChannels["bartington_Y"];
            mag.AverageEvery = 20;
            mag.LowLimit = -10;
            mag.HighLimit = 10;
            mag.Calibration = 1.0e-5; // bartington calibration is 1V = 10uT
            magInputs.Channels.Add(mag);

            ScannedAnalogInput fvy = new ScannedAnalogInput();
            fvy.Channel = (AnalogInputChannel)Environs.Hardware.AnalogInputChannels["quSpinFV_Y"];
            fvy.ReductionMode = quspinReductionMode;
            fvy.ChopStart = quspinChopStart;
            fvy.ChopLength = quspinChopLength;
            fvy.AverageEvery = quspinAverageEvery;
            fvy.LowLimit = quspinLowerLim;
            fvy.HighLimit = quspinUpperLim;
            fvy.Calibration = quspinCalibration; // analog output calibration is 2.7 V/nT
            magInputs.Channels.Add(fvy);

            ScannedAnalogInput hty = new ScannedAnalogInput();
            hty.Channel = (AnalogInputChannel)Environs.Hardware.AnalogInputChannels["quSpinHT_Y"];
            hty.ReductionMode = quspinReductionMode;
            hty.ChopStart = quspinChopStart;
            hty.ChopLength = quspinChopLength;
            hty.AverageEvery = quspinAverageEvery;
            hty.LowLimit = quspinLowerLim;
            hty.HighLimit = quspinUpperLim;
            hty.Calibration = quspinCalibration; // analog output calibration is 2.7 V/nT
            magInputs.Channels.Add(hty);

            ScannedAnalogInput hsy = new ScannedAnalogInput();
            hsy.Channel = (AnalogInputChannel)Environs.Hardware.AnalogInputChannels["quSpinHS_Y"];
            hsy.ReductionMode = quspinReductionMode;
            hsy.ChopStart = quspinChopStart;
            hsy.ChopLength = quspinChopLength;
            hsy.AverageEvery = quspinAverageEvery;
            hsy.LowLimit = quspinLowerLim;
            hsy.HighLimit = quspinUpperLim;
            hsy.Calibration = quspinCalibration; // analog output calibration is 2.7 V/nT
            magInputs.Channels.Add(hsy);

            ScannedAnalogInput hry = new ScannedAnalogInput();
            hry.Channel = (AnalogInputChannel)Environs.Hardware.AnalogInputChannels["quSpinHR_Y"];
            hry.ReductionMode = quspinReductionMode;
            hry.ChopStart = quspinChopStart;
            hry.ChopLength = quspinChopLength;
            hry.AverageEvery = quspinAverageEvery;
            hry.LowLimit = quspinLowerLim;
            hry.HighLimit = quspinUpperLim;
            hry.Calibration = quspinCalibration; // analog output calibration is 2.7 V/nT
            magInputs.Channels.Add(hry);

            ScannedAnalogInput hqy = new ScannedAnalogInput();
            hqy.Channel = (AnalogInputChannel)Environs.Hardware.AnalogInputChannels["quSpinHQ_Y"];
            hqy.ReductionMode = quspinReductionMode;
            hqy.ChopStart = quspinChopStart;
            hqy.ChopLength = quspinChopLength;
            hqy.AverageEvery = quspinAverageEvery;
            hqy.LowLimit = quspinLowerLim;
            hqy.HighLimit = quspinUpperLim;
            hqy.Calibration = quspinCalibration; // analog output calibration is 2.7 V/nT
            magInputs.Channels.Add(hqy);

            ScannedAnalogInput hpy = new ScannedAnalogInput();
            hpy.Channel = (AnalogInputChannel)Environs.Hardware.AnalogInputChannels["quSpinHP_Y"];
            hpy.ReductionMode = quspinReductionMode;
            hpy.ChopStart = quspinChopStart;
            hpy.ChopLength = quspinChopLength;
            hpy.AverageEvery = quspinAverageEvery;
            hpy.LowLimit = quspinLowerLim;
            hpy.HighLimit = quspinUpperLim;
            hpy.Calibration = quspinCalibration; // analog output calibration is 2.7 V/nT
            magInputs.Channels.Add(hpy);

            //ScannedAnalogInput hoy = new ScannedAnalogInput();
            //hoy.Channel = (AnalogInputChannel)Environs.Hardware.AnalogInputChannels["quSpinHO_Y"];
            //hoy.ReductionMode = quspinReductionMode;
            //hoy.ChopStart = quspinChopStart;
            //hoy.ChopLength = quspinChopLength;
            //hoy.AverageEvery = quspinAverageEvery;
            //hoy.LowLimit = quspinLowerLim;
            //hoy.HighLimit = quspinUpperLim;
            //hoy.Calibration = quspinCalibration; // analog output calibration is 2.7 V/nT
            //magInputs.Channels.Add(hoy);

            ScannedAnalogInput hmy = new ScannedAnalogInput();
            hmy.Channel = (AnalogInputChannel)Environs.Hardware.AnalogInputChannels["quSpinHM_Y"];
            hmy.ReductionMode = quspinReductionMode;
            hmy.ChopStart = quspinChopStart;
            hmy.ChopLength = quspinChopLength;
            hmy.AverageEvery = quspinAverageEvery;
            hmy.LowLimit = quspinLowerLim;
            hmy.HighLimit = quspinUpperLim;
            hmy.Calibration = quspinCalibration; // analog output calibration is 2.7 V/nT
            magInputs.Channels.Add(hmy);

            ScannedAnalogInput fvz = new ScannedAnalogInput();
            fvz.Channel = (AnalogInputChannel)Environs.Hardware.AnalogInputChannels["quSpinFV_Z"];
            fvz.ReductionMode = quspinReductionMode;
            fvz.ChopStart = quspinChopStart;
            fvz.ChopLength = quspinChopLength;
            fvz.AverageEvery = quspinAverageEvery;
            fvz.LowLimit = quspinLowerLim;
            fvz.HighLimit = quspinUpperLim;
            fvz.Calibration = quspinCalibration; // analog output calibration is 2.7 V/nT
            magInputs.Channels.Add(fvz);

            ScannedAnalogInput htz = new ScannedAnalogInput();
            htz.Channel = (AnalogInputChannel)Environs.Hardware.AnalogInputChannels["quSpinHT_Z"];
            htz.ReductionMode = quspinReductionMode;
            htz.ChopStart = quspinChopStart;
            htz.ChopLength = quspinChopLength;
            htz.AverageEvery = quspinAverageEvery;
            htz.LowLimit = quspinLowerLim;
            htz.HighLimit = quspinUpperLim;
            htz.Calibration = quspinCalibration; // analog output calibration is 2.7 V/nT
            magInputs.Channels.Add(htz);

            ScannedAnalogInput hsz = new ScannedAnalogInput();
            hsz.Channel = (AnalogInputChannel)Environs.Hardware.AnalogInputChannels["quSpinHS_Z"];
            hsz.ReductionMode = quspinReductionMode;
            hsz.ChopStart = quspinChopStart;
            hsz.ChopLength = quspinChopLength;
            hsz.AverageEvery = quspinAverageEvery;
            hsz.LowLimit = quspinLowerLim;
            hsz.HighLimit = quspinUpperLim;
            hsz.Calibration = quspinCalibration; // analog output calibration is 2.7 V/nT
            magInputs.Channels.Add(hsz);

            ScannedAnalogInput hrz = new ScannedAnalogInput();
            hrz.Channel = (AnalogInputChannel)Environs.Hardware.AnalogInputChannels["quSpinHR_Z"];
            hrz.ReductionMode = quspinReductionMode;
            hrz.ChopStart = quspinChopStart;
            hrz.ChopLength = quspinChopLength;
            hrz.AverageEvery = quspinAverageEvery;
            hrz.LowLimit = quspinLowerLim;
            hrz.HighLimit = quspinUpperLim;
            hrz.Calibration = quspinCalibration; // analog output calibration is 2.7 V/nT
            magInputs.Channels.Add(hrz);

            ScannedAnalogInput hqz = new ScannedAnalogInput();
            hqz.Channel = (AnalogInputChannel)Environs.Hardware.AnalogInputChannels["quSpinHQ_Z"];
            hqz.ReductionMode = quspinReductionMode;
            hqz.ChopStart = quspinChopStart;
            hqz.ChopLength = quspinChopLength;
            hqz.AverageEvery = quspinAverageEvery;
            hqz.LowLimit = quspinLowerLim;
            hqz.HighLimit = quspinUpperLim;
            hqz.Calibration = quspinCalibration; // analog output calibration is 2.7 V/nT
            magInputs.Channels.Add(hqz);

            ScannedAnalogInput hpz = new ScannedAnalogInput();
            hpz.Channel = (AnalogInputChannel)Environs.Hardware.AnalogInputChannels["quSpinHP_Z"];
            hpz.ReductionMode = quspinReductionMode;
            hpz.ChopStart = quspinChopStart;
            hpz.ChopLength = quspinChopLength;
            hpz.AverageEvery = quspinAverageEvery;
            hpz.LowLimit = quspinLowerLim;
            hpz.HighLimit = quspinUpperLim;
            hpz.Calibration = quspinCalibration; // analog output calibration is 2.7 V/nT
            magInputs.Channels.Add(hpz);

            ScannedAnalogInput hoz = new ScannedAnalogInput();
            hoz.Channel = (AnalogInputChannel)Environs.Hardware.AnalogInputChannels["quSpinHO_Z"];
            hoz.ReductionMode = quspinReductionMode;
            hoz.ChopStart = quspinChopStart;
            hoz.ChopLength = quspinChopLength;
            hoz.AverageEvery = quspinAverageEvery;
            hoz.LowLimit = quspinLowerLim;
            hoz.HighLimit = quspinUpperLim;
            hoz.Calibration = quspinCalibration; // analog output calibration is 2.7 V/nT
            magInputs.Channels.Add(hoz);

            ScannedAnalogInput hmz = new ScannedAnalogInput();
            hmz.Channel = (AnalogInputChannel)Environs.Hardware.AnalogInputChannels["quSpinHM_Z"];
            hmz.ReductionMode = quspinReductionMode;
            hmz.ChopStart = quspinChopStart;
            hmz.ChopLength = quspinChopLength;
            hmz.AverageEvery = quspinAverageEvery;
            hmz.LowLimit = quspinLowerLim;
            hmz.HighLimit = quspinUpperLim;
            hmz.Calibration = quspinCalibration; // analog output calibration is 2.7 V/nT
            magInputs.Channels.Add(hmz);

            //ScannedAnalogInput battery = new ScannedAnalogInput();
            //battery.Channel = (AnalogInputChannel)Environs.Hardware.AnalogInputChannels["battery"];
            //battery.ReductionMode = DataReductionMode.Average;
            //battery.AverageEvery = 20;
            //battery.LowLimit = -10;
            //battery.HighLimit = 10;
            //battery.Calibration = quspinCalibration; // analog output calibration is 2.7 V/nT
            //magInputs.Channels.Add(battery);

            //ScannedAnalogInput rfCurrent = new ScannedAnalogInput();
            //rfCurrent.Channel = (AnalogInputChannel)Environs.Hardware.AnalogInputChannels["rfCurrent"];
            //rfCurrent.ReductionMode = DataReductionMode.Average;
            //rfCurrent.AverageEvery = 10; //Bandwidth of the ammeter is aprox 12kHz
            //rfCurrent.LowLimit = -10;
            //rfCurrent.HighLimit = 10;
            //magInputs.Channels.Add(rfCurrent);

            //ScannedAnalogInput reflectedrfAmplitude = new ScannedAnalogInput();
            //reflectedrfAmplitude.Channel = (AnalogInputChannel)Environs.Hardware.AnalogInputChannels["reflectedrfAmplitude"];
            //reflectedrfAmplitude.ReductionMode = DataReductionMode.Chop;
            //reflectedrfAmplitude.ChopStart = 0;
            //reflectedrfAmplitude.ChopLength = 150;
            //reflectedrfAmplitude.LowLimit = -10;
            //reflectedrfAmplitude.HighLimit = 10;
            //magInputs.Channels.Add(reflectedrfAmplitude);

            //ScannedAnalogInput incidentrfAmplitude = new ScannedAnalogInput();
            //incidentrfAmplitude.Channel = (AnalogInputChannel)Environs.Hardware.AnalogInputChannels["incidentrfAmplitude"];
            //incidentrfAmplitude.ReductionMode = DataReductionMode.Chop;
            //incidentrfAmplitude.ChopStart = 0;
            //incidentrfAmplitude.ChopLength = 150;
            //incidentrfAmplitude.LowLimit = -10;
            //incidentrfAmplitude.HighLimit = 10;
            //magInputs.Channels.Add(incidentrfAmplitude);

        }
        #endregion

        private void ConfigureSinglePointAnalogInputs()
        {
            // here we configure the scan of analog inputs that happens after each shot.
            singlePointInputTask = new Task("Blockhead single point inputs");
            //AddChannelToSinglePointTask("topPD");
            //AddChannelToSinglePointTask("ground");
            //AddChannelToSinglePointTask("bottomPD");
            //AddChannelToSinglePointTask("ground");
            AddChannelToSinglePointTask("miniFlux1");
            //AddChannelToSinglePointTask("bartington_relays");
            //AddChannelToSinglePointTask("ground");
            //AddChannelToSinglePointTask("miniFlux2");
            //AddChannelToSinglePointTask("ground");
            //AddChannelToSinglePointTask("miniFlux3");
            //AddChannelToSinglePointTask("ground");
            //AddChannelToSinglePointTask("cPlusMonitor");
            //AddChannelToSinglePointTask("ground");
            //AddChannelToSinglePointTask("cMinusMonitor");
            //AddChannelToSinglePointTask("northLeakage");
            //AddChannelToSinglePointTask("ground");
            //AddChannelToSinglePointTask("southLeakage");
            //AddChannelToSinglePointTask("ground");
            //AddChannelToSinglePointTask("valveMonVoltage");

            //singlePointInputTask.Timing.ConfigureSampleClock(
            //        "",
            //        1000,
            //        SampleClockActiveEdge.Rising,
            //        SampleQuantityMode.FiniteSamples,
            //        1
            //     );

            //singlePointInputTask.Triggers.StartTrigger.ConfigureNone();

            if (!Environs.Debug) singlePointInputTask.Control(TaskAction.Verify);
            singlePointInputReader = new AnalogMultiChannelReader(singlePointInputTask.Stream);

        }

        private void ConfigureSinglePointAnalogInputsForMagBlocks()
        {
            // here we configure the scan of analog inputs that happens after each shot.
            singlePointInputTask = new Task("Blockhead single point inputs for magnetic field blocks");
            AddChannelToSinglePointTask("miniFlux1");
            //AddChannelToSinglePointTask("bartington_relays");
            //AddChannelToSinglePointTask("battery");
            //AddChannelToSinglePointTask("miniFlux2");
            //AddChannelToSinglePointTask("miniFlux3");

            if (!Environs.Debug) singlePointInputTask.Control(TaskAction.Verify);
            singlePointInputReader = new AnalogMultiChannelReader(singlePointInputTask.Stream);

        }

        private void AddChannelToSinglePointTask(string chan)
        {
            AnalogInputChannel aic = ((AnalogInputChannel)Environs.Hardware.AnalogInputChannels[chan]);
            aic.AddToTask(singlePointInputTask, 0, 10);
        }

		// configure hardware and start the pattern output
		private void AcquisitionStarting()
		{
			// iterate through the channels and ready them
			foreach (SwitchedChannel s in switchedChannels) s.AcquisitionStarting();

			// copy running parameters into the BlockConfig
			StuffConfig();

            // prepare the inputs
            inputTask = new Task("BlockHead analog input");

            foreach (ScannedAnalogInput i in inputs.Channels)
                i.Channel.AddToTask(
                    inputTask,
                    i.LowLimit,
                    i.HighLimit
                    );

            inputTask.Timing.ConfigureSampleClock(
                "",
                inputs.RawSampleRate,
                SampleClockActiveEdge.Rising,
                SampleQuantityMode.FiniteSamples,
                inputs.GateLength * inputs.Channels.Count
                );

            inputTask.Triggers.StartTrigger.ConfigureDigitalEdgeTrigger(
                (string)Environs.Hardware.GetInfo("analogTrigger0"),
                DigitalEdgeStartTriggerEdge.Rising
                );

            if (!Environs.Debug) inputTask.Control(TaskAction.Verify);
            inputReader = new AnalogMultiChannelReader(inputTask.Stream);

            ConfigureSinglePointAnalogInputs();

            // set the leakage monitor measurement time to 5ms.
            // With this setting it actually takes 26ms total to acquire two channels.
            //hardwareController.LeakageMonitorMeasurementTime = 0.005;
            //hardwareController.ReconfigureIMonitors();
            // Start the first asynchronous acquisition
            //hardwareController.UpdateIMonitorAsync();
		}

        // configure hardware for magnetic field data taking
        private void MagAcquisitionStarting()
        {
            // iterate through the channels and ready them
            foreach (SwitchedChannel s in switchedChannels) s.AcquisitionStarting();

            // copy running parameters into the BlockConfig
            StuffConfig();

            // prepare the inputs
            inputTask = new Task("BlockHead magnetometer analog input");

            foreach (ScannedAnalogInput i in magInputs.Channels)
                i.Channel.AddToTask(
                    inputTask,
                    i.LowLimit,
                    i.HighLimit
                    );

            inputTask.Timing.ConfigureSampleClock(
                "",
                magInputs.RawSampleRate,
                SampleClockActiveEdge.Rising,
                SampleQuantityMode.FiniteSamples,
                magInputs.GateLength * magInputs.Channels.Count
                );

            inputTask.Triggers.StartTrigger.ConfigureDigitalEdgeTrigger(
                (string)Environs.Hardware.GetInfo("analogTrigger0"),
                DigitalEdgeStartTriggerEdge.Rising
                );

            if (!Environs.Debug) inputTask.Control(TaskAction.Verify);
            inputReader = new AnalogMultiChannelReader(inputTask.Stream);

            ConfigureSinglePointAnalogInputsForMagBlocks();

            // set the leakage monitor measurement time to 5ms.
            // With this setting it actually takes 26ms total to acquire two channels.
            //hardwareController.LeakageMonitorMeasurementTime = 0.005;
            hardwareController.ReconfigureIMonitors();
            // Start the first asynchronous acquisition
            //hardwareController.UpdateIMonitorAsync();
        }

		// If you want to store any information in the BlockConfig this is the place to do it.
		// This function is called at the start of every block.
		private void StuffConfig()
		{
			// dump all of the pg settings into the config - never know when they'll come in
			// handy !
			ScanMaster.Acquire.Plugin.PluginSettings pgSettings = scanMaster.GetPGSettings();
			foreach (String key in pgSettings.Keys) config.Settings[key] = pgSettings[key];

			// rf parameters
			//EDMHardwareControl.Controller hwController = new EDMHardwareControl.Controller();     //new hardware controller
            UEDMHardwareControl.UEDMController hwController = new UEDMHardwareControl.UEDMController();
            config.Settings["greenSynthFrequency"] = hwController.GreenSynthOnFrequency;
			//config.Settings["greenSynthDCFM"] = hwController.GreenSynthDCFM;
			config.Settings["greenSynthAmplitude"] = hwController.GreenSynthOnAmplitude;               //new hardware controller
		}

		private void ThrowSwitches(int point)
		{
			// this works out how long to wait before the switch - it looks at
			// all of the switched channels and figures out which ones are
			// switching. Of those channels it finds the longest wait.
			int delayBeforeSwitch = 0;
			foreach(SwitchedChannel s in switchedChannels)
			{
				int tempDelay = s.Modulation.DelayBeforeSwitch;
				// special case for the first point of a block - treat all channels as if they
				// are switching
				if (point != 0)
				{
					bool[] bits = s.Modulation.Waveform.Bits;
					// if this waveform is not switching just ignore its delay
					if ( bits[point] == bits[point - 1]) tempDelay = 0;
				}
				if (tempDelay > delayBeforeSwitch) delayBeforeSwitch = tempDelay;
			}
			// impose the delay
			if (delayBeforeSwitch != 0) Thread.Sleep(delayBeforeSwitch);

			// now actually throw the switches
			foreach(SwitchedChannel s in switchedChannels)
			{
				bool[] bits = s.Modulation.Waveform.Bits;
                if (point != 0)
                {
                    if (bits[point] != bits[point - 1]) s.State = bits[point];
                }
                else
                {
                    s.State = bits[point];
                }
			}

			// calculate and impose the post switching delays
			int delayAfterSwitch = 0;
			foreach(SwitchedChannel s in switchedChannels)
			{
				int tempDelay = s.Modulation.DelayAfterSwitch;
				// special case for the first point of a block - treat all channels as if they
				// are switching
				if (point != 0)
				{
					bool[] bits = s.Modulation.Waveform.Bits;
					// if this waveform is not switching just ignore its delay
					if ( bits[point] == bits[point - 1]) tempDelay = 0;
				}
				if (tempDelay > delayAfterSwitch) delayAfterSwitch = tempDelay;
			}
			// impose the delay
			if (delayAfterSwitch != 0) Thread.Sleep(delayAfterSwitch);			
		}

		// stop pattern output and release hardware
		private void AcquisitionStopping()
		{
			foreach( SwitchedChannel s in switchedChannels) s.AcquisitionFinishing();
			inputTask.Dispose();
            singlePointInputTask.Dispose();
		}

		private bool CheckIfStopping() 
		{
			lock(this) 
			{
				if (backendState == AcquisitorState.stopping)
				{
					backendState = AcquisitorState.stopped;
					return true;
				}
				else return false;
			}
		}

	}
}
