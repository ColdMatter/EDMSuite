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
	public class LatticePumpProbePatternPlugin : SupersonicPGPluginBase
	{
		[NonSerialized]
		private LatticePumpProbePatternBuilder scanPatternBuilder;

		protected override void InitialiseCustomSettings()
		{
			settings["ttlSwitchPort"] = 0;
			settings["ttlSwitchLine"] = 0;
			settings["sequenceLength"] = 2;
			settings["switchLineDuration"] = 500000;
			settings["switchLineDelay"] = 0;
			settings["padStart"] = 11800;
			settings["padShots"] = 0;
			settings["flashlampPulseLength"] = 100;
			settings["shutterPulseLength"] = 1000;
			settings["flashlampPulseInterval"] = 500000;
			settings["valvePulseLength"] = 350;
			settings["flashToQ"] = 140;
			settings["shutterPulseLength"] = 2000;
			settings["shutteroffDelay"] = 450000;
			settings["shutter1offdelay"] = 450000;
			settings["shutterslowdelay"] = 10000;
			settings["DurationV0"] = 6000;
			settings["steve1delay"] = 0;
			settings["shutterV2delay"] = 0;
			settings["DurationV2"] = 5000;
			settings["DurationV1"] = 40000;
			settings["v3delaytime"] = 14000;
			settings["repumpDelay"] = 5000;
			settings["repumpDuration"] = 10000;
		}

		protected override void DoAcquisitionStarting()
		{
			scanPatternBuilder = new LatticePumpProbePatternBuilder();
		}

		protected override IPatternSource GetScanPattern()
		{
			scanPatternBuilder.Clear();
			int time = scanPatternBuilder.ShotSequence(
				(int)settings["padStart"],
				(int)settings["sequenceLength"],
				(int)settings["padShots"],
				(int)settings["padStart"],
				(int)settings["flashlampPulseInterval"],
				(int)settings["valvePulseLength"],
				(int)settings["valveToQ"],
				(int)settings["flashToQ"],
				(int)settings["flashlampPulseLength"],
				(int)settings["shutterPulseLength"],
				GateStartTimePGUnits,
				(int)settings["ttlSwitchPort"],
				(int)settings["ttlSwitchLine"],
				(int)settings["switchLineDuration"],
				(int)settings["shutteroffDelay"],
				(int)settings["shutterslowdelay"],
				(int)settings["DurationV0"],
				(int)settings["shutterV1delay"],
				(int)settings["shutterV2delay"],
				(int)settings["DurationV2"],
				(int)settings["DurationV1"],
				(bool)config.switchPlugin.Settings["switchActive"],
				(int)settings["switchLineDelay"],
				(int)settings["shutter1offdelay"],
				(int)settings["v3delaytime"],
				(int)settings["repumpDuration"],
				(int)settings["repumpDelay"]);
			/*
			scanPatternBuilder.BuildPattern(2 * ((int)settings["padShots"] + 1) * (int)settings["sequenceLength"]
				* (int)settings["flashlampPulseInterval"]);
			*/
			scanPatternBuilder.BuildPattern(time);


			return scanPatternBuilder;
		}
	}		
}
