using System;
using System.Xml.Serialization;

using DAQ.Environment;
using DAQ.Pattern;

using ScanMaster.Acquire.Patterns;
using ScanMaster.Acquire.Plugin;


namespace ScanMaster.Acquire.Plugins
{
	/// <summary>
	/// Pattern plugin for Microcavity experiment. 
	/// This pattern does not support switch scanning (yet).
	/// </summary>
	[Serializable]
	public class MicrocavityPatternPlugin : MicrocavityPatternPluginBase
	{

		[NonSerialized]
		private MicrocavityPatternBuilder scanPatternBuilder;

		protected override void InitialiseCustomSettings()
		{
            settings["sequenceLength"] = 2;
            settings["padStart"] = 0;
		}

		protected override void DoAcquisitionStarting()
		{
			scanPatternBuilder = new MicrocavityPatternBuilder();
		}

		protected override IPatternSource GetScanPattern()
		{
			// switch over to the scan pattern
			scanPatternBuilder = new MicrocavityPatternBuilder();
			// this is a bit of a hack. I think that the time ordering code in the pattern builder
			// should be improved.
			scanPatternBuilder.EnforceTimeOrdering = false;
			scanPatternBuilder.Clear();
            scanPatternBuilder.ShotSequence(
                (int)settings["padStart"], 
                (int)settings["sequenceLength"], 
                GateStartTimePGUnits);
			scanPatternBuilder.BuildPattern(((int)settings["padShots"] + 1) * (int)settings["sequenceLength"]);
			return scanPatternBuilder;
		}

	}
}
