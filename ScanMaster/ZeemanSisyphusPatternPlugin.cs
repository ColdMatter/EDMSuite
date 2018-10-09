
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
    public class ZeemanSisyphusPatternPlugin : SupersonicPGPluginBase
    {
        [NonSerialized]
        private ZeemanSisyphusPatternBuilder scanPatternBuilder;

        protected override void InitialiseCustomSettings()
        {
            
            settings["pmtTrigger"] = 20000;
            settings["padStart"] = 0;
            settings["flashlampPulseLength"] = 100;
            settings["flashlampPulseInterval"] = 500000;
            settings["sequenceLength"] = 2;
        }

        protected override void DoAcquisitionStarting()
        {
            scanPatternBuilder = new ZeemanSisyphusPatternBuilder();
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
                (int)settings["flashlampPulseLength"],
                (int)settings["pmtTrigger"],
                (bool)config.switchPlugin.Settings["switchActive"]
                );

            scanPatternBuilder.BuildPattern(2 * ((int)settings["padShots"] + 1) * (int)settings["sequenceLength"]
                * (int)settings["flashlampPulseInterval"]);

            return scanPatternBuilder;
        }
    }
}

