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
	public class LatticePnPModYAGPatternPlugin : SupersonicPGPluginBase
	{
		[NonSerialized]
		private LatticePnPModYAGPatternBuilder scanPatternBuilder;

		protected override void InitialiseCustomSettings()
		{
			settings["ttlSwitchPort"] = 0;
			settings["ttlSwitchLine"] = 0;
			settings["sequenceLength"] = 2;
			settings["switchLineDuration"] = 0;
			settings["switchLineDelay"] = 0;
			settings["padStart"] = 0;
			settings["padShots"] = 0;
			settings["valveToQ"] = 570;
			settings["flashlampPulseLength"] = 100;
			settings["shutterPulseLength"] = 1000;
			settings["flashlampPulseInterval"] = 250000;//Quite important from other profiles as we take off shot between yag pulses at 4 Hz
			settings["valvePulseLength"] = 350;
			settings["flashToQ"] = 140;
			settings["shutterPulseLength"] = 2000;
			settings["shutteroffDelay"] = 0;
			settings["shutter1offdelay"] = 0;
			settings["shutterslowdelay"] = 10000;
			settings["DurationV0"] = 4000;
			settings["steve1delay"] = 0;
			settings["DurationV2"] = 0;
			settings["DurationV1"] = 0;
			settings["v3delaytime"] = 0;
			settings["repumpDelay"] = 9600;
			settings["repumpDuration"] = 4000;
			settings["shutterV1delay"] = 0;
			settings["shutterV2delay"] = 0;
			settings["vacShutterDelay"] = 0;
			settings["vacShutterDuration"] = 1000;
			settings["v0chirpTriggerDelay"] = 3500;
			settings["v0chirpTriggerDuration"] = 5000;//10Sept2024, modified to access the chirpTriggerDuration, which is the duration by which the TCL is blocked. N.B. The actual chirp duration is set by Moku:Go GUI. (search liquid instrument on the desktop)
			settings["cameraTriggerDelay"] = 30000;
			settings["cameraBackgroundDelay"] = 70000;
			settings["offShotSlowingDuration"] = 10;
			settings["v2OffDupoint"] = 0;
		}

		protected override void DoAcquisitionStarting()
		{
			scanPatternBuilder = new LatticePnPModYAGPatternBuilder();
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
				(int)settings["repumpDelay"],
				(int)settings["vacShutterDelay"],
				(int)settings["vacShutterDuration"],
				(int)settings["v0chirpTriggerDelay"],
				(int)settings["v0chirpTriggerDuration"],
				(int)settings["cameraTriggerDelay"],
				(int)settings["cameraBackgroundDelay"],
				(int)settings["offShotSlowingDuration"],
				(int)settings["v2OffDupoint"]);
			/*
			scanPatternBuilder.BuildPattern(2 * ((int)settings["padShots"] + 1) * (int)settings["sequenceLength"]
				* (int)settings["flashlampPulseInterval"]);
			*/
			scanPatternBuilder.BuildPattern(time);


			return scanPatternBuilder;
		}
	}		
}
