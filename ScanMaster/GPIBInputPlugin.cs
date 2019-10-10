using System;
using System.Collections;
using System.Xml.Serialization;

using ScanMaster.Acquire.Plugins;

namespace ScanMaster.Acquire.Plugin
{
	/// <summary>
	/// A plugin to acquire a GPIB channel. The acquisitor will call
	/// this plugin to capture for each point in the scan.
	/// </summary>
	[Serializable]
	[
    XmlInclude(typeof(SingleCounterInputPlugin)),
    XmlInclude(typeof(NullGPIBInputPlugin)),
#if DECELERATOR
    XmlInclude(typeof(DecelerationHardwareAnalogInputPlugin))
#endif
    ]
	public abstract class GPIBInputPlugin : AcquisitorPlugin
	{
	
		/// <summary>
		/// This method will be called by the acquisitor. The method should not return
		/// control until it safe for the acquisitor to read the Analogs property. Implementors
		/// should be aware that this method runs on the acquisitor thread - taking too long in
		/// here can knock the acquisitor out of sync with the pattern generator.
		/// </summary>
		public abstract void ArmAndWait();

		/// <summary>
		/// The most recently acquired analog samples.
		/// </summary>
		[XmlIgnore]
		public abstract double GPIBval
		{
			get;
		}

		protected override void InitialiseBaseSettings()
		{
//			settings["channelList"] =  "iodine,cavity";
//			settings["inputRangeLow"] = -5;
//			settings["inputRangeHigh"] = 5;
		}

	}
}
