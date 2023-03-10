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
    /// pattern requires a minimum of 2 shots per sequence).
    /// </summary>
    [Serializable]
    public class CaFBECPatternPlugin : SupersonicPGPluginBase
    {
        [NonSerialized]
        private CaFBECPatternBuilder scanPatternBuilder;

        protected override void InitialiseCustomSettings()
        {

            settings["ttlSwitchPort"] = 1;
            settings["ttlSwitchLine"] = 5;
            settings["switchLineDuration"] = 100;
            settings["flashlampPulseLength"] = 100;
            settings["flashlampPulseInterval"] = 200000;
            settings["sequenceLength"] = 2;
            settings["padStart"] = 20000;
            settings["switchLineDuration"] = 25000;
            settings["switchLineDelay"] = -20000;

            settings["chirpStart"] = 100;
            settings["chirpDuration"] = 300;

            settings["TTL2StartTimes"] = "10000";
            settings["TTL2Durations"] = "5000";
            settings["TTL2Repetitions"] = "1";
        }

        protected override void DoAcquisitionStarting()
        {
            scanPatternBuilder = new CaFBECPatternBuilder();
        }

        private int[] convertStringListToIntArray(string stringList)
        {
            string[] stringArray = stringList.Split(new char[] { ',' });
            int[] intArray = new int[stringArray.Length];
            for (int i = 0; i < stringArray.Length; i++)
            {
                intArray[i] = Convert.ToInt32(stringArray[i]);
            }
            return intArray;
        }

        protected override IPatternSource GetScanPattern()
        {
            int[] ttl1StartTimes = convertStringListToIntArray((string)settings["TTL1StartTimes"]);
            int[] ttl1Durations = convertStringListToIntArray((string)settings["TTL1Durations"]);
            int[] ttl1Repetitions = convertStringListToIntArray((string)settings["TTL1Repetitions"]);

            int[] ttl2StartTimes = convertStringListToIntArray((string)settings["TTL2StartTimes"]);
            int[] ttl2Durations = convertStringListToIntArray((string)settings["TTL2Durations"]);
            int[] ttl2Repetitions = convertStringListToIntArray((string)settings["TTL2Repetitions"]);


            scanPatternBuilder.Clear();
            scanPatternBuilder.ShotSequence(
                (int)settings["padStart"],
                (int)settings["sequenceLength"],
                (int)settings["flashlampPulseInterval"],
                (int)settings["flashToQ"],
                (int)settings["flashlampPulseLength"],
                ttl1StartTimes, ttl1Durations, ttl1Repetitions,
                ttl2StartTimes, ttl2Durations, ttl2Repetitions,
                GateStartTimePGUnits,
                (int)settings["ttlSwitchPort"],
                (int)settings["ttlSwitchLine"],
                (int)settings["switchLineDuration"],
                (int)settings["switchLineDelay"],
                (int)settings["chirpStart"],
                (int)settings["chirpDuration"],
                (bool)config.switchPlugin.Settings["switchActive"]
                );

            scanPatternBuilder.BuildPattern((int)settings["sequenceLength"] * (int)settings["flashlampPulseInterval"]);

            return scanPatternBuilder;
        }
    }
}

