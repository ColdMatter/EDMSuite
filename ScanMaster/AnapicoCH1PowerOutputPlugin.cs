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
	public class AnapicoCH1PowerOutputPlugin : ScanOutputPlugin
	{

		[NonSerialized]
		private double scanParameter;

		[NonSerialized]
		AnapicoSynth synth;

		protected override void InitialiseSettings()
		{
			settings["synth"] = "anapicoSYN420";
			settings["onFrequency"] = 14458087000;
			settings["offAmplitude"] = 1;
			settings["offFrequency"] = 14458087000;
		}

		public override void AcquisitionStarting()
		{
			synth = (AnapicoSynth)Environs.Hardware.Instruments[(string)settings["synth"]];
			synth.Connect();
			synth.Enabled = true;
			synth.CWFrequencyCH1 = (double)settings["onFrequency"];
		}

		public override void ScanStarting()
		{
		}

		public override void ScanFinished()
		{
		}

		public override void AcquisitionFinished()
		{
			synth.PowerCH1 = (double)settings["offAmplitude"];
			synth.CWFrequencyCH1 = (double)settings["offFrequency"];
			synth.Enabled = false;
			synth.Disconnect();
		}

		[XmlIgnore]
		public override double ScanParameter
		{
			set
			{
				scanParameter = value;
				synth.PowerCH1 = ScanParameter;
			}
			get { return scanParameter; }
		}

		
	}
}
