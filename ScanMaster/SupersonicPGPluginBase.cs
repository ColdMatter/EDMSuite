using System;

using DAQ.Environment;
using DAQ.HAL;
using DAQ.Pattern;

using ScanMaster.Acquire.Patterns;

namespace ScanMaster.Acquire.Plugins
{
	/// <summary>
	/// Most of our pattern generator plugins are pretty much the same. This is a common base
	/// class for them. It allows the plugins to provide their own pattern that will be used
	/// during the scan, their own settings and gives them a chance to do some processing
	/// before acquisition begins.
	/// </summary>
	[Serializable]
	public abstract class SupersonicPGPluginBase : ScanMaster.Acquire.Plugin.PatternPlugin
	{

		[NonSerialized]
		private FlashlampPatternBuilder flashlampPatternBuilder;
		[NonSerialized]
		private int patternLength;
		[NonSerialized]
		private DAQMxPatternGenerator pg;

		protected override void InitialiseSettings()
		{
			settings["flashlampPulseInterval"] = 100000;
			settings["valveToQ"] = 570;
			settings["flashToQ"] = 160;
			settings["valvePulseLength"] = 350;
			settings["sequenceLength"] = 1;
			settings["clockFrequency"] = 1000000;
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
	
			// build a flashlamp pattern
			flashlampPatternBuilder = new FlashlampPatternBuilder();

			// configure the pattern generator
			patternLength = (int)settings["flashlampPulseInterval"] * (int)settings["sequenceLength"]
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
			
						
			 loadFlashlampPattern();
		}

		public override void ScanStarting()
		{
			OutputPattern(GetScanPattern());
		}

		public override void ScanFinished()
		{
			// switch back to the flashlamp only pattern
			loadFlashlampPattern();
		}

		public override void AcquisitionFinished()
		{
			// check whether to stop flashlamp pattern
			if ((bool)settings["stopFlashlamps"]) 
			{
				pg.StopPattern();
			}
			else
			{
				// reload the flashlamp pattern
				loadFlashlampPattern();
			}
		}

		public override void ReloadPattern()
		{
			OutputPattern(GetScanPattern());
			System.GC.Collect();
		}

		private void loadFlashlampPattern()
		{
			flashlampPatternBuilder.Clear();
			flashlampPatternBuilder.ShotSequence(
				(int)settings["padStart"],
				((int)settings["padShots"] + 1) * (int)settings["sequenceLength"],
				(int)settings["flashlampPulseInterval"],
				(int)settings["valveToQ"],
				(int)settings["flashToQ"]
				);
			flashlampPatternBuilder.BuildPattern( ((int)settings["padShots"] + 1) 
				* (int)settings["sequenceLength"] * (int)settings["flashlampPulseInterval"] );

			OutputPattern(flashlampPatternBuilder);
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
