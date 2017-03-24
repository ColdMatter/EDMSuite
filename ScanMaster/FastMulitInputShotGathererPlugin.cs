using System;
using System.Threading;
using System.Xml.Serialization;

using NationalInstruments.DAQmx;

using DAQ.Environment;
using DAQ.FakeData;
using DAQ.HAL;
using Data;
using ScanMaster.Acquire.Plugin;

namespace ScanMaster.Acquire.Plugins
{
	/// <summary>
	/// A plugin to capture time-resolved data by buffered event counting and simultaneous analog inputs.
    /// For counting:
	///  - Each element of the buffer will contain the number of edges present at the source pin of the counter
	/// in the time interval between two successive edges at the gate pin of the same counter. The counts are 
	/// gated into the buffer by a sample clock. The number of samples collected is set by the gateLength setting.
	///  - The sample clock is also set up in this class. The sample clock will appear on the out pin of a
	///  second counter. This clock signal should be routed to the gate pin of the first counter. The frequency
	///  of this sample clock is determined by the sampleRate setting.
	///  - This plugin can be used either in triggered or un-triggered mode. In untriggered mode, the 
	///  data collection is not synchronized to anything. In triggered mode, data collection is synchronized
	///  to a trigger which should appear on the gate pin of the sample clock.
	///  - The count data is converted to a frequency (kHz).
	///  
	///  Signal Connections
	///  -----------------
	///  Route the signal to be counted to the source pin of a counter (I'll call it counter A)
	///  Route the output of the second counter (counter B, the sample clock) to the gate pin of counter A
	///  To trigger the data acquisition, route a trigger signal to the analog input standard trigger (PFI0 - for compatibility with analog shot gathering)
    ///  To take modulated (on/off) data, you should have two separate triggers from the PG - one for the off shots and the other for the
    ///  on shots. Route the second trigger to PFI1.
	///  Beware of cross-talk between the source and gate pins - this will upset everything!
    ///  
    /// This fast version starts the task as the scan starts and then only reads at every shot point this 
    /// is significantly faster. This is at the expense of NOT being able to be used in conjunction with the 
    /// switch plugins.
	/// </summary>
	[Serializable]
	public class FastMultiInputShotGathererPlugin : ShotGathererPlugin
	{
		[NonSerialized]
		private Task countingTask;
        [NonSerialized]
        private DaqStream countStream;
		[NonSerialized]
		private Task freqOutTask1;
        [NonSerialized]
        private Task freqOutTask2;
		[NonSerialized]
        private Task shotGateTask;
        [NonSerialized]
        private Task flagTask;
        [NonSerialized]
		private CounterReader countReader;
        [NonSerialized]
        private Task inputTask;
        [NonSerialized]
        private AnalogMultiChannelReader reader;
        [NonSerialized]
        private DigitalSingleChannelWriter flagWriter;
        [NonSerialized]
		private double[] latestData;
        [NonSerialized]
        private double[,] latestAnalogData;
		
		protected override void InitialiseSettings()
		{
			settings["triggerActive"] = true;
            //settings["pointsPerScan"] = 1000;
            settings["analogChannel"] = "uCavityReflectionECDL,uCavityReflectionTiSapph";
		}

