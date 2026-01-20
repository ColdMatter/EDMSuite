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
	/// A plugin to capture time of flight data by sampling an analog input
	/// on an E-series board. Deals with the case where shots are to be gathered
	/// synchronously with something that switches. This is done by making use
	/// of both detector trigger inputs on the board. The pattern that modulates 
	/// the switched channel, should also modulate the trigger channel so that this
	/// shot gatherer has the right behaviour.
	/// </summary>
	[Serializable]
	public class ModulatedFourAnalogShotGathererPlugin : ShotGathererPlugin
	{
	
		[NonSerialized]
		private Task inputTask1;
		[NonSerialized]
		private Task inputTask2;
		[NonSerialized]
		private AnalogMultiChannelReader reader1;
		[NonSerialized]
		private AnalogMultiChannelReader reader2;
		[NonSerialized]
        private double[,] latestData;

		[NonSerialized]
		private Task inputTask3;
		[NonSerialized]
		private Task inputTask4;
		[NonSerialized]
		private Task counterTask1;
		[NonSerialized]
		private CounterSingleChannelWriter counter1;
		[NonSerialized]
		private AnalogMultiChannelReader reader3;
		[NonSerialized]
		private AnalogMultiChannelReader reader4;
		[NonSerialized]
		private double[,] latestData2;

		protected override void InitialiseSettings()
		{
			settings["gateStartTime"] = 600;
			settings["gateLength"] = 12000;
			settings["ccdEnableStartTimeInMs"] = 0;
			settings["ccdEnableLengthInMs"] = 10;
			settings["clockPeriod"] = 1;
			settings["sampleRate"] = 100000;
			settings["channel"] = "pmt";
			settings["cameraChannel"] = "cameraEnabler";
			settings["inputRangeLow"] = -1.0;
			settings["inputRangeHigh"] = 10.0;
			settings["TOFgateSelectionStartInMs"] = 25;
			settings["TOFgateSelectionEndInMs"] = 28;
			settings["TOFgateBgStartInMs"] = 35;
			settings["TOFgateBgEndInMs"] = 40;
		}

		public override void AcquisitionStarting()
		{
			// configure the analog input
			inputTask1 = new Task("analog gatherer 1 -" /*+ (string)settings["channel"]*/);
			inputTask2 = new Task("analog gatherer 2 -" /*+ (string)settings["channel"]*/);
			
			inputTask3 = new Task("analog gatherer 3 -" /*+ (string)settings["channel"]*/);
			inputTask4 = new Task("analog gatherer 4 -" /*+ (string)settings["channel"]*/);

			counterTask1 = new Task("CCD enable Task Counter");

			// new analog channel, range -10 to 10 volts
			if (!Environs.Debug)
			{
				
                string channelList = (string)settings["channel"];// Add channels to this list
                string[] channels = channelList.Split(new char[] { ',' });
				string camChannel = (string)settings["cameraChannel"];

				foreach (string channel in channels)
                {
                    ((AnalogInputChannel)Environs.Hardware.AnalogInputChannels[channel]).AddToTask(
                        inputTask1,
                        (double)settings["inputRangeLow"],
                        (double)settings["inputRangeHigh"]
                        );
                    ((AnalogInputChannel)Environs.Hardware.AnalogInputChannels[channel]).AddToTask(
						inputTask2,
						(double)settings["inputRangeLow"],
						(double)settings["inputRangeHigh"]
						);
					 ((AnalogInputChannel)Environs.Hardware.AnalogInputChannels[channel]).AddToTask(
						inputTask3,
						(double)settings["inputRangeLow"],
						(double)settings["inputRangeHigh"]
						);
					((AnalogInputChannel)Environs.Hardware.AnalogInputChannels[channel]).AddToTask(
						inputTask4,
						(double)settings["inputRangeLow"],
						(double)settings["inputRangeHigh"]
						);
                }

				CounterChannel pulseChannel = ((CounterChannel)Environs.Hardware.CounterChannels[camChannel]);

				counterTask1.COChannels.CreatePulseChannelTicks(
					pulseChannel.PhysicalChannel,
					pulseChannel.Name,
					"100kHzTimebase",
					COPulseIdleState.Low,
					100 * (int)settings["ccdEnableStartTimeInMs"],//the delay in ms, the multiplier needs to be changed for a different timebase. E.g. 100 should be used for 100 kHz
					100,
					100 * (int)settings["ccdEnableLengthInMs"]//the duration in ms
					);

				// internal clock, finite acquisition
				inputTask1.Timing.ConfigureSampleClock(
					"",
					(int)settings["sampleRate"],
					SampleClockActiveEdge.Rising, 
					SampleQuantityMode.FiniteSamples,
					(int)settings["gateLength"]);
				
				inputTask2.Timing.ConfigureSampleClock(
					"",
					(int)settings["sampleRate"],
					SampleClockActiveEdge.Rising, 
					SampleQuantityMode.FiniteSamples,
					(int)settings["gateLength"]);

				inputTask3.Timing.ConfigureSampleClock(
					"",
					(int)settings["sampleRate"],
					SampleClockActiveEdge.Rising, 
					SampleQuantityMode.FiniteSamples,
					(int)settings["gateLength"]);

				inputTask4.Timing.ConfigureSampleClock(
					"",
					(int)settings["sampleRate"],
					SampleClockActiveEdge.Rising, 
					SampleQuantityMode.FiniteSamples,
					(int)settings["gateLength"]);

				counterTask1.Timing.ConfigureImplicit(SampleQuantityMode.FiniteSamples, 1);

				// trigger off PFI0 (with the standard routing, that's the same as trig1)
				inputTask1.Triggers.StartTrigger.ConfigureDigitalEdgeTrigger(
                    (string)Environs.Hardware.GetInfo("analogTrigger0"),
					DigitalEdgeStartTriggerEdge.Rising);
				// trigger off PFI1 (with the standard routing, that's the same as trig2)
				inputTask2.Triggers.StartTrigger.ConfigureDigitalEdgeTrigger(
					(string)Environs.Hardware.GetInfo("analogTrigger1"),
					DigitalEdgeStartTriggerEdge.Rising);
				inputTask3.Triggers.StartTrigger.ConfigureDigitalEdgeTrigger(
					(string)Environs.Hardware.GetInfo("analogTrigger3"),
					DigitalEdgeStartTriggerEdge.Rising);
				inputTask4.Triggers.StartTrigger.ConfigureDigitalEdgeTrigger(
					(string)Environs.Hardware.GetInfo("analogTrigger4"),
					DigitalEdgeStartTriggerEdge.Rising);

				counterTask1.Triggers.StartTrigger.ConfigureDigitalEdgeTrigger(
					(string)Environs.Hardware.GetInfo("analogTrigger2"),
					DigitalEdgeStartTriggerEdge.Rising);

				inputTask1.Control(TaskAction.Verify);
				inputTask2.Control(TaskAction.Verify);
				inputTask3.Control(TaskAction.Verify);
				inputTask4.Control(TaskAction.Verify);
				counterTask1.Control(TaskAction.Verify);
			}
            reader1 = new AnalogMultiChannelReader(inputTask1.Stream);
			reader2 = new AnalogMultiChannelReader(inputTask2.Stream);
			reader3 = new AnalogMultiChannelReader(inputTask3.Stream);
			reader4 = new AnalogMultiChannelReader(inputTask4.Stream);
			counter1 = new CounterSingleChannelWriter(counterTask1.Stream);
		}

		public override void ScanStarting()
		{
		}

		public override void ScanFinished()
		{
		}

		public override void AcquisitionFinished()
		{
			// release the analog input
			inputTask1.Dispose();
			inputTask2.Dispose();
			inputTask3.Dispose();
			inputTask4.Dispose();
			counterTask1.Dispose();
		}

		public override void ArmAndWait()
		{
			lock (this)
			{
				if (!Environs.Debug) 
				{
					if (config.switchPlugin.State == true)
					{
						counterTask1.Start();
						inputTask1.Start(); //"analogTrigger0" Slow On YAG On
						latestData = reader1.ReadMultiSample((int)settings["gateLength"]);
						inputTask1.Stop();
						counterTask1.Stop();

						counterTask1.Start();
						inputTask3.Start();//"analogTrigger3" Slow On YAG Off
						latestData2 = reader3.ReadMultiSample((int)settings["gateLength"]);
						inputTask3.Stop();
						counterTask1.Stop();
					}
					else
					{
						counterTask1.Start();
						inputTask2.Start(); //"analogTrigger1" Slow Off YAG on
						latestData = reader2.ReadMultiSample((int)settings["gateLength"]);
						inputTask2.Stop();
						counterTask1.Stop();

						counterTask1.Start();
						inputTask4.Start();//"analogTrigger4" Slow Off YAG off
						latestData2 = reader4.ReadMultiSample((int)settings["gateLength"]);
						inputTask4.Stop();
						counterTask1.Stop();
					}
				}
			}
		}

		public override Shot Shot
		{
			get 
			{
				lock(this)
				{
					Shot s = new Shot();
					s.SetTimeStamp();
					if (!Environs.Debug)
					{
						for (int i = 0 ; i < inputTask1.AIChannels.Count ; i++)
						{
							TOF t = new TOF();
							t.ClockPeriod = (int)settings["clockPeriod"];
                            t.GateStartTime = (int)settings["gateStartTime"];
							double[] tmp = new double[(int)settings["gateLength"]];
							for (int j = 0 ; j < (int)settings["gateLength"] ; j++)
								tmp[j] = latestData[i,j];
							t.Data = tmp;
							s.TOFs.Add(t);

							TOF t2 = new TOF();
							t2.ClockPeriod = (int)settings["clockPeriod"];
							t2.GateStartTime = (int)settings["gateStartTime"];
							tmp = new double[(int)settings["gateLength"]];
							for (int j = 0; j < (int)settings["gateLength"]; j++)
								tmp[j] = latestData2[i, j];
							t2.Data = tmp;
							s.TOFs.Add(t2);
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
