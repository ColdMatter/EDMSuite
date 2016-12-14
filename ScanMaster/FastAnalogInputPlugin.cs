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
	public class FastAnalogInputPlugin : AnalogInputPlugin
	{
		[NonSerialized]
		private Task inputTask;
		[NonSerialized]
		private AnalogMultiChannelReader reader;
		[NonSerialized]
		private double[] latestData;
        [NonSerialized]
        private BathCryostat bathCryostat;
        [NonSerialized]
        private double[] serialData;

		protected override void InitialiseSettings()
		{
			settings["channelList"] =  "uCavityReflectionECDL,uCavityReflectionTiSapph";
            settings["serialChannelList"] = "BathCryostat";
			settings["inputRangeLow"] = -10.0;
			settings["inputRangeHigh"] = 10.0;
		}

		public override void AcquisitionStarting()
		{
			//make a new serial connection with the elements

            bathCryostat = new BathCryostat((String)Environs.Hardware.GetInfo("BathCryostat"));
            
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
            int[] sensor = {1,2};

            serialData = bathCryostat.ReadCryostatSensor(sensor);
            
            inputTask.Start();
		}

		public override void ScanFinished()
		{
            inputTask.Stop();
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
					latestData = reader.ReadSingleSample();
                    string[] channels = ((String)settings["channelList"]).Split(',');
                    Array.Resize(ref latestData, channels.Length + serialData.Length);
                    for (int i = 0; i < serialData.Length; i++) latestData[channels.Length + i] = serialData[i];
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
