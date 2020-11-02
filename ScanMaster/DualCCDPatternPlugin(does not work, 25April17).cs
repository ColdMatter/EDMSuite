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
            settings["valveToQ"] = 570;
            settings["flashToQ"] = 230;
            settings["ccd1Start"] = 10;
			settings["ccd1Duration"] = 10000;
            settings["ccd2Start"] = 10;
            settings["ccd2Duration"] = 10000;
			settings["ttlSwitchPort"] = 0;
			settings["ttlSwitchLine"] = 5;
            settings["sequenceLength"] = 2;
            settings["switchLineDuration"] = 360000;
            settings["switchLineDelay"] = -50000;
            settings["padStart"] = 50000;
            settings["flashlampPulseLength"] = 100;
            settings["flashlampPulseInterval"] = 500000;
		}

		protected override void DoAcquisitionStarting()
		{
            scanPatternBuilder = new DualCCDPatternBuilder();
		}

		protected override IPatternSource GetScanPattern()
		{
            scanPatternBuilder.EnforceTimeOrdering = false;
			scanPatternBuilder.Clear();
			scanPatternBuilder.ShotSequence(
                (int)settings["padStart"],                  //startTime
                (int)settings["sequenceLength"],            //numberOfOnOffShots
                (int)settings["padShots"],                  //padShots
                (int)settings["padStart"],                  //padStart
                (int)settings["flashlampPulseInterval"],    //flashlampPulseInterval
				(int)settings["valvePulseLength"],          //valvePulseLength
                (int)settings["valveToQ"],                  //valveToQ
                (int)settings["flashToQ"],                  //flashToQ
                (int)settings["flashlampPulseLength"],      //flashlampPulseLength
                (int)settings["ccd1Start"],                 //ccd1Start
                (int)settings["ccd1Duration"],              //ccd1Duration
                (int)settings["ccd2Start"],                 //ccd2Start
				(int)settings["ccd2Duration"],              //ccd2Duration
                GateStartTimePGUnits,
				(int)settings["ttlSwitchPort"],
				(int)settings["ttlSwitchLine"],
                (int)settings["switchLineDuration"],
                (int)settings["switchLineDelay"],
                (bool)config.switchPlugin.Settings["switchActive"]
				);

			scanPatternBuilder.BuildPattern(8 * ((int)settings["padShots"] + 1) * (int)settings["sequenceLength"]
				* (int)settings["flashlampPulseInterval"]);

			return scanPatternBuilder;
		}
	}		
}
