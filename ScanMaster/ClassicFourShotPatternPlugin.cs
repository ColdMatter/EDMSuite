using System;

using System.Xml.Serialization;

using DAQ.Environment;
using DAQ.Pattern;

using ScanMaster.Acquire.Patterns;
using ScanMaster.Acquire.Plugin;

namespace ScanMaster.Acquire.Plugins
{
	/// <summary>
	/// A plugin for pump-probe experiments where the pump is switched on and off using an aom.
    /// This plugin allows for switching of a digital line on successive shots of the scan. The digital line that
    /// is switched is a setting. This switching is only activated when the SwitchPlugin's switchActive setting is
    /// true (see the PumpProbePatternBuilder). When the switch is not activated, the ttl line will always be high.
    /// 
    /// Note carefully: the sequenceLength setting must be a multiple of 2 for the pattern to have the correct
    /// behaviour (because sequenceLength is the number of flashlamp pulse intervals in the sequence, and this 
    /// pattern requires a minimum of 2 shots per sequence, one for the ttl line high and one for the ttl line low).
	/// </summary>
	[Serializable]
	public class ClassicFourShotPatternPlugin : SupersonicPGPluginBase
	{
		[NonSerialized]
		private ClassicFourShotPatternBuilder scanPatternBuilder;

		protected override void InitialiseCustomSettings()
		{
			settings["sequenceLength"] = 100;
			settings["valve"] = 20000;
			settings["shutter1Delay"] = 11600;
			settings["shutter1Duration"] = 20000;
			settings["shutter2Delay"] = 11600;
			settings["shutter2Duration"] = 20000;
			settings["modulation"] = true;
		}

		protected override void DoAcquisitionStarting()
		{
			scanPatternBuilder = new ClassicFourShotPatternBuilder();
		}

		protected override IPatternSource GetScanPattern()
		{
			scanPatternBuilder.Clear();
			int time = scanPatternBuilder.ShotSequence(
				(int)settings["padStart"],
				(int)settings["sequenceLength"],
				(int)settings["padShots"],
				(int)settings["flashlampPulseInterval"],
				(int)settings["valvePulseLength"],
				(int)settings["valveToQ"],
				(int)settings["flashToQ"],
				GateStartTimePGUnits,
				(bool)settings["modulation"],
				(int)settings["valve"],
				(int)settings["shutter1Delay"],
				(int)settings["shutter1Duration"],
				(int)settings["shutter2Delay"],
				(int)settings["shutter2Duration"]
				);
			/*
			scanPatternBuilder.BuildPattern(2 * ((int)settings["padShots"] + 1) * (int)settings["sequenceLength"]
				* (int)settings["flashlampPulseInterval"]);
			*/
			scanPatternBuilder.BuildPattern(time);


			return scanPatternBuilder;
		}
	}		
}
