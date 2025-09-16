using System;

using System.Xml.Serialization;

using DAQ.Environment;
using DAQ.Pattern;

using ScanMaster.Acquire.Patterns;
using ScanMaster.Acquire.Plugin;


namespace ScanMaster.Acquire.Plugins
{
    /// <summary>
    /// This plugin should allow more flexibility to generate a pattern.
    /// A sequence of N shots is generated and each of these N shots can be a different sequence.
    /// If the 'sequenceLength' is even and the 'modulation' is active, it will generate the OnOffShots (as the PumpProbe plugin for example) (for cooling On/Off for example).
    /// If the 'sequenceLength' is a multiple of 4, it will also generates a 'probe' pulse (2 shots high, 2 shots low).
    /// Finally TTLs 1 to 6 can generate any sequence of pulses.
    /// </summary>
    [Serializable]
    public class NshotsPatternPluginCCD : SupersonicPGPluginBase 
    {
        [NonSerialized]
        private NshotsPatternBuilderCCD scanPatternBuilder;


        protected override void InitialiseCustomSettings()
        {
            settings["valveToQ"] = 0;
            settings["valvePulseLength"] = 0;
            settings["flashToQ"] = 150;
            settings["ccd1Start1"] = 1000;
            settings["ccd1Start2"] = 101000;
            settings["ccd2Start1"] = 1000;
            settings["ccd2Start2"] = 101000;
            settings["ttlSwitchPort"] = 0;
            settings["ttlSwitchLine"] = 5;
            settings["sequenceLength"] = 4;
            settings["switchLineDuration"] = 25000;
            settings["switchLineDelay"] = -20000;
            settings["padStart"] = 20000;
            settings["flashlampPulseLength"] = 100;
            settings["flashlampPulseInterval"] = 200000;

            settings["TTL1StartTimes"] = "10000";
            settings["TTL1Durations"] = "5000";
            settings["TTL1Repetitions"] = "1";

            settings["TTL2StartTimes"] = "10000";
            settings["TTL2Durations"] = "5000";
            settings["TTL2Repetitions"] = "1";

            settings["TTL3StartTimes"] = "10000";
            settings["TTL3Durations"] = "5000";
            settings["TTL3Repetitions"] = "1";

            settings["TTL4StartTimes"] = "10000";
            settings["TTL4Durations"] = "5000";
            settings["TTL4Repetitions"] = "1";

            settings["TTL5StartTimes"] = "10000";
            settings["TTL5Durations"] = "5000";
            settings["TTL5Repetitions"] = "1";

            settings["TTL6StartTimes"] = "10000";
            settings["TTL6Durations"] = "5000";
            settings["TTL6Repetitions"] = "1";
        }

        protected override void DoAcquisitionStarting()
        {
            scanPatternBuilder = new NshotsPatternBuilderCCD();
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

        protected int CCDEnableTimePGUnits
        {
            get
            {
                long gateStartShotUnits = (int)config.shotGathererPlugin.Settings["ccdEnableStartTime"];
                long shotGathererSampleRate = (int)config.shotGathererPlugin.Settings["sampleRate"]; //We should use the sample rate to determine the gate start time
                long clockFreq = (int)Settings["clockFrequency"];
                return (int)(
                    (double)(gateStartShotUnits * clockFreq) / shotGathererSampleRate //This used to be divided by 1000000.0
                    );
            }
        }

        protected override IPatternSource GetScanPattern()
        {
            int[] ttl1StartTimes = convertStringListToIntArray((string)settings["TTL1StartTimes"]);
            int[] ttl1Durations = convertStringListToIntArray((string)settings["TTL1Durations"]);
            int[] ttl1Repetitions = convertStringListToIntArray((string)settings["TTL1Repetitions"]);

            int[] ttl2StartTimes = convertStringListToIntArray((string)settings["TTL2StartTimes"]);
            int[] ttl2Durations = convertStringListToIntArray((string)settings["TTL2Durations"]);
            int[] ttl2Repetitions = convertStringListToIntArray((string)settings["TTL2Repetitions"]);

            int[] ttl3StartTimes = convertStringListToIntArray((string)settings["TTL3StartTimes"]);
            int[] ttl3Durations = convertStringListToIntArray((string)settings["TTL3Durations"]);
            int[] ttl3Repetitions = convertStringListToIntArray((string)settings["TTL3Repetitions"]);

            int[] ttl4StartTimes = convertStringListToIntArray((string)settings["TTL4StartTimes"]);
            int[] ttl4Durations = convertStringListToIntArray((string)settings["TTL4Durations"]);
            int[] ttl4Repetitions = convertStringListToIntArray((string)settings["TTL4Repetitions"]);

            int[] ttl5StartTimes = convertStringListToIntArray((string)settings["TTL5StartTimes"]);
            int[] ttl5Durations = convertStringListToIntArray((string)settings["TTL5Durations"]);
            int[] ttl5Repetitions = convertStringListToIntArray((string)settings["TTL5Repetitions"]);

            int[] ttl6StartTimes = convertStringListToIntArray((string)settings["TTL6StartTimes"]);
            int[] ttl6Durations = convertStringListToIntArray((string)settings["TTL6Durations"]);
            int[] ttl6Repetitions = convertStringListToIntArray((string)settings["TTL6Repetitions"]);

            scanPatternBuilder.Clear();
            scanPatternBuilder.ShotSequence(
                (int)settings["padStart"],
                (int)settings["sequenceLength"],
                (int)settings["flashlampPulseInterval"],
                (int)settings["flashToQ"],
                (int)settings["flashlampPulseLength"],
                (int)settings["ccd1Start1"],
                (int)settings["ccd1Start2"],
                (int)settings["ccd2Start1"],
                (int)settings["ccd2Start2"],
                ttl1StartTimes, ttl1Durations, ttl1Repetitions,
                ttl2StartTimes, ttl2Durations, ttl2Repetitions,
                ttl3StartTimes, ttl3Durations, ttl3Repetitions,
                ttl4StartTimes, ttl4Durations, ttl4Repetitions,
                ttl5StartTimes, ttl5Durations, ttl5Repetitions,
                ttl6StartTimes, ttl6Durations, ttl6Repetitions,
                GateStartTimePGUnits,
                CCDEnableTimePGUnits,
                (int)settings["ttlSwitchPort"],
                (int)settings["ttlSwitchLine"],
                (int)settings["switchLineDuration"],
                (int)settings["switchLineDelay"],
                (bool)config.switchPlugin.Settings["switchActive"]
                );


            // In SupersonicPGPluginBase => patternLength = (int)settings["flashlampPulseInterval"] * (int)settings["sequenceLength"]*((int)settings["padShots"] + 1);
            // so this length has to be the same here! This is why some other plugins do unexpected things (the length is twice as long and only half the sequence is actually running)
            // In this plugin, padShots doesnt appear (equals to 0).
            scanPatternBuilder.BuildPattern((int)settings["sequenceLength"] * (int)settings["flashlampPulseInterval"]);

            return scanPatternBuilder;
        }
    }
}
