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
	/// on an E-series board.
	/// </summary>
	[Serializable]
	public class AnalogShotGathererPlugin : ShotGathererPlugin
	{
	
		[NonSerialized]
		private Task inputTask;
		[NonSerialized]
		private AnalogMultiChannelReader reader;
		[NonSerialized]
		private double[,] latestData;
		
		protected override void InitialiseSettings()
		{
            settings["triggered"] = true;
		}

		public override void AcquisitionStarting()
		{
			// configure the analog input
			inputTask = new Task("analog gatherer " /*+ (string)settings["channel"]*/);

			// new analog channel, range -10 to 10 volts
//			if (!Environs.Debug)
//			{
				string channelList = (string)settings["channel"];
				string[] channels = channelList.Split(new char[] {','});

				foreach (string channel in channels)
					((AnalogInputChannel)Environs.Hardware.AnalogInputChannels[channel]).AddToTask(
						inputTask, 
						(double)settings["inputRangeLow"],
						(double)settings["inputRangeHigh"]
						);
            
                // internal clock, finite acquisition
				inputTask.Timing.ConfigureSampleClock(
					"",
					(int)settings["sampleRate"],
					SampleClockActiveEdge.Rising, 
					SampleQuantityMode.FiniteSamples,
					(int)settings["gateLength"]);

                if ((bool)settings["triggered"])
                {
                    inputTask.Triggers.StartTrigger.ConfigureDigitalEdgeTrigger(
                    (string)Environs.Hardware.GetInfo("analogTrigger0"),
                    DigitalEdgeStartTriggerEdge.Rising);
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
			// release the analog input
			inputTask.Dispose();
		}

		public override void ArmAndWait()
		{
			lock (this)
			{
				if (!Environs.Debug) 
				{
                    inputTask.Start();
                    latestData = reader.ReadMultiSample((int)settings["gateLength"]);
                    inputTask.Stop();
				}
			}
		}

		public override Shot Shot
		{
			get 
			{
				lock(this)
				{
					if (!Environs.Debug)
					{
						Shot s = new Shot();
						for (int i = 0 ; i < inputTask.AIChannels.Count ; i++)
						{
							TOF t = new TOF();
 							t.ClockPeriod = (int)settings["clockPeriod"];
                            t.GateStartTime = (int)settings["gateStartTime"];
                            t.Calibration = ((AnalogInputChannel)Environs.Hardware.AnalogInputChannels[inputTask.AIChannels[i].VirtualName]).Calibration;
							double[] tmp = new double[(int)settings["gateLength"]];
							for (int j = 0 ; j < (int)settings["gateLength"] ; j++)
								tmp[j] = latestData[i,j];
							t.Data = tmp;
							s.TOFs.Add(t);
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
