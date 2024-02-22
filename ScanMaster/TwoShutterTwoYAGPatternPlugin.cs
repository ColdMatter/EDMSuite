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
	/// A plugin for measuring the velocity with two different wavelength ablation lasers. This pattern takes two on shots with the two different lasers and then 
	/// two off shots with the two lasers. 
	/// 
	/// Note carefully: the sequenceLength setting must be a multiple of 2 for the pattern to have the correct
	/// behaviour (because sequenceLength is the number of flashlamp pulse intervals in the sequence, and this 
	/// pattern requires a 4 shots per sequence, one for the ttl line high and one for the ttl line low, and two shots in each one with one yag and then with the other.).
	/// </summary>
	[Serializable]
	public class TwoShutterTwoYAGPatternPlugin : SupersonicPGPluginBase
	{
		[NonSerialized]
		private TwoShutterTwoYAGPatternBuilder scanPatternBuilder;
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
			settings["sequenceLength"] = 4;
			settings["switchLineDuration"] = 500000;
			settings["switchLineDelay"] = 0;
			settings["padStart"] = 0;
			settings["padShots"] = 0;
			settings["flashlampPulseLength"] = 100;
			settings["shutterPulseLength"] = 1000;
			settings["flashlampPulseInterval"] = 500000;
			settings["valvePulseLength"] = 350;
			settings["flashToQ"] = 135;
			settings["flashToQ2"] = 135;
			settings["shutterPulseLength"] = 2000;
			settings["shutteroffDelay"] = 480000;
			settings["shutter1offdelay"] = 480000;
			settings["shutterslowdelay"] = 10000;
			settings["ShutterslowPulseLength"] = 20000;
		}

		protected override void DoAcquisitionStarting()
		{
			scanPatternBuilder = new TwoShutterTwoYAGPatternBuilder();
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
				(int)settings["flashToQ2"],
				(int)settings["flashlampPulseLength"],
				(int)settings["shutterPulseLength"],
				GateStartTimePGUnits,
				(int)settings["ttlSwitchPort"],
				(int)settings["ttlSwitchLine"],
				(int)settings["switchLineDuration"],
				(int)settings["switchLineDelay"],
				(int)settings["shutteroffDelay"],
				(int)settings["shutter1offdelay"],
				(int)settings["shutterslowdelay"],
				(int)settings["ShutterslowPulseLength"],
				(bool)config.switchPlugin.Settings["switchActive"]
				);

			scanPatternBuilder.BuildPattern(((int)settings["padShots"] + 1) * (int)settings["sequenceLength"]
				* (int)settings["flashlampPulseInterval"]);

			return scanPatternBuilder;
		}
	}
}
