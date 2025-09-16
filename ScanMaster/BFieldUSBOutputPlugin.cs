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
	public class BFieldUSBOutputPlugin : ScanOutputPlugin
	{
	
		[NonSerialized]
		private double scanParameter;

		[NonSerialized]
		TwinleafCSB currentSource;

		protected override void InitialiseSettings()
		{
			settings["USBDevice"] = "bCurrentSource";
		}

		public override void AcquisitionStarting()
		{
			currentSource = (TwinleafCSB)Environs.Hardware.Instruments[(string)settings["USBDevice"]];
			currentSource.Connect();
		}

		public override void ScanStarting()
		{
		}

		public override void ScanFinished()
		{
		}

		public override void AcquisitionFinished()
		{
			currentSource.SetCurrent(0.00);
			currentSource.Disconnect();
		}

		[XmlIgnore]
		public override double ScanParameter
		{
			set
			{
				scanParameter = value;
				currentSource.SetCurrent((double)ScanParameter);
			}
			get { return scanParameter; }
		}

		
	}
}