		public override void AcquisitionStarting()
		{
            //set up a shot gate pulse
            shotGateTask = new Task("gate for pause trigger for counterTask");

            shotGateTask.COChannels.CreatePulseChannelTime(
                ((CounterChannel)Environs.Hardware.CounterChannels["shot gate"]).PhysicalChannel,
                "photon counter gate signal", COPulseTimeUnits.Seconds,
                COPulseIdleState.Low,
                2.0 / 100000000, 0.01,//the shot gate need 10ms after 
                //gate is done to let daq card transfer the 
                //acquired data
                Convert.ToDouble((int)settings["gateLength"] + 1) / Convert.ToDouble((int)settings["sampleRate"]));

            shotGateTask.Timing.ConfigureImplicit(SampleQuantityMode.FiniteSamples, 1);

            if ((bool)settings["triggerActive"])
            {
                shotGateTask.Triggers.StartTrigger.ConfigureDigitalEdgeTrigger(
                (string)Environs.Hardware.GetInfo("analogTrigger0"),
                DigitalEdgeStartTriggerEdge.Rising);
                shotGateTask.Triggers.StartTrigger.Retriggerable = true;
            }
            shotGateTask.Control(TaskAction.Verify);

            //***

            // set up two tasks to generate the sample clock on the second counter
            freqOutTask1 = new Task("buffered event counter clock generation 1");
            freqOutTask2 = new Task("buffered event counter clock generation 2");
            
            //the frequency of the clock is set by the "sampleRate" setting and the duty cycle is set to 0.5
            //the two output tasks have the same settings
            freqOutTask1.COChannels.CreatePulseChannelFrequency(
                ((CounterChannel)Environs.Hardware.CounterChannels["sample clock"]).PhysicalChannel,
                "photon counter clocking signal",
                COPulseFrequencyUnits.Hertz,
                COPulseIdleState.Low,
                0,
                (int)settings["sampleRate"],
                0.5);
            freqOutTask1.Timing.ConfigureImplicit(SampleQuantityMode.FiniteSamples, (int)settings["gateLength"] + 1);

            freqOutTask2.COChannels.CreatePulseChannelFrequency(
                ((CounterChannel)Environs.Hardware.CounterChannels["sample clock"]).PhysicalChannel,
                "photon counter clocking signal",
                COPulseFrequencyUnits.Hertz,
                COPulseIdleState.Low,
                0,
                (int)settings["sampleRate"],
                0.5);
            freqOutTask2.Timing.ConfigureImplicit(SampleQuantityMode.FiniteSamples, (int)settings["gateLength"] + 1);
            
            // if we're using a hardware trigger to synchronize data acquisition, we need to set up the 
            // trigger parameters on the sample clock.
            // The first output task is triggered on PFI0 and the second is triggered on PFI1 by default
            //both clocks are triggered off of the shotGate
            if ((bool)settings["triggerActive"])
            {
                freqOutTask1.Triggers.StartTrigger.Type = StartTriggerType.DigitalEdge;
                freqOutTask1.Triggers.StartTrigger.DigitalEdge.Edge = DigitalEdgeStartTriggerEdge.Rising;
                freqOutTask2.Triggers.StartTrigger.Type = StartTriggerType.DigitalEdge;
                freqOutTask2.Triggers.StartTrigger.DigitalEdge.Edge = DigitalEdgeStartTriggerEdge.Rising;
                //the trigger is expected to appear on PFI0
                //freqOutTask1.Triggers.StartTrigger.DigitalEdge.Source = (string)Environs.Hardware.Boards["daq"] + "/PFI0";
                freqOutTask1.Triggers.StartTrigger.DigitalEdge.Source = (string)Environs.Hardware.GetInfo("shotTrigger0");
                // the trigger is expected to appear on PFI1
                //freqOutTask2.Triggers.StartTrigger.DigitalEdge.Source = (string)Environs.Hardware.Boards["daq"] + "/PFI1";
                freqOutTask2.Triggers.StartTrigger.DigitalEdge.Source = (string)Environs.Hardware.GetInfo("shotTrigger0");
                freqOutTask1.Triggers.StartTrigger.Retriggerable = true;
                freqOutTask2.Triggers.StartTrigger.Retriggerable = true;
            }
            
            //***

            //set up an edge-counting task
			countingTask = new Task("buffered edge counter gatherer " + (string)settings["channel"]);

			//count upwards on rising edges starting from zero
			countingTask.CIChannels.CreateCountEdgesChannel(
				((CounterChannel)Environs.Hardware.CounterChannels[(string)settings["channel"]]).PhysicalChannel,
				"edge counter", 
				CICountEdgesActiveEdge.Rising, 
				0, 
				CICountEdgesCountDirection.Up);

			//The counting buffer is triggered by a the gate and clocked off of the sample clock
            // which will be routed to the gate pin of ctr0 (PFI9).  The number of samples 
            //to collect is determined by the "gateLength" setting. We add 1 to this,
			// since the first count is not synchronized to anything and will be discarded
			countingTask.Timing.ConfigureSampleClock(
				(string)Environs.Hardware.GetInfo("sample clock reader"), 
				(int)settings["sampleRate"], 
				SampleClockActiveEdge.Rising,
				//SampleQuantityMode.FiniteSamples,
                SampleQuantityMode.ContinuousSamples,
                ((int)settings["gateLength"] + 1));
            //countingTask.Triggers.StartTrigger.Retriggerable = true;
            //countingTask.Triggers.PauseTrigger.DigitalLevel = 

            if ((bool)settings["triggerActive"])
            {
                countingTask.Triggers.PauseTrigger.ConfigureDigitalLevelTrigger(
                (string)Environs.Hardware.GetInfo("shotTrigger0"),
                DigitalLevelPauseTriggerCondition.Low);
            }

			countingTask.Control(TaskAction.Verify);

			// set up a reader for the edge counter
			countReader = new CounterReader(countingTask.Stream);

            //***
            
            // configure the analog input
            inputTask = new Task("analog gatherer " /*+ (string)settings["channel"]*/);

            string channelList = (string)settings["analogChannel"];
            string[] channels = channelList.Split(new char[] { ',' });

            foreach (string channel in channels)
                ((AnalogInputChannel)Environs.Hardware.AnalogInputChannels[channel]).AddToTask(
                    inputTask,
                    (double)settings["inputRangeLow"],
                    (double)settings["inputRangeHigh"]
                    );

            // internal clock, finite acquisition
            inputTask.Timing.ConfigureSampleClock(
                //"",
                (string)Environs.Hardware.GetInfo("sample clock reader"),
                (int)settings["sampleRate"],
                SampleClockActiveEdge.Rising,
                SampleQuantityMode.FiniteSamples,
                //SampleQuantityMode.ContinuousSamples,
                (int)settings["gateLength"] + 1);

            if ((bool)settings["triggerActive"])
            {
                inputTask.Triggers.StartTrigger.ConfigureDigitalEdgeTrigger(
                (string)Environs.Hardware.GetInfo("shotTrigger0"),
                DigitalEdgeStartTriggerEdge.Rising);
                inputTask.Triggers.StartTrigger.Retriggerable = true;
            }

            inputTask.Control(TaskAction.Verify);
            //			}
            reader = new AnalogMultiChannelReader(inputTask.Stream);
		}

