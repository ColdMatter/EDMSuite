using System;
using System.Threading;
using System.Xml.Serialization;

using DAQ.Environment;
using DAQ.HAL;
using ScanMaster.Acquire.Plugin;


namespace ScanMaster.Acquire.Plugins
{
	/// <summary>
	/// A plugin that scans a WindfreakSynthHD's frequency.
	/// </summary>
	[Serializable]
	public class WindfriekSynthFrequencyOutputPlugin : ScanOutputPlugin
	{
	
		[NonSerialized]
		private double scanParameter;

		[NonSerialized]
		WindfreakSynthHD synth;

		protected override void InitialiseSettings()
		{
			settings["synth"] = "WindfreakDetection";
			settings["channel"] = 0;				// 0 is channel A and 1 is channel B
			settings["scanOnAmplitude"] = 7.0;		//dBm
			settings["offAmplitude"] = -30.0;		// dBm
			settings["offFrequency"] = 14467242000;	// Hz
		}

		public override void AcquisitionStarting()
		{
			synth = (WindfreakSynthHD)Environs.Hardware.Instruments[(string)settings["synth"]];
			synth.Connect();
			synth.SetChannel((int)settings["channel"]);
			synth.SetPower((double)settings["scanOnAmplitude"]);

			synth.SetRFMute(false);
			synth.SetPLLPowerOn(true);
			synth.SetPAPowerOn(true);
		}

		public override void ScanStarting()
		{
		}

		public override void ScanFinished()
		{
		}

		public override void AcquisitionFinished()
		{
			synth.SetPower((double)settings["offAmplitude"]); 
			synth.SetFrequency((long)settings["offFrequency"]);
			synth.SetRFMute(true);
			synth.SetPLLPowerOn(false);
			synth.SetPAPowerOn(false);
			synth.Disconnect();
		}

		[XmlIgnore]
		public override double ScanParameter
		{
			set
			{
				scanParameter = value;
				synth.SetFrequency((long)ScanParameter);
			}
			get { return scanParameter; }
		}

		
	}
}
