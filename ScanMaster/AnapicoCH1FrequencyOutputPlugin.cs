using System;
using System.Threading;
using System.Xml.Serialization;

using DAQ.Environment;
using DAQ.HAL;
using ScanMaster.Acquire.Plugin;


namespace ScanMaster.Acquire.Plugins
{
	/// <summary>
	/// A plugin that scans the Anapico frequency.
	/// </summary>
	[Serializable]
	public class AnapicoCH1FrequencyOutputPlugin : ScanOutputPlugin
	{
	
		[NonSerialized]
		private double scanParameter;

		[NonSerialized]
		AnapicoSynth synth;

		protected override void InitialiseSettings()
		{
			settings["synth"] = "anapicoSYN420";
			//settings["scanOnAmplitude"] = 23.0;			// in dBm
			//settings["offAmplitude"] = 1;				// in dBm
			settings["offFrequency"] = 14458087000;		// in Hz
		}

		public override void AcquisitionStarting()
		{
			synth = (AnapicoSynth)Environs.Hardware.Instruments[(string)settings["synth"]];
			synth.Connect();
			synth.Enabled = true;
			synth.Disconnect();
			//synth.PowerCH1 = (double)settings["scanOnAmplitude"];
		}

		public override void ScanStarting()
		{
		}

		public override void ScanFinished()
		{
		}

		public override void AcquisitionFinished()
		{
			synth.Connect();
			//synth.PowerCH1 = (double)settings["offAmplitude"];
			//synth.CWFrequencyCH1 = (double)settings["offFrequency"];
			synth.Enabled = false;
			synth.Disconnect();
		}

		[XmlIgnore]
		public override double ScanParameter
		{
			set
			{
				scanParameter = value;
				synth.Connect();
				synth.CWFrequencyCH1 = ScanParameter;
				synth.Disconnect();
			}
			get { return scanParameter; }
		}

		
	}
}
