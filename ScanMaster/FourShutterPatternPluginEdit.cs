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
	public class FourShutterPatternPluginEdit : SupersonicPGPluginBase
	{
		[NonSerialized]
		private FourShutterPatternBuilderEdit scanPatternBuilder;
		[NonSerialized]
		private FlashlampPatternBuilder flashlampPatternBuilder;
		[NonSerialized]
		private int patternLength;
		[NonSerialized]
		private DAQMxPatternGenerator pg;

		protected override void InitialiseCustomSettings()
		{
			settings["ttlSwitchPort"] = 0;
			settings["ttlSwitchLine"] = 0;
			settings["sequenceLength"] = 2;
			settings["switchLineDuration"] = 500000;
			settings["switchLineDelay"] = 0;
			settings["padStart"] = 0;
			settings["padShots"] = 0;
			settings["flashlampPulseLength"] = 100;
			settings["shutterPulseLength"] = 1000;
			settings["flashlampPulseInterval"] = 500000;
			settings["valvePulseLength"] = 350;
			settings["flashToQ"] = 140;
			settings["shutterPulseLength"] = 2000;
			settings["shutteroffDelay"] = 480000;
			settings["shutter1offdelay"] = 480000;
			settings["shutterslowdelay"] = 10000;
			settings["DurationV0"] = 20000;
			settings["shutterV1delay"] = 6000;
			settings["shutterV2delay"] = 6000;
			settings["V2OnTime"] = 5000;
			settings["DurationV1"] = 20000;
		}

		protected override void DoAcquisitionStarting()
		{
			scanPatternBuilder = new FourShutterPatternBuilderEdit();
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
				(int)settings["V2OnTime"],
				(int)settings["DurationV1"],
				(bool)config.switchPlugin.Settings["switchActive"]
				);

			scanPatternBuilder.BuildPattern(((int)settings["padShots"] + 1) * (int)settings["sequenceLength"]
				* (int)settings["flashlampPulseInterval"]);

			return scanPatternBuilder;
		}
	}
}
