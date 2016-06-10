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
            settings["sequenceLength"] = 2;
            settings["switchLineDuration"] = 1000;
            settings["switchLineDelay"] = 0;
            settings["padStart"] = 0;
            settings["flashlampPulseLength"] = 100;
            settings["chirpStart"] = 100;
            settings["chirpDuration"] = 300;
		}

		protected override void DoAcquisitionStarting()
		{
			scanPatternBuilder = new PumpProbePatternBuilder();
		}

		protected override IPatternSource GetScanPattern()
		{
			scanPatternBuilder.Clear();
			scanPatternBuilder.ShotSequence(
                (int)settings["padStart"],
				(int)settings["sequenceLength"],
				(int)settings["padShots"],
                (int)settings["padStart"],
				(int)settings["flashlampPulseInterval"],
				(int)settings["valvePulseLength"],
				(int)settings["valveToQ"],
				(int)settings["flashToQ"],
                (int)settings["flashlampPulseLength"],
				(int)settings["aomOnStart"],
				(int)settings["aomOffStart"] - (int)settings["aomOnStart"],
				(int)settings["aomOffStart"] + (int)settings["aomOffDuration"], 
				(int)settings["aomOnDuration"] - ((int)settings["aomOffStart"] 
				- (int)settings["aomOnStart"]) - (int)settings["aomOffDuration"],
                GateStartTimePGUnits,
				(int)settings["ttlSwitchPort"],
				(int)settings["ttlSwitchLine"],
                (int)settings["switchLineDuration"],
                (int)settings["switchLineDelay"],
                (int)settings["chirpStart"],
                (int)settings["chirpDuration"],
                (bool)config.switchPlugin.Settings["switchActive"]
				);

			scanPatternBuilder.BuildPattern(2 * ((int)settings["padShots"] + 1) * (int)settings["sequenceLength"]
				* (int)settings["flashlampPulseInterval"]);

			return scanPatternBuilder;
		}
	}		
}
