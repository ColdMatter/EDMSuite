using System;
using System.Threading;
using System.Windows.Forms;

using DAQ.Environment;
using Data;
using Data.Scans;
using ScanMaster.Acquire.Plugin;

namespace ScanMaster.Acquire

{
	public delegate void DataEventHandler(object sender, DataEventArgs e);
	public delegate void ScanFinishedEventHandler(object sender, EventArgs e);

	/// <summary>
	/// This is a brave attempt at making a generic backend component. The idea is that
	/// it only knows how to generically scan and switch. Plugin classes provide all the
	/// specific functionality, like scanning and analog output, gathering shot data
	/// from the board, generating patterns, controlling the laser. The aim is that me
	/// and Mike can use exactly the same code, with our own custom plugins. I'm not sure
	/// whether it will work or not.
	/// </summary>
	public class Acquisitor
	{
		public event DataEventHandler Data;
		public event ScanFinishedEventHandler ScanFinished;
		public object AcquisitorMonitorLock = new Object();

		private AcquisitorConfiguration config;
		public AcquisitorConfiguration Configuration
		{
			set { config = value; }
		}

		private Thread acquireThread;
		private int numberOfScans = 0;
		
		enum AcquisitorState {stopped, running, stopping};
		private AcquisitorState backendState = AcquisitorState.stopped;

		public void AcquireStart(int numberOfScans) 
		{
			this.numberOfScans = numberOfScans;
			acquireThread = new Thread(new ThreadStart(this.Acquire));
			acquireThread.Name = "ScanMaster Acquisitor";
			acquireThread.Priority = ThreadPriority.Normal;
			backendState = AcquisitorState.running;
			acquireThread.Start();
		}

		public void AcquireStop() 
		{
			lock(this)
			{
				backendState = AcquisitorState.stopping;
			}
		}


		private void Acquire() 
		{
            try 
            {
				// lock a monitor onto the acquisitor, to synchronise with the controller
				// when acquiring a set number of scans - the monitor is released in
				// AcquisitionFinishing()
				Monitor.Enter(AcquisitorMonitorLock);

				// initialise all of the plugins
				config.outputPlugin.AcquisitionStarting();
				config.pgPlugin.AcquisitionStarting();
				config.shotGathererPlugin.AcquisitionStarting();
				config.switchPlugin.AcquisitionStarting();
				config.yagPlugin.AcquisitionStarting();
				config.analogPlugin.AcquisitionStarting();

				for (int scanNumber = 0 ;; scanNumber++)
				{
					double scanParameter;

					// prepare for the scan start
					config.outputPlugin.ScanStarting();
					config.pgPlugin.ScanStarting();
					config.shotGathererPlugin.ScanStarting();
					config.switchPlugin.ScanStarting();
					config.yagPlugin.ScanStarting();
					config.analogPlugin.ScanStarting();
					for (int pointNumber = 0 ; pointNumber < (int)config.outputPlugin.Settings["pointsPerScan"] ; pointNumber++)
					{
						// calculate the new scan parameter
						PluginSettings outputSettings = config.outputPlugin.Settings;
						scanParameter = (double)outputSettings["start"] +
							((double)outputSettings["end"] - (double)outputSettings["start"]) * pointNumber
							/ (int)outputSettings["pointsPerScan"];

						// move the scan along
						config.outputPlugin.ScanParameter = scanParameter;

						// check for a change in the pg parameters
						lock(this)
						{
							if (tweakFlag)
							{
								// now it's safe to update the pattern generator settings
								// and ask the pg to reload
								SettingsReflector sr = new SettingsReflector();
								sr.SetField(config.pgPlugin,
									latestTweak.parameter, latestTweak.newValue.ToString());
								config.pgPlugin.ReloadPattern();
								tweakFlag = false;
							}
						}

                        ScanPoint sp = new ScanPoint();
						sp.ScanParameter = config.outputPlugin.ScanParameter;

                        for (int shotNum = 0; shotNum < (int)(config.outputPlugin.Settings["shotsPerPoint"]); shotNum++)
                        {
                            // Set the switch state
                            config.switchPlugin.State = true;

                            // wait for the data gatherer to finish
                            config.shotGathererPlugin.ArmAndWait();

                            // read out the data

                            sp.OnShots.Add(config.shotGathererPlugin.Shot);

                            if ((bool)config.switchPlugin.Settings["switchActive"])
                            {
                                config.switchPlugin.State = false;
                                config.shotGathererPlugin.ArmAndWait();
                                sp.OffShots.Add(config.shotGathererPlugin.Shot);
                            }
                        }

						// sample the analog channels and add them to the ScanPoint
						config.analogPlugin.ArmAndWait();
						sp.Analogs.AddRange(config.analogPlugin.Analogs);

						// send up the data bundle
						DataEventArgs evArgs = new DataEventArgs();
						evArgs.point = sp;
						OnData(evArgs);

						// check for exit
						if (CheckIfStopping()) 
						{
							AcquisitionFinishing(config);
							return;
						}
 					}
					// prepare for the start of the next scan
					OnScanFinished();
					config.pgPlugin.ScanFinished();
					config.yagPlugin.ScanFinished();
					config.outputPlugin.ScanFinished();
					config.shotGathererPlugin.ScanFinished();
					config.switchPlugin.ScanFinished();
					config.analogPlugin.ScanFinished();
                    // I think that this pause will workaround an annoying threading bug
                    // I should probably be less cheezy and put a lock in, but I'm not really
                    // sure that I know what the bug is as it's intermittent (and rare).
                    Thread.Sleep(750);

					// check if we are finished scanning
					if (scanNumber + 1 == numberOfScans)
					{
						backendState = AcquisitorState.stopped;
						// set the controller state to stopped
						Controller.GetController().appState = Controller.AppState.stopped;
						AcquisitionFinishing(config);
						return;
					}
				}

            }
            catch (Exception e)
            {
                // last chance exception handler - this stops a rogue exception in the
                // acquire loop from killing the whole program
                Console.Error.Write(e.Message + e.StackTrace);
                MessageBox.Show("Exception caught in acquire loop.\nTake care - the program " +
                    "is probably unstable.\n" + e.Message + "\n" + e.StackTrace, "Acquire error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                // Try and stop the pattern gracefully before the program dies
                config.pgPlugin.AcquisitionFinished();
                lock (this) backendState = AcquisitorState.stopped;
            }
        }

		private void AcquisitionFinishing(AcquisitorConfiguration config)
		{
			config.pgPlugin.AcquisitionFinished();
			config.shotGathererPlugin.AcquisitionFinished();
			config.switchPlugin.AcquisitionFinished();
			config.yagPlugin.AcquisitionFinished();
			config.analogPlugin.AcquisitionFinished();
			config.outputPlugin.AcquisitionFinished();
			Monitor.Pulse(AcquisitorMonitorLock);
			Monitor.Exit(AcquisitorMonitorLock);
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

		bool tweakFlag = false;
		TweakEventArgs latestTweak;
		public void HandleTweak(object sender, TweakEventArgs e)
		{
			lock(this)
			{
				latestTweak = e;
				tweakFlag = true;
			}
		}


		protected virtual void OnData( DataEventArgs e ) 
		{
			if (Data != null) Data(this, e);
		}
		protected virtual void OnScanFinished() 
		{
			if (ScanFinished != null) ScanFinished(this, new EventArgs());
		}

	}

	/// <summary>
	/// Instances of this class are used to send data to interested listeners.
	/// </summary>
	public class DataEventArgs : EventArgs
	{
		public ScanPoint point;
	}
}
