using System;
using System.Xml.Serialization;

using DAQ.Environment;
using DAQ.Pattern;

using ScanMaster.Acquire.Patterns;
using ScanMaster.Acquire.Plugin;


namespace ScanMaster.Acquire.Plugins
{
	/// <summary>
	/// Pattern plugin for running two Raman transitions from the same pair of beams.
	/// This pattern does not support switch scanning (yet).
	/// </summary>
	[Serializable]
	public class CommonRamanPatternPlugin : SupersonicPGPluginBase
	{

		[NonSerialized]
		private CommonRamanPatternBuilder scanPatternBuilder;

		protected override void InitialiseCustomSettings()
		{
			settings["ttlSwitchPort"] = 0;
			settings["ttlSwitchLine"] = 5;
			settings["rf1CentreTime"] = 1400;
			settings["rf1Length"] = 450;
			settings["fmCentreTime"] = 700;
			settings["fmLength"] = 650;
			settings["piFlipTime"] = 1100;
		}

		protected override void DoAcquisitionStarting()
		{
			scanPatternBuilder = new CommonRamanPatternBuilder();
		}

		protected override IPatternSource GetScanPattern()
		{
			// switch over to the scan pattern
			scanPatternBuilder = new CommonRamanPatternBuilder();
			// this is a bit of a hack. I think that the time ordering code in the pattern builder
			// should be improved.
			scanPatternBuilder.EnforceTimeOrdering = false;
			scanPatternBuilder.Clear();
			scanPatternBuilder.ShotSequence(
				(int)settings["padStart"],
				(int)settings["sequenceLength"],
				(int)settings["padShots"],
				(int)settings["flashlampPulseInterval"],
				(int)settings["valvePulseLength"],
				(int)settings["valveToQ"],
				(int)settings["flashToQ"],
				GateStartTimePGUnits,
				(int)settings["rf1CentreTime"],
				(int)settings["rf1Length"],
				(int)settings["fmCentreTime"],
				(int)settings["fmLength"],
				(int)settings["piFlipTime"]
				);
			scanPatternBuilder.BuildPattern(((int)settings["padShots"] + 1) * (int)settings["sequenceLength"]
				* (int)settings["flashlampPulseInterval"]);
			return scanPatternBuilder;
		}

	}
}
