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
	 XmlInclude(typeof(PumpProbePatternPlugin)),
     XmlInclude(typeof(DualAblationPatternPlugin)),
     XmlInclude(typeof(DualValvePatternPlugin)),
     XmlInclude(typeof(BasicBeamPatternPlugin))]
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
