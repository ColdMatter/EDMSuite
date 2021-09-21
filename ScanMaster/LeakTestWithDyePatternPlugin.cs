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
    public class LeakTestWithDyePatternPlugin : SupersonicPGPluginBase
    {
        [NonSerialized]
        private LeakTestWithDyePatternBuilder scanPatternBuilder;

        protected override void InitialiseCustomSettings()
        {
            settings["valveToQ"] = 0;
            settings["flashToQ"] = 150;
            settings["ccd1Start1"] = 1000;
            settings["ccd1Start2"] = 101000;
            settings["ccd2Start1"] = 1000;
            settings["ccd2Start2"] = 101000;
            settings["ttl1Delay"] = 5000;
            settings["ttlSwitchPort"] = 0;
            settings["ttlSwitchLine"] = 5;
            settings["sequenceLength"] = 4;
            settings["switchLineDuration"] = 25000;
            settings["switchLineDelay"] = -20000;
            settings["padStart"] = 20000;
            settings["flashlampPulseLength"] = 100;
            settings["flashlampPulseInterval"] = 200000;
            settings["dummy"] = "specifically designed for the YbF leak test 2021";
        }

        protected override void DoAcquisitionStarting()
        {
            scanPatternBuilder = new LeakTestWithDyePatternBuilder();
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
                (int)settings["ccd1Start1"],
                (int)settings["ccd1Start2"],
                (int)settings["ccd2Start1"],
                (int)settings["ccd2Start2"],
                GateStartTimePGUnits,
                (int)settings["ttlSwitchPort"],
                (int)settings["ttlSwitchLine"],
                (int)settings["switchLineDuration"],
                (int)settings["switchLineDelay"],
                (int)settings["ttl1Delay"],
                (bool)config.switchPlugin.Settings["switchActive"]
                );

            scanPatternBuilder.BuildPattern(2 * ((int)settings["padShots"] + 1) * (int)settings["sequenceLength"]
                * (int)settings["flashlampPulseInterval"]);

            return scanPatternBuilder;
        }
    }
}
