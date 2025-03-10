using System;
using System.Xml.Serialization;

using ScanMaster.Acquire.Plugins;

namespace ScanMaster.Acquire.Plugin
{
	/// <summary>
	/// A plugin representing something that can be scanned.
	/// </summary>
	[Serializable]
	[
    XmlInclude(typeof(DAQMxAnalogOutputPlugin)),
    XmlInclude(typeof(NullOutputPlugin)), 
	XmlInclude(typeof(SynthAmplitudeOutputPlugin)),
	XmlInclude(typeof(SynthFrequencyOutputPlugin)),
    XmlInclude(typeof(PGOutputPlugin)),
    XmlInclude(typeof(TCLOutputPlugin)),
	XmlInclude(typeof(WindfriekSynthFrequencyOutputPlugin)),
	XmlInclude(typeof(WindfriekSynthF0AmplitudeOutputPlugin)),
	XmlInclude(typeof(WindfriekSynthF1AmplitudeOutputPlugin)),
	XmlInclude(typeof(WindfriekOPAmplitudeOutputPlugin)),
	XmlInclude(typeof(BFieldUSBOutputPlugin)),
	XmlInclude(typeof(WMLOutputPlugin)),
	XmlInclude(typeof(DTCLOutputPlugin)),
	XmlInclude(typeof(MOTMasterScan)),
	XmlInclude(typeof(ManualOutputPlugin)),
	XmlInclude(typeof(MSquaredOutputPlugin))
#if DECELERATOR
    ,XmlInclude(typeof(DecelerationHardwareAnalogOutputPlugin))
#endif
#if EDM
    ,XmlInclude(typeof(NIRfsgAmplitudeOutputPlugin))
    ,XmlInclude(typeof(NIRfsgFrequencyOutputPlugin))
    ,XmlInclude(typeof(HardwareControllerOutputPlugin))
#endif
#if ultracoldEDM
	,XmlInclude(typeof(UEDMHardwareControllerOutputPlugin))
#endif
	]
	public abstract class ScanOutputPlugin : AcquisitorPlugin
	{
		protected override void InitialiseBaseSettings()
		{
			settings["start"] = 0.0;
			settings["end"] = 10.0;
			settings["pointsPerScan"] = 200;
            settings["shotsPerPoint"] = 1;
            settings["scanMode"] = "up"; //allowed values are up, down, updown, downup and random
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
