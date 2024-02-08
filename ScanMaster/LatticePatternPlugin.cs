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
	public class LatticePatternPlugin : SupersonicPGPluginBase
	{
		[NonSerialized]
		private LatticePatternBuilder scanPatternBuilder;

		protected override void InitialiseCustomSettings()
		{
            settings["sequenceLength"] = 2;
			settings["CameraTrigger"] = 5000; //trigger for the image of molecule, default value is 5ms after the startTrigger
			settings["BgTrigger"] = 250000; // trigger for the light background, default value is 250ms after the startTrigger which is fired at 2Hz. 
			settings["shutterPulseLength"] =1000;
		}

		protected override void DoAcquisitionStarting()
		{
            scanPatternBuilder = new LatticePatternBuilder();
		}

		protected override IPatternSource GetScanPattern()
		{
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
				(int)settings["CameraTrigger"],// a camera trigger
				(int)settings["BgTrigger"],//15Sept2023 Guanchen added a trigger for the light background image
				(int)settings["shutterPulseLength"]
				);

			scanPatternBuilder.BuildPattern(((int)settings["padShots"] + 1) * (int)settings["sequenceLength"]
				* (int)settings["flashlampPulseInterval"]);

			return scanPatternBuilder;
		}
	}		
}
