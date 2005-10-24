using System;

using System.Xml.Serialization;

using DAQ.Environment;
using DAQ.Pattern;

using ScanMaster.Acquire.Patterns;
using ScanMaster.Acquire.Plugin;

namespace ScanMaster.Acquire.Plugins
{
	/// <summary>
	/// 
	/// </summary>
	[Serializable]
	public class PumpProbePatternPlugin : SupersonicPGPluginBase
	{
		[NonSerialized]
		private PumpProbePatternBuilder scanPatternBuilder;

		protected override void InitialiseCustomSettings()
		{
			settings["aomOnStart"] = 100;
			settings["aomOnDuration"] = 100;
			settings["aomOffStart"] = 140;
			settings["aomOffDuration"] = 20;
			settings["ttlSwitchPort"] = 0;
			settings["ttlSwitchLine"] = 5;
		}

		protected override void DoAcquisitionStarting()
		{
			scanPatternBuilder = new PumpProbePatternBuilder();
		}

		protected override IPatternSource GetScanPattern()
		{
			scanPatternBuilder.Clear();
			scanPatternBuilder.ShotSequence(
				0,
				(int)settings["sequenceLength"],
				(int)settings["padShots"],
				(int)settings["flashlampPulseInterval"],
				(int)settings["valvePulseLength"],
				(int)settings["valveToQ"],
				(int)settings["flashToQ"],
				(int)settings["aomOnStart"],
				(int)settings["aomOffStart"] - (int)settings["aomOnStart"],
				(int)settings["aomOffStart"] + (int)settings["aomOffDuration"], 
				(int)settings["aomOnDuration"] - ((int)settings["aomOffStart"] 
				- (int)settings["aomOnStart"]) - (int)settings["aomOffDuration"],
				(int)config.shotGathererPlugin.Settings["gateStartTime"],
				(int)settings["ttlSwitchPort"],
				(int)settings["ttlSwitchLine"]
				);

			scanPatternBuilder.BuildPattern(2 * ((int)settings["padShots"] + 1) * (int)settings["sequenceLength"]
				* (int)settings["flashlampPulseInterval"]);

			return scanPatternBuilder;
		}
	}		
}
