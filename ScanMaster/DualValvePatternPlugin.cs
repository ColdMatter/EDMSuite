using System;

using System.Xml.Serialization;

using DAQ.HAL;

using DAQ.Environment;
using DAQ.Pattern;

using ScanMaster.Acquire.Patterns;
using ScanMaster.Acquire.Plugin;

namespace ScanMaster.Acquire.Plugins
{
    /// <summary>
    /// A plugin for experiments with two valves.
    /// </summary>
    [Serializable]
    public class DualValvePatternPlugin : ScanMaster.Acquire.Plugin.PatternPlugin
    {
        [NonSerialized]
        private int patternLength;
        [NonSerialized]
        private DAQMxPatternGenerator pg;
        [NonSerialized]
        private DualValvePatternBuilder scanPatternBuilder;

        protected override void InitialiseSettings()
        {
            settings["dischargeLength"] = 20;
            settings["dischargeToValve1"] = 150;
            settings["valve1ToValve2"] = 10;
            settings["valve1PulseLength"] = 1000;
            settings["valve2PulseLength"] = 1000;
            settings["sequenceLength"] = 2;
            settings["sequenceInterval"] = 100000;
            settings["clockFrequency"] = 1000000;
            settings["internalClock"] = true;
            settings["padShots"] = 0;
            settings["padStart"] = 0;
            settings["fullWidth"] = true;
            settings["lowGroup"] = true;
        }
        private bool firstScan;
        public override void AcquisitionStarting()
        {
            scanPatternBuilder = new DualValvePatternBuilder();

            // get hold of the pattern generator
            pg = new DAQMxPatternGenerator((string)Environs.Hardware.Boards["pg"]);

            // configure the pattern generator
            patternLength = (int)settings["sequenceInterval"] * (int)settings["sequenceLength"]
                * ((int)settings["padShots"] + 1);

            pg.Configure(
                (int)settings["clockFrequency"],
                true,
                (bool)settings["fullWidth"],
                (bool)settings["lowGroup"],
                patternLength,
                (bool)settings["internalClock"],
                false
                );
            firstScan = true;
        }

        protected IPatternSource GetScanPattern()
        {
            scanPatternBuilder.Clear();
            scanPatternBuilder.ShotSequence(
                (int)settings["padStart"],
                (int)settings["sequenceLength"],
                (int)settings["sequenceInterval"],
                (int)settings["dischargeLength"],
                (int)settings["valve1PulseLength"],
                (int)settings["valve2PulseLength"],
                (int)settings["dischargeToValve1"],
                (int)settings["valve1ToValve2"],
                GateStartTimePGUnits
                );

            scanPatternBuilder.BuildPattern( (int)settings["sequenceLength"]
                * (int)settings["sequenceInterval"]);

            return scanPatternBuilder;
        }

        public override void ScanStarting()
        {
            if(firstScan)
                OutputPattern(GetScanPattern());
            firstScan = false;
        }

        public override void ScanFinished()
        {

        }

        public override void AcquisitionFinished()
        {
            pg.StopPattern();           
        }

        public override void ReloadPattern()
        {
            pg.StopPattern();
            pg.Configure(
                (int)settings["clockFrequency"],
                true,
                (bool)settings["fullWidth"],
                (bool)settings["lowGroup"],
                patternLength,
                (bool)settings["internalClock"],
                false
                );
            OutputPattern(GetScanPattern());
            System.GC.Collect();
        }

        private void OutputPattern(IPatternSource pattern)
        {
            if ((bool)settings["fullWidth"])
            {
                pg.OutputPattern(pattern.Pattern);
            }
            else
            {
                if ((bool)settings["lowGroup"])
                {
                    pg.OutputPattern(pattern.LowHalfPatternAsInt16);
                }
                else
                {
                    pg.OutputPattern(pattern.HighHalfPatternAsInt16);
                }
            }
        }

        protected int GateStartTimePGUnits
        {
            get
            {
                long gateStartShotUnits = (int)config.shotGathererPlugin.Settings["gateStartTime"];
                long clockFreq = (int)Settings["clockFrequency"];
                return (int)(
                    (double)(gateStartShotUnits * clockFreq) / 1000000.0
                    );
            }
        }
    }
}
