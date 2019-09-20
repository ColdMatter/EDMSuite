using System;
using System.Threading;
using System.Xml.Serialization;

using DAQ.Environment;
using DAQ.HAL;
using ScanMaster.Acquire.Plugin;

namespace ScanMaster.Acquire.Plugins
{
	/// <summary>
	/// A plugin that scans a synth's amplitude.
	/// </summary>
	[Serializable]
	public class SynthAmplitudeOutputPlugin : ScanOutputPlugin
	{

		[NonSerialized]
		private double scanParameter;

		[NonSerialized]
		Synth synth;

		protected override void InitialiseSettings()
		{
			settings["synth"] = "green";
			settings["onFrequency"] = 170.254;
			settings["offAmplitude"] = -130.0;
			settings["offFrequency"] = 168.0;
		}

		public override void AcquisitionStarting()
		{
			synth = (Synth)Environs.Hardware.Instruments[(string)settings["synth"]];
			synth.Connect();
			synth.Frequency = (double)settings["onFrequency"];
			synth.DCFMEnabled = false;
		}

		public override void ScanStarting()
		{
		}

		public override void ScanFinished()
		{
		}

		public override void AcquisitionFinished()
		{
			synth.Amplitude = (double)settings["offAmplitude"];
			synth.Frequency = (double)settings["offFrequency"];
			synth.Disconnect();
		}

		[XmlIgnore]
		public override double ScanParameter
		{
			set
			{
				scanParameter = value;
				synth.Amplitude = ScanParameter;
			}
			get { return scanParameter; }
		}

		
	}
}
