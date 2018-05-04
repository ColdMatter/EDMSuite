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
	public class SuperPumpingPatternPlugin : SupersonicPGPluginBase
	{

		[NonSerialized]
		private  SuperPumpingPatternBuilder scanPatternBuilder;

		protected override void InitialiseCustomSettings()
		{
        	settings["ttlSwitchPort"] = 0;
			settings["ttlSwitchLine"] = 5;
            settings["pumprfCentreTime"]= 470;
            settings["pumprfLength"] =250;
            settings["pumpmwCentreTime"] = 470;
            settings["pumpmwLength"] = 200;
            settings["bottomProbemwCentreTime"] = 2450; 
            settings["bottomProbemwLength"] = 200;
            settings["topProbemwCentreTime"] = 2700; 
            settings["topProbemwLength"] = 200;
            settings["pulsedrfActive"] = false;
		}

		protected override void DoAcquisitionStarting()
		{
            scanPatternBuilder = new SuperPumpingPatternBuilder();
		}

		protected override IPatternSource GetScanPattern()
		{
			// switch over to the scan pattern
            scanPatternBuilder = new SuperPumpingPatternBuilder();
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
                (bool)settings["pulsedrfActive"],
                (int)settings["pumprfCentreTime"],
                (int)settings["pumprfLength"], 
                (int)settings["pumpmwCentreTime"],
                (int)settings["pumpmwLength"],
                (int)settings["bottomProbemwCentreTime"], 
                (int)settings["bottomProbemwLength"], 
                (int)settings["topProbemwCentreTime"], 
                (int)settings["topProbemwLength"],
                (bool)config.switchPlugin.Settings["switchActive"]
				);
            scanPatternBuilder.BuildPattern(2 * ((int)settings["padShots"] + 1) * (int)settings["sequenceLength"]
                * (int)settings["flashlampPulseInterval"]);
			return scanPatternBuilder;
		}

	}
}
