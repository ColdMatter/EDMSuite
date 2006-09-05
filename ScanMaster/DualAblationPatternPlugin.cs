using System;

using System.Xml.Serialization;

using DAQ.Environment;
using DAQ.Pattern;

using ScanMaster.Acquire.Patterns;
using ScanMaster.Acquire.Plugin;

namespace ScanMaster.Acquire.Plugins
{
    /// <summary>
    /// A plugin for dual-laser-ablation experiments. There are two ablation lasers, each requiring their flashlamps
    /// and Q-switches to be triggered independently. When the SwitchPlugin's switchActive setting is
    /// true, the plugin will modulate between firing both lasers and firing only one. When the switch is
    /// not activated, both lasers will fire on every shot.
    /// 
    /// The sequenceLength setting must be a multiple of 2 for the pattern to have the correct
    /// behaviour (because sequenceLength is the number of flashlamp pulse intervals in the sequence, and this 
    /// pattern requires a minimum of 2 shots per sequence).
    /// </summary>
    [Serializable]
    public class DualAblationPatternPlugin : SupersonicPGPluginBase
    {
        [NonSerialized]
        private DualAblationPatternBuilder scanPatternBuilder;

        protected override void InitialiseCustomSettings()
        {

            settings["flash2ToQ2"] = 150;
            settings["qToQ2"] = 10;
            settings["sequenceLength"] = 2;
        }

        protected override void DoAcquisitionStarting()
        {
            scanPatternBuilder = new DualAblationPatternBuilder();
        }

        protected override IPatternSource GetScanPattern()
        {
            scanPatternBuilder.Clear();
            scanPatternBuilder.ShotSequence(
                0,
                (int)settings["sequenceLength"],
                (int)settings["flashlampPulseInterval"],
                (int)settings["valvePulseLength"],
                (int)settings["valveToQ"],
                (int)settings["flashToQ"],
                (int)settings["flash2ToQ2"],
                (int)settings["qToQ2"],
                (int)config.shotGathererPlugin.Settings["gateStartTime"],
                (bool)config.switchPlugin.Settings["switchActive"]
                );

            scanPatternBuilder.BuildPattern(2 * (int)settings["sequenceLength"]
                * (int)settings["flashlampPulseInterval"]);

            return scanPatternBuilder;
        }
    }
}
