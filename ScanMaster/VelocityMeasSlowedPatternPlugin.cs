using System;

using System.Xml.Serialization;

using DAQ.Environment;
using DAQ.Pattern;
using DAQ.HAL;

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
	public class VelocityMeasSlowedPatternPlugin : SupersonicPGPluginBase
	{
		[NonSerialized]
		private VelocityMeasSlowedPatternBuilder scanPatternBuilder;
		[NonSerialized]
		private FlashlampPatternBuilder flashlampPatternBuilder;
		[NonSerialized]
		private int patternLength;
		[NonSerialized]
		private DAQMxPatternGenerator pg;

		protected override void InitialiseCustomSettings()
		{
			settings["ttlSwitchPort"] = 0;
			settings["sequenceLength"] = 4;
			settings["ttlSwitchLine"] = 0;
			settings["switchLineDuration"] = 500000;
			settings["Probe1ShutterDelay"] = 0;
			settings["padStart"] = 11800;
			settings["flashlampPulseLength"] = 100;
			settings["shutterPulseLength"] = 1000;
			settings["flashlampPulseInterval"] = 500000;
			settings["valvePulseLength"] = 350;
			settings["flashToQ"] = 140;
			settings["ShutterPulseLength"] = 2000;
			settings["shutter1offdelay"] = 900000;
			settings["shutterslowdelay"] = 21800;
			settings["shutterV1delay"] = 10000;
			settings["shutterV2delay"] = 10000;
			settings["DurationV0"] = 100;
			settings["DurationV1"] = 40000;
			settings["DurationV2"] = 40000;
			settings["Probe2ShutterDelay"] = 0;
		}

		protected override void DoAcquisitionStarting()
		{
			scanPatternBuilder = new VelocityMeasSlowedPatternBuilder();
		}

		protected override IPatternSource GetScanPattern()
		{
			scanPatternBuilder.Clear();
			scanPatternBuilder.ShotSequence(
				(int)settings["padStart"],
				(int)settings["sequenceLength"],
				(int)settings["padStart"],
				(int)settings["flashlampPulseInterval"],
				(int)settings["valvePulseLength"],
				(int)settings["valveToQ"],
				(int)settings["flashToQ"],
				(int)settings["flashlampPulseLength"],
				(int)settings["ShutterPulseLength"],
				GateStartTimePGUnits,
				(int)settings["ttlSwitchPort"],
				(int)settings["ttlSwitchLine"],
				(int)settings["switchLineDuration"],
				(int)settings["Probe1ShutterDelay"],
				(int)settings["Probe2ShutterDelay"],
				(int)settings["shutter1offdelay"],
				(int)settings["shutterslowdelay"],
				(int)settings["shutterV1delay"],
				(int)settings["shutterV2delay"],
				(int)settings["DurationV0"],
				(int)settings["DurationV1"],
				(int)settings["DurationV2"],
				(bool)config.switchPlugin.Settings["switchActive"]
				);

			scanPatternBuilder.BuildPattern((1) * (int)settings["sequenceLength"]
				* (int)settings["flashlampPulseInterval"]);

			return scanPatternBuilder;
		}
	}
}
