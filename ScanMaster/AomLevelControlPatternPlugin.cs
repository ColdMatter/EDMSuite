using System;

using System.Xml.Serialization;

using DAQ.Environment;
using DAQ.Pattern;

using ScanMaster.Acquire.Patterns;
using ScanMaster.Acquire.Plugin;

namespace ScanMaster.Acquire.Plugins
{
    /// <summary>
    /// Plugin for controlling the crazy box that accepts two ttl pulses and generates 4 possible analog output levels depending on their states.
    /// There are two ttl lines, and each one is pulsed on twice. The settings are ttlXStartY and ttlXDurationY (X=1,2; Y=1,2).
    /// </summary>
    [Serializable]
    public class AomLevelControlPatternPlugin : SupersonicPGPluginBase
    {
        [NonSerialized]
        private AomLevelControlPatternBuilder scanPatternBuilder;

        protected override void InitialiseCustomSettings()
        {
            settings["ttl1Start1"] = 300;
            settings["ttl1Duration1"] = 200;
            settings["ttl1Start2"] = 1200;
            settings["ttl1Duration2"] = 200;
            settings["ttl2Start1"] = 300;
            settings["ttl2Duration1"] = 200;
            settings["ttl2Start2"] = 1200;
            settings["ttl2Duration2"] = 200;
        }

        protected override void DoAcquisitionStarting()
        {
            scanPatternBuilder = new AomLevelControlPatternBuilder();
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
                (int)settings["ttl1Start1"],
                (int)settings["ttl1Duration1"],
                (int)settings["ttl1Start2"],
                (int)settings["ttl1Duration2"],
                (int)settings["ttl2Start1"],
                (int)settings["ttl2Duration1"],
                (int)settings["ttl2Start2"],
                (int)settings["ttl2Duration2"],
                GateStartTimePGUnits
                );

            scanPatternBuilder.BuildPattern(((int)settings["padShots"] + 1) * (int)settings["sequenceLength"]
                * (int)settings["flashlampPulseInterval"]);

            return scanPatternBuilder;
        }
    }
}
