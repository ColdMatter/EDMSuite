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
	public class DualCCDPatternPlugin : SupersonicPGPluginBase
	{
		[NonSerialized]
        private DualCCDPatternBuilder scanPatternBuilder;

		protected override void InitialiseCustomSettings()
		{
			settings["aomOnStart"] = 10;
            settings["valveToQ"] = 570;
            settings["flashToQ"] = 230;
			settings["aomOnDuration"] = 260000;
			settings["aomOffStart"] = 1000;
            settings["aom2OffStart"] = 140;
			settings["aomOffDuration"] = 250020;
			settings["ttlSwitchPort"] = 0;
			settings["ttlSwitchLine"] = 5;
            settings["sequenceLength"] = 4;
            settings["switchLineDuration"] = 360000;
            settings["switchLineDelay"] = -50000;
            settings["padStart"] = 50000;
            settings["flashlampPulseLength"] = 100;
            settings["chirpStart"] = 100;
            settings["chirpDuration"] = 300;
            settings["flashlampPulseInterval"] = 500000;
		}

		protected override void DoAcquisitionStarting()
		{
            scanPatternBuilder = new DualCCDPatternBuilder();
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
                (int)settings["aomOnStart"],                                                    //aomStart1
                (int)settings["aomOffStart"] - (int)settings["aomOnStart"],                     //aomDuration1
                (int)settings["aomOffStart"] + (int)settings["aomOffDuration"],                 //aomStart2
				(int)settings["aomOnDuration"] - ((int)settings["aomOffStart"]
                - (int)settings["aomOnStart"]) - (int)settings["aomOffDuration"],               //aomDuration2
                (int)settings["aomOffStart"] - (int)settings["aomOnStart"],                     //aom2Duration1
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
