using System;

using System.Xml.Serialization;

using DAQ.Environment;
using DAQ.Pattern;

using ScanMaster.Acquire.Patterns;
using ScanMaster.Acquire.Plugin;

namespace ScanMaster.Acquire.Plugins
{
	/// <summary>
	/// A plugin for running the flashlamps only during pattern generation
    /// Flashlamp pattern is already generated in SupersonicPGPluginBase
    /// </summary>
	[Serializable]
	public class FlashlampsOnlyPatternPlugin : SupersonicPGPluginBase
    {
        [NonSerialized]
        private FlashlampPatternBuilder scanPatternBuilder;

		protected override void InitialiseCustomSettings()
		{
            settings["dummy"] = "chris";
		}

		protected override void DoAcquisitionStarting()
        {
            scanPatternBuilder = new FlashlampPatternBuilder();
		}

		protected override IPatternSource GetScanPattern()
        {
            scanPatternBuilder.Clear();
            scanPatternBuilder.ShotSequence(
                (int)settings["padStart"],
                (int)settings["sequenceLength"],
                (int)settings["flashlampPulseInterval"],
                (int)settings["valveToQ"],
                (int)settings["flashToQ"]
                );

            scanPatternBuilder.BuildPattern(((int)settings["padShots"] + 1) * (int)settings["sequenceLength"]
                * (int)settings["flashlampPulseInterval"]);

            return scanPatternBuilder;
		}
	}		
}
