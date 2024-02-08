using System;
using System.Collections;
using System.Xml.Serialization;

using NationalInstruments.DAQmx;

using DAQ.Environment;
using DAQ.HAL;

using ScanMaster.Acquire.Plugin;

namespace ScanMaster.Acquire.Plugins
{
	/// <summary>
	/// A plugin to capture analog data using an E-series board.
	/// </summary>
	[Serializable]
	public class DAQMxAnalogInputPlugin : AnalogInputPlugin
	{
		[NonSerialized]
		private Task inputTask;
		[NonSerialized]
		private AnalogMultiChannelReader reader;
		[NonSerialized]
		private double[] latestData;

		
		protected override void InitialiseSettings()
		{
			// settings["channelList"] =  "iodine,cavity";
		    // settings["inputRangeLow"] = -5.0;
			// settings["inputRangeHigh"] = 5.0;
		}
		

		public override void AcquisitionStarting()
		{
			// configure the analog input
			inputTask = new Task("analog inputs");

			string[] channels = ((String)settings["channelList"]).Split(',');
			if (!Environs.Debug)
			{
				foreach (string channel in channels)
				{
					((AnalogInputChannel)Environs.Hardware.AnalogInputChannels[channel]).AddToTask(
						inputTask, 
						(double)settings["inputRangeLow"],
						(double)settings["inputRangeHigh"]
						);
					}
				inputTask.Control(TaskAction.Verify);
			}
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
					latestData = reader.ReadSingleSample();
					inputTask.Stop();
				}
			}
		}

		[XmlIgnore]
		public override ArrayList Analogs
		{
			get 
			{
				lock(this)
				{
					ArrayList a = new ArrayList();
					if (!Environs.Debug) foreach (double d in latestData) a.Add(d);
					else for (int i = 0 ; i < ((String)settings["channelList"]).Split(',').Length ; i++)
											a.Add(new Random().NextDouble());
					return a;
				}
			}
		}
	}
}
