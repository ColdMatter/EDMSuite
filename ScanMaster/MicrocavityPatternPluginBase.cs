using System;

using DAQ.Environment;
using DAQ.HAL;
using DAQ.Pattern;

using ScanMaster.Acquire.Patterns;

namespace ScanMaster.Acquire.Plugins
{
	/// <summary>
	/// This is a common base class for microcavity experiments. 
    /// It allows the experiment plugins to provide their own pattern that will be used
	/// during the scan, their own settings and gives them a chance to do some processing
	/// before acquisition begins.
	/// </summary>
	[Serializable]
	public abstract class MicrocavityPatternPluginBase : ScanMaster.Acquire.Plugin.PatternPlugin
	{
		[NonSerialized]
		private int patternLength;
		[NonSerialized]
		private DAQMxPatternGenerator pg;

		protected override void InitialiseSettings()
		{
			settings["sequenceLength"] = 1;
			settings["clockFrequency"] = 500000;
            settings["internalClock"] = true;
			settings["padShots"] = 0;
            settings["padStart"] = 0;
			settings["fullWidth"] = true;
			settings["lowGroup"] = true;
            settings["triggered"] = false;

			InitialiseCustomSettings();
		}

		protected abstract IPatternSource GetScanPattern();

		protected abstract void InitialiseCustomSettings();

		protected abstract void DoAcquisitionStarting();

		public override void AcquisitionStarting()
		{
			// any plugin specific initialisation goes here
			DoAcquisitionStarting();

			// get hold of the pattern generator
            
            pg = new DAQMxPatternGenerator((string)Environs.Hardware.GetInfo("PatternGeneratorBoard"));
	
			// configure the pattern generator
			patternLength = (int)settings["sequenceLength"]
				* ((int)settings["padShots"] + 1);

 			pg.Configure(
				(int)settings["clockFrequency"],
				true,
				(bool)settings["fullWidth"],
				(bool)settings["lowGroup"],
				patternLength,
                (bool)settings["internalClock"],
                (bool)settings["triggered"]
				);
		}

		public override void ScanStarting()
		{
			OutputPattern(GetScanPattern());
		}

		public override void ScanFinished()
		{
            
		}

		public override void AcquisitionFinished()
		{
            pg.StopPatternTaskOnly();
		}

		public override void ReloadPattern()
		{
			OutputPattern(GetScanPattern());
			System.GC.Collect();
		}

		private void OutputPattern(IPatternSource pattern)
		{
			if ( (bool)settings["fullWidth"] )
			{
				pg.OutputPattern(pattern.Pattern);
			}
			else
			{
				if ( (bool)settings["lowGroup"] )
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