		public override void ScanStarting()
		{
		}

		public override void ScanFinished()
		{
		}

		public override void AcquisitionFinished()
		{
			countingTask.Dispose();

            // release the analog input
            inputTask.Dispose();

            if (config.switchPlugin.State)
            {
                freqOutTask1.Stop();
            }
            else
            {
                freqOutTask2.Stop();
            }
			freqOutTask1.Dispose();
            freqOutTask2.Dispose();

            //release shot gate 
            shotGateTask.Dispose();
		}

        public override void PreArm() //The Arm and Wait is divided into PreArm, ArmAndWait
            //and PostArm this means that only the acquisiton is inside the shot loop.
        {
            lock (this)
			{
                //Setup Analog aquisition
                inputTask.Start();

                //Get the counter ready. Nothing will happen until there is output on the sample clock
                countingTask.Start();

                // Get the sample clock ready. If the trigger is inactive, the clock will start its output immediately.
                // If the trigger is active, there is no output until the counter is triggered
                // Which output task runs depends on the switch-state.           
                if (config.switchPlugin.State)
                {
                    freqOutTask1.Start();
                }
                else
                {
                    freqOutTask2.Start();
                }

                //prepare shot gate 
                shotGateTask.Start();
            }
        }

		public override void ArmAndWait()
		{
            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start(); 
            
            lock(this)
            {
                // read the data into a temporary array once all the samples have been acquired
                double[] tempdata = countReader.ReadMultiSampleDouble((int)settings["gateLength"] + 1);
                
                //Read in the analog points
                latestAnalogData = reader.ReadMultiSample((int)settings["gateLength"] + 1);

                string shotreader = stopwatch.Elapsed.ToString(@"mm\:ss\.ffffff");
                //stopwatch.Reset();
                //stopwatch.Start();

				// Each element of the buffer holds the incremental count. Discard the first value and calculate
				// the counts per bin by subtracting the count in each bin by the count in the previous bin.
				// Calculate the rate in kHz.

                latestData = new double[tempdata.Length - 1];
                for (int k = 0; k < latestData.Length; k++)
                    latestData[k] = (tempdata[k + 1] - tempdata[k]) * (int)settings["sampleRate"] / 1000;

                //string datamanip = stopwatch.Elapsed.ToString(@"mm\:ss\.ffffff");                
			}
		}

        public override void PostArm()
        {
            lock(this)
            {

                //stop shot gate
                shotGateTask.Stop();

                // stop the sample clock
                if (config.switchPlugin.State)
                {
                    freqOutTask1.Stop();
                }
                else
                {
                    freqOutTask2.Stop();
                }
                
                // stop the counter; the job's done
                countingTask.Stop();

                //stop the analog task
                inputTask.Stop();

            }
        }

		public override Shot Shot
		{
			get 
			{
                lock(this)
				{
					Shot s = new Shot();
					TOF t = new TOF();
					t.ClockPeriod = (int)settings["clockPeriod"];
                    t.GateStartTime = (int)settings["gateStartTime"];
					if (!Environs.Debug)
					{
						t.Data = latestData;
						s.TOFs.Add(t);
                        for (int i = 0; i < inputTask.AIChannels.Count; i++)
                        {
                            TOF tt = new TOF();
                            tt.ClockPeriod = (int)settings["clockPeriod"];
                            tt.GateStartTime = (int)settings["gateStartTime"];
                            tt.Calibration = ((AnalogInputChannel)Environs.Hardware.AnalogInputChannels[inputTask.AIChannels[i].VirtualName]).Calibration;
                            double[] tmp = new double[(int)settings["gateLength"]];
                            for (int j = 0; j < (int)settings["gateLength"]; j++)
                                tmp[j] = latestAnalogData[i, j];
                            tt.Data = tmp;
                            s.TOFs.Add(tt);
                        }
						return s;
					}
					else 
					{
						Thread.Sleep(50);
						return DataFaker.GetFakeShot((int)settings["gateStartTime"], (int)settings["gateLength"],
							(int)settings["clockPeriod"], 1, 1);
					}
				} 
			}
		}
	}
}
