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
	/// A plugin to capture analog data after the scan has finished this is designed to work with FastAnalogShotGatherer because
    /// that shot gatherer hogs the resources of the card and won't allow analog inputs for every shot.
	/// </summary>
	[Serializable]
	public class AnalogInputAfterScanPlugin : AnalogInputPlugin
	{
		[NonSerialized]
		private Task inputTask;
		[NonSerialized]
		private AnalogMultiChannelReader reader;
		[NonSerialized]
		private double[] latestData;

		protected override void InitialiseSettings()
		{
			settings["channelList"] =  "ECDLError,TiSapphError,TiSapphMonitor";
            settings["serialChannelList"] = "BathCryostat";
			settings["inputRangeLow"] = -10.0;
			settings["inputRangeHigh"] = 10.0;
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

		public override void AcquisitionFinished()
		{
           
            // release the analog input
			inputTask.Dispose();
		}
		
		public override void ArmAndWait()
		{
		}

		[XmlIgnore]
		public override ArrayList Analogs
		{
			get 
			{
				lock(this)
				{
					ArrayList a = new ArrayList();
					if (!Environs.Debug && latestData!=null) foreach (double d in latestData) a.Add(d);
					else for (int i = 0 ; i < ((String)settings["channelList"]).Split(',').Length ; i++)
											a.Add(new Random().NextDouble());
					return a;
				}
			}
		}
	}
}
