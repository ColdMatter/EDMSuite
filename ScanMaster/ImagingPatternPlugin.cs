using System;

using System.Xml.Serialization;

using DAQ.Environment;
using DAQ.Pattern;

using ScanMaster.Acquire.Patterns;
using ScanMaster.Acquire.Plugin;

namespace ScanMaster.Acquire.Plugins
{
    /// <summary>
   // A plugin that does evrything the pump probe plugin does but also controls two digital lines called 'probe' and 'camera' to trigger and aquire images.
    /// </summary>
    [Serializable]
    public class ImagingPatternPlugin : SupersonicPGPluginBase
    {
        [NonSerialized]
        private ImagingPatternBuilder scanPatternBuilder;

        protected override void InitialiseCustomSettings()
        {
            settings["aomOnStart"] = 100;
            settings["aomOnDuration"] = 100;
            settings["aomOffStart"] = 140;
            settings["aomOffDuration"] = 20;
            settings["ttlSwitchPort"] = 0;
            settings["ttlSwitchLine"] = 5;
            settings["sequenceLength"] = 2;
            settings["switchLineDuration"] = 1000;
            settings["switchLineDelay"] = 0;

            settings["probeStart"] = 0;
            settings["probeDuration"] = 100;
            settings["shutterToProbeDelay"] = 50;
            settings["shutterDuration"] = 200;
        }

        protected override void DoAcquisitionStarting()
        {
            scanPatternBuilder = new ImagingPatternBuilder();
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
                (int)settings["aomOnStart"],
                (int)settings["aomOffStart"] - (int)settings["aomOnStart"],
                (int)settings["aomOffStart"] + (int)settings["aomOffDuration"],
                (int)settings["aomOnDuration"] - ((int)settings["aomOffStart"]
                - (int)settings["aomOnStart"]) - (int)settings["aomOffDuration"],
                (int)settings["probeStart"],
                (int)settings["probeDuration"],
                (int)settings["probeStart"]-(int)settings["shutterToProbeDelay"],
                (int)settings["shutterDuration"],
                GateStartTimePGUnits,
                (int)settings["ttlSwitchPort"],
                (int)settings["ttlSwitchLine"],
                (int)settings["switchLineDuration"],
                (int)settings["switchLineDelay"],
                (bool)config.switchPlugin.Settings["switchActive"]
                );

            scanPatternBuilder.BuildPattern(2 * ((int)settings["padShots"] + 1) * (int)settings["sequenceLength"]
                * (int)settings["flashlampPulseInterval"]);

            return scanPatternBuilder;
        }
    }
}

