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
	/// A plugin to capture time-resolved data by buffered event counting.
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
	/// </summary>
	[Serializable]
	public class BufferedEventCountingShotGathererPlugin : ShotGathererPlugin
	{
		[NonSerialized]
		private Task countingTask;
		[NonSerialized]
		private Task freqOutTask1;
        [NonSerialized]
        private Task freqOutTask2;
		[NonSerialized]
		private CounterReader countReader;
		[NonSerialized]
		private double[] latestData;
		
		protected override void InitialiseSettings()
		{
			settings["triggerActive"] = true;
		}

		public override void AcquisitionStarting()
		{
			//set up an edge-counting task
			countingTask = new Task("buffered edge counter gatherer " + (string)settings["channel"]);

			//count upwards on rising edges starting from zero
			countingTask.CIChannels.CreateCountEdgesChannel(
				((CounterChannel)Environs.Hardware.CounterChannels[(string)settings["channel"]]).PhysicalChannel,
				"edge counter", 
				CICountEdgesActiveEdge.Rising, 
				0, 
				CICountEdgesCountDirection.Up);

			//The counting buffer is triggered by a sample clock, which will be routed to the gate pin of ctr0 (PFI9)
			//The number of samples to collect is determined by the "gateLength" setting. We add 1 to this,
			// since the first count is not synchronized to anything and will be discarded
			countingTask.Timing.ConfigureSampleClock(
				(string)Environs.Hardware.Boards["daq"] + "/PFI9", 
				(int)settings["sampleRate"], 
				SampleClockActiveEdge.Rising, 
				SampleQuantityMode.FiniteSamples, 
				(int)settings["gateLength"] + 1);

			countingTask.Control(TaskAction.Verify);

			// set up two taska to generate the sample clock on the second counter
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
			freqOutTask1.Timing.ConfigureImplicit(SampleQuantityMode.ContinuousSamples, 1000);

            freqOutTask2.COChannels.CreatePulseChannelFrequency(
                ((CounterChannel)Environs.Hardware.CounterChannels["sample clock"]).PhysicalChannel,
                "photon counter clocking signal",
                COPulseFrequencyUnits.Hertz,
                COPulseIdleState.Low,
                0,
                (int)settings["sampleRate"],
                0.5);
            freqOutTask2.Timing.ConfigureImplicit(SampleQuantityMode.ContinuousSamples, 1000);
			
			
			// if we're using a hardware trigger to synchronize data acquisition, we need to set up the 
			// trigger parameters on the sample clock.
            // The first output task is triggered on PFI0 and the second is triggered on PFI1
			if((bool)settings["triggerActive"])
			{
				freqOutTask1.Triggers.StartTrigger.Type = StartTriggerType.DigitalEdge;
				freqOutTask1.Triggers.StartTrigger.DigitalEdge.Edge = DigitalEdgeStartTriggerEdge.Rising;
                freqOutTask2.Triggers.StartTrigger.Type = StartTriggerType.DigitalEdge;
                freqOutTask2.Triggers.StartTrigger.DigitalEdge.Edge = DigitalEdgeStartTriggerEdge.Rising;
				// the trigger is expected to appear on PFI0
				freqOutTask1.Triggers.StartTrigger.DigitalEdge.Source = (string)Environs.Hardware.Boards["daq"] + "/PFI0";
                // the trigger is expected to appear on PFI1
                freqOutTask2.Triggers.StartTrigger.DigitalEdge.Source = (string)Environs.Hardware.Boards["daq"] + "/PFI1";
            }
		
			// set up a reader for the edge counter
			countReader = new CounterReader(countingTask.Stream);
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
		}

		public override void ArmAndWait()
		{
			lock(this)
			{
				
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
				
				// read the data into a temporary array once all the samples have been acquired
				double[] tempdata = countReader.ReadMultiSampleDouble(-1);

				// stop the counter; the job's done
				countingTask.Stop();

				// stop the sample clock
                if (config.switchPlugin.State)
                {
                    freqOutTask1.Stop();
                }
                else
                {
                    freqOutTask2.Stop();
                }

				// Each element of the buffer holds the incremental count. Discard the first value and calculate
				// the counts per bin by subtracting the count in each bin by the count in the previous bin.
				// Calculate the rate in kHz.
				latestData = new double[tempdata.Length - 1];
				for (int k = 0; k < latestData.Length; k++) 
					latestData[k] = (tempdata[k + 1] - tempdata[k])*(int)settings["sampleRate"]/1000;
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
