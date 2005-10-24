using System;
using System.Xml.Serialization;

using DAQ.Environment;
using DAQ.Pattern;

using ScanMaster.Acquire.Patterns;
using ScanMaster.Acquire.Plugin;


namespace ScanMaster.Acquire.Plugins
{
	/// <summary>
	/// A pattern plugin for running two rf loops from the same synth.
	/// </summary>
	[Serializable]
	public class PulsedRFScanPatternPlugin : SupersonicPGPluginBase
	{

		[NonSerialized]
		private PulsedRFScanPatternBuilder scanPatternBuilder;

		protected override void InitialiseCustomSettings()
		{
			settings["ttlSwitchPort"] = 0;
			settings["ttlSwitchLine"] = 5;
			settings["rf1CentreTime"] = 1400;
			settings["rf1Length"] = 450;
			settings["rf2CentreTime"] = 700;
			settings["rf2Length"] = 650;
			settings["fmCentreTime"] = 700;
			settings["fmLength"] = 650;
			settings["piFlipTime"] = 1400;
		}

		protected override void DoAcquisitionStarting()
		{
			scanPatternBuilder = new PulsedRFScanPatternBuilder();
		}

		protected override IPatternSource GetScanPattern()
		{
			// switch over to the scan pattern
			scanPatternBuilder = new PulsedRFScanPatternBuilder();
			scanPatternBuilder.Clear();
			scanPatternBuilder.ShotSequence(
				0,
				(int)settings["sequenceLength"],
				(int)settings["padShots"],
				(int)settings["flashlampPulseInterval"],
				(int)settings["valvePulseLength"],
				(int)settings["valveToQ"],
				(int)settings["flashToQ"],
				(int)config.shotGathererPlugin.Settings["gateStartTime"],
				(int)settings["rf1CentreTime"],
				(int)settings["rf1Length"],
				(int)settings["rf2CentreTime"],
				(int)settings["rf2Length"],
				(int)settings["piFlipTime"],
				(int)settings["fmCentreTime"],
				(int)settings["fmLength"]
				);
			scanPatternBuilder.BuildPattern(((int)settings["padShots"] + 1) * (int)settings["sequenceLength"]
				* (int)settings["flashlampPulseInterval"]);
			return scanPatternBuilder;
		}

	}
}
