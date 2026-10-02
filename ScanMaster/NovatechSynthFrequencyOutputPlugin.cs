using System;
using System.Threading;
using System.Xml.Serialization;

using DAQ.Environment;
using DAQ.HAL;
using ScanMaster.Acquire.Plugin;


namespace ScanMaster.Acquire.Plugins
{
	/// <summary>
	/// A plugin that scans the frequency of one channel of a Novatech 426A synth.
	/// The scan parameter is the output frequency in MHz (0.3 - 400 MHz).
	/// </summary>
	[Serializable]
	public class NovatechSynthFrequencyOutputPlugin : ScanOutputPlugin
	{

		[NonSerialized]
		private double scanParameter;

		[NonSerialized]
		Novatech426ASynth synth;

		[NonSerialized]
		Novatech426ASynth.Novatech426AChannel channel;

		protected override void InitialiseSettings()
		{
			settings["synth"] = "Novatech426A"; // name of the instrument in the hardware class
			settings["channel"] = 0;            // 0 or 1
			settings["scanOnAmplitude"] = 1023; // 0-1023, 1023 is full scale
			settings["offAmplitude"] = 0;       // 0-1023, 0 turns the output off
			settings["offFrequency"] = 10.0;    // MHz
		}

		public override void AcquisitionStarting()
		{
			// No Connect() needed: the Novatech class opens and closes the serial port for each command.
			synth = (Novatech426ASynth)Environs.Hardware.Instruments[(string)settings["synth"]];
			channel = synth.Channel((int)settings["channel"]);
			channel.Amplitude = (int)settings["scanOnAmplitude"];
			channel.Enabled = true;
		}

		public override void ScanStarting()
		{
		}

		public override void ScanFinished()
		{
		}

		public override void AcquisitionFinished()
		{
			channel.Amplitude = (int)settings["offAmplitude"];
			channel.Frequency = (double)settings["offFrequency"];
		}

		[XmlIgnore]
		public override double ScanParameter
		{
			set
			{
				scanParameter = value;
				channel.Frequency = ScanParameter;
			}
			get { return scanParameter; }
		}

	}
}
