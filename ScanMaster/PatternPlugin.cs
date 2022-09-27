using System;
using System.Xml.Serialization;

using ScanMaster.Acquire.Plugins;

namespace ScanMaster.Acquire.Plugin
{
	/// <summary>
	/// A plugin representing a pattern generation task. This plugin adds 
	/// little to the IAcquisitor lifecycle.
	/// </summary>
	[Serializable]
	[XmlInclude(typeof(DecelerationPatternPlugin)),
	 XmlInclude(typeof(NullPGPlugin)),
     XmlInclude(typeof(GuidePatternPlugin)),
	 XmlInclude(typeof(CommonRamanPatternPlugin)),
	 XmlInclude(typeof(PulsedRFScanPatternPlugin)),
     XmlInclude(typeof(DualCCDPatternPlugin)),
	 XmlInclude(typeof(LeakTestPatternPlugin)),
	 XmlInclude(typeof(LeakTestPatternPluginModified)),
	 XmlInclude(typeof(LeakTestWithDyePatternPlugin)),
	 XmlInclude(typeof(SuperPumpingPulsedRFScanPatternPlugin)),
     XmlInclude(typeof(SuperPumpingPatternPlugin)),
	 XmlInclude(typeof(PumpProbePatternPlugin)),
     XmlInclude(typeof(DualAblationPatternPlugin)),
     XmlInclude(typeof(DualValvePatternPlugin)),
     XmlInclude(typeof(BasicBeamPatternPlugin)),
     XmlInclude(typeof(AomModulatedPatternPlugin)),
     XmlInclude(typeof(AomLevelControlPatternPlugin)),
     XmlInclude(typeof(ImagingPatternPlugin)),
     XmlInclude(typeof(MOTPatternPlugin)),
     XmlInclude(typeof(FlashlampsOnlyPatternPlugin)),
     XmlInclude(typeof(ZeemanSisyphusPatternPlugin)),
	 XmlInclude(typeof(NshotsPatternPlugin)),
	 XmlInclude(typeof(TwoShutterPatternPlugin)),
	 XmlInclude(typeof(FourShutterPatternPlugin)),
	 XmlInclude(typeof(FourShutterPatternPluginEdit)),
	 XmlInclude(typeof(FindV2PatternPlugin)),
	 XmlInclude(typeof(FindV3PatternPlugin)),
	 XmlInclude(typeof(Find4fPatternPlugin)),
	 XmlInclude(typeof(TenHzTwoHzPatternPlugin)),
	 XmlInclude(typeof(VelocityMeasSlowedPatternPlugin)),
	 XmlInclude(typeof(FourShutterPatternPluginFindV1)),]
     //XmlInclude(typeof(MMPatternPlugin))]
	public abstract class PatternPlugin : AcquisitorPlugin
	{

		protected override void InitialiseBaseSettings()
		{
			settings["stopFlashlamps"] = true;
			settings["test"] = 100;
		}
	
		// calling this reloads the pattern with the current settings. Use if you've changed those settings.
		public abstract void ReloadPattern();
		
	}
}
