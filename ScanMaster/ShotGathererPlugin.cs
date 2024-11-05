using System;
using System.Collections;
using System.Xml.Serialization;

using Data;
using ScanMaster.Acquire.Plugins;

namespace ScanMaster.Acquire.Plugin
{

	/// <summary>
	/// A plugin representing something that gathers time of flight
	/// data.
	/// </summary>
	[Serializable]
	[XmlInclude(typeof(AnalogShotGathererPlugin)), XmlInclude(typeof(NullShotGathererPlugin)), 
	XmlInclude(typeof(ModulatedAnalogShotGathererPlugin)), XmlInclude(typeof(CCDModulatedAnalogShotGathererPlugin)), XmlInclude(typeof(BufferedEventCountingShotGathererPlugin)),
    XmlInclude(typeof(ImageGrabbingAnalogShotGathererPlugin))]
	public abstract class ShotGathererPlugin : AcquisitorPlugin
	{
		protected override void InitialiseBaseSettings()
		{
			settings["gateStartTime"] = 1900;
			settings["gateLength"] = 700;
			settings["clockPeriod"] = 1;
			settings["sampleRate"] = 1000000;
			settings["channel"] = "pmt";
			settings["inputRangeLow"] = -1.0;
			settings["inputRangeHigh"] = 1.0;
			//01Oct2024, we decided to add below new variables for our purpose to change the TOF gate via terminal communication
			settings["TOFgateSelectionStartInMs"] = 25;
			settings["TOFgateSelectionEndInMs"] = 28;
			settings["TOFgateBgStartInMs"] =35;
			settings["TOFgateBgEndInMs"] = 40;
			
		}
		
		/// <summary>
		/// Arm the hardware and wait. This method should not return until it
		/// is safe for the acquisitor to read the shot data out.
		/// </summary>
		public abstract void ArmAndWait();

		/// <summary>
		/// The most recently acquired shot.
		/// </summary>
		public abstract Shot Shot 
		{
			get;
		}

	}
}
