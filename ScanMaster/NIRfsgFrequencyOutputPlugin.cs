using System;
using System.Threading;
using System.Xml.Serialization;

using DAQ.Environment;
using DAQ.HAL;
using ScanMaster.Acquire.Plugin;


namespace ScanMaster.Acquire.Plugins
{
	/// <summary>
	/// A plugin that scans the frequency of an NI-Rfsg device. 
    /// It is very similar to a Synth but it's not GPIB-connected, hence the duplication.
	/// </summary>
	[Serializable]
	public class NIRfsgFrequencyOutputPlugin : ScanOutputPlugin
	{
	
		[NonSerialized]
		private double scanParameter;

		[NonSerialized]
		NIRfsgInstrument niRfsg;

		protected override void InitialiseSettings()
		{
			settings["synth"] = "rfAWG";
			settings["scanOnAmplitude"] = 10;
			settings["offAmplitude"] = -130.0;
			settings["offFrequency"] = 168.0;
		}

		public override void AcquisitionStarting()
		{
			niRfsg = (NIRfsgInstrument)Environs.Hardware.Instruments[(string)settings["synth"]];
            niRfsg.Connect();
			niRfsg.Amplitude = (double)settings["scanOnAmplitude"];
            niRfsg.Frequency = (double)settings["offFrequency"];
            niRfsg.StartGeneration();
		}

		public override void ScanStarting()
		{
		}

		public override void ScanFinished()
		{
		}

		public override void AcquisitionFinished()
		{
			niRfsg.Amplitude = (double)settings["offAmplitude"];
			niRfsg.Frequency = (double)settings["offFrequency"];
            niRfsg.StopGeneration();
			niRfsg.Disconnect();
		}

		[XmlIgnore]
		public override double ScanParameter
		{
			set
			{
				scanParameter = value;
				niRfsg.Frequency = ScanParameter;
                niRfsg.UpdateGeneration();
			}
			get { return scanParameter; }
		}

		
	}
}
