using System;
using System.Threading;
using System.Xml.Serialization;

using DAQ.Environment;
using DAQ.HAL;
using ScanMaster.Acquire.Plugin;


namespace ScanMaster.Acquire.Plugins
{
	/// <summary>
	/// A plugin that scans a synth's frequency.
	/// </summary>
	[Serializable]
	public class SynthFrequencyOutputPlugin : ScanOutputPlugin
	{
	
		[NonSerialized]
		private double scanParameter;

		[NonSerialized]
		Synth synth;

		protected override void InitialiseSettings()
		{
			settings["synth"] = "green";
			settings["scanOnAmplitude"] = 170.254;
			settings["offAmplitude"] = -130.0;
			settings["offFrequency"] = 168.0;
			// If true, at the end of the acquisition the UEDM hardware controller puts the synth back to the
			// settings in its Stirap RF box, instead of this plugin setting it to offAmplitude/offFrequency
			settings["restoreUEDMHardwareControllerSettings"] = false;
		}

		public override void AcquisitionStarting()
		{
			synth = (Synth)Environs.Hardware.Instruments[(string)settings["synth"]];
			synth.Connect();
			synth.Amplitude = (double)settings["scanOnAmplitude"];
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
			// Profiles saved before this setting existed don't have it, so treat a missing value as false
			object restore = settings["restoreUEDMHardwareControllerSettings"];
			if (restore != null && Convert.ToBoolean(restore))
			{
				synth.Disconnect();
				try
				{
					UEDMHardwareControl.UEDMController hardwareController = (UEDMHardwareControl.UEDMController)Activator.GetObject(
						typeof(UEDMHardwareControl.UEDMController), "tcp://localhost:1172/UEDMController.rem");
					hardwareController.RestoreGreenSynthToUISettings();
					return;
				}
				catch (Exception)
				{
					// Hardware controller not reachable - fall back to the usual off settings below
				}
			}
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
				synth.Frequency = ScanParameter;
			}
			get { return scanParameter; }
		}

		
	}
}
