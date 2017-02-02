using System;

using System.Xml.Serialization;

using DAQ.Environment;
using DAQ.Pattern;

using ScanMaster.Acquire.Patterns;
using ScanMaster.Acquire.Plugin;

namespace ScanMaster.Acquire.Plugins
{
    /// <summary>
    /// A plugin for making a MOT!!
    /// 
    /// When switchActive is true, the Q-switch is on for one shot and off the next
    /// 
    /// Note carefully: the sequenceLength setting must be a multiple of 2 for the pattern to have the correct
    /// behaviour (because sequenceLength is the number of flashlamp pulse intervals in the sequence, and this 
    /// pattern requires a minimum of 2 shots per sequence).
    /// </summary>
    [Serializable]
    public class MOTPatternPlugin : SupersonicPGPluginBase
    {
        [NonSerialized]
        private MOTPatternBuilder scanPatternBuilder;

        protected override void InitialiseCustomSettings()
        {
            settings["slowingAOMOnStart"] = 2500;
            settings["slowingAOMOnDuration"] = 460000;
            settings["slowingAOMOffStart"] = 15000;
            settings["slowingRepumpAOMOffStart"] = 15000;
            settings["slowingAOMOffDuration"] = 60000;
            settings["motAOMStart"] = 30000;
            settings["motAOMDuration"] = 1000;
            settings["motRampStart"] = 1000;
            settings["bTrigger"] = 1000;
            settings["bDuration"] = 100000;
            settings["cameraTrigger"] = 20000;
            settings["padStart"] = 0;
            settings["flashlampPulseLength"] = 100;
            settings["chirpStart"] = 100;
            settings["chirpDuration"] = 300;
            settings["sequenceLength"] = 2;
        }

        protected override void DoAcquisitionStarting()
        {
            scanPatternBuilder = new MOTPatternBuilder();
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
                (int)settings["slowingAOMOnStart"],
                (int)settings["slowingAOMOffStart"] - (int)settings["slowingAOMOnStart"],
                (int)settings["slowingAOMOffStart"] + (int)settings["slowingAOMOffDuration"],
                (int)settings["slowingAOMOnDuration"] - ((int)settings["slowingAOMOffStart"]
                - (int)settings["slowingAOMOnStart"]) - (int)settings["slowingAOMOffDuration"],
                (int)settings["slowingRepumpAOMOffStart"] - (int)settings["slowingAOMOnStart"],
                (int)settings["motAOMStart"],
                (int)settings["motAOMDuration"],
                (int)settings["motRampStart"],
                (int)settings["motAOMReStart"],
                (int)settings["bTrigger"],
                (int)settings["bDuration"],
                (int)settings["cameraTrigger"],
                GateStartTimePGUnits,
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

