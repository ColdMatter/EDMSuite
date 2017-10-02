using System;

using System.Xml.Serialization;

using DAQ.Environment;
using DAQ.Pattern;

using ScanMaster.Acquire.Patterns;
using ScanMaster.Acquire.Plugin;

namespace ScanMaster.Acquire.Plugins
{
    /// <summary>
    /// A plugin for experiments where an aom is switched on and off. If modulation is true,
    /// the pattern consists of two shots, one that has aom settings A, and the other having aom settings B.
    /// If modulation is false, the aom will always have settings A.
    ///
    /// 
    /// Note carefully: the sequenceLength setting must be a multiple of 2 for the pattern to have the correct
    /// behaviour (because sequenceLength is the number of flashlamp pulse intervals in the sequence, and this 
    /// pattern requires a minimum of 2 shots per sequence, one for the ttl line high and one for the ttl line low).
    /// </summary>
    [Serializable]
    public class DualAomPatternPlugin : SupersonicPGPluginBase
    {
        [NonSerialized]
        private DualAomPatternBuilder scanPatternBuilder;

        protected override void InitialiseCustomSettings()
        {
            settings["aomOnStartA"] = 10;
            settings["aomOnDurationA"] = 5500;
            settings["aomOffStartA"] = 500;
            settings["aomOffDurationA"] = 5000;
            settings["aomOnStartB"] = 1000;
            settings["aomOnDurationB"] = 5500;
            settings["aomOffStartB"] = 1500;
            settings["aomOffDurationB"] = 5000;
            settings["sequenceLength"] = 2;

        }

        protected override void DoAcquisitionStarting()
        {
            scanPatternBuilder = new DualAomPatternBuilder();
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
                (int)settings["aomOnStartA"],
                (int)settings["aomOffStartA"] - (int)settings["aomOnStartA"],
                (int)settings["aomOffStartA"] + (int)settings["aomOffDurationA"],
                (int)settings["aomOnDurationA"] - ((int)settings["aomOffStartA"]
                - (int)settings["aomOnStartA"]) - (int)settings["aomOffDurationA"],
                (int)settings["aomOnStartB"],
                (int)settings["aomOffStartB"] - (int)settings["aomOnStartB"],
                (int)settings["aomOffStartB"] + (int)settings["aomOffDurationB"],
                (int)settings["aomOnDurationB"] - ((int)settings["aomOffStartB"]
                - (int)settings["aomOnStartB"]) - (int)settings["aomOffDurationB"],
                GateStartTimePGUnits,
                (bool)config.switchPlugin.Settings["switchActive"]
                );

            scanPatternBuilder.BuildPattern(2 * ((int)settings["padShots"] + 1) * (int)settings["sequenceLength"]
                * (int)settings["flashlampPulseInterval"]);

            return scanPatternBuilder;
        }
    }
}
