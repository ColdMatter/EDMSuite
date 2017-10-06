using System;
using System.Threading;
using System.Xml.Serialization;

using NationalInstruments.DAQmx;

using DAQ.Environment;
using DAQ.FakeData;
using DAQ.HAL;
using Data;
using ScanMaster.Acquire.Plugin;

namespace ConfocalMicroscopeControl
{
    [Serializable]
    class MultiInputShotGathererPlugin : ShotGathererPlugin
    {
        [NonSerialized]
		private Task _countingTask;
		[NonSerialized]
		private Task _freqOutTask;
		[NonSerialized]
        private Task _shotGateTask;
        [NonSerialized]
        private Task _flagTask;
        [NonSerialized]
		private CounterReader _countReader;
        [NonSerialized]
        private Task _inputTask;
        [NonSerialized]
        private AnalogMultiChannelReader _reader;
        [NonSerialized]
        private DigitalSingleChannelWriter _flagWriter;
        [NonSerialized]
		private double[] _latestData;
        [NonSerialized]
        private double[,] _latestAnalogData;
		
		protected override void InitialiseSettings()
		{
			settings["triggerActive"] = false;
            settings["analogChannel"] = "";
            settings["preArm"] = true;
            settings["channel"] = "ConfocalAPD";
            settings["gateLength"] = 100;
            settings["sampleRate"] = 1000;
        }

        public void ReInitialiseSettings(double exp)
        {
            InitialiseSettings();
            settings["gateLength"] = (int)Convert.ToInt64(exp * 0.001 
                * Convert.ToDouble((int)settings["sampleRate"]));
            int settingscheck = (int)settings["gateLength"];
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

            try
            {
                if (config.switchPlugin.State)
                {
                    freqOutTask1.Stop();
                }
                else
                {
                    freqOutTask2.Stop();
                }
            }
            catch (NullReferenceException e)
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
                try
                {
                    if (config.switchPlugin.State)
                    {
                        freqOutTask1.Start();
                    }
                    else
                    {
                        freqOutTask2.Start();
                    }
                }
                catch (NullReferenceException e)
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
				// Calculate the rate in Hz.

                latestData = new double[tempdata.Length - 1];
                for (int k = 0; k < latestData.Length; k++)
                    latestData[k] = (tempdata[k + 1] - tempdata[k]) * (int)settings["sampleRate"] ;

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
                try
                {
                    if (config.switchPlugin.State)
                    {
                        freqOutTask1.Stop();
                    }
                    else
                    {
                        freqOutTask2.Stop();
                    }
                }
                catch (NullReferenceException e)
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
                lock (this)
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
