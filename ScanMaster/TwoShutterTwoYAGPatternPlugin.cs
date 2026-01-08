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
			settings["shutterslowdelay"] = 21800;
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
			settings["v0chirpTriggerDelay"] = 10000;
			settings["v0chirpTriggerDuration"] = 5000;//10Sept2024, modified to access the chirpTriggerDuration, which is the duration by which the TCL is blocked. N.B. The actual chirp duration is set by Moku:Go GUI. (search liquid instrument on the desktop)
			settings["cameraTriggerDelay"] = 30000;
			settings["cameraBackgroundDelay"] = 70000;
			settings["offShotSlowingDuration"] = 10;
		}

		protected override void DoAcquisitionStarting()
		{
			scanPatternBuilder = new TwoShutterTwoYAGPatternBuilder();
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
				(int)settings["offShotSlowingDuration"]);

			scanPatternBuilder.BuildPattern(time);

			return scanPatternBuilder;
		}
	}
}
