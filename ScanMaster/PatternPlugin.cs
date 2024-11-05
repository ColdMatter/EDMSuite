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
	 XmlInclude(typeof(BasicBeamTwoYAGPatternPlugin)),
	 XmlInclude(typeof(AomModulatedPatternPlugin)),
     XmlInclude(typeof(AomLevelControlPatternPlugin)),
     XmlInclude(typeof(ImagingPatternPlugin)),
     XmlInclude(typeof(MOTPatternPlugin)),
     XmlInclude(typeof(FlashlampsOnlyPatternPlugin)),
     XmlInclude(typeof(ZeemanSisyphusPatternPlugin)),
	 XmlInclude(typeof(NshotsPatternPlugin)),
	 XmlInclude(typeof(YAGFirePatternPlugin)),
	 XmlInclude(typeof(TwoShutterPatternPlugin)),
	 XmlInclude(typeof(TwoShutterSlowingPatternPlugin)),
	 XmlInclude(typeof(TwoShutterTwoYAGPatternPlugin)),
	 XmlInclude(typeof(FourShutterPatternPlugin)),
	 XmlInclude(typeof(FourShutterPatternPluginEdit)),
	 XmlInclude(typeof(FindV2PatternPlugin)),
	 XmlInclude(typeof(FindV3PatternPlugin)),
	 XmlInclude(typeof(Find4fPatternPlugin)),
	 XmlInclude(typeof(Find4fNewPatternPlugin)),
	 XmlInclude(typeof(TenHzTwoHzPatternPlugin)),
	 XmlInclude(typeof(VelocityMeasSlowedPatternPlugin)),
     XmlInclude(typeof(CaFBECPatternPlugin)),
	 XmlInclude(typeof(STIRAPpatternPlugin)),
	 XmlInclude(typeof(FourShutterPatternPluginFindV1)),
     XmlInclude(typeof(LatticePatternPlugin)),
     XmlInclude(typeof(LatticePumpProbePatternPlugin)),
	 XmlInclude(typeof(LatticePnPModYAGPatternPlugin)),
	 XmlInclude(typeof(LatticePnPModYAGFourShotsPatternPlugin))]
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
