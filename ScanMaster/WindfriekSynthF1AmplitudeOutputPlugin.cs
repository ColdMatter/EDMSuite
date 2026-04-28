using System;
using System.Threading;
using System.Xml.Serialization;

using DAQ.Environment;
using DAQ.HAL;
using ScanMaster.Acquire.Plugin;


namespace ScanMaster.Acquire.Plugins
{
	/// <summary>
	/// A plugin that scans a WindfreakSynthHD's amplitude.
	/// </summary>
	[Serializable]
	public class WindfriekSynthF1AmplitudeOutputPlugin : ScanOutputPlugin
	{
	
		[NonSerialized]
		private double scanParameter;

		[NonSerialized]
		WindfreakSynthHD synth;

		protected override void InitialiseSettings()
		{
			settings["synth"] = "WindfreakDetection"; // WindfreakDetection (two channel synth) or WindfreakDetectionB (mini synth)
			settings["channel"] = 1;				// 0 is channel A and 1 is channel B of windfreak synth detection, default = channel B
			settings["scanOnFrequency"] = 14458087000; //Hz
			settings["offAmplitude"] = 5;		// dBm
			settings["offFrequency"] = 14458087000;	// Hz
		}

		public override void AcquisitionStarting()
		{
			//synth = (WindfreakSynthHD)Environs.Hardware.Instruments["synth"];
			//synth.Connect();
			//synth.SetChannel((int)settings["channel"]);
			//synth.SetFrequency((long)settings["scanOnFrequency"]);
			//synth.SetRFMute(false);
			//synth.SetPLLPowerOn(true);
			//synth.SetPAPowerOn(true);

            string scanDevice = (string)settings["synth"];

            synth = (WindfreakSynthHD)Environs.Hardware.Instruments[scanDevice];
            synth.Connect();

            if (scanDevice == "WindfreakDetection")
            {
                synth.SetChannel((int)settings["channel"]);
                synth.SetFrequency((long)settings["scanOnFrequency"]);
                synth.SetRFMute(false);
                synth.SetPLLPowerOn(true);
                synth.SetPAPowerOn(true);
            }
            else if (scanDevice == "WindfreakDetectionB")
            {
                synth.SetFrequency((long)settings["scanOnFrequency"]);
                synth.SetRFMute(false);
                synth.SetPLLPowerOn(true);
                synth.SetPAPowerOn(true);
            }
            else
            {
                throw new Exception("Invalid scan device setting, please check if the Windfreak synth is on and connected.");
            }

        }

		public override void ScanStarting()
		{
		}

		public override void ScanFinished()
		{
		}

		public override void AcquisitionFinished()
		{
			string scanDevice = (string)settings["synth"];

			synth.SetPowerUEDM((double)settings["offAmplitude"]);
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
				synth.SetPowerUEDM((double)ScanParameter);

				//if (synth != null)
				//{
				//	synth.SetFrequency((long)ScanParameter);
				//}

			}
			get { return scanParameter; }
		}

		
	}
}
