using System;
using System.Collections.Generic;
using System.Text;
using DAQ.Pattern;
using ScanMaster.Acquire.Patterns;
using ScanMaster.Acquire.Plugin;

namespace ScanMaster.Acquire.Plugins
{
	/// <summary>
	/// A plugin for the AG guide. Read the GuidePatternBuilder summary notes
	/// </summary>
	[Serializable]
	public class GuidePatternPlugin : SupersonicPGPluginBase
	{
		[NonSerialized]
		private GuidePatternBuilder scanPatternBuilder;

		protected override void InitialiseCustomSettings()
		{
			settings["delayToGuide"] = 0;
			settings["numberOfLenses"] = 10;
			settings["lensSwitchPeriod"] = 50;
			settings["guideDcDuration"] = 2500;
			settings["sequenceLength"] = 2;
		}

		protected override void DoAcquisitionStarting()
		{
			scanPatternBuilder = new GuidePatternBuilder();
		}

		protected override IPatternSource GetScanPattern()
		{
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
				(int)settings["delayToGuide"],
				(int)settings["lensSwitchPeriod"],
				(int)settings["numberOfLenses"], 
				(int)settings["guideDcDuration"],
				(bool)config.switchPlugin.Settings["switchActive"]
				);

			scanPatternBuilder.BuildPattern(2*((int)settings["padShots"] + 1) * (int)settings["sequenceLength"]
				* (int)settings["flashlampPulseInterval"]);

			return scanPatternBuilder;
		}

	}
}
