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
	public class SuperPumpingPulsedRFScanPatternPlugin : SupersonicPGPluginBase
	{

		[NonSerialized]
		private  SuperPumpingPulsedRFScanPatternBuilder scanPatternBuilder;

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
            settings["attCentreTime"] = 700;
            settings["attLength"] = 700;
			settings["piFlipCentreTime"] = 1600;
			settings["piFlipLength"] = 50;
            settings["scramblerCentreTime"] = 1400;
            settings["scramblerLength"] = 200;
            settings["rf1BlankingCentreTime"] = 1400;
            settings["rf1BlankingLength"] = 500;
            settings["rf2BlankingCentreTime"] =  700;
            settings["rf2BlankingLength"] = 500;
            settings["pumprfCentreTime"]= 470;
            settings["pumprfLength"] =250;
            settings["pumpmwCentreTime"] = 470;
            settings["pumpmwLength"] = 200;
            settings["bottomProbemwCentreTime"] = 2450; 
            settings["bottomProbemwLength"] = 200;
            settings["topProbemwCentreTime"] = 2700; 
            settings["topProbemwLength"]= 200;
			settings["bLength"] = 500;
			settings["bCentreTime"] = 1300;
		}

		protected override void DoAcquisitionStarting()
		{
            scanPatternBuilder = new SuperPumpingPulsedRFScanPatternBuilder();
		}

		protected override IPatternSource GetScanPattern()
		{
			// switch over to the scan pattern
            scanPatternBuilder = new SuperPumpingPulsedRFScanPatternBuilder();
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
				(int)settings["rf2CentreTime"],
				(int)settings["rf2Length"],
				(int)settings["piFlipCentreTime"],
				(int)settings["piFlipLength"],
				(int)settings["fmCentreTime"],
				(int)settings["fmLength"],
                (int)settings["attCentreTime"],
                (int)settings["attLength"],
                (int)settings["scramblerCentreTime"],
                (int)settings["scramblerLength"],
                (int)settings["rf1BlankingCentreTime"],
                (int)settings["rf1BlankingLength"],
                (int)settings["rf2BlankingCentreTime"],
                (int)settings["rf2BlankingLength"],
                (int)settings["pumprfCentreTime"],
                (int)settings["pumprfLength"], 
                (int)settings["pumpmwCentreTime"],
                (int)settings["pumpmwLength"],
                (int)settings["bottomProbemwCentreTime"], 
                (int)settings["bottomProbemwLength"], 
                (int)settings["topProbemwCentreTime"], 
                (int)settings["topProbemwLength"],
				(int)settings["bCentreTime"],
				(int)settings["bLength"],
                (bool)config.switchPlugin.Settings["switchActive"]
				);
            scanPatternBuilder.BuildPattern(2 * ((int)settings["padShots"] + 1) * (int)settings["sequenceLength"]
                * (int)settings["flashlampPulseInterval"]);
			return scanPatternBuilder;
		}

	}
}
