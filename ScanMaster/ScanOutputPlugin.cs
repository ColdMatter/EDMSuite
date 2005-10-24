using System;
using System.Xml.Serialization;

using ScanMaster.Acquire.Plugins;

namespace ScanMaster.Acquire.Plugin
{
	/// <summary>
	/// A plugin representing something that can be scanned.
	/// </summary>
	[Serializable]
	[XmlInclude(typeof(DAQMxAnalogOutputPlugin)), XmlInclude(typeof(NullOutputPlugin)), 
	 XmlInclude(typeof(SynthAmplitudeOutputPlugin)),
	 XmlInclude(typeof(SynthFrequencyOutputPlugin)), XmlInclude(typeof(PGOutputPlugin))]
	public abstract class ScanOutputPlugin : AcquisitorPlugin
	{
		protected override void InitialiseBaseSettings()
		{
			settings["start"] = 0.0;
			settings["end"] = 10.0;
			settings["pointsPerScan"] = 200;
		}

		/// <summary>
		/// Setting the scan parameter here should update the hardware if needed.
		/// Reading the scan parameter should just return the value (quickly).
		/// </summary>
		[XmlIgnore]
		public abstract double ScanParameter
		{
			get;
			set;
		}

	}
}
